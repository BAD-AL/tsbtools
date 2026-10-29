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
  if (/^SNES_TSB1$/.test(version)) return 'snes';
  if (/^GENESIS/.test(version)) return 'genesis';
  if (/^(NES|CXROM)/.test(version)) return 'nes';
  return null;           // TSB2/TSB3 have no COLORS line
}

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

export function slotHex(uniHex, id) {
  const i = FREE_SLOTS.findIndex(s => s.id === id);
  return uniHex.substr(i * 4, 4);
}
export function withSlot(uniHex, id, hex4) {
  const i = FREE_SLOTS.findIndex(s => s.id === id);
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

export function freeRgb(kind, hex4) {
  const l = kind === 'snes' ? snesToLevels(hex4) : genesisToLevels(hex4), max = LEVELS[kind];
  return [Math.round(l.r * 255 / max), Math.round(l.g * 255 / max), Math.round(l.b * 255 / max)];
}
export function freeLevelsFromRgb(kind, [r, g, b]) {
  const max = LEVELS[kind], q = c => Math.max(0, Math.min(max, Math.round(c * max / 255)));
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
  nes_div: 'img/uni_nes_div.png', nes_conf: 'img/uni_nes_conf.png',
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
