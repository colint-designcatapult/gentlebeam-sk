/*
 * Copyright (c) 2018 Nordic Semiconductor ASA
 * Copyright (c) 2015 Runtime Inc
 * Copyright (c) 2020 Embedded Planet
 * SPDX-License-Identifier: Apache-2.0
 *
 * Flash area API used by mcuboot's bootutil/boot_serial. This header mirrors
 * the interface expected by mcuboot; the implementation lives in
 * flash_map_backend.c and is backed by the SAME70 internal flash (FLASH_0).
 */

#ifndef H_UTIL_FLASH_MAP_
#define H_UTIL_FLASH_MAP_

#ifdef __cplusplus
extern "C" {
#endif

#include <inttypes.h>

struct flash_area {
	uint8_t fa_id;
	uint8_t fa_device_id;
	uint16_t pad16;
	uint32_t fa_off;
	uint32_t fa_size;
};

static inline uint8_t flash_area_get_id(const struct flash_area *fa)
{
	return fa->fa_id;
}

static inline uint8_t flash_area_get_device_id(const struct flash_area *fa)
{
	return fa->fa_device_id;
}

static inline uint32_t flash_area_get_off(const struct flash_area *fa)
{
	return fa->fa_off;
}

static inline uint32_t flash_area_get_size(const struct flash_area *fa)
{
	return fa->fa_size;
}

struct flash_sector {
	uint32_t fs_off;
	uint32_t fs_size;
};

static inline uint32_t flash_sector_get_off(const struct flash_sector *fs)
{
	return fs->fs_off;
}

static inline uint32_t flash_sector_get_size(const struct flash_sector *fs)
{
	return fs->fs_size;
}

int flash_area_open(uint8_t id, const struct flash_area **fapp);
void flash_area_close(const struct flash_area *fap);
int flash_area_read(const struct flash_area *fap, uint32_t off, void *dst, uint32_t len);
int flash_area_write(const struct flash_area *fap, uint32_t off, const void *src, uint32_t len);
int flash_area_erase(const struct flash_area *fap, uint32_t off, uint32_t len);
uint32_t flash_area_align(const struct flash_area *fap);
uint8_t flash_area_erased_val(const struct flash_area *fap);
int flash_area_get_sectors(int fa_id, uint32_t *count, struct flash_sector *sectors);
int flash_area_id_from_image_slot(int slot);
int flash_area_id_from_multi_image_slot(int image_index, int slot);
int flash_area_id_to_multi_image_slot(int image_index, int area_id);
int flash_area_id_from_direct_image(int image_id);
int flash_area_get_sector(const struct flash_area *fap, uint32_t off, struct flash_sector *fs);

/* Unlocks the SAME70 EFC lock regions covering the primary/secondary/scratch
 * areas. If those regions are left locked (factory default or a leftover
 * from earlier programming), every flash_area_write() to them silently fails
 * with ERR_DENIED, which looks exactly like a stalled 0%-progress upload. */
void flash_map_backend_init(void);

#ifdef __cplusplus
}
#endif

#endif /* H_UTIL_FLASH_MAP_ */
