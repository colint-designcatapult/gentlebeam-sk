#include "stm32f4xx_hal.h"
#include "main.h"

#include "adc.h"
#include "sys_data.h"

uint32_t pressure_raw[NUM_ADC_SAMPLES];
uint32_t temperature_raw[NUM_ADC_SAMPLES];
uint8_t pressure_idx = 0;
uint8_t temperature_idx = 0;

volatile adc_state adc_status = MEASURING_TEMP;
volatile uint32_t adc_val = 0;

ADC_ChannelConfTypeDef sConfig = {0};

static void report_temperature();
static void report_pressure();


void init_adc()
{
	//Set up adc for temperature measurement first
	sConfig.Channel = ADC_CHANNEL_10;
	sConfig.Rank = 1;
	sConfig.SamplingTime = ADC_SAMPLETIME_15CYCLES;
	HAL_ADC_ConfigChannel(&hadc1, &sConfig);

	//Initialize adc status value
	adc_status = MEASURING_TEMP;

	//Begin ADC measurments
	HAL_ADC_Start_IT(&hadc1);
}

void process_adc()
{
	//Cycle between reading temperature and pressure
	if(adc_status == TEMP_MEASURE_DONE)
	{
		//Update temperature buffer
		temperature_raw[temperature_idx] = adc_val;
		temperature_idx++;
		if(temperature_idx >= NUM_ADC_SAMPLES)
		{
			temperature_idx = 0;
			report_temperature();
		}

		//Set up adc for pressure measurement
		sConfig.Channel = ADC_CHANNEL_0;
		HAL_ADC_ConfigChannel(&hadc1, &sConfig);

		adc_status = MEASURING_PRESSURE;

		//Begin ADC measurments
		HAL_ADC_Start_IT(&hadc1);
	}
	else if (adc_status == PRESSURE_MEASURE_DONE)
	{
		//Update pressure buffer
		pressure_raw[pressure_idx] = adc_val;
		pressure_idx++;
		if(pressure_idx >= NUM_ADC_SAMPLES)
		{
			pressure_idx = 0;
			report_pressure();
		}

		//Set up adc for temperature measurement first
		sConfig.Channel = ADC_CHANNEL_10;
		HAL_ADC_ConfigChannel(&hadc1, &sConfig);

		adc_status = MEASURING_TEMP;

		//Begin ADC measurments
		HAL_ADC_Start_IT(&hadc1);
	}
}

static void report_temperature()
{
	uint32_t temperature_sum = 0;
	//Get average of buffered temperature readings to report
	for(int i = 0; i < NUM_ADC_SAMPLES; i++)
	{
		temperature_sum += temperature_raw[i];
	}
	temperature_sum /= NUM_ADC_SAMPLES;
	//Send temperature value to system data
	report_temperature_data(temperature_sum);
}

static void report_pressure()
{
	uint32_t pressure_sum = 0;
	//Get average of buffered pressure readings to report
	for(int i = 0; i < NUM_ADC_SAMPLES; i++)
	{
		pressure_sum += pressure_raw[i];
	}
	pressure_sum /= NUM_ADC_SAMPLES;
	//Send pressure value to system data
	report_pressure_data(pressure_sum);
}

void adc_cb()
{
	adc_val = HAL_ADC_GetValue(&hadc1);
	if(adc_status == MEASURING_PRESSURE)
	{
		adc_status = PRESSURE_MEASURE_DONE;
	}
	else
	{
		adc_status = TEMP_MEASURE_DONE;
	}
}

