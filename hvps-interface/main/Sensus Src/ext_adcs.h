#ifndef EXT_ADCS_H_
#define EXT_ADCS_H_

#include "stdbool.h"
#include "stdint.h"
#include "FreeRTOS.h"
#include "task.h"
#include "stm32f3xx_ll_spi.h"
#include "stm32f3xx_ll_dma.h"


typedef enum
{
	EXT_ADC_RESULT_VALID = 0,
	EXT_ADC_RESULT_FRAMING_ERROR,
	EXT_ADC_RESULT_DMA_ERROR,
	EXT_ADC_RESULT_TIMING_ERROR
} ext_adc_result_status_t;

typedef struct
{
	uint32_t sequence;
	uint16_t kv_average;
	uint16_t ma_average;
	ext_adc_result_status_t status;
	uint32_t sample_count;
	uint32_t dma_done_mask;
} ext_adc_result_t;

void setup_ext_adcs(void);
void ext_adcs_set_result_task(TaskHandle_t task);
void ext_adcs_start_burst_from_isr(void);
void ext_adcs_check_completion_from_isr(void);
bool ext_adcs_get_latest_result(ext_adc_result_t *result);

#endif /* EXT_ADCS_H_ */
