using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Tsb3SeasonStats: reads the season stats out of the SNES TSB3 battery save (.srm, 32 KB).
    /// TSB3_Season.srm is one full simmed season of TSB3.smc with the Bears' offense sim values at minimum
    /// (SimTests\TSB3_study\TSB3_Bears_MinSim.srm). Expected values from the game's screens
    /// (TSB3_study\BillsOffense_results.csv, BillsDefense_results.csv, TotalOffense/TotalDefense leader
    /// screens; punt return TDs: Tim Brown and O.J. McDuffie only). Layout: TSB3_study\TSB3_SRAM_Stats.txt.
    /// TSB3 keeps stats by player ID (original team and slot), and each team's roster in the save lists the
    /// IDs of who plays where now (trades, free agents).
    /// </summary>
    [TestClass]
    public class Tsb3SeasonStatsTests
    {
        private const string RomFileName = "TSB3.smc";
        private const string SeasonSram = @"SNES\SaveRam\TSB3_Season.srm";
        private const string Tsb2SeasonSram = @"SNES\SaveRam\TSB2_1994_Season.srm";

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static Tsb3SeasonStats Load()
        {
            string error;
            Tsb3SeasonStats stats = Tsb3SeasonStats.FromSram(TestRoms.LoadRom(SeasonSram), out error);
            Assert.IsNotNull(stats, "Load failed: " + error);
            return stats;
        }

        private static ITecmoContent LoadTool(string rom)
        {
            return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom));
        }

        [TestMethod]
        public void FromFile_Tsb3Srm_IsReadAsTsb3_AndTsb2StillAsTsb2()
        {
            string error;
            SeasonStats tsb3 = SeasonStats.FromFile(TestRoms.LoadRom(SeasonSram), out error);
            Assert.IsInstanceOfType(tsb3, typeof(Tsb3SeasonStats), error);
            Assert.AreEqual("SNES TSB3", tsb3.RomKind);

            SeasonStats tsb2 = SeasonStats.FromFile(TestRoms.LoadRom(Tsb2SeasonSram), out error);
            Assert.IsInstanceOfType(tsb2, typeof(Tsb2SeasonStats), error);
        }

        [TestMethod]
        public void EachReader_RejectsTheOtherGamesSave()
        {
            string error;
            Assert.IsNull(Tsb3SeasonStats.FromSram(TestRoms.LoadRom(Tsb2SeasonSram), out error), "TSB2 save read as TSB3");
            Assert.IsNull(Tsb2SeasonStats.FromSram(TestRoms.LoadRom(SeasonSram), out error), "TSB3 save read as TSB2");
        }

        [TestMethod]
        public void EmptySave_ReturnsNullWithReason()
        {
            string error;
            Assert.IsNull(SeasonStats.FromFile(new byte[0x8000], out error));
            StringAssert.Contains(error, "TSB3");
        }

        [TestMethod]
        public void Standings_AndYardsAllowed()
        {
            Tsb3SeasonStats stats = Load();
            TeamSeasonStats bears = stats.GetTeam("bears");
            TeamSeasonStats falcons = stats.GetTeam("falcons");

            Assert.AreEqual(30, stats.Teams.Count);
            CollectionAssert.AreEqual(new int[] { 3, 13, 0, 129, 315 },
                new int[] { bears.Wins, bears.Losses, bears.Ties, bears.PointsFor, bears.PointsAgainst });
            // Total Defense screen
            CollectionAssert.AreEqual(new int[] { 3204, 1238 }, new int[] { falcons.PassYardsAllowed, falcons.RushYardsAllowed });
        }

        [TestMethod]
        public void TeamOffense_MatchesTheTotalOffenseScreen()
        {
            Tsb3SeasonStats stats = Load();
            Dictionary<string, int[]> screen = new Dictionary<string, int[]>
            {   // pass, rush
                { "dolphins", new int[] { 5039, 729 } }, { "raiders", new int[] { 4088, 1387 } },
                { "browns", new int[] { 3517, 1948 } }, { "vikings", new int[] { 4539, 886 } },
                { "patriots", new int[] { 4232, 1117 } }, { "broncos", new int[] { 3569, 1700 } },
                { "redskins", new int[] { 3425, 1745 } }, { "buccaneers", new int[] { 3212, 1934 } },
                { "saints", new int[] { 3955, 1163 } }, { "chargers", new int[] { 3069, 2001 } },
                // the screen says 1602; the Bills' players (on the game's own player screens) add up to 1603
                { "bills", new int[] { 3540, 1603 } },
            };
            foreach (KeyValuePair<string, int[]> row in screen)
            {
                TeamSeasonStats t = stats.GetTeam(row.Key);
                CollectionAssert.AreEqual(row.Value, new int[] { t.PassYards, t.RushYards }, row.Key);
            }
        }

        [TestMethod]
        public void Bills_Offense_MatchesTheGameScreens()
        {
            TeamSeasonStats bills = Load().GetTeam("bills");

            PlayerSeasonStats kelly = bills.GetPlayer("QB1");
            CollectionAssert.AreEqual(new int[] { 394, 255, 3540, 28, 14, 31, 176, 0 },
                new int[] { kelly.PassAttempts, kelly.Completions, kelly.PassYards, kelly.PassTDs, kelly.InterceptionsThrown,
                            kelly.RushAttempts, kelly.RushYards, kelly.RushTDs });
            PlayerSeasonStats rb1 = bills.GetPlayer("RB1");
            CollectionAssert.AreEqual(new int[] { 301, 1065, 6, 52, 622, 4 },
                new int[] { rb1.RushAttempts, rb1.RushYards, rb1.RushTDs, rb1.Receptions, rb1.ReceivingYards, rb1.ReceivingTDs });
            PlayerSeasonStats wr2 = bills.GetPlayer("WR2"); // Andre Reed
            CollectionAssert.AreEqual(new int[] { 67, 1055, 8, 6, 47 },
                new int[] { wr2.Receptions, wr2.ReceivingYards, wr2.ReceivingTDs, wr2.RushAttempts, wr2.RushYards });

            PlayerSeasonStats k = bills.GetPlayer("K");
            PlayerSeasonStats p = bills.GetPlayer("P");
            CollectionAssert.AreEqual(new int[] { 35, 37, 20, 23, 86, 3459 },
                new int[] { k.ExtraPointsMade, k.ExtraPointAttempts, k.FieldGoalsMade, k.FieldGoalAttempts, p.Punts, p.PuntYards });
        }

        [TestMethod]
        public void Bills_Returns_ArePerPlayer()
        {
            TeamSeasonStats bills = Load().GetTeam("bills");

            PlayerSeasonStats wr3 = bills.Returners.Find(r => r.Position == "WR3"); // Bucky Brooks
            CollectionAssert.AreEqual(new int[] { 66, 890, 0 }, new int[] { wr3.KickReturns, wr3.KickReturnYards, wr3.KickReturnTDs });
            PlayerSeasonStats lcb = bills.Returners.Find(r => r.Position == "LCB"); // Jeff Burris
            CollectionAssert.AreEqual(new int[] { 64, 580, 0 }, new int[] { lcb.PuntReturns, lcb.PuntReturnYards, lcb.PuntReturnTDs });
        }

        [TestMethod]
        public void Bills_Defense_PlayersWhoChangedSlots()
        {
            TeamSeasonStats bills = Load().GetTeam("bills");

            // the save's Bills roster: RCB = the Bills' original FS (matt DARBY), DB3 = their original RCB
            // (thomas SMITH), FS = free agent dana HALL (0 sacks, 0 INTs; 71 tackles)
            PlayerSeasonStats rcb = bills.GetPlayer("RCB");
            CollectionAssert.AreEqual(new int[] { 0, 2, 43, 1 }, new int[] { rcb.Sacks, rcb.Interceptions, rcb.InterceptionYards, rcb.InterceptionTDs }, "Darby");
            PlayerSeasonStats db3 = bills.GetPlayer("DB3");
            CollectionAssert.AreEqual(new int[] { 0, 1, 8, 0 }, new int[] { db3.Sacks, db3.Interceptions, db3.InterceptionYards, db3.InterceptionTDs }, "T. Smith");
            PlayerSeasonStats re = bills.GetPlayer("RE");
            PlayerSeasonStats rolb = bills.GetPlayer("ROLB");
            PlayerSeasonStats lb5 = bills.GetPlayer("LB5");
            CollectionAssert.AreEqual(new int[] { 14, 12, 1 }, new int[] { re.Sacks, rolb.Sacks, lb5.Interceptions });
        }

        [TestMethod]
        public void PuntReturnTDs_TimBrownAndMcDuffieOnly_NoKickReturnTDs()
        {
            Tsb3SeasonStats stats = Load();
            int pr = 0, kr = 0;
            foreach (TeamSeasonStats t in stats.Teams)
                foreach (PlayerSeasonStats r in t.Returners)
                {
                    pr += r.PuntReturnTDs;
                    kr += r.KickReturnTDs;
                }
            Assert.AreEqual(2, pr);
            Assert.AreEqual(0, kr);
            Assert.AreEqual(1, stats.GetTeam("raiders").Returners.Find(r => r.Position == "WR2").PuntReturnTDs);
            Assert.AreEqual(1, stats.GetTeam("dolphins").Returners.Find(r => r.Position == "WR2").PuntReturnTDs);
        }

        [TestMethod]
        public void IsRomForTheseStats_OnlyTsb3()
        {
            Tsb3SeasonStats stats = Load();
            Assert.IsTrue(stats.IsRomForTheseStats(LoadTool(RomFileName)));
            Assert.IsFalse(stats.IsRomForTheseStats(LoadTool("TSB2_U.smc")));
            Assert.IsFalse(stats.IsRomForTheseStats(LoadTool("TSB1.smc")));
            Assert.IsFalse(stats.IsRomForTheseStats(null));
        }

        [TestMethod]
        public void ToText_WithRom_NamesWhoPlaysThereNow()
        {
            string text = Load().ToText(LoadTool(RomFileName));

            StringAssert.StartsWith(text, "SNES TSB3 SEASON STATS");
            StringAssert.Contains(text, "jim KELLY");
            StringAssert.Contains(text, "tim BROWN");
            Assert.IsFalse(text.Contains("QB1 Bills"), "ROM names should replace the generic ones.");
        }

        [TestMethod]
        public void ToCsv_WithRom_PlayersWhoChangedSlotsGetTheirOwnNames()
        {
            List<string> lines = new List<string>(Load().ToCsv(LoadTool(RomFileName)).Split('\n'));

            CollectionAssert.Contains(lines, "bills,RCB,DB,\"matt DARBY\",,,,,,,,,,,,,,,,0,2,43,1,,,,,,,,,,,,,,,");
            CollectionAssert.Contains(lines, "bills,DB3,DB,\"thomas SMITH\",,,,,,,,,,,,,,,,0,1,8,0,,,,,,,,,,,,,,,");
            CollectionAssert.Contains(lines, "bills,LCB,DB,\"jeff BURRIS\",,,,,,,,,,,,,,,,0,3,38,0,,,,,,,,0,0,,0,64,580,9.1,0");
        }

        [TestMethod]
        public void ToText_FreeAgentOnTheRoster_HasHisName()
        {
            // Dana Hall (free agent pool, slot FS) plays FS for the Bills; no INTs, so check the name lookup
            Tsb3SeasonStats stats = Load();
            Assert.AreEqual("dana HALL", stats.GetPlayerName(LoadTool(RomFileName), "bills", "FS"));
        }

        [TestMethod]
        public void ToText_WithoutRom_UsesGenericNames()
        {
            StringAssert.Contains(Load().ToText(null), "QB1 Bills");
        }
    }
}
