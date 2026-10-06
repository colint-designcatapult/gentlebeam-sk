"""Head-interface UART integration tests."""

from __future__ import annotations

import time

import pytest

from main_control_system_tests.head_interface import HeadInterfaceSimulator
from main_control_system_tests.evidence import Evidence
from main_control_system_tests.hvps import HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import (
    Directive, Fault, FaultMessage, NormalTelemetry, OperationalPoint,
    QcSessionStatus, State, word_float,
)
from main_control_system_tests.protocol import MainControlClient, Packet


@pytest.mark.strictdoc("TC-H1FWMC-120")
def test_head_interface_fixture_exchanges_valid_uart_packets(
    client: MainControlClient, head_interface: HeadInterfaceSimulator,
    hvps: HvpsSimulator, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-83, scope=function, role=Verifies)
    UID: TC-H1FWMC-120
    TITLE: Complete Periodic and Requested System Telemetry - Test Case

    STATEMENT: Periodic and requested telemetry use the same complete scalar
    layout and report physical inputs throughout a scalar emission.

    PREREQUISITES: Healthy normal-mode system, independently adjustable head,
    HVPS and analog inputs, and a valid 5-second scalar emission.

    STEPS:
    1. Supply distinct safe head, QC, HVPS, coil, pressure and temperature inputs.
    2. Compare requested and periodic telemetry with the supplied values.
    3. Observe unsolicited telemetry for 1.2 seconds and measure its cadence.
    4. Change both unsigned QC totals and verify requested and periodic updates.
    5. Restore supply following, prepare and start emission, then stop and
       observe operating states, timer progression and fault reporting.

    EXPECTED_BEHAVIOR: Telemetry contains exactly 49 fields, without point
    metadata. Every mapped input agrees with its source; QC totals at fields
    47 and 48 remain exact unsigned integers, distinct from live readings.
    Unsolicited rate is 100 Hz within 10% host scheduling tolerance over the
    observation window. Both reporting paths agree on state, interlocks,
    runtime, faults, timers, temperatures, coils and HVPS values.
    """
    head_interface.configure_qc(auto_respond=False)
    head_interface.set_feedback(
        collimator_low=0x01234567, collimator_high=0x89ABCDEF, buttons=0x2100,
        pressure=3.25, flow=4.5, temperature=26.75,
        mag_x_1=11.25, mag_y_1=-12.5, mag_z_1=13.75,
        mag_x_2=-21.5, mag_y_2=22.75, mag_z_2=-23.25,
        qc_channel_0=137, qc_channel_1=259,
        qc_channel_0_connected=True, qc_channel_1_connected=False,
        qc_accumulation_0=0xF1234567, qc_accumulation_1=0x89ABCDEF,
    )
    hvps_values = dict(
        runtime_ms=432123, io_bits=0x80, flag_bits=2,
        kv=1.25, ma=0.125, heater_setpoint=2400.0, heater=2350.0,
        grid_setpoint=51.5, grid=50.25, kv_setpoint=67.5,
        ma_limit=2.75, power_setpoint=123.5,
    )
    hvps.set_feedback(**hvps_values)
    io_model.patch({
        "dac": {"coil": {"simulate": False}},
        "adcs": {
            "coil": {"x_current": 2.75, "y_current": 2.125, "f_current": 0.625},
            "system": {
                "ion_pump_current_2": 1.5,
                "heatsink_thermistor": 2.5,
                "cabinet_thermistor": 3.06501547987616,
            },
        },
    })
    client.wait_for_telemetry(
        lambda value: value.qc_accumulation_0 == 0xF1234567
        and value.hvps_runtime_ms == 432123
        and abs(value.x_coil_current - 150) < 0.5
        and abs(value.y_coil_current + 225) < 0.5
        and abs(value.focus_coil_current - 375) < 0.5
        and abs(value.cabinet_temperature - 30) < 0.2
        and abs(value.ion_pump_pressure - 1e-9) < 1e-11,
    )
    # Expected wire positions are independent of the typed telemetry decoder.
    integer_fields = {
        0: State.COLD, 2: 0, 6: 0x01234567, 7: 0x89ABCDEF, 8: 0x2100,
        11: 0, 13: 0, 15: 432123, 16: 0x80, 17: 2, 44: 0x3FCC,
        46: 2, 47: 0xF1234567, 48: 0x89ABCDEF,
    }
    float_fields = {
        10: 0.0, 12: 0.0, 14: 0.0,
        18: 1.25, 19: 0.125, 20: 2400.0, 21: 2350.0, 22: 51.5, 23: 50.25,
        24: 150.0, 25: -225.0, 26: 375.0, 27: 1e-9,
        28: 3.25, 29: 4.5, 30: 26.75, 31: 25.0, 33: 30.0,
        34: 11.25, 35: -12.5, 36: 13.75, 37: -21.5, 38: 22.75, 39: -23.25,
        40: 137.0, 41: 67.5, 42: 2.75, 43: 123.5, 45: 259.0,
    }
    physical_interlocks = io_model.read()["gpio"]["port_levels"][2] & 0xDFFFF
    integer_fields[3] = physical_interlocks

    def check_mapping(packet: Packet, *, record: bool = False) -> NormalTelemetry:
        if record:
            evidence.record("Telemetry wire mapping (field-indexed engineering units)",
                            field_count=len(packet.payload), expected_field_count=49,
                            integers={i: packet.payload[i] for i in integer_fields}, expected_integers=integer_fields,
                            floats={i: word_float(packet.payload[i]) for i in float_fields},
                            expected_floats=float_fields,
                            absolute_tolerances={i: 1e-11 if i == 27 else 0.5 if i in (24, 25, 26, 31, 33)
                                                 else 1e-5 for i in float_fields})
        assert len(packet.payload) == 49
        for index, expected in integer_fields.items():
            assert packet.payload[index] == expected, (index, packet.payload[index], expected)
        for index, expected in float_fields.items():
            tolerance = 1e-11 if index == 27 else 0.5 if index in (24, 25, 26, 31, 33) else 1e-5
            assert word_float(packet.payload[index]) == pytest.approx(expected, abs=tolerance), index
        decoded = NormalTelemetry.decode(packet)
        assert decoded.qc_accumulation_0 > 0x7FFFFFFF
        assert decoded.qc_accumulation_1 > 0x7FFFFFFF
        return decoded

    requested_packet = client.command(4, 0)
    requested = check_mapping(requested_packet, record=True)
    # Skip startup backlog using the firmware's reported wall-runtime, not an
    # assumed number of packets per second.
    deadline = time.monotonic() + 2
    while True:
        periodic_packet = client.telemetry(timeout=1)
        if periodic_packet.payload[1] > requested.runtime_ms:
            break
        assert time.monotonic() < deadline, "periodic runtime did not advance"
    check_mapping(periodic_packet, record=True)
    # Analog moving averages may still converge within the tolerances above.
    # Fields 4, 5, 9 and 32 lack meaningful firmware producers: parity is
    # checked here, not an assertion that their initial values are correct.
    stable_indices = set(range(49)) - {1, 24, 25, 26, 27, 31, 33}
    assert tuple(periodic_packet.payload[i] for i in stable_indices) == tuple(
        requested_packet.payload[i] for i in stable_indices
    )
    started = time.monotonic()
    arrived = []
    last_keepalive = started
    while time.monotonic() - started < 1.2:
        packet = client.telemetry(timeout=0.2)
        arrived.append(time.monotonic())
        check_mapping(packet)
        if arrived[-1] - last_keepalive >= 0.2:
            check_mapping(client.command(4, 0))
            last_keepalive = arrived[-1]
    elapsed = arrived[-1] - started
    rate_hz = len(arrived) / elapsed
    # Allow 10% aggregate scheduling/observation-edge error, and at most a
    # 50 ms individual receive gap on a non-real-time host. This still rejects
    # a slower periodic publisher or one that sends only one large burst.
    assert 90 <= rate_hz <= 110, f"unsolicited telemetry rate {rate_hz:.1f} Hz"
    assert packet.payload[1] > requested.runtime_ms
    gaps = [right - left for left, right in zip([started, *arrived], arrived)]
    assert max(gaps) <= 0.05, f"longest unsolicited receive gap {max(gaps):.3f} s"
    runtime_elapsed_s = (packet.payload[1] - periodic_packet.payload[1]) / 1000
    evidence.record("Unsolicited telemetry cadence", samples=len(arrived), elapsed_s=elapsed,
                    rate_hz=rate_hz, expected_rate_hz=(90, 110), maximum_gap_s=max(gaps),
                    maximum_allowed_gap_s=0.05, runtime_elapsed_s=runtime_elapsed_s,
                    expected_runtime_elapsed_s=elapsed, runtime_tolerance_s=0.2)
    assert runtime_elapsed_s == pytest.approx(elapsed, abs=0.2)

    # A second pair catches cached totals, channel swaps and signed conversion.
    head_interface.set_feedback(qc_accumulation_0=0x87654321, qc_accumulation_1=0xFEDCBA98)
    integer_fields.update({47: 0x87654321, 48: 0xFEDCBA98})
    client.wait_for_telemetry(lambda value: value.qc_accumulation_0 == 0x87654321)
    check_mapping(client.command(4, 0), record=True)
    deadline = time.monotonic() + 2
    while True:
        packet = client.telemetry(timeout=max(0.001, deadline - time.monotonic()))
        if packet.payload[47] == 0x87654321:
            check_mapping(packet, record=True)
            break
        assert time.monotonic() < deadline, "periodic QC totals did not update"

    hvps.set_feedback(**dict.fromkeys(hvps_values))
    io_model.restore_coil_feedback()
    point = OperationalPoint(5.0, 5.0, 50.0, 1.0, 2500.0, 125.0, -175.0, 500.0)

    def observe_state(state: State) -> NormalTelemetry:
        expected_mask = 0xC3FCD if state in (State.READY, State.EMISSION) else 0x3FCC
        # State transitions and monitored interlock publication are asynchronous.
        # Observe the settled state/mask pair, not a transitional queued update.
        requested = client.wait_for_telemetry(
            lambda value: value.state == state and value.required_interlocks == expected_mask
        )
        deadline = time.monotonic() + 2
        while True:
            periodic = client.normal_telemetry(timeout=max(0.001, deadline - time.monotonic()))
            if (periodic.state == state and periodic.runtime_ms >= requested.runtime_ms
                    and periodic.required_interlocks == expected_mask):
                evidence.record("Requested and periodic state", requested_state=requested.state,
                                periodic_state=periodic.state, expected_state=state,
                                required_interlocks=(requested.required_interlocks, periodic.required_interlocks),
                                expected_mask=expected_mask, faults=(requested.faults, periodic.faults),
                                qc_totals=(periodic.qc_accumulation_0, periodic.qc_accumulation_1),
                                expected_qc_totals=(requested.qc_accumulation_0, requested.qc_accumulation_1),
                                timers_s=(requested.internal_timer_s, requested.timer_1_s, requested.timer_2_s))
                assert periodic.faults == requested.faults
                assert periodic.required_interlocks == requested.required_interlocks
                assert periodic.qc_accumulation_0 == requested.qc_accumulation_0
                assert periodic.qc_accumulation_1 == requested.qc_accumulation_1
                return requested
            assert time.monotonic() < deadline, f"No periodic {state.name}"

    # Observe each stable operating state before issuing its next PC action.
    client.clear_plan()
    observe_state(State.COLD)
    client.warmup()
    observe_state(State.PRIMED)
    client.directive(Directive.RESET_TIMERS)
    session = client.new_session()
    observe_state(State.STAGING)
    client.load_operational_point(session, point)
    client.directive(Directive.STAGE_PLAN)
    observe_state(State.STAGED)
    client.confirm_operational_point(session, point)
    client.release_plan(session)
    ready = observe_state(State.READY)
    assert ready.required_interlocks == 0xC3FCD
    assert ready.internal_timer_s == pytest.approx(0)
    assert ready.timer_1_s == pytest.approx(0)
    assert ready.timer_2_s == pytest.approx(0)
    emission = client.start_emission(session)
    observe_state(State.EMISSION)
    advanced = client.wait_for_telemetry(lambda value: value.internal_timer_s >= 0.5)
    evidence.record("Emission electrical and coil feedback (kV, mA)",
                    state=advanced.state, expected_state=State.EMISSION, faults=advanced.faults, expected_faults=0,
                    kv=advanced.kv_feedback, expected_kv=point.kv, ma=advanced.ma_feedback, expected_ma=point.ma,
                    heater_ma=advanced.heater_setpoint, expected_heater_ma=point.heater_ma,
                    coils_ma=(advanced.x_coil_current, advanced.y_coil_current, advanced.focus_coil_current),
                    expected_coils_ma=(point.x_coil_ma, point.y_coil_ma, point.focus_coil_ma), coil_tolerance_ma=0.5,
                    timer_states=(advanced.timer_1_state, advanced.timer_2_state), expected_timer_states=(2, 2),
                    runtime_ms=advanced.runtime_ms, emission_start_runtime_ms=emission.runtime_ms)
    assert advanced.state == State.EMISSION and not advanced.faults
    assert advanced.kv_feedback == pytest.approx(point.kv)
    assert advanced.ma_feedback == pytest.approx(point.ma)
    assert advanced.heater_setpoint == pytest.approx(point.heater_ma)
    assert advanced.x_coil_current == pytest.approx(point.x_coil_ma, abs=0.5)
    assert advanced.y_coil_current == pytest.approx(point.y_coil_ma, abs=0.5)
    assert advanced.focus_coil_current == pytest.approx(point.focus_coil_ma, abs=0.5)
    assert advanced.timer_1_state == advanced.timer_2_state == 2
    assert advanced.runtime_ms > emission.runtime_ms
    model = io_model.read()
    for timer, reported_s in ((1, advanced.timer_1_s), (2, advanced.timer_2_s)):
        elapsed_s = point.total_time_s + 0.5 - io_model.backup_timer_seconds(timer, model)
        evidence.record("Emission timer comparison (s)", timer=timer, reported_s=reported_s,
                        physical_elapsed_s=elapsed_s, internal_elapsed_s=advanced.internal_timer_s, tolerance_s=0.15)
        assert reported_s == pytest.approx(elapsed_s, abs=0.15)
        assert reported_s == pytest.approx(advanced.internal_timer_s, abs=0.15)
    assert advanced.qc_accumulation_0 == 0x87654321
    assert advanced.qc_accumulation_1 == 0xFEDCBA98
    client.directive(Directive.STOP)
    final = observe_state(State.COLD)
    assert not final.faults
    assert final.required_interlocks == 0x3FCC

    # Fault and physical-interlock fields must change, not merely start at zero.
    client.enter_cold()
    io_model.set_interlock("io_base_estop_n", False)
    try:
        faulted = client.wait_for_telemetry(lambda value: bool(value.faults & Fault.INTERLOCK))
        assert not faulted.interlocks & (1 << 2)
        deadline = time.monotonic() + 2
        while True:
            periodic = client.normal_telemetry(timeout=1)
            if periodic.runtime_ms >= faulted.runtime_ms and periodic.faults & Fault.INTERLOCK:
                evidence.record("Requested and unsolicited interlock fault", requested_faults=faulted.faults,
                                periodic_faults=periodic.faults, expected_fault=Fault.INTERLOCK,
                                interlocks=(faulted.interlocks, periodic.interlocks), expected_open_bit=2)
                assert not periodic.interlocks & (1 << 2)
                break
            assert time.monotonic() < deadline, "No unsolicited interlock fault"
    finally:
        io_model.set_interlock("io_base_estop_n", True)
        client.enter_cold()
    head_interface.assert_healthy()
    hvps.assert_healthy()


@pytest.mark.strictdoc("TC-H1FWMC-90")
@pytest.mark.parametrize(
    "button_index", range(6), ids=("laser", "led", "camera", "function-1", "function-2", "zero-g")
)
def test_head_keypad_press_and_release(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, button_index: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-2, scope=function, role=Verifies)
    UID: TC-H1FWMC-90
    TITLE: Head Interface - IO Feedback - Test Case
    
    STATEMENT: Keypad presses and releases are reported in system telemetry.

    PREREQUISITES: Head keypad inputs and system telemetry are accessible.

    STEPS:
    1. Release all keys.
    2. Press and release each key twice, checking telemetry after every action.

    EXPECTED_BEHAVIOR: Within 3 seconds of each action, the first press enables
    the selected key indication, release retains it, the second press clears it,
    and the second release leaves it cleared. Other key indications stay clear.
    """
    # Button indices follow head-interface/main/SensusSrc/buttons.h.
    # report_button_toggle() in sys_data.c XORs bit 8+index only on a new press.
    sequence = button_index * 5

    def expect_feedback(expected: int, action: str) -> None:
        nonlocal sequence
        sequence += 1
        head_interface.set_feedback(collimator_low=sequence)
        observed = None
        deadline = time.monotonic() + 3
        while (remaining := deadline - time.monotonic()) > 0:
            head_interface.assert_healthy()
            try:
                payload = client.telemetry(timeout=remaining).payload
            except TimeoutError:
                break
            # Both fields come from the same UART frame. Compare the full button
            # word, not just the selected bit, to detect spurious other-key toggles.
            observed = (payload[6], payload[8])
            if observed == (sequence, expected):
                break
        head_interface.assert_healthy()
        evidence.record("Keypad press/release feedback", key=button_index, action=action,
                        observed_frame_buttons=observed, expected_frame_buttons=(sequence, expected), timeout_s=3)
        assert observed == (sequence, expected), (
            f"key {button_index} {action}: expected UDP frame/button words "
            f"({sequence}, 0x{expected:04X}), last received {observed}"
        )

    try:
        head_interface.set_button_pressed(button_index, False)
        head_interface.set_feedback(buttons=0)
        expect_feedback(0, "initial")
        expected = 0
        for cycle in (1, 2):
            head_interface.set_button_pressed(button_index, True)
            expected ^= 1 << (8 + button_index)
            expect_feedback(expected, f"press {cycle}")
            head_interface.set_button_pressed(button_index, False)
            expect_feedback(expected, f"release {cycle}")
    finally:
        # Restore both the held-key state and parity for subsequent actions.
        head_interface.set_button_pressed(button_index, False)
        head_interface.set_feedback(buttons=0)

@pytest.mark.strictdoc("TC-H1FWMC-91")
@pytest.mark.parametrize("failure_mode", ("silence", "corruption"))
def test_head_communication_monitoring(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, failure_mode: str, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-3, scope=function, role=Verifies)
    UID: TC-H1FWMC-91
    TITLE:  Head Interface - Communication Monitoring - Test Case
    
    STATEMENT: Missing head feedback reports a communication fault; corrupted
    feedback leaves the last valid reported values unchanged.

    PREREQUISITES: Head feedback, system telemetry, and fault clearing are accessible.

    STEPS:
    1. Provide valid feedback and verify its reported values.
    2. Stop feedback, observe the fault, then restore feedback and clear faults.
    3. Send corrupted feedback with changed values for 3 seconds.
    4. Restore valid feedback and verify the changed values are reported.

    EXPECTED_BEHAVIOR: Silence reports a communication fault within 2 seconds.
    Corruption produces no communication fault and leaves reported feedback
    unchanged for 3 seconds. Valid feedback resumes and faults can be cleared.
    """
    client.enter_cold()
    assert not client.wait_for_telemetry(lambda value: not value.faults).faults
    head_interface.set_feedback(collimator_low=0x90)
    client.wait_for_telemetry(lambda value: value.collimator_low == 0x90)
    if failure_mode == "silence":
        head_interface.pause()
    else:
        head_interface.set_corruption(byte_offset=25)
        head_interface.set_feedback(collimator_low=0x91)
    try:
        if failure_mode == "silence":
            observed = client.wait_for_telemetry(
                lambda value: bool(value.faults & Fault.HEADBOARD_COMM), timeout=2
            )
            evidence.record("Silent head communication", faults=observed.faults,
                            expected_fault=Fault.HEADBOARD_COMM, timeout_s=2)
        else:
            deadline = time.monotonic() + 3
            while time.monotonic() < deadline:
                observed = client.query_telemetry()
                assert not observed.faults & Fault.HEADBOARD_COMM
                assert observed.collimator_low == 0x90
                head_interface.assert_healthy()
                time.sleep(0.05)
            evidence.record("Corrupted head feedback held for 3 s", faults=observed.faults,
                            forbidden_fault=Fault.HEADBOARD_COMM, collimator_low=observed.collimator_low,
                            expected_collimator_low=0x90, elapsed_s=time.monotonic() - (deadline - 3),
                            minimum_observation_s=3)
    finally:
        head_interface.resume()
        head_interface.set_corruption()
    head_interface.set_feedback(collimator_low=0x91)
    recovered = client.wait_for_telemetry(lambda value: value.collimator_low == 0x91)
    client.clear_faults()
    cleared = client.wait_for_telemetry(lambda value: not value.faults)
    evidence.record("Head communication recovery", collimator_low=recovered.collimator_low,
                    expected_collimator_low=0x91, faults=cleared.faults, expected_faults=0)
    head_interface.assert_healthy()

@pytest.mark.strictdoc("TC-H1FWMC-139")
@pytest.mark.parametrize(
    "flow,pressure,temperature",
    ((9.0, 9.0, 40.0), (1.0, 1.0, 20.0)),
    ids=("high", "low"),
)
def test_head_telemetry_coolant_monitoring(
    client: MainControlClient, head_interface: HeadInterfaceSimulator,
    flow: float, pressure: float, temperature: float, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-62, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-61, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-60, scope=function, role=Verifies)
    UID: TC-H1FWMC-139
    TITLE: Head Interface Coolant Monitoring - Test Case
    
    STATEMENT: Coolant telemetry reports applied values and separate diagnostics
    identify each out-of-range parameter.

    PREREQUISITES: Head coolant inputs, PC commands, telemetry, and fault
    diagnostics are accessible.

    STEPS:
    1. Apply 4 LPM, 4 PSI, and 20 C; prepare emission through PC commands.
    2. With the pump active, apply 9 LPM, 9 PSI, and 40 C together.
    3. Restore normal inputs, verify telemetry recovery, and clear faults.
    4. Repeat with 1 LPM, 1 PSI, and 20 C applied together.

    EXPECTED_BEHAVIOR: All three telemetry values match within 0.1.
    High inputs report three distinct diagnostics: flow, pressure, and temperature.
    Low inputs report two distinct diagnostics: flow and pressure.
    Flow and pressure faults appear within 12 seconds; high temperature faults
    appear within 1 second. Normal inputs do not fault and recovery clears faults.
    """
    head_interface.set_feedback(flow=4.0, pressure=4.0, temperature=20.0)
    client.prepare_emission(OperationalPoint.beam_qa(50, duration_s=5))
    normal = client.wait_for_telemetry(
        lambda item: abs(item.water_flow_rate - 4) < 0.1
        and abs(item.water_pressure - 4) < 0.1
        and abs(item.water_temperature - 20) < 0.1
    )
    assert not normal.faults
    initial = FaultMessage.decode(client.command(2, 0))
    evidence.record("Normal coolant (LPM, PSI, C)", observed=(normal.water_flow_rate, normal.water_pressure,
                    normal.water_temperature), expected=(4, 4, 20), tolerance=0.1,
                    faults=normal.faults, expected_faults=0, active_count=initial.active_count, expected_count=0)
    assert initial.active_count == 0
    expected = {
        "flow": (flow, 6.0 if flow > 4 else 2.0, 12.0),
        "pressure": (pressure, 8.0 if pressure > 4 else 2.0, 12.0),
    }
    if temperature > 35:
        expected["temperature"] = (temperature, 35.0, 1.0)
    observed: dict[str, tuple[int, int]] = {}
    telemetry_matches = False
    started = time.monotonic()
    try:
        head_interface.set_feedback(flow=flow, pressure=pressure, temperature=temperature)
        while len(observed) < len(expected) or not telemetry_matches:
            deadline = started + min(
                (values[2] for field, values in expected.items() if field not in observed),
                default=12.0,
            )
            remaining = deadline - time.monotonic()
            assert remaining > 0, f"Missing coolant diagnostics: {expected.keys() - observed.keys()}"
            snapshot = client.query_telemetry(timeout=remaining)
            telemetry_matches = (
                abs(snapshot.water_flow_rate - flow) < 0.1
                and abs(snapshot.water_pressure - pressure) < 0.1
                and abs(snapshot.water_temperature - temperature) < 0.1
            )
            index = 0
            while True:
                remaining = deadline - time.monotonic()
                assert remaining > 0, f"Missing coolant diagnostics: {expected.keys() - observed.keys()}"
                record = FaultMessage.decode(client.command(2, index, timeout=remaining))
                assert record.clear_epoch == initial.clear_epoch
                if record.fault_type:
                    assert Fault(1 << record.fault_type) == Fault.COOLANT, record.message
                    fields = [field for field in expected if f"Coolant {field} " in record.format_text]
                    assert len(fields) == 1, record.message
                    field = fields[0]
                    value, limit, timeout = expected[field]
                    assert tuple(map(word_float, record.arguments)) == pytest.approx((value, limit))
                    if field not in observed:
                        evidence.record("Coolant diagnostic", field=field,
                                        observed_arguments=tuple(map(word_float, record.arguments)),
                                        expected_arguments=(value, limit), elapsed_s=time.monotonic() - started,
                                        timeout_s=timeout, category=1 << record.fault_type,
                                        expected_category=Fault.COOLANT, clear_epoch=record.clear_epoch,
                                        expected_epoch=initial.clear_epoch, entry_index=record.entry_index)
                        assert time.monotonic() - started <= timeout, record.message
                        observed[field] = (record.clear_epoch, record.entry_index)
                index += 1
                if index >= record.active_count:
                    break
            head_interface.assert_healthy()
            if len(observed) == len(expected) and telemetry_matches:
                client.wait_for_telemetry(
                    lambda item: bool(item.faults & Fault.COOLANT),
                    timeout=max(0.0, started + 12 - time.monotonic()),
                )
                break
            time.sleep(min(0.05, max(0.0, deadline - time.monotonic())))
        evidence.record("Abnormal coolant telemetry (LPM, PSI, C)",
                        observed=(snapshot.water_flow_rate, snapshot.water_pressure, snapshot.water_temperature),
                        expected=(flow, pressure, temperature), tolerance=0.1,
                        distinct_diagnostics=observed, expected_diagnostic_fields=tuple(expected))
        assert len(set(observed.values())) == len(expected), observed
    finally:
        head_interface.set_feedback(flow=4.0, pressure=4.0, temperature=20.0)
        client.wait_for_telemetry(
            lambda item: abs(item.water_flow_rate - 4) < 0.1
            and abs(item.water_pressure - 4) < 0.1
            and abs(item.water_temperature - 20) < 0.1
        )
        client.enter_cold()
        client.clear_faults()
        client.wait_for_telemetry(lambda item: not item.faults)
        cleared = FaultMessage.decode(client.command(2, 0))
        evidence.record("Coolant faults cleared", epoch=cleared.clear_epoch, previous_epoch=initial.clear_epoch,
                        active_count=cleared.active_count, expected_active_count=0)
        assert cleared.clear_epoch != initial.clear_epoch
        assert cleared.active_count == 0

@pytest.mark.strictdoc("TC-H1FWMC-151")
def test_head_led_ring(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-2, scope=function, role=Verifies)
    UID: TC-H1FWMC-151
    TITLE: Head Interface - IO Feedback - Test Case
    
    STATEMENT: The head-ring color indicates the system state.

    PREREQUISITES: PC commands, head-ring commands, telemetry, and an
    emergency-stop input are accessible.

    STEPS:
    1. Enter COLD through PC commands and observe the head-ring command.
    2. Open the emergency stop and observe the head-ring command.
    3. Restore the emergency stop and clear faults.

    EXPECTED_BEHAVIOR: The head receives the COLD color command in COLD and
    the FAULT color command after the emergency stop opens. Faults clear
    after recovery.
    """
    # The fixture already entered COLD. Leave it first so the observed command
    # proves a new transition rather than matching its startup command.
    client.warmup()
    client.wait_for_state(State.WARMUP)
    started = time.monotonic()
    client.enter_cold()
    client.wait_for_state(State.COLD)
    cold_command = head_interface.wait_for_command(led_sequence=1, after=started)
    evidence.record("Cold head-ring command", led_sequence=cold_command.led_sequence, expected_led_sequence=1)
    try:
        started = time.monotonic()
        io_model.set_interlock("io_base_estop_n", False)
        client.wait_for_telemetry(lambda item: bool(item.faults))
        fault_command = head_interface.wait_for_command(led_sequence=9, after=started)
        evidence.record("Fault head-ring command", led_sequence=fault_command.led_sequence, expected_led_sequence=9)
    finally:
        io_model.set_interlock("io_base_estop_n", True)
        client.clear_faults()
        cleared = client.wait_for_telemetry(lambda item: not item.faults)
        evidence.record("Ring fault recovery", faults=cleared.faults, expected_faults=0)

@pytest.mark.strictdoc("TC-H1FWMC-308")
@pytest.mark.parametrize("sensor,axis", [(sensor, axis) for sensor in (1, 2) for axis in ("x", "y", "z")])
def test_head_emission_magnetometer_monitoring(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, sensor: int, axis: str, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-307, scope=function, role=Verifies)
    UID: TC-H1FWMC-308
    TITLE: Emission magnetometer monitoring - Test Case
    
    STATEMENT: Magnetometer deviations during emission report a fault.

    PREREQUISITES: Head magnetometer inputs, PC emission commands, telemetry,
    and fault diagnostics are accessible.

    STEPS:
    1. Provide stable readings and start emission through PC commands.
    2. Move one axis by half the allowed deviation and observe for 0.5 seconds.
    3. Move the axis beyond the allowed deviation and inspect the fault diagnostic.
    4. Restore the reading and clear faults.
    5. Repeat for all three axes of both magnetometers.

    EXPECTED_BEHAVIOR: The allowed deviation is the greater of 10% of the
    absolute baseline or 1.0 uT. Within-limit readings produce no magnetometer
    fault for 0.5 seconds. Beyond-limit readings report a fault within 1 second,
    with diagnostics identifying sensor, axis, reading, baseline, and limit.
    Faults clear after recovery.
    """
    field = f"mag_{axis}_{sensor}"
    # 20 uT exercises the percentage limit (2 uT); 5 uT exercises the 1 uT floor.
    baseline = 20.0 if sensor == 1 else 5.0
    head_interface.set_feedback(**{field: baseline})
    point = OperationalPoint.beam_qa(50, duration_s=10)
    session = client.prepare_emission(point)
    client.start_emission(session)
    assert not client.wait_for_telemetry(lambda item: item.state == State.EMISSION).faults
    limit = max(abs(baseline) * 0.1, 1.0)
    within_limit = baseline + limit / 2
    head_interface.set_feedback(**{field: within_limit})
    readings = f"magnetometer_{sensor}"
    axis_index = "xyz".index(axis)
    client.wait_for_telemetry(
        lambda item: getattr(item, readings)[axis_index] == pytest.approx(within_limit)
    )
    deadline = time.monotonic() + 0.5
    while time.monotonic() < deadline:
        within = client.query_telemetry()
        assert not within.faults & Fault.MAGNETOMETER
        time.sleep(0.05)
    evidence.record("Within-limit magnetometer (uT)", sensor=sensor, axis=axis,
                    reading=getattr(within, readings)[axis_index], expected_reading=within_limit,
                    baseline=baseline, deviation_limit=limit, faults=within.faults,
                    forbidden_fault=Fault.MAGNETOMETER, elapsed_s=time.monotonic() - (deadline - 0.5),
                    minimum_observation_s=0.5)
    try:
        head_interface.set_feedback(**{field: baseline + 5})
        fault = client.wait_for_telemetry(
            lambda item: bool(item.faults & Fault.MAGNETOMETER), timeout=1
        )
        assert fault.faults & Fault.MAGNETOMETER
        deadline = time.monotonic() + 3
        while True:
            message = client.fault_message(timeout=max(0.01, deadline - time.monotonic())).message
            if f"Magnetometer {sensor} axis {'xyz'.index(axis) + 1}" in message:
                evidence.record("Out-of-limit magnetometer diagnostic (uT)", sensor=sensor, axis=axis,
                                reading=getattr(fault, readings)[axis_index], expected_reading=baseline + 5,
                                baseline=baseline, limit=limit, faults=fault.faults,
                                expected_fault=Fault.MAGNETOMETER, diagnostic=message)
                assert "baseline" in message and "limit" in message and "reading" in message
                assert f"reading {baseline + 5:.6f}" in message
                assert f"baseline {baseline:.6f}" in message
                assert f"{max(baseline * 0.1, 1):.6f} uT limit" in message
                break
            assert time.monotonic() < deadline, message
    finally:
        head_interface.set_feedback(**{field: baseline})
        client.enter_cold()
        client.clear_faults()
        cleared = client.wait_for_telemetry(lambda item: not item.faults)
        evidence.record("Magnetometer recovery", faults=cleared.faults, expected_faults=0)

@pytest.mark.strictdoc("TC-H1FWMC-306")
@pytest.mark.parametrize("kv,adc", ((50, 200), (70, 500), (100, 1000)))
def test_head_qc_processing(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, kv: int, adc: int, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-305, scope=function, role=Verifies)
    UID: TC-H1FWMC-306
    TITLE: QC Processing - Test Case
    
    STATEMENT: Beam QA reports diode readings and accumulated results.

    PREREQUISITES: The system is ready for emission; head diode inputs,
    PC commands, telemetry, and Beam QA results are accessible.

    STEPS:
    1. Apply a diode reading of 200 at 50 kV, 500 at 70 kV, or 1000 at 100 kV.
    2. Start a 2-second Beam QA emission through PC commands.
    3. Inspect emission telemetry and the completed result, then clear the plan.
    4. Repeat for each voltage and reading.

    EXPECTED_BEHAVIOR: Telemetry reports the applied diode reading and
    accumulation. Beam QA completes without faults, reports a duration of
    2 seconds within 0.2 seconds, and returns the expected diode accumulation
    matching the head feedback.
    """
    head_interface.set_feedback(qc_channel_0=adc)
    head_interface.configure_qc(auto_respond=True, sample_rate=1000)
    session = client.prepare_emission(OperationalPoint.beam_qa(kv, duration_s=2))
    client.start_emission(session, beam_qa=True)
    during = client.wait_for_telemetry(
        lambda item: item.state == State.EMISSION and item.qc_accumulation_0 > 0
    )
    evidence.record("Beam QA live diode", kv=kv, adc=during.qc_channel_0, expected_adc=adc,
                    connections=(during.qc_adc_2_connected, during.qc_adc_1_connected),
                    expected_connections=(True, False), accumulation=during.qc_accumulation_0,
                    expected_accumulation_multiple=adc)
    assert during.qc_channel_0 == adc
    assert during.qc_adc_2_connected and not during.qc_adc_1_connected
    assert during.qc_accumulation_0 % adc == 0
    result = client.beam_qa_result(timeout=10)
    assert result.status == QcSessionStatus.COMPLETE
    completed = client.query_telemetry()
    assert not completed.faults
    assert completed.internal_timer_s == pytest.approx(2, abs=0.2)
    feedback = head_interface.feedback
    evidence.record("Completed Beam QA", status=result.status, expected_status=QcSessionStatus.COMPLETE,
                    faults=completed.faults, expected_faults=0, duration_s=completed.internal_timer_s,
                    expected_duration_s=2, tolerance_s=0.2, sample_count=result.channel_0_sample_count,
                    expected_sample_count=feedback.qc_sample_count_0,
                    accumulation=result.channel_0_accumulation,
                    expected_accumulation=adc * result.channel_0_sample_count,
                    head_accumulation=feedback.qc_accumulation_0,
                    channel_1=(result.channel_1_accumulation, result.channel_1_sample_count), expected_channel_1=(0, 0))
    assert result.channel_0_sample_count > 0
    assert result.channel_0_accumulation == adc * result.channel_0_sample_count
    assert result.channel_0_accumulation == feedback.qc_accumulation_0
    assert result.channel_0_sample_count == feedback.qc_sample_count_0
    assert result.channel_1_accumulation == result.channel_1_sample_count == 0
    client.clear_plan()
    head_interface.assert_healthy()