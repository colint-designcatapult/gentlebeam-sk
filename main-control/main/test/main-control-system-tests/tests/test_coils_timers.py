"""Coil outputs, current monitoring, and backup timers through real host transports."""

from __future__ import annotations

from dataclasses import replace
import time

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Fault,
    FaultMessage,
    OperationalPoint,
    State,
    word_float,
)
from main_control_system_tests.protocol import MainControlClient


def _fault_record(client: MainControlClient, fault: Fault, evidence: Evidence) -> FaultMessage:
    first = FaultMessage.decode(client.command(2, 0))
    records = [first]
    records.extend(
        FaultMessage.decode(client.command(2, index))
        for index in range(1, first.active_count)
    )
    matching = [record for record in records if (1 << record.fault_type) == fault]
    evidence.record("Coil/timer fault diagnostics", expected_fault=fault,
                    observed_categories=[1 << record.fault_type for record in records],
                    observed_matching_count=len(matching), expected_minimum_matching_count=1)
    assert matching, f"No {fault.name} diagnostic in {[record.message for record in records]}"
    return matching[0]


def _assert_healthy_emission(client: MainControlClient, current_ma: float, evidence: Evidence) -> None:
    snapshot = client.wait_for_telemetry(
        lambda item: abs(item.focus_coil_current - current_ma) < 0.5,
        timeout=1,
    )
    until = snapshot.runtime_ms + 700
    deadline = time.monotonic() + 2
    first_runtime_ms = snapshot.runtime_ms
    first_current_ma = snapshot.focus_coil_current
    try:
        while True:
            assert snapshot.state == State.EMISSION, snapshot
            assert not snapshot.faults, snapshot
            assert snapshot.focus_coil_current == pytest.approx(current_ma, abs=0.5)
            if snapshot.runtime_ms >= until:
                return
            assert time.monotonic() < deadline, "Firmware did not sustain healthy emission for 0.7 seconds"
            time.sleep(0.025)
            snapshot = client.query_telemetry()
    finally:
        evidence.record("Focus feedback healthy boundary hold (mA, ms)",
                        observed_first_ma=first_current_ma, observed_last_ma=snapshot.focus_coil_current,
                        expected_feedback_ma=current_ma, target_ma=500, accepted_error_ma=150, tolerance_ma=0.5,
                        observed_state=snapshot.state, expected_state=State.EMISSION,
                        observed_faults=snapshot.faults, expected_faults=Fault(0),
                        observed_duration_ms=snapshot.runtime_ms - first_runtime_ms, expected_minimum_duration_ms=700)


@pytest.mark.strictdoc("TC-H1FWMC-95")
@pytest.mark.parametrize("feedback_ma", (0.0, 349.0, 651.0), ids=("disconnected", "low", "high"))
def test_focus_coil_feedback_monitoring(
    client: MainControlClient, io_model: HostIOModel, feedback_ma: float, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-9, scope=function, role=Verifies)
    UID: TC-H1FWMC-95
    TITLE: Coils - Focus Coil Feedback Monitoring - Test Case

    STATEMENT: Focus-current errors greater than 150 mA report a coil fault.

    PREREQUISITES: Healthy system, a 500 mA focus-current plan, adjustable
    physical focus feedback, PC emission commands, telemetry, and fault log.

    STEPS:
    1. Prepare a 20-second emission with a 500 mA focus target.
    2. For disconnection, apply 0 mA before starting emission. Otherwise start
       emission with feedback 149 mA below or above target and hold for 0.7 seconds.
    3. For the tolerance checks, increase the deviation to 151 mA.
    4. Check current telemetry and the coil diagnostic, then restore feedback
       and clear faults through PC commands.

    EXPECTED_BEHAVIOR: A 149 mA error sustains emission without faults.
    Disconnection and either 151 mA error report a coil-current fault within
    1.5 seconds of emission or injection; the log identifies the measured
    current, 500 mA target, and 150 mA tolerance. Recovery clears faults.
    """
    point = replace(OperationalPoint.beam_qa(50, duration_s=20), focus_coil_ma=500.0)
    session = client.prepare_emission(point)
    ready = client.query_telemetry()
    evidence.record("Focus monitoring initial state", observed_state=ready.state,
                    observed_faults=ready.faults, expected_faults=Fault(0), target_ma=500)
    assert not ready.faults
    try:
        valid_ma = 351.0 if feedback_ma < 500 else 649.0
        io_model.set_focus_current_ma(0.0 if feedback_ma == 0 else valid_ma)
        started = client.start_emission(session)
        evidence.record("Focus monitoring emission start", observed_state=started.state,
                        expected_state=State.EMISSION, applied_feedback_ma=0.0 if feedback_ma == 0 else valid_ma)
        assert started.state == State.EMISSION
        if feedback_ma:
            _assert_healthy_emission(client, valid_ma, evidence)
            started = client.query_telemetry()
            io_model.set_focus_current_ma(feedback_ma)
        faulted = client.wait_for_telemetry(
            lambda item: bool(item.faults & Fault.COIL_CURRENT), timeout=1.5
        )
        evidence.record("Focus feedback fault (mA)", observed_current_ma=faulted.focus_coil_current,
                        expected_feedback_ma=feedback_ma, target_ma=500, accepted_error_ma=150,
                        tolerance_ma=0.5, observed_faults=faulted.faults, expected_fault=Fault.COIL_CURRENT,
                        observed_state=faulted.state, timeout_s=1.5)
        assert faulted.focus_coil_current == pytest.approx(feedback_ma, abs=0.5)
        record = _fault_record(client, Fault.COIL_CURRENT, evidence)
        evidence.record("Focus fault diagnostic (mA, ms)", observed_state=record.state, expected_state=State.EMISSION,
                        observed_arguments_ma=tuple(map(word_float, record.arguments)),
                        expected_arguments_ma=(feedback_ma, 500.0, 150.0), tolerance_ma=0.5,
                        observed_delay_ms=record.runtime_ms - started.runtime_ms, expected_delay_bounds_ms=(450, 800))
        assert record.state == State.EMISSION
        assert tuple(map(word_float, record.arguments)) == pytest.approx(
            (feedback_ma, 500.0, 150.0), abs=0.5
        )
        # Five 10 ms monitoring rounds, fault on the eleventh failure, plus
        # the 16-sample ADC average. Allow command/snapshot phase at the edges.
        assert 450 <= record.runtime_ms - started.runtime_ms <= 800, record.message
    finally:
        io_model.restore_coil_feedback()
        client.enter_cold()
        client.clear_faults()
        recovered = client.wait_for_telemetry(lambda item: not item.faults)
        evidence.record("Focus feedback recovery", observed_faults=recovered.faults, expected_faults=Fault(0))


@pytest.mark.strictdoc("TC-H1FWMC-96")
@pytest.mark.parametrize("timer", (1, 2), ids=("primary", "secondary"))
def test_backup_timer_communication_integrity(
    client: MainControlClient, io_model: HostIOModel, timer: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-10, scope=function, role=Verifies)
    UID: TC-H1FWMC-96
    TITLE: Backup Timer Communication Integrity - Test Case

    STATEMENT: Corrupted communication from either backup timer reports a fault.

    PREREQUISITES: Healthy system, independently corruptible timer responses,
    PC telemetry, and readable fault diagnostics.

    STEPS:
    1. Verify healthy timer communication after startup.
    2. Corrupt responses from one backup timer without stopping communication.
    3. Observe the timer-communication fault and integrity diagnostic.
    4. Restore valid responses and clear faults; repeat for the other timer.

    EXPECTED_BEHAVIOR: Corruption reports a timer-communication fault and an
    integrity diagnostic within 1 second. Restored valid communication allows
    faults to clear and remain clear for at least 1 second.
    """
    healthy = client.query_telemetry()
    evidence.record("Backup timer healthy communication", timer=timer,
                    observed_faults=healthy.faults, expected_faults=Fault(0))
    assert not healthy.faults
    try:
        io_model.set_timer_fault(timer, checksum_corrupted=True)
        faulted = client.wait_for_telemetry(lambda item: bool(item.faults & Fault.TIMER_COMM), timeout=1)
        evidence.record("Corrupted backup timer communication", timer=timer, injected_checksum_corruption=True,
                        observed_faults=faulted.faults, expected_fault=Fault.TIMER_COMM, timeout_s=1)
        record = _fault_record(client, Fault.TIMER_COMM, evidence)
        evidence.record("Timer integrity diagnostic", timer=timer, observed_arguments=record.arguments,
                        expected_argument_count=2, excluded_first_argument=0xFF)
        assert len(record.arguments) == 2, record.message
        assert record.arguments[0] != 0xFF, record.message
    finally:
        io_model.set_timer_fault(timer)
        client.enter_cold()
        client.clear_faults()
        _assert_timer_recovered(client, evidence)


def _assert_timer_recovered(client: MainControlClient, evidence: Evidence) -> None:
    snapshot = client.wait_for_telemetry(lambda item: not item.faults)
    until = snapshot.runtime_ms + 1000
    deadline = time.monotonic() + 3
    first_runtime_ms = snapshot.runtime_ms
    try:
        while snapshot.runtime_ms < until:
            assert time.monotonic() < deadline, "Firmware did not remain healthy for 1 second"
            time.sleep(0.025)
            snapshot = client.query_telemetry()
            assert not snapshot.faults, snapshot
    finally:
        evidence.record("Timer communication recovery hold (ms)", observed_faults=snapshot.faults,
                        expected_faults=Fault(0), observed_duration_ms=snapshot.runtime_ms - first_runtime_ms,
                        expected_minimum_duration_ms=1000)
    report = FaultMessage.decode(client.command(2, 0))
    evidence.record("Timer recovery diagnostics", observed_active_count=report.active_count, expected_active_count=0)
    assert report.active_count == 0


@pytest.mark.strictdoc("TC-H1FWMC-97")
@pytest.mark.parametrize("timer", (1, 2), ids=("primary", "secondary"))
def test_backup_timer_response_monitoring(
    client: MainControlClient, io_model: HostIOModel, timer: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-11, scope=function, role=Verifies)
    UID: TC-H1FWMC-97
    TITLE: Backup Timer Response Monitoring - Test Case

    STATEMENT: A nonresponding backup timer reports a communication fault.

    PREREQUISITES: Healthy system, a confirmed emission plan, independently
    suppressible backup-timer responses, PC commands, telemetry, and fault log.

    STEPS:
    1. Prepare a 20-second emission and verify all elapsed timers start at zero.
    2. Start emission and verify both backup timers advance.
    3. Stop responses from one timer and observe the fault and timeout diagnostic.
    4. Restore responses and clear faults; repeat for the other timer.

    EXPECTED_BEHAVIOR: Missing responses report a timer-communication fault
    after 0.4 to 0.8 seconds nominally, within 1.5 seconds including observation.
    The diagnostic identifies the configured 0.4-second response interval.
    Restoring responses permits fault clearing and at least 1 second without
    recurrence.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=20))
    ready = client.query_telemetry()
    evidence.record("Ready before timer response loss (s)", timer=timer,
                    observed_state=ready.state, expected_state=State.READY,
                    observed_timers_s=(ready.internal_timer_s, ready.timer_1_s, ready.timer_2_s),
                    expected_timers_s=(0, 0, 0))
    assert ready.state == State.READY
    assert ready.internal_timer_s == ready.timer_1_s == ready.timer_2_s == 0
    client.start_emission(session)
    before = client.wait_for_telemetry(
        lambda item: item.timer_1_s > 0.1 and item.timer_2_s > 0.1,
        timeout=1,
    )
    evidence.record("Timers advancing before response loss (s)", timer=timer,
                    observed_timers_s=(before.timer_1_s, before.timer_2_s), expected_minimum_exclusive_s=0.1,
                    observed_faults=before.faults, expected_faults=Fault(0))
    assert not before.faults
    try:
        io_model.set_timer_fault(timer, response_suppressed=True)
        faulted = client.wait_for_telemetry(lambda item: bool(item.faults & Fault.TIMER_COMM), timeout=1.5)
        evidence.record("Suppressed timer response fault", timer=timer, injected_response_suppression=True,
                        observed_faults=faulted.faults, expected_fault=Fault.TIMER_COMM, timeout_s=1.5)
        record = _fault_record(client, Fault.TIMER_COMM, evidence)
        evidence.record("Timer response timeout diagnostic (ms)", timer=timer,
                        observed_arguments_ms=record.arguments, expected_arguments_ms=(400,),
                        observed_delay_ms=record.runtime_ms - before.runtime_ms, expected_delay_bounds_ms=(400, 1000))
        assert record.arguments == (400,), record.message
        # The repeating 400 ms check first sets timer_bus_stuck and only the
        # next check faults. These bounds include the input application phase.
        assert 400 <= record.runtime_ms - before.runtime_ms <= 1000, record.message
    finally:
        io_model.set_timer_fault(timer)
        client.enter_cold()
        client.clear_faults()
        _assert_timer_recovered(client, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-146")
def test_coil_setpoint_control(client: MainControlClient, io_model: HostIOModel, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-55, scope=function, role=Verifies)
    UID: TC-H1FWMC-146
    TITLE: Coils - Setpoint Control - Test Case

    STATEMENT: PC coil setpoints drive the physical deflection and focus outputs.

    PREREQUISITES: Healthy system with connected coils, observable DAC voltages
    and direction pins, and current telemetry.

    STEPS:
    1. Load a 20-second plan with X=300 mA, Y=-400 mA, and focus=500 mA.
    2. Check output voltages, deflection directions, and measured coil currents.
    3. Run emission for 0.7 seconds and return to COLD.
    4. Repeat with X=-600 mA, Y=200 mA, and focus=800 mA.

    EXPECTED_BEHAVIOR: Each accepted plan produces 2.5 V/A deflection and
    1.666 V/A focus outputs, correct deflection polarity, and corresponding
    measured currents. Emission remains fault-free and COLD disables outputs.
    """
    try:
        for x_ma, y_ma, focus_ma in ((300.0, -400.0, 500.0), (-600.0, 200.0, 800.0)):
            point = replace(
                OperationalPoint.beam_qa(50, duration_s=20),
                x_coil_ma=x_ma, y_coil_ma=y_ma, focus_coil_ma=focus_ma,
            )
            session = client.prepare_emission(point)
            expected_volts = {
                "x": abs(x_ma) * 2.5 / 1000,
                "y": abs(y_ma) * 2.5 / 1000,
                "f": focus_ma * 1.666 / 1000,
            }
            model = io_model.wait_for(lambda item: all(
                abs(item["dac"]["coil"][axis] - voltage) < 0.002
                for axis, voltage in expected_volts.items()
            ))
            pins = model["gpio"]["pins"]["port_b"]
            evidence.record("Plan coil output voltages and direction (V, mA)",
                            expected_currents_ma=(x_ma, y_ma, focus_ma),
                            observed_volts={axis: model["dac"]["coil"][axis] for axis in expected_volts},
                            expected_volts=expected_volts, tolerance_volts=0.002,
                            observed_x_direction=pins["io_coil_x_dir_n"], expected_x_direction=x_ma >= 0,
                            observed_y_direction=pins["io_coil_y_dir_n"], expected_y_direction=y_ma >= 0)
            assert pins["io_coil_x_dir_n"] is (x_ma >= 0)
            assert pins["io_coil_y_dir_n"] is (y_ma >= 0)
            started = client.start_emission(session)
            snapshot = client.wait_for_telemetry(
                lambda item: abs(item.x_coil_current - x_ma) < 1
                and abs(item.y_coil_current - y_ma) < 1
                and abs(item.focus_coil_current - focus_ma) < 1,
                timeout=1,
            )
            deadline = time.monotonic() + 2
            first_currents_ma = (snapshot.x_coil_current, snapshot.y_coil_current, snapshot.focus_coil_current)
            try:
                while True:
                    assert snapshot.state == State.EMISSION
                    assert not snapshot.faults, snapshot
                    assert (snapshot.x_coil_current, snapshot.y_coil_current,
                            snapshot.focus_coil_current) == pytest.approx(
                        (x_ma, y_ma, focus_ma), abs=1,
                    )
                    if snapshot.runtime_ms - started.runtime_ms >= 700:
                        break
                    assert time.monotonic() < deadline, "Emission did not advance 0.7 seconds"
                    time.sleep(0.025)
                    snapshot = client.query_telemetry()
            finally:
                evidence.record("Plan coil setpoint emission hold (mA, ms)",
                                observed_first_currents_ma=first_currents_ma,
                                observed_x_ma=snapshot.x_coil_current, expected_x_ma=x_ma,
                                observed_y_ma=snapshot.y_coil_current, expected_y_ma=y_ma,
                                observed_focus_ma=snapshot.focus_coil_current, expected_focus_ma=focus_ma,
                                tolerance_ma=1, observed_state=snapshot.state, expected_state=State.EMISSION,
                                observed_faults=snapshot.faults, expected_faults=Fault(0),
                                observed_duration_ms=snapshot.runtime_ms - started.runtime_ms,
                                expected_minimum_duration_ms=700)
            client.enter_cold()
            cold_model = io_model.wait_for(lambda item: all(
                item["dac"]["coil"][axis] == 0 for axis in ("x", "y", "f")
            ))
            evidence.record("Cold disables coil outputs (V)",
                            observed_volts={axis: cold_model["dac"]["coil"][axis] for axis in ("x", "y", "f")},
                            expected_volts={"x": 0, "y": 0, "f": 0})
    finally:
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-147")
@pytest.mark.parametrize("axis", ("x", "y"))
@pytest.mark.parametrize("feedback_ma", (0.0, 449.0, 551.0), ids=("disconnected", "low", "high"))
def test_deflection_coil_feedback_monitoring(
    client: MainControlClient, io_model: HostIOModel, axis: str, feedback_ma: float,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-54, scope=function, role=Verifies)
    UID: TC-H1FWMC-147
    TITLE: Coils - Deflection Coil Feedback Monitoring - Test Case

    STATEMENT: Each deflection coil reports current errors greater than 50 mA.

    PREREQUISITES: Healthy system, a 500 mA target on each deflection axis,
    independently adjustable physical current feedback, and PC fault records.

    STEPS:
    1. Prepare a 20-second emission and disconnect one coil's current feedback,
       or hold feedback 49 mA below or above its target.
    2. Start emission; for tolerance checks sustain the 49 mA error for
       0.7 seconds before increasing it to 51 mA.
    3. Read current telemetry and the fault record, then restore feedback.

    EXPECTED_BEHAVIOR: A 49 mA error sustains fault-free emission. Disconnection
    and either 51 mA error report a coil-current fault within 1.5 seconds.
    The record identifies the measured current, 500 mA target, and 50 mA tolerance.
    """
    point = replace(
        OperationalPoint.beam_qa(50, duration_s=20), x_coil_ma=500.0, y_coil_ma=500.0,
    )
    session = client.prepare_emission(point)
    attribute = f"{axis}_coil_current"
    try:
        valid_ma = 451.0 if feedback_ma < 500 else 549.0
        io_model.set_deflection_current_ma(axis, 0.0 if feedback_ma == 0 else valid_ma)
        started = client.start_emission(session)
        evidence.record("Deflection monitoring emission start", axis=axis,
                        applied_feedback_ma=0.0 if feedback_ma == 0 else valid_ma,
                        observed_state=started.state, expected_state=State.EMISSION)
        assert started.state == State.EMISSION
        if feedback_ma:
            snapshot = client.wait_for_telemetry(
                lambda item: abs(getattr(item, attribute) - valid_ma) < 1, timeout=1,
            )
            until = snapshot.runtime_ms + 700
            deadline = time.monotonic() + 2
            first_runtime_ms = snapshot.runtime_ms
            first_current_ma = getattr(snapshot, attribute)
            try:
                while True:
                    assert snapshot.state == State.EMISSION
                    assert not snapshot.faults, snapshot
                    assert getattr(snapshot, attribute) == pytest.approx(valid_ma, abs=1)
                    if snapshot.runtime_ms >= until:
                        break
                    assert time.monotonic() < deadline, "Emission did not sustain 0.7 seconds"
                    time.sleep(0.025)
                    snapshot = client.query_telemetry()
            finally:
                evidence.record("Deflection healthy boundary hold (mA, ms)", axis=axis,
                                observed_first_ma=first_current_ma, observed_last_ma=getattr(snapshot, attribute),
                                expected_feedback_ma=valid_ma, target_ma=500, accepted_error_ma=50, tolerance_ma=1,
                                observed_state=snapshot.state, expected_state=State.EMISSION,
                                observed_faults=snapshot.faults, expected_faults=Fault(0),
                                observed_duration_ms=snapshot.runtime_ms - first_runtime_ms, expected_minimum_duration_ms=700)
            io_model.set_deflection_current_ma(axis, feedback_ma)
        faulted = client.wait_for_telemetry(
            lambda item: bool(item.faults & Fault.COIL_CURRENT), timeout=1.5,
        )
        evidence.record("Deflection feedback fault (mA)", axis=axis, observed_current_ma=getattr(faulted, attribute),
                        expected_feedback_ma=feedback_ma, target_ma=500, accepted_error_ma=50, tolerance_ma=1,
                        observed_faults=faulted.faults, expected_fault=Fault.COIL_CURRENT, timeout_s=1.5)
        assert getattr(faulted, attribute) == pytest.approx(feedback_ma, abs=1)
        record = _fault_record(client, Fault.COIL_CURRENT, evidence)
        evidence.record("Deflection fault diagnostic (mA)", axis=axis,
                        observed_state=record.state, expected_state=State.EMISSION,
                        observed_arguments_ma=tuple(map(word_float, record.arguments)),
                        expected_arguments_ma=(feedback_ma, 500.0, 50.0), tolerance_ma=1)
        assert record.state == State.EMISSION
        assert tuple(map(word_float, record.arguments)) == pytest.approx(
            (feedback_ma, 500.0, 50.0), abs=1,
        )
    finally:
        io_model.restore_coil_feedback()
        client.enter_cold()
        client.clear_faults()
        recovered = client.wait_for_telemetry(lambda item: not item.faults)
        evidence.record("Deflection feedback recovery", axis=axis,
                        observed_faults=recovered.faults, expected_faults=Fault(0))


@pytest.mark.strictdoc("TC-H1FWMC-148")
@pytest.mark.parametrize("duration_s", (8.0, 13.5))
def test_backup_timer_initialization(
    client: MainControlClient, io_model: HostIOModel, duration_s: float,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-53, scope=function, role=Verifies)
    UID: TC-H1FWMC-148
    TITLE: Backup Timer - Initialization - Test Case

    STATEMENT: A PC plan initializes both backup timer ICs before emission.

    PREREQUISITES: Healthy system with no loaded plan and observable backup
    timer hardware, PC commands, and telemetry.

    STEPS:
    1. Confirm both timers are cleared before loading a plan.
    2. Prepare an 8-second or 13.5-second emission and inspect both timers.
    3. Leave the system READY for 0.3 seconds, then start emission.

    EXPECTED_BEHAVIOR: Both timers initialize paused at the plan duration plus
    the 0.5-second safety margin, remain unchanged in READY, and begin counting
    down only when emission starts. The system remains fault-free.
    """
    initial = io_model.read()
    evidence.record("Backup timers initially cleared",
                    observed_states=[initial[f"backup_timer{timer}"]["state"] for timer in (1, 2)],
                    expected_states=[0, 0])
    assert all(initial[f"backup_timer{timer}"]["state"] == 0 for timer in (1, 2))
    try:
        session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=duration_s))
        ready = client.query_telemetry()
        evidence.record("Timer initialization Ready", plan_duration_s=duration_s,
                        observed_state=ready.state, expected_state=State.READY,
                        observed_faults=ready.faults, expected_faults=Fault(0))
        assert ready.state == State.READY
        assert not ready.faults
        model = io_model.wait_for(lambda item: all(
            item[f"backup_timer{timer}"]["state"] == 1 for timer in (1, 2)
        ))
        for timer in (1, 2):
            remaining = io_model.backup_timer_seconds(timer, model)
            evidence.record("Initialized backup timer (s)", timer=timer, plan_duration_s=duration_s,
                            observed_state=model[f"backup_timer{timer}"]["state"], expected_state=1,
                            observed_remaining_s=remaining, expected_remaining_s=duration_s + 0.5, tolerance_s=0.001)
            assert remaining == pytest.approx(duration_s + 0.5, abs=0.001)
        held = client.wait_for_telemetry(
            lambda item: item.runtime_ms >= ready.runtime_ms + 300, timeout=1,
        )
        evidence.record("Ready timer hold (ms)", observed_state=held.state, expected_state=State.READY,
                        observed_faults=held.faults, expected_faults=Fault(0),
                        observed_duration_ms=held.runtime_ms - ready.runtime_ms, expected_minimum_duration_ms=300)
        assert held.state == State.READY
        assert not held.faults
        paused = io_model.read()
        for timer in (1, 2):
            remaining = io_model.backup_timer_seconds(timer, paused)
            evidence.record("Held backup timer paused (s)", timer=timer,
                            observed_state=paused[f"backup_timer{timer}"]["state"], expected_state=1,
                            observed_remaining_s=remaining, expected_remaining_s=duration_s + 0.5, tolerance_s=0.001)
            assert paused[f"backup_timer{timer}"]["state"] == 1
            assert remaining == pytest.approx(duration_s + 0.5, abs=0.001)
        started = client.start_emission(session)
        active = client.wait_for_telemetry(
            lambda item: item.runtime_ms >= started.runtime_ms + 300, timeout=1,
        )
        evidence.record("Active initialized timers (ms)", observed_state=active.state, expected_state=State.EMISSION,
                        observed_faults=active.faults, expected_faults=Fault(0),
                        observed_duration_ms=active.runtime_ms - started.runtime_ms, expected_minimum_duration_ms=300)
        assert active.state == State.EMISSION
        assert not active.faults
        running = io_model.read()
        for timer in (1, 2):
            remaining = io_model.backup_timer_seconds(timer, running)
            evidence.record("Running initialized backup timer (s)", timer=timer,
                            observed_state=running[f"backup_timer{timer}"]["state"], expected_state=2,
                            observed_remaining_s=remaining, expected_exclusive_bounds_s=(0, duration_s + 0.3))
            assert running[f"backup_timer{timer}"]["state"] == 2
            assert 0 < remaining < duration_s + 0.3
    finally:
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-149")
@pytest.mark.parametrize("timer", (1, 2), ids=("primary", "secondary"))
def test_backup_timer_countdown_reporting(
    client: MainControlClient, io_model: HostIOModel, timer: int,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-52, scope=function, role=Verifies)
    UID: TC-H1FWMC-149
    TITLE: Backup Timer - Countdown Reporting - Test Case

    STATEMENT: Elapsed backup-timer telemetry reconstructs the physical countdown.

    PREREQUISITES: Healthy system, a 10-second plan, observable physical backup
    timers, and PC telemetry.

    STEPS:
    1. Start emission and observe physical remaining time and elapsed PC telemetry.
    2. Repeat at least 0.8 seconds later while emission continues.
    3. Subtract each reported elapsed time from the 10.5-second initial backup
       duration and compare the result to the physical countdown.

    EXPECTED_BEHAVIOR: Physical remaining time decreases while reported elapsed
    time increases with real time. Reconstructed remaining time agrees with the
    physical countdown within 0.1 seconds and decreases between readings.
    """
    try:
        point = OperationalPoint.beam_qa(50, duration_s=10)
        initial_duration_s = point.remaining_time_s + 0.5
        session = client.prepare_emission(point)
        started = client.start_emission(session)
        first = client.wait_for_telemetry(
            lambda item: item.runtime_ms >= started.runtime_ms + 300, timeout=1,
        )
        first_remaining = io_model.backup_timer_seconds(timer)
        second = client.wait_for_telemetry(
            lambda item: item.runtime_ms >= first.runtime_ms + 800, timeout=2,
        )
        second_remaining = io_model.backup_timer_seconds(timer)
        evidence.record("Physical backup countdown samples (s)", timer=timer,
                        observed_first_remaining_s=first_remaining, observed_second_remaining_s=second_remaining,
                        initial_duration_s=initial_duration_s,
                        expected_relation="0 < second < first < initial_duration_s")
        for snapshot in (first, second):
            evidence.record("Countdown telemetry state", timer=timer, observed_runtime_ms=snapshot.runtime_ms,
                            observed_state=snapshot.state, expected_state=State.EMISSION,
                            observed_faults=snapshot.faults, expected_faults=Fault(0),
                            observed_timer_state=getattr(snapshot, f"timer_{timer}_state"), expected_timer_state=2)
            assert snapshot.state == State.EMISSION
            assert not snapshot.faults
            assert getattr(snapshot, f"timer_{timer}_state") == 2
        assert 0 < second_remaining < first_remaining < initial_duration_s
        elapsed_s = (second.runtime_ms - first.runtime_ms) / 1000
        evidence.record("Physical backup countdown progress (s)", timer=timer,
                        observed_delta_s=first_remaining - second_remaining, expected_elapsed_s=elapsed_s, tolerance_s=0.1)
        assert first_remaining - second_remaining == pytest.approx(elapsed_s, abs=0.1)
        # Telemetry is elapsed time; reconstruct the hardware countdown,
        # including the backup timer's 0.5-second initialization margin.
        reported_first = getattr(first, f"timer_{timer}_s")
        reported_second = getattr(second, f"timer_{timer}_s")
        reconstructed_first = initial_duration_s - reported_first
        reconstructed_second = initial_duration_s - reported_second
        evidence.record(
            "Elapsed telemetry converted to physical remaining time (s)",
            timer=timer, initial_duration_s=initial_duration_s,
            observed_reported_first_elapsed_s=reported_first,
            observed_reported_second_elapsed_s=reported_second,
            observed_first_remaining_s=reconstructed_first,
            expected_first_remaining_s=first_remaining,
            observed_second_remaining_s=reconstructed_second,
            expected_second_remaining_s=second_remaining,
            observed_elapsed_delta_s=reported_second - reported_first,
            expected_elapsed_delta_s=elapsed_s,
            tolerance_s=0.1,
            expected_relation="elapsed increases; remaining decreases",
        )
        assert 0 < reported_first < reported_second < initial_duration_s
        assert reported_second - reported_first == pytest.approx(elapsed_s, abs=0.1)
        assert reconstructed_first == pytest.approx(first_remaining, abs=0.1)
        assert reconstructed_second == pytest.approx(second_remaining, abs=0.1)
        assert reconstructed_second < reconstructed_first
    finally:
        client.enter_cold()
