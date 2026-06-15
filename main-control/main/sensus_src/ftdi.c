/*
 */ 

#include <atmel_start.h>
#include <string.h>
#include <stdlib.h>
#include "ftdi.h"

#include "state_machine.h"
#include "system_parameters.h"
#include "faults.h"
#include "qc_well.h"
#include "ext_timers.h"

static void ftdi_rx_cb(const struct usart_async_descriptor *const io_descr);
static void ftdi_tx_cb(const struct usart_async_descriptor *const io_descr);

struct io_descriptor *ftdi_io;

volatile bool ftdi_tx_busy = false;
volatile bool process_ftdi_rx = false;
volatile uint8_t ftdi_rx_idx = 0;

//Strings for received command values
char *ftdi_boot_cmd = "BOOTL";
char *ftdi_get_cmd = "GET..";
char *ftdi_set_cmd = "..SET";
char *ftdi_go_cmd = "GO!!!";
char cmd_string_comp[FTDI_RX_CMD_BYTES];

char ftdi_debug_str_array[7];

uint8_t ftdi_rx_buf[FTDI_RX_MAX_BYTES];
uint32_t page_size = 0;

static uint8_t get_ftdi_rx_cmd();
static void process_ftdi_cmd(uint8_t cmd);

static void debug_test_report_state();


void init_ftdi()
{
	strcpy(ftdi_debug_str_array, "0000\n");
	page_size = flash_get_page_size(&FLASH_0);
	
	usart_async_register_callback(&FTDI_UART, USART_ASYNC_RXC_CB, ftdi_rx_cb);
	usart_async_get_io_descriptor(&FTDI_UART, &ftdi_io);
	usart_async_enable(&FTDI_UART);
}

void process_ftdi()
{
	if(!process_ftdi_rx)
	{
		return;
	}
	
	//Extract command received from PC USB
	uint8_t rx_cmd = get_ftdi_rx_cmd();
	
	process_ftdi_cmd(rx_cmd);
	
	ftdi_rx_idx = 0;
	process_ftdi_rx = false;
}

//Calculate application CRC value
uint32_t get_app_crc()
{
	uint32_t page_size = flash_get_page_size(&FLASH_0);
	uint32_t *stored_crc = (uint32_t *)(FLASH_ADDR_START + (CRC_FLASH_PAGE*page_size));
	
	return *stored_crc;	
}

static uint8_t get_ftdi_rx_cmd()
{
	uint8_t ret_cmd = FTDI_CMD_NONE;
	memcpy(cmd_string_comp, ftdi_rx_buf+1, FTDI_RX_CMD_BYTES);
	//Check if PC requests app to go into bootloader mode
	if(strcmp(cmd_string_comp, ftdi_boot_cmd) == 0 && ftdi_rx_idx == FTDI_MIN_RX_BYTES)
	{
		ret_cmd = FTDI_CMD_BOOTLOADER;
	}
	//Check if PC requests debug "get" command
	else if(strcmp(cmd_string_comp, ftdi_get_cmd) == 0 && ftdi_rx_idx == FTDI_MIN_RX_BYTES)
	{
		ret_cmd = FTDI_CMD_GET;
	}
	//Check if PC requests debug "set" command
	else if(strcmp(cmd_string_comp, ftdi_set_cmd) == 0 && ftdi_rx_idx == FTDI_MIN_RX_BYTES)
	{
		ret_cmd = FTDI_CMD_SET;
	}
	//Check if PC requests debug "go" command
	else if(strcmp(cmd_string_comp, ftdi_go_cmd) == 0 && ftdi_rx_idx == FTDI_MIN_RX_BYTES)
	{
		ret_cmd = FTDI_CMD_GO;
	}
	
	return ret_cmd;
}

static void process_ftdi_cmd(uint8_t cmd)
{
	switch(cmd)
	{
		//On bootloader command, the FW will erase the memory location of the application CRC and call an MCU reset
		case FTDI_CMD_BOOTLOADER:
			flash_erase(&FLASH_0, (CRC_FLASH_PAGE*page_size), 1);
			RSTC->RSTC_CR = RSTC_CR_KEY_PASSWD | RSTC_CR_PROCRST;
			while(1)
			{
			}
			break;
		//Debug functionality
		case FTDI_CMD_GET:
			debug_test_report_state();
			break;
		//Debug functionality
		case FTDI_CMD_SET:
			queue_sm_event(EVENT_PC_CLEAR_FAULT);
			break;
		case FTDI_CMD_GO:
			break;
		default:
			break;
	}
}

//Debug function
static void debug_test_report_state()
{
	ftdi_debug_str_array[0] = 0x30;
	ftdi_debug_str_array[1] = 0x20;
	ftdi_debug_str_array[4] = 0x0A;
	ftdi_debug_str_array[5] = 0x0D;
	switch(system_status[SS_STATE].u)
	{
		case STATE_STARTUP:
			ftdi_debug_str_array[2] = 'S';
			ftdi_debug_str_array[3] = 'U';
			break;
		case STATE_COLD:
			ftdi_debug_str_array[2] = 'C';
			ftdi_debug_str_array[3] = 'L';
			break;
		case STATE_COLD_FAULT:
			ftdi_debug_str_array[2] = 'C';
			ftdi_debug_str_array[3] = 'F';
			break;
		case STATE_CONDITIONING:
			ftdi_debug_str_array[2] = 'C';
			ftdi_debug_str_array[3] = 'N';
			break;
		case STATE_WARMUP:
			ftdi_debug_str_array[2] = 'W';
			ftdi_debug_str_array[3] = 'U';
			break;
		case STATE_WARMUP_FAULT:
			ftdi_debug_str_array[2] = 'W';
			ftdi_debug_str_array[3] = 'F';
			break;
		case STATE_PRIMED:
			ftdi_debug_str_array[2] = 'P';
			ftdi_debug_str_array[3] = 'R';
			break;
		case STATE_STAGING:
			ftdi_debug_str_array[2] = 'S';
			ftdi_debug_str_array[3] = 'I';
			break;
		case STATE_STAGED:
			ftdi_debug_str_array[2] = 'S';
			ftdi_debug_str_array[3] = 'D';
			break;
		case STATE_HVPS_CHECK:
			ftdi_debug_str_array[2] = 'H';
			ftdi_debug_str_array[3] = 'V';
			break;
		case STATE_SETUP:
			ftdi_debug_str_array[2] = 'S';
			ftdi_debug_str_array[3] = 'U';
			break;
		case STATE_READY:
			ftdi_debug_str_array[2] = 'R';
			ftdi_debug_str_array[3] = 'Y';
			break;
		case STATE_LAUNCHING:
			ftdi_debug_str_array[2] = 'L';
			ftdi_debug_str_array[3] = 'C';
			break;
		case STATE_EMISSION:
			ftdi_debug_str_array[2] = 'E';
			ftdi_debug_str_array[3] = 'M';
			break;
		case STATE_TERMINATION:
			ftdi_debug_str_array[2] = 'T';
			ftdi_debug_str_array[3] = 'M';
			break;
		case STATE_DISCHARGE:
			ftdi_debug_str_array[2] = 'D';
			ftdi_debug_str_array[3] = 'C';
			break;
		case STATE_FAULT:
			ftdi_debug_str_array[2] = 'F';
			ftdi_debug_str_array[3] = 'F';
			break;
		default:
			ftdi_debug_str_array[2] = 'X';
			ftdi_debug_str_array[3] = 'X';
			break;
	}
	
	io_write(ftdi_io, ftdi_debug_str_array, 6);
}

static void ftdi_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	uint8_t rx_in = 0;
	io_read(ftdi_io, &rx_in, 1);
	
	if(process_ftdi_rx)
	{
		return;
	}
	
	if(ftdi_rx_idx == 0 && rx_in != '*')
	{
		//Do nothing if start character is invalid
	}
	else
	{
		ftdi_rx_buf[ftdi_rx_idx++] = rx_in;
		if(rx_in == '\n')
		{
			process_ftdi_rx = true;
		}
		else if(ftdi_rx_idx >= FTDI_RX_MAX_BYTES)
		{
			ftdi_rx_idx = 0;
		}
	}
}

static void ftdi_tx_cb(const struct usart_async_descriptor *const io_descr)
{
	ftdi_tx_busy = false;
}

