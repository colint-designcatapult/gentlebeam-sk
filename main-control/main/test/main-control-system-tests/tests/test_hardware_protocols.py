"""Treatment hardware protocols exercised through PC, UART, and physical I/O."""

from __future__ import annotations

from dataclasses import replace
import math
import time

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.head_interface import HeadInterfaceSimulator
from main_control_system_tests.hvps import HvpsCommandCode, HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Directive,
    Fault,
    OperationalPoint,
    State,
    float_word,
)
from main_control_system_tests.protocol import MainControlClient


def _hold_healthy(client: MainControlClient, state: State, seconds: float, evidence: Evidence):
    snapshot = client.query_telemetry()
    until = snapshot.runtime_ms + seconds * 1000
    deadline = time.monotonic() + seconds + 2
    first_runtime_ms = snapshot.runtime_ms
    try:
        while True:
            assert snapshot.state == state and not snapshot.faults, snapshot
            if snapshot.runtime_ms >= until:
                return snapshot
            assert time.monotonic() < deadline, "Firmware runtime stopped advancing"
            time.sleep(0.025)
            snapshot = client.query_telemetry()
    finally:
        evidence.record(
            "Healthy state hold (s)", observed_state=snapshot.state, expected_state=state,
            observed_faults=snapshot.faults, expected_faults=Fault(0),
            observed_duration_s=(snapshot.runtime_ms - first_runtime_ms) / 1000,
            expected_minimum_s=seconds,
        )


@pytest.mark.strictdoc("TC-H1FWMC-109")
def test_hvps_interlocks_cycle_before_voltage_ramp(
    client: MainControlClient, io_model: HostIOModel, hvps: HvpsSimulator,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-21, scope=function, role=Verifies)
    UID: TC-H1FWMC-109
    TITLE: Operation 06 - HVPS Check - Test Case

    STATEMENT: Releasing a confirmed plan cycles the HVPS interlocks before
    high voltage rises, including after an open door has latched the interlock.

    PREREQUISITES: Healthy system, door input, HVPS UART, PC commands and telemetry.

    STEPS:
    1. Warm up, reset timers, load and confirm a 50 kV, 20-second plan.
    2. Hold the door open for 1 second without a software fault, then close it.
    3. Release the plan; observe HVPS Check and the HVPS interlock commands.
    4. Wait for Ready, then release the point and observe emission.

    EXPECTED_BEHAVIOR: Closing the door alone leaves the hardware latch set.
    The HVPS check clears the latch and tests both interlocks before commanding
    50 kV. Ready and Emission are reached without faults.
    """
    point = OperationalPoint.beam_qa(50, duration_s=20)
    client.warmup()
    client.wait_for_state(State.PRIMED, timeout=20)
    client.directive(Directive.RESET_TIMERS)
    session = client.new_session()
    client.wait_for_state(State.STAGING)
    client.load_operational_point(session, point)
    client.directive(Directive.STAGE_PLAN)
    client.wait_for_state(State.STAGED)
    client.confirm_operational_point(session, point)
    try:
        io_model.set_interlock("io_door_closed", False)
        _hold_healthy(client, State.STAGED, 1.0, evidence)
        door_open_latch = io_model.read()["gpio"]["pins"]["port_c"]["io_master_fault_n"]
        evidence.record("Door open hardware latch", observed_master_fault_n=door_open_latch,
                        expected_master_fault_n=False)
        assert not door_open_latch
        io_model.set_interlock("io_door_closed", True)
        door_closed_latch = io_model.read()["gpio"]["pins"]["port_c"]["io_master_fault_n"]
        evidence.record("Door closed hardware latch", observed_master_fault_n=door_closed_latch,
                        expected_master_fault_n=False)
        assert not door_closed_latch
        first_command = len(hvps.commands)
        client.release_plan(session)
        checking = client.wait_for_state(State.HVPS_CHECK, timeout=1)
        evidence.record("HVPS check before ramp (kV)", observed_state=checking.state,
                        expected_state=State.HVPS_CHECK, observed_faults=checking.faults,
                        expected_faults=Fault(0), observed_kv=checking.kv_feedback, expected_maximum_exclusive_kv=4)
        assert not checking.faults and checking.kv_feedback < 4
        ready = client.wait_for_state(State.READY, timeout=20)
        evidence.record("HVPS ready voltage (kV)", observed_state=ready.state, expected_state=State.READY,
                        observed_faults=ready.faults, expected_faults=Fault(0),
                        observed_kv=ready.kv_feedback, expected_kv=50, tolerance_kv=0.1)
        assert not ready.faults
        assert ready.kv_feedback == pytest.approx(50, abs=0.1)
        ready_latch = io_model.read()["gpio"]["pins"]["port_c"]["io_master_fault_n"]
        evidence.record("HVPS latch cleared", observed_master_fault_n=ready_latch, expected_master_fault_n=True)
        assert ready_latch
        commands = hvps.commands[first_command:]
        evidence.record("HVPS interlock and ramp command sequence",
                        observed_commands=[(command.code, command.integer, command.parameter)
                                           for command in commands if command.code in (
                                               HvpsCommandCode.CLEAR_FAULTS, HvpsCommandCode.INTERLOCK_TEST,
                                               HvpsCommandCode.SET_KV)],
                        expected_order=("CLEAR_FAULTS", "INTERLOCK_TEST 123", "INTERLOCK_TEST 456", "SET_KV 50"))
        clear = next(i for i, command in enumerate(commands)
                     if command.code == HvpsCommandCode.CLEAR_FAULTS)
        hv_test = next(i for i, command in enumerate(commands)
                       if command.code == HvpsCommandCode.INTERLOCK_TEST and command.integer == 123)
        grid_test = next(i for i, command in enumerate(commands)
                         if command.code == HvpsCommandCode.INTERLOCK_TEST and command.integer == 456)
        ramp = next(i for i, command in enumerate(commands)
                    if command.code == HvpsCommandCode.SET_KV and command.parameter == 50)
        evidence.record("HVPS command ordering", observed_indices=(clear, hv_test, grid_test, ramp),
                        expected_relation="clear < hv_test < grid_test < ramp")
        assert clear < hv_test < grid_test < ramp
        started = client.start_emission(session)
        evidence.record("Emission started", observed_state=started.state, expected_state=State.EMISSION,
                        observed_faults=started.faults, expected_faults=Fault(0))
        assert started.state == State.EMISSION and not started.faults
        _hold_healthy(client, State.EMISSION, 0.3, evidence)
    finally:
        io_model.set_interlock("io_door_closed", True)
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-130")
def test_both_backup_timers_start_with_emission(
    client: MainControlClient, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-72, scope=function, role=Verifies)
    UID: TC-H1FWMC-130
    TITLE: Operation 10 - Emission - Backup Timer Start - Test Case

    STATEMENT: Both backup timers begin counting only when emission starts.

    PREREQUISITES: Healthy system and a confirmed 20-second plan in Ready.

    STEPS: Hold Ready for 0.3 seconds, release the point, and compare both
    physical countdowns and elapsed-time telemetry across 0.5 seconds of emission.

    EXPECTED_BEHAVIOR: Both countdowns remain paused in Ready. During emission
    both run, their remaining seconds decrease, and both reported elapsed times
    increase at one second per second within 0.2 seconds of observation tolerance.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=20))
    paused = io_model.read()
    held = _hold_healthy(client, State.READY, 0.3, evidence)
    evidence.record("Ready elapsed timers (s)", observed_timers=(held.timer_1_s, held.timer_2_s),
                    expected_timers=(0, 0))
    assert held.timer_1_s == held.timer_2_s == 0
    for timer in (1, 2):
        remaining = io_model.backup_timer_seconds(timer)
        expected_remaining = io_model.backup_timer_seconds(timer, paused)
        evidence.record("Ready backup timer paused (s)", timer=timer,
                        observed_state=paused[f"backup_timer{timer}"]["state"], expected_state=1,
                        observed_remaining_s=remaining, expected_remaining_s=expected_remaining)
        assert paused[f"backup_timer{timer}"]["state"] == 1
        assert remaining == expected_remaining
    try:
        client.start_emission(session)
        before = client.wait_for_telemetry(
            lambda item: item.timer_1_s > 0.1 and item.timer_2_s > 0.1, timeout=1,
        )
        physical_before = io_model.read()
        after = _hold_healthy(client, State.EMISSION, 0.5, evidence)
        physical_after = io_model.read()
        elapsed = (after.runtime_ms - before.runtime_ms) / 1000
        for timer in (1, 2):
            evidence.record("Emission backup timer state", timer=timer,
                            observed_before_state=physical_before[f"backup_timer{timer}"]["state"],
                            observed_after_state=physical_after[f"backup_timer{timer}"]["state"], expected_state=2)
            assert physical_before[f"backup_timer{timer}"]["state"] == 2
            assert physical_after[f"backup_timer{timer}"]["state"] == 2
            countdown = (io_model.backup_timer_seconds(timer, physical_before)
                         - io_model.backup_timer_seconds(timer, physical_after))
            evidence.record("Emission physical countdown delta (s)", timer=timer,
                            observed_countdown_s=countdown, expected_elapsed_s=elapsed, tolerance_s=0.2)
            assert countdown == pytest.approx(elapsed, abs=0.2)
            progress = getattr(after, f"timer_{timer}_s") - getattr(before, f"timer_{timer}_s")
            evidence.record("Emission reported timer delta (s)", timer=timer,
                            observed_before_s=getattr(before, f"timer_{timer}_s"),
                            observed_after_s=getattr(after, f"timer_{timer}_s"),
                            observed_progress_s=progress, expected_elapsed_s=elapsed, tolerance_s=0.2)
            assert progress == pytest.approx(elapsed, abs=0.2)
    finally:
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-131")
def test_delivery_timer_starts_with_emission(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-73, scope=function, role=Verifies)
    UID: TC-H1FWMC-131
    TITLE: Operation 10 - Emission - Delivery Timer Start - Test Case

    STATEMENT: The delivery timer measures elapsed emission time in seconds.

    PREREQUISITES: Healthy system and a confirmed 20-second plan in Ready.

    STEPS: Hold Ready for 0.3 seconds, release the point, and observe the delivery
    timer immediately and after another 0.7 seconds of emission.

    EXPECTED_BEHAVIOR: Delivery time remains zero before emission and then
    increases at one second per second, within 0.2 seconds of observation tolerance.
    """
    session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=20))
    held = _hold_healthy(client, State.READY, 0.3, evidence)
    evidence.record("Ready delivery timer (s)", observed_timer_s=held.internal_timer_s, expected_timer_s=0)
    assert held.internal_timer_s == 0
    try:
        started = client.start_emission(session)
        evidence.record("Emission initial delivery timer (s)", observed_faults=started.faults,
                        expected_faults=Fault(0), observed_timer_s=started.internal_timer_s,
                        expected_bounds_s=(0, 0.2))
        assert not started.faults and 0 <= started.internal_timer_s <= 0.2
        after = _hold_healthy(client, State.EMISSION, 0.7, evidence)
        elapsed = (after.runtime_ms - started.runtime_ms) / 1000
        evidence.record("Delivery timer progress (s)", observed_before_s=started.internal_timer_s,
                        observed_after_s=after.internal_timer_s,
                        observed_delta_s=after.internal_timer_s - started.internal_timer_s,
                        expected_elapsed_s=elapsed, tolerance_s=0.2)
        assert after.internal_timer_s - started.internal_timer_s == pytest.approx(elapsed, abs=0.2)
    finally:
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-133")
def test_plan_coil_setpoints_are_applied_before_emission(
    client: MainControlClient, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-70, scope=function, role=Verifies)
    UID: TC-H1FWMC-133
    TITLE: Operation 09: Initiation - Coil Setpoints - Test Case

    STATEMENT: Plan deflection and focus currents are applied before emission.

    PREREQUISITES: Healthy connected coils, physical outputs, and PC telemetry.

    STEPS: Prepare a plan with X=300 mA, Y=-400 mA, focus=500 mA; inspect Ready
    without releasing the point. Repeat with X=-600 mA, Y=200 mA, focus=800 mA.

    EXPECTED_BEHAVIOR: Each plan produces the requested currents within 1 mA,
    corresponding output voltages and polarity, with emission disabled and all
    elapsed timers zero. No emission command is needed to apply the setpoints.
    """
    try:
        for x_ma, y_ma, focus_ma in ((300.0, -400.0, 500.0), (-600.0, 200.0, 800.0)):
            point = replace(OperationalPoint.beam_qa(50, duration_s=20),
                            x_coil_ma=x_ma, y_coil_ma=y_ma, focus_coil_ma=focus_ma)
            client.prepare_emission(point)
            ready = client.wait_for_telemetry(
                lambda item: abs(item.x_coil_current - x_ma) < 1
                and abs(item.y_coil_current - y_ma) < 1
                and abs(item.focus_coil_current - focus_ma) < 1, timeout=2,
            )
            evidence.record("Ready plan coil currents (mA)", observed_x_ma=ready.x_coil_current,
                            observed_y_ma=ready.y_coil_current, observed_focus_ma=ready.focus_coil_current,
                            expected_x_ma=x_ma, expected_y_ma=y_ma, expected_focus_ma=focus_ma, tolerance_ma=1,
                            observed_state=ready.state, expected_state=State.READY,
                            observed_faults=ready.faults, expected_faults=Fault(0),
                            observed_timers_s=(ready.internal_timer_s, ready.timer_1_s, ready.timer_2_s),
                            expected_timers_s=(0, 0, 0))
            assert ready.state == State.READY and not ready.faults
            assert ready.internal_timer_s == ready.timer_1_s == ready.timer_2_s == 0
            model = io_model.read()
            evidence.record("Ready plan coil enable and polarity",
                            observed_emission_enable=model["gpio"]["pins"]["port_a"]["io_emission_en"],
                            expected_emission_enable=False,
                            observed_x_direction=model["gpio"]["pins"]["port_b"]["io_coil_x_dir_n"],
                            expected_x_direction=x_ma >= 0,
                            observed_y_direction=model["gpio"]["pins"]["port_b"]["io_coil_y_dir_n"],
                            expected_y_direction=y_ma >= 0)
            assert not model["gpio"]["pins"]["port_a"]["io_emission_en"]
            assert model["gpio"]["pins"]["port_b"]["io_coil_x_dir_n"] is (x_ma >= 0)
            assert model["gpio"]["pins"]["port_b"]["io_coil_y_dir_n"] is (y_ma >= 0)
            for axis, volts in (("x", abs(x_ma) * 0.0025), ("y", abs(y_ma) * 0.0025),
                                ("f", focus_ma * 0.001666)):
                evidence.record("Ready plan coil DAC (V)", axis=axis,
                                observed_volts=model["dac"]["coil"][axis], expected_volts=volts, tolerance_volts=0.002)
                assert model["dac"]["coil"][axis] == pytest.approx(volts, abs=0.002)
            held = _hold_healthy(client, State.READY, 0.3, evidence)
            evidence.record("Held Ready coil currents (mA)",
                            observed_ma=(held.x_coil_current, held.y_coil_current, held.focus_coil_current),
                            expected_ma=(x_ma, y_ma, focus_ma), tolerance_ma=1)
            assert (held.x_coil_current, held.y_coil_current, held.focus_coil_current) == pytest.approx(
                (x_ma, y_ma, focus_ma), abs=1,
            )
    finally:
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-138")
def test_heatsink_fan_temperature_and_emission_control(
    client: MainControlClient, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-63, scope=function, role=Verifies)
    UID: TC-H1FWMC-138
    TITLE: Temperature monitoring - Heatsink Fan Control - Test Case

    STATEMENT: The heatsink fan provides temperature-dependent cooling and runs
    at full cooling during emission even with a cool heatsink.

    PREREQUISITES: Healthy Cold system, controllable heatsink temperature and
    observable fan enable and speed-control voltage.

    STEPS: Check the fan at 25 C, then 33 C, 38 C and 43 C in Cold. Restore 25 C,
    prepare a 20-second plan, release the point, and check the fan during emission.

    EXPECTED_BEHAVIOR: The fan is disabled at 25 C in Cold. At the elevated
    temperatures it is enabled with 1.8 V, 2.1 V and 2.5 V control respectively.
    It stops again at 25 C in Cold and runs at 2.5 V during healthy emission.
    """
    original = io_model.read()["adcs"]["system"]["heatsink_thermistor"]
    try:
        initial = client.query_telemetry()
        evidence.record("Initial heatsink cooling", observed_state=initial.state, expected_state=State.COLD,
                        observed_temperature_c=initial.heatsink_temperature, expected_temperature_c=25)
        assert initial.state == State.COLD
        model = io_model.wait_for(lambda model: not model["gpio"]["pins"]["port_a"]["io_hs_fan_en"])
        evidence.record("Cold heatsink fan disabled", observed_enable=model["gpio"]["pins"]["port_a"]["io_hs_fan_en"],
                        expected_enable=False)
        # 10 kOhm, beta=3977 K thermistor on a 5 V equal-resistance divider.
        for temperature, control in ((33, 1.8), (38, 2.1), (43, 2.5)):
            resistance_ratio = math.exp(3977 * (1 / (temperature + 273.15) - 1 / 298.15))
            voltage = 5 * resistance_ratio / (1 + resistance_ratio)
            io_model.patch({"adcs": {"system": {"heatsink_thermistor": voltage}}})
            snapshot = client.wait_for_telemetry(
                lambda item: abs(item.heatsink_temperature - temperature) < 0.1, timeout=3,
            )
            evidence.record("Heatsink temperature response (C)", input_volts=voltage,
                            observed_temperature_c=snapshot.heatsink_temperature, expected_temperature_c=temperature,
                            tolerance_c=0.1, observed_state=snapshot.state, expected_state=State.COLD,
                            observed_faults=snapshot.faults, expected_faults=Fault(0))
            assert snapshot.state == State.COLD and not snapshot.faults
            model = io_model.wait_for(lambda model: model["gpio"]["pins"]["port_a"]["io_hs_fan_en"]
                                      and abs(model["dac"]["fan"]["heatsink"] - control) < 0.002)
            evidence.record("Heatsink fan temperature control (V)", input_temperature_c=temperature,
                            observed_enable=model["gpio"]["pins"]["port_a"]["io_hs_fan_en"], expected_enable=True,
                            observed_control_v=model["dac"]["fan"]["heatsink"], expected_control_v=control,
                            tolerance_v=0.002)
        io_model.patch({"adcs": {"system": {"heatsink_thermistor": original}}})
        restored = client.wait_for_telemetry(lambda item: abs(item.heatsink_temperature - 25) < 0.1, timeout=3)
        model = io_model.wait_for(lambda model: not model["gpio"]["pins"]["port_a"]["io_hs_fan_en"])
        evidence.record("Restored cool heatsink (C)", observed_temperature_c=restored.heatsink_temperature,
                        expected_temperature_c=25, tolerance_c=0.1,
                        observed_enable=model["gpio"]["pins"]["port_a"]["io_hs_fan_en"], expected_enable=False)
        session = client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=20))
        client.start_emission(session)
        model = io_model.wait_for(lambda item: item["gpio"]["pins"]["port_a"]["io_hs_fan_en"])
        evidence.record("Emission heatsink fan override (V)",
                        observed_enable=model["gpio"]["pins"]["port_a"]["io_hs_fan_en"], expected_enable=True,
                        observed_control_v=model["dac"]["fan"]["heatsink"], expected_control_v=2.5, tolerance_v=0.002)
        assert model["dac"]["fan"]["heatsink"] == pytest.approx(2.5, abs=0.002)
        _hold_healthy(client, State.EMISSION, 0.3, evidence)
    finally:
        io_model.patch({"adcs": {"system": {"heatsink_thermistor": original}}})
        client.enter_cold()


@pytest.mark.strictdoc("TC-H1FWMC-140")
@pytest.mark.parametrize("flow_lpm", (0.0, 7.0), ids=("disconnected", "high-flow"))
def test_coolant_flow_reports_and_faults(
    client: MainControlClient, io_model: HostIOModel,
    head_interface: HeadInterfaceSimulator, flow_lpm: float,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-61, scope=function, role=Verifies)
    UID: TC-H1FWMC-140
    TITLE: Coolant monitoring - Flow Rate - Test Case

    STATEMENT: With the coolant pump enabled, flow below 2 L/min or above
    6 L/min is reported and produces a coolant fault.

    PREREQUISITES: Healthy head UART feedback, pump output and PC diagnostics.

    STEPS: Prepare a plan and verify normal 4 L/min flow with the pump enabled.
    Report loss of flow at 0 L/min, or excess flow at 7 L/min, over head UART.
    Read flow telemetry and the reported fault.

    EXPECTED_BEHAVIOR: Telemetry reports the applied flow within 0.1 L/min.
    A coolant fault is reported within 12 seconds.
    """
    client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=20))
    pump_enabled = io_model.read()["gpio"]["pins"]["port_a"]["io_pump_en"]
    evidence.record("Coolant flow pump enabled", observed_enable=pump_enabled, expected_enable=True)
    assert pump_enabled
    healthy = client.query_telemetry()
    evidence.record("Healthy coolant flow (L/min)", observed_flow_lpm=healthy.water_flow_rate,
                    expected_flow_lpm=4, tolerance_lpm=0.1, observed_faults=healthy.faults, expected_faults=Fault(0))
    assert not healthy.faults and healthy.water_flow_rate == pytest.approx(4, abs=0.1)
    try:
        head_interface.set_feedback(flow=flow_lpm)
        reported = client.wait_for_telemetry(
            lambda item: abs(item.water_flow_rate - flow_lpm) < 0.1, timeout=1,
        )
        evidence.record("Injected coolant flow (L/min)", observed_flow_lpm=reported.water_flow_rate,
                        expected_flow_lpm=flow_lpm, tolerance_lpm=0.1, healthy_bounds_lpm=(2, 6))
        assert reported.water_flow_rate == pytest.approx(flow_lpm, abs=0.1)
        faulted = client.wait_for_telemetry(lambda item: bool(item.faults & Fault.COOLANT), timeout=12)
        evidence.record("Coolant flow fault", observed_faults=faulted.faults, expected_faults=Fault.COOLANT,
                        observed_state=faulted.state, observed_flow_lpm=faulted.water_flow_rate,
                        input_flow_lpm=flow_lpm, timeout_s=12)
        assert faulted.faults == Fault.COOLANT
    finally:
        head_interface.set_feedback(flow=4.0)


@pytest.mark.strictdoc("TC-H1FWMC-141")
def test_coolant_temperature_reports_and_faults(
    client: MainControlClient, head_interface: HeadInterfaceSimulator,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-60, scope=function, role=Verifies)
    UID: TC-H1FWMC-141
    TITLE: Coolant monitoring - Temperature - Test Case

    STATEMENT: Reported coolant temperature above 35 C produces a coolant fault.

    PREREQUISITES: Healthy Cold system, head UART feedback and PC diagnostics.

    STEPS: Check normal 25 C telemetry. Report 35 C for 1 second, then report
    a high sensor value of 36 C and inspect telemetry and the reported fault.

    EXPECTED_BEHAVIOR: Temperatures are reported within 0.1 C. Exactly 35 C is
    accepted; 36 C produces Cold Fault and a coolant fault within 1 second.
    """
    healthy = client.query_telemetry()
    evidence.record("Healthy coolant temperature (C)", observed_temperature_c=healthy.water_temperature,
                    expected_temperature_c=25, tolerance_c=0.1, observed_faults=healthy.faults, expected_faults=Fault(0))
    assert not healthy.faults and healthy.water_temperature == pytest.approx(25, abs=0.1)
    try:
        head_interface.set_feedback(temperature=35.0)
        boundary = client.wait_for_telemetry(lambda item: item.water_temperature == 35, timeout=1)
        evidence.record("Coolant temperature boundary (C)", observed_temperature_c=boundary.water_temperature,
                        expected_temperature_c=35)
        held = _hold_healthy(client, State.COLD, 1, evidence)
        evidence.record("Held coolant temperature boundary (C)", observed_temperature_c=held.water_temperature,
                        expected_temperature_c=35)
        assert held.water_temperature == 35
        head_interface.set_feedback(temperature=36.0)
        faulted = client.wait_for_telemetry(
            lambda item: item.state == State.COLD_FAULT and bool(item.faults & Fault.COOLANT), timeout=1,
        )
        evidence.record("Overtemperature coolant fault (C)", observed_temperature_c=faulted.water_temperature,
                        expected_temperature_c=36, tolerance_c=0.1, accepted_maximum_c=35,
                        observed_state=faulted.state, expected_state=State.COLD_FAULT,
                        observed_faults=faulted.faults, expected_faults=Fault.COOLANT, timeout_s=1)
        assert faulted.water_temperature == pytest.approx(36, abs=0.1)
        assert faulted.faults == Fault.COOLANT
    finally:
        head_interface.set_feedback(temperature=25.0)


@pytest.mark.strictdoc("TC-H1FWMC-137")
def test_cabinet_fan_and_delayed_coolant_shutdown(
    client: MainControlClient, io_model: HostIOModel,
    head_interface: HeadInterfaceSimulator,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-65, scope=function, role=Verifies)
    UID: TC-H1FWMC-137
    TITLE: Cooling control
    STATEMENT: Cabinet cooling follows temperature with hysteresis; coolant
    circulation continues for three minutes after operation stops.
    PREREQUISITES: Healthy Cold system, cabinet thermistor input, head pressure
    feedback, and observable fan voltages and pump enable.
    STEPS:
    1. Raise cabinet temperature from 17 C through 20.1, 25.1, and 30.1 C;
       lower it to either side of 28, 23, and 18 C.
    2. Report 9 PSI for 12 seconds with the pump off, then restore 4 PSI.
    3. Request conditioning at 2500 mA and observe healthy cooling for 12 seconds.
    4. Send Stop and observe the pump throughout its 180-second cooldown.
    EXPECTED_BEHAVIOR: Cabinet drive rises through 3.7, 4.1, 4.5, and 4.9 V,
    retaining each higher speed until temperature falls 2 C below its threshold.
    Pump-off pressure causes no fault or stale fault after startup. Conditioning
    enables the pump and 1.85 V fan; Stop retains both for 180 seconds, then
    disables the pump and sets its fan to 0 V. Allow 0.5 second observation
    uncertainty, 5 seconds scheduling tolerance, and 0.01 V output tolerance.
    """
    original = io_model.read()["adcs"]["system"]["cabinet_thermistor"]
    # Physical thermistor input voltages; points straddle the boundaries to
    # avoid claiming an exact temperature unavailable with ADC quantization.
    fan_points = (
        (17.0, 3.718923, 3.7),
        (20.1, 3.579971, 4.1),
        (25.1, 3.319892, 4.5),
        (30.1, 3.059625, 4.9),
        (28.1, 3.163845, 4.9),
        (27.9, 3.174248, 4.5),
        (23.1, 3.423924, 4.5),
        (22.9, 3.434327, 4.1),
        (18.1, 3.669881, 4.1),
        (17.9, 3.678798, 3.7),
    )
    try:
        for temperature, input_volts, fan_volts in fan_points:
            io_model.patch({"adcs": {"system": {"cabinet_thermistor": input_volts}}})
            snapshot = client.wait_for_telemetry(
                lambda item: abs(item.cabinet_temperature - temperature) < 0.05,
                timeout=3,
            )
            model = io_model.wait_for(
                lambda model: abs(model["dac"]["fan"]["cabinet"] - fan_volts) < 0.01,
            )
            evidence.record("Cabinet fan hysteresis settled (C, V)", input_volts=input_volts,
                            observed_temperature_c=snapshot.cabinet_temperature, expected_temperature_c=temperature,
                            tolerance_c=0.05, observed_fan_v=model["dac"]["fan"]["cabinet"],
                            expected_fan_v=fan_volts, tolerance_v=0.01)
            _hold_healthy(client, State.COLD, 0.3, evidence)
            held_fan_v = io_model.read()["dac"]["fan"]["cabinet"]
            evidence.record("Cabinet fan hysteresis held (V)", input_temperature_c=temperature,
                            observed_fan_v=held_fan_v, expected_fan_v=fan_volts, tolerance_v=0.01)
            assert held_fan_v == pytest.approx(fan_volts, abs=0.01)

        pump_enabled = io_model.read()["gpio"]["pins"]["port_a"]["io_pump_en"]
        evidence.record("Pump off before pressure injection", observed_enable=pump_enabled, expected_enable=False)
        assert not pump_enabled
        head_interface.set_feedback(pressure=9.0)
        pressure = client.wait_for_telemetry(lambda item: item.water_pressure == 9)
        evidence.record("Pump-off pressure injection (PSI)", observed_pressure_psi=pressure.water_pressure,
                        expected_pressure_psi=9)
        _hold_healthy(client, State.COLD, 12, evidence)
        pump_enabled = io_model.read()["gpio"]["pins"]["port_a"]["io_pump_en"]
        evidence.record("Pump remains off with high pressure", observed_enable=pump_enabled, expected_enable=False)
        assert not pump_enabled
        head_interface.set_feedback(pressure=4.0)
        pressure = client.wait_for_telemetry(lambda item: item.water_pressure == 4)
        evidence.record("Restored pressure (PSI)", observed_pressure_psi=pressure.water_pressure, expected_pressure_psi=4)
        response = client.command(5, float_word(2500), 0)
        evidence.record("Conditioning request (mA)", requested_current_ma=2500,
                        observed_payload=response.payload, expected_payload=(0, 0))
        assert response.payload == (0, 0)
        conditioning = client.wait_for_state(State.CONDITIONING)
        evidence.record("Conditioning state", observed_state=conditioning.state, expected_state=State.CONDITIONING)
        active = io_model.read()
        evidence.record("Conditioning pump cooling (V)",
                        observed_enable=active["gpio"]["pins"]["port_a"]["io_pump_en"], expected_enable=True,
                        observed_fan_v=active["dac"]["fan"]["pump"], expected_fan_v=1.85, tolerance_v=0.01)
        assert active["gpio"]["pins"]["port_a"]["io_pump_en"]
        assert active["dac"]["fan"]["pump"] == pytest.approx(1.85, abs=0.01)
        _hold_healthy(client, State.CONDITIONING, 12, evidence)

        stopped_at = time.monotonic()
        client.directive(Directive.STOP)
        stopped = client.wait_for_state(State.COLD)
        evidence.record("Stop enters Cold", observed_state=stopped.state, expected_state=State.COLD)
        samples = 0
        last_enabled_s = None
        fan_min_v = None
        fan_max_v = None
        cooling = None
        observed = stopped
        elapsed = 0.0
        try:
            while True:
                observed = client.query_telemetry()
                assert observed.state == State.COLD and not observed.faults, observed
                cooling = io_model.read()
                elapsed = time.monotonic() - stopped_at
                samples += 1
                if not cooling["gpio"]["pins"]["port_a"]["io_pump_en"]:
                    assert 179.5 <= elapsed <= 185, f"Pump cooldown lasted {elapsed:.3f} s"
                    assert cooling["dac"]["fan"]["pump"] == pytest.approx(0, abs=0.01)
                    break
                last_enabled_s = elapsed
                fan_v = cooling["dac"]["fan"]["pump"]
                fan_min_v = fan_v if fan_min_v is None else min(fan_min_v, fan_v)
                fan_max_v = fan_v if fan_max_v is None else max(fan_max_v, fan_v)
                assert cooling["dac"]["fan"]["pump"] == pytest.approx(1.85, abs=0.01)
                assert elapsed <= 185, f"Pump still enabled after {elapsed:.3f} s"
                time.sleep(0.2)
        finally:
            evidence.record(
                "Full pump cooldown after Stop (s, V)", observed_samples=samples,
                observed_elapsed_s=elapsed, observed_last_enabled_s=last_enabled_s,
                expected_shutdown_bounds_s=(179.5, 185), nominal_cooldown_s=180,
                observed_enabled_fan_bounds_v=(fan_min_v, fan_max_v), expected_enabled_fan_v=1.85,
                observed_final_enable=None if cooling is None else cooling["gpio"]["pins"]["port_a"]["io_pump_en"],
                expected_final_enable=False,
                observed_final_fan_v=None if cooling is None else cooling["dac"]["fan"]["pump"],
                expected_final_fan_v=0, tolerance_v=0.01,
                observed_state=observed.state, expected_state=State.COLD,
                observed_faults=observed.faults, expected_faults=Fault(0),
            )
    finally:
        head_interface.set_feedback(pressure=4.0)
        io_model.patch({"adcs": {"system": {"cabinet_thermistor": original}}})
