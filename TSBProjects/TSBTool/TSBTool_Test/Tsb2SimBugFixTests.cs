using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// FIX_SIM_BUGS for TSB2/TSB3: the games' INT-credit loop ($84:E0C4 TSB2, $84:E205 TSB3) walks the
    /// defenders down, and when it lands on an absent (injured) one it steps the wrong way and jumps into the
    /// sack loop, so the INT is credited to nobody (about 1 in 20 INTs; SimTests\TSB2_study\
    /// TSB2_Sim_Engine.txt, bug G1). The fix (5 bytes in place) steps down and goes on with the INT loop:
    /// TSB2 0x260F9 E8 88 4C FA DF -> CA EA 4C D1 E0, TSB3 0x2623A E8 88 4C 3B E1 -> CA EA 4C 12 E2.
    /// Simmed for TSB2 (TSB2_G1_IntFix_results.txt: 423 of 423 INTs credited, the rest of the season normal).
    /// </summary>
    [TestClass]
    public class Tsb2SimBugFixTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static TSB2Tool Load(string rom)
        {
            return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom)) as TSB2Tool;
        }

        private static void AssertOnlyChanged(byte[] before, byte[] after, int location, byte[] expected)
        {
            for (int i = 0; i < before.Length; i++)
            {
                if (i >= location && i < location + expected.Length)
                    Assert.AreEqual(expected[i - location], after[i], string.Format("byte 0x{0:X}", i));
                else
                    Assert.AreEqual(before[i], after[i], string.Format("byte 0x{0:X} shouldn't change", i));
            }
        }

        [TestMethod]
        // The same 5 bytes as the ROM that was simmed (SimTests\TSB2_study\TSB2_G1_IntFix.smc, which also had
        // its header checksum fixed; SimTests isn't in git).
        public void Tsb2_FixSimBugs_WritesTheIntFix_AndNothingElse()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            byte[] before = (byte[])tool.OutputRom.Clone();
            Assert.IsTrue(tool.HasSimBugs());

            Assert.IsTrue(tool.FixSimBugs());

            Assert.IsFalse(tool.HasSimBugs());
            AssertOnlyChanged(before, tool.OutputRom, 0x260F9, new byte[] { 0xCA, 0xEA, 0x4C, 0xD1, 0xE0 });
        }

        [TestMethod]
        public void Tsb3_FixSimBugs_WritesTheIntFix_AndNothingElse()
        {
            TSB2Tool tool = Load("TSB3.smc");
            Assert.IsInstanceOfType(tool, typeof(TSB3Tool));
            byte[] before = (byte[])tool.OutputRom.Clone();
            Assert.IsTrue(tool.HasSimBugs());

            Assert.IsTrue(tool.FixSimBugs());

            Assert.IsFalse(tool.HasSimBugs());
            AssertOnlyChanged(before, tool.OutputRom, 0x2623A, new byte[] { 0xCA, 0xEA, 0x4C, 0x12, 0xE2 });
        }

        [TestMethod]
        public void FixSimBugs_TwiceIsHarmless()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            tool.FixSimBugs();
            byte[] once = (byte[])tool.OutputRom.Clone();

            Assert.IsTrue(tool.FixSimBugs());
            CollectionAssert.AreEqual(once, tool.OutputRom);
        }

        [TestMethod]
        public void FixSimBugs_OtherCodeThere_LeftAloneWithAnError()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            tool.OutputRom[0x260FA] = 0xEA; // someone else's patch
            byte[] before = (byte[])tool.OutputRom.Clone();

            Assert.IsFalse(tool.FixSimBugs());
            CollectionAssert.AreEqual(before, tool.OutputRom);
            Assert.IsFalse(tool.HasSimBugs(), "not the stock code either");

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try { StaticUtils.ShowErrors(); }
            finally { StaticUtils.sMessageGiver = null; }
            Assert.AreEqual(1, giver.Errors.Count);
            StringAssert.Contains(giver.Errors[0], "FIX_SIM_BUGS");
        }

        [TestMethod]
        public void FixSimBugsLine_AppliesTheFix()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            tool.ProcessText("FIX_SIM_BUGS\n");
            Assert.IsFalse(tool.HasSimBugs());
        }

        [TestMethod]
        public void Key_MentionsTheLine()
        {
            StringAssert.Contains(Load("TSB2_U.smc").GetKey(), "FIX_SIM_BUGS");
            StringAssert.Contains(Load("TSB3.smc").GetKey(), "FIX_SIM_BUGS");
        }
    }
}
