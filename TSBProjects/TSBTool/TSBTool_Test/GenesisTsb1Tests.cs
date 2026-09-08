using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Phase 0 scaffolding checks for Genesis_TSB1Tool: confirms the factory dispatches a Genesis TSB1
    /// ROM to the right tool class, and that basic plumbing (RomVersion, ProcessText on an empty/no-op
    /// input) doesn't throw. Real behavior gets its own tests as each phase in
    /// compressed-riding-rossum.md lands -- most of Genesis_TSB1Tool is still NotImplementedException
    /// stubs at this point, so this file intentionally stays minimal.
    /// </summary>
    [TestClass]
    public class GenesisTsb1Tests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

        [TestMethod]
        public void GenesisRom_FactoryReturnsGenesisTsb1Tool()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);

            Assert.IsNotNull(tool, "Failed to detect/construct a tool for the Genesis TSB1 ROM.");
            Assert.IsInstanceOfType(tool, typeof(Genesis_TSB1Tool));
            Assert.AreEqual(ROM_TYPE.GENESIS_TSB1, tool.RomVersion);
        }

        [TestMethod]
        public void GenesisRom_ProcessEmptyText_DoesNotThrow()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoContent tool = TecmoToolFactory.GetToolForRom(rom);

            tool.ProcessText(string.Empty);
        }

        [TestMethod]
        public void GenesisRom_SetTeamOffensiveFormation_IsANoOpAndDoesNotThrow()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] romBefore = (byte[])rom.Clone();

            tool.SetTeamOffensiveFormation("bills", "2RB_2WR_1TE");

            CollectionAssert.AreEqual(romBefore, tool.OutputRom, "SetTeamOffensiveFormation must not modify the ROM (no location found -- see Genesis_TSB1_ROM_Findings.md).");
        }

        [TestMethod]
        public void GenesisRom_ApplyJuice_IsANoOpAndDoesNotThrow()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] romBefore = (byte[])rom.Clone();

            bool result = tool.ApplyJuice(3, 10);

            CollectionAssert.AreEqual(romBefore, tool.OutputRom, "ApplyJuice must not modify the ROM (no location found -- see Genesis_TSB1_ROM_Findings.md).");
        }

        [TestMethod]
        public void GenesisRom_ChampColors_AreNoOpsAndDoNotThrow()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            byte[] romBefore = (byte[])rom.Clone();

            tool.SetDivChampColors("bills", "0000000000000000000000000000");
            tool.SetConfChampColors("bills", "0000000000000000000000000000");

            CollectionAssert.AreEqual(romBefore, tool.OutputRom, "Champ color setters must not modify the ROM (matches SNES_TecmoTool's own no-op precedent).");
            Assert.AreEqual(string.Empty, tool.GetDivChampColors("bills"));
            Assert.AreEqual(string.Empty, tool.GetConfChampColors("bills"));
            Assert.AreEqual(string.Empty, tool.GetChampColors("bills"));
        }
    }
}
