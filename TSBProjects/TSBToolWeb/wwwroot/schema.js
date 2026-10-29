// What each position's rating and sim columns mean, per ROM family. Mirrors the Key in the desktop dump and AttributeForm.

/** Values a rating can take (the game stores a 4-bit step). */
export const LADDER = [6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100];

export function familyOf(version) {
  if (/TSB3/i.test(version)) return 'tsb3';
  if (/TSB2/i.test(version)) return 'tsb2';
  return 'tsb1';          // NES, CXROM, SNES TSB1, Genesis
}

/**
 * What the "Face" number means differs per ROM (see the SetFace implementations in the core):
 * portrait = NES/CXROM portrait id, photo = TSB3 photo number, skin = SNES TSB1/Genesis skin tone only
 * (below 0x53 is light, otherwise dark), race = TSB2/TSB3 bit 0x80 (0 light, 8 dark in the high nibble).
 */
export function faceKind(version) {
  if (/^(NES|CXROM)/i.test(version)) return { kind: 'portrait', max: 0xD4 };
  if (/TSB[23]/i.test(version)) return { kind: 'race', max: 0xFF };   // TSB3's photos are separate: see photos.js
  return { kind: 'skin', max: 0xFF };
}

export function isDark(face, kind) {
  return kind === 'race' ? (face >> 4) >= 8 : face >= 0x53;
}

export function withTone(face, kind, dark) {
  if (kind === 'race') return dark ? (face & 0x0f) | 0x80 : face & 0x0f;
  return dark ? face | 0x80 : face & 0x7f;
}

export function kindOf(pos) {
  const p = pos.toUpperCase();
  if (/^QB/.test(p)) return 'QB';
  if (/^(RB|WR|TE)/.test(p)) return 'SKILL';
  if (/^(C|LG|RG|LT|RT)$/.test(p)) return 'OL';
  if (p === 'K') return 'K';
  if (p === 'P') return 'P';
  return 'DEF';
}

const BASE = ['RS', 'RP', 'MS', 'HP'];
const SCHEMA = {
  tsb1: {
    QB:    { attrs: [...BASE, 'PS', 'PC', 'ACC', 'APB'], sim: ['SIM RUN', 'SIM PASS', 'POCKET'] },
    SKILL: { attrs: [...BASE, 'BC', 'REC'], sim: ['RUSH', 'CATCH', 'YPC', 'TARGETS'] },
    OL:    { attrs: [...BASE], sim: [] },
    DEF:   { attrs: [...BASE, 'PI', 'QU'], sim: ['PASS RUSH', 'COVERAGE'] },
    K:     { attrs: [...BASE, 'KA', 'AKB'], sim: ['KICKING'] },
    P:     { attrs: [...BASE, 'KA', 'AKB'], sim: ['KICKING'] },
  },
  tsb2: {
    QB:    { attrs: [...BASE, 'BB', 'PS', 'PC', 'PA', 'AR', 'CO'], sim: [] },
    SKILL: { attrs: [...BASE, 'BB', 'BC', 'RC'], sim: [] },
    OL:    { attrs: [...BASE, 'BB'], sim: [] },
    DEF:   { attrs: [...BASE, 'BB', 'PI', 'QU'], sim: [] },
    K:     { attrs: [...BASE, 'BB', 'KP', 'KA', 'AB'], sim: [] },
    P:     { attrs: [...BASE, 'BB', 'KP', 'AB'], sim: [] },
  },
  tsb3: {
    QB:    { attrs: [...BASE, 'BB', 'AG', 'PS', 'PC', 'PA', 'AR', 'CO'], sim: [] },
    SKILL: { attrs: [...BASE, 'BB', 'AG', 'BC', 'RC'], sim: [] },
    OL:    { attrs: [...BASE, 'BB', 'AG'], sim: [] },
    DEF:   { attrs: [...BASE, 'BB', 'AG', 'PI', 'QU'], sim: [] },
    K:     { attrs: [...BASE, 'BB', 'AG', 'KP', 'KA', 'AB'], sim: [] },
    P:     { attrs: [...BASE, 'BB', 'AG', 'KP', 'AB'], sim: [] },
  },
};

/** Descriptions for the codes whose meaning is documented (TSB1 family). */
export const FULL = {
  RS: 'Rushing speed', RP: 'Rushing power', MS: 'Max speed', HP: 'Hitting power',
  PS: 'Passing speed', PC: 'Pass control', ACC: 'Accuracy', APB: 'Avoid pass block',
  BC: 'Ball control', REC: 'Receptions', PI: 'Pass interceptions', QU: 'Quickness',
  KA: 'Kicking ability', AKB: 'Avoid kick block',
};

/** Labels for a player's columns; the line decides how many there are, the schema names them. */
export function labelsFor(family, pos, attrCount, simCount) {
  const s = SCHEMA[family][kindOf(pos)];
  const attrs = Array.from({ length: attrCount }, (_, i) => s.attrs[i] || `A${i + 1}`);
  const sim = Array.from({ length: simCount }, (_, i) => s.sim[i] || `SIM ${i + 1}`);
  return { attrs, sim };
}

/** Nearest ladder index for a value (so odd values still land on a slider stop). */
export function ladderIndex(v) {
  let best = 0;
  for (let i = 1; i < LADDER.length; i++) if (Math.abs(LADDER[i] - v) < Math.abs(LADDER[best] - v)) best = i;
  return best;
}
