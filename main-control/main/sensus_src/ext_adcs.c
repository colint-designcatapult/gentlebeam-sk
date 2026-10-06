/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: External ADCs
*	Author: Carlton Chow
*	Description:


Note to self, ext timers are started synchronously with main loop
because write values can change and need to be checked before being written
In comparison, ADC values are continuously read and just copied to sync buffer
since the write command values to the ADCs are static
*/

#include <atmel_start.h>
#include "system_parameters.h"
#include "ext_adcs.h"
#include "ext_adcs_i2c.h"

volatile bool adc_bus_stuck = false;
volatile bool adc_check_ready = false;

uint16_t adc_coil_output_buf[EXT_ADC_COIL_CNT][ADC_SAMPLE_BUF_SIZE];
uint16_t adc_sys_output_buf[EXT_ADC_SYS_CNT][ADC_SAMPLE_BUF_SIZE];
uint16_t adc_ion_r_output_buf[EXT_ADC_ION_R_CNT][ADC_SAMPLE_BUF_SIZE];

static uint32_t adc_output_idx;
static struct timer_task VTIMER_ext_adc_check;

static float get_ads7828_voltage(uint16_t *adc_data);
static float get_max11647_voltage(uint16_t *adc_data);
static void ext_adc_comm_timeout_check(const struct timer_task *const timer_task);

void init_ext_adcs(void)
{
	VTIMER_ext_adc_check.interval = ADC_COMM_TIMEOUT_MS;
	VTIMER_ext_adc_check.cb = ext_adc_comm_timeout_check;
	VTIMER_ext_adc_check.mode = TIMER_TASK_REPEAT;

	adc_output_idx = 0;
	adc_check_ready = false;
	ext_adcs_i2c_init();
	ext_adcs_i2c_start_scan();
}

static void ext_adc_comm_timeout_check(const struct timer_task *const timer_task)
{
	(void)timer_task;
	adc_bus_stuck = true;
}

void ext_adcs_i2c_scan_complete(const uint16_t coil[EXT_ADC_COIL_CNT],
	const uint16_t sys[EXT_ADC_SYS_CNT],
	const uint16_t ion_r[EXT_ADC_ION_R_CNT])
{
	if (adc_check_ready) {
		return;
	}

	if (++adc_output_idx >= ADC_SAMPLE_BUF_SIZE) {
		adc_output_idx = 0;
	}

	for (uint32_t i = 0; i < EXT_ADC_COIL_CNT; i++) {
		adc_coil_output_buf[i][adc_output_idx] = coil[i];
	}
	for (uint32_t i = 0; i < EXT_ADC_SYS_CNT; i++) {
		adc_sys_output_buf[i][adc_output_idx] = sys[i];
	}
	for (uint32_t i = 0; i < EXT_ADC_ION_R_CNT; i++) {
		adc_ion_r_output_buf[i][adc_output_idx] = ion_r[i];
	}

	adc_check_ready = true;
}

void process_ext_adcs(void)
{
	if (!adc_check_ready) {
		return;
	}

	report_ext_adc_f_coil_v(get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_F_V]));
	report_ext_adc_f_coil_cur(get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_F_I]));
	report_ext_adc_x_coil_v(get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_X_V]));
	report_ext_adc_x_coil_cur(get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_X_I]));
	report_ext_adc_y_coil_v(get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_Y_V]));
	report_ext_adc_y_coil_cur(get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_Y_I]));
	report_ext_adc_12_v(get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_12V]));
	report_ext_adc_5_v(get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_5V]));
	report_ext_adc_3p3_v(get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_3V3]));
	report_ext_adc_ion_pump(get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_IP_I2]));
	report_ext_adc_cab_temp(get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_CB_THERM]));
	report_ext_adc_hs_temp(get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_HS_THERM]));
	report_ext_adc_ion_rep_v(get_max11647_voltage(adc_ion_r_output_buf[EXT_ADC_CH_REPELLER_V]));
	report_ext_adc_ion_rep_cur(get_max11647_voltage(adc_ion_r_output_buf[EXT_ADC_CH_REPELLER_I]));

	adc_check_ready = false;
	ext_adcs_i2c_start_scan();
}

static float get_max11647_voltage(uint16_t *adc_data)
{
	uint32_t adc_sum = 0;

	for (uint32_t i = 0; i < ADC_SAMPLE_BUF_SIZE; i++) {
		adc_sum += adc_data[i] & 0x3ffU;
	}

	return (float)adc_sum / (MAX11647_ADC_SCALING * ADC_SAMPLE_BUF_SIZE);
}

static float get_ads7828_voltage(uint16_t *adc_data)
{
	uint32_t adc_sum = 0;

	for (uint32_t i = 0; i < ADC_SAMPLE_BUF_SIZE; i++) {
		adc_sum += adc_data[i];
	}

	return (float)adc_sum / (ADS7828_ADC_SCALING * ADC_SAMPLE_BUF_SIZE);
}