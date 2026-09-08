#include <string.h>
#include "stm32f3xx_hal.h"
#include "flash_map_backend/flash_map_backend.h"
#include "bootutil_priv.h"

#define FLASH_BASE_ADDRESS  0x08000000u         /* STM32F3 flash base address */
#define BOOTLOADER_SIZE     0xC000u             /* 49152 */
#define SLOT_SIZE           0x2D000u            /* 184320 (180K), single primary slot only */
#define PAGE_SIZE           0x800u              /* 2048 */

static const struct flash_area areas[] = {
    { FLASH_AREA_BOOTLOADER, FLASH_DEVICE_INTERNAL_FLASH, 0, 0, BOOTLOADER_SIZE },
    { FLASH_AREA_IMAGE_0_PRIMARY, FLASH_DEVICE_INTERNAL_FLASH, 0, BOOTLOADER_SIZE, SLOT_SIZE },
};

static const struct flash_area *find_area(uint8_t id)
{
    for (uint32_t index = 0; index < sizeof(areas) / sizeof(areas[0]); index++) {
        if (areas[index].fa_id == id) return &areas[index];
    }
    return 0;
}

int flash_area_open(uint8_t id, const struct flash_area **area_outp)
{
    *area_outp = find_area(id);
    return *area_outp == 0 ? -1 : 0;
}

int boot_open_all_flash_areas(struct boot_loader_state *state)
{
    return flash_area_open(FLASH_AREA_IMAGE_PRIMARY(0), &state->imgs[0][0].area);
}

void boot_close_all_flash_areas(struct boot_loader_state *state)
{
    if (state->imgs[0][0].area != 0) {
        flash_area_close(state->imgs[0][0].area);
        state->imgs[0][0].area = 0;
    }
}

void flash_area_close(const struct flash_area *area) { (void)area; }

int flash_area_read(const struct flash_area *area, uint32_t off, void *dst, uint32_t len)
{
    if (off > area->fa_size || len > area->fa_size - off) return -1;
    memcpy(dst, (const void *)(FLASH_BASE_ADDRESS + area->fa_off + off), len);
    return 0;
}

int flash_area_write(const struct flash_area *area, uint32_t off, const void *src, uint32_t len)
{
    if (off > area->fa_size || len > area->fa_size - off ||
        ((area->fa_off + off) & 3u) != 0 || ((uintptr_t)src & 3u) != 0 || (len & 3u) != 0) return -1;

    HAL_StatusTypeDef status = HAL_FLASH_Unlock();
    const uint32_t *words = (const uint32_t *)src;
    uint32_t address = FLASH_BASE_ADDRESS + area->fa_off + off;
    for (uint32_t index = 0; status == HAL_OK && index < len / 4; index++) {
        status = HAL_FLASH_Program(FLASH_TYPEPROGRAM_WORD, address, words[index]);
        address += 4;
    }
    HAL_FLASH_Lock();
    return status == HAL_OK ? 0 : -1;
}

int flash_area_erase(const struct flash_area *area, uint32_t off, uint32_t len)
{
    if (off > area->fa_size || len > area->fa_size - off ||
        (off & (PAGE_SIZE - 1)) != 0 || (len & (PAGE_SIZE - 1)) != 0) return -1;

    FLASH_EraseInitTypeDef erase = {
        .TypeErase = FLASH_TYPEERASE_PAGES,
        .PageAddress = FLASH_BASE_ADDRESS + area->fa_off + off,
        .NbPages = len / PAGE_SIZE,
    };
    uint32_t page_error = 0;
    HAL_FLASH_Unlock();
    HAL_StatusTypeDef status = HAL_FLASHEx_Erase(&erase, &page_error);
    HAL_FLASH_Lock();
    return status == HAL_OK ? 0 : -1;
}

uint32_t flash_area_align(const struct flash_area *area) { (void)area; return 4; }
uint8_t flash_area_erased_val(const struct flash_area *area) { (void)area; return 0xff; }

int flash_area_get_sectors(int fa_id, uint32_t *count, struct flash_sector *sectors)
{
    const struct flash_area *area = find_area((uint8_t)fa_id);
    uint32_t sector_count = area == 0 ? 0 : area->fa_size / PAGE_SIZE;
    if (area == 0 || sectors == 0 || *count < sector_count) return -1;
    for (uint32_t index = 0; index < sector_count; index++) {
        sectors[index].fs_off = index * PAGE_SIZE;
        sectors[index].fs_size = PAGE_SIZE;
    }
    *count = sector_count;
    return 0;
}

int flash_area_get_sector(const struct flash_area *area, uint32_t off, struct flash_sector *sector)
{
    if (off >= area->fa_size || sector == 0) return -1;
    sector->fs_off = off & ~(PAGE_SIZE - 1);
    sector->fs_size = PAGE_SIZE;
    return 0;
}

int flash_area_id_from_multi_image_slot(int image_index, int slot)
{
    return image_index == 0 && slot == 0 ? FLASH_AREA_IMAGE_0_PRIMARY :
           image_index == 0 && slot == 1 ? FLASH_AREA_IMAGE_0_PRIMARY : -1;
}

int flash_area_id_from_image_slot(int slot)
{
    return flash_area_id_from_multi_image_slot(0, slot);
}

int flash_area_id_to_multi_image_slot(int image_index, int area_id)
{
    return image_index == 0 && area_id == FLASH_AREA_IMAGE_0_PRIMARY ? 0 :
           image_index == 0 && area_id == FLASH_AREA_IMAGE_0_PRIMARY ? 1 : -1;
}

int flash_area_to_sectors(int idx, int *count, struct flash_area *ret)
{
    const struct flash_area *area = find_area((uint8_t)idx);
    if (area == 0 || count == 0 || ret == 0 || *count < (int)(area->fa_size / PAGE_SIZE)) return -1;
    for (uint32_t index = 0; index < area->fa_size / PAGE_SIZE; index++) {
        ret[index] = *area;
        ret[index].fa_off += index * PAGE_SIZE;
        ret[index].fa_size = PAGE_SIZE;
    }
    *count = (int)(area->fa_size / PAGE_SIZE);
    return 0;
}