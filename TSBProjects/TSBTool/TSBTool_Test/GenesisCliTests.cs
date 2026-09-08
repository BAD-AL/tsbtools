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
            // A *single* apply-then-redump of the stock ROM's own dump is NOT guaranteed to be
            // byte-identical: SetFace can only ever write 0x00 or 0x80 (matching SNES_TecmoTool.SetFace
            // exactly), but 8 of 896 players' stock +2 byte is a third value (0x20) that predates this
            // tool and isn't representable through the Face= text field. That's a real, inherent
            // limitation inherited from SNES's own design, not a bug -- see Genesis_TSB1_ROM_Findings.md.
            // What SHOULD be idempotent is re-applying a dump that's ALREADY in the tool's own
            // representable form: apply once (collapsing any 0x20s), dump, apply that same dump again,
            // dump again -- those last two dumps must match exactly.
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
