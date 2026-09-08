#include <string.h>

#include "flash_map_backend/flash_map_backend.h"
#include "sysflash/sysflash.h"

#include <driver_init.h>
#include <hal_flash.h>
#include <sam.h>

/*
 * Flash layout (offsets relative to the internal flash base, 0x00400000;
 * hal_flash addresses are already relative to that base):
 *
 *   0x00000000 - 0x00032000 : bootloader          (200 KiB)
 *   0x00032000 - 0x000FA000 : primary image slot  (800 KiB)
 *   0x000FA000 - 0x001C2000 : secondary image slot(800 KiB)
 *   0x001C2000 - 0x001D2000 : scratch area          (64 KiB)
 *   0x001D2000 - 0x00200000 : reserved             (184 KiB)
 */
#define GB_BOOT_SIZE        0x00032000u
#define GB_SLOT_SIZE        0x000C8000u
#define GB_SCRATCH_SIZE     0x00010000u
#define GB_PRIMARY_OFF      GB_BOOT_SIZE
#define GB_SECONDARY_OFF    (GB_PRIMARY_OFF + GB_SLOT_SIZE)
#define GB_SCRATCH_OFF      (GB_SECONDARY_OFF + GB_SLOT_SIZE)

/* Largest flash page size we support read-modify-write buffering for. */
#define FLASH_PAGE_BUF_SIZE 512u

/* Erase-block granularity used for mcuboot's sector bookkeeping. Must match
 * (or be a multiple of) the hardware erase block outside the small-sector
 * region (see hpl_efc.c's _efc_in_8k_sect handling) - reporting the raw 512 B
 * write-page size here would inflate MCUBOOT_MAX_IMG_SECTORS 16x and blow the
 * stack-allocated struct boot_sector_buffer in mcuboot's loader.c. */
#define FLASH_SECTOR_SIZE 8192u

static const struct flash_area gb_flash_areas[] = {
	{
		.fa_id = PRIMARY_ID,
		.fa_device_id = 0,
		.fa_off = GB_PRIMARY_OFF,
		.fa_size = GB_SLOT_SIZE,
	},
	{
		.fa_id = SECONDARY_ID,
		.fa_device_id = 0,
		.fa_off = GB_SECONDARY_OFF,
		.fa_size = GB_SLOT_SIZE,
	},
	{
		.fa_id = SCRATCH_ID,
		.fa_device_id = 0,
		.fa_off = GB_SCRATCH_OFF,
		.fa_size = GB_SCRATCH_SIZE,
	},
};

static const struct flash_area *gb_flash_area_lookup(uint8_t id)
{
	for (size_t i = 0; i < (sizeof(gb_flash_areas) / sizeof(gb_flash_areas[0])); i++) {
		if (gb_flash_areas[i].fa_id == id) {
			return &gb_flash_areas[i];
		}
	}
	return NULL;
}

void flash_map_backend_init(void)
{
	const uint32_t region_size = EFC_PAGES_PR_REGION * FLASH_PAGE_BUF_SIZE;
	/* Our partition offsets are only 8 KiB (FLASH_SECTOR_SIZE) aligned, but
	 * _efc_flash_lock() requires exact 16 KiB (region_size) alignment or it
	 * silently no-ops with ERR_INVALID_ARG - round outward so every region
	 * touching our flash areas actually gets unlocked. */
	const uint32_t start = GB_PRIMARY_OFF & ~(region_size - 1);
	const uint32_t end = (GB_SCRATCH_OFF + GB_SCRATCH_SIZE + region_size - 1) & ~(region_size - 1);

	for (uint32_t addr = start; addr < end; addr += region_size) {
		flash_unlock(&FLASH_0, addr, EFC_PAGES_PR_REGION);
	}
}

int flash_area_open(uint8_t id, const struct flash_area **fapp)
{
	const struct flash_area *fa = gb_flash_area_lookup(id);
	if (fa == NULL) {
		return -1;
	}
	*fapp = fa;
	return 0;
}

void flash_area_close(const struct flash_area *fap)
{
	(void)fap;
}

int flash_area_read(const struct flash_area *fap, uint32_t off, void *dst, uint32_t len)
{
	if (fap == NULL || (off + len) > fap->fa_size) {
		return -1;
	}
	return flash_read(&FLASH_0, fap->fa_off + off, (uint8_t *)dst, len) == 0 ? 0 : -1;
}

int flash_area_write(const struct flash_area *fap, uint32_t off, const void *src, uint32_t len)
{
	uint32_t page_size;
	uint32_t abs_off;
	uint32_t page_addr;
	const uint8_t *src8 = (const uint8_t *)src;

	if (fap == NULL || (off + len) > fap->fa_size) {
		return -1;
	}

	page_size = flash_get_page_size(&FLASH_0);
	if (page_size == 0 || page_size > FLASH_PAGE_BUF_SIZE) {
		return -1;
	}

	abs_off = fap->fa_off + off;

	/* The SAME70 EFC driver only accepts whole, page-aligned writes, but
	 * mcuboot's swap-status/trailer writes are frequently smaller than one
	 * page. Read-modify-write each touched page so any offset/length works. */
	page_addr = abs_off - (abs_off % page_size);
	while (page_addr < abs_off + len) {
		uint8_t page_buf[FLASH_PAGE_BUF_SIZE];
		uint32_t page_off_in_src_start = (page_addr > abs_off) ? (page_addr - abs_off) : 0;
		uint32_t copy_start = (abs_off > page_addr) ? (abs_off - page_addr) : 0;
		uint32_t copy_end = ((abs_off + len) < (page_addr + page_size))
		                            ? (abs_off + len - page_addr)
		                            : page_size;

		if (flash_read(&FLASH_0, page_addr, page_buf, page_size) != 0) {
			return -1;
		}
		memcpy(&page_buf[copy_start], &src8[page_off_in_src_start], copy_end - copy_start);
		if (flash_write(&FLASH_0, page_addr, page_buf, page_size) != 0) {
			return -1;
		}

		page_addr += page_size;
	}

	return 0;
}

int flash_area_erase(const struct flash_area *fap, uint32_t off, uint32_t len)
{
	uint32_t page_size;

	if (fap == NULL || (off + len) > fap->fa_size) {
		return -1;
	}

	page_size = flash_get_page_size(&FLASH_0);
	if (page_size == 0 || (off % page_size) != 0 || (len % page_size) != 0) {
		return -1;
	}

	return flash_erase(&FLASH_0, fap->fa_off + off, len / page_size) == 0 ? 0 : -1;
}

uint32_t flash_area_align(const struct flash_area *fap)
{
	(void)fap;
	/* Must NOT exceed BOOT_MAX_ALIGN (default 8): bs_upload() stack-allocates
	 * a `uint8_t buf[BOOT_MAX_ALIGN]` for its unaligned-remainder write and
	 * both memcpy's into it and later reads flash_area_align(fap) bytes back
	 * out of it - returning our real page size (512) here overflows that
	 * buffer. Our flash_area_write() already handles any offset/length via
	 * internal read-modify-write, so no real alignment is required anyway. */
	return 1;
}

uint8_t flash_area_erased_val(const struct flash_area *fap)
{
	(void)fap;
	return 0xff;
}

int flash_area_get_sectors(int fa_id, uint32_t *count, struct flash_sector *sectors)
{
	const struct flash_area *fa = gb_flash_area_lookup((uint8_t)fa_id);
	uint32_t num_sectors;

	if (fa == NULL) {
		return -1;
	}

	num_sectors = fa->fa_size / FLASH_SECTOR_SIZE;

	if (*count < num_sectors) {
		return -1;
	}

	for (uint32_t i = 0; i < num_sectors; i++) {
		sectors[i].fs_off = i * FLASH_SECTOR_SIZE;
		sectors[i].fs_size = FLASH_SECTOR_SIZE;
	}
	*count = num_sectors;
	return 0;
}

int flash_area_id_from_image_slot(int slot)
{
	return flash_area_id_from_multi_image_slot(0, slot);
}

int flash_area_id_from_multi_image_slot(int image_index, int slot)
{
	(void)image_index;
	switch (slot) {
	case 0:
		return PRIMARY_ID;
	case 1:
		return SECONDARY_ID;
	default:
		return -1;
	}
}

int flash_area_id_from_direct_image(int image_id)
{
	/* Match MCUboot's direct-upload numbering: IDs 0 and 1 address the
	 * primary slot of image 0, while ID 2 addresses its secondary slot. */
	switch (image_id) {
	case 0:
	case 1:
		return PRIMARY_ID;
	case 2:
		return SECONDARY_ID;
	default:
		return -1;
	}
}

int flash_area_id_to_multi_image_slot(int image_index, int area_id)
{
	(void)image_index;
	switch (area_id) {
	case PRIMARY_ID:
		return 0;
	case SECONDARY_ID:
		return 1;
	default:
		return -1;
	}
}

int flash_area_get_sector(const struct flash_area *fap, uint32_t off, struct flash_sector *fs)
{
	if (fap == NULL) {
		return -1;
	}

	fs->fs_off = (off / FLASH_SECTOR_SIZE) * FLASH_SECTOR_SIZE;
	fs->fs_size = FLASH_SECTOR_SIZE;
	return 0;
}
