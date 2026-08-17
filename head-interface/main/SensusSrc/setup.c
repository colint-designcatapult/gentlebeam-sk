#include "stm32f4xx_hal.h"

#include "setup.h"
#include "adc.h"
#include "control_comm.h"
#include "flow.h"
#include "magnetometer.h"
#include "timer.h"
#include "sys_data.h"
#include "dotstar.h"
#include "led_ring.h"

#if !defined(CALIBRATION_MODE)
#include "buttons.h"
#include "collimator.h"
#include "leds.h"
#include "1wire_ds2482.h"
#include "qc.h"
#endif

//Run once at start to initialize peripherals
void run_setup()
{
	init_adc();
	init_control_comm();
	init_flow();
	init_magnetometer();
#if !defined(CALIBRATION_MODE)
	init_buttons();
	//init_leds();
	//init_collimator();
	init_1wire();
#endif
	init_rgb_strip();
	init_led_ring();
}

void run_post()
{
	//Can add items here if POST needed
}

//Continuously execute
void run_loop()
{
	process_adc();
#if !defined(CALIBRATION_MODE)
	process_buttons();
#endif
	process_control_comm();
	process_flow();
#if !defined(CALIBRATION_MODE)
	//process_leds();
#endif
	process_magnetometer();
#if !defined(CALIBRATION_MODE)
	process_collimator();
	process_qc();
#endif
	led_ring_tick(20);
}

void HAL_I2C_MemRxCpltCallback(I2C_HandleTypeDef *hi2c)
{
	//TBD TODO differentiate by handler
	/*if(hi2c->Instance == I2C2)
	{
		mag_i2c_rx_cb(2);
	}*/
	if(hi2c->Instance == I2C3)
	{
		mag_i2c_rx_cb(3);
	}
}

void HAL_I2C_MemTxCpltCallback(I2C_HandleTypeDef *hi2c)
{
	//TBD TODO differentiate by handler
	//led_tx_cb();
}

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
	if (huart->Instance == USART2)
	{
		control_comm_rx_cb();
	}
}

void HAL_UART_TxCpltCallback(UART_HandleTypeDef *huart)
{
	if (huart->Instance == USART2)
	{
		control_comm_tx_cb();
	}
}

void HAL_ADC_ConvCpltCallback(ADC_HandleTypeDef *hadc)
{
	if(hadc->Instance == ADC1)
	{
		adc_cb();
	}
}
