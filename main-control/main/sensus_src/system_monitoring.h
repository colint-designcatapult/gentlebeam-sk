/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: System monitoring
*	Author: Carlton Chow
*	Description:
*/


#ifndef SYSTEM_MONITORING_H_
#define SYSTEM_MONITORING_H_

#define SYSTEM_MONITOR_INTERVAL		10

#define PC_COMM_TIMEOUT_COUNT	200

enum systemMonitoringRR
{
	SYS_MON_ROUND_0 = 0,
	SYS_MON_ROUND_1,
	SYS_MON_ROUND_2,
	SYS_MON_ROUND_3,
	SYS_MON_ROUND_4,
	SYS_MON_NUM_ROUNDS	
};


enum systemMonitoringFields
{
	SMON_ROUND = 0,
	SMON_LAST_HS_FAN_STATE,
	SMON_LAST_CB_FAN_STATE,
	SMON_ION_R_A_OOT_COUNTER,
	SMON_ION_R_V_OOT_COUNTER,
	SMON_3V3_OOT_COUNTER,
	SMON_5V_OOT_COUNTER,
	SMON_12V_OOT_COUNTER,
	SMON_ION_P_HI_COUNTER,
	SMON_CLNT_P_HI_COUNTER,
	SMON_CLNT_P_LO_COUNTER,
	SMON_CLNT_F_HI_COUNTER,
	SMON_CLNT_F_LO_COUNTER,
	SMON_COUNT
};

enum expectedValues
{
	EV_COIL_X_A,
	EV_COIL_X_V,
	EV_COIL_Y_A,
	EV_COIL_Y_V,
	EV_COIL_F_A,
	EV_COIL_F_V,
	EV_COUNT
};

extern float expected_coil_value[EV_COUNT];

void init_system_monitoring();
void process_system_monitoring();
void reset_pc_comm_timeout();
void enable_indicators(bool on);
void enable_pump(bool on);

bool tolerance_check_rel(float target, float actual, float tolerance);
bool tolerance_check_abs(float target, float actual, float tolerance);


#endif /* SYSTEM_MONITORING_H_ */