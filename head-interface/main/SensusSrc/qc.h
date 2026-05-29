/*
 * qc.h
 *
 *  Created on: Sep 5, 2025
 *      Author: Steve Holman
 */

#ifndef QC_H_
#define QC_H_

#include "main.h"

// ADC I2C addresses
#define ADC1_ADDRESS      (0x50 << 1)  // Shift for 7-bit addressing	//1010000	Address A0 - Floating/Floating
#define ADC2_ADDRESS      (0x51 << 1)									//1010001	Address A1 - Floating/Ground

#define CONVERSION_REG     0x00         // Pointer to Conversion Result Register

void init_qc(void);
void process_qc(void);
//uint16_t Read_ADC121C021_Conversion(uint8_t devAddr);
HAL_StatusTypeDef Read_ADC121C021_Conversion_IT(uint8_t devAddr);

void I2C_Reset(I2C_HandleTypeDef *hi2c);
void I2C_ForceBusRecovery(I2C_HandleTypeDef *hi2c);


extern uint16_t QC1_data;
extern uint16_t QC2_data;


#endif /* QC_H_ */
