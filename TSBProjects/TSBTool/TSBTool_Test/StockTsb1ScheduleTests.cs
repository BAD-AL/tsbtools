using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Ports the schedule round-trip checks from TEST\schedule\Input1-3.txt (previously verified
    /// by hand with a diff tool against TSPRBOWL.nes) and the "too many weeks" case buried inside
    /// TEST\Test5.txt/Test8.txt, onto the stock 28-team/17-week/14-game ROM using the base
    /// ScheduleHelper2 -- the same class stock TecmoTool.GetSchedule()/ApplySchedule() use directly.
    ///
    /// Each "_Expected.txt" fixture is a golden file: NOT what a human eyeballed once, but what
    /// ScheduleHelper2's actual acceptance rules (14 games/week cap, invalid team rejection, 17-week
    /// cap) produce, worked out independently and reviewed line-by-line before being committed.
    /// </summary>
    [TestClass]
    public class StockTsb1ScheduleTests
    {
        private static List<string> SplitToNonEmptyLines(string schedule)
        {
            return schedule
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        private static List<string> LoadExpectedLines(string fileName)
        {
            List<string> raw = TestRoms.LoadScheduleLines(fileName);
            return SplitToNonEmptyLines(string.Join("\n", raw.ToArray()));
        }

        [TestMethod]
        public void StockRom_FullSchedule_RoundTripsExactly()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            TestRoms.EnsureStockTeamsAreSet(rom);
            ScheduleHelper2 helper = new ScheduleHelper2(rom);

            List<string> input = TestRoms.LoadScheduleLines("StockTsb1_FullSchedule_Input.txt");
            helper.ApplySchedule(input);

            // Fixture note: this is a real 1991-season schedule, and 4 teams (cardinals, packers,
            // rams, vikings) end up with 15 games rather than the expected 16 -- an imbalance in
            // the original historical data, not a scheduling defect. That's expected to produce
            // "Warning! ... games scheduled" messages; it must not produce any actual "Error!" ones.
            List<string> errors = helper.GetErrorMessages() ?? new List<string>();
            List<string> hardErrors = errors.Where(e => e.StartsWith("Error!", StringComparison.OrdinalIgnoreCase)).ToList();
            Assert.AreEqual(0, hardErrors.Count, "A full valid 17-week schedule should apply with no hard errors:\n" + string.Join("\n", hardErrors.ToArray()));

            List<string> actual = SplitToNonEmptyLines(helper.GetSchedule());
            List<string> expected = LoadExpectedLines("StockTsb1_FullSchedule_Expected.txt");
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void StockRom_MinimalSchedule_RoundTripsExactly()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            TestRoms.EnsureStockTeamsAreSet(rom);
            ScheduleHelper2 helper = new ScheduleHelper2(rom);

            List<string> input = TestRoms.LoadScheduleLines("StockTsb1_MinimalSchedule_Input.txt");
            helper.ApplySchedule(input);

            List<string> actual = SplitToNonEmptyLines(helper.GetSchedule());
            List<string> expected = LoadExpectedLines("StockTsb1_MinimalSchedule_Expected.txt");
            CollectionAssert.AreEqual(expected, actual, "A schedule with just 1 game/week for all 17 weeks should round-trip exactly.");
        }

        /// <summary>
        /// Input fixture has an invalid matchup ("poo at pee" in week 1) and, in week 17, 27
        /// attempted games where the stock cap is 14/week. Confirms: the invalid matchup is
        /// rejected with an error and dropped from the output; games past the 14th in week 17
        /// (and the 15th/16th in weeks 15/16) are rejected the same way and dropped; everything
        /// else round-trips untouched.
        /// </summary>
        [TestMethod]
        public void StockRom_InvalidTeamAndWeekOverflow_AreRejectedAndDropped()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            TestRoms.EnsureStockTeamsAreSet(rom);
            ScheduleHelper2 helper = new ScheduleHelper2(rom);

            List<string> input = TestRoms.LoadScheduleLines("StockTsb1_InvalidAndOverflow_Input.txt");
            helper.ApplySchedule(input);

            List<string> errors = helper.GetErrorMessages() ?? new List<string>();

            int invalidTeamErrors = errors.Count(e => e.IndexOf("poo at pee", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.AreEqual(1, invalidTeamErrors, "Expected exactly one error for the invalid 'poo at pee' matchup.");

            int overflowErrors = errors.Count(e => e.IndexOf("no more than 14 games", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.AreEqual(16, overflowErrors, "Expected 16 rejected games total across weeks 15 (1 over), 16 (2 over), and 17 (13 over).");

            List<string> actual = SplitToNonEmptyLines(helper.GetSchedule());
            List<string> expected = LoadExpectedLines("StockTsb1_InvalidAndOverflow_Expected.txt");
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Appends a "WEEK 18" section (with one game: oilers at buccaneers) after a full valid
        /// 17-week schedule. The stock ROM's cap is 17 weeks (ScheduleHelper2's default totalWeeks),
        /// so week 18 must be rejected: an error is recorded, its pointer table entry is never
        /// written, and -- because GetSchedule() only ever reads back `totalWeeks` weeks -- it's
        /// completely invisible on readback even though the game bytes themselves get written past
        /// the real schedule region. The two teams in that game still pick up a extra counted game
        /// each (17 instead of 16), which surfaces as its own warning.
        /// </summary>
        [TestMethod]
        public void StockRom_SchedulingAnEighteenthWeek_IsRejectedAndInvisibleOnReadback()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            TestRoms.EnsureStockTeamsAreSet(rom);
            ScheduleHelper2 helper = new ScheduleHelper2(rom);

            List<string> input = TestRoms.LoadScheduleLines("StockTsb1_TooManyWeeks_Input.txt");
            helper.ApplySchedule(input);

            List<string> errors = helper.GetErrorMessages() ?? new List<string>();
            string errorText = string.Join("\n", errors.ToArray());

            Assert.IsTrue(
                errors.Any(e => e.IndexOf("Weeks 18", StringComparison.OrdinalIgnoreCase) >= 0),
                "Expected an error about exceeding the week cap when scheduling a week 18:\n" + errorText);
            Assert.IsTrue(
                errors.Any(e => e.IndexOf("oilers have 17 games", StringComparison.OrdinalIgnoreCase) >= 0),
                "The extra week-18 game should still count against the oilers' total games scheduled:\n" + errorText);
            Assert.IsTrue(
                errors.Any(e => e.IndexOf("buccaneers have 17 games", StringComparison.OrdinalIgnoreCase) >= 0),
                "The extra week-18 game should still count against the buccaneers' total games scheduled:\n" + errorText);

            List<string> actual = SplitToNonEmptyLines(helper.GetSchedule());
            List<string> expected = LoadExpectedLines("StockTsb1_TooManyWeeks_Expected.txt");
            CollectionAssert.AreEqual(expected, actual, "Week 18 must not appear anywhere in the read-back schedule.");
        }
    }
}
