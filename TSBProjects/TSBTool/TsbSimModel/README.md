# TsbSimModel

A command-line tool for the model behind TSBTool's **TSB2 / TSB3 "Auto Update Sim"**
(`TSB2_TSB3\TSB2_3_Core\TsbSimAutoUpdater.cs`). Auto Update Sim works out every player's *sim values* —
the hidden bytes the game uses when it simulates a game instead of playing it — from his ratings.
The formulas are fitted to Tecmo's own sim values in the stock ROMs. This tool fits them, exports them to
TSBTool, writes the tests' expected output, and has the tools used to check a model with simmed seasons.

TSBTool doesn't need this tool to run: the fitted numbers are compiled into `TsbSimAutoUpdater.cs`.
It's needed only to refit or change the model, or to regenerate the test files.

## Build

`TsbSimModel.csproj` (.NET Framework 4.8, x86) references `..\TSBTool.csproj`: it reads ROMs and battery saves
with TSBTool's own code.

```
MSBuild TsbSimModel\TsbSimModel.csproj
TsbSimModel\bin\Debug\TsbSimModel.exe            (prints the usage)
```

Its own tests (`..\TsbSimModel.Tests`, .NET 4.8 MSTest; e.g. the engine's share chances, `ShareChanceTests`):

```
MSBuild TsbSimModel.Tests\TsbSimModel.Tests.csproj
vstest.console TsbSimModel.Tests\bin\Debug\TsbSimModel.Tests.dll
```

## Commands

| Command | What it does |
|---|---|
| `fit <TSB2\|TSB3> <stock.smc> <outDir>` | Fits the model to the stock ROM. Writes `<game>_model.json`, `<game>_report.txt` (accuracy on data the model wasn't fitted on) and `<game>_stock_rosters.txt`. |
| `fitteam TSB2 <stock.smc> <model.json>` | Fits only the team SimData model (TSB2's rush D / pass D) into an existing model file, leaving its player models as they are, and replaces the TEAM SIMDATA part of the `_report.txt` next to it. |
| `fitfav TSB3 <stock.smc> <model.json>` | Fits only TSB3's free agent value model (Face's last digit) into an existing model file, and replaces the FREE AGENT VALUE part of the `_report.txt` next to it. |
| `constants <tsb2_model.json> <tsb3_model.json> [--patch <TsbSimAutoUpdater.cs>]` | Writes TSBTool's model constants; `--patch` replaces the block between `// ---- generated` and `// ---- end of generated constants ----` in place. |
| `expected <TSB2\|TSB3> <stock.smc> <model.json> <outDir>` | Writes the test files `<game>_stock_rosters.txt` and `<game>_stock_rosters_autoupdated.txt` (`TSBTool_Test\TSB2SimModel`, held to by `TsbSimAutoUpdaterTests`), and for TSB2 `tsb2_stock_team_simdata.txt` (the expected team SimData, `TsbTeamSimDataTests`). |
| `nodice <TSB2\|TSB3> <season> <rom>...` | What each ROM's sim bytes do for a season, averaged over the teams, with no dice: each slot group's chance of a carry, target, sack or INT; scramble chance; top tackler's share; ... |
| `compare <season.srm>...` | League shapes of simmed TSB2/TSB3 seasons (Mesen2 / snes9x battery saves), side by side: completion %, yards per carry, who gets the carries / catches / sacks / INTs, top tackler, FG %, INTs and rush yards the game didn't credit, ... |
| `apply <TSB2\|TSB3> <rom> <out.smc> [--seasons 1,2,3] [--fix-sim-bugs]` | A test ROM: TSBTool's Auto Update Sim on the seasons' rosters (player sim bytes and TSB2's team SimData), optionally the `FIX_SIM_BUGS` fix, SNES header checksum fixed. |

The current models and their reports are in `Models\` (`tsb2_model.json`, `tsb2_report.txt`, `tsb3_model.json`,
`tsb3_report.txt`), fitted from `TSBTool_Test\ROMs\TSB2_U.smc` and `TSB3.smc`.

## Changing the model

1. `fit` each game into a scratch folder; read the reports (held-out accuracy, league shapes).
2. When it's better, copy the model files into `Models\` and run
   `constants Models\tsb2_model.json Models\tsb3_model.json --patch ..\TSB2_TSB3\TSB2_3_Core\TsbSimAutoUpdater.cs`.
3. `expected` for each game into `..\TSBTool_Test\TSB2SimModel`, then run the tests (TSBTool_Test and
   TsbSimModel.Tests).
4. Check it in the games: `apply` a stock ROM, sim a full season in an emulator (new season, sim every
   game, close the game so the battery save is written), and `compare` the save with stock seasons' saves.
   One season has a lot of luck in it: `nodice` shows what the bytes do, so a difference in `compare` that
   `nodice` doesn't show is luck. Stock seasons vary from each other by about as much as the model should.

## What the model knows about the games

From tracing the sim engine (TSB2 bank `$84`; TSB3 is the same code with its own tables):

* **Shares** (carry, target, sack, INT; out of 256): the engine rolls R = 0..255 and walks the pool,
  subtracting each player's share; the player R falls under gets it. Past everyone, it falls back:
  carries to RB1, targets to RB3..TE2 at random, sacks to RE..RILB at random, INTs to NT2..RCB at random.
  The INT walk goes in reverse roster order (DB3 first). So a team's shares should add up to 255.
* **255 (FF) means "injured / absent"**: a player with a share of FF is skipped. Shares stop at 254.
* **QB2's carry share stays 0**: the carrier pick walks both QBs, and a carry that lands on the QB who isn't
  playing is credited to nobody (bug "G3"). Tecmo's QB2s have 0.
* **0-15 values** go through the game's lookup tables (rush, yards after catch, pass/catch, kick, punt,
  tackle). TSB3's tables differ: lower rush values, other pass/catch and YAC values, and the **scramble digit
  the other way round** (TSB2: 0 = most, 20%; TSB3: 0 = least, 3%).
* **Team defense**: TSB2's team byte (rush D, pass D) — the model's team part (below). TSB3 doesn't use a
  team byte; it works out team defense from the defenders plus a per-team bonus (TSBTool's TSB3 team
  SimData), which isn't modelled.
* **TSB3's free agents and all-time team** aren't teams: Tecmo gave them flat placeholder shares. Auto
  Update Sim works out their own values but keeps their shares.

## How the model works

Ratings are used as their 1..16 *steps* (TSB2/TSB3 ratings are 6.25 apart: 6 → 1 .. 100 → 16).

**0-15 values** (QB pass, QB rush, QB scramble, rush, YAC, catch, tackle, K, P):
`value = round(center + stretch × (Σ coef × rating step + constant − center))`, clamped to 0..15.

* The coefficients come from least squares (Householder QR) of Tecmo's value on the rating steps plus a
  constant: one for everyone (QB, K, P), or for the RB/WR/TE and defenders one per position group or per roster
  slot (RB1, RB2, …) — whichever is more accurate on held-out data. Per slot usually wins: Tecmo's starters
  have higher values than their backups with the same ratings.
* A straight-line fit pulls everyone toward the average. `stretch` spreads the values like Tecmo's (their
  standard deviation over the fitted values'); it's kept only when it costs at most 3 points of "within 1"
  accuracy. Tackle weights always get it, tuned further (1.0 to 2.0 times) until each team's top tackler gets
  Tecmo's share of the team's weights — they only count relative to each other.

**Shares**: `weight = (e^(Σ coef × rating step + slot constant) − 1)^spread × group factor`, then each team's
weights are scaled to whole shares adding up to 255 (largest remainders first, none over 254).

* The coefficients come from least squares of `log(Tecmo's share + 1)` on the rating steps plus a constant
  per slot.
* `spread` and one factor per position group (QB/RB/WR/TE for carries; RB/WR/TE for targets; DL/LB/DB for
  sacks and INTs; one group fixed at 1) are then calibrated with Nelder-Mead so that the chances the engine
  gives each player (its walk, above) match what Tecmo's own bytes give. The loss adds up three things:
  how many plays go to a different player, team by team; the league-wide difference per slot; and how far
  the top player's average chance is from Tecmo's.

**Free agent value (TSB3)**: Face's last digit, 0-15 (byte 3 of the ratings record; the first digit is the skin
tone). TSB3's team defense adds it up over the starting front seven (rush) and the front seven + 4 DBs (pass);
a simmed season confirmed it. Tecmo set it by hand (stars far above their ratings; the game's Superstar Editor
makes it for created players as a plain average of some ratings, far lower). The model: least squares on the
rating steps per position group (QB, RB, WR, TE, OL, DL, LB, CB, S, K, P; no negative weight), all of the line's
ratings or the old FAP formula's, whichever is closer held out (thirds of teams). Held out it's within 1 of
Tecmo's value 72% of the time (the old `FAP_Formulas.txt`: 46%, and it ran high -- team defense levels
10.3 / 9.4 against Tecmo's 7.6 / 5.2). Auto Update Sim and the TSB2 -> TSB3 converter use it.

**Team SimData (TSB2)**: each starter (RE NT LE, the 4 LBs, RCB LCB FS SS) is scored with Tecmonster's rating
mix (`TecmonsterTeamSim`: run — DL mostly HP, LB/DB RS RP MS HP; pass — DL mostly HP, LB/DB PI QU heavy); the
DL, LB and DB averages are weighted — least squares with no negative weight, or one shared weight, whichever is
closer held out — plus a constant, then stretched to Tecmo's league spread. Tecmo set the team byte mostly from
each team's real season: held out it's ~2.5 digits from Tecmo's (correlation 0.53 rush, 0.31 pass; giving every
team the average is 2.7 / 2.55 off). A weight per rating overfit (a DL's PI came out as rush D's biggest weight),
so only the group weights are fitted. The player forms' Auto Update (`AutoUpdateSimData`) uses it; the converter
(`AutoUpdatePlayerSimData`) doesn't, keeping a roster's own team values.

**Held-out check**: TSB2 fits on two of its three stock seasons and checks on the third, for each season;
TSB3 (one stock season) does the same with 3 folds of 10 teams. The final model is fitted on everything.
The report shows the held-out accuracy ("within 1" of Tecmo's value; the share of plays going to a
different player than with Tecmo's bytes; league shapes). Tecmo set many values from each year's real
stats, so some can't be predicted from ratings: the same player with the same ratings often has
different values in two seasons.

## History

Fitted 2026-10-09 during the TSB2/TSB3 sim study (traces, sim test seasons and results:
`TSBTool_Test\SimTests\TSB2_study`, `TSB3_study`, not in git). This tool is a C# port of that study's scripts and
reproduces their models exactly (all 370 numbers) and their test files.
