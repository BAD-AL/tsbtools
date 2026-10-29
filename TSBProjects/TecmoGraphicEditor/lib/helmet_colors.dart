// The 8 per-team large-helmet colors (PRG table at $BD69, or $BFE0 for the
// 32-team hack's last four teams). Only the large-helmet routine at $A99E
// reads them, so editing one team's colors changes nothing else.
import 'dart:typed_data';

import 'large_helmet.dart';

/// What each of the 8 color bytes paints, in table order.
const helmetColorLabels = ['Shell 1', 'Shell 2', 'Shell 3', 'Facemask 1', 'Facemask 2', 'Logo 1', 'Logo 2', 'Logo 3'];

/// The team's 8 helmet colors (NES palette indexes 0-63).
List<int> helmetColors(LargeHelmetReader r, int team) {
  final at = r.paletteOffset(team);
  return [for (var k = 0; k < 8; k++) r.rom.prg(at + k)];
}

/// A copy of the ROM with the team's 8 helmet colors replaced.
Uint8List withHelmetColors(LargeHelmetReader r, int team, List<int> colors) {
  if (colors.length != 8) throw ArgumentError('Expected 8 colors, got ${colors.length}');
  if (colors.any((c) => c < 0 || c > 0x3F)) throw ArgumentError('Colors must be 0-63');
  final out = Uint8List.fromList(r.rom.bytes);
  final at = 16 + r.paletteOffset(team);
  out.setRange(at, at + 8, colors);
  return out;
}
