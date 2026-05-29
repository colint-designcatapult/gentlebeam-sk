#include "test_framework.h"

void test_m20ft_19_running_triggers_fault(void);
void test_m20ft_19_not_running_no_fault(void);
void test_m20ft_19_already_elapsed_no_retrigger(void);
void test_m20ft_19_paused_no_fault(void);
void test_m20ft_19_running_sets_elapsed(void);
void test_m20ft_19_running_forces_fault_output_low(void);
void test_m20ft_19_multiple_handler_calls_do_not_retrigger_after_elapsed(void);
void test_m20ft_19_cleared_fault_output_stays_high(void);
void test_m20ft_19_paused_fault_output_stays_high(void);

void run_all_tests(void)
{
	tf_reset();

	tf_begin_test("test_m20ft_19_running_triggers_fault");
	test_m20ft_19_running_triggers_fault();
	tf_end_test();

	tf_begin_test("test_m20ft_19_not_running_no_fault");
	test_m20ft_19_not_running_no_fault();
	tf_end_test();

	tf_begin_test("test_m20ft_19_already_elapsed_no_retrigger");
	test_m20ft_19_already_elapsed_no_retrigger();
	tf_end_test();

	tf_begin_test("test_m20ft_19_paused_no_fault");
	test_m20ft_19_paused_no_fault();
	tf_end_test();

	tf_begin_test("test_m20ft_19_running_sets_elapsed");
	test_m20ft_19_running_sets_elapsed();
	tf_end_test();

	tf_begin_test("test_m20ft_19_running_forces_fault_output_low");
	test_m20ft_19_running_forces_fault_output_low();
	tf_end_test();

	tf_begin_test("test_m20ft_19_multiple_handler_calls_do_not_retrigger_after_elapsed");
	test_m20ft_19_multiple_handler_calls_do_not_retrigger_after_elapsed();
	tf_end_test();
	
	tf_begin_test("test_m20ft_19_cleared_fault_output_stays_high");
	test_m20ft_19_cleared_fault_output_stays_high();
	tf_end_test();

	tf_begin_test("test_m20ft_19_paused_fault_output_stays_high");
	test_m20ft_19_paused_fault_output_stays_high();
	tf_end_test();

	tf_print_summary();
}