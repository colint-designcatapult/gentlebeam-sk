#ifndef SENSUSSRC_SYS_DATA_H_
#define SENSUSSRC_SYS_DATA_H_

#include <stdbool.h>
#include "magnetometer.h"

//#define MAG_CAL 1
#define GB_PCBA_REVB 			1

#define MAX_CAL_SAMPLES 251

#if !defined(CALIBRATION_MODE)
void report_collimator(uint8_t *col_bytes);
void report_led_sequence(int idx);
void report_button_toggle(int idx, GPIO_PinState status);
#endif
void report_flow_data(uint32_t raw_flow_data);
void report_magnetometer_data(int16_t * raw_mag_data, int mag_idx);
void report_calibration_magnetometer_data(uint16_t *raw_mag_data, bool first_mag);
void report_temperature_data(uint32_t raw_temp_data);
void report_pressure_data(uint32_t raw_pressure_data);

void update_mag_cal_window(int32_t samples);

uint32_t get_sys_data(int field_idx);
uint32_t get_cal_data(int field_idx, int sample_idx);

extern int calibration_counter;

extern uint32_t max_mag_idx;
extern int32_t mag_sum_val[3][NUM_MAG_AXIS];
extern int64_t mag_sq_val[3][NUM_MAG_AXIS];
extern int32_t mag_slice_sum_old[3][NUM_MAG_AXIS];
extern int64_t mag_slice_sq_old[3][NUM_MAG_AXIS];


#endif /* SENSUSSRC_SYS_DATA_H_ */
