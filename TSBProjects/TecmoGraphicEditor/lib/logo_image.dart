// Logo pictures for Export / Import PNG: the 72x72 logo, a transparent gap,
// then a strip of three 24-pixel blocks in logo colors 1-3. On import the
// strip says which color each pixel means, and a repainted block recolors
// the logo.
import 'dart:typed_data';

import 'helmet_renderer.dart';
import 'nes_palette.dart';

const logoStripGap = 2;
const logoSwatchHeight = 10;
const logoImageHeight = helmetSize + logoStripGap + logoSwatchHeight;
const _block = helmetSize ~/ 3;

/// [px] (helmet-view logo pixels, 0 = clear) in NES [colors] 1-3, with the
/// color strip: helmetSize x logoImageHeight RGBA.
Uint8List logoImageRgba(Uint8List px, List<int> colors) {
  final out = Uint8List(helmetSize * logoImageHeight * 4);
  void set(int k, int nes) {
    final c = nesPalette[nes & 0x3F];
    out
      ..[k * 4] = c >> 16
      ..[k * 4 + 1] = (c >> 8) & 0xFF
      ..[k * 4 + 2] = c & 0xFF
      ..[k * 4 + 3] = 0xFF;
  }

  for (var k = 0; k < px.length; k++) {
    if (px[k] != 0) set(k, colors[px[k] - 1]);
  }
  for (var y = helmetSize + logoStripGap; y < logoImageHeight; y++) {
    for (var x = 0; x < helmetSize; x++) {
      set(y * helmetSize + x, colors[x ~/ _block]);
    }
  }
  return out;
}

class LogoImport {
  final Uint8List pixels;
  final List<int> colors;
  final List<String> notes;
  LogoImport(this.pixels, this.colors, this.notes);
}

/// Reads a helmetSize-wide RGBA picture, [height] helmetSize (logo only) or
/// logoImageHeight (with the color strip). [colors] are the current NES logo
/// colors 1-3.
LogoImport readLogoImage(Uint8List rgba, int height, List<int> colors) {
  int rgb(int k) => rgba[k * 4] << 16 | rgba[k * 4 + 1] << 8 | rgba[k * 4 + 2];
  bool opaque(int k) => rgba[k * 4 + 3] >= 128;
  final notes = <String>[];
  final newColors = List.of(colors);
  var targets = [for (final c in colors) nesPalette[c & 0x3F]];

  if (height == logoImageHeight) {
    // The most common opaque color in each block (null if the block was erased).
    final strip = [
      for (var i = 0; i < 3; i++)
        () {
          final count = <int, int>{};
          for (var y = helmetSize + logoStripGap; y < logoImageHeight; y++) {
            for (var x = _block * i; x < _block * (i + 1); x++) {
              final k = y * helmetSize + x;
              if (opaque(k)) count.update(rgb(k), (n) => n + 1, ifAbsent: () => 1);
            }
          }
          if (count.isEmpty) return null;
          return count.entries.reduce((a, b) => b.value > a.value ? b : a).key;
        }()
    ];
    final used = strip.whereType<int>().toList();
    if (used.toSet().length < used.length) {
      notes.add('Two blocks in the color strip are the same color, so the strip was ignored.');
    } else {
      for (var i = 0; i < 3; i++) {
        final c = strip[i];
        if (c == null || c == targets[i]) continue;
        targets[i] = c;
        final nes = nearestNesColor(c);
        if (nes == colors[i]) continue;
        newColors[i] = nes;
        notes.add('Color ${i + 1} changed from ${_hex(colors[i])} to ${_hex(nes)} from the color strip'
            '${nesPalette[nes] == c ? '' : ' (the closest NES color to #${c.toRadixString(16).padLeft(6, '0').toUpperCase()})'}.');
      }
    }
  }

  // No transparency (e.g. saved from Paint): treat the corner color as the background.
  var hasAlpha = false;
  for (var k = 0; k < helmetSize * helmetSize; k++) {
    if (!opaque(k)) hasAlpha = true;
  }
  final bg = hasAlpha ? null : rgb(0);
  if (bg != null) notes.add('The picture has no transparency, so its corner color was used as the background.');

  final px = Uint8List(helmetSize * helmetSize);
  for (var k = 0; k < px.length; k++) {
    if (!opaque(k) || rgb(k) == bg) continue;
    var best = 0, bestD = 1 << 30; // (not 1 << 62: shifts wrap at 32 bits on the web)
    for (var i = 0; i < 3; i++) {
      final d = _distance(rgb(k), targets[i]);
      if (d < bestD) {
        bestD = d;
        best = i + 1;
      }
    }
    px[k] = best;
  }
  return LogoImport(px, newColors, notes);
}

/// The NES color closest to [rgb] (0xRRGGBB). Black is \$0F; \$0D is never used.
int nearestNesColor(int rgb) {
  var best = 0x0F, bestD = _distance(rgb, nesPalette[0x0F]);
  for (var c = 0; c < 64; c++) {
    if (c == 0x0D) continue;
    final d = _distance(rgb, nesPalette[c]);
    if (d < bestD) {
      bestD = d;
      best = c;
    }
  }
  return best;
}

int _distance(int a, int b) {
  final dr = (a >> 16) - (b >> 16), dg = ((a >> 8) & 0xFF) - ((b >> 8) & 0xFF), db = (a & 0xFF) - (b & 0xFF);
  return dr * dr + dg * dg + db * db;
}

String _hex(int c) => '\$${c.toRadixString(16).padLeft(2, '0').toUpperCase()}';
