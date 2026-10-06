#pragma once

#include <stdint.h>
#include <stdbool.h>
#include "atmel_start.h"
#include "ext_timers.h"
#include "ext_adcs.h"
#include "hvps_backend.h"
#include "head_board_backend.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
	TIMER_STATE_CLEARED = 0,
	TIMER_STATE_PAUSED,
	TIMER_STATE_RUNNING,
	TIMER_STATE_ELAPSED
} GcbTimerState;

#define GCB_GPIO_PORT_COUNT 4

typedef struct {
	uint32_t io_ext_wd_rst : 1;
	uint32_t io_indicators_en : 1;
	uint32_t io_timers_start_n : 1;
	uint32_t pa3 : 1;
	uint32_t pa4 : 1;
	uint32_t pa5 : 1;
	uint32_t pa6 : 1;
	uint32_t reserved_7 : 1;
	uint32_t io_qc_en : 1;
	uint32_t pa9 : 1;
	uint32_t pa10 : 1;
	uint32_t io_hvps_rdy_n : 1;
	uint32_t io_hvps_warning : 1;
	uint32_t io_ac_fault : 1;
	uint32_t io_hv_on : 1;
	uint32_t io_grid_off : 1;
	uint32_t io_hv_en : 1;
	uint32_t io_grid_en_n : 1;
	uint32_t io_emission_en : 1;
	uint32_t io_hs_fan_en : 1;
	uint32_t io_cb_fan_en : 1;
	uint32_t io_pump_en : 1;
	uint32_t reserved_22 : 1;
	uint32_t io_ion_pump_en : 1;
	uint32_t io_ion_repeller_en : 1;
	uint32_t pa25 : 1;
	uint32_t pa26 : 1;
	uint32_t pa27 : 1;
	uint32_t pa28 : 1;
	uint32_t card_detect_0 : 1;
	uint32_t pa30 : 1;
	uint32_t pa31 : 1;
} GcbPortABits;

typedef struct {
	uint32_t pb0 : 1;
	uint32_t pb1 : 1;
	uint32_t io_coil_x_dir_n : 1;
	uint32_t io_coil_y_dir_n : 1;
	uint32_t reserved_4_31 : 28;
} GcbPortBBits;

typedef struct {
	uint32_t io_door_closed : 1;
	uint32_t io_drive_sys_locked : 1;
	uint32_t io_base_estop_n : 1;
	uint32_t io_remote_estop_n : 1;
	uint32_t io_kuka_fault_1_n : 1;
	uint32_t io_kuka_fault_2_n : 1;
	uint32_t io_water_level : 1;
	uint32_t io_ion_pump_hvon : 1;
	uint32_t io_timer_fault_1_n : 1;
	uint32_t io_timer_fault2_n : 1;
	uint32_t io_hvps_fault_n : 1;
	uint32_t io_cooler_fault_n : 1;
	uint32_t io_water_temp_fault_n : 1;
	uint32_t io_wd_fault_n : 1;
	uint32_t io_mcu_fault_n : 1;
	uint32_t spare_interlock_1 : 1;
	uint32_t io_master_fault_n : 1;
	uint32_t io_clear_fault : 1;
	uint32_t io_remote_key : 1;
	uint32_t io_base_key : 1;
	uint32_t io_coil_dac_ldac_n : 1;
	uint32_t io_coil_dac_clr_n : 1;
	uint32_t io_coil_dac_rdy_n : 1;
	uint32_t io_coil_dac_cs_n : 1;
	uint32_t pc24 : 1;
	uint32_t reserved_25 : 1;
	uint32_t pc26 : 1;
	uint32_t pc27 : 1;
	uint32_t io_fan_dac_ldac_n : 1;
	uint32_t io_fan_dac_clr_n : 1;
	uint32_t io_fan_dac_rdy_n : 1;
	uint32_t io_fan_dac_cs_n : 1;
} GcbPortCBits;

typedef struct {
	uint32_t pd0 : 1;
	uint32_t pd1 : 1;
	uint32_t pd2 : 1;
	uint32_t pd3 : 1;
	uint32_t pd4 : 1;
	uint32_t pd5 : 1;
	uint32_t pd6 : 1;
	uint32_t pd7 : 1;
	uint32_t pd8 : 1;
	uint32_t pd9 : 1;
	uint32_t phy_reset_pin : 1;
	uint32_t reserved_11 : 1;
	uint32_t io_led1 : 1;
	uint32_t io_led2 : 1;
	uint32_t io_led3 : 1;
	uint32_t io_led4 : 1;
	uint32_t io_led5 : 1;
	uint32_t io_led6 : 1;
	uint32_t pd18 : 1;
	uint32_t pd19 : 1;
	uint32_t reserved_20_24 : 5;
	uint32_t pd25 : 1;
	uint32_t pd26 : 1;
	uint32_t pd27 : 1;
	uint32_t pd28 : 1;
	uint32_t io_remote_led_1 : 1;
	uint32_t io_remote_led_2 : 1;
	uint32_t reserved_31 : 1;
} GcbPortDBits;

static_assert(sizeof(GcbPortABits) == sizeof(uint32_t), "Port A bitfields must occupy one word");
static_assert(sizeof(GcbPortBBits) == sizeof(uint32_t), "Port B bitfields must occupy one word");
static_assert(sizeof(GcbPortCBits) == sizeof(uint32_t), "Port C bitfields must occupy one word");
static_assert(sizeof(GcbPortDBits) == sizeof(uint32_t), "Port D bitfields must occupy one word");

typedef struct {
	union {
		uint32_t port_levels[GCB_GPIO_PORT_COUNT];
		struct {
			GcbPortABits port_a;
			GcbPortBBits port_b;
			GcbPortCBits port_c;
			GcbPortDBits port_d;
		} ports;
	};
	bool simulate;
} GcbGpioModel;

typedef struct {
    GcbTimerState state;
    uint32_t time_raw;
    bool simulate;
    bool response_suppressed;
    bool checksum_corrupted;
    uint16_t tick_remainder;
    uint8_t rx_buf[TIMER_RX_SIZE];
} GcbTimerModel;

/* ADC input voltages before the firmware's system-parameter calibration. */
typedef struct {
	float temperature;
	float f_voltage;
	float y_voltage;
	float x_voltage;
	float f_current;
	float y_current;
	float x_current;
} GcbCoilAdcModel;

typedef struct {
	float voltage_12;
	float voltage_5;
	float voltage_3p3;
	float ion_pump_current_1;
	float ion_pump_current_2;
	float ion_pump_voltage;
	float cabinet_thermistor;
	float heatsink_thermistor;
} GcbSystemAdcModel;

typedef struct {
	float repeller_voltage;
	float repeller_current;
} GcbIonRepellerAdcModel;

typedef struct {
	GcbCoilAdcModel coil;
	GcbSystemAdcModel system;
	GcbIonRepellerAdcModel ion_repeller;
} GcbExtAdcsModel;

/* DAC output voltages after DAC quantization. */
typedef struct {
	float heatsink;
	float cabinet;
	float pump;
} GcbFanDacModel;

typedef struct {
	float x;
	float y;
	float f;
    bool simulate;
} GcbCoilDacModel;

typedef struct {
	GcbFanDacModel fan;
	GcbCoilDacModel coil;
} GcbExtDacModel;


typedef struct {
    GcbTimerModel backup_timer1;
    GcbTimerModel backup_timer2;
    GcbExtAdcsModel adcs;
    GcbExtDacModel dac;
    GcbGpioModel gpio;
    const HvpsBackendOps* hvps_ops;
    const HeadBoardBackendOps* hb_ops;
} GcbIoModel;

extern GcbIoModel gIoModel;

extern void ext_timers_i2c_tick_1ms(void);
extern void hvps_tick_1ms(void);
extern void host_apply_gpio_port_levels(const uint32_t port_levels[GCB_GPIO_PORT_COUNT]);
extern void host_io_model_tick(void);


#ifdef __cplusplus
}
#endif