using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace TSBTool2
{
    public class TSB1Tool
    {

        const int ORIG_NES_TSB1_LEN = 0x60010;
        const int CXROM_V105_LEN = 0x80010;
        const int CXROM_V111_LEN = 0xc0010;
        const int SNES_TSB1_LEN = 0x180000; 

        public static bool IsTecmoSuperBowl1Rom(byte[] rom)
        {
            bool retVal = false;
            if (rom != null)
            {
                switch (rom.Length)
                {
                    case ORIG_NES_TSB1_LEN:
                    case CXROM_V105_LEN:
                    case CXROM_V111_LEN:
                    case SNES_TSB1_LEN:
                        retVal = true;
                        break;
                }
            }
            return retVal;
        }

        private static Process process = null;
        public static string GetTSB1Content(string filename)
        {
            string stdout = null;
            string stderr = null;
            string ret = null;
            if (filename.Contains(" "))
                filename = string.Format("\"{0}\"", filename);

            cleanupProcess();
            process = new Process();
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = Path.GetFullPath("TSBToolSupreme.exe");
            process.StartInfo.Arguments = filename;
            process.StartInfo.WorkingDirectory = ".";
            //process = Process.Start(programExecName, argument );

            //process.WaitForExit();
            try
            {
                process.Start();
                stdout = process.StandardOutput.ReadToEnd();
            }
            catch { }
            try
            {
                stderr = process.StandardError.ReadToEnd();
            }
            catch { }

            if (stdout != null && stdout != "")
            {
                ret = stdout;
                if (stderr != null && stderr.IndexOf("Error") > -1 || stderr.IndexOf("Warning") > -1)
                    MessageBox.Show(stderr);
            }
            else if (stderr != null && stderr != "")
                ret = stderr;
            else
                ret = null;

            return ret;
        }

        private static void cleanupProcess()
        {
            if (process != null && !process.HasExited)
            {
                try
                {
                    process.Kill();
                }
                catch { }
            }
        }
    }
}
