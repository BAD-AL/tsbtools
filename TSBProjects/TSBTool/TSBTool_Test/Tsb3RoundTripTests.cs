using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Round-trip coverage for TSB III (TSB3Tool : TSB2Tool). Unlike most of what TSB3Tool inherits
    /// from TSB2Tool unchanged, GetAll(int), GetSchedule(int), and ApplySchedule are all overridden
    /// with TSB3-specific logic (a different attribute layout and its own SNES_TSB3_ScheduleHelper),
    /// so TSB2's round-trip coverage doesn't automatically extend to TSB3 -- this mirrors
    /// Tsb2RoundTripTests.cs's approach against TSB3.smc instead.
    ///
    /// Same two historical bugs from e46c8de (2021-02-13) guarded against as TSB2:
    ///   1. "Schedule shows 2x" -- TSB3Tool.GetAll(int season) delegates to base.GetAll(1)
    ///      (TSB2Tool's fixed version), so this should already be clean, but it's cheap insurance
    ///      against the delegation itself regressing.
    ///   2. Wrong positionNames -- TSB3Tool doesn't override positionNames, so it shares TSB2Tool's
    ///      extended list (7 positions beyond the base 28).
    /// </summary>
    [TestClass]
    public class Tsb3RoundTripTests
    {
        [TestMethod]
        public void Tsb3Rom_GetAll_DoesNotEmbedSchedule()
        {
            byte[] rom = TestRoms.LoadRom("TSB3.smc");
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNotNull(tool, "Failed to detect TSB3.smc's ROM type.");

            string all = tool.GetAll(1);
            Assert.IsFalse(
                all.IndexOf("WEEK ", StringComparison.OrdinalIgnoreCase) >= 0,
                "GetAll(season) must not embed the schedule (regression guard for the 'schedule shows 2x' bug fixed in e46c8de).");

            string schedule = tool.GetSchedule(1);
            Assert.IsTrue(
                schedule.IndexOf("WEEK 1", StringComparison.OrdinalIgnoreCase) >= 0,
                "GetSchedule(season) should still return the actual schedule.");
        }

        [TestMethod]
        public void Tsb3Rom_FullDumpRoundTrip_IsIdempotent()
        {
            byte[] rom = TestRoms.LoadRom("TSB3.smc");
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNotNull(tool, "Failed to detect TSB3.smc's ROM type.");

            // Same wrinkle as TSB2: the raw ROM predates the 2019-08-14 name-capitalization fix, so
            // the first apply normalizes rather than being a no-op. Normalize once, then verify a
            // second apply -> dump truly is idempotent.
            string initial = tool.GetKey() + tool.GetAll(1) + tool.GetSchedule(1);
            tool.ProcessText(initial);
            string before = tool.GetKey() + tool.GetAll(1) + tool.GetSchedule(1);

            tool.ProcessText(before);
            string after = tool.GetKey() + tool.GetAll(1) + tool.GetSchedule(1);

            Assert.AreEqual(before, after, "Re-applying the ROM's own dumped data should reproduce it exactly.");
        }

        [TestMethod]
        public void Tsb3Rom_ExtendedPositions_AppearInRosterDump()
        {
            byte[] rom = TestRoms.LoadRom("TSB3.smc");
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);

            string all = tool.GetAll(1);
            string[] extendedPositions = { "RE2", "NT2", "LE2", "LB5", "DB1", "DB2", "DB3" };
            foreach (string pos in extendedPositions)
            {
                Assert.IsTrue(
                    all.Split('\n').Any(line => line.TrimStart().StartsWith(pos + ",", StringComparison.Ordinal)),
                    string.Format("Expected to find a '{0}, ...' roster line in the dump.", pos));
            }
        }
    }
}
