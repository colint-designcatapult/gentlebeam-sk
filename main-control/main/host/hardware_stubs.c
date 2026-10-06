#include <atmel_start.h>
#include <stddef.h>

#include "hardware_backend.h"
#include "ext_timers_i2c.h"
#include "ext_dac.h"
#include "ftdi.h"
#include "custom_eth_ipstack_main.h"
#include "uart_backend.h"

GcbIoModel gIoModel;

struct timer_descriptor {
	struct timer_task *tasks;
	uint32_t ticks;
	bool running;
};
struct timer_descriptor VTIMER;


Pio PIOs[4];

#define HOST_OTHER_INTERLOCK_MASK (UINT32_C(0x0000FFFF) | (UINT32_C(0x3) << 18))

static bool master_fault_latched;
static bool clear_fault_was_high;


void atmel_start_init(void)
{
	// Default model values
	gIoModel.backup_timer1.simulate = true;
	gIoModel.backup_timer2.simulate = true;
	gIoModel.dac.coil.simulate = true;
	gIoModel.gpio.simulate = true;
	/* Healthy physical inputs, with the fault-clear output initially low.
	 * Tests can open individual interlocks through the I/O model; the latch
	 * still requires the real firmware's fault-clear pulse to recover. */
	const uint32_t initial_levels[GCB_GPIO_PORT_COUNT] = {
		0, 0, HOST_OTHER_INTERLOCK_MASK | (UINT32_C(1) << 16), 0
	};
	host_apply_gpio_port_levels(initial_levels);
}

static Pio *gpio_bank_from_pin(uint8_t pin)
{
	const uint8_t bank = pin >> 5;

	return bank < (sizeof(PIOs) / sizeof(PIOs[0])) ? &PIOs[bank] : NULL;
}

static uint32_t gpio_pin_mask(uint8_t pin)
{
	return UINT32_C(1) << (pin & 0x1f);
}

void gpio_set_pin_level(const uint8_t pin, const bool level)
{
	Pio *const bank = gpio_bank_from_pin(pin);
	const uint32_t mask = gpio_pin_mask(pin);

	if (bank == NULL) {
		return;
	}
	if (level) {
		bank->PIO_SODR = mask;
		bank->PIO_ODSR |= mask;
		bank->PIO_PDSR |= mask;
	} else {
		bank->PIO_CODR = mask;
		bank->PIO_ODSR &= ~mask;
		bank->PIO_PDSR &= ~mask;
	}
	gIoModel.gpio.port_levels[pin >> 5] = bank->PIO_PDSR;
}

void gpio_toggle_pin_level(const uint8_t pin)
{
	Pio *const bank = gpio_bank_from_pin(pin);
	const uint32_t mask = gpio_pin_mask(pin);

	if (bank == NULL) {
		return;
	}
	gpio_set_pin_level(pin, (bank->PIO_PDSR & mask) == 0);
}

uint32_t gpio_get_port_level(const enum gpio_port port)
{
	return (unsigned int)port < (sizeof(PIOs) / sizeof(PIOs[0]))
		? PIOs[port].PIO_PDSR
		: 0;
}

bool gpio_get_pin_level(const uint8_t pin)
{
	Pio *const bank = gpio_bank_from_pin(pin);

	return bank != NULL && (bank->PIO_PDSR & gpio_pin_mask(pin)) != 0;
}

void host_apply_gpio_port_levels(const uint32_t port_levels[GCB_GPIO_PORT_COUNT])
{
	for (size_t index = 0; index < GCB_GPIO_PORT_COUNT; index++) {
		PIOs[index].PIO_ODSR = port_levels[index];
		PIOs[index].PIO_PDSR = port_levels[index];
		gIoModel.gpio.port_levels[index] = port_levels[index];
	}
}

static void simulate_interlock_latch(void)
{
	const uint32_t port_c_levels = PIOs[GPIO_PORTC].PIO_PDSR;
	const bool clear_fault_high = (port_c_levels & gpio_pin_mask(IO_CLEAR_FAULT)) != 0;

	if (!gIoModel.gpio.simulate) {
		clear_fault_was_high = clear_fault_high;
		return;
	}
	if ((port_c_levels & HOST_OTHER_INTERLOCK_MASK) != HOST_OTHER_INTERLOCK_MASK) {
		master_fault_latched = true;
	} else if (clear_fault_high && !clear_fault_was_high) {
		master_fault_latched = false;
	}
	gpio_set_pin_level(IO_MASTER_FAULTn, !master_fault_latched);
	clear_fault_was_high = clear_fault_high;
}

static void host_timer_tick(void)
{
	struct timer_task **link = &VTIMER.tasks;

	if (!VTIMER.running) {
		return;
	}
	VTIMER.ticks++;
	while (*link != NULL) {
		struct timer_task *const task = *link;

		if ((int32_t)(VTIMER.ticks - task->deadline) < 0) {
			link = &task->next;
			continue;
		}
		if (task->mode == TIMER_TASK_REPEAT) {
			task->deadline += task->interval;
			link = &task->next;
		} else {
			*link = task->next;
			task->scheduled = false;
		}
		task->cb(task);
	}
}

int32_t timer_add_task(struct timer_descriptor *const descr, struct timer_task *const task)
{
	if (descr == NULL || task == NULL || task->cb == NULL || task->interval == 0 ||
	    task->scheduled) {
		return -1;
	}
	task->deadline = descr->ticks + task->interval;
	task->scheduled = true;
	task->next = descr->tasks;
	descr->tasks = task;
	return ERR_NONE;
}

int32_t timer_start(struct timer_descriptor *const descr)
{
	if (descr == NULL) {
		return -1;
	}
	descr->running = true;
	return ERR_NONE;
}

uint32_t SysTick_Config(uint32_t ticks)
{
	(void)ticks;
	return 0;
}

void gpio_set_pin_direction(uint8_t pin, uint32_t direction)
{
	(void)pin;
	(void)direction;
}

void gpio_set_pin_function(uint32_t pin, uint32_t function)
{
	(void)pin;
	(void)function;
}



void init_ftdi(void)
{
}

void process_ftdi(void)
{
}


uint32_t get_app_crc(void)
{
	/* Firmware reads this from MCU flash in ftdi.c. There is no application
	 * flash image on the host; this sentinel is not a computed checksum. */
	return UINT32_MAX;
}



static void tick_1ms()
{
	host_io_model_tick();
	simulate_interlock_latch();
	SysTick_Handler();
	host_timer_tick();
	ext_timers_i2c_tick_1ms();
	hvps_tick_1ms();
	uart_tick_1ms();
	eth_ipstack_poll();
}

#if defined(_WIN32)
#include <windows.h>

void sys_check_timeouts(void)
{
	static LARGE_INTEGER frequency;
	static LARGE_INTEGER deadline;
	static uint32_t fractional_ticks;
	LARGE_INTEGER now;

	if (frequency.QuadPart == 0) {
		QueryPerformanceFrequency(&frequency);
		QueryPerformanceCounter(&deadline);
		deadline.QuadPart += frequency.QuadPart / 1000;
		fractional_ticks = (uint32_t)(frequency.QuadPart % 1000);
	}

	QueryPerformanceCounter(&now);
	while (now.QuadPart < deadline.QuadPart) {
		const uint64_t remaining_ticks = deadline.QuadPart - now.QuadPart;
		const DWORD remaining_ms = (DWORD)((remaining_ticks * 1000u +
		                                    frequency.QuadPart - 1) /
		                                   frequency.QuadPart);
		Sleep(remaining_ms);
		QueryPerformanceCounter(&now);
	}

	tick_1ms();
	deadline.QuadPart += frequency.QuadPart / 1000;
	fractional_ticks += (uint32_t)(frequency.QuadPart % 1000);
	if (fractional_ticks >= 1000) {
		deadline.QuadPart++;
		fractional_ticks -= 1000;
	}
	/* Return after each tick, even during catch-up: firmware must consume
	 * timer-ready flags before the next tick can set them again. */
}
#else
#include <errno.h>
#include <time.h>

void sys_check_timeouts(void)
{
	static struct timespec deadline;
	struct timespec now;

	clock_gettime(CLOCK_MONOTONIC, &now);
	if (deadline.tv_sec == 0 && deadline.tv_nsec == 0) {
		deadline = now;
		deadline.tv_nsec += 1000000;
		if (deadline.tv_nsec >= 1000000000) {
			deadline.tv_sec++;
			deadline.tv_nsec -= 1000000000;
		}
	}

	while ((now.tv_sec < deadline.tv_sec) ||
	       (now.tv_sec == deadline.tv_sec && now.tv_nsec < deadline.tv_nsec)) {
		struct timespec remaining = {
			.tv_sec = deadline.tv_sec - now.tv_sec,
			.tv_nsec = deadline.tv_nsec - now.tv_nsec,
		};
		if (remaining.tv_nsec < 0) {
			remaining.tv_sec--;
			remaining.tv_nsec += 1000000000;
		}
		while (nanosleep(&remaining, &remaining) == -1 && errno == EINTR) {
		}
		clock_gettime(CLOCK_MONOTONIC, &now);
	}

	tick_1ms();
	deadline.tv_nsec += 1000000;
	if (deadline.tv_nsec >= 1000000000) {
		deadline.tv_sec++;
		deadline.tv_nsec -= 1000000000;
	}
	/* Let the firmware consume this tick before catching up another. */
}
#endif