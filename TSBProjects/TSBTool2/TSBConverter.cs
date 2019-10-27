using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace TSBTool2
{
    /// 0     1     2     3       4  5   6   7    8  9   10  11    12          13        14
    /// pos, name, face, number, RS, RP, MS, HP, PS, PC, PA, APB, [Sim rush, Sim pass, Sim Pocket].
    /// QB1,qb BILLS, Face=0x52, #0, 25, 69, 13, 13, 56, 81, 81, 81 ,[3, 12, 3 ] *tsb1PlayerLine
    ///                          RS, RP, MS, HP, BC, REC, [Sim rush, Sim catch, Sim punt Ret, Sim kick ret]. -> Skill
    ///                          RS, RP, MS, HP, PI, QU, [Sim pass rush, Sim coverage].                     --> Defense
    ///                          RS, RP, MS, HP, KA, AKB,[ Sim kicking ability].                            --> Punter/Kicker
    /// pos, name, face, number, RS  RP  MS  HP  BB  PS  PC  PA  AR CO [sim vals]
    /// # Skill                  RS  RP  MS  HP  BB  BC  RC [sim vals]
    /// # OL                     RS  RP  MS  HP  BB
    /// # DEF                    RS  RP  MS  HP  BB  PI  QU [sim vals]
    /// # K                      RS  RP  MS  HP  BB  KP  KA  AB [sim val]
    /// # P                      RS  RP  MS  HP  BB  KP AB [sim val]
    /// QB1,jim KELLY,Face=0x00,#12,69,25,13,13,13,63,81,69,69,75,[38,E0,9E]
    public class TSB2Converter
    {
        // this will remove the 'Agility' attribute
        public static String ConvertToTSB2FromTSB3(string input)
        {
            input = input.Replace("\r\n", "\n");
            string[] lines = input.Split("\n".ToCharArray());
            StringBuilder builder = new StringBuilder(input.Length + lines.Length * 2);
            string tmp = "";
            string line = "";
            foreach (string theLine in lines)
            {
                line = theLine;
                if (ShouldConvertTSB3Player(line))
                {
                    tmp = ConvertToTSB2PlayerFromTSB3(line);
                    builder.Append(tmp);
                }
                else
                {
                    builder.Append(line);
                }
                builder.Append("\n");
            }
            StaticUtils.ShowErrors();
            string retVal = builder.ToString();
            return retVal;
        }

        public static String ConvertToTSB2FromTSB1(string input)
        {
            input = input.Replace("\r\n", "\n");
            input = input.Replace("titans", "oilers");
            string[] lines = input.Split("\n".ToCharArray());
            StringBuilder builder = new StringBuilder(input.Length + lines.Length * 2);
            string tmp = "";
            string line="";

            foreach (string theLine in lines)
            {
                line = theLine;
                if (line.StartsWith("#RE2") || line.StartsWith("#NT2") || line.StartsWith("#LE2") || line.StartsWith("#LB5") || line.StartsWith("#DB3"))
                {
                    builder.Append(line.Substring(1));
                }
                else if (line.StartsWith("#PLAYBOOK") && InputParser.playbookRegex.Match(line) != Match.Empty)
                {
                    builder.Append(line.Substring(1));
                }
                else if (line.Trim().StartsWith("#"))
                {
                    builder.Append(line);
                }
                else if (line.StartsWith("COLORS"))
                {
                    builder.Append("#" + line);
                }
                else if (ShouldConvertTSB1Player(line))
                {
                    tmp = ConvertToTSB2PlayerFromTSB1(line);
                    builder.Append(tmp);
                }
                else if (line.StartsWith("PLAYBOOK"))
                {
                    tmp = ConvertPlaybook(line);
                    builder.Append(tmp);
                }
                else
                {
                    builder.Append(line);
                }
                builder.Append("\n");
            }
            StaticUtils.ShowErrors();
            string retVal = builder.ToString();
            retVal = TSBXSimAutoUpdater.AutoUpdatePlayerSimData(retVal,TSBContentType.TSB2 );
            return retVal;
        }

        private static bool ShouldConvertTSB1Player(string line)
        {
            int index = line.IndexOf(',');
            string pos = "";
            if (index > 0)
                pos = line.Substring(0, index);
            return TSB2Tool.positionNames.IndexOf(pos) > -1;
        }

        private static bool ShouldConvertTSB3Player(string line)
        {
            int index = line.IndexOf(',');
            string pos = "";
            if (index > 0)
                pos = line.Substring(0, index);
            return TSB2Tool.positionNames.IndexOf(pos) > -1;
        }

        private static string ConvertPlaybook(string line)
        {
            Match m = InputParser.playbookRegex.Match(line);
            string runs = m.Groups[1].ToString();
            string passes = m.Groups[2].ToString();

            runs = runs.Substring(1);
            passes = passes.Substring(1);
            string retVal = String.Format("PLAYBOOK R{0}{0}, P{1}{1}", runs, passes);
            
            return retVal;
        }

        // basically just removes the Agility attribute
        public static string ConvertToTSB2PlayerFromTSB3(string tsb1PlayerLine)
        {
            int simIndex = tsb1PlayerLine.IndexOf("[");
            string simString = "";
            string[] parts = null;
            if (simIndex > -1)
            {
                simString = tsb1PlayerLine.Substring(simIndex);
                parts = tsb1PlayerLine.Substring(0, simIndex - 1).Split(",".ToCharArray());
            }
            else
                parts = tsb1PlayerLine.Split(",".ToCharArray());

            StringBuilder sb = new StringBuilder(60);
            string pos = parts[0].Trim();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i != 5) // exclude AG
                {
                    sb.Append(parts[i].Trim());
                    sb.Append(",");
                }
            }
            sb.Append(simString);
            return sb.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="tsb1PlayerLine"></param>
        /// <returns></returns>
        public static string ConvertToTSB2PlayerFromTSB1(string tsb1PlayerLine)
        {
            int simIndex = tsb1PlayerLine.IndexOf("[");
            string simString = "";
            string[] parts =null;
            if (simIndex > -1)
            {
                simString = tsb1PlayerLine.Substring(simIndex - 1);
                parts = tsb1PlayerLine.Substring(0, simIndex - 1).Split(",".ToCharArray());
            }
            else
                parts = tsb1PlayerLine.Split(",".ToCharArray());
            StringBuilder sb = new StringBuilder(60);
            string pos = parts[0].Trim();
            string rs = parts[4]; // swap RS & RP
            string rp = parts[5];
            parts[5] = rs;
            parts[4] = rp;
            for (int i = 0; i < parts.Length; i++)
            {
                switch(i)
                {
                    case 2://face 
                        sb.Append(ConvertFaceToTSB2(parts[i].Trim()));
                        break;
                    case 7:
                        sb.Append(parts[i].Trim() +","); // add HP
                        sb.Append(GetBB(pos, parts[6].Trim())); // BB
                        break;
                    case 8:
                        switch (pos)
                        {
                            case "K": 
                                sb.Append(parts[i].Trim() + ","); // add KP
                                sb.Append(parts[i].Trim());       // add KA
                                break;
                            default:
                                sb.Append(parts[i].Trim());
                                break;
                        }
                        break;
                    case 11: // only QB has this, gotta add coolness too
                        sb.Append(parts[i].Trim() + ",");
                        sb.Append(GetCoolness(parts[1], parts[10].Trim()) ); // add coolness
                        break;
                    default:
                        sb.Append(parts[i].Trim());
                        break;
                }
                sb.Append(",");
            }
            AddSimValues(pos, simString, sb);
            return sb.ToString();
        }

        private static void AddSimValues(string pos, string simString, StringBuilder sb)
        {
            int[] vals = InputParser.GetSimVals(simString, false);
            string simVals = "";
            switch (pos)
            {
                case "QB1": case "QB2":
                    simVals = "[51,00,06]"; //???
                    break;
                case "RB1": simVals = "[1A,00,06,05]"; break;
                case "RB2":
                case "RB3":
                case "RB4": simVals = "[00,03,00,00]"; break;
                case "WR1":
                case "WR2": simVals = "[00,07,0A,00]"; break;
                case "WR3":
                case "WR4": simVals = "[0A,00,01,05]"; break;
                case "TE1":
                case "TE2": simVals = "[03,00,00,00]"; break;

                //18 defenders - 3 bytes each (sacks/int/tackles) 
                //TSB1 ==> [Sim pass rush, Sim coverage].
                case "RE":
                case "NT":
                case "LE":
                case "RE2":
                case "NT2":
                case "LE2":
                case "ROLB":
                case "RILB":
                case "LILB":
                case "LOLB":
                case "LB5":
                case "RCB":
                case "LCB":
                case "DB1":
                case "DB2":
                case "FS":
                case "SS":
                case "DB3":
                    if (simVals.Length > 1)
                        simVals = string.Format("[{0:X2},{1:X2},{2:X2}]", vals[0] * 2, vals[1] * 3, vals[0] * 2);
                    else
                        simVals = "[10,10,10]";
                    break;
                case "K":
                case "P":
                    simVals = "[" + vals[0] + "]";
                    break;
            }
            sb.Append(simVals);
        }


        private static string GetCoolness(string name, string pa)
        {
            int space = name.IndexOf(' ');
            if ( space > -1)
            {
                string lastName = name.Substring(space).ToUpper();
                string coolGuys = 
                    " KELLY KOSAR MOON YOUNG ELWAY FAVRE MARINO MONTANA MANNING BRADY BREES MAHOMES RODGERS ROETHLISBERGER BRADSHAW WILSON STAUBACH FOLES UNITAS TARKENTON TESTAVERDE ";
                if (coolGuys.IndexOf(lastName) > -1)
                    return "81";
            }
            return pa;
        }

        private static string GetBB(string pos, string ms)
        {
            string retVal = "13";
            int ms_i = Int32.Parse(ms);
            switch (pos)
            {
                case "RCB": case "LCB": case "DB1": case "DB2":
                case "RB1": case "RB2": case "RB3": case "RB4":
                    retVal = ms;
                    break;
                case "LOLB": case "LILB": case "RILB": case "ROLB": case "LB5":
                    retVal = "19";
                    break;
                case "SS": case "FS":
                    retVal = "25";
                    break;
                case "QB1": case "QB2":
                case "WR1": case "WR2": case "WR3": case "WR4":
                    if (ms_i > 44) retVal = "44";
                    else retVal = "25";
                    break;
            }
            return retVal;
        }

        private static string ConvertFaceToTSB2(string input)
        {
            string tmp = input.Replace("Face=0x","");
            int number = Int32.Parse(tmp, System.Globalization.NumberStyles.AllowHexSpecifier);
            if (number < 0x80)
                number = number & 0x0F;
            else
                number = 0x80 + (number & 0x0F);
            string retVal = String.Format("Face=0x{0:x2}", number);
            return retVal;
        }

        #region Tests

        public static string TestQbTSB2Conversion()
        {
            string retVal = "";
            string joe =       "QB1,joe MONTANA,Face=0x1, #16, 25, 69, 19, 13, 56, 81, 81, 75 ,[3, 12, 2 ]";
            string resultJoe = "QB1,joe MONTANA,Face=0x01,#16,69,25,19,13,25,56,81,81,75,81,[51,00,06]";

            string test = ConvertToTSB2PlayerFromTSB1(joe);
            retVal += StaticUtils.AreEqual(resultJoe, test);

            string vinny = "QB1, vinny TESTAVERDE, Face=0x23, #14, 25, 69, 31, 13, 31, 56, 44, 44 ,[5, 4, 0 ]";
            string resultVinny = "QB1,vinny TESTAVERDE,Face=0x03,#14,69,25,31,13,25,31,56,44,44,44,[51,00,06]";
            test = ConvertToTSB2PlayerFromTSB1(vinny);
            retVal += StaticUtils.AreEqual(resultVinny, test);
            return retVal;
        }

        public static string TestRbTSB2Conversion()
        {
            string retVal = "";
            string thruman = "RB1, thurman THOMAS, Face=0x83, #34, 38,  69,63,25,75,50 ,[10, 7, 8, 8 ]";
            string resultThurman = "RB1,thurman THOMAS,Face=0x83,#34,69,38,63,25,63,75,50,[1A,00,06,05]";

            string test = ConvertToTSB2PlayerFromTSB1(thruman);
            retVal += StaticUtils.AreEqual(resultThurman, test);

            string roger =      "RB1, roger CRAIG,Face=0xd0,#33,38,69,50,25,50,44,[6, 7, 7, 2 ]";
            string resultRoger = "RB1,roger CRAIG,Face=0x80,#33,69,38,50,25,50,50,44,[1A,00,06,05]";
            test = ConvertToTSB2PlayerFromTSB1(roger);
            retVal += StaticUtils.AreEqual(resultRoger, test);
            return retVal;
        }

        public static string TestDbTSB2Conversion()
        {
            string retVal = "";
            string deion =    "RCB, deion SANDERS, Face=0x9e, #21,44,56,75,56,56,50,[1, 59 ]"; //  vals[0]*2, vals[1]*3, vals[0]*2);
            string resultDeion = "RCB,deion SANDERS,Face=0x8e,#21,56,44,75,56,75,56,50,[02,B1,02]";

            string test = ConvertToTSB2PlayerFromTSB1(deion);
            retVal += StaticUtils.AreEqual(resultDeion, test);
            return retVal;
        }

        public static string TestLbTSB2Conversion()
        {
            string retVal = "";
            string ray = "RILB, ray BENTLEY, Face=0x30, #50, 25, 31, 38, 38, 31, 56 ,[13, 10 ]"; //  vals[0]*2, vals[1]*3, vals[0]*2);
            string resultRay = "RILB,ray BENTLEY,Face=0x00,#50,31,25,38,38,19,31,56,[1A,1E,1A]";

            string test = ConvertToTSB2PlayerFromTSB1(ray);
            retVal += StaticUtils.AreEqual(resultRay, test);
            return retVal;
        }

        public static string TestDlTSB2Conversion()
        {
            string retVal = "";
            string target = "LE, leon SEALS, Face=0xac, #96, 25, 31, 38, 44, 31, 50 ,[25, 18 ]"; //  vals[0]*2, vals[1]*3, vals[0]*2);
            string resultTarget = "LE,leon SEALS,Face=0x8c,#96,31,25,38,44,13,31,50,[32,36,32]";

            string test = ConvertToTSB2PlayerFromTSB1(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }

        public static string TestOlTSB2Conversion()
        {
            string retVal = "";
            string target = "LG, jim RITCHER, Face=0x7, #51, 25, 69, 38, 56"; //  vals[0]*2, vals[1]*3, vals[0]*2);
            string resultTarget = "LG,jim RITCHER,Face=0x07,#51,69,25,38,56,13,";

            string test = ConvertToTSB2PlayerFromTSB1(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }

        public static string TestKickerTSB2Conversion()
        {
            string retVal = "";
            string target = "K, scott NORWOOD, Face=0x29, #11, 56, 81, 81, 31, 44, 44 ,[6 ]"; //  vals[0]*2, vals[1]*3, vals[0]*2);
            string resultTarget = "K,scott NORWOOD,Face=0x09,#11,81,56,81,31,13,44,44,44,[6]";

            string test = ConvertToTSB2PlayerFromTSB1(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }

        public static string TestPunterTSB2Conversion()
        {
            string retVal = "";
            string target =       "P,chris MOHR,Face=0x09,#9,81,25,44,31,13,44,69,[8]"; //  vals[0]*2, vals[1]*3, vals[0]*2);
            string resultTarget = "P,chris MOHR,Face=0x09,#9,25,81,44,31,13,13,44,69,[8]";

            string test = ConvertToTSB2PlayerFromTSB1(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }
        #endregion
    }

    public class TSB1Converter
    {
        public static String ConvertToTSB1FromTSB2(string input)
        {
            input = input.Replace("\r\n", "\n");
            string[] lines = input.Split("\n".ToCharArray());
            StringBuilder builder = new StringBuilder(input.Length + lines.Length * 2);
            string tmp = "";
            foreach (string line in lines)
            {
                if (IsPlayerLine(line))
                {
                    tmp = ConvertToTSB1Player(line);
                    builder.Append(tmp);
                }
                else if (line.StartsWith("SEASON"))
                {
                    builder.Append("#" + line);
                }
                else if (line.StartsWith("PLAYBOOK"))
                {
                    tmp = ConvertPlaybook(line);
                    builder.Append(tmp);
                }
                else if (line.StartsWith("#COLORS"))
                {
                    builder.Append(line.Substring(1));
                }
                else if (line.StartsWith("TEAM_ABB"))
                {
                    builder.Append(line.ToUpper());
                }
                else
                {
                    builder.Append(line);
                }
                builder.Append("\n");
            }
            string retVal = TSB1SimAutoUpdater.AutoUpdatePlayerSimData( builder.ToString());
            return retVal;
        }

        private static string ConvertPlaybook(string line)
        {
            return "#"+ line;
        }

        private static bool IsPlayerLine(string line)
        {
            int index = line.IndexOf(',');
            string pos ="";
            if (index > 1)
                pos = line.Substring(0, index);
            return TSB2Tool.positionNames.IndexOf(pos) > -1;
        }

        public static string ConvertToTSB1Player(string tsb2PlayerLine)
        {
            int simIndex = tsb2PlayerLine.IndexOf("[");
            string simString = "";
            string[] parts = null;
            if (simIndex > -1)
            {
                simString = tsb2PlayerLine.Substring(simIndex );
                parts = tsb2PlayerLine.Substring(0, simIndex - 1).Split(",".ToCharArray());
            }
            else
                parts = tsb2PlayerLine.Split(",".ToCharArray());
            
            string rs = parts[4]; // swap RS & RP
            string rp = parts[5];
            parts[5] = rs;
            parts[4] = rp;
            
            List<string> attrs = new List<string>(parts);
            
            StringBuilder sb = new StringBuilder(60);
            string pos = parts[0].Trim();
            switch (pos)
            {
                case "QB1": case "QB2":
                    attrs.RemoveAt(13); // remove coolness
                    attrs.RemoveAt(8);  // remove body balance
                    break;
                case "K":
                    attrs.RemoveAt(9); // remove KP
                    attrs.RemoveAt(8); // remove body balance
                    break;
                case "LB5": case "DB3": case "RE2": case "NT2": case "LE2":
                    return "#" + tsb2PlayerLine;
                default:
                    attrs.RemoveAt(8); // remove body balance
                    break;
            }
            for (int i = 0; i < attrs.Count; i++)
            {
                sb.Append(attrs[i]);
                sb.Append(",");
            }
            if ("RE,NT,LE,ROLB,RILB,LILB,LOLB,RCB,LCB,FS,SS".IndexOf(pos) > -1)
                sb.Append("[2,2]"); // these will get replaced when we fo teh SIM update
            string retVal = sb.ToString();

            return retVal.Replace(",,", ",");
        }

        #region Tests

        public static string TestQbTSB1Conversion()
        {
            string retVal = "";
            string joe =      "QB1,joe MONTANA,Face=0x01,#16,25,69,19,13,25,56,81,81,75,81,[51,00,06]";
            string resultJoe = "QB1,joe MONTANA,Face=0x01,#16,69,25,19,13,56,81,81,75,";

            string test = ConvertToTSB1Player(joe);
            retVal += StaticUtils.AreEqual(resultJoe, test);

            string vinny =       "QB1,vinny TESTAVERDE,Face=0x03,#14,25,69,31,13,25,31,56,44,44,44,[51,00,06]";
            string resultVinny = "QB1,vinny TESTAVERDE,Face=0x03,#14,69,25,31,13,31,56,44,44,";
            test = ConvertToTSB1Player(vinny);
            retVal += StaticUtils.AreEqual(resultVinny, test);
            return retVal;
        }

        public static string TestRbTSB1Conversion()
        {
            string retVal = "";
            string thruman = "RB1,thurman THOMAS,Face=0x83,#34,38,69,63,25,63,75,50,[1A,00,06,05]";
            string resultThurman = "RB1,thurman THOMAS,Face=0x83,#34,69,38,63,25,75,50,";

            string test = ConvertToTSB1Player(thruman);
            retVal += StaticUtils.AreEqual(resultThurman, test);

            string roger =       "RB1,roger CRAIG,Face=0x80,#33,38,69,50,25,50,50,44,[1A,00,06,05]";
            string resultRoger = "RB1,roger CRAIG,Face=0x80,#33,69,38,50,25,50,44,";
            test = ConvertToTSB1Player(roger);
            retVal += StaticUtils.AreEqual(resultRoger, test);
            return retVal;
        }

        public static string TestDbTSB1Conversion()
        {
            string retVal = "";
            string deion = "RCB,deion SANDERS,Face=0x8e,#21,44,56,75,56,75,56,50,[02,B1,02]";
            string resultDeion = "RCB,deion SANDERS,Face=0x8e,#21,56,44,75,56,56,50,[2,2]"; //  vals[0]*2, vals[1]*3, vals[0]*2);

            string test = ConvertToTSB1Player(deion);
            retVal += StaticUtils.AreEqual(resultDeion, test);
            return retVal;
        }

        public static string TestLbTSB1Conversion()
        {
            string retVal = "";
            string ray =       "RILB,ray BENTLEY,Face=0x00,#50,25,31,38,38,19,31,56,[1A,1E,1A]";
            string resultRay = "RILB,ray BENTLEY,Face=0x00,#50,31,25,38,38,31,56,[2,2]"; //  vals[0]*2, vals[1]*3, vals[0]*2);

            string test = ConvertToTSB1Player(ray);
            retVal += StaticUtils.AreEqual(resultRay, test);
            return retVal;
        }

        public static string TestDlTSB1Conversion()
        {
            string retVal = "";
            string target =       "LE,leon SEALS,Face=0x8c,#96,25,31,38,44,13,31,50,[32,36,32]";
            string resultTarget = "LE,leon SEALS,Face=0x8c,#96,31,25,38,44,31,50,[2,2]"; // by default all defenders get [2,2] and we run the sim update code to fix it

            string test = ConvertToTSB1Player(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }

        public static string TestOlTSB1Conversion()
        {
            string retVal = "";
            string target =       "LG,jim RITCHER,Face=0x07,#51,25,69,38,56,13,";
            string resultTarget = "LG,jim RITCHER,Face=0x07,#51,69,25,38,56,";

            string test = ConvertToTSB1Player(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }

        public static string TestKickerTSB1Conversion()
        {
            string retVal = "";
            string target =       "K,scott NORWOOD,Face=0x09,#11,56,81,81,31,13,44,44,44,[6]";
            string resultTarget = "K,scott NORWOOD,Face=0x09,#11,81,56,81,31,44,44,";

            string test = ConvertToTSB1Player(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }

        public static string TestPunterTSB1Conversion()
        {
            string retVal = "";
            string target =       "P,chris MOHR,Face=0x09,#9,81,25,44,31,13,13,44,69,[8]";
            string resultTarget = "P,chris MOHR,Face=0x09,#9,25,81,44,31,13,44,69,";

            string test = ConvertToTSB1Player(target);
            retVal += StaticUtils.AreEqual(resultTarget, test);
            return retVal;
        }
        #endregion

        public static string CONVERT_MSG =
@"Converting between TSB1, TSB2 and TSB3 data is not exact and one does not 'un-do' the other.
When converting from TSB1 --> TSB2 a 'Auto-update' sim data operation is performed (feature taken from TSBToolSupreme).

";


    }

    public class TSB3Converter
    {
        public static void ReloadFormulas()
        {
            sFormulaString = null;// force re-load of the formula file when calling 'FormulaString'
        }

        // basically adds the 'agility' attribute
        public static String ConvertToTSB3FromTSB2(string input)
        {
            ReloadFormulas();
            input = input.Replace("\r\n", "\n");
            string[] lines = input.Split("\n".ToCharArray());
            StringBuilder builder = new StringBuilder(input.Length + lines.Length * 2);
            string tmp = "";
            string line = "";
            foreach (string theLine in lines)
            {
                line = theLine;
                if (ShouldConvertTSB3Player(line))
                {
                    tmp = ConvertToTSB3PlayerFromTSB2(line);
                    builder.Append(tmp);
                }
                else
                {
                    builder.Append(line);
                }
                builder.Append("\n");
            }
            StaticUtils.ShowErrors();
            string retVal = builder.ToString();
            return retVal;
        }


        private static bool ShouldConvertTSB3Player(string line)
        {
            int index = line.IndexOf(',');
            string pos = "";
            if (index > 0)
                pos = line.Substring(0, index);
            return TSB2Tool.positionNames.IndexOf(pos) > -1;
        }

        private static string ConvertToTSB3PlayerFromTSB2(string input)
        {
            string retVal = "";
            int simIndex = input.IndexOf("[");
            string simString = "";
            List<string> parts = null;
            if (simIndex > -1)
            {
                simString = input.Substring(simIndex);
                parts = new List<string>(input.Substring(0, simIndex - 1).Split(",".ToCharArray()));
            }
            else
                parts = new List<string>(input.Split(",".ToCharArray()));
            StringBuilder sb = new StringBuilder(60);
            string pos = parts[0].Trim();
            string hp = parts[7].Trim();
            string ms = parts[6].Trim();
            string ag = GetAG(pos, ms, hp);
            parts.Insert(9, ag);
            for (int i = 0; i < parts.Count; i++)
            {
                sb.Append(parts[i].Trim());
                sb.Append(",");
            }
            sb.Append(simString);
            retVal = UpdateFreeAgentValue(sb.ToString().Replace(",,", ","));
            return retVal;
        }
        
        public static string UpdateFreeAgentValue(string input)
        {
            int simIndex = input.IndexOf("[");
            string simString = "";
            List<string> parts = null;
            if (simIndex > -1)
            {
                simString = input.Substring(simIndex);
                parts = new List<string>(input.Substring(0, simIndex - 1).Split(",".ToCharArray()));
            }
            else
                parts = new List<string>(input.Split(",".ToCharArray()));
            
            string pos = parts[0].Trim();
            string substitutionString ="";
            string formula = GetFreeAgentPointFormula(pos, out substitutionString);
            string[] sub_parts = substitutionString.Split(",".ToCharArray());
            for (int i = 4; i < sub_parts.Length; i++) // start at 'RS'
            {
                formula = formula.Replace(sub_parts[i].Trim(), parts[i]);
            }
            double result = 0;
            try
            {
                string r = StaticUtils.Compute(formula).ToString();
                result = Double.Parse(r);
            }
            catch (Exception)
            {
                throw new ArgumentException(String.Format(
                    "Error calculating FreeAgentPoints for '{0}' Tried to compute:'{1}'", input, formula));
            }
            byte fap = (byte)Math.Round(result);
            if (fap < 0) fap = 0;
            else if (fap > 0x0F) fap = 0x0F;

            string face = parts[2].Trim().Substring(0, parts[2].Length - 1) + fap.ToString("X");
            return input.Replace(parts[2], face); 
        }

        private static string GetFreeAgentPointFormula(string position, out string substitutionString)
        {
            string retVal = null;
            string searchStr = "FAP_OL";
            substitutionString = "OL,name      ,face    , JN,RS,RP,MS,HP,BB";
            #region big switch 
            switch (position)
            {
                case "QB1": case "QB2":
                    searchStr = "FAP_QB";
                    substitutionString = "QB,name       ,face    , JN,RS,RP,MS,HP,BB,AG,PS,PC,PA,AR,CO";
                    break;
                case "RB1":  case "RB2": case "RB3": case "RB4": 
                    searchStr = "FAP_RB";
                    substitutionString = "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC";
                    break;
                case "WR1": case "WR2": case "WR3": case "WR4":
                    searchStr = "FAP_WR";
                    substitutionString = "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC";
                    break;
                case "TE1": case "TE2":
                    searchStr = "FAP_TE";
                    substitutionString = "SKILL,name ,face    , JN,RS,RP,MS,HP,BB,AG,BC,RC";
                    break; 
                case "RE": case "NT": case "LE":
                case "RE2": case "NT2": case "LE2":
                    searchStr = "FAP_DL";
                    substitutionString = "DL,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                    break;
                case "ROLB": case "RILB": case "LILB":
                case "LOLB": case "LB5":
                    searchStr = "FAP_LB";
                    substitutionString = "DEF,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                    break;
                case "RCB": case "LCB": case "DB1": case "DB2":
                    searchStr = "FAP_CB";
                    substitutionString = "DEF,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                    break;
                case "FS": case "SS": case "DB3":
                    searchStr = "FAP_S";
                    substitutionString = "DEF,name      ,face    , JN,RS,RP,MS,HP,BB,AG,PI,QU";
                    break;
                case "K":
                    searchStr = "FAP_K";
                    substitutionString = "K,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,KA,AB";
                    break;
                case "P":
                    searchStr = "FAP_P";
                    substitutionString = "P,name         ,face    , JN,RS,RP,MS,HP,BB,AG,KP,AB";
                    break;
            }
            #endregion
            string pattern = String.Format("{0}\\s*:\\s*(.*)$", searchStr);
            Regex reg = new Regex(pattern, RegexOptions.Multiline);
            Match m = reg.Match(FormulaString);
            if (m != Match.Empty)
            {
                retVal = m.Groups[1].ToString();
            }
            return retVal;
        }

        private static string sFormulaString = null;
        protected static string FormulaString
        {
            get
            {
                if (sFormulaString == null)
                {
                    string fileName = "Formulas\\FAP_Formulas.txt";
                    if (File.Exists(fileName))
                        sFormulaString = File.ReadAllText(fileName);
                    else
                        sFormulaString = StaticUtils.GetEmbeddedTextFile("TSBTool2.Formulas.FAP_Formulas.txt");
                }
                return sFormulaString;
            }
        }

        private static string GetAG(string pos, string ms, string hp)
        {
            string retval = "31";
            int maxSpeed = Int32.Parse(ms);
            int msIndex = StaticUtils.GetTSBAbility(maxSpeed);
            int hpIndex = StaticUtils.GetTSBAbility(Int32.Parse(hp));
            
            switch (pos)
            {
                case "QB1": case "QB2":
                case "RB1": case "RB2": case "RB3": case "RB4":
                case "WR1": case "WR2": case "WR3": case "WR4":
                case "RE": case "NT": case "LE": case "RE2": case "LE2": case "NT2":
                case "SS": case "FS": case "DB3":
                    if( msIndex < 0x0E)
                        retval = StaticUtils.MapAbilityToTSBValue(msIndex + 1).ToString();
                    else
                        retval = StaticUtils.MapAbilityToTSBValue(msIndex).ToString();
                    break;
                case "TE1": case "TE2":
                    if (maxSpeed < 38) retval = "19";
                    break;
                case "C": case "RG": case "LG": case "RT": case "LT":
                    if (msIndex < 0x0E)
                        retval = StaticUtils.MapAbilityToTSBValue(hpIndex + 2).ToString();
                    else
                        retval = StaticUtils.MapAbilityToTSBValue(hpIndex).ToString();
                    break;
                case "ROLB": case "RILB": case "LILB": case "LOLB": case "LB5":
                    retval = ms;
                    break;
                case "LCB": case "RCB": case "DB1": case "DB2":
                    if (msIndex < 0x0E)
                        retval = StaticUtils.MapAbilityToTSBValue(msIndex + 2).ToString();
                    else
                        retval = StaticUtils.MapAbilityToTSBValue(msIndex).ToString();
                    break;
                case "K": case "P":
                    retval = "38";
                    break;
            }
            return retval;
        }
    }
}
