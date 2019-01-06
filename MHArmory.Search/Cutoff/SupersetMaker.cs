using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;

namespace MHArmory.Search.Cutoff
{
    internal class SupersetMaker
    {
        public SupersetInfo CreateSupersetModel(IList<IEquipment> equipments, IAbility[] desiredAbilities)
        {
            var desiredSkillIDs = new HashSet<int>(desiredAbilities.Select(x => x.Skill.Id));
            IEquipment firstEquipment = equipments.First();
            EquipmentType type = firstEquipment.Type;
            int[] slots = new int[CutoffSearchConstants.Slots];
            int maxSkills = 0;
            foreach (IEquipment eq in equipments)
            {
                for (int i = 0; i < eq.Slots.Length; i++)
                {
                    if (slots[i] < eq.Slots[i])
                    {
                        slots[i] = eq.Slots[i];
                    }
                }

                int skillCount = 0;
                foreach (IAbility ability in eq.Abilities)
                {
                    if (!desiredSkillIDs.Contains(ability.Skill.Id))
                    {
                        continue;
                    }
                    skillCount += ability.Level;
                }
                if (skillCount > maxSkills)
                {
                    maxSkills = skillCount;
                }
            }

            IAbility[] abilities = equipments
                .SelectMany(x => x.Abilities)
                .GroupBy(x => x.Skill.Id)
                .Select(x => x.OrderByDescending(y => y.Level).First())
                .ToArray();

            string name = $"{type} superset";
            var nameDict = firstEquipment.Name.ToDictionary(x => x.Key, x => name);
            IEvent ev = firstEquipment.Event;
            IEquipment equipment;
            if (type == EquipmentType.Charm)
            {
                equipment = new CharmLevel(-1, 0, nameDict, 0, slots, abilities, ev);
            }
            else
            {
                var armorPieces = equipments.Cast<IArmorPiece>().ToList();
                IArmorSetSkill[] setSkills = armorPieces
                    .Where(x => x.ArmorSetSkills != null)
                    .SelectMany(x => x.ArmorSetSkills)
                    .Distinct()
                    .ToArray();
                IArmorPiece firstArmor = armorPieces.First();
                equipment = new ArmorPiece(-1, nameDict, type, 0, slots, abilities, setSkills, firstArmor.Defense, firstArmor.Resistances, firstArmor.Attributes, firstArmor.Assets, firstArmor.FullArmorSet, ev);
            }
            var info = new SupersetInfo(equipment, maxSkills);
            return info;
        }
    }
}
