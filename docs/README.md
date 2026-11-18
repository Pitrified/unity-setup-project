# Documentation

The guide's docs: how a project built from this repo is set up, written, checked and shipped.
What the repo is, and the patterns linked from other projects, are in the [root README](../README.md).

---

## Reading order

1. [bootstrap-new-project.md](bootstrap-new-project.md) - starting a new project from this repo
2. [functional-specs.md](functional-specs.md) - **what** the sample is, and the spec's shape a project copies
3. [project-structure.md](project-structure.md) - **where** code lives, asmdef boundaries
4. [coding-standards.md](coding-standards.md) - **how** code is written
5. [ai-development-playbook.md](ai-development-playbook.md) - **how** the agent works: plans, the loop, the gates
6. [headless-unity.md](headless-unity.md) - running Unity in batch mode, `Tools/gates.sh`, pitfalls
7. [dev-environment-setup.md](dev-environment-setup.md) - a workstation from zero
8. [`plans/`](../plans/) - current work

---

## Index

### Starting and structure

- [bootstrap-new-project.md](bootstrap-new-project.md)
- [functional-specs.md](functional-specs.md)
- [project-structure.md](project-structure.md)
- [coding-standards.md](coding-standards.md)
- [git-workflow.md](git-workflow.md)
- [testing-strategy.md](testing-strategy.md)
- [performance-budget.md](performance-budget.md)
- [building_assets.md](building_assets.md) - models from a reference photograph: Blender builders, FBX, passes

### AI workflow

- [ai-development-playbook.md](ai-development-playbook.md)
- [headless-unity.md](headless-unity.md)
- [mcp-setup.md](mcp-setup.md)
- [unity-mcp-best-practices.md](unity-mcp-best-practices.md)

The instructions themselves are `.github/copilot-instructions.md`, imported by `CLAUDE.md`; a new project copies and adapts them.

### Build and release

- [dev-environment-setup.md](dev-environment-setup.md)
- [build-and-release.md](build-and-release.md)
- [google-play-private-alpha.md](google-play-private-alpha.md)

### Runtime systems

- [systems/game-bootstrap.md](systems/game-bootstrap.md)
- [systems/game-manager.md](systems/game-manager.md)
- [systems/scene-loader.md](systems/scene-loader.md)
- [systems/input-manager.md](systems/input-manager.md)
- [systems/avatar-controller.md](systems/avatar-controller.md) - the sample's
- [systems/camera-controller.md](systems/camera-controller.md)
- [systems/save-system.md](systems/save-system.md)
- [systems/audio-manager.md](systems/audio-manager.md)
- [systems/ui-system.md](systems/ui-system.md)
- [systems/logging.md](systems/logging.md)

### Plans and history

- [`plans/`](../plans/) - current work, one folder per feature
- [tracking.md](tracking.md) - the setup checklist from before `plans/`, history of how the runtime and the naval game were built

---

## Ground rules

- [functional-specs.md](functional-specs.md) is the **source of truth** for what the sample does.
- Do not implement features not defined there. Update the spec **first**, then code.
- Code follows the project structure, the asmdef boundaries and the playbook, whoever writes it.
- Prefer small, incremental changes over large rewrites.

## Workflow loop

1. Define or update the feature in [functional-specs.md](functional-specs.md) and the relevant `systems/*.md`.
2. Plan it in a `plans/` folder; the agent works it in the loop of [ai-development-playbook.md §3](ai-development-playbook.md#3-the-work-loop).
3. The agent runs the gates (`Tools/gates.sh all`, [headless-unity.md](headless-unity.md)) and commits per [git-workflow.md](git-workflow.md).
4. A person plays the build on a device.
