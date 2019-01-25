using System.Collections.Generic;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Cutoff.Models;

namespace MHArmory.Search.Cutoff.Services
{
    internal interface ISupersetMaker
    {
        SupersetInfo CreateSupersetModel(IList<IEquipment> equipments, IAbility[] desiredAbilities);
    }
}