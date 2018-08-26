using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Configurations
{
    public interface IConfiguration
    {
        uint Version { get; set; }
        string[] BackupLocations { get; set; }
    }
}
