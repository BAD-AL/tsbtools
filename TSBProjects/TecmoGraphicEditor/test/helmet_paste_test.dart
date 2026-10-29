import 'dart:io';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/helmet_paste.dart';
import 'package:tecmo_graphic_editor/helmet_renderer.dart';
import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:test/test.dart';

LargeHelmetReader load(String name) => LargeHelmetReader(NesRom(File('ROMS/$name').readAsBytesSync()));

Uint8List render(LargeHelmetReader r, int team, {bool shell = true}) =>
    renderHelmetIndexes(r.rom, r.read(team), shell: shell);

int idx(LargeHelmetReader r, String team) => r.teams.indexOf(team);

/// Pastes and checks: target looks like the source, nothing else changed.
PastePlan pasteAndCheck(LargeHelmetReader src, int s, LargeHelmetReader dst, int t,
    {bool logo = true, bool colors = true, bool shell = true}) {
  final before = [for (var i = 0; i < dst.teams.length; i++) render(dst, i)];
  final plan = planPaste(dst, t, copyHelmet(src, s), logo: logo, colors: colors, shell: shell);
  if (!plan.ok) return plan;
  final after = LargeHelmetReader(NesRom(plan.result!));
  expect(after.is32Team, dst.is32Team);

  if (logo && colors) {
    // Shell only matches when it was copied (and a matching shell exists).
    final withShell = shell && plan.shellCopied;
    expect(render(after, t, shell: withShell), render(src, s, shell: withShell),
        reason: '${src.teams[s]} -> ${dst.teams[t]}: target should look like the source');
  }
  for (var i = 0; i < dst.teams.length; i++) {
    if (i == t || plan.alsoAffects.contains(dst.teams[i])) continue;
    expect(render(after, i), before[i], reason: '${src.teams[s]} -> ${dst.teams[t]} changed ${dst.teams[i]}');
  }
  return plan;
}

void main() {
  final orig = load('TSPRBOWL.nes');
  final rom32 = load('TSB 2007-32-105.nes');

  test('colors only: target gets the source palette, logo stays', () {
    final plan = pasteAndCheck(rom32, idx(rom32, 'jaguars'), orig, idx(orig, 'oilers'), logo: false, shell: false);
    expect(plan.ok, isTrue);
    final after = LargeHelmetReader(NesRom(plan.result!));
    final t = idx(orig, 'oilers');
    expect(after.read(t).paletteRam, rom32.read(idx(rom32, 'jaguars')).paletteRam);
    expect(after.read(t).sprites.map((s) => '${s.tile}@${s.x},${s.y}'),
        orig.read(t).sprites.map((s) => '${s.tile}@${s.x},${s.y}'));
  });

  test('32-team -> 32-team: Jaguars over Bills', () {
    expect(pasteAndCheck(rom32, idx(rom32, 'jaguars'), rom32, idx(rom32, 'bills')).ok, isTrue);
  });

  test('original -> 32-team: 1991 Oilers over Titans', () {
    expect(pasteAndCheck(orig, idx(orig, 'oilers'), rom32, idx(rom32, 'titans')).ok, isTrue);
  });

  test('32-team -> original: 2007 Bills over 1991 Bills', () {
    expect(pasteAndCheck(rom32, idx(rom32, 'bills'), orig, idx(orig, 'bills')).ok, isTrue);
  });

  test('hook teams (32-team indexes 30-33) can be source and target', () {
    expect(pasteAndCheck(rom32, idx(rom32, 'seahawks'), rom32, idx(rom32, 'cardinals')).ok, isTrue);
    expect(pasteAndCheck(orig, idx(orig, 'rams'), rom32, idx(rom32, '49ers')).ok, isTrue);
  });

  test('a refused paste explains itself and changes nothing', () {
    // Try every source onto the original Colts; any refusal must carry a reason.
    for (final src in [orig, rom32]) {
      for (var s = 0; s < src.teams.length; s++) {
        final plan = planPaste(orig, idx(orig, 'colts'), copyHelmet(src, s));
        if (!plan.ok) {
          expect(plan.errors, isNotEmpty);
          expect(plan.result, isNull);
        }
      }
    }
  });

  test('every source onto three targets per ROM pair', () {
    var ok = 0, refused = 0;
    for (final src in [orig, rom32]) {
      for (final dst in [orig, rom32]) {
        for (var s = 0; s < src.teams.length; s++) {
          for (final t in [s % dst.teams.length, (s + 7) % dst.teams.length, dst.teams.length - 1 - s % dst.teams.length]) {
            final plan = pasteAndCheck(src, s, dst, t);
            plan.ok ? ok++ : refused++;
          }
        }
      }
    }
    // The 32-team ROM has empty CHR banks, so it should never refuse.
    print('pastes ok: $ok, refused: $refused');
    expect(ok, greaterThan(refused));
  });

  test('pastes chain: a second paste builds on the first', () {
    final p1 = planPaste(rom32, idx(rom32, 'bills'), copyHelmet(orig, idx(orig, 'oilers')));
    expect(p1.ok, isTrue);
    final mid = LargeHelmetReader(NesRom(p1.result!));
    final p2 = pasteAndCheck(orig, idx(orig, 'bengals'), mid, idx(mid, 'dolphins'));
    expect(p2.ok, isTrue);
    final end = LargeHelmetReader(NesRom(p2.result!));
    expect(render(end, idx(end, 'bills'), shell: false), render(orig, idx(orig, 'oilers'), shell: false));
  });

  test('32-team ROM never refuses a logo', () {
    for (final src in [orig, rom32]) {
      for (var s = 0; s < src.teams.length; s++) {
        final plan = planPaste(rom32, idx(rom32, 'jets'), copyHelmet(src, s));
        expect(plan.ok, isTrue, reason: '${src.teams[s]}: ${plan.errors}');
      }
    }
  });
}
