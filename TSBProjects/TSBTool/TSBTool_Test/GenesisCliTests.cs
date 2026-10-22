using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// CLI parity with the SNES version: MainClass.RunMain must recognize a Genesis ROM by filename
    /// (like FullRomDumpTests.cs does for .nes/.smc) and support the same apply/dump flow. No
    /// checked-in golden fixture is used here (unlike FullRomDumpTests.cs's legacy-derived ones) --
    /// since there's no historical Genesis dump to diff against, these tests instead verify the CLI's
    /// own dump is idempotent (dump -> apply that exact text back -> dump again -> same text), the same
    /// property the checked-in fixtures elsewhere in this project were themselves verified against
    /// before being committed (see TestDocument.md).
    /// </summary>
    [TestClass]
    public class GenesisCliTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        [TestMethod]
        public void GenesisRom_MainClassRecognizesRomByFilename_DumpsViaCli()
        {
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));

                MainClass.TestString = "";
                MainClass.RunMain(new[] { localRomName });

                StringAssert.Contains(MainClass.TestString, "TEAM = bills");
                StringAssert.Contains(MainClass.TestString, "YEAR=1993");
            });
        }

        [TestMethod]
        public void GenesisRom_CliApplyThenDump_StabilizesToAFixedPoint()
        {
            // Dump the stock ROM, apply that dump, dump again: the two dumps match except for the 4 stock
            // players with a multi-word last name ("al DEL GRECO" comes back "al del GRECO" -- the text
            // format's "first LAST" rule, kept on purpose). Every Face= value survives, including the
            // 8 stock players whose +2 byte has the unexplained 0x20 bit: SetFace changes only the 0x80
            // skin bit (it used to write 0x00/0x80 and clear the 0x20). Applying the second dump again
            // gives an identical third dump.
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));

                MainClass.TestString = "";
                MainClass.RunMain(new[] { localRomName });
                string firstDump = MainClass.TestString;
                StringAssert.Contains(firstDump, "TEAM = bills");

                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), firstDump);
                string firstOutput = "output1.md";
                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + firstOutput });
                Assert.IsTrue(File.Exists(Path.Combine(scratchDir, firstOutput)), "Expected ModifyStuff to write " + firstOutput);

                MainClass.TestString = "";
                MainClass.RunMain(new[] { firstOutput });
                string secondDump = MainClass.TestString;
                AssertSameExceptLastNameCase(Normalize(firstDump), Normalize(secondDump), 4);

                File.WriteAllText(Path.Combine(scratchDir, inputFile), secondDump);
                string secondOutput = "output2.md";
                MainClass.RunMain(new[] { firstOutput, inputFile, "-out:" + secondOutput });

                MainClass.TestString = "";
                MainClass.RunMain(new[] { secondOutput });
                string thirdDump = MainClass.TestString;

                AssertLinesEqual(Normalize(secondDump), Normalize(thirdDump));
            });
        }

        [TestMethod]
        public void GenesisRom_CliDefault_AutoTrimsUnspecifiedPlayersSoAnOverLongNameSucceeds()
        {
            // Bills QB1 stock "jimKELLY" (oldLength=9) -> "alexander THUNDERBOLT" (newLength=21, +12)
            // needs 12 bytes the stock ROM's zero slack doesn't have. Without any pre-trimming this
            // write would be rejected by InsertPlayer's bounds check (see
            // GenesisRosterTests.GenesisRom_InsertPlayer_RejectsWriteThatWouldOverflowIntoAttributeTable_*).
            // The CLI's default behavior (no -noAutoFit) must run ResolveRosterFit(autoApply:true) first
            // so this succeeds anyway.
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), "TEAM = bills SimData=0x0\nQB1, alexander THUNDERBOLT, #7\n");
                string outputFile = "output.md";

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + outputFile });

                byte[] outputRom = File.ReadAllBytes(Path.Combine(scratchDir, outputFile));
                Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(outputRom);
                Assert.AreEqual("alexanderTHUNDERBOLT", tool.GetPlayerName("bills", "QB1"),
                    "The over-long name must have succeeded -- only possible if the CLI auto-trimmed unspecified players first.");
            });
        }

        [TestMethod]
        public void GenesisRom_CliNoAutoFit_SkipsAutoTrimSoTheSameOverLongNameIsRejected()
        {
            // Same scenario as above, but with -noAutoFit: no pre-trimming happens, so this specific
            // write must fail exactly like a direct InsertPlayer call would against the stock ROM's zero
            // slack -- Bills QB1 keeps its OLD stock name instead.
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), "TEAM = bills SimData=0x0\nQB1, alexander THUNDERBOLT, #7\n");
                string outputFile = "output.md";

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + outputFile, "-noAutoFit" });

                byte[] outputRom = File.ReadAllBytes(Path.Combine(scratchDir, outputFile));
                Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(outputRom);
                Assert.AreEqual("jimKELLY", tool.GetPlayerName("bills", "QB1"),
                    "With -noAutoFit, no unspecified player should be renamed, so this write should still be rejected and the original name kept.");
            });
        }

        [TestMethod]
        public void GenesisRom_CliDefault_FullRosterOfLongerNames_ShorteningsApplied_NothingRejected()
        {
            // Every roster slot specified, each name lengthened -- so trimming unmentioned players can't
            // help, and the report's first-initial and cut-last-name shortenings must really be applied
            // (they used to be report-only: those players were rejected, keeping the OLD name while the
            // rest of their line still applied).
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));
                Genesis_TSB1Tool stock = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));
                string text = System.Text.RegularExpressions.Regex.Replace(stock.GetAll(1),
                    "^([A-Z]+[1-4]?, )([a-z.]+) ([A-Z.]+),", "${1}${2}xx ${3}XX,", System.Text.RegularExpressions.RegexOptions.Multiline);
                File.WriteAllText(Path.Combine(scratchDir, "input.txt"), text);

                Genesis_TSB1Tool.RosterFitReport dryRun = stock.ResolveRosterFit(text, false);
                Assert.AreEqual(0, dryRun.UnresolvedFailures.Count, dryRun.ToString());
                Assert.IsTrue(dryRun.Suggestions.Exists(s => s.LastNameTruncated), "Sanity: this roster needs cut last names.\n" + dryRun);

                TextWriter oldErr = Console.Error;
                StringWriter err = new StringWriter();
                Console.SetError(err);
                try { MainClass.RunMain(new[] { localRomName, "input.txt", "-out:output.md" }); }
                finally { Console.SetError(oldErr); }

                Assert.IsFalse(err.ToString().Contains("would overflow"), err.ToString());
                Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(File.ReadAllBytes(Path.Combine(scratchDir, "output.md")));
                foreach (Genesis_TSB1Tool.RosterFitSuggestion s in dryRun.Suggestions)
                    Assert.AreEqual(s.SuggestedName.Replace(" ", ""), tool.GetPlayerName(s.Team, s.Position), s.Team + " " + s.Position);
            });
        }

        [TestMethod]
        public void Cli_OutFileName_KeepsItsCase()
        {
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), "TEAM = bills SimData=0x0\n");

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:GEN_Output_Mixed.md" });

                // File.Exists ignores case on Windows, so look at the name actually written
                string[] written = Directory.GetFiles(scratchDir, "*output_mixed.md");
                Assert.AreEqual(1, written.Length);
                Assert.AreEqual("GEN_Output_Mixed.md", Path.GetFileName(written[0]));
            });
        }

        [TestMethod]
        public void Cli_OutFileName_FullPathWithDrive_IsUsed()
        {
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), "TEAM = bills SimData=0x0\n");
                string fullOut = Path.Combine(Path.Combine(scratchDir, "sub dir"), "Full.md");
                Directory.CreateDirectory(Path.GetDirectoryName(fullOut));
                StringAssert.Contains(fullOut, ":"); // e.g. C:\Users\...

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + fullOut });

                Assert.IsTrue(File.Exists(fullOut), "Expected the ROM written to " + fullOut);
            });
        }

        [TestMethod]
        public void Cli_GetFileName_FullPathWithDrive_IsUsed()
        {
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "genesis.md";
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, localRomName));
                string getFile = Path.Combine(Path.Combine(scratchDir, "sub dir"), "Locations.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(getFile));
                File.WriteAllText(getFile, "0x100 - 0x10F\n");
                StringAssert.Contains(getFile, ":"); // e.g. C:\Users\...

                TextWriter originalOut = Console.Out;
                StringWriter captured = new StringWriter();
                Console.SetOut(captured);
                try
                {
                    MainClass.RunMain(new[] { localRomName, "-get:" + getFile });
                }
                finally
                {
                    Console.SetOut(originalOut);
                }

                StringAssert.Contains(captured.ToString(), "SET(0x100", "Expected the -get: file's locations printed.");
            });
        }

        private static void RunInScratchDir(Action<string> test)
        {
            string scratchDir = Path.Combine(Path.GetTempPath(), "TSBToolGenesisCliTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scratchDir);
            string originalCurrentDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = scratchDir;
                test(scratchDir);
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

        /// <summary>
        /// 'actual' has the same lines as 'expected', except exactly 'caseOnly' lines that differ only in letter case
        /// (a multi-word last name's first word comes back lowercase); every Face= value is the same.
        /// </summary>
        private static void AssertSameExceptLastNameCase(string expected, string actual, int caseOnly)
        {
            string[] expectedLines = expected.Split('\n');
            string[] actualLines = actual.Split('\n');
            Assert.AreEqual(expectedLines.Length, actualLines.Length, "line count");
            int differing = 0;
            for (int i = 0; i < expectedLines.Length; i++)
            {
                string e = expectedLines[i], a = actualLines[i];
                Assert.AreEqual(FaceOf(e), FaceOf(a), "Face at line " + i + ": " + e);
                if (e == a)
                    continue;
                if (!string.Equals(e, a, StringComparison.OrdinalIgnoreCase))
                    Assert.Fail("Line {0} differs by more than letter case:\nExpected: {1}\nActual:   {2}", i, e, a);
                differing++;
            }
            Assert.AreEqual(caseOnly, differing, "lines differing only in letter case (multi-word last names)");
        }

        private static string FaceOf(string line)
        {
            int at = line.IndexOf("Face=");
            return at < 0 ? null : line.Substring(at, line.IndexOf(',', at) - at);
        }

        private static void AssertLinesEqual(string expected, string actual)
        {
            string[] expectedLines = expected.Split('\n');
            string[] actualLines = actual.Split('\n');
            int max = Math.Max(expectedLines.Length, actualLines.Length);
            for (int i = 0; i < max; i++)
            {
                string e = i < expectedLines.Length ? expectedLines[i] : "<missing>";
                string a = i < actualLines.Length ? actualLines[i] : "<missing>";
                if (e != a)
                    Assert.Fail("First difference at line {0}:\nExpected: {1}\nActual:   {2}", i, e, a);
            }
        }
    }
}
