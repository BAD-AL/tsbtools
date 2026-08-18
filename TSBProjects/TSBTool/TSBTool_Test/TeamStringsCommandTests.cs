using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Verifies the InputParser "TEAM_ABB=XXXX,TEAM_CITY=...,TEAM_NAME=..." command
    /// (added 2019-08-13, never covered by any prior test). Applies to whichever team the most
    /// recent "TEAM = teamname" line named (InputParser tracks this as `currentTeam`), so every
    /// input snippet here declares that first.
    ///
    /// This command writes into a variable-width, pointer-indexed string table
    /// (TecmoTool.SetTeamStringTableString): changing a string to a different length shifts every
    /// later string in the table and rewrites their pointers (ShiftDataUp/Down + AdjustDataPointers).
    /// That's meaningfully different from SET/ReplaceString's fixed-position writes, so the
    /// important thing to verify isn't "only these exact bytes changed" but "the edited team's
    /// strings read back correctly AND every other team's strings still read back correctly" --
    /// i.e. the shift/pointer-adjustment logic doesn't corrupt neighboring teams.
    ///
    /// Three genuinely different implementations are covered (each overrides
    /// Get/SetTeamAbbreviation/City/Name independently): stock TecmoTool (28 teams, 3 separate
    /// fixed-index string-table entries per team), CXRomTSBTool (34-team table, different offsets:
    /// +34 city/+68 name instead of +32/+64), and SNES_TecmoTool (a single combined
    /// "ABB*City Name*" string-table entry per team instead of 3 separate ones). TSB2/TSB3 share yet
    /// another format ("ABB*City Name*", like SNES_TecmoTool) but are left for the dedicated TSB
    /// II/III round-trip task in TestDocument.md rather than duplicated here.
    /// </summary>
    [TestClass]
    public class TeamStringsCommandTests
    {
        private static void VerifyTeamStringsRoundTripAndNeighborsUntouched(string romFileName)
        {
            byte[] rom = TestRoms.LoadRom(romFileName);
            ITecmoContent content = TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNotNull(content, "Failed to detect ROM type for " + romFileName);
            ITecmoTool tool = (ITecmoTool)content;

            int teamCount = TecmoTool.Teams.Count;
            int firstIndex = 0;
            int lastIndex = teamCount - 1;
            int middleIndex = teamCount / 2;
            string middleTeam = TecmoTool.GetTeamFromIndex(middleIndex);

            string firstAbbBefore = tool.GetTeamAbbreviation(firstIndex);
            string firstCityBefore = tool.GetTeamCity(firstIndex);
            string firstNameBefore = tool.GetTeamName(firstIndex);
            string lastAbbBefore = tool.GetTeamAbbreviation(lastIndex);
            string lastCityBefore = tool.GetTeamCity(lastIndex);
            string lastNameBefore = tool.GetTeamName(lastIndex);

            string command = string.Format(
                "TEAM = {0} SimData=0x0\nTEAM_ABB=TEST,TEAM_CITY=Test City,TEAM_NAME=TESTERS",
                middleTeam);
            tool.ProcessText(command);

            Assert.AreEqual("TEST", tool.GetTeamAbbreviation(middleIndex));
            Assert.AreEqual("Test City", tool.GetTeamCity(middleIndex));
            Assert.AreEqual("TESTERS", tool.GetTeamName(middleIndex));

            Assert.AreEqual(firstAbbBefore, tool.GetTeamAbbreviation(firstIndex), "Earlier team's abbreviation should be untouched.");
            Assert.AreEqual(firstCityBefore, tool.GetTeamCity(firstIndex), "Earlier team's city should be untouched.");
            Assert.AreEqual(firstNameBefore, tool.GetTeamName(firstIndex), "Earlier team's name should be untouched.");
            Assert.AreEqual(lastAbbBefore, tool.GetTeamAbbreviation(lastIndex), "Later team's abbreviation should be untouched.");
            Assert.AreEqual(lastCityBefore, tool.GetTeamCity(lastIndex), "Later team's city should be untouched.");
            Assert.AreEqual(lastNameBefore, tool.GetTeamName(lastIndex), "Later team's name should be untouched.");
        }

        [TestMethod]
        public void StockNesRom_TeamStringsCommand_RoundTripsAndPreservesNeighbors()
        {
            VerifyTeamStringsRoundTripAndNeighborsUntouched("TSPRBOWL.nes");
        }

        [TestMethod]
        public void CXRomV105_TeamStringsCommand_RoundTripsAndPreservesNeighbors()
        {
            VerifyTeamStringsRoundTripAndNeighborsUntouched("TSB 2007-32-105.nes");
        }

        [TestMethod]
        public void SnesTsb1Rom_TeamStringsCommand_RoundTripsAndPreservesNeighbors()
        {
            VerifyTeamStringsRoundTripAndNeighborsUntouched("TSB1.smc");
        }

        [TestMethod]
        public void CXRomV105_TeamAbbreviation_WrongLength_IsRejected()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            string beforeAbb = tool.GetTeamAbbreviation(0);
            tool.ProcessText("TEAM = bills SimData=0x0\nTEAM_ABB=TOOLONG,TEAM_CITY=Test City,TEAM_NAME=TESTERS");

            Assert.AreEqual(beforeAbb, tool.GetTeamAbbreviation(0), "A non-4-character abbreviation must be rejected, leaving the original unchanged.");
        }
    }
}
