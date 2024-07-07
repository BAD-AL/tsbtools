using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;

namespace TSBTool
{
	public enum StateEnum
	{
		QB = 0,
		SKILL,
		OLINE,
		DEFENSE,
		KICKER
	}



	/// <summary>
	/// Summary description for AttributeForm.
	/// </summary>
	public class AttributeForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox m_RSBox;
		private System.Windows.Forms.ComboBox m_RPBox;
		private System.Windows.Forms.ComboBox m_MSBox;
		private System.Windows.Forms.ComboBox m_HPBox;
		private System.Windows.Forms.ComboBox m_PC_REC_QU_KABox;
		private System.Windows.Forms.ComboBox m_PS_BC_PI_KABox;
		private System.Windows.Forms.GroupBox mSimBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label m_Sim4Label;
		private System.Windows.Forms.Label m_Sim3Label;
		private System.Windows.Forms.Label m_Sim2Label;
		private System.Windows.Forms.Label m_Sim1Label;
		private System.Windows.Forms.NumericUpDown m_Sim1UpDown;
		private System.Windows.Forms.NumericUpDown m_Sim2UpDown;
		private System.Windows.Forms.NumericUpDown m_Sim3UpDown;
		private System.Windows.Forms.NumericUpDown m_Sim4UpDown;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label m_A3Label;
		private System.Windows.Forms.Label m_A2Label;
		private System.Windows.Forms.Label m_A1Label;
		private System.Windows.Forms.ComboBox m_PositionComboBox;
		private System.Windows.Forms.Button m_PrevPicture;
		private System.Windows.Forms.Button m_NextPicture;

		private StateEnum m_CurrentState = StateEnum.QB;
		private System.Windows.Forms.PictureBox m_FaceBox;
		private System.Windows.Forms.Label m_FaceLabel;
		private System.Windows.Forms.ComboBox m_TeamsComboBox;
		private System.Windows.Forms.Button m_NextPlayerButton;
		private int m_ImageNumber = 0x00;

		private string m_Data = "";
		private System.Windows.Forms.TextBox m_FirstNameTextBox;
		private System.Windows.Forms.TextBox m_LastNameTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button m_PrevPlayerButton;

		private bool m_AutoUpdatePlayers = false;

		public bool AutoUpdatePlayersUI
		{
			get{ return m_AutoUpdatePlayers ; }
			set 
			{
				m_AutoUpdatePlayers = value;
			}
		}

		/// <summary>
		/// The text data to work on and retrieve.
		/// </summary>
		public string Data
		{
			get { return m_Data; }

			set
			{ 
				m_Data = value; 
				if( m_Data != null && m_Data.Length > 0)
				{
					SetupTeams();
					SetCurrentPlayer();
				}
			}
		}

		private void SetupTeams()
		{
			Regex teamRegex = new Regex("TEAM\\s*=\\s*([a-z0-9]+)");
			MatchCollection mc = teamRegex.Matches(m_Data);

			m_TeamsComboBox.Items.Clear();
			m_TeamsComboBox.BeginUpdate();
			foreach( Match m in mc)
			{
				string team = m.Groups[1].Value ;
				m_TeamsComboBox.Items.Add( team );
			}
			m_TeamsComboBox.EndUpdate();
			if( m_TeamsComboBox.Items.Count > 0 )
			{
				m_TeamsComboBox.SelectedItem = m_TeamsComboBox.Items[0];
			}
		}

		/// <summary>
		/// Get and set the current position for the Form.
		/// </summary>
		public string CurrentPosition
		{
			get{ return this.m_PositionComboBox.SelectedItem.ToString();}

			set
			{
				int index = m_PositionComboBox.Items.IndexOf(value);
				if(index > -1 )
				{
//					m_DoneInit = false;
					m_PositionComboBox.SelectedIndex = index;
//					m_DoneInit = true;
				}
			}
		}

		/// <summary>
		/// Get and set the current team.
		/// </summary>
		public string CurrentTeam
		{
			get{ return this.m_TeamsComboBox.SelectedItem.ToString();}

			set
			{
				int index = m_TeamsComboBox.Items.IndexOf(value);
				if(index > -1 )
				{
//					m_DoneInit = false;
					m_TeamsComboBox.SelectedIndex = index;
//					m_DoneInit = true;
				}
			}
		}

		private ComboBox[]      m_Attributes = null;
		private NumericUpDown[] m_SimAttrs   = null;
		private System.Windows.Forms.NumericUpDown m_JerseyNumberUpDown;
		private System.Windows.Forms.Button m_AutoUpdateButton;

		bool m_DoneInit = false;
		private System.Windows.Forms.Button m_SaveButton;
		private System.Windows.Forms.Button m_CancelButton;
		private System.Windows.Forms.ComboBox m_ACCBox;
		private System.Windows.Forms.ComboBox m_APBBox;
		private System.Windows.Forms.Label m_A4Label;
		private SimStuff m_SimStuff = null;

		public AttributeForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_SimStuff = new SimStuff();
			m_Attributes = new ComboBox[8];
			m_Attributes[0] = m_RSBox;
			m_Attributes[1] = m_RPBox;
			m_Attributes[2] = m_MSBox;
			m_Attributes[3] = m_HPBox;
			m_Attributes[4] = m_PS_BC_PI_KABox;
			m_Attributes[5] = m_PC_REC_QU_KABox;
			m_Attributes[6] = m_ACCBox;
			m_Attributes[7] = m_APBBox;

			m_SimAttrs = new NumericUpDown[4];
			m_SimAttrs[0] = m_Sim1UpDown;
			m_SimAttrs[1] = m_Sim2UpDown;
			m_SimAttrs[2] = m_Sim3UpDown;
			m_SimAttrs[3] = m_Sim4UpDown;

			m_TeamsComboBox.SelectedIndex    = 0;
			m_PositionComboBox.SelectedIndex = 0;
			m_DoneInit = true;
			CurrentState = StateEnum.QB;
		}

		/// <summary>
		/// Gets a player 'line' from m_Data from 'team' playing 'position'.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		private string GetPlayerString( string team, string position )
		{
			string pattern = "TEAM\\s*=\\s*"+team;
			Regex findTeamRegex = new Regex(pattern);
			Match m = findTeamRegex.Match(m_Data);
			if( m != Match.Empty )
			{
				int teamIndex = m.Index;
				//int teamIndex = m_Data.IndexOf("TEAM = "+team);
				if( teamIndex == -1 )
					return null;
				//int playerIndex = m_Data.IndexOf("\n"+position+",", teamIndex);
				int playerIndex = -1;
				Regex endLineRegex = new Regex(string.Format("\n\\s*{0}\\s*,",position));
				Match eol = endLineRegex.Match(m_Data, teamIndex);
				if( eol != Match.Empty )
					playerIndex = eol.Index;
				playerIndex++;
			
				if( playerIndex == 0 )
					return null;
				int lineEnd = m_Data.IndexOf("\n",playerIndex);
				string playerLine = m_Data.Substring(playerIndex,lineEnd-playerIndex);
				return playerLine;
			}
			return null;
		}

		/// <summary>
		/// Updates the GUI with the current player.
		/// </summary>
		private void SetCurrentPlayer()
		{
			if( m_TeamsComboBox.SelectedItem != null )
			{
				string team       = m_TeamsComboBox.SelectedItem.ToString();
				string position   = m_PositionComboBox.SelectedItem.ToString();
				string playerData = GetPlayerString(team, position);
				if( playerData != null )
					SetPlayerData(playerData);
			}
		}

		/// <summary>
		/// Updates the GUI with the player 'line' passed.
		/// </summary>
		/// <param name="playerLine"></param>
		private void SetPlayerData(string playerLine)
		{
			string fName = InputParser.GetFirstName(playerLine);
            string lName = InputParser.GetLastName(playerLine);
            int face = InputParser.GetFace(playerLine);
            int jerseyNumber = InputParser.GetJerseyNumber(playerLine);
            int[] attrs = InputParser.GetInts(playerLine);
			int[] simData = InputParser.GetSimVals(playerLine);

			m_FirstNameTextBox.Text = fName;
			m_LastNameTextBox.Text  = lName;
			m_ImageNumber = face;
			if( jerseyNumber > -1 && jerseyNumber < 0x100)
				m_JerseyNumberUpDown.Value = Int32.Parse(string.Format("{0:x}", jerseyNumber));

			if( attrs != null )
			{
				int attrIndex = 0;
				for(int i = 0; i < attrs.Length && i < m_Attributes.Length; i++)
				{
					attrIndex = AttrIndex(attrs[i].ToString());
					if( attrIndex > -1 )
						m_Attributes[i].SelectedIndex = attrIndex;
				}
			}
			if( simData != null)
			{
				for( int i =0; i < simData.Length; i++)
				{
					m_SimAttrs[i].Value = Int32.Parse(simData[i].ToString());
				}
			}
			if( jerseyNumber > -1 && jerseyNumber < 0x100)
			{
				m_JerseyNumberUpDown.Value = Int32.Parse(string.Format("{0:x}", jerseyNumber));
				ShowCurrentFace();
			}
		}

		/// <summary>
		/// Returns the text representation of what the GUI is presenting.
		/// </summary>
		/// <returns></returns>
		private string GetPlayerString_UI()
		{
			string ret = GetPlayerString_UI(m_PositionComboBox.SelectedItem.ToString());
			return ret;
		}

		/// <summary>
		/// Returns the text representation of what the GUI is presenting.
		/// </summary>
		/// <returns></returns>
		public string GetPlayerString_UI(string position)
		{
			StringBuilder sb = new StringBuilder(60);
			sb.Append(position);
			sb.Append(", ");
			sb.Append(m_FirstNameTextBox.Text);
			sb.Append(" ");
			sb.Append(m_LastNameTextBox.Text);
			sb.Append(", ");
			sb.Append(string.Format("Face=0x{0:x}, #{1}, ", 
				m_ImageNumber, m_JerseyNumberUpDown.Value));
			// attrs
			for( int i = 0; i < m_Attributes.Length; i++)
			{
				if( m_Attributes[i].Enabled)
				{
					sb.Append(m_Attributes[i].SelectedItem.ToString());
					sb.Append(", ");
				}
			}
			//sim attrs
			if( m_SimAttrs[0].Enabled)
				sb.Append("[");
			for(int i = 0; i < m_SimAttrs.Length; i++)
			{
				if( m_SimAttrs[i].Enabled )
				{
					sb.Append(m_SimAttrs[i].Value);
					sb.Append(", ");
				}
			}
			if( m_SimAttrs[0].Enabled)
			{
				sb.Remove(sb.Length-2,2);
				sb.Append(" ]");
			}

			string ret = sb.ToString();
			char[] chars = {' ', ',' };
			ret = ret.Trim(chars);
			return ret;
		}

		/// <summary>
		/// Returns the index of 'val'.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		private int AttrIndex(string val)
		{
			int ret = -1;
			for( int i = 0 ; i < m_RSBox.Items.Count; i++)
			{
				if( m_Attributes[0].Items[i].ToString() == val )
				{
					ret = i;
					break;
				}
			}
			return ret;
		}


		/// <summary>
		/// Sets the current state. 
		/// Will enable, disable and modify the contents of the GUI.
		/// </summary>
		public StateEnum CurrentState
		{
			get{ return m_CurrentState; }

			set
			{
				m_DoneInit = false;
				switch(value)
				{
					case StateEnum.QB:
						m_A1Label.Text   = "PS";
						m_A2Label.Text   = "PC";
						m_A3Label.Text   = "ACC";
						m_A4Label.Text   = "APB";
						m_Sim1Label.Text = "Sim Run";
						m_Sim2Label.Text = "Sim Pass";
						m_Sim3Label.Text = "Sim Pocket";
						m_Sim4Label.Text = "";
						m_Sim1UpDown.Enabled = true;
						m_Sim2UpDown.Enabled = true;
						m_Sim3UpDown.Enabled = true;
						m_Sim4UpDown.Enabled = false;
						m_Sim1UpDown.Maximum = 15;
						m_Sim2UpDown.Maximum = 15;
						m_Sim3UpDown.Maximum = 15;
						m_Sim4UpDown.Maximum = 15;
						m_PS_BC_PI_KABox.Enabled  = true;
						m_PC_REC_QU_KABox.Enabled = true;
						m_ACCBox.Enabled          = true;
						m_APBBox.Enabled          = true;
						break;
					case StateEnum.SKILL:
						m_A1Label.Text = "BC";
						m_A2Label.Text = "REC";
						m_A3Label.Text = "";
						m_A4Label.Text = "";
						m_Sim1Label.Text = "Sim Rush";
						m_Sim2Label.Text = "Sim Catch";
						m_Sim3Label.Text = "Sim Yards per catch";
						m_Sim4Label.Text = "Sim Targets";
						m_Sim1UpDown.Enabled = true;
						m_Sim2UpDown.Enabled = true;
						m_Sim3UpDown.Enabled = true;
						m_Sim4UpDown.Enabled = true;
						m_Sim1UpDown.Maximum = 15;
						m_Sim2UpDown.Maximum = 15;
						m_Sim3UpDown.Maximum = 15;
						m_Sim4UpDown.Maximum = 15;
						m_PS_BC_PI_KABox.Enabled  = true;
						m_PC_REC_QU_KABox.Enabled = true;
						m_ACCBox.Enabled          = false;
						m_APBBox.Enabled          = false;
						break;
					case StateEnum.OLINE:
						m_A1Label.Text = "";
						m_A2Label.Text = "";
						m_A3Label.Text = "";
						m_A4Label.Text = "";
						m_Sim1Label.Text = "";
						m_Sim2Label.Text = "";
						m_Sim3Label.Text = "";
						m_Sim4Label.Text = "";
						m_Sim1UpDown.Enabled = false;
						m_Sim2UpDown.Enabled = false;
						m_Sim3UpDown.Enabled = false;
						m_Sim4UpDown.Enabled = false;
						m_PS_BC_PI_KABox.Enabled  = false;
						m_PC_REC_QU_KABox.Enabled = false;
						m_ACCBox.Enabled          = false;
						m_APBBox.Enabled          = false;
						break;
					case StateEnum.DEFENSE:
						m_A1Label.Text = "PI";
						m_A2Label.Text = "QU";
						m_A3Label.Text = "";
						m_A4Label.Text = "";
						m_Sim1Label.Text = "Sim Pass rush";
						m_Sim2Label.Text = "Sim Coverage";
						m_Sim3Label.Text = "";
						m_Sim4Label.Text = "";
						m_Sim1UpDown.Enabled = true;
						m_Sim2UpDown.Enabled = true;
						m_Sim3UpDown.Enabled = false;
						m_Sim4UpDown.Enabled = false;
						m_Sim1UpDown.Maximum = 255;
						m_Sim2UpDown.Maximum = 255;
						m_Sim3UpDown.Maximum = 255;
						m_Sim4UpDown.Maximum = 255;
						m_PS_BC_PI_KABox.Enabled  = true;
						m_PC_REC_QU_KABox.Enabled = true;
						m_ACCBox.Enabled          = false;
						m_APBBox.Enabled          = false;
						break;
					case StateEnum.KICKER:
						m_A1Label.Text = "KA";
						m_A2Label.Text = "AKB";
						m_A3Label.Text = "";
						m_A4Label.Text = "";
						m_Sim1Label.Text = "Sim KA";
						m_Sim2Label.Text = "";
						m_Sim3Label.Text = "";
						m_Sim4Label.Text = "";
						m_Sim1UpDown.Enabled = true;
						m_Sim2UpDown.Enabled = false;
						m_Sim3UpDown.Enabled = false;
						m_Sim4UpDown.Enabled = false;
						m_Sim1UpDown.Maximum = 15;
						m_Sim2UpDown.Maximum = 15;
						m_Sim3UpDown.Maximum = 15;
						m_Sim4UpDown.Maximum = 15;
						m_PS_BC_PI_KABox.Enabled  = true;
						m_PC_REC_QU_KABox.Enabled = true;
						m_ACCBox.Enabled          = false;
						m_APBBox.Enabled          = false;
						break;
				}
				m_CurrentState = value;
				m_DoneInit = true;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttributeForm));
            this.label1 = new System.Windows.Forms.Label();
            this.m_TeamsComboBox = new System.Windows.Forms.ComboBox();
            this.m_PositionComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_FaceBox = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.m_RSBox = new System.Windows.Forms.ComboBox();
            this.m_RPBox = new System.Windows.Forms.ComboBox();
            this.m_MSBox = new System.Windows.Forms.ComboBox();
            this.m_HPBox = new System.Windows.Forms.ComboBox();
            this.m_ACCBox = new System.Windows.Forms.ComboBox();
            this.m_PC_REC_QU_KABox = new System.Windows.Forms.ComboBox();
            this.m_PS_BC_PI_KABox = new System.Windows.Forms.ComboBox();
            this.m_A3Label = new System.Windows.Forms.Label();
            this.m_A2Label = new System.Windows.Forms.Label();
            this.m_A1Label = new System.Windows.Forms.Label();
            this.m_Sim4Label = new System.Windows.Forms.Label();
            this.m_Sim3Label = new System.Windows.Forms.Label();
            this.m_Sim2Label = new System.Windows.Forms.Label();
            this.m_Sim1Label = new System.Windows.Forms.Label();
            this.mSimBox = new System.Windows.Forms.GroupBox();
            this.m_Sim4UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Sim3UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Sim2UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Sim1UpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_APBBox = new System.Windows.Forms.ComboBox();
            this.m_A4Label = new System.Windows.Forms.Label();
            this.m_NextPlayerButton = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.m_FirstNameTextBox = new System.Windows.Forms.TextBox();
            this.m_LastNameTextBox = new System.Windows.Forms.TextBox();
            this.m_PrevPicture = new System.Windows.Forms.Button();
            this.m_NextPicture = new System.Windows.Forms.Button();
            this.m_FaceLabel = new System.Windows.Forms.Label();
            this.m_SaveButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.m_PrevPlayerButton = new System.Windows.Forms.Button();
            this.m_CancelButton = new System.Windows.Forms.Button();
            this.m_JerseyNumberUpDown = new System.Windows.Forms.NumericUpDown();
            this.m_AutoUpdateButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_FaceBox)).BeginInit();
            this.mSimBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim4UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim3UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim1UpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_JerseyNumberUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Team";
            // 
            // m_TeamsComboBox
            // 
            this.m_TeamsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_TeamsComboBox.Items.AddRange(new object[] {
            "bills",
            "colts",
            "dolphins",
            "patriots",
            "jets",
            "bengals",
            "browns",
            "oilers",
            "steelers",
            "broncos",
            "chiefs",
            "raiders",
            "chargers",
            "seahawks",
            "redskins",
            "giants",
            "eagles",
            "cardinals",
            "cowboys",
            "bears",
            "lions",
            "packers",
            "vikings",
            "buccaneers",
            "49ers",
            "rams",
            "saints",
            "falcons"});
            this.m_TeamsComboBox.Location = new System.Drawing.Point(56, 4);
            this.m_TeamsComboBox.Name = "m_TeamsComboBox";
            this.m_TeamsComboBox.Size = new System.Drawing.Size(104, 21);
            this.m_TeamsComboBox.TabIndex = 0;
            this.m_TeamsComboBox.SelectedIndexChanged += new System.EventHandler(this.m_TeamsComboBox_SelectedIndexChanged);
            // 
            // m_PositionComboBox
            // 
            this.m_PositionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_PositionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_PositionComboBox.Items.AddRange(new object[] {
            "QB1",
            "QB2",
            "RB1",
            "RB2",
            "RB3",
            "RB4",
            "WR1",
            "WR2",
            "WR3",
            "WR4",
            "TE1",
            "TE2",
            "C",
            "LG",
            "RG",
            "LT",
            "RT",
            "RE",
            "NT",
            "LE",
            "ROLB",
            "RILB",
            "LILB",
            "LOLB",
            "RCB",
            "LCB",
            "FS",
            "SS",
            "K",
            "P"});
            this.m_PositionComboBox.Location = new System.Drawing.Point(280, 4);
            this.m_PositionComboBox.Name = "m_PositionComboBox";
            this.m_PositionComboBox.Size = new System.Drawing.Size(104, 21);
            this.m_PositionComboBox.TabIndex = 1;
            this.m_PositionComboBox.SelectedIndexChanged += new System.EventHandler(this.m_PositionComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(232, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Position";
            // 
            // m_FaceBox
            // 
            this.m_FaceBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_FaceBox.Image = ((System.Drawing.Image)(resources.GetObject("m_FaceBox.Image")));
            this.m_FaceBox.Location = new System.Drawing.Point(8, 128);
            this.m_FaceBox.Name = "m_FaceBox";
            this.m_FaceBox.Size = new System.Drawing.Size(64, 64);
            this.m_FaceBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_FaceBox.TabIndex = 4;
            this.m_FaceBox.TabStop = false;
            this.m_FaceBox.Click += new System.EventHandler(this.m_FaceBox_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 10);
            this.label4.TabIndex = 7;
            this.label4.Text = "RS";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(56, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 10);
            this.label5.TabIndex = 9;
            this.label5.Text = "RP";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(152, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 10);
            this.label6.TabIndex = 13;
            this.label6.Text = "HP";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(104, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 10);
            this.label7.TabIndex = 11;
            this.label7.Text = "MS";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_RSBox
            // 
            this.m_RSBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_RSBox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_RSBox.Location = new System.Drawing.Point(8, 32);
            this.m_RSBox.Name = "m_RSBox";
            this.m_RSBox.Size = new System.Drawing.Size(43, 21);
            this.m_RSBox.TabIndex = 4;
            this.m_RSBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_RPBox
            // 
            this.m_RPBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_RPBox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_RPBox.Location = new System.Drawing.Point(56, 32);
            this.m_RPBox.Name = "m_RPBox";
            this.m_RPBox.Size = new System.Drawing.Size(43, 21);
            this.m_RPBox.TabIndex = 5;
            this.m_RPBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_MSBox
            // 
            this.m_MSBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_MSBox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_MSBox.Location = new System.Drawing.Point(104, 32);
            this.m_MSBox.Name = "m_MSBox";
            this.m_MSBox.Size = new System.Drawing.Size(43, 21);
            this.m_MSBox.TabIndex = 6;
            this.m_MSBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_HPBox
            // 
            this.m_HPBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_HPBox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_HPBox.Location = new System.Drawing.Point(152, 32);
            this.m_HPBox.Name = "m_HPBox";
            this.m_HPBox.Size = new System.Drawing.Size(43, 21);
            this.m_HPBox.TabIndex = 7;
            this.m_HPBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_ACCBox
            // 
            this.m_ACCBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_ACCBox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_ACCBox.Location = new System.Drawing.Point(296, 32);
            this.m_ACCBox.Name = "m_ACCBox";
            this.m_ACCBox.Size = new System.Drawing.Size(43, 21);
            this.m_ACCBox.TabIndex = 10;
            this.m_ACCBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_PC_REC_QU_KABox
            // 
            this.m_PC_REC_QU_KABox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_PC_REC_QU_KABox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_PC_REC_QU_KABox.Location = new System.Drawing.Point(248, 32);
            this.m_PC_REC_QU_KABox.Name = "m_PC_REC_QU_KABox";
            this.m_PC_REC_QU_KABox.Size = new System.Drawing.Size(43, 21);
            this.m_PC_REC_QU_KABox.TabIndex = 9;
            this.m_PC_REC_QU_KABox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_PS_BC_PI_KABox
            // 
            this.m_PS_BC_PI_KABox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_PS_BC_PI_KABox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_PS_BC_PI_KABox.Location = new System.Drawing.Point(200, 32);
            this.m_PS_BC_PI_KABox.Name = "m_PS_BC_PI_KABox";
            this.m_PS_BC_PI_KABox.Size = new System.Drawing.Size(43, 21);
            this.m_PS_BC_PI_KABox.TabIndex = 8;
            this.m_PS_BC_PI_KABox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_A3Label
            // 
            this.m_A3Label.Location = new System.Drawing.Point(296, 16);
            this.m_A3Label.Name = "m_A3Label";
            this.m_A3Label.Size = new System.Drawing.Size(40, 10);
            this.m_A3Label.TabIndex = 20;
            this.m_A3Label.Text = "ACC";
            this.m_A3Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_A2Label
            // 
            this.m_A2Label.Location = new System.Drawing.Point(248, 16);
            this.m_A2Label.Name = "m_A2Label";
            this.m_A2Label.Size = new System.Drawing.Size(40, 10);
            this.m_A2Label.TabIndex = 19;
            this.m_A2Label.Text = "PC";
            this.m_A2Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_A1Label
            // 
            this.m_A1Label.Location = new System.Drawing.Point(200, 16);
            this.m_A1Label.Name = "m_A1Label";
            this.m_A1Label.Size = new System.Drawing.Size(40, 10);
            this.m_A1Label.TabIndex = 18;
            this.m_A1Label.Text = "PS";
            this.m_A1Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_Sim4Label
            // 
            this.m_Sim4Label.Location = new System.Drawing.Point(248, 16);
            this.m_Sim4Label.Name = "m_Sim4Label";
            this.m_Sim4Label.Size = new System.Drawing.Size(56, 32);
            this.m_Sim4Label.TabIndex = 27;
            this.m_Sim4Label.Text = "blank";
            this.m_Sim4Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_Sim3Label
            // 
            this.m_Sim3Label.Location = new System.Drawing.Point(168, 16);
            this.m_Sim3Label.Name = "m_Sim3Label";
            this.m_Sim3Label.Size = new System.Drawing.Size(56, 32);
            this.m_Sim3Label.TabIndex = 26;
            this.m_Sim3Label.Text = "Sim pocket";
            this.m_Sim3Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_Sim2Label
            // 
            this.m_Sim2Label.Location = new System.Drawing.Point(88, 16);
            this.m_Sim2Label.Name = "m_Sim2Label";
            this.m_Sim2Label.Size = new System.Drawing.Size(56, 32);
            this.m_Sim2Label.TabIndex = 25;
            this.m_Sim2Label.Text = "Sim Pass";
            this.m_Sim2Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_Sim1Label
            // 
            this.m_Sim1Label.Location = new System.Drawing.Point(8, 16);
            this.m_Sim1Label.Name = "m_Sim1Label";
            this.m_Sim1Label.Size = new System.Drawing.Size(56, 32);
            this.m_Sim1Label.TabIndex = 24;
            this.m_Sim1Label.Text = "Sim Run";
            this.m_Sim1Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mSimBox
            // 
            this.mSimBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mSimBox.Controls.Add(this.m_Sim4UpDown);
            this.mSimBox.Controls.Add(this.m_Sim3UpDown);
            this.mSimBox.Controls.Add(this.m_Sim2UpDown);
            this.mSimBox.Controls.Add(this.m_Sim1UpDown);
            this.mSimBox.Controls.Add(this.m_Sim4Label);
            this.mSimBox.Controls.Add(this.m_Sim3Label);
            this.mSimBox.Controls.Add(this.m_Sim2Label);
            this.mSimBox.Controls.Add(this.m_Sim1Label);
            this.mSimBox.Location = new System.Drawing.Point(88, 128);
            this.mSimBox.Name = "mSimBox";
            this.mSimBox.Size = new System.Drawing.Size(312, 80);
            this.mSimBox.TabIndex = 6;
            this.mSimBox.TabStop = false;
            this.mSimBox.Text = "Sim attributes";
            // 
            // m_Sim4UpDown
            // 
            this.m_Sim4UpDown.Location = new System.Drawing.Point(254, 48);
            this.m_Sim4UpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.m_Sim4UpDown.Name = "m_Sim4UpDown";
            this.m_Sim4UpDown.Size = new System.Drawing.Size(40, 20);
            this.m_Sim4UpDown.TabIndex = 14;
            this.m_Sim4UpDown.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_Sim3UpDown
            // 
            this.m_Sim3UpDown.Location = new System.Drawing.Point(172, 48);
            this.m_Sim3UpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.m_Sim3UpDown.Name = "m_Sim3UpDown";
            this.m_Sim3UpDown.Size = new System.Drawing.Size(40, 20);
            this.m_Sim3UpDown.TabIndex = 13;
            this.m_Sim3UpDown.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_Sim2UpDown
            // 
            this.m_Sim2UpDown.Location = new System.Drawing.Point(90, 48);
            this.m_Sim2UpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.m_Sim2UpDown.Name = "m_Sim2UpDown";
            this.m_Sim2UpDown.Size = new System.Drawing.Size(40, 20);
            this.m_Sim2UpDown.TabIndex = 12;
            this.m_Sim2UpDown.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_Sim1UpDown
            // 
            this.m_Sim1UpDown.Location = new System.Drawing.Point(8, 48);
            this.m_Sim1UpDown.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.m_Sim1UpDown.Name = "m_Sim1UpDown";
            this.m_Sim1UpDown.Size = new System.Drawing.Size(40, 20);
            this.m_Sim1UpDown.TabIndex = 11;
            this.m_Sim1UpDown.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.m_APBBox);
            this.groupBox1.Controls.Add(this.m_A4Label);
            this.groupBox1.Controls.Add(this.m_A2Label);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.m_RSBox);
            this.groupBox1.Controls.Add(this.m_MSBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.m_RPBox);
            this.groupBox1.Controls.Add(this.m_HPBox);
            this.groupBox1.Controls.Add(this.m_A1Label);
            this.groupBox1.Controls.Add(this.m_PS_BC_PI_KABox);
            this.groupBox1.Controls.Add(this.m_PC_REC_QU_KABox);
            this.groupBox1.Controls.Add(this.m_ACCBox);
            this.groupBox1.Controls.Add(this.m_A3Label);
            this.groupBox1.Location = new System.Drawing.Point(8, 72);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 56);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attributes";
            // 
            // m_APBBox
            // 
            this.m_APBBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_APBBox.Items.AddRange(new object[] {
            "6",
            "13",
            "19",
            "25",
            "31",
            "38",
            "44",
            "50",
            "56",
            "63",
            "69",
            "75",
            "81",
            "88",
            "94",
            "100"});
            this.m_APBBox.Location = new System.Drawing.Point(344, 32);
            this.m_APBBox.Name = "m_APBBox";
            this.m_APBBox.Size = new System.Drawing.Size(43, 21);
            this.m_APBBox.TabIndex = 21;
            this.m_APBBox.SelectedIndexChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_A4Label
            // 
            this.m_A4Label.Location = new System.Drawing.Point(344, 16);
            this.m_A4Label.Name = "m_A4Label";
            this.m_A4Label.Size = new System.Drawing.Size(40, 10);
            this.m_A4Label.TabIndex = 22;
            this.m_A4Label.Text = "APB";
            this.m_A4Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_NextPlayerButton
            // 
            this.m_NextPlayerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_NextPlayerButton.Location = new System.Drawing.Point(117, 248);
            this.m_NextPlayerButton.Name = "m_NextPlayerButton";
            this.m_NextPlayerButton.Size = new System.Drawing.Size(88, 24);
            this.m_NextPlayerButton.TabIndex = 25;
            this.m_NextPlayerButton.Text = "&Next Player";
            this.m_NextPlayerButton.Click += new System.EventHandler(this.m_NextPlayerButton_Click);
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(64, 32);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(100, 16);
            this.label15.TabIndex = 35;
            this.label15.Text = "First Name";
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(232, 32);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(100, 16);
            this.label16.TabIndex = 36;
            this.label16.Text = "Last Name";
            // 
            // m_FirstNameTextBox
            // 
            this.m_FirstNameTextBox.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_FirstNameTextBox.Location = new System.Drawing.Point(64, 48);
            this.m_FirstNameTextBox.MaxLength = 16;
            this.m_FirstNameTextBox.Name = "m_FirstNameTextBox";
            this.m_FirstNameTextBox.Size = new System.Drawing.Size(152, 21);
            this.m_FirstNameTextBox.TabIndex = 3;
            this.m_FirstNameTextBox.TextChanged += new System.EventHandler(this.ValueChanged);
            this.m_FirstNameTextBox.Leave += new System.EventHandler(this.m_FirstNameTextBox_Leave);
            // 
            // m_LastNameTextBox
            // 
            this.m_LastNameTextBox.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_LastNameTextBox.Location = new System.Drawing.Point(232, 48);
            this.m_LastNameTextBox.MaxLength = 16;
            this.m_LastNameTextBox.Name = "m_LastNameTextBox";
            this.m_LastNameTextBox.Size = new System.Drawing.Size(152, 21);
            this.m_LastNameTextBox.TabIndex = 4;
            this.m_LastNameTextBox.TextChanged += new System.EventHandler(this.ValueChanged);
            this.m_LastNameTextBox.Leave += new System.EventHandler(this.m_LastNameTextBox_Leave);
            // 
            // m_PrevPicture
            // 
            this.m_PrevPicture.Location = new System.Drawing.Point(8, 208);
            this.m_PrevPicture.Name = "m_PrevPicture";
            this.m_PrevPicture.Size = new System.Drawing.Size(32, 18);
            this.m_PrevPicture.TabIndex = 15;
            this.m_PrevPicture.Text = "\\/";
            this.m_PrevPicture.Click += new System.EventHandler(this.m_PrevPicture_Click);
            // 
            // m_NextPicture
            // 
            this.m_NextPicture.Location = new System.Drawing.Point(40, 208);
            this.m_NextPicture.Name = "m_NextPicture";
            this.m_NextPicture.Size = new System.Drawing.Size(32, 18);
            this.m_NextPicture.TabIndex = 16;
            this.m_NextPicture.Text = "/\\";
            this.m_NextPicture.Click += new System.EventHandler(this.m_NextPicture_Click);
            // 
            // m_FaceLabel
            // 
            this.m_FaceLabel.Location = new System.Drawing.Point(8, 192);
            this.m_FaceLabel.Name = "m_FaceLabel";
            this.m_FaceLabel.Size = new System.Drawing.Size(64, 16);
            this.m_FaceLabel.TabIndex = 41;
            this.m_FaceLabel.Text = "00";
            this.m_FaceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_SaveButton
            // 
            this.m_SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_SaveButton.Location = new System.Drawing.Point(224, 248);
            this.m_SaveButton.Name = "m_SaveButton";
            this.m_SaveButton.Size = new System.Drawing.Size(80, 24);
            this.m_SaveButton.TabIndex = 30;
            this.m_SaveButton.Text = "&OK";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 16);
            this.label3.TabIndex = 43;
            this.label3.Text = "#";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_PrevPlayerButton
            // 
            this.m_PrevPlayerButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_PrevPlayerButton.Location = new System.Drawing.Point(8, 248);
            this.m_PrevPlayerButton.Name = "m_PrevPlayerButton";
            this.m_PrevPlayerButton.Size = new System.Drawing.Size(96, 24);
            this.m_PrevPlayerButton.TabIndex = 20;
            this.m_PrevPlayerButton.Text = "&Prev Player";
            this.m_PrevPlayerButton.Click += new System.EventHandler(this.m_PrevPlayerButton_Click);
            // 
            // m_CancelButton
            // 
            this.m_CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_CancelButton.Location = new System.Drawing.Point(312, 248);
            this.m_CancelButton.Name = "m_CancelButton";
            this.m_CancelButton.Size = new System.Drawing.Size(88, 24);
            this.m_CancelButton.TabIndex = 35;
            this.m_CancelButton.Text = "&Cancel";
            // 
            // m_JerseyNumberUpDown
            // 
            this.m_JerseyNumberUpDown.Location = new System.Drawing.Point(8, 48);
            this.m_JerseyNumberUpDown.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.m_JerseyNumberUpDown.Name = "m_JerseyNumberUpDown";
            this.m_JerseyNumberUpDown.Size = new System.Drawing.Size(40, 20);
            this.m_JerseyNumberUpDown.TabIndex = 2;
            this.m_JerseyNumberUpDown.ValueChanged += new System.EventHandler(this.ValueChanged);
            // 
            // m_AutoUpdateButton
            // 
            this.m_AutoUpdateButton.BackColor = System.Drawing.Color.LightCoral;
            this.m_AutoUpdateButton.Location = new System.Drawing.Point(88, 216);
            this.m_AutoUpdateButton.Name = "m_AutoUpdateButton";
            this.m_AutoUpdateButton.Size = new System.Drawing.Size(312, 24);
            this.m_AutoUpdateButton.TabIndex = 18;
            this.m_AutoUpdateButton.Text = "&Auto Update All Player Sim Attributes";
            this.m_AutoUpdateButton.UseVisualStyleBackColor = false;
            this.m_AutoUpdateButton.Click += new System.EventHandler(this.m_AutoUpdateButton_Click);
            // 
            // AttributeForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.m_CancelButton;
            this.ClientSize = new System.Drawing.Size(400, 274);
            this.Controls.Add(this.m_AutoUpdateButton);
            this.Controls.Add(this.m_JerseyNumberUpDown);
            this.Controls.Add(this.m_CancelButton);
            this.Controls.Add(this.m_PrevPlayerButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_SaveButton);
            this.Controls.Add(this.m_FaceLabel);
            this.Controls.Add(this.m_NextPicture);
            this.Controls.Add(this.m_PrevPicture);
            this.Controls.Add(this.m_LastNameTextBox);
            this.Controls.Add(this.m_FirstNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.m_NextPlayerButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mSimBox);
            this.Controls.Add(this.m_FaceBox);
            this.Controls.Add(this.m_PositionComboBox);
            this.Controls.Add(this.m_TeamsComboBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(800, 600);
            this.MinimumSize = new System.Drawing.Size(416, 312);
            this.Name = "AttributeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify Players";
            ((System.ComponentModel.ISupportInitialize)(this.m_FaceBox)).EndInit();
            this.mSimBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim4UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim3UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Sim1UpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_JerseyNumberUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// Sets the current state based on the position selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_PositionComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if( m_DoneInit )
			{
				TSBPlayer pos = (TSBPlayer) m_PositionComboBox.SelectedIndex;
				switch(pos)
				{
					case TSBPlayer.QB1: case TSBPlayer.QB2:
						CurrentState = StateEnum.QB;
						break;
					case TSBPlayer.RB1: case TSBPlayer.RB2: 
					case TSBPlayer.RB3: case TSBPlayer.RB4:
					case TSBPlayer.WR1: case TSBPlayer.WR2: 
					case TSBPlayer.WR3: case TSBPlayer.WR4:
					case TSBPlayer.TE1: case TSBPlayer.TE2:
						CurrentState = StateEnum.SKILL;
						break;
					case TSBPlayer.RT: case TSBPlayer.RG: 
					case TSBPlayer.C: case TSBPlayer.LG: 
					case TSBPlayer.LT:
						CurrentState = StateEnum.OLINE;
						break;
					case TSBPlayer.RE:   case TSBPlayer.LE:   
					case TSBPlayer.NT:
					case TSBPlayer.ROLB: case TSBPlayer.RILB: 
					case TSBPlayer.LILB: case TSBPlayer.LOLB:
					case TSBPlayer.RCB:  case TSBPlayer.LCB:  
					case TSBPlayer.SS:   case TSBPlayer.FS:
						CurrentState = StateEnum.DEFENSE;
						break;
					case TSBPlayer.P: case TSBPlayer.K:
						CurrentState = StateEnum.KICKER;
						break;
				}
				SetCurrentPlayer();
			}
		}

		/// <summary>
		/// Gets the proper face for a given player, and updates the displaying image.
		/// </summary>
		private void ShowCurrentFace()
		{
			if( m_DoneInit )
			{
				string file = "TSBTool.FACES."+string.Format("{0:x2}.BMP",m_ImageNumber).ToUpper();

                Image face = MainClass.GetImage(file);
				if( face != null )
				{
					m_FaceBox.Image = face;
					m_FaceLabel.Text = string.Format("{0:x2}",m_ImageNumber).ToUpper();
				}
				else
					MessageBox.Show("Problem with "+file);
				AutoUpdateRoster();
			}
		}

		/// <summary>
		/// Will update the current player when the index changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_TeamsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if( m_DoneInit )
			{
				// do a replace player on Auto update HERE!
				SetCurrentPlayer();
			}
		}

		// 00 - D4
		/// <summary>
		/// Shows the previous face.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_PrevPicture_Click(object sender, System.EventArgs e)
		{
			if( m_ImageNumber == 0x00 )
				m_ImageNumber = 0xD4;
			else if( m_ImageNumber == 0x80 )
				m_ImageNumber = 0x52;
			else
				m_ImageNumber--;
			ShowCurrentFace();
		}

		/// <summary>
		/// Shows the next face.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_NextPicture_Click(object sender, System.EventArgs e)
		{
			if( m_ImageNumber == 0xD4 )
				m_ImageNumber = 0;
			else if( m_ImageNumber == 0x52 )
				m_ImageNumber = 0x80;
			else
				m_ImageNumber++;
			ShowCurrentFace();
		}

		/// <summary>
		/// Displays the 'Next' player in the data.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_NextPlayerButton_Click(object sender, System.EventArgs e)
		{
			//ReplacePlayer();
			if( m_PositionComboBox.SelectedIndex == m_PositionComboBox.Items.Count - 1 )
			{
				m_PositionComboBox.SelectedIndex = 0;
				if( m_TeamsComboBox.SelectedIndex == m_TeamsComboBox.Items.Count - 1 )
					m_TeamsComboBox.SelectedIndex = 0;
				else
					m_TeamsComboBox.SelectedIndex++;
			}
			else
				m_PositionComboBox.SelectedIndex++;
		}

		private void AutoUpdateRoster()
		{
			if( m_DoneInit && m_AutoUpdatePlayers )
			{
				ReplacePlayer();
			}
		}
		/// <summary>
		/// Replaces the current player with the values specified.
		/// </summary>
		private void ReplacePlayer()
		{
			string oldPlayer = GetPlayerString(m_TeamsComboBox.SelectedItem.ToString(),
				m_PositionComboBox.SelectedItem.ToString());
			if( oldPlayer == null )
				return;

			string newPlayer = GetPlayerString_UI();
			string team = m_TeamsComboBox.Items[m_TeamsComboBox.SelectedIndex].ToString();
			ReplacePlayer(team, oldPlayer, newPlayer);
		}

		private void ReplacePlayer(string team, string oldPlayer, string newPlayer)
		{
			int nextTeamIndex = -1;
			int currentTeamIndex= -1;
			string nextTeam    = null;

			Regex findTeamRegex = new Regex("TEAM\\s*=\\s*"+team);
			
			Match m = findTeamRegex.Match(m_Data);
			if( !m.Success )
				return;

			currentTeamIndex = m.Groups[1].Index;

			int test = m_TeamsComboBox.Items.IndexOf(team);

			if( test != m_TeamsComboBox.Items.Count - 1 )
			{
				nextTeam      = string.Format("TEAM\\s*=\\s*{0}",m_TeamsComboBox.Items[test+1]);
				Regex nextTeamRegex = new Regex(nextTeam);
				Match nt = nextTeamRegex.Match(m_Data);
				if( nt.Success )
					nextTeamIndex = nt.Index;
				//nextTeamIndex = m_Data.IndexOf(nextTeam);
			}
			if( nextTeamIndex < 0)
				nextTeamIndex = m_Data.Length;

			
			int playerIndex = m_Data.IndexOf(oldPlayer,currentTeamIndex);
			if( playerIndex > -1 )
			{
				int endLine     = m_Data.IndexOf('\n',playerIndex);
				string start    = m_Data.Substring(0,playerIndex);
				string last     = m_Data.Substring(endLine);
				
				StringBuilder tmp = new StringBuilder(m_Data.Length + 200);
				tmp.Append(start);
				tmp.Append(newPlayer);
				tmp.Append(last);

				m_Data = tmp.ToString();
				//m_Data = start + newPlayer + last;
			}
			else
			{
				string error = string.Format(
@"An error occured looking up player
     '{0}'
Please verify that this player's attributes are correct.", oldPlayer);
				MessageBox.Show(error);
			}
		}

		/// <summary>
		/// Shows the face selection form, replaces the current face.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_FaceBox_Click(object sender, System.EventArgs e)
		{
			FaceForm form = new FaceForm();
			try
			{
				form.ShowDialog(this);
			}
			catch(Exception ex )
			{
				MessageBox.Show(ex.Message+"\n"+ex.StackTrace);
			}
			if(form.ImageIndex > -1 )
			{
				m_ImageNumber = form.ImageIndex;
				ShowCurrentFace();
			}
		}

		/// <summary>
		/// displays the previous player.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_PrevPlayerButton_Click(object sender, System.EventArgs e)
		{
			if( m_PositionComboBox.SelectedIndex == 0 )
			{
				m_PositionComboBox.SelectedIndex = m_PositionComboBox.Items.Count - 1;
				if( m_TeamsComboBox.SelectedIndex == 0 )
					m_TeamsComboBox.SelectedIndex = m_TeamsComboBox.Items.Count - 1;
				else
					m_TeamsComboBox.SelectedIndex--;
			}
			else
				m_PositionComboBox.SelectedIndex--;
		}

		/// <summary>
		/// calls ReplacePlayer().
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void m_SavePlayerButton_Click(object sender, System.EventArgs e)
		{
			ReplacePlayer();
		}

		private void m_AutoUpdateButton_Click(object sender, System.EventArgs e)
		{
			//MessageBox.Show("This Feature not yet implemented.");
			if( MessageBox.Show(this, 
                "Are you sure you want to Auto Update ALL Player Sim data for ALL teams?",
				"Are you Sure?", MessageBoxButtons.YesNo) == DialogResult.Yes )
			{
				this.Enabled = false;
                if (MessageBox.Show("Use Tecmonster's formulas?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    m_Data = TecmonsterTSB1SimAutoUpdater.AutoUpdatePlayerSimData(m_Data);
                }
                else
                {
                    AutoUpdatePlayerSim();
                }
				MessageBox.Show(this, "Done.");
				this.Enabled = true;
				SetCurrentPlayer();
			}
		}

		/// <summary>
		/// Update all players sim attributes.
		/// </summary>
		private void AutoUpdatePlayerSim()
		{
			foreach( string team in m_TeamsComboBox.Items )
			{
				AutoUpdatePlayers( team );
			}
		}

		/// <summary>
		/// Auto update a team's players sim attributs.
		/// </summary>
		/// <param name="team"></param>
		private void AutoUpdatePlayers( string team )
		{
			// look for 'TEAM = ' here
			string pattern = "TEAM\\s*=\\s*"+team;
			Regex findTeamRegex = new Regex(pattern);
			Match m = findTeamRegex.Match(m_Data);
			if( m != Match.Empty )
			{
				foreach( string position in m_PositionComboBox.Items )
				{
					if( position == "C")
						break;
					AutoUpdatePlayerSimData( team, position);
				}
				AutoUpdatePlayerSimData(team, "P");
				AutoUpdatePlayerSimData(team, "K");
				UpdateTeamSimPassDefense(team);
				UpdateTeamSimPassRush(team);
			}
		}

		/// <summary>
		/// Auto update a player's sim data.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		private void AutoUpdatePlayerSimData(string team, string position)
		{
			string oldValue = GetPlayerString(team, position);
			string newValue = null;

			if( oldValue == null || oldValue == string.Empty )
			{
				return;// he's not there, don't update him.
			}
            string fName = InputParser.GetFirstName(oldValue);
            string lName = InputParser.GetLastName(oldValue);
            int face = InputParser.GetFace(oldValue);
            int jerseyNumber = InputParser.GetJerseyNumber(oldValue);
			
			int[] attrs = new int[4];
			try
			{
				attrs = InputParser.GetInts(oldValue);
			}
			catch(Exception e)
			{
				MessageBox.Show("Oh oh!"+e.Message);
			}
			//int[] simData = InputParser.GetSimVals(playerLine);
			int simPass, simRush, simPocket, simCatch, simPR, simKR, simKA;

			switch( position )
			{
				case "QB1": case "QB2":
					if( attrs != null && attrs.Length > 7)
					{
						simPass   = m_SimStuff.SimPass(/*PC*/attrs[5], /*APB*/attrs[7], /*PS*/attrs[4]);
						simRush   = m_SimStuff.QbSimRun(/*MS*/attrs[2]);
						simPocket = m_SimStuff.SimPocket(/*MS*/attrs[2]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12} ,[{13}, {14}, {15} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5],attrs[6], attrs[7], simRush, simPass, simPocket); 
					}
					break;
				case "RB1": case "RB2": 
				case "RB3": case "RB4":
					if( attrs != null && attrs.Length > 5)
					{
						simRush  = m_SimStuff.RbSimRush(/*MS*/attrs[2],/*HP*/attrs[3],/*BC*/attrs[4],/*RS*/attrs[0]);
						simCatch = m_SimStuff.RbSimCatch(/*REC*/attrs[5]);
						simKR    = m_SimStuff.SimKickRet(/*MS*/attrs[2]);
						simPR    = m_SimStuff.SimPuntRet(/*MS*/attrs[2]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11}, {12} ,{13}, {14} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5],simRush, simCatch, simPR, simKR);
					}
					break;
				case "WR1": case "WR2": 
				case "WR3": case "WR4":
				case "TE1": case "TE2":
					if( attrs != null && attrs.Length > 5)
					{
						simRush  = m_SimStuff.RbSimRush(/*MS*/attrs[2],/*HP*/attrs[3],/*BC*/attrs[4],/*RS*/attrs[0]);
						simCatch = m_SimStuff.RbSimCatch(/*REC*/attrs[5]);
						simKR    = m_SimStuff.SimKickRet(/*MS*/attrs[2]);
						simPR    = m_SimStuff.SimPuntRet(/*MS*/attrs[2]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11}, {12} ,{13}, {14} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5],simRush, simCatch, simPR, simKR);
					}
					break;
				case "P": case "K":
					if( attrs != null && attrs.Length > 5)
					{
						simKA = m_SimStuff.PKSimKick(/*KA*/attrs[4], /*AKB*/attrs[5]);
						newValue  = string.Format( 
							"{0}, {1} {2}, Face=0x{3:x}, #{4:x}, {5}, {6}, {7}, {8}, {9}, {10} ,[{11} ]",
							position, fName, lName, face, jerseyNumber, attrs[0], attrs[1],attrs[2],attrs[3],
							attrs[4],attrs[5], simKA); 
					}
					break;
			}
			if( newValue != null )
			{
				ReplacePlayer(team, oldValue, newValue);
			}
		}

		private void UpdateTeamSimPassDefense(string team)
		{
			string re   = GetPlayerString(team, "RE");
			string le   = GetPlayerString(team, "LE");
			string nt   = GetPlayerString(team, "NT");
			string lolb = GetPlayerString(team, "LOLB");
			string lilb = GetPlayerString(team, "LILB");
			string rilb = GetPlayerString(team, "RILB");
			string rolb = GetPlayerString(team, "ROLB");
			string rcb  = GetPlayerString(team, "RCB");
			string lcb  = GetPlayerString(team, "LCB");
			string fs   = GetPlayerString(team, "FS");
			string ss   = GetPlayerString(team, "SS");

			if( re == null   || le == null   || nt == null   || 
				lolb == null || lilb == null || rilb == null || rolb == null ||
				rcb == null  || lcb == null || fs == null || ss == null )
			{
				// we need the entire defense in order to update this.
				return;
			}
			int[] reAttrs   = InputParser.GetInts(re);
			int[] leAttrs   = InputParser.GetInts(le);
			int[] ntAttrs   = InputParser.GetInts(nt);
			int[] lolbAttrs = InputParser.GetInts(lolb);
			int[] lilbAttrs = InputParser.GetInts(lilb);
			int[] rilbAttrs = InputParser.GetInts(rilb);
			int[] rolbAttrs = InputParser.GetInts(rolb);
			int[] rcbAttrs  = InputParser.GetInts(rcb);
			int[] lcbAttrs  = InputParser.GetInts(lcb);
			int[] fsAttrs   = InputParser.GetInts(fs);
			int[] ssAttrs   = InputParser.GetInts(ss);
			
			int[] passDef = m_SimStuff.GetSimPassDefense(
				rolbAttrs[4], rilbAttrs[4], lilbAttrs[4], lolbAttrs[4],
				rcbAttrs[4],  lcbAttrs[4],  fsAttrs[4],   ssAttrs[4]);

			ReplacePlayer(team, re,   ReplaceSimAttr(re,   2,0));
			ReplacePlayer(team, nt ,  ReplaceSimAttr(nt,   2,0));
			ReplacePlayer(team, le ,  ReplaceSimAttr(le,   2,0));
			ReplacePlayer(team, rolb, ReplaceSimAttr(rolb, 2, passDef[0]));
			ReplacePlayer(team, rilb, ReplaceSimAttr(rilb, 2, passDef[1]));
			ReplacePlayer(team, lilb, ReplaceSimAttr(lilb, 2, passDef[2]));
			ReplacePlayer(team, lolb, ReplaceSimAttr(lolb, 2, passDef[3]));
			ReplacePlayer(team, rcb,  ReplaceSimAttr(rcb,  2, passDef[4]));
			ReplacePlayer(team, lcb,  ReplaceSimAttr(lcb,  2, passDef[5]));
			ReplacePlayer(team, fs,   ReplaceSimAttr(fs,   2, passDef[6]));
			ReplacePlayer(team, ss,   ReplaceSimAttr(ss,   2, passDef[7]));
		}
		
		private void UpdateTeamSimPassRush(string team)
		{
			string re   = GetPlayerString(team, "RE");
			string le   = GetPlayerString(team, "LE");
			string nt   = GetPlayerString(team, "NT");
			string lolb = GetPlayerString(team, "LOLB");
			string lilb = GetPlayerString(team, "LILB");
			string rilb = GetPlayerString(team, "RILB");
			string rolb = GetPlayerString(team, "ROLB");
			string rcb  = GetPlayerString(team, "RCB");
			string lcb  = GetPlayerString(team, "LCB");
			string fs   = GetPlayerString(team, "FS");
			string ss   = GetPlayerString(team, "SS");

			if( re == null   || le == null   || nt == null   || 
				lolb == null || lilb == null || rilb == null || rolb == null ||
				rcb == null  || lcb == null || fs == null || ss == null )
			{
				// we need the entire defense in order to update this.
				return;
			}
			int[] reAttrs   = InputParser.GetInts(re);
			int[] leAttrs   = InputParser.GetInts(le);
			int[] ntAttrs   = InputParser.GetInts(nt);
			int[] lolbAttrs = InputParser.GetInts(lolb);
			int[] lilbAttrs = InputParser.GetInts(lilb);
			int[] rilbAttrs = InputParser.GetInts(rilb);
			int[] rolbAttrs = InputParser.GetInts(rolb);
			int[] rcbAttrs  = InputParser.GetInts(rcb);
			int[] lcbAttrs  = InputParser.GetInts(lcb);
			int[] fsAttrs   = InputParser.GetInts(fs);
			int[] ssAttrs   = InputParser.GetInts(ss);
			
			int[] rushDef = m_SimStuff.GetSimPassRush(
				reAttrs[2]+reAttrs[3], ntAttrs[2]+ntAttrs[3],
				leAttrs[2]+leAttrs[3],
				rolbAttrs[2]+rolbAttrs[3], rilbAttrs[2]+rilbAttrs[3], 
				lilbAttrs[2]+lilbAttrs[3], lolbAttrs[2]+lolbAttrs[3] );

			ReplacePlayer(team, re,   ReplaceSimAttr(re,   1, rushDef[0]));
			ReplacePlayer(team, nt ,  ReplaceSimAttr(nt,   1, rushDef[1]));
			ReplacePlayer(team, le ,  ReplaceSimAttr(le,   1, rushDef[2]));
			ReplacePlayer(team, rolb, ReplaceSimAttr(rolb, 1, rushDef[3]));
			ReplacePlayer(team, rilb, ReplaceSimAttr(rilb, 1, rushDef[4]));
			ReplacePlayer(team, lilb, ReplaceSimAttr(lilb, 1, rushDef[5]));
			ReplacePlayer(team, lolb, ReplaceSimAttr(lolb, 1, rushDef[6]));
			ReplacePlayer(team, rcb,  ReplaceSimAttr(rcb,  1, 0));
			ReplacePlayer(team, lcb,  ReplaceSimAttr(lcb,  1, 0));
			ReplacePlayer(team, fs,   ReplaceSimAttr(fs,   1, 0));
			ReplacePlayer(team, ss,   ReplaceSimAttr(ss,   1, rushDef[7]));

		}

		Regex m_SimRegex = null;
		/// <summary>
		/// replaces the sim attribute specified.
		/// </summary>
		/// <param name="line">Like: 
		/// "LOLB, trev ALBERTS, Face=0x26, #51, 25, 31, 31, 31, 38, 31 ,[30, 20 ]"</param>
		/// <param name="num">1 -> '30', 2->'20' above.</param>
		/// <param name="newValue">the new value</param>
		/// <returns>The input string with the specified replacement.</returns>
		public string ReplaceSimAttr(string line, int num, int newValue)
		{
			string ret = line;
			if( m_SimRegex == null)
			{
				m_SimRegex = new Regex("\\[\\s*([0-9]+)\\s*,\\s*([0-9]+)\\s*\\]");
			}
			Match m = m_SimRegex.Match(line);
			if( m != null )
			{
				int index = m.Groups[num].Index;
				int len   = m.Groups[num].ToString().Length;
				ret = string.Format("{0}{1}{2}", 
					line.Substring(0, index),
					newValue,
					line.Substring(index+len) );
			}
			return ret;
		}

		private void ValueChanged(object sender, System.EventArgs e)
		{
			AutoUpdateRoster();
		}

        private void m_LastNameTextBox_Leave(object sender, EventArgs e)
        {
            m_LastNameTextBox.Text = m_LastNameTextBox.Text.ToUpper();
        }

        private void m_FirstNameTextBox_Leave(object sender, EventArgs e)
        {
            m_FirstNameTextBox.Text = m_FirstNameTextBox.Text.ToLower();
        }

	}
}
