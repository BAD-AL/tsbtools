// Reads the small helmets drawn on the NES TSB team-select grid.
// Format notes and how each offset was found: research/NES_graphics_map.md
import 'dart:typed_data';

import 'large_helmet.dart';
import 'nes_rom.dart';

/// One hardware sprite of a helmet's overlay, relative to the helmet's top-left.
class SmallHelmetSprite {
  final int dx; // signed
  final int dy;
  final int tile; // sprite pattern tile $00-$FF (see [SmallHelmetReader.spriteBank])
  final int attr; // OAM attribute: palette bits 0-1, bit 5 = behind background
  const SmallHelmetSprite(this.dx, this.dy, this.tile, this.attr);

  bool get behind => attr & 0x20 != 0;
  bool get flipX => attr & 0x40 != 0;
  bool get flipY => attr & 0x80 != 0;
  int get palette => attr & 3;
}

class SmallHelmet {
  final String team;
  /// Background tile numbers: top-left, top-right, bottom-left, bottom-right.
  final List<int> tiles;
  /// Background palette (0-3) of each tile (TL, TR, BL, BR). The original ROM
  /// uses the flags byte for all four; the 32-team ROM reads a static
  /// attribute table, which can differ between the left and right tile.
  final List<int> tilePalettes;
  /// 0 = none; otherwise the overlay-table entry (5th team byte >> 2).
  final int overlayId;
  final List<SmallHelmetSprite> sprites; // first wins
  /// The screen's full 32-byte PPU palette.
  final Uint8List paletteRam;

  const SmallHelmet({
    required this.team,
    required this.tiles,
    required this.tilePalettes,
    required this.overlayId,
    required this.sprites,
    required this.paletteRam,
  });
}

class SmallHelmetReader {
  // Team table (CPU $BBB6): TL TR BL BR + (overlayId << 2 | palette), 5 bytes per team.
  static const tileTable = 0x23BB6;
  // 32-team hook (fixed bank, `index < 30` uses the table above): teams 30-33.
  static const topRowTable32 = 0x3F6B3; // 2 bytes per team
  static const bottomRowTable32 = 0x3F6BB; // 2 bytes per team
  static const flagsTable32 = 0x3F6C3; // 1 byte per team
  // Overlay sprites: index (CPU $BC41) holds start offsets into the entry list (CPU $BC51).
  static const overlayIndex = 0x23C41;
  static const overlayEntries = 0x23C51;
  // 32-team hack: the attribute table (64 bytes) is copied from here instead of
  // being built from the flags bytes ($A21D). The second variant, 0x40 later,
  // is for the other screen mode ($C2 != $55).
  static const attrTable32 = 0x423A;
  static const attrTable32Alt = 0x427A;
  // Palette data the screen loads: BG (4 palettes) and sprites.
  static const bgPalette = 0x1A140;
  static const spritePalette = 0x1A830;
  // The helmets' CHR banks while the screen is up (MMC3 R0/R1 for BG tiles
  // $00-$FF; R2-R5 for sprite tiles $00-$FF).
  static const _bgBanks = [0x18, 0x19, 0x7A, 0x7B];
  static const _spriteBanks = [0x0F, 0x1D, 0x7A, 0x7B];

  final NesRom rom;
  final LargeHelmetReader _names;

  SmallHelmetReader(this.rom) : _names = LargeHelmetReader(rom);

  bool get is32Team => _names.is32Team;
  List<String> get teams => _names.teams;

  int _romIndex(int team) => is32Team && team >= 28 ? team + 2 : team;
  bool _extra(int team) => is32Team && _romIndex(team) >= 30;

  /// PRG offsets of the team's four tile bytes (TL, TR, BL, BR) and its flags byte.
  List<int> tileOffsets(int team) {
    final i = _romIndex(team);
    if (_extra(team)) {
      final k = i - 30;
      return [topRowTable32 + 2 * k, topRowTable32 + 2 * k + 1, bottomRowTable32 + 2 * k, bottomRowTable32 + 2 * k + 1];
    }
    return [for (var k = 0; k < 4; k++) tileTable + 5 * i + k];
  }

  int flagsOffset(int team) {
    final i = _romIndex(team);
    return _extra(team) ? flagsTable32 + i - 30 : tileTable + 5 * i + 4;
  }

  /// Nametable (column, row) of the helmet's top-left tile on the grid.
  /// Original ROM: 3 columns of 5/4/5 (AFC) and 5/5/4 (NFC) at columns 2/12/22.
  /// 32-team ROM: 4 columns at 1/9/17/25, four rows per group, AFC above NFC.
  (int, int) gridTile(int team) {
    final groupSize = is32Team ? 16 : 14;
    final nfc = team >= groupSize;
    var i = nfc ? team - groupSize : team;
    if (is32Team) return (1 + 8 * (i >> 2), (nfc ? 18 : 8) + 2 * (i & 3));
    final sizes = nfc ? [5, 5, 4] : [5, 4, 5];
    var col = 0;
    while (i >= sizes[col]) {
      i -= sizes[col++];
    }
    return (2 + 10 * col, (nfc ? 18 : 6) + 2 * i);
  }

  /// Where the 32-team ROM keeps each tile's palette bits: (PRG offset, bit shift)
  /// for the four tiles (TL, TR, BL, BR) in both screen variants. The "team data"
  /// grid starts its AFC helmets at row 8, the preseason select-team grid at row 6;
  /// the NFC rows are the same.
  List<(int, int)> attrSlots32(int team) {
    final (col, row) = gridTile(team);
    final altRow = team < 16 ? row - 2 : row;
    return [
      for (final (base, r0) in [(attrTable32, row), (attrTable32Alt, altRow)])
        for (var k = 0; k < 4; k++)
          (
            base + ((r0 + (k >> 1)) >> 2) * 8 + ((col + (k & 1)) >> 2),
            (((r0 + (k >> 1)) >> 1) & 1) * 4 + (((col + (k & 1)) >> 1) & 1) * 2,
          )
    ];
  }

  int _attrPalette(int col, int row) {
    final a = rom.prg(attrTable32 + (row >> 2) * 8 + (col >> 2));
    return (a >> (((row >> 1) & 1) * 4 + ((col >> 1) & 1) * 2)) & 3;
  }

  /// CHR bank holding sprite tile [tile] on this screen.
  static int spriteBank(int tile) => _spriteBanks[tile >> 6];

  /// CHR bank holding background tile [tile] on this screen. The 32-team hack
  /// uses tiles below $80 (bank $19) for some helmets.
  static int bgBank(int tile) => _bgBanks[tile >> 6];

  SmallHelmet read(int team) {
    final flags = rom.prg(flagsOffset(team));
    final id = flags >> 2;
    final pal = Uint8List(32);
    for (var k = 0; k < 16; k++) {
      pal[k] = rom.prg(bgPalette + k);
      pal[16 + k] = rom.prg(spritePalette + k);
    }
    final (col, row) = gridTile(team);
    return SmallHelmet(
      team: teams[team],
      tiles: [for (final o in tileOffsets(team)) rom.prg(o)],
      tilePalettes: is32Team
          ? [_attrPalette(col, row), _attrPalette(col + 1, row), _attrPalette(col, row + 1), _attrPalette(col + 1, row + 1)]
          : List.filled(4, flags & 3),
      overlayId: id,
      sprites: id == 0 ? const [] : _readOverlay(id),
      paletteRam: pal,
    );
  }

  List<SmallHelmetSprite> _readOverlay(int id) {
    // The game's loop ($A7E8) checks the end after drawing an entry, so it
    // always draws one; the last id reads its end from the entry list (0).
    final start = rom.prg(overlayIndex + id), end = rom.prg(overlayIndex + id + 1);
    int s(int v) => v >= 0x80 ? v - 0x100 : v;
    return [
      for (var p = start; p == start || p < end; p += 4)
        SmallHelmetSprite(
          s(rom.prg(overlayEntries + p + 3)),
          s(rom.prg(overlayEntries + p)),
          rom.prg(overlayEntries + p + 1),
          rom.prg(overlayEntries + p + 2),
        )
    ];
  }
}
