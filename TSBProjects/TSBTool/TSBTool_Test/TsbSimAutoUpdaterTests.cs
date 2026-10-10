using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// The TSB2/TSB3 Auto Update Sim (TsbSimAutoUpdater): sim bytes from the player ratings, with a model fitted
    /// to Tecmo's own sim bytes of the stock seasons, checked with simmed seasons (TSBTool\TsbSimModel: README.md,
    /// Models\*_report.txt). The test files in TSB2SimModel come from its 'expected' command:
    ///   TsbSimModel expected TSB2 TSB2_U.smc Models\tsb2_model.json <dir>   (and TSB3 with TSB3.smc)
    /// *_stock_rosters.txt = TSB2Tool.GetTeams of every stock season; *_autoupdated.txt = the model's output.
    /// </summary>
    [TestClass]
    public class TsbSimAutoUpdaterTests
    {
        private const string StockRosters = "tsb2_stock_rosters.txt";
        private const string Expected = "tsb2_stock_rosters_autoupdated.txt";

        private static readonly string[] CarryPool = { "QB1", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };
        private static readonly string[] Skill = { "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };
        private static readonly string[] Defenders = { "RE", "NT", "LE", "RE2", "NT2", "LE2", "ROLB", "RILB", "LILB", "LOLB", "LB5",
                                                       "RCB", "LCB", "DB1", "DB2", "FS", "SS", "DB3" };

        private static readonly Regex SimRegex = new Regex(@"^([A-Z]+[1-5]?),.*\[([0-9A-F,]+)\]\s*$");

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        /// <summary>Sim bytes by "season/team" then position, from a TSBTool text.</summary>
        private static Dictionary<string, Dictionary<string, int[]>> SimBytes(string text)
        {
            var ret = new Dictionary<string, Dictionary<string, int[]>>();
            string season = "", team = null;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                if (line.StartsWith("SEASON"))
                    season = line.Trim();
                else if (Regex.IsMatch(line, @"^TEAM\s*="))
                {
                    team = season + "/" + Regex.Match(line, @"TEAM\s*=\s*(\w+)").Groups[1].Value;
                    ret[team] = new Dictionary<string, int[]>();
                }
                else if (team != null)
                {
                    Match m = SimRegex.Match(line);
                    if (m.Success)
                        ret[team][m.Groups[1].Value] = Array.ConvertAll(m.Groups[2].Value.Split(','), s => Convert.ToInt32(s, 16));
                }
            }
            return ret;
        }

        private static string[] Lines(string text)
        {
            return text.Replace("\r", "").Split('\n');
        }

        [TestMethod]
        public void StockRosters_MatchTheFittedModel()
        {
            string output = TsbSimAutoUpdater.AutoUpdatePlayerSimData(TestRoms.LoadTsb2SimModelText(StockRosters));

            string[] got = Lines(output);
            string[] want = Lines(TestRoms.LoadTsb2SimModelText(Expected));
            Assert.AreEqual(want.Length, got.Length, "line count");
            for (int i = 0; i < want.Length; i++)
                Assert.AreEqual(want[i], got[i], "line " + (i + 1));
        }

        [TestMethod]
        public void EveryTeam_SharesAddUpTo255_AndQB2CarriesNothing()
        {
            var teams = SimBytes(TsbSimAutoUpdater.AutoUpdatePlayerSimData(TestRoms.LoadTsb2SimModelText(StockRosters)));

            Assert.AreEqual(84, teams.Count);
            foreach (var t in teams)
            {
                var p = t.Value;
                int carries = 0, targets = 0, sacks = 0, ints = 0;
                foreach (string pos in CarryPool) carries += p[pos][0];
                foreach (string pos in Skill) targets += p[pos][2];
                foreach (string pos in Defenders) { sacks += p[pos][0]; ints += p[pos][1]; }
                Assert.AreEqual(255, carries, t.Key + " carry shares");
                Assert.AreEqual(255, targets, t.Key + " target shares");
                Assert.AreEqual(255, sacks, t.Key + " sack shares");
                Assert.AreEqual(255, ints, t.Key + " INT shares");
                // the carrier pick walks both QBs; a carry that lands on the QB who isn't playing loses
                // its credit (game bug G3), so QB2 gets nothing, like Tecmo's QB2s
                Assert.AreEqual(0, p["QB2"][0], t.Key + " QB2 carry share");
            }
        }

        [TestMethod]
        public void OnlySimBytesChange()
        {
            string[] input = Lines(TestRoms.LoadTsb2SimModelText(StockRosters));
            string[] output = Lines(TsbSimAutoUpdater.AutoUpdatePlayerSimData(TestRoms.LoadTsb2SimModelText(StockRosters)));

            Assert.AreEqual(input.Length, output.Length);
            for (int i = 0; i < input.Length; i++)
            {
                int a = input[i].IndexOf('['), b = output[i].IndexOf('[');
                Assert.AreEqual(a < 0 ? input[i] : input[i].Substring(0, a), b < 0 ? output[i] : output[i].Substring(0, b), "line " + (i + 1));
            }
        }

        private const string BillsSeason1 =
            "SEASON 1\n" +
            "TEAM = bills,SimData=0xD8\n" +
            "QB1,jim KELLY,Face=0x00,#12,69,25,13,13,13,63,81,69,69,75,[08,01,C3]\n" +
            "QB2,frank REICH,Face=0x00,#14,69,25,13,13,13,44,44,44,56,50,[00,01,62]\n" +
            "RB1,hb BILLS,Face=0x8f,#34,69,38,69,19,63,56,56,[98,0C,35,58]\n" +
            "RB2,kenneth DAVIS,Face=0x87,#23,69,44,44,38,31,81,25,[46,08,0E,34]\n" +
            "RB3,carwell GARDNER,Face=0x84,#35,69,38,19,56,13,56,19,[13,03,02,54]\n" +
            "RB4,eddie FULLER,Face=0x82,#33,69,25,31,19,31,56,25,[02,03,02,43]\n" +
            "WR1,james LOFTON,Face=0x88,#80,69,31,19,13,19,63,56,[00,02,2F,89]\n" +
            "WR2,andre REED,Face=0x8c,#83,69,38,56,13,50,44,69,[04,03,3C,EF]\n" +
            "WR3,don BEEBE,Face=0x06,#82,69,25,63,13,25,56,44,[00,03,28,87]\n" +
            "WR4,steve TASKER,Face=0x06,#89,63,38,50,38,38,69,13,[00,02,01,73]\n" +
            "TE1,keith MCKELLER,Face=0x86,#84,69,25,38,44,25,56,44,[00,02,0C,45]\n" +
            "TE2,pete METZELAARS,Face=0x06,#88,56,19,13,63,13,50,31,[00,02,18,56]\n" +
            "C,kent HULL,Face=0x0b,#67,69,44,25,69,13,\n";

        [TestMethod]
        public void BetterReceiver_GetsMoreTargetsAndCatchesBetter()
        {
            var before = SimBytes(TsbSimAutoUpdater.AutoUpdatePlayerSimData(BillsSeason1))["SEASON 1/bills"]["WR4"];
            // steve TASKER's RC (last rating) 13 -> 81
            string better = BillsSeason1.Replace("#89,63,38,50,38,38,69,13,", "#89,63,38,50,38,38,69,81,");
            var after = SimBytes(TsbSimAutoUpdater.AutoUpdatePlayerSimData(better))["SEASON 1/bills"]["WR4"];

            Assert.IsTrue(after[2] > before[2], string.Format("target share {0} -> {1}", before[2], after[2]));
            Assert.IsTrue((after[3] & 0x0F) > (before[3] & 0x0F), string.Format("catch {0} -> {1}", before[3] & 15, after[3] & 15));
        }

        [TestMethod]
        public void IncompleteTeam_KeepsTheShares_UpdatesTheRest()
        {
            // no defense, K or P: the carry and target pools are complete, the sack/INT pools aren't there
            string text = BillsSeason1 + "RE,de BILLS,Face=0x8d,#78,50,38,63,69,13,19,75,[51,07,0F]\n";
            var bills = SimBytes(TsbSimAutoUpdater.AutoUpdatePlayerSimData(text))["SEASON 1/bills"];

            int carries = 0;
            foreach (string pos in CarryPool) carries += bills[pos][0];
            Assert.AreEqual(255, carries, "the carry pool is all there");
            Assert.AreEqual(0x51, bills["RE"][0], "sack share kept: the other 17 defenders aren't in the text");
            Assert.AreEqual(0x07, bills["RE"][1], "INT share kept");
            Assert.AreEqual(0x06, bills["RE"][2], "tackle weight is the player's own, so it's updated (model: 6)");
        }

        [TestMethod]
        public void PlayerWithoutSimData_GetsIt()
        {
            string text = BillsSeason1.Replace(",[08,01,C3]", "");
            var bills = SimBytes(TsbSimAutoUpdater.AutoUpdatePlayerSimData(text))["SEASON 1/bills"];
            Assert.AreEqual(3, bills["QB1"].Length);
        }

        [TestMethod]
        public void CarriageReturns_AreKept()
        {
            string output = TsbSimAutoUpdater.AutoUpdatePlayerSimData(BillsSeason1.Replace("\n", "\r\n"));
            StringAssert.Contains(output, "]\r\n");
            Assert.IsFalse(output.Contains("\r\r"));
        }

        [TestMethod]
        public void TsbxUpdater_Tsb2_UsesTheNewModel()
        {
            string text = TestRoms.LoadTsb2SimModelText(StockRosters);
            Assert.AreEqual(TsbSimAutoUpdater.AutoUpdatePlayerSimData(text),
                TSBXSimAutoUpdater.AutoUpdatePlayerSimData(text, TSBContentType.TSB2));
        }

        [TestMethod]
        public void AutoUpdatedText_AppliesToTheRom()
        {
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")) as TSB2Tool;
            string output = TsbSimAutoUpdater.AutoUpdatePlayerSimData(TestRoms.LoadTsb2SimModelText(StockRosters));
            tool.ProcessText(output);

            var want = SimBytes(output);
            foreach (int season in new[] { 1, 3 })
            {
                var got = SimBytes("SEASON " + season + "\n" + tool.GetTeams(season));
                foreach (string team in new[] { "bills", "49ers" })
                    foreach (string pos in new[] { "QB1", "QB2", "RB1", "WR4", "TE2", "ROLB", "LOLB", "LB5", "DB3", "K", "P" })
                        CollectionAssert.AreEqual(want["SEASON " + season + "/" + team][pos], got["SEASON " + season + "/" + team][pos],
                            season + " " + team + " " + pos);
            }
        }

        // ---- a share of 255 (FF) is the sim engine's 'absent player' marker: a player with it is skipped
        //      (seen in a TSB3 sim test, TSB3_study\TSB3_Checks_results.txt), so shares stop at 254

        [TestMethod]
        public void ScaleTo_NeverGivesAShareOf255()
        {
            CollectionAssert.AreEqual(new int[] { 254, 1, 0 }, TsbSimAutoUpdater.ScaleTo(new double[] { 5, 0, 0 }, 255));
            CollectionAssert.AreEqual(new int[] { 1, 254 }, TsbSimAutoUpdater.ScaleTo(new double[] { 0, 9 }, 255));
            CollectionAssert.AreEqual(new int[] { 128, 127 }, TsbSimAutoUpdater.ScaleTo(new double[] { 1, 1 }, 255));
        }

        // ---- a TSB1 roster converted to TSB2 has no RE2/NT2/LE2/LB5/DB3, so its sack and INT pools are
        //      incomplete in the text; after it's applied, UpdateRomSimData works them out from the ROM's
        //      whole roster

        private static TSB2Tool ConvertedTsb1Rom()
        {
            ITecmoContent tsb1 = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB1.smc"));
            string converted = TSB2Converter.ConvertToTSB2FromTSB1(tsb1.GetAll(1));
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")) as TSB2Tool;
            tool.ProcessText(converted); // season 1 (the converted text has no SEASON line)
            return tool;
        }

        [TestMethod]
        public void ConvertedTsb1Roster_UpdateRomSimData_EveryPoolAddsUpTo255()
        {
            TSB2Tool tool = ConvertedTsb1Rom();
            TsbSimAutoUpdater.UpdateRomSimData(tool, 1);

            var teams = SimBytes("SEASON 1\n" + tool.GetTeams(1));
            Assert.AreEqual(28, teams.Count);
            foreach (var t in teams)
            {
                var p = t.Value;
                int carries = 0, targets = 0, sacks = 0, ints = 0;
                foreach (string pos in CarryPool) carries += p[pos][0];
                foreach (string pos in Skill) targets += p[pos][2];
                foreach (string pos in Defenders) { sacks += p[pos][0]; ints += p[pos][1]; }
                CollectionAssert.AreEqual(new int[] { 255, 255, 255, 255, 0 }, new int[] { carries, targets, sacks, ints, p["QB2"][0] },
                    t.Key + ": carry, target, sack, INT shares, QB2 carry");
            }
        }

        [TestMethod]
        public void SeasonsIn_TheSeasonsATextWritesTo()
        {
            CollectionAssert.AreEqual(new int[] { 1 }, TsbSimAutoUpdater.SeasonsIn("TEAM = bills,SimData=0xD8\nQB1,..."), "no SEASON line: season 1");
            CollectionAssert.AreEqual(new int[] { 3, 1 }, TsbSimAutoUpdater.SeasonsIn("SEASON 3\nTEAM = bills\nSEASON 1\nTEAM = bills\nSEASON 3\n"));
        }

        [TestMethod]
        public void UpdateRomSimData_ChangesOnlySimBytes_OfThatSeason()
        {
            TSB2Tool tool = ConvertedTsb1Rom();
            string season1 = tool.GetTeams(1), season3 = tool.GetTeams(3);
            byte[] before = (byte[])tool.OutputRom.Clone();

            TsbSimAutoUpdater.UpdateRomSimData(tool, 1);

            Assert.AreEqual(season3, tool.GetTeams(3), "other seasons untouched");
            string[] a = Lines(season1), b = Lines(tool.GetTeams(1));
            Assert.AreEqual(a.Length, b.Length);
            for (int i = 0; i < a.Length; i++)
            {
                int x = a[i].IndexOf('['), y = b[i].IndexOf('[');
                Assert.AreEqual(x < 0 ? a[i] : a[i].Substring(0, x), y < 0 ? b[i] : b[i].Substring(0, y), "names/ratings, line " + (i + 1));
            }
            // and nothing outside season 1's sim blocks (no name rewrites)
            int simStart = 0x1EE000 - 0xE000, simEnd = 0x1EE000 + 0x2000;
            for (int i = 0; i < before.Length; i++)
                if (before[i] != tool.OutputRom[i])
                    Assert.IsTrue(i >= simStart && i < simEnd, string.Format("byte 0x{0:X} changed", i));
        }

        // ---- TSB3: its own fitted model (TsbSimModel\Models\tsb3_model.json, tsb3_report.txt). TSB3 runs
        //      TSB2's sim engine with its own tables (scramble the other way round, ...); its ratings add AG.
        //      The free agents and the all-time team aren't teams: they keep their shares.

        private const string Tsb3StockRosters = "tsb3_stock_rosters.txt";
        private const string Tsb3Expected = "tsb3_stock_rosters_autoupdated.txt";

        private static string SimPart(string line)
        {
            int i = line.IndexOf('[');
            return i < 0 ? "" : line.Substring(i);
        }

        [TestMethod]
        public void Tsb3_StockRosters_MatchTheFittedModel()
        {
            // whole lines: the sim data and the free agent value (Face's last digit, Tsb3FreeAgentValueTests)
            string output = TsbSimAutoUpdater.AutoUpdatePlayerSimData(TestRoms.LoadTsb2SimModelText(Tsb3StockRosters), TSBContentType.TSB3);

            string[] got = Lines(output);
            string[] want = Lines(TestRoms.LoadTsb2SimModelText(Tsb3Expected));
            Assert.AreEqual(want.Length, got.Length, "line count");
            for (int i = 0; i < want.Length; i++)
                Assert.AreEqual(want[i], got[i], "line " + (i + 1));
        }

        [TestMethod]
        public void Tsb3_EveryTeam_SharesAddUpTo255_FreeAgentsAndAllTimeKeepTheirs()
        {
            string input = TestRoms.LoadTsb2SimModelText(Tsb3StockRosters);
            var before = SimBytes(input);
            var teams = SimBytes(TsbSimAutoUpdater.AutoUpdatePlayerSimData(input, TSBContentType.TSB3));

            Assert.AreEqual(32, teams.Count);
            foreach (var t in teams)
            {
                var p = t.Value;
                int carries = 0, targets = 0, sacks = 0, ints = 0;
                foreach (string pos in CarryPool) carries += p[pos][0];
                foreach (string pos in Skill) targets += p[pos][2];
                foreach (string pos in Defenders) { sacks += p[pos][0]; ints += p[pos][1]; }
                if (t.Key.EndsWith("/freeAgents") || t.Key.EndsWith("/allTime"))
                {
                    foreach (string pos in CarryPool)
                        Assert.AreEqual(before[t.Key][pos][0], p[pos][0], t.Key + " " + pos + " carry share kept");
                    foreach (string pos in Defenders)
                        Assert.AreEqual(before[t.Key][pos][1], p[pos][1], t.Key + " " + pos + " INT share kept");
                    continue;
                }
                CollectionAssert.AreEqual(new int[] { 255, 255, 255, 255, 0 }, new int[] { carries, targets, sacks, ints, p["QB2"][0] },
                    t.Key + ": carry, target, sack, INT shares, QB2 carry");
            }
        }

        [TestMethod]
        public void Tsb3_FreeAgentValue_IsStillWorkedOut()
        {
            // TSB3's free agent value lives in Face's last digit (TsbSimAutoUpdater.UpdateFreeAgentValue)
            string line = "QB1,steve WALSH,Face=0x07,#4,69,25,13,13,19,25,44,44,50,56,56,[02,01,81]";
            string output = Lines(TsbSimAutoUpdater.AutoUpdatePlayerSimData("TEAM = bears,SimData=0x68\n" + line, TSBContentType.TSB3))[1];
            Assert.AreEqual(TsbSimAutoUpdater.UpdateFreeAgentValue(line).Split(',')[2], output.Split(',')[2]);
            // linemen (no sim data) get theirs too
            string ol = "C,jerry FONTENOT,Face=0x06,#67,69,50,50,44,38,56,";
            string olOut = Lines(TsbSimAutoUpdater.AutoUpdatePlayerSimData("TEAM = bears,SimData=0x68\n" + ol, TSBContentType.TSB3))[1];
            Assert.AreEqual(TsbSimAutoUpdater.UpdateFreeAgentValue(ol), olOut);
        }

        [TestMethod]
        public void TsbxUpdater_Tsb3_UsesTheNewModel()
        {
            string text = TestRoms.LoadTsb2SimModelText(Tsb3StockRosters);
            Assert.AreEqual(TsbSimAutoUpdater.AutoUpdatePlayerSimData(text, TSBContentType.TSB3),
                TSBXSimAutoUpdater.AutoUpdatePlayerSimData(text, TSBContentType.TSB3));
        }
    }
}
