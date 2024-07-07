using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;

// TEAM = jets SimData=0x350, OFFENSIVE_FORMATION = 2RB_2WR_1TE
// PLAYBOOK R1265, P6173 
//
namespace TSBTool
{
	/// <summary>
	/// Summary description for ModifyTeamForm.
	/// </summary>
	public class ModifyTeamForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox m_TeamsComboBox;
		private System.Windows.Forms.Label m_Sim3Label;
		private System.Windows.Forms.Label m_Sim2Label;
		private System.Windows.Forms.Label m_Sim1Label;
		private System.Windows.Forms.NumericUpDown m_SimDefenseUpDown;
		private System.Windows.Forms.ComboBox m_OffensivePrefomComboBox;

		private bool m_InitDone = false;

		private string m_Data = "";
		private System.Windows.Forms.Button m_OKButton;
		private System.Windows.Forms.Button m_CancelButton;
		private System.Windows.Forms.NumericUpDown m_SimOffenseUpDown;
		private System.Windows.Forms.Button m_SaveTeamButton;
		private System.Windows.Forms.GroupBox m_RunPlaysGroupBox;
		private System.Windows.Forms.PictureBox m_Run1PictureBox;
		private System.Windows.Forms.NumericUpDown m_Run1UpDown;
		private System.Windows.Forms.NumericUpDown m_Run2UpDown;
		private System.Windows.Forms.PictureBox m_Run2PictureBox;
		private System.Windows.Forms.NumericUpDown m_Run3UpDown;
		private System.Windows.Forms.PictureBox m_Run3PictureBox;
		private System.Windows.Forms.NumericUpDown m_Run4UpDown;
		private System.Windows.Forms.PictureBox m_Run4PictureBox;
		private System.Windows.Forms.NumericUpDown m_Pass4UpDown;
		private System.Windows.Forms.PictureBox m_Pass4PictureBox;
		private System.Windows.Forms.NumericUpDown m_Pass3UpDown;
		private System.Windows.Forms.PictureBox m_Pass3PictureBox;
		private System.Windows.Forms.NumericUpDown m_Pass2UpDown;
		private System.Windows.Forms.PictureBox m_Pass2PictureBox;
		private System.Windows.Forms.NumericUpDown m_Pass1UpDown;
		private System.Windows.Forms.PictureBox m_Pass1PictureBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox m_PassPlaysGroupBox;
		private System.Windows.Forms.ComboBox m_FormationComboBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private Image[][] m_PlayImages;
		private System.Windows.Forms.Button m_AutoButton;
		private PictureBox[] m_Boxes;

		private Regex m_PlaybookRegex;
		private Regex m_OffensiveFormationRegex;
		#region properties
		
		/// <summary>
		/// The data to work on.
		/// </summary>
		public string Data
		{
			get{return m_Data; }

			set
			{ 
				m_InitDone = false;
				m_Data = value;
				if( m_Data.IndexOf("TEAM") != -1 )
				{
					if( m_Data.IndexOf("PLAYBOOK") > -1 )
					{
						m_RunPlaysGroupBox.Enabled  = true;
						m_PassPlaysGroupBox.Enabled = true;
					}

					if( m_Data.IndexOf("OFFENSIVE_FORMATION") > -1 )
					{
						m_FormationComboBox.Enabled = true;
					}
					SetupTeams();
					ShowTeamValues();
				}
				m_InitDone = true;
			}
		}

		public string CurrentTeam
		{
			get{return m_TeamsComboBox.SelectedItem.ToString();}

			set
			{
				int index = m_TeamsComboBox.Items.IndexOf(value);
				if(index > -1 )
				{
					m_TeamsComboBox.SelectedIndex = index;
				}
			}
		}

		#endregion

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

		public ModifyTeamForm()
		{
			m_InitDone = false;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_PlaybookRegex    = new Regex("PLAYBOOK (R[1-8]{4})\\s*,\\s*(P[1-8]{4})");
			m_OffensiveFormationRegex = new Regex("OFFENSIVE_FORMATION\\s*=\\s*([a-zA-Z1234_]+)");
			m_Boxes = new PictureBox[8];
			m_Boxes[0] = m_Run1PictureBox;
			m_Boxes[1] = m_Run2PictureBox;
			m_Boxes[2] = m_Run3PictureBox;
			m_Boxes[3] = m_Run4PictureBox;
			m_Boxes[4] = m_Pass1PictureBox;
			m_Boxes[5] = m_Pass2PictureBox;
			m_Boxes[6] = m_Pass3PictureBox;
			m_Boxes[7] = m_Pass4PictureBox;

			m_RunPlaysGroupBox.Enabled = false;
			m_PassPlaysGroupBox.Enabled = false;
			m_FormationComboBox.Enabled = false;
			PopulatePlayImages();
			m_TeamsComboBox.SelectedIndex = 0;
			m_InitDone = true;
		}

		/// <summary>
		/// Reads in the play images into the m_PlayImages array.
		/// </summary>
		private void PopulatePlayImages()
		{
			m_PlayImages = new Image[8][];
			for(int i = 0; i < m_PlayImages.Length; i++)
			{
				m_PlayImages[i] = new Image[8];
			}

			string fileName ="";
			string type = "R";
			int num = 0;
			for(int i = 0; i < m_PlayImages.Length; i++)
			{
				if( i > 3 )
				{
					type = "P";
					num = i-4;
				}
				else 
					num = i;
				for(int j = 0; j < m_PlayImages[i].Length; j++)
				{//TSBTool.FACES.
					fileName = string.Format("TSBTool.PLAYS.{0}{1}-{2}.BMP",
						type,num+1,j);
                    m_PlayImages[i][j] = MainClass.GetImage(fileName);
				}
			}
		}

		/// <summary>
		/// Returns the currently selected offensive formation string.
		/// </summary>
		/// <returns></returns>
		private string GetOffensiveFormation()
		{
			string ret = string.Format("OFFENSIVE_FORMATION = {0}",
				m_FormationComboBox.SelectedItem.ToString());
			return ret;
		}

		/// <summary>
		/// Gets the current playbook string.
		/// </summary>
		/// <returns></returns>
		private string GetCurrentPlaybook()
		{
			string ret = string.Format("PLAYBOOK R{0}{1}{2}{3}, P{4}{5}{6}{7} ",
				m_Run1UpDown.Value,
				m_Run2UpDown.Value,
				m_Run3UpDown.Value,
				m_Run4UpDown.Value,
				m_Pass1UpDown.Value,
				m_Pass2UpDown.Value,
				m_Pass3UpDown.Value,
				m_Pass4UpDown.Value
				);
			return ret;
		}

		/// <summary>
		/// Shows the data for the current team.
		/// </summary>
		private void ShowTeamValues()
		{
			string team    = m_TeamsComboBox.SelectedItem.ToString();
			string line = GetTeamString(team);
			
			if( line != null )
			{
				int[] vals = InputParser.GetSimData(line);

				if( vals != null && vals[1] > -1 && vals[1] < 4 )
					m_OffensivePrefomComboBox.SelectedIndex = vals[1];
			
				byte[] simVals = GetNibbles(vals[0]);
				m_SimOffenseUpDown.Value = simVals[0];
				m_SimDefenseUpDown.Value = simVals[1];

				Match ofMatch = m_OffensiveFormationRegex.Match(line);
				Match pbMatch = m_PlaybookRegex.Match(line);
				if( ofMatch != Match.Empty )
				{
					string val = ofMatch.Groups[1].ToString();
					int index = m_FormationComboBox.Items.IndexOf(val);
					if( index > -1 )
						m_FormationComboBox.SelectedIndex = index;
				}
				if( pbMatch != Match.Empty )
				{
					string runs   = pbMatch.Groups[1].ToString();
					string passes = pbMatch.Groups[2].ToString();
					SetRuns(runs);
					SetPasses(passes);
				}
			}
		}

		private void SetRuns(string runs)
		{
			if(runs != null && runs.Length == 5)
			{
				m_Run1UpDown.Value = Int32.Parse(""+ runs[1]);
				m_Run2UpDown.Value = Int32.Parse(""+ runs[2]);
				m_Run3UpDown.Value = Int32.Parse(""+ runs[3]);
				m_Run4UpDown.Value = Int32.Parse(""+ runs[4]);
			}
		}

		private void SetPasses(string passes)
		{
			if(passes != null && passes.Length == 5)
			{
				m_Pass1UpDown.Value = Int32.Parse(""+ passes[1]);
				m_Pass2UpDown.Value = Int32.Parse(""+ passes[2]);
				m_Pass3UpDown.Value = Int32.Parse(""+ passes[3]);
				m_Pass4UpDown.Value = Int32.Parse(""+ passes[4]);
			}
		}

		/// <summary>
		/// Gets a string like:
		/// "TEAM = bills SimData=0xab0"
		/// that is currently from m_Data.
		/// </summary>
		/// <param name="team"></param>
		/// <returns></returns>
		private string GetTeamString(string team)
		{
			string theTeam = string.Format("TEAM = {0}",team);
			int teamIndex  = m_Data.IndexOf(theTeam);
			int newLine    = -1;
			string line = null;

			if( teamIndex > -1 && (newLine = m_Data.IndexOf('\n',teamIndex)) > -1 )
			{
				line = m_Data.Substring(teamIndex, newLine - teamIndex);
			}
			if( line != null && m_PassPlaysGroupBox.Enabled )
			{
				Match m = m_PlaybookRegex.Match(m_Data,newLine);
				line = line + "\n" + m.Value;
			}
			return line;
		}

		/// <summary>
		/// Gwts a string that represents the values set in the UI.
		/// Like:
		/// "TEAM = bills SimData=0xab0"
		/// </summary>
		/// <returns></returns>
		private string GetCurrentValues()
		{
			string ret = string.Format("{0:x}{1:x}{2}",
				(int)m_SimOffenseUpDown.Value,
				(int)m_SimDefenseUpDown.Value,
				m_OffensivePrefomComboBox.SelectedIndex);

			return ret;
		}

		/// <summary>
		/// Gets the text representation of the current UI.
		/// </summary>
		/// <returns></returns>
		private string GetCurrentTeamString()
		{
			string vals = GetCurrentValues();
			string ret = string.Format("TEAM = {0} SimData=0x{1}",
				m_TeamsComboBox.SelectedItem,
				vals);
			if( m_FormationComboBox.Enabled )
				ret = ret + ", "+GetOffensiveFormation();
			if(m_RunPlaysGroupBox.Enabled  )
				ret = ret + "\n"+ GetCurrentPlaybook();

			return ret;
		}

		private void UpdateData()
		{
			if(m_InitDone)
			{
				string team     = m_TeamsComboBox.SelectedItem.ToString();
				string oldValue = GetTeamString(team);
				string newValue = GetCurrentTeamString();

				m_Data = m_Data.Replace(oldValue, newValue);
			}
		}

		/// <summary>
		/// Returns the associated nibbles for the value passed (assuming it's a byte).
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		private byte[] GetNibbles(int val)
		{
			byte[] ret = new byte[2];
			byte byteValue = (byte) val;
			ret[1] = (byte)(byteValue & 0x0F); // lo byte
			ret[0] = (byte)(byteValue >> 4);
			return ret;
		}

		private void AutoUpdateAllTeams()
		{
            TecmonsterTeamSim sim = new TecmonsterTeamSim();
            m_Data = sim.AutoUpdateSeasonSimData(1, m_Data);
            ShowTeamValues();
            MessageBox.Show(this, "Done.");
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyTeamForm));
            this.label1 = new System.Windows.Forms.Label();
            this.m_TeamsComboBox = new System.Windows.Forms.ComboBox();
            this.m_OffensivePrefomComboBox = new System.Windows.Forms.ComboBox();
            this.m_SimDefenseUpDown = new System.Windows.Forms.NumericUpDown();
            this.m_SimOffenseUpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Sim3Label = new System.Windows.Forms.Label();
            this.m_Sim2Label = new System.Windows.Forms.Label();
            this.m_Sim1Label = new System.Windows.Forms.Label();
            this.m_OKButton = new System.Windows.Forms.Button();
            this.m_CancelButton = new System.Windows.Forms.Button();
            this.m_SaveTeamButton = new System.Windows.Forms.Button();
            this.m_RunPlaysGroupBox = new System.Windows.Forms.GroupBox();
            this.m_Run4UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Run4PictureBox = new System.Windows.Forms.PictureBox();
            this.m_Run3UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Run3PictureBox = new System.Windows.Forms.PictureBox();
            this.m_Run2UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Run2PictureBox = new System.Windows.Forms.PictureBox();
            this.m_Run1UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Run1PictureBox = new System.Windows.Forms.PictureBox();
            this.m_PassPlaysGroupBox = new System.Windows.Forms.GroupBox();
            this.m_Pass4UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Pass4PictureBox = new System.Windows.Forms.PictureBox();
            this.m_Pass3UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Pass3PictureBox = new System.Windows.Forms.PictureBox();
            this.m_Pass2UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Pass2PictureBox = new System.Windows.Forms.PictureBox();
            this.m_Pass1UpDown = new System.Windows.Forms.NumericUpDown();
            this.m_Pass1PictureBox = new System.Windows.Forms.PictureBox();
            this.m_FormationComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.m_AutoButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_SimDefenseUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_SimOffenseUpDown)).BeginInit();
            this.m_RunPlaysGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run4UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run4PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run3UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run3PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run2PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run1UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run1PictureBox)).BeginInit();
            this.m_PassPlaysGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass4UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass4PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass3UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass3PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass2UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass2PictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass1UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass1PictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 2;
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
            this.m_TeamsComboBox.Location = new System.Drawing.Point(8, 48);
            this.m_TeamsComboBox.Name = "m_TeamsComboBox";
            this.m_TeamsComboBox.Size = new System.Drawing.Size(104, 21);
            this.m_TeamsComboBox.TabIndex = 1;
            this.m_TeamsComboBox.SelectedIndexChanged += new System.EventHandler(this.m_TeamsComboBox_SelectedIndexChanged);
            // 
            // m_OffensivePrefomComboBox
            // 
            this.m_OffensivePrefomComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_OffensivePrefomComboBox.Items.AddRange(new object[] {
            "Balance Rush",
            "Heavy Rushing",
            "Balance Pass",
            "Heavy Pass"});
            this.m_OffensivePrefomComboBox.Location = new System.Drawing.Point(280, 48);
            this.m_OffensivePrefomComboBox.Name = "m_OffensivePrefomComboBox";
            this.m_OffensivePrefomComboBox.Size = new System.Drawing.Size(128, 21);
            this.m_OffensivePrefomComboBox.TabIndex = 15;
            this.m_OffensivePrefomComboBox.SelectedIndexChanged += new System.EventHandler(this.m_OffensivePrefomComboBox_SelectedIndexChanged);
            // 
            // m_SimDefenseUpDown
            // 
            this.m_SimDefenseUpDown.Location = new System.Drawing.Point(200, 48);
            this.m_SimDefenseUpDown.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.m_SimDefenseUpDown.Name = "m_SimDefenseUpDown";
            this.m_SimDefenseUpDown.Size = new System.Drawing.Size(40, 20);
            this.m_SimDefenseUpDown.TabIndex = 10;
            this.m_SimDefenseUpDown.ValueChanged += new System.EventHandler(this.m_SimDefenseUpDown_ValueChanged);
            // 
            // m_SimOffenseUpDown
            // 
            this.m_SimOffenseUpDown.Location = new System.Drawing.Point(136, 48);
            this.m_SimOffenseUpDown.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.m_SimOffenseUpDown.Name = "m_SimOffenseUpDown";
            this.m_SimOffenseUpDown.Size = new System.Drawing.Size(40, 20);
            this.m_SimOffenseUpDown.TabIndex = 5;
            this.m_SimOffenseUpDown.ValueChanged += new System.EventHandler(this.m_SimOffenseUpDown_ValueChanged);
            // 
            // m_Sim3Label
            // 
            this.m_Sim3Label.Location = new System.Drawing.Point(288, 8);
            this.m_Sim3Label.Name = "m_Sim3Label";
            this.m_Sim3Label.Size = new System.Drawing.Size(112, 32);
            this.m_Sim3Label.TabIndex = 26;
            this.m_Sim3Label.Text = "Run/Pass Ratio";
            this.m_Sim3Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_Sim2Label
            // 
            this.m_Sim2Label.Location = new System.Drawing.Point(192, 8);
            this.m_Sim2Label.Name = "m_Sim2Label";
            this.m_Sim2Label.Size = new System.Drawing.Size(80, 32);
            this.m_Sim2Label.TabIndex = 25;
            this.m_Sim2Label.Text = "Sim Pass Defense";
            this.m_Sim2Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_Sim1Label
            // 
            this.m_Sim1Label.Location = new System.Drawing.Point(112, 8);
            this.m_Sim1Label.Name = "m_Sim1Label";
            this.m_Sim1Label.Size = new System.Drawing.Size(72, 32);
            this.m_Sim1Label.TabIndex = 24;
            this.m_Sim1Label.Text = "Sim Rush Defense";
            this.m_Sim1Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // m_OKButton
            // 
            this.m_OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_OKButton.Location = new System.Drawing.Point(144, 392);
            this.m_OKButton.Name = "m_OKButton";
            this.m_OKButton.Size = new System.Drawing.Size(128, 23);
            this.m_OKButton.TabIndex = 45;
            this.m_OKButton.Text = "&OK";
            // 
            // m_CancelButton
            // 
            this.m_CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_CancelButton.Location = new System.Drawing.Point(280, 392);
            this.m_CancelButton.Name = "m_CancelButton";
            this.m_CancelButton.Size = new System.Drawing.Size(128, 23);
            this.m_CancelButton.TabIndex = 50;
            this.m_CancelButton.Text = "&Cancel";
            // 
            // m_SaveTeamButton
            // 
            this.m_SaveTeamButton.Location = new System.Drawing.Point(0, 352);
            this.m_SaveTeamButton.Name = "m_SaveTeamButton";
            this.m_SaveTeamButton.Size = new System.Drawing.Size(272, 23);
            this.m_SaveTeamButton.TabIndex = 35;
            this.m_SaveTeamButton.Text = "&Save Team Data";
            this.m_SaveTeamButton.Visible = false;
            this.m_SaveTeamButton.Click += new System.EventHandler(this.m_SaveTeamButton_Click);
            // 
            // m_RunPlaysGroupBox
            // 
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run4UpDown);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run4PictureBox);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run3UpDown);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run3PictureBox);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run2UpDown);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run2PictureBox);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run1UpDown);
            this.m_RunPlaysGroupBox.Controls.Add(this.m_Run1PictureBox);
            this.m_RunPlaysGroupBox.Location = new System.Drawing.Point(8, 80);
            this.m_RunPlaysGroupBox.Name = "m_RunPlaysGroupBox";
            this.m_RunPlaysGroupBox.Size = new System.Drawing.Size(264, 128);
            this.m_RunPlaysGroupBox.TabIndex = 25;
            this.m_RunPlaysGroupBox.TabStop = false;
            this.m_RunPlaysGroupBox.Text = "Run Plays";
            // 
            // m_Run4UpDown
            // 
            this.m_Run4UpDown.Location = new System.Drawing.Point(199, 94);
            this.m_Run4UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Run4UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run4UpDown.Name = "m_Run4UpDown";
            this.m_Run4UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Run4UpDown.TabIndex = 3;
            this.m_Run4UpDown.Tag = "3";
            this.m_Run4UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run4UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Run4PictureBox
            // 
            this.m_Run4PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Run4PictureBox.Image")));
            this.m_Run4PictureBox.Location = new System.Drawing.Point(199, 14);
            this.m_Run4PictureBox.Name = "m_Run4PictureBox";
            this.m_Run4PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Run4PictureBox.TabIndex = 6;
            this.m_Run4PictureBox.TabStop = false;
            // 
            // m_Run3UpDown
            // 
            this.m_Run3UpDown.Location = new System.Drawing.Point(136, 94);
            this.m_Run3UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Run3UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run3UpDown.Name = "m_Run3UpDown";
            this.m_Run3UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Run3UpDown.TabIndex = 2;
            this.m_Run3UpDown.Tag = "2";
            this.m_Run3UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run3UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Run3PictureBox
            // 
            this.m_Run3PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Run3PictureBox.Image")));
            this.m_Run3PictureBox.Location = new System.Drawing.Point(136, 14);
            this.m_Run3PictureBox.Name = "m_Run3PictureBox";
            this.m_Run3PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Run3PictureBox.TabIndex = 4;
            this.m_Run3PictureBox.TabStop = false;
            // 
            // m_Run2UpDown
            // 
            this.m_Run2UpDown.Location = new System.Drawing.Point(72, 96);
            this.m_Run2UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Run2UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run2UpDown.Name = "m_Run2UpDown";
            this.m_Run2UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Run2UpDown.TabIndex = 1;
            this.m_Run2UpDown.Tag = "1";
            this.m_Run2UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run2UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Run2PictureBox
            // 
            this.m_Run2PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Run2PictureBox.Image")));
            this.m_Run2PictureBox.Location = new System.Drawing.Point(72, 16);
            this.m_Run2PictureBox.Name = "m_Run2PictureBox";
            this.m_Run2PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Run2PictureBox.TabIndex = 2;
            this.m_Run2PictureBox.TabStop = false;
            // 
            // m_Run1UpDown
            // 
            this.m_Run1UpDown.Location = new System.Drawing.Point(8, 96);
            this.m_Run1UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Run1UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run1UpDown.Name = "m_Run1UpDown";
            this.m_Run1UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Run1UpDown.TabIndex = 0;
            this.m_Run1UpDown.Tag = "0";
            this.m_Run1UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Run1UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Run1PictureBox
            // 
            this.m_Run1PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Run1PictureBox.Image")));
            this.m_Run1PictureBox.Location = new System.Drawing.Point(8, 16);
            this.m_Run1PictureBox.Name = "m_Run1PictureBox";
            this.m_Run1PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Run1PictureBox.TabIndex = 0;
            this.m_Run1PictureBox.TabStop = false;
            // 
            // m_PassPlaysGroupBox
            // 
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass4UpDown);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass4PictureBox);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass3UpDown);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass3PictureBox);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass2UpDown);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass2PictureBox);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass1UpDown);
            this.m_PassPlaysGroupBox.Controls.Add(this.m_Pass1PictureBox);
            this.m_PassPlaysGroupBox.Location = new System.Drawing.Point(8, 216);
            this.m_PassPlaysGroupBox.Name = "m_PassPlaysGroupBox";
            this.m_PassPlaysGroupBox.Size = new System.Drawing.Size(264, 128);
            this.m_PassPlaysGroupBox.TabIndex = 30;
            this.m_PassPlaysGroupBox.TabStop = false;
            this.m_PassPlaysGroupBox.Text = "Pass Plays";
            // 
            // m_Pass4UpDown
            // 
            this.m_Pass4UpDown.Location = new System.Drawing.Point(199, 94);
            this.m_Pass4UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Pass4UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass4UpDown.Name = "m_Pass4UpDown";
            this.m_Pass4UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Pass4UpDown.TabIndex = 7;
            this.m_Pass4UpDown.Tag = "7";
            this.m_Pass4UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass4UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Pass4PictureBox
            // 
            this.m_Pass4PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Pass4PictureBox.Image")));
            this.m_Pass4PictureBox.Location = new System.Drawing.Point(199, 14);
            this.m_Pass4PictureBox.Name = "m_Pass4PictureBox";
            this.m_Pass4PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Pass4PictureBox.TabIndex = 6;
            this.m_Pass4PictureBox.TabStop = false;
            // 
            // m_Pass3UpDown
            // 
            this.m_Pass3UpDown.Location = new System.Drawing.Point(136, 94);
            this.m_Pass3UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Pass3UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass3UpDown.Name = "m_Pass3UpDown";
            this.m_Pass3UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Pass3UpDown.TabIndex = 6;
            this.m_Pass3UpDown.Tag = "6";
            this.m_Pass3UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass3UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Pass3PictureBox
            // 
            this.m_Pass3PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Pass3PictureBox.Image")));
            this.m_Pass3PictureBox.Location = new System.Drawing.Point(136, 14);
            this.m_Pass3PictureBox.Name = "m_Pass3PictureBox";
            this.m_Pass3PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Pass3PictureBox.TabIndex = 4;
            this.m_Pass3PictureBox.TabStop = false;
            // 
            // m_Pass2UpDown
            // 
            this.m_Pass2UpDown.Location = new System.Drawing.Point(72, 96);
            this.m_Pass2UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Pass2UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass2UpDown.Name = "m_Pass2UpDown";
            this.m_Pass2UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Pass2UpDown.TabIndex = 5;
            this.m_Pass2UpDown.Tag = "5";
            this.m_Pass2UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass2UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Pass2PictureBox
            // 
            this.m_Pass2PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Pass2PictureBox.Image")));
            this.m_Pass2PictureBox.Location = new System.Drawing.Point(72, 16);
            this.m_Pass2PictureBox.Name = "m_Pass2PictureBox";
            this.m_Pass2PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Pass2PictureBox.TabIndex = 2;
            this.m_Pass2PictureBox.TabStop = false;
            // 
            // m_Pass1UpDown
            // 
            this.m_Pass1UpDown.Location = new System.Drawing.Point(8, 96);
            this.m_Pass1UpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.m_Pass1UpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass1UpDown.Name = "m_Pass1UpDown";
            this.m_Pass1UpDown.Size = new System.Drawing.Size(50, 20);
            this.m_Pass1UpDown.TabIndex = 4;
            this.m_Pass1UpDown.Tag = "4";
            this.m_Pass1UpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.m_Pass1UpDown.ValueChanged += new System.EventHandler(this.m_UpDown_ValueChanged);
            // 
            // m_Pass1PictureBox
            // 
            this.m_Pass1PictureBox.Image = ((System.Drawing.Image)(resources.GetObject("m_Pass1PictureBox.Image")));
            this.m_Pass1PictureBox.Location = new System.Drawing.Point(8, 16);
            this.m_Pass1PictureBox.Name = "m_Pass1PictureBox";
            this.m_Pass1PictureBox.Size = new System.Drawing.Size(50, 74);
            this.m_Pass1PictureBox.TabIndex = 0;
            this.m_Pass1PictureBox.TabStop = false;
            // 
            // m_FormationComboBox
            // 
            this.m_FormationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_FormationComboBox.Items.AddRange(new object[] {
            "2RB_2WR_1TE",
            "1RB_4WR",
            "1RB_3WR_1TE"});
            this.m_FormationComboBox.Location = new System.Drawing.Point(280, 104);
            this.m_FormationComboBox.Name = "m_FormationComboBox";
            this.m_FormationComboBox.Size = new System.Drawing.Size(128, 21);
            this.m_FormationComboBox.TabIndex = 20;
            this.m_FormationComboBox.SelectedIndexChanged += new System.EventHandler(this.m_FormationComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(288, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 28;
            this.label2.Text = "Formation";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_AutoButton
            // 
            this.m_AutoButton.BackColor = System.Drawing.Color.LightCoral;
            this.m_AutoButton.Location = new System.Drawing.Point(280, 136);
            this.m_AutoButton.Name = "m_AutoButton";
            this.m_AutoButton.Size = new System.Drawing.Size(120, 40);
            this.m_AutoButton.TabIndex = 40;
            this.m_AutoButton.Text = "Auto Update All Team Sim attributes";
            this.m_AutoButton.UseVisualStyleBackColor = false;
            this.m_AutoButton.Click += new System.EventHandler(this.m_AutoButton_Click);
            // 
            // ModifyTeamForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.m_CancelButton;
            this.ClientSize = new System.Drawing.Size(408, 418);
            this.Controls.Add(this.m_AutoButton);
            this.Controls.Add(this.m_FormationComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_RunPlaysGroupBox);
            this.Controls.Add(this.m_SaveTeamButton);
            this.Controls.Add(this.m_CancelButton);
            this.Controls.Add(this.m_OKButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_TeamsComboBox);
            this.Controls.Add(this.m_OffensivePrefomComboBox);
            this.Controls.Add(this.m_SimDefenseUpDown);
            this.Controls.Add(this.m_SimOffenseUpDown);
            this.Controls.Add(this.m_Sim3Label);
            this.Controls.Add(this.m_Sim2Label);
            this.Controls.Add(this.m_Sim1Label);
            this.Controls.Add(this.m_PassPlaysGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(424, 456);
            this.MinimumSize = new System.Drawing.Size(424, 456);
            this.Name = "ModifyTeamForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify Team attributes";
            ((System.ComponentModel.ISupportInitialize)(this.m_SimDefenseUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_SimOffenseUpDown)).EndInit();
            this.m_RunPlaysGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_Run4UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run4PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run3UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run3PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run2PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run1UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Run1PictureBox)).EndInit();
            this.m_PassPlaysGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass4UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass4PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass3UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass3PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass2UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass2PictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass1UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_Pass1PictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void m_TeamsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(m_InitDone)
			{
				ShowTeamValues();
			}
		}

		private void m_SaveTeamButton_Click(object sender, System.EventArgs e)
		{
			UpdateData();
		}

		private void m_UpDown_ValueChanged(object sender, System.EventArgs e)
		{
			NumericUpDown ud = sender as NumericUpDown;
			if( ud != null )
			{
				int i = ud.TabIndex;
				int j = (int)ud.Value;
				UpdatePictureBox(i, j-1);
			}
			UpdateData();
		}

		private void UpdatePictureBox(int pictureBoxNumber, int pictureNumber)
		{
			Image img = m_PlayImages[pictureBoxNumber][pictureNumber];
			m_Boxes[pictureBoxNumber].Image = img;
		}

		private void m_AutoButton_Click(object sender, System.EventArgs e)
		{
			if( MessageBox.Show(this,  
				"Are you sure you want to update the sim data for ALL teams?", 
				"Are you sure?",
				MessageBoxButtons.YesNo) == DialogResult.Yes )
			{
				AutoUpdateAllTeams();
			}
		}

		private void m_SimOffenseUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateData();
		}

		private void m_SimDefenseUpDown_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateData();
		}

		private void m_OffensivePrefomComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateData();
		}

		private void m_FormationComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateData();
		}

	}
}
