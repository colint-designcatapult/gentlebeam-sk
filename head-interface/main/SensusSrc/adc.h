#ifndef SENSUSSRC_ADC_H_
#define SENSUSSRC_ADC_H_


#define NUM_ADC_SAMPLES 32

typedef enum AdcState
{
    TEMP_MEASURE_DONE = 0,
    MEASURING_TEMP,
    PRESSURE_MEASURE_DONE,
    MEASURING_PRESSURE
} adc_state;

void init_adc();
void process_adc();
void adc_cb();

#endif /* ADC_H_ */
