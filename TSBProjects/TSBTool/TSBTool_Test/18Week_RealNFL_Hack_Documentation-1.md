# 18-Week Real NFL Schedule Hack — Technical Documentation

Applies to the 32-team NES Tecmo Super Bowl-derived ROM family (SbluemanBase / Dgreen17gameEdition and
compatible variants). Extends the season from 17 weeks with no byes to a genuine **18-week season with
authentic real-2026-NFL bye weeks**, using the real matchup data for every game.

Base ROM assumed: MMC3 mapper, 256KB PRG, 256KB CHR, 32-team roster (BUF=0 … ARZ=33, with 28/29 reserved
as AFC/NFC conference markers, unused as real team IDs).

---

## Summary of what changed

| # | Fix | Purpose |
|---|-----|---------|
| 1 | `TOTAL_GAMES_PER_TEAM` (2 sites) | Division-clinch math: 16 → 17 games/team |
| 2 | Pointer table (17 → 18 entries) | Extends the per-week schedule lookup table |
| 3 | `GAMES_PER_WEEK_ARRAY` (relocated, 18 entries) | Real per-week game counts, including bye weeks |
| 4 | 3× code references to `GAMES_PER_WEEK_ARRAY` | Updated to the array's new relocated address |
| 5 | Schedule data (relocated, 544 bytes) | The actual 272-game real-2026-NFL schedule |
| 6 | 10× `FIRST_WEEK_OF_PLAYOFFS` sites | Season-boundary checks: 17 → 18 |
| 7 | Schedule-page "clear" routine (new code) | Fixes a rendering bug where week 18 was hidden/corrupted |
| 8 | Autoskip menu size byte | Allows selecting week 18 from any point in the season |

Everything is placed using two blocks of genuine free space already present in the ROM — no existing
code or data was overwritten in the process (a byte-for-byte diff against the clean ROM confirms this).

---

## Team ID reference

```
0=BUF   1=MIA   2=NE    3=NYJ   4=CIN   5=CLE   6=BAL   7=PIT   8=IND   9=HOU
10=JAX  11=TEN  12=DEN  13=KC   14=LV   15=LAC  16=WAS  17=NYG  18=PHI  19=DAL
20=CHI  21=DET  22=GB   23=MIN  24=TB   25=NO   26=ATL  27=CAR
28=AFC (unused) 29=NFC (unused)
30=SF   31=LAR  32=SEA  33=ARZ
```

## Key addresses

```
Pointer table start:        file 0x329A7   (fixed address, do not move)
GAMES_PER_WEEK_ARRAY:       file 0x329CB   (18 bytes + 0xFF terminator at 0x329DD)
Relocated schedule data:    file 0x32D36   (544 bytes: 272 games × 2 bytes)
New rendering-fix routine:  file 0x32F5E   (48 bytes)
Bank base for this region:  0x32010 (file) = $8000 (CPU)
```

---

## Fix 1 — `TOTAL_GAMES_PER_TEAM` (division-clinch math)

Two identical hardcoded constants used in the "has this division been mathematically clinched" logic,
originally `16` — the old 16-game-per-team assumption. With 17 real games per team, this needed updating
or clinch calculations would be off by one game.

```
0x22EA2: A9 10  →  A9 11        (LDA #$10 → LDA #$11)
0x22EDE: A9 10  →  A9 11        (LDA #$10 → LDA #$11)
```

---

## Fix 2 & 3 — Pointer table and `GAMES_PER_WEEK_ARRAY`

The ROM uses a genuine, code-referenced pointer table — 17 two-byte entries (one per week), each pointing
to that week's matchup data. Confirmed via community documentation and directly verified against this
ROM's actual code (two separate `LDA $8997,X`-style indexed reads use this table's fixed starting
address, `$8997` CPU / `0x329A7` file — this address must **never move**).

Extending to 18 weeks means the table grows from 34 to 36 bytes. Since `GAMES_PER_WEEK_ARRAY` sat
immediately after it with zero padding, it had to shift forward by 2 bytes (from `0x329C9` to `0x329CB`).

```python
# New pointer table (18 entries), computed from the relocated schedule data:
pos = 0x32D36  # relocated schedule start
for games_this_week in games_per_week_18:
    pointer = 0x8000 + (pos - 0x32010)   # CPU address in this bank
    write_word(pointer)                   # little-endian, at 0x329A7 + i*2
    pos += games_this_week * 2

# New GAMES_PER_WEEK_ARRAY at the shifted location:
# [16,16,16,16,15,14,14,14,15,14,13,16,14,15,16,16,16,16], terminated by 0xFF
```

## Fix 4 — Code references to `GAMES_PER_WEEK_ARRAY`

Three separate instructions read this array via indexed addressing (`LDA $89B9,X`). Since the array's
address shifted by 2 bytes (`$89B9` → `$89BB`), each reference needed updating:

```
0x32087: BD B9 89  →  BD BB 89
0x321F8: BD B9 89  →  BD BB 89
0x3224F: BD B9 89  →  BD BB 89
```

## Fix 5 — Schedule data (the actual 272 games)

The original schedule (256 games, old 16-game-plus-bye structure) fit in 512 bytes with **zero padding**
before real, active code — writing the new 544-byte schedule in place would corrupt that code (this
caused a full freeze-on-load in early testing). The fix: relocate to genuine free space instead.

```
0x32D36 – 0x32D36+544:  the real 2026 NFL schedule, 18 weeks, all 272 games,
                          in (home_id, away_id) byte pairs, exactly matching
                          each week's real-world matchups and bye weeks.
```

Verified: every one of the 32 teams appears in exactly 17 of the 18 weeks (one authentic bye each);
total games = 272; the previously-freeze-causing code region is confirmed byte-for-byte unchanged from
the clean ROM.

---

## Fix 6 — `FIRST_WEEK_OF_PLAYOFFS` boundary (10 sites)

A single, consistently-used constant (`17` → `18`) marking "has the season reached the playoffs yet."
Found via exhaustive signature search — every occurrence loads `CURRENT_WEEK_IN_SEASON` (SRAM `$6758`)
and compares against this boundary, gating the playoff bracket trigger, halftime-show logic, the
schedule-screen's own "am I past the season" check, and season-start branching.

```
9 direct sites (LDA/LDX/LDY $6758; CMP/CPX #$11 → #$12):
  0x202A2   0x20396   0x212C2   0x22095   0x225DF
  0x22E28   0x23362   0x233EC   0x3217E

1 offset-variant site (used by the on-screen week-number display,
  which adds 1 before comparing):
  0x2E85A:  AD 58 67 18 69 01 C9 11  →  ...C9 12
            (LDA $6758; CLC; ADC #$01; CMP #$11 → #$12)
```

---

## Fix 7 — Schedule-page rendering fix (new code, not a value patch)

**The bug:** with weeks 17 and 18 sharing the same schedule display page (`week_index & 0xFC` naturally
groups them), the original "clear leftover screen space" routine always wiped 18 rows starting at a
fixed row (row 10 / PPU `$2140`) — correct when only one week showed there, but this same fixed clear
now ran *after* week 18 had legitimately drawn in that exact space, immediately erasing it. This produced
a corrupted/hidden display for week 18, and (in a second discovered layer) stale content bleeding through
from whichever week previously occupied that screen region during manual scroll-through.

**The fix:** a new 48-byte routine, written into free space, that checks `number_of_weeks_on_page_drawn`
(zero page `$92`) and chooses the correct clear parameters:

```asm
; New routine at file 0x32F5E (CPU $8F4E)
LDA $92              ; number_of_weeks_on_page_drawn
CMP #$02
BCC @clear_single     ; only 1 week drawn -> use original behavior

; 2 weeks drawn (our case): clear only the remaining unused slots,
; starting 6 rows lower than before (each week's band is ~5-6 rows)
LDY #$00              ; low byte of $2200 (row 16)
LDX #$22               ; high byte
LDA #$0C              ; 12 rows to clear (vs. original 18)
STA $3F
LDA #$20              ; row length unchanged (32 tiles)
STA $3E
JSR $D09B             ; call the existing fill/clear routine
LDX #$11
JSR $D8E3
RTS

@clear_single:         ; original, unchanged behavior
LDY #$40
LDX #$21
LDA #$12
STA $3F
LDA #$20
STA $3E
JSR $D09B
LDX #$11
JSR $D8E3
RTS
```

The routine's original entry point (`0x320DF`) is redirected to this new code via a 3-byte `JMP` (plus
1 filler `NOP`); the original code itself is left completely intact and reachable, used as the fallback
for the "1 week only" case.

---

## Fix 8 — Autoskip menu size (the "week 31" bug and its correct fix)

**First attempt (wrong):** the code sequence right after the week-boundary check reads:
```
LDX $6758        ; current week
CPX #$12         ; (already-fixed boundary)
BCS ...
INX
CPX $6243        ; SRAM "remembered last skip target", a high-water-mark value
BCS +3
LDX $6243
STX $E1           ; starting spinner position
LDA #$12          ; <- this looked like "18 = menu size" but is NOT the value itself
STA $E3
LDA #$84
STA $E4
```
`LDA #$12; STA $E3` / `LDA #$84; STA $E4` are **not** the menu-size value — they're the low/high bytes
of a 16-bit pointer (`$8412`) to where the real size byte lives. Changing `$12` here to `$13` shifted the
*pointer itself* one byte off target, causing it to read unrelated garbage data — this is what produced
the nonsensical "select up to week 31" bug.

**Correct fix:** leave the pointer bytes untouched, and instead patch the actual byte the pointer points
to, at its real, traced location:

```
0x20422: 12  →  13        (the genuine autoskip_choices value, 18 → 19)
```

This correctly extends the selectable range by exactly one week (to include week 18) without disturbing
anything else, and was confirmed via full source-level tracing of `AUTOSKIP_START` / `WEEK_TO_SKIP_TO` /
`AUTO_SKIP_MENU_NUM_WEEKS_MENY_OPT_ADDR` in the reference disassembly.

---

## Verification performed

- Every one of the 32 teams appears in exactly 17 of 18 weeks (one real bye each), 272 total games.
- Pointer table entries recomputed and confirmed to exactly match the relocated schedule's real byte
  offsets.
- The originally freeze-causing code region (`0x32BDB` onward) confirmed byte-for-byte identical to the
  clean ROM in the final build.
- IPS patch verified by applying it to a fresh, unmodified copy of the target ROM and confirming an
  exact byte-for-byte match against the working, tested build.
- Live-tested: full 18-week season simulation, all 8 division winners, playoff bracket, and standings
  confirmed correct; schedule screen renders cleanly for the week 17/18 shared page; autoskip menu
  correctly offers weeks up to 18 (and no further) from any point in the season.
