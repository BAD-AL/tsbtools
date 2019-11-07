using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;

namespace TSBTool
{
	/// <summary>
	/// Summary description for CxRomTeamHomeAwayUniformForm.
	/// </summary>
	public class CxRomTeamHomeAwayUniformForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox checkBox3;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.CheckBox checkBox5;
		private System.Windows.Forms.CheckBox checkBox6;
		private System.Windows.Forms.CheckBox checkBox7;
		private System.Windows.Forms.CheckBox checkBox8;
		private System.Windows.Forms.CheckBox checkBox9;
		private System.Windows.Forms.CheckBox checkBox10;
		private System.Windows.Forms.CheckBox checkBox11;
		private System.Windows.Forms.CheckBox checkBox12;
		private System.Windows.Forms.CheckBox checkBox13;
		private System.Windows.Forms.CheckBox checkBox14;
		private System.Windows.Forms.CheckBox checkBox15;
		private System.Windows.Forms.CheckBox checkBox16;
		private System.Windows.Forms.CheckBox checkBox17;
		private System.Windows.Forms.CheckBox checkBox18;
		private System.Windows.Forms.CheckBox checkBox19;
		private System.Windows.Forms.CheckBox checkBox20;
		private System.Windows.Forms.CheckBox checkBox21;
		private System.Windows.Forms.CheckBox checkBox22;
		private System.Windows.Forms.CheckBox checkBox23;
		private System.Windows.Forms.CheckBox checkBox24;
		private System.Windows.Forms.CheckBox checkBox25;
		private System.Windows.Forms.CheckBox checkBox26;
		private System.Windows.Forms.CheckBox checkBox27;
		private System.Windows.Forms.CheckBox checkBox28;
		private System.Windows.Forms.CheckBox checkBox29;
		private System.Windows.Forms.CheckBox checkBox30;
		private System.Windows.Forms.Panel mBottomPanel;
		private System.Windows.Forms.TextBox mResultTextBox;
		private System.Windows.Forms.Button mOkButton;
		private System.Windows.Forms.Button mCancelButton;
		private System.Windows.Forms.CheckBox checkBox31;
		private System.Windows.Forms.CheckBox checkBox32;

		private CheckBox[] mCheckBoxes = new CheckBox[32];
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem mSelectAllMenuItem;
		private System.Windows.Forms.MenuItem mUnSelectAllMenuItem;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CxRomTeamHomeAwayUniformForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			mCheckBoxes[0] =checkBox1;
			mCheckBoxes[1] =checkBox2;
			mCheckBoxes[2] =checkBox3;
			mCheckBoxes[3] =checkBox4;
			mCheckBoxes[4] =checkBox5;
			mCheckBoxes[5] =checkBox6;
			mCheckBoxes[6] =checkBox7;
			mCheckBoxes[7] =checkBox8;
			mCheckBoxes[8] =checkBox9;
			mCheckBoxes[9] =checkBox10;
			mCheckBoxes[10] =checkBox11;
			mCheckBoxes[11] =checkBox12;
			mCheckBoxes[12] =checkBox13;
			mCheckBoxes[13] =checkBox14;
			mCheckBoxes[14] =checkBox15;
			mCheckBoxes[15] =checkBox16;
			mCheckBoxes[16] =checkBox17;
			mCheckBoxes[17] =checkBox18;
			mCheckBoxes[18] =checkBox19;
			mCheckBoxes[19] =checkBox20;
			mCheckBoxes[20] =checkBox21;
			mCheckBoxes[21] =checkBox22;
			mCheckBoxes[22] =checkBox23;
			mCheckBoxes[23] =checkBox24;
			mCheckBoxes[24] =checkBox25;
			mCheckBoxes[25] =checkBox26;
			mCheckBoxes[26] =checkBox27;
			mCheckBoxes[27] =checkBox28;
			mCheckBoxes[28] =checkBox29;
			mCheckBoxes[29] =checkBox30;
			mCheckBoxes[30] =checkBox31;
			mCheckBoxes[31] =checkBox32;
			UpdateResult();
		}

		private Regex mStringValueRegex = new Regex("([0-9a-fA-F]{1,8})");
		
		private string mStringValue;

		public string StringValue
		{
			get
			{
				return mStringValue;
			}
			set
			{
				Match m = mStringValueRegex.Match(value);
				if( m != Match.Empty )
				{
					mStringValue = m.Groups[1].Value;
					uint stuff = 0x00000000;
					bool error = false;
					try
					{
						stuff = uint.Parse(mStringValue, System.Globalization.NumberStyles.AllowHexSpecifier);
						
					}
					catch(Exception )
					{
						error = true;
						MessageBox.Show(this, 
							string.Format("Error setting up form, bad data '{0}'",value), 
							"Error!",MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					if( !error)
					{
						SetupCheckBoxes(stuff);
						if( !updating)
							UpdateResult();
					}
					
				}
			}
		}

		private void SetAllBoxes( bool val)
		{
			for(int i =0; i < mCheckBoxes.Length; i++)
			{
				mCheckBoxes[i].Checked = val;
			}
		}

		private void SetupCheckBoxes(uint data)
		{
			SetAllBoxes(false);

			if( (data & 0x80000000) > 0)
				mCheckBoxes[0].Checked=true;
			if( (data & 0x40000000) > 0)
				mCheckBoxes[1].Checked=true;
			if( (data & 0x20000000) > 0)
				mCheckBoxes[2].Checked=true;
			if( (data & 0x10000000) > 0)
				mCheckBoxes[3].Checked=true;
			if( (data & 0x08000000) > 0)
				mCheckBoxes[4].Checked=true;
			if( (data & 0x04000000) > 0)
				mCheckBoxes[5].Checked=true;
			if( (data & 0x02000000) > 0)
				mCheckBoxes[6].Checked=true;
			if( (data & 0x01000000) > 0)
				mCheckBoxes[7].Checked=true;
			if( (data & 0x00800000) > 0)
				mCheckBoxes[8].Checked=true;
			if( (data & 0x00400000) > 0)
				mCheckBoxes[9].Checked=true;
			if( (data & 0x00200000) > 0)
				mCheckBoxes[10].Checked=true;
			if( (data & 0x00100000) > 0)
				mCheckBoxes[11].Checked=true;
			if( (data & 0x00080000) > 0)
				mCheckBoxes[12].Checked=true;
			if( (data & 0x00040000) > 0)
				mCheckBoxes[13].Checked=true;
			if( (data & 0x00020000) > 0)
				mCheckBoxes[14].Checked=true;
			if( (data & 0x00010000) > 0)
				mCheckBoxes[15].Checked=true;
			if( (data & 0x00008000) > 0)
				mCheckBoxes[16].Checked=true;
			if( (data & 0x00004000) > 0)
				mCheckBoxes[17].Checked=true;
			if( (data & 0x00002000) > 0)
				mCheckBoxes[18].Checked=true;
			if( (data & 0x00001000) > 0)
				mCheckBoxes[19].Checked=true;
			if( (data & 0x00000800) > 0)
				mCheckBoxes[20].Checked=true;
			if( (data & 0x00000400) > 0)
				mCheckBoxes[21].Checked=true;
			if( (data & 0x00000200) > 0)
				mCheckBoxes[22].Checked=true;
			if( (data & 0x00000100) > 0)
				mCheckBoxes[23].Checked=true;
			if( (data & 0x00000080) > 0)
				mCheckBoxes[24].Checked=true;
			if( (data & 0x00000040) > 0)
				mCheckBoxes[25].Checked=true;
			if( (data & 0x00000020) > 0)
				mCheckBoxes[26].Checked=true;
			if( (data & 0x00000010) > 0)
				mCheckBoxes[27].Checked=true;

			if( (data & 0x00000008) > 0)
				mCheckBoxes[28].Checked=true;
			if( (data & 0x00000004) > 0)
				mCheckBoxes[29].Checked=true;
			if( (data & 0x00000002) > 0)
				mCheckBoxes[30].Checked=true;
			
			if( (data & 0x00000001) > 0)
				mCheckBoxes[31].Checked=true;
		}

		public uint Value
		{
			get
			{
				uint result = 0x00000000;
				if(mCheckBoxes[0].Checked)
					result += 0x80000000;
				if(mCheckBoxes[1].Checked)
					result += 0x40000000;
				if(mCheckBoxes[2].Checked)
					result += 0x20000000;
				if(mCheckBoxes[3].Checked)
					result += 0x10000000;
				if(mCheckBoxes[4].Checked)
					result += 0x08000000;
				if(mCheckBoxes[5].Checked)
					result += 0x04000000;
				if(mCheckBoxes[6].Checked)
					result += 0x02000000;
				if(mCheckBoxes[7].Checked)
					result += 0x01000000;
				if(mCheckBoxes[8].Checked)
					result += 0x00800000;
				if(mCheckBoxes[9].Checked)
					result += 0x00400000;
				if(mCheckBoxes[10].Checked)
					result += 0x00200000;
				if(mCheckBoxes[11].Checked)
					result += 0x00100000;
				if(mCheckBoxes[12].Checked)
					result += 0x00080000;
				if(mCheckBoxes[13].Checked)
					result += 0x00040000;
				if(mCheckBoxes[14].Checked)
					result += 0x00020000;
				if(mCheckBoxes[15].Checked)
					result += 0x00010000;
				if(mCheckBoxes[16].Checked)
					result += 0x00008000;
				if(mCheckBoxes[17].Checked)
					result += 0x00004000;
				if(mCheckBoxes[18].Checked)
					result += 0x00002000;
				if(mCheckBoxes[19].Checked)
					result += 0x00001000;
				if(mCheckBoxes[20].Checked)
					result += 0x00000800;
				if(mCheckBoxes[21].Checked)
					result += 0x00000400;
				if(mCheckBoxes[22].Checked)
					result += 0x00000200;
				if(mCheckBoxes[23].Checked)
					result += 0x00000100;
				if(mCheckBoxes[24].Checked)
					result += 0x00000080;
				if(mCheckBoxes[25].Checked)
					result += 0x00000040;
				if(mCheckBoxes[26].Checked)
					result += 0x00000020;
				if(mCheckBoxes[27].Checked)
					result += 0x00000010;

				if(mCheckBoxes[28].Checked)
					result += 0x00000008;
				if(mCheckBoxes[29].Checked)
					result += 0x00000004;
				if(mCheckBoxes[30].Checked)
					result += 0x00000002;
				if(mCheckBoxes[31].Checked)
					result += 0x00000001;

				return result;
			}
		}

		private void UpdateResult()
		{
			if( !updating)
			{
				mResultTextBox.Text = String.Format("{0:X8}",Value);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CxRomTeamHomeAwayUniformForm));
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.checkBox11 = new System.Windows.Forms.CheckBox();
            this.checkBox12 = new System.Windows.Forms.CheckBox();
            this.checkBox13 = new System.Windows.Forms.CheckBox();
            this.checkBox14 = new System.Windows.Forms.CheckBox();
            this.checkBox15 = new System.Windows.Forms.CheckBox();
            this.checkBox16 = new System.Windows.Forms.CheckBox();
            this.checkBox17 = new System.Windows.Forms.CheckBox();
            this.checkBox18 = new System.Windows.Forms.CheckBox();
            this.checkBox19 = new System.Windows.Forms.CheckBox();
            this.checkBox20 = new System.Windows.Forms.CheckBox();
            this.checkBox21 = new System.Windows.Forms.CheckBox();
            this.checkBox22 = new System.Windows.Forms.CheckBox();
            this.checkBox23 = new System.Windows.Forms.CheckBox();
            this.checkBox24 = new System.Windows.Forms.CheckBox();
            this.checkBox25 = new System.Windows.Forms.CheckBox();
            this.checkBox26 = new System.Windows.Forms.CheckBox();
            this.checkBox27 = new System.Windows.Forms.CheckBox();
            this.checkBox28 = new System.Windows.Forms.CheckBox();
            this.checkBox29 = new System.Windows.Forms.CheckBox();
            this.checkBox30 = new System.Windows.Forms.CheckBox();
            this.mBottomPanel = new System.Windows.Forms.Panel();
            this.mResultTextBox = new System.Windows.Forms.TextBox();
            this.mOkButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.checkBox31 = new System.Windows.Forms.CheckBox();
            this.checkBox32 = new System.Windows.Forms.CheckBox();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.mSelectAllMenuItem = new System.Windows.Forms.MenuItem();
            this.mUnSelectAllMenuItem = new System.Windows.Forms.MenuItem();
            this.mBottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox1.Location = new System.Drawing.Point(0, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(16, 24);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox2.Location = new System.Drawing.Point(0, 48);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(16, 24);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.UseVisualStyleBackColor = false;
            this.checkBox2.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox3
            // 
            this.checkBox3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox3.Location = new System.Drawing.Point(0, 80);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(16, 24);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.UseVisualStyleBackColor = false;
            this.checkBox3.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox4
            // 
            this.checkBox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox4.Location = new System.Drawing.Point(0, 112);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(16, 24);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.UseVisualStyleBackColor = false;
            this.checkBox4.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox5
            // 
            this.checkBox5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox5.Location = new System.Drawing.Point(128, 16);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(16, 24);
            this.checkBox5.TabIndex = 4;
            this.checkBox5.UseVisualStyleBackColor = false;
            this.checkBox5.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox6
            // 
            this.checkBox6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox6.Location = new System.Drawing.Point(128, 48);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(16, 24);
            this.checkBox6.TabIndex = 5;
            this.checkBox6.UseVisualStyleBackColor = false;
            this.checkBox6.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox7
            // 
            this.checkBox7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox7.Location = new System.Drawing.Point(128, 80);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(16, 24);
            this.checkBox7.TabIndex = 6;
            this.checkBox7.UseVisualStyleBackColor = false;
            this.checkBox7.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox8
            // 
            this.checkBox8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox8.Location = new System.Drawing.Point(128, 112);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(16, 24);
            this.checkBox8.TabIndex = 7;
            this.checkBox8.UseVisualStyleBackColor = false;
            this.checkBox8.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox9
            // 
            this.checkBox9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox9.Location = new System.Drawing.Point(256, 16);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(16, 24);
            this.checkBox9.TabIndex = 8;
            this.checkBox9.UseVisualStyleBackColor = false;
            this.checkBox9.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox10
            // 
            this.checkBox10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox10.Location = new System.Drawing.Point(256, 48);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(16, 24);
            this.checkBox10.TabIndex = 9;
            this.checkBox10.UseVisualStyleBackColor = false;
            this.checkBox10.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox11
            // 
            this.checkBox11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox11.Location = new System.Drawing.Point(256, 80);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(16, 24);
            this.checkBox11.TabIndex = 10;
            this.checkBox11.UseVisualStyleBackColor = false;
            this.checkBox11.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox12
            // 
            this.checkBox12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox12.Location = new System.Drawing.Point(256, 112);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(16, 24);
            this.checkBox12.TabIndex = 11;
            this.checkBox12.UseVisualStyleBackColor = false;
            this.checkBox12.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox13
            // 
            this.checkBox13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox13.Location = new System.Drawing.Point(384, 16);
            this.checkBox13.Name = "checkBox13";
            this.checkBox13.Size = new System.Drawing.Size(16, 24);
            this.checkBox13.TabIndex = 12;
            this.checkBox13.UseVisualStyleBackColor = false;
            this.checkBox13.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox14
            // 
            this.checkBox14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox14.Location = new System.Drawing.Point(384, 48);
            this.checkBox14.Name = "checkBox14";
            this.checkBox14.Size = new System.Drawing.Size(16, 24);
            this.checkBox14.TabIndex = 13;
            this.checkBox14.UseVisualStyleBackColor = false;
            this.checkBox14.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox15
            // 
            this.checkBox15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox15.Location = new System.Drawing.Point(384, 80);
            this.checkBox15.Name = "checkBox15";
            this.checkBox15.Size = new System.Drawing.Size(16, 24);
            this.checkBox15.TabIndex = 14;
            this.checkBox15.UseVisualStyleBackColor = false;
            this.checkBox15.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox16
            // 
            this.checkBox16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox16.Location = new System.Drawing.Point(384, 112);
            this.checkBox16.Name = "checkBox16";
            this.checkBox16.Size = new System.Drawing.Size(16, 24);
            this.checkBox16.TabIndex = 15;
            this.checkBox16.UseVisualStyleBackColor = false;
            this.checkBox16.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox17
            // 
            this.checkBox17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox17.Location = new System.Drawing.Point(0, 176);
            this.checkBox17.Name = "checkBox17";
            this.checkBox17.Size = new System.Drawing.Size(16, 24);
            this.checkBox17.TabIndex = 16;
            this.checkBox17.UseVisualStyleBackColor = false;
            this.checkBox17.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox18
            // 
            this.checkBox18.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox18.Location = new System.Drawing.Point(0, 208);
            this.checkBox18.Name = "checkBox18";
            this.checkBox18.Size = new System.Drawing.Size(16, 24);
            this.checkBox18.TabIndex = 17;
            this.checkBox18.UseVisualStyleBackColor = false;
            this.checkBox18.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox19
            // 
            this.checkBox19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox19.Location = new System.Drawing.Point(0, 240);
            this.checkBox19.Name = "checkBox19";
            this.checkBox19.Size = new System.Drawing.Size(16, 24);
            this.checkBox19.TabIndex = 18;
            this.checkBox19.UseVisualStyleBackColor = false;
            this.checkBox19.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox20
            // 
            this.checkBox20.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox20.Location = new System.Drawing.Point(0, 272);
            this.checkBox20.Name = "checkBox20";
            this.checkBox20.Size = new System.Drawing.Size(16, 24);
            this.checkBox20.TabIndex = 19;
            this.checkBox20.UseVisualStyleBackColor = false;
            this.checkBox20.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox21
            // 
            this.checkBox21.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox21.Location = new System.Drawing.Point(128, 176);
            this.checkBox21.Name = "checkBox21";
            this.checkBox21.Size = new System.Drawing.Size(16, 24);
            this.checkBox21.TabIndex = 20;
            this.checkBox21.UseVisualStyleBackColor = false;
            this.checkBox21.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox22
            // 
            this.checkBox22.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox22.Location = new System.Drawing.Point(128, 208);
            this.checkBox22.Name = "checkBox22";
            this.checkBox22.Size = new System.Drawing.Size(16, 24);
            this.checkBox22.TabIndex = 21;
            this.checkBox22.UseVisualStyleBackColor = false;
            this.checkBox22.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox23
            // 
            this.checkBox23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox23.Location = new System.Drawing.Point(128, 240);
            this.checkBox23.Name = "checkBox23";
            this.checkBox23.Size = new System.Drawing.Size(16, 24);
            this.checkBox23.TabIndex = 22;
            this.checkBox23.UseVisualStyleBackColor = false;
            this.checkBox23.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox24
            // 
            this.checkBox24.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox24.Location = new System.Drawing.Point(128, 272);
            this.checkBox24.Name = "checkBox24";
            this.checkBox24.Size = new System.Drawing.Size(16, 24);
            this.checkBox24.TabIndex = 23;
            this.checkBox24.UseVisualStyleBackColor = false;
            this.checkBox24.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox25
            // 
            this.checkBox25.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox25.Location = new System.Drawing.Point(256, 176);
            this.checkBox25.Name = "checkBox25";
            this.checkBox25.Size = new System.Drawing.Size(16, 24);
            this.checkBox25.TabIndex = 24;
            this.checkBox25.UseVisualStyleBackColor = false;
            this.checkBox25.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox26
            // 
            this.checkBox26.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox26.Location = new System.Drawing.Point(256, 208);
            this.checkBox26.Name = "checkBox26";
            this.checkBox26.Size = new System.Drawing.Size(16, 24);
            this.checkBox26.TabIndex = 25;
            this.checkBox26.UseVisualStyleBackColor = false;
            this.checkBox26.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox27
            // 
            this.checkBox27.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox27.Location = new System.Drawing.Point(256, 240);
            this.checkBox27.Name = "checkBox27";
            this.checkBox27.Size = new System.Drawing.Size(16, 24);
            this.checkBox27.TabIndex = 26;
            this.checkBox27.UseVisualStyleBackColor = false;
            this.checkBox27.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox28
            // 
            this.checkBox28.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox28.Location = new System.Drawing.Point(256, 272);
            this.checkBox28.Name = "checkBox28";
            this.checkBox28.Size = new System.Drawing.Size(16, 24);
            this.checkBox28.TabIndex = 27;
            this.checkBox28.UseVisualStyleBackColor = false;
            this.checkBox28.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox29
            // 
            this.checkBox29.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox29.Location = new System.Drawing.Point(216, 152);
            this.checkBox29.Name = "checkBox29";
            this.checkBox29.Size = new System.Drawing.Size(16, 24);
            this.checkBox29.TabIndex = 28;
            this.checkBox29.UseVisualStyleBackColor = false;
            this.checkBox29.Visible = false;
            // 
            // checkBox30
            // 
            this.checkBox30.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox30.Location = new System.Drawing.Point(256, 152);
            this.checkBox30.Name = "checkBox30";
            this.checkBox30.Size = new System.Drawing.Size(16, 24);
            this.checkBox30.TabIndex = 29;
            this.checkBox30.UseVisualStyleBackColor = false;
            this.checkBox30.Visible = false;
            // 
            // mBottomPanel
            // 
            this.mBottomPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.mBottomPanel.Controls.Add(this.mResultTextBox);
            this.mBottomPanel.Controls.Add(this.mOkButton);
            this.mBottomPanel.Controls.Add(this.mCancelButton);
            this.mBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.mBottomPanel.Location = new System.Drawing.Point(0, 306);
            this.mBottomPanel.Name = "mBottomPanel";
            this.mBottomPanel.Size = new System.Drawing.Size(512, 48);
            this.mBottomPanel.TabIndex = 38;
            // 
            // mResultTextBox
            // 
            this.mResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.mResultTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.mResultTextBox.Font = new System.Drawing.Font("Courier New", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mResultTextBox.ForeColor = System.Drawing.Color.White;
            this.mResultTextBox.Location = new System.Drawing.Point(8, 16);
            this.mResultTextBox.MaxLength = 8;
            this.mResultTextBox.Name = "mResultTextBox";
            this.mResultTextBox.Size = new System.Drawing.Size(152, 29);
            this.mResultTextBox.TabIndex = 36;
            this.mResultTextBox.Text = "0011223344";
            this.mResultTextBox.TextChanged += new System.EventHandler(this.mResultTextBox_TextChanged);
            // 
            // mOkButton
            // 
            this.mOkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.mOkButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mOkButton.ForeColor = System.Drawing.Color.White;
            this.mOkButton.Location = new System.Drawing.Point(344, 8);
            this.mOkButton.Name = "mOkButton";
            this.mOkButton.Size = new System.Drawing.Size(75, 32);
            this.mOkButton.TabIndex = 34;
            this.mOkButton.Text = "&OK";
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mCancelButton.ForeColor = System.Drawing.Color.White;
            this.mCancelButton.Location = new System.Drawing.Point(432, 8);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 32);
            this.mCancelButton.TabIndex = 35;
            this.mCancelButton.Text = "&Cancel";
            // 
            // checkBox31
            // 
            this.checkBox31.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox31.Location = new System.Drawing.Point(384, 176);
            this.checkBox31.Name = "checkBox31";
            this.checkBox31.Size = new System.Drawing.Size(16, 24);
            this.checkBox31.TabIndex = 39;
            this.checkBox31.UseVisualStyleBackColor = false;
            this.checkBox31.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // checkBox32
            // 
            this.checkBox32.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(113)))), ((int)(((byte)(239)))));
            this.checkBox32.Location = new System.Drawing.Point(384, 216);
            this.checkBox32.Name = "checkBox32";
            this.checkBox32.Size = new System.Drawing.Size(16, 24);
            this.checkBox32.TabIndex = 40;
            this.checkBox32.UseVisualStyleBackColor = false;
            this.checkBox32.Click += new System.EventHandler(this.checkBox23_Click);
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mSelectAllMenuItem,
            this.mUnSelectAllMenuItem});
            // 
            // mSelectAllMenuItem
            // 
            this.mSelectAllMenuItem.Index = 0;
            this.mSelectAllMenuItem.Text = "Select All";
            this.mSelectAllMenuItem.Click += new System.EventHandler(this.mSelectAllMenuItem_Click);
            // 
            // mUnSelectAllMenuItem
            // 
            this.mUnSelectAllMenuItem.Index = 1;
            this.mUnSelectAllMenuItem.Text = "Unselect All";
            this.mUnSelectAllMenuItem.Click += new System.EventHandler(this.mUnSelectAllMenuItem_Click);
            // 
            // CxRomTeamHomeAwayUniformForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(512, 354);
            this.ContextMenu = this.contextMenu1;
            this.Controls.Add(this.checkBox32);
            this.Controls.Add(this.checkBox31);
            this.Controls.Add(this.mBottomPanel);
            this.Controls.Add(this.checkBox30);
            this.Controls.Add(this.checkBox29);
            this.Controls.Add(this.checkBox28);
            this.Controls.Add(this.checkBox27);
            this.Controls.Add(this.checkBox26);
            this.Controls.Add(this.checkBox25);
            this.Controls.Add(this.checkBox24);
            this.Controls.Add(this.checkBox23);
            this.Controls.Add(this.checkBox22);
            this.Controls.Add(this.checkBox21);
            this.Controls.Add(this.checkBox20);
            this.Controls.Add(this.checkBox19);
            this.Controls.Add(this.checkBox18);
            this.Controls.Add(this.checkBox17);
            this.Controls.Add(this.checkBox16);
            this.Controls.Add(this.checkBox15);
            this.Controls.Add(this.checkBox14);
            this.Controls.Add(this.checkBox13);
            this.Controls.Add(this.checkBox12);
            this.Controls.Add(this.checkBox11);
            this.Controls.Add(this.checkBox10);
            this.Controls.Add(this.checkBox9);
            this.Controls.Add(this.checkBox8);
            this.Controls.Add(this.checkBox7);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(528, 392);
            this.MinimumSize = new System.Drawing.Size(528, 392);
            this.Name = "CxRomTeamHomeAwayUniformForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Use Uniform 2 against";
            this.mBottomPanel.ResumeLayout(false);
            this.mBottomPanel.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		
		private bool updating = false;

		private void checkBox23_Click(object sender, System.EventArgs e)
		{
			UpdateResult();
		}

		
		private void mSelectAllMenuItem_Click(object sender, System.EventArgs e)
		{
			this.SetAllBoxes(true);
			UpdateResult();
		}

		private void mUnSelectAllMenuItem_Click(object sender, System.EventArgs e)
		{
			this.SetAllBoxes(false);
			UpdateResult();
		}

		private void mResultTextBox_TextChanged(object sender, System.EventArgs e)
		{
			if( !updating)
			{
				updating = true;
				StringValue = mResultTextBox.Text;
				updating = false;
			}
		}
	}
}
