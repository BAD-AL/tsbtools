using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TSBTool2
{
    /// <summary>
    /// TSB3's player photos, all at once ('By photo'): each photo with the roster slot it belongs to.
    /// Pick a photo to change its slot (or free it); double-click it, or drop an image on it, to edit
    /// the picture (TSB3PhotoEditorForm). Works on a copy of the ROM; OK writes the photos into the ROM
    /// it was given (saved with the rest by 'Apply to Rom').
    /// </summary>
    public class TSB3PhotosForm : Form
    {
        private const string FreeTeam = "(no player)";

        private readonly byte[] mRom;
        private readonly byte[] mWork;
        private readonly PlayerNameLookup mNames;
        private readonly Color[] mPalette;

        private FlowLayoutPanel mGrid;
        private PhotoCard[] mCards;
        private int mSelected = -1;
        private bool mUpdating;

        private PixelView mBig;
        private Label mTitleLabel, mWhoLabel, mUsedLabel, mNoteLabel;
        private ComboBox mTeamBox, mPositionBox;
        private Button mEditButton, mFreeButton;

        /// <param name="rom">The loaded ROM; changed only on OK.</param>
        /// <param name="names">Player names for a slot (the roster text); null shows slots only.</param>
        public TSB3PhotosForm(byte[] rom, PlayerNameLookup names)
        {
            mRom = rom;
            mWork = (byte[])rom.Clone();
            mNames = names;
            mPalette = TSB3Photos.GetPalette(rom);
            BuildUI();
            SelectPhoto(0);
        }

        /// <summary>Selects the photo the player in this slot has, if any.</summary>
        public void SelectPlayer(string team, string position)
        {
            int photo = TSB3Photos.FindPhoto(mWork, TSB3Photos.PlayerId(team, position));
            if (photo >= 0)
                SelectPhoto(photo);
        }

        #region Layout

        private void BuildUI()
        {
            Text = "TSB3 Player Photos";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(940, 640);
            MinimumSize = new Size(700, 420);
            ShowInTaskbar = false;
            MinimizeBox = false;
            KeyPreview = true;

            // top bar
            Panel top = new Panel();
            top.Dock = DockStyle.Top;
            top.Height = 36;
            Button export = MakeButton("&Export all...", 8, 6, 100);
            export.Click += new EventHandler(ExportAll);
            Button import = MakeButton("&Import folder...", 114, 6, 110);
            import.Click += new EventHandler(ImportFolder);
            mUsedLabel = new Label();
            mUsedLabel.AutoSize = true;
            mUsedLabel.Location = new Point(236, 11);
            Label hint = new Label();
            hint.AutoSize = true;
            hint.ForeColor = SystemColors.GrayText;
            hint.Location = new Point(400, 11);
            hint.Text = "Double-click a photo, or drop an image on it, to change the picture.";
            top.Controls.AddRange(new Control[] { export, import, mUsedLabel, hint });

            // bottom bar
            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 40;
            bottom.Width = ClientSize.Width;
            mNoteLabel = new Label();
            mNoteLabel.AutoSize = false;
            mNoteLabel.Location = new Point(8, 6);
            mNoteLabel.Size = new Size(700, 30);
            mNoteLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            mNoteLabel.ForeColor = SystemColors.GrayText;
            mNoteLabel.Text = "OK puts the photos in the loaded ROM; 'Apply to Rom' saves them with the rest. A photo goes with a\r\n" +
                              "roster slot (team and position), not a name.";
            Button ok = MakeButton("OK", 0, 8, 80);
            ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ok.Left = bottom.Width - 176;
            ok.DialogResult = DialogResult.OK;
            ok.Click += new EventHandler(OkClicked);
            Button cancel = MakeButton("Cancel", 0, 8, 80);
            cancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancel.Left = bottom.Width - 90;
            cancel.DialogResult = DialogResult.Cancel;
            bottom.Controls.AddRange(new Control[] { mNoteLabel, ok, cancel });
            AcceptButton = ok;
            CancelButton = cancel;

            // side panel: the selected photo
            Panel side = new Panel();
            side.Dock = DockStyle.Right;
            side.Width = 220;
            side.Padding = new Padding(8);
            mTitleLabel = new Label();
            mTitleLabel.Font = new Font(Font, FontStyle.Bold);
            mTitleLabel.Location = new Point(10, 4);
            mTitleLabel.Size = new Size(200, 18);
            mBig = new PixelView(mPalette, 6);
            mBig.Location = new Point(38, 26);
            mWhoLabel = new Label();
            mWhoLabel.Location = new Point(10, 224);
            mWhoLabel.Size = new Size(200, 34);
            Label teamLabel = new Label();
            teamLabel.Text = "Team";
            teamLabel.AutoSize = true;
            teamLabel.Location = new Point(10, 266);
            mTeamBox = new ComboBox();
            mTeamBox.DropDownStyle = ComboBoxStyle.DropDownList;
            mTeamBox.Location = new Point(70, 262);
            mTeamBox.Size = new Size(130, 21);
            mTeamBox.MaxDropDownItems = 20;
            mTeamBox.Items.Add(FreeTeam);
            foreach (string team in TSB2Tool.teams)
                mTeamBox.Items.Add(team);
            mTeamBox.SelectedIndexChanged += new EventHandler(SlotChanged);
            Label positionLabel = new Label();
            positionLabel.Text = "Position";
            positionLabel.AutoSize = true;
            positionLabel.Location = new Point(10, 294);
            mPositionBox = new ComboBox();
            mPositionBox.DropDownStyle = ComboBoxStyle.DropDownList;
            mPositionBox.Location = new Point(70, 290);
            mPositionBox.Size = new Size(130, 21);
            mPositionBox.MaxDropDownItems = 20;
            foreach (string position in TSB2Tool.positionNames)
                mPositionBox.Items.Add(position);
            mPositionBox.SelectedIndexChanged += new EventHandler(SlotChanged);
            mEditButton = MakeButton("Change &picture...", 10, 326, 190);
            mEditButton.Click += delegate { EditPhoto(mSelected, null); };
            mFreeButton = MakeButton("&No player (free it)", 10, 356, 190);
            mFreeButton.Click += delegate { mTeamBox.SelectedIndex = 0; };
            side.Controls.AddRange(new Control[] { mTitleLabel, mBig, mWhoLabel, teamLabel, mTeamBox, positionLabel, mPositionBox, mEditButton, mFreeButton });

            // the grid
            mGrid = new FlowLayoutPanel();
            mGrid.Dock = DockStyle.Fill;
            mGrid.AutoScroll = true;
            mGrid.BackColor = SystemColors.Window;
            mGrid.Padding = new Padding(4);
            mCards = new PhotoCard[TSB3Photos.PhotoCount];
            mGrid.SuspendLayout();
            for (int photo = 0; photo < TSB3Photos.PhotoCount; photo++)
            {
                PhotoCard card = new PhotoCard(photo, mPalette);
                card.Click += new EventHandler(CardClicked);
                card.DoubleClick += new EventHandler(CardDoubleClicked);
                card.FileDropped += new PhotoCard.FileDroppedHandler(CardFileDropped);
                mCards[photo] = card;
                mGrid.Controls.Add(card);
            }
            mGrid.ResumeLayout();

            Controls.Add(mGrid);
            Controls.Add(side);
            Controls.Add(top);
            Controls.Add(bottom);
            RefreshAll();
        }

        private static Button MakeButton(string text, int x, int y, int width)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(width, 24);
            return b;
        }

        #endregion

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (mSelected >= 0)
                mGrid.ScrollControlIntoView(mCards[mSelected]);
        }

        private void RefreshAll()
        {
            for (int photo = 0; photo < TSB3Photos.PhotoCount; photo++)
                RefreshCard(photo);
            mUsedLabel.Text = string.Format("{0} of {1} have a player", TSB3Photos.UsedCount(mWork), TSB3Photos.PlayerPhotoCount);
            RefreshSide();
        }

        private void RefreshCard(int photo)
        {
            PhotoCard card = mCards[photo];
            card.Pixels = TSB3Photos.GetPixels(mWork, photo);
            int id = TSB3Photos.GetPlayerId(mWork, photo);
            string team, position;
            card.Free = false;
            if (id < 0)
            {
                card.NameText = "everyone else";
                card.SlotText = "(silhouette)";
            }
            else if (TSB3Photos.GetSlot(id, out team, out position))
            {
                string name = mNames == null ? null : mNames(team, position);
                card.NameText = string.IsNullOrEmpty(name) ? "?" : name.Trim();
                card.SlotText = team + " " + position;
            }
            else
            {
                card.NameText = "free";
                card.SlotText = "";
                card.Free = true;
            }
            card.Selected = photo == mSelected;
            card.Invalidate();
        }

        private void SelectPhoto(int photo)
        {
            int was = mSelected;
            mSelected = photo;
            if (was >= 0)
                RefreshCard(was);
            RefreshCard(photo);
            mGrid.ScrollControlIntoView(mCards[photo]);
            RefreshSide();
        }

        private void RefreshSide()
        {
            if (mSelected < 0)
                return;
            mUpdating = true;
            try
            {
                mTitleLabel.Text = string.Format("Photo 0x{0:X2}", mSelected);
                mBig.Pixels = TSB3Photos.GetPixels(mWork, mSelected);
                mBig.Invalidate();
                mWhoLabel.Text = TSB3Photos.Describe(mWork, mSelected, mNames);
                bool silhouette = mSelected == TSB3Photos.DefaultPhoto;
                mTeamBox.Enabled = mPositionBox.Enabled = mFreeButton.Enabled = !silhouette;
                string team, position;
                if (!silhouette && TSB3Photos.GetSlot(TSB3Photos.GetPlayerId(mWork, mSelected), out team, out position))
                {
                    mTeamBox.SelectedItem = team;
                    mPositionBox.SelectedItem = position;
                    mPositionBox.Enabled = true;
                }
                else
                {
                    mTeamBox.SelectedIndex = 0;
                    mPositionBox.SelectedIndex = -1;
                    mPositionBox.Enabled = false;
                }
                if (silhouette)
                    mWhoLabel.Text = "The photo of every player without one.";
            }
            finally
            {
                mUpdating = false;
            }
        }

        private void SlotChanged(object sender, EventArgs e)
        {
            if (mUpdating || mSelected < 0 || mSelected == TSB3Photos.DefaultPhoto)
                return;
            int id;
            if (mTeamBox.SelectedIndex <= 0)
            {
                id = TSB3Photos.FreeId;
            }
            else
            {
                if (mPositionBox.SelectedIndex < 0)
                    mPositionBox.SelectedIndex = 0;
                id = TSB3Photos.PlayerId(mTeamBox.SelectedItem.ToString(), mPositionBox.SelectedItem.ToString());
            }
            if (id == TSB3Photos.GetPlayerId(mWork, mSelected))
                return;
            int other = id == TSB3Photos.FreeId ? -1 : TSB3Photos.FindPhoto(mWork, id);
            if (other >= 0)
            {
                string msg = string.Format("{0} already has photo 0x{1:X2}. A player has one photo:\r\n" +
                    "give him photo 0x{2:X2} instead (0x{1:X2} gets no player)?",
                    TSB3Photos.Describe(mWork, other, mNames), other, mSelected);
                if (MessageBox.Show(this, msg, "Photos", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    RefreshSide();
                    return;
                }
            }
            TSB3Photos.AssignPhoto(mWork, mSelected, id);
            RefreshAll();
        }

        private void CardClicked(object sender, EventArgs e)
        {
            SelectPhoto(((PhotoCard)sender).Photo);
        }

        private void CardDoubleClicked(object sender, EventArgs e)
        {
            int photo = ((PhotoCard)sender).Photo;
            SelectPhoto(photo);
            EditPhoto(photo, null);
        }

        private void CardFileDropped(PhotoCard card, string file)
        {
            SelectPhoto(card.Photo);
            EditPhoto(card.Photo, file);
        }

        private void EditPhoto(int photo, string file)
        {
            if (photo < 0)
                return;
            string title = string.Format("Photo 0x{0:X2} - {1}", photo, TSB3Photos.Describe(mWork, photo, mNames));
            using (TSB3PhotoEditorForm editor = new TSB3PhotoEditorForm(title, TSB3Photos.GetPixels(mWork, photo), mPalette))
            {
                if (file != null)
                    editor.LoadImageFile(file);
                if (editor.ShowDialog(this) == DialogResult.OK && editor.Pixels != null)
                {
                    TSB3Photos.SetPixels(mWork, photo, editor.Pixels);
                    RefreshCard(photo);
                    RefreshSide();
                }
            }
        }

        private void ExportAll(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Folder for the photos (0x00.png - 0x54.png):";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    string report = TSB3Photos.ExportPhotos(mWork, dlg.SelectedPath, mNames);
                    MessageBox.Show(this, string.Format("Wrote {0} photos to '{1}'.", TSB3Photos.PhotoCount, dlg.SelectedPath), "Export photos");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Export photos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportFolder(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Folder with photos named 0x00 - 0x54 (.png .jpg .bmp .gif); the others stay as they are:";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    int changed;
                    string report = TSB3Photos.ImportPhotos(mWork, dlg.SelectedPath, mNames, out changed);
                    RefreshAll();
                    if (report.Length == 0)
                        report = "No photo files (0x00 - 0x54) in that folder.";
                    TSBTool.RichTextDisplay.ShowMessage(string.Format("Import photos: {0} changed", changed), report,
                        SystemIcons.Information, false, false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Import photos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OkClicked(object sender, EventArgs e)
        {
            Array.Copy(mWork, TSB3Photos.PlayerIdTable, mRom, TSB3Photos.PlayerIdTable, 2 * TSB3Photos.PlayerPhotoCount);
            for (int photo = 0; photo < TSB3Photos.PhotoCount; photo++)
            {
                int at = TSB3Photos.GetPhotoLocation(mWork, photo);
                Array.Copy(mWork, at, mRom, at, TSB3Photos.PhotoBytes);
            }
        }
    }

    /// <summary>Photo pixels (palette indexes) drawn 'scale' times, sharp.</summary>
    public class PixelView : Control
    {
        private readonly Color[] mPalette;
        private readonly int mScale;
        public byte[,] Pixels;

        public PixelView(Color[] palette, int scale)
        {
            mPalette = palette;
            mScale = scale;
            Size = new Size(TSB3Photos.Width * scale + 2, TSB3Photos.Height * scale + 2);
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(SystemColors.ControlDark);
            if (Pixels == null)
                return;
            for (int y = 0; y < TSB3Photos.Height; y++)
            {
                for (int x = 0; x < TSB3Photos.Width; x++)
                {
                    using (SolidBrush b = new SolidBrush(mPalette[Pixels[y, x]]))
                        e.Graphics.FillRectangle(b, 1 + x * mScale, 1 + y * mScale, mScale, mScale);
                }
            }
        }
    }

    /// <summary>One photo in the grid: picture, player name and slot. Takes a dropped image file.</summary>
    public class PhotoCard : Control
    {
        public delegate void FileDroppedHandler(PhotoCard card, string file);
        public event FileDroppedHandler FileDropped;

        private const int PixelScale = 3;
        private readonly Color[] mPalette;
        public readonly int Photo;
        public byte[,] Pixels;
        public string NameText = "", SlotText = "";
        public bool Selected, Free;
        private bool mDropHover;

        public PhotoCard(int photo, Color[] palette)
        {
            Photo = photo;
            mPalette = palette;
            Size = new Size(104, 146);
            Margin = new Padding(3);
            AllowDrop = true;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Selected ? SystemColors.Highlight : (mDropHover ? Color.LightSkyBlue : SystemColors.Window));
            Color text = Selected ? SystemColors.HighlightText : SystemColors.WindowText;
            int px = (Width - TSB3Photos.Width * PixelScale) / 2, py = 4;
            if (Pixels != null)
            {
                for (int y = 0; y < TSB3Photos.Height; y++)
                {
                    for (int x = 0; x < TSB3Photos.Width; x++)
                    {
                        using (SolidBrush b = new SolidBrush(mPalette[Pixels[y, x]]))
                            g.FillRectangle(b, px + x * PixelScale, py + y * PixelScale, PixelScale, PixelScale);
                    }
                }
            }
            if (Free)
            {
                using (Pen p = new Pen(Color.Firebrick, 2))
                    g.DrawRectangle(p, px - 1, py - 1, TSB3Photos.Width * PixelScale + 1, TSB3Photos.Height * PixelScale + 1);
            }
            StringFormat center = new StringFormat();
            center.Alignment = StringAlignment.Center;
            center.Trimming = StringTrimming.EllipsisCharacter;
            center.FormatFlags = StringFormatFlags.NoWrap;
            int ty = py + TSB3Photos.Height * PixelScale + 2;
            using (SolidBrush b = new SolidBrush(text))
            using (SolidBrush gray = new SolidBrush(Selected ? SystemColors.HighlightText : SystemColors.GrayText))
            using (Font bold = new Font(Font, FontStyle.Bold))
            {
                g.DrawString(string.Format("0x{0:X2}", Photo), Font, gray, new RectangleF(0, ty, Width, 14), center);
                g.DrawString(NameText, bold, Free ? Brushes.Firebrick : b, new RectangleF(0, ty + 13, Width, 14), center);
                g.DrawString(SlotText, Font, gray, new RectangleF(0, ty + 27, Width, 14), center);
            }
        }

        private static string DroppedImage(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return null;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            return files != null && files.Length == 1 && TSB3Photos.IsImageFile(files[0]) ? files[0] : null;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effect = DroppedImage(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            mDropHover = e.Effect != DragDropEffects.None;
            Invalidate();
        }

        protected override void OnDragLeave(EventArgs e)
        {
            mDropHover = false;
            Invalidate();
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            mDropHover = false;
            Invalidate();
            string file = DroppedImage(e);
            if (file != null && FileDropped != null)
                FileDropped(this, file);
        }
    }
}
