#!/usr/bin/env python3
"""Create a release ZIP from explicitly mapped files and directory trees."""

from __future__ import annotations

import argparse
import zipfile
from pathlib import Path, PurePosixPath
from typing import Iterable


def parse_mapping(value: str) -> tuple[Path, str]:
    source, separator, archive_path = value.partition("=")
    if not separator or not source:
        raise argparse.ArgumentTypeError(
            f"expected SOURCE=ARCHIVE_PATH, received {value!r}"
        )
    return Path(source), archive_path


def normalize_archive_path(value: str, *, allow_empty: bool) -> str:
    normalized = value.replace("\\", "/").strip("/")
    if not normalized:
        if allow_empty:
            return ""
        raise ValueError("archive path must not be empty")

    path = PurePosixPath(normalized)
    if path.is_absolute() or any(part in ("", ".", "..") for part in path.parts):
        raise ValueError(f"archive path must be relative and normalized: {value!r}")
    return path.as_posix()


def collect_entries(
    files: Iterable[tuple[Path, str]], trees: Iterable[tuple[Path, str]]
) -> list[tuple[str, Path]]:
    entries: dict[str, Path] = {}

    def add(source: Path, archive_path: str) -> None:
        if archive_path in entries:
            raise ValueError(f"duplicate archive path: {archive_path}")
        entries[archive_path] = source

    for source, requested_path in files:
        if not source.is_file():
            raise FileNotFoundError(f"release file does not exist: {source}")
        add(source, normalize_archive_path(requested_path, allow_empty=False))

    for source_directory, requested_prefix in trees:
        if not source_directory.is_dir():
            raise FileNotFoundError(
                f"release tree does not exist or is not a directory: {source_directory}"
            )
        prefix = normalize_archive_path(requested_prefix, allow_empty=True)
        for source in sorted(path for path in source_directory.rglob("*") if path.is_file()):
            relative_path = source.relative_to(source_directory).as_posix()
            archive_path = f"{prefix}/{relative_path}" if prefix else relative_path
            add(source, archive_path)

    if not entries:
        raise ValueError("release package must contain at least one file")
    return sorted(entries.items())


def package_release(
    output: Path,
    files: Iterable[tuple[Path, str]] = (),
    trees: Iterable[tuple[Path, str]] = (),
) -> None:
    entries = collect_entries(files, trees)
    output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(output, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        for archive_path, source in entries:
            archive.write(source, archive_path)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True, metavar="ZIP")
    parser.add_argument(
        "--file",
        action="append",
        default=[],
        type=parse_mapping,
        metavar="SOURCE=ARCHIVE_PATH",
    )
    parser.add_argument(
        "--tree",
        action="append",
        default=[],
        type=parse_mapping,
        metavar="SOURCE_DIR=ARCHIVE_PREFIX",
    )
    arguments = parser.parse_args()
    try:
        package_release(arguments.output, arguments.file, arguments.tree)
    except (FileNotFoundError, ValueError) as error:
        parser.error(str(error))


if __name__ == "__main__":
    main()
