using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// "Apply To ROM" used to show up to three name dialogs on a CXROM ROM: a room report per name
    /// area (main + expansion) before applying, then an "Error!" box after with InsertPlayer's
    /// "15 character limit" warnings. They're all "these names will be shortened", so
    /// PlayerNames.PlanFit now builds ONE report covering every name area plus the over-length names,
    /// and RosterNamePlan.Apply writes the shortened names into the text being applied, so InsertPlayer
    /// never sees an over-length name and never warns. One test per ROM type: the families with a
    /// name check (NES, CXROM x3, SNES) get one plan and no length warnings after; Genesis (its own
    /// check), TSB2 and TSB3 get no plan.
    /// </summary>
    [TestClass]
    public class RosterNamePlanTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        // The ROM's own bills block (TEAM line + player lines), as its dump prints it.
        private static string BillsBlock(ITecmoContent tool)
        {
            List<string> lines = tool.GetAll(1).Replace("\r\n", "\n").Split('\n')
                .SkipWhile(l => !l.StartsWith("TEAM = bills"))
                .TakeWhile((l, i) => i == 0 || !l.StartsWith("TEAM ="))
                .ToList();
            Assert.IsTrue(lines.Count > 20, "Sanity: found the bills block.");
            return string.Join("\n", lines.ToArray()) + "\n";
        }

        private static string WithName(string block, string pos, string newName)
        {
            Regex line = new Regex("^(" + pos + "\\s*,\\s*)[^,]+(,.*)$", RegexOptions.Multiline);
            Assert.IsTrue(line.IsMatch(block), "Sanity: " + pos + " line present.");
            return line.Replace(block, "${1}" + newName + "$2", 1);
        }

        private static List<string> ProcessAndCollectErrors(ITecmoContent tool, string text)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            tool.ProcessText(text);
            StaticUtils.ShowErrors();
            return giver.Errors;
        }

        // NES, CXROM (all three) and SNES: one over-length name -> one plan naming it; after applying
        // the plan's text, no length warning, and the ROM holds exactly the name the plan listed.
        [TestMethod]
        public void PlanFit_OverLengthName_OnePlanListsIt_NoLengthWarningAfterApply_EveryNameCheckedRomType()
        {
            foreach (string romName in new[] { "TSPRBOWL.nes", "TSB 2007-32-105.nes", "TSB 2007-32-111.nes", "SbluemanBase_18week_RealNFL-1.nes", "TSB1.smc" })
            {
                ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
                string text = WithName(BillsBlock(tool), "QB1", "anthony RICHARDSONXXXX");

                RosterNamePlan plan = PlayerNames.PlanFit(tool, text);
                Assert.IsTrue(plan.NeedsAttention, romName);
                string report = plan.ToString();
                StringAssert.Contains(report, "anthony RICHARDSONXXXX", romName + ":\n" + report);
                StringAssert.Contains(report, "a. RICHARDSONXXXX", romName + ": the capped name, same rule InsertPlayer uses:\n" + report);

                string applied = plan.Apply(tool, text);
                List<string> errors = ProcessAndCollectErrors(tool, applied);
                Assert.IsFalse(errors.Any(e => e.Contains("character limit")), romName + ":\n" + string.Join("\n", errors.ToArray()));
                StringAssert.Contains(((ITecmoTool)tool).GetTeamPlayers("bills"), "QB1, a. RICHARDSONXXXX,", romName);
            }
        }

        // A roster that already fits with no over-length names needs no dialog at all.
        [TestMethod]
        public void PlanFit_RosterThatFits_NoPlan_EveryNameCheckedRomType()
        {
            foreach (string romName in new[] { "TSPRBOWL.nes", "TSB 2007-32-105.nes", "TSB 2007-32-111.nes", "SbluemanBase_18week_RealNFL-1.nes", "TSB1.smc" })
            {
                ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
                RosterNamePlan plan = PlayerNames.PlanFit(tool, BillsBlock(tool));
                Assert.IsFalse(plan.NeedsAttention, romName + ":\n" + plan);
            }
        }

        // Genesis has its own check (Genesis_TSB1Tool.ResolveRosterFit); TSB2/TSB3 have none.
        [TestMethod]
        public void PlanFit_GenesisTsb2Tsb3_NoPlan()
        {
            foreach (string romName in new[] { "Genesis/Tecmo Super Bowl (USA) (October 1993).bin", "TSB2_U.smc", "TSB3.smc" })
            {
                ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
                RosterNamePlan plan = PlayerNames.PlanFit(tool, "TEAM = bills\nQB1, anthony RICHARDSONXXXX, #12,\n");
                Assert.IsFalse(plan.NeedsAttention, romName);
            }
        }

        // The case reported: the conversion roster on CXROM v105 -- both name areas and the
        // over-length names in ONE report; after applying, no length warnings and no rejections.
        [TestMethod]
        public void PlanFit_CxRomV105_ConversionTestRoster_OneReportBothAreas_NoNameErrorsAfterApply()
        {
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB 2007-32-105.nes"));
            string text = File.ReadAllText(TestRoms.GetConversionTestPath("ConversionTest.txt"));

            RosterNamePlan plan = PlayerNames.PlanFit(tool, text);
            string report = plan.ToString();
            Assert.IsTrue(plan.NeedsAttention);
            StringAssert.Contains(report, "main teams", report);
            StringAssert.Contains(report, "49ers/rams/seahawks/cardinals", report);
            StringAssert.Contains(report, "'anthony RICHARDSON'  ->  'a. RICHARDSON'", report);

            List<string> errors = ProcessAndCollectErrors(tool, plan.Apply(tool, text));
            Assert.IsFalse(errors.Any(e => e.Contains("character limit") || e.Contains("would overflow")), string.Join("\n", errors.ToArray()));
        }
    }
}
