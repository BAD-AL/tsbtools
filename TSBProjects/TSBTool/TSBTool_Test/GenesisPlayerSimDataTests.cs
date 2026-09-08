using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Player-level sim/CPU data (distinct from team-level sim data in GenesisTeamSimDataTests.cs).
    /// Offensive positions (QB1,QB2,RB1-4,WR1-4,TE1-2) at 0xDEA86 + teamIndex*0x30 + positionIndex*2;
    /// defensive positions (RE..SS) at 0x18/0x23 offsets within the same block; kicking/punting share
    /// one nibble-packed byte at 0x2E. All formulas confirmed identical to SNES_TecmoTool's own
    /// GetQBSimData/GetSkillSimData/GetDefensiveSimData/GetKickingSimData/GetPuntingSimData -- see
    /// Genesis_TSB1_ROM_Findings.md's sim/CPU data section. Genesis uses the plain main team order throughout
    /// (no SNES-style mSimTeams reordering quirk, confirmed by the fork investigation).
    /// </summary>
    [TestClass]
    public class GenesisPlayerSimDataTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int SimBlockStart = 0xDEA86;
        private const int SimBlockSize = 0x30;

        private static int OffensiveLoc(int teamIndex, int positionIndex)
        {
            return SimBlockStart + teamIndex * SimBlockSize + positionIndex * 2;
        }

        [TestMethod]
        public void GenesisRom_SetQBSimData_WritesNibblePairAndWholeByte()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            int loc = OffensiveLoc(teamIndex, 0); // QB1

            tool.SetQBSimData("bills", "QB1", new int[] { 3, 12, 200 });

            Assert.AreEqual((byte)((3 << 4) | 12), tool.OutputRom[loc], "byte0 = (data[0]<<4)|data[1]");
            Assert.AreEqual((byte)200, tool.OutputRom[loc + 1], "byte1 = data[2] as a whole byte, not nibble-split (matches SNES_TecmoTool.SetQBSimData exactly).");
        }

        [TestMethod]
        public void GenesisRom_SetQBSimData_QB2UsesNextTwoByteSlot()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            int qb1Loc = OffensiveLoc(teamIndex, 0);
            int qb2Loc = OffensiveLoc(teamIndex, 1);
            byte qb1Before0 = tool.OutputRom[qb1Loc];
            byte qb1Before1 = tool.OutputRom[qb1Loc + 1];

            tool.SetQBSimData("bills", "QB2", new int[] { 5, 9, 100 });

            Assert.AreEqual((byte)((5 << 4) | 9), tool.OutputRom[qb2Loc]);
            Assert.AreEqual((byte)100, tool.OutputRom[qb2Loc + 1]);
            Assert.AreEqual(qb1Before0, tool.OutputRom[qb1Loc], "QB1's slot must be untouched by a QB2 write.");
            Assert.AreEqual(qb1Before1, tool.OutputRom[qb1Loc + 1]);
        }

        [TestMethod]
        public void GenesisRom_SetSkillSimData_WritesFourNibbleValues()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            int loc = OffensiveLoc(teamIndex, 2); // RB1

            tool.SetSkillSimData("bills", "RB1", new int[] { 4, 7, 9, 2 });

            Assert.AreEqual((byte)((4 << 4) | 7), tool.OutputRom[loc]);
            Assert.AreEqual((byte)((9 << 4) | 2), tool.OutputRom[loc + 1]);
        }

        [TestMethod]
        public void GenesisRom_SetSkillSimData_RejectsNonSkillPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            byte before = tool.OutputRom[OffensiveLoc(teamIndex, 0)];

            // "C" (offensive line) has no sim-data slot at all -- must be rejected, not misapplied
            // to some other offensive slot.
            tool.SetSkillSimData("bills", "C", new int[] { 4, 7, 9, 2 });

            Assert.AreEqual(before, tool.OutputRom[OffensiveLoc(teamIndex, 0)]);
        }

        [TestMethod]
        public void GenesisRom_SetDefensiveSimData_WritesPassRushAndInterception()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            int passRushLoc = SimBlockStart + teamIndex * SimBlockSize + 0x18 + 0; // RE = defIndex 0
            int interceptionLoc = passRushLoc + 0xB;

            tool.SetDefensiveSimData("bills", "RE", new int[] { 200, 15 });

            Assert.AreEqual((byte)200, tool.OutputRom[passRushLoc]);
            Assert.AreEqual((byte)15, tool.OutputRom[interceptionLoc]);
        }

        [TestMethod]
        public void GenesisRom_SetDefensiveSimData_DoesNotAffectAdjacentPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            int ntPassRushLoc = SimBlockStart + teamIndex * SimBlockSize + 0x18 + 1; // NT = defIndex 1
            byte before = tool.OutputRom[ntPassRushLoc];

            tool.SetDefensiveSimData("bills", "RE", new int[] { 200, 15 });

            Assert.AreEqual(before, tool.OutputRom[ntPassRushLoc]);
        }

        [TestMethod]
        public void GenesisRom_SetKickingAndPuntingSimData_ShareOneNibblePackedByte()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int teamIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            int loc = SimBlockStart + teamIndex * SimBlockSize + 0x2E;
            int puntingNibbleBefore = tool.OutputRom[loc] & 0x0F;

            // Setting kicking must preserve whatever punting nibble was already there (matches
            // SNES_TecmoTool.SetKickingSimData's own "clear only the hi nibble" behavior) -- so the
            // low nibble here is the stock ROM's existing punting value, not assumed to be 0.
            tool.SetKickingSimData("bills", 9);
            Assert.AreEqual((byte)((9 << 4) | puntingNibbleBefore), tool.OutputRom[loc], "Kicking = hi nibble; existing punting nibble must be preserved.");

            tool.SetPuntingSimData("bills", 3);
            Assert.AreEqual((byte)((9 << 4) | 3), tool.OutputRom[loc], "Setting punting must preserve the already-set kicking nibble.");
        }

        [TestMethod]
        public void GenesisRom_SetKickingSimData_DoesNotAffectOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int coltsLoc = SimBlockStart + 1 * SimBlockSize + 0x2E;
            byte before = tool.OutputRom[coltsLoc];

            tool.SetKickingSimData("bills", 9);

            Assert.AreEqual(before, tool.OutputRom[coltsLoc]);
        }
    }
}
