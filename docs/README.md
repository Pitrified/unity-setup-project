# 📚 Project Documentation Guide

All documentation needed to go from **zero → playable demo → private alpha**.

Split by concern: **what we build**, **how we build it**, **how we ship it**.

---

## 🧭 First-time reading order

1. [functional-specs.md](functional-specs.md) - **WHAT** we are building
2. [project-structure.md](project-structure.md) - **WHERE** code lives
3. [coding-standards.md](coding-standards.md) - **HOW** code is written
4. [ai-development-playbook.md](ai-development-playbook.md) - **HOW** we work with AI
5. [dev-environment-setup.md](dev-environment-setup.md) - get it running locally
6. [`plans/`](../plans/) - current work

---

## 📂 Index

### Project & process

- [functional-specs.md](functional-specs.md)
- [project-structure.md](project-structure.md)
- [coding-standards.md](coding-standards.md)
- [git-workflow.md](git-workflow.md)
- [testing-strategy.md](testing-strategy.md)
- [performance-budget.md](performance-budget.md)

### AI workflow

- [ai-development-playbook.md](ai-development-playbook.md)
- [headless-unity.md](headless-unity.md)
- [mcp-setup.md](mcp-setup.md)
- [unity-mcp-best-practices.md](unity-mcp-best-practices.md)
- [copilot-instructions-template.md](copilot-instructions-template.md)

### Build & release

- [build-and-release.md](build-and-release.md)
- [google-play-private-alpha.md](google-play-private-alpha.md)
- [dev-environment-setup.md](dev-environment-setup.md)

### Systems specs

- [systems/game-bootstrap.md](systems/game-bootstrap.md)
- [systems/game-manager.md](systems/game-manager.md)
- [systems/scene-loader.md](systems/scene-loader.md)
- [systems/input-manager.md](systems/input-manager.md)
- [systems/ship-controller.md](systems/ship-controller.md)
- [systems/camera-controller.md](systems/camera-controller.md)
- [systems/save-system.md](systems/save-system.md)
- [systems/audio-manager.md](systems/audio-manager.md)
- [systems/ui-system.md](systems/ui-system.md)
- [systems/logging.md](systems/logging.md)

### Plans and history

- [`plans/`](../plans/) - current work, one folder per feature
- [tracking.md](tracking.md) - the setup checklist from before `plans/`, kept as history

---

## 🧠 Mental model

| Doc set                 | Question it answers      |
| ----------------------- | ------------------------ |
| Functional spec         | What are we building?    |
| Project structure       | Where does code live?    |
| Coding / git / tests    | How do we write code?    |
| AI playbook / MCP       | How do we work with AI?  |
| Build / Play Console    | How do we ship?          |
| Systems specs           | How does each piece work? |

---

## ⚠️ Ground rules

- [functional-specs.md](functional-specs.md) is the **source of truth**.
- Do not implement features not defined there. Update the spec **first**, then code.
- AI-generated code must follow project structure, asmdef boundaries, and the AI playbook.
- Prefer small, incremental changes over large rewrites.

---

## 🔄 Workflow loop

1. Define / update the feature in [functional-specs.md](functional-specs.md) and the relevant `systems/*.md`.
2. Plan it in a `plans/` folder; the agent works it in the loop of [ai-development-playbook.md §3](ai-development-playbook.md#3-the-work-loop).
3. The agent runs the gates ([headless-unity.md](headless-unity.md)) and commits per [git-workflow.md](git-workflow.md).
4. A person plays the build on a device.

---

## 📌 Notes

- Project is in active **v0.1 → v0.2** evolution.
- Update docs **before** implementing, not after.
- If something is unclear in the docs, fix the docs first.
