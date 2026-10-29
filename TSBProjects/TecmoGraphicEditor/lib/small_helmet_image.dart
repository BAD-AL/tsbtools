// Small-helmet pictures for Export / Import PNG: the 16x16 background area
// (centered over the overlay canvas width), a transparent gap, then the overlay
// sprites on their 24x18 canvas. Colors are the helmet's own palette colors.
import 'dart:typed_data';

import 'nes_palette.dart';
import 'small_helmet_edit.dart';
import 'small_helmet_renderer.dart';

const smallImageWidth = smallHelmetWidth;
const smallImageGap = 2;
const smallImageOverlayTop = smallBgSize + smallImageGap;
const smallImageHeight = smallImageOverlayTop + smallHelmetHeight;

/// RGBA picture of [e]'s current layers, [smallImageWidth] x [smallImageHeight].
Uint8List smallHelmetImageRgba(SmallHelmetEditor e) {
  final out = Uint8List(smallImageWidth * smallImageHeight * 4);
  void set(int x, int y, int nes) {
    if (nes == 0xFF) return;
    final c = nesPalette[nes & 0x3F], k = (y * smallImageWidth + x) * 4;
    out
      ..[k] = c >> 16
      ..[k + 1] = (c >> 8) & 0xFF
      ..[k + 2] = c & 0xFF
      ..[k + 3] = 0xFF;
  }

  final bg = e.bgNes(), overlay = e.overlayNes();
  for (var i = 0; i < bg.length; i++) {
    set(smallHelmetMarginX + i % smallBgSize, i ~/ smallBgSize, bg[i]);
  }
  for (var i = 0; i < overlay.length; i++) {
    set(i % smallHelmetWidth, smallImageOverlayTop + i ~/ smallHelmetWidth, overlay[i]);
  }
  return out;
}

class SmallImport {
  final Uint8List bg; // 16x16 color indexes 0-3
  final Uint8List overlay; // overlay canvas
  final List<String> notes;
  SmallImport(this.bg, this.overlay, this.notes);
}

/// Reads a [smallImageWidth] x [smallImageHeight] picture: each opaque pixel
/// becomes the closest of the 3 colors of the palette it will be drawn with.
SmallImport readSmallHelmetImage(Uint8List rgba, SmallHelmetEditor e) {
  int rgb(int x, int y) {
    final k = (y * smallImageWidth + x) * 4;
    return rgba[k] << 16 | rgba[k + 1] << 8 | rgba[k + 2];
  }

  bool opaque(int x, int y) => rgba[(y * smallImageWidth + x) * 4 + 3] >= 128;
  final notes = <String>[];

  var hasAlpha = false;
  for (var k = 3; k < rgba.length; k += 4) {
    if (rgba[k] < 128) hasAlpha = true;
  }
  final backdrop = hasAlpha ? null : rgb(0, 0);
  if (backdrop != null) notes.add('The picture has no transparency, so its corner color was used as the background.');

  // Palettes can repeat a color, so a pixel whose color already matches what
  // is there keeps its current index.
  int nearest(int color, int paletteBase, int current) {
    if (current != 0 && nesPalette[e.paletteRam[paletteBase + current] & 0x3F] == color) return current;
    var best = 1, bestD = 1 << 30;
    for (var i = 1; i < 4; i++) {
      final d = _distance(color, nesPalette[e.paletteRam[paletteBase + i] & 0x3F]);
      if (d < bestD) {
        bestD = d;
        best = i;
      }
    }
    return best;
  }

  bool clear(int x, int y) => !opaque(x, y) || rgb(x, y) == backdrop;

  final curBg = e.bg, curOverlay = e.overlay;
  final bg = Uint8List(smallBgSize * smallBgSize);
  for (var y = 0; y < smallBgSize; y++) {
    for (var x = 0; x < smallBgSize; x++) {
      final px = x + smallHelmetMarginX;
      if (clear(px, y)) continue;
      final slot = (y >> 3) * 2 + (x >> 3);
      bg[y * smallBgSize + x] = nearest(rgb(px, y), e.tilePalettes[slot] * 4, curBg[y * smallBgSize + x]);
    }
  }
  final overlay = Uint8List(smallHelmetWidth * smallHelmetHeight);
  for (var y = 0; y < smallHelmetHeight; y++) {
    for (var x = 0; x < smallHelmetWidth; x++) {
      if (clear(x, smallImageOverlayTop + y)) continue;
      overlay[y * smallHelmetWidth + x] =
          nearest(rgb(x, smallImageOverlayTop + y), 0x10 + e.overlayPaletteAt(x, y) * 4, curOverlay[y * smallHelmetWidth + x]);
    }
  }
  var stray = 0;
  for (var y = 0; y < smallBgSize; y++) {
    for (var x = 0; x < smallImageWidth; x++) {
      if ((x < smallHelmetMarginX || x >= smallHelmetMarginX + smallBgSize) && !clear(x, y)) stray++;
    }
  }
  for (var y = smallBgSize; y < smallImageOverlayTop; y++) {
    for (var x = 0; x < smallImageWidth; x++) {
      if (!clear(x, y)) stray++;
    }
  }
  if (stray > 0) notes.add('$stray pixel${stray == 1 ? '' : 's'} outside the two drawing areas were ignored.');
  return SmallImport(bg, overlay, notes);
}

int _distance(int a, int b) {
  final dr = (a >> 16) - (b >> 16), dg = ((a >> 8) & 0xFF) - ((b >> 8) & 0xFF), db = (a & 0xFF) - (b & 0xFF);
  return dr * dr + dg * dg + db * db;
}
