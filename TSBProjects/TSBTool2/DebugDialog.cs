using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSBTool2
{
    public partial class DebugDialog : Form
    {
        public DebugDialog(ITecmoTool tool)
        {
            InitializeComponent();
            mResultsTextBox.StatusControl = mStatusLabel;
            this.Tool = tool;
            mSetByteLocUpDown.Maximum = Tool.OutputRom.Length;
        }

        public ITecmoTool Tool { get; set; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            int ch;
            StringBuilder builder = new StringBuilder(20);
            if (mToUpperCheckBox.Checked && mInputTextBox.Text != mInputTextBox.Text.ToUpper())
            {
                mInputTextBox.Text = mInputTextBox.Text.ToUpper();
                mInputTextBox.SelectionStart = mInputTextBox.Text.Length;
                return;
            }
            builder.Append("0x");
            foreach (char c in mInputTextBox.Text)
            {
                if (c == '*')
                    ch = 0;
                else
                    ch = (int)c;
                builder.Append(ch.ToString("X2"));
            }
            textBox2.Text = builder.ToString();
        }

        private void mFindButton_Click(object sender, EventArgs e)
        {
            FindLocations();
        }

        private void FindLocations()
        {
            mResultsTextBox.Clear();
            List<long> locs = StaticUtils.FindStringInFile(mInputTextBox.Text, Tool.OutputRom, 0, Tool.OutputRom.Length);
            foreach (int loc in locs)
            {
                mResultsTextBox.AppendText(String.Format("0x{0:x}\n", loc));
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FindLocations();
                e.Handled = true;
            }
        }

        private void mGetTeamButton_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Text = Tool.GetTeamPlayers(1, mInputTextBox.Text.ToLower());
            StaticUtils.ShowErrors();
        }

        private void mSetByteLocUpDown_ValueChanged(object sender, EventArgs e)
        {
            mSetByteValTextBox.Text = Tool.OutputRom[(int)mSetByteLocUpDown.Value].ToString("X2");
        }

        private void SetBytes()
        {
            byte b1 = 0;
            int loc = (int)mSetByteLocUpDown.Value;
            try
            {
                for (int i = 0; i < mSetByteValTextBox.Text.Length; i += 2)
                {
                    b1 = (byte)UInt16.Parse(mSetByteValTextBox.Text.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                    Tool.OutputRom[loc] = b1;
                    loc++;
                }
            }
            catch
            {
                mStatusLabel.Text = "Set Byte error.";
            }
        }

        private void mSetByteButton_Click(object sender, EventArgs e)
        {
            SetBytes();
        }

        private void mSetByteValTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetBytes();
            }
        }

        private void mFindBytesButton_Click(object sender, EventArgs e)
        {
            FindBytesInFile();
        }

        private void FindBytesInFile()
        {
            string val = mInputTextBox.Text;
            try
            {
                string part = "";
                if (val.StartsWith("0x"))
                    val = val.Substring(2);
                byte[] bytesToSearch = new byte[val.Length / 2];
                int j = 0;
                for (int i = 0; i < val.Length; i += 2)
                {
                    part = val.Substring(i, 2);
                    bytesToSearch[j++] = Byte.Parse(part, System.Globalization.NumberStyles.AllowHexSpecifier);
                }
                mResultsTextBox.Clear();
                List<long> locs = StaticUtils.FindByesInFile(bytesToSearch, Tool.OutputRom, 0, Tool.OutputRom.Length);
                foreach (int loc in locs)
                {
                    mResultsTextBox.AppendText(String.Format("{0:x}\n", loc));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not search for: " + mInputTextBox.Text +
                    ". Ensure you are searching with valid characters [0-9A-F], even number of characters",
                    "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void mathTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable tab = new DataTable();
            try
            {
                Object result = tab.Compute(mResultsTextBox.Text, "");
                mResultsTextBox.Text = result.ToString();
            }
            catch (Exception ex)
            {
                mResultsTextBox.Text = "Math test failed; " + ex.ToString();
            }
        }


        private void mGetBytesButton_Click(object sender, EventArgs e)
        {
            try
            {
                int length = Int32.Parse(mGetBytesTextBox.Text, System.Globalization.NumberStyles.AllowHexSpecifier);
                int start = (int)mSetByteLocUpDown.Value;
                int end = start + length;
                StringBuilder builder = new StringBuilder(length * 3);
                for (int i = start; i < end; i++)
                {
                    builder.Append(string.Format("{0:X2} ", Tool.OutputRom[i]));
                }
                mResultsTextBox.Text = builder.ToString();
            }
            catch (Exception ex)
            {
                mStatusLabel.Text = ex.Message;
            }
        }

        private void replaceStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchStr = StringInputDlg.GetString("Enter Search String", "");
            if (String.IsNullOrEmpty(searchStr))
                return;

            string replaceStr = StringInputDlg.GetString("Enter String to replace it with", "");
            if (String.IsNullOrEmpty(replaceStr))
                return;

            string msg = StaticUtils.ReplaceStringInRom(Tool.OutputRom, searchStr, replaceStr, -1);
            if (msg.StartsWith("Error"))
                MessageBox.Show(msg);
            else
                mResultsTextBox.Text = msg;
        }


        private void findStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string results = RichTextDisplay.ShowMessage("Enter Strings to search for:", "", SystemIcons.Question, true, true);
            if (results != null)
            {
                results = results.Replace("\r\n", "\n");
                string[] lines = results.Split(new char[] { '\n' });
                StringBuilder builder = new StringBuilder(500);
                foreach (string line in lines)
                {
                    if (line == "")
                        continue;
                    builder.Append(line + ": ");
                    List<long> locs = StaticUtils.FindStringInFile(line, Tool.OutputRom, 0, Tool.OutputRom.Length);
                    foreach (int loc in locs)
                    {
                        builder.Append(String.Format("0x{0:x},", loc));
                    }
                    builder.Append("\n");
                }
                mResultsTextBox.Text = builder.ToString();
            }
        }

        private void replaceStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string instructions =
@"#Use a syntax like:
STRING1_TO_FIND:STRING1_TO_REPLACE:Occurance
SAN DIEGO:L.A.
LOS ANGELES:OAKLAND:2
";
            string results = RichTextDisplay.ShowMessage("Replace Strings", instructions, SystemIcons.Question, true, true);
            if (results != null)
            {
                string msg = null;
                StringBuilder builder = new StringBuilder();

                results = results.Replace("\r\n", "\n");
                string[] lines = results.Split("\n".ToCharArray());
                foreach (string line in lines)
                {
                    if (line == "" || line.Trim().StartsWith("#"))
                        continue;
                    string[] parts = line.Trim().Split(":".ToCharArray());
                    if (parts.Length > 3)
                    {
                        StaticUtils.AddError(String.Format("Error! Too many ':' characters on line>{0}", line));
                    }
                    else
                    {
                        int occur = -1;
                        if (parts.Length > 2)
                        {
                            Int32.TryParse(parts[2], out occur);
                            occur--; // adjust because users think of '2' as the second occurence
                        }
                        msg = StaticUtils.ReplaceStringInRom(Tool.OutputRom, parts[0], parts[1], occur);
                        if (msg.StartsWith("Error"))
                            StaticUtils.AddError(msg);
                        else
                            builder.Append(msg);
                    }
                }
                mResultsTextBox.Text = builder.ToString();
                StaticUtils.ShowErrors();
            }
        }

        private void stringTableGetTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string team = StringInputDlg.GetString("Enter Team", "");
            if (String.IsNullOrEmpty(team))
                return;
            int index = TSB2Tool.GetTeamIndex(team);

            if (index > -1)
            {
                mResultsTextBox.Text = String.Format("Team = {0}; TEAM_ABB:{1}; TEAM_CITY:{2}; TEAM_NAME:{3}\n",
                    team, Tool.GetTeamAbbreviation(index), Tool.GetTeamCity(index), Tool.GetTeamName(index));
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This dialog is used for feature testing purposes");
        }

        private string GetStringTableString_TSB2(int string_index, int firstPointer, int offset)
        {
            string retVal = "";
            //int firstPointer = 0x1e8038;
            int pointer = string_index * 2 + firstPointer;
            int length = -1;
            int location = this.GetStringTableStringLocation(pointer, out length, offset);
            if (length > 0)
            {
                StringBuilder builder = new StringBuilder(length + 3);
                for (int i = location; i < location + length; i++)
                {
                    if (builder.Length == 0)
                        builder.Append(String.Format("{0:x} ", Tool.OutputRom[i]));
                    else
                        builder.Append((char)Tool.OutputRom[i]);
                }
                retVal = builder.ToString();
            }
            return retVal;
        }

        private int GetStringTableStringLocation(int pointerLocation, out int length, int offset)
        {
            // QB1 Bills  name Start = 0x1e8853
            // StringTable Start     = 0x1e8000  // 38 80 82 80 
            int pointer_loc = pointerLocation; // 0x1e8000; // 1st pointer? team_string_table_loc + 2 * stringIndex;
            byte b1 = Tool.OutputRom[pointer_loc + 1];
            byte b2 = Tool.OutputRom[pointer_loc];
            byte b3 = Tool.OutputRom[pointer_loc + 3]; // b3 & b4 for length
            byte b4 = Tool.OutputRom[pointer_loc + 2];
            length = ((b3 << 8) + b4) - ((b1 << 8) + b2);
            int pointerVal = (b1 << 8) + b2;
            int stringStartingLocation = pointerVal + offset;// 0x1e0000;
            return stringStartingLocation;
        }

        private void tSB2StringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Clear();

            this.mResultsTextBox.AppendText("YEAR 1\n");
            this.mResultsTextBox.AppendText(GetTSB2PlayerStrings(1));
            this.mResultsTextBox.AppendText("YEAR 2\n");
            this.mResultsTextBox.AppendText(GetTSB2PlayerStrings(2));
            this.mResultsTextBox.AppendText("YEAR 3\n");
            this.mResultsTextBox.AppendText(GetTSB2PlayerStrings(3));
        }

        int tsb2_num_players = 1035;
        // name_string_table_1
        int tsb2_name_string_table_1_first_ptr = 0x1e8038;
        int tsb2_name_string_table_1_offset = 0x1e0000;

        // name string table 2
        int tsb2_name_string_table_2_first_ptr = 0x1f0038;
        int tsb2_name_string_table_2_offset = 0x1e8000;

        // name string table 3
        int tsb2_name_string_table_3_first_ptr = 0x1f8038;
        int tsb2_name_string_table_3_offset = 0x1f0000;

        private string GetTSB2PlayerStrings(int year)
        {
            if (year < 1 || year > 3)
                throw new ArgumentException("Invalid parameter value for 'year'. Must be 1, 2 or 3");
            string retVal = "";
            StringBuilder builder = new StringBuilder(tsb2_num_players * 18);

            int first_ptr = tsb2_name_string_table_1_first_ptr;
            int offset = tsb2_name_string_table_1_offset;
            switch (year)
            {
                case 2:
                    first_ptr = tsb2_name_string_table_2_first_ptr;
                    offset = tsb2_name_string_table_2_offset;
                    break;
                case 3:
                    first_ptr = tsb2_name_string_table_3_first_ptr;
                    offset = tsb2_name_string_table_3_offset;
                    break;
            }
            string current = "";
            for (int i = 0; i <= tsb2_num_players; i++)
            {
                current = GetStringTableString_TSB2(i, first_ptr, offset);
                builder.Append(current);
                builder.Append("\n");
            }
            retVal = builder.ToString();
            return retVal;
        }

        private void convertAttributesToBytesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string input = StringInputDlg.GetString("enter value", "Enter space seperate attributes");
            input = input.Replace(",", " ").Replace("\t", " ");
            string[] parts = input.Split(" ".ToCharArray());
            List<byte> bytes = new List<byte>(10);
            foreach (string p in parts)
                bytes.Add(GetAbility(p));
            string result = "";
            for (int i = 0; i < bytes.Count - 1; i += 2)
            {
                result += String.Format("{0:x2} ", (bytes[i] << 4) + bytes[i + 1]);
            }
            mResultsTextBox.Text = result + "\n";
        }

        private byte GetAbility(string ab)
        {
            byte ret = 0;
            switch (ab)
            {
                case "6": ret = 0x00; break;
                case "13": ret = 0x01; break;
                case "19": ret = 0x02; break;
                case "25": ret = 0x03; break;
                case "31": ret = 0x04; break;
                case "38": ret = 0x05; break;
                case "44": ret = 0x06; break;
                case "50": ret = 0x07; break;
                case "56": ret = 0x08; break;
                case "63": ret = 0x09; break;
                case "69": ret = 0x0a; break;
                case "75": ret = 0x0b; break;
                case "81": ret = 0x0c; break;
                case "88": ret = 0x0d; break;
                case "94": ret = 0x0e; break;
                case "100": ret = 0x0f; break;
            }
            return ret;
        }

        private void listTSB2MenuItem_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Clear();
            mResultsTextBox.AppendText("YEAR 1\n");
            mResultsTextBox.AppendText(Tool.GetTeamStuff(1));
            mResultsTextBox.AppendText(Tool.GetSchedule(1));

            mResultsTextBox.AppendText("YEAR 2\n");
            mResultsTextBox.AppendText(Tool.GetTeamStuff(2));
            mResultsTextBox.AppendText(Tool.GetSchedule(2));

            mResultsTextBox.AppendText("YEAR 3\n");
            mResultsTextBox.AppendText(Tool.GetTeamStuff(3));
            mResultsTextBox.AppendText(Tool.GetSchedule(3));
        }

        private void playBookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string team = StringInputDlg.GetString("Get Playbook", "Which Team?", "bills");
            int index = TSB2Tool.teams.IndexOf(team);
            if (index > -1 || team.ToUpper() == "AFC" || team.ToUpper() == "NFC")
            {
                mResultsTextBox.Clear();
                mResultsTextBox.AppendText(team +"\n");
                mResultsTextBox.AppendText(((TSB2Tool)Tool).GetPlaybook(1, team));
            }
            else
                MessageBox.Show(team,"Invalid team!");
            for (int i = 0; i < 17; i ++)
            {
                mResultsTextBox.AppendText(String.Format("TEAM = {0}\n", TSB2Tool.teams[i]));
                mResultsTextBox.AppendText(String.Format(
                    "PLAYBOOK R{0:X}{0:X}{0:X}{0:X}{0:X}{0:X}{0:X}{0:X}, P{1:X}{1:X}{1:X}{1:X}{1:X}{1:X}{1:X}{1:X}\n", i, i ));
            }
        }

        private void playSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*mResultsTextBox.Clear();
            string result =  PlaySelectForm.GetPlay("R1");
            mResultsTextBox.AppendText("Selected play:" + result +"\n");*/

            PlaySelectForm form = new PlaySelectForm();
            form.ShowEditBox = true;
            form.Show(this);
        }
    }
}
