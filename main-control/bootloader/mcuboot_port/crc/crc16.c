#include "crc/crc16.h"

uint16_t crc16_ccitt(uint16_t seed, const void *buf, uint32_t len)
{
	const uint8_t *p = (const uint8_t *)buf;
	uint16_t crc = seed;

	for (uint32_t i = 0; i < len; i++) {
		crc ^= (uint16_t)p[i] << 8;
		for (int bit = 0; bit < 8; bit++) {
			if (crc & 0x8000) {
				crc = (uint16_t)((crc << 1) ^ 0x1021);
			} else {
				crc = (uint16_t)(crc << 1);
			}
		}
	}

	return crc;
}
