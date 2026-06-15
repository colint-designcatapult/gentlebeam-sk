#include <string.h>
#include <stdbool.h>
#include "stm32f4xx_hal.h"
#include "main.h"

#include "leds.h"
#include "timer.h"
#include "sys_data.h"

volatile int32_t update_led_ms;
volatile led_bus_state led_i2c_state;

int led_frame_idx = 0;
int led_sequence_idx = LED_SEQ_OFF;
int max_led_frame = 0;

led_sequence led_pattern[NUM_LED_SEQUENCES];

uint8_t tlc59116_1_tx[NUM_CHANNELS_PER_DRIVER];
uint8_t tlc59116_2_tx[NUM_CHANNELS_PER_DRIVER];
uint8_t tlc59116_config[4];


static void set_led_frame_single(uint8_t pattern_idx, uint8_t frame_idx, uint8_t r, uint8_t g, uint8_t b);
static void output_led_frame();


void init_leds()
{
	HAL_GPIO_WritePin(IO_LED_RST_GPIO_Port, IO_LED_RST_Pin, GPIO_PIN_RESET);

	led_i2c_state = LED_BUS_WAITING_1;
	update_led_ms = 0;

	//Set up LED driver config
	tlc59116_config[0] = 0b10101010;
	tlc59116_config[1] = 0b10101010;
	tlc59116_config[2] = 0b10101010;
	tlc59116_config[3] = 0;

	uint8_t tlc59116_mode_val = 0b10000001;

	//TBD TODO check HAL return values
	HAL_I2C_Mem_Write_IT(&hi2c1, TLC59116_1_ADDR, TLC59116_MODE_REG, I2C_MEMADD_SIZE_8BIT, &tlc59116_mode_val, 1);
	HAL_I2C_Mem_Write_IT(&hi2c1, TLC59116_2_ADDR, TLC59116_MODE_REG, I2C_MEMADD_SIZE_8BIT, &tlc59116_mode_val, 1);
	HAL_Delay(1);
	HAL_I2C_Mem_Write_IT(&hi2c1, TLC59116_1_ADDR, TLC59116_OUTPUT_REG, I2C_MEMADD_SIZE_8BIT, tlc59116_config, 4);
	HAL_I2C_Mem_Write_IT(&hi2c1, TLC59116_2_ADDR, TLC59116_OUTPUT_REG, I2C_MEMADD_SIZE_8BIT, tlc59116_config, 4);

	//Set led off animation
	led_pattern[LED_SEQ_OFF].num_frames_used = 1;
	led_pattern[LED_SEQ_OFF].frames[0].ms = 1000;
	set_led_frame_single(LED_SEQ_OFF, 0, 0, 0, 0);

	//Set cold animation
	led_pattern[LED_SEQ_COLD].num_frames_used = 1;
	led_pattern[LED_SEQ_COLD].frames[0].ms = 1000;
	set_led_frame_single(LED_SEQ_COLD, 0, 0, 120, 120);

	//Set warmup animation
	led_pattern[LED_SEQ_WARMUP].num_frames_used = 4;
	for(int i = 0; i< 4; i++)
	{
		led_pattern[LED_SEQ_WARMUP].frames[i].ms = 200;
		for(int j = 0; j < NUM_LED_SECTORS; j++)
		{
			led_pattern[LED_SEQ_WARMUP].frames[i].color[j][LED_R_VAL] = 0;
			led_pattern[LED_SEQ_WARMUP].frames[i].color[j][LED_G_VAL] = 0;
			led_pattern[LED_SEQ_WARMUP].frames[i].color[j][LED_B_VAL] = 0;

			if(i == j)
			{
				led_pattern[LED_SEQ_WARMUP].frames[i].color[j][LED_R_VAL] = 125;
				led_pattern[LED_SEQ_WARMUP].frames[i].color[j][LED_B_VAL] = 125;
			}
		}
	}

	//Set warmup fault animation
	led_pattern[LED_SEQ_WARMUP_FAULT].num_frames_used = 2;
	led_pattern[LED_SEQ_WARMUP_FAULT].frames[0].ms = 500;
	led_pattern[LED_SEQ_WARMUP_FAULT].frames[1].ms = 500;
	set_led_frame_single(LED_SEQ_WARMUP_FAULT, 0, 0, 0, 0);
	set_led_frame_single(LED_SEQ_WARMUP_FAULT, 1, 200, 0, 0);

	//Set primed animation
	led_pattern[LED_SEQ_PRIMED].num_frames_used = 1;
	led_pattern[LED_SEQ_PRIMED].frames[0].ms = 1000;
	set_led_frame_single(LED_SEQ_PRIMED, 0, 0, 150, 0);

	//Set setup animation
	led_pattern[LED_SEQ_SETUP].num_frames_used = 4;
	led_pattern[LED_SEQ_SETUP].frames[0].ms = 300;
	led_pattern[LED_SEQ_SETUP].frames[1].ms = 300;
	led_pattern[LED_SEQ_SETUP].frames[2].ms = 300;
	led_pattern[LED_SEQ_SETUP].frames[3].ms = 300;
	led_pattern[LED_SEQ_SETUP].frames[0].color[0][LED_R_VAL] = 120;
	led_pattern[LED_SEQ_SETUP].frames[0].color[0][LED_G_VAL] = 30;
	led_pattern[LED_SEQ_SETUP].frames[0].color[0][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[1][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[1][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[1][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[2][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[2][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[2][LED_B_VAL] = 100;
	led_pattern[LED_SEQ_SETUP].frames[0].color[3][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[3][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[0].color[3][LED_B_VAL] = 0;

	led_pattern[LED_SEQ_SETUP].frames[1].color[0][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[0][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[0][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[1][LED_R_VAL] = 120;
	led_pattern[LED_SEQ_SETUP].frames[1].color[1][LED_G_VAL] = 30;
	led_pattern[LED_SEQ_SETUP].frames[1].color[1][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[2][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[2][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[2][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[3][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[3][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[1].color[3][LED_B_VAL] = 100;

	led_pattern[LED_SEQ_SETUP].frames[2].color[0][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[0][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[0][LED_B_VAL] = 100;
	led_pattern[LED_SEQ_SETUP].frames[2].color[1][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[1][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[1][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[2][LED_R_VAL] = 120;
	led_pattern[LED_SEQ_SETUP].frames[2].color[2][LED_G_VAL] = 30;
	led_pattern[LED_SEQ_SETUP].frames[2].color[2][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[3][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[3][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[2].color[3][LED_B_VAL] = 0;

	led_pattern[LED_SEQ_SETUP].frames[3].color[0][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[0][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[0][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[1][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[1][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[1][LED_B_VAL] = 100;
	led_pattern[LED_SEQ_SETUP].frames[3].color[2][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[2][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[2][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_SETUP].frames[3].color[3][LED_R_VAL] = 120;
	led_pattern[LED_SEQ_SETUP].frames[3].color[3][LED_G_VAL] = 30;
	led_pattern[LED_SEQ_SETUP].frames[3].color[3][LED_B_VAL] = 0;


	//Set ready animation
	led_pattern[LED_SEQ_READY].num_frames_used = 2;
	led_pattern[LED_SEQ_READY].frames[0].ms = 500;
	led_pattern[LED_SEQ_READY].frames[1].ms = 500;

	led_pattern[LED_SEQ_READY].frames[0].color[0][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[0][LED_G_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[0].color[0][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[1][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[1][LED_G_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[0].color[1][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[2][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[2][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[2][LED_B_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[0].color[3][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[3][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[0].color[3][LED_B_VAL] = 120;

	led_pattern[LED_SEQ_READY].frames[1].color[0][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[0][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[0][LED_B_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[1].color[1][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[1][LED_G_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[1][LED_B_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[1].color[2][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[2][LED_G_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[1].color[2][LED_B_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[3][LED_R_VAL] = 0;
	led_pattern[LED_SEQ_READY].frames[1].color[3][LED_G_VAL] = 120;
	led_pattern[LED_SEQ_READY].frames[1].color[3][LED_B_VAL] = 0;

	//Set xray animation
	for(int i = 0; i < 7; i++)
	{
		led_pattern[LED_SEQ_XRAY].frames[i].ms = 20;
		led_pattern[LED_SEQ_XRAY].frames[i+7].ms = 20;
		set_led_frame_single(LED_SEQ_XRAY, (uint8_t)i, 130 + (10*i), 55 + (3*i), 0);
		set_led_frame_single(LED_SEQ_XRAY, (uint8_t)i+7, 210 - (10*i), 79 - (3*i), 0);
	}
	led_pattern[LED_SEQ_XRAY].num_frames_used = 14;

	//Set standby animation
	led_pattern[LED_SEQ_STANDBY].num_frames_used = 2;
	led_pattern[LED_SEQ_STANDBY].frames[0].ms = 360;
	led_pattern[LED_SEQ_STANDBY].frames[1].ms = 360;
	set_led_frame_single(LED_SEQ_STANDBY, 0, 0, 0, 0);
	set_led_frame_single(LED_SEQ_STANDBY, 1, 0, 0, 120);

	//Set fault animation
	led_pattern[LED_SEQ_FAULT].num_frames_used = 2;
	led_pattern[LED_SEQ_FAULT].frames[0].ms = 360;
	led_pattern[LED_SEQ_FAULT].frames[1].ms = 360;
	set_led_frame_single(LED_SEQ_FAULT, 0, 0, 0, 0);
	set_led_frame_single(LED_SEQ_FAULT, 1, 200, 0, 0);

	max_led_frame = led_pattern[led_sequence_idx].num_frames_used;

	//FAULT is red 200 only at a rate of 400 ms on, 400 ms off
	//warmup is 125,0,125 circling at 200 ms (4 frames)
	//primed is 0, 150, 0 static (1 sec)
	//setup is 200, 40, 0 and 0, 0, 150 opposite chasing at 300 ms (4 frames)
	//ready is 0, 150, 0 and 0, 0, 150 half and half at 500 ms toggle
	//xray is 180, 55, 0 to 250, 76, 0 and then back in steps of 10, 3, 0 (7 up from 18 to 24 inclusive, 7 down from 25 to 19 inclusive)
	//standby is cold
	//fault is warmup fault
}

static void set_led_frame_single(uint8_t pattern_idx, uint8_t frame_idx, uint8_t r, uint8_t g, uint8_t b)
{
	//Do nothing if invalid inputs
	if(pattern_idx >= NUM_LED_SEQUENCES || frame_idx >= MAX_FRAMES_PER_SEQUENCE)
	{
		return;
	}

	for(int i = 0; i < NUM_LED_SECTORS; i++)
	{
		led_pattern[pattern_idx].frames[frame_idx].color[i][LED_R_VAL] = r;
		led_pattern[pattern_idx].frames[frame_idx].color[i][LED_G_VAL] = g;
		led_pattern[pattern_idx].frames[frame_idx].color[i][LED_B_VAL] = b;
	}
}

void process_leds()
{
	//Once LED bus is done writing to first set, proceed to second set
	if(led_i2c_state == LED_BUS_WAITING_2)
	{
		led_i2c_state = LED_BUS_WRITING_2;
		HAL_I2C_Mem_Write_IT(&hi2c1, TLC59116_2_ADDR, TLC59116_BRIGHTNESS_REG, I2C_MEMADD_SIZE_8BIT, tlc59116_2_tx, NUM_CHANNELS_PER_DRIVER);
	}

	//Wait until current LED frame is finished
	if(update_led_ms >= 0)
	{
		return;
	}

	//Wait until I2C bus is free
	if(led_i2c_state == LED_BUS_WAITING_1)
	{
		//Write out data to led drivers
		output_led_frame();

		//Get the next frame in the current sequence
		led_frame_idx++;
		led_frame_idx %= max_led_frame;

		update_led_ms = led_pattern[led_sequence_idx].frames[led_frame_idx].ms;
	}

}

static void output_led_frame()
{
	int tx_idx = 0;
	uint8_t color_output = 0;

//For proof of concept only
#ifdef OCTANT_TEST
	//Set values for first LED driver
	for(int c = 0; c < NUM_LED_COLORS; c++)
	{
		for(int q = 0; q < 4; q++)
		{
			color_output = led_pattern[led_sequence_idx].frames[led_frame_idx].color[q][c];
			tlc59116_1_tx[tx_idx++] = color_output;
		}
	}

	//Set values for second LED driver
	tx_idx = 0;
	for(int c = 0; c < NUM_LED_COLORS; c++)
	{
		for(int q = 4; q < 8; q++)
		{
			color_output = led_pattern[led_sequence_idx].frames[led_frame_idx].color[q][c];
			tlc59116_2_tx[tx_idx++] = color_output;
		}
	}

//For normal execution
#else
	//Set values for first LED driver
	for(int c = 0; c < NUM_LED_COLORS; c++)
	{
		for(int q = 0; q < 2; q++)
		{
			color_output = led_pattern[led_sequence_idx].frames[led_frame_idx].color[q][c];
			tlc59116_1_tx[tx_idx++] = color_output;
			tlc59116_1_tx[tx_idx++] = color_output;
		}
	}

	//Set values for second LED driver
	tx_idx = 0;
	for(int c = 0; c < NUM_LED_COLORS; c++)
	{
		for(int q = 2; q < 4; q++)
		{
			color_output = led_pattern[led_sequence_idx].frames[led_frame_idx].color[q][c];
			tlc59116_2_tx[tx_idx++] = color_output;
			tlc59116_2_tx[tx_idx++] = color_output;
		}
	}
#endif

	led_i2c_state = LED_BUS_WRITING_1;
	HAL_I2C_Mem_Write_IT(&hi2c1, TLC59116_1_ADDR, TLC59116_BRIGHTNESS_REG, I2C_MEMADD_SIZE_8BIT, tlc59116_1_tx, NUM_CHANNELS_PER_DRIVER);
}

void set_new_led_sequence(int idx)
{
	//Make sure idx is valid and also not the current idx
	if(idx < NUM_LED_SEQUENCES && idx != led_sequence_idx)
	{
		led_frame_idx = 0;
		led_sequence_idx = idx;
		max_led_frame = led_pattern[led_sequence_idx].num_frames_used;
		max_led_frame %= MAX_FRAMES_PER_SEQUENCE;
		update_led_ms = 0;
		//report_led_sequence(idx);			//TODO: Restore after debug session 10-15-25
	}
}

void led_tx_cb()
{
	//HAL_GPIO_TogglePin(IO_LED_AMBER_GPIO_Port, IO_LED_AMBER_Pin);
	if(led_i2c_state == LED_BUS_WRITING_1)
	{
		led_i2c_state = LED_BUS_WAITING_2;
	}
	else
	{
		led_i2c_state = LED_BUS_WAITING_1;
	}
}
