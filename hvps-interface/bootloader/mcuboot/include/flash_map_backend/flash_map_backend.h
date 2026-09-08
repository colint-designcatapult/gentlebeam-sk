#ifndef FLASH_MAP_BACKEND_H
#define FLASH_MAP_BACKEND_H

#include <stdint.h>
#include <sysflash/sysflash.h>

struct flash_area { uint8_t fa_id; uint8_t fa_device_id; uint16_t pad16; uint32_t fa_off; uint32_t fa_size; };
struct flash_sector { uint32_t fs_off; uint32_t fs_size; };
struct boot_loader_state;

int flash_area_open(uint8_t id, const struct flash_area **area_outp);
int boot_open_all_flash_areas(struct boot_loader_state *state);
void boot_close_all_flash_areas(struct boot_loader_state *state);
void flash_area_close(const struct flash_area *area);
int flash_area_read(const struct flash_area *area, uint32_t off, void *dst, uint32_t len);
int flash_area_write(const struct flash_area *area, uint32_t off, const void *src, uint32_t len);
int flash_area_erase(const struct flash_area *area, uint32_t off, uint32_t len);
uint32_t flash_area_align(const struct flash_area *area);
uint8_t flash_area_erased_val(const struct flash_area *area);
int flash_area_get_sectors(int fa_id, uint32_t *count, struct flash_sector *sectors);
int flash_area_get_sector(const struct flash_area *area, uint32_t off, struct flash_sector *sector);
int flash_area_to_sectors(int idx, int *cnt, struct flash_area *ret);
int flash_area_id_from_multi_image_slot(int image_index, int slot);
int flash_area_id_from_image_slot(int slot);
int flash_area_id_to_multi_image_slot(int image_index, int area_id);

static inline uint8_t flash_area_get_id(const struct flash_area *area) { return area->fa_id; }
static inline uint8_t flash_area_get_device_id(const struct flash_area *area) { return area->fa_device_id; }
static inline uint32_t flash_area_get_off(const struct flash_area *area) { return area->fa_off; }
static inline uint32_t flash_area_get_size(const struct flash_area *area) { return area->fa_size; }
static inline uint32_t flash_sector_get_off(const struct flash_sector *sector) { return sector->fs_off; }
static inline uint32_t flash_sector_get_size(const struct flash_sector *sector) { return sector->fs_size; }

#endif