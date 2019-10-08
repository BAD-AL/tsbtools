using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSBTool2
{
    // TODO: 
    // Free Agents.
    // handle Player Photos
    // TSB3 Schedule Handling
    public class TSB3Tool : TSBTool2.TSB2Tool
    {
        public static bool IsTecmoSuperBowl3Rom(byte[] rom)
        {
            bool retVal = false;
            if (rom != null && rom.Length > 0)
            {
                List<long> results = StaticUtils.FindStringInFile("TECMO SUPERBOWL 3", rom, 0x1be570, 0x1bf8c0);
                if (results.Count > 0)
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        public TSB3Tool(byte[] rom)
        {
            this.OutputRom = rom;
            Init();
        }
        public TSB3Tool()
        {
            Init();
        }

        internal static List<string> scheduleTeamOrder = new List<string>(){
                "dolphins","patriots",  "colts", "bills",   "jets",
                "steelers", "browns",  "bengals", "oilers", "jaguars", 
                "chargers", "chiefs", "raiders", "broncos", "seahawks",
                "cowboys","giants", "cardinals",   "eagles", "redskins",
                "vikings", "packers","lions", "bears", "buccaneers",
                "49ers", "saints", "falcons", "rams", "panthers",  
            };

        private void Init()
        {
            BYTES_PER_PLAYER = 5; // SAME as TSB2
            BYTES_PER_QB = 7;
            NAME_TABLE_SIZE = 0x4798; // SAME as TSB2
            BANK_1_PLAYER_ATTRIBUTES_START = 0x1ec800;// same as TSB2 
            // name_string_table_1
            tsb2_name_string_table_1_first_ptr = 0x1e8038 + 8;
            tsb2_name_string_table_1_offset = 0x1e0000; // SAME as TSB2 

            // Team name String table
            tsb2_team_name_string_table_first_ptr = 0x7000;// SAME as TSB2
            tsb2_team_name_string_table_offset = -32768;// SAME as TSB2
            TEAM_NAME_STRING_TABLE_SIZE = 0x5c0;// We'll go with the same size here as TSB2

            schedule_start_season_1 = 0x16f00c; //data=050D060114130A0C1900 // SAME as TSB2

            team_sim_start_season_1 = 0x1EE000; //???

            team_sim_size = 102;
            //return base.Init(fileName);
            teams = new List<string>(){
                "bills", "colts", "dolphins", "patriots", "jets",
                "bengals", "browns", "oilers", "jaguars", "steelers",
                "broncos", "chiefs", "raiders", "chargers", "seahawks",
                "cardinals", "cowboys", "giants", "eagles", "redskins",
                "bears", "lions", "packers", "vikings", "buccaneers",
                "falcons", "panthers", "saints", "rams", "49ers"
            };


        }


        protected override string GetQBAbilities(int season, // (season = 1-3)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte ps = StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte pc = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte pa = StaticUtils.GetFirstNibble(OutputRom[location + 5]);
            byte ar = StaticUtils.GetSecondNibble(OutputRom[location + 5]);
            byte co = StaticUtils.GetFirstNibble(OutputRom[location + 6]);

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, ps, pc, pa, ar, co };
            string retVal = StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} ", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5], OutputRom[location + 6]);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(1, team, position);
                retVal += String.Format("[{0:X2},{1:X2},{2:X2}]", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2]);
            }
            return retVal;
        }

        // abilities should be in the order they are listed in the game
        public override void SetQBAbilities(int season, string team, string qb, byte[] abilities)
        {
            StaticUtils.CheckTSB2Args(season, team);
            if (qb != "QB1" && qb != "QB2") throw new ArgumentException("Invalid qb position " + qb);
            
            int location = GetPlayerAttributeLocation(1, team, qb);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = StaticUtils.CombineNibbles(abilities[4], abilities[5]);

            byte ps_pc = StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            byte pa_ar = StaticUtils.CombineNibbles(abilities[8], abilities[9]);
            byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 6]);
            byte co_unk1 = StaticUtils.CombineNibbles(abilities[10], unk1);
            
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
            SetByte(location + 4, ps_pc);
            SetByte(location + 5, pa_ar);
            SetByte(location + 6, co_unk1);
        }

        protected override string GetOLPlayerAbilities(int season, // (season = 1)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag };
            string retVal = StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3]);
            return retVal;
        }
        // abilities should be in the order they are listed in the game
        public override void SetOLPlayerAbilities(int season, string team, string pos, byte[] abilities)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(pos);
            if (posIndex < 12 || posIndex > 16) throw new ArgumentException("Invalid position argument! (takes C,RG,RT,LG,LT) " + pos);

            int location = GetPlayerAttributeLocation(1, team, pos);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
        }

        protected override string GetKickerAbilities(int season, // (season = 1)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            
            byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte kp = StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte ka = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte ab = StaticUtils.GetFirstNibble(OutputRom[location + 5]);
            
            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, kp, ka, ab };
            string retVal = StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5]);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(1, team, position);
                retVal += String.Format("[{0:X}]", (OutputRom[location] >> 4));
            }
            return retVal;
        }

        // abilities should be in the order they are listed in the game
        public override void SetKickerAbilities(int season, string team, string position, byte[] abilities)
        {
            StaticUtils.CheckTSB2Args(1, team);
            if (position != "K") throw new ArgumentException("Invalid position argument! (takes K) " + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            byte kp_ka = StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            
            byte unk1 = StaticUtils.GetSecondNibble(OutputRom[location + 5]);
            byte ab_unk1 = StaticUtils.CombineNibbles(abilities[8], unk1);// TODO: check this function

            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
            SetByte(location + 4, kp_ka);
            SetByte(location + 5, ab_unk1);
        }

        protected override string GetPunterAbilities(int season, // (season = 1-3)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);

            byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte kp = StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte ab = StaticUtils.GetSecondNibble(OutputRom[location + 4]); // TODO: check this function
            
            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, kp, ab };
            string retVal = StaticUtils.MapAttributes(attrs);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(1, team, position);
                retVal += String.Format("[{0:X}]", (OutputRom[location] & 0x0F));
            }
            return retVal;
        }

        public override void SetPunterAbilities(int season, string team, string position, byte[] abilities)
        {
            StaticUtils.CheckTSB2Args(1, team);
            if (position != "P") throw new ArgumentException("Invalid position argument! (takes P) " + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = StaticUtils.CombineNibbles(abilities[4], abilities[5]);

            byte kp_ab = StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
            SetByte(location + 4, kp_ab);
        }


        public override string GetPlayerAbilities(int season, // (season = 1-3)
            string team, string position)
        {
            return base.GetPlayerAbilities(1, team, position);
        }

        protected override string GetSkill_DefPlayerAbilities(int season, string team, string position)
        {
            // RB attributes (nibbles):
            // RS RP MS HP BB AG BC REC Race ??
            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs = StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = StaticUtils.GetSecondNibble(OutputRom[location + 1]);

            byte bb = StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = StaticUtils.GetSecondNibble(OutputRom[location + 2]);
            byte bc = StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte rec = StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, bc, rec };
            string retVal = StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4]);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(1, team, position);
                retVal += String.Format("[{0:X2},{1:X2},{2:X2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2]);
                if (positionNames.IndexOf(position) < 13)
                    retVal += ("," + OutputRom[location + 3].ToString("X2"));
                retVal += "]";
            }
            return retVal;
        }

        public override void SetSkillPlayerAbilities(int season, // (season = 1-3)
            string team, string position, byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance, agility
            //byte ballControl, byte receptions)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(position); //2-11
            if (posIndex < 2 || posIndex > 11) throw new ArgumentException("Invalid position argument! (takes RB1=TE2)" + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            byte bc_rec = StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
            SetByte(location + 4, bc_rec);
        }

        public override void SetDefensivePlayerAbilities(int season, string team, string position, byte[] abilities)
            //byte runningSpeed, byte rushingPower,
            //byte maxSpeed, byte hittingPower,
            //byte bodyBalance,
            //byte interceptions, byte quickness)
        {
            StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(position); //17-34
            if (posIndex < 17 || posIndex > 34) throw new ArgumentException("Invalid position argument! (takes RE-DB3)" + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            byte pi_qu = StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
            SetByte(location + 4, pi_qu);
        }

        private int[] seventeenWeeks = new int[] { 15, 15, 15, 12, 13, 13, 13, 13, 13, 14, 14, 15, 15, 15, 15, 15, 15 };

        public override string GetSchedule(int season)
        {
            SNES_ScheduleHelper helper = new SNES_ScheduleHelper(this);
            helper.SetWeekOneLocation(schedule_start_season_1, seventeenWeeks, scheduleTeamOrder);
            return helper.GetSchedule();
        }

        public override void ApplySchedule(int season, List<string> scheduleList)
        {
            SNES_ScheduleHelper helper = new SNES_ScheduleHelper(this);
            helper.SetWeekOneLocation(schedule_start_season_1, seventeenWeeks, scheduleTeamOrder);
            helper.ApplySchedule(scheduleList);
        }

        public override string GetAll()
        {
            return GetAll(1);
        }

        public override string GetAll(int season)
        {
            return base.GetAll(1);
        }

        public override string RomVersion { get { return "SNES_TSB3"; } }
    }
}
