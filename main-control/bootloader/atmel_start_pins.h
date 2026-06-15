/*
 * Code generated from Atmel Start.
 *
 * This file will be overwritten when reconfiguring your Atmel Start project.
 * Please copy examples or other code you want to keep to a separate file
 * to avoid losing it when reconfiguring.
 */
#ifndef ATMEL_START_PINS_H_INCLUDED
#define ATMEL_START_PINS_H_INCLUDED

#include <hal_gpio.h>

// SAME70 has 4 pin functions

#define GPIO_PIN_FUNCTION_A 0
#define GPIO_PIN_FUNCTION_B 1
#define GPIO_PIN_FUNCTION_C 2
#define GPIO_PIN_FUNCTION_D 3

#define IO_INDICATORS_EN GPIO(GPIO_PORTA, 1)
#define IO_QC_EN GPIO(GPIO_PORTA, 8)
#define IO_HV_EN GPIO(GPIO_PORTA, 16)
#define IO_GRID_ENn GPIO(GPIO_PORTA, 17)
#define IO_EMISSION_EN GPIO(GPIO_PORTA, 18)
#define IO_PUMP_EN GPIO(GPIO_PORTA, 21)
#define PB0 GPIO(GPIO_PORTB, 0)
#define PB1 GPIO(GPIO_PORTB, 1)

#endif // ATMEL_START_PINS_H_INCLUDED
