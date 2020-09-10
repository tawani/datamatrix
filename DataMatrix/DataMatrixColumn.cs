using System;

namespace WhichMan.Analytics
{
    public class DataMatrixColumn
    {
        public string Name { get; set; }
        public string[] DependsOn { get; set; }
        internal int Index { get; set; }
        internal bool Hidden { get; set; }

        public Func<object[][], object> Initialize { get; set; }
        public Func<object[], object, object> Compute { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}