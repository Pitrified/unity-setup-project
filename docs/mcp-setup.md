# Unity MCP Setup

The Unity MCP server lets AI agents inspect and modify the open Unity project
through a structured protocol instead of free-form file edits.

---

## 1. What MCP gives us

- Read scene/prefab hierarchy and serialized fields
- Create/destroy GameObjects, attach components
- Set serialized field values
- Trigger Editor methods (build, enter play mode, run tests)
- Read console logs

What MCP **does not** replace:

- Reading `docs/` (still required)
- Source control (still required: commit before MCP edits)
- Visual review in the Editor (still required after each scene change)

---

## 2. Server choice

Use a maintained community server. Recommended candidates (pick one,
document the choice in `tracking.md`):

- `justinpbarnett/unity-mcp` - popular, broad coverage
- `CoderGamester/mcp-unity` - alternative, similar feature set

Whichever is chosen, **pin a specific version**. Do not auto-update.

---

## 3. Install steps (high level)

1. Add the chosen MCP package to `Packages/manifest.json` via Git URL,
   pinned to a tag/commit.
2. Open Unity once so it imports the package.
3. Enable the MCP window/menu the package adds; confirm the listening port.
4. Configure the client (VS Code) to connect - see §4.

The exact package URL goes in `tracking.md` once decided.

---

## 4. VS Code / Copilot client config

`.vscode/mcp.json` in the repo root:

```json
{
  "servers": {
    "unity": {
      "type": "stdio",
      "command": "<path-or-launcher-from-package-readme>",
      "args": []
    }
  }
}
```

(Exact `command`/`args` come from the chosen package's README. The file is
committed so every contributor uses the same wiring.)

Do **not** put secrets in `.vscode/mcp.json`.

---

## 5. Operational rules

These rules are non-negotiable for any MCP-driven session:

1. **Working tree clean** before granting MCP scene/prefab access. Commit or stash first.
2. **One scene per turn.** If a request would touch >1 scene or prefab, split it.
3. **Editor is open and visible** during MCP edits - watch the hierarchy change.
4. **Run PlayMode smoke test** after any scene/prefab edit, before committing.
5. **Diff scene YAML** before committing (`git diff Assets/Scenes/*.unity`). If it
   is incomprehensible, revert and re-do manually.
6. **Never run MCP against `main`** during a release; cut a `feat/` branch.

---

## 6. What MCP must not do

- Modify `ProjectSettings/`
- Modify anything under `Packages/`
- Delete scenes or prefabs
- Bulk-rename assets
- Touch `keystore/`, `*.env`, `Tools/build-android.sh`

If the agent attempts any of the above, halt the session and revert.

---

## 7. Failure modes & recovery

| Symptom                              | Recovery                                         |
| ------------------------------------ | ------------------------------------------------ |
| Scene opens but is empty             | `git checkout -- Assets/Scenes/<name>.unity`     |
| Component refs broken (yellow icons) | Revert scene; re-do edit one component at a time |
| Editor hangs during MCP op           | Kill Editor, `git status` to assess damage       |
| `.meta` files show in diff for moved assets | Expected; commit them with the move        |

---

## 8. Verification checklist (after MCP setup)

- [ ] MCP package pinned in `Packages/manifest.json`
- [ ] `.vscode/mcp.json` present and committed
- [ ] Copilot Chat shows the `unity` MCP server as connected
- [ ] Smoke prompt works: ask the agent "list root GameObjects in Boot scene" and verify the answer matches the actual hierarchy