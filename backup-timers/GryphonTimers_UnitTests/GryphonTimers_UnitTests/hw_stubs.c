#include "atmel_start.h"

/* these are owned by ut_globals.c */
extern volatile uint32_t g_stub_ovf_clear_calls;
extern volatile uint32_t g_stub_disable_calls;
extern volatile uint32_t g_stub_gpio_calls;
extern volatile uint32_t g_stub_count_reg;
extern volatile bool gpio_fault_state;

struct _i2c_s_async_device I2C_0 = {0};

void atmel_start_init(void)
{
}

void system_init(void)
{
}

void driver_init(void)
{
}

void gpio_set_pin_level(uint32_t pin, bool level)
{
	(void)pin;
	gpio_fault_state = level;
	g_stub_gpio_calls++;
}

bool gpio_get_pin_level(uint32_t pin)
{
	(void)pin;
	return false;
}

void gpio_set_pin_direction(uint32_t pin, uint32_t direction)
{
	(void)pin;
	(void)direction;
}

void gpio_set_pin_function(uint32_t pin, uint32_t function)
{
	(void)pin;
	(void)function;
}

void gpio_set_pin_pull_mode(uint32_t pin, uint32_t pull_mode)
{
	(void)pin;
	(void)pull_mode;
}

void ext_irq_register(uint32_t pin, void (*callback)(void))
{
	(void)pin;
	(void)callback;
}

void NVIC_EnableIRQ(int irq)
{
	(void)irq;
}

void NVIC_DisableIRQ(int irq)
{
	(void)irq;
}

void hri_tc_clear_interrupt_OVF_bit(void* hw)
{
	(void)hw;
	g_stub_ovf_clear_calls++;
}

void hri_tc_write_CTRLA_ENABLE_bit(void* hw, uint8_t value)
{
	(void)hw;
	(void)value;
	g_stub_disable_calls++;
}

uint32_t hri_tccount32_read_COUNT_reg(void* hw)
{
	(void)hw;
	return g_stub_count_reg;
}

void hri_tccount32_write_COUNT_reg(void* hw, uint32_t value)
{
	(void)hw;
	g_stub_count_reg = value;
}

void i2c_s_init(struct _i2c_s_async_device* hw, void* desc)
{
	(void)hw;
	(void)desc;
}

void i2c_s_enable(struct _i2c_s_async_device* hw)
{
	(void)hw;
}

int32_t i2c_s_set_addr(struct _i2c_s_async_device* hw, uint8_t addr)
{
	(void)hw;
	(void)addr;
	return 0;
}

int32_t i2c_s_get_status(struct _i2c_s_async_device* hw)
{
	(void)hw;
	return I2C_S_RX_COMPLETE;
}

int32_t i2c_s_read_byte(struct _i2c_s_async_device* hw)
{
	(void)hw;
	return 0;
}

void i2c_s_write_byte(struct _i2c_s_async_device* hw, uint8_t data)
{
	(void)hw;
	(void)data;
}

void i2c_s_async_flush_rx_buffer(struct _i2c_s_async_device* hw)
{
	(void)hw;
}

int32_t io_read(struct io_descriptor* io, uint8_t* buf, uint16_t len)
{
	uint16_t i;
	(void)io;

	for (i = 0; i < len; i++) {
		buf[i] = 0;
	}

	return (int32_t)len;
}

int32_t io_write(struct io_descriptor* io, const uint8_t* buf, uint16_t len)
{
	(void)io;
	(void)buf;
	return (int32_t)len;
}