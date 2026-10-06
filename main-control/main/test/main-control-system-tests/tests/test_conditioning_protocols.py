"""Startup, filament conditioning, warmup, and idle timeout host protocols."""

from __future__ import annotations

import time

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.hvps import HvpsCommandCode, HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import Directive, Fault, State, float_word
from main_control_system_tests.protocol import MainControlClient


SPR_OK = 0
SPR_ACCESS_ERROR = 1
SPR_OOB = 2
SPR_INVALID = 3
TARGET_MA = 2500.0
# Normal GentleBeam configuration, in mA (not the older manual protocol limits).
TARGET_CASES = (999.0, 1000.0, 3250.0, 3251.0, float("-inf"), float("inf"))
NAN_WORDS = (0x7FC00000, 0x7FA00001, 0xFFC00000)


def _record_telemetry(evidence: Evidence, phase: str, telemetry, **expected) -> None:
    evidence.record(
        phase,
        observed_state=telemetry.state if telemetry is not None else None,
        observed_faults=telemetry.faults if telemetry is not None else None,
        observed_heater_target_ma=telemetry.heater_setpoint if telemetry is not None else None,
        observed_heater_feedback_ma=telemetry.heater_feedback if telemetry is not None else None,
        expected_faults=expected.pop("expected_faults", Fault.NONE), **expected,
    )


def _record_hold(evidence: Evidence, phase: str, telemetry, started: float,
                 samples: int, pump, **expected) -> None:
    _record_telemetry(evidence, phase, telemetry, observed_elapsed_s=time.monotonic() - started,
                      observed_samples=samples, observed_pump=pump, **expected)


def _pump(io_model: HostIOModel) -> bool:
    return io_model.read()["gpio"]["pins"]["port_a"]["io_pump_en"]


def _request(client: MainControlClient, operation: str, target_word: int):
    if operation == "conditioning":
        return client.command(5, target_word, 0)
    return client.command(6, target_word)


def _assert_result(response, operation: str, result: int, evidence: Evidence) -> None:
    expected = (result, 0) if operation == "conditioning" else (result,)
    evidence.record("Command response", operation=operation,
                    observed_payload=response.payload, expected_payload=expected)
    assert response.payload == expected


def _assert_rejected_cold(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    commands_before: int, evidence: Evidence,
) -> None:
    started = time.monotonic()
    deadline = started + 0.5
    telemetry = None
    pump = None
    samples = 0
    try:
        while time.monotonic() < deadline:
            telemetry = client.query_telemetry()
            samples += 1
            assert telemetry.state == State.COLD and not telemetry.faults
            assert telemetry.heater_setpoint == 0
            assert telemetry.heater_feedback == 0
            pump = _pump(io_model)
            assert not pump
            time.sleep(0.05)
    finally:
        _record_hold(evidence, "Rejected request remains cold", telemetry, started,
                     samples, pump, expected_state=State.COLD, expected_heater_ma=0,
                     expected_pump=False, expected_hold_s=0.5)
    heater_commands = [command.parameter for command in hvps.commands[commands_before:]
                       if command.code == HvpsCommandCode.SET_HEATER]
    evidence.record("Rejected request heater commands (mA)", observed=heater_commands,
                    expected_positive_commands=0)
    assert not any(value > 0 for value in heater_commands)


def _check_target_range(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    operation: str, target_ma: float, evidence: Evidence,
) -> None:
    hvps.set_warming(True)
    commands_before = len(hvps.commands)
    response = _request(client, operation, float_word(target_ma))
    evidence.record("Requested heater target (mA)", operation=operation,
                    target_ma=target_ma, accepted_range_ma=(1000, 3250))
    if not 1000 <= target_ma <= 3250:
        _assert_result(response, operation, SPR_OOB, evidence)
        _assert_rejected_cold(client, hvps, io_model, commands_before, evidence)
        return
    _assert_result(response, operation, SPR_OK, evidence)
    expected = State.CONDITIONING if operation == "conditioning" else State.WARMUP
    accepted = client.wait_for_telemetry(
        lambda item: item.state == expected and item.heater_setpoint == target_ma,
        timeout=2,
    )
    _record_telemetry(evidence, "Accepted heater target", accepted,
                      expected_state=expected, expected_heater_ma=target_ma)
    assert not accepted.faults
    pump = _pump(io_model)
    evidence.record("Accepted request pump", observed_pump=pump, expected_pump=True)
    assert pump
    heater_commands = [command.parameter for command in hvps.commands[commands_before:]
                       if command.code == HvpsCommandCode.SET_HEATER]
    evidence.record("Accepted request heater commands (mA)", observed=heater_commands,
                    expected_target_ma=target_ma)
    assert target_ma in heater_commands
    client.directive(Directive.STOP)
    stopped = client.wait_for_state(State.COLD)
    _record_telemetry(evidence, "Stop accepted heater request", stopped, expected_state=State.COLD)


def _check_filament_target_timeout(
    client: MainControlClient, hvps: HvpsSimulator, operation: str, evidence: Evidence,
) -> None:
    expected_state = State.CONDITIONING if operation == "conditioning" else State.WARMUP
    client.enter_cold()
    cold = client.query_telemetry()
    _record_telemetry(evidence, "Before filament request", cold, expected_state=State.COLD)
    assert cold.state == State.COLD and not cold.faults
    hvps.set_feedback(heater=1500)
    telemetry = cold
    active = None
    started = time.monotonic()
    try:
        _assert_result(_request(client, operation, float_word(TARGET_MA)), operation, SPR_OK, evidence)
        active = client.wait_for_telemetry(
            lambda item: item.state == expected_state
            and item.heater_setpoint == TARGET_MA and item.heater_feedback == 1500,
            timeout=2,
        )
        _record_telemetry(
            evidence, "Filament below requested target", active,
            expected_state=expected_state, expected_heater_target_ma=TARGET_MA,
            expected_heater_feedback_ma=1500,
        )
        assert not active.faults
        while True:
            telemetry = client.query_telemetry()
            elapsed_s = (telemetry.runtime_ms - active.runtime_ms) / 1000
            if telemetry.faults:
                assert telemetry.faults & Fault.FILAMENT
                assert 14.5 <= elapsed_s <= 15.5, elapsed_s
                break
            assert telemetry.state == expected_state
            assert telemetry.heater_setpoint == TARGET_MA
            assert telemetry.heater_feedback == 1500
            assert time.monotonic() - started < 16, "No filament fault after the 15-second timeout"
            time.sleep(0.05)
        faulted = client.wait_for_state(State.WARMUP_FAULT)
        _record_telemetry(
            evidence, "Filament timeout fault state", faulted,
            expected_state=State.WARMUP_FAULT, expected_faults=Fault.FILAMENT,
        )
    finally:
        _record_telemetry(
            evidence, "Filament target timeout", telemetry,
            operation=operation, requested_target_ma=TARGET_MA, imposed_feedback_ma=1500,
            observed_elapsed_s=time.monotonic() - started,
            observed_runtime_elapsed_s=(
                (telemetry.runtime_ms - active.runtime_ms) / 1000 if active is not None else None
            ),
            expected_faults=Fault.FILAMENT, expected_timeout_s=15,
            observation_tolerance_s=0.5,
        )
        hvps.set_feedback(heater=None)


@pytest.mark.strictdoc("TC-H1FWMC-98")
def test_conditioning_filament_target_timeout(
    client: MainControlClient, hvps: HvpsSimulator, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-12, scope=function, role=Verifies)
    UID: TC-H1FWMC-98
    TITLE: Filament conditioning - Target Timeout
    STATEMENT: Failure to reach the conditioning filament target reports a fault
    after 15 seconds.
    PREREQUISITES: Healthy Cold system, UDP commands, and controllable HVPS feedback.
    STEPS: Request 2500 mA conditioning with feedback held at 1500 mA; observe
    conditioning, current telemetry, and the first fault while maintaining communication.
    EXPECTED_BEHAVIOR: Conditioning remains active without faults before the timeout.
    A filament fault appears after 15 seconds, allowing 0.5 seconds of observation
    tolerance, and the system enters Warmup Fault.
    """
    _check_filament_target_timeout(client, hvps, "conditioning", evidence)


@pytest.mark.strictdoc("TC-H1FWMC-99")
def test_warmup_filament_target_timeout(
    client: MainControlClient, hvps: HvpsSimulator, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-13, scope=function, role=Verifies)
    UID: TC-H1FWMC-99
    TITLE: Warmup procedure - Target Timeout
    STATEMENT: Failure to reach the warmup filament target reports a fault after
    15 seconds.
    PREREQUISITES: Healthy Cold system, UDP commands, and controllable HVPS feedback.
    STEPS: Request 2500 mA warmup directly from Cold with feedback held at 1500 mA;
    observe warmup, current telemetry, and the first fault while maintaining communication.
    EXPECTED_BEHAVIOR: Warmup remains active without faults before the timeout.
    A filament fault appears after 15 seconds, allowing 0.5 seconds of observation
    tolerance, and the system enters Warmup Fault.
    """
    _check_filament_target_timeout(client, hvps, "warmup", evidence)


@pytest.mark.strictdoc("TC-H1FWMC-110")
@pytest.mark.parametrize("target_word", NAN_WORDS, ids=("quiet-nan", "signaling-nan", "negative-nan"))
def test_warmup_invalid_format(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    target_word: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-22, scope=function, role=Verifies)
    UID: TC-H1FWMC-110
    TITLE: Warmup rejects invalid target format
    STATEMENT: A nonnumeric warmup target must not energize the heater or pump.
    PREREQUISITES: Healthy COLD system with UDP commands and HVPS feedback.
    STEPS: Send warmup with an IEEE-754 NaN target; observe the response and outputs.
    EXPECTED_BEHAVIOR: Invalid-value response, unchanged COLD state, zero heater
    current, and pump off for 0.5 seconds.
    """
    commands_before = len(hvps.commands)
    evidence.record("Invalid warmup input", target_word=hex(target_word), expected_result=SPR_INVALID)
    _assert_result(_request(client, "warmup", target_word), "warmup", SPR_INVALID, evidence)
    _assert_rejected_cold(client, hvps, io_model, commands_before, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-127")
@pytest.mark.parametrize("estop", ("io_base_estop_n", "io_remote_estop_n"))
def test_warmup_estop_initiation_and_interruption(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel, estop: str,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-76, scope=function, role=Verifies)
    UID: TC-H1FWMC-127
    TITLE: Warmup respects both emergency stops
    STATEMENT: Either open e-stop prevents warmup; opening it during warmup removes heat.
    PREREQUISITES: Healthy COLD system, independent e-stop inputs and warming feedback.
    STEPS:
    1. Open each e-stop and request 2500 mA warmup; observe for 0.5 seconds.
    2. Restore inputs, clear faults, and request warmup with warming feedback held active.
    3. Open the same e-stop during WARMUP and inspect telemetry and heater commands.
    EXPECTED_BEHAVIOR: An open input returns access-error without heater or pump
    startup. Restored inputs permit WARMUP, 2500 mA and pump startup. Reopening
    the input reports an interlock fault and commands zero heater current within
    2 seconds; coolant circulation may continue for its normal shutdown delay.
    """
    # Approved interfaces cannot pause dispatch between command acceptance and
    # state entry. A held, externally observable WARMUP tests post-acceptance
    # safety without pretending to exercise that unobservable pre-entry race.
    io_model.set_gpio_simulation(False)
    hvps.set_warming(True)
    commands_before = len(hvps.commands)
    try:
        io_model.set_interlock(estop, False)
        _assert_result(_request(client, "warmup", float_word(TARGET_MA)), "warmup", SPR_ACCESS_ERROR, evidence)
        started = time.monotonic()
        deadline = started + 0.5
        blocked = None
        pump = None
        samples = 0
        try:
            while time.monotonic() < deadline:
                blocked = client.query_telemetry()
                samples += 1
                # The independent safety monitor can already have latched the fault.
                assert blocked.state in (State.COLD, State.COLD_FAULT)
                assert blocked.heater_setpoint == 0
                pump = _pump(io_model)
                assert not pump
                time.sleep(0.05)
        finally:
            _record_hold(evidence, "Open e-stop blocks warmup", blocked, started, samples, pump,
                         estop=estop, expected_states=(State.COLD, State.COLD_FAULT),
                         expected_heater_ma=0, expected_pump=False, expected_hold_s=0.5,
                         expected_faults="NONE or latched interlock")
        heater_commands = [command.parameter for command in hvps.commands[commands_before:]
                           if command.code == HvpsCommandCode.SET_HEATER]
        evidence.record("Blocked warmup heater commands (mA)", observed=heater_commands,
                        expected_positive_commands=0)
        assert not any(value > 0 for value in heater_commands)
        io_model.set_interlock(estop, True)
        client.enter_cold()
        client.wait_for_telemetry(lambda item: item.state == State.COLD and not item.faults)
        client.warmup(TARGET_MA)
        warming = client.wait_for_telemetry(
            lambda item: item.state == State.WARMUP and item.heater_setpoint == TARGET_MA,
            timeout=2,
        )
        _record_telemetry(evidence, "Restored e-stop permits warmup", warming,
                          expected_state=State.WARMUP, expected_heater_ma=TARGET_MA)
        assert not warming.faults
        pump = _pump(io_model)
        evidence.record("Warmup pump", observed_pump=pump, expected_pump=True)
        assert pump
        io_model.set_interlock(estop, False)
        interrupted_at = time.monotonic()
        interrupted = client.wait_for_telemetry(
            lambda item: item.state == State.WARMUP_FAULT and item.heater_setpoint == 0,
            timeout=2,
        )
        _record_telemetry(evidence, "Reopened e-stop interrupts warmup", interrupted,
                          expected_state=State.WARMUP_FAULT, expected_faults="includes INTERLOCK",
                          expected_heater_ma=0, observed_elapsed_s=time.monotonic() - interrupted_at,
                          timeout_s=2)
        assert interrupted.faults & Fault.INTERLOCK
        zero = client.wait_for_telemetry(lambda item: item.heater_feedback == 0, timeout=2)
        _record_telemetry(evidence, "Interrupted heater settles", zero, expected_heater_ma=0,
                          expected_faults="includes INTERLOCK")
    finally:
        io_model.set_interlock(estop, True)
        io_model.set_gpio_simulation(True)


@pytest.mark.strictdoc("TC-H1FWMC-134")
def test_primed_idle_timeout(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-69, scope=function, role=Verifies)
    UID: TC-H1FWMC-134
    TITLE: Primed idle timeout
    STATEMENT: PRIMED returns to COLD after 5 minutes without an operational request.
    PREREQUISITES: Healthy COLD system with settled HVPS feedback and live telemetry.
    STEPS: Warm at 2500 mA to PRIMED, then query telemetry throughout the 300 second idle.
    EXPECTED_BEHAVIOR: PRIMED persists until the timeout, then changes to healthy COLD
    with zero heater current. Telemetry queries keep communication alive without
    postponing the timeout; allow 1 second sampling and 10 seconds scheduling tolerance.
    """
    client.warmup(TARGET_MA)
    primed = client.wait_for_state(State.PRIMED)
    _record_telemetry(evidence, "Warmup reaches primed", primed, requested_heater_ma=TARGET_MA,
                      expected_state=State.PRIMED)
    started = time.monotonic()
    telemetry = None
    samples = 0
    try:
        while True:
            telemetry = client.query_telemetry()
            samples += 1
            elapsed = time.monotonic() - started
            assert not telemetry.faults
            if telemetry.state == State.COLD:
                assert 299 <= elapsed <= 310, f"PRIMED timeout after {elapsed:.3f} s"
                assert telemetry.heater_setpoint == telemetry.heater_feedback == 0
                break
            assert telemetry.state == State.PRIMED
            assert elapsed <= 310, f"PRIMED persisted for {elapsed:.3f} s"
            # This is a read-only PC command, not a state-changing keepalive.
            time.sleep(0.25)
    finally:
        _record_hold(evidence, "Primed idle timeout", telemetry, started, samples, None,
                     expected_final_state=State.COLD, expected_timeout_range_s=(299, 310),
                     expected_final_heater_ma=0)


@pytest.mark.strictdoc("TC-H1FWMC-161")
@pytest.mark.parametrize("target_ma", TARGET_CASES)
def test_warmup_target_range(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel, target_ma: float,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-41, scope=function, role=Verifies)
    UID: TC-H1FWMC-161
    TITLE: Warmup validates target range
    STATEMENT: Warmup accepts the configured endpoints and rejects out-of-range current.
    PREREQUISITES: Healthy COLD system configured for 1000–3250 mA.
    STEPS: Request 999, 1000, 3250, 3251 mA and both infinities in separate cold starts.
    EXPECTED_BEHAVIOR: Endpoints enter WARMUP with the requested current and pump on.
    Other values return out-of-bounds and leave COLD, zero heater current, and pump off.
    """
    _check_target_range(client, hvps, io_model, "warmup", target_ma, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-163")
def test_power_on_startup(startup_client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-39, scope=function, role=Verifies)
    UID: TC-H1FWMC-163
    TITLE: Power-on startup state
    STATEMENT: Power-on begins in STARTUP before any initialization command.
    PREREQUISITES: Fresh host with UART peers and no initialization or fault-clear command.
    STEPS: Query the system state without requesting an operational transition.
    EXPECTED_BEHAVIOR: State is STARTUP and heater current is zero.
    """
    telemetry = startup_client.query_telemetry()
    _record_telemetry(evidence, "Power-on startup", telemetry,
                      expected_state=State.STARTUP, expected_heater_ma=0)
    assert telemetry.state == State.STARTUP
    assert telemetry.heater_setpoint == telemetry.heater_feedback == 0


@pytest.mark.strictdoc("TC-H1FWMC-164")
def test_startup_initialization(
    startup_client: MainControlClient, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-38, scope=function, role=Verifies)
    UID: TC-H1FWMC-164
    TITLE: PC initializes startup to cold
    STATEMENT: A PC initialization command moves STARTUP to COLD.
    PREREQUISITES: Fresh host and healthy physical inputs, without initialization.
    STEPS: Verify STARTUP, send STARTUP_INIT, and read telemetry and pump output.
    EXPECTED_BEHAVIOR: Successful reply and healthy COLD within 2 seconds;
    heater current remains zero and pump remains off.
    """
    initial = startup_client.query_telemetry()
    _record_telemetry(evidence, "Before initialization", initial, expected_state=State.STARTUP)
    assert initial.state == State.STARTUP
    startup_client.directive(Directive.STARTUP_INIT)
    started = time.monotonic()
    cold = startup_client.wait_for_state(State.COLD, timeout=2)
    _record_telemetry(evidence, "Initialized cold", cold, expected_state=State.COLD,
                      observed_elapsed_s=time.monotonic() - started, timeout_s=2,
                      expected_heater_ma=0)
    assert not cold.faults
    assert cold.heater_setpoint == cold.heater_feedback == 0
    pump = _pump(io_model)
    evidence.record("Initialized pump", observed_pump=pump, expected_pump=False)
    assert not pump


@pytest.mark.strictdoc("TC-H1FWMC-165")
def test_conditioning_full_fifteen_minute_hold(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-37, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-36, scope=function, role=Verifies)
    UID: TC-H1FWMC-165
    TITLE: Conditioning reaches target and holds for fifteen minutes
    STATEMENT: Stable conditioning current is held for 15 minutes before PRIMED.
    PREREQUISITES: Healthy COLD system with controllable HVPS current and warming feedback.
    STEPS:
    1. Request 2500 mA, report 1000 then 1750 mA over 10 seconds while still warming.
    2. Report stable 2500 mA repeatedly, timestamp its first telemetry indication,
       and query state, current, and pump throughout the full 900 second hold.
    3. Observe completion, then verify PRIMED persists for 2 seconds.
    EXPECTED_BEHAVIOR: CONDITIONING and pump remain active during ramp and hold.
    Completion occurs once, no earlier than 15 minutes at target; repeated stable
    feedback does not restart the hold. Allow 0.5 second observation uncertainty
    and up to 10 seconds stabilization/scheduling overhead. PRIMED commands zero heat.
    """
    hvps.set_warming(True)
    hvps.set_feedback(heater=1000)
    _assert_result(_request(client, "conditioning", float_word(TARGET_MA)), "conditioning", SPR_OK, evidence)
    entered = client.wait_for_telemetry(
        lambda item: item.state == State.CONDITIONING and item.heater_setpoint == TARGET_MA,
        timeout=2,
    )
    _record_telemetry(evidence, "Conditioning accepted", entered,
                      expected_state=State.CONDITIONING, expected_heater_ma=TARGET_MA)
    for current_ma in (1000, 1750):
        hvps.set_feedback(heater=current_ma)
        ramping = client.wait_for_telemetry(lambda item: item.heater_feedback == current_ma, timeout=2)
        started = time.monotonic()
        deadline = started + 5
        samples = 0
        pump = None
        try:
            while time.monotonic() < deadline:
                ramping = client.query_telemetry()
                samples += 1
                assert ramping.state == State.CONDITIONING and not ramping.faults
                assert ramping.heater_setpoint == TARGET_MA
                assert ramping.heater_feedback == current_ma
                pump = _pump(io_model)
                assert pump
                time.sleep(0.25)
        finally:
            _record_hold(evidence, "Conditioning ramp plateau", ramping, started, samples, pump,
                         expected_state=State.CONDITIONING, expected_heater_target_ma=TARGET_MA,
                         expected_heater_feedback_ma=current_ma, expected_pump=True, expected_hold_s=5)
    hvps.set_feedback(heater=TARGET_MA)
    hvps.set_warming(False)
    stable = client.wait_for_telemetry(
        lambda item: item.heater_feedback == TARGET_MA and not item.hvps_flags & (1 << 3),
        timeout=2,
    )
    stable_at = time.monotonic()
    _record_telemetry(evidence, "Conditioning reaches stable current", stable,
                      observed_hvps_flags=stable.hvps_flags, expected_warming_flag=False,
                      expected_heater_ma=TARGET_MA)
    telemetry = stable
    samples = 0
    pump = None
    feedback_min = feedback_max = stable.heater_feedback
    target_min = target_max = stable.heater_setpoint
    last_held = stable
    try:
        while True:
            telemetry = client.query_telemetry()
            samples += 1
            elapsed = time.monotonic() - stable_at
            assert not telemetry.faults
            if telemetry.state == State.PRIMED:
                assert 899.5 <= elapsed <= 910, f"conditioning hold lasted {elapsed:.3f} s"
                break
            last_held = telemetry
            feedback_min = min(feedback_min, telemetry.heater_feedback)
            feedback_max = max(feedback_max, telemetry.heater_feedback)
            target_min = min(target_min, telemetry.heater_setpoint)
            target_max = max(target_max, telemetry.heater_setpoint)
            assert telemetry.state == State.CONDITIONING
            assert elapsed <= 910, f"conditioning did not complete after {elapsed:.3f} s"
            assert telemetry.heater_setpoint == telemetry.heater_feedback == TARGET_MA
            pump = _pump(io_model)
            assert pump
            # Query responses feed the PC watchdog throughout the real-time hold.
            time.sleep(0.25)
    finally:
        _record_hold(evidence, "Full conditioning hold", telemetry, stable_at, samples, pump,
                     expected_hold_range_s=(899.5, 910), expected_hold_heater_ma=TARGET_MA,
                     expected_hold_pump=True, expected_final_state=State.PRIMED,
                     observed_hold_feedback_range_ma=(feedback_min, feedback_max),
                     observed_hold_target_range_ma=(target_min, target_max),
                     observed_last_held_state=last_held.state,
                     observed_last_held_feedback_ma=last_held.heater_feedback,
                     observed_last_held_target_ma=last_held.heater_setpoint)
    hvps.set_feedback(heater=None)
    primed = client.wait_for_telemetry(
        lambda item: item.heater_setpoint == item.heater_feedback == 0,
        timeout=2,
    )
    started = time.monotonic()
    deadline = started + 2
    samples = 0
    try:
        while time.monotonic() < deadline:
            primed = client.query_telemetry()
            samples += 1
            assert primed.state == State.PRIMED and not primed.faults
            assert primed.heater_setpoint == primed.heater_feedback == 0
            time.sleep(0.1)
    finally:
        _record_hold(evidence, "Post-conditioning primed hold", primed, started, samples, None,
                     expected_state=State.PRIMED, expected_heater_ma=0, expected_hold_s=2)


@pytest.mark.strictdoc("TC-H1FWMC-166")
def test_pc_requested_conditioning_and_stop(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-36, scope=function, role=Verifies)
    UID: TC-H1FWMC-166
    TITLE: PC requests and stops filament conditioning
    STATEMENT: Conditioning follows the PC's requested current and can return to COLD.
    PREREQUISITES: Healthy COLD system and controllable HVPS feedback.
    STEPS: Request 2500 mA conditioning, report rising 1000 and 2000 mA feedback,
    then send STOP and allow feedback to follow the resulting heater command.
    EXPECTED_BEHAVIOR: CONDITIONING commands 2500 mA with pump on and reports the
    rising current. STOP returns to COLD with zero heater current within 2 seconds.
    """
    hvps.set_warming(True)
    hvps.set_feedback(heater=1000)
    _assert_result(_request(client, "conditioning", float_word(TARGET_MA)), "conditioning", SPR_OK, evidence)
    for current_ma in (1000, 2000):
        hvps.set_feedback(heater=current_ma)
        observed = client.wait_for_telemetry(
            lambda item: item.state == State.CONDITIONING and item.heater_feedback == current_ma,
            timeout=2,
        )
        _record_telemetry(evidence, "Requested conditioning current", observed,
                          expected_state=State.CONDITIONING, expected_heater_target_ma=TARGET_MA,
                          expected_heater_feedback_ma=current_ma)
        assert observed.heater_setpoint == TARGET_MA and not observed.faults
        pump = _pump(io_model)
        evidence.record("Conditioning pump", observed_pump=pump, expected_pump=True)
        assert pump
    stopped_at = time.monotonic()
    client.directive(Directive.STOP)
    hvps.set_feedback(heater=None)
    stopped = client.wait_for_telemetry(
        lambda item: item.state == State.COLD and item.heater_setpoint == item.heater_feedback == 0,
        timeout=2,
    )
    _record_telemetry(evidence, "PC stop removes conditioning current", stopped,
                      expected_state=State.COLD, expected_heater_ma=0,
                      observed_elapsed_s=time.monotonic() - stopped_at, timeout_s=2)
    assert not stopped.faults
    started = time.monotonic()
    deadline = started + 0.5
    samples = 0
    try:
        while time.monotonic() < deadline:
            stopped = client.query_telemetry()
            samples += 1
            assert stopped.state == State.COLD and not stopped.faults
            assert stopped.heater_setpoint == stopped.heater_feedback == 0
            time.sleep(0.05)
    finally:
        _record_hold(evidence, "Stopped conditioning remains cold", stopped, started, samples, None,
                     expected_state=State.COLD, expected_heater_ma=0, expected_hold_s=0.5)


@pytest.mark.strictdoc("TC-H1FWMC-167")
@pytest.mark.parametrize("target_ma", TARGET_CASES)
def test_conditioning_target_range(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel, target_ma: float,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-35, scope=function, role=Verifies)
    UID: TC-H1FWMC-167
    TITLE: Conditioning validates target range
    STATEMENT: Conditioning accepts configured endpoints and rejects out-of-range current.
    PREREQUISITES: Healthy COLD system configured for 1000–3250 mA.
    STEPS: Request 999, 1000, 3250, 3251 mA and both infinities in separate cold starts.
    EXPECTED_BEHAVIOR: Endpoints enter CONDITIONING with the requested current and
    pump on. Other values return out-of-bounds, leaving COLD and heater and pump off.
    """
    _check_target_range(client, hvps, io_model, "conditioning", target_ma, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-168")
@pytest.mark.parametrize("target_word", NAN_WORDS, ids=("quiet-nan", "signaling-nan", "negative-nan"))
def test_conditioning_invalid_format(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel, target_word: int,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-34, scope=function, role=Verifies)
    UID: TC-H1FWMC-168
    TITLE: Conditioning rejects invalid target format
    STATEMENT: A nonnumeric conditioning target must not energize heater or pump.
    PREREQUISITES: Healthy COLD system with UDP commands and HVPS feedback.
    STEPS: Send conditioning with an IEEE-754 NaN target and observe response and outputs.
    EXPECTED_BEHAVIOR: Invalid-value response, unchanged COLD state, zero heater
    current, and pump off for 0.5 seconds.
    """
    commands_before = len(hvps.commands)
    evidence.record("Invalid conditioning input", target_word=hex(target_word),
                    expected_result=SPR_INVALID)
    _assert_result(_request(client, "conditioning", target_word), "conditioning", SPR_INVALID, evidence)
    _assert_rejected_cold(client, hvps, io_model, commands_before, evidence)


@pytest.mark.strictdoc("TC-H1FWMC-169")
def test_conditioning_rejected_outside_cold(
    client: MainControlClient, hvps: HvpsSimulator, io_model: HostIOModel,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-33, scope=function, role=Verifies)
    UID: TC-H1FWMC-169
    TITLE: Conditioning is restricted to cold
    STATEMENT: A valid conditioning request in PRIMED is rejected without starting heat.
    PREREQUISITES: Healthy system warmed at 2500 mA to PRIMED.
    STEPS: Request 2500 mA conditioning and observe state, heater commands, and pump.
    EXPECTED_BEHAVIOR: Access-error response, no heater startup, unchanged pump,
    and healthy PRIMED throughout a 1 second observation.
    """
    client.warmup(TARGET_MA)
    client.wait_for_state(State.PRIMED)
    client.wait_for_telemetry(lambda item: item.heater_setpoint == item.heater_feedback == 0)
    commands_before = len(hvps.commands)
    pump_before = _pump(io_model)
    _assert_result(_request(client, "conditioning", float_word(TARGET_MA)), "conditioning", SPR_ACCESS_ERROR, evidence)
    started = time.monotonic()
    deadline = started + 1
    telemetry = None
    pump = None
    samples = 0
    try:
        while time.monotonic() < deadline:
            telemetry = client.query_telemetry()
            samples += 1
            assert telemetry.state == State.PRIMED and not telemetry.faults
            assert telemetry.heater_setpoint == telemetry.heater_feedback == 0
            pump = _pump(io_model)
            assert pump == pump_before
            time.sleep(0.05)
    finally:
        _record_hold(evidence, "Rejected conditioning remains primed", telemetry, started, samples, pump,
                     expected_state=State.PRIMED, expected_heater_ma=0,
                     expected_pump=pump_before, expected_hold_s=1)
    heater_commands = [command.parameter for command in hvps.commands[commands_before:]
                       if command.code == HvpsCommandCode.SET_HEATER]
    evidence.record("Rejected conditioning heater commands (mA)", observed=heater_commands,
                    expected_positive_commands=0)
    assert not any(value > 0 for value in heater_commands)
