using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// The 3rd RETURN_TEAM slot (SNES and Genesis TSB1) is always a DB in the stock ROMs (RCB, LCB, FS or
    /// SS), and the game treats that player as a defensive back: in the SNES 2026 season every team with a
    /// WR there lost a starter (WR3 took RB2's carries, WR4 took WR1's catches) and the in-game player
    /// screen showed the WR with DB stats. Putting a non-DB there is an error; the return team is not changed.
    /// Slots 1-2 must be RB/WR/TE for the same reason as the stock data (see the hang note below).
    /// DB1/DB2 are not allowed in slot 3 either: there the SNES sim gives DB1 every kickoff and DB2 every
    /// punt, whatever KR/PR say (four simmed seasons, 112 of 112 teams). A ROM that already breaks the
    /// pattern is fixed when it is loaded, with a warning.
    /// </summary>
    [TestClass]
    public class ReturnTeamSlot3Tests
    {
        private const string SnesRom = "TSB1.smc";
        private const string GenesisRom = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";

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

        private static ITecmoTool Load(string rom)
        {
            return (ITecmoTool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(rom));
        }

        private static string GetReturnTeam(ITecmoTool tool, string team)
        {
            SNES_TecmoTool snes = tool as SNES_TecmoTool;
            return snes != null ? snes.GetReturnTeam(team) : ((Genesis_TSB1Tool)tool).GetReturnTeam(team);
        }

        private static string GetKickReturner(ITecmoTool tool, string team)
        {
            SNES_TecmoTool snes = tool as SNES_TecmoTool;
            return snes != null ? snes.GetKickReturner(team) : ((Genesis_TSB1Tool)tool).GetKickReturner(team);
        }

        private static string GetPuntReturner(ITecmoTool tool, string team)
        {
            SNES_TecmoTool snes = tool as SNES_TecmoTool;
            return snes != null ? snes.GetPuntReturner(team) : ((Genesis_TSB1Tool)tool).GetPuntReturner(team);
        }

        [TestMethod]
        public void Snes_SetReturnTeam_WideReceiverInSlot3_IsAnErrorAndKeepsTheOldReturnTeam()
        {
            OffensivePlayerInSlot3_IsAnError(SnesRom);
        }

        [TestMethod]
        public void Genesis_SetReturnTeam_WideReceiverInSlot3_IsAnErrorAndKeepsTheOldReturnTeam()
        {
            OffensivePlayerInSlot3_IsAnError(GenesisRom);
        }

        private static void OffensivePlayerInSlot3_IsAnError(string rom)
        {
            ITecmoTool tool = Load(rom);
            string before = GetReturnTeam(tool, "bills");

            List<string> errors = RunAndCollectErrors(() => tool.SetReturnTeam("bills", "RB2", "WR4", "WR3"));

            Assert.AreEqual(1, errors.Count);
            StringAssert.Contains(errors[0], "RETURN_TEAM");
            StringAssert.Contains(errors[0], "bills");
            StringAssert.Contains(errors[0], "WR3");
            Assert.AreEqual(before, GetReturnTeam(tool, "bills"));
        }

        [TestMethod]
        public void Snes_SetReturnTeam_EveryOffensivePositionInSlot3_IsAnError()
        {
            foreach (string pos in new string[] { "QB1", "RB1", "RB4", "WR1", "WR4", "TE1", "TE2" })
            {
                ITecmoTool tool = Load(SnesRom);
                List<string> errors = RunAndCollectErrors(() => tool.SetReturnTeam("bills", "RB2", "WR4", pos));
                Assert.AreEqual(1, errors.Count, pos);
            }
        }

        [TestMethod]
        public void SetReturnTeam_DefensiveBackInSlot3_IsApplied()
        {
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                foreach (string db in new string[] { "RCB", "LCB", "FS", "SS" })
                {
                    ITecmoTool tool = Load(rom);
                    List<string> errors = RunAndCollectErrors(() => tool.SetReturnTeam("bills", "WR3", "RB1", db));
                    Assert.AreEqual(0, errors.Count, rom + " " + db);
                    Assert.AreEqual("RETURN_TEAM WR3, RB1, " + db, GetReturnTeam(tool, "bills"), rom + " " + db);
                }
            }
        }

        // Slots 1-2 must be RB/WR/TE: with defenders there the SNES game hung (BRA $82EF41 forever) whenever
        // every return-team player was out -- all defenders share one injury flag.
        [TestMethod]
        public void SetReturnTeam_NonSkillPlayerInSlot1Or2_IsAnErrorAndKeepsTheOldReturnTeam()
        {
            string[][] bad = {
                new string[] { "DB1", "WR3", "RCB" }, new string[] { "WR3", "DB2", "RCB" },
                new string[] { "RCB", "WR3", "SS" },  new string[] { "WR3", "FS", "SS" },
                new string[] { "QB1", "WR3", "SS" },  new string[] { "WR3", "C", "SS" },
                new string[] { "RE", "WR3", "SS" },   new string[] { "WR3", "K", "SS" },
            };
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                foreach (string[] r in bad)
                {
                    ITecmoTool tool = Load(rom);
                    string before = GetReturnTeam(tool, "bills");
                    List<string> errors = RunAndCollectErrors(() => tool.SetReturnTeam("bills", r[0], r[1], r[2]));
                    string label = rom + " " + string.Join(",", r);
                    Assert.AreEqual(1, errors.Count, label);
                    StringAssert.Contains(errors[0], "RETURN_TEAM", label);
                    StringAssert.Contains(errors[0], "bills", label);
                    Assert.AreEqual(before, GetReturnTeam(tool, "bills"), label);
                }
            }
        }

        [TestMethod]
        public void SetReturnTeam_SkillPlayersInSlots1And2_AreApplied()
        {
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                foreach (string pos in new string[] { "RB1", "RB4", "WR1", "WR4", "TE1", "TE2" })
                {
                    ITecmoTool tool = Load(rom);
                    List<string> errors = RunAndCollectErrors(() => tool.SetReturnTeam("bills", pos, "WR3", "FS"));
                    Assert.AreEqual(0, errors.Count, rom + " " + pos);
                    Assert.AreEqual("RETURN_TEAM " + pos + ", WR3, FS", GetReturnTeam(tool, "bills"), rom + " " + pos);
                }
            }
        }

        [TestMethod]
        public void StockReturnTeams_AllPassTheCheck()
        {
            string[] teams = ((SNES_TecmoTool)Load(SnesRom)).GetTeams();
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                ITecmoTool tool = Load(rom);
                foreach (string team in teams)
                {
                    string[] p = GetReturnTeam(tool, team).Replace("RETURN_TEAM", "").Replace(" ", "").Split(',');
                    Assert.IsNull(SNES_TecmoTool.ReturnTeamError(team, p[0], p[1], p[2]), rom + " " + team);
                }
            }
        }

        [TestMethod]
        public void SetReturnTeam_DB1OrDB2InSlot3_IsAnErrorAndKeepsTheOldReturnTeam()
        {
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                foreach (string db in new string[] { "DB1", "DB2" })
                {
                    ITecmoTool tool = Load(rom);
                    string before = GetReturnTeam(tool, "bills");
                    List<string> errors = RunAndCollectErrors(() => tool.SetReturnTeam("bills", "WR3", "RB1", db));
                    Assert.AreEqual(1, errors.Count, rom + " " + db);
                    StringAssert.Contains(errors[0], db, rom);
                    Assert.AreEqual(before, GetReturnTeam(tool, "bills"), rom + " " + db);
                }
            }
        }

        // KR/PR for a player not on the return team puts him on it; that must not break the pattern either.
        [TestMethod]
        public void SetKickOrPuntReturner_NotOnTheReturnTeam_KeepsThePattern()
        {
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                ITecmoTool tool = Load(rom);
                tool.SetReturnTeam("bills", "RB2", "WR3", "FS");

                List<string> errors = RunAndCollectErrors(() => tool.SetKickReturner("bills", "DB1"));
                Assert.AreEqual(1, errors.Count, rom + " KR DB1");
                StringAssert.Contains(errors[0], "DB1", rom);
                Assert.AreEqual("RETURN_TEAM RB2, WR3, FS", GetReturnTeam(tool, "bills"), rom);

                errors = RunAndCollectErrors(() => tool.SetPuntReturner("bills", "RCB"));
                Assert.AreEqual(0, errors.Count, rom + " PR RCB");
                Assert.AreEqual("RETURN_TEAM RB2, WR3, RCB", GetReturnTeam(tool, "bills"), rom);
                Assert.AreEqual("RCB", GetPuntReturner(tool, "bills"), rom);

                errors = RunAndCollectErrors(() => tool.SetKickReturner("bills", "WR4"));
                Assert.AreEqual(0, errors.Count, rom + " KR WR4");
                Assert.AreEqual("RETURN_TEAM WR4, WR3, RCB", GetReturnTeam(tool, "bills"), rom);
                Assert.AreEqual("WR4", GetKickReturner(tool, "bills"), rom);
            }
        }

        private static void AssertFix(string[] returnTeam, string kr, string pr, string expected)
        {
            string newKr, newPr;
            string[] fixedTeam = SNES_TecmoTool.FixReturnTeam(returnTeam, kr, pr, out newKr, out newPr);
            string label = string.Join(",", returnTeam) + " KR " + kr + " PR " + pr;
            Assert.IsNotNull(fixedTeam, label);
            Assert.AreEqual(expected, string.Join(", ", fixedTeam) + " KR " + newKr + " PR " + newPr, label);
            Assert.IsNull(SNES_TecmoTool.ReturnTeamError("t", fixedTeam[0], fixedTeam[1], fixedTeam[2]), label);
        }

        [TestMethod]
        public void FixReturnTeam_KeepsTheReturnersAndFillsWithWR3WR4AndRCB()
        {
            // DB returners move to slot 3 as a starting DB
            AssertFix(new[] { "DB1", "DB2", "RB3" }, "DB1", "DB1", "RB3, WR3, RCB KR RCB PR RCB");
            AssertFix(new[] { "RB4", "DB1", "WR3" }, "RB4", "DB1", "RB4, WR3, RCB KR RB4 PR RCB");
            // offensive returners keep their slots; a starting DB already there is kept
            AssertFix(new[] { "RB2", "DB2", "DB1" }, "RB2", "RB2", "RB2, WR3, RCB KR RB2 PR RB2");
            AssertFix(new[] { "WR3", "DB2", "RB2" }, "WR3", "WR3", "WR3, RB2, RCB KR WR3 PR WR3");
            AssertFix(new[] { "DB2", "RB2", "WR4" }, "DB2", "RB2", "WR4, RB2, RCB KR RCB PR RB2");
            AssertFix(new[] { "WR4", "WR3", "RB4" }, "WR4", "WR3", "WR4, WR3, RCB KR WR4 PR WR3");
            AssertFix(new[] { "FS", "WR3", "SS" }, "FS", "WR3", "WR4, WR3, FS KR FS PR WR3");
            // non-skill offense (QB, OL) loses its spot like a defender
            AssertFix(new[] { "QB2", "RB2", "LCB" }, "QB2", "RB2", "WR3, RB2, LCB KR RB2 PR RB2");
        }

        [TestMethod]
        public void FixReturnTeam_ValidReturnTeam_ReturnsNull()
        {
            string newKr, newPr;
            Assert.IsNull(SNES_TecmoTool.FixReturnTeam(new[] { "RB2", "WR3", "RCB" }, "RB2", "RCB", out newKr, out newPr));
        }

        [TestMethod]
        public void LoadingARomWithABrokenReturnTeam_FixesItAndWarns()
        {
            // bills: DB1, DB2, RB3 with KR and PR = slot 1 (DB1)
            OnLoad_FixesBills(SnesRom, 0x170cb0, 0x170c90);
            OnLoad_FixesBills(GenesisRom, 0x4BDC, 0x4BC0);
        }

        private static void OnLoad_FixesBills(string rom, int returnTeamStart, int krPrStart)
        {
            byte[] bytes = TestRoms.LoadRom(rom);
            bytes[returnTeamStart] = 0x1E; bytes[returnTeamStart + 1] = 0x1F; bytes[returnTeamStart + 2] = 0x04;
            bytes[krPrStart] = 0x00;
            ITecmoTool tool = null;
            List<string> errors = RunAndCollectErrors(() => tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(bytes));

            Assert.AreEqual("RETURN_TEAM RB3, WR3, RCB", GetReturnTeam(tool, "bills"), rom);
            Assert.AreEqual("RCB", GetKickReturner(tool, "bills"), rom);
            Assert.AreEqual("RCB", GetPuntReturner(tool, "bills"), rom);
            Assert.AreEqual(1, errors.Count, rom);
            StringAssert.Contains(errors[0], "auto-fixed", rom);
            StringAssert.Contains(errors[0], "bills: RETURN_TEAM DB1, DB2, RB3 (KR DB1, PR DB1) -> RETURN_TEAM RB3, WR3, RCB (KR RCB, PR RCB)", rom);
        }

        [TestMethod]
        public void LoadingAStockRom_ChangesNothingAndDoesNotWarn()
        {
            foreach (string rom in new string[] { SnesRom, GenesisRom })
            {
                byte[] bytes = TestRoms.LoadRom(rom);
                byte[] copy = (byte[])bytes.Clone();
                List<string> errors = RunAndCollectErrors(() => TecmoToolFactory.GetToolForRom(bytes));
                Assert.AreEqual(0, errors.Count, rom);
                CollectionAssert.AreEqual(copy, bytes, rom);
            }
        }

        [TestMethod]
        public void Snes_InputText_WideReceiverInSlot3_IsReported()
        {
            ITecmoTool tool = Load(SnesRom);
            string before = GetReturnTeam(tool, "bills");

            List<string> errors = RunAndCollectErrors(() =>
            {
                InputParser parser = new InputParser(tool);
                parser.ProcessLines(new string[] { "TEAM = bills", "RETURN_TEAM RB2, WR4, WR3" });
            });

            Assert.IsTrue(errors.Exists(e => e.Contains("RETURN_TEAM") && e.Contains("WR3")));
            Assert.AreEqual(before, GetReturnTeam(tool, "bills"));
        }
    }
}
