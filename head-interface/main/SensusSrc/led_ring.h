/**
 * @file    led_ring.h
 * @brief   LED ring driver for STM32F411, hardcoded to TIM2 Channel 1 PWM.
 *
 * Hardware assumptions:
 *  - TIM2 configured in CubeMX with PWM Generation CH1 enabled.
 *  - TIM2_CH1 routed to PA5 (Nucleo-F411RE default) or PA0/PA15,
 *    whichever your board/pinout uses.
 *  - Timer ARR = 999 (1000 steps -> 0.1% resolution). If your ARR is
 *    different, change LED_RING_PWM_MAX to match (ARR + 1).
 *  - MX_TIM2_Init() must be called before led_ring_init().
 */

#ifndef LED_RING_H
#define LED_RING_H

#ifdef __cplusplus
extern "C" {
#endif

#include "stm32f4xx_hal.h"
#include <stdint.h>
#include <stdbool.h>

/* Must match (ARR + 1) as configured for TIM2 in CubeMX. */
#define LED_RING_PWM_MAX        1000u
/* Brightness levels for each named mode (0-100%). Tune to your LEDs/eyes. */
#define LED_RING_DIM_PCT        10
#define LED_RING_NORMAL_PCT     20
#define LED_RING_HIGH_PCT       30
#define LED_RING_BREATHE_PERIOD_MS  3000u   /* default breathing cycle time */

#define LED_RING_MAX_BREATH_PCT 30.0f

typedef enum {
    LED_RING_MODE_OFF = 0,
    LED_RING_MODE_DIM,
    LED_RING_MODE_NORMAL,
    LED_RING_MODE_HIGH,
    LED_RING_MODE_BREATHING,
    LED_RING_MODE_COUNT, 
} led_ring_mode_t;

bool    init_led_ring(void);
void    led_ring_set_brightness(uint8_t percent);       /* 0-100 */
void    led_ring_set_duty_raw(uint32_t duty);            /* 0-LED_RING_PWM_MAX */
uint8_t led_ring_get_brightness(void);
void    led_ring_turn_off(void);
void    led_ring_turn_on(void);
void    led_ring_fade_to(uint8_t from_pct, uint8_t to_pct, uint16_t duration_ms);
void    led_ring_set_mode(led_ring_mode_t mode);
/* Call this once every 10ms from your existing timer ISR. */
void    led_ring_tick(uint16_t tick_ms);

bool led_ring_is_fading(void);

#ifdef __cplusplus
}
#endif

#endif /* LED_RING_H */
