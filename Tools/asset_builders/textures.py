"""Generate the project's procedural textures as PNG, deterministically.

Run from this folder:

    uv run --no-sync python textures.py water_normals

Each texture is a function below. Tiling comes from building the height field out of
waves with integer frequencies over the tile, so the edges meet. The same seed gives
the same bytes, so git sees only real changes.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image

REPO_ROOT = Path(__file__).resolve().parents[2]
TEXTURES_DIR = REPO_ROOT / "UnitySetup" / "Assets" / "Art" / "Textures"


def tiling_height(size: int, waves: int, min_freq: int, max_freq: int, falloff: float, seed: int) -> np.ndarray:
    """A tiling height field in [-1, 1]: a sum of plane waves with integer frequencies."""
    rng = np.random.default_rng(seed)
    y, x = np.mgrid[0:size, 0:size] / size
    h = np.zeros((size, size))
    for _ in range(waves):
        fx, fy = rng.integers(-max_freq, max_freq + 1, size=2)
        f = float(np.hypot(fx, fy))
        if f < min_freq:
            continue
        phase = rng.uniform(0, 2 * np.pi)
        h += np.sin(2 * np.pi * (fx * x + fy * y) + phase) / f ** falloff
    return h / np.abs(h).max()


def normal_map(height: np.ndarray, strength: float) -> Image.Image:
    """Tangent-space normal map from a tiling height field (wrapped gradients), RGB 0-255."""
    dx = (np.roll(height, -1, axis=1) - np.roll(height, 1, axis=1)) * 0.5
    dy = (np.roll(height, -1, axis=0) - np.roll(height, 1, axis=0)) * 0.5
    n = np.stack([-dx * strength, -dy * strength, np.ones_like(height)], axis=-1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    rgb = np.round((n * 0.5 + 0.5) * 255).astype(np.uint8)
    return Image.fromarray(rgb, "RGB")


def water_normals() -> Path:
    """Small choppy detail for the water surface, tiled a few times per wavelength."""
    height = tiling_height(size=256, waves=48, min_freq=3, max_freq=14, falloff=1.2, seed=11)
    out = TEXTURES_DIR / "T_WaterNormal.png"
    out.parent.mkdir(parents=True, exist_ok=True)
    normal_map(height, strength=24.0).save(out, optimize=False)
    return out


TEXTURES = {"water_normals": water_normals}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("name", choices=sorted(TEXTURES))
    args = parser.parse_args()
    print(f"texture: {TEXTURES[args.name]()}")


if __name__ == "__main__":
    main()
