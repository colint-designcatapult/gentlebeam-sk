#include "boot_uart.h"

#include <atmel_start_pins.h>
#include <driver_init.h>
#include <hal_delay.h>
#include <hal_usart_async.h>
#include <hal_io.h>

static struct io_descriptor *boot_uart_io;

/* usart_async_register_callback() is what actually enables the RXC/TXC
 * interrupt at the peripheral level (see _usart_async_set_irq_state()); the
 * callbacks themselves are no-ops since boot_uart_read()/io_write() already
 * drive the ring buffer directly. */
static void boot_uart_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	(void)io_descr;
}

static void boot_uart_tx_cb(const struct usart_async_descriptor *const io_descr)
{
	(void)io_descr;
}

void boot_uart_init(void)
{
	usart_async_register_callback(&FTDI_UART, USART_ASYNC_RXC_CB, boot_uart_rx_cb);
	usart_async_register_callback(&FTDI_UART, USART_ASYNC_TXC_CB, boot_uart_tx_cb);
	usart_async_get_io_descriptor(&FTDI_UART, &boot_uart_io);
	usart_async_enable(&FTDI_UART);
}

/* Chases the "lit" LED between LED4 and LED5 on every byte received, giving
 * visual feedback while mcumgr traffic is flowing over the FTDI UART.
 * LEDs are active-low: false drives the pin on. */
static void boot_uart_chase_rx_leds(void)
{
	static bool led4_is_on = true;

	gpio_set_pin_level(LED4, !led4_is_on);
	gpio_set_pin_level(LED5, led4_is_on);
	led4_is_on = !led4_is_on;
}

static int boot_uart_read(char *str, int cnt, int *newline)
{
	int n = 0;

	*newline = 0;
	while (n < cnt) {
		uint8_t byte;
		int32_t got = io_read(boot_uart_io, &byte, 1);

		if (got <= 0) {
			continue;
		}

		boot_uart_chase_rx_leds();

		str[n++] = (char)byte;
		if (byte == '\n') {
			*newline = 1;
			break;
		}
	}

	return n;
}

/* usart_async_write() rejects the call with a negative return (does not
 * queue) if a previous transmission hasn't finished yet, and
 * boot_serial_output() issues several back-to-back writes per response - so
 * this must retry until each one is actually accepted, or most of the
 * response gets silently dropped (looks like an NMP timeout / stalled
 * upload progress to the host). Do not replace with a fire-and-forget
 * io_write() call - this has regressed multiple times already. */
static void boot_uart_write_blocking(const uint8_t *buf, uint16_t len)
{
	while (io_write(boot_uart_io, buf, len) < 0) {
		/* Previous transmission still in progress; retry. */
	}
}

static void boot_uart_write(const char *ptr, int cnt)
{
	boot_uart_write_blocking((const uint8_t *)ptr, (uint16_t)cnt);
}

void boot_uart_debug_write(const void *buf, int len)
{
	if (boot_uart_io == NULL) {
		return;
	}
	boot_uart_write_blocking((const uint8_t *)buf, (uint16_t)len);
}

const struct boot_uart_funcs boot_uart_funcs = {
	.read = boot_uart_read,
	.write = boot_uart_write,
};

bool boot_uart_wait_for_rx(uint32_t timeout_ms)
{
	while (timeout_ms > 0) {
		/* Non-consuming check: the caller decides what to do next (e.g.
		 * boot_serial_start()), so the first byte must stay in the ring
		 * buffer for the real protocol parser to read. */
		if (usart_async_is_rx_not_empty(&FTDI_UART)) {
			return true;
		}

		delay_ms(1);
		timeout_ms--;
	}

	return false;
}
