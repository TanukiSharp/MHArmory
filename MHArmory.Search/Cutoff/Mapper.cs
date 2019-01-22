using System.Collections.Generic;
using System.Linq;
using MHArmory.Core.DataStructures;
using MHArmory.Search.Contracts;

namespace MHArmory.Search.Cutoff
{
    internal class Mapper
    {
        public MappingResults MapEverything(IList<IList<IEquipment>> allEquipment, IAbility[] desiredAbilities, IEnumerable<SolverDataJewelModel> jewels, bool createNullEquipments)
        {
            var results = new MappingResults();
            results.DesiredAbilities = MapDesiredAbilities(desiredAbilities);
            IDictionary<int, int> desiredDict = results.DesiredAbilities.ToDictionary(x => x.SkillId, x => x.MappedId);
            results.SetParts = MapSkillParts(allEquipment, desiredDict, results.DesiredAbilities);
            results.Equipment = MapAllEquipment(allEquipment, desiredDict, results, createNullEquipments);
            results.Jewels = MapJewels(jewels, desiredDict, results.DesiredAbilities);
            return results;
        }

        public MappedEquipment[] MapSupersets(IList<SupersetInfo> supersets, MappingResults existingResults)
        {
            IDictionary<int, int> desiredDict = existingResults.DesiredAbilities.ToDictionary(x => x.SkillId, x => x.MappedId);
            var equipments = supersets.Select(x => x.Equipment).ToList();
            MappedEquipment[] mappedEquipments = MapEquipments(equipments, desiredDict, existingResults, false);
            for (int i = 0; i < mappedEquipments.Length; i++)
            {
                MappedEquipment equipment = mappedEquipments[i];
                int maxSkills = supersets[i].MaxSkills;
                int abilityLevels = equipment.Skills.Sum(x => x.Level);
                equipment.SkillDebt = abilityLevels - maxSkills;
            }
            return mappedEquipments;
        }

        private MappedJewel[] MapJewels(IEnumerable<SolverDataJewelModel> jewels, IDictionary<int, int> desiredDict, MappedSkill[] skills)
        {
            var orderedJewels = jewels.OrderByDescending(x => x.Jewel.SlotSize).ToList();
            IList<MappedJewel> mappedJewels = new List<MappedJewel>();
            foreach (SolverDataJewelModel jewel in orderedJewels)
            {
                IAbility ability = jewel.Jewel.Abilities[0];
                if (!desiredDict.ContainsKey(ability.Skill.Id))
                {
                    continue;
                }
                MappedSkill skill = skills[desiredDict[ability.Skill.Id]].CopyWithLevel(ability.Level);
                var map = new MappedJewel();
                map.Jewel = jewel;
                map.Skill = skill;
                mappedJewels.Add(map);
            }
            return mappedJewels.ToArray();
        }

        private MappedSkill[] MapDesiredAbilities(IAbility[] desiredAbilities)
        {
            var map = new MappedSkill[desiredAbilities.Length];
            for (int i = 0; i < desiredAbilities.Length; i++)
            {
                IAbility ability = desiredAbilities[i];
                var mappedAbility = new MappedSkill
                {
                    MappedId = i,
                    SkillId = ability.Skill.Id,
                    Level = ability.Level
                };
                map[i] = mappedAbility;
            }
            return map;
        }

        private MappedSkillPart[] MapSkillParts(IList<IList<IEquipment>> allEquipment, IDictionary<int, int> desiredDict, MappedSkill[] skills)
        {
            IEnumerable<IArmorSetSkillPart> parts = allEquipment
                .SelectMany(x => x)
                .Where(x => x.Type != EquipmentType.Charm)
                .Cast<IArmorPiece>()
                .Where(x => x.ArmorSetSkills != null)
                .SelectMany(x => x.ArmorSetSkills)
                .SelectMany(x => x.Parts);
            var newParts = new Dictionary<int, MappedSkillPart>();
            foreach (IArmorSetSkillPart part in parts)
            {
                MappedSkill[] desiredPartAbilities = part.GrantedSkills
                    .Where(x => desiredDict.ContainsKey(x.Skill.Id))
                    .Select(x => skills[desiredDict[x.Skill.Id]].CopyWithLevel(x.Level))
                    .ToArray();
                if (desiredPartAbilities.Length == 0)
                {
                    continue;
                }
                var newPart = new MappedSkillPart();
                newPart.Requirement = part.RequiredArmorPieces;
                newPart.Skills = desiredPartAbilities;
                newPart.Part = part;
                newParts[part.Id] = newPart;
            }
            MappedSkillPart[] mappedArr = newParts.Values.ToArray();
            for (int i = 0; i < mappedArr.Length; i++)
            {
                MappedSkillPart part = mappedArr[i];
                part.MappedId = i;
            }
            return mappedArr;
        }

        private MappedEquipment[][] MapAllEquipment(IList<IList<IEquipment>> allEquipment, IDictionary<int, int> desiredDict, MappingResults results, bool createNullEquipments)
        {
            var allMapped = new MappedEquipment[allEquipment.Count][];
            for (int i = 0; i < allEquipment.Count; i++)
            {
                IList<IEquipment> equipments = allEquipment[i];
                allMapped[i] = MapEquipments(equipments, desiredDict, results, createNullEquipments);
            }
            return allMapped;
        }

        private MappedEquipment[] MapEquipments(IList<IEquipment> equipments, IDictionary<int, int> desiredDict, MappingResults results, bool createNullEquipment)
        {
            int startingIndex = 0;
            int count = equipments.Count;
            if (createNullEquipment)
            {
                startingIndex = 1;
                count++;
            }
            var mappedEquipments = new MappedEquipment[count];
            if (createNullEquipment)
            {
                var nullEquipment = new MappedEquipment();
                nullEquipment.Skills = new MappedSkill[0];
                nullEquipment.SkillParts = new MappedSkillPart[0];
                mappedEquipments[0] = nullEquipment;
            }
            for (int j = startingIndex; j < count; j++)
            {
                IEquipment equipment = equipments[j-startingIndex];
                var mappedEquipment = new MappedEquipment();
                mappedEquipments[j] = mappedEquipment;
                mappedEquipment.Equipment = equipment;
                mappedEquipment.Skills = equipment.Abilities
                    .Where(x => desiredDict.ContainsKey(x.Skill.Id))
                    .Select(x => results.DesiredAbilities[desiredDict[x.Skill.Id]].CopyWithLevel(x.Level))
                    .ToArray();
                mappedEquipment.SkillParts = new MappedSkillPart[0];
                if (equipment.Type == EquipmentType.Charm)
                {
                    continue;
                }
                var armor = (IArmorPiece)equipment;
                if (armor.ArmorSetSkills == null)
                {
                    continue;
                }
                var containedPartIDs = new HashSet<int>(armor.ArmorSetSkills.SelectMany(x => x.Parts).Select(x => x.Id));
                mappedEquipment.SkillParts = results.SetParts.Where(x => containedPartIDs.Contains(x.Part.Id)).ToArray();
            }
            return mappedEquipments;
        }
    }
}
