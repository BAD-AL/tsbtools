using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TsbSimModel
{
    /// <summary>
    /// The sim-check tools: what a ROM's sim bytes do with no dice ('nodice'), league shapes of simmed
    /// seasons from battery saves ('compare'), and making an auto-updated test ROM ('apply').
    /// Checks are done the way the TSB2/TSB3 study did them (SimTests\TSB2_study, TSB3_study): build a ROM,
    /// sim a full season in an emulator, compare it with stock seasons -- and with 'nodice', to tell what
    /// the bytes do from one season's luck.
    /// </summary>
    public static class Analysis
    {
        // the player sim blocks: [pointer table, bank base] per season (TSB2Tool.GetSimLocation)
        private static readonly int[][] Tsb2Tables = { new[] { 0x1EE000, 0x1E0000 }, new[] { 0x1F6000, 0x1E8000 }, new[] { 0x1FE000, 0x1F0000 } };

        private static int Block(byte[] rom, Game game, int season, int team)
        {
            int[] t = game == Game.Tsb3 ? Tsb2Tables[0] : Tsb2Tables[season - 1];
            return t[1] + (rom[t[0] + 2 * team] | rom[t[0] + 2 * team + 1] << 8);
        }

        /// <summary>
        /// What the sim bytes of each ROM do for one season, averaged over the teams, with no dice: the engine's
        /// chances (carrier pick over QB1, QB2 and the 10 -- runs landing on QB2 lose their credit, bug G3).
        /// </summary>
        public static string NoDice(Game game, int season, string[] roms)
        {
            int teams = game == Game.Tsb3 ? 30 : 28;
            var rows = new List<KeyValuePair<string, double[]>>();
            string[] labels = { "QB1 share of runs %", "runs lost to QB2 (G3) %", "RB1 share of runs %", "QB1 scramble chance %",
                                "ball carrier's rush value", "RBs share of targets %", "WRs share of targets %", "TEs share of targets %",
                                "top target share %", "DL share of sacks %", "LB share of sacks %", "LB share of INTs %",
                                "DB share of INTs %", "top tackle weight share %" };
            double[][] cols = new double[roms.Length][];
            for (int r = 0; r < roms.Length; r++)
            {
                byte[] rom = File.ReadAllBytes(roms[r]);
                double[] acc = new double[labels.Length];
                for (int k = 0; k < teams; k++)
                {
                    int s = Block(rom, game, season, k);
                    double[] carry = Walk(new[] { (double)rom[s], rom[s + 3] }.Concat(Enumerable.Range(0, 10).Select(i => (double)rom[s + 6 + 4 * i])).ToArray(), new[] { 2 });
                    double[] target = Walk(Enumerable.Range(0, 10).Select(i => (double)rom[s + 8 + 4 * i]).ToArray(), Enumerable.Range(2, 8).ToArray());
                    double[] sack = Walk(Enumerable.Range(0, 18).Select(i => (double)rom[s + 46 + 3 * i]).ToArray(), Enumerable.Range(0, 8).ToArray());
                    // the engine's INT walk and fallback (ShareKind.Int): DB3 first, leftover to NT2 .. RCB
                    double[] ints = ShareKind.Int.Chances(pos => rom[s + 47 + 3 * Array.IndexOf(Positions.Defense, pos)]);
                    double[] tackle = Enumerable.Range(0, 18).Select(i => (double)rom[s + 48 + 3 * i]).ToArray();
                    double carrier = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        if (i == 1)
                            continue; // QB2's runs aren't credited
                        int rushDigit = i == 0 ? rom[s + 1] : rom[s + 7 + 4 * (i - 2)]; // QB1's / the RB/WR/TE's
                        carrier += carry[i] * rom[game.RushTableOffset + Math.Min(15, rushDigit)];
                    }
                    double[] v = {
                        carry[0], carry[1], carry[2], rom[game.ScrambleTableOffset + (rom[s + 2] & 15)] / 256.0,
                        carrier / (1 - carry[1]), target.Take(4).Sum(), target.Skip(4).Take(4).Sum(), target.Skip(8).Sum(),
                        target.Max(), sack.Take(6).Sum(), sack.Skip(6).Take(5).Sum(), ints.Skip(6).Take(5).Sum(),
                        ints.Skip(11).Sum(), tackle.Sum() > 0 ? tackle.Max() / tackle.Sum() : 0 };
                    for (int i = 0; i < v.Length; i++)
                        acc[i] += v[i];
                }
                cols[r] = acc.Select((x, i) => (labels[i].StartsWith("ball") ? 1 : 100) * x / teams).ToArray();
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0,-28}", "");
            foreach (string r in roms) sb.AppendFormat("{0,16}", Last(Path.GetFileName(r), 15));
            sb.AppendLine();
            for (int i = 0; i < labels.Length; i++)
            {
                sb.AppendFormat("{0,-28}", labels[i]);
                foreach (double[] c in cols) sb.AppendFormat(CultureInfo.InvariantCulture, "{0,16:0.0}", c[i]);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static double[] Walk(double[] shares, int[] leftover)
        {
            double[] p = new double[shares.Length];
            double cum = 0;
            for (int i = 0; i < shares.Length; i++)
            {
                double take = Math.Max(0, Math.Min(shares[i], 256 - cum));
                p[i] += take / 256.0;
                cum += take;
            }
            double rest = Math.Max(0, 256 - cum) / 256.0;
            foreach (int i in leftover) p[i] += rest / leftover.Length;
            return p;
        }

        private static string Last(string s, int n) { return s.Length <= n ? s : s.Substring(s.Length - n); }

        // ---------------------------------------------------------------------------------------------

        /// <summary>One player's season line, by the slot he plays now (TSB3: through the save's roster IDs).</summary>
        private class Line
        {
            public int Slot;
            public int Att, Comp, PassYds, PassTd, IntThrown, RushAtt, RushYds, Rec, RecYds, Kr, KrYds, Sacks, Ints, Tackles;
        }

        private class SeasonTeam
        {
            public List<Line> Players = new List<Line>();
            public int FgMade, FgAtt, Punts, PuntYds, RushAllowed;
        }

        /// <summary>
        /// A TSB2 or TSB3 season from a battery save (layouts: SimTests\TSB2_study\TSB2_SRAM_Stats.txt,
        /// TSB3_study\TSB3_SRAM_Stats.txt). TSB3 keeps stats by player ID: each slot's player is found
        /// through the team's roster IDs at his original team and slot.
        /// </summary>
        private static List<SeasonTeam> LoadSeason(byte[] d, out string game)
        {
            string err;
            bool tsb2 = TSBTool.Tsb2SeasonStats.FromSram(d, out err) != null;
            bool tsb3 = !tsb2 && TSBTool.Tsb3SeasonStats.FromSram(d, out err) != null;
            if (!tsb2 && !tsb3)
                throw new ArgumentException("Not a TSB2 or TSB3 battery save with season stats: " + err);
            game = tsb2 ? "TSB2" : "TSB3";
            Func<int, int> u = o => d[o] | d[o + 1] << 8;
            Func<int, int> s16 = o => { int v = u(o); return v >= 0x8000 ? v - 0x10000 : v; };
            int teams = tsb2 ? 28 : 30, first = tsb2 ? 0x152A : 0x11C1;
            List<SeasonTeam> ret = new List<SeasonTeam>();
            for (int k = 0; k < teams; k++)
            {
                SeasonTeam t = new SeasonTeam();
                int rec = tsb2 ? 0x3A2 + k * 163 : 0x457 + k * 101 + 76;
                t.RushAllowed = u(rec + 9);
                for (int slot = 0; slot < 37; slot++)
                {
                    int team = k, orig = slot;
                    if (!tsb2)
                    {
                        int id = u(0x457 + k * 101 + 2 * slot) & 0xFFF;
                        team = id / 37;
                        orig = id % 37;
                    }
                    int o = first + team * 485;
                    Line l = new Line { Slot = slot };
                    if (orig < 2)
                    {
                        int q = o + orig * 13;
                        l.PassTd = d[q]; l.Att = u(q + 1); l.IntThrown = d[q + 3]; l.Comp = u(q + 4); l.PassYds = s16(q + 6);
                        l.RushAtt = u(q + 9); l.RushYds = s16(q + 11);
                    }
                    else if (orig < 12)
                    {
                        int q = o + 26 + (orig - 2) * 22;
                        l.Rec = u(q + 1); l.RecYds = s16(q + 3); l.RushAtt = u(q + 6); l.RushYds = s16(q + 8); l.Kr = d[q + 13]; l.KrYds = s16(q + 14);
                    }
                    else if (orig >= 17 && orig < 35)
                    {
                        int q = o + 246;
                        for (int j = 17; j < orig; j++) q += j < 28 ? 9 : 19;
                        l.Sacks = d[q]; l.Ints = d[q + 2]; l.Tackles = u(q + 6);
                    }
                    else if (orig == 35) { t.FgMade = d[o + 480]; t.FgAtt = d[o + 481]; }
                    else if (orig == 36) { t.Punts = d[o + 482]; t.PuntYds = u(o + 483); }
                    if (orig < 12 || (orig >= 17 && orig < 35))
                        t.Players.Add(l);
                }
                ret.Add(t);
            }
            return ret;
        }

        public static string Compare(string[] saves)
        {
            var columns = new List<List<KeyValuePair<string, double>>>();
            foreach (string path in saves)
            {
                string game;
                List<SeasonTeam> ts = LoadSeason(File.ReadAllBytes(path), out game);
                List<Line> all = ts.SelectMany(t => t.Players).ToList();
                Func<Func<Line, bool>, Func<Line, int>, int> sum = (f, g) => all.Where(f).Sum(g);
                Func<Line, bool> any = l => true;
                int n = ts.Count;
                int att = sum(any, l => l.Att), carries = sum(any, l => l.RushAtt), rec = sum(any, l => l.Rec);
                int sacks = sum(any, l => l.Sacks), ints = sum(any, l => l.Ints), thrown = sum(any, l => l.IntThrown);
                var c = new List<KeyValuePair<string, double>>();
                Action<string, double> add = (k, v) => c.Add(new KeyValuePair<string, double>(k, v));
                add("completion %", 100.0 * sum(any, l => l.Comp) / att);
                add("pass yds / att", (double)sum(any, l => l.PassYds) / att);
                add("pass TD / team", (double)sum(any, l => l.PassTd) / n);
                add("INT thrown / team", (double)thrown / n);
                add("INTs not credited (G1)", thrown - ints);
                add("sacks / team", (double)sacks / n);
                add("rush yds / carry", (double)sum(any, l => l.RushYds) / carries);
                add("rush yds not credited (G3)", ts.Sum(t => t.RushAllowed) - sum(any, l => l.RushYds));
                add("QB carries / team", (double)sum(l => l.Slot < 2, l => l.RushAtt) / n);
                add("RB1 share of carries %", 100.0 * sum(l => l.Slot == 2, l => l.RushAtt) / carries);
                add("RBs share of catches %", 100.0 * sum(l => l.Slot >= 2 && l.Slot < 6, l => l.Rec) / rec);
                add("WRs share of catches %", 100.0 * sum(l => l.Slot >= 6 && l.Slot < 10, l => l.Rec) / rec);
                add("TEs share of catches %", 100.0 * sum(l => l.Slot >= 10 && l.Slot < 12, l => l.Rec) / rec);
                add("yds / catch", (double)sum(any, l => l.RecYds) / rec);
                add("top receiver catches / team", ts.Average(t => t.Players.Where(l => l.Slot >= 2 && l.Slot < 12).Max(l => l.Rec)));
                add("DL share of sacks %", 100.0 * sum(l => l.Slot >= 17 && l.Slot < 23, l => l.Sacks) / sacks);
                add("LB share of sacks %", 100.0 * sum(l => l.Slot >= 23 && l.Slot < 28, l => l.Sacks) / sacks);
                add("DB share of INTs %", 100.0 * sum(l => l.Slot >= 28, l => l.Ints) / ints);
                add("LB share of INTs %", 100.0 * sum(l => l.Slot >= 23 && l.Slot < 28, l => l.Ints) / ints);
                add("top tackler / team", ts.Average(t => t.Players.Where(l => l.Slot >= 17).Max(l => l.Tackles)));
                add("FG %", 100.0 * ts.Sum(t => t.FgMade) / ts.Sum(t => t.FgAtt));
                add("punt avg", (double)ts.Sum(t => t.PuntYds) / ts.Sum(t => t.Punts));
                add("KR avg", (double)sum(any, l => l.KrYds) / Math.Max(1, sum(any, l => l.Kr)));
                columns.Add(c);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0,-30}", "");
            foreach (string p in saves) sb.AppendFormat("{0,16}", Last(Path.GetFileName(p), 15));
            sb.AppendLine();
            for (int i = 0; i < columns[0].Count; i++)
            {
                sb.AppendFormat("{0,-30}", columns[0][i].Key);
                foreach (var c in columns) sb.AppendFormat(CultureInfo.InvariantCulture, "{0,16:0.0}", c[i].Value);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Writes 'outRom': 'romPath' with TSBTool's Auto Update Sim run on the given seasons' rosters (sim bytes
        /// only), optionally the FIX_SIM_BUGS fix, and the SNES header checksum made right.
        /// </summary>
        public static void Apply(Game game, string romPath, string outRom, int[] seasons, bool fixSimBugs)
        {
            TSBTool2.TSB2Tool tool = Rosters.LoadTool(game, romPath);
            foreach (int season in seasons)
                TSBTool2.TsbSimAutoUpdater.UpdateRomSimData(tool, season, true);
            if (fixSimBugs)
                tool.FixSimBugs();
            byte[] rom = tool.OutputRom;
            FixChecksum(rom);
            File.WriteAllBytes(outRom, rom);
        }

        /// <summary>The LoROM header's checksum (0x7FDE) and its complement (0x7FDC).</summary>
        public static void FixChecksum(byte[] rom)
        {
            rom[0x7FDC] = 0xFF; rom[0x7FDD] = 0xFF; rom[0x7FDE] = 0; rom[0x7FDF] = 0;
            int sum = 0;
            foreach (byte b in rom) sum += b;
            sum &= 0xFFFF;
            rom[0x7FDC] = (byte)((sum ^ 0xFFFF) & 0xFF);
            rom[0x7FDD] = (byte)((sum ^ 0xFFFF) >> 8);
            rom[0x7FDE] = (byte)(sum & 0xFF);
            rom[0x7FDF] = (byte)(sum >> 8);
        }
    }
}
