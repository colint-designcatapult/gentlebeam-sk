#include "main.h"
#include "stdbool.h"
#include "stm32f3xx_hal.h"

#include "ext_adcs.h"
#include "monitoring.h"
#include "timers.h"

uint8_t kv_rx_buf[3];
uint8_t ma_rx_buf[3];
uint8_t ma_median_idx;

volatile uint8_t ext_adc_flags;

float kv_sum = 0;
float ma_sum = 0;

uint32_t kv_cir_buf[EXT_ADC_KV_BUF_SIZE];
uint32_t kv_buf_idx = 0;
uint32_t kv_buf_sum = 0;
uint32_t ma_cir_buf[EXT_ADC_MA_BUF_SIZE];
uint32_t ma_buf_idx = 0;
uint32_t ma_buf_sum = 0;
uint32_t ma_median_buf[EXT_ADC_MA_MEDIAN_SIZE];

extern volatile bool spi1_recovery_needed;
extern volatile bool spi3_recovery_needed;

void setup_ext_adcs()
{
	ext_adc_flags = 0;

	//TBD TODO placeholder initial startup delay
	ext_adc_ms = 10;

	ma_median_idx = 0;
	for(int i=0;i<EXT_ADC_MA_MEDIAN_SIZE;i++)
	{
		ma_median_buf[i] = 0;
	}
	//Initialize CS lines
	HAL_GPIO_WritePin(GPIOF, IO_KV_CS_Pin|IO_MA_CS_Pin, GPIO_PIN_SET);

	//Dummy receive to initialize SPI ports
	if (HAL_SPI_Receive_DMA(&hspi1, kv_rx_buf, 3) != HAL_OK) {
		spi1_recovery_needed = true;
	}
	if (HAL_SPI_Receive_DMA(&hspi3, ma_rx_buf, 3) != HAL_OK) {
		spi3_recovery_needed = true;
	}
}

void process_ext_adcs()
{
	spi1_recovery_handler();
	spi3_recovery_handler();

	if(ext_adc_flags != EXT_ADC_DONE)
	{
		if(ext_adc_ms < -100)
		{
			HAL_GPIO_WritePin(GPIOF, IO_KV_CS_Pin|IO_MA_CS_Pin, GPIO_PIN_SET);
			ext_adc_flags = 0;
			HAL_GPIO_WritePin(GPIOF, IO_KV_CS_Pin|IO_MA_CS_Pin, GPIO_PIN_RESET);
			HAL_SPI_Receive_DMA(&hspi1, kv_rx_buf, 3);
			HAL_SPI_Receive_DMA(&hspi3, ma_rx_buf, 3);
			ext_adc_ms = 3; //TBD TODO placeholder value
		}

		return;
	}

	if(ext_adc_ms > 0)
	{
	    return;
	}

	uint32_t adc_val = 0;

	adc_val = (kv_rx_buf[0] & 0x07);
	adc_val <<= 8;
	adc_val |= kv_rx_buf[1];
	adc_val <<= 8;
	adc_val |= (kv_rx_buf[2] & 0xF8);
	adc_val >>= 3;

	kv_buf_idx++;

	if (kv_buf_idx >= EXT_ADC_KV_BUF_SIZE) {
		kv_buf_idx = 0;
	}

	kv_buf_sum -= kv_cir_buf[kv_buf_idx];
	kv_buf_sum += adc_val;
	kv_cir_buf[kv_buf_idx] = adc_val;

	report_kv_fb(kv_buf_sum / EXT_ADC_KV_BUF_SIZE);

	adc_val = (ma_rx_buf[0] & 0x07);
	adc_val <<= 8;
	adc_val |= ma_rx_buf[1];
	adc_val <<= 8;
	adc_val |= (ma_rx_buf[2] & 0xF8);
	adc_val >>= 3;

	// use median filter to remove transients
	ma_median_buf[ma_median_idx] = adc_val;
	ma_median_idx++;

	if(ma_median_idx >= EXT_ADC_MA_MEDIAN_SIZE)
	{
		ma_median_idx = 0;
	}

	uint32_t ma_1 = ma_median_buf[0];
	uint32_t ma_2 = ma_median_buf[1];
	uint32_t ma_3 = ma_median_buf[2];

	if (ma_1 > ma_2)
	{
		uint32_t ma_temp = ma_1;
		ma_1 = ma_2;
		ma_2 = ma_temp;
	}
	if (ma_2 > ma_3)
	{
		uint32_t ma_temp = ma_2;
		ma_2 = ma_3;
		ma_3 = ma_temp;
	}
	if (ma_1 > ma_2)
	{
		uint32_t ma_temp = ma_1;
		ma_1 = ma_2;
		ma_2 = ma_temp;
	}
	adc_val = ma_2;

	// circular buffer for averaging
	ma_buf_idx++;

	if (ma_buf_idx >= EXT_ADC_MA_BUF_SIZE) {
		ma_buf_idx = 0;
	}

	ma_buf_sum -= ma_cir_buf[ma_buf_idx];
	ma_buf_sum += adc_val;
	ma_cir_buf[ma_buf_idx] = adc_val;

	report_ma_fb(ma_buf_sum / EXT_ADC_MA_BUF_SIZE);

	ext_adc_flags = 0;

	HAL_GPIO_WritePin(GPIOF, IO_KV_CS_Pin|IO_MA_CS_Pin, GPIO_PIN_RESET);
	if (HAL_SPI_Receive_DMA(&hspi1, kv_rx_buf, 3) != HAL_OK) {
		spi1_recovery_needed = true;
	}
	if (HAL_SPI_Receive_DMA(&hspi3, ma_rx_buf, 3) != HAL_OK) {
		spi3_recovery_needed = true;
	}

	ext_adc_ms = 3; //TBD TODO placeholder value
}

void update_ext_adcs()
{

}


void ext_kv_rx_done()
{
	HAL_GPIO_WritePin(GPIOF, IO_KV_CS_Pin, GPIO_PIN_SET);
	ext_adc_flags |= EXT_ADC_KV_DONE;
}

void ext_ma_rx_done()
{
	HAL_GPIO_WritePin(GPIOF, IO_MA_CS_Pin, GPIO_PIN_SET);
	ext_adc_flags |= EXT_ADC_MA_DONE;
}

void spi1_recovery_handler(void) {
	if (spi1_recovery_needed)
	{
	    spi1_recovery_needed = false;

	    HAL_SPI_DeInit(&hspi1);

	    __HAL_RCC_SPI1_FORCE_RESET();
	    __HAL_RCC_SPI1_RELEASE_RESET();

	    HAL_SPI_Init(&hspi1);
	}
}

void spi3_recovery_handler(void) {
	if (spi3_recovery_needed)
	{
	    spi3_recovery_needed = false;

	    HAL_SPI_DeInit(&hspi3);

	    __HAL_RCC_SPI3_FORCE_RESET();
	    __HAL_RCC_SPI3_RELEASE_RESET();

	    HAL_SPI_Init(&hspi3);
	}
}
