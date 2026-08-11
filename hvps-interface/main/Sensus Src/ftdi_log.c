/**
 * ftdi_log.c
 *
 * See ftdi_log.h for usage. Implementation notes:
 *
 * - TX ring buffer: log calls copy formatted text into a circular buffer.
 *   If HAL UART TX is idle, we kick off HAL_UART_Transmit_IT() for the first
 *   chunk; the TxCpltCallback then keeps draining the buffer until empty.
 * - Thread-safety: ftdi_log_write() disables the UART interrupt briefly while
 *   touching shared buffer indices (short critical section), so it is safe
 *   to call from main loop and from other (lower-priority) ISRs.
 */

#include "ftdi_log.h"
#include <stdio.h>
#include <string.h>
#include "FreeRTOS.h"
#include "task.h"
#include "cmsis_os2.h"
#include "timers.h"

#if FTDI_LOG_ENABLED /* whole file compiles to nothing if disabled */

#ifndef USE_LOGGING_TASK
#define USE_LOGGING_TASK        0
#endif

#if USE_LOGGING_TASK
#define LOGGING_TASK_STACK_WORDS            256
#define LOGGING_TASK_PRIORITY               (osPriority_t) osPriorityNormal
/* Static memory for the task */
static StackType_t logging_task_stack[LOGGING_TASK_STACK_WORDS];
static StaticTask_t logging_task_buffer;

static void logging_task(void *argument)
{
	(void)argument;

	for (;;)
	{
        ftdi_log_status_json();
        vTaskDelay(pdMS_TO_TICKS(500));
    }
}
#endif /* USE_LOGGING_TASK */

extern volatile bool ftdi_tx_busy;

/* ------------------------- Internal state -------------------------------- */

static UART_HandleTypeDef *s_huart = NULL;
static volatile uint8_t s_logEnabled = 1; /* enabled by default */

/* TX ring buffer */
static volatile uint8_t  s_txBuf[FTDI_LOG_TX_BUF_SIZE];
static volatile uint16_t s_txHead = 0;   /* next free write index          */
static volatile uint16_t s_txTail = 0;   /* next byte to transmit          */

/* Single-byte holder used for HAL_UART_Transmit_IT of one byte at a time.
 * (Simplest/most portable approach; see note at bottom for a faster
 * "transmit contiguous chunk" variant.) */
static uint8_t s_txByte;

/* ------------------------- Helpers ---------------------------------------- */

static inline uint16_t tx_count(void)
{
    return (uint16_t)((s_txHead - s_txTail) & (FTDI_LOG_TX_BUF_SIZE - 1));
}

/* Push one byte into the ring buffer. Returns 0 if buffer full (byte dropped). */
static uint8_t tx_push_byte(uint8_t b)
{
    uint16_t next = (uint16_t)((s_txHead + 1) & (FTDI_LOG_TX_BUF_SIZE - 1));
    if (next == s_txTail) {
        return 0; /* full: drop byte rather than block/overflow */
    }
    s_txBuf[s_txHead] = b;
    s_txHead = next;
    return 1;
}

/* Kick off transmission of the next byte if UART is idle and data pending.
 * Must be called with UART IRQ disabled or from within the IRQ context. */
static void tx_start_if_idle(void)
{
    if (!ftdi_tx_busy && s_txTail != s_txHead) {
        ftdi_tx_busy = true;
        s_txByte = s_txBuf[s_txTail];
        s_txTail = (uint16_t)((s_txTail + 1) & (FTDI_LOG_TX_BUF_SIZE - 1));
        HAL_UART_Transmit_IT(s_huart, &s_txByte, 1);
    }
}

/* ------------------------- Public API -------------------------------------- */


void ftdi_log_init(UART_HandleTypeDef *huart)
{
    s_huart = huart;
    s_txHead = s_txTail = 0;
    ftdi_tx_busy = false;
    s_logEnabled = 1;

#if USE_LOGGING_TASK
    TaskHandle_t logging_task_handle  = xTaskCreateStatic(
        logging_task, 
        "logging task",
        LOGGING_TASK_STACK_WORDS, 
        NULL, 
        LOGGING_TASK_PRIORITY, 
        logging_task_stack,
        &logging_task_buffer); 

    configASSERT(logging_task_handle != NULL);
#endif
}

void ftdi_log_set_enabled(uint8_t enabled)
{
    s_logEnabled = enabled ? 1 : 0;
}

uint8_t ftdi_log_is_enabled(void)
{
    return s_logEnabled;
}

void ftdi_log_write(FtdiLog_Level_t level, const char *fmt, ...)
{
    static const char *prefix[] = { "[ERR] ", "[WRN] ", "[INF] ", "[DBG] " };
    char line[FTDI_LOG_LINE_MAX];
    int len;
    va_list args;

    if (!s_logEnabled) {
        return; /* logging turned off: skip formatting and buffer writes entirely */
    }

    if ((unsigned)level >= (sizeof(prefix) / sizeof(prefix[0]))) {
        return; /* guard against out-of-range level */
    }

    len = snprintf(line, sizeof(line), "[%10lu] %s", (uint32_t)runtime_ms, prefix[level]);

    va_start(args, fmt);
    len += vsnprintf(line + len, sizeof(line) - (size_t)len, fmt, args);
    va_end(args);

    if (len < 0) {
        return;
    }
    if ((size_t)len > sizeof(line) - 3) {
        len = (int)sizeof(line) - 3; /* leave room for \r\n */
    }
    line[len++] = '\r';
    line[len++] = '\n';

    /* Critical section: push whole line into ring buffer */
    __disable_irq();
    for (int i = 0; i < len; i++) {
        if (!tx_push_byte((uint8_t)line[i])) {
            break; /* buffer full: remaining bytes of this line are dropped */
        }
    }
    tx_start_if_idle();
    __enable_irq();
}

void ftdi_write(const char *fmt, ...)
{
    char line[FTDI_LOG_LINE_MAX];
    int len;
    va_list args;

    if (!s_logEnabled) {
        return;
    }

    va_start(args, fmt);
    len = vsnprintf(line, sizeof(line), fmt, args);
    va_end(args);

    if (len < 0) {
        return;
    }

    /* Clamp length to leave room for CR/LF */
    if ((size_t)len > sizeof(line) - 3) {
        len = (int)sizeof(line) - 3;
    }

    line[len++] = '\r';
    line[len++] = '\n';

    /* Critical section: push whole line into ring buffer */
    __disable_irq();

    for (int i = 0; i < len; i++) {
        if (!tx_push_byte((uint8_t)line[i])) {
            break; /* buffer full */
        }
    }

    tx_start_if_idle();

    __enable_irq();
}

void ftdi_write_bytes(const uint8_t *data, uint16_t length)
{
    if (!s_logEnabled || data == NULL || length == 0) {
        return;
    }

    __disable_irq();

    for (uint16_t i = 0; i < length; i++) {
        if (!tx_push_byte(data[i])) {
            break;
        }
    }

    tx_start_if_idle();

    __enable_irq();
}

/* ------------------------- Callback dispatch hooks --------------------------
 * NOT named HAL_UART_xxxCallback directly, to avoid "multiple definition"
 * linker errors when those weak HAL callbacks are already defined elsewhere
 * in the project (very common with CubeMX-generated usart.c). Call these
 * from your project's single set of HAL_UART_*Callback functions, guarded
 * by instance. See main_usage_example.c for exactly how to wire this up.
 *
 * If you are SURE no other file in your project defines
 * HAL_UART_TxCpltCallback / ErrorCallback, you can instead just rename
 * these functions back to the HAL names and skip the wrapper in
 * stm32f3xx_it.c / usart.c entirely.
 * --------------------------------------------------------------------------*/

void ftdi_log_on_tx_cplt(UART_HandleTypeDef *huart)
{
    if (huart->Instance != s_huart->Instance) {
        return;
    }
    ftdi_tx_busy = false;
    tx_start_if_idle(); /* send next byte if more data is queued */
}

void ftdi_log_on_error(UART_HandleTypeDef *huart)
{
    if (huart->Instance != s_huart->Instance) {
        return;
    }
    /* Clear error flags so a framing/overrun error doesn't leave the
     * peripheral in a bad state for subsequent transmits. */
    __HAL_UART_CLEAR_PEFLAG(huart);
}

/* ------------------------- JSON status logging ----------------------------
 * See ftdi_log.h for the FtdiLog_Status_t field descriptions. Edit the
 * snprintf format string / field list below to match your own struct.
 * --------------------------------------------------------------------------*/

#define FTDI_LOG_JSON_MAX   320u

extern uint32_t sys_stat;
extern uint32_t sys_io_bits;

void ftdi_log_status_json(void)
{
    char buf[FTDI_LOG_JSON_MAX];
    int len;

    if (!s_logEnabled) {
        return;
    }

    len = snprintf(buf, sizeof(buf),
        "{\"runtime_ms\":%lu,\"sys_stat\":\"0x%08lX\",\"sys_io_bits\":\"0x%08lX\"}\r\n",
        (uint32_t)runtime_ms,
        (uint32_t)sys_stat,
        (uint32_t)sys_io_bits);

    if (len < 0) {
        return;
    }
    if ((size_t)len >= sizeof(buf)) {
        len = (int)sizeof(buf) - 1; /* truncated: still send what fits */
    }

    /* Critical section: push whole JSON line into the same TX ring buffer
     * used by ftdi_log_write(), so ordering with normal log lines is
     * preserved. */
    __disable_irq();
    for (int i = 0; i < len; i++) {
        if (!tx_push_byte((uint8_t)buf[i])) {
            break; /* buffer full: remaining bytes are dropped */
        }
    }
    tx_start_if_idle();
    __enable_irq();
}

#endif /* FTDI_LOG_ENABLED */