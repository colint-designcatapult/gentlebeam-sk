#ifndef GENTLEBEAM_HOST_ATMEL_START_H
#define GENTLEBEAM_HOST_ATMEL_START_H

#if !defined(GENTLEBEAM_HOST_BUILD) || !GENTLEBEAM_HOST_BUILD
#error "The host hardware shim must not be used in firmware builds"
#endif


#include <stdbool.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif
#include <atmel_start_pins.h>


typedef struct {  
  uint32_t                   PIO_PER;        /**< Offset: 0x00 ( /W  32) PIO Enable Register */
  uint32_t                   PIO_PDR;        /**< Offset: 0x04 ( /W  32) PIO Disable Register */
  uint32_t                   PIO_PSR;        /**< Offset: 0x08 (R/   32) PIO Status Register */
  uint8_t                        Reserved1[4];
  uint32_t                   PIO_OER;        /**< Offset: 0x10 ( /W  32) Output Enable Register */
  uint32_t                   PIO_ODR;        /**< Offset: 0x14 ( /W  32) Output Disable Register */
  uint32_t                   PIO_OSR;        /**< Offset: 0x18 (R/   32) Output Status Register */
  uint8_t                        Reserved2[4];
  uint32_t                  PIO_IFER;       /**< Offset: 0x20 ( /W  32) Glitch Input Filter Enable Register */
  uint32_t                  PIO_IFDR;       /**< Offset: 0x24 ( /W  32) Glitch Input Filter Disable Register */
  uint32_t                  PIO_IFSR;       /**< Offset: 0x28 (R/   32) Glitch Input Filter Status Register */
  uint8_t                        Reserved3[4];
  uint32_t                  PIO_SODR;       /**< Offset: 0x30 ( /W  32) Set Output Data Register */
  uint32_t                  PIO_CODR;       /**< Offset: 0x34 ( /W  32) Clear Output Data Register */
  uint32_t                  PIO_ODSR;       /**< Offset: 0x38 (R/W  32) Output Data Status Register */
  uint32_t                  PIO_PDSR;       /**< Offset: 0x3C (R/   32) Pin Data Status Register */
  uint32_t                   PIO_IER;        /**< Offset: 0x40 ( /W  32) Interrupt Enable Register */
  uint32_t                   PIO_IDR;        /**< Offset: 0x44 ( /W  32) Interrupt Disable Register */
  uint32_t                   PIO_IMR;        /**< Offset: 0x48 (R/   32) Interrupt Mask Register */
  uint32_t                   PIO_ISR;        /**< Offset: 0x4C (R/   32) Interrupt Status Register */
  uint32_t                  PIO_MDER;       /**< Offset: 0x50 ( /W  32) Multi-driver Enable Register */
  uint32_t                  PIO_MDDR;       /**< Offset: 0x54 ( /W  32) Multi-driver Disable Register */
  uint32_t                  PIO_MDSR;       /**< Offset: 0x58 (R/   32) Multi-driver Status Register */
  uint8_t                        Reserved4[4];
  uint32_t                  PIO_PUDR;       /**< Offset: 0x60 ( /W  32) Pull-up Disable Register */
  uint32_t                  PIO_PUER;       /**< Offset: 0x64 ( /W  32) Pull-up Enable Register */
  uint32_t                  PIO_PUSR;       /**< Offset: 0x68 (R/   32) Pad Pull-up Status Register */
  uint8_t                        Reserved5[4];
  uint32_t                PIO_ABCDSR[2];  /**< Offset: 0x70 (R/W  32) Peripheral ABCD Select Register 0 */
  uint8_t                        Reserved6[8];
  uint32_t                PIO_IFSCDR;     /**< Offset: 0x80 ( /W  32) Input Filter Slow Clock Disable Register */
  uint32_t                PIO_IFSCER;     /**< Offset: 0x84 ( /W  32) Input Filter Slow Clock Enable Register */
  uint32_t                PIO_IFSCSR;     /**< Offset: 0x88 (R/   32) Input Filter Slow Clock Status Register */
  uint32_t                  PIO_SCDR;       /**< Offset: 0x8C (R/W  32) Slow Clock Divider Debouncing Register */
  uint32_t                 PIO_PPDDR;      /**< Offset: 0x90 ( /W  32) Pad Pull-down Disable Register */
  uint32_t                 PIO_PPDER;      /**< Offset: 0x94 ( /W  32) Pad Pull-down Enable Register */
  uint32_t                 PIO_PPDSR;      /**< Offset: 0x98 (R/   32) Pad Pull-down Status Register */
  uint8_t                        Reserved7[4];
  uint32_t                  PIO_OWER;       /**< Offset: 0xA0 ( /W  32) Output Write Enable */
  uint32_t                  PIO_OWDR;       /**< Offset: 0xA4 ( /W  32) Output Write Disable */
  uint32_t                  PIO_OWSR;       /**< Offset: 0xA8 (R/   32) Output Write Status Register */
  uint8_t                        Reserved8[4];
  uint32_t                 PIO_AIMER;      /**< Offset: 0xB0 ( /W  32) Additional Interrupt Modes Enable Register */
  uint32_t                 PIO_AIMDR;      /**< Offset: 0xB4 ( /W  32) Additional Interrupt Modes Disable Register */
  uint32_t                 PIO_AIMMR;      /**< Offset: 0xB8 (R/   32) Additional Interrupt Modes Mask Register */
  uint8_t                        Reserved9[4];
  uint32_t                   PIO_ESR;        /**< Offset: 0xC0 ( /W  32) Edge Select Register */
  uint32_t                   PIO_LSR;        /**< Offset: 0xC4 ( /W  32) Level Select Register */
  uint32_t                  PIO_ELSR;       /**< Offset: 0xC8 (R/   32) Edge/Level Status Register */
  uint8_t                        Reserved10[4];
  uint32_t                PIO_FELLSR;     /**< Offset: 0xD0 ( /W  32) Falling Edge/Low-Level Select Register */
  uint32_t                PIO_REHLSR;     /**< Offset: 0xD4 ( /W  32) Rising Edge/High-Level Select Register */
  uint32_t                PIO_FRLHSR;     /**< Offset: 0xD8 (R/   32) Fall/Rise - Low/High Status Register */
  uint8_t                        Reserved11[4];
  uint32_t                PIO_LOCKSR;     /**< Offset: 0xE0 (R/   32) Lock Status */
  uint32_t                  PIO_WPMR;       /**< Offset: 0xE4 (R/W  32) Write Protection Mode Register */
  uint32_t                  PIO_WPSR;       /**< Offset: 0xE8 (R/   32) Write Protection Status Register */
  uint8_t                        Reserved12[20];
  uint32_t               PIO_SCHMITT;    /**< Offset: 0x100 (R/W  32) Schmitt Trigger Register */
  uint8_t                        Reserved13[20];
  uint32_t                PIO_DRIVER;     /**< Offset: 0x118 (R/W  32) I/O Drive Register */
  uint8_t                        Reserved14[52];
  uint32_t                  PIO_PCMR;       /**< Offset: 0x150 (R/W  32) Parallel Capture Mode Register */
  uint32_t                 PIO_PCIER;      /**< Offset: 0x154 ( /W  32) Parallel Capture Interrupt Enable Register */
  uint32_t                 PIO_PCIDR;      /**< Offset: 0x158 ( /W  32) Parallel Capture Interrupt Disable Register */
  uint32_t                 PIO_PCIMR;      /**< Offset: 0x15C (R/   32) Parallel Capture Interrupt Mask Register */
  uint32_t                 PIO_PCISR;      /**< Offset: 0x160 (R/   32) Parallel Capture Interrupt Status Register */
  uint32_t                 PIO_PCRHR;      /**< Offset: 0x164 (R/   32) Parallel Capture Reception Holding Register */
} Pio;

extern Pio PIOs[4];
#define PIOA (&PIOs[0])
#define PIOB (&PIOs[1])
#define PIOC (&PIOs[2])
#define PIOD (&PIOs[3])

/* Only the GPIO and timer surface used by the control library is available.
 * This is an inert, single-threaded host build, not a board simulator. */
enum gpio_port { GPIO_PORTA, GPIO_PORTB, GPIO_PORTC, GPIO_PORTD, GPIO_PORTE };

void gpio_set_pin_level(const uint8_t pin, const bool level);
void gpio_toggle_pin_level(const uint8_t pin);
uint32_t gpio_get_port_level(const enum gpio_port port);
bool gpio_get_pin_level(const uint8_t pin);

enum timer_task_mode { TIMER_TASK_ONE_SHOT, TIMER_TASK_REPEAT };
struct timer_task;
typedef void (*timer_cb_t)(const struct timer_task *const timer_task);

/* Host-only task layout and intrusive scheduler state. */
struct timer_task {
	uint32_t interval; /* sys_check_timeouts ticks; one tick is one millisecond. */
	timer_cb_t cb;
	enum timer_task_mode mode;
	struct timer_task *next;
	uint32_t deadline;
	bool scheduled;
};

struct timer_descriptor;
extern struct timer_descriptor VTIMER;
int32_t timer_add_task(struct timer_descriptor *const descr, struct timer_task *const task);
int32_t timer_start(struct timer_descriptor *const descr);
uint32_t SysTick_Config(uint32_t ticks);
void gpio_set_pin_direction(uint8_t pin, uint32_t direction);
void gpio_set_pin_function(uint32_t pin, uint32_t function);

typedef void (*FUNC_PTR)(void);
enum mac_async_cb_type {
	MAC_ASYNC_RECEIVE_CB, /*!< One or more frame been received */
	MAC_ASYNC_TRANSMIT_CB /*!< One or more frame been transmited */
};

struct mac_async_descriptor;
typedef void (*mac_async_cb_t)(struct mac_async_descriptor *const descr);

struct mac_async_callbacks {
	mac_async_cb_t receive;
	mac_async_cb_t transmit;
};

struct ethernet_phy_descriptor {
	void *mac;  /* MAC descriptor handler */
	uint16_t                     addr; /* PHY address, defined by IEEE802.3
	                                      section 22.2.4.5.5 */
};

struct mac_async_descriptor {
	void *dev; /*!< MAC HPL device descriptor */
	struct mac_async_callbacks cb; /*!< MAC callback handlers */
};

extern struct mac_async_descriptor MACIF;
extern struct ethernet_phy_descriptor MACIF_PHY_desc;

typedef void (*usart_cb_t)(const struct usart_async_descriptor *const descr);


extern struct usart_async_descriptor HVPS_UART;
extern struct usart_async_descriptor HB_UART;

#define USART_ASYNC_RXC_CB 1
#define USART_ASYNC_TXC_CB 2

#define ERR_NONE 0

#ifdef __cplusplus
}
#endif

#endif

