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

        IAbility[] IHasAbilities.Abilities { get { return Jewel.Abilities; } }

        public SolverDataJewelModel(IJewel jewel, int available)
        {
            Jewel = jewel;
            Available = available;
        }
    }
}
