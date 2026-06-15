/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: External Timers
*	Author: Carlton Chow
*	Description:
*/


#ifndef EXT_TIMERS_H_
#define EXT_TIMERS_H_

#define PRIMARY_TIMER_ADDR		0xB4
#define SECONDARY_TIMER_ADDR	0xB5
#define TIMER_COMM_TIMEOUT_MS	400

enum
{
	TIMER_SET_CMD_bp = 0,
	TIMER_PAUSE_CMD_bp,
	TIMER_CLEAR_CMD_bp
};

#define TIMER_CMD_READ			0
#define TIMER_CMD_SET_TIME		1 << TIMER_SET_CMD_bp
#define TIMER_CMD_PAUSE			1 << TIMER_PAUSE_CMD_bp
#define TIMER_CMD_CLEAR			1 << TIMER_CLEAR_CMD_bp

#define MAX_TIMER_SECONDS		3600
#define TICKS_PER_SECOND		32768
#define TIMER_COUNT_OVERHEAD	0.5

enum
{
	TIMER_RX_STATE,
	TIMER_RX_TIME_0,
	TIMER_RX_TIME_1,
	TIMER_RX_TIME_2,
	TIMER_RX_TIME_3,
	TIMER_RX_CHECK,
	TIMER_RX_SIZE
};

enum
{
	TIMER_TX_CMD,
	TIMER_TX_TIME_0,
	TIMER_TX_TIME_1,
	TIMER_TX_TIME_2,
	TIMER_TX_TIME_3,
	TIMER_TX_CHECK,
	TIMER_TX_SIZE
};

extern uint32_t ext_timer_tick_start;

void init_ext_timers();
void process_ext_timers();

void set_new_timer_value(float seconds);
void start_ext_timers();
void pause_ext_timers();
void clear_ext_timers();



#endif /* EXT_TIMERS_H_ */