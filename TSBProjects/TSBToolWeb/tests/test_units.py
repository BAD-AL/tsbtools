"""Pure-logic tests for model.js, schema.js and schedule.js, run in the browser's own JS engine (no Node needed)."""
import pytest

from conftest import ROMS, goto, load_rom

PLAYER = {
    "tsb1": "QB1, Joe Smith, Face=0x5, #12, 69, 25, 13, 13, 25, 31, 63, 69, 56, 63, 75, [8, 1, 90]",
    "tsb3": "QB1, Joe Smith, Face=0x05, #12, 69, 25, 13, 13, 25, 31, 63, 69, 56, 63, 75, [08, 01, 5A]",
}


@pytest.fixture
def mods(make_page, site):
    page = make_page()
    page.goto(f"{site}/index.html")
    page.wait_for_function("document.documentElement.dataset.webready === '1'")
    return page


def call(page, module, expr, arg=None):
    return page.evaluate(f"async arg => {{ const m = await import('/{module}.js'); return ({expr}); }}", arg)


@pytest.mark.parametrize("family", PLAYER)
def test_player_line_round_trips_exactly(mods, family):
    line = PLAYER[family]
    out = call(mods, "model", "m.buildPlayerLine(m.parsePlayerLine(arg[0], arg[1]), arg[1])", [line, family])
    assert out == line


def test_position_groups(mods):
    got = call(mods, "model", "['QB2','RILB','RCB','LT','RE2','K','P','TE1'].map(m.groupOf)")
    assert got == ["QB", "LB", "DB", "OL", "DL", "K/P", "K/P", "TE"]


def test_face_kinds_per_rom_family(mods):
    got = call(mods, "schema", "['NES','SNES_TSB1','SNES_TSB2','SNES_TSB3','GENESIS'].map(v => m.faceKind(v).kind)")
    assert got == ["portrait", "skin", "race", "race", "skin"]


def test_skin_tone_toggle(mods):
    got = call(mods, "schema", "[m.isDark(m.withTone(0x0a, 'race', true), 'race'), m.isDark(m.withTone(0x8a, 'race', false), 'race'), m.withTone(0x0a, 'race', true)]")
    assert got == [True, False, 0x8a]


def test_schedule_double_booking_is_detected(mods):
    got = call(mods, "schedule", "m.doubleBooked({ games: [{ away: 'bills', home: 'colts' }, { away: 'jets', home: 'bills' }] })")
    assert "bills" in got


def test_tsb23_playbook_line_parses_and_rebuilds_exactly(mods):
    line = "PLAYBOOK R626005E3, P40483E39 "
    got = call(mods, "model", "(() => { const p = m.parsePlaybook23(arg); return [p.run, p.pass, m.buildPlaybook23(p)]; })()", line)
    assert got[0] == [6, 2, 6, 0, 0, 5, 14, 3]            # playbook 1 run slots 1-4, then playbook 2
    assert got[1] == [4, 0, 4, 8, 3, 14, 3, 9]
    assert got[2] == line


def test_tsb23_playbook_parser_rejects_the_tsb1_format(mods):
    got = call(mods, "model", "[m.parsePlaybook23('PLAYBOOK R1215, P7321 '), m.parsePlaybook('PLAYBOOK R626005E3, P40483E39 ')]")
    assert got == [None, None]
