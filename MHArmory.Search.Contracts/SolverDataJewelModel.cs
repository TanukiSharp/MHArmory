using MHArmory.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Search.Contracts
{
    public class SolverDataJewelModel : IHasAbilities
    {
        public IJewel Jewel { get; }
        public int Available { get; set; }
        // Indicates if this is an automaticly generated jewel with multiskills where only on skill is wanted
        public bool Generic { get; } 

        IAbility[] IHasAbilities.Abilities { get { return Jewel.Abilities; } }

        public SolverDataJewelModel(IJewel jewel, int available, bool generic = false)
        {
            Jewel = jewel;
            Available = available;
            Generic = generic;
        }
    }
}
