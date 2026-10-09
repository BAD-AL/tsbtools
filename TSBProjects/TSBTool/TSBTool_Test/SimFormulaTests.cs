using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// The "Auto Update sim data" formulas (AttributeForm, and TecmonsterTSB1SimAutoUpdater used by the
    /// form, the converter and the TSB2/3 player form). A skill player's 3rd and 4th sim values are
    /// yards per catch and targets (see TSB_Simulation.md), not punt/kick return.
    /// </summary>
    [TestClass]
    public class SimFormulaTests
    {
        [TestMethod]
        public void SimYPC_UsesPositionBasePlusMsAndRec()
        {
            SimStuff s = new SimStuff();
            Assert.AreEqual(10, s.SimYPC("WR1", 63, 81)); // 4 + 144/25 = 9.76
            Assert.AreEqual(5, s.SimYPC("RB2", 25, 31));  // 3 + 56/25  = 5.24
            Assert.AreEqual(12, s.SimYPC("TE1", 100, 100)); // 4 + 8
            Assert.AreEqual(6, s.SimYPC("WR3", 31, 25));  // 4 + 2.24
        }

        [TestMethod]
        public void SimTargets_ComeFromDepthSlot()
        {
            string[] positions = { "WR1", "WR2", "TE1", "RB1", "RB2", "WR3", "WR4", "TE2", "RB3", "RB4" };
            int[] expected = { 9, 8, 5, 4, 4, 2, 2, 1, 1, 1 };
            for (int i = 0; i < positions.Length; i++)
                Assert.AreEqual(expected[i], SimStuff.SimTargets(positions[i]), positions[i]);
        }

        private static int[] SimValues(string text, string position)
        {
            Match line = new Regex("\n" + position + ",[^\n]*\\[([^\\]]*)\\]").Match(text);
            Assert.IsTrue(line.Success, position + " line not found");
            MatchCollection nums = new Regex("[0-9]+").Matches(line.Groups[1].Value);
            int[] ret = new int[nums.Count];
            for (int i = 0; i < nums.Count; i++)
                ret[i] = int.Parse(nums[i].Value);
            return ret;
        }

        private const string TecmonsterInput =
            "TEAM = bills SimData=0x88\n" +
            "RB1, thurman THOMAS, Face=0x0, #34, 44, 69, 50, 31, 69, 56 ,[1, 1, 1, 1 ]\n" +
            "WR1, andre REED, Face=0x0, #83, 44, 69, 63, 13, 56, 81 ,[1, 1, 1, 1 ]\n";

        [TestMethod]
        public void Tecmonster_FourthSkillValue_IsTargetsByDepth()
        {
            string result = TecmonsterTSB1SimAutoUpdater.AutoUpdatePlayerSimData(TecmonsterInput);

            Assert.AreEqual(4, SimValues(result, "RB1")[3]);
            Assert.AreEqual(9, SimValues(result, "WR1")[3]);
        }

        [TestMethod]
        public void Tecmonster_RbSimRush_UsesMsAndHp()
        {
            // RS 44, RP 69, MS 50, HP 31 -> round((13 + 50 + 19 + 18) / 176 * 15) = 9
            string result = TecmonsterTSB1SimAutoUpdater.AutoUpdatePlayerSimData(TecmonsterInput);

            Assert.AreEqual(9, SimValues(result, "RB1")[0]);
        }
    }
}
