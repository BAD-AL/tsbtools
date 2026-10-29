using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TSBTool;

/// <summary>
/// The desktop AttributeForm's "TSBTool formulas" Auto Update Sim for the NES/SNES TSB1/Genesis dumps: every QB, RB, WR, TE,
/// K and P gets his sim values worked out from his ratings with SimStuff. (Defenders are handled by
/// TecmonsterTSB1SimAutoUpdater.AutoUpdateDefenseSimData in both of the desktop's paths.)
/// </summary>
public static class SimFormulas
{
    static readonly Regex PlayerRe = new Regex(
        @"^(?<pre>(?<pos>QB[12]|RB[1-4]|WR[1-4]|TE[12]|K|P),\s*.+?,\s*Face=0x[0-9a-fA-F]+,\s*#\d+\s*,?\s*)(?<attrs>[^\[]*?)\s*(?<sim>\[[^\]]*\])?\s*$");

    public static string UpdatePlayers(string text)
    {
        var sim = new SimStuff();
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i], eol = "";
            if (line.EndsWith("\r")) { line = line.Substring(0, line.Length - 1); eol = "\r"; }
            Match m = PlayerRe.Match(line);
            if (!m.Success) continue;
            string pos = m.Groups["pos"].Value;
            var attrs = new List<int>();
            foreach (string t in m.Groups["attrs"].Value.Split(','))
            {
                int v;
                if (int.TryParse(t.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) attrs.Add(v);
            }
            string bracket = null;
            if (pos.StartsWith("QB"))
            {
                if (attrs.Count > 7)    // RS RP MS HP PS PC PA APB
                    bracket = string.Format("[{0}, {1}, {2} ]", sim.QbSimRun(attrs[2]), sim.SimPass(attrs[5], attrs[7], attrs[4]), sim.SimPocket(attrs[2]));
            }
            else if (pos == "K" || pos == "P")
            {
                if (attrs.Count > 5)    // RS RP MS HP KA AKB
                    bracket = string.Format("[{0} ]", sim.PKSimKick(attrs[4], attrs[5]));
            }
            else if (attrs.Count > 5)   // RB, WR, TE: RS RP MS HP BC REC (the desktop uses the RB formulas for all three)
            {
                bracket = string.Format("[{0}, {1}, {2}, {3} ]",
                    sim.RbSimRush(attrs[2], attrs[3], attrs[4], attrs[0]), sim.RbSimCatch(attrs[5]),
                    sim.SimYPC(pos, attrs[2], attrs[5]), SimStuff.SimTargets(pos));
            }
            if (bracket == null) continue;
            string head = m.Groups["pre"].Value + m.Groups["attrs"].Value.TrimEnd(' ', ',');   // the line may already end in " ,"
            lines[i] = head + " ," + bracket + eol;
        }
        return string.Join("\n", lines);
    }
}
