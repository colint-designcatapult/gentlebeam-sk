#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"

#include "adc.h"
#include "monitoring.h"
#include "timers.h"

uint16_t adc_int[NUM_INT_ADC];
uint32_t adc_int_sums[NUM_INT_ADC];
uint32_t adc_int_buf[NUM_INT_ADC][16];	//TBD TODO magic number

void setup_adc()
{
	HAL_StatusTypeDef status;

	status = HAL_ADCEx_Calibration_Start(&hadc1);

	if (status != HAL_OK)
	{
	    // Calibration failed
	}

	HAL_Delay(10);

	HAL_ADC_Start_DMA(&hadc1, (uint32_t*)adc_int, NUM_INT_ADC);
	int_adc_ms = 5;	//TBD TODO placeholder/magic number
}

//TBD TODO, implement circular buffer if desired but internal ADC signals aren't as critical

void process_adc()
{
	if(int_adc_ms <= 0)
	{
		//Report ADC values
		report_int_adc_vals(adc_int);

		//Restart ADC DMA
		int_adc_ms = 5;	//TBD TODO placeholder/magic number
	}
}
