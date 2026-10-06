#pragma once

#define GPIO_DIRECTION_OFF 0
#define GPIO_DIRECTION_IN 1
#define GPIO_DIRECTION_OUT 2

#define GPIO_PIN(n) (((n)&0x1Fu) << 0)
#define GPIO_PORT(n) ((n) >> 5)
#define GPIO(port, pin) ((((port)&0x7u) << 5) + ((pin)&0x1Fu))
#define GPIO_PIN_FUNCTION_OFF 0xffffffff
