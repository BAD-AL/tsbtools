using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Tecmonster's team formula (TecmonsterTeamSim): TSB1's team SimData (rush D, pass D) from the starters'
    /// ratings, run by the TSB1 player form's Auto Update (with the players' sim data). The run/pass preference
    /// digit is kept; a team missing a starter keeps its value instead of stopping the whole update.
    /// </summary>
    [TestClass]
    public class TecmonsterTeamSimTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static string NesText()
        {
            return ((ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSPRBOWL.nes"))).GetAll();
        }

        private static string TeamLine(string text, string team)
        {
            return text.Replace("\r", "").Split('\n').First(l => Regex.IsMatch(l, @"^TEAM\s*=\s*" + team + @"\b"));
        }

        [TestMethod]
        public void Tsb1_UpdatesTeamDigits_KeepsThePreferenceDigit()
        {
            string text = NesText();
            string before = "TEAM = bills SimData=0xab1";   // with a run/pass preference digit
            text = text.Replace(TeamLine(text, "bills"), before);
            string after = TeamLine(new TecmonsterTeamSim().AutoUpdateSeasonSimData(1, text), "bills");

            StringAssert.Matches(after, new Regex("^TEAM = bills SimData=0x[0-9a-f]{2}1$"));
            Assert.AreNotEqual(before, after);
        }

        [TestMethod]
        public void Tsb1_TeamMissingAStarter_KeepsItsValue_OthersStillUpdate()
        {
            string text = NesText();
            string[] lines = text.Replace("\r", "").Split('\n');
            int bills = System.Array.FindIndex(lines, l => l.StartsWith("TEAM = bills"));
            int lolb = System.Array.FindIndex(lines, bills, l => l.StartsWith("LOLB,"));
            string partial = string.Join("\n", lines.Where((l, i) => i != lolb).ToArray());

            string output = new TecmonsterTeamSim().AutoUpdateSeasonSimData(1, partial);

            Assert.AreEqual(TeamLine(text, "bills"), TeamLine(output, "bills"));
            Assert.AreNotEqual(TeamLine(text, "colts"), TeamLine(output, "colts"));
        }
    }
}
