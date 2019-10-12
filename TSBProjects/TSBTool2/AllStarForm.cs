using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSBTool2
{
    public partial class AllStarForm : Form
    {
        public static bool CombineTeamAndPos = true;

        public AllStarForm()
        {
            InitializeComponent();
            SetButtonText();
        }

        private void SetButtonText()
        {
            if (CombineTeamAndPos)
            {
                mToggleNumberOfComboBoxesButton.Text = "1 Combo box";
            }
            else
            {
                mToggleNumberOfComboBoxesButton.Text = "2 Combo boxes";
            }
        }

        private string mData = "";

        /// <summary>
        /// The Text / player data from main gui
        /// </summary>
        public String Data 
        { 
            get { return mData; }
            set
            {
                mData = value;
                mAFCConfrenceControl.Data = mData;
                mNFCConfrenceControl.Data = mData;
            }
        }

        public override string ToString()
        {
            return String.Concat(
                mAFCConfrenceControl.ToString(),
                "\r\n\r\n",
                mNFCConfrenceControl.ToString());

        }

        private void mToggleNumberOfComboBoxesButton_Click(object sender, EventArgs e)
        {
            CombineTeamAndPos = !CombineTeamAndPos;
            mAFCConfrenceControl.ReInitialize();
            mNFCConfrenceControl.ReInitialize();
            SetButtonText();
        }

        private void mOKButton_Click(object sender, EventArgs e)
        {
            String newData = mData;
            SubstituteProbowlTeam(Conference.AFC, mAFCConfrenceControl.ToString());
            
            String nfcGuys = mNFCConfrenceControl.ToString();
            if( nfcGuys.IndexOf("QB1") > -1 )
                SubstituteProbowlTeam(Conference.NFC, nfcGuys);

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void SubstituteProbowlTeam(Conference conf, String newRoster)
        {
            int index = mData.IndexOf( String.Format("# {0} ProBowl players", conf.ToString()));
            int endIndex = -1;
            if (index < 0)
            {
                index = mData.IndexOf(String.Format("{0},QB1", conf.ToString()));
            }
            if (index > 0)
            {
                endIndex = mData.LastIndexOf(conf.ToString()+",");
                if (endIndex > 0)
                    endIndex = mData.IndexOf("\n", endIndex);
            }
            if (endIndex > 0 && index > 0)
            {
                StringBuilder builder = new StringBuilder(mData.Length + 60);
                builder.Append(mData.Substring(0, index));
                builder.Append(newRoster);
                builder.Append(mData.Substring(endIndex + 1));
                mData = builder.ToString();
            }
        }
    }
}
