using System;
using System.Collections.Generic;

namespace TsbSimModel
{
    /// <summary>How a value's constant term is chosen: one for everyone, one per position group, or one per roster slot.</summary>
    public enum ConstKind { Single, Group, Slot }

    /// <summary>
    /// One 0-15 sim value the model works out (QB pass, rush, ...): which players have it, the ratings it's
    /// fitted on, how to read it from the sim bytes, and the default constant kind.
    /// </summary>
    public class DigitSpec
    {
        public string Name;
        public string[] Positions;
        public string[] Ratings;
        public Func<int[], int> Actual;
        public ConstKind DefaultConst;

        public static readonly string[] QBs = { "QB1", "QB2" };

        /// <summary>The values, in the order the model file and TSBTool's constants list them.</summary>
        public static readonly DigitSpec[] All =
        {
            new DigitSpec { Name = "QB pass",     Positions = QBs, Ratings = new[] { "PS", "PC", "PA", "AR", "CO" }, Actual = s => s[2] >> 4, DefaultConst = ConstKind.Single },
            new DigitSpec { Name = "QB rush",     Positions = QBs, Ratings = new[] { "MS", "HP", "BB" },             Actual = s => s[1],      DefaultConst = ConstKind.Single },
            new DigitSpec { Name = "QB scramble", Positions = QBs, Ratings = new[] { "MS", "BB", "HP" },             Actual = s => s[2] & 15, DefaultConst = ConstKind.Single },
            new DigitSpec { Name = "rush",   Positions = TsbSimModel.Positions.Skill, Ratings = new[] { "MS", "HP", "BB", "BC", "RP" }, Actual = s => s[1],      DefaultConst = ConstKind.Group },
            new DigitSpec { Name = "YAC",    Positions = TsbSimModel.Positions.Skill, Ratings = new[] { "MS", "HP", "BB", "RC", "RP" }, Actual = s => s[3] >> 4, DefaultConst = ConstKind.Group },
            new DigitSpec { Name = "catch",  Positions = TsbSimModel.Positions.Skill, Ratings = new[] { "RC", "BB", "MS", "RP" },       Actual = s => s[3] & 15, DefaultConst = ConstKind.Group },
            new DigitSpec { Name = "tackle", Positions = TsbSimModel.Positions.Defense, Ratings = new[] { "RS", "RP", "MS", "HP", "QU", "PI" }, Actual = s => s[2], DefaultConst = ConstKind.Group },
            // TSBTool text shows the kicker's digit alone ("[7]") and the punter's alone
            new DigitSpec { Name = "K", Positions = new[] { "K" }, Ratings = new[] { "KP", "KA", "AB" }, Actual = s => s[0], DefaultConst = ConstKind.Single },
            new DigitSpec { Name = "P", Positions = new[] { "P" }, Ratings = new[] { "KP", "AB" },       Actual = s => s[0], DefaultConst = ConstKind.Single },
        };

        public static DigitSpec Find(string name)
        {
            foreach (DigitSpec d in All)
                if (d.Name == name)
                    return d;
            throw new ArgumentException(name);
        }

        /// <summary>The key of 'pos's constant.</summary>
        public static string ConstKey(string pos, ConstKind kind)
        {
            switch (kind)
            {
                case ConstKind.Slot: return pos;
                case ConstKind.Group: return TsbSimModel.Positions.GroupOf(pos);
                default: return "";
            }
        }
    }

    /// <summary>The share kinds' fitted ratings and the position groups the calibration balances.</summary>
    public static class ShareSpec
    {
        public static string[] Ratings(ShareKind kind)
        {
            switch (kind.Name)
            {
                case "carry": return new[] { "MS", "HP", "BB" };
                case "target": return new[] { "MS", "BB", "RC", "HP" };
                case "sack": return new[] { "HP", "QU", "MS", "PI" };
                default: return new[] { "PI", "BB", "MS", "HP" };
            }
        }

        /// <summary>Position groups of the pool, in order. The first listed in FixedGroup has factor 1.</summary>
        public static List<KeyValuePair<string, string[]>> Groups(ShareKind kind)
        {
            List<KeyValuePair<string, string[]>> g = new List<KeyValuePair<string, string[]>>();
            string[] d = Positions.Defense;
            if (kind.Name == "carry")
                g.Add(new KeyValuePair<string, string[]>("QB", new[] { "QB1" }));
            if (kind.Name == "carry" || kind.Name == "target")
            {
                g.Add(new KeyValuePair<string, string[]>("RB", new[] { "RB1", "RB2", "RB3", "RB4" }));
                g.Add(new KeyValuePair<string, string[]>("WR", new[] { "WR1", "WR2", "WR3", "WR4" }));
                g.Add(new KeyValuePair<string, string[]>("TE", new[] { "TE1", "TE2" }));
            }
            else
            {
                g.Add(new KeyValuePair<string, string[]>("DL", Slice(d, 0, 6)));
                g.Add(new KeyValuePair<string, string[]>("LB", Slice(d, 6, 11)));
                g.Add(new KeyValuePair<string, string[]>("DB", Slice(d, 11, 18)));
            }
            return g;
        }

        /// <summary>The group whose factor is fixed at 1 (the others are relative to it).</summary>
        public static string FixedGroup(ShareKind kind)
        {
            switch (kind.Name)
            {
                case "carry": return "RB";
                case "target": return "WR";
                case "sack": return "DL";
                default: return "DB";
            }
        }

        public static string GroupIn(ShareKind kind, string pos)
        {
            foreach (KeyValuePair<string, string[]> g in Groups(kind))
                if (Array.IndexOf(g.Value, pos) > -1)
                    return g.Key;
            throw new ArgumentException(pos);
        }

        private static string[] Slice(string[] a, int from, int to)
        {
            string[] r = new string[to - from];
            Array.Copy(a, from, r, 0, r.Length);
            return r;
        }
    }

    /// <summary>
    /// A fitted 0-15 value: value = round(center + stretch * (sum of coef * rating step + constant - center)),
    /// clamped to 0..15. Coef = one per rating, then the constants (one per key in Groups, or one when Single).
    /// </summary>
    public class DigitModel
    {
        public string Name;
        public string[] Positions;
        public string[] Ratings;
        public ConstKind Const;
        public string[] Groups = new string[0];   // constant keys (sorted), empty for Single
        public double[] Coef;
        public double Stretch = 1.0, Center;

        /// <summary>The unrounded fitted value: ratings left to right, then the constant (as TsbSimAutoUpdater).</summary>
        public double Raw(Player p)
        {
            double s = 0;
            for (int i = 0; i < Ratings.Length; i++)
                s += Coef[i] * p.Level[Ratings[i]];
            if (Const == ConstKind.Single)
                return s + Coef[Ratings.Length];
            int g = Array.IndexOf(Groups, DigitSpec.ConstKey(p.Position, Const));
            return s + Coef[Ratings.Length + g];
        }

        public int Value(Player p)
        {
            return Clamp15((int)Math.Round(Center + Stretch * (Raw(p) - Center)));
        }

        public static int Clamp15(int v)
        {
            return Math.Max(0, Math.Min(15, v));
        }
    }

    /// <summary>
    /// A fitted share kind: weight = (e ^ (sum of coef * rating step + slot constant) - 1) ^ spread * group
    /// factor; a team's weights are then scaled to 255 (none over 254). Coef = one per rating, then one
    /// constant per pool slot (Positions order).
    /// </summary>
    public class ShareModel
    {
        public ShareKind Kind;
        public string[] Positions;
        public string[] Ratings;
        public double[] Coef;
        public double Spread = 1.0;
        public Dictionary<string, double> GroupFactors = new Dictionary<string, double>();

        /// <summary>exp(fit) - 1 for the player in 'slot' (QB2 is given QB1's slot), at least 0.</summary>
        public double BaseWeight(Player p, string slot)
        {
            double s = 0;
            for (int i = 0; i < Ratings.Length; i++)
                s += Coef[i] * p.Level[Ratings[i]];
            s += Coef[Ratings.Length + Array.IndexOf(Positions, slot)];
            return Math.Max(0.0, Math.Exp(s) - 1.0);
        }

        public double Weight(Player p, string slot)
        {
            double w = Math.Pow(BaseWeight(p, slot), Spread);
            double f;
            if (GroupFactors.TryGetValue(ShareSpec.GroupIn(Kind, slot), out f))
                w *= f;
            return w;
        }

        /// <summary>The team's whole shares for the pool (adding up to 255), by position.</summary>
        public Dictionary<string, int> Shares(Team t)
        {
            double[] w = new double[Positions.Length];
            for (int i = 0; i < w.Length; i++)
                w[i] = Weight(t.Players[Positions[i]], Positions[i]);
            int[] s = ShareKind.ScaleTo(w, 255);
            Dictionary<string, int> ret = new Dictionary<string, int>();
            for (int i = 0; i < s.Length; i++)
                ret[Positions[i]] = s[i];
            return ret;
        }
    }

    /// <summary>
    /// The team SimData the model works out (TSB2: rush defense, pass defense) and what it reads: the starters,
    /// in position groups -- TSB2's starting defense is RE NT LE, the 4 LBs and RCB LCB FS SS (the 2nd-string
    /// slots RE2, NT2, LE2, LB5, DB1, DB2, DB3 are backups) -- each player scored with a fixed mix of his rating
    /// steps. The mixes are Tecmonster's team formula's (TecmonsterTeamSim): run defense mostly HP for the DL,
    /// RS RP MS HP for LBs and DBs; pass defense HP for the DL, PI and QU for LBs and DBs. With 84 stock
    /// team-seasons and a byte Tecmo set mostly from each team's real season, fitting a weight per rating
    /// overfits (a DL's PI rating came out as rush defense's biggest weight); only the group weights are fitted.
    /// </summary>
    public static class TeamDefenseSpec
    {
        public static readonly string[] Names = { "rush D", "pass D" };
        public static readonly string[] GroupNames = { "DL", "LB", "DB" };
        public static readonly string[][] GroupSlots =
        {
            new[] { "RE", "NT", "LE" },
            new[] { "ROLB", "RILB", "LILB", "LOLB" },
            new[] { "RCB", "LCB", "FS", "SS" },
        };

        /// <summary>Each group's rating mix for the value 'name': the ratings (MixWeights has their weights).</summary>
        public static string[][] MixRatings(string name)
        {
            return name == "rush D"
                ? new[] { new[] { "RS", "RP", "MS", "HP" }, new[] { "RS", "RP", "MS", "HP" }, new[] { "RS", "RP", "MS", "HP" } }
                : new[] { new[] { "RS", "RP", "MS", "HP", "PI", "QU" }, new[] { "RS", "RP", "MS", "PI", "QU" }, new[] { "RS", "RP", "MS", "PI", "QU" } };
        }

        public static double[][] MixWeights(string name)
        {
            return name == "rush D"
                ? new[] { new[] { 0.05, 0.05, 0.05, 0.85 }, new[] { 0.25, 0.25, 0.1, 0.4 }, new[] { 0.25, 0.25, 0.1, 0.4 } }
                : new[] { new[] { 0.05, 0.05, 0.05, 0.75, 0.05, 0.05 }, new[] { 0.15, 0.15, 0.1, 0.3, 0.3 }, new[] { 0.15, 0.15, 0.1, 0.3, 0.3 } };
        }

        /// <summary>Tecmo's digit of the value 'name' for the team (TSB2 team byte: rush D high, pass D low).</summary>
        public static int Actual(Team t, string name)
        {
            return name == "rush D" ? t.RushD : t.PassD;
        }

        /// <summary>True when every starter the model reads is in the team.</summary>
        public static bool HasStarters(Team t)
        {
            foreach (string[] g in GroupSlots)
                foreach (string pos in g)
                    if (!t.Players.ContainsKey(pos))
                        return false;
            return true;
        }
    }

    /// <summary>
    /// A fitted team value (TSB2's rush D / pass D digit): each position group's score is the average over its
    /// starters of their mix (sum of weight * rating step); raw = sum of coef * group score + constant; value =
    /// round(center + stretch * (raw - center)), clamped to 0..15. Coef = DL, LB, DB, then the constant; none
    /// of the group weights is negative (better defenders never lower the value).
    /// </summary>
    public class TeamDefenseModel
    {
        public string Name;
        public string[][] MixRatings;
        public double[][] MixWeights;
        public double[] Coef;
        public double Stretch = 1.0, Center;

        /// <summary>The group scores (DL, LB, DB) of 't' with the mix.</summary>
        public static double[] GroupScores(Team t, string[][] mixRatings, double[][] mixWeights)
        {
            double[] f = new double[TeamDefenseSpec.GroupSlots.Length];
            for (int g = 0; g < f.Length; g++)
            {
                double sum = 0;
                foreach (string pos in TeamDefenseSpec.GroupSlots[g])
                    for (int r = 0; r < mixRatings[g].Length; r++)
                        sum += mixWeights[g][r] * t.Players[pos].Level[mixRatings[g][r]];
                f[g] = sum / TeamDefenseSpec.GroupSlots[g].Length;
            }
            return f;
        }

        public double Raw(Team t)
        {
            double[] f = GroupScores(t, MixRatings, MixWeights);
            double s = Coef[f.Length];
            for (int i = 0; i < f.Length; i++)
                s += Coef[i] * f[i];
            return s;
        }

        public int Value(Team t)
        {
            return DigitModel.Clamp15((int)Math.Round(Center + Stretch * (Raw(t) - Center)));
        }
    }


    /// <summary>A game's whole fitted model: what the model file holds and TSBTool's constants come from.</summary>
    public class FittedModel
    {
        public List<DigitModel> Digits = new List<DigitModel>();
        public List<ShareModel> Shares = new List<ShareModel>();
        public List<TeamDefenseModel> TeamDefense = new List<TeamDefenseModel>();
        public List<FreeAgentValueModel> FreeAgentValue = new List<FreeAgentValueModel>();

        /// <summary>
        /// The team SimData digits (rush D, pass D) the model works out for 't'; null when the model has no team
        /// defense (TSB3) or a starter isn't in the team.
        /// </summary>
        public int[] TeamSimData(Team t)
        {
            if (TeamDefense.Count == 0 || !TeamDefenseSpec.HasStarters(t))
                return null;
            int[] ret = new int[TeamDefenseSpec.Names.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = TeamDefense.Find(m => m.Name == TeamDefenseSpec.Names[i]).Value(t);
            return ret;
        }

        public DigitModel Digit(string name)
        {
            foreach (DigitModel d in Digits)
                if (d.Name == name)
                    return d;
            throw new ArgumentException(name);
        }

        public ShareModel Share(ShareKind kind)
        {
            foreach (ShareModel s in Shares)
                if (s.Kind == kind)
                    return s;
            throw new ArgumentException(kind.Name);
        }

        /// <summary>
        /// What Auto Update Sim makes of a team: sim bytes by position (QB [carry, rush, pass|scramble],
        /// RB/WR/TE [carry, rush, target, YAC|catch], defenders [sack, INT, tackle], K [kick], P [punt]).
        /// QB2 gets no carries (bug G3). 'keepShares' (TSB3's free agents / all-time team) keeps the players'
        /// own share bytes. Pools whose players aren't all there keep theirs too.
        /// </summary>
        public Dictionary<string, int[]> TeamBytes(Team t, bool keepShares)
        {
            Dictionary<string, int>[] pools = new Dictionary<string, int>[4];
            for (int k = 0; k < 4; k++)
            {
                ShareKind kind = ShareKind.All[k];
                bool complete = true;
                foreach (string pos in kind.Pool)
                    complete &= t.Players.ContainsKey(pos);
                pools[k] = keepShares || !complete ? new Dictionary<string, int>() : Share(kind).Shares(t);
            }
            if (!keepShares)
                pools[0]["QB2"] = 0;
            Dictionary<string, int[]> ret = new Dictionary<string, int[]>();
            foreach (Player p in t.Players.Values)
            {
                string pos = p.Position;
                Func<int, int, int> share = (k, b) => pools[k].ContainsKey(pos) ? pools[k][pos] : (p.Sim.Length > b ? p.Sim[b] : 0);
                if (pos == "QB1" || pos == "QB2")
                    ret[pos] = new[] { share(0, 0), Digit("QB rush").Value(p), (Digit("QB pass").Value(p) << 4) | Digit("QB scramble").Value(p) };
                else if (Array.IndexOf(Positions.Skill, pos) > -1)
                    ret[pos] = new[] { share(0, 0), Digit("rush").Value(p), share(1, 2), (Digit("YAC").Value(p) << 4) | Digit("catch").Value(p) };
                else if (Array.IndexOf(Positions.Defense, pos) > -1)
                    ret[pos] = new[] { share(2, 0), share(3, 1), Digit("tackle").Value(p) };
                else if (pos == "K")
                    ret[pos] = new[] { Digit("K").Value(p) };
                else if (pos == "P")
                    ret[pos] = new[] { Digit("P").Value(p) };
            }
            return ret;
        }
    }
}
