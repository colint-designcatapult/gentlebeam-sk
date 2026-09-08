#ifndef OS_MALLOC_H
#define OS_MALLOC_H

#include <stdlib.h>

static inline void *os_malloc(size_t size) { return malloc(size); }
static inline void os_free(void *pointer) { free(pointer); }

#endif