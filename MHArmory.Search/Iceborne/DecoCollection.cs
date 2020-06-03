using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHArmory.Search.Iceborne
{
    class DecoCollection
    {
        public SolverDataJewelModel[] SingleSkillDecos { get; } // Decos with only 1 skill. Index +1 is the number points in this skill. A maximum of 4 points is possible
        public SolverDataJewelModel GenericLevel4Deco { get; } // All Decos with additional skills which are not in the desired abilities list
        public List<SolverDataJewelModel> MultiSkillDecos { get; } // All level 4 decos with another desired ability
        public int TotalSkillLevels { get; }
        public bool HasLevel4Deco { get; }
        public ISkill Skill { get; }

        public DecoCollection(IEnumerable<SolverDataJewelModel> allJewelsWithSkill, IAbility ability)
        {
            Skill = ability.Skill;
            TotalSkillLevels = 0;
            HasLevel4Deco = false;
            SingleSkillDecos = new SolverDataJewelModel[4];
            MultiSkillDecos = new List<SolverDataJewelModel>();
            foreach (SolverDataJewelModel jewel in allJewelsWithSkill)
            {
                // we have to limit this calculation to not get overflows
                TotalSkillLevels += Math.Min(jewel.Available, Skill.MaxLevel) * jewel.Jewel.Abilities.First(j => j.Skill == Skill).Level;
                if (jewel.Jewel.SlotSize > 3 &&
                    (!jewel.Generic || jewel.Available > 0)) // we add a generic deco even if there are none
                    HasLevel4Deco = true;

                if (jewel.Generic)
                    GenericLevel4Deco = jewel;
                else if (jewel.Jewel.Abilities.Length > 1)
                    MultiSkillDecos.Add(jewel);
                else
                    SingleSkillDecos[jewel.Jewel.Abilities[0].Level - 1] = jewel;
            }
        }

        public void SortMultiSkillDecos(Dictionary<ISkill, int> skillCombinations)
        {
            MultiSkillDecos.OrderBy(d => skillCombinations[d.Jewel.Abilities.First(a => a.Skill != Skill).Skill]);
        }
    }
}
