#include "lis3mdl.h"
#include "sys_data.h"


static HAL_StatusTypeDef LIS3MDL_WriteRegister(I2C_HandleTypeDef *hi2c3, uint16_t address, uint8_t reg, uint8_t value) {
	return HAL_I2C_Mem_Write(hi2c3, address, reg, I2C_MEMADD_SIZE_8BIT, &value, 1, 100);
}


static HAL_StatusTypeDef LIS3MDL_ReadRegisters(I2C_HandleTypeDef *hi2c3, uint16_t address, uint8_t reg, uint8_t *data, uint16_t size) {
	return HAL_I2C_Mem_Read(hi2c3, address, reg | 0x80, I2C_MEMADD_SIZE_8BIT, data, size, 100);  // MSB = auto-increment
}


uint8_t LIS3MDL_WhoAmI(I2C_HandleTypeDef *hi2c3, uint16_t address) {
    uint8_t id = 0;
    if (LIS3MDL_ReadRegisters(hi2c3, address, LIS3MDL_WHO_AM_I, &id, 1) == HAL_OK) {
        return id;
    }
    return 0x00;
}


HAL_StatusTypeDef LIS3MDL_Init(I2C_HandleTypeDef *hi2c3) {
	/********* FRONT (U15/ADDR0) MAGNETOMETER REGISTER INIT *********/
	//FRONT magnetometer (U15/ADDR0) Ping
	if (LIS3MDL_WhoAmI(hi2c3, LIS3MDL_I2C_ADDR0) != LIS3MDL_WHO_AM_I_RESP) {
        return HAL_ERROR;
    }

    // Configure CTRL_REG1: Ultra-high performance mode X/Y, 80 Hz ODR
    if (LIS3MDL_WriteRegister(hi2c3, LIS3MDL_I2C_ADDR0, LIS3MDL_CTRL_REG1, 0x70) != HAL_OK)
    	return HAL_ERROR;

    // Configure CTRL_REG2: ±12 gauss/1200uT full scale
    if (LIS3MDL_WriteRegister(hi2c3, LIS3MDL_I2C_ADDR0, LIS3MDL_CTRL_REG2, 0x40) != HAL_OK)
    	return HAL_ERROR;

    // Configure CTRL_REG3: Continuous-conversion mode
    if (LIS3MDL_WriteRegister(hi2c3, LIS3MDL_I2C_ADDR0, LIS3MDL_CTRL_REG3, 0x00) != HAL_OK)
    	return HAL_ERROR;


    /********* BACK (U14/ADDR1) MAGNETOMETER REGISTER INIT *********/
	//BACK magnetometer (U14/ADDR1) Ping
	if (LIS3MDL_WhoAmI(hi2c3, LIS3MDL_I2C_ADDR1) != LIS3MDL_WHO_AM_I_RESP) {
        return HAL_ERROR;
    }

    // Configure CTRL_REG1: Ultra-high performance mode X/Y, 80 Hz ODR
    if (LIS3MDL_WriteRegister(hi2c3, LIS3MDL_I2C_ADDR1, LIS3MDL_CTRL_REG1, 0x70) != HAL_OK)
    	return HAL_ERROR;

    // Configure CTRL_REG2: ±12 gauss/1200uT gauss full scale
    if (LIS3MDL_WriteRegister(hi2c3, LIS3MDL_I2C_ADDR1, LIS3MDL_CTRL_REG2, 0x40) != HAL_OK)
    	return HAL_ERROR;

    // Configure CTRL_REG3: Continuous-conversion mode
    if (LIS3MDL_WriteRegister(hi2c3, LIS3MDL_I2C_ADDR1, LIS3MDL_CTRL_REG3, 0x00) != HAL_OK)
    	return HAL_ERROR;


    return HAL_OK;
}


HAL_StatusTypeDef LIS3MDL_ReadMagnetometers(I2C_HandleTypeDef *hi2c3, LIS3MDL_Data_t *data) {
	/********* FRONT (U15/ADDR0) MAGNETOMETER READ *********/
	uint8_t raw[6];
    if (LIS3MDL_ReadRegisters(hi2c3, LIS3MDL_I2C_ADDR0, LIS3MDL_OUT_X_L, raw, 6) != HAL_OK) {
        return HAL_ERROR;
    }

    data->x0 = (int16_t)(raw[1] << 8 | raw[0]);
    data->y0 = (int16_t)(raw[3] << 8 | raw[2]);
    data->z0 = (int16_t)(raw[5] << 8 | raw[4]);

    /********* BACK (U14/ADDR1) MAGNETOMETER READ *********/
    uint8_t raw2[6];
    if (LIS3MDL_ReadRegisters(hi2c3, LIS3MDL_I2C_ADDR1, LIS3MDL_OUT_X_L, raw2, 6) != HAL_OK) {
    	return HAL_ERROR;
    }
    data->x1 = (int16_t)(raw2[1] << 8 | raw2[0]);
	data->y1 = (int16_t)(raw2[3] << 8 | raw2[2]);
	data->z1 = (int16_t)(raw2[5] << 8 | raw2[4]);

    return HAL_OK;
}
