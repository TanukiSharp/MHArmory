using System.Text;

namespace MHArmory.Search.OpenCL
{
    internal class HostOptionsBuilder
    {
        private readonly StringBuilder innerBuilder;

        public HostOptionsBuilder()
        {
            innerBuilder = new StringBuilder();
        }

        public void AddDefine(string name)
        {
            innerBuilder.Append($"-D {name} ");
        }

        public void AddDefine(string name, object value)
        {
            innerBuilder.Append($"-D {name}={value} ");
        }

        public override string ToString()
        {
            return innerBuilder.ToString(0, innerBuilder.Length - 1);
        }
    }
}
