/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: System parameters
*	Author: Carlton Chow
*	Description:
*/


#ifndef SYSTEM_PARAMETERS_H_
#define SYSTEM_PARAMETERS_H_

#if defined(CALIBRATION_MODE)
#define MRSRC 0
#define GBSRC 1

#if (MRSRC + GBSRC) != 1
    #error "Exactly one of MRSRC or GBSRC must be set to 1"
#endif

#define MAX_OPERATIONAL_POINTS	36

#define MAX_OP_KV				100
#define MAX_OP_F_COIL			3000
#define MAX_OP_DEFL_COIL		3000
#define MIN_OP_DEFL_COIL		-3000

#if MRSRC
#define MAX_OP_MA				1
#define MAX_OP_TIME				900
#define MAX_OP_HEATER			4000
#define MAX_CONDITION_CURRENT	4000
#define MAX_WARMUP_CURRENT		4000
#elif GBSRC
#define MAX_OP_MA				6
#define MAX_OP_TIME				180
#define MAX_OP_HEATER			3250
#define MAX_CONDITION_CURRENT	3250
#define MAX_WARMUP_CURRENT		3250
#endif

#define MAX_REPELLER_CURRENT_VAL	0.5
#define REPELLER_TARGET				150

#define FW_MAJOR_VERSION	1
#define FW_MINOR_VERSION	0
#define FW_LEVEL_VERSION	0

#include "pc_comm_parser.h"
#include <stdbool.h>

typedef union variableValue
{
	uint32_t u;
	int32_t i;
	float f;
} VariableValue;

enum systemStatusFields
{
	SS_MODE = 0,
	SS_STATE,
	SS_OP_IDX, //SS_OPERATIONAL_POINT
	SS_OP_COUNT,
	SS_I_TIMER_STATE,
	SS_TIM_1_STATE,
	SS_TIM_2_STATE,
	SS_SYS_RUNTIME,
	SS_HVPS_RUNTIME,
	SS_WU_HOLD_TIME,
	SS_RESERVED_1,
	SS_BUTTONS,
	SS_FAULTS,
	SS_COMM_FAULTS,
	SS_INTERLOCKS,
	SS_HVPS_IO, //SS_HVPS_IO_STATUS
	SS_HVPS_FLAG_STATUS,
	SS_HVPS_ERR_STATUS,
	SS_INTERNAL_TIMER_VAL, //SS_INTERNAL_TIMER
	SS_TIMER_1_VAL, //SS_TIM_1_TIMER
	SS_TIMER_2_VAL, //SS_TIM_2_TIMER
	SS_KV_FB, //SS_KV
	SS_MA_FB, //SS_EMISSION_CURRENT
	SS_GRID_FB, //SS_GRID_VOLTAGE,
	SS_HEATER_FB, //SS_HEATER_CURRENT
	SS_HEATER_SP, //SS_HEATER_OUTPUT
	SS_RESERVED_3,
	SS_RESERVED_4,
	SS_X_COIL_VOLTAGE,
	SS_X_COIL_CURRENT,
	SS_Y_COIL_VOLTAGE,
	SS_Y_COIL_CURRENT,
	SS_F_COIL_VOLTAGE,
	SS_F_COIL_CURRENT,
	SS_COIL_TEMP,
	SS_IONPUMP_PRESSURE,
	SS_ION_REP_VOL, //SS_REPELLER_VOLTAGE
	SS_ION_REP_CUR, //SS_REPELLER_CURRENT
	SS_WATER_PRESSURE,
	SS_WATER_FLOW_RATE,
	SS_WATER_TEMP,
	SS_HEATSINK_TEMP,
	SS_PELTIER_TEMP,
	SS_CABINET_TEMP,
	SS_3P3V, //SS_3V3_SUPPLY
	SS_5V, //SS_5V_SUPPLY
	SS_12V, //SS_12V_SUPPLY
	SS_COUNT
};

#else

#define MAX_OPERATIONAL_POINTS	5

#define MAX_OP_KV				100
#define MAX_OP_F_COIL			3000
#define MAX_OP_DEFL_COIL		3000
#define MIN_OP_DEFL_COIL		-3000
#define MAX_OP_TIME				180

#define MAX_OP_HEATER			3250
#define MAX_CONDITION_CURRENT	3250
#define MAX_WARMUP_CURRENT		3250

#define MAX_OP_MA				8

#define MAX_REPELLER_CURRENT_VAL	0.5
#define REPELLER_TARGET				150

#define QC_DATA_REQ_COUNT	2	
#define QC_DATA_RES_COUNT	5

#define FW_MAJOR_VERSION	02
#define FW_MINOR_VERSION	00
#define FW_LEVEL_VERSION	01

#include "qc_well.h"
#include "pc_comm_parser.h"
#include <stdbool.h>

typedef union variableValue
{
	uint32_t u;
	int32_t i;
	float f;
} VariableValue;

enum systemStatusFields
{
	SS_STATE = 0,
	SS_SYS_RUNTIME,
	SS_FAULTS,
	SS_INTERLOCKS,
	SS_LED_RING_STATE,
	SS_LED_BASE_STATE,
	SS_COLLIMATOR_LOW,
	SS_COLLIMATOR_HIGH,
	SS_BUTTONS,
	SS_OP_IDX,
	SS_OP_COUNT,
	SS_I_TIMER_STATE,
	SS_INTERNAL_TIMER_VAL,
	SS_TIM_1_STATE,
	SS_TIMER_1_VAL,
	SS_TIM_2_STATE,
	SS_TIMER_2_VAL,		
	SS_HVPS_RUNTIME,
	SS_HVPS_IO,	
	SS_HVPS_FLAG_STATUS,
	SS_KV_FB,
	SS_MA_FB,
	SS_HEATER_SP,
	SS_HEATER_FB,
	SS_GRID_SP,
	SS_GRID_FB,
	SS_X_COIL_CURRENT,
	SS_Y_COIL_CURRENT,
	SS_F_COIL_CURRENT,
	SS_IONPUMP_PRESSURE,
	SS_WATER_PRESSURE,
	SS_WATER_FLOW_RATE,
	SS_WATER_TEMP,
	SS_HEATSINK_TEMP,
	SS_PELTIER_TEMP,
	SS_CABINET_TEMP,
	SS_MAG_X,
	SS_MAG_Y,
	SS_MAG_Z,
	SS_MAG_X2,
	SS_MAG_Y2,
	SS_MAG_Z2,
	SS_TVM_INTERLOCK,	
	SS_KV_SP,
	SS_MA_LIM_SP,
	SS_PWR_SP,
	SS_COUNT
};
#endif

enum treatmentPointsParams
{
	OP_POINT_IDX = 0,
	OP_TOTAL_TIME,
	OP_REMAIN_TIME,
	OP_KV,
	OP_MA,
	OP_FIL,
	OP_X_COIL,
	OP_Y_COIL,
	OP_F_COIL,
	OP_AUTO_EXEC,
	OP_PARAM_COUNT
};

enum hvpsConfig
{
	HVPS_CONF_WARMUP_I = 0,
	HVPS_CONF_CONDITION_I,
	HVPS_CONF_PWR_SETPOINT,
	HVPS_CONF_COUNT
};

enum interlockBP
{
	IBP_DOOR_CLOSED = 0,
	IBP_DRIVE_SYS,
	IBP_BASE_ESTOP,
	IBP_REMOTE_ESTOP,
	IBP_KUKA_FAULT_1,		//4
	IBP_KUKA_FAULT_2,
	IBP_WATER_LEVEL,
	IBP_ION_PUMP_ON,
	IBP_TIMER_FAULT_1,		//8	
	IBP_TIMER_FAULT_2,
	IBP_HVPS_FAULT,
	IBP_COOLER_FAULT,
	IBP_HEADBOARD_FAULT,	//12	
	IBP_WD_FAULT,
	IBP_MCU_FAULT,
	IBP_SPARE_INTERLOCK,
	IBP_BUF_MASTER_FAULT,	//16
	IBP_NA,
	IBP_REMOTE_KEY,
	IBP_COLLIMATOR_ON
};

enum planInfo
{
	PLAN_STAGED_BOOL = 0,
	PLAN_TARGET_BITS_1,
	PLAN_TARGET_BITS_2,
	PLAN_LOADING_FLAGS_1,
	PLAN_LOADING_FLAGS_2,
	PLAN_CONFIRMATION_FLAGS_1,
	PLAN_CONFIRMATION_FLAGS_2,
	NUM_PLAN_INFO
};

enum internalVoltages
{
	INTERNAL_V_3_3,
	INTERNAL_V_5,
	INTERNAL_V_12,
	INTERNAL_V_ION_REP,
	INTERNAL_V_ION_REP_CUR,
	INTERNAL_V_COUNT
};

extern uint32_t device_information[VERSION_RES_COUNT];
extern VariableValue system_status[SS_COUNT];
extern float hvps_config[HVPS_CONF_COUNT];
extern VariableValue operational_points[MAX_OPERATIONAL_POINTS][OP_PARAM_COUNT];
extern uint32_t plan_info[NUM_PLAN_INFO];

#if !defined(CALIBRATION_MODE)
extern VariableValue qc_data[MAX_OPERATIONAL_POINTS][QC_DATA_RES_COUNT];

extern VariableValue qc_reported[QC_DATA_RES_COUNT];

extern VariableValue qc_ping_buf[QC_DATA_RES_COUNT];
extern VariableValue qc_reading_buf[QC_DATA_RES_COUNT];

extern uint32_t qc_samples;
#endif

extern float internal_voltages[INTERNAL_V_COUNT];

void init_system_parameters();

#if !defined(CALIBRATION_MODE)
void init_qc_ping_buf();
void reset_qc_reading();
void reset_qc_reading_buf();
void report_qc_reading();
#endif

void clear_treatment_plan();
void set_plan_flags();

#if !defined(CALIBRATION_MODE)
void report_hb_data(uint32_t hb_idx, float data);
void report_qc_well_data(int16_t *qc_data);
#endif
void report_ext_timer_values(uint32_t state, uint32_t ticks, bool primary);
void report_peltier_temp(float temperature);
void report_hvps_data(VariableValue *hvps_data);

bool verify_keys_ok();
#if !defined(CALIBRATION_MODE)
bool verify_tvm_ok();
#endif
bool verify_collimator_ok();
bool verify_door_ok();
bool verify_estops_ok();
bool verify_drive_ok();

void report_ext_adc_f_coil_v(float voltage);
void report_ext_adc_x_coil_v(float voltage);
void report_ext_adc_y_coil_v(float voltage);
void report_ext_adc_f_coil_cur(float voltage);
void report_ext_adc_x_coil_cur(float voltage);
void report_ext_adc_y_coil_cur(float voltage);
void report_ext_adc_ion_pump(float voltage);
void report_ext_adc_ion_rep_cur(float voltage);
void report_ext_adc_ion_rep_v(float voltage);
void report_ext_adc_cab_temp(float voltage);
void report_ext_adc_hs_temp(float voltage);

void report_ext_adc_3p3_v(float voltage);
void report_ext_adc_5_v(float voltage);
void report_ext_adc_12_v(float voltage);

#endif /* SYSTEM_PARAMETERS_H_ */