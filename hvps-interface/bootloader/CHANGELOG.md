# Changelog

## [6.0.0](https://github.com/colint-designcatapult/gentlebeam-sk/compare/hvps-interface-bootloader-5.0.0...hvps-interface-bootloader-6.0.0) (2026-10-07)


### ⚠ BREAKING CHANGES

* **sqlite:** `SqliteGrpcServerHost` and `SqliteProtoRepository` constructors now require a `SqlCipherConnectionFactory` instead of a database path, so existing integrations must update their construction and encrypted database initialization.
* **bootloader:** Existing raw firmware images and the legacy CRC-based bootloader flow are no longer compatible. Applications must be linked for the new MCUboot slot addresses and packaged as MCUboot images before update or boot.
* **system:** Removes the public multi-point GCB emission-plan and operational-entry APIs, renames and reshapes related board-state and command APIs, and changes command signatures to operate on single emissions.
* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* **bootloader:** implement secure bootloader [GBSK-64] ([#81](https://github.com/colint-designcatapult/gentlebeam-sk/issues/81)) ([971684e](https://github.com/colint-designcatapult/gentlebeam-sk/commit/971684ec8d57041843442b07b47e1f28a69b9ade))
* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/colint-designcatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/colint-designcatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* Import HVPS bootloader and main sources, docs, and consolidate calibraion [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) [#6](https://github.com/colint-designcatapult/gentlebeam-sk/issues/6) ([e84adfd](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e84adfd020506d1664c40cce715e9c63060144e0))
* **sqlite:** add encrypted database management and security automation [GBSK-91] ([#92](https://github.com/colint-designcatapult/gentlebeam-sk/issues/92)) ([29cb845](https://github.com/colint-designcatapult/gentlebeam-sk/commit/29cb84500fe760f3050056b7806217848c68739e))
* **system:** bring up closed-loop operation and quality control [GBSK-52] ([#76](https://github.com/colint-designcatapult/gentlebeam-sk/issues/76)) ([033f6f5](https://github.com/colint-designcatapult/gentlebeam-sk/commit/033f6f59ff4fbfcbee5dd268152ed5b3e2a69c33))


### Bug Fixes

* **bootloader:** increase application page capacity [GBSK-47] ([#60](https://github.com/colint-designcatapult/gentlebeam-sk/issues/60)) ([7b009d8](https://github.com/colint-designcatapult/gentlebeam-sk/commit/7b009d81d7d0fd729a23435b24eb85663773c509))

## [5.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/hvps-interface-bootloader-4.0.0...hvps-interface-bootloader-5.0.0) (2026-09-15)


### ⚠ BREAKING CHANGES

* **sqlite:** `SqliteGrpcServerHost` and `SqliteProtoRepository` constructors now require a `SqlCipherConnectionFactory` instead of a database path, so existing integrations must update their construction and encrypted database initialization.
* **bootloader:** Existing raw firmware images and the legacy CRC-based bootloader flow are no longer compatible. Applications must be linked for the new MCUboot slot addresses and packaged as MCUboot images before update or boot.

### Features

* **bootloader:** implement secure bootloader [GBSK-64] ([#81](https://github.com/DesignCatapult/gentlebeam-sk/issues/81)) ([971684e](https://github.com/DesignCatapult/gentlebeam-sk/commit/971684ec8d57041843442b07b47e1f28a69b9ade))
* **sqlite:** add encrypted database management and security automation [GBSK-91] ([#92](https://github.com/DesignCatapult/gentlebeam-sk/issues/92)) ([29cb845](https://github.com/DesignCatapult/gentlebeam-sk/commit/29cb84500fe760f3050056b7806217848c68739e))

## [4.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/hvps-interface-bootloader-3.0.0...hvps-interface-bootloader-4.0.0) (2026-09-03)


### ⚠ BREAKING CHANGES

* **system:** Removes the public multi-point GCB emission-plan and operational-entry APIs, renames and reshapes related board-state and command APIs, and changes command signatures to operate on single emissions.
* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/DesignCatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/DesignCatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* Import HVPS bootloader and main sources, docs, and consolidate calibraion [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) [#6](https://github.com/DesignCatapult/gentlebeam-sk/issues/6) ([e84adfd](https://github.com/DesignCatapult/gentlebeam-sk/commit/e84adfd020506d1664c40cce715e9c63060144e0))
* **system:** bring up closed-loop operation and quality control [GBSK-52] ([#76](https://github.com/DesignCatapult/gentlebeam-sk/issues/76)) ([033f6f5](https://github.com/DesignCatapult/gentlebeam-sk/commit/033f6f59ff4fbfcbee5dd268152ed5b3e2a69c33))


### Bug Fixes

* **bootloader:** increase application page capacity [GBSK-47] ([#60](https://github.com/DesignCatapult/gentlebeam-sk/issues/60)) ([7b009d8](https://github.com/DesignCatapult/gentlebeam-sk/commit/7b009d81d7d0fd729a23435b24eb85663773c509))
