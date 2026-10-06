#include <atmel_start.h>

#include "ext_dac_spi.h"

static struct io_descriptor *dac_io;

static void dac_transfer_complete(const struct spi_m_async_descriptor *const io_descr)
{
	(void)io_descr;
	ext_dac_spi_transfer_complete();
}

void init_ext_dac_spi(void)
{
	spi_m_async_get_io_descriptor(&DAC_SPI, &dac_io);
	spi_m_async_register_callback(&DAC_SPI, SPI_M_ASYNC_CB_XFER, (FUNC_PTR)dac_transfer_complete);
	spi_m_async_enable(&DAC_SPI);
}

void ext_dac_spi_write(const uint8_t *data, uint16_t length)
{
	io_write(dac_io, data, length);
}
