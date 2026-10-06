enable_language(CXX)
# Keep the target firmware's SysTick implementation unchanged. On the host,
# sys_now must advance even while firmware is busy-waiting outside the tick loop.
set_property(SOURCE "${CMAKE_CURRENT_SOURCE_DIR}/../main.c"
    TARGET_DIRECTORY gryphon_control_lib APPEND PROPERTY
    COMPILE_DEFINITIONS sys_now=host_systick_now)
add_executable(gryphon_control_host
    main.cpp
    hardware_stubs.c
    head_board_backend.c
    hvps_backend.cpp
    network_stubs.c
    ext_timers_i2c.c
    ext_adcs_i2c.c
    ext_dac_spi.c
    uart_backend.cpp
)
target_include_directories(gryphon_control_host PRIVATE vendor vendor/CLI11)
target_compile_features(gryphon_control_host PRIVATE cxx_std_17)
target_link_libraries(gryphon_control_host PRIVATE gryphon_control_lib)
if(WIN32)
    target_link_libraries(gryphon_control_host PRIVATE ws2_32)
endif()
if(NOT MSVC)
    target_link_libraries(gryphon_control_host PRIVATE m)
endif()