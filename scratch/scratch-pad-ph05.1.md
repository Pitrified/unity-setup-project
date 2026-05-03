# Scratch Pad - artificial-pi - Phase 5 - scenes

implementing phase 5 from [tracking.md](docs/tracking.md)

more detailed notes, results, manual steps required, etc go here before being distilled into the final PRs and docs updates

gather additional context before implementing (eg, review relevant docs, load mcp usage guides, etc)

## Scene Creation

- [x] HUMAN: create `Boot.unity`, `Persistent.unity`, `Menu.unity`, `Game.unity` (commit empty) -- done via MCP 2026-05-03

## wire boot scene

- [ ] AI: wire `Boot.unity` (single `GameBootstrap`)

## wire persistent scene

- [ ] AI: wire `Persistent.unity` (GameManager, all systems, UIRoot)

## wire menu scene

- [ ] AI: wire `Menu.unity` (Play / Continue / Quit buttons routed through GameManager)

## wire game scene

- [ ] AI: wire `Game.unity` minimum (sea plane, ship spawn, camera)

## register scenes

- [ ] AI: register all scenes in Build Settings (Boot first)

## test in editor

- [ ] HUMAN: press Play in `Boot.unity` - expect Menu reachable, no console errors
