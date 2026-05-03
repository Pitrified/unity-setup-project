# Unity MCP Best Practices (Session Learnings)

Project-specific guidance distilled from real MCP runs in this repository.
Use this with:
- [docs/ai-development-playbook.md](ai-development-playbook.md)
- [docs/mcp-setup.md](mcp-setup.md)
- [docs/unity-mcp-guide.md](unity-mcp-guide.md)

## 1. Session control and pacing

- Keep operations small and reversible. Avoid large multi-goal MCP calls.
- Prefer checkpoint workflow: change one thing, verify immediately, then continue.
- Before risky scene or asset edits, ensure there is a clean commit.
- If you need the human to do manual Unity actions, pause with a short yes/no question and resume only after confirmation.
- After Unity crash/reload, assume MCP session routing is stale. Reconfirm Unity readiness before next MCP call.

## 2. Incremental debug workflow for MCP

- Start with read-only checks first:
  - Console snapshot (errors/warnings)
  - Asset existence and path check
  - Scene/component presence check
- For any failing mutation, reduce scope:
  - Run one method call at a time
  - Validate state after each call
  - Avoid chaining stateful editor internals in one execution block
- Re-check console after each meaningful step.

Recommended loop:
1. Observe error and capture exact message.
2. Verify current asset/scene state.
3. Apply one minimal fix.
4. Re-verify via runtime-facing API.
5. Enter Play Mode for smoke test.
6. Confirm console remains clean.

## 3. AudioMixer-specific lessons from this repo

### 3.1 Do not trust visual-only checks

A mixer can look correct in Audio Mixer UI but still fail runtime initialization.
Always validate runtime behavior.

### 3.2 Required runtime validation for MainMixer

Use runtime-facing checks (not only editor internals):
- Load `Assets/Settings/MainMixer.mixer` as `UnityEngine.Audio.AudioMixer`
- Verify group discovery for `Master`, `Music`, `SFX`
- Verify `SetFloat` returns true for:
  - `MasterVol`
  - `MusicVol`
  - `SfxVol`
- Run a short Play Mode smoke test and verify zero console errors

If `SetFloat` is false or console reports `Mixer is not initialized`, treat mixer as invalid.

### 3.3 Creation order matters

For this project/session, these patterns mattered:
- Creating group hierarchy first, then parameter wiring, then runtime validation.
- Some internal editor API calls were unstable when run out of order or in one large batch.
- Runtime validation must happen after save/refresh and before accepting success.

## 4. Known risky MCP patterns (avoid)

- Relying on UnityEditor-only internal types as proof of runtime validity.
- Large reflection-heavy edit scripts that create and wire many editor internals at once.
- Assuming a successful tool call means runtime-safe asset data.
- Continuing edits while console emits repeated background errors.

## 5. Safe recovery pattern for suspicious asset corruption

1. Freeze changes and gather diagnostics:
- Current console errors
- Asset search results
- Runtime-facing load/check outcome

2. Isolate broken asset:
- Move or remove corrupted variants
- Confirm no duplicates remain in target folder

3. Recreate minimal valid asset:
- Rebuild only required structure
- Rebind required references

4. Re-validate runtime:
- API-level success checks
- Play Mode smoke test
- Console clean check

5. Document outcome in scratch/tracking notes.

## 6. Recovery folders after crash

- Crash recovery may create `Assets/_Recovery/...` scenes.
- Treat these as review artifacts, not automatic replacements.
- Inspect hierarchy and decide explicitly whether to keep, merge, or ignore.

## 7. Human-in-the-loop prompts that worked well

When waiting for manual Unity actions, use concise prompts such as:
- "Please complete action X in Unity, then answer Yes to continue."
- "Did error Y reappear after focusing Unity for 10-20 seconds? Yes/No + comment."

This keeps the MCP flow synchronized and prevents acting on stale assumptions.

## 8. Practical runbook for future sessions

1. Confirm Unity instance is active and MCP tools respond.
2. Run read-only probes first.
3. Apply minimal mutation.
4. Validate immediately (state + console).
5. For gameplay-affecting assets, validate with runtime APIs and Play Mode.
6. If instability appears, stop expanding scope and shift to recovery pattern.
7. Record outcomes in scratch docs before moving to next phase.
