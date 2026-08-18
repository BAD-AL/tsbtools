using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Tests for the 18-week/17-game schedule layout (CXRom18WeekScheduleHelper), covering the
    /// offsets located by inspecting SbluemanBase_18week_RealNFL-1.nes:
    ///   weekOneStartLoc=0x32d36, end_schedule_section=0x32f56, gamesPerWeekStartLoc=0x329cb,
    ///   weekPointerBaseConst=0x8d26, totalWeeks=18, 272 total games.
    /// </summary>
    [TestClass]
    public class ScheduleTests
    {
        private static readonly int[] ExpectedGamesPerWeek =
            { 16, 16, 16, 16, 15, 14, 14, 14, 15, 14, 13, 16, 14, 15, 16, 16, 16, 16 };

        // Per-week game counts baked into Schedules\EighteenWeek_AlternateSchedule.txt: the ROM's
        // own distribution (ExpectedGamesPerWeek) reversed week-for-week, so the bye pattern is
        // genuinely different from what's already in the ROM.
        private static readonly int[] ReversedGamesPerWeek =
            { 16, 16, 16, 16, 15, 14, 16, 13, 14, 15, 14, 14, 14, 15, 16, 16, 16, 16 };

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
        public void EighteenWeekRom_ScheduleDecodesCorrectly()
        {
            byte[] rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNotNull(tool);
            Assert.AreEqual(ROM_TYPE.CXROM_18WEEK, tool.RomVersion);

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule(0));

            int weekHeaders = lines.Count(l => l.StartsWith("WEEK", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(18, weekHeaders, "Expected 18 week headers in the decoded schedule.");

            int gameLines = lines.Count(l => l.IndexOf(" at ", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.AreEqual(272, gameLines, "Expected 272 total games (32 teams x 17 games / 2).");

            int week1Index = lines.FindIndex(l => l.Equals("WEEK 1", StringComparison.OrdinalIgnoreCase));
            Assert.AreNotEqual(-1, week1Index, "Could not find 'WEEK 1' header.");
            Assert.AreEqual("seahawks at patriots", lines[week1Index + 1], "Week 1's first game should be seahawks at patriots.");

            Assert.AreEqual("cardinals at 49ers", lines[lines.Count - 1], "The last game of the season should be cardinals at 49ers.");
        }

        [TestMethod]
        public void EighteenWeekRom_GamesPerWeekMatchesKnownCounts()
        {
            byte[] rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
            TestRoms.EnsureCXRomTeamsAreSet(rom);
            CXRom18WeekScheduleHelper helper = new CXRom18WeekScheduleHelper(rom);

            for (int week = 0; week < ExpectedGamesPerWeek.Length; week++)
            {
                Assert.AreEqual(
                    ExpectedGamesPerWeek[week],
                    helper.GetGamesInWeek(week),
                    string.Format("Week {0} game count mismatch.", week + 1));
            }
        }

        [TestMethod]
        public void EighteenWeekRom_ApplyScheduleRoundTrip_IsIdempotent()
        {
            byte[] rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
            TestRoms.EnsureCXRomTeamsAreSet(rom);
            CXRom18WeekScheduleHelper helper = new CXRom18WeekScheduleHelper(rom);

            string before = helper.GetSchedule();
            List<string> lines = SplitToNonEmptyLines(before);

            helper.ApplySchedule(lines);

            List<string> errors = helper.GetErrorMessages();
            string errorText = errors == null ? "" : string.Join("\n", errors.ToArray());
            Assert.IsTrue(
                errors == null || errors.Count == 0,
                "Re-applying the ROM's own decoded 18-week schedule should not produce warnings/errors (each team should show 17 games, not the stock scheme's 16):\n" + errorText);

            string after = helper.GetSchedule();
            Assert.AreEqual(before, after, "Schedule should be byte-for-byte unchanged after a decode -> apply -> decode round trip.");
        }

        /// <summary>
        /// Applies a hand-generated 18-week/272-game schedule (Schedules\EighteenWeek_AlternateSchedule.txt)
        /// that is deliberately different from the ROM's own schedule -- different matchups AND a
        /// different per-week bye pattern (byes reversed week-for-week relative to the ROM's actual
        /// distribution) -- then reads it back and confirms it matches exactly. Unlike the idempotent
        /// re-apply test above, this forces gamesPerWeekStartLoc and the week-pointer table to be
        /// recomputed to genuinely different values than what the ROM shipped with, so it actually
        /// exercises the writer rather than just confirming it leaves matching data alone.
        /// </summary>
        [TestMethod]
        public void EighteenWeekRom_ApplyDifferentSchedule_RoundTripsExactly()
        {
            byte[] rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
            TestRoms.EnsureCXRomTeamsAreSet(rom);
            CXRom18WeekScheduleHelper helper = new CXRom18WeekScheduleHelper(rom);

            List<string> input = TestRoms.LoadScheduleLines("EighteenWeek_AlternateSchedule.txt");
            List<string> expected = SplitToNonEmptyLines(string.Join("\n", input.ToArray()));

            // Sanity check on the fixture itself: it must differ from the ROM's actual bye pattern,
            // otherwise this test wouldn't be exercising anything the idempotent round trip doesn't.
            CollectionAssert.AreNotEqual(ExpectedGamesPerWeek, ReversedGamesPerWeek);

            helper.ApplySchedule(input);

            List<string> errors = helper.GetErrorMessages();
            string errorText = errors == null ? "" : string.Join("\n", errors.ToArray());
            Assert.IsTrue(
                errors == null || errors.Count == 0,
                "Applying a structurally-valid alternate 18-week schedule should not produce warnings/errors:\n" + errorText);

            for (int week = 0; week < ReversedGamesPerWeek.Length; week++)
            {
                Assert.AreEqual(
                    ReversedGamesPerWeek[week],
                    helper.GetGamesInWeek(week),
                    string.Format("Week {0} game count should reflect the newly-applied schedule, not the ROM's original.", week + 1));
            }

            List<string> actual = SplitToNonEmptyLines(helper.GetSchedule());
            CollectionAssert.AreEqual(expected, actual, "Schedule read back after applying the alternate schedule should match the input exactly.");
        }

        /// <summary>
        /// Regression test for a bug found via 18Week_RealNFL_Hack_Documentation-1.md: the byte
        /// immediately after the 18-entry games-per-week array (file 0x329DD) is documented -- and
        /// confirmed present in the fixture ROM -- as a 0xFF terminator the running game depends on.
        /// ScheduleHelper2.ClosePrevWeek() had no bounds check (unlike its sibling
        /// SetupPointerForCurrentWeek(), which does), so supplying more weeks than the ROM supports
        /// would silently overwrite that sentinel with the bogus extra week's game count when
        /// ApplySchedule's unconditional final ClosePrevWeek() call ran.
        /// </summary>
        [TestMethod]
        public void EighteenWeekRom_SchedulingANineteenthWeek_DoesNotCorruptGamesPerWeekTerminator()
        {
            const int terminatorLocation = 0x329DD;

            byte[] rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
            Assert.AreEqual((byte)0xFF, rom[terminatorLocation], "Sanity check: the fixture ROM should have the documented 0xFF terminator before we touch anything.");

            TestRoms.EnsureCXRomTeamsAreSet(rom);
            CXRom18WeekScheduleHelper helper = new CXRom18WeekScheduleHelper(rom);

            List<string> input = TestRoms.LoadScheduleLines("EighteenWeek_AlternateSchedule.txt");
            input.Add("WEEK 19");
            input.Add("bills at dolphins");

            helper.ApplySchedule(input);

            Assert.AreEqual((byte)0xFF, rom[terminatorLocation], "The games-per-week array's terminator byte must survive a bogus 19th week.");

            List<string> errors = helper.GetErrorMessages() ?? new List<string>();
            Assert.IsTrue(
                errors.Any(e => e.IndexOf("Weeks 19", StringComparison.OrdinalIgnoreCase) >= 0),
                "Expected an error about exceeding the week cap when scheduling a week 19.");

            // The 18 real weeks should still be exactly what was supplied, unaffected by the rejected 19th.
            for (int week = 0; week < ReversedGamesPerWeek.Length; week++)
            {
                Assert.AreEqual(
                    ReversedGamesPerWeek[week],
                    helper.GetGamesInWeek(week),
                    string.Format("Week {0} game count should be unaffected by the rejected 19th week.", week + 1));
            }
        }
    }
}
