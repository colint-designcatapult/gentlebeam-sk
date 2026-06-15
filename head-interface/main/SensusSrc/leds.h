#ifndef SENSUSSRC_LEDS_H_
#define SENSUSSRC_LEDS_H_

#define NUM_LED_SECTORS		8
//#define OCTANT_TEST			1
#define NUM_CHANNELS_PER_DRIVER		12
#define MAX_FRAMES_PER_SEQUENCE	16

#define TLC59116_MODE_REG			(0x00 | 0x80)
#define TLC59116_BRIGHTNESS_REG		(0x02 | 0x80)
#define TLC59116_OUTPUT_REG			(0x14 | 0x80)
#define TLC59116_1_ADDR				(0b01100000 << 1)
#define TLC59116_2_ADDR				(0b01100001 << 1)
//#define TLC59116_1_ADDR				(0b01010100 << 1)
//#define TLC59116_2_ADDR				(0b01110001 << 1)

enum
{
	LED_R_VAL = 0,
	LED_G_VAL,
	LED_B_VAL,
	NUM_LED_COLORS
};

enum
{
	LED_SEQ_OFF= 0,
	LED_SEQ_COLD,
	LED_SEQ_WARMUP,
	LED_SEQ_WARMUP_FAULT,
	LED_SEQ_PRIMED,
	LED_SEQ_SETUP,
	LED_SEQ_READY,
	LED_SEQ_XRAY,
	LED_SEQ_STANDBY,
	LED_SEQ_FAULT,
	NUM_LED_SEQUENCES = 24
};

typedef enum LedBusState
{
	LED_BUS_WAITING_1 = 0,
	LED_BUS_WRITING_1,
	LED_BUS_WAITING_2,
	LED_BUS_WRITING_2,
	LED_BUS_ERROR
} led_bus_state;

typedef struct LedFrame
{
	int32_t ms;
	uint8_t color[NUM_LED_SECTORS][NUM_LED_COLORS];
} led_frame;

typedef struct LedSequence
{
	uint8_t num_frames_used;
	led_frame frames[MAX_FRAMES_PER_SEQUENCE];
} led_sequence;

void init_leds();
void process_leds();
void led_tx_cb();
void set_new_led_sequence(int idx);

#endif /* SENSUSSRC_LEDS_H_ */
