using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    // Generic (NES + SNES) name-table capacity checker -- see Core\PlayerNames.cs. Verified against
    // the same real fixtures used throughout this session's investigation (TSPRBOWL.nes measured with
    // 483 bytes of real stock slack, TSB1.smc with 1552).
    [TestClass]
    public class PlayerNamesTests
    {
        [TestMethod]
        public void ResolveConfigFor_ReturnsNesConfig_ForNesAndCxRomVariants()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Assert.AreSame(PlayerNamesConfig.Nes, PlayerNames.ResolveConfigFor(tool));
        }

        [TestMethod]
        public void ResolveConfigFor_ReturnsSnesConfig_ForSnesTsb1()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Assert.AreSame(PlayerNamesConfig.Snes, PlayerNames.ResolveConfigFor(tool));
        }

        [TestMethod]
        public void ResolveConfigFor_ReturnsNull_ForGenesisTsb1()
        {
            byte[] rom = TestRoms.LoadRom(@"Genesis\Tecmo Super Bowl (USA) (October 1993).md");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Assert.IsNull(PlayerNames.ResolveConfigFor(tool));
        }

        [TestMethod]
        public void ExtractCurrentRoster_Nes_FindsKnownRealPlayers()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<PlayerNameEntry> roster = PlayerNames.ExtractCurrentRoster(tool, PlayerNamesConfig.Nes);

            Assert.AreEqual(28 * 30, roster.Count); // NES has no DB1/DB2 (30 positions, not 32)
            PlayerNameEntry falconsPunter = roster.Find(e => e.Team == "falcons" && e.Position == "P");
            Assert.IsNotNull(falconsPunter);
            Assert.AreEqual("scott", falconsPunter.FirstName);
            Assert.AreEqual("FULHAGE", falconsPunter.LastName);
        }

        [TestMethod]
        public void ExtractCurrentRoster_Snes_FindsKnownRealPlayers()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<PlayerNameEntry> roster = PlayerNames.ExtractCurrentRoster(tool, PlayerNamesConfig.Snes);

            Assert.AreEqual(28 * 32, roster.Count);
            PlayerNameEntry billsQb = roster.Find(e => e.Team == "bills" && e.Position == "QB1");
            Assert.IsNotNull(billsQb);
            Assert.AreEqual("jim", billsQb.FirstName);
            Assert.AreEqual("KELLY", billsQb.LastName);

            PlayerNameEntry ninersDb2 = roster.Find(e => e.Team == "49ers" && e.Position == "DB2");
            Assert.IsNotNull(ninersDb2);
            Assert.AreEqual("merton", ninersDb2.FirstName);
            Assert.AreEqual("HANKS", ninersDb2.LastName);
        }

        [TestMethod]
        public void ResolveFit_Snes_FitsWithoutAnyTrimming_WhenWellWithinKnownSlack()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            string rosterText = "TEAM = bills\nQB1, joe SMITH, #12,\n";

            PlayerNamesFitReport report = PlayerNames.ResolveFit(tool, PlayerNamesConfig.Snes, rosterText, false);

            Assert.IsTrue(report.Fits);
            Assert.AreEqual(0, report.AutoTrimmedPlayers.Count);
            Assert.AreEqual(0, report.Suggestions.Count);
            Assert.AreEqual(0, report.UnresolvedFailures.Count);
            Assert.IsTrue(report.SafeToApplyInOrder);
        }

        [TestMethod]
        public void ResolveFit_Nes_AutoTrimsUnspecifiedPlayers_WhenNeededSlackExceedsKnownStockSlack()
        {
            // NES_TecmoTool.InsertPlayer caps any single name at 16 characters (auto-abbreviating
            // anything longer -- see PlayerNamesConfig.MaxNameLength), so no *single* specified name can
            // ever exceed stock NES's real ~483 bytes of slack on its own, and even one whole team's
            // worth (~30 positions, ~7 bytes growth each) falls short too -- real pressure needs several
            // teams' worth of positions all at that 16-char cap at once.
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                sb.Append("TEAM = ").Append(PlayerNamesConfig.Nes.Teams[i]).Append('\n');
                foreach (string pos in PlayerNamesConfig.Nes.Positions)
                    sb.Append(pos).Append(", aaaaaaaaaa BBBBBBBBBBBBBB, #12,\n");
            }

            PlayerNamesFitReport report = PlayerNames.ResolveFit(tool, PlayerNamesConfig.Nes, sb.ToString(), true);

            Assert.IsTrue(report.AutoTrimmedPlayers.Count > 0, "Expected some unspecified players to be auto-trimmed to free up room.");
            foreach (PlayerNameEntry trimmed in report.AutoTrimmedPlayers)
            {
                // Checked without a space between fname/lname: the stored bytes are always exactly
                // "position.ToLower()" immediately followed by "team.ToUpper()", but the *displayed*
                // split point (GetName's own lowercase->uppercase transition heuristic) can land in an
                // unexpected place for a team name that starts with a digit ("49ers" -> "49ERS" reads
                // as "...4" still being part of the lowercase run), so asserting the space's exact
                // position isn't reliable across every team.
                string dump = tool.GetTeamPlayers(trimmed.Team).Replace(" ", "");
                StringAssert.Contains(dump, trimmed.Position.ToLower() + trimmed.Team.ToUpper());
            }
        }

        [TestMethod]
        public void ResolveFit_NeverAutoTrimsASlotTheOverlayTextItselfSpecifies()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            // Every one of bills' own positions asks for a name at SNES's 17-char cap -- bills itself
            // must never appear in AutoTrimmedPlayers, even though it's the entire source of the
            // shortfall, since ResolveFit only ever auto-trims slots the overlay text didn't mention.
            StringBuilder sb = new StringBuilder();
            sb.Append("TEAM = bills\n");
            foreach (string pos in PlayerNamesConfig.Snes.Positions)
                sb.Append(pos).Append(", aaaaaaaaaa BBBBBBBBBBBBBB, #12,\n");

            PlayerNamesFitReport report = PlayerNames.ResolveFit(tool, PlayerNamesConfig.Snes, sb.ToString(), false);

            Assert.IsFalse(report.AutoTrimmedPlayers.Exists(e => e.Team == "bills"));
        }

        [TestMethod]
        public void ResolveFit_ReportOnly_NeverMutatesTheRom()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            byte[] originalCopy = (byte[])rom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            string hugeName = new string('x', 600);
            string rosterText = "TEAM = bills\nQB1, " + hugeName + " HUGE, #12,\n";

            PlayerNames.ResolveFit(tool, PlayerNamesConfig.Nes, rosterText, false);

            CollectionAssert.AreEqual(originalCopy, tool.OutputRom, "autoApply:false must never write to the ROM.");
        }

        [TestMethod]
        public void VerifyRomSafety_ReturnsTrue_ForAFitThatStaysWellWithinCapacity()
        {
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            bool safe = PlayerNames.VerifyRomSafety(rom, PlayerNamesConfig.Snes,
                t => t.ProcessText("TEAM = bills\nQB1, joe SMITH, #12,\n"),
                16);
            Assert.IsTrue(safe);
        }

        [TestMethod]
        public void VerifyRomSafety_ReturnsFalse_WhenAnUnprotectedOverflowCorruptsThePastBoundaryBytes()
        {
            // SNES_TecmoTool.InsertPlayer caps any single name at 17 characters (auto-abbreviating
            // anything longer), so one oversized name can never overflow on its own -- real-world risk
            // only shows up from many individual (legally-capped) writes whose *cumulative* growth
            // exceeds the ~1552 bytes of real stock slack, since InsertPlayer's own shift has no bounds
            // check at all once the write is legal-sized. Deliberately bypass ResolveFit and directly
            // call the tool's own public InsertPlayer for every position on every team (except the
            // last team, "49ers" -- its last entry, DB2, is a special-cased sentinel-less slot whose
            // own GetName depends on a well-formed pointer table, so once earlier teams' overflow has
            // already corrupted things, touching it throws rather than cleanly demonstrating the
            // silent-corruption case this test is after) with a name at the 17-char cap, to prove
            // VerifyRomSafety's anchor-byte check actually catches the real corruption this causes.
            byte[] rom = TestRoms.LoadRom("TSB1.smc");
            bool safe = PlayerNames.VerifyRomSafety(rom, PlayerNamesConfig.Snes,
                t =>
                {
                    string[] teams = PlayerNamesConfig.Snes.Teams;
                    for (int i = 0; i < teams.Length - 1; i++)
                        foreach (string pos in PlayerNamesConfig.Snes.Positions)
                            t.InsertPlayer(teams[i], pos, "aaaaaaaaaa", "BBBBBBB", 12);
                },
                16);
            Assert.IsFalse(safe, "Pushing every player to the 17-char cap should overflow ~1552 bytes of real slack and corrupt bytes at the capacity boundary.");
        }
    }
}
