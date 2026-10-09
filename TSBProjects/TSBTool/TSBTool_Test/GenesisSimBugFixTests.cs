using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// GENESIS_FIX_SIM_BUGS: the Genesis game's play-calling code for run/pass preference 1 compares
    /// the roll with $99 but has no branch after it (0xDE790), so "heavy rushing" always passes. The
    /// fix adds the missing branch in place (the run code it needs is already there, unreached, at
    /// 0xDE7AE). Also the sim defense roll (0xDC198) reads the top byte of the random long $FF339C,
    /// which is always 0, instead of the random low byte $FF339F, so team defense SimData barely
    /// counts; the fix changes that one address byte (0xDC225). With defense counting, a stock season
    /// scores ~40% less, so the same group also rebalances (simmed in SimTests GEN_07j): the defense
    /// roll uses (value - 83) via a routine at 0xDF100, the pass play takes 32 more off the pass
    /// defense value via a routine at 0xDF110, and the run-play cutoffs move halfway to the NES ones.
    /// The defense group is written all or nothing. Also, after each game the season receiving-TD
    /// total (stat $0A) is read back with the QB pass-TD helper (byte >> 2) at 0xDD818, so it keeps
    /// collapsing to 0-2; the fix calls the plain-byte helper instead. See TSB_Simulation.md.
    /// </summary>
    [TestClass]
    public class GenesisSimBugFixTests
    {
        private const string RomFileName = @"Genesis\Tecmo Super Bowl (USA) (October 1993).md";
        private const int Pref1Code = 0xDE790;
        private const int DefenseRollCode = 0xDC220;

        private static readonly byte[] StockDefenseRoll = { 0x10, 0x39, 0x00, 0xFF, 0x33, 0x9C }; // move.b $ff339c,d0
        private static readonly byte[] FixedDefenseRoll = { 0x10, 0x39, 0x00, 0xFF, 0x33, 0x9F }; // move.b $ff339f,d0

        private static readonly byte[] StockPref1 =
        {
            0x0C, 0x39, 0x00, 0x99, 0x00, 0xFF, 0x33, 0xB3,
            0x10, 0x39, 0x00, 0xFF, 0x33, 0xB1, 0x02, 0x00, 0x00, 0x03, 0x58, 0x00,
            0x13, 0xC0, 0x00, 0xFF, 0x33, 0xB3, 0x60, 0x00, 0xF9, 0x38
        };

        private static readonly byte[] FixedPref1 =
        {
            0x0C, 0x39, 0x00, 0x99, 0x00, 0xFF, 0x33, 0xB3, // cmpi.b #$99,$ff33b3
            0x65, 0x14,                                     // bcs.s  $DE7AE (run)
            0x10, 0x39, 0x00, 0xFF, 0x33, 0xB1,             // move.b $ff33b1,d0
            0x02, 0x00, 0x00, 0x03,                         // andi.b #3,d0
            0x58, 0x00,                                     // addq.b #4,d0 (pass)
            0x60, 0x10,                                     // bra.s  $DE7B8 (store + exit)
            0x4E, 0x71, 0x4E, 0x71, 0x4E, 0x71              // nop x3 (never reached)
        };

        private const int RecTdCode = 0xDD818;
        private static readonly byte[] StockRecTd = { 0x61, 0x00, 0xFB, 0x16 }; // bsr.w $DD330 (byte >> 2)
        private static readonly byte[] FixedRecTd = { 0x61, 0x00, 0xFA, 0xFE }; // bsr.w $DD318 (byte)

        private const int RollRoutine = 0xDF100;
        private const int PassRoutine = 0xDF110;

        private static readonly byte[] FreeSpace =
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
        };

        // The rebalance that goes with the defense roll fix: location, stock bytes, fixed bytes.
        private static readonly object[][] Balance =
        {
            new object[] { 0xDC23A, new byte[] { 0x32, 0x39, 0x00, 0xFF, 0x33, 0xA8 },   // move.w $ff33a8,d1
                                    new byte[] { 0x4E, 0xB9, 0x00, 0x0D, 0xF1, 0x00 } }, // jsr $DF100
            new object[] { RollRoutine, FreeSpace,
                           new byte[] { 0x32, 0x39, 0x00, 0xFF, 0x33, 0xA8,   // move.w $ff33a8,d1
                                        0x04, 0x41, 0x00, 0x53,               // subi.w #83,d1
                                        0x64, 0x02, 0x72, 0x00,               // bcc.s +2 / moveq #0,d1
                                        0x4E, 0x75 } },                       // rts
            new object[] { 0xDBD24, new byte[] { 0x33, 0xC0, 0x00, 0xFF, 0x33, 0xA8 },   // move.w d0,$ff33a8
                                    new byte[] { 0x4E, 0xB9, 0x00, 0x0D, 0xF1, 0x10 } }, // jsr $DF110
            new object[] { PassRoutine, FreeSpace,
                           new byte[] { 0x04, 0x40, 0x00, 0x20,               // subi.w #32,d0
                                        0x64, 0x02, 0x70, 0x00,               // bcc.s +2 / moveq #0,d0
                                        0x33, 0xC0, 0x00, 0xFF, 0x33, 0xA8,   // move.w d0,$ff33a8
                                        0x4E, 0x75 } },                       // rts
            new object[] { 0xDBA55, new byte[] { 0x3E }, new byte[] { 0x39 } },  // run-play cutoffs
            new object[] { 0xDBA6D, new byte[] { 0x7B }, new byte[] { 0x71 } },
            new object[] { 0xDBA85, new byte[] { 0xB8 }, new byte[] { 0xA9 } },
            new object[] { 0xDBA9D, new byte[] { 0xD7 }, new byte[] { 0xD2 } },
        };

        [TestInitialize]
        public void ClearStaticErrorsBefore() { StaticUtils.ClearErrors(); }

        [TestCleanup]
        public void ClearStaticErrorsAfter() { StaticUtils.ClearErrors(); }

        private static byte[] Slice(byte[] rom)
        {
            byte[] ret = new byte[StockPref1.Length];
            System.Array.Copy(rom, Pref1Code, ret, 0, ret.Length);
            return ret;
        }

        private static byte[] DefenseSlice(byte[] rom)
        {
            byte[] ret = new byte[StockDefenseRoll.Length];
            System.Array.Copy(rom, DefenseRollCode, ret, 0, ret.Length);
            return ret;
        }

        /// <summary>Checks every rebalance part is the stock (fixed == false) or fixed bytes.</summary>
        private static void AssertBalance(byte[] rom, bool isFixed)
        {
            foreach (object[] part in Balance)
            {
                int loc = (int)part[0];
                byte[] expected = (byte[])(isFixed ? part[2] : part[1]);
                byte[] actual = new byte[expected.Length];
                System.Array.Copy(rom, loc, actual, 0, actual.Length);
                CollectionAssert.AreEqual(expected, actual, string.Format("Rebalance bytes at 0x{0:X}", loc));
            }
        }

        private static byte[] RecTdSlice(byte[] rom)
        {
            byte[] ret = new byte[StockRecTd.Length];
            System.Array.Copy(rom, RecTdCode, ret, 0, ret.Length);
            return ret;
        }

        private static bool InAnyFix(int i)
        {
            if (i >= RecTdCode && i < RecTdCode + FixedRecTd.Length)
                return true;
            if (i >= Pref1Code && i < Pref1Code + FixedPref1.Length)
                return true;
            if (i >= DefenseRollCode && i < DefenseRollCode + FixedDefenseRoll.Length)
                return true;
            foreach (object[] part in Balance)
            {
                int loc = (int)part[0];
                if (i >= loc && i < loc + ((byte[])part[2]).Length)
                    return true;
            }
            return false;
        }

        [TestMethod]
        public void GenesisRom_Stock_HasSimBugs()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            CollectionAssert.AreEqual(StockPref1, Slice(rom));
            CollectionAssert.AreEqual(StockDefenseRoll, DefenseSlice(rom));
            AssertBalance(rom, false);
            CollectionAssert.AreEqual(StockRecTd, RecTdSlice(rom));
            Assert.IsTrue(tool.HasSimBugs());
        }

        [TestMethod]
        public void GenesisRom_FixSimBugs_WritesAllFixes_AndNothingElse()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            byte[] original = (byte[])rom.Clone();
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsTrue(tool.FixSimBugs());

            CollectionAssert.AreEqual(FixedPref1, Slice(tool.OutputRom));
            CollectionAssert.AreEqual(FixedDefenseRoll, DefenseSlice(tool.OutputRom));
            AssertBalance(tool.OutputRom, true);
            CollectionAssert.AreEqual(FixedRecTd, RecTdSlice(tool.OutputRom));
            Assert.IsFalse(tool.HasSimBugs());
            for (int i = 0; i < original.Length; i++)
            {
                if (InAnyFix(i))
                    continue;
                Assert.AreEqual(original[i], tool.OutputRom[i], string.Format("Byte 0x{0:X} should be untouched.", i));
            }
        }

        [TestMethod]
        public void GenesisRom_FixSimBugs_TwiceIsHarmless()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.FixSimBugs();
            byte[] once = (byte[])tool.OutputRom.Clone();
            Assert.IsTrue(tool.FixSimBugs());

            CollectionAssert.AreEqual(once, tool.OutputRom);
        }

        [TestMethod]
        public void GenesisRom_Pref1AlreadyFixed_StillGetsTheDefenseFix()
        {
            // e.g. a ROM saved with the first GENESIS_FIX_SIM_BUGS (preference 1 only)
            byte[] rom = TestRoms.LoadRom(RomFileName);
            System.Array.Copy(FixedPref1, 0, rom, Pref1Code, FixedPref1.Length);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsTrue(tool.HasSimBugs());
            Assert.IsTrue(tool.FixSimBugs());

            CollectionAssert.AreEqual(FixedPref1, Slice(tool.OutputRom));
            CollectionAssert.AreEqual(FixedDefenseRoll, DefenseSlice(tool.OutputRom));
            AssertBalance(tool.OutputRom, true);
            Assert.IsFalse(tool.HasSimBugs());
        }

        [TestMethod]
        public void GenesisRom_DefenseRollOnlyFixed_GetsTheRebalance()
        {
            // e.g. a ROM saved by a build that fixed the defense roll without the rebalance
            byte[] rom = TestRoms.LoadRom(RomFileName);
            System.Array.Copy(FixedPref1, 0, rom, Pref1Code, FixedPref1.Length);
            System.Array.Copy(FixedDefenseRoll, 0, rom, DefenseRollCode, FixedDefenseRoll.Length);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsTrue(tool.HasSimBugs());
            Assert.IsTrue(tool.FixSimBugs());

            CollectionAssert.AreEqual(FixedDefenseRoll, DefenseSlice(tool.OutputRom));
            AssertBalance(tool.OutputRom, true);
            Assert.IsFalse(tool.HasSimBugs());
        }

        [TestMethod]
        public void GenesisRom_UnknownDefenseRollBytes_NoDefenseGroupWritten_Pref1StillFixed()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            rom[DefenseRollCode + 5] = 0x12; // someone else's patch
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsFalse(tool.FixSimBugs());

            CollectionAssert.AreEqual(FixedPref1, Slice(tool.OutputRom));
            Assert.AreEqual(0x12, tool.OutputRom[DefenseRollCode + 5]);
            AssertBalance(tool.OutputRom, false);
        }

        [TestMethod]
        public void GenesisRom_RollRoutineSpaceInUse_NoDefenseGroupWritten_Pref1StillFixed()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            rom[RollRoutine + 5] = 0x12; // another hack's code in the free space
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsFalse(tool.FixSimBugs());

            CollectionAssert.AreEqual(FixedPref1, Slice(tool.OutputRom));
            CollectionAssert.AreEqual(StockDefenseRoll, DefenseSlice(tool.OutputRom));
            Assert.AreEqual(0x12, tool.OutputRom[RollRoutine + 5]);
            rom[RollRoutine + 5] = 0xFF;
            tool.OutputRom[RollRoutine + 5] = 0xFF;
            AssertBalance(tool.OutputRom, false);
        }

        [TestMethod]
        public void GenesisRom_PassRoutineSpaceInUse_NoDefenseGroupWritten()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            rom[PassRoutine + 15] = 0x12; // another hack's code in the free space
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsFalse(tool.FixSimBugs());

            CollectionAssert.AreEqual(StockDefenseRoll, DefenseSlice(tool.OutputRom));
            Assert.AreEqual(0x12, tool.OutputRom[PassRoutine + 15]);
            tool.OutputRom[PassRoutine + 15] = 0xFF;
            AssertBalance(tool.OutputRom, false);
        }

        [TestMethod]
        public void GenesisRom_UnknownPref1Bytes_LeavesThemAlone_DefenseStillFixed()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            rom[Pref1Code + 8] = 0x12; // someone else's patch
            byte[] before = Slice(rom);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsFalse(tool.FixSimBugs());
            Assert.IsFalse(tool.HasSimBugs());

            CollectionAssert.AreEqual(before, Slice(tool.OutputRom));
            CollectionAssert.AreEqual(FixedDefenseRoll, DefenseSlice(tool.OutputRom));
            AssertBalance(tool.OutputRom, true);
        }

        [TestMethod]
        public void GenesisRom_ProcessText_FixSimBugsLine_AppliesTheFix()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.ProcessText("GENESIS_FIX_SIM_BUGS\n");

            CollectionAssert.AreEqual(FixedPref1, Slice(tool.OutputRom));
            CollectionAssert.AreEqual(FixedDefenseRoll, DefenseSlice(tool.OutputRom));
            AssertBalance(tool.OutputRom, true);
            CollectionAssert.AreEqual(FixedRecTd, RecTdSlice(tool.OutputRom));
        }

        [TestMethod]
        public void GenesisRom_OnlyReceivingTdBugLeft_HasSimBugs_FixWritesIt()
        {
            // e.g. a ROM saved by a build that had the other fixes but not the receiving-TD one
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool first = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);
            first.FixSimBugs();
            byte[] partlyFixed = (byte[])first.OutputRom.Clone();
            System.Array.Copy(StockRecTd, 0, partlyFixed, RecTdCode, StockRecTd.Length);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(partlyFixed);

            Assert.IsTrue(tool.HasSimBugs());
            Assert.IsTrue(tool.FixSimBugs());

            CollectionAssert.AreEqual(FixedRecTd, RecTdSlice(tool.OutputRom));
            Assert.IsFalse(tool.HasSimBugs());
        }

        [TestMethod]
        public void GenesisRom_UnknownReceivingTdBytes_LeavesThemAlone_OtherFixesStillApplied()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            rom[RecTdCode + 3] = 0x12; // someone else's patch
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            Assert.IsFalse(tool.FixSimBugs());

            Assert.AreEqual(0x12, tool.OutputRom[RecTdCode + 3]);
            CollectionAssert.AreEqual(FixedPref1, Slice(tool.OutputRom));
            CollectionAssert.AreEqual(FixedDefenseRoll, DefenseSlice(tool.OutputRom));
            AssertBalance(tool.OutputRom, true);
        }

        [TestMethod]
        public void GenesisRom_GetAll_DoesNotWriteTheFixLine()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);
            Genesis_TSB1Tool tool = (Genesis_TSB1Tool)TecmoToolFactory.GetToolForRom(rom);

            tool.FixSimBugs();

            Assert.IsFalse(tool.GetAll().Contains("GENESIS_FIX_SIM_BUGS"));
        }

        [TestMethod]
        public void NesRom_ProcessText_FixSimBugsLine_IsIgnored()
        {
            byte[] rom = TestRoms.LoadRom("TSPRBOWL.nes");
            byte[] original = (byte[])rom.Clone();
            ITecmoTool tool = (ITecmoTool)TecmoToolFactory.GetToolForRom(rom);

            tool.ProcessText("GENESIS_FIX_SIM_BUGS\n");

            CollectionAssert.AreEqual(original, tool.OutputRom);
        }
    }
}
