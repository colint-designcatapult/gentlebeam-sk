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
#include <string.h>
#include "faults.h"
#include "system_parameters.h"
#include "ext_adcs.h"

volatile bool adc_write_cycle = true;
volatile uint32_t adc_addr = ION_R_ADC_ADDR;
volatile uint32_t adc_ch = 0;
volatile uint32_t adc_rx_idx = 0;
volatile uint32_t adc_tx_idx = 0;
volatile uint32_t adc_output_idx = 0;

volatile bool adc_bus_stuck = false;
volatile bool adc_check_ready = true;

uint8_t adc_tx_buf[ADC_TX_SIZE+1] = {0};
uint16_t adc_coil_rx_buf[EXT_ADC_COIL_CNT] = {0};
uint16_t adc_coil_output_buf[EXT_ADC_COIL_CNT][ADC_SAMPLE_BUF_SIZE];
uint16_t adc_sys_rx_buf[EXT_ADC_SYS_CNT] = {0};
uint16_t adc_sys_output_buf[EXT_ADC_SYS_CNT][ADC_SAMPLE_BUF_SIZE];
uint16_t adc_ion_r_rx_buf[EXT_ADC_ION_R_CNT] = {0};
uint16_t adc_ion_r_output_buf[EXT_ADC_ION_R_CNT][ADC_SAMPLE_BUF_SIZE];

static struct timer_task VTIMER_ext_adc_check;

static float get_ads7828_voltage(uint16_t *adc_data);
static float get_max11647_voltage(uint16_t *adc_data);

static void adc_tx();
static void adc_rx();
static bool recover_ext_adc_i2c_bus(void);
static void save_adc_rx(uint8_t val);
static uint16_t * get_adc_buffer();
static void adc_transmission_complete();
static void go_to_next_adc_ch();
static uint32_t get_max_adc_ch(uint32_t address);
static uint32_t get_next_adc_addr(uint32_t address);
static void set_adc_command();

static uint8_t get_ads7828_ch();

static void ext_adc_comm_timeout_check(const struct timer_task *const timer_task);

void init_ext_adcs()
{
	//Initialize I2C peripheral and disable interrupt temporarily
	i2c_m_sync_enable(&ADC_I2C);
	NVIC_DisableIRQ(TWIHS2_IRQn);
	
	//Use synchronous driver while interrupts are disabled
	//Try to setup ion repeller ADC
	struct io_descriptor *ADC_I2C_io;
	i2c_m_sync_get_io_descriptor(&ADC_I2C, &ADC_I2C_io);
	i2c_m_sync_set_slaveaddr(&ADC_I2C, ION_R_ADC_ADDR, I2C_M_SEVEN);
	adc_tx_buf[0] = MAX11647_SETUP_BYTE;
	uint32_t adc_setup_retries = 0;
	do
	{
		if(io_write(ADC_I2C_io, adc_tx_buf, 1) == 1)
		{
			break;
		}
		adc_setup_retries++;
	} while (adc_setup_retries < MAX_ADC_SETUP_RETRIES);
	
	if(adc_setup_retries >= MAX_ADC_SETUP_RETRIES)
	{
		if (recover_ext_adc_i2c_bus() == false) {
			report_typed_fault3(FAULT_ADC_BUS, "ADC setup failed at address %u after %u retries (transfer size: %u bytes).", MAKE_ARG(ION_R_ADC_ADDR), MAKE_ARG(MAX_ADC_SETUP_RETRIES), MAKE_ARG(1));
		}
	}
	
	//Clear interrupt statuses before enabling them for async functionality
	uint32_t sr = hri_twihs_read_SR_reg(ADC_I2C.device.hw) & hri_twihs_read_IMR_reg(ADC_I2C.device.hw);
	
	//Dummy code to ensure compiler does not optimize out SR read
	if(!(sr & TWIHS_SR_NACK))
	{
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
	}
	
	//Allows interrupt on NACK
	hri_twihs_set_IMR_NACK_bit(ADC_I2C.device.hw);
	
	//Enable interrupt
	NVIC_EnableIRQ(TWIHS2_IRQn);
	
	//Initialize vtimer task to check external adc bus
	VTIMER_ext_adc_check.interval = ADC_COMM_TIMEOUT_MS;
	VTIMER_ext_adc_check.cb = ext_adc_comm_timeout_check;
	VTIMER_ext_adc_check.mode = TIMER_TASK_REPEAT;
	// timer_add_task(&VTIMER, &VTIMER_ext_adc_check);
	
	//Start up ADC readings
	adc_ch = 100;	//arbitrarily large adc ch to force device change
	adc_addr = ION_R_ADC_ADDR;
	go_to_next_adc_ch();
	adc_check_ready = false;
}

//Callback function, keep short
static void ext_adc_comm_timeout_check(const struct timer_task *const timer_task)
{
	adc_bus_stuck = true;
}

//Function called in main loop, values read/written and checked here
void process_ext_adcs()
{
	//Make sure adc readings are ready to be checked
	if(!adc_check_ready) return;
	
	float adc_voltage;
	
	//Convert raw adc values to voltage values
	//Then report them to the system parameters
	//adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_TEMP]);
	
	adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_F_V]);
	report_ext_adc_f_coil_v(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_F_I]);
	report_ext_adc_f_coil_cur(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_X_V]);
	report_ext_adc_x_coil_v(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_X_I]);
	report_ext_adc_x_coil_cur(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_Y_V]);
	report_ext_adc_y_coil_v(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_coil_output_buf[EXT_ADC_CH_Y_I]);
	report_ext_adc_y_coil_cur(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_12V]);
	report_ext_adc_12_v(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_5V]);
	report_ext_adc_5_v(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_3V3]);
	report_ext_adc_3p3_v(adc_voltage);
	
	//adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_IP_I1]);
	//adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_IP_V]);	
	adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_IP_I2]);
	report_ext_adc_ion_pump(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_CB_THERM]);
	report_ext_adc_cab_temp(adc_voltage);
	
	adc_voltage = get_ads7828_voltage(adc_sys_output_buf[EXT_ADC_CH_HS_THERM]);
	report_ext_adc_hs_temp(adc_voltage);
	
	adc_voltage = get_max11647_voltage(adc_ion_r_output_buf[EXT_ADC_CH_REPELLER_V]);
	report_ext_adc_ion_rep_v(adc_voltage);
	
	adc_voltage = get_max11647_voltage(adc_ion_r_output_buf[EXT_ADC_CH_REPELLER_I]);
	report_ext_adc_ion_rep_cur(adc_voltage);
	
	adc_check_ready = false;
}

static float get_max11647_voltage(uint16_t *adc_data)
{
	float return_val = 0;
	uint32_t adc_sum = 0;
	
	for(int i = 0; i < ADC_SAMPLE_BUF_SIZE; i++)
	{
		//Use only bottom 10 bits
		adc_sum += (adc_data[i] & 0x3FF);
	}
	
	return_val = (float)adc_sum;
	return_val /= (MAX11647_ADC_SCALING*ADC_SAMPLE_BUF_SIZE);
	
	return return_val;
}

static float get_ads7828_voltage(uint16_t *adc_data)
{
	float return_val = 0;
	uint32_t adc_sum = 0;
	
	for(int i = 0; i < ADC_SAMPLE_BUF_SIZE; i++)
	{
		adc_sum += adc_data[i];
	}
	
	return_val = (float)adc_sum;
	return_val /= (ADS7828_ADC_SCALING*ADC_SAMPLE_BUF_SIZE);
	
	return return_val;
}

//This function called within interrupt, keep short
static void adc_tx()
{
	//If bytes are still left to transmit, send next byte out
	if(adc_tx_idx < ADC_TX_SIZE)
	{
		hri_twihs_write_THR_reg(ADC_I2C.device.hw, adc_tx_buf[adc_tx_idx]);

		adc_tx_idx++;
	}
	//Otherwise begin end of transmission
	else
	{
		adc_tx_idx = 0;
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_TXRDY);
		hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_TXCOMP);
		hri_twihs_write_CR_reg(ADC_I2C.device.hw, TWIHS_CR_STOP);
	}
}

//This function called within interrupt, keep short
static void adc_rx()
{
	//Read and save value
	uint32_t read_val = hri_twihs_read_RHR_reg(ADC_I2C.device.hw);
	save_adc_rx((uint8_t)read_val);
	
	//Increment idx
	adc_rx_idx++;
	
	//If next byte is last, set stop condition
	if(adc_rx_idx == ADC_RX_SIZE-1)
	{
		hri_twihs_write_CR_reg(ADC_I2C.device.hw, TWIHS_CR_STOP);
	}
	//If byte is last, begin transmission end
	else if(adc_rx_idx >= ADC_RX_SIZE)
	{
		adc_rx_idx = 0;
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_RXRDY);
		hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_TXCOMP);
	}
}

//This function called within interrupt, keep short
static void save_adc_rx(uint8_t val)
{
	//Bounds check
	if(adc_rx_idx >= ADC_RX_SIZE)
	{
		return;
	}
	
	uint16_t or_val = (uint16_t)val;
	
	//Write to buffer depending on which ADC and channel is being read
	uint16_t *rx_val = get_adc_buffer();
	if(rx_val == NULL)
	{
		return;
	}
	
	//If it's the first byte received, shift 1 byte up and overwrite
	if(adc_rx_idx == 0)
	{
		or_val <<= 8;
		*rx_val = or_val;
	}
	//Otherwise simply OR the value
	else
	{
		*rx_val |= or_val;
	}
}

static uint16_t * get_adc_buffer()
{
	uint16_t *ret_ptr = NULL;
	
	//Return the appropriate buffer locations
	//As long as channel value is also OK
	if(adc_addr == COIL_ADC_ADDR && adc_ch < EXT_ADC_COIL_CNT)
	{
		ret_ptr = adc_coil_rx_buf;
		ret_ptr += adc_ch;
	}
	else if(adc_addr == SYS_ADC_ADDR && adc_ch < EXT_ADC_SYS_CNT)
	{
		ret_ptr = adc_sys_rx_buf;
		ret_ptr += adc_ch;
	}
	else if(adc_addr == ION_R_ADC_ADDR && adc_ch < EXT_ADC_ION_R_CNT)
	{
		ret_ptr = adc_ion_r_rx_buf;
		ret_ptr += adc_ch;
	}
	
	return ret_ptr;
}

//This function called within interrupt, keep short
static void adc_transmission_complete()
{
	//If we are done with a write, proceed to read
	if(adc_write_cycle)
	{
		hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_RXRDY);
		hri_twihs_write_MMR_reg(ADC_I2C.device.hw, TWIHS_MMR_DADR(adc_addr) | TWIHS_MMR_MREAD);
		hri_twihs_write_CR_reg(ADC_I2C.device.hw, TWIHS_CR_START);
		adc_write_cycle = false;
	}
	//Otherwise if we just read, proceed to next adc channel
	else
	{
		go_to_next_adc_ch();
	}
}

//This function called within interrupt, keep short
static void go_to_next_adc_ch()
{
	//Reset indexes
	adc_rx_idx = 0;
	adc_tx_idx = 0;
	adc_write_cycle = true;
	
	//Increment channel index
	adc_ch++;
	
	//Check to see if we need to change to next device
	if(adc_ch >= get_max_adc_ch(adc_addr))
	{
		adc_ch = 0;
		adc_addr = get_next_adc_addr(adc_addr);
	}
	
	//Set channel command based on channel and device
	set_adc_command();
	
	//Start next write
	hri_twihs_write_MMR_reg(ADC_I2C.device.hw, TWIHS_MMR_DADR(adc_addr));
	hri_twihs_set_IMR_reg(ADC_I2C.device.hw, TWIHS_IER_TXRDY);
}

//This function called within interrupt, keep short
static uint32_t get_max_adc_ch(uint32_t address)
{
	uint32_t return_val = 0;
	switch(address)
	{
		case COIL_ADC_ADDR:
		return_val = EXT_ADC_COIL_CNT;
		break;
		
		case SYS_ADC_ADDR:
		return_val = EXT_ADC_SYS_CNT;
		break;
		
		case ION_R_ADC_ADDR:
		return_val = EXT_ADC_ION_R_CNT;
		break;
		
		default:
		break;
	}
	return return_val;
}

//This function called within interrupt, keep short
static uint32_t get_next_adc_addr(uint32_t address)
{
	uint32_t return_val = COIL_ADC_ADDR;
	switch(address)
	{
		case COIL_ADC_ADDR:
		return_val = SYS_ADC_ADDR;
		break;
		
		case SYS_ADC_ADDR:
		return_val = ION_R_ADC_ADDR;
		break;
		
		case ION_R_ADC_ADDR:
		return_val = COIL_ADC_ADDR;
		
		//Only update if adc values are not being processed
		//This prevents race conditions
		//A sample may be lost (should occur very infrequently)
		if(!adc_check_ready)
		{
			if(++adc_output_idx >= ADC_SAMPLE_BUF_SIZE)
			{
				adc_output_idx = 0;
			}
			//Copy output for processing
			for(int i = 0; i < EXT_ADC_COIL_CNT; i++)
			{
				adc_coil_output_buf[i][adc_output_idx] = adc_coil_rx_buf[i];
			}
			for(int i = 0; i < EXT_ADC_SYS_CNT; i++)
			{
				adc_sys_output_buf[i][adc_output_idx] = adc_sys_rx_buf[i];
			}
			for(int i = 0; i < EXT_ADC_ION_R_CNT; i++)
			{
				adc_ion_r_output_buf[i][adc_output_idx] = adc_ion_r_rx_buf[i];
			}
			
			adc_check_ready = true;
		}
		break;
		
		default:
		break;
	}
	return return_val;
}

//This function called within interrupt, keep short
static void set_adc_command()
{
	if(adc_addr == COIL_ADC_ADDR || adc_addr == SYS_ADC_ADDR)
	{
		adc_tx_buf[0] = ADS7828_CMD_BYTE;
		adc_tx_buf[0] |= get_ads7828_ch();
	}
	else
	{
		adc_tx_buf[0] = MAX11647_CONFIG_BYTE | MAX11647_CH(adc_ch);
	}
}

static uint8_t get_ads7828_ch()
{
	uint8_t ch_transform = 0;
	if(adc_ch & 0x01)
	{
		ch_transform = ((adc_ch+7)/2);
	}
	else
	{
		ch_transform = adc_ch/2;
	}
	return ADS7828_CH(ch_transform);
}

//Interrupt handler, keep short
void TWIHS2_Handler()
{
	uint32_t sr = hri_twihs_read_SR_reg(ADC_I2C.device.hw) & hri_twihs_read_IMR_reg(ADC_I2C.device.hw);
	
	//Check for NACK error
	if(sr & TWIHS_SR_NACK)
	{
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		
		//Report fault
		report_typed_fault2(FAULT_ADC_BUS, "ADC at address %u returned NACK (transfer size: %u bytes).", MAKE_ARG(adc_addr), MAKE_ARG(1));
		
		//Move to next ADC
		adc_ch = 100;	//Force ADC device change with arbitrary large channel
		go_to_next_adc_ch();
	}
	//Check for transmission completion
	else if (sr & TWIHS_SR_TXCOMP)
	{
		hri_twihs_clear_IMR_reg(ADC_I2C.device.hw, TWIHS_IDR_TXRDY | TWIHS_IDR_TXCOMP | TWIHS_IDR_RXRDY);
		adc_transmission_complete();
	}
	//Check for TX transmit ready
	else if(sr & TWIHS_SR_TXRDY)
	{
		adc_tx();
	}
	//Check for RX received
	else if(sr & TWIHS_SR_RXRDY)
	{
		adc_rx();
	}
}

#define I2C_RECOVERY_PULSES    9

#define TWI2_SDA_PIN          PIO_PD27
#define TWI2_SCL_PIN          PIO_PD28
#define TWI2_PIO              PIOD

static bool recover_ext_adc_i2c_bus(void)
{
    bool recovered = false;

    /* Disable TWIHS2 interrupt */
    NVIC_DisableIRQ(TWIHS2_IRQn);

    /* Disable TWIHS2 master */
    hri_twihs_write_CR_reg(TWIHS2, TWIHS_CR_MSDIS);

    /* Give PD27/PD28 back to the PIO controller */
    TWI2_PIO->PIO_PER = TWI2_SDA_PIN | TWI2_SCL_PIN;

    /* Enable outputs and drive both high */
    TWI2_PIO->PIO_OER = TWI2_SDA_PIN | TWI2_SCL_PIN;
    TWI2_PIO->PIO_SODR = TWI2_SDA_PIN | TWI2_SCL_PIN;

    delay_us(10);

    /* Release SDA so we can monitor it */
    TWI2_PIO->PIO_ODR = TWI2_SDA_PIN;

    /*
     * If SDA is being held low by a slave,
     * toggle SCL up to 9 times.
     */
    if (!(TWI2_PIO->PIO_PDSR & TWI2_SDA_PIN))
    {
        for (uint32_t i = 0; i < I2C_RECOVERY_PULSES; i++)
        {
            TWI2_PIO->PIO_CODR = TWI2_SCL_PIN;
            delay_us(5);

            TWI2_PIO->PIO_SODR = TWI2_SCL_PIN;
            delay_us(5);

            if (TWI2_PIO->PIO_PDSR & TWI2_SDA_PIN)
            {
                break;
            }
        }
    }

    /*
     * Generate a STOP:
     * SDA low -> SCL high -> SDA high
     */
    TWI2_PIO->PIO_OER = TWI2_SDA_PIN;

    TWI2_PIO->PIO_CODR = TWI2_SDA_PIN;
    delay_us(5);

    TWI2_PIO->PIO_SODR = TWI2_SCL_PIN;
    delay_us(5);

    TWI2_PIO->PIO_SODR = TWI2_SDA_PIN;
    delay_us(5);

    /* Release both lines */
    TWI2_PIO->PIO_ODR = TWI2_SDA_PIN | TWI2_SCL_PIN;

    delay_us(5);

    /* Verify idle bus */
    recovered =
        ((TWI2_PIO->PIO_PDSR & TWI2_SDA_PIN) != 0U) &&
        ((TWI2_PIO->PIO_PDSR & TWI2_SCL_PIN) != 0U);

    /*
     * Restore peripheral control.
     *
     * Check your SAME70 pinmux: TWD2/TWCK2 are typically
     * Peripheral C on PD27/PD28.
     */
    TWI2_PIO->PIO_PDR = TWI2_SDA_PIN | TWI2_SCL_PIN;

    /* Reinitialize TWIHS2 */
    i2c_m_sync_disable(&ADC_I2C);
    i2c_m_sync_enable(&ADC_I2C);

    /* Clear stale status */
    volatile uint32_t sr = hri_twihs_read_SR_reg(TWIHS2);
    (void)sr;

    NVIC_EnableIRQ(TWIHS2_IRQn);

    return recovered;
}