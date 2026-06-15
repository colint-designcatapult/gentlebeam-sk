#include "stm32f4xx_hal.h"
#include "timer.h"

//Function called every 1 ms to update timers and flags
void update_system_timers()
{
	read_mag_ms--;
	control_comm_ms--;
	update_flow = true;
#if !defined(CALIBRATION_MODE)
	update_led_ms--;
	button_process_ready = true;
	collim_ms--;
	qc_reset_count_ms++;
#endif
}
