#ifndef DOTSTAR_H_
#define DOTSTAR_H_

#include <stdint.h>
#include <stdbool.h>

//defines
#define NULL_POINTER 					0		//!< define the NULL pointer for your target device
#define N_LED 							51		//!< define the number of LEDs on the strip

#define MAX_LED_BRIGHTNESS_LEVEL		31
#define MIN_LED_BRIGHTNESS_LEVEL		0
#define LED_BRIGHTNESS_LEVEL    		5

/** DotStar animation update resolution used by the main loop. */
#define DOTSTAR_EFFECT_TICK_MS          20U

typedef void (*fptr_U8_t)(uint8_t);  //!< function pointer with uint8_t parameter


/** @struct led_color_t
   *
   *  @var led_color_t::brightness
   *    color brightness uint8_t range 0 - 31 (0x1F)
   *  @var led_color_t::red
   *    red color uint8_t
   *  @var led_color_t::green
   *    green color uint8_t
   *  @var led_color_t::blue
   *    blue color uint8_t
   */
typedef struct{
	uint8_t brightness;
	uint8_t red;
	uint8_t green;
	uint8_t blue;
}led_color_t;

/** Shared colors used by the head-interface LED sequences and effects. */
extern const led_color_t DOTSTAR_COLOR_OFF;
extern const led_color_t DOTSTAR_COLOR_BLUE;
extern const led_color_t DOTSTAR_COLOR_ORANGE;
extern const led_color_t DOTSTAR_COLOR_AMBER;
extern const led_color_t DOTSTAR_COLOR_PURPLE;
extern const led_color_t DOTSTAR_COLOR_MAGENTA;
extern const led_color_t DOTSTAR_COLOR_CYAN;
extern const led_color_t DOTSTAR_COLOR_YELLOW;
extern const led_color_t DOTSTAR_COLOR_GREEN;
extern const led_color_t DOTSTAR_COLOR_WHITE;
extern const led_color_t DOTSTAR_COLOR_GRAY;
extern const led_color_t DOTSTAR_COLOR_RED;

/** Effects supported by the non-blocking DotStar animation engine. */
typedef enum {
	DOTSTAR_EFFECT_NONE = 0,
	DOTSTAR_EFFECT_ROTATING,
	DOTSTAR_EFFECT_COLOR_CHASE,
	DOTSTAR_EFFECT_ALTERNATING,
	DOTSTAR_EFFECT_PULSATING,
	DOTSTAR_EFFECT_ROTATING_PULSATING,
	DOTSTAR_EFFECT_FLASHING,
	DOTSTAR_EFFECT_BREATHING,
	DOTSTAR_EFFECT_THEATER_CHASE,
	DOTSTAR_EFFECT_COMET,
	DOTSTAR_EFFECT_GRADIENT_CHASE
} dotstar_effect_t;

void init_rgb_strip(void);

void dotstar_test(void);

void dotstar_set_LED_color(uint8_t n_led,led_color_t color);

void dotstar_set_LED_rgb(uint8_t n_led, uint8_t level, uint8_t r, uint8_t g, uint8_t b);

void dotstar_set_color_all(led_color_t color);

void dotstar_set_rgb_all(uint8_t level, uint8_t r, uint8_t g, uint8_t b);

void dotstar_pending_set_LED_color(uint8_t led_n, led_color_t color);

void dotstar_pending_set_LED_rgb(uint8_t led_n, uint8_t level, uint8_t r, uint8_t g, uint8_t b);

void dotstar_update_all(void);

void dotstar_ring_shift_all(bool dir,uint8_t n_position);

void dotstar_shift_all(bool dir,uint8_t n_position);

/**
 * Rotate the pattern currently stored in the strip buffer.
 *
 * @param dir      true rotates right; false rotates left.
 * @param step_ms  Time between one-LED rotations. Zero is treated as 1 ms.
 */
void dotstar_start_rotating(bool dir, uint16_t step_ms);

/** Move chase_color behind a leading target_color on a dark strip. */
void dotstar_start_color_chase(led_color_t chase_color,
		led_color_t target_color, bool dir, uint16_t step_ms);

/** Alternate the entire strip between two colors. */
void dotstar_start_alternating(led_color_t first_color,
		led_color_t second_color, uint16_t interval_ms);

/** Smoothly ramp a color from dark to full brightness and back. */
void dotstar_start_pulsating(led_color_t color, uint16_t period_ms);

/** Rotate a colored segment while smoothly pulsing its intensity. */
void dotstar_start_rotating_pulsating(led_color_t color,
		uint8_t segment_length, bool dir, uint16_t rotation_step_ms,
		uint16_t pulse_period_ms);

/** Alternate the entire strip between a color and off. */
void dotstar_start_flashing(led_color_t color, uint16_t interval_ms);

/** Smoothly breathe a color using an eased brightness curve. */
void dotstar_start_breathing(led_color_t color, uint16_t period_ms);

/** Move evenly spaced illuminated LEDs around the strip. */
void dotstar_start_theater_chase(led_color_t color, uint8_t spacing,
		bool dir, uint16_t step_ms);

/** Move a bright LED with a fading tail around the strip. */
void dotstar_start_comet(led_color_t color, uint8_t tail_length,
		bool dir, uint16_t step_ms);

/** Rotate a smooth two-color gradient around the strip. */
void dotstar_start_gradient_chase(led_color_t first_color,
		led_color_t second_color, bool dir, uint16_t step_ms);

/** Stop an animation without changing the currently displayed frame. */
void dotstar_stop_effect(void);

/** Return the currently selected animation. */
dotstar_effect_t dotstar_get_effect(void);

/** Advance the selected animation when its timer expires. */
void dotstar_effect_tick(void);

void process_led_sequence(uint8_t idx);

#endif /* DOTSTAR_H_ */
