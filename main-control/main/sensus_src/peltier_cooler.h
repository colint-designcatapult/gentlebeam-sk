/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: Peltier cooler
*	Author: Carlton Chow
*	Description:
*/


#ifndef PELTIER_COOLER_H_
#define PELTIER_COOLER_H_

#define PLT_MAX_NO_RESPONSE			5
#define PLT_MAX_INVALID_RESPONSE	5
#define PLT_CHECK_MS				300

#define PLT_TEMP_SCALE_FACTOR		10

enum
{
	PLT_RX_START_BYTE = 0,
	PLT_RX_DATA_BYTE_0,
	PLT_RX_DATA_BYTE_1,
	PLT_RX_DATA_BYTE_2,
	PLT_RX_DATA_BYTE_3,
	PLT_RX_CHECK_BYTE_0,
	PLT_RX_CHECK_BYTE_1,
	PLT_RX_END_BYTE,
	PLT_RX_COUNT
};

enum
{
	PLT_CMD_TEMP_QUERY = 0,
	PLT_CMD_OFF,
	PLT_CMD_ON,
	PLT_CMD_SET_TEMP
};

void init_plt_cooler();
void process_plt();
void enable_plt(bool en);
void set_plt_temperature(uint32_t temperature);


#endif /* PELTIER_COOLER_H_ */