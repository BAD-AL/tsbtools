using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// NES/CXROM player names live in a packed, pointer-indexed area that's shifted whenever a name
    /// changes length. TecmoTool.InsertPlayer used to shift with no check that the area still had
    /// room, so enough longer names ran past the area's end (0x300F) into the attribute table,
    /// overwriting the Bills' player ratings (CodeReview-2026-09.md, H3). Found in practice applying
    /// TSBTool_Test\ConversionTest\ConversionTest.txt (a 2026 32-team roster) to CXROM v105. A write
    /// that doesn't fit must now be rejected with an error, leaving the old name in place.
    /// </summary>
    [TestClass]
    public class NesNameOverflowTests
    {
        private const int NameAreaEnd = 0x300F;          // last usable byte of the main name area
        private const int AttributeTableStart = 0x3010;  // Bills QB1's attributes
        private const int ExpansionAreaEnd = 0x3FFDF;    // CXROM expansion names (CXRomTSBTool.ShiftDataAfter)
        private const int CxRomProtectedBytes = 0x3FFF0; // CXRomTSBTool.m_ExpansionSegmentEnd (14 bytes checked at save)

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        // The end of the main name area's data, from its sentinel pointer (the "next" pointer after
        // the last team's punter).
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

        // The real-world case: a 2026 roster whose names need ~230 more bytes than the 109 free.
        // Names that don't fit are rejected (old name kept, error reported); every rating in the file
        // must land exactly as written -- the Bills' were being overwritten by shifted name text.
        [TestMethod]
        public void CxRomV105_ConversionTestRoster_NamesStayInArea_AllBillsRatingsIntact()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            string text = System.IO.File.ReadAllText(TestRoms.GetConversionTestPath("ConversionTest.txt"));

            List<string> errors = RunAndCollectErrors(() => tool.ProcessText(text));

            int dataEnd = MainNameDataEnd(rom, 0x6e4);
            Assert.IsTrue(dataEnd <= NameAreaEnd, string.Format("Main name data ends at 0x{0:X}, past 0x{1:X}.", dataEnd, NameAreaEnd));
            Assert.IsTrue(errors.Any(e => e.Contains("would overflow")), "Expected the names that don't fit to be reported.");

            // Every Bills player's ratings must match the file exactly.
            List<string> fileLines = text.Replace("\r\n", "\n").Split('\n')
                .SkipWhile(l => !l.StartsWith("TEAM = bills")).Skip(1)
                .TakeWhile(l => !l.StartsWith("TEAM ="))
                .Where(l => l.Length > 3 && l.Contains("Face=")).ToList();
            Assert.AreEqual(30, fileLines.Count, "Sanity: 30 Bills player lines in the file.");
            List<string> mismatches = new List<string>();
            foreach (string line in fileLines)
            {
                string pos = line.Split(',')[0].Trim();
                int[] expected = InputParser.GetInts(line);
                int[] actual = tool.GetAbilities("bills", pos);
                for (int i = 0; i < actual.Length; i++)
                    if (actual[i] != expected[i])
                    {
                        mismatches.Add(string.Format("{0}: file {1}, ROM {2}", pos,
                            string.Join(",", expected.Take(actual.Length).Select(v => v.ToString()).ToArray()),
                            string.Join(",", actual.Select(v => v.ToString()).ToArray())));
                        break;
                    }
            }
            Assert.AreEqual(0, mismatches.Count, "Bills ratings overwritten:\n" + string.Join("\n", mismatches.ToArray()));
        }

        // NES main area: keep lengthening names until the area is full. Nothing past 0x300F may
        // change, and the writes that don't fit are reported.
        [TestMethod]
        public void NesRom_NamesGrowUntilFull_AttributeTableUntouched_OverflowReported()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            byte[] before = (byte[])rom.Clone();
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> errors = RunAndCollectErrors(() =>
            {
                foreach (string team in TecmoTool.Teams)
                    foreach (string pos in TecmoTool.PositionNames)
                        tool.InsertPlayer(team, pos, "abcdefgh", "ABCDEFGH", 0x11);
            });

            int changed = 0;
            for (int i = AttributeTableStart; i < AttributeTableStart + 0x1000; i++)
                if (rom[i] != before[i])
                    changed++;
            Assert.AreEqual(0, changed, "Bytes changed in the attribute table.");
            Assert.IsTrue(MainNameDataEnd(rom, 0x6d8) <= NameAreaEnd);
            Assert.IsTrue(errors.Any(e => e.Contains("would overflow")), "Expected overflow rejections to be reported.");
        }

        // The last name in an area (NES: falcons P; CXROM: panthers P and cardinals P) is rewritten in
        // place without shifting anything after it. When it gets SHORTER, its old tail bytes must be
        // cleared to 0xFF -- otherwise they keep occupying the end of the area: the space they held is
        // lost, and the free-space count (which has to include them) disagrees with the sentinel
        // pointer. Seen as one extra rejected name applying the conversion roster to CXROM v105.
        [TestMethod]
        public void LastNameInArea_GettingShorter_ClearsItsOldTail()
        {
            var cases = new[] {
                new { Rom = "TSPRBOWL.nes", Team = "falcons", Sentinel = 0x6d8 },
                new { Rom = "TSB 2007-32-105.nes", Team = "panthers", Sentinel = 0x6e4 },
            };
            foreach (var c in cases)
            {
                byte[] rom = TestRoms.LoadRom(c.Rom);
                TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
                RunAndCollectErrors(() =>
                {
                    tool.InsertPlayer(c.Team, "P", "abcdef", "ABCDEFGH", 0x11);
                    tool.InsertPlayer(c.Team, "P", "a", "BC", 0x11);
                });

                int sentinelEnd = MainNameDataEnd(rom, c.Sentinel);
                for (int i = sentinelEnd; i <= NameAreaEnd; i++)
                    Assert.AreEqual(0xFF, rom[i], string.Format("{0}: byte 0x{1:X} after the data's end should be cleared.", c.Rom, i));
                Assert.AreEqual("a BC", tool.GetName(c.Team, "P"), c.Rom);
            }

            // CXROM's expansion area: cardinals P is its last name.
            byte[] cx = TestRoms.LoadRom("TSB 2007-32-105.nes");
            TecmoTool cxTool = (TecmoTool)TecmoToolFactory.GetToolForRom(cx);
            RunAndCollectErrors(() =>
            {
                cxTool.InsertPlayer("cardinals", "P", "abcdef", "ABCDEFGH", 0x11);
                cxTool.InsertPlayer("cardinals", "P", "a", "BC", 0x11);
            });
            int expansionEnd = cxTool.GetNextDataPosition("cardinals", "P");
            for (int i = expansionEnd; i < expansionEnd + 14; i++)
                Assert.AreEqual(0xFF, cx[i], string.Format("CXROM expansion: byte 0x{0:X} after the data's end should be cleared.", i));
        }

        // CXROM expansion names (49ers/rams/seahawks/cardinals) must read back exactly as written after
        // length-changing edits, on both CXROM layouts. v111's expansion pointers sit at 0x3E054+, past
        // the team string table's start (0x1FC10), and CXRomTSBTool.AdjustDataPointers used to treat
        // any pointer past that start as a team-string pointer -- so v111's expansion pointers were
        // never moved when names shifted, and later names read with their neighbours' bytes attached
        // (seen in FullRomDumpTests' Test9 golden output as "alan BRANCHr", "ben GRAHAMch").
        [TestMethod]
        public void CxRom_ExpansionNameEdits_AllExpansionNamesReadBackExactly()
        {
            string[] expansionTeams = { "49ers", "rams", "seahawks", "cardinals" };
            foreach (string romName in new[] { "TSB 2007-32-105.nes", "TSB 2007-32-111.nes" })
            {
                byte[] rom = TestRoms.LoadRom(romName);
                TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
                var expected = new Dictionary<string, string>();
                List<string> errors = RunAndCollectErrors(() =>
                {
                    // Alternate shorter and longer names so pointers move both ways; the total stays
                    // close to stock so v105's 26 free bytes are enough.
                    int n = 0;
                    foreach (string team in expansionTeams)
                        foreach (string pos in TecmoTool.PositionNames)
                        {
                            string old = tool.GetName(team, pos);
                            int oldLast = old.Substring(old.IndexOf(' ') + 1).Length;
                            int len = Math.Min(10, Math.Max(2, (n++ % 2 == 0) ? oldLast - 2 : oldLast + 2));
                            string last = new string((char)('A' + n % 26), len);
                            string first = "x" + (char)('a' + n % 26);
                            tool.InsertPlayer(team, pos, first, last, 0x11);
                            expected[team + " " + pos] = first + " " + last;
                        }
                });
                Assert.AreEqual(0, errors.Count, romName + ": " + string.Join("\n", errors.ToArray()));

                List<string> wrong = new List<string>();
                foreach (string team in expansionTeams)
                    foreach (string pos in TecmoTool.PositionNames)
                    {
                        string actual = tool.GetName(team, pos);
                        if (actual != expected[team + " " + pos])
                            wrong.Add(string.Format("{0} {1}: expected '{2}', read '{3}'", team, pos, expected[team + " " + pos], actual));
                    }
                Assert.AreEqual(0, wrong.Count, romName + ":\n" + string.Join("\n", wrong.ToArray()));
            }
        }

        // A name that fits must still be written normally (the check must not reject valid edits).
        [TestMethod]
        public void NesRom_NameThatFits_IsWritten()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            List<string> errors = RunAndCollectErrors(() => tool.InsertPlayer("falcons", "P", "joe", "LONGERNAME", 0x11));
            Assert.AreEqual(0, errors.Count, string.Join("\n", errors.ToArray()));
            Assert.AreEqual("joe LONGERNAME", tool.GetName("falcons", "P"));
        }

        // CXROM expansion area (49ers/rams/seahawks/cardinals): only 26 bytes free on v105. Growth
        // past ExpansionAreaEnd is rejected, and the 14 bytes CXRomTSBTool guards at save are untouched.
        [TestMethod]
        public void CxRomV105_ExpansionNamesGrowUntilFull_ProtectedBytesUntouched_OverflowReported()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            byte[] before = (byte[])rom.Clone();
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> errors = RunAndCollectErrors(() =>
            {
                foreach (string team in new[] { "49ers", "rams", "seahawks", "cardinals" })
                    foreach (string pos in TecmoTool.PositionNames)
                        tool.InsertPlayer(team, pos, "abcdefgh", "ABCDEFGH", 0x11);
            });

            for (int i = CxRomProtectedBytes; i < CxRomProtectedBytes + 14; i++)
                Assert.AreEqual(before[i], rom[i], string.Format("Protected byte 0x{0:X} changed.", i));
            Assert.IsTrue(tool.GetNextDataPosition("cardinals", "P") <= ExpansionAreaEnd);
            Assert.IsTrue(errors.Any(e => e.Contains("would overflow")), "Expected overflow rejections to be reported.");
        }
    }
}
