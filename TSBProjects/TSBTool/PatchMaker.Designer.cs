namespace TSBTool
{
    partial class PatchMaker
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
            this.components = new System.ComponentModel.Container();
            this.mBaseTextBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.mModifiedTextBox = new System.Windows.Forms.TextBox();
            this.mBrowseBaseButton = new System.Windows.Forms.Button();
            this.mBrowseModifiedButton = new System.Windows.Forms.Button();
            this.mResulTextBox = new System.Windows.Forms.TextBox();
            this.mGoButton = new System.Windows.Forms.Button();
            this.ParentItem = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mCancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mBaseTextBox
            // 
            this.mBaseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mBaseTextBox.Location = new System.Drawing.Point(66, 12);
            this.mBaseTextBox.Multiline = false;
            this.mBaseTextBox.Name = "mBaseTextBox";
            this.mBaseTextBox.Size = new System.Drawing.Size(288, 20);
            this.mBaseTextBox.TabIndex = 0;
            this.mBaseTextBox.Text = "";
            this.mBaseTextBox.TextChanged += new System.EventHandler(this.mTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Base";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Modified";
            // 
            // mModifiedTextBox
            // 
            this.mModifiedTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mModifiedTextBox.Location = new System.Drawing.Point(66, 48);
            this.mModifiedTextBox.Name = "mModifiedTextBox";
            this.mModifiedTextBox.Size = new System.Drawing.Size(288, 20);
            this.mModifiedTextBox.TabIndex = 2;
            this.mModifiedTextBox.TextChanged += new System.EventHandler(this.mTextBox_TextChanged);
            // 
            // mBrowseBaseButton
            // 
            this.mBrowseBaseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mBrowseBaseButton.Location = new System.Drawing.Point(360, 12);
            this.mBrowseBaseButton.Name = "mBrowseBaseButton";
            this.mBrowseBaseButton.Size = new System.Drawing.Size(31, 23);
            this.mBrowseBaseButton.TabIndex = 1;
            this.mBrowseBaseButton.Text = "...";
            this.mBrowseBaseButton.UseVisualStyleBackColor = true;
            this.mBrowseBaseButton.Click += new System.EventHandler(this.mBrowseBaseButton_Click);
            // 
            // mBrowseModifiedButton
            // 
            this.mBrowseModifiedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mBrowseModifiedButton.Location = new System.Drawing.Point(360, 46);
            this.mBrowseModifiedButton.Name = "mBrowseModifiedButton";
            this.mBrowseModifiedButton.Size = new System.Drawing.Size(31, 23);
            this.mBrowseModifiedButton.TabIndex = 3;
            this.mBrowseModifiedButton.Text = "...";
            this.mBrowseModifiedButton.UseVisualStyleBackColor = true;
            this.mBrowseModifiedButton.Click += new System.EventHandler(this.mBrowseModifiedButton_Click);
            // 
            // mResulTextBox
            // 
            this.mResulTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mResulTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mResulTextBox.Location = new System.Drawing.Point(3, 100);
            this.mResulTextBox.Multiline = true;
            this.mResulTextBox.Name = "mResulTextBox";
            this.mResulTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.mResulTextBox.Size = new System.Drawing.Size(388, 330);
            this.mResulTextBox.TabIndex = 10;
            // 
            // mGoButton
            // 
            this.mGoButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mGoButton.Enabled = false;
            this.mGoButton.Location = new System.Drawing.Point(66, 74);
            this.mGoButton.Name = "mGoButton";
            this.mGoButton.Size = new System.Drawing.Size(288, 23);
            this.mGoButton.TabIndex = 7;
            this.mGoButton.Text = "&GO!";
            this.mGoButton.UseVisualStyleBackColor = true;
            this.mGoButton.Click += new System.EventHandler(this.mGoButton_Click);
            // 
            // ParentItem
            // 
            this.ParentItem.Name = "ParentItem";
            this.ParentItem.Size = new System.Drawing.Size(61, 4);
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(386, 449);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 11;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            // 
            // PatchMaker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(412, 433);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mGoButton);
            this.Controls.Add(this.mResulTextBox);
            this.Controls.Add(this.mBrowseModifiedButton);
            this.Controls.Add(this.mBrowseBaseButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.mModifiedTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mBaseTextBox);
            this.Name = "PatchMaker";
            this.Text = "PatchMaker";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox mBaseTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox mModifiedTextBox;
        private System.Windows.Forms.Button mBrowseBaseButton;
        private System.Windows.Forms.Button mBrowseModifiedButton;
        private System.Windows.Forms.TextBox mResulTextBox;
        private System.Windows.Forms.Button mGoButton;
        private System.Windows.Forms.ContextMenuStrip ParentItem;
        private System.Windows.Forms.Button mCancelButton;
    }
}