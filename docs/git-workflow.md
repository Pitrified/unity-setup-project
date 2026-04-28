# Git Workflow

Unity projects break in painful ways without LFS and the right ignore rules.
Follow this exactly.

---

## 1. Branch model

- `main` - always shippable. Protected.
- `feat/<short-name>` - feature branches. Squash-merge into `main`.
- No long-lived branches besides `main`.

---

## 2. Commits

- Imperative mood: "Add ship throttle clamp".
- Subject ≤ 72 chars. Body wrapped at 80 if present.
- One logical change per commit. AI-generated diffs are split if they cross systems.
- Reference docs by path: `Per docs/systems/ship-controller.md, ...`.

---

## 3. `.gitignore` essentials (Unity)

```
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
[Mm]emoryCaptures/
[Rr]ecordings/

# IDE
.vs/
.idea/
*.csproj
*.sln
*.user

# Local
keystore/
*.keystore
*.env
*.log
```

`Packages/manifest.json` and `Packages/packages-lock.json` **must** be committed.

---

## 4. `.gitattributes` (LFS - required)

Unity scenes/prefabs are YAML but LFS for binary assets is mandatory:

```
# Force LF
* text=auto eol=lf

# Unity YAML - keep as text but always LF
*.unity     text eol=lf merge=unityyamlmerge
*.prefab    text eol=lf merge=unityyamlmerge
*.asset     text eol=lf merge=unityyamlmerge
*.meta      text eol=lf
*.mat       text eol=lf
*.controller text eol=lf
*.anim      text eol=lf

# Binary assets - LFS
*.png       filter=lfs diff=lfs merge=lfs -text
*.jpg       filter=lfs diff=lfs merge=lfs -text
*.psd       filter=lfs diff=lfs merge=lfs -text
*.tga       filter=lfs diff=lfs merge=lfs -text
*.tif       filter=lfs diff=lfs merge=lfs -text
*.exr       filter=lfs diff=lfs merge=lfs -text
*.fbx       filter=lfs diff=lfs merge=lfs -text
*.obj       filter=lfs diff=lfs merge=lfs -text
*.wav       filter=lfs diff=lfs merge=lfs -text
*.ogg       filter=lfs diff=lfs merge=lfs -text
*.mp3       filter=lfs diff=lfs merge=lfs -text
*.aab       filter=lfs diff=lfs merge=lfs -text
*.apk       filter=lfs diff=lfs merge=lfs -text
*.unitypackage filter=lfs diff=lfs merge=lfs -text
```

After adding/changing this file: `git add --renormalize .`.

---

## 5. Unity YAML merge

Set up Unity's smart merger once per machine:

```
git config --global merge.unityyamlmerge.name "Unity SmartMerge"
git config --global merge.unityyamlmerge.driver \
  "<UnityHub>/Editor/<version>/Editor/Data/Tools/UnityYAMLMerge merge -p %O %B %A %A"
```

The `merge=unityyamlmerge` line in `.gitattributes` triggers it for `.unity`/`.prefab`/`.asset`.

Even with this, **scenes don't merge well**. Coordinate scene edits; never have
two open PRs touching the same scene.

---

## 6. Pre-commit guardrails

A minimal pre-commit hook (or VS Code task) should fail when:

- `Library/` or `Build/` would be committed
- `*.keystore`, `*.env`, files with `PRIVATE KEY` are staged
- Non-LFS-tracked binaries > 1 MB are staged

Add a `Tools/git-hooks/pre-commit` script and instruct devs to symlink it via
`Tools/setup.sh`.

---

## 7. Pull requests

- Small (< 400 lines changed where possible).
- Title = commit subject.
- Description must answer: **What changed? Why? How verified?**
- Self-review the diff before requesting review.
- AI-generated diffs are clearly marked as such in the description.

---

## 8. Tags & releases

- Tag every Play Console upload: `v0.2.0-alpha3`.
- Tag is annotated (`git tag -a`), message names the AAB file and SHA256.
- Never delete or move a release tag.

---

## 9. Forbidden

- `git push --force` on `main` (push `--force-with-lease` only on your own feature branch, before review).
- Rewriting history on a pushed branch others may have pulled.
- Committing into `Library/`, `Build/`, or `keystore/`.
- Adding LFS-eligible assets without updating `.gitattributes` first.