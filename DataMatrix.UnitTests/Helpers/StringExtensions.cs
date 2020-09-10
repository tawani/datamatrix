using System;
using System.Collections.Generic;
using System.Linq;

namespace DataMatrix.UnitTests.Helpers
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string s)
        {
            //return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
            return new string(CharsToTitleCase(s).ToArray());
        }

        static IEnumerable<char> CharsToTitleCase(string s)
        {
            bool newWord = true;
            foreach (char c in s)
            {
                if (newWord) { yield return Char.ToUpper(c); newWord = false; }
                else yield return Char.ToLower(c);
                if (c == ' ') newWord = true;
            }
        }

        /// <summary>
        /// Checks if the String can be converted into a numeric value. 
        /// </summary>
        /// <remarks>null or an empty string will return false.</remarks>
        /// <param name="str">the String to check, may be null </param>
        /// <returns>true if it can be parsed into a numeric (decimal) value</returns>
        public static bool IsNumeric(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            decimal tmp;
            return decimal.TryParse(str, out tmp);
        }

    }
}