cmake_minimum_required(VERSION 3.19)

if(CMAKE_CONFIGURATION_TYPES)
  message(FATAL_ERROR "Only single-config CMake generators are supported.")
endif()

project(gentlebeam_same70_firmware C)

option(BENCH_DEBUG "Enable bench-debug firmware behavior" OFF)

find_package(Python3 REQUIRED COMPONENTS Interpreter)

add_subdirectory(
  "${CMAKE_CURRENT_SOURCE_DIR}/../mcuboot"
  "${CMAKE_BINARY_DIR}/gentlebeam-mcuboot"
)

execute_process(
  COMMAND "${Python3_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/../tools/resolve_dcdoc_version.py"
          --component main-control/main
  OUTPUT_VARIABLE GENTLEBEAM_VERSION
  OUTPUT_STRIP_TRAILING_WHITESPACE
  COMMAND_ERROR_IS_FATAL ANY
)

set(_gentlebeam_build_types Debug Release CalibrationDebug)
if(NOT CMAKE_BUILD_TYPE IN_LIST _gentlebeam_build_types)
  message(FATAL_ERROR "CMAKE_BUILD_TYPE must be Debug, Release, or CalibrationDebug; got '${CMAKE_BUILD_TYPE}'.")
endif()

add_subdirectory(main)
add_subdirectory(bootloader)

add_custom_command(
  OUTPUT "${CMAKE_BINARY_DIR}/main/Gryphon Control FW.combined.hex"
  COMMAND "${Python3_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/../tools/firmware_hex.py" combine
          --input "${CMAKE_BINARY_DIR}/bootloader/Gryphon System Bootloader FW.hex"
      --input "${CMAKE_BINARY_DIR}/main/Gryphon Control FW.mcumgr.hex"
          --output "${CMAKE_BINARY_DIR}/main/Gryphon Control FW.combined.hex"
  DEPENDS gryphon_system_bootloader_fw gryphon_control_fw
          "${CMAKE_SOURCE_DIR}/../tools/firmware_hex.py"
  VERBATIM
)
add_custom_target(gryphon_control_combined_hex ALL
  DEPENDS "${CMAKE_BINARY_DIR}/main/Gryphon Control FW.combined.hex"
)
