#!/usr/bin/env python3
"""Print flash and RAM usage (with percentage of available memory) for an ELF."""

from __future__ import annotations

import argparse
import subprocess
import sys


def parse_berkeley_size(size_tool: str, elf: str) -> tuple[int, int, int, str]:
    output = subprocess.check_output([size_tool, elf], text=True)
    lines = [line for line in output.splitlines() if line.strip()]
    if len(lines) < 2:
        raise ValueError(f"unexpected output from '{size_tool}':\n{output}")
    text, data, bss = (int(value) for value in lines[1].split()[:3])
    return text, data, bss, output


def format_row(name: str, used: int, total: int) -> str:
    percent = (used / total * 100) if total else 0.0
    return f"{name:<6} {used:>10} / {total:<10} bytes  {percent:5.1f}%"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--size-tool", required=True, help="path to arm-none-eabi-size")
    parser.add_argument("--elf", required=True, help="path to the built ELF file")
    parser.add_argument("--flash-size", required=True, type=int, help="flash region size in bytes")
    parser.add_argument("--ram-size", required=True, type=int, help="RAM region size in bytes")
    parser.add_argument("--name", default=None, help="label printed alongside the report")
    args = parser.parse_args()

    text, data, bss, raw_output = parse_berkeley_size(args.size_tool, args.elf)
    flash_used = text + data
    ram_used = data + bss

    if args.name:
        print(f"Memory usage for {args.name}:")
    print(raw_output, end="" if raw_output.endswith("\n") else "\n")
    print(format_row("Flash:", flash_used, args.flash_size))
    print(format_row("RAM:", ram_used, args.ram_size))
    return 0


if __name__ == "__main__":
    sys.exit(main())
