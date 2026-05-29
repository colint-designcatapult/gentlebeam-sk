#ifndef TEST_FRAMEWORK_H
#define TEST_FRAMEWORK_H

#include <stdint.h>

extern volatile uint32_t g_tests_run;
extern volatile uint32_t g_tests_failed;
extern volatile uint32_t g_last_failed_line;

extern char g_test_summary[512];

void tf_reset(void);
void tf_fail(uint32_t line);
void tf_begin_test(const char* name);
void tf_end_test(void);
void tf_print_summary(void);

typedef enum
{
	FAIL = 0,
	PASS = 1
} test_status_t;

typedef struct
{
	const char* name;
	test_status_t status;
} test_result_t;



extern volatile uint32_t g_result_count;
extern test_result_t g_results[20];

#define TEST_ASSERT(cond) \
    do { \
        g_tests_run++; \
        if (!(cond)) { \
            tf_fail((uint32_t)__LINE__); \
            return; \
        } \
    } while (0)

#define TEST_ASSERT_EQ_U32(exp, act) \
    do { \
        g_tests_run++; \
        if ((uint32_t)(exp) != (uint32_t)(act)) { \
            tf_fail((uint32_t)__LINE__); \
            return; \
        } \
    } while (0)

#endif