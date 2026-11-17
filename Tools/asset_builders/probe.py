"""Render the model in the current scene alone, on a plain ground, for shape passes.

One sun, a uniform sky, a neutral ground, Cycles on the GPU when there is one.
The camera looks at the model's bounding box from an azimuth and an elevation and
frames it with a margin, so a probe reads proportion and silhouette, not lighting.
With a reference photograph, the two are written side by side at the same height.
"""

from __future__ import annotations

import math
from pathlib import Path

import bpy
from mathutils import Vector
from PIL import Image

SKY = (0.78, 0.80, 0.82)
GROUND = (0.35, 0.35, 0.35)
MARGIN = 1.15


def _use_gpu(scene: bpy.types.Scene) -> str:
    """Render on the GPU if Cycles finds one (OptiX, then CUDA); say which."""
    prefs = bpy.context.preferences.addons["cycles"].preferences
    for kind in ("OPTIX", "CUDA"):
        try:
            prefs.compute_device_type = kind
        except TypeError:
            continue
        prefs.get_devices()
        gpus = [d for d in prefs.devices if d.type == kind]
        if gpus:
            for d in prefs.devices:
                d.use = d.type == kind
            scene.cycles.device = "GPU"
            return kind
    scene.cycles.device = "CPU"
    return "CPU"


def _bounds() -> tuple[Vector, Vector]:
    lo = Vector((math.inf,) * 3)
    hi = Vector((-math.inf,) * 3)
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        for corner in obj.bound_box:
            w = obj.matrix_world @ Vector(corner)
            lo = Vector(min(a, b) for a, b in zip(lo, w))
            hi = Vector(max(a, b) for a, b in zip(hi, w))
    return lo, hi


def render_probe(out: Path, view: tuple[float, float], lens: float, samples: int, height: int,
                 reference: Path | None = None) -> None:
    """Render the scene's meshes to `out`; with `reference`, write probe and photograph side by side."""
    scene = bpy.context.scene
    lo, hi = _bounds()
    centre = (lo + hi) / 2
    radius = (hi - lo).length / 2

    ground_mesh = bpy.data.meshes.new("probe_ground")
    ground_mesh.from_pydata([(-50, -50, 0), (50, -50, 0), (50, 50, 0), (-50, 50, 0)], [], [(0, 1, 2, 3)])
    ground = bpy.data.objects.new("probe_ground", ground_mesh)
    mat = bpy.data.materials.new("probe_ground")
    mat.use_nodes = True
    mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*GROUND, 1)
    mat.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 1.0
    ground_mesh.materials.append(mat)
    scene.collection.objects.link(ground)

    world = bpy.data.worlds.new("probe_sky")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (*SKY, 1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.6
    scene.world = world

    sun_data = bpy.data.lights.new("probe_sun", "SUN")
    sun_data.energy = 3.0
    sun_data.angle = math.radians(3)
    sun = bpy.data.objects.new("probe_sun", sun_data)
    sun.rotation_euler = (math.radians(45), 0, math.radians(-35))
    scene.collection.objects.link(sun)

    az, el = (math.radians(a) for a in view)
    direction = Vector((math.sin(az) * math.cos(el), -math.cos(az) * math.cos(el), math.sin(el)))
    cam_data = bpy.data.cameras.new("probe_cam")
    cam_data.lens = lens
    cam_data.sensor_width = 36
    cam_data.sensor_fit = "VERTICAL"
    cam_data.sensor_height = 24
    half_fov = math.atan(cam_data.sensor_height / 2 / lens)
    distance = radius * MARGIN / math.sin(half_fov)
    cam = bpy.data.objects.new("probe_cam", cam_data)
    cam.location = centre + direction * distance
    cam.rotation_euler = (-direction).to_track_quat("-Z", "Y").to_euler()
    scene.collection.objects.link(cam)
    scene.camera = cam

    aspect = 0.75
    if reference is not None:
        with Image.open(reference) as ref:
            aspect = ref.width / ref.height
    scene.render.engine = "CYCLES"
    device = _use_gpu(scene)
    scene.cycles.samples = samples
    scene.cycles.use_denoising = True
    scene.render.resolution_y = height
    scene.render.resolution_x = round(height * aspect)
    scene.render.film_transparent = False
    scene.view_settings.view_transform = "Standard"
    scene.render.filepath = str(out.resolve())
    out.parent.mkdir(parents=True, exist_ok=True)
    print(f"probe: Cycles on {device}, {scene.render.resolution_x}x{height}, {samples} samples")
    bpy.ops.render.render(write_still=True)

    if reference is not None:
        with Image.open(out) as probe, Image.open(reference) as ref:
            ref = ref.convert("RGB").resize((round(ref.width * height / ref.height), height))
            pair = Image.new("RGB", (probe.width + ref.width, height))
            pair.paste(probe.convert("RGB"), (0, 0))
            pair.paste(ref, (probe.width, 0))
            pair.save(out)
