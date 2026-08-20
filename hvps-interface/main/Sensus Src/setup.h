#ifndef SETUP_H_
#define SETUP_H_
#ifndef FW_VERSION
#define FW_VERSION "0.0.0-local.0"
#endif

#define HVPS_NORMAL_MODE 0x0
#define HVPS_CALIBRATION_MODE 0x494C4143

void run_setup();
void run_loop();

#endif /* SETUP_H_ */
