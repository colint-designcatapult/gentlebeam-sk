/* Thin wrapper around the C library heap; intentionally does not include
 * os_malloc.h (which redefines malloc/free) to avoid infinite recursion. */
#include <stdlib.h>

void *os_malloc(size_t size)
{
	return malloc(size);
}

void os_free(void *ptr)
{
	free(ptr);
}
