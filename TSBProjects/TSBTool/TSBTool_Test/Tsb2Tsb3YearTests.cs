using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB2Tool/TSB3Tool.SetYear -- the same blanket replace as Genesis_TSB1Tool.SetYear: every on-screen
    /// occurrence of the newest year (TSB2 "1994" = season 3, TSB3 "1995") becomes the given year; TSB2's
    /// 1992/1993 season text is left alone. Found by scanning each ROM for ASCII years and checking context.
    /// Left out on purpose: TSB3's "NAK1989 S-CG-CAD" developer string (0x1A5603), TSB3's leftover copy of
    /// TSB2's "1992 1993 1994" strings, and the "199X SEASON" template both ROMs carry.
    /// </summary>
    [TestClass]
    public class Tsb2Tsb3YearTests
    {
        // season menu, two clear-data prompts, the compact year list, title (C) line, NFL legal line
        private static readonly int[] Tsb2YearLocations = { 0x170EC5, 0x170F07, 0x171699, 0x17175F, 0x173B34, 0x1BE229 };
        // title (C) line, three NFL legal lines, stadium-facts screen, two credits lines
        private static readonly int[] Tsb3YearLocations = { 0x173934, 0x1BCBCD, 0x1BCC15, 0x1BD0B0, 0x1BD6A5, 0x1BE46F, 0x1BF782 };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static TSB2Tool Load(string romName)
        {
            return (TSB2Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(romName));
        }

        private static string At(byte[] rom, int location)
        {
            return "" + (char)rom[location] + (char)rom[location + 1] + (char)rom[location + 2] + (char)rom[location + 3];
        }

        // Every location holding 'year' in the ROM, so a test can tell what changed.
        private static List<int> Find(byte[] rom, string year)
        {
            return StaticUtils.FindStringInFile(year, rom, 0, rom.Length).Select(l => (int)l).ToList();
        }

        [TestMethod]
        public void StockRoms_HaveTheNewestYearAtEveryLocation()
        {
            foreach (int location in Tsb2YearLocations)
                Assert.AreEqual("1994", At(Load("TSB2_U.smc").OutputRom, location), string.Format("TSB2 0x{0:X}", location));
            foreach (int location in Tsb3YearLocations)
                Assert.AreEqual("1995", At(Load("TSB3.smc").OutputRom, location), string.Format("TSB3 0x{0:X}", location));
        }

        [TestMethod]
        public void SetYear_WritesEveryLocation_AndGetYearReadsItBack()
        {
            foreach (object[] c in new[] { new object[] { "TSB2_U.smc", Tsb2YearLocations }, new object[] { "TSB3.smc", Tsb3YearLocations } })
            {
                TSB2Tool tool = Load((string)c[0]);
                tool.SetYear("2026");
                foreach (int location in (int[])c[1])
                    Assert.AreEqual("2026", At(tool.OutputRom, location), string.Format("{0} 0x{1:X}", c[0], location));
                Assert.AreEqual("2026", tool.GetYear(), (string)c[0]);
            }
        }

        [TestMethod]
        public void SetYear_ChangesOnlyTheListedLocations()
        {
            foreach (object[] c in new[] { new object[] { "TSB2_U.smc", Tsb2YearLocations }, new object[] { "TSB3.smc", Tsb3YearLocations } })
            {
                TSB2Tool tool = Load((string)c[0]);
                byte[] before = (byte[])tool.OutputRom.Clone();
                tool.SetYear("2026");
                List<int> changed = Enumerable.Range(0, before.Length).Where(i => before[i] != tool.OutputRom[i]).ToList();
                Assert.IsTrue(changed.Count > 0, (string)c[0] + ": sanity");
                Assert.IsTrue(changed.All(i => ((int[])c[1]).Any(l => i >= l && i < l + 4)), (string)c[0] + ": a byte outside the year text changed");
            }
        }

        [TestMethod]
        public void Tsb2_SetYear_LeavesTheOlderSeasonsAlone()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            List<int> y1992 = Find(tool.OutputRom, "1992"), y1993 = Find(tool.OutputRom, "1993");
            tool.SetYear("2026");
            CollectionAssert.AreEqual(y1992, Find(tool.OutputRom, "1992"));
            CollectionAssert.AreEqual(y1993, Find(tool.OutputRom, "1993"));
            Assert.AreEqual(0, Find(tool.OutputRom, "1994").Count, "no 1994 left");
        }

        // The path that was broken: a YEAR line in the input text.
        [TestMethod]
        public void YearLine_ThroughProcessText_SetsTheYear()
        {
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
                TSB2Tool tool = Load(rom);
                ((ITecmoContent)tool).ProcessText("YEAR=2026\n");
                Assert.AreEqual("2026", tool.GetYear(), rom);
            }
        }

        [TestMethod]
        public void SetYear_BadYear_ErrorAndRomUnchanged()
        {
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
                TSB2Tool tool = Load(rom);
                byte[] before = (byte[])tool.OutputRom.Clone();
                TestMessageGiver giver = new TestMessageGiver();
                StaticUtils.ClearErrors();
                StaticUtils.sMessageGiver = giver;
                tool.SetYear("226");
                StaticUtils.ShowErrors();
                CollectionAssert.AreEqual(before, tool.OutputRom, rom);
                Assert.IsTrue(giver.Errors.Any(e => e.Contains("'226'")), rom + ":\n" + string.Join("\n", giver.Errors.ToArray()));
            }
        }
    }
}
