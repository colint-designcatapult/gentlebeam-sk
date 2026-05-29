/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: System monitoring
*	Author: Carlton Chow
*	Description:
*/


#include <atmel_start.h>
#include "hvps.h"
#include "faults.h"
#include "state_machine.h"
#include "system_monitoring.h"
#include "system_parameters.h"
#include "sys_config_defaults.h"
#include "hvps_monitoring.h"


float hvps_expected_val[HVPS_EXPECTED_CNT];

float hvps_setpoints[HVPS_SP_CNT];

int32_t hvps_stability_timer = 0;
int32_t hvps_stability_counter = 0;
int32_t hvps_uncontrolled_counter = 0;
int32_t hvps_ma_thresh = 0;
int32_t hvps_kv_oot_counter = 0;

static void check_hvps_kv();
static void check_hvps_ma();
static void check_hvps_heater();
static void check_hvps_grid();
static void check_hvps_io();
static void check_hvps_stability();
static bool verify_hvps_setpoints();
static bool hvps_sys_stat_check(uint8_t bitpos);


void check_hvps_values()
{
	check_hvps_kv();
	check_hvps_ma();
	check_hvps_heater();
	check_hvps_grid();
	check_hvps_io();
	check_hvps_stability();
}

//!!!??? break into separate functions
static void check_hvps_stability()
{
	float htr_target = hvps_expected_val[HVPS_EXPECTED_FIL];
	float htr_output = system_status[SS_HEATER_SP].f;
	float htr_actual = system_status[SS_HEATER_FB].f;
	float htr_tolerance = DEFAULT_HTR_ITOL;
	
	float kv_target = hvps_expected_val[HVPS_EXPECTED_KV];
#if defined(CALIBRATION_MODE)
	//float kv_output = system_status[SS_KV_SP].f;
#else
	float kv_output = system_status[SS_KV_SP].f;
#endif
	float kv_actual = system_status[SS_KV_FB].f;
	float kv_tolerance = DEFAULT_KV_TOL;
	
	bool heater_stable = false;
	bool kv_stable = false;
	
	hvps_stability_timer++;
	
	switch(system_status[SS_STATE].i)
	{
		case STATE_CONDITIONING:
			heater_stable = tolerance_check_rel(htr_target, htr_actual, htr_tolerance);
			heater_stable &= (htr_actual > HEATER_MIN_FEEDBACK); //This check is because this FB is unreliable, can only use ballpark on/off
			heater_stable &= (!hvps_sys_stat_check(HVPS_SYS_STAT_WARMING));
			
			if(hvps_stability_timer > CONDITIONING_TIMEOUT)
			{
				report_verbose_fault(FAULT_FILAMENT, FIL_FAULT_RAMP_TIMEOUT, htr_target, CONDITIONING_TIMEOUT, htr_tolerance, htr_actual, (uint32_t)htr_output);
			}
			//Wait for heater to do the initial step ramp
			else if(hvps_stability_timer >= WARMUP_RUNNING_CHECK)
			{
				//If heater is not on after initial ramp period throw a fault
				if(htr_actual < HEATER_MIN_FEEDBACK)
				{
					report_fault(FAULT_FILAMENT, FIL_FAULT_STARTUP, HEATER_MIN_FEEDBACK, 0, htr_actual);
				}
				else
				{
					//If heater is no longer warming and close enough to setpoint, report system stable
					if(heater_stable)
					{
						//System must be stable for a set period before proceeding
						if(++hvps_stability_counter > WARMUP_STABILITY_CHECK)
						{
							queue_sm_event(EVENT_HVPS_SP_REACHED);
							hvps_stability_counter = 0;
							hvps_stability_timer = 0;
						}
					}
					else
					{
						hvps_stability_counter = 0;
					}
				}
			}
			break;
		case STATE_WARMUP:
			heater_stable = tolerance_check_rel(htr_target, htr_actual, htr_tolerance);
			heater_stable &= (htr_actual > HEATER_MIN_FEEDBACK); //This check is because this FB is unreliable, can only use ballpark on/off
			heater_stable &= (!hvps_sys_stat_check(HVPS_SYS_STAT_WARMING));
			
			if(hvps_stability_timer > WARMUP_TIMEOUT)
			{
				report_verbose_fault(FAULT_FILAMENT, FIL_FAULT_RAMP_TIMEOUT, htr_target, WARMUP_TIMEOUT, htr_tolerance, htr_actual, (uint32_t)htr_output);
			}
			//Wait for heater to do the initial step ramp
			else if(hvps_stability_timer >= WARMUP_RUNNING_CHECK)
			{
				//If heater is not on after initial ramp period throw a fault
				if(htr_actual < HEATER_MIN_FEEDBACK)
				{
					report_fault(FAULT_FILAMENT, FIL_FAULT_STARTUP, HEATER_MIN_FEEDBACK, 0, htr_actual);
				}
				else
				{
					//If heater is no longer warming and close enough to setpoint, report system stable
					if(heater_stable)
					{
						//System must be stable for a set period before proceeding
						if(++hvps_stability_counter > WARMUP_STABILITY_CHECK)
						{
							queue_sm_event(EVENT_HVPS_SP_REACHED);
							hvps_stability_counter = 0;
							hvps_stability_timer = 0;
						}
					}
					else
					{
						hvps_stability_counter = 0;
					}
				}
			}
			break;
		case STATE_SETUP:
			//Check to see if both heater and kv are at their target
			heater_stable = tolerance_check_rel(htr_target, htr_actual, htr_tolerance);
			heater_stable &= (!hvps_sys_stat_check(HVPS_SYS_STAT_WARMING));
			
			kv_stable = tolerance_check_rel(kv_target, kv_actual, kv_tolerance);
			kv_stable &= (!hvps_sys_stat_check(HVPS_SYS_STAT_KV_RAMPING));
			
			//Make sure we don't timeout trying to reach target
			if(hvps_stability_timer > SETUP_TIMEOUT)
			{
				if(!heater_stable)
				{
					report_verbose_fault(FAULT_FILAMENT, FIL_FAULT_RAMP_TIMEOUT, htr_target, SETUP_TIMEOUT, htr_tolerance, htr_actual, (uint32_t)htr_output);
				}
				if(!kv_stable)
				{
#if defined(CALIBRATION_MODE)
					report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, SETUP_TIMEOUT, kv_tolerance, kv_actual, 0);
					//report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, SETUP_TIMEOUT, kv_tolerance, kv_actual, (uint32_t)kv_output);
#else
					report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, SETUP_TIMEOUT, kv_tolerance, kv_actual, (uint32_t)kv_output);
#endif
				}
			}
			else if(heater_stable && kv_stable)
			{
				//System must be stable for a set period before proceeding
				if(++hvps_stability_counter > SETUP_STABILITY_CHECK)
				{
					if(verify_hvps_setpoints())
					{
						queue_sm_event(EVENT_HVPS_SP_REACHED);
						hvps_stability_counter = 0;	
						hvps_uncontrolled_counter = 0;
					}
					else
					{
						//TBD TODO report fault for HVPS comm issue, non matching values
						//Note that eventually timeout will be reached if no fault reported here
					}					
				}
			}
			else
			{
				hvps_stability_counter = 0;
			}
			break;
		case STATE_LAUNCHING:
			//Check to see if both heater and kv are at their target
			heater_stable = tolerance_check_rel(htr_target, htr_actual, htr_tolerance);
			heater_stable &= (!hvps_sys_stat_check(HVPS_SYS_STAT_WARMING));
			
			//Make sure we don't timeout trying to reach target
			if(hvps_stability_timer > LAUNCHING_TIMEOUT)
			{
				report_verbose_fault(FAULT_FILAMENT, FIL_FAULT_RAMP_TIMEOUT, htr_target, LAUNCHING_TIMEOUT, htr_tolerance, htr_actual, (uint32_t)htr_output);
			}
			else if(heater_stable)
			{
				//System must be stable for a set period before proceeding
				if(++hvps_stability_counter > LAUNCHING_STABILITY_CHECK)
				{
					queue_sm_event(EVENT_HVPS_SP_REACHED);
					hvps_stability_counter = 0;
					hvps_uncontrolled_counter = 0;
				}
			}
			else
			{
				hvps_stability_counter = 0;
			}
			break;
		case STATE_DISCHARGE:
			//TBD TODO add check to see that warmup bit is not set maybe???
			//TBD TODO && (htr state == stable) maybe??
			heater_stable = tolerance_check_rel(htr_target, htr_actual, htr_tolerance);
			kv_stable = tolerance_check_abs(kv_target, kv_actual, kv_tolerance);
			
			if(hvps_stability_timer > DISCHARGE_TIMEOUT)
			{
				if(!heater_stable)
				{
					report_verbose_fault(FAULT_FILAMENT, FIL_FAULT_RAMP_TIMEOUT, htr_target, DISCHARGE_TIMEOUT, htr_tolerance, htr_actual, (uint32_t)htr_output);
				}
				if(!kv_stable)
				{
#if defined(CALIBRATION_MODE)
					report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, DISCHARGE_TIMEOUT, kv_tolerance, kv_actual, 0);
					//report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, DISCHARGE_TIMEOUT, kv_tolerance, kv_actual, (uint32_t)kv_output);
#else
					report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, DISCHARGE_TIMEOUT, kv_tolerance, kv_actual, (uint32_t)kv_output);
#endif
				}
			}
			else if(kv_stable && heater_stable)
			{
				queue_sm_event(EVENT_HVPS_SP_REACHED);
			}
			break;
		case STATE_TERMINATION:
			heater_stable = tolerance_check_rel(htr_target, htr_actual, htr_tolerance);
			kv_stable = tolerance_check_abs(kv_target, kv_actual, kv_tolerance);
			
			if(hvps_stability_timer > TERMINATION_TIMEOUT)
			{
				if(!heater_stable)
				{
					report_verbose_fault(FAULT_FILAMENT, FIL_FAULT_RAMP_TIMEOUT, htr_target, TERMINATION_TIMEOUT, htr_tolerance, htr_actual, (uint32_t)htr_output);
				}
				if(!kv_stable)
				{
#if defined(CALIBRATION_MODE)
					report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, DISCHARGE_TIMEOUT, kv_tolerance, kv_actual, 0);
					//report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, DISCHARGE_TIMEOUT, kv_tolerance, kv_actual, (uint32_t)kv_output);
#else
					report_verbose_fault(FAULT_KV, KV_FAULT_RAMP_TIMEOUT, kv_target, TERMINATION_TIMEOUT, kv_tolerance, kv_actual, (uint32_t)kv_output);
#endif
				}
			}
			else if(kv_stable && heater_stable)
			{
				if(++hvps_stability_counter > TERM_STABILITY_CHECK)
				{
					queue_sm_event(EVENT_HVPS_SP_REACHED);
					hvps_stability_counter = 0;
				}
			}
			else
			{
				hvps_stability_counter = 0;
			}
			break;
		default:
			//Reset stability checks
			hvps_stability_timer = 0;
			hvps_stability_counter = 0;
			break;
	}
}

static void check_hvps_kv()
{
	float target = hvps_expected_val[HVPS_EXPECTED_KV];
	float actual = system_status[SS_KV_FB].f;
	float tolerance = DEFAULT_KV_TOL;
		
	switch(system_status[SS_STATE].i)
	{

		case STATE_READY:
		case STATE_EMISSION:
			//TBD TODO add counter for consecutive errors before reporting
			if(!tolerance_check_rel(target, actual, tolerance))
			{
				hvps_kv_oot_counter++;
				if(hvps_kv_oot_counter > 20)	// TBD TODO magic number
				{
					report_fault(FAULT_KV, KV_FAULT_OOT, target, tolerance, actual);
				}
			}
			else
			{
				hvps_kv_oot_counter = 0;
			}
			break;
		case STATE_DISCHARGE:
		case STATE_TERMINATION:
		case STATE_SETUP:
		case STATE_LAUNCHING:
			//Do nothing here, defer to stability monitoring here
			break;
		default:
			if(actual >= LOW_KV_THRESH /*|| kV present IO from HVPS is active*/)
			{
				report_verbose_fault(FAULT_KV, KV_FAULT_UNWANTED_HV, LOW_KV_THRESH, DISCHARGE_TIMEOUT, 0, actual, -1);
			}
			break;
	}
}

static void check_hvps_ma()
{
	float target = hvps_expected_val[HVPS_EXPECTED_MA];
	float actual = system_status[SS_MA_FB].f;
	float tolerance = DEFAULT_MA_TOL;
	
	if(system_status[SS_STATE].i == STATE_EMISSION)
	{
		if(!tolerance_check_rel(target, actual, tolerance))
		{
			hvps_uncontrolled_counter++;
			if(hvps_uncontrolled_counter > MA_UNSTABLE_TIME)
			{
				report_verbose_fault(FAULT_MA, MA_FAULT_UNSTABLE, target, MA_UNSTABLE_TIME, tolerance, actual, -1);
				hvps_uncontrolled_counter = 0;
			}
		}
		else
		{
			hvps_uncontrolled_counter = 0;
		}
	}
	else if(system_status[SS_STATE].i == STATE_SETUP)
	{
		if(actual > DEFAULT_MA_THRESH)
		{
			hvps_ma_thresh++;
			if(hvps_ma_thresh > MA_UNWANTED_TIME)
			{
				report_verbose_fault(FAULT_GRID, MA_GRID_FAULT_UNDESIRED, target, MA_UNWANTED_TIME, tolerance, actual, -1);
			}
		}
		else
		{
			hvps_ma_thresh = 0;
		}
	}
	else if(system_status[SS_STATE].i == STATE_READY)
	{
		if(actual > DEFAULT_MA_THRESH)
		{
			hvps_ma_thresh++;
			if(hvps_ma_thresh > MA_UNWANTED_TIME)
			{
				report_verbose_fault(FAULT_GRID, MA_GRID_FAULT_UNDESIRED, target, MA_UNWANTED_TIME, tolerance, actual, -1);
			}
		}
	}
	else
	{
		hvps_ma_thresh = 0;
	}
}

static void check_hvps_heater()
{
	float heater_max = HTR_OVERCUR_THRESH;
	
	//Ensure heater current is below maximum
	if(system_status[SS_HEATER_FB].f > heater_max)
	{
		set_hvps_heater(0);
		set_hvps_grid(0);
		report_fault(FAULT_FILAMENT, FIL_FAULT_OVERCURRENT_FB, HTR_OVERCUR_THRESH, 0, system_status[SS_HEATER_FB].f);
	}
	else if(system_status[SS_HEATER_SP].f > heater_max)
	{
		set_hvps_heater(0);
		set_hvps_grid(0);
		report_fault(FAULT_FILAMENT, FIL_FAULT_OVERCURRENT_SP, HTR_OVERCUR_THRESH, 0, system_status[SS_HEATER_FB].f);
	}
}

static void check_hvps_grid()
{	
	//TBD TODO
	//No longer have reporting of existing voltage on grid, only of supply for emission grid voltage
}

static void check_hvps_io()
{
	//TBD TODO
	/*
	If HVPS fault IO triggered and we are in setup, ready, beam on, control or termination, report fault
	*/
}

static bool verify_hvps_setpoints()
{
	bool hvps_sp_ok = true;
	
	hvps_sp_ok &= tolerance_check_rel(hvps_expected_val[HVPS_EXPECTED_KV], hvps_status[HVPS_STATUS_KV_SP].f, 1);
	//hvps_sp_ok &= tolerance_check_rel(hvps_ev[HVPS_EV_FIL], hvps_status[HVPS_STATUS_FIL_SP].f, 1);
	
	return hvps_sp_ok;
}

static bool hvps_sys_stat_check(uint8_t bitpos)
{
	if(bitpos >= NUM_HVPS_SYS_STAT_BITS)
	{
		return false;
	}
	if((1<<bitpos) & system_status[SS_HVPS_FLAG_STATUS].u)
	{
		return true;
	}
	return false;
}
