// In-place editing of the small team-select helmets: pixels in the CHR tiles
// the helmet is drawn from, plus its palette group.
import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:tecmo_graphic_editor/small_helmet.dart';
import 'package:tecmo_graphic_editor/small_helmet_edit.dart';
import 'package:tecmo_graphic_editor/small_helmet_image.dart';
import 'package:tecmo_graphic_editor/small_helmet_renderer.dart';
import 'package:test/test.dart';

NesRom loadRom(String name) => NesRom(File('ROMS/$name').readAsBytesSync());

int teamIndex(NesRom rom, String name) => SmallHelmetReader(rom).teams.indexOf(name);

/// Bytes that differ between two ROM images.
int diffBytes(Uint8List a, Uint8List b) {
  var n = 0;
  for (var i = 0; i < a.length; i++) {
    if (a[i] != b[i]) n++;
  }
  return n;
}

void main() {
  final orig = loadRom('TSPRBOWL.nes');
  final rom32 = loadRom('TSB 2007-32-105.nes');

  test('an untouched editor changes nothing', () {
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'bills'));
    expect(e.changed, isFalse);
    expect(e.alsoAffects, isEmpty);
    expect(diffBytes(e.build(), orig.bytes), 0);
  });

  test('the layer views match what the renderer draws', () {
    for (final rom in [orig, rom32]) {
      final r = SmallHelmetReader(rom);
      for (var t = 0; t < r.teams.length; t++) {
        final e = SmallHelmetEditor(rom, t);
        expect(e.bg.length, 256);
        expect(e.overlay.length, smallHelmetWidth * smallHelmetHeight);
        // Every opaque background pixel of the helmet is in the layer view.
        final h = r.read(t);
        for (var k = 0; k < 4; k++) {
          if (h.tiles[k] == 0) continue;
          final px = rom.tile(SmallHelmetReader.bgBank(h.tiles[k]), h.tiles[k]);
          for (var i = 0; i < 64; i++) {
            final x = (k & 1) * 8 + i % 8, y = (k >> 1) * 8 + i ~/ 8;
            expect(e.bg[y * 16 + x], px[i], reason: '${h.team} slot $k pixel $i');
          }
        }
      }
    }
  });

  test('painting a background pixel edits its tile and names the other teams that use it', () {
    final t = teamIndex(orig, 'steelers'); // tiles 8f 89 8a 8b, shared
    final e = SmallHelmetEditor(orig, t);
    final before = e.bg[0];
    final value = (before + 1) & 3;
    expect(e.paintBg(0, 0, value), isTrue);
    expect(e.bg[0], value);
    expect(e.changed, isTrue);
    // 8f is also used by the Bears and Falcons, and by the Vikings' overlay.
    expect(e.alsoAffects, ['bears', 'vikings', 'falcons']);

    final out = e.build();
    expect(diffBytes(out, orig.bytes), lessThanOrEqualTo(2)); // one pixel: at most 2 CHR bytes
    final edited = NesRom(out);
    expect(edited.tile(0x7A, 0x8F)[0], value);
    // The Bears' helmet changed in the same place.
    final bears = SmallHelmetEditor(edited, teamIndex(edited, 'bears'));
    expect(bears.bg[0], value);
  });

  test('a private tile affects nobody else', () {
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'vikings')); // BG tiles d2 d3 are private
    e.paintBg(3, 3, 3);
    expect(e.alsoAffects, isEmpty);
  });

  test('empty slots cannot be painted', () {
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'vikings')); // bottom row tiles are 00
    expect(e.bgEditable(2, 12), isFalse);
    expect(e.paintBg(2, 12, 2), isFalse);
    expect(e.changed, isFalse);
  });

  test('overlay pixels are painted into the sprite that covers them', () {
    final t = teamIndex(orig, 'browns'); // overlay 2: sprites b9 a1 a2 8b
    final e = SmallHelmetEditor(orig, t);
    expect(e.overlayEditable(5, 3), isTrue); // inside the first sprite (dx 0, dy 0 -> box at 4,1)
    expect(e.overlayEditable(0, 0), isFalse); // outside every sprite
    final before = e.overlay[3 * smallHelmetWidth + 5];
    final value = (before + 2) & 3 == 0 ? 1 : (before + 2) & 3;
    expect(e.paintOverlay(5, 3, value), isTrue);
    expect(e.overlay[3 * smallHelmetWidth + 5], value);
    expect(e.paintOverlay(0, 0, 1), isFalse);
    final edited = NesRom(e.build());
    // Sprite tile b9 lives in bank 7A, tile index 0x39; the pixel is row 2, column 1 of that tile.
    expect(edited.tile(0x7A, 0x39)[2 * 8 + 1], value);
  });

  test('a tile used by a background slot and an overlay sprite changes in both layers', () {
    final t = teamIndex(orig, 'vikings'); // overlay uses the Steelers shell tiles 8f 89 8a 8b
    final e = SmallHelmetEditor(orig, t);
    expect(e.paintOverlay(5, 2, 3), isTrue); // first sprite is 8f
    expect(e.alsoAffects, containsAll(['steelers', 'bears', 'falcons']));
  });

  test('palette group: original ROM writes the flags byte and keeps the overlay id', () {
    final t = teamIndex(orig, 'dolphins'); // flags 07: overlay 1, palette 3
    final e = SmallHelmetEditor(orig, t)..setPalette(1);
    expect(e.changed, isTrue);
    expect(e.alsoAffects, isEmpty);
    final out = e.build();
    expect(diffBytes(out, orig.bytes), 1);
    final h = SmallHelmetReader(NesRom(out)).read(t);
    expect(h.overlayId, 1);
    expect(h.tilePalettes, [1, 1, 1, 1]);
    // Nobody else moved.
    final others = SmallHelmetReader(NesRom(out));
    for (var i = 0; i < 28; i++) {
      if (i == t) continue;
      expect(others.read(i).tilePalettes, SmallHelmetReader(orig).read(i).tilePalettes, reason: 'team $i');
    }
  });

  test('palette group: 32-team ROM writes both attribute tables for that helmet only', () {
    final t = teamIndex(rom32, 'dolphins'); // left 3, right 0 in the table
    final e = SmallHelmetEditor(rom32, t)..setPalette(2);
    final out = NesRom(e.build());
    final r = SmallHelmetReader(out);
    expect(r.read(t).tilePalettes, [2, 2, 2, 2]);
    for (final (off, shift) in r.attrSlots32(t)) {
      expect((out.prg(off) >> shift) & 3, 2);
    }
    final base = SmallHelmetReader(rom32);
    for (var i = 0; i < 32; i++) {
      if (i == t) continue;
      expect(r.read(i).tilePalettes, base.read(i).tilePalettes, reason: 'team $i');
    }
  });

  test('applying a copied helmet reproduces its pixels where the target has tiles', () {
    final browns = teamIndex(orig, 'browns'); // cc cd ce 00
    final vikings = teamIndex(orig, 'vikings'); // d2 d3 00 00
    final pic = SmallHelmetEditor(orig, vikings).picture();
    final e = SmallHelmetEditor(orig, browns);
    final r = e.applyPicture(pic, overlay: false);
    expect(r.droppedBg, 0); // the Vikings' bottom row is empty, the Browns' empty slot stays empty
    for (var i = 0; i < 256; i++) {
      if (e.bgEditable(i % 16, i ~/ 16)) expect(e.bg[i], pic.bg[i], reason: 'pixel $i');
    }
    expect(e.tilePalettes, List.filled(4, pic.tilePalettes[0])); // original ROM: one palette for all four tiles
  });

  test('pixels the target has no tile for are dropped and counted', () {
    final bills = SmallHelmetEditor(orig, teamIndex(orig, 'bills')).picture(); // four full tiles
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'browns')); // bottom-right slot is empty
    final r = e.applyPicture(bills);
    expect(r.droppedBg, greaterThan(0));
    expect(r.notes.join(' '), contains('background'));
  });

  test('a source with no overlay leaves the target overlay alone', () {
    final bills = SmallHelmetEditor(orig, teamIndex(orig, 'bills')).picture();
    expect(bills.hasOverlay, isFalse);
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'steelers'));
    final overlayBefore = Uint8List.fromList(e.overlay);
    e.applyPicture(bills);
    expect(e.overlay, overlayBefore);
  });

  test('applying a helmet to itself is a no-op', () {
    for (final name in ['bills', 'steelers', 'redskins', 'vikings']) {
      final e = SmallHelmetEditor(orig, teamIndex(orig, name));
      e.applyPicture(e.picture());
      expect(e.changed, isFalse, reason: name);
    }
  });

  test('PNG round trip: every helmet reads back as it was exported', () {
    for (final rom in [orig, rom32]) {
      final n = SmallHelmetReader(rom).teams.length;
      for (var t = 0; t < n; t++) {
        final e = SmallHelmetEditor(rom, t);
        final rgba = smallHelmetImageRgba(e);
        expect(rgba.length, smallImageWidth * smallImageHeight * 4);
        final back = readSmallHelmetImage(rgba, e);
        expect(back.bg, e.bg, reason: 'bg team $t');
        expect(back.overlay, e.overlay, reason: 'overlay team $t');
      }
    }
  });

  test('importing an image with no transparency treats its corner as background', () {
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'bills'));
    final rgba = smallHelmetImageRgba(e);
    final corner = rgba.sublist(0, 4);
    for (var k = 0; k < rgba.length; k += 4) {
      if (rgba[k + 3] == 0) rgba.setRange(k, k + 4, [corner[0], corner[1], corner[2], 255]);
    }
    // Make the corner color something no helmet color is.
    final back = readSmallHelmetImage(rgba, e);
    expect(back.bg.where((v) => v != 0).length, e.bg.where((v) => v != 0).length);
  });

  test('an edited ROM renders the edit in the helmet grid', () {
    final t = teamIndex(orig, 'vikings');
    final e = SmallHelmetEditor(orig, t)..paintBg(0, 0, 3);
    final edited = NesRom(e.build());
    final img = renderSmallHelmetIndexes(edited, SmallHelmetReader(edited).read(t));
    final h = SmallHelmetReader(edited).read(t);
    // Background pixel (0,0) is drawn one line lower, 4 pixels right of the canvas edge.
    expect(img[1 * smallHelmetWidth + smallHelmetMarginX], h.paletteRam[h.tilePalettes[0] * 4 + 3]);
  });

  test('snapshot, restore and revert', () {
    final e = SmallHelmetEditor(orig, teamIndex(orig, 'vikings'));
    final start = e.snapshot();
    e.paintBg(0, 0, 3);
    e.setPalette(1);
    final edited = e.snapshot();
    expect(e.changed, isTrue);
    e.restore(start);
    expect(e.changed, isFalse);
    e.restore(edited);
    expect(e.changed, isTrue);
    e.revert();
    expect(e.changed, isFalse);
    expect(e.bg, SmallHelmetEditor(orig, teamIndex(orig, 'vikings')).bg);
  });
}
