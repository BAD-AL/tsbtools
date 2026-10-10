using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TsbSimModel
{
    /// <summary>
    /// Writes the expected output of TSBTool's Auto Update Sim for a roster text, worked out here from the
    /// model file -- the test files TSBTool_Test\TSB2SimModel\*_autoupdated.txt, which TsbSimAutoUpdaterTests
    /// hold TSBTool to. Only the [..] sim data of the player lines changes, and for TSB3 the free agent value
    /// (Face's last digit, FreeAgentValues) of every player line.
    /// </summary>
    public static class ExpectedWriter
    {
        private static readonly Regex TeamLine = new Regex(@"^TEAM\s*=\s*(\w+)");

        public static string AutoUpdated(Game game, FittedModel model, string rosterText)
        {
            string[] lines = rosterText.Split('\n');
            string[] output = (string[])lines.Clone();
            Team block = new Team();
            Dictionary<string, int> lineOf = new Dictionary<string, int>();
            string teamName = null;

            Action flush = () =>
            {
                if (block.Players.Count == 0)
                    return;
                bool keep = teamName != null && Array.IndexOf(game.NotTeams, teamName) > -1;
                Dictionary<string, int[]> bytes = model.TeamBytes(block, keep);
                foreach (KeyValuePair<string, int[]> kv in bytes)
                {
                    int i = lineOf[kv.Key];
                    string sim = kv.Key == "K" || kv.Key == "P"
                        ? string.Format("[{0:X}]", kv.Value[0])
                        : "[" + string.Join(",", Array.ConvertAll(kv.Value, v => v.ToString("X2"))) + "]";
                    output[i] = NoSim(lines[i].TrimEnd('\r')) + "," + sim + (lines[i].EndsWith("\r") ? "\r" : "");
                }
                block = new Team();
                lineOf.Clear();
            };

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');
                Match t = TeamLine.Match(line);
                if (t.Success || line.StartsWith("SEASON"))
                {
                    flush();
                    teamName = t.Success ? t.Groups[1].Value : null;
                    continue;
                }
                // one player at a time through the roster parser
                List<Team> one = Rosters.Parse(game, "TEAM = x\n" + line);
                if (one.Count == 1 && one[0].Players.Count == 1)
                    foreach (Player p in one[0].Players.Values)
                    {
                        block.Players[p.Position] = p;
                        lineOf[p.Position] = i;
                    }
            }
            flush();
            // TSB3: every player line's free agent value (Face's last digit), linemen and free agents too
            if (model.FreeAgentValue.Count > 0)
                for (int i = 0; i < output.Length; i++)
                {
                    string cr = output[i].EndsWith("\r") ? "\r" : "";
                    output[i] = FreeAgentValues.Update(game, model, output[i].TrimEnd('\r')) + cr;
                }
            return string.Join("\n", output);
        }

        // as TsbSimAutoUpdater.TeamSimDataRegex: the first two digits (a TSB1 text's 3rd is left as it is)
        private static readonly Regex TeamSimDigits = new Regex(@"SimData\s*=\s*0[xX]([0-9a-fA-F]{2})");

        /// <summary>
        /// The expected team SimData of TSBTool's full Auto Update Sim (AutoUpdateSimData) for a roster text:
        /// its "SEASON n" and team lines, each team line with the model's digits (TSB2's rush D, pass D) --
        /// TSBTool_Test\TSB2SimModel\tsb2_stock_team_simdata.txt. A team the model has no value for (a starter
        /// missing; any TSB3 team) keeps its line.
        /// </summary>
        public static string TeamSimData(Game game, FittedModel model, string rosterText)
        {
            List<Team> teams = Rosters.Parse(game, rosterText);
            StringBuilder sb = new StringBuilder();
            int next = 0;
            foreach (string raw in rosterText.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                if (line.StartsWith("SEASON"))
                    sb.Append(line + "\n");
                else if (TeamLine.IsMatch(line))
                {
                    Team t = teams[next++];
                    int[] digits = Array.IndexOf(game.NotTeams, t.Name) > -1 ? null : model.TeamSimData(t);
                    sb.Append((digits == null ? line : TeamLineWith(line, digits)) + "\n");
                }
            }
            return sb.ToString();
        }

        /// <summary>'line' (a team line) with its SimData digits replaced, or ",SimData=0x.." added when it has none.</summary>
        public static string TeamLineWith(string line, int[] digits)
        {
            string hex = string.Format("{0:X}{1:X}", digits[0], digits[1]);
            Match m = TeamSimDigits.Match(line);
            if (!m.Success)
                return line + ",SimData=0x" + hex;
            return line.Substring(0, m.Groups[1].Index) + hex + line.Substring(m.Groups[1].Index + 2);
        }

        private static string NoSim(string line)
        {
            int i = line.IndexOf('[');
            return (i >= 0 ? line.Substring(0, i) : line).TrimEnd(',', ' ');
        }
    }
}
