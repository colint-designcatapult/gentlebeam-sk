#ifndef H_CRC16_
#define H_CRC16_

#include <stddef.h>
#include <stdint.h>

/* Seed expected by mcuboot's boot_serial.c for crc16_ccitt(). */
#define CRC16_INITIAL_CRC 0

#ifdef __cplusplus
extern "C" {
#endif

/* CRC-16/CCITT-FALSE, poly 0x1021, matching Mynewt's crc16_ccitt(). */
uint16_t crc16_ccitt(uint16_t seed, const void *buf, uint32_t len);

#ifdef __cplusplus
}
#endif

#endif /* H_CRC16_ */
