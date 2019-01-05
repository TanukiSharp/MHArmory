using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Configurations.Conversion
{
    public interface IConverter
    {
        int SourceVersion { get; }
        int TargetVersion { get; }
        object Convert(object input);
    }
}
