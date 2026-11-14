---
status: draft
priority: 0
depends_on: [01_repo_split]
description: |
  Rebuild this repo into the Unity guide once the naval game lives in artificial-pirate:
  drop the product, keep a small working sample over the generic systems, and turn the docs
  into setup from zero, a pattern index, the AI and MCP workflow, and distribution.
---

# Rebuild this repo into the Unity guide

Draft spin-off, raised 2026-11-14 with [`01_repo_split`](../01_repo_split/00_start.md). No phases derived.

## Where this came from

In the flutter split the guide clean-up started as the last phase of the split and was superseded by its own folder (`flutter-setup-project` `27_guide_rebuild`), being larger than the rest.
Here it is a separate folder from the start, picked up after `01_repo_split` is done.

## Starting points, to raise as questions when picked up

- The sample: a neutral minimal game (a placeholder avatar on a plane) that exercises scenes, input, save, audio, UI and logging, or the ship slice kept as the sample.
- The save schema, if `01_repo_split` Q9 left it coupled: lift the fix made in artificial-pirate.
- `docs/tracking.md` and `scratch/`: kept as history, and how the README says so.
- The docs' shape: setup from zero for a person and for a session on a box without Unity; a pattern index; which docs stay generic templates (`copilot-instructions-template.md`, the systems spec format).
- How a session bootstrapping a new Unity project is meant to use this repo, written down.
- A headless gate: Unity in batch mode running EditMode tests, whether it is worth a license on a server.
- The project folder name here, `UnitySetupPrpj/`.
