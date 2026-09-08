using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 3 (partial) of compressed-riding-rossum.md: team-level sim/CPU data. This piece was
    /// implemented ahead of its formal phase slot -- every InputParser command is preceded by a
    /// "TEAM = x SimData=0xNN" line, which unconditionally calls SetTeamSimData (and
    /// SetTeamSimOffensePref, when present), so it had to be real (not a NotImplementedException stub)
    /// before any other phase's ProcessText-based tests could run at all. See open question #3 in the
    /// plan: whether SimData=0xNN's single byte is meant to land on the 0x2F team-summary nibble, the
    /// 0xB58E tendency byte, or both isn't confirmed by the research -- both are written for now.
    /// Player-level sim data/abilities (SetQBSimData etc.) remain stubbed; not covered here.
    /// </summary>
    [TestClass]
    public class GenesisTeamSimDataTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int TeamSimBlockStart = 0xDEA86;
        private const int TeamSimBlockSize = 0x30;
        private const int TeamSimSummaryOffset = 0x2F;
        private const int TeamSimOffensivePrefStart = 0xB58E;

        [TestMethod]
        public void GenesisRom_SetTeamSimData_WritesSummaryByteForCorrectTeamOnly()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("packers");
            Assert.IsTrue(teamIndex > 0);
            int loc = TeamSimBlockStart + teamIndex * TeamSimBlockSize + TeamSimSummaryOffset;

            tool.ProcessText("TEAM = packers SimData=0x57");

            Assert.AreEqual((byte)0x57, tool.OutputRom[loc]);

            // Every other team's 0x2F summary byte should be untouched.
            for (int i = 0; i < 28; i++)
            {
                if (i == teamIndex)
                    continue;
                int otherLoc = TeamSimBlockStart + i * TeamSimBlockSize + TeamSimSummaryOffset;
                Assert.AreEqual(originalRom[otherLoc], tool.OutputRom[otherLoc],
                    string.Format("Team index {0}'s sim summary byte should be untouched.", i));
            }
        }

        [TestMethod]
        public void GenesisRom_GetTeamSimData_ReflectsSetTeamSimData()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;

            tool.ProcessText("TEAM = raiders SimData=0x1F");

            Assert.AreEqual((byte)0x1F, genesisTool.GetTeamSimData("raiders"));
        }

        [TestMethod]
        public void GenesisRom_SetTeamSimOffensePref_WritesTendencyByteForCorrectTeamOnly()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;

            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("dolphins");
            bool result = genesisTool.SetTeamSimOffensePref("dolphins", 2);

            Assert.IsTrue(result);
            Assert.AreEqual(2, genesisTool.GetTeamSimOffensePref("dolphins"));
            Assert.AreEqual((byte)2, tool.OutputRom[TeamSimOffensivePrefStart + teamIndex]);

            for (int i = 0; i < 28; i++)
            {
                if (i == teamIndex)
                    continue;
                Assert.AreEqual(originalRom[TeamSimOffensivePrefStart + i], tool.OutputRom[TeamSimOffensivePrefStart + i],
                    string.Format("Team index {0}'s offensive tendency byte should be untouched.", i));
            }
        }

        [TestMethod]
        public void GenesisRom_SetTeamSimOffensePref_RejectsOutOfRangeValue()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            int before = genesisTool.GetTeamSimOffensePref("bills");
            bool result = genesisTool.SetTeamSimOffensePref("bills", 4);

            Assert.IsFalse(result);
            Assert.AreEqual(before, genesisTool.GetTeamSimOffensePref("bills"));
        }
    }
}
