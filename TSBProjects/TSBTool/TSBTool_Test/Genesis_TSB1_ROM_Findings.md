# Genesis Tecmo Super Bowl (1993) — ROM Findings

Reference for the Genesis (Sega Mega Drive) release of Tecmo Super Bowl, implemented in
`Core\Genesis_TSB1Tool.cs`/`Core\Genesis_ScheduleHelper.cs`. Target ROM: `Tecmo Super Bowl (USA)
(October 1993).md`, 1MB (`0x100000` bytes), unheadered. All addresses below are confirmed directly
against real ROM bytes (cross-referenced against known real-world 1993 rosters/schedules/colors, and
in several cases against SNES TSB1's own decoded values for teams whose data carried over unchanged).

Team index (0=Bills … 27=49ers) and the 32-slot roster position order
(`QB1,QB2,RB1,RB2,RB3,RB4,WR1,WR2,WR3,WR4,TE1,TE2,C,LG,RG,LT,RT,RE,NT,LE,ROLB,RILB,LILB,LOLB,RCB,LCB,
FS,SS,K,P,DB1,DB2`) are used consistently across every table in this ROM.

**Three different pointer-table byte formats appear in this one ROM** — don't assume one applies
elsewhere:

| Table | Pointer format |
|---|---|
| Shared string table | `00 00 hh ll` (4 bytes, value in low 2 bytes) |
| Schedule week-pointer table | `hh ll 00 00` (4 bytes, **reversed**) |
| Roster/name pointer table | `hh ll` (2 bytes, tightly packed, no padding) |

## Shared string table (team abbreviations/cities/nicknames + misc UI labels)

Pointer array `0x45C4`–`0x47C8` (129 × 4-byte entries — 128 real strings + 1 sentinel marking where
the last string ends), string data `0x47C8`–`0x4ABF`, implicit length via adjacent-pointer
subtraction, no delimiters.

| Index range | Content |
|---|---|
| 0–27 | Team abbreviations, fixed 4 bytes, underscore-padded (`BUF_`, `N_E_`, …) |
| 28–30 | `NFL`, `AFC`, `NFC` |
| 31 | empty (unexplained) |
| 32–59 | City/market names, variable length |
| 60–63 | Unidentified 3-char strings: `efg`, `hif`, `bcd`, `jkl` |
| 64–91 | Team nicknames, variable length |
| 92–93 | `AFC`, `NFC` (again) |
| 94–95 | empty (unexplained) |
| 96–113 | Ordinal labels `" 1ST"`…`" 18TH"` |
| 114–124 | `OFFENSE`, `DEFENSE`, `WEEK`, `PRESEASON`, `REGULAR SEASON`, `FIRST ROUND`, `DIVISIONAL`, `PLAYOFF`, `CHAMPIONSHIP`, `SUPER BOWL`, `PRO BOWL` |
| 125–127 | `EAST`, `CENTRAL`, `WEST` |

Implemented: `Get/SetTeamAbbreviation`/`City`/`Name` (`abbreviationIndexBase=0`, `cityIndexBase=32`,
`nicknameIndexBase=64`).

## Schedule — 3 seasons baked in; only season 3 is the real 1993 schedule

The cartridge carries 3 complete, independent 224-game schedules back-to-back. **Season 3 is the one
this tool reads/writes** — it's the only one of the three with an 18-week/bye-week games-per-week
structure matching the NFL's actual 1993 format, and its week 1 decodes to the real 1993 opening week
(Bills at Patriots, Browns at Bengals, Colts at Dolphins, …). Seasons 1 and 2 are flat 17-week blocks
(no bye-week dip), presumably 1991/1992, and are out of scope.

| Region | File range | Structure |
|---|---|---|
| Week-pointer table | `0x5210`–`0x52EA` | 55 × 4-byte entries. Entries 0–16 = season 1 weeks 1–17. Entries 17–33 = season 2 weeks 1–17. **Entries 34–51 = season 3 weeks 1–18** (used by this tool). Entries 52–54 = pointers to the 3 games-per-week arrays (season 3, then 2, then 1). |
| Games-per-week arrays | `0x52EA`–`0x5321` | Three `0xFF`-terminated arrays: season 1 at `0x52EA` (17 values), season 2 at `0x52FC` (17), **season 3 at `0x530E` (18, used by this tool)**. |
| Schedule data | `0x5322`–`0x5862` | Three 448-byte `(away,home)` team-index-byte-pair blocks: season 1 at `0x5322`, season 2 at `0x54E2`, **season 3 at `0x56A2` (used by this tool)**. 224 games each, every team appears 16 times. |

Season 3 games-per-week: `14 14 10 10 11 11 10 10 12 12 13 13 14 14 14 14 14 14`.

Implemented in `Genesis_ScheduleHelper.cs`: `weekPointersStartLoc=0x5298` (= `0x5210 + 4*34`),
`gamesPerWeekStartLoc=0x530E`, `weekOneStartLoc=0x56A2`, `endScheduleSection=0x5862`, `totalWeeks=18`.

## Roster / player names

Pointer table `0x5862`–`0x5F62`, 896 entries × 2 bytes (28 teams × 32 slots, `flatPlayerIndex =
teamIndex*32 + positionIndex`), tightly packed. **A real 897th sentinel pointer sits immediately
after the table, at `0x5862 + 2*896 = 0x5F62`** (stock value `0x89FC`) — every entry's length,
*including the last one*, is `pointer[i+1] - pointer[i]`, with no special-casing needed. (An earlier
pass mistook this sentinel for a 2-byte alignment gap and invented a synthetic slack-tracking scheme
instead; that's now removed — see `Genesis_TSB1Tool.GetRosterEntryLocation`'s own comment.)

Name data starts at `0x5F64`, packed contiguously, ending at `0x89FC` (start of the attribute table)
in the stock ROM.

Record format:
```
byte 0:        jersey number (written as-is; hex-parsing "#NN" already produces the same byte a
               BCD-encoding would for any 2-digit number, so no separate BCD conversion is needed)
byte 1..k:     first name, lowercase ASCII
byte k+1..end: last name, UPPERCASE ASCII
```

Implemented: `InsertPlayer`/`GetPlayerName`/`GetAllPlayers`. `RosterFitReport`/`ResolveRosterFit`/
`CheckRosterFit`/`Compress49ersNames` handle the (real, non-hypothetical) case where a replacement
roster's names are collectively longer than the original and would overflow into the attribute table.

## Player attributes/ratings

Flat 5-byte record at `0x89FC + flatPlayerIndex*5`, no pointers. Same 16-level nibble scale as
`TecmoTool.GetAbility`/`MapAbility`: `6,13,19,25,31,38,44,50,56,63,69,75,81,88,94,100`.

| Offset | Content |
|---|---|
| `+0` | `(RushingPower<<4)\|RunningSpeed` — every position |
| `+1` | `(MaxSpeed<<4)\|HittingPower` — every position |
| `+2` | Skin-tone flag, `0x00`/`0x80`, for 888/896 players (matches `SNES_TecmoTool.SetFace`'s own collapsed scheme). **8 players have a third stock value `0x20`, confirmed not a third skin tone** (the 8 span both Black and white real players) — leading unconfirmed hypothesis: a "Preseason Injury Flag" per an older community doc. `SetFace` can only ever write `0x00`/`0x80`; `GetFace` returns the true raw byte. |
| `+3` | Position-dependent: skill (RB/WR/TE) = `(BallControl<<4)\|Receptions`; defensive = `(PassRush<<4)\|Interceptions`; K/P = `(KickingAbility<<4)\|AvoidKickBlock`; OL = unused, always `0x00`. |
| `+4` | QB1/QB2 only: `(PassAccuracy<<4)\|Arm`. `0x00` for every other position (deliberate padding). |

`BB`/`AG`/`CO` (from third-party-tool ground truth) aren't accounted for by any of these 5 bytes —
open, low-priority (least game-affecting stats in the original design too).

## Playbooks

Flat `0x4CC4 + teamIndex*4`, 4 bytes/team, same nibble-packed scheme as NES/SNES:
`byte0=(R1<<4)|R2, byte1=(R3<<4)|R4, byte2=(P1<<4)|P2, byte3=(P3<<4)|P4`.

## Sim/CPU data

48-byte-per-team block at `0xDEA86 + teamIndex*0x30` (`teamSimBlockStart`), plus a separate 28-byte
tendency array (1 byte/team, values 0–3) at `0xB58E` (`teamSimOffensivePrefStart`).

| Relative offset | Content |
|---|---|
| `0x00`–`0x17` | 12 offensive positions (`QB1..TE2`, same order as roster), 2 nibble-packed values each at `positionIndex*2` |
| `0x18`–`0x22` | 11 defensive positions' pass-rush/coverage value, 1 byte each at `0x18+defIndex` |
| `0x23`–`0x2D` | same 11 defensive positions' interception-ability value, at `0x23+defIndex` (exactly `+0xB` past pass-rush) |
| `0x2E` | Kicking (hi nibble) / Punting (lo nibble) |
| `0x2F` | team-level offense/defense summary nibbles (the `SimData=0xNN` in `TEAM =` lines) |

Defensive position order within the sub-block: `RE,NT,LE,ROLB,RILB,LILB,LOLB,RCB,LCB,FS,SS`. No
Redskins/Cowboys-style team-order quirk on Genesis (unlike SNES's `mSimTeams`) — plain main team
order throughout.

## Pro Bowl roster

Base `0x4C30`: AFC block at `+0`, NFC at `+0x46`. 35 slots × 2 bytes/conference (32 roster positions
+ `RET1`/`RET2`/`RET3` at slot indices 32–34). `loc = 0x4C30 + confOffset + 2*slotIndex`, each slot
storing `[teamIndex][positionIndex]` (reversed byte order from SNES's `[positionIndex][teamIndex]`).

## Kick/punt returner & return team

`0x4BC0`–`0x4BDC`: 1 byte/team (28 bytes), hi nibble = kick returner's index (0–2) into that team's
3-man return-team roster, lo nibble = punt returner's index.

`0x4BDC`–`0x4C30`: 3 bytes/team (84 bytes, `0x4BDC + teamIndex*3`), `[pos0][pos1][pos2]`, each a
roster-position index naming that member of the return team (3 bytes/team, not SNES's 4 — no spare
byte). An unexplained 8-byte gap sits between the end of the Pro Bowl table (`0x4CBC`) and the start
of the playbook table (`0x4CC4`) — not investigated.

## Uniform colors

16-color palette per team at `0x58974 + teamIndex*0x40` (Genesis VDP CRAM format: `0000 bbb0 ggg0
rrr0`, 3 bits/channel doubled into the even nibble — `R=(word>>1)&7`, `G=(word>>5)&7`, `B=(word>>9)&7`,
scale ×255/7 for display).

| Index | Role |
|---|---|
| 0–2 | Jersey color ramp (shadow→mid→highlight) |
| 3–5 | Pants color ramp |
| 6–9 | Shared skin-tone + trim — identical across every team, not settable |
| 10–11 | Shared black (shoes/outline) — identical across every team |
| 12–15 | Helmet color ramp |

Confirmed against real in-game screenshots for 10 teams (Bills, Colts, Dolphins, Patriots, Jets,
Steelers, Packers, Raiders, Cowboys, Redskins) — every jersey/pants/helmet color matched. This appears
to be a single palette shared by home and away (no separate block found); whether a distinct
away-palette exists elsewhere per team is unconfirmed. Implemented: `SetHomeUniform`/`SetAwayUniform`/
`GetGameUniform` (`uniformColorStart=0x58974`).

## Year text (menu labels, not a single "current year")

Unlike NES/SNES (a single season → one "current year" concept patched everywhere), this ROM's menus
show all 3 baked-in season years side by side — season-select, "view schedule", etc. all list
`1993....1992....1991` (or similar orderings) as literal on-screen text. `SetYear` is a deliberate
blanket find-and-replace of the 9 real on-screen `"1993"` occurrences (does not attempt to keep the
3-year menus semantically consistent; `1991`/`1992` are left alone):

| Address | Context |
|---|---|
| `0x1CD0F` | `TECMO_LTD_1993` (copyright credit) |
| `0x1CD77` | `SUPER BOWL.......1993 NFL` (title screen) |
| `0x1D08E` | `....1993....1992....1991....REGULAR SEASON` (season-select menu) |
| `0x1D1FA` | `1991 1992 1993....` (season-select menu) |
| `0x1D212` | `....1993....SELECT CONTROL MODE` (season-select menu) |
| `0x1D47E` | `NFL SCHEDULE [\].....1993....1992....1991` (schedule-view menu) |
| `0x1D5FD` | `1993 SEASON` (standalone menu label, `1992 SEASON`/`1991 SEASON` follow) |
| `0x4B6AA` | `1993........ NATIONAL FOOTBALL LEAGUE` (legal boilerplate) |
| `0x4C164` | `1993........TECMO IS A REGISTERED TRADEMARK` (legal boilerplate) |

**Deliberately excluded**: the ROM header's own `(C)T-36 1993.OCT` copyright-date field at `0x118` —
cartridge metadata some tools use for ROM identification, not rendered game text.

## Known gaps / unconfirmed

- Three empty strings at indices 31, 94, 95 — possibly reserved/unused slots.
- Small 2-byte gap between the roster pointer table's real end (`0x5F62`, now known to be the
  sentinel) and where name data starts (`0x5F64`).
- Uniform usage (per-opponent dark/light jersey choice) — SNES's equivalent table was never located
  on Genesis despite extensive searching; stubbed as a no-op.
- Team offensive formation — same wall as uniform usage; stubbed as a no-op.
- 8-byte gap between the Pro Bowl table (`0x4CBC`) and the playbook table (`0x4CC4`).
- 4 Pro Bowl roster mismatches (of 70 slots) vs. SNES's selections — likely legitimate differences
  between the two ports, not a formula error.
- Whether Genesis has a separate home/away uniform palette pair per team (only one game's screenshots
  checked).

## Architecture

`Genesis_TSB1Tool` is a standalone `ITecmoTool`/`ITecmoContent` implementation (own team list, own
`positionNames`, own pointer-table helpers) — not a subclass of `TecmoTool`/`SNES_TecmoTool`, the same
pattern `TSB2Tool`/`TSB3Tool` already use. It speaks the same `InputParser` text protocol as every
other ROM family, so the CLI, GUI, and text-round-trip test pattern all work unmodified.
