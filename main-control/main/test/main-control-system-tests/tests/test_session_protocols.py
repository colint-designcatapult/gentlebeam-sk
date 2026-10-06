"""Scalar treatment sessions exercised through PC UDP and physical host inputs."""

from __future__ import annotations

from dataclasses import replace
import time
import zlib

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.hvps import HvpsCommandCode, HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Directive, Fault, FaultMessage, OperationalPoint, State, authenticated_payload,
    word_float,
)
from main_control_system_tests.protocol import MainControlClient, encode_packet


OK, ACCESS_ERROR, OUT_OF_BOUNDS, INVALID = range(4)
POINT = OperationalPoint.beam_qa(50, duration_s=10)
# Normal-mode limits, in seconds, kV, and mA (including beam-optics currents).
LIMITS = (
    ("total_time_s", 0.0, 180.0), ("remaining_time_s", 0.0, 180.0),
    ("kv", 0.0, 100.0), ("ma", 0.0, 8.0),
    ("heater_ma", 0.0, 3250.0), ("x_coil_ma", -3000.0, 3000.0),
    ("y_coil_ma", -3000.0, 3000.0), ("focus_coil_ma", 0.0, 3000.0),
)


def _primed(client: MainControlClient, evidence: Evidence) -> None:
    client.warmup()
    _state(client, evidence, State.PRIMED)
    client.directive(Directive.RESET_TIMERS)
    timers = client.wait_for_telemetry(
        lambda item: item.internal_timer_s == item.timer_1_s == item.timer_2_s == 0
    )
    evidence.record(
        "Reset timers (s)",
        observed=(timers.internal_timer_s, timers.timer_1_s, timers.timer_2_s),
        expected=(0, 0, 0),
    )


def _staging(client: MainControlClient, evidence: Evidence) -> int:
    _primed(client, evidence)
    session = client.new_session()
    observed = _state(client, evidence, State.STAGING)
    evidence.record(
        "New session",
        observed_session=session,
        observed_state=observed.state,
        expected_state=State.STAGING,
    )
    return session


def _staged(client: MainControlClient, evidence: Evidence, point: OperationalPoint = POINT) -> int:
    session = _staging(client, evidence)
    client.load_operational_point(session, point)
    client.directive(Directive.STAGE_PLAN)
    observed = _state(client, evidence, State.STAGED)
    evidence.record(
        "Loaded scalar point (s, kV, mA)",
        requested_point=point,
        observed_state=observed.state,
        expected_state=State.STAGED,
    )
    return session


def _point_command(
    client: MainControlClient, evidence: Evidence, command: int, session: int, point: OperationalPoint,
) -> tuple[int, ...]:
    response = client.command(command, *authenticated_payload(session, command, point.payload())).payload
    changed_limits = {
        field: (lower, upper) for field, lower, upper in LIMITS
        if getattr(point, field) != getattr(POINT, field)
    }
    evidence.record(
        "Scalar command (s, kV, mA)", command=command, session=session,
        requested_point=point, changed_field_limits=changed_limits,
        observed_statuses=response,
    )
    return response


def _release(client: MainControlClient, evidence: Evidence, session: int) -> tuple[int, ...]:
    response = client.command(11, *authenticated_payload(session, 11, (1,))).payload
    evidence.record("Release session", session=session, observed_statuses=response)
    return response


def _field_error(index: int, status: int) -> tuple[int, ...]:
    return tuple(status if field == index else OK for field in range(9))


def _assert_point(client: MainControlClient, evidence: Evidence, point: OperationalPoint) -> None:
    response = client.command(10).payload
    evidence.record(
        "Scalar readback (s, kV, mA)",
        observed_status=response[:1], expected_status=(OK,),
        observed_point={
            field: word_float(value)
            for (field, _, _), value in zip(LIMITS, response[1:])
        },
        expected_point=point,
    )
    assert response == (OK, *point.payload())


def _assert_inactive(client: MainControlClient, evidence: Evidence, state: State) -> None:
    observed = client.query_telemetry()
    evidence.record(
        "Inactive state and outputs",
        observed_state=observed.state,
        expected_state=state,
        observed_kv=observed.kv_setpoint,
        expected_kv=0,
        observed_ma=observed.ma_feedback,
        expected_ma=0,
        tolerance_ma=0.01,
        observed_faults=observed.faults,
        expected_faults=0,
    )
    assert observed.state == state, observed
    assert observed.kv_setpoint == 0, observed
    assert observed.ma_feedback == pytest.approx(0, abs=0.01), observed
    assert not observed.faults, observed


def _invalid_packet(
    client: MainControlClient,
    evidence: Evidence,
    command: int,
    words: tuple[int, ...],
) -> None:
    packet_id = client.send(command, *words)
    response = client.receive_response(0, packet_id)
    evidence.record(
        "Invalid packet reply",
        command=command,
        payload_word_count=len(words),
        observed_type=response.packet_type,
        expected_type=100,
        observed_packet_id=response.packet_id,
        expected_packet_id=packet_id,
    )
    assert response.packet_type == 100
    assert response.packet_id == packet_id



def _assert_equal(evidence: Evidence, observed, expected, message=None) -> None:
    evidence.record("Command or state comparison", observed=observed, expected=expected)
    assert observed == expected, message


def _state(client: MainControlClient, evidence: Evidence, state: State):
    observed = client.wait_for_state(state)
    evidence.record("Session state transition", observed_state=observed.state, expected_state=state)
    return observed


def _confirm(
    client: MainControlClient,
    evidence: Evidence,
    session: int,
    point: OperationalPoint,
) -> None:
    response = client.confirm_operational_point(session, point)
    evidence.record(
        "Valid scalar confirmation (s, kV, mA)",
        session=session,
        requested_point=point,
        observed_statuses=response.payload,
        expected_statuses=(OK,) * 9,
    )

@pytest.mark.strictdoc("TC-H1FWMC-113")
@pytest.mark.parametrize("interruption", ("standby-timeout", "fault"))
def test_confirmation_requires_uninterrupted_match(
    client: MainControlClient,
    evidence: Evidence,
    io_model: HostIOModel,
    interruption: str,
) -> None:
    """
    @relation(RQ-H1FWMC-24, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-31, scope=function, role=Verifies)
    UID: TC-H1FWMC-113
    TITLE: Uninterrupted scalar confirmation - Test Case
    STATEMENT: Only an exact staged match authorizes release; interruption requires reconfirmation.
    PREREQUISITES: Healthy inputs, PC commands and telemetry, adjustable base emergency stop.
    STEPS:
    1. Stage a 50 kV, 1 mA, 10 second point; confirm 51 kV, then the exact point twice.
    2. Wait for the 120 second standby timeout with PC keepalive, or open the emergency stop.
    3. Recover, warm up the retained plan, attempt release, then reconfirm and release.
    EXPECTED_BEHAVIOR: The mismatch and both post-interruption releases are rejected.
    Exact confirmation succeeds in Staged; reconfirmation permits Ready without emission.
    """
    session = _staged(client, evidence)
    _assert_equal(
        evidence,
        _point_command(client, evidence, 9, session, replace(POINT, kv=51)),
        _field_error(2, INVALID),
    )
    _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
    _assert_equal(evidence, _point_command(client, evidence, 9, session, POINT), (OK,) * 9)
    _assert_equal(evidence, _point_command(client, evidence, 9, session, POINT), (OK,) * 9)
    started = client.query_telemetry()
    if interruption == "standby-timeout":
        stopped = client.wait_for_telemetry(
            lambda item: item.state == State.COLD, timeout=310, poll_interval=0.2,
        )
        evidence.record(
            "Interrupted confirmation standby timeout (ms)",
            observed_elapsed_ms=stopped.runtime_ms - started.runtime_ms,
            expected_bounds_ms=(298_000, 310_000),
            observed_state=stopped.state,
            expected_state=State.COLD,
            observed_faults=stopped.faults,
            expected_faults=0,
        )
        assert 298_000 <= stopped.runtime_ms - started.runtime_ms <= 310_000
        assert not stopped.faults
    else:
        try:
            io_model.set_interlock("io_base_estop_n", False)
            faulted = client.wait_for_telemetry(lambda item: bool(item.faults & Fault.INTERLOCK))
            evidence.record(
                "Interlock interruption",
                observed_state=faulted.state,
                forbidden_states=(State.READY, State.LAUNCHING, State.EMISSION),
                observed_faults=faulted.faults,
                expected_fault_mask=Fault.INTERLOCK,
            )
            assert faulted.state not in (State.READY, State.LAUNCHING, State.EMISSION)
            _state(client, evidence, State.FAULT)
        finally:
            io_model.set_interlock("io_base_estop_n", True)
        client.enter_cold()
    _assert_equal(evidence, _release(client, evidence, session), (ACCESS_ERROR, OK))
    _assert_point(client, evidence, POINT)
    client.warmup()
    _state(client, evidence, State.STAGED)
    _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
    _assert_inactive(client, evidence, State.STAGED)
    _confirm(client, evidence, session, POINT)
    client.release_plan(session)
    _state(client, evidence, State.READY)


@pytest.mark.strictdoc("TC-H1FWMC-114")
def test_open_door_blocks_plan_release(
    client: MainControlClient,
    evidence: Evidence,
    io_model: HostIOModel,
) -> None:
    """
    @relation(RQ-H1FWMC-25, scope=function, role=Verifies)
    UID: TC-H1FWMC-114
    TITLE: Plan release with open door - Test Case
    STATEMENT: An open treatment-room door prevents plan execution.
    PREREQUISITES: Confirmed 50 kV, 1 mA, 10 second point and controllable door input.
    STEPS: Open the door in Staged, send plan release, and inspect telemetry and fault records.
    EXPECTED_BEHAVIOR: The open door is reported, a configuration fault identifies the
    rejected release, and HV and emission stay disabled for 1 second.
    """
    session = _staged(client, evidence)
    _confirm(client, evidence, session, POINT)
    try:
        io_model.set_interlock("io_door_closed", False)
        door = client.wait_for_telemetry(lambda item: not item.interlocks & 1)
        evidence.record(
            "Open treatment door",
            observed_door_closed=bool(door.interlocks & 1),
            expected_door_closed=False,
        )
        # The command acknowledges queueing; the state transition validates readiness.
        _assert_equal(evidence, _release(client, evidence, session), (OK, OK))
        client.wait_for_telemetry(lambda item: bool(item.faults & Fault.INVALID_CONFIG))
        first = FaultMessage.decode(client.command(2, 0))
        records = [first] + [
            FaultMessage.decode(client.command(2, index)) for index in range(1, first.active_count)
        ]
        evidence.record(
            "Rejected release fault records",
            observed_fault_masks=[1 << record.fault_type for record in records],
            expected_fault_mask=Fault.INVALID_CONFIG,
        )
        assert any((1 << record.fault_type) == Fault.INVALID_CONFIG for record in records)
        deadline = time.monotonic() + 1
        samples = []
        while time.monotonic() < deadline:
            observed = client.query_telemetry()
            samples.append((observed.state, observed.kv_setpoint, observed.ma_feedback))
            if (
                len(samples) == 1
                or observed.state in (State.HVPS_CHECK, State.SETUP, State.READY, State.LAUNCHING, State.EMISSION)
                or observed.kv_setpoint != 0
                or observed.ma_feedback != pytest.approx(0, abs=0.01)
            ):
                evidence.record(
                    "Door inhibition sample",
                    observed_state=observed.state,
                    forbidden_states=(State.HVPS_CHECK, State.SETUP, State.READY, State.LAUNCHING, State.EMISSION),
                    observed_kv=observed.kv_setpoint,
                    expected_kv=0,
                    observed_ma=observed.ma_feedback,
                    expected_ma=0,
                    tolerance_ma=0.01,
                )
            assert observed.state not in (State.HVPS_CHECK, State.SETUP, State.READY, State.LAUNCHING, State.EMISSION)
            assert observed.kv_setpoint == 0
            assert observed.ma_feedback == pytest.approx(0, abs=0.01)
            pins = io_model.read()["gpio"]["pins"]["port_a"]
            emission_enabled = pins["io_emission_en"]
            if len(samples) == 1 or emission_enabled:
                evidence.record(
                    "Door inhibition emission pin",
                    observed_emission_enabled=emission_enabled,
                    expected_emission_enabled=False,
                )
            assert not pins["io_emission_en"]
            time.sleep(0.025)
        if samples:
            evidence.record(
                "Open-door inhibition over 1 s",
                observed_states=list(dict.fromkeys(item[0] for item in samples)),
                forbidden_states=(State.HVPS_CHECK, State.SETUP, State.READY, State.LAUNCHING, State.EMISSION),
                observed_kv_bounds=(min(item[1] for item in samples), max(item[1] for item in samples)),
                expected_kv=0,
                observed_ma_bounds=(min(item[2] for item in samples), max(item[2] for item in samples)),
                expected_ma=0,
                tolerance_ma=0.01,
                last_emission_enabled=emission_enabled,
                expected_emission_enabled=False,
            )
    finally:
        io_model.set_interlock("io_door_closed", True)


@pytest.mark.strictdoc("TC-H1FWMC-115")
def test_plan_release_requires_staged_confirmed_session(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-26, scope=function, role=Verifies)
    UID: TC-H1FWMC-115
    TITLE: Plan release state validation - Test Case
    STATEMENT: Release requires a staged and confirmed treatment point.
    PREREQUISITES: Healthy system and PC command replies and telemetry.
    STEPS: Request release in Cold, Primed, Staging, unconfirmed Staged, and Ready.
    EXPECTED_BEHAVIOR: Every request is rejected without starting HV or emission;
    only a confirmed Staged release progresses to Ready.
    """
    _assert_equal(evidence, _release(client, evidence, 0), (ACCESS_ERROR, OK))
    _assert_inactive(client, evidence, State.COLD)
    _primed(client, evidence)
    _assert_equal(evidence, _release(client, evidence, 0), (ACCESS_ERROR, OK))
    _assert_inactive(client, evidence, State.PRIMED)
    session = client.new_session()
    _state(client, evidence, State.STAGING)
    _assert_equal(evidence, _release(client, evidence, session), (ACCESS_ERROR, OK))
    _assert_inactive(client, evidence, State.STAGING)
    client.load_operational_point(session, POINT)
    client.directive(Directive.STAGE_PLAN)
    _state(client, evidence, State.STAGED)
    _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
    _assert_inactive(client, evidence, State.STAGED)
    _confirm(client, evidence, session, POINT)
    client.release_plan(session)
    _state(client, evidence, State.READY)
    _assert_equal(evidence, _release(client, evidence, session), (ACCESS_ERROR, OK))
    ready = client.query_telemetry()
    evidence.record(
        "Ready without emission",
        observed_state=ready.state,
        expected_state=State.READY,
        observed_ma=ready.ma_feedback,
        expected_ma=0,
        tolerance_ma=0.01,
    )
    assert ready.state == State.READY
    assert ready.ma_feedback == pytest.approx(0, abs=0.01)


@pytest.mark.strictdoc("TC-H1FWMC-116")
def test_each_confirmation_value_must_match(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-27, scope=function, role=Verifies)
    UID: TC-H1FWMC-116
    TITLE: Incorrect scalar confirmation values - Test Case
    STATEMENT: Each confirmation parameter must equal the staged value.
    PREREQUISITES: Staged 50 kV, 1 mA, 10 second point with 2500 mA filament current.
    STEPS: Increase each of the eight parameters by 1 second, 1 kV, or 1 mA in turn.
    EXPECTED_BEHAVIOR: Each reply identifies only the changed value as invalid,
    leaves the staged point unchanged, and denies release without enabling HV.
    """
    session = _staged(client, evidence)
    for index, (field, _, _) in enumerate(LIMITS):
        changed = replace(POINT, **{field: getattr(POINT, field) + 1})
        _assert_equal(
            evidence,
            _point_command(client, evidence, 9, session, changed),
            _field_error(index, INVALID),
            field,
        )
        _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
        _assert_point(client, evidence, POINT)
        _assert_inactive(client, evidence, State.STAGED)


@pytest.mark.strictdoc("TC-H1FWMC-117")
def test_malformed_confirmation_is_rejected(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-28, scope=function, role=Verifies)
    UID: TC-H1FWMC-117
    TITLE: Invalid confirmation format - Test Case
    STATEMENT: Malformed or non-finite confirmation values cannot authorize treatment.
    PREREQUISITES: One staged 50 kV, 1 mA, 10 second point and PC UDP access.
    STEPS: Confirm with NaN and both infinities in each field, wrong authentication,
    a missing value, and obsolete point-index/count/automatic-execution fields.
    EXPECTED_BEHAVIOR: Replies reject each malformed request, retain the staged
    point, and deny release. A subsequent valid confirmation is acknowledged.
    """
    session = _staged(client, evidence)
    for index, (field, _, _) in enumerate(LIMITS):
        for value in (float("nan"), float("inf"), float("-inf")):
            point = replace(POINT, **{field: value})
            _assert_equal(
                evidence,
                _point_command(client, evidence, 9, session, point),
                _field_error(index, INVALID),
            )
    valid = authenticated_payload(session, 9, POINT.payload())
    _assert_equal(
        evidence,
        client.command(9, *valid[:-1], valid[-1] ^ 1).payload,
        _field_error(8, INVALID),
    )
    for words in (valid[:-1], (0, *valid), (1, 0, *valid)):
        _invalid_packet(client, evidence, 9, words)
        _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
        _assert_point(client, evidence, POINT)
        _assert_inactive(client, evidence, State.STAGED)
    _confirm(client, evidence, session, POINT)


@pytest.mark.strictdoc("TC-H1FWMC-118")
def test_confirmation_rejects_out_of_range_values(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-29, scope=function, role=Verifies)
    UID: TC-H1FWMC-118
    TITLE: Confirmation operating ranges - Test Case
    STATEMENT: Out-of-range confirmation parameters cannot authorize release.
    PREREQUISITES: Staged point; limits are 0–180 seconds, 0–100 kV, 0–8 mA beam,
    0–3250 mA filament, ±3000 mA deflection, and 0–3000 mA focus.
    STEPS: Confirm each parameter 0.01 units below its lower and above its upper limit.
    EXPECTED_BEHAVIOR: Each offending parameter is rejected, the staged point is
    unchanged, and plan release remains denied with HV off.
    """
    session = _staged(client, evidence)
    for index, (field, lower, upper) in enumerate(LIMITS):
        for value in (lower - 0.01, upper + 0.01):
            _assert_equal(
                evidence,
                _point_command(client, evidence, 9, session, replace(POINT, **{field: value})),
                _field_error(index, INVALID),
            )
            _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
            _assert_point(client, evidence, POINT)
            _assert_inactive(client, evidence, State.STAGED)


@pytest.mark.strictdoc("TC-H1FWMC-119")
def test_valid_confirmation_acknowledges_operating_limits(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-30, scope=function, role=Verifies)
    UID: TC-H1FWMC-119
    TITLE: Valid scalar parameter acknowledgment - Test Case
    STATEMENT: Exact confirmations acknowledge every supported operating boundary.
    PREREQUISITES: Healthy Primed system with PC commands and point readback.
    STEPS: Stage and confirm each field at both limits: 0–180 seconds, 0–100 kV,
    0–8 mA beam, 0–3250 mA filament, ±3000 mA deflection, 0–3000 mA focus.
    EXPECTED_BEHAVIOR: All eight parameter statuses acknowledge success and
    readback matches the point; confirmation alone does not enable HV or emission.
    """
    _primed(client, evidence)
    for field, lower, upper in LIMITS:
        for value in (lower, upper):
            session = client.new_session()
            _state(client, evidence, State.STAGING)
            point = replace(POINT, **{field: value})
            client.load_operational_point(session, point)
            client.directive(Directive.STAGE_PLAN)
            _state(client, evidence, State.STAGED)
            _assert_equal(evidence, _point_command(client, evidence, 9, session, point), (OK,) * 9)
            _assert_point(client, evidence, point)
            _assert_inactive(client, evidence, State.STAGED)
            client.directive(Directive.WIPE_PLAN)
            _state(client, evidence, State.PRIMED)


@pytest.mark.strictdoc("TC-H1FWMC-121")
def test_scalar_point_loads_at_operating_limits(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-82, scope=function, role=Verifies)
    UID: TC-H1FWMC-121
    TITLE: Scalar treatment point loading - Test Case
    STATEMENT: Staging accepts exactly one scalar point at inclusive operating limits.
    PREREQUISITES: Staging session, PC loading commands and scalar point readback.
    STEPS: Load each field at both limits: 0–180 seconds, 0–100 kV, 0–8 mA beam,
    0–3250 mA filament, ±3000 mA deflection, and 0–3000 mA focus.
    EXPECTED_BEHAVIOR: Each load is acknowledged and replaces the single point;
    readback contains status and eight scalars, without a point index or count.
    """
    session = _staging(client, evidence)
    for field, lower, upper in LIMITS:
        for value in (lower, upper):
            point = replace(POINT, **{field: value})
            _assert_equal(evidence, _point_command(client, evidence, 8, session, point), (OK,) * 9)
            _assert_point(client, evidence, point)
            _assert_inactive(client, evidence, State.STAGING)
    client.directive(Directive.STAGE_PLAN)
    _state(client, evidence, State.STAGED)


@pytest.mark.strictdoc("TC-H1FWMC-129")
def test_confirmed_release_runs_hv_check_and_setup(
    client: MainControlClient,
    evidence: Evidence,
    hvps: HvpsSimulator,
) -> None:
    """
    @relation(RQ-H1FWMC-74, scope=function, role=Verifies)
    UID: TC-H1FWMC-129
    TITLE: Confirmed plan release - Test Case
    STATEMENT: A confirmed plan progresses through HV check and setup to Ready.
    PREREQUISITES: Healthy inputs, confirmed 50 kV, 1 mA, 10 second point,
    continuous PC telemetry, and HVPS command observations.
    STEPS: Release the plan and observe states, then inspect ready setpoints.
    EXPECTED_BEHAVIOR: HVPS Check precedes Setup and Ready; an HVPS interlock
    test occurs, acceleration reaches 50 kV, and emission remains off.
    """
    session = _staged(client, evidence)
    _confirm(client, evidence, session, POINT)
    started = client.query_telemetry()
    command_start = len(hvps.commands)
    client.release_plan(session)
    states = []
    deadline = time.monotonic() + 15
    keepalive_at = 0.0
    while time.monotonic() < deadline:
        if time.monotonic() >= keepalive_at:
            client.query_telemetry()
            keepalive_at = time.monotonic() + 0.2
        sample = client.normal_telemetry(timeout=1)
        if sample.runtime_ms < started.runtime_ms:
            continue
        if sample.faults:
            evidence.record(
                "Release progression fault",
                observed_state=sample.state,
                observed_faults=sample.faults,
                expected_faults=0,
                observed_states=states,
            )
        assert not sample.faults, sample
        if not states or states[-1] != sample.state:
            states.append(sample.state)
        if sample.state == State.READY:
            break
    evidence.record(
        "Released plan state progression",
        observed_states=states,
        expected_order=(State.HVPS_CHECK, State.SETUP, State.READY),
        observed_command_codes=[command.code for command in hvps.commands[command_start:]],
        expected_command=HvpsCommandCode.INTERLOCK_TEST,
    )
    assert State.HVPS_CHECK in states, states
    assert State.SETUP in states, states
    assert State.READY in states, states
    assert states.index(State.HVPS_CHECK) < states.index(State.SETUP) < states.index(State.READY)
    assert any(command.code == HvpsCommandCode.INTERLOCK_TEST for command in hvps.commands[command_start:])
    ready = client.wait_for_telemetry(lambda item: item.state == State.READY and abs(item.kv_feedback - 50) < 0.5)
    evidence.record(
        "Ready acceleration and inactive beam",
        observed_kv=ready.kv_setpoint,
        expected_kv=50,
        observed_feedback_kv=ready.kv_feedback,
        feedback_tolerance_kv=0.5,
        observed_ma=ready.ma_feedback,
        expected_ma=0,
        tolerance_ma=0.01,
    )
    assert ready.kv_setpoint == 50
    assert ready.ma_feedback == pytest.approx(0, abs=0.01)
    _assert_point(client, evidence, POINT)


@pytest.mark.strictdoc("TC-H1FWMC-157")
def test_staging_rejects_out_of_range_parameters(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-45, scope=function, role=Verifies)
    UID: TC-H1FWMC-157
    TITLE: Staging parameter range validation - Test Case
    STATEMENT: An out-of-range scalar is rejected without replacing the stored point.
    PREREQUISITES: Staging session with a valid 50 kV, 1 mA, 10 second point.
    STEPS: Load each field 0.01 units outside both limits: 0–180 seconds, 0–100 kV,
    0–8 mA beam, 0–3250 mA filament, ±3000 mA deflection, 0–3000 mA focus.
    EXPECTED_BEHAVIOR: Only the offending field reports out of bounds; the prior
    point remains unchanged and the system stays in Staging with HV off.
    """
    session = _staging(client, evidence)
    client.load_operational_point(session, POINT)
    for index, (field, lower, upper) in enumerate(LIMITS):
        for value in (lower - 0.01, upper + 0.01):
            _assert_equal(
                evidence,
                _point_command(client, evidence, 8, session, replace(POINT, **{field: value})),
                _field_error(index, OUT_OF_BOUNDS),
            )
            _assert_point(client, evidence, POINT)
            _assert_inactive(client, evidence, State.STAGING)


@pytest.mark.strictdoc("TC-H1FWMC-158")
def test_staging_rejects_invalid_parameter_formats(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-44, scope=function, role=Verifies)
    UID: TC-H1FWMC-158
    TITLE: Invalid staging parameter formats - Test Case
    STATEMENT: Malformed scalar loads cannot establish a treatment point.
    PREREQUISITES: Empty Staging session and PC UDP access.
    STEPS: Send NaN and both infinities in every field, wrong authentication,
    a missing value, and obsolete point-index/count/automatic-execution fields.
    EXPECTED_BEHAVIOR: Each load is rejected, point readback remains empty,
    and finishing staging is denied. A valid scalar load subsequently succeeds.
    """
    session = _staging(client, evidence)
    for index, (field, _, _) in enumerate(LIMITS):
        for value in (float("nan"), float("inf"), float("-inf")):
            _assert_equal(
                evidence,
                _point_command(client, evidence, 8, session, replace(POINT, **{field: value})),
                _field_error(index, INVALID),
            )
            _assert_equal(
                evidence,
                client.command(3, int(Directive.STAGE_PLAN), 1 << int(Directive.STAGE_PLAN)).payload,
                (INVALID,),
            )
            _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    valid = authenticated_payload(session, 8, POINT.payload())
    _assert_equal(
        evidence,
        client.command(8, *valid[:-1], valid[-1] ^ 1).payload,
        _field_error(8, INVALID),
    )
    for words in (valid[:-1], (0, *valid), (1, 0, *valid)):
        _invalid_packet(client, evidence, 8, words)
        _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
        _assert_equal(
            evidence,
            client.command(3, int(Directive.STAGE_PLAN), 1 << int(Directive.STAGE_PLAN)).payload,
            (INVALID,),
        )
        _assert_inactive(client, evidence, State.STAGING)
    client.load_operational_point(session, POINT)
    client.directive(Directive.STAGE_PLAN)
    _state(client, evidence, State.STAGED)


@pytest.mark.strictdoc("TC-H1FWMC-159")
def test_loading_requires_staging_process(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-43, scope=function, role=Verifies)
    UID: TC-H1FWMC-159
    TITLE: Treatment loading state restriction - Test Case
    STATEMENT: Scalar loading is accepted only during Staging.
    PREREQUISITES: Healthy system and PC commands with point readback.
    STEPS: Load in Cold and Primed, then create, load and finalize a session;
    attempt another load in Staged and Ready.
    EXPECTED_BEHAVIOR: All loads outside Staging return access errors, preserve
    the stored point and state, and do not begin emission.
    """
    for state in (State.COLD, State.PRIMED):
        if state == State.PRIMED:
            _primed(client, evidence)
        _assert_equal(
            evidence,
            _point_command(client, evidence, 8, 0, POINT),
            _field_error(0, ACCESS_ERROR),
        )
        _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
        _assert_inactive(client, evidence, state)
    session = client.new_session()
    _state(client, evidence, State.STAGING)
    client.load_operational_point(session, POINT)
    client.directive(Directive.STAGE_PLAN)
    _state(client, evidence, State.STAGED)
    for state in (State.STAGED, State.READY):
        if state == State.READY:
            _confirm(client, evidence, session, POINT)
            client.release_plan(session)
            _state(client, evidence, State.READY)
        _assert_equal(
            evidence,
            _point_command(client, evidence, 8, session, replace(POINT, kv=70)),
            _field_error(0, ACCESS_ERROR),
        )
        _assert_point(client, evidence, POINT)
        observed = client.query_telemetry()
        evidence.record(
            "Rejected load preserves state",
            observed_state=observed.state,
            expected_state=state,
            observed_ma=observed.ma_feedback,
            expected_ma=0,
            tolerance_ma=0.01,
        )
        assert observed.state == state
        assert observed.ma_feedback == pytest.approx(0, abs=0.01)


@pytest.mark.strictdoc("TC-H1FWMC-160")
def test_new_session_is_primed_only(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-42, scope=function, role=Verifies)
    UID: TC-H1FWMC-160
    TITLE: New-session Primed restriction - Test Case
    STATEMENT: A new session starts only from Primed and cannot replace an active session.
    PREREQUISITES: Healthy Cold system, PC commands and point readback.
    STEPS: Request a session in Cold; warm up and create one in Primed;
    request another in Staging, Staged, and Ready.
    EXPECTED_BEHAVIOR: Only the Primed request succeeds; rejected requests
    preserve state, stored parameters, and the usable original session ID.
    """
    _assert_equal(evidence, client.command(7).payload, (ACCESS_ERROR, 0))
    _assert_inactive(client, evidence, State.COLD)
    session = _staging(client, evidence)
    client.load_operational_point(session, POINT)
    for state in (State.STAGING, State.STAGED, State.READY):
        if state == State.STAGED:
            client.directive(Directive.STAGE_PLAN)
            _state(client, evidence, state)
        elif state == State.READY:
            _confirm(client, evidence, session, POINT)
            client.release_plan(session)
            _state(client, evidence, state)
        _assert_equal(evidence, client.command(7).payload, (ACCESS_ERROR, 0))
        _assert_equal(evidence, client.query_telemetry().state, state)
        _assert_point(client, evidence, POINT)
    # The rejected creation in Ready must not invalidate the original key.
    _assert_equal(
        evidence,
        client.command(11, *authenticated_payload(session, 11, (0,))).payload,
        (OUT_OF_BOUNDS, OK),
    )


@pytest.mark.strictdoc("TC-H1FWMC-162")
def test_new_session_rejects_malformed_requests(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-40, scope=function, role=Verifies)
    UID: TC-H1FWMC-162
    TITLE: Staging-session request validation - Test Case
    STATEMENT: New-session requests accept no payload and require valid packet framing.
    PREREQUISITES: Healthy Primed system and raw PC UDP access.
    STEPS: Supply obsolete point-count/automatic-execution payloads, then corrupt
    the CRC or synchronization marker of an argument-free request.
    EXPECTED_BEHAVIOR: Every malformed request receives an invalid-packet reply,
    remains Primed with empty point storage, and cannot start staging; a valid request can.
    """
    _primed(client, evidence)
    for words in ((0,), (1,), (63,), (64,), (1, 0)):
        _invalid_packet(client, evidence, 7, words)
        _assert_inactive(client, evidence, State.PRIMED)
        _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    for packet_id, offset in ((0xFFFF0001, -1), (0xFFFF0002, 0)):
        datagram = bytearray(encode_packet(7, packet_id))
        datagram[offset] ^= 1
        if offset == 0:
            datagram[-4:] = zlib.crc32(datagram[:-4]).to_bytes(4, "little")
        client.send_raw(bytes(datagram))
        _assert_equal(evidence, client.receive_response(0, packet_id).packet_type, 100)
        _assert_inactive(client, evidence, State.PRIMED)
        _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    session = client.new_session()
    _state(client, evidence, State.STAGING)
    client.load_operational_point(session, POINT)
    _assert_point(client, evidence, POINT)


@pytest.mark.strictdoc("TC-H1FWMC-170")
def test_argument_free_session_creates_one_scalar_point(
    client: MainControlClient,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-32, scope=function, role=Verifies)
    UID: TC-H1FWMC-170
    TITLE: Scalar session creation - Test Case
    STATEMENT: An argument-free request creates one scalar session and returns its ID.
    PREREQUISITES: Healthy Primed system with reset timers and PC commands.
    STEPS: Create a session, inspect empty scalar readback, reject a load signed
    with another ID, then load and replace the point using the returned ID.
    EXPECTED_BEHAVIOR: The reply contains only success and a generated ID,
    state becomes Staging, and readback holds only the latest authenticated point.
    """
    _primed(client, evidence)
    response = client.command(7)
    evidence.record(
        "Argument-free session response",
        observed_payload=response.payload,
        expected_payload_length=2,
        expected_status=OK,
        forbidden_session_id=0,
    )
    assert len(response.payload) == 2
    assert response.payload[0] == OK
    session = response.payload[1]
    assert session != 0
    _state(client, evidence, State.STAGING)
    _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    _assert_equal(
        evidence,
        _point_command(client, evidence, 8, session ^ 1, POINT),
        _field_error(8, INVALID),
    )
    _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    client.load_operational_point(session, POINT)
    _assert_point(client, evidence, POINT)
    replacement = replace(POINT, kv=70, total_time_s=12, remaining_time_s=12)
    client.load_operational_point(session, replacement)
    _assert_point(client, evidence, replacement)
    client.directive(Directive.STAGE_PLAN)
    _state(client, evidence, State.STAGED)
    _confirm(client, evidence, session, replacement)


@pytest.mark.strictdoc("TC-H1FWMC-171")
def test_confirmation_is_only_after_staging(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-31, scope=function, role=Verifies)
    UID: TC-H1FWMC-171
    TITLE: Scalar confirmation timing - Test Case
    STATEMENT: Confirmation is accepted after staging completes, not before or after release.
    PREREQUISITES: Healthy system, PC commands, and a 50 kV, 1 mA, 10 second point.
    STEPS: Confirm in Cold, Primed, Staging before and after loading, Staged, and Ready.
    EXPECTED_BEHAVIOR: Only the Staged confirmation succeeds; premature attempts
    do not authorize release or change the point, and a Ready attempt does not start emission.
    """
    _assert_equal(
        evidence,
        _point_command(client, evidence, 9, 0, POINT),
        _field_error(0, ACCESS_ERROR),
    )
    _assert_inactive(client, evidence, State.COLD)
    _primed(client, evidence)
    _assert_equal(
        evidence,
        _point_command(client, evidence, 9, 0, POINT),
        _field_error(0, ACCESS_ERROR),
    )
    _assert_inactive(client, evidence, State.PRIMED)
    session = client.new_session()
    _state(client, evidence, State.STAGING)
    _assert_equal(
        evidence,
        _point_command(client, evidence, 9, session, POINT),
        _field_error(0, ACCESS_ERROR),
    )
    _assert_point(client, evidence, OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    client.load_operational_point(session, POINT)
    _assert_equal(
        evidence,
        _point_command(client, evidence, 9, session, POINT),
        _field_error(0, ACCESS_ERROR),
    )
    _assert_point(client, evidence, POINT)
    _assert_equal(evidence, _release(client, evidence, session), (ACCESS_ERROR, OK))
    client.directive(Directive.STAGE_PLAN)
    _state(client, evidence, State.STAGED)
    _assert_equal(evidence, _release(client, evidence, session), (INVALID, OK))
    _assert_equal(evidence, _point_command(client, evidence, 9, session, POINT), (OK,) * 9)
    client.release_plan(session)
    _state(client, evidence, State.READY)
    _assert_equal(
        evidence,
        _point_command(client, evidence, 9, session, POINT),
        _field_error(0, ACCESS_ERROR),
    )
    ready = client.query_telemetry()
    evidence.record(
        "Confirmation rejected after release",
        observed_state=ready.state,
        expected_state=State.READY,
        observed_ma=ready.ma_feedback,
        expected_ma=0,
        tolerance_ma=0.01,
    )
    assert ready.state == State.READY
    assert ready.ma_feedback == pytest.approx(0, abs=0.01)


@pytest.mark.strictdoc("TC-H1FWMC-88")
def test_staged_five_minute_idle_timeout(
    client: MainControlClient,
    evidence: Evidence,
    io_model: HostIOModel,
) -> None:
    """
    @relation(RQ-H1FWMC-87, scope=function, role=Verifies)
    UID: TC-H1FWMC-88
    TITLE: Staged idle timeout
    STATEMENT: An idle staged plan returns to Cold after five minutes.
    PREREQUISITES: Healthy system with one scalar point loaded in Staged.
    STEPS: Withhold plan release and observe state and outputs for 300 seconds,
    using only telemetry queries to keep communication alive.
    EXPECTED_BEHAVIOR: Staged persists for five minutes, then discharges to
    healthy Cold without emission. Allow 1 second observation uncertainty and
    10 seconds for discharge and scheduling.
    """
    _staged(client, evidence)
    started = time.monotonic()
    departure = None
    states = []
    while True:
        observed = client.query_telemetry()
        elapsed = time.monotonic() - started
        if not states or states[-1] != observed.state:
            states.append(observed.state)
            evidence.record(
                "Staged idle state transition (s)",
                observed_state=observed.state,
                observed_elapsed_s=elapsed,
                allowed_states=(State.STAGED, State.DISCHARGE, State.COLD),
                expected_idle_s=300,
                observed_faults=observed.faults,
                expected_faults=0,
            )
        assert not observed.faults, observed
        emission_enabled = io_model.read()["gpio"]["pins"]["port_a"]["io_emission_en"]
        if emission_enabled:
            evidence.record(
                "Staged idle emission pin",
                observed_emission_enabled=emission_enabled,
                expected_emission_enabled=False,
                observed_elapsed_s=elapsed,
            )
        assert not emission_enabled
        assert observed.state in (State.STAGED, State.DISCHARGE, State.COLD), observed
        if observed.state != State.STAGED and departure is None:
            departure = elapsed
            evidence.record(
                "Staged idle departure (s)",
                observed_departure_s=departure,
                expected_departure_bounds_s=(299, 305),
                expected_idle_s=300,
            )
        if observed.state == State.COLD:
            evidence.record(
                "Staged idle Cold endpoint",
                observed_departure_s=departure,
                expected_departure_bounds_s=(299, 305),
                observed_elapsed_s=elapsed,
                maximum_elapsed_s=310,
                observed_states=states,
                observed_kv=observed.kv_setpoint,
                observed_heater_ma=observed.heater_setpoint,
                expected_outputs=0,
                observed_emission_enabled=emission_enabled,
                expected_emission_enabled=False,
            )
            assert departure is not None and 299 <= departure <= 305, (
                f"Staged ended after {departure:.3f} s; RQ-H1FWMC-87 requires 300 s"
            )
            assert observed.kv_setpoint == observed.heater_setpoint == 0
            break
        if elapsed > 310:
            evidence.record(
                "Staged idle timeout deadline (s)",
                observed_elapsed_s=elapsed,
                maximum_elapsed_s=310,
                observed_state=observed.state,
                expected_state=State.COLD,
            )
        assert elapsed <= 310, f"Staged timeout did not reach Cold after {elapsed:.3f} s"
        time.sleep(0.1)
