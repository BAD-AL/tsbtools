using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// The TSB Sim Data Guide (TSBTool_Sim_Guide.txt) is embedded in the program and shown from About ->
    /// TSB Sim Data Guide and the player forms' 'About SimData' buttons, which open it at their game's
    /// section.
    /// </summary>
    [TestClass]
    public class SimGuideTests
    {
        [TestMethod]
        public void Guide_IsEmbedded()
        {
            string text = SimGuide.Text;
            Assert.IsNotNull(text, "TSBTool_Sim_Guide.txt should be an embedded resource");
            StringAssert.StartsWith(text.TrimStart(), "=====");
            StringAssert.Contains(text, "TSB SIM DATA GUIDE");
        }

        [TestMethod]
        public void Guide_HasEveryGamesSection_InOrder()
        {
            string[] sections = { SimGuide.Tsb1Section, SimGuide.GenesisSection, SimGuide.Tsb2Section, SimGuide.Tsb3Section };
            int last = -1;
            foreach (string s in sections)
            {
                int at = SimGuide.SectionStart(s);
                Assert.IsTrue(at > last, "section '" + s + "' missing or out of order");
                last = at;
            }
        }

        [TestMethod]
        public void SectionStart_UnknownSection_IsTheTop()
        {
            Assert.AreEqual(0, SimGuide.SectionStart("=== no such section ==="));
            Assert.AreEqual(0, SimGuide.SectionStart(null));
        }

        [TestMethod]
        public void Guide_NamesTheFixLines()
        {
            StringAssert.Contains(SimGuide.Text, "GENESIS_FIX_SIM_BUGS");
            StringAssert.Contains(SimGuide.Text, "'FIX_SIM_BUGS'");
        }
    }
}
