"""Team themes for the Modern style: 32 NFL teams, picked from the sheet that opens on right-click of the Modern button."""
import pathlib
import re

from conftest import goto, load_rom

THEMES = re.findall(r"\['([\w]+)', '([\w ]+)', '(#[0-9a-f]{6})', '(#[0-9a-f]{6})', '(#[0-9a-f]{6})'\]",
                    (pathlib.Path(__file__).parent.parent / "wwwroot" / "themes.js").read_text())


def rgb(h):
    return [int(h[i:i + 2], 16) for i in (1, 3, 5)]


def lum(c):
    f = lambda v: (v / 255 / 12.92) if v / 255 <= 0.03928 else ((v / 255 + 0.055) / 1.055) ** 2.4
    return 0.2126 * f(c[0]) + 0.7152 * f(c[1]) + 0.0722 * f(c[2])


def contrast(a, b):
    la, lb = sorted((lum(a), lum(b)), reverse=True)
    return (la + 0.05) / (lb + 0.05)


def test_there_are_32_themes_with_the_vikings_as_default():
    assert len(THEMES) == 32 and len({t[0] for t in THEMES}) == 32
    assert [t for t in THEMES if t[0] == "vikings"][0][2] == "#a78bfa"


def test_every_highlight_color_reads_on_its_dark_background_and_carries_its_button_text():
    for tid, name, accent, primary, secondary in THEMES:
        bg = [round(c + (t - c) * .88) for c, t in zip(rgb(primary), (6, 10, 20))]
        assert contrast(rgb(accent), bg) >= 4.5, (name, contrast(rgb(accent), bg))
        a = rgb(accent)
        assert max(contrast(a, (20, 17, 10)), contrast(a, (255, 255, 255))) >= 4.5, (name, "button text")


def open_themes(page):
    goto(page, "#/home")
    page.locator('[data-act="theme:modern"]').first.click(button="right")
    page.wait_for_selector(".themegrid")


def body_var(page, name):
    return page.evaluate(f"document.body.style.getPropertyValue('{name}')")


def test_picking_a_team_sets_the_colors_and_survives_a_reload_and_vikings_is_the_default(make_page):
    page = make_page()
    load_rom(page, "snes2")
    assert body_var(page, "--accent") == ""
    open_themes(page)
    assert page.locator(".themegrid .themebtn").count() == 38   # Retro, Cyber, Y2K, Aqua, XP + Current team + 32 teams
    page.click('[data-act="themeset:bills"]')
    assert body_var(page, "--accent") == "#cf644a" and body_var(page, "--bg") != "" and body_var(page, "--g1") != ""
    page.reload()
    page.wait_for_function("document.documentElement.dataset.webready === '1'")
    page.wait_for_selector("#app")
    page.wait_for_function("document.body.style.getPropertyValue('--accent') !== ''")
    assert body_var(page, "--accent") == "#cf644a"
    load_rom(page, "snes2")
    open_themes(page)
    page.click('[data-act="themeset:vikings"]')
    assert body_var(page, "--accent") == "" and body_var(page, "--bg") == ""


def test_retro_style_ignores_the_team_theme(make_page):
    page = make_page()
    load_rom(page, "snes2")
    open_themes(page)
    page.click('[data-act="themeset:packers"]')
    page.click('.btn.primary[data-act="accentclose"]')
    page.click('[data-act="theme:retro"]')
    assert body_var(page, "--accent") == "" and body_var(page, "--bg") == ""


def test_every_theme_has_a_helmet_image_and_the_rail_shows_the_chosen_teams(make_page):
    img = pathlib.Path(__file__).parent.parent / "wwwroot" / "img" / "helmets"
    for tid, *_ in THEMES:
        assert (img / f"{tid}.png").exists(), tid
    page = make_page()
    load_rom(page, "snes2")
    page.set_viewport_size({"width": 1500, "height": 700})
    goto(page, "#/home")
    src = lambda: page.get_attribute(".railmark img", "src")
    assert "/vikings.png" in src()
    open_themes(page)
    page.click('[data-act="themeset:bills"]')
    assert "/bills.png" in src()
    page.click('.btn.ghost[data-act="accentreset"]')
    assert "/vikings.png" in src()


def test_clicking_the_helmet_opens_the_theme_sheet(make_page):
    page = make_page()
    load_rom(page, "snes2")
    page.set_viewport_size({"width": 1500, "height": 700})
    goto(page, "#/home")
    page.click('.railmark[data-act="openthemes"]')
    page.wait_for_selector(".themegrid")
    page.click('[data-act="themeset:chiefs"]')
    assert "/chiefs.png" in page.get_attribute(".railmark img", "src")


def test_retro_rail_keeps_the_chosen_teams_helmet(make_page):
    page = make_page()
    load_rom(page, "snes2")
    page.set_viewport_size({"width": 1500, "height": 700})
    goto(page, "#/home")
    open_themes(page)
    page.click('[data-act="themeset:packers"]')
    page.click('.btn.primary[data-act="accentclose"]')
    page.click('[data-act="theme:retro"]')
    assert "/packers.png" in page.get_attribute(".railmark img", "src")
    assert body_var(page, "--accent") == ""


def pick_team(page, tid):
    goto(page, "#/roster")
    page.wait_for_selector('[data-act="team-open"]')
    page.click('[data-act="team-open"]')
    page.click(f'[data-act="team:{tid}"]')


def test_current_team_theme_follows_the_team_being_viewed_and_is_remembered(make_page):
    page = make_page()
    load_rom(page, "snes2")
    page.set_viewport_size({"width": 1500, "height": 700})
    goto(page, "#/roster")
    open_themes(page)
    page.click('[data-act="themeset:team"]')
    page.click('.btn.primary[data-act="accentclose"]')
    src = lambda: page.get_attribute(".railmark img", "src")
    for tid, helmet, accent in (("bills", "bills", "#cf644a"), ("redskins", "commanders", "#ffb612"), ("oilers", "titans", "#4ba3e3"), ("chiefs", "chiefs", "#ffd23f")):
        pick_team(page, tid)
        assert f"/{helmet}.png" in src() and body_var(page, "--accent") == accent, tid
    goto(page, "#/home")
    page.reload()
    page.wait_for_function("document.documentElement.dataset.webready === '1'")
    page.wait_for_selector("#app")
    load_rom(page, "snes2")
    goto(page, "#/roster")
    page.wait_for_selector('[data-act="team-open"]')
    pick_team(page, "bills")
    assert body_var(page, "--accent") == "#cf644a"          # still following the team after a reload
    open_themes(page)
    assert page.locator('[data-act="themeset:team"][aria-pressed="true"]').count() == 1


def test_phone_home_has_the_helmet_button_top_left_and_it_opens_the_themes(make_page):
    page = make_page()
    page.set_viewport_size({"width": 400, "height": 800})
    assert page.is_visible(".helmetbtn")                                        # even before a ROM is loaded
    load_rom(page, "snes2")
    box, seg = page.locator(".helmetbtn").bounding_box(), page.locator(".topbar .seg").bounding_box()
    assert box["x"] < 30 and box["x"] + box["width"] < seg["x"]                 # left of the style switch
    page.click(".helmetbtn")
    page.wait_for_selector(".themegrid")
    page.click('[data-act="themeset:packers"]')
    page.click('.btn.primary[data-act="accentclose"]')
    assert "/packers.png" in page.get_attribute(".helmetbtn img", "src")
    page.set_viewport_size({"width": 1500, "height": 700})
    page.wait_for_selector(".railmark")                                         # the wide layout has the helmet in the rail instead
    assert not page.is_visible(".helmetbtn")


def test_retro_is_an_option_in_the_theme_sheet(make_page):
    page = make_page()
    page.set_viewport_size({"width": 400, "height": 800})
    load_rom(page, "snes2")
    page.click('[data-act="theme:retro"]')
    page.click(".helmetbtn")                                   # opening the sheet from Retro keeps Retro
    page.wait_for_selector(".themegrid")
    assert page.evaluate("document.body.classList.contains('theme-retro')")
    assert page.locator('.themegrid [data-act="theme:retro"][aria-pressed="true"]').count() == 1
    assert page.locator('.themegrid .themebtn[aria-pressed="true"]').count() == 1      # only Retro is marked
    page.click('[data-act="themeset:packers"]')                 # a team theme means the Modern style
    assert page.evaluate("document.body.classList.contains('theme-modern')")
    assert page.locator('.themegrid [data-act="theme:retro"][aria-pressed="false"]').count() == 1
    page.click('.themegrid [data-act="theme:retro"]')           # and back to Retro from the sheet, which stays open
    assert page.evaluate("document.body.classList.contains('theme-retro')")
    assert page.locator(".themegrid").count() == 1
    assert page.evaluate("localStorage.getItem('tsb.theme')") == "retro"                  # remembered
