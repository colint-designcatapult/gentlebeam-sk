#pragma once

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Configure native UDP endpoints before firmware initialization. */
void host_configure_udp_ports(uint16_t command_port, uint16_t console_port,
                              uint16_t extra_port, uint16_t telemetry_port);

#ifdef __cplusplus
}
#endif
