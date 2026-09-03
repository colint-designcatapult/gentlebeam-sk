#ifndef SENSUSSRC_ADC_H_
#define SENSUSSRC_ADC_H_

#include <stdbool.h>
#include "stm32f4xx_hal.h"

#define NUM_ADC_SAMPLES 32

typedef enum AdcState
{
    TEMP_MEASURE_DONE = 0,
    MEASURING_TEMP,
    PRESSURE_MEASURE_DONE,
    MEASURING_PRESSURE,
    QC_MEASURE_DONE,
    MEASURING_QC
} adc_state;

void init_adc(void);
void process_adc(void);
void adc_cb(void);
HAL_StatusTypeDef adc_suspend(void);
HAL_StatusTypeDef adc_resume(void);
bool adc_is_suspended(void);

#endif /* SENSUSSRC_ADC_H_ */
