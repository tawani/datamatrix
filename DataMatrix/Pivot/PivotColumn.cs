namespace WhichMan.Analytics.Pivot
{
    internal class PivotColumn
    {
        public string Name { get; set; }
        public object[] Values { get; set; }

        public override string ToString()
        {
            return $"{Name}({Values.Length})";
        }
    }
}