using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Verifies the InputParser "SET(0xADDR, 0xVAL)" raw-byte-poke command, across every ROM type
    /// this tool supports. Unlike the text round-trip tests elsewhere in this project, this compares
    /// raw ROM bytes directly: apply a single SET command in-memory (via ITecmoContent.ProcessText,
    /// no file I/O needed since SetByte writes straight into the in-memory OutputRom array) and
    /// assert the resulting ROM differs from the original in exactly one byte -- the one SET named,
    /// holding exactly the value SET specified. That's a much sharper check than comparing formatted
    /// text: it directly proves SET has no side effects anywhere else in the ROM.
    ///
    /// TSB2/TSB3 (TSB2Tool/TSB3Tool) implement ITecmoContent independently rather than extending
    /// TecmoTool, but parse the identical "SET(...)" regex and route through the same
    /// StaticUtils.ApplySimpleSet -> SetByte path, so they're included here as a side effect of
    /// testing SET even though their own round-trip coverage is still a separate, larger task
    /// (see TestDocument.md).
    /// </summary>
    [TestClass]
    public class SetCommandTests
    {
        // Comfortably inside every fixture ROM (smallest is TSPRBOWL.nes at 393,232 bytes) and well
        // past any header region.
        private const int TestLocation = 0x2000;

        [TestMethod]
        public void StockNesRom_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("TSPRBOWL.nes");
        }

        [TestMethod]
        public void SnesTsb1Rom_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("TSB1.smc");
        }

        [TestMethod]
        public void CXRomV105_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("TSB 2007-32-105.nes");
        }

        [TestMethod]
        public void CXRomV111_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("TSB 2007-32-111.nes");
        }

        [TestMethod]
        public void CXRom18Week_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("SbluemanBase_18week_RealNFL-1.nes");
        }

        [TestMethod]
        public void SnesTsb2Rom_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("TSB2_U.smc");
        }

        [TestMethod]
        public void SnesTsb3Rom_SetCommand_ChangesExactlyOneByte()
        {
            VerifySetChangesExactlyOneByte("TSB3.smc");
        }

        private static void VerifySetChangesExactlyOneByte(string romFileName)
        {
            byte[] romBytes = TestRoms.LoadRom(romFileName);
            byte[] originalBytes = (byte[])romBytes.Clone();

            byte originalValue = originalBytes[TestLocation];
            byte targetValue = (byte)(originalValue ^ 0xFF); // guaranteed different from originalValue

            ITecmoContent tool = TecmoToolFactory.GetToolForRom(romBytes);
            Assert.IsNotNull(tool, "Failed to detect ROM type for " + romFileName);

            string setCommand = string.Format("SET(0x{0:x}, 0x{1:x2})", TestLocation, targetValue);
            tool.ProcessText(setCommand);

            byte[] resultBytes = tool.OutputRom;
            Assert.AreEqual(originalBytes.Length, resultBytes.Length, "SET must not change the ROM's size.");

            List<int> differingIndices = new List<int>();
            for (int i = 0; i < originalBytes.Length; i++)
            {
                if (originalBytes[i] != resultBytes[i])
                    differingIndices.Add(i);
            }

            string diffSummary = string.Join(", ", differingIndices.Take(10).Select(
                i => string.Format("0x{0:x}: 0x{1:x2}->0x{2:x2}", i, originalBytes[i], resultBytes[i])).ToArray());

            Assert.AreEqual(
                1,
                differingIndices.Count,
                string.Format("Expected SET to change exactly 1 byte, but {0} byte(s) differed: {1}", differingIndices.Count, diffSummary));
            Assert.AreEqual(TestLocation, differingIndices[0], "The one changed byte should be at the location SET specified.");
            Assert.AreEqual(targetValue, resultBytes[TestLocation], "The changed byte should hold the value SET specified.");
        }
    }
}
