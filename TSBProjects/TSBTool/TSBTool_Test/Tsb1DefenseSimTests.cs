using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB1 (NES, SNES, Genesis) credits a sack or an interception with a fall-through pick: a random
    /// number 0-255 minus each of the first 10 defenders' shares (RE..FS) in turn; whatever is left goes
    /// to the SS (TSB_Simulation.md, Step 5). The old Auto Update shared only 200 sack points over the
    /// front 7 and wrote 0 for the CBs and FS, so the SS got ~23% of all sacks (SNES 2026 season: the SS
    /// led 17 of 28 teams). The INT split used PI alone, so the LBs got 37% of the INTs (stock ~18%).
    /// The new split is fitted to Tecmo's stock bytes: sacks = position weight * (MS + HP)^3 over a pool
    /// of 246, CB/FS 2 each; INTs = position weight * PI^2, DL 2 each, SS = the leftover.
    /// </summary>
    [TestClass]
    public class Tsb1DefenseSimTests
    {
        // the order of SimStuff's results: the game's sim block order
        private static readonly string[] Defense = { "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS" };
        private const int SS = 10;

        private static int FirstTen(int[] shares)
        {
            int sum = 0;
            for (int i = 0; i < SS; i++)
                sum += shares[i];
            return sum;
        }

        [TestMethod]
        public void PassRush_FrontSevenGetsTheSacks_SafetyOnlyTheLeftover()
        {
            // the 2026 Bills' front 7 (MS + HP)
            int[] shares = SimStuff.GetSimPassRush(82, 106, 113, 100, 82, 82, 107);

            Assert.AreEqual(Defense.Length, shares.Length);
            int firstTen = FirstTen(shares);
            Assert.IsTrue(256 - firstTen <= 12, "SS would get " + (256 - firstTen) + "/256 of the sacks.");
            Assert.AreEqual(256 - firstTen, shares[SS], "The SS value should be his real share (the leftover).");
            Assert.AreEqual(2, shares[7], "RCB");
            Assert.AreEqual(2, shares[8], "LCB");
            Assert.AreEqual(2, shares[9], "FS");
        }

        [TestMethod]
        public void PassRush_StarRusherStandsOut()
        {
            int[] shares = SimStuff.GetSimPassRush(125, 82, 82, 82, 82, 82, 82);

            Assert.IsTrue(shares[0] > 64, "A star RE should get more than a quarter of the sacks, got " + shares[0]);
        }

        [TestMethod]
        public void PassRush_InsideLinebackersGetLessThanEndsAndOutsideLinebackers()
        {
            int[] shares = SimStuff.GetSimPassRush(100, 100, 100, 100, 100, 100, 100);

            Assert.IsTrue(shares[4] < shares[0] && shares[5] < shares[2], "ILB vs DE");
            Assert.IsTrue(shares[4] < shares[3] && shares[5] < shares[6], "ILB vs OLB");
        }

        [TestMethod]
        public void PassRush_NoRatingsAtAll_StillSharesThePool()
        {
            int[] shares = SimStuff.GetSimPassRush(0, 0, 0, 0, 0, 0, 0);

            Assert.IsTrue(256 - FirstTen(shares) <= 12);
        }

        [TestMethod]
        public void Coverage_DefensiveLineGetsTwo_SafetyGetsTheLeftover()
        {
            // ROLB RILB LILB LOLB RCB LCB FS SS (PI)
            int[] shares = SimStuff.GetSimPassDefense(31, 44, 31, 31, 44, 44, 38, 50);

            Assert.AreEqual(Defense.Length, shares.Length);
            Assert.AreEqual(2, shares[0], "RE");
            Assert.AreEqual(2, shares[1], "NT");
            Assert.AreEqual(2, shares[2], "LE");
            Assert.AreEqual(256 - FirstTen(shares), shares[SS], "The SS value should be his real share (the leftover).");
        }

        [TestMethod]
        public void Coverage_LinebackersGetLessThanDefensiveBacksWithTheSamePI()
        {
            int[] shares = SimStuff.GetSimPassDefense(50, 50, 50, 50, 50, 50, 50, 50);

            for (int lb = 3; lb <= 6; lb++)
                for (int db = 7; db <= SS; db++)
                    Assert.IsTrue(shares[lb] < shares[db], Defense[lb] + " vs " + Defense[db]);
        }

        [TestMethod]
        public void AutoUpdateDefenseSimData_StockSnesRosters_SafetyNeverLeadsInSacks()
        {
            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB1.smc"));

            string output = TecmonsterTSB1SimAutoUpdater.AutoUpdateDefenseSimData(tool.GetAll());

            Dictionary<string, Dictionary<string, int[]>> teams = DefenseSims(output);
            Assert.AreEqual(28, teams.Count);
            foreach (KeyValuePair<string, Dictionary<string, int[]>> team in teams)
            {
                int sacks = 0, ints = 0;
                for (int i = 0; i < SS; i++)
                {
                    sacks += team.Value[Defense[i]][0];
                    ints += team.Value[Defense[i]][1];
                }
                Assert.IsTrue(256 - sacks <= 12, team.Key + ": SS gets " + (256 - sacks) + "/256 of the sacks");
                Assert.AreEqual(256 - sacks, team.Value["SS"][0], team.Key + " SS sack value");
                Assert.AreEqual(256 - ints, team.Value["SS"][1], team.Key + " SS INT value");
            }
        }

        [TestMethod]
        public void AutoUpdateDefenseSimData_LeavesTheOffenseAlone()
        {
            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB1.smc"));
            string input = tool.GetAll();

            string output = TecmonsterTSB1SimAutoUpdater.AutoUpdateDefenseSimData(input);

            Regex offense = new Regex(@"^(QB|RB|WR|TE|K|P)\d?,.*$", RegexOptions.Multiline);
            MatchCollection before = offense.Matches(input);
            MatchCollection after = offense.Matches(output);
            Assert.AreEqual(before.Count, after.Count);
            for (int i = 0; i < before.Count; i++)
                Assert.AreEqual(before[i].Value, after[i].Value);
        }

        /// <summary>team -> defender position -> [pass rush, coverage]</summary>
        private static Dictionary<string, Dictionary<string, int[]>> DefenseSims(string text)
        {
            Dictionary<string, Dictionary<string, int[]>> ret = new Dictionary<string, Dictionary<string, int[]>>();
            Dictionary<string, int[]> current = null;
            Regex teamRegex = new Regex(@"^TEAM\s*=\s*(\w+)");
            Regex defRegex = new Regex(@"^(RE|NT|LE|ROLB|RILB|LILB|LOLB|RCB|LCB|FS|SS),.*\[\s*(\d+)\s*,\s*(\d+)\s*\]");
            foreach (string line in text.Split('\n'))
            {
                Match m = teamRegex.Match(line);
                if (m.Success)
                {
                    current = new Dictionary<string, int[]>();
                    ret[m.Groups[1].Value] = current;
                    continue;
                }
                m = defRegex.Match(line);
                if (m.Success && current != null)
                    current[m.Groups[1].Value] = new int[] { int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value) };
            }
            return ret;
        }
    }
}
