SIM ENGINE TEST ROMS
====================
Test ROMs for the "Open questions" in TSBTool\TSB_Simulation.md. Each ROM has a matching
<name>_results.txt sheet with the question, what was changed, which teams are in which group,
what to expect, and a table for recording the season stats.

How every ROM is built
  - All 28 teams get the SAME neutral sim values (QB [4, 8, 3], RB/WR/TE [8, 8, 8, 8],
    defenders [30, 30], K/P 8, team SimData 0x88, run/pass preference 0).
  - Then ONE thing is set to four different levels across groups A/B/C/D of 7 teams each.
    Groups are by roster order (team 1 = A, team 2 = B, ...), so every division has all four.
  - Only the sim data block and the run/pass preference table differ from the stock ROM.
    Names, ratings, schedule and everything else are stock.
  To run one: sim a full regular season with all teams on computer/skip, then fill in its sheet.

ROMs (suggested order -- the first four answer the biggest questions)
  NES_05_TeamDefense       team SimData digits: rush defense / pass defense; sacks + INTs     (Q4)
  NES_06_RunPassPref       run/pass preference splits (50/50, 60 run, 60 pass, 70 pass)
  NES_04_ReceiverYPC       3rd skill sim value = yards per catch (not punt returns)          (Q1)
  NES_08_Returners         which sim value kick / punt returners use                         (Q3)
  NES_07_Scramble          how often QBs scramble; pocket value                              (Q2)
  NES_01_RunOffense        Sim rush vs yards per carry                                       (Q1)
  NES_02_QBPass            QB Sim pass vs completions, yards, sacks, INTs                    (Q1, Q4)
  NES_03_ReceiverCatch     receiver Sim catch vs completions                                 (Q1)
  NES_09_DefenderWeights   defender pass rush / coverage only pick who gets sacks / INTs     (Q4)
  SNES_05_TeamDefense      SNES version of NES_05                                            (Q5)
  SNES_06_RunPassPref      SNES version of NES_06                                            (Q5)
  GEN_05_TeamDefense       Genesis version of NES_05                                         (Q5)
  GEN_06_RunPassPref       Genesis version of NES_06                                         (Q5)
  GEN_05b_TeamDefense      GEN_05 rebuilt after the TSBTool sim-block team order fix         (Q5)
  GEN_06b_RunPassPrefFixed GEN_06 + GENESIS_FIX_SIM_BUGS (preference 1 fix)                  (Q5)
  GEN_05c_TeamDefenseFixed GEN_05b + GENESIS_FIX_SIM_BUGS (defense roll fix)                 (Q5)
  GEN_07a_StockSeason      stock Genesis ROM, unchanged: league scoring baseline              (Q5)
  GEN_07b_StockSeasonFixed stock + GENESIS_FIX_SIM_BUGS: does the fix make seasons too stingy? (Q5)
  GEN_07c_FixShiftedDefense    07b + defense tables shifted down (option 1)               (Q5)
  GEN_07d_FixDefenseRollOffset 07b + defense roll uses value-83 (option 2, code at 0xDF100) (Q5)
  GEN_07e_FixNesRunCutoffs     07b + NES run-play cutoffs (option 3)                       (Q5)
  GEN_07f_FixDefenseRollOffset100 07b + defense roll uses value-100 (code at 0xDF100)    (Q5)
  GEN_07g_FixRollOffsetNesCutoffs 07d (value-83) + NES run-play cutoffs (07e)           (Q5)
  GEN_07h_FixDefenseRollScaled    07b + defense roll uses value*3/8 (code at 0xDF100)   (Q5)
  GEN_07g2_FixRollOffsetNesCutoffsRepeat  same bytes as 07g: second season (noise check) (Q5)
  GEN_07i_FixRollOffsetMidCutoffs 07d (value-83) + run cutoffs halfway to NES 39/71/A9/D2 (Q5)
  GEN_07j_PassDefense115          07i + main pass play pass defense -115 in all (0xDF110) (Q5)
  GEN_07k_PassDefense135          07i + main pass play pass defense -135 in all (0xDF110) (Q5)
  GEN_08a_OilersNinersHeavyPass   stock + GENESIS_FIX_SIM_BUGS (made by TSBTool, = 07j bytes) + 49ers pref 3 (Oilers already 3)
  GEN_08b_ReceivingTdFix          08a + the season receiving-TD fix (0xDD81A FB16->FAFE): receivers' TDs should add up to pass TDs

Rebuilding
  powershell -ExecutionPolicy Bypass -File Generate-SimTests.ps1 -Exe <path to TSBToolSupreme.exe>
  The script writes the TSBTool input text it used to inputs\, applies it to the stock ROMs in
  ..\ROMs, keeps only the sim ranges, and checks every sim value and every other byte.
