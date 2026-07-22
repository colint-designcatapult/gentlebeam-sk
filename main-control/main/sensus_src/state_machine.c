/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Control state machine
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include "hal_atomic.h"
#include "ext_dac.h"
#include "ext_timers.h"
#include "faults.h"
#include "head_board.h"
#include "hvps.h"
#include "system_monitoring.h"
#include "system_parameters.h"
#include "state_machine.h"

#include "ftdi.h"

static void run_crash_state(EventType ev);
static void run_startup_state(EventType ev);
static void goto_cold_state();
static void run_cold_state(EventType ev);
static void goto_cold_fault_state();
static void run_cold_fault_state(EventType ev);
static void goto_conditioning_state();
static void run_conditioning_state(EventType ev);
static void goto_warmup_state();
static void run_warmup_state(EventType ev);
static void goto_warmup_fault_state();
static void run_warmup_fault_state(EventType ev);
static void goto_primed_state();
static void run_primed_state(EventType ev);
static void goto_staging_state();
static void run_staging_state(EventType ev);
static void goto_staged_state();
static void run_staged_state(EventType ev);
static void goto_hvps_check_state();
static void run_hvps_check_state(EventType ev);
static void goto_setup_state();
static void run_setup_state(EventType ev);
static void goto_ready_state();
static void run_ready_state(EventType ev);
static void goto_launching_state();
static void run_launching_state(EventType ev);
static void goto_emission_state();
static void run_emission_state(EventType ev);
static void goto_termination_state();
static void run_termination_state(EventType ev);
static void goto_discharge_state();
static void run_discharge_state(EventType ev);
static void goto_fault_state();
static void run_fault_state(EventType ev);
static void transition_to_latched_fault(void);

static void deci_second_timer(const struct timer_task *const timer_task);

static void clear_timers();

static struct timer_task VTIMER_beamon_timer;
static struct timer_task VTIMER_deci_second_timer;

//Get+set state from system status
XState *state = (XState *)(system_status+SS_STATE);

volatile EventType event_queue[MAX_EVENT_QUEUE_SIZE];
volatile uint32_t event_q_idx = 0;
volatile uint32_t event_q_end = 0;

volatile bool wait_for_release_point = false;

volatile int warmup_deci_seconds = 0;

volatile int deci_seconds_remaining = 0;

volatile int standby_deci_seconds = STANDBY_TICKS;

void (*state_exec_func[NUM_SYSTEM_STATES]) (EventType ev);


void init_state_machine()
{
	system_status[SS_SYS_RUNTIME].u = 0;
	
	//Initialize system to the startup state
	*state = STATE_STARTUP;
	
	//Set up timer to tick at 1/10 of a second or 100 ms
	VTIMER_deci_second_timer.interval = 100;
	VTIMER_deci_second_timer.cb = deci_second_timer;
	VTIMER_deci_second_timer.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_deci_second_timer);
	
	//Initialize function pointers for state machine execution
	for(int i = 0; i < NUM_SYSTEM_STATES; i++)
	{
		//Set everything initially in case additional states are
		//added and then later not explicitly handled
		state_exec_func[i] = run_crash_state;
	}
	
	//Assign the functions to run each individual state
	state_exec_func[STATE_STARTUP] = run_startup_state;
	state_exec_func[STATE_COLD] = run_cold_state;
	state_exec_func[STATE_COLD_FAULT] = run_cold_fault_state;
	state_exec_func[STATE_CONDITIONING] = run_conditioning_state;
	state_exec_func[STATE_WARMUP] = run_warmup_state;
	state_exec_func[STATE_WARMUP_FAULT] = run_warmup_fault_state;
	state_exec_func[STATE_PRIMED] = run_primed_state;
	state_exec_func[STATE_STAGING] = run_staging_state;
	state_exec_func[STATE_STAGED] = run_staged_state;
	state_exec_func[STATE_HVPS_CHECK] = run_hvps_check_state;
	state_exec_func[STATE_SETUP] = run_setup_state;
	state_exec_func[STATE_READY] = run_ready_state;
	state_exec_func[STATE_LAUNCHING] = run_launching_state;
	state_exec_func[STATE_EMISSION] = run_emission_state;
	state_exec_func[STATE_TERMINATION] = run_termination_state;
	state_exec_func[STATE_DISCHARGE] = run_discharge_state;
	state_exec_func[STATE_FAULT] = run_fault_state;
	state_exec_func[STATE_SYSTEM_CRASH] = run_crash_state;
	
}

//100 ms timer
static void deci_second_timer(const struct timer_task *const timer_task)
{
	switch(*state)
	{
		case STATE_EMISSION:
			//update internal emission timer
			if(--deci_seconds_remaining <= 0)
			{
				queue_sm_event(EVENT_OP_COMPLETE);
			}
			system_status[SS_INTERNAL_TIMER_VAL].f += 0.1f;
			int op_idx = system_status[SS_OP_IDX].i;
			if(op_idx < 0)
			{
				report_typed_fault1(FAULT_MEMORY, "Operational-point index %d is negative.", MAKE_ARG(op_idx));
			}
			else if(op_idx >= system_status[SS_OP_COUNT].i)
			{
				report_typed_fault2(FAULT_MEMORY, "Operational-point index %d exceeds the loaded count %d.", MAKE_ARG(op_idx), MAKE_ARG(system_status[SS_OP_COUNT].i));
				
			}
			else if(op_idx >= MAX_OPERATIONAL_POINTS)
			{
				report_typed_fault2(FAULT_MEMORY, "Operational-point index %d exceeds the firmware maximum %u.", MAKE_ARG(op_idx), MAKE_ARG(MAX_OPERATIONAL_POINTS));
			}
			else
			{
				operational_points[op_idx][OP_REMAIN_TIME].f -= 0.1;
			}
			break;
		case STATE_PRIMED: //timeout for if system is warmed up but not executing a plan
		case STATE_STAGING:
		case STATE_STAGED:
		case STATE_READY:
		case STATE_FAULT:
			if(--standby_deci_seconds <= 0)
			{
				queue_sm_event(EVENT_PC_STANDBY);
			}
			break;
		default:
			break;
	}
}

void queue_sm_event(EventType ev)
{
	//Don't queue if event is invalid
	if(ev < 0 || ev >= NUM_EVENT_TYPES)
	{
		return;
	}
	//Queue event
	if(++event_q_end >= MAX_EVENT_QUEUE_SIZE)
	{
		event_q_end = 0;
	}
	event_queue[event_q_end] = ev;
}

void process_state_machine()
{
	if(consume_fault_transition())
	{
		transition_to_latched_fault();
		return;
	}

	//Dequeue all queued events and run state machine
	while(event_q_idx != event_q_end)
	{
		if(++event_q_idx >= MAX_EVENT_QUEUE_SIZE)
		{
			event_q_idx = 0;
		}

		//Ensure state index is valid
		if(*state < NUM_SYSTEM_STATES)
		{
			//If so, run the appropriate state processing function
			//using the given event trigger
			(*state_exec_func[*state])(event_queue[event_q_idx]);
		}
		else
		{
			run_crash_state(event_queue[event_q_idx]);
		}

		if(consume_fault_transition())
		{
			transition_to_latched_fault();
			return;
		}
	}
}

static void transition_to_latched_fault(void)
{
	XState captured_state;

	CRITICAL_SECTION_ENTER()
	captured_state = *state;
	event_q_idx = event_q_end;
	CRITICAL_SECTION_LEAVE()

	switch(captured_state)
	{
		case STATE_STARTUP:
		case STATE_COLD:
			goto_cold_fault_state();
			break;
		case STATE_CONDITIONING:
		case STATE_WARMUP:
			goto_warmup_fault_state();
			break;
		case STATE_COLD_FAULT:
		case STATE_WARMUP_FAULT:
		case STATE_FAULT:
		case STATE_SYSTEM_CRASH:
			break;
		default:
			goto_fault_state();
			break;
	}
}


//Catch all function for system crashes
static void run_crash_state(EventType ev)
{
	//Disable HV interlock
	enable_hv(false);
	//Disable EMISSION interlock
	enable_ecc(false);
	
	//Ensure kV, source heater and grid voltages are 0
	set_hvps_kv(0, 0);
	set_hvps_heater(0);
	set_hvps_grid(0);
	
	//Ensure coil outputs are disabled
	set_coil_voltage(X_COIL_DAC_CH, 0);
	set_coil_voltage(Y_COIL_DAC_CH, 0);
	set_coil_voltage(F_COIL_DAC_CH, 0);
	
	//Stop coolant pump and fan
	enable_pump(false);
	//Stop x-ray indicators
	enable_indicators(false);
	
	*state = STATE_SYSTEM_CRASH;
}

static void run_startup_state(EventType ev)
{
	if(ev == EVENT_STARTUP_INIT)
	{
		clear_faults();
		goto_cold_state();
	}
}

static void goto_cold_state()
{
	//Disable HV interlock
	enable_hv(false);
	//Disable EMISSION interlock (grid)
	enable_ecc(false);
	
	//Ensure kV, source heater and grid voltages are 0
	set_hvps_kv(0, 0);
	set_hvps_heater(0);
	set_hvps_grid(0);
	
	//Ensure coil outputs are disabled
	set_coil_voltage(X_COIL_DAC_CH, 0);
	set_coil_voltage(Y_COIL_DAC_CH, 0);
	set_coil_voltage(F_COIL_DAC_CH, 0);
	
	//Stop coolant pump and fan
	enable_pump(false);
	//Stop x-ray indicators
	enable_indicators(false);
		
	set_led_sequence(LED_SEQ_COLD);
	
	*state = STATE_COLD;
}

static void run_cold_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_cold_fault_state();
	}
	else if(ev == EVENT_PC_CONDITION)
	{
		goto_conditioning_state();
	}
	else if(ev == EVENT_PC_WARMUP)
	{
		goto_warmup_state();
	}
	else if(ev == EVENT_PC_WIPE_PLAN)
	{
		clear_treatment_plan();
	}
	else if(ev == EVENT_PC_RESET_TIMERS)
	{
		clear_timers();
	}
}

static void goto_cold_fault_state()
{
	//Disable HV interlock
	enable_hv(false);
	//Disable EMISSION interlock (grid)
	enable_ecc(false);
	
	//Ensure kV, source heater and grid voltages are 0
	set_hvps_kv(0, 0);
	set_hvps_heater(0);
	set_hvps_grid(0);
	
	//Ensure coil outputs are disabled
	set_coil_voltage(X_COIL_DAC_CH, 0);
	set_coil_voltage(Y_COIL_DAC_CH, 0);
	set_coil_voltage(F_COIL_DAC_CH, 0);
	
	set_led_sequence(LED_SEQ_FAULT);
	
	*state = STATE_COLD_FAULT;
}

static void run_cold_fault_state(EventType ev)
{
	//Go to cold if no faults are currently queued after PC request
	if(ev == EVENT_PC_CLEAR_FAULT)
	{
		clear_faults();
		goto_cold_state();
	}
	else if(ev == EVENT_PC_RESET_TIMERS)
	{
		clear_timers();
	}
}

static void goto_conditioning_state()
{
	//If estops are pressed do not proceed to conditioning
	if(!verify_estops_ok())
	{
		return;
	}
	
	//Set new heater value
	enable_fast_warmup(false);
	float htr_val = hvps_config[HVPS_CONF_CONDITION_I];
	set_hvps_heater(htr_val);
	
	//Start coolant pump and fan
	enable_pump(true);
	
	set_led_sequence(LED_SEQ_WARMUP);
	
	*state = STATE_CONDITIONING;
}

static void run_conditioning_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_warmup_fault_state();
	}
	else if(ev == EVENT_PC_STOP)
	{
		goto_cold_state();
	}
	else if(ev == EVENT_HVPS_SP_REACHED)
	{
		goto_primed_state();
	}
}

static void goto_warmup_state()
{
	//If estops are pressed do not proceed to warmup
	if(!verify_estops_ok())
	{
		return;
	}
	
	//Set new heater value
	enable_fast_warmup(true);
	float htr_val = hvps_config[HVPS_CONF_WARMUP_I];
	set_hvps_heater(htr_val);
	
	//Start coolant pump and fan
	enable_pump(true);
	
	set_led_sequence(LED_SEQ_WARMUP);
	
	*state = STATE_WARMUP;
}

static void run_warmup_state(EventType ev)
{
	//Go to fault state if fault reported
    if(ev == EVENT_FAULT)
    {
		goto_warmup_fault_state();
    }
	//Go back to cold if PC requests a stop
	else if(ev == EVENT_PC_STOP)
	{
		goto_cold_state();
	}
	//Once warmup has reached target setpoint
	else if(ev == EVENT_HVPS_SP_REACHED)
	{
		//If a plan is already staged, go to staged
		if(plan_info[PLAN_STAGED_BOOL] != 0)
		{
			goto_staged_state();
		}
		//If a plan is not yet staged, go to primed
		else
		{
			goto_primed_state();	
		}
	}
}

static void goto_warmup_fault_state()
{	
	//Ensure source heater voltage is 0
	set_hvps_heater(0);

	//Stop coolant pump and fan
	enable_pump(false);
	
	set_led_sequence(LED_SEQ_WARMUP_FAULT);
	
	//Set state
	*state = STATE_WARMUP_FAULT;
}

static void run_warmup_fault_state(EventType ev)
{
	if(ev == EVENT_PC_CLEAR_FAULT)
	{
		clear_faults();
		goto_cold_state();
	}
	else if(ev == EVENT_PC_RESET_TIMERS)
	{
		clear_timers();
	}
}

static void goto_primed_state()
{
	//Restart standby timer
	standby_deci_seconds = PRIMED_STANDBY_TICKS;
	
	//Ensure all plan information is cleared
	clear_treatment_plan();
	
	set_led_sequence(LED_SEQ_PRIMED);
	
	*state = STATE_PRIMED;
}

static void run_primed_state(EventType ev)
{	
	//Go to fault state if fault reported
	if(ev == EVENT_FAULT)
	{		
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STANDBY)
	{
		goto_discharge_state();
	}
	else if(ev == EVENT_PC_RESET_TIMERS)
	{
		clear_timers();
	}
	else if(ev == EVENT_PC_NEW_SESSION)
	{		
		//Check to ensure timers are reset
		bool ok_to_proceed = true;
		
		ok_to_proceed &= (system_status[SS_INTERNAL_TIMER_VAL].f == 0.0);
		ok_to_proceed &= (system_status[SS_TIMER_1_VAL].f == 0.0);
		ok_to_proceed &= (system_status[SS_TIMER_2_VAL].f == 0.0);
		
		//Check that an appropriate number of points is requested
		ok_to_proceed &= (system_status[SS_OP_COUNT].i > 0);
		ok_to_proceed &= (system_status[SS_OP_COUNT].i <= MAX_OPERATIONAL_POINTS);
		
		//Only proceed if system is in ok state
		if(ok_to_proceed)
		{
			goto_staging_state();	
		}
		else
		{
			//Do not continue and report fault if system is not in an ok state
			report_typed_fault(FAULT_INVALID_CONFIG, "The loaded configuration is invalid.");
		}
	}
}

static void goto_staging_state()
{
	//Restart standby timer
	standby_deci_seconds = STANDBY_TICKS;
		
	//Set up target flags for plan points
	set_plan_flags();
	
	//Go to staging state
	*state = STATE_STAGING;
}

static void run_staging_state(EventType ev)
{
	//Go to fault state if fault reported
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STANDBY)
	{
		goto_discharge_state();
	}
	//Go back to primed if plan is wiped
	else if(ev == EVENT_PC_WIPE_PLAN)
	{
		goto_primed_state();
	}
	//Move to staged if directed
	//Point loading should already be checked by comms
	else if(ev == EVENT_PC_FINISH_STAGE)
	{
		goto_staged_state();
	}
}

static void goto_staged_state()
{
	//Restart standby timer
	standby_deci_seconds = STANDBY_TICKS;
	
	//Ensure confirmation flags are completely wiped
	plan_info[PLAN_CONFIRMATION_FLAGS_1] = 0;
	plan_info[PLAN_CONFIRMATION_FLAGS_2] = 0;
	
	//Set flag indicating plan is staged
	plan_info[PLAN_STAGED_BOOL] = 1;
	
	set_led_sequence(LED_SEQ_PRIMED);
	
	*state = STATE_STAGED;
}

static void run_staged_state(EventType ev)
{
	//Go to fault state if fault reported
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STANDBY)
	{
		goto_discharge_state();
	}
	//Go back to primed if plan is wiped
	else if(ev == EVENT_PC_WIPE_PLAN)
	{
		goto_primed_state();
	}
	else if(ev == EVENT_PC_RESET_TIMERS)
	{
		clear_timers();
	}
	//Release plan (confirm checked by comms)
	else if(ev == EVENT_PC_RELEASE_PLAN)
	{
		//Make sure all interlocks are good before releasing plan
		bool ok_to_proceed = true;
		
		ok_to_proceed &= verify_keys_ok();
		ok_to_proceed &= verify_collimator_ok();
		ok_to_proceed &= verify_door_ok();
#if defined(CALIBRATION_MODE)
		ok_to_proceed &= verify_spare_interlock_2_ok();
#endif
				
		//Check to make sure timers are reset before releasing plan
		ok_to_proceed &= (system_status[SS_INTERNAL_TIMER_VAL].f == 0.0);
		ok_to_proceed &= (system_status[SS_TIMER_1_VAL].f == 0.0);
		ok_to_proceed &= (system_status[SS_TIMER_2_VAL].f == 0.0);
		
		if(ok_to_proceed)
		{
			//Always set wait for resume on first point
			wait_for_release_point = true;
			goto_hvps_check_state();	
		}
		else
		{
			//Do not continue and report fault if system is not in an ok state
			report_typed_fault(FAULT_INVALID_CONFIG, "The loaded configuration is invalid.");
		}
	}
}

static void goto_hvps_check_state()
{	
	//Disable HV interlock
	enable_hv(false);
	//Disable EMISSION interlock (grid)
	enable_ecc(false);
	//Disable GRID interlock (grid watchdog)
	enable_grid(false);
	
	init_hvps_check();
	
	*state = STATE_HVPS_CHECK;
}

static void run_hvps_check_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_HVPS_CHECK)
	{
		bool check_done = update_hvps_check();
		if(check_done)
		{
			goto_setup_state();
		}
	}
}

static void goto_setup_state()
{
	//Ensure OP index is valid
	int op_idx = system_status[SS_OP_IDX].i;
	float seconds = 0;
	bool op_time_ok = false;
	
	do
	{
		if(op_idx < 0 || op_idx >= system_status[SS_OP_COUNT].i || op_idx >= MAX_OPERATIONAL_POINTS)
		{
			goto_fault_state();	//TBD TODO add specific reporting in addition to transition
			return;
		}
		
		seconds = operational_points[op_idx][OP_REMAIN_TIME].f;
		if(seconds <= 0)
		{
			op_idx++;
			system_status[SS_OP_IDX].i += 1;
		}
		else
		{
			op_time_ok = true;
		}
	}
	while(!op_time_ok);
	//check remaining time for the current operational point
	//if time <= 0, move to the next point
	
	//Disable HV interlock
	enable_hv(false);
	
	// wait 20ms
	u32_t time_now = sys_now();
	u32_t time_set = time_now + 20;
	
	while(sys_now() < time_set){}
	//Disable EMISSION interlock
	enable_ecc(false);
	
	// wait 20ms
	time_now = sys_now();
	time_set = time_now + 20;
		
	while(sys_now() < time_set){}
	
	//Enable HV interlock
	enable_hv(true);
	
	//Set new timer values (pause first to allow timer change)
	pause_ext_timers();
	set_new_timer_value(seconds);
	deci_seconds_remaining = (int)((seconds * 10) + 0.5);	//Add 0.5 to round due to integer truncation
	system_status[SS_INTERNAL_TIMER_VAL].f = 0;
	//TBD TODO system_status[SS_I_TIMER_STATE].i = ;
	
	//Set filament to target current
	float heater_target = operational_points[op_idx][OP_FIL].f;
	set_hvps_heater(heater_target);
	
	//Set kV to target energy
	float kV = operational_points[op_idx][OP_KV].f;
	float mA_out = operational_points[op_idx][OP_MA].f;
	set_hvps_kv(kV, mA_out);
	
#if defined(CALIBRATION_MODE)
	//Set grid to 150V
	set_hvps_grid(150);
#else
	//Set grid to 200/400/500V
	if(kV <= 50)
	{
		set_hvps_grid(200);
	} 
	else if (kV <= 70)
	{
		set_hvps_grid(400);
	}
	else if (kV <= 100)
	{
		set_hvps_grid(500);
	}
	else 
	{
		set_hvps_grid(0);
	}
#endif
	
	float f_coil_current = (operational_points[op_idx][OP_F_COIL].f / 1000) * 1.666;	//TBD TODO clean up magic number
	set_coil_voltage(F_COIL_DAC_CH, f_coil_current);
	
	float coil_current = (operational_points[op_idx][OP_X_COIL].f / 1000) * 2.5;	//TBD TODO clean up magic number
	set_coil_voltage(X_COIL_DAC_CH, coil_current);

	coil_current = (operational_points[op_idx][OP_Y_COIL].f / 1000) * 2.5;	//TBD TODO clean up magic number
	set_coil_voltage(Y_COIL_DAC_CH, coil_current);
	
	//Set expected coil values
	expected_coil_value[EV_COIL_X_A] = operational_points[op_idx][OP_X_COIL].f;
	expected_coil_value[EV_COIL_Y_A] = operational_points[op_idx][OP_Y_COIL].f;
	expected_coil_value[EV_COIL_F_A] = operational_points[op_idx][OP_F_COIL].f;
	
	//Turn off indicators
	enable_indicators(false);
	
	set_led_sequence(LED_SEQ_SETUP);
	
	*state = STATE_SETUP;
}

static void run_setup_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STOP)
	{
		goto_termination_state();
	}
	else if(ev == EVENT_HVPS_SP_REACHED)
	{
		goto_ready_state();
	}
}

static void goto_ready_state()
{
	int op_idx = system_status[SS_OP_IDX].i;
	
	//Restart standby timer
	standby_deci_seconds = STANDBY_TICKS;
	
	//Pause timers in case we are stopping existing treatment (does not affect new treatment)
	pause_ext_timers();
	
	//Disable EMISSION interlock
	enable_ecc(false);
	
	//Queue start if auto-continue for point is set
	if(!wait_for_release_point && (operational_points[op_idx][OP_AUTO_EXEC].u != 0))
	{
		queue_sm_event(EVENT_PC_RELEASE_POINT);
	}

	set_led_sequence(LED_SEQ_READY);

	*state = STATE_READY;
}

static void run_ready_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STOP)
	{
		goto_termination_state();
	}
	else if(ev == EVENT_PC_STANDBY)
	{
		goto_discharge_state();
	}
	else if(ev == EVENT_PC_RELEASE_POINT)
	{
		//Check keys before proceeding
		bool ok_to_proceed = true;
		
		ok_to_proceed &= verify_keys_ok();
		ok_to_proceed &= verify_collimator_ok();
		ok_to_proceed &= verify_door_ok();
#if defined(CALIBRATION_MODE)
		ok_to_proceed &= verify_spare_interlock_2_ok();
#endif
		
		//Only proceed if interlocks are ok
		if(ok_to_proceed)
		{
			wait_for_release_point = false;	//For now, auto-proceed to all points after first
			goto_launching_state();
		}
		else
		{
			//Do not continue and report fault if system is not in an ok state
			report_typed_fault(FAULT_INVALID_CONFIG, "The loaded configuration is invalid.");
		}
	}
}

static void goto_launching_state()
{
	//Disable EMISSION interlock
	enable_ecc(false);
	
	*state = STATE_LAUNCHING;
}

static void run_launching_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STOP)
	{
		goto_termination_state();
	}
	else if(ev == EVENT_HVPS_SP_REACHED)
	{
		goto_emission_state();
	}
}

static void goto_emission_state()
{
	//Enable x-ray indicators
	enable_indicators(true);
	
	//Enable EMISSION interlock
	enable_ecc(true);

	//Start the external timers
	start_ext_timers();

	set_led_sequence(LED_SEQ_XRAY);
	
	//TBD TODO system_status[SS_I_TIMER_STATE].i = ;
	
	*state = STATE_EMISSION;
}

static void run_emission_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_PC_STOP)
	{
		goto_termination_state();
	}
	else if(ev == EVENT_OP_COMPLETE)
	{
		if(++system_status[SS_OP_IDX].i >= system_status[SS_OP_COUNT].i)
		{	
			goto_termination_state();
		}
		else
		{
			goto_setup_state();	
		}
	}
}

static void goto_termination_state()
{	
	//Disable HV interlock
	enable_hv(false);
	
	// wait 50ms
	u32_t time_now = sys_now();
	u32_t time_set = time_now + 50;
	
	while(sys_now() < time_set){}
	
	//Disable EMISSION interlock
	enable_ecc(false);
	
	//Pause timers
	pause_ext_timers();
	
	//Set kV to 0
	set_hvps_kv(0, 0);

	//Set filament to warmup value
	float htr_val = hvps_config[HVPS_CONF_WARMUP_I];
	set_hvps_heater(htr_val);

	//Set grid to 0
	set_hvps_grid(0);
	
	//Set coils to 0
	set_coil_voltage(X_COIL_DAC_CH, 0);
	set_coil_voltage(Y_COIL_DAC_CH, 0);
	set_coil_voltage(F_COIL_DAC_CH, 0);
	
	//Disable emission indicators
	enable_indicators(false);
	
	//TBD TODO system_status[SS_I_TIMER_STATE].i = ;
	
	*state = STATE_TERMINATION;
}

static void run_termination_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_HVPS_SP_REACHED)
	{
		goto_staged_state();
	}
}

static void goto_discharge_state()
{	
	//Disable HV interlock
	enable_hv(false);
	
	//Disable EMISSION interlock
	enable_ecc(false);
	
	//Set kV, heater and grid to 0
	set_hvps_kv(0, 0);
	set_hvps_heater(0);	
	set_hvps_grid(0);

	//Turn off coils
	set_coil_voltage(X_COIL_DAC_CH, 0);
	set_coil_voltage(Y_COIL_DAC_CH, 0);
	set_coil_voltage(F_COIL_DAC_CH, 0);
	
	*state = STATE_DISCHARGE;
}

static void run_discharge_state(EventType ev)
{
	if(ev == EVENT_FAULT)
	{
		goto_fault_state();
	}
	else if(ev == EVENT_HVPS_SP_REACHED)
	{
		goto_cold_state();
	}
}

static void goto_fault_state()
{
	standby_deci_seconds = FAULT_STANDBY_TICKS;
	
	//Disable HV interlock
	enable_hv(false);
	
	// wait 50ms
	u32_t time_now = sys_now();
	u32_t time_set = time_now + 50;
	
	while(sys_now() < time_set){}
	
	//Disable EMISSION interlock
	enable_ecc(false);
	
	//Pause timers
	pause_ext_timers();
	
	//Set kV to 0
	set_hvps_kv(0, 0);
	set_hvps_heater(0);
	set_hvps_grid(0);
	
	//Turn off coils
	set_coil_voltage(X_COIL_DAC_CH, 0);
	set_coil_voltage(Y_COIL_DAC_CH, 0);
	set_coil_voltage(F_COIL_DAC_CH, 0);	
	
	//Stop coolant pump and fan
	enable_pump(false);
	//Stop x-ray indicators
	enable_indicators(false);
	
	set_led_sequence(LED_SEQ_FAULT);

	//TBD TODO system_status[SS_I_TIMER_STATE].i = ;
	
	*state = STATE_FAULT;
}

static void run_fault_state(EventType ev)
{
	if(ev == EVENT_PC_CLEAR_FAULT)
	{
		clear_faults();
		//TODO: we can try to recover from a fault without going to cold based on the issue
		goto_cold_state();
	}
	else if(ev == EVENT_PC_STANDBY)
	{
		goto_cold_fault_state();
	}
	else if(ev == EVENT_PC_RESET_TIMERS)
	{
		clear_timers();
	}
	/*
	else if(ev == EVENT_CLEAR_PULSE_DONE)
	{
		if(system_status[SS_COMM_FAULTS].i == 0 && system_status[SS_FAULTS].i == 0)
		{
			goto_warmup_state();
		}
	}*/
}

static void clear_timers()
{
	clear_ext_timers();
	deci_seconds_remaining = 0;
	system_status[SS_INTERNAL_TIMER_VAL].f = 0;
}