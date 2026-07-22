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

#include <stddef.h>
#include <stdint.h>

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

#define MAX_FAULT_REPORTS		4
#define FAULT_FORMAT_BYTES		128
#define MAX_FAULT_ARGS			5

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
	FAULT_OTHER,
	NUM_FAULTS
} FaultType;

typedef union
{
	int32_t i;
	uint32_t u;
	float f;
} LogArg_t;

_Static_assert(sizeof(LogArg_t) == sizeof(uint32_t), "LogArg_t must be one protocol word");

#define MAKE_ARG(x) \
	_Generic((x), \
		float: (LogArg_t){ .f = (x) }, \
		double: (LogArg_t){ .f = (float)(x) }, \
		int32_t: (LogArg_t){ .i = (x) }, \
		uint32_t: (LogArg_t){ .u = (x) }, \
		default: (LogArg_t){ .i = (int32_t)(x) })

#define FAULT_FORMAT_ASSERT(format) \
	_Static_assert(__builtin_constant_p(format), "fault format must be a string literal"); \
	_Static_assert(sizeof(format) <= FAULT_FORMAT_BYTES, "fault format exceeds 127 ASCII bytes")

#define report_typed_fault(type, format) \
	do { \
		FAULT_FORMAT_ASSERT(format); \
		fault_latch(type); \
		record_fault_internal(type, format, (uint8_t)(sizeof(format) - 1u), 0u, NULL); \
	} while (0)

#define report_typed_fault1(type, format, arg1) \
	do { \
		FAULT_FORMAT_ASSERT(format); \
		fault_latch(type); \
		const LogArg_t fault_args_[] = { (arg1) }; \
		record_fault_internal(type, format, (uint8_t)(sizeof(format) - 1u), 1u, fault_args_); \
	} while (0)

#define report_typed_fault2(type, format, arg1, arg2) \
	do { \
		FAULT_FORMAT_ASSERT(format); \
		fault_latch(type); \
		const LogArg_t fault_args_[] = { (arg1), (arg2) }; \
		record_fault_internal(type, format, (uint8_t)(sizeof(format) - 1u), 2u, fault_args_); \
	} while (0)

#define report_typed_fault3(type, format, arg1, arg2, arg3) \
	do { \
		FAULT_FORMAT_ASSERT(format); \
		fault_latch(type); \
		const LogArg_t fault_args_[] = { (arg1), (arg2), (arg3) }; \
		record_fault_internal(type, format, (uint8_t)(sizeof(format) - 1u), 3u, fault_args_); \
	} while (0)

#define report_typed_fault4(type, format, arg1, arg2, arg3, arg4) \
	do { \
		FAULT_FORMAT_ASSERT(format); \
		fault_latch(type); \
		const LogArg_t fault_args_[] = { (arg1), (arg2), (arg3), (arg4) }; \
		record_fault_internal(type, format, (uint8_t)(sizeof(format) - 1u), 4u, fault_args_); \
	} while (0)

#define report_typed_fault5(type, format, arg1, arg2, arg3, arg4, arg5) \
	do { \
		FAULT_FORMAT_ASSERT(format); \
		fault_latch(type); \
		const LogArg_t fault_args_[] = { (arg1), (arg2), (arg3), (arg4), (arg5) }; \
		record_fault_internal(type, format, (uint8_t)(sizeof(format) - 1u), 5u, fault_args_); \
	} while (0)

#define report_fault(format) \
	report_typed_fault(FAULT_OTHER, format)

#define report_fault1(format, arg1) \
	report_typed_fault1(FAULT_OTHER, format, arg1)

#define report_fault2(format, arg1, arg2) \
	report_typed_fault2(FAULT_OTHER, format, arg1, arg2)

#define report_fault3(format, arg1, arg2, arg3) \
	report_typed_fault3(FAULT_OTHER, format, arg1, arg2, arg3)

#define report_fault4(format, arg1, arg2, arg3, arg4) \
	report_typed_fault4(FAULT_OTHER, format, arg1, arg2, arg3, arg4)

#define report_fault5(format, arg1, arg2, arg3, arg4, arg5) \
	report_typed_fault5(FAULT_OTHER, format, arg1, arg2, arg3, arg4, arg5)

extern volatile uint32_t fault_reports_dropped;
extern volatile uint32_t internal_time;

void init_faults(void);
void fault_latch(FaultType type);
void record_fault_internal(FaultType type, const char *format, uint8_t format_length, uint8_t arg_count, const LogArg_t *args);
bool consume_fault_transition(void);
void serialize_fault_response(uint32_t requested_index, VariableValue response[FAULT_RES_COUNT]);
void clear_faults(void);
void process_faults(void);
void pulse_fault_clear(void);

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