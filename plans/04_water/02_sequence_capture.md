---
status: planned
---

# Phase 02 - Capture over time

## Overview

Water is motion: a still says nothing about whether waves travel, barrels bob or ripples spread. The capture tools learn to take frame sequences in Play mode, and the measuring script learns to read them.

## Plan

- A PlayMode harness that loads the game, enters the Game scene, and writes a frame every N frames for a duration from a camera pose, when asked (an environment variable); real time, real physics.
  It runs as a test that also asserts what it can (the scene plays without errors), selected with `-testFilter`, so the gates run it like any other test and captures cost nothing unless asked.
- `measure.py`: `sheet` (frames into a contact sheet), `gif` (frames into an animated GIF to look at), `motion` (mean absolute difference between consecutive frames, per region: is it moving, and where), `track` (a patch's brightness or a bright blob's centroid over time: bobbing, a wave passing).
- `headless-unity.md` and the `build-asset` skill describe the sequence capture and when a still is not enough.

## Done when

- A sequence of the current scene captured and turned into a sheet, a GIF and a motion profile; the avatar's idle scene shows near-zero motion, which is the control.

## What the implementation found
