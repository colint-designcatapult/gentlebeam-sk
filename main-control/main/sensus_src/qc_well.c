/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: QC well
*	Author: Carlton Chow
*	Description:
*/

/*
[0x80 0x80 0x80 0x80] [48 x uint16_t] [u16_t crc]
*/

#include <atmel_start.h>
#include <string.h>
#include "faults.h"
#include "system_parameters.h"
#include "checksum.h"
#include "qc_well.h"

struct io_descriptor *qc_io;
static struct timer_task VTIMER_qc_check;

volatile bool qc_data_requested = false;
volatile uint32_t qc_rx_idx = 0;
uint8_t qc_rx_buf[QC_RX_SIZE];
uint16_t qc_raw_buf[QC_DATA_COUNT];

volatile int request_qc_countdown = -1;
volatile int read_qc_countdown = -1;
uint8_t qc_tx_request[] = {QC_TX_START_BYTE, QC_TX_CMD_BYTE, QC_TX_CHECK_BYTE};

static void qc_uart_rx_cb(const struct usart_async_descriptor *const io_descr);
static void qc_timer(const struct timer_task *const timer_task);
static void read_qc();

void init_qc_well()
{
	//Register RX callback
	usart_async_register_callback(&QC_UART, USART_ASYNC_RXC_CB, qc_uart_rx_cb);
	//No need to register error callback with UART. Timeout is sufficient.
	
	//Enable peripheral
	usart_async_get_io_descriptor(&QC_UART, &qc_io);
	usart_async_enable(&QC_UART);
	
	init_crc16_tab();
	
	for(int op_idx = 0; op_idx < MAX_OPERATIONAL_POINTS; op_idx++)
	{
		qc_data[op_idx][0].i = op_idx;
	}
	
	VTIMER_qc_check.interval = 500;
	VTIMER_qc_check.cb = qc_timer;
	VTIMER_qc_check.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_qc_check);
}

static void qc_timer(const struct timer_task *const timer_task)
{
	if(read_qc_countdown > 0)
	{
		read_qc_countdown--;
	}
	if(request_qc_countdown > 0)
	{
		request_qc_countdown--;
	}
}

void process_qc()
{
	if(request_qc_countdown == 0)
	{
		request_qc_countdown--;
		io_write(qc_io, qc_tx_request, sizeof(qc_tx_request));
	}
	
	if(read_qc_countdown == 0)
	{
		read_qc_countdown--;
		read_qc();
	}
}

static void read_qc()
{
	//gpio_set_pin_level(IO_QC_EN, false);
	if(qc_rx_idx != QC_RX_SIZE)
	{
		//report_zfault(FAULT_QC_COMM_TIMEOUT, (float)QC_RX_SIZE, (float)qc_rx_idx, 0);
		qc_data_requested = false;
		qc_rx_idx = 0;
		return;
	}
	
	//Check to make sure received message is correct
	bool msg_ok = true;
	uint16_t *qc_u16_buf = (uint16_t *)qc_rx_buf;
	
	memcpy(qc_raw_buf, qc_rx_buf+4, sizeof(uint16_t)*QC_DATA_COUNT);
	
	for(int i = 0; i < QC_RX_START_COUNT; i++)
	{
		//Check that start byte values are ok
		if(qc_u16_buf[i] != QC_RX_START_VALUE)
		{
			msg_ok = false;
		}
	}
	
	//Check that CRC value is ok
	uint16_t crc16_val = (uint16_t)crc_16(qc_rx_buf, QC_RX_SIZE-sizeof(uint16_t));
	if(qc_u16_buf[QC_RX_CRC_POS] != crc16_val)
	{
		msg_ok = false;
	}
	
	if(msg_ok)
	{
		report_qc_well_data((int16_t*)(qc_u16_buf+QC_RX_START_COUNT));
	}
	
	qc_rx_idx = 0;
	qc_data_requested = false;
}

void request_qc_info(float seconds)
{
	//Do not initialize a request if we have already issued one
	if(qc_data_requested) return;
	
	//Check that time is valid
	if(isnan(seconds) || seconds < QC_CHECK_MIN_SEC) return;
	
	//gpio_set_pin_level(IO_QC_EN, true);
	
	//TBD TODO magic numbers
	//Adjust QC request and read time based on OP time
	int i_sec = (int)seconds;
	read_qc_countdown = (i_sec-1)*2;
	request_qc_countdown = 1;
	//request_qc_countdown = read_qc_countdown-((int)(QC_TX_CMD_BYTE/1.4));
	
	//io_write(qc_io, qc_tx_request, sizeof(qc_tx_request));
	qc_data_requested = true;
	qc_rx_idx = 0;
}

static void qc_uart_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	//Read byte into
	uint8_t qc_rx;
	io_read(qc_io, &qc_rx, 1);
	
	//Do not queue data if we are still waiting on processing
	if(qc_rx_idx >= QC_RX_SIZE) return;
	
	//Reset if invalid start sequence detected
	if(qc_rx_idx < QC_RX_START_COUNT && qc_rx != QC_RX_START_BYTE)
	{
		qc_rx_idx = 0;
	}
	else
	{
		qc_rx_buf[qc_rx_idx++] = qc_rx;
	}
}
