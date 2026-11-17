"""A wooden barrel: bellied staves, recessed heads, iron hoops.

Defaults from a 225 l Bordeaux barrique (about 0.95 m long, 0.56 m across the
heads, 0.72 m at the bilge); see docs/building_assets.md for how they were chosen.
The staves are flat faces around the barrel with sharp seams between them and a
smooth curve along their length, which is how the staves read at a distance
without a texture. One mesh, two material slots: Wood and Iron.
"""

from __future__ import annotations

import math

import bpy

PARAMS = {
    "height": 0.95,            # m, head to head including the chimes
    "head_diameter": 0.56,     # m, outer, at the ends of the staves
    "bilge_diameter": 0.72,    # m, outer, at the middle
    "staves": 24,              # flat faces around
    "rings": 12,               # segments along the height
    "chime": 0.025,            # m, how far the staves stand beyond the heads
    "stave_thickness": 0.025,  # m, seen at the rims
    "hoops": [0.05, 0.2, 0.36, 0.64, 0.8, 0.95],  # band centres, fraction of the height
    "hoop_width": 0.04,        # m
    "hoop_thickness": 0.006,   # m, how far a band stands off the staves
    "wood_rgb": [0.45, 0.28, 0.15],  # probe preview only; Unity has its own material
    "iron_rgb": [0.16, 0.16, 0.17],
}


def radius_at(params: dict, t: float) -> float:
    """Outer radius of the staves at height fraction t (0 base, 1 top): a parabola through the bilge."""
    r_head = params["head_diameter"] / 2
    r_bilge = params["bilge_diameter"] / 2
    return r_bilge - (r_bilge - r_head) * (2 * t - 1) ** 2


def _material(name: str, rgb: list[float], metallic: float) -> bpy.types.Material:
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes["Principled BSDF"]
    bsdf.inputs["Base Color"].default_value = (*rgb, 1)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = 0.7 if metallic == 0 else 0.45
    return mat


def _ring(radius: float, z: float, n: int) -> list[tuple[float, float, float]]:
    """n points on a circle; the first sits on -Y, the face shown to a probe."""
    return [(radius * math.sin(2 * math.pi * i / n), -radius * math.cos(2 * math.pi * i / n), z) for i in range(n)]


def build(params: dict) -> None:
    """Make the barrel as one mesh object named Barrel, origin at the base centre."""
    n = params["staves"]
    h = params["height"]
    rings = params["rings"]
    chime = params["chime"]
    thick = params["stave_thickness"]

    verts: list[tuple[float, float, float]] = []
    faces: list[tuple[int, ...]] = []
    face_mat: list[int] = []
    sharp_pairs: set[tuple[int, int]] = set()

    def add_ring(radius: float, z: float) -> int:
        start = len(verts)
        verts.extend(_ring(radius, z, n))
        return start

    def bridge(a: int, b: int, mat: int, sharp_sides: bool) -> None:
        """Quads between two rings of n vertices; sharp_sides marks the edges joining them."""
        for i in range(n):
            j = (i + 1) % n
            faces.append((a + i, a + j, b + j, b + i))
            face_mat.append(mat)
            if sharp_sides:
                sharp_pairs.add((min(a + i, b + i), max(a + i, b + i)))

    # Outer staves, base to top.
    outer = [add_ring(radius_at(params, k / rings), h * k / rings) for k in range(rings + 1)]
    for k in range(rings):
        bridge(outer[k], outer[k + 1], 0, sharp_sides=True)

    # Rims and recessed heads, at each end.
    for t, z_head, up in ((1.0, h - chime, True), (0.0, chime, False)):
        r_top = radius_at(params, t)
        z_end = h * t
        rim = add_ring(r_top - thick, z_end)
        inner = add_ring(r_top - thick, z_head)
        end_ring = outer[-1] if up else outer[0]
        if up:
            bridge(end_ring, rim, 0, sharp_sides=False)
            bridge(rim, inner, 0, sharp_sides=True)
            faces.append(tuple(inner + i for i in range(n)))
        else:
            bridge(rim, end_ring, 0, sharp_sides=False)
            bridge(inner, rim, 0, sharp_sides=True)
            faces.append(tuple(inner + i for i in reversed(range(n))))
        face_mat.append(0)

    # Hoops: bands standing off the staves, following the belly.
    for centre in params["hoops"]:
        t0 = centre - params["hoop_width"] / 2 / h
        t1 = centre + params["hoop_width"] / 2 / h
        off = params["hoop_thickness"]
        base0 = add_ring(radius_at(params, t0) - 0.002, h * t0)
        band0 = add_ring(radius_at(params, t0) + off, h * t0)
        band1 = add_ring(radius_at(params, t1) + off, h * t1)
        base1 = add_ring(radius_at(params, t1) - 0.002, h * t1)
        bridge(base0, band0, 1, sharp_sides=False)
        bridge(band0, band1, 1, sharp_sides=False)
        bridge(band1, base1, 1, sharp_sides=False)

    mesh = bpy.data.meshes.new("Barrel")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    for poly, mat in zip(mesh.polygons, face_mat):
        poly.material_index = mat
        poly.use_smooth = True
    sharp = mesh.attributes.new("sharp_edge", "BOOLEAN", "EDGE")
    for edge in mesh.edges:
        a, b = edge.vertices
        edge_sharp = (min(a, b), max(a, b)) in sharp_pairs
        # The hoop bands' edges to the staves are sharp too, so the band reads as a separate part.
        sharp.data[edge.index].value = edge_sharp
    mesh.materials.append(_material("Wood", params["wood_rgb"], 0.0))
    mesh.materials.append(_material("Iron", params["iron_rgb"], 0.6))

    obj = bpy.data.objects.new("Barrel", mesh)
    bpy.context.scene.collection.objects.link(obj)
