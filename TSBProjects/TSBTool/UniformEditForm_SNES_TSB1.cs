using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TSBTool
{
    public partial class UniformEditForm_SNES_TSB1 : Form
    {
        public UniformEditForm_SNES_TSB1()
        {
            InitializeComponent();
        }

        #region Pixel locations
        // x,y 
        private int[] JerseyL = { // 'Jersey light' pixels 
           14,20, 14,21, 14,22, 14,23, 15,20, 15,21, 15,22, 15,23, 16,20, 
		   16,21, 16,22, 16,23, 17,20, 17,21, 17,22, 17,23, 18,12, 18,13, 
		   18,14, 18,15, 18,16, 18,17, 18,18, 18,19, 18,40, 18,41, 18,42, 
		   18,43, 19,12, 19,13, 19,14, 19,15, 19,16, 19,17, 19,18, 19,19, 
		   19,40, 19,41, 19,42, 19,43, 20,12, 20,13, 20,14, 20,15, 20,16, 
		   20,17, 20,18, 20,19, 20,40, 20,41, 20,42, 20,43, 21,12, 21,13, 
		   21,14, 21,15, 21,16, 21,17, 21,18, 21,19, 21,40, 21,41, 21,42, 
		   21,43, 22,8, 22,9, 22,10, 22,11, 22,12, 22,13, 22,14, 22,15, 
		   22,16, 22,17, 22,18, 22,19, 23,8, 23,9, 23,10, 23,11, 23,12, 
		   23,13, 23,14, 23,15, 23,16, 23,17, 23,18, 23,19, 24,8, 24,9, 
		   24,10, 24,11, 24,12, 24,13, 24,14, 24,15, 24,16, 24,17, 24,18, 
		   24,19, 25,8, 25,9, 25,10, 25,11, 25,12, 25,13, 25,14, 25,15, 
		   25,16, 25,17, 25,18, 25,19, 26,16, 26,17, 26,18, 26,19, 26,20, 
		   26,21, 26,22, 26,23, 27,16, 27,17, 27,18, 27,19, 27,20, 27,21, 
		   27,22, 27,23, 28,16, 28,17, 28,18, 28,19, 28,20, 28,21, 28,22, 
		   28,23, 29,16, 29,17, 29,18, 29,19, 29,20, 29,21, 29,22, 29,23, 
		   30,20, 30,21, 30,22, 30,23, 31,20, 31,21, 31,22, 31,23, 32,20, 
		   32,21, 32,22, 32,23, 33,20, 33,21, 33,22, 33,23, 42,20, 42,21, 
		   42,22, 42,23, 43,20, 43,21, 43,22, 43,23, 44,20, 44,21, 44,22, 
		   44,23, 45,20, 45,21, 45,22, 45,23, 46,16, 46,17, 46,18, 46,19, 
		   46,20, 46,21, 46,22, 46,23, 47,16, 47,17, 47,18, 47,19, 47,20, 
		   47,21, 47,22, 47,23, 48,16, 48,17, 48,18, 48,19, 48,20, 48,21, 
		   48,22, 48,23, 49,16, 49,17, 49,18, 49,19, 49,20, 49,21, 49,22, 
		   49,23, };
        private int[] JerseyM = { // 'Jersey medium' pixels 
          14,12, 14,13, 14,14, 14,15, 14,16, 14,17, 14,18, 14,19, 14,24, 
		  14,25, 14,26, 14,27, 14,32, 14,33, 14,34, 14,35, 14,36, 14,37, 
		  14,38, 14,39, 15,12, 15,13, 15,14, 15,15, 15,16, 15,17, 15,18, 
		  15,19, 15,24, 15,25, 15,26, 15,27, 15,32, 15,33, 15,34, 15,35, 
		  15,36, 15,37, 15,38, 15,39, 16,12, 16,13, 16,14, 16,15, 16,16, 
		  16,17, 16,18, 16,19, 16,24, 16,25, 16,26, 16,27, 16,32, 16,33, 
		  16,34, 16,35, 16,36, 16,37, 16,38, 16,39, 17,12, 17,13, 17,14, 
		  17,15, 17,16, 17,17, 17,18, 17,19, 17,24, 17,25, 17,26, 17,27, 
		  17,32, 17,33, 17,34, 17,35, 17,36, 17,37, 17,38, 17,39, 18,24, 
		  18,25, 18,26, 18,27, 18,44, 18,45, 18,46, 18,47, 18,48, 18,49, 
		  18,50, 18,51, 19,24, 19,25, 19,26, 19,27, 19,44, 19,45, 19,46, 
		  19,47, 19,48, 19,49, 19,50, 19,51, 20,24, 20,25, 20,26, 20,27, 
		  20,44, 20,45, 20,46, 20,47, 20,48, 20,49, 20,50, 20,51, 21,24, 
		  21,25, 21,26, 21,27, 21,44, 21,45, 21,46, 21,47, 21,48, 21,49, 
		  21,50, 21,51, 26,8, 26,9, 26,10, 26,11, 26,12, 26,13, 26,14, 
		  26,15, 27,8, 27,9, 27,10, 27,11, 27,12, 27,13, 27,14, 27,15, 
		  28,8, 28,9, 28,10, 28,11, 28,12, 28,13, 28,14, 28,15, 29,8, 
		  29,9, 29,10, 29,11, 29,12, 29,13, 29,14, 29,15, 30,16, 30,17, 
		  30,18, 30,19, 30,24, 30,25, 30,26, 30,27, 30,28, 30,29, 30,30, 
		  30,31, 31,16, 31,17, 31,18, 31,19, 31,24, 31,25, 31,26, 31,27, 
		  31,28, 31,29, 31,30, 31,31, 32,16, 32,17, 32,18, 32,19, 32,24, 
		  32,25, 32,26, 32,27, 32,28, 32,29, 32,30, 32,31, 33,16, 33,17, 
		  33,18, 33,19, 33,24, 33,25, 33,26, 33,27, 33,28, 33,29, 33,30, 
		  33,31, 34,24, 34,25, 34,26, 34,27, 34,28, 34,29, 34,30, 34,31, 
		  35,24, 35,25, 35,26, 35,27, 35,28, 35,29, 35,30, 35,31, 36,24, 
		  36,25, 36,26, 36,27, 36,28, 36,29, 36,30, 36,31, 37,24, 37,25, 
		  37,26, 37,27, 37,28, 37,29, 37,30, 37,31, 38,20, 38,21, 38,22, 
		  38,23, 38,24, 38,25, 38,26, 38,27, 39,20, 39,21, 39,22, 39,23, 
		  39,24, 39,25, 39,26, 39,27, 40,20, 40,21, 40,22, 40,23, 40,24, 
		  40,25, 40,26, 40,27, 41,20, 41,21, 41,22, 41,23, 41,24, 41,25, 
		  41,26, 41,27, 42,24, 42,25, 42,26, 42,27, 43,24, 43,25, 43,26, 
		  43,27, 44,24, 44,25, 44,26, 44,27, 45,24, 45,25, 45,26, 45,27, };



        private int[] PantsD = { // 'Pants Dark' pixels 
          14,52, 14,53, 14,54, 14,55, 14,56, 14,57, 14,58, 14,59, 14,60, 
          14,61, 14,62, 14,63, 15,52, 15,53, 15,54, 15,55, 15,56, 15,57, 
		  15,58, 15,59, 15,60, 15,61, 15,62, 15,63, 16,52, 16,53, 16,54, 
		  16,55, 16,56, 16,57, 16,58, 16,59, 16,60, 16,61, 16,62, 16,63, 
		  17,52, 17,53, 17,54, 17,55, 17,56, 17,57, 17,58, 17,59, 17,60, 
		  17,61, 17,62, 17,63, 18,52, 18,53, 18,54, 18,55, 18,56, 18,57, 
		  18,58, 18,59, 18,60, 18,61, 18,62, 18,63, 19,52, 19,53, 19,54, 
		  19,55, 19,56, 19,57, 19,58, 19,59, 19,60, 19,61, 19,62, 19,63, 
		  20,52, 20,53, 20,54, 20,55, 20,56, 20,57, 20,58, 20,59, 20,60, 
		  20,61, 20,62, 20,63, 21,52, 21,53, 21,54, 21,55, 21,56, 21,57, 
		  21,58, 21,59, 21,60, 21,61, 21,62, 21,63, 34,48, 34,49, 34,50, 
		  34,51, 34,52, 34,53, 34,54, 34,55, 34,56, 34,57, 34,58, 34,59, 
		  35,48, 35,49, 35,50, 35,51, 35,52, 35,53, 35,54, 35,55, 35,56, 
		  35,57, 35,58, 35,59, 36,48, 36,49, 36,50, 36,51, 36,52, 36,53, 
		  36,54, 36,55, 36,56, 36,57, 36,58, 36,59, 37,48, 37,49, 37,50, 
		  37,51, 37,52, 37,53, 37,54, 37,55, 37,56, 37,57, 37,58, 37,59, 
		  38,56, 38,57, 38,58, 38,59, 39,56, 39,57, 39,58, 39,59, 40,56, 
		  40,57, 40,58, 40,59, 41,56, 41,57, 41,58, 41,59, };

        private int[] PantsM = { // Pants Medium Pixels
          10,28, 10,29, 10,30, 10,31, 10,32, 10,33, 10,34, 10,35, 10,36, 
		  10,37, 10,38, 10,39, 11,28, 11,29, 11,30, 11,31, 11,32, 11,33, 
		  11,34, 11,35, 11,36, 11,37, 11,38, 11,39, 12,28, 12,29, 12,30, 
		  12,31, 12,32, 12,33, 12,34, 12,35, 12,36, 12,37, 12,38, 12,39, 
		  13,28, 13,29, 13,30, 13,31, 13,32, 13,33, 13,34, 13,35, 13,36, 
		  13,37, 13,38, 13,39, 14,28, 14,29, 14,30, 14,31, 14,40, 14,41, 
		  14,42, 14,43, 14,44, 14,45, 14,46, 14,47, 14,48, 14,49, 14,50, 
		  14,51, 15,28, 15,29, 15,30, 15,31, 15,40, 15,41, 15,42, 15,43, 
		  15,44, 15,45, 15,46, 15,47, 15,48, 15,49, 15,50, 15,51, 16,28, 
		  16,29, 16,30, 16,31, 16,40, 16,41, 16,42, 16,43, 16,44, 16,45, 
		  16,46, 16,47, 16,48, 16,49, 16,50, 16,51, 17,28, 17,29, 17,30, 
		  17,31, 17,40, 17,41, 17,42, 17,43, 17,44, 17,45, 17,46, 17,47, 
		  17,48, 17,49, 17,50, 17,51, 26,36, 26,37, 26,38, 26,39, 27,36, 
		  27,37, 27,38, 27,39, 28,36, 28,37, 28,38, 28,39, 29,36, 29,37, 
		  29,38, 29,39, 34,44, 34,45, 34,46, 34,47, 35,44, 35,45, 35,46, 
		  35,47, 36,44, 36,45, 36,46, 36,47, 37,44, 37,45, 37,46, 37,47, 
		  38,32, 38,33, 38,34, 38,35, 39,32, 39,33, 39,34, 39,35, 40,32, 
		  40,33, 40,34, 40,35, 41,32, 41,33, 41,34, 41,35, 42,32, 42,33, 
		  42,34, 42,35, 43,32, 43,33, 43,34, 43,35, 44,32, 44,33, 44,34, 
		  44,35, 45,32, 45,33, 45,34, 45,35, };

        private int[] PantsL = { // Pants Light Pixles
          22,52, 22,53, 22,54, 22,55, 22,56, 22,57, 22,58, 22,59, 22,60, 
		  22,61, 22,62, 22,63, 23,52, 23,53, 23,54, 23,55, 23,56, 23,57, 
		  23,58, 23,59, 23,60, 23,61, 23,62, 23,63, 24,52, 24,53, 24,54, 
		  24,55, 24,56, 24,57, 24,58, 24,59, 24,60, 24,61, 24,62, 24,63, 
		  25,52, 25,53, 25,54, 25,55, 25,56, 25,57, 25,58, 25,59, 25,60, 
		  25,61, 25,62, 25,63, 26,4, 26,5, 26,6, 26,7, 26,40, 26,41, 
		  26,42, 26,43, 26,44, 26,45, 26,46, 26,47, 26,48, 26,49, 26,50, 
		  26,51, 26,52, 26,53, 26,54, 26,55, 26,60, 26,61, 26,62, 26,63, 
		  26,64, 27,4, 27,5, 27,6, 27,7, 27,40, 27,41, 27,42, 27,43, 27,44, 
		  27,45, 27,46, 27,47, 27,48, 27,49, 27,50, 27,51, 27,52, 27,53, 
		  27,54, 27,55, 27,60, 27,61, 27,62, 27,63, 27,64, 28,4, 28,5, 
		  28,6, 28,7, 28,40, 28,41, 28,42, 28,43, 28,44, 28,45, 28,46, 
		  28,47, 28,48, 28,49, 28,50, 28,51, 28,52, 28,53, 28,54, 28,55, 
		  28,60, 28,61, 28,62, 28,63, 28,64, 29,4, 29,5, 29,6, 29,7, 
		  29,40, 29,41, 29,42, 29,43, 29,44, 29,45, 29,46, 29,47, 29,48, 
		  29,49, 29,50, 29,51, 29,52, 29,53, 29,54, 29,55, 29,60, 29,61, 
		  29,62, 29,63, 29,64, 38,48, 38,49, 38,50, 38,51, 38,52, 38,53, 
		  38,54, 38,55, 39,48, 39,49, 39,50, 39,51, 39,52, 39,53, 39,54, 
		  39,55, 40,48, 40,49, 40,50, 40,51, 40,52, 40,53, 40,54, 40,55, 
		  41,48, 41,49, 41,50, 41,51, 41,52, 41,53, 41,54, 41,55, 42,48, 
		  42,49, 42,50, 42,51, 42,52, 42,53, 42,54, 42,55, 42,56, 42,57, 
		  42,58, 42,59, 43,48, 43,49, 43,50, 43,51, 43,52, 43,53, 43,54, 
		  43,55, 43,56, 43,57, 43,58, 43,59, 44,48, 44,49, 44,50, 44,51, 
		  44,52, 44,53, 44,54, 44,55, 44,56, 44,57, 44,58, 44,59, 45,48, 
		  45,49, 45,50, 45,51, 45,52, 45,53, 45,54, 45,55, 45,56, 45,57, 
		  45,58, 45,59, 46,56, 46,57, 46,58, 46,59, 47,56, 47,57, 47,58, 
		  47,59, 48,56, 48,57, 48,58, 48,59, 49,56, 49,57, 49,58, 49,59, };


         private int[] HelmetM = { // Helmet Medium pixels 
           34,0, 34,1, 34,2, 34,3, 35,0, 35,1, 35,2, 35,3, 36,0, 
           36,1, 36,2, 36,3, 37,0, 37,1, 37,2, 37,3, 38,0, 38,1, 
		   38,2, 38,3, 38,4, 38,5, 38,6, 38,7, 39,0, 39,1, 39,2, 
		   39,3, 39,4, 39,5, 39,6, 39,7, 40,0, 40,1, 40,2, 40,3, 
		   40,4, 40,5, 40,6, 40,7, 41,0, 41,1, 41,2, 41,3, 41,4, 
		   41,5, 41,6, 41,7, 42,4, 42,5, 42,6, 42,7, 42,16, 42,17, 
		   42,18, 42,19, 43,4, 43,5, 43,6, 43,7, 43,16, 43,17, 
		   43,18, 43,19, 44,4, 44,5, 44,6, 44,7, 44,16, 44,17, 
		   44,18, 44,19, 45,4, 45,5, 45,6, 45,7, 45,16, 45,17, 
		   45,18, 45,19 };

           private int[] HelmetD = { // Helmet Dark Pixels
           30,4, 30,5, 30,6, 30,7, 30,8, 30,9, 30,10, 30,11, 30,12, 
           30,13, 30,14, 30,15, 31,4, 31,5, 31,6, 31,7, 31,8, 31,9, 
		   31,10, 31,11, 31,12, 31,13, 31,14, 31,15, 32,4, 32,5, 
		   32,6, 32,7, 32,8, 32,9, 32,10, 32,11, 32,12, 32,13, 32,14, 
		   32,15, 33,4, 33,5, 33,6, 33,7, 33,8, 33,9, 33,10, 33,11, 
		   33,12, 33,13, 33,14, 33,15, 34,4, 34,5, 34,6, 34,7, 34,8, 
		   34,9, 34,10, 34,11, 34,12, 34,13, 34,14, 34,15, 35,4, 35,5, 
		   35,6, 35,7, 35,8, 35,9, 35,10, 35,11, 35,12, 35,13, 35,14, 
		   35,15, 36,4, 36,5, 36,6, 36,7, 36,8, 36,9, 36,10, 36,11, 
		   36,12, 36,13, 36,14, 36,15, 37,4, 37,5, 37,6, 37,7, 37,8, 
		   37,9, 37,10, 37,11, 37,12, 37,13, 37,14, 37,15, 38,16, 38,17, 
		   38,18, 38,19, 39,16, 39,17, 39,18, 39,19, 40,16, 40,17, 40,18, 
		   40,19, 41,16, 41,17, 41,18, 41,19 };
		   
        #endregion 

        private void mHomePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            // code used to get pixel locations
            //Bitmap bmp = new Bitmap(mHomePictureBox.Image);
            
            //Color c = bmp.GetPixel(e.X, e.Y);
            //Color t = Color.Aqua;
            //StringBuilder sb = new StringBuilder(500);
            //sb.Append("[");
            //for (int x = 0; x < bmp.Width; x++)
            //{
            //    for (int y = 0; y < bmp.Height; y++)
            //    {
            //        System.Diagnostics.Debugger.Log(1,"debug", String.Format(" {0}, {1}\n",x,y));
            //        t = bmp.GetPixel(x, y);
            //        if (t == c)
            //        {
            //            sb.Append(String.Format("{0},{1}, ",x,y));
            //        }
            //    }
            //}
            //sb.Append("]");
            //Console.WriteLine(sb.ToString());
        }

        #region Data binding (text-editor-over-InputParser pattern, matching UniformEditForm)

        private string m_Data = string.Empty;
        private string m_OriginalData = string.Empty;

        /// <summary>
        /// The full multi-team text data to work on and retrieve -- this form never touches the ROM,
        /// or the parent form's text, directly. It splices the current team's "COLORS Uniform1=...,
        /// Uniform2=..." line in place within its own private m_Data copy, the same way
        /// UniformEditForm (NES) does; MainGUI only reads this property back out (via Data's getter)
        /// after ShowDialog returns DialogResult.OK. Also snapshots the incoming value so Cancel can
        /// discard whatever was edited locally (every color pick applies immediately to this form's
        /// own m_Data, not to the parent -- there's no other undo path).
        /// </summary>
        public string Data
        {
            get { return m_Data; }
            set
            {
                m_Data = value ?? string.Empty;
                m_OriginalData = m_Data;
            }
        }

        private string m_CurrentTeam = string.Empty;

        /// <summary>
        /// Gets/sets the current team. Setting it loads that team's colors into the UI.
        /// </summary>
        public string CurrentTeam
        {
            get { return m_CurrentTeam; }
            set
            {
                m_CurrentTeam = value;
                SetCurrentTeamColors();
            }
        }

        private string m_UniformUsageString = "00000000";

        /// <summary>
        /// Gets a "COLORS ..." line from m_Data for the given team.
        /// </summary>
        private string GetColorString(string team)
        {
            string ret = string.Empty;
            string pattern = "TEAM\\s*=\\s*" + team;
            Regex findTeamRegex = new Regex(pattern);
            Match m = findTeamRegex.Match(m_Data);
            if (m.Success)
            {
                int teamIndex = m.Index;
                int lineStart = m_Data.IndexOf("COLORS", teamIndex);
                int lineEnd = -1;
                if (lineStart > 0)
                {
                    lineEnd = m_Data.IndexOf("\n", lineStart + 3);
                }
                if (lineStart > -1 && lineEnd > -1)
                {
                    ret = m_Data.Substring(lineStart, lineEnd - lineStart);
                }
            }
            return ret;
        }

        /// <summary>
        /// Updates the GUI with the current team's colors.
        /// </summary>
        private void SetCurrentTeamColors()
        {
            string colorData = GetColorString(CurrentTeam);
            if (!string.IsNullOrEmpty(colorData))
                SetFormColorData(colorData);
        }

        /// <summary>
        /// Parses a "COLORS Uniform1=0x..., Uniform2=0x..., UniformUsage=0x..." line (28-hex-digit
        /// SNES format: pants1,jersey2,pants2,jersey3,pants3,helmetDark,helmetMedium -- see
        /// SNES_TecmoTool.WriteUniformBlock) into the UI.
        /// </summary>
        private void SetFormColorData(string colorData)
        {
            string homeUniform = InputParser.GetHomeUniformColorString(colorData);
            string awayUniform = InputParser.GetAwayUniformColorString(colorData);
            string uniformUsage = InputParser.GetUniformUsageString(colorData);

            if (!string.IsNullOrEmpty(homeUniform) && homeUniform.Length == 28)
            {
                SetColorUI(mHomePantsLabelD, mHomePantsD_c_lab, mHomePictureBox, PantsD, homeUniform.Substring(0, 4));
                SetColorUI(mHomeJerseyLabelM, mHomeJerseyM_c_lab, mHomePictureBox, JerseyM, homeUniform.Substring(4, 4));
                SetColorUI(mHomePantsLabelM, mHomePantsM_c_lab, mHomePictureBox, PantsM, homeUniform.Substring(8, 4));
                SetColorUI(mHomeJerseyLabelL, mHomeJerseyL_c_lab, mHomePictureBox, JerseyL, homeUniform.Substring(12, 4));
                SetColorUI(mHomePantsLabelL, mHomePantsL_c_lab, mHomePictureBox, PantsL, homeUniform.Substring(16, 4));
                SetColorUI(mHomeHelmetLabelD, mHomeHelmetD_c_lab, mHomePictureBox, HelmetD, homeUniform.Substring(20, 4));
                SetColorUI(mHomeHelmetLabelM, mHomeHelmetM_c_lab, mHomePictureBox, HelmetM, homeUniform.Substring(24, 4));
            }
            if (!string.IsNullOrEmpty(awayUniform) && awayUniform.Length == 28)
            {
                // Away Pants/Helmet's hex labels were never renamed from the designer defaults --
                // label17/16/15 are Pants Dark/Medium/Light, label11/10 are Helmet Dark/Medium (see
                // groupBox5/groupBox6 in the .Designer.cs). Left as-is rather than hand-editing the
                // designer's control names.
                SetColorUI(label17, mAwayPantsD_c_lab, pictureBox1, PantsD, awayUniform.Substring(0, 4));
                SetColorUI(mAwayJerseyLabelM, mAwayJerseyM_c_lab, pictureBox1, JerseyM, awayUniform.Substring(4, 4));
                SetColorUI(label16, mAwayPantsM_c_lab, pictureBox1, PantsM, awayUniform.Substring(8, 4));
                SetColorUI(mAwayJerseyLabelL, mAwayJerseyL_c_lab, pictureBox1, JerseyL, awayUniform.Substring(12, 4));
                SetColorUI(label15, mAwayPantsL_c_lab, pictureBox1, PantsL, awayUniform.Substring(16, 4));
                SetColorUI(label11, mAwayHelmetD_c_lab, pictureBox1, HelmetD, awayUniform.Substring(20, 4));
                SetColorUI(label10, mAwayHelmetM_c_lab, pictureBox1, HelmetM, awayUniform.Substring(24, 4));
            }
            if (!string.IsNullOrEmpty(uniformUsage))
                m_UniformUsageString = uniformUsage;
        }

        /// <summary>
        /// Builds a "COLORS ..." line from the current state of the UI.
        /// </summary>
        private string GetCurrentTeamColorData_UI()
        {
            string uniform1 = mHomePantsLabelD.Text + mHomeJerseyLabelM.Text + mHomePantsLabelM.Text +
                               mHomeJerseyLabelL.Text + mHomePantsLabelL.Text + mHomeHelmetLabelD.Text + mHomeHelmetLabelM.Text;
            string uniform2 = label17.Text + mAwayJerseyLabelM.Text + label16.Text +
                               mAwayJerseyLabelL.Text + label15.Text + label11.Text + label10.Text;
            return string.Format("COLORS Uniform1=0x{0}, Uniform2=0x{1}, UniformUsage=0x{2}",
                uniform1, uniform2, m_UniformUsageString);
        }

        private void ReplaceColorData()
        {
            string oldData = GetColorString(CurrentTeam);
            string newData = GetCurrentTeamColorData_UI();
            if (!string.IsNullOrEmpty(oldData))
                ReplaceColorData(CurrentTeam, oldData, newData);
        }

        /// <summary>
        /// Splices newData in place of oldData within m_Data, for the given team's COLORS line only
        /// (bounded by the next "TEAM = " occurrence, so a same-content match belonging to a different
        /// team's block can never be touched). Doesn't depend on a team list/combobox -- just searches
        /// forward in the text.
        /// </summary>
        private void ReplaceColorData(string team, string oldData, string newData)
        {
            Regex findTeamRegex = new Regex("TEAM\\s*=\\s*" + team);
            Match m = findTeamRegex.Match(m_Data);
            if (!m.Success)
                return;
            int currentTeamIndex = m.Index;

            Regex nextTeamRegex = new Regex("TEAM\\s*=\\s*[a-z0-9]+");
            Match nt = nextTeamRegex.Match(m_Data, currentTeamIndex + m.Length);
            int nextTeamIndex = nt.Success ? nt.Index : m_Data.Length;

            int dataIndex = m_Data.IndexOf(oldData, currentTeamIndex);
            if (dataIndex > -1 && dataIndex < nextTeamIndex)
            {
                int endLine = m_Data.IndexOf('\n', dataIndex);
                if (endLine < 0)
                    endLine = m_Data.Length;
                string start = m_Data.Substring(0, dataIndex);
                string last = m_Data.Substring(endLine);
                m_Data = start + newData + last;
            }
        }

        #endregion

        #region Color conversion
        // Matches the round(v/8) <-> idx*8 convention used throughout Core/SNES_TecmoTool.cs and the
        // web-based Uniform Color Editor, not the "mathematically proper" idx*255/31 -- staying
        // consistent with that is what makes stock values round-trip exactly instead of drifting by a
        // shade when re-encoded.

        private static Color SnesHexToColor(string hex4)
        {
            if (string.IsNullOrEmpty(hex4) || hex4.Length != 4)
                return Color.Black;
            byte b0 = Convert.ToByte(hex4.Substring(0, 2), 16);
            byte b1 = Convert.ToByte(hex4.Substring(2, 2), 16);
            int v = b0 | (b1 << 8);
            int r = (v & 0x1F) * 8;
            int g = ((v >> 5) & 0x1F) * 8;
            int b = ((v >> 10) & 0x1F) * 8;
            return Color.FromArgb(r, g, b);
        }

        #endregion

        #region Picture box recoloring

        private void SetBitmapColor(Bitmap bmp, Color c, int[] locations)
        {
            for (int i = 0; i < locations.Length; i += 2)
            {
                bmp.SetPixel(locations[i], locations[i + 1], c);
            }
        }

        /// <summary>
        /// Updates a color swatch (hex text + swatch label) and recolors the matching pixels in the
        /// given picture box's bitmap.
        /// </summary>
        private void SetColorUI(Control hexLabel, Label swatchLabel, PictureBox box, int[] positions, string hex4)
        {
            Color c = SnesHexToColor(hex4);
            hexLabel.Text = hex4;
            swatchLabel.BackColor = c;
            Bitmap bmp = new Bitmap(box.Image);
            SetBitmapColor(bmp, c, positions);
            box.Image = bmp;
        }

        /// <summary>
        /// Opens the SNES-constrained color picker seeded with the swatch's current color, titled with
        /// which specific field is being edited (e.g. "Uniform 1 Jersey Medium Color"), and on OK,
        /// applies it and splices the change back into Data.
        /// </summary>
        private void EditColor(Control hexLabel, Label swatchLabel, PictureBox box, int[] positions, string description)
        {
            using (SnesColorPickerForm dlg = new SnesColorPickerForm())
            {
                dlg.Text = description + " Color";
                dlg.SnesColor = hexLabel.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    SetColorUI(hexLabel, swatchLabel, box, positions, dlg.SnesColor);
                    ReplaceColorData();
                }
            }
        }

        #endregion

        #region Button click handlers

        private void mHomeJerseyButtonM_Click(object sender, EventArgs e)
        {
            EditColor(mHomeJerseyLabelM, mHomeJerseyM_c_lab, mHomePictureBox, JerseyM, "Uniform 1 Jersey Medium");
        }
        private void mHomeJerseyM_c_lab_Click(object sender, EventArgs e) { mHomeJerseyButtonM_Click(sender, e); }

        private void mHomeJerseyButtonL_Click(object sender, EventArgs e)
        {
            EditColor(mHomeJerseyLabelL, mHomeJerseyL_c_lab, mHomePictureBox, JerseyL, "Uniform 1 Jersey Light");
        }
        private void mHomeJerseyL_c_lab_Click(object sender, EventArgs e) { mHomeJerseyButtonL_Click(sender, e); }

        private void mHomePantsButtonD_Click(object sender, EventArgs e)
        {
            EditColor(mHomePantsLabelD, mHomePantsD_c_lab, mHomePictureBox, PantsD, "Uniform 1 Pants Dark");
        }
        private void mHomePantsD_c_lab_Click(object sender, EventArgs e) { mHomePantsButtonD_Click(sender, e); }

        private void mHomePantsButtonM_Click(object sender, EventArgs e)
        {
            EditColor(mHomePantsLabelM, mHomePantsM_c_lab, mHomePictureBox, PantsM, "Uniform 1 Pants Medium");
        }
        private void mHomePantsM_c_lab_Click(object sender, EventArgs e) { mHomePantsButtonM_Click(sender, e); }

        private void mHomePantsButtonL_Click(object sender, EventArgs e)
        {
            EditColor(mHomePantsLabelL, mHomePantsL_c_lab, mHomePictureBox, PantsL, "Uniform 1 Pants Light");
        }
        private void mHomePantsL_c_lab_Click(object sender, EventArgs e) { mHomePantsButtonL_Click(sender, e); }

        private void mHomeHelmetButtonD_Click(object sender, EventArgs e)
        {
            EditColor(mHomeHelmetLabelD, mHomeHelmetD_c_lab, mHomePictureBox, HelmetD, "Uniform 1 Helmet Dark");
        }
        private void mHomeHelmetD_c_lab_Click(object sender, EventArgs e) { mHomeHelmetButtonD_Click(sender, e); }

        private void mHomeHelmetButtonM_Click(object sender, EventArgs e)
        {
            EditColor(mHomeHelmetLabelM, mHomeHelmetM_c_lab, mHomePictureBox, HelmetM, "Uniform 1 Helmet Medium");
        }
        private void mHomeHelmetM_c_lab_Click(object sender, EventArgs e) { mHomeHelmetButtonM_Click(sender, e); }

        private void awayJerseyButtonM_Click(object sender, EventArgs e)
        {
            EditColor(mAwayJerseyLabelM, mAwayJerseyM_c_lab, pictureBox1, JerseyM, "Uniform 2 Jersey Medium");
        }
        private void mAwayJerseyM_c_lab_Click(object sender, EventArgs e) { awayJerseyButtonM_Click(sender, e); }

        private void awayJerseyButtonL_Click(object sender, EventArgs e)
        {
            EditColor(mAwayJerseyLabelL, mAwayJerseyL_c_lab, pictureBox1, JerseyL, "Uniform 2 Jersey Light");
        }
        private void mAwayJerseyL_c_lab_Click(object sender, EventArgs e) { awayJerseyButtonL_Click(sender, e); }

        private void button7_Click(object sender, EventArgs e) // Away Pants Dark
        {
            EditColor(label17, mAwayPantsD_c_lab, pictureBox1, PantsD, "Uniform 2 Pants Dark");
        }
        private void mAwayPantsD_c_lab_Click(object sender, EventArgs e) { button7_Click(sender, e); }

        private void button6_Click(object sender, EventArgs e) // Away Pants Medium
        {
            EditColor(label16, mAwayPantsM_c_lab, pictureBox1, PantsM, "Uniform 2 Pants Medium");
        }
        private void mAwayPantsM_c_lab_Click(object sender, EventArgs e) { button6_Click(sender, e); }

        private void button5_Click(object sender, EventArgs e) // Away Pants Light
        {
            EditColor(label15, mAwayPantsL_c_lab, pictureBox1, PantsL, "Uniform 2 Pants Light");
        }
        private void mAwayPantsL_c_lab_Click(object sender, EventArgs e) { button5_Click(sender, e); }

        private void button2_Click(object sender, EventArgs e) // Away Helmet Dark
        {
            EditColor(label11, mAwayHelmetD_c_lab, pictureBox1, HelmetD, "Uniform 2 Helmet Dark");
        }
        private void mAwayHelmetD_c_lab_Click(object sender, EventArgs e) { button2_Click(sender, e); }

        private void button1_Click(object sender, EventArgs e) // Away Helmet Medium
        {
            EditColor(label10, mAwayHelmetM_c_lab, pictureBox1, HelmetM, "Uniform 2 Helmet Medium");
        }
        private void mAwayHelmetM_c_lab_Click(object sender, EventArgs e) { button1_Click(sender, e); }

        private HomeAwayUniformForm mHomeAwayUniformForm = new HomeAwayUniformForm();

        private void mEditUniformUsageButton_Click(object sender, EventArgs e)
        {
            mHomeAwayUniformForm.StringValue = m_UniformUsageString;
            if (mHomeAwayUniformForm.ShowDialog(this) == DialogResult.OK)
            {
                m_UniformUsageString = mHomeAwayUniformForm.StringValue;
                ReplaceColorData();
            }
        }

        /// <summary>
        /// Every color pick already applied to this form's own m_Data (see EditColor/ReplaceColorData)
        /// -- never to the parent form, which only ever reads Data back out once, after this dialog
        /// closes with OK. Cancel discards those local edits by restoring Data to what it was when the
        /// form was opened, rather than closing with them still baked in and awaiting an OK that would
        /// have propagated them. mCancelButton.DialogResult is already set to Cancel in the
        /// designer, so this only needs to undo the data; the close itself is automatic.
        /// </summary>
        private void mCancelButton_Click(object sender, EventArgs e)
        {
            m_Data = m_OriginalData;
        }

        #endregion
    }
}
