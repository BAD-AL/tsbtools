import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/helmet_renderer.dart';
import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/logo_edit.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:test/test.dart';

LargeHelmetReader load(String name) => LargeHelmetReader(NesRom(File('ROMS/$name').readAsBytesSync()));

Uint8List render(LargeHelmetReader r, int team) => renderHelmetIndexes(r.rom, r.read(team));

/// Applies [pixels] to [team] and checks nothing but that team's logo changed.
LargeHelmetReader editAndCheck(LargeHelmetReader r, int team, Uint8List pixels, {List<(int, int)>? boxes}) {
  final plan = boxes == null ? planLogoEdit(r, team, pixels) : checkLogoEdit(r, team, pixels, boxes: boxes).plan;
  expect(plan.ok, isTrue, reason: '${r.teams[team]}: ${plan.errors}');
  final after = LargeHelmetReader(NesRom(plan.result!));
  expect(logoPixels(after, team), pixels, reason: '${r.teams[team]}: logo should be exactly what was drawn');
  for (var i = 0; i < r.teams.length; i++) {
    if (i == team || plan.alsoAffects.contains(r.teams[i])) continue;
    expect(render(after, i), render(r, i), reason: 'editing ${r.teams[team]} changed ${r.teams[i]}');
  }
  return after;
}

void main() {
  final orig = load('TSPRBOWL.nes');
  final rom32 = load('TSB 2007-32-105.nes');

  test('logoPixels matches the rendered logo', () {
    for (final r in [orig, rom32]) {
      for (var t = 0; t < r.teams.length; t++) {
        final px = logoPixels(r, t);
        final pal = r.read(t).paletteRam;
        final drawn = renderHelmetIndexes(r.rom, r.read(t), shell: false, facemask: false);
        expect(px.length, helmetSize * helmetSize);
        for (var k = 0; k < px.length; k++) {
          expect(drawn[k], px[k] == 0 ? pal[0] : pal[0x1C + px[k]], reason: '${r.teams[t]} pixel $k');
        }
      }
    }
  });

  test('re-applying an unchanged logo works for every team and changes nothing', () {
    for (final r in [orig, rom32]) {
      for (var t = 0; t < r.teams.length; t++) {
        final after = editAndCheck(r, t, logoPixels(r, t));
        expect(render(after, t), render(r, t), reason: r.teams[t]);
      }
    }
  });

  test('drawing new pixels', () {
    for (final (r, team) in [(rom32, 'bills'), (orig, 'bills'), (rom32, 'cardinals')]) {
      final t = r.teams.indexOf(team);
      final px = logoPixels(r, t);
      for (var y = 20; y < 23; y++) {
        for (var x = 10; x < 13; x++) {
          px[y * helmetSize + x] = 2;
        }
      }
      editAndCheck(r, t, px);
    }
  });

  test('erasing a whole logo', () {
    final t = rom32.teams.indexOf('jets');
    final after = editAndCheck(rom32, t, Uint8List(helmetSize * helmetSize));
    expect(after.read(t).sprites.where((s) => s.palette == 3), isEmpty);
  });

  test('a logo needing too many sprites is refused with a reason', () {
    final plan = planLogoEdit(rom32, 0, Uint8List(helmetSize * helmetSize)..fillRange(0, helmetSize * helmetSize, 1));
    expect(plan.ok, isFalse);
    expect(plan.errors.join(), contains('sprites'));
  });

  test('a line with more than 8 sprites is refused with a reason', () {
    final px = Uint8List(helmetSize * helmetSize);
    for (var x = 0; x < helmetSize; x++) {
      px[40 * helmetSize + x] = 1;
    }
    final plan = planLogoEdit(rom32, 0, px);
    expect(plan.ok, isFalse);
    expect(plan.errors.join(), contains('line'));
  });

  group('moving sprites', () {
    Uint8List blank() => Uint8List(helmetSize * helmetSize);
    int at(Uint8List px, int x, int y) => px[y * helmetSize + x];

    test('a sprite takes its pixels along and comes to the front', () {
      final px = blank();
      px[10 * helmetSize + 10] = 2; // inside the box at (8,8)
      px[31 * helmetSize + 31] = 1; // inside the box at (30,30)
      final m = moveLogoSprite(px, [(30, 30), (8, 8)], 1, 5, 2);
      expect(m.boxes, [(13, 10), (30, 30)]);
      expect(at(m.pixels, 10, 10), 0);
      expect(at(m.pixels, 15, 12), 2);
      expect(at(m.pixels, 31, 31), 1);
    });

    test('pixels shown by a sprite in front stay put', () {
      final px = blank();
      px[1 * helmetSize + 5] = 3; // covered by both boxes; the first one shows it
      final m = moveLogoSprite(px, [(0, 0), (4, 0)], 1, 0, 20);
      expect(at(m.pixels, 5, 1), 3);
      expect(at(m.pixels, 5, 21), 0);
    });

    test('a sprite cannot be dragged out of the helmet', () {
      final m = moveLogoSprite(blank(), [(60, 0)], 0, 10, -5);
      expect(m.boxes, [(64, 0)]);
    });

    test('the edit keeps sprites where the user put them', () {
      final t = rom32.teams.indexOf('bills');
      final start = checkLogoEdit(rom32, t, logoPixels(rom32, t));
      final m = moveLogoSprite(logoPixels(rom32, t), start.stats.boxes, 2, 3, -4);
      final c = checkLogoEdit(rom32, t, m.pixels, boxes: m.boxes);
      expect(c.stats.boxes, m.boxes);
      final after = editAndCheck(rom32, t, m.pixels, boxes: m.boxes);
      expect([for (final s in after.read(t).sprites) if (s.palette == 3) (s.x - helmetLeft, s.y - helmetTop)], m.boxes);
    });

    test('a bad hand layout is refused, with a hint when auto-arrange would fit', () {
      final px = blank();
      for (var y = 8; y < 48; y++) {
        for (var x = 8; x < 32; x++) {
          px[y * helmetSize + x] = 1; // 3 x 5 tiles on the 8-pixel grid
        }
      }
      final shifted = [for (var y = 4; y < 48; y += 8) for (var x = 4; x < 32; x += 8) (x, y)];
      final c = checkLogoEdit(rom32, 0, px, boxes: shifted);
      expect(c.plan.ok, isFalse);
      expect(c.stats.boxes.length, 24);
      expect(c.autoArrangeFits, isTrue);
      expect(checkLogoEdit(rom32, 0, px).plan.ok, isTrue);
    });
  });

  test('stats report sprites and the busiest line', () {
    final t = orig.teams.indexOf('bills');
    final s = logoStats(orig, t, logoPixels(orig, t));
    expect(s.sprites, orig.read(t).sprites.where((s) => s.palette == 3).length);
    expect(s.busiestLine, lessThanOrEqualTo(8));
  });
}
