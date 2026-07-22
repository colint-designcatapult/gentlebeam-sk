/*
 * qc.c
 *
 *  Created on: Sep 5, 2025
 *      Author: Steve Holman
 */

#include "qc.h"
#include "main.h"
#include <stdbool.h>
#include "sys_data.h"
#include "timer.h"

extern I2C_HandleTypeDef hi2c1;
volatile int32_t qc_reset_count_ms = 0;

uint16_t QC1_data = 0;
uint16_t QC2_data = 0;

static uint8_t adc_rx_buf[2];
static uint8_t current_devAddr = 0;
static uint32_t conversion_start_tick = 0;
static volatile bool adc_ready = true;
static volatile bool adc1_needs_prime = true;
static volatile bool adc2_needs_prime = true;
static volatile bool adc1_discard_next = true;
static volatile bool adc2_discard_next = true;
static volatile bool adc_tx_cplt = false;

static volatile bool read_first = true;
static volatile uint8_t error_count = 0;
static volatile bool reset_i2c1 = false;
static volatile bool reset_i2c2 = false;

#define DEBOUNCE_TIME_MS 				100

/* Start asynchronous conversion read */
HAL_StatusTypeDef Read_ADC121C021_Conversion_IT(uint8_t devAddr)
{
	if (hi2c1.State != HAL_I2C_STATE_READY || HAL_GPIO_ReadPin(IO_Ready_GPIO_Port, IO_Ready_Pin) != IO_READY_STATE_READY) {
	    return HAL_BUSY;
	}

    uint8_t cmd = CONVERSION_REG;
    adc_ready = false;
    current_devAddr = devAddr;

    // Step 1: transmit register pointer (non-blocking)
    return HAL_I2C_Master_Transmit_IT(&hi2c1, devAddr, &cmd, 1);
}

/* Edge detection function for pin IO_Ready */
static void IO_Ready_Edge_Detect(void)
{
	static IO_ReadyState_t last_stable = IO_READY_STATE_NOT_READY;
	static IO_ReadyState_t last_sample = IO_READY_STATE_NOT_READY;
	static uint32_t last_change_time = 0;

	IO_ReadyState_t current = HAL_GPIO_ReadPin(IO_Ready_GPIO_Port, IO_Ready_Pin);
	uint32_t now = HAL_GetTick();

	// Detect change in raw signal
	if (current != last_sample) {
		last_change_time = now;
		last_sample = current;
	}

	// If stable long enough → accept new state
	if ((now - last_change_time) >= DEBOUNCE_TIME_MS) {
		if (last_stable != current) {
			// Edge detected
			if (current == IO_READY_STATE_READY) {
//				HAL_UART_Transmit_IT(&huart6,
//					(uint8_t*)"IO_READY_RISING_EDGE\n",
//					strlen("IO_READY_RISING_EDGE\n"));
				// Reset I2C1 peripheral
				reset_i2c1 = true;
			} else {
//				HAL_UART_Transmit_IT(&huart6,
//					(uint8_t*)"IO_READY_FALLING_EDGE\n",
//					strlen("IO_READY_FALLING_EDGE\n"));
			}
			last_stable = current;
		}
	}
}

/* Process function: kick off new reads if bus is free */
void process_qc(void)
{
	IO_Ready_Edge_Detect();

	//Check if the i2c bus is stuck first
	//static uint32_t i2c_stuck_counter = 0;

	// Check if bus appears stuck
	if (hi2c1.State != HAL_I2C_STATE_READY)
	{
		if (reset_i2c1 && qc_reset_count_ms > 200)
		{
			reset_i2c1 = false;
			qc_reset_count_ms = 0;
			I2C_ForceBusRecovery(&hi2c1);
		}
		return;
	}
	else
	{
		qc_reset_count_ms = 0; // reset counter when bus healthy
	}

	if (reset_i2c1) {
		reset_i2c1 = false;
		I2C_Reset(&hi2c1);  // clear BUSY
		return;
	}
	if (reset_i2c2) {
		reset_i2c2 = false;
		I2C_Reset(&hi2c2);  // clear BUSY
		return;
	}

	if (adc1_needs_prime && adc_ready)
	{
		adc1_needs_prime = false;
		/* Force ADC internal pointer + start conversion */
		Read_ADC121C021_Conversion_IT(ADC1_ADDRESS);
		adc1_discard_next = true;
		return;
	}
	if (adc2_needs_prime && adc_ready)
	{
		adc2_needs_prime = false;
	    Read_ADC121C021_Conversion_IT(ADC2_ADDRESS);
	    adc2_discard_next = true;
	    return;
	}


	if (adc_tx_cplt && (HAL_GetTick() - conversion_start_tick) >= 2)
	{
		adc_tx_cplt = false;
		// Step 2: read two data bytes
		//HAL_I2C_Master_Receive_IT(&hi2c1, current_devAddr, adc_rx_buf, 2);
		if (HAL_I2C_Master_Receive_IT(&hi2c1, current_devAddr, adc_rx_buf, 2) != HAL_OK)
		{
		    adc_ready = true;
		    adc1_needs_prime = true;
		    adc2_needs_prime = true;
		}
	}

	//Then move ahead with reading it
    if (adc_ready && hi2c1.State == HAL_I2C_STATE_READY)
    {
    	HAL_StatusTypeDef status;

        // Alternate between ADC1 and ADC2
        if (read_first) {
            status = Read_ADC121C021_Conversion_IT((uint8_t)ADC1_ADDRESS);
        } else {
            status = Read_ADC121C021_Conversion_IT((uint8_t)ADC2_ADDRESS);
        }

        if (status != HAL_OK) {
			error_count++;
			if (error_count > 3) {
				I2C_Reset(&hi2c1);   // try to recover after 3 consecutive failures
				error_count = 0;
			}
		} else {
			read_first = !read_first;
			error_count = 0;
		}
    }

}

void I2C_Reset(I2C_HandleTypeDef *hi2c)
{
    adc_tx_cplt = false;
    adc_ready = true;
    adc1_needs_prime = true;
    adc2_needs_prime = true;
    adc1_discard_next = true;
    adc2_discard_next = true;

	__HAL_I2C_DISABLE(hi2c);
    __HAL_I2C_CLEAR_FLAG(hi2c, I2C_FLAG_BERR | I2C_FLAG_ARLO | I2C_FLAG_AF | I2C_FLAG_OVR);
	// Reset HAL state & error
	hi2c->State = HAL_I2C_STATE_RESET;
	hi2c->ErrorCode = HAL_I2C_ERROR_NONE;

    // Re-init (uses hi2c->Init filled by CubeMX)
	if (HAL_I2C_Init(hi2c) != HAL_OK)
	{

	}

    __HAL_I2C_ENABLE(hi2c);
    hi2c->State = HAL_I2C_STATE_READY;
}


void I2C_ForceBusRecovery(I2C_HandleTypeDef *hi2c)
{
    GPIO_InitTypeDef GPIO_InitStruct = {0};
    adc_tx_cplt = false;
    adc_ready = true;
    adc1_needs_prime = true;
    adc2_needs_prime = true;
    adc1_discard_next = true;
    adc2_discard_next = true;

    // 1️⃣ Deinitialize the I2C peripheral
    HAL_I2C_DeInit(hi2c);

    // 2️⃣ Configure SCL and SDA as GPIO outputs (open-drain)
    if (hi2c->Instance == I2C1)
    {
        __HAL_RCC_GPIOB_CLK_ENABLE();
        GPIO_InitStruct.Pin = GPIO_PIN_8 | GPIO_PIN_9; // PB8=SCL, PB9=SDA for I2C1
        GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_OD;
        GPIO_InitStruct.Pull = GPIO_NOPULL;
        GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
        HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

        // 3️⃣ Toggle SCL manually ~10 times to free the line
        for (int i = 0; i < 10; i++)
        {
            HAL_GPIO_WritePin(GPIOB, GPIO_PIN_8, GPIO_PIN_SET);
			for (volatile int delay = 0; delay < 10; delay++) {};
            HAL_GPIO_WritePin(GPIOB, GPIO_PIN_8, GPIO_PIN_RESET);
			for (volatile int delay = 0; delay < 10; delay++) {};
        }

        // 4️⃣ Check if SDA is now released
        if (HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_9) == GPIO_PIN_RESET)
        {
            // SDA still stuck — perform full peripheral reset
            __HAL_RCC_I2C1_FORCE_RESET();
			for (volatile int delay = 0; delay < 100; delay++) {};
            __HAL_RCC_I2C1_RELEASE_RESET();
        }

    }
    if (hi2c->Instance == I2C2)
	{
		__HAL_RCC_GPIOB_CLK_ENABLE();
		GPIO_InitStruct.Pin = GPIO_PIN_3 | GPIO_PIN_10;
		GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_OD;
		GPIO_InitStruct.Pull = GPIO_NOPULL;
		GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
		HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

		// 3️⃣ Toggle SCL manually ~10 times to free the line
		for (int i = 0; i < 10; i++)
		{
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_SET);
			for (volatile int delay = 0; delay < 10; delay++) {};
			HAL_GPIO_WritePin(GPIOB, GPIO_PIN_10, GPIO_PIN_RESET);
			for (volatile int delay = 0; delay < 10; delay++) {};
		}

		// 4️⃣ Check if SDA is now released
		if (HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_3) == GPIO_PIN_RESET)
		{
			// SDA still stuck — perform full peripheral reset
			__HAL_RCC_I2C2_FORCE_RESET();
			for (volatile int delay = 0; delay < 100; delay++) {};
			__HAL_RCC_I2C2_RELEASE_RESET();
		}

	}

    // 5️⃣ Reinitialize I2C pins and peripheral
    HAL_I2C_Init(hi2c);
}




/* Called when TX completes (register pointer sent) */
void HAL_I2C_MasterTxCpltCallback(I2C_HandleTypeDef *hi2c)
{
    if (hi2c->Instance == I2C1)
    {
    	conversion_start_tick = HAL_GetTick();
    	adc_tx_cplt = true;
        // Step 2: read two data bytes
    	//HAL_I2C_Master_Receive_IT(hi2c, current_devAddr, adc_rx_buf, 2);

    }
}

/* Called when RX completes (data received) */
void HAL_I2C_MasterRxCpltCallback(I2C_HandleTypeDef *hi2c)
{
    if (hi2c->Instance == I2C1)
    {
    	// Combine two bytes and mask to get 12-bit ADC result
    	uint16_t result = (((uint16_t)adc_rx_buf[0] << 8) | adc_rx_buf[1]) & 0x0FFF;

        /*if (current_devAddr == (uint8_t)ADC1_ADDRESS) {
            QC1_data = result;
        }
        if (current_devAddr == (uint8_t)ADC2_ADDRESS) {
            QC2_data = result;
        }*/

        if (current_devAddr == ADC1_ADDRESS)
        {
            if (adc1_discard_next) {
                adc1_discard_next = false;   // throw away this sample
            } else {
                QC1_data = result;
            }
        }
        else if (current_devAddr == ADC2_ADDRESS)
        {
            if (adc2_discard_next) {
                adc2_discard_next = false;
            } else {
                QC2_data = result;
            }
        }

        adc_ready = true;
    }
}

void HAL_I2C_ErrorCallback(I2C_HandleTypeDef *hi2c)
{
    if (hi2c->Instance == I2C1) {
        adc_tx_cplt = false;
        adc_ready = true; // allow retry
        adc1_needs_prime = true;
        adc2_needs_prime = true;
        adc1_discard_next = true;
        adc2_discard_next = true;
        reset_i2c1 = true;
        //I2C_Reset(hi2c);  // clear BUSY
    }
    else if (hi2c->Instance == I2C2) {
    	reset_i2c2 = true;
    	//I2C_Reset(hi2c);
    }

}
