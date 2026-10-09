using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// GenesisSeasonStats: reads the season stats out of an ares save state (.bs1) of Genesis TSB, saved
    /// at the 'Season Menu' screen. The expected values come from the game's own stat screens for the
    /// same seasons (SimTests GEN_05b / GEN_06b: leaders screens, team screens, the result sheets).
    /// </summary>
    [TestClass]
    public class GenesisSeasonStatsTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const string Gen06bState = @"Genesis\SaveStates\GEN_06b_SeasonMenu.bs1";
        private const string Gen05bState = @"Genesis\SaveStates\GEN_05b_SeasonMenu.bs1";
        private const string NoStatsState = @"Genesis\SaveStates\GEN_05b_NoSeasonStats.bs1";
        // A season simmed with the GENESIS_FIX_SIM_BUGS receiving-TD fix (SimTests GEN_08b), so the game
        // recorded receiving TDs correctly (stock Genesis loses most of them, see GenesisSimBugFixTests).
        private const string Gen08bState = @"Genesis\SaveStates\GEN_08b_SeasonMenu.bs1";

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static GenesisSeasonStats Load(string state)
        {
            string error;
            GenesisSeasonStats stats = GenesisSeasonStats.FromAresState(TestRoms.LoadRom(state), out error);
            Assert.IsNotNull(stats, "Load failed: " + error);
            return stats;
        }

        [TestMethod]
        public void Gen06b_Standings_Bills()
        {
            TeamSeasonStats bills = Load(Gen06bState).GetTeam("bills");

            Assert.AreEqual(10, bills.Wins);
            Assert.AreEqual(6, bills.Losses);
            Assert.AreEqual(0, bills.Ties);
            Assert.AreEqual(370, bills.PointsFor);
            Assert.AreEqual(327, bills.PointsAgainst);
            Assert.AreEqual(2352, bills.PassYardsAllowed);
            Assert.AreEqual(2042, bills.RushYardsAllowed);
        }

        [TestMethod]
        public void Gen06b_Passing_MatchesTheLeadersScreen()
        {
            GenesisSeasonStats stats = Load(Gen06bState);

            PlayerSeasonStats kelly = stats.GetTeam("bills").GetPlayer("QB1");
            Assert.AreEqual(201, kelly.PassAttempts);
            Assert.AreEqual(155, kelly.Completions);
            Assert.AreEqual(2312, kelly.PassYards);
            Assert.AreEqual(24, kelly.PassTDs);
            Assert.AreEqual(4, kelly.InterceptionsThrown);

            PlayerSeasonStats moon = stats.GetTeam("oilers").GetPlayer("QB1"); // > 255 attempts
            Assert.AreEqual(260, moon.PassAttempts);
            Assert.AreEqual(205, moon.Completions);
            Assert.AreEqual(3048, moon.PassYards);
            Assert.AreEqual(28, moon.PassTDs);
            Assert.AreEqual(7, moon.InterceptionsThrown);
        }

        [TestMethod]
        public void Gen06b_QBRushing_MatchesQBStatsCsv()
        {
            GenesisSeasonStats stats = Load(Gen06bState);

            PlayerSeasonStats elway = stats.GetTeam("broncos").GetPlayer("QB1");
            Assert.AreEqual(78, elway.RushAttempts);
            Assert.AreEqual(562, elway.RushYards);
            Assert.AreEqual(5, elway.RushTDs);

            PlayerSeasonStats peete = stats.GetTeam("lions").GetPlayer("QB1");
            Assert.AreEqual(39, peete.RushAttempts);
            Assert.AreEqual(240, peete.RushYards);
            Assert.AreEqual(3, peete.RushTDs);
        }

        [TestMethod]
        public void Gen06b_QBRushYards_CanBeNegative()
        {
            // jets QB1: 2 rushes, -3 yards
            PlayerSeasonStats jetsQB = Load(Gen06bState).GetTeam("jets").GetPlayer("QB1");
            Assert.AreEqual(2, jetsQB.RushAttempts);
            Assert.AreEqual(-3, jetsQB.RushYards);
        }

        [TestMethod]
        public void Gen06b_BillsRunningBacks_MatchBillsRBStatsCsv()
        {
            TeamSeasonStats bills = Load(Gen06bState).GetTeam("bills");
            // position, rush, rush yds, rush td, rec, rec yds, rec td
            int[][] expected =
            {
                new int[] { 174, 1694, 18, 45, 604, 0 }, // RB1 Thurman Thomas
                new int[] {  64,  583,  6, 44, 764, 1 }, // RB2 Kenneth Davis
                new int[] {  14,  124,  2,  3,  45, 0 }, // RB3 Carwell Gardner
                new int[] {   0,    0,  0,  0,   0, 0 }, // RB4 Eddie Fuller
            };
            for (int i = 0; i < expected.Length; i++)
            {
                PlayerSeasonStats p = bills.GetPlayer("RB" + (i + 1));
                CollectionAssert.AreEqual(expected[i],
                    new int[] { p.RushAttempts, p.RushYards, p.RushTDs, p.Receptions, p.ReceivingYards, p.ReceivingTDs },
                    "RB" + (i + 1));
            }
        }

        [TestMethod]
        public void Gen06b_ReceiverRushing_IsCounted()
        {
            // vikings WR1 Cris Carter: 32 rushes, 248 yds, 5 TD; 45 rec, 477 yds, 1 TD (Rushing.csv)
            PlayerSeasonStats carter = Load(Gen06bState).GetTeam("vikings").GetPlayer("WR1");
            CollectionAssert.AreEqual(new int[] { 32, 248, 5, 45, 477, 1 },
                new int[] { carter.RushAttempts, carter.RushYards, carter.RushTDs, carter.Receptions, carter.ReceivingYards, carter.ReceivingTDs });
        }

        [TestMethod]
        public void Gen06b_Defense_MatchesTheSackLeadersScreen()
        {
            GenesisSeasonStats stats = Load(Gen06bState);

            Assert.AreEqual(10, stats.GetTeam("giants").GetPlayer("NT").Sacks);    // Erik Howard
            Assert.AreEqual(10, stats.GetTeam("patriots").GetPlayer("NT").Sacks);  // Tim Goad
            Assert.AreEqual(8, stats.GetTeam("patriots").GetPlayer("RILB").Sacks); // Vincent Brown
            Assert.AreEqual(8, stats.GetTeam("buccaneers").GetPlayer("ROLB").Sacks); // Jimmy Williams
            Assert.AreEqual(7, stats.GetTeam("buccaneers").GetPlayer("LILB").Sacks); // Hardy Nickerson
        }

        [TestMethod]
        public void Gen06b_KickingAndPunting_MatchTheBillsScreen()
        {
            TeamSeasonStats bills = Load(Gen06bState).GetTeam("bills");

            PlayerSeasonStats k = bills.GetPlayer("K");
            Assert.AreEqual(46, k.ExtraPointsMade);
            Assert.AreEqual(50, k.ExtraPointAttempts);
            Assert.AreEqual(8, k.FieldGoalsMade);
            Assert.AreEqual(10, k.FieldGoalAttempts);

            PlayerSeasonStats p = bills.GetPlayer("P");
            Assert.AreEqual(13, p.Punts);
            Assert.AreEqual(621, p.PuntYards);
        }

        [TestMethod]
        public void Gen06b_TeamRushAndPassAttempts_MatchTheResultSheet()
        {
            GenesisSeasonStats stats = Load(Gen06bState);
            // GEN_06b_RunPassPrefFixed_results.txt (after the re-reads): team, rush att, pass att
            Dictionary<string, int[]> sheet = new Dictionary<string, int[]>
            {
                { "bills",    new int[] { 256, 201 } }, { "jets",       new int[] { 248, 201 } },
                { "steelers", new int[] { 220, 207 } }, { "chargers",   new int[] { 259, 171 } },
                { "eagles",   new int[] { 250, 197 } }, { "lions",      new int[] { 250, 156 } },
                { "falcons",  new int[] { 259, 180 } }, { "colts",      new int[] { 261, 157 } },
                { "bengals",  new int[] { 273, 167 } }, { "broncos",    new int[] { 269, 163 } },
                { "seahawks", new int[] { 258, 144 } }, { "cardinals",  new int[] { 275, 162 } },
                { "packers",  new int[] { 271, 156 } }, { "rams",       new int[] { 282, 147 } },
                { "dolphins", new int[] { 223, 216 } }, { "browns",     new int[] { 201, 211 } },
                { "chiefs",   new int[] { 204, 216 } }, { "cowboys",    new int[] { 218, 200 } },
                { "redskins", new int[] { 223, 232 } }, { "vikings",    new int[] { 217, 238 } },
                { "saints",   new int[] { 213, 221 } }, { "patriots",   new int[] { 184, 231 } },
                { "oilers",   new int[] { 215, 260 } }, { "raiders",    new int[] { 170, 228 } },
                { "giants",   new int[] { 190, 234 } }, { "bears",      new int[] { 181, 233 } },
                { "buccaneers", new int[] { 179, 232 } }, { "49ers",    new int[] { 200, 252 } },
            };
            Assert.AreEqual(28, stats.Teams.Count);
            foreach (KeyValuePair<string, int[]> kv in sheet)
            {
                TeamSeasonStats t = stats.GetTeam(kv.Key);
                Assert.AreEqual(kv.Value[0], t.RushAttempts, kv.Key + " rush att");
                Assert.AreEqual(kv.Value[1], t.PassAttempts, kv.Key + " pass att");
            }
        }

        [TestMethod]
        public void Gen05b_TeamDefense_MatchesTheResultSheet()
        {
            GenesisSeasonStats stats = Load(Gen05bState);
            // GEN_05b_TeamDefense_results.txt: rush yds allowed, pass yds allowed, pts allowed, sacks, INTs
            Dictionary<string, int[]> sheet = new Dictionary<string, int[]>
            {
                { "bills",    new int[] { 2339, 2361, 372, 27,  6 } },
                { "steelers", new int[] { 2422, 2375, 414, 22, 11 } },
                { "colts",    new int[] { 2462, 2187, 343, 22,  9 } },
                { "raiders",  new int[] { 1907, 2198, 360, 31, 16 } },
                { "browns",   new int[] { 2502, 1772, 344, 30, 11 } },
                { "49ers",    new int[] { 2141, 1998, 375, 23,  4 } },
            };
            foreach (KeyValuePair<string, int[]> kv in sheet)
            {
                TeamSeasonStats t = stats.GetTeam(kv.Key);
                CollectionAssert.AreEqual(kv.Value,
                    new int[] { t.RushYardsAllowed, t.PassYardsAllowed, t.PointsAgainst, t.Sacks, t.InterceptionsMade },
                    kv.Key);
            }
        }

        [TestMethod]
        public void Gen08b_ReceivingTDs_AddUpToPassingTDs_ForEveryTeam()
        {
            GenesisSeasonStats stats = Load(Gen08bState);
            Assert.AreEqual(28, stats.Teams.Count);
            int league = 0;
            foreach (TeamSeasonStats t in stats.Teams)
            {
                int passTDs = 0, receivingTDs = 0, completions = 0, receptions = 0;
                foreach (PlayerSeasonStats p in t.Players)
                {
                    passTDs += p.PassTDs;
                    receivingTDs += p.ReceivingTDs;
                    completions += p.Completions;
                    receptions += p.Receptions;
                }
                Assert.AreEqual(passTDs, receivingTDs, t.Team + " TDs");
                Assert.AreEqual(completions, receptions, t.Team + " catches");
                league += receivingTDs;
            }
            Assert.AreEqual(590, league);
        }

        [TestMethod]
        public void Gen08b_Receivers_RecTDsYardsCatches()
        {
            GenesisSeasonStats stats = Load(Gen08bState);
            // {team, position, receptions, yards, TDs}
            object[][] expected =
            {
                new object[] { "dolphins", "TE1", 53,  909, 14 },
                new object[] { "falcons",  "WR2", 62,  869, 13 },
                new object[] { "oilers",   "WR1", 58, 1004, 11 },
                new object[] { "49ers",    "WR2", 82, 1316,  7 },
            };
            foreach (object[] e in expected)
            {
                PlayerSeasonStats p = stats.GetTeam((string)e[0]).GetPlayer((string)e[1]);
                CollectionAssert.AreEqual(new int[] { (int)e[2], (int)e[3], (int)e[4] },
                    new int[] { p.Receptions, p.ReceivingYards, p.ReceivingTDs }, e[0] + " " + e[1]);
            }
        }

        [TestMethod]
        public void StateWithoutSeasonStats_IsRejected_WithAHint()
        {
            string error;
            GenesisSeasonStats stats = GenesisSeasonStats.FromAresState(TestRoms.LoadRom(NoStatsState), out error);

            Assert.IsNull(stats);
            StringAssert.Contains(error, "Season Menu");
        }

        [TestMethod]
        public void NotAnAresState_IsRejected()
        {
            string error;
            Assert.IsNull(GenesisSeasonStats.FromAresState(new byte[300000], out error));
            StringAssert.Contains(error, "ares");

            Assert.IsNull(GenesisSeasonStats.FromAresState(new byte[] { (byte)'B', (byte)'S', (byte)'T', (byte)'1' }, out error));
            StringAssert.Contains(error, "ares");
        }

        [TestMethod]
        public void ToText_WithRom_UsesRomNames()
        {
            GenesisSeasonStats stats = Load(Gen06bState);
            Genesis_TSB1Tool rom = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            string text = stats.ToText(rom);

            StringAssert.Contains(text, "jim KELLY");
            StringAssert.Contains(text, "thurman THOMAS");
            Assert.IsFalse(text.Contains("QB1 Bills"), "ROM names should replace the generic ones.");
        }

        [TestMethod]
        public void ToText_WithoutRom_UsesGenericNames()
        {
            string text = Load(Gen06bState).ToText(null);

            StringAssert.Contains(text, "QB1 Bills");
            StringAssert.Contains(text, "RB1 Bills");
            StringAssert.Contains(text, "NT Giants");
            StringAssert.Contains(text, "QB1 49ers");
        }

        // ---- CSV: one row per player with stats, every row the same columns (blank where n/a) ----

        private const string CsvHeader =
            "Team,Position,Group,Name,PassAtt,Comp,Comp%,PassYds,PassAvg,PassTD,INTThrown," +
            "RushAtt,RushYds,RushAvg,RushTD,Rec,RecYds,RecAvg,RecTD,Sacks,INT,INTYds,INTTD," +
            "XP,XPA,FG,FGA,Punts,PuntYds,PuntAvg,KR,KRYds,KRAvg,KRTD,PR,PRYds,PRAvg,PRTD";

        // A CSV row: the 4 id columns, then the 34 stat columns given as (first column index, values).
        private static string Row(string team, string pos, string group, string name, int start, params string[] values)
        {
            string[] cells = new string[38];
            cells[0] = team; cells[1] = pos; cells[2] = group; cells[3] = "\"" + name + "\"";
            for (int i = 0; i < values.Length; i++)
                cells[start + i] = values[i];
            return string.Join(",", cells);
        }

        private static List<string> CsvLines(string csv)
        {
            return new List<string>(csv.Replace("\r\n", "\n").TrimEnd('\n').Split('\n'));
        }

        [TestMethod]
        public void ToCsv_HeaderAndBillsRows_WithRomNames()
        {
            GenesisSeasonStats stats = Load(Gen06bState);
            Genesis_TSB1Tool rom = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            List<string> lines = CsvLines(stats.ToCsv(rom));

            Assert.AreEqual(CsvHeader, lines[0]);
            CollectionAssert.Contains(lines, Row("bills", "QB1", "QB", "jim KELLY", 4,
                "201", "155", "77.1", "2312", "11.5", "24", "4", "4", "40", "10.0", "0"));
            CollectionAssert.Contains(lines, Row("bills", "RB1", "RB", "thurman THOMAS", 11,
                "174", "1694", "9.7", "18", "45", "604", "13.4", "0"));
            CollectionAssert.Contains(lines, Row("bills", "WR1", "WR", "don BEEBE", 11,
                "0", "0", "", "0", "33", "472", "14.3", "0"));
            CollectionAssert.Contains(lines, Row("bills", "NT", "DL", "jeff WRIGHT", 19, "4", "0", "0", "0"));
            CollectionAssert.Contains(lines, Row("bills", "LE", "DL", "phil HANSEN", 19, "1", "3", "30", "0"));
            CollectionAssert.Contains(lines, Row("bills", "K", "K", "steve CHRISTIE", 23, "46", "50", "8", "10"));
            CollectionAssert.Contains(lines, Row("bills", "P", "P", "chris MOHR", 27, "13", "621", "47.8"));
        }

        [TestMethod]
        public void ToCsv_Groups_CoverLinebackersAndDefensiveBacks()
        {
            List<string> lines = CsvLines(Load(Gen06bState).ToCsv(null));

            CollectionAssert.Contains(lines, Row("patriots", "RILB", "LB", "RILB Patriots", 19, "8", "0", "0", "0"));
            CollectionAssert.Contains(lines, Row("bills", "RCB", "DB", "RCB Bills", 19, "1", "2", "9", "0"));
            CollectionAssert.Contains(lines, Row("vikings", "WR1", "WR", "WR1 Vikings", 11,
                "32", "248", "7.8", "5", "45", "477", "10.6", "1"));
        }

        [TestMethod]
        public void ToCsv_WithoutRom_UsesGenericNames_AndSkipsPlayersWithNoStats()
        {
            List<string> lines = CsvLines(Load(Gen06bState).ToCsv(null));

            CollectionAssert.Contains(lines, Row("bills", "QB1", "QB", "QB1 Bills", 4,
                "201", "155", "77.1", "2312", "11.5", "24", "4", "4", "40", "10.0", "0"));
            foreach (string line in lines)
            {
                Assert.IsFalse(line.StartsWith("bills,QB2,"), "QB2 had no stats: " + line);
                Assert.IsFalse(line.StartsWith("bills,RB4,"), "RB4 had no stats: " + line);
                Assert.IsFalse(line.StartsWith("bills,SS,"), "SS had no stats: " + line);
                Assert.IsFalse(line.StartsWith("bills,C,"), "linemen have no stats: " + line);
            }
        }

        [TestMethod]
        public void ToCsv_EveryRowHasTheHeadersColumnCount()
        {
            List<string> lines = CsvLines(Load(Gen06bState).ToCsv(null));

            Assert.IsTrue(lines.Count > 28 * 10, "expected hundreds of player rows, got " + lines.Count);
            foreach (string line in lines)
                Assert.AreEqual(38, line.Split(',').Length, line);
        }

        [TestMethod]
        public void ToCsv_UsesDotDecimals_WhateverTheUsersCulture()
        {
            System.Globalization.CultureInfo saved = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
                string csv = Load(Gen06bState).ToCsv(null);
                StringAssert.Contains(csv, ",77.1,2312,11.5,");
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = saved;
            }
        }

        // ---- Returns: stored per return-team slot (the 3 'RETURN_TEAM' players, in ROM order) ----

        [TestMethod]
        public void Gen05b_Returns_MatchTheReturnLeadersScreen()
        {
            GenesisSeasonStats stats = Load(Gen05bState);

            PlayerSeasonStats gray = stats.GetTeam("lions").GetReturner(2); // RETURN_TEAM RB3, RB4, RCB
            CollectionAssert.AreEqual(new int[] { 75, 827, 0, 5, 30, 0 },
                new int[] { gray.KickReturns, gray.KickReturnYards, gray.KickReturnTDs, gray.PuntReturns, gray.PuntReturnYards, gray.PuntReturnTDs });

            TeamSeasonStats falcons = stats.GetTeam("falcons");
            Assert.AreEqual(53, falcons.GetReturner(1).KickReturns);   // Keith Jones
            Assert.AreEqual(694, falcons.GetReturner(1).KickReturnYards);
            Assert.AreEqual(27, falcons.GetReturner(3).KickReturns);   // Deion Sanders
            Assert.AreEqual(440, falcons.GetReturner(3).KickReturnYards);

            PlayerSeasonStats mitchell = stats.GetTeam("redskins").GetReturner(2);
            CollectionAssert.AreEqual(new int[] { 60, 856, 14, 116 },
                new int[] { mitchell.KickReturns, mitchell.KickReturnYards, mitchell.PuntReturns, mitchell.PuntReturnYards });

            Assert.AreEqual(924, stats.GetTeam("cardinals").GetReturner(1).KickReturnYards); // Johnny Bailey

            PlayerSeasonStats jarvis = stats.GetTeam("dolphins").GetReturner(3); // Jarvis Williams (SS): a KR TD
            CollectionAssert.AreEqual(new int[] { 69, 853, 1 },
                new int[] { jarvis.KickReturns, jarvis.KickReturnYards, jarvis.KickReturnTDs });
        }

        [TestMethod]
        public void Gen05b_InterceptionReturns_MatchTheInterceptionLeadersScreen()
        {
            GenesisSeasonStats stats = Load(Gen05bState);

            PlayerSeasonStats jamison = stats.GetTeam("lions").GetPlayer("ROLB");
            CollectionAssert.AreEqual(new int[] { 6, 46, 0 }, new int[] { jamison.Interceptions, jamison.InterceptionYards, jamison.InterceptionTDs });
            PlayerSeasonStats williams = stats.GetTeam("steelers").GetPlayer("NT"); // Gerald Williams
            CollectionAssert.AreEqual(new int[] { 4, 193, 1 }, new int[] { williams.Interceptions, williams.InterceptionYards, williams.InterceptionTDs });
        }

        [TestMethod]
        public void ToCsv_WithRom_PutsReturnsOnTheReturnTeamPlayersRow()
        {
            GenesisSeasonStats stats = Load(Gen05bState);
            Genesis_TSB1Tool rom = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            List<string> lines = CsvLines(stats.ToCsv(rom));

            // Mel Gray (lions RB4) had no rushes or catches, only returns
            CollectionAssert.Contains(lines, Row("lions", "RB4", "RB", "mel GRAY", 11,
                "0", "0", "", "0", "0", "0", "", "0", "", "", "", "", "", "", "", "", "", "", "",
                "75", "827", "11.0", "0", "5", "30", "6.0", "0"));
        }

        [TestMethod]
        public void ToCsv_WithoutRom_ReturnersGetTheirOwnRows()
        {
            List<string> lines = CsvLines(Load(Gen05bState).ToCsv(null));

            CollectionAssert.Contains(lines, Row("lions", "RET2", "RET", "RET2 Lions", 30,
                "75", "827", "11.0", "0", "5", "30", "6.0", "0"));
            foreach (string line in lines)
                Assert.IsFalse(line.StartsWith("lions,RET1,"), "RET1 had no returns: " + line);
        }

        [TestMethod]
        public void ToText_HasAReturnsSection()
        {
            GenesisSeasonStats stats = Load(Gen05bState);
            Genesis_TSB1Tool rom = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            string withRom = stats.ToText(rom);
            StringAssert.Contains(withRom, "RETURNS");
            StringAssert.Contains(withRom, "mel GRAY");
            StringAssert.Contains(stats.ToText(null), "RET2 Lions");
        }

        [TestMethod]
        public void ToText_HasEveryTeam_AndTheKeyNumbers()
        {
            string text = Load(Gen06bState).ToText(null);

            foreach (string team in new string[] { "BILLS", "OILERS", "COWBOYS", "REDSKINS", "FALCONS", "49ERS" })
                StringAssert.Contains(text, team);
            StringAssert.Contains(text, "2312"); // Kelly pass yds
            StringAssert.Contains(text, "1694"); // Thurman Thomas rush yds
            StringAssert.Contains(text, "-3");   // jets QB rush yds
        }
    }
}
