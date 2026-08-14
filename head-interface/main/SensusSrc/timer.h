#ifndef SENSUSSRC_TIMER_H_
#define SENSUSSRC_TIMER_H_

#include <stdbool.h>
#include "main.h"
#include "stm32f4xx_hal.h"


extern volatile int32_t read_mag_ms;
extern volatile int32_t control_comm_ms;
extern volatile bool update_flow;
#if !defined (CALIBRATION_MODE)
extern volatile int32_t update_led_ms;
extern volatile int32_t collim_ms;
extern volatile bool button_process_ready;
extern volatile int32_t qc_reset_count_ms;
extern volatile int32_t led_ring_ms; 
#endif
void update_system_timers();


#endif /* SENSUSSRC_TIMER_H_ */
