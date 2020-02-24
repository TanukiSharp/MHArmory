using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;
using MHArmory.Search.Default;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHArmory.Search.Iceborne
{
    class IceborneSolver : SolverBase
    {
        public override string Name { get; } = "Iceborne - Default";
        public new string Author { get; } = "ChaosSaber";
        public override string Description { get; } = "Default Search for Iceborne Data";
        public override int Version { get; } = 1;

        private Dictionary<ISkill, DecoCollection> jewels = new Dictionary<ISkill, DecoCollection>();
        private List<ISkill> noJewelSkills;
        private List<ISkill> noLevel4JewelSkills;
        private List<ISkill> level4JewelSkills;

        protected override ArmorSetSearchResult IsArmorSetMatching(IEquipment weapon, IEquipment[] equips)
        {
            throw new NotImplementedException();
        }

        protected override void OnSearchBegin(SolverDataJewelModel[] matchingJewels, IAbility[] desiredAbilities)
        {
            foreach (IAbility ability in desiredAbilities)
            {
                jewels[ability.Skill] = new DecoCollection(matchingJewels.Where(j => j.Jewel.Abilities.Any(a => a.Skill == ability.Skill)), ability);
            }

            noJewelSkills = jewels.Values
                .Where(x => x.TotalSkillLevels == 0)
                .Select(x => x.Skill)
                .ToList();

            noLevel4JewelSkills = jewels.Values
                .Where(x => x.TotalSkillLevels > 0 && x.HasLevel4Deco == false)
                .OrderByDescending(x => x.SingleSkillDecos[0].Jewel.SlotSize)
                .Select(x => x.Skill)
                .ToList();

            level4JewelSkills = jewels.Values
                .Where(x => x.HasLevel4Deco)             
                .Select(x => x.Skill)
                .ToList();
        }
    }
}
