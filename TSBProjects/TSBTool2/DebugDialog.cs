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
                byte[] bytesToSearch = new byte[val.Length/2];
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
            DataTable tab = new DataTable();
            try
            {
                Object result = tab.Compute(mResultsTextBox.Text, "");
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

        private void stringTableTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
			//StringBuilder builder = new StringBuilder();
			//for (int i = 0; i < Tool.NumberOfStringsInTeamStringTable; i++)  //119 for 28 team rom, 123 for 32-team ROM
			//{
			//    builder.Append(String.Format("tool.GetTeamStringTableString({0}):{1}\n", i, this.Tool.GetTeamStringTableString(i)));
			//}
			//mResultsTextBox.Text = builder.ToString();
        }

        private void stringTableSetTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
			//string indexStr = StringInputDlg.GetString("Enter string index", "");
			//if (String.IsNullOrEmpty(indexStr))
			//    return;
			//int index =-1;
			//Int32.TryParse(indexStr, out index);
			//string oldValue = this.Tool.GetTeamStringTableString(index);

			//string replaceStr = StringInputDlg.GetString(String.Format("String is {0} ",oldValue) , "Enter String to replace it with>");
			//if (String.IsNullOrEmpty(replaceStr))
			//    return;

			//this.Tool.SetTeamStringTableString(index, replaceStr);
			//stringTableTestToolStripMenuItem_Click(sender, e);
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

		private void runTestsToolStripMenuItem_Click(object sender, EventArgs e)
		{

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
					if(builder.Length == 0)
						builder.Append(String.Format("{0:x} ",Tool.OutputRom[i]));
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
			for (int i = 0; i < bytes.Count-1; i+=2)
			{
				result += String.Format("{0:x2} ", (bytes[i] << 4) + bytes[i + 1]);
			}
			mResultsTextBox.Text = result+ "\n";
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
			TSB2Tool twoTool = new TSB2Tool(Tool.OutputRom);
			mResultsTextBox.AppendText("YEAR 1\n");
			mResultsTextBox.AppendText(twoTool.GetPlayerStuff(1));
			mResultsTextBox.AppendText(twoTool.GetSchedule(1));

			mResultsTextBox.AppendText("YEAR 2\n");
			mResultsTextBox.AppendText(twoTool.GetPlayerStuff(2));
			mResultsTextBox.AppendText(twoTool.GetSchedule(2));

			mResultsTextBox.AppendText("YEAR 3\n");
			mResultsTextBox.AppendText(twoTool.GetPlayerStuff(3));
			mResultsTextBox.AppendText(twoTool.GetSchedule(3));

			StringBuilder sb = new StringBuilder();
			/*int tmp = 0;
			int tmp2 = 0;
			for (int i = 0; i < Tool.OutputRom.Length -50; i++)
			{
				tmp = Tool.OutputRom[i] + Tool.OutputRom[i + 1] + Tool.OutputRom[i + 2] + Tool.OutputRom[i + 3] + Tool.OutputRom[i + 4] + Tool.OutputRom[i + 5] + Tool.OutputRom[i + 6]
					+ Tool.OutputRom[i + 7] + Tool.OutputRom[i + 8] + Tool.OutputRom[i + 9] + Tool.OutputRom[i + 10];
				tmp2 = Tool.OutputRom[i + 0xB] + Tool.OutputRom[i + 0xB + 1] + Tool.OutputRom[i + 0xB + 2] + Tool.OutputRom[i + 0xB + 3] + Tool.OutputRom[i + 0xB + 4] + Tool.OutputRom[i + 0xB + 5] + Tool.OutputRom[i + 0xB + 6]
					+ Tool.OutputRom[i + 0xB + 7] + Tool.OutputRom[i + 0xB + 8] + Tool.OutputRom[i + 0xB + 9] + Tool.OutputRom[i + 0xB + 10];
				if (tmp == 255 && tmp2 == 255)
				{
					sb.Append("Possible Location:" + i.ToString("X") + "\n");
				}
			}
			mResultsTextBox.AppendText(sb.ToString());

			/*int[] stuff = {0xE80DC,0xE80DD,0xE80DE,0xE82EA,0xE8740,0xE8741,0xE8742,0xE8743,0xE8744,0xE89A4,0xE8E27,
				0xE8E28,0xE9A60,0xE9A61,0xE9A62,0xE9C32,0xE9C33,0xEA6BE,0xEA6BF,0xEA6C8,0xEA6C9,0xEAAFD,0xEB09E,0xEB09F,
				0xEB0A0,0xEB128,0xEB129,0xEC0EB,0xEC0EC,0xEC0F8,0xEC0F9,0xEC0FA,0xEC0FB,0xEC0FC,0xEC109,0xEC10A,0xF0A73,
				0xF5641,0xFC59D,0x10219B,0x107564,0x119903,0x119904,0x11B757,0x12BA42,0x12BA43,0x12BA53,0x12BA54,0x13202D,
				0x13A811,0x13A812,0x13C93F,0x13C940,0x13C94C,0x13C94D,0x13C94E,0x13C94F,0x13C950,0x13C95B,0x13C95C,0x13C95D,
				0x13C95E,0x13C95F,0x13C960,0x156C2D,0x1572AD,0x15D08D,0x191636,0x191637,0x191646,0x191647,0x191656,0x191657,
				0x191666,0x191667,0x191676,0x191677,0x191686,0x191687,0x191696,0x191697,0x1916A6,0x1916A7,0x1916B6,0x1916B7,
				0x1916C6,0x1916C7,0x1918BD,0x1918BE,0x192CD6,0x192CD7,0x192CF6,0x192CF7,0x192D36,0x192D37,0x192D46,0x192D86,
				0x192D87,0x1930B6,0x1930B7,0x1930E6,0x1930E7,0x193106,0x193107,0x193136,0x193137,0x193156,0x193157,0x1934E7,
				0x193507,0x194547,0x194556,0x194557};* /
			int[] stuff = {0x26ACF,0x26AFF,0x26B2F,0x26B5F,0x26B8F,0x26BBF,
					0x26BEF,0x26C1F,0x26C4F,0x26C7F,0x26CAF,0x26CDF,0x26D0F,0x26D3F,
					0x26E2F,0x26D9F,0x26DCF,0x26DFF,0x26D6F,0x26E5F,0x26E8F,0x26EBF,
					0x26EEF,0x26F1F,0x26FDF,0x26F7F,0x26FAF,0x26F4F};*/
			int[] stuff = {0X26AE7,0X26B17,0X26B47,0X26B77,0X26BA7,0X26BD7,0X26C07,0X26C37,0X26C67,0X26C97,
				0X26CC7,0X26CF7,0X26D27,0X26D57,0X26D87,0X26DB7,0X26DE7,0X26E17,0X26E47,0X26E77,0X26EA7,0X26ED7,
				0X26F07,0X26F37,0X26F67,0X26F97,0X26FC7,0X26FF7,0X27027,0X27057,0X3802D,0X8F2C6,0XA02D6,0XA02D7,
				0XA0576,0XA0577,0XA2F62,0XA2F63,0XA2F73,0XA2F74,0XA406D,0XC0B3C,0XC3160,0XC3161,0XC3162,0XC3163,
				0XC3180,0XC3181,0XC3182,0XC3183,0XC31A0,0XC31A1,0XC31A2,0XC31A3,0XC3400,0XC3401,0XC3402,0XC3403,
				0XCA29E,0XD2149,0XD214A,0XD214B,0XD214C,0XD214D,0XD214E,0XD90A9,0XDB4AE,0XE67C0,0XE742D,0XEB5AD,
				0XEF3AD,0XFA0AC,0XFA0AD,0XFB22D,0XFB8CD,0XFBBED,0XFBFED,0X1021CD,0X102F6D,0X10316D,0X1036ED,
				0X1038ED,0X104D8D,0X104E6D,0X10506D,0X1051ED,0X1053ED,0X10550D,0X10570D,0X10590D,0X105B0D,
				0X10790D,0X107C2D,0X10AE9D,0X10AE9E,0X10AEAC,0X10AEAD,0X10D42D,0X10DAAD,0X110F2D,0X1112ED,
				0X11228D,0X11256D,0X112D6D,0X115D8D,0X116C6D,0X116E2D,0X116F4D,0X1170CD,0X1174CD,0X1176CD,
				0X1179AD,0X11972D,0X11992D,0X151636,0X151637,0X151646,0X151647,0X151656,0X151657,0X151666,
				0X151667,0X151676,0X151677,0X151686,0X151687,0X151696,0X151697,0X1516A6,0X1516A7,0X1516B6,
				0X1516B7,0X1516C6,0X1516C7,0X1518BD,0X1518BE,0X152CD6,0X152CD7,0X152CF6,0X152CF7,0X152D36,
				0X152D37,0X152D46,0X152D86,0X152D87,0X1530B6,0X1530B7,0X1530E6,0X1530E7,0X153106,0X153107,
				0X153136,0X153137,0X153156,0X153157,0X1534E7,0X153507,0X154547,0X154556,0X154557};
			int limit = stuff.Length - 1;
			for(int i= 0; i< limit; i++)
			{
				sb.Append(string.Format("0x{0:x} - {1:x} = {2:x}\n", stuff[i + 1], stuff[i], stuff[i + 1] - stuff[i]));
			}
			mResultsTextBox.AppendText(sb.ToString());//*/

			//string[] tmp = null;
			//StringBuilder b = new StringBuilder();
			//string[] sch_1992_start = { "bengals","seahawks","browns", "colts", "lions","bears", "chiefs","chargers", "rams","bills"};
			//string[] sch_1993_start = { "patriots","bills", "bengals", "browns", "dolphins", "colts",  "broncos","jets","vikings","raiders"};
			//string[] sch_1994_start = { "cardinals", "rams", "falcons", "lions", "browns", "bengals", "cowboys", "steelers", "oilers", "colts" };
			//b.Append("\n\n1992: ");
			//tmp = sch_1992_start;
			//for (int i = 0; i < tmp.Length; i++)
			//    b.Append(TecmoTool.GetTeamIndex(tmp[i]).ToString("X2"));

			//b.Append("\n1993: ");
			//tmp = sch_1993_start;
			//for (int i = 0; i < tmp.Length; i++)
			//    b.Append(TecmoTool.GetTeamIndex(tmp[i]).ToString("X2"));

			//b.Append("\n1994: ");
			//tmp = sch_1994_start;
			//for (int i = 0; i < tmp.Length; i++)
			//    b.Append(TecmoTool.GetTeamIndex(tmp[i]).ToString("X2"));
			//mResultsTextBox.AppendText(b.ToString());

			

		}
	}
}
