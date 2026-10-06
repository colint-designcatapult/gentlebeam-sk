"""Physical interlock and emission-indicator host system protocols."""

from __future__ import annotations

import time

import pytest

from main_control_system_tests.hvps import HvpsCommandCode, HvpsSimulator
from main_control_system_tests.evidence import Evidence
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Directive, Fault, FaultMessage, OperationalPoint, State, authenticated_payload, float_word,
)
from main_control_system_tests.protocol import MainControlClient


# Physical Port C inputs from system_parameters.h; PC17 is an output, not an input.
INTERLOCKS = (
    ("io_door_closed", 0), ("io_drive_sys_locked", 1),
    ("io_base_estop_n", 2), ("io_remote_estop_n", 3),
    ("io_kuka_fault_1_n", 4), ("io_kuka_fault_2_n", 5),
    ("io_water_level", 6), ("io_ion_pump_hvon", 7),
    ("io_timer_fault_1_n", 8), ("io_timer_fault2_n", 9),
    ("io_hvps_fault_n", 10), ("io_cooler_fault_n", 11),
    ("io_water_temp_fault_n", 12), ("io_wd_fault_n", 13),
    ("io_mcu_fault_n", 14), ("spare_interlock_1", 15),
    ("io_master_fault_n", 16), ("io_remote_key", 18), ("io_base_key", 19),
)
PHYSICAL_INPUT_MASK = sum(1 << bit for _, bit in INTERLOCKS)
COLD_REQUIRED_MASK = 0x3FCC
READY_REQUIRED_MASK = 0xC3FCD
STATE_DEPENDENT_INPUTS = (INTERLOCKS[0], INTERLOCKS[-2], INTERLOCKS[-1])
ESTOPS = (INTERLOCKS[2], INTERLOCKS[3])


def _indicators(model: dict) -> tuple[bool, bool]:
    pins = model["gpio"]["pins"]
    return pins["port_a"]["io_indicators_en"], pins["port_d"]["io_led6"]


def _interlock_records(client: MainControlClient) -> list[FaultMessage]:
    first = FaultMessage.decode(client.command(2, 0))
    records = [first] + [
        FaultMessage.decode(client.command(2, index))
        for index in range(1, first.active_count)
    ]
    return [record for record in records if (1 << record.fault_type) == Fault.INTERLOCK]


@pytest.mark.strictdoc("TC-H1FWMC-93")
@pytest.mark.parametrize("blocked_input", (None, "io_door_closed", "io_remote_key", "io_base_key"))
def test_emission_indicators(
    client: MainControlClient, io_model: HostIOModel, blocked_input: str | None, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-7, scope=function, role=Verifies)
    UID: TC-H1FWMC-93
    TITLE: Emission indicator - Test Case
    STATEMENT: Indicators follow accepted emission and remain off for a rejected start.
    PREREQUISITES: Normal-mode PC commands, physical inputs, indicator outputs,
    LED6, and HVPS feedback are accessible; all readiness inputs are initially good.
    STEPS:
    1. Prepare a 50 kV, 5 second emission and verify both indicators are off.
    2. Release emission with all inputs good; observe emission for 1 second, then stop.
    3. Repeat preparation with the door, remote key, or base key open before release.
    EXPECTED_BEHAVIOR: Both indicators remain on throughout the observed emission
    and turn off within 1 second of stop. Each rejected release leaves both off,
    prevents emission, and reports an interlock fault within 1 second.
    """
    initial_indicators = _indicators(io_model.read())
    evidence.record("Initial emission indicators", observed=initial_indicators, expected=(False, False))
    assert initial_indicators == (False, False)
    session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=5))
    ready = client.query_telemetry()
    evidence.record("Emission-ready interlocks", state=ready.state, expected_state=State.READY,
                    faults=ready.faults, expected_faults=0, interlocks=ready.interlocks,
                    required_interlocks=ready.required_interlocks)
    assert ready.state == State.READY and not ready.faults
    assert ready.interlocks & ready.required_interlocks == ready.required_interlocks
    ready_indicators = _indicators(io_model.read())
    evidence.record("Ready emission indicators", observed=ready_indicators, expected=(False, False))
    assert ready_indicators == (False, False)

    if blocked_input is not None:
        try:
            io_model.set_interlock(blocked_input, False)
            # The monitor can fault before release reaches the command gate.
            # Regardless of command acceptance, the bad input must prevent emission.
            client.command(11, *authenticated_payload(session, 11, (2,)))
            fault = client.wait_for_telemetry(
                lambda item: bool(item.faults), timeout=1,
            )
            evidence.record("Blocked emission fault", input=blocked_input, state=fault.state,
                            forbidden_states=(State.LAUNCHING, State.EMISSION), faults=fault.faults)
            assert fault.state not in (State.LAUNCHING, State.EMISSION)
            deadline = time.monotonic() + 1
            while time.monotonic() < deadline:
                observed = client.query_telemetry()
                assert observed.state not in (State.LAUNCHING, State.EMISSION)
                assert observed.ma_feedback == pytest.approx(0, abs=0.01)
                model = io_model.read()
                assert _indicators(model) == (False, False)
                assert not model["gpio"]["pins"]["port_a"]["io_emission_en"]
                time.sleep(0.02)
            evidence.record("Rejected release observation", input=blocked_input, state=observed.state,
                            forbidden_states=(State.LAUNCHING, State.EMISSION), ma=observed.ma_feedback,
                            expected_ma=0, tolerance_ma=0.01, indicators=_indicators(model),
                            expected_indicators=(False, False),
                            emission_enable=model["gpio"]["pins"]["port_a"]["io_emission_en"],
                            expected_emission_enable=False, elapsed_s=time.monotonic() - (deadline - 1),
                            minimum_observation_s=1)
        finally:
            io_model.set_interlock(blocked_input, True)
            client.enter_cold()
        return

    client.start_emission(session)
    client.wait_for_telemetry(lambda item: item.ma_feedback > 0, timeout=1)
    deadline = time.monotonic() + 1
    try:
        while time.monotonic() < deadline:
            observed = client.query_telemetry()
            assert observed.state == State.EMISSION and not observed.faults
            indicators = _indicators(io_model.read())
            assert indicators == (True, True)
            time.sleep(0.02)
        evidence.record("Accepted emission indicator observation", state=observed.state, expected_state=State.EMISSION,
                        faults=observed.faults, expected_faults=0, indicators=indicators,
                        expected_indicators=(True, True), elapsed_s=time.monotonic() - (deadline - 1),
                        minimum_observation_s=1)
    finally:
        client.directive(Directive.STOP)
    stopped_model = io_model.wait_for(lambda model: _indicators(model) == (False, False), timeout=1)
    stopped = client.wait_for_telemetry(
        lambda item: item.state not in (State.LAUNCHING, State.EMISSION)
        and abs(item.ma_feedback) < 0.01, timeout=1,
    )
    evidence.record("Stopped emission indicators", state=stopped.state,
                    forbidden_states=(State.LAUNCHING, State.EMISSION), ma=stopped.ma_feedback,
                    expected_ma=0, tolerance_ma=0.01, indicators=_indicators(stopped_model),
                    expected_indicators=(False, False), timeout_s=1)
    client.enter_cold()


INTERLOCK_CASES = (
    [("cold", name, bit) for name, bit in INTERLOCKS]
    + [("ready", name, bit) for name, bit in STATE_DEPENDENT_INPUTS]
    + [(operation, name, bit) for operation in ("condition", "warmup") for name, bit in ESTOPS]
)


@pytest.mark.strictdoc("TC-H1FWMC-94")
@pytest.mark.parametrize(
    "operation,name,bit", INTERLOCK_CASES,
    ids=[f"{operation}-{name}" for operation, name, _ in INTERLOCK_CASES],
)
def test_physical_interlocks(
    client: MainControlClient, io_model: HostIOModel, hvps: HvpsSimulator,
    operation: str, name: str, bit: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-8, scope=function, role=Verifies)
    UID: TC-H1FWMC-94
    TITLE: Interlocks - Test Case
    STATEMENT: Every physical interlock is reported; required inputs fault and
    either open e-stop prevents conditioning and warmup energization.
    PREREQUISITES: Normal-mode firmware is cold with healthy inputs. Independent
    physical input drive, PC commands, telemetry, and HVPS commands are accessible.
    STEPS:
    1. Open and restore each of the 19 physical Port C inputs in cold.
    2. Repeat for door, remote key, and base key in emission-ready state.
    3. Request conditioning and warmup at 2500 mA with each e-stop open.
    4. Restore each input and clear faults through PC commands.
    EXPECTED_BEHAVIOR: Each changed input is reported within 1 second without
    changing other input reports. Required inputs produce a typed interlock fault;
    non-required inputs remain reported without a fault for 1 second. Door and
    keys become required when ready. Open e-stops never permit heater or pump
    energization. Restoring inputs and clearing faults returns to healthy cold.
    """
    # Disable only the external latch/timer-pin synthesis: firmware monitoring
    # remains active. This isolates non-required inputs from aggregate hardware
    # latch consequences and permits driving the buffered master diagnostic.
    io_model.set_gpio_simulation(False)
    if operation == "ready":
        client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=5))
    initial = client.query_telemetry()
    expected_mask = READY_REQUIRED_MASK if operation == "ready" else COLD_REQUIRED_MASK
    evidence.record("Initial physical interlocks", operation=operation, input=name, bit=bit,
                    required_mask=initial.required_interlocks, expected_required_mask=expected_mask,
                    input_mask=initial.interlocks, expected_input_mask=PHYSICAL_INPUT_MASK,
                    faults=initial.faults, expected_faults=0)
    assert initial.required_interlocks == expected_mask
    assert initial.interlocks == PHYSICAL_INPUT_MASK
    assert not initial.faults
    required = bool(expected_mask & (1 << bit))
    commands_before = len(hvps.commands)
    started = time.monotonic()
    try:
        io_model.set_interlock(name, False)
        if operation in ("condition", "warmup"):
            # Safety monitoring may reject at the command-state gate or at the
            # state transition; both must leave the physical loads de-energized.
            if operation == "condition":
                response = client.command(5, float_word(2500), 0)
            else:
                response = client.command(6, float_word(2500))
            # SPR_OK queues the transition; SPR_ACCESS_ERROR rejects a state
            # already faulted by the independent interlock monitor.
            evidence.record("Open e-stop command", operation=operation, response=response.payload,
                            allowed_status=(0, 1), expected_remaining_payload=0)
            assert response.payload[0] in (0, 1), response
            assert not any(response.payload[1:]), response
        opened = client.wait_for_telemetry(
            lambda item: item.interlocks == PHYSICAL_INPUT_MASK & ~(1 << bit),
            timeout=max(0.001, started + 1 - time.monotonic()),
        )
        evidence.record("Opened physical input", input=name, mask=opened.interlocks,
                        expected_mask=PHYSICAL_INPUT_MASK & ~(1 << bit), excluded_output_bit=17,
                        elapsed_s=time.monotonic() - started, timeout_s=1)
        assert not opened.interlocks & (1 << 17)
        if required:
            client.wait_for_telemetry(
                lambda item: bool(item.faults & Fault.INTERLOCK),
                timeout=max(0.001, started + 1 - time.monotonic()),
            )
            records = _interlock_records(client)
            evidence.record("Typed interlock diagnostics", arguments=[record.arguments for record in records],
                            categories=[1 << record.fault_type for record in records],
                            expected_category=Fault.INTERLOCK, expected_open_bit=bit, expected_required_bit=bit)
            assert any(
                len(record.arguments) == 2
                and not record.arguments[0] & (1 << bit)
                and record.arguments[1] & (1 << bit)
                for record in records
            ), records
        deadline = time.monotonic() + 1
        while time.monotonic() < deadline:
            observed = client.query_telemetry()
            assert observed.interlocks == PHYSICAL_INPUT_MASK & ~(1 << bit)
            if not required:
                assert not observed.faults
                assert observed.state == State.COLD
            if operation in ("condition", "warmup"):
                assert observed.state in (State.COLD, State.COLD_FAULT)
                assert observed.heater_setpoint == 0
                pump_enabled = io_model.read()["gpio"]["pins"]["port_a"]["io_pump_en"]
                assert not pump_enabled
                assert not any(
                    command.code == HvpsCommandCode.SET_HEATER and command.parameter > 0
                    for command in hvps.commands[commands_before:]
                )
            time.sleep(0.02)
        evidence.record("Open-input observation", input=name, state=observed.state,
                        expected_state_if_not_required=State.COLD, required_input=required,
                        interlocks=observed.interlocks, expected_interlocks=PHYSICAL_INPUT_MASK & ~(1 << bit),
                        faults=observed.faults, expected_fault=Fault.INTERLOCK if required else 0,
                        heater_setpoint_ma=observed.heater_setpoint,
                        elapsed_s=time.monotonic() - (deadline - 1), minimum_observation_s=1)
        if operation in ("condition", "warmup"):
            evidence.record("E-stop prevents load energization", pump_enabled=pump_enabled, expected_pump_enabled=False,
                            heater_setpoint_ma=observed.heater_setpoint, expected_heater_setpoint_ma=0,
                            heater_commands_ma=[command.parameter for command in hvps.commands[commands_before:]
                                                if command.code == HvpsCommandCode.SET_HEATER],
                            maximum_heater_command_ma=0)
    finally:
        io_model.set_interlock(name, True)
        restored = client.wait_for_telemetry(
            lambda item: item.interlocks == PHYSICAL_INPUT_MASK, timeout=1,
        )
        client.enter_cold()
        client.clear_faults()
        recovered = client.wait_for_telemetry(lambda item: item.state == State.COLD and not item.faults)
        evidence.record("Restored interlock and cleared faults", interlocks=restored.interlocks,
                        expected_interlocks=PHYSICAL_INPUT_MASK, state=recovered.state,
                        expected_state=State.COLD, faults=recovered.faults, expected_faults=0)
        io_model.set_gpio_simulation(True)
