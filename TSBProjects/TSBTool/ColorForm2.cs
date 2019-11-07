using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TSBTool
{
	/// <summary>
	/// Summary description for ColorForm2.
	/// </summary>
	public class ColorForm2 : System.Windows.Forms.Form
	{
		#region panels
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Panel panel7;
		private System.Windows.Forms.Panel panel8;
		private System.Windows.Forms.Panel panel9;
		private System.Windows.Forms.Panel panel10;
		private System.Windows.Forms.Panel panel11;
		private System.Windows.Forms.Panel panel12;
		private System.Windows.Forms.Panel panel13;
		private System.Windows.Forms.Panel panel14;
		private System.Windows.Forms.Panel panel15;
		private System.Windows.Forms.Panel panel16;
		private System.Windows.Forms.Panel panel17;
		private System.Windows.Forms.Panel panel18;
		private System.Windows.Forms.Panel panel19;
		private System.Windows.Forms.Panel panel24;
		private System.Windows.Forms.Panel panel25;
		private System.Windows.Forms.Panel panel26;
		private System.Windows.Forms.Panel panel27;
		private System.Windows.Forms.Panel panel32;
		private System.Windows.Forms.Panel panel33;
		private System.Windows.Forms.Panel panel34;
		private System.Windows.Forms.Panel panel35;
		private System.Windows.Forms.Panel panel40;
		private System.Windows.Forms.Panel panel41;
		private System.Windows.Forms.Panel panel42;
		private System.Windows.Forms.Panel panel43;
		private System.Windows.Forms.Panel panel59;
		private System.Windows.Forms.Panel panel0;
		private System.Windows.Forms.Panel panel21;
		private System.Windows.Forms.Panel panel22;
		private System.Windows.Forms.Panel panel23;
		private System.Windows.Forms.Panel panel20;
		private System.Windows.Forms.Panel panel30;
		private System.Windows.Forms.Panel panel31;
		private System.Windows.Forms.Panel panel29;
		private System.Windows.Forms.Panel panel28;
		private System.Windows.Forms.Panel panel37;
		private System.Windows.Forms.Panel panel38;
		private System.Windows.Forms.Panel panel39;
		private System.Windows.Forms.Panel panel36;
		private System.Windows.Forms.Panel panel46;
		private System.Windows.Forms.Panel panel47;
		private System.Windows.Forms.Panel panel45;
		private System.Windows.Forms.Panel panel44;
		private System.Windows.Forms.Panel panel57;
		private System.Windows.Forms.Panel panel58;
		private System.Windows.Forms.Panel panel48;
		private System.Windows.Forms.Panel panel49;
		private System.Windows.Forms.Panel panel50;
		private System.Windows.Forms.Panel panel51;
		private System.Windows.Forms.Panel panel53;
		private System.Windows.Forms.Panel panel54;
		private System.Windows.Forms.Panel panel55;
		private System.Windows.Forms.Panel panel52;
		private System.Windows.Forms.Panel panel56;
		private System.Windows.Forms.Panel panel62;
		private System.Windows.Forms.Panel panel63;
		private System.Windows.Forms.Panel panel61;
		private System.Windows.Forms.Panel panel60;
	#endregion

		private System.Windows.Forms.Button mOKButton;
		private System.Windows.Forms.Button mCancelButton;
		private System.Windows.Forms.TextBox mCurrentColorTextBox;
		private System.Windows.Forms.Panel mCurrentColorPanel;

		private Panel[] m00Panels = new Panel[16];
		private Panel[] m10Panels = new Panel[16];
		private Panel[] m20Panels = new Panel[16];
		private Panel[] m30Panels = new Panel[16];

		//00h
		private int[] mZeroHRow = {
						124,124,124,  0,0,252,  0,0,188,  68,40,188,
						148,0,132,    168,0,32, 168,16,0, 136,20,0,
						80,48,0,      0,120,0,  0,104,0,  0,88,0,
						0,64,88,      0,0,0,    0,0,0,    0,0,0 };

		//10h
		private int[] mTenHRow = {
						188,188,188, 0,120,248, 0,88,248,  104,68,252,
						216,0,204,   228,0,88,  248,56,0,  228,92,16,
						172,124,0,   0,184,0,   0,168,0,   0,168,68,
						0,136,136,   0,0,0,     0,0,0,     0,0,0 };

		//20h
		private int[] mTwentyHRow = {
						248,248,248,  60,188,252,  104,136,252, 152,120,248,
						248,120,248,  248,88,152,  248,120,88,  252,160,68,
						248,184,0,    184,248,24,  88,216,84,   88,248,152,
						0,232,216,    120,120,120, 0,0,0,       0,0,0 };

		//30h
		private int[] mThirtyHRow = {
						252,252,252,  164,228,252,  184,184,248,  216,184,248,
						248,184,248,  248,164,192,  240,208,176,  252,224,168,
						248,216,120,  216,248,120,  184,248,184,  184,248,216,
						0,252,252,    248,216,248,  0,0,0,        0,0,0 };


		private Color mCurrentColor;

		public Color GetColor(string hexStr)
		{
			Color ret = Color.Empty;
			if( hexStr != null && hexStr.Length == 2 )
			{
				//				int left = hexStr[0] - 48;
				//				int top  = hexStr[1] - 48;
				byte num = Byte.Parse(hexStr, System.Globalization.NumberStyles.AllowHexSpecifier);
				int left = num >> 4;
				int top = num & 0x0F;
				Panel[] row = null;

				if( left == 0 )
					row = m00Panels;
				else if (left == 1)
					row = m10Panels;
				else if( left == 2 )
					row = m20Panels;
				else if( left == 3 )
					row = m30Panels;

				if( row != null && top < row.Length )
				{
					ret = row[top].BackColor;
				}
			}
			return ret;
		}

		/// <summary>
		/// The last color chosen by the user.
		/// </summary>
		public Color CurrentColor
		{
			get{ return mCurrentColor;}

			set
			{ 
				if(mCurrentColor != value )
				{
					mCurrentColor = value;
					mCurrentColorPanel.BackColor = mCurrentColor;
				}
			}
		}

		private string  mCurrentColorString;
		private System.Windows.Forms.TextBox mDebugBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;


		/// <summary>
		/// The NES Color representation of the current color as a string.
		/// Like "00" or "30"
		/// </summary>
		public string  CurrentColorString
		{
			get{ return mCurrentColorString; }

			set
			{ 
				if( mCurrentColorString != value)
				{
					mCurrentColorString = value; 
					mCurrentColorTextBox.Text = mCurrentColorString;
					CurrentColor = GetColor(value);
				}
			}
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ColorForm2()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			AddPanelsToArrays();
			ColorizePanels();
		}

		private void AddPanelsToArrays()
		{
			m00Panels[0] = panel0;
			m00Panels[1] = panel1;
			m00Panels[2] = panel2;
			m00Panels[3] = panel3;
			m00Panels[4] = panel4;
			m00Panels[5] = panel5;
			m00Panels[6] = panel6;
			m00Panels[7] = panel7;
			m00Panels[8] = panel8;
			m00Panels[9] = panel9;
			m00Panels[10] = panel10;
			m00Panels[11] = panel11;
			m00Panels[12] = panel12;
			m00Panels[13] = panel13;
			m00Panels[14] = panel14;
			m00Panels[15] = panel15;

			m10Panels[0 ] = panel16;
			m10Panels[1 ] = panel17;
			m10Panels[2 ] = panel18;
			m10Panels[3 ] = panel19;
			m10Panels[4 ] = panel20;
			m10Panels[5 ] = panel21;
			m10Panels[6 ] = panel22;
			m10Panels[7 ] = panel23;
			m10Panels[8 ] = panel24;
			m10Panels[9 ] = panel25;
			m10Panels[10] = panel26;
			m10Panels[11] = panel27;
			m10Panels[12] = panel28;
			m10Panels[13] = panel29;
			m10Panels[14] = panel30;
			m10Panels[15] = panel31;

			m20Panels[0 ] = panel32;
			m20Panels[1 ] = panel33;
			m20Panels[2 ] = panel34;
			m20Panels[3 ] = panel35;
			m20Panels[4 ] = panel36;
			m20Panels[5 ] = panel37;
			m20Panels[6 ] = panel38;
			m20Panels[7 ] = panel39;
			m20Panels[8 ] = panel40;
			m20Panels[9 ] = panel41;
			m20Panels[10] = panel42;
			m20Panels[11] = panel43;
			m20Panels[12] = panel44;
			m20Panels[13] = panel45;
			m20Panels[14] = panel46;
			m20Panels[15] = panel47;

			m30Panels[0 ] = panel48;
			m30Panels[1 ] = panel49;
			m30Panels[2 ] = panel50;
			m30Panels[3 ] = panel51;
			m30Panels[4 ] = panel52;
			m30Panels[5 ] = panel53;
			m30Panels[6 ] = panel54;
			m30Panels[7 ] = panel55;
			m30Panels[8 ] = panel56;
			m30Panels[9 ] = panel57;
			m30Panels[10] = panel58;
			m30Panels[11] = panel59;
			m30Panels[12] = panel60;
			m30Panels[13] = panel61;
			m30Panels[14] = panel62;
			m30Panels[15] = panel63;
		}


		private void ColorizePanels()
		{
			int panelNum = 0;
			int r,g,b;
			for(int i =0; i < mZeroHRow.Length; i+=3)
			{
				r = mZeroHRow[i];
				g = mZeroHRow[i+1];
				b = mZeroHRow[i+2];
				m00Panels[panelNum].BackColor = Color.FromArgb(r,g,b);
				panelNum++;
			}

			panelNum = 0;
			for(int i =0; i < mTenHRow.Length; i+=3)
			{
				r = mTenHRow[i];
				g = mTenHRow[i+1];
				b = mTenHRow[i+2];
				m10Panels[panelNum].BackColor = Color.FromArgb(r,g,b);
				//m10Panels[panelNum].BackColor = Color.FromArgb(mTenHRow[i],mTenHRow[1+1],mTenHRow[i+2]);
				panelNum++;
			}

			panelNum = 0;
			for(int i =0; i < mTwentyHRow.Length; i+=3)
			{
				r = mTwentyHRow[i];
				g = mTwentyHRow[i+1];
				b = mTwentyHRow[i+2];
				m20Panels[panelNum].BackColor = Color.FromArgb(r,g,b);
				//m20Panels[panelNum].BackColor = Color.FromArgb(mTwentyHRow[i],mTwentyHRow[1+1],mTwentyHRow[i+2]);
				panelNum++;
			}

			panelNum = 0;
			for(int i =0; i < mThirtyHRow.Length; i+=3)
			{
				r = mThirtyHRow[i];
				g = mThirtyHRow[i+1];
				b = mThirtyHRow[i+2];
				m30Panels[panelNum].BackColor = Color.FromArgb(r,g,b);
				//m30Panels[panelNum].BackColor = Color.FromArgb(mThirtyHRow[i],mThirtyHRow[1+1],mThirtyHRow[i+2]);
				panelNum++;
			}

		}



		private string GetColorString(Panel p)
		{
			string ret = null;
			for(int i = 0; i < m00Panels.Length; i++)
			{
				if(m00Panels[i] == p)
				{
					ret = string.Format("0{0:X}",i);
					return ret;
				}
			}
			for(int i = 0; i < m10Panels.Length; i++)
			{
				if(m10Panels[i] == p)
				{
					ret = string.Format("1{0:X}",i);
					return ret;
				}
			}
			for(int i = 0; i < m20Panels.Length; i++)
			{
				if(m20Panels[i] == p)
				{
					ret = string.Format("2{0:X}",i);
					return ret;
				}
			}
			for(int i = 0; i < m30Panels.Length; i++)
			{
				if(m30Panels[i] == p)
				{
					ret = string.Format("3{0:X}",i);
					return ret;
				}
			}
			return ret;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorForm2));
            this.panel0 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.panel8 = new System.Windows.Forms.Panel();
            this.panel9 = new System.Windows.Forms.Panel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.panel11 = new System.Windows.Forms.Panel();
            this.panel12 = new System.Windows.Forms.Panel();
            this.panel13 = new System.Windows.Forms.Panel();
            this.panel14 = new System.Windows.Forms.Panel();
            this.panel15 = new System.Windows.Forms.Panel();
            this.panel16 = new System.Windows.Forms.Panel();
            this.panel17 = new System.Windows.Forms.Panel();
            this.panel18 = new System.Windows.Forms.Panel();
            this.panel19 = new System.Windows.Forms.Panel();
            this.panel21 = new System.Windows.Forms.Panel();
            this.panel22 = new System.Windows.Forms.Panel();
            this.panel23 = new System.Windows.Forms.Panel();
            this.panel20 = new System.Windows.Forms.Panel();
            this.panel24 = new System.Windows.Forms.Panel();
            this.panel25 = new System.Windows.Forms.Panel();
            this.panel26 = new System.Windows.Forms.Panel();
            this.panel27 = new System.Windows.Forms.Panel();
            this.panel30 = new System.Windows.Forms.Panel();
            this.panel31 = new System.Windows.Forms.Panel();
            this.panel29 = new System.Windows.Forms.Panel();
            this.panel28 = new System.Windows.Forms.Panel();
            this.panel32 = new System.Windows.Forms.Panel();
            this.panel33 = new System.Windows.Forms.Panel();
            this.panel34 = new System.Windows.Forms.Panel();
            this.panel35 = new System.Windows.Forms.Panel();
            this.panel37 = new System.Windows.Forms.Panel();
            this.panel38 = new System.Windows.Forms.Panel();
            this.panel39 = new System.Windows.Forms.Panel();
            this.panel36 = new System.Windows.Forms.Panel();
            this.panel40 = new System.Windows.Forms.Panel();
            this.panel41 = new System.Windows.Forms.Panel();
            this.panel42 = new System.Windows.Forms.Panel();
            this.panel43 = new System.Windows.Forms.Panel();
            this.panel46 = new System.Windows.Forms.Panel();
            this.panel47 = new System.Windows.Forms.Panel();
            this.panel45 = new System.Windows.Forms.Panel();
            this.panel44 = new System.Windows.Forms.Panel();
            this.panel57 = new System.Windows.Forms.Panel();
            this.panel58 = new System.Windows.Forms.Panel();
            this.panel48 = new System.Windows.Forms.Panel();
            this.panel49 = new System.Windows.Forms.Panel();
            this.panel50 = new System.Windows.Forms.Panel();
            this.panel51 = new System.Windows.Forms.Panel();
            this.panel53 = new System.Windows.Forms.Panel();
            this.panel54 = new System.Windows.Forms.Panel();
            this.panel55 = new System.Windows.Forms.Panel();
            this.panel52 = new System.Windows.Forms.Panel();
            this.panel56 = new System.Windows.Forms.Panel();
            this.panel59 = new System.Windows.Forms.Panel();
            this.panel62 = new System.Windows.Forms.Panel();
            this.panel63 = new System.Windows.Forms.Panel();
            this.panel61 = new System.Windows.Forms.Panel();
            this.panel60 = new System.Windows.Forms.Panel();
            this.mOKButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.mCurrentColorTextBox = new System.Windows.Forms.TextBox();
            this.mCurrentColorPanel = new System.Windows.Forms.Panel();
            this.mDebugBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // panel0
            // 
            this.panel0.BackColor = System.Drawing.Color.Silver;
            this.panel0.Location = new System.Drawing.Point(24, 24);
            this.panel0.Name = "panel0";
            this.panel0.Size = new System.Drawing.Size(25, 25);
            this.panel0.TabIndex = 0;
            this.panel0.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Location = new System.Drawing.Point(56, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(25, 25);
            this.panel1.TabIndex = 1;
            this.panel1.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Silver;
            this.panel2.Location = new System.Drawing.Point(88, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(25, 25);
            this.panel2.TabIndex = 2;
            this.panel2.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Silver;
            this.panel3.Location = new System.Drawing.Point(120, 24);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(25, 25);
            this.panel3.TabIndex = 3;
            this.panel3.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.Silver;
            this.panel4.Location = new System.Drawing.Point(152, 24);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(25, 25);
            this.panel4.TabIndex = 4;
            this.panel4.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.Silver;
            this.panel5.Location = new System.Drawing.Point(184, 24);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(25, 25);
            this.panel5.TabIndex = 5;
            this.panel5.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.Silver;
            this.panel6.Location = new System.Drawing.Point(216, 24);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(25, 25);
            this.panel6.TabIndex = 6;
            this.panel6.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.Silver;
            this.panel7.Location = new System.Drawing.Point(248, 24);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(25, 25);
            this.panel7.TabIndex = 7;
            this.panel7.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.Silver;
            this.panel8.Location = new System.Drawing.Point(280, 24);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(25, 25);
            this.panel8.TabIndex = 4;
            this.panel8.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel9
            // 
            this.panel9.BackColor = System.Drawing.Color.Silver;
            this.panel9.Location = new System.Drawing.Point(312, 24);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(25, 25);
            this.panel9.TabIndex = 5;
            this.panel9.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel10
            // 
            this.panel10.BackColor = System.Drawing.Color.Silver;
            this.panel10.Location = new System.Drawing.Point(344, 24);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(25, 25);
            this.panel10.TabIndex = 6;
            this.panel10.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel11
            // 
            this.panel11.BackColor = System.Drawing.Color.Silver;
            this.panel11.Location = new System.Drawing.Point(376, 24);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(25, 25);
            this.panel11.TabIndex = 7;
            this.panel11.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel12
            // 
            this.panel12.BackColor = System.Drawing.Color.Silver;
            this.panel12.Location = new System.Drawing.Point(408, 24);
            this.panel12.Name = "panel12";
            this.panel12.Size = new System.Drawing.Size(25, 25);
            this.panel12.TabIndex = 8;
            this.panel12.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel13
            // 
            this.panel13.BackColor = System.Drawing.Color.Silver;
            this.panel13.Location = new System.Drawing.Point(440, 24);
            this.panel13.Name = "panel13";
            this.panel13.Size = new System.Drawing.Size(25, 25);
            this.panel13.TabIndex = 9;
            this.panel13.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel14
            // 
            this.panel14.BackColor = System.Drawing.Color.Silver;
            this.panel14.Location = new System.Drawing.Point(472, 24);
            this.panel14.Name = "panel14";
            this.panel14.Size = new System.Drawing.Size(25, 25);
            this.panel14.TabIndex = 10;
            this.panel14.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel15
            // 
            this.panel15.BackColor = System.Drawing.Color.Silver;
            this.panel15.Location = new System.Drawing.Point(504, 24);
            this.panel15.Name = "panel15";
            this.panel15.Size = new System.Drawing.Size(25, 25);
            this.panel15.TabIndex = 11;
            this.panel15.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel16
            // 
            this.panel16.BackColor = System.Drawing.Color.Silver;
            this.panel16.Location = new System.Drawing.Point(24, 56);
            this.panel16.Name = "panel16";
            this.panel16.Size = new System.Drawing.Size(25, 25);
            this.panel16.TabIndex = 12;
            this.panel16.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel17
            // 
            this.panel17.BackColor = System.Drawing.Color.Silver;
            this.panel17.Location = new System.Drawing.Point(56, 56);
            this.panel17.Name = "panel17";
            this.panel17.Size = new System.Drawing.Size(25, 25);
            this.panel17.TabIndex = 13;
            this.panel17.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel18
            // 
            this.panel18.BackColor = System.Drawing.Color.Silver;
            this.panel18.Location = new System.Drawing.Point(88, 56);
            this.panel18.Name = "panel18";
            this.panel18.Size = new System.Drawing.Size(25, 25);
            this.panel18.TabIndex = 14;
            this.panel18.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel19
            // 
            this.panel19.BackColor = System.Drawing.Color.Silver;
            this.panel19.Location = new System.Drawing.Point(120, 56);
            this.panel19.Name = "panel19";
            this.panel19.Size = new System.Drawing.Size(25, 25);
            this.panel19.TabIndex = 15;
            this.panel19.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel21
            // 
            this.panel21.BackColor = System.Drawing.Color.Silver;
            this.panel21.Location = new System.Drawing.Point(184, 56);
            this.panel21.Name = "panel21";
            this.panel21.Size = new System.Drawing.Size(25, 25);
            this.panel21.TabIndex = 18;
            this.panel21.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel22
            // 
            this.panel22.BackColor = System.Drawing.Color.Silver;
            this.panel22.Location = new System.Drawing.Point(216, 56);
            this.panel22.Name = "panel22";
            this.panel22.Size = new System.Drawing.Size(25, 25);
            this.panel22.TabIndex = 21;
            this.panel22.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel23
            // 
            this.panel23.BackColor = System.Drawing.Color.Silver;
            this.panel23.Location = new System.Drawing.Point(248, 56);
            this.panel23.Name = "panel23";
            this.panel23.Size = new System.Drawing.Size(25, 25);
            this.panel23.TabIndex = 23;
            this.panel23.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel20
            // 
            this.panel20.BackColor = System.Drawing.Color.Silver;
            this.panel20.Location = new System.Drawing.Point(152, 56);
            this.panel20.Name = "panel20";
            this.panel20.Size = new System.Drawing.Size(25, 25);
            this.panel20.TabIndex = 16;
            this.panel20.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel24
            // 
            this.panel24.BackColor = System.Drawing.Color.Silver;
            this.panel24.Location = new System.Drawing.Point(280, 56);
            this.panel24.Name = "panel24";
            this.panel24.Size = new System.Drawing.Size(25, 25);
            this.panel24.TabIndex = 17;
            this.panel24.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel25
            // 
            this.panel25.BackColor = System.Drawing.Color.Silver;
            this.panel25.Location = new System.Drawing.Point(312, 56);
            this.panel25.Name = "panel25";
            this.panel25.Size = new System.Drawing.Size(25, 25);
            this.panel25.TabIndex = 19;
            this.panel25.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel26
            // 
            this.panel26.BackColor = System.Drawing.Color.Silver;
            this.panel26.Location = new System.Drawing.Point(344, 56);
            this.panel26.Name = "panel26";
            this.panel26.Size = new System.Drawing.Size(25, 25);
            this.panel26.TabIndex = 20;
            this.panel26.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel27
            // 
            this.panel27.BackColor = System.Drawing.Color.Silver;
            this.panel27.Location = new System.Drawing.Point(376, 56);
            this.panel27.Name = "panel27";
            this.panel27.Size = new System.Drawing.Size(25, 25);
            this.panel27.TabIndex = 22;
            this.panel27.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel30
            // 
            this.panel30.BackColor = System.Drawing.Color.Silver;
            this.panel30.Location = new System.Drawing.Point(472, 56);
            this.panel30.Name = "panel30";
            this.panel30.Size = new System.Drawing.Size(25, 25);
            this.panel30.TabIndex = 26;
            this.panel30.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel31
            // 
            this.panel31.BackColor = System.Drawing.Color.Silver;
            this.panel31.Location = new System.Drawing.Point(504, 56);
            this.panel31.Name = "panel31";
            this.panel31.Size = new System.Drawing.Size(25, 25);
            this.panel31.TabIndex = 27;
            this.panel31.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel29
            // 
            this.panel29.BackColor = System.Drawing.Color.Silver;
            this.panel29.Location = new System.Drawing.Point(440, 56);
            this.panel29.Name = "panel29";
            this.panel29.Size = new System.Drawing.Size(25, 25);
            this.panel29.TabIndex = 25;
            this.panel29.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel28
            // 
            this.panel28.BackColor = System.Drawing.Color.Silver;
            this.panel28.Location = new System.Drawing.Point(408, 56);
            this.panel28.Name = "panel28";
            this.panel28.Size = new System.Drawing.Size(25, 25);
            this.panel28.TabIndex = 24;
            this.panel28.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel32
            // 
            this.panel32.BackColor = System.Drawing.Color.Silver;
            this.panel32.Location = new System.Drawing.Point(24, 88);
            this.panel32.Name = "panel32";
            this.panel32.Size = new System.Drawing.Size(25, 25);
            this.panel32.TabIndex = 28;
            this.panel32.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel33
            // 
            this.panel33.BackColor = System.Drawing.Color.Silver;
            this.panel33.Location = new System.Drawing.Point(56, 88);
            this.panel33.Name = "panel33";
            this.panel33.Size = new System.Drawing.Size(25, 25);
            this.panel33.TabIndex = 29;
            this.panel33.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel34
            // 
            this.panel34.BackColor = System.Drawing.Color.Silver;
            this.panel34.Location = new System.Drawing.Point(88, 88);
            this.panel34.Name = "panel34";
            this.panel34.Size = new System.Drawing.Size(25, 25);
            this.panel34.TabIndex = 30;
            this.panel34.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel35
            // 
            this.panel35.BackColor = System.Drawing.Color.Silver;
            this.panel35.Location = new System.Drawing.Point(120, 88);
            this.panel35.Name = "panel35";
            this.panel35.Size = new System.Drawing.Size(25, 25);
            this.panel35.TabIndex = 31;
            this.panel35.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel37
            // 
            this.panel37.BackColor = System.Drawing.Color.Silver;
            this.panel37.Location = new System.Drawing.Point(184, 88);
            this.panel37.Name = "panel37";
            this.panel37.Size = new System.Drawing.Size(25, 25);
            this.panel37.TabIndex = 35;
            this.panel37.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel38
            // 
            this.panel38.BackColor = System.Drawing.Color.Silver;
            this.panel38.Location = new System.Drawing.Point(216, 88);
            this.panel38.Name = "panel38";
            this.panel38.Size = new System.Drawing.Size(25, 25);
            this.panel38.TabIndex = 37;
            this.panel38.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel39
            // 
            this.panel39.BackColor = System.Drawing.Color.Silver;
            this.panel39.Location = new System.Drawing.Point(248, 88);
            this.panel39.Name = "panel39";
            this.panel39.Size = new System.Drawing.Size(25, 25);
            this.panel39.TabIndex = 39;
            this.panel39.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel36
            // 
            this.panel36.BackColor = System.Drawing.Color.Silver;
            this.panel36.Location = new System.Drawing.Point(152, 88);
            this.panel36.Name = "panel36";
            this.panel36.Size = new System.Drawing.Size(25, 25);
            this.panel36.TabIndex = 32;
            this.panel36.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel40
            // 
            this.panel40.BackColor = System.Drawing.Color.Silver;
            this.panel40.Location = new System.Drawing.Point(280, 88);
            this.panel40.Name = "panel40";
            this.panel40.Size = new System.Drawing.Size(25, 25);
            this.panel40.TabIndex = 33;
            this.panel40.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel41
            // 
            this.panel41.BackColor = System.Drawing.Color.Silver;
            this.panel41.Location = new System.Drawing.Point(312, 88);
            this.panel41.Name = "panel41";
            this.panel41.Size = new System.Drawing.Size(25, 25);
            this.panel41.TabIndex = 34;
            this.panel41.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel42
            // 
            this.panel42.BackColor = System.Drawing.Color.Silver;
            this.panel42.Location = new System.Drawing.Point(344, 88);
            this.panel42.Name = "panel42";
            this.panel42.Size = new System.Drawing.Size(25, 25);
            this.panel42.TabIndex = 36;
            this.panel42.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel43
            // 
            this.panel43.BackColor = System.Drawing.Color.Silver;
            this.panel43.Location = new System.Drawing.Point(376, 88);
            this.panel43.Name = "panel43";
            this.panel43.Size = new System.Drawing.Size(25, 25);
            this.panel43.TabIndex = 38;
            this.panel43.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel46
            // 
            this.panel46.BackColor = System.Drawing.Color.Silver;
            this.panel46.Location = new System.Drawing.Point(472, 88);
            this.panel46.Name = "panel46";
            this.panel46.Size = new System.Drawing.Size(25, 25);
            this.panel46.TabIndex = 42;
            this.panel46.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel47
            // 
            this.panel47.BackColor = System.Drawing.Color.Silver;
            this.panel47.Location = new System.Drawing.Point(504, 88);
            this.panel47.Name = "panel47";
            this.panel47.Size = new System.Drawing.Size(25, 25);
            this.panel47.TabIndex = 43;
            this.panel47.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel45
            // 
            this.panel45.BackColor = System.Drawing.Color.Silver;
            this.panel45.Location = new System.Drawing.Point(440, 88);
            this.panel45.Name = "panel45";
            this.panel45.Size = new System.Drawing.Size(25, 25);
            this.panel45.TabIndex = 41;
            this.panel45.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel44
            // 
            this.panel44.BackColor = System.Drawing.Color.Silver;
            this.panel44.Location = new System.Drawing.Point(408, 88);
            this.panel44.Name = "panel44";
            this.panel44.Size = new System.Drawing.Size(25, 25);
            this.panel44.TabIndex = 40;
            this.panel44.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel57
            // 
            this.panel57.BackColor = System.Drawing.Color.Silver;
            this.panel57.Location = new System.Drawing.Point(312, 120);
            this.panel57.Name = "panel57";
            this.panel57.Size = new System.Drawing.Size(25, 25);
            this.panel57.TabIndex = 51;
            this.panel57.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel58
            // 
            this.panel58.BackColor = System.Drawing.Color.Silver;
            this.panel58.Location = new System.Drawing.Point(344, 120);
            this.panel58.Name = "panel58";
            this.panel58.Size = new System.Drawing.Size(25, 25);
            this.panel58.TabIndex = 52;
            this.panel58.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel48
            // 
            this.panel48.BackColor = System.Drawing.Color.Silver;
            this.panel48.Location = new System.Drawing.Point(24, 120);
            this.panel48.Name = "panel48";
            this.panel48.Size = new System.Drawing.Size(25, 25);
            this.panel48.TabIndex = 44;
            this.panel48.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel49
            // 
            this.panel49.BackColor = System.Drawing.Color.Silver;
            this.panel49.Location = new System.Drawing.Point(56, 120);
            this.panel49.Name = "panel49";
            this.panel49.Size = new System.Drawing.Size(25, 25);
            this.panel49.TabIndex = 45;
            this.panel49.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel50
            // 
            this.panel50.BackColor = System.Drawing.Color.Silver;
            this.panel50.Location = new System.Drawing.Point(88, 120);
            this.panel50.Name = "panel50";
            this.panel50.Size = new System.Drawing.Size(25, 25);
            this.panel50.TabIndex = 46;
            this.panel50.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel51
            // 
            this.panel51.BackColor = System.Drawing.Color.Silver;
            this.panel51.Location = new System.Drawing.Point(120, 120);
            this.panel51.Name = "panel51";
            this.panel51.Size = new System.Drawing.Size(25, 25);
            this.panel51.TabIndex = 47;
            this.panel51.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel53
            // 
            this.panel53.BackColor = System.Drawing.Color.Silver;
            this.panel53.Location = new System.Drawing.Point(184, 120);
            this.panel53.Name = "panel53";
            this.panel53.Size = new System.Drawing.Size(25, 25);
            this.panel53.TabIndex = 50;
            this.panel53.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel54
            // 
            this.panel54.BackColor = System.Drawing.Color.Silver;
            this.panel54.Location = new System.Drawing.Point(216, 120);
            this.panel54.Name = "panel54";
            this.panel54.Size = new System.Drawing.Size(25, 25);
            this.panel54.TabIndex = 53;
            this.panel54.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel55
            // 
            this.panel55.BackColor = System.Drawing.Color.Silver;
            this.panel55.Location = new System.Drawing.Point(248, 120);
            this.panel55.Name = "panel55";
            this.panel55.Size = new System.Drawing.Size(25, 25);
            this.panel55.TabIndex = 55;
            this.panel55.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel52
            // 
            this.panel52.BackColor = System.Drawing.Color.Silver;
            this.panel52.Location = new System.Drawing.Point(152, 120);
            this.panel52.Name = "panel52";
            this.panel52.Size = new System.Drawing.Size(25, 25);
            this.panel52.TabIndex = 48;
            this.panel52.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel56
            // 
            this.panel56.BackColor = System.Drawing.Color.Silver;
            this.panel56.Location = new System.Drawing.Point(280, 120);
            this.panel56.Name = "panel56";
            this.panel56.Size = new System.Drawing.Size(25, 25);
            this.panel56.TabIndex = 49;
            this.panel56.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel59
            // 
            this.panel59.BackColor = System.Drawing.Color.Silver;
            this.panel59.Location = new System.Drawing.Point(376, 120);
            this.panel59.Name = "panel59";
            this.panel59.Size = new System.Drawing.Size(25, 25);
            this.panel59.TabIndex = 54;
            this.panel59.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel62
            // 
            this.panel62.BackColor = System.Drawing.Color.Silver;
            this.panel62.Location = new System.Drawing.Point(472, 120);
            this.panel62.Name = "panel62";
            this.panel62.Size = new System.Drawing.Size(25, 25);
            this.panel62.TabIndex = 58;
            this.panel62.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel63
            // 
            this.panel63.BackColor = System.Drawing.Color.Silver;
            this.panel63.Location = new System.Drawing.Point(504, 120);
            this.panel63.Name = "panel63";
            this.panel63.Size = new System.Drawing.Size(25, 25);
            this.panel63.TabIndex = 59;
            this.panel63.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel61
            // 
            this.panel61.BackColor = System.Drawing.Color.Silver;
            this.panel61.Location = new System.Drawing.Point(440, 120);
            this.panel61.Name = "panel61";
            this.panel61.Size = new System.Drawing.Size(25, 25);
            this.panel61.TabIndex = 57;
            this.panel61.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // panel60
            // 
            this.panel60.BackColor = System.Drawing.Color.Silver;
            this.panel60.Location = new System.Drawing.Point(408, 120);
            this.panel60.Name = "panel60";
            this.panel60.Size = new System.Drawing.Size(25, 25);
            this.panel60.TabIndex = 56;
            this.panel60.Click += new System.EventHandler(this.coloredPanelClick);
            // 
            // mOKButton
            // 
            this.mOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.mOKButton.Location = new System.Drawing.Point(368, 168);
            this.mOKButton.Name = "mOKButton";
            this.mOKButton.Size = new System.Drawing.Size(75, 23);
            this.mOKButton.TabIndex = 60;
            this.mOKButton.Text = "&OK";
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(456, 168);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 61;
            this.mCancelButton.Text = "&Cancel";
            // 
            // mCurrentColorTextBox
            // 
            this.mCurrentColorTextBox.BackColor = System.Drawing.Color.Black;
            this.mCurrentColorTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mCurrentColorTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mCurrentColorTextBox.ForeColor = System.Drawing.Color.Lime;
            this.mCurrentColorTextBox.Location = new System.Drawing.Point(24, 160);
            this.mCurrentColorTextBox.Name = "mCurrentColorTextBox";
            this.mCurrentColorTextBox.Size = new System.Drawing.Size(31, 26);
            this.mCurrentColorTextBox.TabIndex = 62;
            this.mCurrentColorTextBox.Text = "00";
            // 
            // mCurrentColorPanel
            // 
            this.mCurrentColorPanel.BackColor = System.Drawing.Color.Silver;
            this.mCurrentColorPanel.Location = new System.Drawing.Point(72, 160);
            this.mCurrentColorPanel.Name = "mCurrentColorPanel";
            this.mCurrentColorPanel.Size = new System.Drawing.Size(25, 25);
            this.mCurrentColorPanel.TabIndex = 47;
            // 
            // mDebugBox
            // 
            this.mDebugBox.Location = new System.Drawing.Point(144, 160);
            this.mDebugBox.Name = "mDebugBox";
            this.mDebugBox.Size = new System.Drawing.Size(208, 20);
            this.mDebugBox.TabIndex = 63;
            this.mDebugBox.Visible = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(504, 23);
            this.label1.TabIndex = 64;
            this.label1.Text = "0   1  2  3   4  5  6  7  8   9  A  B   C  D  E  F  ";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 25);
            this.label2.TabIndex = 65;
            this.label2.Text = "0";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(0, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 25);
            this.label3.TabIndex = 66;
            this.label3.Text = "1";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(0, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 25);
            this.label4.TabIndex = 67;
            this.label4.Text = "2";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(0, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(20, 25);
            this.label5.TabIndex = 68;
            this.label5.Text = "3";
            // 
            // ColorForm2
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(528, 194);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mDebugBox);
            this.Controls.Add(this.mCurrentColorTextBox);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOKButton);
            this.Controls.Add(this.panel0);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.panel7);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel8);
            this.Controls.Add(this.panel9);
            this.Controls.Add(this.panel10);
            this.Controls.Add(this.panel11);
            this.Controls.Add(this.panel14);
            this.Controls.Add(this.panel15);
            this.Controls.Add(this.panel13);
            this.Controls.Add(this.panel12);
            this.Controls.Add(this.panel25);
            this.Controls.Add(this.panel26);
            this.Controls.Add(this.panel16);
            this.Controls.Add(this.panel17);
            this.Controls.Add(this.panel18);
            this.Controls.Add(this.panel19);
            this.Controls.Add(this.panel21);
            this.Controls.Add(this.panel22);
            this.Controls.Add(this.panel23);
            this.Controls.Add(this.panel20);
            this.Controls.Add(this.panel24);
            this.Controls.Add(this.panel27);
            this.Controls.Add(this.panel30);
            this.Controls.Add(this.panel31);
            this.Controls.Add(this.panel29);
            this.Controls.Add(this.panel28);
            this.Controls.Add(this.panel57);
            this.Controls.Add(this.panel44);
            this.Controls.Add(this.panel45);
            this.Controls.Add(this.panel47);
            this.Controls.Add(this.panel46);
            this.Controls.Add(this.panel43);
            this.Controls.Add(this.panel42);
            this.Controls.Add(this.panel41);
            this.Controls.Add(this.panel40);
            this.Controls.Add(this.panel36);
            this.Controls.Add(this.panel39);
            this.Controls.Add(this.panel38);
            this.Controls.Add(this.panel37);
            this.Controls.Add(this.panel35);
            this.Controls.Add(this.panel34);
            this.Controls.Add(this.panel32);
            this.Controls.Add(this.panel33);
            this.Controls.Add(this.panel60);
            this.Controls.Add(this.panel61);
            this.Controls.Add(this.panel63);
            this.Controls.Add(this.panel62);
            this.Controls.Add(this.panel59);
            this.Controls.Add(this.panel56);
            this.Controls.Add(this.panel52);
            this.Controls.Add(this.panel55);
            this.Controls.Add(this.panel54);
            this.Controls.Add(this.panel53);
            this.Controls.Add(this.panel51);
            this.Controls.Add(this.panel50);
            this.Controls.Add(this.panel49);
            this.Controls.Add(this.panel48);
            this.Controls.Add(this.panel58);
            this.Controls.Add(this.mCurrentColorPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(544, 232);
            this.MinimumSize = new System.Drawing.Size(544, 232);
            this.Name = "ColorForm2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ColorForm2";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void coloredPanelClick(object sender, System.EventArgs e)
		{
			Panel p = sender as Panel;
			if( p != null )
			{
				CurrentColor = p.BackColor;
				CurrentColorString = GetColorString(p);
				mDebugBox.Text = String.Format(
					"name={0}  r={1}g={2}b={3}"
					,p.Name, p.BackColor.R,p.BackColor.G,p.BackColor.B);
			}
		}
	}
}
