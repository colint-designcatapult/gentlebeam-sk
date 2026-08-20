#!/usr/bin/env python3
"""Inject bootloader CRCs into Intel HEX firmware images and merge images safely."""

from __future__ import annotations

import argparse
import binascii
from pathlib import Path
from typing import Iterable


class HexFormatError(ValueError):
    pass


def parse_hex(path: Path) -> dict[int, int]:
    image: dict[int, int] = {}
    upper_address = 0
    eof_seen = False

    for line_number, line in enumerate(path.read_text(encoding="ascii").splitlines(), 1):
        if not line.startswith(":"):
            raise HexFormatError(f"{path}:{line_number}: missing record marker")
        try:
            record = bytes.fromhex(line[1:])
        except ValueError as error:
            raise HexFormatError(f"{path}:{line_number}: invalid hexadecimal record") from error
        if len(record) < 5 or record[0] + 5 != len(record):
            raise HexFormatError(f"{path}:{line_number}: invalid record length")
        if sum(record) & 0xFF:
            raise HexFormatError(f"{path}:{line_number}: invalid record checksum")

        length, address, record_type = record[0], int.from_bytes(record[1:3], "big"), record[3]
        data = record[4:-1]
        if record_type == 0x00:
            absolute_address = upper_address + address
            for offset, value in enumerate(data):
                destination = absolute_address + offset
                if destination in image and image[destination] != value:
                    raise HexFormatError(f"{path}:{line_number}: conflicting data at 0x{destination:08X}")
                image[destination] = value
        elif record_type == 0x01:
            if length != 0:
                raise HexFormatError(f"{path}:{line_number}: malformed EOF record")
            eof_seen = True
        elif record_type == 0x02:
            if length != 2 or address != 0:
                raise HexFormatError(f"{path}:{line_number}: malformed extended segment address record")
            upper_address = int.from_bytes(data, "big") << 4
        elif record_type == 0x04:
            if length != 2 or address != 0:
                raise HexFormatError(f"{path}:{line_number}: malformed extended linear address record")
            upper_address = int.from_bytes(data, "big") << 16
        elif record_type in (0x03, 0x05):
            if length != 4 or address != 0:
                raise HexFormatError(f"{path}:{line_number}: malformed start address record")
        else:
            raise HexFormatError(f"{path}:{line_number}: unknown record type {record_type:02X}")

    if not eof_seen:
        raise HexFormatError(f"{path}: missing EOF record")
    return image


def record(address: int, record_type: int, data: bytes) -> str:
    payload = bytes((len(data),)) + address.to_bytes(2, "big") + bytes((record_type,)) + data
    checksum = (-sum(payload)) & 0xFF
    return f":{(payload + bytes((checksum,))).hex().upper()}"


def write_hex(path: Path, image: dict[int, int]) -> None:
    lines: list[str] = []
    current_upper_address: int | None = None
    addresses = sorted(image)
    index = 0
    while index < len(addresses):
        address = addresses[index]
        upper_address = address >> 16
        if upper_address != current_upper_address:
            lines.append(record(0, 0x04, upper_address.to_bytes(2, "big")))
            current_upper_address = upper_address
        start = address
        data = bytearray()
        while index < len(addresses) and len(data) < 16:
            address = addresses[index]
            if address >> 16 != upper_address or address != start + len(data):
                break
            data.append(image[address])
            index += 1
        lines.append(record(start & 0xFFFF, 0x00, bytes(data)))
    lines.append(record(0, 0x01, b""))
    path.write_text("\n".join(lines) + "\n", encoding="ascii")


def crc32_iso_hdlc(data: Iterable[int]) -> int:
    return binascii.crc32(bytes(data)) & 0xFFFFFFFF


def inject_crc(input_path: Path, output_path: Path, application_start: int, application_length: int, crc_address: int) -> int:
    if application_length <= 0:
        raise ValueError("application length must be positive")
    if application_start <= crc_address < application_start + application_length:
        raise ValueError("CRC address must not be inside the CRC input range")

    image = parse_hex(input_path)
    crc = crc32_iso_hdlc(image.get(address, 0xFF) for address in range(application_start, application_start + application_length))
    for offset, value in enumerate(crc.to_bytes(4, "little")):
        image[crc_address + offset] = value
    write_hex(output_path, image)
    return crc


def combine_hex(input_paths: list[Path], output_path: Path) -> None:
    merged: dict[int, int] = {}
    for input_path in input_paths:
        for address, value in parse_hex(input_path).items():
            if address in merged and merged[address] != value:
                raise HexFormatError(f"conflicting data at 0x{address:08X} while merging {input_path}")
            merged[address] = value
    write_hex(output_path, merged)


def integer(value: str) -> int:
    return int(value, 0)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)

    inject = commands.add_parser("inject-crc", help="write an application CRC at the bootloader's CRC address")
    inject.add_argument("--input", type=Path, required=True)
    inject.add_argument("--output", type=Path, required=True)
    inject.add_argument("--crc-output", type=Path)
    inject.add_argument("--application-start", type=integer, required=True)
    inject.add_argument("--application-length", type=integer, required=True)
    inject.add_argument("--crc-address", type=integer, required=True)

    combine = commands.add_parser("combine", help="merge non-conflicting Intel HEX images")
    combine.add_argument("--input", type=Path, required=True, action="append")
    combine.add_argument("--output", type=Path, required=True)

    arguments = parser.parse_args()
    if arguments.command == "inject-crc":
        crc = inject_crc(arguments.input, arguments.output, arguments.application_start, arguments.application_length, arguments.crc_address)
        if arguments.crc_output:
            arguments.crc_output.write_text(f"{crc:08X}\n", encoding="ascii")
        print(f"Wrote CRC 0x{crc:08X} to {arguments.output}")
    else:
        combine_hex(arguments.input, arguments.output)
        print(f"Wrote combined image to {arguments.output}")


if __name__ == "__main__":
    main()
