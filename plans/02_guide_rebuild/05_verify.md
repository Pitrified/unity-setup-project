---
status: planned
---

# Phase 05 - Verify

## Plan

- A fresh clone of the branch on disk (not `/tmp`, a RAM tmpfs with a quota on this box): `Tools/setup.sh`, `Tools/gates.sh all`, `Tools/build-android.sh dev`, link check, `plans.py check`.
- HUMAN: the `dev` APK on the phone: it installs as "Unity Setup" next to Artificial Pirate, boots to the menu, the capsule moves, pause and resume, and the position is restored after a restart.

## Done when

- The box checks pass in the fresh clone, and the device run is in the log.

## What the implementation found
