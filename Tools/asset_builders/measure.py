"""Measure a model's picture against a reference photograph.

Run from this folder with `uv run --no-sync python measure.py ...`.

    measure SPEC [IMAGE ...]      each patch's median, its ratio to the neutral patch, its hue
                                  (the ratio over its mean: what survives a neutral that is not
                                  the same object in both pictures), its spread
    correct RGB --target R G B --current R G B
                                  move a colour (0-1 sRGB) by the ratio of two ratios, in linear
    profile IMAGE X0 Y0 X1 Y1     brightness and chroma along a box, column by column (bands, hoops)
    crop OUT IMAGE:X0,Y0,X1,Y1 ...
                                  crops side by side at the same height, for a look

For frame sequences (a folder of frame_NNN.png from the sequence capture):

    sheet FOLDER OUT              all frames on one contact sheet
    gif FOLDER OUT                an animated GIF to watch
    motion FOLDER                 change between consecutive frames, overall and per grid cell
    track FOLDER X0 Y0 X1 Y1      a box's brightness and bright centroid over time (bobbing, waves)

SPEC is a JSON file:

    {"neutral": "hoop",
     "reference": {"path": "photo.jpg", "patches": {"hoop": [x0, y0, x1, y1], "wood": [...]}},
     "render":    {"patches": {"hoop": [...], "wood": [...]}}}

Either side may add "neutral_mode": "luminance" when its neutral is not grey (see report()).

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


def report(tag: str, image: np.ndarray, patches: dict, neutral: str, mode: str = "rgb") -> dict:
    """Print each patch's colour, ratio to the neutral patch and spread; return the ratios.

    mode "rgb" divides channel by channel (the neutral is grey: its tint is the picture's cast);
    "luminance" divides by its brightness only (the neutral is tinted itself: trust the picture's
    white balance for hue).
    """
    base, _ = patch(image, patches[neutral])
    if mode == "luminance":
        base = np.full(3, base.mean())
    print(f"{tag}\n  {neutral:10} {_fmt(base)} (neutral)")
    ratios = {}
    for name, box in patches.items():
        if name == neutral:
            continue
        rgb, spread = patch(image, box)
        ratio = rgb / np.maximum(base, 1.0)
        ratios[name] = ratio
        hue = ratio / ratio.mean()
        print(f"  {name:10} {_fmt(rgb)}  ratio {ratio[0]:.3f} {ratio[1]:.3f} {ratio[2]:.3f}"
              f"  hue {hue[0]:.3f} {hue[1]:.3f} {hue[2]:.3f}  spread {spread:.1f}")
    return ratios


def measure(spec_path: str, renders: list[str]) -> None:
    spec = json.loads(Path(spec_path).read_text())
    neutral = spec["neutral"]
    ref = spec["reference"]
    target = report(f"reference {ref['path']}", load(ref["path"]), ref["patches"], neutral,
                    ref.get("neutral_mode", "rgb"))
    for render in renders:
        got = report(f"render {render}", load(render), spec["render"]["patches"], neutral,
                     spec["render"].get("neutral_mode", "rgb"))
        for name, ratio in got.items():
            if name in target:
                off = ratio - target[name]
                hue_off = ratio / ratio.mean() - target[name] / target[name].mean()
                print(f"  {name:10} off by {off[0]:+.3f} {off[1]:+.3f} {off[2]:+.3f}"
                      f"  hue off by {hue_off[0]:+.3f} {hue_off[1]:+.3f} {hue_off[2]:+.3f}")


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


def _frames(folder: str) -> list[Path]:
    frames = sorted(Path(folder).glob("frame_*.png"))
    if not frames:
        msg = f"no frame_*.png in {folder}"
        raise FileNotFoundError(msg)
    return frames


def sheet(folder: str, out: str, columns: int, width: int) -> None:
    """Every frame of a sequence on one image, in rows, each labelled with its index."""
    from PIL import ImageDraw  # noqa: PLC0415 - only this command draws

    frames = _frames(folder)
    with Image.open(frames[0]) as first:
        height = round(first.height * width / first.width)
    rows = (len(frames) + columns - 1) // columns
    board = Image.new("RGB", (columns * width, rows * height), "white")
    draw = ImageDraw.Draw(board)
    for i, path in enumerate(frames):
        with Image.open(path) as im:
            x, y = (i % columns) * width, (i // columns) * height
            board.paste(im.convert("RGB").resize((width, height)), (x, y))
            draw.text((x + 4, y + 4), str(i), fill="white")
    board.save(out)
    print(f"sheet: {out} ({len(frames)} frames)")


def gif(folder: str, out: str, fps: float, width: int) -> None:
    """The sequence as an animated GIF, to watch."""
    images = []
    for path in _frames(folder):
        with Image.open(path) as im:
            images.append(im.convert("RGB").resize((width, round(im.height * width / im.width))))
    images[0].save(out, save_all=True, append_images=images[1:], duration=round(1000 / fps), loop=0)
    print(f"gif: {out} ({len(images)} frames at {fps} fps)")


def motion(folder: str, grid: int) -> None:
    """Mean absolute change between consecutive frames, overall and per cell of a grid.

    Near zero where nothing moves; a still scene is the control. The grid shows where it moves.
    """
    frames = [load(p) for p in _frames(folder)]
    diffs = [np.abs(b - a).mean(axis=2) for a, b in zip(frames, frames[1:])]
    total = [float(d.mean()) for d in diffs]
    print(f"frames {len(frames)}  mean change per frame {np.mean(total):.2f} (min {min(total):.2f}, max {max(total):.2f})")
    mean = np.mean(diffs, axis=0)
    h, w = mean.shape
    print(f"per cell ({grid}x{grid}, top row first):")
    for r in range(grid):
        cells = [mean[r * h // grid:(r + 1) * h // grid, c * w // grid:(c + 1) * w // grid].mean() for c in range(grid)]
        print("  " + " ".join(f"{v:5.1f}" for v in cells))


def track(folder: str, box: list[int], colour: list[float] | None = None, tolerance: float = 40.0) -> None:
    """A box's mean brightness and a centroid, frame by frame.

    The centroid is of the box's brightest tenth, or with `colour` of the pixels within
    `tolerance` of that sRGB colour (0-255): a green buoy on bright water is not the brightest
    thing in the box. Brightness shows a wave or a ripple passing; the centroid, something
    bobbing or drifting.
    """
    x0, y0, x1, y1 = box
    rows = []
    for i, path in enumerate(_frames(folder)):
        pixels = load(path)[y0:y1, x0:x1]
        region = pixels.mean(axis=2)
        if colour is None:
            mask = region >= np.percentile(region, 90)
        else:
            mask = np.linalg.norm(pixels - np.array(colour), axis=2) <= tolerance
        ys, xs = np.nonzero(mask)
        if len(ys) == 0:
            rows.append((i, float(region.mean()), float("nan"), float("nan")))
            continue
        rows.append((i, float(region.mean()), x0 + float(xs.mean()), y0 + float(ys.mean())))
    for i, lum, cx, cy in rows:
        print(f"  {i:3}  lum {lum:6.1f}  bright centroid ({cx:6.1f}, {cy:6.1f})")
    lums = np.array([r[1] for r in rows])
    cys = np.array([r[3] for r in rows])
    print(f"lum range {lums.min():.1f}..{lums.max():.1f}  centroid y range {np.nanmin(cys):.1f}..{np.nanmax(cys):.1f}")


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
    sh = sub.add_parser("sheet")
    sh.add_argument("folder")
    sh.add_argument("out")
    sh.add_argument("--columns", type=int, default=8)
    sh.add_argument("--width", type=int, default=240)
    g = sub.add_parser("gif")
    g.add_argument("folder")
    g.add_argument("out")
    g.add_argument("--fps", type=float, default=10)
    g.add_argument("--width", type=int, default=480)
    mo = sub.add_parser("motion")
    mo.add_argument("folder")
    mo.add_argument("--grid", type=int, default=4)
    t = sub.add_parser("track")
    t.add_argument("folder")
    t.add_argument("box", type=int, nargs=4)
    t.add_argument("--colour", type=float, nargs=3, metavar=("R", "G", "B"), help="follow pixels near this sRGB colour")
    t.add_argument("--tolerance", type=float, default=40.0)
    args = parser.parse_args()
    if args.cmd == "measure":
        measure(args.spec, args.renders)
    elif args.cmd == "correct":
        correct(args.rgb, args.target, args.current)
    elif args.cmd == "profile":
        profile(args.image, args.box)
    elif args.cmd == "sheet":
        sheet(args.folder, args.out, args.columns, args.width)
    elif args.cmd == "gif":
        gif(args.folder, args.out, args.fps, args.width)
    elif args.cmd == "motion":
        motion(args.folder, args.grid)
    elif args.cmd == "track":
        track(args.folder, args.box, args.colour, args.tolerance)
    else:
        crop(args.out, args.items)


if __name__ == "__main__":
    main()
