using System;
using System.Collections.Generic;
using System.IO;
using TSBTool;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TSBTool_Test
{
    /// <summary>
    /// Loads the fixture files under TSBTool_Test\ROMs, \Schedules, and \FullRomDumps. These are
    /// copied next to the test assembly at build time (CopyToOutputDirectory), so a plain
    /// BaseDirectory-relative lookup finds them. Legacy MSTest must be run with deployment
    /// disabled (see NoDeploy.testsettings) since its runner doesn't honor [DeploymentItem]
    /// in this environment and would otherwise look for them in the wrong folder.
    /// </summary>
    internal static class TestRoms
    {
        /// <summary>
        /// TecmoTool.Teams is a static, process-wide list with an internal setter, so it can only be
        /// assigned from within the TSBTool assembly. Route through TecmoToolFactory.GetToolForRom
        /// (which sets it as a side effect of detecting a CXRom-family ROM) so tests that talk to a
        /// ScheduleHelper directly don't depend on static state left over from a previous test.
        /// </summary>
        public static void EnsureCXRomTeamsAreSet(byte[] cxRomFamilyRom)
        {
            TecmoToolFactory.GetToolForRom(cxRomFamilyRom);
        }

        /// <summary>
        /// Same idea as EnsureCXRomTeamsAreSet, but for the stock 28-team ROM family
        /// (ROM_TYPE.NES_ORIGINAL_TSB, e.g. TSPRBOWL.nes).
        /// </summary>
        public static void EnsureStockTeamsAreSet(byte[] stockRom)
        {
            TecmoToolFactory.GetToolForRom(stockRom);
        }

        private static string GetFixturePath(string subfolder, string fileName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(Path.Combine(baseDir, subfolder), fileName);

            if (!File.Exists(path))
            {
                // Needed for VS 2008 testing
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                path = Regex.Replace(Uri.UnescapeDataString(uri.Path), "TSBTool_Test.*", "TSBTool_Test\\" + subfolder + "\\" + fileName);

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException(
                        "Test fixture not found. BaseDirectory=[" + baseDir + "] ComputedPath=[" + path + "]",
                        path);
                }
            }
            return path;
        }

        public static string GetRomPath(string fileName)
        {
            return GetFixturePath("ROMs", fileName);
        }

        public static byte[] LoadRom(string fileName)
        {
            return File.ReadAllBytes(GetRomPath(fileName));
        }

        public static List<string> LoadScheduleLines(string fileName)
        {
            return new List<string>(File.ReadAllLines(GetFixturePath("Schedules", fileName)));
        }

        public static string GetFullRomDumpPath(string fileName)
        {
            return GetFixturePath("FullRomDumps", fileName);
        }

        public static string LoadFullRomDumpText(string fileName)
        {
            return File.ReadAllText(GetFullRomDumpPath(fileName));
        }
    }
}
