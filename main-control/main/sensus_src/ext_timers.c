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
#include "ext_timers.h"

volatile bool timer_check_ready = true;
volatile bool timer_bus_stuck = false;
volatile bool timer_write_cycle = true;
volatile uint32_t timer_addr = PRIMARY_TIMER_ADDR;

uint8_t primary_timer_rx_buf[TIMER_RX_SIZE] = {0};
uint8_t secondary_timer_rx_buf[TIMER_RX_SIZE] = {0};
uint8_t timer_tx_buf[TIMER_TX_SIZE] = {0};
volatile uint32_t timer_tx_idx = 0;
volatile uint32_t timer_rx_idx = 0;

volatile uint32_t timer_1_fault_count = 0;
volatile uint32_t timer_2_fault_count = 0;

uint32_t timer_comm_flags = 0;
uint32_t new_timer_val = 0;
uint32_t ext_timer_tick_start = 0;

static struct timer_task VTIMER_ext_timer_check;

static void parse_timer_values(bool primary);

static void timer_rx();
static void save_timer_rx(uint8_t val);
static void timer_tx();
static void go_to_next_timer();
static void timer_transmission_complete();
static void report_timer_nack();

static void ext_timers_comm_timeout_check(const struct timer_task *const timer_task);

void init_ext_timers()
{
	//Initialize I2C peripheral
	i2c_m_sync_enable(&TIMERS_I2C);
	
	//Allows interrupt on NACK
	hri_twihs_set_IMR_NACK_bit(TIMERS_I2C.device.hw);
	
	//Enable interrupt
	NVIC_EnableIRQ(TWIHS0_IRQn);
	
	//Initialize vtimer task to check external timer bus
	VTIMER_ext_timer_check.interval = TIMER_COMM_TIMEOUT_MS;
	VTIMER_ext_timer_check.cb = ext_timers_comm_timeout_check;
	VTIMER_ext_timer_check.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_ext_timer_check);
	
	//Reset rx buffers
	memset(primary_timer_rx_buf, 0, TIMER_RX_SIZE);
	primary_timer_rx_buf[TIMER_RX_CHECK] = 0xFF;	
	memset(secondary_timer_rx_buf, 0, TIMER_RX_SIZE);
	secondary_timer_rx_buf[TIMER_RX_CHECK] = 0xFF;
	
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
	
	//Start next write
	hri_twihs_write_MMR_reg(TIMERS_I2C.device.hw, TWIHS_MMR_DADR(timer_addr));
	hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXRDY);
	timer_write_cycle = true;
	timer_check_ready = false;
}

static void parse_timer_values(bool primary)
{
	uint8_t *time_buf;	
	uint8_t checksum = 0;
	
	//Parse values based on if first or second timer is being read
	if(primary)
	{
		time_buf = primary_timer_rx_buf;
	}
	else
	{
		time_buf = secondary_timer_rx_buf;
	}
	
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

//This function called within interrupt, keep short
static void timer_rx()
{
	//Read and save value
	uint32_t read_val = hri_twihs_read_RHR_reg(TIMERS_I2C.device.hw);
	save_timer_rx((uint8_t)read_val);
	
	//Increment idx
	timer_rx_idx++;
	
	//If next byte is last, set stop condition
	if(timer_rx_idx == TIMER_RX_SIZE-1)
	{
		hri_twihs_write_CR_reg(TIMERS_I2C.device.hw, TWIHS_CR_STOP);
	}
	//If byte is last, begin transmission end
	else if(timer_rx_idx >= TIMER_RX_SIZE)
	{
		timer_rx_idx = 0;
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_RXRDY);
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXCOMP);
	}
}

//This function called within interrupt, keep short
static void save_timer_rx(uint8_t val)
{
	//Bounds check
	if(timer_rx_idx >= TIMER_RX_SIZE)
	{
		return;
	}
	
	//Write to buffer depending on which timer is being read
	if(timer_addr == PRIMARY_TIMER_ADDR)
	{
		primary_timer_rx_buf[timer_rx_idx] = val;
	}
	else
	{
		secondary_timer_rx_buf[timer_rx_idx] = val;
	}
}

//This function called within interrupt, keep short
static void timer_tx()
{
	//If bytes are still left to transmit, send next byte out
	if(timer_tx_idx < TIMER_TX_SIZE)
	{
		hri_twihs_write_THR_reg(TIMERS_I2C.device.hw, timer_tx_buf[timer_tx_idx]);
		timer_tx_idx++;
	}
	//Otherwise begin end of transmission
	else
	{
		timer_tx_idx = 0;
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_TXRDY);
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXCOMP);
		hri_twihs_write_CR_reg(TIMERS_I2C.device.hw, TWIHS_CR_STOP);	
	}
}

//This function called within interrupt, keep short
static void timer_transmission_complete()
{
	//If we are done with a write, proceed to read
	if(timer_write_cycle)
	{
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_RXRDY);
		hri_twihs_write_MMR_reg(TIMERS_I2C.device.hw, TWIHS_MMR_DADR(timer_addr) | TWIHS_MMR_MREAD);
		hri_twihs_write_CR_reg(TIMERS_I2C.device.hw, TWIHS_CR_START);
		timer_write_cycle = false;
	}
	//Otherwise if we just read, proceed to next timer
	else
	{
		go_to_next_timer();
	}
}

//This function called within interrupt, keep short
static void go_to_next_timer()
{
	//Reset indexes
	timer_tx_idx = 0;
	timer_rx_idx = 0;
	timer_bus_stuck = false;
	
	if(timer_addr == PRIMARY_TIMER_ADDR)
	{
		//If we are moving to secondary timer, just automatically continue next write
		timer_addr = SECONDARY_TIMER_ADDR;
		hri_twihs_write_MMR_reg(TIMERS_I2C.device.hw, TWIHS_MMR_DADR(timer_addr));
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXRDY);
		timer_write_cycle = true;
	}
	else
	{
		timer_addr = PRIMARY_TIMER_ADDR;
		timer_check_ready = true;
	}
}

static void report_timer_nack()
{
	//Report fault
	if(timer_addr == PRIMARY_TIMER_ADDR)
	{
		report_typed_fault(FAULT_TIMER_COMM, "Primary timer returned NACK.");
	}
	else
	{
		report_typed_fault(FAULT_TIMER_COMM, "Secondary timer returned NACK.");
	}
}

//Interrupt handler, keep short
void TWIHS0_Handler()
{
	uint32_t sr = hri_twihs_read_SR_reg(TIMERS_I2C.device.hw) & hri_twihs_read_IMR_reg(TIMERS_I2C.device.hw);
	
	//Check for NACK error
	if(sr & TWIHS_SR_NACK)	
	{
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		
		//Report fault
		report_timer_nack();
		
		//Move to next timer
		go_to_next_timer();
	}
	//Check for transmission completion
	else if (sr & TWIHS_SR_TXCOMP)
	{
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		timer_transmission_complete();
	}
	//Check for TX transmit ready
	else if(sr & TWIHS_SR_TXRDY)
	{
		timer_tx();
	}
	//Check for RX received
	else if(sr & TWIHS_SR_RXRDY)
	{
		timer_rx();
	}
}
