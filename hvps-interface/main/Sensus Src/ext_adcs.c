#include "main.h"
#include "ext_adcs.h"

#define ADS8325_FRAME_BYTES          3U
#define EXT_ADC_BURST_SAMPLES        16U
#define EXT_ADC_KV_DONE              (1U << 0)
#define EXT_ADC_MA_DONE              (1U << 1)
#define EXT_ADC_PAIR_DONE            (EXT_ADC_KV_DONE | EXT_ADC_MA_DONE)

typedef enum
{
	EXT_ADC_IDLE = 0,
	EXT_ADC_RUNNING,
	EXT_ADC_STOPPING
} ext_adc_state_t;

static volatile uint8_t kv_frame[ADS8325_FRAME_BYTES];
static volatile uint8_t ma_frame[ADS8325_FRAME_BYTES];
static volatile ext_adc_state_t adc_state;
static volatile uint32_t dma_done_mask;
static volatile uint32_t sample_index;
static volatile uint32_t kv_sum;
static volatile uint32_t ma_sum;
static volatile bool sample_active;
static TaskHandle_t volatile result_task;
static volatile ext_adc_result_t latest_result;
static volatile bool latest_result_available;

static bool ads8325_decode(const volatile uint8_t *data, uint16_t *sample);
static void prepare_ads8325_spi(SPI_TypeDef *spi);
static void wait_for_ads8325_cs_setup(void);
static bool start_ads8325_pair_from_isr(void);
static void stop_ads8325_hardware(void);
static void finish_ads8325_burst_from_isr(ext_adc_result_status_t status,
		uint16_t kv_average, uint16_t ma_average);
static void publish_ads8325_result_from_isr(ext_adc_result_status_t status,
		uint16_t kv_average, uint16_t ma_average, uint32_t completed_samples,
		uint32_t completed_dma_mask);
static void handle_ads8325_dma_complete(SPI_TypeDef *spi, uint32_t cs_pin,
		uint32_t done_bit, bool transfer_error);
static void kv_dma_complete(DMA_HandleTypeDef *hdma);
static void ma_dma_complete(DMA_HandleTypeDef *hdma);
static void kv_dma_error(DMA_HandleTypeDef *hdma);
static void ma_dma_error(DMA_HandleTypeDef *hdma);

void setup_ext_adcs(void)
{
	/* CS is active-low and must idle high while the ADS8325 is powered down. */
	IO_KV_CS_GPIO_Port->BSRR = IO_KV_CS_Pin;
	IO_MA_CS_GPIO_Port->BSRR = IO_MA_CS_Pin;

	prepare_ads8325_spi(SPI1);
	prepare_ads8325_spi(SPI3);

	/* DMA Callbacks */
	hdma_spi1_rx.XferCpltCallback = kv_dma_complete;
	hdma_spi1_rx.XferHalfCpltCallback = NULL;
	hdma_spi1_rx.XferErrorCallback = kv_dma_error;
	hdma_spi3_rx.XferCpltCallback = ma_dma_complete;
	hdma_spi3_rx.XferHalfCpltCallback = NULL;
	hdma_spi3_rx.XferErrorCallback = ma_dma_error;

	adc_state = EXT_ADC_IDLE;
	sample_active = false;
	result_task = NULL;

	sample_index = 0U;
	dma_done_mask = 0U;
	kv_sum = 0U;
	ma_sum = 0U;

	latest_result = (ext_adc_result_t){
		.sequence = 0U,
		.kv_average = 0U,
		.ma_average = 0U,
		.status = EXT_ADC_RESULT_DMA_ERROR,
		.sample_count = 0U,
		.dma_done_mask = 0U,
	};

	latest_result_available = false;
}

void ext_adcs_set_result_task(TaskHandle_t task)
{
	taskENTER_CRITICAL();
	result_task = task;
	taskEXIT_CRITICAL();
}

void ext_adcs_start_burst_from_isr(void)
{
	if (adc_state != EXT_ADC_IDLE)
	{
		finish_ads8325_burst_from_isr(EXT_ADC_RESULT_TIMING_ERROR, 0U, 0U);
		return;
	}

	latest_result_available = false;
	sample_index = 0U;
	dma_done_mask = 0U;
	kv_sum = 0U;
	ma_sum = 0U;
	sample_active = false;
	adc_state = EXT_ADC_RUNNING;

	if (!start_ads8325_pair_from_isr())
	{
		finish_ads8325_burst_from_isr(EXT_ADC_RESULT_DMA_ERROR, 0U, 0U);
	}
}

bool ext_adcs_get_latest_result(ext_adc_result_t *result)
{
	if (result == NULL)
	{
		return false;
	}

	taskENTER_CRITICAL();
	bool result_available = latest_result_available;
	if (result_available)
	{
		result->sequence = latest_result.sequence;
		result->kv_average = latest_result.kv_average;
		result->ma_average = latest_result.ma_average;
		result->status = latest_result.status;
		result->sample_count = latest_result.sample_count;
		result->dma_done_mask = latest_result.dma_done_mask;
	}
	taskEXIT_CRITICAL();

	return result_available;
}

void ext_adcs_check_completion_from_isr(void)
{
	bool timed_out = false;
	bool result_missing = false;
	uint32_t primask = __get_PRIMASK();

	__disable_irq();
	if (adc_state == EXT_ADC_RUNNING)
	{
		adc_state = EXT_ADC_STOPPING;
		timed_out = true;
	}
	else if ((adc_state == EXT_ADC_IDLE) && !latest_result_available)
	{
		result_missing = true;
	}
	__set_PRIMASK(primask);

	if (timed_out || result_missing)
	{
		finish_ads8325_burst_from_isr(EXT_ADC_RESULT_TIMING_ERROR, 0U, 0U);
	}

	TaskHandle_t task = result_task;
	if (task != NULL)
	{
		BaseType_t higher_priority_task_woken = pdFALSE;
		vTaskNotifyGiveFromISR(task, &higher_priority_task_woken);
		portYIELD_FROM_ISR(higher_priority_task_woken);
	}
}


static bool ads8325_decode(const volatile uint8_t *data, uint16_t *sample)
{

    // Read volatile data once to prevent torn reads and improve efficiency
    const uint8_t d0 = data[0];
    const uint8_t d1 = data[1];
    const uint8_t d2 = data[2];

    /*
     * A 24-clock frame contains five acquisition clocks, one LOW framing
     * bit, B15..B0, then repeated B1 and B2 bits. The result occupies bits
     * 17..2 of the received frame.
     */
    if ((d0 & 0x04U) != 0U)
    {
        return false;
    }

    uint16_t value = (uint16_t)(((uint16_t)(d0 & 0x03U) << 14U)
            | ((uint16_t)d1 << 6U)
            | ((uint16_t)d2 >> 2U));

    uint8_t repeated_bits = (uint8_t)(d2 & 0x03U);
    uint8_t expected_bits = (uint8_t)((value & 0x02U)
            | ((value >> 2U) & 0x01U));

    if (repeated_bits != expected_bits)
    {
        return false;
    }

    /* Return the complete ADS8325 straight-binary conversion result. */
    *sample = value;
    return true;
}

static bool start_ads8325_pair_from_isr(void)
{
	prepare_ads8325_spi(SPI1);
	prepare_ads8325_spi(SPI3);

    LL_DMA_DisableChannel(DMA1, LL_DMA_CHANNEL_2);
    LL_DMA_DisableChannel(DMA2, LL_DMA_CHANNEL_1);

    LL_DMA_SetPeriphAddress(
            DMA1,
            LL_DMA_CHANNEL_2,
            LL_SPI_DMA_GetRegAddr(SPI1));

    LL_DMA_SetMemoryAddress(
            DMA1,
            LL_DMA_CHANNEL_2,
            (uint32_t)kv_frame);

    LL_DMA_SetDataLength(
            DMA1,
            LL_DMA_CHANNEL_2,
            ADS8325_FRAME_BYTES);

    LL_DMA_SetPeriphAddress(
            DMA2,
            LL_DMA_CHANNEL_1,
            LL_SPI_DMA_GetRegAddr(SPI3));

    LL_DMA_SetMemoryAddress(
            DMA2,
            LL_DMA_CHANNEL_1,
            (uint32_t)ma_frame);

    LL_DMA_SetDataLength(
            DMA2,
            LL_DMA_CHANNEL_1,
            ADS8325_FRAME_BYTES);

    LL_DMA_ClearFlag_GI2(DMA1);
    LL_DMA_ClearFlag_GI1(DMA2);

    LL_DMA_EnableIT_TC(DMA1, LL_DMA_CHANNEL_2);
    LL_DMA_EnableIT_TE(DMA1, LL_DMA_CHANNEL_2);

    LL_DMA_EnableIT_TC(DMA2, LL_DMA_CHANNEL_1);
    LL_DMA_EnableIT_TE(DMA2, LL_DMA_CHANNEL_1);

    LL_DMA_EnableChannel(DMA1, LL_DMA_CHANNEL_2);
    LL_DMA_EnableChannel(DMA2, LL_DMA_CHANNEL_1);

    dma_done_mask = 0U;
    sample_active = true;

    LL_SPI_EnableDMAReq_RX(SPI1);
    LL_SPI_EnableDMAReq_RX(SPI3);

    GPIOF->BRR = IO_KV_CS_Pin | IO_MA_CS_Pin;

    wait_for_ads8325_cs_setup();

	LL_SPI_Enable(SPI1);
	LL_SPI_Enable(SPI3);

    return true;
}

static void handle_ads8325_dma_complete(SPI_TypeDef *spi, uint32_t cs_pin,
		uint32_t done_bit, bool transfer_error)
{
	LL_SPI_DisableDMAReq_RX(spi);
	LL_SPI_Disable(spi);
	GPIOF->BSRR = cs_pin;
	// prepare_ads8325_spi(spi);

	if ((adc_state != EXT_ADC_RUNNING) || !sample_active)
	{
		return;
	}

	if (transfer_error)
	{
		finish_ads8325_burst_from_isr(EXT_ADC_RESULT_DMA_ERROR, 0U, 0U);
		return;
	}

	dma_done_mask |= done_bit;
	if (dma_done_mask != EXT_ADC_PAIR_DONE)
	{
		return;
	}

	sample_active = false;

	static uint16_t kv_sample;
	static uint16_t ma_sample;
	if (!ads8325_decode(kv_frame, &kv_sample)
			|| !ads8325_decode(ma_frame, &ma_sample))
	{
		finish_ads8325_burst_from_isr(EXT_ADC_RESULT_FRAMING_ERROR, 0U, 0U);
		return;
	}

	kv_sum += kv_sample;
	ma_sum += ma_sample;
	sample_index++;

	if (sample_index < EXT_ADC_BURST_SAMPLES)
	{
		if (!start_ads8325_pair_from_isr())
		{
			finish_ads8325_burst_from_isr(EXT_ADC_RESULT_DMA_ERROR, 0U, 0U);
		}
	}
	else
	{
		uint16_t kv_average = (uint16_t)(kv_sum / EXT_ADC_BURST_SAMPLES);
		uint16_t ma_average = (uint16_t)(ma_sum / EXT_ADC_BURST_SAMPLES);
		finish_ads8325_burst_from_isr(EXT_ADC_RESULT_VALID,
				kv_average, ma_average);
	}
}

static void stop_ads8325_hardware(void)
{
	if (LL_DMA_IsEnabledChannel(DMA1, LL_DMA_CHANNEL_2))
	{
		LL_DMA_DisableChannel(DMA1, LL_DMA_CHANNEL_2);
	}

	if (LL_DMA_IsEnabledChannel(DMA2, LL_DMA_CHANNEL_1))
	{
		LL_DMA_DisableChannel(DMA2, LL_DMA_CHANNEL_1);
	}

    LL_SPI_DisableDMAReq_RX(SPI1);
    LL_SPI_DisableDMAReq_RX(SPI3);
   
	LL_SPI_Disable(SPI1);
	LL_SPI_Disable(SPI3);

    GPIOF->BSRR = IO_KV_CS_Pin | IO_MA_CS_Pin;

    sample_active = false;
    dma_done_mask = 0U;
}

static void finish_ads8325_burst_from_isr(ext_adc_result_status_t status,
		uint16_t kv_average, uint16_t ma_average)
{
	uint32_t completed_samples = sample_index;
	uint32_t completed_dma_mask = dma_done_mask;

	stop_ads8325_hardware();

	adc_state = EXT_ADC_IDLE;
	publish_ads8325_result_from_isr(status, kv_average, ma_average,
			completed_samples, completed_dma_mask);
}

static void publish_ads8325_result_from_isr(ext_adc_result_status_t status,
		uint16_t kv_average, uint16_t ma_average, uint32_t completed_samples,
		uint32_t completed_dma_mask)
{
	/*
	 * The SPI RX DMA IRQs deliberately run at priority 4, above the
	 * FreeRTOS max-syscall priority of 5. Keep this path FreeRTOS-free;
	 * the priority-5 TIM6 callback performs the fixed-cadence notification.
	 */
	latest_result.kv_average = kv_average;
	latest_result.ma_average = ma_average;
	latest_result.status = status;
	latest_result.sample_count = completed_samples;
	latest_result.dma_done_mask = completed_dma_mask;
	latest_result.sequence++;
	latest_result_available = true;
}

static void prepare_ads8325_spi(SPI_TypeDef *spi)
{
	LL_SPI_Disable(spi);
	LL_SPI_SetRxFIFOThreshold(spi, LL_SPI_RX_FIFO_TH_QUARTER);

	while (LL_SPI_GetRxFIFOLevel(spi) != LL_SPI_RX_FIFO_EMPTY)
	{
		(void)LL_SPI_ReceiveData8(spi);
	}
	LL_SPI_ClearFlag_OVR(spi);
}

static void wait_for_ads8325_cs_setup(void)
{
	/*
	 * The ADS8325 requires at least 20 ns from CS falling to the first
	 * rising DCLOCK edge. Complete the GPIO write, then provide more than
	 * 50 ns of margin at the maximum 72 MHz CPU clock.
	 */
	__DSB();
	__NOP();
	__NOP();
	__NOP();
	__NOP();
}

static void kv_dma_complete(DMA_HandleTypeDef *hdma)
{
	(void)hdma;
	handle_ads8325_dma_complete(SPI1, IO_KV_CS_Pin,
			EXT_ADC_KV_DONE, false);
}

static void ma_dma_complete(DMA_HandleTypeDef *hdma)
{
	(void)hdma;
	handle_ads8325_dma_complete(SPI3, IO_MA_CS_Pin,
			EXT_ADC_MA_DONE, false);
}

static void kv_dma_error(DMA_HandleTypeDef *hdma)
{
	(void)hdma;
	handle_ads8325_dma_complete(SPI1, IO_KV_CS_Pin,
			EXT_ADC_KV_DONE, true);
}

static void ma_dma_error(DMA_HandleTypeDef *hdma)
{
	(void)hdma;
	handle_ads8325_dma_complete(SPI3, IO_MA_CS_Pin,
			EXT_ADC_MA_DONE, true);
}
