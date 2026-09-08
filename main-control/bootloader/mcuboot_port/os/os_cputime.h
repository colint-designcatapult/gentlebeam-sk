#ifndef H_OS_CPUTIME_
#define H_OS_CPUTIME_

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Busy-wait delay used only by boot_serial's reset command handler. */
void os_cputime_delay_usecs(uint32_t usecs);

#ifdef __cplusplus
}
#endif

#endif /* H_OS_CPUTIME_ */
