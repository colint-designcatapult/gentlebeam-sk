/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: High voltage power supply
*	Author: Carlton Chow
*	Description:
*/


#ifndef HVPS_H_
#define HVPS_H_

#include <stdbool.h>
#include "system_parameters.h"

#define HVPS_RX_SYNC_COUNT	8
#define HVPS_RX_BYTE_COUNT	(NUM_HVPS_STATUS *sizeof(uint32_t))

#define HVPS_TX_SYNC_COUNT	8
#define HVPS_TX_BYTE_COUNT	(NUM_HVPS_OUT *sizeof(uint32_t))

#define MAX_HVPS_CMDS		20
#define MAX_HVPS_CMD_BYTES	(HVPS_TX_BYTE_COUNT * MAX_HVPS_CMDS)

#if defined(CALIBRATION_MODE)
#if MRSRC
#define MAX_HEATER_MA	4000
#elif GBSRC
#define MAX_HEATER_MA	3250
#endif
#else
#define MAX_HEATER_MA	3250
#endif

#define MAX_KV		100.0f
#define MIN_KV		0.0f

#if defined(CALIBRATION_MODE)
#define MAX_MA		2.0f
#define MIN_MA		0.0f
#else
#define MAX_MA		8.0f

#define DEFAULT_GRID_OFF_V		200
#define DEFAULT_GRID_ON_V		1000
#endif

#define HVPS_MAX_NO_COMM	10

typedef enum hvpsCheckMode
{
	HCM_HV_CHECK_INIT = 0,
	HCM_CLEAR_FAULT,
	HCM_HV_CHECK_CLEAR,
	HCM_HV_CHECK_EN,
	HCM_HV_CHECK_SET,
	HCM_GRID_CHECK_INIT,
	HCM_GRID_CHECK_EN,
	HCM_GRID_CHECK_SET,
	HCM_VALIDATE
	
} HvpsCheckMode;

typedef enum hvpsCmd
{
	HVPS_CMD_TEST = 0,
	HVPS_CMD_ALIVE,
	HVPS_CMD_CLEAR_FAULTS,
	HVPS_CMD_INTERLOCK_TEST,
	HVPS_CMD_SET_PWR,
	HVPS_CMD_SET_KV,
	HVPS_CMD_SET_MA_LIM,
	HVPS_CMD_SET_GRID,
	HVPS_CMD_SET_FIL,
	HVPS_CMD_CONFIG_PWD,
	HVPS_CMD_SET_CONFIG,
#if defined(CALIBRATION_MODE)
	HVPS_CMD_CAL_START,
	HVPS_CMD_CAL_STOP,
#endif
	NUM_HVPS_CMD
} HvpsCmd;


typedef enum hvpsStatus
{
	HVPS_STATUS_SYNC_1 = 0,
	HVPS_STATUS_SYNC_2,
	HVPS_STATUS_FLAG_BITS,
	HVPS_STATUS_IO_BITS,
	HVPS_STATUS_RUNTIME,
	HVPS_STATUS_PWR_SP,
	HVPS_STATUS_KV_SP,
	HVPS_STATUS_MA_LIM_SP,
	HVPS_STATUS_GRID_SP,
	HVPS_STATUS_FIL_SP,
	HVPS_STATUS_FIL_FB,
	HVPS_STATUS_KV_FB,
	HVPS_STATUS_MA_FB,
	HVPS_STATUS_GRID_FB,
	HVPS_STATUS_CRC,
	NUM_HVPS_STATUS
} HvpsStatus;

typedef enum hvpsOutput
{
	HVPS_OUT_SYNC_1 = 0,
	HVPS_OUT_SYNC_2,
	HVPS_OUT_FIELD,
	HVPS_OUT_PARAM_F,
	HVPS_OUT_PARAM_I,
	HVPS_OUT_CRC,
	NUM_HVPS_OUT
} HvpsOutput;

typedef enum hvpsHtrState
{
	HVPS_HTR_STATE_OFF = 0,
	HVPS_HTR_STATE_ON,
	HVPS_HTR_STATE_RAMP_UP,
	HVPS_HTR_STATE_REGULATING,
	HVPS_HTR_STATE_RAMP_DOWN
} HvpsHtrState;

typedef enum hvpsKvState
{
	HVPS_KV_STATE_OFF = 0,
	HVPS_KV_STATE_ON,
	HVPS_KV_STATE_RAMP_UP,
	HVPS_KV_STATE_RAMP_DOWN,
	HVPS_KV_STATE_REGULATING
} HvpskVState;

void init_hvps();
void process_hvps();
void hvps_req_timer(const struct timer_task *const timer_task);

void init_hvps_check();
bool update_hvps_check();

void enable_grid(bool on);
void enable_ecc(bool on);
void enable_hv(bool on);

void set_hvps_heater(float mA);
void set_hvps_kv(float kv, float mA);
void set_hvps_ma_lim(float lim);
void set_hvps_grid(float grid_v);
void enable_fast_warmup(bool en);

void queue_hvps_cmd(HvpsCmd cmd, float param_f, uint32_t param_i);

extern VariableValue hvps_status[NUM_HVPS_STATUS];

#endif /* HVPS_H_ */