using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TSBTool2
{
    /// <summary>A player's name ("jim KELLY") for a roster slot, or null.</summary>
    public delegate string PlayerNameLookup(string team, string position);

    /// <summary>How a converted image is turned into the game's grays (see TSB3Photos.Convert).</summary>
    public class PhotoAdjustments
    {
        /// <summary>-100 - 100, added after the stretch (100 = about half the range).</summary>
        public double Brightness = 0;
        /// <summary>-100 - 100; 100 doubles the distance from middle gray.</summary>
        public double Contrast = 0;
        /// <summary>0 - 2, unsharp mask strength.</summary>
        public double Sharpen = 0.6;
        /// <summary>Stretch the darkest and lightest 2% of pixels to the ends of the range.</summary>
        public bool AutoLevels = true;

        public PhotoAdjustments Clone()
        {
            return (PhotoAdjustments)MemberwiseClone();
        }
    }

    /// <summary>
    /// The player photos of SNES TSB3 (shown in game, not in the Team Data menus).
    ///
    /// The game looks a player up by his player id (team * 37 + position, in TSB3Tool's team and
    /// TSB2Tool's position order) in a list of 84 ids at PlayerIdTable ($86:E844); a player at entry i
    /// gets photo i: the address at AddressTable ($86:E79A) entry i, in bank $A0 (file 0x100000).
    /// A player not in the list gets entry 84's photo, a silhouette (code at $86:E736).
    /// Any entry can hold any slot's id; the first match wins. FreeId matches no player.
    /// Each photo is 24x32 pixels, 3x4 SNES 4bpp tiles (0x180 bytes), all sharing one palette of
    /// 16 grays at PaletteLocation; color 0 is transparent and no photo uses it.
    /// </summary>
    public static class TSB3Photos
    {
        public const int PlayerIdTable = 0x36844;
        public const int AddressTable = 0x3679A;
        public const int PaletteLocation = 0x36716;
        public const int PhotoBank = 0x100000;

        /// <summary>Photos with a player id (0-83); photo 84 is the silhouette everyone else gets.</summary>
        public const int PlayerPhotoCount = 84;
        public const int PhotoCount = 85;
        public const int DefaultPhoto = 84;

        /// <summary>The id of a photo no player has (the game compares the ids as 16 bits; players are 0 - 0x4A3).</summary>
        public const int FreeId = 0xFFFF;

        public const int Width = 24, Height = 32, PhotoBytes = 0x180;

        public static string FileName(int photo)
        {
            return string.Format("0x{0:X2}.png", photo);
        }

        /// <summary>File offset of the photo's tiles.</summary>
        public static int GetPhotoLocation(byte[] rom, int photo)
        {
            int address = rom[AddressTable + 2 * photo] | rom[AddressTable + 2 * photo + 1] << 8;
            return PhotoBank + address - 0x8000;
        }

        #region Player slots

        /// <summary>The player id at the photo's list entry (FreeId when no player has it), or -1 for the silhouette.</summary>
        public static int GetPlayerId(byte[] rom, int photo)
        {
            if (photo >= PlayerPhotoCount)
                return -1;
            return rom[PlayerIdTable + 2 * photo] | rom[PlayerIdTable + 2 * photo + 1] << 8;
        }

        public static void SetPlayerId(byte[] rom, int photo, int id)
        {
            if (photo < 0 || photo >= PlayerPhotoCount)
                throw new ArgumentOutOfRangeException("photo", "only photos 0x00 - 0x53 belong to a player");
            rom[PlayerIdTable + 2 * photo] = (byte)id;
            rom[PlayerIdTable + 2 * photo + 1] = (byte)(id >> 8);
        }

        /// <summary>The player id of a roster slot ("packers", "QB1"), or -1.</summary>
        public static int PlayerId(string team, string position)
        {
            int t = TSB2Tool.teams.IndexOf(team), p = TSB2Tool.positionNames.IndexOf(position);
            if (t < 0 || p < 0)
                return -1;
            return t * TSB2Tool.positionNames.Count + p;
        }

        /// <summary>The roster slot of a player id; false for FreeId or an id that is no slot.</summary>
        public static bool GetSlot(int id, out string team, out string position)
        {
            team = position = null;
            int positions = TSB2Tool.positionNames.Count;
            if (id < 0 || id / positions >= TSB2Tool.teams.Count)
                return false;
            team = TSB2Tool.teams[id / positions];
            position = TSB2Tool.positionNames[id % positions];
            return true;
        }

        /// <summary>The photo the game shows for a player id: the first entry with it, or -1 (the silhouette).</summary>
        public static int FindPhoto(byte[] rom, int id)
        {
            for (int photo = 0; photo < PlayerPhotoCount; photo++)
                if (GetPlayerId(rom, photo) == id)
                    return photo;
            return -1;
        }

        /// <summary>
        /// Gives the photo to the player id (FreeId frees it). A player has one photo, so another entry
        /// with the same id is freed; returns that entry, or -1.
        /// </summary>
        public static int AssignPhoto(byte[] rom, int photo, int id)
        {
            int taken = -1;
            if (id != FreeId)
            {
                for (int other = 0; other < PlayerPhotoCount; other++)
                {
                    if (other != photo && GetPlayerId(rom, other) == id)
                    {
                        SetPlayerId(rom, other, FreeId);
                        if (taken < 0)
                            taken = other;
                    }
                }
            }
            SetPlayerId(rom, photo, id);
            return taken;
        }

        /// <summary>Photos 0 - 83 that a player has.</summary>
        public static int UsedCount(byte[] rom)
        {
            int ret = 0;
            string team, position;
            for (int photo = 0; photo < PlayerPhotoCount; photo++)
                if (GetSlot(GetPlayerId(rom, photo), out team, out position))
                    ret++;
            return ret;
        }

        /// <summary>
        /// "bills QB1 jim KELLY" for a photo's player, "no photo (everyone else)" for the silhouette,
        /// "free" when no player has it, "unused (id 0x...)" for an id that is no roster slot.
        /// 'names' gives the player's name (null: none shown).
        /// </summary>
        public static string Describe(byte[] rom, int photo, PlayerNameLookup names)
        {
            int id = GetPlayerId(rom, photo);
            if (id < 0)
                return "no photo (everyone else)";
            if (id == FreeId)
                return "free";
            string team, position;
            if (!GetSlot(id, out team, out position))
                return string.Format("unused (id 0x{0:X3})", id);
            string name = names == null ? null : names(team, position);
            return string.IsNullOrEmpty(name) ? team + " " + position : string.Format("{0} {1} {2}", team, position, name.Trim());
        }

        /// <summary>Describe, with the names in the ROM.</summary>
        public static string Describe(TSB3Tool tool, int photo)
        {
            return Describe(tool.OutputRom, photo, RomNames(tool));
        }

        /// <summary>Player names as the ROM has them.</summary>
        public static PlayerNameLookup RomNames(TSB3Tool tool)
        {
            return delegate(string team, string position)
            {
                byte number;
                return tool.GetPlayerName(1, team, position, out number);
            };
        }

        #endregion

        #region Pixels

        public static Color[] GetPalette(byte[] rom)
        {
            Color[] ret = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                int v = rom[PaletteLocation + 2 * i] | rom[PaletteLocation + 2 * i + 1] << 8;
                ret[i] = Color.FromArgb(Scale5(v & 31), Scale5(v >> 5 & 31), Scale5(v >> 10 & 31));
            }
            return ret;
        }

        private static int Scale5(int c)
        {
            return c * 255 / 31;
        }

        /// <summary>The photo's palette indexes, [y, x].</summary>
        public static byte[,] GetPixels(byte[] rom, int photo)
        {
            int location = GetPhotoLocation(rom, photo);
            byte[,] ret = new byte[Height, Width];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int tile = location + ((y / 8) * (Width / 8) + x / 8) * 32;
                    int row = y % 8, bit = 7 - x % 8;
                    ret[y, x] = (byte)(
                        (rom[tile + 2 * row] >> bit & 1) |
                        (rom[tile + 2 * row + 1] >> bit & 1) << 1 |
                        (rom[tile + 16 + 2 * row] >> bit & 1) << 2 |
                        (rom[tile + 17 + 2 * row] >> bit & 1) << 3);
                }
            }
            return ret;
        }

        /// <summary>Writes palette indexes [y, x] (1-15) as the photo's tiles.</summary>
        public static void SetPixels(byte[] rom, int photo, byte[,] pixels)
        {
            int location = GetPhotoLocation(rom, photo);
            for (int i = 0; i < PhotoBytes; i++)
                rom[location + i] = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int tile = location + ((y / 8) * (Width / 8) + x / 8) * 32;
                    int row = y % 8, bit = 7 - x % 8, c = pixels[y, x];
                    rom[tile + 2 * row] |= (byte)((c & 1) << bit);
                    rom[tile + 2 * row + 1] |= (byte)((c >> 1 & 1) << bit);
                    rom[tile + 16 + 2 * row] |= (byte)((c >> 2 & 1) << bit);
                    rom[tile + 17 + 2 * row] |= (byte)((c >> 3 & 1) << bit);
                }
            }
        }

        public static Bitmap PixelsToImage(byte[,] pixels, Color[] palette)
        {
            Bitmap ret = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    ret.SetPixel(x, y, palette[pixels[y, x]]);
            return ret;
        }

        public static Bitmap GetPhotoImage(byte[] rom, int photo)
        {
            return PixelsToImage(GetPixels(rom, photo), GetPalette(rom));
        }

        public static bool SamePixels(byte[,] a, byte[,] b)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (a[y, x] != b[y, x])
                        return false;
            return true;
        }

        #endregion

        #region Converting images

        /// <summary>Gray the transparent parts of an image (and a crop beyond its edges) become.</summary>
        public const double Background = 200;

        /// <summary>
        /// An image as photo pixels (palette indexes 1-15, [y, x]). A 24x32 image is taken pixel for pixel,
        /// each pixel the nearest of the game's grays ('how' says how many weren't one already). Any other
        /// size is converted with DefaultCrop and the default PhotoAdjustments.
        /// </summary>
        public static byte[,] ImageToPixels(Bitmap image, Color[] palette, out string how)
        {
            int w = image.Width, h = image.Height;
            if (w == Width && h == Height)
            {
                int[] argb = GetArgb(image);
                double[] lum = GetLuminance(argb);
                byte[,] ret = new byte[Height, Width];
                int changed = 0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        ret[y, x] = NearestGray(lum[y * w + x], palette);
                        if (argb[y * w + x] != palette[ret[y, x]].ToArgb())
                            changed++;
                    }
                }
                how = changed == 0 ? "" : string.Format("{0} pixels set to the nearest of the game's grays", changed);
                return ret;
            }
            how = string.Format("converted from {0}x{1}", w, h);
            return Convert(GetLuminance(GetArgb(image)), w, h, DefaultCrop(w, h), new PhotoAdjustments(), palette);
        }

        /// <summary>The largest 3:4 area of a w x h image: centered across, at the top of a tall image.</summary>
        public static RectangleF DefaultCrop(int w, int h)
        {
            if (w * Height > h * Width)
            {
                int cropW = Math.Max(1, (int)Math.Round(h * (double)Width / Height));
                return new RectangleF((w - cropW) / 2, 0, cropW, h);
            }
            return new RectangleF(0, 0, w, Math.Max(1, (int)Math.Round(w * (double)Height / Width)));
        }

        /// <summary>
        /// Photo pixels from an image's brightness (GetLuminance; w x h): the 'crop' area (image pixels; it
        /// may reach past the edges, which are Background) scaled to 24x32 by averaging, sharpened,
        /// stretched (AutoLevels), contrast and brightness applied, each pixel the nearest of the game's grays.
        /// </summary>
        public static byte[,] Convert(double[] lum, int w, int h, RectangleF crop, PhotoAdjustments adjust, Color[] palette)
        {
            // scale by averaging the area under each photo pixel
            double[] v = new double[Width * Height];
            double sx = crop.Width / (double)Width, sy = crop.Height / (double)Height;
            for (int ty = 0; ty < Height; ty++)
            {
                double y0 = crop.Y + ty * sy, y1 = y0 + sy;
                for (int tx = 0; tx < Width; tx++)
                {
                    double x0 = crop.X + tx * sx, x1 = x0 + sx, sum = 0, weight = 0;
                    for (int y = (int)Math.Floor(y0); y < (int)Math.Ceiling(y1); y++)
                    {
                        double wy = Math.Min(y1, y + 1) - Math.Max(y0, y);
                        for (int x = (int)Math.Floor(x0); x < (int)Math.Ceiling(x1); x++)
                        {
                            double wxy = wy * (Math.Min(x1, x + 1) - Math.Max(x0, x));
                            bool inside = x >= 0 && y >= 0 && x < w && y < h;
                            sum += wxy * (inside ? lum[y * w + x] : Background);
                            weight += wxy;
                        }
                    }
                    v[ty * Width + tx] = weight > 0 ? sum / weight : Background;
                }
            }

            // sharpen (unsharp mask, 3x3)
            double[] sharp = new double[v.Length];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double sum = 0;
                    int n = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int xx = x + dx, yy = y + dy;
                            if (xx < 0 || yy < 0 || xx >= Width || yy >= Height)
                                continue;
                            sum += v[yy * Width + xx];
                            n++;
                        }
                    }
                    double p = v[y * Width + x];
                    sharp[y * Width + x] = Clamp(p + adjust.Sharpen * (p - sum / n));
                }
            }

            // stretch: the darkest and lightest 2% of pixels go to the ends of the range
            double lo = 0, hi = 255;
            if (adjust.AutoLevels)
            {
                double[] sorted = (double[])sharp.Clone();
                Array.Sort(sorted);
                lo = sorted[(int)(sorted.Length * 0.02)];
                hi = sorted[(int)(sorted.Length * 0.98)];
            }
            byte[,] ret = new byte[Height, Width];
            for (int i = 0; i < sharp.Length; i++)
            {
                double s = Clamp((sharp[i] - lo) / Math.Max(1, hi - lo) * 255);
                s = (s - 128) * (1 + adjust.Contrast / 100) + 128 + adjust.Brightness * 1.28;
                ret[i / Width, i % Width] = NearestGray(Clamp(s), palette);
            }
            return ret;
        }

        private static double Clamp(double v)
        {
            return Math.Max(0, Math.Min(255, v));
        }

        /// <summary>The image's pixels as ARGB, row by row.</summary>
        public static int[] GetArgb(Image image)
        {
            int w = image.Width, h = image.Height;
            int[] ret = new int[w * h];
            using (Bitmap copy = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(copy))
                    g.DrawImage(image, new Rectangle(0, 0, w, h), 0, 0, w, h, GraphicsUnit.Pixel);
                BitmapData data = copy.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < h; y++)
                        Marshal.Copy(new IntPtr(data.Scan0.ToInt64() + (long)y * data.Stride), ret, y * w, w);
                }
                finally
                {
                    copy.UnlockBits(data);
                }
            }
            return ret;
        }

        /// <summary>Each pixel's brightness (0-255), transparency blended over Background.</summary>
        public static double[] GetLuminance(int[] argb)
        {
            double[] ret = new double[argb.Length];
            for (int i = 0; i < argb.Length; i++)
            {
                int c = argb[i];
                double a = (c >> 24 & 0xFF) / 255.0;
                double l = 0.299 * (c >> 16 & 0xFF) + 0.587 * (c >> 8 & 0xFF) + 0.114 * (c & 0xFF);
                ret[i] = l * a + Background * (1 - a);
            }
            return ret;
        }

        /// <summary>The palette index (1-15; 0 is transparent) of the gray nearest 'value'.</summary>
        public static byte NearestGray(double value, Color[] palette)
        {
            int best = 1;
            for (int i = 2; i < 16; i++)
            {
                if (Math.Abs(palette[i].R - value) < Math.Abs(palette[best].R - value))
                    best = i;
            }
            return (byte)best;
        }

        #endregion

        #region Files

        /// <summary>The photo number of a file named 0x00 - 0x54 (.png, .jpg, .jpeg, .bmp or .gif), else -1.</summary>
        public static int PhotoNumber(string fileName)
        {
            if (!IsImageFile(fileName))
                return -1;
            string name = Path.GetFileNameWithoutExtension(fileName);
            if (name.Length != 4 || !name.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return -1;
            int n;
            if (!int.TryParse(name.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out n) || n >= PhotoCount)
                return -1;
            return n;
        }

        public static bool IsImageFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif";
        }

        /// <summary>
        /// Writes every photo to 'folder' as 0x00.png - 0x54.png (24x32, the game's grays; 0x54 is the
        /// silhouette). Returns one line per photo: file name and who has it.
        /// </summary>
        public static string ExportPhotos(byte[] rom, string folder, PlayerNameLookup names)
        {
            Directory.CreateDirectory(folder);
            StringBuilder sb = new StringBuilder();
            for (int photo = 0; photo < PhotoCount; photo++)
            {
                string name = FileName(photo);
                using (Bitmap image = GetPhotoImage(rom, photo))
                    image.Save(Path.Combine(folder, name), ImageFormat.Png);
                sb.AppendLine(string.Format("{0}  {1}", name, Describe(rom, photo, names)));
            }
            return sb.ToString();
        }

        public static string ExportPhotos(TSB3Tool tool, string folder)
        {
            return ExportPhotos(tool.OutputRom, folder, RomNames(tool));
        }

        /// <summary>
        /// Replaces the photos that have a file in 'folder' (0x00 - 0x54, see PhotoNumber); the others stay
        /// as they are. Returns one line per file: the photo, its player and what was done ("ERROR: ..." when
        /// the file was skipped). 'changed' is the number of photos written.
        /// </summary>
        public static string ImportPhotos(byte[] rom, string folder, PlayerNameLookup names, out int changed)
        {
            changed = 0;
            StringBuilder sb = new StringBuilder();
            Dictionary<int, List<string>> files = new Dictionary<int, List<string>>();
            string[] all = Directory.GetFiles(folder);
            Array.Sort(all, StringComparer.OrdinalIgnoreCase);
            foreach (string file in all)
            {
                int photo = PhotoNumber(file);
                if (photo >= 0)
                {
                    if (!files.ContainsKey(photo))
                        files[photo] = new List<string>();
                    files[photo].Add(file);
                }
                else if (IsImageFile(file))
                {
                    sb.AppendLine(string.Format("{0}  ignored: not a photo name (0x00 - 0x{1:X2})", Path.GetFileName(file), PhotoCount - 1));
                }
            }

            Color[] palette = GetPalette(rom);
            for (int photo = 0; photo < PhotoCount; photo++)
            {
                if (!files.ContainsKey(photo))
                    continue;
                List<string> list = files[photo];
                string name = Path.GetFileName(list[0]);
                if (list.Count > 1)
                {
                    List<string> fileNames = new List<string>();
                    foreach (string f in list)
                        fileNames.Add(Path.GetFileName(f));
                    sb.AppendLine(string.Format("{0}  ERROR: more than one file for photo 0x{1:X2} ({2}); left as it was",
                        name, photo, string.Join(", ", fileNames.ToArray())));
                    continue;
                }
                byte[,] pixels;
                string how;
                try
                {
                    using (Bitmap image = new Bitmap(list[0]))
                        pixels = ImageToPixels(image, palette, out how);
                }
                catch (Exception e)
                {
                    sb.AppendLine(string.Format("{0}  ERROR: can't read the image ({1}); left as it was", name, e.Message));
                    continue;
                }
                string who = Describe(rom, photo, names);
                if (SamePixels(pixels, GetPixels(rom, photo)))
                {
                    sb.AppendLine(string.Format("{0}  {1}: unchanged", name, who));
                    continue;
                }
                SetPixels(rom, photo, pixels);
                changed++;
                sb.AppendLine(string.Format("{0}  {1}: written{2}", name, who, how.Length > 0 ? " (" + how + ")" : ""));
            }
            return sb.ToString();
        }

        public static string ImportPhotos(TSB3Tool tool, string folder, out int changed)
        {
            return ImportPhotos(tool.OutputRom, folder, RomNames(tool), out changed);
        }

        #endregion
    }
}
