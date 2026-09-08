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
set(CMAKE_PROJECT_NAME gryphon-hvps-interface-fw)

# Enable compile command to ease indexing with e.g. clangd
set(CMAKE_EXPORT_COMPILE_COMMANDS TRUE)

# Core project settings
project(${CMAKE_PROJECT_NAME})
message("Build type: " ${CMAKE_BUILD_TYPE})

get_filename_component(ARM_TOOLCHAIN_BIN "${CMAKE_C_COMPILER}" DIRECTORY)
set(FW_OBJCOPY "${ARM_TOOLCHAIN_BIN}/arm-none-eabi-objcopy${CMAKE_EXECUTABLE_SUFFIX}")

find_package(Python3 QUIET COMPONENTS Interpreter)

add_subdirectory(
    "${CMAKE_CURRENT_LIST_DIR}/../../mcuboot"
    "${CMAKE_BINARY_DIR}/gentlebeam-mcuboot"
)

if(NOT DEFINED FW_VERSION)
    if(Python3_Interpreter_FOUND)
        execute_process(
            COMMAND ${Python3_EXECUTABLE} ${CMAKE_CURRENT_LIST_DIR}/../../tools/resolve_dcdoc_version.py
                --component hvps-interface/main
            OUTPUT_VARIABLE FW_VERSION
            OUTPUT_STRIP_TRAILING_WHITESPACE
            COMMAND_ERROR_IS_FATAL ANY
        )
    else()
        set(FW_VERSION "0.0.0-local.0")
    endif()
endif()

option(CALIBRATION_MODE "Enable calibration mode" OFF)

# Enable CMake support for ASM and C languages
enable_language(C ASM)

# Create an executable object type
add_executable(${CMAKE_PROJECT_NAME})

# Add STM32CubeMX generated sources
add_subdirectory(cmake/stm32cubemx)

# Link directories setup
target_link_directories(${CMAKE_PROJECT_NAME} PRIVATE
    # Add user defined library search paths
)

file(GLOB_RECURSE SENSUS_SRC CONFIGURE_DEPENDS 
    "Sensus Src/*.c" 
)

# Add sources to executable
target_sources(${CMAKE_PROJECT_NAME} PRIVATE
    # Add user sources here
    ${SENSUS_SRC}
)

# Add include paths
target_include_directories(${CMAKE_PROJECT_NAME} PRIVATE
    # Add user defined include paths
)

# Add project symbols (macros)
target_compile_definitions(${CMAKE_PROJECT_NAME} PRIVATE
    "FW_VERSION=\"${FW_VERSION}\""
    $<$<BOOL:${CALIBRATION_MODE}>:CALIBRATION_MODE=1>
)

# Remove wrong libob.a library dependency when using cpp files
list(REMOVE_ITEM CMAKE_C_IMPLICIT_LINK_LIBRARIES ob)

# Add linked libraries
target_link_libraries(${CMAKE_PROJECT_NAME}
    stm32cubemx

    # Add user defined libraries
)

add_custom_command(TARGET ${CMAKE_PROJECT_NAME} POST_BUILD
    COMMAND "${FW_OBJCOPY}"
        -O ihex
        $<TARGET_FILE:${CMAKE_PROJECT_NAME}>
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.hex
    BYPRODUCTS ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}.hex
    VERBATIM
)

if(Python3_Interpreter_FOUND)
set(_hvps_bootloader_build_type ${CMAKE_BUILD_TYPE})
if(CMAKE_BUILD_TYPE STREQUAL "CalibrationDebug")
    set(_hvps_bootloader_build_type Debug)
endif()
set(_hvps_bootloader_binary_dir
    ${CMAKE_CURRENT_LIST_DIR}/../bootloader/build/${_hvps_bootloader_build_type})
set(_hvps_mcuboot_imgtool ${GENTLEBEAM_MCUBOOT_IMGTOOL})
set(_hvps_mcuboot_signing_key ${CMAKE_CURRENT_LIST_DIR}/../bootloader/keys/signing-key.pem)
string(REGEX MATCH "^[0-9]+\\.[0-9]+\\.[0-9]+" _hvps_mcuboot_version "${FW_VERSION}")
if(NOT _hvps_mcuboot_version)
    set(_hvps_mcuboot_version "0.0.0")
endif()

add_custom_target(hvps_interface_combined_hex ALL
    COMMAND ${CMAKE_COMMAND}
        -S ${CMAKE_CURRENT_LIST_DIR}/../bootloader
        -B ${_hvps_bootloader_binary_dir}
        -G ${CMAKE_GENERATOR}
        -DCMAKE_MAKE_PROGRAM=${CMAKE_MAKE_PROGRAM}
        -DCMAKE_BUILD_TYPE=${_hvps_bootloader_build_type}
        -DCMAKE_TOOLCHAIN_FILE=${CMAKE_CURRENT_LIST_DIR}/../bootloader/cmake/gcc-arm-none-eabi.cmake
    COMMAND ${CMAKE_COMMAND} --build ${_hvps_bootloader_binary_dir}
    COMMAND ${Python3_EXECUTABLE} ${_hvps_mcuboot_imgtool} sign
        -S 0x2D000
        -H 0x200
        --pad-header
        --align 4
        -v ${_hvps_mcuboot_version}
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.hex
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.unsigned.bin
    COMMAND ${Python3_EXECUTABLE} ${_hvps_mcuboot_imgtool} sign
        -S 0x2D000
        -H 0x200
        --pad-header
        --align 4
        -v ${_hvps_mcuboot_version}
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.hex
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.unsigned.hex
    COMMAND ${Python3_EXECUTABLE} ${_hvps_mcuboot_imgtool} sign
        -k ${_hvps_mcuboot_signing_key}
        -S 0x2D000
        -H 0x200
        --pad-header
        --align 4
        -v ${_hvps_mcuboot_version}
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.hex
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.signed.bin
    COMMAND ${Python3_EXECUTABLE} ${_hvps_mcuboot_imgtool} sign
        -k ${_hvps_mcuboot_signing_key}
        -S 0x2D000
        -H 0x200
        --pad-header
        --align 4
        -v ${_hvps_mcuboot_version}
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.hex
        $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.signed.hex
    COMMAND ${Python3_EXECUTABLE} ${CMAKE_CURRENT_LIST_DIR}/../../tools/firmware_hex.py combine
        --input ${_hvps_bootloader_binary_dir}/hvps-interface-bootloader.hex
        --input $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.signed.hex
        --output $<TARGET_FILE_DIR:${CMAKE_PROJECT_NAME}>/${CMAKE_PROJECT_NAME}.combined.hex
    DEPENDS
        ${CMAKE_PROJECT_NAME}
        ${CMAKE_CURRENT_LIST_DIR}/../../tools/firmware_hex.py
        ${_hvps_mcuboot_imgtool}
        ${_hvps_mcuboot_signing_key}
    BYPRODUCTS
        ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}.unsigned.bin
        ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}.unsigned.hex
        ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}.signed.bin
        ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}.signed.hex
        ${CMAKE_CURRENT_BINARY_DIR}/${CMAKE_PROJECT_NAME}.combined.hex
    VERBATIM
)
endif()
