"""Normal-mode PC wire values from sensus_src/system_parameters.h and pc_comm_parser.h.

Floats are IEEE-754 words, not integer engineering values. Fault bits use the
firmware's one-based FaultType positions; QC accumulations are unsigned integers.
"""

from __future__ import annotations

from dataclasses import dataclass
from enum import IntEnum, IntFlag
import re
import struct
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from .protocol import Packet


def float_word(value: float) -> int:
    return struct.unpack("<I", struct.pack("<f", value))[0]


def word_float(value: int) -> float:
    return struct.unpack("<f", struct.pack("<I", value))[0]


class State(IntEnum):
    STARTUP = 0
    COLD = 1
    COLD_FAULT = 2
    CONDITIONING = 3
    WARMUP = 4
    WARMUP_FAULT = 5
    PRIMED = 6
    STAGING = 7
    STAGED = 8
    HVPS_CHECK = 9
    SETUP = 10
    READY = 11
    LAUNCHING = 12
    EMISSION = 13
    TERMINATION = 14
    DISCHARGE = 15
    FAULT = 16
    SYSTEM_CRASH = 17
    CALIBRATION = 22
    FAULT_DISCHARGE = 23
    UNKNOWN = 0xFFFFFFFF


class Directive(IntEnum):
    STARTUP_INIT = 1
    STAGE_PLAN = 2
    STOP = 3
    CLEAR_FAULTS = 4
    WIPE_PLAN = 5
    RESET_TIMERS = 6
    STANDBY = 7


class Fault(IntFlag):
    NONE = 0
    INTERLOCK = 1 << 1
    HVPS = 1 << 2
    KV = 1 << 3
    MA = 1 << 4
    FILAMENT = 1 << 5
    GRID = 1 << 6
    COIL_CURRENT = 1 << 7
    ION_PUMP_FB = 1 << 8
    ION_REPELLER = 1 << 9
    PELTIER = 1 << 10
    HEATSINK = 1 << 11
    COOLANT = 1 << 12
    BOARD_VOLTAGE = 1 << 13
    PC_COMM_TIMEOUT = 1 << 14
    HVPS_COMM = 1 << 15
    TIMER_COMM = 1 << 16
    HEADBOARD_COMM = 1 << 17
    LEDBOARD_COMM = 1 << 18
    PELTIER_COMM = 1 << 19
    QC = 1 << 20
    ADC_BUS = 1 << 21
    MEMORY = 1 << 22
    INVALID_CONFIG = 1 << 23
    MAGNETOMETER = 1 << 24
    OTHER = 1 << 25


class QcSessionStatus(IntEnum):
    IDLE = 0
    ARMED = 1
    STARTING = 2
    ACCUMULATING = 3
    STOPPING = 4
    COMPLETE = 5
    ERROR = 6


def _payload(packet: Packet, packet_type: int, count: int) -> tuple[int, ...]:
    from .protocol import ProtocolError

    if packet.packet_type != packet_type or len(packet.payload) != count:
        raise ProtocolError(
            f"expected type {packet_type} with {count} words; "
            f"received type {packet.packet_type} with {len(packet.payload)} words"
        )
    return packet.payload


@dataclass(frozen=True)
class DeviceInformation:
    """Main-control and HVPS release identities in the 19-word version response."""

    main_version: str
    main_crc: int
    main_mode: int
    hvps_version: str
    hvps_mode: int

    @classmethod
    def decode(cls, packet: Packet) -> DeviceInformation:
        from .protocol import ProtocolError

        words = _payload(packet, 101, 19)

        def version_text(values: tuple[int, ...]) -> str:
            raw = struct.pack("<8I", *values)
            terminator = raw.find(b"\0")
            if terminator < 0:
                raise ProtocolError("device version is not NUL-terminated")
            try:
                return raw[:terminator].decode("ascii")
            except UnicodeDecodeError as error:
                raise ProtocolError("device version is not ASCII") from error

        return cls(version_text(words[:8]), words[8], words[9],
                   version_text(words[10:18]), words[18])


@dataclass(frozen=True)
class NormalTelemetry:
    """The complete 49-word normal telemetry layout (not calibration telemetry)."""

    state: State
    runtime_ms: int
    faults: Fault
    interlocks: int
    led_ring_state: int
    led_base_state: int
    collimator_low: int
    collimator_high: int
    buttons: int
    internal_timer_state: int
    internal_timer_s: float
    timer_1_state: int
    timer_1_s: float
    timer_2_state: int
    timer_2_s: float
    hvps_runtime_ms: int
    hvps_io: int
    hvps_flags: int
    kv_feedback: float
    ma_feedback: float
    heater_setpoint: float
    heater_feedback: float
    grid_setpoint: float
    grid_feedback: float
    x_coil_current: float
    y_coil_current: float
    focus_coil_current: float
    ion_pump_pressure: float
    water_pressure: float
    water_flow_rate: float
    water_temperature: float
    heatsink_temperature: float
    peltier_temperature: float
    cabinet_temperature: float
    magnetometer_1: tuple[float, float, float]
    magnetometer_2: tuple[float, float, float]
    qc_channel_0: float
    kv_setpoint: float
    ma_limit_setpoint: float
    power_setpoint: float
    required_interlocks: int
    qc_channel_1: float
    qc_adc_i2c_status: int
    qc_accumulation_0: int
    qc_accumulation_1: int

    @classmethod
    def decode(cls, packet: Packet) -> NormalTelemetry:
        words = _payload(packet, 104, 49)
        floats = tuple(word_float(word) for word in words)
        return cls(
            State(words[0]), words[1], Fault(words[2]), *words[3:10],
            floats[10], words[11], floats[12], words[13], floats[14],
            *words[15:18], *floats[18:34],
            (floats[34], floats[35], floats[36]),
            (floats[37], floats[38], floats[39]),
            *floats[40:44], words[44], floats[45], *words[46:49],
        )

    @property
    def collimator_serial(self) -> int:
        return self.collimator_low | (self.collimator_high << 32)

    @property
    def qc_adc_1_connected(self) -> bool:
        return bool(self.qc_adc_i2c_status & 1)

    @property
    def qc_adc_2_connected(self) -> bool:
        return bool(self.qc_adc_i2c_status & 2)


@dataclass(frozen=True)
class BeamQaResult:
    channel_0_accumulation: int
    channel_1_accumulation: int
    channel_0_sample_count: int
    channel_1_sample_count: int
    status: QcSessionStatus

    @classmethod
    def decode(cls, packet: Packet) -> BeamQaResult:
        words = _payload(packet, 115, 5)
        return cls(*words[:4], QcSessionStatus(words[4]))


@dataclass(frozen=True)
class FaultMessage:
    """A fault record, including clear-marker records whose fault_type is zero."""

    fault_type: int
    format_hash: int
    clear_epoch: int
    entry_index: int
    active_count: int
    state: State
    runtime_ms: int
    format_text: str
    arguments: tuple[int, ...]

    @classmethod
    def decode(cls, packet: Packet) -> FaultMessage:
        from .protocol import ProtocolError

        words = _payload(packet, 102, 45)
        if words[7] > 5:
            raise ProtocolError(f"fault record declares {words[7]} arguments; maximum is 5")
        text = struct.pack("<32I", *words[8:40]).split(b"\0", 1)[0].decode("ascii")
        return cls(*words[:5], State(words[5]), words[6], text, words[40:40 + words[7]])

    @property
    def message(self) -> str:
        """Render the firmware's supported C format specifiers without losing raw args."""
        arguments = iter(self.arguments)

        def render(match: re.Match[str]) -> str:
            specifier = match.group()[1]
            if specifier == "%":
                return "%"
            value = next(arguments)
            if specifier == "f":
                return f"{word_float(value):f}"
            if specifier == "d":
                return str(value if value < 0x80000000 else value - 0x100000000)
            if specifier in ("x", "X"):
                return format(value, specifier)
            return str(value)

        return re.sub(r"%[%duxfX]", render, self.format_text)


@dataclass(frozen=True)
class OperationalPoint:
    total_time_s: float
    remaining_time_s: float
    kv: float
    ma: float
    heater_ma: float
    x_coil_ma: float = 0.0
    y_coil_ma: float = 0.0
    focus_coil_ma: float = 0.0

    @classmethod
    def beam_qa(cls, kv: int, duration_s: float = 2.0) -> OperationalPoint:
        """A simulator point at a supported QA energy; not a clinical prescription."""
        if kv not in (50, 70, 100):
            raise ValueError("Beam QA energy must be 50, 70, or 100 kV")
        return cls(duration_s, duration_s, float(kv), 1.0, 2500.0)

    def payload(self) -> tuple[int, ...]:
        return tuple(float_word(value) for value in (
            self.total_time_s, self.remaining_time_s, self.kv, self.ma,
            self.heater_ma, self.x_coil_ma, self.y_coil_ma, self.focus_coil_ma,
        ))

    @classmethod
    def decode(cls, packet: Packet) -> OperationalPoint:
        from .protocol import ProtocolError

        words = _payload(packet, 110, 9)
        if words[0] != 0:
            raise ProtocolError(f"operational-point query rejected: {words[0]}")
        return cls(*(word_float(word) for word in words[1:]))


def authenticated_payload(session_id: int, packet_type: int, words: tuple[int, ...]) -> tuple[int, ...]:
    """Match the firmware's additive uint32 authentication, including wraparound."""
    return (*words, (session_id + 0x12345678 + packet_type + sum(words)) & 0xFFFFFFFF)
