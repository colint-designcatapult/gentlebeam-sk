

#ifndef EXT_ADCS_H_
#define EXT_ADCS_H_

#define EXT_ADC_KV_DONE		0x01
#define EXT_ADC_MA_DONE		0x02
#define EXT_ADC_DONE		(EXT_ADC_MA_DONE | EXT_ADC_KV_DONE)


//TBD TODO array size placeholder
#define EXT_ADC_KV_BUF_SIZE		12
#define EXT_ADC_MA_BUF_SIZE		32
#define EXT_ADC_MA_MEDIAN_SIZE	3

void setup_ext_adcs();
void process_ext_adcs();
void ext_kv_rx_done();
void ext_ma_rx_done();
void spi1_error_handler(void);
void spi3_error_handler(void);

#endif /* EXT_ADCS_H_ */
