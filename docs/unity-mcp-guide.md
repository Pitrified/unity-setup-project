# Unity MCP: Complete Setup Guide
> Ubuntu + VSCode + GitHub Copilot

---

## 1. What Is MCP?

**Model Context Protocol (MCP)** is an open standard that lets AI assistants (LLMs) talk to external tools in a structured, secure way. Think of it as USB-C for AI: a universal plug that lets any compliant AI client connect to any compliant external system.

Without MCP, an AI can only respond with text. With MCP, it can *act* - reading files, calling APIs, or in this case, directly controlling the Unity Editor.

### Core concepts

| Concept | What it is |
|---|---|
| **MCP Client** | The AI chat interface you talk to (VSCode Copilot, Claude Code, Cursor…) |
| **MCP Server** | A process that bridges the client to an external system (Unity, in this case) |
| **MCP Tool** | A callable function exposed by the server. The LLM decides when to invoke it. |
| **MCP Resource** | Read-only data the LLM can inspect (scene state, asset list…) |
| **MCP Prompt** | Injected instructions/context that shape LLM behavior for a session |

### How a request flows

```
You (chat) → MCP Client (VSCode Copilot)
                  │ MCP protocol (stdio or HTTP)
             MCP Server (runs on your Ubuntu box)
                  │ IPC / HTTP
             Unity Editor (plugin receives commands)
                  │
             SceneManager, AssetDatabase, etc.
```

When you say *"add a point light above the player"*, the LLM picks the right MCP Tool, the server calls it, and Unity executes it - no copy-pasting required.

---

## 2. The Three Implementations

You linked to three distinct things. Here's what each is.

### 2.1 Unity Official MCP - `com.unity.ai.assistant`

**What it is:** Unity's own MCP implementation, shipped as part of the `AI Assistant` package (requires Unity 6000.0+, currently pre-release `2.0.0-pre.1`).

**Architecture:** When Unity opens, it auto-starts a local *MCP Bridge* (a Unix socket on Linux). A small *relay binary* is installed to `~/.unity/relay/`. MCP clients launch this relay binary, which speaks MCP over stdio and forwards everything to the Bridge inside Unity.

```
MCP Client → relay binary (~/.unity/relay/relay_linux) → Unix socket → Unity Editor Bridge
```

**Built-in tools:** Scene management, asset operations, script editing, console access. You can also register custom tools via C# attributes.

**Security:** First connection from an external client requires manual approval in *Project Settings → AI → Unity MCP*. Subsequent connections auto-approve.

**Verdict:** Official, tightly integrated, zero external dependencies. But Unity 6+ only and still pre-release.

---

### 2.2 IvanMurzak/Unity-MCP - "AI Game Developer"

**What it is:** A community package (Apache-2.0, ~1.2k stars) that predates Unity's official solution and works back to Unity 2021+. It has two parts:

- **Unity Plugin** (`com.ivanmurzak.unity.mcp`) - installed into your Unity project
- **MCP Server binary** - auto-built into `<project>/Library/mcp-server/linux-x64/unity-mcp-server`

The server communicates with the plugin via HTTP on a configurable port (default 8080). The MCP client connects to the server via stdio or HTTP.

**Standout features:**
- 50+ built-in tools (scene, assets, scripting, editor state, screenshot, tests…)
- **Runtime (in-game) support** - you can expose MCP tools that run inside your compiled game, not just in the editor
- C# Roslyn for dynamic script compilation/execution
- Reflection-based method discovery and invocation across your whole codebase
- Docker support for remote/cloud hosting
- Extension packages: Animation, ParticleSystem, ProBuilder

**Verdict:** More mature, more tools, works on older Unity. The runtime in-game angle is unique. Slightly more moving parts than the official solution.

---

### 2.3 CoplayDev/Unity-MCP - "MCP for Unity"

**What it is:** Another community package (MIT), backed by a company (Coplay) that makes a paid Unity AI assistant. The MCP server is a **Python package** (`mcpforunityserver`, installable via `uvx`).

**Architecture:** The Unity plugin starts an HTTP server on `localhost:8080`. The MCP client hits that HTTP endpoint directly (streamable HTTP transport), or you can use uvx to run a stdio adapter.

**Standout features:**
- 40+ tools including `manage_physics`, `manage_profiler`, `manage_build`, `manage_graphics`, `manage_camera` with Cinemachine
- `batch_execute` tool - runs multiple operations in one call (10-100× faster for bulk work)
- `unity_docs` and `unity_reflect` tools - the LLM can look up live Unity API docs and inspect C# types via reflection at runtime
- Multi-instance support (target a specific Unity Editor if you have several open)
- Python-based server (so `uv`/`uvx` is your friend here)

**Verdict:** Richest tool coverage right now, especially for graphics/physics/build pipelines. Python server fits your stack well. Actively maintained with frequent releases.

---

### Which one should you use?

| | Official Unity | IvanMurzak | CoplayDev |
|---|---|---|---|
| Unity version | 6+ only | 2021.3+ | 2021.3+ |
| Server language | Go (relay binary) | C# (binary) | Python |
| Transport | stdio (relay) | stdio or HTTP | HTTP or stdio (uvx) |
| Tool count | ~10 built-in | 50+ | 40+ |
| Runtime (in-game) | ❌ | ✅ | ❌ |
| Python-friendly | - | - | ✅ |
| Maturity | Pre-release | Stable | Stable |
| Your VSCode Copilot | ✅ | ✅ | ✅ |

**Recommendation for your setup (Ubuntu, Python dev, VSCode Copilot):** Start with **CoplayDev** - Python server (`uv`/`uvx`), richest tool set, HTTP transport that pairs cleanly with VSCode. If you need in-game AI (NPCs, runtime debug), add **IvanMurzak** to the same project; they can coexist.

The **Official Unity MCP** is worth watching but Unity 6+ only and still pre-release - skip it for now unless you're already on Unity 6.

---

## 3. Setup: CoplayDev/Unity-MCP (Recommended)

### Prerequisites checklist

- Unity 2021.3 LTS or later
- Python 3.10+ (you have it)
- `uv` installed (`pip install uv` or `curl -LsSf https://astral.sh/uv/install.sh | sh`)
- VSCode with GitHub Copilot (you have it)
- Your Unity project path **must have no spaces**

### Step 1 - Install the Unity package

Open your Unity project, then:

**Window → Package Manager → + → Add package from git URL**

```
https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
```

Or via OpenUPM CLI:

```bash
openupm add com.coplaydev.unity-mcp
```

After import, open **Window → MCP for Unity**. You should see the panel with a **Start Server** button.

### Step 2 - Start the Unity HTTP server

In the MCP for Unity panel:

1. Click **Start Server** - it starts an HTTP server on `localhost:8080`
2. The status indicator should go 🟢 green

This server runs as long as the Unity Editor is open. You'll need to click it each session (or you can configure auto-start in the panel settings).

### Step 3 - Configure VSCode Copilot

VSCode Copilot uses a `.vscode/mcp.json` file (or your user-level settings) to know which MCP servers to connect to.

Create or edit `.vscode/mcp.json` in your workspace root:

```json
{
  "servers": {
    "unityMCP": {
      "type": "http",
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

Alternatively, in **VSCode Settings (JSON)**:

```json
{
  "github.copilot.chat.mcpServers": {
    "unityMCP": {
      "type": "http",
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

Or use the auto-configure button in the MCP for Unity panel: select **VS Code** from the client dropdown and click **Configure**.

### Step 4 - Enable Agent mode in Copilot

MCP tools only work in **Agent mode** (not the default Chat mode).

In the Copilot Chat panel, click the mode selector and switch to **Agent**. You should see a tools icon; clicking it should list Unity MCP tools like `manage_scene`, `manage_gameobject`, etc.

### Step 5 - Test it

With Unity open, the HTTP server running, and Copilot in Agent mode:

```
Create a red cube at position (0, 1, 0) in the current scene
```

Copilot will invoke `manage_gameobject` and `manage_material` tools and the cube appears in Unity.

Other quick tests:

```
List all GameObjects in the scene hierarchy
Read the Unity console and summarize any errors
Create a directional light pointing down at 45 degrees
```

---

## 4. Setup: IvanMurzak/Unity-MCP (Optional / In-Game)

Add this if you want the in-game runtime angle or simply prefer a binary-only server with no Python dependency.

### Step 1 - Install the plugin

**Option A - Installer (easiest):**

Download `AI-Game-Dev-Installer.unitypackage` from the [GitHub releases page](https://github.com/IvanMurzak/Unity-MCP/releases) and double-click to import it into Unity.

**Option B - OpenUPM:**

```bash
openupm add com.ivanmurzak.unity.mcp
```

After install, Unity auto-builds the MCP server binary into:

```
<YourProject>/Library/mcp-server/linux-x64/unity-mcp-server
```

### Step 2 - Get the port

Open **Window → AI Game Developer (Unity-MCP)**. Note the port shown (default 8080 - change it if CoplayDev is already using 8080, e.g. set to 8081).

### Step 3 - Configure VSCode Copilot

Add a second server entry to `.vscode/mcp.json`:

```json
{
  "servers": {
    "unityMCP": {
      "type": "http",
      "url": "http://localhost:8080/mcp"
    },
    "aiGameDeveloper": {
      "type": "stdio",
      "command": "/absolute/path/to/YourProject/Library/mcp-server/linux-x64/unity-mcp-server",
      "args": ["--port=8081", "--client-transport=stdio"]
    }
  }
}
```

Replace the path with your actual Unity project path. You can also click **Configure** in the AI Game Developer panel and select VS Code to auto-generate this.

### Step 4 - Approve the connection

First time an external client connects, Unity shows a **Pending Connections** dialog in the panel. Click **Accept**.

---

## 5. Setup: Official Unity MCP (Unity 6+ only)

If you're on Unity 6000.0+ and the `com.unity.ai.assistant` package is installed:

### Step 1 - Verify the bridge

**Edit → Project Settings → AI → Unity MCP**

The Bridge Status should show **Running** (green). If not, click **Start**.

The relay binary is auto-installed to `~/.unity/relay/relay_linux`.

### Step 2 - Configure VSCode Copilot

```json
{
  "servers": {
    "unity-mcp": {
      "type": "stdio",
      "command": "${env:HOME}/.unity/relay/relay_linux",
      "args": ["--mcp"]
    }
  }
}
```

### Step 3 - Approve the connection

In **Project Settings → AI → Unity MCP → Pending Connections**, click **Accept**.

---

## 6. Connecting Everything: Summary

```
Unity Editor (any project)
  ├── CoplayDev plugin  →  HTTP server :8080
  └── IvanMurzak plugin →  binary server :8081 (if used)

Ubuntu box (same machine, or same LAN)
  └── VSCode + Copilot
        └── .vscode/mcp.json
              ├── unityMCP  →  http://localhost:8080/mcp
              └── aiGameDeveloper  →  stdio → binary at Library/mcp-server/...
```

Copilot Agent mode discovers tools from all configured servers automatically. When you send a prompt, it picks the right tool from whichever server provides it.

---

## 7. Tips for Daily Use

**Always start Unity before VSCode** (or at least before opening Copilot Agent) so the HTTP server is up when the client tries to connect.

**Use `batch_execute`** (CoplayDev) for bulk operations - placing multiple objects, batch-modifying materials, etc. It's dramatically faster than sequential calls.

**Check the MCP panel** if tools stop appearing in Copilot. Usually it means the HTTP server stopped (Unity recompiled and restarted it, or you closed/reopened the project).

**Custom tools** in both IvanMurzak and CoplayDev let you expose your own C# methods to the LLM. For project-specific workflows (e.g., your game's level layout conventions), this is worth doing - a well-described custom tool beats a long chat prompt every time.

**Agent mode is required.** Regular Copilot Chat does not invoke MCP tools. Always check you're in Agent mode before complaining nothing works.

---

## 8. Troubleshooting

| Problem | Fix |
|---|---|
| Tools don't appear in Copilot | Confirm Agent mode is active; reload VSCode window |
| HTTP server not reachable | Click **Start Server** in Unity panel; check `curl http://localhost:8080/mcp` |
| Binary not found (IvanMurzak) | Re-open Unity - it rebuilds the binary on startup |
| Port conflict | Change one server to port 8081; update config accordingly |
| Connection pending forever | Go to Unity panel → accept the pending connection |
| Project path has spaces | Rename project folder - spaces break IvanMurzak's binary path |
| Copilot doesn't read `.vscode/mcp.json` | Use user-level settings or ensure the workspace has the file at root |
