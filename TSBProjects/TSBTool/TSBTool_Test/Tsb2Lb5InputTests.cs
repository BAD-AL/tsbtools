using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// GitHub issue #3, "LB5 edit not saving": TSB2/TSB3's InputParser picked the defensive player lines by
    /// "LB" at index 2 (ROLB, RILB, LILB, LOLB), so an LB5 line was skipped: its name, face, ratings and sim
    /// data never reached the ROM.
    /// </summary>
    [TestClass]
    public class Tsb2Lb5InputTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        [TestMethod]
        public void Tsb2_Lb5Line_IsApplied()
        {
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")) as TSB2Tool;
            // stock: LB5,marvcus PATTON,Face=0x84,#53,19,38,31,50,19,19,25,[0C,00,01]
            tool.ProcessText("SEASON 1\nTEAM = bills\nLB5,marvin TESTER,Face=0x84,#53,25,38,31,50,19,19,25,[11,22,03]\n");

            StringAssert.Contains(tool.GetTeams(1), "LB5,marvin TESTER,Face=0x84,#53,25,38,31,50,19,19,25,[11,22,03]");
        }

        [TestMethod]
        public void Tsb3_Lb5Line_IsApplied()
        {
            TSB3Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc")) as TSB3Tool;
            // stock: LB5,sam ROGERS,Face=0x80,#59,38,31,44,31,19,38,19,25,[00,08,03]
            tool.ProcessText("SEASON 1\nTEAM = bills\nLB5,sam TESTER,Face=0x80,#59,44,31,44,31,19,38,19,25,[11,22,03]\n");

            StringAssert.Contains(tool.GetTeams(1), "LB5,sam TESTER,Face=0x80,#59,44,31,44,31,19,38,19,25,[11,22,03]");
        }
    }
}
