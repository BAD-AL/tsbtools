import { dotnet } from './_framework/dotnet.js';
import { GROUPS, FORMATIONS, parseText, parsePlayerLine, buildPlayerLine, parseTeamLine, buildTeamLine, parsePlaybook, buildPlaybook, teamLabel, changedLines } from './model.js';
import * as U from './uniform.js';
import * as S from './schedule.js';
import * as P from './photos.js';
import { LADDER, FULL, familyOf, faceKind, isDark, withTone, labelsFor, ladderIndex } from './schema.js';

const state = {
  ready: false, busy: '', error: '',
  idTotal: 0, idMax: { city: 0, name: 0 },   // team-name budget: total characters in the stock ROM, longest stock city/name
  usel: 0, upick: null, usage: null, uniLoading: false, uniError: false,   // uniform editor: selected uniform, open color picker, usage dialog
  auto: null, autoUndo: null,        // Auto-update sim: dialog { stage: 'ask'|'done'|'error', ... } and the text from before the last run
  accent: null, accentDlg: false,      // Modern style highlight color (null = gold) and its dialog
  pb: null, pbconf: 'AFC', pbEdit: null,   // Pro Bowl rosters read from the core, current conference, open spot dialog
  photoDlg: null, photoEd: null, notice: null,   // TSB3 photos: detail dialog, picture editor; a plain message dialog
  sweek: 0, sview: 'week', steam: null, steamDlg: false, sgame: null,   // schedule: current week, view (week/team), team for the team view, open game dialog
  ttab: 'sim', playPick: null,   // team editor: current tab, open play picker { key, i }
  tedit: null,          // draft of the team being edited: { team, line, pbLine, t, pb, orig }
  edit: null,           // draft of the player being edited: { line, orig, cur, first, last, sel }
  pending: false,       // text has edits the core has not seen yet (applied lazily, before download)
  theme: 'modern',
  rom: null,            // { name, version, ext }
  text: '', baseline: '', parsed: null,
  team: null, group: 'QB', sheet: false,
  messages: '', find: '',
};
let web = null;
const $ = sel => document.querySelector(sel);
const esc = s => String(s).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

try { const t = localStorage.getItem('tsb.theme'); if (t === 'retro' || t === 'modern') state.theme = t; } catch (e) { /* storage may be blocked */ }
try { const a = localStorage.getItem('tsb.accent'); if (/^#[0-9a-f]{6}$/i.test(a || '')) state.accent = a.toLowerCase(); } catch (e) { /* storage may be blocked */ }

const ICON = {
  photo: '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="4" y="3" width="16" height="18" rx="2"/><circle cx="12" cy="10" r="3"/><path d="M6 20c0-3 2.7-5 6-5s6 2 6 5"/></svg>',
  home: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10"/></svg>',
  players: '<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4"/><path d="M4 21c0-4 3.6-7 8-7s8 3 8 7"/></svg>',
  teams: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3 4 6v6c0 4.5 3.4 8 8 9 4.6-1 8-4.5 8-9V6z"/></svg>',
  schedule: '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M3 10h18M8 3v4M16 3v4"/></svg>',
  probowl: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m12 3 2.6 5.6 6.1.7-4.5 4.2 1.2 6L12 16.5 6.6 19.5l1.2-6L3.3 9.3l6.1-.7z"/></svg>',
  text: '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m8 8-5 4 5 4M16 8l5 4-5 4M14 5l-4 14"/></svg>',
  left: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m15 5-7 7 7 7"/></svg>',
  right: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m9 5 7 7-7 7"/></svg>',
  up: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 15 6-6 6 6"/></svg>',
  down: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6"/></svg>',
  dl: '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3v12M7 10l5 5 5-5M4 21h16"/></svg>',
};
// Face portraits are one sprite sheet (img/faces.png) sliced with CSS; only NES/CXROM have per-player portraits.
const sheets = { faces: null, plays: null };
async function loadSheets() {
  try {
    const [json, img] = await Promise.all([fetch('img/faces.json').then(r => r.json()), new Promise((res, rej) => {
      const i = new Image(); i.onload = () => res(i); i.onerror = rej; i.src = 'img/faces.png';
    })]);
    const index = {};
    for (const e of json.Images) index[e.name.toUpperCase()] = e;
    sheets.faces = { w: img.naturalWidth, h: img.naturalHeight, index };
    render();
  } catch (e) { /* no sprites: silhouettes stay */ }
  try {                                   // play diagrams: R1-0..R4-7 (run) and P1-0..P4-7 (pass)
    const [json, img] = await Promise.all([fetch('img/plays_tsb1.json').then(r => r.json()), new Promise((res, rej) => {
      const i = new Image(); i.onload = () => res(i); i.onerror = rej; i.src = 'img/plays_tsb1.png';
    })]);
    const index = {};
    for (const [k, v] of Object.entries(json)) index[k.toUpperCase()] = v;
    sheets.plays = { w: img.naturalWidth, h: img.naturalHeight, index };
    render();
  } catch (e) { /* play names show as text */ }
}

/** A face box: the portrait sprite when this ROM has one for `face` (number or '0x..' text), else the silhouette. */
function faceEntry(n) {
  return sheets.faces && sheets.faces.index[n.toString(16).toUpperCase().padStart(2, '0') + '.BMP'];
}
function spriteHtml(e) {
  const k = 2, s = sheets.faces;
  return `<span class="sprite" style="width:${e.width * k}px;height:${e.height * k}px;background-position:-${e.x * k}px -${e.y * k}px;background-size:${s.w * k}px ${s.h * k}px"></span>`;
}
function faceBox(face, big, team, pos) {
  if (P.cache.on && team) {                         // TSB3: the player's photo, when his slot has one
    const ph = P.photoOf(team, pos);
    if (ph !== undefined) return `<div class="face sp ${big ? 'big' : ''}">${photoImg(ph, 2)}</div>`;
  }
  const n = typeof face === 'string' ? parseInt(face, 16) : face;
  const e = state.rom && faceKind(state.rom.version).kind === 'portrait' && faceEntry(n);
  if (!e) return `<div class="face ${big ? 'big' : ''}">${SILHOUETTE}</div>`;
  return `<div class="face sp ${big ? 'big' : ''}">${spriteHtml(e)}</div>`;
}
/** Bottom sheet with every portrait in the sheet; the current one is highlighted. */
function facePickerSheet(current) {
  const ids = Object.values(sheets.faces.index).map(e => parseInt(e.name, 16)).sort((a, b) => a - b)
    .filter((v, i, a) => a.indexOf(v) === i);
  const cells = ids.map(id => `<button class="facebtn ${id === current ? 'on' : ''}" data-act="facepick:${id}" aria-label="Face 0x${id.toString(16)}">
      <span class="face sp">${spriteHtml(faceEntry(id))}</span><span class="fid">${id.toString(16).toUpperCase().padStart(2, '0')}</span></button>`).join('');
  return `<div class="scrim pop" data-act="close-face"><div class="sheet pop" role="dialog" aria-label="Pick a face" data-stop="1">
      <div class="topbar"><h2>Pick a face</h2><button class="btn icon-btn" data-act="close-face" aria-label="Close">&times;</button></div>
      <div class="muted" style="margin-top:6px">${ids.length} faces. Tap one to use it.</div>
      <div class="facegrid">${cells}</div></div></div>`;
}

const SILHOUETTE ='<svg viewBox="0 0 40 46" aria-hidden="true"><circle cx="20" cy="17" r="10" fill="#0b1220"/><path d="M3 46c0-12 7-18 17-18s17 6 17 18z" fill="#0b1220"/></svg>';

// ---------- routing / rendering ----------
const route = () => (location.hash.replace(/^#\/?/, '') || 'home').split('/')[0];

function themeSwitch() {
  return `<div class="seg" role="group" aria-label="Style">
    <button data-act="theme:modern" class="${state.theme === 'modern' ? 'on' : ''}">Modern</button>
    <button data-act="theme:retro" class="${state.theme === 'retro' ? 'on' : ''}">Retro</button></div>`;
}

function banner() {
  const m = (state.error || state.messages || '').trim();
  if (!m) return '';
  const cut = m.search(/\.\s|\n/);                 // first sentence is the summary, the rest folds away
  if (cut < 0 || cut >= m.length - 2) return `<div class="banner" role="status">${esc(m)}</div>`;
  return `<details class="banner" role="status"><summary>${esc(m.slice(0, cut + 1))}</summary><div class="detail">${esc(m.slice(cut + 1).trim())}</div></details>`;
}

function homeScreen() {
  const art = `<div class="title-art"><span class="eyebrow only-retro">TECMO</span>
    <span class="big only-retro">SUPER<br>BOWL</span><span class="eyebrow only-retro">TOOL</span>
    <h1 class="only-modern">TSBTool</h1><div class="muted only-modern">Tecmo Super Bowl editor</div></div>`;
  if (!state.rom) {
    return `<div class="hdr"><div class="topbar"><span></span>${themeSwitch()}</div></div>${art}${banner()}
      <div class="dropzone" id="drop">
        <button class="btn primary" data-act="load" ${state.ready ? '' : 'disabled'}>${state.ready ? 'Load ROM' : 'Starting...'}</button>
        <div class="muted">or drop a ROM file here</div>
        <div class="muted">NES / CXROM, SNES TSB1-3, Genesis</div>
      </div>
      <div class="muted" style="text-align:center">Your ROM never leaves this device.</div>`;
  }
  const n = changedLines(state.baseline, state.text);
  const tile = (key, label, sub, go) => `<button class="btn tile" ${go ? `data-act="go:${go}"` : 'disabled'}>${ICON[key]}<span>${label}<br><span class="sub">${go ? sub : 'Coming soon'}</span></span></button>`;
  return `<div class="hdr"><div class="topbar"><span></span>${themeSwitch()}</div></div>${art}${banner()}
    <div class="card"><div class="eyebrow"><span class="dot"></span>ROM loaded</div>
      <h2 style="margin-top:8px;text-transform:none;word-break:break-all">${esc(state.rom.name)}</h2>
      <div class="muted" style="margin-top:6px">${esc(state.rom.version)} &middot; ${realTeams().length} teams</div>
      <div style="margin-top:10px"><button class="btn ghost small" data-act="load">Change ROM</button></div></div>
    <div class="muted" style="text-transform:uppercase;letter-spacing:.08em">Edit</div>
    <div class="tile-grid">
      ${tile('players', 'Players', 'Rosters by team', 'roster')}
      ${tile('teams', 'Teams', 'Sim, formation, plays', 'team')}
      ${tile('schedule', 'Schedule', 'Week by week matchups', 'schedule')}
      ${tile('probowl', 'Pro Bowl', 'AFC and NFC rosters', 'probowl')}
    </div>
    ${P.cache.on ? `<button class="btn ghost" data-act="go:photos">${ICON.photo} Player photos</button>` : ''}
    <button class="btn ghost" data-act="go:text">${ICON.text} Text view (power users)</button>
    <div class="bar"><div><div style="flex:0 0 auto;align-self:center" class="muted">${n ? n + ' lines changed' : 'No edits yet'}</div>
      <button class="btn primary" data-act="download">${ICON.dl} Download ROM</button></div></div>`;
}

function rosterScreen() {
  const teams = state.parsed.teams;
  const team = teams.find(t => t.id === state.team) || teams[0];
  const players = groupPlayers(team);
  const rows = players.map(p => `<button class="row ${state.edit && state.edit.line === p.lineIndex ? 'cur' : ''}" data-act="player:${p.lineIndex}">${faceBox(p.face, false, team.id, p.pos)}
      <div class="grow"><div class="eyebrow">${esc(p.pos)} &middot; FACE ${esc(p.face)}</div><div class="nm">${esc(p.name)}</div></div>
      <div class="num">${p.jersey}</div></button>`).join('') || '<div class="status">No players in this group.</div>';
  return `<div class="hdr"><div class="topbar"><button class="btn ghost small" data-act="go:home">${ICON.left} Home</button>${themeSwitch()}</div>
    ${teamHead(team)}
    <div class="chips" role="group" aria-label="Position group">${['ALL', ...GROUPS].map(g => `<button class="chip ${g === state.group ? 'on' : ''}" data-act="group:${g}">${g}</button>`).join('')}</div></div>
    ${team.real ? '<button class="btn ghost small" data-act="go:team">Edit team settings</button>' : ''}
    <div class="list">${rows}</div>
    <div class="muted" style="text-align:center">Tap a player to edit.</div>${teamPicker(teams, team)}`;
}

function teamHead(team) {
  return `<div class="team-head"><button class="btn icon-btn" data-act="team-prev" aria-label="Previous team">${ICON.left}</button>
      <button class="btn name" data-act="team-open" style="flex-grow:1"><span class="swatch"></span>${esc(teamLabel(team.id))}${ICON.down}</button>
      <button class="btn icon-btn" data-act="team-next" aria-label="Next team">${ICON.right}</button></div>`;
}

/** Centered dialog listing every team in the ROM. */
function teamPicker(teams, team) {
  if (!state.sheet) return '';
  return `<div class="scrim pop" data-act="close-sheet"><div class="sheet pop" role="dialog" aria-label="Pick a team" data-stop="1">
      <div class="topbar"><h2>Pick a team</h2><button class="btn icon-btn" data-act="close-sheet" aria-label="Close">&times;</button></div>
      <div class="picker">${teams.map(t => `<button class="btn ${t.id === team.id ? 'on' : ''}" data-act="team:${esc(t.id)}">${esc(teamLabel(t.id))}</button>`).join('')}</div></div></div>`;
}

// ---------- team editor ----------
const ID_RE = /^TEAM_ABB=([^,]*),TEAM_CITY=([^,]*),TEAM_NAME=(.*)$/;
const ID_FIELDS = {
  abb:  { label: 'Abbreviation', bad: /[^0-9A-Z. ]/g, upper: true },
  city: { label: 'City', bad: /[^0-9A-Za-z .]/g, upper: false },
  name: { label: 'Team name', bad: /[^0-9A-Z .]/g, upper: true },
};

/** Characters used by every team's abbreviation + city + name, optionally leaving one team out. */
function idUsed(lines, teams, skipLine) {
  let n = 0;
  for (const t of teams) {
    const li = t.lineIndex + 2;
    if (li === skipLine) continue;
    const m = ID_RE.exec(lines[li] || '');
    if (m) n += m[1].length + m[2].length + m[3].length;
  }
  return n;
}

function idStats() {
  const { lines, teams } = state.parsed;
  state.idTotal = idUsed(lines, teams, -1);
  let c = 0, nm = 0;
  for (const t of teams) { const m = ID_RE.exec(lines[t.lineIndex + 2] || ''); if (m) { c = Math.max(c, m[2].length); nm = Math.max(nm, m[3].length); } }
  state.idMax = { city: c, name: nm };
}

const PREFS = [['1', 'Heavy rush'], ['0', 'Balanced rush'], ['2', 'Balanced pass'], ['3', 'Heavy pass']];

function openTeamDraft(id) {
  const team = realTeams().find(t => t.id === id) || realTeams()[0];
  const fam = family(), lines = state.parsed.lines;
  const parts = parseTeamLine(lines[team.lineIndex]);
  const d = parts.digits;
  const t = { parts, formation: parts.formation, pref: null, max: 15, upper: fam !== 'tsb1' };
  if (fam === 'tsb3') { t.rush = parseInt(d.slice(0, 2), 16); t.pass = parseInt(d.slice(2, 4), 16); t.max = 255; }
  else { t.rush = parseInt(d[0], 16); t.pass = parseInt(d[1], 16); t.pref = d.length > 2 ? d[2] : null; }
  const pbLine = team.lineIndex + 1;
  const pb = parsePlaybook(lines[pbLine]);
  const im = state.rom.version === 'GENESIS_TSB1' ? null : ID_RE.exec(lines[team.lineIndex + 2] || '');   // Genesis ignores these lines
  const ident = im ? { others: idUsed(lines, state.parsed.teams, team.lineIndex + 2), line: team.lineIndex + 2, abb: im[1], city: im[2], name: im[3] } : null;
  state.tedit = { team: team.id, line: team.lineIndex, pbLine, t, pb, id: ident, orig: { t: structuredClone(t), pb: structuredClone(pb), id: structuredClone(ident) } };
  let ci = -1;                                           // the COLORS line sits within a few lines after TEAM
  for (let k = team.lineIndex + 1; k <= team.lineIndex + 6; k++) {
    const l = lines[k];
    if (l === undefined || /^TEAM\s*=/i.test(l)) break;
    if (/^COLORS\b/.test(l)) { ci = k; break; }
  }
  const cparts = ci >= 0 ? U.parseColors(lines[ci]) : null, ckind = U.colorKind(state.rom.version);
  state.tedit.colors = cparts && ckind ? { line: ci, parts: cparts, kind: ckind } : null;
  state.tedit.orig.colors = structuredClone(state.tedit.colors);
  state.team = team.id;
}

function teamDigits(t) {
  const h = n => { const s = n.toString(16); return t.upper ? s.toUpperCase() : s; };
  if (t.max === 255) return h(t.rush).padStart(2, '0') + h(t.pass).padStart(2, '0');
  return h(t.rush) + h(t.pass) + (t.pref === null ? '' : t.pref);
}

function commitTeam() {
  const d = state.tedit;
  if (!d) return;
  const lines = state.parsed.lines;
  const next = buildTeamLine({ ...d.t.parts, digits: teamDigits(d.t), formation: d.t.formation });
  let changed = false;
  if (next !== lines[d.line]) { lines[d.line] = next; changed = true; }
  if (d.pb) {
    const pl = buildPlaybook(d.pb);
    if (pl !== lines[d.pbLine]) { lines[d.pbLine] = pl; changed = true; }
  }
  if (d.id) {
    const o = d.orig.id;
    if (d.id.city.trim().length < 2) d.id.city = o.city;          // the core refuses empty/1-letter names
    if (d.id.name.trim().length < 2) d.id.name = o.name;
    if (d.id.abb.length === 0) d.id.abb = o.abb;
    d.id.abb = d.id.abb.padEnd(4, '.');                             // the abbreviation is always 4 characters
    const il = `TEAM_ABB=${d.id.abb},TEAM_CITY=${d.id.city},TEAM_NAME=${d.id.name}`;
    if (il !== lines[d.id.line]) { lines[d.id.line] = il; changed = true; }
  }
  if (d.colors) {
    const cl = U.buildColors(d.colors.parts);
    if (cl !== lines[d.colors.line]) { lines[d.colors.line] = cl; changed = true; }
  }
  if (changed) { state.text = lines.join('\n'); state.pending = true; }
}

function playSprite(name, k = 1.5) {
  const s = sheets.plays, e = s && s.index[name];
  if (!e) return `<div class="playimg none">${esc(name.replace('.BMP', ''))}</div>`;
  return `<div class="playimg"><span class="sprite play" style="width:${e.width * k}px;height:${e.height * k}px;background-position:-${e.x * k}px -${e.y * k}px;background-size:${s.w * k}px ${s.h * k}px"></span></div>`;
}

function teamTabs(d) {
  const tabs = [['sim', 'General'], ['plays', 'Plays'], ...(d.id ? [['identity', 'Identity']] : []), ['uniform', 'Uniform']];
  if (!tabs.some(([k]) => k === state.ttab)) state.ttab = 'sim';
  return `<div class="seg wide tabs" role="tablist" aria-label="Team sections">${tabs.map(([k, l]) =>
    k === 'uniform' && !d.colors ? `<button disabled title="Not available for this ROM">${l}</button>`
      : `<button role="tab" aria-selected="${state.ttab === k}" class="${state.ttab === k ? 'on' : ''}" data-act="ttab:${k}">${l}</button>`).join('')}</div>`;
}

function teamSimTab(d) {
  const t = d.t, fam = family();
  const show = n => (t.max === 255 ? n.toString(16).toUpperCase().padStart(2, '0') : String(n));
  const simBox = (label, key) => `<div class="card simbox"><span class="k">${label}</span>
      <div class="stepper"><button class="btn icon-btn" data-act="tsim:${key}:-1" aria-label="${label} down">&minus;</button><span class="val">${show(t[key])}</span>
      <button class="btn icon-btn" data-act="tsim:${key}:1" aria-label="${label} up">+</button></div></div>`;
  const sims = fam === 'tsb3' ? [simBox('RUSH D BONUS', 'rush'), simBox('PASS D BONUS', 'pass')] : [simBox('SIM RUSH DEFENSE', 'rush'), simBox('SIM PASS DEFENSE', 'pass')];
  const pref = t.pref === null ? '' : `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">Run / pass ratio</div>
    <div class="seg wide" role="group" aria-label="Run pass ratio">${PREFS.map(([v, l]) => `<button class="${t.pref === v ? 'on' : ''}" data-act="tpref:${v}">${l}</button>`).join('')}</div>
    ${state.rom.version === 'GENESIS_TSB1' ? '<div class="muted">On Genesis, "Heavy rush" plays as heavy pass unless GENESIS_FIX_SIM_BUGS is in the text.</div>' : ''}`;
  const form = t.formation === null ? '' : `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">Formation</div>
    <div class="seg wide three" role="group" aria-label="Formation">${FORMATIONS.map(f => `<button class="${f === t.formation ? 'on' : ''}" data-act="tform:${f}">${f.replace(/_/g, ' ')}</button>`).join('')}</div>`;
  const auto = `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">Sim data</div>
    <button class="btn ghost" data-act="autosim">Auto-update sim data&hellip;</button>
    <div class="muted">Works out the sim values of every player and every team from their ratings. Values set by hand are replaced.</div>`;
  return `<div class="simgrid">${sims.join('')}</div>${pref}${form}${auto}`;
}

function teamPlaysTab(d) {
  if (!d.pb) return `<div class="muted">Playbook editing for this ROM is not available yet. Current: ${esc((state.parsed.lines[d.pbLine] || '').trim())}</div>`;
  const row = (title, kind, key) => `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">${title}</div>
    <div class="playrow">${[0, 1, 2, 3].map(i => `<button class="playbtn" data-act="playopen:${key}:${i}" aria-label="${title} slot ${i + 1}, play ${d.pb[key][i]}">
      ${playSprite(`${kind}${i + 1}-${d.pb[key][i] - 1}.BMP`)}<span class="k">${kind}${i + 1} &middot; play ${d.pb[key][i]}</span></button>`).join('')}</div>`;
  return row('Run plays', 'R', 'run') + row('Pass plays', 'P', 'pass') + '<div class="muted">Tap a play to choose another for that slot.</div>';
}

/** How many more characters this team's city and name may use: ROM-wide budget, minus everyone else, minus the 4-letter abbreviation. */
function idFree(d) { return Math.max(0, state.idTotal - d.id.others - 4); }
function idLimit(d, key) {
  if (key === 'abb') return 4;
  const other = key === 'city' ? d.id.name.length : d.id.city.length;
  return Math.max(0, Math.min(state.idMax[key], idFree(d) - other));
}

function teamIdentityTab(d) {
  const f = key => {
    const v = d.id[key], max = idLimit(d, key) || (key === 'abb' ? 4 : 0);
    return `<label class="field"><span class="fl">${ID_FIELDS[key].label}<span class="cnt" id="cnt-${key}">${v.length}/${Math.max(max, v.length)}</span></span>
      <input data-idf="${key}" value="${esc(v)}" maxlength="${Math.max(max, v.length)}" autocomplete="off" autocapitalize="${ID_FIELDS[key].upper ? 'characters' : 'off'}" spellcheck="false"></label>`;
  };
  return `<div class="idgrid">${f('abb')}${f('city')}${f('name')}</div>
    <div class="muted" id="idinfo">${idInfo(d)}</div>`;
}

function idInfo(d) {
  const used = d.id.others + d.id.abb.length + d.id.city.length + d.id.name.length;
  return `Team names use ${used} of ${state.idTotal} characters in this ROM. The abbreviation is always 4 characters (short ones are padded with .), the city and name need at least 2. To make one name longer, shorten another team's.`;
}

/** Keeps a text box inside the rules while typing: allowed characters, case, and the ROM-wide length budget. */
function idTyped(t) {
  const d = state.tedit, key = t.dataset.idf, rule = ID_FIELDS[key];
  let v = t.value;
  if (rule.upper) v = v.toUpperCase();
  v = v.replace(rule.bad, '');
  d.id[key] = '';                                   // measure the limit without this field's own text
  v = v.slice(0, idLimit(d, key));
  d.id[key] = v;
  if (t.value !== v) t.value = v;
  for (const k of Object.keys(ID_FIELDS)) {         // one field's length changes what the others may use
    const len = d.id[k].length, max = Math.max(idLimit(d, k), len);
    const cnt = document.getElementById('cnt-' + k), box = document.querySelector(`[data-idf="${k}"]`);
    if (cnt) cnt.textContent = `${len}/${max}`;
    if (box) box.maxLength = max;
  }
  const info = document.getElementById('idinfo');
  if (info) info.textContent = idInfo(d);
}

/** Centered dialog with the 8 plays available to one playbook slot. */
function playPickerSheet(d) {
  const { key, i } = state.playPick, kind = key === 'run' ? 'R' : 'P', cur = d.pb[key][i];
  const cells = Array.from({ length: 8 }, (_, n) => `<button class="playopt ${n + 1 === cur ? 'on' : ''}" data-act="playpick:${n + 1}" aria-label="Play ${n + 1}">
      ${playSprite(`${kind}${i + 1}-${n}.BMP`)}<span class="k">${n + 1}</span></button>`).join('');
  return `<div class="scrim pop" data-act="close-play"><div class="sheet pop" role="dialog" aria-label="Pick a play" data-stop="1">
      <div class="topbar"><h2>${kind}${i + 1} play</h2><button class="btn icon-btn" data-act="close-play" aria-label="Close">&times;</button></div>
      <div class="playpick">${cells}</div></div></div>`;
}

function teamScreen() {
  const d = state.tedit;
  const teams = realTeams(), team = teams.find(x => x.id === state.team);
  const body = state.ttab === 'uniform' && d.colors ? teamUniformTab(d) : state.ttab === 'plays' ? teamPlaysTab(d) : state.ttab === 'identity' && d.id ? teamIdentityTab(d) : teamSimTab(d);
  return `<div class="hdr"><div class="topbar"><button class="btn ghost small" data-act="go:roster">${ICON.left} Roster</button><button class="btn ghost small" data-act="tundo">Undo</button></div>
    <h1>Edit team</h1>${teamHead(team)}${teamTabs(d)}</div><div class="tabbody">${body}</div>
    <div class="bar"><div><button class="btn ghost" data-act="team-prev">${ICON.left} Prev</button>
      <button class="btn primary" data-act="tdone">Done</button>
      <button class="btn ghost" data-act="team-next">Next ${ICON.right}</button></div></div>${teamPicker(teams, team)}${state.playPick && d.pb ? playPickerSheet(d) : ''}${uniSheets(d)}${autoSheet()}`;
}

// ---------- uniform editor ----------
let paintPlan = [];

function uniChip(act, label, rgb, hex) {
  return `<button class="colorchip" data-act="${act}"><span class="sw" style="background:${U.css(rgb)}"></span><span class="cl">${label}</span><span class="hx">${esc(hex)}</span></button>`;
}

function teamUniformTab(d) {
  const c = d.colors, kind = c.kind;
  if (!U.assetsReady()) {
    if (!state.uniLoading) { state.uniLoading = true; U.loadAssets().then(() => render()).catch(() => { state.uniError = true; render(); }); }
    return state.uniError ? '<div class="muted">The uniform pictures could not be loaded.</div>' : '<div class="status">Loading pictures...</div>';
  }
  paintPlan = [];
  const single = kind === 'genesis' && U.get(c.parts, 'InGame') !== undefined;     // Genesis: one coin-toss uniform (written to both skin blocks)
  const sel = !single && state.usel === 1 ? 1 : 0, selKey = sel ? 'Uniform2' : 'Uniform1';
  const sprite = i => {
    const hex = U.get(c.parts, i ? 'Uniform2' : 'Uniform1') || '', id = `uc-${i}`;
    if (kind === 'nes') {
      paintPlan.push({ id, image: i ? 'nes_away' : 'nes_home', set: 'nes', scale: 3, groups: [
        ['mPantsColorPositions', U.nesRgb(U.nesSlotHex(hex, 0))], ['mSkinData', U.nesRgb(U.nesSlotHex(hex, 1))], ['mJerseyColorPositions', U.nesRgb(U.nesSlotHex(hex, 2))]] });
    } else if (kind === 'genesis') {
      paintPlan.push({ id, image: 'coin', set: 'coin', scale: 3, groups: U.coinGroups(hex, false) });
    } else {
      paintPlan.push({ id, image: i ? 'snes_away' : 'snes_home', set: 'snes', scale: 2, groups: U.FREE_SLOTS.map(s => [s.id, U.freeRgb(kind, U.slotHex(hex, s.id))]) });
    }
    return `<button class="unibtn ${i === sel ? 'on' : ''}" data-act="usel:${i}" aria-label="Edit uniform ${i + 1}"><span class="k">${single ? 'Coin toss' : 'Uniform ' + (i + 1)}</span><canvas id="${id}" role="img" aria-label="Uniform ${i + 1} preview"></canvas></button>`;
  };
  const hex = U.get(c.parts, selKey) || '';
  let chips;
  if (kind === 'nes') {
    chips = `<div class="chips2">${[[2, 'Jersey'], [0, 'Pants'], [1, 'Skin']].map(([i, l]) => { const h = U.nesSlotHex(hex, i); return uniChip(`ucolor:${selKey}:${i}`, l, U.nesRgb(h), h.toUpperCase()); }).join('')}</div>`;
  } else {
    chips = ['Jersey', 'Pants', 'Helmet'].map(g => `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">${g} colors</div><div class="chips2">${
      U.FREE_DISPLAY.map(id => U.FREE_SLOTS.find(s => s.id === id)).filter(s => s.group === g).map(s => {
        const h = U.slotHex(hex, s.id);
        return uniChip(`ucolor:${selKey}:${s.id}`, s.label, U.freeRgb(kind, h), h);
      }).join('')}</div>`).join('');
  }
  let champ = '';
  if (kind === 'nes') {
    champ = Object.entries(U.CHAMP).map(([key, def]) => {
      const h = U.get(c.parts, key);
      if (h === undefined) return '';
      const id = `uc-${key}`;
      paintPlan.push({ id, image: def.image, set: 'nes', scale: 1, fluid: true, groups: def.slots.map(([, mask], i) => [mask, U.nesRgb(h.substr(i * 2, 2))]) });
      return `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">${key === 'DivChamp' ? 'Division champ colors' : 'Conference champ colors'}</div>
        <canvas id="${id}" class="champcv" role="img" aria-label="${key} preview"></canvas>
        <div class="chips2">${def.slots.map(([label], i) => uniChip(`ucolor:${key}:${i}`, label, U.nesRgb(h.substr(i * 2, 2)), h.substr(i * 2, 2).toUpperCase())).join('')}</div>`;
    }).join('');
  }
  const usage = U.get(c.parts, 'UniformUsage') === undefined ? '' :
    `<button class="btn ghost" data-act="uusage">Uniform usage: who wears Uniform 2&hellip;</button>`;
  const ig = kind === 'genesis' ? U.get(c.parts, 'InGame') : undefined;
  const awayHex = kind === 'genesis' ? U.get(c.parts, 'InGameAway') : undefined;
  const inGame = ig === undefined ? '' : genesisInGame(c, ig, 'home') + (awayHex !== undefined ? genesisInGame(c, awayHex, 'away')
    : '<div class="muted" style="margin-top:18px">This team has no separate away uniform in the game: it always wears the home uniform.</div>');
  const coinHead = ig === undefined ? '' : `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em;margin-top:18px">Coin toss screen</div>
    <div class="muted">These colors are only used on the coin toss picture; the game field uses the colors above. Every player of the team gets the same colors.</div>`;
  const shared = kind === 'genesis' && (state.team === 'patriots' || state.team === 'seahawks')
    ? '<div class="status" role="note">The Patriots and Seahawks share one home uniform in this game: changing it here changes both teams.</div>' : '';
  return `${shared}${inGame}${coinHead}<div class="unirow">${sprite(0)}${single ? '' : sprite(1)}</div>
    <div class="muted">${single ? 'Tap a color to change it.' : `Editing uniform ${sel + 1}. Tap a color to change it.`}</div>${chips}${usage}${champ}`;
}

/** Genesis: the colors the game field uses (helmet, jersey, pants, two shades each), shown on the SNES player picture. kind: 'home' or 'away'. */
function genesisInGame(c, hex, kind) {
  const away = kind === 'away', key = away ? 'InGameAway' : 'InGame', id = away ? 'uc-away' : 'uc-ig';
  const rgb = sid => U.freeRgb('genesis', U.slotHex(hex, sid)), avg = (a, b) => a.map((v, i) => Math.round((v + b[i]) / 2));
  paintPlan.push({ id, image: away ? 'snes_away' : 'snes_home', set: 'snes', scale: 3, groups: [
    ['HelmetD', rgb('iHelmetD')], ['HelmetM', rgb('iHelmetL')], ['JerseyM', rgb('iJerseyD')], ['JerseyL', rgb('iJerseyL')],
    ['PantsD', rgb('iPantsD')], ['PantsM', avg(rgb('iPantsD'), rgb('iPantsL'))], ['PantsL', rgb('iPantsL')]] });
  const chips = ['Jersey', 'Pants', 'Helmet'].map(g => `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">${g} colors</div><div class="chips2">${
    U.INGAME_SLOTS.filter(s => s.group === g).map(s => { const h = U.slotHex(hex, s.id); return uniChip(`ucolor:${key}:${s.id}`, s.label, U.freeRgb('genesis', h), h); }).join('')}</div>`).join('');
  const intro = away
    ? 'The light uniform the game puts this team in against some opponents (which opponents is set elsewhere in the game and cannot be edited yet). Tap a color to change it.'
    : 'What the game field shows. Every player of this team wears these, light-skin and dark-skin alike. Tap a color to change it.';
  return `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em;${away ? 'margin-top:18px' : ''}">${away ? 'Away uniform (light)' : 'Home uniform (dark)'}</div>
    <div class="unirow"><div class="unibtn on" style="cursor:default"><canvas id="${id}" role="img" aria-label="${away ? 'Away' : 'Home'} uniform preview"></canvas></div></div>
    <div class="muted">${intro}</div>${chips}`;
}

/** Draws the queued uniform pictures (called after every render). */
function paintUniforms() {
  for (const p of paintPlan) {
    const cv = document.getElementById(p.id);
    if (!cv) continue;
    U.paint(cv, p.image, p.set, p.groups);
    if (p.fluid) { cv.style.maxWidth = '100%'; cv.style.height = 'auto'; } else cv.style.width = (cv.width * p.scale) + 'px';
  }
  paintPlan = [];
}

function uniSetColor(d, key, slot, value) {          // value: 4-hex (free) or 2-hex (NES)
  const parts = d.colors.parts, hex = U.get(parts, key);
  let next;
  if (d.colors.kind === 'nes') {
    next = hex.slice(0, slot * 2) + value + hex.slice(slot * 2 + 2);
  } else next = U.withSlot(hex, slot, value);
  U.set(parts, key, next);
  if (d.colors.kind === 'genesis' && key === 'Uniform1' && U.get(parts, 'InGame') !== undefined) U.set(parts, 'Uniform2', next);   // one coin-toss uniform for both skin blocks
}

function uniLabel(d, p) {
  if (d.colors.kind === 'nes') {
    const names = { Uniform1: ['Pants', 'Skin', 'Jersey'], Uniform2: ['Pants', 'Skin', 'Jersey'] };
    if (names[p.key]) return `Uniform ${p.key === 'Uniform1' ? 1 : 2} ${names[p.key][p.slot]}`;
    return `${p.key === 'DivChamp' ? 'Division' : 'Conference'} champ ${U.CHAMP[p.key].slots[p.slot][0]}`;
  }
  if (p.key === 'InGame' || p.key === 'InGameAway') { const s = U.slotInfo(p.slot); return `${p.key === 'InGame' ? 'Home' : 'Away'} ${s.group} ${s.label}`; }
  const s = U.FREE_SLOTS.find(x => x.id === p.slot);
  return `Uniform ${p.key === 'Uniform1' ? 1 : 2} ${s.group} ${s.label}`;
}

function uniSheets(d) {
  if (!d.colors) return '';
  if (state.upick) return uniPickerSheet(d);
  if (state.usage) return usageSheet();
  return '';
}

function uniPickerSheet(d) {
  const p = state.upick, kind = d.colors.kind, title = uniLabel(d, p);
  if (kind === 'nes') {
    const cur = U.get(d.colors.parts, p.key).substr(p.slot * 2, 2).toLowerCase();
    const cells = Array.from({ length: 64 }, (_, i) => {
      const hx = i.toString(16).padStart(2, '0'), rgb = U.NES_PALETTE[i];
      return `<button class="nsw ${hx === cur ? 'on' : ''}" style="background:${U.css(rgb)};color:${U.readableOn(rgb)}" data-act="upal:${hx}" aria-label="Palette color ${hx}">${hx.toUpperCase()}</button>`;
    }).join('');
    return `<div class="scrim pop fit" data-act="uclose"><div class="sheet pop fit" role="dialog" aria-label="Pick a color" data-stop="1">
      <div class="topbar"><h2>${esc(title)}</h2><button class="btn icon-btn" data-act="uclose" aria-label="Close">&times;</button></div>
      <div class="nespal">${cells}</div></div></div>`;
  }
  const L = p.levels, max = U.LEVELS[kind], rgb = U.freeRgb(kind, U.freeFromLevels(kind, L));
  const row = (ch, name) => `<div class="lvrow"><span class="k">${name}</span>
      <button class="btn icon-btn" data-act="ulev:${ch}:-1" aria-label="${name} down">&minus;</button>
      <input type="range" data-lev="${ch}" min="0" max="${max}" step="1" value="${L[ch]}" aria-label="${name}">
      <button class="btn icon-btn" data-act="ulev:${ch}:1" aria-label="${name} up">+</button><span class="val sm" id="lv-${ch}">${L[ch]}</span></div>`;
  const cur = U.freeFromLevels(kind, L);
  const cell = (l, label) => { const c = U.freeRgb(kind, U.freeFromLevels(kind, l)); return `<button class="psw ${U.freeFromLevels(kind, l) === cur ? 'on' : ''}" style="background:${U.css(c)}" data-act="ulv:${l.r},${l.g},${l.b}" aria-label="${label}"></button>`; };
  const mine = [...new Set((state.tedit.colors.parts || []).filter(x => /^(Uniform1|Uniform2|InGame|InGameAway)$/.test(x.key)).flatMap(x => (x.hex.match(/.{4}/g) || [])))];
  const palette = U.paletteLevels(kind).map(r => `<div class="prow">${r.map(l => cell(l, 'Palette color')).join('')}</div>`).join('');
  return `<div class="scrim pop fit" data-act="uclose"><div class="sheet pop fit" role="dialog" aria-label="Pick a color" data-stop="1">
      <div class="topbar"><h2>${esc(title)}</h2><button class="btn icon-btn" data-act="uclose" aria-label="Close">&times;</button></div>
      <div class="pickhead"><div class="bigsw" id="ubig" style="background:${U.css(rgb)}"></div><div class="muted" id="uhex">${cur}</div></div>
      <canvas id="usv" class="pickcv" width="240" height="150" role="img" aria-label="Saturation and brightness"></canvas>
      <canvas id="uhue" class="pickcv hue" width="240" height="26" role="img" aria-label="Hue"></canvas>
      <div class="muted pk">Game colors</div><div class="pal">${palette}</div>
      <div class="muted pk">Colors already on this team</div><div class="prow">${mine.map(h => cell(U.freeToLevels(kind, h), 'Team color ' + h)).join('')}</div>
      <button class="btn ghost fine" data-act="ufine" aria-expanded="${!!p.fine}">${p.fine ? 'Hide' : 'Fine tune'} red / green / blue</button>
      ${p.fine ? row('r', 'Red') + row('g', 'Green') + row('b', 'Blue') : ''}
      <label class="btn ghost nat">Pick any color<input type="color" id="unative" value="${rgbToHex(rgb)}"></label>
      <div class="bar2 sheetfoot"><button class="btn ghost" data-act="uclose">Cancel</button><button class="btn primary" data-act="uok">OK</button></div>
    </div></div>`;
}

const rgbToHex = ([r, g, b]) => '#' + [r, g, b].map(v => v.toString(16).padStart(2, '0')).join('');

function usageSheet() {
  const u = state.usage;
  return `<div class="scrim pop fit" data-act="uclose"><div class="sheet pop fit" role="dialog" aria-label="Uniform usage" data-stop="1">
      <div class="topbar"><h2>Use uniform 2 against</h2><button class="btn icon-btn" data-act="uclose" aria-label="Close">&times;</button></div>
      <div class="muted" style="margin-top:6px">Checked teams see this team in Uniform 2. Mask: <b id="umask">${U.usageHex(u.bits, u.original)}</b></div>
      <div class="usagegrid">${u.ids.map((id, i) => `<button class="chk ${u.bits[i] ? 'on' : ''}" role="checkbox" aria-checked="${u.bits[i]}" data-act="utog:${i}"><span class="box"></span>${esc(teamLabel(id))}</button>`).join('')}</div>
      <div class="bar2"><button class="btn ghost" data-act="uall">All</button><button class="btn ghost" data-act="unone">None</button><button class="btn primary" data-act="usave">OK</button></div>
    </div></div>`;
}

/** Auto-update sim data: ask which formula (NES/SNES TSB1/Genesis have two; TSB2/3 have one), run, then report with Undo. */
function autoSheet() {
  const a = state.auto;
  if (!a) return '';
  let title, body, buttons;
  if (a.stage === 'ask') {
    const two = family() === 'tsb1';
    title = two ? 'Use which formula?' : 'Auto-update sim data?';
    body = `<p>This works out the sim values of <b>all players and all teams</b> from their ratings and replaces values set by hand.${two ? ' Each team\'s run/pass preference is kept.' : ''}</p>${two
      ? `<p class="fdesc"><b>Tecmonster</b>: Tecmonster's formulas.</p>
         <p class="fdesc"><b>TSBTool</b>: TSBTool's newer formulas, tested against many simmed seasons to match how the original game simmed. Defenders and teams still use Tecmonster's.</p>` : ''}`;
    buttons = two
      ? `<button class="btn primary" data-act="autorun:t">Tecmonster formulas</button><button class="btn" data-act="autorun:s">TSBTool formulas</button><button class="btn ghost" data-act="autoclose">Cancel</button>`
      : `<button class="btn primary" data-act="autorun:t">Update all</button><button class="btn ghost" data-act="autoclose">Cancel</button>`;
  } else if (a.stage === 'done') {
    title = 'Sim data updated';
    body = a.changed ? `${a.changed} lines changed. Use Undo to go back to the values from before.` : 'Nothing needed to change.';
    buttons = `<button class="btn primary" data-act="autoclose">OK</button>${a.changed ? '<button class="btn ghost" data-act="autoundo">Undo</button>' : ''}`;
  } else {
    title = 'Could not update';
    body = esc(a.msg || 'Something went wrong.');
    buttons = '<button class="btn primary" data-act="autoclose">OK</button>';
  }
  return `<div class="scrim pop fit" data-act="autoclose"><div class="sheet pop fit" role="dialog" aria-label="${esc(title)}" data-stop="1">
      <div class="topbar"><h2>${esc(title)}</h2></div>
      <div class="autobody">${body}</div><div class="autobtns">${buttons}</div></div></div>`;
}

async function runAuto(tsbtoolFormulas) {
  commitTeam(); state.tedit = null;
  const before = state.text;
  state.auto = null; state.busy = 'Updating sim data... this can take a few seconds.'; render();
  await nextFrame();
  try {
    const out = web.AutoUpdateSim(before, tsbtoolFormulas);
    state.text = out; state.parsed = parseText(out); state.pending = true;
    state.autoUndo = { text: before };
    state.auto = { stage: 'done', changed: changedLines(before, out) };
  } catch (e) {
    state.auto = { stage: 'error', msg: 'The update failed: ' + (e.message || e) };
  }
  state.busy = ''; render();
}

function autoUndo() {
  if (!state.autoUndo) return;
  state.text = state.autoUndo.text; state.parsed = parseText(state.text); state.pending = true;
  state.tedit = null; state.edit = null; state.autoUndo = null; state.auto = null;
  render();
}

// ---------- dialogs and the highlight color ----------
/** Every dialog keeps its title and close button fixed; the rest scrolls underneath them. */
function wrapSheets() {
  document.querySelectorAll('.sheet').forEach(sheet => {
    if (sheet.querySelector(':scope > .sheetbody')) return;
    const head = sheet.querySelector(':scope > .topbar');
    if (!head) return;
    const body = document.createElement('div');
    body.className = 'sheetbody';
    while (head.nextSibling) body.appendChild(head.nextSibling);
    sheet.appendChild(body);
    const foot = body.querySelector(':scope > .sheetfoot');      // a footer stays put below the scrolling part
    if (foot) sheet.appendChild(foot);
  });
}

const ACCENT_DEFAULT = '#a78bfa';
const ACCENT_PRESETS = [['Gold', '#ffc93c'], ['Amber', '#ff9f1c'], ['Coral', '#ff6b6b'], ['Pink', '#ff4fa3'], ['Violet', '#a78bfa'],
  ['Blue', '#4da3ff'], ['Cyan', '#22d3ee'], ['Mint', '#34d399'], ['Lime', '#a3e635'], ['White', '#ffffff']];

const hexToRgb = h => [1, 3, 5].map(i => parseInt(h.substr(i, 2), 16));
const mixRgb = (a, b, t) => a.map((v, i) => Math.round(v + (b[i] - v) * t));
const rgbCss = a => `rgb(${a[0]},${a[1]},${a[2]})`;

/** Sets the Modern style's highlight variables from state.accent (Retro keeps its own colors). */
function applyAccent() {
  const st = document.body.style, names = ['--accent', '--accent-ink', '--accent-hi', '--accent-lo', '--accent-glow'];
  if (state.theme !== 'modern' || !state.accent) { names.forEach(n => st.removeProperty(n)); return; }
  const rgb = hexToRgb(state.accent), lum = (rgb[0] * 299 + rgb[1] * 587 + rgb[2] * 114) / 1000;
  st.setProperty('--accent', state.accent);
  st.setProperty('--accent-ink', lum > 150 ? '#14110a' : '#ffffff');
  st.setProperty('--accent-hi', rgbCss(mixRgb(rgb, [255, 255, 255], .32)));
  st.setProperty('--accent-lo', rgbCss(mixRgb(rgb, [0, 0, 0], .08)));
  st.setProperty('--accent-glow', `rgba(${rgb[0]},${rgb[1]},${rgb[2]},.38)`);
}

function setAccent(hex) {
  state.accent = hex && hex.toLowerCase() !== ACCENT_DEFAULT ? hex.toLowerCase() : null;
  try { if (state.accent) localStorage.setItem('tsb.accent', state.accent); else localStorage.removeItem('tsb.accent'); } catch (e) { /* storage may be blocked */ }
  applyAccent();
}

function accentSheet() {
  const cur = state.accent || ACCENT_DEFAULT;
  return `<div class="scrim pop fit" data-act="accentclose"><div class="sheet pop fit" role="dialog" aria-label="Highlight color" data-stop="1">
      <div class="topbar"><h2>Highlight color</h2><button class="btn icon-btn" data-act="accentclose" aria-label="Close">&times;</button></div>
      <div class="muted" style="margin-top:6px">Used by the Modern style's buttons, chips and tabs. Right-click (or long-press) the Modern button to open this.</div>
      <div class="swatches">${ACCENT_PRESETS.map(([n, h]) => `<button class="swatch-btn ${h === cur ? 'on' : ''}" style="background:${h}" data-act="accentset:${h}" aria-label="${n}"></button>`).join('')}</div>
      <div class="accentpick"><input type="color" id="accentcolor" value="${cur}" aria-label="Custom color"><span class="muted">Custom color <b id="accenthex">${cur}</b></span></div>
      <div class="autobtns"><button class="btn primary" data-act="accentclose">Done</button><button class="btn ghost" data-act="accentreset">Reset to violet</button></div>
    </div></div>`;
}

function openAccent() { state.theme = 'modern'; state.accentDlg = true; render(); }

// right-click on the Modern button; long-press for touch screens
let accentSuppressClick = false;
document.addEventListener('contextmenu', e => {
  if (e.target.closest('[data-act="theme:modern"]')) { e.preventDefault(); openAccent(); }
});
{
  let timer = null;
  document.addEventListener('pointerdown', e => {
    if (e.pointerType !== 'touch' || !e.target.closest('[data-act="theme:modern"]')) return;
    timer = setTimeout(() => { timer = null; accentSuppressClick = true; openAccent(); }, 550);
  });
  const cancel = () => { if (timer) { clearTimeout(timer); timer = null; } };
  document.addEventListener('pointerup', cancel); document.addEventListener('pointercancel', cancel); document.addEventListener('pointermove', cancel);
}

// ---------- pro bowl ----------
const SKILL_POS = ['RB1', 'RB2', 'RB3', 'RB4', 'WR1', 'WR2', 'WR3', 'WR4', 'TE1', 'TE2'];
const DB_POS = ['LCB', 'RCB', 'SS', 'FS', 'DB1', 'DB2', 'DB3'];
const PB_GROUPS = [
  ['Quarterbacks', s => /^QB/.test(s)], ['Running backs', s => /^RB/.test(s)], ['Receivers', s => /^(WR|TE)/.test(s)],
  ['Offensive line', s => /^(C|LG|RG|LT|RT)$/.test(s)], ['Defensive line', s => /^(RE|NT|LE)\d?$/.test(s)],
  ['Linebackers', s => /^(ROLB|RILB|LILB|LOLB|LB5)$/.test(s)], ['Defensive backs', s => /^(RCB|LCB|FS|SS|DB\d)$/.test(s)],
  ['Kickers', s => /^(K|P)$/.test(s)], ['Returners', s => /^RET/.test(s)],
];

/** Which positions of a team may fill a Pro Bowl spot (the desktop's rules, plus the extra spots some ROMs have). */
function pbLegal(spot) {
  if (/^QB/.test(spot)) return ['QB1', 'QB2'];
  if (/^(RB|WR|TE)/.test(spot)) return SKILL_POS;
  if (/^(C|LG|RG|LT|RT)$/.test(spot)) return ['LT', 'LG', 'C', 'RG', 'RT'];
  if (/^(RE|NT|LE)\d?$/.test(spot)) return ['LE', 'NT', 'RE', 'LE2', 'NT2', 'RE2'];
  if (/^(ROLB|RILB|LILB|LOLB|LB5)$/.test(spot)) return ['LOLB', 'LILB', 'RILB', 'ROLB', 'LB5'];
  if (/^(RCB|LCB|FS|SS|DB\d)$/.test(spot)) return DB_POS;
  if (/^(K|P)$/.test(spot)) return ['P', 'K'];
  return [...SKILL_POS, ...DB_POS];            // RET1-3
}

/** AFC spots may take AFC teams (the first half of the team list), NFC spots NFC teams. */
function pbTeams(conf) {
  const ids = teamIds(), half = Math.floor(ids.length / 2);
  return conf === 'AFC' ? ids.slice(0, half) : ids.slice(half);
}

const playerAt = (team, pos) => { const t = state.parsed.teams.find(x => x.id === team); return t && t.players.find(p => p.pos === pos); };

function pbLoad() {
  const rows = { AFC: [], NFC: [] };
  for (const line of web.GetProBowl(1).split(/\r?\n/)) {
    const m = /^(AFC|NFC),(\w+),(\w+),(\w+)\s*$/.exec(line);
    if (m) rows[m[1]].push({ spot: m[2], team: m[3], pos: m[4] });
  }
  state.pb = rows;
}

function proBowlScreen() {
  if (!state.pb) pbLoad();
  const conf = state.pbconf === 'NFC' ? 'NFC' : 'AFC', rows = state.pb[conf];
  const groups = PB_GROUPS.map(([title, test]) => {
    const list = rows.map((r, i) => ({ ...r, i })).filter(r => test(r.spot));
    if (!list.length) return '';
    return `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em;margin-top:6px">${title}</div>
      <div class="list games">${list.map(r => {
        const pl = playerAt(r.team, r.pos);
        return `<button class="game pbspot" data-act="pbedit:${r.i}"><span class="code">${esc(r.spot)}</span>
          <span class="tm">${pl ? esc(pl.name) : esc(r.pos)}</span><span class="who">${esc(teamLabel(r.team))} ${esc(r.pos)}</span></button>`;
      }).join('')}</div>`;
  }).join('');
  return `<div class="hdr"><div class="topbar"><button class="btn ghost small" data-act="go:home">${ICON.left} Home</button>${themeSwitch()}</div><h1>Pro Bowl</h1>
      <div class="seg wide two" role="group" aria-label="Conference"><button class="${conf === 'AFC' ? 'on' : ''}" data-act="pbconf:AFC">AFC</button><button class="${conf === 'NFC' ? 'on' : ''}" data-act="pbconf:NFC">NFC</button></div></div>
    ${groups || '<div class="status">This ROM has no Pro Bowl rosters.</div>'}
    <div class="bar"><div><button class="btn primary" data-act="go:home">Done</button></div></div>${pbSheet()}`;
}

/** Centered dialog: pick the team (of this conference) and the position on that team who fills the spot. */
function pbSheet() {
  const e = state.pbEdit;
  if (!e) return '';
  const teams = pbTeams(e.conf);
  if (e.view === 'teams') {
    return `<div class="scrim pop fit" data-act="pbclose"><div class="sheet pop fit" role="dialog" aria-label="Pick a team" data-stop="1">
      <div class="topbar"><button class="btn icon-btn" data-act="pbback" aria-label="Back">${ICON.left}</button><h2>${e.conf} teams</h2><button class="btn icon-btn" data-act="pbclose" aria-label="Close">&times;</button></div>
      ${teamGrid(teams, e.team, 'pbteam')}</div></div>`;
  }
  const legal = pbLegal(e.spot), have = new Set(state.parsed.teams.find(t => t.id === e.team).players.map(p => p.pos));
  const positions = legal.filter(p => have.has(p));
  if (!positions.includes(e.pos)) e.pos = positions[0];
  const pl = playerAt(e.team, e.pos);
  return `<div class="scrim pop fit" data-act="pbclose"><div class="sheet pop fit" role="dialog" aria-label="Pro Bowl spot" data-stop="1">
      <div class="topbar"><h2>${e.conf} ${esc(e.spot)}</h2><button class="btn icon-btn" data-act="pbclose" aria-label="Close">&times;</button></div>
      ${fieldButton('Team', esc(teamLabel(e.team)), 'pbpickteam')}
      <div class="field"><span class="fl">Position</span></div>
      <div class="bubbles" role="group" aria-label="Position">${positions.map(p => { const x = playerAt(e.team, p);
        return `<button class="bub ${p === e.pos ? 'on' : ''}" data-act="pbpos:${esc(p)}"><span class="k">${esc(p)}</span><span class="nm">${x ? esc(x.name) : ''}</span></button>`; }).join('')}</div>
      <div class="autobody"><b>${pl ? esc(pl.name) : '(nobody)'}</b> &middot; ${esc(teamLabel(e.team))} ${esc(e.pos)}</div>
      <div class="autobtns"><button class="btn primary" data-act="pbok">OK</button><button class="btn ghost" data-act="pbclose">Cancel</button></div>
    </div></div>`;
}

function pbApply() {
  const e = state.pbEdit;
  try {
    web.ProcessText(`${e.conf},${e.spot},${e.team},${e.pos}`);        // the core's own Pro Bowl command
    const m = web.TakeMessages();
    if (m) state.messages = m;
    pbLoad();
  } catch (x) { state.error = 'Could not change the Pro Bowl spot: ' + (x.message || x); }
  state.pbEdit = null;
}

// ---------- bubbles: the app's replacement for dropdowns ----------
/** A grid of team bubbles. `disabled(id)` returns a short reason when a team cannot be chosen (it is dimmed, not hidden). */
function teamGrid(ids, current, act, disabled) {
  return `<div class="picker">${ids.map(t => {
    const why = disabled ? disabled(t) : null;
    return `<button class="btn ${t === current ? 'on' : ''}" ${why ? 'disabled' : ''} data-act="${act}:${esc(t)}">${esc(teamLabel(t))}${why ? `<span class="why">${esc(why)}</span>` : ''}</button>`;
  }).join('')}</div>`;
}

/** A field that shows its current value and opens a bubble picker when tapped. */
function fieldButton(label, value, act) {
  return `<div class="field"><span class="fl">${label}</span><button class="btn pickfield" data-act="${act}" aria-haspopup="dialog"><span>${value}</span>${ICON.down}</button></div>`;
}

// ---------- TSB3 photos ----------
const photoImg = (i, scale, cls = '') => `<img class="phimg ${cls}" alt="" src="${P.cache.urls[i]}" width="${P.PHOTO_W * scale}" height="${P.PHOTO_H * scale}">`;

function photoOwner(i) {
  const o = P.cache.owners[i] || 'default';
  if (o === 'default') return { text: 'Everyone who has no photo of their own', short: 'everyone else' };
  if (o === 'free') return { text: 'Not used by any player', short: 'free' };
  if (o.startsWith('unused')) return { text: 'Belongs to an id that is not a roster slot', short: 'unused' };
  const [team, pos] = o.split('|'), pl = playerAt(team, pos);
  return { team, pos, text: `${teamLabel(team)} ${pos}${pl ? ' - ' + pl.name : ''}`, short: pl ? pl.name : `${teamLabel(team)} ${pos}`, slot: `${teamLabel(team)} ${pos}` };
}

function photosScreen() {
  const used = P.cache.owners.slice(0, P.PLAYER_PHOTOS).filter(o => o.includes('|')).length;
  const cards = P.cache.urls.map((_, i) => {
    const o = photoOwner(i);
    return `<button class="pcard" data-act="photo:${i}">${photoImg(i, 3)}<span class="k">${P.hex(i)}</span><span class="nm">${esc(o.short)}</span><span class="slot">${esc(o.slot || '')}</span></button>`;
  }).join('');
  return `<div class="hdr"><div class="topbar"><button class="btn ghost small" data-act="go:home">${ICON.left} Home</button>${themeSwitch()}</div><h1>Player photos</h1></div>
    <div class="muted">${used} of ${P.PLAYER_PHOTOS} photos are in use. A photo goes with a roster slot (team and position), not with a name. Tap one to change who has it or its picture.</div>
    <div class="photogrid">${cards}</div>`;
}

/** Detail dialog for one photo: who has it, change the player, change the picture, free it. */
function photoSheet() {
  const d = state.photoDlg;
  if (!d) return '';
  const i = d.photo, o = photoOwner(i), silhouette = i === P.SILHOUETTE;
  const close = `<button class="btn icon-btn" data-act="phclose" aria-label="Close">&times;</button>`;
  if (d.mode === 'team') {
    return `<div class="scrim pop fit" data-act="phclose"><div class="sheet pop fit" role="dialog" aria-label="Pick a team" data-stop="1">
      <div class="topbar"><button class="btn icon-btn" data-act="phback" aria-label="Back">${ICON.left}</button><h2>Pick a team</h2>${close}</div>
      ${teamGrid(state.parsed.teams.map(t => t.id), d.team, 'phteam')}</div></div>`;
  }
  if (d.mode === 'assign') {
    const team = state.parsed.teams.find(t => t.id === d.team) || state.parsed.teams[0];
    const cur = P.photoOf(team.id, d.pos);
    const note = cur !== undefined && cur !== i ? `${esc(teamLabel(team.id))} ${esc(d.pos)} already has photo ${P.hex(cur)}; it will be freed.` : '&nbsp;';
    return `<div class="scrim pop fit" data-act="phclose"><div class="sheet pop fit" role="dialog" aria-label="Give photo ${P.hex(i)} to a player" data-stop="1">
      <div class="topbar"><button class="btn icon-btn" data-act="phback" aria-label="Back">${ICON.left}</button><h2>Photo ${P.hex(i)} goes to</h2>${close}</div>
      ${fieldButton('Team', esc(teamLabel(team.id)), 'phpickteam')}
      <div class="field"><span class="fl">Position</span></div>
      <div class="bubbles" role="group" aria-label="Position">${team.players.map(p => `<button class="bub ${p.pos === d.pos ? 'on' : ''}" data-act="phpos:${esc(p.pos)}"><span class="k">${esc(p.pos)}</span><span class="nm">${esc(p.name)}</span></button>`).join('')}</div>
      <div class="autobtns sheetfoot"><div class="muted">${note}</div><button class="btn primary" data-act="phassign">Give the photo to ${esc(d.pos)}</button><button class="btn ghost" data-act="phback">Cancel</button></div></div></div>`;
  }
  return `<div class="scrim pop fit" data-act="phclose"><div class="sheet pop fit" role="dialog" aria-label="Photo ${P.hex(i)}" data-stop="1">
      <div class="topbar"><h2>Photo ${P.hex(i)}</h2>${close}</div>
      <div class="phbig">${photoImg(i, 6)}</div>
      <div class="autobody" style="text-align:center"><b>${esc(o.text)}</b></div>
      <div class="autobtns">
        ${silhouette ? '' : '<button class="btn primary" data-act="phchange">Change player&hellip;</button>'}
        <button class="btn ${silhouette ? 'primary' : ''}" data-act="phpic">Change picture&hellip;</button>
        ${!silhouette && o.team ? '<button class="btn ghost" data-act="phfree">Free this photo</button>' : ''}
        <button class="btn ghost" data-act="phclose">Close</button></div></div></div>`;
}

/** Plain message dialog. */
function noticeSheet() {
  if (!state.notice) return '';
  return `<div class="scrim pop fit" data-act="noticeclose"><div class="sheet pop fit" role="dialog" aria-label="Notice" data-stop="1">
      <div class="topbar"><h2>${esc(state.notice.title)}</h2></div><div class="autobody">${esc(state.notice.text)}</div>
      <div class="autobtns"><button class="btn primary" data-act="noticeclose">OK</button></div></div></div>`;
}

// ---- picture editor: choose an image, crop and adjust it, see the game's 16 grays ----
let photoTimer = null;

function photoEditorSheet() {
  const e = state.photoEd;
  if (!e) return '';
  const close = `<button class="btn icon-btn" data-act="phedclose" aria-label="Close">&times;</button>`;
  const head = `<div class="topbar"><h2>Picture for photo ${P.hex(e.photo)}</h2>${close}</div>`;
  if (!e.img) {
    return `<div class="scrim pop fit" data-act="phedclose"><div class="sheet pop fit" role="dialog" aria-label="Change picture" data-stop="1">${head}
      <div class="dropzone" id="phdrop"><label class="btn primary">Choose an image<input type="file" id="phfile" accept="image/*" hidden></label>
        <div class="muted">or drop one here</div><div class="muted">The game uses 15 grays at 24x32 pixels. A 24x32 image is used pixel for pixel; anything else is cropped to 3:4 and converted.</div></div>
      <div class="autobtns"><button class="btn ghost" data-act="phedclose">Cancel</button></div></div></div>`;
  }
  const exact = e.img.w === P.PHOTO_W && e.img.h === P.PHOTO_H;
  const slider = (key, label, min, max, step) => `<label class="lvrow phrow"><span class="k">${label}</span>
      <input type="range" data-ph="${key}" min="${min}" max="${max}" step="${step}" value="${e[key]}" aria-label="${label}"><span class="val sm" id="phv-${key}">${e[key]}</span></label>`;
  return `<div class="scrim pop fit" data-act="phedclose"><div class="sheet pop fit" role="dialog" aria-label="Change picture" data-stop="1">${head}
      <div class="phprev" id="phdrop"><div class="phsrc"><canvas id="phsrc" aria-label="Your image with the crop frame; drag to move the frame"></canvas><span class="muted">${exact ? 'Your image' : 'Drag to move the frame; wheel or pinch to zoom'}</span></div>
        <div class="phres"><canvas id="phres" aria-label="Result in the game's grays"></canvas><span class="muted">In the game</span></div></div>
      ${exact ? '<div class="muted">This image is 24x32, so it is used pixel for pixel (each pixel set to the nearest gray).</div>' : `
      ${slider('zoom', 'Zoom', 1, 4, 0.05)}${slider('panx', 'Left / right', 0, 1, 0.01)}${slider('pany', 'Up / down', 0, 1, 0.01)}
      ${slider('brightness', 'Brightness', -100, 100, 1)}${slider('contrast', 'Contrast', -100, 100, 1)}${slider('sharpen', 'Sharpen', 0, 2, 0.1)}
      <button class="chk ${e.auto ? 'on' : ''}" role="checkbox" aria-checked="${e.auto}" data-act="phauto" style="margin-top:6px"><span class="box"></span>Stretch to the full range of grays</button>`}
      <div class="autobtns"><button class="btn primary" data-act="phuse">Use this picture</button>
        <label class="btn">Choose another image<input type="file" id="phfile" accept="image/*" hidden></label><button class="btn ghost" data-act="phedclose">Cancel</button></div></div></div>`;
}

function photoConvert() {
  const e = state.photoEd;
  if (!e || !e.img) return;
  const im = e.img;
  if (im.w === P.PHOTO_W && im.h === P.PHOTO_H) { e.pixels = web.ConvertPhoto(im.argb, im.w, im.h, 0, 0, im.w, im.h, 0, 0, 0, true); return; }
  const base = web.PhotoDefaultCrop(im.w, im.h), cw = base[2] / e.zoom, ch = base[3] / e.zoom;
  e.crop = { x: (im.w - cw) * e.panx, y: (im.h - ch) * e.pany, w: cw, h: ch };
  e.pixels = web.ConvertPhoto(im.argb, im.w, im.h, e.crop.x, e.crop.y, cw, ch, e.brightness, e.contrast, e.sharpen, e.auto);
}

function photoPaint() {
  const e = state.photoEd;
  if (!e || !e.img) return;
  const res = document.getElementById('phres'), src = document.getElementById('phsrc');
  if (res && e.pixels) { P.draw(res, e.pixels, 7); }
  if (src) {
    const k = Math.min(1, 168 / Math.max(e.img.w, e.img.h)), w = Math.max(1, Math.round(e.img.w * k)), h = Math.max(1, Math.round(e.img.h * k));
    src.width = w; src.height = h;
    const ctx = src.getContext('2d');
    ctx.drawImage(e.img.canvas, 0, 0, w, h);
    if (e.crop) { ctx.fillStyle = 'rgba(0,0,0,.45)'; ctx.fillRect(0, 0, w, h); const c = e.crop; ctx.drawImage(e.img.canvas, c.x, c.y, c.w, c.h, c.x * k, c.y * k, c.w * k, c.h * k);
      ctx.strokeStyle = '#fff'; ctx.lineWidth = 2; ctx.strokeRect(c.x * k, c.y * k, c.w * k, c.h * k); }
  }
}

async function photoLoadFile(file) {
  try {
    const e = state.photoEd;
    e.img = await P.readImage(file);
    Object.assign(e, { zoom: 1, panx: 0.5, pany: 0, brightness: 0, contrast: 0, sharpen: 0.6, auto: true });
    photoConvert(); render();
  } catch (x) { state.photoEd = null; state.notice = { title: 'Could not read the image', text: String(x.message || x) }; render(); }
}

function photoUse() {
  const e = state.photoEd;
  if (e.assign) { web.AssignPhoto(e.photo, e.assign.team, e.assign.pos); P.refreshOwners(web); }   // a player who had no photo gets this one
  web.SetPhotoPixels(e.photo, e.pixels);
  P.refresh(web, e.photo);
  state.photoEd = null; render();
}

/** The player editor's photo box: his photo (or a hint when he has none). */
function playerPhotoBox(team, pos) {
  const ph = P.photoOf(team, pos);
  return `<button class="facebox phbox" data-act="pl-photo" aria-label="${ph !== undefined ? 'Change photo ' + P.hex(ph) : 'Give him a photo'}">
    ${ph !== undefined ? photoImg(ph, 3) : `<span class="phnone">No photo</span>`}<span class="k">${ph !== undefined ? 'Photo ' + P.hex(ph) : 'tap to add'}</span></button>`;
}

/** An image dropped on a photo box (player editor) or a photo card (Photos screen): open the picture editor with it loaded. */
function photoDropped(el, file) {
  let photo, assign = null;
  if (el.dataset.act === 'pl-photo') {
    const team = state.team, pos = state.edit.cur.pos;
    photo = P.photoOf(team, pos);
    if (photo === undefined) {
      photo = P.cache.owners.findIndex((o, i) => i < P.PLAYER_PHOTOS && o === 'free');
      if (photo < 0) { state.notice = { title: 'No free photo', text: 'All 84 photos are in use. Free one on the Photos screen first, then give it to this player.' }; render(); return; }
      assign = { team, pos };
    }
  } else photo = Number(el.dataset.act.split(':')[1]);
  state.photoDlg = null;
  state.photoEd = { photo, assign, img: null, zoom: 1, panx: 0.5, pany: 0, brightness: 0, contrast: 0, sharpen: 0.6, auto: true };
  photoLoadFile(file);
}

function openPlayerPhoto() {
  const team = state.team, pos = state.edit.cur.pos, ph = P.photoOf(team, pos);
  if (ph !== undefined) { state.photoDlg = { photo: ph, mode: 'main' }; return; }
  const free = P.cache.owners.findIndex((o, i) => i < P.PLAYER_PHOTOS && o === 'free');
  if (free < 0) { state.notice = { title: 'No free photo', text: 'All 84 photos are in use. Free one on the Photos screen first, then give it to this player.' }; return; }
  state.photoDlg = { photo: free, mode: 'assign', team, pos };
}


// ---------- schedule ----------
const schedWeeks = () => S.parseSchedule(state.parsed.lines);
const realTeams = () => state.parsed.teams.filter(t => t.real);      // without TSB3's freeAgents / allTime
const teamIds = () => realTeams().map(t => t.id);

/** Replaces one week's game lines in the text (the schedule comes after every team, so no other line index moves). */
function setWeekGames(weekIdx, games) {
  const w = schedWeeks()[weekIdx], lines = state.parsed.lines;
  lines.splice(w.header + 1, w.count, ...games.map(S.gameLine));
  state.text = lines.join('\n');
  state.pending = true;
}

function scheduleScreen() {
  const weeks = schedWeeks(), ids = teamIds(), cap = Math.floor(ids.length / 2);
  state.sweek = clamp(state.sweek, 0, Math.max(0, weeks.length - 1));
  const view = state.sview === 'team' ? 'team' : 'week';
  const top = `<div class="topbar"><button class="btn ghost small" data-act="go:home">${ICON.left} Home</button>${themeSwitch()}</div><h1>Schedule</h1>
    <div class="seg wide two" role="group" aria-label="Schedule view"><button class="${view === 'week' ? 'on' : ''}" data-act="sview:week">By week</button><button class="${view === 'team' ? 'on' : ''}" data-act="sview:team">By team</button></div>`;
  if (!weeks.length) return `<div class="hdr">${top}</div><div class="status">This ROM has no schedule.</div>`;

  if (view === 'team') {
    const team = ids.includes(state.steam) ? state.steam : ids[0];
    const season = S.teamSeason(weeks, team);
    const played = season.filter(Boolean), home = played.filter(x => x.home).length;
    const rows = season.map((s, i) => `<button class="game" data-act="sweekgo:${i}"><span class="tm">Week ${weeks[i].no}</span>
      <span class="at">${s ? (s.home ? 'vs' : 'at') : ''}</span><span class="tm r">${s ? esc(teamLabel(s.opp)) : 'BYE'}</span></button>`).join('');
    return `<div class="hdr">${top}${fieldButton('Team', esc(teamLabel(team)), 'steampick')}</div>
      <div class="muted">${played.length} games (${home} home, ${played.length - home} away), ${season.length - played.length} bye weeks. Tap a week to edit it.</div>
      <div class="list">${rows}</div>${state.steamDlg ? steamSheet(ids, team) : ''}`;
  }

  const w = weeks[state.sweek], bad = S.doubleBooked(w), bye = S.byeTeams(w, ids);
  const chips = `<div class="chips weekchips" role="group" aria-label="Week">${weeks.map((x, i) => {
    const flagged = S.doubleBooked(x).length > 0 || x.games.length > cap;
    return `<button class="chip ${i === state.sweek ? 'on' : ''} ${flagged ? 'warn' : ''}" data-act="sweek:${i}" aria-label="Week ${x.no}${flagged ? ', has a problem' : ''}">${x.no}</button>`;
  }).join('')}</div>`;
  const warn = (bad.length ? `<div class="banner" role="status">${bad.map(t => esc(teamLabel(t))).join(', ')} ${bad.length > 1 ? 'are' : 'is'} booked twice this week. Tap one of the games to move it.</div>` : '')
    + (w.games.length > cap ? `<div class="banner" role="status">This week has ${w.games.length} games; the most a week can hold is ${cap}.</div>` : '');
  const games = w.games.map((g, i) => `<button class="game ${bad.includes(g.away) || bad.includes(g.home) ? 'bad' : ''}" data-act="sgame:${i}">
      <span class="tm">${esc(teamLabel(g.away))}</span><span class="at">at</span><span class="tm r">${esc(teamLabel(g.home))}</span></button>`).join('');
  const full = w.games.length >= cap || bye.length < 2;
  return `<div class="hdr">${top}${chips}</div>${warn}
    <div class="list">${games || '<div class="status">No games this week.</div>'}</div>
    <div class="muted">On bye (${bye.length}): ${bye.length ? bye.map(t => esc(teamLabel(t))).join(', ') : 'none'}</div>
    <div class="bar"><div><button class="btn ghost" data-act="swk:-1" ${state.sweek > 0 ? '' : 'disabled'}>${ICON.left} Week</button>
      <button class="btn primary" data-act="sadd" ${full ? 'disabled' : ''}>Add game</button>
      <button class="btn ghost" data-act="swk:1" ${state.sweek < weeks.length - 1 ? '' : 'disabled'}>Week ${ICON.right}</button></div></div>${gameSheet()}`;
}

/** Centered dialog to add a game or change one: only teams on bye that week (and the game's own teams) can be picked. */
function gameSheet() {
  const g = state.sgame;
  if (!g) return '';
  const w = schedWeeks()[g.week], ids = teamIds();
  const own = g.idx == null ? [] : [w.games[g.idx].away, w.games[g.idx].home];
  const free = new Set([...own, ...S.byeTeams(w, ids)]);
  if (g.pick) {                                   // team grid: teams that cannot play are dimmed with the reason
    const other = g.pick === 'away' ? g.home : g.away;
    return `<div class="scrim pop fit" data-act="gclose"><div class="sheet pop fit" role="dialog" aria-label="Pick the ${g.pick} team" data-stop="1">
      <div class="topbar"><button class="btn icon-btn" data-act="gback" aria-label="Back">${ICON.left}</button><h2>${g.pick === 'away' ? 'Away' : 'Home'} team</h2><button class="btn icon-btn" data-act="gclose" aria-label="Close">&times;</button></div>
      <div class="muted">Week ${w.no}</div>
      ${teamGrid(ids, g[g.pick], 'gteam', t => (t === other ? (g.pick === 'away' ? 'Home team' : 'Away team') : free.has(t) ? null : 'Already playing'))}</div></div>`;
  }
  const same = g.away === g.home;
  return `<div class="scrim pop fit" data-act="gclose"><div class="sheet pop fit" role="dialog" aria-label="${g.idx == null ? 'Add game' : 'Edit game'}" data-stop="1">
      <div class="topbar"><h2>${g.idx == null ? 'Add game' : 'Edit game'}</h2><button class="btn icon-btn" data-act="gclose" aria-label="Close">&times;</button></div>
      <div class="muted" style="margin-top:6px">Week ${w.no}</div>
      ${fieldButton('Away team', esc(teamLabel(g.away)), 'gpick:away')}
      <div class="swaprow"><button class="btn ghost small" data-act="gswap">Swap home and away</button></div>
      ${fieldButton('Home team', esc(teamLabel(g.home)), 'gpick:home')}
      ${same ? '<div class="muted" role="alert">Pick two different teams.</div>' : ''}
      <div class="autobtns"><button class="btn primary" data-act="gok" ${same ? 'disabled' : ''}>OK</button>
        ${g.idx == null ? '' : '<button class="btn" data-act="grm">Remove game</button>'}<button class="btn ghost" data-act="gclose">Cancel</button></div>
    </div></div>`;
}

/** Schedule "By team": the team choice is the same bubble grid. */
function steamSheet(ids, team) {
  return `<div class="scrim pop fit" data-act="steamclose"><div class="sheet pop fit" role="dialog" aria-label="Pick a team" data-stop="1">
      <div class="topbar"><h2>Pick a team</h2><button class="btn icon-btn" data-act="steamclose" aria-label="Close">&times;</button></div>
      ${teamGrid(ids, team, 'steamset')}</div></div>`;
}

/** Scrolls the week strip so the current week sits in the middle. */
function centerWeekChip() {
  const on = document.querySelector('.weekchips .chip.on');
  if (on) { const box = on.parentElement; box.scrollLeft = on.offsetLeft - box.clientWidth / 2 + on.offsetWidth / 2; }
}

const groupPlayers = team => state.group === 'ALL' ? team.players : team.players.filter(p => p.group === state.group);
const family = () => familyOf(state.rom.version);
const hexSim = () => family() !== 'tsb1';
const simText = v => (hexSim() ? v.toString(16).toUpperCase().padStart(2, '0') : String(v));
const clamp = (v, lo, hi) => Math.max(lo, Math.min(hi, v));

// ---------- player editor ----------
function openDraft(lineIndex) {
  const p = parsePlayerLine(state.parsed.lines[lineIndex], family());
  const sp = p.name.indexOf(' ');
  state.edit = {
    line: lineIndex, orig: structuredClone(p), cur: p, sel: 0,
    first: sp < 0 ? '' : p.name.slice(0, sp), last: sp < 0 ? p.name : p.name.slice(sp + 1),
  };
}

function commitDraft() {
  state.facePicker = false;
  const d = state.edit;
  if (!d) return;
  d.cur.name = `${d.first} ${d.last}`.trim();
  const line = buildPlayerLine(d.cur, family());
  if (line === state.parsed.lines[d.line]) return;
  state.parsed.lines[d.line] = line;
  state.text = state.parsed.lines.join('\n');
  state.pending = true;
  const team = state.parsed.teams.find(t => t.id === state.team);
  const pl = team.players.find(x => x.lineIndex === d.line);
  Object.assign(pl, { name: d.cur.name, jersey: d.cur.jersey, face: '0x' + d.cur.face.toString(16) });
}

function playerScreen() {
  const d = state.edit, p = d.cur;
  const team = state.parsed.teams.find(t => t.id === state.team);
  const list = groupPlayers(team);
  const at = list.findIndex(x => x.lineIndex === d.line);
  const lab = labelsFor(family(), p.pos, p.attrs.length, p.sim.length);
  const sel = clamp(d.sel, 0, p.attrs.length - 1);
  const tiles = p.attrs.map((v, i) => `<button class="btn tile2 ${i === sel ? 'on' : ''}" data-act="attr:${i}" id="tile-${i}"><span class="k">${esc(lab.attrs[i])}</span><span class="v">${v}</span></button>`).join('');
  const sims = p.sim.map((v, i) => `<div class="card simbox"><span class="k">${esc(lab.sim[i])}</span>
      <div class="stepper"><button class="btn icon-btn" data-act="sim:${i}:-1" aria-label="${esc(lab.sim[i])} down">&minus;</button><span class="val">${simText(v)}</span>
      <button class="btn icon-btn" data-act="sim:${i}:1" aria-label="${esc(lab.sim[i])} up">+</button></div></div>`).join('');
  const code = lab.attrs[sel];
  return `<div class="hdr"><div class="topbar"><button class="btn ghost small" data-act="pl-done">${ICON.left} Roster</button>
      <button class="btn ghost small" data-act="undo">Undo</button></div>
    <div><div class="eyebrow">${esc(teamLabel(team.id))} &middot; ${esc(p.pos)}</div><h1>Edit player</h1></div></div>
    <div class="editgrid">
      ${faceCard(p)}
      <div class="names">
        <label class="field">First name<input id="first" value="${esc(d.first)}" autocomplete="off" autocapitalize="off" spellcheck="false"></label>
        <label class="field">Last name<input id="last" value="${esc(d.last)}" autocomplete="off" autocapitalize="off" spellcheck="false"></label>
      </div>
    </div>
    <div class="card jersey"><span class="k">Jersey #</span>
      <div class="stepper"><button class="btn icon-btn" data-act="jersey:-1" aria-label="Jersey down">&minus;</button><span class="val" id="jval">${p.jersey}</span>
      <button class="btn icon-btn" data-act="jersey:1" aria-label="Jersey up">+</button></div></div>
    <div class="muted" style="text-transform:uppercase;letter-spacing:.08em">Ratings</div>
    <div class="tiles4">${tiles}</div>
    <div class="card picker2"><div class="pk"><b id="pname">${esc(code)}${FULL[code] ? ' &middot; ' + esc(FULL[code]) : ''}</b><b id="pval" class="big">${p.attrs[sel]}</b></div>
      <div class="slidebar"><button class="btn icon-btn" data-act="rate:-1" aria-label="${esc(code)} down">&minus;</button>
        <input type="range" id="slide" min="0" max="${LADDER.length - 1}" step="1" value="${ladderIndex(p.attrs[sel])}" aria-label="${esc(code)} rating">
        <button class="btn icon-btn" data-act="rate:1" aria-label="${esc(code)} up">+</button></div>
      <div class="ladder"><span>${LADDER[0]}</span><span>${LADDER[4]}</span><span>${LADDER[8]}</span><span>${LADDER[12]}</span><span>${LADDER[15]}</span></div></div>
    ${sims ? `<div class="muted" style="text-transform:uppercase;letter-spacing:.08em">Sim</div><div class="simgrid">${sims}</div>` : ''}
    <div class="bar"><div><button class="btn ghost" data-act="pl-prev" ${at > 0 ? '' : 'disabled'}>${ICON.left} Prev</button>
      <button class="btn primary" data-act="pl-done">Done</button>
      <button class="btn ghost" data-act="pl-next" ${at >= 0 && at < list.length - 1 ? '' : 'disabled'}>Next ${ICON.right}</button></div></div>
    ${state.facePicker && sheets.faces ? facePickerSheet(p.face) : ''}`;
}

function faceCard(p) {
  const fk = faceKind(state.rom.version);
  if (fk.kind === 'portrait') {
    const pick = !!sheets.faces;                       // NES/CXROM portraits can be browsed
    const box = pick ? `<button class="facebox" data-act="face-open" aria-label="Choose a face">${faceBox(p.face, true)}</button>` : faceBox(p.face, true);
    return `<div class="card facecard">${box}
      <div class="stepper col"><button class="btn icon-btn" data-act="face:1" aria-label="Next face">${ICON.up}</button><span class="val sm">0x${p.face.toString(16)}</span>
      <button class="btn icon-btn" data-act="face:-1" aria-label="Previous face">${ICON.down}</button>
      ${pick ? '<button class="btn small" data-act="face-open">Pick</button>' : ''}</div></div>`;
  }
  const dark = isDark(p.face, fk.kind);   // these ROMs store a skin tone, not a portrait
  const skin = dark ? '#8a5a3c' : '#f0b070';
  const box = P.cache.on ? playerPhotoBox(state.team, p.pos)        // TSB3: his photo; the skin tone is a separate setting
    : `<div class="face big"><svg viewBox="0 0 40 46" aria-hidden="true"><circle cx="20" cy="17" r="10" fill="${skin}"/><path d="M3 46c0-12 7-18 17-18s17 6 17 18z" fill="#1f4fd8"/></svg></div>`;
  return `<div class="card facecard">${box}
    <div class="stepper col"><span class="k">Skin</span><div class="seg col" role="group" aria-label="Skin tone">
      <button data-act="skin:0" class="${dark ? '' : 'on'}">Light</button><button data-act="skin:1" class="${dark ? 'on' : ''}">Dark</button></div>
      ${P.cache.on ? `<span class="k">0x${p.face.toString(16)}</span>` : ''}</div></div>`;
}

function goPlayer(delta) {
  const team = state.parsed.teams.find(t => t.id === state.team);
  const list = groupPlayers(team);
  const at = list.findIndex(x => x.lineIndex === state.edit.line);
  commitDraft();
  const nxt = list[at + delta];
  if (nxt) { openDraft(nxt.lineIndex); location.hash = '#/player/' + nxt.lineIndex; render(); }
}

function textScreen() {
  return `<div class="hdr"><div class="topbar"><button class="btn ghost small" data-act="go:home">${ICON.left} Home</button>${themeSwitch()}</div>
    <div><h1>Text view</h1><div class="muted">Same format as the desktop app.</div></div>
    <div class="search"><input id="q" aria-label="Search" placeholder="Search" value="${esc(state.find)}">
      <button class="btn icon-btn" data-act="find-prev" aria-label="Previous match">${ICON.up}</button>
      <button class="btn icon-btn" data-act="find-next" aria-label="Next match">${ICON.down}</button></div>
    <div id="findinfo" class="muted"></div></div>${banner()}
    <textarea id="editor" spellcheck="false" autocapitalize="off" autocomplete="off" aria-label="ROM text">${esc(state.text)}</textarea>
    <div class="bar"><div><button class="btn ghost" data-act="revert">Revert</button><button class="btn primary" data-act="apply">Apply to ROM</button></div></div>`;
}

// ---------- wide layout (tablets, desktop browsers): rail + panes ----------
const wideQuery = window.matchMedia('(min-width: 900px) and (min-height: 540px)');

const screenFor = r => r === 'photos' && P.cache.on ? photosScreen() : r === 'roster' ? rosterScreen() : r === 'probowl' ? proBowlScreen() : r === 'schedule' ? scheduleScreen()
  : r === 'team' ? teamScreen() : r === 'player' ? playerScreen() : r === 'text' ? textScreen() : homeScreen();

function railHtml(r) {
  const on = r === 'player' ? 'roster' : r;
  const n = changedLines(state.baseline, state.text);
  const item = (key, icon, label, go) => `<button class="railbtn ${on === key ? 'on' : ''}" data-act="go:${go}" aria-label="${label}" ${on === key ? 'aria-current="page"' : ''}>${ICON[icon]}<span>${label}</span></button>`;
  return `<nav class="rail" aria-label="Sections"><div class="railmark">TSB</div>
    ${item('home', 'home', 'Home', 'home')}${item('roster', 'players', 'Players', 'roster')}${item('team', 'teams', 'Teams', 'team')}
    ${item('schedule', 'schedule', 'Schedule', 'schedule')}${item('probowl', 'probowl', 'Pro Bowl', 'probowl')}${P.cache.on ? item('photos', 'photo', 'Photos', 'photos') : ''}${item('text', 'text', 'Text', 'text')}
    <div class="railgrow"></div>${themeSwitch()}
    <button class="railbtn dl" data-act="download" aria-label="Download ROM">${ICON.dl}<span>${n ? n + ' edits' : 'Download'}</span></button></nav>`;
}

function emptyEditor() {
  return `<div class="emptyed"><div class="eyebrow">Nothing selected</div><div class="bigtitle">Pick a player</div>
    <div class="muted">Choose a player from the list to edit him here. Prev and Next in the editor move through the list.</div></div>`;
}

/** The wide page: a navigation rail, then either two panes (roster + editor) or one centered pane. */
function shellHtml(r) {
  const two = r === 'roster' || r === 'player';
  const body = two
    ? `<section class="pane list-pane" data-pane="list" data-pid="${state.team}:${state.group}"><div class="col">${rosterScreen()}</div></section>
       <section class="pane edit-pane" data-pane="edit" data-pid="${r === 'player' ? state.edit.line : 'none'}"><div class="col">${r === 'player' ? playerScreen() : emptyEditor()}</div></section>`
    : `<section class="pane main-pane" data-pane="main" data-pid="${r}:${state.team}"><div class="col">${screenFor(r)}</div></section>`;
  return `<div class="shell ${two ? 'two' : ''}">${railHtml(r)}${body}</div>`;
}

function render() {
  const wide = wideQuery.matches && !!state.rom;   // the tablet layout needs a loaded ROM; the landing page keeps the phone-style column
  document.body.className = `theme-${state.theme}${wide ? ' wide' : ''}`;
  const y = window.scrollY;
  const app = $('#app');
  if (state.busy) { app.innerHTML = `<div class="status">${esc(state.busy)}</div>`; return; }
  let r = state.rom ? route() : 'home';
  if (r === 'player') {                       // deep link: #/player/<line number>
    const line = Number(location.hash.split('/')[2]);
    if (!state.edit || state.edit.line !== line) {
      const known = state.parsed.teams.some(t => t.players.some(p => p.lineIndex === line));
      if (known) { state.team = state.parsed.teams.find(t => t.players.some(p => p.lineIndex === line)).id; openDraft(line); }
      else r = 'roster';
    }
  }
  if (r === 'team' && !realTeams().some(t => t.id === state.team)) state.team = realTeams()[0].id;   // the team editor is for real teams only
  if (r === 'team' && (!state.tedit || state.tedit.team !== state.team)) openTeamDraft(state.team);
  const kept = [...document.querySelectorAll('.pane')].map(el => [el.dataset.pane, el.dataset.pid, el.scrollTop]);   // each pane keeps its own scroll position
  app.innerHTML = wide && state.rom ? shellHtml(r) : screenFor(r);
  document.querySelectorAll('.pane').forEach(el => { const k = kept.find(x => x[0] === el.dataset.pane && x[1] === el.dataset.pid); if (k) el.scrollTop = k[2]; });
  if (state.lastRoute === r) window.scrollTo(0, y); else window.scrollTo(0, 0);   // keep scroll on in-place updates
  state.lastRoute = r;
  document.body.classList.toggle('modal-open', !!(state.accentDlg || state.facePicker || state.sheet || state.playPick || state.upick || state.usage || state.auto || state.sgame || state.pbEdit || state.steamDlg || state.photoDlg || state.photoEd || state.notice));
  if (state.accentDlg) app.insertAdjacentHTML('beforeend', accentSheet());
  if (state.photoDlg) app.insertAdjacentHTML('beforeend', photoSheet());
  if (state.photoEd) app.insertAdjacentHTML('beforeend', photoEditorSheet());
  if (state.notice) app.insertAdjacentHTML('beforeend', noticeSheet());
  wrapSheets();
  photoPaint();
  const dz = document.getElementById('phdrop');
  if (dz) { dz.addEventListener('dragover', ev => ev.preventDefault()); dz.addEventListener('drop', ev => { ev.preventDefault(); const f = ev.dataTransfer.files[0]; if (f) photoLoadFile(f); }); }
  applyAccent();
  paintUniforms();
  paintPicker();
  centerWeekChip();
  placePopup();
  const cur = $('.facegrid .on');
  if (cur) cur.scrollIntoView({ block: 'center' });   // open the face picker on the current face
  const drop = $('#drop');
  if (drop) wireDrop(drop);
}

/** Face picker: its top lines up with the page heading and it keeps the same gap below, so it sits centered vertically. */
function placePopup() {
  const scrim = $('.scrim.pop'), h = $('h1') || $('.team-head');
  if (!scrim || !h) return;
  const top = Math.min(Math.max(8, Math.round(h.getBoundingClientRect().top)), Math.max(8, (window.innerHeight - 240) / 2));
  scrim.style.setProperty('--pop-top', top + 'px');
}

// ---------- ROM handling ----------
const nextFrame = () => new Promise(r => setTimeout(r, 30));

async function loadRom(bytes, name) {
  state.busy = 'Reading ROM...'; state.error = ''; render(); await nextFrame();
  try {
    const version = web.LoadRom(bytes);
    if (!version) { state.error = 'Not a recognized Tecmo Super Bowl ROM.'; state.busy = ''; render(); return; }
    state.text = web.GetAll(1);
    state.baseline = state.text;
    state.parsed = parseText(state.text);
    idStats();
    state.team = state.parsed.teams[0] && state.parsed.teams[0].id;
    state.group = 'QB';
    state.messages = web.TakeMessages();
    const m = /\.([A-Za-z0-9]+)$/.exec(name);
    const ext = m && /^(smc|sfc|nes|md|bin|gen)$/i.test(m[1]) ? m[1].toLowerCase()
      : /^SNES/.test(version) ? 'smc' : /^GENESIS/.test(version) ? 'md' : 'nes';
    state.rom = { name, version, ext };
    state.photoDlg = null; state.photoEd = null;
    try { P.load(web); } catch (x) { P.cache.on = false; console.warn('photos:', x); }
    state.edit = null; state.tedit = null; state.pending = false; state.pb = null; state.pbEdit = null;
  } catch (e) {
    state.error = (/\.(md|bin|gen)$/i.test(name) && bytes.length === 2097152)
      ? 'This is a 2 MB Genesis ROM (a Tecmo Super Bowl II or III port). TSBTool cannot read those yet; it supports the Genesis Tecmo Super Bowl I.'
      : 'TSBTool could not read that file. It may be a type that is not supported yet.';
    console.warn('LoadRom failed:', e);
  }
  state.busy = ''; render();
}

async function applyText(text) {
  state.busy = 'Applying...'; render(); await nextFrame();
  try {
    web.ProcessText(text);
    state.messages = web.TakeMessages();
    state.error = '';
    state.text = web.GetAll(1);
    state.parsed = parseText(state.text);
    state.pending = false; state.edit = null; state.tedit = null;
  } catch (e) {
    state.error = 'Apply failed: ' + e.message;
  }
  state.busy = ''; render();
}

async function download() {
  if (state.pending) {                       // hand the pending edits to the core first
    await applyText(state.text);
    if (state.error) return;
  }
  saveBytes();
}

function saveBytes() {
  const base = state.rom.name.replace(/\.[^.]+$/, '');
  const blob = new Blob([web.GetRom()], { type: 'application/octet-stream' });
  const a = document.createElement('a');
  a.href = URL.createObjectURL(blob);
  a.download = `${base}_edited.${state.rom.ext}`;
  document.body.appendChild(a); a.click(); a.remove();
  setTimeout(() => URL.revokeObjectURL(a.href), 10000);
}

function wireDrop(el) {
  el.addEventListener('dragover', e => { e.preventDefault(); el.classList.add('over'); });
  el.addEventListener('dragleave', () => el.classList.remove('over'));
  el.addEventListener('drop', async e => {
    e.preventDefault(); el.classList.remove('over');
    const f = e.dataTransfer.files[0];
    if (f) loadRom(new Uint8Array(await f.arrayBuffer()), f.name);
  });
}

// ---------- find in text view ----------
function find(dir) {
  const ta = $('#editor'), q = $('#q').value;
  state.find = q;
  if (!ta || !q) return;
  const hay = ta.value.toLowerCase(), needle = q.toLowerCase();
  let i = dir > 0 ? hay.indexOf(needle, ta.selectionEnd) : hay.lastIndexOf(needle, Math.max(0, ta.selectionStart - 1));
  if (i < 0) i = dir > 0 ? hay.indexOf(needle) : hay.lastIndexOf(needle);
  $('#findinfo').textContent = i < 0 ? 'No match' : '';
  if (i < 0) return;
  ta.focus(); ta.setSelectionRange(i, i + needle.length);
  const line = hay.slice(0, i).split('\n').length - 1;
  const lh = parseFloat(getComputedStyle(ta).lineHeight) || 20;
  ta.scrollTop = Math.max(0, (line - 3) * lh);
}

function refreshFreePicker() {
  const p = state.upick, kind = state.tedit.colors.kind;
  const hex = U.freeFromLevels(kind, p.levels), rgb = U.freeRgb(kind, hex);
  $('#ubig').style.background = U.css(rgb);
  $('#uhex').textContent = hex;
  for (const ch of ['r', 'g', 'b']) {
    const out = $('#lv-' + ch), sl = document.querySelector(`[data-lev="${ch}"]`);
    if (out) out.textContent = p.levels[ch];
    if (sl) sl.value = p.levels[ch];
  }
  document.querySelectorAll('.psw').forEach(b => b.classList.toggle('on', b.dataset.act === `ulv:${p.levels.r},${p.levels.g},${p.levels.b}`));
  paintPicker();
}

/** Draws the saturation/brightness square (at the picker's hue) and the hue strip, with their markers. */
function paintPicker() {
  const p = state.upick, sv = document.getElementById('usv'), hu = document.getElementById('uhue');
  if (!p || !p.levels || !sv || !hu) return;
  const kind = state.tedit.colors.kind, hsv = U.rgbToHsv(U.freeRgb(kind, U.freeFromLevels(kind, p.levels)));
  if (p.h === undefined || (!pickDrag && hsv.s > 0.08)) p.h = hsv.h;
  const g = sv.getContext('2d'), W = sv.width, H = sv.height;
  const base = g.createLinearGradient(0, 0, W, 0);
  base.addColorStop(0, '#fff'); base.addColorStop(1, `rgb(${U.hsvToRgb({ h: p.h, s: 1, v: 1 })})`);
  g.fillStyle = base; g.fillRect(0, 0, W, H);
  const shade = g.createLinearGradient(0, 0, 0, H);
  shade.addColorStop(0, 'rgba(0,0,0,0)'); shade.addColorStop(1, '#000');
  g.fillStyle = shade; g.fillRect(0, 0, W, H);
  const mark = (x, y) => { g.lineWidth = 3; g.strokeStyle = '#000'; g.beginPath(); g.arc(x, y, 8, 0, 7); g.stroke(); g.lineWidth = 2; g.strokeStyle = '#fff'; g.beginPath(); g.arc(x, y, 8, 0, 7); g.stroke(); };
  mark(hsv.s * W, (1 - hsv.v) * H);
  const h = hu.getContext('2d'), w2 = hu.width, hg = h.createLinearGradient(0, 0, w2, 0);
  for (let i = 0; i <= 12; i++) hg.addColorStop(i / 12, `rgb(${U.hsvToRgb({ h: i * 30, s: 1, v: 1 })})`);
  h.fillStyle = hg; h.fillRect(0, 0, w2, hu.height);
  const hx = p.h / 360 * w2;
  h.fillStyle = '#000'; h.fillRect(hx - 3, 0, 6, hu.height); h.fillStyle = '#fff'; h.fillRect(hx - 1.5, 0, 3, hu.height);
}
let pickDrag = null;
function pickAt(e) {
  const p = state.upick, cv = pickDrag.cv, r = cv.getBoundingClientRect(), kind = state.tedit.colors.kind;
  const fx = Math.min(1, Math.max(0, (e.clientX - r.left) / r.width)), fy = Math.min(1, Math.max(0, (e.clientY - r.top) / r.height));
  if (cv.id === 'uhue') p.h = Math.min(359.9, fx * 360);
  const cur = U.rgbToHsv(U.freeRgb(kind, U.freeFromLevels(kind, p.levels)));
  const hsv = cv.id === 'uhue' ? { h: p.h, s: Math.max(cur.s, 0.01), v: Math.max(cur.v, 0.01) } : { h: p.h, s: fx, v: 1 - fy };
  p.levels = U.freeLevelsFromRgb(kind, U.hsvToRgb(hsv));
  refreshFreePicker();
}
document.addEventListener('pointerdown', e => {
  const cv = e.target.id === 'usv' || e.target.id === 'uhue' ? e.target : null;
  if (!cv || !state.upick || !state.upick.levels) return;
  pickDrag = { id: e.pointerId, cv };
  try { cv.setPointerCapture(e.pointerId); } catch { /* not a live pointer */ }
  pickAt(e); e.preventDefault();
});
document.addEventListener('pointermove', e => { if (pickDrag && e.pointerId === pickDrag.id) pickAt(e); });
const pickEnd = e => { if (pickDrag && e.pointerId === pickDrag.id) pickDrag = null; };
document.addEventListener('pointerup', pickEnd);
document.addEventListener('pointercancel', pickEnd);

// ---------- events ----------
function teamStep(d) {
  commitTeam(); state.tedit = null;
  const t = route() === 'team' ? realTeams() : state.parsed.teams, i = Math.max(0, t.findIndex(x => x.id === state.team));
  state.team = t[(i + d + t.length) % t.length].id; render();
}

document.addEventListener('click', e => {
  if (accentSuppressClick) { accentSuppressClick = false; return; }
  const el = e.target.closest('[data-act]');
  if (!el || (el.classList.contains('scrim') && e.target !== el)) return;
  const [act, arg] = el.dataset.act.split(/:(.*)/);
  switch (act) {
    case 'load': $('#file').click(); break;
    case 'theme': state.theme = arg; try { localStorage.setItem('tsb.theme', arg); } catch (x) { /* ignore */ } render(); break;
    case 'go': location.hash = '#/' + arg; break;
    case 'group': state.group = arg; render(); break;
    case 'tsim': { const [k, d] = arg.split(':'); const t = state.tedit.t; t[k] = clamp(t[k] + Number(d), 0, t.max); render(); break; }
    case 'tpref': state.tedit.t.pref = arg; render(); break;
    case 'ttab': state.ttab = arg; render(); break;
    case 'accentset': setAccent(arg); render(); break;
    case 'accentreset': setAccent(null); render(); break;
    case 'accentclose': state.accentDlg = false; render(); break;
    case 'tform': state.tedit.t.formation = arg; render(); break;
    case 'steampick': state.steamDlg = true; render(); break;
    case 'steamset': state.steam = arg; state.steamDlg = false; render(); break;
    case 'steamclose': state.steamDlg = false; render(); break;
    case 'gpick': state.sgame.pick = arg; render(); break;
    case 'gback': state.sgame.pick = null; render(); break;
    case 'gteam': { const g = state.sgame; g[g.pick] = arg; g.pick = null; render(); break; }
    case 'pbpickteam': state.pbEdit.view = 'teams'; render(); break;
    case 'pbback': state.pbEdit.view = null; render(); break;
    case 'pbteam': state.pbEdit.team = arg; state.pbEdit.view = null; render(); break;
    case 'pbpos': state.pbEdit.pos = arg; render(); break;
    case 'pbconf': state.pbconf = arg; render(); break;
    case 'pbedit': { const r = state.pb[state.pbconf][Number(arg)]; state.pbEdit = { conf: state.pbconf, spot: r.spot, team: r.team, pos: r.pos }; render(); break; }
    case 'pbclose': state.pbEdit = null; render(); break;
    case 'pbok': pbApply(); render(); break;
    case 'photo': state.photoDlg = { photo: Number(arg), mode: 'main' }; render(); break;
    case 'phclose': state.photoDlg = null; render(); break;
    case 'phback': state.photoDlg.mode = state.photoDlg.mode === 'team' ? 'assign' : 'main'; render(); break;
    case 'phchange': {
      const d = state.photoDlg, o = photoOwner(d.photo), team = o.team || state.team, tm = state.parsed.teams.find(t => t.id === team) || state.parsed.teams[0];
      d.mode = 'assign'; d.team = tm.id; d.pos = o.pos || (tm.players[0] && tm.players[0].pos); render(); break;
    }
    case 'phpickteam': state.photoDlg.mode = 'team'; render(); break;
    case 'phteam': { const d = state.photoDlg, tm = state.parsed.teams.find(t => t.id === arg); d.team = arg; if (!tm.players.some(p => p.pos === d.pos)) d.pos = tm.players[0].pos; d.mode = 'assign'; render(); break; }
    case 'phpos': state.photoDlg.pos = arg; render(); break;
    case 'phassign': { const d = state.photoDlg; web.AssignPhoto(d.photo, d.team, d.pos); P.refreshOwners(web); d.mode = 'main'; render(); break; }
    case 'phfree': { const d = state.photoDlg; web.FreePhoto(d.photo); P.refreshOwners(web); render(); break; }
    case 'phpic': { const i = state.photoDlg.photo; state.photoDlg = null; state.photoEd = { photo: i, img: null, zoom: 1, panx: 0.5, pany: 0, brightness: 0, contrast: 0, sharpen: 0.6, auto: true }; render(); break; }
    case 'phedclose': state.photoEd = null; render(); break;
    case 'phauto': state.photoEd.auto = !state.photoEd.auto; photoConvert(); render(); break;
    case 'phuse': photoUse(); break;
    case 'pl-photo': openPlayerPhoto(); render(); break;
    case 'noticeclose': state.notice = null; render(); break;
    case 'sview': state.sview = arg; render(); break;
    case 'sweek': state.sweek = Number(arg); render(); break;
    case 'sweekgo': state.sweek = Number(arg); state.sview = 'week'; render(); break;
    case 'swk': state.sweek += Number(arg); render(); break;
    case 'sgame': { const w = schedWeeks()[state.sweek], g = w.games[Number(arg)]; state.sgame = { week: state.sweek, idx: Number(arg), away: g.away, home: g.home }; render(); break; }
    case 'sadd': { const w = schedWeeks()[state.sweek], bye = S.byeTeams(w, teamIds()); state.sgame = { week: state.sweek, idx: null, away: bye[0], home: bye[1] }; render(); break; }
    case 'gswap': { const g = state.sgame; [g.away, g.home] = [g.home, g.away]; render(); break; }
    case 'gclose': state.sgame = null; render(); break;
    case 'gok': {
      const g = state.sgame;
      if (g.away === g.home) break;
      const games = schedWeeks()[g.week].games.map(x => ({ ...x })), ng = { away: g.away, home: g.home };
      if (g.idx == null) games.push(ng); else games[g.idx] = ng;
      setWeekGames(g.week, games); state.sgame = null; render(); break;
    }
    case 'grm': {
      const g = state.sgame, games = schedWeeks()[g.week].games.filter((_, i) => i !== g.idx);
      setWeekGames(g.week, games); state.sgame = null; render(); break;
    }
    case 'usel': state.usel = Number(arg); render(); break;
    case 'autosim': state.auto = { stage: 'ask' }; render(); break;
    case 'autorun': runAuto(arg === 's'); break;
    case 'autoclose': state.auto = null; render(); break;
    case 'autoundo': autoUndo(); break;
    case 'ucolor': {
      const [key, slot] = arg.split(':'), d = state.tedit, kind = d.colors.kind;
      if (kind === 'nes') state.upick = { key, slot: Number(slot) };
      else state.upick = { key, slot, levels: U.freeToLevels(kind, U.slotHex(U.get(d.colors.parts, key), slot)) };
      render(); break;
    }
    case 'ulev': { const [ch, dl] = arg.split(':'), p = state.upick, mx = U.LEVELS[state.tedit.colors.kind]; p.levels[ch] = clamp(p.levels[ch] + Number(dl), 0, mx); render(); break; }
    case 'uok': { const p = state.upick, d = state.tedit; uniSetColor(d, p.key, p.slot, U.freeFromLevels(d.colors.kind, p.levels)); state.upick = null; render(); break; }
    case 'ulv': { const [r, g, b] = arg.split(',').map(Number); state.upick.levels = { r, g, b }; state.upick.h = undefined; render(); break; }
    case 'ufine': state.upick.fine = !state.upick.fine; render(); break;
    case 'upal': { const p = state.upick; uniSetColor(state.tedit, p.key, p.slot, arg); state.upick = null; render(); break; }
    case 'uclose': state.upick = null; state.usage = null; render(); break;
    case 'uusage': { const orig = U.get(state.tedit.colors.parts, 'UniformUsage'); state.usage = { original: orig, ids: U.usageTeamIds(teamIds()) }; state.usage.bits = U.usageBits(orig, state.usage.ids.length); render(); break; }
    case 'utog': { const b = state.usage.bits, i = Number(arg); b[i] = !b[i]; render(); break; }
    case 'uall': state.usage.bits = state.usage.bits.map(() => true); render(); break;
    case 'unone': state.usage.bits = state.usage.bits.map(() => false); render(); break;
    case 'usave': { const u = state.usage; U.set(state.tedit.colors.parts, 'UniformUsage', U.usageHex(u.bits, u.original)); state.usage = null; render(); break; }
    case 'playopen': { const [k, i] = arg.split(':'); state.playPick = { key: k, i: Number(i) }; render(); break; }
    case 'playpick': { const { key, i } = state.playPick; state.tedit.pb[key][i] = Number(arg); state.playPick = null; render(); break; }
    case 'close-play': state.playPick = null; render(); break;
    case 'tdone': commitTeam(); state.tedit = null; location.hash = '#/roster'; break;
    case 'tundo': { const o = state.tedit.orig; state.tedit.t = structuredClone(o.t); state.tedit.pb = structuredClone(o.pb); state.tedit.id = structuredClone(o.id); state.tedit.colors = structuredClone(o.colors); render(); break; }
    case 'player': location.hash = '#/player/' + arg; break;
    case 'attr': state.edit.sel = Number(arg); render(); break;
    case 'rate': { const d = state.edit, i = ladderIndex(d.cur.attrs[d.sel]); d.cur.attrs[d.sel] = LADDER[clamp(i + Number(arg), 0, LADDER.length - 1)]; render(); break; }
    case 'jersey': state.edit.cur.jersey = clamp(state.edit.cur.jersey + Number(arg), 0, 99); render(); break;
    case 'face-open': state.facePicker = true; render(); break;
    case 'close-face': state.facePicker = false; render(); break;
    case 'facepick': state.edit.cur.face = Number(arg); state.facePicker = false; render(); break;
    case 'face': state.edit.cur.face = clamp(state.edit.cur.face + Number(arg), 0, faceKind(state.rom.version).max); render(); break;
    case 'skin': { const c = state.edit.cur; c.face = withTone(c.face, faceKind(state.rom.version).kind, arg === '1'); render(); break; }
    case 'sim': { const [i, d] = arg.split(':').map(Number); state.edit.cur.sim[i] = clamp(state.edit.cur.sim[i] + d, 0, 255); render(); break; }
    case 'undo': { const keep = state.edit.sel; openDraft(state.edit.line); state.edit.cur = structuredClone(state.edit.orig); state.edit.sel = keep; render(); break; }
    case 'pl-prev': goPlayer(-1); break;
    case 'pl-next': goPlayer(1); break;
    case 'pl-done': commitDraft(); state.edit = null; location.hash = '#/roster'; break;
    case 'team-prev': teamStep(-1); break;
    case 'team-next': teamStep(1); break;
    case 'team-open': state.sheet = true; render(); break;
    case 'team': commitTeam(); state.tedit = null; state.team = arg; state.sheet = false; render(); break;
    case 'close-sheet': state.sheet = false; render(); break;
    case 'download': download(); break;
    case 'find-next': find(1); break;
    case 'find-prev': find(-1); break;
    case 'apply': applyText($('#editor').value); break;
    case 'revert': $('#editor').value = state.text; break;
  }
});
document.addEventListener('input', e => {
  const ph = e.target.dataset && e.target.dataset.ph;
  if (ph && state.photoEd) {                       // picture editor sliders: update in place, convert shortly after the last move
    state.photoEd[ph] = Number(e.target.value);
    const lab = document.getElementById('phv-' + ph);
    if (lab) lab.textContent = state.photoEd[ph];
    clearTimeout(photoTimer);
    photoTimer = setTimeout(() => { photoConvert(); photoPaint(); }, 40);
    return;
  }
  if (e.target.id === 'accentcolor') {            // custom color: applies live while the box is open
    setAccent(e.target.value);
    const h = document.getElementById('accenthex'); if (h) h.textContent = e.target.value;
    return;
  }
  const u = state.upick, tt = e.target;
  if (u && u.levels && tt.dataset && tt.dataset.lev) {                // free-color picker: a channel slider moved
    u.levels[tt.dataset.lev] = Number(tt.value);
    refreshFreePicker();
    return;
  }
  if (u && u.levels && tt.id === 'unative') {                         // native color box: take the nearest level on each channel
    const v = tt.value, rgb = [1, 3, 5].map(i => parseInt(v.substr(i, 2), 16));
    u.levels = U.freeLevelsFromRgb(state.tedit.colors.kind, rgb);
    render();
    return;
  }
  const d = state.edit, t = e.target;
  if (t.dataset && t.dataset.idf && state.tedit && state.tedit.id) { idTyped(t); return; }
  if (!d) return;
  if (t.id === 'first') d.first = t.value;
  else if (t.id === 'last') d.last = t.value;
  else if (t.id === 'slide') {               // slider drags update in place so the thumb is not rebuilt mid-drag
    const v = LADDER[Number(t.value)];
    d.cur.attrs[d.sel] = v;
    $('#pval').textContent = v;
    $(`#tile-${d.sel} .v`).textContent = v;
  }
});
document.addEventListener('keydown', e => {
  if (e.target.id === 'q' && e.key === 'Enter') find(e.shiftKey ? -1 : 1);
  if (e.key === 'Escape' && (state.facePicker || state.sheet || state.playPick || state.upick || state.usage || state.auto || state.sgame || state.accentDlg || state.pbEdit || state.steamDlg || state.photoDlg || state.photoEd || state.notice)) { state.photoDlg = null; state.photoEd = null; state.notice = null; state.steamDlg = false; state.pbEdit = null; state.accentDlg = false; state.sgame = null; state.facePicker = false; state.sheet = false; state.playPick = null; state.upick = null; state.usage = null; state.auto = null; render(); }
});
$('#file').addEventListener('change', async e => {
  const f = e.target.files[0]; e.target.value = '';
  if (f) loadRom(new Uint8Array(await f.arrayBuffer()), f.name);
});
window.addEventListener('resize', () => { if (state.facePicker) render(); });
document.addEventListener('change', e => { if (e.target.id === 'phfile' && e.target.files[0]) { photoLoadFile(e.target.files[0]); e.target.value = ''; } });
// Drag the crop frame on "Your image" instead of using the left/right and up/down sliders.
let panDrag = null, panFrame = 0;
const pinch = new Map();      // active pointers on the preview, for two-finger zoom
let pinchStart = null;

/** Shows the picker state in its sliders and redraws (at most once per frame). */
function photoSync(keys) {
  const ed = state.photoEd;
  for (const key of keys) {
    const inp = document.querySelector(`[data-ph="${key}"]`), lab = document.getElementById('phv-' + key);
    if (inp) inp.value = ed[key];
    if (lab) lab.textContent = Math.round(ed[key] * 100) / 100;
  }
  if (!panFrame) panFrame = requestAnimationFrame(() => { panFrame = 0; photoConvert(); photoPaint(); });
}

/** Zooms around the middle of the crop frame so the picture does not jump. */
function photoZoomTo(z) {
  const ed = state.photoEd, im = ed.img;
  z = Math.min(4, Math.max(1, z));
  const base = web.PhotoDefaultCrop(im.w, im.h), ncw = base[2] / z, nch = base[3] / z;
  const mx = ed.crop.x + ed.crop.w / 2, my = ed.crop.y + ed.crop.h / 2;
  ed.zoom = Math.round(z * 100) / 100;
  ed.panx = im.w - ncw > 0 ? Math.min(1, Math.max(0, (mx - ncw / 2) / (im.w - ncw))) : 0.5;
  ed.pany = im.h - nch > 0 ? Math.min(1, Math.max(0, (my - nch / 2) / (im.h - nch))) : 0;
  photoSync(['zoom', 'panx', 'pany']);
}
document.addEventListener('wheel', e => {
  const ed = state.photoEd;
  if (e.target.id !== 'phsrc' || !ed || !ed.img || !ed.crop) return;
  e.preventDefault();
  photoZoomTo(ed.zoom * Math.exp(-e.deltaY * 0.0015));
}, { passive: false });
document.addEventListener('pointerdown', e => {
  const cv = e.target.id === 'phsrc' && e.target, ed = state.photoEd;
  if (!cv || !ed || !ed.img || !ed.crop) return;
  pinch.set(e.pointerId, { x: e.clientX, y: e.clientY });
  if (pinch.size === 2) { const [a, b] = [...pinch.values()]; pinchStart = { d: Math.hypot(a.x - b.x, a.y - b.y) || 1, zoom: ed.zoom }; panDrag = null; return; }
  const r = cv.getBoundingClientRect(), k = r.width / ed.img.w;
  panDrag = { id: e.pointerId, x: e.clientX, y: e.clientY, cx: ed.crop.x, cy: ed.crop.y, k };
  try { cv.setPointerCapture(e.pointerId); } catch { /* not a live pointer */ }
  e.preventDefault();
});
document.addEventListener('pointermove', e => {
  const ed = state.photoEd;
  if (pinch.has(e.pointerId)) pinch.set(e.pointerId, { x: e.clientX, y: e.clientY });
  if (pinchStart && pinch.size === 2 && ed) {
    const [a, b] = [...pinch.values()];
    photoZoomTo(pinchStart.zoom * (Math.hypot(a.x - b.x, a.y - b.y) / pinchStart.d));
    return;
  }
  if (!panDrag || e.pointerId !== panDrag.id || !ed) return;
  const im = ed.img, rx = im.w - ed.crop.w, ry = im.h - ed.crop.h;
  if (rx > 0) ed.panx = Math.min(1, Math.max(0, (panDrag.cx + (e.clientX - panDrag.x) / panDrag.k) / rx));
  if (ry > 0) ed.pany = Math.min(1, Math.max(0, (panDrag.cy + (e.clientY - panDrag.y) / panDrag.k) / ry));
  photoSync(['panx', 'pany']);
});
const panEnd = e => { pinch.delete(e.pointerId); if (pinch.size < 2) pinchStart = null; if (panDrag && e.pointerId === panDrag.id) panDrag = null; };
document.addEventListener('pointerup', panEnd);
document.addEventListener('pointercancel', panEnd);
const photoTarget = e => P.cache.on && !state.photoEd && e.target.closest && e.target.closest('[data-act="pl-photo"], .pcard');
document.addEventListener('dragover', e => { const t = photoTarget(e); if (t && e.dataTransfer && [...e.dataTransfer.types].includes('Files')) { e.preventDefault(); t.classList.add('over'); } });
document.addEventListener('dragleave', e => { const t = e.target.closest && e.target.closest('.over'); if (t) t.classList.remove('over'); });
document.addEventListener('drop', e => {
  const t = photoTarget(e), f = e.dataTransfer && e.dataTransfer.files[0];
  if (t && f) { e.preventDefault(); t.classList.remove('over'); photoDropped(t, f); }
});
wideQuery.addEventListener('change', () => render());     // rotating a tablet or resizing the window switches layouts
window.addEventListener('hashchange', () => {
  if (state.edit && route() !== 'player') { commitDraft(); state.edit = null; }
  if (state.tedit && route() !== 'team') { commitTeam(); state.tedit = null; }   // Back button keeps the edits
  render();
});

// ---------- boot ----------
state.busy = 'Starting...'; render();
const { getAssemblyExports, getConfig } = await dotnet.create();
web = (await getAssemblyExports(getConfig().mainAssemblyName)).WebHelper;
document.documentElement.dataset.webready = "1";   // tests wait for this before loading a ROM
state.ready = true; state.busy = ''; render();
const sheetsReady = loadSheets();

// Dev hooks: index.html?rom=<url>&theme=retro loads a ROM without the file picker
const themeParam = new URLSearchParams(location.search).get('theme');
if (themeParam === 'retro' || themeParam === 'modern') { state.theme = themeParam; render(); }
const romUrl = new URLSearchParams(location.search).get('rom');
if (romUrl) await loadRom(new Uint8Array(await (await fetch(romUrl)).arrayBuffer()), decodeURIComponent(romUrl.split('/').pop()));
{ const q = new URLSearchParams(location.search), wk = q.get('week'), sv = q.get('sview'); if (q.get('conf')) state.pbconf = q.get('conf'); if (q.get('team')) { state.team = q.get('team'); state.tedit = null; }   // dev hooks: &conf=NFC, &team=cowboys   // dev hooks: &week=<n>, &sview=team
  if (wk || sv) { if (wk) state.sweek = Number(wk) - 1; if (sv) state.sview = sv; render(); } }
if (new URLSearchParams(location.search).get('player') === 'first') { location.hash = '#/player/' + state.parsed.teams[0].players[0].lineIndex; }   // dev hook: &player=first
{ const pk = new URLSearchParams(location.search).get('picker');   // dev hook: &picker=1 face picker, &picker=team team picker
  const tb = new URLSearchParams(location.search).get('tab');          // dev hook: &tab=plays|identity
  if (tb) { state.ttab = tb; render(); }
  if (pk) { await sheetsReady; if (pk === 'team') state.sheet = true; else if (pk === 'play') state.playPick = { key: 'run', i: 0 };
    else if (pk === 'color' && state.tedit && state.tedit.colors) { const c = state.tedit.colors; state.upick = c.kind === 'nes' ? { key: 'Uniform1', slot: 2 } : { key: 'Uniform1', slot: 'JerseyM', levels: U.freeToLevels(c.kind, U.slotHex(U.get(c.parts, 'Uniform1'), 'JerseyM')) }; }
    else if (pk === 'auto') state.auto = { stage: 'ask' };
    else if (pk === 'gpick') { const w = schedWeeks()[state.sweek], g = w.games[0]; state.sgame = { week: state.sweek, idx: 0, away: g.away, home: g.home, pick: 'away' }; }
    else if (pk === 'steam') state.steamDlg = true;
    else if (pk === 'photo') state.photoDlg = { photo: 0, mode: 'main' };
    else if (pk === 'photoassign') { state.photoDlg = { photo: 0, mode: 'assign', team: 'bills', pos: 'QB2' }; }
    else if (pk === 'photoedit') {
      const cv = document.createElement('canvas'); cv.width = 160; cv.height = 120;
      const x = cv.getContext('2d'), g = x.createRadialGradient(80, 55, 5, 80, 65, 75);
      g.addColorStop(0, '#fff'); g.addColorStop(1, '#222'); x.fillStyle = g; x.fillRect(0, 0, 160, 120);
      x.fillStyle = '#000'; x.beginPath(); x.arc(62, 50, 9, 0, 7); x.arc(98, 50, 9, 0, 7); x.fill(); x.fillRect(60, 85, 40, 6);
      const dd = x.getImageData(0, 0, 160, 120).data, argb = new Int32Array(160 * 120);
      for (let i = 0; i < argb.length; i++) argb[i] = (dd[i * 4 + 3] << 24) | (dd[i * 4] << 16) | (dd[i * 4 + 1] << 8) | dd[i * 4 + 2];
      state.photoEd = { photo: 0, img: { w: 160, h: 120, argb, canvas: cv }, zoom: 1.4, panx: 0.5, pany: 0.2, brightness: 0, contrast: 0, sharpen: 0.6, auto: true };
      photoConvert();
    }
    else if (pk === 'pbteam') { if (!state.pb) pbLoad(); const r = state.pb[state.pbconf][0]; state.pbEdit = { conf: state.pbconf, spot: r.spot, team: r.team, pos: r.pos, view: 'teams' }; }
    else if (pk === 'pb') { if (!state.pb) pbLoad(); const r = state.pb[state.pbconf][0]; state.pbEdit = { conf: state.pbconf, spot: r.spot, team: r.team, pos: r.pos }; }
    else if (pk === 'accent') state.accentDlg = true;
    else if (pk === 'game') { const w = schedWeeks()[state.sweek], g = w.games[0]; state.sgame = { week: state.sweek, idx: 0, away: g.away, home: g.home }; }
    else if (pk === 'addgame') { const w = schedWeeks()[state.sweek], bye = S.byeTeams(w, teamIds()); state.sgame = { week: state.sweek, idx: null, away: bye[0], home: bye[1] }; }
    else if (pk === 'usage' && state.tedit && state.tedit.colors) { const o = U.get(state.tedit.colors.parts, 'UniformUsage'); const ids = U.usageTeamIds(teamIds()); state.usage = { original: o, ids, bits: U.usageBits(o, ids.length) }; }
    else state.facePicker = true; render(); } }
{ const sc = new URLSearchParams(location.search).get('scroll');         // dev hook: &scroll=<px> scrolls the page after load
  if (sc) { await sheetsReady; await new Promise(r => setTimeout(r, 1500)); window.scrollTo(0, Number(sc)); await new Promise(r => setTimeout(r, 300));
    const h = document.querySelector('.hdr'), b = document.querySelector('.bar');
    document.documentElement.dataset.scrolltest = JSON.stringify({ scrollY: Math.round(window.scrollY), docH: document.documentElement.scrollHeight, winH: window.innerHeight, hdrTop: h && Math.round(h.getBoundingClientRect().top), hdrBottom: h && Math.round(h.getBoundingClientRect().bottom), barTop: b && Math.round(b.getBoundingClientRect().top) }); } }
{ const ac = new URLSearchParams(location.search).get('accent');          // dev hook: &accent=22d3ee
  if (ac) { setAccent('#' + ac); render(); } }
{ const seq = new URLSearchParams(location.search).get('clicks');          // dev hook: &clicks=act1,act2 clicks [data-act] elements in order (for tests)
  if (seq) { await sheetsReady; for (const a of seq.split(',')) { await new Promise(r => setTimeout(r, 300)); const el = document.querySelector(`[data-act="${a}"]:not([disabled])`); if (el) el.click(); else document.documentElement.dataset.clickmiss = (document.documentElement.dataset.clickmiss || '') + a + ' '; } } }
