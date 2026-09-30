using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// SNES TSB1 schedule application through SNES_TecmoTool.ApplySchedule -- in particular that a
    /// 32-team NFL schedule is fitted to the ROM's 28 teams by ScheduleTransformer (same wiring as
    /// Genesis_TSB1Tool; see GenesisScheduleTests), while schedules using only valid teams are
    /// written exactly as given. SNES is 18 weeks x 14 games, 16 games per team, and
    /// SNES_ScheduleHelper reports through StaticUtils.AddError.
    /// </summary>
    [TestClass]
    public class SnesScheduleTests
    {
        private const string RomFileName = "TSB1.smc";

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static List<string> SplitToNonEmptyLines(string schedule)
        {
            return schedule
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        private static List<string> Load2026ScheduleLines()
        {
            return SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt")));
        }

        private static void AssertValidTwentyEightTeamSeason(List<string> lines)
        {
            int weeks = lines.Count(l => l.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(18, weeks, "Expected 18 week headers.");

            Dictionary<string, int> gamesPerTeam = new Dictionary<string, int>();
            int gamesThisWeek = 0;
            int totalGames = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase))
                {
                    gamesThisWeek = 0;
                    continue;
                }
                gamesThisWeek++;
                totalGames++;
                Assert.IsTrue(gamesThisWeek <= 14, "A week has more than 14 games.");
                foreach (string team in line.Split(new[] { " at " }, StringSplitOptions.None))
                {
                    if (!gamesPerTeam.ContainsKey(team))
                        gamesPerTeam[team] = 0;
                    gamesPerTeam[team]++;
                }
            }

            Assert.AreEqual(224, totalGames, "Expected 224 games (28 teams x 16 / 2).");
            Assert.AreEqual(28, gamesPerTeam.Count, "Expected all 28 teams: " + string.Join(", ", gamesPerTeam.Keys.ToArray()));
            foreach (KeyValuePair<string, int> pair in gamesPerTeam)
                Assert.AreEqual(16, pair.Value, pair.Key + " should have exactly 16 games.");
        }

        [TestMethod]
        public void SnesRom_StockScheduleRoundTrip_IsByteIdentical()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            tool.ApplySchedule(SplitToNonEmptyLines(tool.GetSchedule()));

            CollectionAssert.AreEqual(originalRom, tool.OutputRom, "Re-applying the ROM's own schedule should not change any bytes.");
        }

        [TestMethod]
        public void SnesRom_ValidTeamsUnevenSchedule_NotTransformed_WarningsReported()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int week1 = lines.IndexOf("WEEK 1");
            string removed = lines[week1 + 1];
            lines.RemoveAt(week1 + 1);
            string[] teams = removed.Split(new[] { " at " }, StringSplitOptions.None);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(lines);
                StaticUtils.ShowErrors();

                string all = string.Join("\n", giver.Errors.ToArray());
                StringAssert.Contains(all, "The " + teams[0] + " have 15 games scheduled.");
                StringAssert.Contains(all, "The " + teams[1] + " have 15 games scheduled.");
                Assert.IsFalse(all.Contains("Schedule adjusted"), "A schedule using only this ROM's teams must be written as given, not transformed.");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        [TestMethod]
        public void SnesRom_Schedule2026_Applied_Has224Games18Weeks16PerTeam()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.ApplySchedule(Load2026ScheduleLines());

            AssertValidTwentyEightTeamSeason(SplitToNonEmptyLines(tool.GetSchedule()));
        }

        [TestMethod]
        public void SnesRom_Schedule2026_RomContentsEqualTransformedLines()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();

            ScheduleTransformResult expected = ScheduleTransformer.Transform(input, ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB1));
            tool.ApplySchedule(input);

            CollectionAssert.AreEqual(expected.TransformedLines, SplitToNonEmptyLines(tool.GetSchedule()));
        }

        [TestMethod]
        public void SnesRom_Schedule2026_ReportsAdjustmentSummaryAndNoOtherWarnings()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB1);
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
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        [TestMethod]
        public void SnesRom_ThirtyTwoTeamScheduleWithTypo_WarnsAndDoesNotAdjust()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> input = Load2026ScheduleLines();
            int typoIndex = input.FindIndex(l => l.StartsWith("steelers at "));
            input[typoIndex] = input[typoIndex].Replace("steelers at ", "stealers at ");

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ApplySchedule(input);
                StaticUtils.ShowErrors();

                string all = string.Join("\n", giver.Errors.ToArray());
                StringAssert.Contains(all, "Unrecognized team 'stealers' in week ");
                StringAssert.Contains(all, "the schedule was not adjusted to fit this ROM");
                Assert.IsFalse(all.Contains("Schedule adjusted"), all);
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
        }

        // B9 for SNES: the 2016-2020 schedules have only 17 weeks. The transformer never adds a
        // week, but SNES_ScheduleHelper's AUTO_CORRECT_SCHEDULE (Ensure18Weeks) splits one, so
        // these should still land as a full 18-week, 16-games-per-team season.
        [TestMethod]
        public void SnesRom_RealSchedules2016To2025_AllApplyAsValidSeasons()
        {
            for (int year = 2016; year <= 2025; year++)
            {
                string fileName = string.Format("{0}-schedule-{1}-games.txt", year, year <= 2020 ? 16 : 17);
                List<string> input = SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRealSchedulePath(fileName)));

                byte[] rom = TestRoms.LoadRom(RomFileName);
                ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
                tool.ApplySchedule(input);

                try
                {
                    AssertValidTwentyEightTeamSeason(SplitToNonEmptyLines(tool.GetSchedule()));
                }
                catch (AssertFailedException e)
                {
                    throw new AssertFailedException(fileName + ": " + e.Message, e);
                }
            }
        }
    }
}
