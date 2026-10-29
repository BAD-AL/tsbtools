import 'dart:typed_data';

/// An iNES ROM image. PRG and CHR are addressed by their offset inside
/// their own area (the 16-byte header is skipped).
class NesRom {
  final Uint8List bytes;
  final int prgSize;
  final int chrSize;

  NesRom._(this.bytes, this.prgSize, this.chrSize);

  factory NesRom(Uint8List bytes) {
    if (bytes.length < 16 ||
        bytes[0] != 0x4E || bytes[1] != 0x45 || bytes[2] != 0x53 || bytes[3] != 0x1A) {
      throw const FormatException('Not an iNES ROM (missing NES<1A> header)');
    }
    final prg = bytes[4] * 0x4000;
    final chr = bytes[5] * 0x2000;
    if (bytes.length < 16 + prg + chr) {
      throw const FormatException('ROM file is shorter than its header says');
    }
    return NesRom._(bytes, prg, chr);
  }

  int get mapper => (bytes[6] >> 4) | (bytes[7] & 0xF0);

  int prg(int offset) => bytes[16 + offset];

  /// Little-endian 16-bit word in PRG.
  int prgWord(int offset) => prg(offset) | prg(offset + 1) << 8;

  /// One 1KB CHR bank (64 tiles of 16 bytes).
  Uint8List chrBank(int bank) =>
      Uint8List.sublistView(bytes, 16 + prgSize + bank * 0x400, 16 + prgSize + bank * 0x400 + 0x400);

  /// 8x8 tile from a 1KB CHR bank as 64 color indexes (0-3), row-major.
  List<int> tile(int bank, int index) {
    final b = chrBank(bank);
    final o = (index & 0x3F) * 16;
    return [
      for (var y = 0; y < 8; y++)
        for (var x = 0; x < 8; x++)
          ((b[o + y] >> (7 - x)) & 1) | (((b[o + 8 + y] >> (7 - x)) & 1) << 1)
    ];
  }
}
