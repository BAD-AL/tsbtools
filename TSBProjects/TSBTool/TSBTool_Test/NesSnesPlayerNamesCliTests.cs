using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    // CLI wiring for PlayerNames (see MainClass.ModifyStuff, mirroring MainGUI.ApplyToRom) -- the NES/SNES
    // counterpart to GenesisCliTests' auto-trim tests. Unlike Genesis, a single over-long name can never
    // overflow on its own here (SNES_TecmoTool/TecmoTool's own InsertPlayer already caps + auto-abbreviates
    // any one name), so these tests instead push *many* positions to their cap at once, so the cumulative
    // growth genuinely exceeds the ROM's real stock slack (NES ~483 bytes, SNES ~1552). Unlike Genesis's
    // CLI, there's no "rejected write, old name kept" case here to test: NES/SNES's own InsertPlayer has
    // no bounds check at all and just applies whatever it's given (auto-abbreviated to its own cap), so
    // -noAutoFit's only real, testable guarantee is that it skips the auto-trim step itself.
    //
    // Uses relative filenames + a CurrentDirectory swap, exactly like GenesisCliTests' RunInScratchDir --
    // MainClass's own "-out:"/"-get:" option parsing does a naive Split(':'), which mangles an absolute
    // Windows path (the drive letter's own colon splits it too), so an absolute "-out:" value silently
    // saves to a bogus one-or-two-character filename instead. Relative filenames are the only form that
    // survives that parsing intact.
    [TestClass]
    public class NesSnesPlayerNamesCliTests
    {
        [TestMethod]
        public void SnesRom_CliDefault_AutoTrimsUnspecifiedPlayersSoALargeRosterEditSucceeds()
        {
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "tsb1.smc";
                File.Copy(TestRoms.GetRomPath("TSB1.smc"), Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), BuildOverflowingRosterText(PlayerNamesConfig.Snes, 10));
                string outputFile = "output.smc";

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + outputFile });

                Assert.IsTrue(File.Exists(Path.Combine(scratchDir, outputFile)),
                    "The auto-trim step should have freed enough room for ModifyStuff to succeed and write the output file.");
            });
        }

        [TestMethod]
        public void SnesRom_CliNoAutoFit_SkipsAutoTrimSoUnspecifiedPlayersKeepTheirRealNames()
        {
            RunInScratchDir(scratchDir =>
            {
                string sourceRomPath = TestRoms.GetRomPath("TSB1.smc");
                ITecmoTool sourceTool = (ITecmoTool)TecmoToolFactory.GetToolForRom(File.ReadAllBytes(sourceRomPath));
                // "49ers" isn't among the first 10 teams BuildOverflowingRosterText specifies, so its
                // QB1 is a genuine auto-trim candidate -- with -noAutoFit, it must be left alone.
                string originalName = sourceTool.GetTeamPlayers("49ers");

                string localRomName = "tsb1.smc";
                File.Copy(sourceRomPath, Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), BuildOverflowingRosterText(PlayerNamesConfig.Snes, 10));
                string outputFile = "output.smc";

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + outputFile, "-noAutoFit" });

                string outputPath = Path.Combine(scratchDir, outputFile);
                Assert.IsTrue(File.Exists(outputPath), "The roster text itself is still applied either way with -noAutoFit; only the auto-trim step is skipped.");
                ITecmoTool outputTool = (ITecmoTool)TecmoToolFactory.GetToolForRom(File.ReadAllBytes(outputPath));
                Assert.AreEqual(originalName, outputTool.GetTeamPlayers("49ers"),
                    "With -noAutoFit, unspecified players must be left completely untouched.");
            });
        }

        [TestMethod]
        public void NesRom_CliDefault_AutoTrimsUnspecifiedPlayersSoALargeRosterEditSucceeds()
        {
            RunInScratchDir(scratchDir =>
            {
                string localRomName = "tsprbowl.nes";
                File.Copy(TestRoms.GetRomPath("TSPRBOWL.nes"), Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), BuildOverflowingRosterText(PlayerNamesConfig.Nes, 5));
                string outputFile = "output.nes";

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + outputFile });

                Assert.IsTrue(File.Exists(Path.Combine(scratchDir, outputFile)),
                    "The auto-trim step should have freed enough room for ModifyStuff to succeed and write the output file.");
            });
        }

        [TestMethod]
        public void NesRom_CliNoAutoFit_SkipsAutoTrimSoUnspecifiedPlayersKeepTheirRealNames()
        {
            RunInScratchDir(scratchDir =>
            {
                string sourceRomPath = TestRoms.GetRomPath("TSPRBOWL.nes");
                ITecmoTool sourceTool = (ITecmoTool)TecmoToolFactory.GetToolForRom(File.ReadAllBytes(sourceRomPath));
                // "saints" isn't among the first 5 teams BuildOverflowingRosterText specifies (and isn't
                // NES's own special-cased last team either), so its roster is a genuine auto-trim
                // candidate -- with -noAutoFit, it must be left alone.
                string originalName = sourceTool.GetTeamPlayers("saints");

                string localRomName = "tsprbowl.nes";
                File.Copy(sourceRomPath, Path.Combine(scratchDir, localRomName));
                string inputFile = "input.txt";
                File.WriteAllText(Path.Combine(scratchDir, inputFile), BuildOverflowingRosterText(PlayerNamesConfig.Nes, 5));
                string outputFile = "output.nes";

                MainClass.RunMain(new[] { localRomName, inputFile, "-out:" + outputFile, "-noAutoFit" });

                string outputPath = Path.Combine(scratchDir, outputFile);
                Assert.IsTrue(File.Exists(outputPath), "The roster text itself is still applied either way with -noAutoFit; only the auto-trim step is skipped.");
                ITecmoTool outputTool = (ITecmoTool)TecmoToolFactory.GetToolForRom(File.ReadAllBytes(outputPath));
                Assert.AreEqual(originalName, outputTool.GetTeamPlayers("saints"),
                    "With -noAutoFit, unspecified players must be left completely untouched.");
            });
        }

        // Every position on the first teamCount teams (deliberately avoiding the last team, whose last
        // roster slot is a special-cased sentinel-less edge case on both platforms) gets a name at that
        // platform's own per-name cap -- enough cumulative growth across enough positions to exceed
        // either platform's real stock slack (measured elsewhere this session: NES ~483 bytes, SNES
        // ~1552 bytes).
        private static string BuildOverflowingRosterText(PlayerNamesConfig config, int teamCount)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < teamCount; i++)
            {
                sb.Append("TEAM = ").Append(config.Teams[i]).Append('\n');
                // fname is always collapsed to a single initial by InsertPlayer's own auto-abbreviation
                // once the combined length exceeds the platform cap, so a long fname buys nothing -- a
                // 14-char lname (below both platforms' own LnameTruncateThreshold, so never *itself*
                // further truncated) is what actually maximizes each written record's real length.
                foreach (string pos in config.Positions)
                    sb.Append(pos).Append(", aaaaaaaaaa BBBBBBBBBBBBBB, #12,\n");
            }
            return sb.ToString();
        }

        private static void RunInScratchDir(Action<string> test)
        {
            string scratchDir = Path.Combine(Path.GetTempPath(), "TSBToolPlayerNamesCliTest_" + Guid.NewGuid().ToString("N"));
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
    }
}
