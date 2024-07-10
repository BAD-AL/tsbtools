using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;


namespace TSBTool
{
	/// <summary>
	/// Summary description for InputParser.
	/// </summary>
	public class InputParser
	{
		private ITecmoTool tool;
		private const int scheduleState = 0;
		private const int rosterState   = 1;
		private int currentState = 2;
		public bool showSimError= false;

        private static Regex numberRegex = new Regex("(#[0-9]{1,2})");
        private static Regex teamRegex = new Regex("TEAM\\s*=\\s*([0-9a-z]+)");
        private static Regex simDataRegex = new Regex("SimData=0[xX]([0-9a-fA-F][0-9a-fA-F])([0-3]?)");
        private static Regex weekRegex = new Regex("WEEK ([1-9][0	-7]?)");
        private static Regex gameRegex = new Regex("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
        private static Regex posNameFaceRegex = new Regex("([A-Z]+[1-4]?)\\s*,\\s*([a-zA-Z \\.\\-]+),\\s*(face=0[xX][0-9a-fA-F]+\\s*,\\s*)?");
        private static Regex yearRegex = new Regex("YEAR\\s*=\\s*([0-9]+)");
        private static Regex returnTeamRegex = new Regex("RETURN_TEAM\\s+([A-Z1-4]+)\\s*,\\s*([A-Z1-4]+)\\s*,\\s*([A-Z1-4]+)");
        private static Regex setRegex = new Regex("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
        private static Regex offensiveFormationRegex = new Regex("OFFENSIVE_FORMATION\\s*=\\s*([a-zA-Z1234_]+)");
        private static Regex playbookRegex = new Regex("PLAYBOOK (R[1-8]{4})\\s*,\\s*(P[1-8]{4})");
        private static Regex juiceRegex = new Regex("JUICE\\(\\s*([0-9]{1,2}|ALL)\\s*,\\s*([0-9]{1,2})\\s*\\)");
        private static Regex homeRegex = new Regex("Uniform1\\s*=\\s*0x([0-9a-fA-F]{6})");
        private static Regex awayRegex = new Regex("Uniform2\\s*=\\s*0x([0-9a-fA-F]{6})");
        private static Regex divChampRegex = new Regex("DivChamp\\s*=\\s*0x([0-9a-fA-F]{10})");
        private static Regex confChampRegex = new Regex("ConfChamp\\s*=\\s*0x([0-9a-fA-F]{8})");
        private static Regex uniformUsageRegex = new Regex("UniformUsage\\s*=\\s*0x([0-9a-fA-F]{8})");
        private static Regex replaceStringRegex = new Regex("ReplaceString\\(\\s*\"([A-Za-z0-9 .]+)\"\\s*,\\s*\"([A-Za-z .]+)\"\\s*(,\\s*([0-9]+))*\\s*\\)");
        private static Regex teamStringsRegex = new Regex("TEAM_ABB=([0-9A-Z. ]+),TEAM_CITY=([0-9A-Za-z .]+),TEAM_NAME=([0-9A-Z .]+)");


		private string currentTeam; //used for roster update
		private List<string> scheduleList;

		public InputParser(ITecmoTool tool)
		{
			this.tool = tool;
			currentTeam      = "bills";
		}

		public InputParser()
		{
			currentTeam      = "bills";
		}

		public void ProcessFile(string fileName)
		{
			try
			{
				StreamReader sr = new StreamReader(fileName);
				string contents= sr.ReadToEnd();
				sr.Close();
				char[] chars = "\n\r".ToCharArray();
				string[] lines = contents.Split(chars);
				ProcessLines(lines);
			}
			catch(Exception e){
				StaticUtils.ShowError(e.Message);
			}	
		}

		public void ProcessLines(string[] lines)
		{
			int i =0;
			try
			{
				for( i =0; i < lines.Length; i++)
				{
					ProcessLine(lines[i]);
					//Console.WriteLine(i);
				}
				StaticUtils.ShowErrors();
				ApplySchedule();
			}
			catch(Exception e)
			{
				StringBuilder sb = new StringBuilder(150);
				sb.Append( "Error! ");
				if( i < lines.Length )
					sb.Append(string.Format("line #{0}:\t'{1}'",i, lines[i]));
				sb.Append(e.Message);
				sb.Append("\n");
				sb.Append(e.StackTrace);
//						"Error Processing line {0}:\t'{1}'.\n{2}\n{3}",
//						i,lines[i], e.Message,e.StackTrace);
				sb.Append("\n\nOperation aborted at this point. Data not applied.");
				StaticUtils.ShowError(sb.ToString());
			}
		}

		protected virtual void ApplySchedule()
		{
			if( scheduleList != null )
			{
				tool.ApplySchedule(scheduleList);
				StaticUtils.ShowErrors(  );
				scheduleList = null;
			}
		}

		public void ReadFromStdin()
		{
			string line= "";
			int lineNumber = 0;
			Console.WriteLine("Reading from standard in...");
			try
			{
				while( (line=Console.ReadLine()) != null)
				{
					lineNumber++;
					ProcessLine(line);
					//Console.WriteLine("Line "+lineNumber);
				}
				StaticUtils.ShowErrors();
				ApplySchedule();
			}
			catch(Exception e)
			{
				StaticUtils.ShowError(string.Format(
                 "Error Processing line {0}:'{1}'.\n{2}\n{3}",
					lineNumber,line, e.Message,e.StackTrace));
			}
		}

        public static String CheckTextForRedundentSetCommands(String input)
        {
            StringBuilder ret = new StringBuilder();
            Regex simpleSetRegex = new Regex("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
            MatchCollection mc = simpleSetRegex.Matches(input);
            Match current = null;
            Match m = null;
            long location1 = 0;
            long location2 = 0;
            int valueLength1 = 0;
            int valueLength2 = 0;
            for (int i = 0; i < mc.Count; i++)
            {
                current = mc[i];
                location1 = StaticUtils.ParseLongFromHexString(current.Groups[1].ToString().Substring(2)); // long.Parse(current.Groups[1].ToString().Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
                valueLength1 = current.Groups[2].Length / 2 ;
                for (int j = i+1; j < mc.Count; j++)
                {
                    m = mc[j];
                    location2 = StaticUtils.ParseLongFromHexString(m.Groups[1].ToString().Substring(2)); // long.Parse(m.Groups[1].ToString().Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
                    valueLength2 = m.Groups[2].Length / 2 ;
                    if ((location2 >= location1 && location2 <= location1 + (valueLength1-2)) ||
                        (location1 >= location2 && location1 <= location2 + (valueLength2-2))    )
                    {
                        if (current.Groups[0].ToString() != m.Groups[0].ToString())
                        {
                            ret.Append("WARNING!\n 'SET' Commands modify same locations '");
                            ret.Append(current.Groups[0]);
                            ret.Append("' and '");
                            ret.Append(m.Groups[0]);
                            ret.Append("'\n");
                        }
                    }
                }
            }
            return ret.ToString();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		protected virtual void ProcessLine(string line)
		{
			line = line.Trim();
			Match m;

			if(line.StartsWith("#") || line == "" || line.ToLower().Trim().StartsWith("schedule") )
				return;
			else if( /*setRegex.Match(line) != Match.Empty )//*/
                line.StartsWith("SET") )
			{
				tool.ApplySet(line);
			}
            else if ((m = playbookRegex.Match(line)) != Match.Empty)
			{
				if( m != Match.Empty )
				{
					string runs = m.Groups[1].ToString();
					string passes = m.Groups[2].ToString();
					tool.SetPlaybook(currentTeam, runs, passes);
				}
				else
				{
					StaticUtils.AddError(string.Format("ERROR Setting playbook for team {0}. Line '{1}' is Invalid",
						currentTeam, line));
				}
			}
				// JUICE( ALL, 17)
				// JUICE(1,17)
			else if( (m = juiceRegex.Match(line)) != Match.Empty )
			{
				string juiceWeek  = m.Groups[1].ToString();
				int juiceAmt    = Int32.Parse(m.Groups[2].ToString());
				
				if( juiceWeek == "ALL" )
				{
					for(int i = 0; i < 17; i++)
					{
						tool.ApplyJuice(i+1, juiceAmt);
					}
				}
				else 
				{
					int week = Int32.Parse(juiceWeek)-1;
					if( !tool.ApplyJuice(week, juiceAmt))
					{
						StaticUtils.AddError(string.Format("ERROR! Line = '{0}'",line));
					}
				}
			}
            else if ((m = replaceStringRegex.Match(line)) != Match.Empty)
            {
                string find = "";
                string replace = "";
                int occur = -1;
                if (m.Groups.Count > 1)
                {
                    find = m.Groups[1].ToString();
                    replace = m.Groups[2].ToString();
                    if (m.Groups.Count > 3)
                    {
                        Int32.TryParse(m.Groups[4].ToString(), out occur);
                        occur--;
                    }
                    String msg = StaticUtils.ReplaceStringInRom(tool.OutputRom, find, replace, occur);
                    if (msg.StartsWith("Error"))
                        StaticUtils.AddError(msg);
                    else
                        StaticUtils.LogMessage(msg);
                }
                else
                {
                    StaticUtils.AddError(String.Format("ERROR! Not enough info to use 'ReplaceString' function.Line={0}",line));
                }
            }
            else if ((m = teamStringsRegex.Match(line)) != Match.Empty)
            {
                string teamAbb  = m.Groups[1].ToString();
                string teamCity = m.Groups[2].ToString();
                string teamName = m.Groups[3].ToString();
                int index = TecmoTool.GetTeamIndex(currentTeam);
                tool.SetTeamAbbreviation(index, teamAbb);
                tool.SetTeamCity(index, teamCity);
                tool.SetTeamName(index, teamName);
            }
            else if (line.StartsWith("COLORS")) // do the colors here
            {
                string tmp;

                Match home = homeRegex.Match(line);
                Match away = awayRegex.Match(line);
                Match confChamp = confChampRegex.Match(line);
                Match divChamp = divChampRegex.Match(line);
                Match uniUsage = uniformUsageRegex.Match(line);
                if (home != Match.Empty)
                {
                    tmp = home.Groups[1].Value;
                    tool.SetHomeUniform(currentTeam, tmp);
                }
                if (away != Match.Empty)
                {
                    tmp = away.Groups[1].Value;
                    tool.SetAwayUniform(currentTeam, tmp);
                }
                if (confChamp != Match.Empty)
                {
                    tmp = confChamp.Groups[1].Value;
                    tool.SetConfChampColors(currentTeam, tmp);
                }
                if (divChamp != Match.Empty)
                {
                    tmp = divChamp.Groups[1].Value;
                    tool.SetDivChampColors(currentTeam, tmp);
                }
                if (uniUsage != Match.Empty)
                {
                    tmp = uniUsage.Groups[1].Value;
                    tool.SetUniformUsage(currentTeam, tmp);
                }
            }
            else if (teamRegex.Match(line) != Match.Empty)//line.StartsWith("TEAM") )
            {
                //Console.WriteLine("'{0}' ", line);
                currentState = rosterState;
                string team = GetTeam(line);
                bool ret = SetCurrentTeam(team);
                if (!ret)
                {
                    StaticUtils.AddError(string.Format("ERROR with line '{0}'.", line));
                    StaticUtils.AddError(string.Format("Team input must be in the form 'TEAM = team SimData=0x1F'"));
                    return;
                }
                int[] simData = GetSimData(line);
                if (simData != null)
                {
                    if (simData[0] > -1)
                        tool.SetTeamSimData(currentTeam, (byte)simData[0]);
                    else
                        StaticUtils.AddError(string.Format("Warning: No sim data for team {0}", team));

                    if (simData[1] > -1)
                        tool.SetTeamSimOffensePref(currentTeam, simData[1]);
                }
                else
                    StaticUtils.AddError(string.Format("ERROR with line '{0}'.", line));

                Match oFormMatch = offensiveFormationRegex.Match(line);
                if (oFormMatch != Match.Empty)
                {
                    string formation = oFormMatch.Groups[1].ToString();
                    tool.SetTeamOffensiveFormation(team, formation);
                }
            }
            else if (weekRegex.Match(line) != Match.Empty)  //line.StartsWith("WEEK"))
            {
                currentState = scheduleState;
                if (scheduleList == null)
                    scheduleList = new List<string>(300);
                scheduleList.Add(line);
            }
            else if (yearRegex.Match(line) != Match.Empty)//line.StartsWith("YEAR"))
            {
                SetYear(line);
            }
            else if (line.StartsWith("AFC") || line.StartsWith("NFC"))
            {
                String[] parts = line.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts != null && parts.Length > 3)
                {
                    try
                    {
                        tool.SetProBowlPlayer((Conference)Enum.Parse(typeof(Conference), parts[0]),
                            parts[1], parts[2],
                            (TSBPlayer)Enum.Parse(typeof(TSBPlayer), parts[3]));
                    }
                    catch (Exception)
                    {
                        StaticUtils.AddError("Error processing line > " + line);
                    }
                }
            }
            else if (currentState == scheduleState)
            {
                if (scheduleList != null)
                    scheduleList.Add(line);
            }
            else if (currentState == rosterState)
            {
                UpdateRoster(line);
            }
            else
            {
                StaticUtils.AddError(string.Format("Garbage/orphin line not applied \"{0}\"", line));
            }
		}

		private void SetYear(string line)
		{
			Match m = yearRegex.Match(line);
			string year = m.Groups[1].ToString();
			if(year.Length < 1)
			{
				StaticUtils.AddError(string.Format("'{0}' is not valid.",line));
			}
			else
			{
				tool.SetYear(year);
				StaticUtils.LogMessage(String.Format("Year set to '{0}'",year));
			}
		}

		private string GetTeam(string line)
		{
			Match m = teamRegex.Match(line);
			string team = m.Groups[1].ToString();
			return team;
		}

		public static int[] GetSimData(string line)
		{
			Match m = simDataRegex.Match(line);
			//string data = m.Groups[2].ToString();
			string data = m.Groups[1].ToString();
			string simOffensePref = m.Groups[2].ToString();
			int[] ret = {-1,-1};

			if(data.Length > 0)
			{
				try
				{
					int simData = StaticUtils.ParseIntFromHexString(data);
					ret[0]=simData;
				}
				catch
				{
					StaticUtils.AddError(string.Format("Error getting SimData with line '{0}'.",line));
				}
			}

			if(simOffensePref.Length > 0)
			{
				try
				{
					int so = Int32.Parse(simOffensePref);
					ret[1] = so;
				}
				catch
				{
					StaticUtils.AddError(string.Format("Error getting SimData with line '{0}'.",line));
				}
			}
			return ret;
		}

		private string GetAwayTeam(string line)
		{
			Match m = gameRegex.Match(line);
			string awayTeam = m.Groups[1].ToString();
			return awayTeam;
		}
		
		private string GetHomeTeam(string line)
		{
			Match m = gameRegex.Match(line);
			string team = m.Groups[2].ToString();
			return team;
		}

		private int GetWeek(string line)
		{
			Match m = weekRegex.Match(line);
			string week_str = m.Groups[1].ToString();
			int ret = -1;
			try{
				ret = Int32.Parse(week_str);
				ret--; // our week starts at 0
			}
			catch{
				StaticUtils.AddError(string.Format("Week '{0}' is invalid.",week_str));
			}
			return ret;
		}

		private bool SetCurrentTeam(string team)
		{
			if(TecmoTool.GetTeamIndex(team) < 0)
			{//error condition
				StaticUtils.AddError(string.Format("Team '{0}' is Invalid.",team));
				return false;
			}
			else
				this.currentTeam = team;
			return true;
		}

		protected virtual void UpdateRoster(string line)
		{
			if(line.StartsWith("KR"))
				SetKickReturnMan(line);
			else if(line.StartsWith("PR"))
				SetPuntReturnMan(line);
			else if(line.StartsWith("RETURN_TEAM"))
			{
				Match m = returnTeamRegex.Match(line);
				if( m == Match.Empty )
				{
					StaticUtils.AddError(string.Format(
						"Error with line '{0}'.\n\tCorrect Syntax ='RETURN_TEAM POS1, POS2, POS3'",
						line));
				}
				else
				{
					string pos1 = m.Groups[1].ToString();
					string pos2 = m.Groups[2].ToString();
					string pos3 = m.Groups[3].ToString();
					tool.SetReturnTeam(currentTeam, pos1,pos2,pos3);
				}
			}
			else
			{
				Match m = posNameFaceRegex.Match( line );
				if( line.IndexOf("#") > -1 )
				{
					if( numberRegex.Match(line) == Match.Empty )
					{
						StaticUtils.AddError(string.Format("ERROR! (jersey number) Line  {0}",line));
						return;
					}
				}
				string p = m.Groups[1].ToString();
				if(  m != Match.Empty && tool.IsValidPosition(p) )
				{
					if(line.StartsWith("QB"))
						SetQB(line);
					else if(line.StartsWith("WR") || line.StartsWith("RB") ||
						line.StartsWith("TE"))
						SetSkillPlayer(line);
					else if(line.StartsWith("C") || line.StartsWith("RG") || 
						line.StartsWith("LG")    || line.StartsWith("RT") ||
						line.StartsWith("LT"))
					{
						SetOLPlayer(line);
					}
					else if(line.IndexOf("LB") == 2 || line.IndexOf("CB") == 1 ||
						line.StartsWith("RE") || line.StartsWith("LE")  ||
						line.StartsWith("NT") || line.StartsWith("SS")  ||
						line.StartsWith("FS") || line.StartsWith("DB")    )
					{
						SetDefensivePlayer(line);
					}
					else if( line.StartsWith("P") || line.StartsWith("K"))
						SetKickPlayer(line);
				}
				else
				{
					StaticUtils.AddError(string.Format("ERROR! With line \"{0}\"     team = {1}", line, currentTeam));
				}
			}
		}

		//QB1, chris MILLER, Face=0x33, #12, 25, 69, 13, 13, 31, 44, 50, 31 ,[2, 4, 3 ]

		private void SetQB(string line)
		{
			string fname = GetFirstName(line);
			string lname = GetLastName(line);
			string pos = GetPosition(line);
			int face = GetFace(line);
			int jerseyNumber = GetJerseyNumber(line);//will be in hex, not base 10
			if(face > -1)
				tool.SetFace(currentTeam,pos,face);
			if( jerseyNumber < 0)
			{
				StaticUtils.AddError(string.Format("Error with jersey number for '{0} {1}', setting to 0.",fname,lname));
				jerseyNumber=0;
			}
			tool.InsertPlayer(currentTeam,pos,fname,lname,(byte)jerseyNumber);

			int[] vals = GetInts(line);
			int[] simVals = GetSimVals(line);
			if(vals != null && vals.Length > 7)
				tool.SetQBAbilities(currentTeam,pos,vals[0],vals[1],vals[2],vals[3],vals[4],vals[5],vals[6],vals[7]);
			else
				StaticUtils.AddError(string.Format("Warning! could not set ability data for {0} {1},",currentTeam,pos));
			if(face > -1)
				tool.SetFace(currentTeam,pos,face);
			if(simVals != null)
				tool.SetQBSimData(currentTeam,pos,simVals);
			else if(showSimError)
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No sim data specified.",line));
		}

		private void SetSkillPlayer(string line)
		{
			string fname = GetFirstName(line);
			string lname = GetLastName(line);
			string pos = GetPosition(line);
			int face = GetFace(line);
			int jerseyNumber = GetJerseyNumber(line);//will be in hex, not base 10
			tool.SetFace(currentTeam,pos,face);
			tool.InsertPlayer(currentTeam,pos,fname,lname,(byte)jerseyNumber);

			int[] vals = GetInts(line);
			int[] simVals = GetSimVals(line);
			if(vals != null && vals.Length > 5)
				tool.SetSkillPlayerAbilities(currentTeam,pos,vals[0],vals[1],vals[2],vals[3],vals[4],vals[5]);
			else
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No player data specified.",line));
			if(simVals!= null&& simVals.Length > 3)
				tool.SetSkillSimData(currentTeam,pos,simVals);
			else  if(showSimError)
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No sim data specified.",line));
		}

		private void SetOLPlayer(string line)
		{
			string fname = GetFirstName(line);
			string lname = GetLastName(line);
			string pos = GetPosition(line);
			int face = GetFace(line);
			int jerseyNumber = GetJerseyNumber(line);//will be in hex, not base 10
			int[] vals = GetInts(line);
			
			tool.SetFace(currentTeam,pos,face);
			tool.InsertPlayer(currentTeam,pos,fname,lname,(byte)jerseyNumber);

			if(vals != null && vals.Length > 3)
				tool.SetOLPlayerAbilities(currentTeam,pos,vals[0],vals[1],vals[2],vals[3]);
			else
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No player data specified.",line));
			
		}

		protected virtual void SetDefensivePlayer(string line)
		{
			string fname = GetFirstName(line);
			string lname = GetLastName(line);
			string pos = GetPosition(line);
			int face = GetFace(line);
			int jerseyNumber = GetJerseyNumber(line);//will be in hex, not base 10
			int[] vals = GetInts(line);
			int[] simVals = GetSimVals(line);
			
			tool.SetFace(currentTeam,pos,face);
			tool.InsertPlayer(currentTeam,pos,fname,lname,(byte)jerseyNumber);

			if(vals != null && vals.Length > 5)
				tool.SetDefensivePlayerAbilities(currentTeam,pos,vals[0],vals[1],vals[2],vals[3],vals[4],vals[5]);
			else
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. Invalid player attributes.",line));
			if(simVals != null && simVals.Length > 1)
				tool.SetDefensiveSimData(currentTeam,pos,simVals);
			else if(showSimError)
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No sim data specified.",line));
		}

		private void SetKickPlayer(string line)
		{
			string fname = GetFirstName(line);
			string lname = GetLastName(line);
			string pos = GetPosition(line);
			int face = GetFace(line);
			int jerseyNumber = GetJerseyNumber(line);//will be in hex, not base 10
			int[] vals = GetInts(line);
			int[] simVals = GetSimVals(line);
			
			tool.SetFace(currentTeam,pos,face);
			tool.InsertPlayer(currentTeam,pos,fname,lname,(byte)jerseyNumber);
			if(vals != null && vals.Length > 5)
				tool.SetKickPlayerAbilities(currentTeam,pos,vals[0],vals[1],vals[2],vals[3],vals[4],vals[5]);
			else
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No player data specified.",line));
			if(simVals != null && pos == "P")
				tool.SetPuntingSimData(currentTeam, simVals[0]);
			else if(simVals != null && pos == "K")
				tool.SetKickingSimData(currentTeam, simVals[0]);
			else if(showSimError)
				StaticUtils.AddError(string.Format("Warning! On line '{0}'. No sim data specified.",line));
		}

		private static Regex KickRetMan = new Regex("^KR\\s*,\\s*([A-Z1-4]+)$");
		private static Regex PuntRetMan = new Regex("^PR\\s*,\\s*([A-Z1-4]+)$");

		private void SetKickReturnMan(string line)
		{
			Match m = KickRetMan.Match(line);
			if( m != Match.Empty )
			{
				string pos = m.Groups[1].ToString();
				if( tool.IsValidPosition( pos ) )
				{
					tool.SetKickReturner(currentTeam, pos);
				}
				else
					StaticUtils.AddError(string.Format("ERROR with line '{0}'.",line));
			}
		}

		private void SetPuntReturnMan(string line)
		{
			Match m = PuntRetMan.Match(line);
			if( m != Match.Empty )
			{
				string pos = m.Groups[1].ToString();
				if( tool.IsValidPosition( pos ) )
				{
					tool.SetPuntReturner(currentTeam, pos);
				}
				else
					StaticUtils.AddError(string.Format("ERROR with line '{0}'.",line));
			}
		}

		/// <summary>
		/// Expect line like '   [8, 9, 0 ]'
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static int[] GetSimVals(string input)
		{
			if( input != null )
			{
				string stuff = input.Trim();
				int start = stuff.IndexOf("[");
				int end = stuff.IndexOf("]");
				if(start > -1 && end > -1)
				{
					stuff = stuff.Substring(start+1,end-start-1);
					return GetInts(stuff);
				}
			}
			return null;
		}

        public static int[] GetInts(string input)
        {
            return GetInts(input, false);
        }

        public static int[] GetInts(string input, bool useHex)
        {
            if (input != null)
            {
                //System.Globalization.NumberStyles ns = System.Globalization.NumberStyles.None;
                //if (useHex)
                //    ns = System.Globalization.NumberStyles.AllowHexSpecifier;
                int pound = input.IndexOf("#");
                int brace = input.IndexOf("[");
                if (pound > -1)
                    input = input.Substring(pound + 3);
                if (brace > -1)
                {
                    brace = input.IndexOf("[");
                    input = input.Substring(0, brace);
                }
                char[] seps = new char[] { ' ', ',', '\t' };
                string[] nums = input.Split(seps);
                int j, count = 0;
                for (j = 0; j < nums.Length; j++)
                    if (nums[j].Length > 0)
                        count++;
                int[] result = new int[count];
                j = 0;

                string s = "";
                int i = 0;
                try
                {
                    for (i = 0; i < nums.Length; i++)
                    {
                        s = nums[i] as string;
                        if (s != null && s.Length > 0)
                        {
                            if (useHex)
                                result[j++] = StaticUtils.ParseIntFromHexString(s);
                            else
                                result[j++] = Int32.Parse(s);
                        }
                    }
                    return result;
                }
                catch (Exception e)
                {
                    string error = String.Format("Error with input '{0}', {1}, was jersey number specified?", input, e.Message);
                    StaticUtils.AddError(error);
                }
            }
            return null;
        }

		public static int GetJerseyNumber(string line)
		{
			int ret = -1;
			Regex jerseyRegex = new Regex("#([0-9]+)");
			string num = jerseyRegex.Match(line).Groups[1].ToString();
			try
			{
                ret = StaticUtils.ParseIntFromHexString(num);
			}
			catch{ret = -1; }
			return ret;
		}

		public static int GetFace(string line)
		{
			int ret = -1;
			Regex hexRegex = new Regex("0[xX]([A-Fa-f0-9]+)");
			Match m = hexRegex.Match(line);
			if( m != Match.Empty )
			{
				string num = m.Groups[1].ToString();
				try
				{
                    ret = StaticUtils.ParseIntFromHexString(num);
                }
				catch
				{
					ret = -1; 
					StaticUtils.AddError(string.Format("Face ERROR line '{0}'",line));
				}
			}
			
			return ret;
		}

		public string GetPosition(string line)
		{
			string pos = posNameFaceRegex.Match(line).Groups[1].ToString();
			return pos;
		}

		public static string GetLastName(string line)
		{
			string ret ="";
			Match m = posNameFaceRegex.Match(line);
			if(m != Match.Empty )
			{
				string name = m.Groups[2].ToString().Trim();
				int index = name.LastIndexOf(" ");
				ret = name.Substring(index+1);
			}
			return ret;
		}

		public static string GetFirstName(string line)
		{
			string ret ="";
			Match m = posNameFaceRegex.Match(line);
			if(m != Match.Empty )
			{
				string name = m.Groups[2].ToString().Trim();
				int index = name.LastIndexOf(" ");
				if( index > -1 && index < name.Length )
					ret = name.Substring(0, index);
			}
			return ret;
		}
        /*
        static Regex mFirstNameRegex = new Regex("([a-z. ]+)");
        static Regex mLastNameRegex = new Regex(" ([A-Z. ]+)");

        public static string GetLastName(string line)
        {
            string ret = "";
            Match m = posNameFaceRegex.Match(line);
            if (m != Match.Empty)
            {
                string name = m.Groups[2].ToString().Trim();
                Match m2 = mLastNameRegex.Match(name);
                if (m2 != Match.Empty)
                    ret = m2.Groups[1].ToString().Trim();
                else
                    StaticUtils.WriteError("ERROR Getting last name for>" + line);
            }
            return ret;
        }

        public static string GetFirstName(string line)
        {
            string ret = "";
            Match m = posNameFaceRegex.Match(line);
            if (m != Match.Empty)
            {
                string name = m.Groups[2].ToString().Trim();
                Match m2 = mFirstNameRegex.Match(name);
                if (m2 != Match.Empty)
                    ret = m2.Groups[1].ToString().Trim();
                else
                    StaticUtils.WriteError("ERROR Getting first name for>" + line);
            }
            return ret;
        }*/

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="byteString">String in the format of a hex string (0123456789ABCDEF), must have
		/// an even number of characters.</param>
		/// <returns>The bytes.</returns>
		public static byte[] GetBytesFromString(string byteString)
		{
			byte[] ret = null;
			byte[] tmp = null;
			string b;
			if( byteString!= null && byteString.Length > 1 && (byteString.Length % 2) == 0)
			{
				tmp = new byte[byteString.Length/2];
				for(int i =0; i < tmp.Length; i++)
				{
					b = byteString.Substring(i*2,2);
                    tmp[i] = StaticUtils.ParseByteFromHexString(b);// byte.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				ret = tmp;
			}
			return ret;
		}

		public static string GetHomeUniformColorString(string line)
		{
			string tmp = string.Empty;
			Match match = homeRegex.Match(line);
			if( match != Match.Empty )
			{
				tmp = match.Groups[1].Value;
			}
			return tmp;
		}
		public static string GetAwayUniformColorString(string line)
		{
			string tmp = string.Empty;
			Match match = awayRegex.Match(line);
			if( match != Match.Empty )
			{
				tmp = match.Groups[1].Value;
			}
			return tmp;
		}
		public static string GetConfChampColorString(string line)
		{
			string tmp = string.Empty;
			Match match = confChampRegex.Match(line);
			if( match != Match.Empty )
			{
				tmp = match.Groups[1].Value;
			}
			return tmp;
		}
		public static string GetDivChampColorString(string line)
		{
			string tmp = string.Empty;
			Match match = divChampRegex.Match(line);
			if( match != Match.Empty )
			{
				tmp = match.Groups[1].Value;
			}
			return tmp;
		}

		public static string GetUniformUsageString(string line)
		{
			string tmp = string.Empty;
			Match match = uniformUsageRegex.Match(line);
			if( match != Match.Empty )
			{
				tmp = match.Groups[1].Value;
			}
			return tmp;
		}
		/// <summary>
		/// Returns the text string passed, without thr trailing commas.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string DeleteTrailingCommas(string text)
		{
			Regex rs = new Regex(",+\n");
			Regex rrs = new Regex(",+$");
			string ret = rs.Replace(text,"\n");
			ret = rrs.Replace(ret,"");

			return ret;
		}

	}
}
