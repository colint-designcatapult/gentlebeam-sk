#ifndef EXT_DACS_H_
#define EXT_DACS_H_

enum
{
	KV_DAC = 0,
	FIL_DAC,
	MA_LIM_DAC,
	GRID_DAC,
	NUM_EXT_DACS
};

void setup_ext_dacs();
void process_ext_dacs();
void ext_dac_tx_done();

void write_kv(float kv);
void write_ma_lim(float ma_lim);
#ifndef CALIBRATION_MODE
void write_fil_v(float fil_v);
void write_fil_raw(int16_t fil_raw);
#endif
void write_fil_a(float fil_v);
void write_grid(float grid);


#endif /* EXT_DACS_H_ */
