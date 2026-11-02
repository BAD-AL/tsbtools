// The in-app program guide. Each section is { id, title, html }; the html may use <p>, <ul>, <img src="img/guide/...">, <a href>.
// Add sections here (and screenshots under img/guide/) as the guide grows.
/** A row of screenshots with captions (the images are in img/guide/). */
const shots = (...items) => `<div class="shots">${items.map(([file, caption]) =>
  `<figure><img src="img/guide/${file}.png" alt="${caption}" loading="lazy"><figcaption>${caption}</figcaption></figure>`).join('')}</div>`;

export const GUIDE = [
  { id: 'start', title: 'Getting started', html: `
    <p>TSBTool Web edits Tecmo Super Bowl ROMs right in your browser. Nothing is uploaded; the program runs on your device.</p>
    <ol>
      <li>Press <b>Load ROM</b> (or drop a ROM file on the page). NES / CXROM, SNES TSB1, TSB2 and TSB3, and Genesis TSB1 ROMs are recognized.</li>
      <li>Pick what to edit from the Home page: players, teams, the schedule, the Pro Bowl, or the text view.</li>
      <li>Press <b>Download ROM</b> to get your edited ROM. Your original file is never changed.</li>
    </ol>
    ${shots(['home', 'The Home page after a ROM is loaded'])}` },
  { id: 'players', title: 'Players', html: `
    <p>Choose a team, then a player, to change the name, face, and ratings. Prev and Next in the editor move through the roster.
    Press <b>Done</b> to keep your changes; they are applied to the ROM when you download it.</p>
    <p>Tap the face to pick another one, or use the arrows to step through them. On TSB3 ROMs the face is a photo: you can swap in a picture of your own,
    and the program shows how it will look in the game.</p>
    ${shots(['playerselect', 'Pick a team, a position, then a player'], ['editplayer', 'Edit a player'], ['faceselect-nes', 'Pick a face'])}
    ${shots(['photoedit1-tsb3', 'A TSB3 photo'], ['photoedit2-tsb3', 'Fit your own picture to the photo'])}` },
  { id: 'teams', title: 'Teams', html: `
    <p>Team editing covers the team's name and abbreviation, sim data, formations, playbooks, and (on SNES and Genesis ROMs) uniform colors.</p>
    <p>For TSB2 and TSB3 uniforms, <b>Start from...</b> fills the 12 colors from any team's original uniform, <b>Reset</b> goes back to this team's original, and
    <b>Copy to away</b> / <b>Copy to home</b> duplicates one uniform onto the other. The match option keeps each dark shade in step with its light shade.
    Your own saved presets are kept in this browser only.</p>
    ${shots(['editteam-general', 'General: sim data, run/pass ratio, formation'], ['editplaybook-nes', 'Plays: tap a play to choose another'], ['edituniform-nes', 'Uniform colors'])}` },
  { id: 'schedule', title: 'Schedule and Pro Bowl', html: `
    <p>The schedule is edited week by week; a game can't put a team in two places at once, and the program warns you about problems.
    Tap a game to change its teams, <b>Add game</b> adds one to the week, and the trash can removes one.
    The Pro Bowl screen lists the AFC and NFC rosters.</p>
    ${shots(['editschedule', 'The schedule, week by week'])}` },
  { id: 'text', title: 'Text view', html: `
    <p>Every edit is really a change to a plain-text description of the ROM, and that text is the single source of truth.
    The text view shows it directly, which is handy for copying a whole roster, searching, or pasting in changes made elsewhere.
    The same text works in the desktop TSBTool.</p>
    <p>Press <b>Find...</b> (or Ctrl+F) to search; Enter jumps to the match, and F3 goes on to the next one. <b>Apply to ROM</b> puts your text edits into the ROM; <b>Revert</b> throws them away.</p>
    ${shots(['edittext', 'The text view'])}` },
  { id: 'themes', title: 'Styles and team themes', html: `
    <p>Press <b>Style</b> to change the look: Modern, Retro, Cyber, Y2K, Aqua or Windows XP. Styles only change how the app looks. For the Modern style, <b>Team colors and highlight...</b> in that dialog (or the helmet button) offers 32 team themes and a custom highlight color.</p>` },
  { id: 'privacy', title: 'Privacy and saved settings', html: `
    <p>Your ROM stays on your device. Only a few preferences (style, team theme, saved uniform presets) are remembered, in this browser's own storage.
    Clearing the site's data removes them.</p>` },
];
