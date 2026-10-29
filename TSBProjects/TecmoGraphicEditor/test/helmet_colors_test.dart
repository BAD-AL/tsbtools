import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/helmet_colors.dart';
import 'package:tecmo_graphic_editor/helmet_renderer.dart';
import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:test/test.dart';

LargeHelmetReader load(String name) => LargeHelmetReader(NesRom(File('ROMS/$name').readAsBytesSync()));

void main() {
  final orig = load('TSPRBOWL.nes');
  final rom32 = load('TSB 2007-32-105.nes');

  test('reads the 8 colors in shell, facemask, logo order', () {
    final r = orig, t = orig.teams.indexOf('bills');
    final ram = r.read(t).paletteRam;
    expect(helmetColors(r, t), [ram[0x0D], ram[0x0E], ram[0x0F], ram[0x1A], ram[0x1B], ram[0x1D], ram[0x1E], ram[0x1F]]);
    expect(helmetColorLabels, hasLength(8));
  });

  test('every team owns its own color bytes', () {
    for (final r in [orig, rom32]) {
      final offsets = {for (var i = 0; i < r.teams.length; i++) r.paletteOffset(i)};
      expect(offsets, hasLength(r.teams.length));
    }
  });

  for (final (r, team) in [(orig, 'oilers'), (rom32, 'bills'), (rom32, 'cardinals')]) {
    test('setting ${team}\'s colors (${r.is32Team ? '32-team' : 'original'}) changes only that helmet\'s colors', () {
      final t = r.teams.indexOf(team);
      final before = [for (var i = 0; i < r.teams.length; i++) renderHelmetIndexes(r.rom, r.read(i))];
      final saved = Uint8List.fromList(r.rom.bytes);
      final colors = [0x16, 0x27, 0x30, 0x01, 0x11, 0x2A, 0x1A, 0x0F];

      final after = LargeHelmetReader(NesRom(withHelmetColors(r, t, colors)));
      expect(r.rom.bytes, saved, reason: 'the input ROM must not be modified');
      expect(helmetColors(after, t), colors);
      final h0 = r.read(t), h1 = after.read(t);
      expect(h1.shellTiles, h0.shellTiles);
      expect(h1.sprites.map((s) => '${s.tile}@${s.x},${s.y}'), h0.sprites.map((s) => '${s.tile}@${s.x},${s.y}'));
      for (var i = 0; i < r.teams.length; i++) {
        if (i == t) continue;
        expect(renderHelmetIndexes(after.rom, after.read(i)), before[i], reason: '${r.teams[i]} changed');
      }
    });
  }

  test('rejects bad color lists', () {
    expect(() => withHelmetColors(orig, 0, [0x0F]), throwsArgumentError);
    expect(() => withHelmetColors(orig, 0, List.filled(8, 0x40)), throwsArgumentError);
  });
}
