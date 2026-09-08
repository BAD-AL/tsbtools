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
