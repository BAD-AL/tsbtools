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
    /// check had fitted exactly was rejected "by 1 byte" on lions WR1. Loading a NES/CXROM/SNES ROM now clears
    /// everything past the end pointer to 0xFF. The CXROM expansion area and Genesis measure room from the
    /// pointers alone, so leftovers there never cost room -- tested too, so a later switch to a scan shows up.
    /// </summary>
    [TestClass]
    public class NameAreaLeftoverTests
    {
        private const int NesNameAreaEnd = 0x300F;
        private const int NesSentinel = 0x6d8;
        private const int CxRomSentinel = 0x6e4;
        private const int CxRomExpansionAreaEnd = 0x3FFDF;
        private const int SnesNameAreaEnd = 0x17b7f0;
        private const int SnesSentinel = 0x178738;
        private const int GenesisRosterDataEnd = 0x89FC;
        private const int GenesisSentinel = 0x5F62;
        private const string SnesRom = "TSB1.smc";
        private const string GenesisRom = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static int NesDataEnd(byte[] rom, int sentinelPointerLoc)
        {
            return (rom[sentinelPointerLoc] | (rom[sentinelPointerLoc + 1] << 8)) - 0x8000 + 0x10;
        }

        private static int SnesDataEnd(byte[] rom)
        {
            return (rom[SnesSentinel] | (rom[SnesSentinel + 1] << 8)) + 0x170000;
        }

        private static int GenesisDataEnd(byte[] rom)
        {
            return (rom[GenesisSentinel] << 8) + rom[GenesisSentinel + 1];
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

        private static void PlantLeftover(byte[] rom, int dataEnd)
        {
            Assert.AreEqual(0xFF, rom[dataEnd], "sanity: the ROM is clean after its last name");
            rom[dataEnd] = (byte)'R';
            rom[dataEnd + 1] = (byte)'S';
        }

        private static void AssertAllPadding(byte[] rom, int from, int to)
        {
            for (int i = from; i < to; i++)
                Assert.AreEqual(0xFF, rom[i], string.Format("byte 0x{0:x} (area data ends at 0x{1:x})", i, from));
        }

        // Grows names in team/position order until 'remaining' bytes are used, never past a 17-byte entry
        // (jersey + 16 letters); currentBytes gives an entry's size, insert writes a name. No write may be
        // rejected, and the area must end up exactly full.
        private static void FillExactly(int remaining, string[] teams, string[] positions,
            Func<string, string, int> currentBytes, Action<string, string, string, string> insert)
        {
            List<string> errors = RunAndCollectErrors(delegate
            {
                foreach (string team in teams)
                    foreach (string pos in positions)
                    {
                        if (remaining == 0)
                            return;
                        int oldBytes = currentBytes(team, pos);
                        int growth = Math.Min(17 - oldBytes, remaining);
                        if (growth <= 0)
                            continue;
                        insert(team, pos, "a", new string('B', oldBytes + growth - 2));
                        remaining -= growth;
                    }
            });
            Assert.AreEqual(0, remaining, "sanity: enough players to fill the area");
            CollectionAssert.AreEqual(new List<string>(), errors);
        }

        // ---- the stock ROMs have nothing but padding past the end pointer, so clearing it loses nothing ----

        [TestMethod]
        public void StockRoms_OnlyPaddingPastTheEndPointer()
        {
            byte[] nes = TestRoms.LoadRom("TSPRBOWL.nes");
            AssertAllPadding(nes, NesDataEnd(nes, NesSentinel), NesNameAreaEnd);
            byte[] cx = TestRoms.LoadRom("TSB 2007-32-105.nes");
            AssertAllPadding(cx, NesDataEnd(cx, CxRomSentinel), NesNameAreaEnd);
            byte[] snes = TestRoms.LoadRom(SnesRom);
            AssertAllPadding(snes, SnesDataEnd(snes), SnesNameAreaEnd);
        }

        // ---- NES / CXROM main area ----

        [TestMethod]
        public void NesStock_LeftoverAfterLastName_IsClearedOnLoad()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            int dataEnd = NesDataEnd(rom, NesSentinel);
            PlantLeftover(rom, dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            AssertAllPadding(tool.OutputRom, dataEnd, NesNameAreaEnd);
            Assert.AreEqual("scott FULHAGE", tool.GetName("falcons", "P"), "the last name itself is kept");
        }

        [TestMethod]
        public void CxRom_LeftoverAfterLastName_IsClearedOnLoad()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            int dataEnd = NesDataEnd(rom, CxRomSentinel);
            PlantLeftover(rom, dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            AssertAllPadding(tool.OutputRom, dataEnd, NesNameAreaEnd);
            Assert.AreEqual("jason BAKER", tool.GetName("panthers", "P"), "the last name itself is kept");
        }

        // With the leftover gone, every free byte (counted from the names, as PlayerNames does) can be used.
        [TestMethod]
        public void CxRom_LeftoverRom_EveryFreeByteCanBeUsed()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            int dataEnd = NesDataEnd(rom, CxRomSentinel);
            PlantLeftover(rom, dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);

            FillExactly(NesNameAreaEnd - dataEnd, TecmoTool.Teams.GetRange(0, 28).ToArray(), PlayerNamesConfig.Nes.Positions,
                (t, p) => tool.GetName(t, p).Length, // "f L" == jersey + f + L bytes
                (t, p, f, l) => tool.InsertPlayer(t, p, f, l, 1));
            Assert.AreEqual(NesNameAreaEnd, NesDataEnd(tool.OutputRom, CxRomSentinel), "area exactly full");
        }

        // The expansion teams' room is measured from the cardinals' punter, so a leftover after it never costs room.
        [TestMethod]
        public void CxRomExpansion_LeftoverAfterLastName_EveryFreeByteCanBeUsed()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            TecmoTool probe = (TecmoTool)TecmoToolFactory.GetToolForRom((byte[])rom.Clone());
            int dataEnd = probe.GetDataPosition("cardinals", "P") + probe.GetName("cardinals", "P").Length;
            PlantLeftover(rom, dataEnd);
            TecmoTool tool = (TecmoTool)TecmoToolFactory.GetToolForRom(rom);

            FillExactly(CxRomExpansionAreaEnd - dataEnd, new[] { "49ers", "rams", "seahawks", "cardinals" }, PlayerNamesConfig.Nes.Positions,
                (t, p) => tool.GetName(t, p).Length,
                (t, p, f, l) => tool.InsertPlayer(t, p, f, l, 1));
            Assert.AreEqual(CxRomExpansionAreaEnd, tool.GetDataPosition("cardinals", "P") + tool.GetName("cardinals", "P").Length, "area exactly full");
        }

        // ---- SNES TSB1 ----

        [TestMethod]
        public void Snes_LeftoverAfterLastName_IsClearedOnLoad()
        {
            byte[] rom = TestRoms.LoadRom(SnesRom);
            int dataEnd = SnesDataEnd(rom);
            PlantLeftover(rom, dataEnd);
            SNES_TecmoTool probe = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(SnesRom));
            string lastName = probe.GetName("49ers", "DB2");

            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(rom);
            AssertAllPadding(tool.OutputRom, dataEnd, SnesNameAreaEnd);
            Assert.AreEqual(lastName, tool.GetName("49ers", "DB2"), "the last name itself is kept");
        }

        [TestMethod]
        public void Snes_LeftoverRom_EveryFreeByteCanBeUsed()
        {
            byte[] rom = TestRoms.LoadRom(SnesRom);
            int dataEnd = SnesDataEnd(rom);
            PlantLeftover(rom, dataEnd);
            SNES_TecmoTool tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(rom);

            FillExactly(SnesNameAreaEnd - dataEnd, TecmoTool.Teams.ToArray(), PlayerNamesConfig.Snes.Positions,
                (t, p) => tool.GetName(t, p).Length,
                (t, p, f, l) => tool.InsertPlayer(t, p, f, l, 1));
            Assert.AreEqual(SnesNameAreaEnd, SnesDataEnd(tool.OutputRom), "area exactly full");
        }

        // ---- Genesis TSB1: room comes from the sentinel alone ----

        [TestMethod]
        public void Genesis_LeftoverAfterLastName_EveryFreeByteCanBeUsed()
        {
            // The stock roster is packed (no room), so shorten a few names first to make some.
            Genesis_TSB1Tool shrink = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(GenesisRom));
            foreach (string pos in new[] { "QB1", "RB1", "WR1", "TE1" })
                shrink.InsertPlayer("bills", pos, "a", "B", 1);
            byte[] rom = (byte[])shrink.OutputRom.Clone();
            int dataEnd = GenesisDataEnd(rom);
            Assert.IsTrue(GenesisRosterDataEnd - dataEnd > 10, "sanity: shortening made room");
            // Genesis shortening already leaves the old tail behind the data (no 0xFF fill), so no planting needed.
            Assert.AreNotEqual(0xFF, rom[dataEnd], "sanity: leftover bytes past the end");
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            FillExactly(GenesisRosterDataEnd - dataEnd, new[] { "colts", "dolphins", "patriots" }, PlayerNamesConfig.Snes.Positions,
                (t, p) => tool.GetPlayerName(t, p).Length + 1, // stored "fL", plus the jersey byte
                (t, p, f, l) => tool.InsertPlayer(t, p, f, l, 1));
            Assert.AreEqual(GenesisRosterDataEnd, GenesisDataEnd(tool.OutputRom), "area exactly full");
        }

        // ---- the real case ----

        // This ROM and roster (CLI flow: PlanFit, Apply, ProcessText) gave
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
