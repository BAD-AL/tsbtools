using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Tsb2SeasonStats: reads the season stats out of the SNES TSB2 battery save (.srm, 32 KB).
    /// TSB2_1994_Season.srm is one full season of the stock ROM (TSB2_U.smc), 1994 schedule, all games
    /// simmed (SimTests\TSB2_study\TSB_II.srm). The Bills' expected values come from the game's screens
    /// (TSB2_study\BillsOffenseStats.csv, BillsDefenseStats.csv).
    /// </summary>
    [TestClass]
    public class Tsb2SeasonStatsTests
    {
        private const string RomFileName = "TSB2_U.smc";
        private const string Season1994Sram = @"SNES\SaveRam\TSB2_1994_Season.srm";
        // A second season of the stock ROM. Its players' rushing yards are 18 short of the rushing yards
        // allowed: the game loses a run's credit when the carrier pick lands on the QB who isn't playing
        // (TSB2_Sim_Engine.txt, bug G3; the stock Redskins' QB2 has a carry share of 1).
        private const string StockRun2Sram = @"SNES\SaveRam\TSB2_1994_StockRun2.srm";

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static Tsb2SeasonStats Load(string sram)
        {
            string error;
            Tsb2SeasonStats stats = Tsb2SeasonStats.FromSram(TestRoms.LoadRom(sram), out error);
            Assert.IsNotNull(stats, "Load failed: " + error);
            return stats;
        }

        private static ITecmoContent LoadTool(string rom)
        {
            return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom));
        }

        private static List<string> CsvLines(string csv)
        {
            return new List<string>(csv.Split('\n'));
        }

        [TestMethod]
        public void FromFile_Tsb2Srm_IsReadAsTsb2()
        {
            string error;
            SeasonStats stats = SeasonStats.FromFile(TestRoms.LoadRom(Season1994Sram), out error);

            Assert.IsInstanceOfType(stats, typeof(Tsb2SeasonStats), error);
            Assert.AreEqual("SNES TSB2", stats.RomKind);
        }

        [TestMethod]
        public void Season_IsTheScheduleThatWasPlayed()
        {
            Assert.AreEqual(3, Load(Season1994Sram).Season); // 1994 = TSBTool season 3
        }

        [TestMethod]
        public void Standings_TeamsInTsb2Order()
        {
            Tsb2SeasonStats stats = Load(Season1994Sram);
            TeamSeasonStats bills = stats.GetTeam("bills");
            TeamSeasonStats niners = stats.GetTeam("49ers");

            Assert.AreEqual(28, stats.Teams.Count);
            CollectionAssert.AreEqual(new int[] { 7, 9, 0, 359, 315, 2930, 1718 },
                new int[] { bills.Wins, bills.Losses, bills.Ties, bills.PointsFor, bills.PointsAgainst, bills.PassYardsAllowed, bills.RushYardsAllowed });
            CollectionAssert.AreEqual(new int[] { 11, 5, 0, 344, 270, 2770, 1600 },
                new int[] { niners.Wins, niners.Losses, niners.Ties, niners.PointsFor, niners.PointsAgainst, niners.PassYardsAllowed, niners.RushYardsAllowed });
        }

        [TestMethod]
        public void Bills_Offense_MatchesTheGameScreens()
        {
            TeamSeasonStats bills = Load(Season1994Sram).GetTeam("bills");

            PlayerSeasonStats kelly = bills.GetPlayer("QB1");
            CollectionAssert.AreEqual(new int[] { 208, 132, 2943, 25, 14, 21, 167, 4 },
                new int[] { kelly.PassAttempts, kelly.Completions, kelly.PassYards, kelly.PassTDs, kelly.InterceptionsThrown,
                            kelly.RushAttempts, kelly.RushYards, kelly.RushTDs });

            PlayerSeasonStats rb1 = bills.GetPlayer("RB1");
            CollectionAssert.AreEqual(new int[] { 176, 1464, 10, 9, 228, 1 },
                new int[] { rb1.RushAttempts, rb1.RushYards, rb1.RushTDs, rb1.Receptions, rb1.ReceivingYards, rb1.ReceivingTDs });

            PlayerSeasonStats wr2 = bills.GetPlayer("WR2"); // Andre Reed
            CollectionAssert.AreEqual(new int[] { 31, 683, 9 }, new int[] { wr2.Receptions, wr2.ReceivingYards, wr2.ReceivingTDs });
        }

        [TestMethod]
        public void Bills_Returns_ArePerPlayer()
        {
            TeamSeasonStats bills = Load(Season1994Sram).GetTeam("bills");

            // returns are kept per RB/WR/TE and per DB (RCB..DB3), like NES
            Assert.AreEqual(17, bills.Returners.Count);
            PlayerSeasonStats wr4 = bills.Returners.Find(r => r.Position == "WR4"); // Russell Copeland
            CollectionAssert.AreEqual(new int[] { 64, 1012, 0, 15, 115, 0 },
                new int[] { wr4.KickReturns, wr4.KickReturnYards, wr4.KickReturnTDs, wr4.PuntReturns, wr4.PuntReturnYards, wr4.PuntReturnTDs });
            Assert.IsNotNull(bills.Returners.Find(r => r.Position == "DB3"));
        }

        [TestMethod]
        public void Bills_Defense_AllEighteenDefendersInRosterOrder()
        {
            TeamSeasonStats bills = Load(Season1994Sram).GetTeam("bills");

            // BillsDefenseStats.csv, in TSB2Tool's order: RE NT LE RE2 NT2 LE2 ROLB RILB LILB LOLB LB5 RCB LCB DB1 DB2 FS SS DB3
            Dictionary<string, int[]> sheet = new Dictionary<string, int[]>
            {   // sacks, INT, INT yds, INT TD
                { "RE",   new int[] { 12, 1, 10, 0 } }, { "NT",   new int[] { 5, 0,  0, 0 } }, { "LE",  new int[] { 2, 0, 0, 0 } },
                { "RE2",  new int[] {  0, 0,  0, 0 } }, { "NT2",  new int[] { 0, 0,  0, 0 } }, { "LE2", new int[] { 5, 0, 0, 0 } },
                { "ROLB", new int[] {  9, 0,  0, 0 } }, { "RILB", new int[] { 3, 1, 10, 0 } }, { "LILB", new int[] { 3, 0, 0, 0 } },
                { "LOLB", new int[] {  1, 7, 67, 0 } }, { "LB5",  new int[] { 0, 0,  0, 0 } },
                { "RCB",  new int[] {  0, 1, 10, 0 } }, { "LCB",  new int[] { 0, 0,  0, 0 } }, { "DB1", new int[] { 0, 0, 0, 0 } },
                { "DB2",  new int[] {  1, 0,  0, 0 } }, { "FS",   new int[] { 0, 2,  8, 0 } }, { "SS",  new int[] { 3, 3, 12, 0 } },
                { "DB3",  new int[] {  0, 0,  0, 0 } },
            };
            foreach (KeyValuePair<string, int[]> row in sheet)
            {
                PlayerSeasonStats p = bills.GetPlayer(row.Key);
                Assert.IsNotNull(p, row.Key);
                CollectionAssert.AreEqual(row.Value, new int[] { p.Sacks, p.Interceptions, p.InterceptionYards, p.InterceptionTDs }, row.Key);
            }
        }

        [TestMethod]
        public void Bills_KickerAndPunter()
        {
            TeamSeasonStats bills = Load(Season1994Sram).GetTeam("bills");
            PlayerSeasonStats k = bills.GetPlayer("K");
            PlayerSeasonStats p = bills.GetPlayer("P");

            CollectionAssert.AreEqual(new int[] { 44, 46, 13, 17 },
                new int[] { k.ExtraPointsMade, k.ExtraPointAttempts, k.FieldGoalsMade, k.FieldGoalAttempts });
            CollectionAssert.AreEqual(new int[] { 19, 751 }, new int[] { p.Punts, p.PuntYards });
        }

        [TestMethod]
        public void League_PassingBalances()
        {
            Tsb2SeasonStats stats = Load(Season1994Sram);
            int comp = 0, passYards = 0, intsThrown = 0, intsMade = 0;
            foreach (TeamSeasonStats t in stats.Teams)
            {
                comp += t.Completions;
                passYards += t.PassYards;
                intsMade += t.InterceptionsMade;
                foreach (PlayerSeasonStats p in t.Players)
                    intsThrown += p.InterceptionsThrown;
            }
            Assert.AreEqual(3479, comp);
            Assert.AreEqual(77272, passYards);
            // the game loses an INT credit when it lands on an injured defender (TSB2_Sim_Engine.txt, bug G1)
            Assert.AreEqual(441, intsThrown);
            Assert.AreEqual(421, intsMade);
        }

        [TestMethod]
        public void FromSram_RushYardsShortOfAllowed_StillReads()
        {
            Tsb2SeasonStats stats = Load(StockRun2Sram);
            int rushYards = 0, rushAllowed = 0;
            foreach (TeamSeasonStats t in stats.Teams)
            {
                rushYards += t.RushYards;
                rushAllowed += t.RushYardsAllowed;
            }
            Assert.AreEqual(47792 - 18, rushYards);
            Assert.AreEqual(47792, rushAllowed);
        }

        [TestMethod]
        public void FromSram_EmptySave_ReturnsNullWithReason()
        {
            string error;
            Assert.IsNull(Tsb2SeasonStats.FromSram(new byte[0x8000], out error));
            StringAssert.Contains(error, "TSB2");
            Assert.IsNull(SeasonStats.FromFile(new byte[0x8000], out error));
            StringAssert.Contains(error, "TSB2");
        }

        [TestMethod]
        public void IsRomForTheseStats_OnlyTsb2()
        {
            Tsb2SeasonStats stats = Load(Season1994Sram);

            Assert.IsTrue(stats.IsRomForTheseStats(LoadTool(RomFileName)));
            Assert.IsFalse(stats.IsRomForTheseStats(LoadTool("TSB3.smc")));
            Assert.IsFalse(stats.IsRomForTheseStats(LoadTool("TSB1.smc")));
            Assert.IsFalse(stats.IsRomForTheseStats(null));
        }

        [TestMethod]
        public void ToText_WithRom_UsesThePlayedSeasonsNames()
        {
            string text = Load(Season1994Sram).ToText(LoadTool(RomFileName));

            StringAssert.StartsWith(text, "SNES TSB2 SEASON STATS (1994)");
            StringAssert.Contains(text, "jim KELLY");
            StringAssert.Contains(text, "darryl TALLEY");
            StringAssert.Contains(text, "russell COPELAND"); // 1994 WR4 (1992's is steve TASKER)
            Assert.IsFalse(text.Contains("steve TASKER"));
            Assert.IsFalse(text.Contains("QB1 Bills"), "ROM names should replace the generic ones.");
        }

        [TestMethod]
        public void ToText_WithoutRom_UsesGenericNames()
        {
            string text = Load(Season1994Sram).ToText(null);

            StringAssert.Contains(text, "QB1 Bills");
            StringAssert.Contains(text, "QB1 49ers");
        }

        [TestMethod]
        public void ToCsv_WithRom_BillsRows()
        {
            List<string> lines = CsvLines(Load(Season1994Sram).ToCsv(LoadTool(RomFileName)));

            CollectionAssert.Contains(lines, "bills,QB1,QB,\"jim KELLY\",208,132,63.5,2943,14.1,25,14,21,167,8.0,4,,,,,,,,,,,,,,,,,,,,,,,");
            // returns on the returner's own row
            CollectionAssert.Contains(lines, "bills,WR4,WR,\"russell COPELAND\",,,,,,,,0,0,,0,7,179,25.6,1,,,,,,,,,,,,64,1012,15.8,0,15,115,7.7,0");
            // the extra TSB2 defenders are grouped like the others
            CollectionAssert.Contains(lines, "bills,LE2,DL,\"oliver BARNETT\",,,,,,,,,,,,,,,,5,0,0,0,,,,,,,,,,,,,,,");
            CollectionAssert.Contains(lines, "bills,SS,DB,\"henry JONES\",,,,,,,,,,,,,,,,3,3,12,0,,,,,,,,,,,,,,,");
        }
    }
}
