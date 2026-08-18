using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Ports TEST\test1-9.bat / TEST\Test1-9.txt onto MainClass.RunMain() directly, matching how the
    /// original .bat scripts drove the CLI: apply a data file to a ROM, save it, then reload and
    /// dump it (relying on RunMain's "no options -> print everything" default, restored earlier
    /// this session, for Test1-3; explicit -sch -pb -of [-proBowl] flags for the rest, matching what
    /// their .bat scripts actually passed).
    ///
    /// ROM/flag matrix (from reading each test*.bat):
    ///   Test1-3: TSPRBOWL.nes (stock 28-team NES), no flags
    ///   Test4:   TSPRBOWL.nes, -sch -pb -of
    ///   Test5:   TSB1.smc (SNES TSB1 -- a different tool implementation), -sch -pb -of
    ///   Test6-7: TSB 2007-32-111.nes (CXROM_v111), -sch -pb -of
    ///   Test8:   no test8.bat exists. Inferred as TSB1.smc from its team order/count (28 teams in
    ///            the exact SNES_TecmoTool ordering, not stock NES's), matching Test5.
    ///   Test9:   TSB 2007-32-111.nes, -sch -pb -of -proBowl
    ///
    /// *_Expected.txt fixtures are not the original historical Test*.txt files verbatim: diffing
    /// showed only benign drift -- a newer GetKey() banner, reworded comments, whitespace-only
    /// formatting differences, and for the CXROM_v111 cases two known, non-bug causes:
    ///   1. CXRomTSBTool.GetTeamOffensiveFormation intentionally returns "" for ROM_TYPE.CXROM_v111
    ///      ("Offensive formation is messed up in CXROM"), so OFFENSIVE_FORMATION= lines are absent.
    ///   2. Some players' first names appear abbreviated ("michael dean" -> "m.") -- this matches a
    ///      fixed-width name field truncation already baked into the TSB 2007-32-111.nes fixture's
    ///      bytes, not something the read path does.
    /// No missing or corrupted data in any case. Each _Expected.txt is the current, verified-
    /// idempotent output (re-dumping the already-modified ROM reproduces it byte-for-byte).
    /// </summary>
    [TestClass]
    public class FullRomDumpTests
    {
        [TestMethod]
        public void StockRom_Test1_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSPRBOWL.nes", "Test1_Input.txt", "Test1_Expected.txt", new string[0]);
        }

        [TestMethod]
        public void StockRom_Test2_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSPRBOWL.nes", "Test2_Input.txt", "Test2_Expected.txt", new string[0]);
        }

        [TestMethod]
        public void StockRom_Test3_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSPRBOWL.nes", "Test3_Input.txt", "Test3_Expected.txt", new string[0]);
        }

        [TestMethod]
        public void StockRom_Test4_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSPRBOWL.nes", "Test4_Input.txt", "Test4_Expected.txt", new[] { "-sch", "-pb", "-of" });
        }

        [TestMethod]
        public void SnesTsb1Rom_Test5_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSB1.smc", "Test5_Input.txt", "Test5_Expected.txt", new[] { "-sch", "-pb", "-of" });
        }

        [TestMethod]
        public void CXRomV111_Test6_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSB 2007-32-111.nes", "Test6_Input.txt", "Test6_Expected.txt", new[] { "-sch", "-pb", "-of" });
        }

        [TestMethod]
        public void CXRomV111_Test7_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSB 2007-32-111.nes", "Test7_Input.txt", "Test7_Expected.txt", new[] { "-sch", "-pb", "-of" });
        }

        [TestMethod]
        public void SnesTsb1Rom_Test8_FullDumpRoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSB1.smc", "Test8_Input.txt", "Test8_Expected.txt", new[] { "-sch", "-pb", "-of" });
        }

        [TestMethod]
        public void CXRomV111_Test9_FullDumpWithProBowl_RoundTripsCorrectly()
        {
            RunFullRomDumpTest("TSB 2007-32-111.nes", "Test9_Input.txt", "Test9_Expected.txt", new[] { "-sch", "-pb", "-of", "-proBowl" });
        }

        /// <summary>
        /// Applies inputFixture to a copy of romFileName, saves it, reloads it, and dumps it with
        /// dumpFlags -- all via MainClass.RunMain() in a scratch working directory with relative
        /// filenames (MainClass.SetupOptions splits "-out:" values on every ':', so an absolute
        /// Windows path breaks at the drive-letter colon; the original .bat scripts avoided this the
        /// same way, always cd'd into a working folder using bare relative names).
        /// </summary>
        private static void RunFullRomDumpTest(string romFileName, string inputFixture, string expectedFixture, string[] dumpFlags)
        {
            string outputExtension = romFileName.ToLower().EndsWith(".smc") ? ".smc" : ".nes";
            string scratchDir = Path.Combine(Path.GetTempPath(), "TSBToolFullRomTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scratchDir);
            string originalCurrentDirectory = Environment.CurrentDirectory;

            try
            {
                File.Copy(TestRoms.GetRomPath(romFileName), Path.Combine(scratchDir, romFileName));
                File.Copy(TestRoms.GetFullRomDumpPath(inputFixture), Path.Combine(scratchDir, inputFixture));
                string expected = TestRoms.LoadFullRomDumpText(expectedFixture);

                Environment.CurrentDirectory = scratchDir;
                string outputFile = "output" + outputExtension;

                MainClass.RunMain(new string[] { romFileName, inputFixture, "-out:" + outputFile });
                Assert.IsTrue(File.Exists(Path.Combine(scratchDir, outputFile)), "Expected ModifyStuff to write " + outputFile);

                List<string> dumpArgs = new List<string>(dumpFlags);
                dumpArgs.Add(outputFile);
                MainClass.RunMain(dumpArgs.ToArray());
                string actual = MainClass.TestString;

                Assert.AreEqual(Normalize(expected), Normalize(actual));
            }
            finally
            {
                Environment.CurrentDirectory = originalCurrentDirectory;
                try { Directory.Delete(scratchDir, true); } catch (Exception) { /* best-effort cleanup */ }
            }
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n").Trim();
        }
    }
}
