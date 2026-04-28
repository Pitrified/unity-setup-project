# Development Environment Setup

## 1. Overview

This document defines a **repeatable setup process** to get any developer from zero to a working local Unity + Android build.

Target:

- One-command mental model: _clone → open → run_

---

## 2. Required Software

### Core Tools

- Unity Hub
- Unity Editor (Latest LTS)
- Visual Studio Code
- Git

---

### Android Toolchain

Installed via Unity Hub:

- Android Build Support
- SDK + NDK
- OpenJDK

---

## 3. Repository Setup

```bash
git clone <repo-url>
cd <repo>
```

Open via Unity Hub:

- Add project
- Select correct Unity version

---

## 4. First Run

Steps:

1. Open project
2. Let Unity import assets
3. Open Boot Scene
4. Press Play

Expected:

- App flows to menu without errors

---

## 5. Local Android Build

Steps:

1. Switch platform → Android
2. Connect device (USB debugging enabled)
3. Build & Run

---

## 6. Environment Validation Checklist

- Unity opens without errors
- Scenes load correctly
- Android build succeeds
- App launches on device

---

## 7. Common Issues

### Gradle / SDK issues

- Reinstall Android modules via Unity Hub

### Device not detected

- Enable USB debugging
- Install drivers (Windows)

---

## 8. Future Improvements

- One-click setup script
- Dev container (optional)
- CI validation (later phase)

---
