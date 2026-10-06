#include <atmel_start.h>
#include "faults.h"
#include "ext_adcs.h"
#include "ext_adcs_i2c.h"

#define I2C_RECOVERY_PULSES 9U
#define TWI2_SDA_PIN PIO_PD27
#define TWI2_SCL_PIN PIO_PD28
#define TWI2_PIO PIOD

static bool adc_write_cycle;
static uint32_t adc_addr;
static uint32_t adc_ch;
static uint32_t adc_rx_idx;
static uint32_t adc_tx_idx;
static uint8_t adc_tx_buf[ADC_TX_SIZE];
static uint16_t adc_coil_rx_buf[EXT_ADC_COIL_CNT];
static uint16_t adc_sys_rx_buf[EXT_ADC_SYS_CNT];
static uint16_t adc_ion_r_rx_buf[EXT_ADC_ION_R_CNT];

static void adc_tx(void);
static void adc_rx(void);
static void save_adc_rx(uint8_t value);
static uint16_t *get_adc_buffer(void);
static void adc_transmission_complete(void);
static void go_to_next_adc_ch(void);
static uint32_t get_max_adc_ch(uint32_t address);
static uint32_t get_next_adc_addr(uint32_t address);
static void set_adc_command(void);
static uint8_t get_ads7828_ch(void);
static bool recover_ext_adc_i2c_bus(void);

void ext_adcs_i2c_init(void)
{
	struct io_descriptor *io;
	uint8_t setup = MAX11647_SETUP_BYTE;
	uint32_t retries = 0;

	i2c_m_sync_enable(&ADC_I2C);
	NVIC_DisableIRQ(TWIHS2_IRQn);
	i2c_m_sync_get_io_descriptor(&ADC_I2C, &io);
	i2c_m_sync_set_slaveaddr(&ADC_I2C, ION_R_ADC_ADDR, I2C_M_SEVEN);

	while (retries < MAX_ADC_SETUP_RETRIES && io_write(io, &setup, 1) != 1) {
		retries++;
	}
	if (retries == MAX_ADC_SETUP_RETRIES && !recover_ext_adc_i2c_bus()) {
		report_typed_fault3(FAULT_ADC_BUS,
			"ADC setup failed at address %u after %u retries (transfer size: %u bytes).",
			MAKE_ARG(ION_R_ADC_ADDR), MAKE_ARG(MAX_ADC_SETUP_RETRIES), MAKE_ARG(1));
	}

	(void)(hri_twihs_read_SR_reg(ADC_I2C.device.hw) &
		hri_twihs_read_IMR_reg(ADC_I2C.device.hw));
	hri_twihs_clear_IMR_reg(ADC_I2C.device.hw,
		TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
	hri_twihs_set_IMR_NACK_bit(ADC_I2C.device.hw);
	NVIC_EnableIRQ(TWIHS2_IRQn);
}

void ext_adcs_i2c_start_scan(void)
{
	adc_ch = 100;
	adc_addr = ION_R_ADC_ADDR;
	go_to_next_adc_ch();
}

static void adc_tx(void)
{
	if (adc_tx_idx < ADC_TX_SIZE) {
		hri_twihs_write_THR_reg(ADC_I2C.device.hw, adc_tx_buf[adc_tx_idx++]);
		return;
	}

	adc_tx_idx = 0;
	hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_TXRDY);
	hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_TXCOMP);
	hri_twihs_write_CR_reg(ADC_I2C.device.hw, TWIHS_CR_STOP);
}

static void adc_rx(void)
{
	save_adc_rx((uint8_t)hri_twihs_read_RHR_reg(ADC_I2C.device.hw));

	if (++adc_rx_idx == ADC_RX_SIZE - 1U) {
		hri_twihs_write_CR_reg(ADC_I2C.device.hw, TWIHS_CR_STOP);
	} else if (adc_rx_idx >= ADC_RX_SIZE) {
		adc_rx_idx = 0;
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_RXRDY);
		hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_TXCOMP);
	}
}

static void save_adc_rx(uint8_t value)
{
	uint16_t *rx_value = get_adc_buffer();

	if (adc_rx_idx >= ADC_RX_SIZE || rx_value == NULL) {
		return;
	}
	if (adc_rx_idx == 0) {
		*rx_value = (uint16_t)value << 8;
	} else {
		*rx_value |= value;
	}
}

static uint16_t *get_adc_buffer(void)
{
	if (adc_addr == COIL_ADC_ADDR && adc_ch < EXT_ADC_COIL_CNT) {
		return &adc_coil_rx_buf[adc_ch];
	}
	if (adc_addr == SYS_ADC_ADDR && adc_ch < EXT_ADC_SYS_CNT) {
		return &adc_sys_rx_buf[adc_ch];
	}
	if (adc_addr == ION_R_ADC_ADDR && adc_ch < EXT_ADC_ION_R_CNT) {
		return &adc_ion_r_rx_buf[adc_ch];
	}
	return NULL;
}

static void adc_transmission_complete(void)
{
	if (adc_write_cycle) {
		hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_RXRDY);
		hri_twihs_write_MMR_reg(ADC_I2C.device.hw,
			TWIHS_MMR_DADR(adc_addr) | TWIHS_MMR_MREAD);
		hri_twihs_write_CR_reg(ADC_I2C.device.hw, TWIHS_CR_START);
		adc_write_cycle = false;
	} else {
		go_to_next_adc_ch();
	}
}

static void go_to_next_adc_ch(void)
{
	adc_rx_idx = 0;
	adc_tx_idx = 0;
	adc_write_cycle = true;

	if (++adc_ch >= get_max_adc_ch(adc_addr)) {
		if (adc_addr == ION_R_ADC_ADDR) {
			ext_adcs_i2c_scan_complete(adc_coil_rx_buf, adc_sys_rx_buf, adc_ion_r_rx_buf);
			return;
		}
		adc_ch = 0;
		adc_addr = get_next_adc_addr(adc_addr);
	}
	set_adc_command();
	hri_twihs_write_MMR_reg(ADC_I2C.device.hw, TWIHS_MMR_DADR(adc_addr));
	hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_TXRDY);
}

static uint32_t get_max_adc_ch(uint32_t address)
{
	switch (address) {
	case COIL_ADC_ADDR:
		return EXT_ADC_COIL_CNT;
	case SYS_ADC_ADDR:
		return EXT_ADC_SYS_CNT;
	case ION_R_ADC_ADDR:
		return EXT_ADC_ION_R_CNT;
	default:
		return 0;
	}
}

static uint32_t get_next_adc_addr(uint32_t address)
{
	switch (address) {
	case COIL_ADC_ADDR:
		return SYS_ADC_ADDR;
	case SYS_ADC_ADDR:
		return ION_R_ADC_ADDR;
	default:
		return COIL_ADC_ADDR;
	}
}

static void set_adc_command(void)
{
	if (adc_addr == COIL_ADC_ADDR || adc_addr == SYS_ADC_ADDR) {
		adc_tx_buf[0] = ADS7828_CMD_BYTE | get_ads7828_ch();
	} else {
		adc_tx_buf[0] = MAX11647_CONFIG_BYTE | MAX11647_CH(adc_ch);
	}
}

static uint8_t get_ads7828_ch(void)
{
	uint8_t channel = (adc_ch & 1U) ? (uint8_t)((adc_ch + 7U) / 2U) : (uint8_t)(adc_ch / 2U);
	return ADS7828_CH(channel);
}

void TWIHS2_Handler(void)
{
	uint32_t status = hri_twihs_read_SR_reg(ADC_I2C.device.hw) &
		hri_twihs_read_IMR_reg(ADC_I2C.device.hw);

	if (status & TWIHS_SR_NACK) {
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw,
			TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		report_typed_fault2(FAULT_ADC_BUS,
			"ADC at address %u returned NACK (transfer size: %u bytes).",
			MAKE_ARG(adc_addr), MAKE_ARG(1));
		adc_ch = 100;
		go_to_next_adc_ch();
	} else if (status & TWIHS_SR_TXCOMP) {
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw,
			TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		adc_transmission_complete();
	} else if (status & TWIHS_SR_TXRDY) {
		adc_tx();
	} else if (status & TWIHS_SR_RXRDY) {
		adc_rx();
	}
}

static bool recover_ext_adc_i2c_bus(void)
{
	bool recovered;

	NVIC_DisableIRQ(TWIHS2_IRQn);
	hri_twihs_write_CR_reg(TWIHS2, TWIHS_CR_MSDIS);
	TWI2_PIO->PIO_PER = TWI2_SDA_PIN | TWI2_SCL_PIN;
	TWI2_PIO->PIO_OER = TWI2_SDA_PIN | TWI2_SCL_PIN;
	TWI2_PIO->PIO_SODR = TWI2_SDA_PIN | TWI2_SCL_PIN;
	delay_us(10);
	TWI2_PIO->PIO_ODR = TWI2_SDA_PIN;

	for (uint32_t i = 0; i < I2C_RECOVERY_PULSES &&
		!(TWI2_PIO->PIO_PDSR & TWI2_SDA_PIN); i++) {
		TWI2_PIO->PIO_CODR = TWI2_SCL_PIN;
		delay_us(5);
		TWI2_PIO->PIO_SODR = TWI2_SCL_PIN;
		delay_us(5);
	}

	TWI2_PIO->PIO_OER = TWI2_SDA_PIN;
	TWI2_PIO->PIO_CODR = TWI2_SDA_PIN;
	delay_us(5);
	TWI2_PIO->PIO_SODR = TWI2_SCL_PIN;
	delay_us(5);
	TWI2_PIO->PIO_SODR = TWI2_SDA_PIN;
	delay_us(5);
	TWI2_PIO->PIO_ODR = TWI2_SDA_PIN | TWI2_SCL_PIN;
	recovered = (TWI2_PIO->PIO_PDSR & (TWI2_SDA_PIN | TWI2_SCL_PIN)) ==
		(TWI2_SDA_PIN | TWI2_SCL_PIN);
	TWI2_PIO->PIO_PDR = TWI2_SDA_PIN | TWI2_SCL_PIN;

	i2c_m_sync_disable(&ADC_I2C);
	i2c_m_sync_enable(&ADC_I2C);
	(void)hri_twihs_read_SR_reg(TWIHS2);
	NVIC_EnableIRQ(TWIHS2_IRQn);
	return recovered;
}
