/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Head board
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <stdlib.h>
#include <string.h>
#include "checksum.h"
#include "faults.h"
#include "system_parameters.h"
#include "state_machine.h"
#include "head_board.h"

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


static void send_mag_cal_window(int samples);

static bool hb_rx_packet_check();
static void extract_hb_rx_data();
static void calculate_diodes(uint32_t diodes);

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
	
	//Initialize HB TX packet
	hb_tx_queue[0] = HB_SYNC_VAL;
	hb_tx_queue[1] = HB_SYNC_VAL;
	hb_tx_queue[2] = HB_SYNC_VAL;
	hb_tx_queue[3] = HB_SYNC_VAL;
	hb_tx_queue[4] = 0;
	hb_tx_queue[5] = 0;
	hb_tx_queue[6] = 0;
	hb_tx_queue[7] = 0;

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
		case LED_SEQ_WARMUP_FAULT:
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
	
	if(hb_tx_data_available && !hb_tx_busy)
	{
		hb_tx_data_available = false;
		
		//Copy over TX data to output buffer and send
		memcpy(hb_tx_buf, hb_tx_queue, HB_TX_MSG_SIZE);
		hb_tx_busy = true;
		io_write(hb_io, hb_tx_buf, HB_TX_MSG_SIZE);
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
#endif
		//Calculate the total QC readings if running emission
		if(system_status[SS_STATE].i == STATE_EMISSION)
		{
			qc_samples++;
			calculate_diodes(u_data_val);
		}
		else
		{
			qc_samples = 0;
		}
			
		//Save parameter data
		for(int i = HB_RX_PRESSURE; i < HB_RX_CRC; i++)
		{
			memcpy(&data_val,hb_rx_processing_buf+(i*HB_FIELD_SIZE), sizeof(float));
			report_hb_data(i, data_val);
		}
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

static void calculate_diodes(uint32_t diodes)
{
	// diodes: upper 2 bytes are Diode 1 value, lower 2 are Diode 2
	float diode_0 = (float)(diodes & HB_QC_READING_MASK);
	float diode_1 = (float)((diodes >> 16) & HB_QC_READING_MASK);
	
	qc_reading_buf[0].f += diode_0;
	qc_reading_buf[1].f += diode_1;
	qc_reading_buf[2].f = 0;
	qc_reading_buf[3].f = 0;
	qc_reading_buf[4].f = 0;
}