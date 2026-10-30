"""Byte-for-byte check against the desktop editor.

The web app edits the text dump and the .NET core (compiled to WebAssembly) writes the ROM. The desktop command line
(TSBToolSupreme.exe) runs the same core natively. Both are given the same text, so the two ROMs must be identical.
The desktop exe is TSBTool\\bin\\Debug\\TSBToolSupreme.exe unless TSBWEB_CLI points elsewhere.
"""
import os
import pathlib
import shutil
import subprocess

import pytest

from conftest import ROMS, WEB, download_rom, goto, load_rom, rom_path

CLI = pathlib.Path(os.environ.get("TSBWEB_CLI", WEB.parent / "TSBTool" / "bin" / "Debug" / "TSBToolSupreme.exe"))


# The core tidies a few known leftovers while loading (PlayerNames leftover-byte fix). cx18 has one stray 'R' after a name (0x2F5E).
LOAD_FIXES = {"cx18": 1}


def desktop_rom(tmp_path, rom_file, text):
    """Runs the desktop command line: <rom> <data file> -out:<file> (relative names; it splits -out: values on ':')."""
    if not CLI.is_file():
        pytest.skip(f"desktop exe not found: {CLI}")
    ext = rom_file.suffix
    shutil.copy(rom_file, tmp_path / f"in{ext}")
    (tmp_path / "data.txt").write_text(text, encoding="utf-8")
    r = subprocess.run([str(CLI), f"in{ext}", "data.txt", f"-out:out{ext}"], cwd=tmp_path,
                       capture_output=True, text=True, timeout=180)
    out = tmp_path / f"out{ext}"
    assert out.is_file(), f"desktop run produced no ROM\n{r.stdout[-800:]}\n{r.stderr[-800:]}"
    return out.read_bytes()


def web_text(page):
    goto(page, "#/text")
    page.wait_for_selector("#editor")
    return page.input_value("#editor")


def first_difference(a, b):
    n = next((i for i in range(min(len(a), len(b))) if a[i] != b[i]), None)
    if n is None:
        return f"lengths differ: {len(a)} vs {len(b)}"
    return f"first difference at 0x{n:X}: web {a[n]:02X}, desktop {b[n]:02X}; {sum(x != y for x, y in zip(a, b))} bytes differ"


@pytest.mark.parametrize("rom", ROMS)
def test_unedited_download_is_the_original_file(make_page, rom):
    """With no edits the web app hands the ROM back unchanged (apart from LOAD_FIXES). (The desktop applies the dump even when nothing changed, which rewrites
    the multi-word last names such as 'DE BERG' -> 'deBERG'; that round trip is kept by design, so it is not compared here.)"""
    page = make_page()
    load_rom(page, rom)
    web, orig = download_rom(page), rom_path(rom).read_bytes()
    changed = [i for i in range(len(orig)) if web[i] != orig[i]] if len(web) == len(orig) else None
    assert changed is not None, f"size changed: {len(orig)} -> {len(web)}"
    assert len(changed) <= LOAD_FIXES.get(rom, 0), f"{len(changed)} bytes changed on load, first at 0x{changed[0]:X}"


@pytest.mark.parametrize("rom", ROMS)
def test_edited_download_equals_desktop(make_page, tmp_path, rom):
    """Edit a player in the UI, then give the web app's text to the desktop command line: the two ROMs must be identical."""
    page = make_page()
    load_rom(page, rom)
    goto(page, "#/roster")
    page.locator(".row").first.click()
    page.wait_for_selector("#jval")
    page.click('[data-act="jersey:1"]' if int(page.inner_text("#jval")) < 99 else '[data-act="jersey:-1"]')
    page.click("text=Done")
    text = web_text(page)
    goto(page, "#/home")
    web = download_rom(page)
    desktop = desktop_rom(tmp_path, rom_path(rom), text)
    assert web == desktop, first_difference(web, desktop)


@pytest.mark.parametrize("rom", ROMS)
def test_probowl_edit_equals_desktop(make_page, tmp_path, rom):
    """Pro Bowl spots are text too: the core's own "AFC,QB1,colts,QB1" line. Change one in the UI, give the same line to the desktop."""
    page = make_page()
    load_rom(page, rom)
    original = rom_path(rom).read_bytes()
    for team, pos in (("colts", "QB1"), ("dolphins", "QB2")):          # the first choice may already be what the spot holds
        goto(page, "#/probowl")
        page.wait_for_selector(".pbspot")
        spot = page.locator(".pbspot .code").first.inner_text().strip()
        page.locator(".pbspot").first.click()
        page.click('[data-act="pbpickteam"]')
        page.click(f'[data-act="pbteam:{team}"]')
        page.click(f'[data-act="pbpos:{pos}"]')
        page.click('[data-act="pbok"]')
        goto(page, "#/home")
        web = download_rom(page)
        if web != original:
            break
    assert web != original, f"the Pro Bowl edit changed nothing (spot {spot})"
    desktop = desktop_rom(tmp_path, rom_path(rom), f"AFC,{spot},{team},{pos}" + chr(10))
    assert web == desktop, first_difference(web, desktop)


@pytest.mark.parametrize("rom", ["snes2", "snes3"])
def test_tsb23_playbook_edit_equals_desktop(make_page, tmp_path, rom):
    """Change a playbook slot in both playbooks of the first team, then give the web app's text to the desktop command line."""
    page = make_page()
    load_rom(page, rom)
    goto(page, "#/team")
    page.wait_for_selector(".tabbody")
    page.click('[data-act="ttab:plays"]')
    for book in (0, 1):
        page.click(f'[data-act="tbook:{book}"]')
        page.click('[data-act="playopen:pass:3"]')
        page.click('[data-act="playpick:13"]')
        page.click('[data-act="playopen:run:1"]')
        page.click('[data-act="playpick:5"]')
    page.click('[data-act="tdone"]')
    text = web_text(page)
    goto(page, "#/home")
    web = download_rom(page)
    assert web != rom_path(rom).read_bytes()
    desktop = desktop_rom(tmp_path, rom_path(rom), text)
    assert web == desktop, first_difference(web, desktop)
