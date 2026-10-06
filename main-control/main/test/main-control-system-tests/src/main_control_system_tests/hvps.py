"""Binary HVPS UART peer (sensus_src/hvps.{c,h}), not the PC's ASCII FTDI port.

The ideal supply follows commanded setpoints without a ramp; feedback overrides
can model a ramp, unstable regulation, or a failed sensor. GPIO inputs are read
from the host hardware model because emission enable is not a UART command.
"""

from __future__ import annotations

from collections.abc import Callable, Mapping
from dataclasses import dataclass, replace
from enum import IntEnum
from threading import Event, Lock, Thread
from urllib.error import URLError
import errno
import math
import socket
import struct
import time


HVPS_ADDRESS = ("127.0.0.1", 41022)
SYNC = b"\xff" * 8
FRAME_INTERVAL_SECONDS = 0.1
VERSION_MAGIC = 0x5652534E


class HvpsCommandCode(IntEnum):
    ALIVE = 1
    CLEAR_FAULTS = 2
    INTERLOCK_TEST = 3
    SET_POWER = 4
    SET_KV = 5
    SET_MA_LIMIT = 6
    SET_GRID = 7
    SET_HEATER = 8
    VERSION_REQUEST = 0x7F


@dataclass(frozen=True)
class HvpsCommand:
    code: HvpsCommandCode
    parameter: float
    integer: int
    raw: bytes


@dataclass(frozen=True)
class HvpsFeedback:
    flag_bits: int = 0
    io_bits: int = 1 << 7  # Power-factor correction ready.
    runtime_ms: int = 0
    power_setpoint: float = 0.0
    kv_setpoint: float = 0.0
    ma_limit: float = 8.0
    grid_setpoint: float = 0.0
    heater_setpoint: float = 0.0
    heater: float = 0.0
    kv: float = 0.0
    ma: float = 0.0
    grid: float = 0.0


def _checksum(body: bytes) -> bytes:
    return struct.pack("<I", sum(struct.unpack(f"<{len(body) // 4}I", body)) & 0xFFFFFFFF)


def encode_feedback(feedback: HvpsFeedback) -> bytes:
    """Encode the 15 little-endian words consumed by process_hvps()."""
    body = struct.pack(
        "<III9f", feedback.flag_bits, feedback.io_bits, feedback.runtime_ms,
        feedback.power_setpoint, feedback.kv_setpoint, feedback.ma_limit,
        feedback.grid_setpoint, feedback.heater_setpoint, feedback.heater,
        feedback.kv, feedback.ma, feedback.grid,
    )
    return SYNC + body + _checksum(body)


def encode_version(version: str = "system-test-hvps", mode: int = 0) -> bytes:
    """The version reply is 13 words, with a NUL-terminated 32-byte version."""
    text = version.encode("ascii")
    if len(text) > 31:
        raise ValueError("HVPS version must fit 31 ASCII bytes")
    body = struct.pack("<II32s", VERSION_MAGIC, mode, text)
    return SYNC + body + _checksum(body)


def decode_command(packet: bytes) -> HvpsCommand:
    if len(packet) != 24 or packet[:8] != SYNC:
        raise ValueError("HVPS command must be 24 bytes with eight sync bytes")
    if packet[20:] != _checksum(packet[8:20]):
        raise ValueError("HVPS command checksum mismatch")
    code, parameter, integer = struct.unpack("<IfI", packet[8:20])
    if not math.isfinite(parameter):
        raise ValueError("HVPS command parameter must be finite")
    return HvpsCommand(HvpsCommandCode(code), parameter, integer, packet)


class HvpsSimulator:
    """One host connection, periodic feedback, and fail-fast background health.

    With an io_model_reader, HV/grid/emission enable and watchdog/master-fault
    inputs follow the actual host GPIO. Without one, use set_controls explicitly.
    No ALIVE deadline is invented: main-control sends no periodic ALIVE command;
    its communication watchdog instead requires our periodic valid status frames.
    """

    def __init__(
        self, *, io_model_reader: Callable[[], Mapping] | None = None,
        frame_interval: float = FRAME_INTERVAL_SECONDS,
        address: tuple[str, int] = HVPS_ADDRESS,
    ) -> None:
        if not math.isfinite(frame_interval) or frame_interval <= 0:
            raise ValueError("HVPS frame interval must be positive and finite")
        self._io_model_reader = io_model_reader
        self._frame_interval = frame_interval
        self._address = address
        self._listener: socket.socket | None = None
        self._thread: Thread | None = None
        self._stop = Event()
        self._connected = Event()
        self._lock = Lock()
        self._error: Exception | None = None
        self._feedback = HvpsFeedback()
        self._overrides: dict[str, float | None] = dict.fromkeys(("kv", "ma", "heater", "grid"))
        self._status_overrides: dict[str, float | int] = {}
        self._version_response = encode_version()
        self._commands: list[HvpsCommand] = []
        self._controls = dict(hv_enabled=False, grid_enabled=False, emission_enabled=False, watchdog_ok=True)
        self._hv_control = False
        self._grid_control = False
        self._fast_warmup = False
        self._warming: bool | None = None
        self._fault_flags = 0
        self._fault_io = 0
        self._transmitting = True
        self._io_seen = False
        self._started_at = 0.0

    @property
    def feedback(self) -> HvpsFeedback:
        with self._lock:
            return replace(self._feedback, **self._status_overrides)

    @property
    def commands(self) -> tuple[HvpsCommand, ...]:
        with self._lock:
            return tuple(self._commands)

    def set_feedback(self, **values: float | int | None) -> None:
        """Override reported fields; None restores live command/GPIO following."""
        if values.keys() - HvpsFeedback.__dataclass_fields__.keys():
            raise ValueError("unknown HVPS feedback field")
        for name, value in values.items():
            if value is None:
                continue
            if name in ("flag_bits", "io_bits", "runtime_ms"):
                if type(value) is not int or not 0 <= value <= 0xFFFFFFFF:
                    raise ValueError(f"{name} must be an unsigned 32-bit integer")
            elif not math.isfinite(value):
                raise ValueError("feedback overrides must be finite or None")
        with self._lock:
            for name, value in values.items():
                if name in self._overrides:
                    self._overrides[name] = value
                elif value is None:
                    self._status_overrides.pop(name, None)
                else:
                    self._status_overrides[name] = value

    def set_version(self, version: str, mode: int = 0) -> None:
        """Configure subsequent version replies without restarting the UART peer."""
        if type(mode) is not int or not 0 <= mode <= 0xFFFFFFFF:
            raise ValueError("HVPS mode must be an unsigned 32-bit integer")
        response = encode_version(version, mode)
        with self._lock:
            self._version_response = response

    def set_warming(self, warming: bool | None) -> None:
        """Override heater warming status; None restores the ideal settled supply."""
        if warming is not None and type(warming) is not bool:
            raise ValueError("warming status must be boolean or None")
        with self._lock:
            self._warming = warming

    def set_faults(self, *, flag_bits: int = 0, io_bits: int = 0) -> None:
        """Inject latched status bits; CLEAR_FAULTS clears these injected bits."""
        if not 0 <= flag_bits <= 0xFFFFFFFF or not 0 <= io_bits <= 0xFFFFFFFF:
            raise ValueError("fault bitfields must be unsigned 32-bit integers")
        with self._lock:
            self._fault_flags = flag_bits
            self._fault_io = io_bits

    def set_controls(self, **values: bool) -> None:
        """Drive physical enables for standalone use without a host I/O reader."""
        if self._io_model_reader is not None:
            raise RuntimeError("physical controls are owned by the host I/O model")
        if values.keys() - self._controls.keys() or any(type(value) is not bool for value in values.values()):
            raise ValueError("controls must be boolean hv_enabled/grid_enabled/emission_enabled/watchdog_ok")
        with self._lock:
            self._controls.update(values)

    def set_transmitting(self, enabled: bool) -> None:
        """Pause all replies to exercise the firmware's HVPS communication watchdog."""
        with self._lock:
            self._transmitting = enabled

    def start(self) -> None:
        if self._listener is not None:
            raise RuntimeError("HVPS simulator is already listening")
        listener = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        try:
            listener.bind(self._address)
            listener.listen(1)
            listener.settimeout(0.05)
        except OSError:
            listener.close()
            raise
        self._listener = listener
        self._stop.clear()
        self._connected.clear()
        self._error = None
        self._io_seen = False
        self._started_at = time.monotonic()
        self._thread = Thread(target=self._serve, args=(listener,), name="hvps-simulator", daemon=True)
        self._thread.start()

    @property
    def port(self) -> int:
        """Return the bound TCP port, including OS-selected ephemeral ports."""
        if self._listener is None:
            raise RuntimeError("HVPS simulator is not listening")
        return self._listener.getsockname()[1]

    def wait_connected(self, timeout: float = 3) -> None:
        deadline = time.monotonic() + timeout
        while not self._connected.wait(0.01):
            self.assert_healthy()
            if time.monotonic() >= deadline:
                raise TimeoutError("host firmware did not connect to the HVPS TCP listener")
        self.assert_healthy()

    def assert_healthy(self) -> None:
        with self._lock:
            if self._error is not None:
                raise AssertionError("HVPS simulator failed") from self._error

    def stop(self) -> None:
        self._stop.set()
        if self._thread is not None:
            self._thread.join(timeout=2)
            if self._thread.is_alive():
                raise RuntimeError("HVPS simulator worker did not stop")
            self._thread = None
        if self._listener is not None:
            self._listener.close()
            self._listener = None
        self._connected.clear()

    def _read_controls(self) -> None:
        if self._io_model_reader is None:
            return
        try:
            model = self._io_model_reader()
        except (URLError, ConnectionRefusedError) as error:
            cause = error.reason if isinstance(error, URLError) else error
            refused = isinstance(cause, OSError) and cause.errno in (errno.ECONNREFUSED, 10061)
            if refused and not self._io_seen and time.monotonic() - self._started_at < 3:
                return  # The UART connects before the host's HTTP server is ready.
            raise
        port_a, _, port_c, _ = model["gpio"]["port_levels"]
        with self._lock:
            self._controls.update(
                hv_enabled=bool(port_a & (1 << 16)),
                grid_enabled=bool(port_a & (1 << 17)),
                emission_enabled=bool(port_a & (1 << 18)),
                watchdog_ok=bool(port_c & (1 << 13)) and bool(port_c & (1 << 16)),
            )
        self._io_seen = True

    def _update_feedback(self) -> HvpsFeedback:
        self._read_controls()
        with self._lock:
            controls = self._controls
            healthy = controls["watchdog_ok"] and not (self._fault_flags & 0xFFC00 or self._fault_io & 0x1FC62)
            hv_on = healthy and self._hv_control and controls["hv_enabled"]
            grid_on = healthy and self._grid_control and controls["grid_enabled"]
            emitting = hv_on and grid_on and controls["emission_enabled"]
            current = self._feedback
            values = dict(
                heater=current.heater_setpoint if healthy else 0.0,
                kv=current.kv_setpoint if hv_on else 0.0,
                ma=min(current.power_setpoint / current.kv_setpoint, current.ma_limit)
                if emitting and current.kv_setpoint > 0 else 0.0,
                grid=current.grid_setpoint if healthy else 0.0,
            )
            values.update({key: value for key, value in self._overrides.items() if value is not None})
            flags = (int(self._hv_control) << 1) | (int(self._grid_control) << 2)
            flags |= (int(emitting) << 5) | (int(emitting) << 7) | (int(self._fast_warmup) << 9)
            flags |= int(self._warming is True) << 3
            io_bits = (1 << 7) | (int(grid_on) << 2) | (int(controls["emission_enabled"]) << 3)
            io_bits |= (int(grid_on) << 4) | (int(hv_on) << 8) | (int(hv_on and values["kv"] > 0) << 9)
            self._feedback = replace(
                current, **values, flag_bits=flags | self._fault_flags, io_bits=io_bits | self._fault_io,
                runtime_ms=int((time.monotonic() - self._started_at) * 1000) & 0xFFFFFFFF,
            )
            return replace(self._feedback, **self._status_overrides)

    def _apply_command(self, command: HvpsCommand) -> None:
        fields = {
            HvpsCommandCode.SET_POWER: "power_setpoint", HvpsCommandCode.SET_KV: "kv_setpoint",
            HvpsCommandCode.SET_MA_LIMIT: "ma_limit", HvpsCommandCode.SET_GRID: "grid_setpoint",
            HvpsCommandCode.SET_HEATER: "heater_setpoint",
        }
        with self._lock:
            self._commands.append(command)
            if command.code in fields:
                self._feedback = replace(self._feedback, **{fields[command.code]: command.parameter})
                if command.code == HvpsCommandCode.SET_HEATER:
                    self._fast_warmup = bool(command.integer)
            elif command.code == HvpsCommandCode.CLEAR_FAULTS:
                self._fault_flags = self._fault_io = 0
            elif command.code == HvpsCommandCode.INTERLOCK_TEST:
                if command.integer == 123:
                    self._hv_control = True
                elif command.integer == 456:
                    self._grid_control = True
                else:
                    raise ValueError(f"unknown HVPS interlock handshake {command.integer}")

    def _serve(self, listener: socket.socket) -> None:
        try:
            while not self._stop.is_set():
                try:
                    connection, _ = listener.accept()
                except TimeoutError:
                    continue
                with connection:
                    connection.settimeout(min(0.02, self._frame_interval))
                    connection.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
                    self._connected.set()
                    self._exchange(connection)
                return
        except Exception as error:
            if not self._stop.is_set():
                with self._lock:
                    self._error = error
        finally:
            self._connected.clear()

    def _exchange(self, connection: socket.socket) -> None:
        buffer = bytearray()
        next_feedback = time.monotonic() + self._frame_interval
        version_pending = False
        while not self._stop.is_set():
            if time.monotonic() >= next_feedback:
                feedback = self._update_feedback()
                with self._lock:
                    transmitting = self._transmitting
                    version_response = self._version_response
                if transmitting:
                    # Firmware has a single receive-frame slot.
                    connection.sendall(version_response if version_pending else encode_feedback(feedback))
                    version_pending = False
                next_feedback = time.monotonic() + self._frame_interval
            try:
                received = connection.recv(4096)
            except TimeoutError:
                continue
            if not received:
                raise ConnectionError("host closed the HVPS TCP connection")
            buffer.extend(received)
            while len(buffer) >= 24:
                command = decode_command(bytes(buffer[:24]))
                del buffer[:24]
                self._apply_command(command)
                version_pending |= command.code == HvpsCommandCode.VERSION_REQUEST
