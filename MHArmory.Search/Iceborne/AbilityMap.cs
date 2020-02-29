using MHArmory.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.Iceborne
{
    class AbilityMap
    {
        private Dictionary<ISkill, int> abilities = new Dictionary<ISkill, int>();

        public int this[ISkill skill]
        {
            get
            {
                if (abilities.TryGetValue(skill, out int value))
                    return value;
                else
                    return 0;
            }
        }

        public void AddAbility(IAbility a)
        {
            AddSkill(a.Skill, a.Level);
        }

        public void AddSkill(ISkill skill, int level)
        {
            if (abilities.ContainsKey(skill))
                abilities[skill] += level;
            else
                abilities[skill] = level;
        }

        public void AddDecos(IJewel deco, int count = 1)
        {
            foreach (IAbility ability in deco.Abilities)
                AddSkill(ability.Skill, ability.Level * count);
        }

        public void AddAbility(IAbility[] abilities)
        {
            foreach (IAbility a in abilities)
                AddAbility(a);
        }

        public void Clear()
        {
            abilities.Clear();
        }
    }
}
