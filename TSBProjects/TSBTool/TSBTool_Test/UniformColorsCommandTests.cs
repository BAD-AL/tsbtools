using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Verifies the InputParser "COLORS Uniform1=0x..., Uniform2=0x..., DivChamp=0x..., ConfChamp=0x...,
    /// UniformUsage=0x..." command -- never covered by any prior test (no existing fixture passes
    /// "-colors", and none of the Input fixtures contain a COLORS line).
    ///
    /// Unlike the team-string-table command, these are fixed-position, per-team-indexed writes (no
    /// shifting), so the main risk is location-offset mistakes rather than corrupting neighbors --
    /// still worth an explicit isolation check given TeamStringsCommandTests just found a real bug
    /// in a similarly "should be safe" fixed-offset scheme.
    ///
    /// GetHomeUniform/GetAwayUniform aren't part of ITecmoTool (only the combined GetGameUniform is),
    /// so round-trip checks read back through GetGameUniform's "Uniform1=..., Uniform2=..." format.
    ///
    /// SNES_TecmoTool's equivalents are all no-op stubs (commented-out bodies), so there's nothing to
    /// test there. CXRomTSBTool only overrides the location functions (GetUniformLoc etc.), not the
    /// read/write logic itself, so it's covered by testing both a regular team (teamIndex &lt; 28,
    /// uses the inherited base offsets) and an expansion team (teamIndex &gt;= 30, uses CXRomTSBTool's
    /// own offsets) against the same CXROM_v105 ROM.
    /// </summary>
    [TestClass]
    public class UniformColorsCommandTests
    {
        private const string ColorsLine =
            "COLORS Uniform1=0x010203, Uniform2=0x040506, DivChamp=0x0708090a0b, ConfChamp=0x0c0d0e0f, UniformUsage=0x10111213";

        private static void ApplyColors(ITecmoTool tool, string team)
        {
            tool.ProcessText(string.Format("TEAM = {0} SimData=0x0\n{1}", team, ColorsLine));
        }

        private static void AssertColorsRoundTrip(ITecmoTool tool, string team)
        {
            Assert.AreEqual("Uniform1=0x010203, Uniform2=0x040506", tool.GetGameUniform(team));
            Assert.AreEqual("DivChamp=0x0708090a0b", tool.GetDivChampColors(team));
            Assert.AreEqual("ConfChamp=0x0c0d0e0f", tool.GetConfChampColors(team));
            Assert.AreEqual("UniformUsage=0x10111213", tool.GetUniformUsage(team));
        }

        [TestMethod]
        public void StockNesRom_ColorsCommand_RoundTripsForEditedTeam()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplyColors(tool, "bills");

            AssertColorsRoundTrip(tool, "bills");
        }

        [TestMethod]
        public void StockNesRom_ColorsCommand_DoesNotAffectOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string coltsUniformBefore = tool.GetGameUniform("colts");
            string coltsDivBefore = tool.GetDivChampColors("colts");
            string coltsConfBefore = tool.GetConfChampColors("colts");
            string coltsUsageBefore = tool.GetUniformUsage("colts");

            ApplyColors(tool, "bills");

            Assert.AreEqual(coltsUniformBefore, tool.GetGameUniform("colts"));
            Assert.AreEqual(coltsDivBefore, tool.GetDivChampColors("colts"));
            Assert.AreEqual(coltsConfBefore, tool.GetConfChampColors("colts"));
            Assert.AreEqual(coltsUsageBefore, tool.GetUniformUsage("colts"));
        }

        /// <summary>
        /// SetHomeUniform/SetAwayUniform/SetUniformUsage also mirror pants/jersey colors into a
        /// second "action sequence" palette location, which has no public getter (GetGameUniform only
        /// ever reads the primary location), so the round-trip tests above can't see it. Check those
        /// bytes directly. Location replicates TecmoTool's private constant (BillsActionSeqLoc=
        /// 0x342d8, +0x8 per team) since it's not exposed publicly.
        /// </summary>
        [TestMethod]
        public void StockNesRom_ColorsCommand_WritesActionSequenceMirrorForBills()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplyColors(tool, "bills"); // team index 0

            const int billsActionSeqLoc = 0x342d8;
            Assert.AreEqual((byte)0x01, rom[billsActionSeqLoc], "home pants mirror");
            Assert.AreEqual((byte)0x03, rom[billsActionSeqLoc + 1], "home jersey mirror");
            Assert.AreEqual((byte)0x04, rom[billsActionSeqLoc + 2], "away pants mirror");
            Assert.AreEqual((byte)0x06, rom[billsActionSeqLoc + 3], "away jersey mirror");
            Assert.AreEqual((byte)0x10, rom[billsActionSeqLoc + 4], "uniform usage mirror byte 0");
            Assert.AreEqual((byte)0x11, rom[billsActionSeqLoc + 5], "uniform usage mirror byte 1");
            Assert.AreEqual((byte)0x12, rom[billsActionSeqLoc + 6], "uniform usage mirror byte 2");
            Assert.AreEqual((byte)0x13, rom[billsActionSeqLoc + 7], "uniform usage mirror byte 3");
        }

        [TestMethod]
        public void CXRomV105_ColorsCommand_RoundTripsForRegularTeam()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplyColors(tool, "bills"); // teamIndex < 28: uses the inherited base offsets

            AssertColorsRoundTrip(tool, "bills");
        }

        [TestMethod]
        public void CXRomV105_ColorsCommand_RoundTripsForExpansionTeam()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplyColors(tool, "cardinals"); // teamIndex 33: uses CXRomTSBTool's expansion-team offsets

            AssertColorsRoundTrip(tool, "cardinals");
        }

        [TestMethod]
        public void CXRomV105_ColorsCommand_ExpansionTeamDoesNotAffectRegularOrOtherExpansionTeams()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string billsUniformBefore = tool.GetGameUniform("bills");
            string fortyNinersUniformBefore = tool.GetGameUniform("49ers"); // another expansion team

            ApplyColors(tool, "cardinals");

            Assert.AreEqual(billsUniformBefore, tool.GetGameUniform("bills"));
            Assert.AreEqual(fortyNinersUniformBefore, tool.GetGameUniform("49ers"));
        }
    }
}
