using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace TsbSimModel
{
    /// <summary>
    /// The model file (JSON): what 'fit' writes and 'constants' / 'expected' read. Numbers are rounded to 4
    /// decimals -- TSBTool's constants are these rounded values, so the file is the model as shipped.
    /// Layout:
    ///   { "digits": { "QB pass": { "positions": [..], "ratings": [..], "groups": [..], "group_key": "slot"|"group",
    ///                              "coef": [..], "stretch": x, "center": y }, ... },
    ///     "shares": { "carry": { "positions": [..], "ratings": [..], "coef": [..], "spread": x,
    ///                            "group_factors": { "QB": f, .. }, "groups": { "QB": [..], .. } }, ... } }
    /// ("groups" empty = one constant for everyone.) TSB2's also has the team SimData values:
    ///     "team_defense": { "rush D": { "groups": { "DL": [..], .. }, "mix": { "DL": { "HP": w, .. }, .. },
    ///                                   "coef": [DL, LB, DB, constant], "stretch": x, "center": y }, "pass D": { .. } }
    /// TSB3's has the free agent value (Face's last digit) per position group:
    ///     "free_agent_value": { "QB": { "positions": [..], "ratings": [..], "coef": [.., constant] }, .. }
    /// </summary>
    public static class ModelFile
    {
        public static double Round4(double v)
        {
            return Math.Round(v, 4, MidpointRounding.ToEven);
        }

        public static void Write(FittedModel model, string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n \"digits\": {\n");
            for (int i = 0; i < model.Digits.Count; i++)
            {
                DigitModel d = model.Digits[i];
                sb.AppendFormat("  {0}: {{\n", Str(d.Name));
                sb.AppendFormat("   \"positions\": {0},\n", Strs(d.Positions));
                sb.AppendFormat("   \"ratings\": {0},\n", Strs(d.Ratings));
                sb.AppendFormat("   \"groups\": {0},\n", Strs(d.Groups));
                sb.AppendFormat("   \"group_key\": {0},\n", Str(d.Const == ConstKind.Slot ? "slot" : "group"));
                sb.AppendFormat("   \"coef\": {0},\n", Nums(d.Coef));
                sb.AppendFormat("   \"stretch\": {0},\n", Num(d.Stretch));
                sb.AppendFormat("   \"center\": {0}\n", Num(d.Center));
                sb.Append(i < model.Digits.Count - 1 ? "  },\n" : "  }\n");
            }
            sb.Append(" },\n \"shares\": {\n");
            for (int i = 0; i < model.Shares.Count; i++)
            {
                ShareModel s = model.Shares[i];
                sb.AppendFormat("  {0}: {{\n", Str(s.Kind.Name));
                sb.AppendFormat("   \"positions\": {0},\n", Strs(s.Positions));
                sb.AppendFormat("   \"ratings\": {0},\n", Strs(s.Ratings));
                sb.AppendFormat("   \"coef\": {0},\n", Nums(s.Coef));
                sb.AppendFormat("   \"spread\": {0},\n", Num(s.Spread));
                List<string> f = new List<string>();
                foreach (KeyValuePair<string, double> kv in s.GroupFactors)
                    f.Add(Str(kv.Key) + ": " + Num(kv.Value));
                sb.AppendFormat("   \"group_factors\": {{{0}}},\n", string.Join(", ", f.ToArray()));
                List<string> g = new List<string>();
                foreach (KeyValuePair<string, string[]> kv in ShareSpec.Groups(s.Kind))
                    g.Add(Str(kv.Key) + ": " + Strs(kv.Value));
                sb.AppendFormat("   \"groups\": {{{0}}}\n", string.Join(", ", g.ToArray()));
                sb.Append(i < model.Shares.Count - 1 ? "  },\n" : "  }\n");
            }
            if (model.TeamDefense.Count > 0)
                sb.Append(" },\n \"team_defense\": {\n");
            for (int i = 0; i < model.TeamDefense.Count; i++)
            {
                TeamDefenseModel t = model.TeamDefense[i];
                List<string> g = new List<string>(), mix = new List<string>();
                for (int k = 0; k < TeamDefenseSpec.GroupNames.Length; k++)
                {
                    g.Add(Str(TeamDefenseSpec.GroupNames[k]) + ": " + Strs(TeamDefenseSpec.GroupSlots[k]));
                    List<string> rw = new List<string>();
                    for (int r = 0; r < t.MixRatings[k].Length; r++)
                        rw.Add(Str(t.MixRatings[k][r]) + ": " + Num(t.MixWeights[k][r]));
                    mix.Add(Str(TeamDefenseSpec.GroupNames[k]) + ": {" + string.Join(", ", rw.ToArray()) + "}");
                }
                sb.AppendFormat("  {0}: {{\n", Str(t.Name));
                sb.AppendFormat("   \"groups\": {{{0}}},\n", string.Join(", ", g.ToArray()));
                sb.AppendFormat("   \"mix\": {{{0}}},\n", string.Join(", ", mix.ToArray()));
                sb.AppendFormat("   \"coef\": {0},\n", Nums(t.Coef));
                sb.AppendFormat("   \"stretch\": {0},\n", Num(t.Stretch));
                sb.AppendFormat("   \"center\": {0}\n", Num(t.Center));
                sb.Append(i < model.TeamDefense.Count - 1 ? "  },\n" : "  }\n");
            }
            if (model.FreeAgentValue.Count > 0)
                sb.Append(" },\n \"free_agent_value\": {\n");
            for (int i = 0; i < model.FreeAgentValue.Count; i++)
            {
                FreeAgentValueModel f = model.FreeAgentValue[i];
                sb.AppendFormat("  {0}: {{\n", Str(f.Group));
                sb.AppendFormat("   \"positions\": {0},\n", Strs(FreeAgentValueSpec.Slots(f.Group)));
                sb.AppendFormat("   \"ratings\": {0},\n", Strs(f.Ratings));
                sb.AppendFormat("   \"coef\": {0}\n", Nums(f.Coef));
                sb.Append(i < model.FreeAgentValue.Count - 1 ? "  },\n" : "  }\n");
            }
            sb.Append(" }\n}\n");
            File.WriteAllText(path, sb.ToString());
        }

        public static FittedModel Read(string path)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            Dictionary<string, object> root = (Dictionary<string, object>)js.DeserializeObject(File.ReadAllText(path));
            FittedModel m = new FittedModel();
            foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)root["digits"])
            {
                Dictionary<string, object> d = (Dictionary<string, object>)kv.Value;
                DigitModel dm = new DigitModel
                {
                    Name = kv.Key,
                    Positions = StrArray(d["positions"]),
                    Ratings = StrArray(d["ratings"]),
                    Groups = StrArray(d["groups"]),
                    Coef = NumArray(d["coef"]),
                    Stretch = d.ContainsKey("stretch") ? Convert.ToDouble(d["stretch"], CultureInfo.InvariantCulture) : 1.0,
                };
                dm.Const = dm.Groups.Length == 0 ? ConstKind.Single : (string)d["group_key"] == "slot" ? ConstKind.Slot : ConstKind.Group;
                dm.Center = d.ContainsKey("center") ? Convert.ToDouble(d["center"], CultureInfo.InvariantCulture) : 0.0;
                m.Digits.Add(dm);
            }
            foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)root["shares"])
            {
                Dictionary<string, object> s = (Dictionary<string, object>)kv.Value;
                ShareKind kind = Array.Find(ShareKind.All, k => k.Name == kv.Key);
                ShareModel sm = new ShareModel
                {
                    Kind = kind,
                    Positions = StrArray(s["positions"]),
                    Ratings = StrArray(s["ratings"]),
                    Coef = NumArray(s["coef"]),
                    Spread = s.ContainsKey("spread") ? Convert.ToDouble(s["spread"], CultureInfo.InvariantCulture) : 1.0,
                };
                if (s.ContainsKey("group_factors"))
                    foreach (KeyValuePair<string, object> g in (Dictionary<string, object>)s["group_factors"])
                        sm.GroupFactors[g.Key] = Convert.ToDouble(g.Value, CultureInfo.InvariantCulture);
                m.Shares.Add(sm);
            }
            if (root.ContainsKey("team_defense"))
                foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)root["team_defense"])
                {
                    Dictionary<string, object> t = (Dictionary<string, object>)kv.Value;
                    Dictionary<string, object> mix = (Dictionary<string, object>)t["mix"];
                    int groups = TeamDefenseSpec.GroupNames.Length;
                    string[][] mixR = new string[groups][];
                    double[][] mixW = new double[groups][];
                    for (int k = 0; k < groups; k++)
                    {
                        Dictionary<string, object> rw = (Dictionary<string, object>)mix[TeamDefenseSpec.GroupNames[k]];
                        mixR[k] = rw.Keys.ToArray();
                        mixW[k] = rw.Values.Select(v => Convert.ToDouble(v, CultureInfo.InvariantCulture)).ToArray();
                    }
                    m.TeamDefense.Add(new TeamDefenseModel
                    {
                        Name = kv.Key,
                        MixRatings = mixR,
                        MixWeights = mixW,
                        Coef = NumArray(t["coef"]),
                        Stretch = Convert.ToDouble(t["stretch"], CultureInfo.InvariantCulture),
                        Center = Convert.ToDouble(t["center"], CultureInfo.InvariantCulture),
                    });
                }
            if (root.ContainsKey("free_agent_value"))
                foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)root["free_agent_value"])
                {
                    Dictionary<string, object> f = (Dictionary<string, object>)kv.Value;
                    m.FreeAgentValue.Add(new FreeAgentValueModel { Group = kv.Key, Ratings = StrArray(f["ratings"]), Coef = NumArray(f["coef"]) });
                }
            return m;
        }

        /// <summary>A number as Python's repr writes it (shortest round trip, "1.0" for whole numbers).</summary>
        public static string Num(double v)
        {
            if (v == 0 && 1 / v < 0) return "-0.0";
            string s = v.ToString("R", CultureInfo.InvariantCulture);
            if (s.IndexOf('.') < 0 && s.IndexOf('E') < 0)
                s += ".0";
            return s.Replace("E-0", "e-0").Replace("E-", "e-").Replace("E+", "e+");
        }

        private static string Str(string s) { return "\"" + s + "\""; }

        private static string Strs(string[] a)
        {
            string[] q = Array.ConvertAll(a, Str);
            return "[" + string.Join(", ", q) + "]";
        }

        private static string Nums(double[] a)
        {
            return "[" + string.Join(", ", Array.ConvertAll(a, Num)) + "]";
        }

        private static string[] StrArray(object o)
        {
            List<string> r = new List<string>();
            foreach (object x in (IEnumerable)o) r.Add((string)x);
            return r.ToArray();
        }

        private static double[] NumArray(object o)
        {
            List<double> r = new List<double>();
            foreach (object x in (IEnumerable)o) r.Add(Convert.ToDouble(x, CultureInfo.InvariantCulture));
            return r.ToArray();
        }
    }
}
