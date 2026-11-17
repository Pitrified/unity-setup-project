#!/usr/bin/env python3
"""Check that the audit table gives every tracked path exactly one destination.

Phase 02 of plans/01_repo_split.

    python3 scratch/audit/coverage.py [table.md]

A table row is `| `pattern` [`pattern` ...] | destination | reason |`.
Patterns are fnmatch globs over repo-relative paths (`*` crosses `/`).
When several patterns match a path, the longest pattern wins; two winners of
the same length with different destinations are an error.
Fails on: an unmatched path, a pattern that matches nothing (a `drop` pattern may: its files are gone),
a tie, an unknown destination.
"""

from __future__ import annotations

import fnmatch
import re
import subprocess
import sys
from pathlib import Path

DESTINATIONS = {"guide", "pirate", "both", "drop"}
ROW = re.compile(r"^\|(?P<patterns>[^|]*`[^|]*)\|\s*(?P<dest>[a-z]+)\s*\|")
TICKED = re.compile(r"`([^`]+)`")


def main() -> int:
    root = Path(subprocess.run(["git", "rev-parse", "--show-toplevel"],
                               capture_output=True, text=True, check=True).stdout.strip())
    table = Path(sys.argv[1]) if len(sys.argv) > 1 else root / "plans/01_repo_split/02.1_audit_table.md"
    rows: list[tuple[str, str, int]] = []
    errors: list[str] = []
    for n, line in enumerate(table.read_text(encoding="utf-8").splitlines(), 1):
        m = ROW.match(line)
        if not m:
            continue
        dest = m.group("dest")
        if dest not in DESTINATIONS:
            errors.append(f"{table.name}:{n}: unknown destination '{dest}'")
        for pattern in TICKED.findall(m.group("patterns")):
            rows.append((pattern, dest, n))

    paths = subprocess.run(["git", "-C", str(root), "ls-files"],
                           capture_output=True, text=True, check=True).stdout.splitlines()
    used: set[str] = set()
    unmatched = 0
    for path in paths:
        hits = [(p, d, n) for p, d, n in rows if fnmatch.fnmatchcase(path, p)]
        if not hits:
            errors.append(f"unmatched: {path}")
            unmatched += 1
            continue
        best = max(len(p) for p, _, _ in hits)
        winners = {(p, d, n) for p, d, n in hits if len(p) == best}
        if len({d for _, d, _ in winners}) > 1:
            errors.append(f"tie: {path} <- " + ", ".join(f"{p} ({d}, line {n})" for p, d, n in sorted(winners)))
        used.update(p for p, _, _ in hits)

    gone = 0
    for pattern, dest, n in rows:
        if pattern in used:
            continue
        if dest == "drop":
            gone += 1  # deleted by the cleanup: the row stays as the record
        else:
            errors.append(f"{table.name}:{n}: pattern matches nothing: {pattern}")
    if gone:
        print(f"{gone} drop patterns match nothing: already removed")

    for e in errors:
        print(e)
    print(f"{len(paths)} paths, {len(rows)} patterns, {unmatched} unmatched, {len(errors)} errors")
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
