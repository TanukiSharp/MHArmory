using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.Contracts
{
    public interface IExtension
    {
        string Name { get; }
        string Author { get; }
        string Description { get; }
        int Version { get; }
    }
}
