#ifndef LIS2MDL_H_
#define LIS2MDL_H_

#include "stm32f4xx_hal.h"

// LIS3MDL I2C Address (SA1 = 0 or 1; default is 0x1E << 1)
#define LIS2MDL_I2C_ADDR0       0x1F << 1		//FRONT Magnetometer (U15)
#define LIS2MDL_I2C_ADDR1       0x1E << 1		//BACK Magnetometer	 (U22)

// Register addresses
#define LIS2MDL_WHO_AM_I        0x4F
#define LIS2MDL_CFG_REG_A       0x60
#define LIS2MDL_CFG_REG_B       0x61
#define LIS2MDL_CFG_REG_C       0x62
#define LIS2MDL_OUTX_L_REG     	0x68
#define LIS2MDL_OUTX_H_REG     	0x69
#define LIS2MDL_OUTY_L_REG     	0x6A
#define LIS2MDL_OUTY_H_REG     	0x6B
#define LIS2MDL_OUTZ_L_REG     	0x6C
#define LIS2MDL_OUTZ_H_REG     	0x6D

// WHO_AM_I expected value
#define LIS2MDL_WHO_AM_I_RESP   0x40

typedef struct {
    int16_t x0;
    int16_t y0;
    int16_t z0;
    int16_t x1;
	int16_t y1;
	int16_t z1;
} LIS2MDL_Data_t;

// Public API
HAL_StatusTypeDef LIS2MDL_Init(I2C_HandleTypeDef *hi2c3);
HAL_StatusTypeDef LIS2MDL_ReadMagnetometers(I2C_HandleTypeDef *hi2c3, LIS2MDL_Data_t *data);
uint8_t LIS2MDL_WhoAmI(I2C_HandleTypeDef *hi2c3, uint16_t address);

#endif /* LIS2MDL_H_ */
