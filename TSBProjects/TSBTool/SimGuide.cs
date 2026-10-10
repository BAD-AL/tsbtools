using System;
using System.Drawing;
using System.Windows.Forms;

namespace TSBTool
{
    /// <summary>
    /// The TSB Sim Data Guide (TSBTool_Sim_Guide.txt, embedded): what the sim values do in every game TSBTool
    /// edits. Shown from About -> TSB Sim Data Guide, and from the player forms' 'About SimData' buttons, which
    /// open it at their game's section.
    /// </summary>
    public static class SimGuide
    {
        public const string Tsb1Section = "=== 3. TSB1 (NES, SNES) ===";
        public const string GenesisSection = "=== 4. GENESIS TSB1 ===";
        public const string Tsb2Section = "=== 5. TSB2 (SNES TECMO SUPER BOWL II) ===";
        public const string Tsb3Section = "=== 6. TSB3 (SNES TECMO SUPER BOWL III) ===";

        private const string ResourceName = "TSBTool.TSBTool_Sim_Guide.txt";

        /// <summary>The guide's text, or null when it isn't embedded.</summary>
        public static string Text
        {
            get { return StaticUtils.GetEmbeddedTextFile(ResourceName); }
        }

        /// <summary>Where 'section' (one of the *Section headings) starts in the guide; 0 (the top) when it isn't there.</summary>
        public static int SectionStart(string section)
        {
            string text = Text;
            if (text == null || string.IsNullOrEmpty(section))
                return 0;
            int at = text.Replace("\r\n", "\n").IndexOf(section, StringComparison.Ordinal);
            return at < 0 ? 0 : at;
        }

        /// <summary>Shows the guide (read only), scrolled to 'section' (null: the top).</summary>
        public static void Show(IWin32Window owner, string section)
        {
            string text = Text;
            if (text == null)
            {
                StaticUtils.ShowError("The TSB Sim Data Guide isn't in this build of TSBTool.");
                return;
            }
            RichTextDisplay display = new RichTextDisplay();
            display.Icon = SystemIcons.Information;
            display.Text = "TSB Sim Data Guide";
            display.MessageText = text.Replace("\r\n", "\n");
            display.MessageEditable = false;
            display.ShowCancelButton = false;
            // wide and tall enough for the guide's 100-column lines (Courier), within the screen
            Rectangle screen = Screen.FromPoint(Cursor.Position).WorkingArea;
            display.Size = new Size(Math.Min(880, screen.Width), Math.Min(760, screen.Height));
            if (owner != null)
                display.Show(owner);
            else
                display.Show();
            // a RichTextBox counts '\n' as one character, as SectionStart does
            display.ContentBox.SelectionStart = SectionStart(section);
            display.ContentBox.SelectionLength = 0;
            display.ContentBox.ScrollToCaret();
        }
    }
}
