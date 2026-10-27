using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// The roster name check (PlayerNames) end to end on every NES ROM type, with real 2026 rosters: plan, apply the
    /// plan's text (what MainClass/MainGUI do), then check that no name was rejected or cut by InsertPlayer, that
    /// every name in the applied text is in the ROM exactly, that every team's ratings landed as written (the
    /// Bills' table sits right after the main name area), and that applying the same file again to the result
    /// comes up clean. The 32-team rosters already need more room than these ROMs have, so they exercise the
    /// shortening path as they are; no lengthened copies needed (compare Tsb2Tsb3PlayerNamesTests).
    ///
    /// ROMs: TSPRBOWL (28 teams, 16 games), CXROM v105 / v111 (32 teams, 16 games), CXROM 18-week (32 teams,
    /// 17 games), and TSB 2026-32-105 (a v105 TSBTool already filled: no free name bytes).
    /// </summary>
    [TestClass]
    public class NesPlayerNamesTests
    {
        private static readonly string[] Roms32 = { "TSB 2007-32-105.nes", "TSB 2007-32-111.nes", "SbluemanBase_18week_RealNFL-1.nes", "TSB 2026-32-105.nes" };

        private static readonly Regex teamLine = new Regex("^TEAM\\s*=\\s*([0-9a-z]+)");
        private static readonly Regex playerLine = new Regex("^([A-Z]+[1-4]?)\\s*,\\s*([a-zA-Z \\.\\-]+?)\\s*,\\s*Face=", RegexOptions.IgnoreCase);

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static string ReadInput(string path)
        {
            if (!File.Exists(path))
                Assert.Inconclusive("Local test file missing: " + path);
            return File.ReadAllText(path);
        }

        private static List<string> RunAndCollectErrors(Action action)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                action();
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            return giver.Errors;
        }

        // InsertPlayer's complaints: a rejected name (no room) or one cut to 15 characters. The plan must leave none.
        private static List<string> NameErrors(List<string> errors)
        {
            return errors.Where(e => e.Contains("would overflow") || e.Contains("character limit") || e.Contains("Name will be")).ToList();
        }

        // What MainClass/MainGUI do: plan, apply the plan, then the (rewritten) text. Returns the text applied.
        private static string PlanAndApply(TecmoTool tool, string text, out RosterNamePlan plan, out List<string> errors)
        {
            RosterNamePlan p = PlayerNames.PlanFit(tool, text);
            string applied = null;
            errors = RunAndCollectErrors(delegate
            {
                applied = p.Apply(tool, text);
                tool.ProcessText(applied);
            });
            plan = p;
            return applied;
        }

        // (team, position) -> the player line, for every player line of a team the ROM has.
        private static Dictionary<string, string> PlayerLines(TecmoTool tool, string text)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string team = null;
            foreach (string raw in text.Replace("\r\n", "\n").Split('\n'))
            {
                string line = raw.Trim();
                Match m;
                if ((m = teamLine.Match(line)).Success)
                    team = TecmoTool.GetTeamIndex(m.Groups[1].Value) < 0 ? null : m.Groups[1].Value;
                else if (team != null && (m = playerLine.Match(line)).Success)
                    ret[team + " " + m.Groups[1].Value] = line;
            }
            return ret;
        }

        // Every name in the applied text is in the ROM exactly, and every player's ratings are as written.
        private static void AssertRosterLanded(TecmoTool tool, string applied, int expectedPlayers, string label)
        {
            Dictionary<string, string> lines = PlayerLines(tool, applied);
            Assert.AreEqual(expectedPlayers, lines.Count, label + ": sanity, player lines");
            List<string> mismatches = new List<string>();
            foreach (KeyValuePair<string, string> kv in lines)
            {
                string team = kv.Key.Split(' ')[0], pos = kv.Key.Split(' ')[1];
                string expectedName = Regex.Replace(playerLine.Match(kv.Value).Groups[2].Value.Trim(), "\\s+", " ");
                string actualName = tool.GetName(team, pos);
                if (actualName != expectedName)
                    mismatches.Add(string.Format("{0}: name '{1}', ROM has '{2}'", kv.Key, expectedName, actualName));

                int[] expected = InputParser.GetInts(kv.Value);
                int[] actual = tool.GetAbilities(team, pos);
                if (actual.Where((v, i) => v != expected[i]).Any())
                    mismatches.Add(string.Format("{0}: ratings {1}, ROM has {2}", kv.Key,
                        string.Join(",", expected.Take(actual.Length).Select(v => v.ToString()).ToArray()),
                        string.Join(",", actual.Select(v => v.ToString()).ToArray())));
            }
            Assert.AreEqual(0, mismatches.Count, label + ":\n" + string.Join("\n", mismatches.Take(20).ToArray()));
        }

        private static void AssertRosterAppliesAndReapplies(string romName, string inputPath, int expectedPlayers)
        {
            string text = ReadInput(inputPath);
            string label = romName + " + " + Path.GetFileName(inputPath);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));

            RosterNamePlan plan;
            List<string> errors;
            string applied = PlanAndApply(tool, text, out plan, out errors);

            foreach (PlayerNamesFitReport report in plan.Reports)
                Assert.AreEqual(0, report.UnresolvedFailures.Count, label + ":\n" + report);
            Assert.AreEqual(0, NameErrors(errors).Count, label + ":\n" + string.Join("\n", NameErrors(errors).ToArray()));
            AssertRosterLanded(tool, applied, expectedPlayers, label);

            // The same file again, on the result: no complaints, and again every name lands as planned. (The second
            // plan can differ: the first one may free a few bytes more than needed, which the second gives back.)
            string appliedAgain = PlanAndApply(tool, text, out plan, out errors);
            foreach (PlayerNamesFitReport report in plan.Reports)
                Assert.AreEqual(0, report.UnresolvedFailures.Count, label + " (second apply):\n" + report);
            Assert.AreEqual(0, NameErrors(errors).Count, label + " (second apply):\n" + string.Join("\n", NameErrors(errors).ToArray()));
            AssertRosterLanded(tool, appliedAgain, expectedPlayers, label + " (second apply)");
        }

        // ---- 28 teams, 16 games ----

        [TestMethod]
        public void Tsprbowl_28Team2026Roster_AppliesExactly_ReapplyIsClean()
        {
            AssertRosterAppliesAndReapplies("TSPRBOWL.nes", TestRoms.GetGeneratedInputPath("TSB1_28_2026_input.txt"), 28 * 30);
        }

        // ---- 32 teams: v105 / v111 (16 games), 18-week (17 games), and an already-filled v105 ----

        [TestMethod]
        public void Cxrom32_2026Roster_AppliesExactly_ReapplyIsClean()
        {
            foreach (string rom in Roms32)
                AssertRosterAppliesAndReapplies(rom, TestRoms.GetGeneratedInputPath("TSB1_32-team_input.txt"), 32 * 30);
        }

        [TestMethod]
        public void Cxrom32_2026RosterOldSimData_AppliesExactly_ReapplyIsClean()
        {
            foreach (string rom in Roms32)
                AssertRosterAppliesAndReapplies(rom, TestRoms.GetGeneratedInputPath("TSB1_32_2026_input.txt"), 32 * 30);
        }

        [TestMethod]
        public void Cxrom32_2026Roster18Game_AppliesExactly_ReapplyIsClean()
        {
            foreach (string rom in Roms32)
                AssertRosterAppliesAndReapplies(rom, TestRoms.GetPlayTestPath("TSB1_32-team-18-game_2026.txt"), 32 * 30);
        }

        // Sanity for the matrix: these rosters really do need shortening on every 32-team ROM (otherwise the
        // tests above would only cover the easy path).
        [TestMethod]
        public void Cxrom32_2026Roster_NeedsShortening_OnEveryRom()
        {
            string text = ReadInput(TestRoms.GetGeneratedInputPath("TSB1_32-team_input.txt"));
            foreach (string rom in Roms32)
            {
                RosterNamePlan plan = PlayerNames.PlanFit(TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom)), text);
                Assert.IsTrue(plan.Reports.Any(r => !r.Fits), rom + ":\n" + plan);
            }
        }
    }
}
