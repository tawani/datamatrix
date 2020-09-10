using System.Linq;

namespace WhichMan.Analytics.Pivot
{
    internal class PivotEntry
    {
        public string RowId { get; private set; }
        public string ColId { get; private set; }

        private object[] _rowValues;

        public object[] GetRowValues()
        {
            return _rowValues;
        }

        public void SetRowValues(object[] values)
        {
            _rowValues = values;
            RowId = string.Join("\v", values.Select(c => c + "").ToArray());
        }

        private object[] _colValues;

        public object[] GetColValues()
        {
            return _colValues;
        }

        public void SetColValues(object[] values)
        {
            _colValues = values;
            ColId = string.Join("\v", values.Select(c => c + "").ToArray());
        }

        public object DataValue { get; set; }
    }
}
