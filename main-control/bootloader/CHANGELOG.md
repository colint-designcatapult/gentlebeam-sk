# Changelog

## [5.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/main-control-bootloader-4.0.0...main-control-bootloader-5.0.0) (2026-09-15)


### ⚠ BREAKING CHANGES

* **sqlite:** `SqliteGrpcServerHost` and `SqliteProtoRepository` constructors now require a `SqlCipherConnectionFactory` instead of a database path, so existing integrations must update their construction and encrypted database initialization.
* **bootloader:** Existing raw firmware images and the legacy CRC-based bootloader flow are no longer compatible. Applications must be linked for the new MCUboot slot addresses and packaged as MCUboot images before update or boot.

### Features

* **bootloader:** implement secure bootloader [GBSK-64] ([#81](https://github.com/DesignCatapult/gentlebeam-sk/issues/81)) ([971684e](https://github.com/DesignCatapult/gentlebeam-sk/commit/971684ec8d57041843442b07b47e1f28a69b9ade))
* **sqlite:** add encrypted database management and security automation [GBSK-91] ([#92](https://github.com/DesignCatapult/gentlebeam-sk/issues/92)) ([29cb845](https://github.com/DesignCatapult/gentlebeam-sk/commit/29cb84500fe760f3050056b7806217848c68739e))

## [4.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/main-control-bootloader-3.0.0...main-control-bootloader-4.0.0) (2026-09-03)


### ⚠ BREAKING CHANGES

* **system:** Removes the public multi-point GCB emission-plan and operational-entry APIs, renames and reshapes related board-state and command APIs, and changes command signatures to operate on single emissions.
* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* [firmware]Import main-control source and consolidate calibration [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) [#6](https://github.com/DesignCatapult/gentlebeam-sk/issues/6) ([766d447](https://github.com/DesignCatapult/gentlebeam-sk/commit/766d44773aa8805fab82e89ef0c50004b08e452e))
* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/DesignCatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/DesignCatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* import high level system diagram for head interface, backup timer and main control [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([e19efad](https://github.com/DesignCatapult/gentlebeam-sk/commit/e19efad5a58696a861c5a194ece180984ac865b9))
* import Main control Bootloader docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([c48e32d](https://github.com/DesignCatapult/gentlebeam-sk/commit/c48e32d2cf50038a54c32f3c6859ebcf22106f74))
* **system:** bring up closed-loop operation and quality control [GBSK-52] ([#76](https://github.com/DesignCatapult/gentlebeam-sk/issues/76)) ([033f6f5](https://github.com/DesignCatapult/gentlebeam-sk/commit/033f6f59ff4fbfcbee5dd268152ed5b3e2a69c33))
* Update docs for Head Interface, Backup Timer and Main Control [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([00884f4](https://github.com/DesignCatapult/gentlebeam-sk/commit/00884f4aafcc61e608536b0e5465c88b15587e79))


### Bug Fixes

* **main-control-fw:** fix bootloader issue [GBSK-46] ([#59](https://github.com/DesignCatapult/gentlebeam-sk/issues/59)) ([b2c4a64](https://github.com/DesignCatapult/gentlebeam-sk/commit/b2c4a643caf31fcceb2a9faffd959301c73f4f5b))
