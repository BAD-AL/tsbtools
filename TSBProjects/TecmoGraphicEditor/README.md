# Tecmo Graphic Editor

A browser-based editor for the helmets in NES Tecmo Super Bowl ROMs (the original 28-team ROM and
32-team hacks). It runs entirely in the page: your ROM is never uploaded anywhere.

## What it does

- Two panes (Source and Target). Load a ROM into each by dragging it onto the page or by using the file picker.
- Large and small helmets: right-click a helmet to copy, paste, edit its pixels or colors, and export or import it as a PNG.
- Save writes the edited ROM back to a file. Undo steps back through the edits.
- Style, team colors and light/dark mode are in the header. The `?` button lists the main features.

## ROMs are not included

Commercial ROMs and patches are not part of this repository. Put your own files in a `ROMS/` folder
(it is git-ignored). The tests read `ROMS/TSB 2007-32-105.nes` and other files in that folder.

## Build

Requires the [Dart SDK](https://dart.dev/get-dart) (3.11 or newer).

```
dart pub get
dart compile js web/main.dart -o web/main.dart.js -O2
```

Then open `web/index.html` in a browser (a local web server is not required).

## Test

```
dart analyze lib web test
dart test
```

Run `dart test` from this folder, because the tests use paths relative to it.

## Deploy

Publish only these three files from `web/`, together in one folder:

- `index.html`
- `main.dart.js`
- `Logo.png` (the tab icon)

The `.dart` sources, `main.dart.js.map` and `main.dart.js.deps` are not needed.

## Layout

| Folder | Contents |
| --- | --- |
| `lib/` | ROM reading and editing logic (no browser code, so it is testable on the Dart VM) |
| `web/` | The page: `index.html`, `main.dart` and the `part` files for the small-helmet and theme UI |
| `test/` | Unit tests, including pixel-exact checks against emulator screenshots |
| `research/` | Notes on how the ROM stores its graphics (`NES_graphics_map.md`) |
