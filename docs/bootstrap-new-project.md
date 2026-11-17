# Starting a new project from this guide

How a session (or a person) starts a new Unity mobile project from this repo.
There is no bootstrap script: a session reads this, writes a plan in the new repo, and copies and adapts the files.
The worked example is [artificial-pirate](https://github.com/Pitrified/artificial-pirate)'s [`plans/01_bootstrap/`](https://github.com/Pitrified/artificial-pirate/blob/main/plans/01_bootstrap/00_start.md), which did this for the naval game in an afternoon.

---

## 1. Before anything is copied

- The new repo exists, empty (created on GitHub by the user; no README, no `.gitignore`).
- Decided with the user, and written in the new repo's `plans/01_bootstrap/00_start.md`:
  - the repo name, the Unity project folder name and the product name;
  - the Android `applicationIdentifier` (`com.<owner>.<app>`), **permanent once a build is on Play**;
  - whether the sample avatar stays as the starting point or the project brings its own gameplay.
- The plan names the source commit of this repo, and has one phase per step below.

## 2. Copy the baseline

```
git -C <this repo> archive <commit> | tar -x -C <new repo> --exclude=plans --exclude=scratch
```

- `git archive` copies tracked files only, without history; `.meta` GUIDs are file content, so every reference between assets survives.
- Then delete what is this guide's and not the project's: `docs/tracking.md`, `docs/bootstrap-new-project.md`, and the docs the new project will link here instead of copying (`ai-development-playbook.md`, `mcp-setup.md`, `dev-environment-setup.md`).
- Gate: the project compiles in batch mode (`Tools/gates.sh compile`), from an empty `Library/`.

## 3. Renames

In this order, each checked by `git grep` for the old name:

1. The project folder: `git mv UnitySetup <Name>`, then **delete `<Name>/Library/`** (a moved `Library/` breaks scripts in PlayMode; see [headless-unity.md §5](headless-unity.md#5-pitfalls-seen-here)).
   `Tools/build-android.sh`, `Tools/gates.sh`, `Tools/setup.sh` and the docs name the folder; update them.
2. Product name, `applicationIdentifier` and the Windows Store names through `PlayerSettings` in a temporary editor method run with `-executeMethod` ([headless-unity.md §4](headless-unity.md#4-changing-assets-without-the-editor)), deleted before the commit.
   `productName`, `PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ...)`, `PlayerSettings.WSA.packageName` and `.applicationDescription`.
3. `unity-setup` (the build env path `~/.config/unity-setup/build.env`, the APK name, `BuildPipeline.ProductName`, the `CreateAssetMenu` paths) to the project's name.
4. If the sample becomes the project's gameplay: rename its scripts and assets with `git mv` of each file and its `.meta`; re-link renamed serialized fields and refresh `m_EditorClassIdentifier` ([headless-unity.md §4](headless-unity.md#4-changing-assets-without-the-editor)).

Left as they are: the Unity Cloud link (`cloudProjectId`, `projectName` in `ProjectSettings.asset`), changed later at the Editor if the project uses Unity services.

Gates: `Tools/gates.sh all`, and `Tools/build-android.sh dev` with `aapt2 dump badging` showing the new id and label.

## 4. Docs and instructions

- A root `README.md` about the project: what it is, how to run the gates, where it came from (this repo and the source commit).
- `.github/copilot-instructions.md`, `CLAUDE.md` (imports the instructions) and `AGENTS.md` copied and changed where they name the project. They are the pattern; there is no separate template.
- `docs/functional-specs.md` rewritten for the project, keeping its shape: the locked decisions table and "anything not listed is not decided".
- Links to the docs left here become absolute links to them on this repo's `main` on GitHub.
- Gate: a link check over the new repo's Markdown (relative links resolve; absolute links into this repo name files that exist here).

## 5. Plans

- `plans/01_bootstrap/` is the first folder; open work moves in from wherever it was, numbered from 02, each folder saying where it came from.
- `python3 Tools/plans.py check` passes.

## 6. Verify

- A fresh clone of the new repo **on disk** (not under a RAM-backed `/tmp`): `Tools/setup.sh`, `Tools/gates.sh all` from an empty `Library/`, `Tools/build-android.sh dev`, the link check, `plans.py check`.
- HUMAN: push from the machine with the GitHub credentials; install the `dev` APK on a phone (it installs next to the guide's sample, a different id) and play it; logcat clean, when the phone is on USB.
- The bootstrap folder's log records the device run; this repo's README pattern index gets a line for anything in the new project worth copying.
