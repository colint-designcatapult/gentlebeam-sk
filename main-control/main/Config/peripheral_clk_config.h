/* Auto-generated config file peripheral_clk_config.h */
#ifndef PERIPHERAL_CLK_CONFIG_H
#define PERIPHERAL_CLK_CONFIG_H

// <<< Use Configuration Wizard in Context Menu >>>

// <h> HSMCI Clock Settings
// <y> HSMCI Clock source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <i> This defines the clock source for the HSMCI
// <id> hsmci_clock_source
#ifndef CONF_HSMCI_SRC
#define CONF_HSMCI_SRC CONF_SRC_MCK
#endif
// </h>

/**
 * \def HSMCI FREQUENCY
 * \brief HSMCI's Clock frequency
 */
#ifndef CONF_HSMCI_FREQUENCY
#define CONF_HSMCI_FREQUENCY 150000000
#endif

/**
 * \def CONF_HCLK_FREQUENCY
 * \brief HCLK's Clock frequency
 */
#ifndef CONF_HCLK_FREQUENCY
#define CONF_HCLK_FREQUENCY 300000000
#endif

/**
 * \def CONF_FCLK_FREQUENCY
 * \brief FCLK's Clock frequency
 */
#ifndef CONF_FCLK_FREQUENCY
#define CONF_FCLK_FREQUENCY 300000000
#endif

/**
 * \def CONF_CPU_FREQUENCY
 * \brief CPU's Clock frequency
 */
#ifndef CONF_CPU_FREQUENCY
#define CONF_CPU_FREQUENCY 300000000
#endif

/**
 * \def CONF_SLCK_FREQUENCY
 * \brief Slow Clock frequency
 */
#define CONF_SLCK_FREQUENCY 0

/**
 * \def CONF_MCK_FREQUENCY
 * \brief Master Clock frequency
 */
#define CONF_MCK_FREQUENCY 150000000

/**
 * \def CONF_PCK6_FREQUENCY
 * \brief Programmable Clock Controller 6 frequency
 */
#define CONF_PCK6_FREQUENCY 60000

// <o> RTC Clock source
// <0=> SLCK for Peripheral
// <i> This defines the clock source for the RTC
// <id> rtc_clock_source
#ifndef CONF_RTC_SRC
#define CONF_RTC_SRC 0
#endif

/**
 * \def CONF_CLK_RTC_FREQUENCY
 * \brief RTC's Clock frequency
 */
#ifndef CONF_CLK_RTC_FREQUENCY
#define CONF_CLK_RTC_FREQUENCY 0
#endif

// <h> SPI1 Clock Settings
// <y> SPI1 Clock source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <i> This defines the clock source for the SPI1
// <id> spi_clock_source
#ifndef CONF_SPI1_SRC
#define CONF_SPI1_SRC CONF_SRC_MCK
#endif
// </h>

/**
 * \def SPI1 FREQUENCY
 * \brief SPI1's Clock frequency
 */
#ifndef CONF_SPI1_FREQUENCY
#define CONF_SPI1_FREQUENCY 150000000
#endif

// <y> TC Clock Source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <id> tc_clock_source
// <i> Select the clock source for TC.
#ifndef CONF_TC0_SRC
#define CONF_TC0_SRC CONF_SRC_MCK
#endif

/**
 * \def CONF_TC0_FREQUENCY
 * \brief TC0's Clock frequency
 */
#ifndef CONF_TC0_FREQUENCY
#define CONF_TC0_FREQUENCY 150000000
#endif

// <y> TC Clock Source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <id> tc_clock_source
// <i> Select the clock source for TC.
#ifndef CONF_TC1_SRC
#define CONF_TC1_SRC CONF_SRC_MCK
#endif

/**
 * \def CONF_TC1_FREQUENCY
 * \brief TC1's Clock frequency
 */
#ifndef CONF_TC1_FREQUENCY
#define CONF_TC1_FREQUENCY 150000000
#endif

// <h> TWIHS Clock Settings
// <y> TWIHS Clock source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <i> This defines the clock source for the TWIHS
// <id> twihs_clock_source
#ifndef CONF_TWIHS0_SRC
#define CONF_TWIHS0_SRC CONF_SRC_MCK
#endif
// </h>

/**
 * \def TWIHS FREQUENCY
 * \brief TWIHS's Clock frequency
 */
#ifndef CONF_TWIHS0_FREQUENCY
#define CONF_TWIHS0_FREQUENCY 150000000
#endif

// <h> TWIHS Clock Settings
// <y> TWIHS Clock source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <i> This defines the clock source for the TWIHS
// <id> twihs_clock_source
#ifndef CONF_TWIHS2_SRC
#define CONF_TWIHS2_SRC CONF_SRC_MCK
#endif
// </h>

/**
 * \def TWIHS FREQUENCY
 * \brief TWIHS's Clock frequency
 */
#ifndef CONF_TWIHS2_FREQUENCY
#define CONF_TWIHS2_FREQUENCY 150000000
#endif

// <h> UART Clock Settings
// <o> UART Clock source
// <0=> Master Clock (MCK)
// <1=> Programmable Clock Controller 4 (PMC_PCK4)
// <i> This defines the clock source for the UART
// <id> uart_clock_source
#ifndef CONF_UART0_CK_SRC
#define CONF_UART0_CK_SRC 0
#endif
// </h>

/**
 * \def UART FREQUENCY
 * \brief UART's Clock frequency
 */
#ifndef CONF_UART0_FREQUENCY
#define CONF_UART0_FREQUENCY 150000000
#endif

// <h> UART Clock Settings
// <o> UART Clock source
// <0=> Master Clock (MCK)
// <1=> Programmable Clock Controller 4 (PMC_PCK4)
// <i> This defines the clock source for the UART
// <id> uart_clock_source
#ifndef CONF_UART1_CK_SRC
#define CONF_UART1_CK_SRC 0
#endif
// </h>

/**
 * \def UART FREQUENCY
 * \brief UART's Clock frequency
 */
#ifndef CONF_UART1_FREQUENCY
#define CONF_UART1_FREQUENCY 150000000
#endif

// <h> UART Clock Settings
// <o> UART Clock source
// <0=> Master Clock (MCK)
// <1=> Programmable Clock Controller 4 (PMC_PCK4)
// <i> This defines the clock source for the UART
// <id> uart_clock_source
#ifndef CONF_UART2_CK_SRC
#define CONF_UART2_CK_SRC 0
#endif
// </h>

/**
 * \def UART FREQUENCY
 * \brief UART's Clock frequency
 */
#ifndef CONF_UART2_FREQUENCY
#define CONF_UART2_FREQUENCY 150000000
#endif

// <h> UART Clock Settings
// <o> UART Clock source
// <0=> Master Clock (MCK)
// <1=> Programmable Clock Controller 4 (PMC_PCK4)
// <i> This defines the clock source for the UART
// <id> uart_clock_source
#ifndef CONF_UART4_CK_SRC
#define CONF_UART4_CK_SRC 0
#endif
// </h>

/**
 * \def UART FREQUENCY
 * \brief UART's Clock frequency
 */
#ifndef CONF_UART4_FREQUENCY
#define CONF_UART4_FREQUENCY 150000000
#endif

// <h> USART Clock Settings
// <o> USART Clock source

// <0=> Master Clock (MCK)
// <1=> MCK / 8 for USART
// <2=> Programmable Clock Controller 4 (PMC_PCK4)
// <3=> External Clock
// <i> This defines the clock source for the USART
// <id> usart_clock_source
#ifndef CONF_USART0_CK_SRC
#define CONF_USART0_CK_SRC 0
#endif

// <o> USART External Clock Input on SCK <1-4294967295>
// <i> Inputs the external clock frequency on SCK
// <id> usart_clock_freq
#ifndef CONF_USART0_SCK_FREQ
#define CONF_USART0_SCK_FREQ 10000000
#endif

// </h>

/**
 * \def USART FREQUENCY
 * \brief USART's Clock frequency
 */
#ifndef CONF_USART0_FREQUENCY
#define CONF_USART0_FREQUENCY 150000000
#endif

// <h> GMAC Clock Settings
// <y> GMAC Clock source
// <CONF_SRC_MCK"> Master Clock (MCK)
// <i> Select the clock source for GMAC
// <id> gmac_clock_source
#ifndef CONF_GMAC_SRC
#define CONF_GMAC_SRC CONF_SRC_MCK
#endif
// </h>

/**
 * \def GMAC FREQUENCY
 * \brief GMAC Clock frequency
 */
#ifndef CONF_GMAC_FREQUENCY
#define CONF_GMAC_FREQUENCY 150000000
#endif

// <<< end of configuration section >>>

#endif // PERIPHERAL_CLK_CONFIG_H
