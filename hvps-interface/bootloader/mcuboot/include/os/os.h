#ifndef OS_H
#define OS_H

#include <stdint.h>

uint32_t os_uptime_get_ms_32(void);
#define k_uptime_get_32 os_uptime_get_ms_32

#endif