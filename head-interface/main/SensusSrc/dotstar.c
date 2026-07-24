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

static volatile led_color_t led_strip_buff[N_LED];	//!< LED buffer init

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
	uint8_t end_frame[] = {0xff, 0xff, 0xff, 0xff};
	SPI_Send_Bytes(end_frame, 4);
//	for(uint8_t i = 0; i < 4; i++) {
//		SPI_Send_Byte(0xFF);
//	}
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

	SPI_Send_Byte(0x00);
	SPI_Send_Byte(0x00);
	SPI_Send_Byte(0x00);
	SPI_Send_Byte(0x00);

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
	led_color_t zero={
		.brightness = 0,
		.red = 0,
		.green = 0,
		.blue = 0
	};
	if(dir){
		for(i = (N_LED - 1); i > 0; i--){
			led_strip_buff[i] = led_strip_buff[i-1];
		}
		led_strip_buff[0] = zero;
	}else{
		for(i = 0; i < (N_LED - 1); i++){
			led_strip_buff[i] = led_strip_buff[i+1];
		}
		led_strip_buff[N_LED - 1] = zero;
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
	dotstar_set_rgb_all(0, 0, 0, 0);
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
	color.brightness |= 0xE0;
	led_strip_buff[n_led] = color;
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
		.brightness = level | 0xE0,
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
	uint8_t i = 0;
	for(i = 0; i < N_LED; i++)
	{
	    led_strip_buff[i] = color;
	}

	_writeLEDs();
}

void dotstar_set_rgb_all(uint8_t level, uint8_t r, uint8_t g, uint8_t b)
{
	led_color_t color = {
		.brightness = level | 0xE0,
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
	color.brightness |= 0xE0;
	led_strip_buff[led_n] = color;
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
	led_strip_buff[led_n].brightness = level | 0xE0;
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
	uint8_t i = 0;
	for(i = 0; i<n_position;i++)_ringshift_all_once(dir);
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
	dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 255, 255, 255);
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
	static uint8_t led_sequence_idx = LED_SEQ_OFF;
	if(idx < NUM_LED_SEQUENCES && idx != led_sequence_idx)
	{
		led_sequence_idx = idx;

		switch (led_sequence_idx)
		{
			case LED_SEQ_OFF:
				dotstar_set_rgb_all(0, 0, 0, 0);          					 // Black
				break;

			case LED_SEQ_COLD:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 0, 0, 255);        // Blue
				break;

			case LED_SEQ_WARMUP:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 255, 165, 0);      // Orange
				break;

			case LED_SEQ_WARMUP_FAULT:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 255, 0, 255);      // Magenta
				break;

			case LED_SEQ_PRIMED:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 0, 255, 255);      // Cyan
				break;

			case LED_SEQ_SETUP:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 255, 255, 0);      // Yellow
				break;

			case LED_SEQ_READY:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 0, 255, 0);        // Green
				break;

			case LED_SEQ_XRAY:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 255, 255, 255);    // White
				break;

			case LED_SEQ_STANDBY:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 128, 128, 128);    // Gray
				break;

			case LED_SEQ_FAULT:
				dotstar_set_rgb_all(LED_BRIGHTNESS_LEVEL, 255, 0, 0);        // Red

			default:
				dotstar_set_rgb_all(0, 0, 0, 0);
				break;
		}
	}
}
