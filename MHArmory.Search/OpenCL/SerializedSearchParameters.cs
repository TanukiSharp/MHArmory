using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.OpenCL
{
    internal class SerializedSearchParameters
    {
        public uint Combinations { get; set; }
        public SearchIDMaps SearchIDMaps { get; set; }
        public byte[] Header { get; set; }
        public byte[] Equipment { get; set; }
        public byte[] Decorations { get; set; }
        public byte[] DesiredSkills { get; set; }
    }
}
