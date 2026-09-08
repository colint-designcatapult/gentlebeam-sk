#!/usr/bin/env python3
"""
MCUboot Manager - Combined firmware upload and diagnostic tool
Handles uploading MCUboot images and diagnosing boot issues

Wire format: standard MCUboot/mcumgr NLIP framing (base64 + CRC16-XMODEM,
"\x06\x09"/"\x04\x14" line markers), matching MCUBOOT_SERIAL_RAW_PROTOCOL
being disabled on the device. This is compatible with the stock `mcumgr` CLI.
"""

import argparse
import base64
import struct
import sys
import time
from pathlib import Path

from create_mcuboot_image import IMAGE_MAGIC, create_mcuboot_image, hex_to_binary

try:
    import cbor2
    import serial
except ImportError as error:
    raise SystemExit("Install dependencies with: python -m pip install -r tools/requirements.txt") from error


# MCUboot SMP Protocol Constants
GROUP_IMAGE = 1
GROUP_DEFAULT = 0
IMAGE_UPLOAD_ID = 1
RESET_ID = 5
# MCUboot always supports this default-group command as a lightweight ping.
MCUMGR_PING_ID = 1
CONSOLE_ECHO_CONTROL_ID = MCUMGR_PING_ID
OP_WRITE = 2
OP_READ = 0
MCUBOOT_INPUT_BUFFER = 512
MAX_PAYLOAD = 440

# NLIP line framing constants (must match boot_serial_priv.h / boot_serial.c)
PKT_START = bytes([6, 9])
DATA_START = bytes([4, 20])
FRAME_MTU = 124

# Diagnostic Constants
DIAGNOSTIC_TIMEOUT = 3.0  # seconds to wait for boot messages


def trace(direction, data):
    """Log UART communication (verbose mode)"""
    if data:
        print(f"UART {direction} {len(data)} bytes: {data.hex(' ')} | {data!r}")


def crc16_ccitt(data, crc=0):
    """CRC-16/XMODEM (poly 0x1021, init 0), matching the device's crc16_ccitt()"""
    for byte in data:
        crc ^= byte << 8
        for _ in range(8):
            crc = ((crc << 1) ^ 0x1021) & 0xFFFF if (crc & 0x8000) else (crc << 1) & 0xFFFF
    return crc


def write_uart(port, data, verbose=False):
    """Write raw bytes to UART with optional tracing"""
    if verbose:
        trace("TX", data)
    port.write(data)


def packet(op, group, sequence, command, payload):
    """Build an MCUboot nmgr_hdr + CBOR payload (not yet NLIP-framed)"""
    encoded = cbor2.dumps(payload)
    if len(encoded) + 8 > MCUBOOT_INPUT_BUFFER:
        raise ValueError("MCUboot SMP packet exceeds the device receive buffer")
    return struct.pack("!BBH HBB", op, 0, len(encoded), group, sequence, command) + encoded


def frame_lines(hdr_and_payload):
    """Wrap an nmgr_hdr+CBOR payload in NLIP base64/CRC framing lines"""
    crc = crc16_ccitt(hdr_and_payload)
    body = struct.pack("!H", len(hdr_and_payload) + 2) + hdr_and_payload + struct.pack("!H", crc)
    encoded = base64.b64encode(body)

    lines = bytearray()
    offset = 0
    while offset < len(encoded) or offset == 0:
        marker = PKT_START if offset == 0 else DATA_START
        chunk = encoded[offset:offset + FRAME_MTU]
        lines += marker + chunk + b"\n"
        offset += len(chunk)
        if len(chunk) < FRAME_MTU:
            break
    return bytes(lines)


def send_frame(port, hdr_and_payload, verbose=False):
    """NLIP-frame and write an nmgr_hdr+CBOR payload"""
    write_uart(port, frame_lines(hdr_and_payload), verbose)


def read_frame(port, verbose=False):
    """Reassemble one NLIP-framed nmgr_hdr+CBOR message from the port"""
    decoded = bytearray()
    total_len = None

    while True:
        line = port.readline()
        if verbose:
            trace("RX", line)
        if not line or not line.endswith(b"\n"):
            raise RuntimeError(
                f"Timed out waiting for an MCUboot response on {port.port}; "
                "verify that the updated bootloader is flashed and UART3 is connected"
            )
        if len(line) < 3:
            continue

        marker, b64_part = line[0:2], line[2:].rstrip(b"\n")
        if marker == PKT_START:
            decoded = bytearray()
            total_len = None
        elif marker != DATA_START:
            continue  # not a recognized NLIP line; ignore and keep reading

        padding = b"=" * (-len(b64_part) % 4)
        decoded += base64.b64decode(b64_part + padding)

        if total_len is None and len(decoded) >= 2:
            total_len = struct.unpack_from("!H", decoded, 0)[0]
        if total_len is not None and len(decoded) - 2 >= total_len:
            break

    body = bytes(decoded[2:2 + total_len])
    hdr_and_payload, crc_bytes = body[:-2], body[-2:]
    if crc16_ccitt(hdr_and_payload) != struct.unpack("!H", crc_bytes)[0]:
        raise RuntimeError("MCUboot response failed CRC check")

    op, _flags, length, group, sequence, command = struct.unpack("!BBH HBB", hdr_and_payload[:8])
    payload = hdr_and_payload[8:8 + length]
    return op, group, sequence, command, cbor2.loads(payload) if payload else {}


def send_upload(port, image, verbose=False):
    """Upload MCUboot image in chunks"""
    total = len(image)
    offset = 0
    sequence = 0
    while offset < total:
        chunk = image[offset:offset + MAX_PAYLOAD]
        payload = {"image": 0, "data": chunk, "len": total, "off": offset}
        send_frame(port, packet(OP_WRITE, GROUP_IMAGE, sequence, IMAGE_UPLOAD_ID, payload), verbose)
        port.flush()
        _, _, _, _, response = read_frame(port, verbose)
        if response.get("rc", 0) != 0:
            raise RuntimeError(f"MCUboot rejected offset {offset}: {response}")
        offset += len(chunk)
        sequence = (sequence + 1) & 0xFF
        print(f"Uploaded {offset}/{total} bytes ({offset * 100 // total}%)", end="\r", flush=True)
    print()


def ping_bootloader(port, verbose=False):
    """Test communication with the bootloader over the already-open UART."""
    port_name = getattr(port, "port", str(port))
    send_frame(port, packet(OP_READ, GROUP_DEFAULT, 0, CONSOLE_ECHO_CONTROL_ID, {}), verbose)
    port.flush()
    op, group, _sequence, command, response = read_frame(port, verbose)
    if op != OP_READ + 1 or group != GROUP_DEFAULT or command != CONSOLE_ECHO_CONTROL_ID:
        raise RuntimeError(f"Unexpected MCUboot ping response: op={op}, group={group}, command={command}, payload={response}")
    if response.get("rc", 0) != 0:
        raise RuntimeError(f"MCUboot ping rejected on {port_name}: {response}")

    print(f"✓ MCUboot communication established on {port_name}")


def reset_device(port, verbose=False):
    """Send reset command to device"""
    send_frame(port, packet(OP_WRITE, GROUP_DEFAULT, 0, RESET_ID, {}), verbose)
    port.flush()


def load_firmware_image(image_path):
    """Load a MCUboot binary or convert an Intel HEX input to upload bytes."""
    if image_path.suffix.lower() != ".hex":
        return image_path.read_bytes(), "MCUboot binary"

    image = hex_to_binary(image_path)
    if len(image) >= 4 and struct.unpack_from("<I", image)[0] == IMAGE_MAGIC:
        return image, "MCUboot Intel HEX"

    return create_mcuboot_image(image), "application Intel HEX converted to MCUboot image"


def ping_bootloader_periodic(port, timeout=5.0, verbose=False):
    """Periodically check if device is in bootloader mode over the open UART."""
    start_time = time.time()
    attempt = 0
    port_name = getattr(port, "port", str(port))

    while time.time() - start_time < timeout:
        attempt += 1
        try:
            send_frame(port, packet(OP_READ, GROUP_DEFAULT, attempt & 0xFF,
                                    CONSOLE_ECHO_CONTROL_ID, {}), verbose)
            port.flush()
            op, group, _sequence, command, response = read_frame(port, verbose)
            if (op == OP_READ + 1 and
                group == GROUP_DEFAULT and
                command == CONSOLE_ECHO_CONTROL_ID and
                response.get("rc", 0) == 0):
                print(f"✓ MCUboot communication established on {port_name} (attempt {attempt})")
                return True
        except (OSError, RuntimeError, ValueError):
            pass  # Timeout or error, retry
        
        # Wait before retrying
        elapsed = time.time() - start_time
        remaining = timeout - elapsed
        if remaining > 0:
            wait_time = min(0.3, remaining)  # 300ms between attempts
            time.sleep(wait_time)
    
    return False


def capture_boot_messages(port, timeout=None):
    """Capture bootloader startup messages for diagnostics"""
    print(f"\n{'='*70}")
    print("BOOTLOADER OUTPUT (waiting for messages...):")
    print(f"{'='*70}\n")
    
    output = b""
    start_time = time.time()
    
    while timeout is None or time.time() - start_time < timeout:
        if port.in_waiting:
            chunk = port.read(port.in_waiting)
            output += chunk
            try:
                decoded = chunk.decode('utf-8', errors='replace')
                print(decoded, end='', flush=True)
            except:
                pass
    
    return output


def analyze_boot_output(output):
    """Analyze bootloader output for errors"""
    print(f"\n{'='*70}")
    print("DIAGNOSTIC ANALYSIS:")
    print(f"{'='*70}\n")
    
    if not output:
        print("⚠️  No bootloader output captured.")
        print("   - Device may not have reset")
        print("   - UART may not be connected")
        print("   - Bootloader may have booted immediately")
        return
    
    # Check for specific error messages
    errors_found = False
    
    if b"ERR_ADDR" in output:
        errors_found = True
        print("❌ ERR_ADDR: Image address validation FAILED")
        print("   └─ Problem: br_image_off calculation incorrect")
        print("   └─ Fix: Verify create_mcuboot_image.py wrapper")
        print("   └─ Action: Regenerate firmware image\n")
    
    if b"ERR_SP" in output:
        errors_found = True
        print("❌ ERR_SP: Stack pointer validation FAILED")
        print("   └─ Problem: Stack pointer not in valid RAM (0x20000000-0x20008000)")
        print("   └─ Cause: Application linked for wrong address")
        print("   └─ Fix: Rebuild application for 0x0800C100, not 0x08000000")
        print("   └─ Action: Update app linker script FLASH origin\n")
    
    if b"ERR_RH" in output:
        errors_found = True
        print("❌ ERR_RH: Reset handler validation FAILED")
        print("   └─ Problem: Reset handler address outside application space")
        print("   └─ Cause: Application linked for wrong address")
        print("   └─ Fix: Rebuild application for 0x0800C100, not 0x08000000")
        print("   └─ Action: Update app linker script FLASH origin\n")

    if b"BOOT_GO_FAIL" in output:
        errors_found = True
        print("❌ BOOT_GO_FAIL: MCUboot did not select a bootable image")
        print("   └─ Cause: image validation failed or the primary slot is empty")
        print("   └─ Check: image header, TLV hash, flash mapping, and image size\n")

    if b"BOOT_GO_OK" in output:
        print("✓ BOOT_GO_OK: MCUboot selected a bootable image")
    
    if not errors_found:
        if b"boot" in output.lower() or len(output) > 0:
            print("✓ No validation errors detected")
            print("  Device may have:")
            print("  - Successfully booted application")
            print("  - Started bootloader normally (waiting for command)")
            print("  - Hit different failure in MCUboot (needs deeper investigation)\n")
        else:
            print("⚠️  No recognizable output pattern detected\n")
    
    print(f"Raw output ({len(output)} bytes):")
    print(f"  Hex: {output[:128].hex()}")
    print(f"  Text: {output.decode('utf-8', errors='replace')[:128]!r}")


def run_diagnostic(port_name, baud, verbose=False):
    """Run bootloader diagnostic (capture and analyze output)"""
    print(f"\nMCUboot Diagnostic Mode")
    print(f"Port: {port_name} @ {baud} baud")
    print(f"Timeout: {DIAGNOSTIC_TIMEOUT}s")
    print(f"\n⚠️  Make sure device is powered on or reset!")
    print(f"Waiting for bootloader messages...\n")
    
    with serial.Serial(port_name, baud, bytesize=8, parity="N", stopbits=1,
                       timeout=0.1, write_timeout=2.0, rtscts=False, dsrdtr=False,
                       xonxoff=False) as port:
        time.sleep(0.05)
        
        # Clear any pending data
        if port.in_waiting:
            pending = port.read(port.in_waiting)
            if verbose:
                trace("RX-DISCARD", pending)
        
        # Capture boot messages
        output = capture_boot_messages(port, timeout=DIAGNOSTIC_TIMEOUT)
        
        # Analyze results
        analyze_boot_output(output)


def run_boot_trigger(port_name, baud, verbose=False):
    """Send bootloader trigger command only"""
    print(f"\nBoot Trigger Mode - Sending bootloader trigger")
    print(f"Port: {port_name} @ {baud} baud\n")
    
    with serial.Serial(port_name, baud, bytesize=8, parity="N", stopbits=1,
                       timeout=2.0, write_timeout=2.0, rtscts=False, dsrdtr=False,
                       xonxoff=False) as port:
        time.sleep(0.05)
        
        # Clear any pending data
        if port.in_waiting:
            pending = port.read(port.in_waiting)
            if verbose:
                trace("RX-DISCARD", pending)
        
        # Send bootloader trigger command
        print("Sending bootloader trigger command (*B00TL)...")
        write_uart(port, b"*B00TL\n", verbose)
        port.flush()
        
        print("✓ Boot trigger sent")
        print(f"\nListening for device response (3s)...")
        
        # Capture response
        output = b""
        start_time = time.time()
        while time.time() - start_time < 3.0:
            if port.in_waiting:
                chunk = port.read(port.in_waiting)
                output += chunk
                try:
                    decoded = chunk.decode('utf-8', errors='replace')
                    print(decoded, end='', flush=True)
                except:
                    pass
            time.sleep(0.01)
        
        if output:
            print(f"\n✓ Device responded with {len(output)} bytes")
        else:
            print(f"⚠️  No response from device")



def run_upload(port_name, image_path, baud, verbose=False, diagnostic_timeout=None):
    """Upload firmware image to bootloader"""
    image, image_format = load_firmware_image(image_path)
    
    if not image:
        raise SystemExit("The firmware image is empty")
    if len(image) > 0x2D000:
        raise SystemExit("Image is larger than the 180 KiB MCUboot application slot")
    
    print(f"\nMCUboot Upload Mode")
    print(f"Port: {port_name} @ {baud} baud")
    print(f"Image: {image_path.name} ({len(image)} bytes)")
    print(f"Format: {image_format}")
    
    with serial.Serial(port_name, baud, bytesize=8, parity="N", stopbits=1,
                       timeout=5.0, write_timeout=5.0, rtscts=False, dsrdtr=False,
                       xonxoff=False) as port:
        time.sleep(0.05)
        
        # Clear any pending data
        pending = port.in_waiting
        if pending:
            if verbose:
                trace("RX-DISCARD", port.read(pending))
            else:
                port.read(pending)
        
        # Send bootloader trigger command
        print("Sending bootloader trigger command...")
        write_uart(port, b"*B00TL\n", verbose)
        port.flush()
        time.sleep(0.5)  # Brief delay for device to process
        
        # Periodically check if device is in bootloader mode (5s timeout)
        print("Checking if device is in bootloader mode (5s timeout)...")
        if not ping_bootloader_periodic(port, timeout=5.0, verbose=verbose):
            print(f"\n❌ Device is NOT in bootloader mode (timeout after 5s)")
            print(f"\nAborted: Cannot upload firmware to non-bootloader device")
            raise SystemExit(1)
        
        # Upload image
        print(f"\nUploading {len(image)} bytes...")
        send_upload(port, image, verbose)
        
        # Reset device, then keep the port open to capture boot diagnostics.
        reset_device(port, verbose)
        print("✓ Upload complete; device reset requested.\n")
        print("Listening for boot diagnostics; press Ctrl+C to stop.")
        output = capture_boot_messages(port, timeout=diagnostic_timeout)
        analyze_boot_output(output)


def main():
    parser = argparse.ArgumentParser(
        description="MCUboot Manager - Upload firmware and diagnose boot issues",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Upload firmware and capture boot output
  python mcuboot_manager.py firmware.bin --port COM19 --baud 38400

    # Upload a generated MCUboot Intel HEX file
    python mcuboot_manager.py firmware-signed.hex --port COM19 --baud 38400
  
  # Diagnose bootloader (capture messages without uploading)
  python mcuboot_manager.py --diagnose --port COM19 --baud 38400
  
  # Send bootloader trigger command only
  python mcuboot_manager.py --boot-trigger-only --port COM19
  
  # Test bootloader connection (ping only)
  python mcuboot_manager.py --ping-only --port COM19
  
  # Upload and capture diagnostics for 10 seconds
  python mcuboot_manager.py firmware.bin --port COM19 --diagnostic-timeout 10
        """
    )
    
    parser.add_argument("image", type=Path, nargs="?", help="MCUboot image (.bin or .hex), or application firmware (.hex)")
    parser.add_argument("--port", required=True, help="UART port (e.g. COM19)")
    parser.add_argument("--baud", type=int, default=38400, help="UART baud rate (default 38400)")
    parser.add_argument("--diagnose", action="store_true", 
                        help="Capture bootloader messages without uploading (diagnostic mode)")
    parser.add_argument("--ping-only", action="store_true", 
                        help="Only test bootloader communication")
    parser.add_argument("--boot-trigger-only", action="store_true",
                        help="Send bootloader trigger command only")
    parser.add_argument("--diagnostic-timeout", type=float, default=None,
                        help="Stop post-upload diagnostics after this many seconds (default: until Ctrl+C)")
    parser.add_argument("--verbose", action="store_true", 
                        help="Show detailed UART communication (SMP packets)")
    parser.add_argument("--startup-delay", type=float, default=0.05, 
                        help="Delay after opening UART (default 0.05s)")
    
    args = parser.parse_args()
    
    # Validate arguments
    if args.diagnose:
        # Diagnostic mode doesn't need image
        run_diagnostic(args.port, args.baud, args.verbose)
    elif args.boot_trigger_only:
        # Boot trigger mode doesn't need image
        run_boot_trigger(args.port, args.baud, args.verbose)
    elif args.ping_only:
        # Ping mode doesn't need image
        print(f"\nPing Mode - Testing MCUboot Communication")
        print(f"Port: {args.port} @ {args.baud} baud\n")
        with serial.Serial(args.port, args.baud, bytesize=8, parity="N", stopbits=1,
                           timeout=2.0, write_timeout=2.0, rtscts=False, dsrdtr=False,
                           xonxoff=False) as port:
            time.sleep(args.startup_delay)
            if port.in_waiting:
                port.read(port.in_waiting)
            ping_bootloader(port, args.verbose)
    else:
        # Upload mode requires image
        if args.image is None:
            raise SystemExit("Image path required (or use --diagnose, --boot-trigger-only, or --ping-only)")
        if not args.image.exists():
            raise SystemExit(f"Image file not found: {args.image}")
        
        run_upload(args.port, args.image, args.baud, args.verbose, args.diagnostic_timeout)


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError, ValueError) as error:
        print(f"Error: {error}", file=sys.stderr)
        raise SystemExit(1)
