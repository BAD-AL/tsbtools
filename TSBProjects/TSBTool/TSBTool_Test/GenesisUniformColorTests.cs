using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Uniform colors: 16-color (32-byte) block per team at 0x58974 + teamIndex*0x40, with a second,
    /// structurally identical block +0x20 further in. Confirmed against two real screenshots the
    /// project owner provided (Bills: red helmet/blue jersey/white pants; Jets: green helmet/green
    /// jersey) -- jersey/pants stay byte-identical between the two blocks, only helmet genuinely
    /// differs, matching SNES_TecmoTool's own confirmed Home/Away helmet-independence finding. Treated
    /// as Home (offset 0)/Away (offset 0x20) on that basis; not yet confirmed against an away-game
    /// screenshot. Wire format (28 hex digits: pants1,jersey2,pants2,jersey3,pants3,helmetDark,
    /// helmetMedium) matches SNES_TecmoTool.SetHomeUniform/SetAwayUniform exactly, reusing
    /// InputParser's existing Uniform1/Uniform2 regex unmodified. See Genesis_TSB1_ROM_Findings.md's
    /// uniform-colors section.
    /// </summary>
    [TestClass]
    public class GenesisUniformColorTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int UniformColorStart = 0x58974;
        private const int UniformTeamStride = 0x40;

        // Decoded directly from the stock ROM and cross-checked against
        // Bills_v_jets_genesis_TSB1_Coin_flip.png / _kickoff.png (red helmet, blue jersey, white pants).
        private const string BillsHomeColorString = "04440a200aaa0e400eee0006022e";
        private const string BillsAwayColorString = "04440a200aaa0e400eee06660eee";

        [TestMethod]
        public void GenesisRom_GetHomeUniform_MatchesStockBillsColors()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("Uniform1=0x" + BillsHomeColorString, tool.GetHomeUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_GetAwayUniform_MatchesStockBillsColors()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("Uniform2=0x" + BillsAwayColorString, tool.GetAwayUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_GetGameUniform_ReturnsHomeAndAwayCombined()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual(
                "Uniform1=0x" + BillsHomeColorString + ", Uniform2=0x" + BillsAwayColorString,
                tool.GetGameUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_SetHomeUniform_WritesExpectedBytesAndRoundTrips()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            // pants1=0111 jersey2=2222 pants2=3333 jersey3=4444 pants3=5555 helmetDark=6666 helmetMedium=7777
            string newColors = "0111222233334444555566667777";

            tool.SetHomeUniform("bills", newColors);

            Assert.AreEqual("Uniform1=0x" + newColors, tool.GetHomeUniform("bills"));

            int baseLoc = UniformColorStart + Genesis_TSB1Tool.GetTeamIndex("bills") * UniformTeamStride;
            Assert.AreEqual((byte)0x01, tool.OutputRom[baseLoc + 0x06], "pants1 hi byte");
            Assert.AreEqual((byte)0x11, tool.OutputRom[baseLoc + 0x07], "pants1 lo byte");
            Assert.AreEqual((byte)0x22, tool.OutputRom[baseLoc + 0x02], "jersey2 hi byte");
            Assert.AreEqual((byte)0x22, tool.OutputRom[baseLoc + 0x03], "jersey2 lo byte");
            Assert.AreEqual((byte)0x33, tool.OutputRom[baseLoc + 0x08], "pants2 hi byte");
            Assert.AreEqual((byte)0x44, tool.OutputRom[baseLoc + 0x04], "jersey3 hi byte");
            Assert.AreEqual((byte)0x55, tool.OutputRom[baseLoc + 0x0A], "pants3 hi byte");
            Assert.AreEqual((byte)0x66, tool.OutputRom[baseLoc + 0x18], "helmetDark hi byte");
            Assert.AreEqual((byte)0x66, tool.OutputRom[baseLoc + 0x19], "helmetDark lo byte");
            Assert.AreEqual((byte)0x77, tool.OutputRom[baseLoc + 0x1A], "helmetMedium hi byte");
            Assert.AreEqual((byte)0x77, tool.OutputRom[baseLoc + 0x1B], "helmetMedium lo byte");
        }

        [TestMethod]
        public void GenesisRom_SetHomeUniform_DoesNotTouchAwayBlockOrUnexposedSlotsOrOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string coltsHomeBefore = tool.GetHomeUniform("colts");
            string billsAwayBefore = tool.GetAwayUniform("bills");
            int baseLoc = UniformColorStart + Genesis_TSB1Tool.GetTeamIndex("bills") * UniformTeamStride;
            byte jersey0HiBefore = tool.OutputRom[baseLoc + 0x00];
            byte jersey0LoBefore = tool.OutputRom[baseLoc + 0x01];
            byte skinHiBefore = tool.OutputRom[baseLoc + 0x0C];
            byte facemaskHiBefore = tool.OutputRom[baseLoc + 0x1C];

            tool.SetHomeUniform("bills", "0111222233334444555566667777");

            Assert.AreEqual(coltsHomeBefore, tool.GetHomeUniform("colts"), "Colts' home uniform must be untouched.");
            Assert.AreEqual(billsAwayBefore, tool.GetAwayUniform("bills"), "Bills' away uniform must be untouched.");
            Assert.AreEqual(jersey0HiBefore, tool.OutputRom[baseLoc + 0x00], "jersey index 0 must be left untouched (mirrors SNES's number-outline exclusion).");
            Assert.AreEqual(jersey0LoBefore, tool.OutputRom[baseLoc + 0x01]);
            Assert.AreEqual(skinHiBefore, tool.OutputRom[baseLoc + 0x0C], "skin-tone-like byte must be left untouched.");
            Assert.AreEqual(facemaskHiBefore, tool.OutputRom[baseLoc + 0x1C], "helmet facemask/accent byte must be left untouched.");
        }

        [TestMethod]
        public void GenesisRom_SetAwayUniform_WritesAwayBlockOnlyAndRoundTrips()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string billsHomeBefore = tool.GetHomeUniform("bills");

            tool.SetAwayUniform("bills", "0111222233334444555566667777");

            Assert.AreEqual("Uniform2=0x0111222233334444555566667777", tool.GetAwayUniform("bills"));
            Assert.AreEqual(billsHomeBefore, tool.GetHomeUniform("bills"), "Bills' home uniform must be untouched.");
        }

        [TestMethod]
        public void GenesisRom_SetHomeUniform_RejectsWrongLengthColorString()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string before = tool.GetHomeUniform("bills");

            tool.SetHomeUniform("bills", "1234");

            Assert.AreEqual(before, tool.GetHomeUniform("bills"), "A wrong-length color string must reject the whole write.");
        }

        [TestMethod]
        public void GenesisRom_SetHomeUniform_RejectsInvalidTeam()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            // Must not throw -- StaticUtils.AddError records the problem instead, matching the
            // established convention for invalid-argument tests elsewhere in this project (e.g.
            // GenesisReturnTeamTests.GenesisRom_SetReturnTeam_RejectsInvalidPosition).
            tool.SetHomeUniform("not_a_team", "0111222233334444555566667777");
        }

        [TestMethod]
        public void GenesisRom_ProcessText_ColorsLine_RoundTripsHomeAndAwayUniform()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.ProcessText("TEAM = bills SimData=0x0\nCOLORS Uniform1=0x0111222233334444555566667777, Uniform2=0x7777666655554444333322221111\n");

            Assert.AreEqual("Uniform1=0x0111222233334444555566667777", tool.GetHomeUniform("bills"));
            Assert.AreEqual("Uniform2=0x7777666655554444333322221111", tool.GetAwayUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_GetTeamPlayers_IncludesColorsLineWhenShowColorsIsEnabled()
        {
            bool originalShowColors = TecmoTool.ShowColors;
            try
            {
                TecmoTool.ShowColors = true;
                byte[] rom = TestRoms.LoadRom(RomFileName);
                Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

                string output = tool.GetTeamPlayers("bills");

                StringAssert.Contains(output, "COLORS " + tool.GetHomeUniform("bills") + ", " + tool.GetAwayUniform("bills"));
            }
            finally
            {
                TecmoTool.ShowColors = originalShowColors;
            }
        }

        // In-game palette (confirmed with emulator screenshots of a test ROM): 0x57D6C + team*0x40, two 0x20 blocks
        // (light-skin players, dark-skin players), words [0-1]=helmet dark,light [2-3]=jersey dark,light [4-5]=pants dark,light.
        private const int InGameStart = 0x57D6C;
        // Bills: helmet 0008 000e, jersey 0600 0e40, pants 0666 0eee.
        private const string BillsInGame = "0008000e06000e4006660eee";

        [TestMethod]
        public void GenesisRom_GetInGameUniform_MatchesStockBillsColors()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("InGame=0x" + BillsInGame, tool.GetInGameUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_SetInGameUniform_WritesBothSkinBlocksAndLeavesSkinAndOtherTeamsAlone()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            int baseLoc = InGameStart + Genesis_TSB1Tool.GetTeamIndex("bills") * 0x40;
            byte[] skinA = new byte[] { tool.OutputRom[baseLoc + 0x0C], tool.OutputRom[baseLoc + 0x0D] };
            byte[] skinB = new byte[] { tool.OutputRom[baseLoc + 0x2C], tool.OutputRom[baseLoc + 0x2D] };
            string coltsBefore = tool.GetInGameUniform("colts");
            string coinBefore = tool.GetHomeUniform("bills");

            tool.SetInGameUniform("bills", "111122223333444455556666");

            Assert.AreEqual("InGame=0x111122223333444455556666", tool.GetInGameUniform("bills"));
            for (int block = 0; block < 2; block++)
                for (int i = 0; i < 12; i++)
                    Assert.AreEqual(new byte[] { 0x11, 0x11, 0x22, 0x22, 0x33, 0x33, 0x44, 0x44, 0x55, 0x55, 0x66, 0x66 }[i],
                        tool.OutputRom[baseLoc + block * 0x20 + i], "block " + block + " byte " + i);
            Assert.AreEqual(skinA[0], tool.OutputRom[baseLoc + 0x0C]);
            Assert.AreEqual(skinA[1], tool.OutputRom[baseLoc + 0x0D]);
            Assert.AreEqual(skinB[0], tool.OutputRom[baseLoc + 0x2C]);
            Assert.AreEqual(skinB[1], tool.OutputRom[baseLoc + 0x2D]);
            Assert.AreEqual(coltsBefore, tool.GetInGameUniform("colts"));
            Assert.AreEqual(coinBefore, tool.GetHomeUniform("bills"), "the coin-toss colors are a separate table");
        }

        [TestMethod]
        public void GenesisRom_SetInGameUniform_RejectsWrongLength()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetInGameUniform("bills", "1234");

            Assert.AreEqual("InGame=0x" + BillsInGame, tool.GetInGameUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_ProcessText_ColorsLine_AppliesInGameUniform()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.ProcessText("TEAM = bills SimData=0x0\nCOLORS InGame=0x111122223333444455556666\n");

            Assert.AreEqual("InGame=0x111122223333444455556666", tool.GetInGameUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_GetTeamPlayers_ColorsLineIncludesInGameUniform()
        {
            bool original = TecmoTool.ShowColors;
            try
            {
                TecmoTool.ShowColors = true;
                byte[] rom = TestRoms.LoadRom(RomFileName);
                Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

                StringAssert.Contains(tool.GetTeamPlayers("bills"), ", " + tool.GetInGameUniform("bills"));
            }
            finally { TecmoTool.ShowColors = original; }
        }

        // The coin-toss and in-game home tables have no entry of their own for the Patriots: the game reads entry 13
        // (the Seahawks') for them (emulator-confirmed); entry 3 is read by no team.
        private static byte[] Slice(byte[] rom, int loc, int len)
        {
            byte[] r = new byte[len];
            System.Array.Copy(rom, loc, r, 0, len);
            return r;
        }

        [TestMethod]
        public void GenesisRom_SetHomeUniform_Patriots_WritesTheSeahawksEntryAndLeavesEntry3Alone()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] entry3Before = Slice(tool.OutputRom, UniformColorStart + 3 * UniformTeamStride, UniformTeamStride);

            tool.SetHomeUniform("patriots", "0111222233334444555566667777");

            Assert.AreEqual("Uniform1=0x0111222233334444555566667777", tool.GetHomeUniform("seahawks"), "patriots and seahawks share one entry");
            Assert.AreEqual("Uniform1=0x0111222233334444555566667777", tool.GetHomeUniform("patriots"));
            Assert.AreEqual((byte)0x66, tool.OutputRom[UniformColorStart + 13 * UniformTeamStride + 0x18], "helmetDark lands in entry 13");
            CollectionAssert.AreEqual(entry3Before, Slice(tool.OutputRom, UniformColorStart + 3 * UniformTeamStride, UniformTeamStride), "entry 3 is read by no team and must not change");
        }

        [TestMethod]
        public void GenesisRom_SetInGameUniform_Patriots_WritesTheSeahawksEntryAndLeavesEntry3Alone()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] entry3Before = Slice(tool.OutputRom, InGameStart + 3 * 0x40, 0x40);

            tool.SetInGameUniform("patriots", "111122223333444455556666");

            Assert.AreEqual("InGame=0x111122223333444455556666", tool.GetInGameUniform("seahawks"));
            Assert.AreEqual("InGame=0x111122223333444455556666", tool.GetInGameUniform("patriots"));
            Assert.AreEqual((byte)0x11, tool.OutputRom[InGameStart + 13 * 0x40], "helmet dark lands in entry 13");
            CollectionAssert.AreEqual(entry3Before, Slice(tool.OutputRom, InGameStart + 3 * 0x40, 0x40), "entry 3 must not change");
        }

        [TestMethod]
        public void GenesisRom_SetAwayUniform_Patriots_StillUsesTheSharedEntry()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetAwayUniform("patriots", "0111222233334444555566667777");

            Assert.AreEqual("Uniform2=0x0111222233334444555566667777", tool.GetAwayUniform("seahawks"));
        }

        [TestMethod]
        public void GenesisRom_UniformIndex_OtherTeamsKeepTheirRosterIndex()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetInGameUniform("colts", "111122223333444455556666");

            Assert.AreEqual((byte)0x11, tool.OutputRom[InGameStart + 1 * 0x40], "colts stay at entry 1");
            Assert.AreNotEqual("InGame=0x111122223333444455556666", tool.GetInGameUniform("bills"));
            Assert.AreNotEqual("InGame=0x111122223333444455556666", tool.GetInGameUniform("seahawks"));
        }

        // Away (light) in-game uniforms: a second table of 19 entries at 0x5846C, 0x40 per entry, same word layout as the
        // home table. Only some teams have an entry (entries 0-18 in this team order); the others never get a separate away look.
        private const int AwayStart = 0x5846C;
        private static readonly string[] AwayTeams = { "bills", "dolphins", "jets", "bengals", "steelers", "chiefs", "raiders", "chargers",
            "seahawks", "cowboys", "redskins", "eagles", "cardinals", "bears", "lions", "packers", "49ers", "rams", "saints" };

        [TestMethod]
        public void GenesisRom_GetAwayInGameUniform_MatchesStockBillsColors()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            // helmet 0008/000e (red), jersey 0666/0eee (gray/white), pants 0444/0aaa
            Assert.AreEqual("InGameAway=0x0008000e06660eee04440aaa", tool.GetAwayInGameUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_AwayInGameUniform_EachTeamUsesItsOwnEntry()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            for (int entry = 0; entry < AwayTeams.Length; entry++)
            {
                string hex = tool.GetAwayInGameUniform(AwayTeams[entry]);
                int loc = AwayStart + entry * 0x40;
                string expected = string.Format("InGameAway=0x{0:x2}{1:x2}{2:x2}{3:x2}{4:x2}{5:x2}{6:x2}{7:x2}{8:x2}{9:x2}{10:x2}{11:x2}",
                    tool.OutputRom[loc], tool.OutputRom[loc + 1], tool.OutputRom[loc + 2], tool.OutputRom[loc + 3], tool.OutputRom[loc + 4], tool.OutputRom[loc + 5],
                    tool.OutputRom[loc + 6], tool.OutputRom[loc + 7], tool.OutputRom[loc + 8], tool.OutputRom[loc + 9], tool.OutputRom[loc + 10], tool.OutputRom[loc + 11]);
                Assert.AreEqual(expected, hex, AwayTeams[entry]);
            }
        }

        [TestMethod]
        public void GenesisRom_AwayInGameUniform_TeamsWithoutAnEntryHaveNone()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] before = (byte[])tool.OutputRom.Clone();

            foreach (string team in new[] { "colts", "patriots", "browns", "oilers", "broncos", "giants", "vikings", "buccaneers", "falcons" })
            {
                Assert.AreEqual(string.Empty, tool.GetAwayInGameUniform(team), team);
                tool.SetAwayInGameUniform(team, "111122223333444455556666");
            }
            CollectionAssert.AreEqual(before, tool.OutputRom, "a team without an away entry must not change the ROM");
        }

        [TestMethod]
        public void GenesisRom_SetAwayInGameUniform_WritesBothSkinBlocksAndLeavesTheRestAlone()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            int baseLoc = AwayStart + 2 * 0x40;                    // jets = entry 2
            byte[] skinA = Slice(tool.OutputRom, baseLoc + 0x0C, 8), skinB = Slice(tool.OutputRom, baseLoc + 0x2C, 8);
            string billsBefore = tool.GetAwayInGameUniform("bills"), homeBefore = tool.GetInGameUniform("jets");

            tool.SetAwayInGameUniform("jets", "111122223333444455556666");

            Assert.AreEqual("InGameAway=0x111122223333444455556666", tool.GetAwayInGameUniform("jets"));
            byte[] expected = { 0x11, 0x11, 0x22, 0x22, 0x33, 0x33, 0x44, 0x44, 0x55, 0x55, 0x66, 0x66 };
            for (int block = 0; block < 2; block++)
                for (int i = 0; i < 12; i++)
                    Assert.AreEqual(expected[i], tool.OutputRom[baseLoc + block * 0x20 + i], "block " + block + " byte " + i);
            CollectionAssert.AreEqual(skinA, Slice(tool.OutputRom, baseLoc + 0x0C, 8));
            CollectionAssert.AreEqual(skinB, Slice(tool.OutputRom, baseLoc + 0x2C, 8));
            Assert.AreEqual(billsBefore, tool.GetAwayInGameUniform("bills"));
            Assert.AreEqual(homeBefore, tool.GetInGameUniform("jets"), "the home uniform is a separate table");
        }

        [TestMethod]
        public void GenesisRom_SetAwayInGameUniform_RejectsWrongLength()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string before = tool.GetAwayInGameUniform("bills");

            tool.SetAwayInGameUniform("bills", "1234");

            Assert.AreEqual(before, tool.GetAwayInGameUniform("bills"));
        }

        [TestMethod]
        public void GenesisRom_ProcessText_ColorsLine_AppliesAwayInGameUniform()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string homeBefore = tool.GetInGameUniform("bills");

            tool.ProcessText("TEAM = bills SimData=0x0\nCOLORS InGameAway=0x111122223333444455556666\n");

            Assert.AreEqual("InGameAway=0x111122223333444455556666", tool.GetAwayInGameUniform("bills"));
            Assert.AreEqual(homeBefore, tool.GetInGameUniform("bills"), "InGameAway must not be mistaken for InGame");
        }

        [TestMethod]
        public void GenesisRom_GetTeamPlayers_ColorsLineIncludesAwayUniformOnlyForTeamsThatHaveOne()
        {
            bool original = TecmoTool.ShowColors;
            try
            {
                TecmoTool.ShowColors = true;
                byte[] rom = TestRoms.LoadRom(RomFileName);
                Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

                StringAssert.Contains(tool.GetTeamPlayers("bills"), ", " + tool.GetAwayInGameUniform("bills"));
                StringAssert.DoesNotMatch(tool.GetTeamPlayers("colts"), new System.Text.RegularExpressions.Regex("InGameAway"));
            }
            finally { TecmoTool.ShowColors = original; }
        }

        [TestMethod]
        public void GenesisRom_GetTeamPlayers_OmitsColorsLineByDefault()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string output = tool.GetTeamPlayers("bills");

            StringAssert.DoesNotMatch(output, new System.Text.RegularExpressions.Regex("^COLORS ", System.Text.RegularExpressions.RegexOptions.Multiline));
        }
    }
}
