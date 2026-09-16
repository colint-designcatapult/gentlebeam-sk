/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Head board
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <lwip/sys.h>
#include <stdlib.h>
#include <string.h>
#include "checksum.h"
#include "faults.h"
#include "system_parameters.h"
#include "state_machine.h"
#include "head_board.h"
#include "magnetometer_monitoring.h"

#define HB_QC_READING_MASK		0x0FFFu
#define HB_QC_CONNECTED_FLAG	0x8000u

struct io_descriptor *hb_io;
volatile uint32_t hb_rx_idx = 0;
uint8_t hb_rx_buf[HB_RX_MSG_SIZE];
uint8_t hb_rx_processing_buf[HB_RX_MSG_SIZE];
volatile bool hb_rx_ready = false;
volatile bool hb_tx_data_available = true;
volatile bool hb_tx_busy = false;

uint8_t hb_tx_queue[HB_TX_MSG_SIZE];
uint8_t hb_tx_buf[HB_TX_MSG_SIZE];

static struct timer_task VTIMER_hb_check;
volatile int hb_no_comm = 0;
uint32_t hb_comm_error_count = 0;

VariableValue mag_cal_array[HB_NUM_MAG_CAL];
int32_t mag_window_samples = 100;
#if !defined(CALIBRATION_MODE)
static HbQcDesiredState hb_qc_desired_state = HB_QC_DESIRED_STOPPED;
static QcSessionStatus qc_session_status = QC_SESSION_IDLE;
static bool qc_active_edge_seen = false;
static bool qc_stop_issued = false;
static bool qc_cancel_before_emission = false;
static bool qc_command_pending = false;
static bool qc_fault_reported = false;
static uint32_t qc_command_first_tick = 0;
static uint32_t qc_command_last_tick = 0;
static uint32_t qc_head_accumulations[2] = {0, 0};
static uint32_t qc_head_sample_counts[2] = {0, 0};

static void update_qc_session_from_head(HbQcAcquisitionState head_state);
static void update_qc_response(QcSessionStatus response_status);
static void fail_qc_session(bool acknowledgment_timeout);
static void service_qc_command(void);
#endif


static void send_mag_cal_window(int samples);

static bool hb_rx_packet_check();
static void extract_hb_rx_data();

static void hb_uart_rx_cb(const struct usart_async_descriptor *const io_descr);
static void hb_uart_tx_cb(const struct usart_async_descriptor *const io_descr);
static void hb_timeout_check(const struct timer_task *const timer_task);

void init_head_board()
{
	//Register RX and tx callbacks
	usart_async_register_callback(&HB_UART, USART_ASYNC_TXC_CB, hb_uart_tx_cb);
	usart_async_register_callback(&HB_UART, USART_ASYNC_RXC_CB, hb_uart_rx_cb);
	//No need to register error callback with UART. Timeout is sufficient
	
	usart_async_get_io_descriptor(&HB_UART, &hb_io);
	
	//Enable peripheral
	usart_async_enable(&HB_UART);
	
	init_crcccitt_tab();
	
	//Initialize HB TX packet and persistent desired states.
	memset(hb_tx_queue, 0, sizeof(hb_tx_queue));
	hb_tx_queue[0] = HB_SYNC_VAL;
	hb_tx_queue[1] = HB_SYNC_VAL;
	hb_tx_queue[2] = HB_SYNC_VAL;
	hb_tx_queue[3] = HB_SYNC_VAL;
#if !defined(CALIBRATION_MODE)
	set_led_sequence(LED_SEQ_OFF);
	set_qc_desired_state(HB_QC_DESIRED_STOPPED);
	qc_command_pending = false;
	qc_session_status = QC_SESSION_IDLE;
	update_qc_response(QC_SESSION_IDLE);
#endif

	//Initialize vtimer task to check head board communication
	VTIMER_hb_check.interval = HB_COMM_TIMEOUT_MS;
	VTIMER_hb_check.cb = hb_timeout_check;
	VTIMER_hb_check.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_hb_check);
}

static void hb_timeout_check(const struct timer_task *const timer_task)
{
	//Check to see if no comms received from HB
	//TBD TODO magic number
	if(++hb_no_comm > 2)
	{
		report_typed_fault1(FAULT_HEADBOARD_COMM, "No head-board response was received within %u ms.", MAKE_ARG(HB_COMM_TIMEOUT_MS));
	}
}

void set_led_sequence(int led_idx)
{
	if(led_idx < 0 || led_idx > 255)
	{
		return;
	}
	hb_tx_queue[4] = (uint8_t)led_idx;
	hb_tx_queue[5] = (uint8_t)led_idx;
	hb_tx_queue[6] = 0xFF-(uint8_t)led_idx;
	hb_tx_queue[7] = 0xFF-(uint8_t)led_idx;
	hb_tx_data_available = true;
	
	switch(led_idx)
	{
		case LED_SEQ_FAULT:
			gpio_set_pin_level(GPIO(GPIO_PORTD, 22), true);
			gpio_set_pin_level(GPIO(GPIO_PORTD, 23), true);
			break;
		case LED_SEQ_XRAY:
			gpio_set_pin_level(GPIO(GPIO_PORTD, 22), true);
			gpio_set_pin_level(GPIO(GPIO_PORTD, 23), false);
			break;
		case LED_SEQ_READY:
			gpio_set_pin_level(GPIO(GPIO_PORTD, 22), false);
			gpio_set_pin_level(GPIO(GPIO_PORTD, 23), true);
			break;
		default:
			gpio_set_pin_level(GPIO(GPIO_PORTD, 22), false);
			gpio_set_pin_level(GPIO(GPIO_PORTD, 23), false);
			break;
	}
}

void set_mag_cal_window(int samples)
{
	//DEBUG MAG CAL
	//gpio_toggle_pin_level(IO_LED5);
	if(samples == -1)
	{
		send_mag_cal_window(252);
	}
	else if(samples == -2)
	{
		send_mag_cal_window(253);
	}
	else if(samples > 0 && samples <= 250)
	{
		mag_window_samples = samples;
		send_mag_cal_window(mag_window_samples);	
	}	
}

static void send_mag_cal_window(int samples)
{
	if(samples <= 0 || samples >= 256)
	{
		return;
	}
	
	hb_tx_queue[4] = (uint8_t)samples;
	hb_tx_queue[5] = (uint8_t)samples;
	hb_tx_queue[6] = 0xFF-(uint8_t)samples;
	hb_tx_queue[7] = 0xFF-(uint8_t)samples;
	hb_tx_data_available = true;
}
#if !defined(CALIBRATION_MODE)
void set_qc_desired_state(HbQcDesiredState desired_state)
{
	if(desired_state != HB_QC_DESIRED_STOPPED &&
	   desired_state != HB_QC_DESIRED_ACCUMULATING)
	{
		return;
	}

	bool new_command = !qc_command_pending || hb_qc_desired_state != desired_state;
	hb_qc_desired_state = desired_state;
	hb_tx_queue[8] = (uint8_t)desired_state;
	hb_tx_queue[9] = (uint8_t)desired_state;
	hb_tx_queue[10] = 0xFFu - (uint8_t)desired_state;
	hb_tx_queue[11] = 0xFFu - (uint8_t)desired_state;
	hb_tx_data_available = true;
	qc_command_pending = true;
	if(new_command)
	{
		qc_command_first_tick = sys_now();
		qc_command_last_tick = qc_command_first_tick;
	}
}

void qc_session_reset(void)
{
	qc_command_pending = false;
	qc_active_edge_seen = false;
	qc_stop_issued = false;
	qc_cancel_before_emission = false;
	qc_fault_reported = false;
	qc_command_first_tick = 0;
	qc_command_last_tick = 0;
	qc_head_accumulations[0] = 0;
	qc_head_accumulations[1] = 0;
	qc_head_sample_counts[0] = 0;
	qc_head_sample_counts[1] = 0;
	qc_reported[QC_RES_CHANNEL_0_ACCUMULATION].u = 0;
	qc_reported[QC_RES_CHANNEL_1_ACCUMULATION].u = 0;
	qc_reported[QC_RES_CHANNEL_0_SAMPLE_COUNT].u = 0;
	qc_reported[QC_RES_CHANNEL_1_SAMPLE_COUNT].u = 0;
	qc_session_status = QC_SESSION_IDLE;
	update_qc_response(QC_SESSION_IDLE);
	set_qc_desired_state(HB_QC_DESIRED_STOPPED);
}

QcSessionStatus qc_session_arm(void)
{
	if(qc_session_status == QC_SESSION_ARMED)
	{
		update_qc_response(QC_SESSION_ARMED);
		return QC_SESSION_ARMED;
	}
	if(qc_session_status == QC_SESSION_STARTING ||
	   qc_session_status == QC_SESSION_ACCUMULATING ||
	   qc_session_status == QC_SESSION_STOPPING)
	{
		return QC_SESSION_ERROR;
	}

	qc_head_accumulations[0] = 0;
	qc_head_accumulations[1] = 0;
	qc_head_sample_counts[0] = 0;
	qc_head_sample_counts[1] = 0;
	qc_reported[QC_RES_CHANNEL_0_ACCUMULATION].u = 0;
	qc_reported[QC_RES_CHANNEL_1_ACCUMULATION].u = 0;
	qc_reported[QC_RES_CHANNEL_0_SAMPLE_COUNT].u = 0;
	qc_reported[QC_RES_CHANNEL_1_SAMPLE_COUNT].u = 0;
	qc_active_edge_seen = false;
	qc_stop_issued = false;
	qc_cancel_before_emission = false;
	qc_fault_reported = false;
	qc_session_status = QC_SESSION_ARMED;
	update_qc_response(QC_SESSION_ARMED);
	return QC_SESSION_ARMED;
}

void qc_session_start_for_emission(void)
{
	if(qc_session_status != QC_SESSION_ARMED)
	{
		return;
	}

	qc_active_edge_seen = false;
	qc_stop_issued = false;
	qc_cancel_before_emission = false;
	qc_session_status = QC_SESSION_STARTING;
	update_qc_response(qc_session_status);
	set_qc_desired_state(HB_QC_DESIRED_ACCUMULATING);
}

void qc_session_stop(void)
{
	switch(qc_session_status)
	{
		case QC_SESSION_ARMED:
			qc_head_accumulations[0] = 0;
			qc_head_accumulations[1] = 0;
			qc_head_sample_counts[0] = 0;
			qc_head_sample_counts[1] = 0;
			qc_session_status = QC_SESSION_COMPLETE;
			update_qc_response(qc_session_status);
			break;
		case QC_SESSION_STARTING:
			qc_cancel_before_emission = true;
			qc_stop_issued = true;
			qc_session_status = QC_SESSION_STOPPING;
			update_qc_response(qc_session_status);
			set_qc_desired_state(HB_QC_DESIRED_STOPPED);
			break;
		case QC_SESSION_ACCUMULATING:
			qc_stop_issued = true;
			qc_session_status = QC_SESSION_STOPPING;
			update_qc_response(qc_session_status);
			set_qc_desired_state(HB_QC_DESIRED_STOPPED);
			break;
		default:
			update_qc_response(qc_session_status);
			break;
	}
}

QcSessionStatus qc_session_get_status(void)
{
	return qc_session_status;
}
#endif

//Function called in main loop, values read/written and checked here
void process_hb()
{
	if(hb_rx_ready)
	{
		hb_rx_ready = false;

		if(hb_rx_packet_check())
		{
			extract_hb_rx_data();
		}
	}

#if !defined(CALIBRATION_MODE)
	service_qc_command();
#endif
	
	if(hb_tx_data_available && !hb_tx_busy)
	{
		hb_tx_data_available = false;
		
		//Copy over TX data to output buffer and send
		memcpy(hb_tx_buf, hb_tx_queue, HB_TX_MSG_SIZE);
		hb_tx_busy = true;
		io_write(hb_io, hb_tx_buf, HB_TX_MSG_SIZE);
#if !defined(CALIBRATION_MODE)
		if(qc_command_pending)
		{
			qc_command_last_tick = sys_now();
		}
#endif
	}
}

static bool hb_rx_packet_check()
{
	//Check that all delimiter bytes are correct
	for(int i = 1; i < HB_RX_NUM_FIELDS; i++)
	{
		if(hb_rx_processing_buf[(i*HB_FIELD_SIZE)-1] != HB_DELIM_VAL)
		{
			return false;
		}
	}
	
	//Check that termination byte is correct
	if(hb_rx_processing_buf[HB_RX_MSG_SIZE-1] != HB_TERM_VAL)
	{
		return false;
	}
	
	//Verify CRC from message
	uint32_t *crc_val = (uint32_t *)(hb_rx_processing_buf + (HB_RX_CRC*HB_FIELD_SIZE));
	uint32_t crc_calc =  (uint32_t)crc_ccitt_1d0f(hb_rx_processing_buf, HB_RX_CRC*HB_FIELD_SIZE);
#ifdef HEAD_COMM_TEST_MODE
	if(system_status[SS_STATE].i == STATE_WARMUP)
	{
		crc_calc = 0x00;
		report_typed_fault1(FAULT_HEADBOARD_COMM, "No head-board response was received within %u ms.", MAKE_ARG(HB_COMM_TIMEOUT_MS));
	}
#endif
	if(*crc_val != crc_calc)
	{
		//TBD TODO add potential faults for multiple missed CRCs in a row
		//Currently covered by timeout, could expand for more granular fault reporting
		return false;
	}
	
	return true;
}

static void extract_hb_rx_data()
{	
	float data_val = 0;
	uint32_t u_data_val = 0;
	
	//Check to see if info value is normal packet or mag calibration packet
	memcpy(&u_data_val, hb_rx_processing_buf+(HB_RX_INFO*HB_FIELD_SIZE), sizeof(uint32_t));
	
	//TBD TODO magic number, separate into separate function?
	if(u_data_val == 0x33)
	{
	}
	//TBD TODO magic number
	else if(u_data_val == 0x88)
	{
		gpio_toggle_pin_level(IO_LED5);
		
		//Save button data
		memcpy(&u_data_val,hb_rx_processing_buf+(HB_RX_IO*HB_FIELD_SIZE), sizeof(uint32_t));
		system_status[SS_BUTTONS].u = u_data_val & 0xFFFF;
		
		//Save collimator data
		memcpy(&u_data_val,hb_rx_processing_buf+(HB_RX_COL_LOW*HB_FIELD_SIZE), sizeof(uint32_t));
		system_status[SS_COLLIMATOR_LOW].u = u_data_val;
		memcpy(&u_data_val,hb_rx_processing_buf+(HB_RX_COL_HIGH*HB_FIELD_SIZE), sizeof(uint32_t));
		system_status[SS_COLLIMATOR_HIGH].u = u_data_val;
		
		//Save QC ADC readings and per-device I2C connection state.
		memcpy(&u_data_val,hb_rx_processing_buf+(HB_RX_QC_VAL*HB_FIELD_SIZE), sizeof(uint32_t));
#if !defined(CALIBRATION_MODE)
		uint16_t qc_adc_2 = (uint16_t)(u_data_val & 0xFFFFu);
		uint16_t qc_adc_1 = (uint16_t)(u_data_val >> 16);
		system_status[SS_QC_CHANNEL_0].f = (float)(qc_adc_2 & HB_QC_READING_MASK);
		system_status[SS_QC_CHANNEL_1].f = (float)(qc_adc_1 & HB_QC_READING_MASK);
		system_status[SS_QC_ADC_I2C_STATUS].u =
			((qc_adc_1 & HB_QC_CONNECTED_FLAG) != 0u ? 1u : 0u) |
			((qc_adc_2 & HB_QC_CONNECTED_FLAG) != 0u ? 2u : 0u);

		memcpy(&qc_head_accumulations[0],
			hb_rx_processing_buf+(HB_RX_QC_ACCUMULATION_0*HB_FIELD_SIZE),
			sizeof(uint32_t));
		memcpy(&qc_head_accumulations[1],
			hb_rx_processing_buf+(HB_RX_QC_ACCUMULATION_1*HB_FIELD_SIZE),
			sizeof(uint32_t));
		memcpy(&qc_head_sample_counts[0],
			hb_rx_processing_buf+(HB_RX_QC_SAMPLE_COUNT_0*HB_FIELD_SIZE),
			sizeof(uint32_t));
		memcpy(&qc_head_sample_counts[1],
			hb_rx_processing_buf+(HB_RX_QC_SAMPLE_COUNT_1*HB_FIELD_SIZE),
			sizeof(uint32_t));
		system_status[SS_QC_ACCUMULATION_0].u = qc_head_accumulations[0];
		system_status[SS_QC_ACCUMULATION_1].u = qc_head_accumulations[1];
		memcpy(&u_data_val,
			hb_rx_processing_buf+(HB_RX_QC_ACQUISITION_STATE*HB_FIELD_SIZE),
			sizeof(uint32_t));
		update_qc_session_from_head((HbQcAcquisitionState)u_data_val);
#endif
			
		//Save scalar parameter data. QC result fields are unsigned and handled above.
		for(int i = HB_RX_PRESSURE; i <= HB_RX_MAG_Z_2; i++)
		{
			memcpy(&data_val,hb_rx_processing_buf+(i*HB_FIELD_SIZE), sizeof(float));
			report_hb_data(i, data_val);
		}

		// Evaluate each coherent, CRC-valid set of magnetometer readings once.
		monitor_magnetometer_readings();
	}
}


static void hb_uart_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	uint8_t read_byte = 0;
	io_read(hb_io, &read_byte, 1);

	if(hb_rx_idx < HB_SYNC_COUNT && read_byte != HB_SYNC_VAL)
	{
		hb_rx_idx = 0;
	}
	else
	{
		hb_rx_buf[hb_rx_idx++] = read_byte;
	}
	
	//Once buffer is full, copy bytes and set flag for processing
	if(hb_rx_idx >= HB_RX_MSG_SIZE)
	{
		hb_rx_idx = 0;
		memcpy(hb_rx_processing_buf, hb_rx_buf, HB_RX_MSG_SIZE);
		hb_no_comm = 0;
		hb_rx_ready = true;
	}
}

static void hb_uart_tx_cb(const struct usart_async_descriptor *const io_descr)
{
	hb_tx_busy = false;
}

#if !defined(CALIBRATION_MODE)
static void update_qc_session_from_head(HbQcAcquisitionState head_state)
{
	if(qc_session_status == QC_SESSION_IDLE &&
	   qc_command_pending &&
	   hb_qc_desired_state == HB_QC_DESIRED_STOPPED &&
	   (head_state == HB_QC_STATE_STOPPED ||
		head_state == HB_QC_STATE_COMPLETE ||
		head_state == HB_QC_STATE_ERROR))
	{
		qc_command_pending = false;
		update_qc_response(QC_SESSION_IDLE);
		return;
	}

	if(head_state == HB_QC_STATE_ACTIVE && qc_session_status == QC_SESSION_STARTING)
	{
		qc_active_edge_seen = true;
		qc_command_pending = false;
		qc_session_status = QC_SESSION_ACCUMULATING;
		update_qc_response(qc_session_status);
		if(qc_cancel_before_emission)
		{
			qc_stop_issued = true;
			qc_session_status = QC_SESSION_STOPPING;
			update_qc_response(qc_session_status);
			set_qc_desired_state(HB_QC_DESIRED_STOPPED);
		}
		else
		{
			queue_sm_event(EVENT_QC_ACCUMULATION_ACTIVE);
		}
	}
	else if(head_state == HB_QC_STATE_ACTIVE &&
			qc_cancel_before_emission &&
			qc_session_status == QC_SESSION_STOPPING)
	{
		qc_active_edge_seen = true;
		set_qc_desired_state(HB_QC_DESIRED_STOPPED);
		update_qc_response(qc_session_status);
	}
	else if(head_state == HB_QC_STATE_STOPPED &&
			qc_cancel_before_emission &&
			qc_stop_issued &&
			qc_session_status == QC_SESSION_STOPPING)
	{
		qc_command_pending = false;
		qc_session_status = QC_SESSION_COMPLETE;
		update_qc_response(qc_session_status);
	}
	else if(head_state == HB_QC_STATE_COMPLETE &&
			qc_active_edge_seen &&
			qc_stop_issued &&
			qc_session_status == QC_SESSION_STOPPING)
	{
		qc_command_pending = false;
		qc_session_status = QC_SESSION_COMPLETE;
		update_qc_response(qc_session_status);
	}
	else if(head_state == HB_QC_STATE_ERROR &&
			(qc_session_status == QC_SESSION_STARTING ||
			 qc_session_status == QC_SESSION_ACCUMULATING ||
			 qc_session_status == QC_SESSION_STOPPING))
	{
		fail_qc_session(false);
	}
	else
	{
		update_qc_response(qc_session_status);
	}
}

static void update_qc_response(QcSessionStatus response_status)
{
	if(qc_active_edge_seen || response_status == QC_SESSION_ERROR)
	{
		qc_reported[QC_RES_CHANNEL_0_ACCUMULATION].u = qc_head_accumulations[0];
		qc_reported[QC_RES_CHANNEL_1_ACCUMULATION].u = qc_head_accumulations[1];
		qc_reported[QC_RES_CHANNEL_0_SAMPLE_COUNT].u = qc_head_sample_counts[0];
		qc_reported[QC_RES_CHANNEL_1_SAMPLE_COUNT].u = qc_head_sample_counts[1];
	}
	qc_reported[QC_RES_SESSION_STATUS].u = (uint32_t)response_status;
}

static void fail_qc_session(bool acknowledgment_timeout)
{
	hb_qc_desired_state = HB_QC_DESIRED_STOPPED;
	hb_tx_queue[8] = (uint8_t)HB_QC_DESIRED_STOPPED;
	hb_tx_queue[9] = (uint8_t)HB_QC_DESIRED_STOPPED;
	hb_tx_queue[10] = 0xFFu - (uint8_t)HB_QC_DESIRED_STOPPED;
	hb_tx_queue[11] = 0xFFu - (uint8_t)HB_QC_DESIRED_STOPPED;
	hb_tx_data_available = true;
	qc_command_pending = false;
	qc_session_status = QC_SESSION_ERROR;
	update_qc_response(qc_session_status);
	if(!qc_fault_reported)
	{
		qc_fault_reported = true;
		if(acknowledgment_timeout)
		{
			report_typed_fault(
				FAULT_QC,
				"Head-board QC command acknowledgment timed out.");
		}
		else
		{
			report_typed_fault(
				FAULT_QC,
				"Head-board QC acquisition failed.");
		}
	}
}

static void service_qc_command(void)
{
	if(!qc_command_pending)
	{
		return;
	}

	uint32_t now = sys_now();
	if((now - qc_command_first_tick) >= HB_QC_ACK_TIMEOUT_MS)
	{
		fail_qc_session(true);
		return;
	}
	if((now - qc_command_last_tick) >= HB_QC_COMMAND_RETRY_MS)
	{
		hb_tx_data_available = true;
	}
}
#endif
