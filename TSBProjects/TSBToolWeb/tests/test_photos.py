"""SNES TSB3 photos: the Photos screen, assignment, and the picture editor with real mouse/wheel input."""
import pathlib

from conftest import TABLET, goto, load_rom

PNG_JS = """async () => {
  const c = document.createElement('canvas'); c.width = 80; c.height = 100;
  const x = c.getContext('2d'); x.fillStyle = '#888'; x.fillRect(0, 0, 80, 100); x.fillStyle = '#fff'; x.fillRect(20, 20, 30, 40);
  const blob = await new Promise(r => c.toBlob(r, 'image/png'));
  return Array.from(new Uint8Array(await blob.arrayBuffer()));
}"""


def drop_image(page, selector):
    """Drops a generated PNG on an element (the browser cannot be handed a real OS file drag, so this builds the DataTransfer)."""
    data = page.evaluate(PNG_JS)
    page.evaluate("""([sel, bytes]) => {
      const dt = new DataTransfer();
      dt.items.add(new File([new Uint8Array(bytes)], 'face.png', { type: 'image/png' }));
      const el = document.querySelector(sel);
      el.dispatchEvent(new DragEvent('dragover', { dataTransfer: dt, bubbles: true, cancelable: true }));
      el.dispatchEvent(new DragEvent('drop', { dataTransfer: dt, bubbles: true, cancelable: true }));
    }""", [selector, data])


def open_editor_on_photo0(page):
    goto(page, "#/photos")
    page.click('[data-act="photo:0"]')
    page.click('[data-act="phpic"]')
    drop_image(page, "#phdrop")
    page.wait_for_selector("#phsrc")


def test_photos_screen_lists_every_photo(make_page):
    page = make_page()
    load_rom(page, "snes3")
    goto(page, "#/photos")
    assert page.locator(".pcard").count() == 85
    assert "84 of 84" in page.inner_text("body")


def test_other_roms_have_no_photos_entry(make_page):
    page = make_page(viewport=TABLET)
    load_rom(page, "snes2")
    assert page.locator('[data-act="go:photos"]').count() == 0


def test_roster_row_shows_the_photo(make_page):
    page = make_page()
    load_rom(page, "snes3")
    goto(page, "#/roster")
    assert page.locator(".row img.phimg").count() >= 1


def test_assigning_a_photo_moves_it(make_page):
    page = make_page()
    load_rom(page, "snes3")
    goto(page, "#/photos")
    page.click('[data-act="photo:0"]')
    page.click('[data-act="phchange"]')
    page.click('[data-act="phpos:QB2"]')
    page.click('[data-act="phassign"]')
    page.click('[data-act="phclose"]')
    card = page.locator('[data-act="photo:0"]')
    assert "QB2" in card.inner_text()


def test_assign_dialog_footer_does_not_scroll(make_page):
    page = make_page()
    load_rom(page, "snes3")
    goto(page, "#/photos")
    page.click('[data-act="photo:0"]')
    page.click('[data-act="phchange"]')
    foot = page.locator(".sheet > .sheetfoot")
    assert foot.count() == 1
    box = foot.bounding_box()
    assert box["y"] + box["height"] <= page.viewport_size["height"]


def test_dropping_an_image_on_the_player_photo_box_opens_the_editor(make_page):
    page = make_page(viewport=TABLET)
    load_rom(page, "snes3")
    goto(page, "#/roster")
    page.locator(".row").first.click()
    page.wait_for_selector('[data-act="pl-photo"]')
    drop_image(page, '[data-act="pl-photo"]')
    page.wait_for_selector("#phsrc")
    assert "PICTURE FOR PHOTO" in page.inner_text(".sheet h2").upper()


def test_dragging_the_crop_frame_moves_the_pan_sliders(make_page):
    page = make_page()
    load_rom(page, "snes3")
    open_editor_on_photo0(page)
    before = (page.input_value('[data-ph="panx"]'), page.input_value('[data-ph="pany"]'))
    box = page.locator("#phsrc").bounding_box()
    page.mouse.move(box["x"] + 20, box["y"] + 20)
    page.mouse.down()
    page.mouse.move(box["x"] + 50, box["y"] + 30, steps=5)
    page.mouse.up()
    after = (page.input_value('[data-ph="panx"]'), page.input_value('[data-ph="pany"]'))
    assert after != before


def test_mouse_wheel_zooms(make_page):
    page = make_page()
    load_rom(page, "snes3")
    open_editor_on_photo0(page)
    page.set_viewport_size({"width": 500, "height": 900})
    z0 = float(page.input_value('[data-ph="zoom"]'))
    box = page.locator("#phsrc").bounding_box()
    page.mouse.move(box["x"] + 20, box["y"] + 20)
    page.mouse.wheel(0, -300)
    page.wait_for_function("z => parseFloat(document.querySelector('[data-ph=zoom]').value) > z", arg=z0)


def test_use_this_picture_changes_the_photo(make_page):
    page = make_page()
    load_rom(page, "snes3")
    goto(page, "#/photos")
    old = page.locator('[data-act="photo:0"] img').get_attribute("src")
    open_editor_on_photo0(page)
    page.click('[data-act="phuse"]')
    page.wait_for_function("old => document.querySelector('[data-act=\"photo:0\"] img').src !== old", arg=old)
