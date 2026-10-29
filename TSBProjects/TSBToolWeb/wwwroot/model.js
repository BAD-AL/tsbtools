// Parses the TSBTool text dump into teams and players. The text stays the source of truth:
// edits change lines of text and the core applies them with ProcessText.

export const GROUPS = ['QB', 'RB', 'WR', 'TE', 'OL', 'DL', 'LB', 'DB', 'K/P'];

const PLAYER_RE = /^([A-Za-z0-9_]+),\s*(.+?),\s*Face=(0x[0-9a-fA-F]+),\s*#(\d+)\s*,?\s*(.*)$/;
const TEAM_RE = /^TEAM\s*=\s*([^\s,]+)/i;

export function groupOf(pos) {
  const p = pos.toUpperCase();
  if (/^QB/.test(p)) return 'QB';
  if (/^RB/.test(p)) return 'RB';
  if (/^WR/.test(p)) return 'WR';
  if (/^TE/.test(p)) return 'TE';
  if (/^(C|LG|RG|LT|RT)$/.test(p)) return 'OL';
  if (/^(RE|LE|NT)\d*$/.test(p)) return 'DL';
  if (/^(LB|ROLB|LOLB|RILB|LILB)\d*$/.test(p)) return 'LB';
  if (/^(RCB|LCB|FS|SS|DB)\d*$/.test(p)) return 'DB';
  if (/^(K|P)$/.test(p)) return 'K/P';
  return 'DB';
}

export function parseText(text) {
  const lines = text.split(/\r?\n/);
  const teams = [];
  let cur = null;
  lines.forEach((line, i) => {
    const t = TEAM_RE.exec(line);
    if (t) {
      cur = { id: t[1].toLowerCase(), lineIndex: i, players: [], real: /SimData\s*=/i.test(line) };   // TSB3 also lists freeAgents and allTime, which have no SimData
      teams.push(cur);
      return;
    }
    if (!cur) return;
    if (/^(WEEK|SEASON|YEAR)\b/i.test(line)) { cur = null; return; }
    const m = PLAYER_RE.exec(line);
    if (m) cur.players.push({ lineIndex: i, pos: m[1], name: m[2], face: m[3], jersey: Number(m[4]), group: groupOf(m[1]) });
  });
  return { lines, teams };
}

/** Splits one player line into editable fields, remembering the line's punctuation so it can be rebuilt the same way. */
export function parsePlayerLine(line, family) {
  const sm = /^(.*?)(\s*,?\s*)\[([^\]]*)\]\s*$/.exec(line);
  const body = sm ? sm[1] : line;
  const tokens = body.split(',').map(s => s.trim()).filter(s => s !== '');
  const inner = sm ? sm[3] : '';
  const simTokens = inner.split(',').map(s => s.trim()).filter(s => s !== '');
  const hex = family !== 'tsb1';               // TSB2/3 write sim values as two hex digits
  return {
    pos: tokens[0],
    name: tokens[1],
    face: parseInt(/Face=0x([0-9a-fA-F]+)/.exec(tokens[2])[1], 16),
    jersey: Number(/#(\d+)/.exec(tokens[3])[1]),
    attrs: tokens.slice(4).map(Number),
    sim: simTokens.map(t => (hex ? parseInt(t, 16) : Number(t))),
    fmt: {
      sep: body.includes(', ') ? ', ' : ',',
      tail: sm ? '' : (/[\s,]*$/.exec(body)[0]),
      simJoin: sm ? sm[2] : '',
      simSep: inner.includes(', ') ? ', ' : ',',
      simTrail: /\s$/.test(inner) ? ' ' : '',
      hex, simLen: simTokens.map(t => t.length),
    },
  };
}

export function buildPlayerLine(p, family) {
  const f = p.fmt;
  const face = p.face.toString(16);
  const fields = [p.pos, p.name, `Face=0x${family === 'tsb1' ? face : face.padStart(2, '0')}`, `#${p.jersey}`, ...p.attrs];
  let out = fields.join(f.sep);
  if (p.sim.length) {
    const simText = p.sim.map((v, i) => (f.hex ? v.toString(16).toUpperCase().padStart(f.simLen[i] || 2, '0') : String(v))).join(f.simSep) + f.simTrail;
    out += `${f.simJoin}[${simText}]`;
  } else {
    out += f.tail;
  }
  return out;
}

export const FORMATIONS = ['2RB_2WR_1TE', '1RB_3WR_1TE', '1RB_4WR'];

/** "TEAM = bills SimData=0xe53, OFFENSIVE_FORMATION = 2RB_2WR_1TE" -> pieces; null when the line has no SimData. */
export function parseTeamLine(line) {
  const m = /^(TEAM\s*=\s*[^\s,]+[\s,]+SimData=0x)([0-9a-fA-F]+)(.*)$/i.exec(line);
  if (!m) return null;
  const f = /OFFENSIVE_FORMATION\s*=\s*(\S+)/.exec(m[3]);
  return { prefix: m[1], digits: m[2], rest: m[3], formation: f ? f[1] : null };
}

export function buildTeamLine(t) {
  const rest = t.formation === null ? t.rest : t.rest.replace(/(OFFENSIVE_FORMATION\s*=\s*)\S+/, `$1${t.formation}`);
  return t.prefix + t.digits + rest;
}

/** "PLAYBOOK R1215, P7321 " (TSB1 family: four run slots, four pass slots, play 1-8 each); null for other formats. */
export function parsePlaybook(line) {
  const m = /^PLAYBOOK\s+R([1-8]{4}),\s*P([1-8]{4})(\s*)$/.exec(line || '');
  return m ? { run: [...m[1]].map(Number), pass: [...m[2]].map(Number), tail: m[3] } : null;
}

export function buildPlaybook(p) {
  return `PLAYBOOK R${p.run.join('')}, P${p.pass.join('')}${p.tail}`;
}

const SPECIAL_LABELS = { freeagents: 'Free agents', alltime: 'All-time' };

export function teamLabel(id) {
  if (SPECIAL_LABELS[id]) return SPECIAL_LABELS[id];
  return id.charAt(0).toUpperCase() + id.slice(1);
}

/** Number of lines that differ between two texts (cheap "edits pending" indicator). */
export function changedLines(a, b) {
  const x = a.split(/\r?\n/), y = b.split(/\r?\n/);
  let n = Math.abs(x.length - y.length);
  for (let i = 0; i < Math.min(x.length, y.length); i++) if (x[i] !== y[i]) n++;
  return n;
}
