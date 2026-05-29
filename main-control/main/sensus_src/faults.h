/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Faults
*	Author: Carlton Chow
*	Description:
*/


#ifndef FAULTS_H_
#define FAULTS_H_

#include "system_parameters.h"

#if !defined(CALIBRATION_MODE)
//#define COIL_POL_POS_MODE		1
//#define COIL_POL_NEG_MODE		1

//#define DEFL_OUT_HIGH_MODE	1
//#define DEFL_OUT_LOW_MODE		1
//#define DEFL_FB_HIGH_MODE		1
//#define DEFL_FB_LOW_MODE		1

//#define FOCUS_OUT_HIGH_MODE	1
//#define FOCUS_OUT_LOW_MODE	1
//#define FOCUS_FB_HIGH_MODE	1
//#define FOCUS_FB_LOW_MODE		1

//#define VERIFICATION_MODE_COIL_DEFL	1
//#define VERIFICATION_MODE_MA			1
//#define VERIFICATION_MODE_WATER_TEMP	1
//#define VERIFICATION_MODE_WATER_FLOW	1
//#define VERIFICATION_MODE_KV			1
#endif

#define MAX_FAULT_REPORTS		25

typedef enum faultType
{
	FAULT_INTERLOCK = 1,
	FAULT_HVPS,
	FAULT_KV,
	FAULT_MA,
	FAULT_FILAMENT,
	FAULT_GRID,
	FAULT_COIL_CURRENT,
	FAULT_ION_PUMP_FB,
	FAULT_ION_REPELLER,
	FAULT_PELTIER,
	FAULT_HEATSINK,
	FAULT_COOLANT,
	FAULT_BOARD_VOLTAGE,
	FAULT_PC_COMM_TIMEOUT,
	FAULT_HVPS_COMM,
	FAULT_TIMER_COMM,
	FAULT_HEADBOARD_COMM,
	FAULT_LEDBOARD_COMM,
	FAULT_PELTIER_COMM,
	FAULT_QC,
	FAULT_ADC_BUS,
	FAULT_MEMORY,
	FAULT_INVALID_CONFIG,
	NUM_FAULTS
} FaultType;

enum additionalFaultDetails
{
	ADC_BUS_FAULT_SETUP = 1,
	ADC_BUS_FAULT_TIMEOUT,
	ADC_BUS_FAULT_NACK,
	TMR_FAULT_COMM_TIMEOUT,
	TMR_FAULT_CHECKSUM_1,
	TMR_FAULT_CHECKSUM_2,
	TMR_FAULT_NACK_1,
	TMR_FAULT_NACK_2,
	HEAD_BRD_FAULT_TIMEOUT,
	HEAD_BRD_FAULT_CHECKSUM,
	HVPS_COMM_FAULT_TIMEOUT,
	HVPS_COMM_FAULT_CHECKSUM,
	HVPS_COMM_FAULT_OVERRUN,
	FIL_FAULT_STARTUP,
	FIL_FAULT_RAMP_TIMEOUT,
	FIL_FAULT_OVERCURRENT_SP,
	FIL_FAULT_OVERCURRENT_FB,
	KV_FAULT_RAMP_TIMEOUT,
	KV_FAULT_OOT,
	KV_FAULT_UNWANTED_HV,
	MA_FAULT_UNSTABLE,
	MA_GRID_FAULT_UNDESIRED,
	PLT_COMM_FAULT_TIMEOUT,
	PLT_COMM_FAULT_CHECKSUM,
	ION_REP_FAULT_OVERCURRENT,
	ION_REP_FAULT_OOT,
	COIL_FAULT_X_CURRENT,
	COIL_FAULT_X_VOLTAGE,
	COIL_FAULT_Y_CURRENT,
	COIL_FAULT_Y_VOLTAGE,
	COIL_FAULT_F_CURRENT,
	COIL_FAULT_F_VOLTAGE,
	COOLANT_FAULT_OVERTEMP,
	COOLANT_FAULT_LOW_FLOW,
	COOLANT_FAULT_OVERPRESSURE,
	INVALID_FAULT_PLAN_RELEASE,
	MEMORY_FAULT_OP_IDX
};



typedef struct faultReport
{
	FaultType id;
	uint32_t id_detail;
	uint32_t entry_state;
	uint32_t fault_time;
	float expected_val;
	uint32_t expected_detail;
	float tolerance;
	float measured_val;
	uint32_t measured_detail;
} FaultReport;


extern VariableValue fault_information[FAULT_RES_COUNT];
extern volatile uint32_t internal_time;


void init_faults();
void report_simple_fault(FaultType ftype, float target, float limit, float real);
void report_fault(FaultType ftype, uint32_t type_detail, float target,  float limit, float real);
void report_verbose_fault(FaultType v_ftype, uint32_t v_type_detail, float v_target, uint32_t v_target_detail, float v_limit, float v_real, uint32_t v_real_detail);
void clear_faults();
void process_faults();
void pulse_fault_clear();

#if defined(CALIBRATION_MODE)
//Cal

typedef enum calFault
{
	FLOW_FAULT = 0,
	PRES_FAULT,
	TEMP_FAULT,
	IREP_FAULT,
	IPUM_FAULT,
	DOOR_FAULT
} CalFault;

void fault_detected(CalFault fault_type, bool fault);
bool can_calibrate();
#endif

#endif /* FAULTS_H_ */