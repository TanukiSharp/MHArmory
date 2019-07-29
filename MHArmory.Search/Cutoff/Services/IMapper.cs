using System.Collections.Generic;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Cutoff.Models;
using MHArmory.Search.Cutoff.Models.Mapped;

namespace MHArmory.Search.Cutoff.Services
{
    internal interface IMapper
    {
        MappingResults MapEverything(IList<IList<IEquipment>> allEquipment, IAbility[] desiredAbilities, IEnumerable<SolverDataJewelModel> jewels, bool createNullEquipments);
        MappedEquipment[] MapSupersets(IList<SupersetInfo> supersets, MappingResults existingResults);
    }
}
