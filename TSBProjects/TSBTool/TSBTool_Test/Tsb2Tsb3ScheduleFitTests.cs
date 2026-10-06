using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Fitting modern schedules onto TSB2 and TSB3, across many sources. TSB2Tool and TSB3Tool are
    /// both wired to ScheduleTransformer.FitToRom (see also CrossRomScheduleTests); TSB3's exactly-full
    /// weeks were confirmed in an emulator (full simulated seasons). Each test fits a source schedule with
    /// ScheduleTransformer.ForRom(SNES_TSB2 / SNES_TSB3, season), then loads the fitted text into a
    /// real ROM through the tool's normal text path (ProcessText with a "SEASON n" line -- exactly
    /// what loading the text into the program does), and checks the ROM reads back exactly what the
    /// transformer produced, as a valid season, with no schedule errors, and without disturbing the
    /// ROM's other stored seasons.
    ///
    /// Sources: the 18-week CXROM ROM's own schedule, the 2026 schedule, and 2016-2025.
    ///   TSB2: 28 teams, 14 games/week; seasons 1 and 3 are 17 weeks, season 2 is 18.
    ///   TSB3: 30 teams, 17 weeks, a fixed slot count per week adding up to exactly 240 -- so every
    ///         week must be exactly full.
    /// </summary>
    [TestClass]
    public class Tsb2Tsb3ScheduleFitTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static List<string> SplitToNonEmptyLines(string schedule)
        {
            return schedule.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        }

        // name -> schedule lines. The CXROM source is read through its own tool (GetSchedule), like a
        // user copying a schedule out of one ROM to paste into another.
        internal static List<KeyValuePair<string, List<string>>> LoadSources()
        {
            List<KeyValuePair<string, List<string>>> sources = new List<KeyValuePair<string, List<string>>>();
            ITecmoTool cxrom = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes"));
            sources.Add(new KeyValuePair<string, List<string>>("cxrom-18week", SplitToNonEmptyLines(cxrom.GetSchedule())));
            sources.Add(new KeyValuePair<string, List<string>>("2026",
                SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt")))));
            for (int year = 2016; year <= 2025; year++)
            {
                string file = string.Format("{0}-schedule-{1}-games.txt", year, year <= 2020 ? 16 : 17);
                sources.Add(new KeyValuePair<string, List<string>>(year.ToString(),
                    SplitToNonEmptyLines(System.IO.File.ReadAllText(TestRoms.GetRealSchedulePath(file)))));
            }
            return sources;
        }

        // Fits `source` for (romType, season), loads it into a fresh copy of romFile, and returns a
        // description of every problem found (empty = clean).
        private static List<string> FitAndLoad(string romFile, ROM_TYPE romType, int season, int seasons, List<string> source)
        {
            List<string> problems = new List<string>();
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(romType, season);
            ScheduleTransformResult result = ScheduleTransformer.Transform(source, config);

            problems.AddRange(SeasonProblems(result.TransformedLines, config));

            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romFile));
            Assert.AreEqual(romType, tool.RomVersion);
            Dictionary<int, string> otherSeasonsBefore = new Dictionary<int, string>();
            for (int s = 1; s <= seasons; s++)
                if (s != season)
                    otherSeasonsBefore[s] = tool.GetSchedule(s);

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ProcessText("SEASON " + season + "\n" + string.Join("\n", result.TransformedLines.ToArray()) + "\n");
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            foreach (string error in giver.Errors)
                foreach (string line in SplitToNonEmptyLines(error))
                    problems.Add("tool message: " + line);

            List<string> readBack = SplitToNonEmptyLines(tool.GetSchedule(season));
            if (!readBack.SequenceEqual(result.TransformedLines))
                problems.Add("ROM read-back differs from the transformer's output");
            foreach (KeyValuePair<int, string> other in otherSeasonsBefore)
                if (tool.GetSchedule(other.Key) != other.Value)
                    problems.Add("season " + other.Key + " changed");
            return problems;
        }

        // Week count, 16 games each, per-week caps, no double-booking, no empty week.
        internal static List<string> SeasonProblems(List<string> lines, ScheduleTransformerConfig config)
        {
            List<string> problems = new List<string>();
            Dictionary<string, int> counts = new Dictionary<string, int>();
            List<int> gamesInWeek = new List<int>();
            HashSet<string> busy = new HashSet<string>();
            foreach (string line in lines)
            {
                if (line.StartsWith("WEEK "))
                {
                    gamesInWeek.Add(0);
                    busy.Clear();
                    continue;
                }
                gamesInWeek[gamesInWeek.Count - 1]++;
                foreach (string team in line.Split(new[] { " at " }, StringSplitOptions.None))
                {
                    if (!busy.Add(team))
                        problems.Add(string.Format("week {0}: {1} plays twice", gamesInWeek.Count, team));
                    counts[team] = (counts.ContainsKey(team) ? counts[team] : 0) + 1;
                }
            }
            if (gamesInWeek.Count != config.Weeks)
                problems.Add(string.Format("{0} weeks, expected {1}", gamesInWeek.Count, config.Weeks));
            for (int w = 0; w < gamesInWeek.Count; w++)
            {
                int cap = config.WeekCaps != null && w < config.WeekCaps.Length ? config.WeekCaps[w] : config.MaxGamesPerWeek;
                if (gamesInWeek[w] == 0)
                    problems.Add(string.Format("week {0} is empty", w + 1));
                if (gamesInWeek[w] > cap)
                    problems.Add(string.Format("week {0} has {1} games, cap {2}", w + 1, gamesInWeek[w], cap));
            }
            foreach (string team in config.ValidTeams)
            {
                int n = counts.ContainsKey(team) ? counts[team] : 0;
                if (n != config.GamesPerTeam)
                    problems.Add(string.Format("{0} has {1} games", team, n));
            }
            return problems;
        }

        private static void AssertAllSourcesClean(string romFile, ROM_TYPE romType, int season, int seasons)
        {
            List<string> report = new List<string>();
            foreach (KeyValuePair<string, List<string>> source in LoadSources())
            {
                List<string> problems = FitAndLoad(romFile, romType, season, seasons, source.Value);
                if (problems.Count > 0)
                    report.Add(source.Key + ": " + string.Join("; ", problems.Distinct().Take(6).ToArray()) +
                        (problems.Count > 6 ? string.Format(" (+{0} more)", problems.Count - 6) : ""));
            }
            Assert.AreEqual(0, report.Count, romType + " season " + season + ":\n" + string.Join("\n", report.ToArray()));
        }

        [TestMethod]
        public void Tsb2Season1_AllSourcesFitAndLoadCleanly()
        {
            AssertAllSourcesClean("TSB2_U.smc", ROM_TYPE.SNES_TSB2, 1, 3);
        }

        [TestMethod]
        public void Tsb2Season2_AllSourcesFitAndLoadCleanly()
        {
            AssertAllSourcesClean("TSB2_U.smc", ROM_TYPE.SNES_TSB2, 2, 3);
        }

        [TestMethod]
        public void Tsb2Season3_AllSourcesFitAndLoadCleanly()
        {
            AssertAllSourcesClean("TSB2_U.smc", ROM_TYPE.SNES_TSB2, 3, 3);
        }

        [TestMethod]
        public void Tsb3_AllSourcesFitAndLoadCleanly()
        {
            AssertAllSourcesClean("TSB3.smc", ROM_TYPE.SNES_TSB3, 1, 1);
        }
    }
}
