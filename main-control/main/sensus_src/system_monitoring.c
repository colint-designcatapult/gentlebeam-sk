/*
*	Empyrean Medical Systems
*	08/2018
*	Project: Gryphon System Control Firmware
*	Module: System monitoring
*	Author: Carlton Chow
*	Description:
*/

#include <atmel_start.h>
#include "ext_dac.h"
#include "faults.h"
#include "hvps.h"
#include "hvps_monitoring.h"
#include "state_machine.h"
#include "system_parameters.h"
#include "system_monitoring.h"
#include "sys_config_defaults.h"

static struct timer_task VTIMER_param_check_task;

volatile bool system_param_check = false;

static void check_pc_comm_timeout();
static void check_system_power();
static void check_interlocks();
static void check_ion_pump_values();
static void check_ion_repeller_values();

static void check_cooling_system_values();
static void update_cabinet_fan();
static void update_heatsink_fan();
static void update_pump();
static void check_coolant_pressure();
#if defined(CALIBRATION_MODE)
static void check_coolant_temp();
static void check_heatsink_temp();
static void check_cabinet_temp();
#else
static void check_coolant_temp();
#endif
static void check_coolant_flow();

static void check_coils();
static void check_x_coil();
static void check_y_coil();
static void check_f_coil();

static void set_sys_param_check(const struct timer_task *const timer_task);

int32_t system_monitoring[SMON_COUNT];
float expected_coil_value[EV_COUNT];
uint32_t pc_timeout_count = 0;
volatile bool pump_on = false;
uint32_t coil_err_count[3] = {0, 0, 0};


void init_system_monitoring()
{	
	//Start periodic interval at which system is monitored
	VTIMER_param_check_task.interval = SYSTEM_MONITOR_INTERVAL;
	VTIMER_param_check_task.cb = set_sys_param_check;
	VTIMER_param_check_task.mode = TIMER_TASK_REPEAT;
	timer_add_task(&VTIMER, &VTIMER_param_check_task);
	
	system_param_check = false;
	
	//Reset external watchdog
	gpio_toggle_pin_level(IO_EXT_WD_RST);
	
	system_monitoring[SMON_LAST_HS_FAN_STATE] = -1;
	system_monitoring[SMON_LAST_CB_FAN_STATE] = -1;
	system_monitoring[SMON_ION_R_A_OOT_COUNTER] = -1;
	system_monitoring[SMON_ION_R_V_OOT_COUNTER] = -1;
	system_monitoring[SMON_3V3_OOT_COUNTER] = -1;
	system_monitoring[SMON_5V_OOT_COUNTER] = -1;
	system_monitoring[SMON_12V_OOT_COUNTER] = -1;
	system_monitoring[SMON_ION_P_HI_COUNTER] = -1;
	system_monitoring[SMON_CLNT_P_HI_COUNTER] = -1;
	system_monitoring[SMON_CLNT_P_LO_COUNTER] = -1;
	system_monitoring[SMON_CLNT_F_HI_COUNTER] = -1;
	system_monitoring[SMON_CLNT_F_LO_COUNTER] = -1;
}

void set_sys_param_check(const struct timer_task *const timer_task)
{
	system_param_check = true;
}

void process_system_monitoring()
{	
	if(!system_param_check) return;
	system_param_check = false;
	
	//Always check interlocks
	check_interlocks();
	
	switch(system_monitoring[SMON_ROUND])
	{
		case SYS_MON_ROUND_0:
			check_system_power();
			check_pc_comm_timeout();
			//Reset external watchdog
			gpio_toggle_pin_level(IO_EXT_WD_RST);
			break;
		case SYS_MON_ROUND_1:
			check_cooling_system_values();
			break;
		case SYS_MON_ROUND_2:
			check_ion_pump_values();
			check_ion_repeller_values();
			break;
		case SYS_MON_ROUND_3:
			check_hvps_values();
			break;
		case SYS_MON_ROUND_4:
			check_coils();
			break;
		default:
			system_monitoring[SMON_ROUND] = SYS_MON_ROUND_0;
			break;
	}
	system_monitoring[SMON_ROUND]++;
	if(system_monitoring[SMON_ROUND] >= SYS_MON_NUM_ROUNDS)
	{
		system_monitoring[SMON_ROUND] = SYS_MON_ROUND_0;
	}		
}

void reset_pc_comm_timeout()
{
	pc_timeout_count = 0;
}

static void check_pc_comm_timeout()
{
	pc_timeout_count++;
	if(pc_timeout_count > PC_COMM_TIMEOUT_COUNT)
	{
		//Ignore PC comm timeout in startup state
		if(system_status[SS_STATE].i != STATE_STARTUP)
		{
			report_simple_fault(FAULT_PC_COMM_TIMEOUT, PC_COMM_TIMEOUT_COUNT, 0, -1);
		}
	}
}


static void check_system_power()
{	
	float voltage = internal_voltages[INTERNAL_V_3_3];
	
	//Check 3.3V supply
	if(!tolerance_check_rel(3.3, voltage, 10))
	{
		//TBD TODO magic numbers
		if(++system_monitoring[SMON_3V3_OOT_COUNTER] > 10)
		{
			system_monitoring[SMON_3V3_OOT_COUNTER] = 0;
			report_simple_fault(FAULT_BOARD_VOLTAGE, 3.3, 10, voltage);	
		}
	}
	else
	{
		system_monitoring[SMON_3V3_OOT_COUNTER] = 0;		
	}
	
	//Check 5V supply
	voltage = internal_voltages[INTERNAL_V_5];
	if(!tolerance_check_rel(5.0, voltage, 10))
	{
		//TBD TODO magic numbers
		if(++system_monitoring[SMON_5V_OOT_COUNTER] > 10)
		{
			system_monitoring[SMON_5V_OOT_COUNTER] = 0;
			report_simple_fault(FAULT_BOARD_VOLTAGE, 5.0, 10, voltage);	
		}
	}
	else
	{
		system_monitoring[SMON_5V_OOT_COUNTER] = 0;
	}
	
	//Check 12V supply
	voltage = internal_voltages[INTERNAL_V_12];
	if(!tolerance_check_rel(12.0, voltage, 10))
	{
		//TBD TODO magic numbers
		if(++system_monitoring[SMON_12V_OOT_COUNTER] > 10)
		{
			system_monitoring[SMON_12V_OOT_COUNTER] = 0;
			report_simple_fault(FAULT_BOARD_VOLTAGE, 12.0, 10, voltage);
		}
	}
	else
	{
		system_monitoring[SMON_12V_OOT_COUNTER] = 0;
	}
}

static void check_interlocks()
{
	bool stop_check = false;
#if defined(CALIBRATION_MODE)
	volatile uint32_t interlock_mask = 0b11000011111111111111;	
	//skip (right to left): MCU FAULT, SPARE_INTERLOCK, BUF_MASTER_FAULT, NA
#else
	volatile uint32_t interlock_mask = 0b11000011111111001101;	//skip (right to left): MCU FAULT, SPARE_INTERLOCK, NA
#endif

	//Get interlock input states
	uint32_t interlock_status = gpio_get_port_level(GPIO_PORTC);	

	interlock_status &= interlock_mask;
	
	//Save interlock status
	system_status[SS_INTERLOCKS].i = interlock_status;
	
	//Set remote stop led if remote estop is pressed
	gpio_set_pin_level(IO_REMOTE_LED_1, interlock_status & (1<<IBP_REMOTE_ESTOP));

#if defined(CALIBRATION_MODE)
	//Report fault if door is open
	fault_detected(DOOR_FAULT, !(interlock_status & (1<<IBP_DOOR_CLOSED)));
#endif
	
	//Based on the state, remove specific interlocks from creating a fault condition
	switch(system_status[SS_STATE].i)
	{
		case STATE_STARTUP:
			//In startup ignore all interlock faults
			interlock_mask = 0;
			break;
		case STATE_COLD:
		case STATE_COLD_FAULT:
			interlock_mask &= ~(1<<IBP_DOOR_CLOSED);
#if defined(CALIBRATION_MODE)
			interlock_mask &= ~(1<<IBP_DRIVE_SYS);
#endif
			interlock_mask &= ~(1<<IBP_REMOTE_KEY);
			interlock_mask &= ~(1<<IBP_COLLIMATOR_ON);
			break;
		case STATE_CONDITIONING:
		case STATE_WARMUP:
			break;
		case STATE_WARMUP_FAULT:	
		case STATE_PRIMED:
		case STATE_STAGING:
		case STATE_STAGED:
			interlock_mask &= ~(1<<IBP_DOOR_CLOSED);
#if defined(CALIBRATION_MODE)
			interlock_mask &= ~(1<<IBP_DRIVE_SYS);
#endif
			interlock_mask &= ~(1<<IBP_REMOTE_KEY);
			interlock_mask &= ~(1<<IBP_COLLIMATOR_ON);
			break;
		case STATE_HVPS_CHECK:
		case STATE_SETUP:
		case STATE_READY:
		case STATE_LAUNCHING:
		case STATE_EMISSION:
			break;
		case STATE_TERMINATION:
		case STATE_DISCHARGE:
		case STATE_FAULT:
			interlock_mask &= ~(1<<IBP_DOOR_CLOSED);
#if defined(CALIBRATION_MODE)
			interlock_mask &= ~(1<<IBP_DRIVE_SYS);
#endif
			interlock_mask &= ~(1<<IBP_REMOTE_KEY);
			interlock_mask &= ~(1<<IBP_COLLIMATOR_ON);
			break;
		case STATE_SYSTEM_CRASH:
		case STATE_UNKNOWN:
			break;
		//For all other delivery states, all interlocks should result in a fault
		default:
			break;
	}
	
	//Throw a fault if interlocks are not valid
	if((interlock_status & interlock_mask) != interlock_mask)
	{
		report_simple_fault(FAULT_INTERLOCK, interlock_mask, 0, interlock_status);
	}
}

static void check_ion_pump_values()
{
	if(system_status[SS_IONPUMP_PRESSURE].f > DEFAULT_ION_P_HI_TH)
	{
		if(++system_monitoring[SMON_ION_P_HI_COUNTER] > 10)
		{
			system_monitoring[SMON_ION_P_HI_COUNTER] = 0;
			report_simple_fault(FAULT_ION_PUMP_FB, DEFAULT_ION_P_HI_TH, 0, system_status[SS_IONPUMP_PRESSURE].f);
#if defined(CALIBRATION_MODE)
			// Stop emission
			fault_detected(IPUM_FAULT, true);
#endif
		}
	}
	else
	{
		system_monitoring[SMON_ION_P_HI_COUNTER] = 0;
#if defined(CALIBRATION_MODE)
		fault_detected(IPUM_FAULT, false);
#endif
	}
}

static void check_ion_repeller_values()
{	
	if(!tolerance_check_rel(REPELLER_TARGET, internal_voltages[INTERNAL_V_ION_REP], 10)){
		if(++system_monitoring[SMON_ION_R_V_OOT_COUNTER] > 10)
		{
			system_monitoring[SMON_ION_R_V_OOT_COUNTER] = 0;
			report_fault(FAULT_ION_REPELLER, ION_REP_FAULT_OOT, REPELLER_TARGET, 10, internal_voltages[INTERNAL_V_ION_REP]);
#if defined(CALIBRATION_MODE)
			// Stop emission
			fault_detected(IREP_FAULT, true);
#endif
		}
	}
	else
	{
		system_monitoring[SMON_ION_R_V_OOT_COUNTER] = 0;
#if defined(CALIBRATION_MODE)
		fault_detected(IREP_FAULT, false);
#endif
	}
	
	/*
	if(internal_voltages[INTERNAL_V_ION_REP_CUR] >= MAX_REPELLER_CURRENT_VAL)
	{
		if(++system_monitoring[SMON_ION_R_A_OOT_COUNTER] > 10)
		{
			system_monitoring[SMON_ION_R_A_OOT_COUNTER] = 0;
			report_fault(FAULT_ION_REPELLER, ION_REP_FAULT_OVERCURRENT, MAX_REPELLER_CURRENT_VAL, 0, internal_voltages[INTERNAL_V_ION_REP_CUR]);
		}
	}
	*/
}

static void check_cooling_system_values()
{
	check_coolant_pressure();
	check_coolant_flow();
	check_coolant_temp();
#if defined(CALIBRATION_MODE)
	check_heatsink_temp();
	check_cabinet_temp();
#endif
	
	update_cabinet_fan();
	update_heatsink_fan();
	//update_pump();
}

static void check_coolant_pressure()
{
	//Check for overpressure fault
	if(pump_on)
	{
		if(system_status[SS_WATER_PRESSURE].f > DEFAULT_WTR_P_HI_ERR)
		{
			if(++system_monitoring[SMON_CLNT_P_HI_COUNTER] > 200)
			{
				system_monitoring[SMON_CLNT_P_HI_COUNTER] = 0;
				report_fault(FAULT_COOLANT, COOLANT_FAULT_OVERPRESSURE, DEFAULT_WTR_P_HI_ERR, 0, system_status[SS_WATER_PRESSURE].f);
			}
		}
		else
		{
			system_monitoring[SMON_CLNT_P_HI_COUNTER] = 0;
		}
		
		if(system_status[SS_WATER_PRESSURE].f < DEFAULT_WTR_P_LO_ERR)
		{
			if(++system_monitoring[SMON_CLNT_P_LO_COUNTER] > 200)
			{
				system_monitoring[SMON_CLNT_P_LO_COUNTER] = 0;
				//TODO: add COOLANT_FAULT_UNDERPRESSURE to details
				report_fault(FAULT_COOLANT, COOLANT_FAULT_OVERPRESSURE, DEFAULT_WTR_P_LO_ERR, 0, system_status[SS_WATER_PRESSURE].f);	
			}	
		}
		else
		{
			system_monitoring[SMON_CLNT_P_LO_COUNTER] = 0;
		}
	}	
}

static void check_coolant_flow()
{
	//Check for flow rate fault
	if(pump_on)
	{
		if(system_status[SS_WATER_FLOW_RATE].f < DEFAULT_WTR_F_LO_ERR)
		{
			if(++system_monitoring[SMON_CLNT_F_LO_COUNTER] > 200)
			{
				system_monitoring[SMON_CLNT_F_LO_COUNTER] = 0;
				report_fault(FAULT_COOLANT, COOLANT_FAULT_LOW_FLOW, DEFAULT_WTR_F_LO_ERR, 0, system_status[SS_WATER_FLOW_RATE].f);
			}
		}
		else
		{
			system_monitoring[SMON_CLNT_F_LO_COUNTER] = 0;
		}
		
		if(system_status[SS_WATER_FLOW_RATE].f > DEFAULT_WTR_F_HI_ERR)
		{
			if(++system_monitoring[SMON_CLNT_F_HI_COUNTER] > 200)
			{
				system_monitoring[SMON_CLNT_F_HI_COUNTER] = 0;
				//TODO: add COOLANT_FAULT_HIGH_FLOW to details
				report_fault(FAULT_COOLANT, COOLANT_FAULT_LOW_FLOW, DEFAULT_WTR_F_HI_ERR, 0, system_status[SS_WATER_FLOW_RATE].f);
			}
		}
		else
		{
			system_monitoring[SMON_CLNT_F_HI_COUNTER] = 0;	
		}
	}
	
}

static void check_coolant_temp()
{
#if defined(CALIBRATION_MODE)
	float coolant_temp = system_status[SS_WATER_TEMP].f;
	
	if(coolant_temp > DEFAULT_WTR_TEMP_ERR)
	{
		report_fault(FAULT_COOLANT, COOLANT_FAULT_OVERTEMP, DEFAULT_WTR_TEMP_ERR, 0, coolant_temp);
	}
}

static void check_heatsink_temp()
{
	float heatsink_temp = system_status[SS_HEATSINK_TEMP].f;

	if(heatsink_temp > DEFAULT_HS_TEMP_ERR)
	{
		report_simple_fault(FAULT_HEATSINK, DEFAULT_HS_TEMP_ERR, 0, heatsink_temp);
	}
}

static void check_cabinet_temp()
{
	float cabinet_temp = system_status[SS_CABINET_TEMP].f;

	if(cabinet_temp > DEFAULT_CAB_TEMP_ERR)
	{
		report_simple_fault(FAULT_HEATSINK, DEFAULT_CAB_TEMP_ERR, 0, cabinet_temp); //TODO: add fault type for cabinet
	}
#else
	//Check for temperature fault
	if(system_status[SS_WATER_TEMP].f > DEFAULT_WTR_TEMP_ERR)
	{
		report_fault(FAULT_COOLANT, COOLANT_FAULT_OVERTEMP, DEFAULT_WTR_TEMP_ERR, 0, system_status[SS_WATER_TEMP].f);
	}
#endif
}

static void update_pump()
{
	
}

static void update_cabinet_fan()
{
#if defined (CALIBRATION_MODE)
	uint32_t state_now = system_status[SS_STATE].i;
	float cabinet_temp = system_status[SS_CABINET_TEMP].f;
	
	//Always force cabinet fan to high while high voltage is present (between ramping up until ramped down)
	if(state_now >= STATE_SETUP && state_now <= STATE_DISCHARGE)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 0)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 4.9); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 0;
		}
		return;
	}
	
	//Set cabinet fan based on current cabinet temperature
	
	if(cabinet_temp >= DEFAULT_CAB_HIGH)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 0)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 4.9); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 0;
		}
	}
	//take into account 2C hysteresis when ramping down from HIGH->MED
	else if(cabinet_temp >= DEFAULT_CAB_HIGH - DEFAULT_CAB_HYS)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 0 && system_monitoring[SMON_LAST_CB_FAN_STATE] != 1)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 4.5); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 1;
		}
	}
	else if(cabinet_temp >= DEFAULT_CAB_MED)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 1)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 4.5); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 1;
		}
	}
	//take into account 2C hysteresis when ramping down from MED->LOW	
	else if(cabinet_temp >= DEFAULT_CAB_MED - DEFAULT_CAB_HYS)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 1 && system_monitoring[SMON_LAST_CB_FAN_STATE] != 2)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 4.1); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 2;
		}
	}
	else if(cabinet_temp >= DEFAULT_CAB_LOW)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 2)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 4.1); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 2;
		}
	}
	//take into account 2C hysteresis when ramping down from LOW->MIN
	else if(cabinet_temp >= DEFAULT_CAB_LOW - DEFAULT_CAB_HYS)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 2 && system_monitoring[SMON_LAST_CB_FAN_STATE] != 3)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 3.7); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 3;
		}
	}
	else
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 3)
		{
			set_fan_voltage(CB_FAN_DAC_CH, 3.7); //TBD TODO magic number
			system_monitoring[SMON_LAST_CB_FAN_STATE] = 3;
		}
	}
#else
	uint32_t state_now = system_status[SS_STATE].i;
	
	//Always turn on cabinet fans after ready
	if(state_now >= STATE_READY)
	{
		system_monitoring[SMON_LAST_CB_FAN_STATE] = 0;
		gpio_set_pin_level(IO_CB_FAN_EN, true);
		set_fan_voltage(CB_FAN_DAC_CH, 4.9); //TBD TODO magic number
	}	
	//Set cabinet fan based on current cabinet temperature
	//In the future can add hysteresis
	else if(system_status[SS_CABINET_TEMP].f >= DEFAULT_CAB_ERR)
	{
		//TBD TODO throw fault maybe
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 0) 
		{
			gpio_set_pin_level(IO_CB_FAN_EN, true);
			set_fan_voltage(CB_FAN_DAC_CH, 4.9); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_CB_FAN_STATE] = 0;
	}
	else if(system_status[SS_CABINET_TEMP].f >= DEFAULT_CAB_FULL)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 1) 
		{
			gpio_set_pin_level(IO_CB_FAN_EN, true);
			set_fan_voltage(CB_FAN_DAC_CH, 4.9); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_CB_FAN_STATE] = 1;
	}
	else if(system_status[SS_CABINET_TEMP].f >= DEFAULT_CAB_MED)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 2) {
			gpio_set_pin_level(IO_CB_FAN_EN, true);
			set_fan_voltage(CB_FAN_DAC_CH, 4.2); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_CB_FAN_STATE] = 2;
	}
	else if(system_status[SS_CABINET_TEMP].f >= DEFAULT_CAB_LOW)
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 3) {
			gpio_set_pin_level(IO_CB_FAN_EN, true);
			set_fan_voltage(CB_FAN_DAC_CH, 3.8); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_CB_FAN_STATE] = 3;
	}
	else
	{
		if(system_monitoring[SMON_LAST_CB_FAN_STATE] != 4) {
			gpio_set_pin_level(IO_CB_FAN_EN, false);
		}
		system_monitoring[SMON_LAST_CB_FAN_STATE] = 4;
	}
#endif
}

static void update_heatsink_fan()
{
#if defined(CALIBRATION_MODE)
	uint32_t state_now = system_status[SS_STATE].i;
	float heatsink_temp = system_status[SS_HEATSINK_TEMP].f;
	
	//Always force heatsink fan to high while high voltage is present (between ramping up until ramped down)
	if(state_now >= STATE_SETUP && state_now <= STATE_DISCHARGE)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 0)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 2.5); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 0;
		}
		return;
	}
	
	//Set heatsink fan based on current heatsink temperature
	
	if(heatsink_temp >= DEFAULT_HS_HIGH)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 0)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 2.5); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 0;
		}
	}
	//take into account 2C hysteresis when ramping down from HIGH->MED
	else if(heatsink_temp >= DEFAULT_HS_HIGH - DEFAULT_HS_HYS)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 0 && system_monitoring[SMON_LAST_HS_FAN_STATE] != 1)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 2.1); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 1;
		}
	}
	else if(heatsink_temp >= DEFAULT_HS_MED)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 1)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 2.1); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 1;
		}
	}
	//take into account 2C hysteresis when ramping down from MED->LOW
	else if(heatsink_temp >= DEFAULT_HS_MED - DEFAULT_HS_HYS)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 1 && system_monitoring[SMON_LAST_HS_FAN_STATE] != 2)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 1.7); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 2;
		}
	}
	else if(heatsink_temp >= DEFAULT_HS_LOW)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 2)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 1.7); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 2;
		}
	}
	//take into account 2C hysteresis when ramping down from LOW->MIN
	else if(heatsink_temp >= DEFAULT_HS_LOW - DEFAULT_HS_HYS)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 2 && system_monitoring[SMON_LAST_HS_FAN_STATE] != 3)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 1.3); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 3;
		}
	}
	else
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 3)
		{
			set_fan_voltage(HS_FAN_DAC_CH, 1.3); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 3;
		}
	}
#else
	uint32_t state_now = system_status[SS_STATE].i;
	
	//Set heatsink fan based on current heatsink temperature
	//In the future can add hysteresis
	if(system_status[SS_HEATSINK_TEMP].f >= HEATSINK_ERR_TH)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 0) 
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 2.5); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_HS_FAN_STATE] = 0;
		//TBD TODO throw fault
		report_simple_fault(FAULT_HEATSINK, HEATSINK_ERR_TH, 0, system_status[SS_HEATSINK_TEMP].f);
	}
	else if(state_now >= STATE_READY)
	{
		//Always force heatsink fan to high while high voltage is on
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 1) 
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 2.5); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_HS_FAN_STATE] = 1;
	}
	//If above threshold + hysteresis, always set fan full
	else if(system_status[SS_HEATSINK_TEMP].f >= DEFAULT_HS_FULL_TH + DEFAULT_HS_HYS)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 2)
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 2.5); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_HS_FAN_STATE] = 2;
	}
	//If above threshold without hysteresis, do not set fan full if we were not in lower level
	else if(system_status[SS_HEATSINK_TEMP].f >= DEFAULT_HS_FULL_TH)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 3 && system_monitoring[SMON_LAST_HS_FAN_STATE] != 2)
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 2.5); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 2;
		}
	}
	//If above threshold + hysteresis, set fan medium
	else if(system_status[SS_HEATSINK_TEMP].f >= DEFAULT_HS_MED_TH + DEFAULT_HS_HYS)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 3)
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 2.1); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_HS_FAN_STATE] = 3;
	}
	//If above threshold without hysteresis, do not set fan full if we were not in lower level
	else if(system_status[SS_HEATSINK_TEMP].f >= DEFAULT_HS_MED_TH)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 4 && system_monitoring[SMON_LAST_HS_FAN_STATE] != 3)
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 2.1); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 3;
		}
	}
	//If above threshold + hysteresis, set fan low
	else if(system_status[SS_HEATSINK_TEMP].f >= DEFAULT_HS_LOW_TH + DEFAULT_HS_HYS)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 4) 
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 1.8); //TBD TODO magic number
		}
		system_monitoring[SMON_LAST_HS_FAN_STATE] = 4;
	}
	else if(system_status[SS_HEATSINK_TEMP].f >= DEFAULT_HS_LOW_TH)
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 5 && system_monitoring[SMON_LAST_HS_FAN_STATE] != 4)
		{
			gpio_set_pin_level(IO_HS_FAN_EN, true);
			set_fan_voltage(HS_FAN_DAC_CH, 1.8); //TBD TODO magic number
			system_monitoring[SMON_LAST_HS_FAN_STATE] = 4;
		}
	}
	else
	{
		if(system_monitoring[SMON_LAST_HS_FAN_STATE] != 5)
		{
			gpio_set_pin_level(IO_HS_FAN_EN, false);
		}
		system_monitoring[SMON_LAST_HS_FAN_STATE] = 5;
	}
#endif
}

static void check_coils()
{
	//Check coils during emission only
	if(system_status[SS_STATE].u == STATE_EMISSION)
	{
		check_x_coil();
		check_y_coil();
		check_f_coil();	
	}
}

static void check_x_coil()
{	
	float actual_value = system_status[SS_X_COIL_CURRENT].f;
	
	//If coil values are what is expected, reset error count
	if(tolerance_check_abs(expected_coil_value[EV_COIL_X_A], actual_value, DEFAULT_DEFL_I_TOL))
	{
		coil_err_count[0] = 0;
	}
	//Otherwise increment error count and if count is high enough, report fault
	else
	{
		coil_err_count[0] += 1;
		if(coil_err_count[0] > 10)
		{
			report_fault(FAULT_COIL_CURRENT, COIL_FAULT_X_CURRENT, expected_coil_value[EV_COIL_X_A], DEFAULT_DEFL_I_TOL, actual_value);
		}
	}
}

static void check_y_coil()
{
	float actual_value = system_status[SS_Y_COIL_CURRENT].f;
	
	//If coil values are what is expected, reset error count
	if(tolerance_check_abs(expected_coil_value[EV_COIL_Y_A], actual_value, DEFAULT_DEFL_I_TOL))
	{
		coil_err_count[1] = 0;
	}
	//Otherwise increment error count and if count is high enough, report fault
	else
	{
		coil_err_count[1] += 1;
		if(coil_err_count[1] > 10)
		{
			report_fault(FAULT_COIL_CURRENT, COIL_FAULT_Y_CURRENT, expected_coil_value[EV_COIL_Y_A], DEFAULT_DEFL_I_TOL, actual_value);
		}
	}
}

static void check_f_coil()
{
	float actual_value = system_status[SS_F_COIL_CURRENT].f;
	
	//If coil values are what is expected, reset error count
	if(tolerance_check_abs(expected_coil_value[EV_COIL_F_A], actual_value, DEFAULT_FOCUS_I_TOL))
	{
		coil_err_count[2] = 0;
	}
	//Otherwise increment error count and if count is high enough, report fault
	else
	{
		coil_err_count[2] += 1;
		if(coil_err_count[2] > 10)
		{
			report_fault(FAULT_COIL_CURRENT, COIL_FAULT_F_CURRENT, expected_coil_value[EV_COIL_F_A], DEFAULT_FOCUS_I_TOL, actual_value);
		}
	}
	
	//TBD TODO can check for shorts here as well if desired on focus coil (expected voltage readings)
}

//Check the tolerance here relative to the actual target, eg percent of target value
bool tolerance_check_rel(float target, float actual, float tolerance)
{
	float tol_pct = tolerance / 100;
	if(actual > (target * (1+tol_pct)) || actual < (target * (1-tol_pct)))
	{
		return false;
	}
	return true;
}

//Check the tolerance here with an absolute value instead of percentage of target
bool tolerance_check_abs(float target, float actual, float tolerance)
{
	if(actual > (target + tolerance) || actual < (target - tolerance))
	{
		return false;
	}
	return true;
}

void enable_indicators(bool on)
{
	gpio_set_pin_level(IO_INDICATORS_EN, on);
	gpio_set_pin_level(IO_LED6, on);
}

void enable_pump(bool on)
{
	pump_on = on;
	gpio_set_pin_level(IO_PUMP_EN, on);
	
	if(on)
	{
		set_fan_voltage(PUMP_FAN_DAC_CH, 1.85);
	}
	else
	{
		set_fan_voltage(PUMP_FAN_DAC_CH, 0);
	}
}