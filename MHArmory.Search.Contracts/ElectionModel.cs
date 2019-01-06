using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Contracts
{
    internal class ElectionModel
    {
        public ISolverDataEquipmentModel Model { get; }
        public IEquipment Equipment { get; }

        public int SlotCount { get; }
        public int MaxSlot { get; }
        public int SlotSum { get; }

        public Dictionary<int, IAbility> DesiredAbilities { get; }
        public int DesiredSkillCount { get; }
        public int DesiredSkillSum { get; }

        public bool IsSelected
        {
            get { return Model.IsSelected; }
            set { Model.IsSelected = value; }
        }

        public ElectionModel(IEquipment equipment, HashSet<int> desiredSkills, HashSet<int> excludedSkills)
        {
            Equipment = equipment;
            Model = new SolverDataEquipmentModel(equipment);
            SlotCount = equipment.Slots.Count(x => x != 0);
            MaxSlot = equipment.Slots.Length == 0 ? 0 : equipment.Slots.Max();
            SlotSum = equipment.Slots.Sum();

            IsSelected = true;

            DesiredAbilities = new Dictionary<int, IAbility>();
            foreach (IAbility ability in equipment.Abilities)
            {
                if (excludedSkills.Contains(ability.Skill.Id))
                {
                    IsSelected = false;
                }
                if (desiredSkills.Contains(ability.Skill.Id))
                {
                    DesiredSkillCount++;
                    DesiredSkillSum += ability.Level;
                    DesiredAbilities[ability.Skill.Id] = ability;
                }
            }

            if (equipment is IArmorPiece armorPiece)
            {
                if (armorPiece.ArmorSetSkills != null)
                {
                    foreach (IArmorSetSkill armorSetSkill in armorPiece.ArmorSetSkills)
                    {
                        foreach (IArmorSetSkillPart part in armorSetSkill.Parts)
                        {
                            foreach (IAbility ability in part.GrantedSkills)
                            {
                                if (desiredSkills.Contains(ability.Skill.Id))
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
