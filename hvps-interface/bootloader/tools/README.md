# Bootloader Tools Manual

This directory contains the Python utilities used to create MCUboot images,
communicate with the bootloader, and upload firmware through UART3.

## Prerequisites

Clone the repository with its pinned MCUboot submodule:

```powershell
git clone --recurse-submodules https://github.com/DesignCatapult/gentlebeam-sk.git
```

For an existing clone, initialize the recorded MCUboot revision before
building:

```powershell
git submodule update --init --recursive
```

Use Python 3 and install the required packages from this directory:

```powershell
python -m pip install -r requirements.txt
```

Connect the target board to the PC UART used by the bootloader. The default
communication settings are 38400 baud, 8 data bits, no parity, and 1 stop bit.

## Signing Keys

This bootloader uses **ECDSA with the NIST P-256 curve** (`MCUBOOT_SIGN_EC256`)
to authenticate firmware. `imgtool` signs each image with a private PEM key;
the bootloader verifies it against the matching public key compiled into the
bootloader.

Create a signing-key pair once, from the repository root:

```powershell
python -m imgtool keygen -k keys\signing-key.pem -t ecdsa-p256
```

Keep `keys\signing-key.pem` private. Do not commit, distribute, or place it
on a production device. Store it in an access-controlled secret store and use
it only on trusted release-signing systems.

The public key belongs in `mcuboot\src\signing_key.c`, which is compiled into
the bootloader. Regenerate that source file whenever the signing key changes:

```powershell
python -m imgtool getpub -k keys\signing-key.pem > mcuboot\src\signing_key.c
```

Rebuild and flash the bootloader after changing `signing_key.c`, then sign
firmware using the same private key. An image signed by any other key will be
rejected during MCUboot validation.

## Create MCUboot Images

`create_mcuboot_image.py` finds application firmware at:

```text
../../main/build/<BuildName>/gryphon-hvps-interface-fw.hex
```

It converts each available Intel HEX file to a raw application image, creates
an unsigned MCUboot image, and optionally creates a signed MCUboot image.

Run from this `tools` directory:

```powershell
python create_mcuboot_image.py --key-file ..\keys\signing-key.pem
```

Generated files are kept in this directory so application build folders remain
unchanged:

```text
tools/mcuboot-build/<BuildName>/firmware-unsigned.bin
tools/mcuboot-build/<BuildName>/firmware-signed.bin
```

The signed file is created only when `--key-file` points to a valid private key.
Use the signed image for normal firmware uploads. Keep private key files out of
source control and do not distribute them with release artifacts.

### Direct imgtool Signing

To sign an application without `create_mcuboot_image.py`, run the following
from the `tools` directory. The output is a binary MCUboot image suitable for
`upload_firmware.py` and direct `mcumgr` uploads:

```powershell
python -m imgtool sign `
	-k ..\keys\signing-key.pem `
	-S 0x2D000 `
	-H 0x200 `
	--pad-header `
	--align 4 `
	-v 1.0.0 `
	..\..\main\build\Debug\gryphon-hvps-interface-fw.hex `
	mcuboot-build\Debug\firmware-signed.bin
```

`-S 0x2D000` sets the 180 KiB primary-slot size; `-H 0x200` reserves the
512-byte MCUboot header. Change `Debug` and `-v 1.0.0` as appropriate for the
build configuration and firmware version.

To produce an Intel HEX artifact instead, replace the final output path with:

```text
mcuboot-build\Debug\firmware-signed.hex
```

### Image Generator Options

```powershell
# Generate images for every build that has a HEX artifact.
python create_mcuboot_image.py --key-file ..\keys\signing-key.pem

# Generate one configuration only.
python create_mcuboot_image.py --build-name Debug --key-file ..\keys\signing-key.pem

# Set the MCUboot image version.
python create_mcuboot_image.py --version 1.2.0 --key-file ..\keys\signing-key.pem

# Legacy mode: wrap one raw binary as an unsigned MCUboot image.
python create_mcuboot_image.py --single firmware.bin --output firmware-mcuboot.bin

# Delete the tools/mcuboot-build output directory.
python create_mcuboot_image.py --clean
```

Build configurations without `gryphon-hvps-interface-fw.hex` are skipped. Build
the application configuration first, then rerun the generator.

## Upload Firmware

`upload_firmware.py` uses the `mcumgr` CLI to upload a signed MCUboot image.
It sends the `*B00TL` boot trigger to the running application, waits for the
bootloader's `mcumgr echo` response, lists the existing image, prints the
upload image's SHA-256, uploads to image `0`, lists the image again, and
requests a reset. The bootloader uses standard MCUboot/mcumgr NLIP framing
(base64 + CRC16-XMODEM).

Install both the Python dependencies and the `mcumgr` executable:

```powershell
python -m pip install -r requirements.txt
go install github.com/apache/mynewt-mcumgr-cli/mcumgr@latest
```

Upload a generated signed image:

```powershell
python upload_firmware.py --port COM19 mcuboot-build\Debug\firmware-signed.bin
```

Use the actual Windows serial port assigned to the board in place of `COM19`.
The script accepts `.bin` and `.hex` artifacts, but rejects images that do not
contain an MCUboot header and SHA-256 TLV before any upload is attempted.

The default UART configuration is 38400 baud and `mtu=128`. Override these
only when the target bootloader has been built with different values:

```powershell
python upload_firmware.py --port COM19 --baud 38400 --mtu 128 mcuboot-build\Debug\firmware-signed.bin
```

### Recovery-Mode Options

```powershell
# Send *B00TL and exit. Useful for manually testing recovery mode.
python upload_firmware.py --port COM19 --bootloader-only

# The board is already waiting in MCUboot serial recovery; do not send *B00TL.
python upload_firmware.py --port COM19 --skip-trigger mcuboot-build\Debug\firmware-signed.bin

# Increase the recovery-mode detection period when needed.
python upload_firmware.py --port COM19 --retries 20 --retry-delay 0.5 mcuboot-build\Debug\firmware-signed.bin
```

The bootloader has a single application slot. The uploader targets image `0`;
it does not use secondary-slot confirmation or swap commands.

## mcumgr CLI Compatibility

The bootloader speaks standard NLIP framing (base64 + CRC16-XMODEM line
markers), the same protocol used by the official
[`mcumgr` CLI](https://github.com/apache/mynewt-mcumgr-cli). Install it with:

```powershell
go install github.com/apache/mynewt-mcumgr-cli/mcumgr@latest
```

Then, right after resetting the board (within the serial-recovery window from
`boot_serial_check_start()` in `Src/main.c`):

```powershell
$conn = "dev=COM19,baud=38400,mtu=128"

# List images in the active single-slot layout.
mcumgr --conntype serial --connstring $conn image list

# Echo test (protocol sanity check without touching flash).
mcumgr --conntype serial --connstring $conn echo "hello mcumgr"

# Upload firmware to image 0.
mcumgr --conntype serial --connstring $conn image upload --image 0 mcuboot-build\Debug\firmware-signed.bin

# Reset the device.
mcumgr --conntype serial --connstring $conn reset
```

If an upload stalls on the first chunk, try a smaller `mtu=` value (e.g. `64`
or `96`) — `MCUBOOT_BOOT_MGMT_MCUMGR_PARAMS` isn't enabled, so `mcumgr` can't
auto-negotiate the MTU against this board's `BOOT_SERIAL_FRAME_MTU` (124).

## Normal Workflow

```powershell
# 1. Build the desired application configuration in ../../main/build.
# 2. Create unsigned and signed MCUboot images.
python create_mcuboot_image.py --build-name Debug --key-file ..\keys\signing-key.pem

# 3. Upload the signed image.
python upload_firmware.py --port COM19 mcuboot-build\Debug\firmware-signed.bin
```