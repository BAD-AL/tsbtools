using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Genesis_TSB1Tool.GetAllPlayers/ResolveRosterFit: a whole-roster-aware pre-flight/apply step that
    /// improves on CheckRosterFit by knowing about roster slots the applied text never mentions at all
    /// (e.g. a real-world roster export that skips DB1/DB2) -- those are free to shrink to a short
    /// placeholder before ever touching a name the text actually specified, since the caller never asked
    /// to keep that data anyway. See CheckRosterFit's own tests for the underlying sequential-simulation
    /// logic this reuses (SimulateSequentialApply).
    /// </summary>
    [TestClass]
    public class GenesisRosterResolveTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        [TestMethod]
        public void GetAllPlayers_ReturnsAllEightHundredNinetySixPlayers_WithCorrectNamesAndJerseys()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            System.Collections.Generic.List<Genesis_TSB1Tool.RosterPlayer> players = tool.GetAllPlayers();

            Assert.AreEqual(896, players.Count);
            Genesis_TSB1Tool.RosterPlayer billsQb1 = players[0];
            Assert.AreEqual("bills", billsQb1.Team);
            Assert.AreEqual("QB1", billsQb1.Position);
            Assert.AreEqual("jim", billsQb1.FirstName);
            Assert.AreEqual("KELLY", billsQb1.LastName);
            Assert.AreEqual((byte)0x12, billsQb1.Jersey);

            Genesis_TSB1Tool.RosterPlayer lastPlayer = players[895];
            Assert.AreEqual("49ers", lastPlayer.Team);
            Assert.AreEqual("DB2", lastPlayer.Position);
            Assert.AreEqual("merton", lastPlayer.FirstName);
            Assert.AreEqual("HANKS", lastPlayer.LastName);
        }

        [TestMethod]
        public void ResolveRosterFit_ReportOnly_CoversNeedFromUnspecifiedSlots_TouchesNoSpecifiedNames()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] romBefore = (byte[])rom.Clone();

            // Bills QB1 stock "jimKELLY" (oldLength=9) -> "alex THOMPSO" (newLength=12, delta=+3). Every
            // other one of the other 895 roster slots is unspecified and thus a free auto-trim
            // candidate -- more than enough combined savings exist to cover a mere 3 bytes.
            Genesis_TSB1Tool.RosterFitReport report = tool.ResolveRosterFit(
                "TEAM = bills SimData=0x0\nQB1, alex THOMPSO, #7\n", false);

            Assert.AreEqual(0, report.Suggestions.Count, "No specified name should need touching -- unspecified slack alone covers this.");
            Assert.AreEqual(0, report.UnresolvedFailures.Count);
            Assert.IsTrue(report.SafeToApplyInOrder);
            Assert.IsTrue(report.AutoTrimmedPlayers.Count >= 1);
            Assert.IsTrue(report.SlackFreedByAutoTrim >= 3);

            CollectionAssert.AreEqual(romBefore, tool.OutputRom, "autoApply=false must never mutate the ROM.");
        }

        [TestMethod]
        public void ResolveRosterFit_AutoApplyTrue_ActuallyWritesTheAutoTrimsToTheRom()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Genesis_TSB1Tool.RosterFitReport report = tool.ResolveRosterFit(
                "TEAM = bills SimData=0x0\nQB1, alex THOMPSO, #7\n", true);

            Assert.IsTrue(report.AutoTrimmedPlayers.Count >= 1);
            Assert.IsTrue(tool.RosterRegionSlack >= 3, "Compress-style trims should have actually run and left real slack behind.");

            foreach (Genesis_TSB1Tool.RosterPlayer trimmed in report.AutoTrimmedPlayers)
            {
                string expected = trimmed.Position.ToLower() + trimmed.Team.ToUpper();
                Assert.AreEqual(expected, tool.GetPlayerName(trimmed.Team, trimmed.Position),
                    "Each auto-trimmed slot should now hold the position+team placeholder.");
            }

            // The specified text itself (bills QB1) is NOT applied by ResolveRosterFit -- that's still a
            // separate ProcessText call the caller makes afterward.
            Assert.AreEqual("jimKELLY", tool.GetPlayerName("bills", "QB1"));
        }

        [TestMethod]
        public void ResolveRosterFit_NeverTrimsASlotTheTextItselfSpecifies()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Genesis_TSB1Tool.RosterFitReport report = tool.ResolveRosterFit(
                "TEAM = bills SimData=0x0\nQB1, alex THOMPSO, #7\n", true);

            foreach (Genesis_TSB1Tool.RosterPlayer trimmed in report.AutoTrimmedPlayers)
                Assert.IsFalse(trimmed.Team == "bills" && trimmed.Position == "QB1",
                    "The one slot the text specified must never be chosen as an auto-trim candidate.");
        }

        [TestMethod]
        public void ResolveRosterFit_UsesMultipleCandidatesWhenOneAloneIsNotEnough()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            // A much larger single growth than any one stock name's savings potential is likely to
            // provide alone (real 1993 names top out well under 20 letters) -- forces the greedy
            // selection to draw from more than one candidate.
            Genesis_TSB1Tool.RosterFitReport report = tool.ResolveRosterFit(
                "TEAM = bills SimData=0x0\nQB1, alexanderchristopher THOMPSONVILLE, #7\n", false);

            Assert.IsTrue(report.AutoTrimmedPlayers.Count > 1, "A large-enough need should draw from more than one auto-trim candidate.");
            Assert.IsTrue(report.SlackFreedByAutoTrim >= report.TotalShiftNeeded || report.Suggestions.Count > 0 || report.UnresolvedFailures.Count > 0);
        }
    }
}
