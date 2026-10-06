/*
 * External timer I2C transport boundary.
 *
 * The timer protocol and command scheduling live in ext_timers.c. Platform
 * implementations own the received packet buffers and complete each transfer
 * by calling ext_timers_i2c_transfer_complete().
 */
#ifndef EXT_TIMERS_I2C_H_
#define EXT_TIMERS_I2C_H_

#include <stdbool.h>
#include <stdint.h>

#include "ext_timers.h"

void init_ext_timers_i2c(void);
void ext_timers_i2c_start_transfer(const uint8_t tx_buf[TIMER_TX_SIZE]);
uint8_t *ext_timers_i2c_get_rx_buffer(bool primary);

/* Called by the transport after both timer transactions have completed. */
void ext_timers_i2c_transfer_complete(void);

#endif /* EXT_TIMERS_I2C_H_ */
