using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 1 of compressed-riding-rossum.md: team abbreviation/city/nickname round trip against the
    /// real Genesis TSB1 ROM. Genesis's team string table stores these as three separate, independently
    /// addressable pointer-table index ranges (0-27 abbreviation, 32-59 city, 64-91 nickname) rather
    /// than SNES's single combined "ABB*City Name*" string per team, so -- unlike
    /// TeamStringsCommandTests.cs's shared helper -- each field is verified independently here, and the
    /// real risk surface (a length-changing write shifting every later string's pointer) is exercised
    /// the same way: assert the edited team's fields round-trip, and that an earlier and a later team's
    /// fields (by string-table position, not team order) are untouched.
    /// </summary>
    [TestClass]
    public class GenesisTeamStringsTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        private static Genesis_TSB1Tool LoadTool()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            return (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
        }

        [TestMethod]
        public void GenesisRom_StockAbbreviationsMatchRealTeams()
        {
            Genesis_TSB1Tool tool = LoadTool();

            // Spot-check against TSBTool_Test\Genesis_TSB1_ROM_Findings.md's confirmed abbreviation table --
            // includes an underscore-padded one (patriots) to confirm the padding convention.
            Assert.AreEqual("BUF_", tool.GetTeamAbbreviation(0));
            Assert.AreEqual("N_E_", tool.GetTeamAbbreviation(3));
            Assert.AreEqual("S_F_", tool.GetTeamAbbreviation(27));
        }

        [TestMethod]
        public void GenesisRom_StockCityAndNicknameMatchRealTeams()
        {
            Genesis_TSB1Tool tool = LoadTool();

            Assert.AreEqual("BUFFALO", tool.GetTeamCity(0));
            Assert.AreEqual("BILLS", tool.GetTeamName(0));
            Assert.AreEqual("NEW ENGLAND", tool.GetTeamCity(3));
            Assert.AreEqual("SAN FRANCISCO", tool.GetTeamCity(27));
            Assert.AreEqual("49ERS", tool.GetTeamName(27));
        }

        [TestMethod]
        public void GenesisRom_TeamStringsCommand_RoundTripsAndPreservesNeighbors()
        {
            Genesis_TSB1Tool tool = LoadTool();

            int firstIndex = 0;
            int lastIndex = 27;
            int middleIndex = 14; // cowboys
            string middleTeam = Genesis_TSB1Tool.GetTeamFromIndex(middleIndex);

            string firstAbbBefore = tool.GetTeamAbbreviation(firstIndex);
            string firstCityBefore = tool.GetTeamCity(firstIndex);
            string firstNameBefore = tool.GetTeamName(firstIndex);
            string lastAbbBefore = tool.GetTeamAbbreviation(lastIndex);
            string lastCityBefore = tool.GetTeamCity(lastIndex);
            string lastNameBefore = tool.GetTeamName(lastIndex);

            // The stock table has no spare bytes, so a longer string needs room made first: the city
            // gets shorter (DALLAS -> Test, -2), then the nickname longer (COWBOYS -> TESTERSXX, +2).
            string command = string.Format(
                "TEAM = {0} SimData=0x0\nTEAM_ABB=TEST,TEAM_CITY=Test,TEAM_NAME=TESTERSXX",
                middleTeam);
            tool.ProcessText(command);

            Assert.AreEqual("TEST", tool.GetTeamAbbreviation(middleIndex));
            Assert.AreEqual("Test", tool.GetTeamCity(middleIndex));
            Assert.AreEqual("TESTERSXX", tool.GetTeamName(middleIndex));

            Assert.AreEqual(firstAbbBefore, tool.GetTeamAbbreviation(firstIndex), "Earlier team's abbreviation should be untouched.");
            Assert.AreEqual(firstCityBefore, tool.GetTeamCity(firstIndex), "Earlier team's city should be untouched.");
            Assert.AreEqual(firstNameBefore, tool.GetTeamName(firstIndex), "Earlier team's name should be untouched.");
            Assert.AreEqual(lastAbbBefore, tool.GetTeamAbbreviation(lastIndex), "Later team's abbreviation should be untouched.");
            Assert.AreEqual(lastCityBefore, tool.GetTeamCity(lastIndex), "Later team's city should be untouched.");
            Assert.AreEqual(lastNameBefore, tool.GetTeamName(lastIndex), "Later team's name should be untouched.");
        }

        // The team string table (abbreviations, cities, nicknames, ...) ends at 0x4ABF with no spare
        // bytes in the stock ROM. A longer string must be rejected unless an earlier edit made room, and
        // the shift that makes room must never write past the table's end.
        private const int TeamStringTableDataEnd = 0x4ABF;
        private const int CheckedBytesAfterTable = 0x100;

        [TestMethod]
        public void GenesisRom_TeamCity_LongerWithNoRoom_IsRejected_RomUnchanged()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            byte[] romBefore = (byte[])rom.Clone();
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.SetTeamCity(0, "BUFFALO CITY");
                StaticUtils.ShowErrors();

                CollectionAssert.AreEqual(romBefore, tool.OutputRom, "A string that doesn't fit must be rejected entirely.");
                Assert.AreEqual(1, giver.Errors.Count);
                StringAssert.Contains(giver.Errors[0], "BUFFALO CITY");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
            }
        }

        [TestMethod]
        public void GenesisRom_TeamCity_LongerAfterAShorterOne_FitsAndLeavesBytesAfterTheTableUntouched()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            byte[] romBefore = (byte[])rom.Clone();
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetTeamCity(3, "N E");           // NEW ENGLAND -> N E: 8 bytes of room
            tool.SetTeamCity(0, "BUFFALO CITY");  // +5

            Assert.AreEqual("N E", tool.GetTeamCity(3));
            Assert.AreEqual("BUFFALO CITY", tool.GetTeamCity(0));
            Assert.AreEqual("BILLS", tool.GetTeamName(0));
            Assert.AreEqual("SAN FRANCISCO", tool.GetTeamCity(27));
            Assert.AreEqual("49ERS", tool.GetTeamName(27));
            for (int i = TeamStringTableDataEnd; i < TeamStringTableDataEnd + CheckedBytesAfterTable; i++)
                Assert.AreEqual(romBefore[i], tool.OutputRom[i], string.Format("Byte 0x{0:X} after the table changed.", i));
        }

        [TestMethod]
        public void GenesisRom_TeamAbbreviation_WrongLength_IsRejected()
        {
            Genesis_TSB1Tool tool = LoadTool();

            string beforeAbb = tool.GetTeamAbbreviation(0);
            tool.ProcessText("TEAM = bills SimData=0x0\nTEAM_ABB=TOOLONG,TEAM_CITY=Test City,TEAM_NAME=TESTERS");

            Assert.AreEqual(beforeAbb, tool.GetTeamAbbreviation(0), "A non-4-character abbreviation must be rejected, leaving the original unchanged.");
        }
    }
}
