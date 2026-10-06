#ifndef EXT_DAC_SPI_H_
#define EXT_DAC_SPI_H_

#include <stdint.h>

void init_ext_dac_spi(void);
void ext_dac_spi_write(const uint8_t *data, uint16_t length);
void ext_dac_spi_transfer_complete(void);

#endif
