# Unity MCP

MCP lets an agent drive an **open Unity Editor**: read the hierarchy and the console, create and change GameObjects, set serialized fields, enter Play mode.
It is the interactive option, for when a person is at the Editor on a workstation.
The agent's default way into Unity is batch mode ([headless-unity.md](headless-unity.md)), which needs no open Editor and leaves an editor script in the repo instead of a one-off edit.

Use MCP when the change is easier to see than to script: placing and tuning objects in a scene with a person watching, reading a live console during Play mode.

---

## 1. Server

**MCP for Unity** by CoplayDev (MIT), package `com.coplaydev.unity-mcp`, pinned in `Packages/manifest.json` to commit `b92c05a2`.
Do not move the pin without a plan; an unpinned Git URL changes under you.

The Unity package starts an HTTP server inside the Editor; the client connects to it.
It was chosen in May 2026 over Unity's own `com.unity.ai.assistant` (pre-release then) and IvanMurzak's Unity-MCP (stdio binary, in-game tools this project does not need) for its tool coverage. That comparison is in this file's git history.

---

## 2. Setup

1. Open the project in the Editor. The package is already in the manifest.
2. **Window → MCP for Unity → Start Server**. It listens on `localhost:8080`; the status turns green. Restart it after the Editor restarts, or enable auto-start in the panel.
3. Register it with Claude Code, once per machine:
   ```
   claude mcp add --transport http unityMCP http://localhost:8080/mcp
   ```
   The panel's client dropdown can also write this configuration.
4. Check: ask the agent to list the root GameObjects of `Boot.unity` and compare with the Hierarchy.

Start the Editor and the server before the agent session, so the tools are there when it connects.
The project path must have no spaces.

---

## 3. Rules

1. **Clean working tree** before any MCP scene or prefab edit, so `git checkout` undoes it.
2. **One scene or prefab per step**, checked in the Hierarchy before the next.
3. **PlayMode tests after** any scene or prefab change, before committing ([headless-unity.md §3](headless-unity.md#3-gates), or the Test Runner).
4. **Read the scene YAML diff** before committing. If it is incomprehensible, revert and redo the change as an editor script.
5. A change worth keeping as a repeatable step becomes an editor script, not an MCP sequence.

MCP must not:

- change `ProjectSettings/` or `Packages/`;
- delete or bulk-rename scenes, prefabs or assets;
- touch `keystore/`, `build.env`, `Tools/build-android.sh`.

Session lessons (debugging loop, AudioMixer, recovery after a crash) are in [unity-mcp-best-practices.md](unity-mcp-best-practices.md).

---

## 4. Failure modes

| Symptom | Recovery |
| ------- | -------- |
| Tools missing in the agent | Server not started, or the Editor recompiled and stopped it: Start Server, then reconnect the client |
| `curl http://localhost:8080/mcp` fails | Same; check the panel status |
| Scene opens empty | `git checkout -- <scene>` |
| Component references broken | Revert the scene, redo one component at a time |
| Editor hangs during an MCP call | Kill the Editor, `git status` to see what changed |
