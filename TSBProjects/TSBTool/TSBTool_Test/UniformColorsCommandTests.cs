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
    /// SNES_TecmoTool has its own real (non-stub) uniform implementation -- jersey+pants+helmet
    /// colors, 32 hex digits ordered jersey1,pants1,jersey2,pants2,jersey3,pants3,helmetDark,
    /// helmetMedium, mirrored into both the light-skin and dark-skin player blocks of whichever side
    /// (Home or Away) is being set -- covered separately below since the format and addresses are
    /// entirely different from the NES/CXRom scheme. UniformUsage is also real on SNES (the
    /// uniform-matchup table); DivChamp/ConfChamp remain no-op stubs (SNES's Division/Conference
    /// Championship screens were confirmed in-game to reuse the plain jersey/pants data directly, no
    /// separate table needed -- see Genesis_TSB1_plan.md). CXRomTSBTool
    /// only overrides the location functions (GetUniformLoc etc.), not the
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

        // SNES: pants+jersey+helmet colors, 28 hex digits ordered pants1,jersey2,pants2,jersey3,pants3,
        // helmetDark,helmetMedium (2 raw file-order bytes per SNES color, e.g. "FF7F" for white --
        // matches the community documentation's own convention). Real addresses: HOME light-skin block
        // at 0x158040 + teamIndex*0x40, HOME dark-skin at +0x20, AWAY light-skin at 0x158800 + same
        // stride, AWAY dark-skin at +0x20. Jersey = block bytes 08-0D, pants = block bytes 0E-13,
        // helmet = block bytes 14-17. Helmet is genuinely independent between Home and Away (confirmed
        // in a real emulator: different colors written to each rendered correctly in both a home and
        // an away game) -- deliberately not the separate "Large Helmet" (Team Menu/Matchup screens) or
        // "Mini Helmet" (Team Select screen) systems, both out of scope here.
        //
        // Jersey's "dark" shade (block byte 08) is deliberately absent from this format -- pixel
        // analysis of real sprite screenshots found it's the jersey-number outline color, not fabric
        // shading, so SetHomeUniform/SetAwayUniform never write it, leaving each team's stock value
        // (and its number-outline color) untouched. See StockSnesRom_ColorsCommand_DoesNotTouchJerseyDark.
        private const string SnesColorsLine =
            "COLORS Uniform1=0xccddeeff00112233445560708090, Uniform2=0x30405060708090a0b0c0a1b2c3d4";

        private static void ApplySnesColors(ITecmoTool tool, string team)
        {
            tool.ProcessText(string.Format("TEAM = {0} SimData=0x0\n{1}", team, SnesColorsLine));
        }

        [TestMethod]
        public void StockSnesRom_ColorsCommand_RoundTripsForEditedTeam()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplySnesColors(tool, "bills");

            Assert.AreEqual(
                "Uniform1=0xccddeeff00112233445560708090, Uniform2=0x30405060708090a0b0c0a1b2c3d4",
                tool.GetGameUniform("bills"));
        }

        /// <summary>
        /// Locks in the jersey-dark scope decision itself: applying a COLORS line must leave jersey's
        /// dark shade (block byte 08-09, the jersey-number outline color, not a shading tint) exactly
        /// as the stock ROM had it, for both Home and Away.
        /// </summary>
        [TestMethod]
        public void StockSnesRom_ColorsCommand_DoesNotTouchJerseyDark()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            byte homeJerseyDark0Before = rom[0x158040 + 0x08];
            byte homeJerseyDark1Before = rom[0x158040 + 0x09];
            byte awayJerseyDark0Before = rom[0x158800 + 0x08];
            byte awayJerseyDark1Before = rom[0x158800 + 0x09];

            ApplySnesColors(tool, "bills");

            Assert.AreEqual(homeJerseyDark0Before, rom[0x158040 + 0x08], "home jersey-dark byte 0");
            Assert.AreEqual(homeJerseyDark1Before, rom[0x158040 + 0x09], "home jersey-dark byte 1");
            Assert.AreEqual(awayJerseyDark0Before, rom[0x158800 + 0x08], "away jersey-dark byte 0");
            Assert.AreEqual(awayJerseyDark1Before, rom[0x158800 + 0x09], "away jersey-dark byte 1");
        }

        [TestMethod]
        public void StockSnesRom_ColorsCommand_DoesNotAffectOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string coltsUniformBefore = tool.GetGameUniform("colts");

            ApplySnesColors(tool, "bills");

            Assert.AreEqual(coltsUniformBefore, tool.GetGameUniform("colts"));
        }

        /// <summary>
        /// The light-skin and dark-skin player blocks must end up with identical jersey/pants bytes
        /// (confirmed in the real ROM that they always match, differing only in skin-tone colors) --
        /// check both blocks directly, since GetGameUniform only ever reads the light-skin block and
        /// so can't see a bug that wrote the dark-skin mirror incorrectly (or not at all).
        /// </summary>
        [TestMethod]
        public void StockSnesRom_ColorsCommand_MirrorsIntoLightAndDarkSkinBlocks()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplySnesColors(tool, "bills"); // team index 0, so no stride offset to account for

            // In ascending byte-offset order: jersey2 (0a-0b), jersey3 (0c-0d), pants1 (0e-0f),
            // pants2 (10-11), pants3 (12-13), helmetDark (14-15), helmetMedium (16-17). Jersey1
            // (08-09) is intentionally excluded -- covered separately by DoesNotTouchJerseyDark.
            byte[] expectedJerseyPantsHelmet = { 0xee,0xff, 0x22,0x33, 0xcc,0xdd, 0x00,0x11, 0x44,0x55, 0x60,0x70, 0x80,0x90 };
            int[] byteOffsets = { 0x0a,0x0b, 0x0c,0x0d, 0x0e,0x0f, 0x10,0x11, 0x12,0x13, 0x14,0x15, 0x16,0x17 };

            for (int i = 0; i < byteOffsets.Length; i++)
            {
                Assert.AreEqual(expectedJerseyPantsHelmet[i], rom[0x158040 + byteOffsets[i]], "home light-skin byte " + i);
                Assert.AreEqual(expectedJerseyPantsHelmet[i], rom[0x158060 + byteOffsets[i]], "home dark-skin byte " + i);
            }
        }

        /// <summary>
        /// The whole point of folding helmet color into Uniform1/Uniform2 rather than a separate
        /// shared field: Home and Away must be genuinely independent (confirmed in a real emulator --
        /// different helmet colors written to Home vs. Away both rendered correctly in a home game and
        /// an away game respectively). SetHomeUniform's helmet portion must never touch the Away
        /// blocks, and vice versa.
        /// </summary>
        [TestMethod]
        public void StockSnesRom_ColorsCommand_HomeAndAwayHelmetAreIndependent()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplySnesColors(tool, "bills"); // Home helmet = 60708090, Away helmet = a1b2c3d4

            byte[] expectedHome = { 0x60, 0x70, 0x80, 0x90 };
            byte[] expectedAway = { 0xa1, 0xb2, 0xc3, 0xd4 };
            foreach (int blockBase in new[] { 0x158040, 0x158060 }) // HOME-LT, HOME-DK
            {
                for (int i = 0; i < 4; i++)
                    Assert.AreEqual(expectedHome[i], rom[blockBase + 0x14 + i], "home block " + blockBase.ToString("x") + " helmet byte " + i);
            }
            foreach (int blockBase in new[] { 0x158800, 0x158820 }) // AWAY-LT, AWAY-DK
            {
                for (int i = 0; i < 4; i++)
                    Assert.AreEqual(expectedAway[i], rom[blockBase + 0x14 + i], "away block " + blockBase.ToString("x") + " helmet byte " + i);
            }
        }

        /// <summary>
        /// Locks in the underlying discovery: in the untouched stock ROM, the helmet portion of
        /// Uniform1 already decodes to real-world team helmet colors (Bills=red, Steelers=black --
        /// spot checked against six teams total when this location was found). If this ever starts
        /// failing, either the offset is wrong or the TSB1.smc test fixture changed.
        /// </summary>
        [TestMethod]
        public void StockSnesRom_ColorsCommand_StockHelmetValuesMatchRealTeamColors()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string billsUniform1 = tool.GetGameUniform("bills").Split(',')[0];
            string steelersUniform1 = tool.GetGameUniform("steelers").Split(',')[0];

            Assert.AreEqual("Uniform1=0x08218050945243759c730c009c10", billsUniform1, "Bills -- trailing 8 digits are dark/medium red");
            Assert.IsTrue(steelersUniform1.EndsWith("00000821"), "Steelers -- trailing 8 digits are black/dark gray");
        }

        // SNES uniform-matchup table: which of a team's two uniforms gets worn against each possible
        // opponent, keyed by opponent (not home/away game location). 4 bytes/32 bits per team at
        // x1752+, one bit per opponent, 1=dark/home, 0=light/away.
        private static void ApplyUniformUsage(ITecmoTool tool, string team, string usageHex)
        {
            tool.ProcessText(string.Format("TEAM = {0} SimData=0x0\nCOLORS UniformUsage=0x{1}", team, usageHex));
        }

        [TestMethod]
        public void StockSnesRom_UniformUsageCommand_RoundTripsForEditedTeam()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            ApplyUniformUsage(tool, "bills", "01020304");

            Assert.AreEqual("UniformUsage=0x01020304", tool.GetUniformUsage("bills"));
            Assert.AreEqual((byte)0x01, rom[0x1752]);
            Assert.AreEqual((byte)0x02, rom[0x1753]);
            Assert.AreEqual((byte)0x03, rom[0x1754]);
            Assert.AreEqual((byte)0x04, rom[0x1755]);
        }

        [TestMethod]
        public void StockSnesRom_UniformUsageCommand_DoesNotAffectOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string coltsBefore = tool.GetUniformUsage("colts");

            ApplyUniformUsage(tool, "bills", "01020304");

            Assert.AreEqual(coltsBefore, tool.GetUniformUsage("colts"));
        }

        /// <summary>
        /// Locks in the real-world fact this table was originally decoded and verified against: the
        /// Bills wear their "Light"/away uniform exactly once in the 1993 season, against the Giants
        /// (team index 15), regardless of home/away game location. Byte 1 covers opponent indices
        /// 8-15, and its LSB (index 15 = Giants) is the table's one 0 bit -- 0xFE = 11111110.
        /// </summary>
        [TestMethod]
        public void StockSnesRom_UniformUsageCommand_StockBillsValueMatchesKnownSchedule()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("UniformUsage=0x7ffef5f0", tool.GetUniformUsage("bills"));
        }
    }
}
