/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.h
  * @brief          : Header for main.c file.
  *                   This file contains the common defines of the application.
  ******************************************************************************
  * @attention
  *
  * <h2><center>&copy; Copyright (c) 2020 STMicroelectronics.
  * All rights reserved.</center></h2>
  *
  * This software component is licensed by ST under BSD 3-Clause license,
  * the "License"; You may not use this file except in compliance with the
  * License. You may obtain a copy of the License at:
  *                        opensource.org/licenses/BSD-3-Clause
  *
  ******************************************************************************
  */
/* USER CODE END Header */

/* Define to prevent recursive inclusion -------------------------------------*/
#ifndef __MAIN_H
#define __MAIN_H

#ifdef __cplusplus
extern "C" {
#endif

/* Includes ------------------------------------------------------------------*/
#include "stm32f3xx_hal.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */

extern ADC_HandleTypeDef hadc1;
extern DMA_HandleTypeDef hdma_adc1;

extern DAC_HandleTypeDef hdac1;
extern DAC_HandleTypeDef hdac2;

extern SPI_HandleTypeDef hspi1;
extern SPI_HandleTypeDef hspi2;
extern SPI_HandleTypeDef hspi3;

extern UART_HandleTypeDef huart1;
extern UART_HandleTypeDef huart2;
extern UART_HandleTypeDef huart3;

extern TIM_HandleTypeDef htim7;

/* USER CODE END Includes */

/* Exported types ------------------------------------------------------------*/
/* USER CODE BEGIN ET */
extern volatile int32_t test_ms;
/* USER CODE END ET */

/* Exported constants --------------------------------------------------------*/
/* USER CODE BEGIN EC */

/* USER CODE END EC */

/* Exported macro ------------------------------------------------------------*/
/* USER CODE BEGIN EM */

/* USER CODE END EM */

/* Exported functions prototypes ---------------------------------------------*/
void Error_Handler(void);

/* USER CODE BEGIN EFP */

/* USER CODE END EFP */

/* Private defines -----------------------------------------------------------*/
#define IO_FAN_FAULT_Pin GPIO_PIN_2
#define IO_FAN_FAULT_GPIO_Port GPIOE
#define IO_PFC_OK_Pin GPIO_PIN_3
#define IO_PFC_OK_GPIO_Port GPIOE
#define IO_PFC_ALLOWED_Pin GPIO_PIN_4
#define IO_PFC_ALLOWED_GPIO_Port GPIOE
#define IO_HV_INT_IN_Pin GPIO_PIN_5
#define IO_HV_INT_IN_GPIO_Port GPIOE
#define IO_HV_ALLOWED_Pin GPIO_PIN_6
#define IO_HV_ALLOWED_GPIO_Port GPIOE
#define IO_GRID_ON_Pin GPIO_PIN_13
#define IO_GRID_ON_GPIO_Port GPIOC
#define IO_CATHODE_ARC_Pin GPIO_PIN_14
#define IO_CATHODE_ARC_GPIO_Port GPIOC
#define IO_KV_CS_Pin GPIO_PIN_9
#define IO_KV_CS_GPIO_Port GPIOF
#define IO_MA_CS_Pin GPIO_PIN_10
#define IO_MA_CS_GPIO_Port GPIOF
#define IO_5V_REF_ADC_Pin GPIO_PIN_0
#define IO_5V_REF_ADC_GPIO_Port GPIOC
#define IO_5V_ADC_Pin GPIO_PIN_1
#define IO_5V_ADC_GPIO_Port GPIOC
#define IO_PS_15_ADC_Pin GPIO_PIN_2
#define IO_PS_15_ADC_GPIO_Port GPIOC
#define IO_PS_24_ADC_Pin GPIO_PIN_3
#define IO_PS_24_ADC_GPIO_Port GPIOC
#define IO_15V_ADC_Pin GPIO_PIN_0
#define IO_15V_ADC_GPIO_Port GPIOA
#define IO_3_45V_ADC_Pin GPIO_PIN_1
#define IO_3_45V_ADC_GPIO_Port GPIOA
#define IO_FIL_A_ADC_Pin GPIO_PIN_2
#define IO_FIL_A_ADC_GPIO_Port GPIOA
#define IO_FIL_V_ADC_Pin GPIO_PIN_3
#define IO_FIL_V_ADC_GPIO_Port GPIOA
#define IO_GRID_ADC_Pin GPIO_PIN_7
#define IO_GRID_ADC_GPIO_Port GPIOA
#define IO_MA_LIM_ADC_Pin GPIO_PIN_4
#define IO_MA_LIM_ADC_GPIO_Port GPIOC
#define IO_FIL_SP_ADC_Pin GPIO_PIN_5
#define IO_FIL_SP_ADC_GPIO_Port GPIOC
#define IO_KV_SP_ADC_Pin GPIO_PIN_0
#define IO_KV_SP_ADC_GPIO_Port GPIOB
#define IO_GRID_SP_ADC_Pin GPIO_PIN_1
#define IO_GRID_SP_ADC_GPIO_Port GPIOB
#define IO_HV_STATUS_Pin GPIO_PIN_7
#define IO_HV_STATUS_GPIO_Port GPIOE
#define IO_OC_FAULT_24V_Pin GPIO_PIN_8
#define IO_OC_FAULT_24V_GPIO_Port GPIOE
#define IO_MASTER_FAULT_Pin GPIO_PIN_9
#define IO_MASTER_FAULT_GPIO_Port GPIOE
#define IO_OC_FAULT_HV_Pin GPIO_PIN_10
#define IO_OC_FAULT_HV_GPIO_Port GPIOE
#define IO_TEMP_FAULT_1_Pin GPIO_PIN_11
#define IO_TEMP_FAULT_1_GPIO_Port GPIOE
#define IO_OC_CATHODE_Pin GPIO_PIN_12
#define IO_OC_CATHODE_GPIO_Port GPIOE
#define IO_TEMP_FAULT_3_Pin GPIO_PIN_13
#define IO_TEMP_FAULT_3_GPIO_Port GPIOE
#define IO_TEMP_FAULT_2_Pin GPIO_PIN_14
#define IO_TEMP_FAULT_2_GPIO_Port GPIOE
#define IO_FTDI_RX_Pin GPIO_PIN_15
#define IO_FTDI_RX_GPIO_Port GPIOE
#define IO_FTDI_TX_Pin GPIO_PIN_10
#define IO_FTDI_TX_GPIO_Port GPIOB
#define IO_DAC_MOSI_Pin GPIO_PIN_15
#define IO_DAC_MOSI_GPIO_Port GPIOB
#define IO_DAC_SCK_Pin GPIO_PIN_8
#define IO_DAC_SCK_GPIO_Port GPIOD
#define IO_FIL_DAC_CS_Pin GPIO_PIN_9
#define IO_FIL_DAC_CS_GPIO_Port GPIOD
#define IO_MA_DAC_CS_Pin GPIO_PIN_10
#define IO_MA_DAC_CS_GPIO_Port GPIOD
#define IO_KV_DAC_CS_Pin GPIO_PIN_11
#define IO_KV_DAC_CS_GPIO_Port GPIOD
#define IO_GRID_DAC_CS_Pin GPIO_PIN_12
#define IO_GRID_DAC_CS_GPIO_Port GPIOD
#define IO_TEST_1_Pin GPIO_PIN_13
#define IO_TEST_1_GPIO_Port GPIOD
#define IO_TEST_2_Pin GPIO_PIN_14
#define IO_TEST_2_GPIO_Port GPIOD
#define IO_TEST_3_Pin GPIO_PIN_15
#define IO_TEST_3_GPIO_Port GPIOD
#define IO_KV_SCK_Pin GPIO_PIN_7
#define IO_KV_SCK_GPIO_Port GPIOC
#define IO_KV_MISO_Pin GPIO_PIN_8
#define IO_KV_MISO_GPIO_Port GPIOC
#define IO_ION_PUMP_TX_Pin GPIO_PIN_9
#define IO_ION_PUMP_TX_GPIO_Port GPIOA
#define IO_ION_PUMP_RX_Pin GPIO_PIN_10
#define IO_ION_PUMP_RX_GPIO_Port GPIOA
#define IO_MA_SCK_Pin GPIO_PIN_10
#define IO_MA_SCK_GPIO_Port GPIOC
#define IO_MA_MISO_Pin GPIO_PIN_11
#define IO_MA_MISO_GPIO_Port GPIOC
#define IO_SEND_GRID_STAT_Pin GPIO_PIN_0
#define IO_SEND_GRID_STAT_GPIO_Port GPIOD
#define IO_SEND_ARC_STAT_Pin GPIO_PIN_1
#define IO_SEND_ARC_STAT_GPIO_Port GPIOD
#define IO_SEND_READY_Pin GPIO_PIN_2
#define IO_SEND_READY_GPIO_Port GPIOD
#define IO_SEND_HV_STAT_Pin GPIO_PIN_3
#define IO_SEND_HV_STAT_GPIO_Port GPIOD
#define IO_SEND_WARNING_Pin GPIO_PIN_4
#define IO_SEND_WARNING_GPIO_Port GPIOD
#define IO_CTRL_TX_Pin GPIO_PIN_5
#define IO_CTRL_TX_GPIO_Port GPIOD
#define IO_CTRL_RX_Pin GPIO_PIN_6
#define IO_CTRL_RX_GPIO_Port GPIOD
#define IO_PS_OK_Pin GPIO_PIN_4
#define IO_PS_OK_GPIO_Port GPIOB
#define IO_WD_RST_Pin GPIO_PIN_5
#define IO_WD_RST_GPIO_Port GPIOB
#define IO_NO_GRID_CLK_Pin GPIO_PIN_6
#define IO_NO_GRID_CLK_GPIO_Port GPIOB
#define IO_NO_FIL_CLK_Pin GPIO_PIN_7
#define IO_NO_FIL_CLK_GPIO_Port GPIOB
#define IO_GRID_INT_IN_Pin GPIO_PIN_8
#define IO_GRID_INT_IN_GPIO_Port GPIOB
#define IO_BEAM_EN_IN_Pin GPIO_PIN_9
#define IO_BEAM_EN_IN_GPIO_Port GPIOB
#define IO_BEAM_ALLOWED_Pin GPIO_PIN_0
#define IO_BEAM_ALLOWED_GPIO_Port GPIOE
#define IO_GRID_CLK_Pin GPIO_PIN_1
#define IO_GRID_CLK_GPIO_Port GPIOE

/* USER CODE BEGIN Private defines */

/* USER CODE END Private defines */

#ifdef __cplusplus
}
#endif

#endif /* __MAIN_H */
