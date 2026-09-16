/**
 ****************************************************************
 @file   dotstar.c
 ****************************************************************
 @brief  driver for the dotstar LED-Strip

 ******************************************************************/
#include "dotstar.h"
#include "main.h"
#include "leds.h"
#include <stdint.h>
#include <stdbool.h>

extern SPI_HandleTypeDef hspi1;

const led_color_t DOTSTAR_COLOR_OFF = {0, 0, 0, 0};
const led_color_t DOTSTAR_COLOR_BLUE = {LED_BRIGHTNESS_LEVEL, 0, 0, 255};
const led_color_t DOTSTAR_COLOR_ORANGE = {LED_BRIGHTNESS_LEVEL, 255, 165, 0};
const led_color_t DOTSTAR_COLOR_AMBER = {LED_BRIGHTNESS_LEVEL, 255, 80, 0};
const led_color_t DOTSTAR_COLOR_PURPLE = {LED_BRIGHTNESS_LEVEL, 128, 0, 255};
const led_color_t DOTSTAR_COLOR_MAGENTA = {LED_BRIGHTNESS_LEVEL, 255, 0, 255};
const led_color_t DOTSTAR_COLOR_CYAN = {LED_BRIGHTNESS_LEVEL, 0, 255, 255};
const led_color_t DOTSTAR_COLOR_YELLOW = {LED_BRIGHTNESS_LEVEL, 255, 255, 0};
const led_color_t DOTSTAR_COLOR_GREEN = {LED_BRIGHTNESS_LEVEL, 0, 255, 0};
const led_color_t DOTSTAR_COLOR_WHITE = {LED_BRIGHTNESS_LEVEL, 255, 255, 255};
const led_color_t DOTSTAR_COLOR_GRAY = {LED_BRIGHTNESS_LEVEL, 128, 128, 128};
const led_color_t DOTSTAR_COLOR_RED = {LED_BRIGHTNESS_LEVEL, 255, 0, 0};

#define WARMUP_ROTATING_SEGMENT_LEDS    25U
#define WARMUP_ROTATING_STEP_MS         60U
#define WARMUP_PULSATING_PERIOD_MS      3000U
#define PRIMED_ALTERNATING_INTERVAL_MS  500U
#define SETUP_COLOR_CHASE_STEP_MS        75U
#define COLOR_CHASE_SEGMENT_LEDS         25U
#define COLOR_CHASE_TRAIL_DISTANCE_LEDS  26U
#define XRAY_PULSATING_PERIOD_MS        3000U
#define FAULT_FLASHING_INTERVAL_MS      250U
#define TEST_BREATHING_PERIOD_MS        3000U
#define TEST_THEATER_CHASE_SPACING      3U
#define TEST_THEATER_CHASE_STEP_MS      100U
#define TEST_COMET_TAIL_LEDS             10U
#define TEST_COMET_STEP_MS               60U
#define TEST_GRADIENT_CHASE_STEP_MS      60U
#define DOTSTAR_END_FRAME_BYTES         ((N_LED + 15U) / 16U)

static volatile led_color_t led_strip_buff[N_LED];	//!< LED buffer init

typedef struct {
	dotstar_effect_t type;
	led_color_t primary_color;
	led_color_t secondary_color;
	uint32_t elapsed_ms;
	uint16_t timing_ms;
	uint32_t secondary_elapsed_ms;
	uint16_t secondary_timing_ms;
	uint8_t position;
	uint8_t segment_length;
	uint8_t spacing;
	bool direction;
	bool phase;
} dotstar_effect_state_t;

static dotstar_effect_state_t effect_state;
volatile int32_t dotstar_effect_ms;

static led_color_t _normalize_color(led_color_t color)
{
	color.brightness &= MAX_LED_BRIGHTNESS_LEVEL;
	return color;
}

static void _set_buffer_all(led_color_t color)
{
	color = _normalize_color(color);
	for(uint8_t i = 0; i < N_LED; i++) {
		led_strip_buff[i] = color;
	}
}

static led_color_t _scale_color(led_color_t color, uint32_t intensity)
{
	color.red = (uint8_t)(((uint32_t)color.red * intensity + 127U) / 255U);
	color.green = (uint8_t)(((uint32_t)color.green * intensity + 127U) / 255U);
	color.blue = (uint8_t)(((uint32_t)color.blue * intensity + 127U) / 255U);
	return color;
}

static uint32_t _smoothstep(uint32_t intensity)
{
	intensity = (intensity > 255U) ? 255U : intensity;
	return (intensity * intensity * (765U - (2U * intensity)) + 32512U) /
		65025U;
}

static led_color_t _blend_color(led_color_t first, led_color_t second,
		uint32_t amount)
{
	amount = (amount > 255U) ? 255U : amount;
	uint32_t inverse = 255U - amount;
	led_color_t color = {
		.brightness = (uint8_t)(((uint32_t)first.brightness * inverse +
				(uint32_t)second.brightness * amount + 127U) / 255U),
		.red = (uint8_t)(((uint32_t)first.red * inverse +
				(uint32_t)second.red * amount + 127U) / 255U),
		.green = (uint8_t)(((uint32_t)first.green * inverse +
				(uint32_t)second.green * amount + 127U) / 255U),
		.blue = (uint8_t)(((uint32_t)first.blue * inverse +
				(uint32_t)second.blue * amount + 127U) / 255U),
	};
	return _normalize_color(color);
}

static void _set_moving_segment(uint8_t start, led_color_t color,
		bool dir, uint8_t length)
{
	for(uint8_t i = 0; i < length; i++) {
		uint8_t led = dir
			? (uint8_t)((start + i) % N_LED)
			: (uint8_t)((start + N_LED - i) % N_LED);
		led_strip_buff[led] = color;
	}
}

static void _render_color_chase(void)
{
	uint8_t target_position = effect_state.position;
	uint8_t chase_position = effect_state.direction
		? (uint8_t)((target_position + N_LED - COLOR_CHASE_TRAIL_DISTANCE_LEDS) % N_LED)
		: (uint8_t)((target_position + COLOR_CHASE_TRAIL_DISTANCE_LEDS) % N_LED);

	_set_buffer_all(DOTSTAR_COLOR_OFF);
	_set_moving_segment(target_position, effect_state.secondary_color,
			effect_state.direction, effect_state.segment_length);
	_set_moving_segment(chase_position, effect_state.primary_color,
			effect_state.direction, effect_state.segment_length);
}

static void _render_theater_chase(void)
{
	_set_buffer_all(DOTSTAR_COLOR_OFF);
	for(uint8_t i = 0; i < N_LED; i++) {
		uint8_t distance = (uint8_t)((i + N_LED - effect_state.position) % N_LED);
		if((distance % effect_state.spacing) == 0U) {
			led_strip_buff[i] = effect_state.primary_color;
		}
	}
}

static void _render_comet(void)
{
	_set_buffer_all(DOTSTAR_COLOR_OFF);
	for(uint8_t i = 0; i < effect_state.segment_length; i++) {
		uint8_t led = effect_state.direction
			? (uint8_t)((effect_state.position + N_LED - i) % N_LED)
			: (uint8_t)((effect_state.position + i) % N_LED);
		uint32_t intensity =
			(255U * (effect_state.segment_length - i)) /
			effect_state.segment_length;
		led_strip_buff[led] = _scale_color(effect_state.primary_color,
				intensity);
	}
}

static void _render_gradient_chase(void)
{
	const uint8_t first_half = N_LED / 2U;
	const uint8_t second_half = N_LED - first_half;

	for(uint8_t i = 0; i < N_LED; i++) {
		uint8_t phase = (uint8_t)((i + N_LED - effect_state.position) % N_LED);
		uint32_t amount = (phase <= first_half)
			? (255U * phase) / first_half
			: (255U * (N_LED - phase)) / second_half;
		led_strip_buff[i] = _blend_color(effect_state.primary_color,
				effect_state.secondary_color, amount);
	}
}

static HAL_StatusTypeDef SPI_Send_Byte(uint8_t data) {
	return HAL_SPI_Transmit(&hspi1, &data, 1, HAL_MAX_DELAY);
}

static HAL_StatusTypeDef SPI_Send_Bytes(uint8_t *data, uint16_t size) {
	return HAL_SPI_Transmit(&hspi1, data, size, HAL_MAX_DELAY);
}


/*
 ****************************************************************
 @brief  start sequence for addressing dotstar LEDs. See datasheet
	     for more information.
 @param  -
 @return -
 ****************************************************************
 */
static void _start_sequence()
{
	uint8_t start_frame[] = {0x00, 0x00, 0x00, 0x00};
	SPI_Send_Bytes(start_frame, 4);
//	for(uint8_t i = 0; i < 4; i++) {
//		SPI_Send_Byte(0x00);
//	}
}

/*
 ****************************************************************
 @brief  stop sequence for addressing dotstar LEDs. See datasheet
	     for more information.
 @param  -
 @return -
 ****************************************************************
 */
static void _stop_sequence()
{
	for(uint8_t i = 0; i < DOTSTAR_END_FRAME_BYTES; i++) {
		SPI_Send_Byte(0xFF);
	}
}

/*
 ****************************************************************
 @brief  writes the entire LED settings buffer to the LEDs.
 @param  -
 @return -
 ****************************************************************
 */
static void _writeLEDs()
{
	uint8_t i = 0;
	_start_sequence();
	for(i = 0; i < N_LED ; i++) {
		SPI_Send_Byte(led_strip_buff[i].brightness | 0xE0);
		SPI_Send_Byte(led_strip_buff[i].blue);
		SPI_Send_Byte(led_strip_buff[i].green);
		SPI_Send_Byte(led_strip_buff[i].red);
	}

	_stop_sequence();
}

/*
 ****************************************************************
 @brief  shifts all LEDs once in the desired direction determined
		 by dir.
		 Data shifted over border is lost!
 @param  dir true: right , false: left
 @bug
 @return -
 ****************************************************************
 */
static void _shift_all_once(bool dir)
{
	uint8_t i = 0;
	if(dir){
		for(i = (N_LED - 1); i > 0; i--){
			led_strip_buff[i] = led_strip_buff[i-1];
		}
		led_strip_buff[0] = DOTSTAR_COLOR_OFF;
	}else{
		for(i = 0; i < (N_LED - 1); i++){
			led_strip_buff[i] = led_strip_buff[i+1];
		}
		led_strip_buff[N_LED - 1] = DOTSTAR_COLOR_OFF;
	}
}

/*
 ****************************************************************
 @brief  ring shifts all LEDs once in the desired direction determined
		 by dir.
		 Data shifted over border is attached on the opposite border!
 @param  dir true: right , false: left
 @return -
 ****************************************************************
 */
static void _ringshift_all_once(bool dir)
{
	uint8_t i = 0;
	led_color_t buff;
	if(dir){
		buff = led_strip_buff[N_LED - 1];
		for(i = (N_LED - 1); i > 0; i--){
			led_strip_buff[i] = led_strip_buff[i-1];
		}
		led_strip_buff[0] = buff;
	}else{
		buff = led_strip_buff[0];
		for(i = 0; i < (N_LED - 1); i++){
			led_strip_buff[i] = led_strip_buff[i+1];
		}
		led_strip_buff[N_LED - 1] = buff;
	}
}

/*
 ****************************************************************
 @brief  initializes the ledstrip driver
		 the spi init function is called here and the callbacks
		 for chipselect and for the spi transmission are
		 implemented
 @param  spi_init spi init function pointer
 @param  spi_transmit spi transmit function pointer
 @param  chipselect function containing containing the gpio related
         actions to select/deselect the ledstrip
 @return -
 ****************************************************************
 */
void init_rgb_strip(void)
{
	effect_state.type = DOTSTAR_EFFECT_NONE;
	dotstar_effect_ms = DOTSTAR_EFFECT_TICK_MS;
	dotstar_set_color_all(DOTSTAR_COLOR_OFF);
}

/*
 ****************************************************************
 @brief  updates one LEDs color
         The buffer is updated and all its content is written to the strip
 @param  n_led led number
 @param  color color_t
 @return -
 ****************************************************************
 */
void dotstar_set_LED_color(uint8_t n_led,led_color_t color)
{
	if(n_led >= N_LED) return;
	dotstar_stop_effect();
	led_strip_buff[n_led] = _normalize_color(color);
	_writeLEDs();
}

/*
 ****************************************************************
 @brief  updates rgb of one LED
		 The buffer is updated and all its content is written to the strip
 @param  n_led led number
 @param  r red
 @param  g green
 @param  b blue
 @return -
 ****************************************************************
 */
void dotstar_set_LED_rgb(uint8_t n_led,uint8_t level, uint8_t r, uint8_t g, uint8_t b)
{
	led_color_t color = {
		.brightness = level,
		.red = r,
		.green = g,
		.blue = b,
	};
	dotstar_set_LED_color(n_led, color);
}

/*
 ****************************************************************
 @brief  updates all LED color
         The buffer is updated and all its content is written to the strip
 @param  color color_t
 @return -
 ****************************************************************
 */
void dotstar_set_color_all(led_color_t color)
{
	dotstar_stop_effect();
	_set_buffer_all(color);
	_writeLEDs();
}

void dotstar_set_rgb_all(uint8_t level, uint8_t r, uint8_t g, uint8_t b)
{
	led_color_t color = {
		.brightness = level,
		.red = r,
		.green = g,
		.blue = b
	};
	dotstar_set_color_all(color);
}


/*
 ****************************************************************
 @brief  updates one LEDs color
         The buffer is updated but its content is only written to the strip
		 when ledstrip_update_all() is called.
 @param  n_led led number
 @param  color color_t
 @return -
 ****************************************************************
 */
void dotstar_pending_set_LED_color(uint8_t led_n, led_color_t color)
{
	if(led_n >= N_LED) return;
	dotstar_stop_effect();
	led_strip_buff[led_n] = _normalize_color(color);
}

/*
 ****************************************************************
 @brief  updates rgb of one LED
		 The buffer is updated but its content is only written to the strip
		 when ledstrip_update_all() is called.
 @param  n_led led number
 @param  r red
 @param  g green
 @param  b blue
 @return -
 ****************************************************************
 */
void dotstar_pending_set_LED_rgb(uint8_t led_n, uint8_t level, uint8_t r, uint8_t g, uint8_t b)
{
	if(led_n >= N_LED) return;
	dotstar_stop_effect();
	led_strip_buff[led_n].brightness = level & MAX_LED_BRIGHTNESS_LEVEL;
	led_strip_buff[led_n].blue = b;
	led_strip_buff[led_n].red = r;
	led_strip_buff[led_n].green = g;
}

/*
 ****************************************************************
 @brief  updates all LEDs with the buffer content
		 All settings made in the buffer by the pending functions
		 are written to the LEDs.
 @param  -
 @return -
 ****************************************************************
 */
void dotstar_update_all()
{
	_writeLEDs();
}

/*
 ****************************************************************
 @brief  Shift all LED settings n positions in direction
		 determined by dir.
		 Data pushed over the border is lost!
 @param  dir true : Right , false : left
 @param  n_position shift n positions in direction determined by dir
 @return -
 ****************************************************************
 */
void dotstar_shift_all(bool dir,uint8_t n_position)
{
	dotstar_stop_effect();
	uint8_t i = 0;
	for(i = 0; i<n_position;i++) {
		_shift_all_once(dir);
	}
	_writeLEDs();
}

/*
 ****************************************************************
 @brief  ring shift all LED settings n positions in direction
		 determined by dir.
		 Data pushed over the border is attached to the opposite border.
 @param  dir true : Right , false : left
 @param  n_position shift n positions in direction determined by dir
 @return -
 ****************************************************************
 */
void dotstar_ring_shift_all(bool dir,uint8_t n_position)
{
	dotstar_stop_effect();
	uint8_t i = 0;
	for(i = 0; i<n_position;i++)_ringshift_all_once(dir);
	_writeLEDs();
}

static uint32_t _elapsed_steps(uint16_t elapsed_ms)
{
	effect_state.elapsed_ms += elapsed_ms;
	uint32_t steps = effect_state.elapsed_ms / effect_state.timing_ms;
	effect_state.elapsed_ms %= effect_state.timing_ms;
	return steps;
}

static void _begin_effect(dotstar_effect_t effect, uint16_t timing_ms)
{
	effect_state.type = effect;
	effect_state.elapsed_ms = 0;
	effect_state.timing_ms = (timing_ms == 0U) ? 1U : timing_ms;
	effect_state.secondary_elapsed_ms = 0;
	effect_state.secondary_timing_ms = 1U;
	effect_state.position = 0;
	effect_state.segment_length = 0;
	effect_state.spacing = 1;
	effect_state.phase = false;
}

void dotstar_start_rotating(bool dir, uint16_t step_ms)
{
	_begin_effect(DOTSTAR_EFFECT_ROTATING, step_ms);
	effect_state.direction = dir;
}

void dotstar_start_color_chase(led_color_t chase_color,
		led_color_t target_color, bool dir, uint16_t step_ms)
{
	_begin_effect(DOTSTAR_EFFECT_COLOR_CHASE, step_ms);
	effect_state.primary_color = _normalize_color(chase_color);
	effect_state.secondary_color = _normalize_color(target_color);
	effect_state.direction = dir;
	effect_state.position = dir ? 0U : (N_LED - 1U);
	effect_state.segment_length = COLOR_CHASE_SEGMENT_LEDS;

	_render_color_chase();
	_writeLEDs();
}

void dotstar_start_alternating(led_color_t first_color,
		led_color_t second_color, uint16_t interval_ms)
{
	_begin_effect(DOTSTAR_EFFECT_ALTERNATING, interval_ms);
	effect_state.primary_color = _normalize_color(first_color);
	effect_state.secondary_color = _normalize_color(second_color);

	_set_buffer_all(effect_state.primary_color);
	_writeLEDs();
}

void dotstar_start_pulsating(led_color_t color, uint16_t period_ms)
{
	_begin_effect(DOTSTAR_EFFECT_PULSATING, (period_ms < 2U) ? 2U : period_ms);
	effect_state.primary_color = _normalize_color(color);

	color = effect_state.primary_color;
	color.red = 0;
	color.green = 0;
	color.blue = 0;
	_set_buffer_all(color);
	_writeLEDs();
}

void dotstar_start_rotating_pulsating(led_color_t color,
		uint8_t segment_length, bool dir, uint16_t rotation_step_ms,
		uint16_t pulse_period_ms)
{
	_begin_effect(DOTSTAR_EFFECT_ROTATING_PULSATING, rotation_step_ms);
	effect_state.primary_color = _normalize_color(color);
	effect_state.secondary_timing_ms =
		(pulse_period_ms < 2U) ? 2U : pulse_period_ms;
	effect_state.segment_length =
		(segment_length > N_LED) ? N_LED : segment_length;
	effect_state.direction = dir;

	_set_buffer_all(DOTSTAR_COLOR_OFF);
	_writeLEDs();
}

void dotstar_start_flashing(led_color_t color, uint16_t interval_ms)
{
	_begin_effect(DOTSTAR_EFFECT_FLASHING, interval_ms);
	effect_state.primary_color = _normalize_color(color);

	_set_buffer_all(effect_state.primary_color);
	_writeLEDs();
}

void dotstar_start_breathing(led_color_t color, uint16_t period_ms)
{
	_begin_effect(DOTSTAR_EFFECT_BREATHING,
			(period_ms < 2U) ? 2U : period_ms);
	effect_state.primary_color = _normalize_color(color);

	_set_buffer_all(_scale_color(effect_state.primary_color, 0U));
	_writeLEDs();
}

void dotstar_start_theater_chase(led_color_t color, uint8_t spacing,
		bool dir, uint16_t step_ms)
{
	_begin_effect(DOTSTAR_EFFECT_THEATER_CHASE, step_ms);
	effect_state.primary_color = _normalize_color(color);
	effect_state.spacing = (spacing == 0U) ? 1U :
		((spacing > N_LED) ? N_LED : spacing);
	effect_state.direction = dir;
	effect_state.position = dir ? 0U : (N_LED - 1U);

	_render_theater_chase();
	_writeLEDs();
}

void dotstar_start_comet(led_color_t color, uint8_t tail_length,
		bool dir, uint16_t step_ms)
{
	_begin_effect(DOTSTAR_EFFECT_COMET, step_ms);
	effect_state.primary_color = _normalize_color(color);
	effect_state.segment_length = (tail_length == 0U) ? 1U :
		((tail_length > N_LED) ? N_LED : tail_length);
	effect_state.direction = dir;
	effect_state.position = dir ? 0U : (N_LED - 1U);

	_render_comet();
	_writeLEDs();
}

void dotstar_start_gradient_chase(led_color_t first_color,
		led_color_t second_color, bool dir, uint16_t step_ms)
{
	_begin_effect(DOTSTAR_EFFECT_GRADIENT_CHASE, step_ms);
	effect_state.primary_color = _normalize_color(first_color);
	effect_state.secondary_color = _normalize_color(second_color);
	effect_state.direction = dir;
	effect_state.position = dir ? 0U : (N_LED - 1U);

	_render_gradient_chase();
	_writeLEDs();
}

void dotstar_stop_effect(void)
{
	effect_state.type = DOTSTAR_EFFECT_NONE;
	effect_state.elapsed_ms = 0;
}

dotstar_effect_t dotstar_get_effect(void)
{
	return effect_state.type;
}

/**
 * @brief Advance the active DotStar animation when its update timer expires.
 *
 * This function is called continuously from the main loop. The 1 ms system
 * timer decrements dotstar_effect_ms, while this function updates the effect
 * in fixed DOTSTAR_EFFECT_TICK_MS increments without blocking the main loop.
 */
void dotstar_effect_tick(void)
{
	/* Wait until the periodic effect timer expires. */
	if(dotstar_effect_ms >= 0) {
		return;
	}

	/* Preserve accumulated timing error and process one fixed update period. */
	dotstar_effect_ms += DOTSTAR_EFFECT_TICK_MS;
	const uint16_t elapsed_ms = DOTSTAR_EFFECT_TICK_MS;

	if(effect_state.type == DOTSTAR_EFFECT_NONE || elapsed_ms == 0U) {
		return;
	}

	/* Pulsating effects are rendered every tick for a smooth brightness ramp. */
	if(effect_state.type == DOTSTAR_EFFECT_PULSATING ||
			effect_state.type == DOTSTAR_EFFECT_BREATHING) {
		effect_state.elapsed_ms =
			(effect_state.elapsed_ms + elapsed_ms) % effect_state.timing_ms;
		/* Fold the period in half to create a 0 -> 255 -> 0 waveform. */
		uint32_t phase_ms = effect_state.elapsed_ms;
		if(phase_ms > ((uint32_t)effect_state.timing_ms / 2U)) {
			phase_ms = (uint32_t)effect_state.timing_ms - phase_ms;
		}

		led_color_t color = effect_state.primary_color;
		uint32_t intensity = (510U * phase_ms) / effect_state.timing_ms;
		if(effect_state.type == DOTSTAR_EFFECT_BREATHING) {
			intensity = _smoothstep(intensity);
		}
		color = _scale_color(color, intensity);
		_set_buffer_all(color);
		_writeLEDs();
		return;
	}

	/* Rotation and pulsation use independent timing accumulators. */
	if(effect_state.type == DOTSTAR_EFFECT_ROTATING_PULSATING) {
		uint32_t steps = _elapsed_steps(elapsed_ms) % N_LED;
		if(effect_state.direction) {
			effect_state.position = (uint8_t)
				((effect_state.position + steps) % N_LED);
		} else {
			effect_state.position = (uint8_t)
				((effect_state.position + N_LED - steps) % N_LED);
		}

		effect_state.secondary_elapsed_ms =
			(effect_state.secondary_elapsed_ms + elapsed_ms) %
			effect_state.secondary_timing_ms;
		uint32_t phase_ms = effect_state.secondary_elapsed_ms;
		if(phase_ms > ((uint32_t)effect_state.secondary_timing_ms / 2U)) {
			phase_ms = (uint32_t)effect_state.secondary_timing_ms - phase_ms;
		}
		uint32_t intensity =
			(510U * phase_ms) / effect_state.secondary_timing_ms;
		led_color_t color = _scale_color(effect_state.primary_color, intensity);

		_set_buffer_all(DOTSTAR_COLOR_OFF);
		for(uint8_t i = 0; i < effect_state.segment_length; i++) {
			uint8_t led = (uint8_t)((effect_state.position + i) % N_LED);
			led_strip_buff[led] = color;
		}
		_writeLEDs();
		return;
	}

	/* Interval-based effects update only when one or more steps are due. */
	uint32_t steps = _elapsed_steps(elapsed_ms);
	if(steps == 0U) {
		return;
	}

	switch(effect_state.type) {
		case DOTSTAR_EFFECT_ROTATING:
			steps %= N_LED;
			for(uint32_t i = 0; i < steps; i++) {
				_ringshift_all_once(effect_state.direction);
			}
			break;

		case DOTSTAR_EFFECT_COLOR_CHASE:
			steps %= N_LED;
			if(effect_state.direction) {
				effect_state.position = (uint8_t)
					((effect_state.position + steps) % N_LED);
			} else {
				effect_state.position = (uint8_t)
					((effect_state.position + N_LED - steps) % N_LED);
			}
			_render_color_chase();
			break;

		case DOTSTAR_EFFECT_ALTERNATING:
			if((steps & 1U) != 0U) {
				effect_state.phase = !effect_state.phase;
			}
			_set_buffer_all(effect_state.phase ? effect_state.secondary_color :
					effect_state.primary_color);
			break;

		case DOTSTAR_EFFECT_FLASHING:
			if((steps & 1U) != 0U) {
				effect_state.phase = !effect_state.phase;
			}
			if(effect_state.phase) {
				_set_buffer_all(DOTSTAR_COLOR_OFF);
			} else {
				_set_buffer_all(effect_state.primary_color);
			}
			break;

		case DOTSTAR_EFFECT_THEATER_CHASE:
			steps %= N_LED;
			if(effect_state.direction) {
				effect_state.position = (uint8_t)
					((effect_state.position + steps) % N_LED);
			} else {
				effect_state.position = (uint8_t)
					((effect_state.position + N_LED - steps) % N_LED);
			}
			_render_theater_chase();
			break;

		case DOTSTAR_EFFECT_COMET:
			steps %= N_LED;
			if(effect_state.direction) {
				effect_state.position = (uint8_t)
					((effect_state.position + steps) % N_LED);
			} else {
				effect_state.position = (uint8_t)
					((effect_state.position + N_LED - steps) % N_LED);
			}
			_render_comet();
			break;

		case DOTSTAR_EFFECT_GRADIENT_CHASE:
			steps %= N_LED;
			if(effect_state.direction) {
				effect_state.position = (uint8_t)
					((effect_state.position + steps) % N_LED);
			} else {
				effect_state.position = (uint8_t)
					((effect_state.position + N_LED - steps) % N_LED);
			}
			_render_gradient_chase();
			break;

		default:
			return;
	}

	/* Send the completed animation frame to the DotStar ring. */
	_writeLEDs();
}

/****************************************************************
 * @brief  Verify DotStar LED operation by illuminating all LEDs
 *         in white at a reduced brightness level.
 *
 * @param  None.
 *
 * @return None.
 ****************************************************************/
void dotstar_test(void)
{
	/* Set all LEDs to White */
	dotstar_set_color_all(DOTSTAR_COLOR_WHITE);
}

/****************************************************************
 * @brief  Update the DotStar LED color based on the specified
 *         LED sequence/state.
 *
 *         The LED color is updated only when the requested
 *         sequence differs from the current sequence and the
 *         sequence index is valid.
 *
 * @param  idx  LED sequence index to display.
 *
 * @return None.
 ****************************************************************/
void process_led_sequence(uint8_t idx)
{
	static uint8_t led_sequence_idx = LED_SEQ_IDLE;
	if(idx < NUM_LED_SEQUENCES && idx != led_sequence_idx)
	{
		led_sequence_idx = idx;

		switch (led_sequence_idx)
		{
			case LED_SEQ_OFF:
				dotstar_set_color_all(DOTSTAR_COLOR_OFF);
				break;

			case LED_SEQ_COLD:
				dotstar_start_pulsating(DOTSTAR_COLOR_WHITE,
						XRAY_PULSATING_PERIOD_MS);
				break;

			case LED_SEQ_WARMUP:
				dotstar_start_rotating_pulsating(DOTSTAR_COLOR_PURPLE,
						WARMUP_ROTATING_SEGMENT_LEDS, true,
						WARMUP_ROTATING_STEP_MS,
						WARMUP_PULSATING_PERIOD_MS);
				break;

			case LED_SEQ_PRIMED:
				dotstar_start_alternating(DOTSTAR_COLOR_BLUE,
						DOTSTAR_COLOR_GREEN,
						PRIMED_ALTERNATING_INTERVAL_MS);
				break;

			case LED_SEQ_SETUP:
				dotstar_start_color_chase(DOTSTAR_COLOR_ORANGE,
						DOTSTAR_COLOR_BLUE, true,
						SETUP_COLOR_CHASE_STEP_MS);
				break;

			case LED_SEQ_READY:
				dotstar_set_color_all(DOTSTAR_COLOR_GREEN);
				break;

			case LED_SEQ_XRAY:
				dotstar_start_pulsating(DOTSTAR_COLOR_AMBER,
						XRAY_PULSATING_PERIOD_MS);
				break;

			case LED_SEQ_IDLE:
				dotstar_set_color_all(DOTSTAR_COLOR_WHITE);
				break;

			case LED_SEQ_FAULT:
				dotstar_start_flashing(DOTSTAR_COLOR_RED,
						FAULT_FLASHING_INTERVAL_MS);
				break;

			case LED_SEQ_TEST_BREATHING:
				dotstar_start_breathing(DOTSTAR_COLOR_CYAN,
						TEST_BREATHING_PERIOD_MS);
				break;

			case LED_SEQ_TEST_THEATER_CHASE:
				dotstar_start_theater_chase(DOTSTAR_COLOR_MAGENTA,
						TEST_THEATER_CHASE_SPACING, true,
						TEST_THEATER_CHASE_STEP_MS);
				break;

			case LED_SEQ_TEST_COMET:
				dotstar_start_comet(DOTSTAR_COLOR_AMBER,
						TEST_COMET_TAIL_LEDS, true,
						TEST_COMET_STEP_MS);
				break;

			case LED_SEQ_TEST_GRADIENT_CHASE:
				dotstar_start_gradient_chase(DOTSTAR_COLOR_BLUE,
						DOTSTAR_COLOR_GREEN, true,
						TEST_GRADIENT_CHASE_STEP_MS);
				break;

			default:
				dotstar_set_color_all(DOTSTAR_COLOR_OFF);
				break;
		}
	}
}
