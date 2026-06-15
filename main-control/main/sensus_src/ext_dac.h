/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: External DAC
*	Author: Carlton Chow
*	Description:
*/


#ifndef EXT_DAC_H_
#define EXT_DAC_H_

#define NUM_DAC_CMD_BYTES		3
#define MAX_DAC_VOLTAGE			4.999f
#define DAC_F_TO_12_FACTOR		819.2
#define DAC_CODE_CMD			0x00
#define DAC_CODE_LOAD_CMD		0x30
#define MAX_FAN_CTRL_VOLTAGE	5.0f

#define DAC_CONFIG_VAL_0		0b01101000
#define DAC_CONFIG_VAL_1		0b00001111

#define DAC_REF_CONFIG_VAL		0b01110000

#define MIN_COIL_VOLTAGE		-5.0f

enum
{
	HS_FAN_DAC_CH = 0,
	CB_FAN_DAC_CH,
	PUMP_FAN_DAC_CH,
	NUM_FAN_DAC_CH
};

enum
{
	X_COIL_DAC_CH = 0,
	Y_COIL_DAC_CH,
	F_COIL_DAC_CH,
	NUM_COIL_DAC_CH
};


void init_ext_dac();
void process_ext_dac();

void set_coil_voltage(uint32_t coil_ch, float voltage);
void set_fan_voltage(uint32_t fan_ch, float voltage);

#endif /* EXT_DAC_H_ */