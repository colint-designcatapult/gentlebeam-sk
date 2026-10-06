"""Explicit observations used by a test's protocol assertions."""

from __future__ import annotations

from pprint import pformat


class Evidence:
    """Snapshot selected values now; report them once the test has finished."""

    def __init__(self) -> None:
        self.records: list[str] = []

    def record(self, phase: str, **values: object) -> None:
        """Retain observed and expected values without rereading any device."""
        self.records.append(f"{phase}: {pformat(values, sort_dicts=False, width=110)}")
