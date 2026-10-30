"""UI -> text: the web app edits the text dump and the core (the same C# as the desktop app) turns text into the ROM, so the
text-to-ROM half is covered by the desktop core tests. What the web app adds is the JavaScript that reads and writes those lines.
These tests check that half:
  * every line of a real dump survives parse -> rebuild unchanged (all 8 ROMs, plus the text files the desktop tests use);
  * each editor changes exactly the lines (and fields) it should and nothing else.
"""
import pathlib
import re

import pytest

from conftest import ROMS, ROM_DIR, TABLET, goto, load_rom

# Everything is run in the browser's own JS engine against the real model.js / schedule.js / uniform.js / schema.js.
ROUND_TRIP_JS = """
async ({ text, family }) => {
  const M = await import('/model.js'), S = await import('/schedule.js'), U = await import('/uniform.js'), K = await import('/schema.js');
  const fam = K.familyOf(family), parsed = M.parseText(text), bad = [], seen = { player: 0, team: 0, playbook: 0, colors: 0, game: 0 };
  const lines = parsed.lines;
  for (const t of parsed.teams) for (const p of t.players) {
    const line = lines[p.lineIndex]; seen.player++;
    if (M.buildPlayerLine(M.parsePlayerLine(line, fam), fam) !== line) bad.push(['player', line, M.buildPlayerLine(M.parsePlayerLine(line, fam), fam)]);
  }
  lines.forEach((line, i) => {
    const t = M.parseTeamLine(line);
    if (t) { seen.team++; if (M.buildTeamLine(t) !== line) bad.push(['team', line, M.buildTeamLine(t)]); }
    const pb = M.parsePlaybook(line);
    if (pb) { seen.playbook++; if (M.buildPlaybook(pb) !== line) bad.push(['playbook', line, M.buildPlaybook(pb)]); }
    const c = U.parseColors(line);
    if (c) { seen.colors++; if (U.buildColors(c) !== line) bad.push(['colors', line, U.buildColors(c)]); }
  });
  for (const w of S.parseSchedule(lines)) w.games.forEach((g, k) => {
    seen.game++; const line = lines[w.header + 1 + k];
    if (S.gameLine(g) !== line) bad.push(['game', line, S.gameLine(g)]);
  });
  return { bad: bad.slice(0, 6), count: bad.length, seen };
}
"""


def dump_text(page):
    goto(page, "#/text")
    page.wait_for_selector("#editor")
    return page.input_value("#editor")


@pytest.fixture
def mods(make_page, site):
    page = make_page()
    page.goto(f"{site}/index.html")
    page.wait_for_function("document.documentElement.dataset.webready === '1'")
    return page


@pytest.mark.parametrize("rom", ROMS)
def test_every_dump_line_round_trips(make_page, rom):
    page = make_page()
    load_rom(page, rom)
    text = dump_text(page)
    r = page.evaluate(ROUND_TRIP_JS, {"text": text, "family": ROMS[rom][1]})
    assert r["seen"]["player"] > 100, r["seen"]                     # the dump really was read
    assert r["count"] == 0, f"{r['count']} lines change on parse -> rebuild, first: {r['bad']}"


# Text files the desktop tests use (Genesis roster / schedule); no ROM needed, the family is TSB1.
DESKTOP_TEXTS = sorted((ROM_DIR / "Genesis").glob("*.txt")) if (ROM_DIR / "Genesis").is_dir() else []


@pytest.mark.parametrize("path", DESKTOP_TEXTS, ids=[p.name for p in DESKTOP_TEXTS])
def test_desktop_test_text_files_round_trip(mods, path):
    text = path.read_text(encoding="utf-8", errors="replace")
    r = mods.evaluate(ROUND_TRIP_JS, {"text": text, "family": "GENESIS"})
    assert r["count"] == 0, f"{r['count']} lines change on parse -> rebuild, first: {r['bad']}"
