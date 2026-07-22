/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.h
  * @brief          : Header for main.c file.
  *                   This file contains the common defines of the application.
  ******************************************************************************
  * @attention
  *
  * <h2><center>&copy; Copyright (c) 2019 STMicroelectronics.
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
#include "stm32f4xx_hal.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */

/* USER CODE END Includes */

/* Exported types ------------------------------------------------------------*/
/* USER CODE BEGIN ET */

extern ADC_HandleTypeDef hadc1;

extern I2C_HandleTypeDef hi2c1;
extern I2C_HandleTypeDef hi2c2;
extern I2C_HandleTypeDef hi2c3;

extern TIM_HandleTypeDef htim4;
extern TIM_HandleTypeDef htim10;

extern UART_HandleTypeDef huart2;

/* USER CODE END ET */

/* Exported constants --------------------------------------------------------*/
/* USER CODE BEGIN EC */

/* USER CODE END EC */

/* Exported macro ------------------------------------------------------------*/
/* USER CODE BEGIN EM */
#define I2C_BUS1               (&hi2c1)
#define I2C_BUS2               (&hi2c2)
#define I2C_BUS3               (&hi2c3)
/* USER CODE END EM */

/* Exported functions prototypes ---------------------------------------------*/
void Error_Handler(void);

/* USER CODE BEGIN EFP */

/* USER CODE END EFP */

/* Private defines -----------------------------------------------------------*/
#define IO_WATER_TEMP_Pin GPIO_PIN_0
#define IO_WATER_TEMP_GPIO_Port GPIOC
#define IO_WATER_PRESSURE_Pin GPIO_PIN_0
#define IO_WATER_PRESSURE_GPIO_Port GPIOA
#define IO_CB_TX_Pin GPIO_PIN_2
#define IO_CB_TX_GPIO_Port GPIOA
#define IO_CB_RX_Pin GPIO_PIN_3
#define IO_CB_RX_GPIO_Port GPIOA
#define IO_MAG_INT_Pin GPIO_PIN_4
#define IO_MAG_INT_GPIO_Port GPIOA
#define IO_USER_LED_Pin GPIO_PIN_5
#define IO_USER_LED_GPIO_Port GPIOA
#define IO_EE_HOLDn_Pin GPIO_PIN_4
#define IO_EE_HOLDn_GPIO_Port GPIOC
#define IO_PB_F2_Pin GPIO_PIN_5
#define IO_PB_F2_GPIO_Port GPIOC
#define IO_PB_F1_Pin GPIO_PIN_0
#define IO_PB_F1_GPIO_Port GPIOB
#define IO_PB_IMG_Pin GPIO_PIN_1
#define IO_PB_IMG_GPIO_Port GPIOB
#define IO_PB_LED_Pin GPIO_PIN_2
#define IO_PB_LED_GPIO_Port GPIOB
#define IO_CAP_SCL_Pin GPIO_PIN_10
#define IO_CAP_SCL_GPIO_Port GPIOB
#define IO_PB_LASER_Pin GPIO_PIN_12
#define IO_PB_LASER_GPIO_Port GPIOB
#define IO_INTERLOCK_OUT_Pin GPIO_PIN_13
#define IO_INTERLOCK_OUT_GPIO_Port GPIOB
#define RELAY_CTRL_Pin GPIO_PIN_15
#define RELAY_CTRL_GPIO_Port GPIOB
#define IO_PB_ZEROG_Pin GPIO_PIN_8
#define IO_PB_ZEROG_GPIO_Port GPIOC
#define IO_MAG_SDA_Pin GPIO_PIN_9
#define IO_MAG_SDA_GPIO_Port GPIOC
#define IO_MAG_SCL_Pin GPIO_PIN_8
#define IO_MAG_SCL_GPIO_Port GPIOA
#define IO_SPI_CSn_Pin GPIO_PIN_9
#define IO_SPI_CSn_GPIO_Port GPIOA
#define IO_LASER_CTRL_Pin GPIO_PIN_15
#define IO_LASER_CTRL_GPIO_Port GPIOA
#define IO_LED_AMBER_Pin GPIO_PIN_10
#define IO_LED_AMBER_GPIO_Port GPIOC
#define IO_LED_BLUE_Pin GPIO_PIN_11
#define IO_LED_BLUE_GPIO_Port GPIOC
#define IO_Ready_Pin GPIO_PIN_12
#define IO_Ready_GPIO_Port GPIOC
#define IO_CAP_SDA_Pin GPIO_PIN_3
#define IO_CAP_SDA_GPIO_Port GPIOB
#define IO_LED_RST_Pin GPIO_PIN_5
#define IO_LED_RST_GPIO_Port GPIOB
#define IO_WHITE_LEDS_Pin GPIO_PIN_6
#define IO_WHITE_LEDS_GPIO_Port GPIOB
#define IO_FLOW_Pin GPIO_PIN_7
#define IO_FLOW_GPIO_Port GPIOB
#define IO_LED_SCL_Pin GPIO_PIN_8
#define IO_LED_SCL_GPIO_Port GPIOB
#define IO_LED_SDA_Pin GPIO_PIN_9
#define IO_LED_SDA_GPIO_Port GPIOB

/* USER CODE BEGIN Private defines */

//#defined CALIBRATION_MODE

typedef enum
{
    IO_READY_STATE_NOT_READY = 0,
    IO_READY_STATE_READY     = 1
} IO_ReadyState_t;

/* USER CODE END Private defines */

#ifdef __cplusplus
}
#endif

#endif /* __MAIN_H */
