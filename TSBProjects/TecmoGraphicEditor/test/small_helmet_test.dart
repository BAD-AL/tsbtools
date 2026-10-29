// Checks the small-helmet decoder against the team-select grid the real game
// drew, captured in Mesen save states under research/savestates.
import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:tecmo_graphic_editor/small_helmet.dart';
import 'package:tecmo_graphic_editor/small_helmet_renderer.dart';
import 'package:test/test.dart';

const romDir = 'ROMS';
const stateDir = 'research/savestates';

NesRom loadRom(String name) => NesRom(File('$romDir/$name').readAsBytesSync());

Uint8List load(String name) => File('$stateDir/$name').readAsBytesSync();

/// Top-left screen position of a team's helmet (nametable tile x 8).
(int, int) pos(SmallHelmetReader r, int t) {
  final (c, row) = r.gridTile(t);
  return (c * 8, row * 8);
}

void main() {
  final orig = loadRom('TSPRBOWL.nes');
  final rom32 = loadRom('TSB 2007-32-105.nes');

  test('reads the tile table of both ROMs', () {
    final o = SmallHelmetReader(orig), r = SmallHelmetReader(rom32);
    expect(o.teams.length, 28);
    expect(r.teams.length, 32);
    expect(o.read(0).tiles, [0x94, 0x95, 0x96, 0x8B]); // bills
    expect(r.read(0).tiles, [0x94, 0x95, 0x96, 0x8B]);
    // 49ers/Rams/Seahawks/Cardinals come from the 32-team hook tables.
    expect(r.read(28).tiles, [0xA4, 0xA5, 0xA6, 0x9B]);
    expect(r.read(29).tiles, [0xB4, 0xB5, 0xB6, 0x83]);
    expect(r.read(30).tiles, [0x86, 0x87, 0x82, 0x00]);
    expect(r.read(31).tiles, [0x8C, 0x8D, 0x8E, 0x83]);
  });

  test('32-team helmets match the emulator frame pixel for pixel', () {
    final frame = load('small32.frame.bin');
    final reader = SmallHelmetReader(rom32);
    var totalBad = 0;
    final report = <String>[];
    for (var t = 0; t < 32; t++) {
      final h = reader.read(t);
      final (x, y) = pos(reader, t);
      final img = Uint8List(256 * 240)..fillRange(0, 256 * 240, h.paletteRam[0]);
      drawSmallHelmet(rom32, h, img, 256, 240, x, y);
      var bad = 0;
      for (var yy = y; yy < y + 16; yy++) {
        for (var xx = x; xx < x + 16; xx++) {
          final o = (yy * 256 + xx) * 2;
          final want = (frame[o] | frame[o + 1] << 8) & 0x3F;
          final got = img[yy * 256 + xx];
          if (want != got) bad++;
        }
      }
      if (bad > 0) report.add('${h.team}: $bad');
      totalBad += bad;
    }
    expect(totalBad, 0, reason: report.join(', '));
  });

  test('original-ROM helmets match the nametable, attributes and sprites the game drew', () {
    final nt = load('small28.nt.bin'), oam = load('small28.oam.bin');
    final reader = SmallHelmetReader(orig);
    final sprites = {
      for (var i = 0; i < 64; i++)
        if (oam[i * 4] < 0xEF) '${oam[i * 4 + 1]}@${oam[i * 4 + 3]},${oam[i * 4]},${oam[i * 4 + 2]}'
    };
    final bad = <String>[];
    for (var t = 0; t < 28; t++) {
      final h = reader.read(t);
      final (c, r) = reader.gridTile(t);
      final shown = [nt[r * 32 + c], nt[r * 32 + c + 1], nt[(r + 1) * 32 + c], nt[(r + 1) * 32 + c + 1]];
      if (shown.join(',') != h.tiles.join(',')) bad.add('${h.team} tiles $shown vs ${h.tiles}');
      final a = nt[0x3C0 + (r >> 2) * 8 + (c >> 2)];
      final pal = (a >> (((r >> 1) & 1) * 4 + ((c >> 1) & 1) * 2)) & 3;
      if (pal != h.tilePalettes[0]) bad.add('${h.team} palette $pal vs ${h.tilePalettes}');
      for (final s in h.sprites) {
        final key = '${s.tile}@${c * 8 + s.dx},${r * 8 + s.dy},${s.attr}';
        if (!sprites.contains(key)) bad.add('${h.team} sprite $key missing');
      }
    }
    expect(bad, isEmpty);
  });
}
