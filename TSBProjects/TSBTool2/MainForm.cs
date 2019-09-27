using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            if(season1ToolStripMenuItem.Checked)
                text+= tool.GetAll(1);
            else if(season2ToolStripMenuItem.Checked)
                text+= tool.GetAll(2);
            else if(season3ToolStripMenuItem.Checked)
                text+= tool.GetAll(3);
            else if(allSeasonsToolStripMenuItem.Checked)
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
            dlg.Show();
        }

        private void aboutTSBTool2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
@"You are using the PRE-ALPHA version of TSBTool2. 
It'll get better.
It's mostly intended to assist those doing the discovery of TSB2 stuff.
It likely has bugs.
"
                );
        }


	}
}
