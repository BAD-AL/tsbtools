namespace TSBTool
{
    partial class AllStarForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AllStarForm));
            this.mOKButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.mAFCConfrenceControl = new TSBTool.AllStarConfrenceControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.mNFCConfrenceControl = new TSBTool.AllStarConfrenceControl();
            this.mToggleNumberOfComboBoxesButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mOKButton
            // 
            this.mOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mOKButton.Location = new System.Drawing.Point(527, 372);
            this.mOKButton.Name = "mOKButton";
            this.mOKButton.Size = new System.Drawing.Size(75, 23);
            this.mOKButton.TabIndex = 2;
            this.mOKButton.Text = "OK";
            this.mOKButton.UseVisualStyleBackColor = true;
            this.mOKButton.Click += new System.EventHandler(this.mOKButton_Click);
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(608, 372);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 3;
            this.mCancelButton.Text = "&Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(2, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(691, 363);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.AutoScroll = true;
            this.tabPage1.Controls.Add(this.mAFCConfrenceControl);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(683, 337);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "AFC";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // mAFCConfrenceControl
            // 
            this.mAFCConfrenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mAFCConfrenceControl.AutoScroll = true;
            this.mAFCConfrenceControl.Conference = TSBTool.Conference.AFC;
            this.mAFCConfrenceControl.Data = null;
            this.mAFCConfrenceControl.Location = new System.Drawing.Point(3, 3);
            this.mAFCConfrenceControl.Name = "mAFCConfrenceControl";
            this.mAFCConfrenceControl.Size = new System.Drawing.Size(674, 328);
            this.mAFCConfrenceControl.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.Controls.Add(this.mNFCConfrenceControl);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(683, 337);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "NFC";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // mNFCConfrenceControl
            // 
            this.mNFCConfrenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mNFCConfrenceControl.AutoScroll = true;
            this.mNFCConfrenceControl.Conference = TSBTool.Conference.NFC;
            this.mNFCConfrenceControl.Data = null;
            this.mNFCConfrenceControl.Location = new System.Drawing.Point(3, 3);
            this.mNFCConfrenceControl.Name = "mNFCConfrenceControl";
            this.mNFCConfrenceControl.Size = new System.Drawing.Size(674, 328);
            this.mNFCConfrenceControl.TabIndex = 1;
            // 
            // mToggleNumberOfComboBoxesButton
            // 
            this.mToggleNumberOfComboBoxesButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mToggleNumberOfComboBoxesButton.Location = new System.Drawing.Point(12, 368);
            this.mToggleNumberOfComboBoxesButton.Name = "mToggleNumberOfComboBoxesButton";
            this.mToggleNumberOfComboBoxesButton.Size = new System.Drawing.Size(127, 23);
            this.mToggleNumberOfComboBoxesButton.TabIndex = 5;
            this.mToggleNumberOfComboBoxesButton.Text = "button1";
            this.mToggleNumberOfComboBoxesButton.UseVisualStyleBackColor = true;
            this.mToggleNumberOfComboBoxesButton.Click += new System.EventHandler(this.mToggleNumberOfComboBoxesButton_Click);
            // 
            // AllStarForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(695, 395);
            this.Controls.Add(this.mToggleNumberOfComboBoxesButton);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOKButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(469, 433);
            this.Name = "AllStarForm";
            this.Text = "Pro Bowl";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button mOKButton;
        private System.Windows.Forms.Button mCancelButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private AllStarConfrenceControl mAFCConfrenceControl;
        private AllStarConfrenceControl mNFCConfrenceControl;
        private System.Windows.Forms.Button mToggleNumberOfComboBoxesButton;
    }
}