using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Core\ScheduleTransformer.cs -- pure text-in/text-out, ROM-agnostic. Synthetic small-config
    /// tests cover each mechanic in isolation; the real-2026-schedule tests at the bottom cover the
    /// actual motivating case (32-team/17-game modern schedule -> 28-team/16-game Genesis TSB1).
    /// </summary>
    [TestClass]
    public class ScheduleTransformerTests
    {
        [TestMethod]
        public void Transform_RenamesTeamsBeforeValidityCheck()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b" };
            config.TeamRenames["titans"] = "a";
            config.GamesPerTeam = 1;
            config.MaxGamesPerWeek = 10;

            List<string> lines = new List<string> { "WEEK 1", "titans at b" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(0, result.DroppedGames.Count, "A renamed team that resolves to a valid team must not be dropped.");
            CollectionAssert.Contains(result.TransformedLines, "a at b");
        }

        [TestMethod]
        public void Transform_DropsGamesInvolvingTeamsNotInValidTeams()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b" };
            config.GamesPerTeam = 1; // "a at b" alone already puts both at target -- isolates the drop from any trim/add side effects
            config.MaxGamesPerWeek = 10;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "a at jaguars" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(1, result.DroppedGames.Count);
            Assert.AreEqual("jaguars", result.DroppedGames[0].Home);
            StringAssert.Contains(string.Join("\n", result.TransformedLines.ToArray()), "a at b");
            Assert.IsFalse(string.Join("\n", result.TransformedLines.ToArray()).Contains("jaguars"));
        }

        [TestMethod]
        public void Transform_PreservesUntouchedGamesExactly_SameWeekSameText()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 1;
            config.MaxGamesPerWeek = 10;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "WEEK 2", "c at d" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(0, result.DroppedGames.Count);
            Assert.AreEqual(0, result.TrimmedGames.Count);
            Assert.AreEqual(0, result.AddedGames.Count);
            CollectionAssert.AreEqual(lines, result.TransformedLines, "Every team is already exactly at target -- nothing should change at all.");
        }

        [TestMethod]
        public void Transform_TrimsTeamsOverTarget_PreferringBothOverPairsFirst()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 1;
            config.MaxGamesPerWeek = 10;

            // a and b are both over target (2 games each, via a-b plus one more each); c and d are
            // exactly at target (1 game each). The a-vs-b game is the only pairing where BOTH sides
            // are over target, so it's the one that should be trimmed (fixes two teams with one
            // removal), leaving a-vs-c and b-vs-d untouched.
            List<string> lines = new List<string> { "WEEK 1", "a at b", "a at c", "b at d" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(1, result.TrimmedGames.Count);
            Assert.AreEqual("a", result.TrimmedGames[0].Away);
            Assert.AreEqual("b", result.TrimmedGames[0].Home);
            Assert.AreEqual(1, result.FinalGameCounts["a"]);
            Assert.AreEqual(1, result.FinalGameCounts["b"]);
            Assert.AreEqual(1, result.FinalGameCounts["c"]);
            Assert.AreEqual(1, result.FinalGameCounts["d"]);
        }

        [TestMethod]
        public void Transform_AddsMakeupGames_PairingTwoStillShortTeamsAndAvoidingRematches()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 10;

            // a-b and c-d already played once each; everyone needs exactly one more game. The
            // makeup game(s) must not repeat a-b or c-d while a fresh pairing (a-c, a-d, b-c, or
            // b-d) is still available. Week 2 is empty so the makeup games have a week where the
            // teams are free (a team never plays twice in one week).
            List<string> lines = new List<string> { "WEEK 1", "a at b", "c at d", "WEEK 2" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(0, result.RepeatMakeupGames.Count, "A fresh pairing was available for everyone -- no repeat should have been needed.");
            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
            foreach (KeyValuePair<string, int> kv in result.FinalGameCounts)
                Assert.AreEqual(2, kv.Value, string.Format("{0} should have exactly 2 games.", kv.Key));
        }

        // b and d each need one more game. Every week has one game, so "lightest week" alone would
        // pick week 1 -- where b already plays. The only legal slot is week 3, where both are free.
        [TestMethod]
        public void Transform_MakeupGame_GoesInAWeekWhereBothTeamsAreFree()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 10;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "WEEK 2", "c at d", "WEEK 3", "a at c" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(1, result.AddedGames.Count);
            Assert.AreEqual(3, result.AddedGames[0].Week, "The makeup game must go in the only week where both b and d are free.");
            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
            foreach (KeyValuePair<string, int> kv in result.FinalGameCounts)
                Assert.AreEqual(2, kv.Value, kv.Key + " should have exactly 2 games.");
        }

        // e and f each need one more game, but they've already played each other, and in week 3 --
        // the only week with free teams besides them -- the free teams (a, c) are already at target.
        // A fresh fix needs a chain: e plays a in week 3, a gives up its week-2 game vs c, and c
        // (now short) plays f in week 3. The given-up a-c game is reported as displaced, and the
        // e-f rematch is never used because a fresh chain exists.
        [TestMethod]
        public void Transform_ShortTeamsWithOnlyARematchAvailable_UseFreshChainAndReportDisplacedGame()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d", "e", "f" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 10;

            // Counts: a2 b2 c2 d2 e1 f1. Week 1: e, f free.  Week 2: b, d free.  Week 3: a, c, e, f free.
            List<string> lines = new List<string> {
                "WEEK 1", "a at b", "c at d",
                "WEEK 2", "a at c", "e at f",
                "WEEK 3", "b at d" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
            foreach (KeyValuePair<string, int> kv in result.FinalGameCounts)
                Assert.AreEqual(2, kv.Value, kv.Key + " should have exactly 2 games.");
            Assert.AreEqual(0, result.RepeatMakeupGames.Count, "A fresh chain exists, so no repeat matchup should be used.");
            Assert.IsTrue(result.DisplacedGames.Count > 0, "Reaching e without a repeat requires displacing an existing game.");
            foreach (ScheduledGame g in result.DisplacedGames)
                Assert.IsFalse(result.TransformedLines.Contains(g.Away + " at " + g.Home), "Displaced game still present: " + g.Away + " at " + g.Home);
        }

        // ---- Weeks: fitting the season length ----

        // Too many weeks: the LAST weeks are removed (games and all), reported, and the teams that
        // lost a game there get made up in the weeks that remain.
        [TestMethod]
        public void Transform_TooManyWeeks_RemovesLastWeeksAndReportsThem()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 10;
            config.Weeks = 2;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "c at d", "WEEK 2", "a at c", "b at d", "WEEK 3", "a at d", "b at c" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            CollectionAssert.AreEqual(new List<int> { 3 }, result.RemovedWeeks);
            Assert.AreEqual(2, result.RemovedWeekGames.Count);
            CollectionAssert.AreEqual(new List<string> { "WEEK 1", "a at b", "c at d", "WEEK 2", "a at c", "b at d" }, result.TransformedLines,
                "Removing week 3 leaves everyone at exactly 2 games -- nothing else should change.");
        }

        // Too few weeks: empty weeks are added at the end BEFORE makeup games are placed, so the
        // makeup games have somewhere legal to go.
        [TestMethod]
        public void Transform_TooFewWeeks_AddsEmptyWeeksForMakeupGames()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 10;
            config.Weeks = 2;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "c at d" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            CollectionAssert.AreEqual(new List<int> { 2 }, result.AddedWeeks);
            Assert.AreEqual(2, result.TransformedLines.FindAll(l => l.StartsWith("WEEK ")).Count);
            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
            foreach (KeyValuePair<string, int> kv in result.FinalGameCounts)
                Assert.AreEqual(2, kv.Value, kv.Key + " should have exactly 2 games.");
        }

        [TestMethod]
        public void Transform_WeeksUnset_LeavesSeasonLengthAlone()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b" };
            config.GamesPerTeam = 1;
            config.MaxGamesPerWeek = 10;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "WEEK 2" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            CollectionAssert.AreEqual(lines, result.TransformedLines);
            Assert.AreEqual(0, result.RemovedWeeks.Count);
            Assert.AreEqual(0, result.AddedWeeks.Count);
        }

        [TestMethod]
        public void DescribeAdjustments_IncludesWeekChanges()
        {
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.NES_ORIGINAL_TSB);
            ScheduleTransformResult result = new ScheduleTransformResult();
            result.RemovedWeeks.Add(18);
            result.RemovedWeekGames.Add(new ScheduledGame(18, "bills", "jets"));
            result.RemovedWeekGames.Add(new ScheduledGame(18, "colts", "rams"));
            string text = ScheduleTransformer.DescribeAdjustments(result, config);
            StringAssert.Contains(text, "  Removed week 18 (this ROM holds 17 weeks) -- 2 games");

            result = new ScheduleTransformResult();
            result.AddedWeeks.Add(18);
            text = ScheduleTransformer.DescribeAdjustments(result, ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1));
            StringAssert.Contains(text, "  Added empty week 18 (this ROM holds 18 weeks)");
        }

        // Every 18-week real schedule, fitted to NES (28 teams, 17 weeks): a valid season.
        [TestMethod]
        public void Transform_EighteenWeekRealSchedules_NesConfig_ValidSeventeenWeekSeasons()
        {
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.NES_ORIGINAL_TSB);
            List<string> files = new List<string> { TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt") };
            for (int year = 2021; year <= 2025; year++)
                files.Add(TestRoms.GetRealSchedulePath(year + "-schedule-17-games.txt"));
            foreach (string file in files)
            {
                ScheduleTransformResult result = ScheduleTransformer.Transform(new List<string>(System.IO.File.ReadAllLines(file)), config);
                string name = System.IO.Path.GetFileName(file);
                CollectionAssert.AreEqual(new List<int> { 18 }, result.RemovedWeeks, name);
                AssertValidSeason(result.TransformedLines, 17, 16, 14, name);
            }
        }

        // Every 18-week real schedule, fitted to CXROM v105/v111 (32 teams, 17 weeks): removing the
        // last week is the whole adjustment -- nothing dropped, trimmed, displaced, or added.
        [TestMethod]
        public void Transform_EighteenWeekRealSchedules_CxRomConfig_OnlyLastWeekRemoved()
        {
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.CXROM_v105);
            for (int year = 2021; year <= 2025; year++)
            {
                string name = year + "-schedule-17-games.txt";
                ScheduleTransformResult result = ScheduleTransformer.Transform(new List<string>(System.IO.File.ReadAllLines(TestRoms.GetRealSchedulePath(name))), config);
                CollectionAssert.AreEqual(new List<int> { 18 }, result.RemovedWeeks, name);
                Assert.AreEqual(0, result.DroppedGames.Count + result.TrimmedGames.Count + result.DisplacedGames.Count + result.AddedGames.Count,
                    name + ": only the week-18 removal should be needed.");
                AssertValidSeason(result.TransformedLines, 17, 16, 16, name);
            }
        }

        // weeks, games per team, per-week cap, no double-booking, and no empty week (the schedule
        // helpers reject a week with 0 games).
        private static void AssertValidSeason(List<string> lines, int weeks, int gamesPerTeam, int maxPerWeek, string name)
        {
            AssertNoTeamPlaysTwiceInAWeek(lines);
            Dictionary<string, int> counts = new Dictionary<string, int>();
            List<int> gamesInWeek = new List<int>();
            foreach (string line in lines)
            {
                if (line.StartsWith("WEEK "))
                {
                    gamesInWeek.Add(0);
                    continue;
                }
                gamesInWeek[gamesInWeek.Count - 1]++;
                foreach (string team in line.Split(new[] { " at " }, System.StringSplitOptions.None))
                    counts[team] = (counts.ContainsKey(team) ? counts[team] : 0) + 1;
            }
            Assert.AreEqual(weeks, gamesInWeek.Count, name + ": week count.");
            for (int w = 0; w < gamesInWeek.Count; w++)
            {
                Assert.IsTrue(gamesInWeek[w] >= 1, name + ": week " + (w + 1) + " is empty.");
                Assert.IsTrue(gamesInWeek[w] <= maxPerWeek, name + ": week " + (w + 1) + " has " + gamesInWeek[w] + " games.");
            }
            foreach (KeyValuePair<string, int> kv in counts)
                Assert.AreEqual(gamesPerTeam, kv.Value, name + ": " + kv.Key + " games.");
        }

        [TestMethod]
        public void Transform_RespectsMaxGamesPerWeek_WhenAddingMakeupGames()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 1;

            List<string> lines = new List<string> { "WEEK 1", "a at b", "WEEK 2", "c at d" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Dictionary<int, int> gamesPerWeek = new Dictionary<int, int>();
            List<ScheduledGame> allGames = new List<ScheduledGame>();
            allGames.AddRange(result.AddedGames);
            // Recount every week directly from TransformedLines to check the real cap was honored.
            int currentWeek = -1;
            foreach (string line in result.TransformedLines)
            {
                if (line.StartsWith("WEEK "))
                {
                    currentWeek = int.Parse(line.Substring(5));
                    gamesPerWeek[currentWeek] = 0;
                }
                else
                {
                    gamesPerWeek[currentWeek]++;
                }
            }
            foreach (KeyValuePair<int, int> kv in gamesPerWeek)
                Assert.IsTrue(kv.Value <= 1, string.Format("Week {0} has {1} games, exceeding MaxGamesPerWeek=1.", kv.Key, kv.Value));
        }

        [TestMethod]
        public void Transform_NeverMutatesInputList()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b" };
            config.GamesPerTeam = 1;
            config.MaxGamesPerWeek = 10;

            List<string> lines = new List<string> { "WEEK 1", "a at b" };
            List<string> original = new List<string>(lines);
            ScheduleTransformer.Transform(lines, config);

            CollectionAssert.AreEqual(original, lines);
        }

        // ---- Real-world regression: TSBTool_Test\ROMs\Genesis\2026-schedule-17-games.txt (modern
        // 32-team/17-game schedule) -> Genesis TSB1's real 28-team/16-game season. ----

        private static readonly string[] GenesisTeams =
        {
            "bills",   "colts",  "dolphins", "patriots",  "jets",
            "bengals", "browns", "oilers",   "steelers",
            "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
            "cowboys", "giants", "eagles",   "cardinals", "redskins",
            "bears",   "lions",  "packers",  "vikings",   "buccaneers",
            "falcons", "rams",   "saints",   "49ers"
        };

        // The production config, not a test-local copy -- so every real-schedule test below also
        // covers what Genesis_TSB1Tool.ApplySchedule actually uses.
        private static ScheduleTransformerConfig BuildGenesisConfig()
        {
            return ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1);
        }

        // ---- ForRom: per-ROM_TYPE config lookup ----

        [TestMethod]
        public void ForRom_TwentyEightTeamRoms_ShareGenesisStyleConfig()
        {
            foreach (ROM_TYPE romType in new[] { ROM_TYPE.NES_ORIGINAL_TSB, ROM_TYPE.SNES_TSB1, ROM_TYPE.GENESIS_TSB1 })
            {
                ScheduleTransformerConfig config = ScheduleTransformer.ForRom(romType);
                Assert.IsNotNull(config, romType + " should have a config.");
                CollectionAssert.AreEquivalent(GenesisTeams, config.ValidTeams, romType + " team set.");
                Assert.AreEqual("oilers", config.TeamRenames["titans"], romType + " titans rename.");
                Assert.AreEqual(16, config.GamesPerTeam, romType + " games per team.");
                Assert.AreEqual(14, config.MaxGamesPerWeek, romType + " max games per week.");
                CollectionAssert.AreEquivalent(new[] { "jaguars", "panthers", "ravens", "texans" }, config.DroppableTeams, romType + " droppable teams.");
                Assert.AreEqual(romType == ROM_TYPE.NES_ORIGINAL_TSB ? 17 : 18, config.Weeks, romType + " weeks.");
            }
        }

        // The 32-team, 17-week CXROM hacks: nothing to drop or rename -- only the season length
        // (an 18-week/17-game schedule has to lose its last week) can need adjusting.
        [TestMethod]
        public void ForRom_SeventeenWeekCxRoms_ThirtyTwoTeamConfig()
        {
            foreach (ROM_TYPE romType in new[] { ROM_TYPE.CXROM_v105, ROM_TYPE.CXROM_v111 })
            {
                ScheduleTransformerConfig config = ScheduleTransformer.ForRom(romType);
                Assert.IsNotNull(config, romType + " should have a config.");
                Assert.AreEqual(32, config.ValidTeams.Length, romType + " team count.");
                CollectionAssert.Contains(config.ValidTeams, "jaguars");
                CollectionAssert.Contains(config.ValidTeams, "titans");
                Assert.AreEqual(0, config.DroppableTeams.Length, romType + " droppable teams.");
                Assert.AreEqual(0, config.TeamRenames.Count, romType + " renames.");
                Assert.AreEqual(16, config.GamesPerTeam, romType + " games per team.");
                Assert.AreEqual(16, config.MaxGamesPerWeek, romType + " max games per week.");
                Assert.AreEqual(17, config.Weeks, romType + " weeks.");
            }
        }

        [TestMethod]
        public void ForRom_EighteenWeekCxRomAndUnsupportedRoms_ReturnNull()
        {
            foreach (ROM_TYPE romType in new[] { ROM_TYPE.CXROM_18WEEK, ROM_TYPE.NONE, ROM_TYPE.READ_ONLY_ERROR })
            {
                Assert.IsNull(ScheduleTransformer.ForRom(romType), romType + " should not have a config.");
            }
        }

        // TSB2: the same 28 teams as TSB1 and 14 games a week, but three stored seasons of
        // different lengths -- 17, 18, and 17 weeks.
        [TestMethod]
        public void ForRom_Tsb2_WeeksDependOnSeason()
        {
            int[] expectedWeeks = { 17, 18, 17 };
            for (int season = 1; season <= 3; season++)
            {
                ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB2, season);
                Assert.IsNotNull(config, "season " + season);
                CollectionAssert.AreEquivalent(GenesisTeams, config.ValidTeams, "season " + season + " teams.");
                Assert.AreEqual("oilers", config.TeamRenames["titans"]);
                CollectionAssert.AreEquivalent(new[] { "jaguars", "panthers", "ravens", "texans" }, config.DroppableTeams);
                Assert.AreEqual(16, config.GamesPerTeam);
                Assert.AreEqual(14, config.MaxGamesPerWeek);
                Assert.IsNull(config.WeekCaps);
                Assert.AreEqual(expectedWeeks[season - 1], config.Weeks, "season " + season + " weeks.");
            }
        }

        // TSB3: 30 teams (jaguars and panthers joined in 1995), one 17-week season, and a fixed
        // number of game slots per week that adds up to exactly 30 * 16 / 2 = 240 -- so every week
        // of a valid season is exactly full.
        [TestMethod]
        public void ForRom_Tsb3_ThirtyTeamsAndPerWeekSlotCounts()
        {
            ScheduleTransformerConfig config = ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB3);
            Assert.IsNotNull(config);
            Assert.AreEqual(30, config.ValidTeams.Length);
            CollectionAssert.IsSubsetOf(GenesisTeams, config.ValidTeams);
            CollectionAssert.Contains(config.ValidTeams, "jaguars");
            CollectionAssert.Contains(config.ValidTeams, "panthers");
            CollectionAssert.AreEquivalent(new[] { "ravens", "texans" }, config.DroppableTeams);
            Assert.AreEqual("oilers", config.TeamRenames["titans"]);
            Assert.AreEqual(16, config.GamesPerTeam);
            Assert.AreEqual(17, config.Weeks);
            CollectionAssert.AreEqual(new[] { 15, 15, 15, 12, 13, 13, 13, 13, 13, 14, 14, 15, 15, 15, 15, 15, 15 }, config.WeekCaps);
            int total = 0;
            foreach (int cap in config.WeekCaps)
                total += cap;
            Assert.AreEqual(30 * 16 / 2, total, "TSB3's slots add up to exactly one full season.");
        }

        // ---- Exactly-full weeks (WeekCaps adding up to exactly one season, e.g. TSB3) ----

        // Checks every week holds exactly its cap, every team has GamesPerTeam, nobody plays twice
        // in a week, and every team is idle in exactly (weeks - GamesPerTeam) weeks.
        private static void AssertExactlyFullSeason(ScheduleTransformResult result, ScheduleTransformerConfig config)
        {
            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
            List<int> gamesInWeek = new List<int>();
            foreach (string line in result.TransformedLines)
            {
                if (line.StartsWith("WEEK "))
                    gamesInWeek.Add(0);
                else
                    gamesInWeek[gamesInWeek.Count - 1]++;
            }
            CollectionAssert.AreEqual(config.WeekCaps, gamesInWeek.ToArray(), "Every week should be exactly full.");
            foreach (string team in config.ValidTeams)
                Assert.AreEqual(config.GamesPerTeam, result.FinalGameCounts[team], team + " games.");
        }

        // 4 teams, 2 games each, weeks holding 2/1/1 games (= exactly 4 games): week 1 has no byes,
        // weeks 2 and 3 have 2 byes each, so every team gets exactly one bye in week 2 or 3.
        [TestMethod]
        public void Transform_ExactlyFullWeeks_OneByePerTeamAndEveryWeekFull()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 2;
            config.WeekCaps = new[] { 2, 1, 1 };

            List<string> lines = new List<string> { "WEEK 1", "a at b", "c at d", "WEEK 2", "a at c", "WEEK 3" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            AssertExactlyFullSeason(result, config);
            Assert.AreEqual(0, result.ByeRoomGames.Count, "Byes fit without removing anything.");
            CollectionAssert.AreEqual(new List<string> { "WEEK 1", "a at b", "c at d", "WEEK 2", "a at c", "WEEK 3", "b at d" }, result.TransformedLines);
        }

        // a is free only in week 1, which has no byes -- so a can't get its bye until one of its
        // games in a bye week is removed to make room. That removal is reported, and the result is
        // still an exactly full season.
        [TestMethod]
        public void Transform_ExactlyFullWeeks_RemovesAGameToMakeRoomForAByeAndReportsIt()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 2;
            config.WeekCaps = new[] { 2, 1, 1 };

            List<string> lines = new List<string> { "WEEK 1", "WEEK 2", "a at b", "WEEK 3", "a at c" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            AssertExactlyFullSeason(result, config);
            Assert.AreEqual(1, result.ByeRoomGames.Count, "One of a's games has to go to make room for its bye.");
            Assert.IsTrue(result.ByeRoomGames[0].Away == "a" || result.ByeRoomGames[0].Home == "a");
        }

        [TestMethod]
        public void DescribeAdjustments_IncludesByeRoomCountWhenAny()
        {
            ScheduleTransformResult result = new ScheduleTransformResult();
            result.DroppedGames.Add(new ScheduledGame(1, "ravens", "steelers"));
            result.ByeRoomGames.Add(new ScheduledGame(5, "colts", "jets"));
            result.ByeRoomGames.Add(new ScheduledGame(6, "bills", "jets"));
            result.AddedGames.Add(new ScheduledGame(5, "steelers", "bills"));

            string text = ScheduleTransformer.DescribeAdjustments(result, ScheduleTransformer.ForRom(ROM_TYPE.SNES_TSB3));
            StringAssert.Contains(text, "  0 games trimmed, 2 games removed to make room for byes, 1 makeup games added (0 repeat matchups)");
        }

        // WeekCaps: a per-week limit (by position in the season). Games already over a week's cap
        // are trimmed to fit, and makeup games never exceed it.
        [TestMethod]
        public void Transform_WeekCaps_OverflowTrimmedAndCapsHonored()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b", "c", "d" };
            config.GamesPerTeam = 2;
            config.MaxGamesPerWeek = 10;
            config.WeekCaps = new[] { 1, 2, 1 };

            List<string> lines = new List<string> { "WEEK 1", "a at b", "c at d", "WEEK 2", "a at c", "WEEK 3" };
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, config);

            Assert.AreEqual(1, result.TrimmedGames.Count, "Week 1 holds only 1 game, so one of its 2 must go.");
            Assert.AreEqual(1, result.TrimmedGames[0].Week);
            List<int> gamesInWeek = new List<int>();
            foreach (string line in result.TransformedLines)
            {
                if (line.StartsWith("WEEK "))
                    gamesInWeek.Add(0);
                else
                    gamesInWeek[gamesInWeek.Count - 1]++;
            }
            for (int w = 0; w < gamesInWeek.Count; w++)
                Assert.IsTrue(gamesInWeek[w] <= config.WeekCaps[w], "Week " + (w + 1) + " has " + gamesInWeek[w] + " games, cap " + config.WeekCaps[w]);
            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
        }

        [TestMethod]
        public void ForRom_ReturnsIndependentInstances()
        {
            ScheduleTransformerConfig first = ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1);
            first.TeamRenames["jaguars"] = "oilers";
            first.ValidTeams[0] = "changed";

            ScheduleTransformerConfig second = ScheduleTransformer.ForRom(ROM_TYPE.GENESIS_TSB1);
            Assert.IsFalse(second.TeamRenames.ContainsKey("jaguars"), "Mutating one config must not leak into the next.");
            Assert.AreNotEqual("changed", second.ValidTeams[0], "Mutating one config's team array must not leak into the next.");
        }

        // Guards against ForRom's team list drifting from the one TecmoToolFactory installs.
        [TestMethod]
        public void ForRom_ValidTeamsMatchFactoryTeamList()
        {
            string[] romFiles = { "TSPRBOWL.nes", "TSB1.smc", @"Genesis\Tecmo Super Bowl (USA) (October 1993).md",
                                  "TSB 2007-32-105.nes", "TSB 2007-32-111.nes" };
            foreach (string romFile in romFiles)
            {
                ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romFile));
                ScheduleTransformerConfig config = ScheduleTransformer.ForRom(tool.RomVersion);
                Assert.IsNotNull(config, romFile + " (" + tool.RomVersion + ") should have a config.");
                // CXROM's list also carries the Pro Bowl "AFC"/"NFC" pseudo-teams, which never play.
                List<string> factoryTeams = new List<string>();
                foreach (string t in TecmoTool.Teams)
                    if (t != "AFC" && t != "NFC")
                        factoryTeams.Add(t);
                CollectionAssert.AreEquivalent(factoryTeams, config.ValidTeams, romFile + " team set vs. TecmoTool.Teams.");
            }
        }

        // ---- HasUnknownTeams: the gate for whether a schedule needs transforming at all ----

        [TestMethod]
        public void HasUnknownTeams_OnlyValidTeams_ReturnsFalse()
        {
            List<string> lines = new List<string> { "WEEK 1", "bills at patriots", "WEEK 2", "49ers at rams" };
            Assert.IsFalse(ScheduleTransformer.HasUnknownTeams(lines, BuildGenesisConfig()));
        }

        [TestMethod]
        public void HasUnknownTeams_RenamedTeamCountsAsKnown()
        {
            List<string> lines = new List<string> { "WEEK 1", "titans at colts" };
            Assert.IsFalse(ScheduleTransformer.HasUnknownTeams(lines, BuildGenesisConfig()));
        }

        [TestMethod]
        public void HasUnknownTeams_ExpansionTeam_ReturnsTrue()
        {
            List<string> lines = new List<string> { "WEEK 1", "bills at patriots", "WEEK 2", "jaguars at colts" };
            Assert.IsTrue(ScheduleTransformer.HasUnknownTeams(lines, BuildGenesisConfig()));
        }

        [TestMethod]
        public void HasUnknownTeams_IsCaseInsensitiveAndIgnoresComments()
        {
            List<string> lines = new List<string> { "# jaguars are not here", "WEEK 1", "Bills at PATRIOTS" };
            Assert.IsFalse(ScheduleTransformer.HasUnknownTeams(lines, BuildGenesisConfig()));
        }

        [TestMethod]
        public void HasUnknownTeams_RealSchedule_ReturnsTrue()
        {
            Assert.IsTrue(ScheduleTransformer.HasUnknownTeams(LoadRealScheduleLines(), BuildGenesisConfig()));
        }

        // ---- FindUnrecognizedGames: typo detection (names that are neither valid, droppable, nor renamed) ----

        [TestMethod]
        public void FindUnrecognizedGames_ValidDroppableAndRenamed_ReturnsEmpty()
        {
            List<string> lines = new List<string> { "WEEK 1", "bills at patriots", "jaguars at colts", "titans at texans" };
            Assert.AreEqual(0, ScheduleTransformer.FindUnrecognizedGames(lines, BuildGenesisConfig()).Count);
        }

        [TestMethod]
        public void FindUnrecognizedGames_Typo_ReturnsThatGameWithWeek()
        {
            List<string> lines = new List<string> { "WEEK 1", "bills at patriots", "WEEK 3", "jaguars at colts", "Bills at Buccaners" };
            List<ScheduledGame> found = ScheduleTransformer.FindUnrecognizedGames(lines, BuildGenesisConfig());

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(3, found[0].Week);
            Assert.AreEqual("bills", found[0].Away);
            Assert.AreEqual("buccaners", found[0].Home);
        }

        // Every real schedule uses only valid, droppable, or renamed names -- i.e. DroppableTeams is
        // complete, and FitToRom will never mistake a real team for a typo.
        [TestMethod]
        public void FindUnrecognizedGames_AllRealSchedules_ReturnEmpty()
        {
            string realScheduleDir = System.IO.Path.GetDirectoryName(TestRoms.GetRealSchedulePath("2016-schedule-16-games.txt"));
            List<string> files = new List<string>(System.IO.Directory.GetFiles(realScheduleDir, "*.txt"));
            files.Add(TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt"));
            Assert.IsTrue(files.Count >= 11, "Expected at least 11 real schedule files.");
            foreach (string file in files)
            {
                List<string> lines = new List<string>(System.IO.File.ReadAllLines(file));
                List<ScheduledGame> found = ScheduleTransformer.FindUnrecognizedGames(lines, BuildGenesisConfig());
                Assert.AreEqual(0, found.Count, System.IO.Path.GetFileName(file) + ": " + (found.Count > 0 ? found[0].Away + " at " + found[0].Home : ""));
            }
        }

        // ---- RenamesApplied / DescribeAdjustments: what the user is told ----

        [TestMethod]
        public void Transform_RecordsOnlyRenamesActuallyApplied()
        {
            ScheduleTransformerConfig config = new ScheduleTransformerConfig();
            config.ValidTeams = new[] { "a", "b" };
            config.TeamRenames["titans"] = "a";
            config.TeamRenames["commanders"] = "b";
            config.GamesPerTeam = 1;
            config.MaxGamesPerWeek = 10;

            ScheduleTransformResult result = ScheduleTransformer.Transform(new List<string> { "WEEK 1", "titans at b" }, config);
            CollectionAssert.AreEqual(new List<string> { "titans -> a" }, result.RenamesApplied);

            result = ScheduleTransformer.Transform(new List<string> { "WEEK 1", "a at b" }, config);
            Assert.AreEqual(0, result.RenamesApplied.Count);
        }

        [TestMethod]
        public void DescribeAdjustments_NamesDroppedTeamsWithCounts()
        {
            ScheduleTransformerConfig config = BuildGenesisConfig();
            ScheduleTransformResult result = new ScheduleTransformResult();
            result.DroppedGames.Add(new ScheduledGame(1, "jaguars", "colts"));
            result.DroppedGames.Add(new ScheduledGame(2, "colts", "jaguars"));
            result.DroppedGames.Add(new ScheduledGame(3, "texans", "jaguars")); // counts for both teams, once in the total
            result.RenamesApplied.Add("titans -> oilers");
            result.TrimmedGames.Add(new ScheduledGame(4, "bills", "jets"));
            result.AddedGames.Add(new ScheduledGame(5, "colts", "bills"));
            result.AddedGames.Add(new ScheduledGame(6, "colts", "jets"));
            result.RepeatMakeupGames.Add(result.AddedGames[1]);

            string expected =
                "Schedule adjusted to fit this ROM's 28 teams:\n" +
                "  Teams not in this ROM (games dropped): jaguars (3), texans (1) -- 3 games\n" +
                "  Renamed: titans -> oilers\n" +
                "  1 games trimmed, 2 makeup games added (1 repeat matchups)";
            Assert.AreEqual(expected, ScheduleTransformer.DescribeAdjustments(result, config));
        }

        [TestMethod]
        public void DescribeAdjustments_IncludesDisplacedCountWhenAny()
        {
            ScheduleTransformResult result = new ScheduleTransformResult();
            result.DroppedGames.Add(new ScheduledGame(1, "ravens", "steelers"));
            result.TrimmedGames.Add(new ScheduledGame(2, "bills", "jets"));
            result.DisplacedGames.Add(new ScheduledGame(3, "colts", "jets"));
            result.DisplacedGames.Add(new ScheduledGame(4, "colts", "bills"));
            result.AddedGames.Add(new ScheduledGame(5, "steelers", "bills"));

            string text = ScheduleTransformer.DescribeAdjustments(result, BuildGenesisConfig());
            StringAssert.Contains(text, "  1 games trimmed, 2 games displaced to avoid double-booking, 1 makeup games added (0 repeat matchups)");
        }

        [TestMethod]
        public void DescribeAdjustments_OmitsRenamedLineWhenUnused()
        {
            ScheduleTransformResult result = new ScheduleTransformResult();
            result.DroppedGames.Add(new ScheduledGame(1, "ravens", "steelers"));

            string text = ScheduleTransformer.DescribeAdjustments(result, BuildGenesisConfig());
            Assert.IsFalse(text.Contains("Renamed"), text);
            StringAssert.Contains(text, "ravens (1) -- 1 games");
        }

        private static List<string> LoadRealScheduleLines()
        {
            string text = System.IO.File.ReadAllText(TestRoms.GetRomPath(@"Genesis\2026-schedule-17-games.txt"));
            List<string> lines = new List<string>();
            foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 0)
                    lines.Add(trimmed);
            }
            return lines;
        }

        [TestMethod]
        public void Transform_RealSchedule_EveryTeamEndsAtExactlySixteenGames()
        {
            List<string> lines = LoadRealScheduleLines();
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, BuildGenesisConfig());

            Assert.AreEqual(GenesisTeams.Length, result.FinalGameCounts.Count);
            foreach (string team in GenesisTeams)
            {
                Assert.IsTrue(result.FinalGameCounts.ContainsKey(team), team + " missing from FinalGameCounts.");
                Assert.AreEqual(16, result.FinalGameCounts[team], string.Format("{0} ended with {1} games, expected 16.", team, result.FinalGameCounts[team]));
            }
        }

        [TestMethod]
        public void Transform_RealSchedule_TotalGameCountIsExactlyTwoHundredTwentyFour()
        {
            List<string> lines = LoadRealScheduleLines();
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, BuildGenesisConfig());

            int gameLines = 0;
            foreach (string line in result.TransformedLines)
                if (!line.StartsWith("WEEK "))
                    gameLines++;

            Assert.AreEqual(224, gameLines);
        }

        [TestMethod]
        public void Transform_RealSchedule_NoWeekExceedsFourteenGames()
        {
            List<string> lines = LoadRealScheduleLines();
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, BuildGenesisConfig());

            int currentWeek = -1;
            Dictionary<int, int> perWeek = new Dictionary<int, int>();
            foreach (string line in result.TransformedLines)
            {
                if (line.StartsWith("WEEK "))
                {
                    currentWeek = int.Parse(line.Substring(5));
                    perWeek[currentWeek] = 0;
                }
                else
                {
                    perWeek[currentWeek]++;
                }
            }
            foreach (KeyValuePair<int, int> kv in perWeek)
                Assert.IsTrue(kv.Value <= 14, string.Format("Week {0} has {1} games.", kv.Key, kv.Value));
        }

        [TestMethod]
        public void Transform_RealSchedule_EveryUntouchedGameSurvivesByteForByteInItsOriginalWeek()
        {
            List<string> lines = LoadRealScheduleLines();
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, BuildGenesisConfig());

            // Build the set of (week, "away at home") pairs that were dropped or trimmed -- everything
            // else from the original file must appear, verbatim, under the same WEEK header, in the
            // output.
            Dictionary<string, bool> removed = new Dictionary<string, bool>();
            foreach (ScheduledGame g in result.DroppedGames)
                removed[g.Week + "|" + g.Away + " at " + g.Home] = true;
            foreach (ScheduledGame g in result.TrimmedGames)
                removed[g.Week + "|" + g.Away + " at " + g.Home] = true;
            foreach (ScheduledGame g in result.DisplacedGames)
                removed[g.Week + "|" + g.Away + " at " + g.Home] = true;

            int currentWeek = -1;
            int checkedCount = 0;
            List<string> transformedSet = result.TransformedLines;
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;
                if (line.StartsWith("WEEK"))
                {
                    currentWeek = int.Parse(line.Substring(5));
                    continue;
                }
                // Apply the same rename ScheduleTransformer applies, so the "untouched" comparison
                // is against what the game actually looks like post-rename (titans->oilers).
                string normalized = line.Replace("titans", "oilers");
                string key = currentWeek + "|" + normalized;
                if (removed.ContainsKey(key))
                    continue;

                checkedCount++;
                int weekHeaderIndex = transformedSet.IndexOf("WEEK " + currentWeek);
                Assert.IsTrue(weekHeaderIndex >= 0, "WEEK " + currentWeek + " missing from transformed output.");
                bool found = false;
                for (int i = weekHeaderIndex + 1; i < transformedSet.Count && !transformedSet[i].StartsWith("WEEK "); i++)
                {
                    if (transformedSet[i] == normalized)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, string.Format("Untouched game '{0}' (week {1}) did not survive unchanged.", normalized, currentWeek));
            }
            Assert.IsTrue(checkedCount > 100, "Sanity check that this test actually verified a meaningful number of games.");
        }

        [TestMethod]
        public void Transform_RealSchedule_DropsExactlyTheGamesInvolvingUnmappableExpansionTeams()
        {
            List<string> lines = LoadRealScheduleLines();
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, BuildGenesisConfig());

            foreach (ScheduledGame g in result.DroppedGames)
            {
                bool involvesUnmappable = g.Away == "jaguars" || g.Home == "jaguars"
                    || g.Away == "panthers" || g.Home == "panthers"
                    || g.Away == "ravens" || g.Home == "ravens"
                    || g.Away == "texans" || g.Home == "texans";
                Assert.IsTrue(involvesUnmappable, string.Format("Dropped a game that shouldn't have been dropped: {0} at {1}", g.Away, g.Home));
            }
        }

        // ---- Historical regression: real NFL schedules for 2016-2025, each run through the same
        // Genesis TSB1 transform and checked against the same invariant battery as the 2026 file
        // above. 2016-2020 are 16-game/17-week seasons (256 games); 2021-2025 are 17-game/18-week
        // seasons (272 games) -- both shapes are exercised. Fixtures live under
        // TSBTool_Test\real-schedules\ (not under ROMs -- these are plain text schedules, not ROM
        // fixtures). ----

        private static List<string> LoadRealScheduleFixture(string fileName)
        {
            string text = System.IO.File.ReadAllText(TestRoms.GetRealSchedulePath(fileName));
            List<string> lines = new List<string>();
            foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 0)
                    lines.Add(trimmed);
            }
            return lines;
        }

        // A team can only play once per week -- makeup games and swaps must not double-book anyone.
        private static void AssertNoTeamPlaysTwiceInAWeek(List<string> transformedLines)
        {
            string week = null;
            Dictionary<string, string> seen = new Dictionary<string, string>();
            List<string> doubleBooked = new List<string>();
            foreach (string line in transformedLines)
            {
                if (line.StartsWith("WEEK "))
                {
                    week = line;
                    continue;
                }
                foreach (string team in line.Split(new[] { " at " }, System.StringSplitOptions.None))
                {
                    string key = week + "|" + team;
                    if (seen.ContainsKey(key))
                        doubleBooked.Add(string.Format("{0}: {1} ('{2}' and '{3}')", week, team, seen[key], line));
                    else
                        seen[key] = line;
                }
            }
            Assert.AreEqual(0, doubleBooked.Count, "Teams double-booked:\n" + string.Join("\n", doubleBooked.ToArray()));
        }

        [TestMethod]
        public void Transform_RealSchedule_NoTeamPlaysTwiceInAWeek()
        {
            ScheduleTransformResult result = ScheduleTransformer.Transform(LoadRealScheduleLines(), BuildGenesisConfig());
            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);
        }

        private static void AssertGenesisTransformInvariants(List<string> lines)
        {
            ScheduleTransformResult result = ScheduleTransformer.Transform(lines, BuildGenesisConfig());

            // A full 18-week Genesis season -- the 17-week (2016-2020) schedules get an 18th week added.
            AssertValidSeason(result.TransformedLines, 18, 16, 14, "Genesis");

            // Every team ends at exactly 16 games.
            Assert.AreEqual(GenesisTeams.Length, result.FinalGameCounts.Count);
            foreach (string team in GenesisTeams)
            {
                Assert.IsTrue(result.FinalGameCounts.ContainsKey(team), team + " missing from FinalGameCounts.");
                Assert.AreEqual(16, result.FinalGameCounts[team], string.Format("{0} ended with {1} games, expected 16.", team, result.FinalGameCounts[team]));
            }

            // Total game count is exactly 224 (28 teams * 16 games / 2).
            int gameLines = 0;
            foreach (string line in result.TransformedLines)
                if (!line.StartsWith("WEEK "))
                    gameLines++;
            Assert.AreEqual(224, gameLines);

            // No week exceeds MaxGamesPerWeek.
            int currentWeek = -1;
            Dictionary<int, int> perWeek = new Dictionary<int, int>();
            foreach (string line in result.TransformedLines)
            {
                if (line.StartsWith("WEEK "))
                {
                    currentWeek = int.Parse(line.Substring(5));
                    perWeek[currentWeek] = 0;
                }
                else
                {
                    perWeek[currentWeek]++;
                }
            }
            foreach (KeyValuePair<int, int> kv in perWeek)
                Assert.IsTrue(kv.Value <= 14, string.Format("Week {0} has {1} games.", kv.Key, kv.Value));

            AssertNoTeamPlaysTwiceInAWeek(result.TransformedLines);

            // Every untouched game survives byte-for-byte under its original week.
            Dictionary<string, bool> removed = new Dictionary<string, bool>();
            foreach (ScheduledGame g in result.DroppedGames)
                removed[g.Week + "|" + g.Away + " at " + g.Home] = true;
            foreach (ScheduledGame g in result.TrimmedGames)
                removed[g.Week + "|" + g.Away + " at " + g.Home] = true;
            foreach (ScheduledGame g in result.DisplacedGames)
                removed[g.Week + "|" + g.Away + " at " + g.Home] = true;

            int checkWeek = -1;
            int checkedCount = 0;
            List<string> transformedSet = result.TransformedLines;
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;
                if (line.StartsWith("WEEK"))
                {
                    checkWeek = int.Parse(line.Substring(5));
                    continue;
                }
                string normalized = line.Replace("titans", "oilers");
                string key = checkWeek + "|" + normalized;
                if (removed.ContainsKey(key))
                    continue;

                checkedCount++;
                int weekHeaderIndex = transformedSet.IndexOf("WEEK " + checkWeek);
                Assert.IsTrue(weekHeaderIndex >= 0, "WEEK " + checkWeek + " missing from transformed output.");
                bool found = false;
                for (int i = weekHeaderIndex + 1; i < transformedSet.Count && !transformedSet[i].StartsWith("WEEK "); i++)
                {
                    if (transformedSet[i] == normalized)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, string.Format("Untouched game '{0}' (week {1}) did not survive unchanged.", normalized, checkWeek));
            }
            Assert.IsTrue(checkedCount > 100, "Sanity check that this test actually verified a meaningful number of games.");

            // Every dropped game involves a genuinely unmappable expansion team.
            foreach (ScheduledGame g in result.DroppedGames)
            {
                bool involvesUnmappable = g.Away == "jaguars" || g.Home == "jaguars"
                    || g.Away == "panthers" || g.Home == "panthers"
                    || g.Away == "ravens" || g.Home == "ravens"
                    || g.Away == "texans" || g.Home == "texans";
                Assert.IsTrue(involvesUnmappable, string.Format("Dropped a game that shouldn't have been dropped: {0} at {1}", g.Away, g.Home));
            }
        }

        [TestMethod]
        public void Transform_2016Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2016-schedule-16-games.txt"));
        }

        [TestMethod]
        public void Transform_2017Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2017-schedule-16-games.txt"));
        }

        [TestMethod]
        public void Transform_2018Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2018-schedule-16-games.txt"));
        }

        [TestMethod]
        public void Transform_2019Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2019-schedule-16-games.txt"));
        }

        [TestMethod]
        public void Transform_2020Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2020-schedule-16-games.txt"));
        }

        [TestMethod]
        public void Transform_2021Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2021-schedule-17-games.txt"));
        }

        [TestMethod]
        public void Transform_2022Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2022-schedule-17-games.txt"));
        }

        [TestMethod]
        public void Transform_2023Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2023-schedule-17-games.txt"));
        }

        [TestMethod]
        public void Transform_2024Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2024-schedule-17-games.txt"));
        }

        [TestMethod]
        public void Transform_2025Schedule_AllInvariantsHold()
        {
            AssertGenesisTransformInvariants(LoadRealScheduleFixture("2025-schedule-17-games.txt"));
        }
    }
}
