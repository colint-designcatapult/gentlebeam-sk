#include <string.h>
#include <stdbool.h>
#include "stm32f4xx_hal.h"
#include "main.h"
#include "lis3mdl.h"

#include "magnetometer.h"
#include "timer.h"
#include "sys_data.h"

#define USE_CHAINED_MAG 1


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
    if (LIS3MDL_Init(&hi2c3) == HAL_OK) {
        // Sensor initialized
    }

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
    LIS3MDL_Data_t mag;
    if (LIS3MDL_ReadMagnetometers(&hi2c3, &mag) == HAL_OK) {
        // Use mag.x, mag.y, mag.z
    	magnetometer_rx_0[0] = mag.x0;
    	magnetometer_rx_0[1] = mag.y0;
    	magnetometer_rx_0[2] = mag.z0;

    	magnetometer_rx_1[0] = mag.x1;
    	magnetometer_rx_1[1] = mag.y1;
    	magnetometer_rx_1[2] = mag.z1;

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


