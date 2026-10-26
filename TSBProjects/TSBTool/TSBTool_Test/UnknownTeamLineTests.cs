using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// CodeReview-2026-09 H2: a 'TEAM =' line naming a team the ROM doesn't have (a typo, or a 32-team roster on a
    /// 28-team ROM) used to leave the parser on the previous team, so the whole block overwrote that team -- the
    /// TSB2 2026 roster's panthers became the 49ers. Now the block is skipped up to the next TEAM (or WEEK) line,
    /// with an error saying how many lines were skipped, and the previous team is left alone.
    /// </summary>
    [TestClass]
    public class UnknownTeamLineTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static List<string> ProcessAndCollectErrors(ITecmoContent tool, string text)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            tool.ProcessText(text);
            StaticUtils.ShowErrors();
            return giver.Errors;
        }

        private static string Errors(List<string> errors) { return string.Join("\n", errors.ToArray()); }

        // ---- TSB1 parser: NES, CXROM, SNES TSB1, Genesis ----

        [TestMethod]
        public void Tsb1_UnknownTeam_BlockSkipped_PreviousTeamUntouched_NextTeamApplied()
        {
            foreach (string romName in new[] { "TSPRBOWL.nes", "TSB 2007-32-105.nes", "TSB1.smc", "Genesis/Tecmo Super Bowl (USA) (October 1993).bin" })
            {
                StaticUtils.ClearErrors();
                TSBTool.ITecmoTool tool = (TSBTool.ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
                string patriotsBefore = tool.GetTeamPlayers("patriots");
                string text =
                    patriotsBefore.Replace("\r\n", "\n").Split('\n')[0] + "\n" + // the ROM's own TEAM line: patriots is current, SimData unchanged
                    "TEAM = jetz, SimData=0x1F0\n" +
                    "PLAYBOOK R1111, P1111\n" +
                    "QB1, joe NAMATH, Face=0x10, #12, 25, 69, 13, 13, 56, 81, 81, 81, 13, 13, [1, 1, 1]\n" +
                    "KR, QB1\n" +
                    "TEAM = jets, SimData=0x1F0\n" +
                    "QB1, aaron RODGERS, Face=0x10, #8, 25, 69, 13, 13, 56, 81, 81, 81, 13, 13, [1, 1, 1]\n";

                List<string> errors = ProcessAndCollectErrors((ITecmoContent)tool, text);

                Assert.AreEqual(patriotsBefore, tool.GetTeamPlayers("patriots"), romName + ": the patriots keep their roster\n" + Errors(errors));
                StringAssert.Contains(tool.GetTeamPlayers("jets"), "aaron RODGERS", romName + ": the next team still applies");
                Assert.IsTrue(errors.Any(e => e.Contains("'jetz'") && e.Contains("3 line(s)") && e.Contains("skipped")), romName + ":\n" + Errors(errors));
            }
        }

        // The skip ends at a WEEK line too: the schedule after an unknown team still applies.
        [TestMethod]
        public void Tsb1_UnknownTeam_ThenSchedule_ScheduleApplied_SkipReported()
        {
            TSBTool.ITecmoTool tool = (TSBTool.ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSPRBOWL.nes"));
            string text =
                "TEAM = texans, SimData=0x1F0\n" +
                "QB1, cj STROUD, Face=0x10, #7, 25, 69, 13, 13, 56, 81, 81, 81, 13, 13, [1, 1, 1]\n" +
                "WEEK 1\n" +
                "bills at dolphins\n";
            List<string> errors = ProcessAndCollectErrors((ITecmoContent)tool, text);

            StringAssert.Contains(tool.GetSchedule(), "bills at dolphins", Errors(errors));
            Assert.IsTrue(errors.Any(e => e.Contains("'texans'") && e.Contains("1 line(s)")), Errors(errors));
        }

        // ---- TSB2/TSB3 parser ----

        [TestMethod]
        public void Tsb2Tsb3_UnknownTeam_BlockSkipped_PreviousTeamUntouched()
        {
            foreach (string[] c in new[] { new[] { "TSB2_U.smc", "jaguars" }, new[] { "TSB3.smc", "ravens" } })
            {
                StaticUtils.ClearErrors();
                TSB2Tool tool = (TSB2Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(c[0]));
                string ninersBefore = tool.GetTeamPlayers(1, "49ers");
                string text =
                    "SEASON 1\n" +
                    "TEAM = 49ers\n" +
                    "TEAM = " + c[1] + ",SimData=0x48\n" +
                    "PLAYBOOK R626005E3, P40483E39\n" +
                    "TEAM_ABB=JAX.,TEAM_CITY=jacksonville,TEAM_NAME=JAGUARS\n" +
                    "QB1,trevor LAWRENCE,Face=0x10,#16,69,25,38,38,31,81,75,69,81,69,[18,02,D0]\n";

                List<string> errors = ProcessAndCollectErrors((ITecmoContent)tool, text);

                Assert.AreEqual(ninersBefore, tool.GetTeamPlayers(1, "49ers"), c[0] + ": the 49ers keep their roster\n" + Errors(errors));
                Assert.IsTrue(errors.Any(e => e.Contains("'" + c[1] + "'") && e.Contains("3 line(s)") && e.Contains("skipped")), c[0] + ":\n" + Errors(errors));
            }
        }
    }
}
