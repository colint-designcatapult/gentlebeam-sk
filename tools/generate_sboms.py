#!/usr/bin/env python3
"""Restore/scan each .NET product, stage static firmware BOMs, and merge graphs.

Requires Python 3.11+, .NET SDK, cdxgen 12.8.4, and CycloneDX CLI 0.33.1.
Run on Windows (the products target WPF): python tools/generate_sboms.py
Firmware sbom.cdx.json files are maintained manually when vendor code changes.
Generated artifacts go to artifacts/sbom; checked-in firmware BOMs are never rewritten.
CI publishes these ten JSON files as the SBOMs artifact on every CI run.
The merged gentlebeam-sk.cdx.json retains nine product roots and their graphs;
shared packages remain scoped to each product by CycloneDX hierarchical merge.
Firmware inventories cover selected compiled sources and linked runtimes, not
post-link symbol retention. Update their evidence, versions and licenses when
vendored sources or external SDK selections change; unknown values are omitted.
"""
from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import tempfile
from pathlib import Path

from resolve_dcdoc_version import resolve_version

ROOT = Path(__file__).resolve().parents[1]
CNC = Path("cnc/voxelray-gentlebeamcnc")
PRODUCTS = {
    "Heracles.Indoor": "Heracles.Indoor/Heracles.Indoor.csproj",
    "Heracles.External": "Heracles.Outdoor/Heracles.External.csproj",
    "Heracles.Ucsi": "Heracles.Ucsi/Heracles.Ucsi.csproj",
}
FIRMWARE = (
    "main-control/bootloader", "main-control/main",
    "hvps-interface/bootloader", "hvps-interface/main",
    "head-interface", "backup-timers",
)


def run(*args: str) -> None:
    executable = shutil.which(args[0])
    if executable is None:
        raise FileNotFoundError(f"Required executable is not on PATH: {args[0]}")
    subprocess.run((executable, *args[1:]), cwd=ROOT, check=True)


def components(bom: dict):
    def walk(component):
        yield component
        for child in component.get("components", []):
            yield from walk(child)
    yield from walk(bom["metadata"]["component"])
    for component in bom.get("components", []):
        yield from walk(component)


def check_graph(bom: dict) -> None:
    refs = [c["bom-ref"] for c in components(bom)]
    if len(refs) != len(set(refs)):
        raise ValueError("Duplicate component bom-ref")
    known = set(refs)
    graph = {d["ref"]: set(d.get("dependsOn", [])) for d in bom["dependencies"]}
    if len(graph) != len(bom["dependencies"]):
        raise ValueError("Duplicate dependency ref")
    if not set(graph) <= known or any(not edges <= known for edges in graph.values()):
        raise ValueError("Dangling dependency reference")
    root = bom["metadata"]["component"]["bom-ref"]
    if not graph.get(root):
        raise ValueError("SBOM root has no dependencies")
    reached, pending = set(), [root]
    while pending:
        ref = pending.pop()
        if ref not in reached:
            reached.add(ref)
            pending.extend(graph.get(ref, ()))
    if reached != known:
        raise ValueError(f"Unreachable components: {known - reached}")


def normalize_scan(bom: dict, name: str, version: str) -> None:
    # With assets-only scanning cdxgen emits a synthetic metadata root separate
    # from the real NuGet project root. Promote the project, retaining its edges.
    roots = [c for c in bom["components"] if c["name"] == name and c["type"] == "application"]
    if len(roots) != 1:
        raise ValueError(f"Expected one resolved project root for {name}")
    root = roots[0]
    bom["components"].remove(root)
    bom["metadata"]["component"] = root
    root["version"] = version
    root.pop("purl", None)  # local application, not a published NuGet package
    check_graph(bom)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, default=ROOT / "artifacts/sbom")
    parser.add_argument("--cdxgen", default="cdxgen", help="Scanner executable path")
    parser.add_argument("--cyclonedx", default="cyclonedx", help="CycloneDX CLI executable path")
    args = parser.parse_args()
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    files = []
    for name, relative in PRODUCTS.items():
        project = ROOT / CNC / "Xcc/Heracles" / relative
        run("dotnet", "restore", str(project), "--runtime", "win-x64", "-p:Configuration=Release")
        assets = project.parent / "obj/project.assets.json"
        if not assets.is_file():
            raise ValueError(f"Restore did not produce {assets}")
        version = resolve_version(ROOT, CNC)
        path = output / f"{name}.cdx.json"
        # Only this resolved closure, not sibling projects, tests, stale build
        # output, or every project.assets.json in the monorepo.
        with tempfile.TemporaryDirectory(prefix="gentlebeam-sbom-") as temp:
            shutil.copy2(assets, Path(temp) / assets.name)
            run(args.cdxgen, "-t", "dotnet", temp, "--no-install-deps",
                "--spec-version", "1.6", "--project-name", name,
                "--project-version", version, "--fail-on-error", "-o", str(path))
        bom = json.loads(path.read_text(encoding="utf-8"))
        normalize_scan(bom, name, version)
        path.write_text(json.dumps(bom, indent=2) + "\n", encoding="utf-8")
        files.append(path)
    for component in FIRMWARE:
        source = ROOT / component / "sbom.cdx.json"
        bom = json.loads(source.read_text(encoding="utf-8"))
        check_graph(bom)
        # Release identity is dynamic; the vendored inventory remains static.
        bom["metadata"]["component"]["version"] = resolve_version(ROOT, Path(component))
        path = output / f"{component.replace('/', '-')}.cdx.json"
        path.write_text(json.dumps(bom, indent=2) + "\n", encoding="utf-8")
        files.append(path)
    for path in files:
        run(args.cyclonedx, "validate", "--input-file", str(path),
            "--input-version", "v1_6", "--fail-on-errors")
    merged = output / "gentlebeam-sk.cdx.json"
    run(args.cyclonedx, "merge", "--input-files", *(str(p) for p in files),
        "--output-file", str(merged), "--output-format", "json", "--output-version", "v1_6",
        "--hierarchical", "--name", "gentlebeam-sk", "--version", resolve_version(ROOT, Path(".")))
    run(args.cyclonedx, "validate", "--input-file", str(merged),
        "--input-version", "v1_6", "--fail-on-errors")
    combined = json.loads(merged.read_text(encoding="utf-8"))
    check_graph(combined)
    merged_edges = {d["ref"]: set(d.get("dependsOn", [])) for d in combined["dependencies"]}
    roots = set()
    for path in files:
        bom = json.loads(path.read_text(encoding="utf-8"))
        root = bom["metadata"]["component"]
        merged_root = next(c for c in combined["components"] if c["name"] == root["name"])
        original_ref = root["bom-ref"]
        merged_ref = merged_root["bom-ref"]
        if not merged_ref.endswith(original_ref):
            raise ValueError(f"Merge lost component identity: {path}")
        # Hierarchical merge namespaces each input's refs with its root identity.
        prefix = merged_ref[:-len(original_ref)]
        roots.add(merged_ref)
        for edge in bom["dependencies"]:
            if merged_edges.get(prefix + edge["ref"]) != {prefix + ref for ref in edge.get("dependsOn", [])}:
                raise ValueError(f"Merge changed dependency ownership: {path}: {edge['ref']}")
    if merged_edges[combined["metadata"]["component"]["bom-ref"]] != roots:
        raise ValueError("Merged root must depend on exactly the nine products")
    print(f"Validated {len(files)} component SBOMs and merged graph: {merged}")


if __name__ == "__main__":
    main()
