# TSBTool Test Coverage Effort

## Purpose

TSBTool had no working automated tests — the legacy `TEST\*.bat` scripts (which drove the CLI and
diffed output with a visual diff tool by hand) had been silently broken since a 2019-11-27 CLI
regression. This document tracks the effort to give the tool real, automated regression coverage,
starting with the work to support the 18-week/17-game schedule ROM and expanding from there.

**Scope**: core features reachable through the text-processing command language
(`InputParser`/`Core\InputParser.cs`), which both the GUI and the CLI (`MainClass.RunMain`) route
every data change through. GUI-only code (forms, dialogs) and Godot/Bridge.net-related code are out
of scope — see "Explicitly out of scope" below.

**Test project**: `TSBTool_Test` (legacy MSTest v1 — `Microsoft.VisualStudio.QualityTools.UnitTestFramework`,
matching this project's original VS2008 era). Run with deployment disabled
(`TSBTool_Test\NoDeploy.testsettings`) — this environment's legacy `MSTest.exe` runner doesn't honor
`[DeploymentItem]`, so without it, fixture files (ROMs, schedules, expected-output text) won't be
found. Select it via **Test > Test Settings > Select Test Settings File** in the IDE, or
`MSTest.exe /testsettings:NoDeploy.testsettings /testcontainer:...` on the command line.

**Fixture layout**:
- `TSBTool_Test\ROMs\` — ROM files (`.nes`/`.smc`) for every tool implementation under test.
- `TSBTool_Test\Schedules\` — schedule-only text fixtures (`_Input.txt` applied, `_Expected.txt` golden).
- `TSBTool_Test\FullRomDumps\` — full roster/team text fixtures, same `_Input.txt`/`_Expected.txt` pairing.

**Established pattern**: golden-fixture round trip. Apply an `_Input.txt` fixture to a fresh copy of
a ROM (via `MainClass.RunMain()` for full dumps, or a `ScheduleHelper2`/`CXRomScheduleHelper`
subclass directly for schedule-only tests), read the result back, and compare against an
`_Expected.txt` fixture that was captured from the tool's own current, verified-idempotent output
(not blindly copied from old historical fixtures — those often differ from current output in
benign, already-understood ways: reworded comments, whitespace formatting, corrected name
capitalization, etc.). See `FullRomDumpTests.cs`'s class doc-comment for a worked example of that
verification process.

## Completed

- [x] `ROM_TYPE.CXROM_18WEEK` detection (schedule-content fingerprint, since it shares stock
      `CXROM_v105`'s exact file length) and `CXRom18WeekScheduleHelper`
      — `RomTypeDetectionTests.cs`, `ScheduleTests.cs`
- [x] Stock TSB1 (28-team NES) schedule edge cases: full/minimal schedules, invalid-team and
      per-week overflow rejection, too-many-weeks rejection — `StockTsb1ScheduleTests.cs`
- [x] Full roster/team dump round trip across all three tool implementations that were reachable:
      stock NES (`TSPRBOWL.nes`), SNES TSB1 (`TSB1.smc`), CXROM_v111 (`TSB 2007-32-111.nes`)
      — `FullRomDumpTests.cs` (ports `TEST\Test1-9.txt`)
- [x] `MainClass.cs` CLI bugs found and fixed along the way (all predate this effort):
      - 2019-11-27 regression making `PrintStuff` unreachable (dispatch order + missing
        "no options → print everything" default)
      - `PrintStuff` not treating `-pb`/`-of`/`-colors` as implying a full player dump
      - Stale static fields (`players`, `gui`, `stdin`, `printHelp`) leaking across repeated
        `RunMain()` calls in one process
      - 2013-era `-out:` bug where the extension-based default silently overwrote any explicit
        `-out:` value
      - `MainClass` needed to be `public` (was implicitly `internal`) for the test assembly to
        call `RunMain` directly

- [x] `SET(0xADDR, 0xVAL)` — raw byte poke, across all 7 supported ROM types (stock NES, SNES TSB1,
      CXROM_v105, CXROM_v111, CXROM_18WEEK, SNES TSB2, SNES TSB3) — `SetCommandTests.cs`. Verifies
      byte-level precision directly (raw `OutputRom` diff against the original ROM), not just text
      output: applying one `SET` changes exactly the one byte named, to exactly the value given, and
      nothing else in the ROM. TSB2/TSB3 (`TSB2Tool`/`TSB3Tool`) implement `ITecmoContent`
      independently rather than extending `TecmoTool`, but parse the identical `SET(...)` regex and
      route through the same `StaticUtils.ApplySimpleSet` → `SetByte` path, so they're covered here
      too even though their own full round-trip coverage is still open below.

- [x] `ReplaceString("find", "replace"[, occurrence])` — added 2019-08-13 (`257efd0`) —
      `ReplaceStringCommandTests.cs`. `StaticUtils.ReplaceStringInRom` is a single static function
      with no per-ROM-type branching (operates directly on `tool.OutputRom`), so one representative
      ROM (`TSPRBOWL.nes`) is enough, unlike `SET`. Covers: same-length replace (byte-precision diff,
      same technique as `SetCommandTests`), shorter replacement space-padding, longer-replacement
      rejection with no mutation, and the 1-based occurrence-index parameter (replacing only the Nth
      match, leaving the others untouched).

- [x] `TEAM_ABB=...,TEAM_CITY=...,TEAM_NAME=...` — team string-table replacement, same commit —
      `TeamStringsCommandTests.cs`. This writes into a variable-width, pointer-indexed string table
      (`SetTeamStringTableString`): a length change shifts every later string and rewrites their
      pointers, so the tests check both round-trip correctness and that editing one team's strings
      never corrupts any other team's. Covers three independently-implemented versions: stock
      `TecmoTool` (28 teams, 3 separate fixed-index entries per team), `CXRomTSBTool` (34-team table,
      different offsets), and `SNES_TecmoTool` (a single combined `"ABB*City Name*"` entry per team).
      **Found and fixed a real bug**: `SNES_TecmoTool.SetTeamStringTableString` passed
      `AdjustDataPointers` an exclusive "one past the end" table boundary, but `AdjustDataPointers`
      treats its `lastPointerLocation` parameter as *inclusive* (confirmed from its other caller,
      which points directly at a real pointer address). The off-by-one made it read team index 0's
      *string data* (which sits immediately after the pointer table) as a phantom extra pointer and
      corrupt its first two bytes, on every edit that changed any other team's combined string length.
      Fixed at the call site (`Core\SNES_TecmoTool.cs`) by passing the correct inclusive boundary
      instead of changing the shared `AdjustDataPointers` method, since its other caller relies on
      the existing (correct, for that case) inclusive-boundary behavior.

- [x] `COLORS Uniform1=`/`Uniform2=`/`DivChamp=`/`ConfChamp=`/`UniformUsage=` —
      `UniformColorsCommandTests.cs`. Fixed-position, per-team-indexed writes (no shifting), covering
      round trip, isolation from other teams, and the "action sequence" mirror location that
      `SetHomeUniform`/`SetAwayUniform`/`SetUniformUsage` also write but which has no public getter
      (verified via raw ROM bytes at the known offset, since round-trip alone can't see it). Tested
      against stock `TecmoTool` and both a regular and an expansion team on `CXRomTSBTool` (which
      only overrides the location functions, not the read/write logic, so both code paths matter).
      TSB2/TSB3 still don't parse the command at all — `TSB2_TSB3\TSB2_3_Core\InputParser.cs`'s entire
      `else if (line.StartsWith("COLORS"))` block is commented out, and `TSB2Tool`'s own (separate)
      `ITecmoTool` interface has `SetHomeUniform`/`GetGameUniform`/`SetDivChampColors` etc. commented
      out too — a `COLORS` line in a TSB2/TSB3 input file is simply unrecognized text.

      **`SNES_TecmoTool` (SNES TSB1) is no longer a no-op stub here** — a later session (see
      `Genesis_TSB1_plan.md`'s "Uniform colors" section for the full story) implemented real
      `SetHomeUniform`/`GetHomeUniform`/`SetAwayUniform`/`GetAwayUniform` (pants + jersey + helmet,
      28-hex-digit SNES-format colors, ordered pants1,jersey2,pants2,jersey3,pants3,helmetDark,
      helmetMedium — jersey's dark shade is deliberately excluded, see below) and real
      `SetUniformUsage`/`GetUniformUsage` (the uniform-matchup table at
      `x1752`+, 4 bytes/32 bits per team, one bit per opponent — which of the two uniforms gets worn
      against each opponent, keyed by opponent rather than by home/away game location). Both confirmed
      against real data — jersey/pants/helmet working end-to-end in a real emulator (including a home
      game vs. an away game showing genuinely independent helmet colors), the matchup table verified
      against the Bills' real 1993 schedule (they wear "Light" exactly once, vs. the Giants).
      `GetChampColors` remains an empty stub (SNES's Division/Conference Championship screens were
      confirmed in-game to reuse the plain jersey/pants data directly, no separate table needed — see
      the plan doc). Covered by dedicated `StockSnesRom_*` tests in the same file.

      There is no longer a separate `Helmet=` keyword or `SetHelmetColor`/`GetHelmetColor` pair —
      helmet color went through two designs before landing here. First it was a standalone field
      mirrored into all four of a team's uniform blocks (since the stock ROM's helmet value happened
      to match across all four). Then, prompted by the question of whether Home and Away could have
      genuinely different helmets (like modern "color rush" uniforms), that assumption was tested
      directly against the ROM — confirmed independent in a real emulator — and helmet was folded into
      `Uniform1`/`Uniform2` instead (growing them from 24 to 32 hex digits), so each side owns its own
      helmet value with no shared state to drift out of sync. `-colorsdebug` (see below) was updated to
      show `Helmet1 (Home)`/`Helmet2 (Away)` as separate entries accordingly.

      **Jersey's dark shade was later dropped from the format entirely** — pixel analysis of real
      in-game sprite screenshots found it isn't a fabric-shading tint like the other five jersey/pants
      shades, but the outline/border color for jersey numbers (confirmed directly: a Home sprite's
      jersey-number "1" is outlined in exactly the Home jersey-dark stock value). `SetHomeUniform`/
      `SetAwayUniform` simply never write that byte anymore, leaving each team's stock number-outline
      color untouched rather than overwriting it with something that wasn't meant to control it.
      `-colorsdebug` still surfaces it read-only, relabeled "number outline -- not editable".

      An even earlier pass at helmet color targeted a different thing entirely — War6's guide's "Large
      Helmet" system (the icon on the Team Menu/Team Matchup screens), which needed a real 65816 code
      patch and still didn't visibly work on the screens that mattered. That whole investigation
      (address table, code patch) was removed once the actual in-game location was found by inspecting
      the full uniform block instead of only the documented jersey/pants sub-ranges — Large/Mini Helmet
      editing was an explicit scope decision to drop (too complex for this tool; belongs in a different
      program or a later pass). The `-colorsdebug` CLI flag (`SNES_TecmoTool.GetColorsDebugInfo`/
      `PrintColorsDebugInfo`) was removed along with that investigation and then reintroduced, rescoped
      to only the real, implemented locations (jersey/pants home+away, helmet home+away) — no
      shell/intro/CP-index noise left over from the abandoned system.

- [x] TSB II round trip — `Tsb2RoundTripTests.cs`. `TSB2Tool` is an entirely independent
      implementation (not extending `TecmoTool`) with its own `InputParser`. Rather than hand-author
      an input fixture, this dumps the ROM's own actual data and re-applies it (like
      `ScheduleTests.EighteenWeekRom_ApplyScheduleRoundTrip_IsIdempotent`), which exercises the real
      data set without constructing one — with one wrinkle: the raw ROM predates the 2019-08-14
      name-capitalization fix, so the *first* apply isn't a no-op (e.g. "DEL GRECO" → "del GRECO");
      the test applies once to normalize, then checks that a second apply truly is idempotent.
      Explicitly guards against both historical bugs fixed in `e46c8de` (2021-02-13): asserts
      `GetAll(season)` (the overload the CLI/InputParser pipeline actually calls) contains no `WEEK`
      lines while `GetSchedule(season)` still does (schedule-shows-2x), and that the 7 TSB2/3-only
      positions beyond the base 28 (`RE2`, `NT2`, `LE2`, `LB5`, `DB1`, `DB2`, `DB3`) actually appear in
      the roster dump (wrong-`positionNames` bug).

- [x] TSB III round trip — `Tsb3RoundTripTests.cs`, mirroring `Tsb2RoundTripTests.cs` against
      `TSB3.smc`. `TSB3Tool : TSB2Tool` overrides `GetAll(int)`/`GetSchedule(int)`/`ApplySchedule`
      with TSB3-specific logic (different attribute layout, its own `SNES_TSB3_ScheduleHelper`), so
      TSB2's coverage doesn't automatically extend here — confirmed clean: `GetAll(int season)`
      delegates to `base.GetAll(1)` (TSB2Tool's already-fixed version), so no schedule-doubling.
      `TSB3Tool` doesn't override `positionNames`, so it shares TSB2's extended list — same check.

- [x] TSB1↔TSB2 Player Converter — `PlayerConverterTests.cs`. `TSBConverter.cs` already had its own
      hand-written self-check methods (`Test*TSB2Conversion`/`Test*TSB1Conversion`, one per position
      group, each with real player data and hand-verified expected output), but nothing anywhere
      called them — they'd never actually been run. Wired all 16 up as `[TestMethod]`s rather than
      inventing new test data for this nibble-packed conversion logic, since the author's own cases
      are much stronger ground truth than anything reconstructed after the fact.
      **Found and fixed a real bug**: `TSB2Converter.AddSimValues` (`TSBConverter.cs`), the defensive-
      position branch (18 positions: RE/NT/LE/RE2/NT2/LE2/ROLB/RILB/LILB/LOLB/LB5/RCB/LCB/DB1/DB2/FS/
      SS/DB3), checked `if (simVals.Length > 1)` to decide whether to compute real sim values — but
      `simVals` was the *output* string, still `""` at that point, so the check was always false and
      every defensive player converted from TSB1→TSB2 silently got the hardcoded placeholder
      `[10,10,10]` regardless of actual stats. Fixed to check `vals` (the actual parsed sim-value
      array) instead, matching what `GetSimVals` returning `null` for a missing bracket was clearly
      meant to guard against.

## Planned

Reviewed every non-Godot/Bridge.net commit since 2013 and cross-referenced against what's actually
exercised by the fixtures above. Nothing left outstanding from that original review.

- [x] Reviewed `18Week_RealNFL_Hack_Documentation-1.md` (the ROM hack author's own technical writeup,
      dropped into `TSBTool_Test\`). Independently confirms every address we reverse-engineered for
      the 18-week ROM (pointer table `0x329A7`, games-per-week array `0x329CB`, schedule
      `0x32D36`/544 bytes/272 games). Most of the document describes 6502 game-*engine* patches
      (division-clinch math, playoff-boundary checks, menu/rendering code) that only run when the ROM
      is played — out of scope, since TSBTool never reads or writes that logic.
      **Found and fixed a real bug**: the doc mentions a `0xFF` terminator byte at `0x329DD`,
      immediately after the games-per-week array — confirmed present in the fixture ROM. Tracing that
      exposed `ScheduleHelper2.ClosePrevWeek()` (`Core\SchedulerHelper2.cs`) had no bounds check on
      `week`, unlike its sibling `SetupPointerForCurrentWeek()` which does — so a schedule text with
      more weeks than the ROM supports (e.g. a stray `WEEK 19`) would silently overwrite that sentinel
      via `ApplySchedule`'s unconditional final `ClosePrevWeek()` call. This is in the shared base
      class, so it affected every ROM type, not just this one; confirmed via test that `0x329DD` went
      `0xFF` → `0x00` before the fix. Fixed by adding the same `week < totalWeeks` guard already
      present in `SetupPointerForCurrentWeek()`. Regression test:
      `ScheduleTests.EighteenWeekRom_SchedulingANineteenthWeek_DoesNotCorruptGamesPerWeekTerminator`.

## Explicitly out of scope

- `JUICE(team, amount)` — per explicit direction, skipped for now
- `PatchMaker.cs` — a WinForms `Form`, GUI-only
- Schedule GUI / Pro Bowl edit GUI (`AllStarForm`) / Attribute edit GUI — GUI-only
- Tecmonster's sim-formula auto-updater, except as incidentally covered via the TSB1↔TSB2 converter
  above (its other callers are all GUI forms: `AttributeForm`, `ModifyTeamForm`, TSB2/TSB3 forms)
- Godot / Bridge.net related code and commits
