// The look (style, team colors, light/dark) and the help dialog. Part of main.dart.
part of 'main.dart';

const _styleKey = 'tsb.style', _teamKey = 'tsb.team', _cssKey = 'tsb.css', _modeKey = 'tsb.mode', _skinKey = 'tsb.skin';

String? _stored(String key) {
  try {
    return web.window.localStorage.getItem(key);
  } catch (_) {
    return null; // storage can be blocked (private windows, file:// in some browsers)
  }
}

void _store(String key, String value) {
  try {
    web.window.localStorage.setItem(key, value);
  } catch (_) {}
}

void setUpTheme() {
  final styleSel = byId<web.HTMLSelectElement>('styleSel');
  final teamSel = byId<web.HTMLSelectElement>('teamSel');
  final modeSel = byId<web.HTMLSelectElement>('modeSel');
  teamSel.append(web.document.createElement('option') as web.HTMLOptionElement
    ..value = ''
    ..textContent = 'Default');
  for (final t in teamThemes) {
    teamSel.append(web.document.createElement('option') as web.HTMLOptionElement
      ..value = t.id
      ..textContent = t.name);
  }
  styleSel.value = _stored(_styleKey) == 'nes' ? 'nes' : 'modern';
  final saved = _stored(_teamKey) ?? '';
  teamSel.value = teamThemes.any((t) => t.id == saved) ? saved : '';
  final mode = _stored(_modeKey);
  modeSel.value = mode == 'light' || mode == 'dark' ? mode! : 'auto';
  styleSel.onchange = ((web.Event _) => applyTheme()).toJS;
  teamSel.onchange = ((web.Event _) => applyTheme()).toJS;
  modeSel.onchange = ((web.Event _) => applyTheme()).toJS;
  // Auto follows the computer's light/dark setting, including when it changes while the page is open.
  web.window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', ((web.Event _) => applyTheme()).toJS);
  applyTheme();
}

void applyTheme() {
  final style = byId<web.HTMLSelectElement>('styleSel').value == 'nes' ? ThemeStyle.nes : ThemeStyle.modern;
  final id = byId<web.HTMLSelectElement>('teamSel').value;
  final team = teamThemes.where((t) => t.id == id).firstOrNull;
  final css = themeCss(style, team);
  final root = web.document.documentElement!;
  root.setAttribute('data-style', style == ThemeStyle.nes ? 'nes' : 'modern');
  // Modern with a team gets the full "sports game menu" skin; plain modern and NES don't.
  final skin = style == ThemeStyle.modern && team != null;
  if (skin) {
    root.setAttribute('data-skin', 'team');
  } else {
    root.removeAttribute('data-skin');
  }

  // Light / dark: the chosen mode, or the computer's setting for Auto. NES is always dark.
  final modeSel = byId<web.HTMLSelectElement>('modeSel');
  final osDark = web.window.matchMedia('(prefers-color-scheme: dark)').matches;
  final dark = modeSel.value == 'dark' || (modeSel.value == 'auto' && osDark);
  root.setAttribute('data-mode', dark ? 'dark' : 'light');
  modeSel
    ..disabled = style == ThemeStyle.nes
    ..title = style == ThemeStyle.nes ? 'The NES style is always dark' : 'Light, dark, or follow your computer';

  byId<web.HTMLElement>('themeStyle').textContent = css;
  _store(_styleKey, style == ThemeStyle.nes ? 'nes' : 'modern');
  _store(_teamKey, id);
  _store(_modeKey, modeSel.value);
  _store(_skinKey, skin ? 'team' : '');
  _store(_cssKey, css); // read by the script in index.html so the next visit starts in this look

  final chips = byId<web.HTMLElement>('teamChips')..innerHTML = ''.toJS;
  for (final c in team == null ? const <int>[] : [team.main, team.accent]) {
    final chip = web.document.createElement('i');
    chip.setAttribute('style', 'background:#${c.toRadixString(16).padLeft(6, '0')}');
    chips.append(chip);
  }
}

// ---- help ----

void setUpHelp() {
  final dlg = byId<web.HTMLDialogElement>('helpDlg');
  onClick('helpBtn', () => dlg.showModal());
  onClick('helpClose', () => dlg.close());
  // A click on the dimmed area outside the box closes it.
  dlg.addEventListener('click', ((web.MouseEvent e) {
    if (e.target == dlg) dlg.close();
  }).toJS);
}

/// The ? key opens the help (unless the user is typing in a field).
void helpKey(web.KeyboardEvent e) {
  final typing = e.target?.isA<web.HTMLInputElement>() ?? false;
  if (e.key == '?' && !typing && !e.ctrlKey && !e.metaKey && !e.altKey) {
    final dlg = byId<web.HTMLDialogElement>('helpDlg');
    if (!dlg.open) dlg.showModal();
  }
}
