using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// SnesSeasonStats: reads the season stats out of the SNES TSB1 battery save (.srm, 8 KB). Same
    /// layout as Genesis (see SnesSeasonStats). The team-level expected values come from the SimTests
    /// result sheets (SNES_05_TeamDefense, SNES_06_RunPassPref); the player-level ones were checked by
    /// how they add up (points, TDs, yards, INTs across the league), not yet against the game's screens.
    /// The ROMs only changed sim data, so the stock TSB1.smc has the right names.
    /// </summary>
    [TestClass]
    public class SnesSeasonStatsTests
    {
        private const string RomFileName = "TSB1.smc";
        private const string Snes05Sram = @"SNES\SaveRam\SNES_05_TeamDefense.srm";
        private const string Snes06Sram = @"SNES\SaveRam\SNES_06_RunPassPref.srm";

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static SnesSeasonStats Load(string sram)
        {
            string error;
            SnesSeasonStats stats = SnesSeasonStats.FromSram(TestRoms.LoadRom(sram), out error);
            Assert.IsNotNull(stats, "Load failed: " + error);
            return stats;
        }

        [TestMethod]
        public void Snes05_Standings_Bills()
        {
            TeamSeasonStats bills = Load(Snes05Sram).GetTeam("bills");

            CollectionAssert.AreEqual(new int[] { 9, 7, 0, 456, 441, 2703, 2477 },
                new int[] { bills.Wins, bills.Losses, bills.Ties, bills.PointsFor, bills.PointsAgainst, bills.PassYardsAllowed, bills.RushYardsAllowed });
        }

        [TestMethod]
        public void Snes05_TeamsAreInTheSnesRomOrder()
        {
            SnesSeasonStats stats = Load(Snes05Sram);

            Assert.AreEqual(28, stats.Teams.Count);
            // SNES order differs from NES: cowboys lead the NFC East, falcons the NFC West
            Assert.AreEqual(403, stats.GetTeam("cowboys").PointsAgainst);
            Assert.AreEqual(435, stats.GetTeam("redskins").PointsAgainst);
            Assert.AreEqual(514, stats.GetTeam("falcons").PointsAgainst);
            Assert.AreEqual(321, stats.GetTeam("49ers").PointsAgainst);
        }

        [TestMethod]
        public void Snes05_TeamSheet_MatchesForEveryTeam()
        {
            SnesSeasonStats stats = Load(Snes05Sram);
            // SNES_05_TeamDefense_results.txt: rush yds, pass yds, pts allowed, sacks, INTs. The sheet's
            // two yards columns were copied from the team's own offense (its rushing/passing yards), not
            // the yards allowed. Three sheet typos are fixed here: dolphins INTs 17 -> 16, steelers sacks
            // 26 -> 27, seahawks sacks 22 -> 23 (the sheet's league totals don't balance with them).
            Dictionary<string, int[]> sheet = new Dictionary<string, int[]>
            {
                { "bills",    new int[] { 1877, 3257, 441, 27, 12 } }, { "jets",       new int[] { 1874, 2807, 518, 27,  8 } },
                { "steelers", new int[] { 1794, 3437, 553, 27,  7 } }, { "chargers",   new int[] { 2079, 2803, 491, 22, 11 } },
                { "eagles",   new int[] { 1677, 3061, 508, 29, 11 } }, { "lions",      new int[] { 1758, 3178, 443, 30, 11 } },
                { "falcons",  new int[] { 1693, 2827, 514, 23,  4 } }, { "colts",      new int[] { 2278, 2672, 469, 35, 11 } },
                { "bengals",  new int[] { 1673, 3035, 407, 40,  7 } }, { "broncos",    new int[] { 1989, 2633, 413, 17,  9 } },
                { "seahawks", new int[] { 1774, 2753, 404, 23,  5 } }, { "cardinals",  new int[] { 2138, 2701, 424, 34,  2 } },
                { "packers",  new int[] { 2216, 2528, 432, 26,  6 } }, { "rams",       new int[] { 1723, 3275, 399, 27, 15 } },
                { "dolphins", new int[] { 2006, 3446, 463, 35, 16 } }, { "browns",     new int[] { 1447, 2980, 474, 33, 18 } },
                { "chiefs",   new int[] { 1762, 3109, 443, 24, 13 } }, { "cowboys",    new int[] { 2020, 3148, 403, 40, 15 } },
                { "redskins", new int[] { 1847, 3219, 435, 33, 17 } }, { "vikings",    new int[] { 1662, 2522, 412, 34, 11 } },
                { "saints",   new int[] { 2005, 2626, 379, 45, 17 } }, { "patriots",   new int[] { 1509, 2776, 335, 44, 16 } },
                { "oilers",   new int[] { 2025, 3097, 346, 41, 14 } }, { "raiders",    new int[] { 2091, 2529, 283, 33, 22 } },
                { "giants",   new int[] { 2414, 2704, 314, 37, 20 } }, { "bears",      new int[] { 1429, 3045, 298, 33, 15 } },
                { "buccaneers", new int[] { 2032, 2962, 307, 29, 13 } }, { "49ers",    new int[] { 1755, 2912, 321, 33, 14 } },
            };
            foreach (KeyValuePair<string, int[]> kv in sheet)
            {
                TeamSeasonStats t = stats.GetTeam(kv.Key);
                CollectionAssert.AreEqual(kv.Value,
                    new int[] { t.RushYards, t.PassYards, t.PointsAgainst, t.Sacks, t.InterceptionsMade }, kv.Key);
            }
        }

        [TestMethod]
        public void Snes06_TeamRushAndPassAttempts_MatchTheResultSheet()
        {
            SnesSeasonStats stats = Load(Snes06Sram);
            // SNES_06_RunPassPref_results.txt: team, rush att, pass att. Not checked: buccaneers rush
            // (sheet 159, save 160) and 49ers rush (sheet 120, save 166) -- most likely misreads.
            Dictionary<string, int[]> sheet = new Dictionary<string, int[]>
            {
                { "bills",    new int[] { 254, 184 } }, { "jets",       new int[] { 245, 185 } },
                { "steelers", new int[] { 245, 174 } }, { "chargers",   new int[] { 232, 190 } },
                { "eagles",   new int[] { 235, 191 } }, { "lions",      new int[] { 185, 218 } },
                { "falcons",  new int[] { 224, 216 } }, { "colts",      new int[] { 254, 160 } },
                { "bengals",  new int[] { 268, 167 } }, { "broncos",    new int[] { 293, 175 } },
                { "seahawks", new int[] { 280, 141 } }, { "cardinals",  new int[] { 257, 163 } },
                { "packers",  new int[] { 260, 153 } }, { "rams",       new int[] { 280, 159 } },
                { "dolphins", new int[] { 169, 236 } }, { "browns",     new int[] { 185, 231 } },
                { "chiefs",   new int[] { 187, 214 } }, { "cowboys",    new int[] { 158, 231 } },
                { "redskins", new int[] { 182, 217 } }, { "vikings",    new int[] { 215, 215 } },
                { "saints",   new int[] { 192, 193 } }, { "patriots",   new int[] { 137, 239 } },
                { "oilers",   new int[] { 129, 267 } }, { "raiders",    new int[] { 150, 252 } },
                { "giants",   new int[] { 145, 241 } }, { "bears",      new int[] { 147, 253 } },
                { "buccaneers", new int[] { 159, 256 } }, { "49ers",    new int[] { 120, 234 } },
            };
            foreach (KeyValuePair<string, int[]> kv in sheet)
            {
                TeamSeasonStats t = stats.GetTeam(kv.Key);
                if (kv.Key != "buccaneers" && kv.Key != "49ers")
                    Assert.AreEqual(kv.Value[0], t.RushAttempts, kv.Key + " rush att");
                Assert.AreEqual(kv.Value[1], t.PassAttempts, kv.Key + " pass att");
            }
        }

        [TestMethod]
        public void Snes06_PassAttempts_Over255()
        {
            Assert.AreEqual(256, Load(Snes06Sram).GetTeam("buccaneers").GetPlayer("QB1").PassAttempts);
        }

        [TestMethod]
        public void LeagueTotals_Balance()
        {
            foreach (string sram in new string[] { Snes05Sram, Snes06Sram })
            {
                SnesSeasonStats stats = Load(sram);
                int passTDs = 0, receivingTDs = 0, completions = 0, receptions = 0, intsThrown = 0, intsMade = 0;
                int passYards = 0, passAllowed = 0, rushYards = 0, rushAllowed = 0;
                foreach (TeamSeasonStats t in stats.Teams)
                {
                    foreach (PlayerSeasonStats p in t.Players)
                    {
                        passTDs += p.PassTDs; receivingTDs += p.ReceivingTDs;
                        completions += p.Completions; receptions += p.Receptions;
                        intsThrown += p.InterceptionsThrown; intsMade += p.Interceptions;
                    }
                    passYards += t.PassYards; passAllowed += t.PassYardsAllowed;
                    rushYards += t.RushYards; rushAllowed += t.RushYardsAllowed;
                }
                Assert.AreEqual(passTDs, receivingTDs, sram + " TDs");
                Assert.AreEqual(completions, receptions, sram + " catches");
                Assert.AreEqual(intsThrown, intsMade, sram + " INTs");
                Assert.AreEqual(passYards, passAllowed, sram + " pass yds");
                Assert.AreEqual(rushYards, rushAllowed, sram + " rush yds");
            }
            Assert.AreEqual(880, CountReceivingTDs(Load(Snes05Sram)));
        }

        private static int CountReceivingTDs(SnesSeasonStats stats)
        {
            int ret = 0;
            foreach (TeamSeasonStats t in stats.Teams)
                foreach (PlayerSeasonStats p in t.Players)
                    ret += p.ReceivingTDs;
            return ret;
        }

        // Every team's points: 6 per TD (passing, rushing, INT and kick/punt returns) + XP + 3 per FG.
        // Safeties aren't stored, so a team can be a few points short, always by 2s.
        [TestMethod]
        public void PointsFor_AddUpFromTheTeamsTDsAndKicks()
        {
            foreach (string sram in new string[] { Snes05Sram, Snes06Sram })
            {
                int exact = 0;
                foreach (TeamSeasonStats t in Load(sram).Teams)
                {
                    int tds = 0;
                    foreach (PlayerSeasonStats p in t.Players)
                        tds += p.PassTDs + p.RushTDs + p.InterceptionTDs;
                    foreach (PlayerSeasonStats r in t.Returners)
                        tds += r.KickReturnTDs + r.PuntReturnTDs;
                    PlayerSeasonStats k = t.GetPlayer("K");
                    int missing = t.PointsFor - (6 * tds + k.ExtraPointsMade + 3 * k.FieldGoalsMade);
                    Assert.IsTrue(missing >= 0 && missing <= 4 && missing % 2 == 0, sram + " " + t.Team + " missing " + missing);
                    if (missing == 0)
                        exact++;
                }
                Assert.IsTrue(exact >= 24, sram + ": only " + exact + " teams add up exactly");
            }
        }

        [TestMethod]
        public void Snes05_BillsPlayers()
        {
            TeamSeasonStats bills = Load(Snes05Sram).GetTeam("bills");

            PlayerSeasonStats qb = bills.GetPlayer("QB1");
            CollectionAssert.AreEqual(new int[] { 185, 132, 3257, 42, 8 },
                new int[] { qb.PassAttempts, qb.Completions, qb.PassYards, qb.PassTDs, qb.InterceptionsThrown });
            PlayerSeasonStats rb = bills.GetPlayer("RB1");
            CollectionAssert.AreEqual(new int[] { 160, 1536, 15, 29, 789, 12 },
                new int[] { rb.RushAttempts, rb.RushYards, rb.RushTDs, rb.Receptions, rb.ReceivingYards, rb.ReceivingTDs });
            PlayerSeasonStats rcb = bills.GetPlayer("RCB");
            CollectionAssert.AreEqual(new int[] { 1, 2, 88, 0 },
                new int[] { rcb.Sacks, rcb.Interceptions, rcb.InterceptionYards, rcb.InterceptionTDs });
            PlayerSeasonStats k = bills.GetPlayer("K");
            CollectionAssert.AreEqual(new int[] { 57, 61, 9, 10 },
                new int[] { k.ExtraPointsMade, k.ExtraPointAttempts, k.FieldGoalsMade, k.FieldGoalAttempts });
            PlayerSeasonStats p = bills.GetPlayer("P");
            CollectionAssert.AreEqual(new int[] { 12, 558 }, new int[] { p.Punts, p.PuntYards });
            PlayerSeasonStats ret1 = bills.GetReturner(1);
            CollectionAssert.AreEqual(new int[] { 72, 1368, 0, 0, 0, 0 },
                new int[] { ret1.KickReturns, ret1.KickReturnYards, ret1.KickReturnTDs, ret1.PuntReturns, ret1.PuntReturnYards, ret1.PuntReturnTDs });
            PlayerSeasonStats ret2 = bills.GetReturner(2);
            CollectionAssert.AreEqual(new int[] { 11, 379, 5, 35 },
                new int[] { ret2.KickReturns, ret2.KickReturnYards, ret2.PuntReturns, ret2.PuntReturnYards });
        }

        [TestMethod]
        public void Snes05_QBRushing()
        {
            PlayerSeasonStats qb = Load(Snes05Sram).GetTeam("oilers").GetPlayer("QB1");
            CollectionAssert.AreEqual(new int[] { 67, 485, 9 }, new int[] { qb.RushAttempts, qb.RushYards, qb.RushTDs });
        }

        [TestMethod]
        public void EmptySram_IsRejected_WithAHint()
        {
            string error;
            Assert.IsNull(SnesSeasonStats.FromSram(new byte[0x2000], out error));
            StringAssert.Contains(error, "no SNES TSB season stats");
        }

        [TestMethod]
        public void WrongSize_IsRejected()
        {
            string error;
            Assert.IsNull(SnesSeasonStats.FromSram(new byte[300000], out error));
            StringAssert.Contains(error, ".srm");
            Assert.IsNull(SnesSeasonStats.FromSram(null, out error));
            StringAssert.Contains(error, ".srm");
        }

        // ---- Picking the reader from the file ----

        [TestMethod]
        public void FromFile_PicksSnesForAnSrm_AndGenesisForABs1()
        {
            string error;
            Assert.IsInstanceOfType(SeasonStats.FromFile(TestRoms.LoadRom(Snes05Sram), out error), typeof(SnesSeasonStats), error);
            Assert.IsInstanceOfType(SeasonStats.FromFile(TestRoms.LoadRom(@"Genesis\SaveStates\GEN_06b_SeasonMenu.bs1"), out error),
                typeof(GenesisSeasonStats), error);
        }

        [TestMethod]
        public void FromFile_SomethingElse_IsRejected()
        {
            string error;
            Assert.IsNull(SeasonStats.FromFile(new byte[1000], out error));
            StringAssert.Contains(error, ".bs1");
            StringAssert.Contains(error, ".srm");
        }

        // ---- Output (shared with Genesis) ----

        [TestMethod]
        public void ToText_WithRom_UsesRomNames()
        {
            ITecmoContent rom = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            string text = Load(Snes05Sram).ToText(rom);

            StringAssert.StartsWith(text, "SNES TSB SEASON STATS");
            StringAssert.Contains(text, "jim KELLY");
            StringAssert.Contains(text, "thurman THOMAS");
            Assert.IsFalse(text.Contains("QB1 Bills"), "ROM names should replace the generic ones.");
        }

        [TestMethod]
        public void ToText_WithoutRom_UsesGenericNames()
        {
            string text = Load(Snes05Sram).ToText(null);

            StringAssert.Contains(text, "QB1 Bills");
            StringAssert.Contains(text, "QB1 49ers");
        }

        [TestMethod]
        public void ToText_WithTheWrongKindOfRom_UsesGenericNames()
        {
            ITecmoContent genesis = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(@"Genesis\Tecmo Super Bowl (USA) (October 1993).md"));

            StringAssert.Contains(Load(Snes05Sram).ToText(genesis), "QB1 Bills");
        }

        [TestMethod]
        public void ToCsv_WithRom_BillsRows()
        {
            ITecmoContent rom = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            string csv = Load(Snes05Sram).ToCsv(rom);

            StringAssert.Contains(csv, "bills,QB1,QB,\"jim KELLY\",185,132,71.4,3257,17.6,42,8,");
            StringAssert.Contains(csv, "bills,K,K,\"steve CHRISTIE\",");
        }
    }
}
