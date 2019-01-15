using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Contracts.Incremental
{
    internal class ElectionModel
    {
        public ISolverDataEquipmentModel Model { get; }
        public IEquipment Equipment { get; }

        public bool FullSet { get; }

        public int SlotCount { get; }
        public int MaxSlot { get; }
        public int SlotSum { get; }

        public Dictionary<int, IAbility> DesiredAbilities { get; }
        public Dictionary<int, int> SkillEfficiencies { get; }

        public int DesiredSkillCount { get; }
        public int DesiredSkillSum { get; }

        public IList<ElectionModel> AbsolutelyBetterThan { get; }
        public IList<ElectionModel> SomewhatBetterThan { get; }
        public IList<ElectionModel> SomewhatWorseThan { get; }
        public IList<ElectionModel> AbsolutelyWorseThan { get; }

        public bool IsSelected
        {
            get { return Model.IsSelected; }
            set { Model.IsSelected = value; }
        }

        public ElectionModel(IEquipment equipment, IDictionary<int, IAbility> desiredSkills, HashSet<int> excludedSkills, IDictionary<int, SolverDataJewelModel> jewels)
        {
            AbsolutelyBetterThan = new List<ElectionModel>();
            SomewhatBetterThan = new List<ElectionModel>();
            SomewhatWorseThan = new List<ElectionModel>();
            AbsolutelyWorseThan = new List<ElectionModel>();

            Equipment = equipment;
            Model = new SolverDataEquipmentModel(equipment);
            SlotCount = equipment.Slots.Count(x => x != 0);
            MaxSlot = equipment.Slots.Length == 0 ? 0 : equipment.Slots.Max();
            SlotSum = equipment.Slots.Sum();

            IsSelected = true;

            SkillEfficiencies = new Dictionary<int, int>();
            foreach (IAbility ability in desiredSkills.Values)
            {
                SkillEfficiencies[ability.Skill.Id] = 0;

                bool jewelExists = jewels.TryGetValue(ability.Skill.Id, out SolverDataJewelModel jewel);
                if (!jewelExists)
                {
                    continue;
                }
                int jewelSize = jewel.Jewel.SlotSize;

                foreach (int slot in equipment.Slots)
                {
                    if (slot >= jewelSize)
                    {
                        SkillEfficiencies[ability.Skill.Id] += jewel.Jewel.Abilities[0].Level;
                    }
                }
            }

            DesiredAbilities = new Dictionary<int, IAbility>();
            foreach (IAbility ability in equipment.Abilities)
            {
                if (excludedSkills.Contains(ability.Skill.Id))
                {
                    IsSelected = false;
                }
                if (desiredSkills.ContainsKey(ability.Skill.Id))
                {
                    DesiredSkillCount++;
                    DesiredSkillSum += ability.Level;
                    DesiredAbilities[ability.Skill.Id] = ability;
                    //SkillEfficiencies[ability.Skill.Id] += ability.Level;
                }
            }

            if (equipment is IArmorPiece armorPiece)
            {
                FullSet = armorPiece.FullArmorSet != null;
                if (armorPiece.ArmorSetSkills != null)
                {
                    foreach (IArmorSetSkill armorSetSkill in armorPiece.ArmorSetSkills)
                    {
                        foreach (IArmorSetSkillPart part in armorSetSkill.Parts)
                        {
                            foreach (IAbility ability in part.GrantedSkills)
                            {
                                if (desiredSkills.ContainsKey(ability.Skill.Id))
                                {
                                    DesiredSkillCount++;
                                    DesiredSkillSum += ability.Level;
                                    DesiredAbilities[ability.Skill.Id] = ability;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return Equipment.ToString();
        }
    }
}
