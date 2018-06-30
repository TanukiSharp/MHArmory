using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;

namespace MHArmory.Core
{
    public interface IArmorDataSource
    {
        string Description { get; }
        Task<IArmorPiece[]> GetArmorPieces();
    }

    public interface ISkillDataSource
    {
        string Description { get; }
        Task<ISkill[]> GetSkills();
        Task<IAbility[]> GetAbilities();
    }

    public interface ICharmDataSource
    {
        string Description { get; }
        Task<ICharm[]> GetCharms();
    }
}
