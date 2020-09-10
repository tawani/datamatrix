using System.Data;
using System.Linq;

namespace WhichMan.Analytics.Utils
{
    public static class DataMatrixExtensions
    {
        public static DataTable ToDataTable(this IDataMatrix dm, params string[] columns)
        {
            if (columns?.Length > 0)
                return dm.ToDataTableWithCols(columns);

            var table = new DataTable();
            for (var i = 0; i < dm.Columns.Count; i++)
            {
                if (dm.Columns[i].Hidden)
                    continue;

                table.Columns.Add(dm.Columns[i].Name);
            }

            foreach (object[] row in dm)
            {
                object[] values = new object[dm.Columns.Count(c => !c.Hidden)];
                for (int i = 0; i < values.Length; i++)
                {
                    if (dm.Columns[i].Hidden)
                        continue;

                    values[i] = row[i];
                }
                table.Rows.Add(values);
            }
            return table;
        }

        private static DataTable ToDataTableWithCols(this IDataMatrix dm, string[] columns)
        {
            var colIndexes = new int[columns.Length];


            var table = new DataTable();

            var cols = dm.Columns.Where(c => !c.Hidden).Select(c => c.Name.ToUpper()).ToList();

            for (var i = 0; i < columns.Length; i++)
            {
                var name = columns[i].ToUpper();
                var index = cols.FindIndex(c => c == name);
                if (index < 0)
                    continue;

                colIndexes[i] = index;
                table.Columns.Add(dm.Columns[index].Name);
            }

            foreach (object[] row in dm)
            {
                object[] values = new object[colIndexes.Length];
                for (int i = 0; i < colIndexes.Length; i++)
                {
                    values[i] = row[colIndexes[i]];
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}