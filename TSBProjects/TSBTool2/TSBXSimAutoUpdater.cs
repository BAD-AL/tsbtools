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
            switch (gameVersion)
            {
                case TSBContentType.TSB2: sSubLines = sSubLinesTSB2; break;
                case TSBContentType.TSB3: sSubLines = sSubLinesTSB3; break;
                default: throw new ArgumentException("TSBXSimAutoUpdater: Incorrect version " + gameVersion.ToString());
            }
            
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

        private static Dictionary<string, string> sSubLinesTSB1 = new Dictionary<string, string>(){
            { "QB","QB,name       ,face    , JN,RS,RP,MS,HP,PS,PC,PA,AR"},
            { "SKILL","SKILL,name ,face    , JN,RS,RP,MS,HP,BC,RC"},
            { "OL", "OL,name      ,face    , JN,RS,RP,MS,HP"},
            { "DEF","DEF,name     ,face    , JN,RS,RP,MS,HP,PI,QU"},
            { "K","K,name         ,face    , JN,RS,RP,MS,HP,KP,KA,AB"},
            { "P","P,name         ,face    , JN,RS,RP,MS,HP,KP,AB"}
        };
    }
}
