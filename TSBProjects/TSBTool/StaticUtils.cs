using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TSBTool
{
    /// <summary>
    /// Static utility functions that I don't want to clutter up other files with.
    /// </summary>
    public static class StaticUtils
    {
        /// <summary>
        /// Returns a string starting with 'Error!' on error condition, the locations of the replacements otherwise.
        /// </summary>
        /// <param name="searchStr">The string to search for.</param>
        /// <param name="replaceStr">The string to replace it with.</param>
        /// <param name="occurence">The occurence you wish to replace, -1 for all occurences.</param>
        public static string ReplaceStringInRom(byte[] outputRom, string searchStr, string replaceStr, int occurence)
        {
            if (replaceStr.Length > searchStr.Length)
            {
                return String.Format("Error! Replace({0},{1}), cannot replace a string with a longer string", searchStr, replaceStr);
            }
            while (replaceStr.Length < searchStr.Length)
                replaceStr = replaceStr + " ";

            List<long> locs = StaticUtils.FindStringInFile(searchStr, outputRom, 0, outputRom.Length);
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("Replaced '{0}' with '{1}' at location(s):", searchStr, replaceStr));
            for (int i = 0; i < locs.Count; i++)
            {
                if (occurence < 0 || occurence == i)
                {
                    builder.Append(string.Format("0x{0:x},", locs[i]));
                    int stringLoc = (int)locs[i];
                    for (int j = 0; j < replaceStr.Length; j++)
                    {
                        outputRom[stringLoc] = (byte)replaceStr[j];
                        stringLoc++;
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Find string 'str' (unicode string) in the data byte array.
        /// </summary>
        /// <param name="str">The string to look for</param>
        /// <param name="data">The data to search through.</param>
        /// <param name="start">where to start in 'data'</param>
        /// <param name="end">Where to end in 'data'</param>
        /// <returns>a list of addresses</returns>
        public static List<long> FindStringInFile(string str, byte[] data, int start, int end)
        {
            return FindStringInFile(str, data, start, end, false);
        }


        /// <summary>
        /// Find string 'str' (unicode string) in the data byte array.
        /// </summary>
        /// <param name="str">The string to look for</param>
        /// <param name="data">The data to search through.</param>
        /// <param name="start">where to start in 'data'</param>
        /// <param name="end">Where to end in 'data'</param>
        /// <param name="nullByte">True to append the null byte at the end.</param>
        /// <returns>a list of addresses</returns>
        public static List<long> FindStringInFile(string str, byte[] data, int start, int end, bool nullByte)
        {
            List<long> retVal = new List<long>();
            int length = str.Length;
            if (nullByte)
                length += 1;

            byte[] target = new byte[length];
            int i = 0;
            Array.Clear(target, 0, target.Length); // fill with 0's
            foreach (char c in str)
            {
                target[i++] = (byte)c;
            }
            return FindByesInFile(target, data, start, end);
        }

        /// <summary>
        /// Find an array of bytes in the data byte array.
        /// </summary>
        /// <param name="str">The bytes to look for</param>
        /// <param name="data">The data to search through.</param>
        /// <param name="start">where to start in 'data'</param>
        /// <param name="end">Where to end in 'data'</param>
        /// <returns>a list of addresses</returns>
        public static List<long> FindByesInFile(byte[] target, byte[] data, int start, int end)
        {
            List<long> retVal = new List<long>();

            if (data != null && data.Length > 80)
            {
                if (start < 0)
                    start = 0;
                if (end > data.Length)
                    end = data.Length - 1;

                long num = (long)(end - target.Length);
                for (long num3 = start; num3 < num; num3 += 1L)
                {
                    if (Check(target, num3, data))
                    {
                        retVal.Add(num3);
                    }
                }
            }
            return retVal;
        }

        private static bool Check(byte[] target, long location, byte[] data)
        {
            int i;
            for (i = 0; i < target.Length; i++)
            {
                if (target[i] != data[(int)(checked((IntPtr)(unchecked(location + (long)i))))])
                {
                    break;
                }
            }
            return i == target.Length;
        }


        private static List<string> sErrors = new List<string>();

        public static void AddError(string error)
        {
            sErrors.Add(error);
        }

        public static void ClearErrors()
        {
            sErrors.Clear();
        }

        public static void ShowErrors()
        {
            if (sErrors != null && sErrors.Count > 0)
            {
                StringBuilder sb = new StringBuilder(500);
                foreach (string e in sErrors)
                {
                    sb.Append(e + "\n");
                }
                ShowError(sb.ToString());
                ClearErrors();
            }
        }

        public static void ShowError(string error)
        {
            if (MainClass.GUI_MODE)
            {
                MessageBox.Show(null, error, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                Console.Error.WriteLine(error);
        }



        private static Regex tsb1QB1Regex = new Regex(
            "^QB1\\s*,[a-zA-Z 0-9]+\\s*,\\s*Face=0x[0-9]{1,2}\\s*,\\s*#[0-9]{1,2}\\s*,(\\s*[0-9]{1,2}\\s*,){7}(\\s*[0-9]{1,2}\\s*,?){1}(\\s*\\[|\\s*$)",
             RegexOptions.Multiline);

        private static Regex tsb2QB1Regex = new Regex(
            "^QB1\\s*,[a-zA-Z 0-9]+\\s*,\\s*Face=0x[0-9]{1,2}\\s*,\\s*#[0-9]{1,2}\\s*,(\\s*[0-9]{1,2}\\s*,){9}(\\s*[0-9]{1,2}\\s*,?){1}(\\s*\\[|\\s*$)",
            RegexOptions.Multiline);
        private static Regex tsb3QB1Regex = new Regex(
            "^QB1\\s*,[a-zA-Z 0-9\\.]+\\s*,\\s*Face=0x[08][0-9A-Fa-f]{1}\\s*,\\s*#[0-9]{1,2}\\s*,(\\s*[0-9]{1,2}\\s*,){10}(\\s*[0-9]{1,2}\\s*,?){1}(\\s*\\[|\\s*$)",
            RegexOptions.Multiline);

        internal static bool IsTSB1Content(string data)
        {
            bool retVal = false;
            MatchCollection mc = tsb1QB1Regex.Matches(data);
            if (mc.Count > 0)
            {
                retVal = true;
            }
            return retVal;
        }

        internal static bool IsTSB2Content(string data)
        {
            bool retVal = false;
            MatchCollection mc = tsb2QB1Regex.Matches(data);
            if (mc.Count > 0)
            {
                retVal = true;
            }
            return retVal;
        }

        internal static bool IsTSB3Content(string data)
        {
            bool retVal = false;
            MatchCollection mc = tsb3QB1Regex.Matches(data);
            if (mc.Count > 0)
            {
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Returns the content type (TSB1, TSB2, TSB3, Unknown)
        /// </summary>
        internal static TSBContentType GetContentType(string data)
        {
            if (IsTSB1Content(data))
                return TSBContentType.TSB1;
            if (IsTSB2Content(data))
                return TSBContentType.TSB2;
            if (IsTSB3Content(data))
                return TSBContentType.TSB3;
            return TSBContentType.Unknown;
        }

    }
}