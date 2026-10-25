using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// SNES TSB1 keeps each player's skin tone twice: the in-game bit (0x80 of attribute byte +2) and a cutscene
    /// table at 0x17EF (4 bytes per team, 1 bit per player, MSB first, in position order). SetFace set the
    /// cutscene bit dark for every face except 0 -- so faces 0x01-0x52 (light in game) showed dark in cutscenes,
    /// and the 2026 ROM (NES-style faces) ended up with the whole table FF. A ROM whose table disagrees with the
    /// in-game bits is fixed when it's loaded, with a warning.
    /// </summary>
    [TestClass]
    public class SnesCutsceneSkinTests
    {
        private const string SnesRom = "TSB1.smc";
        private const int CutsceneTable = 0x17EF;
        private static readonly string[] Positions = {
            "QB1", "QB2", "RB1", "RB2", "RB3", "RB4", "WR1", "WR2", "WR3", "WR4", "TE1",
            "TE2", "C", "LG", "RG", "LT", "RT",
            "RE", "NT", "LE", "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS", "SS",
            "K", "P", "DB1", "DB2" };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); StaticUtils.sMessageGiver = null; }

        private static SNES_TecmoTool Load(byte[] rom, List<string> errors)
        {
            TestMessageGiver giver = new TestMessageGiver();
            StaticUtils.ClearErrors();
            StaticUtils.sMessageGiver = giver;
            SNES_TecmoTool tool;
            try
            {
                tool = (SNES_TecmoTool)TecmoToolFactory.GetToolForRom(rom);
                StaticUtils.ShowErrors();
            }
            finally
            {
                StaticUtils.sMessageGiver = null;
            }
            errors.AddRange(giver.Errors);
            return tool;
        }

        private static bool CutsceneDark(SNES_TecmoTool tool, int teamIndex, int positionIndex)
        {
            byte b = tool.OutputRom[CutsceneTable + teamIndex * 4 + positionIndex / 8];
            return ((b >> (7 - positionIndex % 8)) & 1) == 1;
        }

        private static List<string> SkinMismatches(SNES_TecmoTool tool)
        {
            List<string> ret = new List<string>();
            for (int t = 0; t < TecmoTool.Teams.Count; t++)
                for (int p = 0; p < Positions.Length; p++)
                {
                    bool inGameDark = (tool.GetFace(TecmoTool.Teams[t], Positions[p]) & 0x80) != 0;
                    if (inGameDark != CutsceneDark(tool, t, p))
                        ret.Add(TecmoTool.Teams[t] + " " + Positions[p]);
                }
            return ret;
        }

        private static List<string> CutsceneWarnings(List<string> errors)
        {
            return errors.FindAll(delegate(string e) { return e.Contains("cutscene"); });
        }

        [TestMethod]
        public void StockRom_CutsceneTableMatchesInGameSkin_NoWarning()
        {
            List<string> errors = new List<string>();
            SNES_TecmoTool tool = Load(TestRoms.LoadRom(SnesRom), errors);
            CollectionAssert.AreEqual(new List<string>(), SkinMismatches(tool));
            CollectionAssert.AreEqual(new List<string>(), CutsceneWarnings(errors));
        }

        [TestMethod]
        public void SetFace_LightNesFace_IsLightInCutsceneToo()
        {
            SNES_TecmoTool tool = Load(TestRoms.LoadRom(SnesRom), new List<string>());
            int rb1 = Array.IndexOf(Positions, "RB1");
            Assert.IsTrue(CutsceneDark(tool, 0, rb1), "stock bills RB1 is dark");

            tool.SetFace("bills", "RB1", 0x35);

            Assert.AreEqual(0, tool.GetFace("bills", "RB1") & 0x80, "in game: light");
            Assert.IsFalse(CutsceneDark(tool, 0, rb1), "cutscene: light, like the game");
        }

        [TestMethod]
        public void SetFace_DarkFaces_AreDarkInBothPlaces()
        {
            SNES_TecmoTool tool = Load(TestRoms.LoadRom(SnesRom), new List<string>());
            int qb1 = Array.IndexOf(Positions, "QB1");

            foreach (int face in new int[] { 0x53, 0x80, 0xCC })
            {
                tool.SetFace("bills", "QB1", 0x00);
                tool.SetFace("bills", "QB1", face);
                Assert.AreEqual(0x80, tool.GetFace("bills", "QB1") & 0x80, "in game, face 0x" + face.ToString("x"));
                Assert.IsTrue(CutsceneDark(tool, 0, qb1), "cutscene, face 0x" + face.ToString("x"));
            }
        }

        [TestMethod]
        public void SetFace_EveryFace_KeepsBothPlacesInSync()
        {
            SNES_TecmoTool tool = Load(TestRoms.LoadRom(SnesRom), new List<string>());
            for (int face = 0; face <= 0xD4; face += 7)
                tool.SetFace("dolphins", Positions[face % Positions.Length], face);
            CollectionAssert.AreEqual(new List<string>(), SkinMismatches(tool));
        }

        [TestMethod]
        public void Load_BrokenCutsceneTable_IsRebuiltFromInGameSkin_WithOneWarning()
        {
            byte[] stock = TestRoms.LoadRom(SnesRom);
            byte[] broken = (byte[])stock.Clone();
            for (int i = 0; i < 28 * 4; i++)
                broken[CutsceneTable + i] = 0xFF; // what the 2026 ROM had

            List<string> errors = new List<string>();
            SNES_TecmoTool tool = Load(broken, errors);

            for (int i = 0; i < 28 * 4; i++)
                Assert.AreEqual(stock[CutsceneTable + i], tool.OutputRom[CutsceneTable + i], "table byte " + i);
            List<string> warnings = CutsceneWarnings(errors);
            Assert.AreEqual(1, warnings.Count, string.Join("\n", errors.ToArray()));
            StringAssert.Contains(warnings[0], "28 team(s)");
        }
    }
}
