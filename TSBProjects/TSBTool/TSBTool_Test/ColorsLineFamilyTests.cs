using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// A COLORS line is in the format of one kind of ROM: NES/CXROM (6-digit Uniform1/Uniform2, DivChamp, ConfChamp, UniformUsage),
    /// SNES TSB1 (28-digit Uniform1/Uniform2, UniformUsage), Genesis (28-digit, plus InGame), TSB2/TSB3 (48-digit; their own parser).
    /// The TSB1-family parser (Core\InputParser.cs) used to match the first 28 digits of a 48-digit value, and the NES setter took the
    /// first 3 bytes of anything, so text from another kind of ROM was written into the ROM without a word. Now such a line is skipped
    /// whole, with one error, and a TSB2/TSB3-to-TSB1 conversion comments the lines out rather than carrying them over.
    /// </summary>
    [TestClass]
    public class ColorsLineFamilyTests
    {
        const string Nes = "COLORS Uniform1=0x250611, Uniform2=0x150621, DivChamp=0x0111012515, ConfChamp=0x11211125, UniformUsage=0x00112233";
        const string Snes1 = "COLORS Uniform1=0x08218050945243759c730c009c10, Uniform2=0x0821524a9452ff7f9c730c009c10, UniformUsage=0x7ffef5f0";
        const string Tsb2 = "COLORS Uniform1=0x14001e0081348865ed4d99777a73ce4116001e0080382365, Uniform2=0x14001e00ed4dbc7bed4d99777a73ce4116001e0080382365";

        TestMessageGiver giver;

        [TestInitialize]
        public void Before()
        {
            StaticUtils.ClearErrors();
            giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
        }

        [TestCleanup]
        public void After() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        static ITecmoContent Tool(string rom) { return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom)); }

        /// <summary>The ROM after applying text with the COLORS line, and the same ROM after applying it with the line left out.</summary>
        void Apply(string rom, string colorsLine, out byte[] withLine, out byte[] without)
        {
            ITecmoContent a = Tool(rom);
            a.ProcessText("TEAM = bills\n" + colorsLine + "\n");
            StaticUtils.ShowErrors();
            withLine = (byte[])a.OutputRom.Clone();
            ITecmoContent b = Tool(rom);
            b.ProcessText("TEAM = bills\n");
            without = (byte[])b.OutputRom.Clone();
        }

        /// <summary>The lines of the error box(es) about COLORS or uniforms (a skipped line must give one, not one per color).</summary>
        string[] ColorsErrors()
        {
            List<string> lines = new List<string>();
            foreach (string box in giver.Errors)
                foreach (string l in box.Replace("\r\n", "\n").Split('\n'))
                    if (l.Contains("COLORS") || l.Contains("niform")) lines.Add(l);
            return lines.ToArray();
        }

        void AssertSkippedWithOneError(string rom, string colorsLine)
        {
            byte[] withLine, without;
            Apply(rom, colorsLine, out withLine, out without);
            int n = 0;
            for (int i = 0; i < withLine.Length; i++) if (withLine[i] != without[i]) n++;
            Assert.AreEqual(0, n, rom + ": " + n + " ROM bytes were written from a COLORS line of another kind of ROM");
            string[] about = ColorsErrors();
            Assert.AreEqual(1, about.Length, rom + ": " + string.Join(" | ", giver.Errors.ToArray()));
            StringAssert.Contains(about[0], "bills");
        }

        [TestMethod]
        public void Snes1Rom_Tsb2Line_IsSkippedWithOneError() { AssertSkippedWithOneError("TSB1.smc", Tsb2); }

        [TestMethod]
        public void Snes1Rom_NesLine_IsSkippedWithOneError() { AssertSkippedWithOneError("TSB1.smc", Nes); }

        [TestMethod]
        public void NesRom_Snes1Line_IsSkippedWithOneError() { AssertSkippedWithOneError("TSPRBOWL.nes", Snes1); }

        [TestMethod]
        public void NesRom_Tsb2Line_IsSkippedWithOneError() { AssertSkippedWithOneError("TSPRBOWL.nes", Tsb2); }

        [TestMethod]
        public void CxRom_Snes1Line_IsSkippedWithOneError() { AssertSkippedWithOneError("TSB 2007-32-111.nes", Snes1); }

        [TestMethod]
        public void GenesisRom_NesLine_IsSkippedWithOneError() { AssertSkippedWithOneError("Genesis/Tecmo Super Bowl (USA) (October 1993).bin", Nes); }

        [TestMethod]
        public void Snes1Rom_OwnLine_StillApplies()
        {
            byte[] withLine, without;
            Apply("TSB1.smc", Snes1, out withLine, out without);
            Assert.AreEqual(0, ColorsErrors().Length, string.Join(" | ", giver.Errors.ToArray()));
            ITecmoContent t = Tool("TSB1.smc");
            t.ProcessText("TEAM = bills\n" + Snes1 + "\n");
            Assert.AreEqual("Uniform1=0x08218050945243759c730c009c10", ((SNES_TecmoTool)t).GetHomeUniform("bills"));
            Assert.AreEqual("UniformUsage=0x7ffef5f0", ((SNES_TecmoTool)t).GetUniformUsage("bills"));
        }

        [TestMethod]
        public void NesRom_OwnLine_StillApplies()
        {
            ITecmoContent t = Tool("TSPRBOWL.nes");
            t.ProcessText("TEAM = bills\n" + Nes + "\n");
            StaticUtils.ShowErrors();
            Assert.AreEqual(0, ColorsErrors().Length, string.Join(" | ", giver.Errors.ToArray()));
            Assert.AreEqual("Uniform1=0x250611", ((TecmoTool)t).GetHomeUniform("bills"));
            Assert.AreEqual("UniformUsage=0x00112233", ((TecmoTool)t).GetUniformUsage("bills"));
        }

        // ---- the converter ----

        static string Colors(string text)
        {
            List<string> lines = new List<string>();
            foreach (string l in text.Replace("\r\n", "\n").Split('\n'))
                if (l.TrimStart('#').StartsWith("COLORS")) lines.Add(l);
            return string.Join("\n", lines.ToArray());
        }

        [TestMethod]
        public void Tsb2ToTsb1_CommentsOutTheTsb2ColorsLines()
        {
            string text = "TEAM = bills SimData=0x1d\n" + Tsb2 + "\n";
            string tsb1 = TSB1Converter.ConvertToTSB1FromTSB2(text);
            Assert.AreEqual("#" + Tsb2, Colors(tsb1));
        }

        [TestMethod]
        public void Tsb3ToTsb1_CommentsOutTheTsb3ColorsLines()
        {
            string text = "TEAM = bills SimData=0x1d\n" + Tsb2 + "\n";
            string tsb1 = TecmoConverter.Convert(TSBContentType.TSB3, TSBContentType.TSB1, text);
            Assert.AreEqual("#" + Tsb2, Colors(tsb1));
        }

        [TestMethod]
        public void Tsb2ToTsb1_RestoresAColorsLineThatCameFromTsb1()
        {
            // TSB1 -> TSB2 comments the TSB1 colors out; going back keeps working as it did
            string text = "TEAM = bills SimData=0x1d\n#" + Nes + "\n#" + Snes1 + "\n";
            string tsb1 = TSB1Converter.ConvertToTSB1FromTSB2(text);
            Assert.AreEqual(Nes + "\n" + Snes1, Colors(tsb1));
        }
    }
}
