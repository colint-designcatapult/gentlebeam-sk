#ifndef DOTSTAR_H_
#define DOTSTAR_H_

#include <stdint.h>
#include <stdbool.h>

//defines
#define NULL_POINTER 					0		//!< define the NULL pointer for your target device
#define N_LED 							51		//!< define the number of LEDs on the strip

#define MAX_LED_BRIGHTNESS_LEVEL		31
#define MIN_LED_BRIGHTNESS_LEVEL		0
#define LED_BRIGHTNESS_LEVEL    		3

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

void init_rgb_strip(void);

void dotstar_test(void);

void dotstar_set_LED_color(uint8_t n_led,led_color_t color);

void dotstar_set_LED_rgb(uint8_t n_led, uint8_t level, uint8_t r, uint8_t g, uint8_t b);

void dotstar_set_color_all(led_color_t color);

void dotstar_set_rgb_all(uint8_t level, uint8_t r, uint8_t g, uint8_t b);

void dotstar_pending_set_LED_color(uint8_t led_n, led_color_t color);

void dotstar_pending_set_LED_rgb(uint8_t led_n, uint8_t level, uint8_t r, uint8_t g, uint8_t b);

void dotstar_update_all();

void dotstar_ring_shift_all(bool dir,uint8_t n_position);

void dotstar_shift_all(bool dir,uint8_t n_position);

void process_led_sequence(uint8_t idx);

#endif /* DOTSTAR_H_ */
