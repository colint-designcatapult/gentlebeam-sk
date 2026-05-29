/*
 * Code generated from Atmel Start.
 *
 * This file will be overwritten when reconfiguring your Atmel Start project.
 * Please copy examples or other code you want to keep to a separate file
 * to avoid losing it when reconfiguring.
 */

#include "driver_init.h"
#include <hal_init.h>
#include <hpl_pmc.h>
#include <peripheral_clk_config.h>
#include <utils.h>
#include <hpl_usart_base.h>

/*! The buffer size for USART */
#define FTDI_UART_BUFFER_SIZE 16

struct usart_async_descriptor FTDI_UART;

static uint8_t FTDI_UART_buffer[FTDI_UART_BUFFER_SIZE];

struct flash_descriptor FLASH_0;

void FLASH_0_CLOCK_init(void)
{
}

void FLASH_0_init(void)
{
	FLASH_0_CLOCK_init();
	flash_init(&FLASH_0, EFC);
}

/**
 * \brief USART Clock initialization function
 *
 * Enables register interface and peripheral clock
 */
void FTDI_UART_CLOCK_init()
{
	_pmc_enable_periph_clock(ID_USART0);
}

/**
 * \brief USART pinmux initialization function
 *
 * Set each required pin to USART functionality
 */
void FTDI_UART_PORT_init()
{

	gpio_set_pin_function(PB0, MUX_PB0C_USART0_RXD0);

	gpio_set_pin_function(PB1, MUX_PB1C_USART0_TXD0);
}

/**
 * \brief USART initialization function
 *
 * Enables USART peripheral, clocks and initializes USART driver
 */
void FTDI_UART_init(void)
{
	FTDI_UART_CLOCK_init();
	FTDI_UART_PORT_init();
	usart_async_init(&FTDI_UART, USART0, FTDI_UART_buffer, FTDI_UART_BUFFER_SIZE, _usart_get_usart_async());
}

void system_init(void)
{
	init_mcu();

	_pmc_enable_periph_clock(ID_PIOA);

	/* Disable Watchdog */
	hri_wdt_set_MR_WDDIS_bit(WDT);

	/* GPIO on PA1 */

	gpio_set_pin_level(IO_INDICATORS_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_INDICATORS_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_INDICATORS_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA8 */

	gpio_set_pin_level(IO_QC_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_QC_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_QC_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA16 */

	gpio_set_pin_level(IO_HV_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_HV_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_HV_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA17 */

	gpio_set_pin_level(IO_GRID_ENn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_GRID_ENn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_GRID_ENn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA18 */

	gpio_set_pin_level(IO_EMISSION_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_EMISSION_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_EMISSION_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA21 */

	gpio_set_pin_level(IO_PUMP_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_PUMP_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_PUMP_EN, GPIO_PIN_FUNCTION_OFF);

	FLASH_0_init();

	FTDI_UART_init();
}
