#ifndef OS_ENDIAN_H
#define OS_ENDIAN_H

#include <stdint.h>

static inline uint16_t htons(uint16_t value) { return (uint16_t)((value << 8) | (value >> 8)); }
static inline uint16_t ntohs(uint16_t value) { return htons(value); }

#endif