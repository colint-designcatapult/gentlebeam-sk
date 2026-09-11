#ifndef MAGNETOMETER_MONITORING_H_
#define MAGNETOMETER_MONITORING_H_

#include <stdbool.h>

#define MAGNETOMETER_BASELINE_SAMPLE_COUNT 3u
#define MAGNETOMETER_DEVIATION_TOLERANCE_PERCENT 10.0f
#define MAGNETOMETER_MINIMUM_DEVIATION_UT 1.0f
#define MAGNETOMETER_CONSECUTIVE_DEVIATION_COUNT 3u

void start_magnetometer_baseline_collection(void);
bool capture_magnetometer_baseline(void);
void monitor_magnetometer_readings(void);

#endif /* MAGNETOMETER_MONITORING_H_ */
