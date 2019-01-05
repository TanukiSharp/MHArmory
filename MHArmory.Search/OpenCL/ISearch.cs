using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.OpenCL
{
    internal interface ISearch
    {
        List<ArmorSetSearchResult> Run(ISolverData data);
    }
}
