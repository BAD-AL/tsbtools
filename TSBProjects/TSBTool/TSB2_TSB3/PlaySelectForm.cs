using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TSBTool2;

namespace TSBTool2_UI
{
    public partial class PlaySelectForm : Form
    {
        private const int play_width = 50;
        private const int play_height = 59;

        private static Point[] runImageLocs = new Point[]{
                new Point(166, 28), // Run 1
                new Point(135, 93), // Run 2
                new Point(199, 93), // Run 3
                new Point(166, 159) // Run 4
        };
        //private static Point run1ImageLoc = new Point(166, 28);
        //private static Point run2ImageLoc = new Point(135, 93);
        //private static Point run3ImageLoc = new Point(199, 93);
        //private static Point run4ImageLoc = new Point(166, 159);

        private static Point[] passImageLocs = new Point[]{
                new Point(39, 28), //Pass 1
                new Point(8, 93),  //Pass 2
                new Point(71, 93), //Pass 3
                new Point(39, 159) //Pass 4
        };

        private List<PictureBox> pictureBoxes = null;
        private List<Label> playLabels = null; 

        private static List<String> fileNames = new List<string>(){
            "TSB2_PB_000.png", "TSB2_PB_001.png", "TSB2_PB_002.png",
            "TSB2_PB_003.png", "TSB2_PB_004.png", "TSB2_PB_005.png",
            "TSB2_PB_006.png", "TSB2_PB_007.png", "TSB2_PB_008.png",
            "TSB2_PB_009.png", "TSB2_PB_010.png", "TSB2_PB_011.png",
            "TSB2_PB_012.png", "TSB2_PB_013.png", "TSB2_PB_014.png",
            "TSB2_PB_015.png"};

        
        private static string[][] runPlayNames = new string[][]{ 
            new string[] {"T Power Sweep R", "T Fake Pitch", "T Lead Off T", "R&&S 3Wings Open","1 Back Off T", "1 Back Sweep R", "1 Back Daylight", "I Pitch Strong", "I OffTackle", "Off I Sweep R",  "Off I OffTackle", "2TE OffTackle R", "2TE Sweep R",  "R&&S Sweep R",   "1 Motion Open R", "3 Shift Strong"},
            new string[] {"T Sweep Strong",  "T Power Off C", "T Lead Strong","T Open Weak", "1 Back Rev Fake", "1Back Off Guard", "1Back Open Weak", "I Off Center", "OffI Off Center", "2TE Off T L","2TE Sweep",       "R&&S Open L",      "R&&S Draw L",   "1 Motion Weak", "3 Shift Weak",    "T Daylight"},
            new string[] {"T Lead Week",     "T Sweep Weak",  "T HB Draw", "SGun T HB Draw", "1 Back FB OffC", "1 Back FB Sweep", "1 Back Draw", "I Pitch Weak",      "2TE Off Center",  "R&&S Draw",   "T Motion Off T",  "I Motion Off T",  "3 Shift R Rev","T Motion Dive", "SGun HP Draw",    "SGun Sweep L"},
            new string[] {"T FB Off Center", "T X Reverse",  "T FB Strong", "1Back FB Strong", "1Back X Reverse", "R&&S QB Sweep", "R&&S QB Sneak", "3 Shift Off C",    "T Motion Blast", "SGun FB Draw", "SGun QB Sweep",  "1 Back Off C",    "T Dive",       "I Dive",        "Off I Dive",      "2TE Dive"},
          };
        private static string[][] passPlayNames = new string[][]{
            new string[] {"T Wagg L",     "T Waggl R",       "T PA FB Off C",  "T HB Screen A", "T X Catch&&Go", "1 Back PA Post", "1 Back Cross",    "1 Back Hook",   "I Quick Slant", "Off I PA Off C", "2TE Short Out", "R&&S Flare",       "R&&S Cross Post", "1Motion PA Pass","3Shift Roll Out", "T Motion All In"},
            new string[] {"T Flare",      "T Z Hook",       "T HB Screen B", "T PA Strong", "1 Back Flare", "1 Back PA Off C",    "I Flare",         "Off I Flare",   "2TE Flicker",   "R&&S Cross Fly","R&&S Short Hook",  "1Motion HP Loop", "Cross Pass",     "T Motion PA R", "SGun All Curl",    "SGun Cross Out"}, 
            new string[] {"T FB Screen",  "T Flea Flicker",  "T Cross Bomb", "1Back PA Streak", "1 Back TE Block", "1 Back Short", "I PA Off Tackle", "Off I Flood",  "2TE HP Loop",   "R&&S Deep Fly", "R&&S Short Out",   "1Motion Flicker", "3Shift PA Off C", "SGun HB Bomb", "SGun Flood R",     "SGun T X Curl"},
            new string[] {"T Double Flat","1 Back In && Fly", "Rev Flicker", "2TE PA Off T L", "R&&S Z Look In", "R&&S Y Flat",       "No Back X Deep", "T Motion Short", "SGun Slant In", "SGun Draw Fake", "SGun GHail Mary", "Red Gun",      "T Dive Fake",    "I Dive Fake",   "Off I Dive Fake",  "2TE Dive Fake"},
          };

        private string SelectedPlay = null;

        public bool ShowEditBox
        {
            get { return mSlotTextBox.Visible; }
            set
            {
                mSlotTextBox.Visible = value;
                mSlotTextBox.Invalidate();
            }
        }

        /// <summary>
        /// Prompts the user to select a play for the Given Play slot.
        /// </summary>
        /// <param name="playSlot">The play slot. Valid play slots: ("R1","R3","R4", "P1","P3","P4")</param>
        /// <returns>the selected paly for the given 
        /// slot on OK. Posible results are (null, "0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F").
        /// Returns Null on 'Cancel'</returns>
        public static string GetPlay(string playSlot, Form owner)
        {
            string retVal = null;
            if (validPlaySlots.IndexOf(playSlot) > -1)
            {
                PlaySelectForm form = new PlaySelectForm();
                form.PlaySlot = playSlot;
                if (form.ShowDialog(owner) == DialogResult.OK)
                    retVal = form.SelectedPlay;
                form.Dispose();
            }
            else
            {
                MessageBox.Show("Invalid Play Slot:" + playSlot);
            }
            return retVal;
        }

        public PlaySelectForm()
        {
            InitializeComponent();
            pictureBoxes = new List<PictureBox>(){
                pictureBox1, pictureBox2, pictureBox3, pictureBox4,
                pictureBox5, pictureBox6, pictureBox7, pictureBox8,
                pictureBox9, pictureBox10, pictureBox11, pictureBox12,
                pictureBox13, pictureBox14, pictureBox15, pictureBox16};
            playLabels = new List<Label>() { 
                label1, label2, label3, label4,
                label5, label6, label7, label8,
                label9, label10, label11, label12,
                label13, label14, label15, label16};
            LoadPlays();
        }

        private static List<String> validPlaySlots = new List<string>() { 
         "R1","R2","R3","R4", "P1","P2","P3","P4"
                                          };
        private String mPlaySlot = "R1";

        public String PlaySlot
        {
            get { return mPlaySlot; }
            set
            {
                if (validPlaySlots.IndexOf(value) > -1)
                {
                    mSlotTextBox.Text = 
                    mPlaySlot = value;
                    LoadPlays();
                }
            }
        }

        // Load all run or pass plays for the given slot
        // need to go through each playbook image and crop the right spot
        private void LoadPlays()
        {
            Point selection = runImageLocs[0];
            string[] playNames = null;
            char type = PlaySlot[0];
            int slot = 0;
            Int32.TryParse(PlaySlot[1] + "", out slot);
            if (type == 'R')
            {
                selection = runImageLocs[slot - 1];
                playNames = runPlayNames[slot - 1];
            }
            else
            {
                selection = passImageLocs[slot - 1];
                playNames = passPlayNames[slot - 1];
            }
            //switch (type)
            //{
            //    case 'R':
            //        switch (slot)
            //        {
            //            case '1': selection = runImageLocs[0]; playNames = runPlayNames[0]; break;
            //            case '2': selection = runImageLocs[1]; playNames = runPlayNames[1]; break;
            //            case '3': selection = runImageLocs[2]; playNames = runPlayNames[2]; break;
            //            case '4': selection = runImageLocs[3]; playNames = runPlayNames[3]; break;
            //        }
            //        break;
            //    default:
            //        switch (slot)
            //        {
            //            case '1': selection = passImageLocs[0]; playNames = passPlayNames[0]; break;
            //            case '2': selection = passImageLocs[1]; playNames = passPlayNames[1]; break;
            //            case '3': selection = passImageLocs[2]; playNames = passPlayNames[2]; break;
            //            case '4': selection = passImageLocs[3]; playNames = passPlayNames[3]; break;
            //        } 
            //        break;
            //}
            LoadImages(selection);
            SetPlayNames(playNames);
        }

        private void SetPlayNames(string[] playNames)
        {
            for (int i = 0; i < playLabels.Count; i++)
                playLabels[i].Text = playNames[i];
        }

        private void LoadImages(Point selection)
        {
            Image baseImage = null;
            Bitmap current = null;
            Rectangle src = new Rectangle(selection.X, selection.Y, play_width, play_height);
            Rectangle dest = new Rectangle(0,0,play_width, play_height);
            for (int i = 0; i < pictureBoxes.Count; i++)
            {
                baseImage = TSBTool.MainClass.GetImage("TSBTool.TSB2_TSB3.Playbook." + fileNames[i]);
                current = new Bitmap(play_width, play_height);
                Graphics g = Graphics.FromImage(current);
                g.DrawImage(baseImage, dest, src, GraphicsUnit.Pixel);
                pictureBoxes[i].Image = current;
                pictureBoxes[i].Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playType">Should be either "P" or "R"</param>
        /// <param name="slot">1-4</param>
        /// <param name="playNumber">Valid args :["0","1","2","3","4","5","6","7","8","9","A","B","C","D","E","F"]</param>
        /// <returns></returns>
        internal static Image GetPlayImage(string playType, int slot, int playNumber)
        {
            if (( playType != "P" && playType != "R") || playNumber > 0xF ) return null;

            Point selection = runImageLocs[0];
            if (playType == "R")
                selection = runImageLocs[slot - 1];
            else
                selection = passImageLocs[slot - 1];
            Image baseImage = null;
            Bitmap current = null;
            Rectangle src = new Rectangle(selection.X, selection.Y, play_width, play_height);
            Rectangle dest = new Rectangle(0, 0, play_width, play_height);

            baseImage = TSBTool.MainClass.GetImage("TSBTool.TSB2_TSB3.Playbook." + fileNames[playNumber]);
            current = new Bitmap(play_width, play_height);
            Graphics g = Graphics.FromImage(current);
            g.DrawImage(baseImage, dest, src, GraphicsUnit.Pixel);
            return current;
        }

        private void mSlotTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) PlaySlot = mSlotTextBox.Text;
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            SelectedPlay = pb.Tag.ToString();
            this.Text = PlaySlot + ":" + SelectedPlay;
            this.DialogResult = DialogResult.OK;
        }
    }
}
