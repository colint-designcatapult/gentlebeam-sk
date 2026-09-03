/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: PC message processing
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include <math.h>
#include "ext_dac.h"
#include "faults.h"
#include "head_board.h"
#include "hvps.h"
#include "hvps_monitoring.h"
#include "pc_msg_processing.h"
#include "state_machine.h"
#include "sys_config_defaults.h"
#include "system_monitoring.h"
#include "system_parameters.h"

uint32_t network_config[NETWORK_CMD_COUNT];

//Array of pointers for responses
void *return_data_pointers[PACKET_TYPE_COUNT];

/*Response packets
Note: The actual system status, device information, calibration point,
and operational point values are not included here but are instead
handled in the system parameters module*/
uint32_t invalid_packet[INVALID_COUNT];
uint32_t directive_response[DIR_RES_COUNT];
VariableValue fault_request_response[FAULT_RES_COUNT];
SetpointResult condition_cmd_response[CONDITION_CMD_COUNT];
SetpointResult warmup_cmd_response[WARMUP_CMD_COUNT];
uint32_t new_session_response[NEW_SES_RES_COUNT];
SetpointResult op_load_results[OP_CMD_COUNT];
SetpointResult op_confirm_results[OP_CMD_COUNT];
SetpointResult release_cmd_response[RELEASE_CMD_COUNT];

VariableValue op_query_response[OP_RES_COUNT];

uint32_t session_id = 0;

//Array of functions for responses
void (*comm_processing_func[PACKET_TYPE_COUNT]) (uint32_t *data);

//Functions for processing incoming commands
static void default_message_processing(uint32_t *data);

static void process_fault_request(uint32_t *data);
static void process_pc_directive_command(uint32_t *data);
static void process_condition_command(uint32_t *data);
static void process_warmup_command(uint32_t *data);
static void process_new_session_command(uint32_t *data);
static void process_op_load_command(uint32_t *data);
static void process_op_confirm_command(uint32_t *data);
static void process_op_query(uint32_t *data);
static void process_release_command(uint32_t *data);
#if !defined(CALIBRATION_MODE)
static void process_qc_command(uint32_t *data);
#endif

static bool cmd_auth_check(uint32_t *data, uint32_t packet_type, uint32_t auth_idx);
static uint32_t op_val_check(uint32_t *data, int idx, float min, float max);
static uint32_t match_op_confirm(uint32_t *data, uint32_t data_idx);

static void check_plan_for_release();
static void check_point_for_release();

#if defined(CALIBRATION_MODE)
//Variables and Functions for processing incoming calibration commands
SetpointResult cal_coil_response[CAL_COIL_CMD_COUNT];
SetpointResult cal_hvps_cmd_response[CAL_HVPS_CMD_COUNT];
SetpointResult cal_directive_response[CAL_DIRECTIVE_CMD_COUNT];
float cal_setpoint_req_response[CAL_SP_RES_COUNT];
VariableValue cal_mag_response[HB_NUM_MAG_CAL];

static void process_start_directive();
static void process_stop_directive();

static void process_cal_coil_command(uint32_t *data);
static void process_cal_hvps_command(uint32_t *data);
static void process_cal_directive_command(uint32_t *data);
static void process_cal_setpoint_request(uint32_t *data);
static void process_cal_mag_command(uint32_t *data);
#endif

void init_response_pointers()
{
	//Set all return pointers to invalid by default first
	//Safety check to not return null pointer
	for(int i = 0; i < PACKET_TYPE_COUNT; i++)
	{
		return_data_pointers[i] = (void *)invalid_packet;
		comm_processing_func[i] = default_message_processing;
	}
	
	//comm_processing_func[] = ; //no additional processing needed for now
	
	//Initialize pointers for response values and function execution
	return_data_pointers[PCCOM_VERSION_REQUEST] = (void *)&device_information;
	//comm_processing_func[PCCOM_VERSION_REQUEST] = ; //no additional processing needed for now
	
	return_data_pointers[PCCOM_FAULT_REQUEST] = (void *)fault_request_response;
	comm_processing_func[PCCOM_FAULT_REQUEST] = process_fault_request;
	
	return_data_pointers[PCCOM_DIRECTIVE_CMD] = (void *)directive_response;
	comm_processing_func[PCCOM_DIRECTIVE_CMD] = process_pc_directive_command;
	
	return_data_pointers[PCCOM_TELEMETERY_REQUEST] = (void *)system_status;
	//comm_processing_func[PCCOM_TELEMETRY_REQUEST] = ; //no additional processing needed for now
	
	return_data_pointers[PCCOM_CONDITION_CMD] = (void *)condition_cmd_response;
	comm_processing_func[PCCOM_CONDITION_CMD] = process_condition_command;
	
	return_data_pointers[PCCOM_WARMUP_CMD] = (void *)warmup_cmd_response;
	comm_processing_func[PCCOM_WARMUP_CMD] = process_warmup_command;
	
	return_data_pointers[PCCOM_NEW_SESSION] = (void *)new_session_response;
	comm_processing_func[PCCOM_NEW_SESSION] = process_new_session_command;
	
	return_data_pointers[PCCOM_LOAD_OP] = (void *)op_load_results;
	comm_processing_func[PCCOM_LOAD_OP] = process_op_load_command;
	
	return_data_pointers[PCCOM_CONFIRM_OP] = (void *)op_confirm_results;
	comm_processing_func[PCCOM_CONFIRM_OP] = process_op_confirm_command;
	
	//Operational point query always returns the staged point.
	return_data_pointers[PCCOM_QUERY_OP] = (void *)op_query_response;
	comm_processing_func[PCCOM_QUERY_OP] = process_op_query;
	
	return_data_pointers[PCCOM_TREATMENT_RELEASE] = (void *)release_cmd_response;
	comm_processing_func[PCCOM_TREATMENT_RELEASE] = process_release_command;
	
#if defined(CALIBRATION_MODE)
	//Return data pointer for calibration
	return_data_pointers[CAL_COIL_CMD] = (void *)cal_coil_response;
	comm_processing_func[CAL_COIL_CMD] = process_cal_coil_command;
	
	return_data_pointers[CAL_HVPS_CMD] = (void *)cal_hvps_cmd_response;
	comm_processing_func[CAL_HVPS_CMD] = process_cal_hvps_command;

	return_data_pointers[CAL_DIRECTIVE_CMD] = (void *)cal_directive_response;
	comm_processing_func[CAL_DIRECTIVE_CMD] = process_cal_directive_command;

	return_data_pointers[CAL_SP_REQ_CMD] = (void *)cal_setpoint_req_response;
	comm_processing_func[CAL_SP_REQ_CMD] = process_cal_setpoint_request;

	return_data_pointers[CAL_MAG_REQ_CMD] = (void *)cal_mag_response;
	comm_processing_func[CAL_MAG_REQ_CMD] = process_cal_mag_command;
#else
	return_data_pointers[PCCOM_QC_PING] = (void *)qc_ping_buf;
	//comm_processing_func[PCCOM_QC_PING] = ; //no additional processing needed for now
	
	return_data_pointers[PCCOM_QC_READING] = (void *)qc_reported;
	comm_processing_func[PCCOM_QC_READING] = process_qc_command;

	/*
	return_data_pointers[CAL_COIL_CMD] = (void *)cal_coil_response;
	return_data_pointers[CAL_HVPS_CMD] = (void *)cal_hvps_cmd_response;
	return_data_pointers[CAL_DIRECTIVE_CMD] = (void *)cal_directive_response;
	return_data_pointers[CAL_SP_REQ_CMD] = (void *)cal_setpoint_req_response;
	return_data_pointers[CAL_MAG_REQ_CMD] = (void *)mag_cal_array;*/
#endif
}

void *get_response_data(PacketType_t ptype)
{
	if(ptype < PCCOM_INVALID_PACKET || ptype >= PACKET_TYPE_COUNT)
	{
		ptype = PCCOM_INVALID_PACKET;
	}
	return return_data_pointers[(int)ptype];
}

void process_command(PacketType_t ptype, void* data)
{
	//Based on the given command packet type, call the function to execute
	//Use the command type as the index to the array of function pointers
	if(ptype > PCCOM_INVALID_PACKET && ptype < PACKET_TYPE_COUNT)
	{
		(*comm_processing_func[ptype])(data);
		reset_pc_comm_timeout();
	}
}

static void default_message_processing(uint32_t *data)
{
	//Do nothing by default
}

static void process_fault_request(uint32_t *data)
{
	serialize_fault_response(data[FAULT_REQ_INDEX], fault_request_response);
}

//TBD TODO keep switch statements but add static void calls to sub-functions
static void process_pc_directive_command(uint32_t *data)
{	
	directive_response[DIR_RES_STATUS] = SPR_OK;
	
	//Make sure confirmation value matches the given directive
	uint32_t bit_match = 1;
	bit_match <<= data[DIR_CMD_ID];
	
	bool wipe_state_ok = (system_status[SS_STATE].u == STATE_PRIMED || system_status[SS_STATE].u == STATE_STAGING ||
		system_status[SS_STATE].u == STATE_STAGED || system_status[SS_STATE].u == STATE_COLD);
		
	bool timer_reset_state_ok = (system_status[SS_STATE].u == STATE_PRIMED || system_status[SS_STATE].u == STATE_STAGED ||
		system_status[SS_STATE].u == STATE_COLD || system_status[SS_STATE].u == STATE_FAULT || system_status[SS_STATE].u == STATE_COLD_FAULT);
		
	bool standby_state_ok = (system_status[SS_STATE].i == STATE_PRIMED || system_status[SS_STATE].i == STATE_READY ||
		system_status[SS_STATE].u == STATE_STAGING || system_status[SS_STATE].u == STATE_STAGED);
	
	if(bit_match != data[DIR_CMD_CONFIRM])
	{
		directive_response[DIR_RES_STATUS] = SPR_INVALID;
		return;
	}	
	
	//Process the given directive
	switch(data[DIR_CMD_ID])
	{
		case PC_DIR_STARTUP_INIT:
			if(system_status[SS_STATE].u == STATE_STARTUP)
			{
				queue_sm_event(EVENT_STARTUP_INIT);
			}
			else
			{
				directive_response[DIR_RES_STATUS] = SPR_ACCESS_ERROR;
			}
			break;
		case PC_DIR_STAGE_PLAN:
			//Ensure we are in the staging state
			if(system_status[SS_STATE].u != STATE_STAGING)
			{
				directive_response[DIR_RES_STATUS] = SPR_ACCESS_ERROR;
			}
			//Ensure the operational point has been loaded.
			else if(plan_info[PLAN_LOADED_BOOL] == 0)
			{
				directive_response[DIR_RES_STATUS] = SPR_INVALID;
			}
			else
			{
				//Queue event to finish staging
				queue_sm_event(EVENT_PC_FINISH_STAGE);
			}			
			break;
		case PC_DIR_STOP:
			//No state check, stop is always allowed
			//Notify state machine of stop
			queue_sm_event(EVENT_PC_STOP);
			break;
		case PC_DIR_CLEAR_FAULTS:
			//Queue a clear fault request
			queue_sm_event(EVENT_PC_CLEAR_FAULT);
			//Request HVPS to clear faults
			queue_hvps_cmd(HVPS_CMD_CLEAR_FAULTS, 0, 0);
			break;
		case PC_DIR_WIPE_PLAN:
			//Check that state for wiping plan is valid
			if(wipe_state_ok)
			{
				//Queue a wipe plan request
				queue_sm_event(EVENT_PC_WIPE_PLAN);	
			}
			else
			{
				directive_response[DIR_RES_STATUS] = SPR_ACCESS_ERROR;
			}
			break;
		case PC_DIR_RESET_TIMERS:
			//Check that state for timer reset is valid
			if(timer_reset_state_ok)
			{
				//Queue a timer reset request
				queue_sm_event(EVENT_PC_RESET_TIMERS);	
			}
			else
			{
				directive_response[DIR_RES_STATUS] = SPR_ACCESS_ERROR;
			}
			break;
		case PC_DIR_STANDBY:
			//Ensure we are in a valid state to standby
			if(standby_state_ok)
			{
				queue_sm_event(EVENT_PC_STANDBY);
			}
			else
			{
				directive_response[DIR_RES_STATUS] = SPR_ACCESS_ERROR;
			}
			break;
		default:
			directive_response[DIR_RES_STATUS] = SPR_OOB;
			break;
	}
}

static void process_condition_command(uint32_t *data)
{
	//Convert raw value to float
	uint32_t raw_val = *data;
	float *new_condition_val;
	new_condition_val = &raw_val;
	
	//Ensure we are in a valid state to condition
	if(system_status[SS_STATE].i != STATE_COLD)
	{
		condition_cmd_response[CONDITION_CMD_FIL] = SPR_ACCESS_ERROR;
	}
	//Check that value is valid
	else if(isnan(*new_condition_val))
	{
		condition_cmd_response[CONDITION_CMD_FIL] = SPR_INVALID;
	}
	//Check if value is within range
	else if(*new_condition_val < DEFAULT_MIN_HTR_I || *new_condition_val > MAX_CONDITION_CURRENT)
	{
		condition_cmd_response[CONDITION_CMD_FIL] = SPR_OOB;
	}
	else
	{
		//Save new condition heater current
		hvps_config[HVPS_CONF_CONDITION_I] = *new_condition_val;
		
		//Indicate value accepted
		condition_cmd_response[CONDITION_CMD_FIL] = SPR_OK;
		
		//Notify state machine of condition request
		queue_sm_event(EVENT_PC_CONDITION);
	}
}

static void process_warmup_command(uint32_t *data)
{
	//Convert raw value to float
	uint32_t raw_val = *data;
	float *new_wu_val;
	new_wu_val = &raw_val;
	
	//Ensure we are in a valid state to warmup
	if(system_status[SS_STATE].i != STATE_COLD)
	{
		warmup_cmd_response[WARMUP_CMD_FIL] = SPR_ACCESS_ERROR;
	}
	//The state transition also requires both E-stops to be released. Reject
	//the command here instead of acknowledging a transition that cannot occur.
	else if(!verify_estops_ok())
	{
		warmup_cmd_response[WARMUP_CMD_FIL] = SPR_ACCESS_ERROR;
	}
	//Check that value is valid
	else if(isnan(*new_wu_val))
	{
		warmup_cmd_response[WARMUP_CMD_FIL] = SPR_INVALID;
	}
	//Check if value is within range
	else if(*new_wu_val < DEFAULT_MIN_HTR_I || *new_wu_val > MAX_WARMUP_CURRENT)
	{
		warmup_cmd_response[WARMUP_CMD_FIL] = SPR_OOB;
	}
	else
	{
		//Save new warmup heater current
		hvps_config[HVPS_CONF_WARMUP_I] = *new_wu_val;
		
		//Indicate value accepted
		warmup_cmd_response[WARMUP_CMD_FIL] = SPR_OK;
		
		//Notify state machine of condition request
		queue_sm_event(EVENT_PC_WARMUP);
	}
}

static void process_new_session_command(uint32_t *data)
{
	(void)data;
	memset(new_session_response, 0, sizeof(new_session_response));

	//Ensure we are in a valid state to start a new session
	if(system_status[SS_STATE].i != STATE_PRIMED)
	{
		new_session_response[NEW_SES_RES_STATUS] = SPR_ACCESS_ERROR;
		return;
	}

	//Generate new session key
	session_id = system_status[SS_SYS_RUNTIME].u;	//TBD TODO can update to use RNG instead of runtime val

	//Notify PC
	new_session_response[NEW_SES_RES_STATUS] = SPR_OK;
	new_session_response[NEW_SES_RES_ID] = session_id;

	//Notify state machine
	queue_sm_event(EVENT_PC_NEW_SESSION);
}

static bool cmd_auth_check(uint32_t *data, uint32_t packet_type, uint32_t auth_idx)
{
	//Check to ensure auth index is not overly large
	if(auth_idx > PC_MAX_AUTH_INDEX)
	{
		return false;
	}
	
	//TBD TODO replace this with actual AES MAC check
	uint32_t target_auth = session_id + AUTH_SECRET_KEY + packet_type;
	for(int i = 0; i < auth_idx; i++)
	{
		target_auth += data[i];
	}
	if(target_auth != data[auth_idx])
	{
		return false;
	}
	return true;
}

static uint32_t op_val_check(uint32_t *data, int idx, float min, float max)
{
	if(idx >= OP_CMD_COUNT)
	{
		return SPR_INVALID;
	}
	
	//Check that value is a number
	float *target = (float *)(data+idx);
	if(isnan(*target) || isinf(*target))
	{
		return SPR_INVALID;
	}
	//Check that value is within bounds
	else if(*target < min || *target > max)
	{
		return SPR_OOB;
	}
	return SPR_OK;
}

static void process_op_load_command(uint32_t *data)
{
	uint32_t load_packet_ok = SPR_OK;
	memset(op_load_results, 0, sizeof(op_load_results));

	//Ensure we are in a valid state to load the point
	if(system_status[SS_STATE].i != STATE_STAGING)
	{
		op_load_results[OP_CMD_TOTAL_TIME] = SPR_ACCESS_ERROR;
	}
	//Check that the message is authentic
	else if(!cmd_auth_check(data, PCCOM_LOAD_OP, OP_CMD_AUTHENTICATION))
	{
		op_load_results[OP_CMD_AUTHENTICATION] = SPR_INVALID;
	}
	//Check that given values are ok
	else
	{
		op_load_results[OP_CMD_AUTHENTICATION] = SPR_OK;

		op_load_results[OP_CMD_TOTAL_TIME] = op_val_check(data, OP_CMD_TOTAL_TIME, 0, MAX_OP_TIME);
		op_load_results[OP_CMD_REMAIN_TIME] = op_val_check(data, OP_CMD_REMAIN_TIME, 0, MAX_OP_TIME);
		op_load_results[OP_CMD_KV] = op_val_check(data, OP_CMD_KV, 0, MAX_OP_KV);
		op_load_results[OP_CMD_MA] = op_val_check(data, OP_CMD_MA, 0, MAX_OP_MA);
		op_load_results[OP_CMD_FIL] = op_val_check(data, OP_CMD_FIL, 0, MAX_OP_HEATER);
		op_load_results[OP_CMD_X_COIL] = op_val_check(data, OP_CMD_X_COIL, MIN_OP_DEFL_COIL, MAX_OP_DEFL_COIL);
		op_load_results[OP_CMD_Y_COIL] = op_val_check(data, OP_CMD_Y_COIL, MIN_OP_DEFL_COIL, MAX_OP_DEFL_COIL);
		op_load_results[OP_CMD_F_COIL] = op_val_check(data, OP_CMD_F_COIL, 0, MAX_OP_F_COIL);
	}

	for(int i = 0; i < OP_CMD_COUNT; i++)
	{
		load_packet_ok += op_load_results[i];
	}

	if(load_packet_ok == SPR_OK)
	{
		memcpy(operational_point, data, sizeof(uint32_t) * OP_PARAM_COUNT);
		plan_info[PLAN_LOADED_BOOL] = 1;
	}
}

static uint32_t match_op_confirm(uint32_t *data, uint32_t data_idx)
{
	if(data_idx >= OP_PARAM_COUNT)
	{
		return SPR_INVALID;
	}

	float *target = (float *)(data + data_idx);

	if(*target != operational_point[data_idx].f)
	{
		return SPR_INVALID;
	}

	return SPR_OK;
}

static void process_op_confirm_command(uint32_t *data)
{
	uint32_t confirm_packet_ok = SPR_OK;
	memset(op_confirm_results, 0, sizeof(op_confirm_results));

	//Ensure we are in a valid state to confirm the point
	if(system_status[SS_STATE].i != STATE_STAGED)
	{
		op_confirm_results[OP_CMD_TOTAL_TIME] = SPR_ACCESS_ERROR;
	}
	//Check that the message is authentic
	else if(!cmd_auth_check(data, PCCOM_CONFIRM_OP, OP_CMD_AUTHENTICATION))
	{
		op_confirm_results[OP_CMD_AUTHENTICATION] = SPR_INVALID;
	}
	//Check that given values match what has been loaded
	else
	{
		op_confirm_results[OP_CMD_AUTHENTICATION] = SPR_OK;

		for(int i = OP_CMD_TOTAL_TIME; i < OP_CMD_AUTHENTICATION; i++)
		{
			op_confirm_results[i] = match_op_confirm(data, i);
		}
	}

	for(int i = 0; i < OP_CMD_COUNT; i++)
	{
		confirm_packet_ok += op_confirm_results[i];
	}

	if(confirm_packet_ok == SPR_OK)
	{
		plan_info[PLAN_CONFIRMED_BOOL] = 1;
	}
}

static void process_op_query(uint32_t *data)
{
	(void)data;
	op_query_response[OP_RES_STATUS].u = SPR_OK;
	for(int i = 0; i < OP_PARAM_COUNT; i++)
	{
		op_query_response[i + 1].u = operational_point[i].u;
	}
}

static void check_plan_for_release()
{
	//Check that we are in the staged state
	if(system_status[SS_STATE].u != STATE_STAGED)
	{
		release_cmd_response[RELEASE_CMD_SCOPE] = SPR_ACCESS_ERROR;
	}
	else if(plan_info[PLAN_CONFIRMED_BOOL] == 0)
	{
		release_cmd_response[RELEASE_CMD_SCOPE] = SPR_INVALID;
	}
	else
	{
		release_cmd_response[RELEASE_CMD_SCOPE] = SPR_OK;
		queue_sm_event(EVENT_PC_RELEASE_PLAN);
	}	
}

static void check_point_for_release()
{
	if(system_status[SS_STATE].u != STATE_READY)
	{
		release_cmd_response[RELEASE_CMD_SCOPE] = SPR_ACCESS_ERROR;
	}
	else
	{
		release_cmd_response[RELEASE_CMD_SCOPE] = SPR_OK;
		queue_sm_event(EVENT_PC_RELEASE_POINT);
	}
}


static void process_release_command(uint32_t *data)
{
	release_cmd_response[RELEASE_CMD_SCOPE] = SPR_OK;
	release_cmd_response[RELEASE_CMD_AUTH] = SPR_OK;
	
	//Check authenticity
	if(!cmd_auth_check(data, PCCOM_TREATMENT_RELEASE, RELEASE_CMD_AUTH))
	{
		release_cmd_response[RELEASE_CMD_AUTH] = SPR_INVALID;
	}
	//If command is to release plan
	else if(data[RELEASE_CMD_SCOPE] == RELEASE_TYPE_PLAN)
	{
		check_plan_for_release();
	}
	//If command is to release point
	else if(data[RELEASE_CMD_SCOPE] == RELEASE_TYPE_POINT)
	{
		check_point_for_release();
	}
	else
	{
		release_cmd_response[RELEASE_CMD_SCOPE] = SPR_OOB;	
	}
}

#if defined(CALIBRATION_MODE)
// Calibration
static void process_start_directive()
{
	//check if all calibration interlocks are ready
	if (can_calibrate())
	{
		enable_indicators(true);
		queue_hvps_cmd(HVPS_CMD_CAL_START, 0, 0);
	}
}

static void process_stop_directive()
{
	enable_indicators(false);
	queue_hvps_cmd(HVPS_CMD_CAL_STOP, 0, 0);
}

static bool coil_float_check(uint32_t *data, float min, float max, int idx)
{
	if(idx >= CAL_COIL_CMD_COUNT)
	{
		return false;
	}
	float *target = (float *)(data+idx);
	if(isnan(*target) || isinf(*target))
	{
		cal_coil_response[idx] = SPR_INVALID;
		return false;
	}
	else if(*target < min || *target > max)
	{
		cal_coil_response[idx] = SPR_OOB;
		return false;
	}
	return true;
}

static void process_cal_coil_command(uint32_t *data)
{
	if(!try_enter_calibration_state())
	{
		for(int i = 0; i < CAL_COIL_CMD_COUNT; i++)
		{
			cal_coil_response[i] = SPR_ACCESS_ERROR;
		}
		return;
	}

	bool cmd_is_valid = true;
	
	//TODO set SPR all good here
	for(int i = 0; i < CAL_COIL_CMD_COUNT; i++)
	{
		cal_coil_response[i] = SPR_OK;
	}
	
	cmd_is_valid &= coil_float_check(data, -2000, 2000, CAL_COIL_CMD_X);
	cmd_is_valid &= coil_float_check(data, -2000, 2000, CAL_COIL_CMD_Y);
	cmd_is_valid &= coil_float_check(data, 0, 3000, CAL_COIL_CMD_F);
	
	if(cmd_is_valid)
	{
		float *target = (float *)(data);
		float coil_out = (*target / 1000.0f) * 2.5f;
		set_coil_voltage(X_COIL_DAC_CH, coil_out);
		target++;
		coil_out = (*target / 1000.0f) * 2.5f;
		set_coil_voltage(Y_COIL_DAC_CH, coil_out);
		target++;
		coil_out = (*target / 1000.0f) * 1.666f;
		set_coil_voltage(F_COIL_DAC_CH, coil_out);
	}
}

static void process_cal_hvps_command(uint32_t *data)
{
	if(!try_enter_calibration_state())
	{
		for(int i = 0; i < CAL_HVPS_CMD_COUNT; i++)
		{
			cal_hvps_cmd_response[i] = SPR_ACCESS_ERROR;
		}
		return;
	}

	for(int i = 0; i < CAL_HVPS_CMD_COUNT; i++)
	{
		cal_hvps_cmd_response[i] = SPR_OK;
	}

	VariableValue output_data[3];
	for(int i = 0; i < 3; i++)
	{
		output_data[i].u = data[i];
	}
	queue_hvps_cmd(output_data[0].u, output_data[1].f, output_data[2].u);
}

static void process_cal_directive_command(uint32_t *data)
{
	if(!try_enter_calibration_state())
	{
		cal_directive_response[DIRECTIVE_RES_STATUS] = SPR_ACCESS_ERROR;
		return;
	}

	cal_directive_response[DIRECTIVE_RES_STATUS] = SPR_INVALID;

	if(data[DIRECTIVE_CMD_COMMAND] == 0x03)
	{
		cal_directive_response[DIRECTIVE_RES_STATUS] = SPR_OK;
		process_start_directive();
	}
	else if(data[DIRECTIVE_CMD_COMMAND] == 0x04)
	{
		cal_directive_response[DIRECTIVE_RES_STATUS] = SPR_OK;
		process_stop_directive();
		exit_calibration_state();
	}
}

static void process_cal_setpoint_request(uint32_t *data)
{
	(void)data;
	if(!try_enter_calibration_state())
	{
		memset(cal_setpoint_req_response, 0, sizeof(cal_setpoint_req_response));
		return;
	}

	//Copy setpoint values read from HVPS into cal_setpoint_req_response
	memcpy(cal_setpoint_req_response, hvps_setpoints, CAL_SP_RES_COUNT*sizeof(float));
}

static void process_cal_mag_command(uint32_t *data)
{
	if(!try_enter_calibration_state())
	{
		memset(cal_mag_response, 0, sizeof(cal_mag_response));
		return;
	}

	set_mag_cal_window(*data);
	memcpy(cal_mag_response, mag_cal_array, sizeof(cal_mag_response));
}

#else
static void process_qc_command(uint32_t *data)
{
	uint32_t command = data[QC_REQ_COMMAND];
	(void)data[QC_REQ_RESERVED];

	switch(command)
	{
		case 1:
		{
			QcSessionStatus response_status;
			if(qc_session_get_status() == QC_SESSION_ARMED)
			{
				response_status = QC_SESSION_ARMED;
			}
			else if(system_status[SS_STATE].i == STATE_READY)
			{
				response_status = qc_session_arm();
			}
			else
			{
				response_status = QC_SESSION_ERROR;
			}
			qc_reported[QC_RES_SESSION_STATUS].u = (uint32_t)response_status;
			break;
		}
		case 2:
			qc_session_stop();
			break;
		default:
			qc_reported[QC_RES_SESSION_STATUS].u = QC_SESSION_ERROR;
			report_typed_fault1(
				FAULT_QC,
				"QC command was %u; expected 1 or 2.",
				MAKE_ARG(command));
			break;
	}
}
#endif

