#include "stm32f4xx_hal.h"
#include "main.h"

#include "adc.h"
#include "qc.h"


static uint16_t channel0_reading;
static uint16_t channel1_reading;
static volatile bool channel0_connected;
static volatile bool channel1_connected;

static uint16_t qc_dma_buffer[QC_DMA_SAMPLES];
static volatile QcDesiredState desired_state = QC_DESIRED_STOPPED;
static volatile QcDesiredState applied_desired_state = QC_DESIRED_STOPPED;
static volatile QcMode qc_mode = QC_MODE_IDLE;
static volatile QcAcquisitionState acquisition_state = QC_ACQUISITION_STOPPED;
static volatile uint32_t accumulations[2];
static volatile uint32_t sample_counts[2];
static volatile bool dma_transfer_valid;
static volatile bool dma_half_processed;
static volatile bool stop_requested;
static volatile bool stop_before_half;
static volatile bool adc_owned_by_qc;
static volatile bool adc_stop_pending;

static void apply_desired_state(void);
static void begin_dma_session(void);
static HAL_StatusTypeDef configure_qc_adc(void);
static HAL_StatusTypeDef start_dma_burst(void);
static void process_dma_samples(uint32_t offset, uint32_t count);
static void stop_adc_dma(void);
static void finish_session(void);
static void enter_acquisition_error(void);
static void invalidate_dma_transfer(void);
static void resume_monitoring_adc(void);

void init_qc(void)
{
    uint32_t primask = __get_PRIMASK();
    __disable_irq();

    channel1_reading = 0;
    channel0_reading = 0;
    channel1_connected = false;
    channel0_connected = false;
    accumulations[0] = 0;
    accumulations[1] = 0;
    sample_counts[0] = 0;
    sample_counts[1] = 0;
    desired_state = QC_DESIRED_STOPPED;
    applied_desired_state = QC_DESIRED_STOPPED;
    qc_mode = QC_MODE_IDLE;
    acquisition_state = QC_ACQUISITION_STOPPED;
    dma_transfer_valid = false;
    dma_half_processed = false;
    stop_requested = false;
    stop_before_half = false;
    adc_owned_by_qc = false;
    adc_stop_pending = false;

    if (primask == 0u)
    {
        __enable_irq();
    }
}

void qc_set_desired_state(QcDesiredState state)
{
    if (state != QC_DESIRED_STOPPED && state != QC_DESIRED_ACCUMULATING)
    {
        return;
    }

    uint32_t primask = __get_PRIMASK();
    __disable_irq();
    desired_state = state;
    if (primask == 0u)
    {
        __enable_irq();
    }
}

void qc_get_snapshot(QcSnapshot *snapshot)
{
    if (snapshot == NULL)
    {
        return;
    }

    uint32_t primask = __get_PRIMASK();
    __disable_irq();
    snapshot->channel0_accumulation = accumulations[0];
    snapshot->channel1_accumulation = accumulations[1];
    snapshot->channel0_sample_count = sample_counts[0];
    snapshot->channel1_sample_count = sample_counts[1];
    snapshot->acquisition_state = acquisition_state;
    snapshot->channel0_reading = channel0_reading;
    snapshot->channel1_reading = channel1_reading;
    snapshot->channel0_connected = channel0_connected;
    snapshot->channel1_connected = channel1_connected;
    if (primask == 0u)
    {
        __enable_irq();
    }
}

void qc_update_idle_reading(uint16_t reading)
{
    uint32_t primask = __get_PRIMASK();
    __disable_irq();
    if (qc_mode == QC_MODE_IDLE ||
        qc_mode == QC_MODE_COMPLETE ||
        qc_mode == QC_MODE_ERROR)
    {
        channel0_reading = reading & 0x0FFFu;
        channel0_connected = true;
        channel1_reading = 0;
        channel1_connected = false;
    }
    if (primask == 0u)
    {
        __enable_irq();
    }
}

void process_qc(void)
{
    apply_desired_state();

    if (adc_stop_pending)
    {
        adc_stop_pending = false;
        stop_adc_dma();
    }

    if (qc_mode == QC_MODE_DMA_STARTING)
    {
        if (stop_requested)
        {
            finish_session();
        }
        else if (start_dma_burst() != HAL_OK)
        {
            enter_acquisition_error();
        }
    }
    else if (qc_mode == QC_MODE_DMA_ACTIVE)
    {
        if (stop_requested && stop_before_half && dma_transfer_valid)
        {
            qc_mode = QC_MODE_DMA_STOPPING;
            stop_adc_dma();
            invalidate_dma_transfer();
            finish_session();
        }
        else if (!dma_transfer_valid && !stop_requested)
        {
            if (start_dma_burst() != HAL_OK)
            {
                enter_acquisition_error();
            }
        }
    }

    if ((qc_mode == QC_MODE_COMPLETE || qc_mode == QC_MODE_ERROR) && adc_owned_by_qc)
    {
        resume_monitoring_adc();
    }
}

static void apply_desired_state(void)
{
    QcDesiredState requested = desired_state;
    if (requested == applied_desired_state)
    {
        return;
    }
    applied_desired_state = requested;

    if (requested == QC_DESIRED_ACCUMULATING)
    {
        if (qc_mode == QC_MODE_IDLE ||
            qc_mode == QC_MODE_COMPLETE ||
            qc_mode == QC_MODE_ERROR)
        {
            begin_dma_session();
        }
        return;
    }

    if (qc_mode == QC_MODE_DMA_STARTING)
    {
        stop_requested = true;
    }
    else if (qc_mode == QC_MODE_DMA_ACTIVE)
    {
        stop_requested = true;
        stop_before_half = !dma_half_processed;
        if (!dma_transfer_valid)
        {
            finish_session();
        }
    }
    else if (qc_mode == QC_MODE_DMA_STOPPING)
    {
        stop_requested = true;
    }
    else if (qc_mode == QC_MODE_IDLE &&
             acquisition_state != QC_ACQUISITION_COMPLETE &&
             acquisition_state != QC_ACQUISITION_ERROR)
    {
        acquisition_state = QC_ACQUISITION_STOPPED;
    }
}

static void begin_dma_session(void)
{
    uint32_t primask = __get_PRIMASK();
    __disable_irq();
    accumulations[0] = 0;
    accumulations[1] = 0;
    sample_counts[0] = 0;
    sample_counts[1] = 0;
    channel1_reading = 0;
    channel1_connected = false;
    stop_requested = false;
    stop_before_half = false;
    dma_transfer_valid = false;
    dma_half_processed = false;
    acquisition_state = QC_ACQUISITION_STOPPED;
    qc_mode = QC_MODE_DMA_STARTING;
    if (primask == 0u)
    {
        __enable_irq();
    }
}

static HAL_StatusTypeDef configure_qc_adc(void)
{
    if (adc_suspend() != HAL_OK)
    {
        return HAL_ERROR;
    }
    adc_owned_by_qc = true;

    hadc1.Init.ContinuousConvMode = DISABLE;
    hadc1.Init.ExternalTrigConvEdge = ADC_EXTERNALTRIGCONVEDGE_RISING;
    hadc1.Init.ExternalTrigConv = ADC_EXTERNALTRIGCONV_T2_TRGO;
    hadc1.Init.DMAContinuousRequests = ENABLE;
    hadc1.Init.EOCSelection = ADC_EOC_SINGLE_CONV;
    if (HAL_ADC_Init(&hadc1) != HAL_OK)
    {
        return HAL_ERROR;
    }

    ADC_ChannelConfTypeDef config = {0};
    config.Channel = QC_ANALOG_ADC_CHANNEL;
    config.Rank = 1;
    config.SamplingTime = ADC_SAMPLETIME_15CYCLES;
    return HAL_ADC_ConfigChannel(&hadc1, &config);
}

static HAL_StatusTypeDef start_dma_burst(void)
{
    if (!adc_owned_by_qc && configure_qc_adc() != HAL_OK)
    {
        return HAL_ERROR;
    }

    dma_transfer_valid = true;
    dma_half_processed = false;
    HAL_StatusTypeDef status = HAL_ADC_Start_DMA(
        &hadc1,
        (uint32_t *)qc_dma_buffer,
        QC_DMA_SAMPLES);
    if (status != HAL_OK)
    {
        invalidate_dma_transfer();
        return status;
    }

    acquisition_state = QC_ACQUISITION_ACTIVE;
    qc_mode = QC_MODE_DMA_ACTIVE;
    return HAL_OK;
}

void qc_adc_half_complete_callback(ADC_HandleTypeDef *hadc)
{
    if (hadc != &hadc1 || !dma_transfer_valid || dma_half_processed)
    {
        return;
    }
    if (stop_requested && stop_before_half)
    {
        return;
    }

    process_dma_samples(0u, QC_DMA_HALF_SAMPLES);
    dma_half_processed = true;
}

bool qc_adc_complete_callback(ADC_HandleTypeDef *hadc)
{
    if (hadc != &hadc1 || !dma_transfer_valid)
    {
        return false;
    }

    if (stop_requested && stop_before_half)
    {
        stop_adc_dma();
        invalidate_dma_transfer();
        finish_session();
        return true;
    }

    if (!dma_half_processed)
    {
        process_dma_samples(0u, QC_DMA_HALF_SAMPLES);
        dma_half_processed = true;
    }
    process_dma_samples(QC_DMA_HALF_SAMPLES, QC_DMA_HALF_SAMPLES);

    stop_adc_dma();
    invalidate_dma_transfer();
    if (stop_requested)
    {
        finish_session();
    }
    return true;
}

bool qc_adc_error_callback(ADC_HandleTypeDef *hadc)
{
    if (hadc != &hadc1 || !adc_owned_by_qc)
    {
        return false;
    }

    channel0_connected = false;
    invalidate_dma_transfer();
    adc_stop_pending = true;
    enter_acquisition_error();
    return true;
}

static void process_dma_samples(uint32_t offset, uint32_t count)
{
    uint32_t sum = 0;
    uint16_t last = 0;
    for (uint32_t sample = 0; sample < count; sample++)
    {
        last = qc_dma_buffer[offset + sample] & 0x0FFFu;
        sum += last;
    }

    accumulations[0] += sum;
    sample_counts[0] += count;
    channel0_reading = last;
    channel0_connected = true;
    accumulations[1] = 0;
    sample_counts[1] = 0;
    channel1_reading = 0;
    channel1_connected = false;
}

static void stop_adc_dma(void)
{
    if (adc_owned_by_qc)
    {
        (void)HAL_ADC_Stop_DMA(&hadc1);
    }
}

static void finish_session(void)
{
    invalidate_dma_transfer();
    stop_requested = false;
    stop_before_half = false;
    acquisition_state = QC_ACQUISITION_COMPLETE;
    qc_mode = QC_MODE_COMPLETE;
}

static void enter_acquisition_error(void)
{
    invalidate_dma_transfer();
    stop_requested = false;
    stop_before_half = false;
    acquisition_state = QC_ACQUISITION_ERROR;
    qc_mode = QC_MODE_ERROR;
}

static void invalidate_dma_transfer(void)
{
    dma_transfer_valid = false;
    dma_half_processed = false;
}

static void resume_monitoring_adc(void)
{
    stop_adc_dma();
    if (adc_resume() == HAL_OK)
    {
        adc_owned_by_qc = false;
    }
}

