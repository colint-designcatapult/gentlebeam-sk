#include <array>
#include <cstring>
#include <math.h>
#include <stddef.h>

#include "hvps_backend.h"
#include "hardware_backend.h"


extern "C" {



/* One shared status view, with the existing unavailable stub values. */
VariableValue hvps_status[NUM_HVPS_STATUS];


void init_hvps(void)
{
    gIoModel.hvps_ops->init_hvps();
}

void process_hvps(void)
{
    gIoModel.hvps_ops->process_hvps();
}

void hvps_req_timer(const struct timer_task *const timer_task)
{
    gIoModel.hvps_ops->hvps_req_timer(timer_task);
}

void init_hvps_check(void)
{
    gIoModel.hvps_ops->init_hvps_check();
}

bool update_hvps_check(void)
{
    return gIoModel.hvps_ops->update_hvps_check();
}

void enable_grid(bool on)
{
    gIoModel.hvps_ops->enable_grid(on);
}

void enable_ecc(bool on)
{
    gIoModel.hvps_ops->enable_ecc(on);
}

void enable_hv(bool on)
{
    gIoModel.hvps_ops->enable_hv(on);
}

void set_hvps_heater(float mA)
{
    gIoModel.hvps_ops->set_hvps_heater(mA);
}

void set_hvps_kv(float kv, float mA)
{
    gIoModel.hvps_ops->set_hvps_kv(kv, mA);
}

void set_hvps_ma_lim(float lim)
{
    gIoModel.hvps_ops->set_hvps_ma_lim(lim);
}

void set_hvps_grid(float grid_v)
{
    gIoModel.hvps_ops->set_hvps_grid(grid_v);
}

void enable_fast_warmup(bool en)
{
    gIoModel.hvps_ops->enable_fast_warmup(en);
}

void queue_hvps_cmd(HvpsCmd cmd, float param_f, uint32_t param_i)
{
    gIoModel.hvps_ops->queue_hvps_cmd(cmd, param_f, param_i);
}

/* No hardware, callbacks, commands or sensor responses are simulated. */
static void stub_init_hvps(void)
{
}

static void stub_process_hvps(void)
{
}

static void stub_hvps_req_timer(const struct timer_task *const timer_task)
{
    (void)timer_task;
}

static void stub_init_hvps_check(void)
{
}

static bool stub_update_hvps_check(void)
{
    /* No power supply is present to complete the hardware check. */
    return false;
}

static void stub_enable_grid(bool on)
{
    (void)on;
}

static void stub_enable_ecc(bool on)
{
    (void)on;
}

static void stub_enable_hv(bool on)
{
    (void)on;
}

static void stub_set_hvps_heater(float mA)
{
    (void)mA;
}

static void stub_set_hvps_kv(float kv, float mA)
{
    (void)kv;
    (void)mA;
}

static void stub_set_hvps_ma_lim(float lim)
{
    (void)lim;
}

static void stub_set_hvps_grid(float grid_v)
{
    (void)grid_v;
}

static void stub_enable_fast_warmup(bool en)
{
    (void)en;
}

static void stub_queue_hvps_cmd(HvpsCmd cmd, float param_f, uint32_t param_i)
{
    (void)cmd;
    (void)param_f;
    (void)param_i;
}

}

const HvpsBackendOps HVPS_REAL_OPS = {
	hvps_real_init_hvps,
	hvps_real_process_hvps,
	hvps_real_hvps_req_timer,
	hvps_real_init_hvps_check,
	hvps_real_update_hvps_check,
	hvps_real_enable_grid,
	hvps_real_enable_ecc,
	hvps_real_enable_hv,
	hvps_real_set_hvps_heater,
	hvps_real_set_hvps_kv,
	hvps_real_set_hvps_ma_lim,
	hvps_real_set_hvps_grid,
	hvps_real_enable_fast_warmup,
	hvps_real_queue_hvps_cmd
};

const HvpsBackendOps HVPS_STUB_OPS = {
    stub_init_hvps,
    stub_process_hvps,
    stub_hvps_req_timer,
    stub_init_hvps_check,
    stub_update_hvps_check,
    stub_enable_grid,
    stub_enable_ecc,
    stub_enable_hv,
    stub_set_hvps_heater,
    stub_set_hvps_kv,
    stub_set_hvps_ma_lim,
    stub_set_hvps_grid,
    stub_enable_fast_warmup,
    stub_queue_hvps_cmd
};


extern "C" {

void hvps_tick_1ms(void)
{

}

}