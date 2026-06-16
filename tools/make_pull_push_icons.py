"""Generate ctx-pull.png / ctx-push.png (16x16 RGBA) matching the flat, vivid
context-menu icon style. Drawn on a 64px canvas (= 4x) and downscaled with
LANCZOS for clean anti-aliasing.

Pull = download glyph: down arrow over a tray line  (blue)
Push = upload glyph:   up   arrow over a tray line  (green)

All coordinates are in the 64px working space.
"""
from PIL import Image, ImageDraw

N = 64
OUT = r"C:\GitExtensions\ZimerfeldTree\src\GitExtensions.ZimerfeldTree\Resources"

PULL = (30, 136, 229, 255)   # vivid blue
PUSH = (67, 160, 71, 255)    # vivid green


def render(direction, color, path):
    img = Image.new("RGBA", (N, N), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    if direction == "down":
        d.rounded_rectangle([26, 8, 38, 32], radius=4, fill=color)        # shaft
        d.polygon([(15, 28), (49, 28), (32, 47)], fill=color)             # head (down)
    else:  # up
        d.polygon([(15, 28), (49, 28), (32, 9)], fill=color)             # head (up)
        d.rounded_rectangle([26, 26, 38, 48], radius=4, fill=color)       # shaft

    d.rounded_rectangle([13, 50, 51, 56], radius=3, fill=color)           # tray / base

    img.resize((16, 16), Image.LANCZOS).save(path)
    print("wrote", path)


render("down", PULL, OUT + r"\ctx-pull.png")
render("up", PUSH, OUT + r"\ctx-push.png")
