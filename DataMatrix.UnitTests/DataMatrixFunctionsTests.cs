using System;
using System.Collections.Generic;
using System.Linq;
using DataMatrix.UnitTests.Helpers;
using WhichMan.Analytics;
using WhichMan.Analytics.Functions;
using WhichMan.Analytics.Utils;
using Xunit;

namespace DataMatrix.UnitTests
{
    public class DataMatrixFunctionsTests : TestBase
    {
        private readonly List<Student> _students;

        public DataMatrixFunctionsTests()
        {
            _students = LoadStudents();
        }

        [Fact]
        public void Can_compute_by_simple_concat_function()
        {
            var table = _students.ToDataTable();
            var dm = DataMatrixFactory.Create(table, "Score", new DataMatrixColumn
            {
                Name = "Name",
                DependsOn = new[] {"FirstName", "LastName"},
                Compute = (values, args) => values[0] + " " + values[1]
            });
            Assert.Equal(39, dm.RowCount);
            Assert.Equal("David Johnson", dm[0][1]);

            var tb = dm.ToDataTable();
            Assert.Equal(2, tb.Columns.Count);
            Assert.Equal("Score", tb.Columns[0].ColumnName);
            Assert.Equal("Name", tb.Columns[1].ColumnName);
        }

        [Fact]
        public void Can_compute_by_concat_function_and_reorder_DataTable()
        {
            var table = _students.ToDataTable();
            var dm = DataMatrixFactory.Create(table, "Score", new DataMatrixColumn
            {
                Name = "Name",
                DependsOn = new[] {"FirstName", "LastName"},
                Compute = (values, args) => values[0] + " " + values[1]
            });
            Assert.Equal(39, dm.RowCount);
            Assert.Equal("David Johnson", dm[0][1]);

            var tb = dm.ToDataTable("Name", "Score");
            Assert.Equal(2, tb.Columns.Count);
            Assert.Equal("Name", tb.Columns[0].ColumnName);
            Assert.Equal("Score", tb.Columns[1].ColumnName);
        }

        [Fact]
        public void Can_compute_percentile_rank_function()
        {
            var table = _students.ToDataTable();
            var dm = DataMatrixFactory.Create(table, "FirstName,LastName,Score", new DataMatrixColumn
            {
                Name = "PercentileRank",
                DependsOn = new[] {"Score"},
                Initialize = PercentileRank.Initialize,
                Compute = PercentileRank.Compute
            });
            Assert.Equal(39, dm.RowCount);
            Assert.Equal(6.41, dm[0][3]);


            var tb = dm.ToDataTable();
            Assert.Equal(4, tb.Columns.Count);
        }

        [Fact]
        public void Can_compute_outlier_function()
        {
            var items = new[] {71, 70, 73, 70, 70, 69, 70, 72, 71, 300, 71, 69, 2};
            var list = items.Select((c, i) => Tuple.Create("City" + i, c));

            var dm = DataMatrixFactory.Create(list, new[] {"City", "Temperature"},
                new Func<Tuple<string, int>, object>[] {a => a.Item1, a => a.Item2}, new DataMatrixColumn
                {
                    Name = "Outlier",
                    DependsOn = new[] {"Temperature"},
                    Initialize = Outlier.Initialize,
                    Compute = Outlier.Compute
                });

            Assert.Equal(71, dm[0][1]);
            Assert.Equal(Outlier.OutlierType.None, dm[0][2]);

            Assert.Equal(300, dm[9][1]);
            Assert.Equal(Outlier.OutlierType.Major, dm[9][2]);

            Assert.Equal(2, dm[12][1]);
            Assert.Equal(Outlier.OutlierType.Major, dm[12][2]);

            var tb = dm.ToDataTable();
            Assert.Equal(3, tb.Columns.Count);
        }

        [Fact]
        public void Can_compute_standard_deviation_function()
        {
            var heights = new[] { 600, 470, 170, 430, 300 };
            var names = new[] { "Rottweilers", "Labrador", "Dachshunds", "Terrier", "Bulldog" };
            var list = heights.Select((c, i) => Tuple.Create(names[i], c));

            var dm = DataMatrixFactory.Create(list, new[] { "Dog", "Height" },
                new Func<Tuple<string, int>, object>[] { a => a.Item1, a => a.Item2 }, new DataMatrixColumn
                {
                    Name = "Deviation",
                    DependsOn = new[] { "Height" },
                    Initialize = StandardDeviation.Initialize,
                    Compute = StandardDeviation.Compute
                });

            VerifyDeviation(dm, 0, "Rottweilers", 600, 2);
            VerifyDeviation(dm, 1, "Labrador", 470, 1);
            VerifyDeviation(dm, 2, "Dachshunds", 170, -2);
            VerifyDeviation(dm, 3, "Terrier", 430, 1);
            VerifyDeviation(dm, 4, "Bulldog", 300, -1);

            var tb = dm.ToDataTable();
            Assert.Equal(3, tb.Columns.Count);
        }

        private static void VerifyDeviation(IDataMatrix dm, int index, string name, int height, int deviation)
        {
            Assert.Equal(name, dm[index][0]);
            Assert.Equal(height, dm[index][1]);
            Assert.Equal(deviation, dm[index][2]);
        }
    }
}