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

    [JSExport]
    public static string GetAll(int season) => tool.GetKey() + tool.GetAll(season) + tool.GetSchedule(season);

    /// <summary>The Pro Bowl rosters ("AFC,QB1,bills,QB1" lines), which are not part of the normal dump.</summary>
    [JSExport]
    public static string GetProBowl(int season) => tool.GetProBowlPlayers(season);

    [JSExport]
    public static void ProcessText(string text) => tool.ProcessText(text);

    [JSExport]
    public static byte[] GetRom() => tool.OutputRom;

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
