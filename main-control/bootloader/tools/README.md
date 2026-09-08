# Gryphon System Bootloader

For developers implementing a separate firmware upload application, see
[DEVELOPER_UPLOAD_INTEGRATION.md](DEVELOPER_UPLOAD_INTEGRATION.md).

## mcumgr commands

### Configure the connection profile (once)

```powershell
mcumgr conn add main-control-ftdi type="serial" connstring="dev=COM12,baud=115200,mtu=124"
```

Replace `COM12` with the FTDI adapter's actual port. Check existing profiles:

```powershell
mcumgr conn show
```

### Upload with the helper script

```powershell
python .\upload_firmware.py --port COM12 "..\..\out\cmake\debug\main\Gryphon Control FW.mcumgr.bin"
```

The script runs `imgtool dumpinfo`, configures the connection profile, uploads
the image to slot 1, then sends `mcumgr image confirm` for it before resetting.
The resulting swap is permanent. Its `image list` output shows the firmware in
both slots. Use `--skip-trigger` when the board is already in serial-recovery
mode.

To only send the application bootloader command and enter serial recovery:

```powershell
python .\upload_firmware.py --port COM12 --bootloader-only
```

### Image slots

| Slot | Internal-flash address range | Purpose |
| --- | --- | --- |
| 0 | `0x00432000` - `0x004F9FFF` | Primary: image booted normally |
| 1 | `0x004FA000` - `0x005C1FFF` | Secondary: uploaded candidate image |

`image confirm <hash>` marks the slot 1 image for a permanent upgrade. On
reset, mcuboot swaps slots 0 and 1 using scratch flash.

### Echo (connectivity check)

```powershell
mcumgr -c main-control-ftdi echo hello
```

### List images in both slots

```powershell
mcumgr -c main-control-ftdi image list
```

### Upload a new image to the secondary slot

```powershell
mcumgr -c main-control-ftdi image upload --image 2 "..\..\out\cmake\debug\main\Gryphon Control FW.mcumgr.bin"
```

### Mark the uploaded image for a permanent upgrade

```powershell
mcumgr -c main-control-ftdi image confirm <hash-from-image-list>
```

### Reset manually after confirming an image

```powershell
mcumgr -c main-control-ftdi reset
```

## Image signing

MCUboot verifies ECDSA P-256 signatures before it accepts or boots an image.
The CMake build signs `Gryphon Control FW.mcumgr.bin` with the private key at
`bootloader\keys\gryphon-dev-ec256.pem`; its matching public key is compiled
into the bootloader from `bootloader\keys\gryphon-dev-ec256-pub.c`.

Inspect the image before upload:

```powershell
python ..\..\..\mcuboot\mcuboot-2.4.0\scripts\imgtool.py dumpinfo "..\..\out\cmake\debug\main\Gryphon Control FW.mcumgr.bin"
```

The TLV area must include `SHA256`, `KEYHASH`, and `ECDSASIG` entries. Verify
the signature against the private key with:

```powershell
python ..\..\..\mcuboot\mcuboot-2.4.0\scripts\imgtool.py verify -k "..\keys\gryphon-dev-ec256.pem" "..\..\out\cmake\debug\main\Gryphon Control FW.mcumgr.bin"
```

The private PEM must not be committed or distributed. Changing the signing key
requires rebuilding and reflashing the bootloader with the matching generated
public-key source before uploading images signed by the new key.

## Notes

- If `echo`/`upload` stop responding after a code change, check
  `boot_uart_write()` in
  [mcuboot_port/boot_uart.c](mcuboot_port/boot_uart.c) first - it must retry
  `io_write()` until accepted, since `usart_async_write()` silently rejects a
  write if a previous transmission hasn't finished.
