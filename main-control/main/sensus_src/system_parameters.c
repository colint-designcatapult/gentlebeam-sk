/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: System parameters
*	Author: Carlton Chow
*	Description:
*/

#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include <math.h>
#include <ff.h>
#include <atmel_start.h>
#include "checksum.h"
#include "custom_eth_ipstack_main.h"
#include "ext_timers.h"
#include "faults.h"
#include "ftdi.h"
#include "head_board.h"
#include "hvps.h"
#include "hvps_monitoring.h"
#include "jsmn.h"
#include "thermistor_tables.h"
#include "state_machine.h"
#include "sys_config_defaults.h"
#include "system_monitoring.h"
#include "system_parameters.h"

//Static system device information
uint32_t device_information[VERSION_RES_COUNT];

//System status for telemetry
VariableValue system_status[SS_COUNT];

//Configuration tables
float hvps_config[HVPS_CONF_COUNT];

float internal_voltages[INTERNAL_V_COUNT];

//Operational points for treatment plan
VariableValue operational_points[MAX_OPERATIONAL_POINTS][OP_PARAM_COUNT];
uint32_t plan_info[NUM_PLAN_INFO];

#if !defined(CALIBRATION_MODE)
//QC data
VariableValue qc_data[MAX_OPERATIONAL_POINTS][QC_DATA_RES_COUNT];

VariableValue qc_reported[QC_DATA_RES_COUNT];

VariableValue qc_ping_buf[QC_DATA_RES_COUNT];
VariableValue qc_reading_buf[QC_DATA_RES_COUNT];
#endif

uint32_t qc_samples = 0;

FATFS fs;
bool sd_mounted = false;

static float get_coil_temp(float voltage);
static float get_cabinet_temp(float voltage);
static float get_heatsink_temp(float voltage);

static void init_sd_card();
static void check_sd_network_config();

//Initialize the default system parameters
void init_system_parameters()
{
	//Set device information
	device_information[VERSION_RES_MAJ] = FW_MAJOR_VERSION;
	device_information[VERSION_RES_MIN] = FW_MINOR_VERSION;
	device_information[VERSION_RES_LVL] = FW_LEVEL_VERSION;
#if defined(CALIBRATION_MODE)
	device_information[VERSION_RES_MODE] = FW_CALIBRATION_MODE;
#else
	device_information[VERSION_RES_MODE] = FW_NORMAL_MODE;
#endif
	device_information[VERSION_RES_CRC] = get_app_crc();
	
	//Clear treatment plan
	clear_treatment_plan();
	
	
#if !defined(CALIBRATION_MODE)
	init_qc_ping_buf();
	reset_qc_reading_buf();
#endif
		
	hvps_config[HVPS_CONF_WARMUP_I] = DEFAULT_WARMUP_I;
	hvps_config[HVPS_CONF_CONDITION_I] = DEFAULT_CONDITION_I;
	hvps_config[HVPS_CONF_PWR_SETPOINT] = DEFAULT_PWR_SP;
	
	
	//Initialize SD card
	//init_sd_card();
	use_default_network_settings();	
}

//Wipe the existing treatment plan
void clear_treatment_plan()
{
	system_status[SS_OP_COUNT].i = 0;
	system_status[SS_OP_IDX].i = 0;
	//Clear plan info table
	memset(plan_info, 0, sizeof(uint32_t) * NUM_PLAN_INFO);
	//Clear operational point table
	memset(operational_points, 0, sizeof(VariableValue) * MAX_OPERATIONAL_POINTS * OP_PARAM_COUNT);
}

//Update flags to indicate how many and which operation points need to be confirmed
void set_plan_flags()
{
	uint32_t op_cnt_mod = 0;
	
	//Set plan target flag bits to check plan loading and confirmation
	if(system_status[SS_OP_COUNT].i < 32)
	{
		plan_info[PLAN_TARGET_BITS_1] = ((uint32_t)(1 << system_status[SS_OP_COUNT].i)) - 1;
	}
	//Have explicit definition for exactly 32 bits in case of compiler issues
	else if(system_status[SS_OP_COUNT].i == 32)
	{
		plan_info[PLAN_TARGET_BITS_1] = 0xFFFFFFFF;
	}
	else
	{
		op_cnt_mod = system_status[SS_OP_COUNT].i - 32;
		plan_info[PLAN_TARGET_BITS_1] = 0xFFFFFFFF;
		plan_info[PLAN_TARGET_BITS_2] = ((uint32_t)(1 << op_cnt_mod)) - 1;
	}
}

bool verify_keys_ok()
{
	if(!gpio_get_pin_level(IO_REMOTE_KEY))
	{
		return false;
	}
	return true;
}


bool verify_collimator_ok()
{
	if(!gpio_get_pin_level(IO_BASE_KEY))
	{
		return false;
	}
	return true;
}

bool verify_door_ok()
{
	if(!gpio_get_pin_level(IO_DOOR_CLOSED))
	{
		return false;
	}
	return true;
}

bool verify_estops_ok()
{
	if(!gpio_get_pin_level(IO_BASE_ESTOPn) || !gpio_get_pin_level(IO_REMOTE_ESTOPn))
	{
		return false;
	}
	return true;
}

bool verify_spare_interlock_2_ok()
{
	if(!gpio_get_pin_level(IO_DRIVE_SYS_LOCKED))
	{
		return false;
	}
	return true;
}

//Save values of the external timers
void report_ext_timer_values(uint32_t state, uint32_t ticks, bool primary)
{
	float seconds = 0;
	if(ticks >= 0xFF000000)
	{
		seconds = 0;
	}
	else
	{
		seconds = (float)(ext_timer_tick_start - ticks);
		seconds /= TICKS_PER_SECOND;
	}
	
	//Save reported values
	if(primary)
	{
		system_status[SS_TIM_1_STATE].i = state;
		system_status[SS_TIMER_1_VAL].f = seconds;
	}
	else
	{
		system_status[SS_TIM_2_STATE].i = state;
		system_status[SS_TIMER_2_VAL].f = seconds;
	}
}

void report_ext_adc_f_coil_v(float voltage)
{
	float scaling_factor = 4.1;
	float offset_factor = 0;
	
	float converted = (voltage * scaling_factor) - offset_factor;
	
	//TBD TODO save later if desired
}

void report_ext_adc_x_coil_v(float voltage)
{
	float scaling_factor = 0.4;
	float offset_factor = 2.5;
	
	float converted = (voltage * scaling_factor) - offset_factor;
	
	//TBD TODO save later if desired
}

void report_ext_adc_y_coil_v(float voltage)
{
	float scaling_factor = 0.4;
	float offset_factor = 2.5;
	
	float converted = (voltage * scaling_factor) - offset_factor;
	
	//TBD TODO save later if desired
}

void report_ext_adc_f_coil_cur(float voltage)
{
	float offset_factor = 0;	
#if defined(CALIBRATION_MODE)
	float scaling_factor = 0.6;
#else
	float scaling_factor = 600;
#endif
	
	system_status[SS_F_COIL_CURRENT].f = (voltage * scaling_factor) - offset_factor;
}

void report_ext_adc_x_coil_cur(float voltage)
{
#if defined(CALIBRATION_MODE)
	float scaling_factor = 0.6;
	float offset_factor = 1.5;
#else
	float scaling_factor = 600;
	float offset_factor = 1500;
#endif

	system_status[SS_X_COIL_CURRENT].f = (voltage * scaling_factor) - offset_factor;
}

void report_ext_adc_y_coil_cur(float voltage)
{
#if defined(CALIBRATION_MODE)
	float scaling_factor = 0.6;
	float offset_factor = 1.5;
#else
	float scaling_factor = 600;
	float offset_factor = 1500;
#endif

	system_status[SS_Y_COIL_CURRENT].f = (voltage * scaling_factor) - offset_factor;
}

void report_ext_adc_ion_pump(float voltage)
{
	float offset_factor = 0;
	float scaling_factor = 2;
	
	system_status[SS_IONPUMP_PRESSURE].f = (voltage * scaling_factor) - offset_factor;
}

void report_ext_adc_ion_rep_cur(float voltage)
{
	float offset_factor = 0;
	float scaling_factor = 1;
	
#if defined(CALIBRATION_MODE)
	float value = (voltage * scaling_factor) - offset_factor;
	
	internal_voltages[INTERNAL_V_ION_REP_CUR] = value;
	system_status[SS_ION_REP_CUR].f = value;
#else
	internal_voltages[INTERNAL_V_ION_REP_CUR] = (voltage * scaling_factor) - offset_factor;
#endif
}

void report_ext_adc_ion_rep_v(float voltage)
{
	float offset_factor = 0;
	float scaling_factor = 101;

#if defined(CALIBRATION_MODE)
	float value = (voltage * scaling_factor) - offset_factor;
	
	internal_voltages[INTERNAL_V_ION_REP] = value;
	system_status[SS_ION_REP_VOL].f = value;
#else
	internal_voltages[INTERNAL_V_ION_REP] = (voltage * scaling_factor) - offset_factor;
#endif
}

void report_ext_adc_3p3_v(float voltage)
{
	float offset_factor = 0;
	float scaling_factor = 1;

#if defined(CALIBRATION_MODE)
	float value = (voltage * scaling_factor) - offset_factor;
	
	internal_voltages[INTERNAL_V_3_3] = value;
	system_status[SS_3P3V].f = value;
#else
	internal_voltages[INTERNAL_V_3_3] = (voltage * scaling_factor) - offset_factor;
#endif
}

void report_ext_adc_5_v(float voltage)
{
	float offset_factor = 0;
	float scaling_factor = 2;

#if defined(CALIBRATION_MODE)
	float value = (voltage * scaling_factor) - offset_factor;
	
	internal_voltages[INTERNAL_V_5] = value;
	system_status[SS_5V].f = value;
#else
	internal_voltages[INTERNAL_V_5] = (voltage * scaling_factor) - offset_factor;
#endif
}

void report_ext_adc_12_v(float voltage)
{
	float offset_factor = 0;
	float scaling_factor = 6;

#if defined(CALIBRATION_MODE)
	float value = (voltage * scaling_factor) - offset_factor;
	
	internal_voltages[INTERNAL_V_12] = value;
	system_status[SS_12V].f = value;
#else
	internal_voltages[INTERNAL_V_12] = (voltage * scaling_factor) - offset_factor;
#endif
}

void report_ext_adc_cab_temp(float voltage)
{
	system_status[SS_CABINET_TEMP].f = get_cabinet_temp(voltage);
}

void report_ext_adc_hs_temp(float voltage)
{
	system_status[SS_HEATSINK_TEMP].f = get_heatsink_temp(voltage);
}


static float get_coil_temp(float voltage)
{
	if(isnan(voltage))
	{
		return 500;	//TBD TODO
	}
	
	//Check to see if voltage is greater than expected
	if(voltage >= coil_therm_table[0])
	{
		return 0;
	}
	
	int temp_idx = 1;
	int num_temp_points = sizeof(coil_therm_table)/sizeof(float);
	
	while(temp_idx < num_temp_points)
	{
		if(voltage >= coil_therm_table[temp_idx])
		{
			//Get the difference between indicated voltage and array value
			float temp_calc = coil_therm_table[temp_idx-1] - voltage;
			//Extrapolate decimal percentage from sandwiched array valu1es (aka linear)
			temp_calc /= (coil_therm_table[temp_idx-1] - coil_therm_table[temp_idx]);
			//Add on "base" temperature value
			temp_calc += (float)(temp_idx-1);
			return temp_calc;
		}
		temp_idx++;
	}
	
	//Return max temperature if voltage is out of bounds
	return 500;	//TBD TODO
}

static float get_cabinet_temp(float voltage)
{
	if(isnan(voltage))
	{
#if defined (CALIBRATION_MODE)
		return (DEFAULT_CAB_TEMP_ERR+1);
#else
		return (DEFAULT_CAB_ERR+1);
#endif
	}
	
	//Check to see if voltage is greater than expected
	if(voltage >= cabinet_therm_table[0])
	{
		return 0;
	}
	
	int temp_idx = 1;
	int num_temp_points = sizeof(cabinet_therm_table)/sizeof(float);
	
	while(temp_idx < num_temp_points)
	{
		if(voltage >= cabinet_therm_table[temp_idx])
		{
			//Get the difference between indicated voltage and array value
			float temp_calc = cabinet_therm_table[temp_idx-1]-voltage;
			//Extrapolate decimal percentage from sandwiched array values (aka linear)
			temp_calc /= (cabinet_therm_table[temp_idx-1] - cabinet_therm_table[temp_idx]);
			//Add on "base" temperature value
			temp_calc += (float)(temp_idx-1);
			temp_calc *= CABINET_TEMP_SCALING;
			return temp_calc;
		}
		temp_idx++;
	}
	
	//Return max temperature if voltage is out of bounds
	return (float)num_temp_points;
}

static float get_heatsink_temp(float voltage)
{
	if(isnan(voltage))
	{
#if defined(CALIBRATION_MODE)
		return (DEFAULT_HS_TEMP_ERR+1);
#else
		return (HEATSINK_ERR_TH+1);
#endif
	}
	
	//Check to see if voltage is greater than expected
	if(voltage >= heatsink_therm_table[0])
	{
		return 0;
	}
	
	int temp_idx = 1;
	int num_temp_points = sizeof(heatsink_therm_table)/sizeof(float);
	
	while(temp_idx < num_temp_points)
	{
		if(voltage >= heatsink_therm_table[temp_idx])
		{
			//Get the difference between indicated voltage and array value
			float temp_calc = heatsink_therm_table[temp_idx-1] - voltage;
			//Extrapolate decimal percentage from sandwiched array valu1es (aka linear)
			temp_calc /= (heatsink_therm_table[temp_idx-1] - heatsink_therm_table[temp_idx]);
			//Add on "base" temperature value
			temp_calc += (float)(temp_idx-1);
			return temp_calc;
		}
		temp_idx++;
	}
	
	//Return max temperature if voltage is out of bounds
	return (float)num_temp_points;
}

#if !defined(CALIBRATION_MODE)
//Save reported values from the head board
void report_hb_data(uint32_t hb_idx, float data)
{
	switch(hb_idx)
	{
		case HB_RX_PRESSURE:
			system_status[SS_WATER_PRESSURE].f = data;
			break;
		case HB_RX_FLOW:
			system_status[SS_WATER_FLOW_RATE].f = data;
			break;
		case HB_RX_TEMP:
			system_status[SS_WATER_TEMP].f = data;
			break;
		case HB_RX_MAG_X_1:
			system_status[SS_MAG_X].f = data;
			break;
		case HB_RX_MAG_Y_1:
			system_status[SS_MAG_Y].f = data;
			break;
		case HB_RX_MAG_Z_1:
			system_status[SS_MAG_Z].f = data;
			break;
		case HB_RX_MAG_X_2:
			system_status[SS_MAG_X2].f = data;
			break;
		case HB_RX_MAG_Y_2:
			system_status[SS_MAG_Y2].f = data;
			break;
		case HB_RX_MAG_Z_2:
			system_status[SS_MAG_Z2].f = data;
			break;
		default:
			break;
	}
}
#endif

void report_peltier_temp(float temperature)
{
	system_status[SS_PELTIER_TEMP].f = temperature;
}

#if !defined(CALIBRATION_MODE)
void report_qc_well_data(int16_t *qc_raw)
{
	if(qc_raw == NULL) return;
	
	//Get current OP index
	int op_idx = system_status[SS_OP_IDX].i;
	
	//TBD TODO get state here, if not in control, report error
	
	//Iterate over reported QC values
	for(int i = 0; i < QC_DATA_COUNT; i++)
	{
		//Save NAN value if an error is reported
		if(qc_raw[i] == QC_ERROR_VALUE)
		{
			qc_data[op_idx][i+1].i = QC_NAN_OUTPUT;
		}
		//Otherwise convert value to voltage
		else
		{
			qc_data[op_idx][i+1].f = ((float)(qc_raw[i])) / QC_VOLTAGE_SCALE;
		}
	}
}

void init_qc_ping_buf()
{
	for(int i = 0; i < QC_DATA_RES_COUNT; i++)
	{
		if(i == 0)
		{
			qc_ping_buf[i].f = 1;
		}
		else
		{
			qc_reading_buf[i].f = 0;			
		}
	}	
}

void reset_qc_reading_buf()
{
	//Clear QC reading buffer table with NaN values
	memset(qc_reading_buf, 0xFF, sizeof(VariableValue) * QC_DATA_RES_COUNT);
	
	//Initialize values
	for(int i = 0; i < QC_DATA_RES_COUNT; i++)
	{
		qc_reading_buf[i].f = 0;
	}
}

void reset_qc_reading()
{
	//Clear QC reading buffer table with NaN values
	memset(qc_reported, 0xFF, sizeof(VariableValue) * QC_DATA_RES_COUNT);
	
	//Initialize values
	for(int i = 0; i < QC_DATA_RES_COUNT; i++)
	{
		qc_reported[i].f = 0;
	}
}

void report_qc_reading()
{
	//Clear QC reading table with NaN values
	memset(qc_reported, 0xFF, sizeof(VariableValue) * QC_DATA_RES_COUNT);
	
	//Initialize values
	for(int i = 0; i < QC_DATA_RES_COUNT; i++)
	{
		qc_reported[i].f = qc_reading_buf[i].f;
	}
}
#endif

void report_hvps_data(VariableValue *hvps_data)
{
	//Save kV
	system_status[SS_KV_FB].f = hvps_data[HVPS_STATUS_KV_FB].f;
	
	//Save mA
	system_status[SS_MA_FB].f = hvps_data[HVPS_STATUS_MA_FB].f;
	
	//Save heater current
	system_status[SS_HEATER_FB].f = hvps_data[HVPS_STATUS_FIL_FB].f;
	system_status[SS_HEATER_SP].f = hvps_data[HVPS_STATUS_FIL_SP].f;
	
	//Save grid set point
#if !defined(CALIBRATION_MODE)
	system_status[SS_GRID_SP].f = hvps_data[HVPS_STATUS_GRID_SP].f;
#endif

	//Save grid feedback
	system_status[SS_GRID_FB].f = hvps_data[HVPS_STATUS_GRID_FB].f;
	
	//Save status bit fields
	system_status[SS_HVPS_FLAG_STATUS].u = hvps_data[HVPS_STATUS_FLAG_BITS].u;
	
	//Save IO bit fields
	system_status[SS_HVPS_IO].u = hvps_data[HVPS_STATUS_IO_BITS].u;
	
	//Save fault list bit fields
	//system_status[SS_HVPS_ERR_STATUS].u = hvps_data[HVPS_STATUS_ERR_BITS].u;
	system_status[SS_HVPS_RUNTIME].u = hvps_data[HVPS_STATUS_RUNTIME].u;
	
#if !defined(CALIBRATION_MODE)	
	system_status[SS_KV_SP].f = hvps_data[HVPS_STATUS_KV_SP].f;
	system_status[SS_MA_LIM_SP].f = hvps_data[HVPS_STATUS_MA_LIM_SP].f;
	system_status[SS_PWR_SP].f = hvps_data[HVPS_STATUS_PWR_SP].f;
#endif
	
	hvps_setpoints[HVPS_SP_PWR] = hvps_data[HVPS_STATUS_PWR_SP].f;
	hvps_setpoints[HVPS_SP_KV] = hvps_data[HVPS_STATUS_KV_SP].f;
	hvps_setpoints[HVPS_SP_MA_LIM] = hvps_data[HVPS_STATUS_MA_LIM_SP].f;
	hvps_setpoints[HVPS_SP_GRID] = hvps_data[HVPS_STATUS_GRID_SP].f;
	hvps_setpoints[HVPS_SP_FIL] = hvps_data[HVPS_STATUS_FIL_SP].f;
}

static void init_sd_card()
{	
	//Try to mount SD card
	FRESULT fres;
	fres = f_mount(&fs, "", 1);
	
	//If mount unsuccessful, do not finish initializing
	if(fres != FR_OK)
	{
		sd_mounted = false;
		return;
	}
	
	sd_mounted = true;
	check_sd_network_config();
}

static void check_sd_network_config()
{
	//Do nothing if no SD card is present
	if(!sd_mounted)
	{
		return;
	}
}
