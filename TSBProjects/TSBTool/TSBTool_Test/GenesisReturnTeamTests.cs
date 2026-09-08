using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Kick returner / punt returner. Selector byte at 0x4BC0 (1 byte/team, hi nibble = kick
    /// returner's index 0-2 into the team's 3-man return roster, lo nibble = punt returner's index);
    /// return-team roster at 0x4BDC (3 bytes/team -- unlike SNES's 4, no spare byte). Formulas mirror
    /// SNES_TecmoTool.SetReturnTeam/GetReturnTeam/SetKickReturner/SetPuntReturner/GetKickReturner/
    /// GetPuntReturner exactly, just retargeted addresses and a 3- not 4-byte stride. See
    /// Genesis_TSB1_ROM_Findings.md's "Kick/punt returner & return team" section.
    /// </summary>
    [TestClass]
    public class GenesisReturnTeamTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int SelectorStart = 0x4BC0;
        private const int ReturnTeamStart = 0x4BDC;
        private const int ReturnTeamStride = 3;

        [TestMethod]
        public void GenesisRom_StockReturnTeamAndReturners_MatchRealDecodedValues()
        {
            // Decoded directly from the ROM: bills selector=0x01 (KR index 0, PR index 1), return
            // team [RB2, WR4, RCB] -- so KR=RB2, PR=WR4.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("RETURN_TEAM RB2, WR4, RCB", tool.GetReturnTeam("bills"));
            Assert.AreEqual("RB2", tool.GetKickReturner("bills"));
            Assert.AreEqual("WR4", tool.GetPuntReturner("bills"));
        }

        [TestMethod]
        public void GenesisRom_SetReturnTeam_RoundTripsAndPreservesOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string coltsBefore = tool.GetReturnTeam("colts");

            tool.SetReturnTeam("bills", "WR3", "RB1", "LCB");

            Assert.AreEqual("RETURN_TEAM WR3, RB1, LCB", tool.GetReturnTeam("bills"));
            Assert.AreEqual(coltsBefore, tool.GetReturnTeam("colts"), "Colts' return team must be untouched.");
        }

        [TestMethod]
        public void GenesisRom_SetKickReturner_ToExistingReturnTeamMember_JustUpdatesSelectorIndex()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            tool.SetReturnTeam("bills", "WR3", "RB1", "LCB");

            tool.SetKickReturner("bills", "RB1"); // already on the return team, at index 1

            Assert.AreEqual("RB1", tool.GetKickReturner("bills"));
            Assert.AreEqual("RETURN_TEAM WR3, RB1, LCB", tool.GetReturnTeam("bills"), "Roster itself must be unchanged, only the selector nibble moves.");
        }

        [TestMethod]
        public void GenesisRom_SetPuntReturner_ToNewPosition_InsertsThemOnTheReturnTeam()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            tool.SetReturnTeam("bills", "WR3", "RB1", "LCB");

            // FS isn't on the return team yet -- SetPuntReturner should insert it (matching SNES's
            // convention: a punt-returner insert lands at index 1).
            tool.SetPuntReturner("bills", "FS");

            Assert.AreEqual("FS", tool.GetPuntReturner("bills"));
            Assert.AreEqual("RETURN_TEAM WR3, FS, LCB", tool.GetReturnTeam("bills"));
        }

        [TestMethod]
        public void GenesisRom_SetReturnTeam_RejectsInvalidPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string before = tool.GetReturnTeam("bills");

            tool.SetReturnTeam("bills", "XYZ", "RB1", "LCB");

            Assert.AreEqual(before, tool.GetReturnTeam("bills"), "An invalid position must reject the whole write.");
        }
    }
}
