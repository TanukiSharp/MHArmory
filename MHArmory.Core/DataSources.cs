using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core
{
    public interface IDataSource
    {
        string Description { get; }
        Task<IArmorPiece[]> GetArmorPieces();
        Task<ISkill[]> GetSkills();
        Task<IAbility[]> GetAbilities();
        Task<ICharm[]> GetCharms();
        Task<IJewel[]> GetJewels();
    }
}
