using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TsbSimModel
{
    /// <summary>
    /// Fits a game's Auto Update Sim model to Tecmo's own sim bytes (its stock rosters), and reports how
    /// well it does on data it wasn't fitted on.
    ///
    /// 0-15 values (QB pass, rush, ...): least squares of the value on the rating steps plus a constant per
    ///   everyone / position group / roster slot. A straight-line fit pulls everyone toward the average, so
    ///   the fitted values may be stretched around their average to spread like Tecmo's (kept only when it
    ///   costs at most 3 points of 'within 1' accuracy; tackle weights always, and tuned so each team's top
    ///   tackler gets Tecmo's share of the weights -- they only count relative to each other).
    /// Shares (carry, target, sack, INT): least squares of log(share + 1) on the rating steps plus a constant
    ///   per slot, then calibrated: weight = (e^fit - 1) ^ spread * position group factor, the spread and
    ///   factors chosen (Nelder-Mead) so the chances the engine gives each player match what Tecmo's bytes
    ///   give -- team by team, league-wide per slot, and how much the top player gets.
    /// Held-out check: TSB2 fits on two stock seasons and checks the third, for each season; TSB3 (one stock
    ///   season) the same with 3 folds of 10 teams. The final model is then fitted on everything.
    /// </summary>
    public class Fitter
    {
        private readonly Game mGame;
        private readonly List<Team> mTeams;
        private readonly int[] mRushTable;
        private readonly StringBuilder mReport = new StringBuilder();

        public Fitter(Game game, List<Team> teams, int[] rushTable)
        {
            mGame = game;
            mTeams = teams;
            mRushTable = rushTable;
        }

        public string Report { get { return mReport.ToString(); } }

        private void W(string format, params object[] args)
        {
            mReport.AppendLine(args.Length == 0 ? format : string.Format(CultureInfo.InvariantCulture, format, args));
        }

        // ---------------------------------------------------------------------------------------------
        // 0-15 values

        private static List<Player> Samples(IEnumerable<Team> teams, string[] positions)
        {
            List<Player> rows = new List<Player>();
            foreach (Team t in teams)
                foreach (string pos in positions)
                    if (t.Players.ContainsKey(pos))
                        rows.Add(t.Players[pos]);
            return rows;
        }

        /// <summary>Least squares of the value on the rating steps and the constants.</summary>
        private static DigitModel FitDigit(List<Player> rows, DigitSpec spec, ConstKind kind)
        {
            DigitModel m = new DigitModel { Name = spec.Name, Positions = spec.Positions, Ratings = spec.Ratings, Const = kind };
            if (kind != ConstKind.Single)
            {
                SortedSet<string> keys = new SortedSet<string>(StringComparer.Ordinal);
                foreach (Player p in rows)
                    keys.Add(DigitSpec.ConstKey(p.Position, kind));
                m.Groups = keys.ToArray();
            }
            double[][] x = new double[rows.Count][];
            double[] y = new double[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                Player p = rows[i];
                List<double> r = new List<double>();
                foreach (string a in spec.Ratings)
                    r.Add(p.Level[a]);
                if (kind == ConstKind.Single)
                    r.Add(1.0);
                else
                    foreach (string g in m.Groups)
                        r.Add(DigitSpec.ConstKey(p.Position, kind) == g ? 1.0 : 0.0);
                x[i] = r.ToArray();
                y[i] = spec.Actual(p.Sim);
            }
            m.Coef = LeastSquares.Solve(x, y);
            return m;
        }

        /// <summary>The fit, with Center = the fitted values' average and Stretch = what spreads them like Tecmo's.</summary>
        private DigitModel FitDigitStretched(List<Team> teams, DigitSpec spec, ConstKind kind)
        {
            List<Player> rows = Samples(teams, spec.Positions);
            DigitModel m = FitDigit(rows, spec, kind);
            double[] raw = rows.Select(p => m.Raw(p)).ToArray();
            double[] y = rows.Select(p => (double)spec.Actual(p.Sim)).ToArray();
            double sdRaw = StdDev(raw);
            m.Center = raw.Average();
            m.Stretch = sdRaw > 0 ? StdDev(y) / sdRaw : 1.0;
            if (spec.Name == "tackle")
                m.Stretch = TuneTackleStretch(teams, m);
            return m;
        }

        private static int ValueWith(DigitModel m, Player p, double stretch)
        {
            return DigitModel.Clamp15((int)Math.Round(m.Center + stretch * (m.Raw(p) - m.Center)));
        }

        /// <summary>
        /// Tackle weights only count relative to each other: stretch them (1.0 .. 2.0 times the spread-matching
        /// stretch) until each team's top tackler gets Tecmo's share of the team's weights.
        /// </summary>
        private double TuneTackleStretch(List<Team> teams, DigitModel m)
        {
            double goal = TopTackleShare(teams, (t, pos) => t.Players[pos].Sim[2]);
            double best = m.Stretch;
            double bestDiff = double.NaN;
            for (int i = 0; i <= 50; i++)
            {
                double st = m.Stretch * (1.0 + i * 0.02);
                double got = TopTackleShare(teams, (t, pos) => ValueWith(m, t.Players[pos], st));
                if (double.IsNaN(bestDiff) || Math.Abs(got - goal) < bestDiff - 1e-9)
                {
                    best = st;
                    bestDiff = Math.Abs(got - goal);
                }
            }
            return best;
        }

        private static double TopTackleShare(IEnumerable<Team> teams, Func<Team, string, int> tackle)
        {
            List<double> shares = new List<double>();
            foreach (Team t in teams)
            {
                double sum = 0, max = 0;
                foreach (string pos in Positions.Defense)
                {
                    int d = tackle(t, pos);
                    sum += d;
                    max = Math.Max(max, d);
                }
                shares.Add(sum > 0 ? max / sum : 0.0);
            }
            return 100 * shares.Average();
        }

        private double CarrierRushValue(IEnumerable<Team> teams, Func<Team, string, int> rush)
        {
            List<double> vals = new List<double>();
            foreach (Team t in teams)
            {
                Team team = t;
                double[] p = ShareKind.Carry.Chances(pos => team.Players[pos].Sim[0]);
                double v = 0;
                for (int i = 0; i < Positions.CarryPool.Length; i++)
                    v += p[i] * mRushTable[rush(t, Positions.CarryPool[i])];
                vals.Add(v);
            }
            return vals.Average();
        }

        private static double StdDev(double[] v)
        {
            double mean = v.Average();
            double s = 0;
            foreach (double x in v) s += (x - mean) * (x - mean);
            return Math.Sqrt(s / v.Length);
        }

        /// <summary>Exact %, within-1 %, average miss.</summary>
        private static double[] Score(List<int[]> pairs)
        {
            double exact = 0, within = 0, miss = 0;
            foreach (int[] p in pairs)
            {
                int d = Math.Abs(p[0] - p[1]);
                if (d == 0) exact++;
                if (d <= 1) within++;
                miss += d;
            }
            return new[] { 100 * exact / pairs.Count, 100 * within / pairs.Count, miss / pairs.Count };
        }

        // ---------------------------------------------------------------------------------------------
        // Shares

        /// <summary>Least squares of log(share + 1) on the rating steps and a constant per pool slot.</summary>
        private static double[] FitShare(IEnumerable<Team> teams, ShareKind kind, string[] ratings)
        {
            List<double[]> x = new List<double[]>();
            List<double> y = new List<double>();
            foreach (Team t in teams)
                foreach (string pos in kind.Pool)
                {
                    Player p = t.Players[pos];
                    List<double> r = new List<double>();
                    foreach (string a in ratings)
                        r.Add(p.Level[a]);
                    foreach (string s in kind.Pool)
                        r.Add(s == pos ? 1.0 : 0.0);
                    x.Add(r.ToArray());
                    y.Add(Math.Log(p.Sim[kind.SimByte] + 1.0));
                }
            return LeastSquares.Solve(x.ToArray(), y.ToArray());
        }

        private static ShareModel Uncalibrated(ShareKind kind, double[] coef)
        {
            return new ShareModel { Kind = kind, Positions = kind.Pool, Ratings = ShareSpec.Ratings(kind), Coef = coef };
        }

        private static double[] Floats(double[] w)
        {
            double s = w.Sum();
            return s > 0 ? w.Select(v => v * 255.0 / s).ToArray() : new double[w.Length];
        }

        private static double[] TecmoChances(Team t, ShareKind kind)
        {
            return kind.Chances(pos => t.Players[pos].Sim[kind.SimByte]);
        }

        private static double[] ChancesOf(ShareKind kind, Dictionary<string, int> shares)
        {
            return kind.Chances(pos => shares[pos]);
        }

        /// <summary>Spread and the non-fixed groups' factors (exp of the searched values).</summary>
        private static void ParamsTo(ShareKind kind, double[] v, out double spread, out Dictionary<string, double> factors)
        {
            spread = v[0];
            factors = new Dictionary<string, double>();
            int i = 1;
            foreach (KeyValuePair<string, string[]> g in ShareSpec.Groups(kind))
                if (g.Key != ShareSpec.FixedGroup(kind))
                    factors[g.Key] = Math.Exp(v[i++]);
        }

        /// <summary>
        /// Calibration loss: per-team 'differs' + league-wide per-slot difference + how far the top player's
        /// average chance is from Tecmo's (all in chance units).
        /// </summary>
        private static double ShareLoss(ShareKind kind, List<Team> teams, Dictionary<string, double[]> baseW,
            Dictionary<string, double[]> truth, double[] v)
        {
            double spread;
            Dictionary<string, double> factors;
            ParamsTo(kind, v, out spread, out factors);
            if (spread <= 0.05)
                return 9.0;
            int n = kind.Pool.Length;
            double tvSum = 0, topM = 0, topT = 0;
            double[] sumM = new double[n], sumT = new double[n];
            foreach (Team t in teams)
            {
                double[] b = baseW[t.Key];
                double[] w = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double f;
                    w[i] = Math.Pow(b[i], spread) * (factors.TryGetValue(ShareSpec.GroupIn(kind, kind.Pool[i]), out f) ? f : 1.0);
                }
                double[] fl = Floats(w);
                double[] m = kind.Chances(pos => fl[Array.IndexOf(kind.Pool, pos)]);
                double[] tr = truth[t.Key];
                tvSum += ShareKind.Differs(m, tr);
                for (int i = 0; i < n; i++) { sumM[i] += m[i]; sumT[i] += tr[i]; }
                topM += m.Max();
                topT += tr.Max();
            }
            double league = 0;
            for (int i = 0; i < n; i++) league += Math.Abs(sumM[i] - sumT[i]);
            league = 0.5 * league / teams.Count;
            double top = Math.Abs(topM - topT) / teams.Count;
            return tvSum / teams.Count + league + top;
        }

        private static ShareModel CalibrateShare(List<Team> teams, ShareKind kind)
        {
            string[] ratings = ShareSpec.Ratings(kind);
            ShareModel m = Uncalibrated(kind, FitShare(teams, kind, ratings));
            Dictionary<string, double[]> truth = new Dictionary<string, double[]>();
            Dictionary<string, double[]> baseW = new Dictionary<string, double[]>();
            foreach (Team t in teams)
            {
                truth[t.Key] = TecmoChances(t, kind);
                baseW[t.Key] = kind.Pool.Select(pos => m.BaseWeight(t.Players[pos], pos)).ToArray();
            }
            double[] x0 = new double[ShareSpec.Groups(kind).Count];
            x0[0] = 1.0;
            double[] v = NelderMead.Minimize(p => ShareLoss(kind, teams, baseW, truth, p), x0);
            double spread;
            Dictionary<string, double> factors;
            ParamsTo(kind, v, out spread, out factors);
            m.Spread = spread;
            m.GroupFactors = factors;
            return m;
        }

        /// <summary>League-wide chance per position group, and the top player's average chance.</summary>
        private static List<KeyValuePair<string, double>> Shapes(List<Team> teams, Func<ShareKind, Team, Dictionary<string, int>> shares)
        {
            List<KeyValuePair<string, double>> ret = new List<KeyValuePair<string, double>>();
            foreach (ShareKind kind in ShareKind.All)
            {
                double[] tot = new double[kind.Pool.Length];
                double top = 0;
                foreach (Team t in teams)
                {
                    double[] p = ChancesOf(kind, shares(kind, t));
                    for (int i = 0; i < tot.Length; i++) tot[i] += p[i];
                    top += p.Max();
                }
                foreach (KeyValuePair<string, string[]> g in ShareSpec.Groups(kind))
                {
                    double s = 0;
                    foreach (string pos in g.Value) s += tot[Array.IndexOf(kind.Pool, pos)];
                    ret.Add(new KeyValuePair<string, double>(kind.Name + " " + g.Key + " %", 100 * s / teams.Count));
                }
                ret.Add(new KeyValuePair<string, double>(kind.Name + " top player %", 100 * top / teams.Count));
            }
            return ret;
        }

        private static Dictionary<string, int> TecmoShares(ShareKind kind, Team t)
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            foreach (string pos in kind.Pool) d[pos] = t.Players[pos].Sim[kind.SimByte];
            return d;
        }

        // ---------------------------------------------------------------------------------------------

        /// <summary>Fits the model (held-out checks into the report), and returns it as the model file has it (rounded).</summary>
        public FittedModel Fit()
        {
            int[] groups = mTeams.Select(t => t.Group).Distinct().OrderBy(g => g).ToArray();
            string held = mGame.FoldsByTeam ? "fold of 10 teams" : "season";
            W("{0} Auto Update Sim model: {1} team-seasons; every number below is on a {2} the model was NOT tuned on.",
                mGame.Name, mTeams.Count, held);
            W("");

            // ---- shares
            W("SHARES  'differs' = share of plays going to a different player than with Tecmo's bytes, per team");
            W("                first fit   calibrated");
            Dictionary<string, Dictionary<string, int>> heldCal = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> heldFirst = new Dictionary<string, Dictionary<string, int>>();
            foreach (ShareKind kind in ShareKind.All)
            {
                List<double> d1 = new List<double>(), d2 = new List<double>();
                foreach (int test in groups)
                {
                    List<Team> train = mTeams.Where(t => t.Group != test).ToList();
                    ShareModel cal = CalibrateShare(train, kind);
                    ShareModel first = Uncalibrated(kind, FitShare(train, kind, ShareSpec.Ratings(kind)));
                    foreach (Team t in mTeams.Where(t => t.Group == test))
                    {
                        double[] truth = TecmoChances(t, kind);
                        Dictionary<string, int> c = cal.Shares(t), f = first.Shares(t);
                        heldCal[kind.Name + "|" + t.Key] = c;
                        heldFirst[kind.Name + "|" + t.Key] = f;
                        d2.Add(ShareKind.Differs(ChancesOf(kind, c), truth));
                        d1.Add(ShareKind.Differs(ChancesOf(kind, f), truth));
                    }
                }
                W("  {0,-8}        {1,5:0.0}%      {2,5:0.0}%", kind.Name, 100 * d1.Average(), 100 * d2.Average());
            }
            W("");
            var tecmo = Shapes(mTeams, TecmoShares);
            var fs = Shapes(mTeams, (k, t) => heldFirst[k.Name + "|" + t.Key]);
            var cs = Shapes(mTeams, (k, t) => heldCal[k.Name + "|" + t.Key]);
            W("LEAGUE SHAPE (all teams, no dice): chance a carry/target/sack/INT goes to each group");
            W("                        Tecmo   first fit   calibrated");
            for (int i = 0; i < tecmo.Count; i++)
                W("  {0,-20}  {1,5:0.0}      {2,5:0.0}       {3,5:0.0}", tecmo[i].Key, tecmo[i].Value, fs[i].Value, cs[i].Value);
            W("");

            // ---- 0-15 values: constants per group or slot, plain or stretched
            W("VALUES (0-15), held out: exact / within 1 / spread (Tecmo's spread in the last column)");
            W("                   by group, plain       by slot, plain        (by slot,) stretched | Tecmo");
            Dictionary<string, KeyValuePair<ConstKind, bool>> choice = new Dictionary<string, KeyValuePair<ConstKind, bool>>();
            Dictionary<string, Dictionary<string, int>> heldDigits = new Dictionary<string, Dictionary<string, int>>();
            foreach (DigitSpec spec in DigitSpec.All)
            {
                var variants = new List<KeyValuePair<ConstKind, bool>> { new KeyValuePair<ConstKind, bool>(spec.DefaultConst, false) };
                if (spec.DefaultConst != ConstKind.Single)
                {
                    variants.Add(new KeyValuePair<ConstKind, bool>(ConstKind.Slot, false));
                    variants.Add(new KeyValuePair<ConstKind, bool>(ConstKind.Slot, true));
                }
                else
                    variants.Add(new KeyValuePair<ConstKind, bool>(spec.DefaultConst, true));

                var scores = new Dictionary<KeyValuePair<ConstKind, bool>, double[]>();
                var spreads = new Dictionary<KeyValuePair<ConstKind, bool>, double>();
                var preds = new Dictionary<KeyValuePair<ConstKind, bool>, Dictionary<string, int>>();
                double tecmoSpread = 0;
                foreach (var v in variants)
                {
                    List<int[]> pairs = new List<int[]>();
                    List<double> vals = new List<double>(), truthVals = new List<double>();
                    Dictionary<string, int> pred = new Dictionary<string, int>();
                    foreach (int test in groups)
                    {
                        DigitModel m = FitDigitStretched(mTeams.Where(t => t.Group != test).ToList(), spec, v.Key);
                        foreach (Team t in mTeams.Where(t => t.Group == test))
                            foreach (string pos in spec.Positions)
                            {
                                if (!t.Players.ContainsKey(pos)) continue;
                                Player p = t.Players[pos];
                                int val = ValueWith(m, p, v.Value ? m.Stretch : 1.0);
                                int a = spec.Actual(p.Sim);
                                pairs.Add(new[] { val, a });
                                vals.Add(val);
                                truthVals.Add(a);
                                pred[t.Key + "|" + pos] = val;
                            }
                    }
                    scores[v] = Score(pairs);
                    spreads[v] = StdDev(vals.ToArray());
                    preds[v] = pred;
                    tecmoSpread = StdDev(truthVals.ToArray());
                }
                // the most 'within 1' unstretched; stretched (same constants) if it costs at most 3 points; tackle always
                var plain = variants.Where(v => !v.Value).OrderByDescending(v => scores[v][1]).First();
                var best = plain;
                foreach (var v in variants)
                    if (v.Value && v.Key == plain.Key && (spec.Name == "tackle" || scores[v][1] >= scores[plain][1] - 3.0))
                        best = v;
                choice[spec.Name] = best;
                heldDigits[spec.Name] = preds[best];

                List<string> cells = new List<string>();
                var columns = spec.DefaultConst != ConstKind.Single
                    ? new KeyValuePair<ConstKind, bool>?[] { variants[0], variants[1], variants[2] }
                    : new KeyValuePair<ConstKind, bool>?[] { variants[0], null, variants[1] };
                foreach (var c in columns)
                {
                    if (c == null) { cells.Add(new string(' ', 17)); continue; }
                    double[] s = scores[c.Value];
                    cells.Add(string.Format(CultureInfo.InvariantCulture, "{0,5:0.0} {1,5:0.0} {2,4:0.00}{3}", s[0], s[1], spreads[c.Value], c.Value.Equals(best) ? "*" : " "));
                }
                W("  {0,-12} {1}    {2}    {3}   | {4:0.00}", spec.Name, cells[0], cells[1], cells[2], tecmoSpread);
            }
            W("  (* = used. 'by slot' = a constant per roster slot (RB1, RB2 .. / RE, RE2 ..) instead of per group)");
            W("");
            double tecRush = CarrierRushValue(mTeams, (t, pos) => t.Players[pos].Sim[1]);
            double modRush = CarrierRushValue(mTeams, (t, pos) => heldDigits[pos.StartsWith("QB") ? "QB rush" : "rush"][t.Key + "|" + pos]);
            double tecTkl = TopTackleShare(mTeams, (t, pos) => t.Players[pos].Sim[2]);
            double modTkl = TopTackleShare(mTeams, (t, pos) => heldDigits["tackle"][t.Key + "|" + pos]);
            W("  ball carrier's average rush value:          Tecmo {0:0.0}   model {1:0.0}", tecRush, modRush);
            W("  top tackler's share of the tackle weights:  Tecmo {0:0.0}%  model {1:0.0}%", tecTkl, modTkl);
            W("");

            // ---- the final model, on all teams, rounded as the model file has it
            FittedModel model = new FittedModel();
            foreach (DigitSpec spec in DigitSpec.All)
            {
                var c = choice[spec.Name];
                DigitModel m = FitDigitStretched(mTeams, spec, c.Key);
                if (!c.Value)
                    m.Stretch = 1.0;
                m.Coef = m.Coef.Select(ModelFile.Round4).ToArray();
                m.Stretch = ModelFile.Round4(m.Stretch);
                m.Center = ModelFile.Round4(m.Center);
                model.Digits.Add(m);
            }
            foreach (ShareKind kind in ShareKind.All)
            {
                ShareModel s = CalibrateShare(mTeams, kind);
                s.Coef = s.Coef.Select(ModelFile.Round4).ToArray();
                s.Spread = ModelFile.Round4(s.Spread);
                s.GroupFactors = s.GroupFactors.ToDictionary(kv => kv.Key, kv => ModelFile.Round4(kv.Value));
                model.Shares.Add(s);
                W("  {0,-8} spread {1:0.00}  group factors {2}", kind.Name, s.Spread,
                    string.Join(", ", s.GroupFactors.Select(kv => string.Format(CultureInfo.InvariantCulture, "{0} {1:0.00}", kv.Key, kv.Value)).ToArray()));
            }
            FitTeamDefense(model);
            return model;
        }

        // ---------------------------------------------------------------------------------------------
        // team SimData (TSB2: rush defense, pass defense)

        /// <summary>
        /// Fits the team values (when the game has them) into 'model', with their held-out check in the report.
        /// Tecmo set TSB2's team byte mostly from each team's real season, so the starters' ratings explain it
        /// only partly; the report compares the model with giving every team the league average.
        /// </summary>
        public void FitTeamDefense(FittedModel model)
        {
            model.TeamDefense.Clear();
            if (!mGame.HasTeamDefense)
                return;
            List<Team> teams = mTeams.Where(t => TeamDefenseSpec.HasStarters(t) && t.RushD >= 0).ToList();
            int[] groups = teams.Select(t => t.Group).Distinct().OrderBy(g => g).ToArray();
            W("");
            W("TEAM SIMDATA (rush D, pass D: the team byte), {0} team-seasons, held out by season:", teams.Count);
            W("  each starter scored with Tecmonster's rating mix; group scores (DL: RE NT LE; LB: the 4 LBs; DB: RCB LCB");
            W("  FS SS) weighted -- fitted (none negative) or equal -- and stretched to Tecmo's league spread.");
            W("  'off' = average distance from Tecmo's digit, 'w1' = within 1, 'r' = correlation.");
            W("  Tecmo set the byte mostly from each team's real season: ratings explain it only partly.");
            foreach (string name in TeamDefenseSpec.Names)
            {
                double[] truthAll = teams.Select(t => (double)TeamDefenseSpec.Actual(t, name)).ToArray();
                W("  {0}  (Tecmo: average {1:0.0}, spread {2:0.0})", name, truthAll.Average(), StdDev(truthAll));
                List<int[]> constant = new List<int[]>();
                foreach (int test in groups)
                {
                    int avg = (int)Math.Round(teams.Where(t => t.Group != test).Average(t => (double)TeamDefenseSpec.Actual(t, name)));
                    foreach (Team t in teams.Where(t => t.Group == test))
                        constant.Add(new[] { avg, TeamDefenseSpec.Actual(t, name) });
                }
                double[] cs = TeamScore(constant);
                W("    {0,-30}  off {1:0.00}  w1 {2,3:0}%", "every team the league average", cs[0], cs[1]);
                bool bestEqual = false;
                double bestOff = double.MaxValue;
                foreach (bool equal in new[] { false, true })
                {
                    List<int[]> pairs = new List<int[]>();
                    foreach (int test in groups)
                    {
                        TeamDefenseModel m = FitTeamValue(teams.Where(t => t.Group != test).ToList(), name, equal);
                        foreach (Team t in teams.Where(t => t.Group == test))
                            pairs.Add(new[] { m.Value(t), TeamDefenseSpec.Actual(t, name) });
                    }
                    double[] sc = TeamScore(pairs);
                    W("    {0,-30}  off {1:0.00}  w1 {2,3:0}%  r {3:0.00}  spread {4:0.0}",
                        equal ? "equal group weights" : "fitted group weights", sc[0], sc[1], sc[2], sc[3]);
                    if (sc[0] < bestOff - 1e-9)
                    {
                        bestOff = sc[0];
                        bestEqual = equal;
                    }
                }
                TeamDefenseModel final = FitTeamValue(teams, name, bestEqual);
                final.Coef = final.Coef.Select(ModelFile.Round4).ToArray();
                final.Stretch = ModelFile.Round4(final.Stretch);
                final.Center = ModelFile.Round4(final.Center);
                model.TeamDefense.Add(final);
                W("    used: {0} group weights -- DL {1:0.00}, LB {2:0.00}, DB {3:0.00}; stretch {4:0.00}",
                    bestEqual ? "equal" : "fitted", final.Coef[0], final.Coef[1], final.Coef[2], final.Stretch);
            }
        }

        /// <summary>
        /// Least squares of Tecmo's digit on the group scores plus a constant -- with 'equal', on their average
        /// (one weight shared by the groups) -- with no negative weight (the most negative is dropped and the rest
        /// refitted until none is), then stretched around Tecmo's average to Tecmo's spread.
        /// </summary>
        private static TeamDefenseModel FitTeamValue(List<Team> teams, string name, bool equal)
        {
            string[][] mixR = TeamDefenseSpec.MixRatings(name);
            double[][] mixW = TeamDefenseSpec.MixWeights(name);
            int groupCount = TeamDefenseSpec.GroupSlots.Length;
            double[][] scores = teams.Select(t => TeamDefenseModel.GroupScores(t, mixR, mixW)).ToArray();
            double[][] features = equal ? scores.Select(s => new[] { s.Average() }).ToArray() : scores;
            double[] y = teams.Select(t => (double)TeamDefenseSpec.Actual(t, name)).ToArray();
            int n = features[0].Length;
            double[] w = new double[n];
            double constant = y.Average();
            List<int> active = Enumerable.Range(0, n).ToList();
            while (active.Count > 0)
            {
                double[][] x = features.Select(f => active.Select(i => f[i]).Concat(new[] { 1.0 }).ToArray()).ToArray();
                double[] b = LeastSquares.Solve(x, y);
                int worst = -1;
                for (int k = 0; k < active.Count; k++)
                    if (b[k] < 0 && (worst < 0 || b[k] < b[worst]))
                        worst = k;
                if (worst < 0)
                {
                    for (int k = 0; k < active.Count; k++)
                        w[active[k]] = b[k];
                    constant = b[active.Count];
                    break;
                }
                active.RemoveAt(worst);
            }
            double[] coef = new double[groupCount + 1];
            for (int g = 0; g < groupCount; g++)
                coef[g] = equal ? w[0] / groupCount : w[g];
            coef[groupCount] = constant;
            TeamDefenseModel m = new TeamDefenseModel { Name = name, MixRatings = mixR, MixWeights = mixW, Coef = coef };
            double[] raw = teams.Select(t => m.Raw(t)).ToArray();
            m.Center = y.Average();
            double rawSpread = StdDev(raw);
            m.Stretch = rawSpread > 0 ? StdDev(y) / rawSpread : 1.0;
            return m;
        }

        /// <summary>[average distance, % within 1, correlation, spread of the model's values] of (model, Tecmo) pairs.</summary>
        private static double[] TeamScore(List<int[]> pairs)
        {
            double[] a = pairs.Select(p => (double)p[0]).ToArray(), b = pairs.Select(p => (double)p[1]).ToArray();
            double off = pairs.Average(p => Math.Abs(p[0] - p[1]));
            double w1 = 100.0 * pairs.Count(p => Math.Abs(p[0] - p[1]) <= 1) / pairs.Count;
            double ma = a.Average(), mb = b.Average(), sab = 0, saa = 0, sbb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sab += (a[i] - ma) * (b[i] - mb);
                saa += (a[i] - ma) * (a[i] - ma);
                sbb += (b[i] - mb) * (b[i] - mb);
            }
            double r = saa > 0 && sbb > 0 ? sab / Math.Sqrt(saa * sbb) : 0;
            return new[] { off, w1, r, StdDev(a) };
        }
    }
}
