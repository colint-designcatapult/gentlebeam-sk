#include <stdint.h>
#include <stdbool.h>

/* unit test stub state only */
volatile uint32_t g_stub_ovf_clear_calls = 0;
volatile uint32_t g_stub_disable_calls = 0;
volatile uint32_t g_stub_gpio_calls = 0;
volatile uint32_t g_stub_count_reg = 0;
volatile bool gpio_fault_state = true;