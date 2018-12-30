using System;
using System.Collections.Generic;
using System.Linq;
using Cloo;

namespace MHArmory.Search.OpenCL
{
    internal class AutomaticDeviceResolver : IDeviceResolver
    {
        private readonly PreferredDeviceComparer comparer;

        public AutomaticDeviceResolver()
        {
            comparer = new PreferredDeviceComparer();
        }

        // Orders devices in this order: CPU, GPU, Accelerator (I think that means FPGA?)
        private class PreferredDeviceComparer : IComparer<ComputeDeviceTypes>
        {
            public int Compare(ComputeDeviceTypes type1, ComputeDeviceTypes type2)
            {
                if (type1 == type2)
                {
                    return 0;
                }

                switch (type1)
                {
                    case ComputeDeviceTypes.Cpu:
                        return -1;
                    case ComputeDeviceTypes.Gpu:
                        return type2 == ComputeDeviceTypes.Cpu ? 1 : -1;
                    case ComputeDeviceTypes.Accelerator:
                        return 1;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type1), type1, null);
                }
            }
        }

        public ComputeDevice ResolveDevice()
        {
            var allDevices = ComputePlatform.Platforms.SelectMany(p => p.Devices).ToList();
            IOrderedEnumerable<ComputeDevice> candidates = allDevices
                .OrderByDescending(d => d.Type, comparer)
                .ThenBy(d => d.MaxComputeUnits * d.MaxClockFrequency); // Core count * clock frequency ~= performance
            // This is a bit iffy, since "MaxComputeUnits" reports 28 max compute units, while a 1080Ti actually has 3584 cores.
            // But it's the best we have for now
            return candidates.First();
        }
    }
}
