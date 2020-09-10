using System;
using System.Collections.Generic;
using System.Linq;

namespace WhichMan.Analytics.Pivot
{
    public enum AggregateFunction
    {
        Count = 1,
        Sum = 2,
        Average = 3,
        Minimum = 4,
        Maximum = 5,
        First = 6,
        Last = 7
    }

    public static class PivotExtensions
    {
        public static IDataMatrix Pivot(this IDataMatrix dm, string rowField, string dataField, AggregateFunction aggregate, params string[] columnFields)
        {
            return dm.Pivot(dataField, aggregate, new[] { rowField }, columnFields);
        }

        public static IDataMatrix Pivot(this IDataMatrix dm, string dataField, AggregateFunction aggregate,
            string[] rowFields, string[] columnFields)
        {
            var rowFieldIndexes = dm.GetIndexes(rowFields).ToArray();
            var colFieldIndexes = dm.GetIndexes(columnFields).ToArray();
            var dataFieldIndex = dm.Columns.First(c => c.Name.ToUpper() == dataField.ToUpper()).Index;

            var items = new PivotEntry[dm.RowCount];
            for (var i = 0; i < dm.RowCount; i++)
            {
                var entry = new PivotEntry
                {
                    DataValue = dm[i][dataFieldIndex]
                };

                entry.SetRowValues(dm.GetValues(i, rowFieldIndexes).ToArray());
                entry.SetColValues(dm.GetValues(i, colFieldIndexes).ToArray());

                items[i] = entry;
            }

            var rows = (from p in items
                        group p by p.RowId into g
                        select new PivotRow
                        {
                            Row = g.Key,
                            Cols = g.GroupBy(c => c.ColId, c => c.DataValue, (key, c) => new PivotColumn { Name = key, Values = c.ToArray() }).ToArray()
                        }).ToArray();

            var cols = new string[0];
            foreach (var row in rows)
            {
                cols = cols.Union(row.Cols.Select(c => c.Name)).ToArray();
            }
            Array.Sort(cols);

            var values = new object[rows.Length, cols.Length + 1];
            for (var rowIndex = 0; rowIndex < values.GetLength(0); rowIndex++)
            {
                values[rowIndex, 0] = rows[rowIndex].Row;
                for (var colIndex = 1; colIndex < values.GetLength(1); colIndex++)
                {
                    values[rowIndex, colIndex] = GetCellValue(rows[rowIndex].Cols, cols[colIndex - 1], aggregate);
                }
            }

            var columns = new string[cols.Length + 1];
            columns[0] = string.Join("\v", rowFields);
            for (var i = 0; i < cols.Length; i++)
            {
                columns[i + 1] = cols[i];
            }

            var result = DataMatrixFactory.Create(values, columns);

            return result;
        }

        private static object GetCellValue(PivotColumn[] allCols, string columnName, AggregateFunction aggregate)
        {
            var cols = allCols.Where(c => c.Name == columnName).ToArray();

            if (aggregate == AggregateFunction.Count)
                return cols.Length == 0 ? 0 : cols.Sum(col => col.Values.Count(c => c != null));
            if (aggregate == AggregateFunction.Sum)
                return cols.Length == 0 ? 0 : cols.Sum(col => col.Values.Where(c => c != null).Select(Convert.ToDecimal).Sum());
            if (aggregate == AggregateFunction.Average)
                return cols.Length == 0 ? 0 : cols.Average(col => col.Values.Where(c => c != null).Select(Convert.ToDecimal).Average());
            if (aggregate == AggregateFunction.Minimum)
                return cols.Length == 0 ? 0 : cols.Min(col => col.Values.Where(c => c != null).Select(Convert.ToDecimal).Min());
            if (aggregate == AggregateFunction.Maximum)
                return cols.Length == 0 ? 0 : cols.Max(col => col.Values.Where(c => c != null).Select(Convert.ToDecimal).Max());
            if (aggregate == AggregateFunction.First)
                return cols.Length == 0 ? null : cols.First(col => col != null);
            if (aggregate == AggregateFunction.Last)
                return cols.Length == 0 ? null : cols.Last(col => col != null);

            return null;
        }

        internal static IEnumerable<int> GetIndexes(this IDataMatrix dm, IEnumerable<string> columnNames)
        {
            foreach (var name in columnNames)
            {
                yield return dm.Columns.First(c => c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Index;
            }
        }

        internal static IEnumerable<object> GetValues(this IDataMatrix dm, int rowIndex, IEnumerable<int> columnIndexes)
        {
            foreach (var index in columnIndexes)
            {
                yield return dm[rowIndex][index];
            }
        }
    }
}