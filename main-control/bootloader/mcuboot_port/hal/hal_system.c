#include "hal/hal_system.h"

#include <sam.h>

void hal_system_reset(void)
{
	NVIC_SystemReset();
}
