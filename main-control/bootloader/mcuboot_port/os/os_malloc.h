#ifndef H_OS_MALLOC_
#define H_OS_MALLOC_

#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

void *os_malloc(size_t size);
void os_free(void *ptr);

#ifdef __cplusplus
}
#endif

/* bootutil/boot_serial call plain malloc()/free(); route them to our shim. */
#define malloc  os_malloc
#define free    os_free

#endif /* H_OS_MALLOC_ */
