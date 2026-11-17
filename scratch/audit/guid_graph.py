#!/usr/bin/env python3
"""Who references whom in the Unity project, by `.meta` GUID.

Phase 02 of plans/01_repo_split. Reads tracked files only.

    python3 scratch/audit/guid_graph.py            # every asset with its referrers
    python3 scratch/audit/guid_graph.py --orphans  # assets nothing references

A `.cs` file is used by type name as well as by GUID (code calls code), so an
unreferenced script is not dead by this report alone: read it.
"""

from __future__ import annotations

import argparse
import re
import subprocess
from collections import defaultdict
from pathlib import Path

PROJECT = "UnitySetupPrpj/"
# Files whose YAML or JSON carries GUID references to other assets.
REFERRING = (".unity", ".prefab", ".asset", ".mat", ".asmdef", ".inputactions",
             ".mixer", ".shader", ".uxml", ".uss", ".tss", ".controller", ".anim")
GUID_REF = re.compile(r"(?:guid: |guid=|GUID:)([0-9a-f]{32})")
PATH_REF = re.compile(r'src="(?!project:)([^"?]+)"')
META_GUID =re.compile(r"^guid: ([0-9a-f]{32})$", re.MULTILINE)


def tracked(root: Path) -> list[str]:
    out = subprocess.run(["git", "-C", str(root), "ls-files", PROJECT],
                         capture_output=True, text=True, check=True).stdout
    return out.splitlines()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--orphans", action="store_true")
    args = parser.parse_args()

    root = Path(subprocess.run(["git", "rev-parse", "--show-toplevel"],
                               capture_output=True, text=True, check=True).stdout.strip())
    files = tracked(root)

    asset_of: dict[str, str] = {}
    for f in files:
        if f.endswith(".meta"):
            m = META_GUID.search((root / f).read_text(encoding="utf-8", errors="replace"))
            if m:
                asset_of[m.group(1)] = f[: -len(".meta")]

    referrers: dict[str, set[str]] = defaultdict(set)
    for f in files:
        if not f.endswith(REFERRING):
            continue
        text = (root / f).read_text(encoding="utf-8", errors="replace")
        for guid in set(GUID_REF.findall(text)):
            if guid in asset_of and asset_of[guid] != f:
                referrers[asset_of[guid]].add(f)
        # UXML can also reference a stylesheet or template by a path relative to itself.
        if f.endswith(".uxml"):
            for src in PATH_REF.findall(text):
                target = (Path(f).parent / src).as_posix()
                if (root / target).is_file():
                    referrers[target].add(f)

    folders = {a for a in asset_of.values() if not Path(root / a).is_file()}
    for asset in sorted(asset_of.values()):
        if asset in folders:
            continue
        refs = sorted(referrers.get(asset, ()))
        if args.orphans:
            if not refs:
                print(asset)
        else:
            print(asset)
            for r in refs:
                print(f"    <- {r}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
