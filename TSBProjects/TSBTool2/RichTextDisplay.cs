using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TSBTool2
{
	/// <summary>
	/// Summary description for RichTextDisplay.
	/// </summary>
	public class RichTextDisplay : System.Windows.Forms.Form
	{
		private System.Windows.Forms.RichTextBox richTextBox;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem fileMenu;
		private System.Windows.Forms.MenuItem mLoadMenuItem;
		private System.Windows.Forms.MenuItem mSaveMenuItem;
		private System.Windows.Forms.MenuItem mExitMenuItem;
        private System.Windows.Forms.MenuItem mSaveAsMenuItem;
        private IContainer components;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem mContextLoadMenuItem;
		private System.Windows.Forms.MenuItem mContextSaveMenuItem;
		private System.Windows.Forms.MenuItem mContextSaveAsMenuItem;
		private System.Windows.Forms.MenuItem editMenuItem;
		private System.Windows.Forms.MenuItem mCopyMenuItem;
		private System.Windows.Forms.MenuItem mPasteMenuItem;
		private System.Windows.Forms.MenuItem mCopyMenuItemC;
        private Button mCancelButton;
        private Button mOkButton;
		private System.Windows.Forms.MenuItem mPateMenuItemC;
        private System.Windows.Forms.MenuItem mClearMenuItem;

		public RichTextBox ContentBox
		{
			get { return this.richTextBox; }
		}

        /// <summary>
        /// The text to show 
        /// </summary>
        public string MessageText
        {
            get { return richTextBox.Text; }
            set { richTextBox.Text = value; }
        }

        public bool MessageEditable
        {
            get { return !richTextBox.ReadOnly; }
            set { richTextBox.ReadOnly = !value; }
        }

        public bool ShowCancelButton
        {
            set { this.mCancelButton.Visible = value; }
        }

        /// <summary>
        /// Returns the MessageText of the Form
        /// </summary>
        /// <param name="title">The title bar text</param>
        /// <param name="message">The initial message text to display</param>
        /// <param name="icon">the Icon to use</param>
        /// <param name="editable">true to allow the user to edit the MessageText</param>
        /// <returns>The resulting MessageText</returns>
        public static string ShowMessage(string title, string message, Icon icon, bool editable, bool showCancelButton)
        {
            string retVal = null;
            RichTextDisplay mf = new RichTextDisplay();
            mf.Icon = icon;
            mf.MessageEditable = editable;
            mf.Text = title;
            mf.MessageText = message;
            mf.ShowCancelButton = showCancelButton;

            if (mf.ShowDialog() == DialogResult.OK)
            {
                retVal = mf.MessageText;
            }
            mf.Dispose();
            return retVal;
        }

        /// <summary>
        /// Returns the MessageText of the Form
        /// </summary>
        /// <param name="title">The title bar text</param>
        /// <param name="message">The initial message text to display</param>
        /// <param name="icon">the Icon to use</param>
        /// <param name="editable">true to allow the user to edit the MessageText</param>
        /// <returns>The resulting MessageText</returns>
        public static void ShowText(string title, string message, Icon icon, bool editable, bool showCancelButton)
        {
            RichTextDisplay mf = new RichTextDisplay();
            mf.Icon = icon;
            mf.MessageEditable = editable;
            mf.Text = title;
            mf.MessageText = message;
            mf.ShowCancelButton = showCancelButton;

            mf.Show();
        }


        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="title">the title bar text</param>
        /// <param name="message">the message to show</param>
        public static void ShowMessage(string title, string message)
        {
            ShowMessage(title, message, SystemIcons.Hand, false, false);
        }

		private RichTextBoxStreamType mRichTextStreamType = RichTextBoxStreamType.PlainText;

		public bool AllowRichText 
		{
			get
			{
				return mRichTextStreamType == RichTextBoxStreamType.RichText;
			}
			set
			{
				if( value)
				{
					mRichTextStreamType = RichTextBoxStreamType.RichText;
				}
				else
				{
					mRichTextStreamType = RichTextBoxStreamType.PlainText;
				}
			}
		}

		public RichTextDisplay()
		{
			InitializeComponent();
		}

		public void ShowFile(string fileName )
		{
			try
			{
				this.richTextBox.LoadFile(fileName,mRichTextStreamType);
			}
			catch
			{
				MessageBox.Show("Could not find file "+ fileName);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RichTextDisplay));
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.mContextLoadMenuItem = new System.Windows.Forms.MenuItem();
            this.mContextSaveMenuItem = new System.Windows.Forms.MenuItem();
            this.mContextSaveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.mCopyMenuItemC = new System.Windows.Forms.MenuItem();
            this.mPateMenuItemC = new System.Windows.Forms.MenuItem();
            this.mClearMenuItem = new System.Windows.Forms.MenuItem();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenu = new System.Windows.Forms.MenuItem();
            this.mLoadMenuItem = new System.Windows.Forms.MenuItem();
            this.mSaveMenuItem = new System.Windows.Forms.MenuItem();
            this.mSaveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.mExitMenuItem = new System.Windows.Forms.MenuItem();
            this.editMenuItem = new System.Windows.Forms.MenuItem();
            this.mCopyMenuItem = new System.Windows.Forms.MenuItem();
            this.mPasteMenuItem = new System.Windows.Forms.MenuItem();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.mOkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.AcceptsTab = true;
            this.richTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox.ContextMenu = this.contextMenu1;
            this.richTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox.Size = new System.Drawing.Size(692, 523);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            this.richTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox_KeyDown);
            this.richTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox_LinkClicked);
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mContextLoadMenuItem,
            this.mContextSaveMenuItem,
            this.mContextSaveAsMenuItem,
            this.mCopyMenuItemC,
            this.mPateMenuItemC,
            this.mClearMenuItem});
            // 
            // mContextLoadMenuItem
            // 
            this.mContextLoadMenuItem.Index = 0;
            this.mContextLoadMenuItem.Text = "&Load";
            this.mContextLoadMenuItem.Click += new System.EventHandler(this.mLoadMenuItem_Click);
            // 
            // mContextSaveMenuItem
            // 
            this.mContextSaveMenuItem.Index = 1;
            this.mContextSaveMenuItem.Text = "&Save";
            this.mContextSaveMenuItem.Click += new System.EventHandler(this.mSaveAsMenuItem_Click);
            // 
            // mContextSaveAsMenuItem
            // 
            this.mContextSaveAsMenuItem.Index = 2;
            this.mContextSaveAsMenuItem.Text = "Save &as";
            this.mContextSaveAsMenuItem.Click += new System.EventHandler(this.mSaveAsMenuItem_Click);
            // 
            // mCopyMenuItemC
            // 
            this.mCopyMenuItemC.Index = 3;
            this.mCopyMenuItemC.Text = "&Copy";
            this.mCopyMenuItemC.Click += new System.EventHandler(this.mCopyMenuItemC_Click);
            // 
            // mPateMenuItemC
            // 
            this.mPateMenuItemC.Index = 4;
            this.mPateMenuItemC.Text = "&Paste";
            this.mPateMenuItemC.Click += new System.EventHandler(this.mPasteMenuItem_Click);
            // 
            // mClearMenuItem
            // 
            this.mClearMenuItem.Index = 5;
            this.mClearMenuItem.Text = "&Clear";
            this.mClearMenuItem.Click += new System.EventHandler(this.mClearMenuItem_Click);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenu,
            this.editMenuItem});
            // 
            // fileMenu
            // 
            this.fileMenu.Index = 0;
            this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mLoadMenuItem,
            this.mSaveMenuItem,
            this.mSaveAsMenuItem,
            this.mExitMenuItem});
            this.fileMenu.Text = "&File";
            // 
            // mLoadMenuItem
            // 
            this.mLoadMenuItem.Index = 0;
            this.mLoadMenuItem.Text = "&Load";
            this.mLoadMenuItem.Click += new System.EventHandler(this.mLoadMenuItem_Click);
            // 
            // mSaveMenuItem
            // 
            this.mSaveMenuItem.Index = 1;
            this.mSaveMenuItem.Text = "&Save";
            this.mSaveMenuItem.Click += new System.EventHandler(this.mSaveMenuItem_Click);
            // 
            // mSaveAsMenuItem
            // 
            this.mSaveAsMenuItem.Index = 2;
            this.mSaveAsMenuItem.Text = "Save &as";
            this.mSaveAsMenuItem.Click += new System.EventHandler(this.mSaveAsMenuItem_Click);
            // 
            // mExitMenuItem
            // 
            this.mExitMenuItem.Index = 3;
            this.mExitMenuItem.Text = "E&xit";
            this.mExitMenuItem.Click += new System.EventHandler(this.mExitMenuItem_Click);
            // 
            // editMenuItem
            // 
            this.editMenuItem.Index = 1;
            this.editMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mCopyMenuItem,
            this.mPasteMenuItem});
            this.editMenuItem.Text = "&Edit";
            // 
            // mCopyMenuItem
            // 
            this.mCopyMenuItem.Index = 0;
            this.mCopyMenuItem.Text = "&Copy";
            this.mCopyMenuItem.Click += new System.EventHandler(this.mCopyMenuItemC_Click);
            // 
            // mPasteMenuItem
            // 
            this.mPasteMenuItem.Index = 1;
            this.mPasteMenuItem.Text = "&Paste";
            this.mPasteMenuItem.Click += new System.EventHandler(this.mPasteMenuItem_Click);
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(524, 536);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 1;
            this.mCancelButton.Text = "&Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            this.mCancelButton.Visible = false;
            // 
            // mOkButton
            // 
            this.mOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.mOkButton.Location = new System.Drawing.Point(605, 536);
            this.mOkButton.Name = "mOkButton";
            this.mOkButton.Size = new System.Drawing.Size(75, 23);
            this.mOkButton.TabIndex = 2;
            this.mOkButton.Text = "&OK";
            this.mOkButton.UseVisualStyleBackColor = true;
            this.mOkButton.Click += new System.EventHandler(this.mOkButton_Click);
            // 
            // RichTextDisplay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(692, 566);
            this.Controls.Add(this.mOkButton);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.richTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "RichTextDisplay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);

		}
		#endregion

		private void mExitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void mLoadMenuItem_Click(object sender, System.EventArgs e)
		{
			LoadFile();
		}


		private void mSaveAsMenuItem_Click(object sender, System.EventArgs e)
		{
			SaveAs();
		}

		private void mSaveMenuItem_Click(object sender, System.EventArgs e)
		{
			Save();
		}
		private string mCurrentFileName = null;
		
		private string CurrentFileName
		{
			get
			{
				return mCurrentFileName;
			}
			set
			{
				mCurrentFileName = value;
				this.Text = String.Concat("Working on file '", value,"'");
			}
		}

		private void LoadFile()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.RestoreDirectory = true;
			if( dlg.ShowDialog(this) == DialogResult.OK)
			{
				richTextBox.LoadFile(dlg.FileName, mRichTextStreamType);
				CurrentFileName = dlg.FileName;
			}
			dlg.Dispose();
		}

		private void SaveAs()
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.RestoreDirectory = true;
			if( dlg.ShowDialog(this) == DialogResult.OK)
			{
				richTextBox.SaveFile(dlg.FileName, mRichTextStreamType);
				CurrentFileName = dlg.FileName;
			}
			dlg.Dispose();
		}

		private void Save()
		{
			if( CurrentFileName == null )
			{
				SaveAs();
			}
			else
			{
				richTextBox.SaveFile(mCurrentFileName, mRichTextStreamType);
			}
		}

		private void richTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( e.Control && e.KeyCode == Keys.S )
			{
				Save();
			}
			else if( e.Control && e.KeyCode == Keys.L)
			{
				LoadFile();
			}
			else if(e.Control && e.KeyCode == Keys.V )
			{
				richTextBox.Paste(DataFormats.GetFormat(DataFormats.Text));
				e.Handled = true;
			}
		}

		private void mCopyMenuItemC_Click(object sender, System.EventArgs e)
		{
			richTextBox.Copy();
		}

		private void mPasteMenuItem_Click(object sender, System.EventArgs e)
		{
			richTextBox.Paste(DataFormats.GetFormat(DataFormats.Text));
		}

        private void mClearMenuItem_Click(object sender, System.EventArgs e)
        {
            richTextBox.Clear();
        }

        private void richTextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Environment.OSVersion.ToString().ToUpper().Contains("WINDOWS"))
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
        }

        private void mOkButton_Click(object sender, EventArgs e)
        {
            Close();
        }
		
	}
}
