#!/usr/bin/env python3
"""Trigger MCUboot serial recovery and upload a signed firmware image.

The running application recognizes ``*B00TL\\n`` on its UART and resets into
MCUboot serial-recovery mode. The bootloader then accepts standard ``mcumgr``
serial commands on the same port.

Usage:
    python tools/upload_firmware.py --port COM19 firmware-signed.bin
    python tools/upload_firmware.py --port COM19 --skip-trigger firmware-signed.bin
    python tools/upload_firmware.py --port COM19 --bootloader-only
"""

import argparse
import struct
import subprocess
import sys
import time
from pathlib import Path

import serial

from create_mcuboot_image import IMAGE_MAGIC, hex_to_binary


DEFAULT_BAUD = 38400
DEFAULT_MTU = 128
BOOTLOADER_TRIGGER = b"*B00TL\n"
IMAGE_TLV_INFO_MAGIC = 0x6907
IMAGE_TLV_SHA256 = 0x0010


def send_bootloader_command(port: str, baud: int) -> None:
    """Ask the running application to reset into MCUboot serial recovery."""
    with serial.Serial(port, baud, timeout=1, write_timeout=1) as uart:
        uart.write(BOOTLOADER_TRIGGER)
        uart.flush()


def mcumgr_command(port: str, baud: int, mtu: int, *command: str) -> list[str]:
    """Build an mcumgr CLI command for this bootloader's serial transport."""
    connstring = f"dev={port},baud={baud},mtu={mtu}"
    return ["mcumgr", "--conntype", "serial", "--connstring", connstring, *command]


def run_mcumgr(port: str, baud: int, mtu: int, *command: str, capture_output: bool = False) -> subprocess.CompletedProcess[str]:
    """Run an mcumgr command, reporting a missing executable clearly."""
    try:
        return subprocess.run(
            mcumgr_command(port, baud, mtu, *command),
            check=False,
            text=True,
            capture_output=capture_output,
        )
    except FileNotFoundError as error:
        raise RuntimeError(
            "mcumgr was not found on PATH; install it with: "
            "go install github.com/apache/mynewt-mcumgr-cli/mcumgr@latest"
        ) from error


def wait_for_bootloader(port: str, baud: int, mtu: int, retries: int, delay: float) -> bool:
    """Wait until the board responds to the standard mcumgr echo request."""
    for attempt in range(1, retries + 1):
        print(f"Checking for bootloader (attempt {attempt}/{retries})...")
        result = run_mcumgr(port, baud, mtu, "echo", "ping", capture_output=True)
        output = (result.stdout or "") + (result.stderr or "")
        if result.returncode == 0 and "ping" in output:
            print("Bootloader is responding.")
            return True
        time.sleep(delay)
    return False


def read_image(image_path: Path) -> bytes:
    """Read a binary or Intel HEX image and require an MCUboot header."""
    image = hex_to_binary(image_path) if image_path.suffix.lower() == ".hex" else image_path.read_bytes()
    if len(image) < 4 or struct.unpack_from("<I", image)[0] != IMAGE_MAGIC:
        raise ValueError(
            "image is not MCUboot-formatted; generate a signed MCUboot image before uploading"
        )
    return image


def dump_image_hash(image: bytes) -> str:
    """Return the SHA-256 stored in an MCUboot image's regular TLV area."""
    header_size = struct.unpack_from("<H", image, 8)[0]
    image_size = struct.unpack_from("<I", image, 12)[0]
    tlv_offset = header_size + image_size
    if tlv_offset + 4 > len(image):
        raise ValueError("MCUboot image has no complete TLV area")

    tlv_magic, tlv_size = struct.unpack_from("<HH", image, tlv_offset)
    tlv_end = tlv_offset + tlv_size
    if tlv_magic != IMAGE_TLV_INFO_MAGIC or tlv_size < 4 or tlv_end > len(image):
        raise ValueError("MCUboot image has an invalid regular TLV area")

    offset = tlv_offset + 4
    while offset + 4 <= tlv_end:
        tlv_type, value_size = struct.unpack_from("<HH", image, offset)
        offset += 4
        if offset + value_size > tlv_end:
            raise ValueError("MCUboot image contains a truncated TLV record")
        if tlv_type == IMAGE_TLV_SHA256:
            if value_size != 32:
                raise ValueError("MCUboot image SHA-256 TLV has an invalid length")
            return image[offset:offset + value_size].hex()
        offset += value_size

    raise ValueError("MCUboot image does not contain a SHA-256 TLV")


def upload_firmware(port: str, baud: int, mtu: int, image_path: Path) -> int:
    """Upload to image 0, the only application slot in this bootloader."""
    print(f"Uploading {image_path} to image 0...")
    return run_mcumgr(port, baud, mtu, "image", "upload", "--image", "0", str(image_path)).returncode


def list_image_slots(port: str, baud: int, mtu: int, label: str) -> int:
    """Show the image slots reported by MCUboot."""
    print(f"Images reported by MCUboot {label}:")
    return run_mcumgr(port, baud, mtu, "image", "list").returncode


def reset_board(port: str, baud: int, mtu: int) -> int:
    """Request an MCUboot reset after a successful upload."""
    print("Resetting board...")
    return run_mcumgr(port, baud, mtu, "reset").returncode


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("image", type=Path, nargs="?", help="signed MCUboot image (.bin or .hex)")
    parser.add_argument("--port", required=True, help="serial port, e.g. COM19")
    parser.add_argument("--baud", type=int, default=DEFAULT_BAUD, help=f"baud rate (default: {DEFAULT_BAUD})")
    parser.add_argument("--mtu", type=int, default=DEFAULT_MTU, help=f"mcumgr serial MTU (default: {DEFAULT_MTU})")
    parser.add_argument("--retries", type=int, default=10, help="bootloader-detection retry count")
    parser.add_argument("--retry-delay", type=float, default=0.3, help="seconds between retries")
    parser.add_argument(
        "--skip-trigger",
        action="store_true",
        help="do not send *B00TL; use when the board is already in serial recovery",
    )
    parser.add_argument(
        "--bootloader-only",
        action="store_true",
        help="send *B00TL and exit without checking or uploading an image",
    )
    args = parser.parse_args()

    if args.retries < 1:
        parser.error("--retries must be at least 1")
    if args.retry_delay < 0:
        parser.error("--retry-delay cannot be negative")
    if args.mtu < 1:
        parser.error("--mtu must be positive")

    if args.bootloader_only:
        if args.image:
            parser.error("image cannot be used with --bootloader-only")
        print("Sending *B00TL to trigger bootloader mode...")
        send_bootloader_command(args.port, args.baud)
        return 0

    if args.image is None:
        parser.error("image is required unless --bootloader-only is used")
    if not args.image.is_file():
        parser.error(f"image file not found: {args.image}")

    try:
        image = read_image(args.image)
    except (OSError, ValueError) as error:
        print(f"Error: {error}", file=sys.stderr)
        return 1
    print(f"MCUboot image: {args.image.name} ({len(image)} bytes)")

    if not args.skip_trigger:
        print("Sending *B00TL to trigger bootloader mode...")
        send_bootloader_command(args.port, args.baud)
        time.sleep(0.3)

    if not wait_for_bootloader(args.port, args.baud, args.mtu, args.retries, args.retry_delay):
        print("Error: board did not respond in MCUboot serial-recovery mode.", file=sys.stderr)
        return 1

    image_list_result = list_image_slots(args.port, args.baud, args.mtu, "before upload")
    if image_list_result != 0:
        print("Error: could not list MCUboot images before upload.", file=sys.stderr)
        return image_list_result

    try:
        print(f"Upload image SHA-256: {dump_image_hash(image)}")
    except ValueError as error:
        print(f"Error: could not read the MCUboot image hash: {error}", file=sys.stderr)
        return 1

    upload_result = upload_firmware(args.port, args.baud, args.mtu, args.image)
    if upload_result != 0:
        print("Error: firmware upload failed.", file=sys.stderr)
        return upload_result

    image_list_result = list_image_slots(args.port, args.baud, args.mtu, "after upload")
    if image_list_result != 0:
        print("Error: could not list MCUboot images after upload.", file=sys.stderr)
        return image_list_result

    reset_result = reset_board(args.port, args.baud, args.mtu)
    if reset_result != 0:
        print("Error: board reset failed.", file=sys.stderr)
        return reset_result

    print("Upload complete; board reset requested.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, RuntimeError) as error:
        print(f"Error: {error}", file=sys.stderr)
        raise SystemExit(1)
