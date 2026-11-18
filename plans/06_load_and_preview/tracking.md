# Scene-load allocations and monospace preview - implementation tracking

Two leftovers of the diagnostics work. Brainstorm and decisions in [`00_start.md`](00_start.md).

## Phases

| #  | Phase                    | Plan                                       | Status      |
| -- | ------------------------ | ------------------------------------------ | ----------- |
| 01 | Scene-load allocations   | [`01_load_allocations.md`](01_load_allocations.md) | done        |
| 02 | Monospace preview        | [`02_monospace.md`](02_monospace.md)       | in progress |

Status values: draft / planned / in progress / done / superseded / discarded.

## Log

Append-only. Newest at the bottom.

- 2026-11-18 : asked by the user; plan written; branch `feat/06_load_and_preview` from `main` at `b0c30c1`.
- 2026-11-18 : phase 01 - loads profiled in the Linux player and read in the Editor: 4.8 MB, Unity's one-off setup (URP's first frame, UI Toolkit text and panels, JIT); 17 kB of game code. Nothing to fix. The scan now runs on the Mobile quality level (the PC renderer logged an SSAO error every frame). Gates green.
