from __future__ import annotations

from contextlib import ExitStack
from pathlib import Path
import socket
import tempfile
import time

import pytest
from filelock import FileLock

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.head_interface import HeadInterfaceSimulator
from main_control_system_tests.host import HostFirmware
from main_control_system_tests.hvps import HvpsSimulator
from main_control_system_tests.io_model import HostIOModel
from main_control_system_tests.protocol import MainControlClient

_evidence_key = pytest.StashKey[Evidence]()
_outcomes_key = pytest.StashKey[dict[str, str]]()


@pytest.fixture
def evidence(request: pytest.FixtureRequest) -> Evidence:
    observations = Evidence()
    request.node.stash[_evidence_key] = observations
    return observations


@pytest.hookimpl(wrapper=True)
def pytest_runtest_makereport(item: pytest.Item, call: pytest.CallInfo):
    report = yield
    outcomes = item.stash.setdefault(_outcomes_key, {})
    outcomes[report.when] = report.outcome
    if hasattr(report, "wasxfail"):
        outcomes[report.when] = "xpassed" if report.passed else "xfailed"
    if report.when == "teardown":
        if outcomes.get("setup") == "failed" or outcomes.get("teardown") == "failed":
            outcome = "ERROR"
        elif outcomes.get("call") == "failed":
            outcome = "FAILED"
        elif "xpassed" in outcomes.values():
            outcome = "XPASS"
        elif "xfailed" in outcomes.values():
            outcome = "XFAIL"
        elif "skipped" in outcomes.values():
            outcome = "SKIPPED"
        elif all(outcomes.get(phase) == "passed" for phase in ("setup", "call", "teardown")):
            outcome = "PASSED"
        else:
            outcome = "INCOMPLETE"
        protocols = ", ".join(
            str(uid) for marker in item.iter_markers("strictdoc") for uid in marker.args
        )
        header = f"EVIDENCE {outcome}: {item.nodeid}"
        if protocols:
            header += f" [{protocols}]"
        observations = item.stash.get(_evidence_key, None)
        records = observations.records if observations is not None else []
        text = "\n".join([header, *(records or ["No observations recorded."])])
        # User properties travel from xdist workers and are retained by JUnit.
        report.user_properties.append(("test_evidence", text))
        report.sections.append(("Test evidence", text))
    return report


class _EvidenceReporter:
    def __init__(self, config: pytest.Config) -> None:
        self.config = config

    def pytest_runtest_logreport(self, report: pytest.TestReport) -> None:
        if report.when != "teardown" or hasattr(self.config, "workerinput"):
            return
        terminal = self.config.pluginmanager.get_plugin("terminalreporter")
        if terminal is not None:
            for name, text in report.user_properties:
                if name == "test_evidence":
                    terminal.write_sep("-", "Test evidence")
                    terminal.write_line(text)


def pytest_configure(config: pytest.Config) -> None:
    config.pluginmanager.register(_EvidenceReporter(config), "test-evidence-reporter")


def pytest_addoption(parser: pytest.Parser) -> None:
    parser.addoption(
        "--host-executable",
        type=Path,
        default=Path(__file__).resolve().parents[4]
        / "out"
        / "cmake"
        / "host-debug"
        / "main"
        / "host"
        / "gryphon_control_host.exe",
        help="path to gryphon_control_host.exe",
    )


@pytest.fixture(scope="session")
def startup_lock() -> FileLock:
    # Share the reservation-to-bind handoff lock across workers and pytest runs.
    return FileLock(Path(tempfile.gettempdir()) / "main-control-system-tests-startup.lock")


@pytest.fixture
def head_interface(startup_lock: FileLock) -> HeadInterfaceSimulator:
    simulator = HeadInterfaceSimulator(address=("127.0.0.1", 0))
    with startup_lock:
        simulator.start()
    try:
        yield simulator
    finally:
        simulator.stop()


@pytest.fixture
def io_model() -> HostIOModel:
    return HostIOModel()


@pytest.fixture
def hvps(io_model: HostIOModel, startup_lock: FileLock) -> HvpsSimulator:
    simulator = HvpsSimulator(io_model_reader=io_model.read, address=("127.0.0.1", 0))
    with startup_lock:
        simulator.start()
    try:
        yield simulator
    finally:
        simulator.stop()


@pytest.fixture
def startup_client(
    request: pytest.FixtureRequest,
    head_interface: HeadInterfaceSimulator,
    hvps: HvpsSimulator,
    io_model: HostIOModel,
    startup_lock: FileLock,
) -> MainControlClient:
    with ExitStack() as resources:
        # Keep reservations until immediately before spawn. Other test starts
        # cannot select these ports until the host has bound both endpoints.
        with startup_lock:
            with (
                socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as command,
                socket.socket(socket.AF_INET, socket.SOCK_STREAM) as http,
            ):
                command.bind(("127.0.0.1", 0))
                http.bind(("127.0.0.1", 0))
                command_port = command.getsockname()[1]
                io_port = http.getsockname()[1]
                io_model.url = f"http://127.0.0.1:{io_port}/io-model"
                protocol_client = resources.enter_context(
                    MainControlClient(command_port=command_port, telemetry_port=0)
                )
                firmware = HostFirmware(
                    request.config.getoption("host_executable"), protocol_client,
                    head_port=head_interface.port, hvps_port=hvps.port, io_port=io_port,
                )
                resources.callback(firmware.stop)
            firmware.start()
            deadline = time.monotonic() + 3
            while True:
                try:
                    io_model.read()
                    break
                except OSError:
                    if time.monotonic() >= deadline:
                        raise TimeoutError("host I/O model HTTP endpoint did not start")
                    time.sleep(0.05)
        try:
            head_interface.wait_connected()
            hvps.wait_connected()
            # ADC moving-average buffers start empty. Observe physical settling
            # before clearing startup thermal faults through the real PC command.
            protocol_client.wait_for_telemetry(
                lambda item: abs(item.heatsink_temperature - 25) < 2
                and abs(item.cabinet_temperature - 25) < 2
            )
            yield protocol_client
        finally:
            head_interface.assert_healthy()
            hvps.assert_healthy()


@pytest.fixture
def client(startup_client: MainControlClient) -> MainControlClient:
    startup_client.enter_cold()
    return startup_client
