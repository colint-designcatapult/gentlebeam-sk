#!/usr/bin/env python3
"""Resolve a component's inherited dcdoc release version."""

from __future__ import annotations

import argparse
import os
import subprocess
import tomllib
from pathlib import Path


def git_hash(root: Path) -> str:
    github_hash = os.environ.get("GITHUB_SHA")
    if github_hash:
        return github_hash[:8]
    try:
        return subprocess.check_output(
            ["git", "-C", str(root), "rev-parse", "--short=8", "HEAD"],
            text=True,
            stderr=subprocess.DEVNULL,
        ).strip()
    except (OSError, subprocess.CalledProcessError):
        return "local"


def read_version(path: Path) -> str | None:
    with path.open("rb") as file:
        document = tomllib.load(file)
    for section in ("project", "component"):
        version = document.get(section, {}).get("version")
        if version is not None:
            if not isinstance(version, str):
                raise ValueError(f"{path}: {section}.version must be a string")
            return version
    return None


def resolve_version(root: Path, component: Path) -> str:
    root = root.resolve()
    component_path = (root / component).resolve()
    if root not in (component_path, *component_path.parents):
        raise ValueError(f"component '{component}' is outside repository root '{root}'")

    version: str | None = None
    directory = root
    for part in component_path.relative_to(root).parts:
        version_file = directory / "dcdoc.toml"
        if version_file.is_file():
            version = read_version(version_file) or version
        directory /= part
    version_file = directory / "dcdoc.toml"
    if version_file.is_file():
        version = read_version(version_file) or version

    if version is None:
        raise ValueError(f"no version found for component '{component}'")

    return version.replace("{HASH}", git_hash(root)).replace(
        "{BUILD}", os.environ.get("GITHUB_RUN_NUMBER", "0")
    )


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--component", type=Path, required=True)
    arguments = parser.parse_args()
    print(resolve_version(arguments.root, arguments.component))


if __name__ == "__main__":
    main()
