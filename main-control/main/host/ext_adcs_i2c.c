#include "ext_adcs_i2c.h"
#include "hardware_backend.h"

#define ADS7828_RAW_MAX 0x0fffU
#define MAX11647_RAW_MAX 0x03ffU

static uint16_t ads7828_voltage_to_raw(float voltage);
static uint16_t max11647_voltage_to_raw(float voltage);

void ext_adcs_i2c_init(void)
{
	/*
	 * Model values are ADC input voltages.  The firmware applies the board
	 * dividers: 12 V / 6 and 5 V / 2.  The thermistor defaults are 25 °C
	 * under the existing cabinet lookup table and heatsink Beta equation.
	 */
	gIoModel.adcs.system.voltage_12 = 2.0f;
	gIoModel.adcs.system.voltage_5 = 2.5f;
	gIoModel.adcs.system.voltage_3p3 = 3.3f;
	gIoModel.adcs.system.cabinet_thermistor = 3.325094f;
	gIoModel.adcs.system.heatsink_thermistor = 2.5f;

	/*
	 * The ion-pump monitor converts this ADC input to
	 * 10^(-12 + 2 * voltage) Torr.  2 V yields 1e-8 Torr, well below the
	 * 5e-6 Torr fault threshold.  The repeller monitor expects 150 V after
	 * its 101:1 scale factor, so its ADC input is 150 / 101 V.
	 */
	gIoModel.adcs.system.ion_pump_current_2 = 2.0f;
	gIoModel.adcs.ion_repeller.repeller_voltage = 150.0f / 101.0f;
	gIoModel.adcs.ion_repeller.repeller_current = 0.0f;
}

void ext_adcs_i2c_start_scan(void)
{
	const GcbExtAdcsModel *const model = &gIoModel.adcs;
	uint16_t coil[EXT_ADC_COIL_CNT];
	uint16_t system[EXT_ADC_SYS_CNT];
	uint16_t ion_repeller[EXT_ADC_ION_R_CNT];

	coil[EXT_ADC_CH_TEMP] = ads7828_voltage_to_raw(model->coil.temperature);
	coil[EXT_ADC_CH_F_V] = ads7828_voltage_to_raw(model->coil.f_voltage);
	coil[EXT_ADC_CH_Y_V] = ads7828_voltage_to_raw(model->coil.y_voltage);
	coil[EXT_ADC_CH_X_V] = ads7828_voltage_to_raw(model->coil.x_voltage);
	coil[EXT_ADC_CH_F_I] = ads7828_voltage_to_raw(model->coil.f_current);
	coil[EXT_ADC_CH_Y_I] = ads7828_voltage_to_raw(model->coil.y_current);
	coil[EXT_ADC_CH_X_I] = ads7828_voltage_to_raw(model->coil.x_current);

	system[EXT_ADC_CH_12V] = ads7828_voltage_to_raw(model->system.voltage_12);
	system[EXT_ADC_CH_5V] = ads7828_voltage_to_raw(model->system.voltage_5);
	system[EXT_ADC_CH_3V3] = ads7828_voltage_to_raw(model->system.voltage_3p3);
	system[EXT_ADC_CH_IP_I1] = ads7828_voltage_to_raw(model->system.ion_pump_current_1);
	system[EXT_ADC_CH_IP_I2] = ads7828_voltage_to_raw(model->system.ion_pump_current_2);
	system[EXT_ADC_CH_IP_V] = ads7828_voltage_to_raw(model->system.ion_pump_voltage);
	system[EXT_ADC_CH_CB_THERM] = ads7828_voltage_to_raw(model->system.cabinet_thermistor);
	system[EXT_ADC_CH_HS_THERM] = ads7828_voltage_to_raw(model->system.heatsink_thermistor);

	ion_repeller[EXT_ADC_CH_REPELLER_V] =
		max11647_voltage_to_raw(model->ion_repeller.repeller_voltage);
	ion_repeller[EXT_ADC_CH_REPELLER_I] =
		max11647_voltage_to_raw(model->ion_repeller.repeller_current);

	ext_adcs_i2c_scan_complete(coil, system, ion_repeller);
}

static uint16_t ads7828_voltage_to_raw(float voltage)
{
	if (voltage <= 0.0f) {
		return 0;
	}
	if (voltage >= (float)ADS7828_RAW_MAX / ADS7828_ADC_SCALING) {
		return ADS7828_RAW_MAX;
	}
	return (uint16_t)(voltage * ADS7828_ADC_SCALING + 0.5f);
}

static uint16_t max11647_voltage_to_raw(float voltage)
{
	if (voltage <= 0.0f) {
		return 0;
	}
	if (voltage >= (float)MAX11647_RAW_MAX / MAX11647_ADC_SCALING) {
		return MAX11647_RAW_MAX;
	}
	return (uint16_t)(voltage * MAX11647_ADC_SCALING + 0.5f);
}
