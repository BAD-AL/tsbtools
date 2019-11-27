using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TSBTool
{
    /// <summary>
    /// Summary description for SchedulerHelper2.
    /// </summary>
    public class ScheduleHelper2
    {
        protected int weekOneStartLoc      = 0x329db;
        protected int end_schedule_section = 0x3400e;
		protected int gamesPerWeekStartLoc = 0x329c9;
        protected int weekPointersStartLoc = 0x329a7; // you need to swap these bytes
		private int[] teamGames;
		protected int total_games_possible = 238;
		protected int gamePerWeekLimit = 14;
		protected int totalGameLimit = 224;
		protected int totalWeeks = 17;

        private int week             = -1;
        private int week_game_count  =  0;
        private int total_game_count =  0;

		private ArrayList messages;
        private byte[] outputRom;
        private Regex gameRegex;

		protected virtual void AddMessage(String message)
		{
			if( message != null && message.Length > 0 )
				messages.Add(message);
		}

		/// <summary>
		/// 
		/// </summary>
		public int TotalGameCount
		{
			get{ return total_game_count; }
		}

        public ScheduleHelper2(byte[] outputRom)
        {
            this.outputRom = outputRom;
            gameRegex = new Regex("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");
        }

        /// <summary>
        /// Applies a schedule to the rom.
        /// </summary>
        /// <param name="lines">the contents of the schedule file.</param>
        public void ApplySchedule(ArrayList lines)
        {
			week             = -1;
            week_game_count  =  0;
            total_game_count =  0;
			messages         = new ArrayList(50);

            string line;
            for(int i =0; i < lines.Count; i++)
            {
                line = lines[i].ToString().Trim().ToLower();
				try
				{
					if( line.StartsWith("#") || line.Length < 3)
					{ // do nothing.
					}
					else if(line.StartsWith("week"))
					{
						if(week > totalWeeks-1 /*17*/)
						{
							AddMessage("Error! You can have only 17 weeks in a season.");
							break;
						}
						SetupWeek();
						Console.Error.WriteLine("Scheduleing {0}",line);
					}
					else 
					{
						ScheduleGame(line);
					}
				}
				catch(Exception e)
				{
					Console.Error.WriteLine("Exception! with line '{0}' {1}\n{2}",line, e.Message, e.StackTrace);
					AddMessage(string.Format("Error on line '{0}'", line));
				}
            }
			ClosePrevWeek(); // close off last week.
			if( week < totalWeeks-1 )
			{
				AddMessage("Warning! You didn't schedule all 17 weeks. The schedule could be messed up.");
			}
			if( teamGames != null)
			{
				for( int i = 0;  i < teamGames.Length; i++)
				{
					if( teamGames[i] != 16 ) 
					{
						AddMessage(string.Format(
							"Warning! The {0} have {1} games scheduled.", 
							TecmoTool.GetTeamFromIndex(i), teamGames[i] ));

					}
				}
			}
        }

        private void SetupWeek()
        {
            ClosePrevWeek();
            week++;
            total_game_count += week_game_count;
            week_game_count = 0;
            SetupPointerForCurrentWeek();
        }

        private void ClosePrevWeek()
        {
			if( week > -1 )
			{
				int location = gamesPerWeekStartLoc + week;
				outputRom[location] = (byte) week_game_count;
				if( week_game_count == 0)
				{
					AddMessage(string.Format("ERROR! Week {0}. You need at least 1 game in each week.", week+1));
				}
			}
        }

        private void SetupPointerForCurrentWeek()
        {
            if( week == 0) 
                return;
            int val      = ( 2 * total_game_count) + 0x89cb;
            int location = weekPointersStartLoc + (week * 2);
			if( week < 17 )
			{
				outputRom[location+1]   = (byte) (val >> 8);
				outputRom[location] = (byte) (val & 0x00ff);
			}
			else
			{
				AddMessage(string.Format("ERROR! To many Weeks {0}",week +1));
			}
        }

		/// <summary>
		/// Attempts to schedule a game.
		/// </summary>
		/// <param name="awayTeam">Away team's name.</param>
		/// <param name="homeTeam">Home team's name.</param>
		/// <returns> true on success, false on failure.</returns>
		protected virtual bool ScheduleGame(string awayTeam, string homeTeam)
		{
			bool ret = false;
			int awayIndex = TecmoTool.GetTeamIndex(awayTeam);
			int homeIndex = TecmoTool.GetTeamIndex(homeTeam);

			if( awayIndex == -1 || homeIndex == -1)
			{
				AddMessage(string.Format("Error! Week {2}: Game '{0} at {1}'", awayTeam, homeTeam, week+1));
				return false;
			}

			if( awayIndex == homeIndex )
			{
				AddMessage(string.Format(
					"Warning! Week {0}: The {1} are scheduled to play against themselves.",week+1, awayTeam ));
			}
			
			int location = weekOneStartLoc + ((week_game_count + total_game_count) * 2);
			if( location >= weekOneStartLoc && location < end_schedule_section )
			{
				outputRom[location]   = (byte) awayIndex;
				outputRom[location+1] = (byte) homeIndex;
				IncrementTeamGames(awayIndex);
				IncrementTeamGames(homeIndex);
				ret = true;
			}
			return ret;
		}


		/// <summary>
		/// Attempts to schedule a game.
		/// </summary>
		/// <param name="line"></param>
		/// <returns>True on success, false on failure.</returns>
        private bool ScheduleGame(string line)
        {
			bool ret = false;
            Match m = gameRegex.Match(line);
            string awayTeam, homeTeam;

            if( m != Match.Empty )
            {
                awayTeam = m.Groups[1].ToString();
                homeTeam = m.Groups[2].ToString();
				if( week_game_count > gamePerWeekLimit - 1 )
				{
					AddMessage(string.Format(
						"Error! Week {0}: You can have no more than {1} games in a week.",week+1, gamePerWeekLimit));
					ret = false;
				}
				else if( ScheduleGame(awayTeam, homeTeam) )
				{
					week_game_count++;
					ret = true;
				}
				else
				{
					//AddMessage(string.Format("Error scheduling game '{0}' for week {1}.", line, week+1));
				}
            }
			if( total_game_count + week_game_count > totalGameLimit )
			{
				AddMessage(string.Format(
					"Warning! Week {0}: There are more than {1} games scheduled.",week+1,gamePerWeekLimit));
			}
			return ret;
        }

        /// <summary>
        /// Gets the Schedule.
        /// </summary>
        /// <returns></returns>
        public string GetSchedule()
        {
            StringBuilder sb = new StringBuilder(17*28*12);
            for( int i =0; i < 17; i++)
            {
                sb.Append(string.Format("WEEK {0}\n",(i+1)));
                sb.Append(GetWeek(i)+"\n");
            }
            return sb.ToString();
        }

		/// <summary>
		/// Gets the schedule for week 'week'.
		/// </summary>
		/// <param name="week">The week to get.(Zero-based)</param>
		/// <returns>The week as a string. </returns>
        public string GetWeek(int week)
        {
			if( week < 0 || week > totalWeeks-1 )
			{
				AddMessage("Programming Error! 'GetWeek' Week must be in the range 0-16.");
				return null;
			}

            StringBuilder sb = new StringBuilder(14*12);
            int gamesInWeek = GetGamesInWeek(week);
            //sb.Append(gamesInWeek + " games \r\n");
            int prevGames = 0;
            for(int i = 0; i < week; i++)
            {
                prevGames += GetGamesInWeek(i);
            }
            int gameLocation = weekOneStartLoc + ( 2 * prevGames );
            for(int i = 0; i < gamesInWeek; i++)
            {
                sb.Append(string.Format("{0}", GetGame( gameLocation +(2*i) ) ) );
            }
            return sb.ToString();
        }

		/// <summary>
		/// Returns 
		/// </summary>
		/// <param name="romLocation"></param>
		/// <returns></returns>
        public string GetGame( int romLocation )
        {
			// TODO fix the upperbound.
			if( romLocation < weekOneStartLoc /*|| romLocation > weekOneStartLoc + 450*/)
			{
				AddMessage(string.Format(
                 "Programming ERROR! GetGame Invalid Game Location '0x{0}'. Valid locations are 0x{1}-0x{2}.",
					romLocation,weekOneStartLoc, weekOneStartLoc+448 ) );
				return null;
			}
            int away = outputRom[romLocation];
            int home = outputRom[romLocation+1];

			string awayTeam = TecmoTool.GetTeamFromIndex(away);
			string homeTeam = TecmoTool.GetTeamFromIndex(home);

            string ret = string.Format("{0} at {1}\n",
                    awayTeam, homeTeam);
            return ret;
        }

		/// <summary>
		/// Returns the number of games in the given week.
		/// </summary>
		/// <param name="week"></param>
		/// <returns></returns>
        public int GetGamesInWeek(int week)
        {
			if( week < 0 || week > totalWeeks-1)
			{
				AddMessage(string.Format("Programming Error! GetGamesInWeek Week {0} is invalid. Week range = 0-16.",
					week));
				return -1;
			}
            int result = outputRom[gamesPerWeekStartLoc+week];
            return result;
        }

		/// <summary>
		/// Set a geme in a week.
		/// To be called by the user.
		/// </summary>
		/// <param name="week"></param>
		/// <param name="game"></param>
		/// <param name="awayTeam"></param>
		/// <param name="homeTeam"></param>
		/// <returns></returns>
		public bool SetGame( int week, int game, string awayTeam, string homeTeam)
		{
			if( week < 1 || week > totalWeeks )
			{
				AddMessage("Error! valid week range is 1-17.");
				return false;
			}
			week--;
			int gamesInweek = GetGamesInWeek(week);
			if( game > gamesInweek || game < 1 )
			{
				AddMessage(string.Format(
                     "Error! Game Number invalid. Current range for week {0} is 1 - {1}",
					week+1, gamesInweek));
				return false;
			}
			int awayIndex = TecmoTool.GetTeamIndex(awayTeam);
			int homeIndex = TecmoTool.GetTeamIndex(homeTeam);

			if( awayIndex < 0 || homeIndex < 0 )
			{
				AddMessage(string.Format(
					"Error! Team name invalid. Couldn't schedule game '{0} at {1}'",
					awayTeam, homeTeam));
				return false;
			}
			// figure out week pinter
			int pointerLocation = weekPointersStartLoc + (2 * week );
			// TODO: finish this method.
			return false;
		}

		private void IncrementTeamGames(int teamIndex)
		{
			if( teamGames == null )
				teamGames = new int[TecmoTool.Teams.Count];
			teamGames[teamIndex]++;
		}

		/// <summary>
		/// Returns an arraylist of error messages encountered when processing the schedule data.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetErrorMessages()
		{
			return messages;
		}
    }
}
