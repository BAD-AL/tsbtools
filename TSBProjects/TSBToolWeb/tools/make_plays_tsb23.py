r"""Builds wwwroot/img/plays_tsb23.png + plays_tsb23.json from the desktop app's TSB2/TSB3 playbook sheets and play names.
Run from anywhere:  python make_plays_tsb23.py     (needs Pillow)
Sheets: TSBTool/TSB2_TSB3/Playbook/TSB2_PB_000..015.png (one sheet per play number 0-F); every slot's 50x59 picture is cropped from
the sheet at fixed positions (the same numbers as PlaySelectForm.cs). Names come from PlaySelectForm.cs (runPlayNames / passPlayNames)."""
import json, pathlib, re
from PIL import Image

HERE = pathlib.Path(__file__).resolve().parent
SRC = HERE.parent.parent / "TSBTool" / "TSB2_TSB3"
OUT = HERE.parent / "wwwroot" / "img"
W, H = 50, 59
RUN = [(166, 28), (135, 93), (199, 93), (166, 159)]      # R1..R4 positions within a playbook sheet
PASS = [(39, 28), (8, 93), (71, 93), (39, 159)]          # P1..P4

sheet = Image.new("RGBA", (16 * W, 8 * H), (0, 0, 0, 0))
sprites = {}
for n in range(16):
    src = Image.open(SRC / "Playbook" / f"TSB2_PB_{n:03d}.png").convert("RGBA")
    for row, (kind, locs) in enumerate((("R", RUN), ("P", PASS))):
        for s, (x, y) in enumerate(locs):
            r = row * 4 + s
            sheet.paste(src.crop((x, y, x + W, y + H)), (n * W, r * H))
            sprites[f"{kind}{s + 1}-{n}"] = {"x": n * W, "y": r * H, "width": W, "height": H}
sheet.save(OUT / "plays_tsb23.png", optimize=True)

code = (SRC / "PlaySelectForm.cs").read_text(encoding="utf-8")
names = {}
for kind, var in (("R", "runPlayNames"), ("P", "passPlayNames")):
    block = re.search(var + r"\s*=\s*new string\[\]\[\]\{(.*?)\};", code, re.S).group(1)
    rows = re.findall(r"new string\[\]\s*\{(.*?)\}", block, re.S)
    assert len(rows) == 4, var
    for s, row in enumerate(rows):
        items = [x.replace("&&", "&") for x in re.findall(r'"([^"]*)"', row)]      # WinForms writes & as &&
        assert len(items) == 16, (var, s, len(items))
        names[f"{kind}{s + 1}"] = items
(OUT / "plays_tsb23.json").write_text(json.dumps({"sprites": sprites, "names": names}, separators=(",", ":")), encoding="utf-8")
print("wrote", OUT / "plays_tsb23.png", "and plays_tsb23.json:", len(sprites), "pictures,", sum(len(v) for v in names.values()), "names")
