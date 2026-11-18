# Building an asset from a reference

How a game object is made here: a Blender script builds it, Unity imports the FBX, and passes compare it against a photograph until it reads the same.
Everything runs headless; a session can do it unattended. The skill `.claude/skills/build-asset/` is the short form of this page.
The target is **stylized**: the same proportion, silhouette and palette, in the game's shading, not a photoreal copy.
The method is adapted from house-renderer's `match-reference`; the worked example is the barrel (`Tools/asset_builders/builders/barrel.py`).

---

## 1. Build it or take it

- **Build it** when the numbers exist: barrels, crates, planks, masts, buildings, anything made of parts with dimensions.
- **Take it** when the shape is the thing: people, animals, foliage. A taken asset must allow shipping in a commercial app with ads:
  - yes: CC0, public domain, CC BY 4.0 (credit author, title, link, licence, changes), MIT / BSD / Apache 2.0 / zlib (keep the notice), the Unity Asset Store EULA (in a build only, never its source files in a public repo);
  - no: CC BY-SA (share-alike's reach into a game is unclear), CC BY-NC (ads are commercial), CC BY-ND (we adapt), "personal" or "editorial" only, no licence.
- A taken model still goes through a builder script that imports, scales and exports it, so the FBX in the tree is reproducible.
- Every taken asset, and every reference photograph kept in the repo, gets a row in `Tools/asset_builders/references/CREDITS.md`. A build with a CC BY asset needs an in-app credits screen.

## 2. Before the first render

1. **Find references.** Wikimedia Commons is searchable by category with each file's licence in one API call:
   `https://commons.wikimedia.org/w/api.php?action=query&generator=categorymembers&gcmtitle=Category:<Name>&gcmtype=file&gcmlimit=100&prop=imageinfo&iiprop=url|extmetadata|size&iiurlwidth=900&format=json`
   (`extmetadata.LicenseShortName`). Download the 960 px thumbnails one at a time with a pause; Commons answers 429 to a quick burst. Use a generic User-Agent, nothing personal.
2. **Choose one** on a contact sheet: side-on, the object whole, a licence from the list above, ideally the same kind of object as the real numbers. Keep it in `Tools/asset_builders/references/<name>/` (outside `Assets/`, so Unity does not import it), and add its credit.
3. **Find the real numbers** for that kind of object (a product sheet, a standard): dimensions first is cheaper than measuring pixels, and a photograph in perspective gives ratios, not sizes.
4. **Write what right looks like**: a numbered list, from the reference, of what has to read correctly (silhouette ratios, part count and placement, widths, features, palette). Six lines for the barrel. It is the checklist and the stop condition of a loop nobody watches.

## 3. The builder

`Tools/asset_builders/builders/<name>.py`, a `uv` project with Blender's `bpy` (no Blender install):

- `PARAMS`: the defaults, each with its unit and where the number came from.
- `build(params)`: makes the model's objects in the current scene, in **metres, Z up, origin at the base centre, -Y facing the viewer**.
- **Material slots are names** (`Wood`, `Iron`); their colours are a probe preview only. Unity's materials are made in Unity: Blender materials do not survive FBX reliably.
- Parametric where the object is (heights, counts, positions), fixed where it is not.
- Hard edges are `sharp_edge` attributes on a smooth-shaded mesh: an edge left smooth rounds a flat band into a tube.

```
cd Tools/asset_builders
uv sync                                                    # once
uv run --no-sync python build.py barrel                    # FBX into UnitySetup/Assets/Art/Models/
uv run --no-sync python build.py barrel --no-fbx --probe out.png --view 0 5   # shape pass
uv run --no-sync python build.py barrel --set hoop_width=0.05 --no-fbx --probe out.png
```

`build.py` prints the triangles, slots and size for the budget. The probe renders the model alone with Cycles on the GPU: one sun, a uniform sky, a grey ground, framed from its bounds; `--reference` writes the photograph beside it.
The FBX is written for Unity (Y up, units applied, axis change baked) and **byte-identical on a rebuild**, so git shows only real changes.

## 4. The Unity side

Run in batch mode, as `docs/headless-unity.md` describes (`$UNITY ... -executeMethod <method> <arguments>`):

1. **`Game.Editor.ModelSetup.SetUpFromCommandLine -modelPath Assets/Art/Models/<Name>.fbx`**: import settings for a static prop, and each slot mapped to `Assets/Art/M_<slot>.mat` (URP Lit, created if missing). The mapping lives in the `.meta`; run it again when a slot is added. Slots are shared by name across models, so two woods need two slot names.
2. **`Game.Editor.MaterialTuning.SetFromCommandLine -material Assets/Art/M_<slot>.mat -baseColor r,g,b [-metallic m] [-smoothness s]`**: the palette, sRGB 0 to 1.
3. **A prefab** `PF_<Name>` with a simple collider, and its placement in a scene, through a temporary editor method named for what it does (`docs/headless-unity.md` §4).
4. **`Game.Editor.SceneCapture.CaptureFromCommandLine -captureOut shot.png [-captureEye x,y,z -captureTarget x,y,z -captureFov f]`**: the object in the game's own scene, camera and light. This is where the look is judged.
5. **A case in `BuiltModelTests`** (EditMode), for the imported model: size from the builder's numbers, pivot at the base ring's centre, no rotation or scale on any transform, the slots mapped.

## 5. The passes

One class of change per pass, numbered `r<N>` in a scratch folder, never over a kept file. Look at the reference again every pass.

1. **Shape, in the probe**: silhouette, proportion, part placement. Measure rather than guess: `measure.py profile IMAGE X0 Y0 X1 Y1` prints brightness and chroma along a box, and bands, hoops or planks show as steps (the barrel's galvanised hoops are low chroma against the oak). Average both ends of a symmetric object to cancel perspective.
2. **Detail, in the probe**: widths, edges, small features, with `measure.py crop` putting the probe and the reference side by side.
3. **Palette, in the Unity capture**, measured:
   - pick a **neutral patch** present in both pictures and lit like the rest: something white or grey (the barrel used its steel hoop, since a cellar has no white). Ratios to it remove the photograph's white balance and exposure and the render's;
   - when the neutral is **the same object in both pictures** (a part of the model, like the hoop), the ratios carry value and hue. When it is not (a grey rock in the photograph, a grey card in the render: `SceneCapture -captureGreyCard x,y,z`), they carry **hue only**: the two greys' albedos differ. Compare chromaticity (each ratio over their mean), and set the value by what the material plausibly is. Check the "neutral" is neutral: the crate's rock measured `#bec5ad`, green with moss, so it gave brightness only (`"neutral_mode": "luminance"` in the spec) and the photograph's own white balance was trusted for hue;
   - **match the patch that stands for the face**, a median over most of it, not a corner: the crate's lower-left corner was browner than its face. Say which patch and why;
   - `measure.py measure SPEC capture.png` prints each patch's ratio to the neutral and how far it is from the reference's (the spec format is in its docstring; the barrel's is `references/barrel/spec.json`);
   - ratios fix the colours' relation only: anchor one colour by hue and a plausible value for the material, derive the rest from the ratios;
   - `measure.py correct R G B --target ... --current ...` moves a colour by the measured factor in linear light; stop when the chosen patch is within a few hundredths. Two captures did it for the barrel.
4. **Motion, in a sequence.** Anything that moves (a float bobbing, water, a flag) is judged over time: the sequence capture of `docs/headless-unity.md` §3 and `measure.py sheet / gif / motion / track`. A still of moving water says nothing about whether it moves right.
5. **Texture is a separate question.** Compare a patch's spread as well as its median; flat colours cannot carry a grain (the barrel's spread is 8 against the photograph's 30, the crate's about 1 against 87). A weathered board reads grey to the eye because of light and dark streaks around a warm median; a flat colour at that median reads browner. Say so rather than chase it; a small tiling texture is the upgrade.

## 6. Budget

A stop condition like the list: triangles and draw calls against `docs/performance-budget.md` (150 k triangles and 80 draw calls on screen). Each material slot is a draw call per instance; a few hundred to a couple of thousand triangles is right for a prop seen at a few metres. The barrel: 1978 triangles, three slots. The crate: 72 triangles, one slot.

## 7. Pitfalls seen here

- **FBX rebuilds differ** unless the exporter's clock and Python's hash seed are fixed; `build.py` does both.
- **Bounds are not the pivot**: a feature on one side (the barrel's bung) moves the bounds' centre; test the base ring.
- **Smooth shading rounds what should be flat**: mark the ring edges of a band sharp.
- **The first editor render after opening a scene is unlit**; `SceneCapture` renders twice.
- **Commons rate limits** quick downloads; one at a time, thumbnails.

## 8. How a run ends

1. **The list reads correctly** in the Unity capture,
2. **or a line clearly cannot**, and the run says which and why (the barrel's grain),
3. and either way **the findings are written**: what was measured, what each pass changed, what is still off, in the plan, and anything general here.
