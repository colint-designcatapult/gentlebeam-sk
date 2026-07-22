/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: High voltage power supply
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <regex.h>
#include <string.h>
#include <stdio.h>
#include <math.h>
#include "faults.h"
#include "hvps_monitoring.h"
#include "system_monitoring.h"
#include "system_parameters.h"
#include "state_machine.h"
#include "hvps.h"

volatile uint16_t checksum = 0;

struct io_descriptor *hvps_io;
static struct timer_task VTIMER_hvps_req_timer;

uint8_t hvps_tx_queue[MAX_HVPS_CMD_BYTES];
uint8_t hvps_tx_buf[MAX_HVPS_CMD_BYTES];
uint8_t hvps_rx_buf[HVPS_RX_BYTE_COUNT];
VariableValue hvps_status[NUM_HVPS_STATUS];

volatile uint32_t hvps_tx_idx = 0;
volatile uint32_t hvps_rx_idx = 0;
volatile bool hvps_rx_ready = false;
volatile bool hvps_tx_ready = false;
volatile bool hvps_read_req = false;
volatile int hvps_read_incomplete = 0;

HvpsCheckMode hcm = HCM_HV_CHECK_INIT;

bool fast_warmup_enabled = false;

volatile bool hvps_int_test_flag = false;
volatile int hvps_int_test_count = 0;
static struct timer_task VTIMER_hvps_check_timer;

static bool check_hvps_status_checksum();

static void hvps_check_timer(const struct timer_task *const timer_task);
static void hvps_uart_rx_cb(const struct usart_async_descriptor *const io_descr);
static void hvps_uart_tx_cb(const struct usart_async_descriptor *const io_descr);

/*
float hvps_warmup_target = ;
float hvps_condition_target = ;*/


void init_hvps()
{
	usart_async_get_io_descriptor(&HVPS_UART, &hvps_io);
	
	//Register RX callback
	usart_async_register_callback(&HVPS_UART, USART_ASYNC_RXC_CB, hvps_uart_rx_cb);
	usart_async_register_callback(&HVPS_UART, USART_ASYNC_TXC_CB, hvps_uart_tx_cb);
	//No need to register error callback with UART. Timeout is sufficient
	
	//Enable peripheral
	usart_async_enable(&HVPS_UART);
	
	
	//TBD TODO initialize HVPS parameters here
	hvps_tx_ready = true;
	
	VTIMER_hvps_req_timer.interval = 100;
	VTIMER_hvps_req_timer.cb = hvps_req_timer;
	VTIMER_hvps_req_timer.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_hvps_req_timer);
}

void hvps_req_timer(const struct timer_task *const timer_task)
{
	hvps_read_req = true;
	system_status[SS_SYS_RUNTIME].u += 100;
}

static void hvps_check_timer(const struct timer_task *const timer_task)
{
	//hvps_int_test_flag = true;
	queue_sm_event(EVENT_HVPS_CHECK);
}

void init_hvps_check()
{
	hcm = HCM_HV_CHECK_INIT;
	update_hvps_check();
}

bool update_hvps_check()
{
	switch(hcm)
	{
		case HCM_HV_CHECK_INIT:
			//Disable HV interlock
			VTIMER_hvps_check_timer.interval = 100;
			break;
		case HCM_CLEAR_FAULT:
			//Clear faults to try and unlatch any HW faults
			queue_hvps_cmd(HVPS_CMD_CLEAR_FAULTS, 0, 0);
			VTIMER_hvps_check_timer.interval = 50;
			break;
		case HCM_HV_CHECK_CLEAR:
			//Unlatch HW faults
			pulse_fault_clear();
			VTIMER_hvps_check_timer.interval = 150;
			break;
		case HCM_HV_CHECK_EN:
			queue_hvps_cmd(HVPS_CMD_INTERLOCK_TEST, 0, 123);	//TBD TODO magic number
			VTIMER_hvps_check_timer.interval = 150;
			break;
		case HCM_HV_CHECK_SET:
			gpio_set_pin_level(IO_HV_EN, true);
			VTIMER_hvps_check_timer.interval = 50;
			break;
		case HCM_GRID_CHECK_INIT:
			gpio_set_pin_level(IO_GRID_ENn, false);
			VTIMER_hvps_check_timer.interval = 50;
			break;
		case HCM_GRID_CHECK_EN:
			queue_hvps_cmd(HVPS_CMD_INTERLOCK_TEST, 0, 456);	//TBD TODO magic number
			VTIMER_hvps_check_timer.interval = 150;
			break;
		case HCM_GRID_CHECK_SET:
			gpio_set_pin_level(IO_GRID_ENn, true);
			VTIMER_hvps_check_timer.interval = 100;
			break;
		case HCM_VALIDATE:
			break;
		default:
			break;
	}
	
	if(hcm != HCM_VALIDATE)
	{
		VTIMER_hvps_check_timer.cb = hvps_check_timer;
		VTIMER_hvps_check_timer.mode = TIMER_TASK_ONE_SHOT;
		timer_add_task(&VTIMER, &VTIMER_hvps_check_timer);
		hcm++;
		return false;
	}
	else
	{
		hcm = HCM_HV_CHECK_INIT;
		//TBD TODO add status bit checks for HV enabled and grid ctrl enabled
		/*if(hvps_status[])
		{
			//TBD TODO change fault values
			report_zfault(FAULT_HVPS_COMM_ERROR, 0, 1, 0);
			return false;
		}*/
		return true;
	}
}

void process_hvps()
{
	//Flag should be periodic
	if(hvps_read_req)
	{
		hvps_read_req = false;
		
		//Check to see if last read was successful
		if(++hvps_read_incomplete > HVPS_MAX_NO_COMM)
		{
			report_typed_fault1(FAULT_HVPS_COMM, "No HVPS response was received for %u consecutive reads.", MAKE_ARG(HVPS_MAX_NO_COMM));
		}
	}
	
	//Flags indicate TX not busy and can send data
	if(hvps_tx_ready && (hvps_tx_idx > 0))
	{
		//Copy queued bytes to tx output buffer and transmit
		memcpy(hvps_tx_buf, hvps_tx_queue, hvps_tx_idx);
		io_write(hvps_io, hvps_tx_buf, hvps_tx_idx);
		
		//Reset indexing and flag
		hvps_tx_idx = 0;
		hvps_tx_ready = false;
	}
	
	//Flag indicates required number of bytes received
	if(hvps_rx_ready)
	{
		hvps_rx_ready = false;
		
		//If checksums match, copy rx buffer into status
		if(check_hvps_status_checksum())
		{
			gpio_toggle_pin_level(IO_LED4);
			
			memcpy(hvps_status, hvps_rx_buf, HVPS_RX_BYTE_COUNT);
			hvps_read_incomplete = 0;
			
			report_hvps_data(hvps_status);
		}
	}
}

static bool check_hvps_status_checksum()
{
	//TBD TODO CRC instead of checksum if desired
	uint32_t check_val = 0;
	for (int i = HVPS_STATUS_FLAG_BITS; i < HVPS_STATUS_CRC; i++)
	{
		check_val += hvps_status[i].u;
	}
	if (check_val == hvps_status[HVPS_STATUS_CRC].u)
	{
		return true;	
	}
	return false;
}

void queue_hvps_cmd(HvpsCmd cmd, float param_f, uint32_t param_i)
{
	//Make sure there is enough space for next command
	if((hvps_tx_idx+HVPS_TX_BYTE_COUNT) >= MAX_HVPS_CMD_BYTES)
	{
		report_typed_fault1(FAULT_HVPS_COMM, "HVPS command exceeded the %u-byte command buffer.", MAKE_ARG(MAX_HVPS_CMD_BYTES));
		return;
	}
	
	uint32_t queue_addr = 0;
	uint32_t *queue_ptr;
	uint32_t hvps_crc = 0;	
	
	//Add sync bytes
	queue_addr = &hvps_tx_queue;
	queue_addr += hvps_tx_idx;
	queue_ptr = (uint32_t *)(queue_addr);
	*queue_ptr = 0xFFFFFFFF;
	queue_addr += sizeof(uint32_t);
	queue_ptr = (uint32_t *)(queue_addr);
	*queue_ptr = 0xFFFFFFFF;
	queue_addr += sizeof(uint32_t);
	
	
	//Add cmd bytes
	queue_ptr = (uint32_t *)(queue_addr);
	*queue_ptr = cmd;
	hvps_crc += cmd;
	queue_addr += sizeof(uint32_t);
	
	//Add float param bytes
	VariableValue fparam;
	fparam.f = param_f;
	queue_ptr = (uint32_t *)(queue_addr);
	*queue_ptr = fparam.u;
	hvps_crc += fparam.u;
	queue_addr += sizeof(uint32_t);
	
	//Add int param bytes
	queue_ptr = (uint32_t *)(queue_addr);
	*queue_ptr = param_i;
	hvps_crc += param_i;
	queue_addr += sizeof(uint32_t);
	
	//Add crc param bytes
	//TBD TODO calculate checksum here
	queue_ptr = (uint32_t *)(queue_addr);
	*queue_ptr = hvps_crc;
	queue_addr += sizeof(uint32_t);
	
	//Update byte count
	hvps_tx_idx += HVPS_TX_BYTE_COUNT;
}

void enable_ecc(bool on)
{
	gpio_set_pin_level(IO_EMISSION_EN, on);
}

void enable_hv(bool on)
{
	gpio_set_pin_level(IO_HV_EN, on);
}

void enable_grid(bool on)
{
	gpio_set_pin_level(IO_GRID_ENn, on);
}

void set_hvps_heater(float mA)
{
	//Check to make sure we have a valid heater current
	if(isnan(mA) || mA > MAX_HEATER_MA)
	{
		return;
	}
	
	//If we have a low heater value, disable the heater
	if(mA <= 0)
	{
		queue_hvps_cmd(HVPS_CMD_SET_FIL, 0, 0);
		hvps_expected_val[HVPS_EXPECTED_FIL] = 0;
	}
	//Otherwise write the new heater value
	else
	{
		queue_hvps_cmd(HVPS_CMD_SET_FIL, mA, (uint32_t)fast_warmup_enabled);
		hvps_expected_val[HVPS_EXPECTED_FIL] = mA;
	}
}

void set_hvps_kv(float kv, float mA)
{	
	//Check to make sure we have a valid kv
	if(kv > MAX_KV || kv < MIN_KV || isnan(kv))
	{
		return;
	}
#if defined(CALIBRATION_MODE)
	if(mA > MAX_MA || mA < MIN_MA || isnan(mA))
#else
	if(mA > MAX_MA || mA < 0 || isnan(mA))
#endif
	{
		return;
	}
	
	//Calculated expected power from given values
	float power = 0;
	power = kv * mA;
	
	//Set expected values
	hvps_expected_val[HVPS_EXPECTED_KV] = kv;
	hvps_expected_val[HVPS_EXPECTED_MA] = mA;

	//Queue command to write KV and MA values
	queue_hvps_cmd(HVPS_CMD_SET_KV, kv, 0);
	queue_hvps_cmd(HVPS_CMD_SET_PWR, power, 0);
}

void set_hvps_ma_lim(float lim)
{
	/*
	//Check that limit is valid
	if(lim > ?? || lim < ??)
	{
		return;
	}*/
	
	queue_hvps_cmd(HVPS_CMD_SET_MA_LIM, lim, 0);
}

void set_hvps_grid(float grid_v)
{
	/*
	TBD TODO
	check that grid_v is valid value
	*/
	hvps_expected_val[HVPS_EXPECTED_GRID] = grid_v;
	queue_hvps_cmd(HVPS_CMD_SET_GRID, grid_v, 0);	
}

void enable_fast_warmup(bool en)
{
	fast_warmup_enabled = en;
}

static void hvps_uart_rx_cb(const struct usart_async_descriptor *const io_descr)
{
	uint8_t read_byte = 0;
	io_read(hvps_io, &read_byte, 1);
	
	if(hvps_rx_idx < HVPS_RX_SYNC_COUNT && read_byte != 0xFF)
	{	
		hvps_rx_idx = 0;
	}
	else
	{
		hvps_rx_buf[hvps_rx_idx++] = read_byte;
	}
	if(hvps_rx_idx >= HVPS_RX_BYTE_COUNT)
	{
		
		hvps_rx_idx = 0;
		hvps_rx_ready = true;
	}
}

static void hvps_uart_tx_cb(const struct usart_async_descriptor *const io_descr)
{
	hvps_tx_ready = true;
}
