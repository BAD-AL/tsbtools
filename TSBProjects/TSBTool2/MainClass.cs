using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TSBTool2
{
	public static class MainClass
	{
        public static string version 
        {
            get
            {
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
        {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
