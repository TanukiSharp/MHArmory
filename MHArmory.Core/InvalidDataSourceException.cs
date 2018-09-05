using System;

namespace MHArmory.Core
{
    public class InvalidDataSourceException : Exception
    {
        public InvalidDataSourceException()
        {
        }

        public InvalidDataSourceException(string message) : base(message)
        {
        }

        public InvalidDataSourceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
