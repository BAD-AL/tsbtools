using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Pro Bowl roster. Base 0x4C30 (AFC) / 0x4C76 (NFC), 35 slots x 2 bytes/conference (32 roster
    /// positions + RET1/RET2/RET3). Byte order is REVERSED from SNES: Genesis stores
    /// [teamIndex][positionIndex]; SNES stores [positionIndex][teamIndex] -- don't copy SNES's byte
    /// order verbatim. See Genesis_TSB1_ROM_Findings.md's "Pro Bowl roster" section.
    /// </summary>
    [TestClass]
    public class GenesisProBowlTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int ProBowlStart = 0x4C30;
        private const int NfcOffset = 0x46;

        [TestMethod]
        public void GenesisRom_StockProBowlSelections_MatchReal1992ProBowlPlayers()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            // AFC QB = Dan Marino (Dolphins QB1); AFC RE = Bruce Smith (Bills RE); AFC RET1 = Rod
            // Woodson (Steelers, playing RCB on his own team); NFC QB = Steve Young (49ers QB1).
            Assert.AreEqual("AFC,QB1,dolphins,QB1", tool.GetProBowlPlayer(Conference.AFC, "QB1"));
            Assert.AreEqual("AFC,RE,bills,RE", tool.GetProBowlPlayer(Conference.AFC, "RE"));
            Assert.AreEqual("AFC,RET1,steelers,RCB", tool.GetProBowlPlayer(Conference.AFC, "RET1"));
            Assert.AreEqual("NFC,QB1,49ers,QB1", tool.GetProBowlPlayer(Conference.NFC, "QB1"));
        }

        [TestMethod]
        public void GenesisRom_SetProBowlPlayer_UsesReversedByteOrderFromSnes()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetProBowlPlayer(Conference.AFC, "QB1", "colts", TSBPlayer.QB1);

            int loc = ProBowlStart + 0; // AFC, QB1 slot (positionIndex 0)
            Assert.AreEqual((byte)Genesis_TSB1Tool.GetTeamIndex("colts"), tool.OutputRom[loc], "byte0 = teamIndex (reversed from SNES).");
            Assert.AreEqual((byte)TSBPlayer.QB1, tool.OutputRom[loc + 1], "byte1 = fromTeamPos (reversed from SNES).");
            Assert.AreEqual("AFC,QB1,colts,QB1", tool.GetProBowlPlayer(Conference.AFC, "QB1"));
        }

        [TestMethod]
        public void GenesisRom_SetProBowlPlayer_NfcUsesSeparateBlockFromAfc()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string afcBefore = tool.GetProBowlPlayer(Conference.AFC, "WR1");

            tool.SetProBowlPlayer(Conference.NFC, "WR1", "packers", TSBPlayer.WR1);

            Assert.AreEqual("NFC,WR1,packers,WR1", tool.GetProBowlPlayer(Conference.NFC, "WR1"));
            Assert.AreEqual(afcBefore, tool.GetProBowlPlayer(Conference.AFC, "WR1"), "AFC block must be untouched by an NFC write.");
        }

        [TestMethod]
        public void GenesisRom_SetProBowlPlayer_HandlesReturnSlots()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetProBowlPlayer(Conference.AFC, "RET2", "oilers", TSBPlayer.WR2);

            Assert.AreEqual("AFC,RET2,oilers,WR2", tool.GetProBowlPlayer(Conference.AFC, "RET2"));
        }

        [TestMethod]
        public void GenesisRom_GetProBowlPlayers_ProducesFullTextDump()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            string dump = tool.GetProBowlPlayers();

            StringAssert.Contains(dump, "# AFC ProBowl players");
            StringAssert.Contains(dump, "# NFC ProBowl players");
            StringAssert.Contains(dump, "AFC,QB1,dolphins,QB1");
            StringAssert.Contains(dump, "NFC,QB1,49ers,QB1");
            StringAssert.Contains(dump, "RET3");
        }
    }
}
