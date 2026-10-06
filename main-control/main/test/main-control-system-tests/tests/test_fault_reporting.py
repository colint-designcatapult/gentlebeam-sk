"""Active fault reporting through the PC command interface."""

from __future__ import annotations

from dataclasses import replace
import time
import zlib

import pytest

from main_control_system_tests.head_interface import HeadInterfaceSimulator
from main_control_system_tests.evidence import Evidence
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.pc_protocol import Fault, FaultMessage, State, float_word, word_float
from main_control_system_tests.protocol import MainControlClient


@pytest.mark.strictdoc("TC-H1FWMC-92")
def test_active_fault_reporting(
    client: MainControlClient, head_interface: HeadInterfaceSimulator, io_model: HostIOModel, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-6, scope=function, role=Verifies)
    UID: TC-H1FWMC-92
    TITLE: Fault information - Active Fault Reporting - Test Case

    STATEMENT: Four distinct active faults are retained and reported with their
    original diagnostic details; repeated detections do not replace them.

    PREREQUISITES: Coolant temperature, supply inputs, an emergency stop,
    telemetry, unsolicited faults, and indexed fault queries are accessible.

    STEPS:
    1. Raise coolant temperature to 40 degrees C, then lower each supply input.
    2. For each new fault, inspect its unsolicited report and indexed query.
    3. Keep the faults active for 1 second, then open the emergency stop.
    4. Query all retained faults, restore inputs, and clear faults.

    EXPECTED_BEHAVIOR: Each distinct fault is published without a query.
    Indexed reports contain the category, printable format and checksum, raw
    arguments, original state and runtime, clear epoch, index, and active count.
    Four records remain unchanged despite repeated detections and a fifth
    fault. Clearing removes the records and advances the clear epoch.
    """
    client.enter_cold()
    initial = FaultMessage.decode(client.command(2, 0))
    evidence.record("Initial fault table", active_count=initial.active_count, expected_active_count=0,
                    clear_epoch=initial.clear_epoch)
    assert initial.active_count == 0
    epoch = initial.clear_epoch
    saved: list[FaultMessage] = []
    system_inputs = io_model.read()["adcs"]["system"]
    sources = (
        (None, Fault.COOLANT, "Coolant temperature", 40.0),
        ("voltage_3p3", Fault.BOARD_VOLTAGE, "3.3 V supply", 3.3),
        ("voltage_5", Fault.BOARD_VOLTAGE, "5 V supply", 5.0),
        ("voltage_12", Fault.BOARD_VOLTAGE, "12 V supply", 12.0),
    )
    try:
        for index, (field, category, label, expected) in enumerate(sources):
            before = client.query_telemetry()
            if field is None:
                head_interface.set_feedback(temperature=40)
            else:
                io_model.patch({"adcs": {"system": {field: 0.0}}})
            deadline = time.monotonic() + 3
            while True:
                # No fault query until the unsolicited publication is observed.
                report = client.fault_message(timeout=max(0.01, deadline - time.monotonic()))
                if report.clear_epoch == epoch and report.entry_index == index and report.fault_type:
                    break
                assert time.monotonic() < deadline, report
            after = client.query_telemetry()
            evidence.record("Unsolicited fault diagnostic", source=label, category=1 << report.fault_type,
                            expected_category=category, format_text=report.format_text,
                            format_hash=report.format_hash,
                            expected_hash=zlib.crc32(report.format_text.encode("ascii")),
                            arguments=report.arguments, arguments_decoded=tuple(map(word_float, report.arguments)),
                            expected_arguments=(40.0, 35.0) if field is None else
                            {"measured_range": (0, expected * 0.9), "target_V": expected, "tolerance_percent": 10},
                            state=report.state, expected_state=before.state, runtime_ms=report.runtime_ms,
                            runtime_bounds_ms=(before.runtime_ms, after.runtime_ms),
                            active_count=report.active_count, expected_active_count=index + 1,
                            clear_epoch=report.clear_epoch, expected_epoch=epoch, entry_index=report.entry_index,
                            expected_index=index)
            assert 1 << report.fault_type == category
            assert report.format_text.startswith(label)
            assert report.format_text.isprintable()
            assert report.format_hash == zlib.crc32(report.format_text.encode("ascii"))
            assert report.state == before.state
            assert before.runtime_ms <= report.runtime_ms <= after.runtime_ms
            assert report.active_count == index + 1
            assert report.clear_epoch == epoch
            if field is None:
                assert report.arguments == (float_word(40), float_word(35))
            else:
                measured, target, tolerance = report.arguments
                assert 0 <= word_float(measured) < expected * 0.9
                assert target == float_word(expected)
                assert tolerance == float_word(10)
            queried = FaultMessage.decode(client.command(2, index))
            evidence.record("Indexed fault parity", observed=queried, expected=report)
            assert queried == report
            saved.append(report)
            client.wait_for_state(State.COLD_FAULT)

        deadline = time.monotonic() + 1
        while time.monotonic() < deadline:
            latched = client.query_telemetry()
            assert latched.faults & Fault.BOARD_VOLTAGE
            time.sleep(0.05)
        evidence.record("Repeated detection retains category", faults=latched.faults,
                        required_fault=Fault.BOARD_VOLTAGE, elapsed_s=time.monotonic() - (deadline - 1),
                        minimum_observation_s=1)
        io_model.set_interlock("io_base_estop_n", False)
        client.wait_for_telemetry(lambda value: bool(value.faults & Fault.INTERLOCK))
        for index, report in enumerate(saved):
            retained = FaultMessage.decode(client.command(2, index))
            evidence.record("Retained fault after fifth source", observed=retained,
                            expected=replace(report, active_count=4))
            assert retained == replace(report, active_count=4)
        missing = FaultMessage.decode(client.command(2, 4))
        evidence.record("Fault capacity sentinel", entry_index=missing.entry_index, expected_index=4,
                        active_count=missing.active_count, expected_active_count=4,
                        fault_type=missing.fault_type, expected_fault_type=0,
                        format_text=missing.format_text, expected_format_text="",
                        arguments=missing.arguments, expected_arguments=())
        assert missing.entry_index == 4 and missing.active_count == 4
        assert missing.fault_type == 0 and missing.format_text == "" and missing.arguments == ()
    finally:
        head_interface.set_feedback(temperature=20)
        io_model.patch({"adcs": {"system": system_inputs}})
        io_model.set_interlock("io_base_estop_n", True)
        # Allow the analog input filters to settle before clearing latched faults.
        deadline = time.monotonic() + 1
        while time.monotonic() < deadline:
            client.query_telemetry()
            time.sleep(0.05)
        client.clear_faults()
        client.wait_for_telemetry(lambda value: not value.faults)
    cleared = FaultMessage.decode(client.command(2, 0))
    evidence.record("Cleared fault table", active_count=cleared.active_count, expected_active_count=0,
                    fault_type=cleared.fault_type, expected_fault_type=0,
                    clear_epoch=cleared.clear_epoch, expected_epoch=(epoch + 1) & 0xFFFFFFFF)
    assert cleared.active_count == 0 and cleared.fault_type == 0
    assert cleared.clear_epoch == (epoch + 1) & 0xFFFFFFFF
