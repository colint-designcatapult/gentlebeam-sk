#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"
#include "string.h"
#include "stdlib.h"

#include "ftdi.h"
#include "ftdi_log.h"
#include "monitoring.h"

#define ARRAY_SIZE(x) (sizeof(x) / sizeof((x)[0]))

static const ftdi_command_t command_table[] =
{
    { "B00TL", FTDI_CMD_BOOTLOADER, FTDI_RX_MIN_BYTES },
    { "GET..", FTDI_CMD_GET,        FTDI_RX_GET_BYTES },
    { "..SET", FTDI_CMD_SET,        FTDI_RX_MAX_BYTES },
};

char cmd_string_comp[FTDI_RX_CMD_BYTES+1];
uint8_t ftdi_rx_buf[FTDI_RX_MAX_BYTES];
uint8_t ftdi_rx_in[2];
static uint8_t ftdi_tx_buf[FTDI_TX_MAX_BYTES];

volatile bool ftdi_tx_busy = false;
volatile bool process_ftdi_rx = false;
volatile uint8_t ftdi_rx_idx = 0;
volatile uint8_t ftdi_cmd = FTDI_CMD_NONE;

static ftdi_cmd_t get_ftdi_rx_cmd();
static void process_ftdi_cmd(ftdi_cmd_t cmd);
static void erase_crc_flash();
static HAL_StatusTypeDef write_crc_magic(void);
static uint16_t parse_u16_le(const uint8_t *p);
static uint32_t parse_u32_le(const uint8_t *p);
static float    parse_f32_le(const uint8_t *p);

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

	ftdi_log_init(&huart3);
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

void ftdi_respond(ftdi_cmd_t cmd_idx, ftdi_type_t type, uint16_t idx, float value)
{
    uint8_t *p = ftdi_tx_buf;

    *p++ = '*';

    memcpy(p, command_table[cmd_idx].cmd, FTDI_RX_CMD_BYTES);
    p += FTDI_RX_CMD_BYTES;

    memcpy(p, &type, sizeof(type));
    p += sizeof(type);

    memcpy(p, &idx, sizeof(idx));
    p += sizeof(idx);

    memcpy(p, &value, sizeof(value));
    p += sizeof(value);

    *p++ = '\n';

    ftdi_write_bytes(ftdi_tx_buf, (size_t)(p - ftdi_tx_buf));
}

static void process_ftdi_cmd(ftdi_cmd_t cmd_idx)
{
	switch(cmd_idx)
	{
		case FTDI_CMD_BOOTLOADER:
			erase_crc_flash();
			HAL_NVIC_SystemReset();
			while(1);
			break;

		case FTDI_CMD_GET:
		{
			uint16_t type_raw = parse_u16_le(&ftdi_rx_buf[FTDI_RX_TYPE_IDX]);
			uint16_t idx       = parse_u16_le(&ftdi_rx_buf[FTDI_RX_IDX_IDX]);

			if((type_raw < FTDI_TYPE_COUNT) && (idx < FTDI_REG_COUNT))
			{
				ftdi_type_t type = (ftdi_type_t)type_raw;
				
				switch(type)
				{
					case FTDI_TYPE_CONFIG_VALS:
						float value = sys_config_get(idx);
						ftdi_respond(cmd_idx, type, idx, value);
						break; 

					default:
						break;
				}
			}
			/* else: type/idx out of range -- silently dropped. No NACK
			 * defined in the protocol yet; add one here if needed. */
			break;
		}

		case FTDI_CMD_SET:
		{
			uint16_t type_raw = parse_u16_le(&ftdi_rx_buf[FTDI_RX_TYPE_IDX]);
			uint16_t idx       = parse_u16_le(&ftdi_rx_buf[FTDI_RX_IDX_IDX]);

			if((type_raw < FTDI_TYPE_COUNT) && (idx < FTDI_REG_COUNT))
			{
				ftdi_type_t type = (ftdi_type_t)type_raw;
				float value = parse_f32_le(&ftdi_rx_buf[FTDI_RX_DATA_IDX]);

				switch (type)
				{
					case FTDI_TYPE_CONFIG_VALS:
						if (sys_config_set(idx, value))
						{
							ftdi_respond(cmd_idx, type, idx, value);
						}
						else
						{
							// Invalid index; silently drop. No NACK defined in the protocol yet.
						}
						break;

					default:
						break;
				}
			}
			break;
		}

		default:
			break;
	}
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


static uint16_t parse_u16_le(const uint8_t *p)
{
    return (uint16_t)p[0] |
           ((uint16_t)p[1] << 8);
}

static uint32_t parse_u32_le(const uint8_t *p)
{
    return (uint32_t)p[0] |
           ((uint32_t)p[1] << 8) |
           ((uint32_t)p[2] << 16) |
           ((uint32_t)p[3] << 24);
}

/* IEEE-754 single precision, transferred as the little-endian bit
 * pattern of the 32-bit word. memcpy is used (rather than a union
 * or pointer cast) to reinterpret the bits without invoking
 * strict-aliasing undefined behavior. */
static float parse_f32_le(const uint8_t *p)
{
    uint32_t bits = parse_u32_le(p);
    float f;

    memcpy(&f, &bits, sizeof(f));
    return f;
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
