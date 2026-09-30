using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Team sim data ("TEAM = x SimData=0xNN[P]") must survive export -> re-import for every byte
    /// value. The export used to pad single hex digits only for values below 0x0F, so 0x0F came out
    /// as "SimData=0xf": without the offense-preference digit the importer (which needs two hex
    /// digits) ignored it; with the digit, "0xf2" was read back as sim 0xF2 and the preference was
    /// dropped. See CodeReview-2026-09.md, M1.
    /// </summary>
    [TestClass]
    public class TeamSimDataRoundTripTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static void AssertEveryValueRoundTrips(string romFile)
        {
            List<string> failures = new List<string>();
            foreach (bool showPref in new[] { false, true })
            {
                ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romFile));
                tool.ShowOffPref = showPref;
                for (int value = 0; value <= 0xFF; value++)
                {
                    tool.SetTeamSimData("bills", (byte)value);
                    tool.SetTeamSimOffensePref("bills", 2);
                    string exported = tool.GetTeamPlayers("bills").Split('\n')[0];

                    // Scramble, then re-import the exported line exactly as written.
                    tool.SetTeamSimData("bills", (byte)(value ^ 0x55));
                    tool.SetTeamSimOffensePref("bills", 0);
                    TestMessageGiver giver = new TestMessageGiver();
                    StaticUtils.ClearErrors();
                    StaticUtils.sMessageGiver = giver;
                    try
                    {
                        tool.ProcessText(exported + "\n");
                        StaticUtils.ShowErrors();
                    }
                    finally
                    {
                        StaticUtils.sMessageGiver = null;
                    }

                    string reExported = tool.GetTeamPlayers("bills").Split('\n')[0];
                    if (reExported != exported || giver.Errors.Count > 0)
                        failures.Add(string.Format("0x{0:X2} (pref shown={1}): exported '{2}', after re-import '{3}'{4}",
                            value, showPref, exported, reExported,
                            giver.Errors.Count > 0 ? " [" + giver.Errors[0].Trim() + "]" : ""));
                }
            }
            Assert.AreEqual(0, failures.Count, romFile + ":\n" + string.Join("\n", failures.ToArray()));
        }

        [TestMethod]
        public void NesRom_TeamSimData_EveryValueRoundTrips()
        {
            AssertEveryValueRoundTrips("TSPRBOWL.nes");
        }

        [TestMethod]
        public void CxRomV105_TeamSimData_EveryValueRoundTrips()
        {
            AssertEveryValueRoundTrips("TSB 2007-32-105.nes");
        }

        [TestMethod]
        public void SnesRom_TeamSimData_EveryValueRoundTrips()
        {
            AssertEveryValueRoundTrips("TSB1.smc");
        }

        [TestMethod]
        public void GenesisRom_TeamSimData_EveryValueRoundTrips()
        {
            AssertEveryValueRoundTrips(@"Genesis\Tecmo Super Bowl (USA) (October 1993).md");
        }
    }
}
