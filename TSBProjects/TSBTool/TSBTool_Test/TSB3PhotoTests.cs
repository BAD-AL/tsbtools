using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TSBTool;
using TSBTool2;

namespace TSBTool_Test
{
    /// <summary>
    /// TSB3's player photos (TSB3Photos.cs): the photo tables of the stock ROM and the -photosOut export.
    /// </summary>
    [TestClass]
    public class TSB3PhotoTests
    {
        private const string RomFileName = "TSB3.smc";

        [TestMethod]
        public void StockRom_PhotoTables()
        {
            byte[] rom = TestRoms.LoadRom(RomFileName);

            Assert.AreEqual(0x000, TSB3Photos.GetPlayerId(rom, 0));    // bills QB1, Jim Kelly
            Assert.AreEqual(0x431, TSB3Photos.GetPlayerId(rom, 78));   // 49ers QB1, Steve Young
            Assert.AreEqual(0x29C, TSB3Photos.GetPlayerId(rom, 83));   // eagles RB1, Ricky Watters (moved from the 49ers)
            Assert.AreEqual(-1, TSB3Photos.GetPlayerId(rom, TSB3Photos.DefaultPhoto));

            // photos are back to back in bank $A0; the silhouette is last
            for (int photo = 0; photo < TSB3Photos.PhotoCount; photo++)
                Assert.AreEqual(0x100000 + photo * TSB3Photos.PhotoBytes, TSB3Photos.GetPhotoLocation(rom, photo));

            Color[] palette = TSB3Photos.GetPalette(rom);
            Assert.AreEqual(Color.FromArgb(0, 0, 0).ToArgb(), palette[0].ToArgb());
            Assert.AreEqual(Color.FromArgb(255, 255, 255).ToArgb(), palette[15].ToArgb());
            for (int i = 0; i < 16; i++)
                Assert.IsTrue(palette[i].R == palette[i].G && palette[i].G == palette[i].B, "gray " + i);
        }

        [TestMethod]
        public void StockRom_Describe()
        {
            TSB3Tool tool = (TSB3Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));

            Assert.AreEqual("bills QB1 jim KELLY", TSB3Photos.Describe(tool, 0));
            Assert.AreEqual("freeAgents RCB deion SANDERS", TSB3Photos.Describe(tool, 81));
            Assert.AreEqual("no photo (everyone else)", TSB3Photos.Describe(tool, TSB3Photos.DefaultPhoto));
        }

        [TestMethod]
        public void PhotosOut_WritesEveryPhotoAsTheGameShowsIt()
        {
            RunInScratchDir(scratchDir =>
            {
                File.Copy(TestRoms.GetRomPath(RomFileName), Path.Combine(scratchDir, "tsb3.smc"));

                MainClass.TestString = "";
                MainClass.RunMain(new[] { "tsb3.smc", "-photosOut:photos" });

                byte[] rom = File.ReadAllBytes(Path.Combine(scratchDir, "tsb3.smc"));
                Color[] palette = TSB3Photos.GetPalette(rom);
                string[] files = Directory.GetFiles(Path.Combine(scratchDir, "photos"));
                Assert.AreEqual(TSB3Photos.PhotoCount, files.Length);
                for (int photo = 0; photo < TSB3Photos.PhotoCount; photo++)
                {
                    string file = Path.Combine(Path.Combine(scratchDir, "photos"), string.Format("0x{0:X2}.png", photo));
                    Assert.IsTrue(File.Exists(file), file);
                    byte[,] pixels = TSB3Photos.GetPixels(rom, photo);
                    using (Bitmap image = new Bitmap(file))
                    {
                        Assert.AreEqual(24, image.Width);
                        Assert.AreEqual(32, image.Height);
                        for (int y = 0; y < 32; y++)
                        {
                            for (int x = 0; x < 24; x++)
                            {
                                Assert.AreNotEqual(0, pixels[y, x], "photos never use color 0 (transparent)");
                                Assert.AreEqual(palette[pixels[y, x]].ToArgb(), image.GetPixel(x, y).ToArgb(),
                                    string.Format("photo {0} at {1},{2}", photo, x, y));
                            }
                        }
                    }
                }
                StringAssert.Contains(MainClass.TestString, "0x00.png  bills QB1 jim KELLY");
                StringAssert.Contains(MainClass.TestString, "0x54.png  no photo (everyone else)");
            });
        }

        [TestMethod]
        public void PhotosOut_RefusesOtherRoms()
        {
            RunInScratchDir(scratchDir =>
            {
                File.Copy(TestRoms.GetRomPath("TSB2_U.smc"), Path.Combine(scratchDir, "tsb2.smc"));

                MainClass.RunMain(new[] { "tsb2.smc", "-photosOut:photos" });

                Assert.IsFalse(Directory.Exists(Path.Combine(scratchDir, "photos")));
            });
        }

        [TestMethod]
        public void PhotosIn_ExportedPhotos_LeaveTheRomAsItWas()
        {
            RunInScratchDir(scratchDir =>
            {
                TSB3Tool tool = LoadTool();
                byte[] before = (byte[])tool.OutputRom.Clone();
                TSB3Photos.ExportPhotos(tool, "photos");

                int changed;
                string report = TSB3Photos.ImportPhotos(tool, "photos", out changed);

                Assert.AreEqual(0, changed);
                CollectionAssert.AreEqual(before, tool.OutputRom);
                StringAssert.Contains(report, "0x00.png  bills QB1 jim KELLY: unchanged");
                StringAssert.Contains(report, "0x54.png  no photo (everyone else): unchanged");
            });
        }

        [TestMethod]
        public void PhotosIn_EditedPixels_ChangeOnlyThosePhotos()
        {
            RunInScratchDir(scratchDir =>
            {
                TSB3Tool tool = LoadTool();
                byte[] before = (byte[])tool.OutputRom.Clone();
                Color[] palette = TSB3Photos.GetPalette(before);
                Directory.CreateDirectory("photos");
                foreach (int photo in new[] { 5, TSB3Photos.DefaultPhoto })
                {
                    using (Bitmap image = TSB3Photos.GetPhotoImage(before, photo))
                    {
                        int was = TSB3Photos.GetPixels(before, photo)[4, 3];
                        image.SetPixel(3, 4, palette[was == 1 ? 15 : 1]);
                        image.Save(Path.Combine("photos", TSB3Photos.FileName(photo)), System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                int changed;
                string report = TSB3Photos.ImportPhotos(tool, "photos", out changed);

                Assert.AreEqual(2, changed);
                StringAssert.Contains(report, "0x05.png  dolphins WR1 irving FRYAR: written\r\n");
                StringAssert.Contains(report, "0x54.png  no photo (everyone else): written\r\n");
                AssertOnlyPhotosChanged(before, tool.OutputRom, 5, TSB3Photos.DefaultPhoto);
                foreach (int photo in new[] { 5, TSB3Photos.DefaultPhoto })
                {
                    byte[,] was = TSB3Photos.GetPixels(before, photo), now = TSB3Photos.GetPixels(tool.OutputRom, photo);
                    Assert.AreEqual(was[4, 3] == 1 ? 15 : 1, now[4, 3]);
                    now[4, 3] = was[4, 3];
                    CollectionAssert.AreEqual(was, now, "only the one pixel");
                }
            });
        }

        [TestMethod]
        public void PhotosIn_OtherColorsAt24x32_BecomeTheNearestGray()
        {
            RunInScratchDir(scratchDir =>
            {
                TSB3Tool tool = LoadTool();
                Directory.CreateDirectory("photos");
                using (Bitmap image = new Bitmap(24, 32))
                {
                    for (int y = 0; y < 32; y++)
                        for (int x = 0; x < 24; x++)
                            image.SetPixel(x, y, y < 16 ? Color.White : Color.FromArgb(250, 250, 250));
                    image.Save(Path.Combine("photos", "0x07.bmp"), System.Drawing.Imaging.ImageFormat.Bmp);
                }

                int changed;
                string report = TSB3Photos.ImportPhotos(tool, "photos", out changed);

                Assert.AreEqual(1, changed);
                StringAssert.Contains(report, "0x07.bmp  patriots TE1 ben COATES: written (384 pixels set to the nearest of the game's grays)");
                byte[,] pixels = TSB3Photos.GetPixels(tool.OutputRom, 7);
                foreach (byte p in pixels)
                    Assert.AreEqual(15, p, "white");
            });
        }

        [TestMethod]
        public void PhotosIn_Headshot_IsConverted()
        {
            RunInScratchDir(scratchDir =>
            {
                TSB3Tool tool = LoadTool();
                byte[] before = (byte[])tool.OutputRom.Clone();
                Directory.CreateDirectory("photos");
                // a 'headshot': a dark brown face on white, wider than 3:4
                using (Bitmap image = new Bitmap(160, 128))
                {
                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.Clear(Color.White);
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(90, 60, 40)))
                            g.FillEllipse(brush, 50, 20, 60, 90);
                    }
                    image.Save(Path.Combine("photos", "0x10.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                int changed;
                string report = TSB3Photos.ImportPhotos(tool, "photos", out changed);

                Assert.AreEqual(1, changed);
                StringAssert.Contains(report, "0x10.jpg  falcons RB3 eric METCALF: written (converted from 160x128)");
                AssertOnlyPhotosChanged(before, tool.OutputRom, 0x10);
                byte[,] pixels = TSB3Photos.GetPixels(tool.OutputRom, 0x10);
                bool dark = false, light = false;
                foreach (byte p in pixels)
                {
                    Assert.IsTrue(p >= 1 && p <= 15);
                    dark |= p == 1;
                    light |= p == 15;
                }
                Assert.IsTrue(dark && light, "stretched to the full range of grays");
                Assert.IsTrue(pixels[16, 12] < pixels[2, 1], "the face (center) is darker than the background (corner)");
            });
        }

        [TestMethod]
        public void PhotosIn_BadNamesAndDuplicates_AreReportedAndSkipped()
        {
            RunInScratchDir(scratchDir =>
            {
                TSB3Tool tool = LoadTool();
                byte[] before = (byte[])tool.OutputRom.Clone();
                Directory.CreateDirectory("photos");
                using (Bitmap image = new Bitmap(24, 32))
                {
                    foreach (string name in new[] { "0x55.png", "photo.png", "0x01.png", "0x01.bmp" })
                        image.Save(Path.Combine("photos", name), System.Drawing.Imaging.ImageFormat.Png);
                }
                File.WriteAllText(Path.Combine("photos", "notes.txt"), "not an image");
                File.WriteAllText(Path.Combine("photos", "0x02.png"), "not an image either");

                int changed;
                string report = TSB3Photos.ImportPhotos(tool, "photos", out changed);

                Assert.AreEqual(0, changed);
                CollectionAssert.AreEqual(before, tool.OutputRom);
                StringAssert.Contains(report, "0x55.png  ignored: not a photo name (0x00 - 0x54)");
                StringAssert.Contains(report, "photo.png  ignored: not a photo name (0x00 - 0x54)");
                StringAssert.Contains(report, "0x01.bmp  ERROR: more than one file for photo 0x01 (0x01.bmp, 0x01.png); left as it was");
                StringAssert.Contains(report, "0x02.png  ERROR: can't read the image");
                Assert.IsFalse(report.Contains("notes.txt"));
            });
        }

        [TestMethod]
        public void PhotosIn_Cli_SavesToOutAndLeavesTheRomAlone()
        {
            RunInScratchDir(scratchDir =>
            {
                File.Copy(TestRoms.GetRomPath(RomFileName), "tsb3.smc");
                byte[] before = File.ReadAllBytes("tsb3.smc");
                MainClass.RunMain(new[] { "tsb3.smc", "-photosOut:photos" });
                using (Bitmap image = new Bitmap(100, 100))
                {
                    using (Graphics g = Graphics.FromImage(image))
                        g.Clear(Color.Gray);
                    image.SetPixel(50, 50, Color.Black);
                    image.Save(Path.Combine("photos", "0x2A.png"), System.Drawing.Imaging.ImageFormat.Png);
                }

                MainClass.RunMain(new[] { "tsb3.smc", "-photosIn:photos", "-out:new.smc" });

                CollectionAssert.AreEqual(before, File.ReadAllBytes("tsb3.smc"));
                AssertOnlyPhotosChanged(before, File.ReadAllBytes("new.smc"), 0x2A);
                StringAssert.Contains(MainClass.TestString, "0x2A.png  cardinals LILB seth JOYNER: written (converted from 100x100)");
                StringAssert.Contains(MainClass.TestString, "0x00.png  bills QB1 jim KELLY: unchanged");

                // nothing to change: nothing saved
                MainClass.RunMain(new[] { "new.smc", "-photosOut:again" });
                MainClass.RunMain(new[] { "new.smc", "-photosIn:again", "-out:same.smc" });
                Assert.IsFalse(File.Exists("same.smc"));
            });
        }

        [TestMethod]
        public void PhotosIn_RefusesOtherRoms()
        {
            RunInScratchDir(scratchDir =>
            {
                File.Copy(TestRoms.GetRomPath("TSB2_U.smc"), "tsb2.smc");
                Directory.CreateDirectory("photos");

                MainClass.RunMain(new[] { "tsb2.smc", "-photosIn:photos", "-out:new.smc" });

                Assert.IsFalse(File.Exists("new.smc"));
            });
        }

        [TestMethod]
        public void Slots_AssignMovesAPlayersPhotoAndFreesTheOldOne()
        {
            byte[] rom = LoadTool().OutputRom;
            int favre = TSB3Photos.PlayerId("packers", "QB1");
            Assert.AreEqual(0x32E, favre);
            string team, position;
            Assert.IsTrue(TSB3Photos.GetSlot(favre, out team, out position));
            Assert.AreEqual("packers QB1", team + " " + position);
            Assert.IsFalse(TSB3Photos.GetSlot(TSB3Photos.FreeId, out team, out position));
            Assert.AreEqual(61, TSB3Photos.FindPhoto(rom, favre));
            Assert.AreEqual(84, TSB3Photos.UsedCount(rom));

            // Jim Kelly's photo (0) to Favre: Favre's own photo (61) gets no player
            Assert.AreEqual(61, TSB3Photos.AssignPhoto(rom, 0, favre));
            Assert.AreEqual(0, TSB3Photos.FindPhoto(rom, favre));
            Assert.AreEqual(TSB3Photos.FreeId, TSB3Photos.GetPlayerId(rom, 61));
            Assert.AreEqual(-1, TSB3Photos.FindPhoto(rom, 0x000), "Kelly has none now");
            Assert.AreEqual("free", TSB3Photos.Describe(rom, 61, null));
            Assert.AreEqual("packers QB1", TSB3Photos.Describe(rom, 0, null));
            Assert.AreEqual(83, TSB3Photos.UsedCount(rom));

            Assert.AreEqual(-1, TSB3Photos.AssignPhoto(rom, 5, TSB3Photos.FreeId));
            Assert.AreEqual(82, TSB3Photos.UsedCount(rom));
            try
            {
                TSB3Photos.SetPlayerId(rom, TSB3Photos.DefaultPhoto, 0);
                Assert.Fail("the silhouette has no player");
            }
            catch (ArgumentOutOfRangeException) { }
        }

        [TestMethod]
        public void Convert_DefaultsAreTheCliConversion_AdjustmentsAndCropsWork()
        {
            Color[] palette = TSB3Photos.GetPalette(TestRoms.LoadRom(RomFileName));
            using (Bitmap image = new Bitmap(90, 100))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.Clear(Color.LightGray);
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(90, 60, 40)))
                        g.FillEllipse(brush, 25, 15, 40, 60);
                }
                double[] lum = TSB3Photos.GetLuminance(TSB3Photos.GetArgb(image));
                string how;
                byte[,] cli = TSB3Photos.ImageToPixels(image, palette, out how);
                RectangleF fit = TSB3Photos.DefaultCrop(90, 100);
                Assert.AreEqual(new RectangleF(7, 0, 75, 100), fit, "a 3:4 frame as tall as the image, centered");
                Assert.AreEqual(new RectangleF(0, 0, 60, 80), TSB3Photos.DefaultCrop(60, 200), "a tall image keeps its top");
                CollectionAssert.AreEqual(cli, TSB3Photos.Convert(lum, 90, 100, fit, new PhotoAdjustments(), palette));

                PhotoAdjustments brighter = new PhotoAdjustments();
                brighter.Brightness = 50;
                Assert.IsTrue(Sum(TSB3Photos.Convert(lum, 90, 100, fit, brighter, palette)) > Sum(cli));

                // a frame past the image's edges: the outside is the background gray
                PhotoAdjustments plain = new PhotoAdjustments();
                plain.AutoLevels = false;
                plain.Sharpen = 0;
                byte[,] outside = TSB3Photos.Convert(lum, 90, 100, new RectangleF(200, 200, 30, 40), plain, palette);
                foreach (byte p in outside)
                    Assert.AreEqual(TSB3Photos.NearestGray(TSB3Photos.Background, palette), p);
            }
        }

        [TestMethod]
        public void Forms_BuildAndEditorConverts()
        {
            byte[] rom = LoadTool().OutputRom;
            Color[] palette = TSB3Photos.GetPalette(rom);
            using (TSB3PhotosForm form = new TSB3PhotosForm(rom, delegate(string team, string position) { return "a NAME"; }))
                form.SelectPlayer("packers", "QB1");

            using (TSB3PhotoEditorForm editor = new TSB3PhotoEditorForm("test", TSB3Photos.GetPixels(rom, 0), palette))
            {
                Assert.IsNull(editor.Pixels, "nothing until an image is loaded");
                using (Bitmap image = new Bitmap(120, 160))
                {
                    using (Graphics g = Graphics.FromImage(image))
                        g.Clear(Color.DimGray);
                    editor.LoadImage(image, "test");
                }
                Assert.IsNotNull(editor.Pixels);
                Assert.AreEqual(32, editor.Pixels.GetLength(0));
                Assert.AreEqual(24, editor.Pixels.GetLength(1));

                // a 24x32 image is taken pixel for pixel
                using (Bitmap exact = TSB3Photos.GetPhotoImage(rom, 7))
                    editor.LoadImage(exact, "photo 7");
                CollectionAssert.AreEqual(TSB3Photos.GetPixels(rom, 7), editor.Pixels);
            }
        }

        private static int Sum(byte[,] pixels)
        {
            int ret = 0;
            foreach (byte p in pixels)
                ret += p;
            return ret;
        }

        private static TSB3Tool LoadTool()
        {
            return (TSB3Tool)TecmoToolFactory.GetToolForRom(TestRoms.LoadRom(RomFileName));
        }

        /// <summary>The ROMs are the same except inside the given photos' tiles, each of which changed.</summary>
        private static void AssertOnlyPhotosChanged(byte[] before, byte[] after, params int[] photos)
        {
            Assert.AreEqual(before.Length, after.Length);
            bool[] inPhoto = new bool[before.Length];
            foreach (int photo in photos)
            {
                int at = TSB3Photos.GetPhotoLocation(before, photo);
                bool changed = false;
                for (int i = at; i < at + TSB3Photos.PhotoBytes; i++)
                {
                    inPhoto[i] = true;
                    changed |= before[i] != after[i];
                }
                Assert.IsTrue(changed, "photo " + photo + " changed");
            }
            for (int i = 0; i < before.Length; i++)
                if (!inPhoto[i] && before[i] != after[i])
                    Assert.Fail(string.Format("byte 0x{0:X} changed outside the photos", i));
        }

        private static void RunInScratchDir(Action<string> test)
        {
            string scratchDir = Path.Combine(Path.GetTempPath(), "TSBToolPhotoTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(scratchDir);
            string originalCurrentDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = scratchDir;
                test(scratchDir);
            }
            finally
            {
                Environment.CurrentDirectory = originalCurrentDirectory;
                try { Directory.Delete(scratchDir, true); } catch (Exception) { /* best-effort cleanup */ }
            }
        }
    }
}
