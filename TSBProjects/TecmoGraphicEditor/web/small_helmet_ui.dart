// The small team-select helmets: copy / paste, the pixel editor and PNG
// export / import. Part of main.dart (shares its panes, undo and helpers).
part of 'main.dart';

SmallHelmetPicture? smallCopied;
Pane? smallCopiedPane;
int? smallCopiedIndex;
String smallCopiedFrom = '';
final smallChanged = <int>{}; // teams whose small helmet (or a tile it shares) was changed
(Pane, int)? smallMenuFor;

SmallHelmetEditor? smallEd;
int? smallTeam;
var smallLayer = 'bg'; // 'bg' or 'overlay'
var smallTool = 'pencil';
var smallColor = 1;
var smallTitle = '';
var smallNotes = <String>[]; // from a paste or an import
final smallHistory = <(Map<int, Uint8List>, List<int>)>[];
var smallPainting = false;
var smallPaintValue = 1;
(int, int)? smallLast;
const smallZoomBg = 20, smallZoomOverlay = 14;

// ---- grid and menu ----

/// Grid of the small team-select helmets: click copies (source) or pastes
/// (target), right-click opens the menu.
void buildSmallGrid(Pane pane) {
  final r = pane.reader;
  final grid = pane.small;
  grid.innerHTML = ''.toJS;
  if (r == null) return;
  final small = SmallHelmetReader(r.rom);
  for (var i = 0; i < small.teams.length; i++) {
    final classes = ['team'];
    if (pane == smallCopiedPane && i == smallCopiedIndex) classes.add('copied');
    if (pane == dst && i == smallTeam) classes.add('pending');
    final card = web.document.createElement('div') as web.HTMLDivElement..className = classes.join(' ');
    final canvas = web.document.createElement('canvas') as web.HTMLCanvasElement
      ..width = smallHelmetWidth
      ..height = smallHelmetHeight;
    var label = small.teams[i];
    try {
      draw(canvas, renderSmallHelmetIndexes(r.rom, small.read(i)));
    } catch (e) {
      label = '$label (error)';
    }
    card
      ..append(canvas)
      ..append(web.document.createElement('div')..textContent = label);
    if (pane == dst && smallChanged.contains(i)) {
      card.append(web.document.createElement('span')
        ..className = 'dot'
        ..setAttribute('title', 'changed'));
    }
    final index = i;
    card.onclick = ((web.Event _) => pane == src ? copySmall(src, index) : pasteSmall(index)).toJS;
    card.oncontextmenu = ((web.MouseEvent e) => showSmallMenu(e, pane, index)).toJS;
    grid.append(card);
  }
}

void copySmall(Pane pane, int i) {
  final r = pane.reader!;
  smallCopied = SmallHelmetEditor(r.rom, i).picture();
  smallCopiedPane = pane;
  smallCopiedIndex = i;
  smallCopiedFrom = pane == dst ? '${dst.fileName} (target)' : pane.fileName;
  byId<web.HTMLElement>('hint').textContent =
      'Copied ${r.teams[i]}\'s small helmet from $smallCopiedFrom. Right-click a target small helmet → Paste small helmet here.';
  buildSmallGrid(src);
  buildSmallGrid(dst);
}

void pasteSmall(int t) {
  final c = smallCopied;
  if (c == null) {
    byId<web.HTMLElement>('hint').textContent = 'Copy a small helmet first (right-click → Copy small helmet).';
    return;
  }
  openSmall(t, paste: c);
}

void showSmallMenu(web.MouseEvent e, Pane pane, int i) {
  e.preventDefault();
  hideMenu();
  smallMenuFor = (pane, i);
  final name = cap(pane.reader!.teams[i]);
  byId<web.HTMLButtonElement>('ctxSCopy').textContent = 'Copy $name small helmet';
  final paste = byId<web.HTMLButtonElement>('ctxSPaste')
    ..hidden = (pane != dst).toJS
    ..disabled = smallCopied == null
    ..textContent = smallCopied == null ? 'Paste small helmet here' : 'Paste ${cap(smallCopied!.team)} small helmet here';
  byId<web.HTMLElement>('ctxSSep').hidden = (pane != dst).toJS;
  byId<web.HTMLButtonElement>('ctxSEdit')
    ..hidden = (pane != dst).toJS
    ..textContent = 'Edit $name small helmet…';
  paste.hidden = (pane != dst).toJS;
  final menu = byId<web.HTMLElement>('ctxS')..hidden = false.toJS;
  final w = menu.offsetWidth, h = menu.offsetHeight;
  final cx = clientCoord(e, 'clientX'), cy = clientCoord(e, 'clientY');
  final x = cx + w > web.window.innerWidth ? cx - w : cx;
  final y = cy + h > web.window.innerHeight ? cy - h : cy;
  menu.style
    ..left = '${x < 0 ? 0 : x}px'
    ..top = '${y < 0 ? 0 : y}px';
}

// ---- editor ----

void setUpSmallEditor() {
  onClick('ctxSCopy', () {
    final m = smallMenuFor;
    hideMenu();
    if (m != null) copySmall(m.$1, m.$2);
  });
  onClick('ctxSPaste', () {
    final m = smallMenuFor;
    hideMenu();
    if (m != null) pasteSmall(m.$2);
  });
  onClick('ctxSEdit', () {
    final m = smallMenuFor;
    hideMenu();
    if (m != null) openSmall(m.$2);
  });

  final cv = byId<web.HTMLCanvasElement>('seCanvas');
  cv.addEventListener('pointerdown', ((web.PointerEvent e) {
    final p = smallPoint(e);
    final ed = smallEd;
    if (ed == null || p == null) return;
    e.preventDefault();
    final erase = e.button == 2;
    final w = smallLayerWidth;
    if (smallTool == 'pick' && !erase) {
      final v = smallLayerPixels()[p.$2 * w + p.$1];
      if (v != 0) smallColor = v;
      setSmallTool('pencil');
      refreshSmall();
      return;
    }
    if (!smallEditable(p.$1, p.$2)) return;
    pushSmallHistory();
    if (smallTool == 'fill') {
      smallFill(p, erase ? 0 : smallColor);
      refreshSmall();
      return;
    }
    smallPainting = true;
    smallPaintValue = erase ? 0 : smallColor;
    smallLast = p;
    try {
      cv.setPointerCapture(e.pointerId);
    } catch (_) {} // synthetic pointers (and some devices) have nothing to capture
    smallPaint(p.$1, p.$2, smallPaintValue);
    drawSmall();
  }).toJS);
  cv.addEventListener('pointermove', ((web.PointerEvent e) {
    if (!smallPainting) return;
    final p = smallPoint(e, clamp: true)!;
    smallPaintLine(smallLast!, p);
    smallLast = p;
    drawSmall();
  }).toJS);
  void endStroke(web.Event _) {
    if (!smallPainting) return;
    smallPainting = false;
    refreshSmall();
  }

  cv.addEventListener('pointerup', endStroke.toJS);
  cv.addEventListener('pointercancel', endStroke.toJS);
  cv.addEventListener('contextmenu', ((web.Event e) => e.preventDefault()).toJS);

  onClick('sLayerBg', () => setSmallLayer('bg'));
  onClick('sLayerOv', () => setSmallLayer('overlay'));
  onClick('sPencil', () => setSmallTool('pencil'));
  onClick('sFill', () => setSmallTool('fill'));
  onClick('sPick', () => setSmallTool('pick'));
  for (final id in ['seGuide', 'seGrid']) {
    byId<web.HTMLInputElement>(id).onchange = ((web.Event _) => drawSmall()).toJS;
  }
  onClick('seUndo', undoSmall);
  onClick('seRevert', () {
    final ed = smallEd;
    if (ed == null) return;
    pushSmallHistory();
    ed.revert();
    smallNotes = [];
    refreshSmall();
  });
  onClick('seExport', exportSmall);
  final file = byId<web.HTMLInputElement>('seFile');
  onClick('seImport', () => file.click());
  file.onchange = ((web.Event _) {
    final f = file.files?.item(0);
    if (f != null) importSmall(f);
    file.value = '';
  }).toJS;
  onClick('seCancel', closeSmall);
  onClick('seApply', applySmall);
}

void smallKey(web.KeyboardEvent e) {
  if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() == 'z') {
    e.preventDefault();
    undoSmall();
  } else if (!e.ctrlKey && !e.metaKey && !e.altKey) {
    final tool = {'p': 'pencil', 'f': 'fill', 'i': 'pick'}[e.key.toLowerCase()];
    if (tool != null) setSmallTool(tool);
  }
}

/// Opens the editor on the target's small helmet [t]; with [paste], the copied
/// helmet is applied first (and can still be tweaked or cancelled).
void openSmall(int t, {SmallHelmetPicture? paste}) {
  final r = dst.reader;
  if (r == null) return;
  closePreview();
  closeColors();
  closeLogo();
  final ed = SmallHelmetEditor(r.rom, t);
  smallTeam = t;
  smallEd = ed;
  smallHistory.clear();
  smallLayer = 'bg';
  smallNotes = [];
  smallTitle = '${r.teams[t]} small helmet';
  if (paste != null) {
    final res = ed.applyPicture(paste);
    smallNotes = res.notes;
    smallTitle = '${paste.team} ($smallCopiedFrom) → ${r.teams[t]} small helmet';
  }
  byId<web.HTMLElement>('smallEd').hidden = false.toJS;
  setSmallTool(smallTool, redraw: false);
  setSmallLayer('bg', redraw: false);
  buildSmallGrid(dst);
  refreshSmall();
  byId<web.HTMLElement>('smallEd').scrollIntoView();
}

void closeSmall() {
  final was = smallTeam;
  smallTeam = null;
  smallEd = null;
  smallPainting = false;
  byId<web.HTMLElement>('smallEd').hidden = true.toJS;
  if (was != null && dst.reader != null) buildSmallGrid(dst);
}

void setSmallLayer(String layer, {bool redraw = true}) {
  smallLayer = layer;
  byId<web.HTMLElement>('sLayerBg').className = layer == 'bg' ? 'on' : '';
  byId<web.HTMLElement>('sLayerOv').className = layer == 'overlay' ? 'on' : '';
  if (redraw) refreshSmall();
}

void setSmallTool(String tool, {bool redraw = true}) {
  smallTool = tool;
  for (final (id, name) in [('sPencil', 'pencil'), ('sFill', 'fill'), ('sPick', 'pick')]) {
    byId<web.HTMLElement>(id).className = name == tool ? 'on' : '';
  }
  if (redraw) drawSmall();
}

int get smallLayerWidth => smallLayer == 'bg' ? smallBgSize : smallHelmetWidth;
int get smallLayerHeight => smallLayer == 'bg' ? smallBgSize : smallHelmetHeight;
int get smallZoom => smallLayer == 'bg' ? smallZoomBg : smallZoomOverlay;

Uint8List smallLayerPixels() => smallLayer == 'bg' ? smallEd!.bg : smallEd!.overlay;

bool smallEditable(int x, int y) => smallLayer == 'bg' ? smallEd!.bgEditable(x, y) : smallEd!.overlayEditable(x, y);

bool smallPaint(int x, int y, int v) => smallLayer == 'bg' ? smallEd!.paintBg(x, y, v) : smallEd!.paintOverlay(x, y, v);

(int, int)? smallPoint(web.MouseEvent e, {bool clamp = false}) {
  final rect = byId<web.HTMLCanvasElement>('seCanvas').getBoundingClientRect();
  final w = smallLayerWidth, h = smallLayerHeight;
  var x = ((clientCoord(e, 'clientX') - rect.left) / rect.width * w).floor();
  var y = ((clientCoord(e, 'clientY') - rect.top) / rect.height * h).floor();
  if (clamp) {
    x = x.clamp(0, w - 1);
    y = y.clamp(0, h - 1);
  }
  return x >= 0 && x < w && y >= 0 && y < h ? (x, y) : null;
}

void smallPaintLine((int, int) a, (int, int) b) {
  var (x0, y0) = a;
  final (x1, y1) = b;
  final dx = (x1 - x0).abs(), dy = -(y1 - y0).abs();
  final sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
  var err = dx + dy;
  while (true) {
    smallPaint(x0, y0, smallPaintValue);
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

/// Fills the connected area of equal pixels around [start]; only pixels with a tile behind them change.
void smallFill((int, int) start, int value) {
  final w = smallLayerWidth, h = smallLayerHeight;
  final base = Uint8List.fromList(smallLayerPixels());
  final from = base[start.$2 * w + start.$1];
  if (from == value) return;
  final seen = List.filled(w * h, false);
  final todo = [start];
  while (todo.isNotEmpty) {
    final (x, y) = todo.removeLast();
    if (x < 0 || y < 0 || x >= w || y >= h || seen[y * w + x] || base[y * w + x] != from || !smallEditable(x, y)) continue;
    seen[y * w + x] = true;
    smallPaint(x, y, value);
    todo.addAll([(x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)]);
  }
}

void pushSmallHistory() {
  smallHistory.add(smallEd!.snapshot());
  if (smallHistory.length > 200) smallHistory.removeAt(0);
}

void undoSmall() {
  final ed = smallEd;
  if (ed == null || smallHistory.isEmpty) return;
  ed.restore(smallHistory.removeLast());
  refreshSmall();
}

/// Notes, swatches, preview and canvas.
void refreshSmall() {
  final r = dst.reader, ed = smallEd, t = smallTeam;
  if (r == null || ed == null || t == null) return;
  byId<web.HTMLElement>('seTitle').textContent = smallTitle;
  final also = ed.alsoAffects;
  final items = <String>[
    for (final n in smallNotes) '<li class="warn">${esc(n)}</li>',
    if (!ed.changed && smallNotes.isEmpty) '<li>No changes yet.</li>',
    if (ed.changed && also.isEmpty) '<li class="ok">Only this helmet changes.</li>',
    if (also.isNotEmpty)
      '<li class="warn">Also changes ${esc(also.map(cap).join(', '))} (they use the same tiles).</li>',
    if (!ed.hasOverlay && smallLayer == 'overlay') '<li>This helmet has no overlay sprites.</li>',
    if (smallLayer == 'bg' && !ed.bgEditable(0, 0) && !ed.bgEditable(8, 0) && !ed.bgEditable(0, 8) && !ed.bgEditable(8, 8))
      '<li>This helmet has no background tiles.</li>',
  ];
  byId<web.HTMLElement>('seNotes').innerHTML = items.join().toJS;
  byId<web.HTMLButtonElement>('seApply').disabled = !ed.changed;
  byId<web.HTMLButtonElement>('seUndo').disabled = smallHistory.isEmpty;
  byId<web.HTMLButtonElement>('seRevert').disabled = !ed.changed;
  byId<web.HTMLButtonElement>('sLayerOv').disabled = false;
  showSmallSwatches();
  showSmallPreview();
  drawSmall();
}

/// Layer colors 0-3 (color 0 = clear) and the four palette groups.
void showSmallSwatches() {
  final ed = smallEd!;
  final base = smallLayer == 'bg' ? ed.paletteGroup * 4 : 0x10 + ed.overlayPaletteAt(smallBoxX(), smallBoxY()) * 4;
  final row = byId<web.HTMLElement>('seColors')..innerHTML = ''.toJS;
  row.append(web.document.createElement('span')..textContent = 'Color');
  for (var k = 0; k < 4; k++) {
    final nes = ed.paletteRam[base + k];
    final b = web.document.createElement('button') as web.HTMLButtonElement
      ..className = 'sw${k == 0 ? ' clear' : ''}${k == smallColor ? ' sel' : ''}'
      ..textContent = k == 0 ? 'none' : '$k'
      ..title = k == 0 ? 'Clear (right-click also erases)' : 'Color $k: ${hex(nes)}';
    if (k > 0) {
      b.style
        ..background = css(nes)
        ..color = isLight(nes) ? '#000' : '#fff';
    }
    final value = k;
    b.onclick = ((web.Event _) {
      smallColor = value;
      showSmallSwatches();
    }).toJS;
    row.append(b);
  }

  final groups = byId<web.HTMLElement>('sePalGroups')..innerHTML = ''.toJS;
  groups.append(web.document.createElement('span')..textContent = 'Palette group');
  final pals = ed.tilePalettes;
  final uniform = pals.every((p) => p == pals[0]);
  for (var g = 0; g < 4; g++) {
    final chips = [for (var k = 1; k < 4; k++) '<i style="background:${css(ed.paletteRam[g * 4 + k])}"></i>'].join();
    final b = web.document.createElement('button') as web.HTMLButtonElement
      ..className = 'sw${uniform && pals[0] == g ? ' sel' : ''}'
      ..title = 'Use color group ${g + 1} (shared by every helmet that uses it)'
      ..innerHTML = '<span class="sechips">$chips</span>'.toJS;
    b.style.width = '64px';
    final group = g;
    b.onclick = ((web.Event _) {
      if (ed.tilePalettes.every((p) => p == group)) return;
      pushSmallHistory();
      ed.setPalette(group);
      refreshSmall();
    }).toJS;
    groups.append(b);
  }
  if (!uniform) {
    groups.append(web.document.createElement('span')
      ..className = 'muted'
      ..textContent = 'This helmet\'s left and right halves use different groups; picking one sets all four tiles.');
  }
}

/// Which overlay sprite the swatches should take their palette from (the first one).
int smallBoxX() => smallEd!.overlayBoxes.isEmpty ? 0 : smallEd!.overlayBoxes.first.$1;
int smallBoxY() => smallEd!.overlayBoxes.isEmpty ? 0 : smallEd!.overlayBoxes.first.$2;

/// What the game will draw before and after the edit.
void showSmallPreview() {
  final r = dst.reader, ed = smallEd, t = smallTeam;
  if (r == null || ed == null || t == null) return;
  draw(byId('seBefore'), renderSmallHelmetIndexes(r.rom, SmallHelmetReader(r.rom).read(t)));
  final after = NesRom(ed.build());
  draw(byId('seAfter'), renderSmallHelmetIndexes(after, SmallHelmetReader(after).read(t)));
}

/// RGBA for NES color indexes where 0xFF means transparent.
Uint8ClampedList nesToRgba(Uint8List nes) {
  final out = Uint8ClampedList(nes.length * 4);
  for (var i = 0; i < nes.length; i++) {
    if (nes[i] == 0xFF) continue;
    final c = nesPalette[nes[i] & 0x3F];
    out[i * 4] = c >> 16;
    out[i * 4 + 1] = (c >> 8) & 0xFF;
    out[i * 4 + 2] = c & 0xFF;
    out[i * 4 + 3] = 0xFF;
  }
  return out;
}

web.HTMLCanvasElement nesCanvas(Uint8List nes, int w, int h) {
  final c = web.document.createElement('canvas') as web.HTMLCanvasElement
    ..width = w
    ..height = h;
  (c.getContext('2d')! as web.CanvasRenderingContext2D)
      .putImageData(web.ImageData(nesToRgba(nes).toJS, w, h.toJS), 0, 0);
  return c;
}

void drawSmall() {
  final ed = smallEd;
  if (ed == null) return;
  final w = smallLayerWidth, h = smallLayerHeight, z = smallZoom;
  final cv = byId<web.HTMLCanvasElement>('seCanvas');
  if (cv.width != w * z) cv.width = w * z;
  if (cv.height != h * z) cv.height = h * z;
  final ctx = cv.getContext('2d')! as web.CanvasRenderingContext2D
    ..imageSmoothingEnabled = false
    ..globalAlpha = 1;
  ctx
    ..fillStyle = css(ed.paletteRam[0]).toJS
    ..fillRect(0, 0, w * z, h * z);

  final isBg = smallLayer == 'bg';
  if (byId<web.HTMLInputElement>('seGuide').checked) {
    // The other layer, where the game would put it (the background is drawn 1 line lower than the sprites).
    ctx.globalAlpha = 0.35;
    if (isBg) {
      ctx.drawImage(nesCanvas(ed.overlayNes(), smallHelmetWidth, smallHelmetHeight), -smallHelmetMarginX * z, -z,
          smallHelmetWidth * z, smallHelmetHeight * z);
    } else {
      ctx.drawImage(nesCanvas(ed.bgNes(), smallBgSize, smallBgSize), smallHelmetMarginX * z, z, smallBgSize * z,
          smallBgSize * z);
    }
    ctx.globalAlpha = 1;
  }
  ctx.drawImage(isBg ? nesCanvas(ed.bgNes(), w, h) : nesCanvas(ed.overlayNes(), w, h), 0, 0, w * z, h * z);

  // Areas with no tile behind them can't be drawn on.
  ctx.fillStyle = 'rgba(0, 0, 0, 0.5)'.toJS;
  for (var y = 0; y < h; y++) {
    for (var x = 0; x < w; x++) {
      if (!smallEditable(x, y)) ctx.fillRect(x * z, y * z, z, z);
    }
  }
  if (byId<web.HTMLInputElement>('seGrid').checked) {
    ctx.lineWidth = 1;
    if (isBg) {
      ctx.strokeStyle = 'rgba(255, 220, 0, 0.75)'.toJS;
      for (final (x, y) in [(0, 0), (8, 0), (0, 8), (8, 8)]) {
        ctx.strokeRect(x * z + 0.5, y * z + 0.5, 8 * z - 1, 8 * z - 1);
      }
    } else {
      ctx.strokeStyle = 'rgba(255, 220, 0, 0.75)'.toJS;
      for (final (x, y) in ed.overlayBoxes) {
        ctx.strokeRect(x * z + 0.5, y * z + 0.5, 8 * z - 1, 8 * z - 1);
      }
    }
  }
}

// ---- apply, export, import ----

void applySmall() {
  final r = dst.reader, ed = smallEd, t = smallTeam;
  if (r == null || ed == null || t == null || !ed.changed) return;
  final also = {for (final n in ed.alsoAffects) r.teams.indexOf(n)};
  commit(ed.build(), {}, 'Changed ${r.teams[t]}\'s small helmet. Remember to save the target ROM.', small: {t, ...also});
  closeSmall();
  refresh();
}

void exportSmall() {
  final ed = smallEd;
  if (ed == null) return;
  final c = web.document.createElement('canvas') as web.HTMLCanvasElement
    ..width = smallImageWidth
    ..height = smallImageHeight;
  (c.getContext('2d')! as web.CanvasRenderingContext2D).putImageData(
      web.ImageData(Uint8ClampedList.fromList(smallHelmetImageRgba(ed)).toJS, smallImageWidth, smallImageHeight.toJS), 0, 0);
  final a = web.document.createElement('a') as web.HTMLAnchorElement
    ..href = c.toDataURL('image/png')
    ..download = '${ed.teamName}-small-helmet.png';
  web.document.body!.append(a);
  a.click();
  a.remove();
}

/// Reads a picture into both layers (transparent pixels clear, everything
/// else snaps to the nearest of the layer's three colors).
Future<void> importSmall(web.File f) async {
  final ed = smallEd;
  if (ed == null) return;
  try {
    final bmp = await web.window.createImageBitmap(f).toDart;
    if (bmp.width * smallImageHeight != bmp.height * smallImageWidth) {
      smallNotes = ['The picture is ${bmp.width}×${bmp.height}; it must be $smallImageWidth×$smallImageHeight '
          '(or a multiple of that, like an exported picture scaled up).'];
      refreshSmall();
      return;
    }
    final c = web.document.createElement('canvas') as web.HTMLCanvasElement
      ..width = smallImageWidth
      ..height = smallImageHeight;
    final ctx = c.getContext('2d')! as web.CanvasRenderingContext2D
      ..imageSmoothingEnabled = false;
    ctx.drawImage(bmp, 0, 0, smallImageWidth, smallImageHeight);
    final data = ctx.getImageData(0, 0, smallImageWidth, smallImageHeight).data.toDart;

    final back = readSmallHelmetImage(Uint8List.fromList(data), ed);
    pushSmallHistory();
    final notes = [
      if (bmp.width != smallImageWidth) 'The picture was ${bmp.width}×${bmp.height}; it was scaled to $smallImageWidth×$smallImageHeight.',
      ...back.notes,
    ];
    final droppedBg = ed.applyBgPixels(back.bg);
    final droppedOv = ed.applyOverlayPixels(back.overlay);
    if (droppedBg > 0) notes.add('$droppedBg background pixel${droppedBg == 1 ? '' : 's'} had no tile to go in and were left out.');
    if (droppedOv > 0) notes.add('$droppedOv overlay pixel${droppedOv == 1 ? '' : 's'} were outside every sprite and were left out.');
    smallNotes = notes;
    refreshSmall();
  } catch (e) {
    smallNotes = ['Could not read ${f.name}: $e'];
    refreshSmall();
  }
}
