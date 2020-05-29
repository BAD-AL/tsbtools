using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace System.Windows.Forms
{
    public enum DialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
    }

    public enum MessageBoxButtons
    {
        OK = 0,
        OKCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5,
    }

    public enum MessageBoxIcon
    {
        None = 0,
        Error = 16,
        Hand = 16,
        Stop = 16,
        Question = 32,
        Exclamation = 48,
        Warning = 48,
        Information = 64,
        Asterisk = 64,
    }

    public class MessageBox
    {
        public static DialogResult Show(object owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            Console.WriteLine("This should not be called!!!");
            return DialogResult.Cancel;
        }
    }
}

namespace TSBTool
{
    public class MainClass
    {
        public static bool GUI_MODE = false;
    }
}
