# Scratch Pad - artificial-pi

Useful commands accumulated during scaffolding. Will be coalesced into the
appropriate docs later.

---

## Phase 2 - Repository scaffolding

### One-time dev-machine setup (run after cloning)
```bash
# From repo root:
Tools/setup.sh
```
Installs git-lfs hooks, links the pre-commit hook, copies build.env.example.

### After adding/changing .gitattributes
```bash
git add --renormalize .
git status   # verify no unexpected re-normalizations
```

### Configure Unity YAML smart merge (once per machine)
```bash
# Replace <version> with your actual Unity Editor version, e.g. 6000.4.4f1
UNITY_VERSION="6000.4.4f1"
UNITY_MERGE="/home/<user>/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Data/Tools/UnityYAMLMerge"
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver \
  "${UNITY_MERGE} merge -p %O %B %A %A"
```

### Verify LFS is tracking binary assets
```bash
git lfs ls-files          # shows LFS-tracked files after first commit
git lfs status            # staged LFS changes
git check-attr filter -- Assets/some/file.png   # should print: lfs
```

### Manually test the pre-commit hook
```bash
# Simulate staging a large non-LFS file (> 1 MB):
dd if=/dev/urandom of=/tmp/big.bin bs=1M count=2
cp /tmp/big.bin ./big.bin
git add big.bin
git commit -m "test"      # should be BLOCKED by pre-commit hook
git reset HEAD big.bin && rm big.bin

# Simulate staging a fake keystore:
touch test.keystore
git add test.keystore
git commit -m "test"      # should be BLOCKED
git reset HEAD test.keystore && rm test.keystore
```

### Stage and commit Phase 2 files
```bash
cd /path/to/repo
git add .gitignore .gitattributes .editorconfig AGENTS.md \
        .github/copilot-instructions.md \
        Tools/git-hooks/pre-commit Tools/setup.sh
git commit -m "Add Phase 2 repo scaffolding

Per docs/tracking.md Phase 2:
- .gitignore (Unity + IDE + secrets)
- .gitattributes (LFS + UnityYAMLMerge)
- .editorconfig (Allman, 4-space, LF, naming rules)
- .github/copilot-instructions.md
- AGENTS.md
- Tools/git-hooks/pre-commit (Library/Build/secrets/large-binary guards)
- Tools/setup.sh (lfs install, hook symlink, build.env copy)"
```

### Verify .editorconfig is picked up by VS Code
```bash
# Install editorconfig extension if not already present:
# Extension ID: EditorConfig.EditorConfig
code --install-extension EditorConfig.EditorConfig
```

### Useful git lfs commands for later phases
```bash
git lfs track "*.png"          # add a new type to LFS (also updates .gitattributes)
git lfs untrack "*.png"        # remove a type
git lfs migrate import --include="*.png,*.fbx"  # retroactively move blobs to LFS
git lfs prune                  # clean up old LFS objects locally
```

---

## Phase 3 reminders (future)

```bash
# After Unity Hub creates the project, verify LFS sees the binary assets:
git lfs ls-files | head -20

# Check for any Library/ or Build/ accidentally added:
git status | grep -E 'Library/|Build/'

# Confirm Packages/ manifest is committed (required):
git ls-files Packages/manifest.json Packages/packages-lock.json
```
