# NES TSB graphics map (original 28-team ROM, `TSPRBOWL.nes`)

Status: research, 2026-10-27. Offsets are **PRG offsets** (file offset = PRG + 0x10) unless marked "file".
CHR-ROM starts at file `0x40010`; 1KB CHR bank `n` is at file `0x40010 + n*0x400`.

ROM layout: iNES mapper 4 (MMC3). Original = 256KB PRG + 128KB CHR. The 32-team ROM
(`TSB 2007-32-105.nes`) = 256KB PRG + **256KB CHR** (the extra graphics space has to come from somewhere).

## Large helmet (team data screen)

Three layers, matching Helmet Editor 0.10's manual (`tools/HelmetEditor-0.10`):

| Layer | Drawn as | CHR | Palette |
|---|---|---|---|
| Shell | 8×7 background tiles, nametable cols 4–11, rows 4–10 | BG `$0800` = MMC3 R1 = banks `$7A/$7B`, tiles `$80–$FF` | BG palette 3 |
| Logo | sprites | sprite page set by the logo header (`$70` Bills, `$71` Rams) | sprite palette 3 |
| Facemask | 12 sprites, same for every team | bank `$73`, tiles `$C0–$CC` | sprite palette 2 |

### Team tables (bank `$22000`, CPU `$A000`, routine at `$A8C0`)
Indexed by team (`$6C`), 28 entries each, TSB team order (BUF IND MIA NE NYJ CIN CLE HOU PIT DEN KC RAI SD SEA WAS NYG PHI PHX DAL CHI DET GB MIN TB SF RAMS NO ATL):

- `$BE49` (PRG `0x23E49`) **shell number**: `40 40 40 40 41 43 40 40 41 40 41 41 41 41 40 40 41 40 40 41 40 40 41 40 40 42 40 40`
  - 4 shells: `$40` / `$41` are shared (presumably no stripe / stripe), `$43` = Bengals only, `$42` = Rams only.
- `$BE65` (PRG `0x23E65`) **logo sprite-group ID**: `3e 3d 3f 40 31 32 33 34 35 36 37 41 42 3a 45 3c 43 44 3b 46 47 48 49 4a 4e 4c 4d 4b`

- `$BD69` (PRG `0x23D69`) **helmet palette**, 8 bytes per team (copied by `$A98E` to the palette buffer):
  `[shell c1 c2 c3] [facemask c2 c3] [logo c1 c2 c3]` → BG palette 3 colors 1–3, sprite palette 2 colors 2–3, sprite palette 3 colors 1–3.
  Bills `30 16 12 10 30 30 12 26`; Rams `25 30 21 15 25 25 21 00`. Verified against the palette RAM in `bills.mss`.

### Pointer tables
- **Sprite group ID → header**: PRG `0x10000 + 2*ID`, little-endian CPU pointer. `$A000–$BFFF` → PRG `0x10000 + (ptr - $A000)`;
  `$8000–$9FFF` → PRG `0x2E000 + (ptr - $8000)` (the sprite-engine bank; the 32-team hack put its new headers there).
  Original IDs `$31–$4E` all point to `$A5F3–$A7A0` (14 bytes per header, 15 for a few).
- **Shell → screen script**: PRG `0x5000 + 2*shell` (`$40`→`$BA98` = PRG `0x5A98`, then `$BAA3`, `$BAAE`, `$BAB9`, 11 bytes each).
  Script `ff 2a 02 1c 02 02 09 53 54 55 56 ff` leads the BG drawer (`$2C211`) to the shell tiles at `0x16584` etc. Script format not decoded yet.

### Sprite-group ("metasprite") format (decoded 2026-10-27 by patch experiments)
The headers are a small command script. Examples:

```
Bills logo  0x106C0: f6 70 ff ff ff | fc 03 | f8 36 39 | 91 a2 | ab 17 ...
Rams  logo  0x10784: f6 71 ff ff ff | fc 1b | f8 32 3a | 94 45 | ab 17 ...
Facemask    0x107AE: f6 ff 73 ff ff | fc 02 | f8 42 3e | 94 92 | ab 17 ...
```
- `f6 a b c d` = **set the 4 sprite CHR banks** (`ff` = leave alone). Bills logo uses bank `$70`, Rams `$71`, facemask `$73`.
- `f8 Y X` = base position, **Y first**, both in 2-pixel units.
- `91 a2` = big-endian pointer (high bit set) → sprite data at PRG `0x10000 + (ptr & 0x7FFF)` = `0x111A2`.
- `ab 17` = probably a jump/call to shared tail (PRG `0x10B17`).
- `fc nn` in the header: read by the engine, but no visible effect on placement or attributes (tested 10 values on Bills and 8 on Rams).

**Screen position (from the decoder at `$9B0F`, bank `$17`; verified in Mesen):**
`x = 2*X + 8*column + fineX - 60`, `y(OAM+1) = 2*Y + 8*row + fineY - 59` (the −60 is the `$C4` the copy routine at `$A91D` adds).

Sprite data stream (Bills, PRG `0x111A2`):
```
9e  01 06  05 0d  09 14      column -2: (y=1,tile 06) (y=5,tile 0d) (y=9,tile 14)
9f  01 07  05 18  09 15      column -1
80  01 12  05 19             column 0
81  01 13  05 1c             column +1
82  01 16                    column +2
fc 32                        modifier (second grid shifted +2px)
9f  3d 04   80 3d 05   81 3d 10   82 3d 11
ff                           end
```
- A byte ≥ `$80` starts a **column**: low 5 bits = signed column number, 8 px per column.
- Then `(s, tile)` pairs follow (`s` < `$80`). **Corrected 2026-10-28 from the decoder code** (an earlier note here read `s` as a signed 6-bit y in 2px units, which only worked because every ROM logo uses the same palette bits):
  - bits 2–5 = **row**, signed 4-bit, 8 px per row
  - bits 0–1 = **sprite palette** (every logo uses 1, the facemask 0; the copy routine ORs in `$02` → palettes 3 and 2)
  - bit 6 = **horizontal flip** (EOR'd with the group's direction flag, `$9C22`). No ROM logo uses it yet, but it works, so a mirrored tile can share CHR.
  - The tile index gets `| $80` added at run time (routine `$A91D`), so it points into the upper half of the 1KB logo page.
- `fc nn` inside the data stream (handlers at `$9C87`):
  - `fc 2n` = **fineY** = signed 4-bit n, absolute (stored in slot `$11`, added to every later sprite's y)
  - `fc 3n` (and `4n`–`Fn`) = **fineX** = signed 4-bit n: stored absolute in slot `$12` for later columns, **and added to the current column's x right away**. An encoder should put `fc 3n` before the column byte; then the column starts at exactly `2X + 8*col + n - 60`.
  - `fc 00` sets flag bit 2 (vertical flip); `fc 01`–`fc 1F` clears it.
  - fineX and fineY both start at 0 for each group (`$9B06`).
- `ff` = end.
- Sprite order = OAM order = priority: the first sprite wins where two overlap.

Data locations seen: Bills logo `0x111A2–0x111CB` (16 sprites, 42 bytes), Rams logo `0x11445–0x11459` (7 sprites), facemask `0x11492–0x114B6`.

### Shell layout format (PRG `0x16500` region)
The shell's 8×8 tiles are stored as **4×4 quadrants**, row-major inside each quadrant, with `ff` between them:
```
Bills (shell $40) 0x16584: 80 89 8c 8d 88 8b 82 82 8a 82 82 82 a0 82 82 82  ff   <- top-left
                           8e 8f 80 80 82 a5 80 80 82 a7 b2 80 82 82 b8 80        <- top-right
                           cf  a2 82 a3 a6 a8 a1 a9 ac dd df ab ae 80 80 80 80  ff  <- bottom-left
                           ad 80 80 ...                                           <- bottom-right
Rams (shell $42) 0x165FB: 80 e0 e1 e4 b7 e2 e3 81 bd e8 e9 e6 bf ea 82 ec ff ...
```
Open: what `cf` means (probably a control byte), the exact end of each block, and the pointer/index table near `0x16500`.

## 32-team ROM (`TSB 2007-32-105.nes`)

From `savestates/bills32.mss` plus an enter-screen trace (2026-10-27):
- **Same formats, same places.** The Bills logo header/data, facemask and shell are read from the identical PRG offsets (`0x106C0`, `0x111A2`, `0x107AE`, `0x11492`, `0x16584`). The OAM result is identical; only the palette differs (2007 colors).
- **Shell lookup is hooked.** At `$A8CA` the original `LDA $BE49,X` is replaced by `JSR $F64A` (fixed bank, PRG `0x3F64A`):
  ```
  F64A  CPX #$1E / BCS +4 / LDA $BE49,X / RTS     ; teams 0-29: original table (contents re-ordered for the new divisions)
  F652  CPX #$22 / BCS (back) / LDA $F63C,X / RTS  ; teams 30-33: table at $F65A (PRG 0x3F65A) = 40 42 41 40
  ```
  The `$BE65` logo-ID lookup is **not** hooked. Teams were re-ordered (e.g. JAX = index 10, which still reads ID `$37`), and the hack
  **re-pointed the sprite-group pointer table** instead: IDs `$36 $37 $41 $4B` → `$9ED5 $9EE3 $9EF1 $9EFF` (PRG `0x2FED5…`, in the
  sprite-engine bank), whose headers select CHR bank `$81` (e.g. JAX `f6 81 ff ff ff fc 0f f8 36 3b 9f 75 ab 17`, data at `0x11F75`).
  Other IDs were shuffled to match the new team order. Verified by driving the menus headless to the Jaguars screen (`scripts/enter_seq.lua`).
- **CHR:** banks `$0F $19 $70 $71 $72 $73 $74 $7A $7B` changed vs. the original (helmet pages + shells); banks `$80–$82` are new content in the expanded half; the rest of `$83–$FF` is empty. So the hack has a lot of free CHR space.
- Sprite-group region `0x10000–0x11FFF`: 293 bytes differ (logo data edited/added).
- An editor must therefore detect the ROM type (28 vs 32 team) and read the extra shell table for 32-team ROMs.

### More 32-team hooks (found while building the viewer)
- Team indexes: 0–27 regular, **28/29 = AFC/NFC all-stars**, 30–33 = 49ers, Rams, Seahawks, Cardinals (TSBTool `TecmoToolFactory`).
- **Palette** hook at `$A98A` → `JMP $F7A9`: indexes 30–33 read 8 bytes from `$BFE0 + 8*(i-30)` (PRG `0x23FE0`), same byte order.
- **Logo header** hook at `$F8CB`: for indexes 30–33 the header pointer comes from `$9E91 + 2*i` (PRG `0x2FECD + 2*(i-30)`) = `$A7A0 $A784 $A688 $A714`, i.e. the original 49ers/Rams/Seahawks/Cardinals headers. `$BE65` is not used for these.

### Shell drawing (decoded)
- Script at PRG `0x4000 + (ptr - $A000)`; bytes 7–10 are the 4 quadrant **block IDs** (TL, TR, BL, BR).
- Block = PRG `0x16000 + 17*id`: 1 attribute byte (2 bits per 16×16 area) + 4×4 tile numbers (row-major).
- On screen the top area uses **BG CHR banks `$2A/$2B`** for tiles `$80–$FF` (the save state shows `$7A/$7B` because the MMC3 IRQ switches banks for the roster below), and the **vertical scroll is 255**, so the background appears one line below its grid.
- Facemask = sprite group `$4F` (hard-coded `LDA #$4F` at `$A902`); it lands 2 lines lower than the logo formula (second sprite slot; not explained further).
- The header `fc nn` is a running logo number (`02`–`1D`); groups `$38`/`$39` are other screens' sprites, not logos.

Verified by `test/large_helmet_test.dart`: decoded sprites, banks, shell tiles, palette RAM and a **pixel-exact 72×72 render** match Mesen for Bills, Rams, Bills (32) and the Jaguars/49ers sprites.

### Writing helmets back (helmet paste, 2026-10-28)
- Helmet CHR banks are used only by the logo groups and the facemask (`$4F`): orig `$70–$74`, 32-team also `$80–$82` (checked every sprite-group header's `f6`).
  Unreferenced tiles exist only in bank `$74` (31, mostly non-blank, so presumably used by another screen) and are left alone.
- Free space for logo data streams: `FF` runs at the end of PRG `$10000–$11FFF` (orig `0x11F5B`, 165 bytes; 32-team `0x11FBC`, 68 bytes).
  The first FF of a run can be the previous stream's terminator, so the paste skips it. Old streams are refilled with FF when moved.
- 32-team: CHR banks `$83+` are all zero, so they make room for new logos (one bank per relocated logo).
- Paste success over all pairs: orig→32 896/896, 32→32 1024/1024, orig→orig 502/784, 32→orig 570/896 (the original ROM's logo pages are full).
- Verified in Mesen: 4 pasted ROMs (in place, moved to free space, expansion bank; both ROM types). The game's frame matches the renderer pixel for pixel.

### Logo editing (2026-10-28)
- Limits: the game copies 32 helmet sprites to OAM (`CPX #$80` loops at `$A92B`/`$A970`); the facemask uses 12, so a logo can have **20 sprites**. The NES shows **8 sprites per scanline** (logo + facemask); a logo line with exactly 8 shows fully in Mesen.
- Helmet colors (`$BD69` table) are read only by the large-helmet routine (`$A99E`).
- Encoder (`lib/logo_edit.dart`): keeps the logo's own sprite positions (reusing unchanged tiles) and adds 8-px columns only for pixels outside them; falls back to a fresh column layout. Stream written in layout order (priority), `fc 3n` before column bytes, palette bits 1.
- Verified in Mesen: edited Bills logos on both ROMs (small edit, 69-pixel stroke, cleared logo + stroke, 8-sprite line) all match the renderer pixel for pixel.
- Original ROM: the logo pages are nearly full, so an edit that changes several tiles often can't fit (e.g. a stroke across Bills needs 1 more tile than bank `$70` has; a 3×3 dot inside one existing tile fits). The flip bit is the obvious next lever.

## How these were found (repeatable)
- `savestates/bills.mss`, `rams.mss`: Mesen2 states on the team data screen. `.mss` = header + zlib(frame buffer, 256×240×u16) + zlib(key/value state). See `scripts/mss.py` and `scripts/nesgfx.py`.
- `scripts/enter.lua`: headless Mesen2 (`TSBTool_Test/SimTests/EMU/Mesen.exe --testrunner`). It loads a state (must be done from an **exec** callback), presses B to leave and A to re-enter, and logs every PRG data read with the reading PC, plus all writes to the OAM shadow `$0200–$02FF`. Diffing two teams' read logs shows the per-team data.
- `scripts/dis6502.py`: a minimal 6502 disassembler (`dis6502.py ROM fileOffsetHex cpuAddrHex lenHex`).

## Small helmets, team-select grid (found 2026-10-28, original ROM; not yet decoded further)
Reached by booting and pressing Start/A (screen at ~frame 360). The 28 helmets are **2×2 background tiles (16×16 px)**.
- BG pattern `$0800` = MMC3 R1 = banks `$7A/$7B` (tiles `$80–$FF`, 128 tiles shared by all teams; `$8B` etc. are reused by many teams). Sprite banks on that screen: R2 `$0F`, R3 `$1D`, R4 `$7A`, R5 `$7B`.
- Only **4 BG palettes** on the screen (`11 25 31 30 | 11 10 26 0f | 11 30 01 28 | 11 0a 2c 30`), picked per helmet by the nametable attribute (2 bits per 2×2). So teams fall into 4 color groups and differ by tile art; no mid-frame palette changes (traced).
- **Table at PRG `0x23BB6`, 28 teams × 5 bytes**: `TL TR BL BR x` in TSB team order, tile numbers as drawn (`00` = empty). The 5th byte is unknown yet (`00 00 07 00 3b 01 09 …`), possibly a sprite-overlay group ID; PIT/CHI/ATL share the same four tiles `8f 89 8a 8b`, so something else tells them apart.
- Helmets sit at nametable columns 2/12/22, rows 6,8,10… (2 rows per team); name text in tiles `$41–$5A` ASCII.
- **5th byte = `overlayId << 2 | palette`.** Low 2 bits = BG palette of the helmet (original ROM: all four tiles). `id` (0 = none) selects **overlay sprites** drawn on top (team logos that don't fit in BG tiles, e.g. PIT/CHI/ATL share shell tiles `8f 89 8a 8b`): index table PRG `0x23C41` (start offset per id, end = next id's start; the game's loop (`$A7E8`) always draws one entry, and id 15 reads end=0 from the entry list), entries at PRG `0x23C51`, 4 bytes `[dy, tile, attr, dx]` (signed offsets, attr bit 5 = behind BG). Position = helmet pixel position; OAM y = y+dy, shown 1 line lower. Sprite tiles come from banks R2..R5 = `$0F $1D $7A $7B` (tile `>>6`), BG tiles `$00-$FF` from R0/R1 = `$18 $19 $7A $7B` (the 32-team hack uses tiles `$76/$77` of bank `$19` for the Panthers).
- **BG drawn 1 line lower** than its tile grid (scroll 255), exactly like the large helmet. Backdrop color `$11`.
- Palettes: BG at PRG `0x1A140` (4 x 4 bytes), sprites at `0x1A830`.
- **Original ROM grid:** cols 2/12/22, AFC rows 6.. (5/4/5 teams), NFC rows 18.. (5/5/4); all four tiles of a helmet share one attribute cell, palette = flags & 3 (verified against the game's nametable/attributes/OAM).
- **32-team ROM grid:** cols 1/9/17/25 (a helmet straddles two attribute cells), AFC rows 8..14, NFC rows 18..24, 4 teams per column. The hack replaces the attribute logic with a static 64-byte attribute table copied by `$A21D` from PRG `0x423A` (second variant at `0x427A` when `$C2 != $55`; the state used the first). Palettes per tile come from that table, so a helmet's left and right halves can differ (Dolphins, Redskins, Vikings have a palette-0 right half; just table data).
- Verified by `test/small_helmet_test.dart`: 32 helmets pixel-exact vs the Mesen frame (`research/savestates/small32.*`), 28 helmets vs the game's nametable, attributes and OAM (`small28.*`). Code: `lib/small_helmet.dart`, `lib/small_helmet_renderer.dart`; web grid under each pane (read-only).
### What is shared between small helmets (2026-10-28, computed from the tables; other screens not checked)
- **BG tiles are drawn from one pool** (`$7A/$7B`): original ROM 67 distinct tiles, 12 used by >1 team; 32-team ROM 76 distinct, 9 shared. Most shared: `8b` (12 / 15 teams, ear/facemask piece), `83` (9 / 7, ear piece in another color), `89`+`8a` (4 / 7), `9b` (4 / 5), `8f` (3 / 5), `97` (3 / 5), then `a2`, `82`, `99`, `9a`, `a1` (2-4 teams).
- **Identical 4-tile sets:** `8f 89 8a 8b` = Steelers, Bears, Falcons (original); Ravens, Steelers, Texans, Jaguars, Falcons (32-team). Their logos are overlay sprites only.
- **All-private BG tiles:** Browns, Vikings (original); Browns, Chargers, Vikings (32-team).
- **Shared overlay ids:** original 12 = 49ers + Saints; 32-team 12 = Packers/Saints/49ers and 14 = Chargers/Eagles/Seahawks. Shared overlay sprite tiles: `c8 c9 ca` (bank `$7B`; Redskins, Packers, Buccaneers, 49ers, Saints) and `fd fe 83` (Chargers, Eagles, Seahawks).
- **BG and sprite tiles `$80-$FF` are the same CHR bytes** (banks `$7A/$7B`): the Vikings overlay is the Steelers' shell tiles `8f 89 8a 8b`; the Browns overlay uses `8b a1 a2`; in the 32-team ROM BG `c8 c9` (Buccaneers) / `ca` (Chargers) are the same bytes as the `c8 c9 ca` overlay.
- **Palette groups:** 4 color groups per ROM (the 4 BG palettes), and on the 32-team ROM the palette follows the grid position (static table), not the team.
- **Blank test (shared tile zeroed in the real helmets):** `99 9a` (Patriots/Oilers) = lower-right shell next to the facemask; `a1` (Redskins/Giants) = lower-left shell edge; overlay `c8 c9 ca` = solid body-fill behind the BG (Redskins, Packers, Buccaneers, 49ers, Saints); overlay `fd fe 83` = black facemask (Chargers, Eagles, Seahawks). In every case the logo stayed, so **logo pixels live in tiles that only that team uses**; the shared tiles are shell shape, body fill, ear and facemask. Checked only for the tiles above (original ROM + Chargers/Eagles/Seahawks on the 32-team ROM); `a2`, `97` and the other shared tiles were judged from their pixel shape only.
- **Editing consequence:** logo edits are safe per team; changing a shared tile changes every team that uses it, so a different shell/facemask for one team needs its own tile copy (free `$7A/$7B` space is tight on the original ROM). Other screens: see the next section (only the grid screens and the playoff bracket use `$7A/$7B`).
### Who else uses CHR banks `$7A/$7B` (traced 2026-10-28, headless Mesen)
Method: logged every MMC3 CHR register write plus, every 60 frames, the nametable tiles `>=$80` and the on-screen sprites `>=$80`, while the game ran a fixed Start/A pattern (title, menus, matchup, kickoff, gameplay) and 5 (original) / 3 (32-team) random-button menu walks of 9,000-12,000 frames. A screen only counts if a register held `$7A/$7B` **and** it really drew a tile `>=$80` from it. Code scan: only one `LDA #$7A / LDA #$7B` pair is real code (`$A6BC`, the helmet grid draw, PRG `0x226AC`); the other byte hits are data.
- **Helmet-grid screens (R1 = `$7A` = BG tiles `$80-$FF`, R4/R5 = `$7A/$7B` for the overlay sprites):** the team-select / team-data grid in all its modes. Besides the helmet tiles, these screens use 11 non-helmet tiles from the same pool: `b8` and `d4-dd` (letter tiles for the `COM` / `SKP` markers). They are not shared with any helmet tile.
- **Playoff bracket screens (R1 = `$7A`, no sprites):** BG tiles `de df ea eb ec f0 f1 f2 f3` (bracket lines/corners; `e0-e9` and a few more look like the same family). **None overlap the helmet tiles.**
- **Not users:** the large-helmet team data screen uses `$2A/$2B` (BG) and `$70/$73` (sprites); the matchup screen (the big helmets) uses `$70/$73`; schedule pages, game menus and gameplay (`R1=$22`, `R2-R5 = $0F/$09/$07/$08`) do not load `$7A/$7B` as BG; the play book screens set `R5=$7B` but draw no sprite from it.
- **Pool accounting (128 tiles `$80-$FF` in `$7A/$7B`):** original ROM: 85 helmet tiles, 11 grid-text tiles, 9 bracket tiles = 105 identified, 23 non-blank tiles unidentified (`91 92 e0-e9 ed-ef f4-fb`; mostly more bracket-like pieces), 1 blank (`fc`, used as a blank sprite). 32-team ROM: 92 helmet, 10 grid-text, 9 bracket = 111 identified, 17 unidentified, none blank.
- **Consequence for editing:** editing helmet tiles in place can only change the team-select/team-data grid (and the same grid on its other screens); no other screen I reached uses those tiles. Free space for per-team tile copies is limited to the unidentified tiles (not proven free) because every blank tile is gone; a copy needs the 32-team ROM's unused CHR banks (`$83+`) and a BG/sprite bank change, or reuse of tiles inside the pool.
- **Not covered:** screens the random walks never reached (Pro Bowl, standings/rankings/leaders pages, post-game and season-end screens, Super Bowl ceremony). If one of those uses `$7A/$7B` it would have to be a screen that never appeared in ~100,000 frames of play and menus, but that is not proven.
### Editing small helmets in place (2026-10-28)
- **What an edit writes:** pixels of the CHR tiles the helmet is drawn from (BG slots `TL TR BL BR` and the overlay sprites), keyed by (bank, tile) so every reference to a tile sees the change; plus the helmet's palette group. Empty slots (tile `$00`) and areas no overlay sprite covers cannot be edited. Code: `lib/small_helmet_edit.dart` (+ `small_helmet_image.dart` for PNG, `web/small_helmet_ui.dart`).
- **Shared tiles:** `alsoAffects` lists every other team that draws a changed tile (BG slot or overlay sprite; BG and sprite tiles `$80-$FF` are the same CHR). The editor applies it anyway, with a warning.
- **Palette group writes:** original ROM: low 2 bits of the flags byte (`0x23BB6 + 5*team + 4`), overlay id untouched. 32-team ROM: the helmet's four 2-bit slots in **both** attribute tables (`0x423A` team-data layout, AFC helmets at rows 8-14; `0x427A` preseason select-team layout, AFC rows 6-12; NFC rows 18-24 in both). The two tables give identical palettes per helmet, verified. Palette *colors* (the four BG groups at `0x1A140`, sprite palettes at `0x1A830`) are shared by every helmet and are not edited.
- **Paste** = apply the source's 16x16 BG picture and overlay canvas (and palettes) to the target's tiles; pixels with no target tile are dropped and counted; a source with no overlay leaves the target's overlay alone. **PNG** = 24x36: BG layer (x 4-19, y 0-15), gap, overlay canvas (24x18); import snaps to the nearest of 3 palette colors but keeps the current index when the color already matches (palettes can repeat a color).
- **Verified in Mesen:** edited ROMs (shared-tile BG edit, overlay edit, private-tile edit, two palette-group changes) on both ROM types, grid screen at frame 360: changed-pixel counts in the helmet areas match the renderer exactly (185 / 202), and the colors match. In the browser (headless Edge): menu, paint, undo, Apply, main Undo, copy/paste, palette, PNG import (6x scaled, one pixel) all behave.
- Sprites on the screen (cursor/flags) are not the helmets themselves.
- The matchup screen ("Buffalo Bills / Indianapolis Colts") draws larger helmets; not yet examined.
- Tools for this: `boot.lua`-style headless Mesen run (Start/A every 30 frames, screenshot every 120), then dump OAM/nametable/palette RAM with `emu.read(..., emu.memType.nesSpriteRam/nesNametableRam/nesPaletteRam)`.

## Not yet investigated
- Shell screen-script format (`0x5A98`) and the `cf` byte in the shell tile data.
- Small helmets: the second attribute-table variant, the unidentified `$7A/$7B` tiles, screens the traces never reached. End zones, midfield, title screen.
- Whether the 32-team palette table has entries for indexes 28+.
