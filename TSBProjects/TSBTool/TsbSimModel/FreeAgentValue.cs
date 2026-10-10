using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TsbSimModel
{
    /// <summary>
    /// TSB3's free agent value: the last digit of a player's Face (byte 3 of his ratings record, low nibble; the
    /// first digit is the skin tone). TSB3's team defense adds it up over the starting front seven (rush) and the
    /// front seven plus the 4 DBs (pass) -- SimTests\TSB3_study\TSB3_HiddenRating_results.txt. Tecmo set it by
    /// hand, like an overall rating (stars far above their ratings); the Superstar Editor makes it for created
    /// players as a plain average of some ratings, which is far below Tecmo's values. The model fits Tecmo's
    /// values per position group, as the old FAP_Formulas.txt grouped them.
    /// </summary>
    public static class FreeAgentValueSpec
    {
        /// <summary>Position groups: name, slots, and the ratings the old FAP formula used (a candidate set).</summary>
        public static readonly string[] Groups = { "QB", "RB", "WR", "TE", "OL", "DL", "LB", "CB", "S", "K", "P" };

        public static string[] Slots(string group)
        {
            switch (group)
            {
                case "QB": return new[] { "QB1", "QB2" };
                case "RB": return new[] { "RB1", "RB2", "RB3", "RB4" };
                case "WR": return new[] { "WR1", "WR2", "WR3", "WR4" };
                case "TE": return new[] { "TE1", "TE2" };
                case "OL": return new[] { "C", "LG", "RG", "LT", "RT" };
                case "DL": return new[] { "RE", "NT", "LE", "RE2", "NT2", "LE2" };
                case "LB": return new[] { "ROLB", "RILB", "LILB", "LOLB", "LB5" };
                case "CB": return new[] { "RCB", "LCB", "DB1", "DB2" };
                case "S": return new[] { "FS", "SS", "DB3" };
                case "K": return new[] { "K" };
                default: return new[] { "P" };
            }
        }

        public static string[] OldFormulaRatings(string group)
        {
            switch (group)
            {
                case "QB": return new[] { "PS", "PC", "AR" };
                case "RB": return new[] { "MS", "BC", "RC" };
                case "WR": return new[] { "MS", "RC" };
                case "TE": return new[] { "MS", "RC" };
                case "OL": return new[] { "HP" };
                case "DL": return new[] { "MS", "HP" };
                case "LB": return new[] { "MS", "HP" };
                case "CB": return new[] { "MS", "PI" };
                case "S": return new[] { "MS", "PI", "HP" };
                case "K": return new[] { "KA" };
                default: return new[] { "KP" };
            }
        }

        public static string GroupOf(string pos)
        {
            foreach (string g in Groups)
                if (Array.IndexOf(Slots(g), pos) > -1)
                    return g;
            return null;
        }
    }

    /// <summary>One player line's free agent value and ratings (as 1..16 steps), and which team (text order) he's on.</summary>
    public class FreeAgentRow
    {
        public int TeamIndex;
        public string Team, Position;
        public Dictionary<string, int> Level = new Dictionary<string, int>();
        public int Value;
    }

    /// <summary>
    /// A fitted free agent value for a position group: value = round(sum of coef * rating step + constant),
    /// clamped to 0..15; no coefficient is negative (better ratings never lower it).
    /// </summary>
    public class FreeAgentValueModel
    {
        public string Group;
        public string[] Ratings;
        public double[] Coef;     // ratings, then the constant

        public int Value(Dictionary<string, int> level)
        {
            double s = Coef[Ratings.Length];
            for (int i = 0; i < Ratings.Length; i++)
                s += Coef[i] * level[Ratings[i]];
            return DigitModel.Clamp15((int)Math.Round(s));
        }
    }

    public static class FreeAgentValues
    {
        // "C,jerry FONTENOT,Face=0x06,#67,69,50,50,44,38,56," / "QB1,..,Face=0x07,#4,69,..,56,[02,01,81]"
        private static readonly Regex Line = new Regex(@"^(\w+),[^,]*,Face=0x([0-9a-fA-F]+),#\d+,([\d,]+?),?(\[.*)?$");
        private static readonly Regex TeamLine = new Regex(@"^TEAM\s*=\s*(\w+)");

        /// <summary>The ratings on a TSB3 line of 'pos', in order (linemen too).</summary>
        public static string[] RatingsFor(Game game, string pos)
        {
            string[] r = game.RatingsFor(pos);
            if (r != null)
                return r;
            return Array.IndexOf(FreeAgentValueSpec.Slots("OL"), pos) > -1 ? game.LinemanRatings : null;
        }

        /// <summary>Every player line of the text with its free agent value (all teams, free agents and all-time too).</summary>
        public static List<FreeAgentRow> Parse(Game game, string text)
        {
            List<FreeAgentRow> rows = new List<FreeAgentRow>();
            string team = null;
            int teamIndex = -1;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                Match t = TeamLine.Match(line);
                if (t.Success)
                {
                    team = t.Groups[1].Value;
                    teamIndex++;
                    continue;
                }
                FreeAgentRow row = ParseLine(game, line);
                if (row == null || team == null)
                    continue;
                row.Team = team;
                row.TeamIndex = teamIndex;
                rows.Add(row);
            }
            return rows;
        }

        public static FreeAgentRow ParseLine(Game game, string line)
        {
            Match m = Line.Match(line);
            if (!m.Success)
                return null;
            string pos = m.Groups[1].Value;
            string[] names = RatingsFor(game, pos);
            if (names == null || FreeAgentValueSpec.GroupOf(pos) == null)
                return null;
            string[] vals = m.Groups[3].Value.Split(',');
            if (vals.Length != names.Length)
                return null;
            FreeAgentRow row = new FreeAgentRow { Position = pos, Value = Convert.ToInt32(m.Groups[2].Value, 16) & 15 };
            for (int i = 0; i < names.Length; i++)
                row.Level[names[i]] = Rosters.Level(int.Parse(vals[i]));
            return row;
        }

        /// <summary>'line' with its free agent value (Face's last digit) worked out by 'model'; other lines as they are.</summary>
        public static string Update(Game game, FittedModel model, string line)
        {
            FreeAgentRow row = ParseLine(game, line);
            if (row == null)
                return line;
            FreeAgentValueModel m = model.FreeAgentValue.Find(x => x.Group == FreeAgentValueSpec.GroupOf(row.Position));
            if (m == null)
                return line;
            Match face = Regex.Match(line, @"Face=0x[0-9a-fA-F]*([0-9a-fA-F]),");
            int at = face.Groups[1].Index;
            return line.Substring(0, at) + m.Value(row.Level).ToString("x") + line.Substring(at + 1);
        }

        /// <summary>
        /// Fits the free agent value per position group to Tecmo's (all the text's players), choosing per group
        /// between all of the line's ratings and the old formula's ratings by a held-out check (3 folds of teams:
        /// 0, 3, 6 .. / 1, 4, 7 .. / 2, 5, 8 ..). Returns the report.
        /// </summary>
        public static string Fit(Game game, string text, FittedModel model)
        {
            StringBuilder report = new StringBuilder();
            Action<string, object[]> w = (f, a) => report.AppendLine(a.Length == 0 ? f : string.Format(CultureInfo.InvariantCulture, f, a));
            List<FreeAgentRow> rows = Parse(game, text);
            model.FreeAgentValue.Clear();
            w("", new object[0]);
            w("FREE AGENT VALUE (Face's last digit; TSB3's team defense adds it up), {0} players, held out by thirds of teams:", new object[] { rows.Count });
            w("  least squares on rating steps (no negative weight) per position group; the ratings: all of the line's, or the", new object[0]);
            w("  old FAP formula's -- whichever is closer held out. 'exact' / 'w1' = within 1 / 'off' = average distance.", new object[0]);
            w("  group  players   all ratings: exact  w1   off  | old formula's: exact  w1   off   | used", new object[0]);
            Dictionary<FreeAgentRow, int> held = new Dictionary<FreeAgentRow, int>();
            foreach (string g in FreeAgentValueSpec.Groups)
            {
                List<FreeAgentRow> grp = rows.Where(r => FreeAgentValueSpec.GroupOf(r.Position) == g).ToList();
                string[] all = RatingsFor(game, FreeAgentValueSpec.Slots(g)[0]);
                string[] old = FreeAgentValueSpec.OldFormulaRatings(g);
                double[][] scores = new double[2][];
                Dictionary<FreeAgentRow, int>[] preds = { new Dictionary<FreeAgentRow, int>(), new Dictionary<FreeAgentRow, int>() };
                for (int c = 0; c < 2; c++)
                {
                    string[] ratings = c == 0 ? all : old;
                    for (int fold = 0; fold < 3; fold++)
                    {
                        FreeAgentValueModel m = FitGroup(grp.Where(r => r.TeamIndex % 3 != fold).ToList(), g, ratings);
                        foreach (FreeAgentRow r in grp.Where(r => r.TeamIndex % 3 == fold))
                            preds[c][r] = m.Value(r.Level);
                    }
                    scores[c] = Score(grp.Select(r => new[] { preds[c][r], r.Value }).ToList());
                }
                int best = scores[1][2] < scores[0][2] - 1e-9 ? 1 : 0;
                foreach (FreeAgentRow r in grp)
                    held[r] = preds[best][r];
                FreeAgentValueModel final = FitGroup(grp, g, best == 0 ? all : old);
                final.Coef = final.Coef.Select(ModelFile.Round4).ToArray();
                model.FreeAgentValue.Add(final);
                w("  {0,-5}  {1,5}     {2,16:0}% {3,3:0}% {4,5:0.00}  | {5,17:0}% {6,3:0}% {7,5:0.00}   | {8}",
                    new object[] { g, grp.Count, scores[0][0], scores[0][1], scores[0][2], scores[1][0], scores[1][1], scores[1][2], best == 0 ? "all" : "old formula's" });
            }
            double[] s = Score(rows.Select(r => new[] { held[r], r.Value }).ToList());
            w("  all    {0,5}   held out: exact {1:0}%, within 1 {2:0}%, off {3:0.00}", new object[] { rows.Count, s[0], s[1], s[2] });
            return report.ToString();
        }

        private static FreeAgentValueModel FitGroup(List<FreeAgentRow> rows, string group, string[] ratings)
        {
            double[][] x = rows.Select(r => ratings.Select(k => (double)r.Level[k]).ToArray()).ToArray();
            double[] y = rows.Select(r => (double)r.Value).ToArray();
            int n = ratings.Length;
            double[] coef = new double[n + 1];
            coef[n] = y.Average();
            // a rating every player of the group has the same (e.g. Tecmo's kickers' RS) can't be fitted
            List<int> active = Enumerable.Range(0, n).Where(i => x.Select(f => f[i]).Distinct().Count() > 1).ToList();
            while (active.Count > 0)
            {
                double[][] a = x.Select(f => active.Select(i => f[i]).Concat(new[] { 1.0 }).ToArray()).ToArray();
                double[] b = LeastSquares.Solve(a, y);
                int worst = -1;
                for (int k = 0; k < active.Count; k++)
                    if (b[k] < 0 && (worst < 0 || b[k] < b[worst]))
                        worst = k;
                if (worst < 0)
                {
                    for (int k = 0; k < active.Count; k++)
                        coef[active[k]] = b[k];
                    coef[n] = b[active.Count];
                    break;
                }
                active.RemoveAt(worst);
            }
            return new FreeAgentValueModel { Group = group, Ratings = ratings, Coef = coef };
        }

        /// <summary>[% exact, % within 1, average distance] of (model, Tecmo) pairs.</summary>
        private static double[] Score(List<int[]> pairs)
        {
            return new[]
            {
                100.0 * pairs.Count(p => p[0] == p[1]) / pairs.Count,
                100.0 * pairs.Count(p => Math.Abs(p[0] - p[1]) <= 1) / pairs.Count,
                pairs.Average(p => Math.Abs(p[0] - p[1])),
            };
        }
    }
}
