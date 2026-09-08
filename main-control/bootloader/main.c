#include <atmel_start.h>

#include <atmel_start_pins.h>
#include <bootutil/bootutil.h>
#include <bootutil/image.h>
#include <bootutil/fault_injection_hardening.h>
#include <flash_map_backend/flash_map_backend.h>

#include "mcuboot_port/boot_uart.h"

typedef void (*fpJumpHandler)(void);

#define FLASH_BASE_ADDR   0x00400000u
#define BOOTLOADER_REQUEST_MAGIC 0x424F4F54u

static void jump_to_image(const struct boot_rsp *rsp)
{
	uint32_t app_addr;
	uint32_t stack_ptr_val;
	fpJumpHandler app_reset_handler;

	app_addr = FLASH_BASE_ADDR + rsp->br_image_off + rsp->br_hdr->ih_hdr_size;

	__DSB();
	__ISB();

	SCB->VTOR = app_addr & SCB_VTOR_TBLOFF_Msk;

	__DSB();
	__ISB();

	__enable_irq();

	stack_ptr_val = *(uint32_t *)(app_addr);
	__set_MSP(stack_ptr_val);

	app_reset_handler = (fpJumpHandler)(*(uint32_t *)(app_addr + 4));
	(*app_reset_handler)();
}

int main(void)
{
	struct boot_rsp rsp;
	FIH_DECLARE(fih_rc, FIH_FAILURE);

	/* Initializes MCU, drivers and middleware */
	atmel_start_init();

	/* Diagnostic checkpoint: reached main() past early clock/driver init. */
	/* LEDs are active-low: false drives the pin on. */
	gpio_set_pin_level(LED1, false);

	boot_uart_init();

	/* Ensure the primary/secondary/scratch flash lock regions aren't left
	 * locked (factory default or leftover from earlier programming) - a
	 * locked write silently fails, which looks exactly like a stalled
	 * upload rather than an error. */
	flash_map_backend_init();

	/* The application writes this request before resetting in response to
	 * *BOOTL\n. Clear it first so the next reset boots normally. */
	if (GPBR->SYS_GPBR[0] == BOOTLOADER_REQUEST_MAGIC) {
		GPBR->SYS_GPBR[0] = 0;
		__DSB();
		boot_serial_start(&boot_uart_funcs);
	}

	/* Validate/prepare the image (copying the secondary slot over the
	 * primary slot if a pending upgrade is present) and boot it. If no
	 * bootable image is found, service mcumgr image uploads over the FTDI
	 * UART indefinitely instead of jumping into garbage. */
	FIH_CALL(boot_go, fih_rc, &rsp);
	if (FIH_NOT_EQ(fih_rc, FIH_SUCCESS)) {
		/* Diagnostic checkpoint: boot_go failed, entering serial recovery. */
		gpio_set_pin_level(LED3, false);
		boot_serial_start(&boot_uart_funcs);
	}

	jump_to_image(&rsp);

	while (1) {
		/* Unreachable */
	}
}

//RSTC->RSTC_CR = RSTC_CR_KEY_PASSWD | RSTC_CR_PROCRST;