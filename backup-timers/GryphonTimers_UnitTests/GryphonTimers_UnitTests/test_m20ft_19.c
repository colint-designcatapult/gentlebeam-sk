#include <stdint.h>
#include <stdbool.h>
#include "test_framework.h"
#include "../../gryphon_timers.h"

extern volatile bool gpio_fault_state;
extern volatile uint32_t g_stub_ovf_clear_calls;
extern volatile uint32_t g_stub_disable_calls;
extern volatile uint32_t g_stub_gpio_calls;

/* real production symbol from linked main.c */
extern volatile uint8_t timer_state;

void TC1_Handler(void);

static void reset_test_state(void)
{
	g_stub_ovf_clear_calls = 0;
	g_stub_disable_calls = 0;
	g_stub_gpio_calls = 0;
	gpio_fault_state = true;
	timer_state = TIMER_STATE_CLEARED;
}

void test_m20ft_19_running_triggers_fault(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_RUNNING;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
	TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
	TEST_ASSERT_EQ_U32(1, g_stub_gpio_calls);
	TEST_ASSERT(timer_state == TIMER_STATE_ELAPSED);
	TEST_ASSERT(gpio_fault_state == false);
}

void test_m20ft_19_not_running_no_fault(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_CLEARED;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
	TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
	TEST_ASSERT_EQ_U32(0, g_stub_gpio_calls);
	TEST_ASSERT(timer_state == TIMER_STATE_CLEARED);
	TEST_ASSERT(gpio_fault_state == true);
}

void test_m20ft_19_already_elapsed_no_retrigger(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_ELAPSED;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
	TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
	TEST_ASSERT_EQ_U32(0, g_stub_gpio_calls);
	TEST_ASSERT(timer_state == TIMER_STATE_ELAPSED);
	TEST_ASSERT(gpio_fault_state == true);
}

void test_m20ft_19_paused_no_fault(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_PAUSED;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
	TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
	TEST_ASSERT_EQ_U32(0, g_stub_gpio_calls);
	TEST_ASSERT(timer_state == TIMER_STATE_PAUSED);
	TEST_ASSERT(gpio_fault_state == true);
}

void test_m20ft_19_running_sets_elapsed(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_RUNNING;

	TC1_Handler();

	TEST_ASSERT(timer_state == TIMER_STATE_ELAPSED);
}

void test_m20ft_19_running_forces_fault_output_low(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_RUNNING;
	gpio_fault_state = true;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_gpio_calls);
	TEST_ASSERT(gpio_fault_state == false);
}

void test_m20ft_19_multiple_handler_calls_do_not_retrigger_after_elapsed(void)
{
	reset_test_state();

	timer_state = TIMER_STATE_RUNNING;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_gpio_calls);
	TEST_ASSERT(timer_state == TIMER_STATE_ELAPSED);
	TEST_ASSERT(gpio_fault_state == false);

	/* prepare for second call */
	g_stub_ovf_clear_calls = 0;
	g_stub_disable_calls = 0;
	g_stub_gpio_calls = 0;
	gpio_fault_state = true;

	TC1_Handler();

	TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
	TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
	TEST_ASSERT_EQ_U32(0, g_stub_gpio_calls);
	TEST_ASSERT(timer_state == TIMER_STATE_ELAPSED);
	TEST_ASSERT(gpio_fault_state == true);
}

void test_m20ft_19_cleared_fault_output_stays_high(void)
{
    reset_test_state();

    timer_state = TIMER_STATE_CLEARED;
    gpio_fault_state = true;

    TC1_Handler();

    TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
    TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
    TEST_ASSERT_EQ_U32(0, g_stub_gpio_calls);
    TEST_ASSERT(timer_state == TIMER_STATE_CLEARED);
    TEST_ASSERT(gpio_fault_state == true);
}

void test_m20ft_19_paused_fault_output_stays_high(void)
{
    reset_test_state();

    timer_state = TIMER_STATE_PAUSED;
    gpio_fault_state = true;

    TC1_Handler();

    TEST_ASSERT_EQ_U32(1, g_stub_ovf_clear_calls);
    TEST_ASSERT_EQ_U32(1, g_stub_disable_calls);
    TEST_ASSERT_EQ_U32(0, g_stub_gpio_calls);
    TEST_ASSERT(timer_state == TIMER_STATE_PAUSED);
    TEST_ASSERT(gpio_fault_state == true);
}