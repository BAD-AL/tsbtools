namespace TSBTool
{
    partial class SnesColorPickerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mAvailableColorsLabel = new System.Windows.Forms.Label();
            this.mColorGridPanel = new System.Windows.Forms.Panel();
            this.mSelectedColorTitleLabel = new System.Windows.Forms.Label();
            this.mPreviewPanel = new System.Windows.Forms.Panel();
            this.mRedLabel = new System.Windows.Forms.Label();
            this.mRedValueLabel = new System.Windows.Forms.Label();
            this.mRedTrackBar = new System.Windows.Forms.TrackBar();
            this.mRedMinLabel = new System.Windows.Forms.Label();
            this.mGreenLabel = new System.Windows.Forms.Label();
            this.mGreenValueLabel = new System.Windows.Forms.Label();
            this.mGreenTrackBar = new System.Windows.Forms.TrackBar();
            this.mGreenMinLabel = new System.Windows.Forms.Label();
            this.mBlueLabel = new System.Windows.Forms.Label();
            this.mBlueValueLabel = new System.Windows.Forms.Label();
            this.mBlueTrackBar = new System.Windows.Forms.TrackBar();
            this.mBlueMinLabel = new System.Windows.Forms.Label();
            this.mSelectedColorGroupBox = new System.Windows.Forms.GroupBox();
            this.mRgbLabel = new System.Windows.Forms.Label();
            this.mHexLabel = new System.Windows.Forms.Label();
            this.mRgb555Label = new System.Windows.Forms.Label();
            this.mOkButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.mRedTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mGreenTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mBlueTrackBar)).BeginInit();
            this.mSelectedColorGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // mAvailableColorsLabel
            //
            this.mAvailableColorsLabel.AutoSize = true;
            this.mAvailableColorsLabel.Location = new System.Drawing.Point(12, 9);
            this.mAvailableColorsLabel.Name = "mAvailableColorsLabel";
            this.mAvailableColorsLabel.Size = new System.Drawing.Size(180, 13);
            this.mAvailableColorsLabel.TabIndex = 0;
            this.mAvailableColorsLabel.Text = "AVAILABLE COLORS (16x16, 256)";
            //
            // mColorGridPanel
            //
            this.mColorGridPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mColorGridPanel.Location = new System.Drawing.Point(12, 28);
            this.mColorGridPanel.Name = "mColorGridPanel";
            this.mColorGridPanel.Size = new System.Drawing.Size(256, 256);
            this.mColorGridPanel.TabIndex = 1;
            this.mColorGridPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.mColorGridPanel_Paint);
            this.mColorGridPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mColorGridPanel_MouseDown);
            //
            // mSelectedColorTitleLabel
            //
            this.mSelectedColorTitleLabel.AutoSize = true;
            this.mSelectedColorTitleLabel.Location = new System.Drawing.Point(290, 9);
            this.mSelectedColorTitleLabel.Name = "mSelectedColorTitleLabel";
            this.mSelectedColorTitleLabel.Size = new System.Drawing.Size(80, 13);
            this.mSelectedColorTitleLabel.TabIndex = 2;
            this.mSelectedColorTitleLabel.Text = "SELECTED COLOR";
            //
            // mPreviewPanel
            //
            this.mPreviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.mPreviewPanel.Location = new System.Drawing.Point(290, 28);
            this.mPreviewPanel.Name = "mPreviewPanel";
            this.mPreviewPanel.Size = new System.Drawing.Size(230, 90);
            this.mPreviewPanel.TabIndex = 3;
            //
            // mRedLabel
            //
            this.mRedLabel.Location = new System.Drawing.Point(290, 130);
            this.mRedLabel.Name = "mRedLabel";
            this.mRedLabel.Size = new System.Drawing.Size(60, 15);
            this.mRedLabel.TabIndex = 4;
            this.mRedLabel.Text = "RED";
            this.mRedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mRedValueLabel
            //
            this.mRedValueLabel.Location = new System.Drawing.Point(290, 147);
            this.mRedValueLabel.Name = "mRedValueLabel";
            this.mRedValueLabel.Size = new System.Drawing.Size(60, 15);
            this.mRedValueLabel.TabIndex = 5;
            this.mRedValueLabel.Text = "0";
            this.mRedValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mRedTrackBar
            //
            this.mRedTrackBar.LargeChange = 4;
            this.mRedTrackBar.Location = new System.Drawing.Point(295, 167);
            this.mRedTrackBar.Maximum = 31;
            this.mRedTrackBar.Name = "mRedTrackBar";
            this.mRedTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mRedTrackBar.Size = new System.Drawing.Size(45, 170);
            this.mRedTrackBar.TabIndex = 6;
            this.mRedTrackBar.TickFrequency = 1;
            this.mRedTrackBar.ValueChanged += new System.EventHandler(this.ColorTrackBar_ValueChanged);
            //
            // mRedMinLabel
            //
            this.mRedMinLabel.Location = new System.Drawing.Point(290, 340);
            this.mRedMinLabel.Name = "mRedMinLabel";
            this.mRedMinLabel.Size = new System.Drawing.Size(60, 15);
            this.mRedMinLabel.TabIndex = 7;
            this.mRedMinLabel.Text = "0";
            this.mRedMinLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mGreenLabel
            //
            this.mGreenLabel.Location = new System.Drawing.Point(355, 130);
            this.mGreenLabel.Name = "mGreenLabel";
            this.mGreenLabel.Size = new System.Drawing.Size(60, 15);
            this.mGreenLabel.TabIndex = 8;
            this.mGreenLabel.Text = "GREEN";
            this.mGreenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mGreenValueLabel
            //
            this.mGreenValueLabel.Location = new System.Drawing.Point(355, 147);
            this.mGreenValueLabel.Name = "mGreenValueLabel";
            this.mGreenValueLabel.Size = new System.Drawing.Size(60, 15);
            this.mGreenValueLabel.TabIndex = 9;
            this.mGreenValueLabel.Text = "0";
            this.mGreenValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mGreenTrackBar
            //
            this.mGreenTrackBar.LargeChange = 4;
            this.mGreenTrackBar.Location = new System.Drawing.Point(360, 167);
            this.mGreenTrackBar.Maximum = 31;
            this.mGreenTrackBar.Name = "mGreenTrackBar";
            this.mGreenTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mGreenTrackBar.Size = new System.Drawing.Size(45, 170);
            this.mGreenTrackBar.TabIndex = 10;
            this.mGreenTrackBar.TickFrequency = 1;
            this.mGreenTrackBar.ValueChanged += new System.EventHandler(this.ColorTrackBar_ValueChanged);
            //
            // mGreenMinLabel
            //
            this.mGreenMinLabel.Location = new System.Drawing.Point(355, 340);
            this.mGreenMinLabel.Name = "mGreenMinLabel";
            this.mGreenMinLabel.Size = new System.Drawing.Size(60, 15);
            this.mGreenMinLabel.TabIndex = 11;
            this.mGreenMinLabel.Text = "0";
            this.mGreenMinLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mBlueLabel
            //
            this.mBlueLabel.Location = new System.Drawing.Point(420, 130);
            this.mBlueLabel.Name = "mBlueLabel";
            this.mBlueLabel.Size = new System.Drawing.Size(60, 15);
            this.mBlueLabel.TabIndex = 12;
            this.mBlueLabel.Text = "BLUE";
            this.mBlueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mBlueValueLabel
            //
            this.mBlueValueLabel.Location = new System.Drawing.Point(420, 147);
            this.mBlueValueLabel.Name = "mBlueValueLabel";
            this.mBlueValueLabel.Size = new System.Drawing.Size(60, 15);
            this.mBlueValueLabel.TabIndex = 13;
            this.mBlueValueLabel.Text = "0";
            this.mBlueValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mBlueTrackBar
            //
            this.mBlueTrackBar.LargeChange = 4;
            this.mBlueTrackBar.Location = new System.Drawing.Point(425, 167);
            this.mBlueTrackBar.Maximum = 31;
            this.mBlueTrackBar.Name = "mBlueTrackBar";
            this.mBlueTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.mBlueTrackBar.Size = new System.Drawing.Size(45, 170);
            this.mBlueTrackBar.TabIndex = 14;
            this.mBlueTrackBar.TickFrequency = 1;
            this.mBlueTrackBar.ValueChanged += new System.EventHandler(this.ColorTrackBar_ValueChanged);
            //
            // mBlueMinLabel
            //
            this.mBlueMinLabel.Location = new System.Drawing.Point(420, 340);
            this.mBlueMinLabel.Name = "mBlueMinLabel";
            this.mBlueMinLabel.Size = new System.Drawing.Size(60, 15);
            this.mBlueMinLabel.TabIndex = 15;
            this.mBlueMinLabel.Text = "0";
            this.mBlueMinLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // mSelectedColorGroupBox
            //
            this.mSelectedColorGroupBox.Controls.Add(this.mRgbLabel);
            this.mSelectedColorGroupBox.Controls.Add(this.mHexLabel);
            this.mSelectedColorGroupBox.Controls.Add(this.mRgb555Label);
            this.mSelectedColorGroupBox.Location = new System.Drawing.Point(500, 28);
            this.mSelectedColorGroupBox.Name = "mSelectedColorGroupBox";
            this.mSelectedColorGroupBox.Size = new System.Drawing.Size(190, 110);
            this.mSelectedColorGroupBox.TabIndex = 16;
            this.mSelectedColorGroupBox.TabStop = false;
            this.mSelectedColorGroupBox.Text = "Selected Color";
            //
            // mRgbLabel
            //
            this.mRgbLabel.AutoSize = true;
            this.mRgbLabel.Location = new System.Drawing.Point(16, 76);
            this.mRgbLabel.Name = "mRgbLabel";
            this.mRgbLabel.Size = new System.Drawing.Size(70, 13);
            this.mRgbLabel.TabIndex = 2;
            this.mRgbLabel.Text = "RGB: #000000";
            //
            // mHexLabel
            //
            this.mHexLabel.AutoSize = true;
            this.mHexLabel.Location = new System.Drawing.Point(16, 51);
            this.mHexLabel.Name = "mHexLabel";
            this.mHexLabel.Size = new System.Drawing.Size(60, 13);
            this.mHexLabel.TabIndex = 1;
            this.mHexLabel.Text = "HEX: $0000";
            //
            // mRgb555Label
            //
            this.mRgb555Label.AutoSize = true;
            this.mRgb555Label.Location = new System.Drawing.Point(16, 26);
            this.mRgb555Label.Name = "mRgb555Label";
            this.mRgb555Label.Size = new System.Drawing.Size(110, 13);
            this.mRgb555Label.TabIndex = 0;
            this.mRgb555Label.Text = "RGB 555: (0, 0, 0)";
            //
            // mOkButton
            //
            this.mOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.mOkButton.Location = new System.Drawing.Point(520, 410);
            this.mOkButton.Name = "mOkButton";
            this.mOkButton.Size = new System.Drawing.Size(75, 29);
            this.mOkButton.TabIndex = 17;
            this.mOkButton.Text = "OK";
            this.mOkButton.UseVisualStyleBackColor = true;
            //
            // mCancelButton
            //
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(605, 410);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 29);
            this.mCancelButton.TabIndex = 18;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            //
            // SnesColorPickerForm
            //
            this.AcceptButton = this.mOkButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(704, 461);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOkButton);
            this.Controls.Add(this.mSelectedColorGroupBox);
            this.Controls.Add(this.mBlueMinLabel);
            this.Controls.Add(this.mBlueTrackBar);
            this.Controls.Add(this.mBlueValueLabel);
            this.Controls.Add(this.mBlueLabel);
            this.Controls.Add(this.mGreenMinLabel);
            this.Controls.Add(this.mGreenTrackBar);
            this.Controls.Add(this.mGreenValueLabel);
            this.Controls.Add(this.mGreenLabel);
            this.Controls.Add(this.mRedMinLabel);
            this.Controls.Add(this.mRedTrackBar);
            this.Controls.Add(this.mRedValueLabel);
            this.Controls.Add(this.mRedLabel);
            this.Controls.Add(this.mPreviewPanel);
            this.Controls.Add(this.mSelectedColorTitleLabel);
            this.Controls.Add(this.mColorGridPanel);
            this.Controls.Add(this.mAvailableColorsLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SnesColorPickerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SNES Color Editor";
            ((System.ComponentModel.ISupportInitialize)(this.mRedTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mGreenTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mBlueTrackBar)).EndInit();
            this.mSelectedColorGroupBox.ResumeLayout(false);
            this.mSelectedColorGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mAvailableColorsLabel;
        private System.Windows.Forms.Panel mColorGridPanel;
        private System.Windows.Forms.Label mSelectedColorTitleLabel;
        private System.Windows.Forms.Panel mPreviewPanel;
        private System.Windows.Forms.Label mRedLabel;
        private System.Windows.Forms.Label mRedValueLabel;
        private System.Windows.Forms.TrackBar mRedTrackBar;
        private System.Windows.Forms.Label mRedMinLabel;
        private System.Windows.Forms.Label mGreenLabel;
        private System.Windows.Forms.Label mGreenValueLabel;
        private System.Windows.Forms.TrackBar mGreenTrackBar;
        private System.Windows.Forms.Label mGreenMinLabel;
        private System.Windows.Forms.Label mBlueLabel;
        private System.Windows.Forms.Label mBlueValueLabel;
        private System.Windows.Forms.TrackBar mBlueTrackBar;
        private System.Windows.Forms.Label mBlueMinLabel;
        private System.Windows.Forms.GroupBox mSelectedColorGroupBox;
        private System.Windows.Forms.Label mRgbLabel;
        private System.Windows.Forms.Label mHexLabel;
        private System.Windows.Forms.Label mRgb555Label;
        private System.Windows.Forms.Button mOkButton;
        private System.Windows.Forms.Button mCancelButton;
    }
}


