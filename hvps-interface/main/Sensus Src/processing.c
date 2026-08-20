#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"
#include "math.h"

#include "processing.h"
#include "control_comm.h"
#include "ext_dacs.h"
#include "io.h"
#include "monitoring.h"

#ifndef CALIBRATION_MODE
static void set_warmup_speed(int32_t val);
#endif
static void set_pwr_cmd(float val);
static void set_kv_cmd(float val);
static void set_fil_cmd(float val);

static void update_config_lock(int32_t password);
static void update_config_val(float val, int32_t idx);

#ifdef CALIBRATION_MODE
static void calibration_start();
static void calibration_stop();
#endif

void process_command(int32_t cmd, float param_f, int32_t param_i)
{
	//Ensure float value is valid
	if(isnan(param_f))
	{
		return;
	}

	//Throw error if command besides alive received during emission
	if(sys_stat_check(SYS_EMISSION_ON) && (cmd != CTRL_CMD_ALIVE))
	{
		//TBD TODO throw fault
		//return;
	}

	//Process command here (switch statements)
	switch(cmd)
	{
		case CTRL_CMD_CLEAR_FAULTS:
			//TBD TODO tell faults to clear
			break;
		case CTRL_CMD_SET_PWR:
			set_pwr_cmd(param_f);
			break;
		case CTRL_CMD_SET_KV:
			set_kv_cmd(param_f);
			break;
		case CTRL_CMD_SET_MA_LIM:
			write_ma_lim(param_f);
			break;
		case CTRL_CMD_SET_GRID:
			write_grid(param_f);
			break;
		case CTRL_CMD_SET_FIL:
#ifndef CALIBRATION_MODE
            set_warmup_speed(param_i);
#endif
			set_fil_cmd(param_f);
			break;
		case CTRL_CMD_INTERLOCK_TEST:
			interlock_test(param_i);
			break;
		case CTRL_CMD_CONFIG_PASSWORD:
			update_config_lock(param_i);
			break;
		case CTRL_CMD_SET_CONFIG:
			update_config_val(param_f, param_i);
			break;
		case CTRL_CMD_VERSION_REQUEST:
			send_hvps_version();
			break;
#ifdef CALIBRATION_MODE
		case CTRL_CMD_CAL_START:
			calibration_start();
			break;
		case CTRL_CMD_CAL_STOP:
			calibration_stop();
			break;
#endif
		default:
			break;
	}
}

#ifndef CALIBRATION_MODE
//Set whether warmup is fast or slow
static void set_warmup_speed(int32_t val)
{
	if(val == 0)
	{
		clear_sys_bit(SYS_FAST_WARMUP_EN);
	}
	else
	{
		set_sys_bit(SYS_FAST_WARMUP_EN);
	}
}
#endif

//Sets power
static void set_pwr_cmd(float val)
{
	if(val <= config_vals[SYS_CONFIG_MAX_PWR] && val >= 0)
	{
		set_new_pwr(val);
	}
}

//Sets kV
static void set_kv_cmd(float val)
{
	if(val <= config_vals[SYS_CONFIG_MAX_KV] && val >= 0)
	{
		set_new_kv(val);
	}
}

//Sets filament
static void set_fil_cmd(float val)
{
	if(val <= config_vals[SYS_CONFIG_FIL_LIM] && val >= 0)
	{
		set_new_fil(val);
	}
}

//Unlock the configuration values for updating
static void update_config_lock(int32_t password)
{
	if(password == CONFIG_PASSWORD)
	{
		set_sys_bit(SYS_UNLOCKED_CONFIG);
	}
	else
	{
		clear_sys_bit(SYS_UNLOCKED_CONFIG);
	}
}

//Update the configuration values
static void update_config_val(float val, int32_t idx)
{
	if(!sys_stat_check(SYS_UNLOCKED_CONFIG))
	{
		//Do nothing if password is locked
		//return; TBD TODO restore
	}

	if(idx == SYS_CONFIG_RUN_PID)
	{
		if(val == 0)
		{
			clear_sys_bit(SYS_PID_ON);
		}
		else
		{
			set_sys_bit(SYS_PID_ON);
		}
	}

	//TBD TODO replace with bounds checked switch case below
	if(idx >= 0 && idx < NUM_SYS_CONFIG)
	{
		config_vals[idx] = val;
	}
/*
	switch(idx)
	{
		case SYS_CONFIG_MAX_PWR:
			break;
		case SYS_CONFIG_MAX_KV:
			break;
		case SYS_CONFIG_FIL_INIT:
			break;
		case SYS_CONFIG_FIL_LIM:
			break;
		default:
			break;
	}
*/
}

#ifdef CALIBRATION_MODE
static void calibration_start()
{
	if(sys_stat_check(SYS_HV_CTRL_EN) && sys_stat_check(SYS_GRID_CTRL_EN) && sys_stat_check(SYS_CAL_GRID_INT_EN))
	{
		set_sys_bit(SYS_EMISSION_ON);
		HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_SET);
	}
}

static void calibration_stop()
{
	if(sys_stat_check(SYS_HV_CTRL_EN))
	{
		clear_sys_bit(SYS_HV_CTRL_EN);
		HAL_GPIO_WritePin(GPIOE, IO_PFC_ALLOWED_Pin|IO_HV_ALLOWED_Pin, GPIO_PIN_RESET);
		HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_RESET);

		HAL_Delay(20);

		clear_sys_bit(SYS_EMISSION_ON);
		HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);

		HAL_Delay(20);

		set_sys_bit(SYS_HV_CTRL_EN);
		HAL_GPIO_WritePin(GPIOE, IO_PFC_ALLOWED_Pin|IO_HV_ALLOWED_Pin, GPIO_PIN_SET);
		HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_SET);
	}
	else
	{
		clear_sys_bit(SYS_EMISSION_ON);
		HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);
	}
}
#endif