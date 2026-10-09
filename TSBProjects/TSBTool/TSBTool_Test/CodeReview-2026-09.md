# TSBTool code review — Core and TSB2_3_Core (non-UI)

**Date:** 2026-09-29
**Scope:** `Core/*.cs` (18 files, ~14.9k lines) and `TSB2_TSB3/TSB2_3_Core/*.cs` (8 files, ~4.3k lines). GUI code not reviewed.
**Not re-reviewed in depth:** `Core/ScheduleTransformer.cs` (written and tested this session) and `Core/PlayerNames.cs` (has its own test effort).
**No code was changed** — this document is for discussion.

## How to read this

- **Severity:** **High** = can corrupt a ROM, silently lose or misplace user data, or hide errors from the user. **Medium** = wrong results for plausible input, or a crash/abort on bad input. **Low** = cosmetic, misleading messages, dead code, unlikely edge cases.
- **Status:** **Reproduced** = shown with a scratch program against the test ROMs (checks R1–R11). **Confirmed by reading** = the code clearly does this, not run. **Possible** = depends on intent or ROM data I couldn't verify.
- Line numbers are as of this review (uncommitted working tree).

## Summary

| ID | Sev | Finding | Status |
|---|---|---|---|
| H1 | High | Command-line mode prints "Error: Message" instead of the actual error text | Reproduced (R11) |
| H2 | High | A misspelled `TEAM =` line applies the following roster to the *previous* team | Reproduced (R4) |
| H3 | High | NES player names can grow past the name area into the Bills' attribute table | Reproduced (R8) |
| H4 | High | `JUICE(ALL, n)` skips week 1 and writes past the end of the NES juice table | Reproduced (R1) |
| H5 | High | `JUICE(week, 0)` crashes and aborts the rest of the file | Reproduced (R2) |
| H6 | High | Invalid attribute values (e.g. 55) are silently stored as 6 — no error | Reproduced (R3) |
| H7 | High | TSB2/TSB3: one malformed player line aborts the rest of the file | Reproduced (R7) |
| H8 | High | TSB2/TSB3: `LB5` player lines are silently ignored | Reproduced (R6) |
| H9 | High | Saving over a larger existing file leaves the old file's trailing bytes | Reproduced (R10) |
| M1 | Med | Team sim data `0x0F` exports as `SimData=0xf` and doesn't re-import | Reproduced (R5) |
| M2 | Med | Pro Bowl line with a bad position/team writes outside the Pro Bowl table | Reproduced (R9) |
| M3 | Med | Sim values over 15 spill into the neighboring value's nibble | Confirmed by reading |
| M4 | Med | Growing a team/player string has no free-space check (NES, SNES, TSB2/TSB3, Genesis team strings) | Confirmed by reading |
| M5 | Med | TSB2/TSB3 player sim data writes as many bytes as given — extras overwrite the next player | Confirmed by reading |
| M6 | Med | After an exception, "Data not applied" is shown although earlier lines *were* applied | Confirmed by reading |
| M7 | Med | TSB1→TSB2/TSB3 converter throws on K/P lines without sim values, or any malformed line | Confirmed by reading |
| M8 | Med | Sim auto-updater can replace an identical player line on the wrong team | Confirmed by reading |
| M9 | Med | Shared static team lists: creating a TSB3 tool changes a live TSB2 tool's team list | Possible |
| M10 | Med | SNES `SetFace`: skin tone and cutscene race use different thresholds | Possible |
| L1–L13 | Low | Messages, dead code, small edge cases — see below | mostly by reading |

**Themes behind most of these:**
1. **Copied code.** `TecmoTool`, `SNES_TecmoTool`, `Genesis_TSB1Tool` and `TSB2Tool` each have their own copy of the same logic (ability mapping, sim setters, pro bowl, string tables, `SaveRom`, the parser). Most bugs exist in 2–4 places, and the TSB2/TSB3 fixes from this session show how fixes don't propagate.
2. **Bad input is accepted silently** (H2, H6, H8, M1–M3): typos become wrong data instead of errors.
3. **No bounds checks when data grows** (H3, M4, M5). Genesis's `InsertPlayer` overflow check and `CXRomTSBTool`'s save-time check are the models to copy.

---

## High

### H1 — Command-line errors show a placeholder instead of the message
- **Where:** `Core/Interfaces.cs:242` (`ShowError`), also `:237` (`ShowMessageBox`)
- **What:** `ConsoleMessageGiver.ShowError` writes the literal text `"Error: Message"`; the `message` parameter is never used. `ShowMessageBox` writes `"Message"`. `MainClass` uses this giver in command-line mode, so every error `StaticUtils.ShowErrors()` reports (bad lines, schedule warnings, overflow rejections, the new schedule-fit summary) reaches the user as "Error: Message".
- **Reproduced (R11):** `ShowError("Error!", "the real message")` printed `Error: Message`.
- **Fix:** `Console.Error.WriteLine("{0}: {1}", title, message);` (and the same for `ShowMessageBox`). One-line change; high value.

### H2 — Misspelled `TEAM =` line sends the following roster to the previous team
- **Where:** `Core/InputParser.cs:300-308`; same in `TSB2_TSB3/TSB2_3_Core/InputParser.cs:309-317`
- **What:** `currentState` is set to roster mode *before* the team name is validated. When `SetCurrentTeam` fails, the method returns but `currentTeam` keeps the previous team, so every player line after the typo overwrites that team's roster. The user gets one "Team 'jetz' is Invalid" error, easy to miss among others.
- **Reproduced (R4):** `TEAM = patriots` … `TEAM = jetz` then a QB1 line → the Patriots' QB1 was renamed.
- **Fix:** on an invalid team, enter a "skip until the next `TEAM =` line" state (and report how many lines were skipped).

### H3 — NES player names can overflow into the attribute table
- **Where:** `Core/TecmoTool.cs:923-946` (`ShiftDataAfter`, end of region `0x300F`), `:979-992` (`ShiftDataDown`)
- **What:** When a name gets longer, the data after it is shifted down with no check that it still fits before `0x300F`. The next region starts at `0x3010`, the Bills' attributes (`billsQB1AbilityStart = 0x3010`). Enough longer names silently overwrite player ratings. SNES has the same shape (`SNES_TecmoTool.cs:949`, `:995`, region end `0x17b7f0`; not reproduced). CXROM's expansion-team area has a save-time check (`CXRomTSBTool.cs:295`), but the 28 base teams use the unchecked base code.
- **Reproduced (R8):** 196 longer names → 58 bytes changed in `0x3010–0x320F`, no error.
- **Fix:** before shifting, compute free space (0xFF padding before the region end) and reject the write like `Genesis_TSB1Tool.InsertPlayer` does (`Genesis_TSB1Tool.cs:815-822`). Its `CheckRosterFit`/`ResolveRosterFit` could be generalized too.

### H4 — `JUICE(ALL, n)` uses the wrong weeks
- **Where:** `Core/InputParser.cs:213-216`; `Core/TecmoTool.cs:2337-2354`
- **What:** The single-week path passes `week - 1` (0-based), but the `ALL` loop passes `i + 1` for `i = 0..16`, so weeks 1–17 zero-based: week 1 is never juiced, and on NES (17-week table) the last call writes 5 bytes past the table. `ApplyJuice` allows `week` up to 17, so it doesn't catch it. On SNES (18-week table) it only skips week 1.
- **Reproduced (R1):** after `JUICE(ALL, 17)`, bytes `0x1DF65–0x1DF69` (one week past the table) changed; week 1's bytes didn't.
- **Fix:** pass `i` (not `i + 1`) and loop over the ROM's real week count; make `ApplyJuice` reject `week >= weeksInTable`.

### H5 — `JUICE(week, 0)` crashes the import
- **Where:** `Core/TecmoTool.cs:2340-2347`; `Core/SNES_TecmoTool.cs:1815-1822`
- **What:** The check allows `amt == 0`, then `index = (amt - 1) * 5 = -5` → `IndexOutOfRangeException`. The parser's catch aborts the rest of the file. The regex also accepts amounts up to 99; the `ALL` path ignores `ApplyJuice`'s false return, so bad amounts there are silent.
- **Reproduced (R2):** `JUICE(1, 0)` then `YEAR=1999` → exception shown; the YEAR line was not applied.
- **Fix:** accept `1..17` only, and report the `ALL` path's failures.

### H6 — Invalid attribute values are silently stored as 6
- **Where:** `Core/TecmoTool.cs:1354` (`GetAbility`), `Core/SNES_TecmoTool.cs:1381`, `Core/StaticUtils.cs:242` (`GetTSBAbility`, used by TSB2/TSB3)
- **What:** The value→nibble maps return 0 (= rating 6) for anything not in the 16 valid ratings. Callers then run `IsValidAbility` on the *mapped* value, which is always valid, so the "Valid player abilities are 6, 13, 19…" error can never trigger. A typo like 55 or 101 silently becomes 6.
- **Reproduced (R3):** RB1 RS given as 55 → stored as 6, no error.
- **Fix:** map invalid values to -1 (or check the raw value against the scale) so the existing error path works. `Genesis_TSB1Tool` already does this correctly (`IsValidAbilityScore`, `Genesis_TSB1Tool.cs:1291`).

### H7 — TSB2/TSB3: one malformed player line aborts the rest of the file
- **Where:** `TSB2_TSB3/TSB2_3_Core/InputParser.cs:578` (and the other `Set*Player` methods), `Core/StaticUtils.cs:231`
- **What:** `GetInts` returns null when a number doesn't parse; `StaticUtils.GetTsbAbilities(null)` throws `NullReferenceException`, and `ProcessLines`' catch stops processing. The TSB2/TSB3 setters also throw `ArgumentException` for bad positions/teams (`CheckTSB2Args`), which takes the same abort path. The TSB1 parser handles the same case with a warning and continues.
- **Reproduced (R7):** QB1 line with `6x` as an attribute → "Operation aborted"; the next line was not applied.
- **Fix:** null-check `GetInts`'s result (warn and skip the line), and catch per line instead of per file.

### H8 — TSB2/TSB3: `LB5` player lines are silently ignored
- **Where:** `TSB2_TSB3/TSB2_3_Core/InputParser.cs:532-551`
- **What:** The position dispatch checks prefixes (`QB`, `RB`, `C`, `RE`…) and `IndexOf("LB") == 2` (for `ROLB` etc.). `LB5` has "LB" at index 0 and matches nothing, so the line passes validation and then does nothing, with no error.
- **Reproduced (R6):** LB5 name change → not applied, 0 errors.
- **Fix:** dispatch on the position name itself (a lookup/switch on `pos`) instead of line prefixes.

### H9 — Saving over a larger existing file leaves trailing bytes
- **Where:** `Core/TecmoTool.cs:295`, `Core/SNES_TecmoTool.cs:263`, `Core/StaticUtils.cs:522` (used by Genesis, TSB2/TSB3)
- **What:** `FileMode.OpenOrCreate` doesn't truncate. Saving to a path whose existing file is longer leaves the old tail on the end. The result has the wrong size, and **ROM type detection is by length**, so TSBTool can't recognize the file on the next load.
- **Reproduced (R10):** saved a 393,232-byte NES ROM over a 394,232-byte file → result 394,232 bytes.
- **Fix:** `FileMode.Create` (and `using` so the file closes on error). Better still, one shared `StaticUtils.SaveRom` used everywhere.

---

## Medium

### M1 — Team sim data `0x0F` doesn't round-trip
- **Where:** `Core/TecmoTool.cs:608`, `Core/SNES_TecmoTool.cs:599`, `Core/Genesis_TSB1Tool.cs:188`
- **What:** `teamSimData < 0xf` should be `< 0x10`: `0x0F` is formatted as one digit (`SimData=0xf`). The parser needs two hex digits, so re-importing ignores it ("No sim data" warning). With the offense preference shown, `0xf` + `2` reads as `0xf2` — a different value, silently.
- **Reproduced (R5):** set 0x0F, exported `SimData=0xf`, re-import left the old value.
- **Fix:** `string.Format("{0:x2}", teamSimData)` in all three.

### M2 — Pro Bowl lines with a bad position or team write outside the table
- **Where:** `Core/TecmoTool.cs:2686-2704` (team checked, position not), `Core/SNES_TecmoTool.cs:2465-2495` (neither), `TSB2_TSB3/TSB2_3_Core/TSB2Tool.cs:641-650` (neither)
- **What:** An unknown position (e.g. `QB` instead of `QB1`) gives index -1, so 2 bytes *before* the table are written; an unknown team writes `0xFF`/negative data. No error.
- **Reproduced (R9):** `AFC,QB,bills,QB1` → bytes `0x32851-52` changed, 0 errors.
- **Fix:** validate both and report, as `Genesis_TSB1Tool.SetProBowlPlayer` does (`Genesis_TSB1Tool.cs:1898-1912`).

### M3 — Sim values over 15 spill into the neighboring value
- **Where:** punter sim `d += data` — `Core/TecmoTool.cs:1691`, `Core/SNES_TecmoTool.cs:1928`, `Core/Genesis_TSB1Tool.cs:1626`, `TSB2_TSB3/TSB2_3_Core/TSB2Tool.cs:763`; skill sim `(data[0] << 4) + data[1]` — `Core/TecmoTool.cs:1818-1823` and the SNES copy
- **What:** Nibble-packed values aren't masked or range-checked, and the parser doesn't check ranges either. A punter sim of 16+ changes the kicker's value; a skill value of 16+ in the low slot changes the high one.
- **Fix:** range-check in the parser (0–15 for nibble values) and mask (`& 0x0F`) in the setters.

### M4 — Growing a string has no free-space check
- **Where:** `Core/TecmoTool.cs:716-741` (shifts data up to `0x20000`, can push bytes into the next bank), `Core/SNES_TecmoTool.cs:675` (`0x7330`), `Core/StaticUtils.cs:593-622` (`SetStringTableString` — used for TSB2/TSB3 player names *and* team names; writes past `firstPointer + stringTableSizeInBytes`), `Core/Genesis_TSB1Tool.cs:291` (team strings; Genesis player names *are* checked)
- **What:** Same class as H3, for team names/cities and TSB2/TSB3 player names: a longer string shifts everything after it with no check that the last byte still fits.
- **Fix:** compute remaining slack in the table and reject writes that don't fit, with a message saying how many bytes over.

### M5 — TSB2/TSB3 player sim data: extra values overwrite the next player
- **Where:** `TSB2_TSB3/TSB2_3_Core/TSB2Tool.cs:730-755`
- **What:** `SetQBSimData` / `SetSkillSimData` / `SetDefensiveSimData` write `data.Length` bytes, however many the line has. Extra values in the `[...]` go into the next player's sim data.
- **Fix:** cap at the position's sim-slot size (3 for QB/defense, 4 for skill) and warn about extras.

### M6 — "Data not applied" after an exception is misleading
- **Where:** `Core/InputParser.cs:86-99`; `TSB2_TSB3/TSB2_3_Core/InputParser.cs:93-106`
- **What:** An exception on line N shows "Operation aborted at this point. Data not applied." Lines 1..N-1 *were* already applied to the in-memory ROM, and the schedule is not applied at all. A user may save a half-applied ROM thinking nothing changed.
- **Fix:** say "Lines before #N were applied; the rest were not", or process per line so one bad line doesn't stop everything (see H5, H7).

### M7 — Converter throws on K/P lines without sim values, or malformed lines
- **Where:** `TSB2_TSB3/TSB2_3_Core/TSBConverter.cs:264-266` (`vals[0]` with `vals == null`), `:178-207` (fixed `parts[...]` indexes), `:290` (`Int32.Parse`)
- **What:** No error handling in the conversion paths; one unusual line throws out of the whole conversion.
- **Fix:** handle missing sim values (use a default like other positions do) and wrap each line in try/catch, reporting the line.

### M8 — Sim auto-updater may edit the wrong team's player
- **Where:** `Core/TecmonsterTSB1SimAutoUpdater.cs:75-97`
- **What:** `currentTeamIndex = m.Groups[1].Index`, but the regex has no capture group, so the index is 0 and the search for the player line starts at the top of the file. `nextTeamIndex` is computed but never used. If two teams have an identical player line (generated/placeholder rosters), the first one in the file is replaced.
- **Fix:** use `m.Index`, and search only between the team's start and the next team's start.

### M9 — Shared static team lists
- **Where:** `TSB2_TSB3/TSB2_3_Core/TSB2Tool.cs:113` (`public static List<string> teams`), `TSB3Tool.cs:54` (assigns a 32-entry list to it), `Core/TecmoToolFactory.cs` (replaces `TecmoTool.Teams` per tool)
- **What:** The team list is global. Creating a TSB3 tool changes the list a live TSB2 tool uses (different order, extra entries), and creating any tool changes `TecmoTool.Teams` for the others. Only matters with two tools alive at once (tests, or a future feature); the cross-ROM tests already have to work around it.
- **Fix:** make team lists per-instance.

### M10 — SNES face: skin tone and cutscene race disagree
- **Where:** `Core/SNES_TecmoTool.cs:2143-2149` vs `:2168-2183`
- **What:** `SetFace` sets skin tone light for `face < 0x53`, but `SetCutSceneRace` treats only `face == 0` as light. NES-style faces `0x01–0x52` give a light in-game skin with a dark cutscene bit. Exported SNES values are `0x00`/`0x80`, so round-trips are fine; this affects NES rosters applied to SNES.
- **Status:** possible — may be intentional; needs a look in an emulator.
- **Fix:** use the same threshold for both.

---

## Low

| ID | Where | Issue |
|---|---|---|
| L1 | `Core/SchedulerHelper2.cs:229-230` | "More than {1} games scheduled" prints the per-week limit (14) instead of the season total (224). Genesis's copy is correct. |
| L2 | `Core/SchedulerHelper2.cs:367`, `Core/Genesis_ScheduleHelper.cs:265` | A `null` team in a schedule gives index 255 → `teamGames[255]` throws (caught per line, shown as a generic "Error on line"). |
| L3 | `TSB2_TSB3/TSB2_3_Core/TSB2Tool.cs:528` | A stored name without an uppercase last name is shown as the placeholder `fakeNEWS!!!`. |
| L4 | `Core/TecmoTool.cs:385-396` | Name check only requires *some* letter (digits/punctuation pass through as raw bytes); the warning says "15 character limit" but the limit is 16. SNES `InsertPlayer` has no character check at all. |
| L5 | `Core/TecmoTool.cs:757-763` | `SetTeamName` error says `'SetTeamCity'`, and 1-character names are rejected as "empty". CXROM's overrides skip validation entirely (`CXRomTSBTool.cs:480-488`). |
| L6 | `Core/TecmoTool.cs:1552-1568` (+ SNES copy) | `SetTeamSimOffensePref` returns true even when it rejected the value. |
| L7 | `Core/TecmoTool.cs:475-478`, `:1929`, `:1952`, `:1958-2004` | Debug leftover `position = position;`; face-location lines computed then overwritten (dead code); KR/PR comments say hi/lo nibble the wrong way round. |
| L8 | `Core/InputParser.cs:25` (+ TSB2 copy) | `weekRegex` has a TAB inside `[0	-7]` (meant `[0-7]`). Harmless today (only used to detect WEEK lines). |
| L9 | `TSB2_TSB3/TSB2_3_Core/TSB2Tool.cs:940-943` | `SetYear` is a silent no-op; `YEAR=` lines are ignored without a message. |
| L10 | `Core/StaticUtils.cs:193-194` | `FindByesInFile` misses a match that ends on the last byte searched (`<` should be `<=`). |
| L11 | `Core/StaticUtils.cs:671` | TSB1 content detection requires decimal digits in `Face=0x..`; QB1 lines with hex faces (e.g. `0x5a`) don't count (only one match is needed, so usually still detected). |
| L12 | `Core/CXRomTSBTool.cs:293-315` | The "expansion names overwritten" check only asks for confirmation in the GUI; the command line saves anyway (only a warning). |
| L13 | `Core/TecmoTool.cs:2633-2667`; `Core/SNES_TecmoTool.cs:737-783`, `TSB2Tool.cs:826-877` | Uniform-usage getter/setter add 6/4 to a -1 "invalid team" location and pass the `> -1` check (writes near the ROM header; unreachable via the parser today). SNES/TSB2 team names containing a space break the city/name split on the next edit. |

---

## Suggested order

1. **H1** (one line, makes every other error visible in the command line).
2. **H2, H6, H7, H8**: stop silently losing or misplacing data from typos.
3. **H3, M4, M5**: bounds checks when data grows (reuse Genesis's pattern).
4. **H4, H5, H9, M1, M2**: small, contained fixes.
5. **M6–M10** and the Low items as time allows.

Each fix should get a test first, as for this session's schedule work. The R1–R11 reproductions in this review (a scratch program, not committed) are ready-made starting points for those tests.
