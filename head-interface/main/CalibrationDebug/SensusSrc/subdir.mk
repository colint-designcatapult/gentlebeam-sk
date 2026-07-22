################################################################################
# Automatically-generated file. Do not edit!
# Toolchain: GNU Tools for STM32 (14.3.rel1)
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../SensusSrc/1wire_ds2482.c \
../SensusSrc/adc.c \
../SensusSrc/buttons.c \
../SensusSrc/collimator.c \
../SensusSrc/control_comm.c \
../SensusSrc/crcccitt.c \
../SensusSrc/flow.c \
../SensusSrc/leds.c \
../SensusSrc/lis2mdl.c \
../SensusSrc/lis3mdl.c \
../SensusSrc/magnetometer.c \
../SensusSrc/qc.c \
../SensusSrc/setup.c \
../SensusSrc/sys_data.c \
../SensusSrc/timer.c 

OBJS += \
./SensusSrc/1wire_ds2482.o \
./SensusSrc/adc.o \
./SensusSrc/buttons.o \
./SensusSrc/collimator.o \
./SensusSrc/control_comm.o \
./SensusSrc/crcccitt.o \
./SensusSrc/flow.o \
./SensusSrc/leds.o \
./SensusSrc/lis2mdl.o \
./SensusSrc/lis3mdl.o \
./SensusSrc/magnetometer.o \
./SensusSrc/qc.o \
./SensusSrc/setup.o \
./SensusSrc/sys_data.o \
./SensusSrc/timer.o 

C_DEPS += \
./SensusSrc/1wire_ds2482.d \
./SensusSrc/adc.d \
./SensusSrc/buttons.d \
./SensusSrc/collimator.d \
./SensusSrc/control_comm.d \
./SensusSrc/crcccitt.d \
./SensusSrc/flow.d \
./SensusSrc/leds.d \
./SensusSrc/lis2mdl.d \
./SensusSrc/lis3mdl.d \
./SensusSrc/magnetometer.d \
./SensusSrc/qc.d \
./SensusSrc/setup.d \
./SensusSrc/sys_data.d \
./SensusSrc/timer.d 


# Each subdirectory must supply rules for building sources it contributes
SensusSrc/%.o SensusSrc/%.su SensusSrc/%.cyclo: ../SensusSrc/%.c SensusSrc/subdir.mk
	arm-none-eabi-gcc -c "$<" -mcpu=cortex-m4 -std=gnu11 -g3 '-D__weak=__attribute__((weak))' '-D__packed=__attribute__((__packed__))' -DUSE_HAL_DRIVER -DSTM32F411xE -DCALIBRATION_MODE -c -I../Inc -I../Drivers/STM32F4xx_HAL_Driver/Inc -I../Drivers/STM32F4xx_HAL_Driver/Inc/Legacy -I../Drivers/CMSIS/Device/ST/STM32F4xx/Include -I../Drivers/CMSIS/Include -Os -ffunction-sections -fdata-sections -Wall -fstack-usage -fcyclomatic-complexity -MMD -MP -MF"$(@:%.o=%.d)" -MT"$@" --specs=nano.specs -mfpu=fpv4-sp-d16 -mfloat-abi=hard -mthumb -o "$@"

clean: clean-SensusSrc

clean-SensusSrc:
	-$(RM) ./SensusSrc/1wire_ds2482.cyclo ./SensusSrc/1wire_ds2482.d ./SensusSrc/1wire_ds2482.o ./SensusSrc/1wire_ds2482.su ./SensusSrc/adc.cyclo ./SensusSrc/adc.d ./SensusSrc/adc.o ./SensusSrc/adc.su ./SensusSrc/buttons.cyclo ./SensusSrc/buttons.d ./SensusSrc/buttons.o ./SensusSrc/buttons.su ./SensusSrc/collimator.cyclo ./SensusSrc/collimator.d ./SensusSrc/collimator.o ./SensusSrc/collimator.su ./SensusSrc/control_comm.cyclo ./SensusSrc/control_comm.d ./SensusSrc/control_comm.o ./SensusSrc/control_comm.su ./SensusSrc/crcccitt.cyclo ./SensusSrc/crcccitt.d ./SensusSrc/crcccitt.o ./SensusSrc/crcccitt.su ./SensusSrc/flow.cyclo ./SensusSrc/flow.d ./SensusSrc/flow.o ./SensusSrc/flow.su ./SensusSrc/leds.cyclo ./SensusSrc/leds.d ./SensusSrc/leds.o ./SensusSrc/leds.su ./SensusSrc/lis2mdl.cyclo ./SensusSrc/lis2mdl.d ./SensusSrc/lis2mdl.o ./SensusSrc/lis2mdl.su ./SensusSrc/lis3mdl.cyclo ./SensusSrc/lis3mdl.d ./SensusSrc/lis3mdl.o ./SensusSrc/lis3mdl.su ./SensusSrc/magnetometer.cyclo ./SensusSrc/magnetometer.d ./SensusSrc/magnetometer.o ./SensusSrc/magnetometer.su ./SensusSrc/qc.cyclo ./SensusSrc/qc.d ./SensusSrc/qc.o ./SensusSrc/qc.su ./SensusSrc/setup.cyclo ./SensusSrc/setup.d ./SensusSrc/setup.o ./SensusSrc/setup.su ./SensusSrc/sys_data.cyclo ./SensusSrc/sys_data.d ./SensusSrc/sys_data.o ./SensusSrc/sys_data.su ./SensusSrc/timer.cyclo ./SensusSrc/timer.d ./SensusSrc/timer.o ./SensusSrc/timer.su

.PHONY: clean-SensusSrc

