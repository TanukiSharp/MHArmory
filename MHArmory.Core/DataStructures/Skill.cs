using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public interface IAbility
    {
        int Id { get; }
        ISkill Skill { get; }
        int Level { get; }
        string Description { get; }
    }

    public interface ISkill
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        IAbility[] Abilities { get; }
    }
}
