namespace TSBTool2
{
	partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyToROMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugDialogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.seasonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.season1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.season2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.season3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allSeasonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutTSBTool2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mLoadButton = new System.Windows.Forms.Button();
            this.mViewContentsButton = new System.Windows.Forms.Button();
            this.mApplyButton = new System.Windows.Forms.Button();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playbooksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scheduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mTextBox = new TSBTool2.SearchTextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.seasonToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(701, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.applyToROMToolStripMenuItem,
            this.viewContentsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.openToolStripMenuItem.Text = "Load TSB2 ROM";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.handleLoad);
            // 
            // applyToROMToolStripMenuItem
            // 
            this.applyToROMToolStripMenuItem.Name = "applyToROMToolStripMenuItem";
            this.applyToROMToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.applyToROMToolStripMenuItem.Text = "Apply To ROM";
            // 
            // viewContentsToolStripMenuItem
            // 
            this.viewContentsToolStripMenuItem.Name = "viewContentsToolStripMenuItem";
            this.viewContentsToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.viewContentsToolStripMenuItem.Text = "View Contents";
            this.viewContentsToolStripMenuItem.Click += new System.EventHandler(this.viewContentsAction);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugDialogToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // debugDialogToolStripMenuItem
            // 
            this.debugDialogToolStripMenuItem.Name = "debugDialogToolStripMenuItem";
            this.debugDialogToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.debugDialogToolStripMenuItem.Text = "Debug Dialog";
            this.debugDialogToolStripMenuItem.Click += new System.EventHandler(this.debugDialogToolStripMenuItem_Click);
            // 
            // seasonToolStripMenuItem
            // 
            this.seasonToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.season1ToolStripMenuItem,
            this.season2ToolStripMenuItem,
            this.season3ToolStripMenuItem,
            this.allSeasonsToolStripMenuItem});
            this.seasonToolStripMenuItem.Name = "seasonToolStripMenuItem";
            this.seasonToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.seasonToolStripMenuItem.Text = "Season";
            // 
            // season1ToolStripMenuItem
            // 
            this.season1ToolStripMenuItem.Checked = true;
            this.season1ToolStripMenuItem.CheckOnClick = true;
            this.season1ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.season1ToolStripMenuItem.Name = "season1ToolStripMenuItem";
            this.season1ToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.season1ToolStripMenuItem.Text = "Season 1";
            this.season1ToolStripMenuItem.Click += new System.EventHandler(this.seasonItemClicked);
            // 
            // season2ToolStripMenuItem
            // 
            this.season2ToolStripMenuItem.CheckOnClick = true;
            this.season2ToolStripMenuItem.Name = "season2ToolStripMenuItem";
            this.season2ToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.season2ToolStripMenuItem.Text = "Season 2";
            this.season2ToolStripMenuItem.Click += new System.EventHandler(this.seasonItemClicked);
            // 
            // season3ToolStripMenuItem
            // 
            this.season3ToolStripMenuItem.CheckOnClick = true;
            this.season3ToolStripMenuItem.Name = "season3ToolStripMenuItem";
            this.season3ToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.season3ToolStripMenuItem.Text = "Season 3";
            this.season3ToolStripMenuItem.Click += new System.EventHandler(this.seasonItemClicked);
            // 
            // allSeasonsToolStripMenuItem
            // 
            this.allSeasonsToolStripMenuItem.CheckOnClick = true;
            this.allSeasonsToolStripMenuItem.Name = "allSeasonsToolStripMenuItem";
            this.allSeasonsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.allSeasonsToolStripMenuItem.Text = "All Seasons";
            this.allSeasonsToolStripMenuItem.Click += new System.EventHandler(this.seasonItemClicked);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutTSBTool2ToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // aboutTSBTool2ToolStripMenuItem
            // 
            this.aboutTSBTool2ToolStripMenuItem.Name = "aboutTSBTool2ToolStripMenuItem";
            this.aboutTSBTool2ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.aboutTSBTool2ToolStripMenuItem.Text = "About TSBTool2";
            this.aboutTSBTool2ToolStripMenuItem.Click += new System.EventHandler(this.aboutTSBTool2ToolStripMenuItem_Click);
            // 
            // mLoadButton
            // 
            this.mLoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mLoadButton.Location = new System.Drawing.Point(2, 447);
            this.mLoadButton.Name = "mLoadButton";
            this.mLoadButton.Size = new System.Drawing.Size(143, 37);
            this.mLoadButton.TabIndex = 2;
            this.mLoadButton.Text = "Load TSB2 ROM";
            this.mLoadButton.UseVisualStyleBackColor = true;
            this.mLoadButton.Click += new System.EventHandler(this.handleLoad);
            // 
            // mViewContentsButton
            // 
            this.mViewContentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mViewContentsButton.Location = new System.Drawing.Point(149, 447);
            this.mViewContentsButton.Name = "mViewContentsButton";
            this.mViewContentsButton.Size = new System.Drawing.Size(143, 37);
            this.mViewContentsButton.TabIndex = 3;
            this.mViewContentsButton.Text = "View Contents";
            this.mViewContentsButton.UseVisualStyleBackColor = true;
            this.mViewContentsButton.Click += new System.EventHandler(this.viewContentsAction);
            // 
            // mApplyButton
            // 
            this.mApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mApplyButton.Location = new System.Drawing.Point(546, 447);
            this.mApplyButton.Name = "mApplyButton";
            this.mApplyButton.Size = new System.Drawing.Size(143, 37);
            this.mApplyButton.TabIndex = 4;
            this.mApplyButton.Text = "Apply To Rom";
            this.mApplyButton.UseVisualStyleBackColor = true;
            this.mApplyButton.Click += new System.EventHandler(this.mApplyButton_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playbooksToolStripMenuItem,
            this.simDataToolStripMenuItem,
            this.scheduleToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // playbooksToolStripMenuItem
            // 
            this.playbooksToolStripMenuItem.Checked = true;
            this.playbooksToolStripMenuItem.CheckOnClick = true;
            this.playbooksToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.playbooksToolStripMenuItem.Name = "playbooksToolStripMenuItem";
            this.playbooksToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.playbooksToolStripMenuItem.Text = "Playbooks";
            this.playbooksToolStripMenuItem.Click += new System.EventHandler(this.playbooksToolStripMenuItem_Click);
            // 
            // simDataToolStripMenuItem
            // 
            this.simDataToolStripMenuItem.Checked = true;
            this.simDataToolStripMenuItem.CheckOnClick = true;
            this.simDataToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.simDataToolStripMenuItem.Name = "simDataToolStripMenuItem";
            this.simDataToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.simDataToolStripMenuItem.Text = "Sim Data";
            this.simDataToolStripMenuItem.Click += new System.EventHandler(this.simDataToolStripMenuItem_Click);
            // 
            // scheduleToolStripMenuItem
            // 
            this.scheduleToolStripMenuItem.Checked = true;
            this.scheduleToolStripMenuItem.CheckOnClick = true;
            this.scheduleToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.scheduleToolStripMenuItem.Name = "scheduleToolStripMenuItem";
            this.scheduleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.scheduleToolStripMenuItem.Text = "Schedule";
            this.scheduleToolStripMenuItem.Click += new System.EventHandler(this.scheduleToolStripMenuItem_Click);
            // 
            // mTextBox
            // 
            this.mTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mTextBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mTextBox.Location = new System.Drawing.Point(0, 27);
            this.mTextBox.Name = "mTextBox";
            this.mTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.mTextBox.SearchString = null;
            this.mTextBox.Size = new System.Drawing.Size(701, 414);
            this.mTextBox.StatusControl = null;
            this.mTextBox.TabIndex = 1;
            this.mTextBox.Text = "";
            this.mTextBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mTextBox_MouseDoubleClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 487);
            this.Controls.Add(this.mApplyButton);
            this.Controls.Add(this.mViewContentsButton);
            this.Controls.Add(this.mLoadButton);
            this.Controls.Add(this.mTextBox);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "TSBTool2";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem debugDialogToolStripMenuItem;
		private SearchTextBox mTextBox;
		private System.Windows.Forms.Button mLoadButton;
		private System.Windows.Forms.Button mViewContentsButton;
		private System.Windows.Forms.Button mApplyButton;
		private System.Windows.Forms.ToolStripMenuItem applyToROMToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewContentsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem seasonToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem season1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem season2ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem season3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allSeasonsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutTSBTool2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playbooksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scheduleToolStripMenuItem;
	}
}

