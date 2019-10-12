using System;
using System.Collections.Generic;

namespace TSBTool2
{
    public enum TSBPlayer
    {
        QB1 = 0, QB2, RB1, RB2, RB3, RB4, WR1, WR2, WR3, WR4,
        TE1, TE2, C, LG, RG, LT, RT,
        RE, NT, LE, RE2, NT2, LE2, ROLB, RILB, LILB, LOLB, LB5, RCB, LCB, DB1, DB2, FS, SS, DB3,
        K, P
    }

    public enum Conference { AFC, NFC }

    public interface ITecmoTool
    {
        void SetByte(int location, byte b);
        byte[] OutputRom { get; set; }

        //bool ShowOffPref {get; set;}

        string GetKey();

        string GetAll();
        string GetAll(int season);
        string GetTeamPlayers(int season, string team);

        #region team String stuff

        void SetTeamAbbreviation(int teamIndex, string abb);

        void SetTeamName(int teamIndex, string name);

        void SetTeamCity(int teamIndex, string city);

        string GetTeamAbbreviation(int teamIndex);

        string GetTeamName(int teamIndex);

        string GetTeamCity(int teamIndex);
        #endregion

        // Returns a string with all the players listed (for a given season, 1-3)
        string GetTeams(int season);

        string GetSchedule(int season);

        void ApplySet(string line);

        void SetPlaybook(int season, string team, string runs, string passes);

        //bool ApplyJuice(int week,int amount);

        void SetTeamSimData(int season, string team, string data);

        //bool SetTeamSimOffensePref(string team, int val);

        //void SetTeamOffensiveFormation(string team, string formation);

        void SetYear(string year);

        bool IsValidPosition(string position);

        void SetFace(int season, string team, string position, int face);

        void InsertPlayerName(int season, string currentTeam, string pos, string fname, string lname, byte jerseyNumber);

        void SetQBAbilities(int season,
            string team,
            string qb,
            byte[] abilities
            //byte runningSpeed,
            //byte rushingPower,
            //byte maxSpeed,
            //byte hittingPower,
            //byte bodyBalance,
            //byte passingSpeed,
            //byte passControl,
            //byte accuracy,
            //byte avoidRush,
            //byte coolness
            );

        void SetQBSimData(int season, string team, string pos, int[] data);

        void SetSkillPlayerAbilities(int season,
            string team,
            string pos,
            byte[] abilities
            //byte runningSpeed,
            //byte rushingPower,
            //byte maxSpeed,
            //byte hittingPower,
            //byte bodyBalance,
            //byte ballControl,
            //byte receptions
            );

        void SetSkillSimData(int season, string team, string pos, int[] data);

        void SetOLPlayerAbilities(int season,
            string team,
            string pos,
            byte[] abilities
            //byte runningSpeed,
            //byte rushingPower,
            //byte maxSpeed,
            //byte hittingPower,
            //byte bodyBalance
            );

        void SetDefensivePlayerAbilities(int season,
            string team,
            string pos,
            byte[] abilities
            //byte runningSpeed,
            //byte rushingPower,
            //byte maxSpeed,
            //byte hittingPower,
            //byte bodyBalance,
            //byte interceptions,
            //byte quickness
            );

/*
 My rough working theory based on messing with the sim data:
    qb - 3 bytes (6 total) (bit 5 = passing, bit 6 = scramble)
    rb/wr/te - 4 bytes (40 bytes)
    18 defenders - 3 bytes each (sacks/int/tackles) (unlike TSB NES they are grouped per player) (54 bytes) - **CONFIRMED** 
    kicker/punter/rush defense yards allowed /pass defense yards allowed  (last 4 bits/2 bytes) - **CONFIRMED** 
That gets me to 102*/
        void SetDefensiveSimData(int season, string team, string pos, int[] data);

        void SetKickerAbilities(int season,
            string team,
            string pos,
            byte[] abilities
            //byte runningSpeed,
            //byte rushingPower,
            //byte maxSpeed,
            //byte hittingPower,
            //byte bodyBalance,
            //byte kickingPower,
            //byte kickingAccuracy,
            //byte avoidBlock
            );

        void SetPunterAbilities(int season,
            string team,
            string pos,
            byte[] abilities
            //byte runningSpeed,
            //byte rushingPower,
            //byte maxSpeed,
            //byte hittingPower,
            //byte bodyBalance,
            //byte kickingPower,
            //byte avoidBlock
            );
        void SetPuntingSimData(int season, string team, byte data);

        void SetKickingSimData(int season, string team, byte data);

        void SetKickReturner(int season, string team, string position);

        void SetPuntReturner(int season, string team, string position);

        void ApplySchedule(int season, List<string> scheduleList);

        //bool Init(string fileName);

        //void SetHomeUniform(string team, string colorString);

        //void SetAwayUniform(string team, string colorString);

        //string GetGameUniform(string team);

        //void SetUniformUsage(string team, string usage);

        //string GetUniformUsage(string team);

        //void SetDivChampColors(string team, string colorString);

        //void SetConfChampColors(string team, string colorString);

        //string GetDivChampColors(string team );

        //string GetConfChampColors(string team );

        //string GetChampColors(string team);

        //String GetProBowlPlayers(int season);

        void SetProBowlPlayer(int season, Conference conf, String proBowlPos, String fromTeam, TSBPlayer fromTeamPos);

        /// <summary>
        /// "SNES_TSB2", "SNES_TSB3", "GENESIS_TSB2", "GENESIS_TSB3"
        /// </summary>
        string RomVersion { get; }

        // for testing
        void ProcessText(string text);

    }

}