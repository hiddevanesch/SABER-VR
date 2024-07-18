using System.Text.RegularExpressions;

namespace Util
{
    static class StringUtil
    {
        public static string SplitCamelCase(string input)
        {
            return string.Join(" ", Regex.Split(input, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
        }

        public static string ReplaceDotsWithNewlines(string input)
        {
            return input.Replace(".", "\n");
        }
    }
}