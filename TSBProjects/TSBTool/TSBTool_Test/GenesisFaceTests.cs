using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// "Face" on Genesis (like SNES, unlike NES) isn't a real per-player portrait index -- it's a
    /// skin-tone flag written to the same attribute-record byte (+2) the Set*PlayerAbilities methods
    /// leave alone. Confirmed by reading SNES_TecmoTool.SetFace directly (it collapses its wide
    /// 0x00-0xD4 "face" input, inherited from NES's real face system, down to skin=(face&lt;0x53)?0x00:0x80)
    /// and cross-checking Genesis's stock +2 bytes against SNES's for the same real players (13/13
    /// exact match). See Genesis_TSB1_ROM_Findings.md's attributes section.
    /// </summary>
    [TestClass]
    public class GenesisFaceTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int AttributeRecordStart = 0x89FC;
        private const int AttributeRecordSize = 5;

        private static int Loc(int teamIndex, int positionIndex)
        {
            return AttributeRecordStart + (teamIndex * 32 + positionIndex) * AttributeRecordSize + 2;
        }

        [TestMethod]
        public void GenesisRom_SetFace_BelowThreshold_WritesLightSkinFlag()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetFace("bills", "RB1", 0x00);
            Assert.AreEqual((byte)0x00, tool.OutputRom[Loc(0, 2)]);

            tool.SetFace("bills", "RB1", 0x52); // just below the threshold
            Assert.AreEqual((byte)0x00, tool.OutputRom[Loc(0, 2)]);
        }

        [TestMethod]
        public void GenesisRom_SetFace_AtOrAboveThreshold_WritesDarkSkinFlag()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetFace("bills", "RB1", 0x53); // threshold
            Assert.AreEqual((byte)0x80, tool.OutputRom[Loc(0, 2)]);

            tool.SetFace("bills", "RB1", 0xD4); // max valid value
            Assert.AreEqual((byte)0x80, tool.OutputRom[Loc(0, 2)]);
        }

        [TestMethod]
        public void GenesisRom_SetFace_RejectsOutOfRangeValue()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            byte before = tool.OutputRom[Loc(0, 2)];

            tool.SetFace("bills", "RB1", 0xD5); // one past the max valid value

            Assert.AreEqual(before, tool.OutputRom[Loc(0, 2)]);
        }

        [TestMethod]
        public void GenesisRom_GetFace_ReturnsStoredSkinByte()
        {
            // Matches SNES_TecmoTool.GetFace's own (lossy) behavior: it returns whatever raw byte is
            // stored at +2, not a reconstructed original "face" value.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetFace("bills", "RB1", 0x99);
            Assert.AreEqual(0x80, tool.GetFace("bills", "RB1"));

            tool.SetFace("bills", "RB1", 0x10);
            Assert.AreEqual(0x00, tool.GetFace("bills", "RB1"));
        }

        [TestMethod]
        public void GenesisRom_SetFace_DoesNotAffectOtherPlayersOrOtherBytes()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int rb1Record = AttributeRecordStart + (0 * 32 + 2) * AttributeRecordSize;
            byte before0 = tool.OutputRom[rb1Record];
            byte before1 = tool.OutputRom[rb1Record + 1];
            byte before3 = tool.OutputRom[rb1Record + 3];
            byte before4 = tool.OutputRom[rb1Record + 4];
            byte wr1Before = tool.OutputRom[Loc(0, 6)];

            tool.SetFace("bills", "RB1", 0x99);

            Assert.AreEqual(before0, tool.OutputRom[rb1Record], "byte0 must be untouched.");
            Assert.AreEqual(before1, tool.OutputRom[rb1Record + 1], "byte1 must be untouched.");
            Assert.AreEqual(before3, tool.OutputRom[rb1Record + 3], "byte3 must be untouched.");
            Assert.AreEqual(before4, tool.OutputRom[rb1Record + 4], "byte4 must be untouched.");
            Assert.AreEqual(wr1Before, tool.OutputRom[Loc(0, 6)], "WR1's +2 byte must be untouched.");
        }

        [TestMethod]
        public void GenesisRom_NonQbRosterLineWithFaceField_NowRoundTripsThroughProcessText()
        {
            // Now that SetFace is real, InputParser's non-QB dispatchers (which call SetFace
            // unconditionally) no longer need to be bypassed -- confirms the whole line, abilities and
            // all, applies end-to-end.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;

            // The stock roster table has zero slack (see
            // GenesisRosterTests.GenesisRom_InsertPlayer_RejectsWriteThatWouldOverflowIntoAttributeTable_*),
            // so a genuinely longer name needs room freed up first -- shrink an unrelated, far-away
            // entry (49ers QB1) to make space before growing this one.
            genesisTool.InsertPlayer("49ers", "QB1", "x", "X", 0x00);
            tool.ProcessText("TEAM = bills SimData=0x0\nRB1, alexander THUNDERBOLT, Face=0x99, #22, 25, 69, 13, 13, 44, 50");

            Assert.AreEqual("alexanderTHUNDERBOLT", genesisTool.GetPlayerName("bills", "RB1"));
            Assert.AreEqual(0x80, genesisTool.GetFace("bills", "RB1"));
        }
    }
}
