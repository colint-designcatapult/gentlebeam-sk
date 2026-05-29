/*
 * collimator.c
 *
 *  Created on: Dec 5, 2024
 *      Author: Carlton
 */

#include <string.h>
#include <stdbool.h>
#include "stm32f4xx_hal.h"
#include "main.h"

#include "collimator.h"
#include "timer.h"
#include "sys_data.h"
#include "1wire_ds2482.h"

uint32_t col_state = COL_STATE_IDLE;
uint32_t col_rom_idx = 0;
volatile int32_t collim_ms = 250;
uint8_t col_rom_buf[8] = {0};
volatile uint8_t col_rom_read = 0;
uint8_t col_transceiver_output[2] = {0};

uint8_t collimator_test_val[8] = {0};

static void request_1_wire_reset();
static void read_col_transceiver();
static uint32_t check_1_wire_present();
static void request_1_wire_read();
static void prep_transceiver_read();
static uint32_t save_1_wire_byte();

//NOTE: THIS MODULE IS CURRENTLY NOT DEBUGGED, ONLY INITIAL IMPLEMENTATION COMPLETED
//NOTE: THIS MODULE IS CURRENTLY NOT DEBUGGED, ONLY INITIAL IMPLEMENTATION COMPLETED
//NOTE: THIS MODULE IS CURRENTLY NOT DEBUGGED, ONLY INITIAL IMPLEMENTATION COMPLETED
//NOTE: THIS MODULE IS CURRENTLY NOT DEBUGGED, ONLY INITIAL IMPLEMENTATION COMPLETED

void init_collimator()
{
	//Write transceiver configuration
	col_transceiver_output[0] = 0xD2;	//TBD TODO magic numbers
	col_transceiver_output[1] = 0xF0;
	HAL_I2C_Master_Transmit_IT(&hi2c2, COL_TRANSC_ADDR, col_transceiver_output, 2);

	//Set time for first collimator read
	collim_ms = 250;	//TBD TODO magic number
}

static void request_1_wire_reset()
{
	//Write reset command to i2c (send 0xB4 to transceiver) to try and find 1-wire device
	col_transceiver_output[0] = 0xB4;
	HAL_I2C_Master_Transmit_IT(&hi2c2, COL_TRANSC_ADDR, col_transceiver_output, 1);
	collim_ms = 10;	//TBD TODO magic number
}

static void read_col_transceiver()
{
	//Request read from the 1-wire transceiver
	col_rom_read = 0;
	HAL_I2C_Master_Receive_IT(&hi2c2, COL_TRANSC_ADDR, &col_rom_read, 1);
	collim_ms = 2;	//TBD TODO magic number
}

static uint32_t check_1_wire_present()
{
	//If 1-wire device found according to status register, request ROM read
	if(col_rom_read & 0x02)	//TBD TODO magic number for PPD status response
	{
		//Tell transceiver to initiate ROM read request
		col_transceiver_output[0] = 0xA5;	//TBD TODO magic numbers
		col_transceiver_output[1] = 0x33;
		HAL_I2C_Master_Transmit_IT(&hi2c2, COL_TRANSC_ADDR, col_transceiver_output, 2);
		collim_ms = 10;	//TBD TODO magic number
		return COL_STATE_ROM_START;
	}
	//If no reset response, indicate no device found and go back to idle
	else
	{
		memset(col_rom_buf, 0, 8);

		report_collimator(col_rom_buf);

		collim_ms = 100;

		return COL_STATE_IDLE;
	}
}

static void request_1_wire_read()
{
	//Write to transceiver to indicate 1-wire read requested
	col_transceiver_output[0] = 0x96;	//TBD TODO magic numbers
	HAL_I2C_Master_Transmit_IT(&hi2c2, COL_TRANSC_ADDR, col_transceiver_output, 1);
	collim_ms = 5;	//TBD TODO magic numbers
}

static void prep_transceiver_read()
{
	//Write to transceiver to set the read data register
	col_transceiver_output[0] = 0xE1;	//TBD TODO magic numbers
	col_transceiver_output[1] = 0xE1;
	HAL_I2C_Master_Transmit_IT(&hi2c2, COL_TRANSC_ADDR, col_transceiver_output, 2);
	collim_ms = 2;	//TBD TODO magic numbers
}

//Read bytes from the transceiver
static uint32_t save_1_wire_byte()
{
	col_rom_buf[col_rom_idx] = col_rom_read;
	col_rom_idx++;
	//If 8 bytes have been received (64-bit ROM), report the value
	if(col_rom_idx >= 8)
	{
		report_collimator(col_rom_buf);

		col_rom_idx = 0;
		collim_ms = 100;

		return COL_STATE_IDLE;
	}
	//Otherwise continue to read
	else
	{
		request_1_wire_read();
		return COL_STATE_ROM_READ_REQ;
	}
}

void process_collimator()
{
	if(collim_ms > 0)
	{
		return;
	}

	collim_ms = 100;
	// read collimator id

	// option 1
	get_col_id(collimator_test_val, 8);

	// option 2
	//for(int i = 0; i < 7; i++){
	//	collimator_test_val[i] = (uint8_t)ROM_NO[i];
	//}

	collimator_test_val[7] = 0;
	report_collimator(collimator_test_val);

	/*

	switch(col_state)
	{
		case COL_STATE_IDLE:
			request_1_wire_reset();
			col_state = COL_STATE_SENDING_RESET;
			break;
		case COL_STATE_SENDING_RESET:
			//Initiate read of status from transceiver to see if 1-wire device found
			read_col_transceiver();
			col_state = COL_STATE_READING_RESET;
			break;
		case COL_STATE_READING_RESET:
			//Check if device detected after 1-wire reset
			col_state = check_1_wire_present();
			break;
		case COL_STATE_ROM_START:
			//Request 1-wire data read
			col_rom_idx = 0;
			request_1_wire_read();
			col_state = COL_STATE_ROM_READ_REQ;
			break;
		case COL_STATE_ROM_READ_REQ:
			//Prepare the transceiver for data read
			prep_transceiver_read();
			col_state = COL_STATE_ROM_READ_SET;
			break;
		case COL_STATE_ROM_READ_SET:
			//Perform read
			read_col_transceiver();
			col_state = COL_STATE_ROM_READ_WAIT;
			break;
		case COL_STATE_ROM_READ_WAIT:
			//Save ROM value
			col_state = save_1_wire_byte();
			break;
		default:
			col_rom_idx = 0;
			collim_ms = 100;
			col_state = COL_STATE_IDLE;
			break;
	}
	*/
}
