using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 6 of compressed-riding-rossum.md: player attributes, QB1 (slot 0) only. Flat 5 bytes/player
    /// at 0x89FC + flatPlayerIndex*5; byte +2 (unresolved flag/tag byte) is intentionally left untouched
    /// -- open question #4, not guessed at here.
    /// </summary>
    [TestClass]
    public class GenesisAttributeTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int AttributeRecordStart = 0x89FC;
        private const int AttributeRecordSize = 5;

        private static readonly int[] AbilityScale = { 6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100 };

        [TestMethod]
        public void GenesisRom_QbAbilitiesCommand_RoundTripsThroughNibbleScale()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            int billsIndex = Genesis_TSB1Tool.GetTeamIndex("bills");
            Assert.AreEqual(0, billsIndex);
            int loc = AttributeRecordStart + billsIndex * AttributeRecordSize;
            byte originalFlagByte = romCopy[loc + 2];

            int rs = System.Array.IndexOf(AbilityScale, 25);
            int rp = System.Array.IndexOf(AbilityScale, 69);
            int ms = System.Array.IndexOf(AbilityScale, 13);
            int hp = System.Array.IndexOf(AbilityScale, 13);
            int ps = System.Array.IndexOf(AbilityScale, 31);
            int pc = System.Array.IndexOf(AbilityScale, 44);
            int pa = System.Array.IndexOf(AbilityScale, 50);
            int ar = System.Array.IndexOf(AbilityScale, 31);

            byte[] expected = { (byte)((rp << 4) | rs), (byte)((ms << 4) | hp), (byte)((ps << 4) | pc), (byte)((pa << 4) | ar) };
            int[] expectedLocs = { loc, loc + 1, loc + 3, loc + 4 };
            int expectedChangedByteCount = 0;
            for (int i = 0; i < expectedLocs.Length; i++)
            {
                if (originalRom[expectedLocs[i]] != expected[i])
                    expectedChangedByteCount++;
            }

            // runningSpeed=25, rushingPower=69, maxSpeed=13, hittingPower=13, passingSpeed=31,
            // passControl=44, accuracy=50, avoidPassBlock=31 -- all drawn from the 16-value ability
            // scale, matching the InputParser.cs source comment's own worked example.
            tool.ProcessText("TEAM = bills SimData=0x0\nQB1, jim kelly, #12, 25, 69, 13, 13, 31, 44, 50, 31");

            Assert.AreEqual(expected[0], tool.OutputRom[loc], "byte0 = (RushingPower<<4)|RunningSpeed");
            Assert.AreEqual(expected[1], tool.OutputRom[loc + 1], "byte1 = (MaxSpeed<<4)|HittingPower");
            Assert.AreEqual(originalFlagByte, tool.OutputRom[loc + 2], "byte2 (unresolved flag byte) must be left untouched.");
            Assert.AreEqual(expected[2], tool.OutputRom[loc + 3], "byte3 = (PassSpeed<<4)|PassControl");
            Assert.AreEqual(expected[3], tool.OutputRom[loc + 4], "byte4 = (PassAccuracy<<4)|Arm");

            // Every other player's 5-byte record should be untouched.
            int differences = 0;
            for (int i = 0; i < originalRom.Length; i++)
            {
                if (originalRom[i] != tool.OutputRom[i])
                    differences++;
            }
            Assert.AreEqual(expectedChangedByteCount, differences,
                "Only the attribute bytes that actually differ from the stock ROM should have changed.");
        }

        [TestMethod]
        public void GenesisRom_SetQBAbilities_RejectsInvalidAbilityScore()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            int loc = AttributeRecordStart; // bills, index 0
            byte before0 = tool.OutputRom[loc];
            byte before1 = tool.OutputRom[loc + 1];

            // 99 is not one of the 16 valid ability scores.
            tool.SetQBAbilities("bills", "QB1", 99, 69, 13, 13, 31, 44, 50, 31);

            Assert.AreEqual(before0, tool.OutputRom[loc], "An invalid ability score must reject the whole write.");
            Assert.AreEqual(before1, tool.OutputRom[loc + 1]);
        }

        [TestMethod]
        public void GenesisRom_SetQBAbilities_RejectsUnmappedPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            int loc = AttributeRecordStart + 1 * AttributeRecordSize; // colts, slot 1 (not QB1)
            byte before = tool.OutputRom[loc];

            tool.SetQBAbilities("colts", "WR1", 25, 69, 13, 13, 31, 44, 50, 31);

            Assert.AreEqual(before, tool.OutputRom[loc]);
        }

        // positionIndex within the 32-slot roster, per Genesis_TSB1_ROM_Findings.md's confirmed order.
        private const int Rb1Index = 2;
        private const int WrIndex = 6;
        private const int CIndex = 12;
        private const int ReIndex = 17;
        private const int KIndex = 28;

        private static int Idx(int score)
        {
            return System.Array.IndexOf(AbilityScale, score);
        }

        private static int Loc(int teamIndex, int positionIndex)
        {
            return AttributeRecordStart + (teamIndex * 32 + positionIndex) * AttributeRecordSize;
        }

        [TestMethod]
        public void GenesisRom_SkillPlayerAbilitiesCommand_WritesExpectedNibbles()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, Rb1Index); // bills RB1
            byte before2 = tool.OutputRom[loc + 2];
            byte before4 = tool.OutputRom[loc + 4];

            // RS=25,RP=69,MS=13,HP=13, ballControl=44,receptions=50
            tool.SetSkillPlayerAbilities("bills", "RB1", 25, 69, 13, 13, 44, 50);

            Assert.AreEqual((byte)((Idx(69) << 4) | Idx(25)), tool.OutputRom[loc], "byte0 = (RushingPower<<4)|RunningSpeed");
            Assert.AreEqual((byte)((Idx(13) << 4) | Idx(13)), tool.OutputRom[loc + 1], "byte1 = (MaxSpeed<<4)|HittingPower");
            Assert.AreEqual(before2, tool.OutputRom[loc + 2], "byte2 must be left untouched.");
            Assert.AreEqual((byte)((Idx(44) << 4) | Idx(50)), tool.OutputRom[loc + 3], "byte3 = (BallControl<<4)|Receptions for skill positions");
            Assert.AreEqual(before4, tool.OutputRom[loc + 4], "byte4 is QB-only, must stay untouched for RB1.");
        }

        [TestMethod]
        public void GenesisRom_SkillPlayerAbilities_RejectsNonSkillPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, CIndex);
            byte before = tool.OutputRom[loc];

            tool.SetSkillPlayerAbilities("bills", "C", 25, 69, 13, 13, 44, 50);

            Assert.AreEqual(before, tool.OutputRom[loc], "SetSkillPlayerAbilities must reject an OL position.");
        }

        [TestMethod]
        public void GenesisRom_OLPlayerAbilitiesCommand_WritesFirstTwoBytesOnly()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, CIndex); // bills C
            byte before2 = tool.OutputRom[loc + 2];
            byte before3 = tool.OutputRom[loc + 3];
            byte before4 = tool.OutputRom[loc + 4];

            tool.SetOLPlayerAbilities("bills", "C", 25, 69, 13, 13);

            Assert.AreEqual((byte)((Idx(69) << 4) | Idx(25)), tool.OutputRom[loc], "byte0 = (RushingPower<<4)|RunningSpeed");
            Assert.AreEqual((byte)((Idx(13) << 4) | Idx(13)), tool.OutputRom[loc + 1], "byte1 = (MaxSpeed<<4)|HittingPower");
            Assert.AreEqual(before2, tool.OutputRom[loc + 2], "byte2 must be left untouched.");
            Assert.AreEqual(before3, tool.OutputRom[loc + 3], "OL positions have no 3rd stat pair -- byte3 must not be written.");
            Assert.AreEqual(before4, tool.OutputRom[loc + 4], "byte4 is QB-only.");
        }

        [TestMethod]
        public void GenesisRom_OLPlayerAbilities_RejectsNonOLPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, Rb1Index);
            byte before = tool.OutputRom[loc];

            tool.SetOLPlayerAbilities("bills", "RB1", 25, 69, 13, 13);

            Assert.AreEqual(before, tool.OutputRom[loc], "SetOLPlayerAbilities must reject a non-OL position.");
        }

        [TestMethod]
        public void GenesisRom_DefensivePlayerAbilitiesCommand_WritesExpectedNibbles()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, ReIndex); // bills RE
            byte before2 = tool.OutputRom[loc + 2];
            byte before4 = tool.OutputRom[loc + 4];

            // RS=25,RP=69,MS=13,HP=13, passRush=56,interceptions=6
            tool.SetDefensivePlayerAbilities("bills", "RE", 25, 69, 13, 13, 56, 6);

            Assert.AreEqual((byte)((Idx(69) << 4) | Idx(25)), tool.OutputRom[loc]);
            Assert.AreEqual((byte)((Idx(13) << 4) | Idx(13)), tool.OutputRom[loc + 1]);
            Assert.AreEqual(before2, tool.OutputRom[loc + 2]);
            Assert.AreEqual((byte)((Idx(56) << 4) | Idx(6)), tool.OutputRom[loc + 3], "byte3 = (PassRush<<4)|Interceptions for defensive positions");
            Assert.AreEqual(before4, tool.OutputRom[loc + 4]);
        }

        [TestMethod]
        public void GenesisRom_DefensivePlayerAbilities_RejectsNonDefensivePosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, Rb1Index);
            byte before = tool.OutputRom[loc];

            tool.SetDefensivePlayerAbilities("bills", "RB1", 25, 69, 13, 13, 56, 6);

            Assert.AreEqual(before, tool.OutputRom[loc], "SetDefensivePlayerAbilities must reject a non-defensive position.");
        }

        [TestMethod]
        public void GenesisRom_KickPlayerAbilitiesCommand_WritesExpectedNibbles()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, KIndex); // bills K
            byte before2 = tool.OutputRom[loc + 2];
            byte before4 = tool.OutputRom[loc + 4];

            // RS=25,RP=69,MS=13,HP=13, kickingAbility=94,avoidKickBlock=19
            tool.SetKickPlayerAbilities("bills", "K", 25, 69, 13, 13, 94, 19);

            Assert.AreEqual((byte)((Idx(69) << 4) | Idx(25)), tool.OutputRom[loc]);
            Assert.AreEqual((byte)((Idx(13) << 4) | Idx(13)), tool.OutputRom[loc + 1]);
            Assert.AreEqual(before2, tool.OutputRom[loc + 2]);
            Assert.AreEqual((byte)((Idx(94) << 4) | Idx(19)), tool.OutputRom[loc + 3], "byte3 = (KickingAbility<<4)|AvoidKickBlock for K/P");
            Assert.AreEqual(before4, tool.OutputRom[loc + 4]);
        }

        [TestMethod]
        public void GenesisRom_KickPlayerAbilities_RejectsNonKickPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int loc = Loc(0, WrIndex);
            byte before = tool.OutputRom[loc];

            tool.SetKickPlayerAbilities("bills", "WR1", 25, 69, 13, 13, 94, 19);

            Assert.AreEqual(before, tool.OutputRom[loc], "SetKickPlayerAbilities must reject a non-K/P position.");
        }
    }
}
