using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Cutoff.Models;

namespace MHArmory.Search.Cutoff.Services
{
    internal class SupersetMaker : ISupersetMaker
    {
        public SupersetInfo CreateSupersetModel(IList<IEquipment> equipments, IAbility[] desiredAbilities)
        {
            EquipmentType type = equipments.FirstOrDefault()?.Type ?? EquipmentType.Charm; // No equipment probably means charm
            var desiredSkillIDs = new HashSet<int>(desiredAbilities.Select(x => x.Skill.Id));
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
            var nameDict = new Dictionary<string, string>()
            {
                { "EN", name }
            };
            IEvent ev = null;
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
                equipment = new ArmorPiece(-1, nameDict, firstArmor.Type, 0, slots, abilities, setSkills, firstArmor.Defense, firstArmor.Resistances, firstArmor.Attributes, firstArmor.Assets, firstArmor.FullArmorSet, ev);
            }
            var info = new SupersetInfo(equipment, maxSkills);
            return info;
        }
    }
}
