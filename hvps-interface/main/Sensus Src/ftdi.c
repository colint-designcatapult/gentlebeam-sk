#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"
#include "string.h"
#include "stdlib.h"

#include "ftdi.h"
#include "uart_log.h"

#define CRC_MAGIC_VALUE    ((uint32_t)0xDEADBEEF)
#define ARRAY_SIZE(x) (sizeof(x) / sizeof((x)[0]))

static const ftdi_command_t command_table[] =
{
    { "B00TL", FTDI_CMD_BOOTLOADER, FTDI_RX_MIN_BYTES },
    { "GET..", FTDI_CMD_GET,        FTDI_RX_MIN_BYTES },
    { "..SET", FTDI_CMD_SET,        FTDI_RX_MAX_BYTES },
};

char cmd_string_comp[FTDI_RX_CMD_BYTES+1];
uint8_t ftdi_rx_buf[FTDI_RX_MAX_BYTES];
uint8_t ftdi_rx_in[2];

volatile bool ftdi_tx_busy = false;
volatile bool process_ftdi_rx = false;
volatile uint8_t ftdi_rx_idx = 0;
volatile uint8_t ftdi_cmd = FTDI_CMD_NONE;

static ftdi_cmd_t get_ftdi_rx_cmd();
static void process_ftdi_cmd(ftdi_cmd_t cmd);
static void erase_crc_flash();
static HAL_StatusTypeDef write_crc_magic(void);

void setup_ftdi()
{
#if !defined(RELEASE)
	write_crc_magic(); 
#endif

	process_ftdi_rx = false;

	//Clear any outstanding receives before accepting new data
	HAL_UART_AbortReceive(&huart3);

	//Start interrupt reception
	HAL_UART_Receive_IT(&huart3, ftdi_rx_in, 1);
}

void process_ftdi()
{
	if(process_ftdi_rx)
	{
		ftdi_cmd = get_ftdi_rx_cmd();
		process_ftdi_cmd(ftdi_cmd);
		ftdi_rx_idx = 0;
		process_ftdi_rx = false;
	}
}

static ftdi_cmd_t get_ftdi_rx_cmd(void)
{
    for(size_t i = 0; i < ARRAY_SIZE(command_table); i++)
    {
        if((ftdi_rx_idx == command_table[i].expected_len) &&
           (memcmp(&ftdi_rx_buf[1],
                   command_table[i].cmd,
                   FTDI_RX_CMD_BYTES) == 0))
        {
            return command_table[i].id;
        }
    }

    return FTDI_CMD_NONE;
}

static void process_ftdi_cmd(ftdi_cmd_t cmd)
{
	switch(cmd)
	{
		case FTDI_CMD_BOOTLOADER:
			erase_crc_flash();
			HAL_NVIC_SystemReset();
			while(1);
			break;

		case FTDI_CMD_GET:
			break;

		case FTDI_CMD_SET:
			break;

		default:
			break;
	}
}

static void erase_crc_flash()
{
	uint32_t err_resp = 0;

	FLASH_EraseInitTypeDef flash_erase;
	flash_erase.TypeErase = FLASH_TYPEERASE_PAGES;
	flash_erase.PageAddress =  CRC_ADDR_START;
	flash_erase.NbPages = 1;

	HAL_FLASH_Unlock();

	HAL_FLASHEx_Erase(&flash_erase, &err_resp);

	HAL_FLASH_Lock();
}


void ftdi_rx_cb()
{
	if(process_ftdi_rx)
	{
		return;
	}

	if(ftdi_rx_idx == 0 && ftdi_rx_in[0] != '*')
	{
		//Do nothing if start character is invalid
	}
	else
	{
		ftdi_rx_buf[ftdi_rx_idx++] = ftdi_rx_in[0];
		if(ftdi_rx_in[0] == '\n')
		{
			process_ftdi_rx = true;
		}
		else if(ftdi_rx_idx >= FTDI_RX_MAX_BYTES)
		{
			ftdi_rx_idx = 0;
		}
	}

	HAL_UART_Receive_IT(&huart3, ftdi_rx_in, 1);
}

void ftdi_tx_cb()
{
	ftdi_tx_busy = false;
}

/**
 * @brief Write CRC magic value to flash if not already present.
 *
 * Erases the target flash location if needed, programs
 * CRC_MAGIC_VALUE, and verifies the write operation.
 *
 * @return HAL status of the operation.
 */
static HAL_StatusTypeDef write_crc_magic(void)
{
    uint32_t current_value =
        *(volatile uint32_t *)CRC_ADDR_START;

    /* Already exists */
    if (current_value == CRC_MAGIC_VALUE)
    {
        return HAL_OK;
    }

    /* Location is not erased */
    if (current_value != 0xFFFFFFFFU)
    {
        erase_crc_flash();
    }

    HAL_FLASH_Unlock();

    HAL_StatusTypeDef status =
        HAL_FLASH_Program(FLASH_TYPEPROGRAM_WORD,
                          CRC_ADDR_START,
                          CRC_MAGIC_VALUE);

    HAL_FLASH_Lock();

    /* Verify */
    if ((status == HAL_OK) &&
        (*(volatile uint32_t *)CRC_ADDR_START != CRC_MAGIC_VALUE))
    {
        status = HAL_ERROR;
    }

    return status;
}
