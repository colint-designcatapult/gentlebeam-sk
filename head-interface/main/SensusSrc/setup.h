#ifndef SENSUSSRC_SETUP_H_
#define SENSUSSRC_SETUP_H_

#include "main.h"

#if !defined(CALIBRATION_MODE)

#define FW_MAJOR_VERSION	02
#define FW_MINOR_VERSION	00
#define FW_LEVEL_VERSION	01

#else

#define FW_MAJOR_VERSION	1
#define FW_MINOR_VERSION	0
#define FW_LEVEL_VERSION	0

#endif

void run_setup();
void run_post();
void run_loop();

#endif /* SENSUSSRC_SETUP_H_ */
