using System;
using System.Globalization;

namespace DataMatrix.UnitTests.Helpers
{
    public static class ParsingExtensions
    {
        public static decimal? ToDecimal(this string value)
        {
            if (value != null && value.Trim() == string.Empty)
                return null;
            decimal num;
            if (decimal.TryParse(value, out num))
                return num;
            return null;
        }

        public static DateTime? ToDateTime(this object data)
        {
            var s = string.Format("{0}", data);
            DateTime d;
            if (s.Length == 8 && s.IsNumeric())
            {
                if (DateTime.TryParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                    return d;
            }

            if (DateTime.TryParse(s, out d))
                return d;

            return null;
        }

    }
}