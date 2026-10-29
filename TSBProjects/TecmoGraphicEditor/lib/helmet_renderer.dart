import 'dart:typed_data';

import 'large_helmet.dart';
import 'nes_palette.dart';
import 'nes_rom.dart';

/// Screen area the helmet occupies on the team data screen.
const helmetLeft = 32, helmetTop = 32, helmetSize = 72;

/// Renders [h] the way the PPU would, as NES color indexes (0-63), for the
/// screen rectangle at [left],[top] of [width]x[height].
Uint8List renderHelmetIndexes(NesRom rom, LargeHelmet h,
    {int left = helmetLeft,
    int top = helmetTop,
    int width = helmetSize,
    int height = helmetSize,
    bool shell = true,
    bool logo = true,
    bool facemask = true}) {
  final pal = h.paletteRam;
  final out = Uint8List(width * height)..fillRange(0, width * height, pal[0]);

  void plot(int sx, int sy, int color) {
    final x = sx - left, y = sy - top;
    if (x >= 0 && x < width && y >= 0 && y < height) out[y * width + x] = color & 0x3F;
  }

  // Shell: background tiles at screen tile (4,4). The screen's vertical
  // scroll is 255, so the background shows one line lower than its grid.
  for (var r = 0; r < (shell ? 8 : 0); r++) {
    for (var c = 0; c < 8; c++) {
      final t = h.shellTiles[r * 8 + c];
      final px = rom.tile(h.bgBanks[t >> 6], t);
      final p = h.shellPalettes[(r >> 1) * 4 + (c >> 1)];
      for (var k = 0; k < 64; k++) {
        if (px[k] != 0) plot(32 + c * 8 + k % 8, 33 + r * 8 + k ~/ 8, pal[p * 4 + px[k]]);
      }
    }
  }

  // Sprites: the first one in the list has priority, so draw back to front.
  for (final s in h.sprites.reversed) {
    if (!(s.palette == 3 ? logo : facemask)) continue;
    final px = rom.tile(s.bank, s.tile);
    for (var k = 0; k < 64; k++) {
      if (px[k] != 0) plot(s.x + (s.flipX ? 7 - k % 8 : k % 8), s.y + k ~/ 8, pal[0x10 + s.palette * 4 + px[k]]);
    }
  }
  return out;
}

/// NES color indexes to RGBA bytes (for canvas ImageData).
Uint8List indexesToRgba(Uint8List indexes) {
  final out = Uint8List(indexes.length * 4);
  for (var i = 0; i < indexes.length; i++) {
    final c = nesPalette[indexes[i]];
    out[i * 4] = c >> 16;
    out[i * 4 + 1] = (c >> 8) & 0xFF;
    out[i * 4 + 2] = c & 0xFF;
    out[i * 4 + 3] = 0xFF;
  }
  return out;
}
