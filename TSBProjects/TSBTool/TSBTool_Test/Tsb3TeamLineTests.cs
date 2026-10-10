using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB3's team 'SimData' is its own: TSB3's sim doesn't read the team byte TSB2 has (rush D / pass D levels),
    /// it works out team defense from the starting defenders plus a hidden per-team bonus, rush ($84:EA43, file
    /// 0x26A43 + team) and pass ($84:EA67, file 0x26A67 + team). TSB3 team lines show those two bonuses as
    /// 'SimData=0xRRPP'. Checked with a simmed season: SimTests\TSB3_study\TSB3_DefBonus_results.txt.
    /// A one-byte (TSB2 style) SimData in a TSB3 text is ignored with one warning; TSB2 ignores a TSB3 one.
    /// </summary>
    [TestClass]
    public class Tsb3TeamLineTests
    {
        const int RushBonus = 0x26A43, PassBonus = 0x26A67;
        const int Jets = 4, Bears = 20;

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static TSB2Tool Load(string rom)
        {
            return TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom)) as TSB2Tool;
        }

        /// <summary>The 'team' section (team line .. the next team line) of season 1, as the tool writes it.</summary>
        private static string TeamSection(TSB2Tool tool, string team)
        {
            string all = tool.GetTeams(1);
            int start = all.IndexOf("TEAM = " + team + ",");
            int end = all.IndexOf("TEAM = ", start + 1);
            return all.Substring(start, end - start);
        }

        /// <summary>Applies 'text', returning the errors/warnings it gave.</summary>
        private static System.Collections.Generic.List<string> Apply(TSB2Tool tool, string text)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.sMessageGiver = giver;
            try
            {
                tool.ProcessText(text);
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            return giver.Errors;
        }

        [TestMethod]
        public void Tsb3_TeamLines_ShowTheDefenseBonus()
        {
            string text = Load("TSB3.smc").GetTeams(1);
            StringAssert.Contains(text, "TEAM = bills,SimData=0x0000\n");
            StringAssert.Contains(text, "TEAM = steelers,SimData=0x0A14\n");   // rush 10, pass 20
            StringAssert.Contains(text, "TEAM = cardinals,SimData=0x0F0A\n");  // rush 15, pass 10
            StringAssert.Contains(text, "TEAM = oilers,SimData=0x000F\n");     // pass 15 only
        }

        [TestMethod]
        public void Tsb3_FreeAgentsAndAllTime_HaveNoSimData()
        {
            string text = Load("TSB3.smc").GetTeams(1);
            StringAssert.Contains(text, "TEAM = freeAgents\n");
            StringAssert.Contains(text, "TEAM = allTime\n");
        }

        [TestMethod]
        public void Tsb3_SetTeamSimData_WritesOnlyTheTwoBonusBytes()
        {
            TSB2Tool tool = Load("TSB3.smc");
            byte[] before = (byte[])tool.OutputRom.Clone();
            tool.SetTeamSimData(1, "jets", "0xFF14");

            Assert.AreEqual(0xFF, tool.OutputRom[RushBonus + Jets]);
            Assert.AreEqual(0x14, tool.OutputRom[PassBonus + Jets]);
            for (int i = 0; i < before.Length; i++)
                if (i != RushBonus + Jets && i != PassBonus + Jets)
                    Assert.AreEqual(before[i], tool.OutputRom[i], "byte 0x" + i.ToString("X") + " changed");
            Assert.AreEqual("SimData=0xFF14", tool.GetTeamSimData(1, "jets"));
        }

        [TestMethod]
        public void Tsb3_SimDataInText_IsApplied()
        {
            TSB2Tool tool = Load("TSB3.smc");
            string bears = TeamSection(tool, "bears");
            StringAssert.StartsWith(bears, "TEAM = bears,SimData=0x0514\n");

            var errors = Apply(tool, "SEASON 1\n" + bears.Replace("SimData=0x0514", "SimData=0x0C03"));

            Assert.AreEqual(0, errors.Count, string.Join("\n", errors.ToArray()));
            Assert.AreEqual(0x0C, tool.OutputRom[RushBonus + Bears]);
            Assert.AreEqual(0x03, tool.OutputRom[PassBonus + Bears]);
        }

        [TestMethod]
        public void Tsb3_OldOneByteSimData_IsIgnored_WithOneWarning()
        {
            TSB2Tool tool = Load("TSB3.smc");
            string all = tool.GetTeams(1);
            // the Bills and Colts (whole-season texts hit the known multi-word last name round trip, 'DEL GRECO')
            string oldStyle = "SEASON 1\n" +
                TeamSection(tool, "bills").Replace("SimData=0x0000", "SimData=0x66") +
                TeamSection(tool, "colts").Replace("SimData=0x0000", "SimData=0x23");

            var errors = Apply(tool, oldStyle);

            Assert.AreEqual(1, errors.Count, string.Join("\n", errors.ToArray()));
            StringAssert.Contains(errors[0], "0xRRPP");
            Assert.AreEqual(all, tool.GetTeams(1), "nothing should change");
        }

        [TestMethod]
        public void Tsb3_Key_ExplainsItsSimData()
        {
            string key = Load("TSB3.smc").GetKey();
            StringAssert.Contains(key, "SimData=0xRRPP");
            StringAssert.Contains(key, "bonus");
        }

        [TestMethod]
        public void Tsb2_TeamLines_StillHaveSimData()
        {
            StringAssert.Contains(Load("TSB2_U.smc").GetTeams(1), "TEAM = bills,SimData=0xD8\n");
        }

        [TestMethod]
        public void Tsb2_Tsb3StyleSimData_IsIgnored_WithAWarning()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            string bills = TeamSection(tool, "bills");

            var errors = Apply(tool, "SEASON 1\n" + bills.Replace("SimData=0xD8", "SimData=0x0A14"));

            Assert.AreEqual(1, errors.Count, string.Join("\n", errors.ToArray()));
            Assert.AreEqual("SimData=0xD8", tool.GetTeamSimData(1, "bills"));
        }

        [TestMethod]
        public void Tsb2_Tsb1StyleSimData_WithPrefDigit_StillSetsTheByte()
        {
            TSB2Tool tool = Load("TSB2_U.smc");
            string bills = TeamSection(tool, "bills");

            var errors = Apply(tool, "SEASON 1\n" + bills.Replace("TEAM = bills,SimData=0xD8", "TEAM = bills SimData=0x1F2"));

            Assert.AreEqual(0, errors.Count, string.Join("\n", errors.ToArray()));
            Assert.AreEqual("SimData=0x1F", tool.GetTeamSimData(1, "bills"));
        }
    }

    /// <summary>
    /// Converting to TSB3 turns a TSB1/TSB2 team SimData (rush D / pass D digits, the whole team defense) into
    /// TSB3's defense bonus, which comes on top of the converted defenders: only digits above 8 give a bonus,
    /// 2 rush / 3 pass per digit (F -> 14 / 21, near Tecmo's top bonuses 15 / 20). Converting from TSB3 drops
    /// TSB3's SimData (it means something else in TSB1/TSB2), leaving the ROM's own team byte.
    /// </summary>
    [TestClass]
    public class Tsb3TeamSimDataConverterTests
    {
        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        [TestMethod]
        public void Tsb3SimData_FromRushAndPassDigits()
        {
            Assert.AreEqual("0x0000", TSB3Converter.Tsb3SimData(0x0, 0x0));
            Assert.AreEqual("0x0000", TSB3Converter.Tsb3SimData(0x8, 0x8));
            Assert.AreEqual("0x0203", TSB3Converter.Tsb3SimData(0x9, 0x9));
            Assert.AreEqual("0x080C", TSB3Converter.Tsb3SimData(0xC, 0xC));
            Assert.AreEqual("0x0600", TSB3Converter.Tsb3SimData(0xB, 0x6));
            Assert.AreEqual("0x0E15", TSB3Converter.Tsb3SimData(0xF, 0xF));
        }

        [TestMethod]
        public void Tsb2ToTsb3_TeamLine_GetsTheBonus()
        {
            string output = TSB3Converter.ConvertToTSB3FromTSB2("SEASON 1\nTEAM = cowboys,SimData=0xCC\nTEAM = bills,SimData=0x66\n");
            StringAssert.Contains(output, "TEAM = cowboys,SimData=0x080C\n");
            StringAssert.Contains(output, "TEAM = bills,SimData=0x0000\n");
        }

        [TestMethod]
        public void Tsb2ToTsb3_TeamLineWithoutSimData_IsUnchanged()
        {
            string output = TSB3Converter.ConvertToTSB3FromTSB2("TEAM = bills\nTEAM_ABB=BUF,TEAM_CITY=buffalo,TEAM_NAME=bills\n");
            StringAssert.Contains(output, "TEAM = bills\nTEAM_ABB=BUF,");
        }

        [TestMethod]
        public void Tsb1ToTsb3_TeamLine_GetsTheBonus_PrefDigitDropped()
        {
            string output = TecmoConverter.Convert(TSBContentType.TSB1, TSBContentType.TSB3, "TEAM = bills SimData=0xAB1\n");
            StringAssert.Contains(output, "TEAM = bills SimData=0x0409\n");
        }

        [TestMethod]
        public void Tsb3ToTsb2_DropsTsb3SimData()
        {
            string output = TecmoConverter.Convert(TSBContentType.TSB3, TSBContentType.TSB2, "TEAM = steelers,SimData=0x0A14\n");
            StringAssert.Contains(output, "TEAM = steelers\n");
            Assert.IsFalse(output.Contains("SimData"), output);
        }

        [TestMethod]
        public void Tsb3ToTsb1_DropsTsb3SimData()
        {
            string output = TecmoConverter.Convert(TSBContentType.TSB3, TSBContentType.TSB1, "TEAM = steelers,SimData=0x0A14\n");
            Assert.IsFalse(output.Contains("SimData"), output);
        }
    }
}
