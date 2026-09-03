#include "stm32f4xx_hal.h"
#include "main.h"

#include "adc.h"
#include "sys_data.h"
#if !defined(CALIBRATION_MODE)
#include "qc.h"
#endif

uint32_t pressure_raw[NUM_ADC_SAMPLES];
uint32_t temperature_raw[NUM_ADC_SAMPLES];
uint8_t pressure_idx = 0;
uint8_t temperature_idx = 0;

volatile adc_state adc_status = MEASURING_TEMP;
volatile uint32_t adc_val = 0;

static ADC_ChannelConfTypeDef sConfig = {0};
static volatile bool adc_suspended = true;

static void report_temperature(void);
static void report_pressure(void);
static HAL_StatusTypeDef start_monitoring_conversion(uint32_t channel, adc_state state);

void init_adc(void)
{
    (void)adc_resume();
}

HAL_StatusTypeDef adc_suspend(void)
{
    adc_suspended = true;
    return HAL_ADC_Stop_IT(&hadc1);
}

HAL_StatusTypeDef adc_resume(void)
{
    hadc1.Init.ContinuousConvMode = DISABLE;
    hadc1.Init.ExternalTrigConvEdge = ADC_EXTERNALTRIGCONVEDGE_NONE;
    hadc1.Init.ExternalTrigConv = ADC_SOFTWARE_START;
    hadc1.Init.DMAContinuousRequests = DISABLE;
    hadc1.Init.EOCSelection = ADC_EOC_SINGLE_CONV;
    if (HAL_ADC_Init(&hadc1) != HAL_OK)
    {
        return HAL_ERROR;
    }

    adc_suspended = false;
    if (start_monitoring_conversion(ADC_CHANNEL_10, MEASURING_TEMP) != HAL_OK)
    {
        adc_suspended = true;
        return HAL_ERROR;
    }
    return HAL_OK;
}

bool adc_is_suspended(void)
{
    return adc_suspended;
}

void process_adc(void)
{
    if (adc_suspended)
    {
        return;
    }

    if (adc_status == TEMP_MEASURE_DONE)
    {
        temperature_raw[temperature_idx] = adc_val;
        temperature_idx++;
        if (temperature_idx >= NUM_ADC_SAMPLES)
        {
            temperature_idx = 0;
            report_temperature();
        }

        (void)start_monitoring_conversion(ADC_CHANNEL_0, MEASURING_PRESSURE);
    }
    else if (adc_status == PRESSURE_MEASURE_DONE)
    {
        pressure_raw[pressure_idx] = adc_val;
        pressure_idx++;
        if (pressure_idx >= NUM_ADC_SAMPLES)
        {
            pressure_idx = 0;
            report_pressure();
        }

#if !defined(CALIBRATION_MODE)
        (void)start_monitoring_conversion(QC_ANALOG_ADC_CHANNEL, MEASURING_QC);
#else
        (void)start_monitoring_conversion(ADC_CHANNEL_10, MEASURING_TEMP);
#endif
    }
#if !defined(CALIBRATION_MODE)
    else if (adc_status == QC_MEASURE_DONE)
    {
        qc_update_idle_reading((uint16_t)adc_val);
        (void)start_monitoring_conversion(ADC_CHANNEL_10, MEASURING_TEMP);
    }
#endif
}

static HAL_StatusTypeDef start_monitoring_conversion(uint32_t channel, adc_state state)
{
    sConfig.Channel = channel;
    sConfig.Rank = 1;
    sConfig.SamplingTime = ADC_SAMPLETIME_15CYCLES;
    if (HAL_ADC_ConfigChannel(&hadc1, &sConfig) != HAL_OK)
    {
        return HAL_ERROR;
    }

    adc_status = state;
    return HAL_ADC_Start_IT(&hadc1);
}

static void report_temperature(void)
{
    uint32_t temperature_sum = 0;
    for (int i = 0; i < NUM_ADC_SAMPLES; i++)
    {
        temperature_sum += temperature_raw[i];
    }
    temperature_sum /= NUM_ADC_SAMPLES;
    report_temperature_data(temperature_sum);
}

static void report_pressure(void)
{
    uint32_t pressure_sum = 0;
    for (int i = 0; i < NUM_ADC_SAMPLES; i++)
    {
        pressure_sum += pressure_raw[i];
    }
    pressure_sum /= NUM_ADC_SAMPLES;
    report_pressure_data(pressure_sum);
}

void adc_cb(void)
{
    if (adc_suspended)
    {
        return;
    }

    adc_val = HAL_ADC_GetValue(&hadc1);
    if (adc_status == MEASURING_PRESSURE)
    {
        adc_status = PRESSURE_MEASURE_DONE;
    }
#if !defined(CALIBRATION_MODE)
    else if (adc_status == MEASURING_QC)
    {
        adc_status = QC_MEASURE_DONE;
    }
#endif
    else
    {
        adc_status = TEMP_MEASURE_DONE;
    }
}
