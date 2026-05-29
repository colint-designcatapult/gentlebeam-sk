#include <atmel_start.h>

#include <hal_mac_async.h>
#include <lwip_macif_config.h>
#include <ethif_mac.h>
#include <netif/etharp.h>
#include <lwip/dhcp.h>
#include <string.h>
#include "pc_msg_processing.h"
#include "pc_comm_parser.h"
#include "custom_eth_ipstack_main.h"

struct netif my_LWIP_MACIF_desc;
static u8_t  my_LWIP_MACIF_hwaddr[6];

/**
 * Should be called at the beginning of the program to set up the
 * network interface. It calls the function mac_low_level_init() to do the
 * actual setup of the hardware.
 *
 * This function should be passed as a parameter to netif_add().
 *
 * @param netif the lwip network interface structure for this ethernetif
 * @return ERR_OK  if the loopif is initialized
 */
err_t my_LWIP_MACIF_stack_init(struct netif *netif)
{
	LWIP_ASSERT("netif != NULL", (netif != NULL));
	LWIP_ASSERT("netif->state != NULL", (netif->state != NULL));

	netif->output     = etharp_output;
	netif->linkoutput = mac_low_level_output;

	/* device capabilities */
	my_LWIP_MACIF_desc.flags = CONF_LWIP_MACIF_FLAG;
	my_LWIP_MACIF_desc.mtu   = CONF_LWIP_MACIF_MTU;

	/* set MAC hardware address length */
	memcpy(my_LWIP_MACIF_desc.hwaddr, my_LWIP_MACIF_hwaddr, NETIF_MAX_HWADDR_LEN);
	my_LWIP_MACIF_desc.hwaddr_len = ETHARP_HWADDR_LEN;

#if LWIP_NETIF_HOSTNAME
	/* Initialize interface hostname */
	my_LWIP_MACIF_desc.hostname = CONF_LWIP_MACIF_HOSTNAME;
#endif
	memcpy(my_LWIP_MACIF_desc.name, CONF_LWIP_MACIF_HOSTNAME_ABBR, 2);

	/* initialize the mac hardware */
	mac_low_level_init(netif);

	return ERR_OK;
}
void my_LWIP_MACIF_init(u8_t hwaddr[6])
{
	struct ip_addr ip;
	struct ip_addr nm;
	struct ip_addr gw;
	
	char ip_str[MAX_IP_STR_LENGTH] = {0};
	char subnet_str[MAX_IP_STR_LENGTH] = {0};
	char gateway_str[MAX_IP_STR_LENGTH] = {0};	
	
	//If any values are > 3 digits, set to 0
	//Prevention of buffer overrun
	for(int i = NETWORK_IP_0; i < NETWORK_CMD_COUNT; i++)
	{
		if(network_config[i] > 999)
		{
			network_config[i] = 0;
		}
	}
	
	sprintf(ip_str, "%lu.%lu.%lu.%lu", network_config[NETWORK_IP_0], network_config[NETWORK_IP_1],
		network_config[NETWORK_IP_2], network_config[NETWORK_IP_3]);
	sprintf(subnet_str, "%lu.%lu.%lu.%lu", network_config[NETWORK_SUBNET_0], network_config[NETWORK_SUBNET_1],
		network_config[NETWORK_SUBNET_2], network_config[NETWORK_SUBNET_3]);
	sprintf(gateway_str, "%lu.%lu.%lu.%lu", network_config[NETWORK_GATEWAY_0], network_config[NETWORK_GATEWAY_1],
		network_config[NETWORK_GATEWAY_2], network_config[NETWORK_GATEWAY_3]);
	
	
	ipaddr_aton(ip_str, &ip);
	ipaddr_aton(subnet_str, &nm);
	ipaddr_aton(gateway_str, &gw);
	memcpy(my_LWIP_MACIF_hwaddr, hwaddr, 6);

	netif_add(&my_LWIP_MACIF_desc, &ip, &nm, &gw, (void *)&MACIF, my_LWIP_MACIF_stack_init, ethernet_input);
}


void use_default_network_settings()
{
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
