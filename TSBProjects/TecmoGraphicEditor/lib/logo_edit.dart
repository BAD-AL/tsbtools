// Edit a team's large-helmet logo as pixels. The picture is cut back into
// 8x8 sprites and written with the paste engine, so the same tile-fitting and
// "don't touch other teams" rules apply.
import 'dart:typed_data';

import 'helmet_paste.dart';
import 'helmet_renderer.dart';
import 'large_helmet.dart';

/// The game copies 32 helmet sprites to OAM ($A92B / $A970); the facemask uses some.
const helmetSpriteSlots = 32;

/// The NES shows at most 8 sprites on a scanline.
const spritesPerLine = 8;

const _size = helmetSize;

/// The team's logo in the helmet view (72x72, row-major): 0 = transparent,
/// 1-3 = logo colors 1-3.
Uint8List logoPixels(LargeHelmetReader r, int team) {
  final out = Uint8List(_size * _size);
  for (final s in r.read(team).sprites.reversed) {
    if (s.palette != 3) continue;
    final px = _visible(r, s);
    for (var k = 0; k < 64; k++) {
      final x = s.x + k % 8 - helmetLeft, y = s.y + k ~/ 8 - helmetTop;
      if (px[k] != 0 && x >= 0 && x < _size && y >= 0 && y < _size) out[y * _size + x] = px[k];
    }
  }
  return out;
}

/// A sprite's 64 pixels as they appear on screen (flip applied).
Uint8List _visible(LargeHelmetReader r, HelmetSprite s) {
  final px = r.rom.tile(s.bank, s.tile);
  return Uint8List.fromList([for (var k = 0; k < 64; k++) px[s.flipX ? k - k % 8 + 7 - k % 8 : k]]);
}

class LogoStats {
  final int sprites, maxSprites, busiestLine, tiles;
  /// Top-left of each sprite, in helmet-view pixels.
  final List<(int, int)> boxes;
  LogoStats(this.sprites, this.maxSprites, this.busiestLine, this.tiles, this.boxes);
}

/// What applying [pixels] would use, for the editor's status line.
LogoStats logoStats(LargeHelmetReader r, int team, Uint8List pixels) => checkLogoEdit(r, team, pixels).stats;

/// Works out (and applies, on a copy of the ROM) replacing [team]'s logo with [pixels].
PastePlan planLogoEdit(LargeHelmetReader r, int team, Uint8List pixels) => checkLogoEdit(r, team, pixels).plan;

class LogoCheck {
  /// The layout that would be written (or, if nothing fits, the preferred one).
  final LogoStats stats;
  final PastePlan plan;

  /// The hand-placed sprites don't fit, but letting the editor arrange them would.
  final bool autoArrangeFits;
  LogoCheck(this.stats, this.plan, this.autoArrangeFits);
}

/// Plans replacing [team]'s logo with [pixels]. [boxes] are sprite top-lefts in
/// the helmet view, front first, and are kept as placed (new sprites are added
/// only for pixels outside them). Without [boxes] the editor arranges the
/// sprites, preferring the logo's original positions.
LogoCheck checkLogoEdit(LargeHelmetReader r, int team, Uint8List pixels, {List<(int, int)>? boxes}) {
  if (pixels.length != _size * _size) throw ArgumentError('Expected ${_size * _size} pixels');
  final mask = _mask(r, team);
  PastePlan plan(List<_Sprite> layout) {
    final errors = _limitErrors(layout, mask);
    return errors.isEmpty
        ? planPaste(r, team, _toCopy(r, team, layout), colors: false, shell: false)
        : PastePlan(errors, [], [], false, null);
  }

  LogoCheck result(List<_Sprite> s, PastePlan p, bool autoFits) => LogoCheck(
      LogoStats(s.length, helmetSpriteSlots - mask.length, _busiestLine(s, mask).$2, {for (final x in s) x.px.join()}.length,
          [for (final x in s) (x.x - helmetLeft, x.y - helmetTop)]),
      p,
      autoFits);

  if (boxes != null) {
    final layout = _ownLayout(r, team, pixels, [for (final (x, y) in boxes) (x + helmetLeft, y + helmetTop)])!;
    final p = plan(layout);
    return result(layout, p, !p.ok && checkLogoEdit(r, team, pixels).plan.ok);
  }
  final layouts = _layouts(r, team, pixels);
  (List<_Sprite>, PastePlan)? failed;
  for (final layout in layouts) {
    final p = plan(layout);
    if (p.ok) return result(layout, p, false);
    failed ??= (layout, p); // report why the preferred layout failed
  }
  return result(failed!.$1, failed.$2, false);
}

/// Drags sprite [index] of [boxes] (helmet-view top-lefts, front first) by
/// ([dx], [dy]). The pixels it shows go with it, and it moves to the front.
/// It stays inside the helmet view.
({Uint8List pixels, List<(int, int)> boxes}) moveLogoSprite(
    Uint8List pixels, List<(int, int)> boxes, int index, int dx, int dy) {
  final (bx, by) = boxes[index];
  final nx = (bx + dx).clamp(bx < 0 ? bx : 0, bx > _size - 8 ? bx : _size - 8);
  final ny = (by + dy).clamp(by < 0 ? by : 0, by > _size - 8 ? by : _size - 8);
  bool inFront(int x, int y) =>
      boxes.take(index).any((b) => x >= b.$1 && x < b.$1 + 8 && y >= b.$2 && y < b.$2 + 8);
  bool inView(int x, int y) => x >= 0 && x < _size && y >= 0 && y < _size;

  final out = Uint8List.fromList(pixels);
  final lifted = Uint8List(64);
  for (var k = 0; k < 64; k++) {
    final x = bx + k % 8, y = by + k ~/ 8;
    if (!inView(x, y) || inFront(x, y)) continue;
    lifted[k] = pixels[y * _size + x];
    out[y * _size + x] = 0;
  }
  for (var k = 0; k < 64; k++) {
    final x = nx + k % 8, y = ny + k ~/ 8;
    if (lifted[k] != 0 && inView(x, y)) out[y * _size + x] = lifted[k];
  }
  return (pixels: out, boxes: [(nx, ny), for (var i = 0; i < boxes.length; i++) if (i != index) boxes[i]]);
}

/// A sprite in screen coordinates with its 64 pixel values.
class _Sprite {
  final int x, y;
  final Uint8List px;
  _Sprite(this.x, this.y, this.px);
}

List<HelmetSprite> _mask(LargeHelmetReader r, int team) => [for (final s in r.read(team).sprites) if (s.palette == 2) s];

/// Candidate sprite layouts, best first: the logo's own sprite positions (so
/// unchanged tiles can be reused), then a fresh layout.
List<List<_Sprite>> _layouts(LargeHelmetReader r, int team, Uint8List pixels) {
  final own = _ownLayout(r, team, pixels, [for (final s in _logo(r, team)) (s.x, s.y)]);
  return [if (own != null) own, _gridLayout(pixels)];
}

List<HelmetSprite> _logo(LargeHelmetReader r, int team) => [for (final s in r.read(team).sprites) if (s.palette == 3) s];

int _at(Uint8List pixels, int sx, int sy) {
  final x = sx - helmetLeft, y = sy - helmetTop;
  return x >= 0 && x < _size && y >= 0 && y < _size ? pixels[y * _size + x] : 0;
}

/// Keeps sprites at [boxes] (screen top-lefts, front first) and adds new
/// sprites only for pixels outside them; null if that can't reproduce the drawing.
List<_Sprite>? _ownLayout(LargeHelmetReader r, int team, Uint8List pixels, List<(int, int)> boxes) {
  // A box still where an original sprite was can keep that sprite's tile.
  final unused = _logo(r, team);
  final was = [
    for (final (x, y) in boxes)
      () {
        final i = unused.indexWhere((s) => s.x == x && s.y == y);
        return i < 0 ? null : unused.removeAt(i);
      }()
  ];
  bool covered(int sx, int sy) => boxes.any((b) => sx >= b.$1 && sx < b.$1 + 8 && sy >= b.$2 && sy < b.$2 + 8);
  final outside = Uint8List(pixels.length);
  for (var k = 0; k < pixels.length; k++) {
    if (pixels[k] != 0 && !covered(helmetLeft + k % _size, helmetTop + k ~/ _size)) outside[k] = pixels[k];
  }
  final extra = outside.any((v) => v != 0) ? _gridLayout(outside) : <_Sprite>[];

  // Sprites whose area is unchanged keep their original tile (so it can be
  // reused); the rest take, in priority order, the pixels nothing above shows.
  final before = logoPixels(r, team);
  List<Uint8List> cut({required bool keep}) {
    final shown = <int>{};
    return [
      for (var i = 0; i < boxes.length; i++)
        () {
          final (bx, by) = boxes[i];
          final cells = List.generate(64, (k) => (bx + k % 8, by + k ~/ 8));
          final s = was[i];
          final tile = keep && s != null && cells.every((p) => _at(before, p.$1, p.$2) == _at(pixels, p.$1, p.$2))
              ? _visible(r, s)
              : Uint8List.fromList([
                  for (final p in cells) shown.contains(p.$2 * 256 + p.$1) ? 0 : _at(pixels, p.$1, p.$2)
                ]);
          for (var k = 0; k < 64; k++) {
            if (tile[k] != 0) shown.add(cells[k].$2 * 256 + cells[k].$1);
          }
          return tile;
        }()
    ];
  }

  for (final tiles in [cut(keep: true), cut(keep: false)]) {
    final layout = [
      for (var i = 0; i < boxes.length; i++)
        if (tiles[i].any((v) => v != 0)) _Sprite(boxes[i].$1, boxes[i].$2, Uint8List.fromList(tiles[i])),
      ...extra,
    ];
    if (_same(_draw(layout), pixels)) return layout;
  }
  return null;
}

/// Columns of 8 pixels; in each column, tiles start at the first row still
/// uncovered. Tries all 8 column offsets and keeps the one with fewest sprites.
List<_Sprite> _gridLayout(Uint8List pixels) {
  List<_Sprite>? best;
  for (var o = 0; o < 8; o++) {
    final layout = <_Sprite>[];
    for (var c = o - 8; c < _size; c += 8) {
      bool rowUsed(int y) {
        for (var x = c; x < c + 8; x++) {
          if (x >= 0 && x < _size && pixels[y * _size + x] != 0) return true;
        }
        return false;
      }

      for (var y = 0; y < _size;) {
        if (!rowUsed(y)) {
          y++;
          continue;
        }
        final sx = helmetLeft + c, sy = helmetTop + y;
        layout.add(_Sprite(sx, sy, Uint8List.fromList([for (var k = 0; k < 64; k++) _at(pixels, sx + k % 8, sy + k ~/ 8)])));
        y += 8;
      }
    }
    if (best == null || layout.length < best.length) best = layout;
  }
  return best!;
}

Uint8List _draw(List<_Sprite> layout) {
  final out = Uint8List(_size * _size);
  for (final s in layout.reversed) {
    for (var k = 0; k < 64; k++) {
      final x = s.x + k % 8 - helmetLeft, y = s.y + k ~/ 8 - helmetTop;
      if (s.px[k] != 0 && x >= 0 && x < _size && y >= 0 && y < _size) out[y * _size + x] = s.px[k];
    }
  }
  return out;
}

bool _same(List<int> a, List<int> b) {
  for (var i = 0; i < a.length; i++) {
    if (a[i] != b[i]) return false;
  }
  return a.length == b.length;
}

/// (screen line, sprite count) of the busiest line, logo and facemask together.
(int, int) _busiestLine(List<_Sprite> layout, List<HelmetSprite> mask) {
  final count = List.filled(256 + 8, 0);
  for (final y in [...layout.map((s) => s.y), ...mask.map((s) => s.y)]) {
    for (var k = 0; k < 8; k++) {
      count[y + k]++;
    }
  }
  var line = 0;
  for (var y = 0; y < count.length; y++) {
    if (count[y] > count[line]) line = y;
  }
  return (line, count[line]);
}

List<String> _limitErrors(List<_Sprite> layout, List<HelmetSprite> mask) {
  final errors = <String>[];
  final room = helmetSpriteSlots - mask.length;
  if (layout.length > room) {
    errors.add('Logo needs ${layout.length} sprites but the helmet has room for $room '
        '(the facemask uses ${mask.length} of $helmetSpriteSlots).');
  }
  final (line, n) = _busiestLine(layout, mask);
  if (n > spritesPerLine) {
    errors.add('Row ${line - helmetTop} would need $n sprites side by side (logo + facemask); '
        'the NES shows only $spritesPerLine per line. Make the logo narrower there.');
  }
  return errors;
}

/// Encodes [layout] as a logo stream (see LargeHelmetReader._readSpriteData).
HelmetCopy _toCopy(LargeHelmetReader r, int team, List<_Sprite> layout) {
  const baseX = 64, baseY = 64;
  final keys = <String, int>{};
  final tiles = <int, Uint8List>{};
  final placed = [
    for (final s in layout)
      () {
        final dx = s.x - 2 * baseX + 60; // 8*col + fineX
        final dy = s.y - 2 * baseY + 59; // 8*row + fineY
        final key = keys.putIfAbsent(s.px.join(), () {
          tiles[keys.length] = _chr(s.px);
          return keys.length;
        });
        return (col: dx >> 3, fineX: dx & 7, row: dy >> 3, fineY: dy & 7, key: key);
      }()
  ]; // kept in layout order: earlier sprites win where they overlap

  final out = <int>[];
  (int, int)? run;
  var fineX = 0, fineY = 0;
  for (final p in placed) {
    if (run != (p.col, p.fineX)) {
      // fc 3n also nudges the current column, so set it before starting one.
      if (p.fineX != fineX) out.addAll([0xFC, 0x30 | (fineX = p.fineX)]);
      out.add(0x80 | (p.col & 0x1F));
      run = (p.col, p.fineX);
    }
    if (p.fineY != fineY) out.addAll([0xFC, 0x20 | (fineY = p.fineY)]);
    out.addAll([(p.row & 0x0F) << 2 | 1, p.key]); // palette bits 1 (+2 on this screen = 3)
  }
  out.add(0xFF);

  final at = r.paletteOffset(team);
  return HelmetCopy(r.teams[team], r.is32Team, Uint8List.fromList([for (var k = 0; k < 8; k++) r.rom.prg(at + k)]),
      Uint8List(0), baseY, baseX, Uint8List.fromList(out), tiles);
}

/// 64 pixel values -> 16 bytes of 2bpp CHR.
Uint8List _chr(Uint8List px) {
  final out = Uint8List(16);
  for (var k = 0; k < 64; k++) {
    final bit = 0x80 >> (k % 8);
    if (px[k] & 1 != 0) out[k ~/ 8] |= bit;
    if (px[k] & 2 != 0) out[8 + k ~/ 8] |= bit;
  }
  return out;
}
