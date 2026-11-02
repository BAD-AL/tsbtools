// Modern-style team themes: [id, name, highlight color, glow color 1 (large, top left), glow color 2 (top right)].
// "vikings" is the built-in look (no overrides). Highlight colors are chosen to read on the dark glass background.
export const DEFAULT_THEME = 'vikings';
/** The pseudo theme that follows the team being viewed. */
export const CURRENT_TEAM = 'team';
export const TEAM_THEMES = [
  ['49ers', '49ers', '#e6bd6e', '#b50d0d', '#b3995d'],
  ['bears', 'Bears', '#ff7a2b', '#1c4a8a', '#e0560b'],
  ['bengals', 'Bengals', '#ff7a1a', '#e0580c', '#f5a000'],
  ['bills', 'Bills', '#cf644a', '#0a3cc0', '#c60c30'],
  ['broncos', 'Broncos', '#ff6a1a', '#0a2a8a', '#fb4f14'],
  ['browns', 'Browns', '#ff7a2f', '#6b3a1a', '#e8590c'],
  ['buccaneers', 'Buccaneers', '#ff5a3c', '#d50a0a', '#7a6652'],
  ['cardinals', 'Cardinals', '#ffffff', '#97233f', '#3a3a3a'],
  ['chargers', 'Chargers', '#3aa8ff', '#0073cf', '#ffc20e'],
  ['chiefs', 'Chiefs', '#ffd23f', '#c8102e', '#e0b020'],
  ['colts', 'Colts', '#5aa0ff', '#1f4aa8', '#9fb4cc'],
  ['commanders', 'Redskins', '#ffb612', '#8a1f36', '#c99700'],
  ['cowboys', 'Cowboys', '#8fb4e8', '#14407a', '#869397'],
  ['dolphins', 'Dolphins', '#2dd4cf', '#008e97', '#fc4c02'],
  ['eagles', 'Eagles', '#3fd0a0', '#0b6b5c', '#a5acaf'],
  ['falcons', 'Falcons', '#ffffff', '#a71930', '#3a4048'],
  ['giants', 'Giants', '#5a8cf7', '#0b2d9a', '#a71930'],
  ['jaguars', 'Jaguars', '#2ec4b6', '#006778', '#d7a22a'],
  ['jets', 'Jets', '#3ddc84', '#0c8a5a', '#9aa5a0'],
  ['lions', 'Lions', '#4aa8ff', '#0076b6', '#b0b7bc'],
  ['packers', 'Packers', '#ffc20e', '#1f6b45', '#d4a000'],
  ['panthers', 'Panthers', '#3aa8e8', '#0085ca', '#a5acaf'],
  ['patriots', 'Patriots', '#6b9bff', '#1b3f9c', '#c60c30'],
  ['raiders', 'Raiders', '#c8cdd4', '#3a4452', '#7c8590'],
  ['rams', 'Rams', '#ffd100', '#003ea8', '#ffa300'],
  ['ravens', 'Ravens', '#b794f6', '#5a30b0', '#9e7c0c'],
  ['saints', 'Saints', '#d9b86a', '#8a7240', '#3a3a3a'],
  ['seahawks', 'Seahawks', '#69be28', '#0a3a6a', '#69be28'],
  ['steelers', 'Steelers', '#ffc20e', '#3a3f4a', '#c99700'],
  ['texans', 'Texans', '#ff4d5e', '#12386a', '#c8102e'],
  ['titans', 'Titans', '#4ba3e3', '#0a3a7a', '#c8102e'],
  ['vikings', 'Vikings', '#a78bfa', '#6366f1', '#0ea5e9'],
];
export const themeById = id => TEAM_THEMES.find(t => t[0] === id);
/** ROM team ids that differ from the theme ids (older team names). */
const TEAM_ALIASES = { redskins: 'commanders', oilers: 'titans' };
/** The theme id for a ROM team id, or null when there is none (TSB's free agents, all-time teams, unknown names). */
export const themeForTeam = id => { const t = TEAM_ALIASES[id] || id; return themeById(t) ? t : null; };
