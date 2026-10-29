using System;
using System.Drawing;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TSBTool;
using TSBTool2;

/// <summary>TSB3 player photos for the web UI: thin wrappers over the core's TSB3Photos (pixels only; the browser decodes and shows images).</summary>
public static partial class WebHelper
{
    static byte[] PhotoRom()
    {
        if (tool == null || tool.RomVersion != ROM_TYPE.SNES_TSB3) throw new InvalidOperationException("Photos are only available for SNES TSB3.");
        return tool.OutputRom;
    }

    static byte[] Flatten(byte[,] px)
    {
        var ret = new byte[TSB3Photos.Width * TSB3Photos.Height];
        for (int y = 0; y < TSB3Photos.Height; y++)
            for (int x = 0; x < TSB3Photos.Width; x++)
                ret[y * TSB3Photos.Width + x] = px[y, x];
        return ret;
    }

    static byte[,] Unflatten(byte[] flat)
    {
        if (flat == null || flat.Length != TSB3Photos.Width * TSB3Photos.Height) throw new ArgumentException("A photo is 24x32 = 768 pixels.");
        var ret = new byte[TSB3Photos.Height, TSB3Photos.Width];
        for (int y = 0; y < TSB3Photos.Height; y++)
            for (int x = 0; x < TSB3Photos.Width; x++)
                ret[y, x] = flat[y * TSB3Photos.Width + x];
        return ret;
    }

    /// <summary>True when the loaded ROM is SNES TSB3.</summary>
    [JSExport]
    public static bool HasPhotos() => tool != null && tool.RomVersion == ROM_TYPE.SNES_TSB3;

    /// <summary>The photos' 16 grays as "rrggbb" separated by commas.</summary>
    [JSExport]
    public static string GetPhotoPalette()
    {
        Color[] p = TSB3Photos.GetPalette(PhotoRom());
        var sb = new StringBuilder();
        for (int i = 0; i < p.Length; i++) sb.Append(i > 0 ? "," : "").Append(p[i].R.ToString("x2")).Append(p[i].G.ToString("x2")).Append(p[i].B.ToString("x2"));
        return sb.ToString();
    }

    /// <summary>A photo's 24x32 palette indexes, row by row (768 bytes).</summary>
    [JSExport]
    public static byte[] GetPhotoPixels(int photo) => Flatten(TSB3Photos.GetPixels(PhotoRom(), photo));

    /// <summary>
    /// Who each of the 85 photos belongs to, one line per photo: "team|position", "free", "default" (the silhouette everyone else gets)
    /// or "unused:id" (an id that is no roster slot).
    /// </summary>
    [JSExport]
    public static string GetPhotoOwners()
    {
        byte[] rom = PhotoRom();
        var sb = new StringBuilder();
        for (int photo = 0; photo < TSB3Photos.PhotoCount; photo++)
        {
            int id = TSB3Photos.GetPlayerId(rom, photo);
            string line, team, pos;
            if (id < 0) line = "default";
            else if (id == TSB3Photos.FreeId) line = "free";
            else if (TSB3Photos.GetSlot(id, out team, out pos)) line = team + "|" + pos;
            else line = "unused:" + id;
            sb.Append(line).Append('\n');
        }
        return sb.ToString();
    }

    /// <summary>Gives the photo to a roster slot. A player has one photo, so another photo he had is freed; returns that photo's number or -1.</summary>
    [JSExport]
    public static int AssignPhoto(int photo, string team, string position)
    {
        int id = TSB3Photos.PlayerId(team, position);
        if (id < 0) throw new ArgumentException("Unknown roster slot " + team + " " + position);
        return TSB3Photos.AssignPhoto(PhotoRom(), photo, id);
    }

    [JSExport]
    public static void FreePhoto(int photo) => TSB3Photos.AssignPhoto(PhotoRom(), photo, TSB3Photos.FreeId);

    [JSExport]
    public static void SetPhotoPixels(int photo, byte[] pixels) => TSB3Photos.SetPixels(PhotoRom(), photo, Unflatten(pixels));

    /// <summary>The largest 3:4 area of a w x h image, as x, y, width, height.</summary>
    [JSExport]
    public static double[] PhotoDefaultCrop(int w, int h)
    {
        RectangleF r = TSB3Photos.DefaultCrop(w, h);
        return new double[] { r.X, r.Y, r.Width, r.Height };
    }

    /// <summary>
    /// An image (ARGB ints, w x h) as photo pixels: the crop area scaled to 24x32, sharpened, stretched, and each pixel the nearest of the game's grays.
    /// A 24x32 image is taken pixel for pixel.
    /// </summary>
    [JSExport]
    public static byte[] ConvertPhoto(int[] argb, int w, int h, double cropX, double cropY, double cropW, double cropH,
        double brightness, double contrast, double sharpen, bool autoLevels)
    {
        Color[] palette = TSB3Photos.GetPalette(PhotoRom());
        double[] lum = TSB3Photos.GetLuminance(argb);
        if (w == TSB3Photos.Width && h == TSB3Photos.Height)
        {
            var px = new byte[TSB3Photos.Height, TSB3Photos.Width];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y, x] = TSB3Photos.NearestGray(lum[y * w + x], palette);
            return Flatten(px);
        }
        var adjust = new PhotoAdjustments { Brightness = brightness, Contrast = contrast, Sharpen = sharpen, AutoLevels = autoLevels };
        var crop = new RectangleF((float)cropX, (float)cropY, (float)cropW, (float)cropH);
        return Flatten(TSB3Photos.Convert(lum, w, h, crop, adjust, palette));
    }
}
