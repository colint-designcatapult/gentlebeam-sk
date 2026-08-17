#include "stm32f4xx_hal.h"
#include "main.h"

#include "buttons.h"
#include "leds.h"
#include "timer.h"
#include "sys_data.h"
#include "control_comm.h"
#include "led_ring.h"

volatile bool button_process_ready = false;

debouncer button_debounce[NUM_BUTTONS];

static void check_button(int idx);
static void execute_button_function(int idx, GPIO_PinState new_state);

int octant_test_idx = 0;

void init_buttons()
{
	//Initialize button debouncing struct values
	for(int i = 0; i < NUM_BUTTONS; i++)
	{
		button_debounce[i].pin_state = GPIO_PIN_SET;
		button_debounce[i].debounce_ms_left = -1;
	}

	//Set pins for debouncing
	button_debounce[BTN_LED].GPIOx = IO_PB_LED_GPIO_Port;
	button_debounce[BTN_LED].GPIO_Pin = IO_PB_LED_Pin;

	button_debounce[BTN_LASER].GPIOx = IO_PB_LASER_GPIO_Port;
	button_debounce[BTN_LASER].GPIO_Pin = IO_PB_LASER_Pin;

	button_debounce[BTN_CAMERA].GPIOx = IO_PB_IMG_GPIO_Port;
	button_debounce[BTN_CAMERA].GPIO_Pin = IO_PB_IMG_Pin;

	button_debounce[BTN_FUNC_1].GPIOx = IO_PB_F1_GPIO_Port;
	button_debounce[BTN_FUNC_1].GPIO_Pin = IO_PB_F1_Pin;

	button_debounce[BTN_FUNC_2].GPIOx = IO_PB_F2_GPIO_Port;
	button_debounce[BTN_FUNC_2].GPIO_Pin = IO_PB_F2_Pin;

	button_debounce[BTN_ZEROG].GPIOx = IO_PB_ZEROG_GPIO_Port;
	button_debounce[BTN_ZEROG].GPIO_Pin = IO_PB_ZEROG_Pin;
}

void process_buttons()
{
	//Wait for 1 ms boolean to toggle
	if(!button_process_ready)
	{
		return;
	}
	button_process_ready = false;

	for(int i = 0; i < NUM_BUTTONS; i++)
	{
		check_button(i);
	}
}

static void check_button(int idx)
{
	GPIO_PinState pin_status = HAL_GPIO_ReadPin(button_debounce[idx].GPIOx, button_debounce[idx].GPIO_Pin);

	//If button is currently debouncing, simply continue waiting
	if(button_debounce[idx].debounce_ms_left > 0)
	{
		button_debounce[idx].debounce_ms_left--;
	}
	//If debounce has finished, check to see if state toggle is complete
	else if(button_debounce[idx].debounce_ms_left == 0)
	{
		button_debounce[idx].debounce_ms_left = -1;
		if(pin_status != button_debounce[idx].pin_state)
		{
			button_debounce[idx].pin_state = pin_status;
#if !defined(CALIBRATION_MODE)
			report_button_toggle(idx, pin_status);
#endif
			execute_button_function(idx, pin_status);
		}
	}
	//Otherwise if we're not doing anything just check for transitions
	else if(pin_status != button_debounce[idx].pin_state)
	{
		button_debounce[idx].debounce_ms_left = DEBOUNCE_MS;
	}
}

static led_ring_mode_t	s_current_mode = LED_RING_MODE_OFF;

//If specific functionality is needed on button press/release, execute here
static void execute_button_function(int idx, GPIO_PinState new_state)
{
	switch(idx)
	{
		case BTN_LASER:
			if(new_state == GPIO_PIN_RESET)
			{

			}
			break;
		case BTN_LED:
			if(new_state == GPIO_PIN_RESET)
			{
				s_current_mode = (s_current_mode + 1) % LED_RING_MODE_COUNT;
				led_ring_set_mode(s_current_mode);
			}
			break;
		case BTN_FUNC_1:

			break;
		case BTN_FUNC_2:
#ifdef OCTANT_TEST
			if(new_state == GPIO_PIN_RESET)
			{
				octant_test_idx++;
				HAL_GPIO_TogglePin(IO_LED_AMBER_GPIO_Port, IO_LED_AMBER_Pin);\
				if(octant_test_idx >= 16 || octant_test_idx < 0)
				{
					octant_test_idx = 0;
				}
				set_new_led_sequence(octant_test_idx);
			}
#endif
			break;
		case BTN_ZEROG:
			if(new_state == GPIO_PIN_RESET)
			{
				HAL_GPIO_WritePin(RELAY_CTRL_GPIO_Port, RELAY_CTRL_Pin, GPIO_PIN_SET);
			}
			else
			{
				HAL_GPIO_WritePin(RELAY_CTRL_GPIO_Port, RELAY_CTRL_Pin, GPIO_PIN_RESET);
			}
			break;
		default:
			break;
	}
}
