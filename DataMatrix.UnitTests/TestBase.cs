using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataMatrix.UnitTests.Helpers;

namespace DataMatrix.UnitTests
{
    public abstract class TestBase
    {
        protected static IEnumerable<T> ReadFile<T>(string fileName, Func<string[], T> map, bool firstLineIsHeaders = true)
        {
            var skipped = false;
            using (var stream = new FileStream($"data\\{fileName}", FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if(line == null || string.IsNullOrWhiteSpace(line))
                        continue;

                    if (firstLineIsHeaders && !skipped)
                    {
                        skipped = true;
                        continue;
                    }
                    var values = line.Split(',');
                    yield return map(values);
                }
            }
        }

        public static List<Student> LoadStudents(string suffix = "")
        {
            var result = ReadFile($"students{suffix}.csv", (c) => new Student { FirstName = c[0].ToTitleCase(), LastName = c[1], Score = c[2].ToDecimal() ?? 0, Gender = c[3], });
            return result.ToList();
        }


        public static List<Order> LoadOrders()
        {
            var result = ReadFile("orders.csv", (c) => new Order
            {
                OrderId = c[0],
                CustomerId = c[1],
                EmployeeId = c[2],
                OrderDate = DateTime.Parse(c[3]),
                RequiredDate = c[4].ToDateTime(),
                ShippedDate = c[5].ToDateTime(),
                ShipVia = c[6],
                Freight = c[7].ToDecimal() ?? 0,
                ShipName = c[8],
                ShipAddress = c[9],
                ShipCity = c[10],
                ShipRegion = c[11],
                ShipPostalCode = c[12],
                ShipCountry = c[13],
            });
            return result.ToList();
        }
    }
}
