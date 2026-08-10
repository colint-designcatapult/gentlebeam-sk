#include "main.h"
#include "stdbool.h"
#include "math.h"
#include "FreeRTOS.h"
#include "task.h"

#include "monitoring.h"
#include "adc.h"
#include "control_comm.h"
#include "ext_adcs.h"
#include "ext_dacs.h"
#include "io.h"
#include "timers.h"
#include "setup.h"
#include "faults.h"

#define CONTROL_TASK_STACK_WORDS		256U
#define CONTROL_TASK_PRIORITY			32U
#define CONTROL_PERIOD_S				0.005f
#define CONTROL_ADC_WAIT_TICKS			pdMS_TO_TICKS(10U)

#define ADS8325_REFERENCE_VOLTS			4.096f
#define ADS8325_CODE_RANGE				65536.0f
#define ADS8325_COUNTS_PER_VOLT			(ADS8325_CODE_RANGE / ADS8325_REFERENCE_VOLTS)

#define KV_FB_SERIES_RESISTANCE_OHMS	20000.0f
#define KV_FB_SHUNT_RESISTANCE_OHMS		10000.0f
#define KV_FB_VOLTS_PER_KV				0.1f
#define KV_FB_ADC_DIVIDER_RATIO			(KV_FB_SHUNT_RESISTANCE_OHMS / \
										(KV_FB_SERIES_RESISTANCE_OHMS + KV_FB_SHUNT_RESISTANCE_OHMS))
#define KV_FB_COUNTS_PER_KV				(ADS8325_COUNTS_PER_VOLT * \
										KV_FB_ADC_DIVIDER_RATIO * KV_FB_VOLTS_PER_KV)

#define MA_FB_SERIES_RESISTANCE_OHMS	30000.0f
#define MA_FB_SHUNT_RESISTANCE_OHMS		20000.0f
#define MA_FB_VOLTS_PER_MA				1.0f
#define MA_FB_ADC_DIVIDER_RATIO			(MA_FB_SHUNT_RESISTANCE_OHMS / \
										(MA_FB_SERIES_RESISTANCE_OHMS + MA_FB_SHUNT_RESISTANCE_OHMS))
#define MA_FB_COUNTS_PER_MA				(ADS8325_COUNTS_PER_VOLT * \
										MA_FB_ADC_DIVIDER_RATIO * MA_FB_VOLTS_PER_MA)

uint32_t hvps_fault_mask = 0;

uint32_t sys_stat = 0;
uint32_t sys_io_bits = 0;
#ifdef CALIBRATION_MODE
uint32_t sys_fault_bits = 0;
#endif

float fb_vals[NUM_FB];

float config_vals[NUM_SYS_CONFIG];
float setpoints[NUM_SP];
float kv_out = 0;
float fil_out = 0;
float grid_out = 0;

KV_Status kv_stat = KV_STAY;
FIL_Status fil_stat = FIL_STAY;

uint32_t kv_stability_count = 0;
uint32_t fil_stability_count = 0;
uint32_t grid_ctrl_count = 0;
uint32_t fil_fb_count = 0;
#ifdef CALIBRATION_MODE
uint32_t kv_fb_count = 0;
#endif


float fil_adj = 0;
static bool grid_control_active;
static float grid_integral;
static float grid_previous_kp;
static float grid_feedforward;
static StaticTask_t control_task_buffer;
static StackType_t control_task_stack[CONTROL_TASK_STACK_WORDS];

static float update_ma_target(void);
static bool process_kv_ramp(float *command);
static void process_fil_ramp(void);
static void update_kv_ramp(void);
static void get_fil_ramp(void);
static bool run_grid_ctrl(float *command);
static float get_grid_kp(float kv);
static float clamp_float(float value, float minimum, float maximum);

/* Main control task. */
static void control_task(void *argument);

void setup_system_monitoring()
{
#ifndef CALIBRATION_MODE
    config_vals[SYS_CONFIG_MAX_PWR] = 300;
    config_vals[SYS_CONFIG_MIN_GRID] = 50;

    config_vals[SYS_CONFIG_RUN_PID] = 1;
#else
    config_vals[SYS_CONFIG_MAX_PWR] = 400;
    config_vals[SYS_CONFIG_MIN_GRID] = 100;

    config_vals[SYS_CONFIG_RUN_PID] = 0;
#endif

	config_vals[SYS_CONFIG_MAX_GRID] = 600;

	config_vals[SYS_CONFIG_MIN_KV] = 2;
	config_vals[SYS_CONFIG_MAX_KV] = 100;
	config_vals[SYS_CONFIG_FIL_INIT] = 1000;
	config_vals[SYS_CONFIG_MAX_MA] = 4.5;
	config_vals[SYS_CONFIG_FIL_LOW] = 2400;
	config_vals[SYS_CONFIG_FIL_LIM] = 3250;

	config_vals[SYS_CONFIG_KV_BOUND] = 0.1;
	config_vals[SYS_CONFIG_KV_RAMP_FAST] = 5;
	config_vals[SYS_CONFIG_KV_RAMP_SLOW] = 0.5;

	config_vals[SYS_CONFIG_GRID_KP_50] = -25.0f;
	config_vals[SYS_CONFIG_GRID_KP_70] = -35.0f;
	config_vals[SYS_CONFIG_GRID_KP_100] = -150.0f;
	config_vals[SYS_CONFIG_GRID_INTEGRAL_TIME] = 0.2f;
	config_vals[SYS_CONFIG_GRID_SLEW_DOWN] = 10000.0f;
	config_vals[SYS_CONFIG_GRID_SLEW_UP] = 3000.0f;

	//Initialize set points
	setpoints[SP_PWR] = 0;
	setpoints[SP_KV] = 0;
	setpoints[SP_GRID] = 0;
	setpoints[SP_FIL] = 0;
	setpoints[SP_MA_LIM] = 0;

	grid_control_active = false;
	grid_integral = 0.0f;
	grid_previous_kp = config_vals[SYS_CONFIG_GRID_KP_50];
	grid_feedforward = config_vals[SYS_CONFIG_MAX_GRID];
	grid_out = grid_feedforward;
	write_grid(grid_out);

	hvps_fault_mask =
			(1 << IN_FIL_CLK_FAULT) | (1 << IN_CAT_ARC) 	| (1 << IN_FAN_FAULT) 	| (1 << IN_OC_24_FAULT) | (1 << IN_MASTER_FAULT) |
			(1 << IN_OC_HV_FAULT) 	| (1 << IN_TEMP_1_FAULT)| (1 << IN_OC_CAT_FAULT)| (1 << IN_TEMP_3_FAULT)| (1 << IN_TEMP_2_FAULT);

	TaskHandle_t control_task_handle = xTaskCreateStatic(
		control_task,
		"control",
		CONTROL_TASK_STACK_WORDS,
		NULL,
		CONTROL_TASK_PRIORITY,
		control_task_stack,
		&control_task_buffer);

	configASSERT(control_task_handle != NULL);

	ext_adcs_set_result_task(control_task_handle);
}

static void control_task(void *argument)
{
	(void)argument;

	for (;;)
	{
		ext_adc_result_t adc_result;
		bool kv_command_pending = false;
		bool grid_command_pending = false;
		float kv_command = 0.0f;
		float grid_command = 0.0f;

		if (ulTaskNotifyTake(pdTRUE, CONTROL_ADC_WAIT_TICKS) == 0U)
		{
			continue;
		}
		bool adc_read_ok = ext_adcs_get_latest_result(&adc_result)
				&& (adc_result.status == EXT_ADC_RESULT_VALID);

		taskENTER_CRITICAL();
		if (adc_read_ok)
		{
			report_kv_fb(adc_result.kv_average);
			report_ma_fb(adc_result.ma_average);
		}

#ifndef CALIBRATION_MODE
		bool kv_ramp_allowed = !sys_stat_check(SYS_EMISSION_ON);
#else
		bool kv_ramp_allowed = true;
#endif
		if (kv_ramp_allowed && sys_stat_check(SYS_KV_RAMPING)
				&& (kv_ramp_ms <= 0))
		{
			kv_ramp_ms = 1000;
			kv_command_pending = process_kv_ramp(&kv_command);
		}
		grid_command_pending = run_grid_ctrl(&grid_command);
		taskEXIT_CRITICAL();

		if (kv_command_pending)
		{
			write_kv(kv_command);
		}
		if (grid_command_pending)
		{
			write_grid(grid_command);
		}

	}
}

/* ========================================================================
 * System Status Bit Helpers
 *
 * Check, set, and clear status bits in sys_stat.
 *
 * ======================================================================== */
bool sys_stat_check(uint8_t bitpos)
{
	if(bitpos >= NUM_SYS_BITS)
	{
		return false;
	}
	if((1<<bitpos) & sys_stat)
	{
		return true;
	}
	return false;
}

void set_sys_bit(uint8_t bitpos)
{
	if (bitpos < NUM_SYS_BITS)
	{
		taskENTER_CRITICAL();
		sys_stat |= (1UL << bitpos);
		taskEXIT_CRITICAL();
	}
}


void clear_sys_bit(uint8_t bitpos)
{
	if (bitpos < NUM_SYS_BITS)
	{
		taskENTER_CRITICAL();
		sys_stat &= ~(1UL << bitpos);
		taskEXIT_CRITICAL();
	}
}
 

/* ========================================================================
 * System Configuration Access Functions
 *
 * Get or update values in the config_vals table.
 *
 * ======================================================================== */
bool sys_config_set(unsigned int index, float value)
{
    if (index >= NUM_SYS_CONFIG)
    {
        return false;
    }

    config_vals[index] = value;
    return true;
}

float sys_config_get(unsigned int index)
{
    return config_vals[index];
}

/* ========================================================================
 * IO Edge-Triggered Interlock Handlers
 *
 * Called from report_io_state() in <original_file>.c, which computes the
 * rising ('rose') and falling ('fell') edge bitmasks once per call and
 * dispatches them to the handlers below — one per monitored IO signal
 * (HV interlock, grid interlock, master fault, beam control).
 *
 * ======================================================================== */
#define IO_BIT(n)   (1u << (n))

static void handle_hv_int(uint32_t rose, uint32_t fell)
{
    if (fell & IO_BIT(IN_HV_INT))
    {
        lock_hv();
        clear_sys_bit(SYS_EMISSION_ON);
    }
#ifdef CALIBRATION_MODE
    else if (rose & IO_BIT(IN_HV_INT))
    {
        set_sys_bit(SYS_HV_CTRL_EN);
        HAL_GPIO_WritePin(GPIOE, IO_PFC_ALLOWED_Pin | IO_HV_ALLOWED_Pin, GPIO_PIN_SET);
        HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_SET);
    }
#endif
}

static void handle_grid_int(uint32_t rose, uint32_t fell)
{
    if (fell & IO_BIT(IN_GRID_INT))
    {
        lock_grid();
        clear_sys_bit(SYS_EMISSION_ON);
    }
#ifdef CALIBRATION_MODE
    else if (rose & IO_BIT(IN_GRID_INT))
    {
        set_sys_bit(SYS_GRID_CTRL_EN);
    }
#endif
}

static void handle_master_fault(uint32_t rose, uint32_t fell)
{
    if (rose & IO_BIT(IN_MASTER_FAULT))
    {
        lock_hv();
        lock_grid();
        HAL_GPIO_WritePin(GPIOB, IO_PS_OK_Pin, GPIO_PIN_RESET);
    }
    else if (fell & IO_BIT(IN_MASTER_FAULT))
    {
        HAL_GPIO_WritePin(GPIOB, IO_PS_OK_Pin, GPIO_PIN_SET);
    }
}

static void handle_beam_ctrl(uint32_t rose, uint32_t fell)
{
    if (rose & IO_BIT(IN_BEAM_CTRL))
    {
        if (sys_stat_check(SYS_HV_CTRL_EN) && sys_stat_check(SYS_GRID_CTRL_EN))
        {
            prev_error = 0;
            acc_error = 0;
#ifdef CALIBRATION_MODE
            set_sys_bit(SYS_CAL_GRID_INT_EN);
#else
            set_sys_bit(SYS_EMISSION_ON);
            HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_SET);
#endif
        }
        //TBD TODO else throw fault
    }
    else if (fell & IO_BIT(IN_BEAM_CTRL))
    {
#ifdef CALIBRATION_MODE
        clear_sys_bit(SYS_CAL_GRID_INT_EN);
#else
        clear_sys_bit(SYS_EMISSION_ON);
        HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);
#endif
    }
}

/* ========================================================================
 * report_io_state()
 *
 * Entry point for processing a new IO state snapshot. Computes rising/
 * falling edges relative to the previous sys_io_bits, then dispatches
 * to the per-signal handlers in interlock_handlers.c:
 *   - handle_hv_int()
 *   - handle_grid_int()
 *   - handle_master_fault()
 *   - handle_beam_ctrl()
 * ======================================================================== */
void report_io_state(uint32_t io_bits)
{
	static bool first_call = true;

    if (first_call)
    {
        first_call = false;
        sys_io_bits = io_bits;   // establish baseline, no edges to compute yet

        // const uint32_t boot_faults = io_bits & hvps_fault_mask;
        // if (boot_faults != 0)
        // {
        //     report_fault(boot_faults);
        // }
        return;
    }

    const uint32_t rose = (sys_io_bits ^ io_bits) & io_bits;
    const uint32_t fell = (sys_io_bits ^ io_bits) & sys_io_bits;

    sys_io_bits = io_bits;

	const uint32_t new_faults = rose & hvps_fault_mask;
    if (new_faults != 0)
    {
        report_fault(new_faults);
    }

    handle_hv_int(rose, fell);
    handle_grid_int(rose, fell);
    handle_master_fault(rose, fell);
    handle_beam_ctrl(rose, fell);
}

void report_int_adc_vals(uint16_t *vals)
{
#ifndef CALIBRATION_MODE
	float fil_scale = 0.9102222;
#else
    float fil_scale = 0.910222222222222;
#endif
	float grid_scale = 5.7;

	//TBD TODO NOTE: if filament current is > 4 A kill filament DAC directly, update setpoint and throw fault
	fb_vals[FB_FIL_A] = (float)(vals[INT_ADC_FIL_A])/fil_scale;
	fb_vals[FB_GRID] = (float)(vals[INT_ADC_GRID])/grid_scale;
}

void report_kv_fb(uint32_t kv_fb)
{
	fb_vals[FB_KV] = (float)kv_fb / KV_FB_COUNTS_PER_KV;
}

void report_ma_fb(uint32_t ma_fb)
{
	fb_vals[FB_MA] = (float)ma_fb / MA_FB_COUNTS_PER_MA;
}


void set_new_kv(float kv)
{
	bool zero_kv_output = false;
	float ma_limit;

	taskENTER_CRITICAL();
	kv_stability_count = 0;
#ifdef CALIBRATION_MODE
	kv_fb_count = 0;
#endif

	if ((sys_stat & (1UL << SYS_HV_CTRL_EN)) != 0U)
	{
		if(kv < setpoints[SP_KV])
		{
			kv_stat = KV_DOWN;
		}
		else if(kv > setpoints[SP_KV])
		{
			kv_stat = KV_UP;
		}
		else
		{
			kv_stat = KV_STAY;
		}

		sys_stat |= (1UL << SYS_KV_RAMPING);
		setpoints[SP_KV] = kv;
	}
	else
	{
		setpoints[SP_KV] = 0;
		kv_out = 0;
		zero_kv_output = true;
	}
	ma_limit = update_ma_target();
	taskEXIT_CRITICAL();

	if (zero_kv_output)
	{
		write_kv(0);
	}
	write_ma_lim(ma_limit);
}

void set_new_pwr(float pwr)
{
	float ma_limit;

	taskENTER_CRITICAL();
	setpoints[SP_PWR] = pwr;
	ma_limit = update_ma_target();
	taskEXIT_CRITICAL();

	write_ma_lim(ma_limit);
}

static float update_ma_target(void)
{
	if(setpoints[SP_PWR] <= 0 || isnan(setpoints[SP_PWR]) ||
			setpoints[SP_KV] <= 0 || isnan(setpoints[SP_KV]))
	{
		setpoints[SP_MA] = 0;
	}
	else
	{
		setpoints[SP_MA] = setpoints[SP_PWR] / setpoints[SP_KV];
	}

	return setpoints[SP_MA] * 1.5f;
}

void set_new_fil(float fil)
{
	fil_stability_count = 0;
	fil_fb_count = 0;

	// check if the filament is going up or down
	if(fil < setpoints[SP_FIL])
	{
		fil_stat = FIL_DOWN;
	}
	else if(fil > setpoints[SP_FIL])
	{
		fil_stat = FIL_UP;
	}
	else
	{
		fil_stat = FIL_STAY;
	}

	set_sys_bit(SYS_WARMING);
	HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_SET);
	setpoints[SP_FIL] = fil;
}

float get_monitored_float_val(uint32_t comm_idx)
{
	float val = 0;

	switch(comm_idx)
	{
		case COMM_PWR_SP:
			val = setpoints[SP_PWR];
			break;
		case COMM_KV_SP:
			val = setpoints[SP_KV];
			break;
		case COMM_MA_LIM_SP:
			val = setpoints[SP_MA_LIM];
			break;
		case COMM_GRID_SP:
			val = setpoints[SP_GRID];
			break;
		case COMM_FIL_SP:
			val = setpoints[SP_FIL];
			break;
		case COMM_FIL_FB:
			val = fb_vals[FB_FIL_A];
			break;
		case COMM_KV_FB:
			val = fb_vals[FB_KV];
			break;
		case COMM_MA_FB:
			val = fb_vals[FB_MA];
			break;
		case COMM_GRID_FB:
			val = fb_vals[FB_GRID];
			break;
		default:
			break;
	}

	return val;
}

uint32_t get_monitored_int_val(uint32_t comm_idx)
{
	uint32_t val = 0;
	switch(comm_idx)
	{
		case COMM_HVPS_STATUS:
			val = sys_stat;
			break;
		case COMM_IO:
			val = sys_io_bits;
			break;
		case COMM_HVPS_RUNTIME:
			val = runtime_ms;
			/*
			 * TODO:
			 * val = sys_fault_bits;
			 * The faults are not updated
			 * It is saved to HVPS_STATUS_RUNTIME on GCB
			*/
			break;
		default:
			break;
	}
	return val;
}

void process_monitoring()
{
	/*
	 * TBD TODO - not required
	 * unimplemented for now, but in the future could possibly add
	 * comparison of output SP and DAC output feedback readings here
	 */

#ifndef CALIBRATION_MODE
	if (!sys_stat_check(SYS_EMISSION_ON))
	{
		if(sys_stat_check(SYS_WARMING))
		{
#endif
			if(fil_ramp_ms <= 0)
			{
				fil_ramp_ms = 1000;
				process_fil_ramp();
			}
#ifndef CALIBRATION_MODE
		}
	}
#endif
}

static bool process_kv_ramp(float *command)
{
	if(!sys_stat_check(SYS_KV_RAMPING))
	{
		return false;
	}

	if(kv_out < 0 || isnan(kv_out) || setpoints[SP_KV] < 0 || isnan(setpoints[SP_KV]))
	{
		setpoints[SP_KV] = 0;
		kv_out = 0;
		clear_sys_bit(SYS_KV_RAMPING);
	}
	else if(setpoints[SP_KV] < config_vals[SYS_CONFIG_MIN_KV])
	{
		setpoints[SP_KV] = 0;
		kv_out = 0;
		clear_sys_bit(SYS_KV_RAMPING);
	}
	else
	{
		update_kv_ramp();
	}

	*command = kv_out;
	return true;
}

static void update_kv_ramp()
{
	float kv_err = setpoints[SP_KV] - fb_vals[FB_KV];
	float kv_err_pct = 100 - (fb_vals[FB_KV] / setpoints[SP_KV] * 100);
	float fb_err = kv_out - fb_vals[FB_KV];

	if(kv_stat == KV_DOWN)
	{
		kv_stability_count = 0;

		if(kv_err >= 0)
		{
#ifdef CALIBRATION_MODE
			kv_stat = KV_UP;
#else
            kv_stat = KV_STAY;
#endif
		}
		else if(kv_err > -5)
		{
	        kv_out -= config_vals[SYS_CONFIG_KV_RAMP_SLOW];
		}
		else if(kv_err > -20)
		{
	        kv_out -= config_vals[SYS_CONFIG_KV_RAMP_SLOW]*4;
		}
		else
		{
	        kv_out -= config_vals[SYS_CONFIG_KV_RAMP_FAST];
		}
	}
	else if(kv_stat == KV_UP)
	{
		kv_stability_count = 0;

		if(kv_err <= 0)
		{
			kv_stat = KV_STAY;
		}
		else if(kv_err < 5)
		{
			kv_out += config_vals[SYS_CONFIG_KV_RAMP_SLOW];
		}
		else if(kv_err < 20)
		{
			kv_out += config_vals[SYS_CONFIG_KV_RAMP_SLOW]*4;
		}
		else
		{
			kv_out += config_vals[SYS_CONFIG_KV_RAMP_FAST];
		}

		if(kv_out >= 50 && fb_err > 50)
		{
			kv_out = 0;
			clear_sys_bit(SYS_KV_RAMPING);
			return;
		}
	}
	else
	{
		if(kv_err_pct >= -2 && kv_err_pct <= 2)
		{
			if(kv_stability_count++ > 2)
			{
				kv_stability_count = 0;
				clear_sys_bit(SYS_KV_RAMPING);
			}
			return;
		}
#ifdef CALIBRATION_MODE
		else if(kv_err > 0)
		{
			kv_stat = KV_UP;
		}
		else
		{
			kv_stat = KV_DOWN;
		}
#endif
	}

	if(kv_out <= 0)
	{
		kv_out = 0;
	}
	else if(kv_out >= config_vals[SYS_CONFIG_MAX_KV])
	{
		kv_out = config_vals[SYS_CONFIG_MAX_KV];
	}
}

static void process_fil_ramp()
{
	//Do nothing if no longer warming
	if(!sys_stat_check(SYS_WARMING))
	{
		return;
	}

	//Make sure values are valid
	if(fil_out < 0 || isnan(fil_out) || setpoints[SP_FIL] < 0 || isnan(setpoints[SP_FIL]))
	{
		//If not set heater to 0
		setpoints[SP_FIL] = 0;
		fil_out = 0;
		HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
		clear_sys_bit(SYS_WARMING);
	}
	//If setpoint is low just to 0
	else if(setpoints[SP_FIL] < config_vals[SYS_CONFIG_FIL_INIT])
	{
		setpoints[SP_FIL] = 0;
		fil_out = 0;
		HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
		clear_sys_bit(SYS_WARMING);
	}
	//Otherwise get filament ramp up value
	else
	{
		get_fil_ramp();
	}

	write_fil_a(fil_out);
}

static void get_fil_ramp()
{
	float fil_err = setpoints[SP_FIL] - fb_vals[FB_FIL_A];
	float fil_err_pct = 100 - (fb_vals[FB_FIL_A] / setpoints[SP_FIL] * 100);
	float fb_err = fil_out - fb_vals[FB_FIL_A];

	if(fil_err <= 0)
	{
		if(fil_err_pct >= -2 && fil_err_pct <= 2)
		{
			if(fil_stability_count++ > 2)
			{
				fil_stability_count = 0;
				clear_sys_bit(SYS_WARMING);
			}
			return;
		}
		else
		{
			fil_out = setpoints[SP_FIL];
		}
	}
	else
	{
		fil_stability_count = 0;

		if(sys_stat_check(SYS_FAST_WARMUP_EN))
		{
			fil_out += 40;
		}
		else
		{
			fil_out += 20;
		}

		if(fil_out >= 1000 && fb_err > 300)
		{
			fil_fb_count++;

			if(fil_fb_count > 5)
			{
				fil_fb_count = 0;
				fil_out = 0;
				clear_sys_bit(SYS_WARMING);
				return;
			}
		}
	}

	if(fil_out <= config_vals[SYS_CONFIG_FIL_INIT])
	{
		fil_out = config_vals[SYS_CONFIG_FIL_INIT];
	}
	else if(fil_out >= config_vals[SYS_CONFIG_FIL_LIM])
	{
		fil_out = config_vals[SYS_CONFIG_FIL_LIM];
	}
}

#if 1
#define GRID_MAX_STEP 			25

static bool run_grid_ctrl(float *command)
{
    float minimum_grid = config_vals[SYS_CONFIG_MIN_GRID];
    float maximum_grid = config_vals[SYS_CONFIG_MAX_GRID];

    bool enabled = (config_vals[SYS_CONFIG_RUN_PID] != 0.0f);

    if (!enabled)
    {
        return false;
    }

    float error = setpoints[SP_MA] - fb_vals[FB_MA];
    float grid_adj = error;

    if (setpoints[SP_KV] == 50.0f)
    {
        grid_adj *= -25.0f;
    }
    else if (setpoints[SP_KV] == 70.0f)
    {
        grid_adj *= -35.0f;
    }
    else if (setpoints[SP_KV] == 100.0f)
    {
        grid_adj *= -150.0f;
    }
    else
    {
        return false;
    }

#if defined(CALIBRATION_MODE)
    /* Clamp adjustment to configured step limit */
    if (grid_adj > GRID_MAX_STEP)
    {
        grid_adj = GRID_MAX_STEP;
    }
    else if (grid_adj < -GRID_MAX_STEP)
    {
        grid_adj = -GRID_MAX_STEP;
    }
#endif

    grid_out += grid_adj;

    if (grid_out <= minimum_grid)
    {
        grid_out = minimum_grid;
        grid_ctrl_count++;
    }
    else if (grid_out >= maximum_grid)
    {
        grid_out = maximum_grid;
        grid_ctrl_count++;
    }

    *command = grid_out;
    return true;
}
#else
static bool run_grid_ctrl(float *command)
{
	float minimum_grid = config_vals[SYS_CONFIG_MIN_GRID];
	float maximum_grid = config_vals[SYS_CONFIG_MAX_GRID];
	bool enabled = (config_vals[SYS_CONFIG_RUN_PID] != 0.0f)
			&& sys_stat_check(SYS_EMISSION_ON);


	if (!enabled)
	{
		bool output_changed = grid_control_active;
		if (output_changed)
		{
			grid_out = maximum_grid;
			*command = grid_out;
		}
		grid_control_active = false;
		grid_integral = 0.0f;
		grid_feedforward = maximum_grid;
		return output_changed;
	}

	float error = setpoints[SP_MA] - fb_vals[FB_MA];
	float kp = get_grid_kp(setpoints[SP_KV]);
	float integral_time = config_vals[SYS_CONFIG_GRID_INTEGRAL_TIME];
	float slew_down = config_vals[SYS_CONFIG_GRID_SLEW_DOWN];
	float slew_up = config_vals[SYS_CONFIG_GRID_SLEW_UP];


	if (!grid_control_active)
	{
		grid_control_active = true;
		grid_integral = 0.0f;
		grid_previous_kp = kp;
		grid_feedforward = maximum_grid;
		grid_out = maximum_grid;
		*command = grid_out;
		return true;
	}

	grid_integral += (grid_previous_kp - kp) * error;
	grid_integral += grid_feedforward - maximum_grid;
	grid_previous_kp = kp;
	grid_feedforward = maximum_grid;

	float integral_delta = (kp / integral_time) * CONTROL_PERIOD_S * error;
	float integral_candidate = grid_integral + integral_delta;

	float lower_reachable = grid_out - (slew_down * CONTROL_PERIOD_S);
	float upper_reachable = grid_out + (slew_up * CONTROL_PERIOD_S);
	lower_reachable = clamp_float(lower_reachable, minimum_grid, maximum_grid);
	upper_reachable = clamp_float(upper_reachable, minimum_grid, maximum_grid);

	float unsaturated = maximum_grid + (kp * error) + integral_candidate;
	bool inside_limits = (unsaturated >= lower_reachable) && (unsaturated <= upper_reachable);
	bool unwinding_high = (unsaturated > upper_reachable) && (integral_delta < 0.0f);
	bool unwinding_low = (unsaturated < lower_reachable) && (integral_delta > 0.0f);

	if (inside_limits || unwinding_high || unwinding_low)
	{
		grid_integral = integral_candidate;
	}

	float grid_value = maximum_grid + (kp * error) + grid_integral;
	grid_out = clamp_float(grid_value, lower_reachable, upper_reachable);

	if ((grid_out <= minimum_grid) || (grid_out >= maximum_grid))
	{
		grid_ctrl_count++;
	}

	*command = grid_out;
	return true;
}
#endif

static float get_grid_kp(float kv)
{
	if (kv <= 50.0f)
	{
		return config_vals[SYS_CONFIG_GRID_KP_50];
	}
	if (kv <= 70.0f)
	{
		return config_vals[SYS_CONFIG_GRID_KP_70];
	}
	return config_vals[SYS_CONFIG_GRID_KP_100];
}


static float clamp_float(float value, float minimum, float maximum)
{
	if (value < minimum)
	{
		return minimum;
	}
	if (value > maximum)
	{
		return maximum;
	}
	return value;
}


