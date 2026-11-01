"""Genesis uniform tab: the in-game colors (table at 0x57D6C) are editable and saved into both skin blocks."""
from conftest import download_rom, goto, load_rom, rom_path

IN_GAME = 0x57D6C
STRIDE = 0x40


def open_uniform_tab(page):
    goto(page, "#/team")
    page.wait_for_selector(".tabbody")
    page.click('[data-act="ttab:uniform"]')
    page.wait_for_selector("#uc-ig")


def test_in_game_section_is_shown(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)
    body = page.inner_text("body").upper()
    assert "HOME (DARK)" in body and "AWAY (LIGHT)" in body and "COIN TOSS SCREEN" in body
    for slot in ("iHelmetD", "iHelmetL", "iJerseyD", "iJerseyL", "iPantsD", "iPantsL"):
        assert page.locator(f'[data-act="ucolor:InGame:{slot}"]').count() == 1


def test_in_game_color_edit_is_saved_in_both_skin_blocks(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)
    page.click('[data-act="ucolor:InGame:iJerseyL"]')
    page.wait_for_selector("#ubig")
    page.click('[data-act="ufine"]')                   # the sliders sit under "Fine tune"
    for ch in "rgb":                                   # push every channel to a clearly different level
        for _ in range(7):
            page.click(f'[data-act="ulev:{ch}:-1"]')
    page.click('[data-act="uok"]')
    goto(page, "#/home")
    saved, orig = download_rom(page), rom_path("genesis").read_bytes()
    # Only the in-game table is checked: applying any edit also rewrites multi-word last names (kept by design).
    changed = [i for i in range(IN_GAME, IN_GAME + 32 * STRIDE) if saved[i] != orig[i]]
    teams = {(i - IN_GAME) // STRIDE for i in changed}
    assert len(teams) == 1, "only one team's in-game table may change"
    offsets = sorted((i - IN_GAME) % STRIDE for i in changed)
    assert offsets and set(offsets) <= {0x06, 0x07, 0x26, 0x27}, offsets      # jersey light word in block A and block B only
    t = teams.pop()
    assert saved[IN_GAME + t * STRIDE + 6:IN_GAME + t * STRIDE + 8] == b"\x00\x00"
    assert saved[IN_GAME + t * STRIDE + 0x26:IN_GAME + t * STRIDE + 0x28] == b"\x00\x00"


def test_palette_swatch_and_drag_set_the_color(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)
    page.click('[data-act="ucolor:InGame:iJerseyL"]')
    page.wait_for_selector("#usv")
    page.click('[data-act="ulv:7,0,0"]') if page.locator('[data-act="ulv:7,0,0"]').count() else page.locator(".psw").nth(8).click()
    hexed = page.inner_text("#uhex")
    assert hexed.startswith("0") and hexed != "0e40"
    box = page.locator("#usv").bounding_box()          # drag to the top-left corner: white (full brightness, no saturation)
    page.mouse.move(box["x"] + box["width"] / 2, box["y"] + box["height"] / 2)
    page.mouse.down()
    page.mouse.move(box["x"] + 1, box["y"] + 1)
    page.mouse.up()
    assert page.inner_text("#uhex") == "0eee"
    page.click('[data-act="uok"]')
    assert "0eee" in page.inner_text('[data-act="ucolor:InGame:iJerseyL"]')


def test_coin_toss_edit_writes_both_skin_blocks(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)
    assert page.locator("#uc-1").count() == 0, "only one coin-toss uniform is shown"
    page.click('[data-act="ucolor:Uniform1:JerseyL"]')
    page.wait_for_selector("#usv")
    page.click('[data-act="ufine"]')
    for _ in range(7):
        page.click('[data-act="ulev:g:-1"]')
    page.click('[data-act="uok"]')
    goto(page, "#/text")
    page.wait_for_selector("#editor")
    line = next(l for l in page.input_value("#editor").splitlines() if l.startswith("COLORS"))
    import re
    u1, u2 = (re.search(k + r"=0x([0-9a-f]+)", line).group(1) for k in ("Uniform1", "Uniform2"))
    assert u1 == u2


def test_patriots_and_seahawks_show_a_shared_uniform_note(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)
    note = "share one home uniform"
    for team, expect in (("patriots", True), ("bills", False), ("seahawks", True)):
        page.click('[data-act="team-open"]')
        page.click(f'[data-act="team:{team}"]')
        page.wait_for_selector("#uc-ig")
        assert (note in page.inner_text("body")) == expect, team


AWAY = 0x5846C


def test_away_uniform_edit_is_saved_in_both_skin_blocks(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)                                 # first team = Bills = away entry 0
    page.click('[data-act="usel:1"]')                      # the away picture sits beside the home one
    for slot in ("iHelmetD", "iHelmetL", "iJerseyD", "iJerseyL", "iPantsD", "iPantsL"):
        assert page.locator(f'[data-act="ucolor:InGameAway:{slot}"]').count() == 1
    page.click('[data-act="ucolor:InGameAway:iJerseyL"]')
    page.wait_for_selector("#ubig")
    page.click('[data-act="ufine"]')
    for ch in "rgb":
        for _ in range(7):
            page.click(f'[data-act="ulev:{ch}:-1"]')
    page.click('[data-act="uok"]')
    goto(page, "#/home")
    saved, orig = download_rom(page), rom_path("genesis").read_bytes()
    changed = [i for i in range(IN_GAME, AWAY + 20 * STRIDE) if saved[i] != orig[i]]
    away_changed = [i for i in changed if i >= AWAY]
    assert not [i for i in changed if i < AWAY], "the home table must not change"
    assert sorted((i - AWAY) for i in away_changed) and set((i - AWAY) for i in away_changed) <= {0x06, 0x07, 0x26, 0x27}, away_changed
    assert saved[AWAY + 6:AWAY + 8] == b"\x00\x00" and saved[AWAY + 0x26:AWAY + 0x28] == b"\x00\x00"


def test_team_without_an_away_entry_shows_a_note_instead(make_page):
    page = make_page()
    load_rom(page, "genesis")
    open_uniform_tab(page)
    page.click('[data-act="team-open"]')
    page.click('[data-act="team:colts"]')
    page.wait_for_selector("#uc-ig")
    assert page.locator('[data-act^="ucolor:InGameAway:"]').count() == 0
    assert "no separate away uniform" in page.inner_text("body")
