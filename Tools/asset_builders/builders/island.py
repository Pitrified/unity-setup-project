"""A round island with a beach sloping under the sea, as a radial height field.

The top is flat at height 0 out to `top_radius`, then a beach falls on a smooth curve
to `seabed` at `beach_radius`, and the seabed stays flat out to `outer_radius`.
A little low-frequency wobble on the coastline keeps it from reading as a perfect disc.
Two material slots: Sand for the beach and seabed, Grass for the top, split at a height.
The sea level is the scene's choice; the default water sits a little below the top.
"""

from __future__ import annotations

import math

import bpy

PARAMS = {
    "top_radius": 26.0,      # m, flat land
    "beach_radius": 40.0,    # m, where the beach reaches the seabed
    "outer_radius": 90.0,    # m, edge of the seabed
    "seabed": -4.0,          # m, seabed height
    "grass_above": -0.15,    # m, faces above this height are Grass, below Sand
    "coast_wobble": 0.08,    # fraction of the radius the coastline wanders by
    "rings": 40,             # rings from the centre out
    "segments": 96,          # around
    "grass_rgb": [0.42, 0.48, 0.42],  # probe preview only
    "sand_rgb": [0.76, 0.69, 0.52],
}


def height_at(params: dict, r: float) -> float:
    """Ground height at radius r: flat top, a smoothstep beach, flat seabed."""
    top, beach = params["top_radius"], params["beach_radius"]
    if r <= top:
        return 0.0
    if r >= beach:
        return params["seabed"]
    t = (r - top) / (beach - top)
    return params["seabed"] * t * t * (3 - 2 * t)


def _wobble(params: dict, angle: float) -> float:
    w = params["coast_wobble"]
    return 1.0 + w * (0.6 * math.sin(3 * angle + 0.7) + 0.4 * math.sin(5 * angle + 2.1))


def _ring_radii(params: dict) -> list[float]:
    """Ring radii, dense across the beach where the shape changes, sparse elsewhere."""
    top, beach, outer = params["top_radius"], params["beach_radius"], params["outer_radius"]
    n = params["rings"]
    inner = [top * (i / 8) for i in range(1, 8)]
    shore = [top + (beach - top) * i / (n - 16) for i in range(n - 15)]
    sea = [beach + (outer - beach) * i / 8 for i in range(1, 9)]
    return inner + shore + sea


def build(params: dict) -> None:
    """Make the island as one mesh object named Island, origin at the centre, top at height 0."""
    seg = params["segments"]
    verts = [(0.0, 0.0, 0.0)]
    faces: list[tuple[int, ...]] = []
    radii = _ring_radii(params)
    for r in radii:
        for k in range(seg):
            a = 2 * math.pi * k / seg
            rr = r * _wobble(params, a) if r > params["top_radius"] * 0.5 else r
            # The height follows the unwobbled radius, so the coast moves but the profile holds.
            verts.append((rr * math.cos(a), rr * math.sin(a), height_at(params, r)))
    for k in range(seg):
        faces.append((0, 1 + k, 1 + (k + 1) % seg))
    for i in range(len(radii) - 1):
        a0 = 1 + i * seg
        a1 = 1 + (i + 1) * seg
        for k in range(seg):
            j = (k + 1) % seg
            faces.append((a0 + k, a1 + k, a1 + j, a0 + j))

    mesh = bpy.data.meshes.new("Island")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    for name, rgb in (("Grass", params["grass_rgb"]), ("Sand", params["sand_rgb"])):
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*rgb, 1)
        mesh.materials.append(mat)
    for poly in mesh.polygons:
        z = sum(mesh.vertices[v].co.z for v in poly.vertices) / len(poly.vertices)
        poly.material_index = 0 if z > params["grass_above"] else 1
        poly.use_smooth = True
    obj = bpy.data.objects.new("Island", mesh)
    bpy.context.scene.collection.objects.link(obj)
