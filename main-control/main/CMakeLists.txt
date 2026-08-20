file(GLOB main_startup_sources CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/Device_Startup/*.c")
file(GLOB main_sensus_sources CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/sensus_src/*.c")

set(main_sources
  atmel_start.c
  diskio/sdmmc_diskio.c
  diskio_start.c
  driver_init.c
  ethernet_phy/ethernet_phy.c
  ethernet_phy_main.c
  eth_ipstack_main.c
  examples/driver_examples.c
  fatfs/src/ff.c
  fatfs_main.c
  hal/src/hal_atomic.c
  hal/src/hal_calendar.c
  hal/src/hal_delay.c
  hal/src/hal_flash.c
  hal/src/hal_gpio.c
  hal/src/hal_i2c_m_sync.c
  hal/src/hal_init.c
  hal/src/hal_io.c
  hal/src/hal_mac_async.c
  hal/src/hal_mci_sync.c
  hal/src/hal_sleep.c
  hal/src/hal_spi_m_async.c
  hal/src/hal_timer.c
  hal/src/hal_usart_async.c
  hal/utils/src/utils_assert.c
  hal/utils/src/utils_event.c
  hal/utils/src/utils_list.c
  hal/utils/src/utils_ringbuffer.c
  hal/utils/src/utils_syscalls.c
  hpl/core/hpl_core_m7_base.c
  hpl/core/hpl_init.c
  hpl/efc/hpl_efc.c
  hpl/gmac/hpl_gmac.c
  hpl/hsmci/hpl_hsmci.c
  hpl/pmc/hpl_pmc.c
  hpl/pmc/hpl_sleep.c
  hpl/rtc/hpl_rtc.c
  hpl/spi/hpl_spi.c
  hpl/tc/hpl_tc.c
  hpl/tc/tc_lite.c
  hpl/twihs/hpl_twihs.c
  hpl/uart/hpl_uart.c
  hpl/usart/hpl_usart.c
  hpl/xdmac/hpl_xdmac.c
  lwip/lwip-1.4.0/port/ethif_mac.c
  lwip/lwip-1.4.0/src/api/api_lib.c
  lwip/lwip-1.4.0/src/api/api_msg.c
  lwip/lwip-1.4.0/src/api/err.c
  lwip/lwip-1.4.0/src/api/netbuf.c
  lwip/lwip-1.4.0/src/api/netdb.c
  lwip/lwip-1.4.0/src/api/netifapi.c
  lwip/lwip-1.4.0/src/api/sockets.c
  lwip/lwip-1.4.0/src/api/tcpip.c
  lwip/lwip-1.4.0/src/core/def.c
  lwip/lwip-1.4.0/src/core/dhcp.c
  lwip/lwip-1.4.0/src/core/dns.c
  lwip/lwip-1.4.0/src/core/init.c
  lwip/lwip-1.4.0/src/core/ipv4/autoip.c
  lwip/lwip-1.4.0/src/core/ipv4/icmp.c
  lwip/lwip-1.4.0/src/core/ipv4/igmp.c
  lwip/lwip-1.4.0/src/core/ipv4/inet.c
  lwip/lwip-1.4.0/src/core/ipv4/inet_chksum.c
  lwip/lwip-1.4.0/src/core/ipv4/ip.c
  lwip/lwip-1.4.0/src/core/ipv4/ip_addr.c
  lwip/lwip-1.4.0/src/core/ipv4/ip_frag.c
  lwip/lwip-1.4.0/src/core/mem.c
  lwip/lwip-1.4.0/src/core/memp.c
  lwip/lwip-1.4.0/src/core/netif.c
  lwip/lwip-1.4.0/src/core/pbuf.c
  lwip/lwip-1.4.0/src/core/raw.c
  lwip/lwip-1.4.0/src/core/snmp/asn1_dec.c
  lwip/lwip-1.4.0/src/core/snmp/asn1_enc.c
  lwip/lwip-1.4.0/src/core/snmp/mib2.c
  lwip/lwip-1.4.0/src/core/snmp/mib_structs.c
  lwip/lwip-1.4.0/src/core/snmp/msg_in.c
  lwip/lwip-1.4.0/src/core/snmp/msg_out.c
  lwip/lwip-1.4.0/src/core/stats.c
  lwip/lwip-1.4.0/src/core/sys.c
  lwip/lwip-1.4.0/src/core/tcp.c
  lwip/lwip-1.4.0/src/core/tcp_in.c
  lwip/lwip-1.4.0/src/core/tcp_out.c
  lwip/lwip-1.4.0/src/core/timers.c
  lwip/lwip-1.4.0/src/core/udp.c
  lwip/lwip-1.4.0/src/netif/etharp.c
  lwip/lwip-1.4.0/src/netif/ppp/auth.c
  lwip/lwip-1.4.0/src/netif/ppp/chap.c
  lwip/lwip-1.4.0/src/netif/ppp/chpms.c
  lwip/lwip-1.4.0/src/netif/ppp/fsm.c
  lwip/lwip-1.4.0/src/netif/ppp/ipcp.c
  lwip/lwip-1.4.0/src/netif/ppp/lcp.c
  lwip/lwip-1.4.0/src/netif/ppp/magic.c
  lwip/lwip-1.4.0/src/netif/ppp/md5.c
  lwip/lwip-1.4.0/src/netif/ppp/pap.c
  lwip/lwip-1.4.0/src/netif/ppp/ppp.c
  lwip/lwip-1.4.0/src/netif/ppp/ppp_oe.c
  lwip/lwip-1.4.0/src/netif/ppp/randm.c
  lwip/lwip-1.4.0/src/netif/ppp/vj.c
  lwip/lwip-1.4.0/src/netif/slipif.c
  main.c
  sd_mmc/sd_mmc.c
  sd_mmc_start.c
)

add_executable(gryphon_control_fw
  ${main_sources}
  ${main_startup_sources}
  ${main_sensus_sources}
)

set_target_properties(gryphon_control_fw PROPERTIES
  OUTPUT_NAME "Gryphon Control FW"
  SUFFIX ".elf"
)

target_include_directories(gryphon_control_fw PRIVATE
  "${MICROCHIP_STUDIO_CMSIS_ROOT}/CMSIS/Core/Include"
  "${MICROCHIP_STUDIO_SAME70_DFP_ROOT}/same70b/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/Config"
  "${CMAKE_CURRENT_SOURCE_DIR}"
  "${CMAKE_CURRENT_SOURCE_DIR}/examples"
  "${CMAKE_CURRENT_SOURCE_DIR}/hal/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/hal/utils/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/core"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/gmac"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/hsmci"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/nvmctrl"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/pio"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/pmc"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/rtc"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/spi"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/tc"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/twihs"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/uart"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/usart"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/xdmac"
  "${CMAKE_CURRENT_SOURCE_DIR}/hri"
  "${CMAKE_CURRENT_SOURCE_DIR}/diskio"
  "${CMAKE_CURRENT_SOURCE_DIR}/lwip/lwip-1.4.0/port"
  "${CMAKE_CURRENT_SOURCE_DIR}/lwip/lwip-1.4.0/port/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/lwip/lwip-1.4.0/src/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/lwip/lwip-1.4.0/src/include/ipv4"
  "${CMAKE_CURRENT_SOURCE_DIR}/ethernet_phy"
  "${CMAKE_CURRENT_SOURCE_DIR}/fatfs/src"
  "${CMAKE_CURRENT_SOURCE_DIR}/sd_mmc"
)

target_compile_definitions(gryphon_control_fw PRIVATE
  __SAME70Q21B__
  "FW_VERSION=\"${GENTLEBEAM_VERSION}\""
  "$<$<CONFIG:Release>:NDEBUG>"
  "$<$<CONFIG:Debug,CalibrationDebug>:DEBUG>"
  "$<$<CONFIG:Debug,CalibrationDebug>:ARM_MATH_CM7>"
  "$<$<CONFIG:CalibrationDebug>:CALIBRATION_MODE>"
)

target_compile_options(gryphon_control_fw PRIVATE
  -mthumb -mcpu=cortex-m7 -mlong-calls -std=gnu99 -Wall
  -ffunction-sections -fdata-sections
  "$<$<CONFIG:Release>:-Os>"
  "$<$<CONFIG:Release>:-mfloat-abi=softfp>"
  "$<$<CONFIG:Release>:-mfpu=fpv4-sp-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-O1>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-g3>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfloat-abi=softfp>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfpu=fpv5-d16>"
)

target_link_options(gryphon_control_fw PRIVATE
  -mthumb -mcpu=cortex-m7 --specs=nano.specs -Wl,--gc-sections
  "-T${CMAKE_CURRENT_SOURCE_DIR}/Device_Startup/same70q21b_flash.ld"
  "$<$<CONFIG:Release>:-mfloat-abi=softfp>"
  "$<$<CONFIG:Release>:-mfpu=fpv4-sp-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfloat-abi=softfp>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfpu=fpv5-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-u>"
  "$<$<CONFIG:Debug,CalibrationDebug>:_printf_float>"
)
target_link_libraries(gryphon_control_fw PRIVATE c m)

add_custom_command(TARGET gryphon_control_fw POST_BUILD
  COMMAND "${CMAKE_OBJCOPY}" -O ihex -R .eeprom -R .fuse -R .lock -R .signature
          "$<TARGET_FILE:gryphon_control_fw>"
          "$<TARGET_FILE_DIR:gryphon_control_fw>/Gryphon Control FW.hex"
  COMMAND "${Python3_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/../tools/firmware_hex.py" inject-crc
          --input "$<TARGET_FILE_DIR:gryphon_control_fw>/Gryphon Control FW.hex"
          --output "$<TARGET_FILE_DIR:gryphon_control_fw>/Gryphon Control FW.with-crc.hex"
          --crc-output "$<TARGET_FILE_DIR:gryphon_control_fw>/Gryphon Control FW.hex.crc"
          --application-start 0x00420000
          --application-length 0x000E0000
          --crc-address 0x0041E000
  BYPRODUCTS
    "${CMAKE_CURRENT_BINARY_DIR}/Gryphon Control FW.hex"
    "${CMAKE_CURRENT_BINARY_DIR}/Gryphon Control FW.with-crc.hex"
    "${CMAKE_CURRENT_BINARY_DIR}/Gryphon Control FW.hex.crc"
  VERBATIM
)
