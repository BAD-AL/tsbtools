using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TSBTool
{
    /// <summary>
    /// Used in Editing the Pro Bowl rosters.
    /// </summary>
    public partial class AllstarPlayerControl : UserControl, IAllStarPlayerControl
    {
        private static string[] positionNames = { 
	    "QB1", "QB2", "RB1", "RB2",  "RB3",  "RB4",  "WR1",  "WR2", "WR3", "WR4", "TE1", 
    	"TE2", "C",   "LG",  "RG",   "LT",   "RT",
    	"RE", "NT",   "LE",  "ROLB", "RILB", "LILB", "LOLB", "RCB", "LCB", "FS",  "SS",  "K", "P" 
    	};

        private String[] mLegalPositions;

        public AllstarPlayerControl()
        {
            InitializeComponent();
            Initialize();
        }

        public void ReInitialize()
        {
            Initialize();
            SetInitialValue();
        }

        private void Initialize()
        {
            if (AllStarForm.CombineTeamAndPos)
            {
                mTeamComboBox.Visible = false;
                mPositionComboBox.Visible = false;
                mTeamPosComboBox.Visible = true;

                int right = mPlayerNameLabel.Right;
                mPlayerNameLabel.Left = mTeamPosComboBox.Right + 5;
                mPlayerNameLabel.Width = right - mPlayerNameLabel.Left;
            }
            else
            {
                mTeamPosComboBox.Visible = false;
                mTeamComboBox.Visible = true;
                mPositionComboBox.Visible = true;

                int right = mPlayerNameLabel.Right;
                mPlayerNameLabel.Left = mPositionComboBox.Right + 5;
                mPlayerNameLabel.Width = right - mPlayerNameLabel.Left;
            }
        }

        private String GetTeam()
        {
            String ret = "";
            if (AllStarForm.CombineTeamAndPos)
            {
                string[] team_Pos = mTeamPosComboBox.SelectedItem.ToString().Split(new char[] { ' ' });
                ret = team_Pos[0];
            }
            else
            {
                ret = mTeamComboBox.SelectedItem.ToString();
            }
            return ret;
        }

        private String GetPos()
        {
            String ret = "";
            if (AllStarForm.CombineTeamAndPos)
            {
                string[] team_Pos = mTeamPosComboBox.SelectedItem.ToString().Split(new char[] { ' ' });
                ret = team_Pos[1];
            }
            else
            {
                ret = mPositionComboBox.SelectedItem.ToString();
            }
            return ret;
        }


        static Regex sProBowlPositionRegex = null;
        /// <summary>
        /// Groups[1] = Confrence, Groups[2] = Pro bowl position, groups[3]=team, groups[4] = team position
        /// </summary>
        public static Regex ProBowlPositionRegex
        {
            get
            {
                if (sProBowlPositionRegex == null)
                {
                    //"AFC,QB1,dolphins,QB1"
                    sProBowlPositionRegex = new Regex("(AFC|NFC),([A-Z1-4]+),([a-z49]+),([A-Z1-4]+)");
                }
                return sProBowlPositionRegex;
            }
        }

        #region Properties 
        private Conference mConference = Conference.AFC;

        public Conference Conference
        {
            get { return mConference; }
            set
            {
                //if (mConference != value)
                {
                    mConference = value;
                    OnConferenceChanged();
                }
            }
        }

        private string mData = "";
        // the Data from the main text area of the Main Gui
        public String Data 
        {
            get { return mData; }
            set { mData = value; OnDataChanged(); }
        }

        private TSBPlayer mPlayerPosition = TSBPlayer.QB1;

        public TSBPlayer PlayerPosition
        {
            get { return mPlayerPosition; }
            set
            {
                //if (mPlayerPosition != value)
                {
                    mPlayerPosition = value;
                    OnPlayerPositionChanged();
                }
            }
        }
        #endregion

        protected virtual void OnPlayerPositionChanged()
        {
            switch (mPlayerPosition)
            {
                case TSBPlayer.QB1: case TSBPlayer.QB2:
                    mLegalPositions = new String[] {"QB1","QB2"};
                    break;
                case TSBPlayer.RB1: case TSBPlayer.RB2:
                case TSBPlayer.RB3: case TSBPlayer.RB4:
                case TSBPlayer.WR1: case TSBPlayer.WR2:
                case TSBPlayer.WR3: case TSBPlayer.WR4:
                case TSBPlayer.TE1: case TSBPlayer.TE2:
                    mLegalPositions = new String[] { "RB1", "RB2", "RB3", "RB4",
                    "WR1", "WR2", "WR3", "WR4","TE1", "TE2"};
                    break;
                case TSBPlayer.LG: case TSBPlayer.LT:
                case TSBPlayer.C:
                case TSBPlayer.RG: case TSBPlayer.RT:
                    mLegalPositions = new String[] { "LT", "LG", "C", "RG", "RT" };
                    break;
                case TSBPlayer.LE: case TSBPlayer.NT: case TSBPlayer.RE:
                    mLegalPositions = new String[] { "LE", "NT", "RE" };
                    break;
                case TSBPlayer.LOLB: case TSBPlayer.LILB:
                case TSBPlayer.RILB: case TSBPlayer.ROLB:
                    mLegalPositions = new String[] { "LOLB", "LILB", "RILB","ROLB" };
                    break;
                case TSBPlayer.LCB: case TSBPlayer.RCB:
                case TSBPlayer.SS: case TSBPlayer.FS:
                    mLegalPositions = new String[] { "LCB", "RCB", "SS", "FS" };
                    break;
                case TSBPlayer.P: case TSBPlayer.K:
                    mLegalPositions = new String[] { "P", "K"};
                    break;
            }
            mUpdating = true;
            mPositionLabel.Text = mPlayerPosition.ToString();
            mPositionComboBox.BeginUpdate();
            mPositionComboBox.Items.Clear();
            mPositionComboBox.Items.AddRange(mLegalPositions);
            mPositionComboBox.EndUpdate();
            mPositionComboBox.SelectedIndex = 0;

            PopulateTeamPosComboBox();
            mUpdating = false;
        }

        private void PopulateTeamPosComboBox()
        {
            mTeamPosComboBox.BeginUpdate();
            mTeamPosComboBox.Items.Clear();
            if (TecmoTool.Teams.Count == 28)
            {
                int begin = 0;
                int end = 14;
                if (Conference == TSBTool.Conference.NFC)
                {
                    begin = 14; end = 28;
                }
                for (int i = begin; i < end; i++)
                {
                    for (int j = 0; j < mLegalPositions.Length; j++)
                    {
                        mTeamPosComboBox.Items.Add(TecmoTool.Teams[i] + " " + mLegalPositions[j]);
                    }
                }
            }
            else
            {
                // has 34 teams, skip indexes 26 & 27
                List<string> teams = TecmoTool.Teams;
                int begin = 0;
                int end = 16;
                if (Conference == TSBTool.Conference.NFC)
                {
                    begin = 16; end = 34;
                }
                for (int i = begin; i < end; i++)
                {
                    if (i != 28 && i != 29)
                    {
                        for (int j = 0; j < mLegalPositions.Length; j++)
                        {
                            mTeamPosComboBox.Items.Add(TecmoTool.Teams[i] + " " + mLegalPositions[j]);
                        }
                    }
                }
            }
            mTeamPosComboBox.SelectedIndex = 0;
            SetName();
            mTeamPosComboBox.EndUpdate();
        }

        private void OnConferenceChanged()
        {
            mUpdating = true;
            if (TecmoTool.Teams.Count == 28)
            {
                mTeamComboBox.BeginUpdate();
                mTeamComboBox.Items.Clear();
                int begin = 0;
                int end = 14;
                if (Conference == TSBTool.Conference.NFC)
                {
                    begin = 14; end = 28;
                }
                for (int i = begin; i < end; i++)
                {
                    mTeamComboBox.Items.Add(TecmoTool.Teams[i]);
                }
                mTeamComboBox.EndUpdate();
            }
            else
            {
                // has 34 teams, skip indexes 26 & 27
                mTeamComboBox.BeginUpdate();
                mTeamComboBox.Items.Clear();
                List<String> teams = TecmoTool.Teams;
                int begin = 0;
                int end = 16;
                if (Conference == TSBTool.Conference.NFC)
                {
                    begin = 16; end = 34;
                }
                for (int i = begin; i < end; i++)
                {
                    if (i != 28 && i != 29)
                    {
                        mTeamComboBox.Items.Add(teams[i]);
                    }
                }
                mTeamComboBox.EndUpdate();
            }
            mTeamComboBox.SelectedIndex = 0;
            
            PopulateTeamPosComboBox();
            
            mUpdating = false;
            SetName();
        }

        private void OnDataChanged()
        {
            // 1st chack to see if pro bowl position is in text, populate if there.
            // 2nd populate name label
            SetInitialValue();
        }


        private void SetInitialValue()
        {
            //search string ==> "AFC,QB1,dolphins,QB1"
            String searchstring = mConference.ToString() + "," + mPositionLabel.Text;
            if (mData != null)
            {
                int index = mData.IndexOf(searchstring);
                if (index > -1)
                {
                    int endLine = mData.IndexOf('\n', index + 9);
                    String line = mData.Substring(index, endLine - index);
                    Match m = ProBowlPositionRegex.Match(line);
                    if (m != Match.Empty)
                    {
                        String allStarPos = m.Groups[2].Value.ToString();
                        String team = m.Groups[3].Value.ToString();
                        String pos = m.Groups[4].Value.ToString();
                        //if (AllStarForm.CombineTeamAndPos)
                        {
                            String dude = team + " " + pos;
                            int tp_index = mTeamPosComboBox.Items.IndexOf(dude);
                            if (tp_index > -1)
                            {
                                mTeamPosComboBox.SelectedIndex = tp_index;
                            }
                        }
                        //else
                        {
                            int team_index = mTeamComboBox.Items.IndexOf(team);
                            mUpdating = true;
                            int pos_index = mPositionComboBox.Items.IndexOf(pos);
                            if (team_index > -1)
                                mTeamComboBox.SelectedIndex = team_index;
                            if (pos_index > -1)
                                mPositionComboBox.SelectedIndex = pos_index;
                            mUpdating = false;
                            //SetName();
                        }
                    }
                }
            }
        }

        private bool mUpdating = false;

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!mUpdating)
            {
                SetName();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SetName();
        }

        private void SetName()
        {
            if (mUpdating)
                return; // ===============EARLY RETURN===============

            String searchString = "TEAM = "+GetTeam();
            if (Data != null)
            {
                int teamStartIndex = Data.IndexOf(searchString);
                if (teamStartIndex > -1)
                {
                    searchString = "^" + GetPos() + ",";
                    Regex positionRegex = new Regex(searchString, RegexOptions.Multiline);
                    Match m = positionRegex.Match(Data, teamStartIndex);
                    if (m != Match.Empty)
                    {
                        int lineStart = m.Index;
                        if (lineStart > -1)
                        {
                            int lineEnd = Data.IndexOf('\n', lineStart + 5);
                            string playerString = Data.Substring(lineStart, lineEnd - lineStart);
                            mPlayerNameLabel.Text = playerString;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns a string like "AFC,QB1,bills,QB1"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            String ret = String.Format("{0},{1},{2},{3}",Conference.ToString(),this.PlayerPosition.ToString(),
                GetTeam(), GetPos());
            return ret;
        }

        private void mTeamPosComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if( !mUpdating )
                SetName();
        }

    }
}
