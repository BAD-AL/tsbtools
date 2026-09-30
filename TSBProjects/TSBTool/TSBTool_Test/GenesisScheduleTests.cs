using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 4 of compressed-riding-rossum.md: season 3's schedule only -- the real 1993 schedule (see
    /// Genesis_ScheduleHelper.cs for why season 3 specifically, and why not season 1 -- matching SNES
    /// TSB1's single-season shape, per the project owner's explicit architecture decision). Season 3
    /// always has exactly 18 weeks / 224 games; seasons 1 and 2 exist in the ROM but are untouched by
    /// this tool.
    /// </summary>
    [TestClass]
    public class GenesisScheduleTests
    {
        // Genesis_ScheduleHelper reports through the shared StaticUtils error list, so keep each
        // test from seeing (or leaving behind) another test's errors.
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        // Season 1 and season 2's data, split into the two disjoint ranges that actually belong to
        // them -- deliberately excludes season 3's OWN games-per-week array/schedule data, which a
        // season-3 write is of course expected to change.
        private static readonly int[,] Season1And2Ranges =
        {
            { 0x52EA, 0x530E }, // season 1 + season 2's games-per-week sentinel arrays
            { 0x5322, 0x56A2 }  // season 1 + season 2's schedule data (448 bytes each)
        };

        private static List<string> SplitToNonEmptyLines(string schedule)
        {
            return schedule
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        [TestMethod]
        public void GenesisRom_Season3Schedule_HasCorrectGameCountsAndTeamBalance()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            List<string> gameLines = lines.Where(l => l.IndexOf(" at ", StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            Assert.AreEqual(224, gameLines.Count, "Season 3 should have exactly 224 games.");

            Dictionary<string, int> gamesPerTeam = new Dictionary<string, int>();
            foreach (string line in gameLines)
            {
                string[] parts = line.Split(new[] { " at " }, StringSplitOptions.None);
                Assert.AreEqual(2, parts.Length, "Malformed game line: " + line);
                foreach (string team in parts)
                {
                    if (!gamesPerTeam.ContainsKey(team))
                        gamesPerTeam[team] = 0;
                    gamesPerTeam[team]++;
                }
            }

            Assert.AreEqual(28, gamesPerTeam.Count, "Every one of the 28 teams should appear in the schedule.");
            foreach (KeyValuePair<string, int> kvp in gamesPerTeam)
            {
                Assert.AreEqual(16, kvp.Value, string.Format("{0} should appear in exactly 16 games, found {1}.", kvp.Key, kvp.Value));
            }
        }

        [TestMethod]
        public void GenesisRom_Season3Week1_MatchesRealNineteenNinetyThreeOpeningWeek()
        {
            // Spot-check against the real 1993 NFL opening week (Buffalo at New England, etc.),
            // decoded directly from the ROM and confirmed against real-world history -- see
            // Genesis_ScheduleHelper.cs's own comment for the full week-1 slate.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int weekIndex = lines.IndexOf("WEEK 1");
            Assert.IsTrue(weekIndex >= 0);

            Assert.AreEqual("bills at patriots", lines[weekIndex + 1]);
            Assert.AreEqual("browns at bengals", lines[weekIndex + 2]);
        }

        [TestMethod]
        public void GenesisRom_ApplyScheduleRoundTrip_IsIdempotent()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            string before = tool.GetSchedule();
            List<string> lines = SplitToNonEmptyLines(before);

            tool.ApplySchedule(lines);

            string after = tool.GetSchedule();
            Assert.AreEqual(before, after, "Schedule text should be unchanged after a decode -> apply -> decode round trip.");

            // Stronger check: re-applying the ROM's own decoded schedule should leave every byte in
            // the whole ROM unchanged, not just the schedule text -- this catches a pointer-table
            // write bug that happens to still decode to the same text (e.g. a wrong week-pointer value
            // that GetSchedule() doesn't actually read, since reading sums the games-per-week sentinel
            // array rather than walking the pointer table).
            int differences = 0;
            for (int i = 0; i < originalRom.Length; i++)
            {
                if (originalRom[i] != tool.OutputRom[i])
                    differences++;
            }
            Assert.AreEqual(0, differences, "Re-applying the ROM's own schedule should not change any bytes.");
        }

        [TestMethod]
        public void GenesisRom_ApplyingDifferentSeason3Schedule_DoesNotAffectSeason1Or2()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            // Swap week 1's two games relative to the stock schedule -- forces the writer to actually
            // run (not just leave matching data alone), while still producing a structurally valid
            // 18-week/224-game schedule (every other week is re-applied unchanged from the decoded
            // original).
            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int week1 = lines.IndexOf("WEEK 1");
            string game1 = lines[week1 + 1];
            string game2 = lines[week1 + 2];
            lines[week1 + 1] = game2;
            lines[week1 + 2] = game1;

            tool.ApplySchedule(lines);

            List<string> after = SplitToNonEmptyLines(tool.GetSchedule());
            CollectionAssert.AreEqual(lines, after, "The modified schedule should read back exactly as applied.");

            int differences = 0;
            for (int range = 0; range < Season1And2Ranges.GetLength(0); range++)
            {
                for (int i = Season1And2Ranges[range, 0]; i < Season1And2Ranges[range, 1]; i++)
                {
                    if (originalRom[i] != tool.OutputRom[i])
                        differences++;
                }
            }
            Assert.AreEqual(0, differences, "Editing season 3's schedule must not touch season 1 or season 2's data.");
        }

        [TestMethod]
        public void GenesisRom_ApplySchedule_UnevenSchedule_ReportsWarningsThroughStaticUtils()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            // Remove week 1's first game ("bills at patriots") -- both teams drop to 15 games, which
            // Genesis_ScheduleHelper flags. Those warnings must reach the user the same way
            // SNES_ScheduleHelper's do (StaticUtils.AddError), not stay trapped in the helper.
            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int week1 = lines.IndexOf("WEEK 1");
            lines.RemoveAt(week1 + 1);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(lines);
                StaticUtils.ShowErrors();

                string all = string.Join("\n", giver.Errors.ToArray());
                StringAssert.Contains(all, "The bills have 15 games scheduled.");
                StringAssert.Contains(all, "The patriots have 15 games scheduled.");
                // Only valid teams, so ScheduleTransformer must not step in and "fix" the counts.
                Assert.IsFalse(all.Contains("Schedule adjusted"), "A schedule using only this ROM's teams must be written as given, not transformed.");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
            }
        }

        // B4: a 32-team/17-game NFL schedule (from the CXROM_18WEEK 2026 port) applied straight
        // through the tool must come out as a valid 28-team Genesis season -- ApplySchedule is
        // expected to run it through ScheduleTransformer before writing.
        [TestMethod]
        public void GenesisRom_Schedule2026_Applied_Has224Games18Weeks16PerTeam()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string text = System.IO.File.ReadAllText(TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt"));
            tool.ApplySchedule(SplitToNonEmptyLines(text));

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int weeks = lines.Count(l => l.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(18, weeks, "Expected 18 week headers.");

            List<string> gameLines = lines.Where(l => l.IndexOf(" at ", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            Assert.AreEqual(224, gameLines.Count, "Expected 224 games (28 teams x 16 / 2).");

            Dictionary<string, int> gamesPerTeam = new Dictionary<string, int>();
            int gamesThisWeek = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase))
                {
                    gamesThisWeek = 0;
                    continue;
                }
                gamesThisWeek++;
                Assert.IsTrue(gamesThisWeek <= 14, "A week has more than 14 games.");
                foreach (string team in line.Split(new[] { " at " }, StringSplitOptions.None))
                {
                    if (!gamesPerTeam.ContainsKey(team))
                        gamesPerTeam[team] = 0;
                    gamesPerTeam[team]++;
                }
            }

            Assert.AreEqual(28, gamesPerTeam.Count, "Expected all 28 Genesis teams: " + string.Join(", ", gamesPerTeam.Keys.ToArray()));
            foreach (KeyValuePair<string, int> pair in gamesPerTeam)
                Assert.AreEqual(16, pair.Value, pair.Key + " should have exactly 16 games.");
        }

        private static List<string> Load2026ScheduleLines()
        {
            return SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt")));
        }

        // B5: what lands in the ROM is exactly what the transformer produced -- same weeks, same
        // games, same order -- so the transformer's own tests really do describe the ROM output.
        [TestMethod]
        public void GenesisRom_Schedule2026_RomContentsEqualTransformedLines()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();

            ScheduleTransformResult expected = ScheduleTransformer.Transform(input, ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1));
            tool.ApplySchedule(input);

            CollectionAssert.AreEqual(expected.TransformedLines, SplitToNonEmptyLines(tool.GetSchedule()));
        }

        // D12: when the schedule gets adjusted, the user is told how -- one summary line whose
        // counts match what the transformer actually did -- and nothing else goes wrong.
        [TestMethod]
        public void GenesisRom_Schedule2026_ReportsAdjustmentSummaryAndNoOtherWarnings()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1);
            ScheduleTransformResult expected = ScheduleTransformer.Transform(input, config);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(input);
                StaticUtils.ShowErrors();

                Assert.AreEqual(1, giver.Errors.Count, "Expected exactly one ShowErrors dialog.");
                // ShowErrors appends "\n" after each queued error.
                Assert.AreEqual(ScheduleTransformer.DescribeAdjustments(expected, config) + "\n", giver.Errors[0],
                    "The summary should be the only message -- the transformed schedule itself should apply cleanly.");
                StringAssert.Contains(giver.Errors[0], "Teams not in this ROM (games dropped): jaguars (");
                StringAssert.Contains(giver.Errors[0], "Renamed: titans -> oilers");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        // Every trimmed and added game goes to the log (too many for the dialog), one line each.
        [TestMethod]
        public void GenesisRom_Schedule2026_LogsEveryTrimAndAdd()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();
            ScheduleTransformResult expected = ScheduleTransformer.Transform(input, ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1));

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(input);

                List<string> expectedLogs = new List<string>();
                foreach (ScheduledGame g in expected.TrimmedGames)
                    expectedLogs.Add(string.Format("Schedule adjustment: trimmed week {0} '{1} at {2}'", g.Week, g.Away, g.Home));
                foreach (ScheduledGame g in expected.DisplacedGames)
                    expectedLogs.Add(string.Format("Schedule adjustment: displaced week {0} '{1} at {2}'", g.Week, g.Away, g.Home));
                foreach (ScheduledGame g in expected.AddedGames)
                    expectedLogs.Add(string.Format("Schedule adjustment: added week {0} '{1} at {2}'{3}", g.Week, g.Away, g.Home,
                        expected.RepeatMakeupGames.Contains(g) ? " (repeat matchup)" : ""));

                List<string> actualLogs = giver.Logs.Where(l => l.StartsWith("Schedule adjustment:")).ToList();
                Assert.IsTrue(expectedLogs.Count > 0, "The 2026 schedule should need at least one trim or add.");
                CollectionAssert.AreEqual(expectedLogs, actualLogs);
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        // A misspelled team in a 32-team schedule: warn by name, and do NOT adjust anything -- no
        // games are made up because of a typo. The helper's own per-line errors still follow.
        [TestMethod]
        public void GenesisRom_ThirtyTwoTeamScheduleWithTypo_WarnsAndDoesNotAdjust()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();
            int typoIndex = input.FindIndex(l => l.EndsWith(" at buccaneers"));
            string original = input[typoIndex];
            input[typoIndex] = original.Replace(" at buccaneers", " at buccaners");
            int typoWeek = WeekOfLine(input, typoIndex);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(input);
                StaticUtils.ShowErrors();

                string all = string.Join("\n", giver.Errors.ToArray());
                StringAssert.Contains(all, string.Format("Unrecognized team 'buccaners' in week {0}: '{1}'", typoWeek, input[typoIndex]));
                StringAssert.Contains(all, "the schedule was not adjusted to fit this ROM");
                Assert.IsFalse(all.Contains("Schedule adjusted"), all);
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        // The hand-edited case: a valid 28-team schedule with one misspelled name. The typo'd game is
        // rejected by the helper as always -- and no made-up game replaces it.
        [TestMethod]
        public void GenesisRom_TwentyEightTeamScheduleWithTypo_WarnsAndDoesNotAdjust()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = SplitToNonEmptyLines(tool.GetSchedule());
            int week1 = input.IndexOf("WEEK 1");
            string original = input[week1 + 1]; // "bills at patriots"
            input[week1 + 1] = original.Replace("patriots", "patriotz");

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(input);
                StaticUtils.ShowErrors();

                string all = string.Join("\n", giver.Errors.ToArray());
                StringAssert.Contains(all, "Unrecognized team 'patriotz' in week 1: 'bills at patriotz'");
                Assert.IsFalse(all.Contains("Schedule adjusted"), all);
                StringAssert.Contains(all, "The bills have 15 games scheduled.");
                StringAssert.Contains(all, "The patriots have 15 games scheduled.");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        private static int WeekOfLine(List<string> lines, int index)
        {
            for (int i = index; i >= 0; i--)
                if (lines[i].StartsWith("WEEK ", StringComparison.OrdinalIgnoreCase))
                    return int.Parse(lines[i].Substring(5));
            return -1;
        }

        // D13: a 32-team schedule cut down to 2 weeks looks like incomplete input (e.g. a partial
        // paste), not a season to fill with made-up games. It's written as given, with a warning
        // about the week count, and the helper's normal shortfall warnings still reach the user --
        // an uneven schedule is never shipped silently.
        [TestMethod]
        public void GenesisRom_TruncatedThirtyTwoTeamSchedule_WarnsAndShortfallIsReported()
        {
            List<string> input = Load2026ScheduleLines();
            int week3 = input.IndexOf("WEEK 3");
            input.RemoveRange(week3, input.Count - week3);

            string all = ApplyAndCollectMessages(input);
            StringAssert.Contains(all, "Schedule has 2 weeks; this ROM holds 18 -- the schedule was not adjusted to fit this ROM; check that the whole season was included.");
            Assert.IsFalse(all.Contains("Schedule adjusted"), all);
            StringAssert.Contains(all, "The bills have ");
            StringAssert.Contains(all, "didn't schedule all 18 weeks");
        }

        // The week-count limit: off by ONE week is adjusted (see the 2016-2020 test below); off by
        // two is treated as incomplete input and not adjusted.
        [TestMethod]
        public void GenesisRom_ThirtyTwoTeamScheduleTwoWeeksShort_WarnsAndDoesNotAdjust()
        {
            List<string> input = SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRealSchedulePath("2016-schedule-16-games.txt")));
            int week17 = input.IndexOf("WEEK 17");
            input.RemoveRange(week17, input.Count - week17); // 16 weeks

            string all = ApplyAndCollectMessages(input);
            StringAssert.Contains(all, "Schedule has 16 weeks; this ROM holds 18 -- the schedule was not adjusted");
            Assert.IsFalse(all.Contains("Schedule adjusted"), all);
        }

        private static string ApplyAndCollectMessages(List<string> input)
        {
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(input);
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            return string.Join("\n", giver.Errors.ToArray());
        }

        // All ten real schedules -- including the 17-week 2016-2020 ones, which get an 18th week
        // added -- land as full 18-week Genesis seasons, with only the adjustment summary reported.
        [TestMethod]
        public void GenesisRom_RealSchedules2016To2025_ApplyAsFullSeasonsWithNoHelperWarnings()
        {
            for (int year = 2016; year <= 2025; year++)
            {
                string fileName = string.Format("{0}-schedule-{1}-games.txt", year, year <= 2020 ? 16 : 17);
                List<string> input = SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRealSchedulePath(fileName)));

                byte[] rom = TestRoms.LoadRom(RomFileName);
                ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
                TestMessageGiver giver = new TestMessageGiver();
                StaticUtils.ClearErrors();
                StaticUtils.sMessageGiver = giver;
                try
                {
                    tool.ApplySchedule(input);
                    StaticUtils.ShowErrors();
                }
                finally
                {
                    StaticUtils.sMessageGiver = null;
                }

                string all = string.Join("\n", giver.Errors.ToArray());
                Assert.AreEqual(1, giver.Errors.Count, fileName + ": expected only the adjustment summary:\n" + all);
                StringAssert.StartsWith(giver.Errors[0], "Schedule adjusted", fileName);
                if (year <= 2020)
                    StringAssert.Contains(all, "Added empty week 18", fileName);

                List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
                Assert.AreEqual(18, lines.Count(l => l.StartsWith("WEEK")), fileName + ": weeks.");
                Assert.AreEqual(224, lines.Count(l => l.Contains(" at ")), fileName + ": games.");
            }
        }

        [TestMethod]
        public void GenesisRom_ApplySchedule_StockSchedule_ReportsNoWarnings()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(lines);
                StaticUtils.ShowErrors();

                Assert.AreEqual(0, giver.Errors.Count, "Stock schedule should apply cleanly: " + string.Join(" | ", giver.Errors.ToArray()));
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
            }
        }
    }
}
