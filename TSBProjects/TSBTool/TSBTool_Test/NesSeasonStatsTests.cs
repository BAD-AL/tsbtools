using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// NesSeasonStats: reads the season stats out of the NES TSB battery save (.sav, 8 KB, the same size
    /// as a SNES .srm). Layout from bruddog's disassembly of the game (see NesSeasonStats). The fixtures
    /// are full seasons played in Mesen2 on the three ROMs in ROMs\ (stock 28-team TSPRBOWL.nes, and the
    /// 32-team 'TSB 2007-32-105' and Sblueman ROMs). Expected values were checked by how they add up
    /// (points, TDs, yards, INTs across the league), not yet against the game's screens.
    /// </summary>
    [TestClass]
    public class NesSeasonStatsTests
    {
        private const string OriginalRom = "TSPRBOWL.nes";
        private const string Cxrom2007Rom = "TSB 2007-32-105.nes";
        private const string OriginalSav = @"NES\SaveRam\TSPRBOWL.sav";
        private const string Cxrom2007Sav = @"NES\SaveRam\TSB 2007-32-105.sav";
        private const string SbluemanSav = @"NES\SaveRam\SbluemanBase_18week_RealNFL-1.sav";
        private const string Snes05Sram = @"SNES\SaveRam\SNES_05_TeamDefense.srm";
        private static readonly string[] AllSavs = { OriginalSav, Cxrom2007Sav, SbluemanSav };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static NesSeasonStats Load(string sav)
        {
            string error;
            NesSeasonStats stats = NesSeasonStats.FromSav(TestRoms.LoadRom(sav), out error);
            Assert.IsNotNull(stats, "Load failed: " + error);
            return stats;
        }

        private static ITecmoContent LoadTool(string rom)
        {
            return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom));
        }

        // ---- Stock 28-team game ----

        [TestMethod]
        public void Original_Standings_Bills()
        {
            TeamSeasonStats bills = Load(OriginalSav).GetTeam("bills");

            CollectionAssert.AreEqual(new int[] { 11, 5, 0, 396, 319, 2752, 1451 },
                new int[] { bills.Wins, bills.Losses, bills.Ties, bills.PointsFor, bills.PointsAgainst, bills.PassYardsAllowed, bills.RushYardsAllowed });
        }

        [TestMethod]
        public void Original_TeamsAreInTheNesRomOrder()
        {
            NesSeasonStats stats = Load(OriginalSav);

            Assert.IsFalse(stats.Is32Team);
            Assert.AreEqual(28, stats.Teams.Count);
            Assert.AreEqual("bills", stats.Teams[0].Team);
            Assert.AreEqual("falcons", stats.Teams[27].Team);
            // the broncos' block is the first one after the game's 260 unused bytes
            Assert.AreEqual(320, stats.GetTeam("broncos").PointsAgainst);
            Assert.AreEqual(440, stats.GetTeam("falcons").PointsAgainst);
        }

        [TestMethod]
        public void Original_BillsPlayers()
        {
            TeamSeasonStats bills = Load(OriginalSav).GetTeam("bills");

            PlayerSeasonStats qb = bills.GetPlayer("QB1");
            CollectionAssert.AreEqual(new int[] { 175, 113, 2806, 33, 15 },
                new int[] { qb.PassAttempts, qb.Completions, qb.PassYards, qb.PassTDs, qb.InterceptionsThrown });
            PlayerSeasonStats rb = bills.GetPlayer("RB1");
            CollectionAssert.AreEqual(new int[] { 162, 1878, 14, 27, 624, 9 },
                new int[] { rb.RushAttempts, rb.RushYards, rb.RushTDs, rb.Receptions, rb.ReceivingYards, rb.ReceivingTDs });
            PlayerSeasonStats re = bills.GetPlayer("RE");
            Assert.AreEqual(17, re.Sacks);
            PlayerSeasonStats lcb = bills.GetPlayer("LCB");
            CollectionAssert.AreEqual(new int[] { 0, 5, 41, 0 },
                new int[] { lcb.Sacks, lcb.Interceptions, lcb.InterceptionYards, lcb.InterceptionTDs });
            Assert.IsNull(bills.GetPlayer("DB1"), "NES has 11 defenders");
            PlayerSeasonStats k = bills.GetPlayer("K");
            CollectionAssert.AreEqual(new int[] { 51, 54, 7, 8 },
                new int[] { k.ExtraPointsMade, k.ExtraPointAttempts, k.FieldGoalsMade, k.FieldGoalAttempts });
            PlayerSeasonStats p = bills.GetPlayer("P");
            CollectionAssert.AreEqual(new int[] { 11, 437 }, new int[] { p.Punts, p.PuntYards });
        }

        // NES keeps kick/punt returns per RB/WR/TE (no return-team slots): the returners are those players.
        [TestMethod]
        public void Original_Returns_AreKeptPerPlayer()
        {
            TeamSeasonStats bills = Load(OriginalSav).GetTeam("bills");

            PlayerSeasonStats rb4 = bills.Returners.Find(r => r.Position == "RB4");
            CollectionAssert.AreEqual(new int[] { 64, 1049, 0, 0, 0, 0 },
                new int[] { rb4.KickReturns, rb4.KickReturnYards, rb4.KickReturnTDs, rb4.PuntReturns, rb4.PuntReturnYards, rb4.PuntReturnTDs });
            PlayerSeasonStats wr4 = bills.Returners.Find(r => r.Position == "WR4");
            CollectionAssert.AreEqual(new int[] { 0, 18, 159 }, new int[] { wr4.KickReturns, wr4.PuntReturns, wr4.PuntReturnYards });
        }

        [TestMethod]
        public void Original_QBRushing()
        {
            PlayerSeasonStats qb = Load(OriginalSav).GetTeam("falcons").GetPlayer("QB1");
            CollectionAssert.AreEqual(new int[] { 49, 201, 7 }, new int[] { qb.RushAttempts, qb.RushYards, qb.RushTDs });
        }

        // ---- 32-team ROMs ----

        [TestMethod]
        public void Cxrom2007_Has32Teams_TheExpansionTeamsComeFromTheirOwnBlocks()
        {
            NesSeasonStats stats = Load(Cxrom2007Sav);

            Assert.IsTrue(stats.Is32Team);
            Assert.AreEqual(32, stats.Teams.Count);
            Assert.AreEqual("ravens", stats.Teams[6].Team);
            CollectionAssert.AreEqual(new string[] { "49ers", "rams", "seahawks", "cardinals" },
                new string[] { stats.Teams[28].Team, stats.Teams[29].Team, stats.Teams[30].Team, stats.Teams[31].Team });
            TeamSeasonStats niners = stats.GetTeam("49ers");
            CollectionAssert.AreEqual(new int[] { 2, 14, 0, 235, 400 },
                new int[] { niners.Wins, niners.Losses, niners.Ties, niners.PointsFor, niners.PointsAgainst });
            TeamSeasonStats cardinals = stats.GetTeam("cardinals");
            CollectionAssert.AreEqual(new int[] { 6, 10, 0, 318, 367 },
                new int[] { cardinals.Wins, cardinals.Losses, cardinals.Ties, cardinals.PointsFor, cardinals.PointsAgainst });
        }

        [TestMethod]
        public void Cxrom2007_49ersReturner()
        {
            PlayerSeasonStats rb3 = Load(Cxrom2007Sav).GetTeam("49ers").Returners.Find(r => r.Position == "RB3");
            CollectionAssert.AreEqual(new int[] { 79, 1477, 4, 42 },
                new int[] { rb3.KickReturns, rb3.KickReturnYards, rb3.PuntReturns, rb3.PuntReturnYards });
            PlayerSeasonStats player = Load(Cxrom2007Sav).GetTeam("49ers").GetPlayer("RB3");
            CollectionAssert.AreEqual(new int[] { 9, 57, 1, 9 },
                new int[] { player.RushAttempts, player.RushYards, player.Receptions, player.ReceivingYards });
        }

        [TestMethod]
        public void Sblueman_RamsPlayers_WithAnInterceptionTD()
        {
            TeamSeasonStats rams = Load(SbluemanSav).GetTeam("rams");

            CollectionAssert.AreEqual(new int[] { 10, 7, 0, 458, 403 },
                new int[] { rams.Wins, rams.Losses, rams.Ties, rams.PointsFor, rams.PointsAgainst });
            PlayerSeasonStats wr1 = rams.GetPlayer("WR1");
            CollectionAssert.AreEqual(new int[] { 76, 1881, 19 }, new int[] { wr1.Receptions, wr1.ReceivingYards, wr1.ReceivingTDs });
            PlayerSeasonStats rilb = rams.GetPlayer("RILB");
            CollectionAssert.AreEqual(new int[] { 2, 3, 73, 1 },
                new int[] { rilb.Sacks, rilb.Interceptions, rilb.InterceptionYards, rilb.InterceptionTDs });
        }

        // ---- Checks across the league ----

        [TestMethod]
        public void LeagueTotals_Balance()
        {
            foreach (string sav in AllSavs)
            {
                NesSeasonStats stats = Load(sav);
                int passTDs = 0, receivingTDs = 0, completions = 0, receptions = 0, intsThrown = 0, intsMade = 0;
                int passYards = 0, passAllowed = 0, wins = 0, losses = 0, pointsFor = 0, pointsAgainst = 0;
                foreach (TeamSeasonStats t in stats.Teams)
                {
                    foreach (PlayerSeasonStats p in t.Players)
                    {
                        passTDs += p.PassTDs; receivingTDs += p.ReceivingTDs;
                        completions += p.Completions; receptions += p.Receptions;
                        intsThrown += p.InterceptionsThrown; intsMade += p.Interceptions;
                    }
                    passYards += t.PassYards; passAllowed += t.PassYardsAllowed;
                    wins += t.Wins; losses += t.Losses;
                    pointsFor += t.PointsFor; pointsAgainst += t.PointsAgainst;
                }
                Assert.AreEqual(passTDs, receivingTDs, sav + " TDs");
                Assert.AreEqual(completions, receptions, sav + " catches");
                Assert.AreEqual(intsThrown, intsMade, sav + " INTs");
                Assert.AreEqual(passYards, passAllowed, sav + " pass yds");
                Assert.AreEqual(wins, losses, sav + " W/L");
                Assert.AreEqual(pointsFor, pointsAgainst, sav + " points");
            }
        }

        // Every team's points: 6 per TD (passing, rushing, INT and kick/punt returns) + XP + 3 per FG.
        // Safeties aren't stored, so a team can be a few points short, always by 2s.
        [TestMethod]
        public void PointsFor_AddUpFromTheTeamsTDsAndKicks()
        {
            foreach (string sav in AllSavs)
            {
                int exact = 0;
                NesSeasonStats stats = Load(sav);
                foreach (TeamSeasonStats t in stats.Teams)
                {
                    int tds = 0;
                    foreach (PlayerSeasonStats p in t.Players)
                        tds += p.PassTDs + p.RushTDs + p.InterceptionTDs;
                    foreach (PlayerSeasonStats r in t.Returners)
                        tds += r.KickReturnTDs + r.PuntReturnTDs;
                    PlayerSeasonStats k = t.GetPlayer("K");
                    int missing = t.PointsFor - (6 * tds + k.ExtraPointsMade + 3 * k.FieldGoalsMade);
                    Assert.IsTrue(missing >= 0 && missing <= 6 && missing % 2 == 0, sav + " " + t.Team + " missing " + missing);
                    if (missing == 0)
                        exact++;
                }
                Assert.IsTrue(exact >= stats.Teams.Count - 8, sav + ": only " + exact + " teams add up exactly");
            }
        }

        // ---- Rejected files ----

        [TestMethod]
        public void EmptySav_IsRejected_WithAHint()
        {
            string error;
            Assert.IsNull(NesSeasonStats.FromSav(new byte[0x2000], out error));
            StringAssert.Contains(error, "no NES TSB season stats");
        }

        [TestMethod]
        public void WrongSize_IsRejected()
        {
            string error;
            Assert.IsNull(NesSeasonStats.FromSav(new byte[300000], out error));
            StringAssert.Contains(error, ".sav");
            Assert.IsNull(NesSeasonStats.FromSav(null, out error));
            StringAssert.Contains(error, ".sav");
        }

        // A .sav and a .srm are both 8 KB: each is only read as the game it came from.
        [TestMethod]
        public void NesAndSnesSaves_AreNotMixedUp()
        {
            string error;
            Assert.IsNull(NesSeasonStats.FromSav(TestRoms.LoadRom(Snes05Sram), out error), "SNES .srm read as NES");
            foreach (string sav in AllSavs)
                Assert.IsNull(SnesSeasonStats.FromSram(TestRoms.LoadRom(sav), out error), sav + " read as SNES");
        }

        [TestMethod]
        public void FromFile_PicksNesForASav_AndStillSnesForAnSrm()
        {
            string error;
            foreach (string sav in AllSavs)
                Assert.IsInstanceOfType(SeasonStats.FromFile(TestRoms.LoadRom(sav), out error), typeof(NesSeasonStats), sav + ": " + error);
            Assert.IsInstanceOfType(SeasonStats.FromFile(TestRoms.LoadRom(Snes05Sram), out error), typeof(SnesSeasonStats), error);
        }

        [TestMethod]
        public void FromFile_EmptyBatterySave_SaysItHasNoSnesOrNesStats()
        {
            string error;
            Assert.IsNull(SeasonStats.FromFile(new byte[0x2000], out error));
            StringAssert.Contains(error, "SNES");
            StringAssert.Contains(error, "NES TSB");
        }

        [TestMethod]
        public void FromFile_SomethingElse_IsRejected_NamingTheNesSav()
        {
            string error;
            Assert.IsNull(SeasonStats.FromFile(new byte[1000], out error));
            StringAssert.Contains(error, ".sav");
        }

        // ---- Which ROM goes with the stats ----

        [TestMethod]
        public void IsRomForTheseStats_28And32TeamRomsAreKeptApart()
        {
            NesSeasonStats original = Load(OriginalSav);
            NesSeasonStats cxrom = Load(Cxrom2007Sav);

            Assert.IsTrue(original.IsRomForTheseStats(LoadTool(OriginalRom)));
            Assert.IsFalse(original.IsRomForTheseStats(LoadTool(Cxrom2007Rom)));
            Assert.IsTrue(cxrom.IsRomForTheseStats(LoadTool(Cxrom2007Rom)));
            Assert.IsFalse(cxrom.IsRomForTheseStats(LoadTool(OriginalRom)));
            Assert.IsFalse(original.IsRomForTheseStats(LoadTool("TSB1.smc")));
            Assert.IsFalse(original.IsRomForTheseStats(null));
            Assert.AreEqual("NES", original.RomKind);
            Assert.AreEqual("32-team NES", cxrom.RomKind);
        }

        // ---- Output (shared with Genesis/SNES) ----

        [TestMethod]
        public void ToText_WithRom_UsesRomNames()
        {
            string text = Load(OriginalSav).ToText(LoadTool(OriginalRom));

            StringAssert.StartsWith(text, "NES TSB SEASON STATS");
            StringAssert.Contains(text, "qb BILLS");
            StringAssert.Contains(text, "thurman THOMAS");
            Assert.IsFalse(text.Contains("QB1 Bills"), "ROM names should replace the generic ones.");
        }

        [TestMethod]
        public void ToText_ShowsTheReturnersByName()
        {
            string text = Load(OriginalSav).ToText(LoadTool(OriginalRom));

            StringAssert.Contains(text, "RETURNS");
            StringAssert.Matches(text, new Regex(@"RB4\s+don SMITH\s+64\s+1049\s+16\.4\s+0"));
            StringAssert.Matches(text, new Regex(@"WR4\s+al EDWARDS\s+0\s+0\s+-\s+0\s+18\s+159"));
        }

        [TestMethod]
        public void ToText_32TeamRom_NamesTheExpansionTeams()
        {
            string text = Load(Cxrom2007Sav).ToText(LoadTool(Cxrom2007Rom));

            StringAssert.Contains(text, "trent EDWARDS");
            StringAssert.Contains(text, "alex SMITH");   // 49ers QB1, one of the 4 expansion teams
            StringAssert.Contains(text, "==== RAVENS");
        }

        [TestMethod]
        public void ToText_WithoutRom_UsesGenericNames()
        {
            string text = Load(Cxrom2007Sav).ToText(null);

            StringAssert.Contains(text, "QB1 Bills");
            StringAssert.Contains(text, "QB1 Cardinals");
        }

        [TestMethod]
        public void ToText_WithTheWrongKindOfRom_UsesGenericNames()
        {
            StringAssert.Contains(Load(OriginalSav).ToText(LoadTool(Cxrom2007Rom)), "QB1 Bills");
            StringAssert.Contains(Load(Cxrom2007Sav).ToText(LoadTool(OriginalRom)), "QB1 Bills");
            StringAssert.Contains(Load(OriginalSav).ToText(LoadTool("TSB1.smc")), "QB1 Bills");
        }

        [TestMethod]
        public void ToCsv_WithRom_ReturnsGoOnThePlayersOwnRow()
        {
            string csv = Load(OriginalSav).ToCsv(LoadTool(OriginalRom));

            StringAssert.Contains(csv, "bills,QB1,QB,\"qb BILLS\",175,113,64.6,2806,16.0,33,15,");
            StringAssert.Contains(csv, "bills,K,K,\"scott NORWOOD\",");
            StringAssert.Contains(csv, "bills,RB4,RB,\"don SMITH\",");
            StringAssert.Contains(csv, ",64,1049,16.4,0,0,0,,0\n");
            Assert.AreEqual(1, Regex.Matches(csv, "bills,RB4,").Count, "one row per player");
        }

        [TestMethod]
        public void ToCsv_WithoutRom_ReturnsStillGoOnThePlayersOwnRow()
        {
            string csv = Load(OriginalSav).ToCsv(null);

            StringAssert.Contains(csv, "bills,RB4,RB,\"RB4 Bills\",");
            Assert.AreEqual(1, Regex.Matches(csv, "bills,RB4,").Count, "one row per player");
            Assert.IsFalse(csv.Contains(",RET"), "NES has no return-team slots");
        }
    }
}
