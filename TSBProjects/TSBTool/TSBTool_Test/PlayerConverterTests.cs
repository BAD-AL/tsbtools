using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// Wraps TSBConverter.cs's own pre-existing self-check methods (TSB2_TSB3\TSB2_3_Core\
    /// TSBConverter.cs) -- never previously run by anything (no caller anywhere calls them; they were
    /// evidently meant to be invoked by hand during development). Each Test*Conversion() method holds
    /// real player data hand-verified against the target format's nibble-packed attribute layout
    /// (e.g. TestQbTSB2Conversion checks Joe Montana and Vinny Testaverde's TSB1 lines convert to the
    /// exact expected TSB2 byte layout) and returns "" on success or a diagnostic string identifying
    /// exactly which comparison failed (via StaticUtils.AreEqual). That's much stronger ground truth
    /// than anything I could construct by hand for this conversion logic, so this just wires them up
    /// to run automatically rather than reinventing the test data.
    ///
    /// Covers TSB1&lt;-&gt;TSB2 in both directions, once per position group (QB, RB, DB, LB, DL, OL,
    /// Kicker, Punter). TSB2Converter also has a TSB3-&gt;TSB2 direction (ConvertToTSB2FromTSB3) and
    /// TSB3Converter has TSB2-&gt;TSB3 (ConvertToTSB3FromTSB2), but neither has any equivalent
    /// Test*Conversion() self-checks to wrap, so they're not covered here.
    /// </summary>
    [TestClass]
    public class PlayerConverterTests
    {
        [TestMethod]
        public void Tsb1ToTsb2_Qb_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestQbTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Rb_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestRbTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Db_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestDbTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Lb_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestLbTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Dl_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestDlTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Ol_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestOlTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Kicker_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestKickerTSB2Conversion());
        }

        [TestMethod]
        public void Tsb1ToTsb2_Punter_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB2Converter.TestPunterTSB2Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Qb_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestQbTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Rb_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestRbTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Db_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestDbTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Lb_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestLbTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Dl_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestDlTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Ol_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestOlTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Kicker_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestKickerTSB1Conversion());
        }

        [TestMethod]
        public void Tsb2ToTsb1_Punter_ConvertsCorrectly()
        {
            Assert.AreEqual("", TSB1Converter.TestPunterTSB1Conversion());
        }
    }
}
