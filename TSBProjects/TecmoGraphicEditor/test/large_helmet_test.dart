// Checks the large-helmet decoder against what the real game put on screen,
// captured in Mesen save states under research/savestates.
import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/helmet_renderer.dart';
import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:test/test.dart';

const romDir = 'ROMS';
const stateDir = 'research/savestates';

NesRom loadRom(String name) => NesRom(File('$romDir/$name').readAsBytesSync());

/// Key/value blob from a Mesen2 .mss (already zlib-inflated: *.state.bin).
Map<String, Uint8List> readState(String name) {
  final d = File('$stateDir/$name').readAsBytesSync();
  final out = <String, Uint8List>{};
  var i = 0;
  while (i < d.length) {
    final end = d.indexOf(0, i);
    if (end < 0) break;
    final key = String.fromCharCodes(d.sublist(i, end));
    final n = ByteData.sublistView(d, end + 1, end + 5).getUint32(0, Endian.little);
    out[key] = d.sublist(end + 5, end + 5 + n);
    i = end + 5 + n;
  }
  return out;
}

/// OAM entries as "TT@x,y pP" strings (y is the on-screen line, OAM y + 1).
Set<String> oamSprites(Uint8List oam, int palette) => {
      for (var i = 0; i < 64; i++)
        if (oam[i * 4] < 0xEF && (oam[i * 4 + 2] & 3) == palette && i != 0)
          '${hex(oam[i * 4 + 1])}@${oam[i * 4 + 3]},${oam[i * 4] + 1}'
    };

Set<String> decodedSprites(LargeHelmet h, int palette) => {
      for (final s in h.sprites)
        if (s.palette == palette) '${hex(s.tile)}@${s.x},${s.y}'
    };

String hex(int v) => v.toRadixString(16).toUpperCase().padLeft(2, '0');

void main() {
  final orig = loadRom('TSPRBOWL.nes');
  final rom32 = loadRom('TSB 2007-32-105.nes');

  test('detects ROM layouts', () {
    expect(LargeHelmetReader(orig).is32Team, isFalse);
    expect(LargeHelmetReader(rom32).is32Team, isTrue);
    expect(LargeHelmetReader(orig).teams.length, 28);
    expect(LargeHelmetReader(rom32).teams.length, 32);
  });

  for (final (rom, team, state) in [
    (orig, 'bills', 'bills.state.bin'),
    (orig, 'rams', 'rams.state.bin'),
    (rom32, 'bills', 'bills32.state.bin'),
  ]) {
    group('$team ($state)', () {
      final reader = LargeHelmetReader(rom);
      final h = reader.read(reader.teams.indexOf(team));
      final st = readState(state);

      test('logo sprites match OAM', () {
        expect(decodedSprites(h, 3), oamSprites(st['ppu.spriteRam']!, 3));
      });
      test('facemask sprites match OAM', () {
        expect(decodedSprites(h, 2), oamSprites(st['ppu.spriteRam']!, 2));
      });
      test('sprite CHR banks match the MMC3 registers', () {
        final regs = st['mapper.registers']!; // R0..R7
        expect(h.spriteBankLow, regs[4]);
        expect(h.spriteBankHigh, regs[5]);
      });
      test('shell tiles match the nametable', () {
        final nt = st['mapper.nametableRam']!;
        for (var r = 0; r < 8; r++) {
          for (var c = 0; c < 8; c++) {
            expect(h.shellTiles[r * 8 + c], nt[(r + 4) * 32 + c + 4], reason: 'row $r col $c');
          }
        }
      });
      test('palette matches palette RAM', () {
        expect(h.paletteRam, st['ppu.paletteRam']);
      });
      test('rendered helmet matches the emulator frame pixel for pixel', () {
        // *.frame.bin: 256x240 little-endian u16 NES color indexes.
        final frame = File('$stateDir/${state.replaceAll('state', 'frame')}').readAsBytesSync();
        final img = renderHelmetIndexes(rom, h);
        var bad = 0;
        for (var y = 0; y < helmetSize; y++) {
          for (var x = 0; x < helmetSize; x++) {
            final o = ((y + helmetTop) * 256 + x + helmetLeft) * 2;
            if ((frame[o] & 0x3F) != img[y * helmetSize + x]) bad++;
          }
        }
        expect(bad, 0, reason: '$bad of ${helmetSize * helmetSize} pixels differ');
      });
    });
  }

  test('jaguars (32-team expansion logo) matches OAM', () {
    final reader = LargeHelmetReader(rom32);
    final h = reader.read(reader.teams.indexOf('jaguars'));
    final oam = File('$stateDir/jax32_oam.bin').readAsBytesSync();
    expect(decodedSprites(h, 3), oamSprites(oam, 3));
    expect(decodedSprites(h, 2), oamSprites(oam, 2));
    expect(h.spriteBankLow, 0x81);
  });

  test('49ers (32-team hook for ROM indexes 30-33) matches OAM', () {
    final reader = LargeHelmetReader(rom32);
    final h = reader.read(reader.teams.indexOf('49ers'));
    final oam = File('$stateDir/sf32_oam.bin').readAsBytesSync();
    expect(h.logoId, isNull);
    expect(decodedSprites(h, 3), oamSprites(oam, 3));
    expect(decodedSprites(h, 2), oamSprites(oam, 2));
  });

  test('every team decodes on both ROMs', () {
    for (final rom in [orig, rom32]) {
      final reader = LargeHelmetReader(rom);
      for (var i = 0; i < reader.teams.length; i++) {
        final h = reader.read(i);
        expect(h.sprites, isNotEmpty, reason: reader.teams[i]);
        expect([0x40, 0x41, 0x42, 0x43], contains(h.shellId), reason: reader.teams[i]);
      }
    }
  });
}
