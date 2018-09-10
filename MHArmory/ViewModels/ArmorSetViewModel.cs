using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHArmory.Core.DataStructures;
using MHArmory.Search;

namespace MHArmory.ViewModels
{
    public class SearchResultSkillViewModel : ViewModelBase
    {
        public ISkill Skill { get; }
        public int TotalLevel { get; }
        public bool IsExtra { get; }
        public string Description { get; }

        public SearchResultSkillViewModel(ISkill skill, int totalLevel, bool isExtra)
        {
            Skill = skill;
            TotalLevel = totalLevel;
            IsExtra = isExtra;
            Description = Skill.Abilities.FirstOrDefault(x => x.Level == TotalLevel)?.Description;
        }
    }

    public class ArmorSetJewelViewModel : ViewModelBase
    {
        private IJewel jewel;
        public IJewel Jewel
        {
            get { return jewel; }
            private set { SetValue(ref jewel, value); }
        }

        private int count;
        public int Count
        {
            get { return count; }
            private set { SetValue(ref count, value); }
        }

        public ArmorSetJewelViewModel(IJewel jewel, int count)
        {
            this.jewel = jewel;
            this.count = count;
        }
    }

    public class ArmorSetViewModel : ViewModelBase
    {
        private IList<IArmorPiece> armorPieces;
        public IList<IArmorPiece> ArmorPieces
        {
            get { return armorPieces; }
            private set { SetValue(ref armorPieces, value); }
        }

        private ICharmLevel charm;
        public ICharmLevel Charm
        {
            get { return charm; }
            private set { SetValue(ref charm, value); }
        }

        private IList<ArmorSetJewelViewModel> jewels;
        public IList<ArmorSetJewelViewModel> Jewels
        {
            get { return jewels; }
            private set { SetValue(ref jewels, value); }
        }

        public SearchResultSkillViewModel[] AdditionalSkills { get; private set; }

        public int AdditionalSkillsTotalLevel { get; }

        public int[] SpareSlots { get; }

        public int SpareSlotCount { get; }
        public int SpareSlotSizeSquare { get; }
        public int SpareSlotSizeCube { get; }

        public int TotalRarity { get; }

        public int TotalBaseDefense { get; }
        public int TotalMaxDefense { get; }
        public int TotalAugmentedDefense { get; }

        public int TotalFireResistance { get; }
        public int TotalWaterResistance { get; }
        public int TotalThunderResistance { get; }
        public int TotalIceResistance { get; }
        public int TotalDragonResistance { get; }

        public ArmorSetViewModel(ISolverData solverData, IList<IArmorPiece> armorPieces, ICharmLevel charm, IList<ArmorSetJewelViewModel> jewels, int[] spareSlots)
        {
            this.armorPieces = armorPieces;
            this.charm = charm;
            this.jewels = jewels;

            SetSkills(solverData);

            AdditionalSkillsTotalLevel = AdditionalSkills.Sum(x => x.TotalLevel);

            SpareSlots = spareSlots;

            TotalRarity = armorPieces.Sum(x => x.Rarity);

            SpareSlotCount = SpareSlots.Count(x => x > 0);
            SpareSlotSizeSquare = DataUtility.SlotSizeScoreSquare(SpareSlots);
            SpareSlotSizeCube = DataUtility.SlotSizeScoreCube(SpareSlots);

            TotalBaseDefense = armorPieces.Sum(x => x?.Defense.Base ?? 0);
            TotalMaxDefense = armorPieces.Sum(x => x?.Defense.Max ?? 0);
            TotalAugmentedDefense = armorPieces.Sum(x => x?.Defense.Augmented ?? 0);

            TotalFireResistance = armorPieces.Sum(a => a.Resistances.Fire);
            TotalWaterResistance = armorPieces.Sum(a => a.Resistances.Water);
            TotalThunderResistance = armorPieces.Sum(a => a.Resistances.Thunder);
            TotalIceResistance = armorPieces.Sum(a => a.Resistances.Ice);
            TotalDragonResistance = armorPieces.Sum(a => a.Resistances.Dragon);
        }

        private void SetSkills(ISolverData solverData)
        {
            var skills = new Dictionary<int, int>();

            foreach (IArmorPiece armorPiece in armorPieces.Where(x => x != null))
            {
                foreach (IAbility ability in armorPiece.Abilities)
                    IncrementSkillLevel(skills, ability);
            }

            if (charm != null)
            {
                foreach (IAbility ability in charm.Abilities)
                    IncrementSkillLevel(skills, ability);
            }

            foreach (ArmorSetJewelViewModel jewelViewModel in jewels)
            {
                foreach (IAbility ability in jewelViewModel.Jewel.Abilities)
                {
                    for (int i = 0; i < jewelViewModel.Count; i++)
                        IncrementSkillLevel(skills, ability);
                }
            }

            // ------------------------------------

            var additionalSkills = new List<SearchResultSkillViewModel>();

            foreach (KeyValuePair<int, int> skillKeyValue in skills)
            {
                ISkill skill = GlobalData.Instance.Skills.First(s => s.Id == skillKeyValue.Key);
                int totalLevel = skillKeyValue.Value;

                IAbility foundAbility = solverData.DesiredAbilities.FirstOrDefault(a => a.Skill.Id == skill.Id);
                if (foundAbility == null)
                    additionalSkills.Add(new SearchResultSkillViewModel(skill, totalLevel, true));
                else if (totalLevel > foundAbility.Level)
                    additionalSkills.Add(new SearchResultSkillViewModel(skill, totalLevel, false));
            }

            AdditionalSkills = additionalSkills.ToArray();
        }

        private void IncrementSkillLevel(IDictionary<int, int> skills, IAbility ability)
        {
            if (skills.TryGetValue(ability.Skill.Id, out int level))
                skills[ability.Skill.Id] = level + ability.Level;
            else
                skills.Add(ability.Skill.Id, ability.Level);
        }
    }
}
