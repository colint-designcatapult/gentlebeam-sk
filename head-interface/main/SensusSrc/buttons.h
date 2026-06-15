#ifndef SENSUSSRC_BUTTONS_H_
#define SENSUSSRC_BUTTONS_H_

#include "stm32f4xx_hal.h"

#define DEBOUNCE_MS	50

enum
{
	BTN_LASER = 0,
	BTN_LED,
	BTN_CAMERA,
	BTN_FUNC_1,
	BTN_FUNC_2,
	BTN_ZEROG,
	NUM_BUTTONS
};

typedef struct Debouncer
{
	int32_t debounce_ms_left;
	GPIO_PinState pin_state;
	GPIO_TypeDef* GPIOx;
	uint16_t GPIO_Pin;
} debouncer;

void init_buttons();
void process_buttons();


#endif /* SENSUSSRC_BUTTONS_H_ */
