<#
  Generate-SimTests.ps1
  Builds the sim-engine test ROMs described in TSBTool\TSB_Simulation.md ("Open questions"),
  plus one recording sheet (.txt) per ROM.

  Every ROM starts from a NEUTRAL baseline (all teams get identical sim values), then changes ONE
  thing across four groups of 7 teams. Group = team's position in the roster order modulo 4, so every
  division has teams from all four groups.

  Usage (from any folder):
    powershell -ExecutionPolicy Bypass -File Generate-SimTests.ps1 -Exe <path to TSBToolSupreme.exe>
    Add -Only NES_09_DefenderWeights (comma-separated names) to rebuild just some tests; every
    rebuilt test's results sheet is overwritten, so don't rebuild one that has results in it.
#>
param(
    [Parameter(Mandatory = $true)][string]$Exe,
    # Build only these tests (by name). Rebuilding a test overwrites its results sheet.
    [string[]]$Only
)
$ErrorActionPreference = 'Stop'
$here    = Split-Path -Parent $MyInvocation.MyCommand.Path
$romsDir = Join-Path $here '..\ROMs' | Resolve-Path
$stock = @{
    NES     = Join-Path $romsDir 'TSPRBOWL.nes'
    SNES    = Join-Path $romsDir 'TSB1.smc'
    GENESIS = Join-Path $romsDir 'Genesis\Tecmo Super Bowl (USA) (October 1993).bin'
}
$ext = @{ NES = '.nes'; SNES = '.smc'; GENESIS = '.bin' }

# The only bytes a test ROM may differ from stock in: the 28 x 0x30 sim blocks and the 28-byte
# run/pass preference table. TSBTool writes the whole roster back, and a stock dump does not
# round-trip perfectly (two-word last names like "DE BERG" come back as "de BERG"; Genesis SetFace
# clears bit 0x20 of the player's +2 byte), so each test ROM = stock + just these ranges.
$simRanges = @{
    NES     = @( @(0x18163, 0x540), @(0x27526, 28) )
    SNES    = @( @(0x26ACF, 0x540), @(0x15DFA, 28) )
    GENESIS = @( @(0xDEA86, 0x540), @(0xB58E, 28) )
}

$qbPos    = 'QB1','QB2'
$skillPos = 'RB1','RB2','RB3','RB4','WR1','WR2','WR3','WR4','TE1','TE2'
$defPos   = 'RE','NT','LE','LOLB','LILB','RILB','ROLB','RCB','LCB','FS','SS'
$groupNames = 'A','B','C','D'

# ---------------------------------------------------------------------------------------------
# Neutral baseline
# ---------------------------------------------------------------------------------------------
function Get-BaselineSims([string]$pos) {
    if ($qbPos -contains $pos)    { return ,@(4, 8, 3) }        # rush 4, pass 8, pocket 3
    if ($skillPos -contains $pos) { return ,@(8, 8, 8, 8) }     # rush, catch, yards/catch, targets
    if ($defPos -contains $pos)   { return ,@(30, 30) }         # pass rush, coverage
    if ($pos -eq 'K' -or $pos -eq 'P') { return ,@(8) }
    return $null
}
$baselineTeam = @{ Hi = 8; Lo = 8; Pref = 0 }

# ---------------------------------------------------------------------------------------------
# Test definitions
#   Team   : scriptblock(group) -> hashtable overrides for Hi/Lo/Pref
#   Player : scriptblock(group, pos, sims) -> modified sims array
#   Value  : scriptblock(group) -> text describing the group's setting (for the sheet)
# ---------------------------------------------------------------------------------------------
$tests = @(
  [pscustomobject]@{
    Name='NES_01_RunOffense'; Platform='NES'
    Question='How much does a ball carrier''s Sim rush change rushing yards? (Open question 1: run yardage)'
    Changed='All RB/WR/TE Sim rush = 0 / 5 / 10 / 15 by group. All teams run/pass preference 1 (60% run) for more carries.'
    Team={ param($g) @{ Pref = 1 } }
    Player={ param($g,$pos,$s) if ($skillPos -contains $pos) { $s[0] = @(0,5,10,15)[$g] }; $s }
    Value={ param($g) 'Sim rush ' + @(0,5,10,15)[$g] }
    Record=@('Rush att','Rush yds','Yds/att','Points for')
    Expect=@(
      'Yards per carry should rise steadily from group A (0) to group D (15).',
      'If yards/carry barely changes, Sim rush mostly decides WHO carries, not how far.',
      'Rush attempts should be roughly equal across groups (same preference for everyone).')
  }
  [pscustomobject]@{
    Name='NES_02_QBPass'; Platform='NES'
    Question='How much does the QB''s Sim pass change completions, yards, sacks and interceptions? (Open questions 1 and 4)'
    Changed='QB1/QB2 Sim pass = 0 / 5 / 10 / 15 by group. All teams preference 3 (70% pass).'
    Team={ param($g) @{ Pref = 3 } }
    Player={ param($g,$pos,$s) if ($qbPos -contains $pos) { $s[1] = @(0,5,10,15)[$g] }; $s }
    Value={ param($g) 'QB Sim pass ' + @(0,5,10,15)[$g] }
    Record=@('Pass att','Comp','Pass yds','TD','INT thrown','Sacked')
    Expect=@(
      'Completion % should rise from group A to D.',
      'If INTs thrown or times sacked also change with Sim pass, the QB value feeds into whether those happen.',
      'If sacks/INTs are flat across groups, they are decided by something else (see NES_05 / NES_07).')
  }
  [pscustomobject]@{
    Name='NES_03_ReceiverCatch'; Platform='NES'
    Question='How much does a receiver''s Sim catch change completions? (Open question 1: pass outcomes)'
    Changed='All RB/WR/TE Sim catch = 0 / 5 / 10 / 15 by group. All teams preference 3 (70% pass).'
    Team={ param($g) @{ Pref = 3 } }
    Player={ param($g,$pos,$s) if ($skillPos -contains $pos) { $s[1] = @(0,5,10,15)[$g] }; $s }
    Value={ param($g) 'Sim catch ' + @(0,5,10,15)[$g] }
    Record=@('Pass att','Comp','Pass yds','INT thrown')
    Expect=@(
      'Completion % should rise from group A to D.',
      'Compare with NES_02: which of QB Sim pass / receiver Sim catch moves completions more?')
  }
  [pscustomobject]@{
    Name='NES_04_ReceiverYPC'; Platform='NES'
    Question='Does a receiver''s 3rd sim value (Sim yards per catch, formerly labeled "punt ret") control yards after the catch? (Open question 1)'
    Changed='All RB/WR/TE Sim yards per catch = 0 / 5 / 10 / 15 by group. All teams preference 3 (70% pass).'
    Team={ param($g) @{ Pref = 3 } }
    Player={ param($g,$pos,$s) if ($skillPos -contains $pos) { $s[2] = @(0,5,10,15)[$g] }; $s }
    Value={ param($g) 'Sim yards/catch ' + @(0,5,10,15)[$g] }
    Record=@('Comp','Pass yds','Yds/comp','PR yds')
    Expect=@(
      'If the value is yards per catch: yards per completion rises A -> D, completions stay about the same.',
      'If it were punt returns (old label): PR yards would change instead and yds/comp would stay flat.')
  }
  [pscustomobject]@{
    Name='NES_05_TeamDefense'; Platform='NES'
    Question='Do the team SimData digits control rush defense (high) and pass defense (low)? Do they affect sacks and interceptions? (Open question 4)'
    Changed='Team SimData: A = 0x00 (rush 0, pass 0), B = 0xF0 (rush 15, pass 0), C = 0x0F (rush 0, pass 15), D = 0xFF. Preference 0 for all.'
    Team={ param($g) @( @{Hi=0;Lo=0}, @{Hi=15;Lo=0}, @{Hi=0;Lo=15}, @{Hi=15;Lo=15} )[$g] }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) @('SimData 0x00','SimData 0xF0','SimData 0x0F','SimData 0xFF')[$g] }
    Record=@('Rush yds allowed','Pass yds allowed','Pts allowed','Sacks made','INTs made','KR yds allowed')
    Expect=@(
      'Rush yds allowed: low for B and D, high for A and C.',
      'Pass yds allowed: low for C and D, high for A and B.',
      'If sacks/INTs made follow the low digit (C, D high), pass defense decides whether they happen.',
      'KR yds allowed low for B and D would confirm the high digit also acts as kick coverage.')
  }
  [pscustomobject]@{
    Name='NES_06_RunPassPref'; Platform='NES'
    Question='Are the run/pass preference splits really 50/50, 60/40 run, 60/40 pass, 70/30 pass? (Documented from code; needs confirmation)'
    Changed='Run/pass preference = 0 / 1 / 2 / 3 by group.'
    Team={ param($g) @{ Pref = $g } }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'Preference ' + $g }
    Record=@('Rush att','Pass att','Pass %','Sacked')
    Expect=@(
      'Expected pass share of plays: A 50%, B 40%, C 60%, D 70% (late-game situations shift these a bit).',
      'Count QB scrambles and sacks as passes when computing pass % if you can tell them apart.')
  }
  [pscustomobject]@{
    Name='NES_07_Scramble'; Platform='NES'
    Question='How often does the QB scramble, and does the pocket value control it? (Open question 2)'
    Changed='QB1/QB2 Sim pocket = 0 / 1 / 2 / 3 by group, QB Sim rush 15 (so scrambles show up as QB rushing). All teams preference 3 (70% pass).'
    Team={ param($g) @{ Pref = 3 } }
    Player={ param($g,$pos,$s) if ($qbPos -contains $pos) { $s[0] = 15; $s[2] = $g }; $s }
    Value={ param($g) 'Pocket ' + $g }
    Record=@('QB rush att','QB rush yds','Pass att','Sacked')
    Expect=@(
      'Code as read (lookup then AND #3): chance per pass play = pocket 0: 3/256, 1: 1/256, 2: 1/256, 3: 0.',
      '  -> with ~400 pass plays per team-season: A ~4-5 QB runs, B and C ~1-2, D 0.',
      'If instead the table is meant as-is (51/25/13/8 out of 256): A ~80, B ~40, C ~20, D ~12 QB runs per season.',
      'Group D (pocket 3) QB runs, if any, come from designed runs, not scrambles.')
  }
  [pscustomobject]@{
    Name='NES_08_Returners'; Platform='NES'
    Question='Which sim value do kick and punt returners use? (Open question 3)'
    Changed='All RB/WR/TE (Sim rush, Sim yards per catch): A = (0, 0), B = (15, 0), C = (0, 15), D = (15, 15). Every skill player on a team is identical, so it does not matter which one is the returner.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) if ($skillPos -contains $pos) { $s[0] = @(0,15,0,15)[$g]; $s[2] = @(0,0,15,15)[$g] }; $s }
    Value={ param($g) @('rush 0 / ypc 0','rush 15 / ypc 0','rush 0 / ypc 15','rush 15 / ypc 15')[$g] }
    Record=@('KR att','KR yds','KR avg','PR att','PR yds','PR avg')
    Expect=@(
      'If returners use Sim rush: B and D return well, A and C poorly.',
      'If returners use Sim yards per catch: C and D return well, A and B poorly.',
      'If all four groups are the same, the returner value comes from somewhere else (e.g. another player''s byte).')
  }
  [pscustomobject]@{
    Name='NES_09_DefenderWeights'; Platform='NES'
    Question='Do defenders'' Sim pass rush / Sim coverage only decide WHO gets sacks and INTs, not how many? Which byte drives which? What happens to leftover weight? (Open question 4)'
    Changed='Defender [pass rush, coverage]: A = everyone [23, 23]; B = pass rush RE 255, others 0 / coverage LE 255, others 0; C = everyone [0, 0]; D = everyone [200, 200].'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s)
        if ($defPos -contains $pos) {
            switch ($g) {
                0 { $s = @(23, 23) }
                1 { $s = @($(if ($pos -eq 'RE') { 255 } else { 0 }), $(if ($pos -eq 'LE') { 255 } else { 0 })) }
                2 { $s = @(0, 0) }
                3 { $s = @(200, 200) }
            }
        }
        $s }
    Value={ param($g) @('all [23,23]','RE rush 255 / LE cover 255, rest 0','all [0,0]','all [200,200]')[$g] }
    Record=@('Team sacks','Team INTs','RE sacks','NT sacks','SS sacks','LE INTs','SS INTs','Pts allowed')
    Expect=@(
      'How credit is picked (code $9789 / $9802): random 0-255, take each of the first 10 defenders (RE, NT, LE, ROLB, RILB, LILB, LOLB, RCB, LCB, FS) in turn: if the number is below his value he gets it, otherwise subtract his value and go on. Whatever is left goes to the SS. Stock teams'' 11 values add up to 255.',
      'If the values only pick WHO gets credit: team sacks and team INTs about the same in all four groups.',
      'A: sacks/INTs spread about evenly (about 9% each; SS a little more).',
      'B: the RE gets every sack and the LE every INT -- shows pass rush drives sacks, coverage drives INTs.',
      'C: the SS gets every sack and every INT (all the weight is leftover).',
      'D: the RE gets about 78% and the NT about 22% of both; nobody else gets any (values are not scaled when the total is over 255).')
  }
  [pscustomobject]@{
    Name='SNES_05_TeamDefense'; Platform='SNES'
    Question='Does SNES TSB1 use the team SimData digits the same way as the NES? (Open question 5)'
    Changed='Same as NES_05: Team SimData A = 0x00, B = 0xF0, C = 0x0F, D = 0xFF. Preference 0 for all.'
    Team={ param($g) @( @{Hi=0;Lo=0}, @{Hi=15;Lo=0}, @{Hi=0;Lo=15}, @{Hi=15;Lo=15} )[$g] }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) @('SimData 0x00','SimData 0xF0','SimData 0x0F','SimData 0xFF')[$g] }
    Record=@('Rush yds allowed','Pass yds allowed','Pts allowed','Sacks made','INTs made')
    Expect=@('Same pattern as NES_05 if SNES works the same way.')
  }
  [pscustomobject]@{
    Name='SNES_06_RunPassPref'; Platform='SNES'
    Question='Are the SNES run/pass preference splits the same as the NES? (Open question 5)'
    Changed='Run/pass preference = 0 / 1 / 2 / 3 by group.'
    Team={ param($g) @{ Pref = $g } }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'Preference ' + $g }
    Record=@('Rush att','Pass att','Pass %')
    Expect=@('Same as NES: pass share A 50%, B 40%, C 60%, D 70%.')
  }
  [pscustomobject]@{
    Name='GEN_05_TeamDefense'; Platform='GENESIS'
    Question='Does Genesis TSB1 use the team SimData digits the same way as the NES? (Open question 5)'
    Changed='Same as NES_05: Team SimData A = 0x00, B = 0xF0, C = 0x0F, D = 0xFF. Preference 0 for all.'
    Team={ param($g) @( @{Hi=0;Lo=0}, @{Hi=15;Lo=0}, @{Hi=0;Lo=15}, @{Hi=15;Lo=15} )[$g] }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) @('SimData 0x00','SimData 0xF0','SimData 0x0F','SimData 0xFF')[$g] }
    Record=@('Rush yds allowed','Pass yds allowed','Pts allowed','Sacks made','INTs made')
    Expect=@('Same pattern as NES_05 if Genesis works the same way.')
  }
  [pscustomobject]@{
    Name='GEN_06_RunPassPref'; Platform='GENESIS'
    Question='Are the Genesis run/pass preference splits the same as the NES? (Open question 5)'
    Changed='Run/pass preference = 0 / 1 / 2 / 3 by group.'
    Team={ param($g) @{ Pref = $g } }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'Preference ' + $g }
    Record=@('Rush att','Pass att','Pass %')
    Expect=@('Same as NES: pass share A 50%, B 40%, C 60%, D 70%.')
  }
  [pscustomobject]@{
    Name='GEN_05b_TeamDefense'; Platform='GENESIS'
    Question='GEN_05 again, built after the TSBTool fix for the Genesis sim-block team order (cowboys/redskins, falcons/49ers were swapped in GEN_05).'
    Changed='Same as NES_05: Team SimData A = 0x00, B = 0xF0, C = 0x0F, D = 0xFF. Preference 0 for all.'
    Team={ param($g) @( @{Hi=0;Lo=0}, @{Hi=15;Lo=0}, @{Hi=0;Lo=15}, @{Hi=15;Lo=15} )[$g] }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) @('SimData 0x00','SimData 0xF0','SimData 0x0F','SimData 0xFF')[$g] }
    Record=@('Rush yds allowed','Pass yds allowed','Pts allowed','Sacks made','INTs made')
    Expect=@('Same pattern as NES_05 if Genesis works the same way: high digit cuts rush yards allowed,',
             'low digit cuts pass yards allowed and adds sacks/INTs. Check the team screen shows yards ALLOWED, not gained.')
  }
  [pscustomobject]@{
    Name='GEN_06b_RunPassPrefFixed'; Platform='GENESIS'; Fix=$true
    Question='Does GENESIS_FIX_SIM_BUGS make preference 1 play as heavy rushing?'
    Changed='Run/pass preference = 0 / 1 / 2 / 3 by group, and the GENESIS_FIX_SIM_BUGS game fix is applied.'
    Team={ param($g) @{ Pref = $g } }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'Preference ' + $g }
    Record=@('Rush att','Pass att','Pass %')
    Expect=@('Like NES_06: pass share about A 42%, B 38%, C 55%, D 62% (GEN_06 without the fix had B at 72%).')
  }
  [pscustomobject]@{
    Name='GEN_05c_TeamDefenseFixed'; Platform='GENESIS'; Fix=$true
    Question='Does GENESIS_FIX_SIM_BUGS (defense roll fix) make team defense SimData count like it does on NES?'
    Changed='Same as GEN_05b (Team SimData A = 0x00, B = 0xF0, C = 0x0F, D = 0xFF, preference 0), plus the GENESIS_FIX_SIM_BUGS game fix.'
    Team={ param($g) @( @{Hi=0;Lo=0}, @{Hi=15;Lo=0}, @{Hi=0;Lo=15}, @{Hi=15;Lo=15} )[$g] }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) @('SimData 0x00','SimData 0xF0','SimData 0x0F','SimData 0xFF')[$g] }
    Record=@('Rush yds allowed','Pass yds allowed','Pts allowed','Sacks made','INTs made')
    Expect=@('Bigger group differences than GEN_05b, closer to NES_05: high digit cuts rush yds allowed (NES A-D 2353/1431/2417/1440),',
             'low digit cuts pass yds allowed and adds sacks/INTs; pts allowed NES 486/418/424/324 (GEN_05b 397/358/362/346).',
             'Also compare league rush yards with GEN_05b: Genesis run-play tiers are stingier than NES, so runs may come out short.',
             'Save an ares state (.bs1) after the season: yards and points allowed can be checked from it.')
  }
  [pscustomobject]@{
    Name='GEN_07a_StockSeason'; Platform='GENESIS'; Stock=$true
    Question='How much do teams score in a stock Genesis season (the game''s own sim data, no fixes)? Baseline for GEN_07b.'
    Changed='Nothing: this is the stock ROM, byte for byte.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Normal-looking Genesis scores. Compare the league totals with GEN_07b.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07b_StockSeasonFixed'; Platform='GENESIS'; Stock=$true; Fix=$true
    Question='Does GENESIS_FIX_SIM_BUGS make a stock season too low-scoring (compare with GEN_07a)?'
    Changed='The stock ROM plus the GENESIS_FIX_SIM_BUGS game fix (preference 1 + defense roll); nothing else.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Team defense SimData now counts, so good defenses allow less and the spread between teams grows.',
             'League points / yards lower than GEN_07a (GEN_05c dropped ~37% vs GEN_05b). If it looks too stingy,',
             'restoring the NES run-play cutoffs is the next thing to test.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07c_FixShiftedDefense'; Platform='GENESIS'; Stock=$true; Fix=$true
    # Option 1: the two team-defense value tables (one entry per SimData digit) shifted down by about
    # half their average (rush -82, pass -84): same gaps between digits, lower average defense roll.
    Patches=@( @(0xDE9BA, [byte[]]@(13,23,30,40,82,84,86,88,90,92,94,106,108,110,112,145)),
               @(0xDE9CA, [byte[]]@(32,34,36,60,64,78,82,94,92,94,96,98,100,118,120,137)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + team defense tables shifted down (rush -82, pass -84): normal scoring with defense still mattering?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus the defense value tables at 0xDE9BA / 0xDE9CA lowered (option 1).'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + shifted tables' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Scoring back near GEN_07a (about 20 pts/game; GEN_07b had 12.8) with defense digits still mattering.',
             'These tables are also read by 3 other plays (not just the defense roll), so those change too.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07d_FixDefenseRollOffset'; Platform='GENESIS'; Stock=$true; Fix=$true
    # Option 2: only the defense roll uses (defense value - 83, not below 0). The roll's
    # 'move.w $ff33a8,d1' at 0xDC23A becomes 'jsr $DF100' to a new routine in unused (0xFF) space after the sim data:
    #   move.w $ff33a8,d1 / subi.w #83,d1 / bcc.s +2 / moveq #0,d1 / rts
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x53, 0x64,0x02, 0x72,0x00, 0x4E,0x75)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + the defense roll subtracting 83 from the defense value: normal scoring with defense still mattering?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus a small routine at 0xDF100 so only the defense roll uses (defense value - 83) (option 2).'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + roll offset' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Like GEN_07c, but nothing else in the game changes (the other plays keep the stock defense values).',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07e_FixNesRunCutoffs'; Platform='GENESIS'; Stock=$true; Fix=$true
    # Option 3: the Genesis run-play yardage cutoffs (0x3E/0x7B/0xB8/0xD7) set back to NES/SNES 0x34/0x67/0x9A/0xCD.
    Patches=@( @(0xDBA55, [byte[]]@(0x34)), @(0xDBA6D, [byte[]]@(0x67)), @(0xDBA85, [byte[]]@(0x9A)), @(0xDBA9D, [byte[]]@(0xCD)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + the NES run-play yardage cutoffs: how much scoring comes back?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus the 4 run-play cutoffs set to the NES/SNES values (option 3).'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + NES run cutoffs' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('More rush yards than GEN_07b; passing about the same as GEN_07b. Probably not enough on its own.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07f_FixDefenseRollOffset100'; Platform='GENESIS'; Stock=$true; Fix=$true
    # GEN_07d with a bigger offset: the defense roll uses (defense value - 100, not below 0).
    #   move.w $ff33a8,d1 / subi.w #100,d1 / bcc.s +2 / moveq #0,d1 / rts
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x64, 0x64,0x02, 0x72,0x00, 0x4E,0x75)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + the defense roll subtracting 100: scoring back at GEN_07a (20.8 pts/game)?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus a small routine at 0xDF100 so the defense roll uses (defense value - 100).'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + roll offset 100' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('More scoring than GEN_07d (18.3 pts/game); the worst rush defense digit (value 95) counts as 0.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07g_FixRollOffsetNesCutoffs'; Platform='GENESIS'; Stock=$true; Fix=$true
    # GEN_07d (defense roll uses value - 83) + GEN_07e (NES run-play cutoffs).
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x53, 0x64,0x02, 0x72,0x00, 0x4E,0x75)),
               @(0xDBA55, [byte[]]@(0x34)), @(0xDBA6D, [byte[]]@(0x67)), @(0xDBA85, [byte[]]@(0x9A)), @(0xDBA9D, [byte[]]@(0xCD)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + defense roll value - 83 + NES run cutoffs: scoring back at GEN_07a?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus the GEN_07d routine at 0xDF100 (defense value - 83) and the NES/SNES run-play cutoffs.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + roll offset 83 + NES run cutoffs' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('More rushing than GEN_07d, passing like GEN_07d.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07h_FixDefenseRollScaled'; Platform='GENESIS'; Stock=$true; Fix=$true
    # The defense roll uses defense value * 3/8 (scaled, not shifted: the gaps between digits shrink too).
    #   move.w $ff33a8,d1 / mulu.w #3,d1 / lsr.w #3,d1 / rts
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0xC2,0xFC,0x00,0x03, 0xE6,0x49, 0x4E,0x75)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + the defense roll using 3/8 of the defense value: normal scoring, smaller spread than GEN_07d?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus a small routine at 0xDF100 so the defense roll uses defense value * 3/8.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + roll scaled 3/8' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Scoring near GEN_07a; points allowed spread between teams smaller than GEN_07d (sd 59).',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07g2_FixRollOffsetNesCutoffsRepeat'; Platform='GENESIS'; Stock=$true; Fix=$true
    # Same bytes as GEN_07g: a second season, to see how much one season's numbers move.
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x53, 0x64,0x02, 0x72,0x00, 0x4E,0x75)),
               @(0xDBA55, [byte[]]@(0x34)), @(0xDBA6D, [byte[]]@(0x67)), @(0xDBA85, [byte[]]@(0x9A)), @(0xDBA9D, [byte[]]@(0xCD)) )
    Question='GEN_07g again (same ROM bytes): how much do the season numbers move between two seasons?'
    Changed='Same as GEN_07g: stock + GENESIS_FIX_SIM_BUGS + defense roll value - 83 (routine at 0xDF100) + NES/SNES run-play cutoffs.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'repeat of GEN_07g' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Close to GEN_07g (21.7 pts/game, 152 rush yds/game); the differences show the season-to-season noise.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07i_FixRollOffsetMidCutoffs'; Platform='GENESIS'; Stock=$true; Fix=$true
    # GEN_07d (defense roll uses value - 83) + run-play cutoffs halfway between Genesis (3E/7B/B8/D7) and NES (34/67/9A/CD).
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x53, 0x64,0x02, 0x72,0x00, 0x4E,0x75)),
               @(0xDBA55, [byte[]]@(0x39)), @(0xDBA6D, [byte[]]@(0x71)), @(0xDBA85, [byte[]]@(0xA9)), @(0xDBA9D, [byte[]]@(0xD2)) )
    Question='Stock + GENESIS_FIX_SIM_BUGS + defense roll value - 83 + run cutoffs halfway to NES: stock scoring without the GEN_07g run inflation?'
    Changed='The stock ROM plus GENESIS_FIX_SIM_BUGS, plus the GEN_07d routine at 0xDF100 (defense value - 83) and run-play cutoffs 39/71/A9/D2.'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'stock + fix + roll offset 83 + midpoint run cutoffs' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('About 20 pts/game and about 130 rush yds/game (GEN_07d 18.3 / 115, GEN_07g 21.7 / 152), defense effect like GEN_07d.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07j_PassDefense115'; Platform='GENESIS'; Stock=$true; Fix=$true
    # GEN_07i + an extra 32 off the pass defense value for the main pass play only (115 in all for passes).
    # The pass play's 'move.w d0,$ff33a8' at 0xDBD24 (just before the defense roll) becomes 'jsr $DF110':
    #   subi.w #32,d0 / bcc.s +2 / moveq #0,d0 / move.w d0,$ff33a8 / rts
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x53, 0x64,0x02, 0x72,0x00, 0x4E,0x75)),
               @(0xDBA55, [byte[]]@(0x39)), @(0xDBA6D, [byte[]]@(0x71)), @(0xDBA85, [byte[]]@(0xA9)), @(0xDBA9D, [byte[]]@(0xD2)),
               @(0xDBD24, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x10)),
               @(0xDF110, [byte[]]@(0x04,0x40,0x00,0x20, 0x64,0x02, 0x70,0x00, 0x33,0xC0,0x00,0xFF,0x33,0xA8, 0x4E,0x75)) )
    Question='GEN_07i + pass defense value - 115 on the main pass play: stock-level passing (comp% ~75, more 1000-yard receivers)?'
    Changed='Same as GEN_07i, plus a routine at 0xDF110 so the main pass play takes 32 more off the pass defense value (115 in all).'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'GEN_07i + pass defense -115' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('Completion % about 74-75 (GEN_07i 69.9, stock 74.8), pass yds near stock for non-preference-1 teams (138/game).',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
  [pscustomobject]@{
    Name='GEN_07k_PassDefense135'; Platform='GENESIS'; Stock=$true; Fix=$true
    # GEN_07i + an extra 52 off the pass defense value for the main pass play only (135 in all for passes).
    # The pass play's 'move.w d0,$ff33a8' at 0xDBD24 (just before the defense roll) becomes 'jsr $DF110':
    #   subi.w #52,d0 / bcc.s +2 / moveq #0,d0 / move.w d0,$ff33a8 / rts
    Patches=@( @(0xDC23A, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x00)),
               @(0xDF100, [byte[]]@(0x32,0x39,0x00,0xFF,0x33,0xA8, 0x04,0x41,0x00,0x53, 0x64,0x02, 0x72,0x00, 0x4E,0x75)),
               @(0xDBA55, [byte[]]@(0x39)), @(0xDBA6D, [byte[]]@(0x71)), @(0xDBA85, [byte[]]@(0xA9)), @(0xDBA9D, [byte[]]@(0xD2)),
               @(0xDBD24, [byte[]]@(0x4E,0xB9,0x00,0x0D,0xF1,0x10)),
               @(0xDF110, [byte[]]@(0x04,0x40,0x00,0x34, 0x64,0x02, 0x70,0x00, 0x33,0xC0,0x00,0xFF,0x33,0xA8, 0x4E,0x75)) )
    Question='GEN_07i + pass defense value - 135 on the main pass play: stock-level passing (comp% ~75, more 1000-yard receivers)?'
    Changed='Same as GEN_07i, plus a routine at 0xDF110 so the main pass play takes 52 more off the pass defense value (135 in all).'
    Team={ param($g) @{} }
    Player={ param($g,$pos,$s) $s }
    Value={ param($g) 'GEN_07i + pass defense -135' }
    Record=@('Pts for','Pts allowed','Rush att','Rush yds','Pass att','Pass yds','Sacks made','INTs made')
    Expect=@('More passing than GEN_07j; pass defense digits 0-2 all count as 0.',
             'Save an ares state (.bs1) at the Season Menu after the season: every column can be read from it.')
  }
)
# Extra bytes a test may change: GENESIS_FIX_SIM_BUGS rewrites the preference-1 play-calling code,
# the defense roll's random-byte address, and the rebalance (calls at 0xDC23A / 0xDBD24, routines at
# 0xDF100-0xDF11F, run-play cutoffs) and the season receiving-TD update. GEN_05c..GEN_07k were built before the rebalance was part of
# the fix (their Patches did that job), so rebuilding them now would not give the ROMs that were simmed.
$fixRanges = @( @(0xDE790, 30), @(0xDC220, 6), @(0xDC23A, 6), @(0xDBD24, 6), @(0xDF100, 32),
                @(0xDBA55, 1), @(0xDBA6D, 1), @(0xDBA85, 1), @(0xDBA9D, 1),
                @(0xDD818, 4) )   # season receiving-TD update (bsr $DD330 -> $DD318)

# ---------------------------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------------------------
function Invoke-Tool {
    # Windows PowerShell 5.1 turns any native stderr line into an error record; the tool prints
    # informational notes there ("No valid input file..."), so don't let that stop the script.
    $old = $ErrorActionPreference; $ErrorActionPreference = 'Continue'
    try { & $Exe @args 2>&1 | ForEach-Object { "$_" } } finally { $ErrorActionPreference = $old }
}
function Get-Dump([string]$rom) {
    # Drop the tool's status notes so only the roster text is fed back in.
    (Invoke-Tool $rom | Out-String) -split "`r?`n" |
        Where-Object { $_ -notmatch '^(No valid input file|ROM Type =|System\.Management\.Automation\.RemoteException)' }
}

function Format-Sims($s) { '[' + (($s | ForEach-Object { $_ }) -join ', ') + ' ]' }

# Builds the input text for one test from the stock dump. Returns @{ Text; Teams (name->group) }
function Build-Input($dump, $test) {
    $out = New-Object System.Collections.Generic.List[string]
    $teams = [ordered]@{}
    $g = -1
    foreach ($line in $dump) {
        if ($test.Stock) {
            # stock test: keep the game's own sim data, only note the groups for the sheet
            if ($line -match '^TEAM = (\w+) SimData') { $teams[$matches[1]] = $teams.Count % 4 }
            $out.Add($line)
            continue
        }
        if ($line -match '^TEAM = (\w+) SimData=0x\w+(.*)$') {
            $g = $teams.Count % 4
            $teams[$matches[1]] = $g
            $t = @{ Hi = $baselineTeam.Hi; Lo = $baselineTeam.Lo; Pref = $baselineTeam.Pref }
            $ov = & $test.Team $g
            foreach ($k in $ov.Keys) { $t[$k] = $ov[$k] }
            $out.Add(('TEAM = {0} SimData=0x{1:x}{2:x}{3}{4}' -f $matches[1], $t.Hi, $t.Lo, $t.Pref, $matches[2]))
            continue
        }
        if ($g -ge 0 -and $line -match '^(\w+),(.*?),?\s*\[[^\]]*\]\s*$') {
            $pos = $matches[1]
            $base = Get-BaselineSims $pos
            if ($base) {
                $s = @(& $test.Player $g $pos ([int[]]@($base)))
                $prefix = $line.Substring(0, $line.LastIndexOf('['))
                $out.Add($prefix + (Format-Sims $s))
                continue
            }
        }
        $out.Add($line)
    }
    if ($test.Fix) { $out.Add('GENESIS_FIX_SIM_BUGS') }
    @{ Text = ($out -join "`r`n"); Teams = $teams }
}

# Parses a dump into team -> @{ Sim = 'xxx'; Players = @{ pos = 'a,b,c' } } for verification
function Read-Sims($dump) {
    $r = @{}; $cur = $null
    foreach ($line in $dump) {
        if ($line -match '^TEAM = (\w+) SimData=0x(\w+)') { $cur = @{ Sim = $matches[2]; Players = @{} }; $r[$matches[1]] = $cur; continue }
        if ($cur -and $line -match '^(\w+),.*\[([^\]]*)\]\s*$') { $cur.Players[$matches[1]] = (($matches[2] -split ',') | ForEach-Object { $_.Trim() }) -join ',' }
    }
    $r
}

function Write-Sheet($test, $teams, $romFile, $path) {
    $sb = New-Object System.Text.StringBuilder
    $line = '=' * 100
    [void]$sb.AppendLine($line)
    [void]$sb.AppendLine("SIM TEST: $($test.Name)")
    [void]$sb.AppendLine("ROM:      $romFile   (platform: $($test.Platform))")
    [void]$sb.AppendLine($line)
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('QUESTION')
    [void]$sb.AppendLine("  $($test.Question)")
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('WHAT IS DIFFERENT IN THIS ROM')
    if ($test.Stock) {
        [void]$sb.AppendLine("  Stock sim data (the game's own values for every team). $($test.Changed)")
    } else {
        [void]$sb.AppendLine('  Baseline (every team, unless changed below): QB [rush 4, pass 8, pocket 3],')
        [void]$sb.AppendLine('  RB/WR/TE [rush 8, catch 8, yards/catch 8, targets 8], defenders [pass rush 30, coverage 30],')
        [void]$sb.AppendLine('  K/P 8, team SimData 0x88, run/pass preference 0. Names and in-game ratings are stock.')
        [void]$sb.AppendLine("  Change: $($test.Changed)")
    }
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('GROUPS')
    for ($g = 0; $g -lt 4; $g++) {
        $names = ($teams.Keys | Where-Object { $teams[$_] -eq $g }) -join ', '
        [void]$sb.AppendLine(('  {0}  ({1}): {2}' -f $groupNames[$g], (& $test.Value $g), $names))
    }
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('HOW TO RUN')
    [void]$sb.AppendLine('  1. Load the ROM in the emulator with fresh save data (delete any old .sav / battery file first).')
    [void]$sb.AppendLine('  2. Start a season and set all 28 teams to be simulated (SKP / computer, as you normally sim a season).')
    [void]$sb.AppendLine('  3. Simulate the full regular season (fast-forward is fine).')
    [void]$sb.AppendLine('  4. Record the season stats below from the team / player stats screens.')
    [void]$sb.AppendLine('     Short on time? Record at least 2 teams per group (8 teams) -- that is usually enough to see the pattern.')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('WHAT TO EXPECT')
    foreach ($e in $test.Expect) { [void]$sb.AppendLine("  - $e") }
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('RESULTS  (season totals; fill in the blanks)')
    $cols = @('Team', 'Grp') + $test.Record
    $w = @(12, 4) + ($test.Record | ForEach-Object { [Math]::Max(9, $_.Length) })
    $hdr = ''; $sepParts = @()
    for ($i = 0; $i -lt $cols.Count; $i++) { $hdr += $cols[$i].PadRight($w[$i]) + ' | '; $sepParts += ('-' * $w[$i]) }
    $sep = $sepParts -join '-+-'
    [void]$sb.AppendLine('  ' + $hdr.TrimEnd(' ', '|'))
    [void]$sb.AppendLine('  ' + $sep)
    foreach ($g in 0..3) {
        foreach ($t in ($teams.Keys | Where-Object { $teams[$_] -eq $g })) {
            $row = $t.PadRight($w[0]) + ' | ' + $groupNames[$g].PadRight($w[1]) + ' | '
            for ($i = 2; $i -lt $cols.Count; $i++) { $row += (' ' * $w[$i]) + ' | ' }
            [void]$sb.AppendLine('  ' + $row.Substring(0, $row.Length - 3))
        }
        [void]$sb.AppendLine('  ' + $sep)
    }
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('GROUP AVERAGES (optional)')
    foreach ($g in 0..3) { [void]$sb.AppendLine(('  {0} ({1}): ' -f $groupNames[$g], (& $test.Value $g))) }
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('NOTES / ODD THINGS SEEN')
    [void]$sb.AppendLine('  ')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine('CONCLUSION')
    [void]$sb.AppendLine('  ')
    [void]$sb.AppendLine('')
    [void]$sb.AppendLine("Tested by: ____________   Date: ____________   Emulator: ____________")
    [IO.File]::WriteAllText($path, $sb.ToString())
}

# ---------------------------------------------------------------------------------------------
# Build
# ---------------------------------------------------------------------------------------------
if ($Only) {
    # 'powershell -File' passes "A,B" as one string
    $Only = @($Only | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $unknown = @($Only | Where-Object { $tests.Name -notcontains $_ })
    if ($unknown.Count) { throw "Unknown test name(s): $($unknown -join ', ')" }
    $tests = @($tests | Where-Object { $Only -contains $_.Name })
}
$inputDir = Join-Path $here 'inputs'
New-Item -ItemType Directory -Force $inputDir | Out-Null
$dumps = @{}
$failures = 0
Push-Location $here
try {
    foreach ($test in $tests) {
        if (-not $dumps.ContainsKey($test.Platform)) { $dumps[$test.Platform] = Get-Dump $stock[$test.Platform] }
        $built = Build-Input $dumps[$test.Platform] $test
        $inputFile = Join-Path $inputDir ($test.Name + '_input.txt')
        [IO.File]::WriteAllText($inputFile, $built.Text)
        $romFile = $test.Name + $ext[$test.Platform]
        if (Test-Path $romFile) { Remove-Item $romFile }
        # -out: must be a relative name (MainClass splits options on ':'), so we run from this folder.
        $log = Invoke-Tool $stock[$test.Platform] $inputFile "-out:$romFile" | Out-String
        if (-not (Test-Path $romFile)) { Write-Host "FAILED to create $romFile`n$log"; $failures++; continue }
        # The tool lower-cases the -out: name; restore the intended capitalization.
        $made = Get-ChildItem -File -Filter $romFile | Select-Object -First 1
        if ($made.Name -cne $romFile) { Rename-Item $made.FullName ($romFile + '.tmp'); Rename-Item ($romFile + '.tmp') $romFile }

        # Keep only the sim block + preference table from TSBTool's output; everything else = stock.
        $romPath  = Join-Path $here $romFile
        $toolRom  = [IO.File]::ReadAllBytes($romPath)
        $finalRom = [IO.File]::ReadAllBytes($stock[$test.Platform])
        # A stock test copies no sim data (the ROM stays stock); only the fix bytes, if any.
        $ranges = @()
        if (-not $test.Stock) { $ranges = @($simRanges[$test.Platform]) }
        if ($test.Fix) { foreach ($fr in $fixRanges) { $ranges += ,$fr } }
        foreach ($r in $ranges) { [Array]::Copy($toolRom, $r[0], $finalRom, $r[0], $r[1]) }
        # Test-only game patches (experiments, not TSBTool features): bytes written over the result.
        foreach ($pt in @($test.Patches | Where-Object { $_ })) {
            [Array]::Copy([byte[]]$pt[1], 0, $finalRom, $pt[0], $pt[1].Length)
            $ranges += ,@($pt[0], $pt[1].Length)
        }
        [IO.File]::WriteAllBytes($romPath, $finalRom)

        # Verify: every sim value in the new ROM matches the input text
        $want = Read-Sims ($built.Text -split "`r`n")
        $got  = Read-Sims (Get-Dump (Join-Path $here $romFile))
        $bad = @()
        foreach ($team in $want.Keys) {
            if ($want[$team].Sim -ne $got[$team].Sim) { $bad += "$team SimData want $($want[$team].Sim) got $($got[$team].Sim)" }
            foreach ($p in $want[$team].Players.Keys) {
                if ($want[$team].Players[$p] -ne $got[$team].Players[$p]) { $bad += "$team $p want [$($want[$team].Players[$p])] got [$($got[$team].Players[$p])]" }
            }
        }
        if ($log -match '(?im)^\s*(error|warning)') { $bad += "tool output: $($log.Trim())" }
        # And nothing outside those ranges differs from stock
        $stockRom = [IO.File]::ReadAllBytes($stock[$test.Platform])
        $inRange = New-Object bool[] $stockRom.Length
        foreach ($r in $ranges) { for ($i = $r[0]; $i -lt $r[0] + $r[1]; $i++) { $inRange[$i] = $true } }
        if ($test.Fix -and ($finalRom[0xDE798] -ne 0x65 -or $finalRom[0xDC225] -ne 0x9F -or $finalRom[0xDC23A] -ne 0x4E -or $finalRom[0xDBD24] -ne 0x4E)) { $bad += 'GENESIS_FIX_SIM_BUGS was not applied' }
        foreach ($pt in @($test.Patches | Where-Object { $_ })) {
            for ($i = 0; $i -lt $pt[1].Length; $i++) {
                if ($finalRom[$pt[0] + $i] -ne $pt[1][$i]) { $bad += ('patch byte 0x{0:X} not written' -f ($pt[0] + $i)); break }
            }
        }
        for ($i = 0; $i -lt $stockRom.Length; $i++) {
            if (-not $inRange[$i] -and $stockRom[$i] -ne $finalRom[$i]) { $bad += ('byte 0x{0:X} differs from stock outside the sim ranges' -f $i); break }
        }
        if ($bad.Count) { $failures++; Write-Host "VERIFY FAILED $romFile"; $bad | Select-Object -First 10 | ForEach-Object { Write-Host "   $_" } }
        else { Write-Host "OK  $romFile" }

        Write-Sheet $test $built.Teams $romFile (Join-Path $here ($test.Name + '_results.txt'))
    }
} finally { Pop-Location }
if ($failures) { Write-Host "$failures test ROM(s) had problems."; exit 1 }
Write-Host "All $($tests.Count) test ROMs built and verified."
