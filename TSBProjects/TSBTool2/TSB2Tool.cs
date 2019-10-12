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
    // 4. NFL Records ?
    public class TSB2Tool : ITecmoTool
    {
        public static bool IsTecmoSuperBowl2Rom(byte[] rom)
        {
            bool retVal = false;
            if (rom != null && rom.Length > 0)
            {
                List<long> results = StaticUtils.FindStringInFile("CONSECUTIVE", rom, 0x1be4e0, 0x1bef20);
                if (results.Count > 0)
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        public byte[] OutputRom { get; set; }

        public static bool ShowPlayerSimData = true;
        public static bool ShowPlaybooks = true;
        public static bool ShowSchedule = true;
        public static bool ShowProBowlRosters = true;

        #region Special Locations SNES TSB2
        //https://tecmobowl.org/forums/topic/53028-tecmo-super-bowl-ii-hackingresource-documentation/
        const int BYTES_PER_PLAYER = 5;
        protected int BYTES_PER_QB = 6;

        const int NAME_TABLE_SIZE = 0x4798;

        const int BANK_1_PLAYER_ATTRIBUTES_START = 0x1ec800;
        const int BANK_2_PLAYER_ATTRIBUTES_START = 0x1f4800;
        const int BANK_3_PLAYER_ATTRIBUTES_START = 0x1fc800;

        // name_string_table_1
        protected int tsb2_name_string_table_1_first_ptr = 0x1e8038;
        const int tsb2_name_string_table_1_offset = 0x1e0000;

        // name string table 2
        const int tsb2_name_string_table_2_first_ptr = 0x1f0038;
        const int tsb2_name_string_table_2_offset = 0x1e8000;

        // name string table 3
        const int tsb2_name_string_table_3_first_ptr = 0x1f8038;
        const int tsb2_name_string_table_3_offset = 0x1f0000;

        // Team name String table
        const int tsb2_team_name_string_table_first_ptr = 0x7000;
        const int tsb2_team_name_string_table_offset = -32768;
        const int TEAM_NAME_STRING_TABLE_SIZE = 0x5C0;

        protected const int schedule_start_season_1 = 0x16F00C; //data=050D060114130A0C1900
        const int schedule_start_season_2 = 0x16F204; //data=0300050602010904160B 
        const int schedule_start_season_3 = 0x16F418; //data=1119181406050E080701 

        const int team_sim_start_season_1 = 0x1EE000;  // pointers
        const int team_sim_start_season_2 = 0x1F6000;
        const int team_sim_start_season_3 = 0x1FE000;

        const int team_sim_size = 102;

        protected int bills_kr_loc_season_1 = 0xE51DA; // 0xE50AA; // TSB3
        const int bills_kr_loc_season_2 = 0xE5216;
        const int bills_kr_loc_season_3 = 0xE5252;
        // bills_kr_loc = 0xe50AB 
        // colts_kr_loc = 0xe50AC 
        // colts_kr_loc = 0xe50AD

        // Playbooks 
        /*  The order goes  * From Knobbe 
            28 teams from 92  : 0xE5D27
            Pro Bowl Teams    :
            28 teams from 93  :
            28 teams from 94  :
         */
        const int playbook_team_size = 8;// bytes, 1 nibble per play, 16 total plays; League size = E0
        protected int[] playbook_start = { 
            0xE5D26,// 1992 
            0xE5E16,
            0xE5F06
        };
        
        const int pro_bowl_ptr_location_season_1 = 0xE5000; // season 1
        const int pro_bowl_ptr_location_season_2 = 0xE5002; // season 1
        const int pro_bowl_ptr_location_season_3 = 0xE5004; // season 1
        
        protected int pro_bowl_playbook = 0x5E07;
        #endregion

        public static List<string> positionNames = new List<string>(){ 
		  "QB1", "QB2", "RB1", "RB2",  "RB3",  "RB4",  "WR1",  "WR2", "WR3", "WR4", "TE1", "TE2", 
          "C",   "LG",  "RG",   "LT",   "RT",
		  "RE",  "NT",  "LE",  "RE2",  "NT2",  "LE2",
		  "ROLB", "RILB", "LILB", "LOLB", "LB5",
		  "RCB", "LCB",  "DB1", "DB2",  "FS",  "SS", "DB3",
		  "K",   "P" 
		};

        public static List<string> teams = new List<string>(){
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
            Init();
        }
        public TSB2Tool()
        {
            Init();
        }

        private void Init()
        {
            BYTES_PER_QB = 6;
            // name_string_table_1
            tsb2_name_string_table_1_first_ptr = 0x1e8038;
            
            teams = new List<string>(){
			    "bills",   "colts",  "dolphins", "patriots",  "jets",
			    "bengals", "browns", "oilers",   "steelers",
			    "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
			    "cowboys", "giants", "eagles",   "cardinals", "redskins",
			    "bears",   "lions",  "packers",  "vikings",   "buccaneers",
			    "falcons", "rams",   "saints",   "49ers"
		    };
        }

        public virtual string RomVersion { get { return "SNES_TSB2"; } }

        // use this instead of directoy setting data in OutputRom
        public void SetByte(int location, byte b) 
        { 
            this.OutputRom[location] = b; 
        }

        public bool IsValidPosition(string pos) { return positionNames.IndexOf(pos) > -1; }

        private int[] eighteenWeeks = new int[] { 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14 };
        private int[] seventeenWeeks = new int[] { 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14 };
        public virtual string GetSchedule(int season)
        {
            SNES_ScheduleHelper helper = new SNES_ScheduleHelper(this);
            switch (season)
            {
                case 1: helper.SetWeekOneLocation(schedule_start_season_1, seventeenWeeks, teams); break;
                case 2: helper.SetWeekOneLocation(schedule_start_season_2, eighteenWeeks, teams); break;
                case 3: helper.SetWeekOneLocation(schedule_start_season_3, seventeenWeeks, teams); break;
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

        protected virtual int GetPlayerAttributeLocation(int season, string team, string position)
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
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11: // skill players
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

        protected virtual string GetQBAbilities(int season, // (season = 1-3)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            byte ps = StaticUtils.GetFirstNibble(OutputRom[location + 3]);
            byte pc = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
            byte pa = StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte ar = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte co = StaticUtils.GetSecondNibble(OutputRom[location + 5]);
            byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 6]);
            byte sp = StaticUtils.GetSecondNibble(OutputRom[location + 6]); // sim pocket??

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ps, pc, pa, ar, co };
            string retVal = StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} ", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5], OutputRom[location + 6]);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(season, team, position);
                retVal += String.Format("[{0:X2},{1:X2},{2:X2}]", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2]);
            }
            return retVal;
        }

        public virtual void SetQBAbilities(int season,
            string team, string qb, byte[] abilities)
            //byte runningSpeed, byte rushingPower, 
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance,
            //byte passingSpeed, byte passControl, 
            //byte accuracy, byte avoidRush, 
            //byte coolness)
        {
            StaticUtils.CheckTSB2Args(season, team);
            if (qb != "QB1" && qb != "QB2") throw new ArgumentException("Invalid qb position " + qb);

            int location = GetPlayerAttributeLocation(season, team, qb);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte ps_pc = StaticUtils.CombineNibbles(abilities[5], abilities[6]);
            byte pa_ar = StaticUtils.CombineNibbles(abilities[7], abilities[8]);
            byte unk1 = StaticUtils.GetFirstNibble(OutputRom[location + 5]);
            byte unk1_co = StaticUtils.CombineNibbles(unk1, abilities[9]);
            byte unk2 = StaticUtils.GetSecondNibble(OutputRom[location + 6]);
            byte bb_unk2 = StaticUtils.CombineNibbles(abilities[4], unk2);
            SetByte(location, rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 3, ps_pc);
            SetByte(location + 4, pa_ar);
            SetByte(location + 5, unk1_co);
            SetByte(location + 6, bb_unk2);
        }

        protected virtual string GetOLPlayerAbilities(int season, // (season = 1-3)
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

        public virtual void SetOLPlayerAbilities(int season, string team, string pos, byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(pos);
            if (posIndex < 12 || posIndex > 16) throw new ArgumentException("Invalid position argument! (takes C,RG,RT,LG,LT) " + pos);

            int location = GetPlayerAttributeLocation(season, team, pos);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 3]);
            byte bb_unk1 = StaticUtils.CombineNibbles(abilities[4], unk1);
            SetByte(location, rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 3, bb_unk1);
        }

        protected virtual string GetKickerAbilities(int season, // (season = 1-3)
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
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(season, team, position);
                retVal += String.Format("[{0:X}]", (OutputRom[location] >> 4));
            }
            return retVal;
        }

        public virtual void SetKickerAbilities(int season, string team, string position,byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance,
            //byte kickingPower,
            //byte kickingAccuracy, byte avoidBlock)
        {
            StaticUtils.CheckTSB2Args(season, team);
            if (position != "K") throw new ArgumentException("Invalid position argument! (takes K) " + position);

            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte unk1 = StaticUtils.GetFirstNibble(OutputRom[location + 3]);
            byte unk1_kp = StaticUtils.CombineNibbles(unk1, abilities[5]);
            byte ka_ab = StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            byte unk2 = StaticUtils.GetSecondNibble(OutputRom[location + 5]);
            byte bb_unk2 = StaticUtils.CombineNibbles(abilities[4], unk2);

            SetByte(location, rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 3, unk1_kp);
            SetByte(location + 4, ka_ab);
            SetByte(location + 5, bb_unk2);
        }

        protected virtual string GetPunterAbilities(int season, // (season = 1-3)
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
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(season, team, position);
                retVal += String.Format("[{0:X}]", (OutputRom[location] & 0x0F));
            }
            return retVal;
        }

        public virtual void SetPunterAbilities(int season, string team, string position, byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance,
            //byte kickingPower, byte avoidBlock)
        {
            StaticUtils.CheckTSB2Args(season, team);
            if (position != "P") throw new ArgumentException("Invalid position argument! (takes P) " + position);

            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte kp_ab = StaticUtils.CombineNibbles(abilities[5], abilities[6]);
            byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte bb_unk1 = StaticUtils.CombineNibbles(abilities[4], unk1);

            SetByte(location, rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 3, kp_ab);
            SetByte(location + 4, bb_unk1);
        }
        

        public virtual string GetPlayerAbilities(int season, // (season = 1-3)
            string team, string position)
        {
            switch (position)
            {
                case "QB1":
                case "QB2":
                    return GetQBAbilities(season, team, position);
                case "C":
                case "RG":
                case "LG":
                case "RT":
                case "LT":
                    return GetOLPlayerAbilities(season, team, position);
                case "K":
                    return GetKickerAbilities(season, team, position);
                case "P":
                    return GetPunterAbilities(season, team, position);
            }
            return GetSkill_DefPlayerAbilities(season, team, position);
        }

        protected virtual string GetSkill_DefPlayerAbilities(int season, string team, string position)
        {
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
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(season, team, position);
                retVal += String.Format("[{0:X2},{1:X2},{2:X2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2]);
                if (positionNames.IndexOf(position) < 13)
                    retVal += ("," + OutputRom[location + 3].ToString("X2"));
                retVal += "]";
            }
            return retVal;
        }

        public virtual void SetSkillPlayerAbilities(int season, // (season = 1-3)
            string team, string position, byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance,
            //byte ballControl, byte receptions)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(position); //2-11
            if (posIndex < 2 || posIndex > 11) throw new ArgumentException("Invalid position argument! (takes RB1=TE2)" + position);

            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bc_rec = StaticUtils.CombineNibbles(abilities[5], abilities[6]);
            byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte bb_unk1 = StaticUtils.CombineNibbles(abilities[4], unk1);
            SetByte(location, rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 3, bc_rec);
            SetByte(location + 4, bb_unk1);
        }

        public virtual void SetDefensivePlayerAbilities(int season, string team, string position,byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance,
            //byte interceptions, byte quickness)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(position); //17-34
            if (posIndex < 17 || posIndex > 34) throw new ArgumentException("Invalid position argument! (takes RE-DB3)" + position);

            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte pi_qu = StaticUtils.CombineNibbles(abilities[5], abilities[6]);
            byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte bb_unk1 = StaticUtils.CombineNibbles(abilities[4], unk1);
            SetByte(location, rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 3, pi_qu);
            SetByte(location + 4, bb_unk1);
        }
        #endregion

        // called 'GetFace()' for historical purpose, first nibble is race (0=white, 8=black); not sure what the second nibble is
        protected virtual byte GetFace(int season,
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
            byte lowNibble = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte highNibble = StaticUtils.GetFirstNibble((byte)face);
            byte face_b = StaticUtils.CombineNibbles(highNibble, lowNibble);
            SetByte(location, face_b);
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

        public string GetTeams(int season)
        {
            StringBuilder builder = new StringBuilder();
            foreach(string team in teams)
                GetTeam(season, team, builder);
            if (ShowProBowlRosters)
            {
                GetProBowlTeam(season, Conference.AFC, builder);
                GetProBowlTeam(season, Conference.NFC, builder);
            }
            string retVal = builder.ToString();
            return retVal;
        }

        private void GetTeam(int season, string team, StringBuilder builder)
        {
            int i = teams.IndexOf(team);
            builder.Append("TEAM = ");
            builder.Append(team);
            builder.Append(",");
            builder.Append(GetTeamSimData(season, team));
            builder.Append("\n");
            if (ShowPlaybooks)
            {
                builder.Append(GetPlaybook(season, team));
                builder.Append("\n");
            }
            builder.Append(String.Format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}\n",
                GetTeamAbbreviation(i), GetTeamCity(i), GetTeamName(i)));
            builder.Append(GetTeamPlayers(season, team));
        }

        // QB1,004A  (74)  Marino is the 74th player
        // conf, proBowlPos, team, pos
        // AFC,QB2,oilers,QB1
        private void GetProBowlTeam(int season, Conference conf, StringBuilder builder)
        {
            int location = GetProbowlTeamLocation(season, conf);
            builder.Append(String.Format("# {0} ProBowl players\n", conf.ToString()));
            string team = "";
            string playerPos = "";
            int playerPositionIndex = -1;
            int playerIndex = -1;
            int teamIndex = -1;
            foreach (string pos in positionNames)
            {
                playerIndex = (OutputRom[location + 1] << 8) + OutputRom[location];
                teamIndex = playerIndex / positionNames.Count;
                playerPositionIndex = playerIndex % positionNames.Count;
                playerPos = positionNames[playerPositionIndex];
                team = teams[teamIndex];
                builder.Append(string.Format("{0},{1},{2},{3}\n", conf.ToString(),pos,team,playerPos));
                //builder.Append(String.Format("{0},{1:X2}{2:X2}  ({3})\n",pos, OutputRom[location + 1], OutputRom[location],(OutputRom[location + 1]<<8) + OutputRom[location]));
                location += 2;
            }
            builder.Append("\n");
        }

        
        public void SetProBowlPlayer(int season, Conference conf, String proBowlPos, String fromTeam, TSBPlayer fromTeamPos)
        {
            int playerIndex = teams.IndexOf(fromTeam) * positionNames.Count + (int)fromTeamPos;
            int location = GetProbowlTeamLocation(season, conf) + 2 * positionNames.IndexOf(proBowlPos);
            // pi:0x004a ==> 4a 00
            byte b1 = (byte)(playerIndex >> 8);
            byte b2 = (byte)(playerIndex & 0x00FF);
            SetByte(location, b2);
            SetByte(location + 1, b1);
        }

        private int GetProbowlTeamLocation(int season, Conference conf)
        {
            int ptr_location = pro_bowl_ptr_location_season_1;
            switch (season)
            {
                case 2: ptr_location = pro_bowl_ptr_location_season_2; break;
                case 3: ptr_location = pro_bowl_ptr_location_season_3; break;
            }
            if (conf == Conference.NFC)
                ptr_location += 6;
            int location = (OutputRom[ptr_location + 1] << 8) + OutputRom[ptr_location];
            int offset = 0xD8000; //0xd012 + offset = 0xe5012: 
            location += offset;
            return location;
        }

        protected int GetSimLocation(int season, string team, string position)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int index = teams.IndexOf(team);
            //int location = index * team_sim_size;
            int ptr_location = index * 2;
            switch (season)
            {
                case 1: ptr_location += team_sim_start_season_1; break;
                case 2: ptr_location += team_sim_start_season_2; break;
                case 3: ptr_location += team_sim_start_season_3; break;
            }
            int location = 0x1e0000 + (OutputRom[ptr_location + 1] << 8) + OutputRom[ptr_location];
            switch (position)
            {
                case "QB1": break;
                case "QB2": location += 3; break;

                case "RB1": location += 6; break;
                case "RB2": location += 10; break;
                case "RB3": location += 14; break;
                case "RB4": location += 18; break;
                case "WR1": location += 22; break;
                case "WR2": location += 26; break;
                case "WR3": location += 30; break;
                case "WR4": location += 34; break;
                case "TE1": location += 38; break;
                case "TE2": location += 42; break;

                case "RE":  location += 46; break;
                case "NT":  location += 49; break;
                case "LE":  location += 52; break;
                case "RE2": location += 55; break;
                case "NT2": location += 58; break;
                case "LE2": location += 61; break;

                case "LOLB": location += 64; break;
                case "LILB": location += 67; break;
                case "RILB": location += 70; break;
                case "ROLB": location += 73; break;
                case "LB5":  location += 76; break;
                case "RCB":  location += 79; break;
                case "LCB":  location += 82; break;
                case "DB1":  location += 85; break;
                case "DB2":  location += 88; break;
                case "FS":   location += 91; break;
                case "SS":   location += 94; break;
                case "DB3":  location += 97; break;

                case "K":
                case "P": location += 100; break;
                    // 101 = rushD & passD
            }
            return location;
        }

        /// <summary>
        ///  |012345|
        ///  4 = passing
        ///  5 = scramble
        /// </summary>
        /// <param name="data">Array of bytes</param>
        public void SetQBSimData(int season, string team, string position, int[] data)
        {
            int loc = GetSimLocation(season, team, position);
            for (int i = 0; i < data.Length; i++)
            {
                SetByte(loc + i, (byte)data[i]);
            }
        }

        public virtual void SetSkillSimData(int season, string team, string pos, int[] data)
        {
            int loc = GetSimLocation(season, team, pos);
            for (int i = 0; i < data.Length; i++)
            {
                SetByte(loc + i, (byte)data[i]);
            }
        }

        public void SetDefensiveSimData(int season, string team, string pos, int[] data)
        {
            int loc = GetSimLocation(season, team, pos);
            for (int i = 0; i < data.Length; i++)
            {
                SetByte(loc + i, (byte)data[i]);
            }
        }

        // 1st nibble 
        public void SetKickingSimData(int season, string team, byte data)
        {
            int loc = GetSimLocation(season, team, "K");
            byte current = (byte)( OutputRom[loc] & 0x0F);
            data = (byte)(data << 4);
            data += current;
            SetByte(loc, data);
        }

        // 2nd nibble
        public void SetPuntingSimData(int season, string team, byte data)
        {
            int loc = GetSimLocation(season, team, "P");
            byte current =(byte) (OutputRom[loc] & 0xF0);
            data += current;
            SetByte(loc, data);
        }


        public string GetTeamSimData(int season, string team)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int location = GetSimLocation(season, team, "K") + 1;
            string retVal = "SimData=0x" + OutputRom[location].ToString("X2");
            return retVal;
        }

        public void SetTeamSimData(int season, string team, string data)
        {
            StaticUtils.CheckTSB2Args(season, team);
            byte[] theBytes = StaticUtils.GetHexBytes(data);
            int location = GetSimLocation(season, team, "K") + 1;
            SetByte(location, theBytes[0]);
        }

        public string GetTeamPlayers(int season, string team)
        {
            StaticUtils.CheckTSB2Args(season, team);
            StringBuilder builder = new StringBuilder();

            foreach (string position in positionNames)
            {
                GetPlayer(season, team, builder, position);
            }
            builder.Append("KR,");
            builder.Append(GetKickReturner(season, team));
            builder.Append("\nPR,");
            builder.Append(GetPuntReturner(season, team));
            builder.Append("\n\n");

            string retVal = builder.ToString();
            return retVal;
        }

        internal virtual void GetPlayer(int season, string team, StringBuilder builder, string position)
        {
            byte playerNumber = 0;
            byte face = 0;
            builder.Append(position);
            builder.Append(",");
            builder.Append(GetPlayerName(season, team, position, out playerNumber));
            builder.Append(",");
            face = GetFace(season, team, position);
            builder.Append(String.Format("Face=0x{0:x2},#{1:x},", face, playerNumber));
            builder.Append(GetPlayerAbilities(season, team, position));
            builder.Append("\n");
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
            string retVal = StaticUtils.GetStringTableString(OutputRom, teamIndex, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset).Substring(5);
            int lastSpace = retVal.LastIndexOf(' ');
            if (lastSpace > -1)
                retVal = retVal.Substring(0, lastSpace);
            return retVal;
        }

        public virtual string GetTeamAbbreviation(int teamIndex)
        {
            string retVal = StaticUtils.GetStringTableString(OutputRom, teamIndex, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset);
            retVal = retVal.Substring(0, 4);
            return retVal;
        }

        public virtual void SetTeamAbbreviation(int teamIndex, string abb)
        {
            if (abb != null && abb.Length > 0)
            {
                string teamString = String.Format("{0}*{1} {2}*", abb, GetTeamCity(teamIndex), GetTeamName(teamIndex));
                StaticUtils.SetStringTableString(OutputRom, teamIndex, teamString, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset, 30, TEAM_NAME_STRING_TABLE_SIZE);
            }
        }

        public virtual void SetTeamName(int teamIndex, string name)
        {
            if (name != null && name.Length > 0)
            {
                string teamString = String.Format("{0}*{1} {2}*", GetTeamAbbreviation(teamIndex), GetTeamCity(teamIndex), name);
                StaticUtils.SetStringTableString(OutputRom, teamIndex, teamString, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset, 30, TEAM_NAME_STRING_TABLE_SIZE);
            }
        }

        public virtual void SetTeamCity(int teamIndex, string city)
        {
            if (city != null && city.Length > 0)
            {
                string teamString = String.Format("{0}*{1} {2}*", GetTeamAbbreviation(teamIndex), city, GetTeamName(teamIndex));
                StaticUtils.SetStringTableString(OutputRom, teamIndex, teamString, tsb2_team_name_string_table_first_ptr, tsb2_team_name_string_table_offset, 30, TEAM_NAME_STRING_TABLE_SIZE);
            }
        }

        public static int GetTeamIndex(string team) { return teams.IndexOf(team); }
        public static string GetTeamFromIndex(int i) { return teams[i]; }

        public virtual int GetPlaybookLocation(int season, string team)
        {
            int playbookLocation = -1;
            if (team.ToUpper() == "AFC")
                playbookLocation = playbook_start[0] + 28 * playbook_team_size;
            else if (team.ToUpper() == "NFC")
                playbookLocation = playbook_start[0] + 29 * playbook_team_size;
            else
            {
                StaticUtils.CheckTSB2Args(season, team);
                playbookLocation = playbook_start[season - 1] + teams.IndexOf(team) * playbook_team_size;
            }
            return playbookLocation;
        }

        /// <summary>
        /// runs = r12345678
        /// pass = p12345678
        /// </summary>
        public void SetPlaybook(int season, string team, string runs, string passes)
        {
            int playbookLocation = GetPlaybookLocation(season, team);
            byte[] runBytes = StaticUtils.GetHexBytes(runs.Substring(1));
            byte[] passBytes = StaticUtils.GetHexBytes(passes.Substring(1));
            // playbook 1
            if (runBytes.Length > 1 && passBytes.Length > 1)
            {
                SetByte(playbookLocation, runBytes[0]);
                SetByte(playbookLocation + 1, runBytes[1]);
                SetByte(playbookLocation + 2, passBytes[0]);
                SetByte(playbookLocation + 3, passBytes[1]);

                //playbook 2
                if (runBytes.Length > 3 && passBytes.Length > 3)
                {
                    SetByte(playbookLocation + 4, runBytes[2]);
                    SetByte(playbookLocation + 5, runBytes[3]);
                    SetByte(playbookLocation + 6, passBytes[2]);
                    SetByte(playbookLocation + 7, passBytes[3]);
                }
            }
            else
                StaticUtils.AddError(string.Format("ERROR Settings playbook for season:{0} team:{1} data={2} {3}",
                    season,team, runs, passes));
        }

        public string GetPlaybook(int season, string team)
        {
            int playbookLocation = GetPlaybookLocation(season, team);
            string retVal = string.Format("PLAYBOOK R{0:X2}{1:X2}{2:X2}{3:X2}, P{4:X2}{5:X2}{6:X2}{7:X2} ",
                OutputRom[playbookLocation],    OutputRom[playbookLocation + 1], // runs
                OutputRom[playbookLocation + 4],OutputRom[playbookLocation + 5],
                OutputRom[playbookLocation + 2],OutputRom[playbookLocation + 3], //passes
                OutputRom[playbookLocation + 6],OutputRom[playbookLocation + 7]
                );
            return retVal;
        }

        public void SetYear(string year)
        {
            //TODO ???
        }


        public virtual string GetKey()
        {
            return
@"# TSBTool2 forum: https://tecmobowl.org/forums/topic/71051-initial-tsb2-snes-editor/
# TSB2 Hacking documentation: https://tecmobowl.org/forums/topic/53028-tecmo-super-bowl-ii-hackingresource-documentation/
# Key 
# 'SET' commands are supported
# Double click on a team name (or playbook) to bring up the edit Team GUI.
# Double click on a player to bring up the edit player GUI (Click 'Sim Data'
#   button to find out more on Sim Data).
# Attribute Order
# QBs   RS RP MS HP BB PS PC PA AR CO [sim vals]
# Skill RS RP MS HP BB BC RC [sim vals]
# OL    RS RP MS HP BB
# DEF   RS RP MS HP BB PI QU [sim vals]
# K     RS RP MS HP BB KP KA AB [sim val]
# P     RS RP MS HP BB KP AB [sim val]
";
        }

        public virtual string GetAll()
        {
            StringBuilder builder = new StringBuilder(10000);
            builder.Append("SEASON 1\n");
            builder.Append(GetTeams(1));
            if(ShowSchedule)
                builder.Append(GetSchedule(1));

            builder.Append("SEASON 2\n");
            builder.Append(GetTeams(2));
            if (ShowSchedule)
                builder.Append(GetSchedule(2));

            builder.Append("SEASON 3\n");
            builder.Append(GetTeams(3));
            if (ShowSchedule)
                builder.Append(GetSchedule(3));
            return builder.ToString();
        }

        public virtual string GetAll(int season)
        {
            StringBuilder builder = new StringBuilder(5000);
            builder.Append("SEASON ");
            builder.Append(season);
            builder.Append("\n");
            builder.Append(GetTeams(season));
            if (ShowSchedule)
                builder.Append(GetSchedule(season));
            return builder.ToString();
        }

        protected virtual int GetKickReturnLocation(int season, string team)
        {
            int location = bills_kr_loc_season_1;
            switch (season)
            {
                case 2: location = bills_kr_loc_season_2; break;
                case 3: location = bills_kr_loc_season_3; break; 
            }
            location += (2 * teams.IndexOf(team));
            return location;
        }

        public void SetKickReturner(int season, string team, string position)
        {
            int location = GetKickReturnLocation(season, team);
            int pos_num = positionNames.IndexOf(position);
            SetByte(location, (byte)pos_num);
        }

        public void SetPuntReturner(int season, string team, string position)
        {
            int location = 1 + GetKickReturnLocation(season, team);
            int pos_num = positionNames.IndexOf(position);
            SetByte(location, (byte)pos_num);
        }

        protected string GetPuntReturner(int season, string team)
        {
            int location = 1 + GetKickReturnLocation(season, team);
            int pos_num = OutputRom[location];
            string pos = positionNames[pos_num];
            return pos;
        }

        protected string GetKickReturner(int season, string team)
        {
            int location = GetKickReturnLocation(season, team);
            int pos_num = OutputRom[location];
            string pos = positionNames[pos_num];
            return pos;
        }

        public virtual void ApplySchedule(int season, List<string> scheduleList)
        {
            SNES_ScheduleHelper helper = new SNES_ScheduleHelper(this);
            switch (season)
            {
                case 1: helper.SetWeekOneLocation(schedule_start_season_1, seventeenWeeks, teams); break;
                case 2: helper.SetWeekOneLocation(schedule_start_season_2, eighteenWeeks, teams); break;
                case 3: helper.SetWeekOneLocation(schedule_start_season_3, seventeenWeeks, teams); break;
            }
            helper.ApplySchedule(scheduleList);
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
