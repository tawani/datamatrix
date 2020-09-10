using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace WhichMan.Analytics
{
    public class DataMatrixBuilder
    {
        private string[] _columnHeaders;
        private List<DataMatrixColumn> _dependentColumns;
        private object[,] _arrValues;

        private DataMatrixBuilder(){}

        #region - Create from List -

        public static DataMatrixBuilder Create<T>(IEnumerable<T> values,
            params Expression<Func<T, object>>[] valueSelectors)
        {
            var headers = valueSelectors.Select(func => DataMatrixFactory.GetProperty(func).Name).ToArray();
            var selectors = valueSelectors.Select(exp => exp.Compile()).ToArray();
            return Create(values.ToArray(), headers, selectors);
        }

        public static DataMatrixBuilder Create<T>(IReadOnlyList<T> values, string[] columnHeaders,
            params Func<T, object>[] valueSelectors)
        {
            Debug.Assert(columnHeaders.Length == valueSelectors.Length);

            if(columnHeaders.Length != valueSelectors.Length)
                throw new DataMatrixException($"Column headers '{columnHeaders.Length}' not equal to valueSelectors '{valueSelectors.Length}'");

            var result = new DataMatrixBuilder
            {
                _columnHeaders = columnHeaders,
                _arrValues = new object[values.Count, valueSelectors.Length],
                _dependentColumns = new List<DataMatrixColumn>()
            };

            // Fill table rows
            for (int rowIndex = 0; rowIndex < result._arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < result._arrValues.GetLength(1); colIndex++)
                {
                    result._arrValues[rowIndex, colIndex] = valueSelectors[colIndex]
                        .Invoke(values[rowIndex]);
                }
            }

            return result;
        }

        #endregion

        #region - Create from DataTable -

        #endregion

        #region - Add Columns -

        public DataMatrixBuilder AddColumn(string name, string[] dependsOnColumns, 
            Func<object[], object, object> compute, Func<object[][], object> initialize = null)
        {

            Debug.Assert(!string.IsNullOrWhiteSpace(name));
            Debug.Assert(_dependentColumns.All(c => c.Name.ToUpper() != name.ToUpper()));
            Debug.Assert(_columnHeaders.All(c => c.ToUpper() != name.ToUpper()));

            _dependentColumns.Add(new DataMatrixColumn { Name = name, DependsOn = dependsOnColumns, Compute = compute, Initialize = initialize });
            return this;
        }

        public DataMatrixBuilder AddColumn(string name, string dependsOnColumn, Func<object[], object, object> compute, Func<object[][], object> initialize = null)
        {
            return AddColumn(name, new[] { dependsOnColumn }, compute, initialize);
        }

        #endregion

        public IDataMatrix Build()
        {
            var cols = DataMatrixFactory.GetColumns(_columnHeaders, _columnHeaders, _dependentColumns.ToArray(), false);
            var colIndexes = DataMatrixFactory.GetDependencySortOrder(cols);

            //create data matrix

            var dm = new DataMatrixLite(_arrValues.GetLength(0), cols.ToArray());

            for (var rowIndex = 0; rowIndex < _arrValues.GetLength(0); rowIndex++)
            {
                foreach (var colIndex in colIndexes)
                {
                    var myCol = cols[colIndex];
                    if (myCol.Compute == null)
                        dm[rowIndex][colIndex] = _arrValues[rowIndex, colIndex];
                }
            }

            //update computed columns
            dm.ComputeDependentCols(colIndexes);

            return dm;
        }
    }
}