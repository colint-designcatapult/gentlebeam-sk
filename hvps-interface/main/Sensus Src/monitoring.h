#ifndef MONITORING_H_
#define MONITORING_H_


enum
{
	SYS_CONFIG_MAX_PWR = 0,
	SYS_CONFIG_MIN_KV,
	SYS_CONFIG_KV_BOUND,
	SYS_CONFIG_KV_RAMP_FAST,
	SYS_CONFIG_KV_RAMP_SLOW,
	SYS_CONFIG_MAX_KV,
	SYS_CONFIG_FIL_INIT,
	SYS_CONFIG_FIL_LIM,
	SYS_CONFIG_GRID_KP_50,
	SYS_CONFIG_GRID_KP_70,
	SYS_CONFIG_GRID_KP_100,
	SYS_CONFIG_GRID_INTEGRAL_TIME,
	SYS_CONFIG_GRID_SLEW_DOWN,
	SYS_CONFIG_GRID_SLEW_UP,
	SYS_CONFIG_RESERVED_14,
	SYS_CONFIG_RESERVED_15,
	SYS_CONFIG_RESERVED_16,
	SYS_CONFIG_RESERVED_17,
	SYS_CONFIG_RESERVED_18,
	SYS_CONFIG_RESERVED_19,
	SYS_CONFIG_RESERVED_20,
	SYS_CONFIG_RESERVED_21,
	SYS_CONFIG_RESERVED_22,
	SYS_CONFIG_RESERVED_23,
	SYS_CONFIG_RUN_PID,
	SYS_CONFIG_RESERVED_25,
	SYS_CONFIG_RESERVED_26,
	SYS_CONFIG_RESERVED_27,
	SYS_CONFIG_MIN_GRID,
	SYS_CONFIG_MAX_GRID,
	SYS_CONFIG_MAX_MA,
	SYS_CONFIG_FIL_LOW,
	NUM_SYS_CONFIG
};

enum
{
	SP_PWR = 0,
	SP_KV,
	SP_MA_LIM,
	SP_GRID,
	SP_FIL,
	SP_MA,
	NUM_SP
};

enum
{
	FB_KV = 0,
	FB_MA,
	FB_GRID,
	FB_FIL_A,
#ifndef CALIBRATION_MODE
	FB_FIL_V,
#endif
	NUM_FB
};

enum
{
	SYS_TEST = 0,
	SYS_HV_CTRL_EN,
	SYS_GRID_CTRL_EN,
	SYS_WARMING,
	SYS_KV_RAMPING,
	SYS_EMISSION_ON,
	SYS_UNLOCKED_CONFIG,
	SYS_PID_ON,
	SYS_CAL_GRID_INT_EN,
	SYS_FAST_WARMUP_EN,

	/* Individual latched fault flags, mirroring hvps_fault_mask sources */
	SYS_FAULT_FIL_CLK,
	SYS_FAULT_CAT_ARC,
	SYS_FAULT_FAN,
	SYS_FAULT_OC_24,
	SYS_FAULT_MASTER,
	SYS_FAULT_OC_HV,
	SYS_FAULT_TEMP_1,
	SYS_FAULT_OC_CAT,
	SYS_FAULT_TEMP_3,
	SYS_FAULT_TEMP_2,

	NUM_SYS_BITS,

};

typedef enum
{
	KV_DOWN = 0,
	KV_STAY,
	KV_UP
} KV_Status;

typedef enum
{
	FIL_DOWN = 0,
	FIL_STAY,
	FIL_UP
} FIL_Status;


extern float config_vals[NUM_SYS_CONFIG];

void setup_system_monitoring(void);
void process_monitoring(void);

bool sys_stat_check(uint8_t bitpos);
void set_sys_bit(uint8_t bitpos);
void clear_sys_bit(uint8_t bitpos);

void set_new_kv(float kv);
void set_new_pwr(float pwr);
void set_new_fil(float fil);

void report_io_state(uint32_t io_bits);
void report_int_adc_vals(uint16_t *vals);
void report_kv_fb(uint32_t kv_fb);
void report_ma_fb(uint32_t ma_fb);

float get_monitored_float_val(uint32_t comm_idx);
uint32_t get_monitored_int_val(uint32_t comm_idx);
bool sys_config_set(unsigned int index, float value);
float sys_config_get(unsigned int index);

extern float setpoints[NUM_SP];


#endif /* MONITORING_H_ */

