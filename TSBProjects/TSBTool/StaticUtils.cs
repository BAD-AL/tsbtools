using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;

namespace TSBTool
{
    /// <summary>
    /// Static utility functions that I don't want to clutter up other files with.
    /// </summary>
    public static class StaticUtils
    {
        private static Dictionary<string,Image> sImageMap = null;
        private static Dictionary<string, Image> ImageMap 
        {
            get
            {
                if (sImageMap == null)
                    sImageMap = new Dictionary<string, Image>();
                return sImageMap;
            }
        }
        private static ColorInfo ci = null;
        /// <summary>
        /// Get an image from the assembly; caches the image.
        /// </summary>
        public static Image GetEmbeddedImage(string file)
        {
            Image ret = null;
            try
            {
                if (ci == null)
                    ci = new ColorInfo();
                if (ImageMap.ContainsKey(file))
                    ret = ImageMap[file];
                else
                {
                    System.IO.Stream s =
                        ci.GetType().Assembly.GetManifestResourceStream(file);
                    if (s != null)
                        ret = Image.FromStream(s);
                    sImageMap.Add(file, ret);
                }
            }
            catch (Exception )
            {
                Console.Error.WriteLine("Error getting image "+ file);
            }
            return ret;
        }

        /// <summary>
        /// Gets an image from the path; caches the image.
        /// </summary>
        public static Image GetImageFromPath(string path)
        {
            Image ret = null;

            if (ImageMap.ContainsKey(path))
                ret = ImageMap[path];
            else if( File.Exists(path))
            {
                ret = Image.FromFile(path);
                ImageMap.Add(path, ret);
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
    }
}
