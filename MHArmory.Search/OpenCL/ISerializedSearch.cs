using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    internal interface ISerializedSearch
    {
        SerializedSearchResults Run(SerializedSearchParameters searchParameters);
    }
}
