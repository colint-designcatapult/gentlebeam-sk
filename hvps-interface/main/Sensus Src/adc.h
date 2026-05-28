
#ifndef ADC_H_
#define ADC_H_

enum
{
	INT_ADC_15V = 0,
	INT_ADC_VA,
	INT_ADC_FIL_A,
	INT_ADC_FIL_V,
	INT_ADC_GRID,
	INT_ADC_KV_SP,
	INT_ADC_GRID_SP,
	INT_ADC_5VREF,
	INT_ADC_5V,
	INT_ADC_PS15,
	INT_ADC_PS24,
	INT_ADC_MALSP,
	INT_ADC_FILSP,
	INT_ADC_TEMP,
	INT_ADC_VBAT,
	NUM_INT_ADC
};


void setup_adc();
void process_adc();


#endif /* ADC_H_ */
