using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Player roster/names. Slot->position mapping for all 32 roster slots is now confirmed (see
    /// Genesis_TSB1_ROM_Findings.md's "Roster / player names" section): Genesis uses exactly SNES
    /// TSB1's positionNames order. Real-name checks use the same ground truth that confirmed it --
    /// real 1993 players decoded from trusted teams (teams whose data is confirmed carried over
    /// unchanged from SNES: bills, bengals, oilers, broncos, packers, vikings, rams).
    /// </summary>
    [TestClass]
    public class GenesisRosterTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int RosterPointerStart = 0x5862;

        [TestMethod]
        public void GenesisRom_StockQb1Names_MatchReal1993StartingQuarterbacks()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual("jimKELLY", tool.GetPlayerName("bills", "QB1"));
            Assert.AreEqual("jeffGEORGE", tool.GetPlayerName("colts", "QB1"));
            Assert.AreEqual("danMARINO", tool.GetPlayerName("dolphins", "QB1"));
        }

        [TestMethod]
        public void GenesisRom_StockRosterNames_MatchReal1993PlayersAcrossAllPositionGroups()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            // Real 1993 rosters, decoded directly from the ROM and cross-checked against known real
            // players for 3 trusted teams (bills, bengals, broncos) -- one full position group each
            // (skill, OL, defense, kicking) to confirm the position->slot mapping holds everywhere,
            // not just at QB1.
            Assert.AreEqual("thurmanTHOMAS", tool.GetPlayerName("bills", "RB1"));
            Assert.AreEqual("donBEEBE", tool.GetPlayerName("bills", "WR1"));
            Assert.AreEqual("keithMCKELLER", tool.GetPlayerName("bills", "TE1"));
            Assert.AreEqual("kentHULL", tool.GetPlayerName("bills", "C"));
            Assert.AreEqual("jerryCRAFTS", tool.GetPlayerName("bills", "LT"));
            Assert.AreEqual("bruceSMITH", tool.GetPlayerName("bills", "RE"));
            Assert.AreEqual("markMADDOX", tool.GetPlayerName("bills", "ROLB"));
            Assert.AreEqual("nateODOMES", tool.GetPlayerName("bills", "RCB"));
            Assert.AreEqual("steveCHRISTIE", tool.GetPlayerName("bills", "K"));
            Assert.AreEqual("chrisMOHR", tool.GetPlayerName("bills", "P"));
            Assert.AreEqual("jamesWILLIAMS", tool.GetPlayerName("bills", "DB1"));
            Assert.AreEqual("mattDARBY", tool.GetPlayerName("bills", "DB2"));

            Assert.AreEqual("haroldGREEN", tool.GetPlayerName("bengals", "RB1"));
            Assert.AreEqual("carlPICKENS", tool.GetPlayerName("bengals", "WR1"));
            Assert.AreEqual("danielSTUBBS", tool.GetPlayerName("bengals", "RE"));
            Assert.AreEqual("jimBREECH", tool.GetPlayerName("bengals", "K"));

            Assert.AreEqual("johnELWAY", tool.GetPlayerName("broncos", "QB1"));
            Assert.AreEqual("shannonSHARPE", tool.GetPlayerName("broncos", "TE1"));
            Assert.AreEqual("mikeCROEL", tool.GetPlayerName("broncos", "ROLB"));
            Assert.AreEqual("mikeHORAN", tool.GetPlayerName("broncos", "P"));
        }

        [TestMethod]
        public void GenesisRom_InsertPlayer_RejectsGenuinelyInvalidPosition()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;

            string before = genesisTool.GetPlayerName("bills", "QB1");
            tool.InsertPlayer("bills", "XYZ", "test", "PLAYER", 0x99);

            Assert.AreEqual(before, genesisTool.GetPlayerName("bills", "QB1"), "A nonexistent position must be rejected without touching QB1's data.");
        }

        [TestMethod]
        public void GenesisRom_InsertPlayerCommand_RoundTripsAndPreservesNeighbors()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;

            // Bills QB1 is player index 0 (first record); Colts QB1 (index 32) is the next-mapped
            // neighbor in the roster pointer table -- both a shorter and this longer replacement name
            // exercise the pointer-shift path either direction. The stock roster table has zero slack
            // (see GenesisRom_InsertPlayer_RejectsWriteThatWouldOverflowIntoAttributeTable_* below), so
            // a genuinely longer name needs room freed up first -- shrink two unrelated, far-away
            // entries (49ers QB1/QB2) to make space before growing this one.
            string coltsBefore = genesisTool.GetPlayerName("colts", "QB1");
            genesisTool.InsertPlayer("49ers", "QB1", "x", "X", 0x00);
            genesisTool.InsertPlayer("49ers", "QB2", "x", "X", 0x00);

            tool.ProcessText("TEAM = bills SimData=0x0\nQB1, alexander THUNDERBOLT, #7");

            Assert.AreEqual("alexanderTHUNDERBOLT", genesisTool.GetPlayerName("bills", "QB1"));
            Assert.AreEqual((byte)0x07, tool.OutputRom[GetQb1DataLoc(tool, 0)], "Jersey byte should be the hex-parsed '#7' value.");
            Assert.AreEqual(coltsBefore, genesisTool.GetPlayerName("colts", "QB1"), "Colts QB1 (the next player record) must be untouched.");
        }

        [TestMethod]
        public void GenesisRom_InsertPlayerCommand_ShorterName_ShiftsPointersCorrectly()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Genesis_TSB1Tool genesisTool = (Genesis_TSB1Tool)tool;

            string coltsBefore = genesisTool.GetPlayerName("colts", "QB1");

            tool.ProcessText("TEAM = bills SimData=0x0\nQB1, al SMITH, #7");

            Assert.AreEqual("alSMITH", genesisTool.GetPlayerName("bills", "QB1"));
            Assert.AreEqual(coltsBefore, genesisTool.GetPlayerName("colts", "QB1"), "Colts QB1 must be untouched after a shorter-name write shifts data up.");
        }

        [TestMethod]
        public void GenesisRom_InsertPlayer_RoundTripsForNonQbPosition_PreservesNeighboringSlots()
        {
            // Calls InsertPlayer directly rather than via ProcessText: InputParser's SetSkillPlayer
            // (and every other non-QB roster dispatcher) unconditionally calls tool.SetFace before
            // InsertPlayer, and SetFace is an entirely separate, unresearched subsystem (face/portrait
            // graphics) -- out of scope here. SetQB is the only dispatcher that skips SetFace when no
            // Face= is given, which is why the QB1 round-trip tests above can go through ProcessText.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            // RB1 is flat player index teamIndex*32+2; RB2 (index+1) and QB2 (index-1) are its
            // immediate pointer-table neighbors within the same team -- both must survive a
            // length-changing write to RB1 untouched. The stock roster table has zero slack (see
            // GenesisRom_InsertPlayer_RejectsWriteThatWouldOverflowIntoAttributeTable_* below), so a
            // genuinely longer name needs room freed up first -- shrink an unrelated, far-away entry
            // (49ers QB1) to make space before growing this one.
            string qb2Before = tool.GetPlayerName("bills", "QB2");
            string rb2Before = tool.GetPlayerName("bills", "RB2");
            tool.InsertPlayer("49ers", "QB1", "x", "X", 0x00);

            tool.InsertPlayer("bills", "RB1", "alexander", "THUNDERBOLT", 0x22);

            Assert.AreEqual("alexanderTHUNDERBOLT", tool.GetPlayerName("bills", "RB1"));
            Assert.AreEqual(qb2Before, tool.GetPlayerName("bills", "QB2"), "QB2 (previous slot) must be untouched.");
            Assert.AreEqual(rb2Before, tool.GetPlayerName("bills", "RB2"), "RB2 (next slot) must be untouched.");
        }

        // The roster/name table for all 896 players (28 teams x 32 slots) is one shared, contiguous,
        // pointer-indexed region sitting immediately before the attribute table (0x89FC), with zero
        // slack space in the stock ROM -- confirmed directly: the very last entry (49ers DB2) ends at
        // exactly 0x89FB, one byte before the attribute table starts. Any net growth in total name
        // length, however it's distributed across teams, eventually pushes the table's true end past
        // 0x89FC, silently overwriting the attribute table's leading bytes (Bills' attribute records,
        // since Bills is playerIndex 0) with raw name text -- a real corruption this project hit in
        // practice applying a real-world 2026 NFL roster (whose player names are collectively longer
        // than the original 1993 roster). InsertPlayer must reject the whole write and report a clear
        // error instead of silently corrupting neighboring data.

        [TestMethod]
        public void GenesisRom_InsertPlayer_RejectsWriteThatWouldOverflowIntoAttributeTable_DirectEditOfLastEntry()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] romBefore = (byte[])rom.Clone();
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                // 49ers DB2 (stock: "mertonHANKS", 11 chars) is playerIndex 895 -- the very last of all
                // 896 roster entries, sitting flush against the attribute table with zero slack. Any
                // longer replacement name overflows immediately.
                tool.InsertPlayer("49ers", "DB2", "a", "VERYLONGLASTNAME", 0x99);
                StaticUtils.ShowErrors();

                CollectionAssert.AreEqual(romBefore, tool.OutputRom, "A write that would overflow into the attribute table must be rejected entirely.");
                Assert.AreEqual(1, giver.Errors.Count, "Expected exactly one error to be reported.");
                StringAssert.Contains(giver.Errors[0], "InsertPlayer");
                StringAssert.Contains(giver.Errors[0], "overflow");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
            }
        }

        [TestMethod]
        public void GenesisRom_InsertPlayer_RejectsWriteThatWouldOverflowIntoAttributeTable_CascadingEditOfEarlierTeam()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] romBefore = (byte[])rom.Clone();
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                // Bills QB1 (stock: "jimKELLY", playerIndex 0 -- the very first entry) growing by more
                // than the table's available slack (zero) must cascade-reject too: every entry after it,
                // all the way through 49ers DB2 at the far end, would shift past 0x89FC.
                tool.InsertPlayer("bills", "QB1", "jonathan", "KELLERMAN", 0x12);
                StaticUtils.ShowErrors();

                CollectionAssert.AreEqual(romBefore, tool.OutputRom, "A write that would overflow into the attribute table must be rejected entirely, even when the edit itself is far from the boundary.");
                Assert.AreEqual(1, giver.Errors.Count, "Expected exactly one error to be reported.");
                StringAssert.Contains(giver.Errors[0], "overflow");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
            }
        }

        [TestMethod]
        public void Compress49ersNames_ReplacesAllThirtyTwoPositionsWithPositionPlusFortyNiners()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.Compress49ersNames();

            Assert.AreEqual("qb1" + "49ERS", tool.GetPlayerName("49ers", "QB1"));
            Assert.AreEqual("db2" + "49ERS", tool.GetPlayerName("49ers", "DB2"));
            Assert.AreEqual("rolb" + "49ERS", tool.GetPlayerName("49ers", "ROLB"));
            Assert.AreEqual("k" + "49ERS", tool.GetPlayerName("49ers", "K"));
        }

        [TestMethod]
        public void Compress49ersNames_PreservesJerseyNumbers()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            int qb1PlayerIndex = 27 * 32; // 49ers = team index 27, QB1 = slot 0
            byte jerseyBefore = tool.OutputRom[GetQb1DataLoc(tool, qb1PlayerIndex)];

            tool.Compress49ersNames();

            byte jerseyAfter = tool.OutputRom[GetQb1DataLoc(tool, qb1PlayerIndex)];
            Assert.AreEqual(jerseyBefore, jerseyAfter, "Compress49ersNames should preserve jersey numbers, only replacing the name.");
        }

        [TestMethod]
        public void Compress49ersNames_DoesNotTouchOtherTeams()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string billsQb1Before = tool.GetPlayerName("bills", "QB1");
            string ramsQb1Before = tool.GetPlayerName("rams", "QB1"); // rams = team index 25, 49ers' immediate table-order neighbor

            tool.Compress49ersNames();

            Assert.AreEqual(billsQb1Before, tool.GetPlayerName("bills", "QB1"));
            Assert.AreEqual(ramsQb1Before, tool.GetPlayerName("rams", "QB1"));
        }

        [TestMethod]
        public void Compress49ersNames_ThenCheckRosterFit_MakesTheRealRosterSafeToApplyInOrder()
        {
            // The practical point of Compress49ersNames: run it first, and a real roster that otherwise
            // needed dozens of individual trims (see CheckRosterFit's own tests) should now have enough
            // pre-created slack for every other team's edits, with no other trimming required.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            tool.Compress49ersNames();

            // Bills QB1 growing by +12 against a stock-zero-slack ROM would normally be an unresolved
            // failure (see CheckRosterFit_ReportsUnresolvedFailure_WhenAbbreviationAloneIsNotEnough) --
            // confirm the slack Compress49ersNames created covers it with no suggestion needed at all.
            // CheckRosterFit reads available slack live off the given rom's own sentinel pointer, so it
            // sees the slack Compress49ersNames already created with no need to pass it in explicitly.
            Genesis_TSB1Tool.RosterFitReport report = Genesis_TSB1Tool.CheckRosterFit(tool.OutputRom,
                "TEAM = bills SimData=0x0\nQB1, alexander THUNDERBOLT, #7\n");

            Assert.IsTrue(report.SafeToApplyInOrder);
            Assert.AreEqual(0, report.Suggestions.Count);
            Assert.AreEqual(0, report.UnresolvedFailures.Count);
        }

        // Real bug found in practice, twice: first a fresh instance built from already-edited bytes
        // (e.g. a saved-then-reloaded ROM) used to silently assume 0 slack -- fixed with a synthetic
        // 0x00 terminator scheme that made this tool's own reads self-consistent. But that only fixed
        // this *tool's* bookkeeping: the real ROM has a genuine 897th "sentinel" pointer right after the
        // 896-entry roster pointer table (rosterPointerStart + 2*rosterPointerCount, 0x5F62 -- the same
        // "N+1 sentinel" pattern this file's own team-string table already used correctly), which the
        // real Genesis game almost certainly reads to find 49ers DB2's true length. The synthetic
        // terminator scheme never touched that real sentinel, so a real user's edited ROM had a clean,
        // short DB2 name by this tool's own reckoning while 58 bytes of leftover garbage sat between it
        // and rosterDataEnd -- invisible to this tool, but exactly what a reader trusting the
        // never-updated sentinel would read as part of DB2's name in-game. Now GetRosterEntryLocation
        // treats the sentinel as ordinary pointer data (entry 895's own "next" pointer lands on it
        // automatically) and InsertPlayer's AdjustRosterPointers call updates it exactly like every
        // other downstream pointer, so RosterRegionSlack is always read live off real ROM bytes with no
        // separate bookkeeping to go stale. These tests simulate the exact save-then-reload sequence
        // that exposed the first bug, and directly check the sentinel value for the second.

        [TestMethod]
        public void GenesisRom_LastRosterEntry_SurvivesReloadAfterDirectShrink()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.InsertPlayer("49ers", "DB2", "db2", "49ers", 0x00); // exactly Compress49ersNames' own placeholder shape
            Genesis_TSB1Tool reloaded = new Genesis_TSB1Tool((byte[])tool.OutputRom.Clone());

            Assert.AreEqual("db249ERS", reloaded.GetPlayerName("49ers", "DB2"));
            Assert.AreEqual(tool.RosterRegionSlack, reloaded.RosterRegionSlack);
        }

        [TestMethod]
        public void GenesisRom_LastRosterEntry_SurvivesReloadAfterAnEarlierTeamsCascadingShrink()
        {
            // The actual bug scenario: DB2 itself is never directly touched -- an earlier team's edit
            // just cascades slack past it.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            string db2Before = tool.GetPlayerName("49ers", "DB2");

            tool.InsertPlayer("bills", "QB1", "al", "SMITH", 0x07); // shorter than stock "jimKELLY" -> shrink
            Genesis_TSB1Tool reloaded = new Genesis_TSB1Tool((byte[])tool.OutputRom.Clone());

            Assert.AreEqual(db2Before, reloaded.GetPlayerName("49ers", "DB2"), "DB2's content didn't change, only its position -- must still read back correctly after reload.");
        }

        [TestMethod]
        public void GenesisRom_RosterRegionSlack_IsZeroForStockRom()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.AreEqual(0, tool.RosterRegionSlack, "The stock ROM has zero slack (DB2 ends exactly at rosterDataEnd-1) -- a fresh instance must not invent any.");
        }

        [TestMethod]
        public void GenesisRom_ShrinkingLastEntry_MovesTheRealSentinelPointer_NotJustThisToolsOwnBookkeeping()
        {
            // The actual real-hardware-facing bug: confirms the fix updates the *real* 897th pointer in
            // the ROM (0x5F62, right after the 896-entry roster pointer table), not just something only
            // this tool's own GetPlayerName respects. This is the value a real reader (the game itself,
            // or a fresh instance, or any other tool) would use to find DB2's true length.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            const int sentinelLoc = 0x5F62;
            int sentinelBefore = (rom[sentinelLoc] << 8) + rom[sentinelLoc + 1];

            tool.InsertPlayer("49ers", "DB2", "db2", "49ers", 0x00); // shrinks from stock "mertonHANKS" (11 chars) to "db249ERS" (8)

            int sentinelAfter = (tool.OutputRom[sentinelLoc] << 8) + tool.OutputRom[sentinelLoc + 1];
            Assert.AreEqual(sentinelBefore - 3, sentinelAfter, "The sentinel pointer must move by the exact same shrink amount as every other entry's pointer would.");
            Assert.AreEqual(3, tool.RosterRegionSlack, "RosterRegionSlack is derived directly from this same sentinel value.");
        }

        [TestMethod]
        public void GenesisRom_ResolveRosterFit_AutoTrimsSurviveReloadAndReadBackCorrectly()
        {
            // End-to-end version of the same bug, through the real workflow: ResolveRosterFit(autoApply)
            // then simulate save+reload, then confirm every auto-trimmed player (including whichever one
            // happens to be the literal last table entry) still reads back correctly.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Genesis_TSB1Tool.RosterFitReport report = tool.ResolveRosterFit(
                "TEAM = bills SimData=0x0\nQB1, alexanderchristopher THOMPSONVILLE, #7\n", true);
            Genesis_TSB1Tool reloaded = new Genesis_TSB1Tool((byte[])tool.OutputRom.Clone());

            foreach (Genesis_TSB1Tool.RosterPlayer trimmed in report.AutoTrimmedPlayers)
            {
                string expected = trimmed.Position.ToLower() + trimmed.Team.ToUpper();
                Assert.AreEqual(expected, reloaded.GetPlayerName(trimmed.Team, trimmed.Position));
            }
            Assert.AreEqual(tool.RosterRegionSlack, reloaded.RosterRegionSlack);
        }

        private static int GetQb1DataLoc(ITecmoTool tool, int playerIndex)
        {
            byte[] rom = tool.OutputRom;
            int pointerLoc = RosterPointerStart + 2 * playerIndex;
            return (rom[pointerLoc] << 8) + rom[pointerLoc + 1];
        }
    }
}
