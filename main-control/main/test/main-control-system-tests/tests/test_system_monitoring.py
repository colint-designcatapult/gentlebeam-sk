"""Safety monitoring observed through physical inputs and the PC interface."""

from __future__ import annotations

import time

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.hvps import HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Fault,
    FaultMessage,
    OperationalPoint,
    State,
    word_float,
)
from main_control_system_tests.protocol import MainControlClient


def _fault_record(client: MainControlClient, category: Fault, evidence: Evidence) -> FaultMessage:
    first = FaultMessage.decode(client.command(2, 0))
    records = [first]
    records.extend(
        FaultMessage.decode(client.command(2, index))
        for index in range(1, first.active_count)
    )
    matches = [record for record in records if 1 << record.fault_type == category]
    evidence.record("Monitoring fault diagnostic selection", expected_category=category,
                    observed_categories=[1 << record.fault_type for record in records],
                    observed_matching_count=len(matches), expected_matching_count=1)
    assert len(matches) == 1, records
    return matches[0]


@pytest.mark.strictdoc("TC-H1FWMC-132")
@pytest.mark.parametrize("state", (State.COLD, State.PRIMED, State.READY), ids=lambda state: state.name)
@pytest.mark.parametrize("current_ma", (0.295, 0.35, 3.0), ids=("at-limit", "above-srs-limit", "gross-emission"))
def test_undesired_emission_monitoring(
    client: MainControlClient, hvps: HvpsSimulator, state: State, current_ma: float, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-71, scope=function, role=Verifies)
    UID: TC-H1FWMC-132
    TITLE: Undesired emission monitoring - Test Case

    STATEMENT: Cathode current above 0.3 mA faults outside Emission; current at
    the limit is accepted in Cold, Primed, and Ready.

    PREREQUISITES: A healthy host, a controllable HVPS UART peer, and a valid
    operational point allow normal PC commands to reach each tested state.

    STEPS:
    1. Remain Cold, warm up to Primed, or prepare an operational point to Ready.
    2. Report 0.295 mA, 0.35 mA, or 10 mA of cathode current without starting emission.
    3. Observe state, faults, and cathode feedback for up to 3 seconds.
    4. For excess current, inspect the retained fault's measured value and limit.

    EXPECTED_BEHAVIOR: 0.295 mA causes no fault or state change. Both greater
    currents produce the undesired-emission fault, identifying measured mA
    and the 0.3 mA safety limit in each non-Emission state.
    """
    if state == State.PRIMED:
        client.warmup()
        client.wait_for_state(State.PRIMED, timeout=20)
    elif state == State.READY:
        client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=20))
    initial = client.query_telemetry()
    evidence.record("Undesired emission initial state", observed_state=initial.state, expected_state=state,
                    observed_faults=initial.faults, expected_faults=Fault(0))
    assert initial.state == state and not initial.faults

    try:
        hvps.set_feedback(ma=current_ma)
        latest = client.wait_for_telemetry(
            lambda item: abs(item.ma_feedback - current_ma) < 1e-5, timeout=1
        )
        deadline = time.monotonic() + 3
        first_runtime_ms = latest.runtime_ms
        first_current_ma = latest.ma_feedback
        try:
            while time.monotonic() < deadline:
                latest = client.query_telemetry()
                if current_ma > 0.3 and latest.faults & Fault.GRID:
                    break
                assert not latest.faults, latest
                assert latest.state == state, latest
                time.sleep(0.05)
        finally:
            evidence.record("Undesired emission boundary observation (mA, ms)",
                            input_current_ma=current_ma, threshold_ma=0.3,
                            observed_first_current_ma=first_current_ma, observed_last_current_ma=latest.ma_feedback,
                            expected_current_ma=current_ma, initial_feedback_tolerance_ma=1e-5,
                            observed_state=latest.state, expected_healthy_state=state,
                            observed_faults=latest.faults, expected_fault=Fault(0) if current_ma == 0.295 else Fault.GRID,
                            observed_duration_ms=latest.runtime_ms - first_runtime_ms, observation_limit_s=3)

        if current_ma == 0.295:
            assert latest.state == state and not latest.faults
        else:
            assert latest.faults & Fault.GRID, (
                f"RQ-H1FWMC-71: {current_ma} mA exceeds 0.3 mA in {state.name}, "
                f"but no undesired-emission fault was reported within 3 seconds: {latest}"
            )
            report = _fault_record(client, Fault.GRID, evidence)
            evidence.record("Undesired emission diagnostic (mA)", observed_state=report.state, expected_state=state,
                            observed_format=report.format_text, expected_format_terms=("mA", "threshold"),
                            observed_arguments_ma=tuple(map(word_float, report.arguments)),
                            expected_arguments_ma=(current_ma, 0.295))
            assert report.state == state
            assert "mA" in report.format_text and "threshold" in report.format_text
            assert word_float(report.arguments[0]) == pytest.approx(current_ma)
            assert word_float(report.arguments[1]) == pytest.approx(0.3), (
                "RQ-H1FWMC-71 requires a 0.3 mA undesired-emission limit", report
            )
    finally:
        hvps.set_feedback(ma=None)


@pytest.mark.strictdoc("TC-H1FWMC-135")
def test_hvps_communication_activity_and_loss(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-68, scope=function, role=Verifies)
    UID: TC-H1FWMC-135
    TITLE: HVPS Interface Communication - Test Case

    STATEMENT: Valid HVPS communication changes LED4; loss stops the activity
    indication and raises an HVPS communication fault.

    PREREQUISITES: The host is Cold without faults, HVPS status is valid, and
    the physical LED4 output and PC diagnostics are observable.

    STEPS:
    1. Observe two LED4 transitions while checking healthy PC telemetry.
    2. Stop HVPS status transmission and allow 0.25 seconds for in-flight data.
    3. Observe LED4 and PC telemetry for 2 seconds, then query the fault record.

    EXPECTED_BEHAVIOR: Valid communication produces at least two LED4 changes.
    With communication stopped, LED4 remains unchanged and an HVPS
    communication fault identifies the missing HVPS response.
    """
    previous = io_model.read()["gpio"]["pins"]["port_d"]["io_led4"]
    transitions = 0
    deadline = time.monotonic() + 2
    initial_level = previous
    snapshot = None
    try:
        while transitions < 2 and time.monotonic() < deadline:
            current = io_model.read()["gpio"]["pins"]["port_d"]["io_led4"]
            transitions += current != previous
            previous = current
            snapshot = client.query_telemetry()
            assert snapshot.state == State.COLD and not snapshot.faults
            time.sleep(0.01)
    finally:
        evidence.record("HVPS healthy LED4 activity", observed_initial_level=initial_level,
                        observed_final_level=previous, observed_transitions=transitions, expected_minimum_transitions=2,
                        observed_state=None if snapshot is None else snapshot.state, expected_state=State.COLD,
                        observed_faults=None if snapshot is None else snapshot.faults, expected_faults=Fault(0),
                        observation_limit_s=2)
    assert transitions >= 2, "Valid HVPS status did not produce two LED4 transitions"

    try:
        hvps.set_transmitting(False)
        time.sleep(0.25)
        stopped_level = io_model.read()["gpio"]["pins"]["port_d"]["io_led4"]
        deadline = time.monotonic() + 2
        fault_seen = False
        observed_level = stopped_level
        samples = 0
        loss_started = time.monotonic()
        try:
            while time.monotonic() < deadline:
                observed_level = io_model.read()["gpio"]["pins"]["port_d"]["io_led4"]
                samples += 1
                assert observed_level == stopped_level
                snapshot = client.query_telemetry()
                fault_seen |= bool(snapshot.faults & Fault.HVPS_COMM)
                assert not snapshot.faults & ~Fault.HVPS_COMM, snapshot
                time.sleep(0.02)
        finally:
            evidence.record("HVPS communication loss LED4 and fault observation (s)",
                            observed_level=observed_level, expected_stopped_level=stopped_level,
                            observed_samples=samples, observed_duration_s=time.monotonic() - loss_started,
                            observation_limit_s=2, in_flight_settle_s=0.25,
                            observed_fault_seen=fault_seen, expected_fault_seen=True,
                            observed_faults=None if snapshot is None else snapshot.faults, expected_faults=Fault.HVPS_COMM,
                            observed_state=None if snapshot is None else snapshot.state, expected_state=State.COLD_FAULT)
        assert fault_seen, "Missing HVPS status did not produce a communication fault"
        assert snapshot.state == State.COLD_FAULT
        report = _fault_record(client, Fault.HVPS_COMM, evidence)
        evidence.record("HVPS communication fault diagnostic", observed_format=report.format_text,
                        expected_format_terms=("HVPS", "response"), observed_state=report.state, expected_state=State.COLD)
        assert "HVPS" in report.format_text and "response" in report.format_text
        assert report.state == State.COLD
    finally:
        hvps.set_transmitting(True)


@pytest.mark.strictdoc("TC-H1FWMC-136")
def test_cabinet_temperature_boundary_and_diagnostics(
    client: MainControlClient, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-66, scope=function, role=Verifies)
    UID: TC-H1FWMC-136
    TITLE: Temperature monitoring - Cabinet Temperature Limit - Test Case

    STATEMENT: Cabinet temperature on the safe side of 40 degrees C is
    accepted; crossing the limit produces a diagnostic temperature fault.

    PREREQUISITES: The host is Cold without faults and the cabinet thermistor
    voltage can be controlled. The physical ADC cannot represent exactly
    40 degrees C, so adjacent representable values bracket that boundary.

    STEPS:
    1. Apply 2.52685546875 V and wait for reported temperature just below 40 C.
    2. Observe healthy telemetry for 1 second after the input has settled.
    3. Apply 2.525634765625 V to raise reported temperature just above 40 C.
    4. Inspect the temperature fault and its measured-temperature/high-limit arguments.

    EXPECTED_BEHAVIOR: The lower temperature causes no fault. Above 40 C the
    system enters Cold Fault and reports the measured temperature and 40 C
    high limit, identifying the cabinet rather than another thermal sensor.
    """
    original = io_model.read()["adcs"]["system"]["cabinet_thermistor"]
    try:
        io_model.patch({"adcs": {"system": {"cabinet_thermistor": 2.52685546875}}})
        snapshot = client.wait_for_telemetry(
            lambda item: 39.97 < item.cabinet_temperature < 40.0, timeout=3
        )
        deadline = time.monotonic() + 1
        first_runtime_ms = snapshot.runtime_ms
        minimum_c = snapshot.cabinet_temperature
        maximum_c = snapshot.cabinet_temperature
        try:
            while time.monotonic() < deadline:
                snapshot = client.query_telemetry()
                minimum_c = min(minimum_c, snapshot.cabinet_temperature)
                maximum_c = max(maximum_c, snapshot.cabinet_temperature)
                assert 39.97 < snapshot.cabinet_temperature < 40.0
                assert snapshot.state == State.COLD and not snapshot.faults, snapshot
                time.sleep(0.05)
        finally:
            evidence.record("Cabinet safe temperature boundary hold (C, ms)", input_volts=2.52685546875,
                            observed_temperature_c=snapshot.cabinet_temperature, observed_bounds_c=(minimum_c, maximum_c),
                            expected_exclusive_bounds_c=(39.97, 40.0),
                            observed_state=snapshot.state, expected_state=State.COLD,
                            observed_faults=snapshot.faults, expected_faults=Fault(0),
                            observed_duration_ms=snapshot.runtime_ms - first_runtime_ms, requested_hold_s=1)

        io_model.patch({"adcs": {"system": {"cabinet_thermistor": 2.525634765625}}})
        snapshot = client.wait_for_telemetry(
            lambda item: 40.0 < item.cabinet_temperature < 40.02
            and bool(item.faults & Fault.HEATSINK),
            timeout=3,
        )
        evidence.record("Cabinet overtemperature boundary (C)", input_volts=2.525634765625,
                        observed_temperature_c=snapshot.cabinet_temperature, expected_exclusive_bounds_c=(40.0, 40.02),
                        observed_faults=snapshot.faults, expected_faults=Fault.HEATSINK, timeout_s=3)
        assert snapshot.faults == Fault.HEATSINK
        faulted = client.wait_for_state(State.COLD_FAULT)
        evidence.record("Cabinet overtemperature state", observed_state=faulted.state, expected_state=State.COLD_FAULT)
        report = _fault_record(client, Fault.HEATSINK, evidence)
        evidence.record("Cabinet temperature diagnostic identity", observed_format=report.format_text,
                        expected_format_terms=("Cabinet", "temperature"), observed_state=report.state, expected_state=State.COLD)
        assert "Cabinet" in report.format_text and "temperature" in report.format_text
        assert report.state == State.COLD
        measured, high_limit = map(word_float, report.arguments)
        evidence.record("Cabinet fault diagnostic temperature (C)", observed_measured_c=measured,
                        expected_exclusive_measured_bounds_c=(40.0, 40.02),
                        observed_high_limit_c=high_limit, expected_high_limit_c=40.0)
        assert 40.0 < measured < 40.02
        assert high_limit == 40.0
    finally:
        io_model.patch({"adcs": {"system": {"cabinet_thermistor": original}}})
