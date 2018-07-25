using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.AthenaAssDataSource.DataStructures
{
    internal class ArmorSetSkillPrimitive
    {
        [Hidden]
        internal int Id = 0;
        internal string Name = null;
        [Name("Points Needed")]
        internal int PiecesNeeded = 0;
        [Name("Skill Granted")]
        internal string SkillGranted = null;
    }
}
