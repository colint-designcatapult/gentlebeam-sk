/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: PC communications parsing
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include <stdio.h>
#include <string.h>
#include <stdbool.h>
#include <lwip/opt.h>
#include <lwip/debug.h>
#include <lwip/stats.h>
#include <lwip/udp.h>
#include "checksum.h"
#include "pc_comm_parser.h"
#include "pc_msg_processing.h"
#include "system_parameters.h"

#include "state_machine.h"
#include "faults.h"
#include "ext_dac.h"
#include "hvps.h"
#include "head_board.h"

static struct udp_pcb *udp_base_rx_pcb;
static struct udp_pcb *udp_console_rx_pcb;
static struct udp_pcb *udp_extra_rx_pcb;

volatile bool pc_rx_buffered;
bool queue_pc_tx;
ip_addr_t last_ip_addr;
int last_port;
PacketType_t p_type = PCCOM_INVALID_PACKET;
uint32_t packet_id;

int pc_rx_buf_byte_count = 0;
uint8_t pc_rx_buffer[PC_RX_BUFFER_SIZE];
uint8_t pc_tx_buffer[PC_TX_BUFFER_SIZE];

int expected_data_count[PACKET_TYPE_COUNT];
int response_data_count[PACKET_TYPE_COUNT];

static void udp_comm_init();
static void udp_receive_callback(void *arg, struct udp_pcb *upcb,
struct pbuf *p, const ip_addr_t *addr, u16_t port);
static PacketType_t check_pc_packet();
static void send_response_packet(int byte_count, void* output_payload);


void pc_comm_init()
{
	//Set default values
	pc_rx_buffered = false;
	queue_pc_tx = false;
	last_port = 0;
	last_ip_addr.addr = 0;
	p_type = PCCOM_INVALID_PACKET;
	packet_id = 0;
	
	//Default initialize all values before individuals
	for(int i = 0; i < PACKET_TYPE_COUNT; i++)
	{
		expected_data_count[i] = 0;
		response_data_count[i] = INVALID_COUNT;
	}
	
	//Initialize expected incoming and response packet sizes
	expected_data_count[PCCOM_INVALID_PACKET] = 0;
	
	expected_data_count[PCCOM_VERSION_REQUEST] = VERSION_REQ_COUNT;
	response_data_count[PCCOM_VERSION_REQUEST] = VERSION_RES_COUNT;
	
	expected_data_count[PCCOM_FAULT_REQUEST] = FAULT_REQ_COUNT;
	response_data_count[PCCOM_FAULT_REQUEST] = FAULT_RES_COUNT;
	
	expected_data_count[PCCOM_DIRECTIVE_CMD] = DIR_CMD_COUNT;
	response_data_count[PCCOM_DIRECTIVE_CMD] = DIR_RES_COUNT;
	
	expected_data_count[PCCOM_TELEMETERY_REQUEST] = TELEMETRY_REQ_COUNT;
	response_data_count[PCCOM_TELEMETERY_REQUEST] = SS_COUNT;
	
	expected_data_count[PCCOM_CONDITION_CMD] = CONDITION_CMD_COUNT;
	response_data_count[PCCOM_CONDITION_CMD] = CONDITION_CMD_COUNT;
	
	expected_data_count[PCCOM_WARMUP_CMD] = WARMUP_CMD_COUNT;
	response_data_count[PCCOM_WARMUP_CMD] = WARMUP_CMD_COUNT;	
	
	expected_data_count[PCCOM_NEW_SESSION] = NEW_SES_CMD_COUNT;
	response_data_count[PCCOM_NEW_SESSION] = NEW_SES_CMD_COUNT;
	
	expected_data_count[PCCOM_LOAD_OP] = OP_CMD_COUNT;
	response_data_count[PCCOM_LOAD_OP] = OP_CMD_COUNT;
	
	expected_data_count[PCCOM_CONFIRM_OP] = OP_CMD_COUNT;
	response_data_count[PCCOM_CONFIRM_OP] = OP_CMD_COUNT;
	
	expected_data_count[PCCOM_QUERY_OP] = OP_REQ_COUNT;
	response_data_count[PCCOM_QUERY_OP] = OP_RES_COUNT;
	
	expected_data_count[PCCOM_TREATMENT_RELEASE] = RELEASE_CMD_COUNT;
	response_data_count[PCCOM_TREATMENT_RELEASE] = RELEASE_CMD_COUNT;
	
#if defined(CALIBRATION_MODE)
	// Calibration expected data count
	expected_data_count[CAL_COIL_CMD] = CAL_COIL_CMD_COUNT;
	response_data_count[CAL_COIL_CMD] = CAL_COIL_CMD_COUNT;

	expected_data_count[CAL_HVPS_CMD] = CAL_HVPS_CMD_COUNT;
	response_data_count[CAL_HVPS_CMD] = CAL_HVPS_CMD_COUNT;

	expected_data_count[CAL_DIRECTIVE_CMD] = CAL_DIRECTIVE_CMD_COUNT;
	response_data_count[CAL_DIRECTIVE_CMD] = CAL_DIRECTIVE_CMD_COUNT;

	expected_data_count[CAL_SP_REQ_CMD] = CAL_SP_REQ_CMD_COUNT;	
	response_data_count[CAL_SP_REQ_CMD] = CAL_SP_RES_COUNT;

	expected_data_count[CAL_MAG_REQ_CMD] = 1;
	response_data_count[CAL_MAG_REQ_CMD] = HB_NUM_MAG_CAL;
#else
	expected_data_count[PCCOM_IMAGING_RELEASE] = RELEASE_CMD_COUNT;
	response_data_count[PCCOM_IMAGING_RELEASE] = RELEASE_CMD_COUNT;
	
	expected_data_count[PCCOM_IMAGING_BUTTON_WAIT] = RELEASE_CMD_COUNT;
	response_data_count[PCCOM_IMAGING_BUTTON_WAIT] = RELEASE_CMD_COUNT;
	
	expected_data_count[PCCOM_QC_PING] = QC_REQ_COUNT;
	response_data_count[PCCOM_QC_PING] = QC_RES_COUNT;
	
	expected_data_count[PCCOM_QC_READING] = QC_REQ_COUNT;
	response_data_count[PCCOM_QC_READING] = QC_RES_COUNT;
	
	/*
	expected_data_count[CAL_COIL_CMD] = CAL_COIL_CMD_COUNT;
	expected_data_count[CAL_HVPS_CMD] = CAL_HVPS_CMD_COUNT;
	expected_data_count[CAL_DIRECTIVE_CMD] = CAL_DIRECTIVE_CMD_COUNT;
	expected_data_count[CAL_SP_REQ_CMD] = CAL_SP_REQ_CMD_COUNT;
	expected_data_count[CAL_MAG_REQ_CMD] = 1;
	response_data_count[CAL_COIL_CMD] = CAL_COIL_CMD_COUNT;
	response_data_count[CAL_HVPS_CMD] = CAL_HVPS_CMD_COUNT;
	expected_data_count[CAL_DIRECTIVE_CMD] = CAL_DIRECTIVE_CMD_COUNT;
	response_data_count[CAL_SP_REQ_CMD] = CAL_SP_RES_COUNT;
	response_data_count[CAL_MAG_REQ_CMD] = HB_NUM_MAG_CAL;
	*/
#endif
	
	init_response_pointers();
	
	uint64_t *sync_output;
	sync_output = (uint64_t*)pc_tx_buffer;
	*sync_output = PC_SYNC_VAL;
	
	init_crc32_tab();
	
	udp_comm_init();
}


static void udp_comm_init()
{
	//Create new pcb's for incoming datagrams
	udp_base_rx_pcb = udp_new();
	udp_console_rx_pcb = udp_new();
	udp_extra_rx_pcb = udp_new();
	
	//Set up pcb for base
	if (udp_base_rx_pcb != NULL) {
		err_t err;
		
		ip_set_option(udp_base_rx_pcb, SOF_BROADCAST);
		
		//Bind to base port (use any IP)
		err = udp_bind(udp_base_rx_pcb, IP_ADDR_ANY, (u16_t)network_config[NETWORK_BASE_PORT]);
		
		if(err == ERR_OK)
		{
			//Assign receive callback
			udp_recv(udp_base_rx_pcb, udp_receive_callback, NULL);
		}
		else
		{
			//TODO: Go into error loop
		}
	}
	else
	{
		//TODO: Go into error loop
	}
	
	//Set up pcb for console
	if (udp_console_rx_pcb != NULL) {
		err_t err;
		
		ip_set_option(udp_console_rx_pcb, SOF_BROADCAST);
		
		//Bind to base port (use any IP)
		err = udp_bind(udp_console_rx_pcb, IP_ADDR_ANY, (u16_t)network_config[NETWORK_CONSOLE_PORT]);
		
		if(err == ERR_OK)
		{
			//Assign receive callback
			udp_recv(udp_console_rx_pcb, udp_receive_callback, NULL);
		}
		else
		{
			//TODO: Go into error loop
		}
	}
	else
	{
		//TODO: Go into error loop
	}
	
	//Set up pcb for console
	if (udp_extra_rx_pcb != NULL) {
		err_t err;
		
		ip_set_option(udp_extra_rx_pcb, SOF_BROADCAST);
		
		//Bind to base port (use any IP)
		err = udp_bind(udp_extra_rx_pcb, IP_ADDR_ANY, (u16_t)network_config[NETWORK_EXTRA_PORT]);
		
		if(err == ERR_OK)
		{
			//Assign receive callback
			udp_recv(udp_extra_rx_pcb, udp_receive_callback, NULL);
		}
		else
		{
			//TODO: Go into error loop
		}
	}
	else
	{
		//TODO: Go into error loop
	}
}


static void udp_receive_callback(void *arg, struct udp_pcb *upcb,
struct pbuf *p, const ip_addr_t *addr, u16_t port)
{
	LWIP_UNUSED_ARG(arg);
	if(p == NULL) return;
	
	
	//Verify that port is valid
	if(port != (u16_t)network_config[NETWORK_BASE_PORT] && port != (u16_t)network_config[NETWORK_CONSOLE_PORT] && port!= (u16_t)network_config[NETWORK_EXTRA_PORT])
	{
		pbuf_free(p);
		return;
	}
	
	//Verify that an existing message is not already buffered
	if(pc_rx_buffered)
	{
		pbuf_free(p);
		return;
	}
	
	//Verify that data will not overflow RX buffer
	if(p->len >= PC_RX_BUFFER_SIZE /*|| p->len < PC_MIN_PACKET_SIZE*/)
	{
		pbuf_free(p);
		return;
	}
	
	//Verify that payload is not null
	if(p->payload == NULL)
	{
		pbuf_free(p);
		return;
	}
	
	if(p->len == 4)
	{
		uint8_t* testval = (uint8_t *)p->payload;
		
		if(*testval == 0)
		{
			queue_sm_event(EVENT_ENTER_BOOTLOADER);
		}
	}
	
	//Copy payload to rx buffer for parsing
	memcpy(pc_rx_buffer, p->payload, (size_t)p->len);
	
	//Set variables for parsing
	last_ip_addr = *addr;
	last_port = (int)port;
	pc_rx_buf_byte_count = (int)p->len;
	pc_rx_buffered = true;
	pbuf_free(p);
}


void process_pc_comm()
{
	//If no packet is buffered, just return
	if(!pc_rx_buffered) return;
	
	//Verify packet type
	p_type = check_pc_packet();
	
	//Process packet data
	process_command(p_type, (void*)(pc_rx_buffer+PC_PACKET_DATA_POS));
	
	queue_pc_tx = true;
	pc_rx_buffered = false;
}


//Checks received packet for validity
static PacketType_t check_pc_packet()
{
	//Extract packet type
	uint32_t *header_val = (uint32_t*)(pc_rx_buffer+PC_PACKET_TYPE_POS);
	PacketType_t packet_type = (PacketType_t)(*header_val);
	
	//Extract and save packet ID
	header_val = (uint32_t*)(pc_rx_buffer+PC_PACKET_ID_POS);
	packet_id = *header_val;
	
	//Check for valid sync values in header
	uint64_t *sync_check;
	sync_check = (uint64_t*)pc_rx_buffer;
	if(*sync_check != PC_SYNC_VAL)
	{
		packet_type = PCCOM_INVALID_PACKET;
	}
	
	//Make sure a valid type is received
	if(packet_type < PCCOM_INVALID_PACKET || packet_type >= PACKET_TYPE_COUNT)
	{
		packet_type = PCCOM_INVALID_PACKET;
	}
	
	//Make sure reported data count corresponds with reported type
	int *packet_data_count = (int *)(pc_rx_buffer+PC_PACKET_COUNT_POS);
	if(*packet_data_count != expected_data_count[(int)packet_type])
	{
		packet_type = PCCOM_INVALID_PACKET;
	}
	
	//Make sure reported data count matches received packet size
	int byte_count = (*packet_data_count) * sizeof(uint32_t);
	if(pc_rx_buf_byte_count != (byte_count + PC_MIN_PACKET_SIZE))
	{
		packet_type = PCCOM_INVALID_PACKET;
	}
	
	//Make sure CRC is valid
	uint32_t crc_val = crc_32(pc_rx_buffer, pc_rx_buf_byte_count-sizeof(uint32_t));
	uint32_t *given_crc = (uint32_t *)(pc_rx_buffer+pc_rx_buf_byte_count-sizeof(uint32_t));
	if(crc_val != *given_crc)
	{
		packet_type = PCCOM_INVALID_PACKET;
	}
	
	return packet_type;
}

void send_pc_response()
{
	//Check to see if PC is waiting on response
	if(!queue_pc_tx) return;
	queue_pc_tx = false;
	
	//Get response data
	void *output_data = get_response_data(p_type);
	if(output_data == NULL) return;
	
	//Copy all data bytes to the TX buffer
	memcpy(pc_tx_buffer+PC_PACKET_DATA_POS, output_data, response_data_count[(int)p_type] * sizeof(uint32_t));
	
	//Write header information
	uint32_t *header_value = (uint32_t *)(pc_tx_buffer+PC_PACKET_COUNT_POS);
	*header_value = (response_data_count[(int)p_type]);
	
	header_value = (uint32_t *)(pc_tx_buffer+PC_PACKET_TYPE_POS);
	*header_value = (uint32_t)p_type + PC_RESPONSE_TYPE_OFFSET;
	
	header_value = (uint32_t *)(pc_tx_buffer+PC_PACKET_ID_POS);
	*header_value = packet_id;
	
	//Get total number response bytes including header and CRC footer
	int output_byte_count = response_data_count[(int)p_type] * sizeof(uint32_t);
	output_byte_count += PC_MIN_PACKET_SIZE;
	
	//Calculate CRC and write value to last 4 bytes of output
	uint32_t *crc_val = (uint32_t*)(pc_tx_buffer+output_byte_count-sizeof(uint32_t));
	*crc_val = crc_32(pc_tx_buffer, output_byte_count-sizeof(uint32_t));
	
	send_response_packet(output_byte_count, (void *)pc_tx_buffer);
}

//Respond to the PC with the given payload
static void send_response_packet(int byte_count, void* output_payload)
{
	struct udp_pcb *upcb;
	struct pbuf *p = pbuf_alloc(PBUF_TRANSPORT, byte_count, PBUF_RAM);
	
	if(p == NULL) {
		return;
	}
	
	//Copy payload for transmission
	memcpy(p->payload, pc_tx_buffer, byte_count);
	
	//Respond to the port which sent the initial command
	if(last_port == network_config[NETWORK_BASE_PORT])
	{
		upcb = udp_base_rx_pcb;
	}
	else if(last_port == network_config[NETWORK_CONSOLE_PORT])
	{
		upcb = udp_console_rx_pcb;
	}
	else if(last_port == network_config[NETWORK_EXTRA_PORT])
	{
		upcb = udp_extra_rx_pcb;
	}
	else
	{
		//Error, ignore and return
		return;
	}
	
	udp_sendto(upcb, p, IP_ADDR_BROADCAST, (u16_t)last_port);
	//udp_sendto(upcb, p, ((ip_addr_t *)&last_ip_addr), (u16_t)last_port);
	pbuf_free(p);
}
