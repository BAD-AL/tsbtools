"""Each editor changes exactly the line(s) and field(s) it should, and nothing else.

The web app edits the text dump; the core turns text into the ROM (covered by the desktop core tests). So the useful check for
the web side is: do an edit in the UI, then compare the text before and after.
"""
import re

import pytest

from conftest import ROMS, goto, load_rom
from test_ui_to_text import dump_text


def line_diff(before, after):
    """[(line number, before, after)] for the lines that differ; the two texts must have the same number of lines."""
    a, b = before.split("\n"), after.split("\n")
    assert len(a) == len(b), f"line count changed: {len(a)} -> {len(b)}"
    return [(i, x, y) for i, (x, y) in enumerate(zip(a, b)) if x != y]


def tokens(line):
    """Comma separated fields of a dump line, with the sim values in [...] kept as one token."""
    return [t.strip() for t in re.split(r",(?![^\[]*\])", line) if t.strip() != ""]


def open_first_player(page):
    goto(page, "#/roster")
    page.locator(".row").first.click()
    page.wait_for_selector("#jval")


def has_class(locator, name):
    return name in (locator.get_attribute("class") or "").split()


@pytest.mark.parametrize("rom", ROMS)
def test_jersey_edit_changes_only_the_jersey_token(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    open_first_player(page)
    n = int(page.inner_text("#jval"))
    step = 1 if n < 99 else -1
    page.click(f'[data-act="jersey:{step}"]')
    page.click("text=Done")
    d = line_diff(before, dump_text(page))
    assert len(d) == 1, d
    _, old, new = d[0]
    assert re.sub(r"#(\d+)", f"#{n + step}", old, count=1) == new


@pytest.mark.parametrize("rom", ROMS)
def test_rating_edit_changes_only_that_rating(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    for step in ("1", "-1"):                               # the first rating may already sit at the top of the ladder
        open_first_player(page)
        page.click('[data-act="attr:0"]')
        page.click(f'[data-act="rate:{step}"]')
        page.click("text=Done")
        d = line_diff(before, dump_text(page))
        if d:
            break
    assert len(d) == 1, d
    old, new = tokens(d[0][1]), tokens(d[0][2])
    assert len(old) == len(new)
    assert [i for i, (x, y) in enumerate(zip(old, new)) if x != y] == [4], (old, new)     # pos, name, face, jersey, then the first rating


@pytest.mark.parametrize("rom", ROMS)
def test_sim_edit_changes_only_the_first_sim_value(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    open_first_player(page)
    if page.locator('[data-act="sim:0:1"]').count() == 0:
        pytest.skip("no sim values on this player")
    page.click("text=Done")
    for step in ("1", "-1"):
        open_first_player(page)
        page.click(f'[data-act="sim:0:{step}"]')
        page.click("text=Done")
        d = line_diff(before, dump_text(page))
        if d:
            break
    assert len(d) == 1, d
    old, new = d[0][1], d[0][2]
    assert old.split("[")[0] == new.split("[")[0], "everything before the sim values must be unchanged"
    o = [s.strip() for s in old.split("[")[1].rstrip("] ").split(",")]
    n = [s.strip() for s in new.split("[")[1].rstrip("] ").split(",")]
    assert [i for i, (x, y) in enumerate(zip(o, n)) if x != y] == [0], (o, n)


TSB1_FAMILY = ["nes", "cx105", "snes1", "genesis"]


@pytest.mark.parametrize("rom", ["nes", "cx105", "snes1"])        # Genesis has no formation in the dump (the core stubs it)
def test_team_formation_changes_only_the_formation(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    goto(page, "#/team")
    page.wait_for_selector(".tabbody")
    buttons = page.locator('[data-act^="tform:"]')
    assert buttons.count() > 0
    other = next(b for b in range(buttons.count()) if not has_class(buttons.nth(b), "on"))
    buttons.nth(other).click()
    page.click('[data-act="tdone"]')
    d = line_diff(before, dump_text(page))
    assert len(d) == 1, d
    old, new = d[0][1], d[0][2]
    strip = lambda s: re.sub(r"OFFENSIVE_FORMATION\s*=\s*\S+", "", s)
    assert strip(old) == strip(new) and old != new


@pytest.mark.parametrize("rom", TSB1_FAMILY)
def test_playbook_edit_changes_only_one_slot(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    for play in ("8", "1"):                                # slot 1 may already hold play 8
        goto(page, "#/team")
        page.wait_for_selector(".tabbody")
        page.click('[data-act="ttab:plays"]')
        page.click('[data-act="playopen:run:0"]')
        page.click(f'[data-act="playpick:{play}"]')
        page.click('[data-act="tdone"]')
        d = line_diff(before, dump_text(page))
        if d:
            break
    assert len(d) == 1, d
    old, new = d[0][1], d[0][2]
    assert old.startswith("PLAYBOOK") and new.startswith("PLAYBOOK"), d
    r_old, r_new = re.search(r"R(\d{4})", old).group(1), re.search(r"R(\d{4})", new).group(1)
    assert r_old[1:] == r_new[1:] and r_old[0] != r_new[0], (old, new)
    assert old.split("P")[-1] == new.split("P")[-1], "the pass plays must not change"


@pytest.mark.parametrize("rom", ["nes", "cx105", "snes1", "genesis"])
def test_uniform_edit_changes_only_one_colors_value(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    goto(page, "#/team")
    page.wait_for_selector(".tabbody")
    page.click('[data-act="ttab:uniform"]')
    page.wait_for_selector(".colorchip")
    page.locator('[data-act^="ucolor:"]').first.click()
    if page.locator('[data-act^="upal:"]').count():        # NES: the 64-color grid
        cells = page.locator('[data-act^="upal:"]')
        cells.nth(1 if has_class(cells.nth(0), "on") else 0).click()
    else:                                                  # SNES / Genesis: a swatch of the new picker
        swatches = page.locator('[data-act^="ulv:"]')
        swatches.nth(1 if has_class(swatches.nth(0), "on") else 0).click()
        page.click('[data-act="uok"]')
    page.click('[data-act="tdone"]')
    d = line_diff(before, dump_text(page))
    assert len(d) == 1, d
    assert d[0][1].startswith("COLORS") and d[0][2].startswith("COLORS"), d
    old = dict(re.findall(r"(\w+)=0x([0-9a-fA-F]+)", d[0][1]))
    new = dict(re.findall(r"(\w+)=0x([0-9a-fA-F]+)", d[0][2]))
    assert old.keys() == new.keys()
    changed = [k for k in old if old[k] != new[k]]
    if rom == "genesis":                                   # the first chip is the home in-game uniform; the coin-toss values stay
        assert changed == ["InGame"], changed
    else:
        assert changed == ["Uniform1"], changed
    w = 2 if rom in ("nes", "cx105") else 4                # NES colors are one byte each, SNES/Genesis two
    k = changed[0]
    words = [i for i in range(0, len(old[k]), w) if old[k][i:i + w] != new[k][i:i + w]]
    assert len(words) == 1, (old[k], new[k])


@pytest.mark.parametrize("rom", ["nes", "cx105", "snes1", "genesis"])
def test_schedule_swap_changes_only_that_game(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    goto(page, "#/schedule")
    page.wait_for_selector(".game")
    page.locator(".game").first.click()
    page.click('[data-act="gswap"]')
    page.click('[data-act="gok"]')
    d = line_diff(before, dump_text(page))
    assert len(d) == 1, d
    m_old, m_new = (re.match(r"\s*(\S+)\s+at\s+(\S+)", d[0][i]) for i in (1, 2))
    assert (m_old.group(1), m_old.group(2)) == (m_new.group(2), m_new.group(1)), d


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
@pytest.mark.parametrize("book,first_digit", [(0, 0), (1, 4)])
def test_tsb23_playbook_edit_changes_only_one_digit(make_page, rom, book, first_digit):
    """TSB2/TSB3 keep two playbooks per team: PLAYBOOK R<book 1 run x4><book 2 run x4>, P<book 1 pass x4><book 2 pass x4> (hex digits)."""
    page = make_page()
    load_rom(page, rom)
    before = dump_text(page)
    for play in ("F", "0"):                                # slot 1 may already hold play F
        goto(page, "#/team")
        page.wait_for_selector(".tabbody")
        page.click('[data-act="ttab:plays"]')
        page.click(f'[data-act="tbook:{book}"]')
        page.click('[data-act="playopen:run:0"]')
        page.click(f'[data-act="playpick:{int(play, 16)}"]')
        page.click('[data-act="tdone"]')
        d = line_diff(before, dump_text(page))
        if d:
            break
    assert len(d) == 1, d
    old, new = d[0][1], d[0][2]
    assert old.startswith("PLAYBOOK") and new.startswith("PLAYBOOK"), d
    r_old, r_new = re.search(r"R([0-9A-F]{8})", old).group(1), re.search(r"R([0-9A-F]{8})", new).group(1)
    assert [i for i in range(8) if r_old[i] != r_new[i]] == [first_digit], (old, new)
    assert new[new.index(", P"):] == old[old.index(", P"):], "the pass plays must not change"
    assert r_new[first_digit] == play


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_tsb23_playbook_tab_shows_names_and_both_playbooks(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    goto(page, "#/team")
    page.wait_for_selector(".tabbody")
    page.click('[data-act="ttab:plays"]')
    assert page.locator('[data-act^="tbook:"]').count() == 2
    assert page.locator('[data-act^="playopen:run:"]').count() == 4 and page.locator('[data-act^="playopen:pass:"]').count() == 4
    page.click('[data-act="playopen:pass:2"]')
    assert page.locator('[data-act^="playpick:"]').count() == 16
