using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;


namespace PlayProto
{
	/// <summary>
	/// Summary description for TestForm.
	/// </summary>
	public class TestForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button m_RunTestButton;
		private System.Windows.Forms.ComboBox m_TestSelectComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RichTextBox m_ResultsRichTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox m_InputTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.ContextMenu m_ResultsContextMenu;
		private System.Windows.Forms.MenuItem m_CutMenuItem;
		private System.Windows.Forms.MenuItem m_CopyMenuItem;
		private System.Windows.Forms.MenuItem m_PasteMenuItem;
		private string m_SearchString = "";

		private string[] m_ComboItems = { 
"Get Off pattern", 
"Get offensive plays", 
"GetPlayNamesAndNumbers",
"PatternTest",
"Get Def pattern",
"Get Def Plays",
//"Spreadsheet Test",
//"Grid Test",
"Pattern Reader (jstout program output)",
"BurdDogDefense",
"Get bytes [start address, #bytes]",
"Set bytes [address, value]",
"Curve Form",
"Formation x y",
"Field Form",
"Get plays pointer appears in"
										};

		public TestForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			for(int i = 0 ; i < m_ComboItems.Length; i++)
			{
				m_TestSelectComboBox.Items.Add(m_ComboItems[i]);
			}
			m_TestSelectComboBox.SelectedIndex = 0;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TestForm));
			this.m_RunTestButton = new System.Windows.Forms.Button();
			this.m_TestSelectComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.m_ResultsRichTextBox = new System.Windows.Forms.RichTextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.m_InputTextBox = new System.Windows.Forms.TextBox();
			this.m_ResultsContextMenu = new System.Windows.Forms.ContextMenu();
			this.m_CutMenuItem = new System.Windows.Forms.MenuItem();
			this.m_CopyMenuItem = new System.Windows.Forms.MenuItem();
			this.m_PasteMenuItem = new System.Windows.Forms.MenuItem();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_RunTestButton
			// 
			this.m_RunTestButton.Location = new System.Drawing.Point(16, 56);
			this.m_RunTestButton.Name = "m_RunTestButton";
			this.m_RunTestButton.Size = new System.Drawing.Size(256, 23);
			this.m_RunTestButton.TabIndex = 10;
			this.m_RunTestButton.Text = "&Run Test";
			this.m_RunTestButton.Click += new System.EventHandler(this.m_RunTestButton_Click);
			// 
			// m_TestSelectComboBox
			// 
			this.m_TestSelectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.m_TestSelectComboBox.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.m_TestSelectComboBox.Location = new System.Drawing.Point(16, 24);
			this.m_TestSelectComboBox.Name = "m_TestSelectComboBox";
			this.m_TestSelectComboBox.Size = new System.Drawing.Size(256, 22);
			this.m_TestSelectComboBox.TabIndex = 5;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 0);
			this.label1.Name = "label1";
			this.label1.TabIndex = 2;
			this.label1.Text = "Choose Test";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.m_ResultsRichTextBox);
			this.groupBox1.Location = new System.Drawing.Point(8, 80);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(400, 256);
			this.groupBox1.TabIndex = 20;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Results";
			// 
			// m_ResultsRichTextBox
			// 
			this.m_ResultsRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.m_ResultsRichTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.m_ResultsRichTextBox.Location = new System.Drawing.Point(0, 16);
			this.m_ResultsRichTextBox.Name = "m_ResultsRichTextBox";
			this.m_ResultsRichTextBox.Size = new System.Drawing.Size(384, 232);
			this.m_ResultsRichTextBox.TabIndex = 5;
			this.m_ResultsRichTextBox.Text = "";
			this.m_ResultsRichTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_ResultsRichTextBox_KeyDown);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(288, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 28);
			this.label2.TabIndex = 4;
			this.label2.Text = "Input: (single int or list of ints [hex])";
			// 
			// m_InputTextBox
			// 
			this.m_InputTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.m_InputTextBox.Location = new System.Drawing.Point(288, 40);
			this.m_InputTextBox.Name = "m_InputTextBox";
			this.m_InputTextBox.TabIndex = 15;
			this.m_InputTextBox.Text = "";
			this.m_InputTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.m_InputTextBox_KeyDown);
			// 
			// m_ResultsContextMenu
			// 
			this.m_ResultsContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								 this.m_CutMenuItem,
																								 this.m_CopyMenuItem,
																								 this.m_PasteMenuItem});
			// 
			// m_CutMenuItem
			// 
			this.m_CutMenuItem.Index = 0;
			this.m_CutMenuItem.Text = "C&ut";
			this.m_CutMenuItem.Click += new System.EventHandler(this.m_CutMenuItem_Click);
			// 
			// m_CopyMenuItem
			// 
			this.m_CopyMenuItem.Index = 1;
			this.m_CopyMenuItem.Text = "&Copy";
			this.m_CopyMenuItem.Click += new System.EventHandler(this.m_CopyMenuItem_Click);
			// 
			// m_PasteMenuItem
			// 
			this.m_PasteMenuItem.Index = 2;
			this.m_PasteMenuItem.Text = "&Paste";
			this.m_PasteMenuItem.Click += new System.EventHandler(this.m_PasteMenuItem_Click);
			// 
			// TestForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(416, 342);
			this.Controls.Add(this.m_InputTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_TestSelectComboBox);
			this.Controls.Add(this.m_RunTestButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(424, 376);
			this.Name = "TestForm";
			this.Text = "TestForm";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void m_RunTestButton_Click(object sender, System.EventArgs e)
		{
			string item = m_TestSelectComboBox.SelectedItem.ToString();

			try
			{
				switch(item)
				{
					case "Get Off pattern":
						GetOffPattern();
						break;
					case "Get offensive plays":
						GetOffensivePlays();
						break;
					case "PatternTest":
						PatternTest();
						break;
					case "Get Def pattern":
						GetDefensivePattern();
						break;
					case "Get Def Plays":
						GetDefensivePlays();
						break;
//					case "Spreadsheet Test":
//						SpreadSheetTest();
//						break;
                    //case "Grid Test":
                    //    GridTest();
                    //    break;
					case "Pattern Reader (jstout program output)":
						DoPatternReader();
						break;
					case "GetPlayNamesAndNumbers":
						GetPlayNamesAndNumbers();
						break;
					case "BurdDogDefense":
						BurdDogDefense();
						break;
					case "Set bytes [address, value]":
						SetBytes();
						break;
					case "Get bytes [start address, #bytes]":
						GetBytes();
						break;
					case "Curve Form":
						CurveForm f = new CurveForm();
						f.Show();
						break;

					case "Formation x y":
						FormationTest();
						break;
					case "Field Form":
						FieldFormTest();
						break;
					case "Get plays pointer appears in":
						PlayPointerTest();
						break;
				}
			}
			catch(Exception ex )
			{
				MessageBox.Show(string.Format(
"An Error has occured while running your test.\nMessage = {0}\nStack trace=\n{1}\n",
					ex.Message, ex.StackTrace
					));
			}
		}

		private void FormationTest()
		{
			string s ="";
			int[] args = GetArgList_i();
			if( args != null )
			{
				for(int index = 0; index < args.Length; index++)
				{
					m_ResultsRichTextBox.AppendText(string.Format("Formation {0:x2}\n",args[index]));
					int[] crap = MainForm.mPlayTool.GetFormationPoints(args[index]);
					for(int i =0; i < crap.Length; i+=4)
					{
						s = string.Format("{0:x2} {1:x2} {2:x2} {3:x2}\n", 
							crap[i], crap[i+1],crap[i+2],crap[i+3]);
						m_ResultsRichTextBox.AppendText(s);
					}
//					for(int i =0; i < crap.Length; i+=4)
//					{
//						s = string.Format("{2},{1} \n", 
//							crap[i], crap[i+1],crap[i+2],crap[i+3]);
//						m_ResultsRichTextBox.AppendText(s);
//					}
					m_ResultsRichTextBox.AppendText("\n");
				}
			}
		}

        //private void GridTest()
        //{
        //    FlexGridForm form = new FlexGridForm();
        //    form.PopulateData( MainForm.TheOffensePanel.PatternNames);
        //    form.SetColumnHeadders(UtilityClass.OffensivePlayers);

        //    string[] passPlayNames = new string[32];
        //    for(int i =0; i < passPlayNames.Length; i++)
        //    {
        //        passPlayNames[i] = MainForm.mPlayTool.GetPlayName(32+i);
        //    }
        //    form.SetRowTitles(passPlayNames);

        //    if( form.ShowDialog(this) == DialogResult.OK )
        //    {
        //        m_ResultsRichTextBox.Text = string.Format(
        //            "row = {0} col = {1}",
        //            form.SelectedRow,
        //            form.SelectedCol
        //            );
        //    }
        //}

		private void FieldFormTest()
		{
			FieldForm frm = new FieldForm();
			int[] tmp = null;
			for(int i = 0; i < 11; i++)
			{
				tmp = MainForm.mPlayTool.GetPlayerFormationPoint(4, (OPlayer) i);
				frm.AddGuy(tmp, UtilityClass.OffensivePlayers[i]);
			}

			frm.Show();
		}

//		private void SpreadSheetTest()
//		{
//			ExcelForm form = new ExcelForm();
//			form.Text = "Select Pattern";
//			form.PopulateData( UtilityClass.OffensivePlayers, MainForm.TheOffensePanel.PatternNames);
//			if( form.ShowDialog(this) == DialogResult.OK )
//			{
//				m_ResultsRichTextBox.Text = 
//					string.Format("The selected cell was row={0}, col={1}",
//					form.SelectedRow, form.SelectedCol);
//			}
//			form.Dispose();
//		}

		private void PatternTest()
		{
			Pattern p = new Pattern();
			CommandTree ss = null;
			int loc = 0x834A;
			int[] list = GetArgList_i();
			if( list != null && list.Length > 0 )
			{
				m_ResultsRichTextBox.Text = "";
				foreach(int i in list)
				{
					// add 0x10 for headder (offense)
					// for defense you would just add 0x10
					ss = p.GetPattern(i + 0x10 - 0x2000, new CommandTree(""));
					m_ResultsRichTextBox.Text +=  ss.ToString() +"\n\n";
				}
			}
			else
			{
				ss = p.GetPattern(loc, new CommandTree(""));
				m_ResultsRichTextBox.Text =  ss.ToString();
			}
		}


		private void GetOffensivePlays()
		{
			StringBuilder sb = new StringBuilder();
			ArrayList list = new ArrayList(8*80);

			for(int i = 0; i < 64; i++)
			{
				sb.Append(string.Format( "Play #{0,2}: {1}\n", i, MainForm.TheMainForm.
					ThePlayTool.GetPlayToCallText(i)));

				byte[] bytes = MainForm.TheMainForm.
					ThePlayTool.GetOffensivePlayToCall(i);
				foreach( byte b in bytes)
				{
					list.Add(b);
				}
			}
			list.Sort();
			sb.Append("\n\nPlays Used:\n");
			byte lastByte = 0xff;
			int guys = 1;
			for(int i =0; i < list.Count; i++ )
			{
				byte b = (byte)list[i];
				if( b != lastByte )
				{
					sb.Append(string.Format("{0:x2},",b));
					if( guys++ % 16 == 0)
						sb.Append("\n");
				}
				lastByte = b;
				
			}
			m_ResultsRichTextBox.Text = sb.ToString();

		}


		private string[] GetArgList_s()
		{
			string[] args_s = null;
			if( m_InputTextBox.Text.Length > 0 )
			{
				char[] seps = {','};
				string stuff = m_InputTextBox.Text.Trim(seps).Replace(" ","");
				args_s = stuff.Split(seps);
			}
			return args_s;
		}

		private int[] GetArgList_i()
		{
			int[] args = null;
			string[] list = GetArgList_s();
			if( list != null && list.Length > 0 )
			{
				args = new int[list.Length];
				try
				{
					for( int i =0; i < list.Length; i++)
					{
						args[i] = Int32.Parse(list[i],System.Globalization.NumberStyles.AllowHexSpecifier);
					}
				}
				catch(Exception e )
				{
					//error = "You need to enter an int!";
					Console.WriteLine(e.Message);
				}
			}
			return args;
		}


		private void GetOffPattern()
		{
			string error = null;
			if( m_InputTextBox.Text.Length < 1 )
				error = "You need to enter an argument";

			if( error == null ) // we're good let's go!
			{
				int[]args = GetArgList_i();
				StringBuilder sb = new StringBuilder();
				StringBuilder sb2 = new StringBuilder();
				for( int j = 0; j < args.Length; j++ )
				{
					sb.Append(string.Format("Play # {0:x2}:\n",args[j]));
					for(int i = 0; i < 11; i++)
					{
						OPlayer p = (OPlayer)i;
						byte[] b = MainForm.mPlayTool.GetOPatternLocationRelative(p, args[j]);
						sb.Append(string.Format("{0,4}: 0x{1:x2}{2:x2}\n",
							UtilityClass.OffensivePlayers[i],b[0],b[1]));
						sb2.Append(string.Format("{0:x2}{1:x2},",b[0],b[1]));
					}
					sb.Append(sb2.ToString()+"\n");
					sb2 = new StringBuilder();
					sb.Append("\n");
				}
				m_ResultsRichTextBox.Text = sb.ToString();
			}
			else
			{
				MessageBox.Show(this,error);
			}
		}

		private void GetPlayNamesAndNumbers()
		{
			StringBuilder sb = new StringBuilder(2000);
			for( int i =0; i< 64; i++ )
			{
				sb.Append(MainForm.mPlayTool.GetPlayName(i));
				sb.Append(",");
				sb.Append(MainForm.TheMainForm.ThePlayTool.GetPlayToCallText(i));
				sb.Append("\n");
			}
			m_ResultsRichTextBox.Text = sb.ToString();
		}

		private void GetDefensivePattern()
		{
			string error = null;
			if( m_InputTextBox.Text.Length < 1 )
				error = "You need to enter an argument";

			if( error == null ) // we're good let's go!
			{
				int[]args = GetArgList_i();
				StringBuilder sb = new StringBuilder();
				StringBuilder sb2 = new StringBuilder();
				for( int j = 0; j < args.Length; j++ )
				{
					sb.Append(string.Format("Defensive Play # {0,2:x}:\n",args[j]));
					for(int i = 0; i < 11; i++)
					{
						DPlayer p = (DPlayer)i;
						byte[] b = MainForm.mPlayTool.GetDPatternPointerRelative(p, args[j]);
						sb.Append(string.Format("{0,4}: 0x{1:x2}{2:x2}\n",
						 	UtilityClass.DefensivePlayers[i],b[0],b[1]));
						sb2.Append(string.Format("{0:x2}{1:x2},",b[0],b[1]));
					}
					sb.Append(sb2.ToString()+"\n");
					sb2 = new StringBuilder();
					sb.Append("\n");
				}
				m_ResultsRichTextBox.Text = sb.ToString();
			}
			else
			{
				MessageBox.Show(this,error);
			}
		}

		private void GetDefensivePlays()
		{
			StringBuilder sb = new StringBuilder();
			ArrayList list = new ArrayList(8*80);

			for(int i = 0; i < 64; i++)
			{
				sb.Append(string.Format( "Play #{0,2}: {1}\n", i, MainForm.TheMainForm.
					ThePlayTool.GetDefensiveReactionText(i)));

				byte[] bytes = MainForm.TheMainForm.
					ThePlayTool.GetDefensiveReactions(i);
					//ThePlayTool.GetOffensivePlayToCall(i);
				foreach( byte b in bytes)
				{
					list.Add(b);
				}
			}
			list.Sort();
			sb.Append("\n\nDefensive Plays Used:\n");
			byte lastByte = 0xff;
			int guys = 1;
			for(int i =0; i < list.Count; i++ )
			{
				byte b = (byte)list[i];
				if( b != lastByte )
				{
					sb.Append(string.Format("{0:x2},",b));
					if( guys++ % 16 == 0)
						sb.Append("\n");
				}
				lastByte = b;
				
			}
			m_ResultsRichTextBox.Text = sb.ToString();

		}

		private void BurdDogDefense()
		{
			if(m_ResultsRichTextBox.Text.Length < 500)
			{
				MessageBox.Show("you must first paste in jstout program data for def_code");
				return;
			}
			if( m_InputTextBox.Text.Length < 3 )
			{
				MessageBox.Show("Please enter a command in the input text box.");
				return;
			}
			string stuff = m_ResultsRichTextBox.Text.Replace("\r","");
			ArrayList list = new ArrayList(900);
			string pattern = "([A-Z]+) ([A-Z0-9]{4}):.*"+m_InputTextBox.Text;
			Regex ddPassRush = new Regex(pattern);
			string[] lines = stuff.Split(new char[] {'\n'});
			StringBuilder sb = new StringBuilder(900*5);
			Match m = null;
			foreach(string line in lines)
			{
				m = ddPassRush.Match(line);
				if( m.Success )
				{
					sb.Append(m.Groups[2].ToString());
					sb.Append(",");
				}
			}
			m_ResultsRichTextBox.Text = sb.ToString();
			m_ResultsRichTextBox.SelectionStart = 0;
		}

		private void DoPatternReader()
		{
			PatternReader.PatternReaderForm form = new PatternReader.PatternReaderForm();
			form.Show();
		}

		private void GetBytes()
		{
			string input = m_InputTextBox.Text;
			Regex setRegex = new Regex("([A-F0-9a-f]+)\\s*,\\s*([a-fA-F0-9]{2})\\s*$");

			Match m = setRegex.Match(input);
			if(m.Success )
			{
				string addressStr = m.Groups[1].Value;
				string lenStr = m.Groups[2].Value;
				m_ResultsRichTextBox.Text = "Success!\n";
				m_ResultsRichTextBox.AppendText(addressStr);
				m_ResultsRichTextBox.AppendText("\n");
				m_ResultsRichTextBox.AppendText(lenStr);
				m_ResultsRichTextBox.AppendText("\n");
				try
				{
					int address = Int32.Parse(addressStr,System.Globalization.NumberStyles.AllowHexSpecifier);
					int length  = Int32.Parse(lenStr,System.Globalization.NumberStyles.AllowHexSpecifier);

					address += 0x10;
					StringBuilder sb = new StringBuilder(length*3+10);
					byte b =0;
					int bytes = 0;
					for( int i = address; i < address+ length; i++)
					{
						b = MainForm.TheROM[i];
						sb.Append(string.Format("{0:x2}",b));
						bytes++;
						if( bytes %2 == 0)
							sb.Append(" ");
					}
					m_ResultsRichTextBox.AppendText(sb.ToString());
				}
				catch(Exception e)
				{
					MessageBox.Show(this,e.StackTrace,e.Message);
				}
			}
			else
			{
				m_ResultsRichTextBox.Text = "Failure!";
			}
		}

		private void SetBytes()
		{
			string input = m_InputTextBox.Text;
			Regex setRegex = new Regex("([A-F0-9a-f]){4}\\s*,\\s*(([a-fA-F0-9]{2})+)\\s*$");

			Match m = setRegex.Match(input);
			if(m.Success )
			{
				m_ResultsRichTextBox.Text = "Success!\n";
				m_ResultsRichTextBox.AppendText(m.Groups[1].Value);
				m_ResultsRichTextBox.AppendText("\n");
				m_ResultsRichTextBox.AppendText(m.Groups[2].Value);
//				m_ResultsRichTextBox.AppendText("\n");
//				m_ResultsRichTextBox.AppendText(m.Groups[3].Value);
			}
			else
			{
				m_ResultsRichTextBox.Text = "Failure!";
			}
		}

		public void PlayPointerTest()
		{
			string[] args = GetArgList_s();
			if( args != null )
			{
				StringBuilder sb = new StringBuilder(200);
				foreach(string arg in args)
				{
					sb.Append(MainForm.mPlayTool.GetPlaysPointerAppearsIn_str(arg, !true));
					sb.Append("\n");
				}
				m_ResultsRichTextBox.Text = sb.ToString();
			}
		}

		private void m_InputTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if( e.KeyCode == Keys.Enter )
			{
				m_RunTestButton_Click( sender, null);
			}
		}

		private void m_PasteMenuItem_Click(object sender, System.EventArgs e)
		{
			m_ResultsRichTextBox.Paste();
		}

		private void m_CopyMenuItem_Click(object sender, System.EventArgs e)
		{
			m_ResultsRichTextBox.Copy();
		}

		private void m_CutMenuItem_Click(object sender, System.EventArgs e)
		{
			m_ResultsRichTextBox.Cut();
		}

		private void m_ResultsRichTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.Control && e.KeyCode == Keys.F)
			{
				string str = StringInputDlg.GetString("Enter String", "Enter search string", m_SearchString);
				if( str != null )
				{
					m_SearchString = str;
					int index = m_ResultsRichTextBox.Text.IndexOf(m_SearchString, m_ResultsRichTextBox.SelectionStart);
					if( index > -1 )
					{
						m_ResultsRichTextBox.SelectionStart = index;
						m_ResultsRichTextBox.ScrollToCaret();
					}
				}
			}
		}
	}
}
