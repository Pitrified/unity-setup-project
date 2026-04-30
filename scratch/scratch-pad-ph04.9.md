# Scratch Pad - artificial-pi - Phase 4 Manual Steps

All manual (HUMAN) steps for Phase 4, coalesced from scratch pads 4.1 through 4.8.
Complete these in order. Each step tells you exactly where to click and what to expect.

> **New to Unity?** The Unity Editor has four main areas:
> - **Project window** (bottom): your files on disk displayed as a tree/grid.
> - **Hierarchy window** (left): GameObjects in the currently open scene.
> - **Inspector window** (right): properties of the selected file or GameObject.
> - **Console window** (bottom, tab next to Project): log messages and errors.
>
> You open windows from the **Window** menu at the top of the screen.
> Press **Play** (top-centre triangle button) to run the game inside the Editor.

---

## Step 1 - Verify the editor compiles cleanly after Phase 4 code generation

**When to do this:** Before doing anything else in this list.

1. Open Unity Hub, then open the `artificial-pi` project.
2. Unity will detect new scripts and recompile automatically. Wait for the spinning
   progress indicator in the bottom-right corner to finish.
3. Open the Console window: **Window > General > Console** (or `Ctrl+Shift+C`).
4. Check that there are **zero errors** (red lines) and **zero warnings** (yellow lines).

**Expected result:** Console is empty or shows only informational messages (white lines).
If you see red errors, stop and fix them before continuing.

---

## Step 2 - Run all EditMode tests

**Why:** Confirms that all 9 Phase 4 systems have passing unit tests before you wire
anything by hand.

1. Open the Test Runner: **Window > General > Test Runner**.
2. Click the **EditMode** tab.
3. Click **Run All**.
4. Wait for all tests to finish (the icon next to each test turns green or red).

**Expected result:** All tests pass (green checkmarks). The counts to expect:

| Test class | Tests |
|---|---|
| `LogTests` | 9 |
| `SaveSystemTests` | 14 |
| `InputManagerTests` | 8 |
| `AudioManagerTests` | (varies, at least 2) |
| `UISystemTests` | 11 |
| `SceneLoaderTests` | 12 |
| `GameManagerTests` | 26 |

If any test is red, click it to see the failure message. Do not proceed until all
tests are green.

---

## Step 3 - Move and rename the PanelSettings asset

**Why:** The asset was created in the wrong folder with the wrong name during
Phase 4.1 scaffolding. It must be fixed *inside* Unity to preserve GUID references.
Do NOT rename or move files in your OS file manager - Unity tracks assets by GUID
and moving outside the editor will break references.

**Context:** `Assets/UI/NewPanelSettings.asset` holds display settings for the
debug overlay panel.

### 3a - Move the asset to the correct folder

1. In the **Project window**, expand `Assets > UI`.
2. Find `NewPanelSettings` (its icon looks like a white document with a Unity logo).
3. Click and drag it onto the `Assets > Settings` folder.
4. Unity will ask "Are you sure you want to move this asset?" - click **Move**.

**Check:** The Project window should now show `Assets/Settings/NewPanelSettings`.
It should no longer appear under `Assets/UI/`.

### 3b - Rename the asset

1. In the **Project window**, expand `Assets > Settings`.
2. Right-click `NewPanelSettings` and choose **Rename**.
3. Type `SO_DebugOverlayPanel` and press **Enter**.

**Check:** The asset is now listed as `SO_DebugOverlayPanel` in `Assets/Settings/`.

### 3c - Verify the scene reference is still intact

1. In the **Project window**, double-click `Assets/Scenes/SampleScene.unity` to open it.
2. In the **Hierarchy window**, click the `DebugOverlay` GameObject.
3. In the **Inspector window**, find the **UI Document** component.
4. Look at the **Panel Settings** field. It should reference `SO_DebugOverlayPanel`
   (not say "Missing" or be empty).

**Expected result:** `Panel Settings` field shows `SO_DebugOverlayPanel`. If it shows
"Missing", drag `Assets/Settings/SO_DebugOverlayPanel` from the Project window onto
the Panel Settings field in the Inspector.

---

## Step 4 - Verify the DebugOverlay works in SampleScene

**Context:** During Phase 4.1 a `DebugOverlay` GameObject was added to `SampleScene`
as a temporary test bed (it will move to `Persistent.unity` in Phase 5).

1. Make sure `SampleScene.unity` is open (double-click it if not).
2. In the **Hierarchy window**, click `DebugOverlay` and confirm the **Inspector** shows:
   - A **UI Document** component with **Source Asset** = `DebugOverlay` (from `Assets/UI/`)
     and **Panel Settings** = `SO_DebugOverlayPanel`.
   - A **Debug Overlay Controller** component.
3. Press the **Play** button (top-centre of the editor).
4. Press the **F1** key on your keyboard.

**Expected result:** A dark semi-transparent overlay appears in the Game view showing
FPS, frame time, and a few log lines. Press F1 again - it should disappear.
No red errors appear in the Console while the game is running.

5. Press **Play** again to stop the game.

---

## Step 5 - Create the AudioMixer asset

**Context:** `AudioManager.cs` (Phase 4.4) requires a mixer asset with three specific
groups and exposed parameters. The C# code will log an error at runtime if this asset
is missing.

### 5a - Create the mixer

1. In the **Project window**, right-click `Assets/Settings`.
2. Choose **Create > Audio Mixer**.
3. A new asset appears. Name it `MainMixer` and press **Enter**.
4. The **Audio Mixer** window opens automatically. If it does not, double-click
   `Assets/Settings/MainMixer` to open it.

### 5b - Add the Music group

1. In the Audio Mixer window, you will see a **Groups** column on the left with one
   entry called `Master`.
2. Click `Master` to select it.
3. At the bottom of the Groups column, click the `+` button.
4. A new child group appears under Master. Name it `Music` and press **Enter**.

### 5c - Add the SFX group

1. Click `Master` again to select it (not `Music`).
2. Click the `+` button again.
3. Name the new group `SFX` and press **Enter**.

**Check:** The Groups column should now show:
```
Master
  Music
  SFX
```

### 5d - Expose the volume parameters

You need to expose three parameters so that C# code can change them by name.

**For MasterVol:**
1. Click `Master` in the Groups column.
2. In the Inspector, find **Attenuation > Volume** (the slider showing `0.00 dB`).
3. Right-click on the **Volume** label (the word, not the slider).
4. Choose **Expose 'Volume (of Master)' to script**.
5. In the **Exposed Parameters** section (top-right of the Audio Mixer window, a small
   dropdown labelled "Exposed Parameters"), a new parameter appears named something like
   `MyExposedParam`. Click its name and rename it to exactly `MasterVol`.

**For MusicVol:**
1. Click `Music` in the Groups column.
2. In the Inspector, right-click the **Volume** label and choose **Expose 'Volume (of Music)' to script**.
3. In Exposed Parameters, rename the new entry to exactly `MusicVol`.

**For SfxVol:**
1. Click `SFX` in the Groups column.
2. Right-click the **Volume** label and expose it.
3. Rename the exposed parameter to exactly `SfxVol`.

**Check:** The Exposed Parameters dropdown should list exactly three entries:
`MasterVol`, `MusicVol`, `SfxVol`.

> The parameter names are case-sensitive and must match exactly. The C# code
> uses the strings `"MasterVol"`, `"MusicVol"`, `"SfxVol"`.

---

## Step 6 - Commit all Phase 4 assets

At this point all code and assets for Phase 4 are in place. Commit everything before
moving on to Phase 5 scene wiring.

```bash
cd /path/to/your/repo
git add -A
git status          # review what is staged
git commit -m "phase 4: all systems implemented + manual assets wired"
```

**Check:** `git status` after the commit should show nothing outstanding.

---

## Phase 5 preview - steps that will be done next

The following are NOT done in Phase 4. They require new scene files which are created
at the start of Phase 5. They are listed here so you know what is coming.

| Future step | What it involves |
|---|---|
| Create the four scenes | `Boot.unity`, `Persistent.unity`, `Menu.unity`, `Game.unity` in `Assets/Scenes/` |
| Wire Boot.unity | Create one GameObject named `Bootstrap`, attach `GameBootstrap` script |
| Wire Persistent.unity | Add `GameManager`, `SceneLoader`, `InputManager`, `AudioManager`, `UISystem`, `DebugOverlay` - each as a MonoBehaviour on a dedicated GameObject |
| Assign InputActions to InputManager | Drag `Assets/Settings/InputActions.inputactions` onto the `_inputActions` field of the `InputManager` component |
| Assign MainMixer to AudioManager | Drag `Assets/Settings/MainMixer` and each of its three groups onto the matching serialized fields of `AudioManager` |
| Wire UISystem documents | Attach `Loading.uxml` and `Pause.uxml` to the two UIDocument children of the `UIRoot` GameObject |
| Register all scenes in Build Settings | File > Build Settings > drag all four scenes, Boot first |
| Move DebugOverlay from SampleScene to Persistent.unity | Cut/paste the DebugOverlay GameObject; delete from SampleScene |
| Final smoke test | Press Play in `Boot.unity`; confirm it reaches the Menu scene with 0 console errors |
