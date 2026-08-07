#include <stdbool.h>
#include "faults.h"
#include "monitoring.h"
#include "io.h"
#include "uart_log.h"

/* Bitmask of all currently latched faults (persists until explicitly cleared) */
static uint32_t active_fault_mask = 0;

/*
 * io_fault_bit_to_sys_bit()
 *
 * Maps an IN_* io bit position (as used in hvps_fault_mask) to the
 * corresponding individual SYS_FAULT_* status bit. Returns NUM_SYS_BITS
 * if the given io bit has no fault status mapping.
 */
static uint32_t io_fault_bit_to_sys_bit(int io_bit)
{
    switch (io_bit)
    {
        case IN_FIL_CLK_FAULT:  return SYS_FAULT_FIL_CLK;
        case IN_CAT_ARC:        return SYS_FAULT_CAT_ARC;
        case IN_FAN_FAULT:      return SYS_FAULT_FAN;
        case IN_OC_24_FAULT:    return SYS_FAULT_OC_24;
        case IN_MASTER_FAULT:   return SYS_FAULT_MASTER;
        case IN_OC_HV_FAULT:    return SYS_FAULT_OC_HV;
        case IN_TEMP_1_FAULT:   return SYS_FAULT_TEMP_1;
        case IN_OC_CAT_FAULT:   return SYS_FAULT_OC_CAT;
        case IN_TEMP_3_FAULT:   return SYS_FAULT_TEMP_3;
        case IN_TEMP_2_FAULT:   return SYS_FAULT_TEMP_2;
        default:                return NUM_SYS_BITS; // no mapping
    }
}

/*
 * sys_fault_bit_to_name()
 *
 * Returns a human-readable name for a given SYS_FAULT_* status bit,
 * for logging purposes. Returns "UNKNOWN_FAULT" if unmapped.
 */
static const char *sys_fault_bit_to_name(uint32_t sys_bit)
{
    switch (sys_bit)
    {
        case SYS_FAULT_FIL_CLK:  return "SYS_FAULT_FIL_CLK";
        case SYS_FAULT_CAT_ARC:  return "SYS_FAULT_CAT_ARC";
        case SYS_FAULT_FAN:      return "SYS_FAULT_FAN";
        case SYS_FAULT_OC_24:    return "SYS_FAULT_OC_24";
        case SYS_FAULT_MASTER:   return "SYS_FAULT_MASTER";
        case SYS_FAULT_OC_HV:    return "SYS_FAULT_OC_HV";
        case SYS_FAULT_TEMP_1:   return "SYS_FAULT_TEMP_1";
        case SYS_FAULT_OC_CAT:   return "SYS_FAULT_OC_CAT";
        case SYS_FAULT_TEMP_3:   return "SYS_FAULT_TEMP_3";
        case SYS_FAULT_TEMP_2:   return "SYS_FAULT_TEMP_2";
        default:                 return "UNKNOWN_FAULT";
    }
}

static void for_each_set_bit_sys_bit(uint32_t mask, bool set_bit)
{
    while (mask != 0)
    {
        int io_bit = __builtin_ctz(mask);   //TBD TODO swap for portable ctz if not GCC/Clang
        mask &= ~(1u << io_bit);

        uint32_t sys_bit = io_fault_bit_to_sys_bit(io_bit);
        if (sys_bit < NUM_SYS_BITS)
        {
            const char *name = sys_fault_bit_to_name(sys_bit);

            if (set_bit)
            {
                set_sys_bit(sys_bit);
                LOG_INFO("FAULT SET: %s (sys_bit %u, io_bit %d)", name, sys_bit, io_bit);
            }
            else
            {
                clear_sys_bit(sys_bit);
                LOG_INFO("FAULT CLEARED: %s (sys_bit %u, io_bit %d)", name, sys_bit, io_bit);
            }
        }
    }
}

void report_fault(uint32_t new_fault_bits)
{
    if (new_fault_bits == 0)
    {
        return;
    }

    active_fault_mask |= new_fault_bits;

    /* Mirror each newly active io fault bit to its individual sys fault flag */
    for_each_set_bit_sys_bit(new_fault_bits, true);
}

void clear_fault(uint32_t fault_bits)
{
    active_fault_mask &= ~fault_bits;

    /* Mirror each cleared io fault bit to its individual sys fault flag */
    for_each_set_bit_sys_bit(fault_bits, false);
}

void clear_all_faults(void)
{
    LOG_INFO("CLEAR ALL FAULTS requested (mask=0x%08X)", active_fault_mask);
    clear_fault(active_fault_mask);
}

uint32_t get_active_faults(void)
{
    return active_fault_mask;
}