cmake_minimum_required(VERSION 3.22)

#
# This file is generated only once,
# and is not re-generated if converter is called multiple times.
#
# User is free to modify the file as much as necessary
#

# Setup compiler settings
set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS ON)


# Define the build type
if(NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE "Debug")
endif()

# Set the project name
set(CMAKE_PROJECT_NAME hvps-interface-bootloader)

# Enable compile command to ease indexing with e.g. clangd
set(CMAKE_EXPORT_COMPILE_COMMANDS TRUE)

# Core project settings
project(${CMAKE_PROJECT_NAME})
message("Build type: " ${CMAKE_BUILD_TYPE})

get_filename_component(ARM_TOOLCHAIN_BIN "${CMAKE_C_COMPILER}" DIRECTORY)
set(MCUBOOT_OBJCOPY "${ARM_TOOLCHAIN_BIN}/arm-none-eabi-objcopy${CMAKE_EXECUTABLE_SUFFIX}")

add_subdirectory(
    "${CMAKE_CURRENT_LIST_DIR}/../../mcuboot"
    "${CMAKE_BINARY_DIR}/gentlebeam-mcuboot"
)

add_library(bootutil STATIC)
target_include_directories(bootutil PUBLIC
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot/include"
)
target_sources(bootutil PRIVATE
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/boot/bootutil/src/boot_record.c"
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/boot/bootutil/src/swap_move.c"
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot/src/flash_map_backend.c"
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot/src/mcuboot_uart.c"
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot/src/signing_key.c"
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot/src/base64.c"
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/ext/tinycrypt/lib/source/ecc_platform_specific.c"
)

# Enable CMake support for ASM and C languages
enable_language(C ASM)

# Create an executable object type
add_executable(${CMAKE_PROJECT_NAME})

# Add STM32CubeMX generated sources
add_subdirectory(cmake/stm32cubemx)

if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    target_compile_options(bootutil PRIVATE -Os -g3)
    target_compile_options(${CMAKE_PROJECT_NAME} PRIVATE -Os -g3)
    target_compile_options(STM32_Drivers PRIVATE -Os -g3)
endif()

# Link directories setup
target_link_directories(${CMAKE_PROJECT_NAME} PRIVATE
    # Add user defined library search paths
)

# Add sources to executable
target_sources(${CMAKE_PROJECT_NAME} PRIVATE
)

# Add include paths
target_include_directories(${CMAKE_PROJECT_NAME} PRIVATE
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot/include"
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/boot/bootutil/include"
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/boot/boot_serial/include"
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/boot/boot_serial/src"
    "${GENTLEBEAM_MCUBOOT_SOURCE_DIR}/boot/zcbor/include"
)

# Add project symbols (macros)
target_compile_definitions(${CMAKE_PROJECT_NAME} PRIVATE
    # Add user defined symbols

)

# Remove wrong libob.a library dependency when using cpp files
list(REMOVE_ITEM CMAKE_C_IMPLICIT_LINK_LIBRARIES ob)

# Add linked libraries
target_link_libraries(${CMAKE_PROJECT_NAME}
    stm32cubemx
    bootutil

    # Add user defined libraries
)

# Generate HEX
add_custom_command(TARGET ${CMAKE_PROJECT_NAME} POST_BUILD
    COMMAND "${MCUBOOT_OBJCOPY}"
        -O ihex
        $<TARGET_FILE:${CMAKE_PROJECT_NAME}>
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.hex
    VERBATIM
)
target_link_libraries(bootutil
    PRIVATE gentlebeam::mcuboot
    PUBLIC stm32cubemx
)
