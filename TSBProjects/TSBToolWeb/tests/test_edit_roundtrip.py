"""An edit made in the UI survives the real download: save the ROM, load it into a fresh page, see the edit."""
import pytest

from conftest import ROMS, download_rom, goto, load_rom


def first_player_jersey(page):
    goto(page, "#/roster")
    page.locator(".row").first.click()
    page.wait_for_selector("#jval")
    return int(page.inner_text("#jval"))


@pytest.mark.parametrize("rom", ROMS)
def test_jersey_edit_is_saved(make_page, tmp_path, rom):
    page = make_page()
    load_rom(page, rom)
    before = first_player_jersey(page)
    page.click('[data-act="jersey:1"]' if before < 99 else '[data-act="jersey:-1"]')
    page.click('[data-act="player-done"]') if page.locator('[data-act="player-done"]').count() else page.click('text=Done')
    goto(page, "#/home")
    saved = download_rom(page)

    out = tmp_path / ("edited" + ROMS[rom][0][ROMS[rom][0].rfind("."):])
    out.write_bytes(saved)
    fresh = make_page()
    load_rom(fresh, path=out)
    after = first_player_jersey(fresh)
    assert after == (before + 1 if before < 99 else before - 1)
