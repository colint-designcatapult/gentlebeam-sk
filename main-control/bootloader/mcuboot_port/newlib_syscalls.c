#include <sys/types.h>

#include "boot_uart.h"

/* Minimal stdio retargeting stubs so printf()/assert() output is visible on
 * the FTDI UART instead of vanishing (needed to debug why the bootloader
 * ends up in _exit()). */

int _write(int file, char *ptr, int len)
{
	(void)file;
	boot_uart_debug_write(ptr, len);
	return len;
}

int _read(int file, char *ptr, int len)
{
	(void)file;
	(void)ptr;
	(void)len;
	return 0;
}
