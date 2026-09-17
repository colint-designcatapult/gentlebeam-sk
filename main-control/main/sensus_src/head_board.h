/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Head board
*	Author: Carlton Chow
*	Description:
*/


#ifndef HEAD_BOARD_H_
#define HEAD_BOARD_H_
#include "system_parameters.h"
#if !defined(CALIBRATION_MODE)
#include "pc_comm_parser.h"
#endif

#define HB_COMM_VERSION_MAJ		'2'
#define HB_COMM_VERSION_MIN		'0'

#define HB_SYNC_VAL		0xFF
#define HB_SYNC_COUNT	4
#define HB_DELIM_VAL	0xA5
#define HB_TERM_VAL		0x99
#define HB_FIELD_SIZE	5

#define NUM_HB_MAG_DEV		3
#define NUM_HB_MAG_AXIS		3
#define NUM_HB_MAG_TYPE		2
#define NUM_HB_MAG_SAMP		1000

enum
{
	HB_RX_SYNC = 0,
	HB_RX_INFO,
	HB_RX_IO,
	HB_RX_COL_LOW,
	HB_RX_COL_HIGH,
	HB_RX_PRESSURE,
	HB_RX_FLOW,
	HB_RX_TEMP,
	HB_RX_MAG_X_1,
	HB_RX_MAG_Y_1,
	HB_RX_MAG_Z_1,
	HB_RX_MAG_X_2,
	HB_RX_MAG_Y_2,
	HB_RX_MAG_Z_2,
	HB_RX_QC_VAL,
	HB_RX_QC_ACCUMULATION_0,
	HB_RX_QC_ACCUMULATION_1,
	HB_RX_QC_SAMPLE_COUNT_0,
	HB_RX_QC_SAMPLE_COUNT_1,
	HB_RX_QC_ACQUISITION_STATE,
	HB_RX_CRC,
	HB_RX_NUM_FIELDS
};

#define HB_RX_MSG_SIZE		(HB_RX_NUM_FIELDS*HB_FIELD_SIZE)

enum
{
	HB_MAG_CAL_SIZE = 0,
	HB_MAG_CAL_X1_SUM,
	HB_MAG_CAL_Y1_SUM,
	HB_MAG_CAL_Z1_SUM,
	HB_MAG_CAL_X1_SQ,
	HB_MAG_CAL_X1_SQ_2,
	HB_MAG_CAL_Y1_SQ,
	HB_MAG_CAL_Y1_SQ_2,
	HB_MAG_CAL_Z1_SQ,
	HB_MAG_CAL_Z1_SQ_2,
	HB_MAG_CAL_X2_SUM,
	HB_MAG_CAL_Y2_SUM,
	HB_MAG_CAL_Z2_SUM,
	HB_MAG_CAL_X2_SQ,
	HB_MAG_CAL_X2_SQ_2,
	HB_MAG_CAL_Y2_SQ,
	HB_MAG_CAL_Y2_SQ_2,
	HB_MAG_CAL_Z2_SQ,
	HB_MAG_CAL_Z2_SQ_2,
	HB_MAG_CAL_X3_SUM,
	HB_MAG_CAL_Y3_SUM,
	HB_MAG_CAL_Z3_SUM,
	HB_MAG_CAL_X3_SQ,
	HB_MAG_CAL_X3_SQ_2,
	HB_MAG_CAL_Y3_SQ,
	HB_MAG_CAL_Y3_SQ_2,
	HB_MAG_CAL_Z3_SQ,
	HB_MAG_CAL_Z3_SQ_2,
	HB_NUM_MAG_CAL
};

#define MAG_SYNC_VAL		0xFF
#define MAG_SYNC_COUNT		8
#define MAG_RX_MSG_SIZE		((HB_NUM_MAG_CAL+2)*sizeof(int32_t))

#if defined(CALIBRATION_MODE)
#define HB_TX_MSG_SIZE		8
#else
#define HB_TX_MSG_SIZE		12
#endif


#define HB_COMM_TIMEOUT_MS	500
#define HB_LED_MS			100
#define MAX_HB_ERRORS		2
#if !defined(CALIBRATION_MODE)
#define HB_QC_ANALOG_WIRE_CHANNEL	0u
#define HB_QC_UNUSED_WIRE_CHANNEL	1u
#define HB_QC_COMMAND_RETRY_MS		100u
#define HB_QC_ACK_TIMEOUT_MS			1000u

typedef enum
{
	HB_QC_DESIRED_STOPPED = 0,
	HB_QC_DESIRED_ACCUMULATING = 1
} HbQcDesiredState;

typedef enum
{
	HB_QC_STATE_STOPPED = 0,
	HB_QC_STATE_ACTIVE,
	HB_QC_STATE_COMPLETE,
	HB_QC_STATE_ERROR
} HbQcAcquisitionState;
#endif

enum
{
	LED_SEQ_OFF = 0,
	LED_SEQ_COLD = 1,
	LED_SEQ_WARMUP = 2,
	LED_SEQ_PRIMED = 4,
	LED_SEQ_SETUP = 5,
	LED_SEQ_READY = 6,
	LED_SEQ_XRAY = 7,
	LED_SEQ_IDLE = 8,
	LED_SEQ_FAULT = 9,
	NUM_LED_SEQUENCES = 24
};

extern VariableValue mag_cal_array[HB_NUM_MAG_CAL];

void init_head_board();
void process_hb();

void set_led_sequence(int led_idx);

void set_mag_cal_window(int samples);
#if !defined(CALIBRATION_MODE)
void set_qc_desired_state(HbQcDesiredState desired_state);
void qc_session_reset(void);
QcSessionStatus qc_session_arm(void);
void qc_session_start_for_emission(void);
void qc_session_stop(void);
QcSessionStatus qc_session_get_status(void);
#endif


#endif /* HEAD_BOARD_H_ */
