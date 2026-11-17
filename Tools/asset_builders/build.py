"""Build a model with Blender, export it as FBX for Unity, and optionally render a probe.

Run from this folder:

    uv run python build.py barrel                       # writes the FBX
    uv run python build.py barrel --probe out.png       # also renders the model alone
    uv run python build.py barrel --probe out.png --reference photo.jpg --view 0 8
    uv run python build.py barrel --set height=0.9 --set hoops=4 --probe out.png --no-fbx

A builder is `builders/<name>.py` with a `PARAMS` dict of defaults and a
`build(params)` function that makes the model's objects in the current Blender
scene, in metres, Z up, the origin at the base centre, -Y facing the viewer.
Materials are named slots only: Unity makes the real materials (docs/building_assets.md).

The FBX goes to UnitySetup/Assets/Art/Models/<Name>.fbx unless --out says otherwise.
"""

from __future__ import annotations

import os
import sys

# The FBX exporter derives object ids from Python's hash() of names, which is randomised
# per process; a fixed seed makes a rebuild byte-identical, so git sees only real changes.
if os.environ.get("PYTHONHASHSEED") != "0":
    os.environ["PYTHONHASHSEED"] = "0"
    os.execv(sys.executable, [sys.executable, *sys.argv])

import argparse
import datetime
import importlib
import json
import math
from pathlib import Path

import bpy

from probe import render_probe

REPO_ROOT = Path(__file__).resolve().parents[2]
MODELS_DIR = REPO_ROOT / "UnitySetup" / "Assets" / "Art" / "Models"


class BuilderError(Exception):
    """A builder or its parameters are not usable."""


def parse_overrides(pairs: list[str], defaults: dict) -> dict:
    """Apply `key=value` overrides to a builder's defaults, keeping each default's type."""
    params = dict(defaults)
    for pair in pairs:
        key, _, raw = pair.partition("=")
        if key not in defaults:
            msg = f"unknown parameter '{key}'; known: {', '.join(sorted(defaults))}"
            raise BuilderError(msg)
        kind = type(defaults[key])
        params[key] = json.loads(raw) if kind in (list, dict, bool) else kind(raw)
    return params


def reset_scene() -> None:
    """Start from an empty scene with metric units."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0


class _FixedTime(datetime.datetime):
    """The exporter stamps the file with now(); a fixed stamp keeps rebuilds identical."""

    @classmethod
    def now(cls, tz=None):  # noqa: ARG003 - same signature as datetime.now
        return cls(2000, 1, 1)


def export_fbx(path: Path) -> None:
    """Export every mesh in the scene as FBX for Unity: Y up, -Z forward, transforms baked."""
    from io_scene_fbx import export_fbx_bin  # noqa: PLC0415 - importable only once bpy is loaded

    export_fbx_bin.datetime = type("datetime_module", (), {"datetime": _FixedTime})
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=False,
        object_types={"MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        mesh_smooth_type="OFF",
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="STRIP",
        embed_textures=False,
    )


def stats() -> dict:
    """Triangles, material slots and bounds of the scene's meshes, as the budget sees them."""
    tris = 0
    slots: set[str] = set()
    lo = [math.inf] * 3
    hi = [-math.inf] * 3
    depsgraph = bpy.context.evaluated_depsgraph_get()
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        mesh = obj.evaluated_get(depsgraph).to_mesh()
        mesh.calc_loop_triangles()
        tris += len(mesh.loop_triangles)
        slots.update(m.name for m in mesh.materials if m)
        for v in mesh.vertices:
            w = obj.matrix_world @ v.co
            lo = [min(a, b) for a, b in zip(lo, w)]
            hi = [max(a, b) for a, b in zip(hi, w)]
        obj.evaluated_get(depsgraph).to_mesh_clear()
    return {
        "triangles": tris,
        "materials": sorted(slots),
        "size_xyz_m": [round(h - l, 4) for l, h in zip(lo, hi)],
        "min_z_m": round(lo[2], 4),
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("name", help="builder module in builders/")
    parser.add_argument("--set", action="append", default=[], metavar="KEY=VALUE", help="override a parameter")
    parser.add_argument("--out", type=Path, help="FBX path (default: UnitySetup/Assets/Art/Models/<Name>.fbx)")
    parser.add_argument("--no-fbx", action="store_true", help="do not write the FBX (probe passes)")
    parser.add_argument("--probe", type=Path, help="render the model alone to this PNG")
    parser.add_argument("--reference", type=Path, help="put this photograph beside the probe")
    parser.add_argument("--view", type=float, nargs=2, default=(0.0, 8.0), metavar=("AZIMUTH", "ELEVATION"),
                        help="probe camera in degrees: azimuth from the front (-Y), elevation above the horizon")
    parser.add_argument("--lens", type=float, default=50.0, help="probe focal length in mm (36 mm sensor)")
    parser.add_argument("--samples", type=int, default=32, help="probe Cycles samples")
    parser.add_argument("--height", type=int, default=720, help="probe image height in pixels")
    args = parser.parse_args()

    module = importlib.import_module(f"builders.{args.name}")
    params = parse_overrides(args.set, module.PARAMS)

    reset_scene()
    module.build(params)
    info = stats()
    print(json.dumps({"builder": args.name, "params": params, **info}, indent=2))

    if not args.no_fbx:
        out = args.out or MODELS_DIR / f"{args.name.title().replace('_', '')}.fbx"
        export_fbx(out)
        print(f"fbx: {out}")
    if args.probe:
        render_probe(args.probe, view=args.view, lens=args.lens, samples=args.samples,
                     height=args.height, reference=args.reference)
        print(f"probe: {args.probe}")


if __name__ == "__main__":
    main()
