"""Scalar parameter queries and periodic/requested device identity over PC UDP."""

from __future__ import annotations

import time

import pytest

from main_control_system_tests.hvps import HvpsSimulator
from main_control_system_tests.evidence import Evidence
from main_control_system_tests.pc_protocol import Directive, OperationalPoint, State
from main_control_system_tests.protocol import MainControlClient


def _begin_staging(client: MainControlClient) -> int:
    client.clear_plan()
    client.warmup()
    client.wait_for_state(State.PRIMED)
    session = client.new_session()
    client.wait_for_state(State.STAGING)
    return session


@pytest.mark.strictdoc("TC-H1FWMC-111")
def test_scalar_query_rejects_legacy_index(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-23, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-81, scope=function, role=Verifies)
    UID: TC-H1FWMC-111
    TITLE: Indexless Scalar Treatment Query - Test Case

    STATEMENT: An indexless query returns the programmed scalar point; an
    indexed legacy query is rejected without changing the point.

    PREREQUISITES: Healthy system in Staging and a programmed scalar point.

    STEPS:
    1. Program distinct delivery, electrical, and beam-optics parameters.
    2. Query without an index, then send a legacy query with index zero.
    3. Allow 0.2 seconds for an invalid response, then repeat the valid query.

    EXPECTED_BEHAVIOR: Valid responses contain success and exactly eight
    scalar values, with no point metadata. The indexed query produces no
    response, and a subsequent valid query returns the unchanged parameters.
    """
    session = _begin_staging(client)
    point = OperationalPoint(13.5, 12.25, 70.0, 1.25, 2450.0, -125.0, 225.0, 450.0)
    client.load_operational_point(session, point)
    response = client.command(10)
    evidence.record("Indexless scalar response", status=response.payload[0],
                    field_count=len(response.payload), expected_field_count=9,
                    observed=OperationalPoint.decode(response), expected=point)
    assert response.payload == (0, *point.payload())
    assert OperationalPoint.decode(response) == point
    legacy_id = client.send(10, 0)
    with pytest.raises(TimeoutError) as timeout:
        client.receive_response(10, legacy_id, timeout=0.2)
    unchanged = client.query_operational_point()
    evidence.record("Legacy indexed query rejected", request_id=legacy_id,
                    observed_error=type(timeout.value).__name__, expected_error="TimeoutError",
                    timeout_s=0.2, observed_point=unchanged, expected_point=point)
    assert unchanged == point


@pytest.mark.strictdoc("TC-H1FWMC-112")
def test_pc_commands_and_periodic_device_identity(
    client: MainControlClient, hvps: HvpsSimulator, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-84, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-306, scope=function, role=Verifies)
    UID: TC-H1FWMC-112
    TITLE: PC Command Responses and Device Information - Test Case

    STATEMENT: PC commands receive correlated responses, and requested and
    periodic device reports identify both firmware releases and modes.

    PREREQUISITES: Healthy normal-mode main control, its known build release,
    and an HVPS interface reporting its known release and normal mode.

    STEPS:
    1. Send device-information and telemetry commands in turn.
    2. Receive each response and verify its command type and request identifier.
    3. Query until the HVPS identity is available, then observe two periodic
       device-information reports without requesting device information.

    EXPECTED_BEHAVIOR: Each response matches its command type and identifier.
    Both periodic reports and the requested report contain the expected main
    and HVPS releases and normal modes, with unchanged main image CRC. Each
    periodic report arrives within 1.5 seconds after the preceding observation.
    """
    version_id = client.send(1, 0)
    version = client.receive_response(1, version_id)
    telemetry_id = client.send(4, 0)
    telemetry = client.receive_response(4, telemetry_id)
    evidence.record("PC response correlation", telemetry=(telemetry.packet_type, telemetry.packet_id),
                    expected_telemetry=(104, telemetry_id), version=(version.packet_type, version.packet_id),
                    expected_version=(101, version_id))
    assert (telemetry.packet_type, telemetry.packet_id) == (104, telemetry_id)
    assert (version.packet_type, version.packet_id) == (101, version_id)

    deadline = time.monotonic() + 5
    while True:
        requested = client.query_device_information()
        if requested.hvps_version == "system-test-hvps":
            break
        assert time.monotonic() < deadline, requested
        time.sleep(0.05)
    # CMake's host target explicitly builds this release identity.
    evidence.record("Requested device identity", main_version=requested.main_version,
                    expected_main_version="host-debug", main_mode=requested.main_mode, expected_main_mode=0,
                    hvps_version=requested.hvps_version, expected_hvps_version="system-test-hvps",
                    hvps_mode=requested.hvps_mode, expected_hvps_mode=0)
    assert requested.main_version == "host-debug"
    assert requested.main_mode == 0
    assert requested.hvps_version == "system-test-hvps"
    assert requested.hvps_mode == 0

    # Drain queued startup reports by waiting until the socket is caught up.
    # Version queries above also preserve rather than discard unsolicited data.
    while True:
        try:
            client.device_information(timeout=0.02)
        except TimeoutError:
            break
    for _ in range(2):
        # Telemetry requests keep communication healthy without soliciting identity.
        deadline = time.monotonic() + 1.5
        while True:
            client.query_telemetry()
            try:
                periodic = client.device_information(timeout=0.2)
                break
            except TimeoutError:
                assert time.monotonic() < deadline, "No periodic device information"
        evidence.record("Periodic device identity", observed=periodic, expected=requested,
                        elapsed_s=time.monotonic() - (deadline - 1.5), deadline_s=1.5)
        assert periodic == requested
    hvps.assert_healthy()


@pytest.mark.strictdoc("TC-H1FWMC-122")
def test_query_returns_current_programmed_scalar_parameters(client: MainControlClient, evidence: Evidence) -> None:
    """
    @relation(RQ-H1FWMC-81, scope=function, role=Verifies)
    UID: TC-H1FWMC-122
    TITLE: Current Programmed Treatment Parameter Reporting - Test Case

    STATEMENT: The PC query reports the current scalar treatment parameters,
    not a previous load or independently reconstructed values.

    PREREQUISITES: Healthy system and two distinct valid scalar points.

    STEPS:
    1. Enter Staging and load the first point; query every parameter.
    2. Replace the point, query again, stage and confirm it, then query again.
    3. Wipe the treatment and query once more.

    EXPECTED_BEHAVIOR: Every query exactly returns the currently programmed
    delivery times, acceleration potential, emission current, filament current,
    and three coil currents. Staging and confirmation preserve all values;
    wiping clears every value.
    """
    session = _begin_staging(client)
    points = (
        OperationalPoint(20.5, 19.25, 50.0, 1.5, 2400.0, -150.0, 200.0, 350.0),
        OperationalPoint(30.75, 29.5, 100.0, 2.0, 2600.0, 175.0, -225.0, 550.0),
    )
    for point in points:
        client.load_operational_point(session, point)
        queried = client.query_operational_point()
        evidence.record("Loaded scalar parameters (s, kV, mA)", observed=queried, expected=point)
        assert queried == point
    client.directive(Directive.STAGE_PLAN)
    client.wait_for_state(State.STAGED)
    client.confirm_operational_point(session, points[-1])
    confirmed = client.query_operational_point()
    evidence.record("Confirmed scalar parameters (s, kV, mA)", observed=confirmed, expected=points[-1])
    assert confirmed == points[-1]
    client.clear_plan()
    wiped = client.query_operational_point()
    evidence.record("Wiped scalar parameters (s, kV, mA)", observed=wiped,
                    expected=OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0))
    assert wiped == OperationalPoint(0, 0, 0, 0, 0, 0, 0, 0)
