using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// End-to-end acceptance for moving a schedule between ROMs: read the schedule out of the 32-team
    /// / 18-week / 17-game CXROM_18WEEK ROM (SbluemanBase_18week_RealNFL-1.nes) with GetSchedule(),
    /// and apply it to a fresh copy of every other ROM type. Each target must end up with a valid
    /// season for that ROM -- its own team set, week count, and games per team, and no team playing
    /// twice in one week.
    ///
    /// Targets and what makes them work (see ScheduleTransformer.FitToRom):
    ///   Genesis, SNES TSB1 (28 teams / 18 weeks / 16 games): drop the 4 teams the ROM doesn't have,
    ///     trim to 16, make up the rest.
    ///   NES original (28 / 17 / 16): the same, plus removing the final week.
    ///   CXROM v105 / v111 (32 / 17 / 16): no teams to drop; removing the final week alone.
    /// TSB2/TSB3 are out of scope (their own ApplySchedule(season, ...) and parser).
    /// </summary>
    [TestClass]
    public class CrossRomScheduleTests
    {
        private const string SourceRom = "SbluemanBase_18week_RealNFL-1.nes";

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

        // Must run BEFORE the target tool is created: TecmoToolFactory replaces the shared static
        // TecmoTool.Teams list on every call, and GetSchedule() decodes team indexes through it.
        private static List<string> ReadSourceSchedule()
        {
            ITecmoTool source = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(SourceRom));
            Assert.AreEqual(ROM_TYPE.CXROM_18WEEK, source.RomVersion);
            return SplitToNonEmptyLines(source.GetSchedule());
        }

        private static void ApplySourceScheduleAndAssertValidSeason(string targetRom, ROM_TYPE expectedType,
            int weeks, int gamesPerTeam, int maxGamesPerWeek)
        {
            List<string> schedule = ReadSourceSchedule();

            ITecmoTool target = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(targetRom));
            Assert.AreEqual(expectedType, target.RomVersion);
            // CXROM's team list also carries the Pro Bowl "AFC"/"NFC" pseudo-teams, which never play
            // a scheduled game.
            List<string> targetTeams = TecmoTool.Teams.Where(t => t != "AFC" && t != "NFC").ToList();

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                target.ApplySchedule(schedule);
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            string messages = string.Join("\n", giver.Errors.ToArray());

            List<string> result = SplitToNonEmptyLines(target.GetSchedule());
            string context = targetRom + " -- messages:\n" + messages;

            // A clean transfer reports at most the adjustment summary -- no schedule-helper errors or
            // warnings (e.g. a rejected extra week that just happens to leave a valid season behind).
            List<string> otherMessages = SplitToNonEmptyLines(messages)
                .Where(l => !l.StartsWith("Schedule adjusted") && !l.StartsWith("Teams not in this ROM")
                         && !l.StartsWith("Renamed:") && !l.StartsWith("Removed week") && !l.StartsWith("Added empty week")
                         && !l.Contains("makeup games added"))
                .ToList();
            Assert.AreEqual(0, otherMessages.Count, "Unexpected schedule messages. " + context);

            Assert.AreEqual(weeks, result.Count(l => l.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase)), "Week count. " + context);

            Dictionary<string, int> gamesByTeam = new Dictionary<string, int>();
            List<string> doubleBooked = new List<string>();
            HashSet<string> busyThisWeek = new HashSet<string>();
            string week = null;
            int gamesThisWeek = 0;
            foreach (string line in result)
            {
                if (line.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase))
                {
                    week = line;
                    busyThisWeek.Clear();
                    gamesThisWeek = 0;
                    continue;
                }
                gamesThisWeek++;
                Assert.IsTrue(gamesThisWeek <= maxGamesPerWeek, week + " has more than " + maxGamesPerWeek + " games. " + context);
                foreach (string team in line.Split(new[] { " at " }, StringSplitOptions.None))
                {
                    if (!busyThisWeek.Add(team))
                        doubleBooked.Add(week + ": " + team);
                    gamesByTeam[team] = (gamesByTeam.ContainsKey(team) ? gamesByTeam[team] : 0) + 1;
                }
            }

            Assert.AreEqual(0, doubleBooked.Count, "Teams double-booked: " + string.Join(", ", doubleBooked.ToArray()) + ". " + context);
            CollectionAssert.AreEquivalent(targetTeams, gamesByTeam.Keys.ToList(), "Every one of the target ROM's teams should play. " + context);
            foreach (KeyValuePair<string, int> pair in gamesByTeam)
                Assert.AreEqual(gamesPerTeam, pair.Value, pair.Key + " games. " + context);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToGenesis_IsValidSeason()
        {
            ApplySourceScheduleAndAssertValidSeason(@"Genesis\Tecmo Super Bowl (USA) (October 1993).md", ROM_TYPE.GENESIS_TSB1, 18, 16, 14);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToSnesTsb1_IsValidSeason()
        {
            ApplySourceScheduleAndAssertValidSeason("TSB1.smc", ROM_TYPE.SNES_TSB1, 18, 16, 14);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToNesOriginal_IsValidSeason()
        {
            ApplySourceScheduleAndAssertValidSeason("TSPRBOWL.nes", ROM_TYPE.NES_ORIGINAL_TSB, 17, 16, 14);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToCxRomV105_IsValidSeason()
        {
            ApplySourceScheduleAndAssertValidSeason("TSB 2007-32-105.nes", ROM_TYPE.CXROM_v105, 17, 16, 16);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToCxRomV111_IsValidSeason()
        {
            ApplySourceScheduleAndAssertValidSeason("TSB 2007-32-111.nes", ROM_TYPE.CXROM_v111, 17, 16, 16);
        }

        // TSB2 stores three seasons (17, 18, and 17 weeks); the schedule is loaded through the
        // tool's normal text path with a "SEASON n" line, the way the program loads it.
        private static void ApplySourceScheduleToTsb2Season(int season, int weeks)
        {
            List<string> schedule = ReadSourceSchedule();

            ITecmoContent target = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc"));
            Assert.AreEqual(ROM_TYPE.SNES_TSB2, target.RomVersion);
            Dictionary<int, string> otherSeasons = new Dictionary<int, string>();
            for (int s = 1; s <= 3; s++)
                if (s != season)
                    otherSeasons[s] = target.GetSchedule(s);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                target.ProcessText("SEASON " + season + "\n" + string.Join("\n", schedule.ToArray()) + "\n");
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            string messages = string.Join("\n", giver.Errors.ToArray());
            string context = "TSB2 season " + season + " -- messages:\n" + messages;

            StringAssert.Contains(messages, "Schedule adjusted to fit this ROM's 28 teams:", context);
            List<string> otherMessages = SplitToNonEmptyLines(messages)
                .Where(l => !l.StartsWith("Schedule adjusted") && !l.StartsWith("Teams not in this ROM")
                         && !l.StartsWith("Renamed:") && !l.StartsWith("Removed week") && !l.StartsWith("Added empty week")
                         && !l.Contains("makeup games added"))
                .ToList();
            Assert.AreEqual(0, otherMessages.Count, "Unexpected schedule messages. " + context);

            List<string> result = SplitToNonEmptyLines(target.GetSchedule(season));
            List<string> problems = Tsb2Tsb3ScheduleFitTests.SeasonProblems(result, ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB2, season));
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems.ToArray()) + ". " + context);
            Assert.AreEqual(weeks, result.Count(l => l.StartsWith("WEEK")), context);
            foreach (KeyValuePair<int, string> other in otherSeasons)
                Assert.AreEqual(other.Value, target.GetSchedule(other.Key), "Season " + other.Key + " must not change. " + context);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToTsb2Season1_IsValidSeason()
        {
            ApplySourceScheduleToTsb2Season(1, 17);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToTsb2Season2_IsValidSeason()
        {
            ApplySourceScheduleToTsb2Season(2, 18);
        }

        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToTsb2Season3_IsValidSeason()
        {
            ApplySourceScheduleToTsb2Season(3, 17);
        }

        // TSB3: 30 teams, 17 weeks, every week exactly full (one bye per team, weeks 4-11).
        [TestMethod]
        public void EighteenWeekCxRomSchedule_AppliedToTsb3_IsExactlyFullSeason()
        {
            List<string> schedule = ReadSourceSchedule();

            ITecmoContent target = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc"));
            Assert.AreEqual(ROM_TYPE.SNES_TSB3, target.RomVersion);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                target.ProcessText("SEASON 1\n" + string.Join("\n", schedule.ToArray()) + "\n");
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            string messages = string.Join("\n", giver.Errors.ToArray());
            string context = "TSB3 -- messages:\n" + messages;

            StringAssert.Contains(messages, "Schedule adjusted to fit this ROM's 30 teams:", context);
            StringAssert.Contains(messages, "Teams not in this ROM (games dropped): ravens (", context);
            List<string> otherMessages = SplitToNonEmptyLines(messages)
                .Where(l => !l.StartsWith("Schedule adjusted") && !l.StartsWith("Teams not in this ROM")
                         && !l.StartsWith("Renamed:") && !l.StartsWith("Removed week") && !l.StartsWith("Added empty week")
                         && !l.Contains("makeup games added"))
                .ToList();
            Assert.AreEqual(0, otherMessages.Count, "Unexpected schedule messages. " + context);

            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB3);
            List<string> result = SplitToNonEmptyLines(target.GetSchedule(1));
            List<string> problems = Tsb2Tsb3ScheduleFitTests.SeasonProblems(result, config);
            Assert.AreEqual(0, problems.Count, string.Join("; ", problems.ToArray()) + ". " + context);

            // Exactly full: each week holds exactly its slot count.
            List<int> gamesInWeek = new List<int>();
            foreach (string line in result)
            {
                if (line.StartsWith("WEEK"))
                    gamesInWeek.Add(0);
                else
                    gamesInWeek[gamesInWeek.Count - 1]++;
            }
            CollectionAssert.AreEqual(config.WeekCaps, gamesInWeek.ToArray(), "Every TSB3 week should be exactly full. " + context);
        }

        [TestMethod]
        public void Tsb3_OwnScheduleReapplied_IsNotAdjusted()
        {
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc"));
            string before = tool.GetSchedule(1);
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ProcessText("SEASON 1\n" + before + "\n");
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            string messages = string.Join("\n", giver.Errors.ToArray());
            Assert.IsFalse(messages.Contains("Schedule adjusted"), messages);
            Assert.AreEqual(before, tool.GetSchedule(1));
        }

        // A valid 28-team TSB2 schedule (the ROM's own) is never adjusted: re-applying it through the
        // text path leaves the season exactly as it was, with no adjustment summary.
        [TestMethod]
        public void Tsb2_OwnScheduleReapplied_IsNotAdjusted()
        {
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc"));
            for (int season = 1; season <= 3; season++)
            {
                string before = tool.GetSchedule(season);
                TestMessageGiver giver = new TestMessageGiver();
                StaticUtils.ClearErrors();
                StaticUtils.sMessageGiver = giver;
                try
                {
                    tool.ProcessText("SEASON " + season + "\n" + before + "\n");
                    StaticUtils.ShowErrors();
                }
                finally
                {
                    StaticUtils.sMessageGiver = null;
                }
                string messages = string.Join("\n", giver.Errors.ToArray());
                Assert.IsFalse(messages.Contains("Schedule adjusted"), "season " + season + ": " + messages);
                Assert.AreEqual(before, tool.GetSchedule(season), "season " + season);
            }
        }
    }
}
