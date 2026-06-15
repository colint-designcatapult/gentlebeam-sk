/*
 * Code generated from Atmel Start.
 *
 * This file will be overwritten when reconfiguring your Atmel Start project.
 * Please copy examples or other code you want to keep to a separate file
 * to avoid losing it when reconfiguring.
 */

#include "driver_examples.h"
#include "driver_init.h"
#include "utils.h"

static uint8_t src_data[IFLASH_PAGE_SIZE];
static uint8_t chk_data[IFLASH_PAGE_SIZE];

/**
 * Example of using FLASH_0 to read and write buffer.
 */
void FLASH_0_example(void)
{
	uint32_t page_size;
	uint16_t i;

	/* Init source data */
	page_size = flash_get_page_size(&FLASH_0);

	for (i = 0; i < page_size; i++) {
		src_data[i] = i;
	}

	/* Write data to flash */
	flash_write(&FLASH_0, 0x3200, src_data, page_size);

	/* Read data from flash */
	flash_read(&FLASH_0, 0x3200, chk_data, page_size);
}

/**
 * Example of using FTDI_UART to write "Hello World" using the IO abstraction.
 *
 * Since the driver is asynchronous we need to use statically allocated memory for string
 * because driver initiates transfer and then returns before the transmission is completed.
 *
 * Once transfer has been completed the tx_cb function will be called.
 */

static uint8_t example_FTDI_UART[12] = "Hello World!";

static void tx_cb_FTDI_UART(const struct usart_async_descriptor *const io_descr)
{
	/* Transfer completed */
}

void FTDI_UART_example(void)
{
	struct io_descriptor *io;

	usart_async_register_callback(&FTDI_UART, USART_ASYNC_TXC_CB, tx_cb_FTDI_UART);
	/*usart_async_register_callback(&FTDI_UART, USART_ASYNC_RXC_CB, rx_cb);
	usart_async_register_callback(&FTDI_UART, USART_ASYNC_ERROR_CB, err_cb);*/
	usart_async_get_io_descriptor(&FTDI_UART, &io);
	usart_async_enable(&FTDI_UART);

	io_write(io, example_FTDI_UART, 12);
}
