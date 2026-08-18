using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Verifies the InputParser "ReplaceString("find", "replace"[, occurrence])" command
    /// (StaticUtils.ReplaceStringInRom), added 2019-08-13 and never covered by any prior test.
    ///
    /// Unlike SET, ReplaceStringInRom is a single static function that operates directly on
    /// tool.OutputRom (a raw byte[]) with no per-ROM-type branching anywhere in the call path, so
    /// one representative ROM (TSPRBOWL.nes) is enough -- there's no "does it work differently for
    /// TSB2Tool vs TecmoTool" question the way there was for SET's per-class SetByte.
    ///
    /// Each test plants a known marker string into a working copy of the ROM at a scratch offset
    /// (well past any header, comfortably inside every fixture ROM) before applying ReplaceString,
    /// so the search string's location and pre-image are always known exactly, and diffs the byte
    /// array before/after just like SetCommandTests does, for the same reason: it proves the effect
    /// is exactly what's expected and nothing else moved.
    /// </summary>
    [TestClass]
    public class ReplaceStringCommandTests
    {
        private const int MarkerLocation = 0x2000;
        private const int SecondMarkerLocation = 0x3000;

        private static void PlantAsciiString(byte[] rom, int location, string text)
        {
            for (int i = 0; i < text.Length; i++)
                rom[location + i] = (byte)text[i];
        }

        private static string ReadAsciiString(byte[] rom, int location, int length)
        {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)rom[location + i];
            return new string(chars);
        }

        private static int[] DiffIndices(byte[] before, byte[] after)
        {
            return Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToArray();
        }

        [TestMethod]
        public void ReplaceString_SameLength_ReplacesInPlaceWithNoOtherChanges()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            PlantAsciiString(rom, MarkerLocation, "ORIGINAL");
            byte[] beforeReplace = (byte[])rom.Clone();

            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            tool.ProcessText("ReplaceString(\"ORIGINAL\", \"REPLACED\")");

            Assert.AreEqual("REPLACED", ReadAsciiString(tool.OutputRom, MarkerLocation, 8));

            int[] diffs = DiffIndices(beforeReplace, tool.OutputRom);
            CollectionAssert.AreEqual(
                Enumerable.Range(MarkerLocation, 8).ToArray(),
                diffs,
                "Expected only the 8 bytes of the marker string to change.");
        }

        [TestMethod]
        public void ReplaceString_ShorterReplacement_PadsWithTrailingSpaces()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            PlantAsciiString(rom, MarkerLocation, "ORIGINAL");

            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            tool.ProcessText("ReplaceString(\"ORIGINAL\", \"NEW\")");

            // "NEW" padded with spaces to match "ORIGINAL"'s 8-character width.
            Assert.AreEqual("NEW     ", ReadAsciiString(tool.OutputRom, MarkerLocation, 8));
        }

        [TestMethod]
        public void ReplaceString_LongerReplacement_IsRejectedAndDoesNotMutateTheRom()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            PlantAsciiString(rom, MarkerLocation, "ABC");
            byte[] beforeReplace = (byte[])rom.Clone();

            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            tool.ProcessText("ReplaceString(\"ABC\", \"ABCDE\")");

            CollectionAssert.AreEqual(
                beforeReplace,
                tool.OutputRom,
                "A replacement string longer than the search string must be rejected with no ROM changes.");
        }

        [TestMethod]
        public void ReplaceString_WithOccurrenceIndex_ReplacesOnlyThatOccurrence()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            PlantAsciiString(rom, MarkerLocation, "MARKER");
            PlantAsciiString(rom, SecondMarkerLocation, "MARKER");

            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);
            // 1-based occurrence: the marker at the lower address is found first (occurrence 1),
            // so occurrence 2 is the one at SecondMarkerLocation. "REPL" (4 chars) pads to "MARKER"'s
            // 6-char width, since a replacement longer than the search string is rejected outright.
            tool.ProcessText("ReplaceString(\"MARKER\", \"REPL\", 2)");

            Assert.AreEqual("MARKER", ReadAsciiString(tool.OutputRom, MarkerLocation, 6),
                "The first occurrence should be untouched.");
            Assert.AreEqual("REPL  ", ReadAsciiString(tool.OutputRom, SecondMarkerLocation, 6),
                "The second occurrence should be replaced and space-padded to the original 6-char width.");
        }
    }
}
