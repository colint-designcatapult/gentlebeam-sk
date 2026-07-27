#include <string.h>
#include <stdbool.h>
#include "stm32f4xx_hal.h"
#include "main.h"
#include "lis3mdl.h"
#include "lis2mdl.h"

#include "magnetometer.h"
#include "timer.h"
#include "sys_data.h"

#define USE_CHAINED_MAG 1

static HAL_StatusTypeDef I2C3_BusRecovery(void);

volatile int32_t read_mag_ms = 100;
volatile int mag_rx_missing = 0;
volatile bool read_chained_mag = false;
volatile bool chained_mag_complete = false;
int16_t magnetometer_rx_0[NUM_MAG_FIELDS];
int16_t magnetometer_rx_1[NUM_MAG_FIELDS];
int16_t magnetometer_rx_2[NUM_MAG_FIELDS];

bool bus_3_toggle = false;


void init_magnetometer()
{
	if (I2C3_BusRecovery() != HAL_OK) {
		Error_Handler();
	}
#if defined (USE_LIS2MDL)
	if (LIS2MDL_Init(&hi2c3) == HAL_OK) {
		// Sensor initialized
	}
#elif defined (USE_LIS3MDL)
    if (LIS3MDL_Init(&hi2c3) == HAL_OK) {
        // Sensor initialized
    }
#endif

	//Reset buffer and request RX
	memset(magnetometer_rx_0, 0, NUM_MAG_FIELDS*sizeof(int16_t));
	memset(magnetometer_rx_1, 0, NUM_MAG_FIELDS*sizeof(int16_t));

/*
	volatile uint8_t hal_return_val = 0;
	uint8_t i2c_val = KMX62_RATE_VAL;

	//From Giovanni, steps a-d
	// This initialization code has been written based on Kionix App note TN005 section 3.2.1
	//a. Following the power up, write 0x00 to internal register 0x7F using Slave Address #1 as specified
	//in Table 2 or Table 3 depending of the connection of ADDR pin
	//hal_return_val = HAL_I2C_Mem_Write(&hi2c2, KMX62_ADDR, 0x7F, I2C_MEMADD_SIZE_8BIT, 0x00, 1, 100);
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR, 0x7F, I2C_MEMADD_SIZE_8BIT, 0x00, 1, 100);
	HAL_Delay(5);

	//b. Disable device
	//hal_return_val = HAL_I2C_Mem_Write(&hi2c2, KMX62_ADDR, KMX62_CTRL_REG_2, I2C_MEMADD_SIZE_8BIT, 0x00, 1, 100);
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR, KMX62_CTRL_REG_2, I2C_MEMADD_SIZE_8BIT, 0x00, 1, 100);
	HAL_Delay(5);

	//c. Software Reset
	//hal_return_val = HAL_I2C_Mem_Write(&hi2c2, KMX62_ADDR, KMX62_CTRL_REG_1, I2C_MEMADD_SIZE_8BIT, 0x00, 1, 100);
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR, KMX62_CTRL_REG_1, I2C_MEMADD_SIZE_8BIT, 0x00, 1, 100);
	HAL_Delay(5);

	//d. Who Am I?
	//hal_return_val = HAL_I2C_Mem_Read(&hi2c2, KMX62_ADDR, KMX62_WHO_AM_I, 0x01, &i2c_val, 1, 100);
	hal_return_val = HAL_I2C_Mem_Read(&hi2c3, KMX62_ADDR, KMX62_WHO_AM_I, 0x01, &i2c_val, 1, 100);
	HAL_Delay(5);

	//Set up magnetometer sampling rate
	//i2c_val = KMX62_RATE_VAL;
	//hal_return_val = HAL_I2C_Mem_Write(&hi2c2, KMX62_ADDR, KMX62_RATE_REG, I2C_MEMADD_SIZE_8BIT, &i2c_val, 1, 100);
	i2c_val = KMX62_RATE_VAL;
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR, KMX62_RATE_REG, I2C_MEMADD_SIZE_8BIT, &i2c_val, 1, 100);
	HAL_Delay(5);
#ifdef USE_CHAINED_MAG
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR_2, KMX62_RATE_REG, I2C_MEMADD_SIZE_8BIT, &i2c_val, 1, 100);
	HAL_Delay(5);
#endif

	//Enable magnetometer channels
	//i2c_val = KMX62_CTRL_VAL;
	//hal_return_val = HAL_I2C_Mem_Write(&hi2c2, KMX62_ADDR, KMX62_CTRL_REG_2, I2C_MEMADD_SIZE_8BIT, &i2c_val, 1, 100);
	i2c_val = KMX62_CTRL_VAL;
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR, KMX62_CTRL_REG_2, I2C_MEMADD_SIZE_8BIT, &i2c_val, 1, 100);
	HAL_Delay(5);
#ifdef USE_CHAINED_MAG
	hal_return_val = HAL_I2C_Mem_Write(&hi2c3, KMX62_ADDR_2, KMX62_CTRL_REG_2, I2C_MEMADD_SIZE_8BIT, &i2c_val, 1, 100);
	HAL_Delay(5);
#endif
*/
	//TBD TODO add in HAL return value verification and retries
}

void process_magnetometer()
{
#if defined (USE_LIS2MDL)
    LIS2MDL_Data_t mag;
#elif defined (USE_LIS3MDL)
    LIS3MDL_Data_t mag;
#endif

    static uint8_t magReadFailureCount = 0;

#if defined (USE_LIS2MDL)
    if (LIS2MDL_ReadMagnetometers(&hi2c3, &mag) == HAL_OK)
#elif defined (USE_LIS3MDL)
    if (LIS3MDL_ReadMagnetometers(&hi2c3, &mag) == HAL_OK)
#endif
    {
        // Use mag.x, mag.y, mag.z
    	magnetometer_rx_0[0] = mag.x0;
    	magnetometer_rx_0[1] = mag.y0;
    	magnetometer_rx_0[2] = mag.z0;

    	magnetometer_rx_1[0] = mag.x1;
    	magnetometer_rx_1[1] = mag.y1;
    	magnetometer_rx_1[2] = mag.z1;

    	magReadFailureCount = 0;
    }
    else
    {
    	 // Reset mag.x, mag.y, mag.z
    	 magnetometer_rx_0[0] = 0;
    	 magnetometer_rx_0[1] = 0;
    	 magnetometer_rx_0[2] = 0;

    	 magnetometer_rx_1[0] = 0;
    	 magnetometer_rx_1[1] = 0;
    	 magnetometer_rx_1[2] = 0;

    	 magReadFailureCount ++;
		 if (magReadFailureCount > 5) {
			 if (I2C3_BusRecovery() != HAL_OK) {
			 	Error_Handler();
			 }
			 magReadFailureCount = 0;
		 }
    }

	report_magnetometer_data(magnetometer_rx_0, 1);		//Commented out to use for Collimator? Investigate why.
	report_magnetometer_data(magnetometer_rx_1, 2);

	//Reset buffer and request RX
	memset(magnetometer_rx_0, 0, NUM_MAG_FIELDS*sizeof(int16_t));
	memset(magnetometer_rx_1, 0, NUM_MAG_FIELDS*sizeof(int16_t));

	/*
//Enable if using daisy chained magnetometers
#ifdef USE_CHAINED_MAG
	if(read_chained_mag)
	{
		read_chained_mag = false;
		chained_mag_complete = true;
		HAL_I2C_Mem_Read_IT(&hi2c3, KMX62_ADDR_2, KMX62_MAG_REG, I2C_MEMADD_SIZE_8BIT, (uint8_t *)magnetometer_rx_2, NUM_MAG_FIELDS*sizeof(int16_t));
	}
#endif
	//Wait until next magnetometer read is ready
	if(read_mag_ms >= 0)
	{
		return;
	}
	//Reset timer for next magnetometer read
#ifdef MAG_CAL
	read_mag_ms += 10;
#else
	read_mag_ms += 100;
#endif
	//HAL_GPIO_TogglePin(IO_LED_BLUE_GPIO_Port, IO_LED_BLUE_Pin);

	//Check for missing comms
	if(mag_rx_missing++ > MISSING_MAG_RX_COUNT)
	{
		//TBD TODO fault handling
	}

	//Report recorded data
	report_magnetometer_data(magnetometer_rx_0, 0);
	report_magnetometer_data(magnetometer_rx_1, 1);
	report_magnetometer_data(magnetometer_rx_2, 2);

	//Reset buffer and request RX
	memset(magnetometer_rx_0, 0, NUM_MAG_FIELDS*sizeof(int16_t));
	memset(magnetometer_rx_1, 0, NUM_MAG_FIELDS*sizeof(int16_t));
	memset(magnetometer_rx_2, 0, NUM_MAG_FIELDS*sizeof(int16_t));

	//HAL_I2C_Mem_Read_IT(&hi2c2, KMX62_ADDR, KMX62_MAG_REG, I2C_MEMADD_SIZE_8BIT, (uint8_t *)magnetometer_rx_0, NUM_MAG_FIELDS*sizeof(int16_t));
	HAL_I2C_Mem_Read_IT(&hi2c3, KMX62_ADDR, KMX62_MAG_REG, I2C_MEMADD_SIZE_8BIT, (uint8_t *)magnetometer_rx_1, NUM_MAG_FIELDS*sizeof(int16_t));
*/
}


void mag_i2c_rx_cb(int bus)
{
	if(bus == 3)
	{
		mag_rx_missing = 0;
		if(!chained_mag_complete)
		{
			read_chained_mag = true;
		}
		else
		{
			chained_mag_complete = false;
		}
	}
}

/**
 * @brief Recover a potentially stuck I2C3 bus by manually toggling SCL/SDA.
 *
 * This routine performs a standard I2C bus recovery sequence when a slave
 * device holds the SDA line low and prevents normal bus operation.
 *
 * Recovery procedure:
 *   - De-initialize the I2C3 peripheral.
 *   - Reconfigure SCL (PA8) and SDA (PC9) as GPIO open-drain outputs.
 *   - Release both lines and check the SDA state.
 *   - If SDA is held low, generate up to 9 SCL clock pulses to allow
 *     the slave to complete any unfinished transfer and release the bus.
 *   - Generate an I2C STOP condition (SDA low -> SCL high -> SDA high).
 *   - Restore the pins to their I2C alternate-function configuration.
 *   - Re-initialize the I2C3 peripheral.
 *
 * @return HAL_OK    Bus successfully recovered and SDA released.
 * @return HAL_ERROR I2C re-initialization failed or SDA remains stuck low.
 *
 * @note This function should be called after an I2C timeout, bus error,
 *       or any condition indicating that the bus may be locked by a slave.
 */
static HAL_StatusTypeDef I2C3_BusRecovery(void)
{
    GPIO_InitTypeDef GPIO_InitStruct = {0};
    uint8_t i;

    /* Disable I2C peripheral */
    HAL_I2C_DeInit(&hi2c3);

    __HAL_RCC_GPIOA_CLK_ENABLE();
    __HAL_RCC_GPIOC_CLK_ENABLE();

    /* PA8 = SCL */
    GPIO_InitStruct.Pin = GPIO_PIN_8;
    GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_OD;
    GPIO_InitStruct.Pull = GPIO_PULLUP;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

    /* PC9 = SDA */
    GPIO_InitStruct.Pin = GPIO_PIN_9;
    HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);

    /* Release lines */
    HAL_GPIO_WritePin(GPIOA, GPIO_PIN_8, GPIO_PIN_SET);
    HAL_GPIO_WritePin(GPIOC, GPIO_PIN_9, GPIO_PIN_SET);

    HAL_Delay(1);

    /* SDA already released? */
    if (HAL_GPIO_ReadPin(GPIOC, GPIO_PIN_9) == GPIO_PIN_RESET)
    {
        /* Clock SCL up to 9 times */
        for (i = 0; i < 9; i++)
        {
            HAL_GPIO_WritePin(GPIOA, GPIO_PIN_8, GPIO_PIN_SET);
            HAL_Delay(1);

            HAL_GPIO_WritePin(GPIOA, GPIO_PIN_8, GPIO_PIN_RESET);
            HAL_Delay(1);

            if (HAL_GPIO_ReadPin(GPIOC, GPIO_PIN_9) == GPIO_PIN_SET)
                break;
        }
    }

    /* Generate STOP:
       SDA low -> SCL high -> SDA high */
    HAL_GPIO_WritePin(GPIOC, GPIO_PIN_9, GPIO_PIN_RESET);
    HAL_Delay(1);

    HAL_GPIO_WritePin(GPIOA, GPIO_PIN_8, GPIO_PIN_SET);
    HAL_Delay(1);

    HAL_GPIO_WritePin(GPIOC, GPIO_PIN_9, GPIO_PIN_SET);
    HAL_Delay(1);

    /* Restore I2C pins */

    GPIO_InitStruct.Pin = GPIO_PIN_8;
    GPIO_InitStruct.Mode = GPIO_MODE_AF_OD;
    GPIO_InitStruct.Pull = GPIO_PULLUP;
    GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_VERY_HIGH;
    GPIO_InitStruct.Alternate = GPIO_AF4_I2C3;
    HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

    GPIO_InitStruct.Pin = GPIO_PIN_9;
    HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);

    /* Re-init peripheral */
    if (HAL_I2C_Init(&hi2c3) != HAL_OK)
        return HAL_ERROR;

    return (HAL_GPIO_ReadPin(GPIOC, GPIO_PIN_9) == GPIO_PIN_SET)
           ? HAL_OK
           : HAL_ERROR;
}


