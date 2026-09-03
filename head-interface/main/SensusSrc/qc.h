/*
 * qc.h
 *
 * Quality-control analog sampling and emission-scoped accumulation.
 */

#ifndef QC_H_
#define QC_H_

#include <stdbool.h>
#include <stdint.h>
#include "main.h"

/* One analog diode is connected to PC2 / ADC1_IN12 and occupies wire channel 0. */
#define QC_ANALOG_ADC_CHANNEL       ADC_CHANNEL_12
#define QC_CONNECTION_FLAG         0x8000u
#define QC_DMA_SAMPLES              32u
#define QC_DMA_HALF_SAMPLES         16u

typedef enum
{
    QC_MODE_IDLE = 0,
    QC_MODE_DMA_STARTING,
    QC_MODE_DMA_ACTIVE,
    QC_MODE_DMA_STOPPING,
    QC_MODE_COMPLETE,
    QC_MODE_ERROR
} QcMode;

typedef enum
{
    QC_DESIRED_STOPPED = 0,
    QC_DESIRED_ACCUMULATING = 1
} QcDesiredState;

typedef enum
{
    QC_ACQUISITION_STOPPED = 0,
    QC_ACQUISITION_ACTIVE,
    QC_ACQUISITION_COMPLETE,
    QC_ACQUISITION_ERROR
} QcAcquisitionState;

typedef struct
{
    uint32_t channel0_accumulation;
    uint32_t channel1_accumulation;
    uint32_t channel0_sample_count;
    uint32_t channel1_sample_count;
    QcAcquisitionState acquisition_state;
    uint16_t channel0_reading;
    uint16_t channel1_reading;
    bool channel0_connected;
    bool channel1_connected;
} QcSnapshot;

void init_qc(void);
void process_qc(void);
void qc_set_desired_state(QcDesiredState desired_state);
void qc_get_snapshot(QcSnapshot *snapshot);
void qc_update_idle_reading(uint16_t reading);
void qc_adc_half_complete_callback(ADC_HandleTypeDef *hadc);
bool qc_adc_complete_callback(ADC_HandleTypeDef *hadc);
bool qc_adc_error_callback(ADC_HandleTypeDef *hadc);


#endif /* QC_H_ */
