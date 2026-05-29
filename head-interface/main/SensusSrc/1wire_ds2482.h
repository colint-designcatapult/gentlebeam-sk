#ifndef DS2484_H
#define DS2484_H

#include "stm32f4xx_hal.h"

/** @name API mode bit flags */
//@{
/** @def MODE_STANDARD */
#define MODE_STANDARD                  0x00
/** @def MODE_OVERDRIVE */
#define MODE_OVERDRIVE                 0x01
/** @def MODE_STRONG */
#define MODE_STRONG                    0x02
//@}

/**
* @internal
* @name DS2482 commands
* @endinternal
*/
//@{
/** @def CMD_DRST */
#define CMD_DRST   0xF0
/** @def CMD_WCFG */
#define CMD_WCFG   0xD2
/** @def CMD_CHSL */
#define CMD_CHSL   0xC3
/** @def CMD_SRP */
#define CMD_SRP    0xE1
/** @def CMD_1WRS */
#define CMD_1WRS   0xB4
/** @def CMD_1WWB */
#define CMD_1WWB   0xA5
/** @def CMD_1WRB */
#define CMD_1WRB   0x96
/** @def CMD_1WSB */
#define CMD_1WSB   0x87
/** @def CMD_1WT */
#define CMD_1WT    0x78
//@}


#define OW_CMD_READ_ROM  (0x33)
#define OW_CMD_READ_MEMORY (0xF0)
#define OW_CMD_SKIP_ROM  (0xCC)
#define OW_CMD_WRITE_SP  (0x0F)
#define OW_CMD_READ_SP   (0xAA)
#define OW_CMD_COPY_SP   (0x55)

#define DEVICE_CONF_REG  (0xC3)
#define STATUS_REG       (0xF0)
#define READ_DATA_REG    (0xE1)
#define PORT_CONFIG_REG  (0xB4)
/**
* @internal
* @name DS2482 config bits
* @endinternal
*/
//@{
/** @def CONFIG_APU */
#define CONFIG_APU  0x01
/** @def CONFIG_PPM */
#define CONFIG_PPM  0x02
/** @def CONFIG_SPU */
#define CONFIG_SPU  0x04
/** @def CONFIG_1WS */
#define CONFIG_1WS  0x08
//@}

/**
* @internal
* @name DS2482 status bits
* @endinternal
*/
//@{
/** @def STATUS_1WB */
#define STATUS_1WB  0x01
/** @def STATUS_PPD */
#define STATUS_PPD  0x02
/** @def STATUS_SD */
#define STATUS_SD   0x04
/** @def STATUS_LL */
#define STATUS_LL   0x08
/** @def STATUS_RST */
#define STATUS_RST  0x10
/** @def STATUS_SBR */
#define STATUS_SBR  0x20
/** @def STATUS_TSB */
#define STATUS_TSB  0x40
/** @def STATUS_DIR */
#define STATUS_DIR  0x80
//@}

#ifndef uchar
   typedef unsigned char uchar;
#endif

// 1-Wire API for DS2482 function prototypes
int OWReset();
void OWWriteByte(uchar sendbyte);
uchar OWReadByte();
uchar OWTouchByte(uchar sendbyte);
//uchar OWTouchBit(uchar sendbit);
//void  OWWriteBit(uchar sendbit);
//int   OWReadBit(void);
void  OWBlock(uchar *tran_buf, int tran_len);
int   OWFirst();
int   OWNext();
// int   OWVerify(void);
// void  OWTargetSetup(uchar family_code);
// void  OWFamilySkipSetup(void);
int   OWSearch();

// Extended 1-Wire functions
//extern int   OWSpeed(int new_speed);
//extern int   OWLevel(int new_level);
void  OWWriteBytePower(uchar sendbyte);
uchar OWReadBytePower();
//extern int   OWReadBitPower(int applyPowerResponse);

// Helper functions
int   DS2482_detect(uchar addr);
uchar DS2482_search_triplet(int search_direction);
int   DS2482_write_config(uint8_t config);
int   DS2482_reset(void);
//extern int   DS2482_channel_select(int channel);
uchar calc_crc8(uchar data);

// last device number found
extern uchar ROM_NO[8];

void DS2484_ReadRom();
uchar OWReadBit();
uchar OWTouchBit(uchar sendbit);
unsigned char docrc8(unsigned char value);
void init_1wire();

void get_col_id(uint8_t *buf, int size);

#endif // DS2484_H
