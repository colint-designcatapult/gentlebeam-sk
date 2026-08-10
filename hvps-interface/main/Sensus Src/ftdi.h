#ifndef FTDI_H_
#define FTDI_H_

#define FTDI_RX_CMD_IDX			1
#define FTDI_RX_CMD_BYTES		5
#define FTDI_RX_PARAM_IDX		(FTDI_RX_CMD_BYTES + FTDI_RX_CMD_IDX)
#define FTDI_RX_PARAM_BYTES		4
#define FTDI_RX_TYPE_IDX		FTDI_RX_PARAM_IDX
#define FTDI_RX_TYPE_BYTES		2
#define FTDI_RX_IDX_IDX			(FTDI_RX_TYPE_IDX + FTDI_RX_TYPE_BYTES)
#define FTDI_RX_IDX_BYTES		2
#define FTDI_RX_DATA_IDX		(FTDI_RX_PARAM_IDX + FTDI_RX_PARAM_BYTES)
#define FTDI_RX_DATA_BYTES		4
#define FTDI_RX_MIN_BYTES		(FTDI_RX_PARAM_IDX + 1)
#define FTDI_RX_GET_BYTES		(FTDI_RX_PARAM_IDX + FTDI_RX_PARAM_BYTES + 1)
#define FTDI_RX_MAX_BYTES		(FTDI_RX_DATA_IDX + FTDI_RX_DATA_BYTES + 1)

#define FTDI_TX_MAX_BYTES       FTDI_RX_MAX_BYTES

/* Backing store for GET/SET, indexed by [type][idx].
 * Adjust FTDI_REG_COUNT to match how many indices are valid per type. */
#define FTDI_REG_COUNT			64

enum
{
    FTDI_TYPE_CONFIG_VALS = 0,
    FTDI_TYPE_SET_POINTS,
    FTDI_TYPE_FB_VALS,
    FTDI_TYPE_COUNT   
};

typedef uint16_t ftdi_type_t; 

typedef enum
{
    FTDI_CMD_BOOTLOADER,
    FTDI_CMD_GET,
    FTDI_CMD_SET,
    FTDI_CMD_NONE   
} ftdi_cmd_t;

typedef struct
{
    const char *cmd;
    ftdi_cmd_t id;
    uint8_t expected_len;
} ftdi_command_t;

#define FLASH_ADDR_START		0x8000000
#define SUBPAGE_SIZE			256
#define SUBPAGES_PER			4

#define CRC_FLASH_PAGE			31
#define CRC_SUBPAGE				(CRC_FLASH_PAGE * SUBPAGES_PER)
#define CRC_ADDR_START			(FLASH_ADDR_START + (CRC_SUBPAGE*SUBPAGE_SIZE))

#define APP_START_PAGE			(CRC_FLASH_PAGE+1)
#define APP_SUBPAGE				(APP_START_PAGE * SUBPAGES_PER)

#define APP_ADDR_START			(FLASH_ADDR_START + (APP_SUBPAGE * SUBPAGE_SIZE))

#define CRC_MAGIC_VALUE    ((uint32_t)0xDEADBEEF)


void setup_ftdi();
void process_ftdi();

void ftdi_rx_cb();


#endif /* FTDI_H_ */