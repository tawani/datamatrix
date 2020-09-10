using System;
using System.Collections.Generic;
using System.Linq;
using DataMatrix.UnitTests.Helpers;
using WhichMan.DataAnalytics;
using WhichMan.DataAnalytics.Functions;
using Xunit;

namespace DataMatrix.UnitTests
{
    public class DataMatrixFunctionsTests : TestBase
    {
        private List<Student> _students;

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
        }

        [Fact]
        public void Can_compute_percentile_rank_function()
        {
            var table = _students.ToDataTable();
            var dm = DataMatrixFactory.Create(table, "FirstName,LastName,Score", new DataMatrixColumn
            {
                Name = "PercentileRank",
                DependsOn = new[] { "Score" },
                Initialize = PercentileRank.Initialize,
                Compute = PercentileRank.Compute
            });
            Assert.Equal(39, dm.RowCount); 
            Assert.Equal(6.41, dm[0][3]);


            var tb = dm.ToDataTable();
            Assert.Equal(4, tb.Columns.Count);
        }

        [Fact]
        public void Can_compute_outlier_rank_function()
        {
            var items = new[] { 71, 70, 73, 70, 70, 69, 70, 72, 71, 300, 71, 69 };
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


            var tb = dm.ToDataTable();
            Assert.Equal(3, tb.Columns.Count);
        }
    }
}