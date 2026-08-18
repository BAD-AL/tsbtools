using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Round-trip coverage for TSB II (TSB2Tool, TSBTool2 namespace) -- an entirely separate
    /// implementation (implements ITecmoTool/ITecmoContent directly, does not extend TecmoTool) with
    /// its own InputParser (TSB2_TSB3\TSB2_3_Core\InputParser.cs). Never covered by any prior test.
    ///
    /// Rather than hand-author an input fixture (as the schedule/full-dump tests do), this uses the
    /// same idempotency technique as ScheduleTests.EighteenWeekRom_ApplyScheduleRoundTrip_IsIdempotent:
    /// dump the ROM's own actual data, re-apply it, dump again, and assert the two dumps match. That
    /// exercises the full real data set (all positions, all teams) without needing to construct one.
    ///
    /// Specifically guards against two historical bugs fixed in e46c8de (2021-02-13), since a
    /// regression in either would very plausibly slip back in unnoticed without a live test:
    ///   1. "Schedule shows 2x" -- GetAll(int season), the overload actually used by the CLI/
    ///      InputParser pipeline (ITecmoContent.GetAll), used to also embed the schedule inline.
    ///      (The parameterless GetAll() still does this, but nothing in the live text-processing
    ///      pipeline calls it -- confirmed no callers outside TSB2Tool.cs itself.)
    ///   2. StaticUtils.CheckTSB2Args referenced the wrong positionNames list, which rejected
    ///      TSB2/TSB3-only positions (this roster has 7 more than the base 28: RE2, NT2, LE2, LB5,
    ///      DB1, DB2, DB3) as invalid.
    /// </summary>
    [TestClass]
    public class Tsb2RoundTripTests
    {
        [TestMethod]
        public void Tsb2Rom_GetAll_DoesNotEmbedSchedule()
        {
            byte[] rom = TestRoms.LoadRom("TSB2_U.smc");
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNotNull(tool, "Failed to detect TSB2_U.smc's ROM type.");

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
        public void Tsb2Rom_FullDumpRoundTrip_IsIdempotent()
        {
            byte[] rom = TestRoms.LoadRom("TSB2_U.smc");
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNotNull(tool, "Failed to detect TSB2_U.smc's ROM type.");

            // The raw ROM predates the 2019-08-14 name-capitalization fix (e.g. "DEL GRECO" gets
            // normalized to "del GRECO" on the first write), so the very first apply is not a no-op.
            // Apply once to normalize, then check that a second apply -> dump truly is idempotent.
            string initial = tool.GetKey() + tool.GetAll(1) + tool.GetSchedule(1);
            tool.ProcessText(initial);
            string before = tool.GetKey() + tool.GetAll(1) + tool.GetSchedule(1);

            tool.ProcessText(before);
            string after = tool.GetKey() + tool.GetAll(1) + tool.GetSchedule(1);

            Assert.AreEqual(before, after, "Re-applying the ROM's own dumped data should reproduce it exactly.");
        }

        /// <summary>
        /// TSB2Tool.positionNames includes 7 positions beyond the base 28-position roster (RE2, NT2,
        /// LE2, LB5, DB1, DB2, DB3). The historical bug validated against the wrong (base TecmoTool)
        /// list, which doesn't contain these, so any operation naming one of them would have been
        /// incorrectly rejected. The round trip above touches every position via GetAll's per-team
        /// dump, but this asserts on the specific symptom directly: the extended positions actually
        /// appear in the dumped roster data.
        /// </summary>
        [TestMethod]
        public void Tsb2Rom_ExtendedPositions_AppearInRosterDump()
        {
            byte[] rom = TestRoms.LoadRom("TSB2_U.smc");
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
