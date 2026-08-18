using System;

namespace TSBTool
{
	/// <summary>
	/// Schedule helper for the 18-week/17-game CXRom-based ROM
	/// (e.g. SbluemanBase_18week_RealNFL-1.nes). Same 32-team CXRom layout and
	/// team index table, but the schedule data itself was relocated and expanded
	/// to hold 272 games (17 games x 32 teams / 2) across 18 weeks instead of the
	/// stock CXRom's 256 games across 17 weeks.
	///
	/// Locations below were determined by locating the known week 1 (seahawks at
	/// patriots ... chiefs at broncos) and week 18 (... cardinals at 49ers) games
	/// in the ROM directly, and cross-checking the resulting week-pointer table at
	/// 0x329a7 against the in-game "games per week" counts:
	/// [16, 16, 16, 16, 15, 14, 14, 14, 15, 14, 13, 16, 14, 15, 16, 16, 16, 16].
	/// </summary>
	public class CXRom18WeekScheduleHelper : CXRomScheduleHelper
	{
		public CXRom18WeekScheduleHelper(byte[] outputRom) : base(outputRom)
		{
			//weekPointersStartLoc = 0x329a7; // unchanged from CXRomScheduleHelper, now holds 18 entries
			gamesPerWeekStartLoc = 0x329cb;
			weekOneStartLoc      = 0x32d36;
			end_schedule_section = 0x32f56;
			weekPointerBaseConst = 0x8d26;
			totalWeeks           = 18;
			total_games_possible = 17 * 32 / 2; // 272
			gamePerWeekLimit     = 16;
			totalGameLimit       = 17 * 32 / 2; // 272
			gamesPerTeamExpected = 17;
		}
	}
}
