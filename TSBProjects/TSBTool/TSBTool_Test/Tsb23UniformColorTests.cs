using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// SNES TSB2/TSB3 uniform colors (TSBTool_Test\TSB2_TSB3_Uniform_Findings.md). Per team 0x40 bytes = two 16-color
    /// palettes (light-skin, dark-skin; they differ only in the skin colors 7 and 8). Home TSB3 0x19BD60 / TSB2 0x19BE60,
    /// away TSB3 0x19C4E0 / TSB2 0x19C560. The text carries 12 colors (2 raw ROM bytes each) in the order palette
    /// indexes 1-6 (helmet, jersey, pants, dark then light) and 9-14 (socks, pants stripe, helmet stripe).
    /// </summary>
    [TestClass]
    public class Tsb23UniformColorTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        // Decoded from the stock ROMs (TSB2 and TSB3 hold the same Bills colors).
        private const string BillsHome = "14001e0081348865ed4d99777a73ce4116001e0080382365";
        private const string BillsAway = "14001e00ed4dbc7bed4d99777a73ce4116001e0080382365";

        private static TSB2Tool Load(string rom)
        {
            return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom)) as TSB2Tool;
        }

        private static int Start(TSB2Tool tool, bool away)
        {
            bool tsb3 = tool is TSB3Tool;
            return (away ? (tsb3 ? 0x19C4E0 : 0x19C560) : (tsb3 ? 0x19BD60 : 0x19BE60));
        }

        private static string Hex(byte[] rom, int location, int count)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++) sb.Append(rom[location + i].ToString("x2"));
            return sb.ToString();
        }

        [TestMethod]
        public void GetUniform_MatchesStockBillsColors()
        {
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
            TSB2Tool tool = Load(rom);
            Assert.AreEqual("Uniform1=0x" + BillsHome, tool.GetHomeUniform("bills"));
            Assert.AreEqual("Uniform2=0x" + BillsAway, tool.GetAwayUniform("bills"));
            }
        }

        [TestMethod]
        public void SetHomeAndAway_WritesBothSkinPalettes_AndNothingElse()
        {
            foreach (var c in new[] { new { rom = "TSB2_U.smc", team = "bills", index = 0 }, new { rom = "TSB2_U.smc", team = "49ers", index = 27 },
                new { rom = "TSB3.smc", team = "bills", index = 0 }, new { rom = "TSB3.smc", team = "49ers", index = 29 } })
            {
            string rom = c.rom, team = c.team; int index = c.index;
            TSB2Tool tool = Load(rom);
            byte[] before = (byte[])tool.OutputRom.Clone();
            string home = "0102030405060708090a0b0c0d0e0f101112131415161718";
            string away = "e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f8";

            tool.SetHomeUniform(team, home);
            tool.SetAwayUniform(team, away);

            Assert.AreEqual("Uniform1=0x" + home, tool.GetHomeUniform(team));
            Assert.AreEqual("Uniform2=0x" + away, tool.GetAwayUniform(team));
            int[] indexes = { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14 };
            var expected = (byte[])before.Clone();
            for (int b = 0; b < 2; b++)
                for (int a = 0; a < 2; a++)
                {
                    string src = a == 0 ? home : away;
                    int pal = Start(tool, a == 1) + index * 0x40 + b * 0x20;
                    for (int k = 0; k < 12; k++)
                    {
                        expected[pal + indexes[k] * 2] = System.Convert.ToByte(src.Substring(k * 4, 2), 16);
                        expected[pal + indexes[k] * 2 + 1] = System.Convert.ToByte(src.Substring(k * 4 + 2, 2), 16);
                    }
                }
            CollectionAssert.AreEqual(expected, tool.OutputRom);
            }
        }

        [TestMethod]
        public void SetUniform_LeavesSkinAndShadowColorsAlone()
        {
            TSB2Tool tool = Load("TSB3.smc");
            string skinBefore = Hex(tool.OutputRom, 0x19BD60 + 14, 4) + Hex(tool.OutputRom, 0x19BD60 + 0x20 + 14, 4)
                + Hex(tool.OutputRom, 0x19BD60 + 30, 2);
            tool.SetHomeUniform("bills", new string('f', 48));
            string skinAfter = Hex(tool.OutputRom, 0x19BD60 + 14, 4) + Hex(tool.OutputRom, 0x19BD60 + 0x20 + 14, 4)
                + Hex(tool.OutputRom, 0x19BD60 + 30, 2);
            Assert.AreEqual(skinBefore, skinAfter);
        }

        [TestMethod]
        public void SetUniform_ColorsThatDidNotChange_KeepTheirStockDarkSkinValue()
        {
            // The stock Bills away palettes differ in the jersey colors (3, 4): the dark-skin palette is a little darker.
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
                TSB2Tool tool = Load(rom);
                int pal = Start(tool, true);
                string darkJerseyBefore = Hex(tool.OutputRom, pal + 0x20 + 6, 4);
                Assert.AreNotEqual(Hex(tool.OutputRom, pal + 6, 4), darkJerseyBefore);

                tool.SetAwayUniform("bills", "ffff" + BillsAway.Substring(4));     // helmet dark only

                Assert.AreEqual("ffff", Hex(tool.OutputRom, pal + 2, 2));
                Assert.AreEqual("ffff", Hex(tool.OutputRom, pal + 0x20 + 2, 2));
                Assert.AreEqual(darkJerseyBefore, Hex(tool.OutputRom, pal + 0x20 + 6, 4), rom);
            }
        }

        [TestMethod]
        public void InvalidColorString_IsRejected_RomUnchanged()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            byte[] before = (byte[])tool.OutputRom.Clone();
            tool.SetHomeUniform("bills", "1234");
            tool.SetAwayUniform("nosuchteam", BillsHome);
            CollectionAssert.AreEqual(before, tool.OutputRom);
        }

        [TestMethod]
        public void Dump_HasColorsLineAfterTeamNames_AndEditsFromTextApply()
        {
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
            TSB2Tool tool = Load(rom);
            string all = tool.GetAll(1);
            string line = "COLORS Uniform1=0x" + BillsHome + ", Uniform2=0x" + BillsAway;
            StringAssert.Contains(all, "TEAM = bills");
            int t = all.IndexOf("TEAM = bills");
            int c = all.IndexOf(line, t);
            Assert.IsTrue(c > t && c < all.IndexOf("\nQB1,", t), "COLORS line must be in the bills block before its players");

            string newHome = "0a0b0c0d0e0f1011121314151617181920212223";
            newHome = newHome + "24252627"; // 48 digits
            string text = "SEASON 1\nTEAM = bills\nCOLORS Uniform1=0x" + newHome + ", Uniform2=0x" + BillsAway + "\n";
            tool.ProcessText(text);
            Assert.AreEqual("Uniform1=0x" + newHome, tool.GetHomeUniform("bills"));
            Assert.AreEqual("Uniform2=0x" + BillsAway, tool.GetAwayUniform("bills"));
            }
        }

        [TestMethod]
        public void ColorsLine_ForUnknownTeam_DoesNotTouchThePreviousTeam()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            byte[] before = (byte[])tool.OutputRom.Clone();
            tool.ProcessText("SEASON 1\nTEAM = bills\nTEAM = jaguars\nCOLORS Uniform1=0x" + new string('a', 48) + "\n");
            CollectionAssert.AreEqual(before, tool.OutputRom);
        }

        [TestMethod]
        public void FullDump_AppliedUnchanged_LeavesTheUniformTablesAlone()
        {
            // (other parts of a dump, e.g. names, can differ after a round trip; that is not about colors)
            foreach (string rom in new[] { "TSB2_U.smc", "TSB3.smc" })
            {
                TSB2Tool tool = Load(rom);
                byte[] before = (byte[])tool.OutputRom.Clone();
                tool.ProcessText(tool.GetAll(1));
                int from = Start(tool, false), to = Start(tool, true) + (tool is TSB3Tool ? 30 : 28) * 0x40;
                for (int i = from; i < to; i++)
                    Assert.AreEqual(before[i], tool.OutputRom[i], string.Format("{0} byte 0x{1:X}", rom, i));
            }
        }
    }
}
