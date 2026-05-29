#ifndef SENSUSSRC_MAGNETOMETER_H_
#define SENSUSSRC_MAGNETOMETER_H_

#define NUM_MAG_SAMPLES	500
#define NUM_MAG_AXIS	3
#define NUM_MAG_FIELDS	(2*NUM_MAG_AXIS)
#define KMX62_ADDR		(0x0E<<1)
#define KMX62_ADDR_2	(0x0F<<1)
#define KMX62_ACL_REG	0x0A
#define KMX62_MAG_REG	0x10
#define KMX62_MAG_CNT	6
#define KMX62_RATE_REG	0x38
#define KMX62_RATE_VAL	0x44
#define KMX62_WHO_AM_I	0x00
#define KMX62_CTRL_REG_1	0x39
#define KMX62_CTRL_REG_2	0x3A
#define KMX62_CTRL_VAL	0x0F

#define MISSING_MAG_RX_COUNT	5

void init_magnetometer();
void process_magnetometer();
void mag_i2c_rx_cb(int bus);


#endif /* SENSUSSRC_MAGNETOMETER_H_ */
