#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"
#include "string.h"
#include "stdlib.h"

#include "bootloader.h"
#include "checksum.h"

//HAL_NVIC_SystemReset();

volatile bool boot_tx_busy = false;
volatile bool process_boot_rx = false;
volatile uint32_t boot_rx_idx = 0;
volatile uint8_t boot_rx_cmd = RX_CMD_NONE;
uint8_t boot_rx_in[2];

uint8_t program_array[SUBPAGE_SIZE];
uint8_t boot_rx_buf[MAX_RX_BOOT];
char cmd_string_comp[CMD_BYTES+1];
char string_subpage_num[CMD_BYTES+1];
char string_byte[3];
char string_checksum[10];

//Command strings received from PC
char *ping_str = "+P1NG";
char *erase_str = "CLEAR";
char *write_str = "PROGR";
char *check_str = "VERIF";

//Response strings sent to PC
char *ping_respn = ".bootloaderrun\n";
char *erase_done = ".erasecomplete\n";
char *write_done = ".writecomplete\n";
char *check_done = ".verifysuccess\n";

typedef void (*fpJumpHandler)(void);

static uint32_t calculate_flash_crc();
static uint8_t get_rx_cmd();
static bool is_char_hex(uint8_t char_in);
static bool check_data_hex();
static bool check_crc_hex();
static void process_boot_cmd(uint8_t cmd);

static void erase_app_flash();
static bool program_flash_subpage();
static bool verify_app_flash();

static void jump_to_main_application();

void check_app_jump()
{
	init_crc32_tab();

	uint32_t calculated_crc = calculate_flash_crc();
	uint32_t *stored_crc = (uint32_t *)CRC_ADDR_START;

	//Compare stored flash with calculated flash, if match, jump to main application
	//if((calculated_crc == *stored_crc && calculated_crc != 0xFFFFFFFF) || *stored_crc == 0xDEADBEEF)
	//{
		jump_to_main_application();
		while(1)
		{
			//Infinite loop to wait for main jump
		}
	//}
}


void setup_bootloader()
{

	//Clear any outstanding receives before accepting new data
	HAL_UART_AbortReceive(&huart3);

	//Start interrupt reception
	HAL_UART_Receive_IT(&huart3, boot_rx_in, 1);
}

void process_bootloader()
{
	if(process_boot_rx)
	{
		//Get commands from PC
		boot_rx_cmd = get_rx_cmd();

		//Execute command
		process_boot_cmd(boot_rx_cmd);
		boot_rx_idx = 0;
		process_boot_rx = false;
	}
}


static uint8_t get_rx_cmd()
{
	uint8_t ret_cmd = RX_CMD_NONE;
	memcpy(cmd_string_comp, boot_rx_buf+CMD_IDX, CMD_BYTES);
	//Look for ping command
	if(strcmp(cmd_string_comp, ping_str) == 0 && boot_rx_idx == MIN_RX_BOOT)
	{
		ret_cmd = RX_CMD_PING;
	}
	//Look for erase command
	else if(strcmp(cmd_string_comp, erase_str) == 0 && boot_rx_idx == MIN_RX_BOOT)
	{
		ret_cmd = RX_CMD_ERASE;
	}
	//Look for write command
	else if(strcmp(cmd_string_comp, write_str) == 0 && boot_rx_idx == MAX_RX_BOOT)
	{
		HAL_GPIO_TogglePin(GPIOD, IO_TEST_1_Pin);
		//Verify that data values are valid hex
		if(check_data_hex())
		{
			ret_cmd = RX_CMD_WRITE;
		}
	}
	//Look for verify command
	else if(strcmp(cmd_string_comp, check_str) == 0 && boot_rx_idx == (MIN_RX_BOOT+CRC_BYTE_CNT))
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
	for(uint32_t i = PARAM_IDX; i < (boot_rx_idx-1); i++)
	{
		if(!is_char_hex(boot_rx_buf[i]))
		{
			return false;
		}
	}
	return true;
}

static bool check_crc_hex()
{
	//Verify that all CRC bytes are valid hex
	for(uint32_t i = PARAM_IDX; i < (boot_rx_idx-1); i++)
	{
		if(!is_char_hex(boot_rx_buf[i]))
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

void process_boot_cmd(uint8_t cmd)
{
	//Do nothing if TX is busy, should not happen
	if(boot_tx_busy)
	{
		return;
	}
	HAL_GPIO_TogglePin(GPIOD, IO_TEST_3_Pin);
	switch(cmd)
	{
		case RX_CMD_PING:
			HAL_UART_Transmit_IT(&huart3, (uint8_t *)ping_respn, RESPONSE_LENGTH);
			boot_tx_busy = true;
			break;
		case RX_CMD_ERASE:
			erase_app_flash();
			HAL_UART_Transmit_IT(&huart3, (uint8_t *)erase_done, RESPONSE_LENGTH);
			boot_tx_busy = true;
			break;
		case RX_CMD_WRITE:
			if(program_flash_subpage())
			{
				HAL_UART_Transmit_IT(&huart3, (uint8_t *)write_done, RESPONSE_LENGTH);
				boot_tx_busy = true;
			}
			break;
		case RX_CMD_CHECK:
			if(verify_app_flash())
			{
				HAL_UART_Transmit_IT(&huart3, (uint8_t *)check_done, RESPONSE_LENGTH);
				boot_tx_busy = true;
			}
			break;
		default:
			break;
	}
}

//Erase application memory
static void erase_app_flash()
{
	uint32_t err_resp = 0;

	FLASH_EraseInitTypeDef flash_erase;
	flash_erase.TypeErase = FLASH_TYPEERASE_PAGES;
	flash_erase.PageAddress =  CRC_ADDR_START;
	flash_erase.NbPages = MAX_APP_PAGE_COUNT+1;

	HAL_FLASH_Unlock();

	HAL_FLASHEx_Erase(&flash_erase, &err_resp);

	HAL_FLASH_Lock();
}

//Write to flash subpage
static bool program_flash_subpage()
{
	//Extract subpage value from RX parameters
	memcpy(string_subpage_num, boot_rx_buf+PARAM_IDX, 4);
	int subpage_idx = strtol(string_subpage_num, NULL, 16);
	if(subpage_idx > MAX_APP_PAGE_COUNT)
	{
		return false;
	}

	//Extract write values from RX parameters
	int byte_val;
	for(int i = 0; i < SUBPAGE_SIZE; i++)
	{
		memcpy(string_byte, boot_rx_buf+DATA_IDX+(2*i),2);
		byte_val = strtol(string_byte, NULL, 16);
		program_array[i] = (uint8_t)byte_val;
	}

	uint32_t prog_val = 0;
	uint32_t *word_val = (uint32_t *)(program_array);
	uint32_t val_addr = APP_ADDR_START + (subpage_idx * SUBPAGE_SIZE);
	int num_words = SUBPAGE_SIZE/sizeof(uint32_t);

	HAL_FLASH_Unlock();

	//Write subpage with values
	for(int i = 0; i < num_words; i++)
	{
		prog_val = *word_val;
		HAL_FLASH_Program(FLASH_TYPEPROGRAM_WORD, val_addr, prog_val);
		val_addr += 4;
		word_val++;
	}

	HAL_FLASH_Lock();

	return true;
}

static bool verify_app_flash()
{
	//Get verification CRC value
	memcpy(string_checksum, boot_rx_buf+PARAM_IDX, 8);
	uint32_t reported_crc = strtoul(string_checksum, NULL, 16);
	uint32_t calculated_crc = calculate_flash_crc();

	//Compare received vs calculated CRC, if good, update CRC page
	if(reported_crc == calculated_crc)
	{
		HAL_FLASH_Unlock();
		HAL_FLASH_Program(FLASH_TYPEPROGRAM_WORD, CRC_ADDR_START, calculated_crc);
		HAL_FLASH_Lock();
		return true;
	}
	else
	{
		return false;
	}
}

static uint32_t calculate_flash_crc()
{
	uint8_t *flash_start = (uint8_t *)(APP_ADDR_START);

	uint32_t crc_val = crc_32(flash_start, MAX_APP_PAGE_COUNT*SUBPAGES_PER*SUBPAGE_SIZE);
	return crc_val;
}

static void jump_to_main_application()
{
	uint32_t stack_ptr_val;
	fpJumpHandler app_reset_handler;

	__disable_irq();

	__DSB();
	__ISB();

	//Update vector table offset register
	SCB->VTOR = (uint32_t)(APP_ADDR_START);//+ SCB_VTOR_TBLOFF_Msk;

	__DSB();
	__ISB();

	__enable_irq();

	//Update stack pointer
	stack_ptr_val = (uint32_t)(*(uint32_t *)(APP_ADDR_START));
	__set_MSP(stack_ptr_val);

	//Call application reset handler
	app_reset_handler = (fpJumpHandler)(*((uint32_t*)(APP_ADDR_START+4)));
	(*app_reset_handler)();

}

void boot_rx_cb()
{
	if(process_boot_rx)
	{
		//Ignore until processing last packet is done
		return;
	}

	//Wait for start character
	if(boot_rx_idx == 0 && boot_rx_in[0] != '*')
	{
		//Do nothing if start character is invalid
	}
	else
	{
		//Save values until we get end of line
		boot_rx_buf[boot_rx_idx++] = boot_rx_in[0];
		if(boot_rx_in[0] == '\n')
		{
			process_boot_rx = true;
		}
		//Discard packet if byte limit reached with no newline
		else if(boot_rx_idx >= MAX_RX_BOOT)
		{
			boot_rx_idx = 0;
		}
	}

	HAL_UART_Receive_IT(&huart3, boot_rx_in, 1);
}

void boot_tx_cb()
{
	boot_tx_busy = false;
}

void HAL_UART_TxCpltCallback(UART_HandleTypeDef *huart)
{
	if(huart->Instance == USART3)
	{
		boot_tx_cb();
	}
}

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
	if(huart->Instance == USART3)
	{
		boot_rx_cb();
	}
}
