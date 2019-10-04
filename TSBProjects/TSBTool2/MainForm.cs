using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TSBTool2
{
    public partial class MainForm : Form
    {
        ITecmoTool tool = null;

        public MainForm()
        {
            InitializeComponent();
            state1();
        }

        private string snesFilter = "TSB files (*.smc)|*.smc";

        private void handleLoad(object sender, EventArgs e)
        {
            string filename = StaticUtils.GetFileName(snesFilter, false);
            LoadROM(filename);
        }

        private void LoadROM(string filename)
        {
            tool = new TSB2Tool(null);
            if (filename != null)
            {
                tool.Init(filename);
                if (tool.OutputRom != null)
                {
                    state2();
                    UpdateTitle(filename);
                }
                else if (tool.Init(filename))
                {
                    state2();
                    UpdateTitle(filename);
                }
                else
                    state1();
            }
        }

        private void UpdateTitle(string filename)
        {
            if (filename != null)
            {
                string fn = filename;
                int index = filename.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
                if (index > 0)
                {
                    fn = filename.Substring(index);
                }
                String type = tool.RomVersion;
                if (fn.Length > 4)
                    this.Text = string.Format("TSBTool2   '{0}' Loaded   ({1})", fn, type);
            }
        }

        private void state1()
        {
            seasonToolStripMenuItem.Enabled =
                debugToolStripMenuItem.Enabled =
                debugDialogToolStripMenuItem.Enabled =
                mViewContentsButton.Enabled =
                viewContentsToolStripMenuItem.Enabled =
                applyToROMToolStripMenuItem.Enabled =
                mApplyButton.Enabled = false;
        }

        private void state2()
        {
            seasonToolStripMenuItem.Enabled =
                debugToolStripMenuItem.Enabled =
                debugDialogToolStripMenuItem.Enabled =
                mViewContentsButton.Enabled =
                viewContentsToolStripMenuItem.Enabled =
                applyToROMToolStripMenuItem.Enabled =
                mApplyButton.Enabled = true;
        }

        private void viewContentsAction(object sender, EventArgs e) { ViewContents(); }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) { this.Close(); }

        private void ViewContents()
        {
            string text = tool.GetKey();
            if (season1ToolStripMenuItem.Checked)
                text += tool.GetAll(1);
            else if (season2ToolStripMenuItem.Checked)
                text += tool.GetAll(2);
            else if (season3ToolStripMenuItem.Checked)
                text += tool.GetAll(3);
            else if (allSeasonsToolStripMenuItem.Checked)
                text += tool.GetAll();

            SetText(text);
            mTextBox.SelectionStart = 0;
            mTextBox.SelectionLength = tool.GetKey().Length - 1;
            mTextBox.SelectionColor = Color.Magenta;
            mTextBox.SelectionStart = 0;
            mTextBox.SelectionLength = 0;
            StaticUtils.ShowErrors();
        }

        private void SetText(string text)
        {
            this.mTextBox.Text = text;
            mTextBox.SelectAll();
            mTextBox.SelectionColor = Color.Black;
            mTextBox.SelectionLength = 0;
            mTextBox.SelectionStart = 0;
        }

        private void seasonItemClicked(object sender, EventArgs e)
        {
            if (sender == this.season1ToolStripMenuItem)
            {
                allSeasonsToolStripMenuItem.Checked = season2ToolStripMenuItem.Checked = season3ToolStripMenuItem.Checked = false;
            }
            else if (sender == this.season2ToolStripMenuItem)
            {
                allSeasonsToolStripMenuItem.Checked = season1ToolStripMenuItem.Checked = season3ToolStripMenuItem.Checked = false;
            }
            else if (sender == this.season3ToolStripMenuItem)
            {
                allSeasonsToolStripMenuItem.Checked = season2ToolStripMenuItem.Checked = season1ToolStripMenuItem.Checked = false;
            }
            else if (sender == this.allSeasonsToolStripMenuItem)
            {
                season3ToolStripMenuItem.Checked = season2ToolStripMenuItem.Checked = season1ToolStripMenuItem.Checked = false;
            }
        }

        private void mApplyButton_Click(object sender, EventArgs e)
        {
            ApplyData();
        }

        private void ApplyData()
        {
            string saveToFilename = StaticUtils.GetFileName(snesFilter, true);
            if (saveToFilename != null)
            {
                string[] lines = mTextBox.Lines;
                InputParser parser = new InputParser(tool);
                parser.ProcessLines(lines);

                StaticUtils.SaveRom(saveToFilename, tool.OutputRom);
                UpdateTitle(saveToFilename);
            }
        }

        private void debugDialogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugDialog dlg = new DebugDialog(this.tool);
            dlg.Show(this);
        }

        private void aboutTSBTool2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
@"You are using the PRE-ALPHA version of TSBTool2. 
It'll get better.
It's mostly intended to assist those doing the discovery of TSB2 stuff.
It likely has bugs.
Version " + MainClass.version
                );
        }

        private void mTextBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EditStuff();
        }

        private void EditStuff()
        {
            string line = GetLine(mTextBox.SelectionStart);
            if (line == null)
                return;
            if (line.IndexOf("TEAM") > -1 || line.IndexOf("PLAYBOOK") > -1)
            {
                ModifyTeams();
            }
            //else if (line.IndexOf("COLORS") > -1)
            //{
            //    ModifyColors();
            //}
            //else if (line.StartsWith("AFC") || line.StartsWith("NFC"))
            //{
            //    mProwbowlMenuItem_Click(null, EventArgs.Empty);
            //}
            //else if (line.StartsWith("WEEK") || line.IndexOf(" at ") > -1)
            //{
            //    DisplayScheduleForm(GetWeekAtCaret());
            //}
            //else
            //    EditPlayer();
        }

        private void ModifyTeams()
        {
            string team = GetTeam(mTextBox.SelectionStart);
            ModifyTeams(team);
        }
        private string GetTeam(int textPosition)
        {
            string team = "bills";
            Regex r = new Regex("TEAM\\s*=\\s*([a-zA-Z49]+)");
            MatchCollection mc = r.Matches(mTextBox.Text);
            Match theMatch = null;

            foreach (Match m in mc)
            {
                if (m.Index > textPosition)
                    break;
                theMatch = m;
            }

            if (theMatch != null)
            {
                team = theMatch.Groups[1].Value;
            }
            return team;
        }

        private void ModifyTeams(string team)
        {
            TeamForm form = new TeamForm();
            form.Data = mTextBox.Text;
            form.CurrentTeam = team;

            if (form.ShowDialog(this) == DialogResult.OK)
            {
                int index = mTextBox.SelectionStart;
                SetText(form.Data);
                if (mTextBox.Text.Length > index)
                {
                    mTextBox.SelectionStart = index;
                    mTextBox.ScrollToCaret();
                }
            }
            form.Dispose();
        }

        /// <summary>
        /// returns the line that linePosition falls on
        /// </summary>
        /// <param name="textPosition"></param>
        /// <returns></returns>
        private string GetLine(int textPosition)
        {
            string ret = null;
            if (textPosition < mTextBox.Text.Length)
            {
                int i = 0;
                int lineStart = 0;
                int posLen = 0;
                for (i = textPosition; i > 0; i--)
                {
                    if (mTextBox.Text[i] == '\n')
                    {
                        lineStart = i + 1;
                        break;
                    }
                }
                i = lineStart;
                if (i < mTextBox.Text.Length)
                {
                    char current = mTextBox.Text[i];
                    while (i < mTextBox.Text.Length - 1 /*&& current != ' ' && 
					current != ',' */
                                      && current != '\n')
                    {
                        posLen++;
                        i++;
                        current = mTextBox.Text[i];
                    }
                    ret = mTextBox.Text.Substring(lineStart, posLen);
                }
            }
            return ret;
        }

        private void playbooksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSB2Tool.ShowPlaybooks = playbooksToolStripMenuItem.Checked;
        }

        private void simDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSB2Tool.ShowPlayerSimData = simDataToolStripMenuItem.Checked;
        }

        private void scheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSB2Tool.ShowSchedule = scheduleToolStripMenuItem.Checked;
        }

        private void convertToTSB1DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Warning! This is a destructive operation, the text will be changed to a format compatible with TSB1\nDo you wish to continue?",
                "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                if (Check1Season())
                {
                    string data = mTextBox.Text;
                    string output = TSB1Converter.ConvertToTSB1(data);
                    SetText(output);
                }
                else
                {
                    MessageBox.Show("Could not convert multiple seasons. Please have only 1 season of data in the text area.", 
                        "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool Check1Season()
        {
            string search = "SEASON ";
            int count = 0;
            int index = 0;
            while ((index = mTextBox.Text.IndexOf(search, index)) > -1)
            {
                index++;
                count++;
            }
            return count < 2;
        }

        private void convertToTSB2DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Warning! This is a destructive operation, the text will be changed to a format compatible with TSB1\nDo you wish to continue?",
                    "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                string data = mTextBox.Text;
                string output = TSB2Converter.ConvertToTSB2(data);
                SetText(output);
            }
        }

    }
}
