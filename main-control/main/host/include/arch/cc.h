#ifndef GENTLEBEAM_HOST_LWIP_CC_H
#define GENTLEBEAM_HOST_LWIP_CC_H

#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

typedef uint8_t u8_t;
typedef int8_t s8_t;
typedef uint16_t u16_t;
typedef int16_t s16_t;
typedef uint32_t u32_t;
typedef int32_t s32_t;
typedef uintptr_t mem_ptr_t;

#ifndef BYTE_ORDER
#if defined(__BYTE_ORDER__) && __BYTE_ORDER__ == __ORDER_BIG_ENDIAN__
#define BYTE_ORDER BIG_ENDIAN
#else
#define BYTE_ORDER LITTLE_ENDIAN
#endif
#endif

#define U16_F PRIu16
#define S16_F PRId16
#define X16_F PRIx16
#define U32_F PRIu32
#define S32_F PRId32
#define X32_F PRIx32
#define SZT_F "zu"

#if defined(_MSC_VER)
#define PACK_STRUCT_BEGIN __pragma(pack(push, 1))
#define PACK_STRUCT_STRUCT
#define PACK_STRUCT_END __pragma(pack(pop))
#elif defined(__GNUC__)
#define PACK_STRUCT_BEGIN
#define PACK_STRUCT_STRUCT __attribute__((packed))
#define PACK_STRUCT_END
#else
#error Unsupported host compiler
#endif
#define PACK_STRUCT_FIELD(field) field

#define LWIP_PLATFORM_DIAG(message) do { printf message; } while (0)
#define LWIP_PLATFORM_ASSERT(message) do { \
    fprintf(stderr, "lwIP assertion: %s (%s:%d)\n", message, __FILE__, __LINE__); \
    abort(); \
} while (0)

#endif
