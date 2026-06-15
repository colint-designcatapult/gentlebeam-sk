#include "test_framework.h"

volatile uint32_t g_tests_run = 0;
volatile uint32_t g_tests_failed = 0;
volatile uint32_t g_last_failed_line = 0;

volatile uint32_t g_result_count = 0;
test_result_t g_results[20];

char g_test_summary[512];

static const char* g_current_test_name = 0;
static uint8_t g_current_test_failed = 0;

static void append_char(char* buf, uint32_t* idx, char c)
{
    if (*idx < 511U)
    {
        buf[*idx] = c;
        (*idx)++;
        buf[*idx] = '\0';
    }
}

static void append_str(char* buf, uint32_t* idx, const char* s)
{
    while (*s != '\0')
    {
        append_char(buf, idx, *s);
        s++;
    }
}

void tf_reset(void)
{
    uint32_t i;

    g_tests_run = 0;
    g_tests_failed = 0;
    g_last_failed_line = 0;
    g_result_count = 0;
    g_current_test_name = 0;
    g_current_test_failed = 0;
    g_test_summary[0] = '\0';

    for (i = 0; i < 20; i++)
    {
        g_results[i].name = 0;
        g_results[i].status = FAIL;
    }
}

void tf_fail(uint32_t line)
{
    g_tests_failed++;
    g_last_failed_line = line;
    g_current_test_failed = 1;
}

void tf_begin_test(const char* name)
{
    g_current_test_name = name;
    g_current_test_failed = 0;
}

void tf_end_test(void)
{
    if (g_result_count < 20)
    {
        g_results[g_result_count].name = g_current_test_name;
        g_results[g_result_count].status =
        (g_current_test_failed == 0U) ? PASS : FAIL;
        g_result_count++;
    }
}

void tf_print_summary(void)
{
    uint32_t i;
    uint32_t idx = 0;

    g_test_summary[0] = '\0';

    for (i = 0; i < g_result_count; i++)
    {
        append_str(g_test_summary, &idx, g_results[i].name ? g_results[i].name : "(null)");
        append_str(g_test_summary, &idx, " : ");
        append_str(g_test_summary, &idx, g_results[i].status ? "PASS" : "FAIL");
        append_char(g_test_summary, &idx, '\n');
    }

    append_str(g_test_summary, &idx, "Assertions: ");
    append_char(g_test_summary, &idx, '0' + (char)(g_tests_run / 10U));
    append_char(g_test_summary, &idx, '0' + (char)(g_tests_run % 10U));
    append_char(g_test_summary, &idx, '\n');

    append_str(g_test_summary, &idx, "Failures: ");
    append_char(g_test_summary, &idx, '0' + (char)g_tests_failed);
    append_char(g_test_summary, &idx, '\0');

    /* put breakpoint here */
    {
        volatile uint32_t summary_ready = 1;
        (void)summary_ready;
    }
}