/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Peltier cooler
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#include "faults.h"
#include "system_parameters.h"
#include "peltier_cooler.h"
#if defined(CALIBRATION_MODE)
#include "sys_config_defaults.h"
#endif

struct io_descriptor *plt_io;
static struct timer_task VTIMER_plt_check;

volatile bool plt_check_ready = false;

//Disable alarm 2 low {'*', '2', '8', 'f', 'f', 'e', 'b', 'f', 'd', '\r'}
//Disable alarm 2 high {'*', '2', '9', '0', '0', 'c', '8', '6', '6', '\r'}
//Disable alarm 2 {'*', '2', 'a', '0', '0', '0', '0', '5', '3', '\r'}
//Enable output {'*', '3', '0', '0', '0', '0', '1', '2', '4', '\r'}

uint8_t plt_set_temp[] = {'*', '1', 'c', '0', '0', '0', '0', '0', '0', '\r'};
uint8_t plt_setup_tx[] = {'*', '1', 'c', '0', '0', '9', '6', '6', '3', '\r', '*', '2', 'a', '0', '0', '0', '0', '5', '3', '\r', '*', '3', '0', '0', '0', '0', '0', '2', '3', '\r'};
uint8_t plt_off[] = {'*', '3', '0', '0', '0', '0', '0', '2', '3', '\r'};
uint8_t plt_on[] = {'*', '3', '0', '0', '0', '0', '1', '2', '4', '\r'};

uint8_t query_plt_temp_tx[] = {'*', '0', '1', '0', '0', '0', '0', '2', '1', '\r'};
//uint8_t query_plt_temp_tx[] = {'*', '5', '0', '0', '0', '0', '0', '2', '5', '\r'};
//uint8_t query_plt_temp_tx[] = {'*', '6', '4', '0', '0', '0', '0', '2', 'a', '\r'};
//uint8_t query_plt_temp_tx[] = {'*', '0', '3', '0', '0', '0', '0', '2', '3', '\r'};
uint8_t plt_rx_buf[PLT_RX_COUNT] = {0};
uint32_t plt_rx_byte_count = 0;

uint32_t plt_no_response_count = 0;
uint32_t plt_invalid_response_count = 0;

volatile uint32_t plt_cmd = 0;

static void read_previous_plt_temp();
static void check_plt_response();
static bool is_char_hex(uint8_t val);

static void plt_check(const struct timer_task *const timer_task);
static void plt_uart_rx_cb(const struct usart_async_descriptor *const io_descr);

void init_plt_cooler()
{
	//Register RX callbacks
	usart_async_register_callback(&PLT_UART, USART_ASYNC_RXC_CB, plt_uart_rx_cb);
	//No need to register TX callback, no processing needed
	//No need to register error callback with UART. Timeout is sufficient
	
	//Enable peripheral
	usart_async_get_io_descriptor(&PLT_UART, &plt_io);
	usart_async_enable(&PLT_UART);
	
	io_write(plt_io, plt_setup_tx, sizeof(plt_setup_tx));
	
	//Initialize vtimer task to check PLT
	VTIMER_plt_check.interval = PLT_CHECK_MS;
	VTIMER_plt_check.cb = plt_check;
	VTIMER_plt_check.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_plt_check);
}

//Callback function, keep short
static void plt_check(const struct timer_task *const timer_task)
{
	plt_check_ready = true;
}

void set_plt_temperature(uint32_t temperature)
{
#if defined(CALIBRATION_MODE)
	if(temperature < DEFAULT_PLT_MIN_TEMP || temperature > DEFAULT_PLT_MAX_TEMP)
#else
	//TODO: never used, check if needed
	if(temperature > 40)
#endif
	{
		return;
	}
	
	//Convert temperature output
	uint32_t output_val = temperature*10;
	sprintf(plt_set_temp+3, "%04lx", output_val);
	
	//Calculate checksum
	uint8_t check_val = 0;
	for(int i = 1; i < 7; i++)
	{
		check_val += plt_set_temp[i];
	}
	sprintf(plt_set_temp+7, "%02X", check_val);
	plt_set_temp[9] = '\r';
	
	//Queue set temp command
	plt_cmd = PLT_CMD_SET_TEMP;
}

void enable_plt(bool en)
{
	if(en)
	{
		plt_cmd = PLT_CMD_ON;
	}
	else
	{
		plt_cmd = PLT_CMD_OFF;
	}
}

//Function called in main loop, values read/written and checked here
void process_plt()
{
	//Make sure cooler is ready to be checked
	if(!plt_check_ready) return;
	plt_check_ready = false;
	
	switch(plt_cmd)
	{
		case PLT_CMD_OFF:
			io_write(plt_io, plt_off, sizeof(plt_off));
			break;
		case PLT_CMD_ON:
			io_write(plt_io, plt_on, sizeof(plt_on));
			break;
		case PLT_CMD_SET_TEMP:
			io_write(plt_io, plt_set_temp, sizeof(plt_set_temp));
			break;
		case PLT_CMD_TEMP_QUERY:
		default:
			io_write(plt_io, query_plt_temp_tx, sizeof(query_plt_temp_tx));
			break;
	}
	plt_cmd = PLT_CMD_TEMP_QUERY;
	
	//Read back the previous temperature reading
	//This will fail the first time, but we retry before reporting a fault
	read_previous_plt_temp();
}

static void read_previous_plt_temp()
{
	//If fewer than expected bytes are reported, flush buffer
	if(plt_rx_byte_count < PLT_RX_COUNT)
	{
		usart_async_flush_rx_buffer(&PLT_UART);
		plt_no_response_count++;
		
		//If we receive fewer than expected bytes enough times, report a fault
		if(plt_no_response_count >= PLT_MAX_NO_RESPONSE)
		{
			plt_no_response_count = 0;
			report_fault(FAULT_PELTIER_COMM, PLT_COMM_FAULT_TIMEOUT, PLT_MAX_NO_RESPONSE, 0, -1);
		}
	}
	//Otherwise if we have enough bytes, read and process response
	else
	{
		plt_no_response_count = 0;
		plt_rx_byte_count = 0;
		io_read(plt_io, plt_rx_buf, PLT_RX_COUNT);
		check_plt_response();
	}
}

static void check_plt_response()
{
	bool response_ok = true;
	uint8_t datasum = 0;
	uint8_t checkval = 0;
	uint8_t conversion_array[5] = {0};
	
	//Check that start byte is ok
	response_ok &= (plt_rx_buf[PLT_RX_START_BYTE] == '*');
	
	//Check that data and check values are hex representations
	response_ok &= is_char_hex(plt_rx_buf[PLT_RX_DATA_BYTE_0]);
	response_ok &= is_char_hex(plt_rx_buf[PLT_RX_DATA_BYTE_1]);
	response_ok &= is_char_hex(plt_rx_buf[PLT_RX_DATA_BYTE_2]);
	response_ok &= is_char_hex(plt_rx_buf[PLT_RX_DATA_BYTE_3]);
	response_ok &= is_char_hex(plt_rx_buf[PLT_RX_CHECK_BYTE_0]);
	response_ok &= is_char_hex(plt_rx_buf[PLT_RX_CHECK_BYTE_1]);
	
	//Check that end byte is ok
	response_ok &= (plt_rx_buf[PLT_RX_END_BYTE] == '^');
	
	//Get reported check value
	conversion_array[0] = plt_rx_buf[PLT_RX_CHECK_BYTE_0];
	conversion_array[1] = plt_rx_buf[PLT_RX_CHECK_BYTE_1];
	long l_checkval = strtol(conversion_array, NULL, 16);
	checkval = (uint8_t)(l_checkval & 0xFF);
	
	//Calculate check value from data bytes
	for(int i = PLT_RX_DATA_BYTE_0; i <= PLT_RX_DATA_BYTE_3; i++)
	{
		datasum += plt_rx_buf[i];
	}
	//Verify that check value matches
	response_ok &= (datasum == checkval);
	
	if(response_ok)
	{
		conversion_array[0] = plt_rx_buf[PLT_RX_DATA_BYTE_0];
		conversion_array[1] = plt_rx_buf[PLT_RX_DATA_BYTE_1];
		conversion_array[2] = plt_rx_buf[PLT_RX_DATA_BYTE_2];
		conversion_array[3] = plt_rx_buf[PLT_RX_DATA_BYTE_3];
		long l_dataval = strtol(conversion_array, NULL, 16);
		uint16_t u_data_val = (uint16_t)(l_dataval & 0xFFFF);
		int16_t data_val_signed = (int16_t)u_data_val;
		float plt_temp = (float)data_val_signed;
		plt_temp /= PLT_TEMP_SCALE_FACTOR;
		
		report_peltier_temp(plt_temp);
	}
	else
	{
		plt_invalid_response_count++;
		if(plt_invalid_response_count >= PLT_MAX_INVALID_RESPONSE)
		{
			plt_invalid_response_count = 0;
			//Report fault using last checksum values
			report_verbose_fault(FAULT_PELTIER_COMM, PLT_COMM_FAULT_CHECKSUM, PLT_MAX_INVALID_RESPONSE, datasum, 0, -1, checkval);
		}
	}
}

static bool is_char_hex(uint8_t val)
{
	if(val >= '0' && val <= '9')
	{
		return true;
	}
	if(val >= 'a' || val <= 'f')
	{
		return true;
	}
	if(val >= 'A' || val <= 'F')
	{
		return true;
	}
	return false;
}

//Callback function, keep short
static void plt_uart_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	plt_rx_byte_count++;
}
