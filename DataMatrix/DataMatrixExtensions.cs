using System.Data;
using System.Linq;

namespace WhichMan.DataAnalytics
{
    public static class DataMatrixExtensions
    {
        public static DataTable ToDataTable(this IDataMatrix dm)
        {

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
    }
}