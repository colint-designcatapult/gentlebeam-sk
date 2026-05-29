#ifndef ATMEL_START_H
#define ATMEL_START_H

#include <stdint.h>
#include <stdbool.h>

/* -------------------- */
/* Generic fake GPIO    */
/* -------------------- */
#define GPIO(port, pin)   (((port) << 8) | (pin))

#define GPIO_PORTA        0
#define GPIO_PORTB        1
#define GPIO_PORTC        2

#define GPIO_DIRECTION_IN   0
#define GPIO_DIRECTION_OUT  1

/* -------------------- */
/* Fake pins used by production code */
/* -------------------- */
#define PIN_PA04   GPIO(GPIO_PORTA, 4)
#define PIN_PA05   GPIO(GPIO_PORTA, 5)
#define PIN_PA06   GPIO(GPIO_PORTA, 6)
#define PIN_PA07   GPIO(GPIO_PORTA, 7)
#define PIN_PA08   GPIO(GPIO_PORTA, 8)
#define PIN_PA09   GPIO(GPIO_PORTA, 9)
#define PIN_PA10   GPIO(GPIO_PORTA, 10)
#define PIN_PA11   GPIO(GPIO_PORTA, 11)
#define PIN_PA14   GPIO(GPIO_PORTA, 14)
#define PIN_PA15   GPIO(GPIO_PORTA, 15)
#define PIN_PA16   GPIO(GPIO_PORTA, 16)
#define PIN_PA17   GPIO(GPIO_PORTA, 17)
#define PIN_PA22   GPIO(GPIO_PORTA, 22)
#define PIN_PA23   GPIO(GPIO_PORTA, 23)
#define PIN_PA24   GPIO(GPIO_PORTA, 24)
#define PIN_PA25   GPIO(GPIO_PORTA, 25)
#define PIN_PA30   GPIO(GPIO_PORTA, 30)
#define PIN_PA31   GPIO(GPIO_PORTA, 31)

/* -------------------- */
/* Fake peripheral handles */
/* -------------------- */
#define TC1      ((void*)0x1001)

/* -------------------- */
/* Fake register bit positions */
/* -------------------- */
#define TC_CTRLA_ENABLE_Pos  1

/* -------------------- */
/* Fake IRQ names */
/* -------------------- */
#define TC1_IRQn   0
#define EIC_IRQn   1

/* -------------------- */
/* Fake I2C status values */
/* -------------------- */
#define I2C_S_RX_COMPLETE   0
#define I2C_S_TX_PENDING    1

/* -------------------- */
/* Descriptor placeholders */
/* -------------------- */
struct _i2c_s_async_device {
	uint8_t dummy;
};

struct io_descriptor {
	uint8_t dummy;
};

extern struct _i2c_s_async_device I2C_0;

/* -------------------- */
/* Init functions */
/* -------------------- */
void atmel_start_init(void);
void system_init(void);
void driver_init(void);

/* -------------------- */
/* GPIO API */
/* -------------------- */
void gpio_set_pin_level(uint32_t pin, bool level);
bool gpio_get_pin_level(uint32_t pin);
void gpio_set_pin_direction(uint32_t pin, uint32_t direction);
void gpio_set_pin_function(uint32_t pin, uint32_t function);
void gpio_set_pin_pull_mode(uint32_t pin, uint32_t pull_mode);

/* -------------------- */
/* External interrupt API */
/* -------------------- */
void ext_irq_register(uint32_t pin, void (*callback)(void));

/* -------------------- */
/* NVIC API */
/* -------------------- */
void NVIC_EnableIRQ(int irq);
void NVIC_DisableIRQ(int irq);

/* -------------------- */
/* Timer HRI used by production code */
/* -------------------- */
void hri_tc_clear_interrupt_OVF_bit(void* hw);
void hri_tc_write_CTRLA_ENABLE_bit(void* hw, uint8_t value);
uint32_t hri_tccount32_read_COUNT_reg(void* hw);
void hri_tccount32_write_COUNT_reg(void* hw, uint32_t value);

/* -------------------- */
/* I2C API */
/* -------------------- */
void i2c_s_init(struct _i2c_s_async_device* hw, void* desc);
void i2c_s_enable(struct _i2c_s_async_device* hw);
int32_t i2c_s_set_addr(struct _i2c_s_async_device* hw, uint8_t addr);
int32_t i2c_s_get_status(struct _i2c_s_async_device* hw);
int32_t i2c_s_read_byte(struct _i2c_s_async_device* hw);
void i2c_s_write_byte(struct _i2c_s_async_device* hw, uint8_t data);
void i2c_s_async_flush_rx_buffer(struct _i2c_s_async_device* hw);

/* -------------------- */
/* IO API */
/* -------------------- */
int32_t io_read(struct io_descriptor* io, uint8_t* buf, uint16_t len);
int32_t io_write(struct io_descriptor* io, const uint8_t* buf, uint16_t len);

#endif