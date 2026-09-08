#include <stdbool.h>
#include "main.h"
#include "os/os.h"
#include "os/os_cputime.h"
#include "crc/crc16.h"

#define UART3_RX_BUFFER_SIZE 1024u

static uint8_t uart3_rx_byte;
static uint8_t uart3_rx_buffer[UART3_RX_BUFFER_SIZE];
static volatile uint16_t uart3_rx_head;
static volatile uint16_t uart3_rx_tail;
static volatile bool uart3_upload_active;

void mcuboot_bootloader_blink(void);

static void set_idle_leds(void)
{
    HAL_GPIO_WritePin(GPIOD, IO_TEST_1_Pin, GPIO_PIN_RESET);
    HAL_GPIO_WritePin(GPIOD, IO_TEST_2_Pin, GPIO_PIN_SET);
    HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
}

static int uart3_rx_get(uint8_t *byte, uint32_t timeout_ms)
{
    uint32_t start = HAL_GetTick();
    while (uart3_rx_head == uart3_rx_tail) {
        if ((uint32_t)(HAL_GetTick() - start) >= timeout_ms) {
            return 0;
        }
    }

    *byte = uart3_rx_buffer[uart3_rx_tail];
    uart3_rx_tail = (uint16_t)((uart3_rx_tail + 1u) % UART3_RX_BUFFER_SIZE);
    return 1;
}

void mcuboot_uart_start(void)
{
    uart3_rx_head = 0;
    uart3_rx_tail = 0;
    if (!uart3_upload_active) {
        set_idle_leds();
    }
    HAL_UART_Receive_IT(&huart3, &uart3_rx_byte, 1);
}

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
    if (huart == &huart3) {
        uint16_t next_head = (uint16_t)((uart3_rx_head + 1u) % UART3_RX_BUFFER_SIZE);
        if (next_head != uart3_rx_tail) {
            uart3_rx_buffer[uart3_rx_head] = uart3_rx_byte;
            uart3_rx_head = next_head;
        }
        HAL_UART_Receive_IT(&huart3, &uart3_rx_byte, 1);
    }
}

void mcuboot_bootloader_blink(void)
{
    static uint32_t last_toggle;
    static bool chase_state = false;
    uint32_t now = HAL_GetTick();

    /* uart3_upload_active is now just "a line is actively being received";
     * it no longer distinguishes upload chunks from other SMP traffic. */
    if (uart3_upload_active && (uint32_t)(now - last_toggle) >= 150u) {
        last_toggle = now;
        HAL_GPIO_WritePin(GPIOD, IO_TEST_2_Pin, RESET);
        
        /* Chase pattern: IO_TEST_1 and IO_TEST_3 alternate */
        if (chase_state) {
            HAL_GPIO_WritePin(GPIOD, IO_TEST_1_Pin, GPIO_PIN_SET);
            HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_RESET);
        } else {
            HAL_GPIO_WritePin(GPIOD, IO_TEST_1_Pin, GPIO_PIN_RESET);
            HAL_GPIO_WritePin(GPIOD, IO_TEST_3_Pin, GPIO_PIN_SET);
        }
        chase_state = !chase_state;
    }
}

uint32_t os_uptime_get_ms_32(void)
{
    return HAL_GetTick();
}

void os_cputime_delay_usecs(uint32_t usecs)
{
    HAL_Delay((usecs + 999u) / 1000u);
}

uint16_t crc16_ccitt(uint16_t crc, const void *data, uint16_t length)
{
    const uint8_t *bytes = (const uint8_t *)data;
    while (length-- != 0) {
        crc ^= (uint16_t)*bytes++ << 8;
        for (uint8_t bit = 0; bit < 8; bit++) {
            crc = (crc & 0x8000u) != 0 ? (uint16_t)((crc << 1) ^ 0x1021u) : (uint16_t)(crc << 1);
        }
    }
    return crc;
}

int mcuboot_uart_read(char *buffer, int count, int *newline)
{
    uint8_t byte;
    int received = 0;
    *newline = 0;

    /* Line reader for the base64/NLIP framing: boot_serial.c reassembles
     * complete NLIP lines (pkt-start/data-cont marker + base64 + '\n')
     * itself, so this only needs to hand back one line at a time. */
    while (received < count) {
        if (!uart3_rx_get(&byte, received == 0 ? 250 : 100)) {
            /* No more data arrived; hand back whatever partial line we have. */
            uart3_upload_active = false;
            set_idle_leds();
            return received;
        }

        uart3_upload_active = true;
        buffer[received++] = (char)byte;
        mcuboot_bootloader_blink();

        if (byte == (uint8_t)'\n') {
            *newline = 1;
            break;
        }
    }

    if (*newline) {
        uart3_upload_active = false;
    }

    return received;
}

void mcuboot_uart_write(const char *buffer, int count)
{
    HAL_UART_Transmit(&huart3, (uint8_t *)buffer, (uint16_t)count, HAL_MAX_DELAY);
}

void hal_system_reset(void)
{
    HAL_NVIC_SystemReset();
    while (1) {
    }
}