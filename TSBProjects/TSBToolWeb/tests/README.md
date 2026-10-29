# TSBToolWeb tests

Browser tests (Playwright + pytest) that drive the real app in Edge: ROMs are uploaded through the file chooser, screens are
clicked through, edits are downloaded and compared.

## Run them (double-click or from a prompt)
| File | What it does |
|---|---|
| `setup.bat` | One time: installs `playwright` and `pytest` (pip) and builds the .NET runtime. |
| `publish.bat` | Rebuilds the .NET runtime. **Run it after changing any C#** (`Program.cs`, `Photos.cs`, `SimFormulas.cs`, or the shared core). |
| `run_tests.bat` | Everything, about 2 minutes. Extra arguments go to pytest: `run_tests.bat -k photos`, `run_tests.bat -x`. |
| `run_quick.bat` | JS logic + TSB3 photo tests, about 15 seconds. |
| `run_desktop_compare.bat` | Only the byte-for-byte comparison against the desktop editor. |

## What matters
* **Publish first.** The tests need `bin\Release\net8.0\publish\wwwroot\_framework` (the WebAssembly runtime). `setup.bat`/`publish.bat` create it.
  If the C# changed and you did not republish, the tests run against stale C#.
* **JS/CSS/HTML need no publish.** The test server serves `wwwroot` straight from the source folder.
* **ROMs** come from `TSBTool\TSBTool_Test\ROMs` (override with the `TSBWEB_ROMS` environment variable). A missing ROM skips its tests.
* **Desktop comparison** uses `TSBTool\bin\Debug\TSBToolSupreme.exe` (override with `TSBWEB_CLI`). Rebuild the desktop project first if its core changed,
  otherwise you compare against an old build. A missing exe skips those tests.
* **Python** is found with `py`, `python`, or `%LOCALAPPDATA%\Programs\Python\Python314`; set `PYTHON` to use another.
* **Browser** is Edge by default; `--tsb-browser=chromium|chrome` also works (`run_tests.bat --tsb-browser=chrome`).
* Any uncaught JavaScript error or `console.error` in a page fails the test that caused it.

## What is tested
* `test_smoke.py`: every screen on all 8 ROMs (NES, 3 CXROM, SNES TSB1/2/3, Genesis), phone and wide layouts, both themes.
* `test_edit_roundtrip.py`: edit in the UI, press Download, load the file in a fresh page, see the edit.
* `test_matches_desktop.py`: **byte-for-byte against the desktop editor.**
  * Edited: change a player in the UI; the web ROM must equal what `TSBToolSupreme.exe` writes from the same text. Identical on all 8 ROMs.
  * Unedited: the download equals the original file. One known exception: `SbluemanBase_18week_RealNFL-1.nes` loses one stray byte at `0x2F5E`
    (a leftover `R` after a name; the core's leftover-byte cleanup). That is listed in `LOAD_FIXES`.
  * Not compared: an unedited ROM against a desktop run. The desktop applies the dump even with no changes, which rewrites multi-word last names
    (`DE BERG` becomes `deBERG`); that round trip is kept by design.
* `test_photos.py`: SNES TSB3 photos (Photos screen, assignment, drop, drag-pan, wheel zoom, Use this picture).
* `test_units.py`: `model.js` / `schema.js` / `schedule.js` logic, run in the browser's own JS engine (no Node).

## Not covered yet
Real touch input (pinch is only simulated), screenshot comparison, byte comparison of photo edits against `-photosIn`, and edits other than players
(team, schedule, uniforms, Pro Bowl) against the desktop.
