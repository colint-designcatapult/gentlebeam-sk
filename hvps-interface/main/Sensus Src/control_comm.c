#include "main.h"
#include "stm32f3xx_hal.h"
#include "stdbool.h"

#include "control_comm.h"
#include "monitoring.h"
#include "processing.h"
#include "timers.h"


VariableValue comm_tx_out[NUM_COMM_TX_FIELDS];
VariableValue comm_rx_in[NUM_COMM_RX_FIELDS];

volatile bool comm_tx_busy = false;

volatile bool comm_recv = false;
volatile uint8_t comm_rx_recv = 0;

uint8_t rx_in[2];
uint8_t *rx_ptr;

static bool validate_comm_rx();
static void send_comm_tx();


void setup_control_comm()
{
	//Start next comm transmission in 100 ms
	comm_ms = 100;

	//Initialize receive index and pointer
	comm_rx_recv = 0;
	rx_ptr = (uint8_t *)comm_rx_in;

	//Initialize sync values
	comm_tx_out[COMM_TX_SYNC_START].i = 0xFFFFFFFF;
	comm_tx_out[COMM_TX_SYNC_NEXT].i = 0xFFFFFFFF;
	comm_rx_in[COMM_RX_SYNC_START].i = 0xFFFFFFFF;
	comm_rx_in[COMM_RX_SYNC_NEXT].i = 0xFFFFFFFF;

	//Clear any outstanding receives before accepting new data
	HAL_UART_AbortReceive(&huart2);

	//Start interrupt reception
	HAL_UART_Receive_IT(&huart2, rx_in, 1);
}

void process_control_comm()
{
	//Check for complete packet received
	if(comm_rx_recv >= NUM_COMM_RX_BYTES)
	{
		//Validate received packet
		if(validate_comm_rx())
		{
			process_command(comm_rx_in[COMM_RX_FIELD].i, comm_rx_in[COMM_RX_PARAM_F].f, comm_rx_in[COMM_RX_PARAM_I].i);
		}
		comm_rx_recv = 0;
	}

	//Wait until comm timer expires to send new telemetry out
	if(comm_ms < 0)
	{
		comm_ms = 100;	//TBD TODO magic number
		send_comm_tx();
		HAL_GPIO_TogglePin(GPIOB, IO_WD_RST_Pin);
	}
}

static bool validate_comm_rx()
{
	//TBD TODO update to CRC if needed, just use faster checksum for now
	uint32_t crc_calc = 0;
	crc_calc += comm_rx_in[COMM_RX_FIELD].u;
	crc_calc += comm_rx_in[COMM_RX_PARAM_F].u;
	crc_calc += comm_rx_in[COMM_RX_PARAM_I].u;

	if(crc_calc != comm_rx_in[COMM_RX_CRC].u)
	{
		return false;
	}

	return true;
}

static void send_comm_tx()
{
	if(comm_tx_busy)
	{
		return;
	}

	uint32_t comm_check = 0;

	//TBD TODO update better checksum if needed
	for(int i = COMM_HVPS_STATUS; i <= COMM_HVPS_RUNTIME; i++)
	{
#ifdef CALIBRATION_MODE
		comm_tx_out[i].i = get_monitored_int_val(i);
#else
        comm_tx_out[i].u = get_monitored_int_val(i);
#endif
		comm_check += comm_tx_out[i].u;
	}

	for(int i = COMM_PWR_SP; i <= COMM_GRID_FB; i++)
	{
		comm_tx_out[i].f = get_monitored_float_val(i);
		comm_check += comm_tx_out[i].u;
	}

	comm_tx_out[COMM_TX_CRC].u = comm_check;
	comm_tx_busy = true;
	HAL_UART_Transmit_IT(&huart2, (uint8_t *)comm_tx_out, NUM_COMM_TX_BYTES);
}

void comm_rx_cb()
{
	//Discard bytes until sync value is received
	if(comm_rx_recv < NUM_SYNC_BYTES && *rx_in != SYNC_VAL)
	{
		comm_rx_recv = 0;
	}
	//Save bytes until expected number is received
	else if(comm_rx_recv < NUM_COMM_RX_BYTES)
	{
		*(rx_ptr + comm_rx_recv) = *rx_in;
		comm_rx_recv++;
	}
	else
	{
		//TBD TODO could potentially add additional fault handling here
		comm_rx_recv = 0;
	}
	HAL_UART_Receive_IT(&huart2, rx_in, 1);
}

void comm_tx_cb()
{
	comm_tx_busy = false;
}
