"""A flat square grid for the water surface; the shader displaces it.

Evenly spaced quads, so the vertex waves have the same resolution everywhere, centred
on the origin at height 0. The size and resolution set the triangle count (2 * cells^2).
One material slot, Water (Unity's material is the water shader, not this preview).
"""

from __future__ import annotations

import bpy

PARAMS = {
    "size": 260.0,    # m, side of the square; past the island's seabed so its edge stays out of sight
    "cells": 130,     # quads along a side: 2 m quads, 33 800 triangles; the shortest wave should be 3 m or more
    "water_rgb": [0.10, 0.35, 0.45],  # probe preview only
}


def build(params: dict) -> None:
    """Make the grid as one mesh object named WaterGrid."""
    n = params["cells"]
    half = params["size"] / 2
    step = params["size"] / n
    verts = [(-half + i * step, -half + j * step, 0.0) for j in range(n + 1) for i in range(n + 1)]
    faces = []
    for j in range(n):
        for i in range(n):
            a = j * (n + 1) + i
            faces.append((a, a + 1, a + n + 2, a + n + 1))
    mesh = bpy.data.meshes.new("WaterGrid")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    # UVs in metres / size, 0 to 1 over the grid, for the shader's world-aligned lookups.
    uv = mesh.uv_layers.new(name="UVMap")
    for loop in mesh.loops:
        x, y, _ = mesh.vertices[loop.vertex_index].co
        uv.data[loop.index].uv = ((x + half) / params["size"], (y + half) / params["size"])
    mat = bpy.data.materials.new("Water")
    mat.use_nodes = True
    mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*params["water_rgb"], 1)
    mesh.materials.append(mat)
    for poly in mesh.polygons:
        poly.use_smooth = True
    obj = bpy.data.objects.new("WaterGrid", mesh)
    bpy.context.scene.collection.objects.link(obj)
