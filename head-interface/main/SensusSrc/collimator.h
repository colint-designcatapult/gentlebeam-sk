/*
 * collimator.h
 *
 *  Created on: Dec 5, 2024
 *      Author: Carlton
 */

#ifndef COLLIMATOR_H_
#define COLLIMATOR_H_

#define COL_TRANSC_ADDR	0x18
//#define COL_TRANSC_ADDR	0x30

enum
{
	COL_STATE_IDLE = 0,
	COL_STATE_SENDING_RESET,
	COL_STATE_READING_RESET,
	COL_STATE_ROM_START,
	COL_STATE_ROM_READ_REQ,
	COL_STATE_ROM_READ_SET,
	COL_STATE_ROM_READ_WAIT
};

void init_collimator();
void process_collimator();


#endif /* COLLIMATOR_H_ */
