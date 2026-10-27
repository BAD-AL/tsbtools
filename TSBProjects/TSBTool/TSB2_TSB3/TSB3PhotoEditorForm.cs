using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace TSBTool2
{
    /// <summary>
    /// Changes one TSB3 photo: load an image (Browse, Paste or drop it on the form), move and zoom the 3:4
    /// frame over it, adjust brightness, contrast and sharpening, and see the result in the game's 24x32
    /// grays next to the photo it replaces. A 24x32 image is taken pixel for pixel. Pixels is the result
    /// (null when no image was loaded).
    /// </summary>
    public class TSB3PhotoEditorForm : Form
    {
        // Kept for the next photo in this session, so a set of same-style headshots needs no re-cropping.
        private static PhotoAdjustments sAdjust = new PhotoAdjustments();
        private static float sZoom = 1;
        private static float sOffsetX, sOffsetY; // crop center offset from the default crop's, in image widths/heights
        private static string sFolder;

        private readonly Color[] mPalette;
        private readonly byte[,] mNow;
        private byte[,] mNew;
        private PhotoAdjustments mAdjust;

        private Bitmap mImage;
        private double[] mLum;
        private bool mExact; // a 24x32 image: no crop or adjustments
        private bool mUpdating;

        private CropPanel mCrop;
        private PixelView mNowView, mNewView, mNowSmall, mNewSmall;
        private TrackBar mZoomBar, mBrightBar, mContrastBar, mSharpenBar;
        private CheckBox mLevelsBox;
        private Label mSourceLabel, mExactLabel;
        private Button mOkButton;

        public TSB3PhotoEditorForm(string title, byte[,] current, Color[] palette)
        {
            mPalette = palette;
            mNow = current;
            mAdjust = sAdjust.Clone();
            BuildUI();
            Text = title;
            UpdateControls();
        }

        /// <summary>The new photo (palette indexes [y, x]); null until an image is loaded.</summary>
        public byte[,] Pixels
        {
            get { return mNew; }
        }

        #region Layout

        private void BuildUI()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(760, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            AllowDrop = true;
            KeyPreview = true;

            mCrop = new CropPanel();
            mCrop.Location = new Point(10, 10);
            mCrop.Size = new Size(420, 420);
            mCrop.CropChanged += delegate { CropMoved(); };
            mCrop.DragEnter += new DragEventHandler(OnFileDragEnter);
            mCrop.DragDrop += new DragEventHandler(OnFileDragDrop);
            mCrop.AllowDrop = true;

            mSourceLabel = new Label();
            mSourceLabel.Location = new Point(10, 434);
            mSourceLabel.Size = new Size(420, 16);
            mSourceLabel.ForeColor = SystemColors.GrayText;

            Button browse = MakeButton("&Browse...", 10, 454, 80);
            browse.Click += new EventHandler(Browse);
            Button paste = MakeButton("&Paste", 96, 454, 60);
            paste.Click += new EventHandler(Paste);
            Button fit = MakeButton("&Fit", 162, 454, 50);
            fit.Click += delegate
            {
                if (mImage == null || mExact)
                    return;
                SetZoom(1);
                mCrop.Crop = CenteredCrop(1, 0, 0);
                CropMoved();
            };
            Label zoomLabel = MakeLabel("Zoom", 222, 458);
            mZoomBar = MakeBar(260, 450, 170, 50, 400, 100);
            mZoomBar.TickFrequency = 50;
            mZoomBar.ValueChanged += delegate { if (!mUpdating) ZoomTo(mZoomBar.Value / 100f); };

            // previews
            Label nowLabel = MakeLabel("Now", 450, 10);
            Label newLabel = MakeLabel("New", 610, 10);
            mNowView = new PixelView(mPalette, 4);
            mNowView.Location = new Point(450, 28);
            mNowView.Pixels = mNow;
            mNewView = new PixelView(mPalette, 4);
            mNewView.Location = new Point(610, 28);
            mNowSmall = new PixelView(mPalette, 1);
            mNowSmall.Location = new Point(450, 164);
            mNowSmall.Pixels = mNow;
            mNewSmall = new PixelView(mPalette, 1);
            mNewSmall.Location = new Point(610, 164);
            Label actual = MakeLabel("actual size", 482, 176);
            actual.ForeColor = SystemColors.GrayText;
            Label actual2 = MakeLabel("actual size", 642, 176);
            actual2.ForeColor = SystemColors.GrayText;

            mExactLabel = new Label();
            mExactLabel.Location = new Point(450, 204);
            mExactLabel.Size = new Size(300, 30);
            mExactLabel.ForeColor = SystemColors.GrayText;

            // adjustments
            int ay = 240;
            MakeLabel("Brightness", 450, ay + 4);
            mBrightBar = MakeBar(530, ay, 220, -50, 50, 0);
            MakeLabel("Contrast", 450, ay + 44);
            mContrastBar = MakeBar(530, ay + 40, 220, -50, 100, 0);
            MakeLabel("Sharpen", 450, ay + 84);
            mSharpenBar = MakeBar(530, ay + 80, 220, 0, 20, 6);
            mLevelsBox = new CheckBox();
            mLevelsBox.Text = "Stretch to the full range of grays";
            mLevelsBox.Location = new Point(450, ay + 124);
            mLevelsBox.Size = new Size(260, 20);
            mLevelsBox.CheckedChanged += delegate { AdjustmentsChanged(); };
            Button reset = MakeButton("&Reset adjustments", 450, ay + 150, 130);
            reset.Click += delegate { mAdjust = new PhotoAdjustments(); UpdateControls(); Render(); };
            foreach (TrackBar bar in new TrackBar[] { mBrightBar, mContrastBar, mSharpenBar })
                bar.ValueChanged += delegate { AdjustmentsChanged(); };

            Label hint = new Label();
            hint.Location = new Point(450, 430);
            hint.Size = new Size(300, 44);
            hint.ForeColor = SystemColors.GrayText;
            hint.Text = "Drag the frame to move it, mouse wheel to zoom. Drop an image anywhere on this window.";

            mOkButton = MakeButton("OK", 590, 486, 75);
            mOkButton.DialogResult = DialogResult.OK;
            Button cancel = MakeButton("Cancel", 675, 486, 75);
            cancel.DialogResult = DialogResult.Cancel;
            AcceptButton = mOkButton;
            CancelButton = cancel;

            Controls.AddRange(new Control[] { mCrop, mSourceLabel, browse, paste, fit, zoomLabel, mZoomBar,
                nowLabel, newLabel, mNowView, mNewView, mNowSmall, mNewSmall, actual, actual2, mExactLabel,
                mBrightBar, mContrastBar, mSharpenBar, mLevelsBox, reset, hint, mOkButton, cancel });
            DragEnter += new DragEventHandler(OnFileDragEnter);
            DragDrop += new DragEventHandler(OnFileDragDrop);
            KeyDown += new KeyEventHandler(OnKey);
        }

        private Label MakeLabel(string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Location = new Point(x, y);
            Controls.Add(l);
            return l;
        }

        private static Button MakeButton(string text, int x, int y, int width)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(width, 24);
            return b;
        }

        private static TrackBar MakeBar(int x, int y, int width, int min, int max, int value)
        {
            TrackBar t = new TrackBar();
            t.Location = new Point(x, y);
            t.Size = new Size(width, 40);
            t.Minimum = min;
            t.Maximum = max;
            t.Value = value;
            t.TickStyle = TickStyle.None;
            t.SmallChange = 1;
            t.LargeChange = Math.Max(1, (max - min) / 10);
            return t;
        }

        #endregion

        #region Loading images

        public void LoadImageFile(string file)
        {
            try
            {
                Image image;
                using (FileStream fs = File.OpenRead(file))
                using (Image read = Image.FromStream(fs))
                    image = new Bitmap(read); // a copy, so the file isn't kept open
                sFolder = Path.GetDirectoryName(file);
                LoadImage(image, Path.GetFileName(file));
            }
            catch (Exception e)
            {
                MessageBox.Show(this, string.Format("Can't read '{0}':\r\n{1}", file, e.Message), "Photo",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadImage(Image image, string source)
        {
            if (mImage != null)
                mImage.Dispose();
            mImage = new Bitmap(image);
            int w = mImage.Width, h = mImage.Height;
            mLum = TSB3Photos.GetLuminance(TSB3Photos.GetArgb(mImage));
            mExact = w == TSB3Photos.Width && h == TSB3Photos.Height;
            mSourceLabel.Text = string.Format("{0}  ({1}x{2})", source, w, h);
            mCrop.Image = mImage;
            if (mExact)
            {
                mCrop.Crop = new RectangleF(0, 0, w, h);
                string how;
                mNew = TSB3Photos.ImageToPixels(mImage, mPalette, out how);
                mExactLabel.Text = "A 24x32 image is used pixel for pixel" + (how.Length > 0 ? " (" + how + ")." : ".");
            }
            else
            {
                mExactLabel.Text = "";
                SetZoom(sZoom);
                mCrop.Crop = CenteredCrop(sZoom, sOffsetX, sOffsetY);
            }
            UpdateControls();
            Render();
        }

        private void Browse(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*";
                if (sFolder != null && Directory.Exists(sFolder))
                    dlg.InitialDirectory = sFolder;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadImageFile(dlg.FileName);
            }
        }

        private void Paste(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                using (Image image = Clipboard.GetImage())
                    LoadImage(image, "pasted image");
            }
            else if (Clipboard.ContainsFileDropList())
            {
                StringCollection files = Clipboard.GetFileDropList();
                if (files.Count > 0 && TSB3Photos.IsImageFile(files[0]))
                    LoadImageFile(files[0]);
            }
            else
            {
                MessageBox.Show(this, "There's no image on the clipboard.", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnKey(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                Paste(sender, e);
                e.Handled = true;
            }
        }

        private static string DroppedImage(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return null;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            return files != null && files.Length > 0 && TSB3Photos.IsImageFile(files[0]) ? files[0] : null;
        }

        private void OnFileDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DroppedImage(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void OnFileDragDrop(object sender, DragEventArgs e)
        {
            string file = DroppedImage(e);
            if (file != null)
                LoadImageFile(file);
        }

        #endregion

        #region Crop and adjustments

        /// <summary>The default crop zoomed by 'zoom' (2 = half the size), its center moved by the offsets (image widths/heights).</summary>
        private RectangleF CenteredCrop(float zoom, float offsetX, float offsetY)
        {
            RectangleF fit = TSB3Photos.DefaultCrop(mImage.Width, mImage.Height);
            float w = fit.Width / zoom, h = fit.Height / zoom;
            float cx = fit.X + fit.Width / 2 + offsetX * mImage.Width, cy = fit.Y + fit.Height / 2 + offsetY * mImage.Height;
            return new RectangleF(cx - w / 2, cy - h / 2, w, h);
        }

        private float CurrentZoom()
        {
            return TSB3Photos.DefaultCrop(mImage.Width, mImage.Height).Width / mCrop.Crop.Width;
        }

        private void SetZoom(float zoom)
        {
            mUpdating = true;
            mZoomBar.Value = Math.Max(mZoomBar.Minimum, Math.Min(mZoomBar.Maximum, (int)Math.Round(zoom * 100)));
            mUpdating = false;
        }

        /// <summary>Zoom keeping the frame's center.</summary>
        private void ZoomTo(float zoom)
        {
            if (mImage == null || mExact)
                return;
            RectangleF fit = TSB3Photos.DefaultCrop(mImage.Width, mImage.Height), c = mCrop.Crop;
            float w = fit.Width / zoom, h = fit.Height / zoom;
            mCrop.Crop = new RectangleF(c.X + c.Width / 2 - w / 2, c.Y + c.Height / 2 - h / 2, w, h);
            CropMoved();
        }

        private void CropMoved()
        {
            if (mImage == null || mExact)
                return;
            float zoom = CurrentZoom();
            SetZoom(zoom);
            RectangleF fit = TSB3Photos.DefaultCrop(mImage.Width, mImage.Height), c = mCrop.Crop;
            sZoom = zoom;
            sOffsetX = (c.X + c.Width / 2 - (fit.X + fit.Width / 2)) / mImage.Width;
            sOffsetY = (c.Y + c.Height / 2 - (fit.Y + fit.Height / 2)) / mImage.Height;
            Render();
        }

        private void AdjustmentsChanged()
        {
            if (mUpdating)
                return;
            mAdjust.Brightness = mBrightBar.Value * 2;
            mAdjust.Contrast = mContrastBar.Value;
            mAdjust.Sharpen = mSharpenBar.Value / 10.0;
            mAdjust.AutoLevels = mLevelsBox.Checked;
            sAdjust = mAdjust.Clone();
            Render();
        }

        private void UpdateControls()
        {
            mUpdating = true;
            mBrightBar.Value = Math.Max(mBrightBar.Minimum, Math.Min(mBrightBar.Maximum, (int)Math.Round(mAdjust.Brightness / 2)));
            mContrastBar.Value = Math.Max(mContrastBar.Minimum, Math.Min(mContrastBar.Maximum, (int)Math.Round(mAdjust.Contrast)));
            mSharpenBar.Value = Math.Max(mSharpenBar.Minimum, Math.Min(mSharpenBar.Maximum, (int)Math.Round(mAdjust.Sharpen * 10)));
            mLevelsBox.Checked = mAdjust.AutoLevels;
            mUpdating = false;
            bool adjustable = mImage != null && !mExact;
            mZoomBar.Enabled = mBrightBar.Enabled = mContrastBar.Enabled = mSharpenBar.Enabled = mLevelsBox.Enabled = adjustable;
            mCrop.Enabled = adjustable;
            mOkButton.Enabled = mNew != null;
            if (mImage == null)
                mSourceLabel.Text = "No image yet: Browse, Paste (Ctrl+V) or drop an image here.";
        }

        private void Render()
        {
            if (mImage != null && !mExact)
                mNew = TSB3Photos.Convert(mLum, mImage.Width, mImage.Height, mCrop.Crop, mAdjust, mPalette);
            mNewView.Pixels = mNewSmall.Pixels = mNew;
            mNewView.Invalidate();
            mNewSmall.Invalidate();
            mOkButton.Enabled = mNew != null;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing && mImage != null)
            {
                mImage.Dispose();
                mImage = null;
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TSB3PhotoEditorForm));
            this.SuspendLayout();
            // 
            // TSB3PhotoEditorForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TSB3PhotoEditorForm";
            this.ResumeLayout(false);

        }
    }

    /// <summary>
    /// An image with a 3:4 frame over it (Crop, in image pixels; it may reach past the edges): drag to
    /// move the frame, mouse wheel to zoom. The image is shown whole, fitted to the panel.
    /// </summary>
    public class CropPanel : Control
    {
        public event EventHandler CropChanged;

        private Bitmap mImage;
        private Bitmap mScaled; // the image at the panel's scale, drawn on each move
        private RectangleF mCrop;
        private float mScale = 1;
        private PointF mOrigin;
        private Point mDragFrom;
        private bool mDragging;

        public CropPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(64, 64, 64);
            Cursor = Cursors.SizeAll;
        }

        public Bitmap Image
        {
            get { return mImage; }
            set
            {
                mImage = value;
                if (mScaled != null)
                    mScaled.Dispose();
                mScaled = null;
                if (mImage != null)
                {
                    // fit with room around it, so a frame zoomed out past the edges still shows
                    mScale = Math.Min((Width - 40) / (float)mImage.Width, (Height - 40) / (float)mImage.Height);
                    int w = Math.Max(1, (int)(mImage.Width * mScale)), h = Math.Max(1, (int)(mImage.Height * mScale));
                    mOrigin = new PointF((Width - w) / 2f, (Height - h) / 2f);
                    mScaled = new Bitmap(w, h);
                    using (Graphics g = Graphics.FromImage(mScaled))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(mImage, 0, 0, w, h);
                    }
                }
                Invalidate();
            }
        }

        public RectangleF Crop
        {
            get { return mCrop; }
            set
            {
                mCrop = Keep(value);
                Invalidate();
            }
        }

        /// <summary>Keeps the frame's center on the image and its size sane.</summary>
        private RectangleF Keep(RectangleF c)
        {
            if (mImage == null)
                return c;
            float maxW = mImage.Width * 4f, minW = 6f;
            float w = Math.Max(minW, Math.Min(maxW, c.Width));
            float h = w * TSB3Photos.Height / TSB3Photos.Width;
            float cx = Math.Max(0, Math.Min(mImage.Width, c.X + c.Width / 2));
            float cy = Math.Max(0, Math.Min(mImage.Height, c.Y + c.Height / 2));
            return new RectangleF(cx - w / 2, cy - h / 2, w, h);
        }

        private RectangleF ScreenCrop()
        {
            return new RectangleF(mOrigin.X + mCrop.X * mScale, mOrigin.Y + mCrop.Y * mScale, mCrop.Width * mScale, mCrop.Height * mScale);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (mScaled == null)
            {
                using (SolidBrush b = new SolidBrush(Color.Silver))
                {
                    StringFormat f = new StringFormat();
                    f.Alignment = f.LineAlignment = StringAlignment.Center;
                    g.DrawString("Drop an image here", Font, b, ClientRectangle, f);
                }
                return;
            }
            g.DrawImage(mScaled, mOrigin.X, mOrigin.Y);
            RectangleF c = ScreenCrop();
            using (Region outside = new Region(ClientRectangle))
            {
                outside.Exclude(c);
                using (SolidBrush dim = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                    g.FillRegion(dim, outside);
            }
            using (Pen p = new Pen(Color.White, 1))
            {
                p.DashStyle = DashStyle.Dash;
                g.DrawRectangle(p, c.X, c.Y, c.Width, c.Height);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (mImage == null || e.Button != MouseButtons.Left)
                return;
            mDragging = true;
            mDragFrom = e.Location;
            Capture = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!mDragging)
                return;
            float dx = (e.X - mDragFrom.X) / mScale, dy = (e.Y - mDragFrom.Y) / mScale;
            mDragFrom = e.Location;
            Crop = new RectangleF(mCrop.X + dx, mCrop.Y + dy, mCrop.Width, mCrop.Height);
            Changed();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            mDragging = false;
            Capture = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (mImage == null)
                return;
            float f = e.Delta > 0 ? 1 / 1.1f : 1.1f;
            float w = mCrop.Width * f, h = mCrop.Height * f;
            Crop = new RectangleF(mCrop.X + mCrop.Width / 2 - w / 2, mCrop.Y + mCrop.Height / 2 - h / 2, w, h);
            Changed();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (Enabled)
                Focus(); // so the wheel comes here
        }

        private void Changed()
        {
            if (CropChanged != null)
                CropChanged(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && mScaled != null)
            {
                mScaled.Dispose();
                mScaled = null;
            }
            base.Dispose(disposing);
        }
    }
}
