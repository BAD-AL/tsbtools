namespace TSBTool2_UI
{
    partial class TeamForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TeamForm));
            this.mSimDataTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.mPlaybookTextBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mPlaybook2Button = new System.Windows.Forms.Button();
            this.mPlaybook1Button = new System.Windows.Forms.Button();
            this.R4 = new System.Windows.Forms.PictureBox();
            this.R3 = new System.Windows.Forms.PictureBox();
            this.R2 = new System.Windows.Forms.PictureBox();
            this.R1 = new System.Windows.Forms.PictureBox();
            this.P4 = new System.Windows.Forms.PictureBox();
            this.P3 = new System.Windows.Forms.PictureBox();
            this.P2 = new System.Windows.Forms.PictureBox();
            this.P1 = new System.Windows.Forms.PictureBox();
            this.m_TeamsComboBox = new System.Windows.Forms.ComboBox();
            this.leftSidePanel = new System.Windows.Forms.Panel();
            this.sidePanel = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.mAbbreviationTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.mCityTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.mTeamNameTextBox = new System.Windows.Forms.TextBox();
            this.mOkButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.mAutoUpdateTeamSimDataButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.R4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.R3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.R2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.R1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.P4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.P3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.P2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.P1)).BeginInit();
            this.SuspendLayout();
            // 
            // mSimDataTextBox
            // 
            this.mSimDataTextBox.BackColor = System.Drawing.Color.PaleTurquoise;
            this.mSimDataTextBox.Location = new System.Drawing.Point(85, 511);
            this.mSimDataTextBox.Name = "mSimDataTextBox";
            this.mSimDataTextBox.Size = new System.Drawing.Size(86, 20);
            this.mSimDataTextBox.TabIndex = 3;
            this.mSimDataTextBox.Leave += new System.EventHandler(this.teamValueTextBox_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(2, 514);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "SimData";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(247, 517);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Playbook";
            // 
            // mPlaybookTextBox
            // 
            this.mPlaybookTextBox.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.mPlaybookTextBox.Location = new System.Drawing.Point(330, 514);
            this.mPlaybookTextBox.Name = "mPlaybookTextBox";
            this.mPlaybookTextBox.ReadOnly = true;
            this.mPlaybookTextBox.Size = new System.Drawing.Size(222, 20);
            this.mPlaybookTextBox.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Controls.Add(this.mPlaybook2Button);
            this.panel1.Controls.Add(this.mPlaybook1Button);
            this.panel1.Controls.Add(this.R4);
            this.panel1.Controls.Add(this.R3);
            this.panel1.Controls.Add(this.R2);
            this.panel1.Controls.Add(this.R1);
            this.panel1.Controls.Add(this.P4);
            this.panel1.Controls.Add(this.P3);
            this.panel1.Controls.Add(this.P2);
            this.panel1.Controls.Add(this.P1);
            this.panel1.Controls.Add(this.m_TeamsComboBox);
            this.panel1.Location = new System.Drawing.Point(13, 9);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(644, 496);
            this.panel1.TabIndex = 0;
            // 
            // mPlaybook2Button
            // 
            this.mPlaybook2Button.BackColor = System.Drawing.Color.White;
            this.mPlaybook2Button.ForeColor = System.Drawing.Color.Black;
            this.mPlaybook2Button.Location = new System.Drawing.Point(334, 20);
            this.mPlaybook2Button.Name = "mPlaybook2Button";
            this.mPlaybook2Button.Size = new System.Drawing.Size(33, 26);
            this.mPlaybook2Button.TabIndex = 9;
            this.mPlaybook2Button.Text = "2";
            this.mPlaybook2Button.UseVisualStyleBackColor = false;
            this.mPlaybook2Button.Click += new System.EventHandler(this.playbookButton_Click);
            // 
            // mPlaybook1Button
            // 
            this.mPlaybook1Button.BackColor = System.Drawing.Color.Gold;
            this.mPlaybook1Button.ForeColor = System.Drawing.Color.Black;
            this.mPlaybook1Button.Location = new System.Drawing.Point(295, 20);
            this.mPlaybook1Button.Name = "mPlaybook1Button";
            this.mPlaybook1Button.Size = new System.Drawing.Size(33, 26);
            this.mPlaybook1Button.TabIndex = 8;
            this.mPlaybook1Button.Text = "1";
            this.mPlaybook1Button.UseVisualStyleBackColor = false;
            this.mPlaybook1Button.Click += new System.EventHandler(this.playbookButton_Click);
            // 
            // R4
            // 
            this.R4.Location = new System.Drawing.Point(421, 327);
            this.R4.Name = "R4";
            this.R4.Size = new System.Drawing.Size(123, 118);
            this.R4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.R4.TabIndex = 7;
            this.R4.TabStop = false;
            this.R4.Tag = "0";
            this.R4.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // R3
            // 
            this.R3.Location = new System.Drawing.Point(500, 194);
            this.R3.Name = "R3";
            this.R3.Size = new System.Drawing.Size(123, 118);
            this.R3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.R3.TabIndex = 6;
            this.R3.TabStop = false;
            this.R3.Tag = "0";
            this.R3.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // R2
            // 
            this.R2.Location = new System.Drawing.Point(339, 194);
            this.R2.Name = "R2";
            this.R2.Size = new System.Drawing.Size(123, 118);
            this.R2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.R2.TabIndex = 5;
            this.R2.TabStop = false;
            this.R2.Tag = "0";
            this.R2.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // R1
            // 
            this.R1.Location = new System.Drawing.Point(421, 61);
            this.R1.Name = "R1";
            this.R1.Size = new System.Drawing.Size(123, 118);
            this.R1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.R1.TabIndex = 5;
            this.R1.TabStop = false;
            this.R1.Tag = "0";
            this.R1.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // P4
            // 
            this.P4.Location = new System.Drawing.Point(99, 327);
            this.P4.Name = "P4";
            this.P4.Size = new System.Drawing.Size(123, 118);
            this.P4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.P4.TabIndex = 4;
            this.P4.TabStop = false;
            this.P4.Tag = "0";
            this.P4.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // P3
            // 
            this.P3.Location = new System.Drawing.Point(179, 194);
            this.P3.Name = "P3";
            this.P3.Size = new System.Drawing.Size(123, 118);
            this.P3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.P3.TabIndex = 3;
            this.P3.TabStop = false;
            this.P3.Tag = "0";
            this.P3.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // P2
            // 
            this.P2.Location = new System.Drawing.Point(16, 194);
            this.P2.Name = "P2";
            this.P2.Size = new System.Drawing.Size(123, 118);
            this.P2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.P2.TabIndex = 2;
            this.P2.TabStop = false;
            this.P2.Tag = "0";
            this.P2.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // P1
            // 
            this.P1.Location = new System.Drawing.Point(99, 61);
            this.P1.Name = "P1";
            this.P1.Size = new System.Drawing.Size(123, 118);
            this.P1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.P1.TabIndex = 1;
            this.P1.TabStop = false;
            this.P1.Tag = "0";
            this.P1.Click += new System.EventHandler(this.pictureBox_Click);
            // 
            // m_TeamsComboBox
            // 
            this.m_TeamsComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(41)))), ((int)(((byte)(89)))));
            this.m_TeamsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_TeamsComboBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_TeamsComboBox.ForeColor = System.Drawing.Color.White;
            this.m_TeamsComboBox.FormattingEnabled = true;
            this.m_TeamsComboBox.Location = new System.Drawing.Point(399, 18);
            this.m_TeamsComboBox.Name = "m_TeamsComboBox";
            this.m_TeamsComboBox.Size = new System.Drawing.Size(160, 26);
            this.m_TeamsComboBox.TabIndex = 0;
            this.m_TeamsComboBox.SelectedIndexChanged += new System.EventHandler(this.m_TeamsComboBox_SelectedIndexChanged);
            // 
            // leftSidePanel
            // 
            this.leftSidePanel.Location = new System.Drawing.Point(-3, 9);
            this.leftSidePanel.Name = "leftSidePanel";
            this.leftSidePanel.Size = new System.Drawing.Size(22, 496);
            this.leftSidePanel.TabIndex = 2;
            // 
            // sidePanel
            // 
            this.sidePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.sidePanel.Location = new System.Drawing.Point(655, 9);
            this.sidePanel.Name = "sidePanel";
            this.sidePanel.Size = new System.Drawing.Size(62, 496);
            this.sidePanel.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(11, 551);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "Abb";
            // 
            // mAbbreviationTextBox
            // 
            this.mAbbreviationTextBox.BackColor = System.Drawing.Color.PaleTurquoise;
            this.mAbbreviationTextBox.Location = new System.Drawing.Point(54, 549);
            this.mAbbreviationTextBox.Name = "mAbbreviationTextBox";
            this.mAbbreviationTextBox.Size = new System.Drawing.Size(117, 20);
            this.mAbbreviationTextBox.TabIndex = 7;
            this.mAbbreviationTextBox.Leave += new System.EventHandler(this.teamValueTextBox_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(11, 577);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 10;
            this.label4.Text = "City";
            // 
            // mCityTextBox
            // 
            this.mCityTextBox.BackColor = System.Drawing.Color.PaleTurquoise;
            this.mCityTextBox.Location = new System.Drawing.Point(54, 575);
            this.mCityTextBox.Name = "mCityTextBox";
            this.mCityTextBox.Size = new System.Drawing.Size(117, 20);
            this.mCityTextBox.TabIndex = 9;
            this.mCityTextBox.Leave += new System.EventHandler(this.teamValueTextBox_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(11, 603);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 16);
            this.label5.TabIndex = 12;
            this.label5.Text = "Name";
            // 
            // mTeamNameTextBox
            // 
            this.mTeamNameTextBox.BackColor = System.Drawing.Color.PaleTurquoise;
            this.mTeamNameTextBox.Location = new System.Drawing.Point(54, 601);
            this.mTeamNameTextBox.Name = "mTeamNameTextBox";
            this.mTeamNameTextBox.Size = new System.Drawing.Size(117, 20);
            this.mTeamNameTextBox.TabIndex = 11;
            this.mTeamNameTextBox.Leave += new System.EventHandler(this.teamValueTextBox_Leave);
            // 
            // mOkButton
            // 
            this.mOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mOkButton.BackColor = System.Drawing.Color.Silver;
            this.mOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.mOkButton.Location = new System.Drawing.Point(496, 625);
            this.mOkButton.Name = "mOkButton";
            this.mOkButton.Size = new System.Drawing.Size(99, 30);
            this.mOkButton.TabIndex = 13;
            this.mOkButton.Text = "&OK";
            this.mOkButton.UseVisualStyleBackColor = false;
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.BackColor = System.Drawing.Color.Silver;
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(601, 625);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(99, 30);
            this.mCancelButton.TabIndex = 14;
            this.mCancelButton.Text = "&Cancel";
            this.mCancelButton.UseVisualStyleBackColor = false;
            // 
            // mAutoUpdateTeamSimDataButton
            // 
            this.mAutoUpdateTeamSimDataButton.BackColor = System.Drawing.Color.DodgerBlue;
            this.mAutoUpdateTeamSimDataButton.Location = new System.Drawing.Point(333, 575);
            this.mAutoUpdateTeamSimDataButton.Name = "mAutoUpdateTeamSimDataButton";
            this.mAutoUpdateTeamSimDataButton.Size = new System.Drawing.Size(142, 58);
            this.mAutoUpdateTeamSimDataButton.TabIndex = 15;
            this.mAutoUpdateTeamSimDataButton.Text = "Auto Update All Teams Sim Data";
            this.mAutoUpdateTeamSimDataButton.UseVisualStyleBackColor = false;
            this.mAutoUpdateTeamSimDataButton.Click += new System.EventHandler(this.updateTeamsSimButton_Click);
            // 
            // TeamForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(41)))), ((int)(((byte)(89)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(712, 667);
            this.Controls.Add(this.mAutoUpdateTeamSimDataButton);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOkButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.mTeamNameTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.mCityTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.mAbbreviationTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.mPlaybookTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mSimDataTextBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.leftSidePanel);
            this.Controls.Add(this.sidePanel);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TeamForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify Team Attributes";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.R4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.R3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.R2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.R1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.P4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.P3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.P2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.P1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel sidePanel;
        private System.Windows.Forms.Panel leftSidePanel;
        private System.Windows.Forms.ComboBox m_TeamsComboBox;
        private System.Windows.Forms.PictureBox P4;
        private System.Windows.Forms.PictureBox P3;
        private System.Windows.Forms.PictureBox P2;
        private System.Windows.Forms.PictureBox P1;
        private System.Windows.Forms.Button mPlaybook2Button;
        private System.Windows.Forms.Button mPlaybook1Button;
        private System.Windows.Forms.PictureBox R4;
        private System.Windows.Forms.PictureBox R3;
        private System.Windows.Forms.PictureBox R2;
        private System.Windows.Forms.PictureBox R1;
        private System.Windows.Forms.TextBox mSimDataTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mPlaybookTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox mAbbreviationTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox mCityTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox mTeamNameTextBox;
        private System.Windows.Forms.Button mOkButton;
        private System.Windows.Forms.Button mCancelButton;
        private System.Windows.Forms.Button mAutoUpdateTeamSimDataButton;


    }
}