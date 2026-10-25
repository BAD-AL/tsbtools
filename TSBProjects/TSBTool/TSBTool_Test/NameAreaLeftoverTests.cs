using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// TSBTool versions before October 2026 left the old tail of a shortened last name (falcons P; CXROM
    /// panthers P) behind it in the name area, e.g. "samMARTIN" + a stray 'R' in
    /// SbluemanBase_18week_RealNFL-1.nes. InsertPlayer's room check (scanning back over 0xFF) counted that
    /// byte as data while the roster name check (PlayerNames, adding up the names) didn't, so a roster the
    /// check had fitted exactly was rejected "by 1 byte" on lions WR1. Loading a NES/CXROM ROM now clears
    /// everything past the end pointer to 0xFF.
    /// </summary>
    [TestClass]
    public class NameAreaLeftoverTests
    {
        private const int NameAreaEnd = 0x300F;

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static int MainNameDataEnd(byte[] rom, int sentinelPointerLoc)
        {
            return (rom[sentinelPointerLoc] | (rom[sentinelPointerLoc + 1] << 8)) - 0x8000 + 0x10;
        }

        private static List<string> RunAndCollectErrors(Action action)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                action();
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            return giver.Errors;
        }

        // A copy of romName with leftover bytes right after the main area's last name, like an older
        // TSBTool's in-place shrink left them.
        private static byte[] RomWithLeftover(string romName, int sentinelPointerLoc, out int dataEnd)
        {
            byte[] rom = TestRoms.LoadRom(romName);
            dataEnd = MainNameDataEnd(rom, sentinelPointerLoc);
            Assert.AreEqual(0xFF, rom[dataEnd], "sanity: stock ROM is clean after its last name");
            rom[dataEnd] = (byte)'R';
            rom[dataEnd + 1] = (byte)'S';
            return rom;
        }

        [TestMethod]
        public void NesStock_LeftoverAfterLastName_IsClearedOnLoad()
        {
            int dataEnd;
            byte[] rom = RomWithLeftover("TSPRBOWL.nes", 0x6d8, out dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Assert.AreEqual(0xFF, tool.OutputRom[dataEnd]);
            Assert.AreEqual(0xFF, tool.OutputRom[dataEnd + 1]);
            Assert.AreEqual("scott FULHAGE", tool.GetName("falcons", "P"), "the last name itself is kept");
        }

        [TestMethod]
        public void CxRom_LeftoverAfterLastName_IsClearedOnLoad()
        {
            int dataEnd;
            byte[] rom = RomWithLeftover("TSB 2007-32-105.nes", 0x6e4, out dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            Assert.AreEqual(0xFF, tool.OutputRom[dataEnd]);
            Assert.AreEqual(0xFF, tool.OutputRom[dataEnd + 1]);
            Assert.AreEqual("jason BAKER", tool.GetName("panthers", "P"), "the last name itself is kept");
        }

        // With the leftover gone, every free byte (counted from the names, as PlayerNames does) can be
        // used: grow names in order until the area is exactly full -- no write may be rejected.
        [TestMethod]
        public void CxRom_LeftoverRom_EveryFreeByteCanBeUsed()
        {
            int dataEnd;
            byte[] rom = RomWithLeftover("TSB 2007-32-105.nes", 0x6e4, out dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            int remaining = NameAreaEnd - dataEnd;

            List<string> errors = RunAndCollectErrors(delegate
            {
                foreach (string team in TecmoTool.Teams.GetRange(0, 28))
                    foreach (string pos in new[] { "QB1", "RB1", "WR1", "TE1", "RE", "FS", "K" })
                    {
                        if (remaining == 0)
                            return;
                        int oldBytes = tool.GetName(team, pos).Length; // "f L" == jersey + f + L bytes
                        int growth = Math.Min(17 - oldBytes, remaining);
                        if (growth <= 0)
                            continue;
                        tool.InsertPlayer(team, pos, "a", new string('B', oldBytes + growth - 2), 1);
                        remaining -= growth;
                    }
            });

            Assert.AreEqual(0, remaining, "sanity: enough players to fill the area");
            CollectionAssert.AreEqual(new List<string>(), errors);
            Assert.AreEqual(NameAreaEnd, MainNameDataEnd(tool.OutputRom, 0x6e4), "area exactly full");
        }

        // The real case: this ROM and roster (CLI flow: PlanFit, Apply, ProcessText) gave
        // "setting lions's WR1 to 'amon.ra ST.BROWN' would overflow the player name area by 1 byte(s)".
        // Both files are local (untracked), so the test is inconclusive without them.
        [TestMethod]
        public void SbluemanBase_32TeamRoster_AppliesWithNoOverflow()
        {
            byte[] rom;
            string text;
            try
            {
                rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
                text = File.ReadAllText(TestRoms.GetPlayTestPath("TSB1_32-team-18-game_2026.txt"));
            }
            catch (FileNotFoundException e)
            {
                Assert.Inconclusive("Local test files missing: " + e.Message);
                return;
            }

            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> errors = RunAndCollectErrors(delegate
            {
                RosterNamePlan plan = PlayerNames.PlanFit(tool, text);
                string content = plan.Apply(tool, text);
                tool.ProcessText(content);
            });

            Assert.IsFalse(errors.Any(e => e.Contains("would overflow")), string.Join("\n", errors.ToArray()));
            Assert.AreEqual("amon.ra ST.BROWN", tool.GetName("lions", "WR1"));
        }
    }
}
