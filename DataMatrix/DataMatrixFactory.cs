using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WhichMan.Analytics.Utils;

namespace WhichMan.Analytics
{
    public static class DataMatrixFactory
    {
        #region - DataTable -

        public static IDataMatrix Create(DataTable table, params DataMatrixColumn[] dependentColumns)
        {
            return Create(table, "*", dependentColumns);
        }

        public static IDataMatrix Create(DataTable table, string column, params DataMatrixColumn[] dependentColumns)
        {
            var columns = column == "*" || string.IsNullOrWhiteSpace(column)
                ? null
                : column.Split(',').Select(c => c.Trim());
            return Create(table, columns, dependentColumns);
        }

        public static IDataMatrix Create(DataTable table, IEnumerable<string> columns,
            params DataMatrixColumn[] dependentColumns)
        {
            var allColumns = (from DataColumn c in table.Columns select c.ColumnName).ToArray();


            var cols = GetColumns(allColumns, columns?.ToArray(), dependentColumns);
            var colIndexes = GetDependencySortOrder(cols);

            //create data matrix
            var dm = new DataMatrixLite(table.Rows.Count, cols.ToArray());
            var rowIndex = 0;
            foreach (DataRow row in table.Rows)
            {
                foreach (var colIndex in colIndexes)
                {
                    var myCol = cols[colIndex];
                    if (myCol.DependsOn == null)
                        dm[rowIndex][colIndex] = row[myCol.Name];
                }
                rowIndex++;
            }

            //update computed columns
            dm.ComputeDependentCols(colIndexes);

            return dm;
        }

        #endregion

        #region - List -

        public static IDataMatrix Create<T>(IEnumerable<T> values,
            Expression<Func<T, object>>[] valueSelectors, params DataMatrixColumn[] dependentColumns)
        {
            var headers = valueSelectors.Select(func => GetProperty(func).Name).ToArray();
            var selectors = valueSelectors.Select(exp => exp.Compile()).ToArray();
            return Create(values.ToArray(), headers, selectors, dependentColumns);
        }

        public static IDataMatrix Create<T>(IEnumerable<T> values, string[] columnHeaders,
            Func<T, object>[] valueSelectors, params DataMatrixColumn[] dependentColumns)
        {
            Type fieldsType = typeof(T);
            PropertyInfo[] props = fieldsType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var allColumns = (from PropertyInfo c in props select c.Name).ToArray();
            
            var cols = GetColumns(allColumns, columnHeaders?.ToArray(), dependentColumns, false);
            var colIndexes = GetDependencySortOrder(cols);

            //create data matrix
            var myValues = values.ToArray();
            var dm = new DataMatrixLite(myValues.Length, cols.ToArray());
            
            for (var rowIndex = 0; rowIndex < myValues.Length; rowIndex++)
            {
                foreach (var colIndex in colIndexes)
                {
                    var myCol = cols[colIndex];
                    if (myCol.DependsOn == null)
                        dm[rowIndex][colIndex] = valueSelectors[colIndex].Invoke(myValues[rowIndex]);
                }
            }

            //update computed columns
            dm.ComputeDependentCols(colIndexes);

            return dm;
        }

        internal static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expression)
        {
            if (expression.Body is UnaryExpression)
            {
                if ((expression.Body as UnaryExpression).Operand is MemberExpression)
                {
                    return ((expression.Body as UnaryExpression).Operand as MemberExpression).Member as PropertyInfo;
                }
            }

            if ((expression.Body is MemberExpression))
            {
                return (expression.Body as MemberExpression).Member as PropertyInfo;
            }
            return null;
        }

        #endregion

        #region - 2D Array -

        public static IDataMatrix Create(object[,] values, string[] columnHeaders)
        {
            Debug.Assert(columnHeaders.Length == values.GetLength(1));

            var columns = GetColumns(columnHeaders, null, new DataMatrixColumn[0], false).ToArray();

            var dm = new DataMatrixLite(values.GetLength(0), columns);

            //load matrix values
            for (var rowIndex = 0; rowIndex < values.GetLength(0); rowIndex++)
            {
                for (var colIndex = 0; colIndex < values.GetLength(1); colIndex++)
                {
                    var myCol = columns[colIndex];
                    if (myCol.Compute == null)
                        dm[rowIndex][colIndex] = values[rowIndex, colIndex];
                }
            }

            return dm;
        }

        #endregion

        #region - Helper Methods -

        internal static void ComputeDependentCols(this IDataMatrix dm, int[] colIndexes)
        {
            //Compute dynamic columns
            foreach (var colIndex in colIndexes)
            {
                var myCol = dm.Columns[colIndex];
                if (myCol.DependsOn == null)
                    continue;

                var dependsOn = GetIndexes(dm.Columns, myCol.DependsOn).ToArray();

                if (myCol.Compute != null)
                {
                    object args = null;
                    if (myCol.Initialize != null)
                    {
                        var colGroups = new object[dependsOn.Length][];
                        var k = 0;
                        foreach (var dependsOnIndex in dependsOn)
                        {
                            var colValues = new object[dm.RowCount];

                            for (var i = 0; i < dm.RowCount; i++)
                            {
                                colValues[i] = dm[i][dependsOnIndex];
                            }
                            colGroups[k++] = colValues;
                        }
                        args = myCol.Initialize(colGroups);
                    }

                    for (var i = 0; i < dm.RowCount; i++)
                    {
                        var values = new object[dependsOn.Length];
                        for (int j = 0; j < dependsOn.Length; j++)
                        {
                            values[j] = dm[i][dependsOn[j]];
                        }
                        dm[i][colIndex] = myCol.Compute(values, args);
                    }
                }
            }
        }

        internal static IReadOnlyList<DataMatrixColumn> GetColumns(string[] allColumns, 
            string[] selectedColumns, DataMatrixColumn[] dependentColumns, bool verifyColumns = true)
        {
            var index = 0;
            var dict = new Dictionary<string, DataMatrixColumn>();
            if (selectedColumns == null || !selectedColumns.Any())
            {
                dict = allColumns.Select((c, i) => new DataMatrixColumn { Name = c, Index = i })
                    .ToDictionary(c => c.Name.ToUpper(), c => c);
                index = dict.Count;
            }
            else
            {
                foreach (var name in selectedColumns)
                {
                    var key = name.ToUpper();
                    if (dict.ContainsKey(key))
                        throw new DuplicateColumnException(name);
                    if (verifyColumns && allColumns.All(c => c.ToUpper() != key))
                        throw new ColumnNotFoundException(name);
                    dict[key] = new DataMatrixColumn { Name = name, Index = index++ };
                }
            }

            //add dependent columns
            foreach (var column in dependentColumns)
            {
                var key = column.Name.ToUpper();
                if (dict.ContainsKey(key))
                    throw new DuplicateColumnException(column.Name);
                column.Index = index++;
                dict[key] = column;
            }

            //add [hidden] columns required by dependent columns
            foreach (var column in dependentColumns)
            {
                foreach (var name in column.DependsOn)
                {
                    var key = name.ToUpper();
                    if (dict.ContainsKey(key))
                        continue;
                    dict[key] = new DataMatrixColumn { Name = name, Index = index++, Hidden = true };
                }
            }

            return dict.Values.ToArray();
        }

        internal static int[] GetDependencySortOrder(IReadOnlyList<DataMatrixColumn> fields)
        {
            var g = new DependencySorter<int>();
            g.AddObjects(fields.Select(c => c.Index).ToArray());

            //add edges
            foreach (var field in fields)
            {
                if (field.DependsOn != null)
                {
                    var dependsOn = GetIndexes(fields, field.DependsOn).ToArray();
                    g.SetDependencies(field.Index, dependsOn);
                }
            }

            var result = g.Sort();
            return result;
        }

        private static IEnumerable<int> GetIndexes(IReadOnlyList<DataMatrixColumn> fields, string[] dependsOn)
        {
            foreach (var name in dependsOn)
            {
                yield return fields.First(c => c.Name == name).Index;
            }
        }

        #endregion
    }
}