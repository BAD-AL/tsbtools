// The style / team color scheme choices: every combination must stay readable.
import 'dart:io';

import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:tecmo_graphic_editor/theme.dart';
import 'package:test/test.dart';

int hexVar(Map<String, String> vars, String name) => int.parse(vars[name]!.substring(1), radix: 16);

void main() {
  test('there is a color scheme for each of the 32 ROM teams', () {
    final rom = NesRom(File('ROMS/TSB 2007-32-105.nes').readAsBytesSync());
    final names = LargeHelmetReader(rom).teams;
    expect(teamThemes.map((t) => t.id).toSet(), names.toSet());
    expect(teamThemes.length, 32);
    expect(teamThemes.every((t) => t.name.isNotEmpty), isTrue);
  });

  test('contrast: black on white is 21, a color against itself is 1', () {
    expect(contrast(0x000000, 0xFFFFFF), closeTo(21, 0.01));
    expect(contrast(0x1F5FBF, 0x1F5FBF), closeTo(1, 0.0001));
  });

  test('ensureContrast moves a color just far enough', () {
    final c = ensureContrast(0x777777, 0x1F1F22, 4.5);
    expect(contrast(c, 0x1F1F22), greaterThanOrEqualTo(4.5));
    // already fine: unchanged
    expect(ensureContrast(0xFFFFFF, 0x000000, 4.5), 0xFFFFFF);
  });

  test('modern with a team: every surface and text color is readable, day and night', () {
    for (final t in teamThemes) {
      for (final dark in [false, true]) {
        final v = themeVars(ThemeStyle.modern, t, dark: dark);
        final why = '${t.id} ${dark ? 'night' : 'day'}';
        final panel = hexVar(v, '--panel'), hi = hexVar(v, '--panel-hi'), head = hexVar(v, '--panel-head');
        final ink = hexVar(v, '--ink'), accent = hexVar(v, '--accent');
        expect(contrast(ink, panel), greaterThanOrEqualTo(4.5), reason: '$why text on panel');
        expect(contrast(ink, hi), greaterThanOrEqualTo(4.5), reason: '$why text on cards');
        expect(contrast(ink, hexVar(v, '--field')), greaterThanOrEqualTo(4.5), reason: '$why text in fields');
        expect(contrast(ink, hexVar(v, '--sel')), greaterThanOrEqualTo(4.5), reason: '$why text on the selection');
        expect(contrast(hexVar(v, '--muted'), panel), greaterThanOrEqualTo(4.5), reason: '$why muted text');
        expect(contrast(0xFFFFFF, head), greaterThanOrEqualTo(4.5), reason: '$why white title on its bar');
        expect(contrast(hexVar(v, '--accent-ink'), accent), greaterThanOrEqualTo(4.5), reason: '$why button text');
        expect(contrast(hexVar(v, '--headline'), panel), greaterThanOrEqualTo(4.5), reason: '$why headings');
        for (final c in ['--ok', '--warn', '--err']) {
          expect(contrast(hexVar(v, c), panel), greaterThanOrEqualTo(4.5), reason: '$why $c on panel');
        }
        expect(contrast(ink, hexVar(v, '--copied')), greaterThanOrEqualTo(4.5), reason: '$why text on a copied card');
      }
    }
  });

  test('modern with a team also sets the stadium background and edges', () {
    for (final dark in [false, true]) {
      final v = themeVars(ThemeStyle.modern, teamThemes.first, dark: dark);
      expect(v['--page'], contains('linear-gradient'));
      expect(v['--dots'], startsWith('rgba('));
      expect(v['--edge'], '#050b16');
      expect(v['--page-ink'], v['--ink']);
    }
    // night and day use different skies
    expect(themeVars(ThemeStyle.modern, teamThemes.first, dark: true)['--page'],
        isNot(themeVars(ThemeStyle.modern, teamThemes.first, dark: false)['--page']));
  });

  test('modern with no team changes nothing', () {
    expect(themeVars(ThemeStyle.modern, null, dark: false), isEmpty);
    expect(themeVars(ThemeStyle.modern, null, dark: true), isEmpty);
  });

  test('night and day panels are different tints of the team color', () {
    final bills = teamThemes.firstWhere((t) => t.id == 'bills');
    final day = themeVars(ThemeStyle.modern, bills, dark: false), night = themeVars(ThemeStyle.modern, bills, dark: true);
    expect(luminance(hexVar(day, '--panel')), greaterThan(luminance(hexVar(night, '--panel'))));
    // teams differ
    final jets = teamThemes.firstWhere((t) => t.id == 'jets');
    expect(themeVars(ThemeStyle.modern, jets, dark: true)['--panel'], isNot(night['--panel']));
  });

  test('nes: every team (and the classic default) is a dark screen with readable text', () {
    for (final t in [null, ...teamThemes]) {
      final v = themeVars(ThemeStyle.nes, t, dark: false);
      final id = t?.id ?? 'default';
      final bg = hexVar(v, '--bg'), panel = hexVar(v, '--panel');
      expect(contrast(hexVar(v, '--ink'), bg), greaterThanOrEqualTo(7.0), reason: '$id text on screen');
      expect(contrast(hexVar(v, '--ink'), panel), greaterThanOrEqualTo(7.0), reason: '$id text on panel');
      expect(contrast(hexVar(v, '--ink'), hexVar(v, '--sel')), greaterThanOrEqualTo(4.5), reason: '$id text on the selection');
      expect(contrast(hexVar(v, '--muted'), panel), greaterThanOrEqualTo(4.5), reason: '$id muted text');
      expect(contrast(hexVar(v, '--accent'), panel), greaterThanOrEqualTo(4.5), reason: '$id accent');
      expect(contrast(hexVar(v, '--accent-ink'), hexVar(v, '--accent')), greaterThanOrEqualTo(4.5), reason: '$id button text');
      expect(contrast(hexVar(v, '--line'), panel), greaterThanOrEqualTo(3.0), reason: '$id borders');
    }
  });

  test('nes: the same input always gives the same colors, and a team changes them', () {
    final bills = teamThemes.firstWhere((t) => t.id == 'bills'), jets = teamThemes.firstWhere((t) => t.id == 'jets');
    expect(themeVars(ThemeStyle.nes, bills, dark: false), themeVars(ThemeStyle.nes, bills, dark: true));
    expect(themeVars(ThemeStyle.nes, bills, dark: false)['--bg'], isNot(themeVars(ThemeStyle.nes, jets, dark: false)['--bg']));
  });

  test('plain colors are written as #rrggbb', () {
    for (final v in themeVars(ThemeStyle.nes, teamThemes.first, dark: false).values) {
      expect(v, matches(RegExp(r'^#[0-9a-f]{6}$')));
    }
    final modern = themeVars(ThemeStyle.modern, teamThemes.first, dark: true);
    for (final e in modern.entries) {
      if (e.key == '--page' || e.key == '--dots') continue;
      expect(e.value, matches(RegExp(r'^#[0-9a-f]{6}$')), reason: e.key);
    }
  });

  test('the generated CSS scopes each style, and modern switches day/night with data-mode', () {
    final modern = themeCss(ThemeStyle.modern, teamThemes.first);
    expect(modern, contains(':root[data-style="modern"]{'));
    expect(modern, contains(':root[data-style="modern"][data-mode="dark"]{'));
    expect(modern, isNot(contains('prefers-color-scheme'))); // the page decides, so Light/Dark can override the OS
    final nes = themeCss(ThemeStyle.nes, teamThemes.first);
    expect(nes, contains(':root[data-style="nes"]'));
    expect(nes, isNot(contains('data-mode')));
    expect(themeCss(ThemeStyle.modern, null), isEmpty);
  });
}
