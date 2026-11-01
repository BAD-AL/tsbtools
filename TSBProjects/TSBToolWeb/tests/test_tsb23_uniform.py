r"""TSB2/TSB3 uniform tab: Home (Uniform1) and Away (Uniform2) of 12 colors each, saved into both skin palettes of the ROM.
Tables: TSB3 home 0x19BD60 / away 0x19C4E0, TSB2 home 0x19BE60 / away 0x19C560, 0x40 bytes per team (TSBTool_Test\TSB2_TSB3_Uniform_Findings.md)."""
import re

import pytest

from conftest import download_rom, goto, load_rom, rom_path
from test_ui_edits_to_text import has_class, line_diff
from test_ui_to_text import dump_text

TABLES = {"snes2": (0x19BE60, 0x19C560, 28), "snes3": (0x19BD60, 0x19C4E0, 30)}
SLOTS = ["tHelmetD", "tHelmetL", "tJerseyD", "tJerseyL", "tPantsD", "tPantsL", "tSocksD", "tSocksL", "tPStripeD", "tPStripeL", "tHStripeD", "tHStripeL"]
PALETTE_INDEX = [1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14]       # palette entry of each slot, in the order of the text


def open_uniform_tab(page):
    goto(page, "#/team")
    page.wait_for_selector(".tabbody")
    page.click('[data-act="ttab:uniform"]')
    page.wait_for_selector("#uc-home")


def pick_other_swatch(page):
    swatches = page.locator('[data-act^="ulv:"]')
    swatches.nth(1 if has_class(swatches.nth(0), "on") else 0).click()
    page.click('[data-act="uok"]')


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_home_and_away_sections_have_twelve_colors_each(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    open_uniform_tab(page)
    assert page.locator("#uc-home").count() == 1 and page.locator("#uc-away").count() == 1      # both pictures side by side
    for i, key in enumerate(("Uniform1", "Uniform2")):
        page.click(f'[data-act="usel:{i}"]')
        for slot in SLOTS:
            assert page.locator(f'[data-act="ucolor:{key}:{slot}"]').count() == 1, (key, slot)
        assert page.locator(f'[data-act="ucolor:{"Uniform2" if i == 0 else "Uniform1"}:{SLOTS[0]}"]').count() == 0   # only the selected uniform's colors are listed


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
@pytest.mark.parametrize("key,slot", [("Uniform1", "tHelmetD"), ("Uniform2", "tSocksL")])
def test_color_edit_changes_one_color_in_the_text_and_both_rom_palettes(make_page, rom, key, slot):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    open_uniform_tab(page)
    page.click(f'[data-act="usel:{0 if key == "Uniform1" else 1}"]')
    page.click(f'[data-act="ucolor:{key}:{slot}"]')
    page.wait_for_selector("#ubig")
    pick_other_swatch(page)
    page.click('[data-act="tdone"]')
    after = dump_text(page)
    d = line_diff(before, after)
    assert len(d) == 1 and d[0][1].startswith("COLORS") and d[0][2].startswith("COLORS"), d
    old = dict(re.findall(r"(\w+)=0x([0-9a-fA-F]+)", d[0][1]))
    new = dict(re.findall(r"(\w+)=0x([0-9a-fA-F]+)", d[0][2]))
    assert [k for k in old if old[k] != new[k]] == [key]
    k = SLOTS.index(slot)
    words = [i for i in range(0, 48, 4) if old[key][i:i + 4] != new[key][i:i + 4]]
    assert words == [k * 4], (old[key], new[key])

    goto(page, "#/home")
    saved, orig = download_rom(page), rom_path(rom).read_bytes()
    home, away, count = TABLES[rom]
    start = (home if key == "Uniform1" else away)           # the first team in the dump is the Bills, team 0
    changed = [i for i in range(home, away + count * 0x40) if saved[i] != orig[i]]       # (names elsewhere may be rewritten: not checked)
    at = start + PALETTE_INDEX[k] * 2
    assert set(changed) <= {at, at + 1, at + 0x20, at + 0x21}, [hex(i) for i in changed]
    assert changed, "the ROM must change"
    want = bytes.fromhex(new[key][k * 4:k * 4 + 4])
    assert saved[at:at + 2] == want and saved[at + 0x20:at + 0x22] == want


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_unedited_dump_leaves_the_uniform_tables_alone(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    goto(page, "#/text")
    page.wait_for_selector("#editor")
    assert "COLORS Uniform1=0x" in page.input_value("#editor")
    page.evaluate("document.getElementById('editor').dispatchEvent(new Event('input', {bubbles:true}))")
    goto(page, "#/home")
    saved, orig = download_rom(page), rom_path(rom).read_bytes()
    home, away, count = TABLES[rom]
    assert saved[home:away + count * 0x40] == orig[home:away + count * 0x40]


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_preview_is_painted_and_changes_when_a_sock_color_changes(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    open_uniform_tab(page)
    pixels = "(() => { const c = document.getElementById('uc-home'); return c.toDataURL(); })()"
    before = page.evaluate(pixels)
    assert page.evaluate("document.getElementById('uc-home').width") == 16
    page.click('[data-act="ucolor:Uniform1:tSocksL"]')
    page.wait_for_selector("#ubig")
    pick_other_swatch(page)
    page.click('[data-act="tdone"]')
    page.wait_for_selector("#uc-home")
    assert page.evaluate(pixels) != before


def colors_of(page, team):
    text = dump_text(page).split("SEASON")[1]
    m = re.search(r"TEAM = " + team + r"\b.*?COLORS Uniform1=0x([0-9a-f]{48}), Uniform2=0x([0-9a-f]{48})", text, re.S)
    return m.group(1), m.group(2)


def current_team(page):
    return re.sub(r"\W", "", page.inner_text(".name")).lower()


def stock_of(rom, team):
    import json
    import pathlib
    data = json.loads((pathlib.Path(__file__).parent.parent / "wwwroot" / "img" / "uniform_presets.json").read_text())
    return data["SNES_TSB2" if rom == "snes2" else "SNES_TSB3"][team]


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_start_from_a_stock_team_copies_its_twelve_colors(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    open_uniform_tab(page)
    team = "bills"
    page.click('[data-act="usel:1"]')
    page.click('[data-act="upre:Uniform2"]')
    page.wait_for_selector('[data-act="upstock:49ers:0"]')
    page.click('[data-act="upstock:49ers:0"]')
    page.click('[data-act="tdone"]')
    home, away = colors_of(page, team)
    assert away == stock_of(rom, "49ers")[0]            # the 49ers' home colors are now Bills' away
    assert home == stock_of(rom, team)[0]


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_reset_restores_the_stock_colors_and_copy_duplicates(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    open_uniform_tab(page)
    page.click('[data-act="ucolor:Uniform1:tJerseyD"]')
    page.wait_for_selector("#ubig")
    pick_other_swatch(page)
    page.click('[data-act="ureset:Uniform1"]')
    page.click('[data-act="ucopy:Uniform1"]')           # away := home
    page.click('[data-act="tdone"]')
    home, away = colors_of(page, "bills")
    assert home == stock_of(rom, "bills")[0] and away == home


@pytest.mark.parametrize("match", [False, True])
def test_match_toggle_changes_the_partner_color_only_when_on(make_page, match):
    page = make_page()
    load_rom(page, "snes2")
    open_uniform_tab(page)
    if match:
        page.click('[data-act="umatch"]')
    page.click('[data-act="ucolor:Uniform1:tJerseyL"]')
    page.wait_for_selector("#ubig")
    pick_other_swatch(page)
    page.click('[data-act="tdone"]')
    home = colors_of(page, "bills")[0]
    stock = stock_of("snes2", "bills")[0]
    changed = [i for i in range(0, 48, 4) if home[i:i + 4] != stock[i:i + 4]]
    assert changed == ([8, 12] if match else [12]), changed       # jersey dark is the 3rd color (hex digit 8), jersey light the 4th


def test_my_presets_save_paste_use_and_delete(make_page):
    page = make_page()
    load_rom(page, "snes3")
    open_uniform_tab(page)
    page.click('[data-act="upre:Uniform1"]')
    page.wait_for_selector("#upname")
    page.fill("#upname", "Mine")
    page.click('[data-act="upsave"]')
    page.fill("#uptext", "COLORS Uniform1=0x" + "0a" * 24 + ", Uniform2=0x" + "0b" * 24)
    page.fill("#upname", "Pasted")
    page.click('[data-act="upadd"]')
    assert page.locator('[data-act^="upmine:"]').count() == 3
    page.click('[data-act="updel:0"]')
    assert page.locator('[data-act^="upmine:"]').count() == 2
    page.click('[data-act="upmine:0"]')
    page.click('[data-act="tdone"]')
    assert colors_of(page, "bills")[0] == "0a" * 24


def test_uniform_controls_explain_themselves_with_tooltips_and_hints(make_page):
    page = make_page()
    load_rom(page, "snes2")
    open_uniform_tab(page)
    match = page.locator('[data-act="umatch"]')
    assert "Auto-shade" in match.inner_text() and "light shade and a dark" in match.get_attribute("title")
    assert "Pick one shade and the other follows" in page.inner_text("body")
    for act in ("upre", "ureset", "ucopy"):
        assert page.locator(f'[data-act^="{act}:"]').first.get_attribute("title"), act
