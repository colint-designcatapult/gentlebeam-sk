#if !defined(_WIN32) && !defined(_POSIX_C_SOURCE)
#define _POSIX_C_SOURCE 200809L
#endif

#include <stddef.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#if defined(_WIN32)
#include <winsock2.h>
#include <ws2tcpip.h>
typedef SOCKET host_socket_t;
#define HOST_INVALID_SOCKET INVALID_SOCKET
#else
#include <arpa/inet.h>
#include <fcntl.h>
#include <sys/socket.h>
#include <unistd.h>
typedef int host_socket_t;
#define HOST_INVALID_SOCKET (-1)
#endif

#include <ff.h>
#include <lwip/sys.h>
#include <lwip/udp.h>
#include "custom_eth_ipstack_main.h"
#include "pc_msg_processing.h"
#include "atmel_start.h"
#include "network_backend.h"

typedef struct host_udp_pcb {
	struct udp_pcb pcb;
	host_socket_t socket_fd;
	struct host_udp_pcb *next;
} host_udp_pcb_t;

static host_udp_pcb_t *host_udp_pcbs;
struct udp_pcb *udp_pcbs;
static bool sockets_initialized;
static uint8_t receive_buffer[UINT16_MAX];
static uint16_t host_command_port = DEFAULT_BASE_PORT;
static uint16_t host_console_port = DEFAULT_CONSOLE_PORT;
static uint16_t host_extra_port = DEFAULT_EXTRA_PORT;
static uint16_t host_telemetry_port = PC_TELEMETRY_PORT;

void host_configure_udp_ports(uint16_t command_port, uint16_t console_port,
                              uint16_t extra_port, uint16_t telemetry_port)
{
	host_command_port = command_port;
	host_console_port = console_port;
	host_extra_port = extra_port;
	host_telemetry_port = telemetry_port;
}

static u16_t lwip_htons(u16_t n)
{
	return ((n & 0xff) << 8) | ((n & 0xff00) >> 8);
}

static u16_t lwip_ntohs(u16_t n)
{
	return lwip_htons(n);
}

u32_t lwip_htonl(u32_t n)
{
	return ((n & 0xff) << 24) | ((n & 0xff00) << 8) | ((n & 0xff0000UL) >> 8) | ((n & 0xff000000UL) >> 24);
}

static bool initialize_sockets(void)
{
#if defined(_WIN32)
	WSADATA wsa_data;

	if (WSAStartup(MAKEWORD(2, 2), &wsa_data) != 0) {
		return false;
	}
#endif
	sockets_initialized = true;
	return true;
}

static host_udp_pcb_t *host_udp_pcb_from_lwip(struct udp_pcb *pcb)
{
	for (host_udp_pcb_t *node = host_udp_pcbs; node != NULL; node = node->next) {
		if (&node->pcb == pcb) {
			return node;
		}
	}
	return NULL;
}

static bool set_socket_nonblocking(host_socket_t socket_fd)
{
#if defined(_WIN32)
	u_long enabled = 1;

	return ioctlsocket(socket_fd, FIONBIO, &enabled) == 0;
#else
	const int flags = fcntl(socket_fd, F_GETFL, 0);

	return flags >= 0 && fcntl(socket_fd, F_SETFL, flags | O_NONBLOCK) == 0;
#endif
}


void eth_ipstack_init(void)
{
	if (!sockets_initialized) {
		(void)initialize_sockets();
	}
}

/* Host UDP uses the native socket stack while retaining lwIP's public API. */
const ip_addr_t ip_addr_any = {0};
const ip_addr_t ip_addr_broadcast = {UINT32_MAX};

struct udp_pcb *udp_new(void)
{
	host_udp_pcb_t *node;

	if (!sockets_initialized && !initialize_sockets()) {
		return NULL;
	}
	node = calloc(1, sizeof(*node));
	if (node == NULL) {
		return NULL;
	}
	node->socket_fd = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (node->socket_fd == HOST_INVALID_SOCKET || !set_socket_nonblocking(node->socket_fd)) {
#if defined(_WIN32)
		closesocket(node->socket_fd);
#else
		close(node->socket_fd);
#endif
		free(node);
		return NULL;
	}
	node->pcb.next = udp_pcbs;
	udp_pcbs = &node->pcb;
	node->next = host_udp_pcbs;
	host_udp_pcbs = node;
	return &node->pcb;
}

err_t udp_bind(struct udp_pcb *pcb, ip_addr_t *address, u16_t port)
{
	host_udp_pcb_t *const node = host_udp_pcb_from_lwip(pcb);
	struct sockaddr_in local_address = {0};
	uint16_t native_port = port;

	if (node == NULL) {
		return ERR_ARG;
	}
	(void)address;
	local_address.sin_family = AF_INET;
	/* Preserve firmware's logical ports when remapping native host sockets. */
	if (port == DEFAULT_BASE_PORT) {
		native_port = host_command_port;
	} else if (port == DEFAULT_CONSOLE_PORT) {
		native_port = host_console_port;
	} else if (port == DEFAULT_EXTRA_PORT) {
		native_port = host_extra_port;
	}
	local_address.sin_port = htons(native_port);
	local_address.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	if (bind(node->socket_fd, (struct sockaddr *)&local_address, sizeof(local_address)) != 0) {
		return ERR_USE;
	}
	/* The firmware receive callback validates this against network_config. */
	pcb->local_port = port;
	return ERR_OK;
}

void udp_recv(struct udp_pcb *pcb, udp_recv_fn receive, void *argument)
{
	if (pcb != NULL) {
		pcb->recv = receive;
		pcb->recv_arg = argument;
	}
}

err_t udp_sendto(struct udp_pcb *pcb, struct pbuf *packet,
                 ip_addr_t *address, u16_t port)
{
	host_udp_pcb_t *const node = host_udp_pcb_from_lwip(pcb);
	struct sockaddr_in destination = {0};
	uint8_t *payload;
	size_t packet_length = 0;
	int sent;

	if (node == NULL || packet == NULL || address == NULL) {
		return ERR_ARG;
	}
	for (struct pbuf *part = packet; part != NULL; part = part->next) {
		packet_length += part->len;
	}
	if (packet_length > UINT16_MAX) {
		return ERR_BUF;
	}
	if (packet->next == NULL) {
		payload = packet->payload;
	} else {
		size_t offset = 0;

		payload = malloc(packet_length);
		if (payload == NULL) {
			return ERR_MEM;
		}
		for (struct pbuf *part = packet; part != NULL; part = part->next) {
			memcpy(payload + offset, part->payload, part->len);
			offset += part->len;
		}
	}
	/* Host traffic is intentionally isolated from physical network interfaces. */
	destination.sin_family = AF_INET;
	/* Only unsolicited broadcasts are telemetry; unicast reply ports are untouched. */
	destination.sin_port = htons(address->addr == IP_ADDR_BROADCAST->addr && port == PC_TELEMETRY_PORT
	                             ? host_telemetry_port : port);
	destination.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	sent = sendto(node->socket_fd, (const char *)payload, (int)packet_length, 0,
	              (struct sockaddr *)&destination, sizeof(destination));
	if (packet->next != NULL) {
		free(payload);
	}
	return sent == (int)packet_length ? ERR_OK : ERR_IF;
}

struct pbuf *pbuf_alloc(pbuf_layer layer, u16_t length, pbuf_type type)
{
	struct pbuf *packet;

	(void)layer;
	packet = calloc(1, sizeof(*packet) + length);
	if (packet == NULL) {
		return NULL;
	}
	packet->payload = packet + 1;
	packet->tot_len = length;
	packet->len = length;
	packet->type = (u8_t)type;
	packet->ref = 1;
	return packet;
}

u8_t pbuf_free(struct pbuf *packet)
{
	u8_t freed = 0;

	while (packet != NULL) {
		struct pbuf *const next = packet->next;

		if (packet->ref > 1) {
			packet->ref--;
			break;
		}
		free(packet);
		freed++;
		packet = next;
	}
	return freed;
}

void eth_ipstack_poll(void)
{
	for (host_udp_pcb_t *node = host_udp_pcbs; node != NULL; node = node->next) {
		for (;;) {
			struct sockaddr_in sender = {0};
			ip_addr_t sender_address;
			struct pbuf *packet;
#if defined(_WIN32)
			int sender_length = sizeof(sender);
			const int received = recvfrom(node->socket_fd, (char *)receive_buffer,
			                              sizeof(receive_buffer), 0,
			                              (struct sockaddr *)&sender, &sender_length);
#else
			socklen_t sender_length = sizeof(sender);
			const ssize_t received = recvfrom(node->socket_fd, receive_buffer,
			                                  sizeof(receive_buffer), 0,
			                                  (struct sockaddr *)&sender, &sender_length);
#endif

			if (received < 0) {
				break;
			}
			if (MACIF.cb.receive != NULL) {
				MACIF.cb.receive(&MACIF);
			}
			if (node->pcb.recv == NULL) {
				continue;
			}
			packet = pbuf_alloc(PBUF_RAW, (u16_t)received, PBUF_RAM);
			if (packet == NULL) {
				break;
			}
			memcpy(packet->payload, receive_buffer, (size_t)received);
			sender_address.addr = sender.sin_addr.s_addr;
			node->pcb.recv(node->pcb.recv_arg, &node->pcb, packet,
			               &sender_address, lwip_ntohs(sender.sin_port));
		}
	}
}


FRESULT f_mount(FATFS *filesystem, const TCHAR *path, BYTE option)
{
    (void)filesystem;
    (void)path;
    (void)option;
    return FR_NOT_READY;
}

void use_default_network_settings(void)
{
    /* Values come from the same defaults used by the firmware MAC adapter. */
    network_config[NETWORK_BASE_PORT] = DEFAULT_BASE_PORT;
    network_config[NETWORK_CONSOLE_PORT] = DEFAULT_CONSOLE_PORT;
    network_config[NETWORK_EXTRA_PORT] = DEFAULT_EXTRA_PORT;
    network_config[NETWORK_IP_0] = DEFAULT_IP_0;
    network_config[NETWORK_IP_1] = DEFAULT_IP_1;
    network_config[NETWORK_IP_2] = DEFAULT_IP_2;
    network_config[NETWORK_IP_3] = DEFAULT_IP_3;
    network_config[NETWORK_SUBNET_0] = DEFAULT_SUBNET_0;
    network_config[NETWORK_SUBNET_1] = DEFAULT_SUBNET_1;
    network_config[NETWORK_SUBNET_2] = DEFAULT_SUBNET_2;
    network_config[NETWORK_SUBNET_3] = DEFAULT_SUBNET_3;
    network_config[NETWORK_GATEWAY_0] = DEFAULT_GATEWAY_0;
    network_config[NETWORK_GATEWAY_1] = DEFAULT_GATEWAY_1;
    network_config[NETWORK_GATEWAY_2] = DEFAULT_GATEWAY_2;
    network_config[NETWORK_GATEWAY_3] = DEFAULT_GATEWAY_3;
}

int32_t mac_async_register_callback(struct mac_async_descriptor *const descr, const enum mac_async_cb_type type,
                                    const FUNC_PTR func)
{
    if (descr == NULL) {
        return -1;
    }

    switch (type) {
    case MAC_ASYNC_RECEIVE_CB:
        descr->cb.receive = (mac_async_cb_t)func;
        return ERR_NONE;
    case MAC_ASYNC_TRANSMIT_CB:
        descr->cb.transmit = (mac_async_cb_t)func;
        return ERR_NONE;
    default:
        return -1;
    }
}

struct mac_async_descriptor MACIF;
struct ethernet_phy_descriptor MACIF_PHY_desc;
