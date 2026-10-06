#include <stddef.h>
#include <atmel_start.h>
#include "ext_dac.h"
#include "ext_dac_spi.h"
#include "hardware_backend.h"

static void update_fan_dac(uint8_t channel, uint16_t value);
static void update_coil_dac(uint8_t channel, uint16_t value);

void init_ext_dac_spi(void)
{
}

void ext_dac_spi_write(const uint8_t *data, uint16_t length)
{
	if (data != NULL && length == NUM_DAC_CMD_BYTES &&
		(data[0] & 0xf0U) == DAC_CODE_LOAD_CMD) {
		const uint8_t channel = data[0] & 0x03U;
		const uint16_t value = ((uint16_t)data[1] << 8 | data[2]) >> 4;

		if (!gpio_get_pin_level(IO_FAN_DAC_CSn)) {
			update_fan_dac(channel, value);
		}
		if (!gpio_get_pin_level(IO_COIL_DAC_CSn)) {
			update_coil_dac(channel, value);
		}
	}

	ext_dac_spi_transfer_complete();
}

static void update_fan_dac(uint8_t channel, uint16_t value)
{
	switch (channel) {
	case HS_FAN_DAC_CH:
		gIoModel.dac.fan.heatsink = (float)value / DAC_F_TO_12_FACTOR;
		break;
	case CB_FAN_DAC_CH:
		gIoModel.dac.fan.cabinet = (float)value / DAC_F_TO_12_FACTOR;
		break;
	case PUMP_FAN_DAC_CH:
		gIoModel.dac.fan.pump = (float)value / DAC_F_TO_12_FACTOR;
		break;
	default:
		break;
	}
}

static void update_coil_dac(uint8_t channel, uint16_t value)
{
	// If simulating, write voltage to feedback
	bool sim = gIoModel.dac.coil.simulate;


	switch (channel) {
	case X_COIL_DAC_CH:
		gIoModel.dac.coil.x = (float)value / DAC_F_TO_12_FACTOR;
		if (sim) {
			gIoModel.adcs.coil.x_voltage = gIoModel.dac.coil.x;
			/* 2.5 V/A command; bipolar feedback is 2.5 V + I(mA)/600. */
			const float direction = gpio_get_pin_level(IO_COIL_X_DIRn) ? 1.0f : -1.0f;
			gIoModel.adcs.coil.x_current = 2.5f + direction * gIoModel.dac.coil.x * (1000.0f / (2.5f * 600.0f));
		}
		break;
	case Y_COIL_DAC_CH:
		gIoModel.dac.coil.y = (float)value / DAC_F_TO_12_FACTOR;
		if (sim) {
			gIoModel.adcs.coil.y_voltage = gIoModel.dac.coil.y;
			const float direction = gpio_get_pin_level(IO_COIL_Y_DIRn) ? 1.0f : -1.0f;
			gIoModel.adcs.coil.y_current = 2.5f + direction * gIoModel.dac.coil.y * (1000.0f / (2.5f * 600.0f));
		}
		break;
	case F_COIL_DAC_CH:
		gIoModel.dac.coil.f = (float)value / DAC_F_TO_12_FACTOR;
		if (sim) {
			gIoModel.adcs.coil.f_voltage = gIoModel.dac.coil.f;
			/* Focus command is 1.666 V/A with unipolar 600 mA/V feedback. */
			gIoModel.adcs.coil.f_current = gIoModel.dac.coil.f * (1000.0f / (1.666f * 600.0f));
		}
		break;
	default:
		break;
	}
}
