"""Measure a model's picture against a reference photograph.

Run from this folder with `uv run --no-sync python measure.py ...`.

    measure SPEC [IMAGE ...]      each patch's median, its ratio to the neutral patch, its spread
    correct RGB --target R G B --current R G B
                                  move a colour (0-1 sRGB) by the ratio of two ratios, in linear
    profile IMAGE X0 Y0 X1 Y1     brightness and chroma along a box, column by column (bands, hoops)
    crop OUT IMAGE:X0,Y0,X1,Y1 ...
                                  crops side by side at the same height, for a look

SPEC is a JSON file:

    {"neutral": "hoop",
     "reference": {"path": "photo.jpg", "patches": {"hoop": [x0, y0, x1, y1], "wood": [...]}},
     "render":    {"patches": {"hoop": [...], "wood": [...]}}}

Boxes are pixels, origin top left, x1 and y1 excluded; the reference and each render have
their own boxes, paired by name. The neutral patch is something grey or white in both
pictures, lit like the rest: dividing by it removes the photograph's white balance and
exposure and the render's, which differ by more than the colours being matched.
Images are read as sRGB bytes with no colour management, so a JPEG and a PNG compare alike.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
from PIL import Image


def load(path: str | Path) -> np.ndarray:
    """An (h, w, 3) float array of 0 to 255."""
    with Image.open(path) as image:
        return np.asarray(image.convert("RGB"), dtype=float)


def patch(image: np.ndarray, box: list[int]) -> tuple[np.ndarray, float]:
    """A box's median colour and its 10th to 90th percentile luminance spread."""
    x0, y0, x1, y1 = box
    flat = image[y0:y1, x0:x1].reshape(-1, 3)
    if flat.size == 0:
        msg = f"box {box} is empty in an image of {image.shape[1]}x{image.shape[0]}"
        raise ValueError(msg)
    low, high = np.percentile(flat.mean(axis=1), [10, 90])
    return np.median(flat, axis=0), float(high - low)


def _fmt(rgb: np.ndarray) -> str:
    return "#" + "".join(f"{round(c):02x}" for c in np.clip(rgb, 0, 255))


def report(tag: str, image: np.ndarray, patches: dict, neutral: str) -> dict:
    """Print each patch's colour, ratio to the neutral patch and spread; return the ratios."""
    base, _ = patch(image, patches[neutral])
    print(f"{tag}\n  {neutral:10} {_fmt(base)} (neutral)")
    ratios = {}
    for name, box in patches.items():
        if name == neutral:
            continue
        rgb, spread = patch(image, box)
        ratio = rgb / np.maximum(base, 1.0)
        ratios[name] = ratio
        print(f"  {name:10} {_fmt(rgb)}  ratio {ratio[0]:.3f} {ratio[1]:.3f} {ratio[2]:.3f}  spread {spread:.1f}")
    return ratios


def measure(spec_path: str, renders: list[str]) -> None:
    spec = json.loads(Path(spec_path).read_text())
    neutral = spec["neutral"]
    ref = spec["reference"]
    target = report(f"reference {ref['path']}", load(ref["path"]), ref["patches"], neutral)
    for render in renders:
        got = report(f"render {render}", load(render), spec["render"]["patches"], neutral)
        for name, ratio in got.items():
            if name in target:
                off = ratio - target[name]
                print(f"  {name:10} off by {off[0]:+.3f} {off[1]:+.3f} {off[2]:+.3f}")


def _to_linear(c: np.ndarray) -> np.ndarray:
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def _to_srgb(c: np.ndarray) -> np.ndarray:
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * np.power(c, 1 / 2.4) - 0.055)


def correct(rgb: list[float], target: list[float], current: list[float]) -> np.ndarray:
    """Scale a 0-1 sRGB colour by target/current per channel, taken in linear light.

    The ratios are measured on sRGB bytes; a ratio of sRGB values is close to the 1/2.2
    power of the ratio of linear values, so the move is made in linear with that exponent.
    """
    lin_ratio = (np.array(target) / np.maximum(np.array(current), 1e-6)) ** 2.2
    out = _to_srgb(np.clip(_to_linear(np.array(rgb)) * lin_ratio, 0, 1))
    print("corrected", " ".join(f"{c:.3f}" for c in out), _fmt(out * 255))
    return out


def profile(image_path: str, box: list[int]) -> None:
    """Per column: luminance and chroma, averaged over the box's rows, with a bar for chroma."""
    x0, y0, x1, y1 = box
    rows = load(image_path)[y0:y1, x0:x1]
    lum = rows.mean(axis=(0, 2))
    chroma = (rows.max(axis=2) - rows.min(axis=2)).mean(axis=0)
    for i in range(0, x1 - x0, 4):
        print(f"{x0 + i:5} lum {lum[i]:5.1f} chroma {chroma[i]:5.1f} {'#' * int(chroma[i] / 3)}")


def crop(out: str, items: list[str], height: int = 400) -> None:
    """Crops side by side, each scaled to the same height."""
    parts = []
    for item in items:
        path, _, box = item.rpartition(":")
        with Image.open(path) as image:
            part = image.convert("RGB").crop(tuple(int(v) for v in box.split(",")))
        parts.append(part.resize((round(part.width * height / part.height), height)))
    sheet = Image.new("RGB", (sum(p.width for p in parts) + 8 * (len(parts) - 1), height), "white")
    x = 0
    for p in parts:
        sheet.paste(p, (x, 0))
        x += p.width + 8
    sheet.save(out)
    print(f"crop: {out}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="cmd", required=True)
    m = sub.add_parser("measure")
    m.add_argument("spec")
    m.add_argument("renders", nargs="*")
    c = sub.add_parser("correct")
    c.add_argument("rgb", type=float, nargs=3)
    c.add_argument("--target", type=float, nargs=3, required=True)
    c.add_argument("--current", type=float, nargs=3, required=True)
    p = sub.add_parser("profile")
    p.add_argument("image")
    p.add_argument("box", type=int, nargs=4)
    k = sub.add_parser("crop")
    k.add_argument("out")
    k.add_argument("items", nargs="+")
    args = parser.parse_args()
    if args.cmd == "measure":
        measure(args.spec, args.renders)
    elif args.cmd == "correct":
        correct(args.rgb, args.target, args.current)
    elif args.cmd == "profile":
        profile(args.image, args.box)
    else:
        crop(args.out, args.items)


if __name__ == "__main__":
    main()
