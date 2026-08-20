set(CMAKE_SYSTEM_NAME Generic)
set(CMAKE_SYSTEM_PROCESSOR cortex-m7)
set(CMAKE_TRY_COMPILE_TARGET_TYPE STATIC_LIBRARY)

set(_microchip_studio_arm_gcc_bin
    "$ENV{GENTLEBEAM_ARM_GCC_BIN}")
if(NOT _microchip_studio_arm_gcc_bin)
  set(_microchip_studio_arm_gcc_bin
      "C:/Program Files (x86)/Atmel/Studio/7.0/toolchain/arm/arm-gnu-toolchain/bin")
endif()

set(_microchip_studio_cmsis_root "$ENV{GENTLEBEAM_CMSIS_ROOT}")
if(NOT _microchip_studio_cmsis_root)
  set(_microchip_studio_cmsis_root
      "C:/Program Files (x86)/Atmel/Studio/7.0/packs/arm/CMSIS/5.4.0")
endif()

set(_microchip_studio_same70_dfp_root "$ENV{GENTLEBEAM_SAME70_DFP_ROOT}")
if(NOT _microchip_studio_same70_dfp_root)
  set(_microchip_studio_same70_dfp_root
      "C:/Program Files (x86)/Atmel/Studio/7.0/packs/atmel/SAME70_DFP/2.4.166")
endif()

set(_microchip_studio_c_compiler "${_microchip_studio_arm_gcc_bin}/arm-none-eabi-gcc.exe")
set(_microchip_studio_objcopy "${_microchip_studio_arm_gcc_bin}/arm-none-eabi-objcopy.exe")
set(_microchip_studio_objdump "${_microchip_studio_arm_gcc_bin}/arm-none-eabi-objdump.exe")
set(_microchip_studio_size "${_microchip_studio_arm_gcc_bin}/arm-none-eabi-size.exe")
set(_microchip_studio_cmsis_include "${_microchip_studio_cmsis_root}/CMSIS/Core/Include")
set(_microchip_studio_same70_include "${_microchip_studio_same70_dfp_root}/same70b/include")

foreach(_path IN ITEMS
    "${_microchip_studio_c_compiler}"
    "${_microchip_studio_objcopy}"
    "${_microchip_studio_objdump}"
    "${_microchip_studio_size}"
    "${_microchip_studio_cmsis_include}"
    "${_microchip_studio_same70_include}")
  if(NOT EXISTS "${_path}")
    message(FATAL_ERROR "Required Microchip Studio 7 ARM GCC 6.3.1 toolchain or pack path is missing: ${_path}")
  endif()
endforeach()

set(CMAKE_C_COMPILER "${_microchip_studio_c_compiler}" CACHE FILEPATH "" FORCE)
set(CMAKE_OBJCOPY "${_microchip_studio_objcopy}" CACHE FILEPATH "" FORCE)
set(CMAKE_OBJDUMP "${_microchip_studio_objdump}" CACHE FILEPATH "" FORCE)
set(CMAKE_SIZE "${_microchip_studio_size}" CACHE FILEPATH "" FORCE)
set(MICROCHIP_STUDIO_CMSIS_ROOT "${_microchip_studio_cmsis_root}" CACHE PATH "" FORCE)
set(MICROCHIP_STUDIO_SAME70_DFP_ROOT "${_microchip_studio_same70_dfp_root}" CACHE PATH "" FORCE)
