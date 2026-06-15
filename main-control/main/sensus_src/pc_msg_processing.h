/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: PC message processing
*	Author: Carlton Chow
*	Description:
*/


#ifndef PC_MSG_PROCESSING_H_
#define PC_MSG_PROCESSING_H_

#include "pc_comm_parser.h"

#define MIN_CONFIG_TOL	0.1
#define MAX_CONFIG_TOL	100
#define MIN_CONFIG_TEMP	0
#define MAX_CONFIG_TEMP	100
#define MIN_CONFIG_PSI	0
#define MAX_CONFIG_PSI	10

#define AUTH_SECRET_KEY	0x12345678

#define RELEASE_TYPE_PLAN	1
#define RELEASE_TYPE_POINT	2

typedef enum setpointResult
{
	SPR_OK = 0,
	SPR_ACCESS_ERROR,
	SPR_OOB,
	SPR_INVALID,
	SPR_FORCE_INT = 0xFFFFFFFF
} SetpointResult;

typedef enum buttonResult
{
	BTN_OK = 0,
	BTN_MISSING,
	BTN_INVALID,
	BTN_FORCE_INT = 0xFFFFFFFF
} ButtonResult;

void process_command(PacketType_t ptype, void* data);
void *get_response_data(PacketType_t ptype);
void init_response_pointers();
#if defined(CALIBRATION_MODE)
void signal_hvps_stop();
void signal_emission_stop();
#endif

extern uint32_t network_config[NETWORK_CMD_COUNT];


#endif /* PC_MSG_PROCESSING_H_ */