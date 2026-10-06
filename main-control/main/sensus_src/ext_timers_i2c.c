/*
 * External timer TWIHS transport for the SAM target.
 */
#include <atmel_start.h>
#include <string.h>

#include "faults.h"
#include "ext_timers_i2c.h"

static uint8_t primary_timer_rx_buf[TIMER_RX_SIZE];
static uint8_t secondary_timer_rx_buf[TIMER_RX_SIZE];
static const uint8_t *timer_tx_buf;
static volatile bool timer_write_cycle;
static volatile uint32_t timer_addr;
static volatile uint32_t timer_tx_idx;
static volatile uint32_t timer_rx_idx;

static void timer_rx(void);
static void save_timer_rx(uint8_t val);
static void timer_tx(void);
static void go_to_next_timer(void);
static void timer_transmission_complete(void);
static void report_timer_nack(void);

void init_ext_timers_i2c(void)
{
	i2c_m_sync_enable(&TIMERS_I2C);
	hri_twihs_set_IMR_NACK_bit(TIMERS_I2C.device.hw);
	NVIC_EnableIRQ(TWIHS0_IRQn);

	memset(primary_timer_rx_buf, 0, sizeof(primary_timer_rx_buf));
	primary_timer_rx_buf[TIMER_RX_CHECK] = 0xFF;
	memset(secondary_timer_rx_buf, 0, sizeof(secondary_timer_rx_buf));
	secondary_timer_rx_buf[TIMER_RX_CHECK] = 0xFF;

	timer_addr = PRIMARY_TIMER_ADDR;
	timer_tx_idx = 0;
	timer_rx_idx = 0;
	timer_write_cycle = true;
}

uint8_t *ext_timers_i2c_get_rx_buffer(bool primary)
{
	return primary ? primary_timer_rx_buf : secondary_timer_rx_buf;
}

void ext_timers_i2c_start_transfer(const uint8_t tx_buf[TIMER_TX_SIZE])
{
	timer_tx_buf = tx_buf;
	hri_twihs_write_MMR_reg(TIMERS_I2C.device.hw, TWIHS_MMR_DADR(timer_addr));
	hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXRDY);
	timer_write_cycle = true;
}

/* This function is called from the TWIHS interrupt handler; keep it short. */
static void timer_rx(void)
{
	uint32_t read_val = hri_twihs_read_RHR_reg(TIMERS_I2C.device.hw);
	save_timer_rx((uint8_t)read_val);
	timer_rx_idx++;

	if(timer_rx_idx == TIMER_RX_SIZE - 1)
	{
		hri_twihs_write_CR_reg(TIMERS_I2C.device.hw, TWIHS_CR_STOP);
	}
	else if(timer_rx_idx >= TIMER_RX_SIZE)
	{
		timer_rx_idx = 0;
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_RXRDY);
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXCOMP);
	}
}

/* This function is called from the TWIHS interrupt handler; keep it short. */
static void save_timer_rx(uint8_t val)
{
	if(timer_rx_idx >= TIMER_RX_SIZE)
	{
		return;
	}

	ext_timers_i2c_get_rx_buffer(timer_addr == PRIMARY_TIMER_ADDR)[timer_rx_idx] = val;
}

/* This function is called from the TWIHS interrupt handler; keep it short. */
static void timer_tx(void)
{
	if(timer_tx_idx < TIMER_TX_SIZE)
	{
		hri_twihs_write_THR_reg(TIMERS_I2C.device.hw, timer_tx_buf[timer_tx_idx]);
		timer_tx_idx++;
	}
	else
	{
		timer_tx_idx = 0;
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_TXRDY);
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXCOMP);
		hri_twihs_write_CR_reg(TIMERS_I2C.device.hw, TWIHS_CR_STOP);
	}
}

/* This function is called from the TWIHS interrupt handler; keep it short. */
static void timer_transmission_complete(void)
{
	if(timer_write_cycle)
	{
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_RXRDY);
		hri_twihs_write_MMR_reg(TIMERS_I2C.device.hw, TWIHS_MMR_DADR(timer_addr) | TWIHS_MMR_MREAD);
		hri_twihs_write_CR_reg(TIMERS_I2C.device.hw, TWIHS_CR_START);
		timer_write_cycle = false;
	}
	else
	{
		go_to_next_timer();
	}
}

/* This function is called from the TWIHS interrupt handler; keep it short. */
static void go_to_next_timer(void)
{
	timer_tx_idx = 0;
	timer_rx_idx = 0;

	if(timer_addr == PRIMARY_TIMER_ADDR)
	{
		timer_addr = SECONDARY_TIMER_ADDR;
		hri_twihs_write_MMR_reg(TIMERS_I2C.device.hw, TWIHS_MMR_DADR(timer_addr));
		hri_twihs_set_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IER_TXRDY);
		timer_write_cycle = true;
	}
	else
	{
		timer_addr = PRIMARY_TIMER_ADDR;
		ext_timers_i2c_transfer_complete();
	}
}

static void report_timer_nack(void)
{
	if(timer_addr == PRIMARY_TIMER_ADDR)
	{
		report_typed_fault(FAULT_TIMER_COMM, "Primary timer returned NACK.");
	}
	else
	{
		report_typed_fault(FAULT_TIMER_COMM, "Secondary timer returned NACK.");
	}
}

/* Interrupt handler; keep it short. */
void TWIHS0_Handler(void)
{
	uint32_t sr = hri_twihs_read_SR_reg(TIMERS_I2C.device.hw) & hri_twihs_read_IMR_reg(TIMERS_I2C.device.hw);

	if(sr & TWIHS_SR_NACK)
	{
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		report_timer_nack();
		go_to_next_timer();
	}
	else if(sr & TWIHS_SR_TXCOMP)
	{
		hri_twihs_clear_IMR_reg(TIMERS_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		timer_transmission_complete();
	}
	else if(sr & TWIHS_SR_TXRDY)
	{
		timer_tx();
	}
	else if(sr & TWIHS_SR_RXRDY)
	{
		timer_rx();
	}
}
