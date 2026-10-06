"""PC-Firmware Communication Protocol framing and UDP client."""

from __future__ import annotations

from collections.abc import Callable
from dataclasses import dataclass
import select
import socket
import struct
import time
import zlib

from .pc_protocol import (
    BeamQaResult,
    DeviceInformation,
    Directive,
    FaultMessage,
    NormalTelemetry,
    OperationalPoint,
    QcSessionStatus,
    State,
    authenticated_payload,
    float_word,
)


SYNC = 0xFFFFFFFFFFFFFFFF
HEADER_FORMAT = "<QIII"
HEADER_SIZE = struct.calcsize(HEADER_FORMAT)
CRC_SIZE = 4
MIN_PACKET_SIZE = HEADER_SIZE + CRC_SIZE
RESPONSE_TYPE_OFFSET = 100
TELEMETRY_PORT = 40020


class ProtocolError(AssertionError):
    """A UDP datagram violates the PC-Firmware Communication Protocol."""


@dataclass(frozen=True)
class Packet:
    """A CRC-validated PC-Firmware Communication Protocol packet."""

    packet_type: int
    packet_id: int
    payload: tuple[int, ...]

    @classmethod
    def decode(cls, datagram: bytes) -> "Packet":
        if len(datagram) < MIN_PACKET_SIZE:
            raise ProtocolError(f"packet is {len(datagram)} bytes; minimum is {MIN_PACKET_SIZE}")
        sync, packet_type, packet_id, word_count = struct.unpack_from(HEADER_FORMAT, datagram)
        expected_size = MIN_PACKET_SIZE + word_count * 4
        if sync != SYNC:
            raise ProtocolError(f"invalid synchronization value 0x{sync:016X}")
        if len(datagram) != expected_size:
            raise ProtocolError(
                f"packet has {len(datagram)} bytes but header declares {word_count} words ({expected_size} bytes)"
            )
        expected_crc = zlib.crc32(datagram[:-CRC_SIZE]) & 0xFFFFFFFF
        actual_crc, = struct.unpack_from("<I", datagram, len(datagram) - CRC_SIZE)
        if actual_crc != expected_crc:
            raise ProtocolError(f"invalid CRC 0x{actual_crc:08X}; expected 0x{expected_crc:08X}")
        payload = struct.unpack_from(f"<{word_count}I", datagram, HEADER_SIZE) if word_count else ()
        return cls(packet_type=packet_type, packet_id=packet_id, payload=payload)


def encode_packet(packet_type: int, packet_id: int, payload: tuple[int, ...] = ()) -> bytes:
    """Encode a command packet using the firmware's little-endian wire format."""
    if any(word < 0 or word > 0xFFFFFFFF for word in payload):
        raise ValueError("payload words must be unsigned 32-bit integers")
    body = struct.pack(HEADER_FORMAT, SYNC, packet_type, packet_id, len(payload))
    body += struct.pack(f"<{len(payload)}I", *payload) if payload else b""
    return body + struct.pack("<I", zlib.crc32(body) & 0xFFFFFFFF)


class MainControlClient:
    """UDP client for main-control."""

    def __init__(
        self, host: str = "127.0.0.1", command_port: int = 20, timeout: float = 1.0,
        *, telemetry_port: int = TELEMETRY_PORT,
    ) -> None:
        self._target = (host, command_port)
        self._timeout = timeout
        self._telemetry_port = telemetry_port
        self._packet_id = 0
        self._command_socket: socket.socket | None = None
        self._telemetry_socket: socket.socket | None = None
        self._pending: dict[socket.socket, list[Packet]] = {}

    def __enter__(self) -> "MainControlClient":
        self.open()
        return self

    def __exit__(self, *_: object) -> None:
        self.close()

    def open(self) -> None:
        """Bind an ephemeral command socket and the requested telemetry socket."""
        self.close()
        try:
            self._command_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            self._command_socket.bind((self._target[0], 0))
            self._telemetry_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            self._telemetry_socket.bind((self._target[0], self._telemetry_port))
            self._pending = {self._command_socket: [], self._telemetry_socket: []}
        except OSError:
            self.close()
            raise

    @property
    def command_port(self) -> int:
        """Return the firmware command destination port."""
        return self._target[1]

    @property
    def telemetry_port(self) -> int:
        """Return the client's bound telemetry port."""
        return self._telemetry_socket_or_raise().getsockname()[1]

    def close(self) -> None:
        """Close sockets owned by this client."""
        if self._command_socket is not None:
            self._command_socket.close()
            self._command_socket = None
        if self._telemetry_socket is not None:
            self._telemetry_socket.close()
            self._telemetry_socket = None
        self._pending.clear()

    def command(self, packet_type: int, *payload: int, timeout: float | None = None) -> Packet:
        """Send one command and return its matching response."""
        packet_id = self.send(packet_type, *payload)
        return self.receive_response(packet_type, packet_id, timeout)

    def send(self, packet_type: int, *payload: int) -> int:
        """Send a command and return its packet identifier."""
        packet_id = self._packet_id
        self._packet_id = (self._packet_id + 1) & 0xFFFFFFFF
        self._command_socket_or_raise().sendto(
            encode_packet(packet_type, packet_id, tuple(payload)), self._target
        )
        return packet_id

    def send_raw(self, datagram: bytes) -> None:
        """Send an intentionally malformed datagram for parser tests."""
        self._command_socket_or_raise().sendto(datagram, self._target)

    def receive_response(self, request_type: int, packet_id: int, timeout: float | None = None) -> Packet:
        """Receive the response matching one request type and identifier."""
        return self._receive_matching(
            self._command_socket_or_raise(), request_type + RESPONSE_TYPE_OFFSET, packet_id, timeout
        )

    def telemetry(self, timeout: float | None = None) -> Packet:
        """Receive one unsolicited system telemetry packet."""
        return self._receive_matching(self._telemetry_socket_or_raise(), 104, None, timeout)

    def _receive_matching(
        self, sock: socket.socket, expected_type: int, expected_id: int | None, timeout: float | None
    ) -> Packet:
        """Pump both sockets and retain every packet not selected by this call."""
        deadline = time.monotonic() + (self._timeout if timeout is None else timeout)
        identifier = "" if expected_id is None else f" with ID {expected_id}"
        timeout_message = f"timed out waiting for packet type {expected_type}{identifier}"
        while True:
            pending = self._pending[sock]
            for index, packet in enumerate(pending):
                if packet.packet_type == expected_type and (expected_id is None or packet.packet_id == expected_id):
                    return pending.pop(index)
            remaining = deadline - time.monotonic()
            if remaining <= 0:
                raise TimeoutError(timeout_message)
            readable, _, _ = select.select(list(self._pending), [], [], remaining)
            if not readable:
                raise TimeoutError(timeout_message)
            for ready in readable:
                datagram, _ = ready.recvfrom(65535)
                self._pending[ready].append(Packet.decode(datagram))

    def unsolicited(self, packet_type: int, timeout: float | None = None) -> Packet:
        """Select an unsolicited response without discarding other telemetry."""
        return self._receive_matching(self._telemetry_socket_or_raise(), packet_type, None, timeout)

    def normal_telemetry(self, timeout: float | None = None) -> NormalTelemetry:
        return NormalTelemetry.decode(self.telemetry(timeout))

    def query_telemetry(self, timeout: float | None = None) -> NormalTelemetry:
        """Obtain a fresh snapshot correlated to a command, also feeding the PC watchdog."""
        return NormalTelemetry.decode(self.command(4, 0, timeout=timeout))

    def query_device_information(self, timeout: float | None = None) -> DeviceInformation:
        return DeviceInformation.decode(self.command(1, 0, timeout=timeout))

    def device_information(self, timeout: float | None = None) -> DeviceInformation:
        return DeviceInformation.decode(self.unsolicited(101, timeout))

    def query_operational_point(self, timeout: float | None = None) -> OperationalPoint:
        return OperationalPoint.decode(self.command(10, timeout=timeout))

    def beam_qa_result(self, timeout: float = 10.0) -> BeamQaResult:
        """Wait for beam-off, then obtain the final QC result using the PC stop/poll protocol.

        Current firmware publishes no unsolicited type-115 result. Asking it to stop
        QC before beam-off would truncate the acquisition, so first observe beam-off.
        """
        deadline = time.monotonic() + timeout
        self.wait_for_telemetry(
            lambda telemetry: telemetry.state not in (State.EMISSION, State.LAUNCHING),
            timeout,
        )
        while (remaining := deadline - time.monotonic()) > 0:
            result = BeamQaResult.decode(self.command(15, 2, 0, timeout=min(self._timeout, remaining)))
            if result.status in (QcSessionStatus.COMPLETE, QcSessionStatus.ERROR):
                return result
            if result.status not in (
                QcSessionStatus.STARTING, QcSessionStatus.ACCUMULATING, QcSessionStatus.STOPPING,
            ):
                raise ProtocolError(f"unexpected Beam QA result status: {result}")
            time.sleep(min(0.05, max(0.0, deadline - time.monotonic())))
        raise TimeoutError("Beam QA did not produce a terminal result")

    def fault_message(self, timeout: float | None = None) -> FaultMessage:
        return FaultMessage.decode(self.unsolicited(102, timeout))

    def wait_for_telemetry(
        self,
        predicate: Callable[[NormalTelemetry], bool],
        timeout: float = 10.0,
        poll_interval: float = 0.05,
    ) -> NormalTelemetry:
        """Poll fresh snapshots, retaining unsolicited packets and maintaining PC keepalive."""
        deadline = time.monotonic() + timeout
        latest: NormalTelemetry | None = None
        while (remaining := deadline - time.monotonic()) > 0:
            try:
                latest = self.query_telemetry(timeout=min(self._timeout, remaining))
            except TimeoutError as error:
                raise TimeoutError(f"telemetry wait timed out; latest={latest}") from error
            if predicate(latest):
                return latest
            time.sleep(min(poll_interval, max(0.0, deadline - time.monotonic())))
        raise TimeoutError(f"telemetry predicate not satisfied within {timeout}s; latest={latest}")

    def wait_for_state(self, state: State, timeout: float = 10.0) -> NormalTelemetry:
        return self.wait_for_telemetry(lambda telemetry: telemetry.state == state, timeout)

    @staticmethod
    def _require_success(packet: Packet) -> Packet:
        if not packet.payload or any(packet.payload):
            raise ProtocolError(f"command response {packet.packet_type} rejected: {packet.payload}")
        return packet

    def directive(self, directive: Directive) -> Packet:
        return self._require_success(self.command(3, int(directive), 1 << int(directive)))

    def clear_faults(self) -> Packet:
        return self.directive(Directive.CLEAR_FAULTS)

    def enter_cold(self, timeout: float = 10.0) -> NormalTelemetry:
        """Initialize startup or clear an existing fault; never mask an active fault source."""
        telemetry = self.query_telemetry()
        if telemetry.state == State.STARTUP:
            self.directive(Directive.STARTUP_INIT)
        elif telemetry.state == State.FAULT_DISCHARGE:
            self.wait_for_state(State.FAULT, timeout)
            self.clear_faults()
        elif telemetry.state in (State.COLD_FAULT, State.WARMUP_FAULT, State.FAULT):
            self.clear_faults()
        elif telemetry.state in (State.PRIMED, State.STAGING, State.STAGED, State.READY):
            self.directive(Directive.STANDBY)
        elif telemetry.state in (
            State.CONDITIONING, State.WARMUP, State.HVPS_CHECK, State.SETUP,
            State.LAUNCHING, State.EMISSION,
        ):
            self.directive(Directive.STOP)
        return self.wait_for_state(State.COLD, timeout)

    def warmup(self, heater_ma: float = 2500.0) -> Packet:
        return self._require_success(self.command(6, float_word(heater_ma)))

    def new_session(self) -> int:
        packet = self.command(7)
        if len(packet.payload) != 2 or packet.payload[0] != 0:
            raise ProtocolError(f"new session rejected: {packet.payload}")
        return packet.payload[1]

    def load_operational_point(self, session_id: int, point: OperationalPoint) -> Packet:
        return self._require_success(self.command(8, *authenticated_payload(session_id, 8, point.payload())))

    def confirm_operational_point(self, session_id: int, point: OperationalPoint) -> Packet:
        return self._require_success(self.command(9, *authenticated_payload(session_id, 9, point.payload())))

    def release_plan(self, session_id: int) -> Packet:
        return self._require_success(self.command(11, *authenticated_payload(session_id, 11, (1,))))

    def release_point(self, session_id: int) -> Packet:
        return self._require_success(self.command(11, *authenticated_payload(session_id, 11, (2,))))

    def arm_beam_qa(self) -> BeamQaResult:
        result = BeamQaResult.decode(self.command(15, 1, 0))
        if result.status != QcSessionStatus.ARMED:
            raise ProtocolError(f"Beam QA arm rejected: {result}")
        return result

    def stop_beam_qa(self) -> BeamQaResult:
        """Request stop and return current status; STOPPING is not yet a final result."""
        return BeamQaResult.decode(self.command(15, 2, 0))

    def prepare_emission(self, point: OperationalPoint, *, timeout: float = 20.0) -> int:
        """Execute the normal PC workflow up to READY with valid timers and a confirmed plan.

        Hardware simulators must already supply healthy inputs, collimator and feedback.
        No command exists to override firmware configuration or physical interlocks.
        """
        # Wipe a previous plan before warming: firmware otherwise resumes STAGED
        # instead of PRIMED after an interrupted emission.
        self.clear_plan(timeout)
        self.warmup()
        self.wait_for_state(State.PRIMED, timeout)
        self.directive(Directive.RESET_TIMERS)
        self.wait_for_telemetry(
            lambda telemetry: telemetry.internal_timer_s == telemetry.timer_1_s == telemetry.timer_2_s == 0,
            timeout,
        )
        session_id = self.new_session()
        self.wait_for_state(State.STAGING, timeout)
        self.load_operational_point(session_id, point)
        self.directive(Directive.STAGE_PLAN)
        self.wait_for_state(State.STAGED, timeout)
        self.confirm_operational_point(session_id, point)
        self.release_plan(session_id)
        self.wait_for_state(State.READY, timeout)
        return session_id

    def start_emission(
        self, session_id: int, *, beam_qa: bool = False, timeout: float = 20.0
    ) -> NormalTelemetry:
        if beam_qa:
            self.arm_beam_qa()
        self.release_point(session_id)
        return self.wait_for_state(State.EMISSION, timeout)

    def clear_plan(self, timeout: float = 10.0) -> None:
        """Return to COLD, then wipe the staged point through the normal directive."""
        self.enter_cold(timeout)
        self.directive(Directive.WIPE_PLAN)
        deadline = time.monotonic() + timeout
        while (remaining := deadline - time.monotonic()) > 0:
            packet = self.command(10, timeout=min(self._timeout, remaining))
            if len(packet.payload) != 9 or packet.payload[0] != 0:
                raise ProtocolError(f"operational-point query rejected: {packet.payload}")
            if not any(packet.payload[1:]):
                return
            time.sleep(min(0.05, max(0.0, deadline - time.monotonic())))
        raise TimeoutError("operational point was not cleared")

    def _command_socket_or_raise(self) -> socket.socket:
        if self._command_socket is None:
            raise RuntimeError("client is not open")
        return self._command_socket

    def _telemetry_socket_or_raise(self) -> socket.socket:
        if self._telemetry_socket is None:
            raise RuntimeError("client is not open")
        return self._telemetry_socket
