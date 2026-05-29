#include "stdbool.h"
#include "main.h"
#include "stm32f3xx_hal.h"

#include "control_comm.h"
#include "ext_dacs.h"
#include "monitoring.h"


volatile bool dac_busy;
int16_t new_dac_val[NUM_EXT_DACS];
uint8_t dac_tx_buf[2];

static void write_ext_dac(int idx, int16_t val);


void setup_ext_dacs()
{
	HAL_GPIO_WritePin(GPIOD, IO_FIL_DAC_CS_Pin|IO_MA_DAC_CS_Pin|IO_KV_DAC_CS_Pin|IO_GRID_DAC_CS_Pin, GPIO_PIN_SET);

	//Initialize a dummy write to the SPI line to set clock pin to known state
	dac_busy = true;
	HAL_SPI_Transmit_IT(&hspi2, dac_tx_buf, 2);

	//Initialize all DACs with new 0 writes
	for(int i = 0; i < NUM_EXT_DACS; i++)
	{
		new_dac_val[i] = 0;//08000;
	}

}

void process_ext_dacs()
{
	//Do nothing until DAC bus is free
	if(dac_busy)
	{
		return;
	}

	//Check to see if any new DAC values need to be written
	for(int i = 0; i < NUM_EXT_DACS; i++)
	{
		if(new_dac_val[i] >= 0)
		{
			write_ext_dac(i, new_dac_val[i]);
			new_dac_val[i] = -1;
			break;
		}
	}

}

static void write_ext_dac(int idx, int16_t val)
{
	if(val < 0)
	{
		return;
	}

	switch(idx)
	{
		case KV_DAC:
			HAL_GPIO_WritePin(GPIOD, IO_KV_DAC_CS_Pin, GPIO_PIN_RESET);
			break;
		case MA_LIM_DAC:
			HAL_GPIO_WritePin(GPIOD, IO_MA_DAC_CS_Pin, GPIO_PIN_RESET);
			break;
		case FIL_DAC:
			HAL_GPIO_WritePin(GPIOD, IO_FIL_DAC_CS_Pin, GPIO_PIN_RESET);
			break;
		case GRID_DAC:
			HAL_GPIO_WritePin(GPIOD, IO_GRID_DAC_CS_Pin, GPIO_PIN_RESET);
			break;
		default:
			break;
	}

	//Copy bytes individually to buffer to ensure endianness
	dac_tx_buf[1] = (uint8_t)(val & 0xFF);
	dac_tx_buf[0] = (uint8_t)((val>>8) & 0xFF);

	//Start transfer
	dac_busy = true;
	HAL_SPI_Transmit_IT(&hspi2, dac_tx_buf, 2);
}

void write_kv(float kv)
{
	float max_kv = config_vals[SYS_CONFIG_MAX_KV];
	float scale = 27.30666666666665;

	int16_t kv_raw = -1;
	if(kv >= 0 && kv <= max_kv)
	{
		kv_raw = (int16_t)(kv * scale);
		//kv_raw is sent to an external 12bit DAC referenced to 5V (meaning that 4095 is 5V for 150kV) and amplified x3 using IC U400A
	}

	new_dac_val[KV_DAC] = kv_raw;
}

void write_ma_lim(float ma_lim)
{
	float max_ma = config_vals[SYS_CONFIG_MAX_MA];
	float multiplier = 273.0665;

	int16_t ma_lim_raw = -1;

	if(ma_lim >= 0 && ma_lim <= max_ma)
	{
		setpoints[SP_MA_LIM] = ma_lim;
		ma_lim_raw = (int16_t)(ma_lim * multiplier);
		//ma_lim_raw is sent to an external 12bit DAC referenced to 5V (meaning that 4095 is 5V for 15mA in 400W PS and 7.5mA in 50W PS) and amplified x3 using IC U401A

	}
	new_dac_val[MA_LIM_DAC] = ma_lim_raw;
}
#ifndef CALIBRATION_MODE
void write_fil_v(float fil_v)
{
	int16_t fil_raw = -1;
	if(fil_v >= 0)
	{
		fil_raw = (int16_t)(fil_v);
		//TBD TODO placeholder number (verify scaling with real HVPS)
	}
	new_dac_val[FIL_DAC] = fil_raw;
}
#endif

void write_fil_a(float fil_a)
{
	float scale = 0.8192;
	int16_t fil_raw = -1;

	if(fil_a >= 0)
	{
		fil_raw = (int16_t)(fil_a * scale);
		//fil_raw is sent to an external 12bit DAC referenced to 5V (meaning that 4095 is 5V for 5000mA) and not amplified
	}
	new_dac_val[FIL_DAC] = fil_raw;
#ifndef CALIBRATION_MODE
}

void write_fil_raw(int16_t fil_raw)
{
#endif
	new_dac_val[FIL_DAC] = fil_raw;
}

void write_grid(float grid)
{
	float grid_upper_limit = config_vals[SYS_CONFIG_MAX_GRID];
	float grid_scale = 2.73;

	int grid_raw = -1;

	if(grid >= 0 && grid <= grid_upper_limit)
	{
		setpoints[SP_GRID] = grid;
		grid_raw = (int16_t)(grid * grid_scale);
		//grid_raw is sent to an external 12bit DAC referenced to 5V (meaning that 4095 is 10V for 1000V) and amplified x3 using IC U402A
	}

	new_dac_val[GRID_DAC] = grid_raw;
}

void ext_dac_tx_done()
{
	//On DAC TX completion, reset all CS lines and free DAC processing
	HAL_GPIO_WritePin(GPIOD, IO_FIL_DAC_CS_Pin|IO_MA_DAC_CS_Pin|IO_KV_DAC_CS_Pin|IO_GRID_DAC_CS_Pin, GPIO_PIN_SET);
	dac_busy = false;
}
