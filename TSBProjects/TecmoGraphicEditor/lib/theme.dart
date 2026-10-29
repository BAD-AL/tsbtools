// Page styles: Modern or NES, each optionally in a team's colors. The team
// colors and the night/day palette formulas come from research/theme.dart (the
// "sports game menu" look); contrast is enforced on top, and the tests check it
// for every team and style.

enum ThemeStyle { modern, nes }

class TeamTheme {
  final String id; // the ROM's team name
  final String name;
  final int main; // the team's dark color, 0xRRGGBB
  final int accent; // its bright color
  final int onAccent; // text on the accent
  const TeamTheme(this.id, this.name, this.main, this.accent, this.onAccent);
}

const _w = 0xFFFFFF, _k = 0x111111;

/// Display order: by city, like a league table (the same 32 teams as the ROM).
const teamThemes = [
  TeamTheme('cardinals', 'Arizona Cardinals', 0x5C1023, 0xFFB612, _k),
  TeamTheme('falcons', 'Atlanta Falcons', 0x1A1D21, 0xA71930, _w),
  TeamTheme('ravens', 'Baltimore Ravens', 0x241773, 0xC8A24A, _k),
  TeamTheme('bills', 'Buffalo Bills', 0x00338D, 0xC60C30, _w),
  TeamTheme('panthers', 'Carolina Panthers', 0x1A1D21, 0x0085CA, _w),
  TeamTheme('bears', 'Chicago Bears', 0x0B162A, 0xC83803, _w),
  TeamTheme('bengals', 'Cincinnati Bengals', 0x1A1D21, 0xFB4F14, _w),
  TeamTheme('browns', 'Cleveland Browns', 0x311D00, 0xFF3C00, _w),
  TeamTheme('cowboys', 'Dallas Cowboys', 0x041E42, 0xA5ACAF, _k),
  TeamTheme('broncos', 'Denver Broncos', 0x0A2343, 0xFB4F14, _w),
  TeamTheme('lions', 'Detroit Lions', 0x003A63, 0xB0B7BC, _k),
  TeamTheme('packers', 'Green Bay Packers', 0x203731, 0xFFB612, _k),
  TeamTheme('texans', 'Houston Texans', 0x03202F, 0xA71930, _w),
  TeamTheme('colts', 'Indianapolis Colts', 0x002C5F, 0xA2AAAD, _k),
  TeamTheme('jaguars', 'Jacksonville Jaguars', 0x00505E, 0xD7A22A, _k),
  TeamTheme('chiefs', 'Kansas City Chiefs', 0x7A0B1C, 0xFFB81C, _k),
  TeamTheme('dolphins', 'Miami Dolphins', 0x005461, 0xFC4C02, _w),
  TeamTheme('vikings', 'Minnesota Vikings', 0x3B1A5E, 0xFFC62F, _k),
  TeamTheme('patriots', 'New England Patriots', 0x002244, 0xC60C30, _w),
  TeamTheme('saints', 'New Orleans Saints', 0x1A1D21, 0xD3BC8D, _k),
  TeamTheme('giants', 'New York Giants', 0x0B2265, 0xA71930, _w),
  TeamTheme('jets', 'New York Jets', 0x125740, 0xFFFFFF, 0x125740),
  TeamTheme('raiders', 'Oakland Raiders', 0x1A1D21, 0xA5ACAF, _k),
  TeamTheme('eagles', 'Philadelphia Eagles', 0x004C54, 0xA5ACAF, _k),
  TeamTheme('steelers', 'Pittsburgh Steelers', 0x1A1D21, 0xFFB612, _k),
  TeamTheme('chargers', 'San Diego Chargers', 0x002A5E, 0xFFC20E, _k),
  TeamTheme('49ers', 'San Francisco 49ers', 0x6E0000, 0xB3995D, _k),
  TeamTheme('seahawks', 'Seattle Seahawks', 0x002244, 0x69BE28, _k),
  TeamTheme('rams', 'St. Louis Rams', 0x002244, 0xB3995D, _k),
  TeamTheme('buccaneers', 'Tampa Bay Buccaneers', 0x34302B, 0xD50A0A, _w),
  TeamTheme('titans', 'Tennessee Titans', 0x0C2340, 0x4B92DB, _w),
  TeamTheme('redskins', 'Washington Redskins', 0x5A1414, 0xFFB612, _k),
];

/// The NES screen's own blue and gold, used when no team is chosen.
const _classic = TeamTheme('classic', 'Classic', 0x0078F8, 0xF8B800, _k);

/// Edge color of the thick outlines (the "sports menu" ink).
const _edge = 0x050B16;

// ---- color math -------------------------------------------------------

double _channel(int c8) {
  final s = c8 / 255;
  return s <= 0.03928 ? s / 12.92 : _pow((s + 0.055) / 1.055, 2.4);
}

double _pow(double base, double exp) => _exp(exp * _ln(base));

double _ln(double x) {
  // Newton on exp(y) = x.
  var y = 0.0;
  for (var i = 0; i < 40; i++) {
    final e = _exp(y);
    y -= (e - x) / e;
  }
  return y;
}

double _exp(double x) {
  var term = 1.0, sum = 1.0;
  for (var n = 1; n < 60; n++) {
    term *= x / n;
    sum += term;
  }
  return sum;
}

/// WCAG relative luminance of 0xRRGGBB.
double luminance(int c) => 0.2126 * _channel(c >> 16) + 0.7152 * _channel((c >> 8) & 0xFF) + 0.0722 * _channel(c & 0xFF);

/// WCAG contrast ratio (1-21).
double contrast(int a, int b) {
  final la = luminance(a), lb = luminance(b);
  return (la > lb ? la + 0.05 : lb + 0.05) / (la > lb ? lb + 0.05 : la + 0.05);
}

/// [a] moved [t] (0-1) of the way to [b].
int mix(int a, int b, double t) {
  int ch(int shift) {
    final x = (a >> shift) & 0xFF, y = (b >> shift) & 0xFF;
    return (x + (y - x) * t).round().clamp(0, 255);
  }

  return ch(16) << 16 | ch(8) << 8 | ch(0);
}

/// [fg] unchanged if it already has [min] contrast against [bg]; otherwise
/// mixed toward white or black, whichever gets there with the smaller change.
int ensureContrast(int fg, int bg, double min) {
  if (contrast(fg, bg) >= min) return fg;
  int? best;
  for (final target in [0xFFFFFF, 0x000000]) {
    for (var i = 1; i <= 50; i++) {
      final c = mix(fg, target, i / 50);
      if (contrast(c, bg) >= min) {
        if (best == null || i / 50 < _distance(fg, best)) best = c;
        break;
      }
    }
  }
  return best ?? (contrast(0xFFFFFF, bg) >= contrast(0x000000, bg) ? 0xFFFFFF : 0x000000);
}

double _distance(int a, int b) {
  final dr = (a >> 16) - (b >> 16), dg = ((a >> 8) & 0xFF) - ((b >> 8) & 0xFF), db = (a & 0xFF) - (b & 0xFF);
  return (dr * dr + dg * dg + db * db).toDouble() / (3 * 255 * 255);
}

/// White or black, whichever reads better on [bg] (always at least 4.58, the worst case at mid-gray).
int bestInk(int bg) => contrast(0xFFFFFF, bg) >= contrast(0x000000, bg) ? 0xFFFFFF : 0x000000;

/// [panel] tinted toward [accent] by up to [t], backing off until [ink] stays readable on it.
int _tint(int panel, int accent, double t, int ink) {
  var c = mix(panel, accent, t);
  for (var k = t; contrast(ink, c) < 4.5 && k > 0; k -= 0.02) {
    c = mix(panel, accent, k);
  }
  return c;
}

String _hex(int c) => '#${c.toRadixString(16).padLeft(6, '0')}';

String _rgba(int c, double a) => 'rgba(${c >> 16}, ${(c >> 8) & 0xFF}, ${c & 0xFF}, $a)';

// ---- themes -----------------------------------------------------------

/// CSS custom properties for [style] in [team]'s colors. Modern with no team
/// changes nothing (the page's own colors). With a team, [dark] picks the night
/// (dark) or day (light) palette; NES always has its dark screen.
Map<String, String> themeVars(ThemeStyle style, TeamTheme? team, {required bool dark}) {
  if (style == ThemeStyle.modern) {
    return team == null ? {} : _modernVars(team, dark);
  }
  return _nesVars(team ?? _classic);
}

Map<String, String> _modernVars(TeamTheme t, bool night) {
  // Panels are the team's dark color shaded for night or tinted for day.
  final panel = night ? mix(t.main, 0x000000, .30) : mix(t.main, 0xFFFFFF, .86);
  final panelHi = night ? mix(t.main, 0xFFFFFF, .08) : 0xFFFFFF;
  final headBase = night ? mix(t.main, 0x000000, .45) : t.main;
  var head = headBase;
  for (var i = 1; contrast(0xFFFFFF, head) < 4.5 && i <= 50; i++) {
    head = mix(headBase, 0x000000, i / 50);
  }
  final text = night ? 0xF2F5FA : 0x0D1726;
  final field = night ? 0x071224 : 0xFFFFFF;
  int safe(int c) => ensureContrast(c, panel, 4.5);
  final accentInk = contrast(t.onAccent, t.accent) >= 4.5 ? t.onAccent : bestInk(t.accent);
  final ok = safe(night ? 0x7BE495 : 0x157A34);
  return {
    '--accent': _hex(t.accent),
    '--accent-ink': _hex(accentInk),
    '--panel': _hex(panel),
    '--panel-hi': _hex(panelHi),
    '--panel-head': _hex(head),
    '--ink': _hex(text),
    '--muted': _hex(safe(night ? 0xB9C3D3 : 0x3C4A5E)),
    '--line': _hex(ensureContrast(mix(panel, text, .30), panel, 1.6)),
    '--edge': _hex(_edge),
    '--field': _hex(field),
    '--headline': _hex(safe(night ? t.accent : t.main)),
    '--sel': _hex(_tint(panelHi, t.accent, night ? .30 : .20, text)),
    '--copied': _hex(_tint(panelHi, ok, .22, text)),
    '--copied-line': _hex(ok),
    '--ok': _hex(ok),
    '--warn': _hex(safe(night ? 0xFFD166 : 0x8A5A00)),
    '--err': _hex(safe(night ? 0xFF7B72 : 0xB3261E)),
    '--canvas': '#000000',
    '--stripe': _hex(t.accent),
    '--page-ink': _hex(text),
    // A floodlit night sky, or a day sky, over turf; halftone dots in the accent.
    '--page': night
        ? 'radial-gradient(circle at 4% -2%, rgba(255, 253, 230, .5), transparent 30%), '
            'radial-gradient(circle at 96% -2%, rgba(255, 253, 230, .5), transparent 30%), '
            'linear-gradient(to bottom, #071733 0%, #0d2a55 62%, #123c26 100%)'
        : 'radial-gradient(circle at 85% -4%, rgba(255, 255, 255, .8), transparent 35%), '
            'linear-gradient(to bottom, #6fb7ff 0%, #a8d6ff 62%, #5fae5a 100%)',
    '--dots': _rgba(t.accent, .45),
  };
}

Map<String, String> _nesVars(TeamTheme t) {
  // The screen is the team's dark color, pushed darker until white text is easy to
  // read on the slightly lighter panels drawn over it; the accent is the highlight.
  var bg = t.main, panel = mix(bg, 0xFFFFFF, 0.10);
  for (var i = 1; contrast(0xFFFFFF, panel) < 7.2 && i <= 50; i++) {
    bg = mix(t.main, 0x000000, i / 50);
    panel = mix(bg, 0xFFFFFF, 0.10);
  }
  final highlight = ensureContrast(t.accent, panel, 4.5);
  return {
    '--bg': _hex(bg),
    '--panel': _hex(panel),
    '--ink': '#ffffff',
    '--muted': _hex(ensureContrast(mix(0xFFFFFF, bg, 0.4), panel, 4.5)),
    '--line': _hex(ensureContrast(mix(0xFFFFFF, panel, 0.55), panel, 3.0)),
    '--accent': _hex(highlight),
    '--accent-ink': _hex(bestInk(highlight)),
    '--sel': _hex(_tint(panel, highlight, 0.25, 0xFFFFFF)),
    '--stripe': _hex(highlight),
    '--copied': '#1e3323',
    '--copied-line': '#6fcf7a',
    '--warn': '#f0b35a',
    '--err': '#ff8a80',
    '--ok': '#6fcf7a',
    '--canvas': '#000000',
  };
}

String _rule(Map<String, String> vars) => [for (final e in vars.entries) '${e.key}:${e.value}'].join(';');

/// A stylesheet for the page: scoped to `:root[data-style=...]` so it overrides the page
/// defaults only while that style is selected; modern has a light rule and a more specific
/// `[data-mode="dark"]` one (the page sets data-mode, so Auto/Light/Dark all work).
String themeCss(ThemeStyle style, TeamTheme? team) {
  if (style == ThemeStyle.nes) {
    return ':root[data-style="nes"]{${_rule(themeVars(style, team, dark: false))}}';
  }
  if (team == null) return '';
  return ':root[data-style="modern"]{${_rule(themeVars(style, team, dark: false))}}'
      ':root[data-style="modern"][data-mode="dark"]{${_rule(themeVars(style, team, dark: true))}}';
}
