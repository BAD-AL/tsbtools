using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;
using TSBTool2;

namespace TSBTool2_UI
{
    /// <summary>
    /// Summary description for StringInputDlg.
    /// </summary>
    public class StringInputDlg : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox userInput;
        private string result = "";
        private System.Windows.Forms.Button cancelButton;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public StringInputDlg(string title, string message, string initialText)
        {
            InitializeComponent();
            this.Text = title;
            this.label1.Text = message;
            this.userInput.Text = initialText;
            this.userInput.SelectAll();
        }

        public static string GetString(string title, string message)
        {
            StringInputDlg sid = new StringInputDlg(title, message, "");
            sid.ShowDialog();
            string ret = sid.getResult();
            sid.Dispose();
            return ret;
        }

        public static string GetString(string title, string message, string initialText)
        {
            StringInputDlg sid = new StringInputDlg(title, message, initialText);
            sid.ShowDialog();
            string ret = sid.getResult();
            sid.Dispose();
            return ret;
        }

        public string getResult()
        {
            return result;
        }


        /// <summary>
        /// Prompt the user for input for a 'set' command
        /// </summary>
        /// <param name="input">The input string to operate on</param>
        /// <returns>empty string on cancel, 'SET' string when successful.</returns>
        public static string PromptForSetUserInput(string input)
        {
            string retVal = "";
            string msg = GetPromptUserMesage(input);
            string location = GetSetLocation(input);

            while (retVal == "")
            {
                StringBuilder rangeMsg = new StringBuilder(25);
                String userInputValue = "";
                int min = 0;
                int max = 0;
                //SET(0x2224B, {32TeamNES,28TeamNES PromptUser:Msg="Enter desired quarter length":int(1-15)} )
                //SET(0x2224B, {32TeamNES,28TeamNES PromptUser:Msg="Enter desired quarter length":int(0x1-0x15)} )
                //SET(0x2224B, {32TeamNES,28TeamNES PromptUser:Msg="Enter name of...":string(len=8)} ) maybe not this one.
                if (input.IndexOf("int", StringComparison.OrdinalIgnoreCase) > -1 &&
                    (GetHexRange(ref min, ref max, input, rangeMsg) || GetDecimalRange(ref min, ref max, input, rangeMsg)))
                {
                    string rangeMessage = rangeMsg.ToString();
                    NumberStyles style = rangeMessage.IndexOf('x') > 0 ? NumberStyles.HexNumber : NumberStyles.Integer;
                    string initialText = style == NumberStyles.HexNumber ? "0x" : "";
                    StringInputDlg dlg = new StringInputDlg(msg, rangeMessage, initialText);
                    int userVal = Int32.MinValue;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        userInputValue = dlg.getResult();
                        if (userInputValue.StartsWith("0x"))
                            userInputValue = userInputValue.Substring(2);

                        try
                        {
                            userVal = Int32.Parse(userInputValue, style);
                            if (userVal < min || userVal > max)
                            {
                                userVal = Int32.MinValue;
                                throw new Exception("Invalid input.");
                            }
                        }
                        catch (Exception)
                        {
                            StaticUtils.ShowError(string.Format(
                                "Error with '{0}'. Value '{1}' is invalid for range {2}",
                                msg, userInputValue, rangeMessage));
                        }
                        if (userVal > Int32.MinValue)
                        {
                            retVal = string.Format("SET({0}, 0x{1:x})", location, userVal);
                        }
                    }
                    else
                    {
                        retVal = " "; // trigger us to leave the loop
                    }
                    dlg.Dispose();
                }
                else
                {
                    StaticUtils.ShowError("ERROR applying line: " + input);
                    retVal = null;
                }
            }
            return retVal;
        }

        private static string GetSetLocation(string input)
        {
            string retVal = "";
            Regex locationRegex = new Regex(@"SET\s*\(\s*(0x[0-9a-fA-F]+)\s*,");

            Match m = locationRegex.Match(input);
            if (m == Match.Empty)
            {
                StaticUtils.ShowError(string.Format("SET function not used properly. incorrect syntax>'{0}'", input));
            }
            else
            {
                retVal = m.Groups[1].ToString().ToLower();
            }
            return retVal;
        }

        private static bool GetHexRange(ref int min, ref int max, string input, StringBuilder rangeString)
        {
            bool retVal = false;
            Regex hexRangeRegex = new Regex(@"\(\s*0x([0-9a-fA-F]+)\s*-\s*0x([0-9a-fA-F]+)\s*\)");
            Match m = hexRangeRegex.Match(input);
            if (m != Match.Empty)
            {
                min = Int32.Parse(m.Groups[1].ToString(), System.Globalization.NumberStyles.HexNumber);
                max = Int32.Parse(m.Groups[2].ToString(), System.Globalization.NumberStyles.HexNumber);
                rangeString.Append("0x");
                rangeString.Append(m.Groups[1].ToString());
                rangeString.Append("-");
                rangeString.Append("0x");
                rangeString.Append(m.Groups[2].ToString());
                retVal = true;
            }
            return retVal;
        }

        private static bool GetDecimalRange(ref int min, ref int max, string input, StringBuilder rangeString)
        {
            bool retVal = false;
            Regex decRangeRegex = new Regex(@"\(\s*([0-9]+)\s*-\s*([0-9]+)\s*\)");
            Match m = decRangeRegex.Match(input);
            if (m != Match.Empty)
            {
                min = Int32.Parse(m.Groups[1].ToString());
                max = Int32.Parse(m.Groups[2].ToString());
                rangeString.Append(m.Groups[1].ToString());
                rangeString.Append("-");
                rangeString.Append(m.Groups[2].ToString());
                retVal = true;
            }
            return retVal;
        }
        private static string GetPromptUserMesage(string input)
        {
            int msgStartLoc = input.IndexOf("Msg=\"", StringComparison.OrdinalIgnoreCase) + 5;
            string msg = "";
            if (msgStartLoc > 5)
            {
                int endIndex = input.IndexOf('"', msgStartLoc + 1);
                if (endIndex > -1)
                    msg = input.Substring(msgStartLoc, endIndex - msgStartLoc);
            }
            return msg;
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.userInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(65, 64);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(60, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(141, 64);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // userInput
            // 
            this.userInput.Location = new System.Drawing.Point(56, 40);
            this.userInput.Name = "userInput";
            this.userInput.Size = new System.Drawing.Size(200, 20);
            this.userInput.TabIndex = 0;
            this.userInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.userInput_KeyDown);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(56, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 32);
            this.label1.TabIndex = 3;
            // 
            // StringInputDlg
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(282, 104);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.userInput);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StringInputDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "StringInputDlg";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private void userInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //result = userInput.Text;
                okButton_Click(sender, new System.EventArgs());
            }
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            result = userInput.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            result = "";
            this.Close();
        }
    }
}
