// The schedule section of the dump: "WEEK n" followed by "away at home" lines (team ids, as in the rest of the text).

const WEEK_RE = /^WEEK\s*(\d+)/i;
const GAME_RE = /^\s*(\S+)\s+at\s+(\S+)\s*$/i;

/** Weeks in the text: [{ no, header, count, games: [{ away, home }] }]. `header` is the line index of "WEEK n". */
export function parseSchedule(lines) {
  const weeks = [];
  for (let i = 0; i < lines.length; i++) {
    const h = WEEK_RE.exec(lines[i]);
    if (!h) continue;
    const games = [];
    let j = i + 1;
    for (; j < lines.length; j++) {
      const m = GAME_RE.exec(lines[j]);
      if (!m) break;
      games.push({ away: m[1], home: m[2] });
    }
    weeks.push({ no: Number(h[1]), header: i, count: j - i - 1, games });
    i = j - 1;
  }
  return weeks;
}

export const gameLine = g => `${g.away} at ${g.home}`;

const playing = week => week.games.flatMap(g => [g.away, g.home]);

/** Teams with no game that week. */
export function byeTeams(week, teamIds) {
  const busy = new Set(playing(week));
  return teamIds.filter(t => !busy.has(t));
}

/** Teams booked more than once in a week. */
export function doubleBooked(week) {
  const seen = new Map();
  for (const t of playing(week)) seen.set(t, (seen.get(t) || 0) + 1);
  return [...seen].filter(([, n]) => n > 1).map(([t]) => t);
}

/** What a team does in each week: { opp, home } or null for a bye (null as well when the team has no game). */
export function teamSeason(weeks, team) {
  return weeks.map(w => {
    const g = w.games.find(x => x.away === team || x.home === team);
    return g ? { opp: g.away === team ? g.home : g.away, home: g.home === team } : null;
  });
}
