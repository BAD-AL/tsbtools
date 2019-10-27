using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace TSBTool2
{
    public class TSBXSimAutoUpdater
    {
        public static void ReloadFormulas()
        {
            // force a re-load of the 'sSimFormulas' file when calling 'SimFormulas' getter
            sSimFormulas = null;
        }

        public static string AutoUpdatePlayerSimData(string text, TSBContentType gameVersion)
        {
            ReloadFormulas();
            if (gameVersion == TSBContentType.TSB3 )
                sSubLines = sSubLinesTSB3; // TSB# has the 'AG' attribute.
            else
                sSubLines = sSubLinesTSB2;
            TSB3Converter.ReloadFormulas();
            StringBuilder builder = new StringBuilder(text.Length);
            string[] lines = text.Split("\n".ToCharArray());
            foreach (string line in lines)
            {
                if (line.StartsWith("#"))
                    builder.Append(line);
                else if (IsPlayerLine(line))
                {
                    try
                    {
                        builder.Append(UpdatePlayerSimValues(line, gameVersion));
                    }
                    catch (Exception ex)
                    {
                        StaticUtils.ShowError("Error Processing line: " + line + 
                            "\nOperation not applied" + ex.ToString());
                        return text;
                    }
                }
                else
                    builder.Append(line);

                builder.Append("\n");
            }
            return builder.ToString();
        }

        private static bool IsPlayerLine(string line)
        {
            if (InputParser.posNameFaceRegex.Match(line) != Match.Empty)
                return true;
            return false;
        }

        private static string UpdatePlayerSimValues(string playerLine, TSBContentType gameVersion)
        {
            int simIndex = playerLine.IndexOf("[");
            string[] parts = null;
            string playerNoSim = null;
            if (simIndex > -1)
            {
                playerNoSim = playerLine.Substring(0, simIndex - 1);
                parts = playerNoSim.Split(",".ToCharArray());
            }
            else
            {
                playerNoSim = playerLine;
                parts = playerLine.Split(",".ToCharArray());
            }
            List<byte> simVals = new List<byte>();
            string pos = parts[0];
            byte sim3, sim4;
            #region Big Switch
            switch (pos)
            {
                case "QB1": case "QB2":
                    simVals.Add((byte)Calculate("QB_SIM_CARY", sSubLines["QB"], parts, 0, 0x1C));
                    simVals.Add((byte) Calculate("QB_SIM_RUSHING", sSubLines["QB"], parts, 0,8));
                    sim3 = (byte) Calculate("QB_SIM_PASSING", sSubLines["QB"], parts, 1, 0x0F);
                    sim4 = (byte) Calculate("QB_SIM_SCRAMBLE",sSubLines["QB"], parts,0, 3);
                    simVals.Add((byte) ( sim3 << 4 + sim4));
                    break;
                case "RB1": case "RB2": case "RB3": case "RB4":
                    simVals.Add((byte)Calculate("RB_SIM_RUSHING", sSubLines["SKILL"], parts, 0, 0xAD));
                    simVals.Add((byte)Calculate("RB_SIM_CARRIES", sSubLines["SKILL"], parts, 0, 0x0B));
                    simVals.Add((byte)Calculate("RB_SIM_RETURN", sSubLines["SKILL"], parts,0, 0xFF));
                    sim3 = (byte)Calculate("RB_SIM_YPC", sSubLines["SKILL"], parts, 0,0x0F);
                    sim4 = (byte)Calculate("RB_SIM_CATCH", sSubLines["SKILL"], parts, 0, 0x0F);
                    simVals.Add((byte)(sim3 << 8 + sim4));
                    break;
                case "WR1": case "WR2": case "WR3": case "WR4":
                    simVals.Add((byte)Calculate("WR_SIM_RUSHING", sSubLines["SKILL"], parts, 0, 0xAD));
                    simVals.Add((byte)Calculate("WR_SIM_CARRIES", sSubLines["SKILL"], parts, 0, 0x0B));
                    simVals.Add((byte)Calculate("WR_SIM_RETURN", sSubLines["SKILL"], parts, 0,0xFF));
                    sim3 = (byte)Calculate("WR_SIM_YPC", sSubLines["SKILL"], parts, 0,0x0F);
                    sim4 = (byte)Calculate("WR_SIM_CATCH", sSubLines["SKILL"], parts, 0,0x0F);
                    simVals.Add((byte)(sim3 << 8 + sim4));
                    break;
                case "TE1": case "TE2":
                    simVals.Add((byte)Calculate("TE_SIM_RUSHING", sSubLines["SKILL"], parts, 0, 0xAD));
                    simVals.Add((byte)Calculate("TE_SIM_CARRIES", sSubLines["SKILL"], parts, 0, 0x0B));
                    simVals.Add((byte)Calculate("TE_SIM_RETURN", sSubLines["SKILL"], parts, 0, 0xFF));
                    sim3 = (byte)Calculate("TE_SIM_YPC", sSubLines["SKILL"], parts, 0,0x0F);
                    sim4 = (byte)Calculate("TE_SIM_CATCH", sSubLines["SKILL"], parts,0,0x0F);
                    simVals.Add((byte)(sim3 << 8 + sim4));
                    break;
                
                case "RE": case "NT": case "LE": 
                case "RE2": case "NT2": case "LE2":
                    simVals.Add((byte)Calculate("DL_SIM_SACKING", sSubLines["DEF"], parts,0,0xFF));
                    simVals.Add((byte)Calculate("DL_SIM_INT", sSubLines["DEF"], parts,0,0xFF));
                    simVals.Add((byte)Calculate("DL_SIM_TACKLING", sSubLines["DEF"], parts, 0, 0x0A));
                    break;
                case "ROLB": case "RILB": case "LILB": 
                case "LOLB": case "LB5":
                    simVals.Add((byte)Calculate("LB_SIM_SACKING", sSubLines["DEF"], parts, 0, 0xFF));
                    simVals.Add((byte)Calculate("LB_SIM_INT", sSubLines["DEF"], parts, 0, 0xFF));
                    simVals.Add((byte)Calculate("LB_SIM_TACKLING", sSubLines["DEF"], parts, 0, 0x0A));
                    break;
                case "RCB": case "LCB": case "DB1": case "DB2":
                    simVals.Add((byte)Calculate("CB_SIM_SACKING", sSubLines["DEF"], parts, 0, 0xFF));
                    simVals.Add((byte)Calculate("CB_SIM_INT", sSubLines["DEF"], parts, 0, 0xFF));
                    simVals.Add((byte)Calculate("CB_SIM_TACKLING", sSubLines["DEF"], parts, 0, 0x0A));
                    break;
                case "FS": case "SS": case "DB3":
                    simVals.Add((byte)Calculate("S_SIM_SACKING", sSubLines["DEF"], parts, 0, 0xFF));
                    simVals.Add((byte)Calculate("S_SIM_INT", sSubLines["DEF"], parts, 0, 0xFF));
                    simVals.Add((byte)Calculate("S_SIM_TACKLING", sSubLines["DEF"], parts, 0, 0x0A));
                    break;
                case "K":
                    simVals.Add((byte)Calculate("K_SIM_ABILITY", sSubLines["K"], parts, 0, 0x0F));
                    break;
                case "P":
                    simVals.Add((byte)Calculate("P_SIM_ABILITY", sSubLines["P"], parts, 0, 0x0F));
                    break;
            }
            #endregion
            StringBuilder sb = new StringBuilder(30);
            sb.Append(playerNoSim);
            if (simVals.Count > 0)
            {
                sb.Append(",[");
                foreach (byte b in simVals)
                {
                    sb.Append(b.ToString("X2"));
                    sb.Append(",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("]");
            }
            string retVal = sb.ToString();
            if (gameVersion == TSBContentType.TSB3)
                retVal = TSB3Converter.UpdateFreeAgentValue(retVal);
            return retVal;
        }

        private static double Calculate(string formulaName, string substString, string[] playerParts, int min, int max)
        {
            double result = 0;
            string formula = GetFormula(formulaName);
            string[] sub_parts = substString.Split(",".ToCharArray());
            for (int i = 4; i < sub_parts.Length; i++) // start at 'RS'
            {
                formula = formula.Replace(sub_parts[i].Trim(), playerParts[i]);
            }
            try
            {
                string r = StaticUtils.Compute(formula).ToString();
                result = Double.Parse(r);
            }
            catch (Exception )
            {
                throw new ArgumentException(String.Format(
                    "Error processing formula '{0}'\nTrying to calculate:'{1}'", formulaName, formula));
            }
            if (result < min)
                result = min;
            else if (result > max)
                result = max;
            return result;
        }

        private static string GetFormula(string formulaName)
        {
            string retVal = "";
            Regex formulaRegex = new Regex(String.Format("^\\s*{0}\\s*:\\s*(.*)$", formulaName), RegexOptions.Multiline);
            Match m = formulaRegex.Match(SimFormulas);
            if (m != Match.Empty)
                retVal = m.Groups[1].ToString().Trim();
            return retVal;
        }

        private static string sSimFormulas = null;
        private static string SimFormulas
        {
            get
            {
                if (sSimFormulas == null)
                {
                    string fileName = "Formulas\\SIM_Formulas.txt";
                    if (File.Exists(fileName))
                        sSimFormulas = File.ReadAllText(fileName);
                    else
                        sSimFormulas = StaticUtils.GetEmbeddedTextFile("TSBTool2.Formulas.SIM_Formulas.txt");
                }
                return sSimFormulas;
            }
        }

        private static Dictionary<string, string> sSubLines = null;
        private static Dictionary<string, string> sSubLinesTSB3 = new Dictionary<string, string>(){
            { "QB","QB,name       ,face    , JN,RS,RP,MS,HP,BB,AG,PS,PC,PA,AR,CO"},
            { "SKILL","SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC"},
            { "OL", "OL,name      ,face    , JN,RS,RP,MS,HP,BB,AG"},
            { "DEF","DEF,name     ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU"},
            { "K","K,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,KA,AB"},
            { "P","P,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,AB"}
        };

        private static Dictionary<string, string> sSubLinesTSB2 = new Dictionary<string, string>(){
            { "QB","QB,name       ,face    , JN,RS,RP,MS,HP,BB,PS,PC,PA,AR,CO"},
            { "SKILL","SKILL,name ,face    , JN,RS,RP,MS,HP,BB,BC,RC"},
            { "OL", "OL,name      ,face    , JN,RS,RP,MS,HP,BB"},
            { "DEF","DEF,name     ,face    , JN,RS,RP,MS,HP,BB,PI,QU"},
            { "K","K,name         ,face    , JN,RS,RP,MS,HP,BB,KP,KA,AB"},
            { "P","P,name         ,face    , JN,RS,RP,MS,HP,BB,KP,AB"}
        };

    }

    public class DefensiveSimUpdater
    {
        private string mData = "";

        public string Data
        {
            get { return mData; }
            set { mData = value; }
        }

        /// <summary>
        /// Gets a player 'line' from m_Data from 'team' playing 'position'.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private int[] GetPlayerInts(int season, string team, string position)
        {
            string pattern = "TEAM\\s*=\\s*" + team;
            Regex findTeamRegex = new Regex(pattern);
            Match m = findTeamRegex.Match(mData);
            if (m != Match.Empty)
            {
                int teamIndex = m.Index;
                if (teamIndex == -1)
                    return null;
                int playerIndex = -1;
                Regex endLineRegex = new Regex(string.Format("\n\\s*{0}\\s*,", position));
                Match eol = endLineRegex.Match(mData, teamIndex);
                if (eol != Match.Empty)
                    playerIndex = eol.Index;
                playerIndex++;

                if (playerIndex == 0)
                    return null;
                int lineEnd = mData.IndexOf("\n", playerIndex);
                string playerLine = mData.Substring(playerIndex, lineEnd - playerIndex);
                return InputParser.GetInts(playerLine, false);
            }
            return null;
        }

        private int GetPiIndex(string data)
		{
			int retVal = 4; //TSB1
			switch(StaticUtils.GetContentType(data))
			{
				case TSBContentType.TSB2: retVal = 5; break;
				case TSBContentType.TSB3: retVal = 6; break;
			}
			return retVal;
		}

        private int GetSeasonIndex(int season)
        {
            int retVal = 0;
            if (StaticUtils.IsTSB2Content(mData))
            {
                string pattern = String.Format("^\\s*SEASON\\s+{0}", season);
                Regex seasonRegex = new Regex(pattern);
                Match m = seasonRegex.Match(mData);
                if (m.Success)
                    retVal = m.Index;
            }
            return retVal;
        }

        public void UpdateTeamsSimDefense(int season, List<string> teams)
        {
            foreach (string team in teams)
                UpdateTeamSimDefense(season, team);
        }

        /// <summary>
        /// Updates the given team's Sim Defense for the given season
        /// </summary>
        public string UpdateTeamSimDefense(int season, string team)
        {
            string sim_def = "";
            int[] re = GetPlayerInts(season, team, "RE");
            int[] le = GetPlayerInts(season, team, "LE");
            int[] nt = GetPlayerInts(season, team, "NT");
            int[] lolb = GetPlayerInts(season, team, "LOLB");
            int[] lilb = GetPlayerInts(season, team, "LILB");
            int[] rilb = GetPlayerInts(season, team, "RILB");
            int[] rolb = GetPlayerInts(season, team, "ROLB");
            int[] rcb = GetPlayerInts(season, team, "RCB");
            int[] lcb = GetPlayerInts(season, team, "LCB");
            int[] fs = GetPlayerInts(season, team, "FS");
            int[] ss = GetPlayerInts(season, team, "SS");

            sim_def = CalculateTeamDefense(re, le, nt, lolb, lilb, rilb, rolb, rcb, lcb, fs, ss);

            string pattern = String.Format("TEAM\\s*=\\s*{0}\\s*,\\s*SimData\\s*=\\s*0x([0-9a-fA-F]{{2}})", team);
            Regex teamSimRegex = new Regex(pattern);
            int seasonIndex = GetSeasonIndex(season);
            Match m = teamSimRegex.Match(mData, seasonIndex);
            string old = m.Groups[1].ToString();
            if (m != Match.Empty)
            {
                string start = mData.Substring(0, m.Groups[1].Index);
                string last = mData.Substring(m.Groups[1].Index + 2);
                StringBuilder tmp = new StringBuilder(mData.Length + 20);
                tmp.Append(start);
                tmp.Append(sim_def);
                tmp.Append(last);
                mData = tmp.ToString();
            }
            System.Diagnostics.Debugger.Log(0, "sim", string.Format("Team:{0} old:{1} new:{2}\n",team, old, sim_def.ToUpper()));
            return sim_def;
        }

        //https://www.codeproject.com/Tips/715891/Compiling-Csharp-Code-at-Runtime
        private static DateTime sFormulasTimeStamp = DateTime.MinValue;
        private static MethodInfo sTeamSimDataFunction = null;

        /// <summary>
        /// Will use the 'Formulas/SimFormulas.cs' file to calculate the sim defense.
        /// If not there, will call the 'GetTeamSimDefense()' function.
        /// </summary>
        private string CalculateTeamDefense(int[] re, int[] nt, int[] le, int[] lolb, int[] lilb,
                int[] rilb, int[] rolb, int[] rcb, int[] lcb, int[] fs, int[] ss)
        {
            string functionName = "GetTeamSimDefense";
            string fileName = ".\\Formulas\\SimFormulas.cs";
            if (File.Exists(fileName))
            {
                FileInfo info = new FileInfo(fileName);
                if (sFormulasTimeStamp < info.LastWriteTime)
                {
                    string finalCode = File.ReadAllText(fileName);
                    CSharpCodeProvider provider = new CSharpCodeProvider();
                    CompilerResults results = provider.CompileAssemblyFromSource(new CompilerParameters(), finalCode);
                    if (results.Errors.HasErrors)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Errors in ");
                        sb.Append(Path.GetFullPath( fileName));
                        sb.Append("\nFix these errors:\n");
                        foreach (CompilerError error in results.Errors)
                            sb.AppendLine(String.Format("Line {0}: Error ({1}): {2}\n", error.Line, error.ErrorNumber, error.ErrorText));
                        throw new InvalidOperationException(sb.ToString());
                    }
                    Type binaryFunction = results.CompiledAssembly.GetType("UserFunctions.SimFunctions");
                    sTeamSimDataFunction = binaryFunction.GetMethod(functionName);
                    sFormulasTimeStamp = info.CreationTime;
                }
            }
            string sim_def = "";
            if (sTeamSimDataFunction != null)
            {
                System.Diagnostics.Debug.WriteLine("Using User Defined functions.");
                sim_def = sTeamSimDataFunction.Invoke(null, new object[] {
                    re, nt, le, lolb, lilb, rilb, rolb, rcb, lcb, fs, ss
                }).ToString();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("NOT Using User Defined functions.");
                sim_def = GetTeamSimDefense(re, nt, le, lolb, lilb, rilb, rolb, rcb, lcb, fs, ss);
            }
            return sim_def;
        }

        private string GetTeamSimDefense(int[] re, int[] nt, int[] le, int[] lolb, int[] lilb, 
                int[] rilb, int[] rolb, int[] rcb, int[] lcb, int[] fs, int[] ss)
        {
            string sim_def = "";
            int rs = 0; int rp = 1; int ms = 2; int hp = 3;
            int pi = GetPiIndex(mData);
            int qu = pi + 1;
            int team_hp = 0; int team_pi = 0; int team_ms = 0;

            if (re[ms] > 49) team_ms++; if (re[hp] > 49) team_hp++; if (re[pi] > 49) team_pi++;
            if (le[ms] > 49) team_ms++; if (le[hp] > 49) team_hp++; if (le[pi] > 49) team_pi++;
            if (nt[ms] > 49) team_ms++; if (nt[hp] > 49) team_hp++; if (nt[pi] > 49) team_pi++;
            if (lolb[ms] > 49) team_ms++; if (lolb[hp] > 49) team_hp++; if (lolb[pi] > 49) team_pi++;
            if (lilb[ms] > 49) team_ms++; if (lilb[hp] > 49) team_hp++; if (lilb[pi] > 49) team_pi++;
            if (rilb[ms] > 49) team_ms++; if (rilb[hp] > 49) team_hp++; if (rilb[pi] > 49) team_pi++;
            if (rolb[ms] > 49) team_ms++; if (rolb[hp] > 49) team_hp++; if (rolb[pi] > 49) team_pi++;
            if (rcb[ms] > 49) team_ms++; if (rcb[hp] > 49) team_hp++; if (rcb[pi] > 49) team_pi++;
            if (lcb[ms] > 49) team_ms++; if (lcb[hp] > 49) team_hp++; if (lcb[pi] > 49) team_pi++;
            if (fs[ms] > 49) team_ms++; if (fs[hp] > 49) team_hp++; if (fs[pi] > 49) team_pi++;
            if (ss[ms] > 49) team_ms++; if (ss[hp] > 49) team_hp++; if (ss[pi] > 49) team_pi++;

            int rush_def = team_ms + team_hp;
            int pass_def = team_ms + team_pi;
            if (rush_def > 0xf) rush_def = 0xf;
            if (pass_def > 0xf) pass_def = 0xf;
            sim_def = string.Format("{0:x}{1:x}", rush_def, pass_def);
            return sim_def;
        }
    }
}
