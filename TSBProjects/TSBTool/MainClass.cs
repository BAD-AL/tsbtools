using System;
using System.Collections;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace TSBTool
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class MainClass
	{
        //[DllImport("kernel32.dll")]
        //static extern IntPtr GetConsoleWindow();

        //[DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);
        /// <summary>
        /// Used for string comprison in testing 
        /// </summary>
        public static String TestString = "";

		public static bool GUI_MODE = false;
        public static string version //= "Version 1.1.0.1";
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

		//                   -j             -n     -f     -a         -s         -sch    
		private static bool jerseyNumbers, names, faces, abilities, simData,  schedule, 
		//  -gui  -stdin
			gui,  stdin, proBowl; 
		private static bool printStuff, modifyStuff, printHelp;
		private static string outFileName = "output.nes";
		private static string getFileName = null;

        public static bool OnWindows { get; set; }
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] stuff)
		{
            RunMain(stuff);
		}

        public static void RunMain(string[] stuff)
        {
            //GUI_MODE = true;
            //MainGUI.PromptForSetUserInput("SET(0x2224B, {32TeamNES,28TeamNES PromptUser:Msg=\"Enter desired quarter length\":int(0x1-0x15)} )");

            OnWindows = true;
            if (System.Environment.OSVersion.ToString().IndexOf("windows", StringComparison.OrdinalIgnoreCase) < 0)
            {
                OnWindows = false;
            }
            jerseyNumbers = names = faces = abilities = simData =
            printStuff = modifyStuff = schedule = proBowl = 
              TecmoTool.ShowColors = TecmoTool.ShowPlaybook = 
              TecmoTool.ShowTeamFormation = false;
            //Junk(stuff);
            ArrayList args = GetArgs(stuff);
            ArrayList options = GetOptions(stuff);
            SetupOptions(options);
            string romFile = GetRomFileName(args);
            if (romFile != null && romFile.ToLower().EndsWith(".smc"))
            {
                outFileName = "output.smc";
            }
            else
            {
                outFileName = "output.nes";
            }
            string dataFile = GetInputFileName(args);

            if (stuff.Length == 0 || gui)
            {
                MainClass.GUI_MODE = true;
                //if (OnWindows)
                //    ShowWindow(GetConsoleWindow(), WindowShowStyle.Hide);
                Application.Run(new MainGUI(romFile, dataFile));
                //if (OnWindows)
                //    ShowWindow(GetConsoleWindow(), WindowShowStyle.Show);

                return;
            }
            if (printHelp)
            {
                PrintHelp();
                return;
            }

            if (stdin)
                dataFile = null; // if the romFile is null, we'll read from stdin.

            if (romFile == null)
                return;
            if (getFileName != null && File.Exists(getFileName))
            {
                ITecmoTool tool = TecmoToolFactory.GetToolForRom(romFile);
                string result = GetLocations(getFileName, tool.OutputRom);
                Console.WriteLine(result);
                return;
            }
            if (options.Count == 0 && romFile != null)
            {
                printStuff = true;
                jerseyNumbers = names = faces = abilities = simData = printStuff = schedule = proBowl = true;
            }
            else if (jerseyNumbers || names || faces || abilities || simData || printStuff || schedule || proBowl ||
                TecmoTool.ShowColors || TecmoTool.ShowPlaybook || TecmoTool.ShowTeamFormation)
                printStuff = true;

            try
            {
                if (printStuff)
                    PrintStuff(romFile);
                else if (romFile != null && dataFile != null)
                    ModifyStuff(romFile, dataFile);
                else if (romFile != null)
                    ModifyStuff(romFile, null);
                else
                    Console.Error.WriteLine("Exiting...");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR!\n" + e.Message);
            }
        }

		public static void PrintHelp()
		{
		//   -j             -n     -f     -a         -s   
		//	-sch    -gui  -stdin
			Console.WriteLine(string.Format(
@"{0}

Modifies and extracts info from Tecmo Superbowl nes ROM.

Usage: TSBToolSupreme (<tecmorom.nes>|<tecmorom.smc>) [data file] [options]
This program can extract data from and import data into a Tecmo Super Bowl rom (nes version only).

The default behavior when called with a nes filename and no options is to print all player 
and schedule inormation from the given TSB rom file.

When called with a TSB rom file and a data file, the behavior is that it will modify the TSB file
with the data contained in the data file.

The following are the available options.

-j		Print jersey numbers.
-n		Print player names.
-f		Print player face attribute.
-a		Print player abilities (running speed, rushing power, ...).
-s		Print player sim data.
-sch		Print schedule.
-stdin		Read data from standard in.
-gui		Launch GUI.
-pb		Show Playbooks
-proBowl    Show Pro bowl rosters.
-of		Show Offensive Formations
-colors		Show Uniform Colors
-out:filename	Save modified rom to <filename>.
-get:filename   Use <filename> as a 'GetBytes' file (get rom locations specified in file, print to stdout)
",MainClass.version)
				);
		}

		private static ArrayList GetOptions(string[] args)
		{
			ArrayList ret = new ArrayList();
			for(int i = 0 ; i < args.Length; i++)
			{
				if(args[i].StartsWith("-") || args[i].StartsWith("/"))
					ret.Add(args[i]);
			}
			return ret;
		}

		private static ArrayList GetArgs(string[] args)
		{
			ArrayList ret = new ArrayList();
			for(int i = 0 ; i < args.Length; i++)
			{
				if(!args[i].StartsWith("-") && !args[i].StartsWith("/"))
					ret.Add(args[i]);
			}
			return ret;
		}

		private static void SetupOptions(ArrayList options)
		{
			string option="";
			// player options = number,name,face,abilitits, sim data
			for(int i =0;i < options.Count; i++)
			{
				option = options[i].ToString().ToLower();
				if(option.StartsWith("-out:") || option.StartsWith("/out:") && option.Length > 5)
				{
					char[] seps = { ':' };
					string[] parts = option.Split(seps);
					if(parts != null && parts.Length > 1 && parts[1].Length > 1)
						outFileName = parts[1];
				}
				else if( option.StartsWith("-get:") || option.StartsWith("/get:"))
				{
					char[] seps = { ':' };
					string[] parts = option.Split(seps);
					if(parts != null && parts.Length > 1 && parts[1].Length > 1)
						getFileName = parts[1];
				}
				else
				{
					switch(option)
					{
						case "-j": case "/j":
							jerseyNumbers=true;
							break;
						case "-n": case "/n":
							names = true;
							break;
						case "-f": case "/f":
							faces=true;
							break;
						case "-a": case "/a":
							abilities =true;
							break;
						case "-s": case "/s":
							simData=true; // for players
							break;
						case "-sch": case "/sch":
							schedule=true;
							break;
						case "-h": case "/h": case "/?": case "-?":
							printHelp=true;
							break;
						case "-gui": case "/gui":
							gui = true;
							break;
						case "-stdin": case "/stdin":
							stdin = true;
							break;
						case "-pb": case "/pb":
							TecmoTool.ShowPlaybook = true;
							options.Remove(option);
							i--;
							break;
						case "-of": case "/of":
							TecmoTool.ShowTeamFormation = true;
							i--;
							options.Remove(option);
							break;
						case "-colors": case "/colors":
							TecmoTool.ShowColors = true;
							break;
                        case "-probowl": case "/probowl":
                            proBowl = true;
                            break;
						default:
							Console.Error.WriteLine("Invalid option '{0}'",option);
							break;
					}
				}
			}
			if( jerseyNumbers || names || faces || abilities || simData || proBowl)
				printStuff = true;
		}

		private static string GetRomFileName(ArrayList args)
		{
			string arg="";
			for(int i =0; i < args.Count; i++)
			{
				arg=args[i].ToString().ToLower();
				if( (arg.EndsWith(".nes")|| arg.EndsWith(".smc")) && ! arg.StartsWith("-out:"))
					return args[i].ToString();
			}
			Console.Error.WriteLine("No valid rom file passed as an argument.");
			return null;
		}

		private static string GetInputFileName(ArrayList args)
		{
			string arg="";
			for(int i =0; i < args.Count; i++)
			{
				arg=args[i].ToString().ToLower();
				if(!arg.EndsWith(".nes")&& !arg.EndsWith(".smc") )
					return args[i].ToString();
			}
			Console.Error.WriteLine("No valid input file passed as an argument.");
			return null;
		}

		private static void PrintStuff(string filename)
		{
			ITecmoTool tool = TecmoToolFactory.GetToolForRom( filename); 
			if( tool == null )
			{
				ShowError("ERROR determining ROM type.");
				return;
			}
			StringBuilder stuff = new StringBuilder(77000);
			if(jerseyNumbers && names && faces && abilities && simData )
			{
				tool.ShowOffPref = true;
				stuff.Append( tool.GetKey());
				stuff.Append( tool.GetAll());
			}
			else if( TecmoTool.ShowColors || TecmoTool.ShowPlaybook || TecmoTool.ShowTeamFormation )
			{
				tool.ShowOffPref = true;
				stuff.Append(tool.GetKey());
				stuff.Append(tool.GetAll());
				//stuff = tool.GetPlayerStuff(jerseyNumbers,names,faces,abilities,simData);
			}
			else if(jerseyNumbers || names || faces || abilities || simData)
			{
				stuff.Append( tool.GetPlayerStuff(jerseyNumbers,names,faces,abilities,simData));
			}
            if (proBowl)
            {
                stuff.Append(tool.GetProBowlPlayers());
            }
			if(schedule)
			{
				stuff.Append(tool.GetSchedule());
			}

			if(System.IO.Path.DirectorySeparatorChar == '\\')
			{
				stuff = stuff.Replace("\r\n", "\n");
				stuff = stuff.Replace("\n","\r\n");
			}
            TestString = stuff.ToString();
			Console.WriteLine(TestString);
			//string playerStuff = tool.GetPlayerStuff();
		}

		public static void ModifyStuff(string romfile, string inputfile)
		{
			ITecmoTool tt = TecmoToolFactory.GetToolForRom( romfile );

			InputParser parser  = new InputParser(tt);
			if(inputfile != null)
				parser.ProcessFile(inputfile);
			else
				parser.ReadFromStdin();
			tt.SaveRom(outFileName);
		}

        private static ArrayList sErrors = new ArrayList();

        public static void AddError(string error)
        {
            sErrors.Add(error);
        }

        public static void ClearErrors()
        {
            sErrors.Clear();
        }

		public static void ShowErrors()
		{
			if (sErrors != null && sErrors.Count > 0)
			{
				StringBuilder sb = new StringBuilder(500);
				foreach (string e in sErrors)
				{
					sb.Append(e + "\n");
				}
				ShowError(sb.ToString());
				ClearErrors();
			}
		}

		public static void ShowError( string error )
		{
			if( MainClass.GUI_MODE )
			{
				MessageBox.Show(null,error,"",MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
				Console.Error.WriteLine( error );
		}

		/// <summary>
		/// Syntax:
		/// 0x123456789abcd - 0x23456712222
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetLocations(string fileName, byte[] rom )
		{
			string ret = "";
			int defaultWidth= 0x10;
			Regex dude = new Regex(
					"0x([0123456789abcdefABCDEF]+)\\s*-\\s*0x([0123456789abcdefABCDEF]+)");
			
			Regex dude2 = new Regex(
"0x([0123456789abcdefABCDEF]+)\\s*-\\s*0x([0123456789abcdefABCDEF]+)\\s*,\\s*0x([0123456789abcdefABCDEF]+)");
			if( fileName != null && fileName.Length > 0)
			{
				StringBuilder sb = new StringBuilder( 500);
				StreamReader reader = new StreamReader(fileName);
				string contents = reader.ReadToEnd().Replace("\r\n","\n");
				reader.Close();
				string[] lines = contents.Split(new char[]{'\n'});
				Match m,m2;
				foreach(string line in lines)
				{
					if( (m2 = dude2.Match(line)) != Match.Empty )
					{
						long addr1 = long.Parse(m2.Groups[1].Value	,
							System.Globalization.NumberStyles.AllowHexSpecifier);
						long addr2 = long.Parse(m2.Groups[2].Value	, 
							System.Globalization.NumberStyles.AllowHexSpecifier);
						int w      =  int.Parse(m2.Groups[3].Value	, 
							System.Globalization.NumberStyles.AllowHexSpecifier);
						if( addr1 <= addr2 )
						{
							sb.Append(GetSetString(addr1, addr2, w ,rom));
						}
					}
					else if( (m = dude.Match(line)) != Match.Empty )
					{
						long addr1 = long.Parse(m.Groups[1].Value	,
							System.Globalization.NumberStyles.AllowHexSpecifier);
						long addr2 = long.Parse(m.Groups[2].Value	, 
							System.Globalization.NumberStyles.AllowHexSpecifier);
						if( addr1 <= addr2 )
						{
							sb.Append(GetSetString(addr1, addr2,defaultWidth ,rom));
						}
					}
					else if( line.StartsWith("#"))
					{
						sb.Append(line);
						sb.Append("\n");
					}
					else
					{
						sb.Append("#Problem with input line '");
						sb.Append(line);
						sb.Append("'\n");
						sb.Append("#correct format = <starting address>-<ending address>,byte width\n");
						sb.Append("#example: 0x12345-0x12355,0x10\n");
					}
				}
				ret = sb.ToString();
			}
			return ret;
		}

		private static string GetSetString(long addr1, long addr2,int width,  byte[] rom)
		{
			string ret = null;
			if( rom == null )
			{
				ShowError(string.Format(
					"ERROR! rom is null."));
			}
			else if( addr2 < addr1  )
			{
				ShowError(string.Format(
					"ERROR! ending'0x{1:X}' address greater than starting address'{0:X}'.",
					addr1, addr2));
			}
			else if(addr2 > rom.Length )
			{
				ShowError(string.Format(
					"ERROR! ending address '0x{0:X}'is out of range.",
					addr2));
			}
			else
			{
				StringBuilder sb = new StringBuilder( ((int)(addr2-addr1))*2+100);
				bool done = false;
				long len =0;
				long start = addr1;
				long end = addr2;
				int maxLinLen = width;

				while(!done)
				{
					if( end - start > maxLinLen)
					{
						end = start+maxLinLen;
						len = maxLinLen;
					}
					else
					{
						len = end-start;
					}
					sb.Append(string.Format("SET(0x{0:x},0x",start));
					for(long i = start; i < end; i++)
					{
						sb.Append(string.Format("{0:x2}",rom[i]));
					}
					sb.Append(")\n");
					start=end;

					if( end >= addr2)
						done = true;

					end+=maxLinLen;
					if(end > addr2)
						end = addr2+1;// otherwise we skip last byte
				}
				ret = sb.ToString();
			}
			return ret;
		}
		

	}


	public enum ROM_TYPE
	{
		NONE,
		NES,
		CXROM,
		SNES,
		READ_ONLY_ERROR
	}

	/// <summary>Enumeration of the different ways of showing a window using
	/// ShowWindow</summary>
	public enum WindowShowStyle : uint
	{
		/// <summary>Hides the window and activates another window.</summary>
		/// <remarks>See SW_HIDE</remarks>
		Hide = 0,
		/// <summary>Activates and displays a window. If the window is minimized
		/// or maximized, the system restores it to its original size and
		/// position. An application should specify this flag when displaying
		/// the window for the first time.</summary>
		/// <remarks>See SW_SHOWNORMAL</remarks>
		ShowNormal = 1,
		/// <summary>Activates the window and displays it as a minimized window.</summary>
		/// <remarks>See SW_SHOWMINIMIZED</remarks>
		ShowMinimized = 2,
		/// <summary>Activates the window and displays it as a maximized window.</summary>
		/// <remarks>See SW_SHOWMAXIMIZED</remarks>
		ShowMaximized = 3,
		/// <summary>Maximizes the specified window.</summary>
		/// <remarks>See SW_MAXIMIZE</remarks>
		Maximize = 3,
		/// <summary>Displays a window in its most recent size and position.
		/// This value is similar to "ShowNormal", except the window is not
		/// actived.</summary>
		/// <remarks>See SW_SHOWNOACTIVATE</remarks>
		ShowNormalNoActivate = 4,
		/// <summary>Activates the window and displays it in its current size
		/// and position.</summary>
		/// <remarks>See SW_SHOW</remarks>
		Show = 5,
		/// <summary>Minimizes the specified window and activates the next
		/// top-level window in the Z order.</summary>
		/// <remarks>See SW_MINIMIZE</remarks>
		Minimize = 6,
		/// <summary>Displays the window as a minimized window. This value is
		/// similar to "ShowMinimized", except the window is not activated.</summary>
		/// <remarks>See SW_SHOWMINNOACTIVE</remarks>
		ShowMinNoActivate = 7,
		/// <summary>Displays the window in its current size and position. This
		/// value is similar to "Show", except the window is not activated.</summary>
		/// <remarks>See SW_SHOWNA</remarks>
		ShowNoActivate = 8,
		/// <summary>Activates and displays the window. If the window is
		/// minimized or maximized, the system restores it to its original size
		/// and position. An application should specify this flag when restoring
		/// a minimized window.</summary>
		/// <remarks>See SW_RESTORE</remarks>
		Restore = 9,
		/// <summary>Sets the show state based on the SW_ value specified in the
		/// STARTUPINFO structure passed to the CreateProcess function by the
		/// program that started the application.</summary>
		/// <remarks>See SW_SHOWDEFAULT</remarks>
		ShowDefault = 10,
		/// <summary>Windows 2000/XP: Minimizes a window, even if the thread
		/// that owns the window is hung. This flag should only be used when
		/// minimizing windows from a different thread.</summary>
		/// <remarks>See SW_FORCEMINIMIZE</remarks>
		ForceMinimized = 11
	}
}
