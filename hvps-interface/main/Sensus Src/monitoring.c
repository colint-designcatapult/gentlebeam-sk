#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"
#include "math.h"

#include "monitoring.h"
#include "adc.h"
#include "control_comm.h"
#include "ext_dacs.h"
#include "io.h"
#include "timers.h"
#include "setup.h"

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
float prev_error = 0;
float acc_error = 0;

static void update_ma_target();
static void process_kv_ramp();
static void process_fil_ramp();
static void update_kv_ramp();
static void get_fil_ramp();
static void run_grid_ctrl();

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

	//Initialize set points
	setpoints[SP_PWR] = 0;
	setpoints[SP_KV] = 0;
	setpoints[SP_GRID] = 0;
	setpoints[SP_FIL] = 0;
	setpoints[SP_MA_LIM] = 0;

	hvps_fault_mask =
			(1 << IN_FIL_CLK_FAULT) | (1 << IN_CAT_ARC) 	| (1 << IN_FAN_FAULT) 	| (1 << IN_OC_24_FAULT) | (1 << IN_MASTER_FAULT) |
			(1 << IN_OC_HV_FAULT) 	| (1 << IN_TEMP_1_FAULT)| (1 << IN_OC_CAT_FAULT)| (1 << IN_TEMP_3_FAULT)| (1 << IN_TEMP_2_FAULT);
}

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
	if(bitpos < NUM_SYS_BITS)
	{
		sys_stat |= (1<<bitpos);
	}
}


void clear_sys_bit(uint8_t bitpos)
{
	if(bitpos < NUM_SYS_BITS)
	{
		sys_stat &= ~(1<<bitpos);
	}
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

void report_io_state(uint32_t io_bits)
{
	//Calculate which bits are now high
	uint32_t new_sys_io_active = (sys_io_bits ^ io_bits) & io_bits;

	//Calculate which bits are now low
	uint32_t new_sys_io_inactive = (sys_io_bits ^ io_bits) & sys_io_bits;

	//Save all new bits
	sys_io_bits = io_bits;

	//If any new HVPS fault bits are active, throw a fault
	if(new_sys_io_active && (hvps_fault_mask != 0))
	{
		//TBD TODO throw fault, including setting fault status to ctrl board
	}

	if(new_sys_io_inactive & (1<<IN_HV_INT))
	{
		lock_hv();
		clear_sys_bit(SYS_EMISSION_ON);
	}
#ifdef CALIBRATION_MODE
	else if(new_sys_io_active & (1<<IN_HV_INT))
	{
		set_sys_bit(SYS_HV_CTRL_EN);
		HAL_GPIO_WritePin(GPIOE, IO_PFC_ALLOWED_Pin|IO_HV_ALLOWED_Pin, GPIO_PIN_SET);
		HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_SET);
	}
#endif

	if(new_sys_io_inactive & (1<<IN_GRID_INT))
	{
		lock_grid();
		clear_sys_bit(SYS_EMISSION_ON);
	}
#ifdef CALIBRATION_MODE
	else if(new_sys_io_active & (1<<IN_GRID_INT))
	{
		set_sys_bit(SYS_GRID_CTRL_EN);
	}
#endif

	if(new_sys_io_active & (1<<IN_MASTER_FAULT))
	{
		lock_hv();
		lock_grid();
		HAL_GPIO_WritePin(GPIOB, IO_PS_OK_Pin, GPIO_PIN_RESET);
	}
	else if(new_sys_io_inactive & (1<<IN_MASTER_FAULT))
	{
		HAL_GPIO_WritePin(GPIOB, IO_PS_OK_Pin, GPIO_PIN_SET);
	}

	if(new_sys_io_active & (1<<IN_BEAM_CTRL))
	{
		if(sys_stat_check(SYS_HV_CTRL_EN) && sys_stat_check(SYS_GRID_CTRL_EN))
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
	else if(new_sys_io_inactive & (1<<IN_BEAM_CTRL))
	{
#ifdef CALIBRATION_MODE
            clear_sys_bit(SYS_CAL_GRID_INT_EN);
#else
            clear_sys_bit(SYS_EMISSION_ON);
		    HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);
#endif
	}
}

void report_kv_fb(uint32_t kv_fb)
{
	float divider = 266.6666667;	//TBD TODO magic number

	fb_vals[FB_KV] = (float)(kv_fb)/divider;
}

void report_ma_fb(uint32_t ma_fb)
{
	float divider = 3200;

	//using (30/20K) divider and 1V:1mA emission current feedback
	fb_vals[FB_MA] =(float)(ma_fb)/divider;
}


void set_new_kv(float kv)
{
	kv_stability_count = 0;
#ifdef CALIBRATION_MODE
	kv_fb_count = 0;
#endif

	//Make sure HV interlock is OK
	if(sys_stat_check(SYS_HV_CTRL_EN))
	{
		// check if the kV is going up or down
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

		set_sys_bit(SYS_KV_RAMPING);
		setpoints[SP_KV] = kv;
		update_ma_target();
	}
	else
	{
		setpoints[SP_KV] = 0;
		kv_out = 0;
		write_kv(0);
		update_ma_target();
	}
}

void set_new_pwr(float pwr)
{
	setpoints[SP_PWR] = pwr;
	update_ma_target();
}

static void update_ma_target()
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

	write_ma_lim(setpoints[SP_MA] * 1.5);
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
	 * */

	if (sys_stat_check(SYS_EMISSION_ON))
	{
		if(pid_ms <= 0)
		{
			pid_ms = 50;		//Monitor grid every 50ms
			run_grid_ctrl();
		}
	}
#ifndef CALIBRATION_MODE
    else
    {
#endif
#ifndef CALIBRATION_MODE
        if(sys_stat_check(SYS_WARMING))
        {
#endif
            if(fil_ramp_ms <= 0)
            {
                fil_ramp_ms = 1000;	//Ramp heater every 1000ms
                process_fil_ramp();
            }
#ifndef CALIBRATION_MODE
        }
#endif

#ifndef CALIBRATION_MODE
        if(sys_stat_check(SYS_KV_RAMPING))
        {
#endif
            if(kv_ramp_ms <= 0)
            {
                kv_ramp_ms = 1000;	//Ramp kV every 1000ms
                process_kv_ramp();
            }
#ifndef CALIBRATION_MODE
        }
#endif
#ifndef CALIBRATION_MODE
    }
#endif
}

static void process_kv_ramp()
{
	//Do nothing if no longer ramping
	if(!sys_stat_check(SYS_KV_RAMPING))
	{
		return;
	}

	//Make sure values are valid
	if(kv_out < 0 || isnan(kv_out) || setpoints[SP_KV] < 0 || isnan(setpoints[SP_KV]))
	{
		//If not set kV to 0
		setpoints[SP_KV] = 0;
		kv_out = 0;
		clear_sys_bit(SYS_KV_RAMPING);

        //TBD TODO throw error if desired
	}
	//If setpoint is low just go to 0
	else if(setpoints[SP_KV] < config_vals[SYS_CONFIG_MIN_KV])
	{
		setpoints[SP_KV] = 0;
		kv_out = 0;
		clear_sys_bit(SYS_KV_RAMPING);
	}
	//Otherwise get kv ramp value
	else
	{
		update_kv_ramp();
	}

	write_kv(kv_out);
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

static void run_grid_ctrl()
{
	if(config_vals[SYS_CONFIG_RUN_PID] == 0)
	{
		return;
	}

	float error = setpoints[SP_MA] - fb_vals[FB_MA];
	float grid_adj = error;

	if(setpoints[SP_KV] == 50)
	{
		grid_adj *= -25;
	}
	else if(setpoints[SP_KV] == 70)
	{
		grid_adj *= -35;
	}
	else if(setpoints[SP_KV] == 100)
	{
		grid_adj *= -150;
	}
	else
	{
		return;
	}

#if defined (CALIBRATION_MODE)
	/* Clamp grid_adj to stay within -GRID_MAX_STEP and GRID_MAX_STEP */
	if (grid_adj > GRID_MAX_STEP) 
	{
		grid_adj = GRID_MAX_STEP;
	} else if (grid_adj < -GRID_MAX_STEP) 
	{
		grid_adj = -GRID_MAX_STEP;
	}
#endif

	grid_out += grid_adj;

	if(grid_out <= config_vals[SYS_CONFIG_MIN_GRID])
	{
		grid_out = config_vals[SYS_CONFIG_MIN_GRID];
		grid_ctrl_count++;
	}
	else if(grid_out >= config_vals[SYS_CONFIG_MAX_GRID])
	{
		grid_out = config_vals[SYS_CONFIG_MAX_GRID];
		grid_ctrl_count++;
	}

	write_grid(grid_out);
}
