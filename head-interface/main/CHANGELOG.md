# Changelog

## [4.0.0](https://github.com/colint-designcatapult/gentlebeam-sk/compare/head-interface-3.0.0...head-interface-4.0.0) (2026-08-27)


### ⚠ BREAKING CHANGES

* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* [firmware] added code for LIS2MDL [#12](https://github.com/colint-designcatapult/gentlebeam-sk/issues/12) ([af4a152](https://github.com/colint-designcatapult/gentlebeam-sk/commit/af4a152c1394d83efee8caef98b67d655ae0f711))
* [firmware] I2C hot swappable support for QC board [#12](https://github.com/colint-designcatapult/gentlebeam-sk/issues/12) ([ec13fae](https://github.com/colint-designcatapult/gentlebeam-sk/commit/ec13fae246b587979de1cf3c7920cce388693d5c))
* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/colint-designcatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/colint-designcatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* Import Head Interface docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([5cadcad](https://github.com/colint-designcatapult/gentlebeam-sk/commit/5cadcad749a01b735c9e3cae4075bf9dea3b4630))
* Import head-interface main source and consolidate calibration [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) [#6](https://github.com/colint-designcatapult/gentlebeam-sk/issues/6) ([436abc5](https://github.com/colint-designcatapult/gentlebeam-sk/commit/436abc5ad1784e84119cbd00d5e40e2883568db3))
* import high level system diagram for head interface, backup timer and main control [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([e19efad](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e19efad5a58696a861c5a194ece180984ac865b9))
* import Main control Bootloader docs [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([c48e32d](https://github.com/colint-designcatapult/gentlebeam-sk/commit/c48e32d2cf50038a54c32f3c6859ebcf22106f74))
* Update docs for Head Interface, Backup Timer and Main Control [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) ([00884f4](https://github.com/colint-designcatapult/gentlebeam-sk/commit/00884f4aafcc61e608536b0e5465c88b15587e79))
