using System;
using System.Drawing;
using System.Windows.Forms;

namespace TSBTool
{
    /// <summary>
    /// A color picker constrained to exactly what the SNES can display: 32 levels per channel
    /// (5-bit RGB555), not the full 24-bit space a standard ColorDialog offers. Every value this
    /// dialog can produce is already a real, exact SNES color -- no rounding happens on OK, unlike
    /// picking an arbitrary 24-bit color and quantizing it afterward.
    /// </summary>
    public partial class SnesColorPickerForm : Form
    {
        private const int GridCols = 16;
        private const int GridRows = 16;
        private const int CellSize = 16; // 256 / 16

        private static readonly Color[] mGridColors = BuildGridColors();

        // Guards against the grid-click/property-set code re-entering ColorTrackBar_ValueChanged
        // while it's still in the middle of setting all three sliders (each .Value assignment fires
        // ValueChanged on its own, which would otherwise repaint/recompute three times per pick using
        // a still-partially-updated R/G/B triple).
        private bool mUpdating;

        public SnesColorPickerForm()
        {
            InitializeComponent();
            UpdateFromSliders();
        }

        /// <summary>
        /// The picked color, as the same 4-hex-digit raw SNES byte pair (file order) used everywhere
        /// else in this project -- SetColorUI, ColorToSnesHex, -colorsdebug, etc.
        /// </summary>
        public string SnesColor
        {
            get
            {
                return string.Format("{0:x2}{1:x2}", EncodeByte0(), EncodeByte1());
            }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Length != 4)
                    return;
                byte b0 = Convert.ToByte(value.Substring(0, 2), 16);
                byte b1 = Convert.ToByte(value.Substring(2, 2), 16);
                int v = b0 | (b1 << 8);
                mUpdating = true;
                mRedTrackBar.Value = v & 0x1F;
                mGreenTrackBar.Value = (v >> 5) & 0x1F;
                mBlueTrackBar.Value = (v >> 10) & 0x1F;
                mUpdating = false;
                UpdateFromSliders();
            }
        }

        private int EncodeByte0()
        {
            int v = mRedTrackBar.Value | (mGreenTrackBar.Value << 5) | (mBlueTrackBar.Value << 10);
            return v & 0xFF;
        }

        private int EncodeByte1()
        {
            int v = mRedTrackBar.Value | (mGreenTrackBar.Value << 5) | (mBlueTrackBar.Value << 10);
            return (v >> 8) & 0xFF;
        }

        private static Color SnesIdxToColor(int r5, int g5, int b5)
        {
            // Matches the idx*8 convention used throughout Core/SNES_TecmoTool.cs and every other
            // SNES color tool in this project (not the "mathematically proper" idx*255/31).
            return Color.FromArgb(r5 * 8, g5 * 8, b5 * 8);
        }

        private void ColorTrackBar_ValueChanged(object sender, EventArgs e)
        {
            if (!mUpdating)
                UpdateFromSliders();
        }

        private void UpdateFromSliders()
        {
            int r = mRedTrackBar.Value, g = mGreenTrackBar.Value, b = mBlueTrackBar.Value;
            Color c = SnesIdxToColor(r, g, b);

            mRedValueLabel.Text = r.ToString();
            mGreenValueLabel.Text = g.ToString();
            mBlueValueLabel.Text = b.ToString();

            mPreviewPanel.BackColor = c;
            mRgb555Label.Text = string.Format("RGB 555: ({0}, {1}, {2})", r, g, b);
            mHexLabel.Text = string.Format("HEX: ${0}", SnesColor);
            mRgbLabel.Text = string.Format("RGB: #{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        private void mColorGridPanel_Paint(object sender, PaintEventArgs e)
        {
            for (int row = 0; row < GridRows; row++)
            {
                for (int col = 0; col < GridCols; col++)
                {
                    Color c = mGridColors[row * GridCols + col];
                    using (Brush brush = new SolidBrush(c))
                    {
                        e.Graphics.FillRectangle(brush, col * CellSize, row * CellSize, CellSize, CellSize);
                    }
                }
            }
        }

        private void mColorGridPanel_MouseDown(object sender, MouseEventArgs e)
        {
            int col = e.X / CellSize;
            int row = e.Y / CellSize;
            if (col < 0 || col >= GridCols || row < 0 || row >= GridRows)
                return;

            Color c = mGridColors[row * GridCols + col];
            mUpdating = true;
            mRedTrackBar.Value = (int)Math.Round(c.R / 8.0);
            mGreenTrackBar.Value = (int)Math.Round(c.G / 8.0);
            mBlueTrackBar.Value = (int)Math.Round(c.B / 8.0);
            mUpdating = false;
            UpdateFromSliders();
        }

        /// <summary>
        /// Builds the 256 quick-pick swatches: column 0 is a 16-step grayscale ramp, columns 1-15 are
        /// 15 hues evenly spaced around the color wheel, each varying from bright (top row) to dark
        /// (bottom row) at full saturation. Every value is generated then snapped through the same
        /// idx*8 rounding the rest of this form uses, so every swatch is already an exact, real SNES
        /// color -- picking one is never a "close enough" approximation.
        /// </summary>
        private static Color[] BuildGridColors()
        {
            Color[] colors = new Color[GridCols * GridRows];
            for (int row = 0; row < GridRows; row++)
            {
                for (int col = 0; col < GridCols; col++)
                {
                    Color raw;
                    if (col == 0)
                    {
                        int level = (int)Math.Round(row * (255.0 / (GridRows - 1)));
                        raw = Color.FromArgb(level, level, level);
                    }
                    else
                    {
                        double hue = (col - 1) * (360.0 / (GridCols - 1));
                        double value = 1.0 - (row / (double)(GridRows - 1)) * 0.85; // never fully black
                        raw = HsvToColor(hue, 1.0, value);
                    }
                    int r5 = Math.Min(31, (int)Math.Round(raw.R / 8.0));
                    int g5 = Math.Min(31, (int)Math.Round(raw.G / 8.0));
                    int b5 = Math.Min(31, (int)Math.Round(raw.B / 8.0));
                    colors[row * GridCols + col] = SnesIdxToColor(r5, g5, b5);
                }
            }
            return colors;
        }

        private static Color HsvToColor(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
            double m = v - c;
            double r = 0, g = 0, b = 0;
            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }
            return Color.FromArgb(
                (int)Math.Round((r + m) * 255),
                (int)Math.Round((g + m) * 255),
                (int)Math.Round((b + m) * 255));
        }
    }
}
