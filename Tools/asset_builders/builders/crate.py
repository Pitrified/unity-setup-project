"""A small wooden crate with a lid: five boards and a lid board, each a box.

Defaults from a 1940s Swedish margarine shipping crate, about 27 x 26.5 x 13.5 cm
with its lid (references/crate/). Front and back boards run the full width and
cover the end boards; the lid is one board resting on top, overhanging a little.
Flat shaded: every edge is sharp, which is how planks read without a texture.
One material slot, CrateWood.
"""

from __future__ import annotations

import bpy

PARAMS = {
    "width": 0.27,          # m, along X, the face with the stencil
    "depth": 0.265,         # m, along Y
    "height": 0.135,        # m, including the lid
    "board": 0.01,          # m, board thickness
    "lid_front": 0.01,      # m, lid overhang past the front and back boards
    "lid_side": 0.005,      # m, lid overhang past the ends
    "wood_rgb": [0.60, 0.55, 0.47],  # probe preview only; Unity has its own material
}


def _box(name: str, x0: float, x1: float, y0: float, y1: float, z0: float, z1: float) -> bpy.types.Object:
    verts = [(x, y, z) for z in (z0, z1) for y in (y0, y1) for x in (x0, x1)]
    faces = [(0, 2, 3, 1), (4, 5, 7, 6), (0, 1, 5, 4), (2, 6, 7, 3), (0, 4, 6, 2), (1, 3, 7, 5)]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    return obj


def build(params: dict) -> None:
    """Make the crate as one mesh object named Crate, origin at the base centre."""
    w2 = params["width"] / 2
    d2 = params["depth"] / 2
    t = params["board"]
    body = params["height"] - t

    parts = [
        _box("front", -w2, w2, -d2, -d2 + t, 0, body),
        _box("back", -w2, w2, d2 - t, d2, 0, body),
        _box("end_left", -w2, -w2 + t, -d2 + t, d2 - t, 0, body),
        _box("end_right", w2 - t, w2, -d2 + t, d2 - t, 0, body),
        _box("bottom", -w2 + t, w2 - t, -d2 + t, d2 - t, 0, t),
        _box("lid", -w2 - params["lid_side"], w2 + params["lid_side"],
             -d2 - params["lid_front"], d2 + params["lid_front"], body, params["height"]),
    ]

    mat = bpy.data.materials.new("CrateWood")
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (*params["wood_rgb"], 1)
    bsdf.inputs["Roughness"].default_value = 0.8

    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.data.materials.append(mat)
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    crate = bpy.context.view_layer.objects.active
    crate.name = "Crate"
    crate.data.name = "Crate"
    for poly in crate.data.polygons:
        poly.use_smooth = False
