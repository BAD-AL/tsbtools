using System;

namespace TSBTool
{
	/// <summary>
	/// Summary description for CXRomScheduleHelper.
	/// </summary>
	public class CXRomScheduleHelper : ScheduleHelper2
	{
		public CXRomScheduleHelper(byte[] outputRom) : base(outputRom)
		{
			    //weekOneStartLoc      = 0x329db;
				end_schedule_section = 0x3400e;
				//gamesPerWeekStartLoc = 0x329c9;
				weekPointersStartLoc = 0x329a7;
				total_games_possible = 16*16;
				gamePerWeekLimit = 16;
				totalGameLimit = 16*16;
				//totalWeeks = 17;
		}

		protected override void AddMessage(String message)
		{
			if( message.IndexOf("AFC") == -1 && message.IndexOf("NFC") == -1 )
			{
				base.AddMessage (message);
			}
		}


		protected override bool ScheduleGame(string awayTeam, string homeTeam)
		{
			bool ret = false;
			if( TotalGameCount < total_games_possible )
			{
				ret = base.ScheduleGame (awayTeam, homeTeam);
			}
			else
			{
				AddMessage(string.Format(
					"ERROR! maximum game limit reached ({0}) {1} at {1} will not be scheduled",
					total_games_possible ,awayTeam, homeTeam));
			}
			return ret;
		}


	}
}
