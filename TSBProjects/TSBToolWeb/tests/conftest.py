"""Shared fixtures for the TSBToolWeb browser tests.

The app is served by a small static server: the JS/CSS/HTML come straight from wwwroot (so a JS edit needs no
republish) and the .NET runtime (_framework) comes from the published build (run `dotnet publish -c Release` once,
and again whenever the C# bridge changes). ROMs are uploaded through the real file chooser.

Choose the browser with --tsb-browser (default msedge): msedge | chrome | chromium | firefox | webkit.
"""
import http.server
import mimetypes
import os
import pathlib
import threading

import pytest
from playwright.sync_api import sync_playwright

HERE = pathlib.Path(__file__).resolve().parent
WEB = HERE.parent
WWWROOT = WEB / "wwwroot"
PUBLISH = WEB / "bin" / "Release" / "net8.0" / "publish" / "wwwroot"
ROM_DIR = pathlib.Path(os.environ.get("TSBWEB_ROMS", WEB.parent / "TSBTool" / "TSBTool_Test" / "ROMs"))

# key -> (file under ROM_DIR, ROM family label)
ROMS = {
    "nes": ("TSPRBOWL.nes", "NES"),
    "cx105": ("TSB 2007-32-105.nes", "CXROM"),
    "cx111": ("TSB 2007-32-111.nes", "CXROM"),
    "cx18": ("SbluemanBase_18week_RealNFL-1.nes", "CXROM"),
    "snes1": ("TSB1.smc", "SNES_TSB1"),
    "snes2": ("TSB2_U.smc", "SNES_TSB2"),
    "snes3": ("TSB3.smc", "SNES_TSB3"),
    "genesis": ("Genesis/Tecmo Super Bowl (USA) (October 1993).md", "GENESIS"),
}

PHONE = {"width": 500, "height": 900}
TABLET = {"width": 1180, "height": 820}

_TYPES = {".js": "text/javascript", ".mjs": "text/javascript", ".css": "text/css", ".json": "application/json",
          ".wasm": "application/wasm", ".html": "text/html", ".png": "image/png", ".dll": "application/octet-stream",
          ".dat": "application/octet-stream", ".svg": "image/svg+xml"}


class _Handler(http.server.BaseHTTPRequestHandler):
    def log_message(self, *args):
        pass

    def do_GET(self):
        rel = self.path.split("?", 1)[0].split("#", 1)[0].lstrip("/") or "index.html"
        rel = pathlib.PurePosixPath(rel)
        if ".." in rel.parts:
            return self.send_error(403)
        for root in (WWWROOT, PUBLISH):                       # source files win; the runtime comes from the publish
            f = root / rel
            if f.is_file():
                data = f.read_bytes()
                kind = _TYPES.get(f.suffix.lower()) or mimetypes.guess_type(f.name)[0] or "application/octet-stream"
                self.send_response(200)
                self.send_header("Content-Type", kind)
                self.send_header("Content-Length", str(len(data)))
                self.send_header("Cache-Control", "no-store")
                self.end_headers()
                self.wfile.write(data)
                return
        self.send_error(404)


@pytest.fixture(scope="session")
def site():
    if not (PUBLISH / "_framework").is_dir():
        pytest.exit("No published runtime. Run `dotnet publish -c Release` in TSBToolWeb first.", returncode=2)
    server = http.server.ThreadingHTTPServer(("127.0.0.1", 0), _Handler)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    yield f"http://127.0.0.1:{server.server_address[1]}"
    server.shutdown()


def pytest_addoption(parser):
    parser.addoption("--tsb-browser", default=os.environ.get("TSBWEB_BROWSER", "msedge"),
                     help="msedge (default) | chrome | chromium | firefox | webkit")


@pytest.fixture(scope="session")
def browser(request):
    name = request.config.getoption("--tsb-browser")
    with sync_playwright() as p:
        if name in ("msedge", "chrome"):
            b = p.chromium.launch(channel=name)
        else:
            b = getattr(p, name).launch()
        yield b
        b.close()


@pytest.fixture
def make_page(browser, site):
    """Opens the app in a fresh page; JS errors fail the test."""
    opened, errors = [], []

    def make(viewport=PHONE, theme="modern", touch=False):
        ctx = browser.new_context(viewport=viewport, accept_downloads=True, has_touch=touch)
        page = ctx.new_page()
        page.on("pageerror", lambda e: errors.append(f"pageerror: {e}"))
        page.on("console", lambda m: errors.append(f"console.error: {m.text}")
                if m.type == "error" and "Failed to load resource" not in m.text else None)
        page.goto(f"{site}/index.html?theme={theme}")
        page.wait_for_function("document.documentElement.dataset.webready === '1'", timeout=60000)
        opened.append(ctx)
        return page

    yield make
    for c in opened:
        c.close()
    assert not errors, "\n".join(errors)


def rom_path(key):
    p = ROM_DIR / ROMS[key][0]
    if not p.is_file():
        pytest.skip(f"ROM not available: {p}")
    return p


def load_rom(page, key=None, path=None):
    """Uploads a ROM through the real file chooser and waits for the Home screen."""
    page.set_input_files("#file", str(path or rom_path(key)))
    page.wait_for_selector('[data-act="go:roster"]', timeout=60000)


def goto(page, route):
    page.evaluate("r => { location.hash = r }", route)
    page.wait_for_timeout(100)


def download_rom(page):
    """Presses the Download button and returns the saved ROM bytes."""
    with page.expect_download() as d:
        page.click('[data-act="download"]')
    return pathlib.Path(d.value.path()).read_bytes()
