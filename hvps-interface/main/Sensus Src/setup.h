#ifndef SETUP_H_
#define SETUP_H_

#ifdef CALIBRATION_MODE
#define FW_MAJOR_VERSION	1
#define FW_MINOR_VERSION	0
#define FW_LEVEL_VERSION	0
#else
#define FW_MAJOR_VERSION	2
#define FW_MINOR_VERSION	0
#define FW_LEVEL_VERSION	3
#endif

void run_setup();
void run_loop();

#endif /* SETUP_H_ */
