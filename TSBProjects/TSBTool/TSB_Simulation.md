# How Tecmo Super Bowl Simulates a Game (NES, SNES, Genesis)

This document describes what the sim engine in Tecmo Super Bowl (TSB1) actually reads when the CPU
simulates a game, what each sim value TSBTool edits really does, and the bugs found in the Genesis
version (which TSBTool can fix with `GENESIS_FIX_SIM_BUGS`).

TSB2 and TSB3 are covered in `TSB2_TSB3_Simulation.md`; the plain-language guide for users is
`TSBTool_Sim_Guide.txt` (in the program: About -> TSB Sim Data Guide).

Everything marked **confirmed** was read directly from the game code and/or checked with simulated
seasons in an emulator. The NES code was disassembled from the stock `TSPRBOWL.nes`, starting from the
addresses in the CXROM hack source (`CXROM_CODE\...\tsb2007\TSB 2007-32.asm`); the SNES (65816) and
Genesis (68000) code was disassembled from the stock ROMs. Anything not fully traced is marked
**unconfirmed**.

The CXROM / 32-team ROMs run the NES engine; the hack only redirects the data pointers for the
expansion teams. SNES TSB1 is a 1:1 port of the NES engine. Genesis TSB1 is a port too (same
0x30-byte sim block, same lookup tables, same logic), but it has bugs of its own; see
[Genesis TSB1](#genesis-tsb1).

The emulator tests live in `TSBTool_Test\SimTests\` (ROMs, recording sheets with results, and
`README.txt`); see [Emulator test results](#emulator-test-results).

---

## The short version

* **There is no team offense rating.** A simulated offense is made entirely of its players' sim
  values: the QB, the runners, the receivers and the kicker.
* **The team `SimData` byte is all defense:** high digit = **rush defense**, low digit = **pass
  defense**. The third digit is the run/pass preference.
* **Skill players' 3rd and 4th sim values are not punt/kick return.** They are **yards per catch**
  and **how often the player is targeted**. Kick and punt returners use their **Sim rush**.
* **Defenders' sim values (pass rush, coverage) don't make a defense better.** They only decide
  *which* defender gets credit for a sack or an interception. Each value is that defender's share
  out of 256; a team's 11 values should add up to 255, and any shortfall goes to the SS.
* **Player condition matters.** A player's in-season condition (bad/average/good/excellent) shifts
  his offensive sim values by -2 to +4 before every simulated game.
* **Stock Genesis TSB1 has three sim/stat bugs:** preference 1 plays as heavy passing, the team
  defense digits barely count, and season receiving TDs are lost. `GENESIS_FIX_SIM_BUGS` fixes all of
  them and rebalances the sim so a stock season keeps stock scoring and passing.

---

## Per-team sim data block (0x30 bytes)

NES location: `0x18163 + teamIndex * 0x30` (Bills first). The team byte is the last byte of the
block (`0x18192 + teamIndex * 0x30`).

| Offset | Player(s) | Contents (TSBTool name) |
|---|---|---|
| 0x00–0x03 | QB1, QB2 | 2 bytes each: `[Sim rush, Sim pass]` in byte 0, `Sim pocket` in byte 1 |
| 0x04–0x17 | RB1–4, WR1–4, TE1–2 | 2 bytes each: `[Sim rush, Sim catch]` in byte 0, `[Sim yards per catch, Sim targets]` in byte 1 |
| 0x18–0x22 | 11 defenders (RE … SS) | 1 byte each: `Sim pass rush` |
| 0x23–0x2D | 11 defenders | 1 byte each: `Sim coverage` |
| 0x2E | K / P | high digit = kicker, low digit = punter |
| 0x2F | Team | high digit = rush defense, low digit = pass defense |

The run/pass preference is a separate table, 1 byte per team, at NES `0x27526`.

On SNES and Genesis the sim blocks are **not in roster order**: the pointer table swaps teams 14 ↔ 18
(Cowboys / Redskins) and 24 ↔ 27 (Falcons / 49ers). TSBTool's Genesis sim accessors had this wrong
(they edited the wrong team's sim data for those four teams); fixed.

---

## Step 1: Setting up a simulated game (confirmed)

NES routine `$91CA` (PRG bank 0x0B). For both teams it walks the sim block, using the team's
**current starters** (the depth chart in SRAM, `$6611…`), and converts each digit into a working
value through a lookup table (PRG bank 0x0C; the SNES and Genesis tables are byte-for-byte the same):

| Sim value | Lookup table | Converted range | Condition adjusted? |
|---|---|---|---|
| QB / RB / WR / TE Sim rush | `$A0A3` | 180 – 255 | yes |
| QB Sim pass | `$A0B3` | 95 – 150 | yes |
| Receiver Sim catch | `$A0B3` | 95 – 150 | yes |
| Receiver Sim yards per catch | `$A0A3` | 180 – 255 | yes |
| Receiver Sim targets | `$A107` | 0 – 128 (a weight) | no |
| QB Sim pocket | `$A103` | see "Scrambles" below | no |
| Kicker | `$A0E3` | 210 – 255 | yes |
| Punter | `$A0F3` | 30 – 45 | yes |
| Team rush defense (high digit) | `$A0C3` | 95 – 227 | no |
| Team pass defense (low digit) | `$A0D3` | 116 – 221 | no |
| Defender pass rush / coverage | (copied raw) | 0 – 255 (share out of 256; see Step 5) | no |

The defense tables, digit 0 → 15:

* rush: `95, 105, 112, 122, 164, 166, 168, 170, 172, 174, 176, 188, 190, 192, 194, 227`
* pass: `116, 118, 120, 144, 148, 162, 166, 178, 176, 178, 180, 182, 184, 202, 204, 221`

Only the 5 starting skill players (RB1, RB2, WR1, WR2, TE1 in the formation's slots) take part in a
simulated game.

### Player condition (confirmed)

Before the table lookup, each condition-adjusted digit gets a modifier from table `$A084`, chosen
by the player's 2-bit condition plus one random bit. The result is clamped to 0–15:

| Condition | Modifier |
|---|---|
| Bad | -2 or -1 |
| Average | 0 |
| Good | +1 or +2 |
| Excellent | +3 or +4 |

---

## Step 2: Choosing the play (confirmed)

The sim uses the same play-calling routine as the CPU in a normal game (NES `$A6F3`, bank 0x0C).
Down, distance, time, score and quarter can override it. Otherwise the run/pass preference picks one
of four routines (`$AA4E` jump table), each splitting a random byte. The overrides pull every team
toward running, so the passes actually called in a season are lower than the split:

| Preference | TSBTool label | Code split (run / pass) | Measured pass % (NES_06) |
|---|---|---|---|
| 0 | Little more rushing | 50 / 50 | 42 |
| 1 | Heavy rushing | 60 / 40 | 38 |
| 2 | Little more passing | 40 / 60 | 55 |
| 3 | Heavy passing | 30 / 70 | 62 |

SNES measured about the same. Genesis (stock Genesis rosters, with the fix) measured 44 / 38 / 49 / 55.
So even "heavy passing" stays near 55–60% passes; a team can't be made to throw 70% of the time with
sim data alone.

The result is a play type (`$03B3`): 0–3 = run, 4–7 = pass, 8 and up = kicks. The dispatcher is at
`$9108`.

---

## Step 3: Run plays (confirmed)

NES routine `$9446`.

1. The ball carrier is chosen. If it's the QB, his Sim rush is used; otherwise the skill player's
   Sim rush.
2. The carrier's value is rolled against random numbers.
3. The routine switches to the **defending** team and rolls that team's **rush defense** (the high
   digit of its team byte) — `$94C6`: `LDA $6F / EOR #$01 / TAX / LDA $040A,X`; see
   [The defense roll](#the-defense-roll-confirmed).
4. The offense value minus the defense roll is bucketed into yardage tiers by four cutoffs:
   `0x34, 0x67, 0x9A, 0xCD` on NES and SNES. (Stock Genesis uses `0x3E, 0x7B, 0xB8, 0xD7`.)

The yardage within each tier (subroutines `$9FDE`, `$A00F`) is **unconfirmed**. Emulator testing
shows the runner's Sim rush drives yards per carry almost linearly (NES_01: 5.0 / 6.7 / 8.9 / 10.8
yards per carry at Sim rush 0 / 5 / 10 / 15).

### The defense roll (confirmed)

Run and pass plays both call the same routine (NES `$98CD`, Genesis `$DC198`):

```
roll = defense value * (random % M + A) / 100
```

`M` and `A` depend on a "matchup type" chosen by comparing the two teams' play calls (Genesis
`$FF3D33`; this reading of those bytes is unconfirmed):

| Matchup type | When | M | A | Roll, as a share of the defense value |
|---|---|---|---|---|
| 0 | one side called a run, the other a pass | 50 | 0 | 0 – 49% |
| 1 / 2 | same kind of play, different play | 65 / 80 | 0 / 20 | 0 – 64% / 20 – 99% |
| 3 | the same play | 50 | 100 | 100 – 149% |

So the defense helps most when its call matches the offense's play. The random number is the low
byte of the sim's random number generator.

---

## Step 4: Pass plays (confirmed)

NES routine `$9561`.

1. **Scramble check.** A random byte is compared with the QB's pocket value. If it hits, the QB runs
   with his Sim rush and the play is handled like a run.
2. **Pick a receiver.** A weighted random choice over the 5 starting skill players, using each
   player's **Sim targets** as the weight (`$95A9`).
3. **Completion.** The QB's **Sim pass** and the receiver's **Sim catch** are each rolled.
4. **Defense.** The routine switches to the defending team and rolls its **pass defense** (the low
   digit of its team byte) — `$963C`, through the same defense roll as runs.
5. **After the catch.** The receiver's **Sim yards per catch** is rolled for the gain (`$96B8`).

Emulator testing: QB Sim pass 0 → 15 raises completion % from 52 to 71 and cuts his INTs from 22 to
13 a season (NES_02); receiver Sim catch 0 → 15 raises completion % from 54 to 68 (NES_03); Sim
yards per catch 0 → 15 raises yards per catch from 21.6 to 27.7 (NES_04).

### Scrambles (confirmed)

At setup the pocket value is looked up in `$A103` (`51, 25, 13, 8`) and then masked with `AND #$03`,
which makes scrambles very rare (at most 3 in 256 pass plays). NES_07 confirmed they're rare in a
simulated season. On Genesis the pocket value isn't used at all: the scramble lookup (`$DB422`, table
`$DE9FA`) is indexed by the QB's condition-adjusted Sim pass instead, and the read of the pocket byte
is overwritten before use. The effect is at most 3 in 256 either way.

---

## Step 5: Sacks and interceptions (confirmed)

When a sack (`$9779`) or an interception (`$97F4`) happens, the defense picks **which** of its 11
defenders gets the credit:

* Sacks use **Sim pass rush** (`$0410…`, pick loop at `$9789`).
* Interceptions use **Sim coverage** (`$0426…`, pick loop at `$9802`).

The pick is not a proportional draw over all 11 defenders. It works like this:

```
R = random 0-255
for each of the first 10 defenders (RE, NT, LE, ROLB, RILB, LILB, LOLB, RCB, LCB, FS):
    if R < value: this defender gets the credit, stop
    else R = R - value
if nobody was picked: the 11th defender (SS) gets the credit
```

So each value is that defender's **share out of 256**, and whatever the first 10 don't use goes to
the SS. Tecmo built the data that way: in the stock ROM, 25 of 28 teams' pass-rush values and 23 of
28 teams' coverage values add up to exactly 255. For example, the Bills' pass rush is
`116, 30, 25, 25, 13, 13, 29, 0, 0, 2, 2` = 255, so the RE gets 116/256 = 45% of the team's sacks.

A few stock teams don't add up to 255, and it shows. The Bengals' first 10 coverage values already
add up to 249, so their SS (coverage 51) only gets 7/256 of the INTs. The Browns' first 10 add up to
191, so their SS (coverage 12) gets 65/256.

What this means for edits:

| A team's 11 values add up to | Result |
|---|---|
| exactly 255 | each defender gets his value / 256 of the credit (the SS gets his own value + 1) |
| less than 255 | the SS gets all the leftover, even for sacks |
| more than 255 | the values are not scaled: defenders early in the list (RE, NT, LE…) use up the 256 and the ones at the end get nothing |

For example, giving every defender 1 sends about 96% of sacks and INTs to the SS. Giving every
defender 200 sends 78% to the RE and 22% to the NT. NES_09 confirmed the rule (the 200 test came out
86% RE).

These values only distribute the stat. **Whether** the sack or interception happens is decided
elsewhere in the play: the QB's Sim pass and, less so, the receivers' Sim catch affect INTs (Step 4),
and the defending team's **pass defense** digit adds sacks and INTs (NES_05).

---

## Step 6: Kicking and returns

* **Kicker** value is used at `$9D2D` (field goals); **punter** value (30–45) is added at `$9B8E`
  and appears to be punt distance. **Partly unconfirmed.**
* **Kick and punt returns** use the returner's **Sim rush** (confirmed by NES_08), minus the
  **kicking team's rush defense** (high digit) — `$9A55` and `$9C1F`. So the high digit also acts
  as kick coverage.

---

## Emulator test results

One factor pushed to four levels (groups A–D = roster index mod 4), every other team value neutral,
a full season simulated. Full sheets in `TSBTool_Test\SimTests\`.

| Test | Factor (A → D) | Result |
|---|---|---|
| NES_01 | runners' Sim rush 0/5/10/15 | yards per carry 5.0 / 6.7 / 8.9 / 10.8; points for 280 → 412 |
| NES_02 | QB Sim pass 0/5/10/15 | completion % 52 → 71, INTs thrown 22 → 13, more TDs; yards per catch flat |
| NES_03 | receivers' Sim catch | completion % 54 → 68, INTs 20 → 16 |
| NES_04 | Sim yards per catch | yards per catch 21.6 → 27.7; completions and punt returns flat (it's not a return value) |
| NES_05 | team SimData 0x00/0xF0/0x0F/0xFF | high digit cuts rush yards allowed (about −920 a season); low digit cuts pass yards allowed (−516 to −737) and adds sacks and INTs; points allowed 486 / 418 / 424 / 324 |
| NES_06 | run/pass preference 0–3 | 42 / 38 / 55 / 62% passes (Step 2) |
| NES_07 | QB pocket | scrambles rare, as the code says |
| NES_08 | returners | returns follow the returner's Sim rush |
| NES_09 | defender pass rush / coverage | credit follows the fall-through rule (Step 5) |
| SNES_05 | team SimData | points allowed, yards allowed, sacks and INTs follow the digits like NES |
| SNES_06 | run/pass preference | about the same as NES |

The game doesn't track how many times a team was sacked.

---

## Genesis TSB1

Genesis TSB1 (68000) runs the same engine with the same tables, but stock Genesis has these bugs.
`GENESIS_FIX_SIM_BUGS` (a line in TSBTool's input, or a Yes in the GUI prompt when saving a Genesis
ROM) fixes them, in three groups. Each group is written only when every byte in it is the stock or
already-fixed code; if something else is there (another hack), that group is skipped with an error.

### Bug 1: preference 1 plays as heavy passing

The preference-1 code (`$DE790`) compares the roll with `$99` but has no branch after it, so it
always calls a pass: "heavy rushing" teams threw **72%** of the time (GEN_06). Its run code is already
there, unreached, at `$DE7AE`. The fix adds the branch in place (`bcs.s` to the run code, `bra.s` to
the store at `$DE7B8`). After the fix: 38% passes (GEN_06b), matching NES. In stock rosters this
affects the Chiefs, Raiders and Giants.

### Bug 2: the team defense digits barely count

The defense roll (`$DC198`) loads its random number with `move.b $ff339c,d0` (`$DC220`): the **top**
byte of the sim's random long, which the generator (`$DCF24`) always leaves 0. NES reads the random
low byte, and Genesis reads `$ff339f` everywhere else. So on Genesis `random % M` is always 0: the
defense counts for nothing in matchup types 0 and 1, and is never random. GEN_05/05b showed the
defense digits doing much less than on NES. The fix is one byte (`$DC225`: `9C` → `9F`). After the
fix (GEN_05c) the digits match NES: high digit −864 to −914 rush yards allowed, low digit −488 to
−518 pass yards, more sacks and INTs.

### The rebalance

With defense counting, a stock season scores about **40% less** (GEN_07b: 12.8 points a game against
20.8 stock), so Genesis's balance was built around the bug. The defense group of the fix also
rebalances the sim; the amounts were picked by simulating seasons on the stock rosters (GEN_07a–07k):

* the defense roll uses **(defense value − 83)**, not below 0 (call at `$DC23A` to a routine at
  `$DF100`);
* the main pass play takes **32 more** off the pass defense value before the roll (call at `$DBD24`
  to a routine at `$DF110`);
* the run-play cutoffs move **halfway to the NES ones**: `3E 7B B8 D7` → `39 71 A9 D2` (`$DBA55`,
  `$DBA6D`, `$DBA85`, `$DBA9D`).

The two routines use 32 bytes of unused (`0xFF`) space after the sim data (`0xDF100`–`0xDF11F`;
nothing in the ROM points there, and it's free in the community ROMs checked).

| Season (stock rosters) | Points / game | Rush yds / game | Pass yds / game* | Comp % | 1,000-yd receivers | Defense digits vs points allowed** | Points allowed, 3 best vs 3 worst defenses |
|---|---|---|---|---|---|---|---|
| GEN_07a stock | 20.8 | 122 | 138 | 74.8 | 9 | −0.27 | 19.8 vs 22.5 a game |
| GEN_07b bug fixes only | 12.8 | 73 | 115 | 58.5 | 1 | −0.57 | 10.5 vs 16.1 |
| GEN_07j fixes + rebalance | 20.9 | 127 | 137 | 75.6 | 5 | −0.77 | 15.1 vs 26.6 |
| GEN_08b (same, made by TSBTool) | 21.3 | 134 | — | 75.7 | 3 | −0.84 | — |

\* teams not on preference 1. \*\* correlation over the 28 teams; more negative = defense matters more.

Stock's 9 thousand-yard receivers include 3 from preference-1 teams throwing 72% because of Bug 1.

Other options tried (sheets in SimTests): shifting the defense tables (07c, same result as the
offset), the full NES run cutoffs (07e, 07g: too much rushing, 152 yards a game), a bigger offset
(07f, scoring barely moves), scaling the defense value to 3/8 (07h: stock scoring, but the rush
digit stops mattering), a bigger pass offset (07k: completion % above stock).

Still open after the fix: the **pass** defense digit matters little (about 1 pass yard a game per
digit point), and sacks / INTs are about 20–25% below stock.

### Bug 3: season receiving TDs are lost

After each game (simmed or played) the season totals are updated stat by stat (`$DDCE2` → `$DD4EE`,
handlers at `$DD0C2`). The receiving-TD handler (stat `$0A`, `$DD816`) reads the season value with
the QB pass-TD helper `$DD330`, which shifts the byte right by 2 (pass TDs are stored ×4), though
receiving TDs are a plain byte. So after each game the total becomes old ÷ 4 + this game's TDs, and
receivers end a season with 0–2 TDs (Rice 77-1,207, 1 TD). The other 29 stat handlers read their
stats correctly. The fix calls the plain-byte helper `$DD318` instead (`$DD81A`: `FB 16` → `FA FE`).
GEN_08b: every team's receiving TDs add up to its passing TDs (590 / 590).

### Genesis addresses

| What | Address |
|---|---|
| Sim setup | `$DB35E` |
| Sim data pointer table (with the team swaps) | `$DEA0E` |
| Sim lookup tables (same as NES `$A0A3`–`$A117`) | `$DE99A`–`$DEA0E` |
| Defense tables (rush / pass), copied to RAM `$FF3D8C` / `$FF3D8E` | `$DE9BA` / `$DE9CA` |
| Run play: defense roll call / yardage cutoffs | `$DBA30` / `$DBA55`, `$DBA6D`, `$DBA85`, `$DBA9D` |
| Pass play: pass defense load / defense roll call | `$DBD16` / `$DBD2A` |
| Defense roll (M / A tables at `$DD032` / `$DD036`) | `$DC198` |
| Other readers of the defense values (special plays) | `$DC3C0`, `$DC4DE`, `$DC894` |
| Play calling by preference (jump table `$DE6F8`) | `$DE75C`; preference 0 `$DE77C`, 1 `$DE790` |
| Sim random number generator (long at `$FF339C`) | `$DCF24` |
| Season stats update after a game | `$DDCE2`, `$DD4EE`, handlers `$DD0C2` |
| Season stats in RAM | `$FF726A + team * 0x1D0` (roster order) |

TSBTool can read the season stats from an ares save state (`-stats:file.bs1`, or File → View Genesis
Season Stats); the RAM layout is documented in `Core\GenesisSeasonStats.cs`.

---

## What this means when editing a ROM

* To make a team **score more in sims**, raise its starters' Sim rush (runners), Sim pass (QB),
  Sim catch / yards per catch (receivers), and the kicker. Its team byte won't help its offense.
* To make a team **allow less in sims**, raise its team byte: high digit for run defense (and
  kick coverage), low digit for pass defense. On Genesis this only works properly with
  `GENESIS_FIX_SIM_BUGS`.
* **Sim targets** controls how the ball is spread around; it doesn't make the offense better.
* **Defender pass rush / coverage** controls who leads the team in sacks / interceptions. Keep
  each team's 11 values adding up to 255; any shortfall goes to the SS, and anything over 255
  starves the defenders at the end of the list (the DBs).
* The **run/pass preference** sets the play mix, from about 40% passes (1) to about 60% (3). On stock
  Genesis, preference 1 throws 72% unless the ROM is fixed.
* Kick and punt returners are rated by their **Sim rush**.
* Keep starters healthy and in good condition: condition shifts offensive sim values by up to 4.

---

## Address reference (NES `TSPRBOWL.nes`)

File offset = `0x10 + bank * 0x2000 + (address & 0x1FFF)`.

| What | Bank | Address |
|---|---|---|
| Sim setup (loads both teams' sim blocks) | 0x0B | `$91CA` |
| Sim data pointer load | 0x0B | `$91D5` |
| Condition modifier | 0x0B | `$9396`, `$93D9` (table `$A084`) |
| Play dispatcher | 0x0B | `$9108` |
| Run play | 0x0B | `$9446` |
| Pass play | 0x0B | `$9561` |
| Defense roll | 0x0B | `$98CD` |
| Sack credit | 0x0B | `$9779` |
| Interception credit | 0x0B | `$97F4` |
| Kick return / punt return | 0x0B | `$9A10`, `$9BDC` |
| Play calling | 0x0C | `$A6F3` |
| Run/pass preference split | 0x0C | `$AA84` (jump table `$AA4E`) |
| Sim lookup tables | 0x0C | `$A0A3`–`$A116` |
| Sim data blocks | 0x0C | `$A153` (file `0x18163`) |
| Run/pass preference table | 0x13 | `$9516` (file `0x27526`) |

Working RAM used by the sim:

| RAM | Holds (team 0 / team 1) |
|---|---|
| `$03D6`, `$03D8…` | QB / skill Sim rush |
| `$03E6`, `$03E8…` | QB Sim pass / skill Sim catch |
| `$03F2…` | Sim yards per catch |
| `$0400…` | Sim targets |
| `$040A` / `$040B` | Team rush defense |
| `$040C` / `$040D` | Team pass defense |
| `$040E` | QB pocket (scramble) |
| `$0410…`, `$0426…` | Defender pass rush / coverage |
| `$0620` / `$0621` | Run/pass preference |

SNES: sim setup `$04:CEAB`, team rush / pass defense in RAM `$0442` / `$0444`.

---

## Open questions

1. The exact yardage within each run and pass tier, and how the offense side of the roll is built.
2. What the matchup-type bytes (Genesis `$FF3D33`) are exactly (they look like the two teams' play
   calls).
3. Why the **pass** defense digit matters so little on Genesis (also before the fix), and whether
   sacks / INTs can be brought back to stock levels without losing scoring.
4. Which value drives the punter's distance and the kicker's range in detail.
