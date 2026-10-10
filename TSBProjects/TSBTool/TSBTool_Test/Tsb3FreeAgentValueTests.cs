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
    /// TSB3's free agent value -- Face's last digit (byte 3 of the ratings record, low nibble; the first digit is
    /// the skin tone) -- is what TSB3's team defense adds up: the starting front seven for rush defense, plus
    /// the 4 DBs for pass defense (SimTests\TSB3_study\TSB3_HiddenRating_results.txt). Tecmo set it by hand.
    /// Auto Update Sim and the TSB2 -> TSB3 converter work it out with a model fitted to Tecmo's values per
    /// position group (TsbSimModel, Models\tsb3_model.json "free_agent_value"), which keeps Tecmo's scale; the
    /// old hand-written FAP_Formulas.txt ran high (team defense levels 10.3 / 9.4 against Tecmo's 7.6 / 5.2).
    /// </summary>
    [TestClass]
    public class Tsb3FreeAgentValueTests
    {
        private const string Tsb3StockRosters = "tsb3_stock_rosters.txt";

        private static readonly Regex FaceRegex = new Regex(@"^([A-Z]+[1-5]?),[^,]*,Face=0x([0-9a-fA-F])([0-9a-fA-F]),");
        private static readonly string[] ThreeFour = { "bills", "patriots", "steelers", "packers", "panthers" };
        private static readonly int[] RushThresholds = { 30, 33, 35, 38, 40, 42, 45, 48, 50, 53, 55, 60, 80, 100, 120, 255 };
        private static readonly int[] PassThresholds = { 60, 63, 66, 69, 72, 75, 78, 81, 84, 87, 90, 95, 100, 120, 140, 255 };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static string[] Lines(string text)
        {
            return text.Replace("\r", "").Split('\n');
        }

        private static string AutoUpdated()
        {
            return TsbSimAutoUpdater.AutoUpdatePlayerSimData(TestRoms.LoadTsb2SimModelText(Tsb3StockRosters), TSBContentType.TSB3);
        }

        /// <summary>Each real team's average rush and pass defense level from the free agent values (no team bonus).</summary>
        private static double[] AverageLevels(string text)
        {
            var values = new Dictionary<string, Dictionary<string, int>>();
            string team = null;
            foreach (string line in Lines(text))
            {
                Match t = Regex.Match(line, @"^TEAM\s*=\s*(\w+)");
                if (t.Success) { team = t.Groups[1].Value; values[team] = new Dictionary<string, int>(); continue; }
                Match m = FaceRegex.Match(line);
                if (m.Success && team != null)
                    values[team][m.Groups[1].Value] = Convert.ToInt32(m.Groups[3].Value, 16);
            }
            double rush = 0, pass = 0;
            int n = 0;
            foreach (var kv in values.Where(v => v.Key != "freeAgents" && v.Key != "allTime"))
            {
                string[] front = ThreeFour.Contains(kv.Key)
                    ? new[] { "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB" }
                    : new[] { "RE", "NT", "LE", "RE2", "ROLB", "RILB", "LILB" };
                int r = front.Sum(p => kv.Value[p]);
                int s = r + new[] { "RCB", "LCB", "FS", "SS" }.Sum(p => kv.Value[p]);
                rush += Array.FindIndex(RushThresholds, x => x >= r);
                pass += Array.FindIndex(PassThresholds, x => x >= s);
                n++;
            }
            return new[] { rush / n, pass / n };
        }

        [TestMethod]
        public void AutoUpdate_KeepsTecmosTeamDefenseScale()
        {
            double[] stock = AverageLevels(TestRoms.LoadTsb2SimModelText(Tsb3StockRosters));
            double[] updated = AverageLevels(AutoUpdated());

            Assert.AreEqual(7.6, stock[0], 0.05, "stock rush level");
            Assert.AreEqual(5.2, stock[1], 0.05, "stock pass level");
            Assert.AreEqual(stock[0], updated[0], 1.5, "rush defense level");
            Assert.AreEqual(stock[1], updated[1], 1.5, "pass defense level");
        }

        [TestMethod]
        public void AutoUpdate_KeepsTheSkinTone()
        {
            string[] stock = Lines(TestRoms.LoadTsb2SimModelText(Tsb3StockRosters));
            string[] updated = Lines(AutoUpdated());
            int checkedLines = 0;
            for (int i = 0; i < stock.Length; i++)
            {
                Match a = FaceRegex.Match(stock[i]);
                if (!a.Success)
                    continue;
                Assert.AreEqual(a.Groups[2].Value, FaceRegex.Match(updated[i]).Groups[2].Value, "line " + (i + 1));
                checkedLines++;
            }
            Assert.IsTrue(checkedLines > 1000, "every player line, linemen and kickers too");
        }

        [TestMethod]
        public void BetterRatings_NeverLowerTheValue()
        {
            string[] lines =
            {
                "RE,bruce SMITH,Face=0x0e,#78,50,50,63,69,38,63,6,81,[1C,01,0D]",
                "RCB,deion SANDERS,Face=0x0f,#21,94,44,88,50,63,88,81,75,[00,32,0B]",
                "QB1,steve YOUNG,Face=0x0f,#8,69,50,63,44,56,69,81,81,81,75,63,[1A,08,F2]",
                "C,jerry FONTENOT,Face=0x06,#67,69,50,50,44,38,56,",
            };
            foreach (string line in lines)
            {
                int before = Value(TsbSimAutoUpdater.UpdateFreeAgentValue(line));
                int better = Value(TsbSimAutoUpdater.UpdateFreeAgentValue(ChangeRatings(line, 19)));
                int worse = Value(TsbSimAutoUpdater.UpdateFreeAgentValue(ChangeRatings(line, -19)));
                Assert.IsTrue(better >= before && worse <= before, line);
            }
        }

        private static int Value(string line)
        {
            return Convert.ToInt32(FaceRegex.Match(line).Groups[3].Value, 16);
        }

        private static string ChangeRatings(string line, int delta)
        {
            Match m = Regex.Match(line, @"^(\w+,[^,]*,Face=0x\w+,#\d+,)([\d,]+?)(,\[.*|,)$");
            string[] r = m.Groups[2].Value.Split(',').Select(x => Math.Max(6, Math.Min(100, int.Parse(x) + delta)).ToString()).ToArray();
            return m.Groups[1].Value + string.Join(",", r) + m.Groups[3].Value;
        }

        [TestMethod]
        public void Tsb2ToTsb3Converter_UsesTheModel()
        {
            string tsb2 = "RE,bruce SMITH,Face=0x0e,#78,50,50,63,69,38,6,81,[1C,01,0D]\n";
            string converted = Lines(TSB3Converter.ConvertToTSB3FromTSB2(tsb2))[0];
            Assert.AreEqual(TsbSimAutoUpdater.UpdateFreeAgentValue(converted), converted);
        }

        [TestMethod]
        public void UpdateRomSimData_WritesTheFreeAgentValue_KeepsTheSkinTone()
        {
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc")) as TSB2Tool;
            // the Bills' RE and C (linemen too)
            string want = Lines(AutoUpdated()).First(l => l.StartsWith("RE,"));
            string before = Lines(tool.GetTeams(1)).First(l => l.StartsWith("RE,"));
            string wantC = Lines(AutoUpdated()).First(l => l.StartsWith("C,"));

            TsbSimAutoUpdater.UpdateRomSimData(tool, 1);

            string after = Lines(tool.GetTeams(1)).First(l => l.StartsWith("RE,"));
            Assert.AreEqual(wantC, Lines(tool.GetTeams(1)).First(l => l.StartsWith("C,")));
            Assert.AreEqual(FaceRegex.Match(want).Groups[3].Value, FaceRegex.Match(after).Groups[3].Value, "free agent value");
            Assert.AreEqual(FaceRegex.Match(before).Groups[2].Value, FaceRegex.Match(after).Groups[2].Value, "skin tone");
        }

        [TestMethod]
        public void FapFormulas_AreRetired()
        {
            Assert.IsNull(StaticUtils.GetEmbeddedTextFile("TSBTool.Formulas.FAP_Formulas.txt"));
        }
    }
}
