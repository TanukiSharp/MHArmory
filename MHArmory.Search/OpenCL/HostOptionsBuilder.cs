using System.Text;

namespace MHArmory.Search.OpenCL
{
    class HostOptionsBuilder
    {
        private StringBuilder InnerBuilder { get; }

        public HostOptionsBuilder()
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
}