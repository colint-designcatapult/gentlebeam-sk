"""Lifecycle management for the host main-control firmware executable."""

from __future__ import annotations

from pathlib import Path
import subprocess

from .protocol import MainControlClient


class HostFirmware:
    """A host firmware process coupled to an already-open protocol client."""

    def __init__(
        self, executable: Path, client: MainControlClient, *,
        head_port: int = 41021, hvps_port: int = 41022, io_port: int = 8080,
    ) -> None:
        self._executable = executable
        self._client = client
        self._head_port = head_port
        self._hvps_port = hvps_port
        self._io_port = io_port
        self._process: subprocess.Popen[bytes] | None = None

    def start(self) -> None:
        """Start firmware and prove it is publishing UDP telemetry."""
        if self._process is not None:
            raise RuntimeError("host firmware is already running")
        if not self._executable.is_file():
            raise FileNotFoundError(f"host firmware executable not found: {self._executable}")
        self._process = subprocess.Popen(
            [
                str(self._executable),
                "--command-port", str(self._client.command_port),
                "--telemetry-port", str(self._client.telemetry_port),
                "--head-port", str(self._head_port),
                "--hvps-port", str(self._hvps_port),
                "--io-port", str(self._io_port),
                "--console-port", "0",
                "--extra-port", "0",
            ],
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
        )
        try:
            self._client.telemetry(timeout=3)
        except Exception:
            self.stop()
            raise

    def stop(self) -> None:
        """Terminate the firmware process, escalating to kill only when necessary."""
        if self._process is None:
            return
        self._process.terminate()
        try:
            self._process.wait(timeout=3)
        except subprocess.TimeoutExpired:
            self._process.kill()
            self._process.wait(timeout=3)
        finally:
            self._process = None
