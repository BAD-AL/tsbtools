"""Find in the text view: a dialog; Enter closes it and moves to the match in the text."""
from conftest import goto, load_rom


def open_text(page):
    load_rom(page, "snes2")
    goto(page, "#/text")
    page.wait_for_selector("#editor")


def selection(page):
    return page.evaluate("(()=>{const t=document.querySelector('#editor');return [t.selectionStart,t.selectionEnd]})()")


def active(page):
    return page.evaluate("document.activeElement.id")


def test_button_and_ctrl_f_open_the_dialog_with_the_field_focused(make_page):
    page = make_page()
    open_text(page)
    page.click('[data-act="findopen"]')
    assert page.locator('[role="dialog"] #fq').count() == 1 and active(page) == "fq"
    page.keyboard.press("Escape")
    assert page.locator('[role="dialog"]').count() == 0 and active(page) == "editor"
    page.keyboard.press("Control+f")
    assert active(page) == "fq"


def test_enter_closes_the_dialog_and_selects_the_match_in_the_text_without_typing(make_page):
    page = make_page()
    open_text(page)
    before = page.input_value("#editor")
    page.click('[data-act="findopen"]')
    page.fill("#fq", "QB1")
    page.press("#fq", "Enter")
    first = selection(page)
    assert page.locator('[role="dialog"]').count() == 0 and active(page) == "editor"
    assert first[1] - first[0] == 3 and page.input_value("#editor")[first[0]:first[1]].lower() == "qb1"
    assert page.input_value("#editor") == before
    assert page.inner_text("#findinfo").startswith("1 of ")
    page.keyboard.press("F3")
    assert selection(page)[0] > first[0] and page.inner_text("#findinfo").startswith("2 of ")


def test_ctrl_f_again_remembers_the_term_and_shift_enter_goes_back_and_wraps(make_page):
    page = make_page()
    open_text(page)
    page.keyboard.press("Control+f")
    page.fill("#fq", "QB1")
    page.press("#fq", "Enter")
    start = selection(page)
    page.keyboard.press("Control+f")
    assert page.input_value("#fq") == "QB1"
    page.press("#fq", "Shift+Enter")                                    # before the first match: wraps to the last
    assert selection(page)[0] > start[0] and "back at the bottom" in page.inner_text("#findinfo")


def test_no_match_keeps_the_dialog_open_and_says_so(make_page):
    page = make_page()
    open_text(page)
    page.click('[data-act="findopen"]')
    page.fill("#fq", "zzzzqqqq")
    page.press("#fq", "Enter")
    assert page.locator('[role="dialog"]').count() == 1 and page.inner_text("#findmsg") == "No match"


def test_opening_and_closing_find_keeps_unapplied_edits_in_the_text_box(make_page):
    page = make_page()
    open_text(page)
    page.fill("#editor", "hello QB1 world")
    page.keyboard.press("Control+f")
    page.fill("#fq", "world")
    page.press("#fq", "Enter")
    assert page.input_value("#editor") == "hello QB1 world" and selection(page) == [10, 15]
