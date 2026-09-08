using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 4 of compressed-riding-rossum.md: season 3's schedule only -- the real 1993 schedule (see
    /// Genesis_ScheduleHelper.cs for why season 3 specifically, and why not season 1 -- matching SNES
    /// TSB1's single-season shape, per the project owner's explicit architecture decision). Season 3
    /// always has exactly 18 weeks / 224 games; seasons 1 and 2 exist in the ROM but are untouched by
    /// this tool.
    /// </summary>
    [TestClass]
    public class GenesisScheduleTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        // Season 1 and season 2's data, split into the two disjoint ranges that actually belong to
        // them -- deliberately excludes season 3's OWN games-per-week array/schedule data, which a
        // season-3 write is of course expected to change.
        private static readonly int[,] Season1And2Ranges =
        {
            { 0x52EA, 0x530E }, // season 1 + season 2's games-per-week sentinel arrays
            { 0x5322, 0x56A2 }  // season 1 + season 2's schedule data (448 bytes each)
        };

        private static List<string> SplitToNonEmptyLines(string schedule)
        {
            return schedule
                .Replace("\r\n", "\n")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        [TestMethod]
        public void GenesisRom_Season3Schedule_HasCorrectGameCountsAndTeamBalance()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            List<string> gameLines = lines.Where(l => l.IndexOf(" at ", StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            Assert.AreEqual(224, gameLines.Count, "Season 3 should have exactly 224 games.");

            Dictionary<string, int> gamesPerTeam = new Dictionary<string, int>();
            foreach (string line in gameLines)
            {
                string[] parts = line.Split(new[] { " at " }, StringSplitOptions.None);
                Assert.AreEqual(2, parts.Length, "Malformed game line: " + line);
                foreach (string team in parts)
                {
                    if (!gamesPerTeam.ContainsKey(team))
                        gamesPerTeam[team] = 0;
                    gamesPerTeam[team]++;
                }
            }

            Assert.AreEqual(28, gamesPerTeam.Count, "Every one of the 28 teams should appear in the schedule.");
            foreach (KeyValuePair<string, int> kvp in gamesPerTeam)
            {
                Assert.AreEqual(16, kvp.Value, string.Format("{0} should appear in exactly 16 games, found {1}.", kvp.Key, kvp.Value));
            }
        }

        [TestMethod]
        public void GenesisRom_Season3Week1_MatchesRealNineteenNinetyThreeOpeningWeek()
        {
            // Spot-check against the real 1993 NFL opening week (Buffalo at New England, etc.),
            // decoded directly from the ROM and confirmed against real-world history -- see
            // Genesis_ScheduleHelper.cs's own comment for the full week-1 slate.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int weekIndex = lines.IndexOf("WEEK 1");
            Assert.IsTrue(weekIndex >= 0);

            Assert.AreEqual("bills at patriots", lines[weekIndex + 1]);
            Assert.AreEqual("browns at bengals", lines[weekIndex + 2]);
        }

        [TestMethod]
        public void GenesisRom_ApplyScheduleRoundTrip_IsIdempotent()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            string before = tool.GetSchedule();
            List<string> lines = SplitToNonEmptyLines(before);

            tool.ApplySchedule(lines);

            string after = tool.GetSchedule();
            Assert.AreEqual(before, after, "Schedule text should be unchanged after a decode -> apply -> decode round trip.");

            // Stronger check: re-applying the ROM's own decoded schedule should leave every byte in
            // the whole ROM unchanged, not just the schedule text -- this catches a pointer-table
            // write bug that happens to still decode to the same text (e.g. a wrong week-pointer value
            // that GetSchedule() doesn't actually read, since reading sums the games-per-week sentinel
            // array rather than walking the pointer table).
            int differences = 0;
            for (int i = 0; i < originalRom.Length; i++)
            {
                if (originalRom[i] != tool.OutputRom[i])
                    differences++;
            }
            Assert.AreEqual(0, differences, "Re-applying the ROM's own schedule should not change any bytes.");
        }

        [TestMethod]
        public void GenesisRom_ApplyingDifferentSeason3Schedule_DoesNotAffectSeason1Or2()
        {
            byte[] originalRom = TestRoms.LoadRom(RomFileName);
            byte[] romCopy = (byte[])originalRom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(romCopy);

            // Swap week 1's two games relative to the stock schedule -- forces the writer to actually
            // run (not just leave matching data alone), while still producing a structurally valid
            // 18-week/224-game schedule (every other week is re-applied unchanged from the decoded
            // original).
            List<string> lines = SplitToNonEmptyLines(tool.GetSchedule());
            int week1 = lines.IndexOf("WEEK 1");
            string game1 = lines[week1 + 1];
            string game2 = lines[week1 + 2];
            lines[week1 + 1] = game2;
            lines[week1 + 2] = game1;

            tool.ApplySchedule(lines);

            List<string> after = SplitToNonEmptyLines(tool.GetSchedule());
            CollectionAssert.AreEqual(lines, after, "The modified schedule should read back exactly as applied.");

            int differences = 0;
            for (int range = 0; range < Season1And2Ranges.GetLength(0); range++)
            {
                for (int i = Season1And2Ranges[range, 0]; i < Season1And2Ranges[range, 1]; i++)
                {
                    if (originalRom[i] != tool.OutputRom[i])
                        differences++;
                }
            }
            Assert.AreEqual(0, differences, "Editing season 3's schedule must not touch season 1 or season 2's data.");
        }
    }
}
