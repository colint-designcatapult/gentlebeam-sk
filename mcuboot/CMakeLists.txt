cmake_minimum_required(VERSION 3.19)

# Project-owned integration wrapper around the unmodified MCUboot sources.
set(GENTLEBEAM_MCUBOOT_SOURCE_DIR
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot-2.4.0"
    PARENT_SCOPE
)
set(GENTLEBEAM_MCUBOOT_IMGTOOL
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot-2.4.0/scripts/imgtool.py"
    PARENT_SCOPE
)
set(GENTLEBEAM_MCUBOOT_REQUIREMENTS
    "${CMAKE_CURRENT_LIST_DIR}/mcuboot-2.4.0/scripts/requirements.txt"
    PARENT_SCOPE
)

add_library(gentlebeam_mcuboot INTERFACE)
add_library(gentlebeam::mcuboot ALIAS gentlebeam_mcuboot)

set(_gentlebeam_mcuboot_source_dir "${CMAKE_CURRENT_LIST_DIR}/mcuboot-2.4.0")

target_include_directories(gentlebeam_mcuboot INTERFACE
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/include"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src"
    "${_gentlebeam_mcuboot_source_dir}/boot/boot_serial/include"
    "${_gentlebeam_mcuboot_source_dir}/boot/boot_serial/src"
    "${_gentlebeam_mcuboot_source_dir}/boot/zcbor/include"
    "${_gentlebeam_mcuboot_source_dir}/ext/tinycrypt/lib/include"
    "${_gentlebeam_mcuboot_source_dir}/ext/mbedtls-asn1/include"
)

target_sources(gentlebeam_mcuboot INTERFACE
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_area.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_find_key.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_img_hash.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_img_security_cnt.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_loader.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_misc.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/bootutil_public.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/caps.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/fault_injection_hardening.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/image_ecdsa.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/image_validate.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/loader.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/swap_misc.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/swap_scratch.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/bootutil/src/tlv.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/boot_serial/src/boot_serial.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/boot_serial/src/zcbor_bulk.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/zcbor/src/zcbor_common.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/zcbor/src/zcbor_decode.c"
    "${_gentlebeam_mcuboot_source_dir}/boot/zcbor/src/zcbor_encode.c"
    "${_gentlebeam_mcuboot_source_dir}/ext/tinycrypt/lib/source/ecc.c"
    "${_gentlebeam_mcuboot_source_dir}/ext/tinycrypt/lib/source/ecc_dsa.c"
    "${_gentlebeam_mcuboot_source_dir}/ext/tinycrypt/lib/source/sha256.c"
    "${_gentlebeam_mcuboot_source_dir}/ext/tinycrypt/lib/source/utils.c"
    "${_gentlebeam_mcuboot_source_dir}/ext/mbedtls-asn1/src/asn1parse.c"
    "${_gentlebeam_mcuboot_source_dir}/ext/mbedtls-asn1/src/platform_util.c"
)
