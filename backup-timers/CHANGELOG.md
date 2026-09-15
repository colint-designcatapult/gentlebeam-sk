# Changelog

## [5.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/backup-timers-4.0.0...backup-timers-5.0.0) (2026-09-15)


### ⚠ BREAKING CHANGES

* **sqlite:** `SqliteGrpcServerHost` and `SqliteProtoRepository` constructors now require a `SqlCipherConnectionFactory` instead of a database path, so existing integrations must update their construction and encrypted database initialization.

### Features

* **sqlite:** add encrypted database management and security automation [GBSK-91] ([#92](https://github.com/DesignCatapult/gentlebeam-sk/issues/92)) ([29cb845](https://github.com/DesignCatapult/gentlebeam-sk/commit/29cb84500fe760f3050056b7806217848c68739e))

## [4.0.0](https://github.com/DesignCatapult/gentlebeam-sk/compare/backup-timers-3.0.0...backup-timers-4.0.0) (2026-09-03)


### ⚠ BREAKING CHANGES

* **ci:** The VersionInfo model and version-info UDP packet changed from numeric version fields and a 5-word payload to string-based main/HVPS version data and a 19-word payload; existing producers and consumers must be updated.

### Features

* **ci:** add release infrastructure [GBSK-36] ([#56](https://github.com/DesignCatapult/gentlebeam-sk/issues/56)) ([699beeb](https://github.com/DesignCatapult/gentlebeam-sk/commit/699beeb196908f87d0c79e44a06e7290d1edd650))
* Import Backup Timer docs [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([86204ef](https://github.com/DesignCatapult/gentlebeam-sk/commit/86204efbc56e341faed0c4d56384ff9209bb2686))
* Import backup-timers-fw [#5](https://github.com/DesignCatapult/gentlebeam-sk/issues/5) ([13655b6](https://github.com/DesignCatapult/gentlebeam-sk/commit/13655b666b105e7fd273199821efa19902fdb1cb))
* import high level system diagram for head interface, backup timer and main control [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([e19efad](https://github.com/DesignCatapult/gentlebeam-sk/commit/e19efad5a58696a861c5a194ece180984ac865b9))
* Update docs for Head Interface, Backup Timer and Main Control [#4](https://github.com/DesignCatapult/gentlebeam-sk/issues/4) ([00884f4](https://github.com/DesignCatapult/gentlebeam-sk/commit/00884f4aafcc61e608536b0e5465c88b15587e79))
