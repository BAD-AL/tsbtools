// In-place editing of the small team-select helmets. A helmet is drawn from
// background tiles (a 16x16 area) and, for some teams, overlay sprites; the
// editor changes the pixels of those CHR tiles directly. Tiles are shared
// (see research/NES_graphics_map.md), so [SmallHelmetEditor.alsoAffects] names
// the other teams a change shows up on.
import 'dart:typed_data';

import 'nes_rom.dart';
import 'small_helmet.dart';
import 'small_helmet_renderer.dart';

const smallBgSize = 16;

/// A helmet's pixels as copied: color indexes 0-3 (0 = clear) for the 16x16
/// background area and the overlay canvas, plus the palette of each tile.
class SmallHelmetPicture {
  final String team;
  final Uint8List bg;
  final Uint8List overlay;
  final bool hasOverlay;
  final List<int> tilePalettes;
  SmallHelmetPicture(this.team, this.bg, this.overlay, this.hasOverlay, this.tilePalettes);
}

class PictureResult {
  int droppedBg = 0;
  int droppedOverlay = 0;
  final notes = <String>[];
}

class _Box {
  final int x, y; // top-left on the overlay canvas
  final int key;
  final bool flipX, flipY;
  final int palette;
  _Box(this.x, this.y, this.key, this.flipX, this.flipY, this.palette);

  bool covers(int px, int py) => px >= x && px < x + 8 && py >= y && py < y + 8;

  /// Index into the tile's 64 pixels for canvas point [px],[py].
  int pixelAt(int px, int py) {
    final cx = px - x, cy = py - y;
    return (flipY ? 7 - cy : cy) * 8 + (flipX ? 7 - cx : cx);
  }
}

class SmallHelmetEditor {
  final NesRom rom;
  final int team;
  final SmallHelmetReader reader;
  final SmallHelmet original;

  final List<int?> _bgKeys = []; // per slot; null = empty slot (tile $00)
  final List<_Box> _boxes = [];
  final Map<int, Uint8List> _tiles = {};
  final Map<int, Uint8List> _origTiles = {};
  late List<int> _pal = List.of(original.tilePalettes);
  Map<int, Set<int>>? _users; // tile key -> teams that draw it

  SmallHelmetEditor(this.rom, this.team)
      : reader = SmallHelmetReader(rom),
        original = SmallHelmetReader(rom).read(team) {
    for (final t in original.tiles) {
      _bgKeys.add(t == 0 ? null : _key(SmallHelmetReader.bgBank(t), t));
    }
    for (final s in original.sprites) {
      _boxes.add(_Box(smallHelmetMarginX + s.dx, 1 + s.dy, _key(SmallHelmetReader.spriteBank(s.tile), s.tile),
          s.flipX, s.flipY, s.palette));
    }
    for (final key in [..._bgKeys.whereType<int>(), ..._boxes.map((b) => b.key)]) {
      _tiles.putIfAbsent(key, () => Uint8List.fromList(rom.tile(key >> 6, key & 0x3F)));
      _origTiles.putIfAbsent(key, () => Uint8List.fromList(_tiles[key]!));
    }
  }

  static int _key(int bank, int tile) => bank * 64 + (tile & 0x3F);

  bool get is32Team => reader.is32Team;
  String get teamName => original.team;
  bool get hasOverlay => _boxes.isNotEmpty;
  Uint8List get paletteRam => original.paletteRam;
  List<int> get tilePalettes => List.unmodifiable(_pal);
  int get paletteGroup => _pal[0];

  /// Gives all four tiles palette [group] (0-3).
  void setPalette(int group) => _pal = List.filled(4, group);

  // ---- layer views -------------------------------------------------------

  /// The 16x16 background area: color indexes 0-3, 0 = clear (or an empty slot).
  Uint8List get bg {
    final out = Uint8List(smallBgSize * smallBgSize);
    for (var k = 0; k < 4; k++) {
      final key = _bgKeys[k];
      if (key == null) continue;
      final px = _tiles[key]!;
      for (var i = 0; i < 64; i++) {
        out[((k >> 1) * 8 + i ~/ 8) * smallBgSize + (k & 1) * 8 + i % 8] = px[i];
      }
    }
    return out;
  }

  /// The overlay sprites on the [smallHelmetWidth] x [smallHelmetHeight] canvas,
  /// as drawn: the first sprite with an opaque pixel owns it.
  Uint8List get overlay => _overlay().$1;

  /// Overlay pixels, and the palette (0-3) of the sprite that owns each one.
  (Uint8List, Uint8List) _overlay() {
    final out = Uint8List(smallHelmetWidth * smallHelmetHeight);
    final pal = Uint8List(out.length);
    for (final b in _boxes) {
      final px = _tiles[b.key]!;
      for (var y = b.y; y < b.y + 8; y++) {
        for (var x = b.x; x < b.x + 8; x++) {
          if (x < 0 || y < 0 || x >= smallHelmetWidth || y >= smallHelmetHeight) continue;
          final v = px[b.pixelAt(x, y)];
          final at = y * smallHelmetWidth + x;
          if (v != 0 && out[at] == 0) {
            out[at] = v;
            pal[at] = b.palette;
          }
        }
      }
    }
    return (out, pal);
  }

  /// NES color of each background pixel, 0xFF where clear.
  Uint8List bgNes() {
    final px = bg, out = Uint8List(px.length);
    for (var i = 0; i < px.length; i++) {
      final slot = ((i ~/ smallBgSize) >> 3) * 2 + ((i % smallBgSize) >> 3);
      out[i] = px[i] == 0 ? 0xFF : paletteRam[_pal[slot] * 4 + px[i]];
    }
    return out;
  }

  /// NES color of each overlay pixel, 0xFF where clear.
  Uint8List overlayNes() {
    final (px, pal) = _overlay();
    return Uint8List.fromList([
      for (var i = 0; i < px.length; i++) px[i] == 0 ? 0xFF : paletteRam[0x10 + pal[i] * 4 + px[i]]
    ]);
  }

  /// Sprite palette (0-3) a new overlay pixel at [x],[y] would use.
  int overlayPaletteAt(int x, int y) {
    for (final b in _boxes) {
      if (b.covers(x, y)) return b.palette;
    }
    return _boxes.isEmpty ? 0 : _boxes.first.palette;
  }

  /// Top-left corners of the overlay sprites on the canvas, first (front) first.
  List<(int, int)> get overlayBoxes => [for (final b in _boxes) (b.x, b.y)];

  /// Whether the background slot holding pixel [x],[y] of the 16x16 area has a tile to edit.
  bool bgEditable(int x, int y) =>
      x >= 0 && y >= 0 && x < smallBgSize && y < smallBgSize && _bgKeys[(y >> 3) * 2 + (x >> 3)] != null;

  bool overlayEditable(int x, int y) => _boxes.any((b) => b.covers(x, y));

  // ---- painting ----------------------------------------------------------

  /// Sets a background pixel; false if its slot has no tile.
  bool paintBg(int x, int y, int value) {
    if (!bgEditable(x, y)) return false;
    _tiles[_bgKeys[(y >> 3) * 2 + (x >> 3)]!]![(y & 7) * 8 + (x & 7)] = value;
    return true;
  }

  /// Sets an overlay pixel: into the front-most sprite that covers it, or, to
  /// clear it, in every sprite that covers it. False if no sprite covers it.
  bool paintOverlay(int x, int y, int value) {
    final covering = [for (final b in _boxes) if (b.covers(x, y)) b];
    if (covering.isEmpty) return false;
    if (value == 0) {
      for (final b in covering) {
        _tiles[b.key]![b.pixelAt(x, y)] = 0;
      }
    } else if (overlay[y * smallHelmetWidth + x] != value) {
      _tiles[covering.first.key]![covering.first.pixelAt(x, y)] = value;
    }
    return true;
  }

  // ---- whole pictures ----------------------------------------------------

  SmallHelmetPicture picture() => SmallHelmetPicture(teamName, bg, overlay, hasOverlay, tilePalettes);

  /// Writes a 16x16 picture into the background tiles; returns how many
  /// opaque pixels fell on empty slots.
  int applyBgPixels(Uint8List px) {
    var dropped = 0;
    for (var i = 0; i < px.length; i++) {
      if (!paintBg(i % smallBgSize, i ~/ smallBgSize, px[i]) && px[i] != 0) dropped++;
    }
    return dropped;
  }

  /// Writes a canvas-sized picture into the overlay sprites; returns how many
  /// opaque pixels no sprite covers.
  int applyOverlayPixels(Uint8List px) {
    var dropped = 0;
    final now = overlay;
    for (var i = 0; i < px.length; i++) {
      if (px[i] == now[i]) continue;
      if (!paintOverlay(i % smallHelmetWidth, i ~/ smallHelmetWidth, px[i]) && px[i] != 0) dropped++;
    }
    return dropped;
  }

  /// Applies a copied helmet. The overlay is only touched when the source has one.
  PictureResult applyPicture(SmallHelmetPicture p, {bool bg = true, bool overlay = true, bool palette = true}) {
    final r = PictureResult();
    if (palette) {
      _pal = is32Team ? List.of(p.tilePalettes) : List.filled(4, p.tilePalettes[0]);
    }
    if (bg) {
      r.droppedBg = applyBgPixels(p.bg);
      if (r.droppedBg > 0) {
        r.notes.add('${r.droppedBg} background pixel${r.droppedBg == 1 ? '' : 's'} had no tile to go in '
            "(this helmet has empty tile slots there), so they were left out.");
      }
    }
    if (overlay) {
      if (!p.hasOverlay) {
        if (hasOverlay) r.notes.add("${p.team} has no overlay sprites, so this helmet's overlay was left as it is.");
      } else if (!hasOverlay) {
        r.droppedOverlay = p.overlay.where((v) => v != 0).length;
        r.notes.add("This helmet has no overlay sprites, so ${p.team}'s overlay (${r.droppedOverlay} pixels) was left out.");
      } else {
        r.droppedOverlay = applyOverlayPixels(p.overlay);
        if (r.droppedOverlay > 0) {
          r.notes.add("${r.droppedOverlay} overlay pixel${r.droppedOverlay == 1 ? '' : 's'} fell outside this helmet's "
              'sprites, so they were left out.');
        }
      }
    }
    return r;
  }

  // ---- undo --------------------------------------------------------------

  /// The editable state (tile pixels and palettes), for [restore].
  (Map<int, Uint8List>, List<int>) snapshot() =>
      ({for (final e in _tiles.entries) e.key: Uint8List.fromList(e.value)}, List.of(_pal));

  void restore((Map<int, Uint8List>, List<int>) s) {
    for (final e in s.$1.entries) {
      _tiles[e.key]!.setAll(0, e.value);
    }
    _pal = List.of(s.$2);
  }

  /// Back to what the ROM has.
  void revert() => restore((_origTiles, original.tilePalettes));

  // ---- result ------------------------------------------------------------

  Set<int> get _changedKeys => {
        for (final e in _tiles.entries)
          if (!_same(e.value, _origTiles[e.key]!)) e.key
      };

  bool get _paletteChanged {
    for (var k = 0; k < 4; k++) {
      if (_pal[k] != original.tilePalettes[k]) return true;
    }
    return false;
  }

  bool get changed => _changedKeys.isNotEmpty || _paletteChanged;

  /// Other teams whose helmet draws a tile this edit changed.
  List<String> get alsoAffects {
    final keys = _changedKeys;
    if (keys.isEmpty) return const [];
    final users = _users ??= _tileUsers();
    final out = <int>{};
    for (final k in keys) {
      out.addAll(users[k] ?? const {});
    }
    out.remove(team);
    return [for (final t in out.toList()..sort()) reader.teams[t]];
  }

  Map<int, Set<int>> _tileUsers() {
    final users = <int, Set<int>>{};
    for (var t = 0; t < reader.teams.length; t++) {
      final h = reader.read(t);
      for (final tile in h.tiles) {
        if (tile != 0) users.putIfAbsent(_key(SmallHelmetReader.bgBank(tile), tile), () => {}).add(t);
      }
      for (final s in h.sprites) {
        users.putIfAbsent(_key(SmallHelmetReader.spriteBank(s.tile), s.tile), () => {}).add(t);
      }
    }
    return users;
  }

  /// A copy of the ROM with the edit applied.
  Uint8List build() {
    final out = Uint8List.fromList(rom.bytes);
    for (final key in _changedKeys) {
      final at = 16 + rom.prgSize + (key >> 6) * 0x400 + (key & 0x3F) * 16;
      final px = _tiles[key]!;
      for (var y = 0; y < 8; y++) {
        var lo = 0, hi = 0;
        for (var x = 0; x < 8; x++) {
          lo |= (px[y * 8 + x] & 1) << (7 - x);
          hi |= ((px[y * 8 + x] >> 1) & 1) << (7 - x);
        }
        out[at + y] = lo;
        out[at + 8 + y] = hi;
      }
    }
    if (_paletteChanged) {
      if (is32Team) {
        final slots = reader.attrSlots32(team);
        for (var i = 0; i < slots.length; i++) {
          final (off, shift) = slots[i];
          out[16 + off] = (out[16 + off] & ~(3 << shift)) | (_pal[i % 4] << shift);
        }
      } else {
        final at = 16 + reader.flagsOffset(team);
        out[at] = (out[at] & ~3) | _pal[0];
      }
    }
    return out;
  }

  static bool _same(Uint8List a, Uint8List b) {
    for (var i = 0; i < a.length; i++) {
      if (a[i] != b[i]) return false;
    }
    return true;
  }
}
