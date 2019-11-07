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
	/// Action Sequence Team Colors: x342D8 to x343B7 (Buffalo to Atlanta)
	///	Pro Bowl Action Sequence Team Colors: x343B8 to x343C7 (AFC to NFC)
	///	Example (Buffalo) x15 x11 x15 x11 x00 x00 x00 x00:
	///	x15 x11 = NES Colors and Make Up Jersey 1
	///	x15 x11 = NES Colors and Make Up Jersey 2
	///	x00 x00 x00 x00 = When to Use Jersey 1 and 2 this is done bitwise 
	/// (Buffalo to Atlanta then AFC to NFC with final 2 bits always being 0s) 
	/// where a 0 = Jersey 1 and 1 = Jersey 2.
	///	(x00000000 would use Jersey 1 vs every team and xFFFFFFFC would use 
	///	Jersey 2 vs every team)
	/// </summary>
	public class UniformEditForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox mHomePictureBox;
		private System.Windows.Forms.PictureBox mAwayPictureBox;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem mScaleMenuItem;
		private System.Windows.Forms.MenuItem mCloseMenuItem;
		private System.Windows.Forms.Button mHomeJerseyButton;
		private System.Windows.Forms.Button mHomePantsButton;
		private System.Windows.Forms.Button mAwayPantsButton;
		private System.Windows.Forms.Button mAwayJerseyButton;
		private System.Windows.Forms.Label mHomeJerseyLabel;
		private System.Windows.Forms.Label mHomePantsLabel;
		private System.Windows.Forms.Label mAwayPantsLabel;
		private System.Windows.Forms.Label mAwayJerseyLabel;
		private System.Windows.Forms.PictureBox mDivChampPictureBox;
		private System.Windows.Forms.PictureBox mConfChampPictureBox;
		private System.Windows.Forms.Label mDivChampUniform1Label;
		private System.Windows.Forms.Label mDivChampUniform2Label;
		private System.Windows.Forms.Label mDivChampUniform3Label;
		private System.Windows.Forms.Label mDivChampHelmet1Label;
		private System.Windows.Forms.Label mDivChampHelmet2Label;
		private System.Windows.Forms.Label mConfChampHelmetLabel;
		private System.Windows.Forms.Label mConfChampUniform3Label;
		private System.Windows.Forms.Label mConfChampUniform2Label;
		private System.Windows.Forms.Label mConfChampUniform1Label;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button mCancelButton;
		private System.Windows.Forms.Button mSkin2Button;
		private System.Windows.Forms.Button mSkin1Button;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox mTeamsComboBox;
		private System.Windows.Forms.Label mSkin2Label;
		private System.Windows.Forms.Label mSkin1Label;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button button2;
        private IContainer components;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.GroupBox mConfChampGroupBox;
		private System.Windows.Forms.GroupBox mDivChampGroupBox;
		private System.Windows.Forms.GroupBox mUniform1GroupBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private HomeAwayUniformForm mHomeAwayUniformForm = new HomeAwayUniformForm();

		public UniformEditForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		private string m_Data = String.Empty;

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
				}
			}
		}

		private void SetupTeams()
		{
			Regex teamRegex = new Regex("TEAM\\s*=\\s*([a-z0-9]+)");
			MatchCollection mc = teamRegex.Matches(m_Data);

			mTeamsComboBox.Items.Clear();
			mTeamsComboBox.BeginUpdate();
			foreach( Match m in mc)
			{
				string team = m.Groups[1].Value ;
				mTeamsComboBox.Items.Add( team );
			}
			mTeamsComboBox.EndUpdate();
			if( mTeamsComboBox.Items.Count > 0 )
			{
				mTeamsComboBox.SelectedItem = mTeamsComboBox.Items[0];
			}
		}

		/// <summary>
		/// Get and set the current team.
		/// </summary>
		public string CurrentTeam
		{
			get{ return this.mTeamsComboBox.SelectedItem.ToString();}

			set
			{
				int index = mTeamsComboBox.Items.IndexOf(value);
				if(index > -1 )
				{
					mTeamsComboBox.SelectedIndex = index;
					SetCurrentTeamColors();
				}
			}
		}

		/// <summary>
		/// Gets a color 'line' from m_Data from 'team' playing 'position'.
		/// </summary>
		/// <param name="team"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		private string GetColorString( string team )
		{
			string ret =string.Empty;
			string pattern = "TEAM\\s*=\\s*"+team;
			Regex findTeamRegex = new Regex(pattern);
			Match m = findTeamRegex.Match(m_Data);
			if( m != Match.Empty )
			{
				int teamIndex = m.Index;
				if( teamIndex == -1 )
					return null;
				int lineStart = m_Data.IndexOf("COLORS",teamIndex);
				int lineEnd = -1;
				if( lineStart > 0 )
				{
					lineEnd = m_Data.IndexOf("\n",lineStart+3);
				}
				if( lineStart > -1 && lineEnd > -1)
				{
					ret = m_Data.Substring(lineStart, lineEnd-lineStart);
				}
			}
			return ret;
		}

		/// <summary>
		/// Updates the GUI with the current team colors.
		/// </summary>
		private void SetCurrentTeamColors()
		{
			if( mTeamsComboBox.SelectedItem != null )
			{
				string colorData = GetColorString( CurrentTeam );
				if( colorData != null )
					SetFormColorData(colorData);
			}
		}

		/// <summary>
		/// Gets the color string represented by the current state of the UI.
		/// </summary>
		/// <returns></returns>
		private string GetCurrentTeamColorData_UI()
		{
			//COLORS Home=0x3038, Away=0x3038, Skin=0x060f, DivChamp=0x0111012515, ConfChamp=0x11012501
			string ret = string.Empty;
			ret = string.Format(
"COLORS Uniform1=0x{0}{1}{2}, Uniform2=0x{3}{4}{5}, "+
"DivChamp=0x{6}{7}{8}{9}{10}, ConfChamp=0x{11}{12}{13}{14},UniformUsage=0x{15}",
				HomePantsColorString,
				Skin1ColorString,
				HomeJerseyColorString,
				AwayPantsColorString,
				Skin2ColorString,
				AwayJerseyColorString,
				DivisionChampUniform1ColorString,
				DivisionChampUniform2ColorString,
				DivisionChampUniform3ColorString,
				DivisionChampHelmet1ColorString,
				DivisionChampHelmet2ColorString,
				ConfrenceChampUniform1ColorString,
				ConfrenceChampUniform2ColorString,
				ConfrenceChampUniform3ColorString,
				ConfrenceChampHelmetColorString,
				UnifromUsageString
				);
			return ret;
		}

		private void ReplaceColorData()
		{
			string oldData = GetColorString(CurrentTeam);
			string newData = GetCurrentTeamColorData_UI();
			if( oldData != null )
			{
				ReplaceColorData(CurrentTeam, oldData, newData);
			}
		}

		private void ReplaceColorData(string team, string oldData, string newData)
		{
			int nextTeamIndex = -1;
			int currentTeamIndex= -1;
			string nextTeam    = null;

			Regex findTeamRegex = new Regex("TEAM\\s*=\\s*"+team);
			
			Match m = findTeamRegex.Match(m_Data);
			if( !m.Success )
				return;

			currentTeamIndex = m.Groups[1].Index;

			int test = mTeamsComboBox.Items.IndexOf(team);

			if( test != mTeamsComboBox.Items.Count - 1 )
			{
				nextTeam      = string.Format("TEAM\\s*=\\s*{0}",mTeamsComboBox.Items[test+1]);
				Regex nextTeamRegex = new Regex(nextTeam);
				Match nt = nextTeamRegex.Match(m_Data);
				if( nt.Success )
					nextTeamIndex = nt.Index;
				//nextTeamIndex = m_Data.IndexOf(nextTeam);
			}
			if( nextTeamIndex < 0)
				nextTeamIndex = m_Data.Length;

			
			int dataIndex = m_Data.IndexOf(oldData,currentTeamIndex);
			if( dataIndex > -1 )
			{
				int endLine     = m_Data.IndexOf('\n',dataIndex);
				string start    = m_Data.Substring(0,dataIndex);
				string last     = m_Data.Substring(endLine);
				
				StringBuilder tmp = new StringBuilder(m_Data.Length + 200);
				tmp.Append(start);
				tmp.Append(newData);
				tmp.Append(last);

				m_Data = tmp.ToString();
				//m_Data = start + newPlayer + last;
			}
			else
			{
				string error = string.Format(
@"An error occured looking up team
     '{0}'
Please verify that this teams's color attributes are correct.", oldData);
				MessageBox.Show(error);
			}
		}

		/// <summary>
		/// Sets up the form with the data in 'colorData'.
		/// </summary>
		/// <param name="colorData"></param>
		private void SetFormColorData(string colorData)
		{
			string homeUniform = InputParser.GetHomeUniformColorString(colorData);
			string awayUniform = InputParser.GetAwayUniformColorString(colorData);
			string confChamp   = InputParser.GetConfChampColorString(colorData);
			string divChamp    = InputParser.GetDivChampColorString(colorData);
			string uniformUsage= InputParser.GetUniformUsageString(colorData);

			if( homeUniform != null && homeUniform.Length > 5 )
			{
				this.HomePantsColorString = homeUniform.Substring(0,2);
				this.Skin1ColorString     = homeUniform.Substring(2,2);
				this.HomeJerseyColorString= homeUniform.Substring(4,2);
			}
			if( awayUniform != null && awayUniform.Length > 5 )
			{
				this.AwayPantsColorString = awayUniform.Substring(0,2);
				this.Skin2ColorString     = awayUniform.Substring(2,2);
				this.AwayJerseyColorString= awayUniform.Substring(4,2);
			}
			if( confChamp != null && confChamp.Length > 7)
			{
//				mDivChampGroupBox.Enabled = true;
				this.ConfrenceChampUniform1ColorString = confChamp.Substring(0,2);
				this.ConfrenceChampUniform2ColorString = confChamp.Substring(2,2);
				this.ConfrenceChampUniform3ColorString = confChamp.Substring(4,2);
				this.ConfrenceChampHelmetColorString   = confChamp.Substring(6,2);
			}
//			else
//			{
//				mDivChampGroupBox.Enabled = false;
//			}
			if( divChamp != null && divChamp.Length > 9)
			{
//				mDivChampGroupBox.Enabled = true;
				this.DivisionChampUniform1ColorString = divChamp.Substring(0,2);
				this.DivisionChampUniform2ColorString = divChamp.Substring(2,2);
				this.DivisionChampUniform3ColorString = divChamp.Substring(4,2);
				this.DivisionChampHelmet1ColorString  = divChamp.Substring(6,2);
				this.DivisionChampHelmet2ColorString  = divChamp.Substring(8,2);
			}
//			else
//			{
//				mDivChampGroupBox.Enabled = false;
//			}
			if( uniformUsage != null && uniformUsage.Length > 7 )
			{
				this.UnifromUsageString = uniformUsage;
			}
		}

		private ColorForm2 mColorForm2 = new ColorForm2();

//		private string mConfrenceChampUniform1ColorString;

		public string ConfrenceChampUniform1ColorString
		{
			get { return mConfChampUniform1Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mConfChampUniform1Label.Text = value;
//					mConfrenceChampUniform1ColorString = value; 
					mConfChampUniform1Label.BackColor = c;
					Bitmap bmp = new Bitmap(mConfChampPictureBox.Image);
					SetBitmapColor( bmp, c, mConfChampUniform1);
					mConfChampPictureBox.Image = bmp;
					MakeLabelReadable(ConfrenceChampUniform1ColorString, mConfChampUniform1Label);
				}
			}
		}
//		private string mConfrenceChampUniform2ColorString;

		public string ConfrenceChampUniform2ColorString
		{
			get { return mConfChampUniform2Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mConfChampUniform2Label.Text = value;
//					mConfrenceChampUniform2ColorString = value; 
					mConfChampUniform2Label.BackColor = c;
					Bitmap bmp = new Bitmap(mConfChampPictureBox.Image);
					SetBitmapColor( bmp, c, mConfChampUniform2);
					mConfChampPictureBox.Image = bmp;
					MakeLabelReadable(ConfrenceChampUniform2ColorString, mConfChampUniform2Label);
				}
			}
		}
//		private string mConfrenceChampUniform3ColorString;

		public string ConfrenceChampUniform3ColorString
		{
			get { return mConfChampUniform3Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mConfChampUniform3Label.Text = value;
//					mConfrenceChampUniform3ColorString = value; 
					mConfChampUniform3Label.BackColor = c;
					Bitmap bmp = new Bitmap(mConfChampPictureBox.Image);
					SetBitmapColor( bmp, c, mConfChampUniform3);
					mConfChampPictureBox.Image = bmp;
					MakeLabelReadable(ConfrenceChampUniform3ColorString, mConfChampUniform3Label);
				}
			}
		}
//		private string mConfrenceChampHelmetColorString;

		public string ConfrenceChampHelmetColorString
		{
			get { return mConfChampHelmetLabel.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mConfChampHelmetLabel.Text = value;
//					mConfrenceChampHelmetColorString = value; 
					mConfChampHelmetLabel.BackColor = c;
					Bitmap bmp = new Bitmap(mConfChampPictureBox.Image);
					SetBitmapColor( bmp, c, mConfChampHelmet);
					mConfChampPictureBox.Image = bmp;
					MakeLabelReadable(ConfrenceChampHelmetColorString, mConfChampHelmetLabel);
				}
			}
		}

//		private string mDivisionChampUniform1ColorString;

		public string DivisionChampUniform1ColorString
		{
			get { return mDivChampUniform1Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mDivChampUniform1Label.Text = value;
//					mDivisionChampUniform1ColorString = value; 
					mDivChampUniform1Label.BackColor = c;
					Bitmap bmp = new Bitmap(mDivChampPictureBox.Image);
					SetBitmapColor( bmp, c, mDivChampUniform1);
					mDivChampPictureBox.Image = bmp;
					MakeLabelReadable(DivisionChampUniform1ColorString, mDivChampUniform1Label);
				}
			}
		}
//		private string mDivisionChampUniform2ColorString;

		public string DivisionChampUniform2ColorString
		{
			get { return mDivChampUniform2Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mDivChampUniform2Label.Text = value;
//					mDivisionChampUniform2ColorString = value; 
					mDivChampUniform2Label.BackColor = c;
					Bitmap bmp = new Bitmap(mDivChampPictureBox.Image);
					SetBitmapColor( bmp, c, mDivChampUniform2);
					mDivChampPictureBox.Image = bmp;
					MakeLabelReadable(DivisionChampUniform2ColorString, mDivChampUniform2Label);
				}
			}
		}
//		private string mDivisionChampUniform3ColorString;

		public string DivisionChampUniform3ColorString
		{
			get { return mDivChampUniform3Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mDivChampUniform3Label.Text = value;
//					mDivisionChampUniform3ColorString = value; 
					mDivChampUniform3Label.BackColor = c;
					Bitmap bmp = new Bitmap(mDivChampPictureBox.Image);
					SetBitmapColor( bmp, c, mDivChampUniform3);
					mDivChampPictureBox.Image = bmp;
					MakeLabelReadable(DivisionChampUniform3ColorString, mDivChampUniform3Label);
				}
			}
		}
//		private string mDivisionChampHelmet1ColorString;

		public string DivisionChampHelmet1ColorString
		{
			get { return mDivChampHelmet1Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mDivChampHelmet1Label.Text = value;
//					mDivisionChampHelmet1ColorString = value; 
					mDivChampHelmet1Label.BackColor = c;
					Bitmap bmp = new Bitmap(mDivChampPictureBox.Image);
					SetBitmapColor( bmp, c, mDivChampHelmet1);
					mDivChampPictureBox.Image = bmp;
					MakeLabelReadable(DivisionChampHelmet1ColorString, mDivChampHelmet1Label);
				}
			}
		}
//		private string mDivisionChampHelmet2ColorString;

		public string DivisionChampHelmet2ColorString
		{
			get { return mDivChampHelmet2Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mDivChampHelmet2Label.Text = value;
//					mDivisionChampHelmet2ColorString = value; 
					mDivChampHelmet2Label.BackColor = c;
					Bitmap bmp = new Bitmap(mDivChampPictureBox.Image);
					SetBitmapColor( bmp, c, mDivChampHelmet2);
					mDivChampPictureBox.Image = bmp;
					MakeLabelReadable(DivisionChampHelmet2ColorString, mDivChampHelmet2Label);
				}
			}
		}

		public string HomeJerseyColorString
		{
			get { return mHomeJerseyLabel.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mHomeJerseyLabel.Text = value; 
					mHomeJerseyLabel.BackColor = c;
					MakeLabelReadable(mHomeJerseyLabel.Text, mHomeJerseyLabel);
					Bitmap bmp = new Bitmap(mHomePictureBox.Image);
					SetJerseyColor(bmp, c);
					mHomePictureBox.Image = bmp;
				}
			}
		}

		public string HomePantsColorString
		{
			get { return mHomePantsLabel.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mHomePantsLabel.Text = value; 
					mHomePantsLabel.BackColor = c;
					MakeLabelReadable(mHomePantsLabel.Text, mHomePantsLabel);
					Bitmap bmp = new Bitmap(mHomePictureBox.Image);
					SetPantsColor(bmp, c);
					mHomePictureBox.Image = bmp;
				}
			}
		}

		public string AwayJerseyColorString
		{
			get { return mAwayJerseyLabel.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mAwayJerseyLabel.Text = value; 
					mAwayJerseyLabel.BackColor = c;
					MakeLabelReadable(mAwayJerseyLabel.Text, mAwayJerseyLabel);
					Bitmap bmp = new Bitmap(mAwayPictureBox.Image);
					SetJerseyColor(bmp, c);
					mAwayPictureBox.Image = bmp;
				}
			}
		}


		public string AwayPantsColorString
		{
			get { return mAwayPantsLabel.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mAwayPantsLabel.Text = value; 
					mAwayPantsLabel.BackColor = c;
					MakeLabelReadable(mAwayPantsLabel.Text, mAwayPantsLabel);
					Bitmap bmp = new Bitmap(mAwayPictureBox.Image);
					SetPantsColor(bmp, c);
					mAwayPictureBox.Image = bmp;
				}
			}
		}

		public string Skin1ColorString
		{
			get { return mSkin1Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mSkin1Label.Text = value; 
					mSkin1Label.BackColor = c;
					MakeLabelReadable(mSkin1Label.Text, mSkin1Label);
					Bitmap bmp = new Bitmap(mHomePictureBox.Image);
					SetBitmapColor( bmp, c, mSkinData);
					mHomePictureBox.Image = bmp;
					MakeLabelReadable(Skin1ColorString, mSkin1Label);
				}
			}
		}

		public string Skin2ColorString
		{
			get { return mSkin2Label.Text; }
			set 
			{ 
				Color c = mColorForm2.GetColor(value);
				if( c != Color.Empty )
				{
					mSkin2Label.Text = value; 
					mSkin2Label.BackColor = c;
					MakeLabelReadable(mSkin2Label.Text, mSkin2Label);
					Bitmap bmp = new Bitmap(mAwayPictureBox.Image);
					SetBitmapColor( bmp, c, mSkinData);
					mAwayPictureBox.Image = bmp;
					MakeLabelReadable(Skin2ColorString, mSkin2Label);
				}
			}
		}

		private string mUnifromUsageString="";

		public string UnifromUsageString
		{
			get{ return mUnifromUsageString; }
			set{ mUnifromUsageString= value;}
		}

		public string Result;

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
				mColorForm2.Dispose();
				mHomeAwayUniformForm.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UniformEditForm));
            this.mHomePictureBox = new System.Windows.Forms.PictureBox();
            this.mAwayPictureBox = new System.Windows.Forms.PictureBox();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mScaleMenuItem = new System.Windows.Forms.MenuItem();
            this.mCloseMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.mHomeJerseyButton = new System.Windows.Forms.Button();
            this.mHomePantsButton = new System.Windows.Forms.Button();
            this.mAwayPantsButton = new System.Windows.Forms.Button();
            this.mAwayJerseyButton = new System.Windows.Forms.Button();
            this.mHomeJerseyLabel = new System.Windows.Forms.Label();
            this.mHomePantsLabel = new System.Windows.Forms.Label();
            this.mAwayPantsLabel = new System.Windows.Forms.Label();
            this.mAwayJerseyLabel = new System.Windows.Forms.Label();
            this.mDivChampPictureBox = new System.Windows.Forms.PictureBox();
            this.mConfChampPictureBox = new System.Windows.Forms.PictureBox();
            this.mDivChampUniform1Label = new System.Windows.Forms.Label();
            this.mDivChampUniform2Label = new System.Windows.Forms.Label();
            this.mDivChampUniform3Label = new System.Windows.Forms.Label();
            this.mDivChampHelmet1Label = new System.Windows.Forms.Label();
            this.mDivChampHelmet2Label = new System.Windows.Forms.Label();
            this.mConfChampHelmetLabel = new System.Windows.Forms.Label();
            this.mConfChampUniform3Label = new System.Windows.Forms.Label();
            this.mConfChampUniform2Label = new System.Windows.Forms.Label();
            this.mConfChampUniform1Label = new System.Windows.Forms.Label();
            this.mConfChampGroupBox = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.mDivChampGroupBox = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            this.mSkin2Label = new System.Windows.Forms.Label();
            this.mSkin1Label = new System.Windows.Forms.Label();
            this.mSkin2Button = new System.Windows.Forms.Button();
            this.mSkin1Button = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.mTeamsComboBox = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.mUniform1GroupBox = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.mHomePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mAwayPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mDivChampPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mConfChampPictureBox)).BeginInit();
            this.mConfChampGroupBox.SuspendLayout();
            this.mDivChampGroupBox.SuspendLayout();
            this.mUniform1GroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mHomePictureBox
            // 
            this.mHomePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mHomePictureBox.Image = ((System.Drawing.Image)(resources.GetObject("mHomePictureBox.Image")));
            this.mHomePictureBox.Location = new System.Drawing.Point(8, 16);
            this.mHomePictureBox.Name = "mHomePictureBox";
            this.mHomePictureBox.Size = new System.Drawing.Size(56, 88);
            this.mHomePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mHomePictureBox.TabIndex = 0;
            this.mHomePictureBox.TabStop = false;
            this.mHomePictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mHomePictureBox_MouseDown);
            // 
            // mAwayPictureBox
            // 
            this.mAwayPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mAwayPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("mAwayPictureBox.Image")));
            this.mAwayPictureBox.Location = new System.Drawing.Point(8, 16);
            this.mAwayPictureBox.Name = "mAwayPictureBox";
            this.mAwayPictureBox.Size = new System.Drawing.Size(56, 88);
            this.mAwayPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mAwayPictureBox.TabIndex = 1;
            this.mAwayPictureBox.TabStop = false;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mScaleMenuItem,
            this.mCloseMenuItem});
            this.menuItem1.Text = "&File";
            // 
            // mScaleMenuItem
            // 
            this.mScaleMenuItem.Index = 0;
            this.mScaleMenuItem.Text = "&Scale";
            this.mScaleMenuItem.Click += new System.EventHandler(this.mScaleMenuItem_Click);
            // 
            // mCloseMenuItem
            // 
            this.mCloseMenuItem.Index = 1;
            this.mCloseMenuItem.Text = "&Close";
            this.mCloseMenuItem.Click += new System.EventHandler(this.mCloseMenuItem_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem3});
            this.menuItem2.Text = "&How To";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 0;
            this.menuItem3.Text = "How to use";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // mHomeJerseyButton
            // 
            this.mHomeJerseyButton.Location = new System.Drawing.Point(8, 112);
            this.mHomeJerseyButton.Name = "mHomeJerseyButton";
            this.mHomeJerseyButton.Size = new System.Drawing.Size(56, 23);
            this.mHomeJerseyButton.TabIndex = 4;
            this.mHomeJerseyButton.Text = "Jersey";
            this.mHomeJerseyButton.Click += new System.EventHandler(this.mJerseyButton_Click);
            // 
            // mHomePantsButton
            // 
            this.mHomePantsButton.Location = new System.Drawing.Point(8, 144);
            this.mHomePantsButton.Name = "mHomePantsButton";
            this.mHomePantsButton.Size = new System.Drawing.Size(56, 23);
            this.mHomePantsButton.TabIndex = 5;
            this.mHomePantsButton.Text = "Pants";
            this.mHomePantsButton.Click += new System.EventHandler(this.mPantsButton_Click);
            // 
            // mAwayPantsButton
            // 
            this.mAwayPantsButton.Location = new System.Drawing.Point(8, 144);
            this.mAwayPantsButton.Name = "mAwayPantsButton";
            this.mAwayPantsButton.Size = new System.Drawing.Size(56, 23);
            this.mAwayPantsButton.TabIndex = 7;
            this.mAwayPantsButton.Text = "Pants";
            this.mAwayPantsButton.Click += new System.EventHandler(this.mPantsButton_Click);
            // 
            // mAwayJerseyButton
            // 
            this.mAwayJerseyButton.Location = new System.Drawing.Point(8, 112);
            this.mAwayJerseyButton.Name = "mAwayJerseyButton";
            this.mAwayJerseyButton.Size = new System.Drawing.Size(56, 23);
            this.mAwayJerseyButton.TabIndex = 6;
            this.mAwayJerseyButton.Text = "Jersey";
            this.mAwayJerseyButton.Click += new System.EventHandler(this.mJerseyButton_Click);
            // 
            // mHomeJerseyLabel
            // 
            this.mHomeJerseyLabel.Location = new System.Drawing.Point(64, 112);
            this.mHomeJerseyLabel.Name = "mHomeJerseyLabel";
            this.mHomeJerseyLabel.Size = new System.Drawing.Size(24, 23);
            this.mHomeJerseyLabel.TabIndex = 8;
            this.mHomeJerseyLabel.Text = "00";
            this.mHomeJerseyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mHomePantsLabel
            // 
            this.mHomePantsLabel.Location = new System.Drawing.Point(64, 144);
            this.mHomePantsLabel.Name = "mHomePantsLabel";
            this.mHomePantsLabel.Size = new System.Drawing.Size(24, 23);
            this.mHomePantsLabel.TabIndex = 9;
            this.mHomePantsLabel.Text = "00";
            this.mHomePantsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mAwayPantsLabel
            // 
            this.mAwayPantsLabel.Location = new System.Drawing.Point(64, 144);
            this.mAwayPantsLabel.Name = "mAwayPantsLabel";
            this.mAwayPantsLabel.Size = new System.Drawing.Size(24, 23);
            this.mAwayPantsLabel.TabIndex = 11;
            this.mAwayPantsLabel.Text = "00";
            this.mAwayPantsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mAwayJerseyLabel
            // 
            this.mAwayJerseyLabel.Location = new System.Drawing.Point(64, 112);
            this.mAwayJerseyLabel.Name = "mAwayJerseyLabel";
            this.mAwayJerseyLabel.Size = new System.Drawing.Size(24, 23);
            this.mAwayJerseyLabel.TabIndex = 10;
            this.mAwayJerseyLabel.Text = "00";
            this.mAwayJerseyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mDivChampPictureBox
            // 
            this.mDivChampPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mDivChampPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mDivChampPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("mDivChampPictureBox.Image")));
            this.mDivChampPictureBox.Location = new System.Drawing.Point(88, 16);
            this.mDivChampPictureBox.Name = "mDivChampPictureBox";
            this.mDivChampPictureBox.Size = new System.Drawing.Size(232, 136);
            this.mDivChampPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mDivChampPictureBox.TabIndex = 12;
            this.mDivChampPictureBox.TabStop = false;
            this.mDivChampPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mDivChampPictureBox_MouseDown);
            // 
            // mConfChampPictureBox
            // 
            this.mConfChampPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mConfChampPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mConfChampPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("mConfChampPictureBox.Image")));
            this.mConfChampPictureBox.Location = new System.Drawing.Point(120, 16);
            this.mConfChampPictureBox.Name = "mConfChampPictureBox";
            this.mConfChampPictureBox.Size = new System.Drawing.Size(200, 142);
            this.mConfChampPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mConfChampPictureBox.TabIndex = 13;
            this.mConfChampPictureBox.TabStop = false;
            this.mConfChampPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mConfChampPictureBox_MouseDown);
            // 
            // mDivChampUniform1Label
            // 
            this.mDivChampUniform1Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mDivChampUniform1Label.Location = new System.Drawing.Point(56, 16);
            this.mDivChampUniform1Label.Name = "mDivChampUniform1Label";
            this.mDivChampUniform1Label.Size = new System.Drawing.Size(24, 16);
            this.mDivChampUniform1Label.TabIndex = 14;
            this.mDivChampUniform1Label.Text = "00";
            this.mDivChampUniform1Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mDivChampUniform2Label
            // 
            this.mDivChampUniform2Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mDivChampUniform2Label.Location = new System.Drawing.Point(56, 40);
            this.mDivChampUniform2Label.Name = "mDivChampUniform2Label";
            this.mDivChampUniform2Label.Size = new System.Drawing.Size(24, 16);
            this.mDivChampUniform2Label.TabIndex = 15;
            this.mDivChampUniform2Label.Text = "00";
            this.mDivChampUniform2Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mDivChampUniform3Label
            // 
            this.mDivChampUniform3Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mDivChampUniform3Label.Location = new System.Drawing.Point(56, 64);
            this.mDivChampUniform3Label.Name = "mDivChampUniform3Label";
            this.mDivChampUniform3Label.Size = new System.Drawing.Size(24, 16);
            this.mDivChampUniform3Label.TabIndex = 16;
            this.mDivChampUniform3Label.Text = "00";
            this.mDivChampUniform3Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mDivChampHelmet1Label
            // 
            this.mDivChampHelmet1Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mDivChampHelmet1Label.Location = new System.Drawing.Point(56, 104);
            this.mDivChampHelmet1Label.Name = "mDivChampHelmet1Label";
            this.mDivChampHelmet1Label.Size = new System.Drawing.Size(24, 16);
            this.mDivChampHelmet1Label.TabIndex = 17;
            this.mDivChampHelmet1Label.Text = "00";
            this.mDivChampHelmet1Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mDivChampHelmet2Label
            // 
            this.mDivChampHelmet2Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mDivChampHelmet2Label.Location = new System.Drawing.Point(56, 128);
            this.mDivChampHelmet2Label.Name = "mDivChampHelmet2Label";
            this.mDivChampHelmet2Label.Size = new System.Drawing.Size(24, 16);
            this.mDivChampHelmet2Label.TabIndex = 18;
            this.mDivChampHelmet2Label.Text = "00";
            this.mDivChampHelmet2Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mConfChampHelmetLabel
            // 
            this.mConfChampHelmetLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mConfChampHelmetLabel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.mConfChampHelmetLabel.Location = new System.Drawing.Point(80, 112);
            this.mConfChampHelmetLabel.Name = "mConfChampHelmetLabel";
            this.mConfChampHelmetLabel.Size = new System.Drawing.Size(24, 16);
            this.mConfChampHelmetLabel.TabIndex = 22;
            this.mConfChampHelmetLabel.Text = "00";
            this.mConfChampHelmetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mConfChampHelmetLabel.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mConfChampUniform3Label
            // 
            this.mConfChampUniform3Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mConfChampUniform3Label.Location = new System.Drawing.Point(80, 72);
            this.mConfChampUniform3Label.Name = "mConfChampUniform3Label";
            this.mConfChampUniform3Label.Size = new System.Drawing.Size(24, 16);
            this.mConfChampUniform3Label.TabIndex = 21;
            this.mConfChampUniform3Label.Text = "00";
            this.mConfChampUniform3Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mConfChampUniform2Label
            // 
            this.mConfChampUniform2Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mConfChampUniform2Label.Location = new System.Drawing.Point(80, 48);
            this.mConfChampUniform2Label.Name = "mConfChampUniform2Label";
            this.mConfChampUniform2Label.Size = new System.Drawing.Size(24, 16);
            this.mConfChampUniform2Label.TabIndex = 20;
            this.mConfChampUniform2Label.Text = "00";
            this.mConfChampUniform2Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mConfChampUniform1Label
            // 
            this.mConfChampUniform1Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mConfChampUniform1Label.Location = new System.Drawing.Point(80, 24);
            this.mConfChampUniform1Label.Name = "mConfChampUniform1Label";
            this.mConfChampUniform1Label.Size = new System.Drawing.Size(24, 16);
            this.mConfChampUniform1Label.TabIndex = 19;
            this.mConfChampUniform1Label.Text = "00";
            this.mConfChampUniform1Label.Click += new System.EventHandler(this.mUniformLabel_Click);
            // 
            // mConfChampGroupBox
            // 
            this.mConfChampGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mConfChampGroupBox.Controls.Add(this.mConfChampHelmetLabel);
            this.mConfChampGroupBox.Controls.Add(this.mConfChampUniform3Label);
            this.mConfChampGroupBox.Controls.Add(this.mConfChampUniform2Label);
            this.mConfChampGroupBox.Controls.Add(this.mConfChampUniform1Label);
            this.mConfChampGroupBox.Controls.Add(this.mConfChampPictureBox);
            this.mConfChampGroupBox.Controls.Add(this.label9);
            this.mConfChampGroupBox.Controls.Add(this.label10);
            this.mConfChampGroupBox.Controls.Add(this.label12);
            this.mConfChampGroupBox.Controls.Add(this.label11);
            this.mConfChampGroupBox.Location = new System.Drawing.Point(208, 184);
            this.mConfChampGroupBox.Name = "mConfChampGroupBox";
            this.mConfChampGroupBox.Size = new System.Drawing.Size(328, 168);
            this.mConfChampGroupBox.TabIndex = 23;
            this.mConfChampGroupBox.TabStop = false;
            this.mConfChampGroupBox.Text = "Confrence Champ Colors";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(16, 72);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(40, 16);
            this.label9.TabIndex = 37;
            this.label9.Text = "Uni 3";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(16, 48);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(40, 16);
            this.label10.TabIndex = 36;
            this.label10.Text = "Uni 2";
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(16, 112);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(40, 16);
            this.label12.TabIndex = 38;
            this.label12.Text = "Helm 1";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(16, 24);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(40, 16);
            this.label11.TabIndex = 35;
            this.label11.Text = "Uni 1";
            // 
            // mDivChampGroupBox
            // 
            this.mDivChampGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mDivChampGroupBox.Controls.Add(this.label8);
            this.mDivChampGroupBox.Controls.Add(this.label6);
            this.mDivChampGroupBox.Controls.Add(this.label4);
            this.mDivChampGroupBox.Controls.Add(this.label3);
            this.mDivChampGroupBox.Controls.Add(this.mDivChampPictureBox);
            this.mDivChampGroupBox.Controls.Add(this.mDivChampUniform1Label);
            this.mDivChampGroupBox.Controls.Add(this.mDivChampUniform2Label);
            this.mDivChampGroupBox.Controls.Add(this.mDivChampUniform3Label);
            this.mDivChampGroupBox.Controls.Add(this.mDivChampHelmet1Label);
            this.mDivChampGroupBox.Controls.Add(this.mDivChampHelmet2Label);
            this.mDivChampGroupBox.Controls.Add(this.label7);
            this.mDivChampGroupBox.Location = new System.Drawing.Point(208, 8);
            this.mDivChampGroupBox.Name = "mDivChampGroupBox";
            this.mDivChampGroupBox.Size = new System.Drawing.Size(328, 168);
            this.mDivChampGroupBox.TabIndex = 24;
            this.mDivChampGroupBox.TabStop = false;
            this.mDivChampGroupBox.Text = "Division Champ Colors";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(8, 128);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 16);
            this.label8.TabIndex = 34;
            this.label8.Text = "Helm 2";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(8, 64);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 16);
            this.label6.TabIndex = 21;
            this.label6.Text = "Uni 3";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(8, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 20;
            this.label4.Text = "Uni 2";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 16);
            this.label3.TabIndex = 19;
            this.label3.Text = "Uni 1";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 104);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 16);
            this.label7.TabIndex = 33;
            this.label7.Text = "Helm 1";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(352, 376);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 25;
            this.button1.Text = "&OK";
            // 
            // mCancelButton
            // 
            this.mCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.mCancelButton.Location = new System.Drawing.Point(440, 376);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(75, 23);
            this.mCancelButton.TabIndex = 26;
            this.mCancelButton.Text = "&Cancel";
            // 
            // mSkin2Label
            // 
            this.mSkin2Label.Location = new System.Drawing.Point(64, 176);
            this.mSkin2Label.Name = "mSkin2Label";
            this.mSkin2Label.Size = new System.Drawing.Size(24, 23);
            this.mSkin2Label.TabIndex = 30;
            this.mSkin2Label.Text = "00";
            this.mSkin2Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mSkin1Label
            // 
            this.mSkin1Label.Location = new System.Drawing.Point(64, 176);
            this.mSkin1Label.Name = "mSkin1Label";
            this.mSkin1Label.Size = new System.Drawing.Size(24, 23);
            this.mSkin1Label.TabIndex = 29;
            this.mSkin1Label.Text = "00";
            this.mSkin1Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mSkin2Button
            // 
            this.mSkin2Button.Location = new System.Drawing.Point(8, 176);
            this.mSkin2Button.Name = "mSkin2Button";
            this.mSkin2Button.Size = new System.Drawing.Size(56, 23);
            this.mSkin2Button.TabIndex = 28;
            this.mSkin2Button.Text = "Skin 2";
            this.mSkin2Button.Click += new System.EventHandler(this.mSkin2Button_Click);
            // 
            // mSkin1Button
            // 
            this.mSkin1Button.Location = new System.Drawing.Point(8, 176);
            this.mSkin1Button.Name = "mSkin1Button";
            this.mSkin1Button.Size = new System.Drawing.Size(56, 23);
            this.mSkin1Button.TabIndex = 27;
            this.mSkin1Button.Text = "Skin 1";
            this.mSkin1Button.Click += new System.EventHandler(this.mSkin1Button_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 32;
            this.label5.Text = "Team";
            // 
            // mTeamsComboBox
            // 
            this.mTeamsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mTeamsComboBox.Items.AddRange(new object[] {
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
            this.mTeamsComboBox.Location = new System.Drawing.Point(56, 8);
            this.mTeamsComboBox.Name = "mTeamsComboBox";
            this.mTeamsComboBox.Size = new System.Drawing.Size(104, 21);
            this.mTeamsComboBox.TabIndex = 0;
            this.mTeamsComboBox.SelectedIndexChanged += new System.EventHandler(this.mTeamsComboBox_SelectedIndexChanged);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(8, 272);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(192, 96);
            this.button2.TabIndex = 33;
            this.button2.Text = "Edit Uniform Usage";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // mUniform1GroupBox
            // 
            this.mUniform1GroupBox.Controls.Add(this.mHomeJerseyButton);
            this.mUniform1GroupBox.Controls.Add(this.mHomeJerseyLabel);
            this.mUniform1GroupBox.Controls.Add(this.mSkin1Button);
            this.mUniform1GroupBox.Controls.Add(this.mHomePictureBox);
            this.mUniform1GroupBox.Controls.Add(this.mHomePantsButton);
            this.mUniform1GroupBox.Controls.Add(this.mHomePantsLabel);
            this.mUniform1GroupBox.Controls.Add(this.mSkin1Label);
            this.mUniform1GroupBox.Location = new System.Drawing.Point(8, 40);
            this.mUniform1GroupBox.Name = "mUniform1GroupBox";
            this.mUniform1GroupBox.Size = new System.Drawing.Size(96, 224);
            this.mUniform1GroupBox.TabIndex = 34;
            this.mUniform1GroupBox.TabStop = false;
            this.mUniform1GroupBox.Text = "Uniform 1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mAwayPantsButton);
            this.groupBox1.Controls.Add(this.mAwayPictureBox);
            this.groupBox1.Controls.Add(this.mSkin2Button);
            this.groupBox1.Controls.Add(this.mAwayPantsLabel);
            this.groupBox1.Controls.Add(this.mAwayJerseyLabel);
            this.groupBox1.Controls.Add(this.mSkin2Label);
            this.groupBox1.Controls.Add(this.mAwayJerseyButton);
            this.groupBox1.Location = new System.Drawing.Point(108, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(96, 224);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Uniform 2";
            // 
            // UniformEditForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.mCancelButton;
            this.ClientSize = new System.Drawing.Size(536, 402);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mUniform1GroupBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.mTeamsComboBox);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.mDivChampGroupBox);
            this.Controls.Add(this.mConfChampGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(552, 440);
            this.Menu = this.mainMenu1;
            this.MinimumSize = new System.Drawing.Size(552, 440);
            this.Name = "UniformEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Uniforms";
            ((System.ComponentModel.ISupportInitialize)(this.mHomePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mAwayPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mDivChampPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mConfChampPictureBox)).EndInit();
            this.mConfChampGroupBox.ResumeLayout(false);
            this.mDivChampGroupBox.ResumeLayout(false);
            this.mUniform1GroupBox.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void mCloseMenuItem_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void mScaleMenuItem_Click(object sender, System.EventArgs e)
		{
			string percent = null;
			float pct = -1;
			StringInputDlg dlg = new StringInputDlg("Scale Form", "Enter a percentage to scale the form by", "100");
			if( dlg.ShowDialog(this) == DialogResult.OK )
			{
				percent = dlg.getResult();
			}
			dlg.Dispose();
			if( percent != null )
			{
				try
				{
					pct =   float.Parse(percent);
				}
				catch( FormatException )
				{
					MessageBox.Show(this, "ERROR!!", "Error with value entered ");
				}
			}
			float scale = pct/(float)100.0;
			if( pct > 20 )
			{
				Font newFont = new Font(this.Font.FontFamily, this.Font.SizeInPoints * scale);
				this.Font = newFont;
				this.ApplyAutoScaling();
			}
		}

		private void SetJerseyColor(Bitmap bmp, Color jersey)
		{
			int x = 0;
			int y = 0;
			for(int i = 0; i< mJerseyColorPositions.Length; i = i + 2)
			{
				x = mJerseyColorPositions[i];
				y = mJerseyColorPositions[i+1];
				bmp.SetPixel(x, y, jersey);
			}
		}

		private void SetPantsColor(Bitmap bmp, Color jersey)
		{
			for(int i = 0; i< mPantsColorPositions.Length; i+=2)
			{
				bmp.SetPixel(mPantsColorPositions[i],mPantsColorPositions[i+1], jersey);
			}
		}

		private void SetBitmapColor( Bitmap bmp, Color c, int[] locations)
		{
			for(int i = 0; i< locations.Length; i+=2)
			{
				bmp.SetPixel(locations[i],locations[i+1], c);
			}
		}


		private void mJerseyButton_Click(object sender, System.EventArgs e)
		{
			PictureBox box = this.mHomePictureBox;
			Label lbl = this.mHomeJerseyLabel;
			if( sender == this.mAwayJerseyButton )
			{
				box = this.mAwayPictureBox;
				lbl = this.mAwayJerseyLabel;
			}
			SetUniformColor("Select Jersey Color",box, lbl, PantsJersey.Jersey);
		}

		private enum PantsJersey
		{
			Pants=0,
			Jersey
		}

		private void SetUniformColor(string formText, PictureBox box, Label lbl, PantsJersey choice)
		{
			ColorForm2 form = new ColorForm2();
			form.CurrentColorString = lbl.Text;
			form.Text = formText;
			if( form.ShowDialog()  == DialogResult.OK )
			{
				Bitmap bmp = new Bitmap(box.Image);
				if( choice == PantsJersey.Jersey)
					SetJerseyColor(bmp, form.CurrentColor);
				else
					SetPantsColor(bmp, form.CurrentColor);
				box.Image = bmp;
				lbl.Text = form.CurrentColorString;
				lbl.BackColor = form.CurrentColor;
				//makes label readable
				if( form.CurrentColorString.StartsWith("20") || form.CurrentColorString.StartsWith("3"))
					lbl.ForeColor = Color.Black;
				else
					lbl.ForeColor = Color.White;
				if( form.CurrentColorString.EndsWith("F") || form.CurrentColorString.EndsWith("E"))
					lbl.ForeColor = Color.White;

				ReplaceColorData();
			}
			form.Dispose();
		}

		private void mPantsButton_Click(object sender, System.EventArgs e)
		{
			PictureBox box = this.mHomePictureBox;
			Label lbl = this.mHomePantsLabel;
			if( sender == this.mAwayPantsButton)
			{
				box = this.mAwayPictureBox;
				lbl = this.mAwayPantsLabel;
			}
			SetUniformColor("Select Pants Color",box, lbl, PantsJersey.Pants);
		}

		private void mDivChampPictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
//			this.Result = MapPixels(mDivChampPictureBox, e.X, e.Y);
		}
		
		private void mConfChampPictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
//			this.Result = MapPixels(mConfChampPictureBox, e.X, e.Y);
		}

		/// <summary>
		/// Generate an array of pixel locations to color.
		/// usage : string g = MapPixels(mDivChampPictureBox, e.X, e.Y);
		/// </summary>
		/// <param name="box"></param>
		/// <param name="pixX"></param>
		/// <param name="pixY"></param>
		/// <returns></returns>
		private string MapPixels( PictureBox box, int pixX, int pixY)
		{
			Bitmap b2 = new Bitmap(box.Image);
			Color tmp;
			Color c = b2.GetPixel( pixX, pixY);
			StringBuilder sb = new StringBuilder(500);
			sb.Append("{ ");
			int i =0;
			for (int Xcount = 0; Xcount < b2.Width; Xcount++)
			{
				for (int Ycount = 0; Ycount < b2.Height; Ycount++)
				{
					tmp = b2.GetPixel(Xcount,Ycount);
					if( tmp == c )
					{
						sb.Append(Xcount);
						sb.Append(",");
						sb.Append(Ycount);
						sb.Append(",");
						i++;
						if( i % 10 == 0)
							sb.Append("\r\n");
					}
				}
			}
			sb.Append(" };");
			string text = sb.ToString();
			return text;
		}

		private void mUniformLabel_Click(object sender, System.EventArgs e)
		{
			Label lab = sender as Label;
			if( lab != null )
			{
				SetChampColors(lab);
			}
		}

		private void SetChampColors(Label lab)
		{
			ColorForm2 form = new ColorForm2();
			form.CurrentColorString = lab.Text;
			form.Text = "Choose Color";
			
			if( form.ShowDialog(this) == DialogResult.OK)
			{
				if( lab == this.mConfChampUniform1Label)
				{
					ConfrenceChampUniform1ColorString = form.CurrentColorString;
				}
				else if( lab == this.mConfChampUniform2Label)
				{
					ConfrenceChampUniform2ColorString = form.CurrentColorString;
				}
				else if( lab == this.mConfChampUniform3Label)
				{
					ConfrenceChampUniform3ColorString = form.CurrentColorString;
				}
				else if( lab == this.mConfChampHelmetLabel)
				{
					ConfrenceChampHelmetColorString = form.CurrentColorString;
				}
				else if( lab == this.mDivChampUniform1Label)
				{
					DivisionChampUniform1ColorString = form.CurrentColorString;
				}
				else if( lab == this.mDivChampUniform2Label)
				{
					DivisionChampUniform2ColorString = form.CurrentColorString;
				}
				else if( lab == this.mDivChampUniform3Label)
				{
					DivisionChampUniform3ColorString = form.CurrentColorString;
				}
				else if( lab == this.mDivChampHelmet1Label)
				{
					DivisionChampHelmet1ColorString = form.CurrentColorString;
				}
				else if( lab == this.mDivChampHelmet2Label)
				{
					DivisionChampHelmet2ColorString= form.CurrentColorString;
				}
				ReplaceColorData();
			}

			form.Dispose();
		}

		private void MakeLabelReadable( string colorString, Label lab )
		{
			if( colorString.StartsWith("20") || colorString.StartsWith("3"))
				lab.ForeColor = Color.Black;
			else
				lab.ForeColor = Color.White;
			if( colorString.EndsWith("F") || colorString.EndsWith("E"))
				lab.ForeColor = Color.White;
		}
		
		private void mTeamsComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if( this.Visible)
			{
				SetCurrentTeamColors();
			}
		}

		private void mHomePictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//this.Result = MapPixels(mHomePictureBox, e.X, e.Y);
		}

		private void mSkin1Button_Click(object sender, System.EventArgs e)
		{
			ColorForm2 form = new ColorForm2();
			form.CurrentColorString = mSkin1Label.Text;
			if( form.ShowDialog(this) == DialogResult.OK )
			{
				this.Skin1ColorString = form.CurrentColorString;
				ReplaceColorData();
			}
			form.Dispose();
		}

		private void mSkin2Button_Click(object sender, System.EventArgs e)
		{
			ColorForm2 form = new ColorForm2();
			form.CurrentColorString = mSkin2Label.Text;
			if( form.ShowDialog(this) == DialogResult.OK )
			{
				this.Skin2ColorString = form.CurrentColorString;
				ReplaceColorData();
			}
			form.Dispose();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			if( Data.IndexOf("texans") > -1 )
			{
				CxRomTeamHomeAwayUniformForm form = new CxRomTeamHomeAwayUniformForm();
				form.StringValue = UnifromUsageString;
				if( form.ShowDialog(this) == DialogResult.OK)
				{
					UnifromUsageString = form.StringValue;
					ReplaceColorData();
				}
			}
			else
			{
				mHomeAwayUniformForm.StringValue = UnifromUsageString;
				if( mHomeAwayUniformForm.ShowDialog(this) == DialogResult.OK)
				{
					UnifromUsageString = mHomeAwayUniformForm.StringValue;
					ReplaceColorData();
				}
			}
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			string howTo=
				@"To change in-game uniform colors, simply click the jersey, pants or skin buttons.

To change the Confrence Champ or Division Champ colors click on a colored label
next to the picture.";
			MessageBox.Show(this,howTo, "How To use",
				MessageBoxButtons.OK,MessageBoxIcon.Information);
		}

		#region Bitmap Image mappings
		/// <summary>
		/// <code>
		/// for(int i = 0; i&lt mJerseyColorPositions; i+=2)
		/// {
		///    bmp.SetPixel(i,i+1, color);
		/// }
		/// </code>
		/// </summary>
		private int[] mJerseyColorPositions =
			{
					6,12,6,13,6,14,6,15,6,16,6,17,6,27,6,28,6,29,6,30,6,
				31,6,32,6,36,6,37,6,38,6,39,6,40,6,41,7,12,7,13,7,
				14,7,15,7,16,7,17,7,27,7,28,7,29,7,30,7,31,7,32,7,
				36,7,37,7,38,7,39,7,40,7,41,8,12,8,13,8,14,8,15,8,
				16,8,17,8,27,8,28,8,29,8,30,8,31,8,32,8,36,8,37,8,
				38,8,39,8,40,8,41,9,12,9,13,9,14,10,12,10,13,10,14,
				11,12,11,13,11,14,12,12,12,13,12,14,12,15,12,16,12,
				17,13,12,13,13,13,14,13,15,13,16,13,17,14,12,14,13,
				14,14,14,15,14,16,14,17,15,12,15,13,15,14,15,15,15,
				16,15,17,15,18,15,19,15,20,16,12,16,13,16,14,16,15,
				16,16,16,17,16,18,16,19,16,20,17,12,17,13,17,14,17,
				15,17,16,17,17,17,18,17,19,17,20,18,33,18,34,18,35,
				19,33,19,34,19,35,20,33,20,34,20,35,21,15,21,16,21,
				17,21,33,21,34,21,35,21,36,21,37,21,38,22,15,22,16,
				22,17,22,33,22,34,22,35,22,36,22,37,22,38,23,15,23,
				16,23,17,23,33,23,34,23,35,23,36,23,37,23,38,24,15,
				24,16,24,17,25,15,25,16,25,17,26,15,26,16,26,17 };

		private int[] mPantsColorPositions = 
			{
					3,18,3,19,3,20,3,27,3,28,3,29,3,42,3,43,3,44,4,18,4,
				19,4,20,4,27,4,28,4,29,4,42,4,43,4,44,5,18,5,19,5,20,
				5,27,5,28,5,29,5,42,5,43,5,44,6,18,6,19,6,20,6,21,6,
				22,6,23,6,24,6,25,6,26,6,33,6,34,6,35,6,42,6,43,6,44,
				7,18,7,19,7,20,7,21,7,22,7,23,7,24,7,25,7,26,7,33,7,
				34,7,35,7,42,7,43,7,44,8,18,8,19,8,20,8,21,8,22,8,23,
				8,24,8,25,8,26,8,33,8,34,8,35,8,42,8,43,8,44,9,24,9,
				25,9,26,9,27,9,28,9,29,9,30,9,31,9,32,9,33,9,34,9,35,
				9,39,9,40,9,41,9,42,9,43,9,44,10,24,10,25,10,26,10,
				27,10,28,10,29,10,30,10,31,10,32,10,33,10,34,10,35,10,
				39,10,40,10,41,10,42,10,43,10,44,11,24,11,25,11,26,11,
				27,11,28,11,29,11,30,11,31,11,32,11,33,11,34,11,35,11,
				39,11,40,11,41,11,42,11,43,11,44,12,21,12,22,12,23,12,
				24,12,25,12,26,12,30,12,31,12,32,12,42,12,43,12,44,13,
				21,13,22,13,23,13,24,13,25,13,26,13,30,13,31,13,32,13,
				42,13,43,13,44,14,21,14,22,14,23,14,24,14,25,14,26,14,
				30,14,31,14,32,14,42,14,43,14,44,15,6,15,7,15,8,15,9,
				15,10,15,11,15,21,15,22,15,23,16,6,16,7,16,8,16,9,16,
				10,16,11,16,21,16,22,16,23,17,6,17,7,17,8,17,9,17,10,
				17,11,17,21,17,22,17,23,18,3,18,4,18,5,18,6,18,7,18,8,
				18,9,18,10,18,11,18,30,18,31,18,32,18,36,18,37,18,38,
				18,39,18,40,18,41,19,3,19,4,19,5,19,6,19,7,19,8,19,9,
				19,10,19,11,19,30,19,31,19,32,19,36,19,37,19,38,19,39,
				19,40,19,41,20,3,20,4,20,5,20,6,20,7,20,8,20,9,20,10,
				20,11,20,30,20,31,20,32,20,36,20,37,20,38,20,39,20,40,
				20,41,21,3,21,4,21,5,21,6,21,7,21,8,21,12,21,13,21,14,
				21,21,21,22,21,23,21,27,21,28,21,29,21,30,21,31,21,32,
				21,39,21,40,21,41,22,3,22,4,22,5,22,6,22,7,22,8,22,12,
				22,13,22,14,22,21,22,22,22,23,22,27,22,28,22,29,22,30,
				22,31,22,32,22,39,22,40,22,41,23,3,23,4,23,5,23,6,23,7,
				23,8,23,12,23,13,23,14,23,21,23,22,23,23,23,27,23,28,
				23,29,23,30,23,31,23,32,23,39,23,40,23,41,24,6,24,7,24,
				8,24,12,24,13,24,14,24,18,24,19,24,20,24,39,24,40,24,
				41,25,6,25,7,25,8,25,12,25,13,25,14,25,18,25,19,25,20,
				25,39,25,40,25,41,26,6,26,7,26,8,26,12,26,13,26,14,26,
				18,26,19,26,20,26,39,26,40,26,41,27,39,27,40,27,41,28,
				39,28,40,28,41 };

		private int[] mSkinData =
		{
				9,15,9,16,9,17,9,18,9,19,9,20,9,21,9,22,9,23,10,15,
				10,16,10,17,10,18,10,19,10,20,10,21,10,22,10,23,11,15,11,16,
				11,17,11,18,11,19,11,20,11,21,11,22,11,23,12,18,12,19,12,20,
				12,27,12,28,12,29,13,18,13,19,13,20,13,27,13,28,13,29,14,18,
				14,19,14,20,14,27,14,28,14,29,15,24,15,25,15,26,15,27,15,28,
				15,29,15,30,15,31,15,32,16,24,16,25,16,26,16,27,16,28,16,29,
				16,30,16,31,16,32,17,24,17,25,17,26,17,27,17,28,17,29,17,30,
				17,31,17,32,18,12,18,13,18,14,18,15,18,16,18,17,18,27,18,28,
				18,29,19,12,19,13,19,14,19,15,19,16,19,17,19,27,19,28,19,29,
				20,12,20,13,20,14,20,15,20,16,20,17,20,27,20,28,20,29,21,9,
				21,10,21,11,21,18,21,19,21,20,22,9,22,10,22,11,22,18,22,19,
				22,20,23,9,23,10,23,11,23,18,23,19,23,20,24,21,24,22,24,23,
				24,24,24,25,24,26,25,21,25,22,25,23,25,24,25,25,25,26,26,21,
				26,22,26,23,26,24,26,25,26,26,27,21,27,22,27,23,27,24,27,25,
				27,26,28,21,28,22,28,23,28,24,28,25,28,26, };

			private int[] mDivChampUniform1 =
			{   3,69,3,70,3,71,4,70,4,71,4,72,4,73,4,74,4,75,5,69,6,
			    69,6,70,6,71,40,51,40,52,45,37,45,38,46,38,46,39,48,
				37,69,21,69,23,70,21,70,22,70,23,71,22,110,21,110,22,
				111,21,111,22,111,23,112,22,112,23,113,23,114,22,114,
				23,115,21,115,22,116,21,116,22,117,21,168,51,168,52,
				173,37,173,38,174,38,174,39,176,37,177,106,177,107,178,
				107,178,108,178,114,179,103,179,104,179,115,180,103,
				180,104,180,105,180,106,180,115,180,116,181,101,181,
				102,181,103,181,104,181,105,181,106,181,107,181,116,
				182,101,182,102,182,103,182,104,182,105,182,106,182,
				107,182,108,182,111,182,113,182,116,183,101,183,102,
				183,103,183,104,183,105,183,106,183,107,183,108,183,
				109,183,110,183,111,183,112,183,113,183,116,197,21,
				197,23,198,21,198,22,198,23,199,22,216,5,216,7,216,13,
				217,7,217,12,217,13,218,5,218,6,218,8,218,9,218,10,219,
				5,219,6,219,7,219,8,219,9,219,10,220,6,220,7,220,8,220,
				9,221,20,222,20, };

		private int[] mDivChampUniform2 =
			{
				6,116,7,116,8,115,8,116,9,61,9,62,9,63,9,64,9,65,9,114,
				9,115,9,116,10,60,10,61,10,62,10,63,10,64,10,65,10,66,10,105,
				10,114,10,115,10,116,11,60,11,61,11,62,11,63,11,64,11,65,11,66,
				11,67,11,104,11,105,11,106,11,115,12,61,12,62,12,63,12,64,12,65,
				12,66,12,67,12,68,12,104,12,105,12,106,12,107,12,116,13,62,13,63,
				13,64,13,65,13,66,13,67,13,68,13,106,13,107,13,108,13,109,14,63,
				14,64,14,65,14,66,14,67,14,68,14,69,14,72,14,104,14,105,15,64,
				15,65,15,66,15,67,15,68,15,69,15,70,15,72,15,73,15,102,15,105,
				15,106,15,107,15,108,15,109,15,110,15,111,16,65,16,66,16,67,16,68,
				16,69,16,70,16,71,16,72,16,79,16,80,16,84,16,85,16,92,16,104,
				16,106,16,107,16,108,16,109,16,110,16,111,16,112,16,113,16,114,16,115,
				16,116,17,66,17,67,17,68,17,69,17,70,17,71,17,79,17,80,17,84,
				17,85,17,86,17,92,17,105,17,107,17,108,17,109,17,110,17,111,17,112,
				17,113,17,114,17,115,18,62,18,67,18,68,18,69,18,70,18,74,18,85,
				18,86,18,91,18,92,18,105,18,106,18,109,18,110,18,111,18,112,18,113,
				18,114,19,63,19,64,19,68,19,69,19,70,19,73,19,74,19,85,19,86,
				19,91,19,103,19,104,19,106,19,107,19,108,19,111,19,112,19,113,19,114,
				20,64,20,65,20,66,20,69,20,70,20,73,20,74,20,81,20,85,20,104,
				20,105,20,108,20,109,20,110,21,64,21,65,21,66,21,67,21,68,21,69,
				21,70,21,74,21,75,21,76,21,80,21,81,21,84,21,85,21,106,21,107,
				21,111,21,112,22,64,22,65,22,66,22,67,22,68,22,69,22,70,22,78,
				22,79,22,80,22,84,22,108,22,109,23,63,23,64,23,65,23,66,23,67,
				23,68,23,69,23,70,23,71,23,72,23,76,23,77,23,78,23,79,23,83,
				23,84,23,110,23,111,24,61,24,62,24,63,24,64,24,65,24,66,24,67,
				24,68,24,69,24,70,24,71,24,72,24,73,24,74,24,75,24,76,24,77,
				24,78,24,79,24,80,24,81,24,82,24,83,24,84,25,61,25,62,25,63,
				25,64,25,65,25,66,25,67,25,68,25,69,25,70,25,76,25,77,25,78,
				25,79,25,80,25,81,25,82,25,83,26,61,26,62,26,63,26,64,26,65,
				26,66,26,67,26,68,26,69,26,75,26,76,26,77,26,78,26,79,26,80,
				26,81,26,82,26,89,27,61,27,62,27,63,27,64,27,65,27,66,27,67,
				27,68,27,69,27,73,27,74,27,75,27,76,27,77,27,78,27,87,27,88,
				27,89,28,61,28,62,28,63,28,64,28,65,28,66,28,67,28,68,28,73,
				28,74,28,75,28,76,28,86,28,87,28,88,29,61,29,62,29,63,29,64,
				29,65,29,66,29,67,29,68,29,80,29,81,29,85,29,86,29,87,30,60,
				30,61,30,62,30,63,30,64,30,65,30,66,30,67,30,68,30,78,30,79,
				30,80,30,85,30,86,30,87,31,60,31,61,31,62,31,63,31,64,31,65,
				31,66,31,67,31,68,31,69,31,70,31,71,31,72,31,73,31,74,31,75,
				31,76,31,77,31,78,32,61,32,62,32,63,32,64,32,65,32,66,32,67,
				32,68,32,69,32,70,32,71,32,72,32,73,32,74,32,75,32,76,32,77,
				33,60,33,61,33,66,33,67,33,68,33,69,33,70,33,71,33,72,33,73,
				33,74,33,75,34,59,34,69,34,70,34,71,34,72,34,73,35,57,35,58,
				35,59,35,60,35,61,36,54,36,55,36,56,36,57,36,58,36,59,36,62,
				37,56,37,57,37,58,37,59,37,63,38,58,49,69,49,70,49,71,49,72,
				49,73,50,68,50,69,50,70,50,71,50,72,50,73,50,74,51,68,51,69,
				51,70,51,71,51,72,51,73,51,74,51,75,52,69,52,70,52,71,52,72,
				52,73,52,74,52,75,52,76,53,70,53,71,53,72,53,73,53,74,53,75,
				53,76,54,71,54,72,54,73,54,74,54,75,54,76,55,72,55,73,55,74,
				55,75,55,76,56,73,56,74,56,75,56,76,56,77,56,78,56,79,56,80,
				56,81,56,82,56,83,56,84,56,85,56,86,56,93,56,100,57,74,57,75,
				57,76,57,77,57,78,57,79,57,80,57,81,57,82,57,83,57,84,57,85,
				57,86,57,87,57,93,57,94,57,100,58,70,58,71,58,74,58,75,58,76,
				58,77,58,78,58,79,58,80,58,81,58,82,58,83,58,84,58,85,58,86,
				58,87,58,88,58,92,58,93,58,94,58,99,58,100,59,71,59,72,59,75,
				59,76,59,77,59,78,59,79,59,80,59,81,59,82,59,83,59,84,59,85,
				59,86,59,87,59,88,59,89,59,92,59,93,59,94,59,99,60,72,60,73,
				60,75,60,76,60,77,60,78,60,79,60,80,60,81,60,82,60,83,60,84,
				60,85,60,86,60,87,60,88,60,89,60,90,60,91,60,92,60,93,61,72,
				61,73,61,75,61,76,61,77,61,78,61,79,61,80,61,81,61,82,61,83,
				61,84,61,85,61,86,61,87,61,88,61,89,61,90,61,91,61,92,61,93,
				62,72,62,73,62,74,62,75,62,76,62,77,62,78,62,79,62,80,62,81,
				62,82,62,83,62,84,62,85,62,86,62,87,62,88,62,89,62,90,62,91,
				62,92,63,71,63,72,63,73,63,74,63,75,63,76,63,77,63,78,63,79,
				63,80,63,81,63,82,63,83,63,84,63,85,63,86,63,87,63,88,63,89,
				63,90,63,91,64,69,64,70,64,71,64,72,64,73,64,74,64,75,64,76,
				64,77,64,78,64,79,64,80,64,81,64,82,64,83,64,84,64,85,64,86,
				64,87,64,88,64,89,64,90,64,91,64,92,64,112,64,115,65,69,65,70,
				65,71,65,72,65,73,65,74,65,75,65,76,65,77,65,78,65,81,65,82,
				65,83,65,84,65,85,65,86,65,87,65,88,65,89,65,90,65,91,66,69,
				66,70,66,71,66,72,66,73,66,74,66,75,66,76,66,77,66,78,66,81,
				66,82,66,83,66,84,66,85,66,86,66,87,66,88,66,97,66,113,67,69,
				67,70,67,71,67,72,67,73,67,74,67,75,67,76,67,77,67,95,67,96,
				67,97,67,114,68,69,68,70,68,71,68,72,68,73,68,74,68,75,68,76,
				68,77,68,94,68,95,68,96,68,116,69,69,69,70,69,71,69,72,69,73,
				69,74,69,75,69,76,69,77,69,89,69,93,69,94,69,95,70,68,70,69,
				70,70,70,71,70,72,70,73,70,74,70,75,70,76,70,77,70,78,70,79,
				70,80,70,81,70,82,70,83,70,84,70,85,70,86,70,87,70,88,70,93,
				70,94,70,95,71,68,71,69,71,70,71,71,71,72,71,73,71,74,71,75,
				71,76,71,77,71,78,71,79,71,80,71,81,71,82,71,83,71,84,71,85,
				71,86,72,67,72,68,72,69,72,70,72,71,72,72,72,73,72,74,72,75,
				72,76,72,77,72,78,72,79,72,80,72,81,72,82,72,83,72,84,72,85,
				72,86,72,87,72,91,72,92,73,66,73,67,73,68,73,69,73,70,73,71,
				73,72,73,73,73,74,73,75,73,76,73,77,73,78,73,79,73,80,73,81,
				73,82,73,83,73,84,73,85,73,86,73,87,73,90,73,91,74,57,74,66,
				74,67,74,68,74,69,74,70,74,71,74,72,74,73,74,74,74,75,74,76,
				74,77,74,78,74,79,74,80,74,81,74,82,74,83,74,84,74,85,74,86,
				74,90,75,56,75,57,75,58,75,67,75,70,75,71,75,72,75,73,75,74,
				75,75,75,76,75,77,75,78,75,79,75,80,75,81,75,82,75,83,75,84,
				75,85,75,86,76,56,76,57,76,58,76,59,76,68,76,73,76,74,76,75,
				76,76,76,77,76,78,76,79,76,80,76,81,76,82,76,83,76,84,76,85,
				77,58,77,59,77,60,77,61,77,74,77,75,77,76,77,77,77,78,77,79,
				77,80,77,81,77,82,77,83,77,84,78,56,78,57,78,79,78,80,78,81,
				78,82,79,54,79,57,79,58,79,59,79,60,79,61,79,62,79,63,80,56,
				80,58,80,59,80,60,80,61,80,62,80,63,80,64,80,65,80,66,80,67,
				80,68,81,57,81,59,81,60,81,61,81,62,81,63,81,64,81,65,81,66,
				81,67,82,57,82,58,82,61,82,62,82,63,82,64,82,65,82,66,82,116,
				83,55,83,56,83,58,83,59,83,60,83,63,83,64,83,65,83,66,83,116,
				84,56,84,57,84,60,84,61,84,62,85,58,85,59,85,63,85,64,86,60,
				86,61,87,62,87,63,102,116,103,116,104,115,104,116,105,114,105,115,105,116,
				106,105,106,114,106,115,106,116,107,104,107,105,107,106,107,115,108,104,108,105,
				108,106,108,107,108,116,109,106,109,107,109,108,109,109,110,14,110,104,110,105,
				111,13,111,102,111,105,111,106,111,107,111,108,111,109,111,110,111,111,112,14,
				112,104,112,106,112,107,112,108,112,109,112,110,112,111,112,112,112,113,112,114,
				112,115,112,116,113,14,113,105,113,107,113,108,113,109,113,110,113,111,113,112,
				113,113,113,114,113,115,114,14,114,105,114,106,114,109,114,110,114,111,114,112,
				114,113,114,114,115,17,115,18,115,103,115,104,115,106,115,107,115,108,115,111,
				115,112,115,113,115,114,116,20,116,104,116,105,116,108,116,109,116,110,117,16,
				117,17,117,19,117,20,117,106,117,107,117,111,117,112,118,19,118,20,118,108,
				118,109,119,15,119,18,119,110,119,111,120,77,120,84,121,77,121,78,121,84,
				122,77,122,78,122,83,122,84,123,77,123,78,123,83,124,77,125,77,137,61,
				137,62,137,63,137,64,137,65,138,60,138,61,138,62,138,63,138,64,138,65,
				138,66,139,60,139,61,139,62,139,63,139,64,139,65,139,66,139,67,140,61,
				140,62,140,63,140,64,140,65,140,66,140,67,140,68,141,62,141,63,141,64,
				141,65,141,66,141,67,141,68,142,63,142,64,142,65,142,66,142,67,142,68,
				142,69,142,72,143,64,143,65,143,66,143,67,143,68,143,69,143,70,143,72,
				143,73,144,65,144,66,144,67,144,68,144,69,144,70,144,71,144,72,144,79,
				144,80,144,84,144,85,144,92,145,66,145,67,145,68,145,69,145,70,145,71,
				145,79,145,80,145,84,145,85,145,86,145,92,146,62,146,67,146,68,146,69,
				146,70,146,74,146,85,146,86,146,91,146,92,147,63,147,64,147,68,147,69,
				147,70,147,73,147,74,147,85,147,86,147,91,148,64,148,65,148,66,148,69,
				148,70,148,73,148,74,148,81,148,85,149,64,149,65,149,66,149,67,149,68,
				149,69,149,70,149,74,149,75,149,76,149,80,149,81,149,84,149,85,150,64,
				150,65,150,66,150,67,150,68,150,69,150,70,150,78,150,79,150,80,150,84,
				151,63,151,64,151,65,151,66,151,67,151,68,151,69,151,70,151,71,151,72,
				151,76,151,77,151,78,151,79,151,83,151,84,152,61,152,62,152,63,152,64,
				152,65,152,66,152,67,152,68,152,69,152,70,152,71,152,72,152,73,152,74,
				152,75,152,76,152,77,152,78,152,79,152,80,152,81,152,82,152,83,152,84,
				153,61,153,62,153,63,153,64,153,65,153,66,153,67,153,68,153,69,153,70,
				153,73,153,74,153,75,153,76,153,77,153,78,153,79,153,80,153,81,153,82,
				153,83,154,61,154,62,154,63,154,64,154,65,154,66,154,67,154,68,154,69,
				154,70,154,73,154,74,154,75,154,76,154,77,154,78,154,79,154,80,154,89,
				155,61,155,62,155,63,155,64,155,65,155,66,155,67,155,68,155,69,155,87,
				155,88,155,89,156,61,156,62,156,63,156,64,156,65,156,66,156,67,156,68,
				156,69,156,86,156,87,156,88,157,61,157,62,157,63,157,64,157,65,157,66,
				157,67,157,68,157,69,157,81,157,85,157,86,157,87,158,60,158,61,158,62,
				158,63,158,64,158,65,158,66,158,67,158,68,158,69,158,70,158,71,158,72,
				158,73,158,74,158,75,158,76,158,77,158,78,158,79,158,80,158,85,158,86,
				158,87,159,60,159,61,159,62,159,63,159,64,159,65,159,66,159,67,159,68,
				159,69,159,70,159,71,159,72,159,73,159,74,159,75,159,76,159,77,159,78,
				160,61,160,62,160,63,160,64,160,65,160,66,160,67,160,68,160,69,160,70,
				160,71,160,72,160,73,160,74,160,75,160,76,160,77,161,60,161,61,161,66,
				161,67,161,68,161,69,161,70,161,71,161,72,161,73,161,74,161,75,162,59,
				162,69,162,70,162,71,162,72,162,73,163,57,163,58,163,59,163,60,163,61,
				164,54,164,55,164,56,164,57,164,58,164,59,164,62,165,56,165,57,165,58,
				165,59,165,63,166,58,167,93,168,88,168,91,169,94,170,89,170,93,171,90,
				172,92,172,94,172,95,177,85,177,86,177,87,177,88,177,89,178,84,178,85,
				178,86,178,87,178,88,178,89,178,90,179,84,179,85,179,86,179,87,179,88,
				179,89,179,90,179,91,180,85,180,86,180,87,180,88,180,89,180,90,180,91,
				180,92,181,86,181,87,181,88,181,89,181,90,181,91,181,92,182,87,182,88,
				182,89,182,90,182,91,182,92,182,93,182,96,183,88,183,89,183,90,183,91,
				183,92,183,93,183,94,183,96,183,97,184,89,184,90,184,91,184,92,184,93,
				184,94,184,95,184,96,184,109,184,116,185,90,185,91,185,92,185,93,185,94,
				185,95,185,109,185,110,185,116,186,86,186,87,186,90,186,91,186,92,186,93,
				186,94,186,98,186,109,186,110,186,115,186,116,187,87,187,88,187,91,187,92,
				187,93,187,94,187,97,187,98,187,103,187,106,187,109,187,110,187,115,188,88,
				188,89,188,91,188,92,188,93,188,94,188,97,188,98,188,103,188,104,188,106,
				188,109,189,88,189,89,189,91,189,92,189,93,189,94,189,98,189,99,189,100,
				189,104,189,105,189,106,189,109,190,88,190,89,190,90,190,91,190,92,190,93,
				190,94,190,108,191,87,191,88,191,89,191,90,191,91,191,92,191,93,191,94,
				191,95,191,96,191,100,191,102,191,107,191,108,192,85,192,86,192,87,192,88,
				192,89,192,90,192,91,192,92,192,93,192,94,192,95,192,96,192,97,192,98,
				192,99,192,100,192,101,192,102,192,103,192,104,192,105,192,106,192,107,192,108,
				193,85,193,86,193,87,193,88,193,89,193,90,193,91,193,92,193,93,193,94,
				193,97,193,98,193,99,193,100,193,101,193,102,193,103,193,104,193,105,193,106,
				193,107,194,85,194,86,194,87,194,88,194,89,194,90,194,91,194,92,194,93,
				194,94,194,97,194,98,194,99,194,100,194,101,194,102,194,103,194,104,194,113,
				195,85,195,86,195,87,195,88,195,89,195,90,195,91,195,92,195,93,195,111,
				195,112,195,113,196,85,196,86,196,87,196,88,196,89,196,90,196,91,196,92,
				196,93,196,110,196,111,196,112,197,85,197,86,197,87,197,88,197,89,197,90,
				197,91,197,92,197,93,197,105,197,109,197,110,197,111,198,84,198,85,198,86,
				198,87,198,88,198,89,198,90,198,91,198,92,198,93,198,94,198,95,198,96,
				198,97,198,98,198,99,198,100,198,101,198,102,198,103,198,104,198,109,198,110,
				198,111,199,84,199,85,199,86,199,87,199,88,199,89,199,90,199,91,199,92,
				199,93,199,94,199,95,199,96,199,97,199,98,199,99,199,100,199,101,199,102,
				200,83,200,84,200,85,200,86,200,87,200,88,200,89,200,90,200,91,200,92,
				200,93,200,94,200,95,200,96,200,97,200,98,200,99,200,100,200,101,200,102,
				200,103,200,107,200,108,201,82,201,83,201,84,201,85,201,86,201,87,201,88,
				201,89,201,90,201,91,201,92,201,93,201,94,201,95,201,96,201,97,201,98,
				201,99,201,100,201,101,201,102,201,103,201,106,201,107,202,73,202,82,202,83,
				202,84,202,85,202,86,202,87,202,88,202,89,202,90,202,91,202,92,202,93,
				202,94,202,95,202,96,202,97,202,98,202,99,202,100,202,101,202,102,202,106,
				203,72,203,73,203,74,203,83,203,86,203,87,203,88,203,89,203,90,203,91,
				203,92,203,93,203,94,203,95,203,96,203,97,203,98,203,99,203,100,203,101,
				203,102,204,72,204,73,204,74,204,75,204,84,204,89,204,90,204,91,204,92,
				204,93,204,94,204,95,204,96,204,97,204,98,204,99,204,100,204,101,205,74,
				205,75,205,76,205,77,205,90,205,91,205,92,205,93,205,94,205,95,205,96,
				205,97,205,98,205,99,205,100,206,72,206,73,206,95,206,96,206,97,206,98,
				207,70,207,73,207,74,207,75,207,76,207,77,207,78,207,79,208,72,208,74,
				208,75,208,76,208,77,208,78,208,79,208,80,208,81,208,82,208,83,208,84,
				209,73,209,75,209,76,209,77,209,78,209,79,209,80,209,81,209,82,209,83,
				210,73,210,74,210,77,210,78,210,79,210,80,210,81,210,82,211,71,211,72,
				211,74,211,75,211,76,211,79,211,80,211,81,211,82,212,8,212,11,212,72,
				212,73,212,76,212,77,212,78,213,9,213,74,213,75,213,79,213,80,214,7,
				214,76,214,77,215,5,215,78,215,79,216,0,216,4,217,1,217,2,217,4,
				218,4,218,46,219,47,219,48,220,44,220,45,220,46,220,47,220,48,221,39,
				221,49,222,39,222,40,222,41,222,42,223,39,223,40,223,41,223,42,223,43,
				223,44,223,45,223,46,223,47,223,48,223,49,223,50,224,39,224,40,224,41,
				224,42,224,43,224,44,224,45,224,46,224,47,224,48,224,49,224,50,225,40,
				225,41,225,42,225,43,225,44,225,45,225,46,225,47,225,48,225,49,225,50,
				225,51,226,40,226,41,226,42,226,43,226,44,226,45,226,46,226,47,227,42,
				227,43,227,44,227,45,228,45, };

		private int[] mDivChampUniform3 =
			{
					0,50,0,51,0,52,0,53,0,54,0,55,0,56,0,57,0,58,0,59,
				0,60,0,62,0,63,0,66,1,49,1,50,1,51,1,52,1,53,1,54,
				1,55,1,56,1,57,1,58,1,59,1,60,1,62,1,63,1,64,1,66,
				1,67,2,49,2,50,2,51,2,52,2,53,2,54,2,55,2,56,2,57,
				2,58,2,59,2,60,2,63,2,64,2,65,2,67,2,68,3,49,3,50,
				3,51,3,52,3,53,3,54,3,55,3,56,3,57,3,58,3,59,3,60,
				3,61,3,63,3,64,3,65,3,66,4,49,4,50,4,51,4,52,4,53,
				4,54,4,55,4,56,4,57,4,58,4,59,4,60,4,61,4,62,4,64,
				4,65,4,66,4,67,5,50,5,51,5,52,5,53,5,54,5,55,5,56,
				5,57,5,58,5,59,5,60,5,62,5,66,5,67,5,68,6,50,6,51,
				6,52,6,53,6,54,6,55,6,56,6,57,6,58,6,59,6,60,6,63,
				6,64,6,67,6,68,7,51,7,52,7,53,7,54,7,55,7,56,7,57,
				7,58,7,59,7,60,7,61,7,62,7,63,7,115,8,53,8,54,8,55,
				8,56,8,57,8,58,8,59,8,60,8,61,8,62,8,63,8,64,8,65,
				8,66,8,67,8,68,8,69,8,70,8,110,8,111,8,112,8,114,9,53,
				9,54,9,55,9,56,9,57,9,58,9,59,9,60,9,66,9,67,9,68,
				9,69,9,70,9,71,9,72,9,82,9,83,9,93,9,110,9,111,9,112,
				9,113,10,54,10,55,10,56,10,57,10,58,10,59,10,67,10,68,10,69,
				10,70,10,71,10,72,10,73,10,74,10,83,10,84,10,90,10,94,10,103,
				10,104,10,110,10,111,10,112,10,113,11,56,11,57,11,58,11,59,11,68,
				11,69,11,70,11,71,11,72,11,73,11,74,11,75,11,79,11,80,11,91,
				11,95,11,103,11,111,11,112,11,113,11,114,11,116,12,54,12,58,12,59,
				12,60,12,69,12,70,12,71,12,72,12,73,12,74,12,75,12,76,12,79,
				12,80,12,81,12,82,12,91,12,92,12,95,12,96,12,112,12,113,12,114,
				12,115,13,55,13,59,13,60,13,61,13,69,13,70,13,71,13,72,13,73,
				13,74,13,75,13,76,13,77,13,78,13,79,13,80,13,81,13,82,13,83,
				13,92,13,93,13,94,13,96,13,115,13,116,14,60,14,61,14,62,14,70,
				14,71,14,73,14,74,14,75,14,76,14,77,14,78,14,79,14,80,14,81,
				14,82,14,83,14,84,14,87,14,89,14,92,14,93,14,94,14,95,14,96,
				14,97,14,102,14,103,15,61,15,62,15,63,15,71,15,74,15,75,15,76,
				15,77,15,78,15,79,15,80,15,81,15,82,15,83,15,84,15,85,15,86,
				15,87,15,88,15,89,15,92,15,93,15,94,15,95,15,96,15,97,15,98,
				15,103,16,54,16,63,16,64,16,75,16,76,16,77,16,78,16,81,16,82,
				16,83,16,86,16,87,16,88,16,93,16,94,16,96,16,97,16,98,16,103,
				17,53,17,54,17,55,17,60,17,61,17,64,17,65,17,76,17,77,17,78,
				17,87,17,88,17,91,17,93,17,96,17,97,17,98,17,99,17,102,17,104,
				17,116,18,55,18,61,18,66,18,75,18,76,18,77,18,78,18,87,18,90,
				18,97,18,98,18,99,18,102,18,103,18,115,18,116,19,62,19,67,19,75,
				19,76,19,77,19,87,19,90,19,92,19,97,19,99,19,100,19,115,19,116,
				20,62,20,63,20,68,20,75,20,76,20,80,20,86,20,89,20,90,20,91,
				20,92,20,100,20,103,20,114,21,61,21,62,21,63,21,79,21,86,21,89,
				21,91,21,104,21,105,21,110,21,113,21,114,22,62,22,63,22,85,22,86,
				22,88,22,90,22,91,22,106,22,107,23,62,23,85,23,90,23,108,23,109,
				23,112,24,89,24,90,25,88,25,89,25,90,26,87,26,88,26,90,27,90,
				28,89,29,88,29,89,30,81,30,83,30,84,30,88,31,59,31,79,31,80,
				31,83,31,84,31,85,31,86,32,55,32,56,32,57,32,58,32,59,32,60,
				32,78,32,79,32,82,32,83,32,84,33,53,33,54,33,55,33,56,33,57,
				33,58,33,59,33,62,33,63,33,64,33,65,33,76,33,77,33,78,33,82,
				33,83,34,58,34,64,34,65,34,66,34,67,34,68,34,74,34,75,34,76,
				34,81,34,82,34,83,35,55,35,56,35,67,35,68,35,69,35,70,35,71,
				35,72,35,73,35,74,35,80,35,81,35,82,36,53,36,60,36,61,36,63,
				36,69,36,70,36,71,36,80,37,53,37,54,37,55,37,60,37,61,37,62,
				38,55,38,56,38,57,38,59,38,60,38,61,38,62,38,76,39,53,39,54,
				39,59,39,60,39,61,39,62,39,63,39,67,39,75,40,53,40,54,40,55,
				40,56,40,57,40,58,41,55,41,56,41,57,41,58,41,59,41,60,42,58,
				42,59,42,60,42,69,42,70,42,71,43,60,43,68,43,69,43,70,43,71,
				44,67,44,68,44,69,44,70,44,71,44,72,44,73,44,77,44,78,45,65,
				45,66,45,67,45,68,45,69,45,70,45,71,45,72,45,73,45,74,45,75,
				45,78,45,79,46,64,46,65,46,66,46,67,46,68,46,69,46,70,46,71,
				46,72,46,73,46,74,46,75,46,76,46,77,46,78,46,79,46,80,46,81,
				47,63,47,64,47,65,47,66,47,67,47,68,47,69,47,70,47,71,47,72,
				47,73,47,74,47,75,47,76,47,77,47,78,47,79,47,80,47,81,47,82,
				47,83,48,61,48,62,48,63,48,64,48,65,48,66,48,67,48,68,48,69,
				48,70,48,71,48,72,48,73,48,74,48,75,48,76,48,77,48,78,48,79,
				48,80,48,81,48,82,48,83,48,84,49,61,49,62,49,63,49,64,49,65,
				49,66,49,67,49,68,49,74,49,75,49,76,49,77,49,78,49,79,49,80,
				49,81,49,82,49,83,49,84,49,90,49,91,49,101,50,62,50,63,50,64,
				50,65,50,66,50,67,50,75,50,76,50,77,50,78,50,79,50,80,50,81,
				50,82,50,83,50,84,50,91,50,92,50,98,50,102,51,64,51,65,51,66,
				51,67,51,76,51,77,51,78,51,79,51,80,51,81,51,82,51,83,51,84,
				51,87,51,88,51,99,51,103,52,62,52,66,52,67,52,68,52,77,52,78,
				52,79,52,80,52,81,52,82,52,83,52,84,52,87,52,88,52,89,52,90,
				52,99,52,100,52,103,52,104,53,63,53,67,53,68,53,69,53,77,53,78,
				53,79,53,80,53,81,53,82,53,83,53,84,53,85,53,86,53,87,53,88,
				53,89,53,90,53,91,53,100,53,101,53,102,53,104,54,68,54,69,54,70,
				54,77,54,78,54,79,54,80,54,81,54,82,54,83,54,84,54,85,54,86,
				54,87,54,88,54,89,54,90,54,91,54,92,54,95,54,97,54,100,54,101,
				54,102,54,103,54,104,54,105,55,69,55,70,55,71,55,77,55,78,55,79,
				55,80,55,81,55,82,55,83,55,84,55,85,55,86,55,87,55,88,55,89,
				55,90,55,91,55,92,55,93,55,94,55,95,55,96,55,97,55,100,55,101,
				55,102,55,103,55,104,55,105,55,106,56,59,56,60,56,62,56,71,56,72,
				56,87,56,88,56,89,56,90,56,91,56,92,56,94,56,95,56,96,56,101,
				56,102,56,104,56,105,56,106,57,53,57,55,57,61,57,62,57,63,57,68,
				57,69,57,72,57,73,57,88,57,89,57,90,57,91,57,92,57,95,57,96,
				57,99,57,101,57,104,57,105,57,106,57,107,58,63,58,69,58,73,58,89,
				58,90,58,91,58,95,58,98,58,105,58,106,58,107,59,54,59,70,59,74,
				59,90,59,91,59,95,59,98,59,100,59,105,59,107,59,108,59,116,60,53,
				60,70,60,71,60,74,60,94,60,97,60,98,60,99,60,100,60,108,60,115,
				60,116,61,69,61,70,61,71,61,94,61,97,61,99,61,113,61,114,61,115,
				62,70,62,71,62,93,62,94,62,96,62,98,62,99,62,112,62,113,62,114,
				62,115,63,70,63,92,63,93,63,98,63,112,63,113,63,115,64,14,64,15,
				64,16,64,17,64,97,64,98,64,113,64,116,65,13,65,14,65,96,65,97,
				65,98,65,113,65,116,66,13,66,16,66,17,66,18,66,20,66,95,66,96,
				66,98,66,110,66,111,66,114,67,14,67,15,67,16,67,17,67,18,67,19,
				67,20,67,98,67,108,67,109,67,110,67,111,67,115,67,116,68,14,68,15,
				68,18,68,19,68,20,68,92,68,97,68,107,68,108,68,109,68,110,69,13,
				69,20,69,92,69,96,69,97,69,107,69,108,69,109,70,14,70,15,70,16,
				70,18,70,20,70,89,70,91,70,92,70,96,70,106,70,107,70,108,70,109,
				70,110,70,111,70,112,70,113,71,17,71,18,71,67,71,87,71,88,71,91,
				71,93,71,94,71,106,71,107,71,108,71,109,71,110,71,111,71,112,71,113,
				71,114,71,115,72,62,72,63,72,64,72,66,72,88,72,106,72,107,72,108,
				72,109,72,110,72,111,72,112,72,113,72,114,72,115,72,116,73,62,73,63,
				73,64,73,65,73,105,73,106,73,107,73,108,73,109,73,110,73,111,73,112,
				73,113,73,114,73,115,73,116,74,55,74,56,74,62,74,63,74,64,74,65,
				74,89,74,91,74,105,74,106,74,107,74,108,74,109,74,110,74,111,74,112,
				74,113,74,114,74,115,74,116,75,55,75,63,75,64,75,65,75,66,75,68,
				75,69,75,91,75,105,75,106,75,107,75,108,75,109,75,110,75,111,75,112,
				75,113,75,114,75,115,75,116,76,64,76,65,76,66,76,67,76,69,76,70,
				76,71,76,72,76,105,76,106,76,107,76,108,76,109,76,110,76,111,76,112,
				76,113,76,114,76,115,76,116,77,67,77,68,77,69,77,70,77,71,77,72,
				77,73,77,106,77,107,77,108,77,109,77,110,77,111,77,112,77,113,77,114,
				77,115,77,116,78,54,78,55,78,71,78,72,78,73,78,74,78,75,78,76,
				78,77,78,78,78,83,78,84,78,106,78,107,78,108,78,109,78,110,78,111,
				78,112,78,113,78,114,78,115,78,116,79,55,79,72,79,73,79,74,79,75,
				79,76,79,77,79,78,79,79,79,80,79,107,79,108,79,109,79,110,79,111,
				79,112,79,113,79,114,79,115,79,116,80,55,80,69,80,70,80,71,80,72,
				80,73,80,74,80,75,80,109,80,110,80,111,80,112,80,113,80,114,80,115,
				80,116,81,54,81,56,81,68,81,69,81,70,81,71,81,72,81,73,81,109,
				81,110,81,111,81,112,81,113,81,114,81,115,81,116,82,54,82,55,82,67,
				82,68,82,69,82,70,82,110,82,111,82,112,82,113,82,114,82,115,83,67,
				83,68,83,69,83,112,83,113,83,114,83,115,84,55,84,66,84,110,84,114,
				84,115,84,116,85,56,85,57,85,62,85,65,85,66,85,111,85,115,85,116,
				86,58,86,59,86,116,87,60,87,61,87,64,88,107,88,108,88,110,89,101,
				89,103,89,109,89,110,89,111,89,116,90,111,91,102,92,101,103,115,104,110,
				104,111,104,112,104,114,105,110,105,111,105,112,105,113,106,103,106,104,106,110,
				106,111,106,112,106,113,107,103,107,111,107,112,107,113,107,114,107,116,108,112,
				108,113,108,114,108,115,109,115,109,116,110,15,110,16,110,17,110,18,110,19,
				110,102,110,103,111,15,111,16,111,17,111,18,111,19,111,20,111,103,112,13,
				112,15,112,16,112,20,112,103,113,13,113,16,113,19,113,82,113,83,113,85,
				113,102,113,104,113,116,114,13,114,16,114,17,114,18,114,83,114,84,114,86,
				114,102,114,103,114,115,114,116,115,14,115,79,115,80,115,87,115,115,115,116,
				116,14,116,15,116,79,116,80,116,81,116,82,116,87,116,88,116,103,116,114,
				117,15,117,77,117,78,117,79,117,80,117,81,117,82,117,83,117,85,117,86,
				117,88,117,104,117,105,117,110,117,113,117,114,118,77,118,78,118,79,118,80,
				118,81,118,82,118,83,118,84,118,85,118,86,118,87,118,88,118,89,118,106,
				118,107,119,14,119,16,119,77,119,78,119,79,119,80,119,81,119,82,119,83,
				119,84,119,85,119,86,119,87,119,88,119,89,119,90,119,108,119,109,119,112,
				120,64,120,78,120,79,120,80,120,85,120,86,120,88,120,89,120,90,121,66,
				121,69,121,79,121,80,121,83,121,85,121,88,121,89,121,90,121,91,122,70,
				122,79,122,82,122,89,122,90,122,91,123,67,123,71,123,79,123,82,123,84,
				123,89,123,91,123,92,124,67,124,71,124,72,124,78,124,81,124,82,124,83,
				124,84,124,92,125,67,125,69,125,70,125,72,125,78,125,81,125,83,126,66,
				126,67,126,69,126,70,126,71,126,72,126,73,126,77,126,78,126,80,126,82,
				126,83,127,66,127,69,127,70,127,71,127,72,127,73,127,74,127,77,127,82,
				128,65,128,66,128,67,128,69,128,70,128,72,128,73,128,74,129,64,129,65,
				129,69,129,72,129,73,129,74,129,75,130,63,130,64,130,66,130,73,130,74,
				130,75,131,54,131,61,131,62,131,65,131,73,131,75,131,76,132,53,132,55,
				132,56,132,57,132,61,132,63,132,64,132,76,133,53,133,54,133,57,133,58,
				133,59,133,60,133,61,133,62,133,63,133,64,133,65,133,67,134,54,134,55,
				134,56,134,60,134,61,134,62,134,63,134,64,134,65,134,66,134,67,135,55,
				135,56,135,57,135,58,135,59,135,60,135,63,135,64,135,65,135,66,135,67,
				135,68,136,53,136,54,136,55,136,56,136,57,136,58,136,59,136,60,136,61,
				136,62,136,63,136,64,136,65,136,66,136,67,136,68,136,69,136,70,137,53,
				137,54,137,55,137,56,137,57,137,58,137,59,137,60,137,66,137,67,137,68,
				137,69,137,70,137,71,137,72,137,82,137,83,137,93,138,54,138,55,138,56,
				138,57,138,58,138,59,138,67,138,68,138,69,138,70,138,71,138,72,138,73,
				138,74,138,83,138,84,138,90,138,94,139,56,139,57,139,58,139,59,139,68,
				139,69,139,70,139,71,139,72,139,73,139,74,139,75,139,79,139,80,139,91,
				139,95,140,54,140,58,140,59,140,60,140,69,140,70,140,71,140,72,140,73,
				140,74,140,75,140,76,140,79,140,80,140,81,140,82,140,91,140,92,140,95,
				140,96,141,55,141,59,141,60,141,61,141,69,141,70,141,71,141,72,141,73,
				141,74,141,75,141,76,141,77,141,78,141,79,141,80,141,81,141,82,141,83,
				141,92,141,93,141,94,141,96,142,60,142,61,142,62,142,70,142,71,142,73,
				142,74,142,75,142,76,142,77,142,78,142,79,142,80,142,81,142,82,142,83,
				142,84,142,87,142,89,142,92,142,93,142,94,142,95,142,96,142,97,143,61,
				143,62,143,63,143,71,143,74,143,75,143,76,143,77,143,78,143,79,143,80,
				143,81,143,82,143,83,143,84,143,85,143,86,143,87,143,88,143,89,143,92,
				143,93,143,94,143,95,143,96,143,97,143,98,144,54,144,63,144,64,144,75,
				144,76,144,77,144,78,144,81,144,82,144,83,144,86,144,87,144,88,144,93,
				144,94,144,96,144,97,144,98,145,53,145,54,145,55,145,60,145,61,145,64,
				145,65,145,76,145,77,145,78,145,87,145,88,145,91,145,93,145,96,145,97,
				145,98,145,99,146,55,146,61,146,66,146,75,146,76,146,77,146,78,146,87,
				146,90,146,97,146,98,146,99,147,62,147,67,147,75,147,76,147,77,147,87,
				147,90,147,92,147,97,147,99,147,100,148,62,148,63,148,68,148,75,148,76,
				148,80,148,86,148,89,148,90,148,91,148,92,148,100,149,61,149,62,149,63,
				149,79,149,86,149,89,149,91,150,62,150,63,150,85,150,86,150,88,150,90,
				150,91,151,62,151,85,151,90,152,89,152,90,153,88,153,89,153,90,154,87,
				154,88,154,90,155,90,156,84,156,89,157,84,157,88,157,89,158,81,158,83,
				158,84,158,88,159,59,159,79,159,80,159,83,159,85,159,86,160,55,160,56,
				160,57,160,58,160,59,160,60,160,78,160,79,160,82,160,83,160,84,161,53,
				161,54,161,55,161,56,161,57,161,58,161,59,161,62,161,63,161,64,161,65,
				161,76,161,77,161,78,161,82,161,83,162,58,162,64,162,65,162,66,162,67,
				162,68,162,74,162,75,162,76,162,81,162,82,162,83,163,55,163,56,163,67,
				163,68,163,69,163,70,163,71,163,72,163,73,163,74,163,80,163,81,163,82,
				163,92,163,94,163,96,163,97,163,98,164,53,164,60,164,61,164,63,164,69,
				164,70,164,71,164,80,164,91,164,92,164,93,164,95,164,99,164,100,165,53,
				165,54,165,55,165,60,165,61,165,62,165,89,165,90,165,91,165,93,165,95,
				165,97,165,100,166,55,166,56,166,57,166,59,166,60,166,61,166,62,166,76,
				166,88,166,89,166,90,166,91,166,93,166,95,166,98,166,100,167,53,167,54,
				167,59,167,60,167,61,167,62,167,63,167,67,167,75,167,88,167,89,167,91,
				167,94,167,96,167,97,167,98,167,99,167,100,168,53,168,54,168,55,168,56,
				168,57,168,58,168,61,168,62,168,63,168,64,168,65,168,66,168,67,168,68,
				168,89,168,92,168,94,168,95,168,97,168,99,168,100,169,55,169,56,169,57,
				169,58,169,59,169,60,169,62,169,63,169,64,169,65,169,66,169,89,169,92,
				169,95,169,96,169,97,169,99,170,58,170,59,170,60,170,61,170,62,170,64,
				170,65,170,86,170,87,170,90,170,96,171,60,171,61,171,62,171,63,171,64,
				171,65,171,84,171,85,171,86,171,87,171,91,171,92,171,94,171,95,171,96,
				172,61,172,62,172,63,172,83,172,84,172,85,172,86,172,93,173,62,173,63,
				173,81,173,82,173,83,173,84,173,85,174,80,174,81,174,82,174,83,174,84,
				174,85,174,86,174,87,174,88,174,89,174,94,175,79,175,80,175,81,175,82,
				175,83,175,84,175,85,175,86,175,87,175,88,175,89,175,90,175,91,175,93,
				175,94,175,95,175,97,176,77,176,78,176,79,176,80,176,81,176,82,176,83,
				176,84,176,85,176,86,176,87,176,88,176,89,176,90,176,91,176,92,176,93,
				176,94,177,77,177,78,177,79,177,80,177,81,177,82,177,83,177,84,177,90,
				177,91,177,92,177,93,177,94,177,95,177,96,178,78,178,79,178,80,178,81,
				178,82,178,83,178,91,178,92,178,93,178,94,178,95,178,96,178,97,178,98,
				179,80,179,81,179,82,179,83,179,92,179,93,179,94,179,95,179,96,179,97,
				179,98,179,99,180,78,180,82,180,83,180,84,180,93,180,94,180,95,180,96,
				180,97,180,98,180,99,180,100,181,79,181,83,181,84,181,85,181,93,181,94,
				181,95,181,96,181,97,181,98,181,99,181,100,182,84,182,85,182,86,182,94,
				182,95,182,97,182,98,182,99,182,100,183,85,183,86,183,87,183,95,183,98,
				183,99,183,100,184,75,184,76,184,78,184,87,184,88,184,99,184,100,184,101,
				184,102,184,103,184,104,184,105,184,106,184,107,184,108,184,110,184,111,184,112,
				185,69,185,71,185,77,185,78,185,79,185,84,185,85,185,88,185,89,185,100,
				185,101,185,102,185,103,185,104,185,108,185,111,185,112,185,115,186,79,186,85,
				186,89,186,99,186,100,186,101,186,102,186,103,186,104,186,111,186,114,187,70,
				187,86,187,90,187,99,187,100,187,104,187,105,187,111,187,114,187,116,188,69,
				188,86,188,87,188,90,188,99,188,100,188,105,188,110,188,113,188,114,188,115,
				188,116,189,85,189,86,189,87,189,110,189,113,189,115,190,86,190,87,190,109,
				190,110,190,112,190,114,190,115,191,86,191,101,191,109,191,114,192,14,192,15,
				192,16,192,17,192,113,192,114,193,13,193,14,193,112,193,113,193,114,194,13,
				194,16,194,17,194,18,194,20,194,111,194,112,194,114,195,14,195,15,195,16,
				195,17,195,18,195,19,195,20,195,114,196,14,196,15,196,18,196,19,196,20,
				196,108,196,113,197,13,197,20,197,108,197,112,197,113,198,14,198,15,198,16,
				198,18,198,20,198,105,198,107,198,108,198,112,199,17,199,18,199,83,199,103,
				199,104,199,107,199,109,199,110,200,78,200,79,200,80,200,82,200,104,201,78,
				201,79,201,80,201,81,202,71,202,72,202,78,202,79,202,80,202,81,202,105,
				202,107,203,71,203,79,203,80,203,81,203,82,203,84,203,85,203,107,204,80,
				204,81,204,82,204,83,204,85,204,86,204,87,204,88,205,83,205,84,205,85,
				205,86,205,87,205,88,205,89,206,70,206,71,206,87,206,88,206,89,206,90,
				206,91,206,92,206,93,206,94,206,99,206,100,207,71,207,88,207,89,207,90,
				207,91,207,92,207,93,207,94,207,95,207,96,208,71,208,85,208,86,208,87,
				208,88,208,89,208,90,208,91,209,70,209,72,209,84,209,85,209,86,209,87,
				209,88,209,89,210,70,210,71,210,83,210,84,210,85,210,86,211,83,211,84,
				211,85,212,71,212,82,213,11,213,12,213,72,213,73,213,78,213,81,213,82,
				214,10,214,74,214,75,215,8,215,76,215,77,215,80,217,0,217,41,217,43,
				217,44,217,45,218,2,218,40,218,41,218,42,218,45,219,41,219,42,219,43,
				219,44,220,38,221,37,221,48,222,43,224,51,224,52,226,48,226,49,226,50,
				227,46,227,47,227,48,228,43,228,44,228,46, };

		private int[] mDivChampHelmet2 =
			{
					0,26,0,27,0,30,0,31,0,32,0,33,0,90,0,92,0,93,0,94,
				0,95,0,96,0,97,0,98,0,99,1,30,1,31,1,32,1,33,1,34,
				1,90,1,93,1,94,1,95,1,96,1,97,1,98,1,99,2,29,2,32,
				2,33,2,34,2,90,2,95,2,96,2,97,2,98,3,31,3,34,4,36,
				4,91,4,98,5,14,5,36,5,98,6,15,6,30,6,31,6,36,7,16,
				7,19,7,28,7,29,7,32,7,33,7,35,7,94,8,26,8,27,8,32,
				9,24,9,25,9,30,9,33,10,21,10,27,10,28,11,23,15,48,15,49,
				16,51,16,52,17,37,17,38,17,45,17,47,18,36,18,37,18,38,18,44,
				19,35,19,36,19,37,19,38,19,39,19,42,19,43,19,44,19,46,20,35,
				20,36,20,37,20,38,20,39,20,40,20,41,20,42,20,43,20,45,21,34,
				21,35,21,36,21,37,21,38,21,39,21,40,21,41,21,42,21,43,22,34,
				22,36,22,37,22,38,22,39,22,40,22,41,22,42,22,43,23,34,23,36,
				23,37,23,38,23,39,23,40,23,41,23,42,23,43,24,34,24,36,24,37,
				24,38,24,39,24,40,24,41,24,42,24,43,25,34,25,37,25,38,25,39,
				25,40,25,41,25,42,25,43,26,34,26,39,26,40,26,41,26,42,28,35,
				28,42,29,42,31,38,32,39,32,40,33,41,33,42,33,43,33,44,57,45,
				57,46,58,44,58,45,58,46,58,52,59,43,59,44,59,45,59,46,59,47,
				59,50,59,51,59,52,60,43,60,44,60,45,60,46,60,47,60,48,60,49,
				60,50,60,51,61,42,61,43,61,44,61,45,61,46,61,47,61,48,61,49,
				61,50,61,51,62,42,62,44,62,45,62,46,62,47,62,48,62,49,62,50,
				62,51,63,42,63,44,63,45,63,46,63,47,63,48,63,49,63,50,63,51,
				64,42,64,44,64,45,64,46,64,47,64,48,64,49,64,50,64,51,65,42,
				65,45,65,46,65,47,65,48,65,49,65,50,65,51,66,42,66,47,66,48,
				66,49,66,50,68,43,68,50,69,50,71,46,72,47,72,48,73,49,73,50,
				73,51,73,52,89,93,89,94,90,92,90,93,90,94,90,100,91,91,91,92,
				91,93,91,94,91,95,91,98,91,99,91,100,92,91,92,92,92,93,92,94,
				92,95,92,96,92,97,92,98,92,99,93,90,93,91,93,92,93,93,93,94,
				93,95,93,96,93,97,93,98,93,99,94,90,94,92,94,93,94,94,94,95,
				94,96,94,97,94,98,94,99,95,90,95,92,95,93,95,94,95,95,95,96,
				95,97,95,98,95,99,96,90,96,92,96,93,96,94,96,95,96,96,96,97,
				96,98,96,99,97,90,97,93,97,94,97,95,97,96,97,97,97,98,97,99,
				98,90,98,95,98,96,98,97,98,98,100,91,100,98,101,98,103,94,121,29,
				121,30,122,28,122,29,122,30,122,36,123,27,123,28,123,29,123,30,123,31,
				123,34,123,35,123,36,124,27,124,28,124,29,124,30,124,31,124,32,124,33,
				124,34,124,35,125,26,125,27,125,28,125,29,125,30,125,31,125,32,125,33,
				125,34,125,35,126,26,126,28,126,29,126,30,126,31,126,32,126,33,126,34,
				126,35,127,26,127,28,127,29,127,30,127,31,127,32,127,33,127,34,127,35,
				128,26,128,28,128,29,128,30,128,31,128,32,128,33,128,34,128,35,129,26,
				129,29,129,30,129,31,129,32,129,33,129,34,129,35,130,26,130,31,130,32,
				130,33,130,34,132,27,132,34,133,34,135,30,136,31,136,32,137,33,137,34,
				137,35,137,36,143,48,143,49,144,51,144,52,145,37,145,38,145,45,145,47,
				146,36,146,37,146,38,146,44,147,35,147,36,147,37,147,38,147,39,147,42,
				147,43,147,44,147,46,148,35,148,36,148,37,148,38,148,39,148,40,148,41,
				148,42,148,43,148,45,149,34,149,35,149,36,149,37,149,38,149,39,149,40,
				149,41,149,42,149,43,150,34,150,36,150,37,150,38,150,39,150,40,150,41,
				150,42,150,43,151,34,151,36,151,37,151,38,151,39,151,40,151,41,151,42,
				151,43,152,34,152,36,152,37,152,38,152,39,152,40,152,41,152,42,152,43,
				153,34,153,37,153,38,153,39,153,40,153,41,153,42,153,43,154,34,154,39,
				154,40,154,41,154,42,156,35,156,42,157,0,157,42,158,0,158,1,159,0,
				159,1,159,38,160,0,160,1,160,39,160,40,161,0,161,1,161,2,161,41,
				161,42,161,43,161,44,162,0,162,1,162,2,163,2,164,4,165,4,166,4,
				167,0,167,1,167,3,168,0,169,1,185,61,185,62,186,60,186,61,186,62,
				186,68,187,59,187,60,187,61,187,62,187,63,187,66,187,67,187,68,188,59,
				188,60,188,61,188,62,188,63,188,64,188,65,188,66,188,67,189,58,189,59,
				189,60,189,61,189,62,189,63,189,64,189,65,189,66,189,67,190,58,190,60,
				190,61,190,62,190,63,190,64,190,65,190,66,190,67,191,58,191,60,191,61,
				191,62,191,63,191,64,191,65,191,66,191,67,192,58,192,60,192,61,192,62,
				192,63,192,64,192,65,192,66,192,67,193,58,193,61,193,62,193,63,193,64,
				193,65,193,66,193,67,194,58,194,63,194,64,194,65,194,66,196,59,196,66,
				197,66,199,62,200,32,200,33,201,31,201,32,201,33,201,34,201,35,202,30,
				202,31,202,32,202,33,202,34,202,35,202,36,202,41,202,42,203,31,203,32,
				203,33,203,34,203,35,203,36,203,37,204,31,204,32,204,33,204,34,204,35,
				204,36,204,37,204,45,205,31,205,32,205,33,205,34,205,35,205,36,205,47,
				206,31,206,32,206,33,206,34,206,35,206,36,207,32,207,33,207,34,207,35,
				207,42,208,32,208,33,208,34,208,35,209,33,209,34,209,35,210,33,210,34,
				211,34,212,34,213,34, };

		private int[] mDivChampHelmet1 =
			{
					0,91,1,91,1,92,2,91,2,92,2,93,2,94,2,99,3,91,3,92,
				3,93,3,94,3,95,3,96,3,97,3,98,3,99,4,92,4,93,4,94,
				4,95,4,96,4,97,5,92,5,93,5,94,5,95,5,96,5,97,6,93,
				6,94,6,95,6,96,6,97,6,98,6,99,7,95,7,96,7,97,7,98,
				7,99,22,35,23,35,24,35,25,35,25,36,26,35,26,36,26,37,26,38,
				26,43,27,35,27,36,27,37,27,38,27,39,27,40,27,41,27,42,27,43,
				28,36,28,37,28,38,28,39,28,40,28,41,29,36,29,37,29,38,29,39,
				29,40,29,41,30,37,30,38,30,39,30,40,30,41,30,42,30,43,31,39,
				31,40,31,41,31,42,31,43,32,41,32,42,32,43,62,43,63,43,64,43,
				65,43,65,44,66,43,66,44,66,45,66,46,66,51,67,43,67,44,67,45,
				67,46,67,47,67,48,67,49,67,50,67,51,68,44,68,45,68,46,68,47,
				68,48,68,49,69,44,69,45,69,46,69,47,69,48,69,49,70,45,70,46,
				70,47,70,48,70,49,70,50,70,51,71,47,71,48,71,49,71,50,71,51,
				72,49,72,50,72,51,72,52,94,91,95,91,96,91,97,91,97,92,98,91,
				98,92,98,93,98,94,98,99,99,91,99,92,99,93,99,94,99,95,99,96,
				99,97,99,98,99,99,100,92,100,93,100,94,100,95,100,96,100,97,101,92,
				101,93,101,94,101,95,101,96,101,97,102,93,102,94,102,95,102,96,102,97,
				102,98,102,99,103,95,103,96,103,97,103,98,103,99,126,27,127,27,128,27,
				129,27,129,28,130,27,130,28,130,29,130,30,130,35,131,27,131,28,131,29,
				131,30,131,31,131,32,131,33,131,34,131,35,132,28,132,29,132,30,132,31,
				132,32,132,33,133,28,133,29,133,30,133,31,133,32,133,33,134,29,134,30,
				134,31,134,32,134,33,134,34,134,35,135,31,135,32,135,33,135,34,135,35,
				136,33,136,34,136,35,145,46,146,45,146,46,147,45,150,35,151,35,152,35,
				153,35,153,36,154,35,154,36,154,37,154,38,154,43,155,35,155,36,155,37,
				155,38,155,39,155,40,155,41,155,42,155,43,156,0,156,1,156,36,156,37,
				156,38,156,39,156,40,156,41,157,1,157,2,157,36,157,37,157,38,157,39,
				157,40,157,41,158,2,158,3,158,37,158,38,158,39,158,40,158,41,158,42,
				158,43,159,2,159,3,159,39,159,40,159,41,159,42,159,43,160,2,160,4,
				160,41,160,42,160,43,161,3,161,5,161,6,162,3,162,4,162,5,162,7,
				163,0,163,1,163,3,163,4,163,6,163,8,163,18,164,2,164,3,164,9,
				165,3,165,10,166,3,166,10,167,2,167,4,168,1,168,2,168,3,169,0,
				169,4,170,3,171,1,171,2,173,0,173,1,173,3,173,4,190,59,191,59,
				192,59,193,59,193,60,194,59,194,60,194,61,194,62,194,67,195,59,195,60,
				195,61,195,62,195,63,195,64,195,65,195,66,195,67,196,60,196,61,196,62,
				196,63,196,64,196,65,197,60,197,61,197,62,197,63,197,64,197,65,198,61,
				198,62,198,63,198,64,198,65,198,66,198,67,199,63,199,64,199,65,199,66,
				199,67,203,30,204,29,204,30,205,29,205,30,206,29,206,30,207,29,207,30,
				207,31,208,29,208,30,208,31,209,29,209,30,209,31,209,32,210,30,210,31,
				210,32,211,30,211,31,211,32,211,33,212,31,212,32,212,33,213,32,213,33,
		};

		private int[] mConfChampUniform1 =
			{
					0,103,0,104,0,105,0,106,0,107,0,108,0,109,0,110,0,111,1,61,
				1,62,1,63,1,64,1,65,1,66,1,67,1,68,1,69,1,103,1,104,
				1,105,1,106,1,107,1,108,1,109,1,110,1,111,2,56,2,59,2,103,
				2,104,2,105,2,106,2,107,2,108,2,109,2,110,2,111,3,55,3,57,
				3,58,3,59,3,60,3,61,3,62,3,63,3,64,3,65,3,66,3,67,
				3,68,3,69,3,103,3,104,3,105,3,106,3,107,3,108,3,109,3,110,
				3,111,4,55,4,103,4,104,4,105,4,106,4,107,4,108,4,109,4,110,
				5,103,5,104,5,105,5,106,5,107,5,108,5,109,5,110,6,103,6,104,
				6,105,6,106,6,107,6,108,6,109,6,110,7,103,7,104,7,105,7,106,
				7,107,7,108,7,109,7,110,8,57,8,59,8,60,8,103,8,104,8,105,
				8,106,8,107,8,108,8,109,8,110,9,58,9,59,9,60,9,61,9,103,
				9,104,9,105,9,106,9,107,9,108,9,109,9,110,10,103,10,104,10,105,
				10,106,10,107,10,108,10,109,10,110,11,103,11,104,11,105,11,106,11,107,
				11,108,11,109,11,110,58,103,58,104,58,105,58,106,58,107,58,108,59,103,
				59,104,59,105,59,106,59,107,60,103,60,104,60,105,60,106,60,107,61,103,
				61,104,61,105,61,107,62,103,62,104,62,106,62,107,63,103,63,104,63,106,
				64,103,64,106,65,61,65,62,65,63,65,64,65,65,65,66,65,67,65,68,
				65,69,65,103,66,56,66,59,66,103,66,107,66,108,67,55,67,57,67,58,
				67,59,67,60,67,61,67,62,67,63,67,64,67,65,67,66,67,67,67,68,
				67,69,67,107,67,108,68,55,68,106,68,107,68,108,69,105,69,106,69,107,
				69,108,70,104,70,105,70,106,70,107,70,108,71,104,71,106,71,107,71,108,
				72,57,72,59,72,60,72,103,72,106,72,107,72,108,73,58,73,59,73,60,
				73,61,73,103,73,106,73,107,74,105,74,106,74,107,74,109,75,105,75,106,
				75,109,76,104,76,105,76,108,76,109,76,112,76,113,76,114,76,115,76,116,
				76,117,76,118,77,104,77,105,77,108,77,109,77,110,78,104,78,108,79,103,
				92,23,93,23,94,23,95,23,96,23,97,23,98,23,99,23,100,23,101,23,
				102,23,103,23,104,23,105,23,106,23,107,23,124,108,124,109,124,111,124,112,
				124,113,124,114,124,115,124,116,124,117,124,118,125,110,125,111,125,113,125,114,
				125,115,126,110,126,111,126,113,126,114,127,110,127,111,127,113,127,114,127,117,
				127,118,128,110,128,111,128,112,128,113,128,114,128,117,128,118,129,111,129,113,
				129,114,130,99,130,100,130,101,130,102,130,103,130,104,130,105,130,106,130,107,
				130,111,130,112,130,113,130,114,130,115,131,112,131,113,131,114,131,115,131,116,
				131,117,131,118,132,99,132,100,132,101,132,102,132,103,132,104,132,105,132,106,
				132,107,132,111,132,112,132,113,132,114,132,115,132,116,132,117,132,118,133,111,
				133,113,133,114,133,115,134,110,134,111,134,113,134,114,135,110,135,111,135,113,
				135,114,135,117,135,118,136,110,136,111,136,112,136,113,136,114,136,117,136,118,
				137,111,137,113,137,114,138,111,138,112,138,113,138,114,138,115,139,112,139,113,
				139,114,139,115,139,116,139,117,139,118,140,23,141,23,142,23,143,23,144,23,
				145,23,146,23,147,23,148,23,149,23,150,23,151,23,152,23,153,23,154,23,
				155,23,172,100,172,101,173,102,174,102,175,102,176,102,178,91,178,92,178,93,
				178,94,178,95,178,96,178,97,178,98,178,99,180,91,180,92,180,93,180,94,
				180,95,180,96,180,97,180,98,180,99,182,102,183,102,184,102,188,23,188,105,
				188,106,188,107,188,108,188,109,188,110,188,111,188,112,188,113,188,114,188,115,
				188,116,188,117,188,118,189,23,189,103,189,104,189,107,189,108,189,109,189,110,
				189,111,189,112,189,113,189,114,189,115,189,116,189,117,189,118,190,23,190,104,
				190,105,190,108,190,109,190,110,190,111,190,112,190,113,190,114,190,115,190,116,
				190,117,190,118,191,23,191,103,191,104,191,105,191,106,191,107,191,108,191,109,
				191,110,191,111,191,112,191,113,191,114,191,115,191,116,191,117,191,118,192,23,
				192,103,192,104,192,105,192,106,192,107,192,108,192,109,192,110,192,111,192,112,
				192,113,192,114,192,115,192,116,192,117,192,118,193,23,193,103,193,104,193,105,
				193,106,193,107,193,108,193,109,193,110,193,111,193,112,193,113,193,114,193,115,
				193,116,194,23,194,103,194,104,194,105,194,106,194,107,194,108,194,109,194,110,
				194,111,194,112,194,113,195,23,195,104,195,105,195,106,195,107,196,23,197,23,
		};

		private int[] mConfChampUniform2=
			{
					0,76,0,79,0,80,0,84,0,85,0,93,0,94,0,95,0,97,0,98,
				0,100,0,101,0,102,1,76,1,77,1,78,1,79,1,80,1,81,1,82,
				1,83,1,84,1,85,1,86,1,87,1,88,1,89,1,90,1,94,1,95,
				1,98,1,101,1,102,2,76,2,79,2,80,2,81,2,82,2,83,2,84,
				2,85,2,86,2,87,2,95,2,98,3,76,3,79,3,80,3,85,3,86,
				3,99,4,76,4,77,4,79,4,80,5,76,5,79,5,80,6,76,6,77,
				6,79,6,80,7,76,8,75,8,76,8,78,9,75,9,76,10,75,11,74,
				11,75,12,74,12,75,13,71,13,73,13,74,14,72,14,74,15,73,15,110,
				16,73,16,74,16,75,17,71,17,76,18,72,18,78,19,71,19,72,19,74,
				28,27,28,28,29,30,30,15,30,31,31,15,31,19,32,12,32,17,32,26,
				32,27,34,15,34,17,35,13,35,18,37,9,37,16,38,19,38,20,39,16,
				40,10,40,14,40,15,40,20,42,10,42,12,42,19,44,36,44,37,44,38,
				44,39,45,34,45,35,45,36,45,37,45,38,45,39,45,40,46,15,46,33,
				46,34,46,35,46,36,46,37,46,38,46,39,46,40,47,15,47,19,47,33,
				47,36,47,37,47,38,48,12,48,17,48,34,48,36,48,37,49,36,49,70,
				49,71,49,73,49,74,49,75,50,15,50,17,50,35,50,36,50,68,50,69,
				50,70,50,73,50,76,50,77,51,13,51,18,51,67,51,68,51,70,51,71,
				51,72,51,73,51,74,51,75,51,77,51,78,52,67,52,69,52,70,52,71,
				52,72,52,73,52,74,52,75,52,76,52,77,52,78,52,79,52,80,52,81,
				53,9,53,16,53,66,53,68,53,69,53,70,53,71,53,72,53,73,53,74,
				53,75,53,76,53,77,53,78,53,80,53,81,53,82,53,83,54,19,54,20,
				54,66,54,68,54,69,54,70,54,71,54,72,54,73,54,74,54,75,54,76,
				54,77,54,78,54,79,54,82,54,83,54,84,54,85,54,86,55,16,55,66,
				55,67,55,68,55,69,55,70,55,71,55,72,55,73,55,74,55,75,55,76,
				55,77,55,78,55,79,55,84,55,85,55,86,55,87,55,88,55,89,56,10,
				56,14,56,15,56,20,56,67,56,68,56,69,56,70,56,71,56,72,56,73,
				56,74,56,75,56,76,56,77,56,78,56,79,56,86,56,87,56,88,56,89,
				56,90,56,91,56,92,56,93,56,94,56,95,57,67,57,69,57,70,57,71,
				57,72,57,73,57,74,57,75,57,76,57,77,57,78,57,87,57,88,57,89,
				57,90,57,91,57,92,57,93,57,94,57,95,57,96,57,97,57,98,57,99,
				57,100,58,10,58,12,58,19,58,70,58,71,58,72,58,73,58,74,58,75,
				58,76,58,77,58,78,58,79,58,91,58,92,58,93,58,94,58,95,58,96,
				58,97,58,98,58,99,58,100,58,101,58,102,59,71,59,72,59,73,59,74,
				59,75,59,76,59,77,59,78,59,96,59,97,59,98,59,99,59,100,60,74,
				60,75,60,76,60,77,60,98,60,100,61,74,61,76,62,15,62,74,62,76,
				63,15,63,19,63,74,64,12,64,17,64,74,65,74,66,15,66,17,66,74,
				67,13,67,18,67,74,68,74,69,9,69,16,70,19,70,20,71,16,72,10,
				72,14,72,15,72,20,74,10,74,12,74,19,78,15,78,25,78,34,79,15,
				79,19,79,25,79,26,79,27,79,28,79,29,79,30,79,31,79,32,79,33,
				79,34,79,85,79,86,79,87,79,88,80,12,80,17,80,25,80,34,80,83,
				80,84,80,85,80,86,80,87,80,88,80,89,80,90,81,81,81,82,81,83,
				81,89,81,90,81,92,81,93,82,15,82,17,82,80,82,92,82,94,82,95,
				83,13,83,18,83,94,83,95,85,9,85,16,86,19,86,20,86,25,86,34,
				87,16,87,25,87,26,87,27,87,28,87,29,87,30,87,31,87,32,87,33,
				87,34,88,10,88,14,88,15,88,20,88,25,88,34,88,60,90,10,90,12,
				90,19,91,61,94,15,94,63,94,79,95,15,95,63,95,67,95,79,96,12,
				96,60,96,65,96,76,98,63,98,65,99,13,99,18,99,61,99,66,99,77,
				99,82,101,9,101,57,101,64,101,73,102,19,102,20,102,67,102,68,102,83,
				102,84,103,16,103,64,103,80,104,14,104,15,104,20,104,58,104,62,104,63,
				104,68,104,78,104,79,104,84,106,12,106,19,106,58,106,60,106,67,106,76,
				106,83,108,92,108,93,108,94,108,95,109,90,109,91,109,92,109,93,109,94,
				109,95,109,96,110,15,110,63,110,79,110,89,110,90,110,91,110,92,110,93,
				110,94,110,95,110,96,111,15,111,19,111,63,111,79,111,83,111,89,111,92,
				111,93,111,94,112,12,112,17,112,60,112,76,112,81,112,90,112,92,112,93,
				113,92,114,15,114,17,114,79,114,81,114,91,114,92,115,13,115,18,115,61,
				115,66,115,77,115,82,116,110,116,111,116,112,116,113,116,114,116,115,117,9,
				117,16,117,57,117,80,117,108,117,109,117,110,117,111,117,112,117,113,117,114,
				117,115,117,116,117,117,117,118,118,19,118,20,118,67,118,68,118,83,118,84,
				118,107,118,108,118,109,118,110,118,111,118,112,118,113,118,114,118,115,118,116,
				118,117,118,118,119,16,119,64,119,80,119,107,119,108,119,109,119,110,119,111,
				119,112,119,113,119,114,119,115,119,116,119,117,119,118,120,10,120,14,120,15,
				120,20,120,62,120,63,120,68,120,76,120,79,120,84,120,107,120,108,120,109,
				120,110,120,111,120,112,120,113,120,114,120,115,121,107,121,108,121,109,121,110,
				121,111,121,112,121,113,121,114,122,10,122,12,122,19,122,60,122,67,122,108,
				122,109,122,110,122,111,122,114,123,77,123,79,123,83,123,109,125,72,126,15,
				126,63,126,75,126,76,126,79,127,15,127,19,127,63,127,67,127,72,127,79,
				127,83,128,12,128,17,128,60,128,65,128,71,128,76,128,81,130,15,130,17,
				130,63,130,65,130,79,130,81,131,13,131,18,131,61,131,66,131,71,131,75,
				131,82,133,9,133,16,133,57,133,64,133,80,134,19,134,20,134,67,134,68,
				134,83,134,84,135,16,135,64,135,80,136,10,136,14,136,15,136,20,136,58,
				136,62,136,63,136,68,136,76,136,79,136,84,138,10,138,12,138,19,138,58,
				138,60,138,67,139,77,139,79,139,83,141,72,142,15,142,63,142,75,142,76,
				142,79,143,15,143,19,143,63,143,67,143,72,143,79,144,12,144,17,144,60,
				144,65,144,71,144,76,146,15,146,17,146,63,146,65,146,75,147,13,147,18,
				147,61,147,66,147,82,148,90,148,91,149,9,149,16,149,57,149,64,149,91,
				149,92,149,93,150,19,150,20,150,67,150,68,150,90,150,92,150,93,150,94,
				151,16,151,64,151,89,151,90,151,92,151,93,151,94,151,95,151,96,152,10,
				152,14,152,15,152,20,152,58,152,62,152,63,152,68,152,89,152,90,152,91,
				152,92,152,93,152,94,152,95,152,96,153,92,154,10,154,12,154,19,154,58,
				154,60,154,67,156,25,156,34,156,76,156,77,156,78,156,79,157,25,157,26,
				157,33,157,34,157,74,157,75,157,76,157,77,157,78,157,79,157,80,158,15,
				158,25,158,27,158,28,158,31,158,32,158,34,158,63,158,73,158,74,158,75,
				158,76,158,77,158,78,158,79,158,80,159,15,159,28,159,29,159,30,159,31,
				159,63,159,73,159,76,159,77,159,78,160,12,160,25,160,27,160,28,160,31,
				160,32,160,34,160,60,160,74,160,76,160,77,161,25,161,26,161,33,161,34,
				161,76,162,25,162,34,162,75,162,76,163,13,163,18,163,61,163,66,164,25,
				164,34,164,102,164,103,164,104,164,105,164,106,164,107,165,9,165,25,165,26,
				165,33,165,34,165,57,165,100,165,101,165,102,165,103,165,104,165,105,165,106,
				165,107,165,108,165,109,165,110,165,111,166,19,166,20,166,25,166,27,166,28,
				166,31,166,32,166,34,166,67,166,68,166,99,166,100,166,101,166,102,166,103,
				166,104,166,105,166,106,166,107,166,108,166,109,166,110,166,111,166,112,166,113,
				166,114,166,115,167,16,167,28,167,29,167,30,167,31,167,64,167,99,167,100,
				167,101,167,102,167,103,167,104,167,105,167,106,167,107,167,108,167,109,167,110,
				167,111,167,112,167,113,167,114,167,115,167,116,167,117,167,118,168,14,168,15,
				168,20,168,25,168,27,168,28,168,31,168,32,168,34,168,62,168,63,168,68,
				168,99,168,100,168,101,168,102,168,103,168,104,168,105,168,106,168,107,168,111,
				168,112,168,113,168,114,168,115,168,116,168,117,168,118,169,25,169,26,169,33,
				169,34,169,99,169,100,169,101,169,102,169,103,169,104,169,105,169,106,169,115,
				169,116,169,117,169,118,170,12,170,19,170,25,170,34,170,60,170,67,170,100,
				170,101,170,102,170,103,170,106,170,118,171,101,172,25,172,34,173,25,173,26,
				173,33,173,34,174,15,174,25,174,27,174,28,174,31,174,32,174,34,174,63,
				174,71,175,15,175,19,175,28,175,29,175,30,175,31,175,63,175,67,175,71,
				175,75,176,12,176,17,176,25,176,27,176,28,176,31,176,32,176,34,176,60,
				176,65,176,73,177,25,177,26,177,33,177,34,178,15,178,17,178,25,178,34,
				178,63,178,65,178,71,178,73,179,13,179,18,179,61,179,66,179,74,181,9,
				181,16,181,57,181,64,182,19,182,20,182,25,182,34,182,67,182,68,182,75,
				182,76,183,16,183,25,183,26,183,27,183,28,183,29,183,30,183,31,183,32,
				183,33,183,34,183,64,183,72,184,10,184,14,184,15,184,20,184,25,184,34,
				184,58,184,62,184,63,184,68,184,71,184,76,186,10,186,12,186,19,186,58,
				186,60,186,67,186,75,190,15,190,63,191,15,191,19,191,63,191,67,192,12,
				192,17,192,60,192,65,194,15,194,17,194,63,194,65,195,13,195,18,195,61,
				195,66,196,74,196,75,197,9,197,16,197,75,197,76,197,77, };

		private int[] mConfChampUniform3=
			{
					0,96,0,99,1,91,1,92,1,93,1,96,1,97,1,99,1,100,2,88,
				2,89,2,90,2,91,2,92,2,93,2,94,2,96,2,97,2,99,2,100,
				2,101,2,102,3,87,3,88,3,89,3,90,3,91,3,92,3,93,3,94,
				3,95,3,96,3,97,3,98,3,100,3,101,3,102,4,85,4,86,4,87,
				4,88,4,89,4,90,4,91,4,92,4,93,4,94,4,95,4,96,4,97,
				4,98,4,99,4,100,4,101,4,102,5,83,5,84,5,85,5,86,5,87,
				5,88,5,89,5,90,5,91,5,92,5,95,5,96,5,97,5,98,5,99,
				5,100,5,101,5,102,6,83,6,84,6,85,6,86,6,87,6,88,6,89,
				6,95,6,96,6,97,6,98,6,99,6,100,6,101,6,102,7,79,7,80,
				7,83,7,84,7,85,7,86,7,87,7,94,7,95,7,96,7,97,7,98,
				7,99,7,100,7,101,7,102,8,79,8,80,8,83,8,84,8,85,8,91,
				8,92,8,93,8,94,8,95,8,96,8,97,8,98,8,99,8,100,8,101,
				8,102,9,74,9,77,9,78,9,79,9,80,9,89,9,90,9,91,9,92,
				9,93,9,94,9,95,9,96,9,97,9,98,9,99,9,100,9,101,9,102,
				10,72,10,73,10,74,10,76,10,77,10,78,10,79,10,80,10,87,10,88,
				10,89,10,90,10,91,10,92,10,93,10,94,10,95,10,96,10,97,10,98,
				10,99,10,100,10,101,10,102,11,71,11,72,11,73,11,76,11,77,11,78,
				11,79,11,80,11,85,11,86,11,87,11,88,11,89,11,90,11,91,11,92,
				11,93,11,94,11,95,11,96,11,97,11,98,11,99,11,100,11,101,11,102,
				12,73,12,76,12,77,12,78,12,79,12,80,12,81,12,82,12,83,12,84,
				12,85,12,86,12,87,12,88,12,89,12,90,12,91,12,92,12,93,12,94,
				12,95,12,96,12,97,12,98,12,99,12,100,12,101,12,102,12,103,12,104,
				12,105,12,106,12,107,12,108,12,109,12,110,13,69,13,75,13,76,13,77,
				13,78,13,79,13,80,13,81,13,82,13,83,13,84,13,85,13,86,13,87,
				13,88,13,89,13,90,13,91,13,92,13,93,13,94,13,95,13,96,13,97,
				13,98,13,99,13,100,13,101,13,102,13,103,13,104,13,105,13,106,13,107,
				13,108,13,109,13,110,14,68,14,71,14,75,14,76,14,77,14,78,14,79,
				14,80,14,81,14,82,14,83,14,84,14,85,14,86,14,87,14,88,14,89,
				14,90,14,91,14,92,14,93,14,94,14,95,14,96,14,97,14,98,14,99,
				14,100,14,101,14,102,14,103,14,104,14,105,14,106,14,107,14,108,14,109,
				14,110,15,67,15,68,15,72,15,75,15,76,15,77,15,78,15,79,15,80,
				15,81,15,82,15,83,15,84,15,85,15,86,15,87,15,88,15,89,15,90,
				15,91,15,92,15,93,15,94,15,95,15,96,15,97,15,98,15,99,15,100,
				15,101,15,102,15,103,15,104,15,105,15,106,15,107,15,108,15,109,16,67,
				16,68,16,76,16,77,16,78,16,79,16,80,16,81,16,82,16,83,16,84,
				16,85,16,86,16,87,16,88,16,89,16,90,16,91,16,92,16,93,16,94,
				16,95,16,96,16,97,16,98,16,99,16,100,16,101,16,102,16,103,16,104,
				16,105,16,106,16,107,16,108,17,66,17,67,17,68,17,70,17,74,17,75,
				17,78,17,79,17,80,17,81,17,82,17,83,17,84,17,85,17,86,17,87,
				17,88,17,89,17,90,17,91,17,92,17,93,17,94,17,95,17,96,17,97,
				17,98,17,99,17,100,17,101,18,66,18,67,18,68,18,69,18,70,18,71,
				18,75,18,76,18,77,18,79,18,80,18,81,18,82,18,83,18,84,18,85,
				18,86,18,87,18,88,18,89,18,90,18,91,18,92,18,93,18,94,18,95,
				18,96,19,66,19,67,19,68,19,69,19,70,19,73,19,78,19,79,19,80,
				19,81,19,82,19,83,19,84,19,85,19,86,19,87,19,88,19,89,19,90,
				19,91,19,92,19,93,19,94,20,66,20,67,20,68,20,69,20,70,20,71,
				20,72,20,73,20,74,20,75,20,76,20,77,20,78,20,79,20,80,20,81,
				20,82,20,83,20,84,20,85,20,86,20,87,20,88,20,89,20,90,20,91,
				20,92,21,66,21,67,21,68,21,69,21,70,21,71,21,72,21,73,21,74,
				21,75,21,76,21,77,21,78,21,79,21,80,21,81,21,82,21,83,21,84,
				21,85,21,86,21,87,21,88,21,89,21,90,21,91,22,67,22,68,22,69,
				22,70,22,71,22,72,22,73,22,74,22,75,22,76,22,77,22,78,22,79,
				22,80,22,81,22,82,22,83,22,84,22,85,22,86,22,87,22,88,22,89,
				23,67,23,68,23,69,23,70,23,71,23,72,23,73,23,74,23,75,23,76,
				23,77,23,78,23,79,23,80,23,81,23,82,23,83,23,84,23,85,23,86,
				24,68,24,69,24,70,24,71,24,72,24,73,24,74,24,75,24,76,24,77,
				24,78,24,79,24,80,24,81,24,82,24,83,24,84,25,68,25,69,25,70,
				25,71,25,72,25,73,25,74,25,75,25,76,25,77,25,78,25,79,25,80,
				25,81,25,82,26,69,26,70,26,71,26,72,26,73,26,74,26,75,26,76,
				26,77,26,78,26,79,26,80,27,71,27,72,27,73,27,74,27,75,27,76,
				27,77,27,78,28,15,28,26,29,18,29,27,29,28,29,29,30,10,30,12,
				30,25,30,26,30,28,30,29,30,30,31,9,31,25,31,28,31,29,31,30,
				32,25,32,28,32,29,32,30,32,31,32,33,32,34,32,35,33,26,33,27,
				33,28,33,29,33,30,33,31,33,32,33,34,33,35,34,18,34,27,34,28,
				34,29,34,30,34,31,34,32,34,33,35,29,35,30,35,31,38,18,40,19,
				43,18,44,15,45,18,46,10,46,12,47,9,47,34,47,35,47,39,47,40,
				48,33,48,38,48,39,48,40,49,34,49,37,49,38,50,18,50,37,51,35,
				53,79,54,18,54,80,54,81,55,80,55,81,55,82,55,83,56,19,56,80,
				56,81,56,82,56,83,56,84,56,85,57,79,57,80,57,81,57,82,57,83,
				57,84,57,85,57,86,58,69,58,80,58,81,58,82,58,83,58,84,58,85,
				58,86,58,87,58,88,58,89,58,90,59,18,59,70,59,79,59,80,59,81,
				59,82,59,83,59,84,59,86,59,87,59,88,59,89,59,90,59,91,59,92,
				59,93,59,94,59,95,59,101,59,102,60,15,60,73,60,78,60,79,60,80,
				60,81,60,82,60,83,60,87,60,88,60,89,60,90,60,91,60,92,60,93,
				60,94,60,95,60,96,60,97,60,101,60,102,61,18,61,75,61,79,61,80,
				61,81,61,82,61,83,61,100,61,101,61,102,62,10,62,12,62,75,62,79,
				62,80,62,82,62,83,62,100,62,101,62,102,63,9,63,75,63,78,63,79,
				63,80,63,81,63,82,63,83,63,84,63,85,63,86,63,87,63,88,63,89,
				63,90,63,91,63,92,63,93,63,94,63,95,63,96,63,97,63,98,63,100,
				63,101,63,102,64,75,64,76,64,79,64,80,64,81,64,82,64,83,64,84,
				64,85,64,86,64,87,64,88,64,89,64,90,64,91,64,92,64,93,64,94,
				64,95,64,96,64,97,64,98,64,99,64,100,64,101,64,102,65,75,65,76,
				65,77,65,79,65,80,65,81,65,82,65,83,65,84,65,99,65,100,65,101,
				65,102,66,18,66,75,66,79,66,80,66,81,66,82,66,83,66,100,66,101,
				66,102,67,73,67,75,67,76,67,77,67,78,67,79,67,80,67,81,67,82,
				67,83,67,86,67,87,67,88,67,89,67,92,67,93,67,94,67,95,67,96,
				67,97,67,100,67,101,67,102,68,0,68,5,68,6,68,73,68,75,68,79,
				68,80,68,81,68,82,68,83,68,86,68,87,68,88,68,89,68,92,68,93,
				68,94,68,95,68,96,68,97,68,100,68,101,68,102,69,5,69,6,69,73,
				69,74,69,75,69,76,69,78,69,79,69,80,69,81,69,82,69,83,69,86,
				69,87,69,88,69,89,69,93,69,94,69,95,69,96,69,97,69,100,69,101,
				70,5,70,6,70,18,70,73,70,74,70,75,70,79,70,80,70,81,70,82,
				70,83,70,86,70,87,70,88,70,89,70,100,70,101,71,0,71,5,71,6,
				71,72,71,73,71,74,71,75,71,76,71,77,71,78,71,79,71,80,71,81,
				71,82,71,83,71,84,71,85,71,86,71,87,71,88,71,89,71,90,71,99,
				71,100,71,101,72,5,72,6,72,19,72,72,72,73,72,74,72,75,72,77,
				72,78,72,79,72,80,72,81,72,82,72,83,72,84,72,85,72,86,72,87,
				72,88,72,89,72,90,72,91,72,92,72,93,72,94,72,95,72,96,72,98,
				72,99,72,100,73,5,73,6,73,72,73,73,73,74,73,75,73,76,73,77,
				73,79,73,80,73,81,73,82,73,83,73,84,73,85,73,86,73,87,73,88,
				73,89,73,90,73,91,73,92,73,93,73,94,73,95,73,98,73,99,73,100,
				73,102,74,0,74,5,74,6,74,72,74,73,74,74,74,75,74,76,74,77,
				74,78,74,79,74,80,74,81,74,82,74,83,74,84,74,85,74,86,74,87,
				74,88,74,89,74,90,74,91,74,92,74,93,74,94,74,95,74,97,74,98,
				74,99,74,100,74,102,75,5,75,6,75,18,75,71,75,72,75,73,75,74,
				75,75,75,76,75,77,75,79,75,80,75,81,75,82,75,83,75,84,75,85,
				75,86,75,87,75,88,75,89,75,90,75,91,75,92,75,93,75,94,75,96,
				75,97,75,98,75,99,75,100,75,101,75,102,76,15,76,23,76,71,76,72,
				76,73,76,74,76,75,76,82,76,83,76,84,76,85,76,86,76,87,76,88,
				76,89,76,90,76,91,76,96,76,97,76,98,76,99,76,100,76,101,77,18,
				77,23,77,71,77,72,77,73,77,74,77,84,77,85,77,86,77,87,77,96,
				77,97,77,98,77,99,78,10,78,12,78,23,78,71,78,72,78,73,78,74,
				78,75,78,80,78,81,78,82,78,83,78,86,78,87,78,88,78,89,78,90,
				78,95,78,96,78,97,79,9,79,23,79,71,79,72,79,73,79,74,79,78,
				79,79,79,80,79,81,79,89,79,90,79,91,80,23,80,71,80,72,80,73,
				80,76,80,77,80,78,80,79,80,80,80,93,80,96,81,23,81,71,81,72,
				81,73,81,75,81,76,81,77,81,78,81,84,81,85,81,86,81,87,81,88,
				81,95,81,96,82,18,82,23,82,71,82,72,82,74,82,75,82,76,82,77,
				82,81,82,82,82,83,82,84,82,85,82,86,82,87,82,88,82,89,82,90,
				83,23,83,72,83,73,83,74,83,75,83,76,83,78,83,79,83,80,83,81,
				83,82,83,83,83,84,83,85,83,86,83,87,83,88,83,89,83,91,83,92,
				84,23,84,72,84,73,84,74,84,75,84,76,84,77,84,78,84,79,84,80,
				84,81,84,82,84,83,84,84,84,85,84,86,84,87,84,88,84,89,84,91,
				84,92,84,94,85,23,85,72,85,73,85,74,85,75,85,76,85,77,85,78,
				85,79,85,80,85,81,85,83,85,84,85,87,85,88,85,91,85,93,85,94,
				86,18,86,23,86,58,86,60,86,72,86,73,86,74,86,75,86,76,86,77,
				86,78,86,79,86,80,86,81,86,85,86,86,86,87,86,89,86,91,86,93,
				86,94,87,23,87,57,87,73,87,74,87,75,87,76,87,77,87,78,87,79,
				87,80,87,81,87,82,87,83,87,84,87,87,87,89,87,91,87,93,87,94,
				88,19,88,23,88,74,88,75,88,76,88,77,88,78,88,79,88,80,88,81,
				88,82,88,86,88,88,88,89,88,91,88,93,88,94,89,23,89,76,89,77,
				89,78,89,79,89,80,89,81,89,82,89,83,89,84,89,85,89,86,90,23,
				90,78,90,79,90,80,90,81,90,82,90,83,90,84,90,85,91,18,91,23,
				92,15,92,63,92,79,93,18,93,66,93,82,94,10,94,12,94,58,94,60,
				94,74,94,76,95,9,95,57,95,73,98,18,98,66,98,82,101,39,102,18,
				102,39,102,66,102,82,103,39,103,40,104,19,104,39,104,41,104,42,104,67,
				104,83,105,43,105,44,105,45,105,46,107,18,107,66,107,82,108,15,108,63,
				108,79,109,18,109,66,109,82,110,10,110,12,110,58,110,60,110,74,110,76,
				111,9,111,57,111,73,111,90,111,91,111,95,111,96,112,89,112,94,112,95,
				112,96,113,90,113,93,113,94,114,18,114,66,114,82,114,93,115,91,118,18,
				118,66,118,74,118,76,118,82,119,73,120,19,120,67,120,83,120,116,120,117,
				120,118,121,81,121,116,121,117,121,118,122,83,122,115,122,116,122,117,122,118,
				123,18,123,66,123,82,123,108,123,113,123,114,123,115,123,116,123,117,123,118,
				124,15,124,63,124,79,125,18,125,66,125,82,126,10,126,12,126,58,126,60,
				126,74,127,9,127,57,128,75,129,73,130,18,130,66,130,75,130,82,131,74,
				132,0,132,5,132,6,133,5,133,6,134,5,134,6,134,18,134,66,134,74,
				134,76,134,82,135,0,135,5,135,6,135,73,136,5,136,6,136,19,136,67,
				136,83,137,5,137,6,137,81,138,0,138,5,138,6,138,83,139,5,139,6,
				139,18,139,66,139,82,140,15,140,63,140,79,140,107,140,108,140,113,140,114,
				140,115,140,116,140,117,140,118,141,18,141,66,141,82,141,107,141,108,141,109,
				141,111,141,112,141,115,141,116,141,117,141,118,142,10,142,12,142,58,142,60,
				142,74,142,107,142,108,142,109,142,110,142,112,142,113,142,116,142,117,142,118,
				143,9,143,57,143,108,143,109,143,110,143,111,143,112,143,113,143,114,143,115,
				143,116,143,117,143,118,144,75,144,108,144,109,144,110,144,111,144,112,144,113,
				144,114,144,115,144,116,144,117,144,118,145,109,145,110,145,111,145,112,145,113,
				145,114,145,115,145,116,145,117,145,118,146,18,146,66,146,82,146,110,146,111,
				146,112,146,113,146,114,146,115,146,116,146,117,146,118,147,74,147,112,147,113,
				147,114,147,115,149,39,150,18,150,39,150,66,151,39,151,40,152,19,152,39,
				152,41,152,42,152,67,152,88,153,43,153,44,153,45,153,46,153,89,153,91,
				153,93,153,94,153,95,153,96,154,90,154,91,154,92,154,93,154,94,154,95,
				154,96,155,18,155,66,155,92,155,93,155,94,155,95,156,15,156,23,156,63,
				157,18,157,23,157,66,158,10,158,12,158,23,158,58,158,60,159,9,159,23,
				159,57,159,74,159,75,159,79,159,80,160,23,160,73,160,78,160,79,160,80,
				161,23,161,74,161,77,161,78,162,18,162,23,162,66,162,77,163,23,163,75,
				164,23,165,23,166,18,166,23,166,66,167,23,168,19,168,23,168,67,168,108,
				168,109,168,110,169,23,169,108,169,109,169,110,169,111,169,112,169,113,169,114,
				170,23,170,107,170,108,170,109,170,110,170,111,170,112,170,113,170,114,170,115,
				170,116,170,117,171,18,171,23,171,66,171,100,171,105,171,106,171,107,171,108,
				171,109,171,110,171,111,171,112,171,113,171,114,171,115,171,116,171,117,171,118,
				172,15,172,23,172,63,172,71,172,103,172,104,172,105,172,106,172,107,172,108,
				172,109,172,110,172,111,172,112,172,113,172,114,172,115,172,116,172,117,172,118,
				173,18,173,23,173,66,173,74,173,103,173,105,173,106,173,107,173,111,173,112,
				173,113,173,114,173,115,174,10,174,12,174,23,174,58,174,60,174,103,174,105,
				174,106,174,111,174,112,174,113,175,9,175,23,175,57,175,103,175,105,175,106,
				175,109,175,110,175,111,175,112,175,117,175,118,176,23,176,103,176,104,176,105,
				176,106,176,109,176,110,176,116,176,117,177,23,177,103,177,105,177,106,177,114,
				177,115,177,116,177,117,178,18,178,23,178,66,178,74,178,103,178,104,178,105,
				178,106,178,107,178,111,178,112,178,113,178,114,178,115,178,116,178,117,178,118,
				179,23,179,104,179,105,179,106,179,107,179,108,179,109,179,110,179,111,179,112,
				179,113,179,114,179,115,179,116,179,117,179,118,180,23,180,103,180,104,180,105,
				180,106,180,107,180,108,180,109,180,110,180,111,180,112,180,113,180,114,180,115,
				180,116,180,117,180,118,181,23,181,103,181,105,181,106,181,107,181,111,181,112,
				181,113,181,114,181,115,182,18,182,23,182,66,182,74,182,103,182,105,182,106,
				182,111,182,112,182,113,183,23,183,103,183,105,183,106,183,109,183,110,183,111,
				183,112,183,117,183,118,184,19,184,23,184,67,184,75,184,103,184,104,184,105,
				184,106,184,109,184,110,184,116,184,117,185,23,185,103,185,105,185,106,185,114,
				185,115,185,116,185,117,186,23,186,103,186,104,186,105,186,106,186,107,186,111,
				186,112,186,113,186,114,186,115,186,116,186,117,186,118,187,18,187,23,187,66,
				187,74,187,104,187,105,187,106,187,107,187,108,187,109,187,110,187,111,187,112,
				187,113,187,114,187,115,187,116,187,117,187,118,188,15,188,63,188,99,188,100,
				189,18,189,66,189,99,189,100,189,101,190,10,190,12,190,58,190,60,190,99,
				190,100,190,101,190,102,191,9,191,57,191,100,191,101,191,102,192,100,192,101,
				192,102,193,101,193,102,194,18,194,66,194,102,196,0,196,5,196,6,196,63,
				197,5,197,6,197,39,197,66, };

		private int[] mConfChampHelmet=
			{
					0,56,0,57,0,58,0,59,0,60,0,61,0,62,0,63,0,64,0,65,
				0,66,0,67,0,68,0,69,0,113,0,114,0,115,0,116,0,117,0,118,
				1,55,1,56,1,57,1,58,1,59,1,60,1,112,1,113,1,114,1,115,
				1,116,1,117,1,118,2,55,2,57,2,58,2,112,2,113,2,114,2,115,
				2,116,2,117,2,118,3,112,3,113,3,114,3,115,3,116,3,117,3,118,
				4,56,4,57,4,58,4,59,4,60,4,61,4,62,4,63,4,64,4,65,
				4,66,4,67,4,68,4,69,4,111,4,112,4,113,4,114,4,115,4,116,
				4,117,4,118,5,55,5,56,5,57,5,58,5,59,5,60,5,61,5,62,
				5,63,5,64,5,65,5,66,5,67,5,68,5,69,5,111,5,112,5,113,
				5,114,5,115,5,116,5,117,5,118,6,55,6,56,6,57,6,58,6,59,
				6,60,6,61,6,62,6,63,6,64,6,65,6,66,6,69,6,111,6,112,
				6,113,6,114,6,115,6,116,6,117,6,118,7,56,7,57,7,58,7,59,
				7,60,7,61,7,62,7,63,7,64,7,65,7,66,7,67,7,68,7,69,
				7,111,7,112,7,113,7,114,7,115,7,116,7,117,7,118,8,56,8,58,
				8,61,8,62,8,63,8,64,8,65,8,66,8,67,8,68,8,69,8,70,
				8,111,8,112,8,113,8,114,8,115,8,116,8,117,8,118,9,57,9,62,
				9,63,9,64,9,65,9,66,9,67,9,68,9,69,9,111,9,112,9,113,
				9,114,9,115,9,116,9,117,9,118,10,59,10,60,10,61,10,62,10,63,
				10,64,10,65,10,66,10,67,10,111,10,112,10,113,10,114,10,115,10,116,
				10,117,10,118,11,111,11,112,11,113,11,114,11,115,11,116,11,117,11,118,
				12,111,12,112,12,113,12,114,12,115,12,116,12,117,12,118,13,111,13,112,
				13,113,13,114,13,115,13,116,13,117,13,118,14,111,14,112,14,113,14,114,
				14,115,14,116,14,117,14,118,15,111,15,112,15,113,15,114,15,115,15,116,
				15,117,15,118,16,113,16,114,16,115,16,116,16,117,16,118,17,117,17,118,
				54,116,54,117,54,118,55,114,55,115,55,116,55,117,55,118,56,112,56,113,
				56,114,56,115,56,116,56,117,56,118,57,111,57,112,57,113,57,114,57,115,
				57,116,57,117,57,118,58,111,58,112,58,113,58,114,58,115,58,116,58,117,
				58,118,59,110,59,111,59,112,59,113,59,114,59,115,59,116,59,117,59,118,
				60,110,60,111,60,112,60,113,60,114,60,115,60,116,60,117,60,118,61,59,
				61,60,61,61,61,64,61,65,61,66,61,67,61,110,61,111,61,112,61,113,
				61,114,61,115,61,116,61,117,61,118,62,57,62,58,62,61,62,62,62,63,
				62,64,62,65,62,66,62,67,62,68,62,69,62,110,62,111,62,112,62,113,
				62,114,62,115,62,116,62,117,62,118,63,56,63,57,63,59,63,60,63,61,
				63,62,63,63,63,64,63,65,63,66,63,69,63,70,63,110,63,111,63,112,
				63,113,63,114,63,115,63,116,63,117,63,118,64,56,64,57,64,58,64,59,
				64,60,64,61,64,62,64,63,64,64,64,65,64,66,64,67,64,68,64,69,
				64,110,64,111,64,112,64,113,64,114,64,115,64,116,64,117,64,118,65,55,
				65,56,65,57,65,58,65,59,65,60,65,110,65,111,65,112,65,113,65,114,
				65,115,65,116,65,117,65,118,66,55,66,57,66,58,66,110,66,111,66,112,
				66,113,66,114,66,115,66,116,66,117,66,118,67,110,67,111,67,112,67,113,
				67,114,67,115,67,116,67,117,67,118,68,56,68,57,68,58,68,59,68,60,
				68,61,68,62,68,63,68,64,68,65,68,66,68,67,68,68,68,69,68,110,
				68,111,68,112,68,113,68,114,68,115,68,116,68,117,68,118,69,55,69,56,
				69,57,69,58,69,59,69,60,69,61,69,62,69,63,69,64,69,65,69,66,
				69,67,69,68,69,69,69,110,69,111,69,112,69,113,69,114,69,115,69,116,
				69,117,69,118,70,55,70,56,70,57,70,58,70,59,70,60,70,61,70,62,
				70,63,70,64,70,65,70,66,70,69,70,110,70,111,70,112,70,113,70,114,
				70,115,70,116,70,117,70,118,71,56,71,57,71,58,71,59,71,60,71,61,
				71,62,71,63,71,64,71,65,71,66,71,67,71,68,71,69,71,111,71,112,
				71,113,71,114,71,115,71,116,71,117,71,118,72,56,72,58,72,61,72,62,
				72,63,72,64,72,65,72,66,72,67,72,68,72,69,72,70,72,111,72,112,
				72,113,72,114,72,115,72,116,72,117,72,118,73,57,73,62,73,63,73,64,
				73,65,73,66,73,67,73,68,73,69,73,111,73,112,73,113,73,114,73,115,
				73,116,73,117,73,118,74,59,74,60,74,61,74,62,74,63,74,64,74,65,
				74,66,74,67,74,111,74,112,74,113,74,114,74,115,74,116,74,117,74,118,
				75,111,75,112,75,113,75,114,75,115,75,116,75,117,75,118,77,113,77,114,
				77,115,77,116,77,117,77,118,80,115,80,116,80,117,80,118,93,31,93,32,
				93,33,94,27,94,28,94,31,94,34,95,26,95,27,95,28,95,31,95,32,
				95,33,95,34,96,25,96,26,96,27,96,28,96,29,96,31,96,33,96,34,
				97,25,97,26,97,27,97,28,97,29,97,30,97,31,97,32,97,33,97,34,
				98,25,98,26,98,27,98,28,98,30,98,32,98,33,98,34,99,25,99,26,
				99,27,99,28,99,29,99,30,99,31,99,32,99,33,100,25,100,26,100,28,
				100,29,100,30,100,31,100,32,100,33,101,26,101,29,101,30,101,31,101,32,
				102,27,102,28,102,29,102,30,102,31,102,32,125,103,125,104,125,105,125,106,
				126,101,126,104,126,106,126,107,126,108,127,99,127,100,127,102,127,103,127,104,
				127,105,127,106,127,107,127,108,127,109,128,99,128,100,128,101,128,102,128,103,
				128,104,128,105,128,106,128,108,129,98,129,99,129,100,129,101,129,102,129,103,
				129,104,129,105,129,106,129,107,130,98,131,98,132,98,133,98,133,99,133,100,
				133,101,133,102,133,103,133,104,133,105,133,106,133,107,134,99,134,100,134,101,
				134,102,134,103,134,104,134,105,134,106,134,108,135,99,135,100,135,101,135,102,
				135,103,135,104,135,105,135,106,135,107,135,108,135,109,136,101,136,102,136,103,
				136,104,136,105,136,106,136,107,136,108,137,103,137,104,137,105,137,106,145,27,
				145,28,145,29,145,30,145,31,145,32,146,26,146,29,146,30,146,31,146,32,
				147,25,147,26,147,28,147,29,147,30,147,31,147,32,147,33,148,25,148,26,
				148,27,148,28,148,29,148,30,148,31,148,32,148,33,149,25,149,26,149,27,
				149,28,149,30,149,32,149,33,149,34,150,25,150,26,150,27,150,28,150,29,
				150,30,150,31,150,32,150,33,150,34,151,25,151,26,151,27,151,28,151,29,
				151,31,151,33,151,34,152,26,152,27,152,28,152,31,152,32,152,33,152,34,
				153,27,153,28,153,31,153,34,154,31,154,32,154,33,173,95,173,96,173,97,
				173,98,174,93,174,96,174,98,174,99,174,100,175,91,175,92,175,94,175,95,
				175,96,175,97,175,98,175,99,175,100,175,101,176,91,176,92,176,93,176,94,
				176,95,176,96,176,97,176,98,176,100,177,90,177,91,177,92,177,93,177,94,
				177,95,177,96,177,97,177,98,177,99,178,90,179,90,180,90,181,90,181,91,
				181,92,181,93,181,94,181,95,181,96,181,97,181,98,181,99,182,91,182,92,
				182,93,182,94,182,95,182,96,182,97,182,98,182,100,183,91,183,92,183,93,
				183,94,183,95,183,96,183,97,183,98,183,99,183,100,183,101,184,93,184,94,
				184,95,184,96,184,97,184,98,184,99,184,100,185,95,185,96,185,97,185,98,
				189,31,189,32,189,33,190,27,190,28,190,31,190,34,191,26,191,27,191,28,
				191,31,191,32,191,33,191,34,192,25,192,26,192,27,192,28,192,29,192,31,
				192,33,192,34,193,25,193,26,193,27,193,28,193,29,193,30,193,31,193,32,
				193,33,193,34,194,25,194,26,194,27,194,28,194,30,194,32,194,33,194,34,
				195,25,195,26,195,27,195,28,195,29,195,30,195,31,195,32,195,33,196,25,
				196,26,196,28,196,29,196,30,196,31,196,32,196,33,197,26,197,29,197,30,
				197,31,197,32, };
	#endregion

	}
}
//			for (int Xcount = 0; Xcount < b2.Width; Xcount++)
//			{
//				for (int Ycount = 0; Ycount < b2.Height; Ycount++)
//				{
//					tmp = b2.GetPixel(Xcount,Ycount);
//					if( tmp == c )
//					{
//						sb.Append(Xcount);
//						sb.Append(",");
//						sb.Append(Ycount);
//                      sb.Append(",");
//					}
//				}
//			}