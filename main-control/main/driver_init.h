/*
 * Code generated from Atmel Start.
 *
 * This file will be overwritten when reconfiguring your Atmel Start project.
 * Please copy examples or other code you want to keep to a separate file
 * to avoid losing it when reconfiguring.
 */
#ifndef DRIVER_INIT_H_INCLUDED
#define DRIVER_INIT_H_INCLUDED

#include "atmel_start_pins.h"

#ifdef __cplusplus
extern "C" {
#endif

#include <hal_atomic.h>
#include <hal_delay.h>
#include <hal_gpio.h>
#include <hal_init.h>
#include <hal_io.h>
#include <hal_sleep.h>

#include <hal_flash.h>

#include <hal_mci_sync.h>

#include <hal_calendar.h>

#include <hal_spi_m_async.h>
#include <hal_timer.h>
#include <tc_lite.h>
#include <hal_i2c_m_sync.h>

#include <hal_i2c_m_sync.h>
#include <hal_usart_async.h>
#include <hpl_uart_base.h>
#include <hal_usart_async.h>
#include <hpl_uart_base.h>
#include <hal_usart_async.h>
#include <hpl_uart_base.h>
#include <hal_usart_async.h>
#include <hpl_uart_base.h>
#include <hal_usart_async.h>

#include <hal_mac_async.h>

extern struct flash_descriptor FLASH_0;

extern struct mci_sync_desc IO_BUS;

extern struct calendar_descriptor CALENDER_INTERFACE;

extern struct spi_m_async_descriptor DAC_SPI;
extern struct timer_descriptor       VTIMER;

extern struct i2c_m_sync_desc TIMERS_I2C;

extern struct i2c_m_sync_desc        ADC_I2C;
extern struct usart_async_descriptor QC_UART;
extern struct usart_async_descriptor HVPS_UART;
extern struct usart_async_descriptor HB_UART;
extern struct usart_async_descriptor PLT_UART;
extern struct usart_async_descriptor FTDI_UART;

extern struct mac_async_descriptor MACIF;

void FLASH_0_init(void);
void FLASH_0_CLOCK_init(void);

void IO_BUS_PORT_init(void);
void IO_BUS_CLOCK_init(void);
void IO_BUS_init(void);

void CALENDER_INTERFACE_CLOCK_init(void);
void CALENDER_INTERFACE_init(void);

void DAC_SPI_PORT_init(void);
void DAC_SPI_CLOCK_init(void);
void DAC_SPI_init(void);

void LOG_TIMER_CLOCK_init(void);
void LOG_TIMER_PORT_init(void);

void TIMERS_I2C_CLOCK_init(void);
void TIMERS_I2C_init(void);
void TIMERS_I2C_PORT_init(void);

void ADC_I2C_CLOCK_init(void);
void ADC_I2C_init(void);
void ADC_I2C_PORT_init(void);

void QC_UART_PORT_init(void);
void QC_UART_CLOCK_init(void);
void QC_UART_init(void);
void QC_UART_example(void);

void HVPS_UART_PORT_init(void);
void HVPS_UART_CLOCK_init(void);
void HVPS_UART_init(void);
void HVPS_UART_example(void);

void HB_UART_PORT_init(void);
void HB_UART_CLOCK_init(void);
void HB_UART_init(void);
void HB_UART_example(void);

void PLT_UART_PORT_init(void);
void PLT_UART_CLOCK_init(void);
void PLT_UART_init(void);
void PLT_UART_example(void);

void FTDI_UART_PORT_init(void);
void FTDI_UART_CLOCK_init(void);
void FTDI_UART_init(void);

void MACIF_CLOCK_init(void);
void MACIF_init(void);
void MACIF_PORT_init(void);

/**
 * \brief Perform system initialization, initialize pins and clocks for
 * peripherals
 */
void system_init(void);

#ifdef __cplusplus
}
#endif
#endif // DRIVER_INIT_H_INCLUDED
