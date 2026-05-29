#include "stm32f4xx_hal.h"
#include "main.h"

#include "flow.h"
#include "timer.h"
#include "sys_data.h"

volatile bool update_flow = false;
volatile int32_t flow_update_ms = 20;
uint32_t last_flow_count = 0;
volatile uint32_t flow_buf_idx = 0;
uint32_t flow_count_buf[NUM_FLOW_SAMPLES];
uint32_t flow_sum = 0;

GPIO_PinState last_flow_state = GPIO_PIN_RESET;


void init_flow()
{
	//HAL_TIM_Base_Start(&htim4); not used
}

void process_flow()
{
	//Boolean toggle every ms
	if(update_flow)
	{
		flow_update_ms--;
		update_flow = false;
		//Check pin to see if toggle, if so, increment counter
		//Flow sensor toggles faster (more counts) with faster flow
		GPIO_PinState current_state = HAL_GPIO_ReadPin(IO_FLOW_GPIO_Port, IO_FLOW_Pin);
		if(current_state != last_flow_state)
		{
			last_flow_state = current_state;
			last_flow_count++;
		}
	}

	//Wait for timer expiration
	if(flow_update_ms > 0)
	{
		return;
	}

	flow_update_ms += 100;

	//Update the flow toggle counts
	flow_count_buf[flow_buf_idx] = last_flow_count;
	flow_sum += last_flow_count;
	flow_buf_idx++;
	flow_buf_idx %= NUM_FLOW_SAMPLES;
	flow_sum -= flow_count_buf[flow_buf_idx];
	last_flow_count = 0;

	//Report flow counts
	report_flow_data(flow_sum);
}
