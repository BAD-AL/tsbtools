using System.Collections.Generic;
using TSBTool;

namespace TSBTool_Test
{
    /// <summary>
    /// Captures messages passed to StaticUtils.ShowError/ShowMessageBox/LogMessage instead of popping
    /// real UI, so tests can assert on error content rather than only "the ROM didn't change".
    /// StaticUtils.AddError just queues text internally -- install this as StaticUtils.sMessageGiver,
    /// then call StaticUtils.ShowErrors() to flush the queue through to ShowError() below.
    /// </summary>
    internal class TestMessageGiver : MessageGiver
    {
        public List<string> Errors = new List<string>();
        public List<string> Messages = new List<string>();

        public void ShowMessageBox(string title, string message) { Messages.Add(message); }
        public void ShowError(string title, string message) { Errors.Add(message); }
        public bool ShowConfirmationDialog(string title, string message) { return true; }
        public string PromptForSetUserInput(string input) { return ""; }
        public void LogMessage(string message) { }
    }
}
