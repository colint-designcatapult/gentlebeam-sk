#ifndef GENTLEBEAM_HOST_HAL_ATOMIC_H
#define GENTLEBEAM_HOST_HAL_ATOMIC_H

#if !defined(GENTLEBEAM_HOST_BUILD) || !GENTLEBEAM_HOST_BUILD
#error "The host atomic shim must not be used in firmware builds"
#endif

/* The host entry point is single-threaded and has no interrupt callbacks.
 * Retain lexical scope only; these macros provide no synchronization and
 * must not be used to run the control library concurrently. */
#define CRITICAL_SECTION_ENTER() {
#define CRITICAL_SECTION_LEAVE() }

#endif
