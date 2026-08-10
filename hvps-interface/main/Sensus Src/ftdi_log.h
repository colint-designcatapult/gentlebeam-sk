/**
 * ftdi_log.h
 *
 * Interrupt-driven TX-only logging over USART3 for STM32F373.
 * TX is fully interrupt-driven via a ring buffer, so log calls never block
 * waiting for the wire.
 *
 * Usage:
 *   1. Configure USART3 in CubeMX/HAL as normal (baud rate, pins PB10/PB11
 *      or PC10/PC11 depending on your board, NVIC interrupt enabled).
 *   2. Call ftdi_log_init(&huart3) once after MX_USART3_UART_Init().
 *   3. Call LOG_INFO("value=%d", x); from anywhere (not from very high
 *      priority ISRs unless you accept the ring buffer overflow risk).
 *   4. Make sure USART3_IRQHandler() in stm32f3xx_it.c calls
 *      HAL_UART_IRQHandler(&huart3) (see notes at bottom of ftdi_log.c).
 */

#ifndef FTDI_LOG_H
#define FTDI_LOG_H

#ifdef __cplusplus
extern "C" {
#endif

#include "stm32f3xx_hal.h"
#include <stdarg.h>
#include <stdint.h>

/* ------------------------- Configuration ------------------------------- */

/* Master compile-time switch. Set to 0 to strip ALL logging code out of the
 * build (LOG_x macros become no-ops, ftdi_log_* calls compile away to
 * nothing) -- e.g. for release builds where you don't want the UART/ISR
 * overhead at all. Set to 1 for normal operation (runtime on/off via
 * ftdi_log_set_enabled() still applies on top of this). */
#ifndef FTDI_LOG_ENABLED
#define FTDI_LOG_ENABLED        1
#endif

#define FTDI_LOG_TX_BUF_SIZE   1024u   /* ring buffer size, power of 2 recommended */
#define FTDI_LOG_LINE_MAX      256u    /* max length of a single formatted log line */

typedef enum {
    LOG_LEVEL_ERROR = 0,
    LOG_LEVEL_WARN,
    LOG_LEVEL_INFO,
    LOG_LEVEL_DEBUG
} FtdiLog_Level_t;

/* Compile-time filter: messages above this level are compiled out */
#define FTDI_LOG_MAX_LEVEL     LOG_LEVEL_DEBUG

/* ------------------------- Public API ----------------------------------- */

#if FTDI_LOG_ENABLED

/* Call once after MX_USART3_UART_Init() */
void ftdi_log_init(UART_HandleTypeDef *huart);

/* Runtime on/off switch. Logging is enabled by default after init.
 * When disabled, ftdi_log_write() returns immediately (no formatting,
 * no buffer writes) -- cheap to leave calls in place throughout code. */
void ftdi_log_set_enabled(uint8_t enabled);
uint8_t ftdi_log_is_enabled(void);

/* Core logging function (usually called via LOG_x macros below) */
void ftdi_log_write(FtdiLog_Level_t level, const char *fmt, ...);
void ftdi_write(const char *fmt, ...);
void ftdi_write_bytes(const uint8_t *data, uint16_t length); 

/* ------------------------- Callback dispatch hooks -----------------------
 * These are NOT named HAL_UART_xxxCallback on purpose, to avoid multiple-
 * definition linker errors if your project already defines those weak HAL
 * callbacks elsewhere (e.g. in usart.c). Call these from your project's
 * single HAL_UART_TxCpltCallback / HAL_UART_ErrorCallback, guarded by
 * instance == USART3. See main_usage_example.c for the pattern.
 * If you don't already define those callbacks anywhere else, you can skip
 * the wrapper and just rename these back to the HAL names directly.
 * ------------------------------------------------------------------------*/
void ftdi_log_on_tx_cplt(UART_HandleTypeDef *huart);
void ftdi_log_on_error(UART_HandleTypeDef *huart);

/* ------------------------- JSON status logging ---------------------------
 * Logs sys_stat / sys_io_bits as a single line of JSON (newline-delimited
 * JSON / NDJSON), so a host-side script/log tool can parse it easily.
 * Bypasses the [LVL] prefix and level filter used by LOG_x(); still
 * respects the runtime ftdi_log_set_enabled() switch.
 * ------------------------------------------------------------------------*/
typedef struct {
    uint32_t sys_stat;      /* extern sys_stat    -- system status word */
    uint32_t sys_io_bits;   /* extern sys_io_bits -- I/O bitfield        */
} FtdiLog_Status_t;

void ftdi_log_status_json(void);

#else /* !FTDI_LOG_ENABLED -- stub everything out to nothing */

#define ftdi_log_init(huart)              ((void)0)
#define ftdi_log_set_enabled(enabled)     ((void)0)
#define ftdi_log_is_enabled()             (0)
#define ftdi_log_write(level, fmt, ...)   ((void)0)
#define ftdi_log_on_tx_cplt(huart)        ((void)0)
#define ftdi_log_on_error(huart)          ((void)0)
#define ftdi_log_status_json(status)      ((void)0)

#endif /* FTDI_LOG_ENABLED */

/* ------------------------- Convenience macros --------------------------- */

#if FTDI_LOG_ENABLED

#if FTDI_LOG_MAX_LEVEL >= LOG_LEVEL_ERROR
#define LOG_ERROR(fmt, ...) ftdi_log_write(LOG_LEVEL_ERROR, fmt, ##__VA_ARGS__)
#else
#define LOG_ERROR(fmt, ...)
#endif

#if FTDI_LOG_MAX_LEVEL >= LOG_LEVEL_WARN
#define LOG_WARN(fmt, ...)  ftdi_log_write(LOG_LEVEL_WARN,  fmt, ##__VA_ARGS__)
#else
#define LOG_WARN(fmt, ...)
#endif

#if FTDI_LOG_MAX_LEVEL >= LOG_LEVEL_INFO
#define LOG_INFO(fmt, ...)  ftdi_log_write(LOG_LEVEL_INFO,  fmt, ##__VA_ARGS__)
#else
#define LOG_INFO(fmt, ...)
#endif

#if FTDI_LOG_MAX_LEVEL >= LOG_LEVEL_DEBUG
#define LOG_DEBUG(fmt, ...) ftdi_log_write(LOG_LEVEL_DEBUG, fmt, ##__VA_ARGS__)
#else
#define LOG_DEBUG(fmt, ...)
#endif

#else /* !FTDI_LOG_ENABLED -- all log macros compile away to nothing */

#define LOG_ERROR(fmt, ...)
#define LOG_WARN(fmt, ...)
#define LOG_INFO(fmt, ...)
#define LOG_DEBUG(fmt, ...)

#endif /* FTDI_LOG_ENABLED */

#ifdef __cplusplus
}
#endif

#endif /* FTDI_LOG_H */
