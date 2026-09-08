#include "os/os_cputime.h"

#include <peripheral_clk_config.h>

void os_cputime_delay_usecs(uint32_t usecs)
{
	/* Rough cycle-counted busy wait; good enough for the one-shot delay
	 * before a serial-recovery triggered reset. */
	uint32_t cycles_per_usec = CONF_CPU_FREQUENCY / 1000000u;
	uint32_t loops = usecs * cycles_per_usec / 4u;

	while (loops--) {
		__asm volatile("nop");
	}
}
