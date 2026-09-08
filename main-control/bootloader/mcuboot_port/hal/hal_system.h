#ifndef H_HAL_SYSTEM_
#define H_HAL_SYSTEM_

#ifdef __cplusplus
extern "C" {
#endif

/* Resets the MCU; used by boot_serial's "reset" mgmt command. */
void hal_system_reset(void);

#ifdef __cplusplus
}
#endif

#endif /* H_HAL_SYSTEM_ */
