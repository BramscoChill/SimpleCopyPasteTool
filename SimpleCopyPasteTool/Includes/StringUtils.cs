using System;
using System.IO;

namespace SimpleCopyPasteTool.Includes
{
    public static class StringUtils
    {
        public static string TrimUnixEnters(this string input)
        {
            return input.Replace("\r\n", "\n\r");
        }
        public static string ToSigleNewline(this string input)
        {
            return input.Replace("\r", string.Empty);
        }
        public static bool IsNullOrEmpty(this string input)
        {
            return input == null || string.IsNullOrEmpty(input.Trim());
        }
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }
        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }
    }
}