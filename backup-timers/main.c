#include <atmel_start.h>
#include <string.h>
#include "gryphon_timers.h"

uint8_t rx_buf[RX_MAX_COUNT] = {0};
volatile uint32_t rx_idx = 0;
volatile bool rx_recv = false;
volatile bool flush_rx = false;

uint8_t tx_buf[TX_MAX_COUNT] = {0};
volatile uint32_t tx_idx = 0;
volatile uint8_t timer_state = TIMER_STATE_CLEARED;
volatile uint32_t last_time_val = 0;

uint16_t i2c_addr = 0xb4;
struct io_descriptor *i2c_io;

static void init_io_pins();
static void process_rx();
static void set_timer();
static void stop_timer();
static void reset_timer();
static void update_tx();


void TC1_Handler()
{
	//Clear overflow and disable timer
	hri_tc_clear_interrupt_OVF_bit(TC1);
	hri_tc_write_CTRLA_ENABLE_bit(TC1, 0);
	
	if(timer_state == TIMER_STATE_RUNNING)
	{
		//Trigger fault IO
		gpio_set_pin_level(IO_FAULT_n, false);
		
		timer_state = TIMER_STATE_ELAPSED;
	}
}

void start_triggered()
{
	if(timer_state == TIMER_STATE_PAUSED)
	{
		gpio_set_pin_level(IO_LED, false);
		
		//Enable timer
		hri_tc_write_CTRLA_ENABLE_bit(TC1, 1 << TC_CTRLA_ENABLE_Pos);
		
		//Change timer state to running
		timer_state = TIMER_STATE_RUNNING;
	}
}

static void init_io_pins()
{
	//Start pin already initialized by Atmel START
	//Just assign the interrupt callback
	ext_irq_register(PIN_PA05, start_triggered);
	
	//Set address pin as input
	gpio_set_pin_direction(IO_ADDR, GPIO_DIRECTION_IN);
	
	//Set fault pin as output, default high
	gpio_set_pin_direction(IO_FAULT_n, GPIO_DIRECTION_OUT);
	gpio_set_pin_level(IO_FAULT_n, true);
	
	//Set LED pin as output, default low
	gpio_set_pin_direction(IO_LED, GPIO_DIRECTION_OUT);
	gpio_set_pin_level(IO_LED, true);
	
	//Modify I2C address if address pin is set
	if(gpio_get_pin_level(IO_ADDR))
	{
		i2c_addr += 1;
	}
}

void i2c_rx()
{
	tx_idx = 0;
	if(rx_idx < RX_MAX_COUNT)
	{
		io_read(i2c_io, rx_buf+rx_idx, 1);
		rx_idx++;
		if(rx_idx >= RX_MAX_COUNT)
		{
			rx_recv = true;
		}
	}
}

void i2c_tx()
{
	//Reset RX as we are now transmitting
	rx_idx = 0;
	i2c_s_async_flush_rx_buffer(&I2C_0);
	
	//Write values
	io_write(i2c_io, tx_buf+tx_idx, 1);
	
	tx_idx++;
	
	if(tx_idx >= TX_MAX_COUNT)
	{
		tx_idx = 0;
	}
}

#ifndef UNIT_TEST
int main(void)
{
	/* Initializes MCU, drivers and middleware */
	atmel_start_init();
	init_io_pins();
	
	tx_buf[TX_CHECKSUM] = 0xFF;
	
	//Set up i2c
	i2c_s_async_get_io_descriptor(&I2C_0, &i2c_io);
	i2c_s_async_register_callback(&I2C_0, I2C_S_RX_COMPLETE, i2c_rx);
	i2c_s_async_register_callback(&I2C_0, I2C_S_TX_PENDING, i2c_tx);
	i2c_s_async_set_addr(&I2C_0, i2c_addr);
	i2c_s_async_enable(&I2C_0);
	
	//Enable timer interrupt
	NVIC_EnableIRQ(TC1_IRQn);
	
	while (1) {
		if(rx_recv)
		{
			rx_recv = false;
			
			//Prepare response for I2C first
			update_tx();
			
			//Process received data
			process_rx();
		}
	}
}
#endif

static void update_tx()
{
	//Get current time status and copy to output buffer
	uint32_t current_time = hri_tccount32_read_COUNT_reg(TC1);
	memcpy(tx_buf+TX_TIME_VAL_0, &current_time, sizeof(uint32_t));
	tx_buf[TX_STATE] = timer_state;
	
	//Calculate checksum and copy to output buffer
	tx_buf[TX_CHECKSUM] = 0xFF;
	for(int i = 0; i < TX_CHECKSUM; i++)
	{
		tx_buf[TX_CHECKSUM] -= tx_buf[i];
	}
}

static void process_rx()
{
	uint32_t msg = rx_buf[RX_MSG_TYPE];
	
	uint8_t rx_check = 0;
	
	//Calculate and verify checksum
	for(int i = 0; i < RX_MAX_COUNT; i++)
	{
		rx_check += rx_buf[i];
	}
	if(rx_check != 0xFF)
	{
		msg = MSG_UNKNOWN;
	}
	/*
	else
	{
		gpio_toggle_pin_level(IO_LED);
	}*/
	
	//Based on message, perform task
	switch(msg)
	{
		case MSG_SET_TIMER:
			set_timer();
			break;
		case MSG_STOP_TIMER:
			stop_timer();
			break;
		case MSG_CLEAR_TIMER:
			reset_timer();
			break;
		//Do nothing, just send back timer value
		case MSG_READ_TIMER:
		case MSG_UNKNOWN:
		default:
			break;
	}
}

static void set_timer()
{
	//Only set timer if we are paused or cleared
	if(timer_state == TIMER_STATE_PAUSED || timer_state == TIMER_STATE_CLEARED)
	{	
		//Set state to paused
		timer_state = TIMER_STATE_PAUSED;
		
		//Get time value from message
		uint32_t time_val = 0;
		memcpy(&time_val, rx_buf+RX_PARAM_0, sizeof(uint32_t));
		
		//Set TC value
		hri_tccount32_write_COUNT_reg(TC1, time_val);
	}
}

static void stop_timer()
{
	//Only allow a pause if start is not asserted
	if(gpio_get_pin_level(IO_START_n))
	{
		last_time_val = hri_tccount32_read_COUNT_reg(TC1);
		
		gpio_set_pin_level(IO_LED, true);
		
		//Stop timer and set state to paused
		timer_state = TIMER_STATE_PAUSED;
		hri_tc_write_CTRLA_ENABLE_bit(TC1, 0);
		
		hri_tccount32_write_COUNT_reg(TC1, last_time_val);
	}
}

static void reset_timer()
{
	//Only clear timer if we are not running
	if(timer_state != TIMER_STATE_RUNNING)
	{
		//Set timer to max count to not accidentally trigger interrupt at 0
		timer_state = TIMER_STATE_CLEARED;
		hri_tccount32_write_COUNT_reg(TC1, 0xFFFFFFFF);
		
		//Clear fault IO
		gpio_set_pin_level(IO_FAULT_n, true);
	}
}
