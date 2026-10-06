using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// PlayerNames' pre-apply name-fit check used to give every CXROM type the plain NES settings:
    /// the NES 28-team list (with "oilers", without jaguars/panthers/ravens/texans) and the NES name
    /// area. On a 32-team CXROM ROM that skipped the growth of the titans/jaguars/panthers/ravens/
    /// texans names entirely, counted the 49ers/rams/seahawks/cardinals (which live in CXROM's separate
    /// expansion name area) against the main area, and asked the ROM for team "oilers" -- the
    /// "GetTeamPlayers:: team oilers is invalid" error seen applying
    /// TSBTool_Test\ConversionTest\ConversionTest.txt to CXROM v105. CXROM now gets two settings, one
    /// per name area, each with that area's real teams and bounds.
    /// </summary>
    [TestClass]
    public class CxRomPlayerNamesTests
    {
        private static readonly string[] ExpansionTeams = { "49ers", "rams", "seahawks", "cardinals" };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        // Free bytes measured directly on the stock ROMs (main area to 0x300F, expansion area to 0x3FFDF).
        [TestMethod]
        public void ResolveConfigsFor_CxRomVariants_MainAndExpansionAreasWithRealTeamsAndFreeSpace()
        {
            var cases = new[] {
                new { Rom = "TSB 2007-32-105.nes", MainFree = 109, ExpansionFree = 26 },
                new { Rom = "TSB 2007-32-111.nes", MainFree = 109, ExpansionFree = 6350 },
                new { Rom = "SbluemanBase_18week_RealNFL-1.nes", MainFree = 177, ExpansionFree = 72 },
            };
            foreach (var c in cases)
            {
                ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(c.Rom));
                List<PlayerNamesConfig> configs = PlayerNames.ResolveConfigsFor(tool);
                Assert.AreEqual(2, configs.Count, c.Rom + ": expected a main and an expansion name area.");

                PlayerNamesConfig main = configs[0];
                PlayerNamesConfig expansion = configs[1];
                Assert.AreEqual(28, main.Teams.Length, c.Rom);
                foreach (string team in new[] { "titans", "jaguars", "panthers", "ravens", "texans", "bills" })
                    CollectionAssert.Contains(main.Teams, team, c.Rom + " main area should include " + team);
                CollectionAssert.DoesNotContain(main.Teams, "oilers", c.Rom);
                foreach (string team in ExpansionTeams)
                    CollectionAssert.DoesNotContain(main.Teams, team, c.Rom + " " + team + " lives in the expansion area");
                CollectionAssert.AreEqual(ExpansionTeams, expansion.Teams, c.Rom);
                Assert.AreEqual(0x300F, main.CapacityBoundaryAddress, c.Rom);
                Assert.AreEqual(0x3FFDF, expansion.CapacityBoundaryAddress, c.Rom);

                Assert.AreEqual(c.MainFree, PlayerNames.ResolveFit(tool, main, "", false).AvailableSlack, c.Rom + " main area free bytes");
                Assert.AreEqual(c.ExpansionFree, PlayerNames.ResolveFit(tool, expansion, "", false).AvailableSlack, c.Rom + " expansion area free bytes");
            }
        }

        [TestMethod]
        public void ResolveConfigsFor_NesAndSnes_StillSingleArea()
        {
            ITecmoTool nes = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSPRBOWL.nes"));
            CollectionAssert.AreEqual(new[] { PlayerNamesConfig.Nes }, PlayerNames.ResolveConfigsFor(nes));
            ITecmoTool snes = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB1.smc"));
            CollectionAssert.AreEqual(new[] { PlayerNamesConfig.Snes }, PlayerNames.ResolveConfigsFor(snes));
        }

        // The conversion roster's main-area names need far more than the 109 free bytes; the check must
        // see that (counting every base team, titans/jaguars/etc. included) and raise no team errors.
        [TestMethod]
        public void ResolveFit_CxRomV105_ConversionTestRoster_CountsEveryBaseTeam_NoInvalidTeamError()
        {
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB 2007-32-105.nes"));
            string text = File.ReadAllText(TestRoms.GetConversionTestPath("ConversionTest.txt"));
            PlayerNamesConfig main = PlayerNames.ResolveConfigsFor(tool)[0];

            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            PlayerNamesFitReport report;
            try
            {
                report = PlayerNames.ResolveFit(tool, main, text, false);
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }

            Assert.AreEqual(0, giver.Errors.Count, string.Join("\n", giver.Errors.ToArray()));
            Assert.AreEqual(109, report.AvailableSlack);
            Assert.IsFalse(report.Fits, "This roster needs more room than the main area has.");
            Assert.IsTrue(report.NeededSlack > 109, "NeededSlack " + report.NeededSlack);
            Assert.IsTrue(report.Suggestions.Count + report.UnresolvedFailures.Count > 0,
                "Expected the report to name players to shorten.");
        }

        // ApplySuggestions rewrites exactly the suggested players' lines (first name -> initial) and
        // nothing else -- same team/position matching ProcessText uses.
        [TestMethod]
        public void ApplySuggestions_RewritesOnlySuggestedPlayers()
        {
            string text =
                "TEAM = chiefs SimData=0xc53\n" +
                "RB2, brashard SMITH, Face=0x10, #22, 38, 69, 31, 13, 50, 38 ,[4, 4, 6, 0 ]\n" +
                "RB3, emmett JOHNSON, Face=0x10, #27, 38, 69, 31, 13, 50, 38 ,[4, 4, 6, 0 ]\n" +
                "TEAM = jets SimData=0x222\n" +
                "RB2, brashard SMITH, Face=0x10, #22, 38, 69, 31, 13, 50, 38 ,[4, 4, 6, 0 ]\n";
            PlayerNamesFitReport report = new PlayerNamesFitReport();
            PlayerNamesSuggestion s = new PlayerNamesSuggestion();
            s.Team = "chiefs";
            s.Position = "RB2";
            s.ProposedName = "brashard SMITH";
            s.SuggestedName = "b. SMITH";
            report.Suggestions.Add(s);

            string result = PlayerNames.ApplySuggestions(text, report);

            string[] lines = result.Split('\n');
            Assert.AreEqual("RB2, b. SMITH, Face=0x10, #22, 38, 69, 31, 13, 50, 38 ,[4, 4, 6, 0 ]", lines[1]);
            Assert.AreEqual("RB3, emmett JOHNSON, Face=0x10, #27, 38, 69, 31, 13, 50, 38 ,[4, 4, 6, 0 ]", lines[2], "Not suggested -- unchanged.");
            Assert.AreEqual("RB2, brashard SMITH, Face=0x10, #22, 38, 69, 31, 13, 50, 38 ,[4, 4, 6, 0 ]", lines[4], "Other team -- unchanged.");
        }

        // When a first initial alone doesn't free enough room, the last name is cut to whatever fits
        // instead of leaving the player unfixable. (An unfixable name used to be rejected at write
        // time while the rest of its line -- face, ratings, sim data -- still applied, leaving the OLD
        // player's name and jersey on the NEW player's ratings.)
        [TestMethod]
        public void ResolveFit_CxRomV105_ConversionTestRoster_InitialNotEnough_LastNameCutToFit_NothingUnfixable()
        {
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB 2007-32-105.nes"));
            string text = File.ReadAllText(TestRoms.GetConversionTestPath("ConversionTest.txt"));

            List<string> truncated = new List<string>();
            foreach (PlayerNamesConfig config in PlayerNames.ResolveConfigsFor(tool))
            {
                PlayerNamesFitReport report = PlayerNames.ResolveFit(tool, config, text, false);
                Assert.AreEqual(0, report.UnresolvedFailures.Count, config.PlatformName + ":\n" + report);
                foreach (PlayerNamesSuggestion s in report.Suggestions.Where(x => x.LastNameTruncated))
                {
                    truncated.Add(s.Team + " " + s.Position);
                    string proposedLast = s.ProposedName.Substring(s.ProposedName.IndexOf(' ') + 1);
                    string suggestedLast = s.SuggestedName.Substring(s.SuggestedName.IndexOf(' ') + 1);
                    Assert.IsTrue(suggestedLast.Length >= 1 && suggestedLast.Length < proposedLast.Length && proposedLast.StartsWith(suggestedLast),
                        string.Format("{0} {1}: '{2}' -> '{3}' should keep the start of the last name", s.Team, s.Position, s.ProposedName, s.SuggestedName));
                }
                Assert.IsTrue(report.ToString().Contains("last name"), "The report should say which last names get cut:\n" + report);
            }
            // The formerly unfixable players get cut last names. A cut name uses up the room that's left
            // (an unfixable one used none), so a later player in file order can need a cut too --
            // here texans P, 'kai KROEGER' -> 'k. KROEGE'.
            foreach (string p in new[] { "ravens RE", "ravens NT", "ravens FS", "49ers NT", "49ers P" })
                CollectionAssert.Contains(truncated, p);
            Assert.AreEqual(6, truncated.Count, string.Join(", ", truncated.ToArray()));
        }

        // With the suggestions actually applied, nothing is rejected: every suggested player -- first
        // initial or cut last name -- gets exactly the name the report promised; everyone else keeps
        // their full name.
        [TestMethod]
        public void Cli_CxRomV105_ConversionTestRoster_SuggestedShorteningsApplied_NothingRejected()
        {
            string text = File.ReadAllText(TestRoms.GetConversionTestPath("ConversionTest.txt"));
            ITecmoTool preview = (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB 2007-32-105.nes"));
            List<PlayerNamesSuggestion> suggestions = new List<PlayerNamesSuggestion>();
            foreach (PlayerNamesConfig config in PlayerNames.ResolveConfigsFor(preview))
                suggestions.AddRange(PlayerNames.ResolveFit(preview, config, text, false).Suggestions);

            RunCli(new[] { "rom.nes", "input.txt", "-out:out.nes" }, (stdout, stderr, rom) =>
            {
                int rejected = stderr.Split('\n').Count(l => l.Contains("would overflow"));
                Assert.AreEqual(0, rejected, "No name should be rejected:\n" + stderr);

                TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
                foreach (PlayerNamesSuggestion s in suggestions)
                    Assert.AreEqual(s.SuggestedName, tool.GetName(s.Team, s.Position), s.Team + " " + s.Position);
                Assert.AreEqual("b. SMITH", tool.GetName("chiefs", "RB2"), "Suggested shortening should be applied.");
                Assert.AreEqual("josh ALLEN", tool.GetName("bills", "QB1"), "Players that fit keep their full names.");
                int dataEnd = (rom[0x6e4] | (rom[0x6e5] << 8)) - 0x8000 + 0x10;
                Assert.IsTrue(dataEnd <= 0x300F);
            });
        }

        // -noAutoFit: the report is shown but nothing is changed, so the names that don't fit are
        // rejected (old names kept) -- still never past the name area.
        [TestMethod]
        public void Cli_CxRomV105_ConversionTestRoster_NoAutoFit_SuggestionsNotApplied()
        {
            RunCli(new[] { "rom.nes", "input.txt", "-out:out.nes", "-noAutoFit" }, (stdout, stderr, rom) =>
            {
                TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
                Assert.AreNotEqual("b. SMITH", tool.GetName("chiefs", "RB2"), "-noAutoFit must not shorten names.");
                int dataEnd = (rom[0x6e4] | (rom[0x6e5] << 8)) - 0x8000 + 0x10;
                Assert.IsTrue(dataEnd <= 0x300F);
            });
        }

        private static void RunCli(string[] args, Action<string, string, byte[]> check)
        {
            string scratchDir = Path.Combine(Path.GetTempPath(), "TSBToolCxRomNamesTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scratchDir);
            string oldDir = Environment.CurrentDirectory;
            TextWriter oldErr = Console.Error, oldOut = Console.Out;
            StringWriter err = new StringWriter(), output = new StringWriter();
            try
            {
                Environment.CurrentDirectory = scratchDir;
                File.Copy(TestRoms.GetRomPath("TSB 2007-32-105.nes"), Path.Combine(scratchDir, "rom.nes"));
                File.Copy(TestRoms.GetConversionTestPath("ConversionTest.txt"), Path.Combine(scratchDir, "input.txt"));
                Console.SetError(err);
                Console.SetOut(output);
                MainClass.RunMain(args);
                Console.SetError(oldErr);
                Console.SetOut(oldOut);
                check(output.ToString(), err.ToString(), File.ReadAllBytes(Path.Combine(scratchDir, "out.nes")));
            }
            finally
            {
                Console.SetError(oldErr);
                Console.SetOut(oldOut);
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
                Environment.CurrentDirectory = oldDir;
                try { Directory.Delete(scratchDir, true); } catch { }
            }
        }

        // End to end through the command line, as the user ran it: no team errors, the name area stays
        // in bounds, and every Bills rating lands as written.
        [TestMethod]
        public void Cli_CxRomV105_ConversionTestRoster_NoTeamErrors_NameAreaInBounds_BillsRatingsIntact()
        {
            string scratchDir = Path.Combine(Path.GetTempPath(), "TSBToolCxRomNamesTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scratchDir);
            string oldDir = Environment.CurrentDirectory;
            TextWriter oldErr = Console.Error;
            StringWriter err = new StringWriter();
            try
            {
                Environment.CurrentDirectory = scratchDir;
                File.Copy(TestRoms.GetRomPath("TSB 2007-32-105.nes"), Path.Combine(scratchDir, "rom.nes"));
                File.Copy(TestRoms.GetConversionTestPath("ConversionTest.txt"), Path.Combine(scratchDir, "input.txt"));
                Console.SetError(err);

                MainClass.RunMain(new[] { "rom.nes", "input.txt", "-out:out.nes" });

                Console.SetError(oldErr);
                string stderr = err.ToString();
                Assert.IsFalse(stderr.Contains("is invalid"), "Unexpected team error:\n" + stderr);

                byte[] rom = File.ReadAllBytes(Path.Combine(scratchDir, "out.nes"));
                int dataEnd = (rom[0x6e4] | (rom[0x6e5] << 8)) - 0x8000 + 0x10;
                Assert.IsTrue(dataEnd <= 0x300F, string.Format("Main name data ends at 0x{0:X}.", dataEnd));

                TecmoTool outTool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
                string text = File.ReadAllText(Path.Combine(scratchDir, "input.txt"));
                foreach (string line in text.Replace("\r\n", "\n").Split('\n')
                    .SkipWhile(l => !l.StartsWith("TEAM = bills")).Skip(1)
                    .TakeWhile(l => !l.StartsWith("TEAM =")).Where(l => l.Contains("Face=")))
                {
                    string pos = line.Split(',')[0].Trim();
                    int[] expected = InputParser.GetInts(line);
                    int[] actual = outTool.GetAbilities("bills", pos);
                    CollectionAssert.AreEqual(expected.Take(actual.Length).ToArray(), actual, "bills " + pos);
                }
            }
            finally
            {
                Console.SetError(oldErr);
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
                Environment.CurrentDirectory = oldDir;
                try { Directory.Delete(scratchDir, true); } catch { }
            }
        }
    }
}
