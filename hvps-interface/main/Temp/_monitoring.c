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

uint32_t hvps_fault_mask = 0;

uint32_t sys_stat = 0;
uint32_t sys_io_bits = 0;
uint32_t sys_fault_bits = 0;

float fb_vals[NUM_FB];

float config_vals[NUM_SYS_CONFIG];
float setpoints[NUM_SP];
float kv_out = 0;
float fil_out = 0;
float grid_out = 0;

uint32_t kv_stability_count = 0;

float fil_adj = 0;
float prev_error = 0;
float acc_error = 0;

float p_term_debug = 0;
float i_term_debug = 0;
float d_term_debug = 0;

static void update_ma_target();
static void process_kv_ramp();
static void process_fil_ramp();
static void update_kv_ramp();
static float get_fil_ramp();
static void run_pid();
static void run_grid_ctrl();

void setup_system_monitoring()
{
	//TBD TODO initialize system config values
#ifdef CALIBRATION_MODE
	config_vals[SYS_CONFIG_MAX_PWR] = 200;
#else
	config_vals[SYS_CONFIG_MAX_PWR] = 200;
#endif
	config_vals[SYS_CONFIG_MIN_KV] = 4;
	config_vals[SYS_CONFIG_KV_BOUND] = 0.1;
	config_vals[SYS_CONFIG_KV_RAMP_FAST] = 1;
	config_vals[SYS_CONFIG_KV_RAMP_SLOW] = 0.5;		//EDIT: was 0.1
	config_vals[SYS_CONFIG_MAX_KV] = 101;
	config_vals[SYS_CONFIG_FIL_INIT] = 1000;
	config_vals[SYS_CONFIG_FIL_LIM] = 4000;
	config_vals[SYS_CONFIG_P_HARD] = 10;
	config_vals[SYS_CONFIG_D_HARD] = 2;
	config_vals[SYS_CONFIG_P] = 5;
	config_vals[SYS_CONFIG_I] = 2;
	config_vals[SYS_CONFIG_D] = 1;
	config_vals[SYS_CONFIG_MAX_I_POS] = 1.5;
	config_vals[SYS_CONFIG_MAX_I_NEG] = -0.1;
#ifdef DEMO_MODE
	config_vals[SYS_CONFIG_MAX_ADJ_POS] = 0.1;
	config_vals[SYS_CONFIG_MAX_ADJ_NEG] = -0.1;
#else
	config_vals[SYS_CONFIG_MAX_ADJ_POS] = 3;
	config_vals[SYS_CONFIG_MAX_ADJ_NEG] = -5;
#endif

	config_vals[SYS_CONFIG_HARD_THRESH_POS] = 0.15;
	config_vals[SYS_CONFIG_HARD_THRESH_NEG] = -0.15;
	config_vals[SYS_CONFIG_P_THRESH_POS] = 0.08;
	config_vals[SYS_CONFIG_P_THRESH_NEG] = -0.08;
	config_vals[SYS_CONFIG_D_THRESH_POS] = 0.01;
	config_vals[SYS_CONFIG_D_THRESH_NEG] = -0.01;
	//config_vals[SYS_CONFIG_MA_THRESH] = 2.5;
	config_vals[SYS_CONFIG_MAX_ERR_POS] = 0.2;
	config_vals[SYS_CONFIG_MAX_ERR_NEG] = -0.2;
	config_vals[SYS_CONFIG_FAST_WU] = 0;
#ifdef CALIBRATION_MODE
	config_vals[SYS_CONFIG_RUN_PID] = 0;
#else
	config_vals[SYS_CONFIG_RUN_PID] = 1;
	set_sys_bit(SYS_PID_ON);
#endif

	//Initialize set points
#ifdef CALIBRATION_MODE
	setpoints[SP_PWR] = 0;
#else
	setpoints[SP_PWR] = 200;
#endif
	setpoints[SP_KV] = 0;
	setpoints[SP_MA_LIM] = 2.5;		//this is fine assuming 2V = 1mA, so 25% additional. So 2.5V = 1.25mA (for 50W operation)
	setpoints[SP_GRID] = 0;
	setpoints[SP_FIL] = 0;

	hvps_fault_mask = (1 << IN_FIL_CLK_FAULT) | (1 << IN_CAT_ARC) |
			(1 << IN_FAN_FAULT) | (1 << IN_OC_24_FAULT) |
			(1 << IN_MASTER_FAULT) | (1 << IN_OC_HV_FAULT) |
			(1 << IN_TEMP_1_FAULT) | (1 << IN_OC_CAT_FAULT) |
			(1 << IN_TEMP_3_FAULT) | (1 << IN_TEMP_2_FAULT);
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
	//TBD TODO NOTE: if filament current is > 4 A kill filament DAC directly, update setpoint and throw fault
#ifdef OLD_REV_HW
	fb_vals[FB_FIL_A] = (float)(vals[INT_ADC_FIL_A])/0.975;
	fb_vals[FB_GRID] = (float)(vals[INT_ADC_GRID])/13.65;
#else
	fb_vals[FB_FIL_A] = (float)(vals[INT_ADC_FIL_A])/0.910222222222222;
	//fb_vals[FB_FIL_V] = (float)(vals[INT_ADC_KV_SP])/27.306666666666667;
	fb_vals[FB_GRID] = (float)(vals[INT_ADC_GRID])/13.003174603174602;
#endif

#ifdef FIL_DEBUG_MODE
	fb_vals[FB_FIL_A] = (float)(vals[INT_ADC_FILSP])/0.273;
#endif

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
		//TBD TODO throw fault, incl setting fault status to ctrl board
	}

	if(new_sys_io_inactive & (1<<IN_HV_INT))
	{
		lock_hv();
		clear_sys_bit(SYS_EMISSION_ON);
	}
#ifdef CALIBRATION_MODE
	else if(new_sys_io_active &(1<<IN_HV_INT))
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
		//enable_grid_clock();
		if(sys_stat_check(SYS_HV_CTRL_EN) && sys_stat_check(SYS_GRID_CTRL_EN))
		{
			//enable_grid_clock();
			pid_ms = 100;
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
#endif
		clear_sys_bit(SYS_EMISSION_ON);
		HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);
		//disable_grid_clock();
	}
}

void report_kv_fb(uint32_t kv_fb)
{
#ifdef OLD_REV_HW
	fb_vals[FB_KV] = (float)(kv_fb)/218.45;	//TBD TODO magic number
#else
	fb_vals[FB_KV] = (float)(kv_fb)/266.6666666666667;	//TBD TODO magic number
#endif

#ifdef KVFB_HIGH_MODE
	fb_vals[FB_KV] *= 3;
#endif

#ifdef KVFB_LOW_MODE
	fb_vals[FB_KV] *= 0.8;
#endif
}

void report_ma_fb(uint32_t ma_fb)
{
#ifdef OLD_REV_HW
	fb_vals[FB_MA] = (float)(ma_fb)/10922.66;
#else
	//fb_vals[FB_MA] = (float)(ma_fb)/15238.09523809524;	// 1 / 20 K divider
	//fb_vals[FB_MA] =(float)(ma_fb)/6201.5504;				//31.6 / 20 K divider
	fb_vals[FB_MA] =(float)(ma_fb)/6400;				//30 / 20 K divider
#endif

}


void set_new_kv(float kv)
{
	kv_stability_count = 0;

	//Make sure HV interlock is OK
	if(sys_stat_check(SYS_HV_CTRL_EN))
	{
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

	write_ma_lim(setpoints[SP_MA_LIM]);
	//write_ma_lim(config_vals[SYS_CONFIG_MA_THRESH]);
}

void set_new_fil(float fil)
{
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
			//val = p_term_debug;
			break;
		case COMM_KV_SP:
			val = setpoints[SP_KV];
			//val = i_term_debug;
			break;
		case COMM_MA_LIM_SP:
			val = setpoints[SP_MA_LIM];
			//val = d_term_debug;
			break;
		case COMM_GRID_SP:
			val = setpoints[SP_GRID];
			//val = fil_adj;
			break;
		case COMM_FIL_SP:
#ifdef CALIBRATION_MODE
			val = setpoints[SP_FIL];
#else
			val = fil_out;
			//val = fb_vals[FB_FIL_V];
#endif
			break;
		case COMM_FIL_FB:
			val = fb_vals[FB_FIL_A];
			break;
		case COMM_KV_FB:
			val = fb_vals[FB_KV];
			break;
		case COMM_MA_FB:
			val = fb_vals[FB_MA];
#ifdef FIXED_MA_REPORT
			val = 0.1;
#endif
#ifdef LARGE_MA_REPORT
			val *= 10;
#endif
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
		case COMM_FAULTS_LIST:
			val = sys_fault_bits;
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

	//Only check kV and heater ramps while no emission is ongoing
	if(!sys_stat_check(SYS_EMISSION_ON))
	{
		//Ramp kV every 1000 ms
		if(kv_ramp_ms <= 0)
		{
			kv_ramp_ms = 1000; //TBD TODO magic number
			process_kv_ramp();
		}

		//Ramp heater every 1000 ms
		if(fil_ramp_ms <= 0)
		{

			fil_ramp_ms = 1000; //TBD TODO magic number

			if(sys_stat_check(SYS_WARMING))
			{
				process_fil_ramp();
			}
		}
	}
	else if(pid_ms <= 0)
	{
		pid_ms = 10;
		//run_pid();		EDIT: 1/16/25
		run_grid_ctrl();	//grid control, could put onto separate timer if desired
#ifdef FORCE_EMISSION_KV_CLAMP
		write_kv(0);
#endif
	}
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

	//Make sure we do not overshoot setpoint after ramp
	if(kv_out >= config_vals[SYS_CONFIG_MAX_KV])
	{
		kv_out = 0;
		setpoints[SP_KV] = 0;
		clear_sys_bit(SYS_KV_RAMPING);
		//TBD TODO throw error
	}
	write_kv(kv_out);
}


static void update_kv_ramp()
{
	float kv_err = fb_vals[FB_KV] - setpoints[SP_KV];
	float kv_err_neg = kv_err * -1;

	//Check to see if KV is stable within a certain bound
	if(kv_err < 1 && kv_err_neg < 1)
	{
		if(kv_stability_count++ > 2)
		{
			kv_stability_count = 0;
			clear_sys_bit(SYS_KV_RAMPING);
			if(kv_out > (setpoints[SP_KV]*1.1) || kv_out < (setpoints[SP_KV]*0.9))
			{
				kv_out = 0;
				setpoints[SP_KV] = 0;
				clear_sys_bit(SYS_KV_RAMPING);
				//TBD TODO throw error here if desired
			}
		}
		return;
	}

	kv_stability_count = 0;

	//If kv is high, simply drop it down
	if(kv_err > 2.5)
	{
		kv_out = setpoints[SP_KV] * 0.9;
	}
	//Otherwise kV is close, but still too high, decrease slowly
	else if(kv_err > 1)
	{
		kv_out -= config_vals[SYS_CONFIG_KV_RAMP_SLOW];
	}
	else if(kv_err > 0.1)
	{
		kv_out -= (config_vals[SYS_CONFIG_KV_RAMP_SLOW]/5);	//Edit: was /4
	}
	//If kv is much lower, increase quickly
	else if(kv_err_neg > 5)
	{
		kv_out += config_vals[SYS_CONFIG_KV_RAMP_FAST];
	}
	//If kv is still too low but close, increase slowly
	else if(kv_err_neg > 1)
	{
		kv_out += config_vals[SYS_CONFIG_KV_RAMP_SLOW];
	}
	else if(kv_err_neg > 0.1)
	{
		kv_out += (config_vals[SYS_CONFIG_KV_RAMP_SLOW]/5);	//Edit: was /4
	}
}

static void process_fil_ramp()
{
	//Make sure values are valid
	if(fil_out < 0 || isnan(fil_out) || setpoints[SP_FIL] < 0 || isnan(setpoints[SP_FIL]))
	{
		//If not set heater to 0
		setpoints[SP_FIL] = 0;
		fil_out = 0;
		write_fil_a(fil_out);
		HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
		clear_sys_bit(SYS_WARMING);
	}
	//Drop heater quickly
	else if(fil_out > setpoints[SP_FIL])
	{
		fil_out -= 500;	//TBD TODO magic number
		if(fil_out <= setpoints[SP_FIL] || fil_out < config_vals[SYS_CONFIG_FIL_INIT])
		{
			fil_out = setpoints[SP_FIL];
			HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
			clear_sys_bit(SYS_WARMING);
		}
		write_fil_a(fil_out);
	}
	//Ramp up if current heater is lower than setpoint
	else if(fil_out <= setpoints[SP_FIL])
	{
		//If output is lower than initial first step, ramp to initial step
		if(fil_out < config_vals[SYS_CONFIG_FIL_INIT])
		{
			fil_out = config_vals[SYS_CONFIG_FIL_INIT];
		}
		else
		{
			fil_out += get_fil_ramp();
		}


		//Make sure we do not overshoot setpoint after ramp
		if(fil_out >= setpoints[SP_FIL])
		{
			fil_out = setpoints[SP_FIL];
			HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
			clear_sys_bit(SYS_WARMING);
		}
		write_fil_a(fil_out);
	}
}

static float get_fil_ramp()
{
	float step_val = 0;
	//TBD TODO magic numbers

	//24.5mA/second ramp up over 2 minutes to go from 1000mA to 3900mA FB
	step_val = 24.5;

	return step_val;
}

/* ORIGINAL STEP VALUE FUNCTION FOR FILAMENT */
//Ramp the filament, since power going to filament is not linear with current, we must take smaller steps as current goes higher
/* static float get_fil_ramp()
{
	float step_val = 0;
	//TBD TODO magic numbers

	//Fastest at lower current, decrease step size as current goes up
	if(fil_out < 2500)
	{
		step_val = 10;
	}
	else if(fil_out < 3000)
	{
		step_val = 5;
	}
	else if(fil_out < 3500)
	{
		step_val = 3;
	}
	else
	{
		step_val = 2;
	}

	//Scaling factor for if fast filament ramp is enabled
	if(sys_stat_check(SYS_FAST_WARMUP_EN) || (config_vals[SYS_CONFIG_FAST_WU] == 1))
	{
		step_val *= 20;
	}

	return step_val;
}*/


//Test grid control. All numbers can be tweaked
static void run_grid_ctrl()
{

	float error = setpoints[SP_MA] - fb_vals[FB_MA];
	float grid_adj = 0;

	if(error > 0.1)
	{
		//Decrease grid voltage by 10
		grid_adj = -10;
	}
	else if(error > 0.05)
	{
		grid_adj = -1;
	}
	else if(error > 0.02)
	{
		//Decrease grid voltage by 1
		grid_adj = -0.5;
	}
	else if(error > 0)
	{
		grid_adj = -0.1;
	}
	else if(error < -0.1)
	{
		//Increase grid voltage by 10
		grid_adj = 10;
	}
	else if(error < -0.05)
	{
		grid_adj = 1;
	}
	else if(error < -0.02)
	{
		//Increase grid voltage by 1
		grid_adj = 0.5;
	}
	else if(error < 0)
	{
		grid_adj = 0.1;
	}

	grid_out += grid_adj;

	if(grid_out <= 0)
	{
		grid_out = 0;

		//Emission current is low, grid at min and can't help anymore, need to increase mA

		//Update filament value
		fil_out += 0.5;

		if(fil_out > config_vals[SYS_CONFIG_FIL_LIM])
		{
			fil_out = config_vals[SYS_CONFIG_FIL_LIM];
		}

		write_fil_a(fil_out);
	}
	else if(grid_out >= 199)
	{
		grid_out = 199;

		//Emission current is high, grid at max and can't help anymore, need to decrease mA

		//Update filament value
		fil_out -= 0.5;

		if(fil_out > config_vals[SYS_CONFIG_FIL_LIM])
		{
			fil_out = config_vals[SYS_CONFIG_FIL_LIM];
		}

		write_fil_a(fil_out);
	}

	write_grid(grid_out);
}

static void run_pid()
{
	if(config_vals[SYS_CONFIG_RUN_PID] == 0)
	{
		return;
	}
	float error = setpoints[SP_MA] - fb_vals[FB_MA];
	float diff_error = error - prev_error;

	float p_term = 0;
	float i_term = 0;
	float d_term = 0;

	bool p_ok = false;
	bool d_ok = false;
	bool hard_ok = false;

	/*
	//If error is too large, terminate emission
	if(error > config_vals[SYS_CONFIG_MAX_ERR_POS] || error < config_vals[SYS_CONFIG_MAX_ERR_NEG])
	{
		lock_hv();
		lock_grid();
		return;
	}*/

	//Check error values to help determine PID action
	if(error < config_vals[SYS_CONFIG_P_THRESH_POS] && error > config_vals[SYS_CONFIG_P_THRESH_NEG])
	{
		p_ok = true;
	}
	if(diff_error < config_vals[SYS_CONFIG_D_THRESH_POS] && diff_error > config_vals[SYS_CONFIG_D_THRESH_NEG])
	{
		d_ok = true;
	}
	if(error < config_vals[SYS_CONFIG_HARD_THRESH_POS] && error > config_vals[SYS_CONFIG_HARD_THRESH_NEG])
	{
		hard_ok = true;
	}

	if(!hard_ok)
	{
		//If we are very far use very hard PD drive
		acc_error = 0;
		p_term = config_vals[SYS_CONFIG_P_HARD] * error;
		p_term_debug = p_term;
		i_term = config_vals[SYS_CONFIG_I] * acc_error;
		i_term_debug = i_term;
		d_term = config_vals[SYS_CONFIG_D_HARD] * diff_error;
		d_term_debug = d_term;
	}
	else if(p_ok && d_ok)
	{
		//If we are close and are not in a drastic change, use only PI
		diff_error = 0;
		acc_error += error;
		if(acc_error > config_vals[SYS_CONFIG_MAX_I_POS])
		{
			acc_error = config_vals[SYS_CONFIG_MAX_I_POS];
		}
		else if(acc_error < config_vals[SYS_CONFIG_MAX_I_NEG])
		{
			acc_error = config_vals[SYS_CONFIG_MAX_I_NEG];
		}
		p_term = config_vals[SYS_CONFIG_P] * error;
		p_term_debug = p_term;
		i_term = config_vals[SYS_CONFIG_I] * acc_error;
		i_term_debug = i_term;
		d_term = config_vals[SYS_CONFIG_D] * diff_error;
		d_term_debug = d_term;
	}
	else
	{
		//If we are far or have a drastic change, use only PD
		p_term = config_vals[SYS_CONFIG_P] * error;
		p_term_debug = p_term;
		i_term = config_vals[SYS_CONFIG_I] * acc_error;
		i_term_debug = i_term;
		d_term = config_vals[SYS_CONFIG_D] * diff_error;
		d_term_debug = d_term;
		acc_error = 0;
	}

	prev_error = error;

	//Get filament adjustment value based on calculations above
	fil_adj = p_term + i_term + d_term;

	//If filament adjustment is too radical, limit filament step change
	if(fil_adj > config_vals[SYS_CONFIG_MAX_ADJ_POS])
	{
		fil_adj = config_vals[SYS_CONFIG_MAX_ADJ_POS];
	}
	else if(fil_adj < config_vals[SYS_CONFIG_MAX_ADJ_NEG])
	{
		fil_adj = config_vals[SYS_CONFIG_MAX_ADJ_NEG];
	}

	//Update filament value
	fil_out += fil_adj;
	if(fil_out > config_vals[SYS_CONFIG_FIL_LIM])
	{
		fil_out = config_vals[SYS_CONFIG_FIL_LIM];
	}
	write_fil_a(fil_out);
}
