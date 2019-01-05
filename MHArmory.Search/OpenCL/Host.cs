using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Cloo;

namespace MHArmory.Search.OpenCL
{
    internal class Host : ISerializedSearch, IDisposable
    {
        private readonly ComputeContext context;
        private readonly ComputeProgram program;
        private readonly ComputeDevice device;

        private const string KernelName = "search";

        public Host(IDeviceResolver deviceResolver)
        {
            ArraySegment<byte> source = TrimUTFBOM(Properties.Resources.search);
            string sourceStr = Encoding.ASCII.GetString(source.Array, source.Offset, source.Count);

            device = deviceResolver.ResolveDevice();
            var properties = new ComputeContextPropertyList(device.Platform);
            context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
            program = new ComputeProgram(context, sourceStr);

            var optionsBuilder = new HostOptionsBuilder();
            optionsBuilder.AddDefine("MAX_RESULTS", SearchLimits.ResultCount);
            optionsBuilder.AddDefine("MAX_DESIRED_SKILLS", SearchLimits.MaxDesiredSkills);
            optionsBuilder.AddDefine("MAX_SET_SKILLS", SearchLimits.MaxSetSkills);
            optionsBuilder.AddDefine("MAX_JEWEL_SIZE", SearchLimits.MaxJewelSize);
            optionsBuilder.AddDefine("SLOTS_PER_EQUIPMENT", SearchLimits.SlotsPerEquipment);
            optionsBuilder.AddDefine("SKILLS_PER_EQUIPMENT", SearchLimits.SkillsPerEquipment);
            optionsBuilder.AddDefine("SET_SKILLS_PER_EQUIPMENT", SearchLimits.SetSkillsPerEquipment);
            optionsBuilder.AddDefine("EQUIPMENT_TYPES", SearchLimits.EquipmentTypes);
            string options = optionsBuilder.ToString();

            program.Build(null, options, null, IntPtr.Zero);
        }

        // For some reason when you embed a string to an assembly's resources, it adds a UTF BOM,
        // and the OpenCL compiler doesn't like that. So this is to remove it manually.
        private ArraySegment<byte> TrimUTFBOM(byte[] data)
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            if (data.Length < bom.Length)
            {
                return new ArraySegment<byte>(data);
            }

            for (int i = 0; i < bom.Length; i++)
            {
                if (bom[i] != data[i])
                {
                    return new ArraySegment<byte>(data);
                }
            }
            var segment = new ArraySegment<byte>(data, bom.Length, data.Length - bom.Length);
            return segment;
        }

        public SerializedSearchResults Run(SerializedSearchParameters searchParameters)
        {
            ushort[] resultCount = new ushort[1];

            byte[] resultData = new byte[LengthConstants.ResultLength];

            // Not that much data, better copy to device (CopyHostPointer) and back rather than query the host (UseHostPointer)
            var headerBuffer = new ComputeBuffer<byte>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.Header);
            var equipmentBuffer = new ComputeBuffer<byte>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.Equipment);
            var decoBuffer = new ComputeBuffer<byte>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.Decorations);
            var desiredSkillBuffer = new ComputeBuffer<byte>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.DesiredSkills);
            var resultCountBuffer = new ComputeBuffer<ushort>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, resultCount);
            var resultBuffer = new ComputeBuffer<byte>(context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, resultData);

            using (ComputeCommandQueue queue = new ComputeCommandQueue(context, device, ComputeCommandQueueFlags.None))
            {
                using (ComputeKernel kernel = program.CreateKernel(KernelName))
                {
                    kernel.SetMemoryArgument(0, headerBuffer);
                    kernel.SetMemoryArgument(1, equipmentBuffer);
                    kernel.SetMemoryArgument(2, decoBuffer);
                    kernel.SetMemoryArgument(3, desiredSkillBuffer);
                    kernel.SetMemoryArgument(4, resultCountBuffer);
                    kernel.SetMemoryArgument(5, resultBuffer);

                    long[] globalWorkSize = new long[] { searchParameters.Combinations };
                    queue.Execute(kernel, null, globalWorkSize, null, null);
                }
                queue.ReadFromBuffer(resultCountBuffer, ref resultCount, true, null);
                queue.ReadFromBuffer(resultBuffer, ref resultData, true, null);
                queue.Finish();
            }

            var result = new SerializedSearchResults();
            result.ResultCount = resultCount[0];
            result.Data = resultData;

            return result;
        }

        public void Dispose()
        {
            program.Dispose();
            context.Dispose();
        }
    }
}
