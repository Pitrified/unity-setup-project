# Scratch Pad - artificial-pi - Phase 6 - scenes

implementing phase 6 from [tracking.md](docs/tracking.md)

more detailed notes, results, manual steps required, etc go here before being distilled into the final PRs and docs updates

gather additional context before implementing (eg, review relevant docs, load mcp usage guides, etc)

## Relevant docs by task

| Task  | Doc reference  |
| --- | ----- |
...

## ship controller

- [ ] AI: implement [systems/ship-controller.md](systems/ship-controller.md) (+ tests for input clamping)

## camera controller

- [ ] AI: implement [systems/camera-controller.md](systems/camera-controller.md)

## stylized water shader

- [ ] AI: create stylized water shader (URP Shader Graph) - minimal animation

## island prefab

- [ ] AI: create island prefab (static mesh, no collisions in v0.2)

## UI overlay

- [ ] AI: implement on-screen virtual stick + pause button (UI Toolkit overlay)

## SaveSystem integration

- [ ] AI: wire SaveSystem into GameManager pause/quit and Menu Continue button

## Manual test: gameplay vertical slice

- [ ] HUMAN: play in Editor - Menu → Game → drive → Pause → Resume → Menu → Continue restores position
