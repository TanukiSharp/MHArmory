using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    public class OpenCLSerializationException : Exception
    {
        public OpenCLSerializationException() : base()
        {
        }

        public OpenCLSerializationException(string message) : base(message)
        {
        }

        public OpenCLSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
