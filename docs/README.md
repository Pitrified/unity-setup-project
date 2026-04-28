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
6. [tracking.md](tracking.md) - current setup roadmap (start here for action items)

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
- [mcp-setup.md](mcp-setup.md)
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

### Tracking

- [tracking.md](tracking.md) - checklist of what's left to set up / build

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
2. Write a focused prompt (see [ai-development-playbook.md §6](ai-development-playbook.md#6-prompting-pattern)).
3. Review the diff against the [§7 checklist](ai-development-playbook.md#7-review-checklist-per-ai-diff).
4. Run tests; verify in Editor and on device.
5. Commit per [git-workflow.md](git-workflow.md). Update [tracking.md](tracking.md).

---

## 📌 Notes

- Project is in active **v0.1 → v0.2** evolution.
- Update docs **before** implementing, not after.
- If something is unclear in the docs, fix the docs first.
