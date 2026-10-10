using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TsbSimModel
{
    /// <summary>One player line: his ratings (raw 6..100 and as 1..16 steps) and his sim bytes.</summary>
    public class Player
    {
        public string Position;
        public Dictionary<string, int> Raw = new Dictionary<string, int>();
        public Dictionary<string, int> Level = new Dictionary<string, int>();
        public int[] Sim;
    }

    /// <summary>
    /// One team of one season. 'Group' is what the held-out check leaves out: the season (TSB2) or the
    /// fold (TSB3, see Game.FoldsByTeam).
    /// </summary>
    public class Team
    {
        public int Group;
        public string Name;
        public Dictionary<string, Player> Players = new Dictionary<string, Player>();
        /// <summary>The team line's SimData digits (TSB2: rush defense, pass defense); -1 when it has none.</summary>
        public int RushD = -1, PassD = -1;

        public string Key { get { return Group + "/" + Name; } }

        /// <summary>Season/fold, then team name in ordinal order (as the original Python sorted its keys).</summary>
        public static int Compare(Team a, Team b)
        {
            int c = a.Group.CompareTo(b.Group);
            return c != 0 ? c : string.CompareOrdinal(a.Name, b.Name);
        }
    }

    public static class Rosters
    {
        // "QB1,jim KELLY,Face=0x00,#12,69,25,...,75,[08,01,C3]" (players with sim data only)
        private static readonly Regex PlayerLine = new Regex(@"^(\w+),[^,]*,Face=0x\w+,#\d+,([\d,]+?),?\[([0-9A-F,]+)\]");
        private static readonly Regex SeasonLine = new Regex(@"^SEASON (\d)");
        private static readonly Regex TeamLine = new Regex(@"^TEAM = (\w+)");
        private static readonly Regex TeamSimData = new Regex(@"SimData=0x([0-9A-Fa-f])([0-9A-Fa-f])(?![0-9A-Fa-f])");

        /// <summary>A rating 6..100 as its 1..16 step (ratings are 6.25 apart). Rounds half to even, like Python.</summary>
        public static int Level(int rating)
        {
            return (int)Math.Round(rating / 6.25);
        }

        /// <summary>
        /// The ROM's rosters as TSBTool text, one "SEASON n" block per stock season (TSB2Tool.GetTeams).
        /// TSBTool's LB sim-order fix (2026-10-09) matters here: earlier versions swapped ROLB/LOLB and
        /// RILB/LILB sim bytes.
        /// </summary>
        public static string RosterText(Game game, string romPath)
        {
            TSBTool2.TSB2Tool tool = LoadTool(game, romPath);
            StringBuilder sb = new StringBuilder();
            foreach (int season in game.Seasons)
            {
                sb.Append("SEASON " + season + "\n");
                sb.Append(tool.GetTeams(season));
            }
            return sb.ToString();
        }

        public static TSBTool2.TSB2Tool LoadTool(Game game, string romPath)
        {
            TSBTool.ITecmoContent content = TSBTool.TecmoToolFactory.GetToolForRom(File.ReadAllBytes(romPath));
            TSBTool2.TSB2Tool tool = content as TSBTool2.TSB2Tool;
            bool isTsb3 = tool is TSBTool2.TSB3Tool;
            if (tool == null || isTsb3 != (game == Game.Tsb3))
                throw new ArgumentException(string.Format("'{0}' isn't a {1} ROM.", romPath, game.Name));
            return tool;
        }

        /// <summary>Every team in 'text' (TSBTool text), in text order, with its season as Group.</summary>
        public static List<Team> Parse(Game game, string text)
        {
            List<Team> teams = new List<Team>();
            int season = 1;
            Team team = null;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                Match m = SeasonLine.Match(line);
                if (m.Success)
                {
                    season = int.Parse(m.Groups[1].Value);
                    continue;
                }
                m = TeamLine.Match(line);
                if (m.Success)
                {
                    team = new Team { Group = season, Name = m.Groups[1].Value };
                    Match sim = TeamSimData.Match(line);
                    if (sim.Success)
                    {
                        team.RushD = Convert.ToInt32(sim.Groups[1].Value, 16);
                        team.PassD = Convert.ToInt32(sim.Groups[2].Value, 16);
                    }
                    teams.Add(team);
                    continue;
                }
                m = PlayerLine.Match(line);
                if (!m.Success || team == null)
                    continue;
                string pos = m.Groups[1].Value;
                string[] names = game.RatingsFor(pos);
                if (names == null)
                    continue;
                Player p = new Player { Position = pos };
                string[] vals = m.Groups[2].Value.Split(',');
                for (int i = 0; i < names.Length && i < vals.Length; i++)
                {
                    int v = int.Parse(vals[i]);
                    p.Raw[names[i]] = v;
                    p.Level[names[i]] = Level(v);
                }
                p.Sim = Array.ConvertAll(m.Groups[3].Value.Split(','), s => Convert.ToInt32(s, 16));
                team.Players[pos] = p;
            }
            return teams;
        }

        /// <summary>
        /// The teams the model is fitted on, sorted (Team.Compare): TSB2 every season's teams; TSB3 the 30
        /// real teams in 3 folds by text order (not the free agents / all-time team).
        /// </summary>
        public static List<Team> ForFitting(Game game, List<Team> parsed)
        {
            List<Team> ret = new List<Team>();
            int i = 0;
            foreach (Team t in parsed)
            {
                if (Array.IndexOf(game.NotTeams, t.Name) > -1)
                    continue;
                if (game.FoldsByTeam)
                    t.Group = i % 3 + 1;
                ret.Add(t);
                i++;
            }
            ret.Sort(Team.Compare);
            return ret;
        }
    }
}
