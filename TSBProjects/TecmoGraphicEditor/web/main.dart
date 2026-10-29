import 'dart:convert';
import 'dart:js_interop';
import 'dart:js_interop_unsafe';
import 'dart:typed_data';

import 'package:tecmo_graphic_editor/helmet_colors.dart';
import 'package:tecmo_graphic_editor/helmet_paste.dart';
import 'package:tecmo_graphic_editor/helmet_renderer.dart';
import 'package:tecmo_graphic_editor/large_helmet.dart';
import 'package:tecmo_graphic_editor/logo_edit.dart';
import 'package:tecmo_graphic_editor/logo_image.dart';
import 'package:tecmo_graphic_editor/nes_palette.dart';
import 'package:tecmo_graphic_editor/nes_rom.dart';
import 'package:tecmo_graphic_editor/small_helmet.dart';
import 'package:tecmo_graphic_editor/small_helmet_edit.dart';
import 'package:tecmo_graphic_editor/small_helmet_image.dart';
import 'package:tecmo_graphic_editor/small_helmet_renderer.dart';
import 'package:tecmo_graphic_editor/theme.dart';
import 'package:tecmo_graphic_editor/zip.dart';
import 'package:web/web.dart' as web;

part 'small_helmet_ui.dart';
part 'theme_ui.dart';

/// One side of the editor: a loaded ROM and its helmet grid.
class Pane {
  final String prefix; // 'src' / 'dst'
  LargeHelmetReader? reader;
  String fileName = '';
  Pane(this.prefix);

  web.HTMLElement get grid => byId('${prefix}Grid');
  web.HTMLElement get info => byId('${prefix}Info');
  web.HTMLElement get element => byId('${prefix}Pane');
  web.HTMLElement get small => byId('${prefix}Small');
}

final src = Pane('src'), dst = Pane('dst');
HelmetCopy? copied;
Pane? copiedPane; // where the copy came from (for the highlight)
int? copiedIndex;
(Pane, int)? menuFor; // card the context menu was opened on
int? pendingTarget;
PastePlan? plan;
final changed = <int>{};
final undoStack = <(Uint8List, Set<int>, Set<int>)>[];
int? colorTeam; // team open in the color editor
var colorEdit = <int>[];
var colorSlot = 0;

T byId<T extends JSObject>(String id) => web.document.getElementById(id)! as T;

void onClick(String id, void Function() f) => byId<web.HTMLElement>(id).onclick = ((web.Event _) => f()).toJS;

/// package:web types clientX/Y as int, but browsers report fractions.
double clientCoord(web.MouseEvent e, String name) => (e as JSObject).getProperty<JSNumber>(name.toJS).toDartDouble;

/// The first pane (Source, then Target) that has no ROM yet.
Pane? emptyPane() => src.reader == null ? src : (dst.reader == null ? dst : null);

void clearDropHighlight() {
  for (final pane in [src, dst]) {
    pane.element.classList.remove('over');
  }
}

void main() {
  for (final pane in [src, dst]) {
    final input = byId<web.HTMLInputElement>('${pane.prefix}File');
    onClick('${pane.prefix}Open', () => input.click());
    onClick('${pane.prefix}Logos', () => exportAllLogos(pane));
    input.onchange = ((web.Event _) {
      final f = input.files?.item(0);
      if (f != null) loadFile(pane, f);
      input.value = '';
    }).toJS;
    final el = byId<web.HTMLElement>('${pane.prefix}Pane');
    el.ondragover = ((web.DragEvent e) {
      e.preventDefault();
      e.stopPropagation(); // the page-level handler below is for drops outside the panes
      clearDropHighlight();
      el.classList.add('over');
    }).toJS;
    el.ondragleave = ((web.DragEvent _) => el.classList.remove('over')).toJS;
    el.ondrop = ((web.DragEvent e) {
      e.preventDefault();
      e.stopPropagation();
      clearDropHighlight();
      final f = e.dataTransfer?.files.item(0);
      if (f != null) loadFile(pane, f);
    }).toJS;
  }
  // A ROM dropped anywhere else on the page goes to the first pane without one
  // (Source, then Target); otherwise the browser would open the file itself.
  web.document.addEventListener('dragover', ((web.DragEvent e) {
    e.preventDefault();
    clearDropHighlight();
    emptyPane()?.element.classList.add('over');
  }).toJS);
  web.document.addEventListener('dragleave', ((web.DragEvent e) {
    if (e.relatedTarget == null) clearDropHighlight();
  }).toJS);
  web.document.addEventListener('drop', ((web.DragEvent e) {
    e.preventDefault();
    clearDropHighlight();
    final f = e.dataTransfer?.files.item(0);
    if (f == null) return;
    final pane = emptyPane();
    if (pane != null) {
      loadFile(pane, f);
    } else {
      byId<web.HTMLElement>('hint').textContent = 'Both ROMs are loaded. Drop the file onto the Source or Target pane to replace that one.';
    }
  }).toJS);
  onClick('srcFromTarget', () {
    final r = dst.reader;
    if (r == null) return;
    setRom(src, Uint8List.fromList(r.rom.bytes), '${dst.fileName} (target, with edits)');
  });
  onClick('undo', undo);
  onClick('save', save);
  onClick('pvCancel', closePreview);
  onClick('pvApply', applyPaste);
  for (final id in ['optLogo', 'optColors', 'optShell']) {
    byId<web.HTMLInputElement>(id).onchange = ((web.Event _) => showPreview()).toJS;
  }

  onClick('ctxCopy', () {
    final m = menuFor;
    hideMenu();
    if (m != null) copyFrom(m.$1, m.$2);
  });
  onClick('ctxPaste', () {
    final m = menuFor;
    hideMenu();
    if (m != null) pasteOnto(m.$2);
  });
  onClick('ctxColors', () {
    final m = menuFor;
    hideMenu();
    if (m != null) openColors(m.$2);
  });
  onClick('ceReset', () {
    colorEdit = helmetColors(dst.reader!, colorTeam!);
    showColors();
  });
  onClick('ceCancel', closeColors);
  onClick('ceApply', applyColors);
  onClick('ctxLogo', () {
    final m = menuFor;
    hideMenu();
    if (m != null) openLogo(m.$2);
  });
  setUpLogoEditor();
  setUpSmallEditor();
  setUpTheme();
  setUpHelp();
  web.document.addEventListener('mousedown', ((web.MouseEvent e) {
    final menu = byId<web.HTMLElement>('ctx'), small = byId<web.HTMLElement>('ctxS');
    if (!menu.contains(e.target as web.Node?) && !small.contains(e.target as web.Node?)) hideMenu();
  }).toJS);
  web.document.addEventListener('keydown', ((web.KeyboardEvent e) {
    if (e.key == 'Escape') hideMenu();
    helpKey(e);
    if (logoTeam != null && !(e.target?.isA<web.HTMLInputElement>() ?? false)) logoKey(e);
    if (smallTeam != null && !(e.target?.isA<web.HTMLInputElement>() ?? false)) smallKey(e);
  }).toJS);
  web.window.addEventListener('scroll', ((web.Event _) => hideMenu()).toJS);
  web.window.addEventListener('resize', ((web.Event _) => hideMenu()).toJS);
  refresh();
}

void showMenu(web.MouseEvent e, Pane pane, int i) {
  e.preventDefault();
  menuFor = (pane, i);
  final paste = byId<web.HTMLButtonElement>('ctxPaste');
  paste.hidden = (pane != dst).toJS;
  byId<web.HTMLElement>('ctxSep').hidden = (pane != dst).toJS;
  byId<web.HTMLButtonElement>('ctxColors')
    ..hidden = (pane != dst).toJS
    ..textContent = 'Edit ${cap(pane.reader!.teams[i])} colors…';
  byId<web.HTMLButtonElement>('ctxLogo')
    ..hidden = (pane != dst).toJS
    ..textContent = 'Edit ${cap(pane.reader!.teams[i])} logo…';
  paste.disabled = copied == null;
  paste.textContent = copied == null ? 'Paste helmet here' : 'Paste ${cap(copied!.team)} helmet here';
  byId<web.HTMLButtonElement>('ctxCopy').textContent = 'Copy ${cap(pane.reader!.teams[i])} helmet';
  final menu = byId<web.HTMLElement>('ctx')..hidden = false.toJS;
  // Keep the menu inside the window.
  final w = menu.offsetWidth, h = menu.offsetHeight;
  final cx = clientCoord(e, 'clientX'), cy = clientCoord(e, 'clientY');
  final x = cx + w > web.window.innerWidth ? cx - w : cx;
  final y = cy + h > web.window.innerHeight ? cy - h : cy;
  menu.style
    ..left = '${x < 0 ? 0 : x}px'
    ..top = '${y < 0 ? 0 : y}px';
}

void hideMenu() {
  menuFor = null;
  smallMenuFor = null;
  byId<web.HTMLElement>('ctx').hidden = true.toJS;
  byId<web.HTMLElement>('ctxS').hidden = true.toJS;
}

Future<void> loadFile(Pane pane, web.File f) async {
  try {
    final buf = await f.arrayBuffer().toDart;
    setRom(pane, buf.toDart.asUint8List(), f.name);
  } catch (e) {
    pane.info.textContent = 'Could not read ${f.name}: $e';
  }
}

void setRom(Pane pane, Uint8List bytes, String name) {
  try {
    final r = LargeHelmetReader(NesRom(bytes));
    r.read(0); // fail early on ROMs that aren't TSB
    pane.reader = r;
    pane.fileName = name;
    pane.info.textContent = '$name — ${r.is32Team ? '32-team' : 'original 28-team'} ROM';
  } catch (e) {
    pane.info.textContent = 'Could not use $name: $e';
    return;
  }
  if (pane == copiedPane) {
    copied = null;
    copiedPane = null;
    copiedIndex = null;
  }
  if (pane == smallCopiedPane) {
    smallCopied = null;
    smallCopiedPane = null;
    smallCopiedIndex = null;
  }
  if (pane == dst) {
    changed.clear();
    smallChanged.clear();
    undoStack.clear();
    closeColors();
    closeLogo();
    closeSmall();
  }
  closePreview();
  refresh();
}

void refresh() {
  buildGrid(src);
  buildGrid(dst);
  byId<web.HTMLButtonElement>('undo').disabled = undoStack.isEmpty;
  byId<web.HTMLButtonElement>('save').disabled = dst.reader == null;
  byId<web.HTMLButtonElement>('srcFromTarget').disabled = dst.reader == null;
  for (final pane in [src, dst]) {
    byId<web.HTMLButtonElement>('${pane.prefix}Logos').disabled = pane.reader == null;
  }
}

void buildGrid(Pane pane) {
  final r = pane.reader;
  final grid = pane.grid;
  grid.innerHTML = ''.toJS;
  buildSmallGrid(pane);
  if (r == null) {
    grid.append(web.document.createElement('div')
      ..className = 'empty'
      ..textContent = 'No ROM loaded.');
    return;
  }
  for (var i = 0; i < r.teams.length; i++) {
    final classes = ['team'];
    if (pane == copiedPane && i == copiedIndex) classes.add('copied');
    if (pane == dst && i == pendingTarget) classes.add('pending');
    final card = web.document.createElement('div') as web.HTMLDivElement..className = classes.join(' ');
    final canvas = web.document.createElement('canvas') as web.HTMLCanvasElement
      ..width = helmetSize
      ..height = helmetSize;
    var label = r.teams[i];
    try {
      draw(canvas, renderHelmetIndexes(r.rom, r.read(i)));
    } catch (e) {
      label = '$label (error)';
    }
    card
      ..append(canvas)
      ..append(web.document.createElement('div')..textContent = label);
    if (pane == dst && changed.contains(i)) {
      card.append(web.document.createElement('span')
        ..className = 'dot'
        ..setAttribute('title', 'changed'));
    }
    final index = i;
    card.onclick = ((web.Event _) => pane == src ? copyFrom(src, index) : pasteOnto(index)).toJS;
    card.oncontextmenu = ((web.MouseEvent e) => showMenu(e, pane, index)).toJS;
    grid.append(card);
  }
}

void copyFrom(Pane pane, int i) {
  final r = pane.reader!;
  copied = copyHelmet(r, i);
  copiedPane = pane;
  copiedIndex = i;
  copiedFrom = pane == dst ? '${dst.fileName} (target)' : pane.fileName;
  byId<web.HTMLElement>('hint').textContent =
      'Copied ${r.teams[i]} from $copiedFrom. Right-click a target helmet → Paste helmet here.';
  buildGrid(src);
  buildGrid(dst);
  if (pendingTarget != null) showPreview();
}

String copiedFrom = '';

void pasteOnto(int t) {
  if (copied == null) {
    byId<web.HTMLElement>('hint').textContent = 'Copy a helmet first (right-click → Copy helmet).';
    return;
  }
  closeColors();
  closeLogo();
  closeSmall();
  pendingTarget = t;
  buildGrid(dst);
  showPreview();
}

void showPreview() {
  final r = dst.reader, c = copied, t = pendingTarget;
  if (r == null || c == null || t == null) return;
  final logo = byId<web.HTMLInputElement>('optLogo').checked;
  final colors = byId<web.HTMLInputElement>('optColors').checked;
  final shell = byId<web.HTMLInputElement>('optShell').checked;
  final p = plan = planPaste(r, t, c, logo: logo, colors: colors, shell: shell);

  byId<web.HTMLElement>('preview').hidden = false.toJS;
  byId<web.HTMLElement>('pvTitle').textContent = '${c.team} ($copiedFrom) → ${r.teams[t]}';
  draw(byId('pvBefore'), renderHelmetIndexes(r.rom, r.read(t)));
  final after = p.ok ? LargeHelmetReader(NesRom(p.result!)) : null;
  final afterCanvas = byId<web.HTMLCanvasElement>('pvAfter');
  if (after != null) {
    draw(afterCanvas, renderHelmetIndexes(after.rom, after.read(t)));
  } else {
    (afterCanvas.getContext('2d')! as web.CanvasRenderingContext2D).clearRect(0, 0, 72, 72);
  }

  final items = <String>[
    for (final e in p.errors) '<li class="err">${esc(e)}</li>',
    for (final n in p.notes) '<li class="${n.contains("isn't in this ROM") ? 'warn' : 'ok'}">${esc(n)}</li>',
    if (p.alsoAffects.isNotEmpty)
      '<li class="warn">Also changes ${esc(p.alsoAffects.join(', '))} (they share this logo).</li>',
    if (!logo && !colors && !shell) '<li class="warn">Nothing selected to copy.</li>',
  ];
  byId<web.HTMLElement>('pvNotes').innerHTML = items.join().toJS;
  byId<web.HTMLButtonElement>('pvApply').disabled = !p.ok || (!logo && !colors && !shell);
  byId<web.HTMLElement>('preview').scrollIntoView();
}

void applyPaste() {
  final r = dst.reader, p = plan, t = pendingTarget;
  if (r == null || p == null || !p.ok || t == null) return;
  commit(p.result!, {t, for (final name in p.alsoAffects) r.teams.indexOf(name)},
      'Pasted ${copied!.team} over ${r.teams[t]}. Remember to save the target ROM.');
  closePreview();
  refresh();
}

/// Replaces the target ROM with [bytes], keeping the old one for Undo.
void commit(Uint8List bytes, Set<int> teams, String message, {Set<int> small = const {}}) {
  undoStack.add((Uint8List.fromList(dst.reader!.rom.bytes), Set.of(changed), Set.of(smallChanged)));
  dst.reader = LargeHelmetReader(NesRom(bytes));
  changed.addAll(teams);
  smallChanged.addAll(small);
  byId<web.HTMLElement>('hint').textContent = message;
}

void openColors(int t) {
  closePreview();
  closeLogo();
  closeSmall();
  colorTeam = t;
  colorEdit = helmetColors(dst.reader!, t);
  colorSlot = 0;
  showColors();
  byId<web.HTMLElement>('colorEd').scrollIntoView();
}

void showColors() {
  final r = dst.reader, t = colorTeam;
  if (r == null || t == null) return;
  byId<web.HTMLElement>('colorEd').hidden = false.toJS;
  byId<web.HTMLElement>('ceTitle').textContent = '${r.teams[t]} colors';
  draw(byId('ceBefore'), renderHelmetIndexes(r.rom, r.read(t)));
  final after = LargeHelmetReader(NesRom(withHelmetColors(r, t, colorEdit)));
  draw(byId('ceAfter'), renderHelmetIndexes(after.rom, after.read(t)));

  final swatches = byId<web.HTMLElement>('ceSwatches')..innerHTML = ''.toJS;
  for (final (name, slots) in [('Shell', [0, 1, 2]), ('Facemask', [3, 4]), ('Logo', [5, 6, 7])]) {
    final row = web.document.createElement('div') as web.HTMLDivElement..className = 'swgroup';
    row.append(web.document.createElement('span')..textContent = name);
    for (final k in slots) {
      final c = colorEdit[k];
      final b = web.document.createElement('button') as web.HTMLButtonElement
        ..className = k == colorSlot ? 'sw sel' : 'sw'
        ..title = '${helmetColorLabels[k]}: ${hex(c)}'
        ..textContent = hex(c);
      b.style
        ..background = css(c)
        ..color = isLight(c) ? '#000' : '#fff';
      b.onclick = ((web.Event _) {
        colorSlot = k;
        showColors();
      }).toJS;
      row.append(b);
    }
    swatches.append(row);
  }
  byId<web.HTMLElement>('ceHelp').textContent = 'Choose a color for ${helmetColorLabels[colorSlot]}.';

  final grid = byId<web.HTMLElement>('cePalette')..innerHTML = ''.toJS;
  for (var c = 0; c < 64; c++) {
    final value = c;
    final b = web.document.createElement('button') as web.HTMLButtonElement
      ..className = c == colorEdit[colorSlot] ? 'sel' : ''
      ..title = c == 0x0D ? '\$0D is unsafe (can upset TVs)' : hex(c);
    b.style.background = css(c);
    b.disabled = c == 0x0D;
    b.onclick = ((web.Event _) {
      colorEdit[colorSlot] = value;
      showColors();
    }).toJS;
    grid.append(b);
  }
  final current = helmetColors(r, t);
  byId<web.HTMLButtonElement>('ceApply').disabled = _same(current, colorEdit);
  byId<web.HTMLButtonElement>('ceReset').disabled = _same(current, colorEdit);
}

bool _same(List<int> a, List<int> b) => List.generate(a.length, (i) => a[i] == b[i]).every((x) => x);

void applyColors() {
  final r = dst.reader, t = colorTeam;
  if (r == null || t == null) return;
  commit(withHelmetColors(r, t, colorEdit), {t}, 'Changed ${r.teams[t]} colors. Remember to save the target ROM.');
  closeColors();
  refresh();
}

void closeColors() {
  colorTeam = null;
  byId<web.HTMLElement>('colorEd').hidden = true.toJS;
}

String hex(int c) => '\$${c.toRadixString(16).padLeft(2, '0').toUpperCase()}';

String css(int c) => '#${nesPalette[c].toRadixString(16).padLeft(6, '0')}';

bool isLight(int c) {
  final v = nesPalette[c];
  return 0.299 * (v >> 16) + 0.587 * (v >> 8 & 0xFF) + 0.114 * (v & 0xFF) > 140;
}

void undo() {
  if (undoStack.isEmpty) return;
  final (bytes, was, smallWas) = undoStack.removeLast();
  dst.reader = LargeHelmetReader(NesRom(bytes));
  changed
    ..clear()
    ..addAll(was);
  smallChanged
    ..clear()
    ..addAll(smallWas);
  closePreview();
  closeColors();
  closeLogo();
  closeSmall();
  refresh();
}

void closePreview() {
  pendingTarget = null;
  plan = null;
  byId<web.HTMLElement>('preview').hidden = true.toJS;
  if (dst.reader != null) buildGrid(dst);
}

void save() {
  final r = dst.reader;
  if (r == null) return;
  download(r.rom.bytes, '${baseName(dst.fileName)}-helmets.nes', 'application/octet-stream');
}

/// The file name without its extension (and without the "(target, with edits)" note).
String baseName(String fileName) {
  final name = fileName.replaceAll(RegExp(r'\s*\(.*\)$'), '');
  final dot = name.lastIndexOf('.');
  return dot > 0 ? name.substring(0, dot) : name;
}

void download(Uint8List bytes, String name, String type) {
  final blob = web.Blob([bytes.toJS].toJS, web.BlobPropertyBag(type: type));
  final url = web.URL.createObjectURL(blob);
  final a = web.document.createElement('a') as web.HTMLAnchorElement
    ..href = url
    ..download = name;
  web.document.body!.append(a);
  a.click();
  a.remove();
  web.URL.revokeObjectURL(url);
}

/// Saves every team's logo as <team>-logo.png in one zip (the same files
/// Export PNG makes, so any of them can be imported back).
void exportAllLogos(Pane pane) {
  final r = pane.reader;
  if (r == null) return;
  final files = <String, Uint8List>{};
  for (var t = 0; t < r.teams.length; t++) {
    var name = '${r.teams[t]}-logo.png';
    for (var n = 2; files.containsKey(name); n++) {
      name = '${r.teams[t]}-$n-logo.png';
    }
    final url = logoPngUrl(logoPixels(r, t), helmetColors(r, t).sublist(5));
    files[name] = base64Decode(url.substring(url.indexOf(',') + 1));
  }
  download(storedZip(files), '${baseName(pane.fileName)}-logos.zip', 'application/zip');
}

String cap(String s) => s.isEmpty ? s : s[0].toUpperCase() + s.substring(1);

String esc(String s) => s.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');

void draw(web.HTMLCanvasElement canvas, Uint8List indexes) {
  final rgba = Uint8ClampedList.fromList(indexesToRgba(indexes));
  final ctx = canvas.getContext('2d')! as web.CanvasRenderingContext2D;
  ctx.putImageData(web.ImageData(rgba.toJS, canvas.width, canvas.height.toJS), 0, 0);
}

// ---- Logo editor ----

int? logoTeam;
var logoPx = Uint8List(0); // helmet-view pixels being edited: 0 = clear, 1-3 = logo colors
var logoOrig = Uint8List(0);
// Sprite top-lefts in the helmet view, front first; null lets the editor arrange them.
List<(int, int)>? logoBoxes;
var logoOrigBoxes = <(int, int)>[];
int? logoSel; // the sprite picked with the Move tool
var logoColors = <int>[], logoOrigColors = <int>[]; // NES colors of logo colors 1-3
var logoPalOpen = false;
final logoHistory = <(Uint8List, List<(int, int)>?, List<int>)>[];
({Uint8List pixels, List<(int, int)> boxes, int index, (int, int) from})? dragging;
var logoColor = 1;
var logoTool = 'pencil';
PastePlan? logoPlan;
LogoStats? logoNow;
String? importNote;
var painting = false, paintValue = 1;
(int, int)? lastPoint;
const zoom = 6;
final logoScratch = web.document.createElement('canvas') as web.HTMLCanvasElement
  ..width = helmetSize
  ..height = helmetSize;

void setUpLogoEditor() {
  final cv = byId<web.HTMLCanvasElement>('leCanvas');
  cv.addEventListener('pointerdown', ((web.PointerEvent e) {
    final p = canvasPoint(e);
    if (logoTeam == null || p == null) return;
    e.preventDefault();
    final erase = e.button == 2;
    if (logoTool == 'pick' && !erase) {
      final v = logoPx[p.$2 * helmetSize + p.$1];
      if (v != 0) logoColor = v;
      setTool('pencil');
      showSwatches();
      return;
    }
    if (logoTool == 'move') {
      final boxes = logoBoxes ?? logoNow?.boxes ?? [];
      final i = boxes.indexWhere((b) => p.$1 >= b.$1 && p.$1 < b.$1 + 8 && p.$2 >= b.$2 && p.$2 < b.$2 + 8);
      logoSel = i < 0 ? null : i;
      if (i >= 0) {
        pushLogoHistory();
        dragging = (pixels: Uint8List.fromList(logoPx), boxes: List.of(boxes), index: i, from: p);
        cv.setPointerCapture(e.pointerId);
      }
      drawLogo();
      return;
    }
    pushLogoHistory();
    if (logoTool == 'fill') {
      floodFill(p, erase ? 0 : logoColor);
      checkLogo();
      return;
    }
    painting = true;
    paintValue = erase ? 0 : logoColor;
    lastPoint = p;
    cv.setPointerCapture(e.pointerId);
    logoPx[p.$2 * helmetSize + p.$1] = paintValue;
    drawLogo();
  }).toJS);
  cv.addEventListener('pointermove', ((web.PointerEvent e) {
    final d = dragging;
    if (d != null) {
      final p = canvasPoint(e, clamp: true)!;
      final m = moveLogoSprite(d.pixels, d.boxes, d.index, p.$1 - d.from.$1, p.$2 - d.from.$2);
      logoPx = m.pixels;
      logoBoxes = m.boxes;
      logoSel = 0;
      drawLogo();
      return;
    }
    if (!painting) return;
    final p = canvasPoint(e, clamp: true)!;
    paintLine(lastPoint!, p);
    lastPoint = p;
    drawLogo();
  }).toJS);
  void endStroke(web.Event _) {
    if (!painting && dragging == null) return;
    painting = false;
    dragging = null;
    checkLogo();
  }

  cv.addEventListener('pointerup', endStroke.toJS);
  cv.addEventListener('pointercancel', endStroke.toJS);
  cv.addEventListener('contextmenu', ((web.Event e) => e.preventDefault()).toJS);

  onClick('tPencil', () => setTool('pencil'));
  onClick('tFill', () => setTool('fill'));
  onClick('tPick', () => setTool('pick'));
  onClick('tMove', () => setTool('move'));
  onClick('lePalBtn', () {
    logoPalOpen = !logoPalOpen;
    showSwatches();
  });
  onClick('leArrange', () {
    pushLogoHistory();
    logoBoxes = null;
    logoSel = null;
    checkLogo();
  });
  for (final id in ['leGuide', 'leBoxes']) {
    byId<web.HTMLInputElement>(id).onchange = ((web.Event _) => drawLogo()).toJS;
  }
  onClick('leUndo', undoLogo);
  onClick('leClear', () {
    pushLogoHistory();
    logoPx.fillRange(0, logoPx.length, 0);
    checkLogo();
  });
  onClick('leRevert', () {
    pushLogoHistory();
    logoPx = Uint8List.fromList(logoOrig);
    logoBoxes = null;
    logoSel = null;
    logoColors = List.of(logoOrigColors);
    importNote = null;
    checkLogo();
  });
  onClick('leExport', exportLogo);
  final file = byId<web.HTMLInputElement>('leFile');
  onClick('leImport', () => file.click());
  file.onchange = ((web.Event _) {
    final f = file.files?.item(0);
    if (f != null) importLogo(f);
    file.value = '';
  }).toJS;
  onClick('leCancel', closeLogo);
  onClick('leApply', applyLogo);
}

void logoKey(web.KeyboardEvent e) {
  if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() == 'z') {
    e.preventDefault();
    undoLogo();
  } else if (!e.ctrlKey && !e.metaKey && !e.altKey) {
    final step = e.shiftKey ? 8 : 1;
    final nudge = {'ArrowLeft': (-step, 0), 'ArrowRight': (step, 0), 'ArrowUp': (0, -step), 'ArrowDown': (0, step)}[e.key];
    final boxes = logoBoxes, sel = logoSel;
    if (nudge != null && logoTool == 'move' && boxes != null && sel != null && sel < boxes.length) {
      e.preventDefault();
      pushLogoHistory();
      final m = moveLogoSprite(logoPx, boxes, sel, nudge.$1, nudge.$2);
      logoPx = m.pixels;
      logoBoxes = m.boxes;
      logoSel = 0;
      checkLogo();
      return;
    }
    final tool = {'p': 'pencil', 'f': 'fill', 'i': 'pick', 'm': 'move'}[e.key.toLowerCase()];
    if (tool != null) setTool(tool);
  }
}

void openLogo(int t) {
  closePreview();
  closeColors();
  closeSmall();
  logoTeam = t;
  logoOrig = logoPixels(dst.reader!, t);
  logoPx = Uint8List.fromList(logoOrig);
  logoOrigBoxes = checkLogoEdit(dst.reader!, t, logoOrig).stats.boxes;
  logoBoxes = null;
  logoSel = null;
  logoOrigColors = helmetColors(dst.reader!, t).sublist(5);
  logoColors = List.of(logoOrigColors);
  logoPalOpen = false;
  logoHistory.clear();
  importNote = null;
  byId<web.HTMLElement>('logoEd').hidden = false.toJS;
  byId<web.HTMLElement>('leTitle').textContent = '${dst.reader!.teams[t]} logo';
  checkLogo();
  byId<web.HTMLElement>('logoEd').scrollIntoView();
}

void closeLogo() {
  logoTeam = null;
  logoPlan = null;
  painting = false;
  dragging = null;
  byId<web.HTMLElement>('logoEd').hidden = true.toJS;
}

void setTool(String tool) {
  logoTool = tool;
  for (final (id, name) in [('tPencil', 'pencil'), ('tFill', 'fill'), ('tPick', 'pick'), ('tMove', 'move')]) {
    byId<web.HTMLElement>(id).className = name == tool ? 'on' : '';
  }
  byId<web.HTMLElement>('leCanvas').style.cursor = tool == 'move' ? 'move' : 'crosshair';
  drawLogo();
}

(int, int)? canvasPoint(web.MouseEvent e, {bool clamp = false}) {
  final rect = byId<web.HTMLCanvasElement>('leCanvas').getBoundingClientRect();
  var x = ((clientCoord(e, 'clientX') - rect.left) / rect.width * helmetSize).floor();
  var y = ((clientCoord(e, 'clientY') - rect.top) / rect.height * helmetSize).floor();
  if (clamp) {
    x = x.clamp(0, helmetSize - 1);
    y = y.clamp(0, helmetSize - 1);
  }
  return x >= 0 && x < helmetSize && y >= 0 && y < helmetSize ? (x, y) : null;
}

void paintLine((int, int) a, (int, int) b) {
  var (x0, y0) = a;
  final (x1, y1) = b;
  final dx = (x1 - x0).abs(), dy = -(y1 - y0).abs();
  final sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
  var err = dx + dy;
  while (true) {
    logoPx[y0 * helmetSize + x0] = paintValue;
    if (x0 == x1 && y0 == y1) return;
    final e2 = 2 * err;
    if (e2 >= dy) {
      err += dy;
      x0 += sx;
    }
    if (e2 <= dx) {
      err += dx;
      y0 += sy;
    }
  }
}

void floodFill((int, int) start, int value) {
  final from = logoPx[start.$2 * helmetSize + start.$1];
  if (from == value) return;
  final todo = [start];
  while (todo.isNotEmpty) {
    final (x, y) = todo.removeLast();
    if (x < 0 || y < 0 || x >= helmetSize || y >= helmetSize || logoPx[y * helmetSize + x] != from) continue;
    logoPx[y * helmetSize + x] = value;
    todo.addAll([(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]);
  }
}

void pushLogoHistory() {
  logoHistory.add((Uint8List.fromList(logoPx), logoBoxes == null ? null : List.of(logoBoxes!), List.of(logoColors)));
  if (logoHistory.length > 200) logoHistory.removeAt(0);
}

void undoLogo() {
  if (logoHistory.isEmpty) return;
  final (px, boxes, colors) = logoHistory.removeLast();
  logoPx = px;
  logoBoxes = boxes;
  logoColors = colors;
  logoSel = null;
  checkLogo();
}

/// Recomputes sprite use and whether the logo fits, then redraws.
void checkLogo() {
  final r = dst.reader, t = logoTeam;
  if (r == null || t == null) return;
  final c = checkLogoEdit(r, t, logoPx, boxes: logoBoxes);
  final s = logoNow = c.stats;
  logoBoxes = s.boxes; // from now on, sprites stay where they are unless moved
  if (logoSel != null && logoSel! >= s.boxes.length) logoSel = null;
  final edited = !_same(logoPx, logoOrig) || !_sameBoxes(s.boxes, logoOrigBoxes);
  final p = logoPlan = edited ? c.plan : null;
  final recolored = !_same(logoColors, logoOrigColors);

  byId<web.HTMLElement>('leStats').textContent = '${s.sprites} of ${s.maxSprites} sprites · busiest line '
      '${s.busiestLine} of $spritesPerLine · ${s.tiles} tile${s.tiles == 1 ? '' : 's'}';
  final items = <String>[
    if (importNote != null) '<li class="warn">${esc(importNote!)}</li>',
    if (p == null && !recolored) '<li>No changes yet.</li>',
    if (p == null && recolored) '<li class="ok">New colors. Only this team uses them.</li>',
    if (p != null) ...[
      for (final e in p.errors) '<li class="err">${esc(e)}</li>',
      if (c.autoArrangeFits) '<li class="warn">Auto-arrange sprites would make this fit.</li>',
      if (p.ok) '<li class="ok">Fits. ${esc(p.notes.join(' '))}</li>',
      if (p.alsoAffects.isNotEmpty)
        '<li class="warn">Also changes ${esc(p.alsoAffects.join(', '))} (they share this logo).</li>',
    ],
  ];
  byId<web.HTMLElement>('leNotes').innerHTML = items.join().toJS;
  byId<web.HTMLButtonElement>('leApply').disabled = p == null ? !recolored : !p.ok;
  byId<web.HTMLButtonElement>('leUndo').disabled = logoHistory.isEmpty;
  byId<web.HTMLButtonElement>('leRevert').disabled = !edited && !recolored;
  showSwatches();
  drawLogo();
}

void showSwatches() {
  final row = byId<web.HTMLElement>('leColors')..innerHTML = ''.toJS;
  row.append(web.document.createElement('span')..textContent = 'Color');
  for (var k = 0; k < 4; k++) {
    final b = web.document.createElement('button') as web.HTMLButtonElement
      ..className = 'sw${k == 0 ? ' clear' : ''}${k == logoColor ? ' sel' : ''}'
      ..textContent = k == 0 ? 'none' : '$k'
      ..title = k == 0 ? 'Transparent (right-click also erases)' : 'Logo color $k: ${hex(logoColors[k - 1])}';
    if (k > 0) {
      b.style
        ..background = css(logoColors[k - 1])
        ..color = isLight(logoColors[k - 1]) ? '#000' : '#fff';
    }
    final value = k;
    b.onclick = ((web.Event _) {
      logoColor = value;
      showSwatches();
    }).toJS;
    row.append(b);
  }

  // Changing what logo color 1-3 looks like (this team's own palette bytes).
  final btn = byId<web.HTMLButtonElement>('lePalBtn')
    ..disabled = logoColor == 0
    ..textContent = logoPalOpen ? 'Done changing colors' : 'Change color $logoColor…';
  if (logoColor == 0) btn.textContent = 'Change color… (pick color 1-3 first)';
  final grid = byId<web.HTMLElement>('lePalette')
    ..hidden = (!logoPalOpen || logoColor == 0).toJS
    ..innerHTML = ''.toJS;
  if (logoColor == 0) return;
  for (var c = 0; c < 64; c++) {
    final value = c;
    final b = web.document.createElement('button') as web.HTMLButtonElement
      ..className = c == logoColors[logoColor - 1] ? 'sel' : ''
      ..title = c == 0x0D ? '\$0D is unsafe (can upset TVs)' : hex(c);
    b.style.background = css(c);
    b.disabled = c == 0x0D;
    b.onclick = ((web.Event _) {
      if (logoColors[logoColor - 1] == value) return;
      pushLogoHistory();
      logoColors[logoColor - 1] = value;
      checkLogo();
    }).toJS;
    grid.append(b);
  }
}

void drawLogo() {
  final r = dst.reader, t = logoTeam;
  if (r == null || t == null) return;
  final h = r.read(t);
  final pal = h.paletteRam;
  final base = byId<web.HTMLInputElement>('leGuide').checked
      ? renderHelmetIndexes(r.rom, h, logo: false)
      : (Uint8List(helmetSize * helmetSize)..fillRange(0, helmetSize * helmetSize, pal[0]));
  for (var k = 0; k < base.length; k++) {
    if (logoPx[k] != 0) base[k] = logoColors[logoPx[k] - 1];
  }
  draw(logoScratch, base);
  final cv = byId<web.HTMLCanvasElement>('leCanvas');
  final ctx = cv.getContext('2d')! as web.CanvasRenderingContext2D
    ..imageSmoothingEnabled = false;
  ctx.drawImage(logoScratch, 0, 0, helmetSize * zoom, helmetSize * zoom);
  final boxes = logoBoxes ?? logoNow?.boxes;
  final moving = logoTool == 'move';
  if (boxes != null && (moving || byId<web.HTMLInputElement>('leBoxes').checked)) {
    ctx.lineWidth = 1;
    for (var i = boxes.length - 1; i >= 0; i--) {
      final (x, y) = boxes[i];
      final sel = moving && i == logoSel;
      ctx
        ..strokeStyle = (sel ? 'rgba(0, 230, 255, 1)' : 'rgba(255, 220, 0, 0.75)').toJS
        ..lineWidth = sel ? 3 : 1;
      ctx.strokeRect(x * zoom + 0.5, y * zoom + 0.5, 8 * zoom - 1, 8 * zoom - 1);
    }
  }
}

bool _sameBoxes(List<(int, int)> a, List<(int, int)> b) =>
    a.length == b.length && Iterable.generate(a.length).every((i) => a[i] == b[i]);

void applyLogo() {
  final r = dst.reader, t = logoTeam, p = logoPlan;
  final recolored = !_same(logoColors, logoOrigColors);
  if (r == null || t == null || (p == null && !recolored) || (p != null && !p.ok)) return;
  var bytes = p?.result ?? r.rom.bytes;
  if (recolored) {
    final edited = LargeHelmetReader(NesRom(bytes));
    bytes = withHelmetColors(edited, t, [...helmetColors(edited, t).sublist(0, 5), ...logoColors]);
  }
  commit(bytes, {t, for (final name in p?.alsoAffects ?? <String>[]) r.teams.indexOf(name)},
      'Changed ${r.teams[t]} logo. Remember to save the target ROM.');
  closeLogo();
  refresh();
}

/// Saves the logo as a 72x72 PNG with a transparent background.
void exportLogo() {
  final r = dst.reader, t = logoTeam;
  if (r == null || t == null) return;
  final a = web.document.createElement('a') as web.HTMLAnchorElement
    ..href = logoPngUrl(logoPx, logoColors)
    ..download = '${r.teams[t]}-logo.png';
  web.document.body!.append(a);
  a.click();
  a.remove();
}

/// [px] (helmet-view logo pixels) as a PNG data URL in the NES [colors] of
/// logo colors 1-3, with the color strip below (see logo_image.dart).
String logoPngUrl(Uint8List px, List<int> colors) {
  final c = web.document.createElement('canvas') as web.HTMLCanvasElement
    ..width = helmetSize
    ..height = logoImageHeight;
  (c.getContext('2d')! as web.CanvasRenderingContext2D).putImageData(
      web.ImageData(Uint8ClampedList.fromList(logoImageRgba(px, colors)).toJS, helmetSize, logoImageHeight.toJS), 0, 0);
  return c.toDataURL('image/png');
}

/// Reads a picture into the logo: transparent pixels stay clear, everything
/// else snaps to the nearest of the 3 logo colors.
Future<void> importLogo(web.File f) async {
  final r = dst.reader, t = logoTeam;
  if (r == null || t == null) return;
  try {
    final bmp = await web.window.createImageBitmap(f).toDart;
    // Our own exports (or scaled-up copies of them) carry the color strip.
    final height = bmp.width * logoImageHeight == bmp.height * helmetSize ? logoImageHeight : helmetSize;
    final c = web.document.createElement('canvas') as web.HTMLCanvasElement
      ..width = helmetSize
      ..height = height;
    final ctx = c.getContext('2d')! as web.CanvasRenderingContext2D
      ..imageSmoothingEnabled = false;
    ctx.drawImage(bmp, 0, 0, helmetSize, height);
    final data = ctx.getImageData(0, 0, helmetSize, height).data.toDart;

    final back = readLogoImage(Uint8List.fromList(data), height, logoColors);
    final notes = [
      if (bmp.width != helmetSize || bmp.height != height)
        'The picture was ${bmp.width}×${bmp.height}; it was scaled to $helmetSize×$height.',
      ...back.notes,
    ];
    pushLogoHistory();
    logoPx = back.pixels;
    logoColors = back.colors;
    logoBoxes = null; // a new picture: let the editor arrange the sprites
    importNote = notes.isEmpty ? null : notes.join(' ');
    checkLogo();
  } catch (e) {
    importNote = 'Could not read ${f.name}: $e';
    checkLogo();
  }
}
