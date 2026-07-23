#include <string.h>
#include <stdbool.h>
#include "stm32f4xx_hal.h"

#include "sys_data.h"
#include "control_comm.h"
#include "leds.h"
#include "checksum.h"
#include "dotstar.h"

volatile int32_t control_comm_ms = 50;

volatile bool control_comm_tx_busy = false;
volatile int control_comm_rx_timeout = 0;
volatile int control_comm_tx_timeout = 0;
volatile bool control_comm_rx_recv = false;
uint32_t control_comm_rx_idx = 0;
uint8_t control_comm_rx_in = 0;
uint8_t control_comm_rx_val = 0;
uint8_t control_comm_rx_buf[CC_RX_NUM];

uint8_t control_tx_out[CC_TX_NUM_FIELDS * CC_FIELD_SIZE];
uint8_t mag_cal_tx_out[NUM_MAG_TX_CAL*sizeof(uint32_t)];

int rx_cal_counter = -1;

bool mag_rolling = true;

static void copy_sys_data(uint32_t field_idx);
static void parse_comm_rx();
static void process_comm_rx_packet();

void init_control_comm()
{
	init_crcccitt_tab();

	//Initialize sync val
	for(int i = 0; i < sizeof(uint32_t); i++)
	{
		control_tx_out[i] = CC_SYNC_VAL;
	}

	//Initialize info
	//TBD TODO

	//Initialize delimiter values
	for(int i = 1; i < CC_TX_NUM_FIELDS; i++)
	{
		control_tx_out[(i*CC_FIELD_SIZE)-1] = CC_DELIM_VAL;
	}

	//Initialize terminator value
	control_tx_out[(CC_TX_NUM_FIELDS * CC_FIELD_SIZE)-1] = CC_TERM_VAL;

	//Initialize RX reception
	control_comm_rx_recv = false;
	HAL_UART_Receive_IT(&huart2, &control_comm_rx_val, sizeof(uint8_t));
}

void process_control_comm()
{
	//If byte has been received, parse it to try and
	if(control_comm_rx_recv)
	{
		control_comm_rx_timeout = 0;
		control_comm_rx_val = control_comm_rx_in;
		control_comm_rx_recv = false;
		HAL_UART_Receive_IT(&huart2, &control_comm_rx_in, 1);
		parse_comm_rx();
	}

	//If we have received an appropriate number of bytes, try to process
	if(control_comm_rx_idx >= CC_RX_NUM)
	{
		process_comm_rx_packet();
		control_comm_rx_idx = 0;
	}

	//Wait until control comm timer has expired to send new output
	if(control_comm_ms >= 0)
	{
		return;
	}

	//Check to make sure TX bus is free
	if(control_comm_tx_busy)
	{
		if(++control_comm_tx_timeout > 5)
		{
			control_comm_tx_timeout = 0;
			control_comm_tx_busy = false;
		}

		//TBD TODO throw error here
		return;
	}

	//If we have RX timeout, reset the interrupt
	if(++control_comm_rx_timeout > 5)
	{
		control_comm_rx_timeout = 0;
		HAL_UART_Receive_IT(&huart2, &control_comm_rx_in, 1);
	}
	control_comm_ms += 100;

//For magnetometer calibration only
#if defined(MAG_CAL) || defined(CALIBRATION_MODE)
	uint32_t *mag_cal_tx_32 = (uint32_t *)mag_cal_tx_out;
	mag_cal_tx_32[MAG_TX_CAL_SYNC] = 0xFFFFFFFF;
	mag_cal_tx_32[MAG_TX_CAL_SYNC_2] = 0xFFFFFFFF;
	mag_cal_tx_32[MAG_TX_CAL_SIZE] = max_mag_idx;

	if(!mag_rolling)
	{
		//Copy sums
		for(int axis = 0; axis < 3; axis++)
		{
			mag_cal_tx_32[MAG_TX_CAL_X1_SUM + axis] = (uint32_t)mag_slice_sum_old[0][axis];
			mag_cal_tx_32[MAG_TX_CAL_X2_SUM + axis] = (uint32_t)mag_slice_sum_old[1][axis];
			mag_cal_tx_32[MAG_TX_CAL_X3_SUM + axis] = (uint32_t)mag_slice_sum_old[2][axis];
		}

		//Copy squares
		memcpy(&mag_cal_tx_32[MAG_TX_CAL_X1_SQ], &mag_slice_sq_old[0][0], 24);
		memcpy(&mag_cal_tx_32[MAG_TX_CAL_X2_SQ], &mag_slice_sq_old[1][0], 24);
		memcpy(&mag_cal_tx_32[MAG_TX_CAL_X3_SQ], &mag_slice_sq_old[2][0], 24);
	}
	else
	{
		//Copy sums
		for(int axis = 0; axis < 3; axis++)
		{
			mag_cal_tx_32[MAG_TX_CAL_X1_SUM + axis] = (uint32_t)mag_sum_val[0][axis];
			mag_cal_tx_32[MAG_TX_CAL_X2_SUM + axis] = (uint32_t)mag_sum_val[1][axis];
			mag_cal_tx_32[MAG_TX_CAL_X3_SUM + axis] = (uint32_t)mag_sum_val[2][axis];
		}

		//Copy squares
		memcpy(&mag_cal_tx_32[MAG_TX_CAL_X1_SQ], &mag_sq_val[0][0], 24);
		memcpy(&mag_cal_tx_32[MAG_TX_CAL_X2_SQ], &mag_sq_val[1][0], 24);
		memcpy(&mag_cal_tx_32[MAG_TX_CAL_X3_SQ], &mag_sq_val[2][0], 24);
	}

#if defined(CALIBRATION_MODE)
	//set WATER PRESSURE, FLOW RATE, TEMP
	mag_cal_tx_32[MAG_TX_CAL_X1_SQ_2] = get_sys_data(CC_TX_PRESSURE);
	mag_cal_tx_32[MAG_TX_CAL_Y1_SQ_2] = get_sys_data(CC_TX_FLOW);
	mag_cal_tx_32[MAG_TX_CAL_Z1_SQ_2] = get_sys_data(CC_TX_TEMP);
#endif

	HAL_UART_Transmit_IT(&huart2, mag_cal_tx_out, NUM_MAG_TX_CAL*sizeof(uint32_t));
	control_comm_tx_busy = true;

//For regular execution
#else

	//Copy data into TX buffer
	for(int i = CC_TX_INFO; i < CC_TX_CRC; i++)
	{
		copy_sys_data(i);
	}

	//Calculate CRC and place in TX buffer
	uint32_t *crc_val = (uint32_t *)(control_tx_out + (CC_FIELD_SIZE * CC_TX_CRC));
	*crc_val = (uint32_t)crc_ccitt_1d0f(control_tx_out, CC_FIELD_SIZE * CC_TX_CRC);

	HAL_UART_Transmit_IT(&huart2, control_tx_out, CC_TX_NUM_FIELDS * CC_FIELD_SIZE);
	//HAL_GPIO_TogglePin(IO_LED_AMBER_GPIO_Port, IO_LED_AMBER_Pin);
	control_comm_tx_busy = true;
#endif
}

static void parse_comm_rx()
{
	//Only start saving bytes if sync val has been received
	if(control_comm_rx_idx < NUM_SYNC_BYTES && control_comm_rx_val != CC_SYNC_VAL)
	{
		control_comm_rx_idx = 0;
	}
	//Save valid bytes
	else if(control_comm_rx_idx < CC_RX_NUM)
	{
		control_comm_rx_buf[control_comm_rx_idx++] = control_comm_rx_val;
	}
	//On buffer overflow, just reset and discard packet
	else
	{
		control_comm_rx_idx = 0;
		//TBD TODO throw error if desired
	}
}

static void process_comm_rx_packet()
{
	//TBD TODO, add in additional byte checks if desired
	if(control_comm_rx_buf[4] == control_comm_rx_buf[5])
	{
//For magnetometer calibration only, determine if window is rolling or not
#if defined(MAG_CAL) || defined(CALIBRATION_MODE)
		uint32_t window_val = (uint32_t)control_comm_rx_buf[4];
		window_val &= 0xFF;
		if(window_val == 252)
		{
			mag_rolling = true;
		}
		else if(window_val == 253)
		{
			mag_rolling = false;
		}
		update_mag_cal_window((int32_t)window_val);
//For regular execution, update LED sequence
#else
//		set_new_led_sequence((int)control_comm_rx_buf[5]);
		process_led_sequence(control_comm_rx_buf[5]);
#endif
	}
}

static void copy_sys_data(uint32_t field_idx)
{
	//Check for valid index
	if(field_idx >= CC_TX_NUM_FIELDS)
	{
		return;
	}
	uint32_t* output_val = (uint32_t *)(control_tx_out + (CC_FIELD_SIZE * field_idx));
	*output_val = get_sys_data(field_idx);
}

void control_comm_rx_cb()
{
	control_comm_rx_recv = true;
	//HAL_GPIO_TogglePin(IO_LED_BLUE_GPIO_Port, IO_LED_BLUE_Pin);
}

void control_comm_tx_cb()
{
	control_comm_tx_busy = false;
}


