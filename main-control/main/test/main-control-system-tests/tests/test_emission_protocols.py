"""Scalar emission lifecycle and QC launch protocols over real host interfaces."""

from __future__ import annotations

import time

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.head_interface import (
    HeadInterfaceSimulator, QC_ACTIVE, QC_COMPLETE, QC_ERROR, QC_STOPPED,
)
from main_control_system_tests.hvps import HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    BeamQaResult, Directive, Fault, NormalTelemetry, OperationalPoint, QcSessionStatus, State,
)
from main_control_system_tests.protocol import MainControlClient


def _record_telemetry(evidence: Evidence, phase: str, telemetry, **expected) -> None:
    evidence.record(
        phase, observed_state=telemetry.state if telemetry is not None else None,
        observed_faults=telemetry.faults if telemetry is not None else None,
        observed_kv_feedback=telemetry.kv_feedback if telemetry is not None else None,
        observed_kv_target=telemetry.kv_setpoint if telemetry is not None else None,
        observed_ma_feedback=telemetry.ma_feedback if telemetry is not None else None,
        observed_ma_limit_target=telemetry.ma_limit_setpoint if telemetry is not None else None,
        observed_heater_feedback_ma=telemetry.heater_feedback if telemetry is not None else None,
        observed_heater_target_ma=telemetry.heater_setpoint if telemetry is not None else None,
        observed_internal_timer_s=telemetry.internal_timer_s if telemetry is not None else None,
        observed_hvps_flags=telemetry.hvps_flags if telemetry is not None else None,
        **expected,
    )


def _beam_disabled(io_model: HostIOModel, evidence: Evidence, *, record: bool = True) -> dict:
    pins = io_model.read()["gpio"]["pins"]["port_a"]
    outputs = {name: pins[name] for name in ("io_hv_en", "io_emission_en", "io_grid_en_n")}
    enabled = all(outputs.values())
    if record or enabled:
        evidence.record("Beam enable outputs", observed_outputs=outputs,
                        observed_beam_enabled=enabled, expected_beam_enabled=False)
    assert not (pins["io_hv_en"] and pins["io_emission_en"] and pins["io_grid_en_n"])
    return outputs


def _hold_nonemitting(
    client: MainControlClient, io_model: HostIOModel, state: State, seconds: float,
    evidence: Evidence,
) -> None:
    started = time.monotonic()
    deadline = started + seconds
    telemetry = None
    outputs = None
    samples = 0
    try:
        while True:
            telemetry = client.query_telemetry()
            samples += 1
            assert telemetry.state == state
            assert telemetry.faults == Fault.NONE
            outputs = _beam_disabled(io_model, evidence, record=False)
            if time.monotonic() >= deadline:
                return
            time.sleep(0.025)
    finally:
        _record_telemetry(evidence, "Non-emitting hold", telemetry,
                          observed_elapsed_s=time.monotonic() - started,
                          observed_samples=samples, observed_outputs=outputs,
                          expected_state=state, expected_faults=Fault.NONE,
                          expected_hold_s=seconds, expected_beam_enabled=False)


def _stage(client: MainControlClient, point: OperationalPoint, evidence: Evidence) -> int:
    client.clear_plan()
    client.warmup()
    client.wait_for_state(State.PRIMED)
    client.directive(Directive.RESET_TIMERS)
    reset = client.wait_for_telemetry(
        lambda item: item.internal_timer_s == item.timer_1_s == item.timer_2_s == 0,
    )
    evidence.record("Staging timer reset (s)",
                    observed_timers=(reset.internal_timer_s, reset.timer_1_s, reset.timer_2_s),
                    expected_timers=(0, 0, 0), requested_point=point)
    session = client.new_session()
    client.wait_for_state(State.STAGING)
    client.load_operational_point(session, point)
    client.directive(Directive.STAGE_PLAN)
    client.wait_for_state(State.STAGED)
    client.confirm_operational_point(session, point)
    return session


def _finish(
    client: MainControlClient, io_model: HostIOModel, timeout: float, evidence: Evidence,
) -> NormalTelemetry:
    started = time.monotonic()
    deadline = started + timeout
    states = []
    telemetry = None
    try:
        while True:
            telemetry = client.query_telemetry()
            assert telemetry.faults == Fault.NONE
            if not states or states[-1] != telemetry.state:
                states.append(telemetry.state)
            assert telemetry.state in (State.EMISSION, State.DISCHARGE, State.COLD), states
            if telemetry.state == State.COLD:
                assert State.DISCHARGE in states, states
                _beam_disabled(io_model, evidence)
                return telemetry
            assert time.monotonic() < deadline, states
            time.sleep(0.01)
    finally:
        _record_telemetry(evidence, "Emission finishes through discharge", telemetry,
                          observed_states=states, observed_elapsed_s=time.monotonic() - started,
                          expected_states=(State.EMISSION, State.DISCHARGE, State.COLD),
                          expected_final_state=State.COLD, expected_faults=Fault.NONE,
                          timeout_s=timeout)


def _wait_fault(
    client: MainControlClient, io_model: HostIOModel, fault: Fault, timeout: float,
    evidence: Evidence,
) -> NormalTelemetry:
    started = time.monotonic()
    telemetry = client.wait_for_telemetry(lambda item: bool(item.faults & fault), timeout)
    _record_telemetry(evidence, "Emission fault", telemetry, expected_fault_mask=fault,
                      observed_elapsed_s=time.monotonic() - started, timeout_s=timeout)
    faulted = client.wait_for_state(State.FAULT)
    _record_telemetry(evidence, "Fault state reached", faulted, expected_state=State.FAULT)
    _beam_disabled(io_model, evidence)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Fault retains undelivered time (s)", observed_remaining_s=remaining,
                    expected_remaining_lower_exclusive_s=0)
    assert remaining > 0
    return telemetry


@pytest.mark.strictdoc("TC-H1FWMC-89")
def test_pc_stop_terminates_emission(
    client: MainControlClient, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-1, scope=function, role=Verifies)
    UID: TC-H1FWMC-89
    TITLE: Stop an ongoing emission
    STATEMENT: A PC Stop command terminates radiation before the planned end.
    PREREQUISITES: Healthy system and a 10-second, 50 kV, 1 mA emission.
    STEPS: Prepare and release; observe at least 0.5 seconds of emission; send Stop.
    EXPECTED_BEHAVIOR: Radiation stops, Discharge reaches Cold, and remaining time is retained.
    """
    point = OperationalPoint.beam_qa(50, 10)
    session = client.prepare_emission(point)
    client.start_emission(session)
    active = client.wait_for_telemetry(lambda item: item.internal_timer_s >= 0.5)
    _record_telemetry(evidence, "Emission before PC stop", active,
                      expected_state=State.EMISSION, expected_ma=point.ma,
                      relative_tolerance=0.05, expected_minimum_delivery_s=0.5)
    assert active.state == State.EMISSION
    assert active.ma_feedback == pytest.approx(point.ma, rel=0.05)
    client.directive(Directive.STOP)
    _finish(client, io_model, 5, evidence)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Stopped remaining time (s)", observed_remaining_s=remaining,
                    expected_exclusive_bounds_s=(0, point.total_time_s))
    assert 0 < remaining < point.total_time_s
    _hold_nonemitting(client, io_model, State.COLD, 0.5, evidence)
    held_remaining = client.query_operational_point().remaining_time_s
    evidence.record("Stopped time retained during cold hold (s)",
                    observed_remaining_s=held_remaining, expected_remaining_s=remaining)
    assert held_remaining == remaining


@pytest.mark.strictdoc("TC-H1FWMC-100")
def test_setup_voltage_target_timeout(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-14, scope=function, role=Verifies)
    UID: TC-H1FWMC-100
    TITLE: Operational voltage setup timeout
    STATEMENT: Failure to reach the requested voltage produces a fault within 90 seconds.
    PREREQUISITES: Healthy system, 50 kV plan, and HVPS feedback held at 0 kV.
    STEPS: Stage and confirm the plan, release it, and monitor Setup for 95 seconds.
    EXPECTED_BEHAVIOR: Radiation stays off; a kV fault occurs at 90 +/- 5 seconds, not Ready.
    """
    session = _stage(client, OperationalPoint.beam_qa(50), evidence)
    hvps.set_feedback(kv=0.0)
    client.release_plan(session)
    setup = client.wait_for_state(State.SETUP)
    started = time.monotonic()
    deadline = started + 95
    telemetry = setup
    outputs = None
    samples = 0
    try:
        while True:
            telemetry = client.query_telemetry()
            samples += 1
            outputs = _beam_disabled(io_model, evidence, record=False)
            if telemetry.faults:
                assert telemetry.faults & Fault.KV
                elapsed = (telemetry.runtime_ms - setup.runtime_ms) / 1000
                assert 85 <= elapsed <= 95, elapsed
                break
            assert telemetry.state == State.SETUP
            assert time.monotonic() < deadline, "No kV fault within the required 90 +/- 5 seconds"
            time.sleep(0.05)
    finally:
        _record_telemetry(evidence, "Voltage setup timeout", telemetry,
                          observed_elapsed_s=time.monotonic() - started,
                          observed_runtime_elapsed_s=(telemetry.runtime_ms - setup.runtime_ms) / 1000,
                          observed_samples=samples, observed_outputs=outputs,
                          expected_fault_mask=Fault.KV, expected_timeout_range_s=(85, 95),
                          expected_kv_target=50, imposed_kv_feedback=0, expected_beam_enabled=False)
    faulted = client.wait_for_state(State.FAULT)
    _record_telemetry(evidence, "Setup timeout reaches fault", faulted, expected_state=State.FAULT)


@pytest.mark.strictdoc("TC-H1FWMC-101")
def test_emission_voltage_feedback_monitoring(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-15, scope=function, role=Verifies)
    UID: TC-H1FWMC-101
    TITLE: High-voltage feedback monitoring
    STATEMENT: A voltage deviation greater than 5% interrupts emission with a kV fault.
    PREREQUISITES: Healthy system and a 20-second, 50 kV, 1 mA emission.
    STEPS: Emit at 48 kV for 3 seconds, then clamp feedback to 45 kV.
    EXPECTED_BEHAVIOR: The 4% deviation is accepted; 10% produces a fault and leaves undelivered time.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, 20))
    client.start_emission(session)
    hvps.set_feedback(kv=48)
    telemetry = client.wait_for_telemetry(lambda item: item.kv_feedback == 48)
    started = time.monotonic()
    deadline = started + 3
    samples = 0
    try:
        while time.monotonic() < deadline:
            telemetry = client.query_telemetry()
            samples += 1
            assert telemetry.state == State.EMISSION and telemetry.faults == Fault.NONE
            time.sleep(0.05)
    finally:
        _record_telemetry(evidence, "Accepted voltage deviation hold", telemetry,
                          observed_elapsed_s=time.monotonic() - started, observed_samples=samples,
                          expected_hold_s=3, expected_state=State.EMISSION, expected_faults=Fault.NONE,
                          imposed_kv_feedback=48, expected_kv_target=50, relative_tolerance=0.05)
    hvps.set_feedback(kv=45)
    fault_started = time.monotonic()
    try:
        faulted = client.wait_for_telemetry(lambda item: bool(item.faults & Fault.KV), 5)
        _record_telemetry(evidence, "Rejected voltage deviation", faulted,
                          observed_elapsed_s=time.monotonic() - fault_started, timeout_s=5,
                          imposed_kv_feedback=45, expected_kv_target=50,
                          relative_tolerance=0.05, expected_fault_mask=Fault.KV)
        assert faulted.faults & Fault.KV
    finally:
        hvps.set_feedback(kv=None)
    fault_state = client.wait_for_state(State.FAULT)
    _record_telemetry(evidence, "Voltage fault state", fault_state, expected_state=State.FAULT)
    _beam_disabled(io_model, evidence)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Voltage fault retains time (s)", observed_remaining_s=remaining,
                    expected_remaining_lower_exclusive_s=0)
    assert remaining > 0


@pytest.mark.strictdoc("TC-H1FWMC-102")
def test_ready_two_minute_timeout(
    client: MainControlClient, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-86, scope=function, role=Verifies)
    UID: TC-H1FWMC-102
    TITLE: Ready idle timeout
    STATEMENT: An unreleased emission returns from Ready through Discharge to Cold after 2 minutes.
    PREREQUISITES: Healthy system and a prepared 50 kV emission.
    STEPS: Withhold point release; poll telemetry for 125 seconds while maintaining communication.
    EXPECTED_BEHAVIOR: No radiation or delivered time; Ready ends at 120 +/- 2 seconds without a fault.
    """
    point = OperationalPoint.beam_qa(50)
    client.prepare_emission(point)
    ready = client.query_telemetry()
    started = time.monotonic()
    deadline = started + 125
    seen_discharge = False
    departure = None
    telemetry = ready
    outputs = None
    samples = 0
    try:
        while True:
            telemetry = client.query_telemetry()
            samples += 1
            assert telemetry.faults == Fault.NONE
            outputs = _beam_disabled(io_model, evidence, record=False)
            assert telemetry.internal_timer_s == 0
            assert telemetry.state in (State.READY, State.DISCHARGE, State.COLD)
            if telemetry.state != State.READY and departure is None:
                departure = (telemetry.runtime_ms - ready.runtime_ms) / 1000
            seen_discharge |= telemetry.state == State.DISCHARGE
            if telemetry.state == State.COLD:
                break
            assert time.monotonic() < deadline, telemetry
            time.sleep(0.025)
    finally:
        _record_telemetry(evidence, "Ready idle timeout", telemetry,
                          observed_elapsed_s=time.monotonic() - started, observed_samples=samples,
                          observed_departure_s=departure, expected_departure_range_s=(118, 122),
                          observed_discharge=seen_discharge, expected_discharge=True,
                          observed_outputs=outputs, expected_beam_enabled=False,
                          expected_internal_timer_s=0, expected_final_state=State.COLD,
                          expected_faults=Fault.NONE)
    assert seen_discharge
    assert departure is not None and 118 <= departure <= 122, departure
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Ready timeout undelivered time (s)", observed_remaining_s=remaining,
                    expected_remaining_s=point.remaining_time_s)
    assert remaining == point.remaining_time_s


@pytest.mark.strictdoc("TC-H1FWMC-103")
def test_ordinary_launch_waits_for_filament(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-16, scope=function, role=Verifies)
    UID: TC-H1FWMC-103
    TITLE: Launch waits for the filament target
    STATEMENT: Ordinary radiation starts only after the filament reaches its requested current.
    PREREQUISITES: Healthy system, 50 kV, 1 mA, 2500 mA filament, 2-second plan.
    STEPS: Prepare, hold filament at 1500 mA while warming, release, then restore 2500 mA feedback.
    EXPECTED_BEHAVIOR: Launching remains non-emitting for 0.5 seconds; settled feedback permits emission.
    """
    point = OperationalPoint.beam_qa(50)
    session = client.prepare_emission(point)
    hvps.set_feedback(heater=1500)
    hvps.set_warming(True)
    warming = client.wait_for_telemetry(lambda item: item.heater_feedback == 1500)
    _record_telemetry(evidence, "Filament below launch target", warming,
                      imposed_heater_feedback_ma=1500, expected_heater_target_ma=point.heater_ma)
    client.release_point(session)
    client.wait_for_state(State.LAUNCHING)
    _hold_nonemitting(client, io_model, State.LAUNCHING, 0.5, evidence)
    hvps.set_feedback(heater=None)
    hvps.set_warming(None)
    emitted = client.wait_for_state(State.EMISSION)
    _record_telemetry(evidence, "Settled filament permits emission", emitted,
                      expected_state=State.EMISSION, expected_heater_feedback_ma=point.heater_ma,
                      relative_tolerance=0.05)
    assert emitted.heater_feedback == pytest.approx(point.heater_ma, rel=0.05)
    _finish(client, io_model, 5, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-103")
def test_qc_active_acknowledgment_releases_one_emission(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-16, scope=function, role=Verifies)
    UID: TC-H1FWMC-103
    TITLE: QC Active acknowledgment gates radiation
    STATEMENT: QC Launching waits for Active, and repeated Active reports cannot restart delivery.
    PREREQUISITES: Healthy prepared 2-second emission and manually controlled QC acknowledgments.
    STEPS: Arm QC, release, delay Active 0.4 seconds; report Active continuously through completion.
    EXPECTED_BEHAVIOR: Radiation waits for acknowledgment, emits once, and stays off after completion.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50))
    head_interface.configure_qc(auto_respond=False)
    head_interface.set_feedback(qc_acquisition_state=QC_STOPPED)
    client.arm_beam_qa()
    released = time.monotonic()
    client.release_point(session)
    client.wait_for_state(State.LAUNCHING)
    command = head_interface.wait_for_command(qc_desired_state=QC_ACTIVE, after=released)
    evidence.record("QC launch request", observed_desired_state=command.qc_desired_state,
                    expected_desired_state=QC_ACTIVE, observed_after_release_s=command.received_at - released)
    _hold_nonemitting(client, io_model, State.LAUNCHING, 0.4, evidence)
    head_interface.set_feedback(qc_acquisition_state=QC_ACTIVE)
    active = client.wait_for_state(State.EMISSION)
    _record_telemetry(evidence, "QC Active acknowledgment permits emission", active,
                      supplied_qc_ack=QC_ACTIVE, expected_state=State.EMISSION)
    _finish(client, io_model, 5, evidence)
    _hold_nonemitting(client, io_model, State.COLD, 0.25, evidence)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("QC delivery remaining time (s)", observed_remaining_s=remaining,
                    expected_remaining_s=0, absolute_tolerance_s=0.11)
    assert remaining == pytest.approx(0, abs=0.11)
    stopped = head_interface.wait_for_command(qc_desired_state=QC_STOPPED, after=released)
    head_interface.set_feedback(qc_acquisition_state=QC_COMPLETE)
    result = client.beam_qa_result()
    evidence.record("QC completion result", observed_result=result,
                    expected_status=QcSessionStatus.COMPLETE,
                    observed_stop_desired_state=stopped.qc_desired_state,
                    expected_stop_desired_state=QC_STOPPED)
    assert result.status == QcSessionStatus.COMPLETE
    later_states = [command.qc_desired_state for command in head_interface.commands
                    if command.received_at > stopped.received_at]
    evidence.record("No QC restart after completion", observed_later_desired_states=later_states,
                    forbidden_desired_state=QC_ACTIVE)
    assert not any(state == QC_ACTIVE for state in later_states)


@pytest.mark.strictdoc("TC-H1FWMC-103")
@pytest.mark.parametrize("failure", ["timeout", "error"])
@pytest.mark.parametrize("clear_ack", [QC_STOPPED, QC_COMPLETE, QC_ERROR],
                         ids=["stopped", "complete", "retained-error"])
def test_qc_failure_clear_and_stale_active(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, io_model: HostIOModel,
    failure: str, clear_ack: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-16, scope=function, role=Verifies)
    UID: TC-H1FWMC-103
    TITLE: QC failure, clear, and stale acknowledgment safety
    STATEMENT: Missing or rejected QC acknowledgment prevents radiation and is safely cleared.
    PREREQUISITES: Healthy 50 kV plan and controlled head QC status, totals, and sample counts.
    STEPS: Withhold Active or report Error; clear the fault with stale Active, then acknowledge
      Stopped using Stopped, Complete, or retained Error. Prepare another session and reject it.
    EXPECTED_BEHAVIOR: Timeout at 1 second or Error reports QC fault without radiation. Clear
      returns Idle with zero totals/counts, retransmits Stopped, ignores stale Active, accepts
      each stop acknowledgment, and allows a subsequent session to report its own QC fault.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, 3))
    head_interface.configure_qc(auto_respond=False)
    head_interface.set_feedback(qc_acquisition_state=QC_STOPPED)
    client.arm_beam_qa()
    released = time.monotonic()
    client.release_point(session)
    command = head_interface.wait_for_command(qc_desired_state=QC_ACTIVE, after=released)
    head_interface.set_feedback(qc_accumulation_0=123456, qc_accumulation_1=789,
                                qc_sample_count_0=112, qc_sample_count_1=16)
    _hold_nonemitting(client, io_model, State.LAUNCHING, 0.3, evidence)
    if failure == "error":
        head_interface.set_feedback(qc_acquisition_state=QC_ERROR)
    started = time.monotonic()
    deadline = started + 2
    telemetry = None
    outputs = None
    samples = 0
    try:
        while True:
            telemetry = client.query_telemetry()
            samples += 1
            outputs = _beam_disabled(io_model, evidence, record=False)
            assert telemetry.state != State.EMISSION
            if telemetry.faults & Fault.QC:
                break
            assert time.monotonic() < deadline, "QC acknowledgment failure did not report a fault"
            time.sleep(0.01)
    finally:
        _record_telemetry(evidence, "QC acknowledgment failure", telemetry,
                          failure_mode=failure, observed_elapsed_s=time.monotonic() - started,
                          observed_samples=samples, observed_outputs=outputs,
                          expected_fault_mask=Fault.QC, forbidden_state=State.EMISSION,
                          expected_beam_enabled=False, timeout_s=2)
    if failure == "timeout":
        elapsed = time.monotonic() - command.received_at
        evidence.record("QC acknowledgment timeout (s)", observed_elapsed_s=elapsed,
                        expected_elapsed_range_s=(0.85, 1.35))
        assert 0.85 <= elapsed <= 1.35
    client.wait_for_state(State.FAULT)
    failed = client.stop_beam_qa()
    evidence.record("Failed QC status and retained totals", observed_result=failed,
                    expected_result=BeamQaResult(123456, 789, 112, 16, QcSessionStatus.ERROR))
    assert failed.status == QcSessionStatus.ERROR
    assert (failed.channel_0_accumulation, failed.channel_1_accumulation,
            failed.channel_0_sample_count, failed.channel_1_sample_count) == (123456, 789, 112, 16)
    stopped = head_interface.wait_for_command(qc_desired_state=QC_STOPPED, after=command.received_at)
    evidence.record("QC failure requests stop", observed_desired_state=stopped.qc_desired_state,
                    expected_desired_state=QC_STOPPED)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("QC failure preserves time (s)", observed_remaining_s=remaining, expected_remaining_s=3)
    assert remaining == 3

    # Clear while the head still reports Active: that stale report is not a new release.
    head_interface.set_feedback(qc_acquisition_state=QC_ACTIVE)
    head_interface.wait_for_feedback(after_count=head_interface.sent_frame_count)
    cleared = time.monotonic()
    client.clear_faults()
    client.wait_for_state(State.COLD)
    first_stop = head_interface.wait_for_command(qc_desired_state=QC_STOPPED, after=cleared)
    _hold_nonemitting(client, io_model, State.COLD, 0.3, evidence)
    repeated_stop = head_interface.wait_for_command(qc_desired_state=QC_STOPPED, after=first_stop.received_at)
    evidence.record("Clear retransmits stop despite stale Active",
                    observed_desired_states=(first_stop.qc_desired_state, repeated_stop.qc_desired_state),
                    expected_desired_states=(QC_STOPPED, QC_STOPPED),
                    observed_retransmission_interval_s=repeated_stop.received_at - first_stop.received_at,
                    supplied_stale_ack=QC_ACTIVE)
    reset = client.stop_beam_qa()
    evidence.record("Clear resets QC totals and counts", observed_result=reset,
                    expected_result=BeamQaResult(0, 0, 0, 0, QcSessionStatus.IDLE))
    assert reset == BeamQaResult(0, 0, 0, 0, QcSessionStatus.IDLE)
    head_interface.set_feedback(qc_acquisition_state=clear_ack)
    head_interface.wait_for_feedback(after_count=head_interface.sent_frame_count)
    _hold_nonemitting(client, io_model, State.COLD, 1.2, evidence)
    acknowledged = client.stop_beam_qa()
    evidence.record("Stop acknowledgment keeps QC idle", supplied_ack=clear_ack,
                    observed_result=acknowledged, expected_result=BeamQaResult(0, 0, 0, 0, QcSessionStatus.IDLE))
    assert acknowledged == BeamQaResult(0, 0, 0, 0, QcSessionStatus.IDLE)

    head_interface.set_feedback(qc_acquisition_state=QC_STOPPED)
    session = client.prepare_emission(OperationalPoint.beam_qa(50, 3))
    client.arm_beam_qa()
    released = time.monotonic()
    client.release_point(session)
    head_interface.wait_for_command(qc_desired_state=QC_ACTIVE, after=released)
    head_interface.set_feedback(qc_acquisition_state=QC_ERROR)
    _wait_fault(client, io_model, Fault.QC, 2, evidence)
    retried = client.stop_beam_qa()
    evidence.record("Subsequent QC session reports its own error", observed_result=retried,
                    expected_status=QcSessionStatus.ERROR)
    assert retried.status == QcSessionStatus.ERROR
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Subsequent QC failure preserves time (s)", observed_remaining_s=remaining,
                    expected_remaining_s=3)
    assert remaining == 3


@pytest.mark.strictdoc("TC-H1FWMC-104")
def test_ready_requires_explicit_point_release(
    client: MainControlClient, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-17, scope=function, role=Verifies)
    UID: TC-H1FWMC-104
    TITLE: Explicit emission release
    STATEMENT: Ready never automatically releases radiation.
    PREREQUISITES: Healthy prepared 2-second, 50 kV emission.
    STEPS: Wait 1 second in Ready, send one point release, and observe completion.
    EXPECTED_BEHAVIOR: No delivered time before release; one emission ends through Discharge without Setup.
    """
    point = OperationalPoint.beam_qa(50)
    session = client.prepare_emission(point)
    _hold_nonemitting(client, io_model, State.READY, 1, evidence)
    observed_point = client.query_operational_point()
    evidence.record("Unreleased point unchanged", observed_point=observed_point, expected_point=point)
    assert observed_point == point
    ready = client.query_telemetry()
    _record_telemetry(evidence, "No delivery before explicit release", ready, expected_internal_timer_s=0)
    assert ready.internal_timer_s == 0
    client.start_emission(session)
    _finish(client, io_model, 5, evidence)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Explicitly released delivery completes (s)", observed_remaining_s=remaining,
                    expected_remaining_s=0, absolute_tolerance_s=0.11)
    assert remaining == pytest.approx(0, abs=0.11)
    _hold_nonemitting(client, io_model, State.COLD, 0.5, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-105")
def test_separate_qa_sessions_need_separate_release(
    client: MainControlClient, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-18, scope=function, role=Verifies)
    UID: TC-H1FWMC-105
    TITLE: Separate single-point QA sessions
    STATEMENT: Each requested QA emission requires its own session and point release.
    PREREQUISITES: Healthy system and 2-second QA emissions at 50 and 70 kV.
    STEPS: Complete the first session; prepare the second, wait 1 second, then release it.
    EXPECTED_BEHAVIOR: First completion discharges; the second remains non-emitting until its own release.
    """
    first = client.prepare_emission(OperationalPoint.beam_qa(50))
    client.start_emission(first)
    _finish(client, io_model, 5, evidence)
    second_point = OperationalPoint.beam_qa(70)
    second = client.prepare_emission(second_point)
    _hold_nonemitting(client, io_model, State.READY, 1, evidence)
    observed_point = client.query_operational_point()
    evidence.record("Second unreleased session", observed_point=observed_point, expected_point=second_point,
                    first_session=first, second_session=second)
    assert observed_point == second_point
    ready = client.query_telemetry()
    _record_telemetry(evidence, "Second session has no delivery before release", ready,
                      expected_internal_timer_s=0)
    assert ready.internal_timer_s == 0
    active = client.start_emission(second)
    _record_telemetry(evidence, "Second session released at its voltage", active,
                      expected_kv_feedback=70, relative_tolerance=0.05)
    assert active.kv_feedback == pytest.approx(70, rel=0.05)
    _finish(client, io_model, 5, evidence)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Second session completes (s)", observed_remaining_s=remaining,
                    expected_remaining_s=0, absolute_tolerance_s=0.11)
    assert remaining == pytest.approx(0, abs=0.11)


@pytest.mark.strictdoc("TC-H1FWMC-106")
def test_setup_waits_for_voltage_and_ramp_completion(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-19, scope=function, role=Verifies)
    UID: TC-H1FWMC-106
    TITLE: Verify target voltage before emission
    STATEMENT: Setup waits for target voltage within 5% and for ramp completion.
    PREREQUISITES: Healthy system and a 50 kV scalar plan.
    STEPS: Hold feedback at 45 kV, then 50 kV with ramping reported, then report ramp complete.
    EXPECTED_BEHAVIOR: Both blocking conditions retain non-emitting Setup for 1.5 seconds each;
      Ready reports 50 kV within 5%, and remains non-emitting until release.
    """
    session = _stage(client, OperationalPoint.beam_qa(50), evidence)
    hvps.set_feedback(kv=0)
    client.release_plan(session)
    client.wait_for_state(State.SETUP)
    hvps.set_feedback(kv=45)
    _hold_nonemitting(client, io_model, State.SETUP, 1.5, evidence)
    hvps.set_feedback(kv=50, flag_bits=1 << 4)
    ramping = client.wait_for_telemetry(
        lambda item: item.kv_feedback == 50 and bool(item.hvps_flags & (1 << 4)),
    )
    _record_telemetry(evidence, "Target voltage while ramp remains active", ramping,
                      expected_ramping_mask=1 << 4,
                      expected_kv_feedback=50, expected_state=State.SETUP)
    _hold_nonemitting(client, io_model, State.SETUP, 1.5, evidence)
    hvps.set_feedback(kv=None, flag_bits=None)
    ready = client.wait_for_state(State.READY)
    _record_telemetry(evidence, "Ramp completed and target verified", ready,
                      expected_state=State.READY, expected_kv_feedback=50, relative_tolerance=0.05)
    assert ready.kv_feedback == pytest.approx(50, rel=0.05)
    _hold_nonemitting(client, io_model, State.READY, 0.5, evidence)
    client.start_emission(session)
    _finish(client, io_model, 5, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-107")
def test_stopped_scalar_emission_resumes_same_voltage_and_remaining_time(
    client: MainControlClient, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-20, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-85, scope=function, role=Verifies)
    UID: TC-H1FWMC-107
    TITLE: Resume the same scalar emission
    STATEMENT: Resuming retains undelivered time and reestablishes the same voltage before release.
    PREREQUISITES: Healthy system and an 8-second, 70 kV, 1 mA emission.
    STEPS: Emit at least 0.5 seconds, Stop, reset timers, warm, reconfirm the retained point,
      release the same plan, wait in Ready, then send its new point release.
    EXPECTED_BEHAVIOR: Remaining time is unchanged during preparation; Ready voltage is within 5%;
      no automatic restart occurs, and completion discharges to Cold.
    """
    point = OperationalPoint.beam_qa(70, 8)
    session = client.prepare_emission(point)
    client.start_emission(session)
    active = client.wait_for_telemetry(lambda item: item.internal_timer_s >= 0.5)
    _record_telemetry(evidence, "Delivery before resumable stop", active,
                      expected_minimum_delivery_s=0.5, expected_kv_target=point.kv)
    client.directive(Directive.STOP)
    _finish(client, io_model, 5, evidence)
    retained = client.query_operational_point()
    evidence.record("Stopped point retained for resume", observed_point=retained,
                    expected_remaining_exclusive_bounds_s=(0, point.total_time_s),
                    expected_kv=point.kv)
    assert 0 < retained.remaining_time_s < point.total_time_s
    assert retained.kv == point.kv
    client.directive(Directive.RESET_TIMERS)
    reset = client.wait_for_telemetry(
        lambda item: item.internal_timer_s == item.timer_1_s == item.timer_2_s == 0,
    )
    evidence.record("Resume timer reset (s)",
                    observed_timers=(reset.internal_timer_s, reset.timer_1_s, reset.timer_2_s),
                    expected_timers=(0, 0, 0))
    client.warmup()
    client.wait_for_state(State.STAGED)
    staged_point = client.query_operational_point()
    evidence.record("Resume staging retains point", observed_point=staged_point, expected_point=retained)
    assert staged_point == retained
    client.confirm_operational_point(session, retained)
    client.release_plan(session)
    ready = client.wait_for_state(State.READY)
    _record_telemetry(evidence, "Resume reestablishes original voltage", ready,
                      expected_kv_target=point.kv, expected_kv_feedback=point.kv,
                      relative_tolerance=0.05, expected_state=State.READY)
    assert ready.kv_setpoint == point.kv
    assert ready.kv_feedback == pytest.approx(point.kv, rel=0.05)
    _hold_nonemitting(client, io_model, State.READY, 0.5, evidence)
    ready_point = client.query_operational_point()
    evidence.record("Ready hold retains resumable time", observed_point=ready_point, expected_point=retained)
    assert ready_point == retained
    client.start_emission(session)
    final = _finish(client, io_model, retained.remaining_time_s + 5, evidence)
    evidence.record("Resumed delivery timer (s)", observed_internal_timer_s=final.internal_timer_s,
                    expected_internal_timer_s=retained.remaining_time_s, absolute_tolerance_s=0.15)
    assert final.internal_timer_s == pytest.approx(retained.remaining_time_s, abs=0.15)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Resumed point completes (s)", observed_remaining_s=remaining,
                    expected_remaining_s=0, absolute_tolerance_s=0.11)
    assert remaining == pytest.approx(0, abs=0.11)


@pytest.mark.strictdoc("TC-H1FWMC-108")
def test_completed_scalar_session_returns_cold(
    client: MainControlClient, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-85, scope=function, role=Verifies)
    UID: TC-H1FWMC-108
    TITLE: Completed session returns through Discharge to Cold
    STATEMENT: A completed scalar treatment ends the session rather than staging another emission.
    PREREQUISITES: Healthy system and a 3-second, 50 kV emission.
    STEPS: Prepare, release, observe emission, and monitor the end state for 1 second.
    EXPECTED_BEHAVIOR: Delivered time reaches 3 seconds, remaining time reaches zero,
      Discharge reaches Cold, and radiation remains disabled.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, 3))
    client.start_emission(session)
    final = _finish(client, io_model, 6, evidence)
    evidence.record("Completed session delivery timer (s)", observed_internal_timer_s=final.internal_timer_s,
                    expected_internal_timer_s=3, absolute_tolerance_s=0.15)
    assert final.internal_timer_s == pytest.approx(3, abs=0.15)
    remaining = client.query_operational_point().remaining_time_s
    evidence.record("Completed session remaining time (s)", observed_remaining_s=remaining,
                    expected_remaining_s=0, absolute_tolerance_s=0.11)
    assert remaining == pytest.approx(0, abs=0.11)
    _hold_nonemitting(client, io_model, State.COLD, 1, evidence)


@pytest.mark.strictdoc("TCH1FWMC-128")
def test_cathode_feedback_fault_leaves_incomplete_emission(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-75, scope=function, role=Verifies)
    UID: TCH1FWMC-128
    TITLE: Cathode current feedback monitoring
    STATEMENT: Cathode feedback outside 5% of target faults rather than completing treatment.
    PREREQUISITES: Healthy system and a 25-second, 50 kV, 1 mA emission.
    STEPS: Emit with 0.96 mA feedback for 7 seconds, then clamp feedback to 0.1 mA.
    EXPECTED_BEHAVIOR: The 4% deviation is accepted; 0.1 mA reports a current fault,
      disables radiation and retains undelivered time in the point response.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, 25))
    client.start_emission(session)
    hvps.set_feedback(ma=0.96)
    telemetry = client.wait_for_telemetry(lambda item: abs(item.ma_feedback - 0.96) < 0.001)
    started = time.monotonic()
    deadline = started + 7
    samples = 0
    try:
        while time.monotonic() < deadline:
            telemetry = client.query_telemetry()
            samples += 1
            assert telemetry.state == State.EMISSION and telemetry.faults == Fault.NONE
            time.sleep(0.05)
    finally:
        _record_telemetry(evidence, "Accepted cathode current deviation hold", telemetry,
                          observed_elapsed_s=time.monotonic() - started, observed_samples=samples,
                          expected_hold_s=7, expected_state=State.EMISSION, expected_faults=Fault.NONE,
                          imposed_ma_feedback=0.96, expected_ma_target=1, relative_tolerance=0.05)
    hvps.set_feedback(ma=0.1)
    evidence.record("Rejected cathode feedback input (mA)", imposed_ma_feedback=0.1,
                    expected_ma_target=1, relative_tolerance=0.05)
    _wait_fault(client, io_model, Fault.MA, 9, evidence)
    result = client.query_operational_point()
    evidence.record("Cathode fault leaves incomplete delivery (s)",
                    observed_remaining_s=result.remaining_time_s,
                    expected_remaining_exclusive_bounds_s=(0, result.total_time_s))
    assert 0 < result.remaining_time_s < result.total_time_s
    record_deadline = time.monotonic() + 2
    record = None
    observed_types = []
    try:
        while True:
            record = client.fault_message(timeout=max(0.01, record_deadline - time.monotonic()))
            observed_types.append(record.fault_type)
            if record.fault_type == Fault.MA.bit_length() - 1:
                assert record.state == State.EMISSION
                break
            assert time.monotonic() < record_deadline, "No cathode-current fault record"
    finally:
        evidence.record("Cathode fault event", observed_fault_types=observed_types,
                        observed_fault_state=record.state if record is not None else None,
                        expected_fault_type=Fault.MA.bit_length() - 1,
                        expected_fault_state=State.EMISSION, timeout_s=2)
