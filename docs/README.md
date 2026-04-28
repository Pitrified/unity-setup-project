# 📚 Project Documentation Guide

This folder contains all documentation required to go from **zero → playable demo → private alpha release**.

The docs are intentionally split by concern: **what we build**, **how we build it**, and **how we ship it**.

---

## 🧭 Recommended Reading Order (First Time)

If you are new to the project, follow this order:

1. **`functional-specs.md`**
   → Defines *what we are building* (game scope, architecture, systems)

2. **`ai-development-playbook.md`**
   → Defines *how we build it* (AI-first workflow, rules, constraints)

3. **`project-structure.md`**
   → Defines *how the codebase is organized*

4. **`dev-environment-setup.md`**
   → Get the project running locally

---

## 🚀 Development Workflow

During development, use:

* **`functional-specs.md`** → Source of truth for features and architecture
* **`ai-development-playbook.md`** → How to interact with AI tools safely
* **`project-structure.md`** → Where things should live

---

## 📦 Build & Release

When preparing builds:

1. **`build-and-release.md`**
   → How to generate builds (APK/AAB)

2. **`google-play-private-alpha.md`**
   → How to distribute to testers via Play Store

---

## 🧠 Mental Model

* **Functional spec = WHAT**
* **AI playbook = HOW (development process)**
* **Structure = WHERE (code lives)**
* **Build docs = HOW (shipping)**

---

## ⚠️ Ground Rules

* `functional-specs.md` is the **source of truth**
* Do not implement features not defined there
* AI-generated code must follow:

  * project structure
  * architectural constraints
* Prefer small, incremental changes over large rewrites

---

## 🔄 Suggested Workflow Loop

1. Read/define feature in `functional-specs.md`
2. Generate implementation using AI (playbook rules)
3. Place code correctly (`project-structure.md`)
4. Test locally
5. Iterate

---

## 📌 Notes

* This is a **v0.1 → v0.2 evolving project**
* Docs are expected to grow alongside implementation
* Keep documentation updated when making architectural changes

---

👉 If something is unclear, update the docs *before* implementing - not after.
