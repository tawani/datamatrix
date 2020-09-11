using System;
using System.Linq;

namespace WhichMan.Analytics.Functions
{
    public static class Outlier
    {
        public static object Initialize(object[][] dependsOn)
        {
            if (dependsOn.Length == 0)
                return null;
            var values = dependsOn[0].Where(c => c != null).Select(Convert.ToDecimal).OrderBy(c => c).ToArray();
            if (values.Length == 0)
                return null;

            //get the median
            var mid = 0m;
            var q2 = GetMedian(values, out mid);

            //get the lower quartile
            var values1 = values.Where((c, i) => i < mid).Select(c => c).ToArray();
            var q1 = GetMedian(values1);

            //get the upper quartile
            var values3 = values.Where((c, i) => i > mid).Select(c => c).ToArray();
            var q3 = GetMedian(values3);

            //find the interquartile range
            var iqr = q3 - q1;

            //find the inner fences
            var innerFence = iqr * 1.5m;
            var iub = q3 + innerFence; //inner upper Boundary
            var ilb = q1 - innerFence; //inner lower Boundary

            //find the inner fences
            var outerFence = iqr * 3m;
            var oub = q3 + outerFence; //outer upper Boundary
            var olb = q1 - outerFence; //outer lower Boundary

            return new OutlierBoundaries(iub, ilb, oub, olb);

        }

        class OutlierBoundaries
        {
            public decimal InnerUpperBoundary { get; }
            public decimal InnerLowerBoundary { get; }
            public decimal OuterUpperBoundary { get; }
            public decimal OuterLowerBoundary { get; }

            public OutlierBoundaries(decimal iub, decimal ilb, decimal oub, decimal olb)
            {
                InnerUpperBoundary = iub;
                InnerLowerBoundary = ilb;
                OuterUpperBoundary = oub;
                OuterLowerBoundary = olb;
            }
        }

        public enum OutlierType { None = 0, Minor = 1, Major = 2 }

        private static decimal GetMedian(decimal[] values)
        {
            return GetMedian(values, out var mid);
        }

        private static decimal GetMedian(decimal[] values, out decimal midIndex)
        {
            //get the median
            int size = values.Length;
            //var mid = (int)Math.Ceiling(size / 2m);
            //var median = (size % 2 != 0) ? values[mid] : (values[mid] + values[mid - 1]) / 2;
            //midIndex = (size%2 != 0) ? mid - 1 : mid - 0.5m;

            var mid = (int)Math.Floor(size / 2m);
            if (values.Length - mid == mid + 1)
            {
                midIndex = mid;
                return values[mid];
            }

            midIndex = mid - 1;
            var median = values[mid - 1];
            if (size % 2 == 0)
            {
                median = (median + values[mid]) / 2m;
                midIndex += 0.5m;
            }

            return median;
        }

        public static object Compute(object[] values, object args)
        {
            if (values.Length == 0)
                return null;
            var bn = args as OutlierBoundaries;
            if (bn == null)
                return null;

            var value = Convert.ToInt32(values[0]);
            if (value >= bn.InnerLowerBoundary && value <= bn.InnerUpperBoundary)
                return OutlierType.None;
            if (value >= bn.OuterLowerBoundary && value <= bn.OuterUpperBoundary)
                return OutlierType.Minor;
            return OutlierType.Major;
        }
    }
}
