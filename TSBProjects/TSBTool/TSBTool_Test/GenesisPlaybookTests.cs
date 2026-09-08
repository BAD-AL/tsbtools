using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 2 of compressed-riding-rossum.md: playbooks. Flat 4-bytes-per-team array at
    /// 0x4CC4 + teamIndex*4, no season dimension -- so the important check is a byte-diff (only the
    /// edited team's 4 bytes changed, with the exact nibble packing expected), not a round trip through
    /// a pointer table like Phase 1.
    /// </summary>
    [TestClass]
    public class GenesisPlaybookTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int PlaybookStart = 0x4CC4;

        [TestMethod]
        public void GenesisRom_PlaybookCommand_ChangesExactlyFourBytesWithExpectedNibblePacking()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            Assert.AreEqual(0, teamIndex);

            int loc = PlaybookStart + teamIndex * 4;
            byte[] expected = { 0x01, 0x23, 0x45, 0x67 }; // (R1<<4)|R2, (R3<<4)|R4, (P1<<4)|P2, (P3<<4)|P4 for R1234/P5678
            int expectedChangedByteCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (originalRom[loc + i] != expected[i])
                    expectedChangedByteCount++;
            }

            tool.ProcessText("TEAM = bills SimData=0x0\nPLAYBOOK R1234, P5678");

            Assert.AreEqual(expected[0], tool.OutputRom[loc], "byte0 = (R1<<4)|R2 = (0<<4)|1");
            Assert.AreEqual(expected[1], tool.OutputRom[loc + 1], "byte1 = (R3<<4)|R4 = (2<<4)|3");
            Assert.AreEqual(expected[2], tool.OutputRom[loc + 2], "byte2 = (P1<<4)|P2 = (4<<4)|5");
            Assert.AreEqual(expected[3], tool.OutputRom[loc + 3], "byte3 = (P3<<4)|P4 = (6<<4)|7");

            int differences = 0;
            for (int i = 0; i < originalRom.Length; i++)
            {
                if (originalRom[i] != tool.OutputRom[i])
                    differences++;
            }
            Assert.AreEqual(expectedChangedByteCount, differences,
                "Only the Bills' playbook bytes that actually differ from the stock ROM should have changed " +
                "(the stock ROM's byte0 happens to already equal 0x01, which is what R1234 also produces).");
        }

        [TestMethod]
        public void GenesisRom_GetPlaybook_RoundTripsThroughSetPlaybook()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.ProcessText("TEAM = cowboys SimData=0x0\nPLAYBOOK R8765, P4321");

            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;
            Assert.AreEqual("PLAYBOOK R8765, P4321 ", genesisTool.GetPlaybook("cowboys"));
        }
    }
}
