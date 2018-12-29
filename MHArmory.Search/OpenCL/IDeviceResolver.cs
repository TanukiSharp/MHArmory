using System;
using System.Collections.Generic;
using System.Text;
using Cloo;

namespace MHArmory.Search.OpenCL
{
    public interface IDeviceResolver
    {
        ComputeDevice ResolveDevice();
    }
}
