#ifndef __MCUBOOT_LOGGING_H__
#define __MCUBOOT_LOGGING_H__

#include <stdio.h>

#define MCUBOOT_LOG_LEVEL_OFF     0
#define MCUBOOT_LOG_LEVEL_ERROR   1
#define MCUBOOT_LOG_LEVEL_WARNING 2
#define MCUBOOT_LOG_LEVEL_INFO    3
#define MCUBOOT_LOG_LEVEL_DEBUG   4

#ifndef MCUBOOT_LOG_LEVEL
#define MCUBOOT_LOG_LEVEL MCUBOOT_LOG_LEVEL_ERROR
#endif

#define MCUBOOT_LOG_MODULE_DECLARE(domain)
#define MCUBOOT_LOG_MODULE_REGISTER(domain)

#if MCUBOOT_LOG_LEVEL >= MCUBOOT_LOG_LEVEL_ERROR
#define MCUBOOT_LOG_ERR(...) do { printf(__VA_ARGS__); printf("\n"); } while (0)
#else
#define MCUBOOT_LOG_ERR(...)
#endif

#if MCUBOOT_LOG_LEVEL >= MCUBOOT_LOG_LEVEL_WARNING
#define MCUBOOT_LOG_WRN(...) do { printf(__VA_ARGS__); printf("\n"); } while (0)
#else
#define MCUBOOT_LOG_WRN(...)
#endif

#if MCUBOOT_LOG_LEVEL >= MCUBOOT_LOG_LEVEL_INFO
#define MCUBOOT_LOG_INF(...) do { printf(__VA_ARGS__); printf("\n"); } while (0)
#else
#define MCUBOOT_LOG_INF(...)
#endif

#if MCUBOOT_LOG_LEVEL >= MCUBOOT_LOG_LEVEL_DEBUG
#define MCUBOOT_LOG_DBG(...) do { printf(__VA_ARGS__); printf("\n"); } while (0)
#else
#define MCUBOOT_LOG_DBG(...)
#endif

#endif /* __MCUBOOT_LOGGING_H__ */
