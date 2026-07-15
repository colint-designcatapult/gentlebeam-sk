#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"

#include "setup.h"
#include "adc.h"
#include "checksum.h"
#include "control_comm.h"
#include "ext_adcs.h"
#include "ext_dacs.h"
#include "ftdi.h"
#include "io.h"
#include "monitoring.h"
#include "processing.h"
#include "timers.h"

volatile bool spi1_recovery_needed = false;
volatile bool spi3_recovery_needed = false;

//Run once at startup to initialize peripherals
void run_setup()
{
	//Delay for slower supplies to come up
	//Call before initializing any peripherals to prevent backfeeding into unpowered ICs
	HAL_Delay(1000);

	//Initialize internal DACs for testing
	DAC1->CR |= 0x10001;
	DAC2->CR |= 1;

	setup_adc();
	setup_control_comm();
	setup_ext_adcs();
	setup_ext_dacs();
	setup_ftdi();
	setup_io();
	setup_system_monitoring();

	init_crc32_tab();

	//DAC1->DHR8R1 = 0x60;
	//DAC1->DHR8R2 = 0x70;
	//DAC2->DHR8R1 = 0x70;

	enable_grid_clock();
	enable_runtime_timer();
}

//Run continuously
void run_loop()
{
	process_adc();
	process_control_comm();
	process_ext_adcs();
	process_ext_dacs();
	process_ftdi();
	process_io();
	process_monitoring();
}

void HAL_SPI_ErrorCallback(SPI_HandleTypeDef *hspi) {
	if(hspi->Instance == SPI1)
	{
		spi1_recovery_needed = true;
	}
	else if(hspi->Instance == SPI3)
	{
		spi3_recovery_needed = true;
	}
}

//HAL callbacks here will trigger the module specific functions
void HAL_SPI_TxCpltCallback(SPI_HandleTypeDef *hspi)
{
	if(hspi->Instance == SPI2)
	{
		ext_dac_tx_done();
	}
}

void HAL_SPI_RxCpltCallback(SPI_HandleTypeDef *hspi)
{
	if(hspi->Instance == SPI1)
	{
		ext_kv_rx_done();
	}
	else if(hspi->Instance == SPI3)
	{
		ext_ma_rx_done();
	}
}


void HAL_UART_TxCpltCallback(UART_HandleTypeDef *huart)
{
	if(huart->Instance == USART2)
	{
		comm_tx_cb();
	}
	else if(huart->Instance == USART3)
	{
		ftdi_tx_cb();
	}
}

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
	if(huart->Instance == USART2)
	{
		comm_rx_cb();
	}
	else if(huart->Instance == USART3)
	{
		ftdi_rx_cb();
	}
}

void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
	if(htim->Instance == TIM7)
	{
		HAL_GPIO_TogglePin(GPIOE, IO_GRID_CLK_Pin);
	}
	else if(htim->Instance == TIM6)
	{
		runtime_ms++;
	}
}
