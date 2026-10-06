using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB2_TSB3\TSB2_3_Core\SNES_ScheduleHelper.cs (shared by TSB2 and TSB3) edge cases that its
    /// own round-trip coverage never reaches -- both found by the cross-ROM schedule work:
    ///   1. Week limits were hard-coded for 18 weeks, so a schedule with too many weeks crashed
    ///      (IndexOutOfRange) on 17-week seasons (TSB2 seasons 1/3, TSB3) instead of being rejected.
    ///   2. CloseWeek only cleared unused slots up to 14, but TSB3 weeks have up to 15 -- a 15-slot
    ///      week given fewer games read back a leftover game from before.
    /// Schedules are loaded through each tool's normal text path, as the program does.
    /// </summary>
    [TestClass]
    public class Tsb2Tsb3ScheduleHelperTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static List<string> SplitToNonEmptyLines(string text)
        {
            return text.Replace("\r\n", "\n").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        }

        private static string LoadAndCollectMessages(ITecmoContent tool, int season, List<string> schedule)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ProcessText("SEASON " + season + "\n" + string.Join("\n", schedule.ToArray()) + "\n");
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            return string.Join("\n", giver.Errors.ToArray());
        }

        // The ROM's own 17-week season plus weeks 18 and 19 (two over, so the schedule fit leaves it
        // alone): rejected with a clear error, the 17 real weeks written as given.
        private static void AssertTooManyWeeksRejectedCleanly(string romFile, int season)
        {
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romFile));
            string before = tool.GetSchedule(season);
            List<string> input = SplitToNonEmptyLines(before);
            input.Add("WEEK 18");
            input.Add("bills at dolphins");
            input.Add("WEEK 19");
            input.Add("jets at patriots");

            string messages = LoadAndCollectMessages(tool, season, input);

            Assert.IsFalse(messages.Contains("Index was outside"), romFile + " crashed:\n" + messages);
            StringAssert.Contains(messages, "Error! You can have a maximum of 17 weeks in a season.", romFile);
            Assert.AreEqual(before, tool.GetSchedule(season), romFile + ": the 17 real weeks should be unchanged.");
        }

        [TestMethod]
        public void Tsb2SeventeenWeekSeason_TooManyWeeks_RejectedWithErrorNotCrash()
        {
            AssertTooManyWeeksRejectedCleanly("TSB2_U.smc", 1);
        }

        [TestMethod]
        public void Tsb3_TooManyWeeks_RejectedWithErrorNotCrash()
        {
            AssertTooManyWeeksRejectedCleanly("TSB3.smc", 1);
        }

        // Adds one extra game to `week` of the ROM's own schedule and checks it's rejected with the
        // week's real slot count, and that nothing in any season changes -- the extra game used to
        // be written into the next week's first slot (or past the season's end, for the last week).
        private static void AssertExtraGameRejected(string romFile, int season, int seasons, int week, int slots)
        {
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romFile));
            Dictionary<int, string> before = new Dictionary<int, string>();
            for (int s = 1; s <= seasons; s++)
                before[s] = tool.GetSchedule(s);

            List<string> input = SplitToNonEmptyLines(before[season]);
            int header = input.IndexOf("WEEK " + week);
            int next = input.IndexOf("WEEK " + (week + 1));
            int end = next < 0 ? input.Count : next;
            Assert.AreEqual(slots, end - header - 1, "Sanity: week " + week + " is full in the stock schedule.");
            input.Insert(end, "bills at dolphins");

            string messages = LoadAndCollectMessages(tool, season, input);

            StringAssert.Contains(messages, string.Format("Error! Week {0}: You can have no more than {1} games in a week.", week, slots), romFile);
            for (int s = 1; s <= seasons; s++)
                Assert.AreEqual(before[s], tool.GetSchedule(s), romFile + ": season " + s + " should be unchanged.");
        }

        [TestMethod]
        public void Tsb3_TwelveSlotWeekGivenThirteenGames_ExtraGameRejected()
        {
            AssertExtraGameRejected("TSB3.smc", 1, 1, 4, 12);
        }

        [TestMethod]
        public void Tsb2_WeekGivenFifteenGames_ExtraGameRejectedAndNextWeekUntouched()
        {
            AssertExtraGameRejected("TSB2_U.smc", 1, 3, 1, 14);
        }

        [TestMethod]
        public void Tsb2_LastWeekGivenFifteenGames_ExtraGameRejectedAndOtherSeasonsUntouched()
        {
            AssertExtraGameRejected("TSB2_U.smc", 1, 3, 17, 14);
        }

        // TSB3 week 1 has 15 slots. Give it 14 games: it must read back exactly those 14 -- not the
        // 15th game left over from the ROM's own schedule.
        [TestMethod]
        public void Tsb3_FifteenSlotWeekGivenFourteenGames_ReadsBackOnlyThoseFourteen()
        {
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc"));
            List<string> input = SplitToNonEmptyLines(tool.GetSchedule(1));
            int week1 = input.IndexOf("WEEK 1");
            int week2 = input.IndexOf("WEEK 2");
            Assert.AreEqual(15, week2 - week1 - 1, "Sanity: TSB3's week 1 holds 15 games.");
            input.RemoveAt(week2 - 1); // drop week 1's last game
            List<string> expectedWeek1 = input.GetRange(week1 + 1, 14);

            LoadAndCollectMessages(tool, 1, input);

            List<string> readBack = SplitToNonEmptyLines(tool.GetSchedule(1));
            int rb1 = readBack.IndexOf("WEEK 1");
            int rb2 = readBack.IndexOf("WEEK 2");
            CollectionAssert.AreEqual(expectedWeek1, readBack.GetRange(rb1 + 1, rb2 - rb1 - 1),
                "Week 1 should read back exactly the 14 games written.");
        }
    }
}
