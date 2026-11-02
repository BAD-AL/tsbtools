using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TSBTool;

public static partial class WebHelper
{
    static ITecmoContent tool;

    static void Main() { Console.WriteLine("TSBToolWeb runtime started"); }

    /// <summary>Returns the ROM type name (e.g. SNES_TSB1), or null when the bytes are not a TSB ROM.</summary>
    [JSExport]
    public static string LoadRom(byte[] rom)
    {
        if (StaticUtils.sMessageGiver == null) StaticUtils.sMessageGiver = new WebMessageGiver();
        WebMessageGiver.Clear();
        TecmoTool.ShowPlaybook = true;
        TecmoTool.ShowTeamFormation = true;
        TecmoTool.ShowTeamStrings = true;       // adds the TEAM_ABB/CITY/NAME line to the NES/SNES TSB1/Genesis dumps
        TecmoTool.ShowColors = true;            // adds the COLORS line (uniforms/helmets) to the NES/SNES TSB1/Genesis dumps
        tool = TecmoToolFactory.GetToolForRom(rom);
        if (tool == null) return null;
        tool.ShowOffPref = true;
        return tool.RomVersion.ToString();
    }

    /// <summary>The TSBTool version (the assembly version from TSBTool\AssemblyInfo.cs, shared with the desktop program).</summary>
    [JSExport]
    public static string GetVersion() => typeof(WebHelper).Assembly.GetName().Version.ToString();

    [JSExport]
    public static string GetAll(int season) => tool.GetKey() + tool.GetAll(season) + tool.GetSchedule(season);

    /// <summary>The Pro Bowl rosters ("AFC,QB1,bills,QB1" lines), which are not part of the normal dump.</summary>
    [JSExport]
    public static string GetProBowl(int season) => tool.GetProBowlPlayers(season);

    [JSExport]
    public static void ProcessText(string text) => tool.ProcessText(text);

    [JSExport]
    public static byte[] GetRom() => tool.OutputRom;

    /// <summary>What kind of dump the text looks like: TSB1, TSB2, TSB3 or Unknown (the desktop's content check).</summary>
    [JSExport]
    public static string ContentType(string text) => StaticUtils.GetContentType(text).ToString();

    /// <summary>The kind of text the loaded ROM takes (the desktop's mapping: SNES TSB2 and TSB3 have their own, everything else is TSB1).</summary>
    [JSExport]
    public static string RomContentType() =>
        tool.RomVersion == ROM_TYPE.SNES_TSB2 ? "TSB2" : tool.RomVersion == ROM_TYPE.SNES_TSB3 ? "TSB3" : "TSB1";

    /// <summary>
    /// Comments out the COLORS lines whose uniform format belongs to another kind of ROM (6-digit NES colors, 28-digit SNES TSB1 or Genesis colors,
    /// 48-digit TSB2/TSB3 colors), so text copied from, say, an NES ROM applies to a SNES ROM instead of failing on every team. Says so in the messages.
    /// </summary>
    [JSExport]
    public static string DropForeignColors(string text)
    {
        string mine = ColorsFamily(tool.RomVersion);
        int dropped = 0;
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith("COLORS")) continue;
            string family = ColorsFamily(lines[i]);
            if (family == null || family == mine) continue;
            lines[i] = "#" + lines[i];
            dropped++;
        }
        if (dropped == 0) return text;
        WebMessageGiver.Add("Left out " + dropped + " COLORS lines: their uniform colors are for a different kind of ROM than the one loaded.");
        return string.Join("\n", lines);
    }

    static string ColorsFamily(ROM_TYPE v)
    {
        switch (v)
        {
            case ROM_TYPE.SNES_TSB1: return "snes1";
            case ROM_TYPE.SNES_TSB2: case ROM_TYPE.SNES_TSB3: return "tsb23";
            case ROM_TYPE.GENESIS_TSB1: return "genesis";
            default: return "nes";
        }
    }

    /// <summary>Which kind of ROM a COLORS line is for, from its keys and the length of Uniform1; null when it cannot tell.</summary>
    static string ColorsFamily(string line)
    {
        if (line.Contains("DivChamp")) return "nes";
        if (line.Contains("InGame")) return "genesis";
        System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(line, @"Uniform1\s*=\s*0[xX]([0-9a-fA-F]+)");
        if (!m.Success) return null;
        if (m.Groups[1].Length == 28) return "snes1";
        if (m.Groups[1].Length == 48) return "tsb23";
        return null;
    }

    /// <summary>
    /// Converts the text between TSB1, TSB2 and TSB3 formats (the desktop's Convert menu and its prompt on Apply). The result is approximate,
    /// and TSB1 takes one season only.
    /// </summary>
    [JSExport]
    public static string ConvertText(string text, string to)
    {
        TSBContentType from = StaticUtils.GetContentType(text);
        if (from == TSBContentType.Unknown) throw new InvalidOperationException("The text is not recognized as TSB1, TSB2 or TSB3 content.");
        TSBContentType target = (TSBContentType)Enum.Parse(typeof(TSBContentType), to);
        if (target == TSBContentType.TSB1 && from != TSBContentType.TSB1 && System.Text.RegularExpressions.Regex.Matches(text, "SEASON ").Count > 1)
            throw new InvalidOperationException("Could not convert multiple seasons. Please have only 1 season of data in the text.");
        return TSBTool2.TecmoConverter.Convert(from, target, text);
    }

    /// <summary>
    /// The desktop's Auto Update Sim, on the whole dump text: every player's sim values from his ratings, then every team's
    /// rush/pass defense. NES/SNES TSB1/Genesis can use Tecmonster's formulas or TSBTool's for the offense and kickers;
    /// TSB2/TSB3 have one model (the formula flag is ignored there).
    /// </summary>
    [JSExport]
    public static string AutoUpdateSim(string text, bool tsbtoolFormulas)
    {
        ROM_TYPE v = tool.RomVersion;
        if (v == ROM_TYPE.SNES_TSB2 || v == ROM_TYPE.SNES_TSB3)
            return TSBTool2.TSBXSimAutoUpdater.AutoUpdateSimData(text, v == ROM_TYPE.SNES_TSB2 ? TSBContentType.TSB2 : TSBContentType.TSB3);
        if (tsbtoolFormulas)
        {
            text = SimFormulas.UpdatePlayers(text);
            text = TecmonsterTSB1SimAutoUpdater.AutoUpdateDefenseSimData(text);   // sack and INT shares: shared by both paths
        }
        else
            text = TecmonsterTSB1SimAutoUpdater.AutoUpdatePlayerSimData(text);
        return new TecmonsterTeamSim().AutoUpdateSeasonSimData(1, text);
    }

    /// <summary>Messages the core logged since the last call (warnings, auto-fix notices, errors), one per line.</summary>
    [JSExport]
    public static string TakeMessages() => WebMessageGiver.Take();
}

public class WebMessageGiver : MessageGiver
{
    static readonly List<string> sMessages = new List<string>();

    public static void Clear() { sMessages.Clear(); }

    public static void Add(string m) { sMessages.Add(m); }

    public static string Take()
    {
        string s = string.Join("\n", sMessages);
        sMessages.Clear();
        return s;
    }

    public void LogMessage(string m) { Console.WriteLine(m); }
    public string PromptForSetUserInput(string i) { return null; }
    public bool ShowConfirmationDialog(string t, string m) { sMessages.Add(m); return true; }
    public void ShowError(string t, string m) { sMessages.Add(m); }
    public void ShowMessageBox(string t, string m) { sMessages.Add(m); }
}
