using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    class CLSerializationException : Exception
    {
        public CLSerializationException() : base()
        {
        }

        public CLSerializationException(string message) : base(message)
        {
        }

        public CLSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
