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
    /// SNES TSB1 player names live in a packed, pointer-indexed area ($178038 pointers, names from 0x17873A to
    /// 0x17B7F0) that's shifted whenever a name changes length. PlayerNames.PlanFit shortens names to fit before
    /// an apply (GUI, command line), but SNES_TecmoTool.InsertPlayer itself had no check -- so with -noAutoFit, or
    /// anything calling ProcessText directly, enough longer names shifted data past the area's end
    /// (CodeReview-2026-09.md, H3; NES/CXROM got their check 2026-10-03). A write that doesn't fit must be
    /// rejected with an error, keeping the old name, as on NES and Genesis.
    /// </summary>
    [TestClass]
    public class SnesNameOverflowTests
    {
        private const string Rom = "TSB1.smc";
        private const int PointersStart = 0x178038;
        private const int NameAreaEnd = 0x17B7F0;   // SNES_TecmoTool.nameNumberSegmentEnd

        // SNES TSB1's 32 roster slots, in name-table order (as PlayerNamesConfig.Snes)
        private static readonly string[] Positions = { "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4",
            "TE1", "TE2", "C", "LG", "RG", "LT", "RT", "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB",
            "FS", "SS", "K", "P", "DB1", "DB2" };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

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

        /// <summary>The end (exclusive) of the name data: scanning back from the area's end over 0xFF padding.</summary>
        private static int NameDataEnd(byte[] rom)
        {
            int i = NameAreaEnd;
            while (rom[i] == 0xFF)
                i--;
            return i + 1;
        }

        /// <summary>Every byte outside the name area (pointers + names) is as it was.</summary>
        private static void AssertOnlyNameAreaChanged(byte[] before, byte[] after)
        {
            Assert.AreEqual(before.Length, after.Length);
            for (int i = 0; i < before.Length; i++)
                if ((i < PointersStart || i > NameAreaEnd) && before[i] != after[i])
                    Assert.Fail("byte 0x{0:X} outside the name area changed: {1:X2} -> {2:X2}", i, before[i], after[i]);
        }

        [TestMethod]
        public void TooManyLongerNames_AreRejected_NothingPastTheAreaChanges()
        {
            byte[] stock = TestRoms.LoadRom(Rom);
            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom((byte[])stock.Clone());

            // every player a 16-letter name (17 bytes with the jersey): far more than the area's free bytes
            List<string> errors = RunAndCollectErrors(() =>
            {
                foreach (string team in tool.GetTeams())
                    foreach (string pos in Positions)
                        tool.InsertPlayer(team, pos, "abcdefgh", "ABCDEFGH", 7);
            });

            Assert.IsTrue(errors.Any(e => e.Contains("would overflow")), "names that don't fit should be reported");
            Assert.IsTrue(NameDataEnd(tool.OutputRom) <= NameAreaEnd, string.Format("name data ends at 0x{0:X}", NameDataEnd(tool.OutputRom)));
            AssertOnlyNameAreaChanged(stock, tool.OutputRom);
            // the names that were written read back; the rejected ones keep their old name
            Assert.AreEqual("abcdefgh ABCDEFGH", tool.GetName("bills", "QB1"));
            Assert.AreEqual(new SNES_TecmoTool_Stock().Name("49ers", "DB2"), tool.GetName("49ers", "DB2"));
        }

        [TestMethod]
        public void LongerNameThatFits_IsWritten()
        {
            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(Rom));
            string qb2 = tool.GetName("bills", "QB2");
            List<string> errors = RunAndCollectErrors(() => tool.InsertPlayer("bills", "QB1", "alexander", "KELLYSON", 12));
            Assert.AreEqual(0, errors.Count, string.Join("\n", errors.ToArray()));
            Assert.AreEqual("alexander KELLYSON", tool.GetName("bills", "QB1"));
            Assert.AreEqual(qb2, tool.GetName("bills", "QB2"), "the next player's name moved with the shift");
        }

        [TestMethod]
        public void ShorterName_IsWritten()
        {
            byte[] stock = TestRoms.LoadRom(Rom);
            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom((byte[])stock.Clone());
            string rb1 = tool.GetName("bills", "RB1");
            List<string> errors = RunAndCollectErrors(() => tool.InsertPlayer("bills", "QB1", "j", "K", 12));
            Assert.AreEqual(0, errors.Count, string.Join("\n", errors.ToArray()));
            Assert.AreEqual("j K", tool.GetName("bills", "QB1"));
            Assert.AreEqual(rb1, tool.GetName("bills", "RB1"));
            AssertOnlyNameAreaChanged(stock, tool.OutputRom);
        }

        [TestMethod]
        public void CommandLine_NoAutoFit_OverFullRoster_StaysInTheArea()
        {
            string dir = Path.Combine(Path.GetTempPath(), "TSBToolSnesNames_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            string oldDir = Environment.CurrentDirectory;
            TextWriter oldErr = Console.Error, oldOut = Console.Out;
            try
            {
                Environment.CurrentDirectory = dir;
                File.Copy(TestRoms.GetRomPath(Rom), Path.Combine(dir, "rom.smc"));
                Console.SetError(new StringWriter());
                Console.SetOut(new StringWriter());
                MainClass.TestString = "";
                MainClass.RunMain(new[] { "rom.smc" });
                // every player line's name -> 16 letters
                string longNames = Regex.Replace(MainClass.TestString, @"^([A-Z]+[0-9]?), [^,]+,", "$1, abcdefgh ABCDEFGH,", RegexOptions.Multiline);
                File.WriteAllText(Path.Combine(dir, "input.txt"), longNames);
                MainClass.RunMain(new[] { "rom.smc", "input.txt", "-noAutoFit", "-out:out.smc" });
            }
            finally
            {
                Console.SetError(oldErr);
                Console.SetOut(oldOut);
                Environment.CurrentDirectory = oldDir;
            }
            byte[] output = File.ReadAllBytes(Path.Combine(dir, "out.smc"));
            try { Directory.Delete(dir, true); } catch { }

            Assert.IsTrue(NameDataEnd(output) <= NameAreaEnd, string.Format("name data ends at 0x{0:X}", NameDataEnd(output)));
            byte[] stock = TestRoms.LoadRom(Rom);
            for (int i = NameAreaEnd + 1; i < NameAreaEnd + 0x100; i++)
                Assert.AreEqual(stock[i], output[i], string.Format("byte 0x{0:X} past the name area changed", i));
        }

        /// <summary>A stock SNES ROM's names, for comparison.</summary>
        private class SNES_TecmoTool_Stock
        {
            private readonly SNES_TecmoTool mTool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(Rom));
            public string Name(string team, string pos) { return mTool.GetName(team, pos); }
        }
    }
}
