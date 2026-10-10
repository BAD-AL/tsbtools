using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool2
{
    /// <summary>
    /// TSB2's and TSB3's Auto Update Sim: works out every player's sim bytes from his ratings, a team at a time.
    ///
    /// What the bytes do is from tracing the games' sim engine (TSBTool_Test\SimTests\TSB2_study\
    /// TSB2_Sim_Engine.txt; TSB3 runs the same engine with its own lookup tables, TSB3_study\
    /// TSB3_Checks_results.txt). The numbers below are models fitted to Tecmo's own sim bytes -- TSB2's three
    /// stock seasons, TSB3's one -- checked on data they weren't fitted on and with simmed seasons.
    /// The fitting, the constants below and the tests' expected output come from the TsbSimModel tool
    /// (TSBTool\TsbSimModel: README.md; the fitted models and their reports in TsbSimModel\Models).
    ///  - Values 0-15 (QB pass / rush / scramble, rush, yards after catch, catch, tackles, K, P):
    ///      value = round(center + stretch * (sum of coef * rating step + constant - center)), 0..15,
    ///    a rating step being rating / 6.25 rounded (6 -> 1 .. 100 -> 16). The constant is per roster slot
    ///    (RB1, RB2 .. / RE, RE2 ..) for the RB/WR/TE and defenders' values.
    ///  - Shares out of 255 (carries, targets, sacks, INTs: who gets the ball / the credit):
    ///      weight = (e ^ (sum of coef * rating step + slot constant) - 1) ^ spread * position group factor,
    ///    then each team's weights are scaled to add up to 255, none over 254 (255 means 'absent player').
    ///    QB2 gets no carries: the game's carrier pick walks both QBs, and a carry landing on the QB who isn't
    ///    playing loses its credit (bug G3). A share pool whose players aren't all in the text keeps the
    ///    shares it has; so do TSB3's free agents and all-time team, which aren't teams.
    ///  - TSB3: the free agent value (Face's last digit) is worked out too, for every player (UpdateFreeAgentValue):
    ///    TSB3's team defense adds it up (TSBTool_Test\SimTests\TSB3_study\TSB3_HiddenRating_results.txt). Tecmo set
    ///    it by hand; the model fits Tecmo's values per position group (it replaced FAP_Formulas.txt, which ran high).
    /// TsbSimModel's 'expected' command does the same sums in the same order; the tests hold this class to its
    /// output for the stock rosters (TsbSimAutoUpdaterTests).
    /// </summary>
    public static class TsbSimAutoUpdater
    {
        private static readonly string[] Skill = { "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1", "TE2" };
        private static readonly string[] Defense = { "RE", "NT", "LE", "RE2", "NT2", "LE2", "ROLB", "RILB", "LILB", "LOLB", "LB5",
                                                     "RCB", "LCB", "DB1", "DB2", "FS", "SS", "DB3" };
        private static readonly string[] Linemen = { "C", "LG", "RG", "LT", "RT" };

        // TSB2's starting defense, in the team SimData model's position groups (DL, LB, DB); the 2nd-string
        // slots (RE2, NT2, LE2, LB5, DB1, DB2, DB3) are backups
        private static readonly string[][] TeamDefenseGroups =
        {
            new string[] { "RE", "NT", "LE" },
            new string[] { "ROLB", "RILB", "LILB", "LOLB" },
            new string[] { "RCB", "LCB", "FS", "SS" },
        };

        // a team line's SimData digits (TSB2: rush D, pass D; a TSB1 text's 3rd digit is left as it is)
        private static readonly Regex TeamSimDataRegex = new Regex("SimData\\s*=\\s*0[xX]([0-9a-fA-F]{2})");

        // "TEAM = bills" (not "TEAM_ABB=..."): a new team
        private static readonly Regex TeamNameRegex = new Regex("^TEAM\\s*=\\s*([0-9a-zA-Z]+)");
        private static readonly Regex SeasonLineRegex = new Regex("^\\s*SEASON\\s+([1-3])", RegexOptions.Multiline);

        // "QB1,jim KELLY,Face=0x00," (as TSB2's InputParser.posNameFaceRegex, from the start of the line)
        private static readonly Regex PlayerRegex = new Regex("^([A-Z]+[1-5]?)\\s*,\\s*([a-zA-Z \\.\\-]+),\\s*(Face=0[xX][0-9a-fA-F]+\\s*,\\s*)");

        // A share of 255 (FF) means 'absent player' to the sim engine (TSB2_Sim_Engine.txt).
        private const int MaxShare = 254;

        /// <summary>TSB2's Auto Update Sim (see AutoUpdatePlayerSimData(string, TSBContentType)).</summary>
        public static string AutoUpdatePlayerSimData(string text)
        {
            return AutoUpdatePlayerSimData(text, TSBTool.TSBContentType.TSB2);
        }

        /// <summary>
        /// Returns 'text' (TSBTool's TSB2 or TSB3 text: SEASON / TEAM lines and player lines) with every
        /// player's sim bytes worked out from his ratings. Only the [..] part of the player lines changes
        /// (and, for TSB3, the free agent value in Face); team lines stay as they are. For the converter and
        /// UpdateRomSimData: a roster's own team values (a TSB1 roster's mean the same in TSB2) are kept.
        /// </summary>
        public static string AutoUpdatePlayerSimData(string text, TSBTool.TSBContentType game)
        {
            return Update(text, game, false);
        }

        /// <summary>
        /// The player forms' Auto Update Sim: AutoUpdatePlayerSimData, and TSB2's team SimData (rush D, pass D)
        /// worked out from each team's 11 starting defenders (a team missing one keeps its value; a team line
        /// without SimData gets it). TSB3's team SimData -- a defense bonus, not a team defense -- isn't changed.
        /// </summary>
        public static string AutoUpdateSimData(string text, TSBTool.TSBContentType game)
        {
            return Update(text, game, true);
        }

        private static string Update(string text, TSBTool.TSBContentType game, bool teamSimData)
        {
            GameModel model = ModelFor(game);
            string[] lines = text.Split('\n');
            Dictionary<string, Player> team = new Dictionary<string, Player>();
            string teamName = null;
            int teamLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');
                Match t = TeamNameRegex.Match(line);
                if (t.Success || line.StartsWith("SEASON"))
                {
                    UpdateTeam(model, lines, team, teamName, teamSimData ? teamLine : -1);
                    team.Clear();
                    teamName = t.Success ? t.Groups[1].Value : null;
                    teamLine = t.Success ? i : -1;
                    continue;
                }
                Player p = Player.Parse(model, line, i);
                if (p != null)
                    team[p.Position] = p;
                else if (model.FreeAgentValue && Array.IndexOf(Linemen, PlayerPosition(line)) > -1)
                    lines[i] = UpdateFreeAgentValue(model, line) + (lines[i].EndsWith("\r") ? "\r" : "");
            }
            UpdateTeam(model, lines, team, teamName, teamSimData ? teamLine : -1);
            return string.Join("\n", lines);
        }

        /// <summary>
        /// The seasons (1-3) a TSB2 text writes to, in the order its "SEASON n" lines name them; a text
        /// without one writes to season 1 (InputParser's default).
        /// </summary>
        public static int[] SeasonsIn(string text)
        {
            List<int> ret = new List<int>();
            foreach (Match m in SeasonLineRegex.Matches(text))
            {
                int season = int.Parse(m.Groups[1].Value);
                if (!ret.Contains(season))
                    ret.Add(season);
            }
            if (ret.Count == 0)
                ret.Add(1);
            return ret.ToArray();
        }

        /// <summary>
        /// Works out the sim bytes of every player of 'season' from the ROM's own rosters and writes them
        /// (only the sim bytes -- and TSB3's free agent values -- names, ratings and the rest stay as they are). For after a text whose teams
        /// aren't complete was applied -- a TSB1 roster converted to TSB2 has no RE2/NT2/LE2/LB5/DB3, so
        /// its sack and INT shares can only be worked out with the ROM's players in those slots.
        /// </summary>
        public static void UpdateRomSimData(TSB2Tool tool, int season)
        {
            UpdateRomSimData(tool, season, false);
        }

        /// <summary>
        /// UpdateRomSimData; with 'teamSimData', TSB2's team SimData too (as AutoUpdateSimData). TsbSimModel's
        /// 'apply' (test ROMs) uses it.
        /// </summary>
        public static void UpdateRomSimData(TSB2Tool tool, int season, bool teamSimData)
        {
            TSBTool.TSBContentType game = tool is TSB3Tool ? TSBTool.TSBContentType.TSB3 : TSBTool.TSBContentType.TSB2;
            GameModel model = ModelFor(game);
            string text = Update("SEASON " + season + "\n" + tool.GetTeams(season), game, teamSimData);
            string team = null;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                Match m = TeamNameRegex.Match(line);
                if (m.Success)
                {
                    team = m.Groups[1].Value;
                    Match sim = TeamSimDataRegex.Match(line);
                    if (teamSimData && model.TeamDefense.Length > 0 && sim.Success)
                        tool.SetTeamSimData(season, team, sim.Groups[1].Value);
                    continue;
                }
                // TSB3: the free agent value (Face's last digit; every player, linemen too)
                Match face = model.FreeAgentValue && team != null ? FreeAgentLineRegex.Match(line) : Match.Empty;
                if (face.Success)
                    tool.SetFace(season, team, face.Groups[1].Value, Convert.ToInt32(face.Groups[2].Value + face.Groups[3].Value, 16));
                Player p = team == null ? null : Player.Parse(model, line, 0);
                if (p == null || p.Sim.Length == 0)
                    continue;
                string pos = p.Position;
                if (pos == "QB1" || pos == "QB2")
                    tool.SetQBSimData(season, team, pos, p.Sim);
                else if (Array.IndexOf(Skill, pos) > -1)
                    tool.SetSkillSimData(season, team, pos, p.Sim);
                else if (Array.IndexOf(Defense, pos) > -1)
                    tool.SetDefensiveSimData(season, team, pos, p.Sim);
                else if (pos == "K")
                    tool.SetKickingSimData(season, team, (byte)p.Sim[0]);
                else if (pos == "P")
                    tool.SetPuntingSimData(season, team, (byte)p.Sim[0]);
            }
        }

        /// <summary>
        /// Writes the team's players' sim bytes into 'lines', and when 'teamLine' is a line (not -1) and the game
        /// has a team model, the team SimData from its starters (all of them there; not for KeepShareTeams).
        /// </summary>
        private static void UpdateTeam(GameModel model, string[] lines, Dictionary<string, Player> team, string teamName, int teamLine)
        {
            if (team.Count == 0)
                return;
            bool keepShares = teamName != null && Array.IndexOf(model.KeepShareTeams, teamName) > -1;
            if (teamLine > -1 && !keepShares && model.TeamDefense.Length > 0 && HasStarters(team))
                lines[teamLine] = TeamLineWith(lines[teamLine], model.FindTeamDefense("rush D").Value(team), model.FindTeamDefense("pass D").Value(team));
            List<string> carryPool = new List<string>(Skill);
            carryPool.Insert(0, "QB1");
            Dictionary<string, int> carry = TeamShares(model, "carry", team, carryPool.ToArray(), 0, keepShares);
            Dictionary<string, int> target = TeamShares(model, "target", team, Skill, 2, keepShares);
            Dictionary<string, int> sack = TeamShares(model, "sack", team, Defense, 0, keepShares);
            Dictionary<string, int> ints = TeamShares(model, "int", team, Defense, 1, keepShares);
            if (!keepShares)
                carry["QB2"] = 0; // see the class comment (G3)

            foreach (Player p in team.Values)
            {
                string pos = p.Position;
                string sim;
                if (pos == "QB1" || pos == "QB2")
                    sim = Hex(Share(carry, p, 0), Digit(model, "QB rush", p), (Digit(model, "QB pass", p) << 4) | Digit(model, "QB scramble", p));
                else if (Array.IndexOf(Skill, pos) > -1)
                    sim = Hex(Share(carry, p, 0), Digit(model, "rush", p), Share(target, p, 2), (Digit(model, "YAC", p) << 4) | Digit(model, "catch", p));
                else if (Array.IndexOf(Defense, pos) > -1)
                    sim = Hex(Share(sack, p, 0), Share(ints, p, 1), Digit(model, "tackle", p));
                else if (pos == "K")
                    sim = string.Format("[{0:X}]", Digit(model, "K", p));
                else
                    sim = string.Format("[{0:X}]", Digit(model, "P", p));
                string line = p.LineWithoutSim + "," + sim;
                if (model.FreeAgentValue)
                    line = UpdateFreeAgentValue(model, line);
                lines[p.LineIndex] = line + (lines[p.LineIndex].EndsWith("\r") ? "\r" : "");
            }
        }

        private static bool HasStarters(Dictionary<string, Player> team)
        {
            foreach (string[] g in TeamDefenseGroups)
                foreach (string pos in g)
                    if (!team.ContainsKey(pos))
                        return false;
            return true;
        }

        /// <summary>
        /// 'line' (a team line, maybe ending in '\r') with its SimData digits set to rush D / pass D: only the two
        /// digits change; a line without SimData gets ",SimData=0x..".
        /// </summary>
        private static string TeamLineWith(string line, int rushD, int passD)
        {
            string cr = line.EndsWith("\r") ? "\r" : "";
            string body = line.TrimEnd('\r');
            string hex = string.Format("{0:X}{1:X}", rushD, passD);
            Match m = TeamSimDataRegex.Match(body);
            if (!m.Success)
                return body + ",SimData=0x" + hex + cr;
            return body.Substring(0, m.Groups[1].Index) + hex + body.Substring(m.Groups[1].Index + 2) + cr;
        }

        // the player's share from the pool, or what he has now
        private static int Share(Dictionary<string, int> pool, Player p, int simIndex)
        {
            int ret;
            if (pool.TryGetValue(p.Position, out ret))
                return ret;
            return p.Sim.Length > simIndex ? p.Sim[simIndex] : 0;
        }

        private static string Hex(params int[] values)
        {
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");
                sb.Append(values[i].ToString("X2"));
            }
            return sb.Append("]").ToString();
        }

        private static string PlayerPosition(string line)
        {
            Match m = PlayerRegex.Match(line);
            return m.Success ? m.Groups[1].Value : null;
        }

        // ---------------------------------------------------------------------------------------------

        private static int Digit(GameModel model, string name, Player p)
        {
            DigitModel m = model.FindDigit(name);
            double raw = Sum(m.Coef, m.Ratings, p);
            string key = m.BySlot ? p.Position : (m.Constant.ContainsKey("") ? "" : GroupOf(p.Position));
            raw += m.Constant[key];
            int value = (int)Math.Round(m.Center + m.Stretch * (raw - m.Center));
            return Math.Max(0, Math.Min(15, value));
        }

        private static double Sum(double[] coef, string[] ratings, Player p)
        {
            double s = 0.0;
            for (int i = 0; i < ratings.Length; i++)
                s += coef[i] * p.Steps[ratings[i]];
            return s;
        }

        /// <summary>
        /// The pool's shares (adding up to 255) by position; empty -- everyone keeps what he has -- when one
        /// of the pool's players isn't in the text or the 'team' keeps its shares.
        /// </summary>
        private static Dictionary<string, int> TeamShares(GameModel model, string name, Dictionary<string, Player> team,
            string[] pool, int simIndex, bool keepShares)
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();
            if (keepShares)
                return ret;
            foreach (string pos in pool)
                if (!team.ContainsKey(pos))
                    return ret;
            ShareModel m = model.FindShare(name);
            double[] weights = new double[pool.Length];
            for (int i = 0; i < pool.Length; i++)
            {
                Player p = team[pool[i]];
                double w = Math.Max(0.0, Math.Exp(Sum(m.Coef, m.Ratings, p) + m.SlotConstant[p.Position]) - 1.0);
                w = Math.Pow(w, m.Spread);
                double factor;
                if (m.GroupFactor.TryGetValue(m.GroupOf[p.Position], out factor))
                    w *= factor;
                weights[i] = w;
            }
            int[] shares = ScaleTo(weights, 255);
            for (int i = 0; i < pool.Length; i++)
                ret[pool[i]] = shares[i];
            return ret;
        }

        /// <summary>
        /// Whole numbers in proportion to 'weights' that add up to 'total' (largest remainder first), none
        /// over 254: a share of 255 (FF) is the sim engine's 'absent player' marker, so a player with it would
        /// be skipped. What's over 254 goes to the next players in remainder order.
        /// </summary>
        public static int[] ScaleTo(double[] weights, int total)
        {
            int[] ret = new int[weights.Length];
            double sum = 0.0;
            foreach (double w in weights)
                sum += w;
            if (sum <= 0)
                return ret;
            double[] exact = new double[weights.Length];
            int given = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                exact[i] = weights[i] * total / sum;
                ret[i] = (int)exact[i];
                given += ret[i];
            }
            // the rest, one each, to the biggest remainders (ties: roster order)
            List<int> order = new List<int>();
            for (int i = 0; i < weights.Length; i++)
            {
                int at = order.Count;
                while (at > 0 && exact[order[at - 1]] - ret[order[at - 1]] < exact[i] - ret[i])
                    at--;
                order.Insert(at, i);
            }
            for (int k = 0; k < total - given && k < order.Count; k++)
                ret[order[k]]++;
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i] <= MaxShare)
                    continue;
                int extra = ret[i] - MaxShare;
                ret[i] = MaxShare;
                foreach (int j in order)
                {
                    if (j != i && extra > 0)
                    {
                        ret[j]++;
                        extra--;
                    }
                }
            }
            return ret;
        }

        private static string GroupOf(string pos)
        {
            if (pos.StartsWith("QB")) return "QB";
            if (Array.IndexOf(Skill, pos) > -1) return pos.Substring(0, 2);
            int d = Array.IndexOf(Defense, pos);
            if (d > -1) return d < 6 ? "DL" : d < 11 ? "LB" : "DB";
            return pos;
        }

        // ---------------------------------------------------------------------------------------------

        /// <summary>One game's ratings on a player line, fitted models, and what's special about it.</summary>
        private class GameModel
        {
            public string[] QB, Skill, Defense, Kicker, Punter, Lineman;
            public DigitModel[] Digits;
            public ShareModel[] Shares;
            public TeamDefenseModel[] TeamDefense; // empty: the game's team SimData isn't a team defense (TSB3)
            public string[] KeepShareTeams;
            public bool FreeAgentValue;
            public FreeAgentValueModel[] FreeAgentValues;

            public DigitModel FindDigit(string name)
            {
                foreach (DigitModel m in Digits)
                    if (m.Name == name)
                        return m;
                throw new ArgumentException("No sim model for " + name);
            }

            public ShareModel FindShare(string name)
            {
                foreach (ShareModel m in Shares)
                    if (m.Name == name)
                        return m;
                throw new ArgumentException("No share model for " + name);
            }

            public TeamDefenseModel FindTeamDefense(string name)
            {
                foreach (TeamDefenseModel m in TeamDefense)
                    if (m.Name == name)
                        return m;
                throw new ArgumentException("No team model for " + name);
            }
        }

        private static GameModel sTsb2, sTsb3;

        private static GameModel ModelFor(TSBTool.TSBContentType game)
        {
            if (game == TSBTool.TSBContentType.TSB3)
            {
                if (sTsb3 == null)
                {
                    GameModel m = new GameModel();
                    m.QB = new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "PS", "PC", "PA", "AR", "CO" };
                    m.Skill = new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "BC", "RC" };
                    m.Defense = new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "PI", "QU" };
                    m.Kicker = new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "KP", "KA", "AB" };
                    m.Punter = new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "KP", "AB" };
                    m.Digits = Tsb3Digits;
                    m.Shares = Tsb3Shares;
                    m.TeamDefense = Tsb3TeamDefense;
                    // not teams: Tecmo gave them flat placeholder shares; their players' shares count once
                    // they're signed, alongside their new team's
                    m.KeepShareTeams = new string[] { "freeAgents", "allTime" };
                    m.FreeAgentValue = true;
                    m.FreeAgentValues = Tsb3FreeAgentValue;
                    m.Lineman = new string[] { "RS", "RP", "MS", "HP", "BB", "AG" };
                    sTsb3 = m;
                }
                return sTsb3;
            }
            if (game != TSBTool.TSBContentType.TSB2)
                throw new ArgumentException("TsbSimAutoUpdater: TSB2 or TSB3 only, not " + game);
            if (sTsb2 == null)
            {
                GameModel m = new GameModel();
                m.QB = new string[] { "RS", "RP", "MS", "HP", "BB", "PS", "PC", "PA", "AR", "CO" };
                m.Skill = new string[] { "RS", "RP", "MS", "HP", "BB", "BC", "RC" };
                m.Defense = new string[] { "RS", "RP", "MS", "HP", "BB", "PI", "QU" };
                m.Kicker = new string[] { "RS", "RP", "MS", "HP", "BB", "KP", "KA", "AB" };
                m.Punter = new string[] { "RS", "RP", "MS", "HP", "BB", "KP", "AB" };
                m.Digits = Tsb2Digits;
                m.Shares = Tsb2Shares;
                m.TeamDefense = Tsb2TeamDefense;
                m.KeepShareTeams = new string[0];
                m.FreeAgentValue = false;
                m.FreeAgentValues = Tsb2FreeAgentValue;
                m.Lineman = new string[] { "RS", "RP", "MS", "HP", "BB" };
                sTsb2 = m;
            }
            return sTsb2;
        }

        private class Player
        {
            public string Position;
            public int LineIndex;
            public string LineWithoutSim;
            public Dictionary<string, int> Steps = new Dictionary<string, int>();
            public int[] Sim = new int[0];

            /// <summary>The player on 'line', or null when it isn't a player with sim data (OL, KR, ...).</summary>
            public static Player Parse(GameModel model, string line, int lineIndex)
            {
                Match m = PlayerRegex.Match(line);
                if (!m.Success)
                    return null;
                string pos = m.Groups[1].Value;
                string[] names = RatingNamesFor(model, pos);
                if (names == null)
                    return null;
                Player p = new Player();
                p.Position = pos;
                p.LineIndex = lineIndex;
                int simStart = line.IndexOf('[');
                p.LineWithoutSim = (simStart > -1 ? line.Substring(0, simStart) : line).TrimEnd(',', ' ');
                string[] parts = p.LineWithoutSim.Substring(m.Length).Split(','); // "#12", ratings
                if (parts.Length - 1 != names.Length)
                    return null;
                for (int i = 0; i < names.Length; i++)
                {
                    int rating;
                    if (!int.TryParse(parts[i + 1].Trim(), out rating))
                        return null;
                    p.Steps[names[i]] = (int)Math.Round(rating / 6.25);
                }
                if (simStart > -1)
                {
                    int simEnd = line.IndexOf(']', simStart);
                    string[] sim = line.Substring(simStart + 1, (simEnd > simStart ? simEnd : line.Length) - simStart - 1).Split(',');
                    List<int> vals = new List<int>();
                    foreach (string s in sim)
                    {
                        try { vals.Add(Convert.ToInt32(s.Trim(), 16)); }
                        catch (FormatException) { }
                    }
                    p.Sim = vals.ToArray();
                }
                return p;
            }

            public static string[] RatingNamesFor(GameModel model, string pos)
            {
                if (pos == "QB1" || pos == "QB2") return model.QB;
                if (Array.IndexOf(TsbSimAutoUpdater.Skill, pos) > -1) return model.Skill;
                if (Array.IndexOf(TsbSimAutoUpdater.Defense, pos) > -1) return model.Defense;
                if (pos == "K") return model.Kicker;
                if (pos == "P") return model.Punter;
                return null; // OL: no sim data
            }
        }

        private class DigitModel
        {
            public string Name;
            public string[] Ratings;
            public double[] Coef;
            public Dictionary<string, double> Constant; // by roster slot, by group, or "" for everyone
            public bool BySlot;
            public double Stretch, Center;

            public DigitModel(string name, string[] ratings, double[] coef, Dictionary<string, double> constant,
                bool bySlot, double stretch, double center)
            {
                Name = name; Ratings = ratings; Coef = coef; Constant = constant;
                BySlot = bySlot; Stretch = stretch; Center = center;
            }
        }

        private class ShareModel
        {
            public string Name;
            public string[] Ratings;
            public double[] Coef;
            public Dictionary<string, double> SlotConstant;
            public double Spread;
            public Dictionary<string, double> GroupFactor; // groups not listed: 1
            public Dictionary<string, string> GroupOf;

            public ShareModel(string name, string[] ratings, double[] coef, Dictionary<string, double> slotConstant,
                double spread, Dictionary<string, double> groupFactor, Dictionary<string, string> groupOf)
            {
                Name = name; Ratings = ratings; Coef = coef; SlotConstant = slotConstant;
                Spread = spread; GroupFactor = groupFactor; GroupOf = groupOf;
            }
        }

        /// <summary>
        /// TSB3's free agent value for a position group: round(sum of coef * rating step + the last coef), 0..15.
        /// No coefficient is negative (better ratings never lower it).
        /// </summary>
        private class FreeAgentValueModel
        {
            public string Group;
            public string[] Positions, Ratings;
            public double[] Coef;

            public FreeAgentValueModel(string group, string[] positions, string[] ratings, double[] coef)
            {
                Group = group; Positions = positions; Ratings = ratings; Coef = coef;
            }

            public int Value(Dictionary<string, int> steps)
            {
                double s = Coef[Ratings.Length];
                for (int i = 0; i < Ratings.Length; i++)
                    s += Coef[i] * steps[Ratings[i]];
                return Math.Max(0, Math.Min(15, (int)Math.Round(s)));
            }
        }

        // "C,jerry FONTENOT,Face=0x06,#67,69,50,50,44,38,56," -- position, Face's digits, the ratings
        private static readonly Regex FreeAgentLineRegex = new Regex(
            "^([A-Z]+[1-5]?)\\s*,[^,]*,\\s*Face=0[xX]([0-9a-fA-F]*)([0-9a-fA-F])\\s*,\\s*#\\s*\\d+\\s*,([\\d,\\s]+?),?\\s*(\\[.*)?$");

        /// <summary>
        /// TSB3: 'line' (a player line) with its free agent value -- Face's last digit -- worked out from his
        /// ratings; the first digit (skin tone) and the rest of the line stay. Other lines come back as they are.
        /// </summary>
        public static string UpdateFreeAgentValue(string line)
        {
            return UpdateFreeAgentValue(ModelFor(TSBTool.TSBContentType.TSB3), line);
        }

        private static string UpdateFreeAgentValue(GameModel model, string line)
        {
            Match m = FreeAgentLineRegex.Match(line);
            if (!m.Success)
                return line;
            string pos = m.Groups[1].Value;
            FreeAgentValueModel fav = null;
            foreach (FreeAgentValueModel f in model.FreeAgentValues)
                if (Array.IndexOf(f.Positions, pos) > -1)
                    fav = f;
            string[] names = Array.IndexOf(Linemen, pos) > -1 ? model.Lineman : Player.RatingNamesFor(model, pos);
            string[] values = m.Groups[4].Value.Split(',');
            if (fav == null || names == null || values.Length != names.Length)
                return line;
            Dictionary<string, int> steps = new Dictionary<string, int>();
            for (int i = 0; i < names.Length; i++)
            {
                int rating;
                if (!int.TryParse(values[i].Trim(), out rating))
                    return line;
                steps[names[i]] = (int)Math.Round(rating / 6.25);
            }
            int at = m.Groups[3].Index;
            return line.Substring(0, at) + fav.Value(steps).ToString("x") + line.Substring(at + 1);
        }

        /// <summary>
        /// A team SimData digit (TSB2: rush D, pass D) from the starters. Each position group's score
        /// (TeamDefenseGroups: DL, LB, DB) is the average over its starters of their rating mix (sum of weight *
        /// rating step; Tecmonster's team formula's mixes); raw = sum of coef * group score + the last coef;
        /// value = round(center + stretch * (raw - center)), 0..15. No group weight is negative.
        /// </summary>
        private class TeamDefenseModel
        {
            public string Name;
            public string[][] MixRatings;
            public double[][] MixWeights;
            public double[] Coef;
            public double Stretch, Center;

            public TeamDefenseModel(string name, string[][] mixRatings, double[][] mixWeights, double[] coef, double stretch, double center)
            {
                Name = name; MixRatings = mixRatings; MixWeights = mixWeights; Coef = coef; Stretch = stretch; Center = center;
            }

            public int Value(Dictionary<string, Player> team)
            {
                double raw = Coef[TeamDefenseGroups.Length];
                for (int g = 0; g < TeamDefenseGroups.Length; g++)
                {
                    double sum = 0;
                    foreach (string pos in TeamDefenseGroups[g])
                        for (int r = 0; r < MixRatings[g].Length; r++)
                            sum += MixWeights[g][r] * team[pos].Steps[MixRatings[g][r]];
                    raw += Coef[g] * sum / TeamDefenseGroups[g].Length;
                }
                int value = (int)Math.Round(Center + Stretch * (raw - Center));
                return Math.Max(0, Math.Min(15, value));
            }
        }

        // ---- generated by TsbSimModel ('constants') from tsb2_model.json ----
        private static readonly DigitModel[] Tsb2Digits = {
            new DigitModel("QB pass", new string[] { "PS", "PC", "PA", "AR", "CO" }, new double[] { 0.3547, 0.428, 0.411, 0.2634, 0.0908 },
                new Dictionary<string, double> { { "", -6.1681 } },
                false, 1.119, 5.1845),
            new DigitModel("QB rush", new string[] { "MS", "HP", "BB" }, new double[] { 0.7437, -0.5742, 0.1063 },
                new Dictionary<string, double> { { "", 0.4214 } },
                false, 1.2684, 1.3333),
            new DigitModel("QB scramble", new string[] { "MS", "BB", "HP" }, new double[] { -0.4069, -0.1134, -0.0077 },
                new Dictionary<string, double> { { "", 3.4514 } },
                false, 1.2514, 2.1548),
            new DigitModel("rush", new string[] { "MS", "HP", "BB", "BC", "RP" }, new double[] { 0.2759, 0.1343, -0.0167, -0.0574, 0.1747 },
                new Dictionary<string, double> { { "RB1", 4.1671 }, { "RB2", 1.5752 }, { "RB3", 1.6768 }, { "RB4", 1.2887 }, { "TE1", -0.5709 }, { "TE2", 0.0448 }, { "WR1", 0.4592 }, { "WR2", 0.1885 }, { "WR3", 0.5683 }, { "WR4", 0.2982 } },
                true, 1.0, 3.5036),
            new DigitModel("YAC", new string[] { "MS", "HP", "BB", "RC", "RP" }, new double[] { 0.1806, -0.0264, 0.0682, 0.2981, -0.0094 },
                new Dictionary<string, double> { { "RB1", 0.5235 }, { "RB2", 1.7465 }, { "RB3", 1.416 }, { "RB4", 1.7736 }, { "TE1", 2.8182 }, { "TE2", 2.7871 }, { "WR1", 4.2426 }, { "WR2", 4.4284 }, { "WR3", 3.2901 }, { "WR4", 3.7575 } },
                true, 1.0, 5.6833),
            new DigitModel("catch", new string[] { "RC", "BB", "MS", "RP" }, new double[] { 0.554, 0.0284, 0.1415, 0.1505 },
                new Dictionary<string, double> { { "RB1", -0.0312 }, { "RB2", 0.8584 }, { "RB3", 0.3753 }, { "RB4", -0.1943 }, { "TE1", 1.4808 }, { "TE2", 1.0225 }, { "WR1", 1.688 }, { "WR2", 1.7545 }, { "WR3", 0.59 }, { "WR4", -0.2357 } },
                true, 1.0, 5.8702),
            new DigitModel("tackle", new string[] { "RS", "RP", "MS", "HP", "QU", "PI" }, new double[] { 0.0987, 0.2032, 0.1466, 0.2793, 0.0318, 0.0464 },
                new Dictionary<string, double> { { "DB1", -1.9137 }, { "DB2", -2.2404 }, { "DB3", -2.8575 }, { "FS", -0.9944 }, { "LB5", -2.9889 }, { "LCB", -0.6253 }, { "LE", -1.6452 }, { "LE2", -3.0612 }, { "LILB", -0.0546 }, { "LOLB", -1.4447 }, { "NT", -1.66 }, { "NT2", -3.3584 }, { "RCB", -0.7352 }, { "RE", -1.9021 }, { "RE2", -2.2054 }, { "RILB", 0.9239 }, { "ROLB", -0.4901 }, { "SS", -1.119 } },
                true, 1.3847, 3.3598),
            new DigitModel("K", new string[] { "KP", "KA", "AB" }, new double[] { 0.0967, 0.26, 0.5241 },
                new Dictionary<string, double> { { "", -0.516 } },
                false, 1.5842, 7.131),
            new DigitModel("P", new string[] { "KP", "AB" }, new double[] { 0.5383, 0.1318 },
                new Dictionary<string, double> { { "", 1.3449 } },
                false, 1.874, 6.5238),
        };
        private static readonly ShareModel[] Tsb2Shares = {
            new ShareModel("carry", new string[] { "MS", "HP", "BB" }, new double[] { 0.0933, 0.0925, 0.0136 },
                new Dictionary<string, double> { { "QB1", 1.775 }, { "RB1", 3.4476 }, { "RB2", 2.2749 }, { "RB3", 2.407 }, { "RB4", 1.4136 }, { "WR1", -0.2877 }, { "WR2", -0.4287 }, { "WR3", -0.1629 }, { "WR4", -0.5316 }, { "TE1", -1.2165 }, { "TE2", -1.0077 } },
                0.8638,
                new Dictionary<string, double> { { "QB", 0.8841 }, { "WR", 0.7694 }, { "TE", 1.5029 } },
                new Dictionary<string, string> { { "QB1", "QB" }, { "RB1", "RB" }, { "RB2", "RB" }, { "RB3", "RB" }, { "RB4", "RB" }, { "WR1", "WR" }, { "WR2", "WR" }, { "WR3", "WR" }, { "WR4", "WR" }, { "TE1", "TE" }, { "TE2", "TE" } }),
            new ShareModel("target", new string[] { "MS", "BB", "RC", "HP" }, new double[] { 0.0308, -0.0032, 0.2161, -0.0011 },
                new Dictionary<string, double> { { "RB1", 1.668 }, { "RB2", 1.6644 }, { "RB3", 1.2387 }, { "RB4", 0.2454 }, { "WR1", 1.7732 }, { "WR2", 1.8376 }, { "WR3", 0.8957 }, { "WR4", 0.4449 }, { "TE1", 1.7668 }, { "TE2", 0.7971 } },
                0.8061,
                new Dictionary<string, double> { { "RB", 1.0045 }, { "TE", 0.9165 } },
                new Dictionary<string, string> { { "RB1", "RB" }, { "RB2", "RB" }, { "RB3", "RB" }, { "RB4", "RB" }, { "WR1", "WR" }, { "WR2", "WR" }, { "WR3", "WR" }, { "WR4", "WR" }, { "TE1", "TE" }, { "TE2", "TE" } }),
            new ShareModel("sack", new string[] { "HP", "QU", "MS", "PI" }, new double[] { 0.1438, 0.0409, 0.1442, -0.0839 },
                new Dictionary<string, double> { { "RE", 0.9501 }, { "NT", 0.8343 }, { "LE", 0.7731 }, { "RE2", 0.8518 }, { "NT2", 0.0856 }, { "LE2", -0.1636 }, { "ROLB", 0.1869 }, { "RILB", -0.6553 }, { "LILB", -0.0607 }, { "LOLB", -0.1826 }, { "LB5", -1.0332 }, { "RCB", -0.658 }, { "LCB", -0.864 }, { "DB1", -0.5494 }, { "DB2", -0.6052 }, { "FS", -1.216 }, { "SS", -1.1573 }, { "DB3", -0.9687 } },
                1.0319,
                new Dictionary<string, double> { { "LB", 1.3305 }, { "DB", 1.4231 } },
                new Dictionary<string, string> { { "RE", "DL" }, { "NT", "DL" }, { "LE", "DL" }, { "RE2", "DL" }, { "NT2", "DL" }, { "LE2", "DL" }, { "ROLB", "LB" }, { "RILB", "LB" }, { "LILB", "LB" }, { "LOLB", "LB" }, { "LB5", "LB" }, { "RCB", "DB" }, { "LCB", "DB" }, { "DB1", "DB" }, { "DB2", "DB" }, { "FS", "DB" }, { "SS", "DB" }, { "DB3", "DB" } }),
            new ShareModel("int", new string[] { "PI", "BB", "MS", "HP" }, new double[] { 0.2375, 0.1463, 0.0499, 0.0645 },
                new Dictionary<string, double> { { "RE", -1.5286 }, { "NT", -1.7282 }, { "LE", -1.5334 }, { "RE2", -1.6769 }, { "NT2", -1.7026 }, { "LE2", -1.4499 }, { "ROLB", -0.8501 }, { "RILB", -0.4529 }, { "LILB", -0.9778 }, { "LOLB", -1.3668 }, { "LB5", -1.4614 }, { "RCB", -0.0757 }, { "LCB", -0.0769 }, { "DB1", -0.4549 }, { "DB2", -1.0214 }, { "FS", -0.3323 }, { "SS", -0.1019 }, { "DB3", -1.0683 } },
                0.7473,
                new Dictionary<string, double> { { "DL", 0.5282 }, { "LB", 1.0786 } },
                new Dictionary<string, string> { { "RE", "DL" }, { "NT", "DL" }, { "LE", "DL" }, { "RE2", "DL" }, { "NT2", "DL" }, { "LE2", "DL" }, { "ROLB", "LB" }, { "RILB", "LB" }, { "LILB", "LB" }, { "LOLB", "LB" }, { "LB5", "LB" }, { "RCB", "DB" }, { "LCB", "DB" }, { "DB1", "DB" }, { "DB2", "DB" }, { "FS", "DB" }, { "SS", "DB" }, { "DB3", "DB" } }),
        };
        private static readonly TeamDefenseModel[] Tsb2TeamDefense = {
            new TeamDefenseModel("rush D",
                new string[][] { new string[] { "RS", "RP", "MS", "HP" }, new string[] { "RS", "RP", "MS", "HP" }, new string[] { "RS", "RP", "MS", "HP" } },
                new double[][] { new double[] { 0.05, 0.05, 0.05, 0.85 }, new double[] { 0.25, 0.25, 0.1, 0.4 }, new double[] { 0.25, 0.25, 0.1, 0.4 } },
                new double[] { 0.5427, 0.8871, 2.6899, -19.9446 }, 1.8221, 6.9643),
            new TeamDefenseModel("pass D",
                new string[][] { new string[] { "RS", "RP", "MS", "HP", "PI", "QU" }, new string[] { "RS", "RP", "MS", "PI", "QU" }, new string[] { "RS", "RP", "MS", "PI", "QU" } },
                new double[][] { new double[] { 0.05, 0.05, 0.05, 0.75, 0.05, 0.05 }, new double[] { 0.15, 0.15, 0.1, 0.3, 0.3 }, new double[] { 0.15, 0.15, 0.1, 0.3, 0.3 } },
                new double[] { 0.7615, 0.7615, 0.7615, -9.5274 }, 3.2189, 6.7381),
        };
        private static readonly FreeAgentValueModel[] Tsb2FreeAgentValue = {
        };

        // ---- generated by TsbSimModel ('constants') from tsb3_model.json ----
        private static readonly DigitModel[] Tsb3Digits = {
            new DigitModel("QB pass", new string[] { "PS", "PC", "PA", "AR", "CO" }, new double[] { 0.3689, 0.5776, 0.3921, 0.1788, 0.1534 },
                new Dictionary<string, double> { { "", -7.2875 } },
                false, 1.0882, 5.3333),
            new DigitModel("QB rush", new string[] { "MS", "HP", "BB" }, new double[] { 1.067, -0.0588, 0.0638 },
                new Dictionary<string, double> { { "", -1.107 } },
                false, 1.2433, 1.6667),
            new DigitModel("QB scramble", new string[] { "MS", "BB", "HP" }, new double[] { 0.3213, 0.1604, -0.07 },
                new Dictionary<string, double> { { "", -0.1866 } },
                false, 1.4456, 1.05),
            new DigitModel("rush", new string[] { "MS", "HP", "BB", "BC", "RP" }, new double[] { 0.2614, 0.1675, 0.0124, -0.0811, 0.1274 },
                new Dictionary<string, double> { { "RB1", 4.7523 }, { "RB2", 1.6128 }, { "RB3", 2.5717 }, { "RB4", 1.3314 }, { "TE1", -1.0539 }, { "TE2", -0.5486 }, { "WR1", -0.0576 }, { "WR2", 0.0641 }, { "WR3", -0.0868 }, { "WR4", -0.3907 } },
                true, 1.1886, 3.2733),
            new DigitModel("YAC", new string[] { "MS", "HP", "BB", "RC", "RP" }, new double[] { 0.2532, -0.0145, 0.0557, 0.1595, -0.003 },
                new Dictionary<string, double> { { "RB", 0.8505 }, { "TE", 1.9262 }, { "WR", 4.2014 } },
                false, 1.0, 5.1233),
            new DigitModel("catch", new string[] { "RC", "BB", "MS", "RP" }, new double[] { 0.7806, 0.0204, 0.0884, 0.1671 },
                new Dictionary<string, double> { { "RB", -0.4354 }, { "TE", -0.0035 }, { "WR", -0.9344 } },
                false, 1.0, 5.73),
            new DigitModel("tackle", new string[] { "RS", "RP", "MS", "HP", "QU", "PI" }, new double[] { 0.1282, -0.0059, 0.0326, 0.2449, 0.1735, 0.0538 },
                new Dictionary<string, double> { { "DB1", -1.4844 }, { "DB2", -1.8939 }, { "DB3", -2.6014 }, { "FS", -0.2402 }, { "LB5", -1.9805 }, { "LCB", 0.0252 }, { "LE", -0.4688 }, { "LE2", -2.1201 }, { "LILB", 0.9655 }, { "LOLB", -0.9327 }, { "NT", -0.3116 }, { "NT2", -2.6961 }, { "RCB", -0.1895 }, { "RE", -0.9569 }, { "RE2", -1.4855 }, { "RILB", 2.5467 }, { "ROLB", 0.6505 }, { "SS", -0.4153 } },
                true, 1.2674, 3.4093),
            new DigitModel("K", new string[] { "KP", "KA", "AB" }, new double[] { 0.7415, 0.329, 0.1864 },
                new Dictionary<string, double> { { "", -3.4275 } },
                false, 1.1313, 7.9),
            new DigitModel("P", new string[] { "KP", "AB" }, new double[] { 0.5961, 0.2084 },
                new Dictionary<string, double> { { "", -0.1527 } },
                false, 1.521, 5.9),
        };
        private static readonly ShareModel[] Tsb3Shares = {
            new ShareModel("carry", new string[] { "MS", "HP", "BB" }, new double[] { 0.0799, 0.1081, 0.0284 },
                new Dictionary<string, double> { { "QB1", 1.5177 }, { "RB1", 3.2999 }, { "RB2", 2.0119 }, { "RB3", 2.4632 }, { "RB4", 1.3997 }, { "WR1", -0.4837 }, { "WR2", -0.5692 }, { "WR3", -0.3441 }, { "WR4", -0.4406 }, { "TE1", -1.4069 }, { "TE2", -1.2469 } },
                0.8722,
                new Dictionary<string, double> { { "QB", 0.882 }, { "WR", 0.7603 }, { "TE", 0.855 } },
                new Dictionary<string, string> { { "QB1", "QB" }, { "RB1", "RB" }, { "RB2", "RB" }, { "RB3", "RB" }, { "RB4", "RB" }, { "WR1", "WR" }, { "WR2", "WR" }, { "WR3", "WR" }, { "WR4", "WR" }, { "TE1", "TE" }, { "TE2", "TE" } }),
            new ShareModel("target", new string[] { "MS", "BB", "RC", "HP" }, new double[] { 0.0482, 0.021, 0.2183, 0.017 },
                new Dictionary<string, double> { { "RB1", 1.4383 }, { "RB2", 1.5295 }, { "RB3", 0.6366 }, { "RB4", 0.0822 }, { "WR1", 1.4561 }, { "WR2", 1.5299 }, { "WR3", 0.8705 }, { "WR4", 0.2407 }, { "TE1", 1.6536 }, { "TE2", 0.3933 } },
                0.7686,
                new Dictionary<string, double> { { "RB", 0.8999 }, { "TE", 0.8712 } },
                new Dictionary<string, string> { { "RB1", "RB" }, { "RB2", "RB" }, { "RB3", "RB" }, { "RB4", "RB" }, { "WR1", "WR" }, { "WR2", "WR" }, { "WR3", "WR" }, { "WR4", "WR" }, { "TE1", "TE" }, { "TE2", "TE" } }),
            new ShareModel("sack", new string[] { "HP", "QU", "MS", "PI" }, new double[] { 0.1329, 0.0569, 0.1006, -0.0206 },
                new Dictionary<string, double> { { "RE", 1.1618 }, { "NT", 0.7243 }, { "LE", 0.9654 }, { "RE2", 1.0495 }, { "NT2", 0.3564 }, { "LE2", -0.3987 }, { "ROLB", 0.7355 }, { "RILB", 0.212 }, { "LILB", 0.4529 }, { "LOLB", 0.0508 }, { "LB5", -1.1934 }, { "RCB", -0.8506 }, { "LCB", -0.9674 }, { "DB1", -0.8955 }, { "DB2", -1.1968 }, { "FS", -1.5892 }, { "SS", -1.2117 }, { "DB3", -1.5533 } },
                1.0682,
                new Dictionary<string, double> { { "LB", 1.1269 }, { "DB", 1.112 } },
                new Dictionary<string, string> { { "RE", "DL" }, { "NT", "DL" }, { "LE", "DL" }, { "RE2", "DL" }, { "NT2", "DL" }, { "LE2", "DL" }, { "ROLB", "LB" }, { "RILB", "LB" }, { "LILB", "LB" }, { "LOLB", "LB" }, { "LB5", "LB" }, { "RCB", "DB" }, { "LCB", "DB" }, { "DB1", "DB" }, { "DB2", "DB" }, { "FS", "DB" }, { "SS", "DB" }, { "DB3", "DB" } }),
            new ShareModel("int", new string[] { "PI", "BB", "MS", "HP" }, new double[] { 0.1759, 0.0327, 0.0644, 0.0028 },
                new Dictionary<string, double> { { "RE", -0.6499 }, { "NT", -0.6338 }, { "LE", -0.648 }, { "RE2", -0.569 }, { "NT2", -0.6098 }, { "LE2", -0.4103 }, { "ROLB", 0.1803 }, { "RILB", 0.6951 }, { "LILB", 0.2619 }, { "LOLB", -0.3082 }, { "LB5", -0.2335 }, { "RCB", 1.3466 }, { "LCB", 1.6196 }, { "DB1", 1.212 }, { "DB2", 0.7224 }, { "FS", 1.6077 }, { "SS", 1.8104 }, { "DB3", 0.5391 } },
                0.8711,
                new Dictionary<string, double> { { "DL", 0.0 }, { "LB", 1.4657 } },
                new Dictionary<string, string> { { "RE", "DL" }, { "NT", "DL" }, { "LE", "DL" }, { "RE2", "DL" }, { "NT2", "DL" }, { "LE2", "DL" }, { "ROLB", "LB" }, { "RILB", "LB" }, { "LILB", "LB" }, { "LOLB", "LB" }, { "LB5", "LB" }, { "RCB", "DB" }, { "LCB", "DB" }, { "DB1", "DB" }, { "DB2", "DB" }, { "FS", "DB" }, { "SS", "DB" }, { "DB3", "DB" } }),
        };
        private static readonly TeamDefenseModel[] Tsb3TeamDefense = {
        };
        private static readonly FreeAgentValueModel[] Tsb3FreeAgentValue = {
            new FreeAgentValueModel("QB", new string[] { "QB1", "QB2" }, new string[] { "PS", "PC", "AR" }, new double[] { 0.7348, 0.9696, 0.3565, -9.616 }),
            new FreeAgentValueModel("RB", new string[] { "RB1", "RB2", "RB3", "RB4" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "BC", "RC" }, new double[] { 0.0, 0.4451, 1.2559, 0.6867, 0.2662, 0.0, 0.1085, 0.5332, -14.8549 }),
            new FreeAgentValueModel("WR", new string[] { "WR1", "WR2", "WR3", "WR4" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "BC", "RC" }, new double[] { 0.0, 0.0, 0.4617, 0.7383, 0.1721, 0.1651, 0.0303, 1.3191, -12.4939 }),
            new FreeAgentValueModel("TE", new string[] { "TE1", "TE2" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "BC", "RC" }, new double[] { 0.3022, 0.1379, 0.3189, 0.7564, 0.0, 0.3145, 0.1166, 0.9406, -14.3448 }),
            new FreeAgentValueModel("OL", new string[] { "C", "LG", "RG", "LT", "RT" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG" }, new double[] { 0.0, 0.1931, 0.2381, 1.6382, 0.0, 0.0483, -10.4936 }),
            new FreeAgentValueModel("DL", new string[] { "RE", "NT", "LE", "RE2", "NT2", "LE2" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "PI", "QU" }, new double[] { 0.0161, 0.2343, 0.4269, 1.2909, 0.3385, 0.0, 0.3696, 0.1807, -11.6474 }),
            new FreeAgentValueModel("LB", new string[] { "ROLB", "RILB", "LILB", "LOLB", "LB5" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "PI", "QU" }, new double[] { 0.1509, 0.3969, 0.4555, 0.6817, 0.5348, 0.3923, 0.0281, 0.0, -10.8127 }),
            new FreeAgentValueModel("CB", new string[] { "RCB", "LCB", "DB1", "DB2" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "PI", "QU" }, new double[] { 0.4504, 0.101, 0.6933, 0.1164, 0.1226, 0.3063, 0.4663, 0.2934, -13.4473 }),
            new FreeAgentValueModel("S", new string[] { "FS", "SS", "DB3" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "PI", "QU" }, new double[] { 0.2179, 0.2199, 0.4635, 0.3451, 0.0, 0.2496, 0.487, 0.374, -10.9891 }),
            new FreeAgentValueModel("K", new string[] { "K" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "KP", "KA", "AB" }, new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.4917, 0.3523, 0.2745, -1.6976 }),
            new FreeAgentValueModel("P", new string[] { "P" }, new string[] { "RS", "RP", "MS", "HP", "BB", "AG", "KP", "AB" }, new double[] { 0.0, 0.0, 0.0, 0.0, 0.3095, 0.0, 0.7834, 0.3943, -3.2668 }),
        };
        // ---- end of generated constants ----
    }
}
