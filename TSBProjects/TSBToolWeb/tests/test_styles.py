"""The Cyber and Y2K styles: chosen from the Theme sheet, remembered, and usable on every screen."""
import os
import pathlib

import pytest

from conftest import goto, load_rom

SHOTS = os.environ.get("TSB_SHOTS")          # set to a folder to save a screenshot of each screen
WIDE = {"width": 1500, "height": 800}
ROUTES = ("#/home", "#/roster", "#/team", "#/schedule", "#/text", "#/probowl")


def no_sideways_overflow(page):
    return page.evaluate("document.documentElement.scrollWidth <= document.documentElement.clientWidth")


def shot(page, name):
    if SHOTS:
        pathlib.Path(SHOTS).mkdir(parents=True, exist_ok=True)
        page.screenshot(path=str(pathlib.Path(SHOTS) / f"{name}.png"))


@pytest.mark.parametrize("style", ["cyber", "y2k", "aqua", "xp"])
def test_style_is_chosen_in_the_theme_sheet_and_remembered(make_page, style):
    page = make_page()
    page.click('[data-act="openthemes"]')
    page.click(f'[data-act="theme:{style}"]')
    assert page.evaluate("document.body.className").startswith(f"theme-{style}")
    assert page.evaluate("localStorage.getItem('tsb.theme')") == style
    assert page.locator(f'[data-act="theme:{style}"].on').count() == 1
    assert page.locator('.themebtn.on').count() == 1                 # no team theme shows as picked
    page.click('[data-act="themeset:bills"]')                         # a team theme returns to Modern
    assert page.evaluate("document.body.className").startswith("theme-modern")


@pytest.mark.parametrize("style", ["cyber", "y2k", "aqua", "xp"])
@pytest.mark.parametrize("viewport", [None, WIDE], ids=["phone", "wide"])
def test_style_fits_every_screen(make_page, style, viewport):
    page = make_page(theme=style, **({"viewport": viewport} if viewport else {}))
    load_rom(page, "snes2")
    for h in ROUTES:
        goto(page, h)
        page.wait_for_timeout(150)
        assert no_sideways_overflow(page), (style, h)
        shot(page, f"{style}_{'wide' if viewport else 'phone'}_{h[2:]}")
    page.locator('[data-act="help"]').first.click()
    page.click('[data-act="helpabout"]')
    shot(page, f"{style}_{'wide' if viewport else 'phone'}_about")
