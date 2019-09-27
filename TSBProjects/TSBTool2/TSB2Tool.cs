using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool2
{
	// QB attributes (nibbles):
	// RS RP MS HP __ __ PS PC PA AR __ CO BB __

	// RB attributes (nibbles):
	// RS RP MS HP __ __ BC RC BB __
	// Currently found: Player names, player attributes, team stringa
	// TODO:
	// 1. Schedule
	// 2. Sim data
	// 3. Playbooks 
	public class TSB2Tool : ITecmoTool
	{
		public byte[] OutputRom { get;  set; }

		const int BYTES_PER_PLAYER = 5;
		const int BYTES_PER_QB = 6;

		const int NAME_TABLE_SIZE = 0x4798;
		
		int BANK_1_PLAYER_ATTRIBUTES_START = 0x1ec800;
		int BANK_2_PLAYER_ATTRIBUTES_START = 0x1f4800;
		int BANK_3_PLAYER_ATTRIBUTES_START = 0x1fc800;

		// name_string_table_1
		int tsb2_name_string_table_1_first_ptr = 0x1e8038;
		int tsb2_name_string_table_1_offset = 0x1e0000;

		// name string table 2
		int tsb2_name_string_table_2_first_ptr = 0x1f0038;
		int tsb2_name_string_table_2_offset = 0x1e8000;

		// name string table 3
		int tsb2_name_string_table_3_first_ptr = 0x1f8038;
		int tsb2_name_string_table_3_offset = 0x1f0000;

		// Team name String table
		int tsb2_team_name_string_table_first_ptr = 0x7000;
		int tsb2_team_name_string_table_offset    = -32768;
		const int TEAM_NAME_STRING_TABLE_SIZE     = 0x5c0;

		int schedule_start_1992 = 0x16f00c; //data=050D060114130A0C1900
		int schedule_start_1993 = 0x16f204; //data=0300050602010904160B 
		int schedule_start_1994 = 0x16f418; //data=1119181406050E080701 

        int team_sim_start_1992 = 0x1EE000;
        int team_sim_start_1993 = 0x1F6000;
        int team_sim_start_1994 = 0x1FE000;

        const int team_sim_size = 102;

		public static List<string> positionNames = new List<string>(){ 
		  "QB1", "QB2", "RB1", "RB2",  "RB3",  "RB4",  "WR1",  "WR2", "WR3", "WR4", "TE1", 
		  "TE2", "C",   "LG",  "RG",   "LT",   "RT",
		  "RE",  "NT",  "LE",  "RE2",  "NT2",  "LE2",
		  "ROLB", "RILB", "LILB", "LOLB", "LB5",
		  "RCB", "LCB",  "DB1", "DB2",  "FS",  "SS", "DB3",
		  "K",   "P" 
		};

		public static List<string> teams = new List<string> (){
			"bills",   "colts",  "dolphins", "patriots",  "jets",
			"bengals", "browns", "oilers",   "steelers",
			"broncos", "chiefs", "raiders",  "chargers",  "seahawks",
			"cowboys", "giants", "eagles",   "cardinals", "redskins",
			"bears",   "lions",  "packers",  "vikings",   "buccaneers",
			"falcons", "rams",   "saints",   "49ers"
		};

		public TSB2Tool(byte[] rom)
		{
			this.OutputRom = rom;
		}

		public bool Init(string fileName)
		{
			OutputRom = StaticUtils.ReadRom(fileName);
			return OutputRom != null;
		}
		public string RomVersion { get { return "SNES_TSB2"; } }

		// use this instead of directoy setting data in OutputRom
		public void SetByte(int location, byte b) { this.OutputRom[location] = b; }

		public bool IsValidPosition(string pos) { return positionNames.IndexOf(pos) > -1; }

		private int[] eighteenGames = new int[] { 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14 };
		private int[] seventeenGames = new int[] { 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14 };
		public string GetSchedule(int season)
		{
			SNES_ScheduleHelper helper = new SNES_ScheduleHelper(this.OutputRom);
			switch (season)
			{
				case 1: helper.SetWeekOneLocation(schedule_start_1992, seventeenGames); break;
				case 2: helper.SetWeekOneLocation(schedule_start_1993, eighteenGames); break;
				case 3: helper.SetWeekOneLocation(schedule_start_1994, seventeenGames); break;
			}
			return helper.GetSchedule();
		}

		private int GetPlayerIndex(string team, string position)
		{
			int teamIndex = teams.IndexOf(team);
			int positionIndex = positionNames.IndexOf(position);
			int retVal = teamIndex * positionNames.Count + positionIndex;
			return retVal;
		}

		private int GetPlayerAttributeLocation(int season, string team, string position)
		{
			int retVal = -1;
			int attributeStart = BANK_3_PLAYER_ATTRIBUTES_START;
			switch (season)
			{
				case 1: attributeStart = BANK_1_PLAYER_ATTRIBUTES_START; break; 
				case 2: attributeStart = BANK_2_PLAYER_ATTRIBUTES_START; break; 
			}
			int teamByteSize = 0xb9;
			int teamIndex = teams.IndexOf(team);
			int positionIndex = positionNames.IndexOf(position);
			int teamStart = attributeStart + (teamIndex * teamByteSize);
			switch (positionIndex)
			{
				case 0: retVal = teamStart; break;
				case 1: retVal = teamStart + 7; break;
				case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9: case 10: case 11: // skill players
					retVal = teamStart + BYTES_PER_PLAYER * positionIndex + 4;//(2 extra bytes for qbs)
					break;
				// Offensive linemen
				case 12: retVal = teamStart + 0x40; break;
				case 13: retVal = teamStart + 0x44; break;
				case 14: retVal = teamStart + 0x48; break;
				case 15: retVal = teamStart + 0x4c; break;
				case 16: retVal = teamStart + 0x50; break;
				case 36: retVal = teamStart + 0xb4; break;// punter 
				// Defense + Kicker
				//case 17: retVal = teamStart + 0x54; break;
				default:
					retVal = teamStart + 0x54 + 5 * (positionIndex - 17);
					break;
			}

			return retVal;
		}
		#region Get specific Attributes

		private string GetQBAbilities(int season, // (season = 1-3)
			string team, string position)
		{
			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs = StaticUtils.GetFirstNibble( OutputRom[location]);
			byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
			byte ms = StaticUtils.GetFirstNibble( OutputRom[location+1]);
			byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
			byte ps = StaticUtils.GetFirstNibble( OutputRom[location + 3]);
			byte pc = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
			byte pa = StaticUtils.GetFirstNibble( OutputRom[location + 4]);
			byte ar = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
			byte co = StaticUtils.GetSecondNibble(OutputRom[location + 5]);
			byte bb = StaticUtils.GetFirstNibble( OutputRom[location + 6]);

			byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ps, pc, pa, ar, co };
			string retVal = StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} ", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5], OutputRom[location + 6]);
			return retVal;
		}

		public void SetQBAbilities(int season,
			string team, string qb,
			byte runningSpeed, byte rushingPower, byte maxSpeed, byte hittingPower,
			byte bodyBalance,
			byte passingSpeed, byte passControl, byte accuracy,
			byte avoidRush, byte coolness)
		{
			StaticUtils.CheckTSB2Args(season, team);
			if (qb != "QB1" && qb != "QB2") throw new ArgumentException("Invalid qb position " + qb);

			int location = GetPlayerAttributeLocation(season, team, qb);
			byte rs_rp = StaticUtils.CombineNibbles(runningSpeed, rushingPower);
			byte ms_hp = StaticUtils.CombineNibbles(maxSpeed, hittingPower);
			byte ps_pc = StaticUtils.CombineNibbles(passingSpeed, passControl);
			byte pa_ar = StaticUtils.CombineNibbles(accuracy,  avoidRush);
			byte unk1 = StaticUtils.GetFirstNibble(OutputRom[location + 5]);
			byte unk1_co = StaticUtils.CombineNibbles(unk1, coolness);
			byte unk2 = StaticUtils.GetSecondNibble(OutputRom[location + 6]);
			byte bb_unk2 = StaticUtils.CombineNibbles(bodyBalance, unk2);
			SetByte(location, rs_rp);
			SetByte(location + 1, ms_hp);
			SetByte(location + 3, ps_pc);
			SetByte(location + 4, pa_ar);
			SetByte(location + 5, unk1_co);
			SetByte(location + 6, bb_unk2);
		}

		private string GetOLPlayerAbilities(int season, // (season = 1-3)
			string team, string position)
		{
			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
			byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
			byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
			byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
			byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 3]);

			byte[] attrs = new byte[] { rs, rp, ms, hp, bb };
			string retVal = StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3]);
			return retVal;
		}

		public void SetOLPlayerAbilities(int season,  string team,  string pos, 
			byte  runningSpeed,  byte  rushingPower, 
			byte  maxSpeed, byte  hittingPower,
			byte  bodyBalance )
		{
			StaticUtils.CheckTSB2Args(season, team);
			int posIndex = positionNames.IndexOf(pos);
			if (posIndex < 12 || posIndex > 16) throw new ArgumentException("Invalid position argument! (takes C,RG,RT,LG,LT) " + pos);

			int location = GetPlayerAttributeLocation(season, team, pos);
			byte rs_rp = StaticUtils.CombineNibbles(runningSpeed, rushingPower);
			byte ms_hp = StaticUtils.CombineNibbles(maxSpeed, hittingPower);
			byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
			byte bb_unk1 = StaticUtils.CombineNibbles(bodyBalance, unk1);
			SetByte(location, rs_rp);
			SetByte(location + 1, ms_hp);
			SetByte(location + 3, bb_unk1);
		}

		private string GetKickerAbilities(int season, // (season = 1-3)
			string team, string position)
		{
			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
			byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
			byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
			byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
			byte kp = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
			byte ka = StaticUtils.GetFirstNibble(OutputRom[location + 4]);
			byte ab = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
			byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 5]);

			byte[] attrs = new byte[] { rs, rp, ms, hp, bb, kp, ka, ab };
			string retVal = StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5]);
			return retVal;
		}

		public void SetKickerAbilities(int season, string team, string position,
			byte  runningSpeed, byte  rushingPower,
			byte  maxSpeed, byte  hittingPower,
			byte  bodyBalance, 
			byte  kickingPower,
			byte  kickingAccuracy, byte  avoidBlock )
		{
			StaticUtils.CheckTSB2Args(season, team);
			if (position != "K") throw new ArgumentException("Invalid position argument! (takes K) " + position);

			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs_rp = StaticUtils.CombineNibbles(runningSpeed, rushingPower);
			byte ms_hp = StaticUtils.CombineNibbles(maxSpeed, hittingPower);
			byte unk1 = StaticUtils.GetFirstNibble(OutputRom[location + 3]);
			byte unk1_kp = StaticUtils.CombineNibbles(unk1, kickingPower);
			byte ka_ab = StaticUtils.CombineNibbles(kickingAccuracy, avoidBlock);
			byte unk2 = StaticUtils.GetSecondNibble(OutputRom[location + 5]);
			byte bb_unk2 = StaticUtils.CombineNibbles(bodyBalance, unk2);

			SetByte(location, rs_rp);
			SetByte(location + 1, ms_hp);
			SetByte(location + 3, unk1_kp);
			SetByte(location + 4, ka_ab);
			SetByte(location + 5, bb_unk2);
		}

		private string GetPunterAbilities(int season, // (season = 1-3)
			string team, string position)
		{
			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
			byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
			byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
			byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
			byte kp = StaticUtils.GetFirstNibble(OutputRom[location + 3]);
			byte ab = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
			byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 4]);

			byte[] attrs = new byte[] { rs, rp, ms, hp, bb, kp, ab };
			string retVal = StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4]);
			return retVal;
		}

		public void SetPunterAbilities(int season, string team, string position,
			byte  runningSpeed, byte  rushingPower,
			byte  maxSpeed, byte  hittingPower,
			byte  bodyBalance,
			byte  kickingPower, byte  avoidBlock )
		{
			StaticUtils.CheckTSB2Args(season, team);
			if (position != "P") throw new ArgumentException("Invalid position argument! (takes P) " + position);

			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs_rp = StaticUtils.CombineNibbles(runningSpeed, rushingPower);
			byte ms_hp = StaticUtils.CombineNibbles(maxSpeed, hittingPower);
			byte kp_ab = StaticUtils.CombineNibbles(kickingPower, avoidBlock);
			byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
			byte bb_unk1 = StaticUtils.CombineNibbles(bodyBalance, unk1);

			SetByte(location, rs_rp);
			SetByte(location + 1, ms_hp);
			SetByte(location + 3, kp_ab);
			SetByte(location + 4, bb_unk1);
		}
		#endregion

		public string GetPlayerAbilities(int season, // (season = 1-3)
			string team, string position)
		{
			switch (position)
			{
				case "QB1": case "QB2":
					return GetQBAbilities(season, team, position);
				case "C":  case "RG":  case "LG": case "RT": case "LT":
					return GetOLPlayerAbilities(season, team, position);
				case "K": 
					return GetKickerAbilities(season, team, position);
				case "P":
					return GetPunterAbilities(season, team, position);
			}
			// RB attributes (nibbles):
			// RS RP MS HP __ __ BC RC BB __
			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
			byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
			byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
			byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
			
			byte bc = StaticUtils.GetFirstNibble(OutputRom[location + 3]);
			byte rec = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
			byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 4]);

			byte[] attrs = new byte[] { rs, rp, ms, hp, bb, bc, rec };
			string retVal = StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4]);
			return retVal;
		}

		public void SetSkillPlayerAbilities(int season, // (season = 1-3)
			string team, string position,
			byte runningSpeed, byte rushingPower,
			byte maxSpeed, byte hittingPower,
			byte bodyBalance,
			byte ballControl, byte receptions)
		{
			StaticUtils.CheckTSB2Args(season, team);
			int posIndex = positionNames.IndexOf(position); //2-11
			if (posIndex < 2 || posIndex > 11) throw new ArgumentException("Invalid position argument! (takes RB1=TE2)" + position);

			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs_rp = StaticUtils.CombineNibbles(runningSpeed, rushingPower);
			byte ms_hp = StaticUtils.CombineNibbles(maxSpeed, hittingPower);
			byte bc_rec = StaticUtils.CombineNibbles(ballControl, receptions);
			byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
			byte bb_unk1 = StaticUtils.CombineNibbles(bodyBalance, unk1); 
			SetByte(location, rs_rp);
			SetByte(location + 1, ms_hp);
			SetByte(location + 3, bc_rec);
			SetByte(location + 4, bb_unk1);
		}

		public void SetDefensivePlayerAbilities(int season, string team, string position,
			byte runningSpeed, byte rushingPower,
			byte maxSpeed, byte hittingPower,
			byte bodyBalance,
            byte interceptions, byte quickness)
		{
			StaticUtils.CheckTSB2Args(season, team);
			int posIndex = positionNames.IndexOf(position); //17-34
			if (posIndex < 17 || posIndex > 34) throw new ArgumentException("Invalid position argument! (takes RE-DB3)" + position);

			int location = GetPlayerAttributeLocation(season, team, position);
			byte rs_rp = StaticUtils.CombineNibbles(runningSpeed, rushingPower);
			byte ms_hp = StaticUtils.CombineNibbles(maxSpeed, hittingPower);
			byte pi_qu = StaticUtils.CombineNibbles(interceptions, quickness);
			byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
			byte bb_unk1 = StaticUtils.CombineNibbles(bodyBalance, unk1);
			SetByte(location, rs_rp);
			SetByte(location + 1, ms_hp);
			SetByte(location + 3, pi_qu);
			SetByte(location + 4, bb_unk1);
		}

		// called 'GetFace()' for historical purpose, first nibble is race (0=white, 8=black); not sure what the second nibble is
		private byte GetFace(int season,
			string team, string position)
		{
			int location = GetPlayerAttributeLocation(season, team, position) + 2;
			byte retVal = OutputRom[location];
			return retVal;
		}

		public virtual void SetFace(int season, string team, string position, int face)
		{
			StaticUtils.CheckTSB2Args(season, team, position);
			int location = GetPlayerAttributeLocation(season, team, position) + 2;
			SetByte(location, (byte)face);
		}

		public string GetPlayerName(int season, string team, string position, out byte playerNumber)
		{
			string retVal = "fakeNEWS!!!";
			int first_ptr = tsb2_name_string_table_1_first_ptr;
			int offset = tsb2_name_string_table_1_offset;
			switch (season)
			{
				case 2:
					first_ptr = tsb2_name_string_table_2_first_ptr;
					offset = tsb2_name_string_table_2_offset;
					break;
				case 3:
					first_ptr = tsb2_name_string_table_3_first_ptr;
					offset = tsb2_name_string_table_3_offset;
					break;
			}
			int playerIndex = GetPlayerIndex(team, position);
			string name = StaticUtils.GetStringTableString(OutputRom, playerIndex, first_ptr, offset);
			playerNumber = (byte)name[0];
            if ((char)playerNumber == '*') // tricky little trick; we replace the 'null' byte with a '*' and now have to check for it.
                playerNumber = 0;
			name = name.Substring(1); // trim off player number
			for (int i = 0; i < name.Length; i++)
				if (Char.IsUpper(name[i]))
				{
					retVal = name.Substring(0, i) + " " + name.Substring(i);
					break;
				}
			return retVal;
		}

		public void InsertPlayerName(int season, string currentTeam, string position, string fname, string lname, byte jerseyNumber)
		{
			int first_ptr = tsb2_name_string_table_1_first_ptr;
			int offset = tsb2_name_string_table_1_offset;
			switch (season)
			{
				case 2:
					first_ptr = tsb2_name_string_table_2_first_ptr;
					offset = tsb2_name_string_table_2_offset;
					break;
				case 3:
					first_ptr = tsb2_name_string_table_3_first_ptr;
					offset = tsb2_name_string_table_3_offset;
					break;
			}
			int playerIndex = GetPlayerIndex(currentTeam, position);
			string insertThis = (char)jerseyNumber + fname.ToLower() + lname.ToUpper();
			int stringsInTable = teams.Count * positionNames.Count;
			StaticUtils.SetStringTableString(OutputRom, playerIndex, insertThis, first_ptr, offset, stringsInTable, NAME_TABLE_SIZE);
		}

		public string GetPlayerStuff(int season)
		{
			StringBuilder builder = new StringBuilder();
			string team = "";
			for(int i=0; i< teams.Count; i++)
			{
				team = teams[i];
				builder.Append("TEAM = ");
				builder.Append(team);
                builder.Append(",");
                builder.Append(GetTeamSimData(season, team));
				builder.Append("\n");
				builder.Append(String.Format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n",
					GetTeamAbbreviation(i), GetTeamCity(i), GetTeamName(i) ));
				builder.Append(GetTeamPlayers(season, team));
			}
			string retVal = builder.ToString();
			return retVal;
		}

        public string GetTeamSimData(int season, string team)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int index = teams.IndexOf(team);
            int location = index * team_sim_size;
            switch (season)
            {
                case 1: location += team_sim_start_1992; break;
                case 2: location += team_sim_start_1993; break;
                case 3: location += team_sim_start_1994; break;
            }
            StringBuilder retVal = new StringBuilder(210);
            retVal.Append("SimData=0x");
            for (index = 0; index < team_sim_size; index++)
            {
                retVal.Append(OutputRom[location + index].ToString("x2"));
            }
            return retVal.ToString();
        }

        public void SetTeamSimData(int season, string team, string data)
        {
            StaticUtils.CheckTSB2Args(season, team);
            byte[] theBytes = StaticUtils.GetHexBytes(data);
            if (theBytes.Length == team_sim_size)
            {
                int index = teams.IndexOf(team);
                int location = index * team_sim_size;
                
                switch (season)
                {
                    case 1: location += team_sim_start_1992; break;
                    case 2: location += team_sim_start_1993; break;
                    case 3: location += team_sim_start_1994; break;
                }
                for (int i = 0; i < theBytes.Length; i++)
                {
                    OutputRom[i + location] = theBytes[i];
                }
            }
        }

		public string GetTeamPlayers(int season, string team)
		{
			StaticUtils.CheckTSB2Args(season, team);
			StringBuilder builder = new StringBuilder();
			byte playerNumber = 0;
			byte face = 0;

			foreach (string position in positionNames)
			{
				builder.Append(position);
				builder.Append(",");
				builder.Append(GetPlayerName(season, team, position, out playerNumber));
				builder.Append(",");
				face = GetFace(season, team, position);
				builder.Append(String.Format("Face=0x{0:x2},#{1:x},", face, playerNumber));
				builder.Append(GetPlayerAbilities(season, team, position));
				builder.Append("\n");
			}
			builder.Append("\n");

			string retVal = builder.ToString();
			return retVal;
		}

		public virtual string GetTeamName(int teamIndex)
		{
			string retVal = StaticUtils.GetStringTableString(OutputRom, teamIndex, tsb2_team_name_string_table_first_ptr,
				tsb2_team_name_string_table_offset);
			int lastSpace = retVal.LastIndexOf(' ');
			retVal = retVal.Substring(lastSpace + 1).Replace("*", "");
			//retVal = retVal.Substring(0, retVal.Length - 1);// trim off trailing null byte
			return retVal;
		}

		public virtual string GetTeamCity(int teamIndex)
		{
			string retVal = StaticUtils.GetStringTableString(OutputRom,teamIndex, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset).Substring(5);
			int lastSpace = retVal.LastIndexOf(' ');
			retVal = retVal.Substring(0, lastSpace);
			return retVal;
		}

		public virtual string GetTeamAbbreviation(int teamIndex)
		{
			string retVal = StaticUtils.GetStringTableString(OutputRom,teamIndex, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset);
			retVal = retVal.Substring(0, 4);
			return retVal;
		}

		public virtual void SetTeamAbbreviation(int teamIndex, string abb)
		{
			string teamString = String.Format("{0}*{1} {2}*", abb, GetTeamCity(teamIndex), GetTeamName(teamIndex));
			StaticUtils.SetStringTableString(OutputRom, teamIndex, teamString, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset, 30, TEAM_NAME_STRING_TABLE_SIZE);
		}

		public virtual void SetTeamName(int teamIndex, string name)
		{
			string teamString = String.Format("{0}*{1} {2}*", GetTeamAbbreviation(teamIndex), GetTeamCity(teamIndex), name);
			StaticUtils.SetStringTableString(OutputRom, teamIndex, teamString, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset, 30, TEAM_NAME_STRING_TABLE_SIZE);
		}

		public virtual void SetTeamCity(int teamIndex, string city)
		{
			string teamString = String.Format("{0}*{1} {2}*", GetTeamAbbreviation(teamIndex), city, GetTeamName(teamIndex));
			StaticUtils.SetStringTableString(OutputRom, teamIndex, teamString, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset, 30, TEAM_NAME_STRING_TABLE_SIZE);
		}

		public static int GetTeamIndex(string team) { return teams.IndexOf(team); }
		public static string GetTeamFromIndex(int i) { return teams[i]; }


		public void SetYear(string year)
		{
			//TODO
		}


		public string GetKey()
		{
			return
@"# Key 
# Team SimData is unknown 
# 'SET' commands are supported
# Attribute Order
# QBs   RS RP MS HP BB PS PC PA AR CO 
# Skill RS RP MS HP BB BC RC
# OL    RS RP MS HP BB
# DEF   RS RP MS HP BB PI QU 
# K     RS RP MS HP BB KP KA AB
# P     RS RP MS HP BB KP AB 
";
		}

		public string GetAll()
		{
			StringBuilder builder = new StringBuilder(10000);
			builder.Append("SEASON 1\n");
			builder.Append(GetPlayerStuff(1));
			builder.Append(GetSchedule(1));

			builder.Append("SEASON 2\n");
			builder.Append(GetPlayerStuff(2));
			builder.Append(GetSchedule(2));

			builder.Append("SEASON 3\n");
			builder.Append(GetPlayerStuff(3));
			builder.Append(GetSchedule(3));
			return builder.ToString();
		}

		public string GetAll(int season)
		{
			StringBuilder builder = new StringBuilder(5000);
			builder.Append("SEASON ");
			builder.Append(season);
			builder.Append("\n");
			builder.Append(GetPlayerStuff(season));
			builder.Append(GetSchedule(season));
			return builder.ToString();
		}

		public void SetKickReturner(int season, string team, string position)
		{
			//TODO
		}

		public void SetPuntReturner(int season, string team, string position)
		{
			//TODO
		}

		public void ApplySchedule(int season, List<string> scheduleList)
		{
			//TODO
		}

		public void ApplySet(string line)
		{
			StaticUtils.ApplySet(line, this);
		}

		public void ProcessText(string text)
		{
			InputParser parser = new InputParser(this);
			parser.ProcessText(text);
		}

	}
}
