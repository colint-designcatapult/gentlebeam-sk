file(GLOB bootloader_startup_sources CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/Device_Startup/*.c")

set(bootloader_sources
  atmel_start.c
  driver_init.c
  examples/driver_examples.c
  hal/src/hal_atomic.c
  hal/src/hal_delay.c
  hal/src/hal_flash.c
  hal/src/hal_gpio.c
  hal/src/hal_init.c
  hal/src/hal_io.c
  hal/src/hal_sleep.c
  hal/src/hal_usart_async.c
  hal/utils/src/utils_assert.c
  hal/utils/src/utils_event.c
  hal/utils/src/utils_list.c
  hal/utils/src/utils_ringbuffer.c
  hal/utils/src/utils_syscalls.c
  hpl/core/hpl_core_m7_base.c
  hpl/core/hpl_init.c
  hpl/efc/hpl_efc.c
  hpl/pmc/hpl_pmc.c
  hpl/pmc/hpl_sleep.c
  hpl/usart/hpl_usart.c
  hpl/xdmac/hpl_xdmac.c
  main.c
  mcuboot_port/base64/base64.c
  mcuboot_port/boot_uart.c
  mcuboot_port/crc/crc16.c
  mcuboot_port/flash_map_backend.c
  mcuboot_port/hal/hal_system.c
  mcuboot_port/newlib_syscalls.c
  mcuboot_port/os/os_cputime.c
  mcuboot_port/os/os_malloc.c
  mcuboot_port/signing_keys.c
  keys/gryphon-dev-ec256-pub.c
)

add_executable(gryphon_system_bootloader_fw
  ${bootloader_sources}
  ${bootloader_startup_sources}
)

set_target_properties(gryphon_system_bootloader_fw PROPERTIES
  OUTPUT_NAME "Gryphon System Bootloader FW"
  SUFFIX ".elf"
  LINK_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/Device_Startup/same70q21b_flash.ld"
)

target_include_directories(gryphon_system_bootloader_fw PRIVATE
  "${MICROCHIP_STUDIO_CMSIS_ROOT}/CMSIS/Core/Include"
  "${MICROCHIP_STUDIO_SAME70_DFP_ROOT}/same70b/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/Config"
  "${CMAKE_CURRENT_SOURCE_DIR}"
  "${CMAKE_CURRENT_SOURCE_DIR}/examples"
  "${CMAKE_CURRENT_SOURCE_DIR}/hal/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/hal/utils/include"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/core"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/nvmctrl"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/pio"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/pmc"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/usart"
  "${CMAKE_CURRENT_SOURCE_DIR}/hpl/xdmac"
  "${CMAKE_CURRENT_SOURCE_DIR}/hri"
  "${CMAKE_CURRENT_SOURCE_DIR}/mcuboot_port"
)

target_compile_definitions(gryphon_system_bootloader_fw PRIVATE
  __SAME70Q21B__
  $<$<CONFIG:Release>:NDEBUG>
  $<$<CONFIG:Debug,CalibrationDebug>:DEBUG>
)

target_compile_options(gryphon_system_bootloader_fw PRIVATE
  -mthumb -mcpu=cortex-m7 -mlong-calls -std=gnu99 -Wall
  -ffunction-sections -fdata-sections
  "$<$<CONFIG:Release>:-Os>"
  "$<$<CONFIG:Release>:-mfloat-abi=softfp>"
  "$<$<CONFIG:Release>:-mfpu=fpv4-sp-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-O1>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-g3>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfloat-abi=hard>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfpu=fpv5-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfp16-format=ieee>"
)

target_link_options(gryphon_system_bootloader_fw PRIVATE
  -mthumb -mcpu=cortex-m7 --specs=nano.specs -Wl,--gc-sections
  "-T${CMAKE_CURRENT_SOURCE_DIR}/Device_Startup/same70q21b_flash.ld"
  "$<$<CONFIG:Release>:-mfloat-abi=softfp>"
  "$<$<CONFIG:Release>:-mfpu=fpv4-sp-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfloat-abi=hard>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfpu=fpv5-d16>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-mfp16-format=ieee>"
  "$<$<CONFIG:Debug,CalibrationDebug>:-u>"
  "$<$<CONFIG:Debug,CalibrationDebug>:_printf_float>"
)
target_link_libraries(gryphon_system_bootloader_fw PRIVATE
  gentlebeam::mcuboot
  c
  m
)

add_custom_command(TARGET gryphon_system_bootloader_fw POST_BUILD
  COMMAND "${CMAKE_OBJCOPY}" -O ihex -R .eeprom -R .fuse -R .lock -R .signature
          "$<TARGET_FILE:gryphon_system_bootloader_fw>"
          "$<TARGET_FILE_DIR:gryphon_system_bootloader_fw>/Gryphon System Bootloader FW.hex"
  BYPRODUCTS "${CMAKE_CURRENT_BINARY_DIR}/Gryphon System Bootloader FW.hex"
  VERBATIM
)

add_custom_command(TARGET gryphon_system_bootloader_fw POST_BUILD
  COMMAND "${Python3_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/../tools/print_size.py"
          --size-tool "${CMAKE_SIZE}"
          --elf "$<TARGET_FILE:gryphon_system_bootloader_fw>"
          --flash-size 204800
          --ram-size 393216
          --name gryphon_system_bootloader_fw
  VERBATIM
)
