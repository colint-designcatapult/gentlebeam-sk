#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"


#include "timers.h"

volatile int32_t ext_adc_ms;
volatile int32_t int_adc_ms;
volatile int32_t comm_ms;
volatile int32_t kv_ramp_ms;
volatile int32_t fil_ramp_ms;
volatile int32_t lock_timer_ms;
volatile int32_t io_ms;
volatile int32_t pid_ms;
volatile int32_t grid_ms;

volatile uint32_t runtime_ms = 0;

//Timer that executes once per ms to decrement flags
void update_timers()
{
	ext_adc_ms--;
	int_adc_ms--;
	comm_ms--;
	kv_ramp_ms--;
	fil_ramp_ms--;
	lock_timer_ms--;
	io_ms--;
	pid_ms--;
	grid_ms--;
}

// Toggles the IO_TEST_1 pin every 500 ms as a heartbeat indicator
void heartbeat()
{
	static uint32_t last_toggle_ms = 0;

    if ((runtime_ms - last_toggle_ms) >= 500)
    {
        last_toggle_ms = runtime_ms;
        HAL_GPIO_TogglePin(IO_TEST_1_GPIO_Port, IO_TEST_1_Pin);
    }

}

void enable_grid_clock()
{
	HAL_TIM_Base_Start_IT(&htim7);
}

void disable_grid_clock()
{
	HAL_TIM_Base_Stop_IT(&htim7);
}

void enable_runtime_timer()
{
	HAL_TIM_Base_Start_IT(&htim6);
}
