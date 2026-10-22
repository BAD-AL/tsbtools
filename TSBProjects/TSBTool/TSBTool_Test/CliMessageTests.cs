using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// The command line's "missing file" messages: an input (data) file is optional -- printing a ROM, -stats:,
    /// -help don't take one -- so leaving it out says nothing. A ROM is needed only to print or modify one, so
    /// "No valid rom file" shows only then (-stats: works without a ROM).
    /// </summary>
    [TestClass]
    public class CliMessageTests
    {
        private const string NoInput = "No valid input file";
        private const string NoRom = "No valid rom file";

        [TestCleanup]
        public void Cleanup()
        {
            StaticUtils.sMessageGiver = null;
            StaticUtils.ClearErrors();
        }

        /// <summary>Runs the command line; returns { stdout, stderr }.</summary>
        private static string[] Run(params string[] args)
        {
            TextWriter oldErr = Console.Error, oldOut = Console.Out;
            StringWriter err = new StringWriter(), output = new StringWriter();
            try
            {
                Console.SetError(err);
                Console.SetOut(output);
                MainClass.RunMain(args);
            }
            finally
            {
                Console.SetError(oldErr);
                Console.SetOut(oldOut);
            }
            return new[] { output.ToString(), err.ToString() };
        }

        private static string Save(string name)
        {
            return TestRoms.GetRomPath(@"SNES\SaveRam\" + name);
        }

        [TestMethod]
        public void PrintRom_WithoutInputFile_SaysNothingAboutIt()
        {
            string[] r = Run(TestRoms.GetRomPath("TSPRBOWL.nes"));
            StringAssert.Contains(r[0], "TEAM = bills");
            Assert.IsFalse(r[1].Contains(NoInput), r[1]);
            Assert.IsFalse(r[1].Contains(NoRom), r[1]);
        }

        [TestMethod]
        public void Stats_WithoutRom_SaysNothingAboutFiles()
        {
            string[] r = Run("-stats:" + Save("TSB2_1994_Season.srm"));
            StringAssert.Contains(r[0], "STANDINGS");
            Assert.IsFalse(r[1].Contains(NoInput), r[1]);
            Assert.IsFalse(r[1].Contains(NoRom), r[1]);
        }

        [TestMethod]
        public void Stats_WithRom_SaysNothingAboutTheInputFile()
        {
            string[] r = Run(TestRoms.GetRomPath("TSB2_U.smc"), "-stats:" + Save("TSB2_1994_Season.srm"));
            StringAssert.Contains(r[0], "STANDINGS");
            Assert.IsFalse(r[1].Contains(NoInput), r[1]);
        }

        [TestMethod]
        public void InputFileWithoutRom_StillSaysTheRomIsMissing()
        {
            string[] r = Run(TestRoms.GetConversionTestPath("ConversionTest.txt"));
            StringAssert.Contains(r[1], NoRom);
            Assert.IsFalse(r[1].Contains(NoInput), r[1]);
        }
    }
}
