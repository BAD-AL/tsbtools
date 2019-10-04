using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TSBTool2
{
    public class TSB1SimAutoUpdater
    {
        public static List<string> PositionNames = new List<string>() { 
				"QB1", "QB2", "RB1", "RB2",  "RB3",  "RB4",  "WR1",  "WR2", "WR3", "WR4", "TE1", 
				"TE2", "C",   "LG",  "RG",   "LT",   "RT",
				"RE", "NT",   "LE",  "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS",  "SS",  "K", "P" 
			};

        private List<String> mTeams = new List<string>();
        private SimStuff mSimStuff = new SimStuff();
        private string mData="";
		/// <summary>
		/// The text data to work on and retrieve.
		/// </summary>
        protected string Data { 
            get { return mData; } 
            set 
            {
                mTeams.Clear();
                mData = value;
                Regex findTeamRegex = new Regex("TEAM\\s*=\\s*([a-z49]+)");
                MatchCollection mc = findTeamRegex.Matches(mData);
                foreach (Match m in mc)
                {
                    mTeams.Add(m.Groups[1].ToString());
                }
            }
        }

		/// <summary>
		/// Gets a player 'line' from m_Data from 'team' playing 'position'.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		private string GetPlayerString( string team, string position )
		{
			string pattern = "TEAM\\s*=\\s*"+team;
			Regex findTeamRegex = new Regex(pattern);
			Match m = findTeamRegex.Match(mData);
			if( m != Match.Empty )
			{
				int teamIndex = m.Index;
				if( teamIndex == -1 )
					return null;
				int playerIndex = -1;
				Regex endLineRegex = new Regex(string.Format("\n\\s*{0}\\s*,",position));
				Match eol = endLineRegex.Match(mData, teamIndex);
				if( eol != Match.Empty )
					playerIndex = eol.Index;
				playerIndex++;
			
				if( playerIndex == 0 )
					return null;
				int lineEnd = mData.IndexOf("\n",playerIndex);
				string playerLine = mData.Substring(playerIndex,lineEnd-playerIndex);
				return playerLine;
			}
			return null;
		}

		private void ReplacePlayer(string team, string oldPlayer, string newPlayer)
		{
			int nextTeamIndex = -1;
			int currentTeamIndex= -1;
			string nextTeam    = null;

			Regex findTeamRegex = new Regex("TEAM\\s*=\\s*"+team);
			
			Match m = findTeamRegex.Match(mData);
			if( !m.Success )
				return;

			currentTeamIndex = m.Groups[1].Index;

			int test = mTeams.IndexOf(team);

			if( test != mTeams.Count - 1 )
			{
				nextTeam      = string.Format("TEAM\\s*=\\s*{0}",mTeams[test+1]);
				Regex nextTeamRegex = new Regex(nextTeam);
				Match nt = nextTeamRegex.Match(mData);
				if( nt.Success )
					nextTeamIndex = nt.Index;
			}
			if( nextTeamIndex < 0)
				nextTeamIndex = mData.Length;

			
			int playerIndex = mData.IndexOf(oldPlayer,currentTeamIndex);
			if( playerIndex > -1 )
			{
				int endLine     = mData.IndexOf('\n',playerIndex);
				string start    = mData.Substring(0,playerIndex);
				string last     = mData.Substring(endLine);
				
				StringBuilder tmp = new StringBuilder(mData.Length + 200);
				tmp.Append(start);
				tmp.Append(newPlayer);
				tmp.Append(last);

				mData = tmp.ToString();
			}
			else
			{
				string error = string.Format(
@"An error occured looking up player
     '{0}'
Please verify that this player's attributes are correct.", oldPlayer);
                StaticUtils.AddError(error);
			}
		}

        public static string AutoUpdatePlayerSimData(string input)
        {
            TSB1SimAutoUpdater tmp = new TSB1SimAutoUpdater();
            tmp.Data = input;
            tmp.AutoUpdatePlayerSim();
            return tmp.Data;
        }

		/// <summary>
		/// Update all players sim attributes.
		/// </summary>
		private void AutoUpdatePlayerSim()
		{
            foreach( string team in mTeams )
			{
				AutoUpdatePlayers( team );
			}
		}

		/// <summary>
		/// Auto update a team's players sim attributs.
		/// </summary>
		/// <param name="team"></param>
		private void AutoUpdatePlayers( string team )
		{
			// look for 'TEAM = ' here
			string pattern = "TEAM\\s*=\\s*"+team;
			Regex findTeamRegex = new Regex(pattern);
			Match m = findTeamRegex.Match(mData);
			if( m != Match.Empty )
			{
				foreach( string position in PositionNames)
				{
					if( position == "C")
						break;
					AutoUpdatePlayerSimData( team, position);
				}
				AutoUpdatePlayerSimData(team, "P");
				AutoUpdatePlayerSimData(team, "K");
				UpdateTeamSimPassDefense(team);
				UpdateTeamSimPassRush(team);
			}
		}

		/// <summary>
		/// Auto update a player's sim data.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		private void AutoUpdatePlayerSimData(string team, string position)
		{
			string oldValue = GetPlayerString(team, position);
			string newValue = null;

			if( oldValue == null || oldValue == string.Empty )
			{
				return;// he's not there, don't update him.
			}
			string fName = InputParser.GetFirstName(oldValue);
            string lName = InputParser.GetLastName(oldValue);
            int face = InputParser.GetFace(oldValue);
            int jerseyNumber = InputParser.GetJerseyNumber(oldValue);
			
			int[] attrs = new int[4];
			try
			{
                attrs = InputParser.GetInts(oldValue, false);
			}
			catch(Exception e)
			{
				MessageBox.Show("Oh oh!"+e.Message);
			}
			//int[] simData = m_Parser.GetSimVals(playerLine);
			int simPass, simRush, simPocket, simCatch, simPR, simKR, simKA;

			switch( position )
			{
				case "QB1": case "QB2":
					if( attrs != null && attrs.Length > 7)
					{
						simPass   = mSimStuff.SimPass(/*PC*/attrs[5], /*APB*/attrs[7], /*PS*/attrs[4]);
						simRush   = mSimStuff.QbSimRun(/*MS*/attrs[2]);
						simPocket = mSimStuff.SimPocket(/*MS*/attrs[2]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12} ,[{13}, {14}, {15} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5],attrs[6], attrs[7], simRush, simPass, simPocket); 
					}
					break;
				case "RB1": case "RB2": 
				case "RB3": case "RB4":
					if( attrs != null && attrs.Length > 5)
					{
						simRush  = mSimStuff.RbSimRush(/*MS*/attrs[2],/*HP*/attrs[3],/*BC*/attrs[4],/*RS*/attrs[0]);
						simCatch = mSimStuff.RbSimCatch(/*REC*/attrs[5]);
						simKR    = mSimStuff.SimKickRet(/*MS*/attrs[2]);
						simPR    = mSimStuff.SimPuntRet(/*MS*/attrs[2]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11}, {12} ,{13}, {14} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5],simRush, simCatch, simPR, simKR);
					}
					break;
				case "WR1": case "WR2": 
				case "WR3": case "WR4":
				case "TE1": case "TE2":
					if( attrs != null && attrs.Length > 5)
					{
						simRush  = mSimStuff.RbSimRush(/*MS*/attrs[2],/*HP*/attrs[3],/*BC*/attrs[4],/*RS*/attrs[0]);
						simCatch = mSimStuff.RbSimCatch(/*REC*/attrs[5]);
						simKR    = mSimStuff.SimKickRet(/*MS*/attrs[2]);
						simPR    = mSimStuff.SimPuntRet(/*MS*/attrs[2]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11}, {12} ,{13}, {14} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5],simRush, simCatch, simPR, simKR);
					}
					break;
				case "P": case "K":
					if( attrs != null && attrs.Length > 5)
					{
						simKA = mSimStuff.PKSimKick(/*KA*/attrs[4], /*AKB*/attrs[5]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5], simKA); 
					}
					break;
			}
			if( newValue != null )
			{
				ReplacePlayer(team, oldValue, newValue);
			}
		}

		private void UpdateTeamSimPassDefense(string team)
		{
			string re   = GetPlayerString(team, "RE");
			string le   = GetPlayerString(team, "LE");
			string nt   = GetPlayerString(team, "NT");
			string lolb = GetPlayerString(team, "LOLB");
			string lilb = GetPlayerString(team, "LILB");
			string rilb = GetPlayerString(team, "RILB");
			string rolb = GetPlayerString(team, "ROLB");
			string rcb  = GetPlayerString(team, "RCB");
			string lcb  = GetPlayerString(team, "LCB");
			string fs   = GetPlayerString(team, "FS");
			string ss   = GetPlayerString(team, "SS");

			if( re == null   || le == null   || nt == null   || 
				lolb == null || lilb == null || rilb == null || rolb == null ||
				rcb == null  || lcb == null || fs == null || ss == null )
			{
				// we need the entire defense in order to update this.
				return;
			}
			int[] reAttrs   = InputParser.GetInts(re, false);
            int[] leAttrs = InputParser.GetInts(le, false);
            int[] ntAttrs = InputParser.GetInts(nt, false);
            int[] lolbAttrs = InputParser.GetInts(lolb, false);
            int[] lilbAttrs = InputParser.GetInts(lilb, false);
            int[] rilbAttrs = InputParser.GetInts(rilb, false);
            int[] rolbAttrs = InputParser.GetInts(rolb, false);
            int[] rcbAttrs = InputParser.GetInts(rcb, false);
            int[] lcbAttrs = InputParser.GetInts(lcb, false);
            int[] fsAttrs = InputParser.GetInts(fs, false);
            int[] ssAttrs = InputParser.GetInts(ss, false);
			
			int[] passDef = mSimStuff.GetSimPassDefense(
				rolbAttrs[4], rilbAttrs[4], lilbAttrs[4], lolbAttrs[4],
				rcbAttrs[4],  lcbAttrs[4],  fsAttrs[4],   ssAttrs[4]);

			ReplacePlayer(team, re,   ReplaceSimAttr(re,   2,0));
			ReplacePlayer(team, nt ,  ReplaceSimAttr(nt,   2,0));
			ReplacePlayer(team, le ,  ReplaceSimAttr(le,   2,0));
			ReplacePlayer(team, rolb, ReplaceSimAttr(rolb, 2, passDef[0]));
			ReplacePlayer(team, rilb, ReplaceSimAttr(rilb, 2, passDef[1]));
			ReplacePlayer(team, lilb, ReplaceSimAttr(lilb, 2, passDef[2]));
			ReplacePlayer(team, lolb, ReplaceSimAttr(lolb, 2, passDef[3]));
			ReplacePlayer(team, rcb,  ReplaceSimAttr(rcb,  2, passDef[4]));
			ReplacePlayer(team, lcb,  ReplaceSimAttr(lcb,  2, passDef[5]));
			ReplacePlayer(team, fs,   ReplaceSimAttr(fs,   2, passDef[6]));
			ReplacePlayer(team, ss,   ReplaceSimAttr(ss,   2, passDef[7]));
		}
		
		private void UpdateTeamSimPassRush(string team)
		{
			string re   = GetPlayerString(team, "RE");
			string le   = GetPlayerString(team, "LE");
			string nt   = GetPlayerString(team, "NT");
			string lolb = GetPlayerString(team, "LOLB");
			string lilb = GetPlayerString(team, "LILB");
			string rilb = GetPlayerString(team, "RILB");
			string rolb = GetPlayerString(team, "ROLB");
			string rcb  = GetPlayerString(team, "RCB");
			string lcb  = GetPlayerString(team, "LCB");
			string fs   = GetPlayerString(team, "FS");
			string ss   = GetPlayerString(team, "SS");

			if( re == null   || le == null   || nt == null   || 
				lolb == null || lilb == null || rilb == null || rolb == null ||
				rcb == null  || lcb == null || fs == null || ss == null )
			{
				// we need the entire defense in order to update this.
				return;
			}
			int[] reAttrs   = InputParser.GetInts(re,false);
            int[] leAttrs = InputParser.GetInts(le, false);
            int[] ntAttrs = InputParser.GetInts(nt, false);
            int[] lolbAttrs = InputParser.GetInts(lolb, false);
            int[] lilbAttrs = InputParser.GetInts(lilb, false);
            int[] rilbAttrs = InputParser.GetInts(rilb, false);
            int[] rolbAttrs = InputParser.GetInts(rolb, false);
            int[] rcbAttrs = InputParser.GetInts(rcb, false);
            int[] lcbAttrs = InputParser.GetInts(lcb, false);
            int[] fsAttrs = InputParser.GetInts(fs, false);
            int[] ssAttrs = InputParser.GetInts(ss, false);
			
			int[] rushDef = mSimStuff.GetSimPassRush(
				reAttrs[2]+reAttrs[3], ntAttrs[2]+ntAttrs[3],
				leAttrs[2]+leAttrs[3],
				rolbAttrs[2]+rolbAttrs[3], rilbAttrs[2]+rilbAttrs[3], 
				lilbAttrs[2]+lilbAttrs[3], lolbAttrs[2]+lolbAttrs[3] );

			ReplacePlayer(team, re,   ReplaceSimAttr(re,   1, rushDef[0]));
			ReplacePlayer(team, nt ,  ReplaceSimAttr(nt,   1, rushDef[1]));
			ReplacePlayer(team, le ,  ReplaceSimAttr(le,   1, rushDef[2]));
			ReplacePlayer(team, rolb, ReplaceSimAttr(rolb, 1, rushDef[3]));
			ReplacePlayer(team, rilb, ReplaceSimAttr(rilb, 1, rushDef[4]));
			ReplacePlayer(team, lilb, ReplaceSimAttr(lilb, 1, rushDef[5]));
			ReplacePlayer(team, lolb, ReplaceSimAttr(lolb, 1, rushDef[6]));
			ReplacePlayer(team, rcb,  ReplaceSimAttr(rcb,  1, 0));
			ReplacePlayer(team, lcb,  ReplaceSimAttr(lcb,  1, 0));
			ReplacePlayer(team, fs,   ReplaceSimAttr(fs,   1, 0));
			ReplacePlayer(team, ss,   ReplaceSimAttr(ss,   1, rushDef[7]));

		}

		Regex m_SimRegex = null;
		/// <summary>
		/// replaces the sim attribute specified.
		/// </summary>
		/// <param name="line">Like: 
		/// "LOLB, trev ALBERTS, Face=0x26, #51, 25, 31, 31, 31, 38, 31 ,[30, 20 ]"</param>
		/// <param name="num">1 -> '30', 2->'20' above.</param>
		/// <param name="newValue">the new value</param>
		/// <returns>The input string with the specified replacement.</returns>
		private string ReplaceSimAttr(string line, int num, int newValue)
		{
			string ret = line;
			if( m_SimRegex == null)
			{
				m_SimRegex = new Regex("\\[\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*\\]");
			}
			Match m = m_SimRegex.Match(line);
			if( m != null )
			{
				int index = m.Groups[num].Index;
				int len   = m.Groups[num].ToString().Length;
				ret = string.Format("{0}{1}{2}", 
					line.Substring(0, index),
					newValue,
					line.Substring(index+len) );
			}
			return ret;
		}
    }

    /// <summary>
    /// Summary description for SimStuff.
    /// </summary>
    public class SimStuff
    {
        public const int FRONT_7_SIM_POINT_POOL = 200;
        public const int FRONT_7_MIN_SIM_PASS_RUSH = 13;

        public SimStuff()
        {
        }

        /// <summary>
        /// Returns the SimPocket value when passed the QB's
        /// MS.
        /// </summary>
        /// <param name="MS"></param>
        /// <returns></returns>
        public int SimPocket(int MS)
        {
            int ret = 0;
            switch (MS)
            {
                case 100:
                case 94:
                case 88:
                case 81:
                case 75:
                case 69:
                case 63:
                case 56:
                case 50:
                    ret = 0;
                    break;
                case 44:
                case 38:
                    ret = 1;
                    break;
                case 31:
                case 25:
                    ret = 2;
                    break;
                default:
                    ret = 3;
                    break;
            }
            return ret;
        }

        public int SimPass(int PC, int APB, int PS)
        {
            int ret = 0;

            if (PC > 75)
                ret = 13;
            else if (PC > 44)
                ret = (PS + PC + APB) / 17;
            else
                ret = (PC + APB) / 14;
            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int QbSimRun(int MS)
        {
            int ret = MS / 5;
            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int SimKickRet(int MS)
        {
            int ret = MS / 4;
            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int SimPuntRet(int MS)
        {
            int ret = MS / 4;
            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int RbSimCatch(int REC)
        {
            int ret = 0;
            if (REC > 44)
                ret = REC / 6;
            else
                ret = REC / 10;

            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int RbSimRush(int MS, int HP, int BC, int RS)
        {
            int ret = 0;
            if (HP < 50)
                ret = ((MS + BC) / 11) - 2;
            else
                ret = (RS + HP) / 15;
            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int WrTeSimCatch(int REC)
        {
            int ret = REC / 6;
            if (ret > 15)
                ret = 15;
            return ret;
        }

        public int WrTeSimRush()
        {
            return 2;
        }

        public int PKSimKick(int KA, int AKB)
        {
            int ret = (KA + (AKB / 2)) / 11;
            if (ret > 15)
                ret = 15;
            return ret;
        }


        /// <summary>
        /// Use PI 
        /// </summary>
        /// <param name="rolbInts"></param>
        /// <param name="rilbInts"></param>
        /// <param name="lilbInts"></param>
        /// <param name="lolbInts"></param>
        /// <param name="rcbInts"></param>
        /// <param name="lcbInts"></param>
        /// <param name="fsInts"></param>
        /// <param name="ssInts"></param>
        /// <returns></returns>
        public int[] GetSimPassDefense(
            int rolbInts, int rilbInts, int lilbInts, int lolbInts,
            int rcbInts, int lcbInts, int fsInts, int ssInts)
        {
            double totalInts = rolbInts + rilbInts + lilbInts + lolbInts + rcbInts + lcbInts + fsInts + ssInts;
            double totalSimPoints = 254;
            int rolbPoints, rilbPoints, lilbPoints, lolbPoints, rcbPoints, lcbPoints, fsPoints, ssPoints;

            rolbPoints = (int)((rolbInts / totalInts) * totalSimPoints);
            rilbPoints = (int)((rilbInts / totalInts) * totalSimPoints);
            //lilbPoints = (int)((lilbInts / totalInts )* totalSimPoints);
            lolbPoints = (int)((lolbInts / totalInts) * totalSimPoints);
            rcbPoints = (int)((rcbInts / totalInts) * totalSimPoints);
            lcbPoints = (int)((lcbInts / totalInts) * totalSimPoints);
            fsPoints = (int)((fsInts / totalInts) * totalSimPoints);
            ssPoints = (int)((ssInts / totalInts) * totalSimPoints);

            lilbPoints = 1 + (int)(totalSimPoints
                -
                (rcbPoints + lcbPoints + fsPoints + rolbPoints + ssPoints +
                rilbPoints + lolbPoints));

            int[] ret = new int[8];
            ret[0] = rolbPoints;
            ret[1] = rilbPoints;
            ret[2] = lilbPoints;
            ret[3] = lolbPoints;
            ret[4] = rcbPoints;
            ret[5] = lcbPoints;
            ret[6] = fsPoints;
            ret[7] = ssPoints;

            return ret;
        }


        /// <summary>
        /// use HP instead of sacks
        /// </summary>
        /// <param name="reSacks"></param>
        /// <param name="ntSacks"></param>
        /// <param name="leSacks"></param>
        /// <param name="rolbSacks"></param>
        /// <param name="rilbSacks"></param>
        /// <param name="lilbSacks"></param>
        /// <param name="lolbSacks"></param>
        /// <param name="playerData"></param>
        /// <returns></returns>

        public int[] GetSimPassRush(
            double reSacks, double ntSacks, double leSacks, double rolbSacks,
            double rilbSacks, double lilbSacks, double lolbSacks)
        {
            double totalSacks = reSacks + ntSacks + leSacks + rolbSacks + rilbSacks + lilbSacks + lolbSacks;

            int totalSimPoints = FRONT_7_SIM_POINT_POOL;
            int minPr = FRONT_7_MIN_SIM_PASS_RUSH;

            int rePoints, ntPoints, lePoints, rolbPoints, rilbPoints, lilbPoints, lolbPoints, ssPoints;
            int dbPoints = 0;
            int cbPoints = 0;
            int front7Points = 0;

            if (totalSacks == 0)
            {
                rePoints = ntPoints = lePoints = rolbPoints = rilbPoints = lilbPoints = lolbPoints = ssPoints = 31;
                rePoints += 4;
            }
            else
            {
                rePoints = Math.Max((int)((reSacks / totalSacks) * totalSimPoints), minPr);
                lePoints = Math.Max((int)((leSacks / totalSacks) * totalSimPoints), minPr);
                ntPoints = Math.Max((int)((ntSacks / totalSacks) * totalSimPoints), minPr);
                rolbPoints = Math.Max((int)((rolbSacks / totalSacks) * totalSimPoints), minPr);
                rilbPoints = Math.Max((int)((rilbSacks / totalSacks) * totalSimPoints), minPr);
                lilbPoints = Math.Max((int)((lilbSacks / totalSacks) * totalSimPoints), minPr);
                lolbPoints = Math.Max((int)((lolbSacks / totalSacks) * totalSimPoints), minPr);

                front7Points = rePoints + lePoints + ntPoints + rolbPoints +
                    rilbPoints + lilbPoints + lolbPoints;

                dbPoints = (int)(255 - front7Points);

                cbPoints = dbPoints / 4;
                ssPoints = (int)(255 - ((3 * cbPoints) + front7Points));
            }
            int[] ret = new int[8];

            ret[0] = rePoints;
            ret[1] = ntPoints;
            ret[2] = lePoints;
            ret[3] = rolbPoints;
            ret[4] = rilbPoints;
            ret[5] = lilbPoints;
            ret[6] = lolbPoints;
            ret[7] = ssPoints;

            return ret;
        }

        public int GetSimOffense(int QB1SimPass,
            int RB1SimRush, int RB2SimRush,
            int WR1SimCatch, int WR2SimCatch)
        {
            int f1, f2;
            if (RB1SimRush > RB2SimRush)
                f1 = RB1SimRush;
            else
                f1 = RB2SimRush;
            if (WR1SimCatch > WR2SimCatch)
                f2 = WR1SimCatch;
            else
                f2 = WR2SimCatch;

            int ret = (QB1SimPass + f1 + f2) / 3;
            return ret;
        }


    }
}
