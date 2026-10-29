// Reads the large helmet shown on the NES TSB "team data" screen.
// Format notes and how each offset was found: research/NES_graphics_map.md
import 'dart:typed_data';

import 'nes_rom.dart';

/// One hardware sprite as the game places it on screen.
class HelmetSprite {
  final int tile; // $80-$FF in the sprite pattern table
  final int bank; // 1KB CHR bank the tile comes from
  final int x;
  final int y; // screen line (OAM y + 1)
  final int palette; // sprite palette 0-3
  final bool flipX;

  const HelmetSprite(this.tile, this.bank, this.x, this.y, this.palette, {this.flipX = false});
}

class LargeHelmet {
  final String team;
  final int shellId; // $40-$43
  /// Sprite group; null for 32-team ROM indexes 30-33, whose logo header
  /// pointer comes straight from the hack's own table.
  final int? logoId;
  /// 8x8 background tile numbers, row-major, screen tiles (4,4)-(11,11).
  final List<int> shellTiles;
  /// Background palette (0-3) for each 16x16 area of the shell, 4x4 row-major.
  final List<int> shellPalettes;
  final List<HelmetSprite> sprites; // drawing priority: first wins
  final int spriteBankLow; // CHR bank for sprite tiles $80-$BF (logo)
  final int spriteBankHigh; // CHR bank for sprite tiles $C0-$FF (facemask)
  /// The screen's full 32-byte PPU palette with this team's colors in it.
  final Uint8List paletteRam;
  /// Background CHR banks for tiles $00-$3F, $40-$7F, $80-$BF, $C0-$FF.
  final List<int> bgBanks;

  const LargeHelmet({
    required this.team,
    required this.shellId,
    required this.logoId,
    required this.shellTiles,
    required this.shellPalettes,
    required this.sprites,
    required this.spriteBankLow,
    required this.spriteBankHigh,
    required this.paletteRam,
    required this.bgBanks,
  });
}

class LargeHelmetReader {
  // Original-ROM team order (PRG tables are indexed this way).
  static const _teams28 = [
    'bills', 'colts', 'dolphins', 'patriots', 'jets',
    'bengals', 'browns', 'oilers', 'steelers',
    'broncos', 'chiefs', 'raiders', 'chargers', 'seahawks',
    'redskins', 'giants', 'eagles', 'cardinals', 'cowboys',
    'bears', 'lions', 'packers', 'vikings', 'buccaneers',
    '49ers', 'rams', 'saints', 'falcons',
  ];
  // 32-team (CXROM) order; ROM indexes 28/29 are the AFC/NFC all-stars, so the
  // last four teams sit at 30-33 (matches TSBTool's TecmoToolFactory).
  static const _teams32 = [
    'bills', 'dolphins', 'patriots', 'jets',
    'bengals', 'browns', 'ravens', 'steelers',
    'colts', 'texans', 'jaguars', 'titans',
    'broncos', 'chiefs', 'raiders', 'chargers',
    'redskins', 'giants', 'eagles', 'cowboys',
    'bears', 'lions', 'packers', 'vikings',
    'buccaneers', 'saints', 'falcons', 'panthers',
    '49ers', 'rams', 'seahawks', 'cardinals',
  ];

  // Team tables in PRG bank $22000 (CPU $A000).
  static const _paletteTable = 0x23D69; // $BD69: 8 bytes per team
  static const _shellTable = 0x23E49; // $BE49
  static const _logoTable = 0x23E65; // $BE65
  // 32-team hack: extra tables for ROM team indexes 30-33.
  static const _shellTable32 = 0x3F65A; // $F65A
  static const _paletteTable32 = 0x23FE0; // $BFE0
  static const _logoHeaders32 = 0x2FECD; // $9E91 + 2*30 (hook at $F8CB)

  static const _groupPointers = 0x10000; // sprite group ID -> header
  static const _facemaskGroup = 0x4F; // hard-coded at $A902
  static const _shellScripts = 0x5000; // shell ID -> script ($A000 window, bank $4000)
  static const _shellBlocks = 0x16000; // block ID -> 17 bytes (attribute + 4x4 tiles)

  // What the team data screen has loaded when the helmet is drawn (identical
  // on both ROMs). The top of the screen uses BG banks $2A/$2B for tiles
  // $80-$FF; the MMC3 IRQ switches to $7A/$7B for the roster below, which is
  // what a save state's registers show. $00-$7F are unused by the shell.
  static const _bgBanks = [0x18, 0x19, 0x2A, 0x2B];
  static const _defaultSpriteBanks = [0x70, 0x73]; // MMC3 R4, R5
  static const _basePalette = [
    0x0F, 0x25, 0x00, 0x30, 0x0F, 0x00, 0x00, 0x29, 0x0F, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00,
    0x0F, 0x00, 0x00, 0x30, 0x0F, 0x00, 0x00, 0x25, 0x0F, 0x0F, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00,
  ];
  // Where the 8 palette bytes land in palette RAM: shell (BG 3, colors 1-3),
  // facemask (sprite 2, colors 2-3), logo (sprite 3, colors 1-3).
  static const _paletteSlots = [0x0D, 0x0E, 0x0F, 0x1A, 0x1B, 0x1D, 0x1E, 0x1F];

  // Screen position constants: the copy routine at $A91D adds $C4 to x and
  // $B4 to y.
  static const _xBias = -60;
  static const _yBias = -59;

  final NesRom rom;
  final bool is32Team;

  LargeHelmetReader(this.rom) : is32Team = _detect32(rom);

  /// The 32-team hack doubles CHR and replaces `LDA $BE49,X` at $A8CA with `JSR $F64A`.
  static bool _detect32(NesRom rom) =>
      rom.chrSize == 0x40000 &&
      rom.prg(0x228CA) == 0x20 && rom.prg(0x228CB) == 0x4A && rom.prg(0x228CC) == 0xF6;

  List<String> get teams => is32Team ? _teams32 : _teams28;

  int _romIndex(int team) => is32Team && team >= 28 ? team + 2 : team;

  // ROM indexes 30-33 of a 32-team ROM go through the hack's own tables.
  bool _extra(int team) => is32Team && _romIndex(team) >= 30;

  /// PRG offset of the team's shell ID byte.
  int shellIdOffset(int team) {
    final i = _romIndex(team);
    return _extra(team) ? _shellTable32 + i - 30 : _shellTable + i;
  }

  /// PRG offset of the team's 8 helmet palette bytes.
  int paletteOffset(int team) {
    final i = _romIndex(team);
    return _extra(team) ? _paletteTable32 + 8 * (i - 30) : _paletteTable + 8 * i;
  }

  int? _logoId(int team) => _extra(team) ? null : rom.prg(_logoTable + _romIndex(team));

  /// CPU address of the team's logo sprite-group header.
  int logoHeaderPointer(int team) {
    final id = _logoId(team);
    return id == null ? rom.prgWord(_logoHeaders32 + 2 * (_romIndex(team) - 30)) : _groupHeader(id);
  }

  int get facemaskHeaderPointer => _groupHeader(_facemaskGroup);

  /// PRG offset of the sprite group header at CPU address [ptr].
  static int headerOffset(int ptr) => ptr >= 0xA000 ? 0x10000 + ptr - 0xA000 : 0x2E000 + ptr - 0x8000;

  /// Sprite data streams live in PRG $10000-$11FFF; headers hold them as `$80 | high`, low.
  static int dataOffset(int hi, int lo) => 0x10000 + (((hi << 8) | lo) & 0x7FFF);

  LargeHelmet read(int team) {
    final shellId = rom.prg(shellIdOffset(team));
    final logoId = _logoId(team);

    final banks = List.of(_defaultSpriteBanks);
    final logo = _readGroup(logoHeaderPointer(team), banks);
    final mask = _readGroup(facemaskHeaderPointer, banks);
    // Tiles go to the bank selected when the frame is shown, i.e. after both groups.
    final sprites = [
      for (final s in [...logo, ...mask])
        HelmetSprite(s.tile, s.tile < 0xC0 ? banks[0] : banks[1], s.x, s.y, s.palette, flipX: s.flipX)
    ];

    final (tiles, palettes) = _readShell(shellId);

    final pal = Uint8List.fromList(_basePalette);
    final palAt = paletteOffset(team);
    for (var k = 0; k < 8; k++) {
      pal[_paletteSlots[k]] = rom.prg(palAt + k);
    }

    return LargeHelmet(
      team: teams[team],
      shellId: shellId,
      logoId: logoId,
      shellTiles: tiles,
      shellPalettes: palettes,
      sprites: sprites,
      spriteBankLow: banks[0],
      spriteBankHigh: banks[1],
      paletteRam: pal,
      bgBanks: _bgBanks,
    );
  }

  int _groupHeader(int id) => rom.prgWord(_groupPointers + 2 * id);

  /// Sprite group header at CPU [ptr]: `f6 a b c d` (sprite CHR banks, ff = keep),
  /// `fc n` (running logo number; no effect on placement), `f8 Y X` (base, 2px units),
  /// then a big-endian data pointer ($80-$EF high byte).
  List<HelmetSprite> _readGroup(int ptr, List<int> banks) {
    var p = headerOffset(ptr);
    var baseX = 0, baseY = 0;
    while (true) {
      final cmd = rom.prg(p++);
      if (cmd == 0xF6) {
        for (var k = 0; k < 4; k++) {
          final b = rom.prg(p++);
          if (b != 0xFF && k < 2) banks[k] = b;
        }
      } else if (cmd == 0xFC) {
        p++;
      } else if (cmd == 0xF8) {
        baseY = rom.prg(p++);
        baseX = rom.prg(p++);
      } else if (cmd >= 0x80 && cmd < 0xF0) {
        return _readSpriteData(dataOffset(cmd, rom.prg(p)), baseX, baseY);
      } else {
        throw FormatException('Unknown sprite group command \$${cmd.toRadixString(16)} (header \$${ptr.toRadixString(16)})');
      }
    }
  }

  /// Column stream (decoder at $9B0F in bank $17): `80-9F` starts a column
  /// (signed 5-bit, 8px) at the current fine x; `(s, tile)` pairs where s has
  /// y in bits 2-5 (signed, 8px), palette in bits 0-1 and h-flip in bit 6;
  /// `fc 2n` sets fine y; `fc 3n` sets fine x *and* adds n to the current
  /// column; `ff` ends. This screen ORs 2 into the palette ($A94E).
  List<HelmetSprite> _readSpriteData(int p, int baseX, int baseY) {
    final out = <HelmetSprite>[];
    var colX = 0, fineX = 0, fineY = 0;
    while (true) {
      final b = rom.prg(p++);
      if (b == 0xFF) return out;
      if (b == 0xFC) {
        final n = rom.prg(p++);
        if (n >> 4 == 2) {
          fineY = _signed(n & 0x0F, 4);
        } else if (n >> 4 >= 3) {
          fineX = _signed(n & 0x0F, 4);
          colX += fineX;
        }
      } else if (b >= 0x80) {
        colX = 2 * baseX + 8 * _signed(b & 0x1F, 5) + fineX + _xBias;
      } else {
        final tile = rom.prg(p++) | 0x80;
        final y = 2 * baseY + 8 * _signed((b >> 2) & 0x0F, 4) + fineY + _yBias;
        out.add(HelmetSprite(tile, 0, colX & 0xFF, y & 0xFF, (b & 3) | 2, flipX: b & 0x40 != 0));
      }
    }
  }

  /// Shell script ends with 4 block IDs (top-left, top-right, bottom-left,
  /// bottom-right); each block is an attribute byte plus 4x4 tiles.
  (List<int>, List<int>) _readShell(int shellId) {
    final script = 0x4000 + rom.prgWord(_shellScripts + 2 * shellId) - 0xA000;
    final tiles = List.filled(64, 0);
    final palettes = List.filled(16, 0);
    for (var q = 0; q < 4; q++) {
      final block = _shellBlocks + 17 * rom.prg(script + 7 + q);
      final attr = rom.prg(block);
      final r0 = (q >> 1) * 4, c0 = (q & 1) * 4;
      for (var k = 0; k < 16; k++) {
        tiles[(r0 + k ~/ 4) * 8 + c0 + k % 4] = rom.prg(block + 1 + k);
      }
      for (var a = 0; a < 4; a++) {
        palettes[(r0 ~/ 2 + a ~/ 2) * 4 + c0 ~/ 2 + a % 2] = (attr >> (2 * a)) & 3;
      }
    }
    return (tiles, palettes);
  }

  static int _signed(int v, int bits) => v >= 1 << (bits - 1) ? v - (1 << bits) : v;
}
