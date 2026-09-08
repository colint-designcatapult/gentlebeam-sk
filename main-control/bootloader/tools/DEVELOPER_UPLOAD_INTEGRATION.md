# Firmware Uploader Integration Guide

This document defines the host-side behavior for software that uploads
firmware to the Gryphon Control board. A replacement uploader can use the
Apache Mynewt mcumgr library/CLI or implement the MCUboot serial-recovery SMP
protocol directly.

## Transport

Use the board FTDI serial port with these settings:

| Setting | Value |
| --- | --- |
| Baud rate | 115200 |
| Data bits | 8 |
| Parity | None |
| Stop bits | 1 |
| Flow control | None |
| mcumgr serial MTU | 124 |

The application and the current bootloader serial recovery service both use
115200 baud. Keep the port open only as long as required by the host serial
library, then close and reopen it as necessary when changing between the
application command and the mcumgr client.

## Firmware Package Requirements

Upload only a compact MCUboot image with the `.mcumgr.bin` extension. Do not
upload the raw application `.bin`, ELF, application-only HEX, or combined HEX.

The image must have all of the following:

- A 512-byte MCUboot image header.
- A payload linked for the primary image slot starting at `0x00432200`.
- An SHA-256 TLV.
- An ECDSA P-256 `KEYHASH` and `ECDSASIG` TLV generated with the private key
  matching the public key embedded in the bootloader.
- A total size no larger than 819200 bytes.

The project CMake build produces the expected artifact at
`out/cmake/<configuration>/main/Gryphon Control FW.mcumgr.bin`. Verify a file
before offering it for upload:

```powershell
python ..\mcuboot\mcuboot-2.4.0\scripts\imgtool.py dumpinfo ".\out\cmake\debug\main\Gryphon Control FW.mcumgr.bin"
```

The TLV area must contain `SHA256`, `KEYHASH`, and `ECDSASIG`. Images without
the ECDSA signature are rejected by the bootloader and are omitted from
`mcumgr image list`.

## Slots

| Slot | Flash range | Host uploader use |
| --- | --- | --- |
| 0 | `0x00432000` - `0x004F9FFF` | Current primary image; never write directly for updates |
| 1 | `0x004FA000` - `0x005C1FFF` | Upload destination |
| Scratch | `0x005C2000` - `0x005D1FFF` | Used internally by MCUboot during swaps |

The board has one updateable image. With direct serial upload enabled,
mcumgr target `--image 2` maps to image 0's secondary slot, slot 1. Target
`--image 0` or omitting the image selector targets primary storage and must
not be used by update software.

## Required Upload Workflow

1. Open the application FTDI port at 115200 baud and write the seven bytes
   `*BOOTL\n` (`0x2A 42 4F 4F 54 4C 0A`). Flush the serial write.
2. The application writes a one-shot serial-recovery request to a backup
   register and resets. MCUboot consumes the request and enters serial
   recovery. Start probing after the serial port is available.
3. Create the mcumgr serial connection using the same port, 115200 baud, and
   MTU 124.
4. Send `echo ping` until it succeeds or the uploader retry budget expires.
   A failed probe during startup is expected; use a short retry interval.
5. Upload the validated `.mcumgr.bin` using direct image target 2.
6. Request `image list`. Confirm that slot 1 is present, `bootable: true`, and
   has a 64-hex-character hash.
7. Send `image confirm <slot-1-hash>` to request a permanent upgrade.
8. Send `reset`.
9. Treat transport success as distinct from firmware acceptance. If slot 1 is
   absent or has no hash after an upload, do not call `image confirm`; issue
   `reset` so the board returns to its valid primary image, and report the
   update as failed.

Equivalent mcumgr CLI commands:

```powershell
mcumgr conn add main-control-ftdi type="serial" connstring="dev=COM12,baud=115200,mtu=124"
mcumgr -c main-control-ftdi echo ping
mcumgr -c main-control-ftdi image upload --image 2 ".\Gryphon Control FW.mcumgr.bin"
mcumgr -c main-control-ftdi image list
mcumgr -c main-control-ftdi image confirm <slot-1-hash>
mcumgr -c main-control-ftdi reset
```

## Completion and Failure Rules

An uploader reports success only after the upload, slot-1 validation,
confirmation, and reset commands succeed. It must report failure when the
file is malformed, signature validation fails, serial recovery cannot be
reached, an mcumgr request fails, or slot 1 is not listed with a hash.

After `reset`, MCUboot swaps slot 1 into slot 0 using scratch flash. The
mcumgr service normally stops when the application boots, so a post-reset
`image list` requires entering serial recovery again with `*BOOTL\n`.

## Signing-Key Management

MCUboot trusts only the ECDSA P-256 public key compiled into the bootloader.
The corresponding private signing key must remain outside source control and
outside the uploader application. The uploader needs only signed `.mcumgr.bin`
files; it must never receive or embed the private key.

Changing the signing key requires generating a new public-key C source,
rebuilding, and reflashing the bootloader before deploying images signed by
the replacement key. Existing images signed with the old key will no longer
be accepted.
