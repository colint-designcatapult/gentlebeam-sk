#include "lis2mdl.h"
#include "sys_data.h"


static HAL_StatusTypeDef LIS2MDL_WriteRegister(I2C_HandleTypeDef *hi2c3, uint16_t address, uint8_t reg, uint8_t value) {
	return HAL_I2C_Mem_Write(hi2c3, address, reg, I2C_MEMADD_SIZE_8BIT, &value, 1, 100);
}


static HAL_StatusTypeDef LIS2MDL_ReadRegisters(I2C_HandleTypeDef *hi2c3, uint16_t address, uint8_t reg, uint8_t *data, uint16_t size) {
	return HAL_I2C_Mem_Read(hi2c3, address, reg , I2C_MEMADD_SIZE_8BIT, data, size, 100);  // MSB = auto-increment
}


uint8_t LIS2MDL_WhoAmI(I2C_HandleTypeDef *hi2c3, uint16_t address) {
    uint8_t id = 0;
    if (LIS2MDL_ReadRegisters(hi2c3, address, LIS2MDL_WHO_AM_I, &id, 1) == HAL_OK) {
        return id;
    }
    return 0x00;
}


HAL_StatusTypeDef LIS2MDL_Init(I2C_HandleTypeDef *hi2c3) {
	/********* FRONT (U15/ADDR0) MAGNETOMETER REGISTER INIT *********/
	//FRONT magnetometer (U15/ADDR0) Ping
	if (LIS2MDL_WhoAmI(hi2c3, LIS2MDL_I2C_ADDR0) != LIS2MDL_WHO_AM_I_RESP) {
        return HAL_ERROR;
    }

	// Configure CFG_REG_A: enable temperature compensation, Mag = 10 Hz (high-resolution and continuous mode)
	if (LIS2MDL_WriteRegister(hi2c3, LIS2MDL_I2C_ADDR0, LIS2MDL_CFG_REG_A, 0x80) != HAL_OK)
		return HAL_ERROR;

	// Configure CFG_REG_C: Enable mag. data-ready interrupt
	if (LIS2MDL_WriteRegister(hi2c3, LIS2MDL_I2C_ADDR0, LIS2MDL_CFG_REG_C, 0x01) != HAL_OK)
			return HAL_ERROR;


    /********* BACK (U14/ADDR1) MAGNETOMETER REGISTER INIT *********/
	//BACK magnetometer (U14/ADDR1) Ping
	if (LIS2MDL_WhoAmI(hi2c3, LIS2MDL_I2C_ADDR1) != LIS2MDL_WHO_AM_I_RESP) {
        return HAL_ERROR;
    }

	// Configure CFG_REG_A: enable temperature compensation, Mag = 10 Hz (high-resolution and continuous mode)
	if (LIS2MDL_WriteRegister(hi2c3, LIS2MDL_I2C_ADDR1, LIS2MDL_CFG_REG_A, 0x80) != HAL_OK)
		return HAL_ERROR;

	// Configure CFG_REG_C: Enable mag. data-ready interrupt
	if (LIS2MDL_WriteRegister(hi2c3, LIS2MDL_I2C_ADDR1, LIS2MDL_CFG_REG_C, 0x01) != HAL_OK)
		return HAL_ERROR;


    return HAL_OK;
}


HAL_StatusTypeDef LIS2MDL_ReadMagnetometers(I2C_HandleTypeDef *hi2c3, LIS2MDL_Data_t *data) {
	/********* FRONT (U15/ADDR0) MAGNETOMETER READ *********/
	uint8_t raw[6];
    if (LIS2MDL_ReadRegisters(hi2c3, LIS2MDL_I2C_ADDR0, LIS2MDL_OUTX_L_REG, raw, 6) != HAL_OK) {
        return HAL_ERROR;
    }

    data->x0 = (int16_t)(raw[1] << 8 | raw[0]);
    data->y0 = (int16_t)(raw[3] << 8 | raw[2]);
    data->z0 = (int16_t)(raw[5] << 8 | raw[4]);

    /********* BACK (U14/ADDR1) MAGNETOMETER READ *********/
    uint8_t raw2[6];
    if (LIS2MDL_ReadRegisters(hi2c3, LIS2MDL_I2C_ADDR1, LIS2MDL_OUTX_L_REG, raw2, 6) != HAL_OK) {
    	return HAL_ERROR;
    }
    data->x1 = (int16_t)(raw2[1] << 8 | raw2[0]);
	data->y1 = (int16_t)(raw2[3] << 8 | raw2[2]);
	data->z1 = (int16_t)(raw2[5] << 8 | raw2[4]);

    return HAL_OK;
}
