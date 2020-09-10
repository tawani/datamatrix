using System;
using System.Collections.Generic;
using System.Linq;

namespace WhichMan.Analytics.Functions
{
    public static class PercentileRank
    {
        public static object Initialize(object[][] dependsOn)
        {
            if (dependsOn.Length == 0)
                return null;
            var values = dependsOn[0].Select(Convert.ToInt32).OrderBy(c => c).ToArray();
            var dict = new Dictionary<int, int>();
            foreach (var value in values)
            {
                if (dict.ContainsKey(value))
                    dict[value]++;
                else
                    dict.Add(value, 1);
            }

            return dict;
        }

        public static object Compute(object[] values, object args)
        {
            if (values.Length == 0)
                return null;
            var dict = args as Dictionary<int, int>;
            if (dict == null)
                return null;
            var value = Convert.ToInt32(values[0]);
            var n = dict.Values.Sum();
            var i = dict.Where(c => c.Key <= value).Select(c => c.Value).Sum();
            return Math.Round((100 * (i - 0.5)) / n, 3);
        }
    }
}
