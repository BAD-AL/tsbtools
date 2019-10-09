namespace TSBTool2
{
    partial class DebugDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugDialog));
            this.mInputTextBox = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.mFindButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.miscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceStringsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findStringsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mathTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stringTableGetTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stringTableSetTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tSB2StringsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertAttributesToBytesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getSpecialLocationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playBookToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playSelectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.conversionTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scheduleHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mGetTeamButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.mGetBytesTextBox = new System.Windows.Forms.TextBox();
            this.mGetBytesButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.mSetByteLocUpDown = new System.Windows.Forms.NumericUpDown();
            this.mSetByteButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.mSetByteValTextBox = new System.Windows.Forms.TextBox();
            this.mFindBytesButton = new System.Windows.Forms.Button();
            this.mStatusLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.mToUpperCheckBox = new System.Windows.Forms.CheckBox();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.getPositionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.qbsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rbsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wrsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.olItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dlItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cbsItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mResultsTextBox = new TSBTool2.SearchTextBox();
            this.teamItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mSetByteLocUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // mInputTextBox
            // 
            this.mInputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mInputTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mInputTextBox.Location = new System.Drawing.Point(11, 29);
            this.mInputTextBox.Name = "mInputTextBox";
            this.mInputTextBox.Size = new System.Drawing.Size(355, 20);
            this.mInputTextBox.TabIndex = 0;
            this.mInputTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.mInputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(74, 108);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(434, 20);
            this.textBox2.TabIndex = 1;
            // 
            // mFindButton
            // 
            this.mFindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mFindButton.Location = new System.Drawing.Point(385, 26);
            this.mFindButton.Name = "mFindButton";
            this.mFindButton.Size = new System.Drawing.Size(124, 23);
            this.mFindButton.TabIndex = 4;
            this.mFindButton.Text = "Find string in ROM";
            this.mFindButton.UseVisualStyleBackColor = true;
            this.mFindButton.Click += new System.EventHandler(this.mFindButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miscToolStripMenuItem,
            this.findToolStripMenuItem,
            this.getPositionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(522, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // miscToolStripMenuItem
            // 
            this.miscToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.replaceStringToolStripMenuItem,
            this.replaceStringsToolStripMenuItem,
            this.findStringsToolStripMenuItem});
            this.miscToolStripMenuItem.Name = "miscToolStripMenuItem";
            this.miscToolStripMenuItem.Size = new System.Drawing.Size(130, 20);
            this.miscToolStripMenuItem.Text = "String Find && replace";
            // 
            // replaceStringToolStripMenuItem
            // 
            this.replaceStringToolStripMenuItem.Name = "replaceStringToolStripMenuItem";
            this.replaceStringToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.replaceStringToolStripMenuItem.Text = "Replace String";
            this.replaceStringToolStripMenuItem.Click += new System.EventHandler(this.replaceStringToolStripMenuItem_Click);
            // 
            // replaceStringsToolStripMenuItem
            // 
            this.replaceStringsToolStripMenuItem.Name = "replaceStringsToolStripMenuItem";
            this.replaceStringsToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.replaceStringsToolStripMenuItem.Text = "Replace Strings";
            this.replaceStringsToolStripMenuItem.Click += new System.EventHandler(this.replaceStringsToolStripMenuItem_Click);
            // 
            // findStringsToolStripMenuItem
            // 
            this.findStringsToolStripMenuItem.Name = "findStringsToolStripMenuItem";
            this.findStringsToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.findStringsToolStripMenuItem.Text = "&Find Strings";
            this.findStringsToolStripMenuItem.Click += new System.EventHandler(this.findStringsToolStripMenuItem_Click);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mathTestToolStripMenuItem,
            this.stringTableGetTestToolStripMenuItem,
            this.stringTableSetTestToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.tSB2StringsToolStripMenuItem,
            this.convertAttributesToBytesToolStripMenuItem,
            this.getSpecialLocationsToolStripMenuItem,
            this.playBookToolStripMenuItem,
            this.playSelectToolStripMenuItem,
            this.conversionTestToolStripMenuItem,
            this.scheduleHelpToolStripMenuItem});
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.findToolStripMenuItem.Text = "Misc";
            // 
            // mathTestToolStripMenuItem
            // 
            this.mathTestToolStripMenuItem.Name = "mathTestToolStripMenuItem";
            this.mathTestToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.mathTestToolStripMenuItem.Text = "Math test";
            this.mathTestToolStripMenuItem.Click += new System.EventHandler(this.mathTestToolStripMenuItem_Click);
            // 
            // stringTableGetTestToolStripMenuItem
            // 
            this.stringTableGetTestToolStripMenuItem.Name = "stringTableGetTestToolStripMenuItem";
            this.stringTableGetTestToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.stringTableGetTestToolStripMenuItem.Text = "StringTable Get Test";
            this.stringTableGetTestToolStripMenuItem.Click += new System.EventHandler(this.stringTableGetTestToolStripMenuItem_Click);
            // 
            // stringTableSetTestToolStripMenuItem
            // 
            this.stringTableSetTestToolStripMenuItem.Name = "stringTableSetTestToolStripMenuItem";
            this.stringTableSetTestToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.stringTableSetTestToolStripMenuItem.Text = "StringTable Set Test";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // tSB2StringsToolStripMenuItem
            // 
            this.tSB2StringsToolStripMenuItem.Name = "tSB2StringsToolStripMenuItem";
            this.tSB2StringsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.tSB2StringsToolStripMenuItem.Text = "TSB2 Strings";
            this.tSB2StringsToolStripMenuItem.Click += new System.EventHandler(this.tSB2StringsToolStripMenuItem_Click);
            // 
            // convertAttributesToBytesToolStripMenuItem
            // 
            this.convertAttributesToBytesToolStripMenuItem.Name = "convertAttributesToBytesToolStripMenuItem";
            this.convertAttributesToBytesToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.convertAttributesToBytesToolStripMenuItem.Text = "Convert Attributes To Bytes";
            this.convertAttributesToBytesToolStripMenuItem.Click += new System.EventHandler(this.convertAttributesToBytesToolStripMenuItem_Click);
            // 
            // getSpecialLocationsToolStripMenuItem
            // 
            this.getSpecialLocationsToolStripMenuItem.Name = "getSpecialLocationsToolStripMenuItem";
            this.getSpecialLocationsToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.getSpecialLocationsToolStripMenuItem.Text = "List TSB2 Teams";
            this.getSpecialLocationsToolStripMenuItem.Click += new System.EventHandler(this.listTSB2MenuItem_Click);
            // 
            // playBookToolStripMenuItem
            // 
            this.playBookToolStripMenuItem.Name = "playBookToolStripMenuItem";
            this.playBookToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.playBookToolStripMenuItem.Text = "PlayBook";
            this.playBookToolStripMenuItem.Click += new System.EventHandler(this.playBookToolStripMenuItem_Click);
            // 
            // playSelectToolStripMenuItem
            // 
            this.playSelectToolStripMenuItem.Name = "playSelectToolStripMenuItem";
            this.playSelectToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.playSelectToolStripMenuItem.Text = "Play Select";
            this.playSelectToolStripMenuItem.Click += new System.EventHandler(this.playSelectToolStripMenuItem_Click);
            // 
            // conversionTestToolStripMenuItem
            // 
            this.conversionTestToolStripMenuItem.Name = "conversionTestToolStripMenuItem";
            this.conversionTestToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.conversionTestToolStripMenuItem.Text = "Conversion Test";
            this.conversionTestToolStripMenuItem.Click += new System.EventHandler(this.conversionTestToolStripMenuItem_Click);
            // 
            // scheduleHelpToolStripMenuItem
            // 
            this.scheduleHelpToolStripMenuItem.Name = "scheduleHelpToolStripMenuItem";
            this.scheduleHelpToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            this.scheduleHelpToolStripMenuItem.Text = "ScheduleHelp";
            this.scheduleHelpToolStripMenuItem.Click += new System.EventHandler(this.scheduleHelpToolStripMenuItem_Click);
            // 
            // mGetTeamButton
            // 
            this.mGetTeamButton.Location = new System.Drawing.Point(13, 74);
            this.mGetTeamButton.Name = "mGetTeamButton";
            this.mGetTeamButton.Size = new System.Drawing.Size(162, 23);
            this.mGetTeamButton.TabIndex = 12;
            this.mGetTeamButton.Text = "Get Team (enter name above)";
            this.mGetTeamButton.UseVisualStyleBackColor = true;
            this.mGetTeamButton.Click += new System.EventHandler(this.mGetTeamButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.mGetBytesTextBox);
            this.groupBox3.Controls.Add(this.mGetBytesButton);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.mSetByteLocUpDown);
            this.groupBox3.Controls.Add(this.mSetByteButton);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.mSetByteValTextBox);
            this.groupBox3.Location = new System.Drawing.Point(3, 304);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(502, 37);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Set Bytes";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(464, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "bytes";
            // 
            // mGetBytesTextBox
            // 
            this.mGetBytesTextBox.Location = new System.Drawing.Point(406, 13);
            this.mGetBytesTextBox.MaxLength = 80;
            this.mGetBytesTextBox.Name = "mGetBytesTextBox";
            this.mGetBytesTextBox.Size = new System.Drawing.Size(52, 20);
            this.mGetBytesTextBox.TabIndex = 24;
            // 
            // mGetBytesButton
            // 
            this.mGetBytesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mGetBytesButton.Location = new System.Drawing.Point(342, 10);
            this.mGetBytesButton.Name = "mGetBytesButton";
            this.mGetBytesButton.Size = new System.Drawing.Size(61, 23);
            this.mGetBytesButton.TabIndex = 23;
            this.mGetBytesButton.Text = "Get";
            this.mGetBytesButton.UseVisualStyleBackColor = true;
            this.mGetBytesButton.Click += new System.EventHandler(this.mGetBytesButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Loc";
            // 
            // mSetByteLocUpDown
            // 
            this.mSetByteLocUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mSetByteLocUpDown.Hexadecimal = true;
            this.mSetByteLocUpDown.Location = new System.Drawing.Point(36, 14);
            this.mSetByteLocUpDown.Maximum = new decimal(new int[] {
            820032,
            0,
            0,
            0});
            this.mSetByteLocUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.mSetByteLocUpDown.Name = "mSetByteLocUpDown";
            this.mSetByteLocUpDown.Size = new System.Drawing.Size(83, 20);
            this.mSetByteLocUpDown.TabIndex = 21;
            this.mSetByteLocUpDown.Value = new decimal(new int[] {
            45731,
            0,
            0,
            0});
            this.mSetByteLocUpDown.ValueChanged += new System.EventHandler(this.mSetByteLocUpDown_ValueChanged);
            // 
            // mSetByteButton
            // 
            this.mSetByteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mSetByteButton.Location = new System.Drawing.Point(267, 10);
            this.mSetByteButton.Name = "mSetByteButton";
            this.mSetByteButton.Size = new System.Drawing.Size(61, 23);
            this.mSetByteButton.TabIndex = 21;
            this.mSetByteButton.Text = "Set";
            this.mSetByteButton.UseVisualStyleBackColor = true;
            this.mSetByteButton.Click += new System.EventHandler(this.mSetByteButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(124, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Val (hex)";
            // 
            // mSetByteValTextBox
            // 
            this.mSetByteValTextBox.Location = new System.Drawing.Point(178, 12);
            this.mSetByteValTextBox.MaxLength = 8000;
            this.mSetByteValTextBox.Name = "mSetByteValTextBox";
            this.mSetByteValTextBox.Size = new System.Drawing.Size(83, 20);
            this.mSetByteValTextBox.TabIndex = 1;
            this.mSetByteValTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mSetByteValTextBox_KeyDown);
            // 
            // mFindBytesButton
            // 
            this.mFindBytesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mFindBytesButton.Location = new System.Drawing.Point(244, 74);
            this.mFindBytesButton.Name = "mFindBytesButton";
            this.mFindBytesButton.Size = new System.Drawing.Size(124, 23);
            this.mFindBytesButton.TabIndex = 15;
            this.mFindBytesButton.Text = "Find bytes in ROM";
            this.mFindBytesButton.UseVisualStyleBackColor = true;
            this.mFindBytesButton.Click += new System.EventHandler(this.mFindBytesButton_Click);
            // 
            // mStatusLabel
            // 
            this.mStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mStatusLabel.AutoSize = true;
            this.mStatusLabel.ForeColor = System.Drawing.Color.Red;
            this.mStatusLabel.Location = new System.Drawing.Point(389, 57);
            this.mStatusLabel.Name = "mStatusLabel";
            this.mStatusLabel.Size = new System.Drawing.Size(0, 13);
            this.mStatusLabel.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "input (hex)";
            // 
            // mToUpperCheckBox
            // 
            this.mToUpperCheckBox.AutoSize = true;
            this.mToUpperCheckBox.Location = new System.Drawing.Point(11, 53);
            this.mToUpperCheckBox.Name = "mToUpperCheckBox";
            this.mToUpperCheckBox.Size = new System.Drawing.Size(106, 17);
            this.mToUpperCheckBox.TabIndex = 25;
            this.mToUpperCheckBox.Text = "Force uppercase";
            this.mToUpperCheckBox.UseVisualStyleBackColor = true;
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(446, 351);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 26;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            // 
            // getPositionsToolStripMenuItem
            // 
            this.getPositionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.getNowToolStripMenuItem,
            this.qbsItem,
            this.rbsItem,
            this.wrsItem,
            this.tesItem,
            this.olItem,
            this.dlItem,
            this.lbsItem,
            this.cbsItem,
            this.sItem,
            this.kItem,
            this.pItem,
            this.teamItem});
            this.getPositionsToolStripMenuItem.Name = "getPositionsToolStripMenuItem";
            this.getPositionsToolStripMenuItem.Size = new System.Drawing.Size(88, 20);
            this.getPositionsToolStripMenuItem.Text = "Get Positions";
            // 
            // getNowToolStripMenuItem
            // 
            this.getNowToolStripMenuItem.Name = "getNowToolStripMenuItem";
            this.getNowToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.getNowToolStripMenuItem.Text = "Get Now";
            this.getNowToolStripMenuItem.Click += new System.EventHandler(this.getNowToolStripMenuItem_Click);
            // 
            // qbsItem
            // 
            this.qbsItem.Checked = true;
            this.qbsItem.CheckOnClick = true;
            this.qbsItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.qbsItem.Name = "qbsItem";
            this.qbsItem.Size = new System.Drawing.Size(181, 22);
            this.qbsItem.Text = "QBs";
            // 
            // rbsItem
            // 
            this.rbsItem.CheckOnClick = true;
            this.rbsItem.Name = "rbsItem";
            this.rbsItem.Size = new System.Drawing.Size(181, 22);
            this.rbsItem.Text = "RBs";
            // 
            // wrsItem
            // 
            this.wrsItem.CheckOnClick = true;
            this.wrsItem.Name = "wrsItem";
            this.wrsItem.Size = new System.Drawing.Size(181, 22);
            this.wrsItem.Text = "WRs";
            // 
            // tesItem
            // 
            this.tesItem.CheckOnClick = true;
            this.tesItem.Name = "tesItem";
            this.tesItem.Size = new System.Drawing.Size(181, 22);
            this.tesItem.Text = "TEs";
            // 
            // olItem
            // 
            this.olItem.CheckOnClick = true;
            this.olItem.Name = "olItem";
            this.olItem.Size = new System.Drawing.Size(181, 22);
            this.olItem.Text = "OL";
            // 
            // dlItem
            // 
            this.dlItem.CheckOnClick = true;
            this.dlItem.Name = "dlItem";
            this.dlItem.Size = new System.Drawing.Size(181, 22);
            this.dlItem.Text = "DL";
            // 
            // lbsItem
            // 
            this.lbsItem.CheckOnClick = true;
            this.lbsItem.Name = "lbsItem";
            this.lbsItem.Size = new System.Drawing.Size(181, 22);
            this.lbsItem.Text = "LBs";
            // 
            // cbsItem
            // 
            this.cbsItem.CheckOnClick = true;
            this.cbsItem.Name = "cbsItem";
            this.cbsItem.Size = new System.Drawing.Size(181, 22);
            this.cbsItem.Text = "CBs";
            // 
            // sItem
            // 
            this.sItem.CheckOnClick = true;
            this.sItem.Name = "sItem";
            this.sItem.Size = new System.Drawing.Size(181, 22);
            this.sItem.Text = "Safety";
            // 
            // kItem
            // 
            this.kItem.CheckOnClick = true;
            this.kItem.Name = "kItem";
            this.kItem.Size = new System.Drawing.Size(181, 22);
            this.kItem.Text = "K";
            // 
            // pItem
            // 
            this.pItem.CheckOnClick = true;
            this.pItem.Name = "pItem";
            this.pItem.Size = new System.Drawing.Size(181, 22);
            this.pItem.Text = "P";
            // 
            // mResultsTextBox
            // 
            this.mResultsTextBox.AcceptsTab = true;
            this.mResultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mResultsTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mResultsTextBox.Location = new System.Drawing.Point(11, 134);
            this.mResultsTextBox.Name = "mResultsTextBox";
            this.mResultsTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.mResultsTextBox.SearchString = null;
            this.mResultsTextBox.Size = new System.Drawing.Size(497, 164);
            this.mResultsTextBox.StatusControl = null;
            this.mResultsTextBox.TabIndex = 3;
            this.mResultsTextBox.Text = "";
            // 
            // teamItem
            // 
            this.teamItem.CheckOnClick = true;
            this.teamItem.Name = "teamItem";
            this.teamItem.Size = new System.Drawing.Size(181, 22);
            this.teamItem.Text = "Include Team Name";
            // 
            // DebugDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(522, 349);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mToUpperCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mStatusLabel);
            this.Controls.Add(this.mFindBytesButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.mGetTeamButton);
            this.Controls.Add(this.mFindButton);
            this.Controls.Add(this.mResultsTextBox);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.mInputTextBox);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DebugDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Debug Dialog";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mSetByteLocUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox mInputTextBox;
        private System.Windows.Forms.TextBox textBox2;
        private SearchTextBox mResultsTextBox;
        private System.Windows.Forms.Button mFindButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.Button mGetTeamButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox mSetByteValTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown mSetByteLocUpDown;
        private System.Windows.Forms.Button mSetByteButton;
        private System.Windows.Forms.Button mFindBytesButton;
        private System.Windows.Forms.ToolStripMenuItem mathTestToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox mGetBytesTextBox;
        private System.Windows.Forms.Button mGetBytesButton;
        private System.Windows.Forms.ToolStripMenuItem miscToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceStringToolStripMenuItem;
        private System.Windows.Forms.Label mStatusLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem findStringsToolStripMenuItem;
        private System.Windows.Forms.CheckBox mToUpperCheckBox;
        private System.Windows.Forms.ToolStripMenuItem replaceStringsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stringTableSetTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stringTableGetTestToolStripMenuItem;
        private System.Windows.Forms.Button mCancelButton;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem tSB2StringsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem convertAttributesToBytesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem getSpecialLocationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playBookToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playSelectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem conversionTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scheduleHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getPositionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem qbsItem;
        private System.Windows.Forms.ToolStripMenuItem rbsItem;
        private System.Windows.Forms.ToolStripMenuItem wrsItem;
        private System.Windows.Forms.ToolStripMenuItem tesItem;
        private System.Windows.Forms.ToolStripMenuItem olItem;
        private System.Windows.Forms.ToolStripMenuItem dlItem;
        private System.Windows.Forms.ToolStripMenuItem lbsItem;
        private System.Windows.Forms.ToolStripMenuItem cbsItem;
        private System.Windows.Forms.ToolStripMenuItem sItem;
        private System.Windows.Forms.ToolStripMenuItem kItem;
        private System.Windows.Forms.ToolStripMenuItem pItem;
        private System.Windows.Forms.ToolStripMenuItem teamItem;
    }
}