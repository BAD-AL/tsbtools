using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Configuration;

// defect happens when saving player attributes for 49ers +
// need to find out where 49ers QB1 attributes start. -=> 0x3CDC
/*
 cxrom posted this a couple weeks ago:

this is all for the NFC-West:

the numbers in "()" are length per team and "[]" is total length for all 4 teams

0x199C1 - sim data ($30)[$C0]
0x23FF0 - large helmet palettes ($08)[$20]
0x27FDB - run/pass ratio ($01)[$04]
0x2CF82 - in game jersey colors ($0A)[$28]
0x348F7 - action sequence palettes ($08)[$20]
0x34953 - division champ screen palettes ($05)[$14]
0x349D2 - conference champ screen palettes ($04)[$10]

 */
namespace TSBTool
{
	/// <summary>
	/// Summary description for CXRomTSBTool.
	///   Still having problems with playbooks.
	///   
	/// Done:
	/// 1. Names and numbers
	/// 2. normal attributes.
	/// 3. Faces
	/// 4. player Sim attributes
	/// 5. team sim attributes
	/// 6. Team Playbooks.
	/// 7. team offensive preference.
	/// 8. team offensive formation
	/// </summary>
	public class CXRomTSBTool : TecmoTool
	{
		// extra teams (team index)
		// fortyNiners = 0x1E, rams = 0x1F, seahawks = 0x20, cardinals = 0x21

		// extra teams' name pointers occur at 0x3EB0
		// find out where Attributes are for new teams.
		
		private const int FORTY_NINERS_PLAYBOOK_START = 0x1D390;

		private bool DoSchedule = true;
		// player sim attributes
		//0x199BF - 0x199C6: pointers to sim attributes (8 bytes:2 each)
		//0x199C1 - 0x19A80: sim attributes (all teams)
		 int fortyNinersQB1SimAttrStart = 0x199C1;//0x199C7;
		 int fortyNinersRESimLoc = 0x199D9;//0x199DF;

		//const int fortyNinersRunPassPreferenceLoc = 0x27526; // defect
		private int fortyNinersRunPassPreferenceLoc   = 0x27fdb;
		
		private  int  FORTY_NINERS_QB1_POINTER   = 0x3eb0;

        public const int ROM_SIZE_v1_05 = 0x80010;
        public const int ROM_SIZE_v1_11 = 0xc0010; // 786448;
        /* v1.11 
         * if( outputRom.length == 786448){
         *   FORTY_NINERS_QB1_POINTER = 0x3e054
         *   mGetDataPositionOffset = 0x36010;
         * }
         * TODO:
         * Currently needs fixing v1.11: 
         *   1. Offensive formations (all)
         */
        private int mGetDataPositionOffset = 0x30000 + 0x010;
        private int FORTY_NINERS_KR_PR_LOC = 0x32cb2;
        private int FORTY_NINERS_KR_PR_LOC_1 = 0x3F93C;		
		private  int    m_ExpansionSegmentEnd = 0x3fff0;
		private byte[] m_RomVersionData = null;

        /// <summary>
        /// Returns the rom version 
        /// </summary>
        public override string RomVersion { 
            get
            {
                //if (ROM_LENGTH == ROM_SIZE_v1_11)
                //    return "32TeamNES_CXROM_v1.11";
                //else
                //    return "32TeamNES_CXROM_v1.05";
                return "32TeamNES";
            } 
        }

		public CXRomTSBTool()
		{
		}

		public CXRomTSBTool(string fileName)
		{
			Init(fileName);
		}

		private void SetupForCxROM()
		{
            if (outputRom.Length == ROM_SIZE_v1_11)
            {
                /* Version 1.11*/
                FORTY_NINERS_QB1_POINTER = 0x3e054;
                mGetDataPositionOffset = 0x36010;
                fortyNinersRunPassPreferenceLoc = 0x27fd6;
                ROM_LENGTH = ROM_SIZE_v1_11;
                //KR/PR 
                FORTY_NINERS_KR_PR_LOC = 0x32CC7;
                FORTY_NINERS_KR_PR_LOC_1 = 0x7FD51;
            }
            else
            {
                /* Version 1.05*/
                ROM_LENGTH = ROM_SIZE_v1_05;
            }

            mTeamFormationsStartingLoc = 0x3f940;
            namePointersStart = 0x48 + 12;
            lastPlayerNamePointer = 0x06e4;//0x6d9 + 12;
			
			try
			{
				string test = ConfigurationSettings.AppSettings["CXROM_ExpansionNameSegmentEnd"];
				if( test != null && test.Length > 1 )
				{
					m_ExpansionSegmentEnd = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_FORTY_NINERS_QB1_POINTER"];
				if( test != null && test.Length > 1 )
				{
					FORTY_NINERS_QB1_POINTER = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_FortyNinersQB1SimAttrStart"];
				if( test != null && test.Length > 1 )
				{
					fortyNinersQB1SimAttrStart = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}

				// Colors stuff
				test = ConfigurationSettings.AppSettings["CXROM_FortyNinersUniformColorLoc"];
				if( test != null && test.Length > 1 )
				{
					mFortyNinersUniformLoc    = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_FortyNinersActionSeqColorLoc"];
				if( test != null && test.Length > 1 )
				{
					mFortyNinersActionSeqLoc   = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_FortyNinersDivChampColorLoc"];
				if( test != null && test.Length > 1 )
				{
					m49ersDivChampLoc  = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_FortyiNersConfChampColorLoc"];
				if( test != null && test.Length > 1 )
				{
					m49ersConfChampLoc    = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_BillsDivChampColorLoc"];
				if( test != null && test.Length > 1 )
				{
					BillsDivChampLoc  = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				test = ConfigurationSettings.AppSettings["CXROM_BillsConfChampColorLoc"];
				if( test != null && test.Length > 1 )
				{
					BillsConfChampLoc    = Int32.Parse( test,
						System.Globalization.NumberStyles.AllowHexSpecifier);
				}
				//m49ersConfChampLoc = 0x349D2;
			}
			catch(Exception  )
			{
				MainClass.ShowError(
					"Error reading Config file options, do you have TSBToolSupreme.exe.config in the same directory?");
			}
			faceTeamOffsets= new int[]
				{
				0x3012, 0x3087, 0x30FC, 0x3171, 0x31E6,	0x325B, 0x32D0, 0x3345, 0x33BA, 0x342F, 0x34A4, 0x3519, 0x358e, 0x3603,
				0x384C, 0x36ed, 0x3762, 0x37D7, 0x3678, 0x38C1, 0x3936, 0x39AB, 0x3A20, 0x3A95, 0x3B0A, 0x3B7F,	0x3BF4, 0x3C69 

					,0x384C, 0x36ed, 0x3762, 0x37D7, 0x3678, 0x38C1
				};
		}

        bool mAddedFormationRomError = false;
		/// <summary>
		/// Sets the team's offensive formation.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="formation"></param>
		public override void SetTeamOffensiveFormation( string team, string formation)
		{
            if (outputRom.Length == ROM_SIZE_v1_11)
            {
                if (!mAddedFormationRomError)
                {
                    mAddedFormationRomError = true;
                    Errors.Add("Setting offensive formation on CXROM_v1.11 ROM is not yet supported.");
                }
                return;
            }
			int teamIndex = GetTeamIndex(team);
			if( teamIndex > -1 && teamIndex < 34 )
			{
				int location  = mTeamFormationsStartingLoc  + teamIndex;
//				int location2 = mTeamFormationsStartingLoc2 + teamIndex;

				switch( formation )
				{
					case m2RB_2WR_1TE:
						outputRom[location ] = (byte)0x00;
//						outputRom[location2] = (byte)0x00;
						break;
					case m1RB_3WR_1TE:
						outputRom[location ] = (byte)0x02;
//						outputRom[location2] = (byte)0x02;
						break;
					case m1RB_4WR:
						outputRom[location ] = (byte)0x01;
//						outputRom[location2] = (byte)0x01; 
						break;
					default:
						Errors.Add(string.Format(
							"ERROR! Formation {0} for team '{1}' is invalid.",formation, team));
						Errors.Add(string.Format("  Valid formations are:\n  {0}\n  {1}\n  {2}",
							m2RB_2WR_1TE, m1RB_3WR_1TE, m1RB_4WR ));
						break;
				}
			}
			else
			{
				Errors.Add(string.Format("ERROR! Team '{0}' is invalid, Offensive Formation not set",team));
			}
		}

        /**
         * 
         * @param len the length of the ROM
         * @return true if it's the correct length, false otherwise.
         */
        public override bool IsValidRomSize(long len)
        {
            bool ret = false;
            if( len == ROM_SIZE_v1_05 )
            {
                /* Version 1.05*/
                ret = true;
            }
            else if( len == ROM_SIZE_v1_11)
            {
                /* Version 1.11*/
                //FORTY_NINERS_QB1_POINTER = 0x3e054;
                //mGetDataPositionOffset = 0x36010;
                ret = true;
            }
            return ret;
        }

		public override bool ReadRom(string filename)
		{
			bool ret = false;
			ret = base.ReadRom (filename);
			if( ret )
			{
                SetupForCxROM();
				m_RomVersionData = new byte[14];
				for(int i = 0; i < m_RomVersionData.Length; i++)
				{
					m_RomVersionData[i] = outputRom[i+m_ExpansionSegmentEnd];
				}
			}
			return ret;
		}

		private bool CheckROMVersion()
		{
			bool ret = true;
			if( outputRom.Length > m_ExpansionSegmentEnd +20)
			{
				for(int i = 0; i < m_RomVersionData.Length; i++ )
				{
					if( outputRom[i+m_ExpansionSegmentEnd] != m_RomVersionData[i] )
					{

						ret = false;
						break;
					}
				}
			}
			return ret;
		}

		/// <summary>
		/// Check to see if we overwrote any ROM data after the end of the expansion
		/// name segment. If we are in GUI mode, prompt the user to confirm that they want to save the
		/// data.
		/// </summary>
		/// <param name="filename"></param>
		public override void SaveRom(string filename)
		{
			if( CheckROMVersion() )
			{
				base.SaveRom (filename);
			}
			else
			{
				MainClass.ShowError(
				"WARNING!! Expansion team name section has been overwritten, ROM could be messed up.");
				if( MainClass.GUI_MODE )
				{
					if( MessageBox.Show(null,  "ROM could be messed up, do you want to save anyway?", "ERROR!",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						base.SaveRom(filename);
					}
				}
				else
				{
					base.SaveRom(filename);
				}
			}
		}



		/// <summary>
		/// Gets the position who returns punts.
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public override string GetPuntReturner(string team)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				return base.GetPuntReturner(team);
			}
			else
			{
				string ret ="";
				int location = FORTY_NINERS_KR_PR_LOC + teamIndex-30;
				int b = outputRom[location];
				b = b & 0x0F;
				ret = positionNames[b];
				return ret;
			}
		}

		/// <summary>
		/// Gets the position who returns kicks.
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		public override string GetKickReturner(string team)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				return base.GetKickReturner(team);
			}
			else
			{
				string ret = "";
				int location = FORTY_NINERS_KR_PR_LOC + teamIndex-30;
				int b = outputRom[location];
				b = b & 0xF0;
				b = b >> 4;
				ret = positionNames[b];
				return ret;
			}
		}

		public override void SetPuntReturner(string team, string position)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				base.SetPuntReturner (team, position);
				return;
			}
			else
			{
				int location  = FORTY_NINERS_KR_PR_LOC   + GetTeamIndex(team)-30;
				int location1 = FORTY_NINERS_KR_PR_LOC_1 + GetTeamIndex(team)-30;
				switch(position)
				{
					case "QB1": case "QB2": case "C": case "LG": // these guys can return punts/kicks too.
					case "RB1": case "RB2": case "RB3": case "RB4": 
					case "WR1": case "WR2": case "WR3": case "WR4": 
					case "TE1": case "TE2":
						int pos = Index(positionNames,position);
						int b = outputRom[location];
						b = b & 0xF0;
						b = b + pos;
						outputRom[location] = (byte)b;
						outputRom[location1] = (byte)b;
						break;
					default:
						Errors.Add(string.Format("Cannot assign '{0}' as a punt returner",position));
						break;
				}
			}
		}

		
		public override void SetKickReturner(string team, string position)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				base.SetKickReturner (team, position);
				return;
			}
			else
			{
				int location = FORTY_NINERS_KR_PR_LOC + GetTeamIndex(team)-30;
				int location2 = FORTY_NINERS_KR_PR_LOC_1 + GetTeamIndex(team)-30;
				switch(position)
				{
					case "QB1": case "QB2": case "C": case "LG":  // these guys can return punts/kicks too.
					case "RB1": case "RB2": case "RB3": case "RB4": 
					case "WR1": case "WR2": case "WR3": case "WR4": 
					case "TE1": case "TE2":
						int pos = Index(positionNames,position);
						int b = outputRom[location];
						b = b & 0x0F;
						b = b + ( pos << 4);
						outputRom[location] = (byte)b;
						outputRom[location2] = (byte)b;
						break;
				
					default:
						Errors.Add(string.Format("Cannot assign '{0}' as a kick returner",position));
						break;
				}
			}
		}

		public override string GetAll()
		{
			string team;
			StringBuilder all = new StringBuilder(30*41*positionNames.Length);
			string year = string.Format("YEAR={0}\n",GetYear());
			all.Append(year);
			int normalTeamEnd = 28;
			for(int i = 0; i < normalTeamEnd; i++)
			{
				team = teams[i];
				all.Append(GetTeamPlayers(team));
			}
			string expansionTeams = GetExpansionTeams();
			all.Append(expansionTeams);

			return all.ToString();
		}

        public override string GetTeamName(int teamIndex)
        {
            string retVal = GetTeamStringTableString(teamIndex + 68);
            return retVal;
        }

        public override string GetTeamCity(int teamIndex)
        {
            string retVal = GetTeamStringTableString(teamIndex + 34);
            return retVal;
        }

        public override string GetTeamAbbreviation(int teamIndex)
        {
            string retVal = GetTeamStringTableString(teamIndex);
            return retVal;
        }

        public override void SetTeamAbbreviation(int teamIndex, string abb)
        {
            if (abb != null && abb.Length == 4)
            {
                SetTeamStringTableString(teamIndex, abb);
            }
            else
                Errors.Add(string.Format(
                    "Error setting team abbreviation, teamIndex={0}; value length must == 4; {1}",
                    teamIndex, abb));
        }

        public override void SetTeamName(int teamIndex, string name)
        {
            SetTeamStringTableString(teamIndex + 68, name);
        }

        public override void SetTeamCity(int teamIndex, string city)
        {
            SetTeamStringTableString(teamIndex + 34, city);
        }

        public override int NumberOfStringsInTeamStringTable { get { return 123; } }

        /// <summary>
        /// Offensive formation is messed up in CXROM 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public override string GetTeamOffensiveFormation(string team)
        {
            String retVal = "";
            if (outputRom.Length != ROM_SIZE_v1_11)
            {
                retVal = base.GetTeamOffensiveFormation(team);
            }
            return retVal;
        }

		private string GetExpansionTeams()
		{
			StringBuilder ret = new StringBuilder(2000);
			
			ret.Append(GetTeamPlayers(teams[30])); // fortyNiners
			ret.Append(GetTeamPlayers(teams[31])); // rams
			ret.Append(GetTeamPlayers(teams[32])); // seahawks
			ret.Append(GetTeamPlayers(teams[33])); // cardinals

			string result = ret.ToString();
			return result;
		}

		/// <summary>
		/// Sets the team sim offense tendency . 
		/// 00 = Little more rushing, 01 = Heavy Rushing, 
		/// 02 = little more passing, 03 = Heavy Passing. 
		/// </summary>
		/// <param name="team">the team name</param>
		/// <param name="val">the number to set it to.</param>
		/// <returns>true if set, fales if could not set it.</returns>
		public override bool SetTeamSimOffensePref(string team, int val)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
				return base.SetTeamSimOffensePref( team, val);

			if( val > -1 && val < 4 && teamIndex != -1)
			{
				int loc = fortyNinersRunPassPreferenceLoc + teamIndex - 30;
				outputRom[loc] = (byte) val;
			}
			else
			{
				if(teamIndex != -1)
					Errors.Add(string.Format("Can't set offensive pref to '{0}' valid values are 0-3.\n",val));
				else
					Errors.Add(string.Format("Team '{0}' is invalid\n",team));
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
		public override int GetTeamSimOffensePref(string team)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
				return base.GetTeamSimOffensePref(team);

			int val = -1;
			if( teamIndex > -1)
			{
				int loc = fortyNinersRunPassPreferenceLoc + teamIndex - 30;
				val = outputRom[loc];
			}
			else
			{
				Errors.Add(string.Format("Team '{0}' is invalid\n",team));
			}
			return val;
		}


		protected override int GetOffensivePlayerSimDataLocation(string team, string position)
		{
			int location = -4;

			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				location = base.GetOffensivePlayerSimDataLocation(team, position);
			}
			else if( teamIndex > 29 )
			{

				int positionIndex = GetPositionIndex(position);
				location = (teamIndex -30 )*teamSimOffset + (positionIndex*2) +fortyNinersQB1SimAttrStart;
			}
			return location;
		}

		protected override int GetDefinsivePlayerSimDataLocation(string team, string position)
		{
			int location = -4;
			
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				location = base.GetDefinsivePlayerSimDataLocation(team, position);
			}
			else if( teamIndex > 29 )
			{
				int positionIndex = GetPositionIndex(position);
				location = (teamIndex -30)* teamSimOffset + (positionIndex - 17)+ fortyNinersRESimLoc;
			}
			return location;
		}

		
		protected override int GetPunkKickSimDataLocation(int teamIndex)
		{
			int ret = -1;
			
			if( teamIndex < 28 )
				ret = base.GetPunkKickSimDataLocation(teamIndex);
			else
				ret = (teamIndex -30 )*teamSimOffset + fortyNinersQB1SimAttrStart+ 0x2E;

			return ret;
		}

		/// <summary>
		/// Returns the simulation data for the given team.
		/// Simulation data is of the form '0xNN' where N is a number 1-F (hex).
		/// A team's sim data of '0x57' signifies that the team has a simulation figure of
		/// '5' for offense, and '7' for defense.
		/// </summary>
		/// <param name="team">The team of interest</param>
		/// <returns></returns>
		public override byte GetTeamSimData(string team)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
				return base.GetTeamSimData(team);

			if( teamIndex > 29 && teamIndex < 34 )
			{
				int location = (teamIndex-30)*teamSimOffset + fortyNinersQB1SimAttrStart +0x2f;
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
		public override void SetTeamSimData(string team, byte values)
		{
			if( !IsValidTeam(team)  )
			{
				Errors.Add(string.Format("ERROR! (low level) SetTeamSimData:: team {0} is invalid ",team));
				return;
			}

			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				base.SetTeamSimData(team, values);
			}
			else
			{
				// not yet implemented in cxrom's rom
				int location = (teamIndex-30)*teamSimOffset + fortyNinersQB1SimAttrStart +0x2f;
				int currentValue = outputRom[location];
				outputRom[location] = values;
				currentValue = outputRom[location];
			}
		}
		/// <summary>
		/// Gets the point in the player number name data that a player's data begins.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public override int GetDataPosition(string team, string position)
		{
			int ret = -1;
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) GetDataPosition:: either team {0} or position {1} is invalid.", team, position));
			}
			int teamIndex     = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				return base.GetDataPosition(team, position );
			}
			if( teamIndex > 29 )
			{
				int positionIndex = GetPositionIndex(position);
				// the players total index (QB1 bills=0, QB2 bills=2 ...)
				int pointerLocation = 0;

                pointerLocation = (teamIndex - 30) * 0x3c + FORTY_NINERS_QB1_POINTER
                   + (positionIndex * 2);

				byte lowByte = outputRom[pointerLocation];
				int  hiByte  = outputRom[pointerLocation+1];
				hiByte =  hiByte << 8;
				hiByte = hiByte + lowByte;

				//int ret = hiByte - 0x8000 + 0x010;
				//ret = hiByte + 0x30000 + 0x010;
                ret = hiByte + mGetDataPositionOffset;
			}
			return  ret;
		}

		/// <summary>
		/// Get the starting point of the guy AFTER the one passed to this method.
		/// This is hacked up to work with CXROM's rom.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public override int GetNextDataPosition(string team, string position)
		{
			int pointerLocation = 0;
			int teamIndex = GetTeamIndex(team);

			if( teamIndex > 29 && position == "P" )
			{
                pointerLocation = FORTY_NINERS_QB1_POINTER + 0x3c + (teamIndex - 30) * 0x3c;
			}
			else if( teamIndex > 29 )
			{
				int positionIndex = GetPositionIndex(position)+1;
                pointerLocation = (teamIndex - 30) * 0x3c + FORTY_NINERS_QB1_POINTER
                    + (positionIndex * 2);
			}

			if( pointerLocation != 0 )
			{
				byte lowByte = outputRom[pointerLocation];
				int  hiByte  = outputRom[pointerLocation+1];
				hiByte =  hiByte << 8;
				hiByte = hiByte + lowByte;

				//int ret = hiByte - 0x8000 + 0x010;
                //int ret = hiByte + 0x30000 + 0x010;
                int ret = hiByte + mGetDataPositionOffset;
				return ret;
			}
			
			return base.GetNextDataPosition(team, position);
		}


		protected override int GetPointerPosition(string team, string position)
		{
			int ret = -4;
			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) GetPointerPosition:: either team {0} or position {1} is invalid.", team, position));
			}
			int teamIndex     = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				return base.GetPointerPosition(team, position);
			}
			if( teamIndex > 29 )
			{
				int positionIndex = GetPositionIndex(position);
				int playerSpot    = (teamIndex - 30 )*  positionNames.Length + positionIndex;
				if(positionIndex < 0)
				{
					Errors.Add(string.Format("ERROR! (low level) Position '{0}' does not exist. Valid positions are:",position));
					for(int i =1; i <= positionNames.Length; i++)
					{
						Console.Error.Write("{0}\t", positionNames[i-1]);
					}
					return -1;
				}
				ret = FORTY_NINERS_QB1_POINTER + (2*playerSpot);
			}
			return ret;
		}

		protected override void ShiftDataAfter(string team, string position, int shiftAmount)
		{
			int teamIndex = GetTeamIndex(team);
			if(teamIndex == 27 && position == "P")
				return;

			if( teamIndex < 28 )
			{
				base.ShiftDataAfter(team, position, shiftAmount);
				return;
			}

			if( !IsValidTeam(team) || !IsValidPosition( position ))
			{
				throw new Exception(
					string.Format("ERROR! (low level) ShiftDataAfter:: either team {0} or position {1} is invalid.", team, position));
			}

			if(team == teams[teams.Length-1] && position == "P")
				return;

			int startPosition = this.GetNextDataPosition(team,position);
			int endPosition = m_ExpansionSegmentEnd-17;// -17 to compensate for shifting down
			
			if(shiftAmount < 0)
				ShiftDataUp(startPosition, endPosition, shiftAmount, outputRom);
			else if(shiftAmount > 0)
				ShiftDataDown(startPosition, endPosition, shiftAmount, outputRom);
		}

		protected override void AdjustDataPointers(int pos, int change, int lastPointer)
		{
			if (pos >= GetTeamStringTableStart()) // doing Team String table stuff
			{
				base.AdjustDataPointers(pos, change, lastPointer);
				return;
			}
			if( pos == lastPlayerNamePointer-2)//0x06e2) // panther's punter
			{
				int pointerLoc = pos+2;
				byte lo = outputRom[pointerLoc];
				byte hi = outputRom[pointerLoc+1];
				int pVal = hi;
				pVal = pVal << 8;
				pVal += lo;
				pVal += change;

				lo  =  (byte)(pVal & 0x00ff);
				pVal =  pVal >> 8;
				hi   =  (byte)pVal;
				outputRom[pointerLoc]   = lo;
				outputRom[pointerLoc+1] = hi;
			}
			else if ( pos < lastPlayerNamePointer +1 )
			{
				base.AdjustDataPointers(pos, change, base.lastPlayerNamePointer);
			}
			else
			{
				byte low, hi;
				int  word;
				// last pointer is at 0x69d For NES
				// snes is lastpointer+1 (0x178738+1)

				int start = pos+2;
				int i=0;
				int end = //-1;
                        //CARDINALS_PUNTER_POINTER + 2; // cards are team #33
                        FORTY_NINERS_QB1_POINTER + 0x3c + (33 - 30) * 0x3c + 2;
				
				for( i = start; i < end; i+=2)
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
		}

		/// <summary>
		/// Get the face number from the given team/position
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public override int GetFace(string team, string position)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
				return base.GetFace(team, position);
			int positionOffset = GetPositionIndex(position);
			
			if(positionOffset < 0 || teamIndex < 0 )
			{
				Errors.Add(string.Format("GetFace Error getting face for {0} {1}",team,position));
				return -1;
			}
			teamIndex -= 2;
			int loc = 0x3012 + faceOffsets[positionOffset] + teamIndex*0x75;
			int ret = outputRom[loc];
			return ret;
		}

		/// <summary>
		/// Sets the face for the guy at position 'position' on team 'team'.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <param name="face"></param>
		public override void SetFace(string team, string position, int face)
		{
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				base.SetFace(team, position, face);
				return;
			}
			int positionOffset = GetPositionIndex(position);
			
			if(positionOffset < 0 || teamIndex < 0 || face < 0x00 | face > 0xD4 )
			{
				Errors.Add(string.Format("SetFace Error setting face for {0} {1} face={2}",team,position,face));
				if( face < 0x00 | face > 0xD4 )
					Errors.Add(string.Format("Valid Face numbers are 0x00 - 0xD4"));
				return ;
			}
			teamIndex -= 2;
			int loc = 0x3012 + faceOffsets[positionOffset] + teamIndex*0x75;
			outputRom[loc] = (byte)face;
		}

		protected override int GetAttributeLocation(int teamIndex, int posIndex)
		{
			int location = -1;
			if( teamIndex < 28 )
			{
				location = base.GetAttributeLocation (teamIndex, posIndex);
			}
			else
			{
				location = base.GetAttributeLocation (teamIndex-2, posIndex);
			}
			return location;
		}


		/// <summary>
		/// Returns an ArrayList of errors that were encountered during the operation.
		/// </summary>
		/// <param name="scheduleList"></param>
		/// <returns></returns>
		public override ArrayList ApplySchedule( ArrayList scheduleList )
		{
			if( scheduleList != null && outputRom != null )
			{
				CXRomScheduleHelper sch = new CXRomScheduleHelper( outputRom );
				sch.ApplySchedule( scheduleList );
				ArrayList errors = sch.GetErrorMessages();
				return errors;
			}
			return null;
		}

		protected override int GetPlaybookLocation(int team_index)
		{
			if( team_index  < 28 )
				return base.GetPlaybookLocation( team_index );
			else
			{
				team_index -= 30;
				return  FORTY_NINERS_PLAYBOOK_START + team_index * 4;
			}
		}

		public override string GetSchedule()
		{
			string ret = "";
			if( outputRom != null && DoSchedule )
			{
				CXRomScheduleHelper sh2 = new CXRomScheduleHelper( outputRom );
				ret = sh2.GetSchedule();
				ArrayList errors = sh2.GetErrorMessages();
				if( errors != null && errors.Count > 0 )
				{
					MainClass.ShowErrors( errors );
				}
			}

			return ret;
		}

		#region Uniform Color Stuff
		private int mFortyNinersUniformLoc = 0x2cf82;
		
		protected override int GetUniformLoc(string team)
		{
			int ret = -1;
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				ret = base.GetUniformLoc(team);
			}
			else
			{
				teamIndex -= 30;
				ret = mFortyNinersUniformLoc + (teamIndex * 0xa);
			}
			return ret;
		}

		private int mFortyNinersActionSeqLoc = 0x348f7;
		/// <summary>
		/// Gets the location of the given team's uniform data.
		/// </summary>
		/// <param name="team"></param>
		/// <returns>The location of the given team's uniform data, -1 on error</returns>
		protected override int GetActionSeqUniformLoc(string team)
		{
			int ret = -1;
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28 )
			{
				ret = base.GetActionSeqUniformLoc(team);
			}
			else
			{
				teamIndex -= 30;
				ret = mFortyNinersActionSeqLoc + (teamIndex * 0x8);
			}
			return ret;
		}

		private int m49ersDivChampLoc = 0x3494f;//; 0x34953;

		protected override int GetDivChampLoc(string team)
		{
			int ret = -1;
			int teamIndex = GetTeamIndex(team);
			if( teamIndex < 28)
			{
				ret = base.GetDivChampLoc(team);
			}
			else
			{
				teamIndex -= 30;
				ret = m49ersDivChampLoc + (teamIndex * 0x5);
			}
			
			return ret;
		}
		
		private int m49ersConfChampLoc = 0x349cf;//0x349D2;

		protected override int GetConfChampLoc(string team)
		{
			int ret = -1;
			int teamIndex = GetTeamIndex(team);

			if( teamIndex < 28)
			{
				ret = base.GetConfChampLoc(team);
			}
			else
			{
				teamIndex -= 30;
				ret = m49ersConfChampLoc + (teamIndex * 0x4);
			}
			return ret;
		}
		#endregion
	}
}
