using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace TSBTool
{
	/// NOTE: byte swapping 
	///    0x12345678 
	///        |
	///        ---> 0x78563412
	///
	/// <summary>
	/// Summary description for ScheduleHelper.
	/// </summary>
	public class SNES_ScheduleHelper
	{
		private const int weekOneStartLoc = 0x15f3be;//0x329db;
		private int[] teamGames;

		int week, week_game_count,total_game_count;
		 private Regex gameRegex = new Regex("([0-9a-z]+)\\s+at\\s+([0-9a-z]+)");


		private int[] gamesPerWeek = 
			{14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14};
		private byte[] outputRom;
		
		public SNES_ScheduleHelper(byte[] outputRom)
		{
			this.outputRom = outputRom;
		}

		private void CloseWeek()
		{
			if( week > -1 )
			{
				int i = week_game_count;
				while( i < 14 )
				{
					ScheduleGame(0xff,0xff,week, i /*week_game_count*/);
					i++;
				}
			}
			week++;
			total_game_count += week_game_count;
			week_game_count = 0;
		}

		/// <summary>
		/// Applies a schedule to the rom.
		/// </summary>
		/// <param name="lines">the contents of the schedule file.</param>
		public void ApplySchedule(List<string> lines)
		{
			week             = -1;
			week_game_count  =  0;
			total_game_count =  0;

			if( SNES_TecmoTool.AUTO_CORRECT_SCHEDULE )
			{
				lines = Ensure18Weeks(lines);
			}

			string line;
			for(int i =0; i < lines.Count; i++)
			{
				line = lines[i].ToString().Trim().ToLower();
				if( line.StartsWith("#") || line.Length < 3)
				{ // do nothing.
				}
				else if(line.StartsWith("week"))
				{
					if(week > 18)
					{
						StaticUtils.AddError("Error! You can have only 18 weeks in a season.");
						break;
					}
					CloseWeek();
					StaticUtils.WriteError( string.Format("Scheduleing {0}",line));
				}
				else 
				{
					ScheduleGame(line);
				}
			}
			CloseWeek();// close week 18

			if( week < 18 )
			{
				StaticUtils.AddError("Warning! You didn't schedule all 18 weeks. The schedule could be messed up.");
			}
			if( teamGames != null)
			{
				for( int i = 0;  i < teamGames.Length; i++)
				{
					if( teamGames[i] != 16 )
					{
						StaticUtils.AddError(string.Format(
							"Warning! The {0} have {1} games scheduled.", 
							TecmoTool.GetTeamFromIndex(i), teamGames[i] ));
					}
				}
			}
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
				if( week_game_count > 13 )
				{
					StaticUtils.AddError(string.Format(
						"Error! Week {0}: You can have no more than 14 games in a week.",week+1));
					ret = false;
				}
				else if( ScheduleGame(awayTeam, homeTeam, week, week_game_count) )
				{
					week_game_count++;
					ret = true;
				}
				
			}
			if( total_game_count + week_game_count > 224 )
			{
				StaticUtils.AddError(string.Format(
					"Warning! Week {0}: There are more than 224 games scheduled.",week+1));
			}
			return ret;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="awayTeam"></param>
		/// <param name="homeTeam"></param>
		/// <param name="week">Week is 0-16 (0 = week 1).</param>
		/// <param name="gameOfWeek"></param>
		public bool ScheduleGame(string awayTeam, string homeTeam, int week, int gameOfWeek)
		{
			int awayIndex = TecmoTool.GetTeamIndex(awayTeam);
			int homeIndex = TecmoTool.GetTeamIndex(homeTeam);
			
			if( awayIndex == -1 || homeIndex == -1 )
			{
				StaticUtils.AddError(string.Format("Error! Week {2}: Game '{0} at {1}'", awayTeam, homeTeam, week+1));
				return false;
			}

			if( awayIndex == homeIndex && awayIndex < 28 )
			{
				StaticUtils.AddError(string.Format(
					"Warning! Week {0}: The {1} are scheduled to play against themselves.",week+1, awayTeam ));
			}

			if(week < 0 || week > 17){
				StaticUtils.AddError(string.Format("Week {0} is not valid. Weeks range 1 - 18.",week+1));
				return false;
			}
			if( GameLocation(week,gameOfWeek) < 0 ){
				StaticUtils.AddError(string.Format("Game {0} for week {1} is not valid. Valid games for week {1} are 0-{2}.",
					gameOfWeek,week,gamesPerWeek[week]-1));
				StaticUtils.AddError(string.Format("{0} at {1}",awayTeam, homeTeam));
			}

			ScheduleGame(awayIndex, homeIndex, week, gameOfWeek);

			if( awayTeam == "null" || homeTeam == "null")
				return false;
			return true;
		}

		private void ScheduleGame(int awayTeamIndex, int homeTeamIndex, int week, int gameOfWeek)
		{
			int location = GameLocation(week,gameOfWeek);
			if(location > 0)
			{
				outputRom[location]   = (byte)awayTeamIndex;
				outputRom[location+1] = (byte)homeTeamIndex;
				if( awayTeamIndex < 28)
				{
					IncrementTeamGames(awayTeamIndex);
					IncrementTeamGames(homeTeamIndex);
				}
			}
			/*else
			{
				StaticUtils.AddError(string.Format("INVALID game for ROM. Week={0} Game of Week ={1}",
					week,gameOfWeek);
			}*/
		}

		/// <summary>
		/// Returns a string like "49ers at giants", for a valid week, game of week combo.
		/// </summary>
		/// <param name="week">The week in question.</param>
		/// <param name="gameOfWeek">The game to get.</param>
		/// <returns>Returns a string like "49ers at giants", for a valid week, game of week combo, returns null
		/// upon error. </returns>
		public string GetGame(int week, int gameOfWeek)
		{
			int location = GameLocation(week,gameOfWeek);
			if(location == -1)
				return null ;
			int awayIndex = outputRom[location];
			int homeIndex = outputRom[location+1];
			string ret = "";

			if( awayIndex < 28 )
			{
				ret = string.Format("{0} at {1}", 
					TecmoTool.GetTeamFromIndex(awayIndex), 
					TecmoTool.GetTeamFromIndex(homeIndex));
			}
			return ret;
		}

		/// <summary>
		/// Returns a week from the season.
		/// </summary>
		/// <param name="week">The week to get [0-16] (0= week 1).</param>
		/// <returns></returns>
		public string GetWeek(int week)
		{
			if(week < 0 || week > gamesPerWeek.Length-1)
				return null;
			StringBuilder sb = new StringBuilder(20*14);
			sb.Append(string.Format("WEEK {0}\n",week+1));

			string game;

			for(int i = 0; i < gamesPerWeek[week]; i++)
			{
				game = GetGame(week,i);
				if( game != null && game.Length > 0 )
				{
					sb.Append(string.Format("{0}\n",game));
				}
			}
			sb.Append("\n");
			return sb.ToString();
		}

		public string GetSchedule()
		{
			StringBuilder sb = new StringBuilder(20*14*18);
			//sb.Append("Schedule\n\n");
			for(int week = 0; week < gamesPerWeek.Length; week++)
				sb.Append(GetWeek(week));

			return sb.ToString();
		}

		private int GameLocation(int week, int gameOfweek)
		{
			if( week < 0 || week > gamesPerWeek.Length-1 || 
				gameOfweek > gamesPerWeek[week] || gameOfweek < 0)
				return -1;

			int offset = 0;
			for(int i = 0; i < week; i++)
				offset += (gamesPerWeek[i]*2);

			offset += gameOfweek*2;
			int location = weekOneStartLoc+ offset;
			return location;
		}
		
		private void IncrementTeamGames(int teamIndex)
		{
			if( teamGames == null )
				teamGames = new int[28];
			//Console.WriteLine("IncrementTeamGames team index = "+teamIndex);
			if( teamIndex < teamGames.Length )
				teamGames[teamIndex]++;

		}

		private List<string> Ensure18Weeks(List<string> lines )
		{

			int wks = CountWeeks(lines);
			string line1, line2;
			for( int i = lines.Count-2; i > 0; i-=2 )
			{
				line1 = lines[i].ToString();
				line2 = lines[i+1].ToString();
				if(wks > 17)
				{
					break;
				}
				else if( line1.IndexOf("at") > -1 && line2.IndexOf("at") > -1 )
				{
					lines.Insert(i+1, "WEEK ");
					i--;
					wks++;
				}
			}

			//if( MainClass.GUI_MODE )
			//	ShowLines(lines);
			return lines;
		}

		private int CountWeeks(List<string> lines)
		{
			int count = 0;
			foreach(string line in lines)
			{
				if( line.ToLower().IndexOf("week") > -1)
					count++;
			}
			return count;
		}

	}
}
