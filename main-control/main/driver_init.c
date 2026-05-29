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
#include <hpl_spi_base.h>
#include <hpl_tc.h>

#include <hpl_usart_base.h>

/*! The buffer size for USART */
#define QC_UART_BUFFER_SIZE 16
/*! The buffer size for USART */
#define HVPS_UART_BUFFER_SIZE 16
/*! The buffer size for USART */
#define HB_UART_BUFFER_SIZE 16
/*! The buffer size for USART */
#define PLT_UART_BUFFER_SIZE 16
/*! The buffer size for USART */
#define FTDI_UART_BUFFER_SIZE 16

struct timer_descriptor       VTIMER;
struct usart_async_descriptor QC_UART;
struct usart_async_descriptor HVPS_UART;
struct usart_async_descriptor HB_UART;
struct usart_async_descriptor PLT_UART;
struct usart_async_descriptor FTDI_UART;

static uint8_t QC_UART_buffer[QC_UART_BUFFER_SIZE];
static uint8_t HVPS_UART_buffer[HVPS_UART_BUFFER_SIZE];
static uint8_t HB_UART_buffer[HB_UART_BUFFER_SIZE];
static uint8_t PLT_UART_buffer[PLT_UART_BUFFER_SIZE];
static uint8_t FTDI_UART_buffer[FTDI_UART_BUFFER_SIZE];

struct flash_descriptor FLASH_0;

struct mci_sync_desc IO_BUS;

struct calendar_descriptor CALENDER_INTERFACE;

struct spi_m_async_descriptor DAC_SPI;

struct i2c_m_sync_desc TIMERS_I2C;

struct i2c_m_sync_desc ADC_I2C;

struct mac_async_descriptor MACIF;

void FLASH_0_CLOCK_init(void)
{
}

void FLASH_0_init(void)
{
	FLASH_0_CLOCK_init();
	flash_init(&FLASH_0, EFC);
}

void IO_BUS_PORT_init(void)
{

	gpio_set_pin_direction(PA28,
	                       // <y> Pin direction
	                       // <id> pad_direction
	                       // <GPIO_DIRECTION_OFF"> Off
	                       // <GPIO_DIRECTION_IN"> In
	                       // <GPIO_DIRECTION_OUT"> Out
	                       GPIO_DIRECTION_OUT);

	gpio_set_pin_level(PA28,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	gpio_set_pin_pull_mode(PA28,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(PA28,
	                      // <y> Pin function
	                      // <id> pad_function
	                      // <i> Auto : use driver pinmux if signal is imported by driver, else turn off function
	                      // <MUX_PA28C_HSMCI_MCCDA"> Auto
	                      // <GPIO_PIN_FUNCTION_OFF"> Off
	                      // <GPIO_PIN_FUNCTION_A"> A
	                      // <GPIO_PIN_FUNCTION_B"> B
	                      // <GPIO_PIN_FUNCTION_C"> C
	                      // <GPIO_PIN_FUNCTION_D"> D
	                      MUX_PA28C_HSMCI_MCCDA);

	gpio_set_pin_direction(PA25,
	                       // <y> Pin direction
	                       // <id> pad_direction
	                       // <GPIO_DIRECTION_OFF"> Off
	                       // <GPIO_DIRECTION_IN"> In
	                       // <GPIO_DIRECTION_OUT"> Out
	                       GPIO_DIRECTION_OUT);

	gpio_set_pin_level(PA25,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	gpio_set_pin_pull_mode(PA25,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(PA25,
	                      // <y> Pin function
	                      // <id> pad_function
	                      // <i> Auto : use driver pinmux if signal is imported by driver, else turn off function
	                      // <MUX_PA25D_HSMCI_MCCK"> Auto
	                      // <GPIO_PIN_FUNCTION_OFF"> Off
	                      // <GPIO_PIN_FUNCTION_A"> A
	                      // <GPIO_PIN_FUNCTION_B"> B
	                      // <GPIO_PIN_FUNCTION_C"> C
	                      // <GPIO_PIN_FUNCTION_D"> D
	                      MUX_PA25D_HSMCI_MCCK);

	gpio_set_pin_direction(PA30,
	                       // <y> Pin direction
	                       // <id> pad_direction
	                       // <GPIO_DIRECTION_OFF"> Off
	                       // <GPIO_DIRECTION_IN"> In
	                       // <GPIO_DIRECTION_OUT"> Out
	                       GPIO_DIRECTION_OUT);

	gpio_set_pin_level(PA30,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	gpio_set_pin_pull_mode(PA30,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(PA30,
	                      // <y> Pin function
	                      // <id> pad_function
	                      // <i> Auto : use driver pinmux if signal is imported by driver, else turn off function
	                      // <MUX_PA30C_HSMCI_MCDA0"> Auto
	                      // <GPIO_PIN_FUNCTION_OFF"> Off
	                      // <GPIO_PIN_FUNCTION_A"> A
	                      // <GPIO_PIN_FUNCTION_B"> B
	                      // <GPIO_PIN_FUNCTION_C"> C
	                      // <GPIO_PIN_FUNCTION_D"> D
	                      MUX_PA30C_HSMCI_MCDA0);

	gpio_set_pin_direction(PA31,
	                       // <y> Pin direction
	                       // <id> pad_direction
	                       // <GPIO_DIRECTION_OFF"> Off
	                       // <GPIO_DIRECTION_IN"> In
	                       // <GPIO_DIRECTION_OUT"> Out
	                       GPIO_DIRECTION_OUT);

	gpio_set_pin_level(PA31,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	gpio_set_pin_pull_mode(PA31,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(PA31,
	                      // <y> Pin function
	                      // <id> pad_function
	                      // <i> Auto : use driver pinmux if signal is imported by driver, else turn off function
	                      // <MUX_PA31C_HSMCI_MCDA1"> Auto
	                      // <GPIO_PIN_FUNCTION_OFF"> Off
	                      // <GPIO_PIN_FUNCTION_A"> A
	                      // <GPIO_PIN_FUNCTION_B"> B
	                      // <GPIO_PIN_FUNCTION_C"> C
	                      // <GPIO_PIN_FUNCTION_D"> D
	                      MUX_PA31C_HSMCI_MCDA1);

	gpio_set_pin_direction(PA26,
	                       // <y> Pin direction
	                       // <id> pad_direction
	                       // <GPIO_DIRECTION_OFF"> Off
	                       // <GPIO_DIRECTION_IN"> In
	                       // <GPIO_DIRECTION_OUT"> Out
	                       GPIO_DIRECTION_OUT);

	gpio_set_pin_level(PA26,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	gpio_set_pin_pull_mode(PA26,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(PA26,
	                      // <y> Pin function
	                      // <id> pad_function
	                      // <i> Auto : use driver pinmux if signal is imported by driver, else turn off function
	                      // <MUX_PA26C_HSMCI_MCDA2"> Auto
	                      // <GPIO_PIN_FUNCTION_OFF"> Off
	                      // <GPIO_PIN_FUNCTION_A"> A
	                      // <GPIO_PIN_FUNCTION_B"> B
	                      // <GPIO_PIN_FUNCTION_C"> C
	                      // <GPIO_PIN_FUNCTION_D"> D
	                      MUX_PA26C_HSMCI_MCDA2);

	gpio_set_pin_direction(PA27,
	                       // <y> Pin direction
	                       // <id> pad_direction
	                       // <GPIO_DIRECTION_OFF"> Off
	                       // <GPIO_DIRECTION_IN"> In
	                       // <GPIO_DIRECTION_OUT"> Out
	                       GPIO_DIRECTION_OUT);

	gpio_set_pin_level(PA27,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	gpio_set_pin_pull_mode(PA27,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(PA27,
	                      // <y> Pin function
	                      // <id> pad_function
	                      // <i> Auto : use driver pinmux if signal is imported by driver, else turn off function
	                      // <MUX_PA27C_HSMCI_MCDA3"> Auto
	                      // <GPIO_PIN_FUNCTION_OFF"> Off
	                      // <GPIO_PIN_FUNCTION_A"> A
	                      // <GPIO_PIN_FUNCTION_B"> B
	                      // <GPIO_PIN_FUNCTION_C"> C
	                      // <GPIO_PIN_FUNCTION_D"> D
	                      MUX_PA27C_HSMCI_MCDA3);
}

void IO_BUS_CLOCK_init(void)
{
	_pmc_enable_periph_clock(ID_HSMCI);
}

void IO_BUS_init(void)
{
	IO_BUS_CLOCK_init();
	mci_sync_init(&IO_BUS, HSMCI);
	IO_BUS_PORT_init();
}

void CALENDER_INTERFACE_CLOCK_init(void)
{
}

void CALENDER_INTERFACE_init(void)
{
	CALENDER_INTERFACE_CLOCK_init();
	calendar_init(&CALENDER_INTERFACE, RTC);
}

void DAC_SPI_PORT_init(void)
{

	gpio_set_pin_function(PC26, MUX_PC26C_SPI1_MISO);

	gpio_set_pin_function(PC27, MUX_PC27C_SPI1_MOSI);

	gpio_set_pin_function(PC24, MUX_PC24C_SPI1_SPCK);
}

void DAC_SPI_CLOCK_init(void)
{
	_pmc_enable_periph_clock(ID_SPI1);
}

void DAC_SPI_init(void)
{
	DAC_SPI_CLOCK_init();
	spi_m_async_set_func_ptr(&DAC_SPI, _spi_get_spi_m_async());
	spi_m_async_init(&DAC_SPI, SPI1);
	DAC_SPI_PORT_init();
}

/**
 * \brief Timer initialization function
 *
 * Enables Timer peripheral, clocks and initializes Timer driver
 */
static void VTIMER_init(void)
{
	_pmc_enable_periph_clock(ID_TC0_CHANNEL0);
	timer_init(&VTIMER, TC0, _tc_get_timer());
}

void LOG_TIMER_PORT_init(void)
{
}

void LOG_TIMER_CLOCK_init(void)
{
	_pmc_enable_periph_clock(ID_TC1_CHANNEL0);
}

void TIMERS_I2C_PORT_init(void)
{

	gpio_set_pin_function(PA4, MUX_PA4A_TWIHS0_TWCK0);

	gpio_set_pin_function(PA3, MUX_PA3A_TWIHS0_TWD0);
}

void TIMERS_I2C_CLOCK_init(void)
{
	_pmc_enable_periph_clock(ID_TWIHS0);
}

void TIMERS_I2C_init(void)
{
	TIMERS_I2C_CLOCK_init();

	i2c_m_sync_init(&TIMERS_I2C, TWIHS0);

	TIMERS_I2C_PORT_init();
}

void ADC_I2C_PORT_init(void)
{

	gpio_set_pin_function(PD28, MUX_PD28C_TWIHS2_TWCK2);

	gpio_set_pin_function(PD27, MUX_PD27C_TWIHS2_TWD2);
}

void ADC_I2C_CLOCK_init(void)
{
	_pmc_enable_periph_clock(ID_TWIHS2);
}

void ADC_I2C_init(void)
{
	ADC_I2C_CLOCK_init();

	i2c_m_sync_init(&ADC_I2C, TWIHS2);

	ADC_I2C_PORT_init();
}

/**
 * \brief USART Clock initialization function
 *
 * Enables register interface and peripheral clock
 */
void QC_UART_CLOCK_init()
{
	_pmc_enable_periph_clock(ID_UART0);
}

/**
 * \brief USART pinmux initialization function
 *
 * Set each required pin to USART functionality
 */
void QC_UART_PORT_init()
{

	gpio_set_pin_function(PA9, MUX_PA9A_UART0_URXD0);

	gpio_set_pin_function(PA10, MUX_PA10A_UART0_UTXD0);
}

/**
 * \brief USART initialization function
 *
 * Enables USART peripheral, clocks and initializes USART driver
 */
void QC_UART_init(void)
{
	QC_UART_CLOCK_init();
	usart_async_init(&QC_UART, UART0, QC_UART_buffer, QC_UART_BUFFER_SIZE, _uart_get_usart_async());
	QC_UART_PORT_init();
}

/**
 * \brief USART Clock initialization function
 *
 * Enables register interface and peripheral clock
 */
void HVPS_UART_CLOCK_init()
{
	_pmc_enable_periph_clock(ID_UART1);
}

/**
 * \brief USART pinmux initialization function
 *
 * Set each required pin to USART functionality
 */
void HVPS_UART_PORT_init()
{

	gpio_set_pin_function(PA5, MUX_PA5C_UART1_URXD1);

	gpio_set_pin_function(PA6, MUX_PA6C_UART1_UTXD1);
}

/**
 * \brief USART initialization function
 *
 * Enables USART peripheral, clocks and initializes USART driver
 */
void HVPS_UART_init(void)
{
	HVPS_UART_CLOCK_init();
	usart_async_init(&HVPS_UART, UART1, HVPS_UART_buffer, HVPS_UART_BUFFER_SIZE, _uart_get_usart_async());
	HVPS_UART_PORT_init();
}

/**
 * \brief USART Clock initialization function
 *
 * Enables register interface and peripheral clock
 */
void HB_UART_CLOCK_init()
{
	_pmc_enable_periph_clock(ID_UART2);
}

/**
 * \brief USART pinmux initialization function
 *
 * Set each required pin to USART functionality
 */
void HB_UART_PORT_init()
{

	gpio_set_pin_function(PD25, MUX_PD25C_UART2_URXD2);

	gpio_set_pin_function(PD26, MUX_PD26C_UART2_UTXD2);
}

/**
 * \brief USART initialization function
 *
 * Enables USART peripheral, clocks and initializes USART driver
 */
void HB_UART_init(void)
{
	HB_UART_CLOCK_init();
	usart_async_init(&HB_UART, UART2, HB_UART_buffer, HB_UART_BUFFER_SIZE, _uart_get_usart_async());
	HB_UART_PORT_init();
}

/**
 * \brief USART Clock initialization function
 *
 * Enables register interface and peripheral clock
 */
void PLT_UART_CLOCK_init()
{
	_pmc_enable_periph_clock(ID_UART4);
}

/**
 * \brief USART pinmux initialization function
 *
 * Set each required pin to USART functionality
 */
void PLT_UART_PORT_init()
{

	gpio_set_pin_function(PD18, MUX_PD18C_UART4_URXD4);

	gpio_set_pin_function(PD19, MUX_PD19C_UART4_UTXD4);
}

/**
 * \brief USART initialization function
 *
 * Enables USART peripheral, clocks and initializes USART driver
 */
void PLT_UART_init(void)
{
	PLT_UART_CLOCK_init();
	usart_async_init(&PLT_UART, UART4, PLT_UART_buffer, PLT_UART_BUFFER_SIZE, _uart_get_usart_async());
	PLT_UART_PORT_init();
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

void MACIF_PORT_init(void)
{

	gpio_set_pin_function(PD8, MUX_PD8A_GMAC_GMDC);

	gpio_set_pin_function(PD9, MUX_PD9A_GMAC_GMDIO);

	gpio_set_pin_function(PD5, MUX_PD5A_GMAC_GRX0);

	gpio_set_pin_function(PD6, MUX_PD6A_GMAC_GRX1);

	gpio_set_pin_function(PD4, MUX_PD4A_GMAC_GRXDV);

	gpio_set_pin_function(PD7, MUX_PD7A_GMAC_GRXER);

	gpio_set_pin_function(PD2, MUX_PD2A_GMAC_GTX0);

	gpio_set_pin_function(PD3, MUX_PD3A_GMAC_GTX1);

	gpio_set_pin_function(PD0, MUX_PD0A_GMAC_GTXCK);

	gpio_set_pin_function(PD1, MUX_PD1A_GMAC_GTXEN);
}

void MACIF_CLOCK_init(void)
{
	_pmc_enable_periph_clock(ID_GMAC);
}

void MACIF_init(void)
{
	MACIF_CLOCK_init();
	mac_async_init(&MACIF, GMAC);
	MACIF_PORT_init();
}

void system_init(void)
{
	init_mcu();

	_pmc_enable_periph_clock(ID_PIOA);

	_pmc_enable_periph_clock(ID_PIOB);

	_pmc_enable_periph_clock(ID_PIOC);

	_pmc_enable_periph_clock(ID_PIOD);

	/* Disable Watchdog */
	hri_wdt_set_MR_WDDIS_bit(WDT);

	/* GPIO on PA0 */

	gpio_set_pin_level(IO_EXT_WD_RST,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_EXT_WD_RST, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_EXT_WD_RST, GPIO_PIN_FUNCTION_OFF);

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

	/* GPIO on PA2 */

	gpio_set_pin_level(IO_TIMERS_STARTn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_TIMERS_STARTn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_TIMERS_STARTn, GPIO_PIN_FUNCTION_OFF);

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

	/* GPIO on PA11 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_HVPS_RDYn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_HVPS_RDYn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_HVPS_RDYn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA12 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_HVPS_WARNING, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_HVPS_WARNING,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_HVPS_WARNING, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA13 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_AC_FAULT, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_AC_FAULT,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_AC_FAULT, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA14 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_HV_ON, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_HV_ON,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_HV_ON, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA15 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_GRID_OFF, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_GRID_OFF,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_GRID_OFF, GPIO_PIN_FUNCTION_OFF);

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

	/* GPIO on PA19 */

	gpio_set_pin_level(IO_HS_FAN_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_HS_FAN_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_HS_FAN_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA20 */

	gpio_set_pin_level(IO_CB_FAN_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_CB_FAN_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_CB_FAN_EN, GPIO_PIN_FUNCTION_OFF);

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

	/* GPIO on PA23 */

	gpio_set_pin_level(IO_ION_PUMP_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_ION_PUMP_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_ION_PUMP_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA24 */

	gpio_set_pin_level(IO_ION_REPELLER_EN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_ION_REPELLER_EN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_ION_REPELLER_EN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PA29 */

	// Set pin direction to input
	gpio_set_pin_direction(CARD_DETECT_0, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(CARD_DETECT_0,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(CARD_DETECT_0, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PB2 */

	gpio_set_pin_level(IO_COIL_X_DIRn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_COIL_X_DIRn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_COIL_X_DIRn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PB3 */

	gpio_set_pin_level(IO_COIL_Y_DIRn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_COIL_Y_DIRn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_COIL_Y_DIRn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC0 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_DOOR_CLOSED, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_DOOR_CLOSED,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_DOOR_CLOSED, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC1 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_DRIVE_SYS_LOCKED, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_DRIVE_SYS_LOCKED,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_DRIVE_SYS_LOCKED, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC2 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_BASE_ESTOPn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_BASE_ESTOPn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_BASE_ESTOPn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC3 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_REMOTE_ESTOPn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_REMOTE_ESTOPn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_REMOTE_ESTOPn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC4 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_KUKA_FAULT_1n, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_KUKA_FAULT_1n,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_KUKA_FAULT_1n, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC5 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_KUKA_FAULT_2n, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_KUKA_FAULT_2n,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_KUKA_FAULT_2n, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC6 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_WATER_LEVEL, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_WATER_LEVEL,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_WATER_LEVEL, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC7 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_ION_PUMP_HVON, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_ION_PUMP_HVON,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_ION_PUMP_HVON, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC8 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_TIMER_FAULT_1n, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_TIMER_FAULT_1n,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_TIMER_FAULT_1n, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC9 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_TIMER_FAULT2n, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_TIMER_FAULT2n,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_TIMER_FAULT2n, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC10 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_HVPS_FAULTn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_HVPS_FAULTn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_HVPS_FAULTn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC11 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_COOLER_FAULTn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_COOLER_FAULTn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_COOLER_FAULTn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC12 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_WATER_TEMP_FAULTn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_WATER_TEMP_FAULTn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_WATER_TEMP_FAULTn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC13 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_WD_FAULTn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_WD_FAULTn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_WD_FAULTn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC14 */

	gpio_set_pin_level(IO_MCU_FAULTn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_MCU_FAULTn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_MCU_FAULTn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC15 */

	// Set pin direction to input
	gpio_set_pin_direction(SPARE_INTERLOCK_1, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(SPARE_INTERLOCK_1,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(SPARE_INTERLOCK_1, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC16 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_MASTER_FAULTn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_MASTER_FAULTn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_MASTER_FAULTn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC17 */

	gpio_set_pin_level(IO_CLEAR_FAULT,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_CLEAR_FAULT, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_CLEAR_FAULT, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC18 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_REMOTE_KEY, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_REMOTE_KEY,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_REMOTE_KEY, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC19 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_BASE_KEY, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_BASE_KEY,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_BASE_KEY, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC20 */

	gpio_set_pin_level(IO_COIL_DAC_LDACn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_COIL_DAC_LDACn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_COIL_DAC_LDACn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC21 */

	gpio_set_pin_level(IO_COIL_DAC_CLRn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_COIL_DAC_CLRn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_COIL_DAC_CLRn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC22 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_COIL_DAC_RDYn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_COIL_DAC_RDYn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_COIL_DAC_RDYn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC23 */

	gpio_set_pin_level(IO_COIL_DAC_CSn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_COIL_DAC_CSn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_COIL_DAC_CSn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC28 */

	gpio_set_pin_level(IO_FAN_DAC_LDACn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_FAN_DAC_LDACn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_FAN_DAC_LDACn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC29 */

	gpio_set_pin_level(IO_FAN_DAC_CLRn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_FAN_DAC_CLRn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_FAN_DAC_CLRn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC30 */

	// Set pin direction to input
	gpio_set_pin_direction(IO_FAN_DAC_RDYn, GPIO_DIRECTION_IN);

	gpio_set_pin_pull_mode(IO_FAN_DAC_RDYn,
	                       // <y> Pull configuration
	                       // <id> pad_pull_config
	                       // <GPIO_PULL_OFF"> Off
	                       // <GPIO_PULL_UP"> Pull-up
	                       // <GPIO_PULL_DOWN"> Pull-down
	                       GPIO_PULL_OFF);

	gpio_set_pin_function(IO_FAN_DAC_RDYn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PC31 */

	gpio_set_pin_level(IO_FAN_DAC_CSn,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_FAN_DAC_CSn, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_FAN_DAC_CSn, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD10 */

	gpio_set_pin_level(PHY_RESET_PIN,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(PHY_RESET_PIN, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(PHY_RESET_PIN, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD12 */

	gpio_set_pin_level(IO_LED1,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_LED1, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_LED1, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD13 */

	gpio_set_pin_level(IO_LED2,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_LED2, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_LED2, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD14 */

	gpio_set_pin_level(IO_LED3,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_LED3, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_LED3, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD15 */

	gpio_set_pin_level(IO_LED4,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_LED4, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_LED4, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD16 */

	gpio_set_pin_level(IO_LED5,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_LED5, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_LED5, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD17 */

	gpio_set_pin_level(IO_LED6,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   true);

	// Set pin direction to output
	gpio_set_pin_direction(IO_LED6, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_LED6, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD29 */

	gpio_set_pin_level(IO_REMOTE_LED_1,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_REMOTE_LED_1, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_REMOTE_LED_1, GPIO_PIN_FUNCTION_OFF);

	/* GPIO on PD30 */

	gpio_set_pin_level(IO_REMOTE_LED_2,
	                   // <y> Initial level
	                   // <id> pad_initial_level
	                   // <false"> Low
	                   // <true"> High
	                   false);

	// Set pin direction to output
	gpio_set_pin_direction(IO_REMOTE_LED_2, GPIO_DIRECTION_OUT);

	gpio_set_pin_function(IO_REMOTE_LED_2, GPIO_PIN_FUNCTION_OFF);

	FLASH_0_init();

	IO_BUS_init();

	CALENDER_INTERFACE_init();

	DAC_SPI_init();
	VTIMER_init();

	LOG_TIMER_CLOCK_init();
	LOG_TIMER_PORT_init();
	LOG_TIMER_init();

	TIMERS_I2C_init();

	ADC_I2C_init();
	QC_UART_init();
	HVPS_UART_init();
	HB_UART_init();
	PLT_UART_init();
	FTDI_UART_init();

	MACIF_init();
}
