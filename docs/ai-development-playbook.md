# ai-development-playbook.md

# AI Development Playbook

## 1. Philosophy

Goal:

> AI writes ~80-90% of code, **human enforces architecture and correctness**

Rules:

- AI generates → human validates
- Never trust generated scene/prefab blindly
- Always preserve architectural boundaries

---

## 2. Tooling Stack

Primary tools:

- Visual Studio Code
- GitHub Copilot (agent mode)
- Unity MCP Server

---

## 3. MCP Integration Strategy

### Goal

Allow AI agents to:

- Read Unity project state
- Modify scenes and prefabs
- Generate scripts with context awareness

---

### Capabilities (target)

- Create GameObjects
- Attach components
- Modify serialized fields
- Navigate scene hierarchy

---

### Risks

- Corrupt scenes/prefabs
- Hidden side effects
- Hard-to-debug changes

---

### Mitigation Strategy

#### 1. Strict Guidelines

Define:

- Naming conventions
- Component ownership rules
- Scene boundaries

---

#### 2. Small Atomic Changes

AI must:

- Modify one system at a time
- Avoid large refactors in one step

---

#### 3. Review Loop

Every AI action:

1. Generate
2. Inspect diff
3. Test in Unity

---

#### 4. Scene Safety Rules

- No mass deletion
- No restructuring without explicit instruction
- Prefer additive changes

---

## 4. AI Usage Levels

### Level 1 - Code Generation

- Scripts
- Helpers
- Boilerplate

---

### Level 2 - System Design

- GameManager
- State machines
- Input abstraction

---

### Level 3 - Scene Editing (enabled with MCP)

- Create objects
- Wire components

⚠️ Always review changes in Unity Editor

---

## 5. VSCode + Copilot Setup

Recommended setup:

- Enable Copilot Chat + Agent mode
- Use workspace-level instructions
- Use structured prompts

---

### Copilot Instruction Pattern

Example:

```
You are working on a Unity URP mobile game.

Constraints:
- Kinematic movement
- No physics-based systems
- Mobile performance first
- Clean architecture

Task:
Create a ShipController script with forward thrust and rotation.
```

---

## 6. Project-Level AI Rules

### DO

- Generate small, composable systems
- Ask AI for alternatives before choosing
- Use AI for refactoring suggestions

---

### DO NOT

- Let AI redesign architecture mid-project
- Accept large unreviewed diffs
- Mix responsibilities in scripts

---

## 7. File & System Boundaries

Each system should be:

- Small
- Independent
- Replaceable

Examples:

- ShipController
- CameraController
- GameManager
- InputHandler

---

## 8. Prompting Strategy

Use:

- Context (project constraints)
- Clear task
- Expected output format

Avoid:

- Vague prompts
- Multi-system requests

---

## 9. Debugging with AI

Use AI to:

- Analyze logs
- Suggest fixes
- Explain Unity errors

But:

- Always verify in editor/runtime

---

## 10. Build & Release with AI

AI can:

- Generate build scripts
- Document release steps

Human must:

- Execute builds
- Upload to Play Console

---

## 11. Long-Term Evolution

Future upgrades:

- CI/CD pipeline
- Automated testing
- AI-assisted QA
