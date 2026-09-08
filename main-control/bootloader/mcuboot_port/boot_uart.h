#ifndef H_BOOT_UART_
#define H_BOOT_UART_

#include <stdbool.h>
#include <stdint.h>

#include "boot_serial/boot_serial.h"

/* Initializes the FTDI USART for blocking use by mcuboot's serial recovery
 * (mcumgr) console. Call once before boot_uart_funcs is used. */
void boot_uart_init(void);

/* Raw blocking write used to retarget stdio (printf/assert) onto the FTDI
 * UART for debugging; safe to call even before boot_uart_init(). */
void boot_uart_debug_write(const void *buf, int len);

/* Polls (without consuming) for any incoming byte for up to timeout_ms.
 * Returns true as soon as a byte is pending, false on timeout. */
bool boot_uart_wait_for_rx(uint32_t timeout_ms);

extern const struct boot_uart_funcs boot_uart_funcs;

#endif /* H_BOOT_UART_ */
