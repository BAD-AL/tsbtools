// Uniform colors: parsing the COLORS line, color formats per ROM family, and recoloring the preview pictures.
import { NES_PALETTE } from './nes_palette.js';

export { NES_PALETTE };

/** "COLORS Uniform1=0x..., Uniform2=0x..., UniformUsage=0x..." -> ordered key/value pairs (null if not a COLORS line). */
export function parseColors(line) {
  if (!/^COLORS\b/.test(line || '')) return null;
  const parts = [];
  for (const m of line.matchAll(/(\w+)=0x([0-9a-fA-F]+)/g)) parts.push({ key: m[1], hex: m[2] });
  return parts.length ? parts : null;
}

export function buildColors(parts) {
  return 'COLORS ' + parts.map(p => `${p.key}=0x${p.hex}`).join(', ');
}

export const get = (parts, key) => (parts.find(p => p.key === key) || {}).hex;
export const set = (parts, key, hex) => { const p = parts.find(x => x.key === key); if (p) p.hex = hex; };

/** Which kind of color editor a ROM gets: 'snes' (15-bit), 'genesis' (9-bit), 'nes' (64-color palette), or null. */
export function colorKind(version) {
  if (/^SNES_TSB[123]$/.test(version)) return 'snes';
  if (/^GENESIS/.test(version)) return 'genesis';
  if (/^(NES|CXROM)/.test(version)) return 'nes';
  return null;
}
/** TSB2/TSB3 keep 12 colors per uniform (Uniform1 = home, Uniform2 = away) instead of TSB1's seven. */
export const isTsb23 = version => /^SNES_TSB[23]$/.test(version);

/** TSB2/TSB3 colors (48 hex digits, 12 colors): the game's palette entries 1-6 and 9-14. */
export const TSB23_SLOTS = [
  { id: 'tHelmetD', group: 'Helmet', label: 'Dark' }, { id: 'tHelmetL', group: 'Helmet', label: 'Light' },
  { id: 'tJerseyD', group: 'Jersey', label: 'Dark' }, { id: 'tJerseyL', group: 'Jersey', label: 'Light' },
  { id: 'tPantsD', group: 'Pants', label: 'Dark' }, { id: 'tPantsL', group: 'Pants', label: 'Light' },
  { id: 'tSocksD', group: 'Socks', label: 'Light' }, { id: 'tSocksL', group: 'Socks', label: 'Dark' },   // (the ROM keeps the lighter sock shade first)
  { id: 'tPStripeD', group: 'Pants stripe', label: 'Dark' }, { id: 'tPStripeL', group: 'Pants stripe', label: 'Light' },
  { id: 'tHStripeD', group: 'Helmet stripe', label: 'Dark' }, { id: 'tHStripeL', group: 'Helmet stripe', label: 'Light' },
];
export const TSB23_GROUPS = ['Jersey', 'Pants', 'Helmet', 'Socks', 'Pants stripe', 'Helmet stripe'];

// ---- free-color formats: seven 4-hex-digit colors per uniform ----
/** Order of the seven colors inside Uniform1/Uniform2, as the desktop editor reads them. */
export const FREE_SLOTS = [
  { id: 'PantsD', group: 'Pants', label: 'Dark' }, { id: 'JerseyM', group: 'Jersey', label: 'Medium' },
  { id: 'PantsM', group: 'Pants', label: 'Medium' }, { id: 'JerseyL', group: 'Jersey', label: 'Light' },
  { id: 'PantsL', group: 'Pants', label: 'Light' }, { id: 'HelmetD', group: 'Helmet', label: 'Dark' },
  { id: 'HelmetM', group: 'Helmet', label: 'Medium' },
];
/** Display order of the slots (the desktop dialog groups Jersey, Pants, Helmet). */
export const FREE_DISPLAY = ['JerseyM', 'JerseyL', 'PantsD', 'PantsM', 'PantsL', 'HelmetD', 'HelmetM'];

/** Genesis in-game colors (the "InGame=0x" value, six colors): what the game field uses, two shades per part. */
export const INGAME_SLOTS = [
  { id: 'iHelmetD', group: 'Helmet', label: 'Dark' }, { id: 'iHelmetL', group: 'Helmet', label: 'Light' },
  { id: 'iJerseyD', group: 'Jersey', label: 'Dark' }, { id: 'iJerseyL', group: 'Jersey', label: 'Light' },
  { id: 'iPantsD', group: 'Pants', label: 'Dark' }, { id: 'iPantsL', group: 'Pants', label: 'Light' },
];
export const slotInfo = id => FREE_SLOTS.find(s => s.id === id) || INGAME_SLOTS.find(s => s.id === id) || TSB23_SLOTS.find(s => s.id === id);
const slotIndex = id => {
  for (const list of [FREE_SLOTS, INGAME_SLOTS, TSB23_SLOTS]) { const i = list.findIndex(s => s.id === id); if (i >= 0) return i; }
  return -1;
};

export function slotHex(uniHex, id) {
  return uniHex.substr(slotIndex(id) * 4, 4);
}
export function withSlot(uniHex, id, hex4) {
  const i = slotIndex(id);
  return uniHex.slice(0, i * 4) + hex4 + uniHex.slice(i * 4 + 4);
}

// SNES: two bytes, low byte first, 5 bits each of red, green, blue.
export function snesToLevels(hex4) {
  const v = parseInt(hex4.substr(0, 2), 16) | (parseInt(hex4.substr(2, 2), 16) << 8);
  return { r: v & 31, g: (v >> 5) & 31, b: (v >> 10) & 31 };
}
export function snesFromLevels({ r, g, b }) {
  const v = (r & 31) | ((g & 31) << 5) | ((b & 31) << 10);
  return (v & 255).toString(16).padStart(2, '0') + ((v >> 8) & 255).toString(16).padStart(2, '0');
}
// Genesis: one word written high byte first, nibbles 0-B-G-R, each an even value 0-14 (3 bits per channel).
export function genesisToLevels(hex4) {
  const n = i => parseInt(hex4[i], 16) >> 1;
  return { r: n(3), g: n(2), b: n(1) };
}
export function genesisFromLevels({ r, g, b }) {
  return '0' + (b * 2).toString(16) + (g * 2).toString(16) + (r * 2).toString(16);
}

export const LEVELS = { snes: 31, genesis: 7 };

/** What the Genesis emulator shows for the 8 levels of a channel (measured from screenshots; not level * 255 / 7). */
export const GENESIS_SHADES = [0x00, 0x34, 0x57, 0x74, 0x90, 0xac, 0xce, 0xff];
export function freeRgb(kind, hex4) {
  const l = kind === 'snes' ? snesToLevels(hex4) : genesisToLevels(hex4), max = LEVELS[kind];
  if (kind === 'genesis') return [GENESIS_SHADES[l.r], GENESIS_SHADES[l.g], GENESIS_SHADES[l.b]];
  return [Math.round(l.r * 255 / max), Math.round(l.g * 255 / max), Math.round(l.b * 255 / max)];
}
export function freeLevelsFromRgb(kind, [r, g, b]) {
  const max = LEVELS[kind];
  if (kind === 'genesis') {            // nearest of the 8 displayed shades
    const q = c => GENESIS_SHADES.reduce((best, v, i) => (Math.abs(v - c) < Math.abs(GENESIS_SHADES[best] - c) ? i : best), 0);
    return { r: q(r), g: q(g), b: q(b) };
  }
  const q = c => Math.max(0, Math.min(max, Math.round(c * max / 255)));
  return { r: q(r), g: q(g), b: q(b) };
}
export const freeToLevels = (kind, hex4) => (kind === 'snes' ? snesToLevels(hex4) : genesisToLevels(hex4));
export const freeFromLevels = (kind, l) => (kind === 'snes' ? snesFromLevels(l) : genesisFromLevels(l));

export const css = ([r, g, b]) => `rgb(${r},${g},${b})`;
export const nesRgb = hex2 => NES_PALETTE[parseInt(hex2, 16) & 63] || [0, 0, 0];
export const readableOn = ([r, g, b]) => ((r * 299 + g * 587 + b * 114) / 1000 > 140 ? '#000' : '#fff');

// ---- NES: three palette bytes per uniform (pants, skin, jersey) ----
export const NES_SLOTS = [{ id: 'pants', label: 'Pants' }, { id: 'skin', label: 'Skin' }, { id: 'jersey', label: 'Jersey' }];
export function nesSlotHex(uniHex, i) { return uniHex.substr(i * 2, 2); }
export function nesWithSlot(uniHex, i, hex2) { return uniHex.slice(0, i * 2) + hex2 + uniHex.slice(i * 2 + 2); }

/** Champion pictures: which bytes of DivChamp / ConfChamp color which pixel group. */
export const CHAMP = {
  DivChamp: { image: 'nes_div', slots: [['Uni 1', 'mDivChampUniform1'], ['Uni 2', 'mDivChampUniform2'], ['Uni 3', 'mDivChampUniform3'], ['Helm 1', 'mDivChampHelmet1'], ['Helm 2', 'mDivChampHelmet2']] },
  ConfChamp: { image: 'nes_conf', slots: [['Uni 1', 'mConfChampUniform1'], ['Uni 2', 'mConfChampUniform2'], ['Uni 3', 'mConfChampUniform3'], ['Helm 1', 'mConfChampHelmet']] },
};

// ---- picture assets ----
const IMAGES = {
  snes_home: 'img/uni_snes_home.png', snes_away: 'img/uni_snes_away.png',
  nes_home: 'img/uni_nes_home.png', nes_away: 'img/uni_nes_away.png',
  tsb23: 'img/uni_tsb23.png', coin: 'img/uni_coin.png', nes_div: 'img/uni_nes_div.png', nes_conf: 'img/uni_nes_conf.png',
};
let assets = null;

export async function loadAssets() {
  if (assets) return assets;
  const masks = await (await fetch('img/uniform_masks.json')).json();
  const imgs = {};
  await Promise.all(Object.entries(IMAGES).map(([k, url]) => new Promise((res, rej) => {
    const i = new Image(); i.onload = () => { imgs[k] = i; res(); }; i.onerror = rej; i.src = url;
  })));
  for (const k of ['nes_home', 'nes_away']) imgs[k] = clearBackground(imgs[k]);   // their pure-white backdrop would swallow white uniforms
  assets = { masks, imgs };
  return assets;
}
/** Turns the white backdrop (white pixels connected to the picture's edge) transparent; white inside the figure is left alone. */
function clearBackground(img) {
  const w = img.naturalWidth, h = img.naturalHeight, c = document.createElement('canvas');
  c.width = w; c.height = h;
  const ctx = c.getContext('2d');
  ctx.drawImage(img, 0, 0);
  const data = ctx.getImageData(0, 0, w, h), px = data.data, seen = new Uint8Array(w * h), stack = [];
  const push = (x, y) => {
    if (x < 0 || y < 0 || x >= w || y >= h) return;
    const k = y * w + x;
    if (seen[k]) return;
    seen[k] = 1;
    const i = k * 4;
    if (px[i + 3] > 0 && px[i] > 245 && px[i + 1] > 245 && px[i + 2] > 245) stack.push(k);
  };
  for (let x = 0; x < w; x++) { push(x, 0); push(x, h - 1); }
  for (let y = 0; y < h; y++) { push(0, y); push(w - 1, y); }
  while (stack.length) {
    const k = stack.pop(), x = k % w, y = (k / w) | 0;
    px[k * 4 + 3] = 0;
    push(x + 1, y); push(x - 1, y); push(x, y + 1); push(x, y - 1);
  }
  ctx.putImageData(data, 0, 0);
  return c;
}

export const assetsReady = () => assets;

/**
 * Genesis coin-toss player: the pixel groups S0-S15 are the 16 palette entries of the team's coin-toss block (mask set 'coin').
 * Entries the text does not carry get typical values: jersey index 0 is the Medium jersey darkened, skin is the light/dark pair
 * every team shares, 8-10 are gray/white/black and 14-15 the facemask gray/white.
 */
export function coinGroups(uniHex, dark) {
  const rgb = hex4 => freeRgb('genesis', hex4), L = id => genesisToLevels(slotHex(uniHex, id));
  const jm = L('JerseyM'), jersey0 = genesisFromLevels({ r: jm.r >> 1, g: jm.g >> 1, b: jm.b >> 1 });
  const colors = [jersey0, slotHex(uniHex, 'JerseyM'), slotHex(uniHex, 'JerseyL'), slotHex(uniHex, 'PantsD'), slotHex(uniHex, 'PantsM'), slotHex(uniHex, 'PantsL'),
    dark ? '0224' : '044a', dark ? '0468' : '08ae', '0444', '0eee', '0000', '0000', slotHex(uniHex, 'HelmetD'), slotHex(uniHex, 'HelmetM'), '0444', '0eee'];
  return colors.map((h, i) => ['S' + i, rgb(h)]);
}

/** Draws a picture on a canvas, then paints each [maskName, [r,g,b]] group of pixels over it. */
export function paint(canvas, imageKey, maskSet, groups) {
  const a = assets;
  if (!a) return;
  const img = a.imgs[imageKey];
  canvas.width = img.naturalWidth || img.width; canvas.height = img.naturalHeight || img.height;
  const ctx = canvas.getContext('2d');
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  ctx.drawImage(img, 0, 0);
  for (const [maskName, rgb] of groups) {
    const px = a.masks[maskSet][maskName];
    if (!px) continue;
    ctx.fillStyle = css(rgb);
    for (let i = 0; i < px.length; i += 2) ctx.fillRect(px[i], px[i + 1], 1, 1);
  }
}

/**
 * The order of the usage bits on 28-team ROMs. It is the desktop dialog's (and the NES's) team order, which is NOT the
 * dump order on SNES/Genesis (Cowboys/Redskins and Falcons/49ers are swapped there). 32-team ROMs use the dump order.
 */
export const NES_TEAM_ORDER = ['bills', 'colts', 'dolphins', 'patriots', 'jets', 'bengals', 'browns', 'oilers', 'steelers', 'broncos', 'chiefs', 'raiders',
  'chargers', 'seahawks', 'redskins', 'giants', 'eagles', 'cardinals', 'cowboys', 'bears', 'lions', 'packers', 'vikings', 'buccaneers', '49ers', 'rams', 'saints', 'falcons'];
export function usageTeamIds(dumpIds) {
  return dumpIds.length === 28 && NES_TEAM_ORDER.every(id => dumpIds.includes(id)) ? NES_TEAM_ORDER : dumpIds;
}

// ---- "use Uniform 2 against" mask: one bit per team, team 1 in the top bit ----
export function usageBits(hex, teamCount) {
  const v = BigInt('0x' + (hex || '0'));
  return Array.from({ length: teamCount }, (_, i) => ((v >> BigInt(31 - i)) & 1n) === 1n);
}
export function usageHex(bits, original) {
  let v = 0n;
  bits.forEach((on, i) => { if (on) v |= 1n << BigInt(31 - i); });
  const keepMask = (1n << BigInt(32 - bits.length)) - 1n;          // bits beyond the last team stay as they were
  v |= BigInt('0x' + (original || '0')) & keepMask;
  const s = v.toString(16).padStart(8, '0');
  return /[A-F]/.test(original || '') ? s.toUpperCase() : s;
}

// ---- color picker helpers (hue strip, saturation/brightness square, palette grid) ----
export function rgbToHsv([r, g, b]) {
  r /= 255; g /= 255; b /= 255;
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  let h = 0;
  if (d) h = mx === r ? ((g - b) / d + 6) % 6 : mx === g ? (b - r) / d + 2 : (r - g) / d + 4;
  return { h: h * 60, s: mx ? d / mx : 0, v: mx };
}
export function hsvToRgb({ h, s, v }) {
  const f = n => { const k = (n + h / 60) % 6; return v - v * s * Math.max(0, Math.min(k, 4 - k, 1)); };
  return [f(5), f(3), f(1)].map(x => Math.round(x * 255));
}
/** The game's colors as a grid: a gray ramp, then 12 hues in four shades (bright, light, medium, dark). Each is snapped to the console's levels. */
export function paletteLevels(kind) {
  const snap = rgb => freeLevelsFromRgb(kind, rgb), key = l => `${l.r},${l.g},${l.b}`, seen = new Set(), rows = [];
  const add = (row, l) => { const k = key(l); if (!seen.has(k)) { seen.add(k); row.push(l); } };
  const grays = [];
  for (let i = 0; i <= 7; i++) add(grays, snap(Array(3).fill(Math.round(i * 255 / 7))));
  rows.push(grays);
  for (const [s, v] of [[1, 1], [0.5, 1], [1, 0.6], [1, 0.35]]) {
    const row = [];
    for (let h = 0; h < 360; h += 30) add(row, snap(hsvToRgb({ h, s, v })));
    rows.push(row);
  }
  return rows;
}

// ---- TSB2/TSB3 presets and the "match dark to light" option ----
/** Dark and light slot of each TSB2/TSB3 color group (socks keep their lighter shade in the first slot). */
const TSB23_PAIRS = [['tHelmetD', 'tHelmetL'], ['tJerseyD', 'tJerseyL'], ['tPantsD', 'tPantsL'], ['tSocksL', 'tSocksD'], ['tPStripeD', 'tPStripeL'], ['tHStripeD', 'tHStripeL']];
/** The stock colors have a dark shade at about this fraction of their light shade (median over all stock uniforms). */
const DARK_SHADE = 0.48;
/** Sets one color and, when the game keeps a dark/light pair, the other color of the pair derived from it. */
export function withMatchedSlot(uniHex, id, hex4) {
  const out = withSlot(uniHex, id, hex4);
  const pair = TSB23_PAIRS.find(p => p.includes(id));
  if (!pair) return out;
  const isDark = pair[0] === id, other = isDark ? pair[1] : pair[0], l = snesToLevels(hex4);
  const f = c => Math.max(0, Math.min(31, Math.round(isDark ? c / DARK_SHADE : c * DARK_SHADE)));
  return withSlot(out, other, snesFromLevels({ r: f(l.r), g: f(l.g), b: f(l.b) }));
}
let presetData = null;
/** The stock uniforms of every team: { SNES_TSB2: { bills: [home, away], ... }, SNES_TSB3: {...} }. */
export async function loadPresets() {
  if (!presetData) presetData = await (await fetch('img/uniform_presets.json')).json();
  return presetData;
}
export const presetsReady = () => presetData;
/** A 12-color uniform from "COLORS Uniform1=0x..., Uniform2=0x..." or one bare 48-digit hex value: the list of [label, hex]. */
export function parsePresetText(text) {
  const t = (text || '').trim();
  if (/^(0x)?[0-9a-fA-F]{48}$/.test(t)) return [['', t.replace(/^0x/, '').toLowerCase()]];
  const parts = parseColors(t) || [];
  return parts.filter(p => /^Uniform[12]$/.test(p.key) && p.hex.length === 48).map(p => [p.key === 'Uniform1' ? 'home' : 'away', p.hex.toLowerCase()]);
}
