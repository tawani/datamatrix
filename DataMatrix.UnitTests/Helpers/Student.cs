using System.Collections.Generic;
using System.Text;

namespace DataMatrix.UnitTests.Helpers
{
    public class Student
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public decimal Score { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
