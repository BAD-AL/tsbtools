using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace TSBTool2
{
        
    public class TecmonsterTeamSim
    {
        protected string TextData{ get; set; } 

        // 'season' only really used for TSB2  
        public string AutoUpdateSeasonSimData(int season, string textData)
        {
            this.TextData = textData; // 'this.TextData' will be updated by functions called below 
            List<string> teamsForSeason = GetTeams(season); // get the list of teams in the text data
            //UpdatePlayerSimAttributes(); // according to specified formulas
            List<TeamRatings> ratings = GetTeamsRatings(season, teamsForSeason); // calculate with tecmonster's ratings formulas
            SimAverages averages = CalculateSimAverages(ratings);  //holds the averages so we don't need to re-caculate each time
            CalculateSimDefenses(ratings, ref averages);
            for (int i = 0; i < ratings.Count; i++ )
            {
                UpdateTeamSimData(season, ratings[i], averages); // calcualtes and updates the Team sim data in the text data
            }
            return this.TextData;
        }

        private void CalculateSimDefenses(List<TeamRatings> ratings, ref SimAverages averages)
        {
            foreach (TeamRatings rat in ratings)
            {
                rat.totalRunD =  (rat.dlRunDefenseRating / averages.DL_run_ave) * 0.4 + 
                            (rat.lbRunDefenseRating / averages.LB_run_ave) * 0.4 +
                            (rat.dbRunDefenseRating / averages.DB_run_ave) * 0.2;
                rat.totalPassD = (rat.dlPassDefenseRating / averages.DL_pass_ave) * 0.2 + 
                            (rat.lbPassDefenseRating / averages.LB_pass_ave) * 0.2 +
                            (rat.dbPassDefenseRating / averages.DB_pass_ave) * 0.6;
                averages.TOTAL_RUN_DEFENSE += rat.totalRunD;
                averages.TOTAL_PASS_DEFENSE += rat.totalPassD;

                if (averages.MIN_PASS_DEFENSE == 0 || rat.totalPassD < averages.MIN_PASS_DEFENSE)
                    averages.MIN_PASS_DEFENSE = rat.totalPassD;
                if (averages.MIN_RUN_DEFENSE == 0 || rat.totalRunD < averages.MIN_RUN_DEFENSE)
                    averages.MIN_RUN_DEFENSE= rat.totalRunD;
                if (rat.totalPassD > averages.MAX_PASS_DEFENSE)
                    averages.MAX_PASS_DEFENSE = rat.totalPassD;
                if (rat.totalRunD > averages.MAX_RUN_DEFENSE)
                    averages.MAX_RUN_DEFENSE = rat.totalRunD;
            }
        }

        private void UpdateTeamSimData(int season, TeamRatings ratings, SimAverages averages)
        {
            double spread = averages.MAX_RUN_DEFENSE - averages.MIN_RUN_DEFENSE;
            double simRunDefense = Math.Round(((ratings.totalRunD - averages.MIN_RUN_DEFENSE)/spread)*15);
            spread = averages.MAX_PASS_DEFENSE - averages.MIN_PASS_DEFENSE;
            double simPassDefense = Math.Round(((ratings.totalPassD - averages.MIN_PASS_DEFENSE) / spread) * 15);
            if (simRunDefense > 0xf)
                simRunDefense = 0xf;
            if (simPassDefense > 0xf)
                simPassDefense = 0xf;
            string sim_def = String.Format("{0:x}{1:x}", (int)simRunDefense, (int)simPassDefense);

            string pattern = String.Format("TEAM\\s*=\\s*{0}\\s*,?\\s*SimData\\s*=\\s*0x([0-9a-fA-F]{{2}})", ratings.team);
            Regex teamSimRegex = new Regex(pattern);
            int seasonIndex = GetSeasonIndex(season);
            Match m = teamSimRegex.Match(TextData, seasonIndex);
            string old = m.Groups[1].ToString();
            if (m != Match.Empty)
            {
                string start = TextData.Substring(0, m.Groups[1].Index);
                string last = TextData.Substring(m.Groups[1].Index + 2);
                StringBuilder tmp = new StringBuilder(TextData.Length + 20);
                tmp.Append(start);
                tmp.Append(sim_def);
                tmp.Append(last);
                TextData = tmp.ToString();
            }
        }

        private int GetSeasonIndex(int season)
        {
            int retVal = 0;
            if (StaticUtils.IsTSB2Content(TextData))
            {
                string pattern = String.Format("^\\s*SEASON\\s+{0}", season);
                Regex seasonRegex = new Regex(pattern);
                Match m = seasonRegex.Match(TextData);
                if (m.Success)
                    retVal = m.Index;
            }
            return retVal;
        }

        private SimAverages CalculateSimAverages(List<TeamRatings> ratings)
        {
            SimAverages retVal = new SimAverages();
            foreach (TeamRatings rat in ratings)
            {
                retVal.QB_ave+= rat.qbRating;
                retVal.RB_ave+= rat.rb1Rating; // + rat.rb2Rating);
                retVal.WR_ave+= (rat.wr1Rating + rat.wr2Rating);
                retVal.TE_ave+= rat.teRating;
                retVal.OL_ave+= rat.olRating;
                retVal.DL_run_ave+= rat.dlRunDefenseRating;
                retVal.DL_pass_ave+= rat.dlPassDefenseRating;
                retVal.LB_run_ave+= rat.lbRunDefenseRating;
                retVal.LB_pass_ave+= rat.lbPassDefenseRating;
                retVal.DB_run_ave+= rat.dbRunDefenseRating;
                retVal.DB_pass_ave += rat.dbPassDefenseRating;
            }
            retVal.QB_ave = retVal.QB_ave / ratings.Count;
            retVal.RB_ave = retVal.RB_ave / ratings.Count;
            retVal.WR_ave = retVal.WR_ave / (ratings.Count *2);
            retVal.TE_ave = retVal.TE_ave / ratings.Count;
            retVal.OL_ave = retVal.OL_ave / ratings.Count;
            retVal.DL_run_ave = retVal.DL_run_ave / ratings.Count;
            retVal.DL_pass_ave = retVal.DL_pass_ave / ratings.Count;
            retVal.LB_run_ave = retVal.LB_run_ave / ratings.Count;
            retVal.LB_pass_ave = retVal.LB_pass_ave / ratings.Count;
            retVal.DB_run_ave = retVal.DB_run_ave / ratings.Count;
            retVal.DB_pass_ave = retVal.DB_pass_ave / ratings.Count;
            return retVal;
        }

        private List<TeamRatings> GetTeamsRatings(int season, List<string> teams)
        {
            List<TeamRatings> retVal = new List<TeamRatings>(teams.Count);
            string seasonChunk = GetSeasonText(season);
            foreach (string team in teams)
            {
                retVal.Add(GetTeamRatings(seasonChunk, team));
            }
            return retVal;
        }

        private TeamRatings GetTeamRatings(string textData, string team)
        {
            TeamRatings retVal = new TeamRatings();
            //RS, RP, MS, HP, PS, PC, PA, APB,
            int RS=0, RP=1, MS=2, HP=3, PS=4, PC=5, PA=6, APB=7, BC=4, RC=5, PI=4, QU=5;//TSB1
            if (StaticUtils.GetContentType(TextData) == TSBContentType.TSB2)
            {
                PS = 5; PC = 6; PA = 7; APB = 8; BC = 5; RC = 6; PI = 5; QU = 6;
            }
            else if (StaticUtils.GetContentType(TextData) == TSBContentType.TSB3)
            {
                PS = 6; PC = 7; PA = 8; APB = 9; BC = 6; RC = 7; PI = 6; QU = 7;
            }
            
            int[] qb1 = GetPlayerInts(textData, team, "QB1");
            int[] rb1 = GetPlayerInts(textData, team, "RB1");
            int[] rb2 = GetPlayerInts(textData, team, "RB2");
            int[] wr1 = GetPlayerInts(textData, team, "WR1");
            int[] wr2 = GetPlayerInts(textData, team, "WR2");
            int[] te1 = GetPlayerInts(textData, team, "TE1");

            int[] center = GetPlayerInts(textData, team, "C");
            int[] lg = GetPlayerInts(textData, team, "LG");
            int[] rg = GetPlayerInts(textData, team, "RG");
            int[] lt = GetPlayerInts(textData, team, "LT");
            int[] rt = GetPlayerInts(textData, team, "RT");

            int[] re = GetPlayerInts(textData, team, "RE");
            int[] nt = GetPlayerInts(textData, team, "NT");
            int[] le = GetPlayerInts(textData, team, "LE");

            int[] rolb = GetPlayerInts(textData, team, "ROLB");
            int[] rilb = GetPlayerInts(textData, team, "RILB");
            int[] lilb = GetPlayerInts(textData, team, "LILB");
            int[] lolb = GetPlayerInts(textData, team, "LOLB");

            int[] rcb = GetPlayerInts(textData, team, "RCB");
            int[] lcb = GetPlayerInts(textData, team, "LCB");
            int[] fs = GetPlayerInts(textData, team, "FS");
            int[] ss = GetPlayerInts(textData, team, "SS");

            #region calculations
            retVal.team = team;
            retVal.qbRating = (qb1[RS] * 0.06) + (qb1[RP] * 0.02) + (qb1[MS] * 0.12) + (qb1[HP] * 0.02) + (qb1[PS] * 0.22) + (qb1[PC] * 0.23) + (qb1[PA] * 0.23) + (qb1[APB] * 0.1);
            retVal.rb1Rating = (rb1[RS] * 0.15) + (rb1[RP] * 0.15) + (rb1[MS] * 0.40) + (rb1[HP] * 0.25) + (rb1[BC] * 0.02) + (rb1[RC] * 0.03);
            retVal.rb2Rating = (rb2[RS] * 0.15) + (rb2[RP] * 0.15) + (rb2[MS] * 0.40) + (rb2[HP] * 0.25) + (rb2[BC] * 0.02) + (rb2[RC] * 0.03);
            retVal.wr1Rating = (wr1[RS] * 0.20) + (wr1[RP] * 0.15) + (wr1[MS] * 0.25) + (wr1[HP] * 0.03) + (wr1[BC] * 0.02) + (wr1[RC] * 0.35);
            retVal.wr2Rating = (wr2[RS] * 0.20) + (wr2[RP] * 0.15) + (wr2[MS] * 0.25) + (wr2[HP] * 0.03) + (wr2[BC] * 0.02) + (wr2[RC] * 0.35);
            retVal.teRating =  (te1[RS] * 0.18) + (te1[RP] * 0.1)  + (te1[MS] * 0.25) + (te1[HP] * 0.25) + (te1[BC] * 0.02) + (te1[RC] * 0.2);
            retVal.olRating = (center[RS] * 0.02) + (center[RP] * 0.01) + (center[MS] * 0.02) + (center[HP] * 0.95) +
                                (lg[RS] * 0.02) + (lg[RP] * 0.01) + (lg[MS] * 0.02) + (lg[HP] * 0.95) +
                                (rg[RS] * 0.02) + (rg[RP] * 0.01) + (rg[MS] * 0.02) + (rg[HP] * 0.95) +
                                (lt[RS] * 0.02) + (lt[RP] * 0.01) + (lt[MS] * 0.02) + (lt[HP] * 0.95) +
                                (rt[RS] * 0.02) + (rt[RP] * 0.01) + (rt[MS] * 0.02) + (rt[HP] * 0.95);
            retVal.dlRunDefenseRating = (re[RS] * 0.05) + (re[RP] * 0.05) + (re[MS] * 0.05) + (re[HP] * 0.85) +
                                        (nt[RS] * 0.05) + (nt[RP] * 0.05) + (nt[MS] * 0.05) + (nt[HP] * 0.85) +
                                        (le[RS] * 0.05) + (le[RP] * 0.05) + (le[MS] * 0.05) + (le[HP] * 0.85);
            retVal.dlPassDefenseRating =(re[RS] * 0.05) + (re[RP] * 0.05) + (re[MS] * 0.05) + (re[HP] * 0.75) + (re[PI] * 0.05) + (re[QU] * 0.05) +
                                        (nt[RS] * 0.05) + (nt[RP] * 0.05) + (nt[MS] * 0.05) + (nt[HP] * 0.75) + (nt[PI] * 0.05) + (nt[QU] * 0.05) +
                                        (le[RS] * 0.05) + (le[RP] * 0.05) + (le[MS] * 0.05) + (le[HP] * 0.75) + (le[PI] * 0.05) + (le[QU] * 0.05);
            retVal.lbRunDefenseRating = (rolb[RS] * 0.25) + (rolb[RP] * 0.25) + (rolb[MS] * 0.1) + (rolb[HP] * 0.4) +
                                        (rilb[RS] * 0.25) + (rilb[RP] * 0.25) + (rilb[MS] * 0.1) + (rilb[HP] * 0.4) +
                                        (lilb[RS] * 0.25) + (lilb[RP] * 0.25) + (lilb[MS] * 0.1) + (lilb[HP] * 0.4) +
                                        (lolb[RS] * 0.25) + (lolb[RP] * 0.25) + (lolb[MS] * 0.1) + (lolb[HP] * 0.4);
            retVal.lbPassDefenseRating = (rolb[RS] * 0.15) + (rolb[RP] * 0.15) + (rolb[MS] * 0.1) + (rolb[PI] * 0.3) + (rolb[QU] * 0.3) +
                                        (rilb[RS] * 0.15) + (rilb[RP] * 0.15) + (rilb[MS] * 0.1) + (rilb[PI] * 0.3) + (rilb[QU] * 0.3) +
                                        (lilb[RS] * 0.15) + (lilb[RP] * 0.15) + (lilb[MS] * 0.1) + (lilb[PI] * 0.3) + (lilb[QU] * 0.3) +
                                        (lolb[RS] * 0.15) + (lolb[RP] * 0.15) + (lolb[MS] * 0.1) + (lolb[PI] * 0.3) + (lolb[QU] * 0.3);
            retVal.dbRunDefenseRating = (rcb[RS] * 0.25) + (rcb[RP] * 0.25) + (rcb[MS] * 0.1) + (rcb[HP] * 0.4) +
                                        (lcb[RS] * 0.25) + (lcb[RP] * 0.25) + (lcb[MS] * 0.1) + (lcb[HP] * 0.4) +
                                        (fs[RS] * 0.25) + (fs[RP] * 0.25) + (fs[MS] * 0.1) + (fs[HP] * 0.4) +
                                        (ss[RS] * 0.25) + (ss[RP] * 0.25) + (ss[MS] * 0.1) + (ss[HP] * 0.4);
            retVal.dbPassDefenseRating = (rcb[RS] * 0.15) + (rcb[RP] * 0.15) + (rcb[MS] * 0.1) + (rcb[PI] * 0.3) + (rcb[QU] * 0.3) +
                                        (lcb[RS] * 0.15) + (lcb[RP] * 0.15) + (lcb[MS] * 0.1) + (lcb[PI] * 0.3) + (lcb[QU] * 0.3) +
                                        (fs[RS] * 0.15) + (fs[RP] * 0.15) + (fs[MS] * 0.1) + (fs[PI] * 0.3) + (fs[QU] * 0.3) +
                                        (ss[RS] * 0.15) + (ss[RP] * 0.15) + (ss[MS] * 0.1) + (ss[PI] * 0.3) + (ss[QU] * 0.3);
            #endregion
            return retVal;
        }

        /// <summary>
        /// Gets a player 'line' from m_Data from 'team' playing 'position'.
        /// </summary>
        /// <param name="team"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private int[] GetPlayerInts(string seasonChunk, string team, string position)
        {
            string pattern = "TEAM\\s*=\\s*" + team;
            Regex findTeamRegex = new Regex(pattern);
            Match m = findTeamRegex.Match(seasonChunk);
            if (m != Match.Empty)
            {
                int teamIndex = m.Index;
                if (teamIndex == -1)
                    return null;
                int playerIndex = -1;
                Regex endLineRegex = new Regex(string.Format("\n\\s*{0}\\s*,", position));
                Match eol = endLineRegex.Match(seasonChunk, teamIndex);
                if (eol != Match.Empty)
                    playerIndex = eol.Index;
                playerIndex++;

                if (playerIndex == 0)
                    return null;
                int lineEnd = seasonChunk.IndexOf("\n", playerIndex);
                string playerLine = seasonChunk.Substring(playerIndex, lineEnd - playerIndex);
                return InputParser.GetInts(playerLine, false);
            }
            return null;
        }

        private List<string> GetTeams(int season)
        {
            string seasonChunk= GetSeasonText(season);
            List<string> retVal = new List<string>(35);
            Regex teamRegex = new Regex("TEAM\\s*=\\s*([a-z0-9]+)");
            MatchCollection mc = teamRegex.Matches(seasonChunk);
            foreach (Match m in mc)
            {
                string team = m.Groups[1].Value;
                retVal.Add(team);
            }
            return retVal;
        }

        private string GetSeasonText(int season)
        {
            int start = 0; int end = TextData.Length;
            Regex reg = new Regex(String.Format("^\\s*SEASON\\s+({0})", season));
            Match m = reg.Match(TextData);
            if (m.Success)
            {
                int index = TextData.IndexOf("SEASON", m.Index + 50);
                if (index > -1)
                    end = index;
            }
            string seasonChunk = TextData.Substring(start, end);
            return seasonChunk;
        }

    }

    // holding place for a teams ratings (according to Tecmonster's formulas)
    public class TeamRatings
    {
        public string team {get;set;}
        public double qbRating {get; set;}
        public double rb1Rating {get; set;}
        public double rb2Rating {get; set;}
        public double wr1Rating {get; set;}
        public double wr2Rating {get; set;}
        public double teRating {get; set;}
        public double olRating {get; set;}
        public double dlRunDefenseRating {get;set;}
        public double dlPassDefenseRating {get;set;}
        public double lbRunDefenseRating {get;set;}
        public double lbPassDefenseRating {get;set;}
        public double dbRunDefenseRating {get;set;}
        public double dbPassDefenseRating {get;set;}

        public double totalRunD { get; set; }
        public double totalPassD { get; set; }
    }

    // used for convience; holds the average ratings 
    public struct SimAverages
    {
        public double QB_ave {get; set;}
        public double RB_ave {get; set;}
        public double WR_ave {get; set;}
        public double TE_ave {get; set;}
        public double OL_ave {get; set;}
        public double DL_run_ave {get; set;}
        public double DL_pass_ave {get; set;}
        public double LB_run_ave {get; set;}
        public double LB_pass_ave {get; set;}
        public double DB_run_ave {get; set;}
        public double DB_pass_ave {get; set;}

        public double TOTAL_RUN_DEFENSE  {get; set;}
        public double TOTAL_PASS_DEFENSE {get; set;}

        public double MIN_RUN_DEFENSE  { get; set; }
        public double MAX_RUN_DEFENSE  { get; set; }
        public double MIN_PASS_DEFENSE { get; set; }
        public double MAX_PASS_DEFENSE { get; set; } 
    }
}