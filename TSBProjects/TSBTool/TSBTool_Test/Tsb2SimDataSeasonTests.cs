using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB2 keeps one sim data block per season (1992/1993/1994). Each season's pointer table
    /// (0x1EE000 / 0x1F6000 / 0x1FE000) holds 16-bit pointers into its own LoROM bank, so the
    /// Bills' blocks are at 0x1EE038 / 0x1F6038 / 0x1FE038. TSB2Tool used to add 0x1E0000 for every
    /// season, so seasons 2 and 3 showed and edited season 1's block.
    /// </summary>
    [TestClass]
    public class Tsb2SimDataSeasonTests
    {
        static readonly int[] BillsSimBlock = { 0x1EE038, 0x1F6038, 0x1FE038 };

        static TSB2Tool LoadTool()
        {
            TSB2Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")) as TSB2Tool;
            Assert.IsNotNull(tool, "TSB2_U.smc should load as a TSB2Tool.");
            return tool;
        }

        [TestMethod]
        public void GetTeamSimData_EachSeason_ReadsItsOwnBlock()
        {
            TSB2Tool tool = LoadTool();
            Assert.AreEqual("SimData=0xD8", tool.GetTeamSimData(1, "bills"));
            Assert.AreEqual("SimData=0x67", tool.GetTeamSimData(2, "bills"));
            Assert.AreEqual("SimData=0x66", tool.GetTeamSimData(3, "bills"));
        }

        [TestMethod]
        public void GetPlayerAbilities_Season3_ShowsSeason3SimBytes()
        {
            TSB2Tool tool = LoadTool();
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "QB1"), "[08,01,C3]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(3, "bills", "QB1"), "[08,01,B3]");
        }

        [TestMethod]
        public void SetTeamSimData_Season3_WritesSeason3BlockOnly()
        {
            TSB2Tool tool = LoadTool();
            tool.SetTeamSimData(3, "bills", "0x12");
            Assert.AreEqual(0x12, tool.OutputRom[BillsSimBlock[2] + 101]);
            Assert.AreEqual(0xD8, tool.OutputRom[BillsSimBlock[0] + 101], "season 1 must not change");
            Assert.AreEqual(0x67, tool.OutputRom[BillsSimBlock[1] + 101], "season 2 must not change");
        }

        [TestMethod]
        public void SetDefensiveSimData_Season2_WritesSeason2BlockOnly()
        {
            TSB2Tool tool = LoadTool();
            tool.SetDefensiveSimData(2, "bills", "RE", new int[] { 1, 2, 3 });
            int re = 0x2E;
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 },
                new[] { tool.OutputRom[BillsSimBlock[1] + re], tool.OutputRom[BillsSimBlock[1] + re + 1], tool.OutputRom[BillsSimBlock[1] + re + 2] });
            CollectionAssert.AreEqual(new byte[] { 0x51, 0x00, 0x06 },
                new[] { tool.OutputRom[BillsSimBlock[0] + re], tool.OutputRom[BillsSimBlock[0] + re + 1], tool.OutputRom[BillsSimBlock[0] + re + 2] },
                "season 1 must not change");
        }

        // The defenders' sim bytes are in roster order (RE NT LE RE2 NT2 LE2 ROLB RILB LILB LOLB LB5 ...),
        // like the names, attributes and the season stats in the save (checked with a simmed season: the
        // ROLB slot's tackle byte 7 / sack share 0x20 -> c. Bennett 115 tackles, 9 sacks; the LOLB slot's
        // tackle byte 9 / INT share 0x30 -> d. Talley 145 tackles, 7 INTs). TSBTool used to read
        // LOLB/LILB/RILB/ROLB there, swapping the outside and the inside linebackers.
        [TestMethod]
        public void GetPlayerAbilities_Linebackers_UseTheirOwnSimBytes()
        {
            TSB2Tool tool = LoadTool();
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "ROLB"), "[14,00,07]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "RILB"), "[06,00,06]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "LILB"), "[0C,0B,06]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "LOLB"), "[17,00,09]");
        }

        [TestMethod]
        public void SetDefensiveSimData_Rolb_WritesTheRolbSlot()
        {
            TSB2Tool tool = LoadTool();
            tool.SetDefensiveSimData(1, "bills", "ROLB", new int[] { 1, 2, 3 });
            int rolb = 0x2E + 6 * 3; // 7th defender
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 },
                new[] { tool.OutputRom[BillsSimBlock[0] + rolb], tool.OutputRom[BillsSimBlock[0] + rolb + 1], tool.OutputRom[BillsSimBlock[0] + rolb + 2] });
        }

        [TestMethod]
        public void Tsb3_Linebackers_UseTheirOwnSimBytes()
        {
            // TSB3 shares GetSimLocation. Its stock ROM shows the same order: with the LB pairs read in
            // roster order the tackle byte follows QU (correlation 0.65 over 128 LBs, 0.04 the old way).
            TSB3Tool tool = TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc")) as TSB3Tool;
            Assert.IsNotNull(tool);
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "ROLB"), "[40,08,07]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "LOLB"), "[20,08,09]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "RILB"), "[07,00,04]");
            StringAssert.EndsWith(tool.GetPlayerAbilities(1, "bills", "LILB"), "[07,00,03]");
        }
    }
}
