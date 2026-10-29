import 'dart:typed_data';

import 'nes_rom.dart';
import 'small_helmet.dart';

/// Canvas around one small helmet: overlay sprites reach 4px left of it and
/// a line below it.
const smallHelmetMarginX = 4;
const smallHelmetWidth = 24, smallHelmetHeight = 18;

/// Draws [h] with its top-left at screen [x],[y] into [canvas] (NES color
/// indexes, [width] wide), the way the PPU would: background tiles, then
/// sprites (lowest OAM slot wins; "behind" sprites only show over color 0).
void drawSmallHelmet(NesRom rom, SmallHelmet h, Uint8List canvas, int width, int height, int x, int y) {
  final pal = h.paletteRam;
  final bgOpaque = <int>{};

  void plot(int px, int py, int color) {
    if (px >= 0 && px < width && py >= 0 && py < height) canvas[py * width + px] = color & 0x3F;
  }

  for (var k = 0; k < 4; k++) {
    final t = h.tiles[k];
    final px = rom.tile(SmallHelmetReader.bgBank(t), t);
    for (var i = 0; i < 64; i++) {
      if (px[i] == 0) continue;
      final sx = x + (k & 1) * 8 + i % 8, sy = y + 1 + (k >> 1) * 8 + i ~/ 8;
      plot(sx, sy, pal[h.tilePalettes[k] * 4 + px[i]]);
      bgOpaque.add(sy * width + sx);
    }
  }

  // The line-by-line sprite order: first sprite in the list owns a pixel.
  final claimed = <int>{};
  for (final s in h.sprites) {
    final px = rom.tile(SmallHelmetReader.spriteBank(s.tile), s.tile);
    for (var i = 0; i < 64; i++) {
      final cx = s.flipX ? 7 - i % 8 : i % 8, cy = s.flipY ? 7 - i ~/ 8 : i ~/ 8;
      if (px[i] == 0) continue;
      final sx = x + s.dx + cx, sy = y + s.dy + 1 + cy;
      if (sx < 0 || sx >= width || sy < 0 || sy >= height) continue;
      final at = sy * width + sx;
      if (!claimed.add(at)) continue;
      if (s.behind && bgOpaque.contains(at)) continue;
      plot(sx, sy, pal[0x10 + s.palette * 4 + px[i]]);
    }
  }
}

/// One helmet on its own backdrop, [smallHelmetWidth] x [smallHelmetHeight];
/// the helmet's top-left is at ([smallHelmetMarginX], 0).
Uint8List renderSmallHelmetIndexes(NesRom rom, SmallHelmet h) {
  final out = Uint8List(smallHelmetWidth * smallHelmetHeight)..fillRange(0, smallHelmetWidth * smallHelmetHeight, h.paletteRam[0]);
  drawSmallHelmet(rom, h, out, smallHelmetWidth, smallHelmetHeight, smallHelmetMarginX, 0);
  return out;
}
