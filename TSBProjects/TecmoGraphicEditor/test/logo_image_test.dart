import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/helmet_colors.dart';
import 'package:tecmo_graphic_editor/helmet_renderer.dart';
import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/logo_edit.dart';
import 'package:tecmo_graphic_editor/logo_image.dart';
import 'package:tecmo_graphic_editor/nes_palette.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:test/test.dart';

void main() {
  final r = LargeHelmetReader(NesRom(File('ROMS/TSB 2007-32-105.nes').readAsBytesSync()));
  final t = r.teams.indexOf('bills');
  final px = logoPixels(r, t);
  final colors = helmetColors(r, t).sublist(5);

  void setRgb(Uint8List rgba, int k, int rgb) => rgba
    ..[k * 4] = rgb >> 16
    ..[k * 4 + 1] = (rgb >> 8) & 0xFF
    ..[k * 4 + 2] = rgb & 0xFF
    ..[k * 4 + 3] = 255;
  void paintBlock(Uint8List rgba, int i, int rgb) {
    for (var y = helmetSize + logoStripGap; y < logoImageHeight; y++) {
      for (var x = 24 * i; x < 24 * i + 24; x++) {
        setRgb(rgba, y * helmetSize + x, rgb);
      }
    }
  }

  test('the exported picture has the logo on top and a color strip below', () {
    final rgba = logoImageRgba(px, colors);
    expect(rgba.length, helmetSize * logoImageHeight * 4);
    for (var i = 0; i < 3; i++) {
      final k = (logoImageHeight - 1) * helmetSize + 24 * i + 5;
      expect(rgba[k * 4] << 16 | rgba[k * 4 + 1] << 8 | rgba[k * 4 + 2], nesPalette[colors[i]]);
    }
    expect(rgba[(helmetSize * helmetSize) * 4 + 3], 0, reason: 'the gap row is transparent');
  });

  test('exporting and importing gives the same logo and colors', () {
    final back = readLogoImage(logoImageRgba(px, colors), logoImageHeight, colors);
    expect(back.pixels, px);
    expect(back.colors, colors);
    expect(back.notes, isEmpty);
  });

  test('a repainted strip block changes that logo color', () {
    final rgba = logoImageRgba(px, colors);
    for (var k = 0; k < px.length; k++) {
      if (px[k] == 2) setRgb(rgba, k, nesPalette[0x2A]);
    }
    paintBlock(rgba, 1, nesPalette[0x2A]);
    final back = readLogoImage(rgba, logoImageHeight, colors);
    expect(back.colors, [colors[0], 0x2A, colors[2]]);
    expect(back.pixels, px);
    expect(back.notes.join(), contains('Color 2'));
  });

  test('a strip color that is not an NES color snaps to the closest one', () {
    final rgba = logoImageRgba(px, colors);
    paintBlock(rgba, 0, 0x10C010);
    final back = readLogoImage(rgba, logoImageHeight, colors);
    expect(back.colors[0], nearestNesColor(0x10C010));
    expect(back.notes.join(), contains('closest'));
  });

  test('two strip blocks of the same color are ignored', () {
    final rgba = logoImageRgba(px, colors);
    paintBlock(rgba, 2, nesPalette[colors[1]]);
    final back = readLogoImage(rgba, logoImageHeight, colors);
    expect(back.colors, colors);
    expect(back.pixels, px);
    expect(back.notes.join(), contains('same color'));
  });

  test('a picture without a strip keeps the current colors', () {
    final rgba = logoImageRgba(px, colors).sublist(0, helmetSize * helmetSize * 4);
    final back = readLogoImage(rgba, helmetSize, colors);
    expect(back.pixels, px);
    expect(back.colors, colors);
  });

  test('closest NES color prefers \$0F for black and never picks \$0D', () {
    expect(nearestNesColor(0x000000), 0x0F);
    for (var c = 0; c < 64; c++) {
      expect(nearestNesColor(nesPalette[c]), isNot(0x0D));
    }
  });
}
