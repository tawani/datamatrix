using System;
using System.Collections.Generic;
using System.Linq;

namespace WhichMan.Analytics.Functions
{
    public static class StandardDeviation
    {
        public static object Initialize(object[][] dependsOn)
        {
            return Initialize(dependsOn, false);
        }

        public static object InitializeSample(object[][] dependsOn)
        {
            return Initialize(dependsOn, true);
        }

        private static object Initialize(object[][] dependsOn, bool sample)
        {
            if (dependsOn.Length == 0)
                return null;
            var values = dependsOn[0].Where(c => c != null).Select(Convert.ToDecimal).ToArray();
            if (values.Length < 2)
                return null;

            var mean = values.Sum() / values.Length;

            //Then for each number: subtract the Mean and square the result
            var vars = values.Select(c => (c - mean) * (c - mean)).ToArray();

            var size = sample ? vars.Length - 1 : vars.Length;
            var variance = vars.Sum() / size;
            var stdev = (decimal)Math.Sqrt((double)variance);

            return Tuple.Create(mean, stdev);
        }

        public static object Compute(object[] values, object args)
        {
            if (values.Length == 0)
                return null;
            var tpl = args as Tuple<decimal, decimal>;
            if (tpl == null)
                return null;
            var mean = tpl.Item1;
            var stdev = tpl.Item2;
            var value = Convert.ToDecimal(values[0]);
            var deviation = (value - mean) / stdev;
            var result = deviation > 0 ? Math.Ceiling(deviation) : Math.Floor(deviation);
            return (int)result;
        }
    }
}
