using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TSBTool2
{
    /// <summary>
    /// Static utility functions that I don't want to clutter up other files with.
    /// </summary>
    public static class StaticUtils
    {
        private static Control form = null;

        public static Image GetImage(string file)
        {
            Image ret = null;
            try
            {
                if (form == null)
                    form = new SearchTextBox();
                System.IO.Stream s =
                    form.GetType().Assembly.GetManifestResourceStream(file);
                if (s != null)
                    ret = Image.FromStream(s);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return ret;
        }

        /// <summary>
        /// Returns filename on 'OK' null on 'cancel'.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string GetFileName(string filter, bool saveFileDlg)
        {
            string ret = null;
            FileDialog dlg;
            if (saveFileDlg)
            {
                dlg = new SaveFileDialog();
            }
            else
            {
                dlg = new OpenFileDialog();
            }
            dlg.CheckFileExists = false;
            dlg.RestoreDirectory = true;
            //dlg.Filter="nes files (*.nes)|*.nes";
            if (filter != null && filter.Length > 0)
                dlg.Filter = filter;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ret = dlg.FileName;
            }
            return ret;
        }

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

        public static string MapAttributes(byte[] attrs)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in attrs)
            {
                builder.Append(MapAbilityToTSBValue(b).ToString());
                builder.Append(",");
            }
            return builder.ToString();
        }

        public static byte[] GetTsbAbilities(int[] abilities)
        {
            byte[] retVal = new byte[abilities.Length];
            for (int i = 0; i < retVal.Length; i++)
                retVal[i] = GetTSBAbility(abilities[i]);
            return retVal;
        }

        public static byte GetTSBAbility(int ab)
        {
            byte ret = 0;
            switch (ab)
            {
                case 6: ret = 0x00; break;
                case 13: ret = 0x01; break;
                case 19: ret = 0x02; break;
                case 25: ret = 0x03; break;
                case 31: ret = 0x04; break;
                case 38: ret = 0x05; break;
                case 44: ret = 0x06; break;
                case 50: ret = 0x07; break;
                case 56: ret = 0x08; break;
                case 63: ret = 0x09; break;
                case 69: ret = 0x0a; break;
                case 75: ret = 0x0b; break;
                case 81: ret = 0x0c; break;
                case 88: ret = 0x0d; break;
                case 94: ret = 0x0e; break;
                case 100: ret = 0x0f; break;
            }
            return ret;
        }

        public static byte MapAbilityToTSBValue(int ab)
        {
            byte ret = 0;
            switch (ab)
            {
                case 0x00: ret = 6; break;
                case 0x01: ret = 13; break;
                case 0x02: ret = 19; break;
                case 0x03: ret = 25; break;
                case 0x04: ret = 31; break;
                case 0x05: ret = 38; break;
                case 0x06: ret = 44; break;
                case 0x07: ret = 50; break;
                case 0x08: ret = 56; break;
                case 0x09: ret = 63; break;
                case 0x0A: ret = 69; break;
                case 0x0B: ret = 75; break;
                case 0x0C: ret = 81; break;
                case 0x0D: ret = 88; break;
                case 0x0E: ret = 94; break;
                case 0x0F: ret = 100; break;
            }
            return ret;
        }

        public static byte CombineNibbles(byte first, byte second)
        {
            int retVal = first << 4;
            retVal += second;
            return (byte)retVal;
        }

        public static byte GetFirstNibble(byte b)
        {
            byte retVal = (byte)(b >> 4);
            return retVal;
        }

        public static byte GetSecondNibble(byte b)
        {
            byte retVal = (byte)(b & 0x0f);
            return retVal;
        }

        internal static void CheckTSB2Args(int season, string team)
        {
            if (season < 1 || season > 3)
                throw new ArgumentException("Invalid season! " + season);
            if (TSB2Tool.teams.IndexOf(team) < 0)
                throw new ArgumentException("Invalid team! " + team);
        }

        internal static void CheckTSB2Args(int season, string team, string position)
        {
            CheckTSB2Args(season, team);
            if (TSB2Tool.positionNames.IndexOf(position) < 0)
                throw new ArgumentException("Invalid position! " + position);
        }
        static string RomVersion { get { return "SNES_TSB2"; } }

        private static Regex simpleSetRegex;

        public static void ApplySet(string line, ITecmoTool tool)
        {
            if (simpleSetRegex == null)
                simpleSetRegex = new Regex("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");

            if (simpleSetRegex.Match(line) != Match.Empty)
            {
                ApplySimpleSet(line, tool);
            }
            else if (line.IndexOf("PromptUser", StringComparison.OrdinalIgnoreCase) > -1)
            {
                if (line.IndexOf(RomVersion, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    // good to go! apply it
                    string simpleSetLine = StringInputDlg.PromptForSetUserInput(line);
                    if (!string.IsNullOrEmpty(simpleSetLine))
                    {
                        ApplySet(simpleSetLine, tool);
                    }
                }
                else
                {
                    //StaticUtils.ShowError("Rom version not specified in Hack: " + line);
                    StaticUtils.AddError(string.Format("line '{0}' not applied,", line));
                }
            }
            else
            {
                StaticUtils.AddError(string.Format("ERROR with line \"{0}\"", line));
            }
        }

        private static void ApplySimpleSet(string line, ITecmoTool tool)
        {
            if (simpleSetRegex == null)
                simpleSetRegex = new Regex("SET\\s*\\(\\s*(0x[0-9a-fA-F]+)\\s*,\\s*(0x[0-9a-fA-F]+)\\s*\\)");

            Match m = simpleSetRegex.Match(line);
            if (m == Match.Empty)
            {
                StaticUtils.ShowError(string.Format("SET function not used properly. incorrect syntax>'{0}'", line));
                return;
            }
            string loc = m.Groups[1].ToString().ToLower();
            string val = m.Groups[2].ToString().ToLower();
            loc = loc.Substring(2);
            val = val.Substring(2);
            if (val.Length % 2 != 0)
                val = "0" + val;

            try
            {
                int location = Int32.Parse(loc, System.Globalization.NumberStyles.AllowHexSpecifier);
                byte[] bytes = GetHexBytes(val);
                if (location + bytes.Length > tool.OutputRom.Length)
                {
                    StaticUtils.ShowError(string.Format("ApplySet:> Error with line {0}. Data falls off the end of rom.\n", line));
                }
                else if (location < 0)
                {
                    StaticUtils.ShowError(string.Format("ApplySet:> Error with line {0}. location is negative.\n", line));
                }
                else
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        tool.SetByte(location + i, bytes[i]);
                        //outputRom[location + i] = bytes[i];
                    }
                }
            }
            catch (Exception e)
            {
                StaticUtils.ShowError(string.Format("ApplySet:> Error with line {0}.\n{1}", line, e.Message));
            }
        }

        public static byte[] GetHexBytes(string input)
        {
            if (input == null)
                return null;
            if (input.Length > 2 &&(input.StartsWith("0x") || input.StartsWith("0X")))
                input = input.Substring(2);

            byte[] ret = new byte[input.Length / 2];
            string b = "";
            int tmp = 0;
            int j = 0;

            for (int i = 0; i < input.Length; i += 2)
            {
                b = input.Substring(i, 2);
                tmp = Int32.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier);
                ret[j++] = (byte)tmp;
            }
            return ret;
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
            MessageBox.Show(null, error, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            /*if (MainClass.GUI_MODE)
            {
                MessageBox.Show(null, error, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                Console.Error.WriteLine(error);*/
        }


        public static byte[] ReadRom(string filename)
        {
            byte[] outputRom = null;
            try
            {
                FileInfo f1 = new FileInfo(filename);
                long len = f1.Length;
                FileStream s1 = new FileStream(filename, FileMode.Open);
                outputRom = new byte[(int)len];
                s1.Read(outputRom, 0, (int)len);
                s1.Close();
            }
            catch (Exception e)
            {
                ShowError(e.ToString());
            }
            return outputRom;
        }

        public static void SaveRom(string filename, byte[] outputRom)
        {
            if (filename != null)
            {
                try
                {
                    long len = outputRom.Length;
                    FileStream s1 = new FileStream(filename, FileMode.OpenOrCreate);
                    s1.Write(outputRom, 0, (int)len);
                    s1.Close();
                }
                catch (Exception e)
                {
                    ShowError(e.ToString());
                }
            }
            else
            {
                AddError("ERROR! You passed a null filename");
            }
        }

        /// <summary>
        /// Updates strng pointers
        /// </summary>
        /// <param name="pos">The position of the current pointer</param>
        /// <param name="change">the amount of change</param>
        /// <param name="lastPointer">the last pointer to update.</param>
        private static void AdjustDataPointers(byte[] rom, int firstPointerLocation, int change, int lastPointerLocation)
        {
            byte low, hi;
            int word;
            int i = 0;
            int end = lastPointerLocation + 1;
            for (i = firstPointerLocation + 2; i < end; i += 2)
            {
                low = rom[i];
                hi = rom[i + 1];
                word = hi;
                word = word << 8;
                word += low;
                word += change;
                low = (byte)(word & 0x00ff);
                word = word >> 8;
                hi = (byte)word;
                rom[i] = low;
                rom[i + 1] = hi;
            }
        }

        private static void ShiftDataUp(int startPos, int endPos, int shiftAmount, byte[] data)
        {
            if (startPos < 0 || endPos < 0)
                throw new Exception(string.Format("ERROR! (low level) ShiftDataUp:: either startPos {0} or endPos {1} is invalid.", startPos, endPos));

            int i;
            if (shiftAmount > 0)
                Console.WriteLine("positive shift amount in ShiftDataUp");

            for (i = startPos; i <= endPos; i++)
                data[i + shiftAmount] = data[i];

            /*i += shiftAmount;
            while (outputRom[i] != 0xff && i < 0x300f) { // with this commented out, there will be junk at the end that looks kinda valid, but is just left over
                SetByte(i, 0xff);
                i++;
            }*/
        }

        private static void ShiftDataDown(int startPos, int endPos, int shiftAmount, byte[] data)
        {
            if (startPos < 0 || endPos < 0)
                throw new Exception(string.Format("ERROR! (low level) ShiftDataDown:: either startPos {0} or endPos {1} is invalid.", startPos, endPos));

            for (int i = endPos + shiftAmount; i > startPos; i--)
                data[i] = data[i - shiftAmount];
        }

        public static void SetStringTableString(byte[] rom, int stringIndex, string newValue,
            int firstPointer, int offset, int numberOfStringsInTable, int stringTableSizeInBytes)
        {
            int junk;
            string oldValue = GetStringTableString(rom, stringIndex, firstPointer, offset);
            if (oldValue == newValue)
                return;
            int shiftAmount = newValue.Length - oldValue.Length;
            if (shiftAmount != 0)
            {
                int currentPointerLocation = firstPointer + 2 * stringIndex;
                int lastPointerLocation = firstPointer + 2 * numberOfStringsInTable;
                AdjustDataPointers(rom, currentPointerLocation, shiftAmount, lastPointerLocation);
                int startPosition = GetStringTableStringLocation(rom, (stringIndex + 1) * 2 + firstPointer, out junk, offset);
                int endPosition = firstPointer + stringTableSizeInBytes;
                if (shiftAmount < 0)
                    ShiftDataUp(startPosition, endPosition, shiftAmount, rom);
                else if (shiftAmount > 0)
                    ShiftDataDown(startPosition, endPosition, shiftAmount, rom);
            }
            // lay down the value
            int startLoc = GetStringTableStringLocation(rom, stringIndex * 2 + firstPointer, out junk, offset);
            for (int i = 0; i < newValue.Length; i++)
            {
                if (newValue[i] == '*') // do the star substitution
                    rom[startLoc + i] = 0;
                else
                    rom[startLoc + i] = (byte)newValue[i];
            }
        }

        public static string GetStringTableString(byte[] rom, int string_index, int firstPointer, int offset)
        {
            string retVal = "";
            int pointer = string_index * 2 + firstPointer;
            int length = -1;

            int location = GetStringTableStringLocation(rom, pointer, out length, offset);
            if (length > 0)
            {
                char[] stringChars = new char[length];
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = (char)rom[location + i];
                    if (stringChars[i] == 0)
                        stringChars[i] = '*';
                }
                retVal = new string(stringChars);
            }
            return retVal;
        }


        private static int GetStringTableStringLocation(byte[] rom, int pointerLocation, out int length, int offset)
        {
            int pointer_loc = pointerLocation;
            byte b1 = rom[pointer_loc + 1];
            byte b2 = rom[pointer_loc];
            byte b3 = rom[pointer_loc + 3]; // b3 & b4 for length
            byte b4 = rom[pointer_loc + 2];
            length = ((b3 << 8) + b4) - ((b1 << 8) + b2);
            int pointerVal = (b1 << 8) + b2;
            int stringStartingLocation = pointerVal + offset;// 0x1e0000;
            return stringStartingLocation;
        }

        internal static string AreEqual(string str1, string str2)
        {
            string retVal = "";
            if (str1 != str2)
            {
                retVal = string.Format("AreEqual:Failure '{0}' and '{1}'\n", str1, str2);
                System.Diagnostics.Debugger.Log(1, "TEST", retVal);
            }
            return retVal;
        }
    }
}
