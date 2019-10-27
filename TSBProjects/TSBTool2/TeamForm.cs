using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using TSBTool2;

namespace TSBTool2_UI
{
    public partial class TeamForm : Form
    {
        private PictureBox[] runBoxes = null;
        private PictureBox[] passBoxes = null;

        public TeamForm()
        {
            InitializeComponent();
            runBoxes = new PictureBox[]{
                R1, R2, R3, R4
            };
            passBoxes = new PictureBox[] {
                P1, P2, P3, P4
            };
        }


        private bool m_InitDone = false;
        private string mData = "";
        /// <summary>
        /// The data to work on.
        /// </summary>
        public string Data
        {
            get { return mData; }

            set
            {
                m_InitDone = false;
                mData = value;
                if (mData.IndexOf("TEAM") != -1)
                {
                    SetupTeams();
                    ShowTeamValues();
                }
                m_InitDone = true;
            }
        }

        public string CurrentTeam
        {
            get { return m_TeamsComboBox.SelectedItem.ToString(); }

            set
            {
                int index = m_TeamsComboBox.Items.IndexOf(value);
                if (index > -1)
                {
                    m_TeamsComboBox.SelectedIndex = index;
                }
            }
        }

        private void SetupTeams()
        {
            Regex teamRegex = new Regex("TEAM\\s*=\\s*([a-z0-9]+)");
            MatchCollection mc = teamRegex.Matches(mData);

            m_TeamsComboBox.Items.Clear();
            m_TeamsComboBox.BeginUpdate();
            foreach (Match m in mc)
            {
                string team = m.Groups[1].Value;
                m_TeamsComboBox.Items.Add(team);
            }
            m_TeamsComboBox.EndUpdate();
            if (m_TeamsComboBox.Items.Count > 0)
            {
                m_TeamsComboBox.SelectedItem = m_TeamsComboBox.Items[0];
            }
        }
        /// <summary>
        /// Shows the data for the current team.
        /// </summary>
        private void ShowTeamValues()
        {
            if( !IsShowingPlaybook1)
                ShowPlaybook(1);

            string team = m_TeamsComboBox.SelectedItem.ToString();
            string teamData = GetTeamString(team);

            if (teamData != null)
            {
                Regex simDataRegex = new Regex("SimData\\s*=\\s*(0x[0-9A-Fa-f]+)");
                Match simMatch = simDataRegex.Match(teamData);
                if (simMatch != Match.Empty)
                {
                    mSimDataTextBox.Text = simMatch.Groups[1].ToString();
                }
                Match pbMatch = InputParser.playbookRegex.Match(teamData);
                if (pbMatch != Match.Empty)
                {
                    string runs = pbMatch.Groups[1].ToString();
                    string passes = pbMatch.Groups[2].ToString();
                    SetRuns(1,runs);
                    SetPasses(1,passes);
                    mPlaybookTextBox.Text = pbMatch.Groups[0].ToString();
                }
                Match teamStringsMatch = InputParser.teamStringsRegex.Match(teamData);
                if (teamStringsMatch != Match.Empty)
                {
                    mAbbreviationTextBox.Text = teamStringsMatch.Groups[1].ToString();
                    mCityTextBox.Text = teamStringsMatch.Groups[2].ToString();
                    mTeamNameTextBox.Text = teamStringsMatch.Groups[3].ToString();
                }
            }
        }

        private string mRuns = "";
        private string mPasses = "";

        private void SetRuns(int playbook, string runs)
        {
            string currentPlays = "";
            mRuns = runs;
            if (playbook == 1)
                currentPlays = runs.Substring(1, 4);
            else
                currentPlays = runs.Substring(5);

            if (currentPlays.Length > 3)
            {
                for (int i = 0; i < runBoxes.Length; i++)
                    runBoxes[i].Tag = currentPlays[i] + "";
            }
            UpdatePictureBoxes();
        }

        private void SetPasses(int playbook, string passes)
        {
            string currentPlays = "";
            mPasses = passes;
            if (playbook == 1)
                currentPlays = passes.Substring(1, 4);
            else
                currentPlays = passes.Substring(5);
            if (currentPlays.Length > 3)
            {
                for (int i = 0; i < passBoxes.Length; i++)
                    passBoxes[i].Tag = currentPlays[i] + "";
            }
            UpdatePictureBoxes();
        }

        private string GetCurrentPlaybook()
        {
            return String.Format("PLAYBOOK {0}, {1}", mRuns, mPasses);
        }

        private void UpdateData()
        {
            if (m_InitDone)
            {
                string team = m_TeamsComboBox.SelectedItem.ToString();
                string oldValue = GetTeamString(team);
                string newValue = GetCurrentTeamString();

                mData = mData.Replace(oldValue, newValue);
            }
        }

        /// <summary>
        /// Gets the text representation of the current UI.
        /// </summary>
        /// <returns></returns>
        private string GetCurrentTeamString()
        {
            string tmp = mSimDataTextBox.Text;
            if (tmp.IndexOf("0x", StringComparison.InvariantCultureIgnoreCase) != 0)
                tmp = "0x" + tmp;
            string ret = string.Format("TEAM = {0},SimData={1}", m_TeamsComboBox.SelectedItem, tmp);
            if (mData.IndexOf("PLAYBOOK") > 3)
                ret = ret + "\n" + GetCurrentPlaybook();
            if (mData.IndexOf("TEAM_ABB") >  -1)
                ret = ret +"\n" + string.Format("TEAM_ABB={0},TEAM_CITY={1},TEAM_NAME={2}",
                    mAbbreviationTextBox.Text, mCityTextBox.Text, mTeamNameTextBox.Text);

            return ret;
        }

        /// <summary>
        /// Gets a string like:
        /// "TEAM = bills SimData=0xab0"
        /// that is currently from mData.
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        private string GetTeamString(string team)
        {
            string theTeam = string.Format("TEAM = {0}", team);
            int teamIndex = mData.IndexOf(theTeam);
            string retVal = null;

            if (teamIndex > -1 )
            {
                int qb1Index = mData.IndexOf("QB1", teamIndex) - 1;
                retVal = mData.Substring(teamIndex, qb1Index - teamIndex);
            }
            return retVal;
        }

        private bool IsShowingPlaybook1 { get { return mPlaybook1Button.BackColor == mSelectedColor; } }

        private Color mSelectedColor = System.Drawing.Color.Gold;
        private Color mUnselectedColor = System.Drawing.Color.White;

        private void playbookButton_Click(object sender, EventArgs e)
        {
            if (sender == mPlaybook1Button)
                ShowPlaybook(1);
            else
                ShowPlaybook(2);
        }

        /// <summary>
        /// Show the playbook
        /// </summary>
        /// <param name="num">1 or 2</param>
        private void ShowPlaybook(int num)
        {
            if (num == 1)
            {
                mPlaybook1Button.BackColor = mSelectedColor;
                mPlaybook2Button.BackColor = mUnselectedColor;
                SetRuns(1, mRuns);
                SetPasses(1, mPasses);
            }
            else
            {
                mPlaybook1Button.BackColor = mUnselectedColor;
                mPlaybook2Button.BackColor = mSelectedColor;
                SetRuns(2, mRuns);
                SetPasses(2, mPasses);
            }
        }

        private void UpdatePictureBoxes()
        {
            for (int i = 0; i < runBoxes.Length; i++)
            {
                runBoxes[i].Image = PlaySelectForm.GetPlayImage("R", i + 1, Int32.Parse( runBoxes[i].Tag.ToString(),System.Globalization.NumberStyles.AllowHexSpecifier ));
                passBoxes[i].Image = PlaySelectForm.GetPlayImage("P", i + 1, Int32.Parse(passBoxes[i].Tag.ToString(),System.Globalization.NumberStyles.AllowHexSpecifier ));
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            string play =  PlaySelectForm.GetPlay(pb.Name, this);
            if (play != null)
            {
                pb.Tag = play;
                UpdatePictureBoxes();
                UpdatePlaybook();
                UpdateData();
            }
        }

        private void UpdatePlaybook()
        {
            string runs = "";
            string passes = "";
            for (int i = 0; i < 4; i++)
            {
                runs   += runBoxes[i].Tag;
                passes += passBoxes[i].Tag;
            }
            if (mPlaybook1Button.BackColor == mSelectedColor)
            {
                mRuns = "R" + runs + mRuns.Substring(5);
                mPasses = "P" + passes + mPasses.Substring(5);
            }
            else
            {
                mRuns = mRuns.Substring(0, 4) + runs;
                mPasses = mPasses.Substring(0, 4) + passes;
            }
            mPlaybookTextBox.Text = "PLAYBOOK " + mRuns +", "+ mPasses;
        }

        private void m_TeamsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowTeamValues();
        }

        private void teamValueTextBox_Leave(object sender, EventArgs e)
        {
            UpdateData();
        }

        private void updateTeamsSimButton_Click(object sender, EventArgs e)
        {
            DefensiveSimUpdater dsu = new DefensiveSimUpdater();
            dsu.Data = mData;
            List<string> teams = new List<string>( m_TeamsComboBox.Items.Count);
            foreach (object team in m_TeamsComboBox.Items) teams.Add(team.ToString());
            dsu.UpdateTeamsSimDefense(1, teams);
            this.mData = dsu.Data;
        }

        private void updateTeamSimData_Click(object sender, EventArgs e)
        {
            DefensiveSimUpdater dsu = new DefensiveSimUpdater();
            dsu.Data = mData;
            try
            {
                string result = "0x" + dsu.UpdateTeamSimDefense(1, CurrentTeam).ToUpper();
                mSimDataTextBox.Text = result;
            }
            catch (Exception ex)
            {
                StaticUtils.ShowError(ex.ToString());
            }
        }

    }
}
