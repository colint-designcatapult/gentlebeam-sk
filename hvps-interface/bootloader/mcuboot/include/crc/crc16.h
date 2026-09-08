#ifndef CRC16_H
#define CRC16_H

#include <stdint.h>

/* boot_serial.c only defines this for __ZEPHYR__/__ESPRESSIF__; supply it
 * here since this header is included by the bare-metal build too. */
#ifndef CRC16_INITIAL_CRC
#define CRC16_INITIAL_CRC 0
#endif

uint16_t crc16_ccitt(uint16_t crc, const void *data, uint16_t length);

#endif