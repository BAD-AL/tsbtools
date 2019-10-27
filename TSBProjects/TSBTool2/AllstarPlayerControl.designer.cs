namespace TSBTool2_UI
{
    partial class AllstarPlayerControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mPositionLabel = new System.Windows.Forms.Label();
            this.mTeamComboBox = new System.Windows.Forms.ComboBox();
            this.mPositionComboBox = new System.Windows.Forms.ComboBox();
            this.mPlayerNameLabel = new System.Windows.Forms.Label();
            this.mTeamPosComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // mPositionLabel
            // 
            this.mPositionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mPositionLabel.Location = new System.Drawing.Point(3, 9);
            this.mPositionLabel.Name = "mPositionLabel";
            this.mPositionLabel.Size = new System.Drawing.Size(42, 17);
            this.mPositionLabel.TabIndex = 0;
            this.mPositionLabel.Text = "QB1";
            // 
            // mTeamComboBox
            // 
            this.mTeamComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mTeamComboBox.FormattingEnabled = true;
            this.mTeamComboBox.Location = new System.Drawing.Point(64, 5);
            this.mTeamComboBox.Name = "mTeamComboBox";
            this.mTeamComboBox.Size = new System.Drawing.Size(87, 21);
            this.mTeamComboBox.TabIndex = 1;
            this.mTeamComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // mPositionComboBox
            // 
            this.mPositionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mPositionComboBox.FormattingEnabled = true;
            this.mPositionComboBox.Location = new System.Drawing.Point(157, 5);
            this.mPositionComboBox.Name = "mPositionComboBox";
            this.mPositionComboBox.Size = new System.Drawing.Size(96, 21);
            this.mPositionComboBox.TabIndex = 2;
            this.mPositionComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // mPlayerNameLabel
            // 
            this.mPlayerNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mPlayerNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mPlayerNameLabel.Location = new System.Drawing.Point(259, 3);
            this.mPlayerNameLabel.Name = "mPlayerNameLabel";
            this.mPlayerNameLabel.Size = new System.Drawing.Size(175, 23);
            this.mPlayerNameLabel.TabIndex = 3;
            this.mPlayerNameLabel.Text = "Joe Nobody";
            this.mPlayerNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mTeamPosComboBox
            // 
            this.mTeamPosComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mTeamPosComboBox.FormattingEnabled = true;
            this.mTeamPosComboBox.Location = new System.Drawing.Point(64, 5);
            this.mTeamPosComboBox.Name = "mTeamPosComboBox";
            this.mTeamPosComboBox.Size = new System.Drawing.Size(122, 21);
            this.mTeamPosComboBox.TabIndex = 4;
            this.mTeamPosComboBox.SelectedIndexChanged += new System.EventHandler(this.mTeamPosComboBox_SelectedIndexChanged);
            // 
            // AllstarPlayerControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.mTeamPosComboBox);
            this.Controls.Add(this.mPlayerNameLabel);
            this.Controls.Add(this.mPositionComboBox);
            this.Controls.Add(this.mTeamComboBox);
            this.Controls.Add(this.mPositionLabel);
            this.Name = "AllstarPlayerControl";
            this.Size = new System.Drawing.Size(437, 31);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label mPositionLabel;
        private System.Windows.Forms.ComboBox mTeamComboBox;
        private System.Windows.Forms.ComboBox mPositionComboBox;
        private System.Windows.Forms.Label mPlayerNameLabel;
        private System.Windows.Forms.ComboBox mTeamPosComboBox;
    }
}
