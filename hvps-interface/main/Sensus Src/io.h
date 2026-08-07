
#ifndef IO_H_
#define IO_H_

#include "stdbool.h"
#include "main.h"

#define LOCK_TIMER_PERIOD	1500
#define LOCK_TIMER_MIN_MS	10
#define LOCK_TIMER_MAX_MS	(LOCK_TIMER_PERIOD-50)

#define DEBOUNCE_MS		30


enum
{
	//Port B pins
	IN_GRID_CLK_STAT = 0,
	IN_FIL_CLK_FAULT,
	IN_GRID_INT,
	IN_BEAM_CTRL,

	//Port C pins
	IN_GRID_STAT,
	IN_CAT_ARC,

	//Port E pins
	IN_FAN_FAULT,
	IN_PFC_OK,
	IN_HV_INT,
	IN_HV_STAT,
	IN_OC_24_FAULT,
	IN_MASTER_FAULT,
	IN_OC_HV_FAULT,
	IN_TEMP_1_FAULT,
	IN_OC_CAT_FAULT,
	IN_TEMP_3_FAULT,
	IN_TEMP_2_FAULT,

	NUM_IO_INPUTS
};

typedef struct input_debouncer
{
	int32_t ms_left;
	uint32_t level;
	uint32_t idr_bit;
	uint32_t status_bit;
	GPIO_TypeDef* GPIOx;
} input_debouncer_t;

void setup_io();
void process_io();

void lock_hv();
void lock_grid();

void interlock_test(uint32_t param);


#endif /* IO_H_ */
