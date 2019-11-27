using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool
{
	/// <summary>
	/// Summary description for SNES_TecmoTool.
	/// Location = pointer - 0x8000 + 0x0010;
	/// Where pointer is of the 'swapped' format like '0x86dd'
	/// </summary>
    public class SNES_TecmoTool : ITecmoTool, ITecmoContent
	{
		private byte[] outputRom;
		private const int nameNumberSegmentEnd      = 0x17b7f0;
		private const int namePointersStart         = 0x178038;
		private const int playerNumberNameDataStart = 0x17873a;
		private const int teamSimOffensivePrefStart = 0x15DFA; //0x27526;
		protected     int dataPositionOffset        = 0x170000;
		private const int pr_kr_start_offset        = 0x170c90;
		private const int pr_kr_team_start_offset   = 0x170cb0;
		private const int lastPointer               = 0x178738; //= 0x178736;

		private const int ROM_LENGTH                = 1572864;

        /// <summary>
        /// Returns the rom version 
        /// </summary>
        public virtual ROM_TYPE RomVersion { get { return ROM_TYPE.SNES_TSB1; } }

		//		private bool mShowTeamFormation = false;
		//
		//		private int startScreenLine1Loc = 0xc4ec;// TODO allow user to edit these 2 lines.
		//		private int startScreenLine2Loc = 0xc504;
		
		public byte[] OutputRom
		{
			get{ return outputRom;}
			set{ outputRom = value; }
		}

		public bool ShowOffPref
		{
			get{ return mShowOffPref;}
			set{ mShowOffPref = value;}
		}

        public static bool GUI_MODE = false;
		public static bool AUTO_CORRECT_SCHEDULE = true;

		private bool mShowOffPref = true;
		
		private const int billsQB1SimLoc  = 0x26acf; //0x18163;
		private const int billsRESimLoc   = 0x26ae7; //0x1817b;
		
		private const int billsTeamSimLoc = 0x26afe;//0x18192;
		private const int teamSimOffset   = 0x30;
		
		private const int billsQB1AbilityStart = 0x17e000;
		private const int teamAbilityOffset = 0x7D;
		public  const int QUARTER_LENGTH = 0xA0EE;

		private int[] abilityOffsets={   0x00, 0x05, 0x0A, 0x0E, 0x12, 0x16, 0x1A, 0x1E, 0x22, 0x26, 0x2A,
										 0x2E, 0x32, 0x35, 0x38, 0x3B, 0x3E, 0x41, 0x45, 0x49, 0x4D, 0x51,
										 0x55, 0x59, 0x5D, 0x61, 0x65, 0x69, 0x6D, 0x71, 0x75, 0x79  };

		private int[] gameYearLocations = { 0x2e16b,  0x12329e, 0x123348, 0x171069, 0x1712fb,
											  0x17133c, 0x172c84, 0x172d02, //0x172d10, //0x172cae, 
											  /*0x172d02,*/ 0x172d3e, 0x172d5b, 0X172D1F
										  };

		private  string[] positionNames = { 
											  "QB1", "QB2", "RB1", "RB2",  "RB3",  "RB4",  "WR1",  "WR2", "WR3", "WR4", "TE1", 
											  "TE2", "C",   "LG",  "RG",   "LT",   "RT",
											  "RE",  "NT",  "LE",  "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS",  "SS",  
											  "K",   "P" ,  "DB1", "DB2"
										  };

		private static  string[] teams =
		{
			"bills",   "colts",  "dolphins", "patriots",  "jets",
			"bengals", "browns", "oilers",   "steelers",
			"broncos", "chiefs", "raiders",  "chargers",  "seahawks",
			"cowboys", "giants", "eagles",   "cardinals", "redskins",
			"bears",   "lions",  "packers",  "vikings",   "buccaneers",
			"falcons", "rams",   "saints",   "49ers"
		};

		private static  string[] mSimTeams =
		{
			"bills",    "colts",  "dolphins", "patriots",  "jets",
			"bengals",  "browns", "oilers",   "steelers",
			"broncos",  "chiefs", "raiders",  "chargers",  "seahawks",
			"redskins", "giants", "eagles",   "cardinals", "cowboys",
			"bears",    "lions",  "packers",  "vikings",   "buccaneers",
			"49ers",    "rams",   "saints",   "falcons"
		};

		public SNES_TecmoTool(string fileName)
		{
			Init(fileName);
		}

		public string[] GetTeams()
		{
			return teams;
		}

		public string[] GetPositionNames()
		{
			return positionNames;
		}

		public bool IsValidPosition( string pos )
		{
			bool ret = false;
			for( int i = 0; i < positionNames.Length; i++)
			{
				if(pos == positionNames[i] )
				{
					ret = true;
					break;;
				}
			}
			return ret;
		}

		public bool IsValidTeam(string team)
		{
			bool ret = false;
			for( int i = 0; i < teams.Length; i++)
			{
				if(team == teams[i] )
				{
					ret = true;
					break;;
				}
			}
			return ret;
		}
        // use this instead of directoy setting data in OutputRom
        public void SetByte(int location, byte b)
        {
            this.OutputRom[location] = b;
        }

		protected bool Init(string fileName)
		{
			return ReadRom(fileName);
		}

		public void Test2()
		{
			string team = "bills";
			for(int i = 0; i < positionNames.Length;i++)
			{
				InsertPlayer(team,positionNames[i],"player",team, (byte) (i % 10));
				switch(positionNames[i])
				{
					case "QB1":  case "QB2": 
						SetQBAbilities(team,positionNames[i],31,31,31,31,31,31,31,31);
						break;
					case "RB1": case "RB2": case "RB3": case "RB4":
					case "WR1": case "WR2": case "WR3": case "WR4":
					case "TE1": case "TE2": 
						SetSkillPlayerAbilities(team,positionNames[i],31,31,31,31,31,31);
						break;
					case "C":   case "RG":	case "LG":
					case "RT":	case "LT": 
						SetOLPlayerAbilities(team,positionNames[i],31,31,31,31);
						break;
					case "RE":   case "NT":   case "LE":   case "LOLB":
					case "LILB": case "RILB": case "ROLB": case "RCB":
					case "LCB":  case "FS":   case "SS": 
						SetDefensivePlayerAbilities(team,positionNames[i],31,31,31,31,31,31);
						break;
					case "K":  
					case "P":  
						SetKickPlayerAbilities(team,positionNames[i],31,31,31,31,31,31);
						break;
				}
			}
		}

		public void shiftTest()
		{
			byte[] stuff  = {0xff, 0xff, 0xff, 0xff, 0xff,
								0x4a, 0x4c, 0x4e,0x50, 0x52, 0x54, 0x56, 0x58, 0x5a, 
								0x5c, 0x5e, 0x60, 0x62, 0x64, 0x66, 0x68, 0x6a, 0x6c, 
								0x6e, 0x70, 0x72, 0xff, 0xff, 0xff, 0xff, 0xff	};
			for(int i =0; i < stuff.Length; i++)
				Console.Write(" {0:x} ",stuff[i]);
			Console.WriteLine();
			Console.WriteLine("shift 3");
			this.ShiftDataDown(6, stuff.Length-7, 3, stuff);
			for(int i =0; i < stuff.Length; i++)
				Console.Write(" {0:x} ",stuff[i]);
			Console.WriteLine();

		}
		
		public bool ReadRom(string filename)
		{
			bool ret = false;
			try
			{
				System.Windows.Forms.DialogResult result = 
					System.Windows.Forms.DialogResult.Yes;
				FileInfo f1 = new FileInfo(filename);
				long len = f1.Length;
				if( len != ROM_LENGTH )
				{
					if( MainClass.GUI_MODE )
					{
						result = System.Windows.Forms.MessageBox.Show(null, 
							string.Format(
							@"Warning! 

The input Rom is not the correct Size ({0} bytes).

You should only continue if you know for sure that you are loading a snes TSB1 ROM.

Do you want to continue?",ROM_LENGTH),
							"WARNING!",
							System.Windows.Forms.MessageBoxButtons.YesNo,
							System.Windows.Forms.MessageBoxIcon.Warning );
					}
					else
					{
						string msg = String.Format(
							@"ERROR! ROM '{0}' is not the correct length.  
    Legit TSB1 snes ROMS are {1} bytes long.
    If you know this is really a snes TSB1 ROM, you can force TSBToolSupreme to load it in GUI mode.",
							filename, ROM_LENGTH);
						StaticUtils.AddError(msg);
					}
				}
				
				if( result == System.Windows.Forms.DialogResult.Yes )
				{
					FileStream s1 = new FileStream(filename, FileMode.Open);
					outputRom = new byte[(int)len];
					s1.Read(outputRom,0,(int)len);
					s1.Close();
					
					//					if( len == ROM_LENGTH + 0x200 )// works
					//					{// some TSB1 snes ROMS are like this, we'll just strip the first 
					//						byte[] newRom = new byte[ROM_LENGTH];
					//						Array.Copy(outputRom,0x200, newRom,0,newRom.Length);
					//						outputRom = newRom;
					//					}
					ret = true;
				}
			}
			catch(Exception e)
			{
				StaticUtils.ShowError(e.ToString());
			}
			return ret;
		}

		public void SaveRom(string filename)
		{
			if( filename != null )
			{
				try
				{
					long len = outputRom.Length;
					FileStream s1 = new FileStream(filename, FileMode.OpenOrCreate);
					s1.Write (outputRom,0,(int)len);
					s1.Close();
				}
				catch(Exception e)
				{
					StaticUtils.ShowError(e.ToString());
				}
			}
			else
			{
				StaticUtils.AddError("ERROR! You passed a null filename");
			}
		}

		/// <summary>
		/// Returns a string consisting of number, name\n for all players in the game.
		/// </summary>
		/// <returns></returns>
		public string GetPlayerStuff(bool jerseyNumber_b, bool name_b, bool face_b, 
			bool abilities_b, bool simData_b)
		{
			StringBuilder sb = new StringBuilder(16*28*30*3);
			string team="";
			for(int i =0; i < teams.Length; i++)
			{
				team = teams[i];
				sb.Append(string.Format("TEAM={0}\n",team));
				for(int j = 0; j < positionNames.Length; j++)
				{
					sb.Append(GetPlayerData(team,positionNames[j],abilities_b,jerseyNumber_b,face_b,name_b,simData_b)+"\n");
				}
			}
			return sb.ToString();
		}

		public string GetSchedule()
		{
			string ret = "";
			if( outputRom != null )
			{
				SNES_ScheduleHelper sh2 = new SNES_ScheduleHelper(outputRom);
				ret = sh2.GetSchedule();
                StaticUtils.ShowErrors();
			}
			return ret;
		}

		public void SetYear(string year)
		{
			if(year == null || year.Length != 4)
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) {0} is not a valid year.",year));
				return;
			}
			int location;
			for(int i = 0 ; i < gameYearLocations.Length; i++)
			{
				location = gameYearLocations[i];
				outputRom[location]   = (byte)year[0];
				outputRom[location+1] = (byte)year[1];
				outputRom[location+2] = (byte)year[2];
				outputRom[location+3] = (byte)year[3];
			}
			if(year != "1993")
			{
				// This spot in the ROM doesn't like change
				// so we just blank it out.
				outputRom[0x172d02] = (byte)' ';
				outputRom[0x172d03] = (byte)' ';
				outputRom[0x172d04] = (byte)' ';
				outputRom[0x172d05] = (byte)' ';
			}
			try
			{
				//0x2E193 = 28th  (28th superbowl)
				int theYear = Int32.Parse(year);
				int superbowlNumber = theYear-1965;
				if( superbowlNumber < 0 )
					superbowlNumber = 0;

				string sbw;// = superbowlNumber.ToString();
			
				string suffix = "TH";
				int test = superbowlNumber % 10;
			
				switch(test)
				{
					case 1: suffix = "ST"; break;
					case 2: suffix = "ND"; break;
					case 3: suffix = "RD"; break;
				}
				if(superbowlNumber < 10)
				{
					sbw = " "+superbowlNumber+suffix;
				}
				else if( superbowlNumber < 21 )
				{
					sbw = " "+superbowlNumber+"TH";
				}
				else
				{
					sbw = superbowlNumber+suffix;
				}

			
				outputRom[0x2E193] = (byte)sbw[0];
				outputRom[0x2E194] = (byte)sbw[1];
				outputRom[0x2E195] = (byte)sbw[2];
				outputRom[0x2E196] = (byte)sbw[3];
			}
			catch(Exception )
			{
				StaticUtils.AddError("Problem setting superbowl number.");
			}
		}

		public string GetYear()
		{
			int location = gameYearLocations[0];
			string ret ="";
			for(int i =location; i < location+4; i++)
				ret += (char)outputRom[i];

			return ret;
		}

		public void InsertPlayer(string team, 
			string position, 
			string fname, 
			string lname, 
			byte number)
		{
			if( !IsValidPosition( position) || fname == null || lname == null || fname.Length < 1 || lname.Length < 1)
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) InsertPlayer:: Player name or position invalid"));
			}
			else
			{
				fname = fname.ToLower();
				lname = lname.ToUpper(); //15 18  char max for name
				if(lname.Length + fname.Length > 17)
				{
					StaticUtils.AddError(string.Format("Warning!! There is a 17 character limit for names\n '{0} {1}' is {2} characters long.",
						fname,lname, fname.Length+lname.Length));
					if(lname.Length > 16)
					{
						lname= lname.Substring(0,12);
						//fname =""+fname[0]+".";
						fname = string.Format("{0}.",fname[0]);
					}
					else
						fname = string.Format("{0}.",fname[0]);
					//fname = ""+fname[0];

					StaticUtils.AddError(string.Format("Name will be {0} {1}", fname, lname ));
				}
				if(fname.Length < 1)
					fname = "Joe";
				if(lname.Length < 1)
					lname = "Nobody";
			
				string oldName = GetName(team,position);
				byte[] bytes = new byte[1+fname.Length+lname.Length];
				int change = bytes.Length - oldName.Length;
				int i=0;
				bytes[0] = number;
				for(i=1; i < fname.Length+1; i++)
					bytes[i] = (byte)fname[i-1];
				for(int j = 0;j < lname.Length;j++)
					bytes[i++]=(byte)lname[j]; 
				int pos = GetPointerPosition(team,position);

				UpdatePlayerData(team,position,bytes, change);
				AdjustDataPointers(pos, change, lastPointer);
			}
		}

		private void AdjustDataPointers(int pos, int change, int lastPointerLocation)
		{
			byte low, hi;
			int  word;
			// last pointer is at 0x178738
			
			int i=0;
			int end = lastPointerLocation +1;//here was just lastPointer
			for( i = pos+2; i < end; i+=2)
			{
				low  =  outputRom[i];
				hi   =  outputRom[i+1];
				word =  hi;
				word =  word << 8;
				word += low;
				word += change;
				low  =  (byte)(word & 0x00ff);
				word =  word >> 8;
				hi   =  (byte)word;
				outputRom[i] = low;
				outputRom[i+1] = hi;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="team">The team the player is assigned to.</param>
		/// <param name="position">The player's position ('QB1', 'WR1' ...)</param>
		/// <returns></returns>
		public string GetName(string team, string position)
		{
			if( !IsValidTeam( team) || !IsValidPosition( position ) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetName:: team '{0}' or position '{1}' is invalid.",
					team,position));
				return null;
			}
			int pos = GetDataPosition(team,position);
			int nextPos = GetNextDataPosition(team,position);
			string name ="";

			if( pos < 0 )
				return "ERROR!";
			if(nextPos > 0)
			{
				//start at pos+1 to skip his jersey number. 
				for(int i = pos+1;i < nextPos ; i++)
					name += (char)outputRom[i];
			}
			else
			{ // 49ers DB2
				for(int i = pos+1;outputRom[i] != 0xff ; i++)
					name += (char)outputRom[i];
			}
			int split =1;
			for(int i=0; i < name.Length; i++)
			{
				if((byte)name[i] > 64 && (byte)name[i] < 91)
				{
					split = i; break;
				}
			}

			string first,last,full;
			full = null;
			try
			{
				first = name.Substring(0,split);
				last = name.Substring(split);
				full = first+" "+last;
			}
			catch
			{
				return full;
			}
			return full;
		}

		public string GetPlayerData(string team, string position, bool ability_b,
			bool jerseyNumber_b, bool face_b, bool name_b,bool simData_b )
		{
			if( !IsValidTeam( team))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) Team {0} is invalid.",team));
				return null;
			}
			else if( !IsValidPosition(position) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) position {0} is invalid.",position));
				return null;
			}

			StringBuilder result = new StringBuilder();

			//result.Append( string.Format("{0}, {1}, Face=0x{2:x}, ",
			//	position, GetName(team,position), GetFace(team,position)));
			result.Append(string.Format("{0}, ",position));
			if(name_b)
				result.Append(string.Format("{0}, ",GetName(team,position)));
			if(face_b)
				result.Append(string.Format("Face=0x{0:x}, ",GetFace(team,position)));
			int location = GetDataPosition(team,position);

			if(location < 0 )
				return "Messed Up Pointer";

			string jerseyNumber = string.Format("#{0:x}, ",(byte)outputRom[location]);
			if(jerseyNumber_b)
				result.Append(jerseyNumber);
			if(ability_b)
				result.Append(GetAbilityString(team,position));
			int[] simData = GetPlayerSimData(team,position);
			if(simData != null && simData_b)
				result.Append( string.Format(",[{0}]",StringifyArray(simData)));
			return result.ToString();
		}

		public string GetKey()
		{
            return String.Format(
@"# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/
# Editing: Tecmo Super Bowl I (snes) [{0}]
# 
# Double click on a team name (or playbook) to bring up the edit Team GUI.
# Double click on a player to bring up the edit player GUI (Click 'Sim Data'
#   button to find out more on Sim Data). 
# Key
# -- Quarterbacks:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, PS, PC, PA, APB, [Sim rush, Sim pass, Sim Pocket].
# -- Offensive Skill players (non-QB):
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, BC, REC, [Sim rush, Sim catch, Sim punt Ret, Sim kick ret].
# -- Offensive Linemen:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP
# -- Defensive Players:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, PI, QU, [Sim pass rush, Sim coverage].
# -- Punters and Kickers:
# Position, Name (first LAST), FaceID, Jersey number, RP, RS, MS, HP, KA, AKB,[ Sim kicking ability].
# TEAM:\n
#  name, SimData  0x<offense><defense><offense preference>
#  Offensive pref values 0-3. 
#     0 = Little more rushing, 1 = Heavy Rushing,
#     2 = little more passing, 3 = Heavy Passing.
# credit to elway7 for finding	'offense preference'", this.RomVersion);
		}

		public string GetTeamPlayers(string team)
		{
			if( !IsValidTeam( team ) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetTeamPlayers:: team {0} is invalid.",team));
				return null;
			}

			StringBuilder result = new StringBuilder(41* positionNames.Length);
			string pos;
			byte teamSimData = GetTeamSimData(team);
			string data = "";
			if(teamSimData < 0xf)
				data = string.Format("0{0:x}",teamSimData);
			else
				data = string.Format("{0:x}",teamSimData);
			if( ShowOffPref )
				data += GetTeamSimOffensePref(team);

			string teamString = string.Format("TEAM = {0} SimData=0x{1}",team, data);
			result.Append( teamString );
			
			if( TecmoTool.ShowTeamFormation )
			{
				result.Append(string.Format(", {0}", GetTeamOffensiveFormation(team) ));
			}
			result.Append("\n");

			if( TecmoTool.ShowPlaybook )
			{
				result.Append(string.Format("{0}\n", GetPlaybook(team)));
			}
			if (TecmoTool.ShowTeamStrings)
			{
				int teamIndex = GetTeamIndex(team);
				result.Append(string.Format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n", GetTeamAbbreviation(teamIndex), GetTeamCity(teamIndex), GetTeamName(teamIndex)));
			}

			for(int i =0; i < positionNames.Length; i++)
			{
				pos = positionNames[i];
				result.Append(string.Format("{0}\n",GetPlayerData(team,pos,true,true,true,true,true) ));
			}
			result.Append(string.Format("{0}\n", GetReturnTeam(team) ) );
			result.Append( string.Format("KR, {0}\nPR, {1}\n",GetKickReturner(team),GetPuntReturner(team)));
			result.Append("\n");
			return result.ToString();
		}

		#region Team String Table Stuff
		public virtual int NumberOfStringsInTeamStringTable { get { return 30; } }

		public void SetTeamStringTableString(int stringIndex, string newValue)
		{
			int junk = 0;
			string oldValue = GetTeamStringTableString(stringIndex);
			if (oldValue == newValue)
				return;
			int shiftAmount = newValue.Length - oldValue.Length;
			if (shiftAmount != 0)
			{
				int currentPointerLocation = GetTeamStringTableStart() + 2 * stringIndex;
				int lastPointerLocation = GetTeamStringTableStart() + 2 * NumberOfStringsInTeamStringTable;
				AdjustDataPointers(currentPointerLocation, shiftAmount, lastPointerLocation);
				int startPosition = GetTeamStringTableLocation(stringIndex + 1, out junk) - 1;
				int endPosition = 0x7330;
				if (shiftAmount < 0)
					ShiftDataUp(startPosition, endPosition, shiftAmount, outputRom);
				else if (shiftAmount > 0)
					ShiftDataDown(startPosition, endPosition, shiftAmount, outputRom);
			}
			// lay down the value
			int startLoc = GetTeamStringTableLocation(stringIndex, out shiftAmount);
			for (int i = 0; i < newValue.Length; i++)
			{
				if (newValue[i] == '*') // do the star substitution
					OutputRom[startLoc + i] = 0;
				else
					OutputRom[startLoc + i] = (byte)newValue[i];
			}
		}

		public virtual string GetTeamStringTableString(int stringIndex)
		{
			int length = 0;
			int stringStartingLocation = GetTeamStringTableLocation(stringIndex, out length);

			char[] stringChars = new char[length];
			for (int i = 0; i < stringChars.Length; i++)
			{
				stringChars[i] = (char)OutputRom[stringStartingLocation + i];
				if (stringChars[i] == 0)
					stringChars[i] = '*';
			}
			string retVal = new string(stringChars);
			return retVal;
		}

		/// <summary>
		/// Returns the location of the 'Team' string table. This string table 
		/// contains the city abbreviations, city names and team names.
		/// </summary>
		/// <param name="stringIndex">The index of the string to get.</param>
		/// <param name="length">out param stores the length.</param>
		/// <returns>Returns the location of the string at the specified index.</returns>
		private int GetTeamStringTableLocation(int stringIndex, out int length)
		{
			int team_string_table_loc = GetTeamStringTableStart();
			int pointer_loc = team_string_table_loc + 2 * stringIndex;
			byte b1 = OutputRom[pointer_loc + 1];
			byte b2 = OutputRom[pointer_loc];
			byte b3 = OutputRom[pointer_loc + 3]; // b3 & b4 for length
			byte b4 = OutputRom[pointer_loc + 2];
			length = ((b3 << 8) + b4) - ((b1 << 8) + b2);
			if (stringIndex == NumberOfStringsInTeamStringTable - 1)
				length = 9; // hack. there are no more pointers to use for length   :'(
			int pointerVal = (b1 << 8) + b2;
			int stringStartingLocation = pointerVal - 0x8000;// table adjustment
			return stringStartingLocation;
		}

		protected virtual int GetTeamStringTableStart()
		{
			int team_string_table_loc = 0x7000;
			return team_string_table_loc;
		}

		public virtual string GetTeamName(int teamIndex)
		{
			string retVal = GetTeamStringTableString(teamIndex);
			int lastSpace = retVal.LastIndexOf(' ');
			retVal = retVal.Substring(lastSpace+1).Replace("*","");
			return retVal;
		}

		public virtual string GetTeamCity(int teamIndex)
		{
			string retVal = GetTeamStringTableString(teamIndex).Substring(5);
			int lastSpace = retVal.LastIndexOf(' ');
			retVal = retVal.Substring(0, lastSpace);
			return retVal;
		}

		public virtual string GetTeamAbbreviation(int teamIndex)
		{
			string retVal = GetTeamStringTableString(teamIndex);
			retVal = retVal.Substring(0, 4);
			return retVal;
		}

		public virtual void SetTeamAbbreviation(int teamIndex, string abb)
		{
			if (abb == null || abb.Length != 4)
			{
				StaticUtils.AddError(String.Format("Error Setting TeamAbbreviation; TeamIndex:{0}; abb:{1}",teamIndex, abb));
			}
			else
			{
				string teamString = String.Format("{0}*{1} {2}*",abb, GetTeamCity(teamIndex), GetTeamName(teamIndex) );
				SetTeamStringTableString(teamIndex, teamString);
			}
		}

		public virtual void SetTeamName(int teamIndex, string name)
		{
			string teamString = String.Format("{0}*{1} {2}*", GetTeamAbbreviation(teamIndex), GetTeamCity(teamIndex), name);
			SetTeamStringTableString(teamIndex, teamString);
		}

		public virtual void SetTeamCity(int teamIndex, string city)
		{
			string teamString = String.Format("{0}*{1} {2}*", GetTeamAbbreviation(teamIndex),city, GetTeamName(teamIndex));
			SetTeamStringTableString(teamIndex, teamString);
		}
		#endregion

        public virtual string GetAll(int season)
        {
            return GetAll();
        }

        public string GetProBowlPlayers(int season)
        {
            return GetProBowlPlayers();
        }

        public string GetSchedule(int season)
        {
            return GetSchedule();
        }

		public string GetAll()
		{
			string team;
			StringBuilder all = new StringBuilder(30*41*positionNames.Length);
			string year = string.Format("YEAR={0}\n",GetYear());
			all.Append(year);
			for(int i = 0; i < teams.Length; i++)
			{
				team = teams[i];
				all.Append(GetTeamPlayers(team));
			}
			return all.ToString();
		}

		/// <summary>
		/// Gets the point in the player number name data that a player's data begins.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public int GetDataPosition(string team, string position)
		{
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) GetDataPosition:: either team {0} or position {1} is invalid.", team, position));
			}
			int teamIndex     = GetTeamIndex(team);
			int positionIndex = GetPositionIndex(position);
			// the players total index (QB1 bills=0, QB2 bills=2 ...)
			int guy = teamIndex * positionNames.Length + positionIndex;
			int pointerLocation = namePointersStart + (2 * guy);
			byte lowByte = outputRom[pointerLocation];
			int  hiByte  = outputRom[pointerLocation+1];
			hiByte =  hiByte << 8;
			hiByte = hiByte + lowByte;

			//int ret = hiByte - 0x8000 + 0x010;
			int ret = hiByte + dataPositionOffset; //= 0x170000;
			return  ret;
		}

		/// <summary>
		/// Get the starting point of the guy AFTER the one passed to this method.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public int GetNextDataPosition(string team, string position)
		{
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) GetNextDataPosition:: either team {0} or position {1} is invalid.", team, position));
			}

			int ti = GetTeamIndex(team);
			int pi = GetPositionIndex(position);
			pi++;
			if(position == "DB2"/*"P"*/)
			{
				ti++;
				pi=0;
			}
			//if(team == "falcons" && position == "P" )
			if(team == "49ers" && position == "DB2" )
			{ 
				return -1;
			}
			else
				return GetDataPosition(teams[ti],positionNames[pi]);
		}

		private int GetPointerPosition(string team, string position)
		{
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) GetPointerPosition:: either team {0} or position {1} is invalid.", team, position));
			}
			int teamIndex     = GetTeamIndex(team);
			int positionIndex = GetPositionIndex(position);
			int playerSpot    = teamIndex *  positionNames.Length + positionIndex;
			//if(team == "falcons" && position == "P")
			//	return 0x6d6;
			if( team == "49ers" && position == "DB2" )
				//return lastPointer;
				return lastPointer-2;

			if(positionIndex < 0)
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) Position '{0}' does not exist. Valid positions are:",position));
				for(int i =1; i <= positionNames.Length; i++)
				{
					Console.Error.Write("{0}\t", positionNames[i-1]);
				}
				return -1;
			}
			int loc = namePointersStart + (teamIndex * 0x40) + ( positionIndex * 2 );
			return loc;
		}

		/// <summary>
		/// Sets the player data (jersey number, player name) in the data segment.
		/// </summary>
		/// <param name="team">The team the player is assigned to.</param>
		/// <param name="position">The position the player is assigned to.</param>
		/// <param name="bytes">The player's number and name data. </param>
		public void UpdatePlayerData(string team, string position, byte[] bytes, int change)
		{
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) UpdatePlayerData:: either team {0} or position {1} is invalid.", team, position));
			}
			if( bytes == null )
				return;

			int dataStart     = this.GetDataPosition(team,position);
			// need to do a cleaver splice here.
			ShiftDataAfter(team,position, change);
			int j = 0;
			int i;
			for(i = dataStart; j < bytes.Length; i++)
				outputRom[i]= bytes[j++];

			if(team == "49ers" && position == "DB2")
			{
				while(outputRom[i] != 0xff)
				{
					outputRom[i++] = 0xff;
				}
			}
		}

		private void ShiftDataAfter(string team, string position, int shiftAmount)
		{
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) ShiftDataAfter:: either team {0} or position {1} is invalid.", team, position));
			}

			if(team == teams[teams.Length-1] && position == "DB2" /* "P"*/)
				return;

			//int endPosition = 0x0300F; //(end of name-number segment)
			
			int endPosition = nameNumberSegmentEnd;

			while(outputRom[endPosition] == 0xff)
				endPosition--;

			endPosition++;// it was set to falcons punter's last letter

			//int startPosition = GetDataPosition(teams[teamIndex], positionNames[positionIndex]);
			int startPosition = this.GetNextDataPosition(team,position);
			if(shiftAmount < 0)
				ShiftDataUp(startPosition, endPosition, shiftAmount, outputRom);
			else if(shiftAmount > 0)
				ShiftDataDown(startPosition, endPosition, shiftAmount, outputRom);
		}

		private void ShiftDataUp(int startPos, int endPos, int shiftAmount, byte[] data)
		{
			if( startPos  < 0 ||  endPos < 0 )
			{
				throw new Exception(
					string.Format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", startPos, endPos));
			}

			// commented out code was in release 1
			//int end = endPos+shiftAmount;
			int i;
			if(shiftAmount > 0 )
				Console.WriteLine("positive shift amount in ShiftDataUp");

			for(i = startPos /*+ shiftAmount*/; i <= endPos /*end*/; i++)
			{
				data[i+shiftAmount] = data[i];
			}
			/*i--;
			for(int j=shiftAmount; j < 0; j++) 
				data[i++] = 0xff; */

			i+= shiftAmount;
			while( outputRom[i] != 0xff && i < nameNumberSegmentEnd /*0x300f*/)
			{
				outputRom[i] = 0xff;
				i++;
			}

		}

		private void ShiftDataDown(int startPos, int endPos, int shiftAmount, byte[] data)
		{
			if( startPos  < 0 ||  endPos < 0 )
			{
				throw new Exception(
					string.Format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", startPos, endPos));
			}

			for(int i = endPos + shiftAmount; i > startPos ;i--)
			{
				data[i] = data[i-shiftAmount];
			}
		}


		private byte[] GetDataAfter(string team, string position)
		{
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) GetDataAfter:: either team {0} or position {1} is invalid.", team, position));
			}

			if(team == teams[teams.Length-1] && position == "DB2")
				return null;

			int teamIndex     = GetTeamIndex(team);
			int positionIndex = GetPositionIndex(position);
			positionIndex++;
			if(position == "DB2")
			{ // if it's the last guy on the team.
				teamIndex++;
				positionIndex = 0;
			}
			int endPosition = nameNumberSegmentEnd; //0x0300F; //(end of name-numbur segment)
			while(outputRom[endPosition] == 0xff)
				endPosition--;

			endPosition++;// it was set to 49ers DB2's last letter
			int startPosition = GetDataPosition(teams[teamIndex], positionNames[positionIndex]);
			byte[] retBytes = new byte[endPosition - startPosition];

			int j = 0;
			for(int i = startPosition; i < endPosition+1; i++)
				retBytes[j++] = outputRom[i];

			return retBytes;
		}

		public static int GetTeamIndex(string teamName)
		{
			int ret = -1;
			if(teamName.ToLower() == "null")
				return 255;
			for(int i = 0; i < teams.Length; i++)
			{
				if(teams[i] == teamName)
				{
					ret = i;
					break;
				}
			}
			return ret;
		}

		public static int GetSimTeamIndex(string teamName)
		{
			int ret = -1;
			if(teamName.ToLower() == "null")
				return 255;
			for(int i = 0; i < mSimTeams.Length; i++)
			{
				if(mSimTeams[i] == teamName)
				{
					ret = i;
					break;
				}
			}
			return ret;
		}

		/// <summary>
		/// Returns the team specified by the index passed. (0= bills).
		/// </summary>
		/// <param name="index"></param>
		/// <returns>team name on success, null on failure</returns>
		public static string GetTeamFromIndex(int index)
		{
			if(index == 255)
				return "null";
			if(index < 0 || index > teams.Length-1)
				return null;
			return teams[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="positionName"> like 'QB1', 'K','P' ... </param>
		/// <returns></returns>
		internal int GetPositionIndex(string positionName)
		{
			int ret = -1;
			for(int i = 0; i < positionNames.Length; i++)
			{
				if(positionNames[i] == positionName)
				{
					ret = i;
					break;
				}
			}
			return ret;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="team"></param>
		/// <param name="qb">Either 'QB1' or 'QB2'</param>
		public void SetQBAbilities(string team, 
			string qb, 
			int runningSpeed, 
			int rushingPower, 
			int maxSpeed,
			int hittingPower,
			int passingSpeed,
			int passControl,
			int accuracy, 
			int avoidPassBlock
			)
		{
			if( !IsValidTeam(team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) team {0} is invalid",team));
				return;
			}
			if(qb != "QB1" && qb != "QB2")
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) Cannot set qb ablities for {0}",qb));
				return;
			}
			runningSpeed = GetAbility(runningSpeed);
			rushingPower = GetAbility(rushingPower);
			maxSpeed = GetAbility(maxSpeed);
			hittingPower = GetAbility(hittingPower);
			passingSpeed = GetAbility(passingSpeed);
			passControl = GetAbility(passControl);
			accuracy = GetAbility(accuracy);
			avoidPassBlock = GetAbility(avoidPassBlock);

			if( !IsValidAbility(runningSpeed) ||  
				!IsValidAbility(rushingPower) ||  
				!IsValidAbility(maxSpeed) || 
				!IsValidAbility(hittingPower) || 
				!IsValidAbility(passingSpeed) || 
				!IsValidAbility(passControl) || 
				!IsValidAbility(accuracy) ||  
				!IsValidAbility(avoidPassBlock) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) Abilities for {0} on {1} were not set.",qb,team));
				PrintValidAbilities();
				return;
			}
			SaveAbilities(team,qb,runningSpeed,rushingPower,maxSpeed,hittingPower,passingSpeed,passControl);
			int teamIndex = GetTeamIndex(team);
			int posIndex = GetPositionIndex(qb);
			int location = (teamIndex * teamAbilityOffset)+ abilityOffsets[posIndex] + billsQB1AbilityStart;
			int lastByte = accuracy << 4;
			lastByte += avoidPassBlock;
			outputRom[location+4] = (byte) lastByte;
			lastByte = passingSpeed << 4;
			lastByte += passControl;
			outputRom[location+3] = (byte) lastByte;
		}

		public void SetSkillPlayerAbilities(string team, 
			string pos, 
			int runningSpeed, 
			int rushingPower, 
			int maxSpeed,
			int hittingPower,
			int ballControl,
			int receptions
			)
		{
			if( !IsValidTeam(team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) team {0} is invalid",team));
				return;
			}

			if(pos != "RB1" && pos != "RB2"&& pos != "RB3"&& pos != "RB4" &&
				pos != "WR1" && pos != "WR2"&& pos != "WR3"&& pos != "WR4" 
				&& pos != "TE1"&& pos != "TE2")
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) Cannot set skill player ablities for {0}.",pos));
				return;
			}
			runningSpeed = GetAbility(runningSpeed);
			rushingPower = GetAbility(rushingPower);
			maxSpeed = GetAbility(maxSpeed);
			hittingPower = GetAbility(hittingPower);
			ballControl = GetAbility(ballControl);
			receptions = GetAbility(receptions);

			if( !IsValidAbility(runningSpeed) ||  
				!IsValidAbility(rushingPower) ||  
				!IsValidAbility(maxSpeed)     || 
				!IsValidAbility(hittingPower) || 
				!IsValidAbility(receptions )  || 
				!IsValidAbility(ballControl)    )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) Invalid attribute. Abilities for {0} on {1} were not set.",pos,team));
				PrintValidAbilities();
				return;
			}
			SaveAbilities(team,pos,runningSpeed,rushingPower,maxSpeed,hittingPower,ballControl, receptions);
		}

		public void SetKickPlayerAbilities(string team, 
			string pos, 
			int runningSpeed, 
			int rushingPower, 
			int maxSpeed,
			int hittingPower,
			int kickingAbility,
			int avoidKickBlock
			)
		{
			if( !IsValidTeam(team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) team {0} is invalid",team));
				return;
			}

			if(pos != "K" && pos != "P" )
			{
				StaticUtils.AddError(string.Format("Cannot set kick player ablities for {0}.",pos));
				return;
			}
			runningSpeed = GetAbility(runningSpeed);
			rushingPower = GetAbility(rushingPower);
			maxSpeed = GetAbility(maxSpeed);
			hittingPower = GetAbility(hittingPower);
			kickingAbility = GetAbility(kickingAbility);
			avoidKickBlock = GetAbility(avoidKickBlock);

			if( !IsValidAbility(runningSpeed) ||  
				!IsValidAbility(rushingPower) ||  
				!IsValidAbility(maxSpeed)     || 
				!IsValidAbility(hittingPower) || 
				!IsValidAbility(kickingAbility )  || 
				!IsValidAbility(avoidKickBlock)    )
			{
				StaticUtils.AddError(string.Format("Abilities for {0} on {1} were not set.",pos,team));
				PrintValidAbilities();
				return;
			}
			SaveAbilities(team,pos,runningSpeed,rushingPower,maxSpeed,hittingPower,kickingAbility, avoidKickBlock);
		}

		public void SetDefensivePlayerAbilities(string team, 
			string pos, 
			int runningSpeed, 
			int rushingPower, 
			int maxSpeed,
			int hittingPower,
			int passRush,
			int interceptions
			)
		{
			if( !IsValidTeam(team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) team {0} is invalid",team));
				return;
			}

			if(pos != "RE"   && pos != "NT"   && pos != "LE"   && pos != "ROLB" &&
				pos != "RILB" && pos != "LILB" && pos != "LOLB" && pos != "RCB"  && 
				pos != "LCB"  && pos != "SS"   && pos != "FS"   && pos != "DB2"  && pos != "DB1" )
			{
				StaticUtils.AddError(string.Format("Cannot set defensive player ablities for {0}.",pos));
				return;
			}
			runningSpeed = GetAbility(runningSpeed);
			rushingPower = GetAbility(rushingPower);
			maxSpeed = GetAbility(maxSpeed);
			hittingPower = GetAbility(hittingPower);
			passRush = GetAbility(passRush);
			interceptions = GetAbility(interceptions);

			if( !IsValidAbility(runningSpeed) ||  
				!IsValidAbility(rushingPower) ||  
				!IsValidAbility(maxSpeed)     || 
				!IsValidAbility(hittingPower) || 
				!IsValidAbility(passRush )    || 
				!IsValidAbility(interceptions)   )
			{
				StaticUtils.AddError(string.Format("Abilities for {0} on {1} were not set.",pos,team));
				PrintValidAbilities();
				return;
			}
			SaveAbilities(team,pos,runningSpeed,rushingPower,maxSpeed,hittingPower,passRush, interceptions);
		}

		public void SetOLPlayerAbilities(string team, 
			string pos, 
			int runningSpeed, 
			int rushingPower, 
			int maxSpeed,
			int hittingPower )
		{
			if( !IsValidTeam(team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) team {0} is invalid",team));
				return;
			}

			if(pos != "C" && pos != "RG"&& pos != "LG"&& pos != "RT" &&
				pos != "LT" )
			{
				StaticUtils.AddError(string.Format("Cannot set OL player ablities for {0}.",pos));
				return;
			}
			runningSpeed = GetAbility(runningSpeed);
			rushingPower = GetAbility(rushingPower);
			maxSpeed = GetAbility(maxSpeed);
			hittingPower = GetAbility(hittingPower);

			if( !IsValidAbility(runningSpeed) ||  
				!IsValidAbility(rushingPower) ||  
				!IsValidAbility(maxSpeed)     || 
				!IsValidAbility(hittingPower)   )
			{
				StaticUtils.AddError(string.Format("Abilities for {0} on {1} were not set.",pos,team));
				PrintValidAbilities();
				return;
			}//GetAbility
			SaveAbilities(team,pos,runningSpeed,rushingPower,maxSpeed,hittingPower,-1,-1);
		}

		private void SaveAbilities(string team, string pos,
			int runningSpeed, 
			int rushingPower, 
			int maxSpeed,
			int hittingPower,
			int bc,
			int rec)
		{
			if( !IsValidTeam(team)  )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SaveAbilities:: team {0} is invalid",team));
				return;
			}
			else if( !IsValidPosition(pos) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SaveAbilities:: position {0} is invalid",pos));
				return;
			}

			int byte1, byte2, byte3;
			byte1 =(byte)rushingPower;
			byte1 = byte1 << 4;
			byte1 += (byte)runningSpeed;
			byte2 = (byte) maxSpeed;
			byte2 = byte2 << 4;
			byte2 += (byte)hittingPower;
			byte3 = (byte) bc;
			byte3 = byte3 << 4;
			byte3 += (byte)rec;
			// save data here in rom 
			int teamIndex = GetTeamIndex(team);
			int posIndex = GetPositionIndex(pos);
			int location = (teamIndex * teamAbilityOffset)+ abilityOffsets[posIndex] + billsQB1AbilityStart;
			outputRom[location] = (byte)byte1;
			outputRom[location+1] = (byte)byte2;

			if(bc > -1 && rec > -1)
				outputRom[location+3] = (byte)byte3;
		}

        internal bool IsValidAbility(int ab)
        {
            return ab >= 0 && ab <= 0xF;
        }

		private byte GetAbility(int ab)
		{
			byte ret = 0;
			switch(ab)
			{
				case 6:  ret = 0x00; break;
				case 13: ret = 0x01; break;
				case 19: ret = 0x02; break;
				case 25: ret = 0x03; break;
				case 31: ret = 0x04; break;
				case 38: ret = 0x05; break;
				case 44: ret = 0x06; break;
				case 50: ret = 0x07; break;
				case 56: ret = 0x08; break;
				case 63: ret = 0x09; break; 
				case 69: ret = 0x0a; break;
				case 75: ret = 0x0b; break;
				case 81: ret = 0x0c; break;
				case 88: ret = 0x0d; break;
				case 94: ret = 0x0e; break;
				case 100: ret = 0x0f; break;
			}
			return ret;
		}

		private byte MapAbality(int ab)
		{
			/*if(abilityMap.ContainsKey(ab))
				return (byte) abilityMap[ab];
			else
				return 0;*/
			
			byte ret = 0;
			switch(ab)
			{
				case 0x00:  ret = 6; break;
				case 0x01: ret = 13; break;
				case 0x02: ret = 19; break;
				case 0x03: ret = 25; break;
				case 0x04: ret = 31; break;
				case 0x05: ret = 38; break;
				case 0x06: ret = 44; break;
				case 0x07: ret = 50; break;
				case 0x08: ret = 56; break;
				case 0x09: ret = 63; break; 
				case 0x0A: ret = 69; break;
				case 0x0B: ret = 75; break;
				case 0x0C: ret = 81; break;
				case 0x0D: ret = 88; break;
				case 0x0E: ret = 94; break;
				case 0x0F: ret = 100; break;
			}
			return ret;
		}

		/// <summary>
		/// Returns an array of ints mapping to a player's abilities.
		/// Like { 13, 13, 50, 56, 31, 25}. The length of the array returned varies depending
		/// on position.
		/// </summary>
		/// <param name="team">Team name like 'oilers'.</param>
		/// <param name="position">Position name like 'RB4'.</param>
		/// <returns>an array of ints.</returns>
		public int[] GetAbilities(string team, string position)
		{
			if( !IsValidTeam(team)  ||  !IsValidPosition(position) )
			{
				return null;
			}

			int[] ret = {0}; // ret is re-created later.
			int teamIndex = GetTeamIndex(team);
			int posIndex = GetPositionIndex(position);
			int location = (teamIndex * teamAbilityOffset)+ abilityOffsets[posIndex] + billsQB1AbilityStart;
			// wild1 and wild2 map to [receptions and ball control], [pass interceptions and quickness],
			// [kicking ability and avoid kick block]
			int runningSpeed, rushingPower, maxSpeed, hittingPower, wild1, wild2, accuracy, avoidPassBlock;
			int b1,b2,b3,b4; // note 3rd byte maps to the player's face
			b1 = outputRom[location];
			b2 = outputRom[location+1];
			b3 = outputRom[location+3];
			b4 = outputRom[location+4]; // this is only used for qb, but since we are not assigning it here,
			// it doesn't hurt to get it.
			runningSpeed   = b1 & 0x0F; runningSpeed  = MapAbality(runningSpeed);
			rushingPower   = b1 & 0xF0; rushingPower  = MapAbality(rushingPower >> 4);
			maxSpeed       = b2 & 0xF0; maxSpeed      = MapAbality(maxSpeed >> 4);
			hittingPower   = b2 & 0x0F; hittingPower  = MapAbality(hittingPower);
			wild1          = b3 & 0xF0; wild1         = MapAbality(wild1 >> 4);
			wild2          = b3 & 0x0F; wild2         = MapAbality(wild2);
			accuracy       = b4 & 0xF0; accuracy      = MapAbality(accuracy >> 4);
			avoidPassBlock = b4 & 0x0F; avoidPassBlock= MapAbality(avoidPassBlock);
			switch(position)
			{
				case "C":
				case "RG":
				case "LG":
				case "RT":
				case "LT": 
					ret = new int[4];  break;
				case "QB1":
				case "QB2": 
					ret = new int[8]; 
					ret[4] = wild1;
					ret[5] = wild2;
					ret[6] = accuracy;
					ret[7] = avoidPassBlock;
					break;
				default:	
					ret = new int[6]; 
					ret[4] = wild1;
					ret[5] = wild2;
					break;
			}
			ret[0] = runningSpeed;
			ret[1] = rushingPower;
			ret[2] = maxSpeed;
			ret[3] = hittingPower;
			return ret;
		}

		/// <summary>
		/// Returns a string consisting of numbers, spaces and commas.
		/// Like "31, 69, 13, 13, 31, 44"
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public string GetAbilityString(string team, string position)
		{
			if( !IsValidTeam(team)  ||  !IsValidPosition(position) )
			{
				return null;
			}
			int[] abilities = GetAbilities(team,position);
			StringBuilder stuff =new StringBuilder();

			for(int i = 0; i < abilities.Length; i++)
			{
				stuff.Append(abilities[i]);
				stuff.Append(", ");
			}
			stuff.Remove(stuff.Length-2,1);// trim off last comma
			//Console.WriteLine(stuff);
			return stuff.ToString();
		}

		/// <summary>
		/// Returns the simulation data for the given team.
		/// Simulation data is of the form '0xNN' where N is a number 1-F (hex).
		/// A team's sim data of '0x57' signifies that the team has a simulation figure of
		/// '5' for offense, and '7' for defense.
		/// </summary>
		/// <param name="team">The team of interest</param>
		/// <returns></returns>
		public byte GetTeamSimData(string team)
		{
			int teamIndex = GetSimTeamIndex(team);
			if( teamIndex >= 0 )
			{
				int location = teamIndex*teamSimOffset + billsTeamSimLoc;
				return outputRom[location];
			}
			return 0x00;
		}

		/// <summary>
		/// Sets the given team's offense and defense sim values.
		/// Simulation data is of the form '0xNN' where N is a number 1-F (hex).
		/// A team's sim data of '0x57' signifies that the team has a simulation figure of
		/// '5' for offense, and '7' for defense.
		/// </summary>
		/// <param name="team">The team to set.</param>
		/// <param name="values">The value to set it to.</param>
		public void SetTeamSimData(string team, byte values)
		{
			if( !IsValidTeam(team)  )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetTeamSimData:: team {0} is invalid ",team));
				return;
			}

			int teamIndex = GetSimTeamIndex(team);
			int location = teamIndex*teamSimOffset + billsTeamSimLoc;
			//int currentValue = outputRom[location];
			outputRom[location] = values;
			//currentValue = outputRom[location];
		}

		/// <summary>
		/// Sets the team sim offense tendency . 
		/// 00 = Little more rushing, 01 = Heavy Rushing, 
		/// 02 = little more passing, 03 = Heavy Passing. 
		/// </summary>
		/// <param name="team">the team name</param>
		/// <param name="val">the number to set it to.</param>
		/// <returns>true if set, fales if could not set it.</returns>
		public bool SetTeamSimOffensePref(string team, int val)
		{
			int teamIndex = GetTeamIndex(team);
			if( val > -1 && val < 4 && teamIndex != -1)
			{
				int loc = teamSimOffensivePrefStart + teamIndex;
				outputRom[loc] = (byte) val;
			}
			else
			{
				if(teamIndex != -1)
					StaticUtils.AddError(string.Format("Can't set offensive pref to '{0}' valid values are 0-3.\n",val));
				else
					StaticUtils.AddError(string.Format("Team '{0}' is invalid\n",team));
			}
			return true;
		}

		/// <summary>
		/// Sets the team sim offense tendency . 
		/// 00 = Little more rushing, 01 = Heavy Rushing, 
		/// 02 = little more passing, 03 = Heavy Passing. 
		/// </summary>
		/// <param name="team">Teh team name.</param>
		/// <returns>their sim offense pref (0 - 3)</returns>
		public int GetTeamSimOffensePref(string team)
		{
			int teamIndex = GetTeamIndex(team);
			int val = -1;
			if( teamIndex > -1)
			{
				int loc = teamSimOffensivePrefStart + teamIndex;
				val = outputRom[loc];
			}
			else
			{
				StaticUtils.AddError(string.Format("Team '{0}' is invalid\n",team));
			}
			return val;
		}
		
		private const string m2RB_2WR_1TE = "2RB_2WR_1TE";
		private const string m1RB_3WR_1TE = "1RB_3WR_1TE";
		private const string m1RB_4WR     = "1RB_4WR";


		private int mTeamFormationsStartingLoc = 0xedf3;

		/// <summary>
		/// Sets the team's offensive formation.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="formation"></param>
		public void SetTeamOffensiveFormation( string team, string formation)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex > -1 && teamIndex < 255 )
			{
				int location = mTeamFormationsStartingLoc + teamIndex;

				switch( formation )
				{
					case m2RB_2WR_1TE:
						outputRom[location] = (byte)0x00;
						break;
					case m1RB_3WR_1TE:
						outputRom[location] = (byte)0x01;
						break;
					case m1RB_4WR:
						outputRom[location] = (byte)0x02;
						break;
					default:
						StaticUtils.AddError(string.Format(
							"ERROR! Formation {0:x} for team '{1}' is invalid.",formation, team));
						StaticUtils.AddError(string.Format("  Valid formations are:\n  {0}\n  {1}\n  {2}",
							m2RB_2WR_1TE, m1RB_3WR_1TE, m1RB_4WR ));
						break;
				}
			}
			else
			{
				StaticUtils.AddError(string.Format("ERROR! Team '{0}' is invalid, Offensive Formation not set",team));
			}
		}

		/// <summary>
		/// Gets the team's offensive formation.
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public string GetTeamOffensiveFormation(string team)
		{
			int teamIndex = GetTeamIndex( team);
			string ret= "OFFENSIVE_FORMATION = ";
			if( teamIndex > -1 && teamIndex < 255 )
			{
				int location = mTeamFormationsStartingLoc + teamIndex;
				int formation = outputRom[location];

				switch( formation )
				{
					case 0x00:
						ret += m2RB_2WR_1TE;
						break;
					case 0x01:
						ret += m1RB_3WR_1TE;
						break;
					case 0x02:
						ret += m1RB_4WR;
						break;
					default:
						StaticUtils.AddError(string.Format(
							"ERROR! Formation {0:x} for team {1} is invalid, ROM FORMATIONS could be messed up.",formation, team));
						ret="";
						break;
				}
			}
			else
			{
				ret="";
				StaticUtils.AddError(string.Format("ERROR! Team '{0}' is invalid, Offensive Formation get failed.",team));
			}
			return ret;
		}

		private const int mPlaybookStartLoc = 0x170d30;
		/// <summary>
		/// Returns a string like "PLAYBOOK R1, R4, R6, R8, P1, P3, P7, P3"
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public string GetPlaybook(string team)
		{
			string ret = "";
			int rSlot1, rSlot2, rSlot3, rSlot4,
				pSlot1, pSlot2, pSlot3, pSlot4;

			int teamIndex = Index(teams, team);
			if( teamIndex > -1  )
			{
				int pbLocation = mPlaybookStartLoc + (teamIndex * 4);
				rSlot1 = outputRom[pbLocation] >> 4;
				rSlot2 = outputRom[pbLocation] & 0x0f;
				rSlot3 = outputRom[pbLocation+1] >> 4;
				rSlot4 = outputRom[pbLocation+1] & 0x0f;

				pSlot1 = outputRom[pbLocation+2] >> 4;
				pSlot2 = outputRom[pbLocation+2] & 0x0f;
				pSlot3 = outputRom[pbLocation+3] >> 4;
				pSlot4 = outputRom[pbLocation+3] & 0x0f;
				
				ret = string.Format(
					"PLAYBOOK R{0}{1}{2}{3}, P{4}{5}{6}{7} ",
					rSlot1+1, rSlot2+1, rSlot3+1, rSlot4+1,
					pSlot1+1, pSlot2+1, pSlot3+1, pSlot4+1 );
			}
			
			return ret;
		}

		Regex runRegex, passRegex;

		/// <summary>
		/// Sets the team's playbook
		/// </summary>
		/// <param name="runPlays">String like "R1234"</param>
		/// <param name="passPlays">String like "P4567"</param>
		public void SetPlaybook( string team, string runPlays, string passPlays )
		{
			if( runRegex == null || passRegex == null )
			{
				runRegex  = new Regex("R([1-8])([1-8])([1-8])([1-8])");
				passRegex = new Regex("P([1-8])([1-8])([1-8])([1-8])");
			}
			Match runs = runRegex.Match(runPlays);
			Match pass = passRegex.Match(passPlays);

			int r1,r2,r3,r4,p1,p2,p3,p4;

			int teamIndex = Index(teams, team);
			if( teamIndex > -1 && runs != Match.Empty && pass != Match.Empty )
			{
				int pbLocation = mPlaybookStartLoc + (teamIndex * 4);
				r1 = Int32.Parse( runs.Groups[1].ToString()) - 1;
				r2 = Int32.Parse( runs.Groups[2].ToString()) - 1;
				r3 = Int32.Parse( runs.Groups[3].ToString()) - 1;
				r4 = Int32.Parse( runs.Groups[4].ToString()) - 1;

				p1 = Int32.Parse( pass.Groups[1].ToString()) - 1;
				p2 = Int32.Parse( pass.Groups[2].ToString()) - 1;
				p3 = Int32.Parse( pass.Groups[3].ToString()) - 1;
				p4 = Int32.Parse( pass.Groups[4].ToString()) - 1;

				r1 = (r1 << 4) + r2;
				r3 = (r3 << 4) + r4;
				p1 = (p1 << 4) + p2;
				p3 = (p3 << 4) + p4;
				outputRom[pbLocation]   = (byte)r1;
				outputRom[pbLocation+1] = (byte)r3;
				outputRom[pbLocation+2] = (byte)p1;
				outputRom[pbLocation+3] = (byte)p3;
			}
			else
			{
				if( teamIndex < 0 )
					StaticUtils.AddError(string.Format("ERROR! SetPlaybook: Team {0} is Invalid.",team));
				if( runs ==  Match.Empty )
					StaticUtils.AddError(string.Format("ERROR! SetPlaybook Run play definition '{0} 'is Invalid", runPlays));
				if( pass == Match.Empty )
					StaticUtils.AddError(string.Format("ERROR! SetPlaybook Pass play definition '{0} 'is Invalid", passPlays));
			}
		}

		public const int JUICE_LOCATION = 0x2699a;
		private  byte[] m_JuiceArray = 
	   {
		   0, 1, 0, 0, 0,
		   1, 2, 1, 1, 1,
		   1, 2, 1, 2, 2, 
		   1, 2, 1, 3, 2, 
		   2, 2, 2, 3, 3, 
		   2, 2, 2, 4, 3, 
		   2, 2, 2, 4, 4, 
		   2, 2, 2, 5, 4, 
		   2, 2, 3, 5, 5, 
		   2, 2, 3, 6, 5, 
		   2, 2, 4, 6, 6, 
		   3, 2, 4, 7, 6, 
		   3, 3, 4, 7, 7, 
		   3, 3, 5, 8, 7, 
		   3, 3, 5, 8, 8, 
		   3, 3, 5, 9, 8,
		   3, 4, 6, 9, 9
	   };

		public bool ApplyJuice(int week, int amt)
		{
			bool ret = true;
			if( week > 17 || week < 0 || amt > 17 || amt < 0)
			{
				ret = false;
			}
			else
			{
				int rom_location = JUICE_LOCATION + (week * 5);
				int index = (amt - 1 )* 5;
				for(int i = 0; i < 5; i++)
				{
					outputRom[rom_location+i] = m_JuiceArray[index+i];
				}
			}
			return ret;
		}


		public int[] GetPlayerSimData(string team, string pos)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetPlayerSimData:: Invalid team {0}", team));
				return null;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetPlayerSimData:: Invalid Position {0}", pos));
				return null;
			}

			switch(pos)
			{
				case "QB1":  case "QB2": 
					return GetQBSimData(team, pos);
				case "RB1": case "RB2": case "RB3": case "RB4":
				case "WR1": case "WR2": case "WR3": case "WR4":
				case "TE1": case "TE2": 
					return GetSkillSimData(team,pos);
				case "RE":   case "NT":   case "LE":   case "LOLB":
				case "LILB": case "RILB": case "ROLB": case "RCB":
				case "LCB":  case "FS":   case "SS": 
					return GetDefensiveSimData(team, pos);
				case "K":  
					return GetKickingSimData(team);
				case "P":  
					return GetPuntingSimData(team);
				default:
					return null;
			}
		}


		private int[] GetKickingSimData(string team)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetKickingSimData:: Invalid team {0}", team));
				return null;
			}
			int[] ret = new int[1];
			int teamIndex = GetSimTeamIndex(team);
			//QB1 + 0x2E
			int location = teamIndex*teamSimOffset + billsQB1SimLoc + 0x2E;
			ret[0] = outputRom[location] >> 4;
			return ret;
		}

		public void SetKickingSimData(string team, int data)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetKickingSimData:: Invalid team {0}", team));
				return;
			}
			//int teamIndex = GetTeamIndex(team);
			int teamIndex = GetSimTeamIndex(team);
			//QB1 + 0x2E
			int location = teamIndex*teamSimOffset + billsQB1SimLoc + 0x2E;
			int g =  outputRom[location];
			g = g & 0x0F;
			int g2 = data << 4;
			g = g + g2;
			outputRom[location] = (byte)g;
		}

		private int[] GetPuntingSimData(string team)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetPuntingSimData:: Invalid team {0}", team));
				return null;
			}
			int[] ret = new int[1];
			int teamIndex = GetSimTeamIndex(team);
			//QB1 + 0x2E
			int location = teamIndex*teamSimOffset + billsQB1SimLoc + 0x2E;
			ret[0] = outputRom[location] & 0x0F;
			return ret;
		}

		public void SetPuntingSimData(string team, int data)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetPuntingSimData:: Invalid team {0}", team));
				return;
			}
			//int teamIndex = GetTeamIndex(team);
			int teamIndex = GetSimTeamIndex(team);
			//QB1 + 0x2E
			int location = teamIndex*teamSimOffset + billsQB1SimLoc + 0x2E;
			int d = outputRom[location];
			d = d & 0xF0;
			d += data;
			outputRom[location] = (byte)d;
		}

		private int[] GetDefensiveSimData(string team, string pos)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetDefensiveSimData:: Invalid team {0}", team));
				return null;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetDefensiveSimData:: Invalid Position {0}", pos));
				return null;
			}

			int[] ret = new int[2];
			int teamIndex     = GetSimTeamIndex(team);
			int positionIndex = GetPositionIndex(pos);
			//int location = teamIndex*teamSimOffset + (positionIndex*2) +billsQB1SimLoc - 0x0A; // OL-men have no sim data, 2*5=0xA
			int location = teamIndex * teamSimOffset + (positionIndex - 17)+ billsRESimLoc;
			ret[0] = outputRom[location]; //pass rush
			ret[1] = outputRom[location+0xB];// interception ability
			return ret;
		}

		/// <summary>
		/// Sets the simulation data for a defensive player.
		/// </summary>
		/// <param name="team">The team the player belongs to.</param>
		/// <param name="pos">the position he plays.</param>
		/// <param name="data">the data to set it to (length = 2).</param>
		public void SetDefensiveSimData(string team, string pos, int[] data)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetDefensiveSimData:: Invalid team {0}", team));
				return;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetDefensiveSimData:: Invalid Position {0}", pos));
				return;
			}
			else if(data == null || data.Length < 2)
			{
				StaticUtils.AddError(string.Format("Error setting sim data for {0}, {1}. Sim data not set.",team,pos));
				return;
			}
			int teamIndex     = GetSimTeamIndex(team);
			int positionIndex = GetPositionIndex(pos);
			//int location = teamIndex*teamSimOffset + (positionIndex*2) +billsLESimLoc - 0x0A; // OL-men have no sim data, 2*5=0xA
			int location = teamIndex * teamSimOffset + (positionIndex - 17)+ billsRESimLoc;
			byte byte1,byte2;
			byte1 = (byte)data[0];
			byte2= (byte)data[1];

			outputRom[location] = byte1; //pass rush
			outputRom[location+0xB] = byte2;// interception ability
		}


		private int[] GetSkillSimData(string team, string pos)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetSkillSimData:: Invalid team {0}", team));
				return null;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetSkillSimData:: Invalid Position {0}", pos));
				return null;
			}

			int[] ret = new int[4];
			int teamIndex     = GetSimTeamIndex(team);
			int positionIndex = GetPositionIndex(pos);
			int location = teamIndex*teamSimOffset + (positionIndex*2) +billsQB1SimLoc;
			ret[0] = outputRom[location]   >> 4;
			ret[1] = outputRom[location]   &  0x0F;
			ret[2] = outputRom[location+1] >> 4;
			ret[3] = outputRom[location+1] &  0x0F;
			return ret;
		}

		public void SetSkillSimData(string team, string pos, int[] data)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetSkillSimData:: Invalid team {0}", team));
				return;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetSkillSimData:: Invalid Position {0}", pos));
				return;
			}
			else if(data == null || data.Length < 4)
			{
				StaticUtils.AddError(string.Format("Error setting sim data for {0}, {1}. Sim data not set.",team,pos));
				return;
			}

			int teamIndex = GetSimTeamIndex(team);
			int positionIndex = GetPositionIndex(pos);
			int location = teamIndex*teamSimOffset + (positionIndex*2) +billsQB1SimLoc;
			int byte1,byte2;
			byte1 = data[0]<<4;
			byte1 = byte1+data[1];
			byte2 = data[2] << 4;
			byte2 += data[3];
			outputRom[location]  = (byte)byte1;
			outputRom[location+1]= (byte)byte2;
		}

		private int[] GetQBSimData(string team, string pos)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetQBSimData:: Invalid team {0}", team));
				return null;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetQBSimData:: Invalid Position {0}", pos));
				return null;
			}

			int[] ret = new int[3];
			int teamIndex = GetSimTeamIndex(team);
			
			int location = teamIndex*teamSimOffset +billsQB1SimLoc;
			if(pos == "QB2")
				location+=2;
			ret[0] = outputRom[location] >> 4;
			ret[1] = outputRom[location] & 0x0F;
			ret[2] = outputRom[location+1];
			return ret;
		}

		public void SetQBSimData(string team, string pos, int[] data)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetQBSimData:: Invalid team {0}", team));
				return ;
			}
			else if( !IsValidPosition( pos ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetQBSimData:: Invalid Position {0}", pos));
				return ;
			}
			else if(data == null || data.Length < 2)
			{
				StaticUtils.AddError(string.Format("Error setting sim data for {0}, {1}. Sim data not set.",team,pos));
				return;
			}

			int teamIndex = GetSimTeamIndex(team);
			
			int location = teamIndex*teamSimOffset +billsQB1SimLoc;
			if(pos == "QB2")
				location+=2;
			int byte1,byte2;
			byte1 = (byte)data[0] << 4;
			byte1 =  byte1 + (byte)data[1];
			byte2 = (byte)data[2];
			outputRom[location]   = (byte)byte1;
			outputRom[location+1] = (byte)byte2;
		}
		

		/// <summary>
		/// Get the face number from the given team/position
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public int GetFace(string team, string position)
		{
			int positionOffset = GetPositionIndex(position);
			int teamIndex = GetTeamIndex(team);
			if(positionOffset < 0 || teamIndex < 0 )
			{
				StaticUtils.AddError(string.Format("GetFace Error getting face for {0} {1}",team,position));
				return -1;
			}
			int loc = (teamIndex * teamAbilityOffset)+ abilityOffsets[positionOffset] + billsQB1AbilityStart+2;

			//int loc = faceOffsets[positionOffset] + faceTeamOffsets[teamIndex];
			//loc = 0x3012 + faceOffsets[positionOffset] + teamIndex*0x75;
			int ret = outputRom[loc];
			return ret;
		}

		/// <summary>
		/// Sets the face for the guy at position 'position' on team 'team'.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <param name="face"></param>
		public void SetFace(string team, string position, int face)
		{
			int positionOffset = GetPositionIndex(position);
			int teamIndex = GetTeamIndex(team);
			if(positionOffset < 0 || teamIndex < 0 || face < 0x00 | face > 0xD4 )
			{
				StaticUtils.AddError(string.Format("SetFace Error setting face for {0} {1} face={2}",team,position,face));
				if( face < 0x00 | face > 0xD4 )
					StaticUtils.AddError(string.Format("Valid Face numbers are 0x00 - 0xD4"));
				return ;
			}
			int loc = (teamIndex * teamAbilityOffset)+ abilityOffsets[positionOffset] + billsQB1AbilityStart+2;
			int skin =0x80;
			
			if(face < 0x53)
				skin =0x00;

			SetCutSceneRace(teamIndex, positionOffset, face);
			outputRom[loc] = (byte)skin;//(byte)face;
		}

		private const int mRaceCutsceneStartPos = 0x17EF;

		private void SetCutSceneRace(int teamIndex, int positionIndex, int color)
		{
			int pi = (positionIndex/8);
			int romPosition = mRaceCutsceneStartPos + pi  + teamIndex*4;
			byte oldValue = outputRom[romPosition];
			//			byte mask = GetMask(positionIndex, color);
			//			byte newValue = (byte) (oldValue & mask);
			byte newValue = GetNewValue(oldValue, positionIndex, color);
			//			For initial debugging
			//			System.Diagnostics.Debug.Assert(oldValue == newValue, "Race Error!", 
			//				string.Format("teamIndex={0}, positionIndex={1}, color={2}",teamIndex, positionIndex, color));
			outputRom[romPosition] = newValue;
		}

		private byte GetNewValue(byte oldValue, int positionIndex, int race)
		{
			byte mask = 0xFF;
			byte ret = oldValue;
			int bitIndex = positionIndex % 8;
			if( race == 0 )
			{
				mask = GetWhiteMask(positionIndex);
				ret = (byte)(ret & mask);
			}
			else
			{
				mask = GetColorMask(positionIndex);
				ret = (byte)(ret | mask);
			}
			return ret;
		}

		private byte GetWhiteMask(int positionIndex)
		{
			byte ret = 0xFF;
			int bitIndex = positionIndex % 8;
			switch(bitIndex)
			{
				case 0: ret = 0x7F; break;//01111111
				case 1: ret = 0xBF; break;//10111111
				case 2: ret = 0xDF; break;//11011111
				case 3: ret = 0xEF; break;//11101111
				case 4: ret = 0xF7; break;//11110111
				case 5: ret = 0xFB; break;//11111011
				case 6: ret = 0xFD; break;//11111101
				case 7: ret = 0xFE; break;//11111110
			}
			return ret;
		}
		private byte GetColorMask(int positionIndex)
		{
			byte ret = 0x00;
			int bitIndex = positionIndex % 8;
			switch(bitIndex)
			{
				case 0: ret = 0x80; break;//10000000
				case 1: ret = 0x40; break;//01000000
				case 2: ret = 0x20; break;//00100000
				case 3: ret = 0x10; break;//00010000
				case 4: ret = 0x08; break;//00001000
				case 5: ret = 0x04; break;//00000100
				case 6: ret = 0x02; break;//00000010
				case 7: ret = 0x01; break;//00000001
			}
			return ret;
		}

		/// <summary>
		/// Sets the return team for 'team'
		/// </summary>
		/// <param name="team"></param>
		/// <param name="pos0"></param>
		/// <param name="pos1"></param>
		/// <param name="pos2"></param>
		public void SetReturnTeam(string team, string pos0, string pos1, string pos2)
		{
			if( Index(positionNames, pos0) > -1 && 
				Index(positionNames,pos1)  > -1 && 
				Index(positionNames, pos2) > -1    )
			{
				InsertGuyOnReturnTeam(pos0, team, 0);
				InsertGuyOnReturnTeam(pos1, team, 1);
				InsertGuyOnReturnTeam(pos2, team, 2);
			}
			else
			{
				StaticUtils.AddError(string.Format(
					"ERROR! Invalid position on RETURN_TEAM {0} {1} {2}",pos0,pos1,pos2));
			}
		}

		/// <summary>
		/// Returns a string like "RETURN_TEAM WR3, RB3, RCB"
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public string GetReturnTeam(string team)
		{
			string ret = null;
			int teamIndex = Index(teams,team);
			if( teamIndex < 0 )
			{
				StaticUtils.AddError( string.Format("ERROR! GetReturnTeam.Invalid team {0}",team));
			}
			else
			{
				int teamLocation = pr_kr_team_start_offset + (4*teamIndex);
				int pos0, pos1, pos2;

				pos0 = outputRom[teamLocation];
				pos1 = outputRom[teamLocation+1];
				pos2 = outputRom[teamLocation+2];

				// finish this (error checking)
				if( pos0 > -1 && pos0 < positionNames.Length &&
					pos1 > -1 && pos1 < positionNames.Length &&
					pos2 > -1 && pos2 < positionNames.Length )
				{
					ret = string.Format(
						"RETURN_TEAM {0}, {1}, {2}",
						positionNames[pos0] ,positionNames[pos1], positionNames[pos2] );
				}
				else
				{
					StaticUtils.AddError("ERROR! Return Team Messed up in ROM.");
				}
			}
			return ret;
		}

		/// <summary>
		/// Set the punt returner by position.
		/// Hi nibble.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		public void SetPuntReturner(string team, string position)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetPuntReturner:: Invalid team {0}", team));
				return ;
			}
			else if( !IsValidPosition( position ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetPuntReturner:: Invalid Position {0}", position));
				return ;
			}

			int index = IsGuyOnReturnTeam(position, team);
			if( index < 0 )
			{
				index = 1;
				InsertGuyOnReturnTeam(position, team, index);
			}
			
			int teamIndex = Index(teams, team);
			int location = pr_kr_start_offset+ teamIndex;
			int kr_pr = outputRom[location];
			
			kr_pr = kr_pr & 0xf0;
			kr_pr = kr_pr + index;
			outputRom[location] = (byte)kr_pr;
		}

		/// <summary>
		/// Set the kick returner by position.
		/// Lo nibble.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		public void SetKickReturner(string team, string position)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetKickReturner:: Invalid team {0}", team));
				return ;
			}
			else if( !IsValidPosition( position ))
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) SetKickReturner:: Invalid Position {0}", position));
				return ;
			}

			int index = IsGuyOnReturnTeam(position, team);
			if( index < 0 )
			{
				index = 0;
				InsertGuyOnReturnTeam(position, team, index);
			}
			int teamIndex = Index(teams, team);
			int location = pr_kr_start_offset+ teamIndex;
			int kr_pr = outputRom[location];
			kr_pr = kr_pr & 0x0f;
			kr_pr += (index << 4);
			outputRom[location] = (byte)kr_pr;
		}

		/// <summary>
		/// Gets the position who returns punts.
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public string GetKickReturner(string team)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetKickReturner:: Invalid team {0}", team));
				return null;
			}

			string ret ="";
			//int location = 0x328d3 + Index(teams,team);
			int teamIndex = Index(teams, team);
			int returnTeamIndex = outputRom[pr_kr_start_offset + teamIndex ] >> 4;
			int teamLocation = pr_kr_team_start_offset + (4*teamIndex);

			int positionIndex = outputRom[ returnTeamIndex + teamLocation];

			if( positionIndex < positionNames.Length )
			{
				ret = positionNames[positionIndex];
			}
			return ret;
			
			/*
			int b = outputRom[loc1];
			b = b & 0x0F;
			ret = positionNames[b];
			return ret;
			*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="guy">the position name of a guy</param>
		/// <param name="team">the team</param>
		/// <returns></returns>
		private int IsGuyOnReturnTeam(string guy, string team)
		{
			int ret = -1;
			int teamIndex = Index(teams, team);
			int posIndex  = Index(positionNames, guy);
			int teamLocation = pr_kr_team_start_offset + (4*teamIndex);

			if( outputRom[teamLocation] == posIndex )
				ret = 0;
			else if( outputRom[teamLocation+1] == posIndex )
				ret = 1;
			else if( outputRom[teamLocation+2] == posIndex )
				ret = 2;

			return ret;
		}

		private void InsertGuyOnReturnTeam( string position, string team, int index )
		{
			int teamIndex = Index(teams, team);
			int posIndex  = Index(positionNames, position);

			if( index < 0 || index > 2 ||teamIndex < 0 || teamIndex > 27 || posIndex < 0 )
			{
				StaticUtils.AddError(string.Format(
					"InsertGuyOnReturnTeam: invalid arguments {0}, {1}, {2}",position, team, index));
				return ;
			}
			
			int teamLocation = pr_kr_team_start_offset + (4*teamIndex);
			outputRom[teamLocation+index] = (byte)posIndex;
		}

		/// <summary>
		/// Gets the position who returns kicks.
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public string GetPuntReturner(string team)
		{
			if( !IsValidTeam( team) )
			{
				StaticUtils.AddError(string.Format("ERROR! (low level) GetPuntReturner:: Invalid team {0}", team));
				return null;
			}

			string ret ="";
			int teamIndex = Index(teams, team);
			int returnTeamIndex = outputRom[(pr_kr_start_offset + teamIndex )] & 0x0f;
			int teamLocation = pr_kr_team_start_offset + (4*teamIndex);

			int positionIndex = outputRom[ returnTeamIndex + teamLocation];

			if( positionIndex < positionNames.Length )
			{
				ret = positionNames[positionIndex];
			}
			return ret;
			/*
			string ret = "";
			int location = 0x328d3 + Index(teams,team);
			int b = outputRom[location];
			b = b & 0xF0;
			b = b >> 4;
			ret = positionNames[b];
			return ret;
			*/
		}


        int mProwbowlStartingLoc = 0x170C00;

        public void SetProBowlPlayer(Conference conf, String proBowlPos, String fromTeam, TSBPlayer fromTeamPos)
        {
            //NFC => 30*2 + (int) 
            int offset = 0;
            if (conf == Conference.NFC)
                offset += 0x48; //(30 spots * 2 bytes);
            int teamIndex = GetTeamIndex(fromTeam);
            byte ti = (byte)teamIndex;
            byte pi = (byte)fromTeamPos;

            int posIndex = -1;
            switch(proBowlPos)
            {
                case "RET1":
                    posIndex = positionNames.Length;
                    break;
                case "RET2":
                    posIndex = positionNames.Length +1;
                    break;
                case "RET3":
                    posIndex = positionNames.Length +2;
                    break;
                default:
                posIndex = GetPositionIndex(proBowlPos);
                break;
            }
            int loc = mProwbowlStartingLoc + offset + (2 * posIndex);
            
            OutputRom[loc ] = pi;
            OutputRom[loc + 1] = ti;
        }


		public void SetQuarterLength(byte len)
		{
			if( outputRom != null )
			{
				outputRom[ QUARTER_LENGTH ] = len;
			}
		}

		public byte GetQuarterLength()
		{
			byte ret = 0;
			if( outputRom != null )
			{
				ret = outputRom[ QUARTER_LENGTH ];
			}
			return ret;
		}

		private Regex simpleSetRegex;

		public void ApplySet(string line)
		{
			if( simpleSetRegex == null)
				simpleSetRegex = new Regex("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");
			
			if( simpleSetRegex.Match(line) != Match.Empty )
				StaticUtils.ApplySimpleSet(line, this);
            else if (line.IndexOf("PromptUser", StringComparison.OrdinalIgnoreCase) > -1)
            {
                if (line.IndexOf(RomVersion.ToString(), StringComparison.OrdinalIgnoreCase) > -1)
                {
                    // good to go! apply it
                    string simpleSetLine = StringInputDlg.PromptForSetUserInput( line);
                    if( !string.IsNullOrEmpty(simpleSetLine))
                    {
                        ApplySet( simpleSetLine);
                    }
                }
                else
                {
                    //StaticUtils.ShowError("Rom version not specified in Hack: " + line);
                    StaticUtils.AddError(string.Format("line '{0}' not applied; RomVersion = {1}", line, RomVersion.ToString()));
                }
            }
            else
            {
                StaticUtils.AddError(string.Format("ERROR with line \"{0}\"", line));
            }
		}

		private byte[] GetHexBytes(string input)
		{
			if( input == null)
				return null;

			byte[] ret = new byte[input.Length/2];
			string b="";
			int tmp=0;
			int j = 0;

			for(int i =0; i < input.Length; i+=2 )
			{
				b = input.Substring(i,2);
				tmp = Int32.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier);
				ret[j++] = (byte)tmp;
			}
			return ret;
		}
		/// <summary>
		/// Returns the first index of element that occurs in 'array'. returns
		/// -1 if 'element' doesn't occur in 'array'.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="element"></param>
		/// <returns></returns>
		private int Index(string[] array, string element)
		{
			for(int i =0; i < array.Length; i++)
				if(array[i] == element)
					return i;

			return -1;
		}

		private void PrintValidAbilities()
		{
			StaticUtils.AddError(string.Format(
				"Valid player abilities are 6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100"));
		}

		public string StringifyArray(int[] input)
		{
			if( input == null )
				return null;

			StringBuilder sb = new StringBuilder(40);
			for(int i = 0; i < input.Length; i++)
				sb.Append(string.Format("{0}, ",input[i]));
			sb.Remove(sb.Length-2,1); //trim last comma
			return sb.ToString();
		}

		/// <summary>
		/// Returns an ArrayList of errors that were encountered during the operation.
		/// </summary>
		/// <param name="scheduleList"></param>
		/// <returns></returns>
		public virtual void ApplySchedule( ArrayList scheduleList )
		{
			if( scheduleList != null && outputRom != null )
			{
				SNES_ScheduleHelper sch = new SNES_ScheduleHelper( outputRom );
				sch.ApplySchedule( scheduleList );
			}
		}
        
		private int mBillsUniformLoc = 0x2c2e4;// this is for the NES version

		protected int BillsUniformLoc
		{
			get{ return mBillsUniformLoc;}
			set{ mBillsUniformLoc = value;}
		}

		public virtual void SetHomeUniform(string team, string colorString)
		{
			int loc = GetUniformLoc(team);
			if( loc > -1 )
			{
				//				OutputRom[loc]     = pantsColor;
				//				OutputRom[loc + 2] = jerseyColor;
			}
		}

		public virtual void SetAwayUniform(string team, string colorString)
		{
			int loc = GetUniformLoc(team);
			if( loc > -1 )
			{
				//				OutputRom[loc + 3] = pantsColor;
				//				OutputRom[loc + 5] = jerseyColor;
			}
		}


		public virtual string GetHomeUniform(string team)
		{
			string ret = string.Empty;
			int loc = GetUniformLoc(team);
			if( loc > -1 )
			{
				//				ret = string.Format("Home=0x{0:x2}{1:x2}",
				//					OutputRom[loc], 
				//					OutputRom[loc + 2] );
			}
			return ret;
		}

		public virtual string GetAwayUniform(string team)
		{
			string ret = string.Empty;
			int loc = GetUniformLoc(team);
			if( loc > -1 )
			{
				//				ret = string.Format("Away=0x{0:x2}{1:x2}",
				//					OutputRom[loc + 3], 
				//					OutputRom[loc + 5] );
			}
			return ret;
		}

		protected virtual int GetUniformLoc(string team)
		{
			int ret = -1;
			int teamIndex = GetTeamIndex(team);
			if( teamIndex > -1 && teamIndex < 28 )
			{
				ret = BillsUniformLoc + (teamIndex * 0xa);
			}
			return ret;
		}
		
		public virtual string GetGameUniform(string team)
		{
			string ret = string.Empty;
			//			ret = string.Format("{0},{1}", GetHomeUniform(team), GetAwayUniform(team));
			return ret;
		}

		public virtual void SetDivChampColors(string team, string colorString)
		{
		}
		public void SetUniformUsage(string team, string usage)
		{
		}

		public string GetUniformUsage(string team)
		{
			return String.Empty;
		}
		public virtual void SetConfChampColors(string team, string colorString)
		{
		}

		public virtual string GetDivChampColors(string team )
		{
			string ret = string.Empty;
			return ret;
		}

		public virtual string GetConfChampColors(string team )
		{
			string ret = string.Empty;
			return ret;
		}

		public string GetChampColors(string team)
		{
			string ret = string.Empty;
//				string.Format("{0}, {1}",
//				GetDivChampColors(team),
//				GetConfChampColors(team)
//				);
			return ret;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="proBowlPos"></param>
        /// <returns></returns>
        public String GetProBowlPlayer(Conference conf, String proBowlPos)
        {
            String ret = "";
            int offset = 0;
            if (conf == Conference.NFC)
                offset += 0x48; //(30 spots * 2 bytes);

            int posIndex = -1;
            switch (proBowlPos)
            {
                case "RET1":
                    posIndex = positionNames.Length;
                    break;
                case "RET2":
                    posIndex = positionNames.Length + 1;
                    break;
                case "RET3":
                    posIndex = positionNames.Length + 2;
                    break;
                default:
                    posIndex = GetPositionIndex(proBowlPos);
                    break;
            }
            int loc = mProwbowlStartingLoc + offset + (2 * (int)posIndex);

            int teamIndex = OutputRom[loc+1];
            int pos = OutputRom[loc ];

            string team = TecmoTool.Teams[teamIndex];
            ret = String.Format("{0},{1},{2},{3}", conf.ToString(),
                proBowlPos.ToString(), team, ((TSBPlayer)pos).ToString());

            return ret;
        }

        public String GetConferenceProBowlPlayers(Conference conf)
        {
            StringBuilder builder = new StringBuilder(500);
            for (int i = 0; i < positionNames.Length; i++)
            {
                builder.Append(GetProBowlPlayer(conf, positionNames[i]));
                builder.Append("\r\n");
            }
            builder.Append(GetProBowlPlayer(conf, "RET1"));
            builder.Append("\r\n");
            builder.Append(GetProBowlPlayer(conf,"RET2"));
            builder.Append("\r\n");
            builder.Append(GetProBowlPlayer(conf, "RET3"));
            builder.Append("\r\n");

            return builder.ToString();
        }

        public String GetProBowlPlayers()
        {
            StringBuilder builder = new StringBuilder(1000);
            builder.Append("# AFC ProBowl players\r\n");
            builder.Append(GetConferenceProBowlPlayers(Conference.AFC));
            builder.Append("\r\n");

            builder.Append("# NFC ProBowl players\r\n");
            builder.Append(GetConferenceProBowlPlayers(Conference.NFC));
            builder.Append("\r\n");
            return builder.ToString();
        }

		public void ProcessText(string text)
		{
			InputParser parser = new InputParser(this);
			text = text.Replace("\r\n", "\n");
			string[] lines = text.Split("\n".ToCharArray());
			parser.ProcessLines(lines);
		}
	}
}
