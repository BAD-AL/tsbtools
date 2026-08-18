using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    

    /// <summary>
    /// Regression tests for TecmoToolFactory.CheckRomType. These guard against a change to the
    /// new CXROM_18WEEK detection (which has to distinguish itself from CXROM_v105 by schedule
    /// content, since both ROM types share the exact same file length) accidentally breaking
    /// detection of the ROM types that were already working.
    /// </summary>
    [TestClass]
    public class RomTypeDetectionTests
    {
        [TestMethod]
        public void StockCXRom_v105_IsDetectedAsCXRom_v105()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-105.nes");
            ROM_TYPE type = TecmoToolFactory.CheckRomType(rom);
            Assert.AreEqual(ROM_TYPE.CXROM_v105, type);
        }

        [TestMethod]
        public void CXRom_v111_IsDetectedAsCXRom_v111()
        {
            byte[] rom = TestRoms.LoadRom("TSB 2007-32-111.nes");
            ROM_TYPE type = TecmoToolFactory.CheckRomType(rom);
            Assert.AreEqual(ROM_TYPE.CXROM_v111, type);
        }

        [TestMethod]
        public void OriginalNesTsbRom_IsDetectedAsNesOriginalTsb()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            ROM_TYPE type = TecmoToolFactory.CheckRomType(rom);
            Assert.AreEqual(ROM_TYPE.NES_ORIGINAL_TSB, type);
        }

        [TestMethod]
        public void EighteenWeekRom_IsDetectedAsCXRom18Week_NotStockV105()
        {
            byte[] rom = TestRoms.LoadRom("SbluemanBase_18week_RealNFL-1.nes");
            ROM_TYPE type = TecmoToolFactory.CheckRomType(rom);
            Assert.AreEqual(
                ROM_TYPE.CXROM_18WEEK,
                type,
                "This ROM has the exact same file length as stock CXROM_v105 (0x80010); " +
                "it must be distinguished by inspecting the schedule's week-pointer table, " +
                "not misdetected as CXROM_v105.");
        }
    }
}
