/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Faults
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <string.h>
#include "state_machine.h"
#include "faults.h"

volatile uint32_t log_time = 0;

//Information on latest fault
VariableValue fault_information[FAULT_RES_COUNT];

//Queued list of faults
FaultReport fault_reports[MAX_FAULT_REPORTS];
volatile uint8_t fault_report_idx = 0;

volatile bool fault_clear_pulsing = false;
static struct timer_task VTIMER_pulse_clear;

uint32_t faults_present = 0;

static void finalize_clear_pulse(const struct timer_task *const timer_task);
//static void log_timer_cb(uint32_t status);

#if defined(CALIBRATION_MODE)
//cal
bool flow_ok = false;
bool pres_ok = false;
bool temp_ok = false;
bool irep_ok = false;
bool ipum_ok = false;
bool door_ok = false;
#endif

void init_faults()
{
	fault_clear_pulsing = false;
	
	//Clear fault information
	memset(fault_information, 0, sizeof(VariableValue) * FAULT_RES_COUNT);
	
	/*
	LOG_TIMER_init();
	tc_register_callback(TC1, 0, log_timer_cb);
	start_timer(TC1, 0);*/
}

//Fault reporting when no "detailed" values need to be sent
void report_simple_fault(FaultType s_ftype, float s_target, float s_limit, float s_real)
{
	report_verbose_fault(s_ftype, 0, s_target, 0, s_limit, s_real, 0);
}

//Fault reporting when only the type needs detail
void report_fault(FaultType ftype, uint32_t type_detail, float target, float limit, float real)
{
	report_verbose_fault(ftype, type_detail, target, 0, limit, real, 0);
}

//Fault reporting can be called from interrupts, keep short
void report_verbose_fault(FaultType v_ftype, uint32_t v_type_detail, float v_target, uint32_t v_target_detail, float v_limit, float v_real, uint32_t v_real_detail)
{	
	if(fault_report_idx >= MAX_FAULT_REPORTS)
	{
		//Do nothing if we overrun the fault reports
		//If this happens something went very wrong
		return;
	}
	
	uint32_t fault_bit = 1;
	fault_bit <<= v_ftype;
	
	//If we are already in fault mode and the fault has been reported ignore it
	if((faults_present & (1<<v_ftype)) && (system_status[SS_STATE].i == STATE_FAULT || system_status[SS_STATE].i == STATE_COLD_FAULT || system_status[SS_STATE].i == STATE_WARMUP_FAULT))
	{
		return;
	}
	//Otherwise log the fault
	else
	{
		faults_present |= fault_bit;
	}
	
	//Update system status fault bits
	system_status[SS_FAULTS].i = faults_present;
	
	//Log fault details
	fault_reports[fault_report_idx].id = v_ftype;
	fault_reports[fault_report_idx].id_detail = v_type_detail;
	fault_reports[fault_report_idx].entry_state = system_status[SS_STATE].u;
	fault_reports[fault_report_idx].fault_time = system_status[SS_SYS_RUNTIME].u;
	fault_reports[fault_report_idx].expected_val = v_target;
	fault_reports[fault_report_idx].expected_detail = v_target_detail;
	fault_reports[fault_report_idx].tolerance = v_limit;
	fault_reports[fault_report_idx].measured_val = v_real;
	fault_reports[fault_report_idx].measured_detail = v_real_detail;
	
	fault_report_idx++;
}


void clear_faults()
{
	/*if(fault_clear_pulsing)
	{
		return;
	}*/
	//Clear existing fault information
	//Note we are not clearing any faults reported before clear command was received
	//(aka the fault reports table remains as is, only clear fault information)
	memset(fault_information, 0, sizeof(VariableValue) * FAULT_RES_COUNT);
	
	faults_present = 0;
	
	system_status[SS_FAULTS].i = 0;
	
	gpio_set_pin_level(IO_LED1, true);
	
	pulse_fault_clear();
}

//Perform pulsing to unlatch master fault on interlock hardware preventing HV and grid
void pulse_fault_clear()
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

void process_faults()
{
	//Check to see if new faults have been reported
	if(fault_report_idx > 0)
	{
		gpio_set_pin_level(IO_LED1, false);
		
		// TODO: report fault only if in emission
		
		//Report to state machine that a fault is present
		queue_sm_event(EVENT_FAULT);
		
		//For now, do nothing with queued faults beyond the first
		//Can potentially update in the future to log additional faults
		
		//If no other fault is currently reported, report oldest fault to PC
		if(fault_information[FAULT_RES_ID].i == 0)
		{
			fault_information[FAULT_RES_ID].i = fault_reports[0].id;
			fault_information[FAULT_RES_ID_DETAIL].u = fault_reports[0].id_detail;
			fault_information[FAULT_RES_STATE].u = fault_reports[0].entry_state;
			fault_information[FAULT_RES_TIME].u = fault_reports[0].fault_time;
			fault_information[FAULT_RES_EXPECTED].f = fault_reports[0].expected_val;
			fault_information[FAULT_RES_EXPECTED_DETAIL].u = fault_reports[0].expected_detail;
			fault_information[FAULT_RES_TOLERANCE].f = fault_reports[0].tolerance;
			fault_information[FAULT_RES_MEASURED].f = fault_reports[0].measured_val;
			fault_information[FAULT_RES_MEASURED_DETAIL].u = fault_reports[0].measured_detail;
		}
		
		//Reset fault report index
		fault_report_idx = 0;
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