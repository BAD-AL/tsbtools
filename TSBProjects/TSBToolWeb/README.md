# TSBToolWeb

TSBTool in the browser (phone, tablet, desktop). The real C# core (`TSBTool\Core` and `TSB2_3_Core`, linked in place)
runs as WebAssembly on .NET 8; the UI is plain HTML/CSS/JS with no framework and no build step.

## Layout
* `Program.cs`: `[JSExport]` bridge (`LoadRom`, `GetAll`, `ProcessText`, `GetRom`, `TakeMessages`); `SimFormulas.cs` (auto-update sim data); `Photos.cs` (TSB3 photos).
* `wwwroot/model.js`: parses the text dump into teams and players. The text stays the source of truth;
  edits change lines of text and the core applies them with `ProcessText`.
* `wwwroot/app.js`: screens, routing (`#/home`, `#/roster`, `#/text`), load/save.
* `wwwroot/app.css`: two themes (`theme-modern`, `theme-retro`) sharing one DOM; differences are CSS variables.

## Build and run
```
dotnet publish -c Release
```
Serve `bin\Release\net8.0\publish\wwwroot` with any static server (e.g. `python -m http.server 8080`) and open it.
No workload install is needed (.NET 8 SDK only).

Dev hooks (query string): `?rom=<url>` loads a ROM without the picker, `&theme=retro|modern`, and a hash such as
`#/roster` or `#/text` deep-links to a screen.

* `wwwroot/img/`: sprite sheets (`faces.png` + `faces.json`, `plays_tsb1.png` + `plays_tsb1.json`), copied from the Godot project; sliced with CSS.
* `wwwroot/uniform.js` + `nes_palette.js` + `img/uni_*.png` + `img/uniform_masks.json`: COLORS-line parsing, color formats (SNES 15-bit, Genesis 9-bit, NES palette), preview recoloring (images and pixel tables extracted from the desktop forms by a one-off script).
* `wwwroot/schedule.js`: parses the WEEK n / "away at home" section of the dump.
* `wwwroot/schema.js`: per-ROM-family meaning of each rating/sim column, the rating ladder, and what "Face" means per ROM.

* `wwwroot/photos.js` + `Photos.cs`: SNES TSB3 player photos (85 pictures, 24x32, 16 grays; not part of the text dump, so the photo calls
  read and write the ROM directly). Needs `System.Drawing.Common` for the core's `TSB3Photos` conversion.

Edits are written into the text immediately and handed to the core (`ProcessText`) lazily, before download or from the
Text view's Apply.

## Status
Done: load/save ROM (NES, CXROM, SNES TSB1/2/3, Genesis), Modern/Retro switch, team picker, roster by position group
(plus ALL), player editor (name, jersey, face/skin, ratings on the game's 16-step ladder, sim values), text view with
search and Apply, face picker (NES/CXROM), team editor (sim defense, run/pass ratio, formation, 4+4 playbook slots with play
(tabs: General / Plays / Identity / Uniform (SNES TSB1, Genesis, NES, CXROM; not TSB2/3); plays are thumbnails that open a picker;
Identity = abbreviation/city/name with a ROM-wide character budget, hidden on Genesis).
TSB2/3 edit sim values and team names only (playbook read-only). Schedule: by week (add/edit/remove games, only bye teams offered so no double-booking) and by team. Pro Bowl: AFC/NFC spot lists, team + position picker (conference teams only), edits go straight to the core. Wide layout (>= 900x540: tablets, desktop browsers): nav rail + two panes for Players (roster list beside the editor) or one centered pane; phones keep the single column.
TSB3 photos: photos appear in the roster rows and the player editor (Light/Dark skin toggle beside the photo), and a Photos screen (`#/photos`)
lists all 85. Tap one to give it to another team/position, free it, or change its picture: choose or drop an image (also drop one onto the
player editor's photo box), drag the crop frame on the preview, zoom with the wheel or a pinch (or the sliders), adjust brightness/contrast/sharpen/auto-levels,
and see the result in the game's grays. Not built: PNG export/import of photos.
Genesis Uniform tab: a Home (dark) in-game section (helmet, jersey, pants, dark + light; the core's `InGame=0x` value, table at 0x57D6C, written into both skin blocks), an Away (light) section for the 19 teams that have one (`InGameAway=0x`, table at 0x5846C; which opponents trigger it is a matchup table not yet found for Genesis), and a note that Patriots and Seahawks share one home uniform above one coin-toss uniform (`Uniform1`, copied to `Uniform2` on every edit so both skin blocks match; shown on the coin-toss screen only). The color picker (SNES/Genesis) has a saturation/brightness square, hue strip, palette grid, this team's colors and optional R/G/B fine tune. Previews use the emulator's channel levels (`GENESIS_SHADES` in uniform.js).
Dialogs can mark a footer with `class="sheetfoot"`; `wrapSheets()` keeps it below the scrolling part.
Design mockups: the "TSBTool Web Mockups" canvas artifact.
Round-trip (dump, apply, save) was verified byte-identical against `TSBToolSupreme.exe` on all five ROM families.

## Tests
`tests/` has Playwright + pytest browser tests (smoke on every ROM and screen, edit-save-reload, byte-for-byte vs the desktop editor, TSB3 photos, JS logic). Double-click `tests/setup.bat` once, then `tests/run_tests.bat`; see `tests/README.md`.
