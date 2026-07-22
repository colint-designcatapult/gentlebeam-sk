/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Faults
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <lwip/err.h>
#include <lwip/sys.h>
#include <string.h>
#include "checksum.h"
#include "hal_atomic.h"
#include "state_machine.h"
#include "faults.h"

#define FAULT_REPLAY_INTERVAL_MS 1000u

typedef struct
{
	const char *format;
	uint8_t format_length;
	FaultType type;
	uint32_t format_hash;
	uint32_t captured_state;
	uint32_t captured_runtime;
	uint8_t arg_count;
	LogArg_t args[MAX_FAULT_ARGS];
} FaultRecord;

static FaultRecord active_faults[MAX_FAULT_REPORTS];
static uint8_t active_fault_count = 0;
static uint8_t next_fault_to_publish = 0;
static uint32_t fault_clear_epoch = 1;
static bool clear_publish_pending = false;
static uint32_t last_fault_publication_ms = 0;
static volatile bool fault_latched = false;
static volatile bool fault_transition_pending = false;
volatile uint32_t fault_reports_dropped = 0;

static VariableValue fault_telemetry_snapshot[FAULT_RES_COUNT];

volatile bool fault_clear_pulsing = false;
static struct timer_task VTIMER_pulse_clear;

static uint32_t faults_present = 0;

static void finalize_clear_pulse(const struct timer_task *const timer_task);
static FaultType normalize_fault_type(FaultType type);
static bool validate_fault_format(const char *format, uint8_t format_length, uint8_t arg_count);

#if defined(CALIBRATION_MODE)
//cal
bool flow_ok = false;
bool pres_ok = false;
bool temp_ok = false;
bool irep_ok = false;
bool ipum_ok = false;
bool door_ok = false;
#endif

void init_faults(void)
{
	fault_clear_pulsing = false;
}

static FaultType normalize_fault_type(FaultType type)
{
	if(type < FAULT_INTERLOCK || type >= NUM_FAULTS)
	{
		return FAULT_OTHER;
	}

	return type;
}

void fault_latch(FaultType type)
{
	type = normalize_fault_type(type);

	CRITICAL_SECTION_ENTER()
	if(!fault_latched)
	{
		fault_transition_pending = true;
	}
	fault_latched = true;
	faults_present |= 1u << (uint32_t)type;
	system_status[SS_FAULTS].u = faults_present;
	CRITICAL_SECTION_LEAVE()
}

bool consume_fault_transition(void)
{
	bool pending;

	CRITICAL_SECTION_ENTER()
	pending = fault_transition_pending;
	fault_transition_pending = false;
	CRITICAL_SECTION_LEAVE()

	return pending;
}

static bool validate_fault_format(const char *format, uint8_t format_length, uint8_t arg_count)
{
	if(format == NULL || format_length >= FAULT_FORMAT_BYTES || arg_count > MAX_FAULT_ARGS)
	{
		return false;
	}
	if(format[format_length] != '\0')
	{
		return false;
	}

	uint8_t consuming_specifiers = 0;
	for(uint8_t i = 0; i < format_length; i++)
	{
		unsigned char current = (unsigned char)format[i];
		if(current < 0x20u || current > 0x7Eu)
		{
			return false;
		}
		if(current != '%')
		{
			continue;
		}
		if(++i >= format_length)
		{
			return false;
		}

		switch(format[i])
		{
			case '%':
				break;
			case 'd':
			case 'u':
			case 'x':
			case 'X':
			case 'f':
				consuming_specifiers++;
				break;
			default:
				return false;
		}
	}

	return consuming_specifiers == arg_count;
}

void record_fault_internal(FaultType type, const char *format, uint8_t format_length, uint8_t arg_count, const LogArg_t *args)
{
	if(args == NULL && arg_count > 0u)
	{
		return;
	}
	if(!validate_fault_format(format, format_length, arg_count))
	{
		return;
	}

	type = normalize_fault_type(type);
	uint32_t format_hash = crc_32((const unsigned char *)format, format_length);
	bool duplicate = false;

	CRITICAL_SECTION_ENTER()
	for(uint8_t i = 0; i < active_fault_count; i++)
	{
		const FaultRecord *active = &active_faults[i];
		if(active->format_hash == format_hash &&
			active->format_length == format_length &&
			memcmp(active->format, format, format_length) == 0)
		{
			duplicate = true;
			break;
		}
	}

	if(!duplicate)
	{
		if(active_fault_count >= MAX_FAULT_REPORTS)
		{
			fault_reports_dropped++;
		}
		else
		{
			FaultRecord *record = &active_faults[active_fault_count];
			record->format = format;
			record->format_length = format_length;
			record->type = type;
			record->format_hash = format_hash;
			record->captured_state = system_status[SS_STATE].u;
			record->captured_runtime = system_status[SS_SYS_RUNTIME].u;
			record->arg_count = arg_count;
			memset(record->args, 0, sizeof(record->args));
			if(arg_count > 0u)
			{
				memcpy(record->args, args, arg_count * sizeof(LogArg_t));
			}
			active_fault_count++;
		}
	}
	CRITICAL_SECTION_LEAVE()
}

void serialize_fault_response(uint32_t requested_index, VariableValue response[FAULT_RES_COUNT])
{
	memset(response, 0, sizeof(VariableValue) * FAULT_RES_COUNT);

	CRITICAL_SECTION_ENTER()
	response[FAULT_RES_CLEAR_EPOCH].u = fault_clear_epoch;
	response[FAULT_RES_ENTRY_INDEX].u = requested_index;
	response[FAULT_RES_ACTIVE_COUNT].u = active_fault_count;

	if(requested_index < active_fault_count)
	{
		const FaultRecord *record = &active_faults[requested_index];
		response[FAULT_RES_TYPE].u = (uint32_t)record->type;
		response[FAULT_RES_FORMAT_HASH].u = record->format_hash;
		response[FAULT_RES_STATE].u = record->captured_state;
		response[FAULT_RES_TIME].u = record->captured_runtime;
		response[FAULT_RES_ARG_COUNT].u = record->arg_count;
		memcpy((uint8_t *)&response[FAULT_RES_FORMAT_0], record->format, record->format_length);
		for(uint8_t i = 0; i < record->arg_count; i++)
		{
			memcpy(&response[FAULT_RES_ARG_0 + i], &record->args[i], sizeof(LogArg_t));
		}
	}
	CRITICAL_SECTION_LEAVE()
}

void clear_faults(void)
{
	gpio_set_pin_level(IO_LED1, true);
	pulse_fault_clear();

	CRITICAL_SECTION_ENTER()
	fault_latched = false;
	fault_transition_pending = false;
	active_fault_count = 0;
	next_fault_to_publish = 0;
	faults_present = 0;
	system_status[SS_FAULTS].u = 0;
	fault_clear_epoch++;
	clear_publish_pending = true;
	CRITICAL_SECTION_LEAVE()
}

//Perform pulsing to unlatch master fault on interlock hardware preventing HV and grid
void pulse_fault_clear(void)
{
	if(!fault_clear_pulsing)
	{
		gpio_set_pin_level(IO_CLEAR_FAULT, true);
		VTIMER_pulse_clear.interval = 100;				//100 ms tick
		VTIMER_pulse_clear.cb = finalize_clear_pulse;
		VTIMER_pulse_clear.mode = TIMER_TASK_ONE_SHOT;
		timer_add_task(&VTIMER, &VTIMER_pulse_clear);
		fault_clear_pulsing = true;
	}
}

static void finalize_clear_pulse(const struct timer_task *const timer_task)
{
	gpio_set_pin_level(IO_CLEAR_FAULT, false);
	fault_clear_pulsing = false;
}

void process_faults(void)
{
	bool publish_clear;
	bool latched;
	uint8_t active_count;
	uint8_t publish_index;
	uint32_t publish_epoch;
	uint32_t now = sys_now();

	CRITICAL_SECTION_ENTER()
	publish_clear = clear_publish_pending;
	latched = fault_latched;
	active_count = active_fault_count;
	publish_index = next_fault_to_publish;
	publish_epoch = fault_clear_epoch;

	if(!publish_clear &&
		publish_index >= active_count &&
		(uint32_t)(now - last_fault_publication_ms) >= FAULT_REPLAY_INTERVAL_MS)
	{
		if(active_count > 0u)
		{
			next_fault_to_publish = 0;
			publish_index = 0;
		}
		else if(publish_epoch > 1u)
		{
			publish_clear = true;
		}
	}
	CRITICAL_SECTION_LEAVE()

	if(latched)
	{
		gpio_set_pin_level(IO_LED1, false);
	}

	if(publish_clear)
	{
		memset(fault_telemetry_snapshot, 0, sizeof(fault_telemetry_snapshot));
		fault_telemetry_snapshot[FAULT_RES_CLEAR_EPOCH].u = publish_epoch;
		if(send_telemetry_packet(PC_TELEMETRY_PORT, PCCOM_FAULT_REQUEST,
			fault_telemetry_snapshot, FAULT_RES_COUNT) == ERR_OK)
		{
			last_fault_publication_ms = now;
			CRITICAL_SECTION_ENTER()
			if(fault_clear_epoch == publish_epoch)
			{
				clear_publish_pending = false;
			}
			CRITICAL_SECTION_LEAVE()
		}
		return;
	}

	serialize_fault_response(publish_index, fault_telemetry_snapshot);
	if(fault_telemetry_snapshot[FAULT_RES_TYPE].u == 0u)
	{
		return;
	}

	if(send_telemetry_packet(PC_TELEMETRY_PORT, PCCOM_FAULT_REQUEST,
		fault_telemetry_snapshot, FAULT_RES_COUNT) == ERR_OK)
	{
		last_fault_publication_ms = now;
		CRITICAL_SECTION_ENTER()
		if(!clear_publish_pending &&
			fault_clear_epoch == publish_epoch &&
			next_fault_to_publish == publish_index)
		{
			next_fault_to_publish++;
		}
		CRITICAL_SECTION_LEAVE()
	}
}

#if defined(CALIBRATION_MODE)
void fault_detected(CalFault fault_type, bool fault)
{
	switch (fault_type)
	{
		case FLOW_FAULT:
			flow_ok = !fault;
			if (fault)
			{
				signal_emission_stop();
			}
			break;
		case PRES_FAULT:
			pres_ok = !fault;
			if (fault)
			{
				signal_emission_stop();
			}
			break;
		case TEMP_FAULT:
			temp_ok = !fault;
			if (fault)
			{
				signal_emission_stop();
			}
			break;
		case IREP_FAULT:
			irep_ok = !fault;
			if (fault)
			{
				signal_emission_stop();
			}
			break;
		case IPUM_FAULT:
			ipum_ok = !fault;
			if (fault)
			{
				signal_emission_stop();
			}
			break;
		case DOOR_FAULT:
			door_ok = !fault;
			if (fault)
			{
				signal_hvps_stop();
			}
			break;
		default:
			break;
	}
}

bool can_calibrate()
{
	return flow_ok && pres_ok && temp_ok && irep_ok && ipum_ok && door_ok;
}
#endif

/*
static void log_timer_cb(uint32_t status)
{
	log_time++;
}
*/