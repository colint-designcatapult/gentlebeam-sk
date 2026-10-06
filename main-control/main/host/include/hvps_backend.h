#ifndef GENTLEBEAM_HOST_HVPS_BACKEND_H
#define GENTLEBEAM_HOST_HVPS_BACKEND_H

#if !defined(GENTLEBEAM_HOST_BUILD) || !GENTLEBEAM_HOST_BUILD
#error "HVPS runtime dispatch is only available in host builds"
#endif

#include "hvps.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct {
    void (*init_hvps)(void);
    void (*process_hvps)(void);
    void (*hvps_req_timer)(const struct timer_task *const timer_task);
    void (*init_hvps_check)(void);
    bool (*update_hvps_check)(void);
    void (*enable_grid)(bool on);
    void (*enable_ecc)(bool on);
    void (*enable_hv)(bool on);
    void (*set_hvps_heater)(float mA);
    void (*set_hvps_kv)(float kv, float mA);
    void (*set_hvps_ma_lim)(float lim);
    void (*set_hvps_grid)(float grid_v);
    void (*enable_fast_warmup)(bool en);
    void (*queue_hvps_cmd)(HvpsCmd cmd, float param_f, uint32_t param_i);
} HvpsBackendOps;

void hvps_real_init_hvps();
void hvps_real_process_hvps();
void hvps_real_hvps_req_timer(const struct timer_task *const timer_task);
void hvps_real_init_hvps_check();
bool hvps_real_update_hvps_check();
void hvps_real_enable_grid(bool on);
void hvps_real_enable_ecc(bool on);
void hvps_real_enable_hv(bool on);
void hvps_real_set_hvps_heater(float mA);
void hvps_real_set_hvps_kv(float kv, float mA);
void hvps_real_set_hvps_ma_lim(float lim);
void hvps_real_set_hvps_grid(float grid_v);
void hvps_real_enable_fast_warmup(bool en);
void hvps_real_queue_hvps_cmd(HvpsCmd cmd, float param_f, uint32_t param_i);

extern const HvpsBackendOps HVPS_REAL_OPS;
extern const HvpsBackendOps HVPS_STUB_OPS;

#ifdef __cplusplus
}
#endif

#endif
