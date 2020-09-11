using System;
using DataMatrix.UnitTests.Helpers;
using WhichMan.Analytics;
using WhichMan.Analytics.Pivot;
using WhichMan.Analytics.Utils;
using Xunit;

namespace DataMatrix.UnitTests
{
    public class DataMatrixPivotTests : TestBase
    {
        [Fact]
        public void Can_pivot_matrix_by_row_column_count_orderDate()
        {
            var list = LoadOrders();

            var builder = DataMatrixBuilder.Create(list, new[] { "OrderDate" }, a => a.OrderDate);
            builder.AddColumn("Year", "OrderDate", (values, args) => $"{values[0]:yyyy}");
            builder.AddColumn("Month", "OrderDate", (values, args) => $"{values[0]:MM-MMM}");

            var dm = builder.Build();
            dm = dm.Pivot("Year", "OrderDate", AggregateFunction.Count, "Month");

            //verify columns
            Assert.Equal("Year", dm.Columns[0].Name);
            Assert.Equal("01-Jan", dm.Columns[1].Name);
            Assert.Equal("12-Dec", dm.Columns[12].Name);

            //verify data
            Assert.Equal("1996", dm[0][0]);
            Assert.Equal(0, dm[0][1]);
            Assert.Equal(22, dm[0][7]);
            Assert.Equal("1998", dm[2][0]);

            var tb = dm.ToDataTable();
            Assert.Equal(13, tb.Columns.Count);
        }

        [Fact]
        public void Can_pivot_matrix_by_row_column_count_orderDate_datatable()
        {
            var list = LoadOrders().ToDataTable();

            var builder = DataMatrixBuilder.Create(list, "OrderDate");
            builder.AddColumn("Year", "OrderDate", (values, args) => $"{values[0]:yyyy}");
            builder.AddColumn("Month", "OrderDate", (values, args) => $"{values[0]:MM-MMM}");

            var dm = builder.Build();
            dm = dm.Pivot("Year", "OrderDate", AggregateFunction.Count, "Month");

            //verify columns
            Assert.Equal("Year", dm.Columns[0].Name);
            Assert.Equal("01-Jan", dm.Columns[1].Name);
            Assert.Equal("12-Dec", dm.Columns[12].Name);

            //verify data
            Assert.Equal("1996", dm[0][0]);
            Assert.Equal(0, dm[0][1]);
            Assert.Equal(22, dm[0][7]);
            Assert.Equal("1998", dm[2][0]);

            var tb = dm.ToDataTable();
            Assert.Equal(13, tb.Columns.Count);
        }

        [Fact]
        public void Can_pivot_matrix_by_row_column_sum_freight()
        {
            var list = LoadOrders();

            var builder = DataMatrixBuilder.Create(list, new[] { "OrderDate", "Freight" }, a => a.OrderDate, a => a.Freight);
            builder.AddColumn("Year", "OrderDate", (values, args) => $"{values[0]:yyyy}");
            builder.AddColumn("Month", "OrderDate", (values, args) => $"{values[0]:MM-MMM}");

            var dm = builder.Build();
            dm = dm.Pivot("Year", "Freight", AggregateFunction.Sum, "Month");

            //verify columns
            Assert.Equal("Year", dm.Columns[0].Name);
            Assert.Equal("02-Feb", dm.Columns[2].Name);
            Assert.Equal("11-Nov", dm.Columns[11].Name);

            //verify data
            Assert.Equal("1996", dm[0][0]);
            Assert.Equal(0m, dm[0][1]);
            Assert.Equal(1288.18m, dm[0][7]);
            Assert.Equal(5463.44m, dm[2][1]);

            var tb = dm.ToDataTable();
            Assert.Equal(13, tb.Columns.Count);
        }

        [Fact]
        public void Can_pivot_matrix_by_row_column_average_freight()
        {
            var list = LoadOrders();

            var builder = DataMatrixBuilder.Create(list, new[] { "OrderDate", "Freight" }, a => a.OrderDate, a => a.Freight);
            builder.AddColumn("Year", "OrderDate", (values, args) => $"{values[0]:yyyy}");
            builder.AddColumn("Month", "OrderDate", (values, args) => $"{values[0]:MM-MMM}");

            var dm = builder.Build();
            dm = dm.Pivot("Year", "Freight", AggregateFunction.Average, "Month");

            //verify columns
            Assert.Equal("Year", dm.Columns[0].Name);
            Assert.Equal("03-Mar", dm.Columns[3].Name);
            Assert.Equal("10-Oct", dm.Columns[10].Name);

            //verify data
            Assert.Equal("1996", dm[0][0]);
            Assert.Equal(0m, dm[0][1]);
            Assert.Equal(67.85m, Math.Round((decimal)dm[1][1], 2));
            Assert.Equal(2238.98m / 33m, dm[1][1]);
            Assert.Equal(5463.44m / 55m, dm[2][1]);

            var tb = dm.ToDataTable();
            Assert.Equal(13, tb.Columns.Count);
        }

        [Fact]
        public void Can_pivot_matrix_by_row_column_min_freight()
        {
            var list = LoadOrders();

            var builder = DataMatrixBuilder.Create(list, new[] { "OrderDate", "Freight" }, a => a.OrderDate, a => a.Freight);
            builder.AddColumn("Year", "OrderDate", (values, args) => $"{values[0]:yyyy}");
            builder.AddColumn("Month", "OrderDate", (values, args) => $"{values[0]:MM-MMM}");

            var dm = builder.Build();
            dm = dm.Pivot("Year", "Freight", AggregateFunction.Minimum, "Month");

            //verify columns
            Assert.Equal("Year", dm.Columns[0].Name);
            Assert.Equal("03-Mar", dm.Columns[3].Name);
            Assert.Equal("10-Oct", dm.Columns[10].Name);

            //verify data
            Assert.Equal("1996", dm[0][0]);
            Assert.Equal(0m, dm[0][1]);

            Assert.Equal("1997", dm[1][0]);
            Assert.Equal(0.20m, Math.Round((decimal)dm[1][1], 2));

            Assert.Equal("1998", dm[2][0]);
            Assert.Equal(0.56m, dm[2][1]);

            var tb = dm.ToDataTable();
            Assert.Equal(13, tb.Columns.Count);
        }

        [Fact]
        public void Can_pivot_matrix_by_row_column_max_freight()
        {
            var list = LoadOrders();

            var builder = DataMatrixBuilder.Create(list, new[] { "OrderDate", "Freight" }, a => a.OrderDate, a => a.Freight);
            builder.AddColumn("Year", "OrderDate", (values, args) => $"{values[0]:yyyy}");
            builder.AddColumn("Month", "OrderDate", (values, args) => $"{values[0]:MM-MMM}");

            var dm = builder.Build();
            dm = dm.Pivot("Year", "Freight", AggregateFunction.Maximum, "Month");

            //verify columns
            Assert.Equal("Year", dm.Columns[0].Name);
            Assert.Equal("03-Mar", dm.Columns[3].Name);
            Assert.Equal("10-Oct", dm.Columns[10].Name);

            //verify data
            Assert.Equal("1996", dm[0][0]);
            Assert.Equal(0m, dm[0][1]);

            Assert.Equal("1997", dm[1][0]);
            Assert.Equal(458.78m, Math.Round((decimal)dm[1][1], 2));

            Assert.Equal("1998", dm[2][0]);
            Assert.Equal(719.78m, dm[2][1]);

            var tb = dm.ToDataTable();
            Assert.Equal(13, tb.Columns.Count);
        }
    }
}