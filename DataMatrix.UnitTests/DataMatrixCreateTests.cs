using System;
using System.Linq.Expressions;
using DataMatrix.UnitTests.Helpers;
using WhichMan.DataAnalytics;
using Xunit;

namespace DataMatrix.UnitTests
{
    public class DataMatrixCreateTests: TestBase
    {
        [Theory]
        [InlineData(null, 4)]
        [InlineData("*", 4)]
        [InlineData("FirstName",1)]
        [InlineData("FirstName,LastName,SCore",3)]
        public void Can_create_dataMatrix_from_dataTable(string columns, int count)
        {
            var table = LoadStudents().ToDataTable();
            var dm = columns == null ? DataMatrixFactory.Create(table) : DataMatrixFactory.Create(table, columns);
            Assert.Equal(39, dm.RowCount);
            Assert.Equal(count, dm.Columns.Count);

            if (count == 3)
                VerifyStudents(dm);
        }

        private static void VerifyStudents(IDataMatrix dm)
        {
            Assert.Equal("Johnson", dm[0][1]);
            Assert.Equal(56m, dm[0][2]);
            Assert.Equal("Williams", dm[1][1]);
        }

        [Fact]
        public void Can_create_dataMatrix_from_list()
        {
            var list = LoadStudents();
            var dm = DataMatrixFactory.Create(list,
                new Expression<Func<Student, object>>[] {a => a.FirstName, a => a.LastName, a => a.Score});
            Assert.Equal(39, dm.RowCount);
            Assert.Equal(3, dm.Columns.Count);
            VerifyStudents(dm);
        }

        [Fact]
        public void Can_create_dataMatrix_from_list_add_headers()
        {
            var list = LoadStudents();
            var dm = DataMatrixFactory.Create(list, new[] { "FirstName", "LastName", "MyScore" },
                new Func<Student, object>[] { a => a.FirstName, a => a.LastName, a => a.Score });
            Assert.Equal(39, dm.RowCount);
            Assert.Equal(3, dm.Columns.Count);
            VerifyStudents(dm);
        }
    }
}
