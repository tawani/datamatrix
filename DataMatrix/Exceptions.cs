using System;
using System.Collections.Generic;
using System.Text;

namespace WhichMan.Analytics
{

    public class DataMatrixException : Exception
    {
        public DataMatrixException() { }
        public DataMatrixException(string message) : base(message) { }
        public DataMatrixException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class ColumnNotFoundException : DataMatrixException
    {
        public ColumnNotFoundException(string name) : base($"The column '{name}' is not in the data source.") { }
    }

    /// <summary>
    /// Represents a duplicate column exception when the same column is added more than once
    /// </summary>
    public class DuplicateColumnException : DataMatrixException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateColumnException"/> class.
        /// </summary>
        public DuplicateColumnException(string name)
            : base($"This column '{name}' has already been specified.")
        {
        }
    }
}
