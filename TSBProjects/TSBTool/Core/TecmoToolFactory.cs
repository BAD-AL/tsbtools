using System;
using System.Collections.Generic;

namespace TSBTool
{
	/// <summary>
	/// Summary description for TecmoToolFactory.
	/// </summary>
	public class TecmoToolFactory
	{
        public const int ORIG_NES_TSB1_LEN = 0x60010;
        public const int CXROM_V105_LEN    = 0x80010;
        public const int CXROM_V111_LEN    = 0xc0010;
        public const int SNES_TSB1_LEN     = 0x180000;

		public static ITecmoContent GetToolForRom(byte[] rom)
		{
            ITecmoContent tool = null;
			ROM_TYPE type = ROM_TYPE.NONE;
			try
			{
				type = CheckRomType( rom);
			}
			catch( UnauthorizedAccessException )
			{
				type = ROM_TYPE.READ_ONLY_ERROR;
				StaticUtils.ShowError("ERROR opening ROM, Please check ROM to make sure it's not 'read-only'.");
				return null;
			}
			catch(Exception e)
			{
				StaticUtils.ShowError(string.Format("ERROR determining ROM type. Exception=\n{0}\n{1}",
					e.Message,e.StackTrace));
				return null;
			}

            if (type == ROM_TYPE.CXROM_v105 || type == ROM_TYPE.CXROM_v111 || type == ROM_TYPE.CXROM_18WEEK)
			{
				TecmoTool.Teams = new List<string>() {
					"bills",     "dolphins", "patriots", "jets",
					"bengals",    "browns",  "ravens",   "steelers",
					"colts",      "texans",  "jaguars",  "titans",
					"broncos",    "chiefs",  "raiders",  "chargers",  
					"redskins",   "giants",  "eagles",   "cowboys",
					"bears",      "lions",   "packers",  "vikings",   
					"buccaneers", "saints",  "falcons",  "panthers",
					 
					"AFC",     "NFC",
					"49ers",   "rams", "seahawks",   "cardinals"
				};
                CXRomTSBTool cxt = new CXRomTSBTool(rom, type);
                tool = cxt;
                // Hack here; There are some ROMS out there whose SIZE == CXROM_v111 and are work better as CXROM_v105
                if (type == ROM_TYPE.CXROM_v111)
                {
                    string test = cxt.GetName("49ers", "QB1");
                    if (test == null)
                        tool = new CXRomTSBTool(rom, ROM_TYPE.CXROM_v105);
                }
			}
			else if( type == ROM_TYPE.SNES_TSB1 )
			{
				TecmoTool.Teams = new List<string>(){
				    "bills",   "colts",  "dolphins", "patriots",  "jets",
				    "bengals", "browns", "oilers",   "steelers",
				    "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
				    "cowboys", "giants", "eagles",   "cardinals", "redskins",
				    "bears",   "lions",  "packers",  "vikings",   "buccaneers",
				    "falcons", "rams",   "saints",   "49ers"
				  };
				tool = new SNES_TecmoTool(rom);
			}
            else if (type == ROM_TYPE.SNES_TSB2)
            {
                tool = new TSBTool2.TSB2Tool(rom);
            }
            else if (type == ROM_TYPE.SNES_TSB3)
            {
                tool = new TSBTool2.TSB3Tool(rom);
            }
            else
            {
                TecmoTool.Teams = new List<string>() {
				    "bills",   "colts",  "dolphins", "patriots",  "jets",
				    "bengals", "browns", "oilers",   "steelers",
				    "broncos", "chiefs", "raiders",  "chargers",  "seahawks",
				    "redskins","giants", "eagles",   "cardinals", "cowboys",
				    "bears",   "lions",  "packers",  "vikings",   "buccaneers",
				    "49ers",   "rams",   "saints",   "falcons"
				};
                tool = new TecmoTool(rom);
            }
			return tool;
		}

		/// <summary>
		/// returns 0 if regular NES TSB rom
		///         1 if it's cxrom TSBROM type.
		/// Throws exceptions (UnauthorizedAccessException and others)
		/// </summary>
		/// <param name="rom"></param>
		/// <returns></returns>
		public static ROM_TYPE CheckRomType(byte[] rom)
		{
			ROM_TYPE ret = ROM_TYPE.NONE;
			System.IO.FileStream s1=null;
			try
			{
                long len = rom.Length;
                if (len == ORIG_NES_TSB1_LEN)
                {
                    ret = ROM_TYPE.NES_ORIGINAL_TSB;
                }
                else if (len == CXROM_V105_LEN)
				{
                    // Some 18-week/17-game schedule ROMs (e.g. based on SbluemanBase_18week_RealNFL-1.nes)
                    // are built on the CXRom base and happen to be the same length as stock CXROM_v105.
                    // Distinguish them by checking whether the schedule's week-pointer table is internally
                    // consistent with the 18-week schedule layout.
                    ret = IsCXRom18WeekSchedule(rom) ? ROM_TYPE.CXROM_18WEEK : ROM_TYPE.CXROM_v105;
				}
                else if (len == CXROM_V111_LEN)
                {
                    ret = ROM_TYPE.CXROM_v111;
                }
				else if(len == SNES_TSB1_LEN)
				{
					ret = ROM_TYPE.SNES_TSB1;
				}
                else if (TSBTool2.TSB2Tool.IsTecmoSuperBowl2Rom(rom))
                {
                    ret = ROM_TYPE.SNES_TSB2;
                }
                else if (TSBTool2.TSB3Tool.IsTecmoSuperBowl3Rom(rom))
                {
                    ret = ROM_TYPE.SNES_TSB3;
                }
                StaticUtils.WriteError("ROM Type = " + ret.ToString());
			}
			finally
			{
				if( s1 != null )
					s1.Close();
			}
			return ret;
		}

        /// <summary>
        /// Checks whether the ROM's schedule data matches the 18-week/17-game schedule layout
        /// used by ROM_TYPE.CXROM_18WEEK (see CXRom18WeekScheduleHelper), as opposed to the stock
        /// 17-week/16-game CXROM_v105 layout. Both layouts share the same size (CXROM_V105_LEN) and
        /// the same weekPointersStartLoc, so this can't be determined from length alone.
        ///
        /// The check reads the candidate "games per week" byte array and verifies that every stored
        /// week-pointer value equals (2 * cumulativeGamesBeforeWeek) + weekPointerBaseConst. This is
        /// an arithmetic identity across 17 independent pointer entries, so it's extremely unlikely to
        /// hold by coincidence against an unrelated/stock ROM.
        /// </summary>
        /// <param name="rom">the raw rom bytes</param>
        /// <returns>true if the ROM's schedule matches the 18-week layout</returns>
        private static bool IsCXRom18WeekSchedule(byte[] rom)
        {
            const int weekPointersStartLoc = 0x329a7;
            const int gamesPerWeekStartLoc = 0x329cb;
            const int totalWeeks           = 18;
            const int weekPointerBaseConst = 0x8d26;
            const int gamePerWeekLimit     = 16;

            if (rom.Length < weekPointersStartLoc + (totalWeeks * 2))
                return false;

            int[] cumGamesBeforeWeek = new int[totalWeeks];
            int cumulative = 0;
            for (int week = 0; week < totalWeeks; week++)
            {
                int gamesInWeek = rom[gamesPerWeekStartLoc + week];
                if (gamesInWeek < 1 || gamesInWeek > gamePerWeekLimit)
                    return false;
                cumGamesBeforeWeek[week] = cumulative;
                cumulative += gamesInWeek;
            }

            for (int week = 1; week < totalWeeks; week++)
            {
                int location = weekPointersStartLoc + (week * 2);
                int storedPointer = rom[location] | (rom[location + 1] << 8);
                int expectedPointer = (2 * cumGamesBeforeWeek[week]) + weekPointerBaseConst;
                if (storedPointer != expectedPointer)
                    return false;
            }

            return true;
        }
	}
}
