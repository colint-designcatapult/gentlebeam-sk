#include <stdbool.h>
#include <string.h>

#include "stm32f4xx_hal.h"
#include "main.h"

#include "sys_data.h"
#include "magnetometer.h"
#include "control_comm.h"
#include "flow.h"
#if !defined(CALIBRATION_MODE)
#include "buttons.h"
#include "qc.h"
#endif

uint32_t raw_sys_pressure_data;
float sys_data_pressure;
float sys_data_flow_rate;
uint32_t raw_sys_temp_data;
float sys_data_temperature;
float sys_data_cap_sense;
uint32_t sys_data_io = 0;
uint8_t sys_data_button_down = 0;
uint8_t sys_data_button_up = 0;
float sys_data_magnetometer[2][NUM_MAG_AXIS];
uint32_t mag_calibr_data[2][NUM_MAG_AXIS];

uint32_t col_low = 0;
uint32_t col_high = 0;

int calibration_counter = -1;

//Thermistor table, each entry representing values 1C apart
float thermistor_table[] = {
	0.70140, 0.72923, 0.75761, 0.78653, 0.81596, 0.84588, 0.87627, 0.90711, 0.93834, 0.96995,
	1.00191, 1.03420, 1.06678, 1.09957, 1.13262, 1.16580, 1.19917, 1.23261, 1.26614, 1.29971,
	1.33329, 1.36683, 1.40030, 1.43367, 1.46694, 1.50000, 1.53291, 1.56554, 1.59795, 1.63008,
	1.66187, 1.69338, 1.72447, 1.75528, 1.78557, 1.81554, 1.84509, 1.87414, 1.90275, 1.93083,
	1.95857, 1.98570, 2.01234, 2.03851, 2.06413, 2.08923, 2.11377, 2.13787, 2.16138, 2.18447,
	2.20686, 2.22883, 2.25034, 2.27124, 2.29159, 2.31160, 2.33100, 2.34987, 2.36830, 2.38626,
	2.40372, 2.42066, 2.43718, 2.45325, 2.46886, 2.48413, 2.49889, 2.51326, 2.52724, 2.54079,
	2.55392, 2.56674, 2.57909, 2.59112, 2.60296, 2.61430, 2.62528, 2.63590, 2.64628, 2.65643,
	2.66619, 2.67570, 2.68480, 2.69380, 2.70238, 2.71068, 2.71887, 2.72678, 2.73440, 2.74190,
	2.74910, 2.75600, 2.76277, 2.76923, 2.77572, 2.78190, 2.78776, 2.79364, 2.79920, 2.80461
};

//Values for magnetometer calibration
uint32_t mag_sum_idx[3][NUM_MAG_AXIS];
uint32_t max_mag_idx = 100;
int32_t mag_sum_val[3][NUM_MAG_AXIS];
int32_t mag_slice_sum[3][NUM_MAG_AXIS];
int32_t mag_slice_sum_old[3][NUM_MAG_AXIS];
int64_t mag_sq_val[3][NUM_MAG_AXIS];
int64_t mag_slice_sq[3][NUM_MAG_AXIS];
int64_t mag_slice_sq_old[3][NUM_MAG_AXIS];
int32_t mag_sum_array[3][NUM_MAG_AXIS][MAX_CAL_SAMPLES];
int64_t mag_sq_array[3][NUM_MAG_AXIS][MAX_CAL_SAMPLES];


#if !defined(CALIBRATION_MODE)
//Save 64-bit value of collimator into two 32-bit values
void report_collimator(uint8_t *col_bytes)
{
	uint32_t col_data = 0;

	memcpy(&col_data, col_bytes, sizeof(uint32_t));
	col_high = col_data;

	memcpy(&col_data, (col_bytes+4), sizeof(uint32_t));
	col_low = col_data;
}

/*
//Save current LED sequence for reporting
void report_led_sequence(int idx)
{
	uint8_t led_idx = (uint8_t)idx;
	sys_data_io &= ~(0xFF);
	sys_data_io |= led_idx;
}
*/

//Save button state for reporting
void report_button_toggle(int idx, GPIO_PinState status)
{
	//TBD TODO
	if(idx <= NUM_BUTTONS && status == GPIO_PIN_RESET)
	{
		uint32_t button_bit = (1 << (8+idx));
		sys_data_io ^= button_bit;
	}
}
#endif

//For magnetometer calibration
void update_mag_cal_window(int32_t samples)
{
	if(samples <= 0 || samples >= MAX_CAL_SAMPLES || max_mag_idx == samples)
	{
		return;
	}
	//Clear all arrays
	for(int mag_idx = 0; mag_idx < 3; mag_idx++)
	{
		for(int axis = 0; axis < 3; axis++)
		{
			for(int i = 0; i < MAX_CAL_SAMPLES; i++)
			{
				mag_sum_array[mag_idx][axis][i] = 0;
				mag_sq_array[mag_idx][axis][i] = 0;
			}
			mag_sum_val[mag_idx][axis] = 0;
			mag_sq_val[mag_idx][axis] = 0;
			mag_slice_sum[mag_idx][axis] = 0;
			mag_slice_sum_old[mag_idx][axis] = 0;
			mag_slice_sq[mag_idx][axis] = 0;
			mag_slice_sq_old[mag_idx][axis] = 0;
			mag_sum_idx[mag_idx][axis] = 0;
		}
	}
	//Set new window size
	max_mag_idx = samples;
}

void report_magnetometer_data(int16_t *raw_mag_data, int mag_idx)
{
//For magnetometer calibration
#if defined(MAG_CAL) || defined(CALIBRATION_MODE)
	int32_t mag_data = 0;
	int64_t mag_sq = 0;
	uint8_t array_idx = 0;
	for(int axis = 0; axis < NUM_MAG_AXIS; axis++)
	{
		array_idx = mag_sum_idx[mag_idx][axis];
		mag_data = (int32_t)raw_mag_data[axis];
		mag_sq = (int64_t)mag_data * (int64_t)mag_data;

		//Subtract oldest value from sum
		mag_sum_val[mag_idx][axis] -= mag_sum_array[mag_idx][axis][array_idx];
		mag_sq_val[mag_idx][axis] -= mag_sq_array[mag_idx][axis][array_idx];

		//Add new value
		mag_sum_val[mag_idx][axis] += mag_data;
		mag_sq_val[mag_idx][axis] += mag_sq;
		mag_slice_sum[mag_idx][axis] += mag_data;
		mag_slice_sq[mag_idx][axis] += mag_sq;

		//Save new value
		mag_sum_array[mag_idx][axis][array_idx] = mag_data;
		mag_sq_array[mag_idx][axis][array_idx] = mag_sq;

		//Update sum idx
		mag_sum_idx[mag_idx][axis] += 1;
		if(mag_sum_idx[mag_idx][axis] >= max_mag_idx)
		{
			mag_sum_idx[mag_idx][axis] = 0;
			mag_slice_sum_old[mag_idx][axis] = mag_slice_sum[mag_idx][axis];
			mag_slice_sq_old[mag_idx][axis] = mag_slice_sq[mag_idx][axis];
			mag_slice_sq[mag_idx][axis] = 0;
			 mag_slice_sum[mag_idx][axis] = 0;
			if(mag_idx == 1)
			{
				if(mag_slice_sum_old[mag_idx][axis] == 0)
				{
					HAL_GPIO_TogglePin(IO_LED_AMBER_GPIO_Port, IO_LED_AMBER_Pin);
				}
			}
			else if(mag_idx == 2)
			{
				if(mag_slice_sum_old[mag_idx][axis] == 0)
				{
					HAL_GPIO_TogglePin(IO_LED_BLUE_GPIO_Port, IO_LED_BLUE_Pin);
				}
			}
		}
	}

//For normal operation
#else
#if defined(USE_LIS2MDL)
	if(mag_idx == 1 || mag_idx == 2)
	{
		for(int i = 0; i < NUM_MAG_AXIS; i++)
		{
			//Save magnetometer values for system reporting.
			/* NOTE:	for LIS2MDL, range is ±50 gauss max. Sensitivity is 1.5 mgauss/LSB.
			the raw value must be multiplied by 1.5 for mGauss, then multiply 0.1 for uT.
			*/
			sys_data_magnetometer[mag_idx-1][i] = ((float)raw_mag_data[i]) * 0.15;
		}
	}
#elif defined (USE_LIS3MDL)
	if(mag_idx == 1 || mag_idx == 2)
	{
		for(int i = 0; i < NUM_MAG_AXIS; i++)
		{
			//Save magnetometer values for system reporting.
			/* NOTE:	for ±12 gauss max. the raw value must be divided by 2281 for Gauss, then multiply 100 for uT. */
			sys_data_magnetometer[mag_idx-1][i] = ((float)raw_mag_data[i]) * 0.0438404209; //KMX62: 0.03662109375;
		}
	}
#endif
#endif
}

void report_temperature_data(uint32_t raw_temp_data)
{
	//Only save raw temperature data for now
	raw_sys_temp_data = raw_temp_data;
}

static void update_temperature_conversion()
{
	float temp_voltage = ((float)raw_sys_temp_data * 3)/4095;

	//Check to see if voltage is less than expected
	if(temp_voltage < thermistor_table[0])
	{
		sys_data_temperature = 100;
		return;
	}

	int temp_idx = 1;
	int num_temp_points = sizeof(thermistor_table)/sizeof(float);

	while(temp_idx < num_temp_points)
	{
		if(temp_voltage <= thermistor_table[temp_idx])
		{
			//Get the difference between indicated voltage and array value
			float temp_calc = temp_voltage-thermistor_table[temp_idx-1];

			//Extrapolate decimal percentage from sandwiched array values (aka linear)
			temp_calc /= (thermistor_table[temp_idx] - thermistor_table[temp_idx-1]);

			//Add on "base" temperature value
			temp_calc += (float)(temp_idx-1);
			sys_data_temperature = temp_calc;
			return;
		}
		temp_idx++;
	}
	sys_data_temperature = (float)num_temp_points;
}

void report_flow_data(uint32_t raw_flow_data)
{
	sys_data_flow_rate = (float)raw_flow_data / TICKS_TO_LPM;
}

void report_pressure_data(uint32_t raw_pressure_data)
{
	raw_sys_pressure_data = raw_pressure_data;
}

static void update_pressure_conversion()
{
	//TBD TODO magic numbers
	//Convert system pressure into input voltage (full scale is 5V)
	float pressure_voltage = ((float)raw_sys_pressure_data) / 819;

#ifdef GB_PCBA_REVB
	//0.5-4.5V is 0-15 PSI
	sys_data_pressure = (pressure_voltage * 8.67) - 24.01;
#else
	// 0 PSI is 2275 (2.77V) / 15 PSI is 4095 (5V)
	// 2.77-4.5V is 0-15 PSI
	sys_data_pressure = (pressure_voltage * 3.75) - 1.875;
#endif
}


uint32_t get_sys_data(int field_idx)
{
	uint32_t output_val = 0;
	switch(field_idx)
	{
		case CC_TX_INFO:
			//create an if-statement, if the collimator sees a QC cap and gets a QC request then it's a QC packet
			output_val = 0x88;	//TBD TODO magic number
			break;
		case CC_TX_IO:
			output_val = sys_data_io;
			break;
#if !defined(CALIBRATION_MODE)
		case CC_TX_COL_LOW:
			output_val = col_low;
			break;
		case CC_TX_COL_HIGH:
			output_val = col_high;
			break;
#endif
		case CC_TX_PRESSURE:
			update_pressure_conversion();
			memcpy(&output_val, &sys_data_pressure, sizeof(uint32_t));
			break;
		case CC_TX_FLOW:
			memcpy(&output_val, &sys_data_flow_rate, sizeof(uint32_t));
			break;
		case CC_TX_TEMP:
			update_temperature_conversion();
			memcpy(&output_val, &sys_data_temperature, sizeof(uint32_t));
			break;
		case CC_TX_MAG_X_1:
			memcpy(&output_val, &sys_data_magnetometer[0][0], sizeof(uint32_t));
			break;
		case CC_TX_MAG_Y_1:
			memcpy(&output_val, &sys_data_magnetometer[0][1], sizeof(uint32_t));
			break;
		case CC_TX_MAG_Z_1:
			memcpy(&output_val, &sys_data_magnetometer[0][2], sizeof(uint32_t));
			break;
		case CC_TX_MAG_X_2:
			memcpy(&output_val, &sys_data_magnetometer[1][0], sizeof(uint32_t));
			break;
		case CC_TX_MAG_Y_2:
			memcpy(&output_val, &sys_data_magnetometer[1][1], sizeof(uint32_t));
			break;
		case CC_TX_MAG_Z_2:
			memcpy(&output_val, &sys_data_magnetometer[1][2], sizeof(uint32_t));
			break;
#if !defined(CALIBRATION_MODE)
		case CC_TX_QC_VAL:
			//cram the two 16-bit values into the 32-bit array position
			output_val |= ((uint32_t)QC1_data << 16);   // Upper 16 bits
			output_val |= (uint32_t)QC2_data & 0xFFFF;  // Lower 16 bits
#endif
		default:
			break;
	}
	return output_val;
}
