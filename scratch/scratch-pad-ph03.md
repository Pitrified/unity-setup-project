# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 3

### Unity Hub project creation notes

- Organization: (picked non-nominal one)
- Template: standard 3D (URP) Mobile
- Do NOT select the Unity VCS tracker option
- Accept license when prompted
- Project created at repo root as `UnitySetupPrpj/`

---

### Phase 3 - Point 2: Initial commit

What gets committed (gitignore excludes the rest):
- `Assets/` - scenes, settings, URP assets, tutorial info
- `Packages/manifest.json` + `packages-lock.json` - MUST be committed
- `ProjectSettings/` - all project settings assets

What is excluded by `.gitignore`:
- `Library/` - Unity's regenerable cache (never commit)
- `Temp/` - build temporaries
- `Logs/`
- `UserSettings/` - per-developer IDE prefs

Commit done: `86ef97f`

```bash
# Stage everything under the Unity project folder
git add UnitySetupPrpj/

# Commit
git commit -m "Add initial Unity 6 LTS URP project (UnitySetupPrpj)"
```

Pre-commit hook ran and passed (checks for Library/, secrets, large non-LFS binaries).

---

### Phase 3 - Point 3: Verify LFS

`URP.png` (only binary file in the initial project) was correctly intercepted
by LFS. Confirmed via `git lfs status` before commit (showed `LFS: 1d17a9f`)
and `git lfs ls-files` after commit.

```bash
# Check before committing: should show (LFS: <hash>) for binary files
git lfs status

# Check after committing: lists all LFS-tracked files in the repo
git lfs ls-files
# Expected output:
# 1d17a9ff35 * UnitySetupPrpj/Assets/TutorialInfo/Icons/URP.png
```

LFS status key:
- `(LFS: <hash>)` in `git lfs status` = file is staged as an LFS pointer (correct)
- `(Git: <hash>)` in `git lfs status` = file is staged as a plain git object

If a binary that should be LFS shows `(Git: ...)`, the `.gitattributes` rule
is missing or the file was added before `git lfs install` was run. Fix:
```bash
git rm --cached <file>
git add <file>          # re-add after LFS rules are in place
git lfs ls-files        # confirm it now appears
```

