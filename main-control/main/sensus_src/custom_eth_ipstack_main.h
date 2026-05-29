#ifndef CUSTOM_ETH_IPSTACK_MAIN_H_
#define CUSTOM_ETH_IPSTACK_MAIN_H_

#define DEFAULT_BASE_PORT		20
#define DEFAULT_CONSOLE_PORT	7
#define DEFAULT_EXTRA_PORT		35
#define DEFAULT_IP_0			172
#define DEFAULT_IP_1			31
#define DEFAULT_IP_2			1
#define DEFAULT_IP_3			100
#define DEFAULT_SUBNET_0		255
#define DEFAULT_SUBNET_1		255
#define DEFAULT_SUBNET_2		255
#define DEFAULT_SUBNET_3		0
#define DEFAULT_GATEWAY_0		172
#define DEFAULT_GATEWAY_1		31
#define DEFAULT_GATEWAY_2		1
#define DEFAULT_GATEWAY_3		1
#define MAX_IP_STR_LENGTH		16

#ifdef __cplusplus
extern "C" {
#endif

#include <lwip/init.h>

#include <ethif_mac.h>

extern struct netif my_LWIP_MACIF_desc;

void my_LWIP_MACIF_init(u8_t hwaddr[6]);
void use_default_network_settings();


#ifdef __cplusplus
}
#endif



#endif /* CUSTOM_ETH_IPSTACK_MAIN_H_ */