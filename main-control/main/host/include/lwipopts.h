#ifndef GENTLEBEAM_HOST_LWIPOPTS_H
#define GENTLEBEAM_HOST_LWIPOPTS_H

/* Use the real raw-UDP declarations without an OS port or network stack. */
#define NO_SYS 1
#define NO_SYS_NO_TIMERS 1
#define SYS_LIGHTWEIGHT_PROT 0
#define LWIP_SOCKET 0
#define LWIP_NETCONN 0
#define LWIP_TCP 0
#define LWIP_UDP 1
#define LWIP_STATS 0

#endif
