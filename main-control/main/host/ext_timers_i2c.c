/*
 * In-memory external timer transport and simulator for host builds.
 */
#include <atmel_start.h>
#include <limits.h>
#include <string.h>

#include "hardware_backend.h"
#include "ext_timers_i2c.h"

static bool transfer_pending;

static GcbTimerModel *timer_model(bool primary)
{
	return primary ? &gIoModel.backup_timer1 : &gIoModel.backup_timer2;
}

static void update_rx_buffer(bool primary)
{
	GcbTimerModel *const model = timer_model(primary);
	uint8_t *const rx_buf = model->rx_buf;
	uint8_t checksum = 0xFF;

	rx_buf[TIMER_RX_STATE] = (uint8_t)model->state;
	memcpy(rx_buf + TIMER_RX_TIME_0, &model->time_raw, sizeof(model->time_raw));
	for (int i = 0; i < TIMER_RX_CHECK; i++) {
		checksum -= rx_buf[i];
	}
	rx_buf[TIMER_RX_CHECK] = model->checksum_corrupted ? checksum ^ 1u : checksum;
}

static void update_fault_pin(bool primary)
{
	if (!gIoModel.gpio.simulate) {
		return;
	}
	const GcbTimerModel *const model = timer_model(primary);

	gpio_set_pin_level(primary ? IO_TIMER_FAULT_1n : IO_TIMER_FAULT2n,
	                   model->state != TIMER_STATE_ELAPSED);
}

static bool valid_command(const uint8_t tx_buf[TIMER_TX_SIZE])
{
	uint8_t checksum = 0;

	for (int i = 0; i < TIMER_TX_SIZE; i++) {
		checksum += tx_buf[i];
	}
	return checksum == 0xFF;
}

static void apply_command(GcbTimerModel *model, const uint8_t tx_buf[TIMER_TX_SIZE])
{
	uint32_t time_raw;

	if (!model->simulate) {
		return;
	}

	switch (tx_buf[TIMER_TX_CMD]) {
	case TIMER_CMD_SET_TIME:
		if (model->state == TIMER_STATE_PAUSED || model->state == TIMER_STATE_CLEARED) {
			memcpy(&time_raw, tx_buf + TIMER_TX_TIME_0, sizeof(time_raw));
			model->time_raw = time_raw;
			model->tick_remainder = 0;
			model->state = TIMER_STATE_PAUSED;
		}
		break;
	case TIMER_CMD_PAUSE:
		if (model->state == TIMER_STATE_RUNNING && gpio_get_pin_level(IO_TIMERS_STARTn)) {
			model->state = TIMER_STATE_PAUSED;
		}
		break;
	case TIMER_CMD_CLEAR:
		if (model->state != TIMER_STATE_RUNNING) {
			model->time_raw = UINT32_MAX;
			model->tick_remainder = 0;
			model->state = TIMER_STATE_CLEARED;
		}
		break;
	case TIMER_CMD_READ:
	default:
		break;
	}
}

void init_ext_timers_i2c(void)
{
	transfer_pending = false;
	memset(gIoModel.backup_timer1.rx_buf, 0, sizeof(gIoModel.backup_timer1.rx_buf));
	memset(gIoModel.backup_timer2.rx_buf, 0, sizeof(gIoModel.backup_timer2.rx_buf));

	gIoModel.backup_timer1.state = TIMER_STATE_CLEARED;
	gIoModel.backup_timer1.time_raw = UINT32_MAX;
	gIoModel.backup_timer1.tick_remainder = 0;
	gIoModel.backup_timer2.state = TIMER_STATE_CLEARED;
	gIoModel.backup_timer2.time_raw = UINT32_MAX;
	gIoModel.backup_timer2.tick_remainder = 0;

	gpio_set_pin_level(IO_TIMERS_STARTn, true);
	update_fault_pin(true);
	update_fault_pin(false);
	update_rx_buffer(true);
	update_rx_buffer(false);
}

uint8_t *ext_timers_i2c_get_rx_buffer(bool primary)
{
	return timer_model(primary)->rx_buf;
}

static void complete_transfer_if_ready(void)
{
	if (!transfer_pending || gIoModel.backup_timer1.response_suppressed ||
	    gIoModel.backup_timer2.response_suppressed) {
		return;
	}

	if (gIoModel.backup_timer1.simulate) {
		update_rx_buffer(true);
	}
	if (gIoModel.backup_timer2.simulate) {
		update_rx_buffer(false);
	}
	transfer_pending = false;
	ext_timers_i2c_transfer_complete();
}

void ext_timers_i2c_start_transfer(const uint8_t tx_buf[TIMER_TX_SIZE])
{
	if (valid_command(tx_buf)) {
		apply_command(&gIoModel.backup_timer1, tx_buf);
		apply_command(&gIoModel.backup_timer2, tx_buf);
	}

	update_fault_pin(true);
	update_fault_pin(false);
	/* A silent slave holds the transport pending: unchanged feedback alone
	 * must not count as a completed response and reset the firmware watchdog. */
	transfer_pending = true;
	complete_transfer_if_ready();
}

static void tick_timer(GcbTimerModel *model)
{
	uint32_t elapsed_ticks;

	if (!model->simulate) {
		return;
	}
	if (model->state == TIMER_STATE_PAUSED && !gpio_get_pin_level(IO_TIMERS_STARTn)) {
		model->state = TIMER_STATE_RUNNING;
	}
	if (model->state != TIMER_STATE_RUNNING) {
		return;
	}

	model->tick_remainder += TICKS_PER_SECOND;
	elapsed_ticks = model->tick_remainder / 1000u;
	model->tick_remainder %= 1000u;
	if (elapsed_ticks >= model->time_raw) {
		model->time_raw = 0;
		model->state = TIMER_STATE_ELAPSED;
	} else {
		model->time_raw -= elapsed_ticks;
	}
}

void ext_timers_i2c_tick_1ms(void)
{
	tick_timer(&gIoModel.backup_timer1);
	tick_timer(&gIoModel.backup_timer2);
	update_fault_pin(true);
	update_fault_pin(false);
	if (gIoModel.backup_timer1.simulate && !gIoModel.backup_timer1.response_suppressed) {
		update_rx_buffer(true);
	}
	if (gIoModel.backup_timer2.simulate && !gIoModel.backup_timer2.response_suppressed) {
		update_rx_buffer(false);
	}
	/* Resume the outstanding transaction when the physical response returns. */
	complete_transfer_if_ready();
}
