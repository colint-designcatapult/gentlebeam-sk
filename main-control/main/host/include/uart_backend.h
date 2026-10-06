#pragma once

#include <stdint.h>

#ifdef __cplusplus
class IoDescriptor;
extern "C" {
#else
typedef void IoDescriptor; 
#endif

IoDescriptor* uart_create_tcp_descriptor(uint16_t port);
void uart_destroy_descriptor(IoDescriptor* descriptor);
void uart_set_descriptor_instance(struct usart_async_descriptor* descr, IoDescriptor* instance);
void uart_tick_1ms();

#ifdef __cplusplus
}
#endif