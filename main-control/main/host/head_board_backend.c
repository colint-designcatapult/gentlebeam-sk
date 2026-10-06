#include "head_board.h"
#include "hardware_backend.h"

VariableValue mag_cal_array[HB_NUM_MAG_CAL];

void init_head_board(void)
{
    gIoModel.hb_ops->init_head_board();
}

void process_hb(void)
{
    gIoModel.hb_ops->process_hb();
}

void set_led_sequence(int led_idx)
{
    gIoModel.hb_ops->set_led_sequence(led_idx);
}

void set_mag_cal_window(int samples)
{
    gIoModel.hb_ops->set_mag_cal_window(samples);
}

void set_qc_desired_state(HbQcDesiredState desired_state)
{
    gIoModel.hb_ops->set_qc_desired_state(desired_state);
}

void qc_session_reset(void)
{
    gIoModel.hb_ops->qc_session_reset();
}

QcSessionStatus qc_session_arm(void)
{
    return gIoModel.hb_ops->qc_session_arm();
}

void qc_session_start_for_emission(void)
{
    gIoModel.hb_ops->qc_session_start_for_emission();
}

void qc_session_stop(void)
{
    gIoModel.hb_ops->qc_session_stop();
}

QcSessionStatus qc_session_get_status(void)
{
    return gIoModel.hb_ops->qc_session_get_status();
}

/* No hardware, callbacks, commands or sensor responses are simulated. */
static void stub_init_head_board(void)
{
}

static void stub_process_hb(void)
{
}

static void stub_set_led_sequence(int led_idx)
{
    (void)led_idx;
}

static void stub_set_mag_cal_window(int samples)
{
    (void)samples;
}

static void stub_set_qc_desired_state(HbQcDesiredState desired_state)
{
    (void)desired_state;
}

/* A missing head board cannot arm, acquire, or successfully finish QC. */
static void stub_qc_session_reset(void)
{
    qc_reported[QC_RES_SESSION_STATUS].u = QC_SESSION_ERROR;
}

static QcSessionStatus stub_qc_session_arm(void)
{
    qc_reported[QC_RES_SESSION_STATUS].u = QC_SESSION_ERROR;
    return QC_SESSION_ERROR;
}

static void stub_qc_session_start_for_emission(void)
{
    qc_reported[QC_RES_SESSION_STATUS].u = QC_SESSION_ERROR;
}

static void stub_qc_session_stop(void)
{
    qc_reported[QC_RES_SESSION_STATUS].u = QC_SESSION_ERROR;
}

static QcSessionStatus stub_qc_session_get_status(void)
{
    return QC_SESSION_ERROR;
}

const HeadBoardBackendOps HB_STUB_OPS = {
    .init_head_board = stub_init_head_board,
    .process_hb = stub_process_hb,
    .set_led_sequence = stub_set_led_sequence,
    .set_mag_cal_window = stub_set_mag_cal_window,
    .set_qc_desired_state = stub_set_qc_desired_state,
    .qc_session_reset = stub_qc_session_reset,
    .qc_session_arm = stub_qc_session_arm,
    .qc_session_start_for_emission = stub_qc_session_start_for_emission,
    .qc_session_stop = stub_qc_session_stop,
    .qc_session_get_status = stub_qc_session_get_status,
};

const HeadBoardBackendOps HB_REAL_OPS = {
    .init_head_board = head_board_real_init_head_board,
    .process_hb = head_board_real_process_hb,
    .set_led_sequence = head_board_real_set_led_sequence,
    .set_mag_cal_window = head_board_real_set_mag_cal_window,
    .set_qc_desired_state = head_board_real_set_qc_desired_state,
    .qc_session_reset = head_board_real_qc_session_reset,
    .qc_session_arm = head_board_real_qc_session_arm,
    .qc_session_start_for_emission = head_board_real_qc_session_start_for_emission,
    .qc_session_stop = head_board_real_qc_session_stop,
    .qc_session_get_status = head_board_real_qc_session_get_status,
};
