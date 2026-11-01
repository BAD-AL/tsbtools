// The in-app program guide. Each section is { id, title, html }; the html may use <p>, <ul>, <img src="img/guide/...">, <a href>.
// Add sections here (and screenshots under img/guide/) as the guide grows.
export const GUIDE = [
  { id: 'start', title: 'Getting started', html: `
    <p>TSBTool Web edits Tecmo Super Bowl ROMs right in your browser. Nothing is uploaded; the program runs on your device.</p>
    <ol>
      <li>Press <b>Load ROM</b> (or drop a ROM file on the page). NES / CXROM, SNES TSB1, TSB2 and TSB3, and Genesis TSB1 ROMs are recognized.</li>
      <li>Pick what to edit from the Home page: players, teams, the schedule, the Pro Bowl, or the text view.</li>
      <li>Press <b>Download ROM</b> to get your edited ROM. Your original file is never changed.</li>
    </ol>` },
  { id: 'players', title: 'Players', html: `
    <p>Choose a team, then a player, to change the name, face, and ratings. Prev and Next in the editor move through the roster.
    Press <b>Done</b> to keep your changes; they are applied to the ROM when you download it.</p>` },
  { id: 'teams', title: 'Teams', html: `
    <p>Team editing covers the team's name and abbreviation, sim data, formations, playbooks, and (on SNES and Genesis ROMs) uniform colors.</p>
    <p>For TSB2 and TSB3 uniforms, <b>Start from...</b> fills the 12 colors from any team's original uniform, <b>Reset</b> goes back to this team's original, and
    <b>Copy to away</b> / <b>Copy to home</b> duplicates one uniform onto the other. The match option keeps each dark shade in step with its light shade.
    Your own saved presets are kept in this browser only.</p>` },
  { id: 'schedule', title: 'Schedule and Pro Bowl', html: `
    <p>The schedule is edited week by week; a game can't put a team in two places at once, and the program warns you about problems.
    The Pro Bowl screen lists the AFC and NFC rosters.</p>` },
  { id: 'text', title: 'Text view', html: `
    <p>Every edit is really a change to a plain-text description of the ROM, and that text is the single source of truth.
    The text view shows it directly, which is handy for copying a whole roster, searching, or pasting in changes made elsewhere.
    The same text works in the desktop TSBTool.</p>` },
  { id: 'themes', title: 'Styles and team themes', html: `
    <p><b>Modern</b> and <b>Retro</b> only change the look. Press the helmet (or right-click / long-press the Modern button) to choose one of 32 team themes for the Modern style.</p>` },
  { id: 'privacy', title: 'Privacy and saved settings', html: `
    <p>Your ROM stays on your device. Only a few preferences (style, team theme, saved uniform presets) are remembered, in this browser's own storage.
    Clearing the site's data removes them.</p>` },
];
