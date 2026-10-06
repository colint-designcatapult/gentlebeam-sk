#ifndef GENTLEBEAM_HOST_HEAD_BOARD_BACKEND_H
#define GENTLEBEAM_HOST_HEAD_BOARD_BACKEND_H

#if !defined(GENTLEBEAM_HOST_BUILD) || !GENTLEBEAM_HOST_BUILD
#error "Head board runtime dispatch is only available in host builds"
#endif

#include "head_board.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef struct {
    void (*init_head_board)(void);
    void (*process_hb)(void);
    void (*set_led_sequence)(int led_idx);
    void (*set_mag_cal_window)(int samples);
    void (*set_qc_desired_state)(HbQcDesiredState desired_state);
    void (*qc_session_reset)(void);
    QcSessionStatus (*qc_session_arm)(void);
    void (*qc_session_start_for_emission)(void);
    void (*qc_session_stop)(void);
    QcSessionStatus (*qc_session_get_status)(void);
} HeadBoardBackendOps;

void head_board_real_init_head_board(void);
void head_board_real_process_hb(void);
void head_board_real_set_led_sequence(int led_idx);
void head_board_real_set_mag_cal_window(int samples);
void head_board_real_set_qc_desired_state(HbQcDesiredState desired_state);
void head_board_real_qc_session_reset(void);
QcSessionStatus head_board_real_qc_session_arm(void);
void head_board_real_qc_session_start_for_emission(void);
void head_board_real_qc_session_stop(void);
QcSessionStatus head_board_real_qc_session_get_status(void);

extern const HeadBoardBackendOps HB_REAL_OPS;
extern const HeadBoardBackendOps HB_STUB_OPS;

#ifdef __cplusplus
}
#endif

#endif
