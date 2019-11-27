using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TSBTool
{
    public partial class DebugDialog : Form
    {
        public DebugDialog()
        {
            InitializeComponent();
            mResultsTextBox.StatusControl = mStatusLabel;
        }

        public ITecmoContent Tool { get; set; }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            int ch;
            StringBuilder builder = new StringBuilder(20);
            if ( mToUpperCheckBox.Checked && mInputTextBox.Text != mInputTextBox.Text.ToUpper())
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
            //mResultsTextBox.Text = Tool.GetTeamPlayers(mInputTextBox.Text.ToLower());
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
                byte[] bytesToSearch = new byte[val.Length];
                int j = 0;
                for (int i = 0; i < val.Length; i+=2)
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
            try
            {
                Object result = StaticUtils.Compute(mResultsTextBox.Text);
                mResultsTextBox.Text = result.ToString();
            }
            catch (Exception ex)
            {
                mResultsTextBox.Text = "Math test failed; "+ ex.ToString();
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
            if (String.IsNullOrEmpty( searchStr ))
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
                string[] lines = results.Split(new char[] {'\n'});
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

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("This dialog is used for feature testing purposes");
		}
    }
}
