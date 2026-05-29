

#ifndef LIS3MDL_H
#define LIS3MDL_H

#include "stm32f4xx_hal.h"

// LIS3MDL I2C Address (SA1 = 0 or 1; default is 0x1E << 1)
#define LIS3MDL_I2C_ADDR0       0x1C << 1		//FRONT Magnetometer (U15)
#define LIS3MDL_I2C_ADDR1       0x1E << 1		//BACK Magnetometer	 (U14)

// Register addresses
#define LIS3MDL_WHO_AM_I        0x0F
#define LIS3MDL_CTRL_REG1       0x20
#define LIS3MDL_CTRL_REG2       0x21
#define LIS3MDL_CTRL_REG3       0x22
#define LIS3MDL_OUT_X_L         0x28

// WHO_AM_I expected value
#define LIS3MDL_WHO_AM_I_RESP   0x3D

typedef struct {
    int16_t x0;
    int16_t y0;
    int16_t z0;
    int16_t x1;
	int16_t y1;
	int16_t z1;
} LIS3MDL_Data_t;

// Public API
HAL_StatusTypeDef LIS3MDL_Init(I2C_HandleTypeDef *hi2c3);
HAL_StatusTypeDef LIS3MDL_ReadMagnetometers(I2C_HandleTypeDef *hi2c3, LIS3MDL_Data_t *data);
uint8_t LIS3MDL_WhoAmI(I2C_HandleTypeDef *hi2c3, uint16_t address);

#endif
