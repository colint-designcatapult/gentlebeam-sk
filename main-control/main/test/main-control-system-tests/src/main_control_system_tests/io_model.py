"""Control host-only physical inputs without bypassing firmware state transitions."""

from __future__ import annotations

import json
from collections.abc import Callable
import time
from urllib.request import Request, urlopen


class HostIOModel:
    def __init__(self, url: str = "http://127.0.0.1:8080/io-model") -> None:
        self.url = url

    def read(self) -> dict:
        with urlopen(self.url, timeout=1) as response:
            return json.load(response)

    def patch(self, values: dict) -> None:
        request = Request(
            self.url, data=json.dumps(values).encode(), method="POST",
            headers={"Content-Type": "application/json"},
        )
        with urlopen(request, timeout=1) as response:
            if response.status != 202 or json.load(response) != {"status": "queued"}:
                raise AssertionError("host did not accept I/O model patch")

    def set_interlock(self, name: str, closed: bool, timeout: float = 1) -> None:
        """Drive a named Port C input and wait for its applied physical level."""
        pins = self.read()["gpio"]["pins"]["port_c"]
        if name not in pins:
            raise ValueError(f"unknown Port C pin {name!r}")
        self.patch({"gpio": {"pins": {"port_c": {name: closed}}}})
        deadline = time.monotonic() + timeout
        while time.monotonic() < deadline:
            if self.read()["gpio"]["pins"]["port_c"][name] is closed:
                return
            time.sleep(0.01)
        raise TimeoutError(f"interlock {name} did not become {closed}")

    def wait_for(self, predicate: Callable[[dict], bool], timeout: float = 1) -> dict:
        """Wait for an applied host input or observed hardware output."""
        deadline = time.monotonic() + timeout
        latest = None
        while time.monotonic() < deadline:
            latest = self.read()
            if predicate(latest):
                return latest
            time.sleep(0.01)
        raise TimeoutError(f"host I/O condition not met; latest={latest}")

    def set_gpio_simulation(self, enabled: bool) -> None:
        """Select simulated latch/timer inputs or independently driven raw inputs."""
        self.patch({"gpio": {"simulate": enabled}})
        self.wait_for(lambda model: model["gpio"]["simulate"] is enabled)

    def set_focus_current_ma(self, current_ma: float) -> None:
        """Hold focus-current feedback while retaining the other coil inputs."""
        voltage = current_ma / 600
        self.patch({
            "dac": {"coil": {"simulate": False}},
            "adcs": {"coil": {"f_current": voltage}},
        })
        self.wait_for(
            lambda model: not model["dac"]["coil"]["simulate"]
            and abs(model["adcs"]["coil"]["f_current"] - voltage) < 1e-6
        )

    def set_deflection_current_ma(self, axis: str, current_ma: float) -> None:
        """Hold one bipolar coil feedback current, retaining the other inputs."""
        if axis not in ("x", "y"):
            raise ValueError("deflection axis must be x or y")
        voltage = 2.5 + current_ma / 600
        channel = f"{axis}_current"
        self.patch({
            "dac": {"coil": {"simulate": False}},
            "adcs": {"coil": {channel: voltage}},
        })
        self.wait_for(
            lambda model: not model["dac"]["coil"]["simulate"]
            and abs(model["adcs"]["coil"][channel] - voltage) < 1e-6
        )

    def backup_timer_seconds(self, timer: int, model: dict | None = None) -> float:
        """Read physical remaining seconds from a backup timer's oscillator count."""
        if timer not in (1, 2):
            raise ValueError("backup timer must be 1 or 2")
        snapshot = self.read() if model is None else model
        return snapshot[f"backup_timer{timer}"]["time_raw"] / 32768

    def restore_coil_feedback(self) -> None:
        """Restore feedback to the current physical DAC outputs and resume following."""
        model = self.read()
        dac = model["dac"]["coil"]
        pins = model["gpio"]["pins"]["port_b"]
        x_direction = 1 if pins["io_coil_x_dir_n"] else -1
        y_direction = 1 if pins["io_coil_y_dir_n"] else -1
        self.patch({
            "dac": {"coil": {"simulate": True}},
            "adcs": {"coil": {
                "x_voltage": dac["x"], "y_voltage": dac["y"], "f_voltage": dac["f"],
                "x_current": 2.5 + x_direction * dac["x"] * 1000 / (2.5 * 600),
                "y_current": 2.5 + y_direction * dac["y"] * 1000 / (2.5 * 600),
                "f_current": dac["f"] * 1000 / (1.666 * 600),
            }},
        })
        self.wait_for(lambda model: model["dac"]["coil"]["simulate"] is True)

    def set_timer_fault(
        self, timer: int, *, response_suppressed: bool = False,
        checksum_corrupted: bool = False,
    ) -> None:
        """Apply or remove one backup timer's transport fault."""
        if timer not in (1, 2):
            raise ValueError("backup timer must be 1 or 2")
        name = f"backup_timer{timer}"
        values = {
            "response_suppressed": response_suppressed,
            "checksum_corrupted": checksum_corrupted,
        }
        self.patch({name: values})
        self.wait_for(lambda model: all(model[name][key] is value for key, value in values.items()))
