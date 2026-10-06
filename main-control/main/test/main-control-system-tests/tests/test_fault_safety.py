"""Fault safety through real PC commands and physical host I/O."""

from __future__ import annotations

from dataclasses import replace
import time

import pytest

from main_control_system_tests.hvps import HvpsCommandCode, HvpsSimulator
from main_control_system_tests.evidence import Evidence
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Directive, Fault, FaultMessage, OperationalPoint, State,
    authenticated_payload, float_word,
)
from main_control_system_tests.protocol import MainControlClient


ZERO_COMMANDS = (
    HvpsCommandCode.SET_KV, HvpsCommandCode.SET_POWER,
    HvpsCommandCode.SET_HEATER, HvpsCommandCode.SET_GRID,
)


def _first_fault(client: MainControlClient) -> FaultMessage:
    return FaultMessage.decode(client.command(2, 0))


def _assert_zero_outputs(model: dict, *, emission_disabled: bool = True) -> None:
    pins = model["gpio"]["pins"]["port_a"]
    assert not pins["io_hv_en"]
    assert not pins["io_indicators_en"]
    if emission_disabled:
        assert not pins["io_emission_en"]
        assert not pins["io_grid_en_n"]
    assert tuple(model["dac"]["coil"][axis] for axis in ("x", "y", "f")) == pytest.approx((0, 0, 0), abs=0.002)


def _record_zero_outputs(evidence: Evidence, model: dict, *, emission_disabled: bool = True) -> None:
    names = ["io_hv_en", "io_indicators_en"]
    if emission_disabled:
        names.extend(("io_emission_en", "io_grid_en_n"))
    evidence.record("Safe physical outputs", outputs={name: model["gpio"]["pins"]["port_a"][name] for name in names},
                    expected_outputs=False, coil_dac_v=tuple(model["dac"]["coil"][axis] for axis in ("x", "y", "f")),
                    expected_coil_dac_v=(0, 0, 0), tolerance_v=0.002)


def _wait_zero_commands(client: MainControlClient, hvps: HvpsSimulator, since: int, evidence: Evidence) -> None:
    deadline = time.monotonic() + 2
    while True:
        latest = {command.code: command for command in hvps.commands[since:]}
        if all(code in latest and latest[code].parameter == 0 for code in ZERO_COMMANDS):
            evidence.record("Safe HVPS commands", parameters={code: latest[code].parameter for code in ZERO_COMMANDS},
                            expected_parameters={code: 0 for code in ZERO_COMMANDS})
            return
        assert time.monotonic() < deadline, latest
        client.query_telemetry()
        time.sleep(0.02)


def _energized_emission(client: MainControlClient, io_model: HostIOModel, evidence: Evidence) -> int:
    point = replace(OperationalPoint.beam_qa(50, duration_s=30),
                    x_coil_ma=100, y_coil_ma=-100, focus_coil_ma=500)
    session = client.prepare_emission(point)
    client.start_emission(session)
    observed = client.wait_for_telemetry(
        lambda item: item.kv_feedback > 4 and item.heater_feedback > 700
        and item.timer_1_s > 0.1 and item.timer_2_s > 0.1,
        timeout=2,
    )
    assert observed.state == State.EMISSION and not observed.faults
    model = io_model.read()
    pins = model["gpio"]["pins"]["port_a"]
    evidence.record("Energized emission before fault", state=observed.state, expected_state=State.EMISSION,
                    faults=observed.faults, expected_faults=0, kv=observed.kv_feedback, minimum_kv=4,
                    heater_ma=observed.heater_feedback, minimum_heater_ma=700,
                    outputs={key: pins[key] for key in ("io_hv_en", "io_emission_en", "io_grid_en_n",
                             "io_indicators_en", "io_pump_en")}, expected_outputs=True,
                    timer_states=(model["backup_timer1"]["state"], model["backup_timer2"]["state"]),
                    expected_timer_states=(2, 2), coil_dac_v={axis: model["dac"]["coil"][axis] for axis in ("x", "y", "f")},
                    minimum_coil_dac_v=0)
    assert pins["io_hv_en"] and pins["io_emission_en"] and pins["io_grid_en_n"]
    assert pins["io_indicators_en"] and pins["io_pump_en"]
    assert model["backup_timer1"]["state"] == model["backup_timer2"]["state"] == 2
    assert all(model["dac"]["coil"][axis] > 0 for axis in ("x", "y", "f"))
    return session


@pytest.mark.strictdoc("TC-H1FWMC-123")
@pytest.mark.parametrize("scenario", ("cold-estop", "emission-estop", "emission-timer"))
def test_safe_fault_handling(
    client: MainControlClient, io_model: HostIOModel, hvps: HvpsSimulator, scenario: str, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-80, scope=function, role=Verifies)
    UID: TC-H1FWMC-123
    TITLE: Fault mode - Safe State Handling - Test Case

    STATEMENT: Physical safety faults and failed backup-timer communication
    select safe fault handling and prevent unsafe operating-state progression.

    PREREQUISITES: Healthy host, real PC commands and telemetry, adjustable
    physical emergency-stop inputs and backup-timer responses, and observable
    HVPS commands, interlocks, indicators, and coil outputs are available.

    STEPS:
    1. From Cold, open the base emergency stop; separately, during an energized
       emission, open the remote emergency stop or suppress backup-timer responses.
    2. Observe the corresponding fault record, safe state, and output commands.
    3. Restore the physical input or communication without clearing the fault.
    4. Request startup, warmup, a new session, staging, and, where a treatment
       session exists, emission release; monitor safe outputs for 0.5 seconds.
    5. Clear the fault through the PC and observe recovery to Cold.

    EXPECTED_BEHAVIOR: Cold enters Cold Fault; emission enters latched Fault
    after discharge. The fault category identifies the failed safety input or
    timer communication. HV, emission, grid, and indicators are disabled;
    HVPS and all coil outputs are commanded to zero. Restoring the cause does
    not permit operating-state progression: commands return access errors and
    safe outputs remain disabled until an accepted PC clear permits recovery.
    """
    client.enter_cold()
    session = None if scenario == "cold-estop" else _energized_emission(client, io_model, evidence)
    timer_failure = scenario == "emission-timer"
    stop_input = "io_base_estop_n" if scenario == "cold-estop" else "io_remote_estop_n"
    expected_fault = Fault.TIMER_COMM if timer_failure else Fault.INTERLOCK
    expected_state = State.COLD_FAULT if scenario == "cold-estop" else State.FAULT
    since = len(hvps.commands)
    try:
        if timer_failure:
            io_model.set_timer_fault(1, response_suppressed=True)
        else:
            io_model.set_interlock(stop_input, False)
        client.wait_for_telemetry(lambda item: bool(item.faults & expected_fault), timeout=2)
        faulted = client.wait_for_state(expected_state, timeout=3)
        record = _first_fault(client)
        evidence.record("Fault entry", scenario=scenario, state=faulted.state, expected_state=expected_state,
                        faults=faulted.faults, expected_fault=expected_fault, diagnostic=record.message,
                        category=1 << record.fault_type, expected_category=expected_fault,
                        recorded_state=record.state, expected_recorded_state=State.COLD if session is None else State.EMISSION)
        assert 1 << record.fault_type == expected_fault
        assert record.state == (State.COLD if session is None else State.EMISSION)
        _wait_zero_commands(client, hvps, since, evidence)
        model = io_model.wait_for(
            lambda item: all(abs(item["dac"]["coil"][axis]) < 0.002 for axis in ("x", "y", "f"))
        )
        _record_zero_outputs(evidence, model)
        _assert_zero_outputs(model)
    finally:
        if timer_failure:
            io_model.set_timer_fault(1)
        else:
            io_model.set_interlock(stop_input, True)

    commands = [
        (3, int(Directive.STARTUP_INIT), 1 << int(Directive.STARTUP_INIT)),
        (6, float_word(2500)),
        (7,),
        (3, int(Directive.STAGE_PLAN), 1 << int(Directive.STAGE_PLAN)),
    ]
    if session is not None:
        commands.append((11, *authenticated_payload(session, 11, (2,))))
    for command in commands:
        response = client.command(*command)
        evidence.record("Latched fault command rejection", command_type=command[0],
                        status=response.payload[0], expected_status=1)
        assert response.payload[0] == 1, response
        rejected = client.query_telemetry()
        evidence.record("Rejected command retains state", state=rejected.state, expected_state=expected_state)
        assert rejected.state == expected_state
    deadline = time.monotonic() + 0.5
    while True:
        telemetry = client.query_telemetry()
        assert telemetry.state == expected_state and telemetry.faults == faulted.faults
        retained = _first_fault(client)
        assert retained == record
        model = io_model.read()
        _assert_zero_outputs(model)
        if time.monotonic() >= deadline:
            break
        time.sleep(0.025)
    evidence.record("Safe fault latch observation", state=telemetry.state, expected_state=expected_state,
                    faults=telemetry.faults, expected_faults=faulted.faults, observed_record=retained,
                    expected_record=record, elapsed_s=time.monotonic() - (deadline - 0.5), minimum_observation_s=0.5)
    _record_zero_outputs(evidence, model)
    client.clear_faults()
    recovered = client.wait_for_state(State.COLD)
    evidence.record("Accepted clear recovery", state=recovered.state, expected_state=State.COLD,
                    faults=recovered.faults, expected_faults=0)
    assert not recovered.faults


@pytest.mark.strictdoc("TC-H1FWMC-124")
def test_fault_records_latch_after_estop_release(client: MainControlClient, io_model: HostIOModel, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-79, scope=function, role=Verifies)
    UID: TC-H1FWMC-124
    TITLE: Fault information - Fault Latching - Test Case

    STATEMENT: Releasing an emergency stop does not clear fault state or records.

    PREREQUISITES: Healthy Cold system, physical emergency-stop control,
    PC fault queries, and telemetry are available.

    STEPS:
    1. Open the base emergency stop and record the fault category and diagnostic.
    2. Release the emergency stop and monitor state and fault records for 60 seconds.

    EXPECTED_BEHAVIOR: The physical stop is released, but Cold Fault, the
    interlock category flag, and the original detailed record remain latched
    throughout the observation period without a PC clear command.
    """
    io_model.set_interlock("io_base_estop_n", False)
    faulted = client.wait_for_state(State.COLD_FAULT)
    record = _first_fault(client)
    assert faulted.faults & Fault.INTERLOCK
    assert 1 << record.fault_type == Fault.INTERLOCK
    io_model.set_interlock("io_base_estop_n", True)
    released = io_model.read()["gpio"]["pins"]["port_c"]["io_base_estop_n"]
    evidence.record("Released physical emergency stop", base_estop_n=released, expected=True,
                    initial_faults=faulted.faults, expected_fault=Fault.INTERLOCK, diagnostic=record.message)
    assert released
    deadline = time.monotonic() + 60
    while True:
        telemetry = client.query_telemetry()
        assert telemetry.state == State.COLD_FAULT
        assert telemetry.faults == faulted.faults
        retained = _first_fault(client)
        assert retained == record
        if time.monotonic() >= deadline:
            break
        time.sleep(0.1)
    evidence.record("Fault remains latched for 60 s", state=telemetry.state, expected_state=State.COLD_FAULT,
                    faults=telemetry.faults, expected_faults=faulted.faults, observed_record=retained,
                    expected_record=record, elapsed_s=time.monotonic() - (deadline - 60), minimum_observation_s=60)


@pytest.mark.strictdoc("TC-H1FWMC-125")
def test_clear_fault_confirmation_and_hvps_request(
    client: MainControlClient, io_model: HostIOModel, hvps: HvpsSimulator, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-78, scope=function, role=Verifies)
    UID: TC-H1FWMC-125
    TITLE: Fault information - Fault Clearing - Test Case

    STATEMENT: A confirmed clear removes the latched fault and requests HVPS
    clearing without waiting for an HVPS response; bad confirmation clears neither.

    PREREQUISITES: PC directives and fault reports, emergency-stop input, HVPS
    command capture, and independently suppressible HVPS responses are available.

    STEPS:
    1. Open then release the emergency stop and verify the fault remains latched.
    2. Suppress HVPS responses, send a confirmed clear, and capture both links.
    3. Restore HVPS responses and recreate/release the fault; send a mismatched clear.

    EXPECTED_BEHAVIOR: A successful PC response precedes an empty active-fault
    update and Cold state, without an HVPS response. Exactly one zero-parameter
    HVPS clear is sent. Invalid confirmation returns invalid status, retains the
    fault record and category, and sends no additional HVPS clear.
    """
    io_model.set_interlock("io_base_estop_n", False)
    client.wait_for_state(State.COLD_FAULT)
    record = _first_fault(client)
    assert 1 << record.fault_type == Fault.INTERLOCK
    deadline = time.monotonic() + 1
    while True:
        published = client.fault_message(timeout=max(0.01, deadline - time.monotonic()))
        if published == record:
            break
        assert time.monotonic() < deadline, published
    assert _first_fault(client) == record  # Still active while the stop is open.
    io_model.set_interlock("io_base_estop_n", True)
    assert _first_fault(client) == record
    before = sum(command.code == HvpsCommandCode.CLEAR_FAULTS for command in hvps.commands)
    hvps.set_transmitting(False)
    try:
        response = client.clear_faults()
        evidence.record("Confirmed PC clear response", response=response.payload, expected_response=(0,))
        assert response.payload == (0,)
        deadline = time.monotonic() + 1
        while True:
            update = client.fault_message(timeout=max(0.01, deadline - time.monotonic()))
            if update.clear_epoch == (record.clear_epoch + 1) & 0xFFFFFFFF:
                break
            assert time.monotonic() < deadline, update
        assert update.active_count == update.fault_type == 0
        evidence.record("Clear publication without HVPS response", active_count=update.active_count,
                        expected_active_count=0, fault_type=update.fault_type, expected_fault_type=0,
                        epoch=update.clear_epoch, expected_epoch=(record.clear_epoch + 1) & 0xFFFFFFFF)
        assert not client.wait_for_state(State.COLD, timeout=1).faults
        while sum(command.code == HvpsCommandCode.CLEAR_FAULTS for command in hvps.commands) == before:
            assert time.monotonic() < deadline, "No HVPS clear command captured"
            client.query_telemetry()
            time.sleep(0.01)
        clears = [command for command in hvps.commands if command.code == HvpsCommandCode.CLEAR_FAULTS]
        evidence.record("HVPS clear command", count=len(clears), expected_count=before + 1,
                        parameter=clears[-1].parameter, integer=clears[-1].integer, expected_parameter=0,
                        expected_integer=0)
        assert len(clears) == before + 1
        assert clears[-1].parameter == 0 and clears[-1].integer == 0
    finally:
        hvps.set_transmitting(True)

    io_model.set_interlock("io_base_estop_n", False)
    faulted = client.wait_for_state(State.COLD_FAULT)
    retained = _first_fault(client)
    io_model.set_interlock("io_base_estop_n", True)
    response = client.command(3, int(Directive.CLEAR_FAULTS), 0)
    evidence.record("Invalid clear confirmation", response=response.payload, expected_response=(3,),
                    retained_diagnostic=retained.message)
    assert response.payload == (3,)
    deadline = time.monotonic() + 0.5
    while True:
        active = _first_fault(client)
        assert active == retained
        telemetry = client.query_telemetry()
        assert telemetry.state == State.COLD_FAULT and telemetry.faults == faulted.faults
        clear_count = sum(command.code == HvpsCommandCode.CLEAR_FAULTS for command in hvps.commands)
        assert clear_count == before + 1
        if time.monotonic() >= deadline:
            break
        time.sleep(0.02)
    evidence.record("Invalid clear retains fault and command count", observed_record=active, expected_record=retained,
                    state=telemetry.state, expected_state=State.COLD_FAULT, faults=telemetry.faults,
                    expected_faults=faulted.faults, clear_count=clear_count, expected_clear_count=before + 1,
                    elapsed_s=time.monotonic() - (deadline - 0.5), minimum_observation_s=0.5)


@pytest.mark.strictdoc("TC-H1FWMC-126")
def test_fault_emission_discharge_boundaries(
    client: MainControlClient, io_model: HostIOModel, hvps: HvpsSimulator, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-77, scope=function, role=Verifies)
    UID: TC-H1FWMC-126
    TITLE: Fault mode - Emission Termination and Safe Discharge - Test Case

    STATEMENT: An energized emission stops safely and remains in Fault Discharge
    until voltage, filament feedback, and HVPS warming permit latched Fault.

    PREREQUISITES: A 30-second emission plan, controlled HVPS feedback, command
    capture, physical outputs, cabinet fan, and backup timers are observable.

    STEPS:
    1. Start emission with energized kV/filament and active timers/indicators.
    2. Hold energized feedback, open the emergency stop, and inspect safe outputs.
    3. Hold each blocking boundary separately for 0.5 seconds: 4 kV with 700 mA,
       3.9 kV with 701 mA, then 3.9 kV with 700 mA and warming asserted.
    4. Clear warming at 3.9 kV and 700 mA and observe the final state.

    EXPECTED_BEHAVIOR: The fault pauses timers, disables HV and indicators,
    and commands kV, filament, grid, and all coils to zero. Each blocking
    condition retains Fault Discharge and a 4.9 V high-speed cabinet-fan command.
    Only all-safe feedback permits Fault, which disables emission/grid interlocks
    and retains the zero output commands.
    """
    _energized_emission(client, io_model, evidence)
    hvps.set_feedback(kv=50, heater=2500)
    since = len(hvps.commands)
    try:
        io_model.set_interlock("io_base_estop_n", False)
        client.wait_for_state(State.FAULT_DISCHARGE)
        model = io_model.wait_for(
            lambda item: item["backup_timer1"]["state"] == 1
            and item["backup_timer2"]["state"] == 1
            and all(abs(item["dac"]["coil"][axis]) < 0.002 for axis in ("x", "y", "f"))
        )
        _record_zero_outputs(evidence, model, emission_disabled=False)
        _assert_zero_outputs(model, emission_disabled=False)
        assert model["gpio"]["pins"]["port_a"]["io_timers_start_n"]
        _wait_zero_commands(client, hvps, since, evidence)
        paused = client.wait_for_telemetry(lambda item: item.timer_1_state == item.timer_2_state == 1)
        for kv, heater, warming in ((4.0, 700.0, False), (3.9, 701.0, False), (3.9, 700.0, True)):
            hvps.set_warming(warming)
            hvps.set_feedback(kv=kv, heater=heater)
            observed = client.wait_for_telemetry(
                lambda item: abs(item.kv_feedback - kv) < 0.01
                and abs(item.heater_feedback - heater) < 0.1
                and bool(item.hvps_flags & (1 << 3)) == warming,
                timeout=1,
            )
            deadline = time.monotonic() + 0.5
            while True:
                assert observed.state == State.FAULT_DISCHARGE
                assert observed.timer_1_s == paused.timer_1_s
                assert observed.timer_2_s == paused.timer_2_s
                model = io_model.read()
                _assert_zero_outputs(model, emission_disabled=False)
                assert model["dac"]["fan"]["cabinet"] == pytest.approx(4.9, abs=0.01)
                if time.monotonic() >= deadline:
                    break
                time.sleep(0.025)
                observed = client.query_telemetry()
            evidence.record("Discharge blocking boundary (kV, mA, s)", kv=observed.kv_feedback, expected_kv=kv,
                            heater_ma=observed.heater_feedback, expected_heater_ma=heater,
                            warming=bool(observed.hvps_flags & (1 << 3)), expected_warming=warming,
                            state=observed.state, expected_state=State.FAULT_DISCHARGE,
                            timer_s=(observed.timer_1_s, observed.timer_2_s),
                            expected_timer_s=(paused.timer_1_s, paused.timer_2_s),
                            cabinet_fan_v=model["dac"]["fan"]["cabinet"], expected_fan_v=4.9, tolerance_v=0.01,
                            elapsed_s=time.monotonic() - (deadline - 0.5), minimum_observation_s=0.5)
            _record_zero_outputs(evidence, model, emission_disabled=False)
        hvps.set_warming(False)
        faulted = client.wait_for_state(State.FAULT, timeout=2)
        evidence.record("Safe discharge completed", state=faulted.state, expected_state=State.FAULT,
                        kv=faulted.kv_feedback, exclusive_maximum_kv=4, heater_ma=faulted.heater_feedback,
                        maximum_heater_ma=700, warming=bool(faulted.hvps_flags & (1 << 3)), expected_warming=False,
                        setpoints=(faulted.kv_setpoint, faulted.heater_setpoint, faulted.grid_setpoint),
                        expected_setpoints=(0, 0, 0))
        assert faulted.kv_feedback < 4 and faulted.heater_feedback <= 700
        assert not faulted.hvps_flags & (1 << 3)
        model = io_model.read()
        _record_zero_outputs(evidence, model)
        _assert_zero_outputs(model)
        assert faulted.kv_setpoint == faulted.heater_setpoint == faulted.grid_setpoint == 0
        _wait_zero_commands(client, hvps, since, evidence)
    finally:
        hvps.set_warming(None)
        hvps.set_feedback(kv=None, heater=None)
        io_model.set_interlock("io_base_estop_n", True)
