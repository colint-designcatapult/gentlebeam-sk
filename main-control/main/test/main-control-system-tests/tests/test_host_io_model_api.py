"""Host I/O model HTTP API integration tests."""

from __future__ import annotations

import time

from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.evidence import Evidence


PORT_C = 2
MASTER_FAULT_BIT = 16
CLEAR_FAULT_BIT = 17
OTHER_INTERLOCK_MASK = 0x0000FFFF | (0x3 << 18)
EXPECTED_PIN_COUNTS = {"port_a": 30, "port_b": 4, "port_c": 31, "port_d": 25}


def test_partial_post_applies_on_host_tick_and_excludes_backend_pointers(
    client: object, io_model: HostIOModel, evidence: Evidence,
) -> None:
    before = io_model.read()
    io_model.patch({"adcs": {"system": {"voltage_12": 12.75}}})

    for _ in range(60):
        after = io_model.read()
        if after["adcs"]["system"]["voltage_12"] == 12.75:
            break
        time.sleep(0.01)
    else:
        raise AssertionError("queued voltage_12 patch was not applied")

    evidence.record("Partial ADC patch (V)", voltage_12=after["adcs"]["system"]["voltage_12"],
                    expected_voltage_12=12.75, voltage_5=after["adcs"]["system"]["voltage_5"],
                    expected_voltage_5=before["adcs"]["system"]["voltage_5"],
                    backend_keys_present=[key for key in ("hvps_ops", "hb_ops") if key in after],
                    expected_backend_keys_present=[])
    assert after["adcs"]["system"]["voltage_5"] == before["adcs"]["system"]["voltage_5"]
    assert "hvps_ops" not in after
    assert "hb_ops" not in after


def test_named_gpio_fields_cover_atmel_start_pins_and_overlay_ports(
    client: object, io_model: HostIOModel, evidence: Evidence,
) -> None:
    io_model.patch(
        {
            "gpio": {
                "simulate": False,
                "pins": {
                    "port_a": {"pa3": True},
                    "port_b": {"pb0": True},
                    "port_c": {"pc26": True},
                    "port_d": {"pd25": True},
                },
            }
        }
    )

    for _ in range(60):
        model = io_model.read()
        pins = model["gpio"]["pins"]
        if all(pins[port][field] for port, field in (("port_a", "pa3"), ("port_b", "pb0"), ("port_c", "pc26"), ("port_d", "pd25"))):
            break
        time.sleep(0.01)
    else:
        raise AssertionError("named GPIO patch was not applied")

    evidence.record("Named GPIO overlay", pin_counts={port: len(fields) for port, fields in pins.items()},
                    expected_pin_counts=EXPECTED_PIN_COUNTS,
                    pins={port: pins[port][field] for port, field in
                          (("port_a", "pa3"), ("port_b", "pb0"), ("port_c", "pc26"), ("port_d", "pd25"))},
                    expected_pin_high=True, port_levels=model["gpio"]["port_levels"],
                    required_bits=(1 << 3, 1, 1 << 26, 1 << 25))
    assert {port: len(fields) for port, fields in pins.items()} == EXPECTED_PIN_COUNTS
    assert model["gpio"]["port_levels"][0] & (1 << 3)
    assert model["gpio"]["port_levels"][1] & 1
    assert model["gpio"]["port_levels"][2] & (1 << 26)
    assert model["gpio"]["port_levels"][3] & (1 << 25)


def test_gpio_interlock_simulation_latches_until_clear_pulse(
    client: object, io_model: HostIOModel, evidence: Evidence,
) -> None:

    def post_port_c(levels: int) -> None:
        io_model.patch({"gpio": {"port_levels": [0, 0, levels, 0], "simulate": True}})

    def wait_for_master_fault(expected_high: bool) -> dict[str, object]:
        for _ in range(60):
            model = io_model.read()
            levels = model["gpio"]["port_levels"][PORT_C]
            if bool(levels & (1 << MASTER_FAULT_BIT)) is expected_high:
                evidence.record("Master-fault latch transition", port_c_levels=levels,
                                master_fault_n=bool(levels & (1 << MASTER_FAULT_BIT)),
                                expected_master_fault_n=expected_high,
                                clear_fault=model["gpio"]["pins"]["port_c"]["io_clear_fault"],
                                simulate=model["gpio"]["simulate"], expected_simulate=True)
                return model
            time.sleep(0.01)
        raise AssertionError(f"master fault did not become {'high' if expected_high else 'low'}")

    def wait_for_pin(name: str, expected_high: bool) -> dict[str, object]:
        for _ in range(60):
            model = io_model.read()
            if model["gpio"]["pins"]["port_c"][name] is expected_high:
                evidence.record("Physical interlock transition", pin=name,
                                observed=model["gpio"]["pins"]["port_c"][name],
                                expected=expected_high, port_c_levels=model["gpio"]["port_levels"][PORT_C])
                return model
            time.sleep(0.01)
        raise AssertionError(f"Port C pin {name} did not become {'high' if expected_high else 'low'}")

    # Start with a real interlock trip; the shared host fixture now establishes
    # healthy COLD operation instead of relying on an initially latched fault.
    wait_for_pin("io_clear_fault", False)
    post_port_c(OTHER_INTERLOCK_MASK & ~(1 << 2))
    wait_for_pin("io_base_estop_n", False)
    wait_for_master_fault(False)
    post_port_c(OTHER_INTERLOCK_MASK)
    wait_for_pin("io_door_closed", True)
    model = wait_for_master_fault(False)
    assert model["gpio"]["pins"]["port_c"]["io_door_closed"] is True
    assert model["gpio"]["pins"]["port_c"]["io_master_fault_n"] is False

    io_model.patch({"gpio": {"pins": {"port_c": {"io_clear_fault": True}}}})
    model = wait_for_master_fault(True)
    assert model["gpio"]["pins"]["port_c"]["io_clear_fault"] is True

    io_model.patch({"gpio": {"pins": {"port_c": {"io_base_estop_n": False}}}})
    model = wait_for_pin("io_base_estop_n", False)
    assert model["gpio"]["port_levels"][PORT_C] & (1 << 2) == 0
    wait_for_master_fault(False)

    io_model.patch({"gpio": {"pins": {"port_c": {"io_base_estop_n": True, "io_clear_fault": False}}}})
    wait_for_pin("io_base_estop_n", True)
    wait_for_master_fault(False)

    io_model.patch({"gpio": {"pins": {"port_c": {"io_clear_fault": True}}}})
    model = wait_for_master_fault(True)
    assert model["gpio"]["simulate"] is True
