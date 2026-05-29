#include <atmel_start.h>
#include <string.h>
#include <stdlib.h>
#include "checksum.h"


#define FLASH_ADDR_START	0x400000
#define CRC_FLASH_PAGE		240
#define PROGRAM_START_PAGE	256
#define MAX_PAGE_COUNT		1792

#define APP_ADDR_START		(FLASH_ADDR_START + (PROGRAM_START_PAGE * page_size))

#define DATA_COUNT		512
#define NUM_DATA_BYTES	(DATA_COUNT*2)

#define CMD_IDX			1
#define CMD_BYTES		5
#define PARAM_IDX		(CMD_BYTES+CMD_IDX)
#define PARAM_BYTES		4
#define DATA_IDX		(PARAM_IDX + PARAM_BYTES)
#define MIN_RX_BYTES	(PARAM_IDX+1)
#define MAX_RX_BYTES	(DATA_IDX + NUM_DATA_BYTES + 1)

#define CRC_BYTE_CNT	8

#define RESPONSE_LENGTH	15

typedef void (*fpJumpHandler)(void);

//Enumeration for received commands
enum
{
	RX_CMD_NONE = 0,
	RX_CMD_PING,
	RX_CMD_ERASE,
	RX_CMD_WRITE,
	RX_CMD_CHECK
};

struct io_descriptor *ftdi_io;
volatile bool tx_busy = false;
volatile bool process_rx = false;
volatile uint32_t rx_idx = 0;
volatile uint8_t rx_cmd = RX_CMD_NONE;

char rx_buf[MAX_RX_BYTES];

char cmd_string_comp[CMD_BYTES];
char string_page_num[5];
char string_byte[3];
char string_checksum[9];

//Command strings received from PC
char *ping_str = "-PING";
char *erase_str = "ERASE";
char *write_str = "WRITE";
char *check_str = "CHECK";

//Response strings sent to PC
char *ping_respn = ".bootloaderrun\n";
char *erase_done = ".erasecomplete\n";
char *write_done = ".writecomplete\n";
char *check_done = ".verifysuccess\n";

uint8_t program_array[DATA_COUNT];

uint32_t page_size = 0;

static void uart_rx_cb(const struct usart_async_descriptor *const io_descr);
static void uart_tx_cb(const struct usart_async_descriptor *const io_descr);

static uint32_t calculate_flash_crc();
static uint8_t get_rx_cmd();
static bool is_char_hex(uint8_t char_in);
static bool check_data_hex();
static bool check_crc_hex();
static void process_cmd(uint8_t cmd);

static void jump_to_main_application()
{
	uint32_t stack_ptr_val;
	fpJumpHandler app_reset_handler;
	
	__DSB();
	__ISB();
	
	//Update vector table offset register
	SCB->VTOR = (uint32_t)(APP_ADDR_START) + SCB_VTOR_TBLOFF_Msk;
	
	__DSB();
	__ISB();
	
	__enable_irq();
	
	//Update stack pointer
	stack_ptr_val = (uint32_t)(*(uint32_t *)(APP_ADDR_START));
	__set_MSP(stack_ptr_val);
	
	//Call application reset handler
	app_reset_handler = (fpJumpHandler)(*((uint32_t*)(APP_ADDR_START + 4)));
	(*app_reset_handler)();
	
}

int main(void)
{	
	/* Initializes MCU, drivers and middleware */
	atmel_start_init();
	
	init_crc32_tab();
	
	page_size = flash_get_page_size(&FLASH_0);
	
	rx_idx = 0;
	
	//Compare stored flash with calculated flash, if match, jump to main application
	uint32_t calculated_crc = calculate_flash_crc();
	uint32_t *stored_crc = (uint32_t *)(FLASH_ADDR_START + (CRC_FLASH_PAGE*page_size));
	if(calculated_crc == *stored_crc && calculated_crc != 0xFFFFFFFF)
	{
		jump_to_main_application();
		while(1)
		{
			//Infinite loop to wait for main jump
		}
		//return 0;
	}
	
	usart_async_register_callback(&FTDI_UART, USART_ASYNC_RXC_CB, uart_rx_cb);
	usart_async_register_callback(&FTDI_UART, USART_ASYNC_TXC_CB, uart_tx_cb);
	usart_async_get_io_descriptor(&FTDI_UART, &ftdi_io);
	usart_async_enable(&FTDI_UART);

	while (1) {
		if(process_rx)
		{
			//Get commands from PC
			rx_cmd = get_rx_cmd();
			
			//Execute command
			process_cmd(rx_cmd);
			
			rx_idx = 0;
			process_rx = false;
		}
	}
}

//Calculate CRC over application flash memory
static uint32_t calculate_flash_crc()
{
	uint8_t *flash_start = (uint8_t *)(APP_ADDR_START);
	
	uint32_t crc_val = crc_32(flash_start, MAX_PAGE_COUNT*page_size);	
	return crc_val;
}

static uint8_t get_rx_cmd()
{
	uint8_t ret_cmd = RX_CMD_NONE;
	memcpy(cmd_string_comp, rx_buf+CMD_IDX, CMD_BYTES);
	//Look for ping command
	if(strcmp(cmd_string_comp, ping_str) == 0 && rx_idx == MIN_RX_BYTES)
	{
		ret_cmd = RX_CMD_PING;
	}
	//Look for erase command
	else if(strcmp(cmd_string_comp, erase_str) == 0 && rx_idx == MIN_RX_BYTES)
	{
		ret_cmd = RX_CMD_ERASE;
	}
	//Look for write command
	else if(strcmp(cmd_string_comp, write_str) == 0 && rx_idx == MAX_RX_BYTES)
	{
		//Verify that data values are valid hex
		if(check_data_hex())
		{
			ret_cmd = RX_CMD_WRITE;	
		}
	}
	//Look for verify command
	else if(strcmp(cmd_string_comp, check_str) == 0 && rx_idx == (MIN_RX_BYTES+CRC_BYTE_CNT))
	{
		//Verify that given crc is valid hex
		if(check_crc_hex())
		{
			ret_cmd = RX_CMD_CHECK;	
		}
	}
	
	return ret_cmd;
}

static bool check_data_hex()
{
	//Verify that all data bytes are valid hex
	for(uint32_t i = PARAM_IDX; i < (rx_idx-1); i++)
	{
		if(!is_char_hex(rx_buf[i]))
		{
			return false;
		}
	}
	return true;
}

static bool check_crc_hex()
{
	//Verify that all CRC bytes are valid hex
	for(uint32_t i = PARAM_IDX; i < (rx_idx-1); i++)
	{
		if(!is_char_hex(rx_buf[i]))
		{
			return false;
		}
	}
	return true;
}

//Verify that a given byte is valid hex (A-F must be capitalized)
static bool is_char_hex(uint8_t char_in)
{
	if(char_in >= '0' && char_in <= '9')
	{
		return true;
	}
	else if(char_in >= 'A' && char_in <= 'F')
	{
		return true;
	}
	
	return false;
}

static void process_cmd(uint8_t cmd)
{
	//Do nothing if TX is busy, should not happen
	if(tx_busy)
	{
		return;
	}
	
	switch(cmd)
	{
		//On ping command, just respond to PC
		case RX_CMD_PING:
			io_write(ftdi_io, ping_respn, RESPONSE_LENGTH);
			break;
		//On erase, erase all application memory then respond to PC
		case RX_CMD_ERASE:
			flash_erase(&FLASH_0, CRC_FLASH_PAGE*page_size, MAX_PAGE_COUNT);
			io_write(ftdi_io, erase_done, RESPONSE_LENGTH);
			break;
		//On write, process values
		case RX_CMD_WRITE:
			//Extract and convert page number from parameters
			memcpy(string_page_num, rx_buf+PARAM_IDX, 4);
			int page_num = strtol(string_page_num, NULL, 16);
			int byte_val = 0;
			//Extract and convert data values from parameters
			for(int i = 0; i < page_size; i++)
			{
				memcpy(string_byte, rx_buf+DATA_IDX+(2*i),2);
				byte_val = strtol(string_byte, NULL, 16);
				program_array[i] = (uint8_t)byte_val;
			}
			//If page is valid, write flash page and respond
			if(page_num < MAX_PAGE_COUNT)
			{
				flash_write(&FLASH_0, ((page_num+PROGRAM_START_PAGE)*page_size), program_array, page_size);
				io_write(ftdi_io, write_done, RESPONSE_LENGTH);	
			}
			break;
		case RX_CMD_CHECK:
			//Get confirmation CRC value from parameters
			memcpy(string_checksum, rx_buf+PARAM_IDX, 8);
			uint32_t reported_crc = strtoul(string_checksum, NULL, 16);
			uint32_t calculated_crc = calculate_flash_crc();
			
			//Compare calculated and received CRC value, on match, update CRC page and respond
			if(reported_crc == calculated_crc)
			{
				flash_write(&FLASH_0, (CRC_FLASH_PAGE*page_size), (uint8_t *)&reported_crc, 4);
				io_write(ftdi_io, check_done, RESPONSE_LENGTH);
			}
			break;
		//Do nothing by default
		case RX_CMD_NONE:
			break;
		default:
			break;
	}
}


static void uart_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	uint8_t rx_in = 0;
	io_read(ftdi_io, &rx_in, 1);
	if(process_rx)
	{
		//Ignore until processing last packet is done
		return;
	}
	
	
	//Wait for start character
	if(rx_idx == 0 && rx_in != '*')
	{
		//Do nothing if start character is invalid
	}
	else
	{
		//Save values until we get end of line
		rx_buf[rx_idx++] = rx_in;
		if(rx_in == '\n')
		{
			process_rx = true;
		}
		//Discard packet if byte limit reached with no newline
		else if(rx_idx >= MAX_RX_BYTES)
		{
			rx_idx = 0;
		}
	}
}

static void uart_tx_cb(const struct usart_async_descriptor *const io_descr)
{
	tx_busy = false;
}

//RSTC->RSTC_CR = RSTC_CR_KEY_PASSWD | RSTC_CR_PROCRST;