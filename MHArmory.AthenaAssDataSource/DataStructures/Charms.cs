using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.AthenaAssDataSource.DataStructures
{
    internal class CharmPrimitive
    {
        internal string Name = null;
        internal string Skill1 = null;
        internal int Points1 = 0;
        internal string Skill2 = null;
        internal int Points2 = 0;
        internal int Acquire = 0;
        internal string Material1 = null;
        [Name("Points1", Index = 1)]
        internal int Material1Points = 0;
        internal string Material2 = null;
        [Name("Points2", Index = 1)]
        internal int Material2Points = 0;
        internal string Material3 = null;
        [Name("Points3")]
        internal int Material3Points = 0;
        internal string Material4 = null;
        [Name("Points4")]
        internal int Material4Points = 0;
    }
}
