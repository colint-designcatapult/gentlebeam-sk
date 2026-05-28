#ifndef BOOTLOADER_H_
#define BOOTLOADER_H_


#define FLASH_ADDR_START	0x8000000
#define SUBPAGE_SIZE		256
#define SUBPAGES_PER		4

#define CRC_FLASH_PAGE		31
#define CRC_SUBPAGE			(CRC_FLASH_PAGE * SUBPAGES_PER)
#define CRC_ADDR_START		(FLASH_ADDR_START + (CRC_SUBPAGE*SUBPAGE_SIZE))

#define APP_START_PAGE		(CRC_FLASH_PAGE+1)
#define APP_SUBPAGE			(APP_START_PAGE * SUBPAGES_PER)

#define APP_ADDR_START		(FLASH_ADDR_START + (APP_SUBPAGE * SUBPAGE_SIZE))

#define MAX_APP_PAGE_COUNT	90


#define NUM_DATA_BYTES	(SUBPAGE_SIZE*2)
#define CMD_IDX			1
#define CMD_BYTES		5
#define PARAM_IDX		(CMD_BYTES+CMD_IDX)
#define PARAM_BYTES		4
#define DATA_IDX		(PARAM_IDX + PARAM_BYTES)
#define MIN_RX_BOOT		(PARAM_IDX+1)
#define MAX_RX_BOOT		(DATA_IDX + NUM_DATA_BYTES + 1)
#define CRC_BYTE_CNT	8
#define RESPONSE_LENGTH	15

//Enumeration for received commands
enum
{
	RX_CMD_NONE = 0,
	RX_CMD_PING,
	RX_CMD_ERASE,
	RX_CMD_WRITE,
	RX_CMD_CHECK
};


void check_app_jump();
void setup_bootloader();
void process_bootloader();
void boot_rx_cb();
void boot_tx_cb();



#endif /* BOOTLOADER_H_ */
