using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Genesis_TSB1Tool.CheckRosterFit: a read-only pre-flight check for a whole roster text block
    /// (same format as ProcessText), so a caller can find out before applying whether it would overflow
    /// the shared roster/name table into the attribute table (see InsertPlayer's own bounds check and
    /// its comment for why that's possible -- the stock ROM has zero slack in this table). Reuses
    /// InputParser.GetFirstName/GetLastName so parsing can't drift from what a real apply actually does.
    ///
    /// Fits/OverflowBytes/TotalShiftNeeded are a simple total-vs-slack summary and are NOT sufficient on
    /// their own to know whether a real top-to-bottom apply succeeds -- a shrink late in the file can't
    /// retroactively rescue a growth that already got rejected earlier, since InsertPlayer only sees
    /// slack that's actually been created so far at the point each line is processed. SafeToApplyInOrder
    /// (backed by a real line-by-line simulation, tracking running slack exactly like InsertPlayer does)
    /// is the field that answers "will this actually work."
    /// </summary>
    [TestClass]
    public class GenesisRosterFitTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        [TestMethod]
        public void CheckRosterFit_FitsWithNoSuggestionsNeeded_ForNamesNoLongerThanStock()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);

            // Bills QB1 stock is "jimKELLY" (9 bytes incl. jersey); this replacement is shorter.
            Genesis_TSB1Tool.RosterFitReport report = Genesis_TSB1Tool.CheckRosterFit(rom,
                "TEAM = bills SimData=0x0\nQB1, al SMITH, #7\n");

            Assert.IsTrue(report.Fits);
            Assert.AreEqual(0, report.OverflowBytes);
            Assert.IsTrue(report.SafeToApplyInOrder);
            Assert.AreEqual(0, report.Suggestions.Count);
            Assert.AreEqual(0, report.UnresolvedFailures.Count);
        }

        [TestMethod]
        public void CheckRosterFit_SuggestsAbbreviation_WhenTheEntrysOwnFirstNameSavesEnough()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);

            // Bills QB1 stock "jimKELLY" (oldLength=9). "thunderbolt Z" (newLength=13, delta=+4) needs 4
            // bytes with zero slack available -- but "thunderbolt" (11 letters) abbreviates to "t." for
            // a 9-byte saving, more than enough to cover the shortfall on its own.
            Genesis_TSB1Tool.RosterFitReport report = Genesis_TSB1Tool.CheckRosterFit(rom,
                "TEAM = bills SimData=0x0\nQB1, thunderbolt Z, #7\n");

            Assert.IsFalse(report.Fits, "Total-only check should flag this as over budget.");
            Assert.AreEqual(4, report.OverflowBytes);

            Assert.IsTrue(report.SafeToApplyInOrder, "The suggestion should fully resolve the only entry.");
            Assert.AreEqual(0, report.UnresolvedFailures.Count);
            Assert.AreEqual(1, report.Suggestions.Count);
            Genesis_TSB1Tool.RosterFitSuggestion s = report.Suggestions[0];
            Assert.AreEqual("bills", s.Team);
            Assert.AreEqual("QB1", s.Position);
            Assert.AreEqual("thunderbolt Z", s.ProposedName);
            Assert.AreEqual("t. Z", s.SuggestedName);
            Assert.AreEqual(9, s.BytesSaved);
        }

        [TestMethod]
        public void CheckRosterFit_ReportsUnresolvedFailure_WhenAbbreviationAloneIsNotEnough()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);

            // Bills QB1 stock "jimKELLY" (oldLength=9). "alexander THUNDERBOLT" (newLength=21, delta=+12)
            // needs 12 bytes with zero slack available; "alexander" (9 letters) only saves 7 even fully
            // abbreviated ("a."), leaving a 5-byte shortfall -- not resolvable by trimming this entry's
            // first name alone.
            Genesis_TSB1Tool.RosterFitReport report = Genesis_TSB1Tool.CheckRosterFit(rom,
                "TEAM = bills SimData=0x0\nQB1, alexander THUNDERBOLT, #7\n");

            Assert.IsFalse(report.Fits);
            Assert.AreEqual(12, report.OverflowBytes);
            Assert.IsFalse(report.SafeToApplyInOrder);
            Assert.AreEqual(0, report.Suggestions.Count);

            Assert.AreEqual(1, report.UnresolvedFailures.Count);
            Genesis_TSB1Tool.RosterFitSuggestion f = report.UnresolvedFailures[0];
            Assert.AreEqual("bills", f.Team);
            Assert.AreEqual("QB1", f.Position);
            Assert.AreEqual("alexander THUNDERBOLT", f.ProposedName);
            Assert.IsNull(f.SuggestedName);
            Assert.AreEqual(7, f.BytesSaved, "Should report the best it could do (max possible savings), even though it wasn't enough.");
        }

        [TestMethod]
        public void CheckRosterFit_OrderMatters_ALaterShrinkCannotRescueAnEarlierGrowth()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);

            // Bills (team index 0, first in file order) QB1 "jimKELLY" (oldLength=9) growing to
            // "alexander THUNDR" (newLength=16, delta=+7) needs 7 bytes -- but at this point in the
            // file, zero slack has been created yet.
            // 49ers (team index 27, last in file order) QB1 "steveYOUNG" (oldLength=11) shrinking to
            // "a B" (newLength=3, delta=-8) frees 8 bytes -- MORE than bills QB1 needed -- but it comes
            // AFTER bills QB1 in the file, so it can't help bills QB1's write succeed.
            //
            // Globally the total nets to -1 (7 + -8), which looks completely fine (Fits=true,
            // OverflowBytes=0) -- exactly the misleading signal a total-only check would give. The real
            // sequential simulation must still catch that bills QB1 needed trimming.
            Genesis_TSB1Tool.RosterFitReport report = Genesis_TSB1Tool.CheckRosterFit(rom,
                "TEAM = bills SimData=0x0\nQB1, alexander THUNDR, #7\n" +
                "TEAM = 49ers SimData=0x0\nQB1, a B, #1\n");

            Assert.IsTrue(report.Fits, "The naive total-vs-slack check nets out fine here -- that's the trap.");
            Assert.AreEqual(0, report.OverflowBytes);

            Assert.AreEqual(1, report.Suggestions.Count, "Bills QB1 still needed abbreviating despite the total looking fine.");
            Assert.AreEqual("bills", report.Suggestions[0].Team);
            Assert.AreEqual("QB1", report.Suggestions[0].Position);
            Assert.AreEqual(0, report.UnresolvedFailures.Count);
            Assert.IsTrue(report.SafeToApplyInOrder, "Safe once bills QB1's suggested abbreviation is applied.");
        }

        [TestMethod]
        public void CheckRosterFit_NeverMutatesTheGivenRom()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            byte[] romBefore = (byte[])rom.Clone();

            Genesis_TSB1Tool.CheckRosterFit(rom, "TEAM = bills SimData=0x0\nQB1, alexander THUNDERBOLT, #7\n");

            CollectionAssert.AreEqual(romBefore, rom, "CheckRosterFit must be read-only.");
        }
    }
}
