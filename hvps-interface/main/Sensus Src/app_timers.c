#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"

#include "timers.h"

volatile int32_t int_adc_ms;
volatile int32_t comm_ms;
volatile int32_t kv_ramp_ms;
volatile int32_t fil_ramp_ms;
volatile int32_t lock_timer_ms;
volatile int32_t io_ms;
volatile int32_t grid_ms;

volatile uint32_t runtime_ms = 0;

static const uint32_t grid_clock_edges[2] =
{
	IO_GRID_CLK_Pin,
	(uint32_t)IO_GRID_CLK_Pin << 16U
};

//Timer that executes once per ms to decrement flags
void update_timers()
{
	int_adc_ms--;
	comm_ms--;
	kv_ramp_ms--;
	fil_ramp_ms--;
	lock_timer_ms--;
	io_ms--;
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
	HAL_GPIO_WritePin(IO_GRID_CLK_GPIO_Port, IO_GRID_CLK_Pin, GPIO_PIN_RESET);
	__HAL_TIM_SET_COUNTER(&htim7, 0U);
	__HAL_TIM_CLEAR_FLAG(&htim7, TIM_FLAG_UPDATE);

	if (hdma_tim7_up.State == HAL_DMA_STATE_BUSY)
	{
		(void)HAL_DMA_Abort(&hdma_tim7_up);
	}

	if (HAL_DMA_Start(&hdma_tim7_up,
			(uint32_t)&grid_clock_edges[0],
			(uint32_t)&IO_GRID_CLK_GPIO_Port->BSRR,
			2U) != HAL_OK)
	{
		return;
	}

	__HAL_TIM_ENABLE_DMA(&htim7, TIM_DMA_UPDATE);
	if (HAL_TIM_Base_Start(&htim7) != HAL_OK)
	{
		__HAL_TIM_DISABLE_DMA(&htim7, TIM_DMA_UPDATE);
		(void)HAL_DMA_Abort(&hdma_tim7_up);
	}
}

void disable_grid_clock()
{
	(void)HAL_TIM_Base_Stop(&htim7);
	__HAL_TIM_DISABLE_DMA(&htim7, TIM_DMA_UPDATE);
	if (hdma_tim7_up.State == HAL_DMA_STATE_BUSY)
	{
		(void)HAL_DMA_Abort(&hdma_tim7_up);
	}
	HAL_GPIO_WritePin(IO_GRID_CLK_GPIO_Port, IO_GRID_CLK_Pin, GPIO_PIN_RESET);
}

void enable_runtime_timer()
{
	HAL_TIM_Base_Start_IT(&htim6);
}
