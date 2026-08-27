# Changelog

## [4.0.0](https://github.com/colint-designcatapult/gentlebeam-sk/compare/hvps-interface-bootloader-3.0.0...hvps-interface-bootloader-4.0.0) (2026-08-27)


### ⚠ BREAKING CHANGES

* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/colint-designcatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/colint-designcatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* Import HVPS bootloader and main sources, docs, and consolidate calibraion [#4](https://github.com/colint-designcatapult/gentlebeam-sk/issues/4) [#5](https://github.com/colint-designcatapult/gentlebeam-sk/issues/5) [#6](https://github.com/colint-designcatapult/gentlebeam-sk/issues/6) ([e84adfd](https://github.com/colint-designcatapult/gentlebeam-sk/commit/e84adfd020506d1664c40cce715e9c63060144e0))


### Bug Fixes

* **bootloader:** increase application page capacity [GBSK-47] ([#60](https://github.com/colint-designcatapult/gentlebeam-sk/issues/60)) ([7b009d8](https://github.com/colint-designcatapult/gentlebeam-sk/commit/7b009d81d7d0fd729a23435b24eb85663773c509))
