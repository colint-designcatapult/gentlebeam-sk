/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: External DAC
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <math.h>
#include <string.h>
#include "faults.h"
#include "system_parameters.h"
#include "ext_dac.h"

float new_coil_dac_voltage[NUM_COIL_DAC_CH] = {-1};
float new_fan_dac_voltage[NUM_FAN_DAC_CH] = {-1};
uint8_t dac_tx_buffer[NUM_DAC_CMD_BYTES] = {0};
volatile bool dac_check_ready = false;

struct io_descriptor *dac_io;
static struct timer_task VTIMER_dac_spi_start;
static struct timer_task VTIMER_dac_spi_end;

static void write_dac_channel(uint32_t ch, float v_out);
static void write_dac(uint8_t cmd, uint16_t param);

static void dac_rx_complete_cb(const struct spi_m_async_descriptor *const io_descr);
static void start_dac_rx(const struct timer_task *const timer_task);
static void end_dac_rx(const struct timer_task *const timer_task);

void init_ext_dac()
{
	//Initialize IO	
	gpio_set_pin_level(IO_COIL_DAC_CLRn, true);
	gpio_set_pin_level(IO_COIL_DAC_LDACn, true);
	gpio_set_pin_level(IO_FAN_DAC_CLRn, true);
	gpio_set_pin_level(IO_FAN_DAC_LDACn, true);
	
	//Initialize SPI module
	spi_m_async_get_io_descriptor(&DAC_SPI, &dac_io);
	spi_m_async_register_callback(&DAC_SPI, SPI_M_ASYNC_CB_XFER, (FUNC_PTR)dac_rx_complete_cb);
	spi_m_async_enable(&DAC_SPI);
	
	//Set up timers for start and end delays
	VTIMER_dac_spi_start.interval = 1;
	VTIMER_dac_spi_start.cb       = start_dac_rx;
	VTIMER_dac_spi_start.mode     = TIMER_TASK_ONE_SHOT;
	
	VTIMER_dac_spi_end.interval	= 2;
	VTIMER_dac_spi_end.cb       = end_dac_rx;
	VTIMER_dac_spi_end.mode     = TIMER_TASK_ONE_SHOT;
	
	//Write the setup config to both DACs
	gpio_set_pin_level(IO_COIL_DAC_CSn, false);
	gpio_set_pin_level(IO_FAN_DAC_CSn, false);
	dac_tx_buffer[0] = DAC_CONFIG_VAL_0;
	dac_tx_buffer[1] = DAC_CONFIG_VAL_1;
	
	//Start spi write timer
	dac_check_ready = false;
	timer_add_task(&VTIMER, &VTIMER_dac_spi_start);
	
	//Wait for setup config write to finish
	while(!dac_check_ready)
	{
		//Do nothing, just wait for async process to complete
	}
	
	//Write reference config to both DACs
	gpio_set_pin_level(IO_COIL_DAC_CSn, false);
	gpio_set_pin_level(IO_FAN_DAC_CSn, false);
	dac_tx_buffer[0] = DAC_REF_CONFIG_VAL;
	
	//Start spi write timer
	dac_check_ready = false;
	timer_add_task(&VTIMER, &VTIMER_dac_spi_start);
}

//Function called in main loop, values written and checked here
void process_ext_dac()
{
	//Wait until DAC SPI bus is free
	if(!dac_check_ready)
	{
		return;	
	}
	
	//Check to see if new coil values are available
	for(int i = 0; i < NUM_COIL_DAC_CH; i++)
	{
		if(new_coil_dac_voltage[i] >= 0)
		{
			//If new value available, write out value and return
			//since bus is now busy
			gpio_set_pin_level(IO_COIL_DAC_CSn, false);
			write_dac_channel(i, new_coil_dac_voltage[i]);
			new_coil_dac_voltage[i] = -1;
			return;
		}
	}
	
	//Check to see if new fan values are available
	for(int i = 0; i < NUM_FAN_DAC_CH; i++)
	{
		if(new_fan_dac_voltage[i] >= 0)
		{
			//If new value available, write out value and return
			//since bus is now busy
			gpio_set_pin_level(IO_FAN_DAC_CSn, false);
			write_dac_channel(i, new_fan_dac_voltage[i]);
			new_fan_dac_voltage[i] = -1;
			return;
		}
	}
}



static void write_dac_channel(uint32_t ch, float v_out)
{
	if(v_out > MAX_DAC_VOLTAGE)
	{
		v_out = MAX_DAC_VOLTAGE;
	}
	
	//Calculate 12-bit dac value from 
	uint16_t dac_out = (uint16_t)(DAC_F_TO_12_FACTOR*v_out);
	//Bitshift adjustment for formatting
	dac_out <<= 4;
	
	//Set command based on channel
	uint8_t cmd = (uint8_t)(ch & 0x03) | DAC_CODE_LOAD_CMD;
	
	//Setup tx buffer
	write_dac(cmd, dac_out);
}

static void write_dac(uint8_t cmd, uint16_t param)
{
	//Copy data to tx buffer
	dac_tx_buffer[0] = cmd;
	dac_tx_buffer[1] = (uint8_t)(param >> 8);
	dac_tx_buffer[2] = (uint8_t)(param & 0xFF);
	
	//Indicate bus is buys and start timer for comm start
	//Timer used to give delay for CS line
	dac_check_ready = false;
	timer_add_task(&VTIMER, &VTIMER_dac_spi_start);
}

void set_coil_voltage(uint32_t coil_ch, float voltage)
{
	//Do nothing if we are given invalid values
	if(coil_ch >= NUM_COIL_DAC_CH || isnan(voltage))
	{
		return;
	}
	
	//Check to see if given voltage value is negative
	bool negative = voltage < 0;
	if(negative)
	{
		voltage *= -1;
	}
	
	//Flip direction pin of X coil to appropriate state
	if(coil_ch == X_COIL_DAC_CH)
	{
		if(negative)
		{
			gpio_set_pin_level(IO_COIL_X_DIRn, false);
		}
		else
		{
			gpio_set_pin_level(IO_COIL_X_DIRn, true);
		}
	}
	//Flip direction pin of Y coil to appropriate state
	else if(coil_ch == Y_COIL_DAC_CH)
	{
		if(negative)
		{
			gpio_set_pin_level(IO_COIL_Y_DIRn, false);
		}
		else
		{
			gpio_set_pin_level(IO_COIL_Y_DIRn, true);
		}
	}
	
	new_coil_dac_voltage[coil_ch] = voltage;
}

void set_fan_voltage(uint32_t fan_ch, float voltage)
{
	//Do nothing if DAC channel is invalid
	if(fan_ch >= NUM_FAN_DAC_CH)
	{
		return;
	}
	
	//Reduce voltages past maximum fan controller drive
	if(voltage >= MAX_FAN_CTRL_VOLTAGE)
	{
		new_fan_dac_voltage[fan_ch] = MAX_FAN_CTRL_VOLTAGE;
	}
	else
	{
		new_fan_dac_voltage[fan_ch] = voltage;	
	}
}

//Callback function, keep short
static void start_dac_rx(const struct timer_task *const timer_task)
{
	//Disable LDAC lines while we write data
	gpio_set_pin_level(IO_FAN_DAC_LDACn, true);
	gpio_set_pin_level(IO_COIL_DAC_LDACn, true);
	
	//Start data write
	io_write(dac_io, dac_tx_buffer, NUM_DAC_CMD_BYTES);
}

//Callback function, keep short
static void end_dac_rx(const struct timer_task *const timer_task)
{
	dac_check_ready = true;
	
	//Disable LDAC lines after we have written data
	gpio_set_pin_level(IO_FAN_DAC_LDACn, true);
	gpio_set_pin_level(IO_COIL_DAC_LDACn, true);
}

//Callback function, keep short
static void dac_rx_complete_cb(const struct spi_m_async_descriptor *const io_descr)
{
	//Clear CS lines
	gpio_set_pin_level(IO_FAN_DAC_CSn, true);
	gpio_set_pin_level(IO_COIL_DAC_CSn, true);
	
	//Enable LDAC lines to latch data
	gpio_set_pin_level(IO_FAN_DAC_LDACn, false);
	gpio_set_pin_level(IO_COIL_DAC_LDACn, false);
	
	//Start timer to free up SPI bus again (CS line reset)
	timer_add_task(&VTIMER, &VTIMER_dac_spi_end);
}
