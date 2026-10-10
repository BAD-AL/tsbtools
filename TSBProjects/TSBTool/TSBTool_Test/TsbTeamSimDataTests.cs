using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB2's team SimData (0xRP: rush defense, pass defense) in Auto Update Sim (TsbSimAutoUpdater.AutoUpdateSimData):
    /// worked out from the 11 starting defenders' ratings (RE NT LE, the 4 LBs, RCB LCB FS SS) with a model
    /// fitted to Tecmo's own team bytes of the three stock seasons (TsbSimModel, Models\tsb2_report.txt).
    /// Ratings explain Tecmo's bytes only partly (Tecmo set them mostly from each team's real season), so the
    /// model gives a ratings-based value with Tecmo's league spread. The expected values come from TsbSimModel's
    /// 'expected' command (TSB2SimModel\tsb2_stock_team_simdata.txt).
    /// AutoUpdatePlayerSimData (the converter, UpdateRomSimData) leaves team values alone; TSB3's team SimData
    /// (a defense bonus, TSB3Tool) is never touched.
    /// </summary>
    [TestClass]
    public class TsbTeamSimDataTests
    {
        private const string StockRosters = "tsb2_stock_rosters.txt";
        private const string PlayersOnly = "tsb2_stock_rosters_autoupdated.txt";
        private const string ExpectedTeams = "tsb2_stock_team_simdata.txt";

        private static readonly Regex TeamLine = new Regex(@"^TEAM\s*=\s*(\w+)");
        private static readonly Regex TeamSim = new Regex(@"SimData\s*=\s*0x([0-9A-Fa-f])([0-9A-Fa-f])");

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static string[] Lines(string text)
        {
            return text.Replace("\r", "").Split('\n');
        }

        /// <summary>"SEASON n" lines and team lines only.</summary>
        private static List<string> SeasonAndTeamLines(string text)
        {
            return Lines(text).Where(l => l.StartsWith("SEASON") || TeamLine.IsMatch(l)).ToList();
        }

        /// <summary>One team's block (team line .. before the next team line) of a season of 'text'.</summary>
        private static string TeamBlock(string text, int season, string team)
        {
            string[] lines = Lines(text);
            int s = Array.FindIndex(lines, l => l == "SEASON " + season);
            int start = Array.FindIndex(lines, s, l => TeamLine.Match(l).Groups[1].Value == team);
            int end = Array.FindIndex(lines, start + 1, l => TeamLine.IsMatch(l) || l.StartsWith("SEASON"));
            return string.Join("\n", lines, start, (end < 0 ? lines.Length : end) - start) + "\n";
        }

        private static int[] TeamValues(string teamLine)
        {
            Match m = TeamSim.Match(teamLine);
            Assert.IsTrue(m.Success, "no SimData in '" + teamLine + "'");
            return new[] { Convert.ToInt32(m.Groups[1].Value, 16), Convert.ToInt32(m.Groups[2].Value, 16) };
        }

        [TestMethod]
        public void StockRosters_TeamSimData_MatchesTheFittedModel()
        {
            string output = TsbSimAutoUpdater.AutoUpdateSimData(TestRoms.LoadTsb2SimModelText(StockRosters), TSBContentType.TSB2);

            List<string> want = SeasonAndTeamLines(TestRoms.LoadTsb2SimModelText(ExpectedTeams));
            List<string> got = SeasonAndTeamLines(output);
            Assert.AreEqual(3 + 84, want.Count, "the expected file: 3 seasons x 28 teams");
            CollectionAssert.AreEqual(want, got);
        }

        [TestMethod]
        public void StockRosters_PlayerSimData_IsThePlayersOnlyUpdate()
        {
            string[] output = Lines(TsbSimAutoUpdater.AutoUpdateSimData(TestRoms.LoadTsb2SimModelText(StockRosters), TSBContentType.TSB2));
            string[] playersOnly = Lines(TestRoms.LoadTsb2SimModelText(PlayersOnly));

            Assert.AreEqual(playersOnly.Length, output.Length);
            for (int i = 0; i < output.Length; i++)
                if (!TeamLine.IsMatch(output[i]))
                    Assert.AreEqual(playersOnly[i], output[i], "line " + (i + 1));
        }

        [TestMethod]
        public void StockRosters_LeagueSpreadIsLikeTecmos()
        {
            string output = TsbSimAutoUpdater.AutoUpdateSimData(TestRoms.LoadTsb2SimModelText(StockRosters), TSBContentType.TSB2);
            List<int[]> values = Lines(output).Where(l => TeamLine.IsMatch(l)).Select(TeamValues).ToList();

            Assert.AreEqual(84, values.Count);
            for (int k = 0; k < 2; k++)
            {
                double[] v = values.Select(x => (double)x[k]).ToArray();
                double mean = v.Average(), sd = Math.Sqrt(v.Select(x => (x - mean) * (x - mean)).Average());
                string what = k == 0 ? "rush D" : "pass D";
                Assert.IsTrue(mean >= 6 && mean <= 8, what + " average " + mean);   // Tecmo's: 7.0 / 6.7
                Assert.IsTrue(sd >= 2.5 && sd <= 3.5, what + " spread " + sd);      // Tecmo's: 3.2 / 3.0
            }
        }

        [TestMethod]
        public void PlayersOnlyUpdate_KeepsTeamValues()
        {
            string input = TestRoms.LoadTsb2SimModelText(StockRosters);
            string output = TsbSimAutoUpdater.AutoUpdatePlayerSimData(input, TSBContentType.TSB2);
            CollectionAssert.AreEqual(SeasonAndTeamLines(input), SeasonAndTeamLines(output));
        }

        [TestMethod]
        public void TeamMissingAStarter_KeepsItsValue()
        {
            string bills = TeamBlock(TestRoms.LoadTsb2SimModelText(StockRosters), 1, "bills");
            string noLolb = string.Join("\n", Lines(bills).Where(l => !l.StartsWith("LOLB,")).ToArray());

            string output = TsbSimAutoUpdater.AutoUpdateSimData("SEASON 1\n" + noLolb, TSBContentType.TSB2);

            StringAssert.Contains(output, "TEAM = bills,SimData=0xD8\n");
        }

        [TestMethod]
        public void TeamLine_OnlyTheDigitsChange_LineEndingsKept()
        {
            string stock = TestRoms.LoadTsb2SimModelText(StockRosters);
            string bills = TeamBlock(stock, 1, "bills");
            string want = Lines(TsbSimAutoUpdater.AutoUpdateSimData("SEASON 1\n" + bills, TSBContentType.TSB2))[1];
            int[] v = TeamValues(want);

            string spaced = bills.Replace("TEAM = bills,SimData=0xD8", "TEAM = bills, SimData=0xd8").Replace("\n", "\r\n");
            string[] output = TsbSimAutoUpdater.AutoUpdateSimData("SEASON 1\r\n" + spaced, TSBContentType.TSB2).Split('\n');

            Assert.AreEqual(string.Format("TEAM = bills, SimData=0x{0:X}{1:X}\r", v[0], v[1]), output[1]);
        }

        [TestMethod]
        public void TeamLineWithoutSimData_GetsOne()
        {
            string bills = TeamBlock(TestRoms.LoadTsb2SimModelText(StockRosters), 1, "bills");
            string want = Lines(TsbSimAutoUpdater.AutoUpdateSimData("SEASON 1\n" + bills, TSBContentType.TSB2))[1];

            string output = TsbSimAutoUpdater.AutoUpdateSimData("SEASON 1\n" + bills.Replace("TEAM = bills,SimData=0xD8", "TEAM = bills"), TSBContentType.TSB2);

            Assert.AreEqual(want, Lines(output)[1]);
        }

        [TestMethod]
        public void BetterDefenders_NeverLowerTheValues()
        {
            string bills = "SEASON 1\n" + TeamBlock(TestRoms.LoadTsb2SimModelText(StockRosters), 1, "bills");
            int[] before = TeamValues(Lines(TsbSimAutoUpdater.AutoUpdateSimData(bills, TSBContentType.TSB2))[1]);
            int[] better = TeamValues(Lines(TsbSimAutoUpdater.AutoUpdateSimData(ChangeDefenders(bills, 25), TSBContentType.TSB2))[1]);
            int[] worse = TeamValues(Lines(TsbSimAutoUpdater.AutoUpdateSimData(ChangeDefenders(bills, -25), TSBContentType.TSB2))[1]);

            for (int k = 0; k < 2; k++)
            {
                Assert.IsTrue(better[k] >= before[k], "better defenders lowered value " + k);
                Assert.IsTrue(worse[k] <= before[k], "worse defenders raised value " + k);
            }
            Assert.IsTrue(better[0] + better[1] > worse[0] + worse[1], "the defenders' ratings should matter");
        }

        /// <summary>Every defender's ratings (RS RP MS HP BB PI QU) moved by 'delta', kept within 6..100.</summary>
        private static string ChangeDefenders(string text, int delta)
        {
            Regex def = new Regex(@"^((?:RE|NT|LE|RE2|NT2|LE2|ROLB|RILB|LILB|LOLB|LB5|RCB|LCB|DB1|DB2|FS|SS|DB3),[^,]*,Face=0x\w+,#\d+,)([\d,]+?)(,\[.*)$");
            return string.Join("\n", Lines(text).Select(l =>
            {
                Match m = def.Match(l);
                if (!m.Success)
                    return l;
                string[] r = m.Groups[2].Value.Split(',').Select(x => Math.Max(6, Math.Min(100, int.Parse(x) + delta)).ToString()).ToArray();
                return m.Groups[1].Value + string.Join(",", r) + m.Groups[3].Value;
            }).ToArray());
        }

        [TestMethod]
        public void Tsb3_TeamSimData_IsNeverTouched()
        {
            string tsb3 = TestRoms.LoadTsb2SimModelText("tsb3_stock_rosters.txt").Replace("TEAM = bills,SimData=0x0000\n", "TEAM = bills,SimData=0x0F14\n");
            string output = TsbSimAutoUpdater.AutoUpdateSimData(tsb3, TSBContentType.TSB3);
            StringAssert.Contains(output, "TEAM = steelers,SimData=0x0A14\n");
            StringAssert.Contains(output, "TEAM = bills,SimData=0x0F14\n");
            StringAssert.Contains(output, "TEAM = freeAgents\n");
        }

        [TestMethod]
        public void Tsb1ToTsb2Conversion_KeepsTheRosterAuthorsTeamValue()
        {
            string tsb1 = "TEAM = bills SimData=0xAB1\n";
            string output = TecmoConverter.Convert(TSBContentType.TSB1, TSBContentType.TSB2, tsb1);
            StringAssert.Contains(output, "TEAM = bills SimData=0xAB1\n");
        }

        [TestMethod]
        public void UpdateRomSimData_WithTeams_WritesThatSeasonsTeamSimData()
        {
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")) as TSB2Tool;
            string want = TeamBlock(TestRoms.LoadTsb2SimModelText(ExpectedTeams) + "TEAM = end\n", 2, "bills");
            Match m = TeamSim.Match(want);

            TsbSimAutoUpdater.UpdateRomSimData(tool, 2, true);

            Assert.AreEqual("SimData=0x" + m.Groups[1].Value + m.Groups[2].Value, tool.GetTeamSimData(2, "bills"));
            Assert.AreEqual("SimData=0xD8", tool.GetTeamSimData(1, "bills"), "season 1 untouched");
            Assert.AreEqual("SimData=0x66", tool.GetTeamSimData(3, "bills"), "season 3 untouched");
        }

        [TestMethod]
        public void UpdateRomSimData_PlayersOnly_KeepsTeamSimData()
        {
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")) as TSB2Tool;
            TsbSimAutoUpdater.UpdateRomSimData(tool, 1);
            Assert.AreEqual("SimData=0xD8", tool.GetTeamSimData(1, "bills"));
        }

        [TestMethod]
        public void PlayerFormUpdater_UpdatesTsb2Teams()
        {
            string stock = TestRoms.LoadTsb2SimModelText(StockRosters);
            Assert.AreEqual(TsbSimAutoUpdater.AutoUpdateSimData(stock, TSBContentType.TSB2),
                TSBXSimAutoUpdater.AutoUpdateSimData(stock, TSBContentType.TSB2));
        }
    }
}
