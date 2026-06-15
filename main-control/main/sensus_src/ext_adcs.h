/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: External ADCs
*	Author: Carlton Chow
*	Description:
*/


#ifndef EXT_ADCS_H_
#define EXT_ADCS_H_

#define IO_TEST_LED_1		GPIO(GPIO_PORTD, 12)

#define COIL_ADC_ADDR		0b1001001
#define SYS_ADC_ADDR		0b1001000
#define ION_R_ADC_ADDR		0b0110110

#define ADC_RX_SIZE				2
#define ADC_TX_SIZE				1
#define ADC_COMM_TIMEOUT_MS		100

#define ADC_SAMPLE_BUF_SIZE		16

#define ADS7828_CMD_BYTE		0b10000100
#define ADS7828_CH(ch)			((ch & 0x07) << 4)

#define MAX11647_SETUP_BYTE		0b11010010
#define MAX11647_CONFIG_BYTE	0b01100001
#define MAX11647_CH(ch)			((ch & 0x01) << 1)
#define MAX_ADC_SETUP_RETRIES	5

#define ADS7828_ADC_SCALING		819.2
#define MAX11647_ADC_SCALING	500


enum
{
	EXT_ADC_CH_TEMP = 0,
	EXT_ADC_CH_F_V,
	EXT_ADC_CH_Y_V,
	EXT_ADC_CH_X_V,
	EXT_ADC_CH_F_I,
	EXT_ADC_CH_Y_I,
	EXT_ADC_CH_X_I,
	EXT_ADC_COIL_CNT
};

enum
{
	EXT_ADC_CH_12V = 0,
	EXT_ADC_CH_5V,
	EXT_ADC_CH_3V3,
	EXT_ADC_CH_IP_I1,
	EXT_ADC_CH_IP_I2,
	EXT_ADC_CH_IP_V,
	EXT_ADC_CH_CB_THERM,
	EXT_ADC_CH_HS_THERM,
	EXT_ADC_SYS_CNT
};

enum
{
	EXT_ADC_CH_REPELLER_V = 0,
	EXT_ADC_CH_REPELLER_I,
	EXT_ADC_ION_R_CNT
};

void init_ext_adcs();
void process_ext_adcs();



#endif /* EXT_ADCS_H_ */