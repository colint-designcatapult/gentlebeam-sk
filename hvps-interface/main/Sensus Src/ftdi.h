
#ifndef FTDI_H_
#define FTDI_H_

#define FTDI_RX_CMD_IDX			1
#define FTDI_RX_CMD_BYTES		5
#define FTDI_RX_PARAM_IDX		(FTDI_RX_CMD_BYTES + FTDI_RX_CMD_IDX)
#define FTDI_RX_PARAM_BYTES		4
#define FTDI_RX_DATA_IDX		(FTDI_RX_PARAM_IDX + FTDI_RX_PARAM_BYTES)
#define FTDI_RX_DATA_BYTES		8
#define FTDI_RX_MIN_BYTES		(FTDI_RX_PARAM_IDX + 1)
#define FTDI_RX_MAX_BYTES		(FTDI_RX_DATA_IDX + FTDI_RX_DATA_BYTES + 1)

#define FLASH_ADDR_START		0x8000000
#define SUBPAGE_SIZE			256
#define SUBPAGES_PER			4

#define CRC_FLASH_PAGE			31
#define CRC_SUBPAGE				(CRC_FLASH_PAGE * SUBPAGES_PER)
#define CRC_ADDR_START			(FLASH_ADDR_START + (CRC_SUBPAGE*SUBPAGE_SIZE))

#define APP_START_PAGE			(CRC_FLASH_PAGE+1)
#define APP_SUBPAGE				(APP_START_PAGE * SUBPAGES_PER)

#define APP_ADDR_START			(FLASH_ADDR_START + (APP_SUBPAGE * SUBPAGE_SIZE))

typedef enum
{
    FTDI_CMD_NONE = 0,
    FTDI_CMD_BOOTLOADER,
    FTDI_CMD_GET,
    FTDI_CMD_SET
} ftdi_cmd_t;

typedef struct
{
    const char *cmd;
    ftdi_cmd_t id;
    uint8_t expected_len;
} ftdi_command_t;

void setup_ftdi();
void process_ftdi();

void ftdi_rx_cb();
void ftdi_tx_cb();


#endif /* FTDI_H_ */
