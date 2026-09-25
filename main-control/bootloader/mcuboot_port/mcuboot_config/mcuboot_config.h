#ifndef __MCUBOOT_CONFIG_H__
#define __MCUBOOT_CONFIG_H__

/*
 * Gryphon bootloader mcuboot configuration.
 *
 * ECDSA P-256 signature verification is enabled. The matching public key is
 * embedded from bootloader/keys/gryphon-dev-ec256-pub.c.
 */

/* Crypto backend: tinycrypt provides the SHA-256 implementation used to
 * verify the image hash TLV. */
#define MCUBOOT_USE_TINYCRYPT
#define MCUBOOT_SIGN_EC256

/* Single updateable image (one primary + one secondary slot). */
#define MCUBOOT_IMAGE_NUMBER 1

/* No platform-specific mcumgr command groups implemented. */
#define MCUBOOT_PERUSER_MGMT_GROUP_ENABLED 0

/* Enables the "echo" mcumgr command (mcumgr echo <text>) for connectivity
 * testing; without this boot_serial.c compiles it out and silently replies
 * MGMT_ERR_ENOTSUP instead of echoing back the text. */
#define MCUBOOT_BOOT_MGMT_ECHO

/* Report each valid image's SHA-256 hash and boot state through
 * `mcumgr image list`, so an uploaded secondary image can be selected for a
 * test swap. */
#define MCUBOOT_SERIAL_IMG_GRP_HASH
#define MCUBOOT_SERIAL_IMG_GRP_IMAGE_STATE

/* Keep direct upload enabled so the port's target mapping can enforce
 * secondary-only uploads via `mcumgr image upload --image 2`.
 * Primary targets (0/default and 1) are rejected by that mapping. */
#define MCUBOOT_SERIAL_DIRECT_IMAGE_UPLOAD

/* boot_serial.c defaults this to 512, which is too small for a typical
 * mcumgr image-upload chunk once base64+CBOR framing overhead is included;
 * an oversized frame is silently dropped (no response), which looks to the
 * host like a stalled/hanging upload rather than an error. */
#define MCUBOOT_SERIAL_MAX_RECEIVE_SIZE 4096

/* Swap-based upgrade with a scratch area: the secondary slot image is
 * swapped into the primary slot (and the displaced primary image is kept in
 * the secondary slot), so a failed/incomplete upgrade can be reverted. This
 * requires a scratch flash area (see sysflash.h / flash_map_backend.c). Not
 * defining MCUBOOT_OVERWRITE_ONLY (or any other upgrade mode) makes bootutil
 * default to MCUBOOT_SWAP_USING_SCRATCH. */

/* Always validate the primary slot's image hash before booting. */
#define MCUBOOT_VALIDATE_PRIMARY_SLOT

/* Use the newer flash_area_get_sectors() based API. */
#define MCUBOOT_USE_FLASH_AREA_GET_SECTORS

/* Maximum number of flash sectors per image slot (800 KiB / 8 KiB erase
 * blocks = 100; see FLASH_SECTOR_SIZE in flash_map_backend.c). */
#define MCUBOOT_MAX_IMG_SECTORS 100

/* Without this, bs_upload() erases the *entire* secondary slot (800 KiB, ~100
 * sector erases) up front on the very first upload chunk, which easily takes
 * long enough on SAME70's EFC to blow past mcumgr's per-request timeout
 * before the device can even ack the first chunk. Progressive erase only
 * erases the sectors needed for each chunk as it arrives. */
#define MCUBOOT_ERASE_PROGRESSIVELY

/* No fault-injection hardening. */
#define MCUBOOT_FIH_PROFILE_OFF

/* No watchdog or idle integration yet. */
#define MCUBOOT_WATCHDOG_FEED() \
	do {                     \
	} while (0)

#define MCUBOOT_CPU_IDLE() \
	do {                \
	} while (0)

#endif /* __MCUBOOT_CONFIG_H__ */
