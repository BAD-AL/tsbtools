"""Every screen and dialog on every ROM family opens without a JavaScript error and shows its content."""
import pytest

from conftest import ROMS, TABLET, goto, load_rom

# route (hash), selector that must exist
SCREENS = [
    ("#/roster", ".row"),
    ("#/team", ".tabbody"),
    ("#/schedule", ".game"),
    ("#/probowl", ".pbspot"),
    ("#/text", "#editor"),
]


@pytest.mark.parametrize("rom", ROMS)
@pytest.mark.parametrize("route,marker", SCREENS, ids=[s[0] for s in SCREENS])
def test_screen_opens(make_page, rom, route, marker):
    page = make_page()
    load_rom(page, rom)
    goto(page, route)
    assert page.locator(marker).count() > 0


@pytest.mark.parametrize("rom", ROMS)
def test_player_editor_wide(make_page, rom):
    page = make_page(viewport=TABLET)
    load_rom(page, rom)
    goto(page, "#/roster")
    page.locator(".row").first.click()
    page.wait_for_selector(".card.jersey")
    assert "EDIT PLAYER" in page.inner_text("body").upper()


@pytest.mark.parametrize("theme", ["modern", "retro"])
def test_both_themes(make_page, theme):
    page = make_page(theme=theme)
    load_rom(page, "snes3")
    assert page.locator(f"body.theme-{theme}").count() == 1
