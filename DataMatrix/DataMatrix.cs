using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WhichMan.Analytics
{
    public interface IDataMatrix : IEnumerable
    {
        IReadOnlyList<DataMatrixColumn> Columns { get; }
        object[] this[int rowIndex] { get; }
        int RowCount { get; }
    }
    
    public class DataMatrix : IDataMatrix
    {
        public IReadOnlyList<DataMatrixColumn> Columns { get; set; }
        public List<object[]> Rows { get; set; }
        public object[] this[int rowIndex] => Rows[rowIndex];

        public int RowCount => Rows.Count;

        public DataMatrix()
        {
            Rows = new List<object[]>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.AsEnumerable().GetEnumerator();
        }

        public void NewRow()
        {
            Rows.Add(new object[Columns.Count]);
        }
    }

    public class DataMatrixLite : IDataMatrix
    {
        public IReadOnlyList<DataMatrixColumn> Columns { get; set; }
        public object[][] Rows { get; set; }
        public object[] this[int rowIndex] => Rows[rowIndex];

        public int RowCount => Rows.Length;

        public DataMatrixLite(int rows, IReadOnlyList<DataMatrixColumn> cols)
        {
            Columns = cols;
            Rows = new object[rows][];
            for (var i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new object[cols.Count];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.AsEnumerable().GetEnumerator();
        }
    }
}
