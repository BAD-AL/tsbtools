using System;
using System.Collections.Generic;
using System.Text;

namespace TSBTool2
{
    // TODO: 
    // handle Player Photos
    public class TSB3Tool : TSBTool2.TSB2Tool
    {
        public static bool IsTecmoSuperBowl3Rom(byte[] rom)
        {
            bool retVal = false;
            if (rom != null && rom.Length > 0)
            {
                List<long> results = TSBTool.StaticUtils.FindStringInFile("TECMO SUPERBOWL 3", rom, 0x1be570, 0x1bf8c0);
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

        /// <summary>
        /// Overrides for setting up locations
        /// </summary>
        private void Init()
        {
            BYTES_PER_QB = 7;
            // name_string_table_1
            tsb2_name_string_table_1_first_ptr = 0x1e8038 + 8;
            bills_kr_loc_season_1 =  0xE50AA; // TSB3

            teams = new List<string>(){
                "bills", "colts", "dolphins", "patriots", "jets",
                "bengals", "browns", "oilers", "jaguars", "steelers",
                "broncos", "chiefs", "raiders", "chargers", "seahawks",
                "cardinals", "cowboys", "giants", "eagles", "redskins",
                "bears", "lions", "packers", "vikings", "buccaneers",
                "falcons", "panthers", "saints", "rams", "49ers", "freeAgents", "allTime"
            };
        }


        public override string GetKey()
        {
            return string.Format(
@"# TSBTool Forum: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/
# TSBTool2 Forum: https://tecmobowl.org/forums/topic/71072-tsbii-tsbiii-editor-tsbtool2/
# Editing: Tecmo Super Bowl III (snes) [{0}]
# TSBIII Hacking documentation: https://tecmobowl.org/forums/topic/53029-tecmo-super-bowl-iii-hackingresource-documentation/
# Key 
# 'SET' commands are supported
# Double click on a team name (or playbook) to bring up the edit Team GUI.
# Double click on a player to bring up the edit player GUI (Click 'Sim Data'
#   button to find out more on Sim Data).
# Attribute Order
# QBs   RS RP MS HP BB AG PS PC PA AR CO [sim vals]
# Skill RS RP MS HP BB AG BC RC [sim vals]
# OL    RS RP MS HP BB AG 
# DEF   RS RP MS HP BB AG PI QU [sim vals]
# K     RS RP MS HP BB AG KP KA AB [sim val]
# P     RS RP MS HP BB AG KP AB [sim val]
", this.RomVersion);
        }
        
        internal override void GetPlayer(int season, string team, StringBuilder builder, string position)
        {
            base.GetPlayer(1, team, builder, position);
        }

        protected override byte GetFace(int season, string team, string position)
        {
            int location = GetPlayerAttributeLocation(season, team, position) + 3;
            byte retVal = OutputRom[location];
            return retVal;
        }
        
        public override void SetFace(int season, string team, string position, int face)
        {
            TSBTool.StaticUtils.CheckTSB2Args(season, team, position);
            int location = GetPlayerAttributeLocation(season, team, position) + 3;
            SetByte(location, (byte)face);
        }

        protected override string GetQBAbilities(int season, // (season = 1-3)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(season, team, position);
            byte rs = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            byte bb = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 2]);
            byte ps = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte pc = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte pa = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 5]);
            byte ar = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 5]);
            byte co = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 6]);
            byte sp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 6]); //sim pocket? maybe do more research for this one.

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, ps, pc, pa, ar, co };
            string retVal = TSBTool.StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} ", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5], OutputRom[location + 6]);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(1, team, position);
                retVal += String.Format("[{0:X2},{1:X2},{2:X2}]",OutputRom[location], OutputRom[location + 1], OutputRom[location + 2]);
            }
            return retVal;
        }

        // abilities should be in the order they are listed in the game
        public override void SetQBAbilities(int season, string team, string qb, byte[] abilities)
        {
            TSBTool.StaticUtils.CheckTSB2Args(season, team);
            if (qb != "QB1" && qb != "QB2") throw new ArgumentException("Invalid qb position " + qb);
            
            int location = GetPlayerAttributeLocation(1, team, qb);
            byte rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[4], abilities[5]);

            byte ps_pc = TSBTool.StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            byte pa_ar = TSBTool.StaticUtils.CombineNibbles(abilities[8], abilities[9]);
            byte unk1 = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 6]);
            byte co_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[10], unk1);
            
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
            byte rs = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            byte bb = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag };
            string retVal = TSBTool.StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3]);
            return retVal;
        }
        // abilities should be in the order they are listed in the game
        public override void SetOLPlayerAbilities(int season, string team, string pos, byte[] abilities)
        {
            TSBTool.StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(pos);
            if (posIndex < 12 || posIndex > 16) throw new ArgumentException("Invalid position argument! (takes C,RG,RT,LG,LT) " + pos);

            int location = GetPlayerAttributeLocation(1, team, pos);
            byte rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
        }

        protected override string GetKickerAbilities(int season, // (season = 1)
            string team, string position)
        {
            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 1]);
            
            byte bb = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte kp = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte ka = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            byte ab = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 5]);
            
            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, kp, ka, ab };
            string retVal = TSBTool.StaticUtils.MapAttributes(attrs); // +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2} {5:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4], OutputRom[location + 5]);
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
            TSBTool.StaticUtils.CheckTSB2Args(1, team);
            if (position != "K") throw new ArgumentException("Invalid position argument! (takes K) " + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            byte kp_ka = TSBTool.StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            
            byte unk1 = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 5]);
            byte ab_unk1 = TSBTool.StaticUtils.CombineNibbles(abilities[8], unk1);

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
            byte rs = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 1]);

            byte bb = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 2]);

            byte kp = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte ab = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            
            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, kp, ab };
            string retVal = TSBTool.StaticUtils.MapAttributes(attrs);
            if (ShowPlayerSimData)
            {
                location = GetSimLocation(1, team, position);
                retVal += String.Format("[{0:X}]", (OutputRom[location] & 0x0F));
            }
            return retVal;
        }

        public override void SetPunterAbilities(int season, string team, string position, byte[] abilities)
        {
            TSBTool.StaticUtils.CheckTSB2Args(1, team);
            if (position != "P") throw new ArgumentException("Invalid position argument! (takes P) " + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[4], abilities[5]);

            byte kp_ab = TSBTool.StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            
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
            byte rs = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location]);
            byte rp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location]);
            byte ms = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 1]);
            byte hp = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 1]);

            byte bb = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 2]);
            byte ag = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 2]);
            byte bc = TSBTool.StaticUtils.GetFirstNibble(OutputRom[location + 4]);
            byte rec = TSBTool.StaticUtils.GetSecondNibble(OutputRom[location + 4]);
            

            byte[] attrs = new byte[] { rs, rp, ms, hp, bb, ag, bc, rec };
            string retVal = TSBTool.StaticUtils.MapAttributes(attrs);// +String.Format("|{0:x2} {1:x2} {2:x2} {3:x2} {4:x2}", OutputRom[location], OutputRom[location + 1], OutputRom[location + 2], OutputRom[location + 3], OutputRom[location + 4]);
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
            TSBTool.StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(position); //2-11
            if (posIndex < 2 || posIndex > 11) throw new ArgumentException("Invalid position argument! (takes RB1=TE2)" + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            byte bc_rec = TSBTool.StaticUtils.CombineNibbles(abilities[6], abilities[7]);
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
            TSBTool.StaticUtils.CheckTSB2Args(season, team);
            int posIndex = positionNames.IndexOf(position); //17-34
            if (posIndex < 17 || posIndex > 34) throw new ArgumentException("Invalid position argument! (takes RE-DB3)" + position);

            int location = GetPlayerAttributeLocation(1, team, position);
            byte rs_rp = TSBTool.StaticUtils.CombineNibbles(abilities[0], abilities[1]);
            byte ms_hp = TSBTool.StaticUtils.CombineNibbles(abilities[2], abilities[3]);
            byte bb_ag = TSBTool.StaticUtils.CombineNibbles(abilities[4], abilities[5]);
            byte pi_qu = TSBTool.StaticUtils.CombineNibbles(abilities[6], abilities[7]);
            SetByte(location    , rs_rp);
            SetByte(location + 1, ms_hp);
            SetByte(location + 2, bb_ag);
            SetByte(location + 4, pi_qu);
        }

        private int[] seventeenWeeks = new int[] { 15, 15, 15, 12, 13, 13, 13, 13, 13, 14, 14, 15, 15, 15, 15, 15, 15 };

        public override string GetSchedule(int season)
        {
            SNES_TSB3_ScheduleHelper helper = new SNES_TSB3_ScheduleHelper(this);
            helper.SetWeekOneLocation(schedule_start_season_1, seventeenWeeks, scheduleTeamOrder);
            return helper.GetSchedule();
        }

        public override void ApplySchedule(int season, List<string> scheduleList)
        {
            SNES_TSB3_ScheduleHelper helper = new SNES_TSB3_ScheduleHelper(this);
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

        public override TSBTool.ROM_TYPE RomVersion { get { return TSBTool.ROM_TYPE.SNES_TSB3; } }
    }
}
