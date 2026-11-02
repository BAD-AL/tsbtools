"""The ? button: a Help popup with About and Program guide."""
import pathlib
import re

from conftest import goto, load_rom

ASSEMBLY_INFO = pathlib.Path(__file__).parent.parent.parent / "TSBTool" / "AssemblyInfo.cs"


def desktop_version():
    return re.search(r'^\[assembly: AssemblyVersion\("([\d.]+)"\)\]', ASSEMBLY_INFO.read_text(encoding="utf-8-sig"), re.M).group(1)


def no_sideways_overflow(page):
    return page.evaluate("(()=>{const s=document.querySelector('.sheet');return s.scrollWidth<=s.clientWidth})()")


def test_question_button_opens_a_two_button_popup_even_before_a_rom_is_loaded(make_page):
    page = make_page()
    page.click('[data-act="help"]')
    page.wait_for_selector('[role="dialog"]')
    assert page.locator('.helpmenu .btn').all_inner_texts() == ["About", "Program guide"]
    page.keyboard.press("Escape")
    assert page.locator('[role="dialog"]').count() == 0


def test_about_shows_the_desktop_version_and_the_links(make_page):
    page = make_page()
    page.click('[data-act="help"]')
    page.click('[data-act="helpabout"]')
    body = page.locator(".aboutbody")
    assert f"Version {desktop_version()}" in body.inner_text()
    hrefs = [a.get_attribute("href") for a in body.locator("a").all()]
    assert "https://github.com/BAD-AL/tsbtools" in hrefs and "https://tecmobowl.org/" in hrefs
    assert all(a.get_attribute("target") == "_blank" and "noopener" in a.get_attribute("rel") for a in body.locator("a").all())
    assert "same internal code as the desktop" in body.inner_text()
    assert no_sideways_overflow(page)
    page.click('[role="dialog"] [data-act="help"]')                      # Back returns to the two-button menu
    assert page.locator('.helpmenu .btn').count() == 2


def test_about_names_the_loaded_rom_and_copies_diagnostics(make_page):
    page = make_page()
    load_rom(page, "snes2")
    page.click('[data-act="help"]')
    page.click('[data-act="helpabout"]')
    assert "SNES_TSB2" in page.locator(".aboutbody").inner_text()
    page.evaluate("navigator.clipboard.writeText = async t => { window.__copied = t; }")
    page.click('[data-act="helpcopy"]')
    page.wait_for_function("window.__copied")
    copied = page.evaluate("window.__copied")
    assert desktop_version() in copied and "SNES_TSB2" in copied


def test_program_guide_lists_its_sections_and_scrolls_inside_the_sheet(make_page):
    page = make_page()
    page.click('[data-act="help"]')
    page.click('[data-act="helpguide"]')
    assert page.locator(".guidetoc a").count() == page.locator(".guidebody section").count() >= 5
    page.locator(".guidetoc a").last.click()
    assert page.locator(".guidebody section").last.is_visible()
    assert no_sideways_overflow(page)


def test_question_button_is_on_every_screen_and_in_the_wide_layout(make_page):
    page = make_page()
    load_rom(page, "snes2")
    for h in ("#/home", "#/roster", "#/team", "#/schedule", "#/text", "#/probowl"):
        goto(page, h)
        assert page.locator('[data-act="help"]').count() >= 1, h
    page.set_viewport_size({"width": 1500, "height": 700})
    goto(page, "#/home")
    page.locator('[data-act="help"]').first.click()
    page.wait_for_selector('.helpmenu')
    page.keyboard.press("Escape")
    page.locator('[data-act="openstyles"]').first.click()
    page.click('[data-act="stylepick:retro"]')
    page.locator('[data-act="help"]').first.click()
    page.click('[data-act="helpabout"]')
    assert no_sideways_overflow(page)


def test_guide_topic_link_scrolls_on_the_first_click_and_the_guide_has_pictures(make_page):
    page = make_page()
    page.click('[data-act="help"]')
    page.click('[data-act="helpguide"]')
    assert page.locator(".guidebody img").count() > 0
    hash_before = page.evaluate("location.hash")
    page.click('[data-act="guidego:text"]')
    page.wait_for_timeout(300)
    assert page.evaluate("location.hash") == hash_before                      # no route change, so no re-render
    top = page.evaluate("document.getElementById('guide-text').getBoundingClientRect().top")
    assert top < 200
