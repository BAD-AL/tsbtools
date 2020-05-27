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
    public partial class MainForm : Form
    {
        ITecmoTool tool = null;

        public MainForm()
        {
            InitializeComponent();
            state1();
        }
        //"TSB files (*.nes;*.smc)|*.nes;*.smc";
        private string tsbFilter = "TSB files (*.nes;*.smc)|*.nes;*.smc";
        private string snesFilter = "TSB files (*.smc)|*.smc";

        private string FileFilter
        {
            get
            {
                if (HasTSBToolSupreme())
                    return tsbFilter;
                else
                    return snesFilter;
            }
        }

        private void handleLoad(object sender, EventArgs e)
        {
            string filename = StaticUtils.GetFileName(FileFilter, false);
            if( filename != null)
                LoadROM(filename);
        }

        private bool HasTSBToolSupreme()
        {
            return System.IO.File.Exists("TSBToolSupreme.exe");
        }

        private void LoadROM(string filename)
        {
            byte[] rom = StaticUtils.ReadRom(filename);
            if (TSB2Tool.IsTecmoSuperBowl2Rom(rom))
                tool = new TSB2Tool(rom);
            else if (TSB3Tool.IsTecmoSuperBowl3Rom(rom))
                tool = new TSB3Tool(rom);
            else if (HasTSBToolSupreme() && TSB1Tool.IsTecmoSuperBowl1Rom(rom))
            {
                if (MessageBox.Show(this, "Do you wish to display the contents of this TSB1 ROM?", "Show Contents", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string contents = TSB1Tool.GetTSB1Content(filename);
                    SetText(contents);
                    this.Text = string.Format("TSBTool2   '{0}' Loaded   ({1})", filename, 
                        StaticUtils.GetContentType(contents).ToString());
                    tool = null;
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure this is a Valid TSB2 or TSB3 ROM?",
                    "Does not seem to be a TSBII or TSBIII rom", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (MessageBox.Show("Is this a TSBII ROM?",
                    "Does not seem to be a TSBII or TSBIII rom", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        tool = new TSB2Tool(rom);
                    else
                        tool = new TSB3Tool(rom);
                }
            }
            if (tool != null && tool.OutputRom != null)
            {
                state2();
                UpdateTitle(filename);
                if (!tool.RomVersion.Contains("TSB2"))
                    seasonMenuItem.Enabled = false;
            }
            else
                state1();
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
            seasonMenuItem.Enabled =
                debugToolStripMenuItem.Enabled =
                debugDialogToolStripMenuItem.Enabled =
                mViewContentsButton.Enabled =
                viewContentsToolStripMenuItem.Enabled =
                applyToROMToolStripMenuItem.Enabled =
                mApplyButton.Enabled = false;
        }

        private void state2()
        {
            seasonMenuItem.Enabled =
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
            string key  = tool.GetKey();
            string text = "";
            if (season1MenuItem.Checked)
                text += tool.GetAll(1);
            else if (season2MenuItem.Checked)
                text += tool.GetAll(2);
            else if (season3MenuItem.Checked)
                text += tool.GetAll(3);
            else if (allSeasonsMenuItem.Checked)
                text += tool.GetAll();
            SetText(key + text);
            
            int selLen = mTextBox.Text.IndexOf("SEASON");
            if (selLen > 0)
            {
                mTextBox.SelectionStart = 0;
                mTextBox.SelectionLength = selLen - 1;
                mTextBox.SelectionColor = Color.Magenta;
                mTextBox.SelectionStart = 0;
                mTextBox.SelectionLength = 0;
            }
            StaticUtils.ShowErrors();
        }

        private void SetText(string text)
        {
            //string header = "#" + StaticUtils.GetContentType(text).ToString() +" content\n";
            this.mTextBox.Text = text;
            mTextBox.SelectAll();
            mTextBox.SelectionColor = Color.Black;
            mTextBox.SelectionLength = 0;
            mTextBox.SelectionStart = 0;
        }

        private void seasonItemClicked(object sender, EventArgs e)
        {
            if (sender == this.season1MenuItem)
            {
                allSeasonsMenuItem.Checked = season2MenuItem.Checked = season3MenuItem.Checked = false;
            }
            else if (sender == this.season2MenuItem)
            {
                allSeasonsMenuItem.Checked = season1MenuItem.Checked = season3MenuItem.Checked = false;
            }
            else if (sender == this.season3MenuItem)
            {
                allSeasonsMenuItem.Checked = season2MenuItem.Checked = season1MenuItem.Checked = false;
            }
            else if (sender == this.allSeasonsMenuItem)
            {
                season3MenuItem.Checked = season2MenuItem.Checked = season1MenuItem.Checked = false;
            }
        }

        private void mApplyButton_Click(object sender, EventArgs e)
        {
            ApplyData();
        }

        private void ApplyData()
        {
            int index = mTextBox.Text.IndexOf("QB1,");
            if (index > -1)
            {
                string contentType = StaticUtils.GetContentType(mTextBox.Text).ToString();
                if (!tool.RomVersion.Contains(contentType))
                {
                    if (MessageBox.Show(String.Format(
@"The content type shows as '{0}'. The ROM loaded is of type '{1}'
It is recommended that you try to convert (Convert menu) the content to {1}.
This will probably not go well, do you wish to try anyway?",
                        contentType, tool.RomVersion), "Warning",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
                    }
                }
            }
            string saveToFilename = StaticUtils.GetFileName(FileFilter, true);
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
@"You are using the ALPHA version of TSBTool2.
It works on the SNES version of TSB2 & TSB3.
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
                EditTeams();
            }
            else if (line.StartsWith("AFC") || line.StartsWith("NFC"))
                EditProBowlPlayers();
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
            else
                EditPlayer();
        }

        private void EditTeams()
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
            TSBContentType t = StaticUtils.GetContentType(this.mTextBox.Text);
            if (t == TSBContentType.TSB2 || t == TSBContentType.TSB3)
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
            else if (t == TSBContentType.TSB1 && HasTSBToolSupreme())
            {
                MessageBox.Show("Cannot edit TSB1 teams in TSBTool2.\n     (But you can edit players)");
            }
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

        private void proBowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TSB2Tool.ShowProBowlRosters = proBowlToolStripMenuItem.Checked;
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

        private void tsb2ToTsb1tem_Click(object sender, EventArgs e)
        {
            if (!StaticUtils.IsTSB2Content(mTextBox.Text))
            {
                DialogResult result = MessageBox.Show("The text does not appear to be TSB2 Content. Do you still want to continue?",
                    "Incorrect content detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }
            if (MessageBox.Show("Warning! This is a destructive operation, the text will be changed to a format compatible with TSB1\nDo you wish to continue?",
                "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                if (Check1Season())
                {
                    string output = TSB1Converter.ConvertToTSB1FromTSB2(mTextBox.Text);
                    SetText(output);
                }
                else
                {
                    MessageBox.Show("Could not convert multiple seasons. Please have only 1 season of data in the text area.", 
                        "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tsb1ToTsb2Item_Click(object sender, EventArgs e)
        {
            if (!StaticUtils.IsTSB1Content(mTextBox.Text))
            {
                DialogResult result = MessageBox.Show("The text does not appear to be TSB1 Content. Do you still want to continue?", 
                    "Incorrect content detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }
            if (MessageBox.Show("Warning! This is a destructive operation, the text will be changed to a format compatible with TSB2\nDo you wish to continue?",
                    "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                string output = TSB2Converter.ConvertToTSB2FromTSB1(mTextBox.Text);
                SetText(output);
            }
        }


        private void tsb3ToTsb2Item_Click(object sender, EventArgs e)
        {
            if (!StaticUtils.IsTSB3Content(mTextBox.Text))
            {
                DialogResult result = MessageBox.Show("The text does not appear to be TSB3 Content. Do you still want to continue?",
                    "Incorrect content detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }
            if (MessageBox.Show("Warning! This is a destructive operation, the text will be changed to a format compatible with TSB2\nDo you wish to continue?",
                    "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                string output = TSB2Converter.ConvertToTSB2FromTSB3(mTextBox.Text);
                SetText(output);
            }
        }


        private void tsb2ToTsb3Item_Click(object sender, EventArgs e)
        {
            if (!StaticUtils.IsTSB2Content(mTextBox.Text))
            {
                DialogResult result = MessageBox.Show("The text does not appear to be TSB2 Content. Do you still want to continue?",
                    "Incorrect content detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }
            if (MessageBox.Show("Warning! This is a text change operation, the text will be changed to a format compatible with TSB3\nDo you wish to continue?",
                    "Continue?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string output = TSB3Converter.ConvertToTSB3FromTSB2(mTextBox.Text);
                    SetText(output);
                }
                catch(Exception ex)
                {
                    StaticUtils.ShowError(ex.ToString());
                }
            }
        }

        private void aboutConvertingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(TSB1Converter.CONVERT_MSG);
        }

        private void EditPlayer()
        {
            int pos = mTextBox.SelectionStart;
            int lineStart = 0;
            int posLen = 0;
            string position = "QB1";
            string team = "bills";

            if (pos > 0 && pos < mTextBox.Text.Length)
            {
                int i = 0;
                for (i = pos; i > 0; i--)
                {
                    if (mTextBox.Text[i] == '\n')
                    {
                        lineStart = i + 1;
                        break;
                    }
                }
                i = lineStart;
                char current = mTextBox.Text[i];
                while (i < mTextBox.Text.Length && current != ' ' &&
                    current != ',' && current != '\n')
                {
                    posLen++;
                    i++;
                    current = mTextBox.Text[i];
                }
                position = mTextBox.Text.Substring(lineStart, posLen);

                team = GetTeam(pos);
                ModifyPlayers(team, position);
            }
        }

        private void ModifyPlayers(string team, string position)
        {
            ModifyPlayerForm form = new ModifyPlayerForm();
            form.RomVersion = StaticUtils.GetContentType(mTextBox.Text);

            form.Data = mTextBox.Text;
            form.CurrentTeam = team;
            form.CurrentPosition = position;
            
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                int spot2 = mTextBox.SelectionStart;
                SetText(form.Data);
                if (mTextBox.Text.Length > spot2)
                {
                    mTextBox.SelectionStart = spot2;
                }
            }
        }

        private void editPlayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditPlayer();
        }

        private void editTeamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditTeams();
        }

        private void EditProBowlPlayers()
        {
            try
            {
                AllStarForm form = new AllStarForm();
                form.Data = mTextBox.Text;
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SetText(form.Data);
                }
                form.Dispose();
            }
            catch (Exception err)
            {
                MessageBox.Show(String.Concat("Error in ALLStarForm. \n", err.Message, "\n", err.StackTrace), "Error!!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadTextFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = ".";
            string fileName = null;
            if (dlg.ShowDialog(this) == DialogResult.OK)
                fileName = dlg.FileName;
            dlg.Dispose();

            if (fileName != null)
            {
                this.mTextBox.LoadFile(fileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void saveTextFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = ".";
            string fileName = null;
            if (dlg.ShowDialog(this) == DialogResult.OK)
                fileName = dlg.FileName;
            dlg.Dispose();

            if (fileName != null)
            {
                this.mTextBox.SaveFile(fileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void editProBowlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditProBowlPlayers();
        }
    }
}
