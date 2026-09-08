#!/usr/bin/env python3
"""Trigger the board into mcuboot serial-recovery mode and upload firmware.

Sends "*BOOTL\\n" (the command the main app's FTDI parser recognizes, see
main/sensus_src/ftdi.c) to request serial recovery and reset the board. The
bootloader consumes that one-shot request and starts mcumgr; the script then
checks for mcumgr and uploads the given image. Use --skip-trigger if the board
is already sitting in recovery mode (e.g. blank primary slot).

Usage:
    python upload_firmware.py --port COM5 path\\to\\firmware.mcuboot.bin
    python upload_firmware.py --port COM5 --bootloader-only
"""
import argparse
from pathlib import Path
import re
import subprocess
import sys
import time

import serial


def send_bootloader_command(port: str, baud: int) -> None:
    with serial.Serial(port, baud, timeout=1) as ser:
        ser.write(b"*BOOTL\n")
        ser.flush()


def ensure_mcumgr_connection(conn_name: str, port: str, baud: int, mtu: int) -> None:
    connstring = f"dev={port},baud={baud},mtu={mtu}"
    subprocess.run(
        ["mcumgr", "conn", "add", conn_name, "type=serial", f"connstring={connstring}"],
        check=False,
        capture_output=True,
    )


def wait_for_bootloader(conn_name: str, retries: int, delay: float) -> bool:
    for attempt in range(1, retries + 1):
        print(f"Checking for bootloader (attempt {attempt}/{retries})...")
        result = subprocess.run(
            ["mcumgr", "-c", conn_name, "echo", "ping"],
            capture_output=True,
            text=True,
        )
        if result.returncode == 0 and "ping" in result.stdout:
            print("Bootloader is responding.")
            return True
        time.sleep(delay)
    return False


def dump_image_info(image_path: str) -> int:
    imgtool_path = (
        Path(__file__).resolve().parents[2]
        / ".."
        / "mcuboot"
        / "mcuboot-2.4.0"
        / "scripts"
        / "imgtool.py"
    )
    print(f"Inspecting {image_path} ...")
    result = subprocess.run([sys.executable, str(imgtool_path), "dumpinfo", image_path])
    return result.returncode


def upload_firmware(conn_name: str, image_path: str) -> int:
    print(f"Uploading {image_path} ...")
    result = subprocess.run(
        ["mcumgr", "-c", conn_name, "image", "upload", "--image", "2", image_path]
    )
    return result.returncode


def list_image_slots(conn_name: str) -> subprocess.CompletedProcess[str]:
    print("Images in primary (slot 0) and secondary (slot 1):")
    result = subprocess.run(
        ["mcumgr", "-c", conn_name, "image", "list"],
        capture_output=True,
        text=True,
    )
    if result.stdout:
        print(result.stdout, end="" if result.stdout.endswith("\n") else "\n")
    if result.stderr:
        print(result.stderr, end="" if result.stderr.endswith("\n") else "\n", file=sys.stderr)
    return result


def secondary_image_hash(image_list: str) -> str | None:
    slot_match = re.search(
        r"^\s*image=0\s+slot=1\b(?:(?!^\s*image=).)*?^\s*hash:\s*([0-9a-f]+)\s*$",
        image_list,
        re.IGNORECASE | re.MULTILINE | re.DOTALL,
    )
    return slot_match.group(1) if slot_match else None


def confirm_image(conn_name: str, image_hash: str) -> int:
    print(f"Marking secondary image {image_hash} for a permanent swap...")
    result = subprocess.run(["mcumgr", "-c", conn_name, "image", "confirm", image_hash])
    return result.returncode


def reset_board(conn_name: str) -> int:
    print("Resetting board...")
    result = subprocess.run(["mcumgr", "-c", conn_name, "reset"])
    return result.returncode


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("image", nargs="?", help="path to the mcuboot-formatted firmware image to upload")
    parser.add_argument("--port", required=True, help="serial port, e.g. COM5")
    parser.add_argument("--baud", type=int, default=115200, help="baud rate (default: 115200)")
    parser.add_argument("--mtu", type=int, default=124, help="mcumgr serial MTU (default: 124)")
    parser.add_argument("--conn", default="main-control-ftdi", help="mcumgr connection profile name")
    parser.add_argument("--retries", type=int, default=10, help="bootloader-detection retry count")
    parser.add_argument("--retry-delay", type=float, default=1.0, help="seconds between retries")
    parser.add_argument(
        "--skip-trigger",
        action="store_true",
        help="skip sending *BOOTL and just check/upload (board already in recovery)",
    )
    parser.add_argument(
        "--bootloader-only",
        action="store_true",
        help="send *BOOTL and exit without running mcumgr or uploading an image",
    )
    args = parser.parse_args()

    if args.bootloader_only:
        if args.image:
            parser.error("image cannot be used with --bootloader-only")
        print("Sending *BOOTL to trigger bootloader mode...")
        send_bootloader_command(args.port, args.baud)
        return 0

    if not args.image:
        parser.error("image is required unless --bootloader-only is used")

    ensure_mcumgr_connection(args.conn, args.port, args.baud, args.mtu)

    if not args.skip_trigger:
        print("Sending *BOOTL to trigger bootloader mode...")
        send_bootloader_command(args.port, args.baud)
        time.sleep(1.0)

    if not wait_for_bootloader(args.conn, args.retries, args.retry_delay):
        print("Error: board did not respond as expected in bootloader mode.", file=sys.stderr)
        return 1

    rc = dump_image_info(args.image)
    if rc != 0:
        print("Error: image is not a valid MCUboot image.", file=sys.stderr)
        return rc

    rc = upload_firmware(args.conn, args.image)
    if rc != 0:
        print("Error: firmware upload failed.", file=sys.stderr)
        return rc

    image_list = list_image_slots(args.conn)
    if image_list.returncode != 0:
        print("Error: could not list image slots after upload.", file=sys.stderr)
        return image_list.returncode

    image_hash = secondary_image_hash(image_list.stdout)
    if image_hash is None:
        print(
            "Error: uploaded image is not valid in secondary slot 1; resetting without confirmation.",
            file=sys.stderr,
        )
        rc = reset_board(args.conn)
        if rc != 0:
            print("Error: board reset failed.", file=sys.stderr)
            return rc
        return 1

    rc = confirm_image(args.conn, image_hash)
    if rc != 0:
        print("Error: could not mark the uploaded image for a permanent swap.", file=sys.stderr)
        return rc

    rc = reset_board(args.conn)
    if rc != 0:
        print("Error: board reset failed.", file=sys.stderr)
        return rc

    print("Upload complete; permanent swap marked and board reset.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
