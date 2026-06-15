#include <atmel_start.h>
#include <peripheral_clk_config.h>
#include <lwip/netif.h>
#include <lwip/timers.h>
#include <string.h>
#include "sensus_src/custom_eth_ipstack_main.h"
#include "sensus_src/ext_adcs.h"
#include "sensus_src/ext_dac.h"
#include "sensus_src/ext_timers.h"
#include "sensus_src/faults.h"
#include "sensus_src/ftdi.h"
#include "sensus_src/head_board.h"
#include "sensus_src/hvps.h"
#include "sensus_src/pc_comm_parser.h"
#include "sensus_src/pc_msg_processing.h"
#include "sensus_src/peltier_cooler.h"
#include "sensus_src/state_machine.h"
#include "sensus_src/system_monitoring.h"
#include "sensus_src/system_parameters.h"

#if !defined (CALIBRATION_MODE)
#include "sensus_src/qc_well.h"
#endif

//#include "system_test.h"

#include "examples/driver_examples.h"


volatile static u32_t systick_timems;
volatile static bool  recv_flag = false;
static bool           link_up   = false;
u8_t    mac[6];

static void finalize_setup();
static inline void process_peripherals();


u32_t sys_now(void)
{
	return systick_timems;
}

void SysTick_Handler(void)
{
	systick_timems++;
}

void mac_receive_cb(struct mac_async_descriptor *desc)
{
	recv_flag = true;
}

static inline void process_peripherals()
{
	process_ext_adcs();
	process_ext_dac();
	process_ext_timers();
	process_hb();
	process_plt();
#if !defined(CALIBRATION_MODE)
	//process_qc();			//No QC well in 
#endif
	process_hvps();	
}

static void finalize_setup()
{	
	//Set up interlock HW debouncing
	uint32_t interlock_mask = 0;
	interlock_mask |= (1<<IBP_DOOR_CLOSED);
	interlock_mask |= (1<<IBP_DRIVE_SYS);
	interlock_mask |= (1<<IBP_BASE_ESTOP);
	interlock_mask |= (1<<IBP_REMOTE_ESTOP);
	interlock_mask |= (1<<IBP_KUKA_FAULT_1);
	interlock_mask |= (1<<IBP_KUKA_FAULT_2);	
	interlock_mask |= (1<<IBP_WATER_LEVEL);
	interlock_mask |= (1<<IBP_ION_PUMP_ON);
	interlock_mask |= (1<<IBP_TIMER_FAULT_1);
	interlock_mask |= (1<<IBP_TIMER_FAULT_2);
	interlock_mask |= (1<<IBP_HVPS_FAULT);
	interlock_mask |= (1<<IBP_COOLER_FAULT);
	interlock_mask |= (1<<IBP_HEADBOARD_FAULT);
	interlock_mask |= (1<<IBP_WD_FAULT);
	interlock_mask |= (1<<IBP_REMOTE_KEY);
	interlock_mask |= (1<<IBP_COLLIMATOR_ON);
	
	//Enable debouncing with MCU hardware peripheral (no SW debounce needed)
	((Pio *)PIOC)->PIO_SCDR = 100;
	((Pio *)PIOC)->PIO_IFSCER = interlock_mask;
	((Pio *)PIOC)->PIO_IFER = interlock_mask;
	
	//Ensure cooling system is disabled
	gpio_set_pin_level(IO_PUMP_EN, false);
	
	//Ensure external indicators are disabled
	gpio_set_pin_level(IO_INDICATORS_EN, false);
	
	//Ensure fans are enabled
	gpio_set_pin_level(IO_HS_FAN_EN, true);
	gpio_set_pin_level(IO_CB_FAN_EN, true);
	
	//Ensure ion pump is enabled
	gpio_set_pin_level(IO_ION_PUMP_EN, true);	
	
	//Ensure ion repeller is enabled
	gpio_set_pin_level(IO_ION_REPELLER_EN, true);
	
	set_led_sequence(LED_SEQ_COLD);
	
	/* GPIO on PD21 for new LED board comm*/
	gpio_set_pin_level(GPIO(GPIO_PORTD, 21), false);

	// Set pin direction to output
	gpio_set_pin_direction(GPIO(GPIO_PORTD, 21), GPIO_DIRECTION_OUT);

	gpio_set_pin_function(GPIO(GPIO_PORTD, 21), GPIO_PIN_FUNCTION_OFF);
	
	/* GPIO on PD22 for new LED board comm*/
	gpio_set_pin_level(GPIO(GPIO_PORTD, 22), false);

	// Set pin direction to output
	gpio_set_pin_direction(GPIO(GPIO_PORTD, 22), GPIO_DIRECTION_OUT);

	gpio_set_pin_function(GPIO(GPIO_PORTD, 22), GPIO_PIN_FUNCTION_OFF);
	
	/* GPIO on PD23 for new LED board comm*/
	gpio_set_pin_level(GPIO(GPIO_PORTD, 23), false);

	// Set pin direction to output
	gpio_set_pin_direction(GPIO(GPIO_PORTD, 23), GPIO_DIRECTION_OUT);

	gpio_set_pin_function(GPIO(GPIO_PORTD, 23), GPIO_PIN_FUNCTION_OFF);
}


int main(void)
{
	/* Initializes MCU, drivers and middleware */
	atmel_start_init();
	
	timer_start(&VTIMER);
	
	//Enable systick
	systick_timems = 0;
	SysTick_Config((CONF_CPU_FREQUENCY) / 1000);
	
	//Network settings
	mac[0] = 0x02;
	mac[1] = 0x00;
	mac[2] = 0x00;
	mac[3] = 0x00;
	mac[4] = 0x00;
	mac[5] = 0x00;
	
	//Initial system setup
	init_system_parameters();
		
	//Turn off remote indicator
	gpio_set_pin_level(IO_LED6, false);  //change back to false and LED6

	
	//Set up LWIP
	mac_async_register_callback(&MACIF, MAC_ASYNC_RECEIVE_CB, (FUNC_PTR)mac_receive_cb);
	eth_ipstack_init();
	
	//Set up communication to PC
	pc_comm_init();
	
	//Set up ethernet
	int32_t ret;
	do {
		ret = ethernet_phy_get_link_status(&MACIF_PHY_desc, &link_up);
		if (ret == ERR_NONE && link_up) {
			break;
		}
	} while (true);
	
	//Use custom MACIF functionality to set custom IP address
	my_LWIP_MACIF_init(mac);
	netif_set_up(&my_LWIP_MACIF_desc);
	netif_set_default(&my_LWIP_MACIF_desc);
	mac_async_enable(&MACIF);
	
	//Set up remaining peripherals
	init_faults();
	init_ext_adcs();
	init_ext_dac();	
	init_ext_timers();
	init_ftdi();
	init_head_board();
	init_plt_cooler();
	init_hvps();
	init_system_monitoring();
	init_state_machine();
	
	//Finish setup with misc IO and HVPS test
	finalize_setup();
	
	//Synchronous loop
	while (1) {		
		
		if (recv_flag) {
			recv_flag = false;
			ethernetif_mac_input(&my_LWIP_MACIF_desc);
		}
		sys_check_timeouts();
		
		//Get any USB requests
		process_ftdi();
		
		//Get any PC requests
		process_pc_comm();
		
		//Update peripherals
		process_peripherals();
		
		//Check system monitoring
		process_system_monitoring();
		
		//Check faults
		process_faults();
		
		//Run state machine
#if !defined(CALIBRATION_MODE)
		process_state_machine();
#endif
		
		//Send PC response
		send_pc_response();
	}
}
