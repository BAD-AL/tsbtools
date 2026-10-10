# How Tecmo Super Bowl II and III Simulate a Game (SNES)

The technical companion to `TSB_Simulation.md` (TSB1) and to the user guide (`TSBTool_Sim_Guide.txt`,
About -> TSB Sim Data Guide). It records what TSB2's and TSB3's sim engine reads, what each sim value
does, the game bugs found, the battery-save layouts TSBTool reads, and the simulated seasons that
checked it all.

Everything here was read from the game code (65816 disassembly of `TSB2_U.smc` and `TSB3.smc`) and/or
checked with whole seasons simulated in Mesen2 (October 2026). Anything not confirmed is marked
**unconfirmed**. The working notes, test ROMs and saves were kept in `TSBTool_Test\SimTests\TSB2_study` and
`TSB3_study` (not in git); this document is their summary.

Both ROMs are LoROM: file offset = (bank & 0x7F) × 0x8000 + (address − 0x8000).

---

## The short version

* TSB2 and TSB3 run **TSB1's sim engine** (same random-number and maths helpers, same yardage tiers, same
  defense roll), rewritten for 18 defenders, carry and target *shares*, and tackles. TSB3 is TSB2's code
  with **its own lookup tables** and **its own team defense**.
* There is no team offense value: a simulated offense is its players' sim values.
* **Shares** (carry, target, sack, INT) are chances out of 256, walked in order; a team's should add up
  to 255. **255 (FF) marks an injured player** and is skipped.
* **TSB2's team byte** is rush defense / pass defense, as TSB1. **TSB3 doesn't use a team byte**: it works
  out team defense from its starting defenders plus a hidden per-team rush and pass bonus, which is TSB3's
  team SimData in TSBTool (`0xRRPP`).
* **Player condition does nothing** in the TSB2/TSB3 sim (it's zeroed before every player).
* Game bugs: **G1** loses about 1 in 20 interceptions in TSB2 and 1 in 8 in TSB3 (TSBTool's `FIX_SIM_BUGS`
  fixes it); **G3** loses a run's credit when the carrier pick lands on the QB who isn't playing (avoided by
  keeping QB2's carry share 0); **G2** scramble yards use the wrong QB on one side (minor, not fixed).

---

## Where the sim data is

| | TSB2 | TSB3 |
|---|---|---|
| Season pointer table | `$84:E85F`: `$BD:E000`, `$BE:E000`, `$BF:E000` | `$84:E9A1`: `$BD:E000` for all three entries |
| Season played | SRAM `$7000D6` & 3 = 0/1/2 = 1992/1993/1994 (TSBTool SEASON 1/2/3) | one season |
| Team pointers | 28 × 16-bit, **into the table's own bank**: blocks at 0x1E0000 / 0x1E8000 / 0x1F0000 + pointer | 32 × 16-bit at 0x1EE000 (30 teams, free agents, all-time team), blocks at 0x1E0000 + pointer |
| Lookup tables | `$BF:FA00` (file 0x1FFA00) | `$BD:EE00` (file 0x1EEE00) |
| Kick / punt returner bytes | 0xE51DA / 0xE5216 / 0xE5252 + team × 2 (KR slot, PR slot) | 0xE50AA + team × 2 |

### The team block (101 bytes, +1 for TSB2's team byte)

| Offset | Who | Bytes |
|---|---|---|
| +0, +3 | QB1, QB2 | carry share, rush, pass (hi) / scramble (lo) |
| +6 + 4i | RB1–4, WR1–4, TE1–2 | carry share, rush, target share, YAC (hi) / catch (lo) |
| +46 + 3i | RE NT LE RE2 NT2 LE2 ROLB RILB LILB LOLB LB5 RCB LCB DB1 DB2 FS SS DB3 | sack share, INT share, tackle weight |
| +100 | K / P | kicker (hi), punter (lo) |
| +101 | team (TSB2 only) | rush defense (hi), pass defense (lo) |

The defenders are in **roster order** (ROLB, RILB, LILB, LOLB), the same order as the names, ratings and the
season stats. TSBTool used to read LOLB/LILB/RILB/ROLB here, swapping two pairs (fixed October 2026;
checked by tackles in a simmed season and by the tackle value's relation to QU in both games).

---

## Loading

TSB2 `$84:D3A8` (TSB3 `$84:D3E5`) copies both teams' sim values into RAM, turning digits into working values
through the lookup tables. Each digit gets `$0DB7` added (capped at 15) — but `$84:D75B` (TSB3 `$84:D878`)
zeroes `$0DB7` before every player, so **condition never affects the sim**.

**TSB3 player IDs.** TSB3's rosters are lists of player IDs in RAM (`$7E:9000` / `$7E:9300`, from the save).
`$84:D7FB` divides an ID by 37: quotient = the player's *original* team, remainder = his slot, and that's
where his sim values are read from — a traded player or signed free agent brings his own values.
`$84:E9AD`: a team ID of 0x20 or more reads the sim values from the battery save (`$70:7630`) instead of
the ROM — **unconfirmed** what those are (created players?). Regular teams (0–29), free agents (30) and the
all-time team (31) use the ROM.

---

## The values

### Lookup tables, digit 0 → 15

| Table | TSB2 (`$BF:FA00`) | TSB3 (`$BD:EE00`) |
|---|---|---|
| Rush | 170, 175 … 240, 255 | 130, 135 … 205 |
| Yards after catch | 155 … 225, 255 | 101 … 165, then **10** for 15 |
| Pass / catch | 95, 98 … 137, 150 | 100 … 162 |
| Rush defense | 95, 105, 112, 122, 164 … 194, 227 (= TSB1) | same |
| Pass defense | 116, 118, 120, 144 … 204, 221 (= TSB1) | different values |
| Kick | 210 … 255 (step 3) | same |
| Punt | 30 … 45 | same |
| Scramble chance /256 | **51, 25, 13, 8**, 0, 20, 25 … 84 | **8, 13, 25, 51**, 0, 20, 25 … 84 |
| Tackle | 8 … 38 (step 2) | same |

So the scramble digit goes the **other way round** in TSB3 (0 = least, 3 = most; 4 = never in both).

### What each value does

* **Carry share** (QB, RB/WR/TE byte 0): the run-play ball carrier pick (below).
* **Rush** (byte 1): run yardage; for RB/WR/TE also kick and punt return yardage when he's the returner.
* **Pass** (QB byte 2, hi): the completion roll. **Scramble** (lo): chance of a scramble on a pass play.
* **Target share** (RB/WR/TE byte 2): the pass target pick.
* **YAC** (RB/WR/TE byte 3, hi): yards after a completion. **Catch** (lo): added to the QB's pass roll.
* **Sack / INT share** (defenders bytes 0, 1): who gets the credit (below). They don't change how many.
* **Tackle weight** (defenders byte 2): who makes the tackle; season tackles ≈ 16 × value in a stock season.
* **Kick / punt** (+100): field goals (`kick × (70..99)% − 3 × distance`) and punt distance (`punt + rand % 20`).

### Plays (as TSB1)

* **Run** (TSB2 `$84:DBD5`): carrier by carry shares; value = rush × (80..99)% minus the defense roll of the
  defending team's rush defense; tiers 0x34/0x67/0x9A/0xCD → yards.
* **Pass** (`$84:DD42`): scramble check; target by target shares; value = pass × (80..99)% + catch ×
  (70..99)% minus the defense roll (pass defense) → tier → outcome table `$84:E851`, out of 256
  [complete, incomplete, sack, INT]: tier 0 26/128/64/38, tier 1 77/102/51/26, tier 2 128/77/38/13,
  tier 3 179/38/31/8, tier 4 230/10/13/3. On a completion, YAC × (80..99)% minus the same roll → yards.
* **Defense roll** (`$84:E4BD`, = TSB1): value × (rand % [50, 65, 80, 50] + [0, 0, 20, 100]) / 100.
* **Kick / punt return**: the returner's rush × (90..99 / 70..99)% minus the kicking team's rush defense roll.

---

## Shares and credit

R = random 0–255; walk the pool subtracting each player's share; the player R falls under gets it.
**Absent (injured) players are marked FF and skipped**, so a share of 255 makes a player look absent.

| Pick | Pool, in walk order | A roll past everyone goes to |
|---|---|---|
| Ball carrier (`$84:DBE1`) | QB1, QB2, RB1 … TE2 (12) | RB1 (the next present player after slot 1) |
| Pass target | RB1 … TE2 | a random RB3 … TE2 |
| Sack | the 18 defenders, roster order | a random RE … RILB |
| Interception (`$84:E0C4`) | the 18 defenders, **reverse** order (DB3 first) | a random NT2 … RCB (roster slots 4–11) |

In the stock ROM each team's carry and target shares add up to 255 (seasons 1992/1993; 250–268 in 1994);
sack shares add up to 155–331 and INT shares 108–378 (not scaled — late defenders get little when > 255).

---

## Team defense

**TSB2**: the team byte (+101): high digit rush defense, low digit pass defense, through the rush/pass
defense tables, as TSB1. Checked in a simmed 1992 season: a team set to 0x00 allowed the most pass yards
(3678 vs a 2731 average) and the second most points; rush yards allowed rose less (1853 vs 1739).

**TSB3** doesn't read a team byte. Its loader (`$84:D729`) works team defense out per game:

* `$84:E9CF` per team: front 0 = 4-3, 1 = 3-4 (Bills, Patriots, Steelers, Packers, Panthers).
* Rush defense: sum, over the front seven (4-3: RE NT LE RE2 ROLB RILB LILB; 3-4: RE NT LE ROLB RILB LILB
  LOLB — lists at `$84:E9F7` / `$E9FF`), of a per-player value from `$83:801E` (the game's read-a-rating
  routine, `$83:850E`; Y = $87 = the **low nibble of byte 3** of the player's ratings record — TSB3's 'Face'
  byte in TSBTool, whose last digit TSBTool calls the **free agent value** — plus a condition adjustment, − 1,
  clamped 0..15), plus the per-team bonus `$84:EA43` (0–15), then
  thresholds `$84:EA23` (30 … 120, 255) → index into the rush defense table.
* Pass defense: the same seven plus RCB LCB FS SS (`$84:EA0B` / `$EA17`), bonus `$84:EA67` (0–20),
  thresholds `$84:EA33` (60 … 140, 255) → the pass defense table.

Both bonus adds are capped at 255 (`adc` / `bcc` / `lda #$FF`), so a 255 bonus gives the top level whatever
the defenders are. The defenders' own sum (`$84:D78B`) is 8-bit with **no** overflow check; stock TSB3 stays
under it, but very high per-player values could wrap a team to a weak defense (not tested).

Rush thresholds `$84:EA23`: 30 33 35 38 40 42 45 48 50 53 55 60 80 100 120 255; pass `$84:EA33`: 60 63 66
69 72 75 78 81 84 87 90 95 100 120 140 255 → the TSB3 pass defense table `$BD:EE40` (128, 132, 138, 155,
170 … 225, 240); rush `$BD:EE30` (= TSB1's). In the usual range the levels are only 2–3 apart, so a bonus of
20 is about 6 levels.

Stock bonuses (rush / pass, TSBTool's TSB3 team order, tables at file 0x26A43 / 0x26A67 + team): Steelers 10 /
20, Bears 5 / 20, Browns 10 / 15, Cowboys 5 / 15, Oilers 0 / 15, Raiders 10 / 10, Cardinals 15 / 10,
Chargers 15 / 0, Vikings 15 / 0, Packers 10 / 0, Saints 0 / 10, 49ers 5 / 0, the rest 0. (The 3-4 table next to
them flags Bills, Patriots, Steelers, Packers and Panthers, which confirms the team order.)

Checked:
* A TSB3 team whose (unused) team byte was set to 0x00 allowed fewer points than average.
* `TSB3_DefBonus` (vs the stock season): Jets 255/255 — points allowed 341 → 239, rush yds allowed 1736 →
  1133, sacks 38 → 56, INTs 17 → 26; Jaguars rush 255 — rush yds allowed 2327 → 1394; Colts pass 255 — sacks
  48 → 61 (league high); Steelers 0/0 — sacks 51 → 34. Pass yards allowed didn't follow the pass bonus
  clearly in one season. 255 ran normally.

---

## Game bugs

### G1: interceptions credited to nobody — fixed by `FIX_SIM_BUGS`

The INT credit loop walks the defenders downward (DEX). When it lands on an absent defender, the code
steps the wrong way (INX) and jumps into the **sack** credit loop, so no defender gets the INT.

| | Location | Stock | Fixed |
|---|---|---|---|
| TSB2 | `$84:E0F9`, file 0x260F9 | `E8 88 4C FA DF` (inx / dey / jmp $DFFA) | `CA EA 4C D1 E0` (dex / nop / jmp $E0D1) |
| TSB3 | `$84:E23A`, file 0x2623A | `E8 88 4C 3B E1` (… jmp $E13B) | `CA EA 4C 12 E2` (… jmp $E212) |

`$E0D1` / `$E212` is the INT loop's own `dey / bne` step, so when the absent defender was the last one the
loop's random pick runs. Stock seasons lost 20–43 of ~450 INTs in TSB2 and 63–79 of ~550 in TSB3; a TSB2
season with the fix credited all 423 INTs thrown, the rest of the season normal. TSBTool:
`TSB2Tool.HasSimBugs/FixSimBugs` (TSB3Tool supplies its location), the `FIX_SIM_BUGS` text line, and a
prompt when saving. The sack loop's absent-defender path (`$84:E028`) has the same missing loop-count check
but only matters if DB3 is absent with R in his share — left alone.

### G3: a run credited to nobody — avoided

The carrier pick walks both QBs. When it lands on the QB who isn't in the game (`$0CA3` holds the one who
is), `$84:DC53` credits a running-back run to that QB slot, which has no running stats: the yards count as
rush yards allowed but no player gets the carry. Tecmo gave QB2s a carry share of 0. A model ROM with QB2
shares of about 10 lost 4% of its carries' credit (1284 and 1655 yards in two seasons). TSBTool's Auto
Update Sim writes QB2 = 0.

### G2: scramble yards use the wrong QB — not fixed

Scramble yardage reads `$0CDB,X` with X = the team (0/1). For team 1 that's team 0's QB2 rush digit instead
of team 1's QB1. Needs code moved to free space; minor.

### Tight end kick returners aren't credited

With a TE as kick returner, no player on the team was credited with kick returns (TSB2 and TSB3); the stock
returner wasn't either, so the game did read the returner byte. Tecmo never used a TE as kick returner (one
TE punt returner, TSB2 1993). Not investigated further.

---

## Battery saves (.srm, 32 KB)

File offset = SRAM address − `$700000`; 16-bit values little-endian; yards signed.

**TSB2** (`Core\Tsb2SeasonStats.cs`): season played at 0xD6 (& 3). Team records at 0x3A2 + k × 163:
W, L, T, PF, PA, pass yds allowed, rush yds allowed. Player stats at 0x152A + k × 485 (team k = roster order):
QB1/QB2 13 bytes (TD, att, INT, comp, yds, rush TD, rush att, rush yds); RB/WR/TE 22 bytes (rec TD, rec,
rec yds, rush TD, rush att, rush yds, rec long, rush long, then a 10-byte return block: KR TD, KR, KR yds,
PR TD, PR, PR yds, KR long, PR long); 18 defenders 9 bytes (sacks, 0, INT, INT TD, INT yds, tackles, forced
fumbles), the 7 DBs plus a return block; K (XP made, XP att, FG made, FG att); P (punts, yds).

**TSB3** (`Core\Tsb3SeasonStats.cs`): team chunks at 0x457 + k × 101: 37 player IDs (ID & 0x0FFF = original
team × 37 + slot), then at +76 W, L, T, PF, PA, pass/rush yds allowed. Player stats at 0x11C1 + t × 485 by
**original** team t (30 = free agents), the TSB2 layout; a player's stats are found through the roster ID
of the slot he plays now. Season marker: **unknown**.

Both were checked field by field against the games' stat screens and by league balances (completions =
receptions, W = L, PF = PA, every team's points = TDs, XPs, FGs and safeties). The game's own rush yards
allowed can be a few yards off the players' total (bug G3), so TSBTool doesn't require those to balance.

---

## Simulated seasons

All in Mesen2, a new season simmed game by game; compared with `TsbSimModel compare` / `nodice`.

| ROM | What it checked | Result |
|---|---|---|
| TSB2 stock (twice) | baseline, luck | completion 59.5 / 60.6 %, 6.4 yds/carry, RB1 50 / 48 % of carries; 20 / 43 INTs not credited (G1) |
| TSB2 G1 fix | the INT fix | 423 of 423 INTs credited; the rest of the season normal |
| TSB2 auto-updated (3 rounds) | the Auto Update Sim model | round 1: shares too concentrated, tackles too even → calibration; round 2: G3 found; final: league shape inside the stock range except LB INT share (14 % vs 19–21 %) |
| TSB2 2026 rosters | converted TSB1 roster + auto-update | sensible season (J. Taylor 1919 rush yds, A. J. Brown / G. Wilson top receivers) |
| TSB2 1992 checks | team byte, season 1 block, returners | team byte works; the 1992 block is used; PR → RB4 credited, KR → TE2 not credited |
| TSB3 Bears offense minimum | TSBTool's TSB3 writes | Bears last in points (129 vs 274), yds/carry, completion %, QB carries, punt average |
| TSB3 checks (FF shares) | shares, free agents | every FF-share player skipped (FF = absent) — redone with FE |
| TSB3 checks 2 (FE) | shares, free agents | RE 37 of 37 sacks, FS 14 of 14 INTs; free agent Hall (on the Dolphins that season) 11 of 12 INTs |
| TSB3 stock + auto-updated | the TSB3 model | inside or near the stock range; with no dice the model's bytes match stock (LB INT share a bit low) |
| TSB3 defense bonus | the per-team bonus tables | rush bonus strong (−600 / −930 rush yds allowed); pass bonus shows in sacks; 255 safe |
| TSB3 hidden rating (`TSB3_HiddenRating`) | free agent values of 3 teams' starters set to 0 / F | Steelers at 0: best defense → worst (424 points allowed); Jaguars' front seven at F: 2nd best |
| TSB3 free agent value model (`TSB3_AutoSim_FAV`) | Auto Update's fitted free agent values, same sim bytes as `TSB3_AutoSim` | normal league (points allowed 289 vs 280 / 273); levels track simmed defense (r −0.65 rush yds, −0.48 pass yds; Tecmo's −0.55 / −0.41) |
| TSB2 team SimData model (`TSB2_TeamSim_1994`) | Auto Update's team values, same players as SimCal3 | league stock-like; model values track the simmed defenses (r −0.62 rush yds, −0.54 pass yds, −0.75 points allowed; Tecmo's bytes −0.54 / −0.24 / −0.52) |

---

## TSBTool notes

* Sim values per season: `TSB2Tool.GetSimLocation` (bank of the season's table); TSB3 reads season 1's.
* `TsbSimAutoUpdater` (TSB2/TSB3 Auto Update Sim): models fitted to Tecmo's values, team at a time, shares to
  255 (none over 254), QB2 carry 0; TSB3's free agents / all-time team keep their shares. Fitted, exported
  and checked with `TsbSimModel` (`TsbSimModel\README.md`).
* `TsbSimModel`'s INT chances use the engine's fallback, roster slots NT2 .. RCB (fixed 2026-10-10: it had used
  the reversed walk's 4th-11th slots, ROLB .. DB1). Only the leftover of pools under 255 -- Tecmo's, in fitting --
  was affected; the refit moved a little INT credit from DBs to the DL (TSB2 DL 0.7 -> 1.8 %); `ShareChanceTests`.
* TSB2 team SimData in Auto Update (`AutoUpdateSimData`, the player form's button): from the 11 starting
  defenders, Tecmonster's rating mix per player, group weights fitted to Tecmo's 84 team bytes, Tecmo's league
  spread. Ratings explain Tecmo's team byte only partly (held out ~2.5 digits off; r 0.53 rush, 0.31 pass) —
  TSB2 also has a per-player nibble like TSB3's free agent value, and it predicts no better.
  The converter (`AutoUpdatePlayerSimData`) keeps a roster's own team values. The team forms' old Auto Update
  buttons are hidden; TSB1/Genesis use Tecmonster's team formula from the player form.
* TSB3 team SimData is `0xRRPP`, the two defense bonuses (`TSB3Tool.GetTeamSimData` / `SetTeamSimData`;
  none for freeAgents / allTime). A one-byte SimData in TSB3 text is skipped with one warning, as is a TSB3
  one in TSB2 text. Converting to TSB3 (`TSB3Converter.Tsb3SimData`): bonus = 2 rush / 3 pass per TSB1/TSB2
  digit above 8; converting from TSB3 drops it.
* Season stats: `Tsb2SeasonStats`, `Tsb3SeasonStats` (CLI `-stats:`, the GUI's View Season Stats).

## Open questions

* TSB3's free agent value (Face's last digit), which its team defense sums, looks like an overall rating Tecmo
  set by hand (stars far above their ratings). The Superstar Editor makes it for created players as the average
  of a position's ratings, rounded down (`$A1:8A79`, lists at `$A1:9ADD`; defense: RS RP MS HP BB AG PI QU) —
  far below Tecmo's values. TSB2 has a similar nibble, which doesn't predict Tecmo's TSB2 team byte better than
  the ratings do.
* TSB3's `$70:7630` sim values: the Superstar Editor writes created players' sim data there (bank `$A1`, from
  `$A1:8B0B`) -- Tecmo's own sim formulas, not yet read. And where a TSB3 save records its season.
* Whether TSB3's free agency uses the free agent value too (the team defense does).
* G2 (scramble yards) and the tight end kick returner, if anyone wants them fixed.
