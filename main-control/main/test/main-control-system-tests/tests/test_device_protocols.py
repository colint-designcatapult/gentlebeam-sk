"""Device identity and rejected PC commands through the live UDP transport."""

from __future__ import annotations

import struct
import time
import zlib

import pytest

from main_control_system_tests.evidence import Evidence
from main_control_system_tests.hvps import HvpsSimulator
from main_control_system_tests.protocol import MainControlClient, encode_packet


def _identity(client: MainControlClient, evidence: Evidence):
    deadline = time.monotonic() + 5
    while True:
        identity = client.query_device_information()
        if identity.hvps_version == "system-test-hvps":
            break
        if time.monotonic() >= deadline:
            evidence.record(
                "HVPS identity startup timeout",
                observed_identity=identity,
                expected_hvps_version="system-test-hvps",
                timeout_s=5,
            )
        assert time.monotonic() < deadline, identity
        time.sleep(0.05)
    evidence.record(
        "Installed firmware identity",
        observed_main_version=identity.main_version,
        expected_main_version="host-debug",
        observed_hvps_version=identity.hvps_version,
        expected_hvps_version="system-test-hvps",
        observed_main_mode=identity.main_mode,
        observed_hvps_mode=identity.hvps_mode,
        expected_mode=0,
    )
    assert identity.main_version == "host-debug"
    assert identity.main_mode == 0
    assert identity.hvps_mode == 0
    return identity


@pytest.mark.strictdoc("TC-H1FWMC-307")
def test_device_information_startup_refresh_and_periodic_report(
    client: MainControlClient, hvps: HvpsSimulator, evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-306, scope=function, role=Verifies)
    UID: TC-H1FWMC-307
    TITLE: Report Device Information to PC - Test Case

    STATEMENT: Requested and periodic information identifies both firmware releases
    and operating modes after startup.

    PREREQUISITES: Main control and the HVPS simulator have known release identities.

    STEPS:
    1. Request device information after startup and verify both releases and modes.
    2. Observe a fresh periodic report without requesting device information.
    3. Request device information again.

    EXPECTED_BEHAVIOR: Periodic and refreshed reports identify the same installed
    main-control and HVPS releases and normal operating modes.
    """
    expected = _identity(client, evidence)
    while True:
        try:
            client.device_information(timeout=0.02)
        except TimeoutError:
            break
    deadline = time.monotonic() + 1.5
    periodic_started = time.monotonic()
    while True:
        client.query_telemetry()
        try:
            periodic = client.device_information(timeout=0.2)
            break
        except TimeoutError:
            if time.monotonic() >= deadline:
                evidence.record(
                    "Periodic identity deadline (s)",
                    observed_elapsed_s=time.monotonic() - periodic_started,
                    deadline_s=1.5,
                    expected_identity=expected,
                )
            assert time.monotonic() < deadline, "No periodic device information"
    evidence.record(
        "Periodic device identity",
        observed_identity=periodic,
        expected_identity=expected,
        observed_elapsed_s=time.monotonic() - periodic_started,
        deadline_s=1.5,
    )
    assert periodic == expected
    refreshed = client.query_device_information()
    evidence.record(
        "Refreshed device identity",
        observed_identity=refreshed,
        expected_identity=expected,
    )
    assert refreshed == expected
    hvps.assert_healthy()


@pytest.mark.strictdoc("TC-H1FWMC-156")
@pytest.mark.parametrize("malformation", ("unknown_command", "payload_size", "sync", "checksum", "declared_size"))
def test_unrecognized_command_response(
    client: MainControlClient,
    malformation: str,
    evidence: Evidence,
) -> None:
    """
    @relation(RQ-H1FWMC-46, scope=function, role=Verifies)
    @relation(RQ-H1FWMC-306, scope=function, role=Verifies)
    UID: TC-H1FWMC-156
    TITLE: PC Ethernet Commands - Unrecognized Command Handling - Test Case

    STATEMENT: Unknown or malformed commands receive an unrecognized-command reply.

    PREREQUISITES: Command responses and known firmware identities are available.

    STEPS:
    1. Send an unknown command or a device-information request with invalid framing.
    2. Verify its unrecognized-command reply.
    3. Send a valid device-information request.

    EXPECTED_BEHAVIOR: The invalid request receives an unrecognized-command reply;
    a subsequent valid request still reports both firmware releases and modes.
    """
    request_id = 0x71560000
    if malformation == "unknown_command":
        datagram = encode_packet(0xFFFF, request_id)
    elif malformation == "payload_size":
        datagram = encode_packet(1, request_id)
    else:
        data = bytearray(encode_packet(1, request_id, (0,)))
        if malformation == "checksum":
            data[-1] ^= 1
        else:
            if malformation == "sync":
                data[0] ^= 1
            else:
                struct.pack_into("<I", data, 16, 2)
            struct.pack_into("<I", data, len(data) - 4, zlib.crc32(data[:-4]))
        datagram = bytes(data)
    client.send_raw(datagram)
    rejected = client.receive_response(0, request_id)
    evidence.record(
        "Invalid command framing reply",
        malformation=malformation,
        observed_packet_type=rejected.packet_type,
        expected_packet_type=100,
        observed_packet_id=rejected.packet_id,
        request_id=request_id,
    )
    assert rejected.packet_type == 100
    _identity(client, evidence)
