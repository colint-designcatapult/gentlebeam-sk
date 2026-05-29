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
 * Example of using CALENDER_INTERFACE.
 */
static struct calendar_alarm alarm;

static void alarm_cb(struct calendar_descriptor *const descr)
{
	/* alarm expired */
}

void CALENDER_INTERFACE_example(void)
{
	struct calendar_date date;
	struct calendar_time time;

	calendar_enable(&CALENDER_INTERFACE);

	date.year  = 2000;
	date.month = 12;
	date.day   = 31;

	time.hour = 12;
	time.min  = 59;
	time.sec  = 59;

	calendar_set_date(&CALENDER_INTERFACE, &date);
	calendar_set_time(&CALENDER_INTERFACE, &time);

	alarm.cal_alarm.datetime.time.sec = 4;
	alarm.cal_alarm.option            = CALENDAR_ALARM_MATCH_SEC;
	alarm.cal_alarm.mode              = REPEAT;

	calendar_set_alarm(&CALENDER_INTERFACE, &alarm, alarm_cb);
}

/**
 * Example of using DAC_SPI to write "Hello World" using the IO abstraction.
 *
 * Since the driver is asynchronous we need to use statically allocated memory for string
 * because driver initiates transfer and then returns before the transmission is completed.
 *
 * Once transfer has been completed the tx_cb function will be called.
 */

static uint8_t example_DAC_SPI[12] = "Hello World!";

static void complete_cb_DAC_SPI(const struct spi_m_async_descriptor *const io_descr)
{
	/* Transfer completed */
}

void DAC_SPI_example(void)
{
	struct io_descriptor *io;
	spi_m_async_get_io_descriptor(&DAC_SPI, &io);

	spi_m_async_register_callback(&DAC_SPI, SPI_M_ASYNC_CB_XFER, (FUNC_PTR)complete_cb_DAC_SPI);
	spi_m_async_enable(&DAC_SPI);
	io_write(io, example_DAC_SPI, 12);
}

/**
 * Example of using VTIMER.
 */
static struct timer_task VTIMER_task1, VTIMER_task2;

static void VTIMER_task1_cb(const struct timer_task *const timer_task)
{
}

static void VTIMER_task2_cb(const struct timer_task *const timer_task)
{
}

void VTIMER_example(void)
{
	VTIMER_task1.interval = 100;
	VTIMER_task1.cb       = VTIMER_task1_cb;
	VTIMER_task1.mode     = TIMER_TASK_REPEAT;
	VTIMER_task2.interval = 200;
	VTIMER_task2.cb       = VTIMER_task2_cb;
	VTIMER_task2.mode     = TIMER_TASK_REPEAT;

	timer_add_task(&VTIMER, &VTIMER_task1);
	timer_add_task(&VTIMER, &VTIMER_task2);
	timer_start(&VTIMER);
}

void TIMERS_I2C_example(void)
{
	struct io_descriptor *TIMERS_I2C_io;

	i2c_m_sync_get_io_descriptor(&TIMERS_I2C, &TIMERS_I2C_io);
	i2c_m_sync_enable(&TIMERS_I2C);
	i2c_m_sync_set_slaveaddr(&TIMERS_I2C, 0x12, I2C_M_SEVEN);
	io_write(TIMERS_I2C_io, (uint8_t *)"Hello World!", 12);
}

void ADC_I2C_example(void)
{
	struct io_descriptor *ADC_I2C_io;

	i2c_m_sync_get_io_descriptor(&ADC_I2C, &ADC_I2C_io);
	i2c_m_sync_enable(&ADC_I2C);
	i2c_m_sync_set_slaveaddr(&ADC_I2C, 0x12, I2C_M_SEVEN);
	io_write(ADC_I2C_io, (uint8_t *)"Hello World!", 12);
}

/**
 * Example of using QC_UART to write "Hello World" using the IO abstraction.
 *
 * Since the driver is asynchronous we need to use statically allocated memory for string
 * because driver initiates transfer and then returns before the transmission is completed.
 *
 * Once transfer has been completed the tx_cb function will be called.
 */

static uint8_t example_QC_UART[12] = "Hello World!";

static void tx_cb_QC_UART(const struct usart_async_descriptor *const io_descr)
{
	/* Transfer completed */
}

void QC_UART_example(void)
{
	struct io_descriptor *io;

	usart_async_register_callback(&QC_UART, USART_ASYNC_TXC_CB, tx_cb_QC_UART);
	/*usart_async_register_callback(&QC_UART, USART_ASYNC_RXC_CB, rx_cb);
	usart_async_register_callback(&QC_UART, USART_ASYNC_ERROR_CB, err_cb);*/
	usart_async_get_io_descriptor(&QC_UART, &io);
	usart_async_enable(&QC_UART);

	io_write(io, example_QC_UART, 12);
}

/**
 * Example of using HVPS_UART to write "Hello World" using the IO abstraction.
 *
 * Since the driver is asynchronous we need to use statically allocated memory for string
 * because driver initiates transfer and then returns before the transmission is completed.
 *
 * Once transfer has been completed the tx_cb function will be called.
 */

static uint8_t example_HVPS_UART[12] = "Hello World!";

static void tx_cb_HVPS_UART(const struct usart_async_descriptor *const io_descr)
{
	/* Transfer completed */
}

void HVPS_UART_example(void)
{
	struct io_descriptor *io;

	usart_async_register_callback(&HVPS_UART, USART_ASYNC_TXC_CB, tx_cb_HVPS_UART);
	/*usart_async_register_callback(&HVPS_UART, USART_ASYNC_RXC_CB, rx_cb);
	usart_async_register_callback(&HVPS_UART, USART_ASYNC_ERROR_CB, err_cb);*/
	usart_async_get_io_descriptor(&HVPS_UART, &io);
	usart_async_enable(&HVPS_UART);

	io_write(io, example_HVPS_UART, 12);
}

/**
 * Example of using HB_UART to write "Hello World" using the IO abstraction.
 *
 * Since the driver is asynchronous we need to use statically allocated memory for string
 * because driver initiates transfer and then returns before the transmission is completed.
 *
 * Once transfer has been completed the tx_cb function will be called.
 */

static uint8_t example_HB_UART[12] = "Hello World!";

static void tx_cb_HB_UART(const struct usart_async_descriptor *const io_descr)
{
	/* Transfer completed */
}

void HB_UART_example(void)
{
	struct io_descriptor *io;

	usart_async_register_callback(&HB_UART, USART_ASYNC_TXC_CB, tx_cb_HB_UART);
	/*usart_async_register_callback(&HB_UART, USART_ASYNC_RXC_CB, rx_cb);
	usart_async_register_callback(&HB_UART, USART_ASYNC_ERROR_CB, err_cb);*/
	usart_async_get_io_descriptor(&HB_UART, &io);
	usart_async_enable(&HB_UART);

	io_write(io, example_HB_UART, 12);
}

/**
 * Example of using PLT_UART to write "Hello World" using the IO abstraction.
 *
 * Since the driver is asynchronous we need to use statically allocated memory for string
 * because driver initiates transfer and then returns before the transmission is completed.
 *
 * Once transfer has been completed the tx_cb function will be called.
 */

static uint8_t example_PLT_UART[12] = "Hello World!";

static void tx_cb_PLT_UART(const struct usart_async_descriptor *const io_descr)
{
	/* Transfer completed */
}

void PLT_UART_example(void)
{
	struct io_descriptor *io;

	usart_async_register_callback(&PLT_UART, USART_ASYNC_TXC_CB, tx_cb_PLT_UART);
	/*usart_async_register_callback(&PLT_UART, USART_ASYNC_RXC_CB, rx_cb);
	usart_async_register_callback(&PLT_UART, USART_ASYNC_ERROR_CB, err_cb);*/
	usart_async_get_io_descriptor(&PLT_UART, &io);
	usart_async_enable(&PLT_UART);

	io_write(io, example_PLT_UART, 12);
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

void MACIF_example(void)
{
	mac_async_enable(&MACIF);
	mac_async_write(&MACIF, (uint8_t *)"Hello World!", 12);
}
