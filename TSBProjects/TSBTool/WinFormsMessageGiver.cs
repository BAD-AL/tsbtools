using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace TSBTool
{
    public class WinFormsMessageGiver : MessageGiver
    {

        #region MessageGiver Members

        public void ShowMessageBox(string title, string message)
        {
            MessageBox.Show(message, title);
        }

        public void ShowError(string title, string error)
        {
            RichTextDisplay.ShowMessage("Error!", error, System.Drawing.SystemIcons.Error, false, false);
        }

        public bool ShowConfirmationDialog(string title, string message)
        {
            bool retVal = false;
            if (MessageBox.Show(null, "ROM could be messed up, do you want to save anyway?", "ERROR!",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                retVal = true;
            }
            return retVal;
        }

        public string PromptForSetUserInput(string input)
        {
            string simpleSetLine = StringInputDlg.PromptForSetUserInput(input);
            return simpleSetLine;
        }

        public void LogMessage(String msg)
        {
            Console.WriteLine(msg);
        }

        #endregion
    }
}
