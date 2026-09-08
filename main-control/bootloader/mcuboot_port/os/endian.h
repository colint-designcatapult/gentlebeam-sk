#ifndef H_OS_ENDIAN_
#define H_OS_ENDIAN_

#include <stdint.h>

static inline uint16_t ntohs(uint16_t v)
{
	return (uint16_t)((v << 8) | (v >> 8));
}

static inline uint16_t htons(uint16_t v)
{
	return ntohs(v);
}

#endif /* H_OS_ENDIAN_ */
