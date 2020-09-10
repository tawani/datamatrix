namespace WhichMan.Analytics.Pivot
{
    internal class PivotRow
    {
        public string Row { get; set; }
        public PivotColumn[] Cols { get; set; }

        public override string ToString()
        {
            return $"{Row}({Cols.Length})";
        }
    }
}