// Copy a large helmet out of one ROM and paste it over a team in another.
// The copy carries pixels, not tile numbers, so it can be re-packed into any
// supported ROM; see research/NES_graphics_map.md for the formats.
import 'dart:typed_data';

import 'large_helmet.dart';
import 'nes_rom.dart';

/// A self-contained large helmet: colors, shell look, logo layout and the
/// pixel data of every logo tile.
class HelmetCopy {
  final String team;
  final bool from32Team;
  final Uint8List palette; // the 8 team palette bytes
  final Uint8List shellSignature; // shell tile pixels + attributes, for matching
  final int baseY, baseX; // logo header `f8 Y X`
  /// Logo data stream, `ff`-terminated, tile bytes as in the source ROM.
  final Uint8List stream;
  /// Source tile byte -> 16 bytes of 2bpp CHR data.
  final Map<int, Uint8List> tiles;

  HelmetCopy(this.team, this.from32Team, this.palette, this.shellSignature, this.baseY, this.baseX,
      this.stream, this.tiles);
}

class PastePlan {
  final List<String> errors;
  final List<String> notes;
  /// Other teams whose helmet changes too (they share the target's logo).
  final List<String> alsoAffects;
  final bool shellCopied;
  /// The whole target ROM with the paste applied; null when refused.
  final Uint8List? result;

  PastePlan(this.errors, this.notes, this.alsoAffects, this.shellCopied, this.result);

  bool get ok => result != null;
}

// Logo headers we can rewrite: f6 A ff ff ff fc n f8 Y X P1 P2 ...
bool _isLogoHeader(NesRom rom, int at) =>
    rom.prg(at) == 0xF6 && rom.prg(at + 5) == 0xFC && rom.prg(at + 7) == 0xF8 && (rom.prg(at + 10) & 0xE0) == 0x80;

/// One parsed sprite group.
class _Group {
  final int header; // PRG offset
  final int bankA; // CHR bank for tile bytes $00-$3F
  final int data; // PRG offset of the stream
  final int length; // stream bytes including the ff
  final Set<(int, int)> refs; // (bank, index) tiles used
  _Group(this.header, this.bankA, this.data, this.length, this.refs);
}

/// Parses the logo/facemask group at CPU [ptr]. [bankB] is the bank for
/// tile bytes $40-$7F (whatever the facemask selects).
_Group _parseGroup(NesRom rom, int ptr, int bankA, int bankB) {
  var p = LargeHelmetReader.headerOffset(ptr);
  final header = p;
  int? data;
  while (data == null) {
    final cmd = rom.prg(p++);
    if (cmd == 0xF6) {
      if (rom.prg(p) != 0xFF) bankA = rom.prg(p);
      p += 4;
    } else if (cmd == 0xFC) {
      p++;
    } else if (cmd == 0xF8) {
      p += 2;
    } else if (cmd >= 0x80 && cmd < 0xF0) {
      data = LargeHelmetReader.dataOffset(cmd, rom.prg(p));
    } else {
      throw FormatException('Unknown sprite group command \$${cmd.toRadixString(16)}');
    }
  }
  final refs = <(int, int)>{};
  var q = data;
  while (true) {
    final b = rom.prg(q++);
    if (b == 0xFF) break;
    if (b == 0xFC) {
      q++;
    } else if (b < 0x80) {
      final t = rom.prg(q++) & 0x7F;
      refs.add(t < 0x40 ? (bankA, t) : (bankB, t & 0x3F));
    }
  }
  return _Group(header, bankA, data, q - data, refs);
}

const _defaultBankA = 0x70, _facemaskBank = 0x73;

Uint8List _tileBytes(NesRom rom, int bank, int index) {
  final o = 16 + rom.prgSize + bank * 0x400 + index * 16;
  return Uint8List.fromList(rom.bytes.sublist(o, o + 16));
}

Uint8List _shellSignature(LargeHelmetReader r, int team) {
  final h = r.read(team);
  final out = BytesBuilder();
  for (final t in h.shellTiles) {
    out.add(_tileBytes(r.rom, h.bgBanks[t >> 6], t & 0x3F));
  }
  out.add(h.shellPalettes);
  return out.toBytes();
}

bool _sameBytes(List<int> a, List<int> b) {
  if (a.length != b.length) return false;
  for (var i = 0; i < a.length; i++) {
    if (a[i] != b[i]) return false;
  }
  return true;
}

HelmetCopy copyHelmet(LargeHelmetReader r, int team) {
  final rom = r.rom;
  final pal = Uint8List.fromList([for (var k = 0; k < 8; k++) rom.prg(r.paletteOffset(team) + k)]);
  final g = _parseGroup(rom, r.logoHeaderPointer(team), _defaultBankA, _facemaskBank);
  final tiles = <int, Uint8List>{};
  var q = g.data;
  while (true) {
    final b = rom.prg(q++);
    if (b == 0xFF) break;
    if (b == 0xFC) {
      q++;
    } else if (b < 0x80) {
      final t = rom.prg(q++) & 0x7F;
      tiles[t] = t < 0x40 ? _tileBytes(rom, g.bankA, t) : _tileBytes(rom, _facemaskBank, t & 0x3F);
    }
  }
  return HelmetCopy(
    r.teams[team],
    r.is32Team,
    pal,
    _shellSignature(r, team),
    rom.prg(g.header + 8),
    rom.prg(g.header + 9),
    Uint8List.fromList(rom.bytes.sublist(16 + g.data, 16 + g.data + g.length)),
    tiles,
  );
}

/// Works out (and applies, on a copy of the ROM) pasting [src] over [team].
PastePlan planPaste(LargeHelmetReader target, int team, HelmetCopy src,
    {bool logo = true, bool colors = true, bool shell = true}) {
  final errors = <String>[], notes = <String>[], also = <String>[];
  final rom = NesRom(Uint8List.fromList(target.rom.bytes));
  final r = LargeHelmetReader(rom);
  final name = r.teams[team];
  var shellCopied = false;

  void setPrg(int offset, int value) => rom.bytes[16 + offset] = value;

  if (colors) {
    final at = r.paletteOffset(team);
    for (var k = 0; k < 8; k++) {
      setPrg(at + k, src.palette[k]);
    }
    notes.add('Colors: copied (shell, facemask, logo).');
  }

  if (shell) {
    final current = rom.prg(r.shellIdOffset(team));
    int? match;
    for (final id in [current, 0x40, 0x41, 0x42, 0x43]) {
      final who = [for (var i = 0; i < r.teams.length; i++) if (rom.prg(r.shellIdOffset(i)) == id) i];
      if (who.isEmpty) continue; // only shells some team uses are known to be valid
      if (_sameBytes(_shellSignature(r, who.first), src.shellSignature)) {
        match = id;
        break;
      }
    }
    if (match == null) {
      notes.add("Shell: ${src.team}'s shell isn't in this ROM; $name keeps its own.");
    } else {
      setPrg(r.shellIdOffset(team), match);
      shellCopied = true;
      notes.add(match == current
          ? 'Shell: same as before (\$${match.toRadixString(16).toUpperCase()}).'
          : 'Shell: switched to \$${match.toRadixString(16).toUpperCase()}.');
    }
  }

  if (logo) {
    final err = _pasteLogo(r, team, src, notes, also);
    if (err != null) errors.add(err);
  }

  return errors.isEmpty
      ? PastePlan(errors, notes, also, shellCopied, rom.bytes)
      : PastePlan(errors, notes, also, false, null);
}

/// Writes the logo into [r]'s ROM; returns an error message instead when it can't fit.
String? _pasteLogo(LargeHelmetReader r, int team, HelmetCopy src, List<String> notes, List<String> also) {
  final rom = r.rom;
  void setPrg(int offset, int value) => rom.bytes[16 + offset] = value;

  // Every helmet sprite group in the ROM, by header pointer.
  final ptrs = <int, List<int>>{};
  for (var i = 0; i < r.teams.length; i++) {
    ptrs.putIfAbsent(r.logoHeaderPointer(i), () => []).add(i);
  }
  final groups = {for (final p in ptrs.keys) p: _parseGroup(rom, p, _defaultBankA, _facemaskBank)};
  final mask = _parseGroup(rom, r.facemaskHeaderPointer, _defaultBankA, _facemaskBank);

  final targetPtr = r.logoHeaderPointer(team);
  final target = groups[targetPtr]!;
  if (!_isLogoHeader(rom, target.header)) {
    return "${r.teams[team]}'s logo header has an unexpected layout; not touching it.";
  }
  for (final i in ptrs[targetPtr]!) {
    if (i != team) also.add(r.teams[i]);
  }

  // Tiles: who else uses each (bank, index)?
  final usedByOthers = <(int, int)>{...mask.refs};
  for (final e in groups.entries) {
    if (e.key != targetPtr) usedByOthers.addAll(e.value.refs);
  }
  final helmetBanks = {for (final g in [...groups.values, mask]) ...g.refs.map((t) => t.$1), for (final g in groups.values) g.bankA};

  // Unique source tiles by content.
  final unique = <String, Uint8List>{};
  for (final t in src.tiles.values) {
    unique[t.join(',')] = t;
  }

  // Try the target's own bank, then (32-team) an empty expansion bank.
  final candidates = <int>[target.bankA];
  if (r.is32Team) {
    for (var b = 0x80; b < rom.chrSize ~/ 0x400; b++) {
      if (helmetBanks.contains(b)) continue;
      final o = 16 + rom.prgSize + b * 0x400;
      if (rom.bytes.sublist(o, o + 0x400).every((v) => v == 0)) {
        candidates.add(b);
        break;
      }
    }
  }

  Map<String, int>? placement;
  int? bank;
  var neededFree = 0, haveFree = 0;
  for (final b in candidates) {
    final keep = {for (final t in usedByOthers) if (t.$1 == b) t.$2};
    final emptyBank = b != target.bankA;
    final free = [for (var k = 0; k < 64; k++) if (!keep.contains(k) && (emptyBank || target.refs.contains((b, k)))) k];
    final place = <String, int>{};
    final writable = List.of(free);
    // Reuse identical tiles already in the bank where possible.
    for (final e in unique.entries) {
      for (var k = 0; k < 64; k++) {
        if ((keep.contains(k) || writable.contains(k)) && _sameBytes(_tileBytes(rom, b, k), e.value)) {
          place[e.key] = k;
          writable.remove(k);
          break;
        }
      }
    }
    final rest = unique.keys.where((k) => !place.containsKey(k)).toList();
    if (b == target.bankA) {
      neededFree = rest.length;
      haveFree = writable.length;
    }
    if (rest.length > writable.length) continue;
    for (final k in rest) {
      place[k] = writable.removeAt(0);
    }
    placement = place;
    bank = b;
    break;
  }
  if (placement == null || bank == null) {
    final sharing = [
      for (final e in groups.entries)
        if (e.key != targetPtr && e.value.bankA == target.bankA) ...ptrs[e.key]!.map((i) => r.teams[i])
    ];
    return 'Logo needs $neededFree new tile${neededFree == 1 ? '' : 's'} but only $haveFree are free in CHR bank '
        '\$${target.bankA.toRadixString(16).toUpperCase()} (shared with ${sharing.join(', ')}).';
  }

  // New stream: source layout with tile bytes renumbered into [bank].
  final stream = Uint8List.fromList(src.stream);
  for (var q = 0; stream[q] != 0xFF;) {
    final b = stream[q++];
    if (b == 0xFC) {
      q++;
    } else if (b < 0x80) {
      stream[q] = placement[src.tiles[stream[q] & 0x7F]!.join(',')]!;
      q++;
    }
  }

  // Data: in place if it fits and nobody else points at it, else free space.
  final sharedData = groups.entries.any((e) => e.key != targetPtr && e.value.data == target.data);
  int dataAt;
  if (!sharedData && stream.length <= target.length) {
    dataAt = target.data;
  } else {
    final found = _findFree(rom, stream.length, exclude: sharedData ? null : (target.data, target.length));
    if (found == null) return 'No free space left for the logo layout (${stream.length} bytes).';
    dataAt = found;
  }
  if (!sharedData) {
    for (var k = 0; k < target.length; k++) {
      setPrg(target.data + k, 0xFF); // give the old space back
    }
  }
  for (var k = 0; k < stream.length; k++) {
    setPrg(dataAt + k, stream[k]);
  }

  // Tiles.
  final chr = 16 + rom.prgSize + bank * 0x400;
  for (final e in placement.entries) {
    rom.bytes.setRange(chr + e.value * 16, chr + e.value * 16 + 16, unique[e.key]!);
  }

  // Header: bank, base position, data pointer (logo number and tail kept).
  final h = target.header;
  setPrg(h + 1, bank);
  setPrg(h + 2, 0xFF);
  setPrg(h + 3, 0xFF);
  setPrg(h + 4, 0xFF);
  setPrg(h + 8, src.baseY);
  setPrg(h + 9, src.baseX);
  final ptr = 0x8000 + dataAt - 0x10000;
  setPrg(h + 10, ptr >> 8);
  setPrg(h + 11, ptr & 0xFF);

  notes.add('Logo: ${unique.length} tiles in CHR bank \$${bank.toRadixString(16).toUpperCase()}'
      '${bank == target.bankA ? '' : ' (empty expansion bank)'}, ${stream.length}-byte layout '
      '${dataAt == target.data ? 'in place' : 'moved to free space'}.');
  return null;
}

/// A run of 0xFF in PRG $10000-$11FFF long enough for [length] bytes. The
/// first FF of a run may be the previous stream's terminator, so it's skipped.
/// [exclude] is the target's own old stream, which is about to be freed.
int? _findFree(NesRom rom, int length, {(int, int)? exclude}) {
  bool isFree(int x) => rom.prg(x) == 0xFF || (exclude != null && x >= exclude.$1 && x < exclude.$1 + exclude.$2);
  var start = -1;
  for (var x = 0x10000; x < 0x12000; x++) {
    if (isFree(x)) {
      if (start < 0) start = x + 1;
      if (x - start + 1 >= length) return start;
    } else {
      start = -1;
    }
  }
  return null;
}
