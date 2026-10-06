/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: External Timers
*	Author: Carlton Chow
*	Description:


Note to self, timers are started synchronously with main loop every cycle
because write values can change and need to be checked before being written
In comparison, ADC values are continuously read and just copied to sync buffer
since the write command values to the ADCs are static
*/

#include <atmel_start.h>
#include <string.h>
#include "faults.h"
#include "state_machine.h"
#include "system_parameters.h"
#include "ext_timers_i2c.h"

volatile bool timer_check_ready = true;
volatile bool timer_bus_stuck = false;



uint32_t timer_comm_flags = 0;
uint32_t new_timer_val = 0;
uint32_t ext_timer_tick_start = 0;

uint8_t timer_tx_buf[TIMER_TX_SIZE] = {0};

static struct timer_task VTIMER_ext_timer_check;

static void parse_timer_values(bool primary);


static void ext_timers_comm_timeout_check(const struct timer_task *const timer_task);

void init_ext_timers()
{
	//Initialize I2C transport
	init_ext_timers_i2c();
	
	//Initialize vtimer task to check external timer bus
	VTIMER_ext_timer_check.interval = TIMER_COMM_TIMEOUT_MS;
	VTIMER_ext_timer_check.cb = ext_timers_comm_timeout_check;
	VTIMER_ext_timer_check.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_ext_timer_check);
	
	//Set timer check ready initially to start up timer writes
	timer_check_ready = true;
	timer_comm_flags = 0;
}

//Callback function, keep short
static void ext_timers_comm_timeout_check(const struct timer_task *const timer_task)
{
	//Check to see if timer bus is stuck (i.e. slave hold)
	if(timer_bus_stuck)
	{
		report_typed_fault1(FAULT_TIMER_COMM, "No timer response was received within %u ms.", MAKE_ARG(TIMER_COMM_TIMEOUT_MS));
	}
	timer_bus_stuck = true;
}

//Function called in main loop, values read/written and checked here
void process_ext_timers()
{
	//Make sure timers are ready to be checked
	if(!timer_check_ready) return;
	
	//Get reported values of timers
	parse_timer_values(true);	//check primary timer values
	parse_timer_values(false);	//check secondary timer values
	
	//Check for new timer commands
	//Note: order must be pause > clear > set
	if(timer_comm_flags & TIMER_CMD_PAUSE)
	{
		//Clear pause flag and send out pause command
		timer_comm_flags &= ~(TIMER_CMD_PAUSE);
		timer_tx_buf[TIMER_TX_CMD] = TIMER_CMD_PAUSE;
	}
	else if(timer_comm_flags & TIMER_CMD_CLEAR)
	{
		//Clear clear timer flag and send out command
		timer_comm_flags &= ~(TIMER_CMD_CLEAR);
		timer_tx_buf[TIMER_TX_CMD] = TIMER_CMD_CLEAR;
	}
	else if(timer_comm_flags & TIMER_CMD_SET_TIME)
	{
		//Clear set timer flag and send out command
		timer_comm_flags &= ~(TIMER_CMD_SET_TIME);
		memcpy(timer_tx_buf+TIMER_TX_TIME_0, &new_timer_val, sizeof(uint32_t));
		timer_tx_buf[TIMER_TX_CMD] = TIMER_CMD_SET_TIME;
		ext_timer_tick_start = new_timer_val;
	}
	else
	{
		timer_tx_buf[TIMER_TX_CMD] = TIMER_CMD_READ;
	}
	
	//Update timer check value
	uint8_t check_val = 0xFF;
	for(int i = 0; i < TIMER_TX_CHECK; i++)
	{
		check_val -= timer_tx_buf[i];
	}
	timer_tx_buf[TIMER_TX_CHECK] = check_val;
	
	//Start next I2C transfer
	timer_check_ready = false;
	ext_timers_i2c_start_transfer(timer_tx_buf);
}

static void parse_timer_values(bool primary)
{
	uint8_t *time_buf = ext_timers_i2c_get_rx_buffer(primary);
	uint8_t checksum = 0;
	
	//Check that packet sum equals 0xFF
	for(int i = 0; i < TIMER_RX_SIZE; i++)
	{
		checksum += time_buf[i];
	}

	if(checksum != 0xFF)
	{
		if(primary)
		{
			report_typed_fault2(FAULT_TIMER_COMM, "Primary timer checksum was %u; expected %u.", MAKE_ARG((uint32_t)checksum), MAKE_ARG((uint32_t)time_buf[TIMER_RX_CHECK]));
		}
		else
		{
			report_typed_fault2(FAULT_TIMER_COMM, "Secondary timer checksum was %u; expected %u.", MAKE_ARG((uint32_t)checksum), MAKE_ARG((uint32_t)time_buf[TIMER_RX_CHECK]));
		}
	}
	else
	{
		//Get timer state and value
		uint8_t timer_state = time_buf[TIMER_RX_STATE];
		uint32_t *timer_raw_val = (uint32_t *)(time_buf+TIMER_RX_TIME_0);
		report_ext_timer_values((uint32_t)timer_state, *timer_raw_val, primary);
	}
}

void set_new_timer_val(uint32_t ticks)
{
	new_timer_val = ticks;
	timer_comm_flags |= TIMER_CMD_SET_TIME;
}

void set_new_timer_value(float seconds)
{
	//Check for valid time
	if(seconds > MAX_TIMER_SECONDS|| seconds <= 0)
	{
		new_timer_val = 0;
	}
	else
	{
		//Convert timer minutes to ticks
		float ticks = seconds * TICKS_PER_SECOND;
		
		//Add small overhead value since external timers should be
		//set to expire JUST after the internal main timer
		//External timers stopping emission is a fault and to prevent false positives, small overhead is added
		ticks += (TICKS_PER_SECOND * TIMER_COUNT_OVERHEAD);
		
		//Save new timer value
		new_timer_val = (uint32_t)ticks;
	}
	
	//Set flag to set new time
	timer_comm_flags |= TIMER_CMD_SET_TIME;
}

void start_ext_timers()
{
	gpio_set_pin_level(IO_TIMERS_STARTn, false);
}

void pause_ext_timers()
{
	gpio_set_pin_level(IO_TIMERS_STARTn, true);
	timer_comm_flags |= TIMER_CMD_PAUSE;
}

void clear_ext_timers()
{
	timer_comm_flags |= TIMER_CMD_CLEAR;
}

void ext_timers_i2c_transfer_complete()
{
	timer_bus_stuck = false;
	timer_check_ready = true;
}

