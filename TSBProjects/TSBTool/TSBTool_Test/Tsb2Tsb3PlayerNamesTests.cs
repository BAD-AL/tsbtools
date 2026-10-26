using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// PlayerNames (the roster name check NES/SNES TSB1 use) for SNES TSB2 and TSB3: one name area per season
    /// table (TSB2 has 3, TSB3 1), the same plan -- cut unmentioned players to a placeholder, then first names to
    /// an initial, then last names -- one report, and the roster text rewritten so InsertPlayerName's room check
    /// never rejects a name. No length cap: TSB2/TSB3 have none.
    ///
    /// Test data: GeneratedInput\TSB2_2026_input.txt (SEASON 3) / TSB3_2026_input.txt (real 2026 rosters, fit as they
    /// are) and *_long_names.txt (the same with first names lengthened, "josh" -> "josh.josh", so each needs about 300
    /// bytes more than its table has even after placeholders).
    /// </summary>
    [TestClass]
    public class Tsb2Tsb3PlayerNamesTests
    {
        private static readonly Regex teamLine = new Regex("^TEAM\\s*=\\s*([0-9a-zA-Z]+)");
        private static readonly Regex posLine = new Regex("^([A-Z]+[1-5]?)\\s*,\\s*([a-zA-Z \\.\\-]+),");
        private static readonly Regex seasonLine = new Regex("^\\s*SEASON\\s+([1-3])");

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static string Input(string fileName)
        {
            return File.ReadAllText(TestRoms.GetGeneratedInputPath(fileName));
        }

        // The TSB2 files are SEASON 3 rosters; the season tests move them to another season.
        private static string AsSeason(string text, int season)
        {
            StringAssert.Contains(text, "SEASON 3", "sanity");
            return Regex.Replace(text, "^SEASON 3", "SEASON " + season, RegexOptions.Multiline);
        }

        private static TSB2Tool Load(string romName)
        {
            return (TSB2Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
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

        // What MainClass/MainGUI do: plan, apply the plan, then the (rewritten) text. Returns the text applied.
        private static string PlanAndApply(TSB2Tool tool, string text, out RosterNamePlan plan, out List<string> errors)
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

        // Every player line of 'season' in the applied text, for a team the ROM has, must be in the ROM exactly.
        private static void AssertNamesLanded(TSB2Tool tool, string appliedText, int season)
        {
            List<string> mismatches = new List<string>();
            string team = null;
            int currentSeason = 1;
            int checkedNames = 0;
            foreach (string raw in appliedText.Replace("\r\n", "\n").Split('\n'))
            {
                string line = raw.Trim();
                Match m;
                if ((m = seasonLine.Match(line)).Success) { currentSeason = int.Parse(m.Groups[1].Value); continue; }
                if ((m = teamLine.Match(line)).Success) { team = m.Groups[1].Value; continue; }
                if (currentSeason != season || team == null || TSB2Tool.teams.IndexOf(team) < 0)
                    continue;
                if (!(m = posLine.Match(line)).Success || TSB2Tool.positionNames.IndexOf(m.Groups[1].Value) < 0)
                    continue;
                // The ROM splits a name at its first capital: "r mason THOMAS" is first "r mason", last "THOMAS".
                string name = m.Groups[2].Value.Trim();
                int lastStart = Regex.Match(name, "[A-Z]").Index;
                string expected = name.Substring(0, lastStart).Trim() + " " + name.Substring(lastStart);
                byte jersey;
                string actual = tool.GetPlayerName(season, team, m.Groups[1].Value, out jersey);
                checkedNames++;
                if (actual != expected)
                    mismatches.Add(string.Format("{0} {1}: expected '{2}', ROM has '{3}'", team, m.Groups[1].Value, expected, actual));
            }
            Assert.IsTrue(checkedNames > 800, "sanity: names checked = " + checkedNames);
            Assert.AreEqual(0, mismatches.Count, string.Join("\n", mismatches.Take(20).ToArray()));
        }

        // ---- areas ----

        [TestMethod]
        public void Tsb2_ThreeAreas_OnePerSeason_Tsb3_One()
        {
            CollectionAssert.AreEqual(new[] { "TSB2 season 1", "TSB2 season 2", "TSB2 season 3" },
                PlayerNames.PlanFit(Load("TSB2_U.smc"), "").Configs.Select(c => c.PlatformName).ToArray());
            CollectionAssert.AreEqual(new[] { "TSB3" },
                PlayerNames.PlanFit(Load("TSB3.smc"), "").Configs.Select(c => c.PlatformName).ToArray());
        }

        // The planner counts room by adding up the names; InsertPlayerName's check reads the end pointer. They
        // must agree, or a planned roster gets rejected (the NES "1 byte over" bug).
        [TestMethod]
        public void PlannerRoom_MatchesTheToolsOwnRoomCheck()
        {
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
                TSB2Tool tool = Load(rom);
                RosterNamePlan plan = PlayerNames.PlanFit(tool, "");
                for (int i = 0; i < plan.Configs.Count; i++)
                    Assert.AreEqual(tool.GetNameTableFreeBytes(plan.Configs[i].Season), plan.Reports[i].AvailableSlack, rom + " " + plan.Configs[i].PlatformName);
            }
        }

        [TestMethod]
        public void StockDump_ReappliedToItself_NeedsNothing()
        {
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
                TSB2Tool tool = Load(rom);
                RosterNamePlan plan = PlayerNames.PlanFit(tool, tool.GetAll());
                Assert.IsFalse(plan.NeedsAttention, rom + ":\n" + plan);
            }
        }

        // ---- the real 2026 rosters fit as they are ----

        [TestMethod]
        public void Tsb2_2026Roster_Fits_AppliesUnchanged()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            string text = Input("TSB2_2026_input.txt");
            RosterNamePlan plan;
            List<string> errors;
            string applied = PlanAndApply(tool, text, out plan, out errors);

            Assert.IsFalse(plan.NeedsAttention, plan.ToString());
            Assert.AreEqual(text, applied);
            Assert.IsFalse(errors.Any(e => e.Contains("would overflow")), string.Join("\n", errors.ToArray()));
            AssertNamesLanded(tool, applied, 3);
        }

        [TestMethod]
        public void Tsb3_2026Roster_Fits_AppliesUnchanged_IncludingFreeAgents()
        {
            TSB2Tool tool = Load("TSB3.smc");
            string text = Input("TSB3_2026_input.txt");
            RosterNamePlan plan;
            List<string> errors;
            string applied = PlanAndApply(tool, text, out plan, out errors);

            Assert.IsFalse(plan.NeedsAttention, plan.ToString());
            Assert.IsFalse(errors.Any(e => e.Contains("would overflow")), string.Join("\n", errors.ToArray()));
            AssertNamesLanded(tool, applied, 1);
            byte jersey;
            Assert.AreEqual("mitch WISHNOWSKY", tool.GetPlayerName(1, "freeAgents", "P", out jersey));
        }

        // ---- too long: shortened by the plan, then applied with no rejection ----

        private static void AssertLongNamesShortenedAndApplied(string rom, string inputFile, string area, int season)
        {
            TSB2Tool tool = Load(rom);
            RosterNamePlan plan;
            List<string> errors;
            string applied = PlanAndApply(tool, Input(inputFile), out plan, out errors);

            PlayerNamesFitReport report = plan.Reports[plan.Configs.FindIndex(c => c.PlatformName == area)];
            Assert.IsFalse(report.Fits, "sanity: the data needs more room than the table has");
            Assert.IsTrue(report.Suggestions.Count > 0, "names were shortened");
            Assert.AreEqual(0, report.UnresolvedFailures.Count, report.ToString());
            Assert.IsFalse(errors.Any(e => e.Contains("would overflow")), string.Join("\n", errors.ToArray()));
            Assert.IsTrue(tool.GetNameTableFreeBytes(season) >= 0);
            AssertNamesLanded(tool, applied, season);
        }

        [TestMethod]
        public void Tsb2_LongNames_ShortenedToFit_NoRejections()
        {
            AssertLongNamesShortenedAndApplied("TSB2_U.smc", "TSB2_2026_long_names.txt", "TSB2 season 3", 3);
        }

        [TestMethod]
        public void Tsb3_LongNames_ShortenedToFit_NoRejections()
        {
            AssertLongNamesShortenedAndApplied("TSB3.smc", "TSB3_2026_long_names.txt", "TSB3", 1);
        }

        // ---- seasons are separate ----

        // The long roster as season 2: only season 2's area needs attention, and seasons 1 and 3 keep their names.
        [TestMethod]
        public void Tsb2_LongNamesInSeason2_OnlySeason2Shortened_OtherSeasonsUntouched()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            string season1Before = tool.GetTeamPlayers(1, "bills") + tool.GetTeamPlayers(1, "49ers");
            string season3Before = tool.GetTeamPlayers(3, "bills") + tool.GetTeamPlayers(3, "49ers");
            RosterNamePlan plan;
            List<string> errors;
            string applied = PlanAndApply(tool, AsSeason(Input("TSB2_2026_long_names.txt"), 2), out plan, out errors);

            CollectionAssert.AreEqual(new[] { false, true, false }, plan.Reports.Select(r => r.NeedsAttention).ToArray(), plan.ToString());
            Assert.IsFalse(errors.Any(e => e.Contains("would overflow")), string.Join("\n", errors.ToArray()));
            AssertNamesLanded(tool, applied, 2);
            Assert.AreEqual(season1Before, tool.GetTeamPlayers(1, "bills") + tool.GetTeamPlayers(1, "49ers"));
            Assert.AreEqual(season3Before, tool.GetTeamPlayers(3, "bills") + tool.GetTeamPlayers(3, "49ers"));
        }

        // A shortened season 2 name must not rename the same team/position in another season's block.
        [TestMethod]
        public void Tsb2_Rewrite_OnlyTouchesTheSeasonsOwnBlock()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            string season1 = "SEASON 1\r\nTEAM = bills\r\nQB1, josh ALLEN, Face=0x10, #17,\r\n";
            string long2 = AsSeason(Input("TSB2_2026_long_names.txt"), 2);
            RosterNamePlan plan = PlayerNames.PlanFit(tool, season1 + long2);
            string applied = plan.Apply(tool, season1 + long2);

            StringAssert.StartsWith(applied, season1, "season 1's lines are left as they are");
        }
    }
}
