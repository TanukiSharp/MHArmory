using System;
using System.Collections.ObjectModel;
using System.Text;
using Cloo;

namespace MHArmory.Search.OpenCL
{
    class Host : IDisposable
    {
        private class DefineBuilder
        {
            private StringBuilder InnerBuilder { get; }

            public DefineBuilder()
            {
                InnerBuilder = new StringBuilder();
            }

            public void AddDefine(string name)
            {
                InnerBuilder.Append($"-D {name} ");
            }

            public void AddDefine(string name, object value)
            {
                InnerBuilder.Append($"-D {name}={value} ");
            }

            public override string ToString()
            {
                return InnerBuilder.ToString(0, InnerBuilder.Length - 1);
            }
        }

        private ComputeContext Context { get; }
        private ComputeProgram Program { get; }
        private const string KernelName = "search";

        private byte[] TrimUTFBOM(byte[] data)
        {
            byte[] bom = Encoding.UTF8.GetPreamble();
            if (data.Length < bom.Length)
            {
                return data;
            }

            for (int i = 0; i < bom.Length; i++)
            {
                if (bom[i] != data[i])
                {
                    return data;
                }
            }

            byte[] copy = new byte[data.Length - bom.Length];
            Buffer.BlockCopy(data, bom.Length, copy, 0, copy.Length);
            return copy;
        }

        public Host()
        {
            byte[] source = TrimUTFBOM(Properties.Resources.search);
            string sourceStr = Encoding.ASCII.GetString(source);

            ComputePlatform platform = ComputePlatform.Platforms[2];
            var properties = new ComputeContextPropertyList(platform);
            Context = new ComputeContext(ComputeDeviceTypes.All, properties, null, IntPtr.Zero);
            Program = new ComputeProgram(Context, sourceStr);
            var defineBuilder = new DefineBuilder();
            defineBuilder.AddDefine("MAX_RESULTS", SearchLimits.ResultCount);
            string defines = defineBuilder.ToString();
            Program.Build(null, defines, null, IntPtr.Zero);
        }
        
        public SerializedSearchResults Run(SerializedSearchParameters searchParameters)
        {
            ushort[] resultCount = new ushort[1];
            byte[] resultData = new byte[SearchLimits.ResultCount * (sizeof(ushort) * 6 + 1 + 3 * 7 * 3)];

            var headerBuffer = new ComputeBuffer<byte>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.Header);
            var equipmentBuffer = new ComputeBuffer<byte>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.Equipment);
            var decoBuffer = new ComputeBuffer<byte>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.Decorations);
            var desiredSkillBuffer = new ComputeBuffer<byte>(Context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, searchParameters.DesiredSkills);
            var resultCountBuffer = new ComputeBuffer<ushort>(Context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, resultCount);
            var resultBuffer = new ComputeBuffer<byte>(Context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, resultData);

            using (ComputeCommandQueue queue = new ComputeCommandQueue(Context, Context.Devices[0], ComputeCommandQueueFlags.None))
            {
                using (ComputeKernel kernel = Program.CreateKernel(KernelName))
                {
                    kernel.SetMemoryArgument(0, headerBuffer);
                    kernel.SetMemoryArgument(1, equipmentBuffer);
                    kernel.SetMemoryArgument(2, decoBuffer);
                    kernel.SetMemoryArgument(3, desiredSkillBuffer);
                    kernel.SetMemoryArgument(4, resultCountBuffer);
                    kernel.SetMemoryArgument(5, resultBuffer);
                    queue.Execute(kernel, null, new long[] { searchParameters.Combinations }, null, null);
                }
                //queue.Finish();
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
            Program.Dispose();
            Context.Dispose();
        }
    }
}
