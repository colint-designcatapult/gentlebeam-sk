#include "stdbool.h"

#ifndef TIMERS_H_
#define TIMERS_H_

extern volatile bool toggle_grid_clock;


extern volatile int32_t int_adc_ms;
extern volatile int32_t comm_ms;
extern volatile int32_t kv_ramp_ms;
extern volatile int32_t fil_ramp_ms;
extern volatile int32_t lock_timer_ms;
extern volatile int32_t io_ms;
extern volatile int32_t grid_ms;

extern volatile uint32_t runtime_ms;

void update_timers();
void enable_grid_clock();
void disable_grid_clock();
void enable_runtime_timer();
void heartbeat();

#endif /* TIMERS_H_ */
