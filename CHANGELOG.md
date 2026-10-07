# Changelog

## [5.1.0](https://github.com/colint-designcatapult/gentlebeam-sk/compare/5.0.0...5.1.0) (2026-10-07)


### ⚠ BREAKING CHANGES

* **cnc:** The public `ISystemTelemetry` contract and corresponding telemetry field have been renamed from Peltier temperature to condition remaining deciseconds, and the field type/meaning has changed.
* **head-interface:** The public LED sequence enum no longer exposes `LED_SEQ_WARMUP_FAULT` or `LED_SEQ_STANDBY`, and LED sequence mappings are explicitly reassigned; consumers using those identifiers or numeric values must update their integration.
* **sqlite:** `SqliteGrpcServerHost` and `SqliteProtoRepository` constructors now require a `SqlCipherConnectionFactory` instead of a database path, so existing integrations must update their construction and encrypted database initialization.
* **sqlite:** `PresetConfigurationServiceImpl` and `QCSampleServiceImpl` constructors now require an `AuthServiceImpl` dependency, so direct consumers constructing these public service classes must provide the authentication service.
* **main-control:** The public fault enum assigns the new MagnetometerFault value 24 and moves OtherFault to value 25, changing the numeric fault-category mapping for consumers that persisted or exchanged the previous OtherFault value.
* **bootloader:** Existing raw firmware images and the legacy CRC-based bootloader flow are no longer compatible. Applications must be linked for the new MCUboot slot addresses and packaged as MCUboot images before update or boot.
* **qc:** QC service and command interfaces no longer accept sampling-window or diode-count parameters, QcReadings uses accumulated channel values and sample counts instead of the previous data array model, and the QC communication payloads have new fields and session semantics.
* **system:** Removes the public multi-point GCB emission-plan and operational-entry APIs, renames and reshapes related board-state and command APIs, and changes command signatures to operate on single emissions.
* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* [CNC, main-control-FW, docs]Remove obsolete interlocks, improve interlock UI [GBSK-20] ([3c7d980](https://github.com/colint-designcatapult/gentlebeam-sk/commit/3c7d98059d300e215c1d8c7c1946ad3cd748edea))
* [CNC, main-control-fw]String-based fault reporting with latching [GBSK-17] ([4e4c777](https://github.com/colint-designcatapult/gentlebeam-sk/commit/4e4c777c329ec25ac2e2cc2057b459c95c63ffa3))
* [CNC] Add .vs cache and generated cs files to .gitignore [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) ([b234cc0](https://github.com/colint-designcatapult/gentlebeam-sk/commit/b234cc06bdacf8ed9df54e40e8cad498ebde2e51))
* [CNC] import CNC external and internal software documentation [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([21d22fc](https://github.com/colint-designcatapult/gentlebeam-sk/commit/21d22fcbcacd351bcf2e178f601f47bc6814f435))
* [CNC] initial commit for CNC source code. [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) ([e20731b](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e20731b00f6edec1b9c52d7df5b2362d1e40d568))
* [CNC] Telemetry parsing as immutable interface [GBSK-17] ([09abc17](https://github.com/colint-designcatapult/gentlebeam-sk/commit/09abc17394fcf1b752d2e0770c7c59c6e0e968ce))
* [CNC]Local test script, add clear faults to external service [GBSK-20] ([8588877](https://github.com/colint-designcatapult/gentlebeam-sk/commit/85888774569c964cb291094f05cfe2d1fd120da6))
* [CNC]Lock clinical tabs in calibration mode [GBSK-17] ([ee19f3a](https://github.com/colint-designcatapult/gentlebeam-sk/commit/ee19f3aae57a2256d3fae58cb543c85a9128f4a1))
* [CNC]Show applicator status in interlock UI [GBSK-20] ([00ff120](https://github.com/colint-designcatapult/gentlebeam-sk/commit/00ff120c5186e5e8cdd7056f61a56bf6ed7f8e42))
* [CNC]Unify GCB communication code [GBSK-17] ([f84c1d6](https://github.com/colint-designcatapult/gentlebeam-sk/commit/f84c1d6c41c45b8548c614a1aae525501bb5d4c0))
* [CNC]Use receive-only UDP for telemetry [GBSK-15] ([340040c](https://github.com/colint-designcatapult/gentlebeam-sk/commit/340040c95338b68f6780ce6668d3a7ed499fb98c))
* [docs]Update SW documentation for telemetry [GBSK-15] ([8a54262](https://github.com/colint-designcatapult/gentlebeam-sk/commit/8a542621ae995b66186015d5521391f1d697295b))
* [firmware] added code for LIS2MDL [#12](https://github.com/colint-designcatapult/gentlebeam-sk/issues/12) ([af4a152](https://github.com/colint-designcatapult/gentlebeam-sk/commit/af4a152c1394d83efee8caef98b67d655ae0f711))
* [firmware] I2C hot swappable support for QC board [#12](https://github.com/colint-designcatapult/gentlebeam-sk/issues/12) ([ec13fae](https://github.com/colint-designcatapult/gentlebeam-sk/commit/ec13fae246b587979de1cf3c7920cce388693d5c))
* [firmware] remove the Peltier driver/code [GBSK-2] ([e2d3055](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e2d30551d6c72bfd29d3d0cb7c0510ba9a88b003))
* [firmware]Import main-control source and consolidate calibration [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) [#6](https://github.com/colint-designcatapult/gentlebeam-sk/issues/6) ([766d447](https://github.com/colint-designcatapult/gentlebeam-sk/commit/766d44773aa8805fab82e89ef0c50004b08e452e))
* [hvps-FW] Mention FreeRTOS task in SDS [GBSK-25] ([f23bfa2](https://github.com/colint-designcatapult/gentlebeam-sk/commit/f23bfa2f74ce715bea63ac5d2baa28265f811d43))
* [hvps-FW] Setup FreeRTOS with fat task [GBSK-25] ([d140a57](https://github.com/colint-designcatapult/gentlebeam-sk/commit/d140a57e9c4a1eb948da39d5924543e6d053806d))
* [hvps-FW] Use CMake as alternative build system [GBSK-25] ([215899b](https://github.com/colint-designcatapult/gentlebeam-sk/commit/215899b9da0514690b7a7dcbae297f9d399fc729))
* [hvps-FW] Use new CubeMX linker script for FreeRTOS [GBSK-25] ([7f3e3a6](https://github.com/colint-designcatapult/gentlebeam-sk/commit/7f3e3a6420b1982cb987672a13931934e6804348))
* [hvps-if FW] Update docs for [GBSK-37] ([57440c3](https://github.com/colint-designcatapult/gentlebeam-sk/commit/57440c3fe5014e6882e920575de90e5a1f74e4ee))
* [hvps-if-FW] Update docs for peripheral  changes [GBSK-29] ([a06b2ac](https://github.com/colint-designcatapult/gentlebeam-sk/commit/a06b2ac500c72a9a450eaf08ce9924b6baa20d62))
* [main-control-FW]Send device info every second w/ calibration [GBSK-17] ([67c0ed4](https://github.com/colint-designcatapult/gentlebeam-sk/commit/67c0ed48c67b551bccd27b4bdf9d51bf099c3a2e))
* [main-control-FW]Use ref bufs for telemetry for perf [GBSK-15] ([6ff8e9a](https://github.com/colint-designcatapult/gentlebeam-sk/commit/6ff8e9af4beb4fee143c81a531f5c36a2684003d))
* Add strictdoc foundation for req management [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([c76c57f](https://github.com/colint-designcatapult/gentlebeam-sk/commit/c76c57fd29d32e8c812b60b1915cc257a53dbbc5))
* **bootloader:** implement secure bootloader [GBSK-64] ([#81](https://github.com/colint-designcatapult/gentlebeam-sk/issues/81)) ([971684e](https://github.com/colint-designcatapult/gentlebeam-sk/commit/971684ec8d57041843442b07b47e1f28a69b9ade))
* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/colint-designcatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/colint-designcatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* **cnc-ucsi:** rewrite and integrate calibration software [GBSK-35] ([#63](https://github.com/colint-designcatapult/gentlebeam-sk/issues/63)) ([2f459b6](https://github.com/colint-designcatapult/gentlebeam-sk/commit/2f459b6fbffb2d05b969e2d53c3b26ad1c8ef95d))
* **cnc:** add Daily Warmup countdown timer [GBSK-98] ([#127](https://github.com/colint-designcatapult/gentlebeam-sk/issues/127)) ([70d5b35](https://github.com/colint-designcatapult/gentlebeam-sk/commit/70d5b35db5ecf31f8b153a45f7c23a087e89c2fe))
* **cnc:** migrate treatment camera monitoring to WebView and go2rtc [GBSK-79] ([#86](https://github.com/colint-designcatapult/gentlebeam-sk/issues/86)) ([e87c932](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e87c932b1ffb8e4d01a5ec199ea3e0e3c23aec46))
* **head-interface:** add DotStar effects and align LED sequences [GBSK-93] ([#97](https://github.com/colint-designcatapult/gentlebeam-sk/issues/97)) ([9a07d7f](https://github.com/colint-designcatapult/gentlebeam-sk/commit/9a07d7faa060a473d919c2ee472bd7702c3df737))
* Import Backup Timer docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([86204ef](https://github.com/colint-designcatapult/gentlebeam-sk/commit/86204efbc56e341faed0c4d56384ff9209bb2686))
* Import backup-timers-fw [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) ([13655b6](https://github.com/colint-designcatapult/gentlebeam-sk/commit/13655b666b105e7fd273199821efa19902fdb1cb))
* Import Head Interface docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([5cadcad](https://github.com/colint-designcatapult/gentlebeam-sk/commit/5cadcad749a01b735c9e3cae4075bf9dea3b4630))
* Import head-interface main source and consolidate calibration [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) [#6](https://github.com/colint-designcatapult/gentlebeam-sk/issues/6) ([436abc5](https://github.com/colint-designcatapult/gentlebeam-sk/commit/436abc5ad1784e84119cbd00d5e40e2883568db3))
* import high level system diagram for head interface, backup timer and main control [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([e19efad](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e19efad5a58696a861c5a194ece180984ac865b9))
* Import HVPS bootloader and main sources, docs, and consolidate calibraion [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) [#6](https://github.com/colint-designcatapult/gentlebeam-sk/issues/6) ([e84adfd](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e84adfd020506d1664c40cce715e9c63060144e0))
* import Main control Bootloader docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([c48e32d](https://github.com/colint-designcatapult/gentlebeam-sk/commit/c48e32d2cf50038a54c32f3c6859ebcf22106f74))
* Import Main Control SRS and SDD docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([ce2fb89](https://github.com/colint-designcatapult/gentlebeam-sk/commit/ce2fb8960c225d279788220ef43b8ffe1b8eeac7))
* Import Main Control Test Protocol docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([d0407eb](https://github.com/colint-designcatapult/gentlebeam-sk/commit/d0407eb2916d2e7ac367435d43464721fc82737e))
* **main-control-fw:** request HVPS to clear faults [GBSK-55] ([#68](https://github.com/colint-designcatapult/gentlebeam-sk/issues/68)) ([41de6b4](https://github.com/colint-designcatapult/gentlebeam-sk/commit/41de6b4cc013d476ab214063c6f7628898309181))
* **main-control:** add emission magnetometer fault monitoring [GBSK-62] ([#87](https://github.com/colint-designcatapult/gentlebeam-sk/issues/87)) ([df685bc](https://github.com/colint-designcatapult/gentlebeam-sk/commit/df685bc0b432537cf1fac69aa4b538ab6a7a8542))
* **qc:** implement ADC continuous accumulation [GBSK-78] ([#77](https://github.com/colint-designcatapult/gentlebeam-sk/issues/77)) ([377e794](https://github.com/colint-designcatapult/gentlebeam-sk/commit/377e79456acc952fa2fd89924191f340ed92de02))
* Revise code/SRS to ensure compliant flow and temperature monitoring [GBSK-2] https://op.designcatapult.com/wp/GBSK-2 ([e84c2ef](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e84c2ef853afb60d5a37fea56480abea2788f741))
* **sqlite:** add encrypted database management and security automation [GBSK-91] ([#92](https://github.com/colint-designcatapult/gentlebeam-sk/issues/92)) ([29cb845](https://github.com/colint-designcatapult/gentlebeam-sk/commit/29cb84500fe760f3050056b7806217848c68739e))
* **system:** bring up closed-loop operation and quality control [GBSK-52] ([#76](https://github.com/colint-designcatapult/gentlebeam-sk/issues/76)) ([033f6f5](https://github.com/colint-designcatapult/gentlebeam-sk/commit/033f6f59ff4fbfcbee5dd268152ed5b3e2a69c33))
* **test:** automate main-control system tests [GBSK-107] ([#130](https://github.com/colint-designcatapult/gentlebeam-sk/issues/130)) ([e3c153f](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e3c153f15ab15374af22d6a48e1e5cc7a8ee67e1))
* Update docs for Head Interface, Backup Timer and Main Control [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([00884f4](https://github.com/colint-designcatapult/gentlebeam-sk/commit/00884f4aafcc61e608536b0e5465c88b15587e79))
* Update docs for Logging Module and Configuration Interface Module [GBSK-34] ([c72159c](https://github.com/colint-designcatapult/gentlebeam-sk/commit/c72159c20a5b4e4d12ab753c241494218271a259))
* update internal calibration start [GBSK-9] HVPS internal ADCs report invalid data https://op.designcatapult.com/wp/GBSK-9 ([c127050](https://github.com/colint-designcatapult/gentlebeam-sk/commit/c127050de34cf08da6115d4674f5a4f0f3574a30))


### Bug Fixes

* **bootloader:** Firmware CRC calculation regression [GBSK-49] ([#61](https://github.com/colint-designcatapult/gentlebeam-sk/issues/61)) ([50c9319](https://github.com/colint-designcatapult/gentlebeam-sk/commit/50c9319dde5c42f62215adedd6ee3f15564c7701))
* **bootloader:** increase application page capacity [GBSK-47] ([#60](https://github.com/colint-designcatapult/gentlebeam-sk/issues/60)) ([7b009d8](https://github.com/colint-designcatapult/gentlebeam-sk/commit/7b009d81d7d0fd729a23435b24eb85663773c509))
* **ci:** prevent CNC test hangs [GBSK-94] ([#95](https://github.com/colint-designcatapult/gentlebeam-sk/issues/95)) ([b9172c0](https://github.com/colint-designcatapult/gentlebeam-sk/commit/b9172c0acb6334e7d692d2b18f811b16e8d851b4))
* **ci:** prevent duplicate UCSI appsettings in embedded publishes [GBSK-56] ([#71](https://github.com/colint-designcatapult/gentlebeam-sk/issues/71)) ([8338ef2](https://github.com/colint-designcatapult/gentlebeam-sk/commit/8338ef28f88761b4f80105f9fc5a70907bff12c1))
* **ci:** update HVPS MCUboot firmware packaging [GBSK-88] ([#85](https://github.com/colint-designcatapult/gentlebeam-sk/issues/85)) ([af6a1f3](https://github.com/colint-designcatapult/gentlebeam-sk/commit/af6a1f3674c33871207d5b7041418eb6229b67a7))
* **cnc-ucsi:** correct HVPS fault status bit mappings [GBSK-58] ([#69](https://github.com/colint-designcatapult/gentlebeam-sk/issues/69)) ([4fb9608](https://github.com/colint-designcatapult/gentlebeam-sk/commit/4fb9608173215a4e999b18d4c0b4709e995cb405))
* **cnc-ucsi:** display firmware and software version information [GBSK-53] ([#66](https://github.com/colint-designcatapult/gentlebeam-sk/issues/66)) ([d2df255](https://github.com/colint-designcatapult/gentlebeam-sk/commit/d2df2553e5ac7d5d4962af5687a3c53718885390))
* **cnc-ucsi:** enable Clear Faults in standalone UCSI [GBSK-57] ([#67](https://github.com/colint-designcatapult/gentlebeam-sk/issues/67)) ([08d83bd](https://github.com/colint-designcatapult/gentlebeam-sk/commit/08d83bdd042d94db9ca08efcab142488f1cf9310))
* **cnc/docs:** populate prescription energy and correct TDF protocol [GBSK-112] ([#114](https://github.com/colint-designcatapult/gentlebeam-sk/issues/114)) ([2889e94](https://github.com/colint-designcatapult/gentlebeam-sk/commit/2889e9434c851a9276a2acb25349d32f76463d24))
* **cnc:** alert when prescription energy changes [GBSK-111] ([#113](https://github.com/colint-designcatapult/gentlebeam-sk/issues/113)) ([a7855f2](https://github.com/colint-designcatapult/gentlebeam-sk/commit/a7855f2d9bce5701d705d0f028c55df42325fc34))
* **cnc:** fix GitHub Action CNC testing [GBSK-59] ([#72](https://github.com/colint-designcatapult/gentlebeam-sk/issues/72)) ([0e544cc](https://github.com/colint-designcatapult/gentlebeam-sk/commit/0e544cc68ee34fdd9055d2d0dceb472fd58e5e62))
* **cnc:** hide camera views after session timeout [GBSK-100] ([#101](https://github.com/colint-designcatapult/gentlebeam-sk/issues/101)) ([fb94a03](https://github.com/colint-designcatapult/gentlebeam-sk/commit/fb94a03178ec201112b55d18f4bd3e2f65c75657))
* **cnc:** initialize database and first-run administrator setup [GBSK-90] ([#91](https://github.com/colint-designcatapult/gentlebeam-sk/issues/91)) ([28944c0](https://github.com/colint-designcatapult/gentlebeam-sk/commit/28944c0333287c3a84b1bec0e219a5675d64e9a9))
* **cnc:** pool connections and reduce KDF iterations [GBSK-101] ([#99](https://github.com/colint-designcatapult/gentlebeam-sk/issues/99)) ([970a5cb](https://github.com/colint-designcatapult/gentlebeam-sk/commit/970a5cbd79b40e82ab7257b739926c66d73a6209))
* **cnc:** prevent duplicate patient records [GBSK-109] ([#111](https://github.com/colint-designcatapult/gentlebeam-sk/issues/111)) ([283e8cc](https://github.com/colint-designcatapult/gentlebeam-sk/commit/283e8ccb790406424a37506c93cf4ad8c8617211))
* **cnc:** register dummy telemetry services and interlocks [GBSK-115] ([#116](https://github.com/colint-designcatapult/gentlebeam-sk/issues/116)) ([83febe7](https://github.com/colint-designcatapult/gentlebeam-sk/commit/83febe7a9257b04182077d686a0b22d94a04bd61))
* **cnc:** support middle-name patient search [GBSK-110] ([#112](https://github.com/colint-designcatapult/gentlebeam-sk/issues/112)) ([384b7ed](https://github.com/colint-designcatapult/gentlebeam-sk/commit/384b7ed73618e28739453beda3f7f0974994b71d))
* **cnc:** terminate X-ray emission when remote console disconnects [GBSK-116] ([#123](https://github.com/colint-designcatapult/gentlebeam-sk/issues/123)) ([4aaf507](https://github.com/colint-designcatapult/gentlebeam-sk/commit/4aaf5072b5d7dfa2582f6c3d881e85effc5389e1))
* **config:** tune source-specific parameters [GBSK-108] ([#107](https://github.com/colint-designcatapult/gentlebeam-sk/issues/107)) ([7bd96f0](https://github.com/colint-designcatapult/gentlebeam-sk/commit/7bd96f00afe3e39068f8feed126531644d6f690f))
* **docs:** prevent docx export corruption [GBSK-105] ([#104](https://github.com/colint-designcatapult/gentlebeam-sk/issues/104)) ([563b9c3](https://github.com/colint-designcatapult/gentlebeam-sk/commit/563b9c3d91674a714f204fec677dfb5e1d10b9d8))
* **gcb:** turn off heater when primed [GBSK-80] ([#79](https://github.com/colint-designcatapult/gentlebeam-sk/issues/79)) ([dabfb88](https://github.com/colint-designcatapult/gentlebeam-sk/commit/dabfb8873dbfae2635cdc6b000672423ef7bc78e))
* **head-interface:** sync IOC with continuous DMA ADC configuration [GBSK-83] ([#82](https://github.com/colint-designcatapult/gentlebeam-sk/issues/82)) ([54c840d](https://github.com/colint-designcatapult/gentlebeam-sk/commit/54c840dfecfdb03500430a9e605d55b3e396a1dd))
* **hvps-interface:** adjust KV_RAMP_FAST and KV_RAMP_SLOW configs [GBSK-95] ([#98](https://github.com/colint-designcatapult/gentlebeam-sk/issues/98)) ([9ac383f](https://github.com/colint-designcatapult/gentlebeam-sk/commit/9ac383f470d9d41f5c737fac8ebd183ce20a16a8))
* **hvps:** handle active-low OC_CAT fault [GBSK-51] ([#62](https://github.com/colint-designcatapult/gentlebeam-sk/issues/62)) ([3c41564](https://github.com/colint-designcatapult/gentlebeam-sk/commit/3c4156486089245c6103669a585441d5038d7ff8))
* **hvps:** prevent hanging communication with HVPS [GBSK-84] ([#84](https://github.com/colint-designcatapult/gentlebeam-sk/issues/84)) ([1428efe](https://github.com/colint-designcatapult/gentlebeam-sk/commit/1428efe1c293db1cf02c37e7bce25d8dbf07392c))
* **hvps:** restore LED4 blinking functionality [GBSK-54] ([#64](https://github.com/colint-designcatapult/gentlebeam-sk/issues/64)) ([5b88673](https://github.com/colint-designcatapult/gentlebeam-sk/commit/5b88673c0436c61ab7ad61b3bfbf0eb3e13910b7))
* **main-control-fw:** fix bootloader issue [GBSK-46] ([#59](https://github.com/colint-designcatapult/gentlebeam-sk/issues/59)) ([b2c4a64](https://github.com/colint-designcatapult/gentlebeam-sk/commit/b2c4a643caf31fcceb2a9faffd959301c73f4f5b))
* **main-control-fw:** fix no head interface data unpacking in calibration mode [GBSK-102] ([#100](https://github.com/colint-designcatapult/gentlebeam-sk/issues/100)) ([a849e54](https://github.com/colint-designcatapult/gentlebeam-sk/commit/a849e54edb96000bf2784b38108c1b8f0298df5c))
* **main-control/bootloader:** restrict serial uploads to secondary slot [GBSK-117] ([#117](https://github.com/colint-designcatapult/gentlebeam-sk/issues/117)) ([85d3cec](https://github.com/colint-designcatapult/gentlebeam-sk/commit/85d3cec29393c5a84db76719538bdafc5c84a435))
* **main-control:** convert ion pump feedback from voltage to Torr [GBSK-81] ([#90](https://github.com/colint-designcatapult/gentlebeam-sk/issues/90)) ([3f1dd9d](https://github.com/colint-designcatapult/gentlebeam-sk/commit/3f1dd9df2fb231732a0d4789f433a6ae07675cf5))
* **release-please:** adjust versioning [GBSK-133] ([#133](https://github.com/colint-designcatapult/gentlebeam-sk/issues/133)) ([11765a3](https://github.com/colint-designcatapult/gentlebeam-sk/commit/11765a3f846b17476d674e0268672d11b6a748a9))
* **sqlite:** improve database service consistency [GBSK-85] ([#89](https://github.com/colint-designcatapult/gentlebeam-sk/issues/89)) ([2d67bbe](https://github.com/colint-designcatapult/gentlebeam-sk/commit/2d67bbebaf4f75337fff73f0f6e7c0bb51f08903))
* **treatment:** clear plan after treatment [GBSK-104] ([#103](https://github.com/colint-designcatapult/gentlebeam-sk/issues/103)) ([1abe7df](https://github.com/colint-designcatapult/gentlebeam-sk/commit/1abe7df94b461115e65b373bbdd1050fe49ff58f))
* **ucsi:** disable emission start in internal service tab [GBSK-122] ([#125](https://github.com/colint-designcatapult/gentlebeam-sk/issues/125)) ([c007d96](https://github.com/colint-designcatapult/gentlebeam-sk/commit/c007d96b3d40b01749aa45db8a9a6ed3a02fb4ad))

## [5.0.1](https://github.com/DesignCatapult/gentlebeam-sk/compare/5.0.0...5.0.1) (2026-09-15)


### Bug Fixes

* **ci:** prevent CNC test hangs [GBSK-94] ([#95](https://github.com/DesignCatapult/gentlebeam-sk/issues/95)) ([b9172c0](https://github.com/DesignCatapult/gentlebeam-sk/commit/b9172c0acb6334e7d692d2b18f811b16e8d851b4))

## [5.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/4.0.0...5.0.0) (2026-09-15)


### ⚠ BREAKING CHANGES

* **sqlite:** `SqliteGrpcServerHost` and `SqliteProtoRepository` constructors now require a `SqlCipherConnectionFactory` instead of a database path, so existing integrations must update their construction and encrypted database initialization.
* **sqlite:** `PresetConfigurationServiceImpl` and `QCSampleServiceImpl` constructors now require an `AuthServiceImpl` dependency, so direct consumers constructing these public service classes must provide the authentication service.
* **main-control:** The public fault enum assigns the new MagnetometerFault value 24 and moves OtherFault to value 25, changing the numeric fault-category mapping for consumers that persisted or exchanged the previous OtherFault value.
* **bootloader:** Existing raw firmware images and the legacy CRC-based bootloader flow are no longer compatible. Applications must be linked for the new MCUboot slot addresses and packaged as MCUboot images before update or boot.

### Features

* **bootloader:** implement secure bootloader [GBSK-64] ([#81](https://github.com/DesignCatapult/gentlebeam-sk/issues/81)) ([971684e](https://github.com/DesignCatapult/gentlebeam-sk/commit/971684ec8d57041843442b07b47e1f28a69b9ade))
* **cnc:** migrate treatment camera monitoring to WebView and go2rtc [GBSK-79] ([#86](https://github.com/DesignCatapult/gentlebeam-sk/issues/86)) ([e87c932](https://github.com/DesignCatapult/gentlebeam-sk/commit/e87c932b1ffb8e4d01a5ec199ea3e0e3c23aec46))
* **main-control:** add emission magnetometer fault monitoring [GBSK-62] ([#87](https://github.com/DesignCatapult/gentlebeam-sk/issues/87)) ([df685bc](https://github.com/DesignCatapult/gentlebeam-sk/commit/df685bc0b432537cf1fac69aa4b538ab6a7a8542))
* **sqlite:** add encrypted database management and security automation [GBSK-91] ([#92](https://github.com/DesignCatapult/gentlebeam-sk/issues/92)) ([29cb845](https://github.com/DesignCatapult/gentlebeam-sk/commit/29cb84500fe760f3050056b7806217848c68739e))


### Bug Fixes

* **ci:** update HVPS MCUboot firmware packaging [GBSK-88] ([#85](https://github.com/DesignCatapult/gentlebeam-sk/issues/85)) ([af6a1f3](https://github.com/DesignCatapult/gentlebeam-sk/commit/af6a1f3674c33871207d5b7041418eb6229b67a7))
* **cnc:** initialize database and first-run administrator setup [GBSK-90] ([#91](https://github.com/DesignCatapult/gentlebeam-sk/issues/91)) ([28944c0](https://github.com/DesignCatapult/gentlebeam-sk/commit/28944c0333287c3a84b1bec0e219a5675d64e9a9))
* **head-interface:** sync IOC with continuous DMA ADC configuration [GBSK-83] ([#82](https://github.com/DesignCatapult/gentlebeam-sk/issues/82)) ([54c840d](https://github.com/DesignCatapult/gentlebeam-sk/commit/54c840dfecfdb03500430a9e605d55b3e396a1dd))
* **hvps:** prevent hanging communication with HVPS [GBSK-84] ([#84](https://github.com/DesignCatapult/gentlebeam-sk/issues/84)) ([1428efe](https://github.com/DesignCatapult/gentlebeam-sk/commit/1428efe1c293db1cf02c37e7bce25d8dbf07392c))
* **main-control:** convert ion pump feedback from voltage to Torr [GBSK-81] ([#90](https://github.com/DesignCatapult/gentlebeam-sk/issues/90)) ([3f1dd9d](https://github.com/DesignCatapult/gentlebeam-sk/commit/3f1dd9df2fb231732a0d4789f433a6ae07675cf5))
* **sqlite:** improve database service consistency [GBSK-85] ([#89](https://github.com/DesignCatapult/gentlebeam-sk/issues/89)) ([2d67bbe](https://github.com/DesignCatapult/gentlebeam-sk/commit/2d67bbebaf4f75337fff73f0f6e7c0bb51f08903))

## [4.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/3.0.0...4.0.0) (2026-09-03)


### ⚠ BREAKING CHANGES

* **qc:** QC service and command interfaces no longer accept sampling-window or diode-count parameters, QcReadings uses accumulated channel values and sample counts instead of the previous data array model, and the QC communication payloads have new fields and session semantics.
* **system:** Removes the public multi-point GCB emission-plan and operational-entry APIs, renames and reshapes related board-state and command APIs, and changes command signatures to operate on single emissions.
* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* [CNC, main-control-FW, docs]Remove obsolete interlocks, improve interlock UI [GBSK-20] ([3c7d980](https://github.com/DesignCatapult/gentlebeam-sk/commit/3c7d98059d300e215c1d8c7c1946ad3cd748edea))
* [CNC, main-control-fw]String-based fault reporting with latching [GBSK-17] ([4e4c777](https://github.com/DesignCatapult/gentlebeam-sk/commit/4e4c777c329ec25ac2e2cc2057b459c95c63ffa3))
* [CNC] Add .vs cache and generated cs files to .gitignore [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) ([b234cc0](https://github.com/DesignCatapult/gentlebeam-sk/commit/b234cc06bdacf8ed9df54e40e8cad498ebde2e51))
* [CNC] import CNC external and internal software documentation [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([21d22fc](https://github.com/DesignCatapult/gentlebeam-sk/commit/21d22fcbcacd351bcf2e178f601f47bc6814f435))
* [CNC] initial commit for CNC source code. [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) ([e20731b](https://github.com/DesignCatapult/gentlebeam-sk/commit/e20731b00f6edec1b9c52d7df5b2362d1e40d568))
* [CNC] Telemetry parsing as immutable interface [GBSK-17] ([09abc17](https://github.com/DesignCatapult/gentlebeam-sk/commit/09abc17394fcf1b752d2e0770c7c59c6e0e968ce))
* [CNC]Local test script, add clear faults to external service [GBSK-20] ([8588877](https://github.com/DesignCatapult/gentlebeam-sk/commit/85888774569c964cb291094f05cfe2d1fd120da6))
* [CNC]Lock clinical tabs in calibration mode [GBSK-17] ([ee19f3a](https://github.com/DesignCatapult/gentlebeam-sk/commit/ee19f3aae57a2256d3fae58cb543c85a9128f4a1))
* [CNC]Show applicator status in interlock UI [GBSK-20] ([00ff120](https://github.com/DesignCatapult/gentlebeam-sk/commit/00ff120c5186e5e8cdd7056f61a56bf6ed7f8e42))
* [CNC]Unify GCB communication code [GBSK-17] ([f84c1d6](https://github.com/DesignCatapult/gentlebeam-sk/commit/f84c1d6c41c45b8548c614a1aae525501bb5d4c0))
* [CNC]Use receive-only UDP for telemetry [GBSK-15] ([340040c](https://github.com/DesignCatapult/gentlebeam-sk/commit/340040c95338b68f6780ce6668d3a7ed499fb98c))
* [docs]Update SW documentation for telemetry [GBSK-15] ([8a54262](https://github.com/DesignCatapult/gentlebeam-sk/commit/8a542621ae995b66186015d5521391f1d697295b))
* [firmware] added code for LIS2MDL [#12](https://github.com/DesignCatapult/gentlebeam-sk/issues/12) ([af4a152](https://github.com/DesignCatapult/gentlebeam-sk/commit/af4a152c1394d83efee8caef98b67d655ae0f711))
* [firmware] I2C hot swappable support for QC board [#12](https://github.com/DesignCatapult/gentlebeam-sk/issues/12) ([ec13fae](https://github.com/DesignCatapult/gentlebeam-sk/commit/ec13fae246b587979de1cf3c7920cce388693d5c))
* [firmware] remove the Peltier driver/code [GBSK-2] ([e2d3055](https://github.com/DesignCatapult/gentlebeam-sk/commit/e2d30551d6c72bfd29d3d0cb7c0510ba9a88b003))
* [firmware]Import main-control source and consolidate calibration [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) [#6](https://github.com/DesignCatapult/gentlebeam-sk/issues/6) ([766d447](https://github.com/DesignCatapult/gentlebeam-sk/commit/766d44773aa8805fab82e89ef0c50004b08e452e))
* [hvps-FW] Mention FreeRTOS task in SDS [GBSK-25] ([f23bfa2](https://github.com/DesignCatapult/gentlebeam-sk/commit/f23bfa2f74ce715bea63ac5d2baa28265f811d43))
* [hvps-FW] Setup FreeRTOS with fat task [GBSK-25] ([d140a57](https://github.com/DesignCatapult/gentlebeam-sk/commit/d140a57e9c4a1eb948da39d5924543e6d053806d))
* [hvps-FW] Use CMake as alternative build system [GBSK-25] ([215899b](https://github.com/DesignCatapult/gentlebeam-sk/commit/215899b9da0514690b7a7dcbae297f9d399fc729))
* [hvps-FW] Use new CubeMX linker script for FreeRTOS [GBSK-25] ([7f3e3a6](https://github.com/DesignCatapult/gentlebeam-sk/commit/7f3e3a6420b1982cb987672a13931934e6804348))
* [hvps-if FW] Update docs for [GBSK-37] ([57440c3](https://github.com/DesignCatapult/gentlebeam-sk/commit/57440c3fe5014e6882e920575de90e5a1f74e4ee))
* [hvps-if-FW] Update docs for peripheral  changes [GBSK-29] ([a06b2ac](https://github.com/DesignCatapult/gentlebeam-sk/commit/a06b2ac500c72a9a450eaf08ce9924b6baa20d62))
* [main-control-FW]Send device info every second w/ calibration [GBSK-17] ([67c0ed4](https://github.com/DesignCatapult/gentlebeam-sk/commit/67c0ed48c67b551bccd27b4bdf9d51bf099c3a2e))
* [main-control-FW]Use ref bufs for telemetry for perf [GBSK-15] ([6ff8e9a](https://github.com/DesignCatapult/gentlebeam-sk/commit/6ff8e9af4beb4fee143c81a531f5c36a2684003d))
* Add strictdoc foundation for req management [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([c76c57f](https://github.com/DesignCatapult/gentlebeam-sk/commit/c76c57fd29d32e8c812b60b1915cc257a53dbbc5))
* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/DesignCatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/DesignCatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* **cnc-ucsi:** rewrite and integrate calibration software [GBSK-35] ([#63](https://github.com/DesignCatapult/gentlebeam-sk/issues/63)) ([2f459b6](https://github.com/DesignCatapult/gentlebeam-sk/commit/2f459b6fbffb2d05b969e2d53c3b26ad1c8ef95d))
* Import Backup Timer docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([86204ef](https://github.com/DesignCatapult/gentlebeam-sk/commit/86204efbc56e341faed0c4d56384ff9209bb2686))
* Import backup-timers-fw [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) ([13655b6](https://github.com/DesignCatapult/gentlebeam-sk/commit/13655b666b105e7fd273199821efa19902fdb1cb))
* Import Head Interface docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([5cadcad](https://github.com/DesignCatapult/gentlebeam-sk/commit/5cadcad749a01b735c9e3cae4075bf9dea3b4630))
* Import head-interface main source and consolidate calibration [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) [#6](https://github.com/DesignCatapult/gentlebeam-sk/issues/6) ([436abc5](https://github.com/DesignCatapult/gentlebeam-sk/commit/436abc5ad1784e84119cbd00d5e40e2883568db3))
* import high level system diagram for head interface, backup timer and main control [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([e19efad](https://github.com/DesignCatapult/gentlebeam-sk/commit/e19efad5a58696a861c5a194ece180984ac865b9))
* Import HVPS bootloader and main sources, docs, and consolidate calibraion [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) [#6](https://github.com/DesignCatapult/gentlebeam-sk/issues/6) ([e84adfd](https://github.com/DesignCatapult/gentlebeam-sk/commit/e84adfd020506d1664c40cce715e9c63060144e0))
* import Main control Bootloader docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([c48e32d](https://github.com/DesignCatapult/gentlebeam-sk/commit/c48e32d2cf50038a54c32f3c6859ebcf22106f74))
* Import Main Control SRS and SDD docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([ce2fb89](https://github.com/DesignCatapult/gentlebeam-sk/commit/ce2fb8960c225d279788220ef43b8ffe1b8eeac7))
* Import Main Control Test Protocol docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([d0407eb](https://github.com/DesignCatapult/gentlebeam-sk/commit/d0407eb2916d2e7ac367435d43464721fc82737e))
* **main-control-fw:** request HVPS to clear faults [GBSK-55] ([#68](https://github.com/DesignCatapult/gentlebeam-sk/issues/68)) ([41de6b4](https://github.com/DesignCatapult/gentlebeam-sk/commit/41de6b4cc013d476ab214063c6f7628898309181))
* **qc:** implement ADC continuous accumulation [GBSK-78] ([#77](https://github.com/DesignCatapult/gentlebeam-sk/issues/77)) ([377e794](https://github.com/DesignCatapult/gentlebeam-sk/commit/377e79456acc952fa2fd89924191f340ed92de02))
* Revise code/SRS to ensure compliant flow and temperature monitoring [GBSK-2] https://op.designcatapult.com/wp/GBSK-2 ([e84c2ef](https://github.com/DesignCatapult/gentlebeam-sk/commit/e84c2ef853afb60d5a37fea56480abea2788f741))
* **system:** bring up closed-loop operation and quality control [GBSK-52] ([#76](https://github.com/DesignCatapult/gentlebeam-sk/issues/76)) ([033f6f5](https://github.com/DesignCatapult/gentlebeam-sk/commit/033f6f59ff4fbfcbee5dd268152ed5b3e2a69c33))
* Update docs for Head Interface, Backup Timer and Main Control [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([00884f4](https://github.com/DesignCatapult/gentlebeam-sk/commit/00884f4aafcc61e608536b0e5465c88b15587e79))
* Update docs for Logging Module and Configuration Interface Module [GBSK-34] ([c72159c](https://github.com/DesignCatapult/gentlebeam-sk/commit/c72159c20a5b4e4d12ab753c241494218271a259))
* update internal calibration start [GBSK-9] HVPS internal ADCs report invalid data https://op.designcatapult.com/wp/GBSK-9 ([c127050](https://github.com/DesignCatapult/gentlebeam-sk/commit/c127050de34cf08da6115d4674f5a4f0f3574a30))


### Bug Fixes

* **bootloader:** Firmware CRC calculation regression [GBSK-49] ([#61](https://github.com/DesignCatapult/gentlebeam-sk/issues/61)) ([50c9319](https://github.com/DesignCatapult/gentlebeam-sk/commit/50c9319dde5c42f62215adedd6ee3f15564c7701))
* **bootloader:** increase application page capacity [GBSK-47] ([#60](https://github.com/DesignCatapult/gentlebeam-sk/issues/60)) ([7b009d8](https://github.com/DesignCatapult/gentlebeam-sk/commit/7b009d81d7d0fd729a23435b24eb85663773c509))
* **ci:** prevent duplicate UCSI appsettings in embedded publishes [GBSK-56] ([#71](https://github.com/DesignCatapult/gentlebeam-sk/issues/71)) ([8338ef2](https://github.com/DesignCatapult/gentlebeam-sk/commit/8338ef28f88761b4f80105f9fc5a70907bff12c1))
* **cnc-ucsi:** correct HVPS fault status bit mappings [GBSK-58] ([#69](https://github.com/DesignCatapult/gentlebeam-sk/issues/69)) ([4fb9608](https://github.com/DesignCatapult/gentlebeam-sk/commit/4fb9608173215a4e999b18d4c0b4709e995cb405))
* **cnc-ucsi:** display firmware and software version information [GBSK-53] ([#66](https://github.com/DesignCatapult/gentlebeam-sk/issues/66)) ([d2df255](https://github.com/DesignCatapult/gentlebeam-sk/commit/d2df2553e5ac7d5d4962af5687a3c53718885390))
* **cnc-ucsi:** enable Clear Faults in standalone UCSI [GBSK-57] ([#67](https://github.com/DesignCatapult/gentlebeam-sk/issues/67)) ([08d83bd](https://github.com/DesignCatapult/gentlebeam-sk/commit/08d83bdd042d94db9ca08efcab142488f1cf9310))
* **cnc:** fix GitHub Action CNC testing [GBSK-59] ([#72](https://github.com/DesignCatapult/gentlebeam-sk/issues/72)) ([0e544cc](https://github.com/DesignCatapult/gentlebeam-sk/commit/0e544cc68ee34fdd9055d2d0dceb472fd58e5e62))
* **gcb:** turn off heater when primed [GBSK-80] ([#79](https://github.com/DesignCatapult/gentlebeam-sk/issues/79)) ([dabfb88](https://github.com/DesignCatapult/gentlebeam-sk/commit/dabfb8873dbfae2635cdc6b000672423ef7bc78e))
* **hvps:** handle active-low OC_CAT fault [GBSK-51] ([#62](https://github.com/DesignCatapult/gentlebeam-sk/issues/62)) ([3c41564](https://github.com/DesignCatapult/gentlebeam-sk/commit/3c4156486089245c6103669a585441d5038d7ff8))
* **hvps:** restore LED4 blinking functionality [GBSK-54] ([#64](https://github.com/DesignCatapult/gentlebeam-sk/issues/64)) ([5b88673](https://github.com/DesignCatapult/gentlebeam-sk/commit/5b88673c0436c61ab7ad61b3bfbf0eb3e13910b7))
* **main-control-fw:** fix bootloader issue [GBSK-46] ([#59](https://github.com/DesignCatapult/gentlebeam-sk/issues/59)) ([b2c4a64](https://github.com/DesignCatapult/gentlebeam-sk/commit/b2c4a643caf31fcceb2a9faffd959301c73f4f5b))
