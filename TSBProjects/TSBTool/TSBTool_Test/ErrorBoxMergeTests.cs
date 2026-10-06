using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Applying text used to show the roster problems and the schedule problems in two separate
    /// "Error!" boxes: InputParser.ProcessLines showed what had collected after the last line, then
    /// ApplySchedule showed the schedule's own problems (both InputParsers, TSB1 and TSB2/3). Now
    /// everything from one apply is shown together, once, at the end.
    /// </summary>
    [TestClass]
    public class ErrorBoxMergeTests
    {
        [TestCleanup]
        public void Cleanup() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        [TestMethod]
        public void ProcessText_RosterAndScheduleProblems_OneErrorBox_EveryRomType()
        {
            foreach (string romName in new[] { "TSPRBOWL.nes", "TSB 2007-32-105.nes", "TSB1.smc",
                "Genesis/Tecmo Super Bowl (USA) (October 1993).bin", "TSB2_U.smc", "TSB3.smc" })
            {
                ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
                // The ROM's own schedule with the first game's home team misspelled, after a player
                // line with no ratings.
                string schedule = new Regex(@"(\w+) at (\w+)").Replace(tool.GetSchedule(1), "$1 at foobars", 1);
                string text = "TEAM = bills\nQB1, joe SMITH, #12,\n" + schedule;

                TestMessageGiver giver = new TestMessageGiver();
                StaticUtils.ClearErrors();
                StaticUtils.sMessageGiver = giver;
                tool.ProcessText(text);
                StaticUtils.ShowErrors(); // anything left over would be a further box

                Assert.AreEqual(1, giver.Errors.Count, romName + ":\n" + string.Join("\n-----\n", giver.Errors.ToArray()));
                StringAssert.Contains(giver.Errors[0], "QB1", romName + ": the roster problem");
                StringAssert.Contains(giver.Errors[0], "foobars", romName + ": the schedule problem");
            }
        }
    }
}
