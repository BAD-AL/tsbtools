using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// SNES TSB2 (one name table per season) and TSB3 (one table) keep names packed behind a pointer table, in a
    /// fixed-size area (TSB2Tool.NAME_TABLE_SIZE from the first pointer); the player attributes start a few
    /// bytes after it. InsertPlayerName used to shift with no room check, so names that grew too much pushed
    /// the table's last names out of it -- silently. Now a name that doesn't fit is rejected with an error and
    /// the old name is kept, like NES/SNES TSB1's InsertPlayer. Room is measured from the pointers (the pointer
    /// after the last name), so leftover letters in the 00 padding (stock TSB2 has some) never cost room.
    /// </summary>
    [TestClass]
    public class Tsb2Tsb3NameGuardTests
    {
        private const int NameTableSize = 0x4798;

        private class Table
        {
            public int FirstPointer, Offset, Season;
            public Table(int season, int firstPointer, int offset) { Season = season; FirstPointer = firstPointer; Offset = offset; }
            public int End { get { return FirstPointer + NameTableSize; } }
        }

        private static readonly Table[] Tsb2Tables = {
            new Table(1, 0x1e8038, 0x1e0000), new Table(2, 0x1f0038, 0x1e8000), new Table(3, 0x1f8038, 0x1f0000) };
        private static readonly Table Tsb3Table = new Table(1, 0x1e8040, 0x1e0000);

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static List<string> RunAndCollectErrors(Action action)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            try
            {
                action();
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            return giver.Errors;
        }

        // The exclusive end of a table's name data: the pointer after the last name.
        private static int DataEnd(byte[] rom, Table t, int names)
        {
            int p = t.FirstPointer + 2 * names;
            return ((rom[p + 1] << 8) | rom[p]) + t.Offset;
        }

        private static int Names(TSB2Tool tool) { return TSB2Tool.teams.Count * TSB2Tool.positionNames.Count; }

        private static int Bytes(TSB2Tool tool, int season, string team, string pos)
        {
            byte jersey;
            return tool.GetPlayerName(season, team, pos, out jersey).Length; // "f L" == jersey + f + L bytes
        }

        // Grows names in order until the table is exactly full (no write may be rejected), then asks for one
        // byte more: that one must be rejected with an error and change nothing.
        private static void FillThenOverflow(TSB2Tool tool, Table t)
        {
            int names = Names(tool);
            int remaining = t.End - DataEnd(tool.OutputRom, t, names);
            Assert.IsTrue(remaining > 0, "sanity: the table has room");

            List<string> fillErrors = RunAndCollectErrors(delegate
            {
                foreach (string team in TSB2Tool.teams)
                    foreach (string pos in TSB2Tool.positionNames)
                    {
                        if (remaining == 0)
                            return;
                        int oldBytes = Bytes(tool, t.Season, team, pos);
                        int growth = Math.Min(33 - oldBytes, remaining);
                        if (growth <= 0)
                            continue;
                        int letters = oldBytes + growth - 1;
                        tool.InsertPlayerName(t.Season, team, pos, new string('a', letters / 2), new string('B', letters - letters / 2), 1);
                        remaining -= growth;
                    }
            });
            Assert.AreEqual(0, remaining, "sanity: enough players to fill the table");
            CollectionAssert.AreEqual(new List<string>(), fillErrors, "filling to exactly full must work");
            Assert.AreEqual(t.End, DataEnd(tool.OutputRom, t, names), "table exactly full");

            byte[] before = (byte[])tool.OutputRom.Clone();
            byte jersey;
            string oldName = tool.GetPlayerName(t.Season, "bills", "QB1", out jersey);
            List<string> errors = RunAndCollectErrors(delegate
            {
                tool.InsertPlayerName(t.Season, "bills", "QB1", oldName.Split(' ')[0] + "x", oldName.Split(' ')[1], jersey);
            });

            Assert.AreEqual(1, errors.Count, string.Join("\n", errors.ToArray()));
            StringAssert.Contains(errors[0], "would overflow");
            CollectionAssert.AreEqual(before, tool.OutputRom, "a rejected name changes nothing");
        }

        [TestMethod]
        public void Tsb2_EverySeason_FillsExactly_ThenRejectsOneByteMore()
        {
            foreach (Table t in Tsb2Tables)
                FillThenOverflow((TSB2Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc")), t);
        }

        [TestMethod]
        public void Tsb3_FillsExactly_ThenRejectsOneByteMore()
        {
            FillThenOverflow((TSB2Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB3.smc")), Tsb3Table);
        }

        // The seasons' tables are separate: filling season 1 leaves seasons 2 and 3 their own room.
        [TestMethod]
        public void Tsb2_FullSeason1_DoesNotLimitSeason2()
        {
            TSB2Tool tool = (TSB2Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom("TSB2_U.smc"));
            FillThenOverflow(tool, Tsb2Tables[0]);
            StaticUtils.ClearErrors();
            FillThenOverflow(tool, Tsb2Tables[1]);
        }
    }
}
