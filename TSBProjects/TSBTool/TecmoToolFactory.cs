using System;

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

		public static ITecmoTool GetToolForRom(String fileName)
		{
			ITecmoTool tool = null;
			ROM_TYPE type = ROM_TYPE.NONE;
			try
			{
				type = CheckRomType(fileName);
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

            if (type == ROM_TYPE.CXROM_v105 || type == ROM_TYPE.CXROM_v111)
			{
				TecmoTool.Teams = new String[] 
					{
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
                CXRomTSBTool cxt = new CXRomTSBTool(fileName, type);
                tool = cxt;
                // Hack here; There are some ROMS out there whose SIZE == CXROM_v111 and are work better as CXROM_v105
                if (type == ROM_TYPE.CXROM_v111)
                {
                    string test = cxt.GetName("49ers", "QB1");
                    if (test == null)
                        tool = new CXRomTSBTool(fileName, ROM_TYPE.CXROM_v105);
                }
			}
			else if( type == ROM_TYPE.SNES_TSB1 )
			{
				TecmoTool.Teams = new String[] {
				"bills",   "colts",  "dolphins", "patriots",  "jets",
				"bengals", "browns", "oilers",   "steelers",
				"broncos", "chiefs", "raiders",  "chargers",  "seahawks",
				"cowboys", "giants", "eagles",   "cardinals", "redskins",
				"bears",   "lions",  "packers",  "vikings",   "buccaneers",
				"falcons", "rams",   "saints",   "49ers"
				  };
				tool = new SNES_TecmoTool(fileName);
			}
			else
			{
                TecmoTool.Teams = new String[]
					{
						"bills",   "colts",  "dolphins", "patriots",  "jets",
						"bengals", "browns", "oilers",   "steelers",
						"broncos", "chiefs", "raiders",  "chargers",  "seahawks",
						"redskins","giants", "eagles",   "cardinals", "cowboys",
						"bears",   "lions",  "packers",  "vikings",   "buccaneers",
						"49ers",   "rams",   "saints",   "falcons"
					};
                tool = new TecmoTool(fileName);
			}
			return tool;
		}

		/// <summary>
		/// returns 0 if regular NES TSB rom
		///         1 if it's cxrom TSBROM type.
		/// Throws exceptions (UnauthorizedAccessException and others)
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static ROM_TYPE CheckRomType(string fileName )
		{
			ROM_TYPE ret = ROM_TYPE.NONE;
			System.IO.FileStream s1=null;
			try
			{
				if( System.IO.File.Exists(fileName) )
				{
					System.IO.FileInfo f1 = new System.IO.FileInfo(fileName);
					long len = f1.Length;
                    if (len == ORIG_NES_TSB1_LEN)
                    {
                        ret = ROM_TYPE.NES_ORIGINAL_TSB;
                    }
                    else if (len == CXROM_V105_LEN)
					{
						ret = ROM_TYPE.CXROM_v105;
					}
                    else if (len == CXROM_V111_LEN)
                    {
                        ret = ROM_TYPE.CXROM_v111;
                    }
					else if( fileName.ToLower().EndsWith(".smc") && len == SNES_TSB1_LEN)
					{
						ret = ROM_TYPE.SNES_TSB1;
					}
				}
                Console.Error.WriteLine("ROM Type = " + ret.ToString());
			}
			finally
			{
				if( s1 != null )
					s1.Close();
			}
			return ret;
		}
	}
}
