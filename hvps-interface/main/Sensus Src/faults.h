#ifndef FAULTS_H
#define FAULTS_H

#include <stdint.h>

/*
 * report_fault()
 *
 * Called with a mask of fault bits that just went active (typically
 * rose & hvps_fault_mask from report_io_state()). Latches them into
 * the active fault mask and forwards the cause to the ctrl board.
 */
void report_fault(uint32_t new_fault_bits);

/*
 * clear_fault()
 *
 * Clears the given fault bit(s) from the active fault mask. Call from
 * wherever fault-reset logic lives (button press, ctrl board command,
 * auto-clear on de-assert, etc).
 */
void clear_fault(uint32_t fault_bits);
void clear_all_faults(void);

/*
 * get_active_faults()
 *
 * Returns the current latched fault mask.
 */
uint32_t get_active_faults(void);

#endif /* FAULTS_H */