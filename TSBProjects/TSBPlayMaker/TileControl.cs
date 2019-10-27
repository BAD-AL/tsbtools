using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace PlayProto
{
	/// <summary>
	/// Summary description for TileControl.
	/// </summary>
	public class TileControl : System.Windows.Forms.UserControl, IPlay
	{
		private PlayProto.TilePanel mTilePanel;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label mCurrentTileLabel;
		private System.Windows.Forms.Button mFillButton;
		private System.Windows.Forms.TextBox mPatternTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button mUpdateGraphicButton;
		private System.Windows.Forms.Label mDragTileNameLabel;
		private System.Windows.Forms.Button mNextButton;
		private System.Windows.Forms.TextBox mPlayIndexTextBox;
		private System.Windows.Forms.Button mGoButton;

		private PlayGraphicPanel mPlayGraphicPanel = null;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button mSaveDesignButton;

		private int mPlayNumber = 0;
		private System.Windows.Forms.Button mRefreashButton;

		public const int GRAPHIC_START = 0x27546;

		/// <summary>
		/// the index of the play to show.
		/// </summary>
		public int PlayNumber
		{
			get{ return mPlayNumber; }

			set
			{
				mPlayNumber = value;
				if( mPlayNumber > 63 )
					mPlayNumber = 0;

				mPlayIndexTextBox.Text = ""+ mPlayNumber;
			}

		}

		public TileControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			StandAlone = false;
		}

		public void Init()
		{
			try
			{
				AddTiles();
			}
			catch{}
			mDragTile = mTilePanel.GetImageGuy("00");
			mPlayGraphicPanel.FillWithImage( mDragTile);
			UpdatePatternTextBox();
		}

		private bool mStandAlone = true;

		public bool StandAlone
		{
			set
			{
				if( mStandAlone != value )
				{
					mStandAlone = value;
					mPlayIndexTextBox.Enabled = mStandAlone;
					mNextButton.Visible = mStandAlone;
					mGoButton.Visible   = mStandAlone;
				}
			}
			get{ return mStandAlone; }
		}
		private void AddTiles()
		{
			//string[] files = Directory.GetFiles(".\\Tiles", "*.bmp");
            string[] files = {
                "00.BMP", "01.BMP", "8D.BMP", "8E.BMP", "8F.BMP", "90.BMP", "91.BMP", "92.BMP",
                "93.BMP","94.BMP","95.BMP","96.BMP","97.BMP","98.BMP","99.BMP","9A.BMP",
                "9B.BMP","9C.BMP","9D.BMP","9E.BMP","9F.BMP","A0.BMP","A1.BMP","A2.BMP",
                "A3.BMP","A4.BMP","A5.BMP","A6.BMP","A7.BMP","A8.BMP","A9.BMP","AA.BMP",
                "AB.BMP","AC.BMP","AD.BMP","AE.BMP","AF.BMP","B0.BMP","B1.BMP","B2.BMP",
                "B3.BMP","B4.BMP","B5.BMP","B6.BMP","B7.BMP","B8.BMP","B9.BMP","BA.BMP",
                "BB.BMP","BC.BMP","BD.BMP","BE.BMP","BF.BMP","C0.BMP","C1.BMP","C2.BMP",
                "C3.BMP","C4.BMP","C5.BMP","C6.BMP","C7.BMP","C8.BMP","C9.BMP","CA.BMP",
                "CB.BMP","CC.BMP","CD.BMP","CE.BMP","CF.BMP","D0.BMP","D1.BMP","D2.BMP",
                "D3.BMP","D4.BMP","D5.BMP","D6.BMP","D7.BMP","D8.BMP","D9.BMP","DA.BMP",
                "DB.BMP","DC.BMP","DD.BMP","DE.BMP","DF.BMP","E0.BMP","E1.BMP","E2.BMP",
                "E3.BMP","E4.BMP","E5.BMP","E6.BMP","E7.BMP","E8.BMP","E9.BMP","EA.BMP",
                "EB.BMP","EC.BMP","ED.BMP","EE.BMP","EF.BMP","F0.BMP","F1.BMP","F2.BMP",
                "F3.BMP","F4.BMP","F5.BMP","F6.BMP","F7.BMP","F8.BMP","F9.BMP","FA.BMP",
                "FB.BMP","FC.BMP","FD.BMP","FE.BMP","FF.BMP"};

			mTilePanel.AddTiles(files);
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mTilePanel = new PlayProto.TilePanel();
			this.mPlayGraphicPanel = new PlayProto.PlayGraphicPanel();
			this.mCurrentTileLabel = new System.Windows.Forms.Label();
			this.mFillButton = new System.Windows.Forms.Button();
			this.mPatternTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.mUpdateGraphicButton = new System.Windows.Forms.Button();
			this.mDragTileNameLabel = new System.Windows.Forms.Label();
			this.mNextButton = new System.Windows.Forms.Button();
			this.mPlayIndexTextBox = new System.Windows.Forms.TextBox();
			this.mGoButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.mSaveDesignButton = new System.Windows.Forms.Button();
			this.mRefreashButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// mTilePanel
			// 
			this.mTilePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.mTilePanel.Location = new System.Drawing.Point(240, 4);
			this.mTilePanel.Name = "mTilePanel";
			this.mTilePanel.Size = new System.Drawing.Size(192, 360);
			this.mTilePanel.TabIndex = 2;
			this.mTilePanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mTilePanel_MouseDown);
			// 
			// mPlayGraphicPanel
			// 
			this.mPlayGraphicPanel.AllowDrop = true;
			this.mPlayGraphicPanel.BackColor = System.Drawing.Color.Gainsboro;
			this.mPlayGraphicPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.mPlayGraphicPanel.Location = new System.Drawing.Point(8, 24);
			this.mPlayGraphicPanel.Name = "mPlayGraphicPanel";
			this.mPlayGraphicPanel.Size = new System.Drawing.Size(144, 168);
			this.mPlayGraphicPanel.TabIndex = 0;
			this.mPlayGraphicPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mPlayGraphicPanel_MouseDown);
			// 
			// mCurrentTileLabel
			// 
			this.mCurrentTileLabel.AutoSize = true;
			this.mCurrentTileLabel.Location = new System.Drawing.Point(8, 208);
			this.mCurrentTileLabel.Name = "mCurrentTileLabel";
			this.mCurrentTileLabel.Size = new System.Drawing.Size(66, 16);
			this.mCurrentTileLabel.TabIndex = 3;
			this.mCurrentTileLabel.Text = "Current Tile:";
			// 
			// mFillButton
			// 
			this.mFillButton.Location = new System.Drawing.Point(144, 200);
			this.mFillButton.Name = "mFillButton";
			this.mFillButton.Size = new System.Drawing.Size(40, 23);
			this.mFillButton.TabIndex = 20;
			this.mFillButton.Text = "Fill";
			this.mFillButton.Click += new System.EventHandler(this.mFillButton_Click);
			// 
			// mPatternTextBox
			// 
			this.mPatternTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.mPatternTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.mPatternTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.mPatternTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.mPatternTextBox.Location = new System.Drawing.Point(8, 264);
			this.mPatternTextBox.MaxLength = 168;
			this.mPatternTextBox.Multiline = true;
			this.mPatternTextBox.Name = "mPatternTextBox";
			this.mPatternTextBox.Size = new System.Drawing.Size(176, 104);
			this.mPatternTextBox.TabIndex = 50;
			this.mPatternTextBox.Text = "";
			this.mPatternTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mPatternTextBox_KeyDown);
			this.mPatternTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mPatternTextBox_KeyPress);
			this.mPatternTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.mPatternTextBox_KeyUp);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(160, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 17);
			this.label1.TabIndex = 7;
			this.label1.Text = "Select Tile: -->";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(203, 16);
			this.label2.TabIndex = 8;
			this.label2.Text = "Click on square to place the current Tile";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(8, 240);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(62, 16);
			this.label3.TabIndex = 9;
			this.label3.Text = "Tile Pattern";
			// 
			// mUpdateGraphicButton
			// 
			this.mUpdateGraphicButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.mUpdateGraphicButton.Location = new System.Drawing.Point(8, 368);
			this.mUpdateGraphicButton.Name = "mUpdateGraphicButton";
			this.mUpdateGraphicButton.Size = new System.Drawing.Size(168, 23);
			this.mUpdateGraphicButton.TabIndex = 10;
			this.mUpdateGraphicButton.Text = "Update Graphic based on text";
			this.mUpdateGraphicButton.Click += new System.EventHandler(this.mUpdateGraphicButton_Click);
			// 
			// mDragTileNameLabel
			// 
			this.mDragTileNameLabel.Location = new System.Drawing.Point(112, 204);
			this.mDragTileNameLabel.Name = "mDragTileNameLabel";
			this.mDragTileNameLabel.Size = new System.Drawing.Size(24, 23);
			this.mDragTileNameLabel.TabIndex = 11;
			this.mDragTileNameLabel.Text = "00";
			// 
			// mNextButton
			// 
			this.mNextButton.Location = new System.Drawing.Point(168, 152);
			this.mNextButton.Name = "mNextButton";
			this.mNextButton.Size = new System.Drawing.Size(48, 23);
			this.mNextButton.TabIndex = 12;
			this.mNextButton.Text = "Next";
			this.mNextButton.Click += new System.EventHandler(this.mNextButton_Click);
			// 
			// mPlayIndexTextBox
			// 
			this.mPlayIndexTextBox.Location = new System.Drawing.Point(168, 96);
			this.mPlayIndexTextBox.MaxLength = 2;
			this.mPlayIndexTextBox.Name = "mPlayIndexTextBox";
			this.mPlayIndexTextBox.Size = new System.Drawing.Size(32, 20);
			this.mPlayIndexTextBox.TabIndex = 13;
			this.mPlayIndexTextBox.Text = "0";
			this.mPlayIndexTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mPlayIndexTextBox_KeyDown);
			this.mPlayIndexTextBox.TextChanged += new System.EventHandler(this.mPlayIndexTextBox_TextChanged);
			// 
			// mGoButton
			// 
			this.mGoButton.Location = new System.Drawing.Point(168, 120);
			this.mGoButton.Name = "mGoButton";
			this.mGoButton.Size = new System.Drawing.Size(48, 23);
			this.mGoButton.TabIndex = 14;
			this.mGoButton.Text = "Go";
			this.mGoButton.Click += new System.EventHandler(this.mGoButton_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(168, 72);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 20);
			this.label4.TabIndex = 15;
			this.label4.Text = "Play index:";
			// 
			// mSaveDesignButton
			// 
			this.mSaveDesignButton.Location = new System.Drawing.Point(344, 368);
			this.mSaveDesignButton.Name = "mSaveDesignButton";
			this.mSaveDesignButton.TabIndex = 40;
			this.mSaveDesignButton.Text = "Update";
			this.mSaveDesignButton.Click += new System.EventHandler(this.mSaveDesignButton_Click);
			// 
			// mRefreashButton
			// 
			this.mRefreashButton.Location = new System.Drawing.Point(160, 40);
			this.mRefreashButton.Name = "mRefreashButton";
			this.mRefreashButton.Size = new System.Drawing.Size(64, 23);
			this.mRefreashButton.TabIndex = 30;
			this.mRefreashButton.Text = "Undo all";
			this.mRefreashButton.Click += new System.EventHandler(this.mRefreashButton_Click);
			// 
			// TileControl
			// 
			this.Controls.Add(this.mRefreashButton);
			this.Controls.Add(this.mSaveDesignButton);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.mGoButton);
			this.Controls.Add(this.mPlayIndexTextBox);
			this.Controls.Add(this.mNextButton);
			this.Controls.Add(this.mDragTileNameLabel);
			this.Controls.Add(this.mUpdateGraphicButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.mPatternTextBox);
			this.Controls.Add(this.mFillButton);
			this.Controls.Add(this.mCurrentTileLabel);
			this.Controls.Add(this.mPlayGraphicPanel);
			this.Controls.Add(this.mTilePanel);
			this.Name = "TileControl";
			this.Size = new System.Drawing.Size(440, 416);
			this.VisibleChanged += new System.EventHandler(this.TileControl_VisibleChanged);
			this.ResumeLayout(false);

		}
		#endregion

		private ImageGuy mDragTile = null;
		
		private int mDragTileWidth = 24;
		private int mDragTile_x = 80;
		private int mDragTile_y = 200;
		

		private void mTilePanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ImageGuy test = mTilePanel.GetImageGuy(e.X, e.Y);
			
			if( test != null )
			{
				mDragTile = test;
				mDragTileNameLabel.Text = mDragTile.FileNameNoExt;
				this.Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if( mDragTile != null )
			{
				//Rectangle r = new Rectangle(x*sizeX, y*sizeY,img.Width*mTileScale, img.Height*mTileScale);
				Rectangle r = new Rectangle(mDragTile_x,mDragTile_y,mDragTileWidth,mDragTileWidth);
				e.Graphics.DrawImage(mDragTile.TheImage, r);
			}
			base.OnPaint (e);
		}

		private void mPlayGraphicPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			mPlayGraphicPanel.AddImageGuy(mDragTile,e.X,e.Y);
			UpdatePatternTextBox();
		}

		private void mFillButton_Click(object sender, System.EventArgs e)
		{
			mPlayGraphicPanel.FillWithImage(mDragTile);
			UpdatePatternTextBox();
		}

		private void UpdatePatternTextBox()
		{
			StringBuilder sb = new StringBuilder(300);
			for( int col = 0; col < 7; col++ )
			{
				for( int row = 0; row < 6; row++ )
				{
					ImageGuy guy = mPlayGraphicPanel.GetImageGuy(row,col);
					if( guy != null )
					{
						sb.Append(guy.FileNameNoExt);
					}
					else
					{
						sb.Append("00");
					}
					sb.Append(", ");
				}
				sb.Remove(sb.Length-2,2);
				sb.Append("\r\n");
			}
			mPatternTextBox.Text = sb.ToString();
		}

		private void mUpdateGraphicButton_Click(object sender, System.EventArgs e)
		{
			UpdateGraphicBasedOnText();
		}

		private void UpdateGraphicBasedOnText()
		{
			ArrayList theChunks = GetImagePattern();
			int index = 0;
			for( int col = 0; col < 7; col++ )
			{
				for( int row = 0; row < 6; row++ )
				{
					if( index < theChunks.Count )
					{
						ImageGuy guy = mTilePanel.GetImageGuy(theChunks[index++].ToString() );
						mPlayGraphicPanel.SetImageGuy( guy,row,col);
					}
				}
			}
		}

		/// <summary>
		/// Gets an arraylist of ImageGuys that make up the current graphic.
		/// </summary>
		/// <returns></returns>
		private ArrayList GetImagePattern()
		{
			string[] chunks =  mPatternTextBox.Text.Split("\r\n, ".ToCharArray());
			ArrayList theChunks = new ArrayList(chunks.Length);
			for(int i = 0; i < chunks.Length; i++ )
			{
				if( chunks[i].Length > 0 )
					theChunks.Add(chunks[i]);
			}
			theChunks.TrimToSize();
			return theChunks;
		}

		private void mPatternTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( e.KeyCode == Keys.Enter )
			{
				e.Handled = true;
				UpdateGraphicBasedOnText();
			}
			else if( e.KeyCode > Keys.F )
			{
				e.Handled = true;
			}
		}

		private void mPatternTextBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if( e.KeyChar == 13 )
			{
				e.Handled = true;
			}
			else if( e.KeyChar > 'f' )
			{
				e.Handled = true;
			}
		}

		private void mPatternTextBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( e.KeyCode == Keys.Enter )
			{
				e.Handled = true;
			}
		}

		
		private void mNextButton_Click(object sender, System.EventArgs e)
		{
			if( MainForm.TheROM == null )
			{
				MessageBox.Show("You must first open a ROM.");
				return;
			}
			PlayNumber++;
			ShowGraphicAtPlayIndex();		
		}

		public void ShowGraphicAtPlayIndex()
		{
			if( MainForm.TheROM == null )
			{
				MessageBox.Show("You must first open a ROM.");
				return;
			}
			int start = GRAPHIC_START + (PlayNumber * 42);
			
			long loc = start;
			StringBuilder sb = new StringBuilder(300);
			string s="";

			for( int col = 0; col < 7; col++ )
			{
				for( int row = 0; row < 6; row++ )
				{
					s = string.Format("{0:x2}",MainForm.TheROM[loc]);
					sb.Append(s);
					loc++;
					sb.Append(", ");
				}
				sb.Remove(sb.Length-2,2);
				sb.Append("\r\n");
			}
			mPatternTextBox.Text = sb.ToString();
			int g = mPatternTextBox.Text .Length;
			UpdateGraphicBasedOnText();
		}

		public void SavePlayDesign()
		{
			if( MainForm.TheROM == null )
			{
				MessageBox.Show("You must first open a ROM.");
				return;
			}

			int loc = GRAPHIC_START + 42*PlayNumber;
			ArrayList theChunks = GetImagePattern();

			try
			{
				long end = loc + theChunks.Count;
				int j = 0;
				string current = null;
				byte b = 0;
				for(long i = loc; i < end; i++)
				{
					current = theChunks[j++] as string;
					b = Byte.Parse(current,System.Globalization.NumberStyles.AllowHexSpecifier);
					MainForm.TheROM[i] = b;
				}
			}
			catch(Exception e)
			{
				MessageBox.Show("Error saving play design! "+ e.Message);
			}
		}

		private void mGoButton_Click(object sender, System.EventArgs e)
		{
			ShowGraphicAtPlayIndex();
		}

		private void mPlayIndexTextBox_TextChanged(object sender, System.EventArgs e)
		{
			try
			{
				PlayNumber = Int32.Parse( mPlayIndexTextBox.Text );
			}
			catch
			{
			}
		}

		private void mPlayIndexTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( e.KeyCode == Keys.Enter )
			{
				ShowGraphicAtPlayIndex();
			}
		}

		private void mSaveDesignButton_Click(object sender, System.EventArgs e)
		{
			SavePlayDesign();
		}
		#region IPlay Members

		public void OnPlayChanged(int playIndex)
		{
			// TODO:  Add TileControl.OnPlayChanged implementation
			mPlayIndexTextBox.Text = MainForm.TheMainForm.PlayIndex +"";
			ShowGraphicAtPlayIndex();
		}

		public string GetXML()
		{
			string ret = "";
			StringBuilder sb = new StringBuilder(300);
			sb.Append("    PlayGraphic='\r\n        ");
			sb.Append(mPatternTextBox.Text.Replace("\n","\n        ").Trim() );
			sb.Append("'\r\n");
			ret = sb.ToString();
			return ret;
		}

		public void IPlayTabIndexChanged(int index)
		{
		}

		#endregion

		private void TileControl_VisibleChanged(object sender, System.EventArgs e)
		{
			if( this.Visible )
			{
				UpdateGraphicBasedOnText();
			}
		}

		private void mRefreashButton_Click(object sender, System.EventArgs e)
		{
			if( MessageBox.Show(this, 
                "Are you sure you want to Undo all changes to the graphic?", 
                "Are you sure?", 
				MessageBoxButtons.YesNoCancel) == DialogResult.Yes )
			{
				ShowGraphicAtPlayIndex();
			}
		}
	}
}
