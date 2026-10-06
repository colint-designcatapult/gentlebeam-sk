"""TCP-backed simulator for the head-interface UART protocol."""

from __future__ import annotations

from binascii import crc_hqx
from collections.abc import Callable
from dataclasses import dataclass, replace
from threading import Condition, Event, RLock, Thread
import math
import socket
import struct
import time


HEAD_INTERFACE_ADDRESS = ("127.0.0.1", 41021)
SYNC = b"\xff" * 4
FIELD_DELIMITER = 0xA5
TERMINATOR = 0x99
# control_comm.c schedules normal-mode feedback every 100 ms.
FRAME_INTERVAL_SECONDS = 0.1
QC_STOPPED = 0
QC_ACTIVE = 1
QC_COMPLETE = 2
QC_ERROR = 3


@dataclass(frozen=True)
class HeadInterfaceFeedback:
    """One normal-mode head-to-main-control UART packet."""

    buttons: int = 0
    collimator_low: int = 0
    collimator_high: int = 0
    # Normal-build coolant limits: pressure 2..8, flow 2..6, temperature <=35.
    pressure: float = 4.0
    flow: float = 4.0
    temperature: float = 25.0
    mag_x_1: float = 0.0
    mag_y_1: float = 0.0
    mag_z_1: float = 0.0
    mag_x_2: float = 0.0
    mag_y_2: float = 0.0
    mag_z_2: float = 0.0
    qc_channel_0: int = 0
    qc_channel_1: int = 0
    qc_channel_0_connected: bool = True
    qc_channel_1_connected: bool = False
    qc_accumulation_0: int = 0
    qc_accumulation_1: int = 0
    qc_sample_count_0: int = 0
    qc_sample_count_1: int = 0
    qc_acquisition_state: int = 0


@dataclass(frozen=True)
class HeadInterfaceCommand:
    """A validated normal-mode main-control-to-head command packet."""

    led_sequence: int
    qc_desired_state: int
    raw: bytes
    received_at: float = 0.0


@dataclass(frozen=True)
class HeadInterfaceFrame:
    """A completed send, timestamped with the same monotonic clock as commands."""

    count: int
    sent_at: float
    feedback: HeadInterfaceFeedback
    raw: bytes
    corrupted: bool


def _u32(value: int) -> bytes:
    if not 0 <= value <= 0xFFFFFFFF:
        raise ValueError("UART integer fields must be unsigned 32-bit values")
    return struct.pack("<I", value)


def _field(payload: bytes) -> bytes:
    return payload + bytes((FIELD_DELIMITER,))


def encode_feedback(feedback: HeadInterfaceFeedback) -> bytes:
    """Encode a CRC-valid 105-byte normal-mode head-interface packet."""
    buttons = feedback.buttons & 0xFFFF
    qc_channel_0 = feedback.qc_channel_0 & 0x0FFF
    qc_channel_1 = feedback.qc_channel_1 & 0x0FFF
    if feedback.qc_channel_0_connected:
        qc_channel_0 |= 0x8000
    if feedback.qc_channel_1_connected:
        qc_channel_1 |= 0x8000

    fields = (
        _u32(0x88),
        _u32(buttons),
        _u32(feedback.collimator_low),
        _u32(feedback.collimator_high),
        struct.pack("<f", feedback.pressure),
        struct.pack("<f", feedback.flow),
        struct.pack("<f", feedback.temperature),
        struct.pack("<f", feedback.mag_x_1),
        struct.pack("<f", feedback.mag_y_1),
        struct.pack("<f", feedback.mag_z_1),
        struct.pack("<f", feedback.mag_x_2),
        struct.pack("<f", feedback.mag_y_2),
        struct.pack("<f", feedback.mag_z_2),
        _u32((qc_channel_1 << 16) | qc_channel_0),
        _u32(feedback.qc_accumulation_0),
        _u32(feedback.qc_accumulation_1),
        _u32(feedback.qc_sample_count_0),
        _u32(feedback.qc_sample_count_1),
        _u32(feedback.qc_acquisition_state),
    )
    body = SYNC + bytes((FIELD_DELIMITER,)) + b"".join(_field(field) for field in fields)
    return body + _u32(crc_hqx(body, 0x1D0F)) + bytes((TERMINATOR,))


def decode_command(packet: bytes) -> HeadInterfaceCommand:
    """Validate and decode a 12-byte normal-mode main-control command."""
    if len(packet) != 12 or packet[:4] != SYNC:
        raise ValueError("head-interface command must begin with four sync bytes")
    led_sequence, led_copy, led_inverse, led_inverse_copy = packet[4:8]
    qc_state, qc_state_copy, qc_inverse, qc_inverse_copy = packet[8:12]
    if led_sequence != led_copy or led_inverse != led_inverse_copy or led_inverse != 0xFF - led_sequence:
        raise ValueError("head-interface LED command duplication check failed")
    if qc_state != qc_state_copy or qc_inverse != qc_inverse_copy or qc_inverse != 0xFF - qc_state:
        raise ValueError("head-interface QC command duplication check failed")
    return HeadInterfaceCommand(led_sequence=led_sequence, qc_desired_state=qc_state, raw=packet)


class HeadInterfaceSimulator:
    """Exchange UART frames, recording commands independently of feedback failures.

    Automatic QC models the head's one physical ADC channel, 16-sample DMA
    half-batches, persistent desired states, and retained completion totals.
    The configurable sample rate is a simulation rate, not a hardware claim.
    Use ``configure_qc(auto_respond=False)`` for explicit state/error injection.
    """

    def __init__(
        self, *, frame_interval: float = FRAME_INTERVAL_SECONDS,
        address: tuple[str, int] = HEAD_INTERFACE_ADDRESS,
    ) -> None:
        if not math.isfinite(frame_interval) or frame_interval <= 0:
            raise ValueError("frame_interval must be finite and positive")
        self._frame_interval = frame_interval
        self._address = address
        self._listener: socket.socket | None = None
        self._connection: socket.socket | None = None
        self._stop = Event()
        self._connected = Event()
        self._thread: Thread | None = None
        self._lock = RLock()
        self._changed = Condition(self._lock)
        self._feedback = HeadInterfaceFeedback()
        self._pressed_buttons = 0
        self._error: Exception | None = None
        self._paused = False
        self._corruption: tuple[int, int] | None = None
        self._commands: list[HeadInterfaceCommand] = []
        self._last_frame: HeadInterfaceFrame | None = None
        self._auto_qc = True
        self._qc_start_delay = 0.0
        self._qc_stop_delay = 0.0
        self._qc_sample_rate = 1000.0
        self._qc_desired = QC_STOPPED
        self._qc_transition: tuple[int, float] | None = None
        self._qc_last_sample_at = 0.0

    def start(self) -> None:
        with self._lock:
            if self._listener is not None:
                raise RuntimeError("head-interface simulator is already listening")
            listener = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            try:
                listener.bind(self._address)
                listener.listen(1)
                listener.settimeout(0.1)
            except OSError:
                listener.close()
                raise
            self._listener = listener
            self._stop.clear()
            self._connected.clear()
            self._error = None
            self._thread = Thread(target=self._serve, args=(listener,), daemon=True)
            self._thread.start()

    @property
    def port(self) -> int:
        """Return the bound TCP port, including OS-selected ephemeral ports."""
        with self._lock:
            if self._listener is None:
                raise RuntimeError("head-interface simulator is not listening")
            return self._listener.getsockname()[1]

    def wait_connected(self, timeout: float = 3) -> None:
        deadline = time.monotonic() + timeout
        with self._changed:
            while not self._connected.is_set():
                self._wait(deadline, "host firmware did not connect to the head-interface TCP listener")
            self.assert_healthy()

    def stop(self) -> None:
        self._stop.set()
        with self._changed:
            connection = self._connection
            listener = self._listener
            thread = self._thread
            self._changed.notify_all()
        if connection is not None:
            try:
                connection.shutdown(socket.SHUT_RDWR)
            except OSError:
                pass
        if listener is not None:
            listener.close()
        if thread is not None:
            thread.join(timeout=2)
            if thread.is_alive():
                raise RuntimeError("head-interface worker did not stop")
        with self._lock:
            self._thread = None
            self._listener = None

    @property
    def feedback(self) -> HeadInterfaceFeedback:
        """Return an immutable, coherent sensor/acquisition snapshot."""
        with self._lock:
            self._advance_qc(time.monotonic())
            return self._feedback

    @property
    def commands(self) -> tuple[HeadInterfaceCommand, ...]:
        with self._lock:
            return tuple(self._commands)

    @property
    def last_frame(self) -> HeadInterfaceFrame | None:
        with self._lock:
            return self._last_frame

    @property
    def sent_frame_count(self) -> int:
        with self._lock:
            return self._last_frame.count if self._last_frame is not None else 0

    @property
    def last_sent_at(self) -> float | None:
        with self._lock:
            return self._last_frame.sent_at if self._last_frame is not None else None

    def wait_for_command(
        self,
        predicate: Callable[[HeadInterfaceCommand], bool] | None = None,
        *,
        led_sequence: int | None = None,
        qc_desired_state: int | None = None,
        after: float = 0.0,
        timeout: float = 3,
    ) -> HeadInterfaceCommand:
        """Wait for a matching received command strictly after a monotonic timestamp."""
        deadline = time.monotonic() + timeout
        with self._changed:
            index = 0
            while True:
                self.assert_healthy()
                for command in self._commands[index:]:
                    if (
                        command.received_at > after
                        and (led_sequence is None or command.led_sequence == led_sequence)
                        and (qc_desired_state is None or command.qc_desired_state == qc_desired_state)
                        and (predicate is None or predicate(command))
                    ):
                        return command
                index = len(self._commands)
                self._wait(deadline, "no matching head-interface command before deadline")

    def wait_for_feedback(self, *, after_count: int = 0, timeout: float = 3) -> HeadInterfaceFrame:
        """Wait for a sent frame; raw bytes include any deliberately stale-CRC corruption."""
        deadline = time.monotonic() + timeout
        with self._changed:
            while self._last_frame is None or self._last_frame.count <= after_count:
                self._wait(deadline, "no head-interface feedback sent before deadline")
            self.assert_healthy()
            return self._last_frame

    def _wait(self, deadline: float, message: str) -> None:
        self.assert_healthy()
        if self._stop.is_set():
            raise RuntimeError("head-interface simulator stopped while waiting")
        remaining = deadline - time.monotonic()
        if remaining <= 0:
            raise TimeoutError(message)
        self._changed.wait(remaining)

    def pause(self) -> HeadInterfaceFrame | None:
        """Suppress feedback only; return the last send, with no later send in flight."""
        with self._lock:
            self._paused = True
            return self._last_frame

    def resume(self) -> None:
        with self._lock:
            self._paused = False

    def set_corruption(self, byte_offset: int | None = None, *, xor_mask: int = 1) -> None:
        """XOR one byte of every encoded frame without recomputing its CRC; None disables."""
        if byte_offset is not None and not 0 <= byte_offset < 105:
            raise ValueError("byte_offset must identify a byte in the 105-byte feedback frame")
        if not 1 <= xor_mask <= 255:
            raise ValueError("xor_mask must be a nonzero byte")
        with self._lock:
            self._corruption = None if byte_offset is None else (byte_offset, xor_mask)

    def configure_qc(
        self, *, auto_respond: bool = True, start_delay: float = 0,
        stop_delay: float = 0, sample_rate: float = 1000,
    ) -> None:
        """Configure acknowledgments and sampling before starting a QC session.

        Disabling automatic responses leaves all QC feedback fields under
        ``set_feedback`` control, allowing missing acknowledgments and errors.
        Retransmitted desired states never restart acquisition or delay an ACK.
        """
        if any(not math.isfinite(value) or value < 0 for value in (start_delay, stop_delay, sample_rate)):
            raise ValueError("QC delays and sample rate must be finite and nonnegative")
        with self._lock:
            self._advance_qc(time.monotonic())
            self._auto_qc = auto_respond
            self._qc_start_delay = start_delay
            self._qc_stop_delay = stop_delay
            self._qc_sample_rate = sample_rate
            if not auto_respond:
                self._qc_transition = None

    def set_feedback(self, **values: int | float | bool) -> None:
        """Atomically set coolant, magnetometer, ADC, keypad, or explicit QC fields."""
        with self._lock:
            # Account for samples using the old ADC value before changing it.
            self._advance_qc(time.monotonic())
            feedback = replace(self._feedback, **values)
            encode_feedback(feedback)
            self._feedback = feedback

    def set_button_pressed(self, button_index: int, pressed: bool) -> None:
        """Model a debounced key: each new press flips bit 8+index; release retains it."""
        if not 0 <= button_index < 6:
            raise ValueError("head keypad button index must be between 0 and 5")
        mask = 1 << button_index
        with self._lock:
            if pressed == bool(self._pressed_buttons & mask):
                return
            if pressed:
                self._pressed_buttons |= mask
                self._feedback = replace(
                    self._feedback, buttons=self._feedback.buttons ^ (mask << 8)
                )
            else:
                self._pressed_buttons &= ~mask

    def assert_healthy(self) -> None:
        """Surface background protocol or transport failures in the test thread."""
        with self._lock:
            if self._error is not None:
                raise AssertionError("head-interface simulator failed") from self._error

    def _receive_command(self, command: HeadInterfaceCommand, now: float) -> None:
        self._advance_qc(now)
        self._commands.append(replace(command, received_at=now))
        desired = command.qc_desired_state
        if desired in (0, 1) and desired != self._qc_desired:
            self._qc_desired = desired
            if self._auto_qc:
                if desired == 1:
                    self._feedback = replace(
                        self._feedback, qc_acquisition_state=QC_STOPPED,
                        qc_accumulation_0=0, qc_accumulation_1=0,
                        qc_sample_count_0=0, qc_sample_count_1=0,
                        qc_channel_1=0, qc_channel_1_connected=False,
                    )
                    self._qc_transition = (QC_ACTIVE, now + self._qc_start_delay)
                elif self._feedback.qc_acquisition_state in (QC_STOPPED, QC_ACTIVE):
                    self._qc_transition = (QC_COMPLETE, now + self._qc_stop_delay)
        self._advance_qc(now)
        self._changed.notify_all()

    def _advance_qc(self, now: float) -> None:
        if not self._auto_qc:
            return
        transition = self._qc_transition
        until = min(now, transition[1]) if transition is not None else now
        if self._feedback.qc_acquisition_state == QC_ACTIVE and self._qc_sample_rate > 0:
            # Real firmware commits DMA half-buffers of 16 samples, uint32 wrapping.
            batches = (until - self._qc_last_sample_at) * self._qc_sample_rate / 16
            count = int(batches + 1e-7) * 16
            if count > 0:
                feedback = self._feedback
                self._feedback = replace(
                    feedback,
                    qc_accumulation_0=(feedback.qc_accumulation_0 + count * (feedback.qc_channel_0 & 0xFFF)) & 0xFFFFFFFF,
                    qc_sample_count_0=(feedback.qc_sample_count_0 + count) & 0xFFFFFFFF,
                    qc_channel_0_connected=True,
                    qc_channel_1=0, qc_channel_1_connected=False,
                    qc_accumulation_1=0, qc_sample_count_1=0,
                )
                self._qc_last_sample_at += count / self._qc_sample_rate
        if transition is not None and now >= transition[1]:
            if (
                transition[0] == QC_COMPLETE
                and self._feedback.qc_acquisition_state == QC_ACTIVE
                and self._feedback.qc_sample_count_0 % 32 == 16
                and self._qc_sample_rate > 0
            ):
                # Once the first half was committed, the head finishes the
                # 32-sample burst before acknowledging STOP. Before the first
                # half it discards the unfinished burst instead (qc.c).
                self._qc_transition = (
                    QC_COMPLETE, self._qc_last_sample_at + 16 / self._qc_sample_rate,
                )
                self._advance_qc(now)
                return
            state, changed_at = transition
            self._qc_transition = None
            self._feedback = replace(self._feedback, qc_acquisition_state=state)
            if state == QC_ACTIVE:
                self._qc_last_sample_at = changed_at
                self._advance_qc(now)

    def _serve(self, listener: socket.socket) -> None:
        try:
            while not self._stop.is_set():
                try:
                    connection, _ = listener.accept()
                except TimeoutError:
                    continue
                with connection:
                    connection.settimeout(min(0.02, self._frame_interval))
                    with self._changed:
                        self._connection = connection
                        self._connected.set()
                        self._changed.notify_all()
                    self._exchange(connection)
                return
        except Exception as error:
            if not self._stop.is_set():
                with self._changed:
                    self._error = error
                    self._changed.notify_all()
        finally:
            with self._changed:
                self._connection = None
                self._connected.clear()
                self._changed.notify_all()

    def _exchange(self, connection: socket.socket) -> None:
        buffer = bytearray()
        next_feedback = time.monotonic()
        while not self._stop.is_set():
            with self._changed:
                now = time.monotonic()
                self._advance_qc(now)
                if now >= next_feedback and not self._paused:
                    feedback = self._feedback
                    packet = encode_feedback(feedback)
                    if self._corruption is not None:
                        offset, mask = self._corruption
                        corrupted = bytearray(packet)
                        corrupted[offset] ^= mask
                        packet = bytes(corrupted)
                    # Serialize send and pause so pause's returned timestamp is definitive.
                    connection.sendall(packet)
                    sent_at = time.monotonic()
                    count = 1 if self._last_frame is None else self._last_frame.count + 1
                    self._last_frame = HeadInterfaceFrame(
                        count, sent_at, feedback, packet, self._corruption is not None,
                    )
                    next_feedback = sent_at + self._frame_interval
                    self._changed.notify_all()
            try:
                received = connection.recv(4096)
            except TimeoutError:
                continue
            if not received:
                raise ConnectionError("host closed the head-interface TCP connection")
            buffer.extend(received)
            while len(buffer) >= 12:
                command = decode_command(bytes(buffer[:12]))
                del buffer[:12]
                with self._changed:
                    self._receive_command(command, time.monotonic())
