#include "main.h"
#include "stdbool.h"
#include "stm32f3xx_hal.h"

#include "io.h"
#include "monitoring.h"
#include "timers.h"

bool start_hv_unlock = false;
bool start_grid_unlock = false;

bool wait_hv_unlock = false;
bool wait_grid_unlock = false;

bool io_updated = false;
uint32_t input_bits = 0;

input_debouncer_t debouncer[NUM_IO_INPUTS];


static void unlock_hv();
static void process_hv_unlock();
static void check_hv_unlock();
static void unlock_grid();
static void process_grid_unlock();
static void check_grid_unlock();

static void debounce_io();
static void check_debounce(int idx);


void setup_io()
{
	//Initialize debouncer setup
	for(int i = 0; i < NUM_IO_INPUTS; i++)
	{
		debouncer[i].level = 0;
		//Start a 1 ms debounce on all inputs to get initial state
		debouncer[i].ms_left = 1;
		debouncer[i].status_bit = (1 << i);
	}

	debouncer[IN_GRID_CLK_STAT].idr_bit = 0x0040;
	debouncer[IN_FIL_CLK_FAULT].idr_bit = 0x0080;
	debouncer[IN_GRID_INT].idr_bit = 0x0100;
	debouncer[IN_BEAM_CTRL].idr_bit = 0x0200;
	for(int i = IN_GRID_CLK_STAT; i <= IN_BEAM_CTRL; i++)
	{
		debouncer[i].GPIOx = GPIOB;
	}

	debouncer[IN_GRID_STAT].idr_bit = 0x2000;
	debouncer[IN_CAT_ARC].idr_bit = 0x4000;
	for(int i = IN_GRID_STAT; i <= IN_CAT_ARC; i++)
	{
		debouncer[i].GPIOx = GPIOC;
	}

	debouncer[IN_FAN_FAULT].idr_bit = 0x0004;
	debouncer[IN_PFC_OK].idr_bit = 0x0008;
	debouncer[IN_HV_INT].idr_bit = 0x0020;
	debouncer[IN_HV_STAT].idr_bit = 0x0080;
	debouncer[IN_OC_24_FAULT].idr_bit = 0x0100;
	debouncer[IN_MASTER_FAULT].idr_bit = 0x0200;
	debouncer[IN_OC_HV_FAULT].idr_bit = 0x0400;
	debouncer[IN_TEMP_1_FAULT].idr_bit = 0x0800;
	debouncer[IN_OC_CAT_FAULT].idr_bit = 0x1000;
	debouncer[IN_TEMP_3_FAULT].idr_bit = 0x2000;
	debouncer[IN_TEMP_2_FAULT].idr_bit = 0x4000;
	for(int i = IN_FAN_FAULT; i <= IN_TEMP_2_FAULT; i++)
	{
		debouncer[i].GPIOx = GPIOE;
	}

	HAL_GPIO_WritePin(GPIOB, IO_PS_OK_Pin, GPIO_PIN_SET);
}

void process_io()
{
	if(io_ms < 0)
	{
		io_ms = 1;

		debounce_io();

		if(io_updated)
		{
			report_io_state(input_bits);
			io_updated = false;
		}
	}

	process_hv_unlock();
	process_grid_unlock();
}

static void debounce_io()
{
	for(int i = 0; i < NUM_IO_INPUTS; i++)
	{
		if(debouncer[i].ms_left > 0)
		{
			debouncer[i].ms_left--;
		}
		else if(debouncer[i].ms_left == 0)
		{
			debouncer[i].ms_left--;
			check_debounce(i);
		}
		else if((debouncer[i].GPIOx->IDR & debouncer[i].idr_bit) ^ debouncer[i].level)
		{
			debouncer[i].ms_left = DEBOUNCE_MS;
		}
	}
}

static void check_debounce(int idx)
{
	if((debouncer[idx].GPIOx->IDR & debouncer[idx].idr_bit) ^ debouncer[idx].level)
	{
		io_updated = true;
		debouncer[idx].level ^= debouncer[idx].idr_bit;
		if(debouncer[idx].level == 0)
		{
			input_bits &= ~(debouncer[idx].status_bit);
		}
		else
		{
			input_bits |= debouncer[idx].status_bit;
		}
	}
}

void interlock_test(uint32_t param)
{
#ifndef CALIBRATION_MODE
	//TBD TODO placeholder values
	if(param == 123)
	{
		unlock_hv();
	}
	else if (param == 456)
	{
		unlock_grid();
	}
#else
    // Not active during calibration
	return;
#endif
}

void lock_hv()
{
	start_hv_unlock = false;
	wait_hv_unlock = false;
	clear_sys_bit(SYS_HV_CTRL_EN);
	clear_sys_bit(SYS_EMISSION_ON);
	HAL_GPIO_WritePin(GPIOE, IO_PFC_ALLOWED_Pin|IO_HV_ALLOWED_Pin, GPIO_PIN_RESET);
	HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_RESET);
	HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);
	set_new_kv(0);
}

void lock_grid()
{
	start_grid_unlock = false;
	wait_grid_unlock = false;
	clear_sys_bit(SYS_GRID_CTRL_EN);
	clear_sys_bit(SYS_EMISSION_ON);
	HAL_GPIO_WritePin(GPIOE, IO_BEAM_ALLOWED_Pin, GPIO_PIN_RESET);
	HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_RESET);
	//disable_grid_clock();
}

static void unlock_hv()
{
	//If we are already trying to unlock the grid or HV, lock down both
	if(wait_hv_unlock || wait_grid_unlock)
	{
		//TBD TODO fault?
		lock_hv();
		lock_grid();
	}
	else
	{
		start_hv_unlock = true;
	}
}

static void unlock_grid()
{
	if(wait_hv_unlock || wait_grid_unlock)
	{
		//TBD TODO fault?
		lock_hv();
		lock_grid();
	}
	else
	{
		start_grid_unlock = true;
	}
}

static void process_hv_unlock()
{
#ifndef CALIBRATION_MODE
	if(start_hv_unlock)
	{
		//Re-lock HV just in case
		lock_hv();

		//If interlock is disengaged, we can now wait for engage
		if(debouncer[IN_HV_INT].level == 0)
		{
			wait_hv_unlock = true;
			lock_timer_ms = LOCK_TIMER_PERIOD;
		}
	}
	else if(wait_hv_unlock)
	{
		check_hv_unlock();
	}
#else
    // Not active during calibration
	return;
#endif
}

static void process_grid_unlock()
{
#ifndef CALIBRATION_MODE
	if(start_grid_unlock)
	{
		//Re-lock grid just in case
		lock_grid();

		//If interlock is disengaged, we can now wait for engage
		if(debouncer[IN_GRID_INT].level == 0)
		{
			wait_grid_unlock = true;
			lock_timer_ms = LOCK_TIMER_PERIOD;
		}
	}
	else if(wait_grid_unlock)
	{
		check_grid_unlock();
	}
#else
    // Not active during calibration
	return;
#endif
}

static void check_hv_unlock()
{
	if(debouncer[IN_HV_INT].level != 0)
	{
		wait_hv_unlock = false;
		if(lock_timer_ms > LOCK_TIMER_MIN_MS && lock_timer_ms < LOCK_TIMER_MAX_MS)
		{
			set_sys_bit(SYS_HV_CTRL_EN);
			HAL_GPIO_WritePin(GPIOE, IO_PFC_ALLOWED_Pin|IO_HV_ALLOWED_Pin, GPIO_PIN_SET);
			HAL_GPIO_WritePin(GPIOD, IO_SEND_READY_Pin, GPIO_PIN_SET);
		}
		else
		{
			lock_hv();
		}
	}
}

static void check_grid_unlock()
{
	if(debouncer[IN_GRID_INT].level != 0)
	{
		wait_grid_unlock = false;
		if(lock_timer_ms > LOCK_TIMER_MIN_MS && lock_timer_ms < LOCK_TIMER_MAX_MS)
		{
			set_sys_bit(SYS_GRID_CTRL_EN);
		}
		else
		{
			lock_grid();
		}
	}
}
