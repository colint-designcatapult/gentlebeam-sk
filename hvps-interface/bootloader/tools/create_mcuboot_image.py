#!/usr/bin/env python3
"""
Create MCUboot-compatible images from application firmware.
Scans ../../main/build for different builds and creates signed/unsigned images.

Usage:
    python create_mcuboot_image.py [--build-name CalibrationDebug] [--key-file key.pem]
    python create_mcuboot_image.py --single <input.bin> [--output output.bin]
"""

import argparse
import shutil
import struct
import sys
import subprocess
import hashlib
import tempfile
from pathlib import Path
# MCUboot image header magic number
IMAGE_MAGIC = 0x96f3b83d
IMAGE_TLV_INFO_MAGIC = 0x6907
IMAGE_TLV_SHA256 = 0x0010
IMAGE_HEADER_SIZE = 0x200
IMAGE_SLOT_ADDRESS = 0x0800C000

class ImageHeader:
    def __init__(self, app_size, version=0x01000000):
        self.magic = IMAGE_MAGIC
        self.load_addr = 0
        self.hdr_size = IMAGE_HEADER_SIZE
        self.protect_tlv_size = 0
        self.img_size = app_size
        self.flags = 0
        self.version = version

    def to_bytes(self):
        return struct.pack(
            "<IIHHII4BI",
            self.magic,
            self.load_addr,
            self.hdr_size,
            self.protect_tlv_size,
            self.img_size,
            self.flags,
            (self.version >> 24) & 0xff,
            (self.version >> 16) & 0xff,
            (self.version >> 8) & 0xff,
            self.version & 0xff,
            0,
        )


class TLVRecord:
    # TLV Types
    SHA256 = 0x10
    EC256 = 0x22
    RSA2048_PSS = 0x23
    ED25519 = 0x24
    ECDSA_P256 = 0x25
    ECDSA_P384 = 0x26
    ECDSA_P521 = 0x27
    END = 0x00

    def __init__(self, tlv_type, data=b""):
        self.tlv_type = tlv_type
        self.data = data
        self.length = len(data)

    def to_bytes(self):
        return struct.pack(
            "<HH", self.tlv_type, self.length
        ) + self.data


def create_mcuboot_image(app_binary, version_tuple=(1, 0, 0)):
    """
    Create MCUboot image with header and TLV records.
    
    Args:
        app_binary: Raw application binary data
        version_tuple: (major, minor, revision) version tuple
    
    Returns:
        Complete MCUboot image with header, app, and TLV
    """
    
    if len(app_binary) >= 4 and struct.unpack_from("<I", app_binary)[0] == IMAGE_MAGIC:
        raise ValueError("Input already contains an MCUboot header; use it directly")

    # Convert version tuple to 32-bit version field (major.minor.revision.build)
    major, minor, revision = version_tuple
    version_field = (major << 24) | (minor << 16) | (revision << 8) | 0
    
    # Create header
    header = ImageHeader(len(app_binary), version_field)
    header_bytes = header.to_bytes()
    
    # Pad the header to the application image's 512-byte boundary.
    padded_header = header_bytes + b'\xff' * (IMAGE_HEADER_SIZE - len(header_bytes))
    
    # MCUboot hashes the complete header and image body.
    image_sha256 = hashlib.sha256(padded_header + app_binary).digest()
    
    # Create TLV records
    tlv_records = [
        TLVRecord(IMAGE_TLV_SHA256, image_sha256),
    ]
    
    tlv_record = tlv_records[0].to_bytes()
    tlv_data = struct.pack("<HH", IMAGE_TLV_INFO_MAGIC, 4 + len(tlv_record)) + tlv_record
    
    # Pad TLV to 4-byte alignment
    tlv_padding = (4 - (len(tlv_data) % 4)) % 4
    tlv_data += b'\xff' * tlv_padding
    
    # Combine: header + app + TLV
    mcuboot_image = padded_header + app_binary + tlv_data
    
    return mcuboot_image


def hex_to_binary(hex_file_path):
    """
    Convert Intel HEX file to binary.
    
    Args:
        hex_file_path: Path to .hex file
    
    Returns:
        Binary data as bytes
    """
    hex_file = Path(hex_file_path)
    if not hex_file.exists():
        raise FileNotFoundError(f"Hex file not found: {hex_file}")
    
    data_by_address = read_intel_hex(hex_file)
    min_address = min(data_by_address)
    max_address = max(data_by_address) + 1
    
    # Build binary from segments
    # Build relative to the first data address; HEX records use absolute flash addresses.
    binary = bytearray([0xFF] * (max_address - min_address))
    for address, value in data_by_address.items():
        binary[address - min_address] = value
    
    return bytes(binary)


def read_intel_hex(hex_file_path):
    """Read Intel HEX data into an absolute address-to-byte mapping."""
    data_by_address = {}
    extended_address = 0

    with Path(hex_file_path).open("r", encoding="ascii") as hex_file:
        for line_num, line in enumerate(hex_file, 1):
            line = line.strip()
            if not line:
                continue
            if not line.startswith(":"):
                raise ValueError(f"Invalid HEX file at line {line_num}: missing ':'")

            try:
                record = bytes.fromhex(line[1:])
            except ValueError as error:
                raise ValueError(f"Invalid HEX file at line {line_num}: {error}") from error
            if len(record) < 5 or len(record) != record[0] + 5 or sum(record) & 0xFF:
                raise ValueError(f"Invalid HEX file at line {line_num}: invalid record or checksum")

            byte_count, address_high, address_low, record_type = record[:4]
            address = (address_high << 8) | address_low
            data = record[4:4 + byte_count]
            if record_type == 0x00:
                for offset, value in enumerate(data):
                    data_by_address[extended_address + address + offset] = value
            elif record_type == 0x01:
                break
            elif record_type == 0x04:
                if byte_count != 2:
                    raise ValueError(f"Invalid HEX file at line {line_num}: invalid extended address")
                extended_address = int.from_bytes(data, "big") << 16

    if not data_by_address:
        raise ValueError("No data found in HEX file")
    return data_by_address


def write_intel_hex(data_by_address, output_path):
    """Write an absolute address-to-byte mapping as an Intel HEX file."""
    lines = []
    addresses = sorted(data_by_address)
    current_upper_address = None
    index = 0

    while index < len(addresses):
        absolute_address = addresses[index]
        upper_address = absolute_address >> 16
        if upper_address != current_upper_address:
            record = bytes([2, 0, 0, 4, upper_address >> 8, upper_address & 0xFF])
            lines.append(":" + record.hex().upper() + f"{(-sum(record)) & 0xFF:02X}")
            current_upper_address = upper_address

        chunk = bytearray()
        start_address = absolute_address
        while index < len(addresses) and len(chunk) < 16:
            next_address = addresses[index]
            if next_address != start_address + len(chunk) or next_address >> 16 != upper_address:
                break
            chunk.append(data_by_address[next_address])
            index += 1

        address = start_address & 0xFFFF
        record = bytes([len(chunk), address >> 8, address & 0xFF, 0]) + chunk
        lines.append(":" + record.hex().upper() + f"{(-sum(record)) & 0xFF:02X}")

    lines.append(":00000001FF")
    Path(output_path).write_text("\n".join(lines) + "\n", encoding="ascii")


def binary_to_hex(binary_data, output_path, start_address=IMAGE_SLOT_ADDRESS):
    """Write binary data as an Intel HEX file at the specified flash address."""
    data_by_address = {
        start_address + offset: value for offset, value in enumerate(binary_data)
    }
    write_intel_hex(data_by_address, output_path)


def create_combined_hex(bootloader_hex, application_hex, output_path):
    """Merge non-overlapping bootloader and MCUboot application HEX files."""
    bootloader_data = read_intel_hex(bootloader_hex)
    application_data = read_intel_hex(application_hex)
    overlaps = set(bootloader_data).intersection(application_data)
    if overlaps:
        first_overlap = min(overlaps)
        raise ValueError(f"Bootloader and application overlap at 0x{first_overlap:08X}")

    bootloader_data.update(application_data)
    write_intel_hex(bootloader_data, output_path)


def sign_image_with_imgtool(app_binary, key_file, output_path, version="1.0.0"):
    """
    Sign an MCUboot image using imgtool (MCUboot utility).
    
    Args:
        app_binary: Raw application binary data
        key_file: Path to signing key (PEM format)
        output_path: Output path for signed image
        version: Version string
    
    Returns:
        True if successful, False otherwise
    """
    temp_path = None
    try:
        with tempfile.NamedTemporaryFile(suffix=".bin", delete=False) as temp_file:
            temp_file.write(app_binary)
            temp_path = Path(temp_file.name)

        # Use the current interpreter so imgtool resolves from its installed environment.
        cmd = [
            sys.executable, "-m", "imgtool.main", "sign",
            "-k", str(key_file),
            "-S", "0x2D000",  # Slot size (180 KiB)
            "-H", "0x200",    # Header size
            "--pad-header",
            "--align", "4",
            "-v", version,
            str(temp_path),
            str(output_path)
        ]
        
        result = subprocess.run(cmd, capture_output=True, text=True, timeout=10)
        if result.returncode == 0:
            return True
        else:
            print(f"⚠️  imgtool signing failed: {result.stderr}")
            return False
    except FileNotFoundError:
        print("⚠️  imgtool not found. Install with: pip install mcuboot")
        return False
    except Exception as e:
        print(f"⚠️  Signing error: {e}")
        return False
    finally:
        if temp_path is not None:
            temp_path.unlink(missing_ok=True)


def process_build(build_path, output_root, bootloader_hex=None, key_file=None, version="1.0.0"):
    """
    Process a single build directory.
    Converts HEX to binary and creates signed/unsigned MCUboot images.
    
    Args:
        build_path: Path to build directory
        output_root: Root directory for generated MCUboot images
        bootloader_hex: Optional bootloader HEX for factory-image generation
        key_file: Optional signing key (PEM format)
        version: Version string
    
    Returns:
        List of created files
    """
    hex_file = build_path / "gryphon-hvps-interface-fw.hex"
    if not hex_file.exists():
        print(f"⚠️  Hex file not found in {build_path}: {hex_file.name}")
        return []
    
    print(f"\n{'='*70}")
    print(f"Processing: {build_path.name}")
    print(f"{'='*70}")
    
    try:
        # Convert HEX to binary
        print(f"  Converting HEX to binary...")
        app_binary = hex_to_binary(hex_file)
        print(f"  ✓ Binary size: {len(app_binary)} bytes")
        
        # Parse version
        try:
            version_parts = version.split(".")
            version_tuple = tuple(int(p) for p in version_parts[:3])
            if len(version_tuple) < 3:
                version_tuple += (0,) * (3 - len(version_tuple))
        except ValueError:
            raise SystemExit(f"Invalid version format: {version}")
        
        # Create unsigned MCUboot image
        print(f"  Creating unsigned MCUboot image...")
        mcuboot_image = create_mcuboot_image(app_binary, version_tuple)
        
        # Save output under tools/mcuboot-build/<build-name>.
        output_dir = output_root / build_path.name
        output_dir.mkdir(parents=True, exist_ok=True)
        unsigned_path = output_dir / "firmware-unsigned.bin"
        unsigned_path.write_bytes(mcuboot_image)
        print(f"  ✓ Unsigned image: {unsigned_path}")
        unsigned_hex_path = output_dir / "firmware-unsigned.hex"
        binary_to_hex(mcuboot_image, unsigned_hex_path)
        print(f"  ✓ Unsigned HEX: {unsigned_hex_path}")
        
        created_files = [unsigned_path, unsigned_hex_path]
        
        # Create signed image if key available
        if key_file and Path(key_file).exists():
            print(f"  Creating signed MCUboot image...")
            signed_path = output_dir / "firmware-signed.bin"
            if sign_image_with_imgtool(app_binary, key_file, signed_path, version):
                print(f"  ✓ Signed image: {signed_path}")
                created_files.append(signed_path)
                signed_hex_path = output_dir / "firmware-signed.hex"
                binary_to_hex(signed_path.read_bytes(), signed_hex_path)
                print(f"  ✓ Signed HEX: {signed_hex_path}")
                created_files.append(signed_hex_path)
                if bootloader_hex and bootloader_hex.exists():
                    combined_hex_path = output_dir / "firmware-combined.hex"
                    create_combined_hex(bootloader_hex, signed_hex_path, combined_hex_path)
                    print(f"  ✓ Combined HEX: {combined_hex_path}")
                    created_files.append(combined_hex_path)
                elif bootloader_hex:
                    print(f"  ⚠️  Bootloader HEX not found, skipping combined image: {bootloader_hex}")
            else:
                print(f"  ⚠️  Signing failed, using unsigned image only")
        else:
            print(f"  ℹ️  No key file provided (--key-file), skipping signed image")
        
        print(f"\n  Summary:")
        print(f"    Input:   {hex_file.name} ({len(app_binary)} bytes)")
        print(f"    Output:  {output_dir}/firmware-[unsigned|signed|combined].[bin|hex]")
        print(f"    Version: {version}")
        
        return created_files
        
    except Exception as e:
        print(f"  ❌ Error: {e}")
        return []


def main():
    parser = argparse.ArgumentParser(
        description="Create MCUboot images from application firmware",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    # Process all builds in ../../main/build
  python create_mcuboot_image.py
  
  # Process specific build
  python create_mcuboot_image.py --build-name CalibrationDebug
  
  # Process with signing
  python create_mcuboot_image.py --key-file ../keys/sign_key.pem

    # Use the Release bootloader HEX for the combined factory image
    python create_mcuboot_image.py --key-file ../keys/sign_key.pem --bootloader-build Release
  
  # Legacy: Process single binary file
  python create_mcuboot_image.py --single firmware.bin --output firmware-mcuboot.bin
        """
    )
    
    parser.add_argument(
        "--build-name", default=None,
        help="Process specific build (e.g., CalibrationDebug). If omitted, process all builds"
    )
    parser.add_argument(
        "--key-file", default=None,
        help="Signing key (PEM format). If provided, creates signed images"
    )
    parser.add_argument(
        "--version", default="1.0.0",
        help="Image version (major.minor.revision)"
    )
    parser.add_argument(
        "--bootloader-build", default="Debug",
        help="Bootloader build used for firmware-combined.hex (default: Debug)"
    )
    parser.add_argument(
        "--single", type=Path, default=None,
        help="[Legacy] Process single binary file instead of scanning builds"
    )
    parser.add_argument(
        "--output", type=Path, default=None,
        help="[Legacy] Output path for single binary mode"
    )
    parser.add_argument(
        "--clean", action="store_true",
        help="Delete the mcuboot-build output directory and exit"
    )
    
    args = parser.parse_args()
    
    if args.clean:
        output_root = Path(__file__).parent / "mcuboot-build"
        if output_root.exists():
            shutil.rmtree(output_root)
            print(f"Removed {output_root}")
        else:
            print(f"Nothing to clean: {output_root} does not exist")
        return
    
    # Legacy single-file mode
    if args.single:
        if not args.single.exists():
            raise SystemExit(f"Input file not found: {args.single}")
        
        app_binary = args.single.read_bytes()
        if not app_binary:
            raise SystemExit("Input binary is empty")
        
        # Parse version
        try:
            version_parts = args.version.split(".")
            version_tuple = tuple(int(p) for p in version_parts[:3])
            if len(version_tuple) < 3:
                version_tuple += (0,) * (3 - len(version_tuple))
        except ValueError:
            raise SystemExit(f"Invalid version format: {args.version}")
        
        # Create MCUboot image
        mcuboot_image = create_mcuboot_image(app_binary, version_tuple)
        
        # Determine output path
        if args.output is None:
            output_path = args.single.with_name(f"{args.single.stem}_mcuboot.bin")
        else:
            output_path = args.output
        
        # Write output
        output_path.write_bytes(mcuboot_image)
        
        print(f"Created MCUboot image:")
        print(f"  Input:  {args.single} ({len(app_binary)} bytes)")
        print(f"  Output: {output_path} ({len(mcuboot_image)} bytes)")
        print(f"  Version: {args.version}")
        return
    
    # Batch mode: scan ../../main/build
    print("MCUboot Image Creator - Batch Build Processor")
    print("=" * 70)
    
    # Resolve build directory relative to this script
    script_dir = Path(__file__).parent
    main_build_dir = script_dir.parent.parent / "main" / "build"
    output_root = script_dir / "mcuboot-build"
    bootloader_hex = script_dir.parent / "build" / args.bootloader_build / "hvps-interface-bootloader.hex"
    
    if not main_build_dir.exists():
        raise SystemExit(f"Build directory not found: {main_build_dir}")
    
    print(f"Scanning: {main_build_dir}")
    print(f"Output:   {output_root}\n")
    
    # Find builds
    if args.build_name:
        # Process specific build
        build_path = main_build_dir / args.build_name
        if not build_path.is_dir():
            raise SystemExit(f"Build not found: {build_path}")
        builds = [build_path]
    else:
        # Process all builds (subdirectories)
        builds = [d for d in main_build_dir.iterdir() if d.is_dir()]
        builds.sort()
    
    if not builds:
        raise SystemExit(f"No builds found in {main_build_dir}")
    
    # Process each build
    all_created = []
    for build_path in builds:
        created = process_build(build_path, output_root, bootloader_hex, args.key_file, args.version)
        all_created.extend(created)
    
    # Summary
    print(f"\n{'='*70}")
    print(f"SUMMARY")
    print(f"{'='*70}")
    print(f"Processed: {len(builds)} build(s)")
    print(f"Created:   {len(all_created)} file(s)")
    for f in all_created:
        print(f"  • {f}")
    print()


if __name__ == "__main__":
    try:
        main()
    except (OSError, SystemExit, ValueError) as e:
        if isinstance(e, SystemExit):
            raise
        print(f"Error: {e}", file=sys.stderr)
        raise SystemExit(1)
