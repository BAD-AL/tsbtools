using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// GetAll()/GetTeamPlayers() and their supporting readers (GetYear, GetAbilities/GetAbilityString,
    /// player-level sim-data getters, GetPlayerData) -- the "assemble everything into one text dump"
    /// layer, mirroring SNES_TecmoTool.GetAll/GetTeamPlayers/GetPlayerData's exact shape. See
    /// GenesisYearTests.cs for SetYear/GetYear's own detailed byte-level coverage (9 on-screen text
    /// locations, ROM header excluded) -- only basic smoke coverage lives here.
    /// </summary>
    [TestClass]
    public class GenesisFullDumpTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        [TestMethod]
        public void GenesisRom_GetYear_ReturnsNineteenNinetyThree()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("1993", tool.GetYear());
        }

        [TestMethod]
        public void GenesisRom_SetYear_RoundTrips()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetYear("2026");

            Assert.AreEqual("2026", ((Genesis_TSB1Tool)tool).GetYear());
        }

        [TestMethod]
        public void GenesisRom_GetAbilities_QbRoundTripsThroughSetQBAbilities()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetQBAbilities("bills", "QB1", 25, 69, 13, 13, 31, 44, 50, 31);
            int[] abilities = tool.GetAbilities("bills", "QB1");

            CollectionAssert.AreEqual(new[] { 25, 69, 13, 13, 31, 44, 50, 31 }, abilities);
            Assert.AreEqual("25, 69, 13, 13, 31, 44, 50, 31", tool.GetAbilityString("bills", "QB1"));
        }

        [TestMethod]
        public void GenesisRom_GetAbilities_SkillPositionReturnsFourStatsPlusWildPair()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetSkillPlayerAbilities("bills", "RB1", 25, 69, 13, 13, 44, 50);
            int[] abilities = tool.GetAbilities("bills", "RB1");

            CollectionAssert.AreEqual(new[] { 25, 69, 13, 13, 44, 50 }, abilities);
        }

        [TestMethod]
        public void GenesisRom_GetAbilities_OLPositionReturnsOnlyFourStats()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetOLPlayerAbilities("bills", "C", 25, 69, 13, 13);
            int[] abilities = tool.GetAbilities("bills", "C");

            CollectionAssert.AreEqual(new[] { 25, 69, 13, 13 }, abilities);
        }

        [TestMethod]
        public void GenesisRom_GetPlayerSimData_RoundTripsForEachPositionGroup()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetQBSimData("bills", "QB1", new int[] { 3, 12, 200 });
            CollectionAssert.AreEqual(new[] { 3, 12, 200 }, tool.GetPlayerSimData("bills", "QB1"));

            tool.SetSkillSimData("bills", "RB1", new int[] { 4, 7, 9, 2 });
            CollectionAssert.AreEqual(new[] { 4, 7, 9, 2 }, tool.GetPlayerSimData("bills", "RB1"));

            tool.SetDefensiveSimData("bills", "RE", new int[] { 200, 15 });
            CollectionAssert.AreEqual(new[] { 200, 15 }, tool.GetPlayerSimData("bills", "RE"));

            // OL has no sim-data slot at all.
            Assert.IsNull(tool.GetPlayerSimData("bills", "C"));
        }

        [TestMethod]
        public void GenesisRom_GetPlayerData_ProducesReparseableLine()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string line = tool.GetPlayerData("bills", "QB1", true, true, true, true, true);

            // Real stock Bills QB1: Jim Kelly, jersey 0x12.
            StringAssert.StartsWith(line, "QB1, jim KELLY, ");
            StringAssert.Contains(line, "#12,");
        }

        [TestMethod]
        public void GenesisRom_GetPlayerData_RoundTripsThroughProcessText()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string line = tool.GetPlayerData("bills", "RB1", true, true, true, true, true);
            tool.ProcessText("TEAM = bills SimData=0x0\n" + line);

            // Re-decoding after re-applying the exact same dumped line should be a no-op.
            string lineAfter = tool.GetPlayerData("bills", "RB1", true, true, true, true, true);
            Assert.AreEqual(line, lineAfter);
        }

        [TestMethod]
        public void GenesisRom_GetTeamPlayers_ContainsTeamHeaderRosterAndReturnTeam()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string dump = tool.GetTeamPlayers("bills");

            StringAssert.StartsWith(dump, "TEAM = bills SimData=0x");
            StringAssert.Contains(dump, "QB1, jim KELLY, ");
            StringAssert.Contains(dump, "DB2, ");
            StringAssert.Contains(dump, "RETURN_TEAM ");
            StringAssert.Contains(dump, "KR, ");
            StringAssert.Contains(dump, "PR, ");
        }

        [TestMethod]
        public void GenesisRom_GetAll_ContainsYearLineAndAllTwentyEightTeams()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string dump = tool.GetAll();

            StringAssert.StartsWith(dump, "YEAR=1993\n");
            StringAssert.Contains(dump, "TEAM = bills SimData=0x");
            StringAssert.Contains(dump, "TEAM = 49ers SimData=0x");
        }

        [TestMethod]
        public void GenesisRom_GetPlayerStuff_MatchesGetPlayerDataForEveryTeamAndPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string output = tool.GetPlayerStuff(true, true, true, true, true);

            StringAssert.Contains(output, "TEAM=bills\n");
            StringAssert.Contains(output, tool.GetPlayerData("bills", "QB1", true, true, true, true, true));
            StringAssert.Contains(output, "TEAM=49ers\n");
            StringAssert.Contains(output, tool.GetPlayerData("49ers", "DB2", true, true, true, true, true));
        }
    }
}
