using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Genesis_TSB1Tool.SetYear/GetYear -- unlike NES/SNES (a single season -> one "current year"
    /// concept patched everywhere), this ROM shows all 3 baked-in season years side by side in its
    /// menus. Per the project owner's explicit direction, SetYear is a deliberately simple blanket
    /// find-and-replace of every real on-screen "1993" occurrence with the given year -- it does not
    /// try to keep the 3-year menus semantically consistent, and 1991/1992 are left untouched. Found by
    /// scanning the ROM for every occurrence of ASCII "1993" (10 hits) and checking context on each --
    /// see Genesis_TSB1Tool.cs's own comment above yearTextLocations for the full list and reasoning.
    /// </summary>
    [TestClass]
    public class GenesisYearTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        // The 9 real on-screen "1993" locations SetYear is expected to patch -- title screen, two
        // copyright/legal lines, and four season-select menu occurrences.
        private static readonly int[] ExpectedYearLocations =
        {
            0x1CD0F, 0x1CD77, 0x1D08E, 0x1D1FA, 0x1D212, 0x1D47E, 0x1D5FD, 0x4B6AA, 0x4C164
        };

        // The ROM header's own "(C)T-36 1993.OCT" copyright-date field -- cartridge metadata, not
        // rendered game text, deliberately never touched.
        private const int RomHeaderYearLoc = 0x118;

        [TestMethod]
        public void GenesisRom_SetYear_WritesAllNineOnScreenLocations()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetYear("2026");

            foreach (int location in ExpectedYearLocations)
            {
                string atLocation = "" + (char)tool.OutputRom[location] + (char)tool.OutputRom[location + 1]
                    + (char)tool.OutputRom[location + 2] + (char)tool.OutputRom[location + 3];
                Assert.AreEqual("2026", atLocation, string.Format("Expected '2026' at 0x{0:X}.", location));
            }
        }

        [TestMethod]
        public void GenesisRom_SetYear_DoesNotTouchTheRomHeader()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            byte[] headerBefore = new byte[4];
            System.Array.Copy(rom, RomHeaderYearLoc, headerBefore, 0, 4);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetYear("2026");

            byte[] headerAfter = new byte[4];
            System.Array.Copy(tool.OutputRom, RomHeaderYearLoc, headerAfter, 0, 4);
            CollectionAssert.AreEqual(headerBefore, headerAfter, "The ROM header's copyright-date field must never be touched.");
        }

        [TestMethod]
        public void GenesisRom_SetYear_LeavesNineteenNinetyOneAndNinetyTwoLabelsAlone()
        {
            // Every one of the 9 patched locations sits inside a "....1993....1992....1991...." style
            // list (or similar) -- confirm the neighboring year labels survive untouched, proving the
            // write is a narrow 4-byte patch, not something that clobbers surrounding menu text.
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetYear("2026");

            StringAssert.Contains(DumpAscii(tool.OutputRom, 0x1D060, 100), "1992");
            StringAssert.Contains(DumpAscii(tool.OutputRom, 0x1D060, 100), "1991");
            StringAssert.Contains(DumpAscii(tool.OutputRom, 0x1D5D0, 100), "1992 SEASON");
        }

        [TestMethod]
        public void GenesisRom_GetYear_ReflectsWhatWasJustSet()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetYear("2099");

            Assert.AreEqual("2099", tool.GetYear());
        }

        [TestMethod]
        public void GenesisRom_SetYear_RejectsWrongLength()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            byte[] romBefore = (byte[])rom.Clone();
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.SetYear("99");

                CollectionAssert.AreEqual(romBefore, tool.OutputRom, "A wrong-length year must be rejected entirely.");
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
                StaticUtils.ClearErrors();
            }
        }

        [TestMethod]
        public void GenesisRom_SetYear_SurvivesReload()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.SetYear("2026");
            Genesis_TSB1Tool reloaded = new Genesis_TSB1Tool((byte[])tool.OutputRom.Clone());

            Assert.AreEqual("2026", reloaded.GetYear());
        }

        private static string DumpAscii(byte[] rom, int start, int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                byte b = rom[start + i];
                chars[i] = (b >= 0x20 && b <= 0x7E) ? (char)b : '.';
            }
            return new string(chars);
        }
    }
}
